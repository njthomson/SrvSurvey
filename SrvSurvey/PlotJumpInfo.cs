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
            // Size set during paint

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            // determine next system
            if (game.fsdTarget != null || game.navRoute.Route.Count > 1)
                this.initFromRoute();

            // make sure these are closed
            Program.closePlotter<PlotBioStatus>();
            Program.closePlotter<PlotGuardianStatus>();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // we may have closed plotters on opening, force a mode change event to bring them back
            game.fireUpdate(true);
        }

        private void initFromRoute()
        {
            var route = game.navRoute.Route.Skip(1).ToList();

            if (route.Count == 0 && game.fsdTarget?.Name != null)
            {
                Game.log($"Faking route for: {game.fsdTarget.Name}");

                // create a route if we have a single hop
                var destination = new RouteEntry()
                {
                    StarClass = game.fsdTarget.StarClass,
                    // no StarPos it is not part of FSDTarget journal entries
                    StarSystem = game.fsdTarget.Name,
                    SystemAddress = game.fsdTarget.SystemAddress,
                };
                route.Add(destination);
            }

            // find next hop in route
            this.nextSystem = game.fsdTarget?.Name ?? game.status.Destination?.Name!;
            if (this.nextSystem == null) return;

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
                if (this.totalDistance == 0)
                    this.calculateSingleHopDistances();
                return;
            }

            systemsCache[nextSystem] = new JumpInfo();

            // make EDSM request for next system traffic
            Game.edsm.getSystemTraffic(next.StarSystem).ContinueWith(task => Program.crashGuard(() =>
            {
                if (task.Exception != null || !task.IsCompletedSuccessfully)
                {
                    Util.isFirewallProblem(task.Exception);
                    return;
                }

                this.info.traffic = task.Result;
                Program.control.BeginInvoke(() => this.Invalidate());
            }));

            // make Spansh request to get faction states and ports
            Game.spansh.getSystem(next.SystemAddress).ContinueWith(task => Program.crashGuard(() =>
            {
                if (task.Exception != null || !task.IsCompletedSuccessfully)
                {
                    Util.isFirewallProblem(task.Exception);
                    return;
                }

                var data = task.Result;
                if (data == null) return;

                // last updated
                if (data.updated_at != null && data.bodies?.Count > 0)
                    systemsCache[nextSystem].lastUpdated = data.updated_at;

                // how many stations are there?
                if (data.stations?.Count > 0)
                {
                    var countFC = 0;
                    var countSettlements = 0;
                    var countStarports = 0;
                    var countOutposts = 0;
                    foreach (var sta in data.stations)
                    {
                        if (sta.type == "Drake-Class Carrier") countFC++;
                        if (sta.type == "Settlement") countSettlements++;
                        if (sta.type == "Outpost") countOutposts++;
                        if (EdsmSystemStations.Starports.Contains(sta.type)) countStarports++;
                        // Include dockable mega ships with shipyards
                        if (sta.type == "Mega ship" && sta.has_shipyard) countStarports++;
                    }

                    var parts = new List<string>();
                    if (countStarports > 0) this.info.countPOI["FC"] = countFC;
                    if (countSettlements > 0) this.info.countPOI["Settlements"] = countSettlements;
                    if (countOutposts > 0) this.info.countPOI["Outposts"] = countOutposts;
                    if (countStarports > 0) this.info.countPOI["Star ports"] = countStarports;

                    Program.control.BeginInvoke(() => this.Invalidate());
                }

                // Any factions at war?
                var countWars = data.minor_faction_presences?.Count(f => f.state == "War" || f.state == "Civil War") ?? 0;
                if (countWars > 0)
                    this.info.countPOI["Wars"] = countWars / 2;

                Program.control.BeginInvoke(() => this.Invalidate());
            }));

            // make Canonn request for exo biology
            Game.canonn.systemBioStats(next.SystemAddress).ContinueWith(task => Program.crashGuard(() =>
            {
                if (task.Exception != null || !task.IsCompletedSuccessfully)
                {
                    Util.isFirewallProblem(task.Exception);
                    return;
                }

                if (task?.Result == null) return;

                foreach (var body in task.Result.bodies)
                {
                    var bioSignals = body.signals?.signals?.GetValueOrDefault("$SAA_SignalType_Biological;") ?? 0;
                    if (bioSignals > 0) this.info.countPOI["Genus"] += bioSignals;
                }

                // inject missing StarPos if needed
                if (nextHop.entry.StarPos == null)
                {
                    nextHop.entry.StarPos = task.Result.coords;
                    this.calculateSingleHopDistances();
                }

                //// last updated
                //if (task.Result.date != null && task.Result.bodies?.Count > 0)
                //    systemsCache[nextSystem].lastUpdated = task.Result.date;

                Program.control.BeginInvoke(() => this.Invalidate());
            }));

            // Guardian stuff?
            var countRuins = Game.canonn.loadAllRuins().Count(r => r.systemAddress == next.SystemAddress);
            var countStructures = Game.canonn.loadAllStructures().Count(r => r.systemAddress == next.SystemAddress);
            var countBeacons = Game.canonn.loadAllBeacons().Count(r => r.systemAddress == next.SystemAddress);
            this.info.countPOI["Guardian"] = countRuins + countStructures + countBeacons;
            if (this.info.countPOI["Guardian"] > 0) this.Invalidate();
        }

        private void calculateSingleHopDistances()
        {
            // measure distance from current system to next
            this.hopDistances.Clear();

            var d = (float)Util.getSystemDistance(game.cmdr.starPos, nextHop.entry.StarPos);
            this.hopDistances.Add(d);
            this.totalDistance = d;
            var scoopable = "KGBFOAM".Contains(nextHop.entry.StarClass);
            this.hopScoops.Add(scoopable);
        }


        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.nextHop == null)
            {
                // avoid showing an empty plotter
                this.Opacity = 0;
                return;
            }

            // 1st line the name of the system we are jumping to
            dty += two;
            drawTextAt(eight, $"Next jump: ");
            dty -= two;

            drawTextAt(this.nextHop.systemName, GameColors.fontMiddleBold);
            drawTextAt(this.Width - eight, $"class: {nextHop.entry.StarClass}", nextHop.entry.StarClass == "N" ? GameColors.brushCyan : null, null, true);
            newLine(+eight, true);

            this.drawJumpLine();

            // 2nd line: discovered vs unvisited + discovered and update dates
            var lineTwo = string.IsNullOrEmpty(nextHop.subStatus)
                ? "► " + nextHop.status
                : "► Discovered by" + nextHop.subStatus.Substring(2);

            var lastUpdated = systemsCache[nextSystem].lastUpdated;
            if (lastUpdated != null && lastUpdated.Value > nextHop.discoveredDate)
                lineTwo += $", last updated: " + lastUpdated.Value.ToLocalTime().ToString("d");
            drawTextAt(eight, lineTwo, nextHop.highlight ? GameColors.brushCyan : null);
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
            var szRight = drawTextAt(this.Width - eight, $"{totalDistance.ToString("N1")}LY", null, null, true);

            // calc left edge of line + whole line width to fix between rendered text
            var left = szLeft.Width + oneFour;
            var lineWidth = this.Width - left - szRight.Width - oneSix;
            var pixelsPerLY = lineWidth / this.totalDistance;

            // prep pixel coords for parts of the line
            var x = left + lineWidth;
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
            var limitPixelsPerLY = 0.25f;
            var xNow = left;
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
                    g.DrawLine(GameColors.Route.penNext4, x - one, y, x - w, y);
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
                    g.FillEllipse(n < nextHopIdx ? GameColors.brushGameOrangeDim : GameColors.brushGameOrange, r);

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
                        g.DrawLine(GameColors.Route.penNext2, x - two, y, x - oneTwo, y - ten);
                        g.DrawLine(GameColors.Route.penNext2, x - two, y, x - oneTwo, y + ten);
                    }
                    else
                    {
                        g.DrawLine(GameColors.Route.penNext2, x - dotRadius, y, x - ten - dotRadius, y - ten);
                        g.DrawLine(GameColors.Route.penNext2, x - dotRadius, y, x - ten - dotRadius, y + ten);
                    }
                }
                else if (n + 1 == nextHopIdx)
                {
                    xNow = x;
                }

                r.X -= w;
                x -= w;
            }

            // draw the left most the starting dot/tick
            if (this.totalDistance > limitExcessDistance)
                g.DrawLine(GameColors.penGameOrange1, x - 1, y - four, x - 1, y + four);
            else
                g.FillEllipse(nextHopIdx == 0 ? GameColors.brushCyan : GameColors.brushGameOrangeDim, r);


            // finally redraw dot for next jump, as it got clipped by prior rendering
            if (pixelsPerLY < limitPixelsPerLY)
            {
                g.DrawLine(GameColors.Route.penNext2, xNow, y - six, xNow, y + six);
            }
            else if (nextHopIdx > 0)
            {
                r.X = xNow - dotRadius;
                g.FillEllipse(GameColors.brushCyan, r);
            }

            newLine(+four);
        }

    }

    internal class JumpInfo
    {
        public DateTimeOffset? lastUpdated;

        public EdsmSystemTraffic? traffic;

        public Dictionary<string, int> countPOI = new Dictionary<string, int>()
        {
            // (these are rendered in this order)
            { "Guardian", 0 }, // Total count for the system
            { "Genus", 0 }, // Total count for the system
            { "Star ports", 0 }, // Anything with a large pad
            { "Outposts", 0 },
            { "Settlements", 0 }, // Odyssey settlements
            { "FC", 0 }, // Fleet carriers
            { "Wars", 0 }, // Count of wars - (Count of factions in War or Civil-War state / 2)
        };

        //public RouteInfo info;
    }
}
