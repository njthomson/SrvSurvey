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

        // various game modes that can be active
        private PlotBioStatus plotBioStatus;
        private bool bioScanning = false;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.newGame();

            // do we have a targetLatLong to hydrate?
            if (Game.settings.targetLatLong != null)
                this.setTargetLatLong(Game.settings.targetLatLong);

            if (this.game.isRunning)
            {
                PlotPulse.show();
            }

            // is there a game mode/tool we should highlight?
            if (game.nearBody?.Genuses?.Count > 0)
            {
                Game.log("Bio signals near!");
                this.updateBioTexts();
            }

            // TMP!
            //new PlotGroundTarget(Game.activeGame.nearBody, new LatLong2(10.0, 40.0)).ShowDialog();
            //new PlotGrounded().Show(this);
            //btnBioScan_Click(sender, e);
        }

        private void newGame()
        {
            this.game = new Game(null);
            this.game.modeChanged += Game_modeChanged;

            this.txtCommander.Text = game.Commander;

            this.Game_modeChanged(this.game.mode);
        }

        private void Game_modeChanged(GameMode newMode)
        {
            this.lblMode.Text = game.mode.ToString();
            this.updateCommanderTexts();
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
            if (this.plotBioStatus == null)
            {
                this.plotBioStatus = new PlotBioStatus();
                this.plotBioStatus.FormClosed += PlotBioStatus_FormClosed;
                this.plotBioStatus.Show(this);
            }

        }

        private void PlotBioStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.plotBioStatus = null;
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
                new PlotTrackTarget(Game.activeGame.nearBody, Game.settings.targetLatLong).Show();
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

            var plotter = Program.activePlotters.FirstOrDefault(_ => _.Name == nameof(PlotTrackTarget));
            Game.log($"Clearing lat/long. Active plotter: {plotter != null}");
            if (plotter != null)
            {
                plotter.Close();
                Program.activePlotters.Remove(plotter);
            }
        }
    }
}

