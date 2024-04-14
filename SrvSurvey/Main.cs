using SrvSurvey.game;
using SrvSurvey.net.EDSM;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace SrvSurvey
{
    internal partial class Main : Form
    {
        private Game? game;
        private FileSystemWatcher? logFolderWatcher;
        private FileSystemWatcher? settingsFolderWatcher;
        private FileSystemWatcher? screenshotWatcher;

        private Rectangle lastWindowRect;
        private bool lastWindowHasFocus;
        private List<Control> bioCtrls;
        private Dictionary<string, Screenshot> pendingScreenshots = new Dictionary<string, Screenshot>();
        private bool wasWithinDssDuration;

        public Main()
        {
            InitializeComponent();
            PlotPos.prepPlotterPositions();

            if (Path.Exists(Game.settings.watchedJournalFolder))
            {
                // watch for creation of new log files
                this.logFolderWatcher = new FileSystemWatcher(Game.settings.watchedJournalFolder, "*.log");
                this.logFolderWatcher.Created += logFolderWatcher_Created;
                this.logFolderWatcher.EnableRaisingEvents = true;
                Game.log($"Watching folder: {Game.settings.watchedJournalFolder}");
            }

            if (Path.Exists(Elite.displaySettingsFolder))
            {
                // watch for changes in DisplaySettings.xml 
                this.settingsFolderWatcher = new FileSystemWatcher(Elite.displaySettingsFolder, "DisplaySettings.xml");
                this.settingsFolderWatcher.Changed += settingsFolderWatcher_Changed;
                this.settingsFolderWatcher.EnableRaisingEvents = true;
                Game.log($"Watching file: {Elite.displaySettingsXml}");
            }
            else
            {
                lblNotInstalled.Visible = true;
            }

            if (Game.settings.processScreenshots)
                this.startWatchingScreenshots();

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formMainLocation);

            this.bioCtrls = new List<Control>()
            {
                txtSystemBioSignals,
                txtSystemBioValues,
                txtBodyBioSignals,
                txtBodyBioValues,
            };

            btnPublish.Visible = Debugger.IsAttached;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                this.Text += " (dbg)";

            foreach (Control ctrl in this.Controls) ctrl.Enabled = false;
            btnLogs.Enabled = true;
            btnSettings.Enabled = true;
            btnQuit2.Enabled = true;
            Application.DoEvents();

            if (!Path.Exists(Elite.displaySettingsFolder))
            {
                lblNotInstalled.Enabled = true;
                // stop here if Elite is not installed
                return;
            }

            // if journal folder is not found, warn and push user directly into settings
            if (!Directory.Exists(Game.settings.watchedJournalFolder))
            {
                this.BeginInvoke(() => this.handleJournalFolderNotFound());
                return;
            }

            if (Game.settings.focusGameOnStart)
                Elite.setFocusED();

            this.updateAllControls();

            this.checkFullScreenGraphics();
            this.lastWindowRect = Elite.getWindowRect();

            // check for 1.0.0.0 to 1.1.0.0 migrations
            var isMigrationValid = Program.getMigratableFolders().Any();
            Game.log($"isMigrationValid: {isMigrationValid}, dataFolder1100: {Game.settings.dataFolder1100}");
            if (isMigrationValid)
            {
                if (!Game.settings.dataFolder1100)
                {
                    // we need to migrate all data to the new folder
                    foreach (Control ctrl in this.Controls) ctrl.Enabled = false;
                    btnLogs.Enabled = true;
                    this.Activate();

                    this.BeginInvoke(new Action(() =>
                    {
                        txtCommander.Text = "Data migration needed";
                        txtLocation.Text = "";
                        txtMode.Text = "";
                        Application.DoEvents();
                        var rslt = MessageBox.Show(this, "This new version of SrvSurvey needs to perform a migration of data files. This might take a few minutes if you have visited many systems with SrvSurvey.\r\n\r\nReady to proceed?", "SrvSurvey", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        if (rslt == DialogResult.Cancel)
                        {
                            var rslt2 = MessageBox.Show(this, "The migration is necessary before SrvSurvey can run.\r\n\r\nIf migration keeps failing, please report the issue on:\r\nhttps://github.com/njthomson/SrvSurvey/issues.\r\n\r\nAlternatively you may click 'Retry' to ignore your old data and proceed without it.", "SrvSurvey", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                            if (rslt2 == DialogResult.Retry)
                            {
                                Game.settings.dataFolder1100 = true;
                                Game.settings.Save();
                                Program.forceRestart();
                            }
                            else
                            {
                                Application.Exit();
                                return;
                            }
                        }
                        txtCommander.Text = "Preparing reference data...";
                        txtLocation.Text = "(watch the logs to see progress)";
                        txtMode.Text = "Please stand by...";

                        // Prep CodexRef first
                        Game.codexRef.init(true).ContinueWith((rslt) =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                if (!rslt.IsCompletedSuccessfully)
                                {
                                    Util.handleError(rslt.Exception);
                                    return;
                                }

                                // migrate the data files
                                txtCommander.Text = "Migrating data...";
                                Application.DoEvents();
                            }));

                            // keep this on background thread
                            Program.migrateToNewDataFolder();

                            this.Invoke(new Action(() =>
                            {
                                // migrate the data files
                                txtCommander.Text = "Reticulating data...";
                                Application.DoEvents();
                            }));

                            // keep this on background thread
                            Program.migrate_BodyData_Into_SystemData().ContinueWith((result) =>
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (result.IsFaulted && result.Exception != null)
                                    {
                                        FormErrorSubmit.Show(result.Exception.InnerException ?? result.Exception);
                                        Application.Exit();
                                        return;
                                    }

                                    // show thank you message
                                    txtCommander.Text = "Ready!";
                                    txtLocation.Text = "";
                                    Application.DoEvents();
                                    MessageBox.Show(this, "Thank you, your data has been migrated.\r\n\r\nSrvSurvey will now restart...", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // save that migration completed
                                    Application.DoEvents();
                                    var newSettings = Settings.Load();
                                    newSettings.dataFolder1100 = true;
                                    newSettings.Save();

                                    // force a restart
                                    Application.DoEvents();
                                    Application.DoEvents();
                                    Process.Start(Application.ExecutablePath);
                                    Application.DoEvents();
                                    Process.GetCurrentProcess().Kill();
                                }));
                            });
                        });
                    }));
                    return;
                }
            }
            else if (!Game.settings.dataFolder1100)
            {
                Game.settings.dataFolder1100 = true;
                Game.settings.Save();
            }

            // update pub data and other files
            txtCommander.Text = "Preparing reference data...";
            txtLocation.Text = "This is a (mostly) one time thing";
            this.txtMode.Text = "";


            Task.Factory.StartNew(new Action(async () =>
            {
                try
                {
                    SiteTemplate.Import();
                    HumanSiteTemplate.import();
                    await Game.git.refreshPublishedData();

                    Game.canonn.init();

                    await Game.codexRef.init(false);

                    this.Invoke(new Action(() =>
                    {
                        btnSettings.Enabled = true;
                        btnQuit2.Enabled = true;

                        this.updateCommanderTexts();
                        Application.DoEvents();

                        if (Elite.isGameRunning)
                            this.newGame();

                        foreach (Control ctrl in this.Controls) ctrl.Enabled = true;
                        btnSphereLimit.Enabled = false;

                        this.timer1.Interval = 200;
                        this.timer1.Start();
                    }));
                }
                catch (Exception ex)
                {
                    Util.handleError(ex);
                }
            }));
        }

        private void handleJournalFolderNotFound()
        {
            Game.log($"Journal watch folder not found: {Game.settings.watchedJournalFolder}");
            txtCommander.Text = "Journal folder not found!";
            txtLocation.Text = "Settings update required";
            txtMode.Text = "";

            txtCommander.Enabled
                = txtLocation.Enabled
                = btnSettings.Enabled
                = btnQuit2.Enabled
                = true;

            MessageBox.Show(
                this,
                $"Cannot find the folder for journal files:\r\n\r\n{Game.settings.watchedJournalFolder}\r\n\r\nPlease update 'watch journal folder' in settings...",
                "SrvSurvey",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation
            );
            new FormSettings().ShowDialog(this);

        }

        private void updateAllControls(GameMode? newMode = null)
        {
            //Game.log("** ** ** updateAllControls ** ** **");

            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateTrackTargetTexts();
            this.updateGuardianTexts();
            this.updateSphereLimit();

            var gameIsActive = game != null && Elite.isGameRunning && game.Commander != null;

            if (gameIsActive && Game.settings.autoShowPlotFSS && (newMode == GameMode.FSS || game?.mode == GameMode.FSS))
                Program.showPlotter<PlotFSS>();
            else
                Program.closePlotter<PlotFSS>();

            if (gameIsActive && PlotSysStatus.allowPlotter)
                Program.showPlotter<PlotSysStatus>();

            if (gameIsActive && PlotBioSystem.allowPlotter)
                Program.showPlotter<PlotBioSystem>();

            if (gameIsActive && PlotHumanSite.allowPlotter)
                Program.showPlotter<PlotHumanSite>();

            // show high gravity warning
            var isHighGravity = (game?.systemBody?.surfaceGravity ?? 0) >= Game.settings.highGravityWarningLevel * 10;
            if (Game.settings.autoShowFlightWarnings && game?.systemBody != null && isHighGravity && game.isMode(GameMode.Landed, GameMode.SuperCruising, GameMode.GlideMode, GameMode.Flying, GameMode.InFighter, GameMode.InSrv))
                Program.showPlotter<PlotFlightWarning>();
            else
                Program.closePlotter<PlotFlightWarning>();

            // show Guardian status
            if (Game.settings.enableGuardianSites && Game.settings.autoShowGuardianSummary && PlotGuardianSystem.allowPlotter && game?.systemData?.settlements.Count > 0)
                Program.showPlotter<PlotGuardianSystem>();
            else
                Program.closePlotter<PlotGuardianSystem>();

            if (Game.settings.autoShowPlotGalMapTest && PlotGalMap.allowPlotter)
                Program.showPlotter<PlotGalMap>();
            else
                Program.closePlotter<PlotGalMap>();

            // Why was this necessary? (Which plotter is getting missed now?)
            //Program.invalidateActivePlotters();
        }

        private void settingsFolderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;

            Program.crashGuard(() =>
            {
                Application.DoEvents();
                this.checkFullScreenGraphics();
            }, true);
        }

        private void logFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            Program.crashGuard(() =>
            {

                Game.log($"New journal file detected: {e.Name} (existing game: {this.game != null})");
                if (this.game == null)
                {
                    Application.DoEvents();
                    this.newGame();
                }

            }, true);
        }

        private void removeGame()
        {
            Game.log($"Main.removeGame, has old game: {this.game != null}");
            Program.closeAllPlotters();

            if (this.game != null)
            {
                Game.update -= Game_modeChanged;
                if (this.game.journals != null)
                    this.game.journals.onJournalEntry -= Journals_onJournalEntry;
                this.game.Dispose();
            }
            this.game = null;

            this.updateAllControls();
        }

        private void newGame()
        {
            Game.log($"Main.newGame: has old game:{this.game != null} ");

            if (this.game != null)
            {
                this.removeGame();
                Application.DoEvents();
            }

            var newGame = new Game(Game.settings.preferredCommander);
            if (newGame.isShutdown || !Elite.isGameRunning)
            {
                newGame.Dispose();
                return;
            }

            this.game = newGame;

            Game.update += Game_modeChanged;
            this.game.journals!.onJournalEntry += Journals_onJournalEntry;

            if (!Game.settings.hideJournalWriteTimer)
                Program.showPlotter<PlotPulse>();

            this.updateAllControls();

            Game.log($"migratedScannedOrganicsInEntryId: {newGame.cmdr?.migratedScannedOrganicsInEntryId}, migratedNonSystemDataOrganics: {newGame.cmdr?.migratedNonSystemDataOrganics}");
            if (newGame?.cmdr != null && (!newGame.cmdr.migratedScannedOrganicsInEntryId || !newGame.cmdr.migratedNonSystemDataOrganics))
            {
                Task.Run(new Action(() =>
                {
                    if (!newGame.cmdr.migratedScannedOrganicsInEntryId)
                        BodyData.migrate_ScannedOrganics_Into_ScannedBioEntryIds(newGame.cmdr);

                    if (!newGame.cmdr.migratedNonSystemDataOrganics)
                        SystemData.migrate_BodyData_Into_SystemData(newGame.cmdr).ConfigureAwait(false);

                    Game.log($"Cmdr migrations complete!");
                }));
            }
        }

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            if (force || this.txtMode.Text != newMode.ToString())
            {
                this.updateAllControls(newMode);
            }
        }

        private void updateCommanderTexts()
        {
            var gameIsActive = game != null && Elite.isGameRunning && game.Commander != null;

            if (!gameIsActive || game == null)
            {
                if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
                    this.txtCommander.Text = Game.settings.preferredCommander + " (only)";
                else
                    this.txtCommander.Text = Game.settings.lastCommander + " ?";

                this.txtVehicle.Text = "";
                this.txtLocation.Text = "";
                this.txtNearBody.Text = "";

                if (game?.mode == GameMode.MainMenu)
                    this.txtMode.Text = "MainMenu";
                if (Elite.isGameRunning)
                    this.txtMode.Text = "Wrong commander or game not active";
                else
                    this.txtMode.Text = "Game is not active";

                return;
            }

            var gameMode = game.cmdr.isOdyssey ? "live" : "legacy";
            this.txtCommander.Text = $"{game.Commander} (FID:{game.fid}, mode:{gameMode})";
            this.txtMode.Text = game.mode.ToString();
            if (game.mode == GameMode.Docked && game.humanSite != null)
                this.txtMode.Text += ": " + game.humanSite.name;

            if (game.atMainMenu)
            {
                this.txtLocation.Text = "";
                this.txtVehicle.Text = "";
                this.txtNearBody.Text = "";
                return;
            }

            this.txtVehicle.Text = game.vehicle.ToString();

            this.txtLocation.Text = game.systemBody?.name ?? $"{game.systemData?.name}" ?? "";

            if (game.fsdJumping)
            {
                this.txtNearBody.Text = "Witch space";
            }
            else if (game.systemBody != null)
            {
                this.txtNearBody.Text = game.systemBody.type.ToString();// "Near body";
            }
            else
            {
                this.txtNearBody.Text = "Deep space";
            }
        }

        private void updateBioTexts()
        {
            if (game?.cmdr != null)
            {
                txtBioRewards.Text = Util.credits(game.cmdr.organicRewards, true)
                    + $", organisms: {game.cmdr.scannedBioEntryIds.Count}";
            }

            if (game == null || game.atMainMenu || !Elite.isGameRunning || !game.initialized || game.systemData == null)
            {
                foreach (var ctrl in this.bioCtrls) ctrl.Text = "-";
                lblSysBio.Enabled = txtSystemBioSignals.Enabled = txtSystemBioValues.Enabled = labelSignalsAndRewards.Enabled = false;
                lblBodyBio.Enabled = txtBodyBioSignals.Enabled = txtBodyBioValues.Enabled = checkFirstFootFall.Enabled = false;

                Program.closePlotter<PlotBioStatus>();
                Program.closePlotter<PlotGrounded>();
                Program.closePlotter<PlotTrackers>();
                Program.closePlotter<PlotPriorScans>();
                return;
            }

            // update system bio numbers
            var systemTotal = game.systemData.bodies.Sum(_ => _.bioSignalCount);
            var systemScanned = game.systemData.bodies.Sum(_ => _.countAnalyzedBioSignals);
            txtSystemBioSignals.Text = $"{systemScanned} of {systemTotal}";

            var sysEstimate = game.systemData.bodies.Sum(_ => _.sumPotentialEstimate);
            var sysActual = game.systemData.bodies.Sum(_ => _.sumAnalyzed);
            txtSystemBioValues.Text = $"{Util.credits(sysActual, true)} of {Util.credits(sysEstimate, true)}";
            if (game.systemData.bodies.Any(_ => _.bioSignalCount > 0 && _.organisms?.All(o => o.species != null) != true))
                txtSystemBioValues.Text += "?";

            var countFirstFootFall = game.systemData.bodies.Count(_ => _.firstFootFall && _.bioSignalCount > 0);
            if (countFirstFootFall > 0)
                txtSystemBioValues.Text += $" (FF: {countFirstFootFall})";

            // update First Footfall checkbox
            checkFirstFootFall.AutoEllipsis = false;
            checkFirstFootFall.Enabled = game?.systemBody != null;
            // checkFirstFootFall.Text = $"First footfall: {game?.systemBody?.name}";
            if (game?.systemBody == null)
                checkFirstFootFall.CheckState = CheckState.Unchecked;
            else if (checkFirstFootFall.Checked != game.systemBody.firstFootFall)
                checkFirstFootFall.Checked = game.systemBody.firstFootFall;

            checkFirstFootFall.AutoEllipsis = true;

            // enable/disable controls according to
            lblSysBio.Enabled = txtSystemBioSignals.Enabled = txtSystemBioValues.Enabled = labelSignalsAndRewards.Enabled = systemTotal > 0;
            lblBodyBio.Enabled = txtBodyBioSignals.Enabled = txtBodyBioValues.Enabled = game?.systemBody?.bioSignalCount > 0;

            if (game?.systemBody == null)
            {
                txtBodyBioSignals.Text = txtBodyBioValues.Text = "-";

                //Program.closePlotter<PlotBioStatus>();
                //Program.closePlotter<PlotGrounded>();
                //Program.closePlotter<PlotTrackers>();
                //Program.closePlotter<PlotPriorScans>();
            }
            else if (game.systemBody?.bioSignalCount == 0)
            {
                foreach (var ctrl in this.bioCtrls) ctrl.Text = "-";

                //Program.closePlotter<PlotBioStatus>();
                //Program.closePlotter<PlotGrounded>();
                //Program.closePlotter<PlotPriorScans>();
            }
            else
            {
                txtBodyBioSignals.Text = $"{game.systemBody!.countAnalyzedBioSignals} of {game.systemBody!.bioSignalCount}";
                txtBodyBioValues.Text = $"{Util.credits(game.systemBody.sumAnalyzed, true)} of {Util.credits(game.systemBody.sumPotentialEstimate, true)}";
                if (game.systemBody.organisms?.All(o => o.species != null) != true)
                    txtBodyBioValues.Text += "?";

                if (game.systemBody.firstFootFall) txtBodyBioValues.Text += " (FF)";

                if (game.systemSite == null && game.systemBody?.organisms != null)
                {
                    if (Game.settings.autoShowBioSummary && (game.showBodyPlotters || game.mode == GameMode.SAA) && !game.showGuardianPlotters && !Program.isPlotter<PlotGuardians>())
                        Program.showPlotter<PlotBioStatus>();
                }

                // show prior scan data only if present
                var showPlotPriorScans = Game.settings.useExternalData && Game.settings.autoLoadPriorScans && (game.showBodyPlotters || game.mode == GameMode.SAA) && game.canonnPoiHasLocalBioSignals();
                if (game.systemSite == null && showPlotPriorScans && !game.showGuardianPlotters && !Program.isPlotter<PlotGuardians>())
                    Program.showPlotter<PlotPriorScans>();

                if (game.showBodyPlotters && Game.settings.autoShowBioPlot && !this.game.showGuardianPlotters)
                {
                    // show trackers only if we have some
                    var showPlotTrackers = game.systemBody?.bookmarks?.Count > 0;
                    if (game.systemSite == null && showPlotTrackers)
                        Program.showPlotter<PlotTrackers>();

                    // show radar if we have trackers, prior scans, we landed or started scanning already
                    if (game.systemSite == null && !game.isMode(GameMode.SuperCruising, GameMode.GlideMode) && (game.isLanded || showPlotTrackers || showPlotPriorScans || game.cmdr.scanOne != null))
                        Program.showPlotter<PlotGrounded>();
                    else
                        Program.closePlotter<PlotGrounded>();
                }
            }
        }

        private void updateTrackTargetTexts()
        {
            if (game == null || game.atMainMenu || !Elite.isGameRunning || !game.initialized)
            {
                txtTargetLatLong.Text = "";
                lblTrackTargetStatus.Text = "-";
                Program.closePlotter<PlotTrackTarget>();
            }
            else if (!Game.settings.targetLatLongActive)
            {
                txtTargetLatLong.Text = "<none>";
                lblTrackTargetStatus.Text = "Inactive";
                Program.closePlotter<PlotTrackTarget>();
            }
            else if (game.showBodyPlotters && (game.status!.Flags & StatusFlags.HasLatLong) > 0 && game.systemBody != null)
            {
                txtTargetLatLong.Text = Game.settings.targetLatLong.ToString();
                lblTrackTargetStatus.Text = "Active";
                Program.showPlotter<PlotTrackTarget>();
            }
            else
            {
                txtTargetLatLong.Text = Game.settings.targetLatLong.ToString();
                lblTrackTargetStatus.Text = "Ready";
                Program.closePlotter<PlotTrackTarget>();
            }
        }

        private void updateGuardianTexts()
        {
            if (!groupBox4.Visible || game == null || game.atMainMenu || !Elite.isGameRunning || !game.initialized)
            {
                lblGuardianCount.Text = "";
                txtGuardianSite.Text = "";
                Program.closePlotter<PlotGuardians>();
                Program.closePlotter<PlotGuardianStatus>();
                Program.closePlotter<PlotRamTah>();

                btnRuinsMap.Enabled = false;
                btnRuinsOrigin.Enabled = false;
            }
            else if (game.systemSite == null || game.systemBody == null || game.systemBody.settlements?.Count == 0)
            {
                lblGuardianCount.Text = game?.systemBody?.settlements?.Count.ToString() ?? "0";
                txtGuardianSite.Text = "";
                Program.closePlotter<PlotGuardians>();
                Program.closePlotter<PlotGuardianStatus>();
                Program.closePlotter<PlotRamTah>();
                btnRuinsMap.Enabled = false;
                btnRuinsOrigin.Enabled = false;
            }
            else if (Game.settings.enableGuardianSites)
            {
                lblGuardianCount.Text = game.systemBody.settlements?.Count.ToString();
                if (this.game.systemSite != null)
                {
                    if (this.game.systemSite.isRuins)
                        txtGuardianSite.Text = $"Ruins #{this.game.systemSite.index} - {this.game.systemSite.type}, {this.game.systemSite.siteHeading}°";
                    else
                        txtGuardianSite.Text = $"{this.game.systemSite.type}, {this.game.systemSite.siteHeading}°";

                    if (game.showBodyPlotters && this.game.showGuardianPlotters)
                    {
                        Program.showPlotter<PlotGuardians>();
                        Program.showPlotter<PlotGuardianStatus>();
                        if (Game.settings.autoShowRamTah && (this.game.systemSite.isRuins && game.cmdr.decodeTheRuinsMissionActive == TahMissionStatus.Active || !this.game.systemSite.isRuins && game.cmdr.decodeTheLogsMissionActive == TahMissionStatus.Active))
                            Program.showPlotter<PlotRamTah>();

                        Program.closePlotter<PlotGrounded>();
                        Program.closePlotter<PlotBioStatus>();
                        Program.closePlotter<PlotPriorScans>();
                        Program.closePlotter<PlotTrackers>();
                    }

                    btnRuinsMap.Enabled = game.systemSite.siteHeading != -1 && this.game.showGuardianPlotters;
                    btnRuinsOrigin.Enabled = game.systemSite.siteHeading != -1 && this.game.showGuardianPlotters;
                }
            }
        }

        private void updateSphereLimit()
        {
            btnSphereLimit.Enabled = game?.cmdr != null;

            // show/hide the sphere limit plotter
            if (PlotSphericalSearch.allowPlotter)
                Program.showPlotter<PlotSphericalSearch>();
            else
                Program.closePlotter<PlotSphericalSearch>();
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(Shutdown entry)
        {
            Game.log($"Main.Shutdown: Commander:{this.game?.Commander}");
            this.removeGame();
        }

        private void onJournalEntry(Disembark entry)
        {
            Game.log($"Main.Disembark {entry.Body}");

            if (entry.OnPlanet && !entry.OnStation && game?.systemBody?.bioSignalCount > 0 && !game.showGuardianPlotters && !Program.isPlotter<PlotGuardians>())
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
                if (Game.settings.autoShowBioPlot && !this.game.showGuardianPlotters)
                    Program.showPlotter<PlotGrounded>();
            }

            // close these upon disembarking
            if (!PlotSysStatus.allowPlotter) Program.closePlotter<PlotSysStatus>();
            if (!PlotBioSystem.allowPlotter) Program.closePlotter<PlotBioSystem>();
        }

        private void onJournalEntry(LaunchSRV entry)
        {
            Game.log($"Main.LaunchSRV {game?.status?.BodyName}");

            if (game!.showBodyPlotters && game.systemBody?.bioSignalCount > 0 && !game.showGuardianPlotters && !Program.isPlotter<PlotGuardians>())
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
                if (Game.settings.autoShowBioPlot && !this.game.showGuardianPlotters)
                    Program.showPlotter<PlotGrounded>();
            }
        }

        private void onJournalEntry(ApproachBody entry)
        {
            Game.log($"Main.ApproachBody {entry.Body}");

            if (game!.showBodyPlotters && game.systemBody?.bioSignalCount > 0)
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
            }

            if (Game.settings.targetLatLongActive && game.showBodyPlotters)
            {
                Program.showPlotter<PlotTrackTarget>();
            }
        }

        private void onJournalEntry(SAASignalsFound entry)
        {
            Application.DoEvents();
            this.updateAllControls();
        }

        private void onJournalEntry(FSSBodySignals entry)
        {
            Application.DoEvents();
            this.updateAllControls();
        }

        private void onJournalEntry(ScanOrganic entry)
        {
            if (entry.ScanType == ScanType.Analyse)
            {
                Application.DoEvents();
                this.updateAllControls();
            }
        }

        private void onJournalEntry(CodexEntry entry)
        {
            if (entry.Name == "$Codex_Ent_Guardian_Beacons_Name;")
            {
                // Scanned a Guardian Beacon
                Game.log($"Scanned Guardian Beacon in: {entry.System}");
                Program.showPlotter<PlotGuardianBeaconStatus>();
            }
        }

        private void onJournalEntry(SupercruiseDestinationDrop entry)
        {
            if (entry.Type == "Guardian Beacon")
            {
                // Arrived  Guardian Beacon
                Game.log($"Arrived at Guardian Beacon in: {game?.cmdr.currentSystem}");
                Program.showPlotter<PlotGuardianBeaconStatus>();
            }
        }

        private void onJournalEntry(DataScanned entry)
        {
            if (entry.Type == "$Datascan_AncientPylon;")
            {
                // A Guardian Beacon
                Game.log($"Scanned data from Guardian Beacon in: {game?.cmdr.currentSystem}");
                Program.showPlotter<PlotGuardianBeaconStatus>();
            }
        }

        private void onJournalEntry(SupercruiseEntry entry)
        {
            Game.log($"Main.SupercruiseEntry {entry.Starsystem}");

            // close these plotters upon super-cruise
            Program.closePlotter<PlotGrounded>();
            Program.closePlotter<PlotGuardians>();
            Program.closePlotter<PlotGuardianStatus>();
            Program.closePlotter<PlotRamTah>();
        }

        private void onJournalEntry(StartJump entry)
        {
            if (entry.JumpType == "Hyperspace")
            {
                // close everything when entering hyperspace (except PlotPulse)
                Program.closeAllPlotters(true);
            }
        }

        private void onJournalEntry(FSDJump entry)
        {
            // Trigger forms to update as we jump systems
            var systemMatch = new net.EDSM.StarSystem()
            {
                name = entry.StarSystem,
                id64 = entry.SystemAddress,
                coords = new SystemCoords()
                {
                    x = entry.StarPos[0],
                    y = entry.StarPos[1],
                    z = entry.StarPos[2],
                },
            };

            if (FormAllRuins.activeForm != null)
            {
                FormAllRuins.activeForm.comboCurrentSystem.Text = systemMatch.name;
                FormAllRuins.activeForm.StarSystemLookup_starSystemMatch(systemMatch);
            }
            if (FormBeacons.activeForm != null)
            {
                FormBeacons.activeForm.comboCurrentSystem.Text = systemMatch.name;
                FormBeacons.activeForm.StarSystemLookup_starSystemMatch(systemMatch);
            }
        }

        private void onJournalEntry(SendText entry)
        {
            if (game == null) return;
            var msg = entry.Message.ToLowerInvariant().Trim();

            switch (msg)
            {
                // target tracking commands
                case MsgCmd.targetHere:
                    Game.settings.targetLatLong = Status.here.clone();
                    Game.settings.targetLatLongActive = true;
                    Game.settings.Save();
                    this.updateTrackTargetTexts();
                    return;
                case MsgCmd.targetOff:
                    Game.settings.targetLatLongActive = false;
                    Game.settings.Save();
                    this.updateTrackTargetTexts();
                    return;
                case MsgCmd.targetOn:
                    Game.settings.targetLatLongActive = true;
                    Game.settings.Save();
                    this.updateTrackTargetTexts();
                    return;

                case MsgCmd.imgs:
                    var folder = Path.Combine(Game.settings.screenshotTargetFolder!, game.cmdr.currentSystem);
                    if (Directory.Exists(folder))
                        Util.openLink(folder);
                    return;

                case MsgCmd.kill:
                    Application.Exit();
                    return;
            }

            if (game.systemData != null && game.systemBody != null && (msg.StartsWith(MsgCmd.trackAdd) || msg.StartsWith(MsgCmd.trackRemove) || msg.StartsWith(MsgCmd.trackRemoveLast)))
            {
                // book marking
                if (msg == MsgCmd.trackRemoveAll)
                {
                    game.clearAllBookmarks();
                }
                else if (msg.StartsWith(MsgCmd.trackAdd))
                {
                    var name = msg.Substring(1).Trim();
                    game.addBookmark(name, Status.here.clone());
                }
                else if (msg.StartsWith(MsgCmd.trackRemoveName))
                {
                    var name = msg.Substring(2).Trim();
                    game.removeBookmarkName(name);
                }
                else if (msg.StartsWith(MsgCmd.trackRemove))
                {
                    var name = msg.Substring(1).Trim();
                    game.removeBookmark(name, Status.here.clone(), true);
                }
                else if (msg.StartsWith(MsgCmd.trackRemoveLast))
                {
                    var name = msg.Substring(1).Trim();
                    game.removeBookmark(name, Status.here.clone(), false);
                }

                // force a re-render
                if (game.showBodyPlotters)
                    Program.showPlotter<PlotTrackers>()?.prepTrackers();
            }

            // submit a Landscape survey
            if (msg.StartsWith(MsgCmd.visited, StringComparison.OrdinalIgnoreCase))
            {
                var bodyName = entry.Message.Substring(MsgCmd.visited.Length).Trim();
                game.systemStatus.visitedTargetBody(bodyName);
            }
            else if (msg.StartsWith(MsgCmd.submit, StringComparison.OrdinalIgnoreCase))
            {
                var notes = entry.Message.Substring(MsgCmd.submit.Length).Trim();
                game.systemStatus.submitSurvey(notes);
            }
            else if (msg.Equals(MsgCmd.nextSystem, StringComparison.OrdinalIgnoreCase))
            {
                game.systemStatus.nextSystem().ConfigureAwait(false);
            }

            // first foot fall
            else if (game.systemData != null && (msg.StartsWith(MsgCmd.firstFoot, StringComparison.OrdinalIgnoreCase) || msg.StartsWith(MsgCmd.ff, StringComparison.OrdinalIgnoreCase)))
            {
                var parts = msg.Split(' ', 2)!;
                game.toggleFirstFootfall(parts.Length == 2 ? parts[1] : null);
            }

            if (game.humanSite != null)
            {
                var siteLocation = game.humanSite.location;
                var siteHeading = (decimal)game.humanSite.heading;

                // temp?
                if (msg == "!!")
                {
                    // set site origin as target
                    Game.settings.targetLatLongActive = true;
                    Game.settings.targetLatLong = siteLocation;
                    this.updateTrackTargetTexts();
                }
                else if (msg == "..")
                {
                    // measure dist/angle from site origin
                    var radius = game.status.PlanetRadius;
                    var dist = Util.getDistance(siteLocation, Status.here, radius).ToString("N2");
                    var angle = Util.getBearing(siteLocation, Status.here) - siteHeading;
                    if (angle < 0) angle += 360;
                    var angleTxt = angle.ToString("N2");
                    // "dist": 67.2, "angle": 36.5
                    var txt = $"\"dist\": {dist}, \"angle\": {angleTxt}";
                    Game.log($"Relative to site origin:\r\n\r\n\t{txt}\r\n\r\n\tSite: {siteLocation} / {siteHeading}°\r\n\tcmdr: {Status.here} / {game.status.Heading}°\r\n");
                    Clipboard.SetText(txt);
                }
            }
        }

        private void checkFirstFootFall_CheckedChanged(object sender, EventArgs e)
        {
            // abuse AutoEllipsis to stop stack overflowing
            if (checkFirstFootFall.AutoEllipsis && game?.systemBody != null)
            {
                game.toggleFirstFootfall(null);
                Elite.setFocusED();
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch
            {
                // swallow
                Application.Exit();
            }
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            ViewLogs.show(Game.logs);
        }

        private void setTargetLatLong()
        {
            // update our UX
            this.updateTrackTargetTexts();

            // show plotter if near a body
            if (game?.systemBody != null && game.showBodyPlotters)
            {
                var form = Program.showPlotter<PlotTrackTarget>();
                form.targetLocation = Game.settings.targetLatLong;
                form.calculate(Game.settings.targetLatLong); // TODO: retire
            }
        }

        private void btnGroundTarget_Click(object sender, EventArgs e)
        {
            var form = new FormGroundTarget();
            form.ShowDialog(this);

            if (Game.settings.targetLatLongActive)
            {
                Game.log($"Main.Set target lat/long: {Game.settings.targetLatLong}, near: {game?.cmdr?.lastSystemLocation}");
                setTargetLatLong();
            }
            else
            {
                Program.closePlotter<PlotTrackTarget>();
                this.updateTrackTargetTexts();
            }
            Elite.setFocusED();
        }

        private void btnPasteLatLong_Click(object sender, EventArgs e)
        {
            var newLocation = FormGroundTarget.pasteFromClipboard();
            if (newLocation != null)
            {
                Game.settings.targetLatLong = newLocation;
                Game.settings.targetLatLongActive = true;
                Game.log($"Main.Set target from clipboard lat/long: {Game.settings.targetLatLong}, near: {game?.cmdr?.lastSystemLocation}");
                setTargetLatLong();
                this.updateTrackTargetTexts();
            }
            Elite.setFocusED();
        }

        private void btnClearTarget_Click(object sender, EventArgs e)
        {
            Game.settings.targetLatLongActive = false;
            Game.settings.Save();

            this.updateTrackTargetTexts();
            Elite.setFocusED();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // slow timer to check the location of the game window, repositioning plotters if needed
            var rect = Elite.getWindowRect();
            var hasFocus = rect != Rectangle.Empty && rect.X != -32000;

            if (this.lastWindowHasFocus != hasFocus)
            {
                //Game.log($"EliteDangerous window focus changed: {hasFocus}");

                if (hasFocus)
                    Program.showActivePlotters();
                else
                    Program.hideActivePlotters();

                this.lastWindowHasFocus = hasFocus;
            }
            else if (rect != this.lastWindowRect && hasFocus)
            {
                Game.log($"EliteDangerous window reposition: {this.lastWindowRect} => {rect}");
                this.lastWindowRect = rect;
                Program.repositionPlotters(rect);
            }

            // if the game process is NOT running, but we have an active game object processing journals ... append a fake shutdown entry and stop processing journal entries
            if (!Elite.isGameRunning && game != null && !game.isShutdown && game.journals != null)
            {
                Game.log($"EliteDangerous process died?!");
                game.journals.fakeShutdown();
                this.removeGame();
            }

            // check/clear the systemBody reference we're holding onto for some duration after a DSS scan completes
            if (game != null)
            {
                var isWithinDssDuration = SystemData.isWithinLastDssDuration();
                if (isWithinDssDuration)
                {
                    this.wasWithinDssDuration = true;
                }
                else if (this.wasWithinDssDuration && !isWithinDssDuration)
                {
                    game.fireUpdate(true);
                    this.wasWithinDssDuration = false;
                }
            }
        }

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            if (Game.settings.formMainLocation != this.Location)
            {
                Game.settings.formMainLocation = this.Location;
                Game.settings.Save();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            new FormSettings().ShowDialog(this);

            // force update form controls
            this.updateAllControls();

            // force opacity changes to take immediate effect
            Program.showActivePlotters();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://discord.gg/GJjTFa9fsz");
        }

        private void Main_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && Game.settings.focusGameOnMinimize)
            {
                Game.log($"Main.Minimized, focusing on game...");
                Elite.setFocusED();
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Game.log("Saving settings on close...");
            this.game?.cmdr?.Save();
            Game.settings.Save();
            Game.log("Closed without errors");
        }

        private void checkFullScreenGraphics()
        {
            // 0: Windows / 1: FullScreen / 2: Borderless
            lblFullScreen.Visible = Elite.getGraphicsMode() == GraphicsMode.FullScreen;
            lblFullScreen.Enabled = true;
        }

        private void lblNotInstalled_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://www.elitedangerous.com");
        }

        private void btnRuinsMap_Click(object sender, EventArgs e)
        {
            PlotGuardians.switchMode(Mode.map);
        }

        private void btnRuinsOrigin_Click(object sender, EventArgs e)
        {
            PlotGuardians.switchMode(Mode.origin);
        }

        #region screenshot manipulations

        private void stopWatchingScreenshots()
        {
            if (this.screenshotWatcher != null)
            {
                Game.log($"Stop watching screenshot folder: {this.screenshotWatcher.Path}");
                this.screenshotWatcher.EnableRaisingEvents = false;
                this.screenshotWatcher.Created -= ScreenshotWatcher_Created;
                this.screenshotWatcher.Dispose();
                this.screenshotWatcher = null;
            }
        }

        private void startWatchingScreenshots()
        {
            this.stopWatchingScreenshots();

            // watch for creation of new log files
            var folder = Game.settings.screenshotSourceFolder;
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                // assume a hard-coded folder if the setting is bad
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments\\Elite Dangerous");
            }
            this.screenshotWatcher = new FileSystemWatcher(folder, "*.bmp");
            this.screenshotWatcher.Created += ScreenshotWatcher_Created;
            this.screenshotWatcher.EnableRaisingEvents = true;
            Game.log($"Start watching screenshot folder: {folder}");
        }

        public void onJournalEntry(Screenshot entry)
        {
            if (!Game.settings.processScreenshots || string.IsNullOrEmpty(Game.settings.screenshotTargetFolder))
                return;

            // prepare source image filename
            var imgSourceFolder = Game.settings.screenshotSourceFolder ?? Elite.defaultScreenshotFolder;
            var imageFilename = entry.Filename.Replace("\\ED_Pictures", imgSourceFolder);

            // process the image if it already exists, otherwise add to the queue
            if (File.Exists(imageFilename))
            {
                Task.Run(() =>
                {
                    // do this in a background thread
                    this.processScreenshot(entry, imageFilename);
                });
            }
            else
            {
                Game.log($"Waiting for: {imageFilename}");
                this.pendingScreenshots.Add(imageFilename, entry);
            }
        }

        private void ScreenshotWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (this.IsDisposed) return;
            Program.crashGuard(() =>
            {
                var matchFound = this.pendingScreenshots.ContainsKey(e.FullPath);
                Game.log($"Screenshot created: {e.FullPath}, matchFound: {matchFound}");

                if (matchFound)
                    this.processScreenshot(this.pendingScreenshots[e.FullPath], e.FullPath);
            });
        }

        private void processScreenshot(Screenshot entry, string imageFilename)
        {
            if (!Game.settings.processScreenshots || this.game == null)
                return;

            if (!File.Exists(imageFilename))
            {
                Game.log($"Cannot find expected picture: {imageFilename}");
                return;
            }

            if (string.IsNullOrEmpty(entry.System))
                entry.System = "unknown";
            if (string.IsNullOrEmpty(entry.Body))
                entry.Body = "unknown";

            /**
             * File names are formatted as one of:
             * 
             * {body name} {UTC time}.png
             * {body name} {UTC time} (HighRes).png
             * {body name} Ruins{1} {UTC time}.png
             * {body name} Ruins{1} {Alpha} {UTC time}.png
             */
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HHmmss");
            var filename = $"{entry.Body} ({timestamp})";

            var extraTxt = "";
            var isAerialScreenshot = false;
            var siteType = GuardianSiteData.SiteType.Unknown;

            if (game!.systemSite != null)
            {
                var siteData = game!.systemSite;

                var majorType = siteData.isRuins ? $"Ruins{siteData.index} " : "";
                filename += $", {majorType}{siteData.type}";

                extraTxt = $"\r\n  {siteData.nameLocalised}";
                siteType = siteData.type;
                if (siteType != GuardianSiteData.SiteType.Unknown)
                {
                    extraTxt += $" - {siteData.type}";

                    // measure distance from site origin (from the entry lat/long if possible)
                    var td = new TrackingDelta(game.systemBody!.radius, siteData.location);
                    if (!string.IsNullOrEmpty(entry.Latitude) && !string.IsNullOrEmpty(entry.Longitude))
                        td.Current = new LatLong2(double.Parse(entry.Latitude), double.Parse(entry.Longitude));

                    // if we are within 50m of the origin, and altitude is between 500m and 2000 - this qualifies as an aerial screenshot
                    if (td.distance < 50 && game.status.Altitude > 500 && game.status.Altitude < 2000)
                        isAerialScreenshot = true;
                }
            }

            // maybe HighRes? and file type
            if (entry.Width > Screen.PrimaryScreen!.WorkingArea.Width) filename += " (HighRes)";
            filename += ".png";


            // load the image - quickly, so as not to lock the file
            Image sourceImage;
            using (var img = Bitmap.FromFile(imageFilename))
                sourceImage = new Bitmap(img);

            // bucket all screenshots into 1 folder per system
            var folder = Path.Combine(Game.settings.screenshotTargetFolder!, entry.System);

            // save final image
            var saveImage = new Bitmap(sourceImage);

            // optionally - add image banner, only if image was created in the past 10 seconds
            if (Game.settings.addBannerToScreenshots)
            {
                var latitude = entry.Latitude ?? game.status.Latitude.ToString();
                var longitude = entry.Longitude ?? game.status.Longitude.ToString();
                var heading = entry.Heading ?? game.status.Heading.ToString();

                var duration = DateTime.Now - entry.timestamp;
                if (duration.TotalSeconds < 10 && game.status.hasLatLong)
                {
                    extraTxt += $"\r\n  Lat: {latitude}° Long: {longitude}°\r\n  Heading: {heading}°:";

                    if (!string.IsNullOrEmpty(entry.Altitude))
                        extraTxt += $"  Altitude: {(int)float.Parse(entry.Altitude)}m";
                }

                this.addBannerToScreenshot(entry, saveImage, extraTxt);
            }

            Game.log($"Writing screenshot '{filename}' in: {folder}");
            Directory.CreateDirectory(folder);
            saveImage.Save(Path.Combine(folder, filename), ImageFormat.Png);

            // also save the image in Ruins specific folders, if we are aligned with the site origin
            if (isAerialScreenshot && Game.settings.useGuardianAerialScreenshotsFolder)
            {
                // if it's an alpha site - truncate and rotate
                if (siteType == GuardianSiteData.SiteType.Alpha && Game.settings.rotateAndTruncateAlphaAerialScreenshots)
                {
                    var truncated = new Bitmap((int)(sourceImage.Height * 1.3f), sourceImage.Height);
                    using (var g = Graphics.FromImage(truncated))
                    {
                        g.Clear(Color.Red);
                        var x = (sourceImage.Width / 2) - (truncated.Width / 2);
                        g.DrawImage(sourceImage, -x, 0);
                    }
                    sourceImage = truncated;
                    sourceImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                this.addBannerToScreenshot(entry, sourceImage, extraTxt);

                folder = Path.Combine(Game.settings.screenshotTargetFolder!, $"Aerial {siteType}");
                Game.log($"Writing screenshot '{filename}' in: {folder}");
                Directory.CreateDirectory(folder);
                sourceImage.Save(Path.Combine(folder, filename), ImageFormat.Png);
            }

            // finally, remove the original file?
            if (Game.settings.deleteScreenshotOriginal)
            {
                Game.log($"Removing original file: {imageFilename}");
                File.Delete(imageFilename);
            }
        }

        private void addBannerToScreenshot(Screenshot entry, Image img, string extra)
        {
            using (var g = Graphics.FromImage(img))
            {
                // main details
                var txtBig = $"Body: {entry.Body}";
                var timestamp = Game.settings.screenshotBannerLocalTime
                    ? entry.timestamp.ToLocalTime().ToString()
                    : new DateTimeOffset(entry.timestamp).ToString("u");
                var txt = $"System: {entry.System}\r\nCmdr: {game!.Commander}  -  " + timestamp + extra;

                /* fake details - for creating sample image in settings
                var txtBig = $"Body: <Nearest body name>";
                var txt = $"System: <Current system name>\r\nCmdr: <Commander name>  -  <time stamp>"
                    + "\r\n  Ancient Ruins <N> - <site type>\r\n  Lat: +123.456789° Long: -12.345678°\r\n  Heading: 123°  Altitute: 123m";
                // */

                var fontBig = GameColors.fontScreenshotBannerBig;
                var fontSmall = GameColors.fontScreenshotBannerSmall;

                var szBig = g.MeasureString(txtBig, fontBig);
                var szSmall = g.MeasureString(txt, fontSmall);

                var y = 10f;
                var h = szBig.Height + szSmall.Height + 10;

                g.FillRectangle(
                    Brushes.Black,
                    10f, y,
                    Math.Max(szBig.Width, szSmall.Width) + 10,
                    h
                );
                var bb = new SolidBrush(Game.settings.screenshotBannerColor);
                g.DrawString(txtBig, fontBig, bb, 15, y + 5);
                g.DrawString(txt, fontSmall, bb, 15, y + 5 + szBig.Height);
            }
        }

        #endregion

        private void btnGuardianThings_Click(object sender, EventArgs e)
        {
            FormBeacons.show();
        }

        private void btnRuins_Click(object sender, EventArgs e)
        {
            FormRuins.show();

            //game!.systemData!.prepSettlements();
            //Program.invalidateActivePlotters();

            //if (game?.systemData?.bodies != null)
            //    Game.log(string.Join("\r\n", game.systemData.bodies.Select(_ => $"'{_.name}' ({_.id}) : {Util.credits((long)_.rewardEstimate)}")));
        }

        private void btnSphereLimit_Click(object sender, EventArgs e)
        {
            Program.closePlotter<PlotSphericalSearch>();

            var form = new FormSphereLimit();
            form.ShowDialog(this);

            this.updateSphereLimit();
        }

        private void btnRamTah_Click(object sender, EventArgs e)
        {
            //FormRamTah.show();
            Program.showPlotter<PlotHumanSite>();
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            btnPublish.Enabled = false;
            SiteTemplate.publish();
            Game.canonn.init(true);

            Game.git.publishLocalData(); // 1st: for updating publish data from local surveys
            Game.canonn.readXmlSheetRuins2(); // 2nd: for updating allRuins.json and reading from Excel data
            Game.canonn.readXmlSheetRuins3() // 3rd: for updating allStructures.json and reading from Excel data
                .ContinueWith(task =>
                {
                    Game.log("\r\n****\r\n**** Publishing all complete\r\n****");
                    this.Invoke(() => { btnPublish.Enabled = true; });
                });
        }

        private void checkTempHide_CheckedChanged(object sender, EventArgs e)
        {
            Program.tempHideAllPlotters = !Program.tempHideAllPlotters;
            checkTempHide.Checked = Program.tempHideAllPlotters;

            if (Program.tempHideAllPlotters)
                Program.hideActivePlotters();
            else
                Program.showActivePlotters();
        }
    }
}

