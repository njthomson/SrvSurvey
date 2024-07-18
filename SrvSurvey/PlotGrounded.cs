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

            this.scale = 0.25f;
            this.Cursor = Cursors.Cross;
        }

        //public override bool allow { get => PlotGrounded.allowPlotter; }

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

        private void PlotGrounded_Load(object sender, EventArgs e)
        {
            this.Height = PlotBase.scaled(500);
            this.Width = PlotBase.scaled(380);
            this.mw = this.Width / 2;
            this.mh = this.Height / 2;
            this.initializeOnLoad();
            var gameRect = Elite.getWindowRect(true);
            this.reposition(gameRect);

            // reposition trackers if it already exists
            var plotTrackers = Program.getPlotter<PlotTrackers>();
            if (plotTrackers != null)
            {
                plotTrackers.reposition(gameRect);
            }
        }

        private void initializeOnLoad()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            game.status!.StatusChanged += Status_StatusChanged;
            Game.update += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode, true);
            this.Status_StatusChanged(false);

            game.journals!.onJournalEntry += Journals_onJournalEntry;

            // get landing location
            Game.log($"initialize here: {Status.here}, touchdownLocation: {game.touchdownLocation}, radius: {game.systemBody!.radius.ToString("N0")}");

            if (game.touchdownLocation == null)
            {
                Game.log("Why no touchdownLocation?");
                return;
            }

            if (game.systemBody.lastTouchdown != null)
                this.td = new TrackingDelta(
                    game.systemBody.radius,
                    game.systemBody.lastTouchdown);
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
                this.srvLocation = new TrackingDelta(game.systemBody!.radius, Status.here.clone());
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

        public static bool allowPlotter
        {
            get => Game.settings.autoShowBioPlot
                && Game.activeGame?.systemBody != null
                && Game.activeGame.status != null
                && !Game.activeGame.isShutdown // not needed?
                && !Game.activeGame.atMainMenu // not needed?
                && !Game.activeGame.status.OnFootSocial
                && Game.activeGame.humanSite == null
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.status.hasLatLong
                && Game.activeGame.status.Altitude < 10_000
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.status.FsdChargingJump
                && !PlotGuardians.allowPlotter
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel);
            // TODO: include these?
            // if (game.systemSite == null && !game.isMode(GameMode.SuperCruising, GameMode.GlideMode) && (game.isLanded || showPlotTrackers || showPlotPriorScans || game.cmdr.scanOne != null))
        }

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !PlotGrounded.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotGrounded.allowPlotter)
                this.reposition(Elite.getWindowRect());

            if (game.systemBody == null)
                Program.closePlotter<PlotGrounded>();
            else
                this.Invalidate();
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
            if (this.IsDisposed || game.systemBody == null || game.status == null) return;
            var four = PlotBase.scaled(4);
            var eight = PlotBase.scaled(8);
            var ten = PlotBase.scaled(10);
            var sixFour = PlotBase.scaled(64f);

            this.mw = this.Width / 2;
            this.mh = this.Height / 2;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            // clip to preserve top/bottom text area

            var bottomClip = PlotBase.scaled(game.cmdr.scanOne == null ? 34 : 52);
            g.Clip = new Region(new RectangleF(4, 24, this.Width - 8, this.Height - bottomClip));

            // draw basic compass cross hairs centered in the window
            this.drawCompassLines(g);

            this.drawBioScans(g);

            // draw touchdown marker
            var shipDeparted = game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty;
            if (game.systemBody.lastTouchdown != null)
            {
                if (this.td == null)
                    this.td = new TrackingDelta(game.systemBody.radius, game.systemBody.lastTouchdown);

                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(mw, mh);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status!.Heading);


                float touchdownSize = sixFour;
                var rect = PlotBase.scaled(new RectangleF((float)td.dx - 64, (float)-td.dy - 64, 128, 128));
                var b = shipDeparted ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(b, rect);

                if (!shipDeparted && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
                {
                    // draw 2km circle for when ship may depart
                    const float liftoffSize = 2000f;
                    rect = PlotBase.scaled(new RectangleF((float)td.dx - liftoffSize, (float)-td.dy - liftoffSize, liftoffSize * 2, liftoffSize * 2));
                    var p = GameColors.penShipDepartFar;
                    if (this.td.distance > 1800)
                        p = GameColors.penShipDepartNear;

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
                g.FillRectangle(GameColors.brushSrvLocation, PlotBase.scaled(rect));
            }

            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mw, mh);
            g.RotateTransform(360 - game.status!.Heading);

            var locSz = PlotBase.scaled(5f);
            g.DrawEllipse(GameColors.penLime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * locSz * 2;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * locSz * 2;
            g.DrawLine(GameColors.penLime2, 0, 0, +dx, -dy);

            g.ResetTransform();
            // remove clip to preserving top/bottom text area
            g.Clip = new Region(new RectangleF(0, four, this.Width, this.Height - eight));

            if (this.td != null)
                this.drawBearingTo(g, four, eight, "Touchdown:", this.td.Target);

            if (this.srvLocation != null)
                this.drawBearingTo(g, four + mw, eight, "SRV:", this.srvLocation.Target);

            float y = this.Height - PlotBase.scaled(24);
            if (game.cmdr!.scanOne != null)
                this.drawBearingTo(g, ten, y, "Scan one:", game.cmdr.scanOne.location!);

            if (game.cmdr.scanTwo != null)
                this.drawBearingTo(g, ten + mw, y, "Scan two:", game.cmdr.scanTwo.location!);

            // TODO: fix bug where warning shown and ship already departed
            if (!shipDeparted && this.td?.distance > 1800 && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
            {
                var msg = "Nearing ship departure distance";
                var font = GameColors.fontSmall;
                var sz = g.MeasureString(msg, font);
                var tx = mw - (sz.Width / 2);
                var ty = PlotBase.scaled(42);

                int pad = 14;
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
            if (game.systemBody == null) return;

            // delta to ship
            g.ResetTransform();

            g.TranslateTransform(mw, mh);
            g.ScaleTransform(scale, scale);
            var rotation = 360 - game.status!.Heading;
            g.RotateTransform(rotation);

            //this.bbs.ForEach(b =>
            //{
            //    b.ResetTransform();
            //    //this.bb1.RotateTransform(-rotation);
            //    //b.TranslateTransform((float)this.td.dx, (float)this.td.dy);

            //    var tt = new TrackingDelta(game.systemBody.radius, LatLong2.Empty);
            //    b.TranslateTransform((float)tt.dx, -(float)tt.dy);

            //    //var lx = (float)game.status.Latitude * 1000;
            //    //var ly = (float)game.status.Longitude * 1000;
            //    //b.TranslateTransform(lx, ly);
            //});

            // use the same Tracking delta for all bioScans against the same currentLocation
            var currentLocation = new LatLong2(this.game.status);
            var d = new TrackingDelta(game.systemBody.radius, currentLocation.clone());
            if (game.systemBody?.bioScans?.Count > 0)
                foreach (var scan in game.systemBody.bioScans)
                    drawBioScan(g, d, scan);

            // draw prior scan circle first
            if (Game.settings.useExternalData && Game.settings.autoLoadPriorScans && Game.settings.showCanonnSignalsOnRadar)
                drawPriorScans(g);

            // draw trackers circles first
            if (game.systemBody?.bookmarks?.Count > 0)
                drawTrackers(g);

            // draw active scans top most
            if (game.cmdr.scanOne != null)
            {
                drawBioScan(g, d, game.cmdr.scanOne);
            }
            if (game.cmdr.scanTwo != null)
            {
                drawBioScan(g, d, game.cmdr.scanTwo);
            }

            g.ResetTransform();
        }

        private void drawPriorScans(Graphics g)
        {
            var form = Program.getPlotter<PlotPriorScans>();
            if (game.systemBody == null || form == null) return;

            foreach (var signal in form.signals)
            {
                var analyzed = game.systemBody.organisms?.FirstOrDefault(_ => _.genus == signal.genusName)?.analyzed == true;
                var isActive = (game.cmdr.scanOne?.genus == null && !analyzed) || game.cmdr.scanOne?.genus == signal.genusName;

                // default range to 50m unless name matches a Genus
                var radius = signal.genusName != null && BioScan.ranges.ContainsKey(signal.genusName) ? BioScan.ranges[signal.genusName] : 50;

                // draw radar circles for this group, and lines
                foreach (var tt in signal.trackers)
                {
                    if (Util.isCloseToScan(tt.Target, signal.genusName) || analyzed) continue;

                    var b = isActive ? GameColors.PriorScans.Active.brush : GameColors.PriorScans.Inactive.brush;

                    var p = isActive ? GameColors.penTrackerActive : GameColors.penTrackerInactive;

                    var c = isActive ? GameColors.PriorScans.Active.color : GameColors.PriorScans.Inactive.color;

                    if (tt.distance < PlotTrackers.highlightDistance)
                    {
                        b = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush; // TODO: confirm
                                                                                                                            // this.bbClose : this.bbCloseInactive; 

                        p = isActive ? GameColors.PriorScans.CloseActive.penRadar : GameColors.PriorScans.CloseInactive.penRadar;
                        c = isActive ? GameColors.PriorScans.CloseActive.color : GameColors.PriorScans.CloseInactive.color;
                    }

                    // animate brush

                    // draw differed radar circle - either small or at radius size
                    GraphicsPath path = new GraphicsPath();
                    if (Game.settings.useSmallCirclesWithCanonn)
                        path.AddEllipse(PlotBase.scaled(new RectangleF((float)tt.dx - 100, (float)-tt.dy - 100, 200, 200)));
                    else
                        path.AddEllipse(PlotBase.scaled(new RectangleF((float)tt.dx - radius, (float)-tt.dy - radius, radius * 2f, radius * 2f)));

                    var gb = new PathGradientBrush(path)
                    {
                        CenterColor = Color.Transparent,
                        SurroundColors = new Color[] { c }
                    };
                    g.FillPath(gb, path);

                    // draw an inner circle if really close
                    if (tt.distance < PlotTrackers.highlightDistance) // && game.cmdr.scanOne != null)
                    {
                        var innerRadius = 50;
                        var rect = new RectangleF((float)tt.dx - innerRadius, (float)-tt.dy - innerRadius, innerRadius * 2f, innerRadius * 2f);
                        this.drawRadarCircle(g, rect, Brushes.Transparent, p);
                    }
                }
            }
        }

        private void drawTrackers(Graphics g)
        {
            var form = Program.getPlotter<PlotTrackers>();
            if (game.systemBody?.bookmarks == null || form == null) return;

            foreach (var name in form.trackers.Keys)
            {
                BioScan.prefixes.TryGetValue(name, out var genusName);
                var isActive = /* game.cmdr.scanOne?.genus == null || */ game.cmdr.scanOne?.genus == genusName;

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
                    if (tt.distance < PlotTrackers.highlightDistance)
                    {
                        if (game.cmdr.scanOne != null)
                            p = GameColors.penExclusionActive;

                        var innerRadius = 50;
                        rect = new RectangleF((float)tt.dx - innerRadius, (float)-tt.dy - innerRadius, innerRadius * 2f, innerRadius * 2f);
                        this.drawRadarCircle(g, rect, b, p);
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
                var analyzed = game.systemBody?.organisms?.FirstOrDefault(_ => _.genus == scan.genus)?.analyzed == true;
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
            rect = PlotBase.scaled(rect);
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

            var sz = PlotBase.scaled(6);
            x += txtSz.Width + PlotBase.scaled(8);
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            var dd = new TrackingDelta(game.systemBody!.radius, location);

            Angle deg = dd.angle - game.status!.Heading;
            var ten = PlotBase.scaled(10f);
            var dx = (float)Math.Sin(Util.degToRad(deg)) * ten;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * ten;
            g.DrawLine(GameColors.penGameOrange2, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += sz * 3;
            g.DrawString(Util.metersToString(dd.distance), GameColors.fontSmall, GameColors.brushGameOrange, x, y);
        }
    }
}
