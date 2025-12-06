using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Diagnostics;
using Res = Loc.PlotJumpInfo;

namespace SrvSurvey.plotters
{
    internal class PlotJumpInfo : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotJumpInfo),
            allowed = allowed,
            ctor = (game, def) => new PlotJumpInfo(game, def),
            defaultSize = new Size(600, 100),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotJumpInfo
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && (
                    // when FSD is charging for a jump, or ...
                    (game.status.FsdChargingJump && game.isMode(GameMode.Flying, GameMode.SuperCruising))
                    // whilst in witch space, jumping to next system
                    || game.fsdJumping
                    // or a keystroke forced it
                    || (PlotJumpInfo.forceShow && game.mode != GameMode.FSS)
                    || (Game.settings.showPlotJumpInfoIfNextHop && game.destinationNextRouteHop && game.isMode(GameMode.SuperCruising))
                );
        }

        #endregion

        public static bool forceShow = false;

        private NetSysData netData;
        private int nextHopIdx;
        private List<float> hopDistances = new List<float>();
        private List<bool> hopScoops = new List<bool>();
        private float totalDistance;

        private PlotJumpInfo(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;

            // determine next system
            this.initFromRoute();

            // make sure these are closed
            PlotBase2.remove(PlotBioStatus.def);
            PlotBase2.remove(PlotGuardianStatus.def);
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (status.changed.Contains(StatusFlags.FsdCharging.ToString()))
                this.invalidate();
        }

        private void initFromRoute()
        {
            // take the current route, without the first (current) system, but keep that for below
            var routeStart = game.navRoute.Route.FirstOrDefault() ?? RouteEntry.from(game.systemData);
            var route = game.navRoute.Route.Skip(1).ToList();

            RouteEntry? next = null;
            var onNetData = (NetSysData.Source source, NetSysData netData2) =>
            {
                if (next != null)
                {
                    if (next.StarClass == null && netData2.starClass != null)
                        next.StarClass = netData2.starClass;

                    // if we were waiting on the starPos for the next system, populate it and recalculate hop distances too
                    if (next.StarPos == null && netData2.starPos != null)
                        next.StarPos = netData2.starPos;

                    if (this.hopDistances.Count == 0)
                        this.calcHopDistances(route);
                }

                //this.netData = netData2;
                this.invalidate();
            };

            // proceed with either FSDTarget or status.Destination, or exit early if none
            if (game.fsdTarget != null)
            {
                this.netData = NetSysData.get(game.fsdTarget.Name!, game.fsdTarget.SystemAddress, onNetData);
            }
            else if (game.status.Destination?.Name != null && game.status.Destination.System > 0 && game.status.Destination.Body == 0)
            {
                this.netData = NetSysData.get(game.status.Destination.Name, game.status.Destination.System, onNetData);
            }
            if (this.netData == null)
            {
                Game.log($"Why no next name of address?");
                this.hide();
                Debugger.Break();
                return;
            }

            // find next jump within the route
            next = route.Find(r => r.StarSystem == netData.systemName);
            if (next == null)
            {
                Game.log($"Faking route for: {netData.systemName} ({netData.systemAddress})");

                // create a route entry even though it wasn't in the root ...
                next = new RouteEntry()
                {
                    StarClass = netData.starClass!,
                    // probably no StarPos
                    StarSystem = netData.systemName!,
                    SystemAddress = netData.systemAddress,
                };
                if (next.StarPos == null && netData.starPos != null) next.StarPos = netData.starPos;
                if (next.StarClass == null && game.fsdTarget?.SystemAddress == next.SystemAddress) next.StarClass = game.fsdTarget.StarClass;

                if (game.systemData == null) return;

                // using our current system as the start
                if (routeStart == null || routeStart.SystemAddress != game.systemData.address)
                {
                    routeStart = route.Find(r => r.SystemAddress == game.systemData.address);
                    if (routeStart == null) routeStart = RouteEntry.from(game.systemData);
                }

                // and force a new route from here to there
                route.Clear();
                route.Add(next);
            }
            this.nextHopIdx = route.IndexOf(next);

            // start loading net data
            if (this.netData.starClass == null) this.netData.starClass = next.StarClass;

            // measure distances for each hop of the route, putting the current system first again
            if (routeStart != null) route.Insert(0, routeStart);
            this.calcHopDistances(route);

            // Guardian stuff?
            if (Game.canonn.allRuins == null || Game.canonn.allStructures == null || Game.canonn.allBeacons == null)
            {
                Game.log("Why is allRuins not populated?");
                Debugger.Break();
            }
            else
            {
                // show potential guardian stuff as a special line
                var list = new HashSet<string>();

                var countRuins = Game.canonn.allRuins.Count(r => r.systemAddress == next.SystemAddress);
                if (countRuins > 0) list.Add(Res.GuardianRuins.format(countRuins));

                var countStructures = Game.canonn.allStructures.Count(r => r.systemAddress == next.SystemAddress);
                if (countStructures > 0) list.Add(Res.GuardianStructures.format(countStructures));

                var countBeacons = Game.canonn.allBeacons.Count(r => r.systemAddress == next.SystemAddress);
                if (countBeacons > 0) list.Add(Res.GuardianBeacon);

                if (list.Count > 0)
                    netData.special[Res.SpecialGuardian] = list;
            }

            if (game.cmdr?.route.active == true && game.cmdr?.route.nextHop != null)
            {
                var nextHop = game.cmdr.route.nextHop;
                if (nextHop.id64 == next.SystemAddress || nextHop.name.like(next.StarSystem))
                {
                    var set = netData.special.init(Res.SpecialRouteHop);
                    var lastIdx = game.cmdr.route.hops.IndexOf(nextHop);
                    set.Add(Res.SpecialHopInfo.format(lastIdx + 1, game.cmdr.route.hops.Count));
                    // and show any notes
                    if (nextHop.notes != null)
                        set.Add(nextHop.notes!);
                }
            }

            // are we entering a different galactic region?
            if (next.StarPos != null)
            {
                var nextRegion = EliteDangerousRegionMap.RegionMap.FindRegion(next.StarPos);
                if (nextRegion.Name != GalacticRegions.current)
                    netData.special.init(Res.SpecialNowEntering).Add(nextRegion.Name);
            }
        }

        private void calcHopDistances(List<RouteEntry> route)
        {
            this.hopDistances.Clear();
            this.hopScoops.Clear();

            this.totalDistance = 0;
            for (var n = 1; n < route.Count; n++)
            {
                if (route[n].StarPos == null) continue;

                var d = (float)Util.getSystemDistance(route[n - 1].StarPos, route[n].StarPos);
                this.hopDistances.Add(d);
                this.totalDistance += d;
                var scoopable = route[n].StarClass != null && "KGBFOAM".Contains(route[n].StarClass);
                this.hopScoops.Add(scoopable);
            }
        }

        protected override void onJournalEntry(FSDTarget entry)
        {
            // re-populate from scratch if the target changes and we're open
            if (PlotJumpInfo.forceShow && !game.fsdJumping)
                initFromRoute();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            if (this.netData == null)
            {
                // avoid showing an empty plotter
                this.hide();
                return frame.Size;
            }

            // 1st line the name of the system we are jumping to
            tt.dty += N.two;
            tt.draw(N.eight, Res.NextJump);
            tt.dty -= N.two;

            tt.draw(" " + this.netData.systemName, GameColors.fontMiddleBold);
            if (netData.starClass != null)
                tt.draw(this.width - N.eight, Res.StarClass.format(netData.starClass), netData.starClass == "N" ? GameColors.Cyan : null, null, true);
            tt.newLine(+N.ten, true);

            this.drawJumpLine(g, tt);

            // 2nd line: discovered vs unvisited + discovered and update dates
            tt.dtx = N.eight;
            if (netData.totalBodyCount == 0)
                tt.draw("▶️ " + netData.discoveryStatus, GameColors.Cyan);
            else if (netData.discoveredDate.HasValue)
                tt.draw("▶️ " + Res.DiscoveredBy.format(netData.discoveredBy, netData.discoveredDate.Value.ToCmdrShortDateTime24Hours(true)));

            var lastUpdated = netData.lastUpdated;// ?? netData.spanshSystem?.updated_at.GetValueOrDefault()?.ToLocalTime();
            if (lastUpdated != null && (lastUpdated > netData.discoveredDate || netData.discoveredDate == null))
            {
                if (tt.dtx > N.eight) tt.dtx += N.eight;
                tt.draw("▶️ " + Res.LastUpdated.format(lastUpdated.Value.ToCmdrShortDateTime24Hours(true)));
                //tt.draw(eight, lineTwo, netData.discovered == false ? GameColors.Cyan : null);
                //tt.draw(Game.settings.useLastUpdatedFromSpanshNotEDSM ? "(Spansh)" : "(EDSM)", GameColors.OrangeDim);
            }
            tt.newLine(+N.two, true);

            // traffic (if known)
            var traffic = netData.edsmTraffic?.traffic;
            if (traffic != null && traffic.total > 0)
            {
                var lineThree = $"▶️ " + Res.TrafficInfo.format(traffic.day.ToString("n0"), traffic.week.ToString("n0"), traffic.total.ToString("n0"));
                tt.draw(N.eight, lineThree);
                tt.draw(" (EDSM)", GameColors.OrangeDim);
                tt.newLine(+N.two, true);
            }

            // 3rd line: count of ports, genus, etc
            var POIs = netData.countPOI
                .Where(_ => _.Value > 0)
                .Select(_ => Misc.ResourceManager.GetString($"NetSysData_{_.Key}") + $": {_.Value}");
            if (POIs.Any())
            {
                var lineThree = "▶️ " + string.Join(", ", POIs);
                tt.draw(N.eight, lineThree);
                tt.newLine(+N.two, true);
            }

            // 4th line: anything special
            if (netData.special?.Count > 0)
            {
                foreach (var pair in netData.special)
                {
                    tt.draw(N.eight, $"▶️ {pair.Key}: ", GameColors.Cyan);
                    tt.draw(string.Join(Res.SpecialJoiner + " ", pair.Value), GameColors.Cyan);
                    tt.newLine(+N.two, true);
                }
            }

            // resize height to fit, allow width to be bigger but not smaller
            var sz = tt.pad(+N.six, +N.ten);
            if (this.width > sz.Width) sz.Width = this.width;
            return sz;
        }

        private void drawJumpLine(Graphics g, TextCursor tt)
        {
            if (hopDistances.Count == 0) return;
            tt.dty += N.six;
            // draw text for `#1 of 2` on left, and total distance travelled on the right
            var szLeft = tt.draw(N.eight, Res.JumpCounts.format(nextHopIdx + 1, hopDistances.Count));
            var szRight = tt.draw(this.width - N.eight, Res.JumpDistance.format(totalDistance.ToString("N1")), null, null, true);

            // calc left edge of line + whole line width to fix between rendered text
            var left = szLeft.Width + N.oneEight;
            var lineWidth = this.width - left - szRight.Width - N.oneSix;
            var pixelsPerLY = lineWidth / this.totalDistance;

            // prep pixel coords for parts of the line
            var x = left + lineWidth;
            var y = tt.dty + (szRight.Height / 2) - N.two;

            // draw the whole line if we are travelling a long way (as drawing it in parts looks poor)
            var limitExcessDistance = 1000;
            if (this.totalDistance > limitExcessDistance)
                g.DrawLine(GameColors.Route.penBehind, x, y, x - lineWidth, y);

            // prep rectangles for drawing dots and scoop arcs above them
            var dotRadius = N.five;
            var r = new RectangleF(x - dotRadius, y - dotRadius, dotRadius * 2, dotRadius * 2);
            var r0 = new RectangleF(r.Location, r.Size);
            r0.Inflate(-0.5f, -0.5f);
            var r2 = new RectangleF(r.Location, r.Size);
            r2.Inflate(dotRadius, dotRadius);

            // draw line in pieces, from right to left
            var limitPixelsPerLY = 0.25f;
            var xNow = left;
            for (var n = hopDistances.Count - 1; n >= 0; n--)
            {
                // (before drawing line parts, if dots are too close together) draw a TICK for each system
                if (pixelsPerLY < limitPixelsPerLY)
                {
                    // render scoopable stars a ticker and taller
                    if (hopScoops[n])
                        g.DrawLine(n < nextHopIdx ? GameColors.penGameOrangeDim1 : GameColors.penGameOrange1, x - 1, y - N.six, x - 1, y + N.six);
                    else
                        g.DrawLine(n < nextHopIdx ? GameColors.penGameOrangeDim1 : GameColors.penGameOrange1, x - 1, y - N.three, x - 1, y + N.three);
                }

                var d = hopDistances[n];
                var w = d * pixelsPerLY;

                // draw line part from right to left, highlighting if Neutron and/or if next, ahead or behind
                if (n == nextHopIdx)
                    g.DrawLine(GameColors.Route.penNext4, x - N.one, y, x - w, y);
                else if (n < nextHopIdx && d > game.currentShip.maxJump)
                    g.DrawLine(GameColors.Route.penNeutronBehind, x, y, x - w, y);
                else if (n < nextHopIdx)
                    g.DrawLine(GameColors.Route.penBehind, x, y, x - w, y);
                else if (d > game.currentShip.maxJump && this.totalDistance <= limitExcessDistance)
                    g.DrawLine(GameColors.Route.penNeutronAhead, x - N.two, y, x - w, y);
                else if (d > game.currentShip.maxJump && this.totalDistance > limitExcessDistance)
                    g.DrawLine(GameColors.Route.penNeutronBehind, x - N.two, y, x - w, y);
                else if (this.totalDistance < limitExcessDistance)
                    g.DrawLine(GameColors.Route.penAhead, x, y, x - w, y);

                // (before drawing line parts, if not too close together) draw a DOT for each system
                if (pixelsPerLY > limitPixelsPerLY)
                {
                    if (n < nextHopIdx - 1)
                    {
                        //g.FillEllipse(C.Brushes.black, r);
                        //g.DrawEllipse(C.Pens.orangeDark2, r);
                        g.FillEllipse(C.Brushes.black, r0);
                        g.DrawEllipse(C.Pens.orangeDark3, r0);
                    }
                    else if (n >= nextHopIdx)
                    {
                        g.FillEllipse(GameColors.brushGameOrange, r);
                    }

                    // and render a little arc above scoopable stars
                    if (hopScoops[n])
                    {
                        r2.X = r.X - dotRadius;
                        var b = n < nextHopIdx ? GameColors.penGameOrangeDim2 : GameColors.penGameOrange2;
                        if (n + 1 == nextHopIdx)
                            b = GameColors.penCyan2;
                        g.DrawArc(b, r2, 270 - 40, 80);
                    }
                }

                // save the x value for drawing later
                if (n == nextHopIdx)
                {
                    if (pixelsPerLY < limitPixelsPerLY)
                    {
                        g.DrawLine(GameColors.Route.penNext2, x - N.two, y, x - N.six, y - N.four);
                        g.DrawLine(GameColors.Route.penNext2, x - N.two, y, x - N.six, y + N.four);
                    }
                    else
                    {
                        g.DrawLine(GameColors.Route.penNext2, x - dotRadius, y, x - N.ten - dotRadius, y - N.ten);
                        g.DrawLine(GameColors.Route.penNext2, x - dotRadius, y, x - N.ten - dotRadius, y + N.ten);
                    }
                }
                else if (n + 1 == nextHopIdx)
                {
                    xNow = x;
                }

                r.X -= w;
                r0.X = r.X;
                x -= w;
            }

            // draw the left most the starting dot/tick
            if (this.totalDistance > limitExcessDistance)
            {
                g.DrawLine(GameColors.penGameOrange1, x - 1, y - N.four, x - 1, y + N.four);
            }
            else if (nextHopIdx == 0)
            {
                //r0.X = r.X;
                try
                {
                    g.FillEllipse(C.Brushes.cyanDark, r0);
                    g.DrawEllipse(C.Pens.cyan3, r0);
                }
                catch (Exception ex)
                {
                    // TODO: Why is this happening?
                    Game.log($"?? {r0}\r\n\r\n{ex.Message}");
                    Debugger.Break();
                }
                //g.FillEllipse(C.Brushes.cyanDark, r);
                //g.DrawEllipse(C.Pens.cyan2, r);
            }
            else if (nextHopIdx > 0)
            {
                //r0.X = r.X;
                g.FillEllipse(C.Brushes.black, r0);
                g.DrawEllipse(C.Pens.orangeDark3, r0);
            }


            // finally redraw dot for next jump, as it got clipped by prior rendering
            if (pixelsPerLY < limitPixelsPerLY)
            {
                g.DrawLine(GameColors.Route.penNext2, xNow, y - N.six, xNow, y + N.six);
            }
            else if (nextHopIdx > 0)
            {
                r.X = xNow - dotRadius;
                r0.X = r.X;
                //g.FillEllipse(C.Brushes.cyanDark, r);
                //g.DrawEllipse(C.Pens.cyan2, r);
                g.FillEllipse(C.Brushes.cyanDark, r0);
                g.DrawEllipse(C.Pens.cyan3, r0);

            }

            tt.newLine(+N.six);
        }
    }
}
