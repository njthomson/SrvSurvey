using DecimalMath;
using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SrvSurvey
{
    public partial class PlotGrounded : Form, PlotterForm
    {
        private Game game = Game.activeGame!;

        private TrackingDelta? srvLocation;

        private TrackingDelta? td;

        private float scale;
        private float mw;
        private float mh;

        private PlotGrounded()
        {
            InitializeComponent();

            // TODO: use a size from some setting?
            //this.Height = 200;
            //this.Width = 200;

            this.scale = 0.25f;
            this.mw = this.Width / 2;
            this.mh = this.Height / 2;
            this.Cursor = Cursors.Cross;
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatRightMiddle(this, gameRect, 20);
        }

        private void PlotGrounded_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            game.status!.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode, true);
            this.Status_StatusChanged(false);

            game.journals!.onJournalEntry += Journals_onJournalEntry;
            game.nearBody!.bioScanEvent += NearBody_bioScanEvent;

            // get landing location
            Game.log($"initialize here: {Status.here}, touchdownLocation: {game.touchdownLocation}");

            if (game.touchdownLocation == null)
            {
                Game.log("Why no touchdownLocation?");
                return;
            }

            this.td = new TrackingDelta(
                game.nearBody.radius,
                game.touchdownLocation);
        }

        private void NearBody_bioScanEvent()
        {
            this.Invalidate();
        }

        #region journal events

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(Touchdown entry)
        {
            var newLocation = new LatLong2(entry);
            Game.log($"re-touchdownLocation: {newLocation}");

            if (this.td == null) return;

            this.td.Current = newLocation;
            this.td.Target = newLocation;

            this.Invalidate();
        }

        private void onJournalEntry(Disembark entry)
        {
            Game.log($"Disembark srvLocation {Status.here}");
            if (entry.SRV && this.srvLocation == null)
            {
                this.srvLocation = new TrackingDelta(game.nearBody!.radius, Status.here.clone());
                this.Invalidate();
            }
        }

        private void onJournalEntry(Embark entry)
        {
            Game.log($"Embark {Status.here}");
            if (entry.SRV && this.srvLocation != null)
            {
                this.srvLocation = null;
                this.Invalidate();
            }
        }

        #endregion

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.Opacity > 0 && !game.showBodyPlotters)
            {
                this.Opacity = 0;
            }
            else if (this.Opacity == 0 && game.showBodyPlotters)
            {
                this.reposition(Elite.getWindowRect());
            }
        }

        private void Status_StatusChanged(bool blink)
        {
            if (this.td != null)
                this.td.Current = Status.here;

            if (this.srvLocation != null)
                this.srvLocation.Current = Status.here;

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

        private void PlotGrounded_Paint(object sender, PaintEventArgs e)
        {
            if (game.nearBody == null) return;

            var g = e.Graphics;

            // draw basic compass cross hairs centered in the window
            this.drawCompassLines(g);

            this.drawBioScans(g);

            // draw touchdown marker
            if (this.td != null)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(mw, mh);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status!.Heading);

                const float touchdownSize = 64f;
                var rect = new RectangleF((float)td.dx - touchdownSize, (float)-td.dy - touchdownSize, touchdownSize * 2, touchdownSize * 2);
                var b = (game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty) ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(b, rect);
            }

            // draw SRV marker
            if (this.srvLocation != null)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(mw, mh);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status!.Heading);

                const float srvSize = 32f;
                var rect = new RectangleF((float)srvLocation.dx - srvSize, (float)-srvLocation.dy - srvSize, srvSize * 2, srvSize * 2);
                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }

            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mw, mh);
            g.RotateTransform(360 - game.status!.Heading);

            var locSz = 5f;
            g.DrawEllipse(GameColors.Lime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.Lime2, 0, 0, +dx, -dy);

            g.ResetTransform();

            if (this.td != null)
                this.drawBearingTo(g, 4, 8, "Touchdown:", this.td.Target);

            if (this.srvLocation != null)
                this.drawBearingTo(g, 4 + mw, 8, "SRV:", this.srvLocation.Target);

            float y = this.Height - 24;
            if (game.nearBody!.scanOne != null)
                this.drawBearingTo(g, 10, y, "Scan one:", game.nearBody.scanOne.location!);

            if (game.nearBody.scanTwo != null)
                this.drawBearingTo(g, 10 + mw, y, "Scan two:", game.nearBody.scanTwo.location!);
        }

        private void drawCompassLines(Graphics g)
        {
            const float pad = 8;

            g.ResetTransform();

            var r = new RectangleF(0, pad, this.Width, this.Height - pad * 2);
            g.Clip = new Region(r);
            g.TranslateTransform(mw, mh);

            // draw compass rose lines
            g.RotateTransform(360 - game.status!.Heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);
        }

        private void drawBioScans(Graphics g)
        {
            if (game.nearBody == null || this.td == null) return;

            // delta to ship
            g.ResetTransform();
            g.TranslateTransform(mw, mh);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(360 - game.status!.Heading);

            // use the same Tracking delta for all bioScans against the same currentLocation
            var currentLocation = new LatLong2(this.game.status);
            var d = new TrackingDelta(game.nearBody.radius, currentLocation.clone());
            foreach (var scan in game.nearBody.data.bioScans)
            {
                drawBioScan(g, d, scan);
            }

            if (game.nearBody.scanOne != null)
            {
                drawBioScan(g, d, game.nearBody.scanOne);
            }
            if (game.nearBody.scanTwo != null)
            {
                drawBioScan(g, d, game.nearBody.scanTwo);
            }
        }

        private void drawBioScan(Graphics g, TrackingDelta d, BioScan scan)
        {
            d.Target = scan.location!;

            Brush b;
            Pen p;
            if (scan.status == BioScan.Status.Complete)
            {
                // grey colours for completed scans
                b = GameColors.brushExclusionComplete;
                p = GameColors.penExclusionComplete;
            }
            else if (scan.status == BioScan.Status.Abandoned)
            {
                // blue colors for abandoned scans
                b = GameColors.brushExclusionAbandoned;
                p = GameColors.penExclusionAbandoned;
            }
            else if (d.distance < (decimal)scan.radius)
            {
                // red colours
                b = GameColors.brushExclusionDenied;
                p = GameColors.penExclusionDenied;
            }
            else
            {
                // green colours
                b = GameColors.brushExclusionActive;
                p = GameColors.penExclusionActive;
            }

            var fudge = 10;
            var radius = scan.radius * 1f;
            var rect = new RectangleF((float)d.dx - radius, (float)-d.dy - radius, radius * 2f, radius * 2f);

            rect.Inflate(-(fudge * 2), -(fudge * 2));
            g.FillEllipse(b, rect);
            rect.Inflate(fudge, fudge);
            g.DrawEllipse(p, rect);
            rect.Inflate(fudge, fudge);
            g.DrawEllipse(p, rect);
        }

        private void drawBearingTo(Graphics g, float x, float y, string txt, LatLong2 location)
        {
            //var txt = scan == game.nearBody.scanOne ? "Scan one:" : "Scan two:";
            g.DrawString(txt, Game.settings.fontSmall, GameColors.brushGameOrange, x, y);

            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);

            var sz = 6;
            x += txtSz.Width + 8;
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            var dd = new TrackingDelta(game.nearBody!.radius, location);

            Angle deg = dd.angle - game.status!.Heading;
            var dx = (float)Math.Sin(Util.degToRad(deg)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * 10F;
            g.DrawLine(GameColors.penGameOrange2, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += sz * 3;
            g.DrawString(Util.metersToString(dd.distance), Game.settings.fontSmall, GameColors.brushGameOrange, x, y);
        }
    }
}
