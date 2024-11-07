using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.Properties;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotGalMap : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap
                && Game.settings.useExternalData;
        }

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

        private static float leftWidth = Util.maxWidth(GameColors.fontSmall2,
                Misc.PlotGalMap_Selected,
                Misc.PlotGalMap_Current,
                Misc.PlotGalMap_Destination,
                Misc.PlotGalMap_NextJump) + oneTwo;

        private double distanceJumped;
        /// <summary> Next hop data </summary>
        private NetSysData? nextNetData;
        /// <summary> Destination system data </summary>
        private NetSysData? finalNetData;

        private PlotGalMap() : base()
        {
            this.Font = GameColors.fontSmall2;
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
                this.finalNetData = NetSysData.get(game.systemData.name, game.systemData.address, (source, netData) => this.Invalidate());
        }

        protected override void onJournalEntry(FSDTarget entry)
        {
            if (this.IsDisposed) return;

            // exit early when the target is the first hop (this happens when setting a route)
            if (this.nextNetData?.systemAddress == entry.SystemAddress)
                return;

            this.distanceJumped = 0;

            this.finalNetData = NetSysData.get(entry.Name, entry.SystemAddress, (source, netData) => this.Invalidate());
            this.nextNetData = null;

            this.Invalidate();
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (this.IsDisposed) return;

            // lookup if target system has been discovered
            if (game.navRoute.Route.Count < 2)
                return;

            // load data for destination system
            var lastHop = game.navRoute.Route.Last();
            this.finalNetData = NetSysData.get(lastHop.StarSystem, lastHop.SystemAddress, (source, netData) => this.Invalidate());

            // load data for first jump
            var firstHop = game.navRoute.Route.Skip(1).First();
            if (firstHop == lastHop)
                this.nextNetData = null;
            else
                this.nextNetData = NetSysData.get(firstHop.StarSystem, firstHop.SystemAddress, (source, netData) => this.Invalidate());

            this.distanceJumped = 0;
            for (int n = 1; n < game.navRoute.Route.Count; n++)
            {
                var d = Util.getSystemDistance(game.navRoute.Route[n - 1].StarPos, game.navRoute.Route[n].StarPos);
                this.distanceJumped += d;
            }
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (this.IsDisposed) return;

            this.nextNetData = null;
            this.finalNetData = null;

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.finalNetData == null)
            {
                // TODO: keep hidden until we have a route?
                this.drawTextAt(eight, Misc.PlotGalMap_NoRouteSet);
                this.newLine(+ten, true);
            }
            else
            {
                if (this.finalNetData != null)
                    this.drawSystemSummary(this.finalNetData);

                if (this.nextNetData != null)
                    this.drawSystemSummary(this.nextNetData);

                if (this.distanceJumped > 0)
                {
                    var txt = Misc.PlotGalMap_RouteFooter.format(
                        game.navRoute.Route.Count - 1,
                        this.distanceJumped.ToString("N1"));
                    this.drawTextAt(eight, txt, GameColors.brushGameOrange);
                    this.newLine(true);
                }

                this.dty += two;
                this.drawTextAt(eight, Misc.PlotGalMap_DataFrom, GameColors.brushGameOrangeDim);
                this.newLine(true);
            }

            this.formAdjustSize(+ten, +ten);
        }

        private void drawSystemSummary(NetSysData netData)
        {
            var header = Misc.PlotGalMap_Selected;

            if (netData.systemAddress == game.systemData?.address)
                header = Misc.PlotGalMap_Current;
            else if (netData.systemAddress == game.navRoute.Route.LastOrDefault()?.SystemAddress)
                header = Misc.PlotGalMap_Destination;
            else if (netData.systemAddress == nextNetData?.systemAddress)
                header = Misc.PlotGalMap_NextJump;

            this.drawTextAt(eight, header);

            // line 1: system name
            this.drawTextAt(leftWidth, $"► {netData.systemName}", GameColors.fontSmall2Bold);
            this.newLine(true);

            var highlight = netData.discovered == false || netData.scanBodyCount < netData.totalBodyCount || (netData.totalBodyCount == 0 && netData.discovered.HasValue);

            // line 2: status
            var discoveryStatus = netData.discoveryStatus ?? "...";
            if (highlight)
                this.drawTextAt(leftWidth, $"{discoveryStatus}", GameColors.brushCyan, GameColors.fontSmall2Bold);
            else
                this.drawTextAt(leftWidth, $"{discoveryStatus}");

            // line 3: who discovered + last updated
            if (netData.discoveredBy != null && netData.discoveredDate != null)
            {
                this.newLine(true);
                this.drawTextAt(leftWidth, Misc.PlotGalMap_DiscoveredBy.format(netData.discoveredBy, netData.discoveredDate?.ToString("d") ?? "?"));
            }
            if (netData.lastUpdated != null && (netData.lastUpdated > netData.discoveredDate || netData.discoveredDate == null))
            {
                this.newLine(true);
                this.drawTextAt(leftWidth, Misc.PlotGalMap_LastUpdated.format(netData.lastUpdated?.ToString("d") ?? "?"));
            }

            // line 4: bio signals?
            if (netData.genusCount > 0)
            {
                this.newLine(true);
                this.drawTextAt(leftWidth, Misc.PlotGalMap_CountGenus.format(netData.genusCount), GameColors.brushCyan, GameColors.fontSmall2Bold);
            }

            this.newLine(+ten, true);
        }
    }
}
