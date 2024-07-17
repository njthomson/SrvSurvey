using SrvSurvey.game;
using SrvSurvey.net.EDSM;

namespace SrvSurvey
{
    internal class PlotJumpInfo : PlotBase, PlotterForm
    {
        private static Dictionary<string, JumpInfo> systemsCache = new Dictionary<string, JumpInfo>();

        private string nextSystem;
        private RouteInfo nextHop;
        private int nextHopIdx;
        private List<float> hopDistances = new List<float>();
        private List<bool> hopScoops = new List<bool>();
        private float totalDistance;

        private PlotJumpInfo() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall;
        }

        private JumpInfo info { get => systemsCache[nextSystem]; }

        public override bool allow { get => PlotJumpInfo.allowPlotter; }

        public static bool allowPlotter
        {
            get => Game.activeGame?.status != null
                && Game.settings.autoShowPlotJumpInfoTest
                && (
                    // when FSD is charging for a jump, or ...
                    (Game.activeGame.status.FsdChargingJump && Game.activeGame.isMode(GameMode.Flying, GameMode.SuperCruising))
                    // whilst in whitch space, jumping to next system
                    || Game.activeGame.mode == GameMode.FSDJumping
                    // || Game.activeGame.mode == GameMode.RolePanel // debugging helper
                );
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(540);
            this.Height = scaled(88);
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            // determine next system
            if (game.navRoute.Route.Count > 1)
                this.initFromRoute();
        }

        private void initFromRoute()
        {
            var route = game.navRoute.Route.Skip(1).ToList();

            // find next hop in route
            nextSystem = game.fsdTarget ?? game.status.Destination?.Name;
            var next = route.Find(r => r.StarSystem == nextSystem);
            if (next == null) return;
            this.nextHop = RouteInfo.create(next, false);
            this.nextHopIdx = route.IndexOf(next);

            // measure distances for each hop of the route
            this.hopDistances.Clear();
            this.totalDistance = 0;
            route = game.navRoute.Route;
            for (var n = 1; n < route.Count; n++)
            {
                var d = (float)Util.getSystemDistance(route[n - 1].StarPos, route[n].StarPos);
                this.hopDistances.Add(d);
                this.totalDistance += d;
                var scoopable = "KGBFOAM".Contains(route[n].StarClass);
                this.hopScoops.Add(scoopable);
            }

            // did we get POIs for this system already?
            if (systemsCache.ContainsKey(nextSystem))
            {
                // ... yes
                this.Invalidate();
                return;
            }

            systemsCache[nextSystem] = new JumpInfo();

            // make EDSM request for next system traffic
            Game.edsm.getSystemTraffic(next.StarSystem).ContinueWith(response => Program.crashGuard(() =>
            {
                if (response.Exception != null)
                {
                    Util.isFirewallProblem(response.Exception);
                    return;
                }

                this.info.traffic = response.Result;
                Program.control.BeginInvoke(() => this.Invalidate());
            }));

            // make EDSM request for system stations
            Game.edsm.getSystemStations(next.StarSystem).ContinueWith(response => Program.crashGuard(() =>
            {
                if (response.Exception != null)
                {
                    Util.isFirewallProblem(response.Exception);
                    return;
                }

                // how many stations are there?
                if (response.Result.stations?.Count > 0)
                {
                    var countFC = 0;
                    var countSettlements = 0;
                    var countStarports = 0;
                    var countOutposts = 0;
                    foreach (var sta in response.Result.stations)
                    {
                        if (sta.type == "Drake-Class Carrier") countFC++;
                        if (sta.type == "Odyssey Settlement") countSettlements++;
                        if (sta.type == "Outpost") countOutposts++;
                        if (EdsmSystemStations.Starports.Contains(sta.type)) countStarports++;
                        // Include dockable mega ships with Starports
                        if (sta.type == "Mega ship" && (sta.haveMarket || sta.haveShipyard || sta.haveOutfitting || sta.otherServices?.Count > 0)) countStarports++;
                    }

                    var parts = new List<string>();
                    if (countStarports > 0) this.info.countPOI["FC"] = countFC;
                    if (countSettlements > 0) this.info.countPOI["Settlements"] = countSettlements;
                    if (countStarports > 0) this.info.countPOI["Outposts"] = countStarports;
                    if (countStarports > 0) this.info.countPOI["Star ports"] = countStarports;

                    Program.control.BeginInvoke(() => this.Invalidate());
                }
            }));

            // make Canonn request for exo biology
            Game.canonn.systemBioStats(next.SystemAddress).ContinueWith(response => Program.crashGuard(() =>
            {
                if (response.Exception != null)
                {
                    Util.isFirewallProblem(response.Exception);
                    return;
                }

                if (response.Result.date != null && response.Result.bodies.Count > 0)
                    systemsCache[nextSystem].lastUpdated = response.Result.date;

                foreach (var body in response.Result.bodies)
                {
                    var bioSignals = body.signals?.signals?.GetValueOrDefault("$SAA_SignalType_Biological;") ?? 0;
                    if (bioSignals > 0) this.info.countPOI["Genus"] += bioSignals;
                }

                Program.control.BeginInvoke(() => this.Invalidate());
            }));
        }

        protected override void onJournalEntry(FSDJump entry)
        {
            // remove this plotter once we arrive in some system
            Program.closePlotter<PlotJumpInfo>();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (nextHop == null) return;

            // 1st line the name of the system we are jumping to
            dty += two;
            drawTextAt(eight, $"Next jump: ");
            dty -= two;
            if (nextHop == null) return;

            drawTextAt(nextHop.systemName, GameColors.fontMiddleBold);
            newLine(+two, true);

            this.drawJumpLine();

            // 2nd line: discovered, unvisited, etc
            var lineTwo = string.IsNullOrEmpty(nextHop.subStatus)
                ? "► " + nextHop.status
                : "► Discovered by" + nextHop.subStatus.Substring(2);

            var lastUpdated = systemsCache[nextSystem].lastUpdated;
            if (lastUpdated != null && lastUpdated.Value != nextHop.discoveredDate)
                lineTwo += $", last updated: " + lastUpdated.Value.ToShortDateString();
            drawTextAt(eight, lineTwo);
            drawTextAt("(EDSM)", GameColors.brushGameOrangeDim);
            newLine(+one, true);

            // traffic (if known)
            if (this.info.traffic?.traffic != null && this.info.traffic.traffic.total > 0)
            {
                var lineThree = $"► Traffic last 24 hours: {this.info.traffic.traffic.day.ToString("n0")}, week: {this.info.traffic.traffic.week.ToString("n0")}, ever: {this.info.traffic.traffic.total.ToString("n0")}";
                drawTextAt(eight, lineThree);
                drawTextAt("(EDSM)", GameColors.brushGameOrangeDim);
                newLine(+one, true);
            }

            // 3rd line: POI: ports, genus, etc
            var POIs = this.info.countPOI
                .Where(_ => _.Value > 0)
                .Select(_ => $"{_.Key}: {_.Value}");
            if (POIs.Any())
            {
                var lineFive = "► POI: " + string.Join(", ", POIs);
                drawTextAt(eight, lineFive);
                newLine(+one, true);
            }

            // resize height to fit - width will not change
            this.formSize.Width = this.Width;
            this.formAdjustSize(0, +ten);
        }

        private void drawJumpLine()
        {
            if (hopDistances.Count == 0) return;
            dty += six;
            // draw text for `#1 of 2` on left, and total distance travelled on the right
            var szLeft = drawTextAt(eight, $"#{nextHopIdx + 1} of {hopDistances.Count}");
            var szRight = drawTextAt(this.Width - four, $"{totalDistance.ToString("N1")}LY", null, null, true);

            // calc left edge of line + whole line width to fix between rendered text
            var left = szLeft.Width + oneSix;
            var lineWidth = this.Width - left - szRight.Width - twenty;
            var pixelsPerLY = lineWidth / this.totalDistance;

            // prep pixel coords for parts of the line
            var x = left + lineWidth + four;
            var y = dty + (szRight.Height / 2) - two;

            // draw the whole line if we are travelling a long way (as drawing it in parts looks poor)
            var limitExcessDistance = 1000;
            if (this.totalDistance > limitExcessDistance)
                g.DrawLine(GameColors.Route.penBehind, x, y, x - lineWidth, y);

            // prep rectangles for drawing dots and scoop arcs above them
            var dotRadius = five;
            var r = new RectangleF(x - dotRadius, y - dotRadius, dotRadius * 2, dotRadius * 2);
            var r2 = new RectangleF(r.Location, r.Size);
            r2.Inflate(dotRadius, dotRadius);

            // draw line in pieces, from right to left
            var limitPixelsPerLY = 0.3f;
            for (var n = hopDistances.Count - 1; n >= 0; n--)
            {
                // (before drawing line parts, if dots are too close together) draw a TICK for each system
                if (pixelsPerLY < limitPixelsPerLY)
                {
                    // render scoopable stars a ticker and taller
                    if (hopScoops[n])
                        g.DrawLine(n < nextHopIdx ? GameColors.penGameOrangeDim1 : GameColors.penGameOrange1, x - 1, y - six, x - 1, y + six);
                    else
                        g.DrawLine(n < nextHopIdx ? GameColors.penGameOrangeDim1 : GameColors.penGameOrange1, x - 1, y - four, x - 1, y + four);
                }

                var d = hopDistances[n];
                var w = d * pixelsPerLY;

                // draw line part from right to left, highlighting if Neutron and/or if next, ahead or behind
                if (n == nextHopIdx)
                    g.DrawLine(GameColors.Route.penNext, x - one, y, x - w, y);
                else if (n < nextHopIdx && d > game.shipMaxJump)
                    g.DrawLine(GameColors.Route.penNeutronBehind, x, y, x - w, y);
                else if (n < nextHopIdx)
                    g.DrawLine(GameColors.Route.penBehind, x, y, x - w, y);
                else if (d > game.shipMaxJump && this.totalDistance <= limitExcessDistance)
                    g.DrawLine(GameColors.Route.penNeutronAhead, x - two, y, x - w, y);
                else if (d > game.shipMaxJump && this.totalDistance > limitExcessDistance)
                    g.DrawLine(GameColors.Route.penNeutronBehind, x - two, y, x - w, y);
                else if (this.totalDistance < limitExcessDistance)
                    g.DrawLine(GameColors.Route.penAhead, x, y, x - w, y);

                // (before drawing line parts, if not too close together) draw a DOT for each system
                if (pixelsPerLY > limitPixelsPerLY)
                {
                    g.FillEllipse(GameColors.brushGameOrange, r);

                    // and render a little arc above scoopable stars
                    if (hopScoops[n])
                    {
                        r2.X = r.X - dotRadius;
                        g.DrawArc(n + 1 == nextHopIdx ? GameColors.penCyan2 : GameColors.penGameOrange2, r2, 270 - 40, 80);
                    }
                }

                // redraw dot for next jump, as it got clipped by a line on the last iteration
                if (n + 1 == nextHopIdx) g.FillEllipse(GameColors.brushCyan, r);

                r.X -= w;
                x -= w;
            }

            // finally, draw the left most the starting dot
            g.FillEllipse(nextHopIdx == 0 ? GameColors.brushCyan : GameColors.brushGameOrange, r);

            newLine(+four);
        }

    }

    internal class JumpInfo
    {
        public DateTime? lastUpdated;

        public EdsmSystemTraffic? traffic;

        public Dictionary<string, int> countPOI = new Dictionary<string, int>()
        {
            // (these are rendered in this order)
            { "Genus", 0 }, // Total count for the system
            { "Star ports", 0 }, // Anything with a large pad
            { "Outposts", 0 },
            { "Settlements", 0 }, // Odyssey settlements
            { "FC", 0 }, // Fleet carriers
        };
    }
}
