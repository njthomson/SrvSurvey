using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;


namespace SrvSurvey
{
    partial class PlotTrackTarget : PlotBase, PlotterForm
    {
        private TrackingDelta td;
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
            if (this.IsDisposed || game?.systemBody == null || !game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode))
                return;

            if (newLocation != null)
                this.td = new TrackingDelta(game.systemBody.radius, newLocation);

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

        int four = scaled(4);
        int six = scaled(6);
        int ten = scaled(10);

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            try
            {
                if (this.IsDisposed || this.td == null) return;

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                float w = this.Width / 2;
                float h = this.Height / 2;


                // draw heading text (center bottom)
                var headingTxt = ((int)this.td.angle).ToString();
                var sz = g.MeasureString(headingTxt, GameColors.fontSmall);
                var tx = w - (sz.Width / 2);
                var ty = this.Height - sz.Height - six;
                g.DrawString(this.td.angle.ToString(), GameColors.fontSmall, Brushes.Orange, tx, ty);

                // draw distance text (top left corner
                var dist = td.distance;
                if ((game.status.Flags & StatusFlags.AltitudeFromAverageRadius) > 0)
                    dist += td.distance + game.status.Altitude;
                var txt = "Distance: " + Util.metersToString(dist);
                g.DrawString(txt, GameColors.fontSmall, Brushes.Orange, four, ten);

                g.TranslateTransform(w, h);

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
