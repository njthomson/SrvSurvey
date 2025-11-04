using BioCriterias;
using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.net;
using SrvSurvey.plotters;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;

namespace SrvSurvey
{
    [Draggable, TrackPosition]
    internal partial class Main : FixedForm
    {
        private Game? game;
        private FileSystemWatcher? logFolderWatcher;
        private FileSystemWatcher? settingsFolderWatcher;
        private FileSystemWatcher? screenshotWatcher;

        private Rectangle lastWindowRect;
        private List<Control> bioCtrls;
        private Dictionary<string, Screenshot> pendingScreenshots = new Dictionary<string, Screenshot>();
        private bool wasWithinDssDuration;
        private bool gameHadFocus;
        private FormMultiFloatie? multiFloatie => FormMultiFloatie.current;
        private DateTime lastProcCheck;

        public static Main form;
        public static bool isClosing;
        public KeyboardHook hook;

        public Main()
        {
            Game.log("Starting main form...");
            form = this;
            InitializeComponent();
            btnSearch.Font = GameColors.Fonts.segoeEmoji_16_ns;
            btnGuardian.Font = GameColors.Fonts.segoeEmoji_16_ns;
            btnTravel.Font = GameColors.Fonts.segoeEmoji_16_ns;
            btnColonize.Font = GameColors.Fonts.segoeEmoji_16_ns;
            if (Debugger.IsAttached) this.Text += " (dbg)";
            lblNotInstalled.BringToFront();
            lblFullScreen.BringToFront();
            lblFullScreen.ForeColor = Color.White;
            lblNotInstalled.ForeColor = Color.White;
            comboDev.Items.AddRange(comboDevItems);
            comboDev.SelectedIndex = 0;
            PlotBase2.invalidate(); // do this first, so we hit the static constructor before prepPlotterPositions
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
            this.applySavedLocation();

            this.bioCtrls = new List<Control>()
            {
                txtSystemBioSignals,
                txtSystemBioValues,
                txtBodyBioSignals,
                txtBodyBioValues,
            };

            comboDev.Visible = Debugger.IsAttached;


            // disable colonization menu items if feature is disabled
            if (!Game.settings.buildProjects_TEST) menuColonize.targetButton = null;

            Util.applyTheme(this);
            btnNextWindow.BackColor = C.cyanDark;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Main.isClosing = true;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.setChildrenEnabled(false, btnLogs, btnSettings, btnQuit2);
            // toggling hide/show guarantee's we show in the OS taskbar
            this.Hide();
            this.Show();
            Application.DoEvents();

            Program.defer(() =>
            {
                this.MinimumSize = this.Size;
            });

            if (!Path.Exists(Elite.displaySettingsFolder))
            {
                Game.log("Elite Dangerous does not appear to be installed?");
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

            Program.defer(() => afterMainLoad());
        }

        private void doMigrationFromData1000()
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
                        Program.forceRestart();
                    }));
                });
            });
        }

        private void afterMainLoad()
        {
            this.updateAllControls();

            this.checkFullScreenGraphics();
            this.lastWindowRect = Elite.getWindowRect();

            // check for 1.0.0.0 to 1.1.0.0 migrations
            var isMigrationNeeded = Program.getMigratableFolders().Count > 0;
            Game.log($"isMigrationNeeded: {isMigrationNeeded}, dataFolder1100: {Game.settings.dataFolder1100}");
            if (isMigrationNeeded)
            {
                if (!Game.settings.dataFolder1100)
                {
                    // we need to migrate all data to the new folder
                    this.setChildrenEnabled(false, btnLogs);
                    this.Activate();

                    Program.defer(() => this.doMigrationFromData1000());
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

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    GuardianSiteTemplate.Import();
                    HumanSiteTemplate.import(Debugger.IsAttached);
                    await Game.git.refreshPublishedData().ContinueWith(_ =>
                    {
                        // show update available link as needed
                        if (_.Result)
                            Program.defer(() => linkNewBuildAvailable.Visible = true);
                    });

                    Game.canonn.init();

                    await Game.codexRef.init(false);

                    this.Invoke(new Action(() =>
                    {
                        btnSettings.Enabled = true;
                        btnQuit2.Enabled = true;

                        this.updateCommanderTexts();
                        Application.DoEvents();

                        // do this once first so we know the window location
                        timer1_Tick(null!, null!);

                        this.startHooks();

                        if (Elite.isGameRunning || Program.useLastIfShutdown)
                            this.newGame();

                        this.setChildrenEnabled(true);
                        btnCodexShow.Enabled = FormShowCodex.allow;
                        this.updateCommanderTexts();

                        this.timer1.Start();

                        if (Game.settings.focusGameOnStart)
                            this.BeginInvoke(() => Elite.setFocusED());
                    }));
                }
                catch (Exception ex)
                {
                    Util.handleError(ex);
                }
            });
        }

        public void startHooks()
        {
            if (Game.settings.keyActions_TEST != null && this.hook == null && (Game.settings.keyhook_TEST || Game.settings.hookDirectX_TEST))
                this.hook = new KeyboardHook();
        }

        public void stopHooks()
        {
            if (this.hook != null)
            {
                this.hook.Dispose();
                this.hook = null!;
            }
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            // a periodic timer to check the location of the game window, repositioning plotters if needed
            var rect = Elite.getWindowRect();

            // hide overlays if game is minimized or hide-all is checked
            if (rect == Rectangle.Empty || Program.tempHideAllPlotters)
            {
                //Game.log($"EliteDangerous window minimized: {this.lastWindowRect} => {rect}");
                Program.hideActivePlotters();
                BigOverlay.current?.Hide();
            }
            else if (rect != this.lastWindowRect)
            {
                // reposition plotters if game window has moved
                Game.log($"EliteDangerous window reposition: {this.lastWindowRect} => {rect}");
                if (Game.settings.disableLargeOverlay)
                {
                    PlotBase2.renderAll(game);
                    foreach (var plotter in PlotBase2.active) plotter.setPosition(rect);
                }
                else
                    BigOverlay.current?.reposition(rect);
                Program.repositionPlotters(rect);
                multiFloatie?.positionOverGame(rect);
            }


            // show/hide multi-floatie if we have focus
            if (multiFloatie != null && !multiFloatie.IsDisposed) multiFloatie.Visible = Elite.focusElite || Elite.focusSrvSurvey;

            this.lastWindowRect = rect;
            this.gameHadFocus = Elite.focusElite;

            // every ~5 seconds - force a process check
            if ((DateTime.Now - lastProcCheck).TotalSeconds > 5)
            {
                try
                {
                    lastProcCheck = DateTime.Now;
                    var procs = Elite.GetGameProcs();
                    Elite.hadManyGameProcs = procs.Length > 1;

                    // handle single/multiple processes
                    btnNextWindow.Visible = Elite.hadManyGameProcs;
                    if (!Game.settings.hideMultiFloatie)
                    {
                        if (this.multiFloatie == null && Elite.hadManyGameProcs) FormMultiFloatie.create();
                        if (this.multiFloatie != null && !Elite.hadManyGameProcs && !multiFloatie.IsDisposed) multiFloatie.Close();
                    }
                }
                catch (Exception ex)
                {
                    // report but ignore any errors here
                    Game.log($"timer1_Tick: GetProcessesByName failed:\r\n\t{ex.Message}\r\n{ex.StackTrace}");
                }
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

            // poke the current journal file, combatting batched file writes by the game
            if (game?.journals != null) game.journals.poke();
        }

        private void removeGame()
        {
            Game.log($"Main.removeGame, has old game: {this.game != null} (cmdr: {this.game?.Commander})");
            Program.closeAllPlotters();
            BigOverlay.close();
            PlotBase2.closeAll();
            if (VR.enabled) VR.shutdown();

            this.hook?.stopDirectX();

            if (this.game != null)
            {
                Game.update -= Game_modeChanged;
                if (this.game.journals != null)
                    this.game.journals.onJournalEntry -= Journals_onJournalEntry;
                this.game.Dispose();
                this.game = null;
            }


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
            if (game.cmdr?.isOdyssey == false) this.Text = $"{this.Text} LEGACY MODE";

            Game.update += Game_modeChanged;
            this.game.journals!.onJournalEntry += Journals_onJournalEntry;

            this.updateAllControls();
            this.hook?.startDirectX(Game.settings.hookDirectXDeviceId_TEST);

            Game.log($"migratedScannedOrganicsInEntryId: {newGame.cmdr?.migratedScannedOrganicsInEntryId}, migratedNonSystemDataOrganics: {newGame.cmdr?.migratedNonSystemDataOrganics}");
            if (newGame?.cmdr != null && (!newGame.cmdr.migratedScannedOrganicsInEntryId || !newGame.cmdr.migratedNonSystemDataOrganics))
            {
                Task.Run(new Action(() =>
                {
                    if (!newGame.cmdr.migratedScannedOrganicsInEntryId)
                        BodyDataOld.migrate_ScannedOrganics_Into_ScannedBioEntryIds(newGame.cmdr);

                    if (!newGame.cmdr.migratedNonSystemDataOrganics)
                        SystemData.migrate_BodyData_Into_SystemData(newGame.cmdr).ConfigureAwait(false);

                    Game.log($"Cmdr migrations complete!");
                }));
            }

            PlotBase.startWindowOne();

            // reset bigOverlay
            BigOverlay.init(game);
            if (VR.enabled) VR.init();
        }

        private void updateAllControls(GameMode? newMode = null)
        {
            //Game.log("** ** ** updateAllControls ** ** **");
            //Game.log($"systemBody? {game?.systemBody != null} / {game?.mode} / {PlotBodyInfo.allowPlotter}");

            // if the game got shutdown
            if (game != null && Elite.isGameRunning && game.isShutdown == true)
            {
                this.removeGame();
                return;
            }

            this.updateColonizationMenuItems();
            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateGuardianTexts(newMode);

            groupCodex.Invalidate();

            // enable button only if this system has some bio signals
            btnPredictions.Enabled = Game.activeGame?.systemData?.bioSignalsTotal > 0;

            // ShowCodex button and form
            this.btnCodexShow.Enabled = FormShowCodex.allow;
            if (!FormShowCodex.allow) BaseForm.close<FormShowCodex>();

            if (newMode == GameMode.MainMenu)
            {
                Program.closeAllPlotters();
                PlotBase2.closeAll();
                return;
            }
        }

        private void groupCodex_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var txt = game?.cmdrCodex?.completionProgress > 0
                ? $"{Math.Floor(game.cmdrCodex.completionProgress * 100)}%" // truncate not round
                : "?";
            var r = new Rectangle(scaleBy(4), scaleBy(12), groupCodex.Width, scaleBy(40));

            TextRenderer.DrawText(e.Graphics, txt, lblBig.Font, r, Color.Black);
        }

        private void settingsFolderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;

            Program.crashGuard(true, () =>
            {
                Application.DoEvents();
                this.checkFullScreenGraphics();
            });
        }

        private void logFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            Program.crashGuard(true, () =>
            {
                Game.log($"New journal file detected: {e.Name} (existing game: {this.game != null})");
                if (this.game == null)
                {
                    Application.DoEvents();
                    this.newGame();
                }

            });
        }

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            if (force || this.txtMode.Text != newMode.ToString())
            {
                this.updateAllControls(newMode);
            }
        }

        private void btnCopyLocation_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLocation.Text))
                Clipboard.SetText(txtLocation.Text);
        }

        private void btnResetExploration_Click(object sender, EventArgs e)
        {
            if (game == null) return;

            var rslt = MessageBox.Show(this, "Are you sure you want to reset estimated exploration values and statistics?", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (rslt == DialogResult.Yes)
            {
                game.cmdr.explRewards = 0;
                game.cmdr.countJumps = 0;
                game.cmdr.distanceTravelled = 0;
                game.cmdr.countScans = 0;
                game.cmdr.countDSS = 0;
                game.cmdr.countLanded = 0;
                game.cmdr.Save();
                game.fireUpdate(true);
            }
        }

        private void updateCommanderTexts()
        {
            var gameIsActive = game != null && Elite.isGameRunning && game.Commander != null;

            if (!gameIsActive || game == null)
            {
                if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
                    this.txtCommander.Text = Game.settings.preferredCommander + " (only)";
                else if (Program.forceFid != null)
                    this.txtCommander.Text = $"{Program.forceFid} ? (forced)";
                else
                    this.txtCommander.Text = Game.settings.lastCommander + " ?";

                this.notifyIcon.Text = $"SrvSurvey: {Program.forceFid} ?\nDouble click to show";
                this.menuNotifyCmdr.Text = $"Cmdr: {Program.forceFid} ?";
                this.txtVehicle.Text = "";
                this.txtNearBody.Text = "";
                var cmdr = CommanderSettings.LoadCurrentOrLast();
                this.txtLocation.Text = Game.settings.lastFid != null ? cmdr?.currentSystem ?? "" : "";
                btnCopyLocation.Enabled = !string.IsNullOrEmpty(this.txtLocation.Text);
                menuNotifyCopy.Visible = false;

                if (game?.mode == GameMode.MainMenu)
                    this.txtMode.Text = "MainMenu";
                if (Elite.isGameRunning)
                    this.txtMode.Text = "Wrong commander or game not active";
                else
                    this.txtMode.Text = "Game is not active";

                btnRamTah.Enabled = cmdr != null;
                btnSearch.Enabled = cmdr != null;
                btnTravel.Enabled = cmdr != null;
                btnColonize.Enabled = cmdr != null;
                btnResetExploration.Enabled = false;
                return;
            }

            this.txtCommander.Text = game.Commander;
            if (Program.forceFid != null) this.txtCommander.Text += " (forced)";
            this.notifyIcon.Text = $"SrvSurvey: {game.Commander}\nDouble click to show";
            this.menuNotifyCmdr.Text = $"Cmdr: {game.Commander}";
            if (this.multiFloatie != null && game.Commander != null && multiFloatie.cmdr != game.Commander) multiFloatie.setCmdr(game.Commander);

            this.txtMode.Text = game.mode.ToString();
            if (game.mode == GameMode.Docked && game.systemStation != null)
                this.txtMode.Text += ": " + game.systemStation.name;

            if (game.atMainMenu)
            {
                this.txtLocation.Text = "";
                this.txtVehicle.Text = "";
                this.txtNearBody.Text = "";
                return;
            }

            if (game.vehicle == ActiveVehicle.MainShip)
                this.txtVehicle.Text = Util.pascal(game.currentShip.name ?? game.currentShip.type);
            else
                this.txtVehicle.Text = game.vehicle.ToString();

            this.txtLocation.Text = game.systemBody?.name ?? $"{game.systemData?.name}" ?? "";
            btnCopyLocation.Enabled = !string.IsNullOrEmpty(this.txtLocation.Text);
            menuNotifyCopy.Visible = btnCopyLocation.Enabled;
            menuNotifyCopy.Text = "Copy: " + txtLocation.Text;

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

            // exploration stats
            if (game == null)
            {
                menuSetLatLong.Enabled = false;
                txtExplorationValue.Text = "";
                txtJumps.Text = "";
                txtDistance.Text = "";
                txtBodies.Text = "";
            }
            else
            {
                menuSetLatLong.Enabled = true;
                txtExplorationValue.Text = Util.credits(game.cmdr.explRewards, true);
                txtJumps.Text = game.cmdr.countJumps.ToString("N0");
                txtDistance.Text = game.cmdr.distanceTravelled.ToString("N1") + " ly";
                txtBodies.Text = $"Scanned: {game.cmdr.countScans}, DSS: {game.cmdr.countDSS}, Landed: {game.cmdr.countLanded}";
            }

            btnRamTah.Enabled = true;
            btnSearch.Enabled = true;
            btnTravel.Enabled = true;
            btnColonize.Enabled = true;
            btnResetExploration.Enabled = true;
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
                return;
            }

            // update system bio numbers
            var bodies = game.systemData.bodies.ToList();
            var systemTotal = bodies.Sum(_ => _.bioSignalCount);
            var systemScanned = bodies.Sum(_ => _.countAnalyzedBioSignals);
            txtSystemBioSignals.Text = $"{systemScanned} of {systemTotal}";

            var sysEstimate = bodies.Sum(body => body.firstFootFall ? body.maxBioRewards * 5 : body.maxBioRewards);
            var sysActual = bodies.Sum(body => body.sumAnalyzed);
            txtSystemBioValues.Text = $" {Util.credits(sysActual, true)} of {Util.credits(sysEstimate, true)}";
            if (game.systemData.bodies.Any(_ => _.bioSignalCount > 0 && _.organisms?.All(o => o.species != null) != true))
                txtSystemBioValues.Text += "?";

            var countFirstFootFall = bodies.Count(_ => _.firstFootFall && _.bioSignalCount > 0);
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
            btnPredictions.Enabled = FormPredictions.allow;

            if (game?.systemBody == null)
            {
                txtBodyBioSignals.Text = txtBodyBioValues.Text = "-";
            }
            else if (game.systemBody?.bioSignalCount == 0)
            {
                foreach (var ctrl in this.bioCtrls) ctrl.Text = "-";
            }
            else
            {
                txtBodyBioSignals.Text = $"{game.systemBody!.countAnalyzedBioSignals} of {game.systemBody!.bioSignalCount}";
                txtBodyBioValues.Text = $" {Util.credits(game.systemBody.sumAnalyzed, true)} of {Util.credits(game.systemBody.firstFootFall ? game.systemBody.maxBioRewards * 5 : game.systemBody.maxBioRewards, true)}";
                if (game.systemBody.organisms?.All(o => o.species != null) != true)
                    txtBodyBioValues.Text += "?";

                if (game.systemBody.firstFootFall) txtBodyBioValues.Text += " (FF)";

                /* safe to remove?
                if (PlotGrounded.allowPlotter)
                {
                    // show trackers only if we have some
                    var showPlotTrackers = game.systemBody?.bookmarks?.Count > 0;
                    if (game.systemSite == null && showPlotTrackers)
                        Program.showPlotter<PlotTrackers>();

                    // show radar if we have trackers, prior scans, we landed or started scanning already
                    if (game.systemSite == null && !game.isMode(GameMode.SuperCruising, GameMode.GlideMode) && (game.isLanded || showPlotTrackers || game.cmdr.scanOne != null || (game.status.Altitude < 1000 && game.systemBody?.bioScans?.Count > 0)))
                        Program.showPlotter<PlotGrounded>();
                    else
                        Program.closePlotter<PlotGrounded>();
                }
                */
            }
        }

        private void updateGuardianTexts(GameMode? newMode = null)
        {
            if (game == null || game.atMainMenu || !Elite.isGameRunning || !game.initialized)
            {
                //lblGuardianCount.Text = "";
                //txtGuardianSite.Text = "";

                btnRuinsMap.Enabled = false;
                btnRuinsOrigin.Enabled = false;
            }
            else if (game.systemSite == null || game.systemBody == null || game.systemBody.settlements?.Count == 0)
            {
                //lblGuardianCount.Text = game?.systemBody?.settlements?.Count.ToString() ?? "0";
                //txtGuardianSite.Text = "";
                btnRuinsMap.Enabled = false;
                btnRuinsOrigin.Enabled = false;
            }
            else if (Game.settings.enableGuardianSites)
            {
                if (this.game.systemSite != null)
                {
                    var allowed = PlotGuardians.allowed(game);
                    btnRuinsMap.Enabled = game.systemSite.siteHeading != -1 && allowed;
                    btnRuinsOrigin.Enabled = game.systemSite.siteHeading != -1 && allowed;
                }
            }
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

        private void onJournalEntry(SAASignalsFound entry)
        {
            // TODO: Remove with PlotBase
            Application.DoEvents();
            this.updateAllControls();
        }

        private void onJournalEntry(FSSBodySignals entry)
        {
            // TODO: Remove with PlotBase
            Application.DoEvents();
            this.updateAllControls();
        }

        private void onJournalEntry(ScanOrganic entry)
        {
            if (entry.ScanType == ScanType.Analyse)
                Program.defer(() => this.updateAllControls());
        }

        private void onJournalEntry(SupercruiseEntry entry)
        {
            Game.log($"Main.SupercruiseEntry {entry.Starsystem}");

            FormRuins.activeForm?.showFilteredSites();
        }

        private void onJournalEntry(StartJump entry)
        {
            if (entry.JumpType == "Hyperspace")
            {
                // close everything when entering hyperspace (except PlotJumpInfo)
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
                coords = entry.StarPos,
            };

            var formBeacons = BaseForm.get<FormBeacons>();
            if (formBeacons != null)
            {
                formBeacons.comboCurrentSystem.Text = systemMatch.name;
                formBeacons.StarSystemLookup_starSystemMatch(systemMatch);
            }

            FormPredictions.refresh();

            if (Game.settings.focusGameAfterFsdJump)
            {
                Game.log($"Setting focus to game after arriving in: {entry.StarSystem} ({entry.SystemAddress})");
                Elite.setFocusED();
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
                    PlotBase2.addOrRemove(game, PlotTrackTarget.def);
                    return;
                case MsgCmd.targetOff:
                    Game.settings.targetLatLongActive = false;
                    Game.settings.Save();
                    PlotBase2.addOrRemove(game, PlotTrackTarget.def);
                    return;
                case MsgCmd.targetOn:
                    Game.settings.targetLatLongActive = true;
                    Game.settings.Save();
                    PlotBase2.addOrRemove(game, PlotTrackTarget.def);
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
            }

            // first foot fall
            else if (game.systemData != null && (msg.StartsWith(MsgCmd.firstFoot, StringComparison.OrdinalIgnoreCase) || msg.StartsWith(MsgCmd.ff, StringComparison.OrdinalIgnoreCase)))
            {
                var parts = msg.Split(' ', 2)!;
                game.toggleFirstFootfall(parts.Length == 2 ? parts[1] : null);
            }


            if (msg == "@")
            {
                // set current location as tracking target
                Game.settings.targetLatLongActive = true;
                Game.settings.targetLatLong = Status.here.clone(); // current location
                Game.settings.Save();

                this.setTargetLatLong();
                Game.log("Set current location as target");
            }
            else if (msg == "@@")
            {
                // helper for ship cockpit offsets (from target lat/long)
                var po = Util.getOffset(game.status.PlanetRadius, Game.settings.targetLatLong, game.status.Heading);
                Game.log($"cockpit offset: {{ \"{game.currentShip.type}\", new PointM({po.x}, {po.y}) }}");
                CanonnStation.setShipOffset(game.currentShip.type, po);
                Clipboard.SetText($"{{ \"{game.currentShip.type}\", new PointM({po.x}, {po.y}) }}, ");
            }

            if (game.systemStation != null)
            {
                var siteLocation = game.systemStation.location;
                var siteHeading = (decimal)game.systemStation.heading;

                // temp?

                if (msg == "!")
                {
                    // set site origin as tracking target
                    Game.settings.targetLatLongActive = true;
                    Game.settings.targetLatLong = siteLocation;
                    Game.settings.Save();

                    this.setTargetLatLong();
                    Game.log("Set site origin location as target");
                }
                else if (msg == "!!")
                {
                    // get delta from tracking target
                    var radius = game.status.PlanetRadius;
                    var pf = Util.getOffset(radius, Game.settings.targetLatLong, game.systemStation.heading);

                    var txt = $"\"offset\": {{ \"X\": {(float)pf.X}, \"Y\": {(float)pf.Y} }}";
                    var dh = game.status.Heading - game.systemStation.heading;
                    if (dh < 0) dh += 360;
                    if (dh != 0) txt += $", \"rot\": {dh}";

                    Game.log(txt);
                    Clipboard.SetText(txt);
                }
                else if (msg == "..")
                {
                    // measure dist/angle from site origin
                    var radius = game.status.PlanetRadius;
                    var pf = Util.getOffset(radius, game.systemStation.location, game.systemStation.heading);

                    var txt = $"{{ \"X\": {pf.X}, \"Y\": {pf.Y} }}";
                    Game.log($"Relative to site origin:\r\n\r\n\t{txt}\r\n");
                    Clipboard.SetText(txt);
                }
                else if (msg == "//")
                {
                    // measure dist/angle from site origin
                    var radius = game.status.PlanetRadius;
                    var cmdrOffset = Util.getOffset(radius, game.systemStation.location, Status.here, game.systemStation.heading);

                    var dist = Util.getDistance(siteLocation, Status.here, radius);
                    var angle = Util.getBearing(siteLocation, Status.here) - siteHeading;
                    if (angle < 0) angle += 360;
                    var pf = Util.rotateLine(180 - angle, dist);

                    var txt = $"\"offset\": {{ \"X\": {pf.X}, \"Y\": {pf.Y} }}";
                    var txt2 = $"\"offset\": {{ \"X\": {cmdrOffset.X}, \"Y\": {cmdrOffset.Y} }}";
                    Game.log($"Relative to site origin:\r\n\r\n\t{txt}\r\nvs  {txt2}");
                }

            }

            if (msg == MsgCmd.settlement && Game.settings.autoShowHumanSitesTest)
            {
                Game.log($"Try infer site from heading: {game.status.Heading}");
                // TODO: still needed or unhelpful?
                // game.initHumanSite();
                if (game.systemData != null && game.systemStation != null && PlotHumanSite.allowed(game))
                {
                    var changed = false;
                    if (game.status.OnFootExterior)
                        changed = game.systemStation.inferFromFoot(game.status.Heading, game.status.Latitude, game.status.Longitude, (double)game.status.PlanetRadius);
                    else if (game.status.Docked)
                        changed = game.systemStation.inferFromShip(game.status.Heading, game.status.Latitude, game.status.Longitude, game.currentShip.type, (double)game.status.PlanetRadius, CalcMethod.AutoDock);
                    else
                        return;

                    if (changed)
                        game.systemData.Save();

                    PlotBase2.add(game, PlotHumanSite.def);
                    game.cmdr.setMarketId(game.systemStation.marketId, $"inferSiteFromHeading: {game.systemStation}");
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

        private void setTargetLatLong()
        {
            // show plotter if near a body
            var form = PlotBase2.addOrRemove<PlotTrackTarget>(game);
            if (form != null)
            {
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

            PlotBase2.addOrRemove(game, PlotTrackTarget.def);
            Elite.setFocusED();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            try
            {
                btnSettings.Enabled = false;
                if (VR.enabled) VR.shutdown();

                var form = new FormSettings();
                var rslt = form.ShowDialog(this);

                if (rslt == DialogResult.OK)
                {
                    // force update form controls
                    this.updateAllControls();

                    // force opacity changes to take immediate effect
                    Program.showActivePlotters();
                    Util.applyTheme(this);

                    Application.DoEvents();
                    if (game != null)
                    {
                        if (VR.enabled) VR.init();
                        BigOverlay.init(game);
                    }
                }
                else
                {
                    // restore things that might have changed from settings
                    PlotBase2.getPlotter<PlotHumanSite>()?.setSize(Game.settings.plotHumanSiteWidth, Game.settings.plotHumanSiteHeight);
                    if (BigOverlay.current != null)
                        BigOverlay.current.setOpacity(Game.settings.plotterOpacity);
                }
            }
            finally
            {
                btnSettings.Enabled = true;
            }
        }

        private void btnNextWindow_Click(object sender, EventArgs e)
        {
            // ... show a form asking which commander to launch or switch windows
            var fid = Program.forceFid ?? CommanderSettings.currentOrLastFid;
            if (fid != null)
            {
                new FormStartNewCmdr(fid).ShowDialog(this);
            }
            else
            {
                Game.log($"btnNextWindow_Click: no FID?");
            }
        }

        private void menuNotifyShowMain_Click(object sender, EventArgs e)
        {
            if (!this.Visible)
                this.Show();

            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Activate();
        }

        private void linkNewBuildAvailable_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // open MS App store link or GitHub Releases
            if (Program.isAppStoreBuild)
                Util.openLink("https://www.microsoft.com/store/productId/9NGT6RRH6B7N");
            else
            {
                var rslt = Git.nextBuild == null || Program.isLinux
                    ? DialogResult.No
                    : MessageBox.Show($"A new build of SrvSurvey is ready on GitHub.\n\nPress YES to auto install: {Git.nextBuild}\n\nPress NO to view releases on GitHub or CANCEL to do nothing.", $"SrvSurvey - {Program.releaseVersion}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (rslt == DialogResult.Yes && Git.nextBuild != null)
                {
                    this.Text = "Updating ...";
                    this.setChildrenEnabled(false, btnQuit2);
                    Application.DoEvents();
                    Git.updateToNextVersion(Git.nextBuild).justDoIt();
                }
                else if (rslt == DialogResult.No)
                {
                    Util.openLink("https://github.com/njthomson/SrvSurvey/releases");
                }
            }
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

            if (Game.settings.minimizeToTray)
            {
                // make sys tray icon visible upon minimizing
                this.ShowInTaskbar = this.WindowState != FormWindowState.Minimized;
                this.Visible = this.ShowInTaskbar;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
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
                        td.Current = new LatLong2(double.Parse(entry.Latitude, CultureInfo.InvariantCulture), double.Parse(entry.Longitude, CultureInfo.InvariantCulture));

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
                        extraTxt += $"  Altitude: {(int)float.Parse(entry.Altitude, CultureInfo.InvariantCulture)}m";
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
                    : entry.timestamp.ToString("u");
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
            BaseForm.show<FormBeacons>();
        }

        private void btnRuins_Click(object sender, EventArgs e)
        {
            FormRuins.show();
        }

        private async Task publishGuardians()
        {
            GuardianSiteTemplate.publish();
            Game.canonn.init(true);

            Game.git.publishLocalData(); // 1st: for updating publish data from local surveys
            await Game.canonn.readXmlSheetRuins2(); // 2nd: for updating allRuins.json and reading from Excel data
            await Game.canonn.readXmlSheetRuins3(); // 3rd: for updating allStructures.json and reading from Excel data

            Game.log("\r\n****\r\n**** Publishing all complete\r\n****");
        }

        private void checkTempHide_CheckedChanged(object sender, EventArgs e)
        {
            Program.tempHideAllPlotters = !Program.tempHideAllPlotters;
            checkTempHide.Checked = Program.tempHideAllPlotters;
            BigOverlay.invalidate();
            if (Program.tempHideAllPlotters)
                Program.hideActivePlotters();
            else
                Program.showActivePlotters();
        }

        private void Main_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var plotFss = PlotBase2.getPlotter<PlotFSS>();
            if (plotFss != null && e.Button == MouseButtons.Right)
            {
                Elite.setFocusED();
                Application.DoEvents();
                plotFss.analyzeGrab(true);
            }

            if (game != null)
                PlotBase2.renderAll(game, true);
        }

        //#region key chording

        //private void Hook_KeyUp(object? sender, KeyEventArgs e)
        //{
        //    var chord = (e.Alt ? "ALT " : "") +
        //         (e.Control ? "CTRL " : "") +
        //         (e.Shift ? "SHIFT " : "") +
        //         e.KeyCode.ToString();
        //    Game.log($"Chord:{chord} =>");

        //    //// does this chord match any actions?
        //    //foreach (var action in Game.settings.keyActions_TEST)
        //    //{
        //    //    if (action.Value == chord)
        //    //    {
        //    //        Game.log($"Chord:{chord} => {action.Key}");
        //    //        doKeyAction(action.Key);
        //    //    }
        //    //    Main.form.checkTempHide.Checked = !Main.form.checkTempHide.Checked;

        //    //}

        //    //Game.log($"?? {e.KeyCode} {e.Control} {e.Shift} {e.Alt}");
        //    //if (e.KeyCode == Keys.F2 && e.Alt)
        //    //    checkTempHide.Checked = !checkTempHide.Checked;
        //}

        //#endregion

        private void btnPredictions_Click(object sender, EventArgs e)
        {
            if (game?.systemData == null) return;
            game?.predictSystemSpecies();
            BaseForm.show<FormPredictions>();
        }

        private readonly string[] comboDevItems = new[]
        {
            "...",
            "Localize_Pseudo",
            "Localize_Real",
            "Pub_Guardian",
            "Pub_BioCriteria",
            "Pub_HumanSites",
            "Test_BioCriteria",
            "Build_BioCriteria",
            "Query_Factions",
            "Colony thumbnails"
        };

        private void comboDev_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDev.SelectedIndex == 0) return;

            // do the thing on a background thread
            comboDev.Enabled = false;
            var txt = comboDev.Text;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    Game.log($"comboDev => {txt}");
                    switch (txt)
                    {
                        case "Localize_Pseudo": await Localize.localize(true); break;
                        case "Localize_Real": await Localize.localize(false); break;
                        case "Pub_Guardian": await this.publishGuardians(); break;
                        case "Pub_BioCriteria": Game.git.publishBioCriteria(); break;
                        case "Pub_HumanSites": Game.git.publishHumanSettlements(); break;
                        case "Test_BioCriteria": await BioPredictor.testSystemsAsync(); break;
                        case "Build_BioCriteria": CriteriaBuilder.buildWholeSet(); break;
                        case "Query_Factions": await Game.spansh.queryMinorFactionSystems(); break;
                        case "Colony thumbnails": Util.makeColonyImageThumbnails(); break;

                        default: Game.log("Unexpected!"); break;
                    }

                    Game.log($"{txt} completed");
                }
                catch (Exception ex)
                {
                    Game.log($"{txt} failed!\r\n{ex}");
                }

                // restore dropdown
                this.BeginInvoke(() =>
                {
                    comboDev.SelectedIndex = 0;
                    comboDev.Enabled = true;
                });
            });
        }

        private void updateColonizationMenuItems()
        {
            if (!Game.settings.buildProjects_TEST) return;

            menuRefreshProjects.Enabled = game?.cmdrColony != null;

            var showMyProjects = (Game.settings.preferredCommander ?? Game.settings.lastCommander) != null;
            menuMyProjects.Visible = showMyProjects;

            // use where we are docked or the general primary
            var localBuildId = game?.cmdrColony?.getBuildId(game.lastDocked) ?? ColonyData.localUntrackedProject?.buildId;
            var activeBuildId = localBuildId ?? game?.cmdrColony?.primaryBuildId;
            var showCurrentProject = localBuildId != null && !string.IsNullOrEmpty(activeBuildId);
            menuCurrentProject.Visible = showCurrentProject;
            menuPrimaryProject.Visible = localBuildId != null && ColonyData.localUntrackedProject == null;
            menuPrimaryProject.Text = localBuildId == game?.cmdrColony?.primaryBuildId ? "Clear primary" : "Set primary";

            menuColonizeLine1.Visible = showCurrentProject || showMyProjects;

            var showNewProject = localBuildId == null && game?.cmdrColony != null && ColonyData.isConstructionSite(game?.lastDocked);
            menuNewProject.Visible = showNewProject;
            menuColonizeLine2.Visible = showNewProject;

            // show if docked on a Fleet Carrier and not already linked
            var showPublishFC = game?.lastDocked?.StationType == StationType.FleetCarrier;
            menuPublishFC.Visible = showPublishFC;
            menuPublishFC.Text = $"Publish FC: {game?.lastDocked?.StationName}";

            // show if in a system when FSS is complete
            var showUpdateSystemBods = game?.systemData?.fssAllBodies == true;
            menuUpdateSystem.Visible = showUpdateSystemBods;

            // show if game is active
            var showUpdateStations = game?.systemData?.address > 0 && Debugger.IsAttached; // TODO: Not ready for everyone yet
            menuUpdateStations.Visible = showUpdateStations;

            // show if any update item is shown
            menuColonizeLine4.Visible = showPublishFC || showUpdateSystemBods || showUpdateStations;
            menuUpdateHeader.Visible = showPublishFC || showUpdateSystemBods || showUpdateStations;

            // highlight the outer button if we are docked at a project
            if (localBuildId != null || showNewProject)
                btnColonize.BackColor = C.cyan;
            else
                Util.applyTheme(btnColonize, Game.settings.darkTheme);
        }

        private void menuJourney_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // decide which entries should be visible/enabled
            var cmdr = CommanderSettings.LoadCurrentOrLast();
            if (cmdr == null) return;

            var hasActiveJourney = cmdr?.activeJourney != null;

            // show BEGIN only when there's no active journey
            menuJourney.Items[2].Visible = !hasActiveJourney;
            // show NOTES when journey is active and we're in a system
            menuJourney.Items[3].Visible = hasActiveJourney;
            // show REVIEW when there is any journey
            menuJourney.Items[4].Visible = hasActiveJourney;
            var folder = Path.Combine(CommanderJourney.dataFolder, CommanderSettings.currentOrLastFid);
            menuJourney.Items[5].Visible = Directory.Exists(folder) && Directory.GetFiles(folder, "*.json").Any();
        }

        private void menuJourneyBegin_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormJourneyBegin>(this);
        }

        private void menuJourneyEdit_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormJourneyEdit>(this);
        }

        private void menuJourneyReview_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormJourneyViewer>(this);
        }

        private void menuPastJourneys_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormJourneyList>();
        }

        private void menuJourneyNotes_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormSystemNotes>(this);
        }

        private void menuFollowRoute_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormRoute>();
        }

        private void btnCodexShow_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormShowCodex>();
        }

        private void menuSpherical_Click(object sender, EventArgs e)
        {
            PlotBase2.remove(PlotSphericalSearch.def);
            new FormSphereLimit().ShowDialog(this);
            PlotBase2.addOrRemove(Game.activeGame, PlotSphericalSearch.def);
        }

        private void menuBoxel_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormBoxelSearch>();
        }

        private void menuBuildProjects_Click(object sender, EventArgs e)
        {
            if (game?.cmdrColony != null)
                BaseForm.show<FormBuildNew>();
        }

        private void menuMyProjects_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormMyProjects>();
        }

        private void menuRavenColonial_Click(object sender, EventArgs e)
        {
            Util.openLink($"{RavenColonial.uxUri}/");
        }

        private void menuUpdateSystem_Click(object sender, EventArgs e)
        {

            if (game?.systemData == null) return;
            btnColonize.Enabled = false;
            Task.Run(() => ColonyData.updateSysBodies(game.systemData).continueOnMain(this, success =>
            {
                btnColonize.Enabled = true;
                Game.log($"System updated: {success}");
                if (success)
                    MessageBox.Show(this, $"System bodies have been updated.\n\nPlease refresh any relevant web pages.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(this, $"Updating system bodies was not successful. Check logs for more details.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }));
        }

        private void menuPublishFC_Click(object sender, EventArgs e)
        {
            if (game?.lastDocked?.StationType != StationType.FleetCarrier || game.journals == null) return;

            btnColonize.Enabled = false;
            ColonyData.publishFC(game).continueOnMain(this, fc =>
            {
                btnColonize.Enabled = true;
                Game.log($"FC Published: {fc}");
                if (fc == null)
                    MessageBox.Show(this, $"Fleet Carrier publish was not successful. Check logs for more details.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    game.cmdrColony.fetchLatest().justDoIt();
                    MessageBox.Show(this, $"Fleet Carrier {fc.displayName} - {fc.name} has been published and linked to your Commander.\n\nPlease refresh any relevant web pages.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
        }

        private void menuUpdateStations_Click(object sender, EventArgs e)
        {
            if (game?.systemData?.address > 0)
                BaseForm.show<FormRavenUpdater>();
        }

        private void menuCurrentProject_Click(object sender, EventArgs e)
        {
            // use where we are docked or the general primary
            var localBuildId = game?.cmdrColony?.getBuildId(game.lastDocked) ?? ColonyData.localUntrackedProject?.buildId;
            var activeBuildId = localBuildId ?? game?.cmdrColony?.primaryBuildId;

            if (!string.IsNullOrEmpty(activeBuildId))
                Util.openLink($"{RavenColonial.uxUri}/#build={activeBuildId}");
        }

        private void menuPrimaryProject_Click(object sender, EventArgs e)
        {
            var localBuildId = game?.cmdrColony.getBuildId(game.lastDocked);
            if (game == null || string.IsNullOrEmpty(localBuildId)) return;

            // clear if local is already the primary
            if (game.cmdrColony.primaryBuildId == localBuildId)
                localBuildId = null;

            btnColonize.Enabled = false;
            Game.rcc.setPrimary(game.cmdrColony.cmdr, localBuildId).continueOnMain(this, () =>
            {
                game.cmdrColony.primaryBuildId = localBuildId;
                game.cmdrColony.Save();

                this.btnColonize.Enabled = true;
                this.updateColonizationMenuItems();
            });
        }

        private void menuNewProject_Click(object sender, EventArgs e)
        {
            if (game?.cmdrColony != null)
                BaseForm.show<FormNewProject>();
        }

        private void menuRefreshProjects_Click(object sender, EventArgs e)
        {
            if (game?.cmdrColony == null) return;

            Util.applyTheme(btnColonize, Game.settings.darkTheme);
            btnColonize.Enabled = false;
            Application.DoEvents();

            game.cmdrColony.fetchLatest().continueOnMain(this, () =>
            {
                this.btnColonize.Enabled = true;
                this.updateColonizationMenuItems();
            });
        }

        private void menuColonizeWiki_Click(object sender, EventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki/Colonization");
        }

        private void btnRamTah_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormRamTah>();
        }

        private void btnCodexBingo_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormCodexBingo>();

            //var folly = game!.cmdrColony.getProject("a4da9c2f-5bcc-4b55-a15e-c13f6c68a9d4")!;
            //var match = ColonyData.matchByCargo(folly.commodities);
            //Game.log($"folly match: {match}");

            //var json = "";
            //var entry = JsonConvert.DeserializeObject<ColonisationConstructionDepot>(json)!;
            //var match = ColonyData.matchByCargo(entry.ResourcesRequired.ToDictionary(r => r.Name.Substring(1).Replace("_name;", ""), r => r.RequiredAmount));
            //Game.log($"Entry match: {match}");

            //var filterMarket = new Spansh.SearchQuery.Markets();
            //filterMarket.Add(new Spansh.SearchQuery.Market() { name = "Copper", supply = new Spansh.Query.Market.Clause(100, 10_000_000) });
            //var filterType = new Spansh.SearchQuery.Values("Asteroid base", "Coriolis Starport", "Dockable Planet Station", "GameplayPOI", "Mega ship", "Ocellus Starport", "Orbis Starport", "Outpost", "Planetary Outpost", "Planetary Port", "Settlement", "Surface Settlement");
            //var q = new Spansh.SearchQuery
            //{
            //    page = 0,
            //    size = 10,
            //    sort = new() { new("distance", net.SortOrder.asc) },
            //    reference_system = "IC 2391 Sector LH-V b2-5",
            //    filters = new() {
            //        { "market", filterMarket },
            //        { "type", filterType },
            //    },
            //};

            //Game.spansh.queryStations(q).continueOnMain(this, rslt =>
            //{
            //    Game.log(rslt);
            //});
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            BaseForm.show<ViewLogs>();
        }

        private void btnColonize_Click(object sender, EventArgs e)
        {
            // no-op if colonization is enabled
            if (Game.settings.buildProjects_TEST) return;
            var rslt = MessageBox.Show("Colonisation features will track cargo supplied to construction sites that will be uploaded to\nhttps://ravencolonial.com\n\nThis can be disabled in Settings > External Data.\n\nEnabling this requires restarting SrvSurvey.\nWould you like to proceed?", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.Yes)
            {
                Game.log("Enabling buildProjects_TEST and restarting");
                Game.settings.buildProjects_TEST = true;
                Game.settings.Save();
                Program.forceRestart(Program.forceFid);
            }
        }
    }
}

