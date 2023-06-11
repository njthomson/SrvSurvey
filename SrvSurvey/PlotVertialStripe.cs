using SrvSurvey.game;
using SrvSurvey.units;
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
            RelicTower,
        }

        public static Mode mode;
        public static double targetAltitude;

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
            //if (this.Opacity > 0)
            this.Opacity = getOpacity() * Game.settings.Opacity;

            this.Invalidate();
        }

        private double getOpacity()
        {
            if (PlotVertialStripe.mode == Mode.RelicTower)
                return 0.8;


            var delta = Math.Abs(game.status.Altitude - targetAltitude);
            if (delta > 220)
                return 0;
            else if (delta < 20)
                return 0.8;
            else
                return (220 - (delta)) / 200f;


            //const float limit = 200;
            //if (game.status.Altitude > limit + 100)
            //    return 0;
            //else if (game.status.Altitude < 100)
            //    return 1;
            //else
            //    return 1f - ((game.status.Altitude - 100) / limit);
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

            drawAngleString(g);

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
                case Mode.RelicTower:
                    this.drawRelicTowerTarget(g);
                    break;

                case Mode.Buttress:
                default:
                    this.drawButtressTarget(g);
                    break;
            }
        }

        private void drawAngleString(Graphics g)
        {
            var w = (this.Width / 2f);
            var y = 1;
            g.FillRectangle(Brushes.Black, w - 45, 5 + y, 85, 30);

            var txt = $"{new Angle(game.status.Heading)}";
            var sz = g.MeasureString(txt, Game.settings.fontBig);

            var x = (this.Width / 2f) - (sz.Width / 2);
            g.DrawString(txt, Game.settings.fontBig, Brushes.Yellow, x, y);
        }

        private void drawLine(Graphics g, float x1, float y1, float x2, float y2)
        {
            var p1 = GameColors.penYellow4;
            var p2 = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };

            g.DrawLine(p1, x1, y1, x2, y2);
            g.DrawLine(p2, x1, y1, x2, y2);
        }

        private void drawCircle(Graphics g, float x, float y, float r)
        {
            var p1 = GameColors.penYellow4;
            var p2 = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };

            var rect = new RectangleF(x - r, y - r, r * 2, r * 2);
            g.DrawEllipse(p1, rect);
            g.DrawEllipse(p2, rect);
        }

        private void drawAlphaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };

            var w = (this.Width / 2f);
            var h = (this.Height / 2f);

            // pit circles
            var x = w;
            var y = h - (h * 0.05f);
            drawCircle(g, x, y, h * 0.10f);
            drawCircle(g, x, y, h * 0.22f);
            drawCircle(g, x, y, h * 0.30f);

            // pit spike
            var d = h * 0.20f;

            x += d;
            y += d * 0.8f;
            drawLine(g, x, y, this.Width - 5, y);

            return;
            //rect.Inflate(40, 40);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

            //rect.Inflate(-d * 0.4f, -d * 0.4f);
            //g.DrawEllipse(GameColors.penYellow8, rect);
            //g.DrawEllipse(pp, rect);

        }

        private void drawBetaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 4) { DashStyle = DashStyle.Dot };

            var w = (this.Width / 2f);
            var h = (this.Height / 2f);

            //var d = 120f;
            //var x = w - (d / 2f);
            //var y = this.Height * 0.255f;
            //var rect = new RectangleF(x, y, d, d);
            //g.FillEllipse(Brushes.Yellow, rect);
            //g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);

            //rect.Inflate(-10, -10);
            //g.DrawEllipse(pp, rect);
            var er = Elite.getWindowRect();
            var ew = er.Width * 0.01f;
            var eh = er.Height * 0.01f;

            var x = w;
            var y = this.Height * 0.33f;
            drawCircle(g, x, y, 50);
            //drawCircle(g, x, y, 90);
            //drawCircle(g, x, y, 150);

            y += eh * 2.0f;
            drawLine(g, 10, y, w - 140, y);
            drawLine(g, w + 140, y, this.Width - 10, y);

            y += 20;
            drawLine(g, w, y, w, y + 240);

            //g.FillRectangle(Brushes.Yellow, w - 5, y, 10, 460);
            //g.DrawLine(pp, w, y + d, w, y + 456);
        }

        private void drawRelicTowerTarget(Graphics g)
        {
            var er = Elite.getWindowRect();
            var r = (float)er.Width / (float)er.Height;
            var ew = er.Width * 0.01f;
            var eh = er.Height * 0.01f;
            var pp = new Pen(Color.Black, 4) { DashStyle = DashStyle.Dash };

            var d = 0f;
            var x = 0f;
            var y = 0f;
            var w = (this.Width / 2f);
            var h = (this.Height / 2f);

            // rock marker
            x = w;
            y = this.Height * 0.65f;
            //drawLine(g, w, y, w, y + 100); // h - eh*12);

            // tops
            //*
            d = eh * 1.5f;
            x = w - d;
            y = h - eh * 4;
            drawLine(g, x, y, x - eh * 1f, y);
            drawLine(g, x, y + 100, x, y + 100 + eh * 20); // h - eh*12);

            x = w + d;
            drawLine(g, x, y, x + eh * 1f, y);
            drawLine(g, x, y + 100, x, y + 100 + eh * 20); // h - eh*12);


            //drawLine(g, w, 4, w, 500); // h - eh*12);
            //drawLine(g, 0, h, 1000, h);

            // rock marker
            //x = w - ew*7;
            //y = this.Height * 0.8f;
            //drawLine(g, x, this.Height * 0.7f, x, this.Height - 20);

            //x = w + ew * 5;
            //drawLine(g, x, h, x, h + eh * 20);

            //x = w - ew * 5;
            //y = this.Height * 0.6f;
            //drawCircle(g, x, y, 80);

            //x = w + ew * 1;
            //y = eh * 24;
            //drawCircle(g, x, y, 60);
        }

        private void drawRelicTowerTarget2(Graphics g)
        {
            var er = Elite.getWindowRect();
            var r = (float)er.Width / (float)er.Height;
            var ew = er.Width * 0.01f;
            var eh = er.Height * 0.01f;

            var d = 0f;
            var x = 0f;
            var y = 0f;

            var w = (this.Width / 2f);
            var h = (this.Height / 2f);
            //drawLine(g, w, 4, w, 500); // h - eh*12);
            //drawLine(g, 0, h, 1000, h);

            // rock marker
            x = w;
            y = this.Height * 0.8f;
            drawLine(g, w, this.Height * 0.7f, w, this.Height - 20); // h - eh*12);

            // horiz
            x = w;
            y = h + eh * 5;
            drawLine(g, x - 200, y, x + 200, y);

            // knob
            /*
            var d = eh * 1.9f;
            x = w - d;
            y = h;
            drawLine(g, x, y, x - eh * 0.9f, y + eh * 5);
            x = w + d + ew * 0.01f;
            drawLine(g, x, y, x + eh * 0.9f, y + eh * 5);
            */

            // tops
            /*
            d = eh * 2;
            x = w - d;
            drawLine(g, x, eh, x - eh * 0.2f, eh * 7);
            x = w + d;
            drawLine(g, x, eh, x + eh * 0.2f, eh * 7);
            // */


            // tops
            //*
            d = eh * 2;
            x = w - d;
            y = h - eh * 24;
            drawLine(g, x, y, x - eh * 2f, y);
            x = w + d;
            drawLine(g, x, y, x + eh * 2f, y);
            // */

            //x = w + d + eh * 10.5f;
            //drawLine(g, x, y, x - 8, y + eh * 5);


            //// upper
            //d = 122;
            //x = w - d;
            //y = h - 350;
            //drawLine(g, x, y, x-22, y + 420);
            //x = w + d;
            //drawLine(g, x, y, x+22, y + 420);

            //// lower
            //d = 192;
            //x = w - d;
            //y = h + 120;
            //drawLine(g, x, y, x - 16, y + 500);
            //x = w + d;
            //drawLine(g, x, y, x + 16, y + 500);

            // triangle
            //x = w - r * 54f;             y = h + r * 188f; // ratio
            //x = w - eh * 8f; y = h + eh * 23f; // eh only
            //x = w - eh * 6f; y = h + ew * 15f; // inverse
            x = w - ew * 3f; y = h + eh * 23f; // matching
            drawLine(g, x, y, x + eh * 10, y - eh * 5);
            drawLine(g, x, y, x + eh * 4f, y + eh * 2.8f);
            //x = w + 152;
            //drawLine(g, x, h + 40, x + 16, h + 440);

        }

        private void drawGammaSiteTarget(Graphics g)
        {
            var pp = new Pen(Color.Black, 4) { DashStyle = DashStyle.Dash };

            var w = (this.Width / 2f);
            var h = (this.Height / 2f);

            var x = w * 1.4f;
            var y = h * 0.535f;
            drawCircle(g, x, y, 30);

            drawLine(g, x - 30, y, x - w * 0.50f, y);
            drawLine(g, x + 22, y-4, x + 30, y-4);

            drawLine(g, x, y + 30, x, y + 50);
            drawLine(g, x - 19, y + 25, x - 30, y + 42);
            drawLine(g, x - 26, y + 12, x - 45, y + 25);

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

            //x = w * 1.23f;
            //y = h * 0.675f;
            //drawLine(g, x, y, x - w * 0.22f, y + h * 0.150f);

            //drawLine(g, x, y, x - 120, y + 140);

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
