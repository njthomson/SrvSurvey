﻿using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotGalMap : PlotBase, PlotterForm
    {
        private static bool smaller = true; // temp?
        private static GraphicsPath triangle;

        static PlotGalMap()
        {
            triangle = new GraphicsPath();
            triangle.AddPolygon(new Point[]
            {
                scaled(new Point(0, 0)),
                scaled(new Point(0, smaller ? 20 : 36)),
                scaled(new Point(smaller ? 8 : 12,  smaller ? 10 : 18)),
            });
        }

        private List<RouteInfo> hops = new List<RouteInfo>();
        private double distanceJumped;
        private string destinationName;

        private PlotGalMap() : base()
        {
            this.Font = GameColors.fontSmall2;
            this.destinationName = game.status.Destination?.Name;
        }

        public override bool allow { get => PlotGalMap.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            // Size set during paint

            base.OnLoad(e);

            this.initializeOnLoad();

            this.reposition(Elite.getWindowRect(true));

            // show existing route, otherwise data for current system
            if (game.navRoute.Route.Count > 1)
                this.onJournalEntry(new NavRoute());
            else if (game.systemData != null)
                this.hops.Add(RouteInfo.create(RouteEntry.from(game.systemData), true));
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap
                && Game.settings.useExternalData;
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game.systemData == null) return;
            base.Status_StatusChanged(blink);

            // if the destination changed ...
            if (game.status.Destination != null && this.destinationName != game.status.Destination.Name)
            {
                this.destinationName = game.status.Destination.Name;

                this.distanceJumped = 0;
                this.hops.Clear();
                var destintation = new RouteEntry()
                {
                    SystemAddress = game.status.Destination.System,
                    StarSystem = game.status.Destination.Name
                };
                this.hops.Add(RouteInfo.create(destintation, true));
                this.Invalidate();
            }
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (this.IsDisposed) return;
            this.hops.Clear();

            // lookup if target system has been discovered
            if (game.navRoute.Route.Count < 2)
                return;

            // the destination is always last entry
            this.hops.Add(RouteInfo.create(game.navRoute.Route.Last(), true));

            // find next hop by fsdTarget?
            var next = game.fsdTarget != null
                ? game.navRoute.Route.Find(_ => _.StarSystem == game.fsdTarget?.Name) ?? game.navRoute.Route[1]
                : game.navRoute.Route[1];

            if (game.navRoute.Route.Count > 2 && next.StarSystem != hops.FirstOrDefault()?.systemName)
            {
                this.destinationName = next.StarSystem;
                this.hops.Add(RouteInfo.create(next, false));
            }

            this.distanceJumped = 0;
            for (int n = 1; n < game.navRoute.Route.Count; n++)
            {
                var d = Util.getSystemDistance(game.navRoute.Route[n - 1].StarPos, game.navRoute.Route[n].StarPos);
                this.distanceJumped += d;
            }

            //var target = game.navRoute.Route.LastOrDefault()?.StarSystem;
            //this.hops.Add(new RouteInfo(target, this));

            //var next = game.navRoute.Route.Count == 0 ? null : game.navRoute.Route[1].StarSystem;
            //this.hops.Add(new RouteInfo(next, this));

            //if (target != null)
            //    this.lookupSystem(target, false);
            //else
            //{
            //    this.targetSystem = null;
            //    this.targetStatus = null;
            //    this.targetSubStatus = null;
            //}

            //if (next != null && target != next)
            //    this.lookupSystem(next, true);
            //else
            //{
            //    this.nextSystem = null;
            //    this.nextStatus = null;
            //    this.nextSubStatus = null;
            //}
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (this.IsDisposed) return;
            this.hops.Clear();

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.hops.Count == 0)
            {
                // TODO: keep hidden until we have a route?
                this.drawTextAt(eight, $"No route set");
                this.newLine(+ten, true);
            }
            else
            {
                foreach (var hop in this.hops)
                    drawSystemSummary(hop);// "Next jump", nextSystem, nextStatus, nextSubStatus);

                if (this.distanceJumped > 0)
                {
                    this.drawTextAt(eight, $"Total jumps: {game.navRoute.Route.Count - 1} ► Distance: {this.distanceJumped.ToString("N1")} ly", GameColors.brushGameOrange);
                    this.newLine(true);
                }

                this.drawTextAt(eight, $"Data from: edsm.net + spansh.co.uk", GameColors.brushGameOrangeDim);
                this.newLine(true);
            }

            this.formAdjustSize(+ten, +ten);
        }

        private void drawSystemSummary(RouteInfo hop)
        {
            var header = "Next jump:";

            if (hop.entry.SystemAddress == game.systemData?.address)
                header = "Current:";
            else if (hops.Count == 1)
                header = "Selected:";
            else if (hop.destination)
                header = "Destination:";

            this.drawTextAt(eight, header);

            // line 1: system name
            this.drawTextAt(eightSix, $"► {hop.systemName}", GameColors.fontSmall2Bold);
            this.newLine(true);

            // line 2: status
            if (hop.highlight)
                this.drawTextAt(eightSix, $"{hop.status}", GameColors.brushCyan, GameColors.fontSmall2Bold);
            else
                this.drawTextAt(eightSix, $"{hop.status}");

            // line 3: who discovered
            if (!string.IsNullOrEmpty(hop.subStatus))
            {
                this.newLine(true);
                this.drawTextAt(eightSix, $"{hop.subStatus}");
            }

            // line 4: bio signals?
            if (hop.sumGenus > 0)
            {
                this.newLine(true);
                this.drawTextAt(eightSix, $"{hop.sumGenus}x Genus", GameColors.brushCyan, GameColors.fontSmall2Bold);
            }

            this.newLine(+ten, true);
        }
    }

    class RouteInfo
    {
        private static Dictionary<double, RouteInfo> cache = new Dictionary<double, RouteInfo>();

        public static RouteInfo create(RouteEntry entry, bool destination)
        {
            var info = cache.GetValueOrDefault(entry.SystemAddress);

            if (info == null)
            {
                info = new RouteInfo(entry, destination);
                cache.Add(entry.SystemAddress, info);
            }

            info.destination = destination;
            return info;
        }

        public RouteEntry entry;
        public string status = "...";
        public string? subStatus;
        public string? bio;
        public bool highlight;
        public bool destination;
        public int sumGenus;
        public DateTimeOffset? discoveredDate;
        public DateTimeOffset? lastUpdated;
        public bool allBodiesFound;
        public int countBodies;
        public Dictionary<string, List<string>>? special;

        public RouteInfo(RouteEntry entry, bool destination)
        {
            this.entry = entry;
            this.destination = destination;

            this.lookupSystem();
        }

        public string systemName { get => entry.StarSystem; }

        private void lookupSystem()
        {
            // lookup in EDSM
            Game.edsm.getBodies(systemName).ContinueWith(task => Program.crashGuard(() =>
            {
                if (task.Exception != null || !task.IsCompletedSuccessfully)
                {
                    Util.isFirewallProblem(task.Exception);
                    return;
                }
                var edsmResult = task.Result;

                if (edsmResult.name == null || edsmResult.id64 == 0)
                {
                    // system is not known to EDSM
                    status = "Undiscovered system";
                    highlight = true;
                }
                else if (edsmResult.bodyCount == 0)
                {
                    // system is known from routes but it has not been visited or scanned
                    status = "Unscanned system";
                    highlight = true;
                }
                else
                {
                    this.countBodies = edsmResult.bodyCount;
                    this.allBodiesFound = edsmResult.bodyCount == edsmResult.bodies.Count;
                    if (this.allBodiesFound)
                        status = $"Discovered, {edsmResult.bodyCount} bodies";
                    else
                        status = $"Discovered ({edsmResult.bodies.Count} of {edsmResult.bodyCount} bodies)";

                    var discCmdr = edsmResult.bodies.FirstOrDefault()?.discovery?.commander;
                    var discDate = edsmResult.bodies.FirstOrDefault()?.discovery?.date.ToLocalTime().ToString("d");
                    this.discoveredDate = edsmResult.bodies.FirstOrDefault()?.discovery?.date;
                    if (discCmdr != null && discDate != null)
                        subStatus = $"By {discCmdr}, {discDate}";
                }

                if (edsmResult.bodies?.Count > 0)
                    this.lastUpdated = edsmResult.bodies.Max(b => b.updateTime ?? DateTimeOffset.MinValue);

                var plotter = Program.getPlotter<PlotGalMap>();
                if (plotter != null && plotter.Created)
                    plotter.BeginInvoke(() => plotter.Invalidate());

            }));

            Game.spansh.getSystemDump((long)entry.SystemAddress).ContinueWith(response => Program.crashGuard(() =>
            {
                if (response.Exception != null)
                {
                    Util.isFirewallProblem(response.Exception);
                    return;
                }
                var spanshResult = response.Result;
                this.sumGenus = spanshResult.bodies.Sum(_ => _.signals?.genuses?.Count ?? 0);

                foreach (var sta in spanshResult.stations)
                {
                    if (sta.services == null || sta.services.Count == 0) continue;
                    if (sta.services.Contains("Material Trader"))
                    {
                        if (this.special == null) this.special = new Dictionary<string, List<string>>();
                        if (!this.special.ContainsKey(sta.name)) this.special[sta.name] = new List<string>();
                        this.special[sta.name].Add("Material Trader");
                    }
                    if (sta.services.Contains("Technology Broker"))
                    {
                        if (this.special == null) this.special = new Dictionary<string, List<string>>();
                        if (!this.special.ContainsKey(sta.name)) this.special[sta.name] = new List<string>();
                        this.special[sta.name].Add("Technology Broker");
                    }
                }
            }));

            // TODO: maybe lookup in Canonn for bio data too?
        }

    }

}
