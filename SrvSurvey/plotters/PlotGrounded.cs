using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
using Res = Loc.PlotGrounded;

namespace SrvSurvey.plotters
{
    internal partial class PlotGrounded : PlotBase2Surface
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotGrounded),
            allowed = allowed,
            ctor = (game, def) => new PlotGrounded(game, def),
            defaultSize = new Size(320, 440),
            invalidationJournalEvents = new HashSet<string>() { nameof(Disembark), nameof(Embark), nameof(Liftoff), nameof(Touchdown) },
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowBioPlot
                && game.systemBody != null
                && !game.status.Docked
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && game.status.hasLatLong
                && game.status.Altitude < 10_000
                && !game.status.InTaxi
                && !game.status.FsdChargingJump
                && (!Game.settings.enableGuardianSites || !PlotGuardians.allowed(game))
                && (!Game.settings.autoShowHumanSitesTest || !PlotHumanSite.allowed(game))
                && (game.systemBody.bioSignalCount > 0 || game.systemBody.bookmarks?.Count > 0)
                && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.RolePanel)
                && (!Game.settings.autoHideBioPlotNoGear || game.mode != GameMode.Flying || game.status.landingGearDown)
                ;
            // TODO: include these?
            // if (game.systemSite == null && !game.isMode(GameMode.SuperCruising, GameMode.GlideMode) && (game.isLanded || showPlotTrackers || showPlotPriorScans || game.cmdr.scanOne != null))
        }

        private static Size getWindowSize()
        {
            switch (Game.settings.bioPlotSize)
            {
                case 0: return new Size(250, 400);
                case 1: return new Size(250, 500);
                default:
                case 2: return new Size(320, 440);
                case 3: return new Size(380, 500);
                case 4: return new Size(440, 600);
            }
        }

        #endregion

        private PlotGrounded(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;
            this.scale = 0.25f;

            // we use a preset size
            this.setSize(getWindowSize());
            this.background = GameGraphics.getBackgroundImage(this.size);

            // get landing location
            Game.log($"initialize here: {Status.here}, touchdownLocation: {game.touchdownLocation}, radius: {game.systemBody!.radius.ToString("N0")}");

            // reposition trackers if it already exists // Revisit !!!
            var plotTrackers = PlotBase2.getPlotter<PlotTrackers>();
            if (plotTrackers != null)
                Program.defer(() => plotTrackers.setPosition(null));
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            if (game.systemBody == null || game.status == null) return this.size;

            // draw the background inline
            g.DrawImage(background, 0, 0);

            // clip to preserve top/bottom text area
            var bottomClip = game.cmdr.scanOne == null ? N.threeFour : N.fiveTwo;
            g.Clip = new Region(new RectangleF(N.four, N.twoFour, this.width - N.eight, this.height - bottomClip));

            // draw basic compass cross hairs centered in the window
            this.resetMiddle(g);
            this.drawCompassLines(g);

            this.drawBioScans(g);

            this.resetMiddleRotated(g);
            // draw touchdown marker (may reset transforms)
            this.drawShipAndSrvLocation(g, tt);

            // draw current location pointer (always at center of plot + unscaled)
            this.resetMiddle(g);
            base.drawCommander(g);

            g.ResetTransform();

            // draw fade bars top and bottom
            var tp = new Pen(Color.FromArgb(128, 0, 0, 0), 4);
            g.DrawLine(tp, 0, N.twoFour + 1, this.width, N.twoFour + 1);
            g.DrawLine(tp, 0, this.height - 2 - bottomClip + N.twoFour, this.width, this.height - 2 - bottomClip + N.twoFour);

            // remove clip to preserving top/bottom text area
            g.ResetClip();

            if (game.cmdr.lastTouchdownLocation != null)
                this.drawBearingTo(g, tt, N.eight, N.oneOne, Res.Ship, game.cmdr.lastTouchdownLocation);

            if (game.srvLocation != null)
                this.drawBearingTo(g, tt, N.four + mid.Width, N.oneOne, Res.SRV, game.srvLocation);

            float y = this.height - N.twoTwo;
            if (game.cmdr.scanOne != null)
            {
                if (game.cmdr.scanOne.body != game.systemBody.name)
                    g.DrawString(Res.ScanOneInvalid, GameColors.fontSmall, GameColors.brushRed, N.ten, y);
                else
                    this.drawBearingTo(g, tt, N.ten, y, this.width < 280 ? Res.ScanOne_Short : Res.ScanOne_Long, game.cmdr.scanOne.location);
                // TODO:                                     Measure ^^^ based on strings
            }

            if (game.cmdr.scanTwo != null)
            {
                if (game.cmdr.scanTwo.body != game.systemBody.name)
                    g.DrawString(Res.ScanTwoInvalid, GameColors.fontSmall, GameColors.brushRed, N.ten + mid.Width, y);
                else
                    this.drawBearingTo(g, tt, N.ten + mid.Width, y, this.width < 280 ? Res.ScanTwo_Short : Res.ScanTwo_Long, game.cmdr.scanTwo.location);
                // TODO:                                                 Measure ^^^ based on strings
            }

            return this.size;
        }

        private void drawBioScans(Graphics g)
        {
            if (game.systemBody == null) return;

            this.resetMiddleRotated(g);

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
                drawBioScan(g, d, game.cmdr.scanOne);

            if (game.cmdr.scanTwo != null)
                drawBioScan(g, d, game.cmdr.scanTwo);
        }

        private void drawPriorScans(Graphics g)
        {
            var form = PlotBase2.getPlotter<PlotPriorScans>();
            if (game.systemBody == null || form == null) return;

            foreach (var signal in form.signals)
            {
                var analyzed = game.systemBody.organisms?.FirstOrDefault(_ => _.species == signal.match.species.name)?.analyzed == true;
                var isActive = (game.cmdr.scanOne?.genus == null && !analyzed) || game.cmdr.scanOne?.species == signal.match.species.name;

                // default range to 50m unless name matches a Genus
                var radius = signal.match.genus.dist;

                // draw radar circles for this group, and lines
                foreach (var tt in signal.trackers)
                {
                    if (Util.isCloseToScan(tt.Target, signal.match.species.name) || analyzed) continue;

                    var b = isActive ? GameColors.PriorScans.Active.brush : GameColors.PriorScans.Inactive.brush;

                    var p = isActive ? GameColors.penTrackerActive : GameColors.penTrackerInactive;

                    var c = isActive ? GameColors.PriorScans.Active.color : GameColors.PriorScans.Inactive.color;

                    if (tt.distance < PlotTrackers.highlightDistance)
                    {
                        b = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush;
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
            var form = PlotBase2.getPlotter<PlotTrackers>();
            if (game.systemBody?.bookmarks == null || form == null) return;

            foreach (var name in form.trackers.Keys)
            {
                var isActive = /* game.cmdr.scanOne?.genus == null || */ game.cmdr.scanOne?.genus == name;

                // default range to 50m unless name matches a Genus
                var radius = BioGenus.getRange(name);

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
                        if (game.cmdr.scanOne == null || game.cmdr.scanOne.genus == name)
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
            if (scan.body != null && scan.body != game.systemBody?.name) return;

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
                b = GameColors.brushExclusionAbandoned;
                p = GameColors.penExclusionAbandoned;

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

        private void drawBearingTo(Graphics g, TextCursor tt, float x, float y, string txt, LatLong2 location)
        {
            // draw prefix
            tt.dtx = x;
            tt.dty = y;
            var txtSz = tt.draw(txt, C.orange, GameColors.fontSmall);

            x += txtSz.Width + N.four;
            var deg = (double)Util.getBearing(Status.here, location) - game.status!.Heading;
            var dist = Util.getDistance(Status.here, location, game.systemBody!.radius);
            var msg = Util.metersToString(dist);
            BaseWidget.renderBearingTo(g, x, y, N.five, deg, msg);
        }
    }
}
