using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    internal class PlotVertialStripe : Form, PlotterForm
    {
        private PlotVertialStripe()
        {
            this.Text = "";

            this.StartPosition = FormStartPosition.Manual;
            this.Width = 200;
            this.Height = 200;
            this.MinimumSize = Size.Empty;

            this.FormBorderStyle = FormBorderStyle.None;

            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.Visible = false;

            this.Opacity = Game.settings.Opacity;
            this.TopMost = true;

            this.BackColor = Color.Red;
            this.TransparencyKey = Color.Red;
            this.AllowTransparency = true;
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            // float center, spanning whole height
            this.Left = gameRect.Left + (gameRect.Width / 2) - 100;
            this.Width = 200;

            this.Top = gameRect.Top;
            this.Height = gameRect.Height;

            var mode = Elite.getGraphicsMode();
            if (mode == 0)
            {
                // if window'd
                this.Left += 8;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var gameRect = Elite.getWindowRect();
            this.reposition(gameRect);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Red);

            // top . height
            var t = this.Height / 2f;
            var h = (this.Height - t) * 0.8f;

            // central thick line
            g.DrawLine(GameColors.penCyan8, 100, t, 100, t + h);

            // thinner lines
            var w1 = 40; // width one
            g.DrawLine(GameColors.penCyan4, 100 - w1, t+40, 100 - w1, t + 220);
            g.DrawLine(GameColors.penCyan4, 100 + w1, t+40, 100 + w1, t + 220);

            var w2 = 70; // width two
            g.DrawLine(GameColors.penCyan2, 100 - w2, t + 60, 100 - w2, t + 200);
            g.DrawLine(GameColors.penCyan2, 100 + w2, t + 60, 100 + w2, t + 200);

            var ch = 80; // cross height
            g.DrawLine(GameColors.penCyan2, 100 - w2, t + ch, 100 + w2, t + ch);
            g.DrawLine(GameColors.penCyan2, 100 - w2, t + ch + ch, 100 + w2, t + ch + ch);
        }
    }
}
