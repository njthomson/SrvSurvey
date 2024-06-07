using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;


namespace SrvSurvey
{
    partial class PlotTrackTarget : PlotBase, PlotterForm
    {
        public LatLong2 targetLocation = Game.settings.targetLatLong;
        private decimal targetAngle;
        private decimal targetDistance;

        private TrackingDelta td; // TODO: Retire this
        private GraphicsPath pp, tt;

        private PlotTrackTarget()
        {
            this.Width = scaled(120);
            this.Height = scaled(108);

            this.plotPrep();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void initialize()
        {
            base.initialize();

            if (game.systemBody != null && game.status != null)
            {
                this.td = new TrackingDelta(game.systemBody.radius, Game.settings.targetLatLong);
                this.calculate();
            }
        }

        public static bool allowPlotter
        {
            get => Game.activeGame?.systemBody != null
                && !Game.activeGame.isShutdown // not needed?
                && !Game.activeGame.atMainMenu // not needed?
                && Game.activeGame.status != null
                && Game.activeGame.status.hasLatLong
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.status.OnFootSocial
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel);
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

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Debugger.IsAttached)
            {
                // use a different cursor if debugging
                this.Cursor = Cursors.No;
            }
            else if (Game.settings.hideOverlaysFromMouse)
            {
                // move the mouse outside the overlay
                System.Windows.Forms.Cursor.Position = Elite.gameCenter;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        #endregion

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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            try
            {
                if (this.IsDisposed || game.systemBody == null || this.td == null) return;

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                float hw = this.Width / 2;
                float hh = this.Height / 2;

                // draw heading text (center bottom)
                var headingTxt = ((int)this.td.angle).ToString(); // TODO: this.targetAngle
                //var headingTxt = this.targetAngle.ToString("N0");
                var sz = g.MeasureString(headingTxt, GameColors.fontSmall);
                var tx = hw - (sz.Width / 2);
                var ty = this.Height - sz.Height - six;
                g.DrawString(this.td.angle.ToString(), GameColors.fontSmall, Brushes.Orange, tx, ty);

                // draw distance text (center top)
                var dist = td.distance; // TODO: this.targetDist
                //var dist = this.targetDistance;
                if ((game.status.Flags & StatusFlags.AltitudeFromAverageRadius) > 0)
                    dist += td.distance + game.status.Altitude; // Remove it? I don't think it helps any more

                var txt = "Distance: " + Util.metersToString(dist); // dist.ToString("N2");
                g.DrawString(txt, GameColors.fontSmall, Brushes.Orange, four, ten);

                g.TranslateTransform(hw, hh);

                // rotate so the arrow aligns "up" is forwards/in front of us
                var da = (int)td.angle - game.status.Heading;
                g.RotateTransform(da);
                g.ScaleTransform(0.3F, 0.3F);

                // clip to prevent filling
                float fill = (float)(td.complete);
                var clipR = scaled(new RectangleF(-100, -60, 200, 100 - fill));
                g.Clip = new Region(clipR);
                g.FillPath(Brushes.Yellow, tt);

                // draw thd rest unclipped
                g.Clip = new Region();
                g.DrawPath(GameColors.penGameOrange8, pp);
            }
            catch (Exception ex)
            {
                Game.log($"PlotTrackTarget.OnPaintBackground error: {ex}");
            }
        }
    }
}
