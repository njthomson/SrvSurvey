using SrvSurvey.game;
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

        public Main()
        {
            InitializeComponent();

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
            if (Game.settings.formMainLocation != Point.Empty)
                this.useLastWindowLocation();

            this.bioCtrls = new List<Control>()
            {
                txtSystemBioSignals,
                txtSystemBioScanned,
                txtSystemBioValues,
                txtBodyBioSignals,
                txtBodyBioScanned,
                txtBodyBioValues,
            };

            // TODO: Remove once system signals are implemented
            lblSysBio.Enabled = txtSystemBioScanned.Enabled = txtSystemBioSignals.Enabled = txtSystemBioValues.Enabled = false;
        }

        private void useLastWindowLocation()
        {
            // position ourself within the bound of which ever screen is chosen
            var pt = Game.settings.formMainLocation;
            var r = Screen.GetBounds(pt);
            if (pt.X < r.Left) pt.X = r.Left;
            if (pt.X + this.Width > r.Right) pt.X = r.Right - this.Width;

            if (pt.Y < r.Top) pt.Y = r.Top;
            if (pt.Y + this.Height > r.Bottom) pt.Y = r.Bottom - this.Height;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = pt;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                this.Text += " (dbg)";

            if (Game.settings.focusGameOnStart)
                Elite.setFocusED();

            this.updateAllControls();

            if (!Path.Exists(Elite.displaySettingsFolder))
            {
                return;
            }

            this.checkFullScreenGraphics();

            this.lastWindowRect = Elite.getWindowRect();

            Game.canonn.init();
            SiteTemplate.Import();

            if (Elite.isGameRunning)
                this.newGame();

            this.timer1.Interval = 200;
            this.timer1.Start();

            Game.codexRef.init();

            if (!Game.settings.migratedAlphaSiteHeading)
                GuardianSiteData.migrateAlphaSites();
            //if (!Game.settings.migratedLiveAndLegacyLocations)
            //    GuardianSiteData.migrateLiveLegacyLocations();
        }

        private void updateAllControls(GameMode? newMode = null)
        {
            //Game.log("** ** ** updateAllControls ** ** **");

            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateTrackTargetTexts();
            this.updateGuardianTexts();
            this.updateSphereLimit();

            var gameIsActive = game != null && game.isRunning && game.Commander != null;

            if (gameIsActive && Game.settings.autoShowPlotFSS && (newMode == GameMode.FSS || game?.mode == GameMode.FSS))
                Program.showPlotter<PlotFSS>();
            else
                Program.closePlotter<PlotFSS>();

            if (gameIsActive && SystemStatus.showPlotter)
                Program.showPlotter<PlotSysStatus>();
            else
                Program.closePlotter<PlotSysStatus>();
        }

        private void settingsFolderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!this.IsHandleCreated) return;

            this.Invoke((MethodInvoker)delegate
            {
                Application.DoEvents();
                this.checkFullScreenGraphics();
            });
        }

        private void logFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                Game.log($"New journal file created: {e.Name}");
                if (this.game == null)
                {
                    this.newGame();
                }
            });
        }

        private void removeGame()
        {
            Game.log($"Main.removeGame, has old game: {this.game != null}");
            if (this.game != null)
            {
                Game.update -= Game_modeChanged;
                this.game.nearingBody -= Game_nearingBody;
                this.game.departingBody -= Game_departingBody;
                if (this.game.journals != null)
                    this.game.journals.onJournalEntry -= Journals_onJournalEntry;
                this.game.Dispose();
            }
            this.game = null;

            Program.closeAllPlotters();

            this.updateAllControls();
        }

        private void newGame()
        {
            Game.log($"Main.newGame: has old game:{this.game != null} ");

            if (this.game != null)
                this.removeGame();

            var newGame = new Game(Game.settings.preferredCommander);
            if (newGame.isShutdown || !newGame.isRunning)
            {
                newGame.Dispose();
                return;
            }

            this.game = newGame;

            Game.update += Game_modeChanged;
            this.game.nearingBody += Game_nearingBody;
            this.game.departingBody += Game_departingBody;
            this.game.journals!.onJournalEntry += Journals_onJournalEntry;

            if (!Game.settings.hideJournalWriteTimer)
                Program.showPlotter<PlotPulse>();

            this.updateAllControls();
        }

        private void Game_departingBody(LandableBody nearBody)
        {
            nearBody.bioScanEvent -= NearBody_bioScanEvent;
            this.NearBody_bioScanEvent();
        }

        private void Game_nearingBody(LandableBody nearBody)
        {
            this.NearBody_bioScanEvent();
            nearBody.bioScanEvent += NearBody_bioScanEvent;
        }

        private void NearBody_bioScanEvent()
        {
            this.updateAllControls();
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
            var gameIsActive = game != null && game.isRunning && game.Commander != null;

            if (!gameIsActive || game == null)
            {
                this.txtCommander.Text = game?.Commander ?? Game.settings.preferredCommander;
                if (string.IsNullOrWhiteSpace(this.txtCommander.Text))
                    this.txtCommander.Text = Game.settings.lastCommander + " ?";
                this.txtMode.Text = game?.mode == GameMode.MainMenu ? "MainMenu" : "Game is not active";
                this.txtVehicle.Text = "";
                this.txtLocation.Text = "";
                this.txtNearBody.Text = "";
                return;
            }

            var gameMode = game.cmdr.isOdyssey ? "live" : "legacy";
            this.txtCommander.Text = $"{game.Commander} (FID:{game.fid}, mode:{gameMode})";
            this.txtMode.Text = game.mode.ToString();

            if (game.atMainMenu)
            {
                this.txtLocation.Text = "";
                this.txtVehicle.Text = "";
                this.txtNearBody.Text = "";
                return;
            }

            this.txtVehicle.Text = game.vehicle.ToString();

            this.txtLocation.Text = game.cmdr?.lastSystemLocation
                ?? "Unknown";

            // TODO: use "game.cmdr.lastBody" instead??
            if (game.nearBody != null)
                this.txtNearBody.Text = "Near body";
            else
                this.txtNearBody.Text = "Deep space";
        }

        private void updateBioTexts()
        {
            if (game?.cmdr != null)
            {
                txtBioRewards.Text = Util.credits(game.cmdr.organicRewards)
                    + $", organisms: {game.cmdr.scannedOrganics.Count}";
            }

            if (game == null || game.atMainMenu || !game.isRunning || !game.initialized)
            {
                foreach (var ctrl in this.bioCtrls) ctrl.Text = "-";
                Program.closePlotter<PlotBioStatus>();
                Program.closePlotter<PlotGrounded>();
                Program.closePlotter<PlotTrackers>();
                Program.closePlotter<PlotPriorScans>();
            }
            else if (game.nearBody == null)
            {
                foreach (var ctrl in this.bioCtrls) ctrl.Text = "-";
                Program.closePlotter<PlotBioStatus>();
                Program.closePlotter<PlotGrounded>();
                Program.closePlotter<PlotTrackers>();
                Program.closePlotter<PlotPriorScans>();
            }
            else if (game.nearBody.data.countOrganisms == 0)
            {
                foreach (var ctrl in this.bioCtrls) ctrl.Text = "-";
                Program.closePlotter<PlotBioStatus>();
                Program.closePlotter<PlotGrounded>();
                Program.closePlotter<PlotPriorScans>();
            }
            else
            {
                txtBodyBioSignals.Text = game.nearBody.data.countOrganisms.ToString();
                txtBodyBioScanned.Text = game.nearBody.data.countAnalyzed.ToString();
                txtBodyBioValues.Text = Util.credits(game.nearBody.data.sumPotentialEstimate, true) + " / " + Util.credits(game.nearBody.data.sumAnalyzed);

                if (Game.settings.autoShowBioSummary && (game.showBodyPlotters || game.mode == GameMode.SAA))
                    Program.showPlotter<PlotBioStatus>();

                if (game.showBodyPlotters && Game.settings.autoShowBioPlot && !this.game.showGuardianPlotters)
                {
                    var showPlotTrackers = game.systemBody?.bookmarks?.Count > 0;
                    if (showPlotTrackers)
                        Program.showPlotter<PlotTrackers>();

                    var plotPriorScans = Program.getPlotter<PlotPriorScans>();
                    if (game.mode != GameMode.SuperCruising && (game.isLanded || showPlotTrackers || plotPriorScans != null || game.cmdr.scanOne != null))
                        Program.showPlotter<PlotGrounded>();
                }
            }
        }

        private void updateTrackTargetTexts()
        {
            if (game == null || game.atMainMenu || !game.isRunning || !game.initialized)
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
            else if (game.showBodyPlotters && (game.status!.Flags & StatusFlags.HasLatLong) > 0 && game.nearBody != null)
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
            if (!groupBox4.Visible || game == null || game.atMainMenu || !game.isRunning || !game.initialized)
            {
                lblGuardianCount.Text = "";
                txtGuardianSite.Text = "";
                Program.closePlotter<PlotGuardians>();
                btnRuinsMap.Enabled = false;
                btnRuinsOrigin.Enabled = false;
            }
            else if (game.nearBody == null || game.nearBody.settlements.Count == 0)
            {
                lblGuardianCount.Text = "0";
                txtGuardianSite.Text = "";
                Program.closePlotter<PlotGuardians>();
                btnRuinsMap.Enabled = false;
                btnRuinsOrigin.Enabled = false;
            }
            else if (Game.settings.enableGuardianSites)
            {
                lblGuardianCount.Text = game.nearBody.settlements.Count.ToString();
                if (this.game.nearBody.siteData != null)
                {
                    if (this.game.nearBody.siteData.isRuins)
                        txtGuardianSite.Text = $"Ruins #{this.game.nearBody.siteData.index} - {this.game.nearBody.siteData.type}, {this.game.nearBody.siteData.siteHeading}°";
                    else
                        txtGuardianSite.Text = $"{this.game.nearBody.siteData.type}, {this.game.nearBody.siteData.siteHeading}°";

                    if (game.showBodyPlotters && this.game.showGuardianPlotters)
                    {
                        Program.showPlotter<PlotGuardians>();
                        Program.showPlotter<PlotGuardianStatus>();
                        Program.closePlotter<PlotGrounded>();
                        Program.closePlotter<PlotBioStatus>();
                    }

                    btnRuinsMap.Enabled = game.nearBody.siteData.siteHeading != -1 && this.game.showGuardianPlotters;
                    btnRuinsOrigin.Enabled = game.nearBody.siteData.siteHeading != -1 && this.game.showGuardianPlotters;
                }
            }
        }

        private void updateSphereLimit()
        {
            btnSphereLimit.Enabled = game?.cmdr != null;

            // show/hide the sphere limit plotter
            if (game?.mode == GameMode.GalaxyMap && game.cmdr.sphereLimit.active)
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

            if (entry.OnPlanet && !entry.OnStation && game?.nearBody?.data.countOrganisms > 0)
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
                if (Game.settings.autoShowBioPlot && !this.game.showGuardianPlotters)
                    Program.showPlotter<PlotGrounded>();
            }
        }

        private void onJournalEntry(LaunchSRV entry)
        {
            Game.log($"Main.LaunchSRV {game?.status?.BodyName}");

            if (game!.showBodyPlotters && game.nearBody?.data.countOrganisms > 0)
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

            if (game!.showBodyPlotters && game.nearBody?.data.countOrganisms > 0)
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

            if (msg.StartsWith(MsgCmd.trackAdd) || msg.StartsWith(MsgCmd.trackRemove) || msg.StartsWith(MsgCmd.trackRemoveLast))
            {
                // PlotTrackers.processCommand(msg, Status.here.clone()); // TODO: retire

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
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            ViewLogs.show(Game.logs);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormSettings().ShowDialog(this);
        }

        private void setTargetLatLong()
        {
            // update our UX
            this.updateTrackTargetTexts();

            // show plotter if near a body
            if (game?.nearBody != null && game.showBodyPlotters)
                Program.showPlotter<PlotTrackTarget>();
        }

        private void btnGroundTarget_Click(object sender, EventArgs e)
        {
            var form = new FormGroundTarget();
            form.ShowDialog(this);

            if (Game.settings.targetLatLongActive)
            {
                Game.log($"Main.Set target lat/long: {Game.settings.targetLatLong}, near: {game?.cmdr.lastSystemLocation}");
                setTargetLatLong();
            }
            else
                Program.closePlotter<PlotTrackTarget>();
        }

        private void btnClearTarget_Click(object sender, EventArgs e)
        {
            Game.settings.targetLatLongActive = false;
            Game.settings.Save();

            this.updateTrackTargetTexts();
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
            var fullScreen = Elite.getGraphicsMode();
            // 0: Windows / 1: FullScreen / 2: Borderless
            lblFullScreen.Visible = fullScreen == 1;
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
                this.processScreenshot(entry, imageFilename);
            else
            {
                Game.log($"Waiting for: {imageFilename}");
                this.pendingScreenshots.Add(imageFilename, entry);
            }
        }

        private void ScreenshotWatcher_Created(object sender, FileSystemEventArgs e)
        {
            var matchFound = this.pendingScreenshots.ContainsKey(e.FullPath);
            Game.log($"Screenshot created: {e.FullPath}, matchFound: {matchFound}");

            if (matchFound)
                this.processScreenshot(this.pendingScreenshots[e.FullPath], e.FullPath);
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

            if (game!.nearBody?.siteData != null)
            {
                var siteData = game!.nearBody?.siteData!;

                var majorType = siteData.isRuins ? $"Ruins{siteData.index} " : "";
                filename += $", {majorType}{siteData.type}";

                extraTxt = $"\r\n  {siteData.nameLocalised}";
                siteType = siteData.type;
                if (siteType != GuardianSiteData.SiteType.Unknown)
                {
                    extraTxt += $" - {siteData.type}";

                    // measure distance from site origin (from the entry lat/long if possible)
                    var td = new TrackingDelta(game.nearBody!.radius, siteData.location);
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
                var txt = $"System: {entry.System}\r\nCmdr: {game!.Commander}  -  " + new DateTimeOffset(entry.timestamp).ToString("u") + extra;

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
                g.DrawString(txtBig, fontBig, Brushes.Yellow, 15, y + 5);
                g.DrawString(txt, fontSmall, Brushes.Yellow, 15, y + 5 + szBig.Height);
            }
        }

        #endregion

        private void btnAllRuins_Click(object sender, EventArgs e)
        {
            FormAllRuins.show();
        }

        private void btnGuarduanThings_Click(object sender, EventArgs e)
        {
            FormBeacons.show();
            //Program.closePlotter<PlotGuardianBeaconStatus>(); Program.showPlotter<PlotGuardianBeaconStatus>();
        }

        private void btnRuins_Click(object sender, EventArgs e)
        {
            FormRuins.show();
        }

        private void btnSphereLimit_Click(object sender, EventArgs e)
        {
            Program.closePlotter<PlotSphericalSearch>();

            var form = new FormSphereLimit();
            form.ShowDialog(this);

            this.updateSphereLimit();
        }
    }
}

