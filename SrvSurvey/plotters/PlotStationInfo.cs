using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
    [ApproxSize(200, 300)]
    internal class PlotStationInfo : PlotBase, PlotterForm
    {
        private PenBrush pb = new PenBrush(C.orange, 3, LineCap.Flat);
        private ApiSystemDump.System.Station? station;

        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotStationInfo_TEST
                && Game.activeGame?.systemData != null
                && Game.activeGame.isMode(GameMode.ExternalPanel)
                // destination is a Station within the current system
                ;
        }

        private PlotStationInfo() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.gothic_10;
        }

        public override bool allow { get => PlotStationInfo.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(200);
            this.Height = scaled(200);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            setStationFromDestination();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;
            base.Status_StatusChanged(blink);

            setStationFromDestination();
        }

        private void setStationFromDestination()
        {
            if (game.systemData == null || game.status.Destination?.System != game.systemData.address) return;

            // is a local station selected?
            this.station = game.systemData.spanshStations?.FirstOrDefault(s => s.name == game.status.Destination.Name);
            if (station == null)
            {
                // it is not
                setOpacity(0);
            }
            else if (game.systemData != null)
            {
                // yes
                resetOpacity();
                //Program.defer(() => Util.fadeOpacity(this, PlotPos.getOpacity(this), Game.settings.fadeInDuration));
                Game.log($"Selected station: {this.station?.name} ({this.station?.id})");
            }

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed) return;
            if (this.station == null)
            {
                setOpacity(0);
                return;
            }
            var indent = oneEight;

            // title
            drawTextAt2(eight, station.name, GameColors.Fonts.gothic_12B);
            newLine(true);
            drawTextAt2(indent, station.type, GameColors.Fonts.gothic_9);
            newLine(true);
            drawTextAt2(indent, station.primaryEconomy, GameColors.Fonts.gothic_9);
            newLine(+ten, true);

            // faction
            if (station.controllingFaction != null)
            {
                var (rep, inf, state) = game.getFactionInfRep(station.controllingFaction);
                var txtRep = Util.getReputationText(rep);

                drawTextAt2(eight, $"Faction:");
                newLine(true);
                drawTextAt2(indent, station.controllingFaction);
                if (state != null && state != "None")
                {
                    newLine(true);
                    drawTextAt2(indent, $"State: {state}");
                }
                newLine(true);
                drawTextAt2(indent, $"Inf: {inf:P0} | Rep: {txtRep}");
            }


            // largest pad
            string? largestPad = null;
            if (station.landingPads?.Large > 0) largestPad = "Large";
            else if (station.landingPads?.Medium > 0) largestPad = "Medium";
            else if (station.landingPads?.Small > 0) largestPad = "Small";
            if (largestPad != null)
            {
                newLine(+ten, true);
                drawTextAt2(eight, $"Landing pads: {largestPad}");
            }


            // relevant services
            if (station.services != null)
            {
                newLine(+ten, true);
                drawTextAt2(eight, "Relevant services:");
                var interesting = new List<string>() { "Shipyard", "Outfitting", "Refuel", "Restock", "Repair", "Market", "Universal Cartographics", "Search and Rescue", "Interstellar Factors" };
                // TODO: Tech broker, mat trader, engineer, black market
                foreach (var service in station.services)
                {
                    if (!interesting.Contains(service)) continue;
                    newLine(true);
                    drawTextAt2(ten, "- " + service, GameColors.Fonts.gothic_9);
                }
            }


            // Prohibited goods?
            if (station.market?.prohibitedCommodities.Count > 0)
            {
                newLine(+ten, true);
                drawTextAt2(eight, "Prohibited:");
                foreach (var commodity in station.market.prohibitedCommodities)
                {
                    newLine(true);
                    drawTextAt2(ten, "- " + commodity, GameColors.Fonts.gothic_9);
                }
                //newLine(+ten, true);
            }

            newLine(+ten, true);
            drawTextAt2(eight, "Data: Spansh.co.uk", C.orangeDark);
            newLine(true);
            drawTextAt2(eight, $"Updated: {station.updateTime.LocalDateTime.ToShortDateString()}", C.orangeDark);
            newLine(true);

            this.formAdjustSize(+eight, +eight);
        }
    }
}
