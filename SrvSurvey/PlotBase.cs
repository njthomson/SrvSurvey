using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    internal abstract class PlotBase : Form, PlotterForm, IDisposable
    {
        protected Game game = Game.activeGame!;
        protected TrackingDelta touchdownLocation;
        protected TrackingDelta? srvLocation;
        /// <summary> The center point on this plotter. </summary>
        protected Size mid;
        protected Graphics? g;
        protected float scale = 1.0f;

        protected PlotBase()
        {
            this.Opacity = 0;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;

            this.touchdownLocation = new TrackingDelta(
                game.nearBody!.radius,
                game.touchdownLocation ?? LatLong2.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (game != null)
                {
                    game.modeChanged -= Game_modeChanged;
                    game.status!.StatusChanged -= Status_StatusChanged;
                    game.journals!.onJournalEntry -= Journals_onJournalEntry;
                    game = null!;
                }
            }
        }

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Debugger.IsAttached)
                // use a different cursor if debugging
                this.Cursor = Cursors.No;
            else
                // otherwise hide the cursor entirely
                Cursor.Hide();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // restore the cursor when it leaves
            Cursor.Show();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        #endregion

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
            this.Game_modeChanged(game.mode, true);
            this.Status_StatusChanged(false);
        }

        protected virtual void Game_modeChanged(GameMode newMode, bool force)
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

        protected virtual void Status_StatusChanged(bool blink)
        {
            this.touchdownLocation.Current = Status.here;

            if (this.srvLocation != null)
                this.srvLocation.Current = Status.here;

            this.Invalidate();
        }

        protected void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        protected void onJournalEntry(JournalEntry entry) { /* ignore */ }


        protected void onJournalEntry(Touchdown entry)
        {
            this.touchdownLocation.Target = entry;
            this.Invalidate();
        }

        protected void onJournalEntry(Disembark entry)
        {
            Game.log($"Disembark srvLocation {Status.here}");
            if (entry.SRV && this.srvLocation == null)
            {
                this.srvLocation = new TrackingDelta(
                    game.nearBody!.radius,
                    Status.here.clone());
                this.Invalidate();
            }
        }

        protected void onJournalEntry(Embark entry)
        {
            Game.log($"Embark {Status.here}");
            if (entry.SRV && this.srvLocation != null)
            {
                this.srvLocation = null;
                this.Invalidate();
            }
        }

        protected virtual void onJournalEntry(SendText entry)
        {
            Game.log($"PlotBase: SendText: {entry.Message}");
            var msg = entry.Message.ToLowerInvariant();

            // adjust the zoom factor 'z <number>'
            if (msg.StartsWith(MsgCmd.z))
            {
                float zoomFactor;
                if (float.TryParse(entry.Message.Substring(1), out zoomFactor))
                {
                    Game.log($"Change zoom scale from: '{this.scale}' to: '{zoomFactor}'");
                    this.scale = zoomFactor;
                    this.Invalidate();
                    return;
                }
            }
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

        protected void clipToMiddle()
        {
            this.clipToMiddle(4, 20, 4, 24);
        }

        protected void clipToMiddle(float left, float top, float right, float bottom)
        {
            if (g == null) return;

            var r = new RectangleF(left, top, this.Width - left - right, this.Height - top - bottom);
            g.ResetClip();
            g.Clip = new Region(r);
        }

        protected void drawCompassLines(int heading = -1)
        {
            if (g == null) return;

            if (heading == -1) heading = game.status!.Heading;

            g.ResetTransform();
            this.clipToMiddle(4, 26, 4, 24);

            g.TranslateTransform(mid.Width, mid.Height);

            // draw compass rose lines
            g.RotateTransform(360 - heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);
            g.ResetClip();
        }

        protected void drawTouchdownAndSrvLocation()
        {
            if (g == null || (this.touchdownLocation == null && this.srvLocation == null)) return;

            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
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
                const float srvSize = 16f; // 32f;
                var rect = new RectangleF(
                    (float)srvLocation.dx - srvSize,
                    (float)-srvLocation.dy - srvSize,
                    srvSize * 2,
                    srvSize * 2);

                g.FillRectangle(GameColors.brushShipLocation, rect);
            }

            g.ResetTransform();

            if (this.touchdownLocation != null)
                this.drawBearingTo(4, 10, "Touchdown:", this.touchdownLocation.Target);

            if (this.srvLocation != null)
                this.drawBearingTo(4 + mid.Width, 10, "SRV:", this.srvLocation.Target);
        }

        protected void drawBearingTo(float x, float y, string txt, LatLong2 location)
        {
            if (g == null) return;

            //var txt = scan == game.nearBody.scanOne ? "Scan one:" : "Scan two:";
            g.DrawString(txt, Game.settings.fontSmall, GameColors.brushGameOrange, x, y);

            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);

            var sz = 5;
            x += txtSz.Width + 8;
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            var dd = new TrackingDelta(game.nearBody!.radius, location);

            Angle deg = dd.angle - game.status!.Heading;
            var dx = (float)Math.Sin(Util.degToRad(deg)) * 9F;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * 9F;
            g.DrawLine(GameColors.penGameOrange2, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += sz * 3;
            g.DrawString(Util.metersToString(dd.distance), Game.settings.fontSmall, GameColors.brushGameOrange, x, y);
        }

        protected void drawHeaderText(string msg)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();
            var sz = g.MeasureString(msg, Game.settings.fontSmall);
            var tx = mid.Width - (sz.Width / 2);
            var ty = 6;
            g.DrawString(msg, Game.settings.fontSmall, GameColors.brushGameOrange, tx, ty);
        }

        protected void drawFooterText(string msg)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();
            var sz = g.MeasureString(msg, Game.settings.fontSmall);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 6;
            g.DrawString(msg, Game.settings.fontSmall, GameColors.brushGameOrange, tx, ty);
        }
    }
}
