using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
    internal class PlotMiniTrack : PlotBase, PlotterForm
    {
        private PenBrush pb = new PenBrush(C.orange, 3, LineCap.Flat);

        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotMiniTrack_TEST
                && Game.activeGame?.systemBody != null
                && Game.activeGame.status.hasLatLong
                && !Game.activeGame.status.Docked
                && hasQuickTrackers
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.RolePanel)
                ;
        }

        public static bool hasQuickTrackers
        {
            get => quickTrackers?.Any() ?? false;
        }

        public static List<string> quickTrackers
        {
            get => Game.activeGame?.systemBody?.bookmarks?.Keys.Where(k => k[0] == '#').ToList() ?? new();
        }

        private PlotMiniTrack() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.console_8;
        }

        public override bool allow { get => PlotMiniTrack.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(100);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            var bookmarks = game.systemBody?.bookmarks;
            if (this.IsDisposed || bookmarks == null) return;

            if (!hasQuickTrackers)
            {
                Program.closePlotter<PlotMiniTrack>();
                return;
            }

            var blockWidth = fifty;
            var cmdr = Status.here;
            var radius = game.status.PlanetRadius;

            dtx = eight;
            foreach (var key in quickTrackers.Order())
            {
                var list = bookmarks[key];
                foreach (var target in list)
                {
                    var x = dtx;
                    // calculate as if a 2d plane
                    var angle2d = Util.getBearing(cmdr, target);
                    var dist2d = Util.getDistance(cmdr, target, radius);

                    dty = ten;
                    drawTextAt(x, key);

                    dty = sixty;
                    drawTextAt(x , Util.metersToString(dist2d));

                    var deg = angle2d - game.status.Heading;
                    if (deg < 0) deg += 360;
                    if (dist2d == 0) deg += game.status.Heading;
                    BaseWidget.renderBearingTo(g, x + oneTwo, twoEight, ten, (double)deg, this.Font, pb.brush, pb.pen);

                    dtx = x + blockWidth;
                }
            }

            newLine(+four, true);
            this.formAdjustSize(0, +four);
        }
    }
}
