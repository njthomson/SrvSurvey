using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    internal class PlotVertialStripe : Form, PlotterForm, IDisposable
    {
        // TODO: use an enum!
        public static bool mode = false;

        protected Game game = Game.activeGame!;

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

            game.status.StatusChanged += Status_StatusChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (game?.status != null)
                {
                    game.status!.StatusChanged -= Status_StatusChanged;
                }
            }

            base.Dispose(disposing);
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = getOpacity() * Game.settings.Opacity;

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

        private void Status_StatusChanged(bool blink)
        {
            this.Opacity = getOpacity() * Game.settings.Opacity;
            this.Invalidate();
        }

        private double getOpacity()
        {
            if (mode)
                return 1;

            const float limit = 200;
            if (game.status.Altitude > limit + 100)
                return 0;
            else if (game.status.Altitude < 100)
                return 1;
            else
                return 1f - ((game.status.Altitude - 100) / limit);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var gameRect = Elite.getWindowRect();
            this.reposition(gameRect);
        }

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Debugger.IsAttached)
            {
                // use a different cursor if debugging
                this.Cursor = Cursors.No;
            }
            else if (Game.settings.hideOverlaysFromMouse)
            {
                // move the mouse outside the overlay
                System.Windows.Forms.Cursor.Position = new Point(
                    Elite.gameCenter.X,
                    this.Left - 50);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Red);

            if (PlotVertialStripe.mode)
                this.drawBetaSiteTarget(g);
            else
                this.drawButtressTarget(g);

        }

        private void drawBetaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 4)
            {
                DashStyle = DashStyle.Dot
            };

            var w = (this.Width / 2f);

            var d = 120f;
            var x = w - (d / 2f);
            var y = this.Height * 0.255f;
            var rect = new RectangleF(x, y, d, d);
            g.FillEllipse(Brushes.Yellow, rect);
            g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);

            rect.Inflate(-10, -10);
            g.DrawEllipse(pp, rect);

            //g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);
            g.DrawLine(pp, w, y + d, w, y + 456);
        }

        private void drawButtressTarget(Graphics g)
        {

            // top / height
            var t = this.Height / 2f;
            var h = (this.Height - t) * 0.8f;

            var nightVision = true; // (game.status.Flags & StatusFlags.NightVision) == 0;

            // central thick line
            g.DrawLine(nightVision ? GameColors.penYellow8 : GameColors.penCyan8, 100, t, 100, t + h);

            // thinner lines
            var w1 = 40; // width one
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan4, 100 - w1, t + 40, 100 - w1, t + 220);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan4, 100 + w1, t + 40, 100 + w1, t + 220);

            var w2 = 70; // width two
            g.DrawLine(nightVision ? GameColors.penYellow2 : GameColors.penCyan2, 100 - w2, t + 60, 100 - w2, t + 200);
            g.DrawLine(nightVision ? GameColors.penYellow2 : GameColors.penCyan2, 100 + w2, t + 60, 100 + w2, t + 200);

            g.DrawLine(nightVision ? GameColors.penYellow2 : GameColors.penCyan2, 100 - w2, t + 80, 100 + w2, t + 80);
            g.DrawLine(nightVision ? GameColors.penYellow2 : GameColors.penCyan2, 100 - w2 - 50, t + 130, 200 + w2, t + 130);
            g.DrawLine(nightVision ? GameColors.penYellow2 : GameColors.penCyan2, 100 - w2, t + 180, 100 + w2, t + 180);
        }
    }
}
