using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text;

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
            this.Height = 500;
            this.Width = 380;

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
            var gameRect = Elite.getWindowRect(true);
            this.reposition(gameRect);

            // reposition trackers if it already exists
            var plotTrackers = Program.getPlotter<PlotTrackers>();
            if (plotTrackers != null)
            {
                plotTrackers.reposition(gameRect);
            }
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            game.status!.StatusChanged += Status_StatusChanged;
            Game.update += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode, true);
            this.Status_StatusChanged(false);

            game.journals!.onJournalEntry += Journals_onJournalEntry;
            game.nearBody!.bioScanEvent += NearBody_bioScanEvent;

            // get landing location
            Game.log($"initialize here: {Status.here}, touchdownLocation: {game.touchdownLocation}, radius: {game.nearBody!.radius.ToString("N0")}");

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
            if (this.IsDisposed) return;

            this.Invalidate();
        }

        #region journal events

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            if (this.IsDisposed) return;

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

        private void onJournalEntry(SendText entry)
        {
            if (entry.Message.ToLower() == MsgCmd.dbgDump)
            {
                var str = new StringBuilder($"Distance diagnostics from here: {Status.here}, nearBody.radius: {game.nearBody?.radius.ToString("N0")}\r\n");

                if (game.nearBody?.data.bioScans != null)
                {
                    foreach (var scan in game.nearBody.data.bioScans)
                    {
                        str.AppendLine($"\r\n> Species: {scan.species} ({scan.status})");
                        str.AppendLine($"      radius: " + scan.radius.ToString("N0"));
                        str.AppendLine($"      location: {scan.location}");
                    }
                }

                if (game.cmdr.scanOne != null)
                    str.AppendLine($"ScanOne: radius: {game.cmdr.scanOne.radius.ToString("N0")}, location: {game.cmdr.scanOne.location}");
                if (game.cmdr.scanTwo != null)
                    str.AppendLine($"ScanTwo: radius: {game.cmdr.scanTwo.radius.ToString("N0")}, location: {game.cmdr.scanTwo.location}");

                var form = Program.getPlotter<PlotTrackers>();
                if (form != null)
                {
                    foreach (var foo in form.trackers)
                    {
                        str.AppendLine($"\r\n> Key: {foo.Key}");
                        foreach (var bar in foo.Value)
                        {
                            str.AppendLine($" --> {bar}");
                            str.AppendLine($"      radius: " + bar.radius.ToString("N0"));
                            str.AppendLine($"      target: {bar.Target}");
                            str.AppendLine($"      current: {bar.Current}");
                        }
                    }
                }

                Game.log(str.ToString());
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
            g.SmoothingMode = SmoothingMode.HighQuality;
            // clip to preserve top/bottom text area

            var bottomClip = game.cmdr.scanOne == null ? 34 : 52;
            g.Clip = new Region(new RectangleF(4, 24, this.Width - 8, this.Height - bottomClip));

            // draw basic compass cross hairs centered in the window
            this.drawCompassLines(g);

            this.drawBioScans(g);

            // draw touchdown marker
            var shipDeparted = game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty;
            if (game.touchdownLocation != null)
            {
                if (this.td == null)
                    this.td = new TrackingDelta(game.nearBody.radius, game.touchdownLocation);

                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(mw, mh);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status!.Heading);

                const float touchdownSize = 64f;
                var rect = new RectangleF((float)td.dx - touchdownSize, (float)-td.dy - touchdownSize, touchdownSize * 2, touchdownSize * 2);
                var b = shipDeparted ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(b, rect);

                if (!shipDeparted && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
                {
                    // draw 2km circle for when ship may depart
                    const float liftoffSize = 2000f;
                    rect = new RectangleF((float)td.dx - liftoffSize, (float)-td.dy - liftoffSize, liftoffSize * 2, liftoffSize * 2);
                    var p = new Pen(Color.FromArgb(64, Color.Red), 32) { DashStyle = DashStyle.DashDotDot };
                    if (this.td.distance > 1800)
                        p = new Pen(Color.FromArgb(255, Color.Red), 32) { DashStyle = DashStyle.DashDotDot };

                    g.DrawEllipse(p, rect);
                }
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
            g.DrawEllipse(GameColors.penLime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.penLime2, 0, 0, +dx, -dy);

            g.ResetTransform();
            // remove clip to preserving top/bottom text area
            g.Clip = new Region(new RectangleF(0, 4, this.Width, this.Height - 8));

            if (this.td != null)
                this.drawBearingTo(g, 4, 8, "Touchdown:", this.td.Target);

            if (this.srvLocation != null)
                this.drawBearingTo(g, 4 + mw, 8, "SRV:", this.srvLocation.Target);

            float y = this.Height - 24;
            if (game.nearBody!.scanOne != null)
                this.drawBearingTo(g, 10, y, "Scan one:", game.nearBody.scanOne.location!);

            if (game.nearBody.scanTwo != null)
                this.drawBearingTo(g, 10 + mw, y, "Scan two:", game.nearBody.scanTwo.location!);

            if (!shipDeparted && this.td?.distance > 1800 && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
            {
                var msg = "Nearing ship departure distance";
                var font = GameColors.fontSmall;
                var sz = g.MeasureString(msg, font);
                var tx = mw - (sz.Width / 2);
                var ty = 42;

                const int pad = 14;
                var rect = new RectangleF(tx - pad, ty - pad, sz.Width + pad * 2, sz.Height + pad * 2);
                g.FillRectangle(GameColors.brushShipDismissWarning, rect);

                rect.Inflate(-10, -10);
                g.FillRectangle(Brushes.Black, rect);
                g.DrawString(msg, font, Brushes.Red, tx + 1, ty + 1);
            }
        }

        private void drawCompassLines(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(mw, mh);

            // draw compass rose lines
            g.RotateTransform(360 - game.status!.Heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);

            g.ResetTransform();
        }

        private void drawBioScans(Graphics g)
        {
            if (game.nearBody == null || game.systemBody == null) return;

            // delta to ship
            g.ResetTransform();

            g.TranslateTransform(mw, mh);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(360 - game.status!.Heading);

            // use the same Tracking delta for all bioScans against the same currentLocation
            var currentLocation = new LatLong2(this.game.status);
            var d = new TrackingDelta(game.nearBody.radius, currentLocation.clone());
            if (game.systemBody?.bioScans?.Count > 0)
                foreach (var scan in game.systemBody.bioScans)
                    drawBioScan(g, d, scan);

            // draw prior scan circle first
            if (Game.settings.autoLoadPriorScans)
                drawPriorScans(g);

            // draw trackers circles first
            if (game.systemBody?.bookmarks?.Count > 0)
                drawTrackers(g);

            // draw active scans top most
            if (game.nearBody.scanOne != null)
            {
                drawBioScan(g, d, game.nearBody.scanOne);
            }
            if (game.nearBody.scanTwo != null)
            {
                drawBioScan(g, d, game.nearBody.scanTwo);
            }

            g.ResetTransform();
        }

        private void drawPriorScans(Graphics g)
        {
            var form = Program.getPlotter<PlotPriorScans>();
            if (game.nearBody == null || form == null) return;

            foreach (var signal in form.signals)
            {
                var analyzed = signal.genusName != null && game.nearBody.data.organisms[signal.genusName].analyzed;
                var isActive = (game.cmdr.scanOne?.genus == null && !analyzed) || game.cmdr.scanOne?.genus == signal.genusName;

                // default range to 50m unless name matches a Genus
                var radius = signal.genusName != null && BioScan.ranges.ContainsKey(signal.genusName) ? BioScan.ranges[signal.genusName] : 50;

                // draw radar circles for this group, and lines
                foreach (var tt in signal.trackers)
                {
                    if (Util.isCloseToScan(tt.Target, signal.genusName) || analyzed) continue;

                    Brush b = isActive ? /*GameColors.brushTracker*/ new HatchBrush(HatchStyle.DottedDiamond, Color.FromArgb(48, Color.Lime), Color.Transparent)
                        : /*GameColors.brushTrackInactive*/ new HatchBrush(HatchStyle.DottedDiamond, Color.FromArgb(48, Color.SlateGray), Color.Transparent);

                    var p = isActive ? /*GameColors.penTracker*/ new Pen(Color.FromArgb(64, Color.Lime)) { Width = 8, DashStyle = DashStyle.Dot }
                        : /*GameColors.penTrackInactive*/ new Pen(Color.FromArgb(48, Color.SlateGray)) { Width = 8, DashStyle = DashStyle.Dot };

                    if (tt.distance < PlotTrackers.highlightDistance)
                    {
                        b = isActive ? new HatchBrush(HatchStyle.DottedDiamond, Color.FromArgb(80, Color.Yellow), Color.Transparent)
                        : new HatchBrush(HatchStyle.DottedDiamond, Color.FromArgb(80, Color.Olive), Color.Transparent);
                        //p = Pens.Yellow; //GameColors.penTrackerClose;
                        p = isActive ? new Pen(Color.FromArgb(80, Color.Yellow)) { Width = 8, DashStyle = DashStyle.Dot }
                        : new Pen(Color.FromArgb(80, Color.Olive)) { Width = 8, DashStyle = DashStyle.Dot };
                    }

                    var rect = new RectangleF((float)tt.dx - radius, (float)-tt.dy - radius, radius * 2f, radius * 2f);
                    this.drawRadarCircle(g, rect, b, p);

                    // draw an inner circle if really close
                    if (tt.distance < PlotTrackers.highlightDistance) // && game.cmdr.scanOne != null)
                    {
                        var innerRadius = 50;
                        rect = new RectangleF((float)tt.dx - innerRadius, (float)-tt.dy - innerRadius, innerRadius * 2f, innerRadius * 2f);
                        this.drawRadarCircle(g, rect, b, p);
                    }
                }
            }
        }

        private void drawTrackers(Graphics g)
        {
            var form = Program.getPlotter<PlotTrackers>();
            if (game.systemBody?.bookmarks == null || game.nearBody == null || form == null) return;

            foreach (var name in form.trackers.Keys)
            {
                var isActive = /* game.cmdr.scanOne?.genus == null || */ game.cmdr.scanOne?.genus == name;

                // default range to 50m unless name matches a Genus
                BioScan.prefixes.TryGetValue(name, out var fullName);
                var radius = fullName != null && BioScan.ranges.ContainsKey(fullName) ? BioScan.ranges[fullName] : 50;

                // draw radar circles for this group, and lines
                foreach (var tt in form.trackers[name])
                {
                    var b = isActive ? GameColors.brushTracker : GameColors.brushTrackInactive;
                    var p = isActive ? GameColors.penTracker : GameColors.penTrackInactive;
                    if (isActive && tt.distance < PlotTrackers.highlightDistance)
                    {
                        b = GameColors.brushTrackerClose;
                        p = GameColors.penTrackerClose;
                    }

                    var rect = new RectangleF((float)tt.dx - radius, (float)-tt.dy - radius, radius * 2f, radius * 2f);
                    this.drawRadarCircle(g, rect, b, p);

                    // draw an inner circle if really close
                    if (tt.distance < PlotTrackers.highlightDistance && game.cmdr.scanOne != null)
                    {
                        var innerRadius = 50;
                        rect = new RectangleF((float)tt.dx - innerRadius, (float)-tt.dy - innerRadius, innerRadius * 2f, innerRadius * 2f);
                        this.drawRadarCircle(g, rect, b, GameColors.penExclusionActive);
                    }
                }
            }
        }

        private void drawBioScan(Graphics g, TrackingDelta d, BioScan scan)
        {
            d.Target = scan.location!;
            var radius = scan.radius * 1f;

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
                // grey colors for abandoned scans
                //b = GameColors.brushExclusionAbandoned;
                //p = GameColors.penExclusionAbandoned;
                b = GameColors.brushTrackInactive;
                p = GameColors.penTrackInactive;

            }
            else if (scan.status == BioScan.Status.Died)
            {
                var isActive = game.cmdr.scanOne == null || scan.genus == game.cmdr.scanOne.genus;
                var analyzed = game.nearBody != null && game.nearBody.data.organisms.ContainsKey(scan.genus!) && game.nearBody.data.organisms[scan.genus!].analyzed;
                // blue colors for scans lost due to death
                b = isActive && !analyzed ? GameColors.brushExclusionAbandoned : Brushes.Transparent;
                p = isActive && !analyzed ? GameColors.penExclusionAbandoned : Pens.Navy;
                radius = 40;
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

            var rect = new RectangleF((float)d.dx - radius, (float)-d.dy - radius, radius * 2f, radius * 2f);

            this.drawRadarCircle(g, rect, b, p);
        }

        private void drawRadarCircle(Graphics g, RectangleF rect, Brush b, Pen p)
        {
            var fudge = 10;

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
            g.DrawString(txt, GameColors.fontSmall, GameColors.brushGameOrange, x, y);

            var txtSz = g.MeasureString(txt, GameColors.fontSmall);

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
            g.DrawString(Util.metersToString(dd.distance), GameColors.fontSmall, GameColors.brushGameOrange, x, y);
        }
    }
}
