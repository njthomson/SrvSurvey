using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private Game game = Game.activeGame;

        private TrackingDelta srvLocation;

        private TrackingDelta td;

        private PlotGrounded()
        {
            InitializeComponent();

            // TODO: use a size from some setting?
            //this.Height = 200;
            //this.Width = 200;
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatRightMiddle(this, 20);
        }

        private void PlotGrounded_Load(object sender, EventArgs e)
        {
            this.initialize();
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            game.status.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode);
            this.Status_StatusChanged();

            game.journals.onJournalEntry += Journals_onJournalEntry;
            game.nearBody.bioScanEvent += NearBody_bioScanEvent;

            this.reposition(Elite.getWindowRect());

            // get landing location
            Game.log($"initialize here: {game.status.here}, touchdownLocation: {game.touchdownLocation}");

            if (game.touchdownLocation == null)
            {
                Game.log("Why no touchdownLocation?");
                return;
            }

            this.td = new TrackingDelta(
                game.nearBody.radius,
                game.status.here,
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

            this.td.Point1 = newLocation;
            this.td.Point2 = newLocation;

            this.Invalidate();
        }

        private void onJournalEntry(Disembark entry)
        {
            Game.log($"Disembark srvLocation {game.status.here}");
            if (entry.SRV && this.srvLocation == null)
            {
                this.srvLocation = new TrackingDelta(game.nearBody.radius, game.status.here, new LatLong2(game.status));
                this.Invalidate();
            }
        }

        private void onJournalEntry(Embark entry)
        {
            Game.log($"Embark {game.status.here}");
            if (entry.SRV && this.srvLocation != null)
            {
                this.srvLocation = null;
                this.Invalidate();
            }
        }

        #endregion

        private void Game_modeChanged(GameMode newMode)
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

        private void Status_StatusChanged()
        {
            if (this.td != null)
                this.td.Point1 = game.status.here;

            if (this.srvLocation != null)
                this.srvLocation.Point1 = game.status.here;

            this.Invalidate();
        }

        private void PlotGrounded_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void PlotGrounded_DoubleClick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void PlotGrounded_Click(object sender, EventArgs e)
        {
            Elite.setFocusED();
        }

        private void PlotGrounded_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            const float scale = 0.25f;

            // draw basic compass cross hairs centered in the window
            float w = this.Width / 2;
            float h = this.Height / 2;

            this.drawCompassLines(g);

            this.drawBioScans(g, scale);

            // draw touchdown marker
            if (this.td != null)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(w, h);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status.Heading);

                const float touchdownSize = 64f;
                var rect = new RectangleF((float)td.dx - touchdownSize, (float)-td.dy - touchdownSize, touchdownSize * 2, touchdownSize * 2);
                var b = game.touchdownLocation == null ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(b, rect);
            }

            // draw SRV marker
            if (this.srvLocation != null)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(w, h);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status.Heading);

                const float srvSize = 32f;
                var rect = new RectangleF((float)srvLocation.dx - srvSize, (float)-srvLocation.dy - srvSize, srvSize * 2, srvSize * 2);
                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }

            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(w, h);
            g.RotateTransform(360 - game.status.Heading);

            var locSz = 5f;
            g.DrawEllipse(GameColors.Lime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.Lime2, 0, 0, +dx, -dy);

            //drawScale(g, scale);
            g.ResetTransform();

            if (this.td != null)
                this.drawBearingTo(g, 4, 8, "Touchdown:", this.td.Point2);

            if (this.srvLocation != null)
                this.drawBearingTo(g, 4 + w, 8, "SRV:", this.srvLocation.Point2);

            float y = this.Height - 24;
            if (game.nearBody.scanOne != null)
                this.drawBearingTo(g, 10, y, "Scan one:", game.nearBody.scanOne.location);

            if (game.nearBody.scanTwo != null)
                this.drawBearingTo(g, 10 + w, y, "Scan two:", game.nearBody.scanTwo.location);
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

            var dd = new TrackingDelta(game.nearBody.radius, game.status.here, location);

            Angle deg = dd.angle - game.status.Heading;
            var dx = (float)Math.Sin(Util.degToRad(deg)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * 10F;
            g.DrawLine(GameColors.penGameOrange2, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += sz * 3;
            g.DrawString(Util.metersToString(dd.distance), Game.settings.fontSmall, GameColors.brushGameOrange, x, y);
        }

        private void drawCompassLines(Graphics g)
        {
            g.ResetTransform();
            const float pad = 8;

            float w = this.Width / 2;
            float h = this.Height / 2;
            var r = new RectangleF(0, pad, this.Width, this.Height - pad * 2);
            g.Clip = new Region(r);
            g.TranslateTransform(w, h);

            // draw compass rose lines
            g.RotateTransform(360 - game.status.Heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);
        }

        private void drawBioScans(Graphics g, float scale)
        {
            if (game.nearBody == null || this.td == null) return;

            float w = this.Width / 2;
            float h = this.Height / 2;

            // delta to ship
            g.ResetTransform();
            g.TranslateTransform(w, h);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(360 - game.status.Heading);

            // use the same Tracking delta for all bioScans against the same currentLocation
            var currentLocation = new LatLong2(this.game.status);
            var d = new TrackingDelta(game.nearBody.radius, currentLocation, currentLocation);
            foreach (var scan in game.nearBody.completedScans)
            {
                drawBioScan(g, d, scan);
            }

            var txt = $"Distances: {Util.metersToString(this.td.distance)}";

            if (game.nearBody.scanOne != null)
            {
                drawBioScan(g, d, game.nearBody.scanOne);
                txt += $" / {Util.metersToString(d.distance)}";
            }
            if (game.nearBody.scanTwo != null)
            {
                drawBioScan(g, d, game.nearBody.scanTwo);
                txt += $" / {Util.metersToString(d.distance)}";
            }

            //// draw distances along the top
            //g.ResetTransform();
            //var txtSz = g.MeasureString(txt, this.font);
            //var r = new RectangleF(8, 8, this.Width - 20, txtSz.Height);
            //g.DrawString(txt, this.font, GameColors.brushGameOrange, r, StringFormat.GenericTypographic);;
        }

        private void drawBioScan(Graphics g, TrackingDelta d, BioScan scan)
        {
            var fudge = 10;

            d.Point2 = scan.location;
            var rect = new RectangleF((float)d.dx - scan.radius, (float)-d.dy - scan.radius, scan.radius * 2, scan.radius * 2);
            //Game.log($"d.dx: {rect.X}, d.dy: {rect.Y}");

            var complete = scan.scanType == ScanType.Analyse;
            g.FillEllipse(complete ? GameColors.brushExclusionComplete : GameColors.brushExclusionActive, rect);
            rect.Inflate(fudge, fudge);
            g.DrawEllipse(complete ? GameColors.penExclusionComplete : GameColors.penExclusionActive, rect);
            rect.Inflate(fudge, fudge);
            g.DrawEllipse(complete ? GameColors.penExclusionComplete : GameColors.penExclusionActive, rect);
        }

        private void drawScale(Graphics g, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            float dist = game.nearBody.scanOne?.radius ?? 100;

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);

            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height;

            g.DrawString(
                txt, 
                Game.settings.fontSmall,
                GameColors.brushGameOrange,
                x, //this.Width - pad - txtSz.Width, // x
                y, //this.Height - pad - txtSz.Height, // y
                StringFormat.GenericTypographic);

            x -= pad;
            y += pad - 2;
            dist *= scale;
            g.DrawLine(GameColors.penGameOrange2, x, y, x - dist, y);
            g.DrawLine(GameColors.penGameOrange2, x, y - 4, x, y + 4);
            g.DrawLine(GameColors.penGameOrange2, x - dist, y - 4, x - dist, y + 4);
        }
    }
}
