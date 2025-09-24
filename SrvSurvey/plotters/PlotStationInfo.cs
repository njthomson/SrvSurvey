using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotStationInfo : PlotBase2
    {
        #region def + statics

        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotStationInfo),
            allowed = allowed,
            ctor = (game, def) => new PlotStationInfo(game, def),
            defaultSize = new Size(200, 300),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotStationInfo_TEST
                && Game.activeGame?.systemData != null
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && (Game.activeGame.isMode(GameMode.ExternalPanel) || PlotStationInfo.forceShow);
        }

        public static bool forceShow = false;

        #endregion

        public ApiSystemDump.System.Station? station;

        public PlotStationInfo(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.gothic_10;
            this.color = C.orange;

            setStationFromDestination();
        }

        protected override void onStatusChange(Status status)
        {
            if (status.changed.Contains(nameof(Status.Destination)))
                setStationFromDestination();
        }

        private void setStationFromDestination()
        {
            if (game.systemData == null || game.status.Destination?.System != game.systemData.address)
            {
                this.hide();
                return;
            }

            // disregard construction sites
            if (ColonyData.isConstructionSite(game.status.Destination.Name))
            {
                this.station = null;
            }
            else
            {
                // is a local station selected?
                this.station = game.systemData.spanshStations?.FirstOrDefault(s => s.name == game.status.Destination.Name);
            }

            // render only if we have something
            if (station == null)
                this.hide();
            else if (game.systemData != null)
                this.show();

            Game.log($"PlotStationInfo.Selected station: {this.station?.name} ({this.station?.id})");
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            // render nothing if there is no station
            if (this.station == null) return frame.Size;

            var indent = N.oneEight;

            // title
            tt.draw(N.eight, station.name, GameColors.Fonts.gothic_12B);
            tt.newLine(N.two, true);

            // settlement type?
            var match = game.systemData?.stations?.Find(s => s.marketId == station.id);
            if (match != null)
            {
                tt.draw(indent, $"{station.type}: ", GameColors.Fonts.gothic_9);
                var x = tt.dtx;
                tt.draw(x, $"{match.economy} #{match.subType}", GameColors.Fonts.gothic_9);
                tt.newLine(true);

                var key = $"{match.stationEconomy}{match.subType}";
                if (CanonnStation.mapSettlementTypes.TryGetValue(key, out var buildType))
                {
                    tt.draw(x, $"( {Util.pascal(buildType)} )",  GameColors.Fonts.gothic_9);
                    tt.newLine(true);
                }
            }
            else
            {
                tt.draw(indent, station.type, GameColors.Fonts.gothic_9);
                tt.newLine(true);
            }

            // largest pad
            string? largestPad = null;
            var padSize = 0;
            if (station.landingPads?.Large > 0) { largestPad = "Large"; padSize = 3; }
            else if (station.landingPads?.Medium > 0) { largestPad = "Medium"; padSize = 2; }
            else if (station.landingPads?.Small > 0) { largestPad = "Small"; padSize = 1; }
            if (largestPad != null)
            {
                tt.draw(indent, "Pads: ", C.orangeDark, GameColors.Fonts.gothic_9);
                // be green/red if ship will fit at this station
                var shipSize = CanonnStation.mapShipSizes.GetValueOrDefault(game.currentShip.type);
                var shipFits = padSize >= shipSize;
                tt.draw(largestPad, shipFits ? C.green : C.red, GameColors.Fonts.gothic_9);
                if (shipFits)
                    tt.draw(" ✔️", C.green, GameColors.Fonts.segoeEmoji_8);
                else
                    tt.draw(" ❌", C.red, GameColors.Fonts.segoeEmoji_8);

                tt.newLine(true);
            }

            //if (station.economies.Count )
            if (station.economies == null)
            {
                tt.draw(indent, station.primaryEconomy, GameColors.Fonts.gothic_9);
                tt.newLine(+N.ten, true);
            }
            else
            {
                tt.dty += N.ten;
                tt.draw(N.eight, $"Economy:", C.orangeDark, GameColors.Fonts.gothic_9);
                tt.newLine(true);

                var count = 0;
                foreach (var entry in station.economies.OrderByDescending(kv => kv.Value))
                {
                    tt.draw(indent, $"{entry.Key}: ", GameColors.Fonts.gothic_9);
                    tt.draw($"{entry.Value}%", count < 2 ? C.cyan : C.orange, GameColors.Fonts.gothic_9);
                    tt.newLine(true);
                    count++;
                }
                tt.dty += 10;
            }

            // faction
            if (station.controllingFaction != null)
            {
                var (rep, inf, state) = game.getFactionInfRep(station.controllingFaction);
                var txtRep = Util.getReputationText(rep);

                tt.draw(N.eight, $"Faction:", C.orangeDark, GameColors.Fonts.gothic_9);
                tt.newLine(true);
                tt.draw(indent, station.controllingFaction);
                if (state != null && state != "None")
                {
                    tt.newLine(true);
                    tt.draw(indent, $"State: {state}", C.orangeDark, GameColors.Fonts.gothic_9);
                }
                tt.newLine(true);
                tt.draw(indent, "Inf: ", C.orangeDark, GameColors.Fonts.gothic_9);
                tt.draw($"{inf:P0}", GameColors.Fonts.gothic_9);
                tt.draw(" | Rep: ", C.orangeDark, GameColors.Fonts.gothic_9);
                var colRep = C.orange;
                if (rep < -35) colRep = C.red;
                else if (rep >= 35) colRep = C.green;
                tt.draw(txtRep, colRep, GameColors.Fonts.gothic_9);
            }


            // relevant services
            if (station.services != null)
            {
                tt.newLine(+N.ten, true);
                tt.draw(N.eight, "Relevant services:", C.orangeDark, GameColors.Fonts.gothic_9);
                var interesting = new List<string>() { "Shipyard", "Outfitting", "Refuel", "Restock", "Repair", "Market", "Universal Cartographics", "Search and Rescue", "Interstellar Factors" };
                // TODO: Tech broker, mat trader, engineer, black market
                foreach (var service in station.services)
                {
                    if (!interesting.Contains(service)) continue;
                    tt.newLine(true);
                    tt.draw(N.ten, "- " + service, GameColors.Fonts.gothic_9);
                }
            }


            // Prohibited goods?
            if (station.market?.prohibitedCommodities.Count > 0)
            {
                tt.newLine(+N.ten, true);
                tt.draw(N.eight, "Prohibited:", C.orangeDark, GameColors.Fonts.gothic_9);
                foreach (var commodity in station.market.prohibitedCommodities)
                {
                    tt.newLine(true);
                    tt.draw(N.ten, "- " + commodity, GameColors.Fonts.gothic_9);
                }
                //tt.newLine(+ten, true);
            }

            tt.newLine(+N.ten, true);
            tt.draw(N.eight, "Data: Spansh.co.uk", C.orangeDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);
            tt.draw(N.eight, $"Updated: {station.updateTime.LocalDateTime.ToShortDateString()}", C.orangeDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);

            return tt.pad(+N.eight, +N.eight);
        }
    }
}
