using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    internal partial class Main : Form
    {
        private Game game;

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
            this.newGame();
            this.lastWindowRect = Overlay.getEDWindowRect();
            this.timer1.Start();
        }

        private void newGame()
        {
            // TODO: use a local instance until we know the commander, then call newGame again
            this.game = new Game(null);
            this.game.modeChanged += Game_modeChanged;
            this.game.journals.onJournalEntry += Journals_onJournalEntry;

            this.txtCommander.Text = game.Commander;

            this.Game_modeChanged(this.game.mode);

            // do we have a targetLatLong to hydrate?
            if (Game.settings.targetLatLongActive)
                this.setTargetLatLong();

            if (this.game.isRunning)
            {
                Program.showPlotter<PlotPulse>();
            }
            else
            {
                // not much to do if game is not running
                return;
            }

            // are there already bio signals?
            if (game.nearBody?.Genuses?.Count > 0)
            {
                Game.log("Bio signals near!");
                this.updateBioTexts();

                if (game.showBodyPlotters && Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>();

                if (game.showBodyPlotters && Game.settings.autoShowBioPlot)
                    Program.showPlotter<PlotGrounded>();
            }
        }

        private void Game_modeChanged(GameMode newMode)
        {
            this.lblMode.Text = game.mode.ToString();
            //Game.log($"!!>> {newMode}");
            this.updateCommanderTexts();

            // DockSRV seems to be really stale :/
            // Maybe we can do something here to judge the same?
        }

        private void updateCommanderTexts()
        {
            var gameIsActive = game.isRunning && game.Commander != null;

            if (!gameIsActive)
            {
                this.txtVehicle.Text = "";
                this.txtLocation.Text = "";
                return;
            }

            this.txtCommander.Text = game.Commander;
            this.txtVehicle.Text = game.vehicle.ToString();

            if (game.nearBody != null)
                this.txtLocation.Text = game.nearBody.bodyName;
            else
                this.txtLocation.Text = "Unknown";
        }

        private void updateBioTexts()
        {
            if (game.nearBody == null)
            {
                lblBioSignalCount.Text = "";
                lblAnalyzedCount.Text = "";
                txtGenuses.Text = "";
                return;
            }

            lblBioSignalCount.Text = game.nearBody.Genuses.Count.ToString();
            lblAnalyzedCount.Text = game.nearBody.analysedSpecies.Count.ToString();

            txtGenuses.Text = string.Join(
                ", ",
                game.nearBody.Genuses.Select(_ => $"{_.Genus_Localised}:{game.nearBody.analysedSpecies.ContainsKey(_.Genus)}")
                );
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }
        
        private void onJournalEntry(Disembark entry)
        {
            //var goodEntry = entry.OnPlanet && !entry.OnStation && game.nearBody.Genuses?.Count > 0;
            //if (goodEntry && Game.settings.autoShowBioSummary)
            //    Program.showPlotter<PlotBioStatus>();
            //if (goodEntry && Game.settings.autoShowBioPlot)
            //    Program.showPlotter<PlotGrounded>();
        }

        private void onJournalEntry(LaunchSRV entry)
        {
            //if (game.showBodyPlotters && Game.settings.autoShowBioPlot)
            //    Program.showPlotter<PlotGrounded>();
        }

        private void onJournalEntry(SupercruiseEntry entry)
        {
            Game.log("SupercruiseEntry");
            // close these plotters upon super-cruise
            Program.closePlotter(nameof(PlotGrounded));
        }

        private void onJournalEntry(ApproachBody entry)
        {
            Game.log("ApproachBody");
            if (game.nearBody.Genuses?.Count > 0 && Game.settings.autoShowBioSummary)
                Program.showPlotter<PlotBioStatus>();
        }

        private void onJournalEntry(SAASignalsFound entry)
        {
            Game.log("SAASignalsFound");
            if (entry.Genuses.Count > 0 && Game.settings.autoShowBioSummary)
                Program.showPlotter<PlotBioStatus>();
        }

        private void onJournalEntry(LeaveBody entry)
        {
            Game.log("LeaveBody");

            // close these plotters upon leaving a body
            Program.closePlotter(nameof(PlotBioStatus));
            Program.closePlotter(nameof(PlotGrounded));
            Program.closePlotter(nameof(PlotTrackTarget));
        }

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
            txtTargetLatLong.Text = Game.settings.targetLatLong.ToString();
            Game.log($"New target lat/long: {Game.settings.targetLatLong}, near body: {game.nearBody != null}");

            // show plotter if near a body
            if (game.nearBody != null)
            {
                var plotter = Program.showPlotter<PlotTrackTarget>();
                plotter.setTarget(Game.activeGame.nearBody, Game.settings.targetLatLong);
                //new PlotTrackTarget().Show();
            }
        }

        private void btnGroundTarget_Click(object sender, EventArgs e)
        {
            var form = new FormGroundTarget();
            form.ShowDialog(this);

            if (Game.settings.targetLatLongActive)
            {
                setTargetLatLong();
            }
            else
            {
                Program.closePlotter(nameof(PlotTrackTarget));
            }
        }

        private void btnClearTarget_Click(object sender, EventArgs e)
        {
            txtTargetLatLong.Text = "<none>";
            Game.settings.targetLatLongActive = false;
            Game.settings.Save();

            Program.closePlotter(nameof(PlotTrackTarget));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (game.isShutdown && game.isRunning)
            {
                this.newGame();
                return;
            }

            // slow timer to check the location of the game window, repositioning plotters if needed
            var rect = Overlay.getEDWindowRect();

            if (rect != lastWindowRect) // TMP!
            {
                Game.log("moved!");
                this.lastWindowRect = rect;

                Program.repositionPlotters();
            }

            if (this.saveSettingsOnNextTick && this.Location.X > -32000 && this.Location.Y > -32000 && Game.settings.mainLocation != this.Location)
            {
                Game.settings.mainLocation = this.Location;
                Game.settings.Save();
                this.saveSettingsOnNextTick = false;
            }
        }

        private void Main_LocationChanged(object sender, EventArgs e)
        {
            if (Game.settings.mainLocation != this.Location)
                this.saveSettingsOnNextTick = true;
        }
    }
}

