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
    public partial class Main : Form
    {
        private Game game;

        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            this.newGame();

            //new PlotGroundTarget(Game.activeGame.nearBody, new LatLong2(10.0, 40.0)).ShowDialog();
        }

        private void newGame()
        {
            this.game = new Game(null);
            this.game.modeChanged += Game_modeChanged;

            this.lblCommander.Text = game.Commander;
            this.lblMode.Text = game.mode.ToString();
        }

        private void Game_modeChanged(GameMode newMode)
        {
            this.lblMode.Text = game.mode.ToString();
        }

        private int wideWidth = 0;

        private void goNarrow()
        {
            this.wideWidth = this.Width;
            this.Width = 80;
            this.panel1.Hide();
        }

        private void goWide()
        {
            this.Width = this.wideWidth;
            this.panel1.Show();
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
            //new JUNK().Show();
            //this.TopMost = true;
            //this.Opacity = 0.5;
            //Overlay.setOverlay(this);
            var a = 20 - new Angle(45);
            Game.log(a);
        }

        private void btnSurvey_Click(object sender, EventArgs e)
        {
            if (Game.activeGame.nearBody != null)
                new PlotGroundTarget(Game.activeGame.nearBody, new LatLong2(10.0, 40.0)).ShowDialog();

            //var mm = new TrackingDelta(1000, new LatLong2(10, 10), new LatLong2(10 + 30, 10+40));
            //var mm = new TrackingDelta(1000, new LatLong2(-10, -10), new LatLong2(20, 30));
            //Game.log(mm.MetersToString(123.45678));


            //game.status
            //Overlay.setFocusED();
            //lblMode.Text = "Foot";
            //Overlay.setFormMinimal(this);
            //LatLong2 ll2 = new LatLong2(-100, -40);
            //a += 140;
            this.Text = $"{ game.isRunning } / {game.Commander} / {game.mode}";
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Main_DoubleClick(object sender, EventArgs e)
        {
            //Overlay.setFormMinimal(this);
            this.goNarrow();
        }


        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            ViewLogs.show(game.logs);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog(this);
        }

        private void btnGroundTarget_Click(object sender, EventArgs e)
        {
            new FormGroundTarget().ShowDialog();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog(this);
        }
    }
}

