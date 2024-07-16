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
                    Game.activeGame.status.FsdChargingJump
                    // whilst in whitch space, jumping to next system
                    || Game.activeGame.mode == GameMode.FSDJumping
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
            nextSystem = game.fsdTarget ?? game.status.Destination.Name;
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
                if (response.Result.stations.Count > 0)
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
            drawTextAt(eight, lineTwo);
            drawTextAt("(EDSM)", GameColors.brushGameOrangeDim);
            newLine(+one, true);

            // traffic (if known)
            if (this.info.traffic != null)
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

            drawTextAt(eight, $"#{nextHopIdx + 1} of {hopDistances.Count}");
            var left = dtx + eight;
            var sz = drawTextAt(this.Width - four, $"{totalDistance.ToString("N1")}LY", null, null, true);

            var lineWidth = this.Width - left - sz.Width - twenty;
            var pixelsPerLY = lineWidth / this.totalDistance;
            //Game.log($"pixelsPerLY: {pixelsPerLY}");

            var dotRadius = five;
            var x = left + lineWidth + four;
            var y = dty + (sz.Height / 2) - two;

            // the whole line
            if (this.totalDistance > 1000)
                g.DrawLine(GameColors.Route.penBehind, x, y, x - lineWidth, y);

            // draw line in pieces, from right to left
            var r = new RectangleF(x - dotRadius, y, dotRadius * 2, dotRadius * 2);
            r.Y -= dotRadius;
            for (var n = hopDistances.Count - 1; n >= 0; n--)
            {
                // right side marker - tick or dot
                if (pixelsPerLY < 0.3)
                    g.DrawLine(n < nextHopIdx ? GameColors.penGameOrangeDim1 : GameColors.penGameOrange1, x - 1, y - four, x - 1, y + four); // tick
                //else
                //    g.FillEllipse(GameColors.brushGameOrange, r); // dot

                var d = hopDistances[n];
                var w = d * pixelsPerLY;

                // draw line to the left (higlight any neutron assisted jumps)
                if (n == nextHopIdx)
                    g.DrawLine(GameColors.Route.penNext, x - one, y, x - w, y);
                else if (n < nextHopIdx && d > game.shipMaxJump)
                    g.DrawLine(GameColors.Route.penNeutronBehind, x, y, x - w, y);
                else if (n < nextHopIdx)
                    g.DrawLine(GameColors.Route.penBehind, x, y, x - w, y);
                else if (d > game.shipMaxJump && this.totalDistance < 1000)
                    g.DrawLine(GameColors.Route.penNeutronAhead, x, y, x - w, y);
                else if (this.totalDistance < 1000)
                    g.DrawLine(GameColors.Route.penAhead, x, y, x - w, y);

                // right side marker - tick or dot
                if (pixelsPerLY > 0.3)
                    g.FillEllipse(GameColors.brushGameOrange, r); // dot


                //var p = (d > 90) ? GameColors.Route.penNeutron : GameColors.Route.penBehind;
                //if (n == nextHopIdx) p = GameColors.Route.penNext;
                //if (n <= nextHopIdx)
                //    g.DrawLine(p, x - 1, y, x - w, y);

                // redraw dot for next jump
                if (n + 1 == nextHopIdx) g.FillEllipse(GameColors.brushCyan, r);

                r.X -= w;
                x -= w;
            }

            // highlight line potion for the next hop
            var x1 = hopDistances.Take(nextHopIdx).Sum() * pixelsPerLY;

            //+ (this.distBeforeCurrentHop * pixelsPerLY);
            var x2 = hopDistances[nextHopIdx] * pixelsPerLY;
            //g.DrawLine(GameColors.penCyan4, left + x1, y, left + x1 + x2, y);

            y -= dotRadius;

            // the starting dot
            g.FillEllipse(nextHopIdx == 0 ? GameColors.brushCyan : GameColors.brushGameOrange, r);

            newLine(+four);
        }

    }

    internal class JumpInfo
    {
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
