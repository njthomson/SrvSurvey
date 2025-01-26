using EliteDangerousRegionMap;
using SharpDX.DirectInput;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
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
            this.Font = GameColors.Fonts.console_8;
        }

        public override bool allow { get => PlotStationInfo.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(100);
            this.Height = scaled(80);

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

            this.station = game.systemData.spanshStations?.FirstOrDefault(s => s.name == game.status.Destination.Name);


            // is a local station selected?
            if (station == null)
            {
                // no
                this.Opacity = 0;
            }
            else if (game.systemData != null)
            {
                // yes
                this.Opacity = PlotPos.getOpacity(this);
                Game.log($"Selected station: {this.station?.name} ({this.station?.id})");
            }

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed) return;
            if (this.station == null)
            {
                this.Opacity = 0;
                return;
            }
            var indent = oneEight;

            // title
            drawTextAt2(eight, station.name, GameColors.Fonts.gothic_12);
            newLine(+two, true);
            drawTextAt2(indent, station.primaryEconomy);
            newLine(+ten, true);


            // faction
            if (station.controllingFaction != null)
            {
                drawTextAt2(eight, $"Faction:");
                newLine(+two, true);
                drawTextAt2(indent, station.controllingFaction);
                if (station.controllingFactionState != null)
                {
                    newLine(+two, true);
                    drawTextAt2(indent, station.controllingFactionState);
                }
                newLine(+two, true);
                drawTextAt2(indent, $"Inf: ?%");
                newLine(+ten, true);
            }


            // largest pad
            string? largestPad = null;
            if (station.landingPads?.Large > 0) largestPad = "Large";
            else if (station.landingPads?.Medium > 0) largestPad = "Medium";
            else if (station.landingPads?.Small > 0) largestPad = "Small";
            if (largestPad != null)
            {
                drawTextAt2(eight, $"Landing pad: {largestPad}");
                newLine(+ten, true);
            }


            // relevant services
            if (station.services != null)
            {
                drawTextAt2(eight, "Relevant services:");
                newLine(+two, true);
                var interesting = new List<string>() { "Shipyard", "Outfitting", "Refuel", "Restock", "Repair", "Market", "Universal Cartographics", "Search and Rescue", "Interstellar Factors" };
                // TODO: Tech broker, mat trader, engineer, black market
                foreach (var service in station.services)
                {
                    if (!interesting.Contains(service)) continue;
                    drawTextAt2(ten, "- " + service);
                    newLine(+two, true);
                }
            }

            // prohibited goods?

            newLine(+one, true);
            drawTextAt2(eight, "Data: Spansh.co.uk", C.orangeDark);
            newLine(+one, true);
            drawTextAt2(eight, $"Updated: {station.updateTime.LocalDateTime.ToShortDateString()}", C.orangeDark);
            newLine(+one, true);

            this.formAdjustSize(+eight, +eight);
        }
    }
}
