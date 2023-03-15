using SrvSurvey.game;
using System.Data;
using System.Diagnostics;

namespace SrvSurvey
{
    internal partial class Main : Form
    {
        private Game game;
        private FileSystemWatcher folderWatcher;

        private Rectangle lastWindowRect;
        private bool saveSettingsOnNextTick = false;

        public Main()
        {
            InitializeComponent();

            // can we fit in our last location
            if (Game.settings.mainLocation != Point.Empty)
                this.useLastLocation();
        }

        private void useLastLocation()
        {
            // position ourself within the bound of which ever screen is chosen
            var pt = Game.settings.mainLocation;
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
            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateTrackTargetTexts();

            this.lastWindowRect = Overlay.getEDWindowRect();

            if (Overlay.isGameRunning)
            {
                this.newGame();
            }

            this.folderWatcher = new FileSystemWatcher(SrvSurvey.journalFolder, "*.log");
            this.folderWatcher.Created += FolderWatcher_Created;
            this.folderWatcher.EnableRaisingEvents = true;
            Game.log($"Watching folder: {SrvSurvey.journalFolder}");

            this.timer1.Start();
        }

        private void FolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Game.log($"New journal file created: {e.Name}");
            if (this.game == null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.newGame();
                });
            }
        }

        private void removeGame()
        {
            Game.log($"Main.removeGame, has old game: {this.game != null}");
            if (this.game != null)
            {
                this.game.modeChanged -= Game_modeChanged;
                this.game.nearingBody -= Game_nearingBody;
                this.game.departingBody -= Game_departingBody;
                if (this.game.journals != null)
                    this.game.journals.onJournalEntry -= Journals_onJournalEntry;
                this.game.Dispose();
            }
            this.game = null;

            Program.closeAllPlotters();

            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateTrackTargetTexts();
        }

        private void newGame()
        {
            Game.log($"Main.newGame: has old game:{this.game != null} ");

            if (this.game != null)
                this.removeGame();

            var newGame = new Game(null);
            if (newGame.isShutdown || !newGame.isRunning)
            {
                newGame.Dispose();
                return;
            }

            this.game = newGame;

            this.game.modeChanged += Game_modeChanged;
            this.game.nearingBody += Game_nearingBody;
            this.game.departingBody += Game_departingBody;
            this.game.journals.onJournalEntry += Journals_onJournalEntry;

            Program.showPlotter<PlotPulse>();

            // are there already bio signals?
            this.updateBioTexts();

            // should we show the track-target plotter?
            this.updateTrackTargetTexts();
            this.updateCommanderTexts();
        }

        private void Game_departingBody()
        {
            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateTrackTargetTexts();
        }

        private void Game_nearingBody(LandableBody nearBody)
        {
            this.updateCommanderTexts();
            this.updateBioTexts();
            this.updateTrackTargetTexts();
        }

        private void Game_modeChanged(GameMode newMode)
        {
            if (this.txtMode.Text != newMode.ToString())
            {
                this.updateCommanderTexts();
                this.updateBioTexts();
                this.updateTrackTargetTexts();
            }
        }

        private void updateCommanderTexts()
        {
            var gameIsActive = game != null && game.isRunning && game.Commander != null;

            if (!gameIsActive)
            {
                this.txtCommander.Text = "";
                this.txtMode.Text = game?.mode == GameMode.MainMenu ? "MainMenu" : "Game is not active";
                this.txtVehicle.Text = "";
                this.txtLocation.Text = "";
                this.txtNearBody.Text = "";
                return;
            }

            this.txtCommander.Text = game.Commander;
            this.txtMode.Text = game.mode.ToString();

            Game.log($"Main.updateCommanderTexts: game.atMainMenu: {game.atMainMenu}");
            if (game.atMainMenu)
            {
                this.txtLocation.Text = "";
                this.txtVehicle.Text = "";
                this.txtNearBody.Text = "";
                return;
            }

            this.txtVehicle.Text = game.vehicle.ToString();

            if (!string.IsNullOrEmpty(game.systemLocation))
                this.txtLocation.Text = game.systemLocation;
            else if (!string.IsNullOrEmpty(game.starSystem))
                this.txtLocation.Text = game.starSystem;
            else
                this.txtLocation.Text = "Unknown";

            if (game.nearBody != null)
                this.txtNearBody.Text = "Near body";
            else
                this.txtNearBody.Text = "Deep space";
        }

        private void updateBioTexts()
        {
            if (game == null || game.atMainMenu || !game.isRunning || !game.initialized)
            {
                lblBioSignalCount.Text = "-";
                lblAnalyzedCount.Text = "-";
                txtGenuses.Text = "";
            }
            else if (game.nearBody == null)
            {
                lblBioSignalCount.Text = "-";
                lblAnalyzedCount.Text = "-";
                txtGenuses.Text = "No body close enough";
                Program.closePlotter(nameof(PlotBioStatus));
                Program.closePlotter(nameof(PlotGrounded));
            }
            else if (game.nearBody.Genuses == null || game.nearBody.Genuses.Count == 0)
            {
                lblBioSignalCount.Text = "-";
                lblAnalyzedCount.Text = "-";
                txtGenuses.Text = "No biological signals detected";
                Program.closePlotter(nameof(PlotBioStatus));
                Program.closePlotter(nameof(PlotGrounded));
            }
            else
            {
                Game.log("Main.Bio signals near!");

                lblBioSignalCount.Text = game.nearBody.Genuses.Count.ToString();
                lblAnalyzedCount.Text = game.nearBody.analysedSpecies.Count.ToString();

                txtGenuses.Text = string.Join(
                    "\r\n",
                    game.nearBody.Genuses.Select(_ => $"{_.Genus_Localised}:{game.nearBody.analysedSpecies.ContainsKey(_.Genus)}")
                );

                if (game.showBodyPlotters)
                {
                    if (Game.settings.autoShowBioSummary)
                        Program.showPlotter<PlotBioStatus>();

                    if (game.isLanded && Game.settings.autoShowBioPlot)
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
                Program.closePlotter(nameof(PlotTrackTarget));
            }
            else if (!Game.settings.targetLatLongActive || game == null || game.atMainMenu)
            {
                txtTargetLatLong.Text = "<none>";
                lblTrackTargetStatus.Text = "Inactive";
                Program.closePlotter(nameof(PlotTrackTarget));
            }
            else if (game.showBodyPlotters && (game.status.Flags & StatusFlags.HasLatLong) > 0)
            {
                txtTargetLatLong.Text = Game.settings.targetLatLong.ToString();
                lblTrackTargetStatus.Text = "Active";
                Program.showPlotter<PlotTrackTarget>();
            }
            else
            {
                txtTargetLatLong.Text = Game.settings.targetLatLong.ToString();
                lblTrackTargetStatus.Text = "Ready";
                Program.closePlotter(nameof(PlotTrackTarget));
            }
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        //private void onJournalEntry(LoadGame entry)
        //{
        //    Game.log($"Main.LoadGame: Commander {entry.Commander} / {entry.FID} / {entry.GameMode}");
        //    Program.closeAllPlotters();

        //    // kill any existing plotters, then start over
        //    this.newGame();
        //}

        private void onJournalEntry(Music entry)
        {
            var atMainMenu = entry.MusicTrack == "MainMenu";
            if (atMainMenu)
            {
                Game.log($"Main.MusicTrack == MainMenu, Commander:{this.game?.Commander}");
                //this.removeGame();
            }
        }

        private void onJournalEntry(Shutdown entry)
        {
            Game.log($"Main.Shutdown: Commander:{this.game?.Commander}");
            this.removeGame();
        }

        private void onJournalEntry(Disembark entry)
        {
            Game.log($"Main.Disembark {entry.Body}");

            if (entry.OnPlanet && !entry.OnStation && game.nearBody.Genuses?.Count > 0)
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
                if (Game.settings.autoShowBioPlot)
                    Program.showPlotter<PlotGrounded>();
            }
        }

        private void onJournalEntry(LaunchSRV entry)
        {
            Game.log($"Main.LaunchSRV {game.status.BodyName}");

            if (game.showBodyPlotters && game.nearBody.Genuses?.Count > 0)
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
                if (Game.settings.autoShowBioPlot)
                    Program.showPlotter<PlotGrounded>();
            }
        }

        private void onJournalEntry(ApproachBody entry)
        {
            Game.log($"Main.ApproachBody {entry.Body}");

            if (game.showBodyPlotters && game.nearBody.Genuses?.Count > 0)
            {
                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();
            }

            if (Game.settings.targetLatLongActive && game.showBodyPlotters)
            {
                Program.showPlotter<PlotTrackTarget>();
            }
        }

        //private void onJournalEntry(SAASignalsFound entry)
        //{
        //    Game.log($"Main.SAASignalsFound {entry.BodyName}");
        //    if (entry.Genuses?.Count > 0)
        //    {
        //        if (Game.settings.autoShowBioSummary)
        //            Program.showPlotter<PlotBioStatus>();
        //    }
        //}

        private void onJournalEntry(SupercruiseEntry entry)
        {
            Game.log($"Main.SupercruiseEntry {entry.Starsystem}");

            // close these plotters upon super-cruise
            Program.closePlotter(nameof(PlotGrounded));
        }

        //private void onJournalEntry(LeaveBody entry)
        //{
        //    Game.log($"LeaveBody {entry.Body}");

        //    // close these plotters upon leaving a body
        //    Program.closePlotter(nameof(PlotBioStatus));
        //    Program.closePlotter(nameof(PlotGrounded));
        //    Program.closePlotter(nameof(PlotTrackTarget));
        //}

        private void onJournalEntry(SendText entry)
        {
            /*
            switch (entry.Message)
            {
                case "11":

                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Log,
                        Genus = "$Codex_Ent_Shrubs_Genus_Name;",
                        Species = "Anemone Foo",
                        Species_Localised = "Tussock Cultro",
                        Body = game.nearBody.bodyId,
                        SystemAddress = game.nearBody.systemAddress,
                    });
                    return;
                case "12":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Sample,
                        Genus = "$Codex_Ent_Shrubs_Genus_Name;",
                        Species = "Anemone Foo",
                        Species_Localised = "Tussock Cultro",
                        Body = game.nearBody.bodyId,
                        SystemAddress = game.nearBody.systemAddress,
                    });
                    return;
                case "13":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Analyse,
                        Genus = "$Codex_Ent_Shrubs_Genus_Name;",
                        Species = "Anemone Foo",
                        Species_Localised = "Tussock Cultro",
                        Body = game.nearBody.bodyId,
                        SystemAddress = game.nearBody.systemAddress,
                    });
                    return;

                case "21":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Log,
                        Genus = "$Codex_Ent_Stratum_Genus_Name;",
                        Species = "Stratum Tectonicas",
                        Species_Localised = "Stratum Tectonicas",
                        Body = game.nearBody.bodyId,
                        SystemAddress = game.nearBody.systemAddress,
                    });
                    return;
                case "22":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Sample,
                        Genus = "$Codex_Ent_Stratum_Genus_Name;",
                        Species = "Stratum Tectonicas",
                        Species_Localised = "Stratum Tectonicas",
                        Body = game.nearBody.bodyId,
                        SystemAddress = game.nearBody.systemAddress,
                    });
                    return;
                case "23":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Analyse,
                        Genus = "$Codex_Ent_Stratum_Genus_Name;",
                        Species = "Stratum Tectonicas",
                        Species_Localised = "Stratum Tectonicas",
                        Body = game.nearBody.bodyId,
                        SystemAddress = game.nearBody.systemAddress,
                    });
                    return;
            }
            // */

            //}
            //var newScan = new BioScan()
            //{
            //    location = new LatLong2(game.status),
            //    radius = BioScan.ranges[fakeGenus],
            //    genus = fakeGenus,
            //};
            //this.bioScans.Add(newScan);
            //Game.log($"Fake scan: {newScan}");
            this.Invalidate();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Main_DoubleClick(object sender, EventArgs e)
        {
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
            if (game.nearBody != null && game.showBodyPlotters)
                Program.showPlotter<PlotTrackTarget>();
        }

        private void btnGroundTarget_Click(object sender, EventArgs e)
        {
            var form = new FormGroundTarget();
            form.ShowDialog(this);

            if (Game.settings.targetLatLongActive)
            {
                Game.log($"Main.Set target lat/long: {Game.settings.targetLatLong}, near: {game.systemLocation}");
                setTargetLatLong();
            }
            else
                Program.closePlotter(nameof(PlotTrackTarget));
        }

        private void btnClearTarget_Click(object sender, EventArgs e)
        {
            Game.settings.targetLatLongActive = false;
            Game.settings.Save();

            this.updateTrackTargetTexts();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (this.saveSettingsOnNextTick && this.Location.X > -32000 && this.Location.Y > -32000 && Game.settings.mainLocation != this.Location)
            {
                Game.settings.mainLocation = this.Location;
                Game.settings.Save();
                this.saveSettingsOnNextTick = false;
            }

            // slow timer to check the location of the game window, repositioning plotters if needed
            var rect = Overlay.getEDWindowRect();

            if (rect != lastWindowRect)
            {
                Game.log($"Main.ED window changed, active: {rect != Rectangle.Empty}");
                this.lastWindowRect = rect;

                Program.repositionPlotters();
            }
        }

        private void Main_LocationChanged(object sender, EventArgs e)
        {
            if (Game.settings.mainLocation != this.Location)
                this.saveSettingsOnNextTick = true;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            new FormSettings().ShowDialog(this);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://njthomson.github.io/SrvSurvey/");
        }

        private void Main_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && Game.settings.focusGameOnMinimize)
            {
                Game.log($"Main.Minimized, focusing on game...");
                Overlay.setFocusED();
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Game.settings.Save();
        }
    }
}

