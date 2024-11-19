using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
    [System.ComponentModel.DesignerCategory("")]
    internal class PlotVertialStripe : Form, PlotterForm, IDisposable
    {
        public static PlotVertialStripe? show(PlotVertialStripe.Mode mode, double targetAltitude)
        {
            if (Game.settings.disableAerialAlignmentGrid) return null;

            PlotVertialStripe.targetAltitude = targetAltitude;
            PlotVertialStripe.mode = mode;
            return Program.showPlotter<PlotVertialStripe>();
        }

        public enum Mode
        {
            Buttress,
            RelicTower,

            Alpha,
            Beta,
            Gamma,

            Bear,
            Bowl,
            Crossroads,
            Fistbump,
            Hammerbot,
            Lacrosse,
            Robolobster,
            Squid,
            Stickyhand,
            Turtle,
        }

        public static Mode mode;
        public static double targetAltitude;

        protected Game game = Game.activeGame!;
        public bool didFirstPaint { get; set; } = true;
        public bool showing { get; set; }

        private Rectangle er;
        private float mw;
        private float mh;

        private PlotVertialStripe()
        {
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

            this.Opacity = getOpacity() * Game.settings.Opacity;
            this.TopMost = true;

            this.BackColor = Color.Red;
            this.TransparencyKey = Color.Red;
            this.AllowTransparency = true;

            game.status.StatusChanged += Status_StatusChanged;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!this.showing || Elite.focusElite)
            {
                Elite.setFocusED();
            }
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
            this.Opacity = getOpacity() * Game.settings.Opacity;

            // float center, spanning whole height
            if (gameRect != Rectangle.Empty)
            {
                this.Width = 600;
                this.Left = gameRect.Left + (gameRect.Width / 2) - (this.Width / 2);

                this.Top = gameRect.Top;
                this.Height = gameRect.Height - 10;

                var mode = Elite.getGraphicsMode();
                if (mode == GraphicsMode.Windowed)
                {
                    // if window'd
                    this.Left += 8;
                }
            }
        }

        private void Status_StatusChanged(bool blink)
        {
            this.Opacity = getOpacity() * Game.settings.Opacity;

            this.Invalidate();
        }

        private double getOpacity()
        {
            if (Program.tempHideAllPlotters || !Elite.gameHasFocus)
                return 0;

            if (game.mode == GameMode.Landed)
                return 0;

            if (PlotVertialStripe.mode == Mode.RelicTower)
                return 0.8;

            var gameAltitude = Math.Max(0, game.status.Altitude);
            var delta = Math.Abs(gameAltitude - targetAltitude);
            if (delta > 220)
                return 0;
            else if (delta < 20)
                return 0.8;
            else
                return (220 - (delta)) / 200f;
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
            this.er = Elite.getWindowRect();
            this.mw = (this.Width / 2f);
            this.mh = (this.Height / 2f);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillRectangle(Brushes.Red, 0, 0, this.Width, this.Height);

            drawAngleString(g);

            switch (mode)
            {
                case Mode.Buttress: this.drawButtressTarget(g); break;
                case Mode.RelicTower: this.drawRelicTowerTarget(g); break;

                case Mode.Alpha: this.drawAlphaSiteTarget(g); break;
                case Mode.Beta: this.drawBetaSiteTarget(g); break;
                case Mode.Gamma: this.drawGammaSiteTarget(g); break;

                case Mode.Bear: this.drawBearTarget(g); break;
                case Mode.Bowl: this.drawBowlTarget(g); break;
                case Mode.Crossroads: this.drawCrossroadsTarget(g); break;
                case Mode.Fistbump: this.drawFistbumpTarget(g); break;
                case Mode.Hammerbot: this.drawHammerbotTarget(g); break;
                case Mode.Lacrosse: this.drawLacrosseTarget(g); break;
                case Mode.Robolobster: this.drawRobolobsterTarget(g); break;
                case Mode.Squid: this.drawSquidTarget(g); break;
                case Mode.Stickyhand: this.drawStickyhandTarget(g); break;
                case Mode.Turtle: this.drawTurtleTarget(g); break;

                default:
                    Game.log($"Unexpected mode: {mode}");
                    this.Opacity = 0;
                    break;
            }
        }

        private void drawAngleString(Graphics g)
        {
            var y = 1;
            g.FillRectangle(Brushes.Black, mw - 45, 5 + y, 85, 30);

            var txt = $"{new Angle(game.status.Heading)}";
            var sz = g.MeasureString(txt, GameColors.fontBig);

            var x = mw - (sz.Width / 2);
            g.DrawString(txt, GameColors.fontBig, Brushes.Yellow, x, y);
        }

        private void drawLine(Graphics g, float x1, float y1, float x2, float y2)
        {
            g.DrawLine(GameColors.penYellow4, x1, y1, x2, y2);
            g.DrawLine(GameColors.penBlack2Dash, x1, y1, x2, y2);
        }

        private void drawCircle(Graphics g, float x, float y, float r)
        {
            var rect = new RectangleF(x - r, y - r, r * 2, r * 2);
            g.DrawEllipse(GameColors.penYellow4, rect);
            g.DrawEllipse(GameColors.penBlack2Dash, rect);
        }

        private void drawAlphaSiteTarget(Graphics g)
        {
            // pit circles
            var y = mh - (mh * 0.05f);
            drawCircle(g, mw, y, mh * 0.10f);
            drawCircle(g, mw, y, mh * 0.22f);
            drawCircle(g, mw, y, mh * 0.30f);

            // pit spike
            var d = mh * 0.20f;
            y += d * 0.8f;
            drawLine(g, mw + d, y, this.Width - 5, y);
        }

        private void drawBetaSiteTarget(Graphics g)
        {
            var y = this.Height * 0.33f;
            drawCircle(g, mw, y, 50);

            var eh = er.Height * 0.01f;
            y += eh * 2.0f;
            drawLine(g, 10, y, mw - 140, y);
            drawLine(g, mw + 140, y, this.Width - 10, y);

            y += 20;
            drawLine(g, mw, y, mw, y + 240);
        }

        private void drawRelicTowerTarget(Graphics g)
        {
            var eh = er.Height * 0.01f;

            // tops
            //*
            var d = eh * 1.5f;
            var x = mw - d;
            var y = mh - eh * 4;
            drawLine(g, x, y, x - eh * 1f, y);
            drawLine(g, x, y + 100, x, y + 100 + eh * 20);

            x = mw + d;
            drawLine(g, x, y, x + eh * 1f, y);
            drawLine(g, x, y + 100, x, y + 100 + eh * 20);
        }

        private void drawGammaSiteTarget(Graphics g)
        {
            var x = mw * 1.4f;
            var y = mh * 0.535f;
            drawCircle(g, x, y, 30);

            drawLine(g, x - 30, y, x - mw * 0.50f, y);
            drawLine(g, x + 22, y - 4, x + 30, y - 4);

            drawLine(g, x, y + 30, x, y + 50);
            drawLine(g, x - 19, y + 25, x - 30, y + 42);
            drawLine(g, x - 26, y + 12, x - 45, y + 25);
        }

        private void drawButtressTarget(Graphics g)
        {
            var nightVision = true; // (game.status.Flags & StatusFlags.NightVision) == 0;

            // central thick line
            var h = mh * 0.8f;
            g.DrawLine(nightVision ? GameColors.penYellow8 : GameColors.penCyan8, mw, mh, mw, mh + h);
            g.DrawLine(GameColors.penBlack4Dash, mw, mh, mw, mh + h);

            // thinner lines
            var w1 = 40; // width one
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan4, mw - w1, mh + 40, mw - w1, mh + 220);
            g.DrawLine(GameColors.penBlack2Dash, mw - w1, mh + 40, mw - w1, mh + 220);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan4, mw + w1, mh + 40, mw + w1, mh + 220);
            g.DrawLine(GameColors.penBlack2Dash, mw + w1, mh + 40, mw + w1, mh + 220);

            var w2 = 70; // width two
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, mw - w2, mh + 60, mw - w2, mh + 200);
            g.DrawLine(GameColors.penBlack2Dash, mw - w2, mh + 60, mw - w2, mh + 200);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, mw + w2, mh + 60, mw + w2, mh + 200);
            g.DrawLine(GameColors.penBlack2Dash, mw + w2, mh + 60, mw + w2, mh + 200);

            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, mw - w2, mh + 80, mw + w2, mh + 80);
            g.DrawLine(GameColors.penBlack2Dash, mw - w2, mh + 80, mw + w2, mh + 80);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, mw - w2 - 50, mh + 130, mw + w2 + 50, mh + 130);
            g.DrawLine(GameColors.penBlack2Dash, mw - w2 - 50, mh + 130, mw + w2 + 50, mh + 130);
            g.DrawLine(nightVision ? GameColors.penYellow4 : GameColors.penCyan2, mw - w2, mh + 180, mw + w2, mh + 180);
            g.DrawLine(GameColors.penBlack2Dash, mw - w2, mh + 180, mw + w2, mh + 180);
        }

        private void drawRobolobsterTarget(Graphics g)
        {
            var eh = er.Height * 0.01f;
            var x = mw - 0;
            var y = mh - eh * 4;

            drawCircle(g, x, y, 80);
            drawCircle(g, x, y, 120);

            drawLine(g, x, 200, x, y + 200);
        }

        private void drawHammerbotTarget(Graphics g)
        {
            var ex = er.Width * 0.01f;

            // horiz
            drawLine(g, mw - ex, mh - 10, mw + ex, mh - 10);

            drawLine(g, mw - 6 * ex, mh + 10, mw - 4 * ex, mh + 10);
            drawLine(g, mw + 6 * ex, mh + 10, mw + 4 * ex, mh + 10);

            // vert
            drawLine(g, mw, mh + 100, mw, mh + 400);

            // others
            var ex2 = er.Width * 0.007f;
            var ey = er.Height * 0.03f;
            drawLine(g, mw - ex - ex2, mh + ey, mw - ex, mh + ey + ey);
            drawLine(g, mw + ex + ex2, mh + ey, mw + ex, mh + ey + ey);
        }

        private void drawBowlTarget(Graphics g)
        {
            // horiz
            var ey = er.Height * 0.1f;
            drawLine(g, 100, mh - ey, this.Width - 100, mh - ey);

            // vert
            drawLine(g, mw, 200, mw, this.Height - 100);

            // circles
            var ee = this.Height / 7f;
            drawCircle(g, mw, mh + ee, ee);
            drawCircle(g, mw, mh + ee, ee * 1.3f);
        }

        private void drawFistbumpTarget(Graphics g)
        {
            // diagonal cross
            var ey = er.Height * 0.1f;
            var d = 100;
            drawLine(g, mw - d, mh - ey - d, mw + d, mh - ey + d);
            drawLine(g, mw + d, mh - ey - d, mw - d, mh - ey + d);

            // vert
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }

        private void drawBearTarget(Graphics g)
        {
            // vert
            var d = 100;
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }

        private void drawCrossroadsTarget(Graphics g)
        {
            // vert
            var d = 100;
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }

        private void drawLacrosseTarget(Graphics g)
        {
            // vert
            var d = 100;
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }

        private void drawSquidTarget(Graphics g)
        {
            // vert
            var d = 100;
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }

        private void drawStickyhandTarget(Graphics g)
        {
            // vert
            var d = 100;
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }

        private void drawTurtleTarget(Graphics g)
        {
            // vert
            var d = 100;
            drawLine(g, mw, mh - 2 * d, mw, mh - 4 * d);
        }
    }
}
