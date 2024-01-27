using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SrvSurvey
{
    partial class PlotTrackTarget : Form, PlotterForm
    {
        private Game game = Game.activeGame!;

        private TrackingDelta td;

        private PlotTrackTarget()
        {
            InitializeComponent();
            this.TopMost = true;

            this.Height = 108;
            this.Width = 100;

            this.Cursor = Cursors.Cross;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Elite.setFocusED();
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

        }

        private void PlotGroundTarget_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        private void initialize()
        {
            if (game.systemBody == null || game.status == null) return;

            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
            this.td = new TrackingDelta(game.systemBody.radius, Game.settings.targetLatLong);

            game.status.StatusChanged += Status_StatusChanged;
            Game.update += Game_modeChanged;

            this.plotPrep();

            this.calculate();
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

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            //Game.log($"?? {game.vehicle} / {newMode} / {game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode)}");
            // Landed / OnFoot / InSrv
            
            if (this.Opacity == 0 && game.showBodyPlotters)
            {
                this.reposition(Elite.getWindowRect());
            }
            else if (this.Opacity != 0 && !game.showBodyPlotters)
            {
                this.Opacity = 0;
            }
            else if (game.isMode(GameMode.InSrv, GameMode.OnFoot))
            {
                // force plotter to reposition when switching between SRV and foot.
                this.reposition(Elite.getWindowRect());
            }

            //if (game.nearBody != null && game.mode != GameMode.SuperCruising && game.mode != GameMode.Flying && game.mode != GameMode.Landed && game.mode != GameMode.InSrv && game.mode != GameMode.OnFoot && game.mode != GameMode.GlideMode)
            //    this.Opacity = 0;
            //else
            //    this.reposition(Overlay.getEDWindowRect());
        }

        private void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            this.calculate();
        }

        public void calculate(LatLong2? newLocation = null)
        {
            // exit early if we're not in the right mode
            if (game.systemBody != null && game.mode != GameMode.SuperCruising && game.mode != GameMode.Flying && game.mode != GameMode.Landed && game.mode != GameMode.InSrv && game.mode != GameMode.OnFoot && game.mode != GameMode.GlideMode)
                return;

            if (newLocation != null)
                this.td = new TrackingDelta(game.systemBody.radius, newLocation);

            this.td.Current = Status.here;
            this.Invalidate();
        }

        private void plotPrep()
        {
            // prep for plotting
            var p1 = new Point(0, -50);

            pp = new GraphicsPath(FillMode.Alternate);
            pp.AddLine(p1, new Point(20, 30));
            pp.AddLine(p1, new Point(-20, 30));

            tt = new GraphicsPath(FillMode.Winding);
            tt.AddPolygon(new Point[] {
                new Point(0, -50),
                new Point(18, 30),
                new Point(-18,  30),
            });
            tt.PathPoints[1].X += 20;
        }

        GraphicsPath pp;
        GraphicsPath tt;

        private static Pen pen1 = new Pen(Color.FromArgb(255, 249, 165, 0), 3);
        private void plotPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            float w = this.Width / 2;
            float h = this.Height / 2;

            // draw heading text (center bottom)
            var headingTxt = ((int)this.td.angle).ToString();
            var sz = g.MeasureString(headingTxt, GameColors.fontSmall);
            var tx = w - (sz.Width / 2);
            var ty = this.Height - sz.Height - 6;
            g.DrawString(this.td.angle.ToString(), GameColors.fontSmall, Brushes.Orange, tx, ty);

            // draw distance text (top left corner
            var dist = td.distance;
            if ((game.status.Flags & StatusFlags.AltitudeFromAverageRadius) > 0)
                dist += td.distance + game.status.Altitude;
            var txt = "Distance: " + Util.metersToString(dist);
            g.DrawString(txt, GameColors.fontSmall, Brushes.Orange, 4, 10);

            // assuming panel is 200 x 200
            g.TranslateTransform(w, h);

            // rotate so the arrow aligns "up" is forwards/infront of us
            var da =  (int)td.angle - game.status.Heading;
            g.RotateTransform(da);
            g.ScaleTransform(0.3F, 0.3F);

            // clip to prevent filling
            float fill = (float)(td.complete);
            var clipR = new RectangleF(-100, -60, 200, 100 - fill);
            g.Clip = new Region(clipR);
            g.FillPath(Brushes.Yellow, tt);

            // draw thd rest unclipped
            g.Clip = new Region();
            g.DrawPath(GameColors.penGameOrange8, pp);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
