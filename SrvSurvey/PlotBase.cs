using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal abstract class PlotBase : Form, PlotterForm, IDisposable
    {
        protected Game game = Game.activeGame!;
        protected TrackingDelta? touchdownLocation;
        protected TrackingDelta? srvLocation;
        /// <summary> The center point on this plotter. </summary>
        protected Size mid;
        protected Graphics g;
        public float scale = 1.0f;

        protected PlotBase()
        {
            this.BackColor = Color.Black;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = Game.settings.Opacity;
            this.DoubleBuffered = true;

            this.TopMost = true;
            this.Cursor = Cursors.Cross;

            if (this.game == null) throw new Exception("Why no active game?");

            if (game.systemData != null && game.systemBody != null) // retire
            {
                this.touchdownLocation = new TrackingDelta(
                    game.systemBody.radius,
                    game.touchdownLocation ?? LatLong2.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (game != null)
                {
                    Game.update -= Game_modeChanged;
                    if (game.status != null)
                        game.status.StatusChanged -= Status_StatusChanged;
                    if (game.journals != null)
                        game.journals.onJournalEntry -= Journals_onJournalEntry;
                    game = null!;
                }
            }
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

        public abstract void reposition(Rectangle gameRect);

        protected virtual void initialize()
        {
            if (this.IsDisposed) return;

            this.reposition(Elite.getWindowRect());
            this.mid = this.Size / 2;

            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            Game.update += Game_modeChanged;
            game.status!.StatusChanged += Status_StatusChanged;
            game.journals!.onJournalEntry += Journals_onJournalEntry;
            //game.nearBody!.bioScanEvent += NearBody_bioScanEvent;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode, true);
            this.Status_StatusChanged(false);
        }

        protected virtual void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var targetMode = game.showBodyPlotters;
            if (this.Opacity > 0 && !targetMode)
                this.Opacity = 0;
            else if (this.Opacity == 0 && targetMode)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected virtual void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            if (this.touchdownLocation != null)
                this.touchdownLocation.Current = Status.here;

            if (this.srvLocation != null)
                this.srvLocation.Current = Status.here;

            this.Invalidate();
        }

        protected void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            if (this.IsDisposed) return;

            this.onJournalEntry((dynamic)entry);
        }

        protected void onJournalEntry(JournalEntry entry) { /* ignore */ }

        protected void onJournalEntry(Touchdown entry)
        {
            if (this.touchdownLocation == null)
            {
                this.touchdownLocation = new TrackingDelta(
                    game.systemBody!.radius,
                    game.touchdownLocation ?? LatLong2.Empty);
            }
            else
            {
                this.touchdownLocation.Target = entry;
            }

            this.Invalidate();
        }

        protected virtual void onJournalEntry(Disembark entry)
        {
            //Game.log($"Disembark srvLocation {Status.here}");
            if (entry.SRV && this.srvLocation == null)
            {
                this.srvLocation = new TrackingDelta(
                    game.systemBody!.radius,
                    Status.here.clone());
                this.Invalidate();
            }
        }

        protected virtual void onJournalEntry(Embark entry)
        {
            //Game.log($"Embark {Status.here}");
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
                    zoomFactor = (float)Math.Max(zoomFactor, 0.2);
                    zoomFactor = (float)Math.Min(zoomFactor, 8);
                    this.scale = zoomFactor;
                    this.Invalidate();
                    return;
                }
            }
        }

        protected virtual void onJournalEntry(CodexEntry entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(FSDTarget entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(Screenshot entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(DataScanned entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(SupercruiseEntry entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(FSDJump entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(NavRoute entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(NavRouteClear entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(FSSDiscoveryScan entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(Scan entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(FSSBodySignals entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(ScanOrganic entry)
        {
            // overriden as necessary
        }

        protected virtual void onJournalEntry(MaterialCollected entry)
        {
            // overriden as necessary
        }

        protected void drawCommander()
        {
            if (g == null) return;

            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.RotateTransform(360 - game.status!.Heading);

            var locSz = 5f;
            g.DrawEllipse(GameColors.penLime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.penLime2, 0, 0, +dx, -dy);
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

        protected void drawTouchdownAndSrvLocation(bool hideHeader = false)
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

                var brush = (game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty) ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(brush, rect);
            }

            // draw SRV marker
            if (this.srvLocation != null)
            {
                const float srvSize = 10f;
                var rect = new RectangleF(
                    (float)srvLocation.dx - srvSize,
                    (float)-srvLocation.dy - srvSize,
                    srvSize * 2,
                    srvSize * 2);

                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }

            g.ResetTransform();

            if (!hideHeader)
            {
                if (this.touchdownLocation != null)
                    this.drawBearingTo(4, 10, "Touchdown:", this.touchdownLocation.Target);

                if (this.srvLocation != null)
                    this.drawBearingTo(4 + mid.Width, 10, "SRV:", this.srvLocation.Target);
            }
        }

        protected void drawBearingTo(float x, float y, string txt, LatLong2 location)
        {
            var dd = new TrackingDelta(game.systemBody!.radius, location);
            Angle deg = dd.angle - game.status!.Heading;

            drawBearingTo(x, y, txt, (double)dd.distance, (double)deg);
        }

        protected void drawBearingTo(float x, float y, string txt, double dist, double deg, Brush? brush = null, Pen? pen = null)
        {
            if (g == null) return;
            if (brush == null) brush = GameColors.brushGameOrange;
            if (pen == null) pen = GameColors.penGameOrange2;

            if (!string.IsNullOrEmpty(txt))
            {
                //var txt = scan == game.nearBody.scanOne ? "Scan one:" : "Scan two:";
                g.DrawString(txt, GameColors.fontSmall, brush, x, y);
            }

            var txtSz = g.MeasureString(txt, GameColors.fontSmall);

            var sz = 5;
            x += txtSz.Width + 8;
            //y += 4;
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(pen, r);


            var dx = (float)Math.Sin(Util.degToRad(deg)) * 9F;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * 9F;
            g.DrawLine(pen, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += 2 + sz * 3;
            g.DrawString(Util.metersToString(dist), GameColors.fontSmall, brush, x, y);
        }

        protected void drawHeaderText(string msg, Brush? brush = null)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            var font = GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = 4;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
        }

        protected void drawFooterText(string msg, Brush? brush = null, Font? font = null)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            font = font ?? GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 5;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
        }

        protected void drawCenterMessage(string msg, Brush? brush = null)
        {
            var font = GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = 34;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
        }

        /// <summary> The x location to use in drawTextAt</summary>
        protected float dtx;
        /// <summary> The y location to use in drawTextAt</summary>
        protected float dty;

        /// <summary>
        /// Draws text at the location of ( dtx, dty ) incrementing dtx by the width of the rendered string.
        /// </summary>
        protected void drawTextAt(string txt, Brush? brush = null, Font? font = null)
        {
            brush = brush ?? GameColors.brushGameOrange;

            g.DrawString(txt, font ?? this.Font, brush, this.dtx, this.dty);
            this.dtx += g.MeasureString(txt, this.Font).Width;
        }
    }

    internal class PlotBaseSelectable : PlotBase, PlotterForm
    {
        protected int selectedIndex = 0;
        private Point[] ptMain;
        private Point[] ptLetter;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool highlightBlink = false;

        protected PlotBaseSelectable() : base()
        {
            this.Width = 500;
            this.Height = 108;

            timer.Tick += Timer_Tick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Tick -= Timer_Tick;
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));

            const int blockWidth = 100;
            const int blockTop = 45;
            const int letterOffset = 10;
            ptMain = new Point[]
            {
                new Point((int)(this.mid.Width - (blockWidth * 1.5f)) , blockTop ),
                new Point((int)(this.mid.Width - (blockWidth * 0.5f)), blockTop ),
                new Point((int)(this.mid.Width + (blockWidth * 0.5f)) , blockTop )
            };
            ptLetter = new Point[]
            {
                new Point((int)(this.mid.Width - (blockWidth * 1.5f) - letterOffset) , blockTop - letterOffset),
                new Point((int)(this.mid.Width - (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset),
                new Point((int)(this.mid.Width + (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset)
            };
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatCenterTop(this, gameRect, 0);

            this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            this.selectedIndex = game.status.FireGroup % 3;

            if (!blink && timer.Enabled == false)
            {
                // if we are within a blink detection window - highlight the footer
                var duration = DateTime.Now - game.status.lastblinkChange;
                if (duration.TotalMilliseconds < Game.settings.blinkDuration)
                {
                    this.highlightBlink = true;
                    timer.Interval = Game.settings.blinkDuration;
                    timer.Start();
                }
            }

            base.Status_StatusChanged(blink);
        }

        protected void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            this.highlightBlink = false;
            this.Invalidate();
        }

        protected void drawOptions(string msg1, string msg2, string? msg3, int highlightIdx)
        {
            var c = highlightIdx == 0 ? GameColors.Cyan : GameColors.Orange;
            if (highlightIdx == -2) c = Color.Gray;
            TextRenderer.DrawText(g, "A:", GameColors.fontSmall, ptLetter[0], c);
            TextRenderer.DrawText(g, msg1, GameColors.fontMiddle, ptMain[0], c);

            c = highlightIdx == 1 ? GameColors.Cyan : GameColors.Orange;
            if (highlightIdx == -2) c = Color.Gray;
            TextRenderer.DrawText(g, "B:", GameColors.fontSmall, ptLetter[1], c);
            TextRenderer.DrawText(g, msg2, GameColors.fontMiddle, ptMain[1], c);

            if (msg3 != null)
            {
                c = highlightIdx == 2 ? GameColors.Cyan : GameColors.Orange;
                if (highlightIdx == -2) c = Color.Gray;
                TextRenderer.DrawText(g, "C:", GameColors.fontSmall, ptLetter[2], c);
                TextRenderer.DrawText(g, msg3, GameColors.fontMiddle, ptMain[2], c);
            }

            // show selection rectangle
            var rect = new Rectangle(0, 0, 86, 44);
            rect.Location = ptMain[selectedIndex];
            rect.Offset(-12, -12);
            var p = highlightIdx == selectedIndex ? Pens.Cyan : GameColors.penGameOrange1;
            if (highlightIdx == -2 || (msg3 == null && highlightIdx == 2)) p = Pens.Gray;
            g.DrawRectangle(p, rect);

            if (highlightIdx != -2)
                showSelectionCue();
        }

        protected void showSelectionCue()
        {
            // show cue to select
            var triggerTxt = "cockpit mode";
            var b = this.highlightBlink ? GameColors.brushCyan : GameColors.brushGameOrange;
            var times = this.highlightBlink ? "again" : "once";
            drawFooterText($"(toggle {triggerTxt} {times} to set)", b);
        }
    }
}
