using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotFlightWarning : PlotBase, PlotterForm
    {
        private PlotFlightWarning() : base()
        {
            this.Width = 0;
            this.Height = 0;
            this.Font = GameColors.fontSmall;
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

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

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

        int pad = scaled(15);

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            try
            {
                if (this.IsDisposed || game.systemBody == null)
                {
                    Program.closePlotter<PlotFlightWarning>();
                    return;
                }

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Black);


                var bodyGrav = (game.systemBody!.surfaceGravity / 10).ToString("N2");
                var txt = $"Warning: Surface gravity {bodyGrav}g";

                var sz = g.MeasureString(txt, this.Font);
                sz.Width += two;
                this.Width = scaled((int)sz.Width + pad * 2);
                this.Height = scaled((int)sz.Height + pad * 2);

                PlotPos.reposition(this, Elite.getWindowRect());

                var rect = new RectangleF(0, 0, sz.Width + pad * 2, sz.Height + pad * 2);
                g.FillRectangle(GameColors.brushShipDismissWarning, rect);

                rect.Inflate(-10, -10);
                g.FillRectangle(Brushes.Black, rect);
                g.DrawString(txt, this.Font, Brushes.Red, pad + one, pad + one);
            }
            catch (Exception ex)
            {
                Game.log($"PlotFlightWarning.OnPaintBackground error: {ex}");
            }
        }
    }
}
