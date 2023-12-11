using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotFlightWarning : PlotBase, PlotterForm
    {
        private static int fromTop = 90;

        private PlotFlightWarning() : base()
        {
            this.Width = 0;
            this.Height = 0;
            this.Font = GameColors.fontSmall;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ??
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
            this.BackgroundImage = null;
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            Elite.floatCenterTop(this, gameRect, fromTop);

            this.Invalidate();
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var showPlotter = game.isMode(GameMode.Landed, GameMode.SuperCruising, GameMode.GlideMode, GameMode.Flying, GameMode.InFighter, GameMode.InSrv);
            if (this.Opacity > 0 && !showPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && showPlotter)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game.nearBody == null) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);
            g.Clear(Color.Black);

            const int pad = 14;

            var bodyGrav = (game.systemBody!.surfaceGravity / 10).ToString("N2");
            var txt = $"Warning: Surface gravity {bodyGrav}g";

            var sz = g.MeasureString(txt, this.Font);
            this.Width = (int)sz.Width + pad * 2;
            this.Height = (int)sz.Height + pad * 2;

            Elite.floatCenterTop(this, Elite.getWindowRect(), fromTop);

            var rect = new RectangleF(0, 0, sz.Width + pad * 2, sz.Height + pad * 2);
            g.FillRectangle(GameColors.brushShipDismissWarning, rect);

            rect.Inflate(-10, -10);
            g.FillRectangle(Brushes.Black, rect);
            g.DrawString(txt, this.Font, Brushes.Red, pad + 1, pad + 1);
        }
    }
}
