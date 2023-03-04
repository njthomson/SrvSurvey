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

        private bool bioScanning = false;
        private Rectangle lastWindowRect;

        public Main()
        {
            InitializeComponent();
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
            if (Game.settings.targetLatLong != null)
                this.setTargetLatLong(Game.settings.targetLatLong);

            if (this.game.isRunning)
            {
                Program.showPlotter<PlotPulse>(this);
            }

            // are there already bio signals?
            if (game.nearBody?.Genuses?.Count > 0)
            {
                Game.log("Bio signals near!");
                this.updateBioTexts();

                if (Game.settings.autoShowBioSummary)
                    Program.showPlotter<PlotBioStatus>(this);

                if (Game.settings.autoShowBioPlot && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
                    Program.showPlotter<PlotGrounded>(this);
            }
        }

        private void Game_modeChanged(GameMode newMode)
        {
            this.lblMode.Text = game.mode.ToString();
            Game.log($"!!>> {newMode}");
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
                game.nearBody.Genuses.Select(_ => _.Genus_Localised)
                );

            // still ?
            btnBioScan.BackColor = Game.settings.GameOrange; // GameColors.Orange;
        }


        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(Disembark entry)
        {
            if (Game.settings.autoShowBioPlot)
                Program.showPlotter<PlotGrounded>(this);
        }

        private void onJournalEntry(Embark entry)
        {
            //Program.closePlotter(nameof(PlotGrounded));
        }

        private void onJournalEntry(LaunchSRV entry)
        {
            if (Game.settings.autoShowBioPlot)
                Program.showPlotter<PlotGrounded>(this);
        }

        private void onJournalEntry(DockSRV entry)
        {
            Program.closePlotter(nameof(PlotGrounded));
        }

        private void btnOverlay_Click(object sender, EventArgs e)
        {
            //var handleED = Overlaying.getEDWindowHandle();

            //var rect = new RECT();
            //var rslt = Overlaying.GetWindowRect(handleED, ref rect);

            this.Text = Overlay.getEDWindowRect().ToString();

            //return procED[0].MainWindowHandle;
        }

        private void btnBioScan_Click(object sender, EventArgs e)
        {
            Program.showPlotter<PlotBioStatus>(this);
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

        private void setTargetLatLong(LatLong2 targetLatLong)
        {
            // update settings
            Game.settings.targetLatLong = targetLatLong;
            Game.settings.Save();

            // update our UX
            txtTargetLatLong.Text = Game.settings.targetLatLong.ToString();
            Game.log($"New target lat/long: {Game.settings.targetLatLong}, near body: {game.nearBody != null}");

            // show plotter if near a body
            if (game.nearBody != null)
            {
                var plotter = Program.showPlotter<PlotTrackTarget>(this);
                plotter.setTarget(Game.activeGame.nearBody, Game.settings.targetLatLong);
                //new PlotTrackTarget().Show();
            }
        }

        private void btnGroundTarget_Click(object sender, EventArgs e)
        {
            var form = new FormGroundTarget();
            var rslt = form.ShowDialog(this);

            if (rslt == DialogResult.OK)
            {
                setTargetLatLong(form.targetLatLong);
            }
        }

        private void btnClearTarget_Click(object sender, EventArgs e)
        {
            txtTargetLatLong.Text = "<none>";
            Game.settings.targetLatLong = null;
            Game.settings.Save();

            Program.closePlotter(nameof(PlotTrackTarget));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // slow timer to check the location of the game window, repositioning plotters if needed
            var rect = Overlay.getEDWindowRect();

            if (rect != lastWindowRect)
            {
                Game.log("moved!");
                this.lastWindowRect = rect;

                Program.repositionPlotters();
            }
        }
    }
}

