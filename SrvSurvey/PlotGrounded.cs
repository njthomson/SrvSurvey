using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    public partial class PlotGrounded : Form
    {
        private Game game = Game.activeGame;

        public PlotGrounded()
        {
            InitializeComponent();

            game.status.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

            this.Game_modeChanged(game.mode);
            this.Status_StatusChanged();

            //this.Opacity = 1;
            this.prepPaint();
        }

        private Boolean isSuitableMode
        {
            get
            {
                return game.mode == GameMode.InSrv || game.mode == GameMode.OnFoot || game.mode == GameMode.Landed;
            }
        }

        private void Game_modeChanged(GameMode newMode)
        {
            if (!this.isSuitableMode)
            {
                this.Opacity = 0.5;

                this.floatLeftMiddle();
            }
            else
            {
                this.Opacity = 0;

            }
        }

        private void floatLeftMiddle()
        {
            // position form top center above the heading
            var rect = Overlay.getEDWindowRect();

            this.Left = rect.Left + 40;
            this.Top = rect.Top + (rect.Height / 2) - (this.Height / 2);
        }


        private void Status_StatusChanged()
        {
            //throw new NotImplementedException();
        }

        private void PlotGrounded_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void prepPaint()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
        }

        private void PlotGrounded_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;


        }
    }
}
