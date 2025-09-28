using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
using Res = Loc.PlotTrackTarget;

namespace SrvSurvey.plotters
{
    partial class PlotTrackTarget : PlotBase2
    {
        #region def + statics

        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotTrackTarget),
            allowed = allowed,
            ctor = (game, def) => new PlotTrackTarget(game, def),
            defaultSize = new Size(128, 108),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.targetLatLongActive
                && !game.atMainMenu
                && game.systemBody != null
                && game.status != null
                && game.status.hasLatLong
                && !game.status.InTaxi
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && !game.hidePlottersFromCombatSuits
                && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel);
        }

        #endregion

        public LatLong2 targetLocation = Game.settings.targetLatLong;
        private decimal targetAngle;
        private decimal targetDistance;

        private TrackingDelta td; // TODO: Retire this

        private PlotTrackTarget(Game game, PlotDef def) : base(game, def)
        {
            if (game.systemBody != null && game.status != null)
            {
                this.td = new TrackingDelta(game.systemBody.radius, Game.settings.targetLatLong);
                this.calculate();
            }
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (status.changed.Contains(nameof(Status.Heading)) || status.changed.Contains(nameof(Status.Latitude)) || status.changed.Contains(nameof(Status.Longitude)))
                this.calculate();
        }

        public void calculate(LatLong2? newLocation = null)
        {
            // exit early if we're not in the right mode
            if (game?.systemBody == null) return;

            if (newLocation != null)
                this.td = new TrackingDelta(game.systemBody.radius, newLocation);

            this.td?.calc(); // retire
            this.targetDistance = Util.getDistance(this.targetLocation, Status.here, game.systemBody.radius);
            this.targetAngle = Util.getBearing(this.targetLocation, Status.here);

            this.invalidate();
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (game.systemBody == null || this.td == null) return frame.Size;
            //Game.log($"?? {this.Opacity}");

            float hh = this.height / 2;

            // draw heading text (center bottom)
            var headingTxt = ((int)this.td.angle).ToString(); // TODO: this.targetAngle
                                                              //var headingTxt = this.targetAngle.ToString("N0");
            var sz = g.MeasureString(headingTxt, GameColors.fontSmall);
            var ty = this.height - sz.Height - N.six;
            BaseWidget.renderText(g, Res.BearingTo.format(this.td.angle.ToString()), N.four, ty, GameColors.fontSmall);

            // draw distance text (center top)
            var dist = td.distance; // TODO: this.targetDist
                                    //var dist = this.targetDistance;
            if ((game.status.Flags & StatusFlags.AltitudeFromAverageRadius) > 0)
                dist += td.distance + game.status.Altitude; // Remove it? I don't think it helps any more

            var txt = Res.DistanceTo.format(Util.metersToString(dist));
            BaseWidget.renderText(g, txt, N.four, N.ten, GameColors.fontSmall);

            var gt = new GroundTarget(this.font);
            gt.renderAngleOfAttack(g, N.ten, N.fourFour, game.status.PlanetRadius, this.targetLocation, Status.here, true);

            // fixed size
            return plotDef.defaultSize;
        }
    }
}
