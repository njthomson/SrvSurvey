using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
// Currently, this does not require any localization

namespace SrvSurvey.plotters
{
    internal class PlotMiniTrack : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotMiniTrack),
            allowed = allowed,
            ctor = (game, def) => new PlotMiniTrack(game, def),
            defaultSize = new Size(240, 80), // Not 100, 80?
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotMiniTrack_TEST
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && game.systemBody != null
                && game.status.hasLatLong
                && quickTrackers?.Any() == true
                && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.RolePanel)
                ;
        }

        private static PenBrush pb = new PenBrush(C.orange, 3, LineCap.Flat);

        public static List<string> quickTrackers
        {
            get => Game.activeGame?.systemBody?.bookmarks?.Keys.Where(k => k[0] == '#').ToList() ?? new();
        }

        #endregion

        private PlotMiniTrack(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.console_8;
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (status.changed.Contains(nameof(Status.Heading)) || status.changed.Contains(nameof(Status.Latitude)) || status.changed.Contains(nameof(Status.Longitude)))
                this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var bookmarks = game.systemBody?.bookmarks;
            if (bookmarks == null)
            {
                remove(def);
                return frame.Size;
            }

            var blockWidth = N.fiveTwo;
            var cmdr = Status.here;
            var radius = game.status.PlanetRadius;

            tt.dtx = N.eight;
            foreach (var key in quickTrackers.Order())
            {
                var list = bookmarks[key];
                foreach (var target in list)
                {
                    var x = tt.dtx;
                    // calculate as if a 2d plane
                    var angle2d = Util.getBearing(cmdr, target);
                    var dist2d = Util.getDistance(cmdr, target, radius);

                    tt.dty = N.ten;
                    tt.draw(x, key);

                    tt.dty = N.sixty;
                    tt.draw(x , Util.metersToString(dist2d));

                    var deg = angle2d - game.status.Heading;
                    if (deg < 0) deg += 360;
                    if (dist2d == 0) deg += game.status.Heading;
                    BaseWidget.renderBearingTo(g, x + N.oneTwo, N.twoEight, N.ten, (double)deg, pb.brush, pb.pen);

                    tt.dtx = x + blockWidth;
                }
            }

            tt.newLine(+N.four, true);
            return tt.pad(0, +N.four);
        }
    }
}
