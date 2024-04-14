using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotHumanSite : PlotBaseSite, PlotterForm
    {
        private PlotHumanSite() : base()
        {
            this.Width = scaled(300);
            this.Height = scaled(400);
            this.Font = GameColors.fontSmall;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
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

        public static bool keepPlotter
        {
            get => Game.activeGame?.status != null
                && Game.activeGame.status.hasLatLong
                && Game.activeGame.humanSite != null
                && Game.activeGame.humanSite.subType > 0;
        }

        public static bool allowPlotter
        {
            get => keepPlotter
                && Game.activeGame!.isMode(GameMode.OnFoot, GameMode.Flying, GameMode.Docked, GameMode.InSrv, GameMode.InTaxi, GameMode.Landed);
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && PlotHumanSite.keepPlotter)
                this.Opacity = 0;
            else if (this.Opacity > 0 && !PlotHumanSite.allowPlotter)
                Program.closePlotter<PlotHumanSite>(true);
            else if (this.Opacity == 0 && PlotHumanSite.keepPlotter)
                this.reposition(Elite.getWindowRect());
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;

            try
            {
                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                dtx = eight;
                dty = eight;
                this.drawTextAt("Hello world");
            }
            catch (Exception ex)
            {
                Game.log($"PlotGalMap.OnPaintBackground error: {ex}");
            }
        }
    }

}
