using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;

namespace SrvSurvey
{
    internal abstract class PlotBase : Form, PlotterForm, IDisposable
    {
        protected Game game = Game.activeGame!;
        protected TrackingDelta? touchdownLocation; // TODO: move to PlotSurfaceBase
        protected TrackingDelta? srvLocation; // TODO: move to PlotSurfaceBase

        /// <summary> The center point on this plotter. </summary>
        protected Size mid;
        protected Graphics g;

        public float scale = 1.0f;
        public float customScale = -1.0f;

        protected float one = scaled(1f);
        protected float two = scaled(2f);
        protected float four = scaled(4f);
        protected float five = scaled(5f);
        protected float six = scaled(6f);
        protected float eight = scaled(8f);
        protected float ten = scaled(10f);
        protected float oneTwo = scaled(12f);
        protected float oneFour = scaled(14f);
        protected float oneSix = scaled(16f);
        protected float oneEight = scaled(18f);
        protected float twenty = scaled(20f);
        protected float twoTwo = scaled(22f);
        protected float twoFour = scaled(24f);
        protected float twoSix = scaled(26f);
        protected float twoEight = scaled(28f);
        protected float thirty = scaled(30f);
        protected float threeFour = scaled(34f);
        protected float forty = scaled(40f);
        protected float fourFour = scaled(44f);
        protected float fourSix = scaled(46f);
        protected float fifty = scaled(50f);
        protected float sixFive = scaled(65f);
        protected float eightSix = scaled(86f);

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
            this.Text = this.GetType().Name;

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

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Elite.setFocusED();
            this.Invalidate();
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

        public static int scaled(int n)
        {
            return (int)(n * GameColors.scaleFactor);
        }

        public static float scaled(float n)
        {
            return (n * GameColors.scaleFactor);
        }

        public static Rectangle scaled(Rectangle r)
        {
            r.X = scaled(r.X);
            r.Y = scaled(r.Y);
            r.Width = scaled(r.Width);
            r.Height = scaled(r.Height);

            return r;
        }

        public static RectangleF scaled(RectangleF r)
        {
            r.X = scaled(r.X);
            r.Y = scaled(r.Y);
            r.Width = scaled(r.Width);
            r.Height = scaled(r.Height);

            return r;
        }

        public static Point scaled(Point pt)
        {
            pt.X = scaled(pt.X);
            pt.Y = scaled(pt.Y);

            return pt;
        }

        public static SizeF scaled(SizeF sz)
        {
            sz.Width = scaled(sz.Width);
            sz.Height = scaled(sz.Height);

            return sz;
        }

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

            if (this.Opacity > 0 && !game.showBodyPlotters)
                this.Opacity = 0;
            else if (this.Opacity == 0 && game.showBodyPlotters)
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
                    entry); // Really? LatLong2.Empty);
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
            var msg = entry.Message.ToLowerInvariant().Trim();

            // adjust the zoom factor 'z <number>'
            if (msg == MsgCmd.z)
            {
                Game.log($"Resetting custom zoom scale");
                this.customScale = -1f;
                this.Invalidate();
                return;
            }
            else if (msg.StartsWith(MsgCmd.z))
            {
                if (float.TryParse(entry.Message.Substring(1), out this.customScale))
                {
                    this.customScale = (float)Math.Max(customScale, 0.1);
                    this.customScale = (float)Math.Min(customScale, 20);
                    Game.log($"Changing custom zoom scale from: '{this.scale}' to: '{this.customScale}'");
                    this.scale = this.customScale;
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
            this.clipToMiddle(four, twoSix, four, twoFour);
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
            this.clipToMiddle();

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

                var isFormerTouchdownLocation = game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty || (game.vehicle == ActiveVehicle.MainShip && game.mode == GameMode.Flying);
                var brush = isFormerTouchdownLocation ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
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

            var sz = scaled(5);
            x += txtSz.Width + scaled(8);
            //y += 4;
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(pen, r);


            var dx = (float)Math.Sin(Util.degToRad(deg)) * scaled(9F);
            var dy = (float)Math.Cos(Util.degToRad(deg)) * scaled(9F);
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
        protected SizeF drawTextAt(string txt)
        {
            return this.drawTextAt(txt, null, null);
        }

        /// <summary>
        /// Draws text at the location of ( dtx, dty ) incrementing dtx by the width of the rendered string.
        /// </summary>
        protected SizeF drawTextAt(string txt, Font? font = null)
        {
            return this.drawTextAt(txt, null, font);
        }

        /// <summary>
        /// Draws text at the location of ( dtx, dty ) incrementing dtx by the width of the rendered string.
        /// </summary>
        protected SizeF drawTextAt(string txt, Brush? brush = null, Font? font = null)
        {
            brush = brush ?? GameColors.brushGameOrange;
            font = font ?? this.Font;
            g.DrawString(txt, font, brush, this.dtx, this.dty);

            var sz = g.MeasureString(txt, font);
            this.dtx += sz.Width;

            return sz;
        }

        private static Dictionary<string, POIType> itemPoiTypeMap = new Dictionary<string, POIType>()
        {
            { ObeliskItem.casket.ToString(), POIType.casket },
            { ObeliskItem.orb.ToString(), POIType.orb },
            { ObeliskItem.relic.ToString(), POIType.relic },
            { ObeliskItem.tablet.ToString(), POIType.tablet},
            { ObeliskItem.totem.ToString(), POIType.totem },
            { ObeliskItem.urn.ToString(), POIType.urn },
        };

        private static PointF[] ramTahRelicPoints = {
            new PointF(-8, -4),
            new PointF(+8, -4),
            new PointF(0, +10),
            new PointF(-8, -4),
        };

        protected void drawRamTahDot(float dx, float dy, string item)
        {
            if (item == POIType.relic.ToString())
            {
                dx += this.dtx + four;
                dy += this.dty + four;
                this.dtx += oneTwo;

                g.TranslateTransform(dx, dy);
                g.FillPolygon(GameColors.brushPoiPresent, ramTahRelicPoints);
                g.DrawPolygon(GameColors.penPoiRelicPresent, ramTahRelicPoints);

                g.TranslateTransform(-dx, -dy);
            }
            else
            {
                var r = new RectangleF(this.dtx + dx, this.dty + dy, ten, ten);
                var poiType = itemPoiTypeMap[item];
                this.dtx += oneTwo;

                g.FillEllipse(GameColors.Map.brushes[poiType][SitePoiStatus.present], r);
                g.DrawEllipse(GameColors.Map.pens[poiType][SitePoiStatus.present], r);
            }

            // TODO: Thargoid items?
        }
    }

    internal abstract class PlotSurfaceBase : PlotBase
    {
        // TODO: Move these to here
        //protected TrackingDelta? touchdownLocation;
        //protected TrackingDelta? srvLocation;
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

    internal class PlotPos
    {
        #region static loading

        private static string customPlotterPositionPath = Path.Combine(Program.dataFolder, "plotters.json");
        private static string defaultPlotterPositionPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "plotters.json");

        private static Dictionary<string, PlotPos> plotterPositions = new Dictionary<string, PlotPos>();

        private static FileSystemWatcher watcher;

        public static void prepPlotterPositions()
        {
            if (!File.Exists(customPlotterPositionPath))
                PlotPos.reset();

            watcher = new FileSystemWatcher(Program.dataFolder, "plotters.json");
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;

            try
            {
                readPlotterPositions(customPlotterPositionPath);
            }
            catch (Exception ex)
            {
                if (ex is IOException) return; // ignore these
                Game.log($"Error first reading overlay positions:\r\n\r\n{ex.Message}");

                // if we fail the first time, retry using the default file
                readPlotterPositions(defaultPlotterPositionPath);
            }
        }

        public static void reset()
        {
            Game.log($"Resetting custom overlay positions");
            File.Copy(defaultPlotterPositionPath, customPlotterPositionPath, true);
        }

        private static void readPlotterPositions(string filepath)
        {
            Game.log($"Reading PlotterPositions from: {filepath}");

            var json = File.ReadAllText(filepath);
            var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;

            var newPositions = new Dictionary<string, PlotPos>();

            foreach (var _ in obj)
                newPositions[_.Key] = new PlotPos(_.Value);

            plotterPositions = newPositions;
            var rect = Elite.getWindowRect();
            Program.control.Invoke(() => Program.repositionPlotters(rect));
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                readPlotterPositions(e.FullPath);
            }
            catch (Exception ex)
            {
                if (ex is IOException) return; // ignore these
                Game.log($"Error reading overlay positions:\r\n\r\n{ex.Message}");
            }
        }

        public static float getOpacity(PlotterForm form)
        {
            if (Program.tempHideAllPlotters) return 0;

            var pp = plotterPositions[form.GetType().Name];
            return pp.opacity ?? Game.settings.Opacity;
        }

        public static void reposition(PlotterForm form, Rectangle rect)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            var pp = plotterPositions[form.GetType().Name];

            var left = 0;
            if (pp.h == Horiz.Left)
                left = rect.Left + pp.x;
            else if (pp.h == Horiz.Center)
                left = rect.Left + (rect.Width / 2) - (form.Width / 2) + pp.x;
            else if (pp.h == Horiz.Right)
                left = rect.Right - form.Width - pp.x;
            else if (pp.h == Horiz.OS)
                left = pp.x;

            var top = 0;
            if (pp.v == Vert.Top)
                top = rect.Top + pp.y;
            else if (pp.v == Vert.Middle)
                top = rect.Top + (rect.Height / 2) - (form.Height / 2) + pp.y;
            else if (pp.v == Vert.Bottom)
                top = rect.Bottom - form.Height - pp.y;
            else if (pp.v == Vert.OS)
                top = pp.y;

            form.Location = new Point(left, top);
        }

        #endregion

        private Horiz h;
        private Vert v;
        private int x;
        private int y;
        private float? opacity;

        public PlotPos(string txt)
        {
            // eg: "left:40,top:50",

            var parts = txt.Split(new char[] { ':', ',' });
            if (parts.Length < 4) throw new Exception($"Bad plotter position: '{txt}'");
            this.h = Enum.Parse<Horiz>(parts[0], true);
            this.x = int.Parse(parts[1]);
            this.v = Enum.Parse<Vert>(parts[2], true);
            this.y = int.Parse(parts[3]);
            if (parts.Length >= 5)
            {
                this.opacity = float.Parse(parts[4]);
                if (this.opacity < 0 || this.opacity > 1)
                    throw new ArgumentException("Opacity must be a decimal number between 0 and 1");
            }
        }

        public enum Horiz
        {
            Left,
            Center,
            Right,
            OS,
        };

        public enum Vert
        {
            Top,
            Middle,
            Bottom,
            OS,
        };
    }

}
