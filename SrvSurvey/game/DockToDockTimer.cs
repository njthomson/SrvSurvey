using Newtonsoft.Json;

namespace SrvSurvey.game
{
    class DockToDockTimer
    {
        Game game;
        DateTime start;
        string shipType;
        Dictionary<string, int> cargo;
        Dictionary<string, string> csvData = new();
        int jumps;
        double distance;
        /// <summary> Time of first FSD jump or SuperCruise after undocking </summary>
        DateTime egressEndTime;
        /// <summary> Time spent before docking after leaving SuperCruise </summary>
        DateTime ingressStartTime;

        public DockToDockTimer(Game game, Undocked undocked, Docked? lastDocked)
        {
            // start timing
            this.game = game;
            this.start = DateTime.Now;
            this.shipType = game.currentShip.type;
            this.cargo = game.cargoFile.Inventory.ToDictionary(_ => _.Name, _ => _.Count);
            Game.log($"DockToDockTimer.started: in:{shipType}, from: {undocked.StationName} ({undocked.MarketID})");

            this.csvData = new()
            {
                // columns are written in this order (hence we reserve some up front)
                { "startDate", this.start.ToShortDateString() },
                { "startTime", this.start.ToString("HH:mm:ss") },
                { "endDate", "" },
                { "endTime", "" },
                { "duration", "" },
                { "durationEgress", "" },
                { "durationIngress", "" },
                { "jumps", "" },
                { "distance", "" },
                { "startSystem", game.systemData?.name ?? "?" },
                { "startAddress", game.systemData?.address.ToString() ?? "-1" },
                { "startBodyNum", game.systemBody?.id.ToString() ?? "-1" },
                { "startBodyName", game.systemBody?.name ?? "?" },
                { "startDistFromStartLs", game.systemBody?.distanceFromArrivalLS.ToString() ?? "-1" },
                { "startMarketId", undocked.MarketID.ToString() },
                { "startStationName", undocked.StationName },
                { "startStationType", lastDocked?.StationType.ToString() ?? "?" },
                { "interdicted", false.ToString() },
                // populated upon docking
                { "endSystem", "" },
                { "endAddress", "" },
                { "endBodyNum", "" },
                { "endBodyName", "" },
                { "endMarketId", "" },
                { "endStationName", "" },
                { "endStationType", "" },
                { "endDistFromStartLs", "" },
                // ship details
                { "shipType", game.currentShip.type },
                { "shipName", game.currentShip.name },
                { "shipMaxJump", game.currentShip.maxJump.ToString() },
                // cargo may be huge so goes last
                { "cargo", "" },
            };
        }

        public void writeToFile()
        {
            var end = DateTime.Now;
            var duration = end - start;
            var durationEgress = egressEndTime - start;
            var durationIngress = end - ingressStartTime;
            Game.log($"DockToDockTimer.docked: in:{shipType}, duration: {duration}");

            // these names were reserved
            this.csvData["endDate"] = end.ToShortDateString();
            this.csvData["endTime"] = end.ToString("HH:mm:ss");
            this.csvData["duration"] = duration.ToString("hh\\:mm\\:ss");
            this.csvData["durationEgress"] = durationEgress.ToString("hh\\:mm\\:ss");
            this.csvData["durationIngress"] = durationIngress.ToString("hh\\:mm\\:ss");
            this.csvData["jumps"] = this.jumps.ToString();
            this.csvData["distance"] = this.distance.ToString();
            this.csvData["cargo"] = JsonConvert.SerializeObject(this.cargo);

            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SrvSurvey-dock-to-dock-times.csv");

                // write column names if file does not yet exist
                if (!File.Exists(filePath))
                    File.AppendAllText(filePath, string.Join(",", this.csvData.Keys) + "\r\n");

                File.AppendAllText(filePath, string.Join(",", this.csvData.Values) + "\r\n");
            }
            catch (Exception ex)
            {
                // Excel annoyingly locks files open - swallow any such exceptions / IOException
                FormErrorSubmit.Show(ex);
            }
        }

        public void onJournalEntry(Docked entry)
        {
            this.csvData["endSystem"] = entry.StarSystem;
            this.csvData["endAddress"] = entry.SystemAddress.ToString();
            this.csvData["endBodyNum"] = game.systemBody?.id.ToString() ?? "-1";
            this.csvData["endBodyName"] = game.systemBody?.name ?? "?";
            this.csvData["endMarketId"] = entry.MarketID.ToString();
            this.csvData["endStationName"] = entry.StationName;
            this.csvData["endStationType"] = entry.StationType.ToString();
            this.csvData["endDistFromStartLs"] = entry.DistFromStarLS.ToString();

            this.writeToFile();
        }

        public void onJournalEntry(FSDJump entry)
        {
            // after FSD jumps
            this.jumps++;
            this.distance += entry.JumpDist;
        }

        public void onJournalEntry(StartJump entry)
        {
            // before FSD jump or super cruising
            if (this.egressEndTime == DateTime.MinValue)
                this.egressEndTime = DateTime.Now;
        }

        public void onJournalEntry(SupercruiseExit entry)
        {
            // exiting super cruise
            this.ingressStartTime = DateTime.Now;
        }

        public void onJournalEntry(Interdicted entry)
        {
            this.csvData["interdicted"] = true.ToString();
        }

    }
}
