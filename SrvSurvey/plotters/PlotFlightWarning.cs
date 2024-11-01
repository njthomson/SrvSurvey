using SrvSurvey.game;
using SrvSurvey.Properties;

namespace SrvSurvey
{
    internal class PlotFlightWarning : PlotBase, PlotterForm
    {
        private PlotFlightWarning() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall;
        }

        public override bool allow { get => PlotFlightWarning.allowPlotter; }

        public static bool allowPlotter
        {
            get => Game.settings.autoShowFlightWarnings
                && Game.activeGame?.systemBody != null
                && Game.activeGame.systemBody.type == SystemBodyType.LandableBody
                && Game.activeGame.systemBody.surfaceGravity >= Game.settings.highGravityWarningLevel * 10
                && Game.activeGame.isMode(GameMode.Landed, GameMode.SuperCruising, GameMode.GlideMode, GameMode.Flying, GameMode.InFighter, GameMode.InSrv)
                ;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
            this.BackgroundImage = null;
        }

        float pad = scaled(15);

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || game.systemBody == null)
            {
                Program.closePlotter<PlotFlightWarning>();
                return;
            }

            var bodyGrav = (game.systemBody!.surfaceGravity / 10).ToString("N2");
            var txt = Misc.PlotFlightWarning_SurfaceGravityWarning.format(bodyGrav);

            var sz = g.MeasureString(txt, this.Font);
            sz.Width += two;
            this.Width = (int)(sz.Width + pad * 2);
            this.Height = (int)(sz.Height + pad * 2);

            PlotPos.reposition(this, Elite.getWindowRect());

            var rect = new RectangleF(0, 0, sz.Width + pad * 2, sz.Height + pad * 2);
            g.FillRectangle(GameColors.brushShipDismissWarning, rect);

            rect.Inflate(-10, -10);
            g.FillRectangle(Brushes.Black, rect);
            g.DrawString(txt, this.Font, Brushes.Red, pad + one, pad + one);
        }
    }
}
