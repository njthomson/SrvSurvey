using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
using Res = Loc.PlotTrackTarget;

namespace SrvSurvey.plotters
{
    [ApproxSize(128, 108)]
    partial class PlotTrackTarget : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.activeGame?.systemBody != null
                && Game.activeGame.status != null
                && Game.activeGame.status.hasLatLong
                && !Game.activeGame.status.InTaxi
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel);
        }

        public LatLong2 targetLocation = Game.settings.targetLatLong;
        private decimal targetAngle;
        private decimal targetDistance;

        private TrackingDelta td; // TODO: Retire this
        private GraphicsPath pp, tt;

        private PlotTrackTarget()
        {
            this.Size = Size.Empty;

            this.plotPrep();
        }

        public override bool allow { get => PlotTrackTarget.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(128);
            this.Height = scaled(108);
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void initializeOnLoad()
        {
            base.initializeOnLoad();

            if (game.systemBody != null && game.status != null)
            {
                this.td = new TrackingDelta(game.systemBody.radius, Game.settings.targetLatLong);
                this.calculate();
            }
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            this.calculate();
        }

        public void calculate(LatLong2? newLocation = null)
        {
            // exit early if we're not in the right mode
            if (this.IsDisposed || game?.systemBody == null) return;

            if (newLocation != null)
                this.td = new TrackingDelta(game.systemBody.radius, newLocation);

            this.td?.calc(); // retire
            this.targetDistance = Util.getDistance(this.targetLocation, Status.here, game.systemBody.radius);
            this.targetAngle = Util.getBearing(this.targetLocation, Status.here);

            this.Invalidate();
        }

        private void plotPrep()
        {
            // prep for plotting
            var p1 = scaled(new Point(0, -50));

            pp = new GraphicsPath(FillMode.Alternate);
            pp.AddLine(p1, scaled(new Point(20, 30)));
            pp.AddLine(p1, scaled(new Point(-20, 30)));

            tt = new GraphicsPath(FillMode.Winding);
            tt.AddPolygon(new Point[] {
                scaled(new Point(0, -50)),
                scaled(new Point(18, 30)),
                scaled(new Point(-18,  30)),
            });
            tt.PathPoints[1].X += scaled(20);
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game.systemBody == null || this.td == null) return;
            //Game.log($"?? {this.Opacity}");

            float hh = this.Height / 2;

            // draw heading text (center bottom)
            var headingTxt = ((int)this.td.angle).ToString(); // TODO: this.targetAngle
                                                              //var headingTxt = this.targetAngle.ToString("N0");
            var sz = g.MeasureString(headingTxt, GameColors.fontSmall);
            var ty = this.Height - sz.Height - six;
            BaseWidget.renderText(g, Res.BearingTo.format(this.td.angle.ToString()), four, ty, GameColors.fontSmall);

            // draw distance text (center top)
            var dist = td.distance; // TODO: this.targetDist
                                    //var dist = this.targetDistance;
            if ((game.status.Flags & StatusFlags.AltitudeFromAverageRadius) > 0)
                dist += td.distance + game.status.Altitude; // Remove it? I don't think it helps any more

            var txt = Res.DistanceTo.format(Util.metersToString(dist));
            BaseWidget.renderText(g, txt, four, ten, GameColors.fontSmall);

            var gt = new GroundTarget(this.Font);
            gt.renderAngleOfAttack(g, ten, fourFour, game.status.PlanetRadius, this.targetLocation, Status.here);
        }
    }
}
