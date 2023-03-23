using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    internal abstract class PlotBase : Form, PlotterForm
    {
        protected Game game = Game.activeGame!;
        protected TrackingDelta touchdownLocation;
        protected TrackingDelta? srvLocation;
        protected Size mid;
        protected Graphics? g;
        protected float scale = 0.25f;

        protected PlotBase()
        {
            this.TopMost = true;

            this.touchdownLocation = new TrackingDelta(
                game.nearBody!.radius,
                game.status!.here,
                game.touchdownLocation);
        }
        public abstract void reposition(Rectangle gameRect);

        protected virtual void initialize()
        {
            this.mid = this.Size / 2;

            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            game.modeChanged += Game_modeChanged;
            game.status!.StatusChanged += Status_StatusChanged;
            game.journals!.onJournalEntry += Journals_onJournalEntry;
            //game.nearBody!.bioScanEvent += NearBody_bioScanEvent;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode);
            this.Status_StatusChanged();
        }

        protected virtual void Game_modeChanged(GameMode newMode)
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

        protected virtual void Status_StatusChanged()
        {
            this.touchdownLocation.Point1 = game.status!.here;

            if (this.srvLocation != null)
                this.srvLocation.Point1 = game.status!.here;

            this.Invalidate();
        }

        protected void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        protected void onJournalEntry(JournalEntry entry) { /* ignore */ }

        protected void onJournalEntry(Disembark entry)
        {
            Game.log($"Disembark srvLocation {game.status!.here}");
            if (entry.SRV && this.srvLocation == null)
            {
                this.srvLocation = new TrackingDelta(game.nearBody!.radius, game.status.here, new LatLong2(game.status));
                this.Invalidate();
            }
        }

        protected void onJournalEntry(Embark entry)
        {
            Game.log($"Embark {game.status!.here}");
            if (entry.SRV && this.srvLocation != null)
            {
                this.srvLocation = null;
                this.Invalidate();
            }
        }

        protected virtual void onJournalEntry(SendText entry)
        {
            // overridden
        }

        protected void drawCommander()
        {
            if (g == null) return;

            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.RotateTransform(360 - game.status!.Heading);

            var locSz = 5f;
            g.DrawEllipse(GameColors.Lime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.Lime2, 0, 0, +dx, -dy);
        }

        protected void clipToMiddle(float left, float top, float right, float bottom)
        {
            if (g == null) return;

            var r = new RectangleF(left, top, this.Width - left - right, this.Height - top - bottom);
            g.Clip = new Region(r);
        }

        protected void drawCompassLines()
        {
            if (g == null) return;

            g.ResetTransform();
            this.clipToMiddle(0, 8, 0, 8);
            g.TranslateTransform(mid.Width, mid.Height);

            // draw compass rose lines
            g.RotateTransform(360 - game.status!.Heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);
        }

        protected void drawTouchdownAndSrvLocation(float dx = 0, float dy = 0)
        {
            if (g==null || (this.touchdownLocation == null && this.srvLocation == null)) return;

            g.ResetTransform();
            g.TranslateTransform(mid.Width + dx, mid.Height + dy);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(360 - game.status!.Heading);

            // draw touchdown marker
            if (this.touchdownLocation != null)
            {
                const float touchdownSize = 24f; // 64f;
                var rect = new RectangleF(
                    (float)touchdownLocation.dx - touchdownSize,
                    (float)-touchdownLocation.dy - touchdownSize,
                    touchdownSize * 2,
                    touchdownSize * 2);

                var brush = game.touchdownLocation == null ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(brush, rect);
            }

            // draw SRV marker
            if (this.srvLocation != null)
            {
                const float srvSize = 32f;
                var rect = new RectangleF(
                    (float)srvLocation.dx - srvSize,
                    (float)-srvLocation.dy - srvSize,
                    srvSize * 2,
                    srvSize * 2);

                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }

            g.ResetTransform();

            if (this.touchdownLocation != null)
                this.drawBearingTo(4, 8, "Touchdown:", this.touchdownLocation.Point2);

            if (this.srvLocation != null)
                this.drawBearingTo(4 + mid.Width, 8, "SRV:", this.srvLocation.Point2);
        }

        protected void drawBearingTo(float x, float y, string txt, LatLong2 location)
        {
            if (g == null) return;

            //var txt = scan == game.nearBody.scanOne ? "Scan one:" : "Scan two:";
            g.DrawString(txt, Game.settings.fontSmall, GameColors.brushGameOrange, x, y);

            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);

            var sz = 6;
            x += txtSz.Width + 8;
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            var dd = new TrackingDelta(game.nearBody!.radius, game.status!.here, location);

            Angle deg = dd.angle - game.status.Heading;
            var dx = (float)Math.Sin(Util.degToRad(deg)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * 10F;
            g.DrawLine(GameColors.penGameOrange2, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += sz * 3;
            g.DrawString(Util.metersToString(dd.distance), Game.settings.fontSmall, GameColors.brushGameOrange, x, y);
        }

    }
}
