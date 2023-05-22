using SrvSurvey.game;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotVertialStripe : Form, PlotterForm, IDisposable
    {
        public enum Mode
        {
            Buttress,
            Alpha,
            Beta,
            Gamma,
        }

        // TODO: use an enum!
        public static Mode mode;

        protected Game game = Game.activeGame!;

        private PlotVertialStripe()
        {
            this.Text = "";

            this.StartPosition = FormStartPosition.Manual;
            this.Width = 800;
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
            this.Width = 600;
            this.Left = gameRect.Left + (gameRect.Width / 2) - (this.Width / 2);

            this.Top = gameRect.Top;
            this.Height = gameRect.Height - 10;

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
            if (mode != Mode.Buttress)
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

            switch (mode)
            {
                case Mode.Alpha:
                    this.drawAlphaSiteTarget(g);
                    break;
                case Mode.Beta:
                    this.drawBetaSiteTarget(g);
                    break;
                case Mode.Gamma:
                    this.drawGammaSiteTarget(g);
                    break;

                case Mode.Buttress:
                default:
                    this.drawButtressTarget(g);
                    break;
            }
        }

        private void drawLine(Graphics g, float x1, float y1, float x2, float y2)
        {
            var p1 = GameColors.penYellow8;
            var p2 = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };

            g.DrawLine(p1, x1, y1, x2, y2);
            g.DrawLine(p2, x1, y1, x2, y2);
        }

        private void drawAlphaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };

            var w = (this.Width / 2f);
            var h = (this.Height / 2f);

            //drawLine(g, w, 0, w, 2 * h);

            //drawLine(g, 0, h, 2 * w, h);

            //var dy = h * 0.65f;
            //drawLine(g, 0, h+dy, 2 * w, h+dy);
            //drawLine(g, 0, h-dy, 2 * w, h-dy);

            //return;


            // pit spike
            var d = h * 0.50f; //300f;

            var x = w + d / 2f;
            var y = h + d / 3f;
            g.DrawLine(GameColors.penYellow8, x, y, x + 120, y);
            g.DrawLine(pp, x, y, x + 120, y);

            /*
            g.DrawLine(GameColors.penYellow8, x, y, x - 45, y + 20);
            g.DrawLine(pp, x, y, x - 45, y + 20);

            g.DrawLine(GameColors.penYellow8, x, y, x + 70, y + 15);
            g.DrawLine(pp, x, y, x + 70, y + 15);
            x += 10;
            y -= 20;
            g.DrawLine(GameColors.penYellow8, x, y, x - 45, y - 40);
            g.DrawLine(pp, x, y, x - 45, y - 40);
            g.DrawLine(GameColors.penYellow8, x, y, x + 70, y + 15);
            g.DrawLine(pp, x, y, x + 70, y + 15);
            g.DrawLine(GameColors.penYellow8, x - 10 + 70, y + 20 + 15, x + 70, y + 15);
            g.DrawLine(pp, x - 10 + 70, y + 20 + 15, x + 70, y + 15);
            */

            // pit circles
            x = w - (d / 2f);
            y = h - (d / 2f);
            var rect = new RectangleF(x, y, d, d);
            g.DrawEllipse(GameColors.penYellow8, rect);
            g.DrawEllipse(pp, rect);

            rect.Inflate(40, 40);
            g.DrawEllipse(GameColors.penYellow8, rect);
            g.DrawEllipse(pp, rect);

            rect.Inflate(-d * 0.4f, -d * 0.4f);
            g.DrawEllipse(GameColors.penYellow8, rect);
            g.DrawEllipse(pp, rect);

            //// orbs
            //d = 100;
            //rect = new RectangleF(w + 150, h + 240, d, d);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

            //d = 70;
            //rect = new RectangleF(w - 250, h + 125, d, d);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

            //rect = new RectangleF(w - 270, h - 160, d, d);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);


            //g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);

            //rect.Inflate(-10, -10);
            //g.DrawEllipse(pp, rect);

            ////g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);
            //g.DrawLine(pp, w, y + d, w, y + 456);
        }

        private void drawBetaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 4) { DashStyle = DashStyle.Dot };

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

        private void drawGammaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 4) { DashStyle = DashStyle.Dash };

            var w = (this.Width / 2f);
            var h = (this.Height / 2f);

            var d = 60f;
            var x = w + 90;
            var y = h - 385;
            //var rect = new RectangleF(x, y, d, d);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

            //rect.Inflate(35, 35);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

            //rect.Inflate(30, 30);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

            //y += 25;
            //g.DrawLine(GameColors.penYellow8, x, y, x - 160, y);
            //g.DrawLine(pp, x, y, x - 160, y);

            x = w + 60;
            y = h - 234;
            g.DrawLine(GameColors.penYellow8, x, y, x - 120, y + 140);
            g.DrawLine(pp, x, y, x - 120, y + 140);

            //x = w + 60;
            //y = h - 734;
            //g.DrawLine(GameColors.penYellow8, x, y, x - 120, y + 140);
            //g.DrawLine(pp, x, y, x - 120, y + 140);


            //rect.Inflate(-10, -10);
            //g.DrawEllipse(pp, rect);

            //g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);
            //g.DrawLine(pp, w, y + d, w, y + 456);
        }

        private void drawButtressTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 4) { DashStyle = DashStyle.Dash };
            var ppp = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };

            // top / height
            var t = this.Height / 2f;
            var h = (this.Height - t) * 0.8f;

            var nightVision = true; // (game.status.Flags & StatusFlags.NightVision) == 0;

            // central thick line
            var w = (this.Width / 2);
            g.DrawLine(nightVision ? GameColors.penYellow8 : GameColors.penCyan8, w, t, w, t + h);
            g.DrawLine(pp, w, t, w, t + h);

            // thinner lines
            var w1 = 40; // width one
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan4, w - w1, t + 40, w - w1, t + 220);
            g.DrawLine(ppp, w - w1, t + 40, w - w1, t + 220);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan4, w + w1, t + 40, w + w1, t + 220);
            g.DrawLine(ppp, w + w1, t + 40, w + w1, t + 220);

            var w2 = 70; // width two
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, w - w2, t + 60, w - w2, t + 200);
            g.DrawLine(ppp, w - w2, t + 60, w - w2, t + 200);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, w + w2, t + 60, w + w2, t + 200);
            g.DrawLine(ppp, w + w2, t + 60, w + w2, t + 200);

            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, w - w2, t + 80, w + w2, t + 80);
            g.DrawLine(ppp, w - w2, t + 80, w + w2, t + 80);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, w - w2 - 50, t + 130, w + w2 + 50, t + 130);
            g.DrawLine(ppp, w - w2 - 50, t + 130, w + w2 + 50, t + 130);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, w - w2, t + 180, w + w2, t + 180);
            g.DrawLine(ppp, w - w2, t + 180, w + w2, t + 180);
        }
    }
}
