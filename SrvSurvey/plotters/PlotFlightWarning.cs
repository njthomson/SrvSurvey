using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotFlightWarning;

namespace SrvSurvey.plotters
{
    internal class PlotFlightWarning : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotFlightWarning),
            allowed = allowed,
            ctor = (game, def) => new PlotFlightWarning(game, def),
            defaultSize = new Size(300, 80),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowFlightWarnings
                && game.systemBody != null
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && game.systemBody.type == SystemBodyType.LandableBody
                && game.systemBody.surfaceGravity >= Game.settings.highGravityWarningLevel * 10
                && game.isMode(GameMode.Landed, GameMode.SuperCruising, GameMode.GlideMode, GameMode.Flying, GameMode.InFighter, GameMode.InSrv)
                //&& (!Game.settings.autoShowPlotJumpInfo || !PlotJumpInfo.allowed(game)) // hide if PlotJumpInfo is showing? Maybe not
                ;
        }

        #endregion

        private PlotFlightWarning(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;
        }

        float pad = N.oneFive;

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var bodyGrav = (game.systemBody!.surfaceGravity / 10).ToString("N2");
            var txt = Res.SurfaceGravityWarning.format(bodyGrav);

            var sz = new SizeF(TextRenderer.MeasureText(g, txt, this.font));
            sz.Width += pad + N.ten;
            sz.Height += pad * 2;

            var rect = new RectangleF(Point.Empty, sz);
            g.FillRectangle(GameColors.brushShipDismissWarning, rect);

            rect.Inflate(-N.ten, -N.ten);
            g.FillRectangle(Brushes.Black, rect);

            tt.dtx = pad + N.one;
            tt.dty = pad + N.one;
            tt.draw(txt, C.red);

            return sz;
        }
    }
}
