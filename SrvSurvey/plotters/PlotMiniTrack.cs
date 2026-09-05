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
            return (Game.settings.autoShowPlotMiniTrack || Game.settings.autoShowPlotMiniTrackRhino)
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && game.systemBody != null
                && game.status?.hasLatLong == true
                && (quickTrackers?.Count > 0 || inRhino)
                && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.RolePanel)
                ;
        }

        public static List<string> quickTrackers
        {
            get => Game.activeGame?.systemBody?.bookmarks?.Keys.Where(k => k[0] == '#').ToList() ?? new();
        }

        #endregion

        public static bool inRhino => Game.settings.autoShowPlotMiniTrackRhino && Game.activeGame?.vehicle == ActiveVehicle.SRV && Game.activeGame.lastLaunchSrv?.SRVType == "mev_rhino";

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
            if (bookmarks == null && !inRhino)
            {
                remove(def);
                return frame.Size;
            }

            var blockWidth = N.fiveTwo;
            var cmdr = Status.here;
            // offset to the center of the Rhino
            if (inRhino)
                cmdr = canonn.CanonnStation.adjustForCockpitOffset(game.status.PlanetRadius, cmdr, "MEV_Rhino", game.status.Heading);

            var radius = game.status.PlanetRadius;

            tt.dtx = N.eight;
            var keys = inRhino ? new [] { "#1", "#2", "#3", "#4", "#5", "#6" } : quickTrackers.Order().ToArray();
            foreach (var key in keys)
            {
                var x = tt.dtx;
                tt.dty = N.ten;

                if (inRhino && bookmarks?.ContainsKey(key) != true)
                {
                    // render an empty spot if in a Rhino and the bookmark is not set
                    tt.draw(x, key, C.orangeDarker);
                    var rect = new RectangleF(x + N.oneTwo, N.twoEight, N.ten * 2, N.ten * 2);
                    g.DrawEllipse(C.Pens.orangeDarker1, rect);
                    tt.dty = N.sixty;
                    tt.draw(x, "--", C.orangeDarker);
                }
                else
                {
                    tt.draw(x, key);

                    var target = bookmarks![key].First(); // we should only have 1 entry per quickTracker

                    // calculate as if a 2d plane
                    var angle2d = Util.getBearing(cmdr, target);
                    var dist2d = Util.getDistance(cmdr, target, radius);

                    tt.dty = N.sixty;
                    tt.draw(x, Util.metersToString(dist2d));

                    var deg = angle2d - game.status.Heading;
                    if (deg < 0) deg += 360;
                    if (dist2d == 0) deg += game.status.Heading;
                    var p = C.Pens.orange3r;
                    var b = C.Brushes.orange;
                    if (inRhino && dist2d < 5M)
                    {
                        // be blue if within pickup distance
                        p = C.Pens.cyan3r;
                        b = C.Brushes.cyan;
                    }
                    else if (inRhino && dist2d < 78M)
                    {
                        // be red if too close to deploy another rig
                        p = C.Pens.red3r;
                        b = C.Brushes.red;
                    }

                    BaseWidget.renderBearingTo(g, x + N.oneTwo, N.twoEight, N.ten, (double)deg, b, p);
                }

                tt.dtx = x + blockWidth;
            }

            // draw cargo capacity
            if (inRhino)
            {
                tt.newLine(+N.ten, true);
                var used = game.cargoFile.Count;
                tt.draw(N.ten, $"Cargo capacity: {used} of 72");
                var w = tt.containerWidth - N.six - tt.dtx - N.six;
                var r = new RectangleF(tt.dtx+ N.six, tt.dty, w / 72f * used, N.oneTwo);

                g.FillRectangle(C.Brushes.orangeDark, r);

                r.Width = w;
                r.Inflate(1f,1f);
                g.DrawRectangle(C.Pens.orange1, r);
                tt.dty += N.two;
            }

            tt.newLine(+N.six, true);
            return tt.pad(0, +N.four);
        }
    }
}
