﻿using BioCriterias;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.units;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SrvSurvey.game.GuardianSiteData;

namespace SrvSurvey.canonn
{
    internal class Canonn
    {
        static Canonn()
        {
            Canonn.client = new HttpClient(Util.getResilienceHandler());
            Canonn.client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            Canonn.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        private static HttpClient client;
        private static string allRuinsStaticPathDbg = Path.Combine(Git.srcRootFolder, "SrvSurvey", "allRuins.json");
        //private static string allRuinsStaticPath = Debugger.IsAttached ? allRuinsStaticPathDbg : Path.Combine(Application.StartupPath, "allRuins.json");
        private static string allRuinsStaticPath = Path.Combine(Application.StartupPath, "allRuins.json");
        private static string allBeaconsStaticPath = Path.Combine(Application.StartupPath, "allBeacons.json");
        private static string allStructuresStaticPathDbg = Path.Combine(Git.srcRootFolder, "SrvSurvey", "allStructures.json");
        //private static string allStructuresStaticPath = Debugger.IsAttached ? allStructuresStaticPathDbg : Path.Combine(Application.StartupPath, "allStructures.json");
        private static string allStructuresStaticPath = Path.Combine(Application.StartupPath, "allStructures.json");
        public List<GuardianRuinSummary> allRuins { get; private set; }
        public List<GuardianBeaconSummary> allBeacons { get; private set; }
        public List<GuardianSiteSummary> allStructures { get; private set; }

        public void init(bool devReload = false)
        {
            // load static ruins summaries
            var pubAllRuinsPath = Path.Combine(Git.pubDataFolder, "allRuins.json");
            this.allRuins = File.Exists(pubAllRuinsPath) && !devReload
                    ? JsonConvert.DeserializeObject<List<GuardianRuinSummary>>(File.ReadAllText(pubAllRuinsPath))!
                    : JsonConvert.DeserializeObject<List<GuardianRuinSummary>>(File.ReadAllText(Canonn.allRuinsStaticPath))!;
            this.allBeacons = JsonConvert.DeserializeObject<List<GuardianBeaconSummary>>(File.ReadAllText(Canonn.allBeaconsStaticPath))!;

            var pubAllStructuresPath = Path.Combine(Git.pubDataFolder, "allStructures.json");
            this.allStructures = File.Exists(pubAllStructuresPath) && !devReload
                ? JsonConvert.DeserializeObject<List<GuardianSiteSummary>>(File.ReadAllText(pubAllStructuresPath))!
                : JsonConvert.DeserializeObject<List<GuardianSiteSummary>>(File.ReadAllText(Canonn.allStructuresStaticPath))!;

            Game.log($"Loaded {this.allRuins.Count} ruins, {this.allBeacons.Count} beacons, {this.allStructures.Count} structures");

            /*
            if (Debugger.IsAttached)
            {
                //Game.canonn.prepareNewSummaries();
                Game.canonn.prepareNewStructures();
            }
            // */

            // to calculate average distance from Beacon to related structure.
            //this.getDistancesFromBeaconToStructure().ContinueWith((stuff) => { Game.log($"Done"); });

            //createRuinSummaries().ContinueWith((stuff) =>
            //{
            //    if (stuff.IsCompletedSuccessfully)
            //        ruinSummaries = stuff.Result;
            //    else
            //        Game.log($"Something bad happened in Canonn.init? {stuff.Exception}");
            //});
        }

        #region getSystemPoi

        private Dictionary<string, SystemPoi> cacheGetSystemPoi = new();

        public async Task<SystemPoi> getSystemPoi(string systemName, string cmdrName)
        {
            Game.log($"Requesting getSystemPoi: {systemName}");
            // https://us-central1-canonn-api-236217.cloudfunctions.net/query/getSystemPoi?system=Colonia&odyssey=Y&cmdr=GRINNING2002

            // use cached value if present
            var key = $"{systemName}/{cmdrName}";
            if (cacheGetSystemPoi.ContainsKey(key))
                return cacheGetSystemPoi[key];

            var json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/getSystemPoi?system={Uri.EscapeDataString(systemName)}&odyssey=Y&cmdr={Uri.EscapeDataString(cmdrName)}");
            var systemPoi = JsonConvert.DeserializeObject<SystemPoi>(json)!;

            // add to cache
            cacheGetSystemPoi[key] = systemPoi;

            return systemPoi;
        }

        public async Task<Dictionary<int, CanonnBioStats>> biostats(string genus)
        {
            Game.log($"Requesting biostats: {genus}");

            var json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/biostats/{genus}");
            if (json == "what happen") return null!;
            var biostats = JsonConvert.DeserializeObject<Dictionary<int, CanonnBioStats>>(json)!;

            return biostats;
        }

        public async Task<CanonnBodyBioStats> systemBioStats(long systemAddress)
        {
            var cacheFilename = Path.Combine(BioPredictor.netCache, $"systemBioStats-{systemAddress}.json");
            string json;
            if (BioPredictor.useTestCache && File.Exists(cacheFilename))
            {
                json = File.ReadAllText(cacheFilename);
            }
            else
            {
                json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/biostats?id={systemAddress}");
                if (BioPredictor.useTestCache)
                {
                    Directory.CreateDirectory(BioPredictor.netCache);
                    File.WriteAllText(cacheFilename, json);
                }
            }

            var obj = JsonConvert.DeserializeObject<JObject>(json)!;
            var stats = obj["system"]?.ToObject<CanonnBodyBioStats>()!;
            return stats;
        }

        #endregion

        #region stations

        public async Task<string> submitStation(CanonnStation station)
        {
            station.clientVer = Version.Parse(Program.releaseVersion);
            Game.log($"~~submitStation:\r\n" + JsonConvert.SerializeObject(station, Formatting.Indented));

            var json = JsonConvert.SerializeObject(station, Formatting.Indented);
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/postEvent/srvsurvey/stations", body);

            var responseText = await response.Content.ReadAsStringAsync();
            return responseText;
        }

        public async Task<List<CanonnStation>> getStations(long systemAddress)
        {
            Game.log($"Requesting queryStations: {systemAddress}");

            var json1 = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/srvsurvey/system/{systemAddress}");

            var list = JsonConvert.DeserializeObject<JArray>(json1)!;
            var stations = new List<CanonnStation>();

            foreach (var obj in list)
            {
                var json2 = obj["raw_json"]?.Value<string>();
                if (json2 != null)
                {
                    var station = JsonConvert.DeserializeObject<CanonnStation>(json2)!;
                    stations.Add(station);
                }

            }

            return stations;
        }

        #endregion

        #region get all Guardian Ruins from GRSites

        private static string allRuinsRefPath = Path.Combine(Program.dataFolder, "allRuins.json");
        private static string ruinSummariesPath = Path.Combine(Program.dataFolder, "ruinSummaries.json");
        //public IEnumerable<GuardianRuinSummary> ruinSummaries { get; private set; }

        public async Task<List<GRReports.Data.Report>> getRuinsReports(string bodyName, int idx, bool descending, string? cmdr = null)
        {
            var fields = "updated_at\\n type\\n latitude\\n longitude\\n";
            var filter = $"bodyName:\\\"{bodyName}\\\", frontierID:{idx}";
            //var query = "{\n grreports(limit:32,sort:\"updated_at\",where:{ frontierID:1  }) { updated_at type latitude longitude cmdrName } }";
            var sortOrder = descending ? "DESC" : "ASC";
            var query = "{\\n grreports(limit:32, sort: \\\"updated_At:" + sortOrder + "\\\", where:{\\n " + filter + " \\n}) { " + fields + " } \\n}";

            var op = "{\"operationName\":null,\"variables\":{},\"query\":\"" + query + "\"}";
            //op = @"{""operationName"":null,""variables"":{},""query"":""{\n  grreports(limit: 32, sort: \""updated_at:DESC\"", where: {frontierID: 1}) {\n    updated_at\n    type\n    latitude\n    longitude\n    cmdrName\n  }\n}\n""}";
            var body = new StringContent(
                op,
                Encoding.ASCII,
                "application/json");


            var response = await Canonn.client.PostAsync("https://api.canonn.tech/graphql", body);
            //Game.log($"{response.StatusCode} : {response.IsSuccessStatusCode}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                File.WriteAllText(allRuinsRefPath, json);
            else
                Game.log(json);

            var obj = JsonConvert.DeserializeObject<GRReports>(json)!;
            return obj.data.grreports;
        }

        public async Task<List<GRReports.Data.Report>> getRuinsReportsByCmdr(string cmdrName)
        {
            var fields = "updated_at\\n type\\n latitude\\n longitude\\n bodyName\\n frontierID\\n";
            var filter = $"cmdrName:\\\"{cmdrName}\\\"";
            var query = "{\\n grreports(limit:1000, where:{\\n " + filter + " \\n}) { " + fields + " } \\n}";

            var op = "{\"operationName\":null,\"variables\":{},\"query\":\"" + query + "\"}";
            //op = @"{""operationName"":null,""variables"":{},""query"":""{\n  grreports(limit: 32, sort: \""updated_at:DESC\"", where: {frontierID: 1}) {\n    updated_at\n    type\n    latitude\n    longitude\n    cmdrName\n  }\n}\n""}";
            var body = new StringContent(
                op,
                Encoding.ASCII,
                "application/json");

            var response = await Canonn.client.PostAsync("https://api.canonn.tech/graphql", body);
            //Game.log($"{response.StatusCode} : {response.IsSuccessStatusCode}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                File.WriteAllText(allRuinsRefPath, json);
            else
                Game.log(json);

            var obj = JsonConvert.DeserializeObject<GRReports>(json)!;
            return obj.data.grreports;
        }

        /// <summary>
        /// Read raw GRSites data either from file or from a network request.
        /// </summary>
        public async Task<List<GRSite>> getRawRuinsData()
        {
            string json;

            if (!File.Exists(allRuinsRefPath))
            {
                Game.log("Requesting all Guardian Ruins from network");

                var query = @"{
  grsites(limit: 1000) {
    id
    siteID
    updated_at
    created_at
    system { systemName edsmID id64 edsmCoordX edsmCoordY edsmCoordZ }
    body { bodyName bodyID distanceToArrival }
    visible
    verified
    type { type }
    latitude
    longitude
    frontierID
  }
}".Replace("\r", "").Replace("\n", "");

                var body = new StringContent(
                    "{\"operationName\":null,\"variables\":{},\"query\":\"" + query + "\"}",
                    Encoding.ASCII,
                    "application/json");


                var response = await Canonn.client.PostAsync("https://api.canonn.tech/graphql", body);
                //Game.log($"{response.StatusCode} : {response.IsSuccessStatusCode}");
                json = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    File.WriteAllText(allRuinsRefPath, json);
                else
                    Game.log(json);
            }
            else
            {
                Game.log("Reading codex/ref from disk");
                json = File.ReadAllText(allRuinsRefPath);
            }

            var obj = JsonConvert.DeserializeObject<GRSitesData>(json)!;
            return obj.data.grsites;
        }

        private void postProcessAndFixRuinsData(IEnumerable<GuardianRuinSummary> summaries)
        {
            var foo = summaries.GroupBy(_ => $"{_.systemAddress} {_.bodyId}");
            var nn = 0;

            foreach (var grp in foo)
            {
                var unTyped = grp.Where(_ => _.idx == 0);
                if (unTyped.Count() == 1)
                {
                    if (grp.Count() == 1)
                    {
                        // only a single Ruins - safe to call it #1!
                        grp.First().idx = 1;
                        nn++;
                    }
                    else if (grp.Count() > 1)
                    {
                        // other ruins are numbered, assign the 1 missing number
                        var idx = 0;
                        while (++idx <= grp.Count())
                        {
                            var ugh = grp.FirstOrDefault(_ => _.idx == idx);
                            if (ugh == null)
                            {
                                unTyped.First().idx = idx;
                                nn++;
                            }
                        }
                    }
                }
            }

            Game.log($"Fixed: {nn} ruins!");
        }

        /// <summary>
        /// Create summaries of ruins from raw data.
        /// </summary>
        public async Task<IEnumerable<GuardianRuinSummary>> createRuinSummaries()
        {
            IEnumerable<GuardianRuinSummary> summaries;
            if (!File.Exists(ruinSummariesPath))
            {
                Game.log("Creating ruin summaries");
                var allRuins = await this.getRawRuinsData();

                summaries = allRuins.Select(_ => GuardianRuinSummary.from(_));

                var json = JsonConvert.SerializeObject(summaries);
                File.WriteAllText(ruinSummariesPath, json);
            }
            else
            {
                Game.log("Reading ruin summaries from disk");
                var json = File.ReadAllText(ruinSummariesPath);
                summaries = JsonConvert.DeserializeObject<IEnumerable<GuardianRuinSummary>>(json)!;
            }
            postProcessAndFixRuinsData(summaries);

            return summaries;
        }

        public enum ShowLogs
        {
            All,
            Needed,
            None
        }

        public List<GuardianGridEntry> loadAllRuins(ShowLogs showLogs = ShowLogs.None)
        {
            Data.suppressLoadingMsg = true;
            Game.log($"loadAllRuins: showLogs: {showLogs}");
            if (this.allRuins == null)
            {
                Game.log("Why no ruinSummaries?");
                return new List<GuardianGridEntry>();
            }

            // start by converting allRuins into entries
            var allRuinEntries = this.allRuins.Select(_ => new GuardianGridEntry(_)).ToList();

            // optionally include Ram Tah logs available at each Ruins
            if (showLogs != ShowLogs.None)
            {
                foreach (var entry in allRuinEntries)
                {
                    var pubData = GuardianSitePub.Load(entry.fullBodyName, entry.idx, entry.siteType);
                    // set notes as Ram Tah logs
                    var obelisks = pubData?.ao;
                    if (showLogs == ShowLogs.Needed && Game.activeGame?.cmdr.decodeTheRuins.Count > 0 && obelisks != null)
                    {
                        var o2 = obelisks.Where(o => !Game.activeGame.cmdr.decodeTheRuins.Contains(o.msg)).ToHashSet();
                        obelisks = o2;
                    }
                    entry.ramTahLogs = ActiveObelisk.orderedRamTahLogs(obelisks)!;
                }
            }

            // exit early if cmdr has no Guardian files
            var folder = Path.Combine(Program.dataFolder, "guardian", Game.settings.lastFid!);
            if (!Directory.Exists(folder)) return allRuinEntries;


            // process all guardian data gathered by cmdr
            var files = Directory.GetFiles(folder, "*-ruins-*.json");
            Game.log($"Reading {files.Length} ruins files from disk");
            foreach (var filename in files)
            {
                var data = Data.Load<GuardianSiteData>(filename);
                if (data == null) throw new Exception($"Why no siteData for: {filename}");

                if (data.type == SiteType.Unknown)
                {
                    // find a match?
                    var match = allRuinEntries.FirstOrDefault(_ => _.systemAddress == data.systemAddress && _.bodyId == data.bodyId && _.idx == data.index && _.isRuins);
                    if (match != null)
                    {
                        data.type = Enum.Parse<SiteType>(match.siteType, true);
                        data.Save();
                    }
                }

                var entry = allRuinEntries.FirstOrDefault(_ => _.systemAddress == data.systemAddress && _.bodyId == data.bodyId && _.idx == data.index && _.siteType == data.type.ToString());
                if (entry == null)
                {
                    // did somebody discover a new Ruins??
                    Game.log($"Skipping unexpected ruins? {data.displayName}\r\n{filename}");
                    continue;
                }

                if (entry != null)
                {
                    entry.merge(data);
                }
                else
                {
                    // create an entry from logs?
                    Game.log($"Unknown Guardian body: {data.bodyName} {data.name}. Adding entry without starPos or distanceToArrival");
                    //var newSummary = new GuardianStructureSummary
                    //{
                    //    bodyId = data.bodyId,
                    //    bodyName = data.bodyName,
                    //    distanceToArrival = -1,
                    //    idx = data.index,
                    //    latitude = data.location.Lat,
                    //    longitude = data.location.Long,
                    //    siteHeading = data.siteHeading,
                    //    relicTowerHeading = data.relicTowerHeading,
                    //    siteType = data.type.ToString(),
                    //    systemName = data.systemName,
                    //    systemAddress = data.systemAddress,
                    //    starPos = new double[3],
                    //};

                    //entry = new GuardianGridEntry(newSummary);
                    //allRuins.Add(entry);
                }
            }

            Data.suppressLoadingMsg = false;
            return allRuinEntries;
        }

        public List<GuardianGridEntry> loadAllStructures(ShowLogs showLogs = ShowLogs.None)
        {
            var newEntries = new List<GuardianGridEntry>();

            // start by converting allRuins into entries
            var allStructures = this.allStructures.Select(_ => new GuardianGridEntry(_)).ToList();

            // optionally include Ram Tah logs available at each Ruins
            if (showLogs != ShowLogs.None)
            {
                foreach (var entry in allStructures)
                {
                    var pubData = GuardianSitePub.Load(entry.fullBodyName, 1, entry.siteType);
                    // set notes as Ram Tah logs
                    var obelisks = pubData?.ao;
                    if (showLogs == ShowLogs.Needed && Game.activeGame?.cmdr.decodeTheRuins.Count > 0 && obelisks != null)
                    {
                        var o2 = obelisks.Where(o => !Game.activeGame.cmdr.decodeTheRuins.Contains(o.msg)).ToHashSet();
                        obelisks = o2;
                    }
                    entry.ramTahLogs = ActiveObelisk.orderedRamTahLogs(obelisks)!;
                }
            }

            // exit early if cmdr has no Guardian files
            var folder = Path.Combine(Program.dataFolder, "guardian", Game.settings.lastFid!);
            if (!Directory.Exists(folder)) return allStructures;


            var files = Directory.GetFiles(folder, "*-structure-*.json");

            Game.log($"Reading {files.Length} structure files from disk");
            foreach (var filename in files)
            {
                var data = Data.Load<GuardianSiteData>(filename)!;

                var matches = allStructures.Where(_ => _.systemAddress == data.systemAddress
                    && _.bodyId == data.bodyId
                    && string.Equals(_.siteType.ToString(), data.type.ToString(), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                GuardianGridEntry? entry = null;

                // take the first, assuming only 1 ruin on the body
                if (matches.Count == 1)
                    entry = matches.First();
                else
                    throw new Exception("Why structure match?");

                if (entry != null)
                {
                    entry.merge(data);
                }
            }

            return allStructures;
        }

        public List<GuardianGridEntry> loadAllBeacons()
        {
            var summaries = this.allBeacons;

            var newEntries = new List<GuardianGridEntry>();

            // start by converting allRuins into entries
            var allStructures = summaries.Select(_ => new GuardianGridEntry(_)).ToList();

            // exit early if cmdr has no Guardian files
            var folder = Path.Combine(Program.dataFolder, "guardian", Game.settings.lastFid!);
            if (!Directory.Exists(folder)) return allStructures;

            var files = Directory.GetFiles(folder, "*-beacon.json");

            Game.log($"Reading {files.Length} beacon files from disk");
            foreach (var filename in files)
            {
                var data = Data.Load<GuardianBeaconData>(filename)!;

                var matches = allStructures.Where(_ => _.systemAddress == data.systemAddress
                    && _.bodyId == data.bodyId
                ).ToList();

                GuardianGridEntry? entry = null;

                // take the first, assiming only 1 ruin on the body
                if (matches.Count == 1)
                    entry = matches.First();
                else
                    throw new Exception("Why beacon match?");

                if (entry != null)
                {
                    entry.merge(data);
                }
            }

            return allStructures;
        }

        #endregion

        #region parse Excel sheet of Ruins

        private List<GuardianRuinSummary> newSummaries;

        public async void prepareNewSummaries()
        {
            if (!File.Exists(allRuinsStaticPathDbg)) return;

            // re-read XML if no partial progress
            if (!File.Exists(allRuinsStaticPathDbg))
            {
                readXmlSheetRuins(allRuinsStaticPathDbg);
                this.showSummaryCounts();
            }
            else
            {
                var json = File.ReadAllText(allRuinsStaticPathDbg);
                this.newSummaries = JsonConvert.DeserializeObject<List<GuardianRuinSummary>>(json)!;
            }

            await this.matchSystemAddresses();
            await this.matchBodyIds();
            await this.matchLatLongs();

            // match against known Canonn ruins?
            await this.matchKnownCanonnRuins();

            //await this.matchSystemNameToGRReports();
            //await this.matchBodyNameToGRReports();

            /*
            await this.matchLatLongsByCmdr("Yure"); // no ruins
            await this.matchLatLongsByCmdr("Alton Davies"); // 4 ruins
            await this.matchLatLongsByCmdr("ThArGosu"); // 4 ruins
            await this.matchLatLongsByCmdr("Jasper_Lit"); // 13 ruins

            // await this.matchLatLongsByCmdr("lilaclight"); // has legacy mode reports? 27 ruins
            // await this.matchLatLongsByCmdr("grinning2002"); // has legacy mode reports? 3 ruins

            this.saveNewSummaries();
            // */

            Game.log($"-=-=-=-=- Done!");
            this.showSummaryCounts();
        }

        private void showSummaryCounts()
        {
            var missingSystems = new HashSet<string>();
            var missingBodies = new HashSet<string>();
            var visitedAfterOdysseyReleased = new HashSet<string>();
            var missingLiveLatLong = new HashSet<string>();

            var countHasLiveLatLong = 0;
            var countHasLegacyLatLong = 0;
            var countHasBothLatLong = 0;
            var countHasGRSiteId = 0;
            int countAlpha = 0, countBeta = 0, countGamma = 0;
            foreach (var _ in newSummaries)
            {
                if (_.systemAddress <= 0) missingSystems.Add(_.systemName);
                if (_.bodyId <= 0) missingBodies.Add(_.fullBodyName);

                if (!_.missingLiveLatLong) { countHasLiveLatLong++; } // else { missingLiveLatLong.Add(_.fullBodyName); }
                if (!_.missingLegacyLatLong) countHasLegacyLatLong++;
                if (!_.missingLiveLatLong && !_.missingLegacyLatLong) countHasBothLatLong++;

                if (_.latitude == 0 || _.longitude == 0) { visitedAfterOdysseyReleased.Add(_.fullBodyNameWithIdx); }
                if (_.missingLiveLatLong) { missingLiveLatLong.Add($"{_.fullBodyNameWithIdx}"); }

                if (_.siteID > 0) countHasGRSiteId++;
                if (_.siteType == "Alpha") countAlpha++;
                if (_.siteType == "Beta") countBeta++;
                if (_.siteType == "Gamma") countGamma++;
            }
            var countMissingSystems = missingSystems.Count;
            var countMissingBodyId = missingBodies.Count;

            var totalSystems = new HashSet<string>(newSummaries.Select(_ => _.systemName)).Count;
            var totalBodies = new HashSet<string>(newSummaries.Select(_ => _.fullBodyName)).Count;
            var totalRuins = newSummaries.Count;

            Game.log($"-=-=-=-=- Total systems: {totalSystems}, bodies: {totalBodies}, ruins: {totalRuins}. Alpha: {countAlpha}, Beta: {countBeta}, Gamma: {countGamma}, countHasLiveLatLong: {countHasLiveLatLong}, countHasLegacyLatLong: {countHasLegacyLatLong}, countHasBothLatLong: {countHasBothLatLong}");
            Game.log($"-=-=-=-=- Total with grSiteId: {countHasGRSiteId} (max: 335), bodiesVisitedAfterOdysseyReleased: {visitedAfterOdysseyReleased.Count}");
            Game.log($"-=-=-=-=- Missing missing systemAddress: {countMissingSystems}, bodyId: {countMissingBodyId}, live lat/long: {totalRuins - countHasLiveLatLong}, legacy lat/long: {totalRuins - countHasLegacyLatLong}\r\n");

            var sorted = visitedAfterOdysseyReleased.ToList(); sorted.Sort();
            Game.log($"-=-=-=-=- VisitedAfterOdysseyReleased: {visitedAfterOdysseyReleased.Count}"); //:\r\n {string.Join("\r\n", sorted)}\r\n");

            sorted = missingLiveLatLong.ToList(); sorted.Sort();
            Game.log($"-=-=-=-=-\r\n\r\nGuardian Ruins missing live lat/long co-ordinates: {missingLiveLatLong.Count}");
            //Game.log($"-=-=-=-=-\r\n\r\nGuardian Ruins missing live lat/long co-ordinates: {missingLiveLatLong.Count}\r\n\r\n{string.Join("\r\n", sorted)}\r\n");
        }

        private void saveNewSummaries()
        {
            // save new summaries
            var json = JsonConvert.SerializeObject(newSummaries, Formatting.Indented);
            File.WriteAllText(allRuinsStaticPathDbg, json);
        }

        public void readXmlSheetRuins(string filepath)
        {
            this.newSummaries = new List<GuardianRuinSummary>();

            var doc = XDocument.Load(@"d:\code\Catalog of Guardian Structures and Ruins.xml");
            var table = doc.Root?.Elements().Where(_ => _.Name.LocalName == "Worksheet" && _.FirstAttribute?.Value == "Ruins").First().Elements()!;
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(1);

            foreach (var row in rows)
                processXmlRowRuins(row);

            // save new summaries
            this.saveNewSummaries();
        }

        private void processXmlRowRuins(XElement row)
        {
            var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();

            var systemName = cells[0].Value;
            var bodyName = cells[2].Value;
            var starPos = new double[3] /* x, y, z */ { double.Parse(cells[17].Value), double.Parse(cells[18].Value), double.Parse(cells[19].Value) };

            var types = cells[4].Value.Split(' ');

            for (int n = 0; n < types.Length; n++)
            {
                var siteType = types[n] == "α"
                    ? "Alpha"
                    : types[n] == "β" ? "Beta" : "Gamma";


                var summary = new GuardianRuinSummary
                {
                    /*
                    x int siteID;
                    string systemName;
                    + long systemAddress;
                    string bodyName;
                    x int bodyId;
                    string siteType;
                    int idx;
                    DateTimeOffset lastUpdated;
                    x double distanceToArrival;
                    double[] starPos;
                    double latitude;
                    double longitude;
                     */
                    systemName = systemName,
                    bodyName = bodyName,
                    starPos = starPos,
                    idx = n + 1,
                    siteType = siteType,
                    siteID = -1, // match against grsites
                    bodyId = -1, // find from Spansh?
                    distanceToArrival = -1, //find from Spansh?
                    systemAddress = -1, // query from EDSM or Spansh
                    latitude = Double.NaN, // query from grreports 
                    longitude = Double.NaN, // query from grreports
                    lastUpdated = DateTimeOffset.UtcNow,
                };

                // confirm no prior entry before adding to list
                var existingMatch = newSummaries.Find(_ =>
                {
                    return _.systemName.Equals(summary.systemName, StringComparison.OrdinalIgnoreCase)
                        && _.bodyName.Equals(summary.bodyName, StringComparison.OrdinalIgnoreCase)
                        && _.idx == summary.idx;
                });

                if (existingMatch != null)
                    Game.log($"Duplicate ruins? {summary}");
                else
                    newSummaries.Add(summary);
            }
        }

        private async Task matchKnownCanonnRuins()
        {
            var ruinSummaries = await this.createRuinSummaries();

            foreach (var ruins in newSummaries)
            {
                // ruins.siteID = -1; // reset?
                var dirty = false;

                var onBody = ruinSummaries
                    .Where(_ => _.systemName.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase) && _.bodyName.Equals(ruins.bodyName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // find known ruin summaries on same body by idx?
                var onBodyMatch = onBody.FirstOrDefault(_ => _.idx == ruins.idx && _.siteType == ruins.siteType);

                if (ruins.fullBodyName == "Blae Eork NE-E d13-25 B 2" || true)
                {
                    //var onBodyOfType = onBody.Where(_ => _.siteType == ruins.siteType).ToList();
                    //if (onBodyMatch == null && onBodyOfType.Count == 1)
                    //    onBodyMatch = onBodyOfType.First();

                    if (onBodyMatch == null && !ruins.missingLegacyLatLong) //ruins.fullBodyName == "Col 173 Sector LY-Q d5-13")
                    {
                        // find by lat/long match?
                        onBodyMatch = onBody.FirstOrDefault(_ =>
                            {
                                var siteLatLong = $"{_.latitude},{_.longitude}";
                                var ruinsLegacyLatLong = $"{ruins.legacyLatitude},{ruins.legacyLongitude}";
                                var isClose = this.isCloseLatLong(siteLatLong, ruinsLegacyLatLong);
                                //if (ruins.fullBodyName == "Blae Eork NE-E d13-25 B 2") //isClose && _.siteType == ruins.siteType)
                                //    Game.log("oh?");

                                return isClose && _.siteType == ruins.siteType;
                            });
                    }
                    if (onBodyMatch == null && !ruins.missingLiveLatLong)
                    {
                        // find by lat/long match?
                        onBodyMatch = onBody.FirstOrDefault(_ =>
                        {
                            var siteLatLong = $"{_.latitude},{_.longitude}";
                            var ruinsLiveLatLong = $"{ruins.latitude},{ruins.longitude}";
                            var isClose = this.isCloseLatLong(siteLatLong, ruinsLiveLatLong);
                            //if (isClose && _.siteType == ruins.siteType)
                            //    Game.log("oh?");
                            return isClose && _.siteType == ruins.siteType;
                        });
                    }
                }

                // values from the specific site
                if (ruins.siteID <= 0 && onBodyMatch != null)
                {
                    if (ruins.fullBodyName == "Blae Eork NE-E d13-25 B 2") //isClose && _.siteType == ruins.siteType)
                        Game.log("oh?");

                    ruins.siteID = onBodyMatch.siteID;
                    ruins.lastUpdated = DateTimeOffset.UtcNow;
                    dirty = true;
                }

                //if (onBodyMatch != null && !onBodyMatch.missingLiveLatLong) // always live as grsites doesn't know live from legacy
                //{
                //    var siteLatLong = $"{onBodyMatch.latitude},{onBodyMatch.longitude}";
                //    //Game.log($"GRSite #{onBodyMatch.siteID} lat/long: {siteLatLong}");

                //    //if (ruins.missingLiveLatLong && !ruins.missingLiveLatLong)
                //    if (!ruins.missingLegacyLatLong)
                //    {
                //        var ruinsLegacyLatLong = $"{ruins.legacyLatitude},{ruins.legacyLongitude}";
                //        if (this.isCloseLatLong(siteLatLong, ruinsLegacyLatLong))
                //            Game.log($"GRSite #{onBodyMatch.siteID} has LEGACY lat/long co-ordinates: {siteLatLong} vs {ruinsLegacyLatLong}");
                //        else if (ruins.missingLiveLatLong)
                //            Game.log($"** GRSite #{onBodyMatch.siteID} update live lat/long?: {onBodyMatch.fullBodyNameWithIdx}. {siteLatLong} vs {ruinsLegacyLatLong}");
                //    }

                //    if (!ruins.missingLiveLatLong)
                //    {
                //        var ruinsLiveLatLong = $"{ruins.latitude},{ruins.longitude}";
                //        if (this.isCloseLatLong(siteLatLong, ruinsLiveLatLong))
                //            Game.log($"GRSite #{onBodyMatch.siteID} has LIVE lat/long co-ordinates: {siteLatLong} vs {ruinsLiveLatLong}");
                //        else if (ruins.missingLegacyLatLong)
                //            Game.log($"** GRSite #{onBodyMatch.siteID} update legacy lat/long?: {onBodyMatch.fullBodyNameWithIdx}. {siteLatLong} vs {ruinsLiveLatLong}");
                //        //ruins.longitude = onBodyMatch.longitude;
                //    }
                //}

                if (dirty)
                    this.saveNewSummaries();
            }

            var unmatchedSiteIds = ruinSummaries.Where(_ => newSummaries.Find(ruins => ruins.siteID == _.siteHeading) == null).Select(_ => $"#{_.idx}").ToList();
            var sorted = unmatchedSiteIds.ToList(); sorted.Sort();
            Game.log($"-=-=-=-=- unmatchedSiteIds: {unmatchedSiteIds.Count}:\r\n{string.Join("\r\n", sorted)}\r\n");
        }

        private async Task matchSystemAddresses()
        {
            foreach (var ruins in newSummaries)
            {
                var dirty = false;

                if (ruins.systemAddress > 0) continue;

                // do we have an existing match already?
                var existingSystem = newSummaries.Find(_ => _.systemName.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase) && _.systemAddress > 0);
                if (ruins.systemAddress <= 0 && existingSystem != null)
                {
                    ruins.systemAddress = existingSystem.systemAddress;
                    ruins.lastUpdated = DateTimeOffset.UtcNow;
                    dirty = true;
                    continue;
                }

                // find a match in EDSM?
                Game.log($"EDSM lookup system: {ruins.systemName}");
                var edsmResponse = await Game.edsm.getSystems(ruins.systemName);
                Game.log($"Found {edsmResponse.Length} systems from: {ruins.systemName}");
                var matchedSystemAddress = edsmResponse.FirstOrDefault()?.id64;
                if (matchedSystemAddress > 0)
                {
                    ruins.systemAddress = (long)matchedSystemAddress;
                    ruins.lastUpdated = DateTimeOffset.UtcNow;
                    dirty = true;
                    continue;
                }


                // find a match in Spansh?
                Game.log($"Spansh lookup system: {ruins.systemName}");
                var spanshResponse = await Game.spansh.getSystems(ruins.systemName);
                matchedSystemAddress = spanshResponse.min_max.FirstOrDefault(_ => _.name.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase))?.id64;
                if (matchedSystemAddress > 0)
                {
                    ruins.systemAddress = (long)matchedSystemAddress;
                    ruins.lastUpdated = DateTimeOffset.UtcNow;
                    dirty = true;
                    continue;
                }

                if (dirty)
                {
                    this.saveNewSummaries();
                }
            }
        }

        private async Task matchBodyIds()
        {
            foreach (var ruins in newSummaries)
            {
                var dirty = false;

                if (ruins.bodyId > 0 && ruins.distanceToArrival > 0) continue;

                // do we have an existing match already?
                var existingBody = newSummaries.Find(_ => _.systemName.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase) && _.bodyName.Equals(ruins.bodyName, StringComparison.OrdinalIgnoreCase) && _.bodyId > 0);
                if (ruins.bodyId <= 0 && existingBody?.bodyId > 0)
                {
                    ruins.bodyId = existingBody.bodyId;
                    ruins.lastUpdated = DateTimeOffset.UtcNow;
                    dirty = true;
                }
                if (ruins.distanceToArrival <= 0 && existingBody?.distanceToArrival > 0)
                {
                    ruins.distanceToArrival = existingBody.distanceToArrival;
                    ruins.lastUpdated = DateTimeOffset.UtcNow;
                    dirty = true;
                }

                if (ruins.bodyId < 0 || ruins.distanceToArrival < 0)
                {
                    // find a match in EDSM?
                    Game.log($"EDSM lookup bodies: {ruins.systemName}");
                    var edsmResponse = await Game.edsm.getBodies(ruins.systemName);
                    var fullBodyName = $"{ruins.systemName} {ruins.bodyName}";
                    var matchedBody = edsmResponse.bodies.FirstOrDefault(_ => _.name.Equals(fullBodyName, StringComparison.OrdinalIgnoreCase));
                    if (ruins.bodyId <= 0 && matchedBody?.bodyId > 0)
                    {
                        ruins.bodyId = matchedBody.bodyId.Value;
                        ruins.lastUpdated = DateTimeOffset.UtcNow;
                        dirty = true;
                    }
                    if (ruins.distanceToArrival <= 0 && matchedBody?.distanceToArrival > 0)
                    {
                        ruins.distanceToArrival = matchedBody.distanceToArrival;
                        ruins.lastUpdated = DateTimeOffset.UtcNow;
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    this.saveNewSummaries();
                }
            }
        }

        private async Task matchLatLongs()
        {
            foreach (var ruins in newSummaries)
            {
                var dirty = false;
                // for now, skip if either are present
                if (!ruins.missingLiveLatLong || !ruins.missingLegacyLatLong) continue;
                // skip if we already failed to find lat/long
                if (ruins.latitude == 0 && ruins.longitude == 0) continue;

                var fullBodyName = $"{ruins.systemName} {ruins.bodyName}";
                var recent = await this.getRuinsReports(fullBodyName, ruins.idx, true);
                var old = await this.getRuinsReports(fullBodyName, ruins.idx, false);
                recent.AddRange(old);
                dirty = getLatLong(recent, ruins);

                if (dirty)
                {
                    this.saveNewSummaries();
                    dirty = false;
                }
            }
        }

        private readonly DateTimeOffset odysseyReleaseDate = DateTimeOffset.Parse("2021-05-19T00:00:00Z");

        private bool getLatLong(List<GRReports.Data.Report> reports, GuardianRuinSummary ruins)
        {
            // ignore anything from "EDMC-Canonn.6.9.7" ???

            var dirty = false;
            var liveHits = new Dictionary<string, int>();
            var legacyHits = new Dictionary<string, int>();

            // group hits as simple strings
            foreach (var report in reports)
            {
                var latLong = $"{report.latitude},{report.longitude}";
                if (!liveHits.ContainsKey(latLong)) liveHits[latLong] = 0;
                liveHits[latLong]++;

                if (report.updated_at < odysseyReleaseDate)
                {
                    if (!legacyHits.ContainsKey(latLong)) legacyHits[latLong] = 0;
                    legacyHits[latLong]++;
                }
            }

            // capture the legacy lat/long - there's no ambiguity there
            var topLegacyHit = legacyHits.OrderByDescending(_ => _.Value).FirstOrDefault().Key;
            if (topLegacyHit != null)
            {
                var parts = topLegacyHit.Split(',');
                ruins.legacyLatitude = double.Parse(parts[0], CultureInfo.InvariantCulture);
                ruins.legacyLongitude = double.Parse(parts[1], CultureInfo.InvariantCulture);
                dirty = true;
            }

            // remove horizons numbers
            foreach (var latLong in legacyHits.Keys)
                foreach (var tooClose in liveHits.Keys.Where(_ => isCloseLatLong(latLong, _)))
                    liveHits.Remove(tooClose);

            var topLiveHit = liveHits.OrderByDescending(_ => _.Value).FirstOrDefault().Key;
            if (topLiveHit != null)
            {
                var parts = topLiveHit.Split(',');
                ruins.latitude = double.Parse(parts[0], CultureInfo.InvariantCulture);
                ruins.longitude = double.Parse(parts[1], CultureInfo.InvariantCulture);
                dirty = true;
            }

            if (legacyHits.Count == 0 && liveHits.Count > 0)
            {
                // we cannot be certain when there were zero reports before Odyssey was released
                Game.log($"Careful: Zero legacy hits for: {ruins.fullBodyNameWithIdx}");
            }
            else if (legacyHits.Count == 0 && liveHits.Count == 0)
            {
                // we cannot be certain when there were zero reports before Odyssey was released
                Game.log($"Careful: Zero hits ever for: {ruins.fullBodyNameWithIdx}");
                ruins.latitude = 0;
                ruins.longitude = 0;
                dirty = true;
            }

            var topLiveHitCount = string.IsNullOrEmpty(topLiveHit) ? 0 : liveHits[topLiveHit];
            var topLegacyHitCount = string.IsNullOrEmpty(topLegacyHit) ? 0 : legacyHits[topLegacyHit];
            Game.log($"**!!**!!** {ruins.fullBodyNameWithIdx} => live: {topLiveHitCount}x'{topLiveHit}', legacy: {topLegacyHitCount}x'{topLegacyHit}'");
            //Game.log($"\r\n**!!**!!** {ruins.systemName} {ruins.bodyName} #{ruins.idx} => live: {topLiveHitCount}x'{topLiveHit}', legacy: {topLegacyHitCount}x'{topLegacyHit}'\r\nlive:{JsonConvert.SerializeObject(liveHits, Formatting.Indented)}\r\nlegacy:{JsonConvert.SerializeObject(legacyHits, Formatting.Indented)}\r\n");

            return dirty;
        }

        private bool isCloseLatLong(string left, string right)
        {
            if (left == null || right == null || left == "NaN,NaN" || right == "NaN,NaN") return false;

            var leftParts = left.Split(',').Select(_ => double.Parse(_, CultureInfo.InvariantCulture)).ToList();
            var rightParts = right.Split(',').Select(_ => double.Parse(_, CultureInfo.InvariantCulture)).ToList();

            var dlat = (double)Math.Abs(Math.Round((decimal)leftParts[0], 3) - Math.Round((decimal)rightParts[0], 3));
            var dlong = (double)Math.Abs(Math.Round((decimal)leftParts[0], 3) - Math.Round((decimal)rightParts[0], 3));
            var cutoff = 0.1;
            var isClose = dlat < cutoff && dlong < cutoff;
            return isClose;
        }

        private async Task matchSystemNameToGRReports()
        {
            var allSystems = new HashSet<string>(newSummaries.Select(_ => _.systemName));
            Game.log($"Checking: {allSystems.Count} systems ...");

            var results = new Dictionary<string, bool>();
            foreach (var systemName in allSystems)
            {
                Game.log($"Checking: ({(100.0f / allSystems.Count * results.Count).ToString("#.#")}%) {systemName}");
                var json = await client.GetStringAsync($"https://api.canonn.tech/grreports?_limit=1&systemName={Uri.EscapeDataString(systemName)}");
                results[systemName] = json != "[]";
            }

            var missingSystems = results.Where(_ => !_.Value).Select(_ => _.Key).ToList();
            Game.log($"Unmatched systems ({missingSystems.Count}):\r\n" + string.Join("\r\n", missingSystems));
        }

        private async Task matchBodyNameToGRReports()
        {
            var allBodies = new HashSet<string>(newSummaries.Select(_ => _.fullBodyName));
            Game.log($"Checking: {allBodies.Count} bodies ...");

            var results = new Dictionary<string, bool>();
            foreach (var bodyName in allBodies)
            {
                Game.log($"Checking: ({Math.Round(100.0f / allBodies.Count * results.Count)}%) {bodyName}");
                var json = await client.GetStringAsync($"https://api.canonn.tech/grreports?_limit=1&bodyName={Uri.EscapeDataString(bodyName)}");
                results[bodyName] = json != "[]";
            }

            var missingBodies = results.Where(_ => !_.Value).Select(_ => _.Key).ToList();
            Game.log($"Unmatched allBodies ({missingBodies.Count}):\r\n" + string.Join("\r\n", missingBodies));
        }

        private async Task matchLatLongsByCmdr(string cmdrName)
        {
            var stuff = await this.getRuinsReportsByCmdr(cmdrName);
            Game.log($"\r\nFound {stuff.Count} reports by: {cmdrName}");

            var count = 0;
            foreach (var report in stuff)
            {
                var ruins = newSummaries.Find(_ => _.fullBodyName.Equals(report.bodyName, StringComparison.OrdinalIgnoreCase) && _.idx == report.frontierID);
                if (ruins == null || !ruins.missingLiveLatLong) continue;

                // we have a ruins needing updating
                ruins.latitude = report.latitude;
                ruins.longitude = report.longitude;
                count++;
                Game.log($"  {report.bodyName} Ruins #{report.frontierID}");
            }

            Game.log($"Updated {count} ruins from reports by: {cmdrName}\r\n");

            //foreach (var ruins in newSummaries)
            //{
            //    var dirty = false;
            //    // for now, skip if either are present
            //    if (!ruins.missingLiveLatLong || !ruins.missingLegacyLatLong) continue;
            //    // skip if we already failed to find lat/long
            //    if (ruins.latitude == 0 && ruins.longitude == 0) continue;

            //    var fullBodyName = $"{ruins.systemName} {ruins.bodyName}";
            //    var recent = await this.getRuinsReports(fullBodyName, ruins.idx, true);
            //    var old = await this.getRuinsReports(fullBodyName, ruins.idx, false);
            //    recent.AddRange(old);
            //    dirty = getLatLong(recent, ruins);

            //    if (dirty)
            //    {
            //        this.saveNewSummaries();
            //        dirty = false;
            //    }
            //}
        }

        private async Task getDistancesFromBeaconToStructure()
        {
            if (this.allBeacons == null) return;

            double sum = 0;
            foreach (var beacon in this.allBeacons)
            {
                if (!(beacon.relatedStructureDist > 0))
                {
                    var beaconPos = beacon.starPos;

                    var structureBody = beacon.relatedStructure;
                    var match = Regex.Match(structureBody, "(.*)\\s.*?\\s.*?$");
                    var structureSystem = match.Groups[1].Value;
                    if (string.IsNullOrEmpty(structureSystem))
                        return;

                    var rslt = await Game.edsm.getSystems(structureSystem);
                    var structPos = rslt.FirstOrDefault()?.coords;

                    if (structPos == null)
                        return;

                    var dist = Util.getSystemDistance(beaconPos, structPos);
                    Game.log($"{beacon.systemName} to {beacon.relatedStructure} => {dist}");

                    beacon.relatedStructureDist = dist;
                }

                sum += beacon.relatedStructureDist;
            }

            var avg = sum / this.allBeacons.Count;
            Game.log($"Average distance: {avg} ly");

            /* update needed?
            var json = JsonConvert.SerializeObject(this.allBeacons, Formatting.Indented);
            File.WriteAllText(allBeaconsStaticPath, json);
            // */
        }

        #endregion

        #region parse LilacLight sheets

        public async Task readXmlSheetRuins2()
        {
            var doc = XDocument.Load(@"d:\code\SrvSurvey\..\Guardian Ruin Survey.xml");

            var obeliskGroupings = new Dictionary<string, string>();
            parseRuinObeliskGroupings(doc, "Alpha - Groups", obeliskGroupings);
            parseRuinObeliskGroupings(doc, "Beta - Groups", obeliskGroupings);
            parseRuinObeliskGroupings(doc, "Gamma - Groups", obeliskGroupings);

            var sites = parseGRRuins(doc, obeliskGroupings);
            Game.log($"Writing {sites.Count} pubData Ruins files");

            // now write them out to disk
            foreach (var site in sites)
            {
                var pubPath = Path.Combine(@"D:\code\SrvSurvey\data\guardian", $"{site.systemName} {site.bodyName}-ruins-{site.idx}.json");
                site.ao = site.ao.OrderBy(_ => _.name).ToHashSet();

                if (site.sh <= 0 && site.rh <= 0)
                {
                    // restore -1's where they became zero's
                    site.sh = -1;
                    site.rh = -1;
                }

                var json = JsonConvert.SerializeObject(site, Formatting.Indented);
                File.WriteAllText(pubPath, json);

                // update parts of summary from file
                var ruinSummary = this.allRuins.FirstOrDefault(_ => _.systemName.Equals(site.systemName, StringComparison.OrdinalIgnoreCase) && _.bodyName.Equals(site.bodyName, StringComparison.OrdinalIgnoreCase) && _.idx == site.idx);
                if (ruinSummary == null) throw new Exception("Why?");
                // location
                if (site.ll != null && (double.IsNaN(ruinSummary.latitude) || double.IsNaN(ruinSummary.longitude)))
                {
                    ruinSummary.latitude = (double)site.ll.Lat;
                    ruinSummary.longitude = (double)site.ll.Long;
                }
                // siteHeading
                if (site.sh != -1 && !Util.isClose(ruinSummary.siteHeading, site.sh, 1m))
                    ruinSummary.siteHeading = site.sh;
                // relicTowerHeading 
                if (site.rh != -1 && !Util.isClose(ruinSummary.relicTowerHeading, site.rh, 1m))
                    ruinSummary.relicTowerHeading = site.rh;

                // re-compute surveyComplete
                var completionStatus = site.getCompletionStatus();
                ruinSummary.surveyProgress = completionStatus.progress;

                if (ruinSummary.systemAddress <= 0)
                {
                    var sysRef = await Game.spansh.getSystemRef(ruinSummary.systemName);
                    if (sysRef == null)
                        throw new NotImplementedException("TODO?");

                    ruinSummary.systemAddress = sysRef.id64;
                }

                if (ruinSummary.bodyId <= 0)
                {
                    var sysDump = await Game.spansh.getSystemDump(ruinSummary.systemAddress);
                    var bodyMatch = sysDump?.bodies.Find(b => b.name.EndsWith(ruinSummary.bodyName));
                    if (bodyMatch == null)
                        throw new NotImplementedException("TODO?");

                    ruinSummary.bodyId = bodyMatch.bodyId;
                }
            }

            var summaryJson = JsonConvert.SerializeObject(this.allRuins, Formatting.Indented);
            File.WriteAllText(allRuinsStaticPathDbg, summaryJson);

            Game.log($"Writing {sites.Count} pubData Ruins files - complete");
        }

        private List<GuardianSitePub> parseGRRuins(XDocument doc, Dictionary<string, string> obeliskGroupings)
        {
            var sites = new List<GuardianSitePub>();

            var table = doc.Root?.Elements().Where(_ => _.Name.LocalName == "Worksheet" && _.FirstAttribute?.Value == "Ruins").First().Elements()!;
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(1);

            GuardianSitePub site = null!;

            foreach (var row in rows)
            {
                var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();
                try
                {
                    // start a new site?
                    var siteId = cells[0].Value;
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        siteId = siteId.Substring(0, 5);
                        var systemName = cells[2].Value;
                        var bodyName = cells[10].Value;
                        var idx = int.Parse(cells[23].Value);

                        var pubPath = Path.Combine(@"D:\code\SrvSurvey\data\guardian", $"{systemName} {bodyName}-ruins-{idx}.json");
                        if (File.Exists(pubPath))
                        {
                            // start from existing file
                            var json = File.ReadAllText(pubPath);
                            site = JsonConvert.DeserializeObject<GuardianSitePub>(json)!;
                            if (site.sid == null) site.sid = siteId;
                            site.systemName = systemName;
                            site.bodyName = bodyName;
                            if (site.idx == 0) site.idx = idx;
                            if (site.t == SiteType.Unknown) site.t = Enum.Parse<SiteType>(cells[24].Value, true);
                            if (string.IsNullOrWhiteSpace(site.og)) site.og = obeliskGroupings.GetValueOrDefault(siteId)?.Replace(" ", "").Replace(",", "") ?? "";
                            if (site.ao == null) site.ao = new HashSet<ActiveObelisk>();
                        }
                        else
                        {
                            site = new GuardianSitePub()
                            {
                                sid = siteId,
                                systemName = systemName,
                                bodyName = bodyName,
                                idx = idx,
                                t = Enum.Parse<SiteType>(cells[24].Value, true),
                                og = obeliskGroupings.GetValueOrDefault(siteId)?.Replace(" ", "").Replace(",", "") ?? "",
                                ao = new HashSet<ActiveObelisk>(),
                            };
                        }
                        sites.Add(site);

                        // and create/update the summary entry for this Ruins
                        var ruinSummary = this.allRuins.FirstOrDefault(_ => _.systemName == site.systemName && _.bodyName == site.bodyName && _.idx == site.idx);
                        if (ruinSummary == null)
                        {
                            ruinSummary = new GuardianRuinSummary()
                            {
                                systemName = site.systemName,
                                bodyName = site.bodyName,
                                distanceToArrival = double.Parse(cells[11].Value, CultureInfo.InvariantCulture),
                                siteType = site.t.ToString(),
                                idx = site.idx,

                                // populated from pubData file ...
                                //   siteHeading
                                //   relicTowerHeading
                            };
                            this.allRuins.Add(ruinSummary);
                        }

                        // update starPos if missing
                        ruinSummary.siteID = int.Parse(site.sid.Substring(2));
                        if (ruinSummary.starPos == null)
                        {
                            ruinSummary.starPos = new double[3]
                            {
                                double.Parse(cells[4].Value, CultureInfo.InvariantCulture),
                                double.Parse(cells[5].Value, CultureInfo.InvariantCulture),
                                double.Parse(cells[6].Value, CultureInfo.InvariantCulture),
                            };
                        }
                        // update legacy lat/long if missing
                        if (double.IsNaN(ruinSummary.legacyLatitude) || double.IsNaN(ruinSummary.legacyLongitude))
                        {
                            ruinSummary.legacyLatitude = double.Parse(cells[16].Value, CultureInfo.InvariantCulture);
                            ruinSummary.legacyLongitude = double.Parse(cells[17].Value, CultureInfo.InvariantCulture);
                        }
                        // update live lat/long if missing
                        if (site.ll == null && cells[20].Value != "" && cells[21].Value != "")
                        {
                            site.ll = new LatLong2(
                                double.Parse(cells[20].Value, CultureInfo.InvariantCulture),
                                double.Parse(cells[21].Value, CultureInfo.InvariantCulture)
                            );
                        }
                        if (site.ll != null && (double.IsNaN(ruinSummary.latitude) || double.IsNaN(ruinSummary.longitude)))
                        {
                            ruinSummary.latitude = (double)site.ll.Lat;
                            ruinSummary.longitude = (double)site.ll.Long;
                        }

                        continue;
                    }

                    var bank = cells[28].Value;
                    if (string.IsNullOrEmpty(bank))
                    {
                        Game.log($"Empty bank. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var bankIdx = cells[29].Value;
                    if (bankIdx == "19*B")
                    {
                        Game.log($"Ignore rows about 19*B. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var item1 = cells[30].Value;
                    var item2 = cells[31].Value;
                    var correctCombo = cells[37].Value;
                    if (!string.IsNullOrEmpty(correctCombo) && correctCombo.StartsWith("Correct Combo:"))
                    {
                        Game.log($"Using 'Correct Combo:' => '{correctCombo}' replacing: '{item1}' / '{item2}' on row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        var parts = correctCombo.Substring(correctCombo.IndexOf(':') + 1).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        item1 = parts[0];
                        item2 = parts[2];
                    }

                    if (string.IsNullOrEmpty(item1) || item1 == "-")
                    {
                        // skip rows without items
                        Game.log($"Uncertain items. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }

                    var msgType = cells[32].Value;
                    if (msgType.EndsWith("?"))
                    {
                        // skip rows without items
                        Game.log($"Uncertain msgType. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var msgNum = int.Parse(cells[33].Value);

                    var activeObelisk = new ActiveObelisk()
                    {
                        name = bank.ToUpperInvariant() + int.Parse(bankIdx).ToString("00"),
                        msg = $"{msgType.ToUpperInvariant()[0]}{msgNum}",
                    };

                    // remove any prior entries of the same name but otherwise different
                    var existingButDiff = site.ao.FirstOrDefault(_ => _.name == activeObelisk.name && _.ToString() != activeObelisk.ToString());
                    if (existingButDiff != null)
                        site.ao.Remove(existingButDiff);

                    if (!site.ao.Any(_ => _.ToString() == activeObelisk.ToString()))
                        site.ao.Add(activeObelisk);

                    if (!site.og.Contains(bank))
                        site.og = string.Join("", (site.og + bank).ToList().Order());
                }
                catch (Exception ex)
                {
                    Game.log($"Error: {ex}\r\n" +
                        $"Row:" + string.Join(", ", cells.Select(_ => _.Value)));
                }
            }

            return sites;
        }

        private void parseRuinObeliskGroupings(XDocument doc, string tabName, Dictionary<string, string> obeliskGroupings)
        {
            var table = doc.Root?.Elements().Where(_ => _.Name.LocalName == "Worksheet" && _.FirstAttribute?.Value == tabName).First().Elements()!;
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(1);

            foreach (var row in rows)
            {
                var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();

                var siteId = cells[1].Value.Substring(0, 5);
                var groups = cells[2].Value;

                obeliskGroupings.Add(siteId, groups);
            }
        }

        public async Task readXmlSheetRuins3()
        {
            var doc = XDocument.Load(@"d:\code\SrvSurvey\..\Guardian Structure Survey.xml");

            var obeliskGroupings = new Dictionary<string, string>();

            var sites = parseGRStructures(doc, obeliskGroupings);
            Game.log($"Writing {sites.Count} pubData Structure files");

            // now write them out to disk
            foreach (var site in sites)
            {
                var pubPath = Path.Combine(@"D:\code\SrvSurvey\data\guardian", $"{site.systemName} {site.bodyName}-structure-{site.idx}.json");
                site.ao = site.ao.OrderBy(_ => _.name).ToHashSet();

                var json = JsonConvert.SerializeObject(site, Formatting.Indented);
                File.WriteAllText(pubPath, json);

                // update parts of summary from file
                var siteSummary = this.allStructures.FirstOrDefault(_ => _.systemName.Equals(site.systemName, StringComparison.OrdinalIgnoreCase) && _.bodyName.Equals(site.bodyName, StringComparison.OrdinalIgnoreCase)); // && _.idx == site.idx);
                if (siteSummary == null) throw new Exception("Why?");
                // location
                if (site.ll != null && (double.IsNaN(siteSummary.latitude) || double.IsNaN(siteSummary.longitude)))
                {
                    siteSummary.latitude = (double)site.ll.Lat;
                    siteSummary.longitude = (double)site.ll.Long;
                }
                // siteHeading
                if (siteSummary.siteHeading == -1 && site.sh != -1 && siteSummary.siteHeading != site.sh)
                    siteSummary.siteHeading = site.sh;

                if (siteSummary.siteID <= 0)
                    siteSummary.siteID = int.Parse(site.sid.Substring(2));

                // recompute surveyComplete
                //if (siteSummary.systemName == "HIP 39768") Debugger.Break();
                var completionStatus = site.getCompletionStatus();
                siteSummary.surveyProgress = completionStatus.progress;

                // match some Ruins?
                if (siteSummary.systemAddress < 0)
                    siteSummary.systemAddress = this.allRuins.FirstOrDefault(_ => _.systemName == siteSummary.systemName)?.systemAddress ?? -1;
                // match some other structure?
                if (siteSummary.systemAddress < 0)
                    siteSummary.systemAddress = this.allStructures.FirstOrDefault(_ => _.systemName == siteSummary.systemName)?.systemAddress ?? -1;

                // match some Ruins?
                if (siteSummary.bodyId <= 0)
                    siteSummary.bodyId = this.allRuins.FirstOrDefault(_ => _.systemName == siteSummary.systemName && _.bodyName == siteSummary.bodyName)?.bodyId ?? -1;
                if (siteSummary.bodyId <= 0)
                    siteSummary.bodyId = this.allStructures.FirstOrDefault(_ => _.systemName == siteSummary.systemName && _.bodyName == siteSummary.bodyName)?.bodyId ?? -1;

                if (siteSummary.systemAddress < 0 || siteSummary.bodyId < 0)
                {
                    var rslt = await Game.edsm.getBodies(siteSummary.systemName);
                    siteSummary.systemAddress = rslt.id64;
                    var body = rslt.bodies.FirstOrDefault(_ => _.name == $"{siteSummary.systemName} {siteSummary.bodyName}");
                    siteSummary.bodyId = body?.bodyId ?? -1;
                    siteSummary.distanceToArrival = body?.distanceToArrival ?? -1;
                }

                if (siteSummary.systemAddress <= 0 || siteSummary.bodyId <= 0)
                    throw new Exception("Why?!");
            }

            var summaryJson = JsonConvert.SerializeObject(this.allStructures, Formatting.Indented);
            File.WriteAllText(allStructuresStaticPathDbg, summaryJson);

            Game.log($"Writing {sites.Count} pubData Structure files - complete");
        }

        private static Dictionary<string, string> structureObeliskGroups = new Dictionary<string, string>()
        {
            { "Turtle", "ABCD" },
            { "Crossroads", "ABCD" },
            { "Fistbump", "ABC" },
            { "Lacrosse", "ABCD" },
            { "Bear", "ABCDE" },
            { "Bowl", "ABCD" },
            { "Hammerbot", "ABC" },
            { "Robolobster", "ABCD" },
            { "Squid", "ABCDEF" },
            { "Stickyhand", "ABCDEFGH" },
        };

        private List<GuardianSitePub> parseGRStructures(XDocument doc, Dictionary<string, string> obeliskGroupings)
        {
            var sites = new List<GuardianSitePub>();

            var table = doc.Root?.Elements().Where(_ => _.Name.LocalName == "Worksheet" && _.FirstAttribute?.Value == "Structures").First().Elements()!;
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(1);

            GuardianSitePub site = null!;

            foreach (var row in rows)
            {
                var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();
                try
                {
                    // start a new site?
                    var siteId = cells[0].Value;
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        siteId = siteId.Replace(" ", "").Substring(0, 5);
                        var systemName = cells[2].Value;
                        var bodyName = cells[10].Value;
                        var idx = 1;
                        var siteType = cells[24].Value;
                        var latitudeTxt = string.IsNullOrEmpty(cells[20].Value) ? cells[16].Value : cells[20].Value;
                        var longitudeTxt = string.IsNullOrEmpty(cells[21].Value) ? cells[17].Value : cells[21].Value;
                        var latitude = double.Parse(latitudeTxt, CultureInfo.InvariantCulture);
                        var longitude = double.Parse(longitudeTxt, CultureInfo.InvariantCulture);

                        var pubPath = Path.Combine(@"D:\code\SrvSurvey\data\guardian", $"{systemName} {bodyName}-structure-{idx}.json");
                        if (File.Exists(pubPath))
                        {
                            // start from existing file
                            var json = File.ReadAllText(pubPath);
                            site = JsonConvert.DeserializeObject<GuardianSitePub>(json)!;
                            if (site.sid == null) site.sid = siteId;
                            site.systemName = systemName;
                            site.bodyName = bodyName;
                            site.ll = new LatLong2(latitude, longitude);
                            if (site.idx == 0) site.idx = idx;
                            if (site.t == SiteType.Unknown) site.t = Enum.Parse<SiteType>(siteType, true);
                            if (site.og == null) site.og = structureObeliskGroups[siteType];
                            if (site.ao == null) site.ao = new HashSet<ActiveObelisk>();
                        }
                        else
                        {
                            site = new GuardianSitePub()
                            {
                                sid = siteId,
                                systemName = systemName,
                                bodyName = bodyName,
                                ll = new LatLong2(latitude, longitude),
                                idx = idx,
                                t = Enum.Parse<SiteType>(siteType, true),
                                og = structureObeliskGroups[siteType],
                                ao = new HashSet<ActiveObelisk>(),
                            };
                        }
                        sites.Add(site);

                        // and create/update the summary entry for this Structure
                        var siteSummary = this.allStructures.FirstOrDefault(_ => _.systemName.Equals(site.systemName, StringComparison.OrdinalIgnoreCase) && _.bodyName.Equals(site.bodyName, StringComparison.OrdinalIgnoreCase)); // && _.idx == site.idx);
                        if (siteSummary == null)
                        {
                            siteSummary = new GuardianSiteSummary()
                            {
                                // siteID is below
                                systemName = site.systemName,
                                bodyName = site.bodyName,
                                distanceToArrival = double.Parse(cells[11].Value, CultureInfo.InvariantCulture),
                                siteType = site.t.ToString(),
                                idx = site.idx,
                                latitude = latitude,
                                longitude = longitude,

                                // populated from pubData file ...
                                //   siteHeading
                                //   relicTowerHeading
                            };
                            this.allStructures.Add(siteSummary);
                        }

                        // update starPos if missing
                        siteSummary.siteID = int.Parse(site.sid.Substring(2));
                        if (siteSummary.starPos == null)
                        {
                            siteSummary.starPos = new double[3]
                            {
                                double.Parse(cells[4].Value, CultureInfo.InvariantCulture),
                                double.Parse(cells[5].Value, CultureInfo.InvariantCulture),
                                double.Parse(cells[6].Value, CultureInfo.InvariantCulture),
                            };
                        }

                        continue;
                    }

                    // collect obelisk items and logs
                    var bank = cells[28].Value;
                    if (string.IsNullOrEmpty(bank))
                    {
                        Game.log($"Empty bank. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var bankIdx = cells[29].Value;

                    var item1 = cells[30].Value;
                    var item2 = cells[31].Value;
                    if (string.IsNullOrEmpty(item1) || item1 == "-")
                    {
                        // skip rows without items
                        Game.log($"Uncertain items. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }

                    var msgNum = int.Parse(cells[32].Value.Replace("?", ""));

                    var activeObelisk = new ActiveObelisk()
                    {
                        name = bank.ToUpperInvariant() + int.Parse(bankIdx).ToString("00"),
                        msg = $"#{msgNum}",
                    };

                    // remove any prior entries of the same name but otherwise different
                    var existingButDiff = site.ao.FirstOrDefault(_ => _.name == activeObelisk.name && _.ToString() != activeObelisk.ToString());
                    if (existingButDiff != null)
                        site.ao.Remove(existingButDiff);

                    if (!site.ao.Any(_ => _.ToString() == activeObelisk.ToString()))
                        site.ao.Add(activeObelisk);
                }
                catch (Exception ex)
                {
                    Game.log($"Error: {ex}\r\n" +
                        $"Row:" + string.Join(", ", cells.Select(_ => _.Value)));
                }
            }

            return sites;
        }

        #endregion

        #region build summaries of structures

        private List<GuardianSiteSummary> newStructures;

        public void readXmlSheetStructures()
        {
            this.newStructures = new List<GuardianSiteSummary>();

            var doc = XDocument.Load(@"d:\code\Catalog of Guardian Structures and Ruins.xml");
            var table = doc.Root?.Elements().Where(_ => _.Name.LocalName == "Worksheet" && _.FirstAttribute?.Value == "Structures").First().Elements()!;
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(1);

            foreach (var row in rows)
                processXmlRowStructure(row);
        }

        private void processXmlRowStructure(XElement row)
        {
            var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();

            var systemName = cells[0].Value;
            var bodyName = cells[2].Value;
            var starPos = new double[3] /* x, y, z */ { double.Parse(cells[17].Value, CultureInfo.InvariantCulture), double.Parse(cells[18].Value, CultureInfo.InvariantCulture), double.Parse(cells[19].Value, CultureInfo.InvariantCulture) };
            var siteType = cells[4].Value;

            var summary = new GuardianSiteSummary
            {
                systemName = systemName,
                bodyName = bodyName,
                starPos = starPos,
                //idx = n + 1,
                siteType = siteType,
                //siteID = -1, // match against grsites
                //bodyId = -1, // find from Spansh?
                distanceToArrival = -1, //find from Spansh?
                systemAddress = -1, // query from EDSM or Spansh
                //latitude = Double.NaN, // query from grreports 
                //longitude = Double.NaN, // query from grreports
                //lastUpdated = DateTimeOffset.UtcNow,
            };

            // confirm no prior entry before adding to list
            /*var existingMatch = newSummaries.Find(_ =>
            {
                return _.systemName.Equals(summary.systemName, StringComparison.OrdinalIgnoreCase)
                    && _.bodyName.Equals(summary.bodyName, StringComparison.OrdinalIgnoreCase)
                    && _.idx == summary.idx;
            });

            if (existingMatch != null)
                Game.log($"Duplicate ruins? {summary}");
            else
                newSummaries.Add(summary);
            */
            newStructures.Add(summary);
        }

        public async Task prepareNewStructures()
        {
            Game.log("prepareNewStructures");

            //if (!File.Exists(allStructuresStaticPathDbg)) return;

            // re-read XML if no partial progress
            if (!File.Exists(allStructuresStaticPathDbg))
            {
                readXmlSheetStructures();
                //this.showSummaryCounts();
                this.saveNewStructures();
            }
            else
            {
                var json = File.ReadAllText(allStructuresStaticPathDbg);
                this.newStructures = JsonConvert.DeserializeObject<List<GuardianSiteSummary>>(json)!;
            }

            await this.matchSystemAddresses2();
            await this.matchBodyIds2();

            Game.log($"-=-=-=-=- Done!");
        }

        private void saveNewStructures()
        {
            // save new summaries
            var json = JsonConvert.SerializeObject(this.newStructures, Formatting.Indented);
            File.WriteAllText(allStructuresStaticPathDbg, json);
        }


        private async Task matchSystemAddresses2()
        {
            foreach (var ruins in newStructures)
            {
                var dirty = false;

                if (ruins.systemAddress > 0) continue;

                // do we have an existing match already?
                var existingSystem = newStructures.Find(_ => _.systemName.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase) && _.systemAddress > 0);
                if (ruins.systemAddress <= 0 && existingSystem != null)
                {
                    ruins.systemAddress = existingSystem.systemAddress;
                    dirty = true;
                    continue;
                }

                // find a match in EDSM?
                Game.log($"EDSM lookup system: {ruins.systemName}");
                var edsmResponse = await Game.edsm.getSystems(ruins.systemName);
                Game.log($"Found {edsmResponse.Length} systems from: {ruins.systemName}");
                var matchedSystemAddress = edsmResponse.FirstOrDefault()?.id64;
                if (matchedSystemAddress > 0)
                {
                    ruins.systemAddress = (long)matchedSystemAddress;
                    dirty = true;
                    continue;
                }

                // find a match in Spansh?
                Game.log($"Spansh lookup system: {ruins.systemName}");
                var spanshResponse = await Game.spansh.getSystems(ruins.systemName);
                matchedSystemAddress = spanshResponse.min_max.FirstOrDefault(_ => _.name.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase))?.id64;
                if (matchedSystemAddress > 0)
                {
                    ruins.systemAddress = (long)matchedSystemAddress;
                    dirty = true;
                    continue;
                }

                if (dirty)
                {
                    this.saveNewStructures();
                }
            }
        }

        private async Task matchBodyIds2()
        {
            foreach (var ruins in newStructures)
            {
                var dirty = false;

                if (ruins.bodyId > 0 && ruins.distanceToArrival > 0) continue;

                // do we have an existing match already?
                var existingBody = newStructures.Find(_ => _.systemName.Equals(ruins.systemName, StringComparison.OrdinalIgnoreCase) && _.bodyName.Equals(ruins.bodyName, StringComparison.OrdinalIgnoreCase) && _.bodyId > 0);
                if (ruins.bodyId <= 0 && existingBody?.bodyId > 0)
                {
                    ruins.bodyId = existingBody.bodyId;
                    dirty = true;
                }
                if (ruins.distanceToArrival <= 0 && existingBody?.distanceToArrival > 0)
                {
                    ruins.distanceToArrival = existingBody.distanceToArrival;
                    dirty = true;
                }

                if (ruins.bodyId < 0 || ruins.distanceToArrival < 0)
                {
                    // find a match in EDSM?
                    Game.log($"EDSM lookup bodies: {ruins.systemName}");
                    var edsmResponse = await Game.edsm.getBodies(ruins.systemName);
                    var fullBodyName = $"{ruins.systemName} {ruins.bodyName}";
                    var matchedBody = edsmResponse.bodies.FirstOrDefault(_ => _.name.Equals(fullBodyName, StringComparison.OrdinalIgnoreCase));
                    if (ruins.bodyId <= 0 && matchedBody?.bodyId > 0)
                    {
                        ruins.bodyId = matchedBody.bodyId.Value;
                        dirty = true;
                    }
                    if (ruins.distanceToArrival <= 0 && matchedBody?.distanceToArrival > 0)
                    {
                        ruins.distanceToArrival = matchedBody.distanceToArrival;
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    this.saveNewStructures();
                }
            }
        }

        #endregion

        #region export to csv

        public void dumpToFiles()
        {
            Game.log($"dumpToFiles: begin");
            Data.suppressLoadingMsg = true;

            var dumpFolder = @"d:\code";
            var totals = new DumpTotalCounts();

            var ruinsRows = dumpRuins(dumpFolder, totals);

            // dump summaries
            dumpTotals(dumpFolder, totals);

            Data.suppressLoadingMsg = false;
            Game.log($"dumpToFiles: end");
        }

        private List<List<string>> dumpRuins(string dumpFolder, DumpTotalCounts totals)
        {
            var rows = new List<List<string>>();

            var allRuins = this.allRuins.OrderBy(_ => _.siteID);

            foreach (var site in allRuins)
            {
                var pubData = GuardianSitePub.Load(site.fullBodyName, site.idx, site.siteType);
                if (pubData == null) throw new Exception($"Why no pubData for: {site.fullBodyName}");

                var siteType = Enum.Parse<GuardianSiteData.SiteType>(site.siteType);
                var template = GuardianSiteTemplate.sites[siteType];

                var status = pubData.getCompletionStatus();
                var siteHeading = site.siteHeading == -1 ? "" : $"{site.siteHeading}";
                var relicTowerHeading = site.relicTowerHeading == -1 ? "" : $"{site.relicTowerHeading}";

                // update totals
                var tc = totals.typeCounts[siteType];
                tc.total += 1;
                if (site.siteHeading != -1) tc.hasSiteHeading += 1;
                if (site.relicTowerHeading != -1) tc.hasRelicHeadings += 1;
                if (!double.IsNaN(site.latitude) && !double.IsNaN(site.longitude)) tc.hasLocation += 1;
                if (status.isComplete) tc.surveyComplete += 1;

                // build raw row
                var row = new List<string>(new string[] {
                    // columns to identify the site
                    $"GR{site.siteID}",
                    site.systemName,
                    site.bodyName, // the short form
                    site.siteType,
                    $"{site.idx}",

                    // columns reporting on survey status
                    status.isComplete ? "1" : "",
                    $"{status.percent}",
                    $"{status.score}",
                    $"{status.maxScore}",
                    siteHeading,
                    relicTowerHeading,
                    $"{status.countRelicsPresent}", // count relics present
                    $"{template.poiRelics.Count}", // max relic count
                    $"{status.countPuddlesPresent}", // count puddles present
                    $"{status.maxPuddles}", // max puddle count
                    $"{status.countPoiConfirmed}", // count confirmed
                    $"{status.maxPoiConfirmed}", // max confirmed

                    // columns that will be hidden at the end
                    $"{site.systemAddress}",
                    $"{site.bodyId}",
                    $"{site.distanceToArrival}",
                    double.IsNaN(site.latitude) ? "" : $"{site.latitude}",
                    double.IsNaN(site.longitude) ? "" : $"{site.longitude}",
                    $"{site.starPos[0]}", // X
                    $"{site.starPos[1]}", // Y
                    $"{site.starPos[2]}", // Z
                });
                rows.Add(row);
            }

            // write raw data to file
            var headers = new List<string>(new string[] {
                    // columns to identify the site
                    "SiteID",
                    "System name",
                    "Body name", // short form
                    "Site type",
                    "Index",

                    // columns reporting on survey status
                    "Survey complete",
                    "Survey percent",
                    "Score",
                    "Max score",
                    "Site heading",
                    "Tower heading",
                    "Count relics", // count relics present
                    "Max relics", // max relic count
                    "Count puddles", // count puddles present
                    "Max puddles", // max puddle count
                    "Count confirmed",
                    "Max confirmed", // total valid survey valid POI

                    // columns that will be hidden at the end
                    "System address",
                    "Body Id",
                    "Distance to arrival",
                    "Latitude",
                    "Longitude",
                    "Star pos X", // X
                    "Star pos Y", // Y
                    "Star pos Z", // Z
                });
            rows.Insert(0, headers);

            var lines = rows.Select(row => string.Join(',', row.Select(_ => $"\"{_}\"")));
            File.WriteAllText(Path.Combine(dumpFolder, "ruins-raw.csv"), string.Join("\n", lines));

            // return raw data without headers
            rows.RemoveAt(0);
            return rows;
        }

        private void dumpTotals(string dumpFolder, DumpTotalCounts totals)
        {
            var rows = new Dictionary<string, int>();
            foreach (var foo in totals.typeCounts)
            {
                rows.Add($"Total {foo.Key} sites:", foo.Value.total);
                rows.Add($"Total {foo.Key} surveyComplete:", foo.Value.surveyComplete);


            }

            var lines = rows.Select(row => string.Join(',', $"\"{row.Key}\",\"{row.Value}\""));
            File.WriteAllText(Path.Combine(dumpFolder, "raw-summary.csv"), string.Join("\n", lines));
        }

        internal class DumpTotalCounts
        {
            public Dictionary<SiteType, DumpSiteTypeTotals> typeCounts = new Dictionary<SiteType, DumpSiteTypeTotals>();

            public DumpTotalCounts()
            {
                foreach (var foo in Enum.GetValues<SiteType>())
                    this.typeCounts.Add(foo, new DumpSiteTypeTotals());
            }
        }

        internal class DumpSiteTypeTotals
        {
            public int total;
            public int surveyComplete;
            public int hasLocation;
            public int hasSiteHeading;
            public int hasRelicHeadings;

            // max count of each POI type from templates
            public Dictionary<POIType, int> poiCounts = new Dictionary<POIType, int>();
        }

        #endregion

        #region Import from Canonn Challenge

        public async Task<int> importCanonnChallenge(CommanderCodex cmdrCodex)
        {
            var codexRef = await Game.codexRef.loadCodexRef();

            // https://us-central1-canonn-api-236217.cloudfunctions.net/query/challenge/status?cmdr=grinning2001

            Game.log($"importCanonnChallenge: requesting for: {cmdrCodex.commander}");
            var json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/challenge/status?cmdr={cmdrCodex.commander}");
            var data = JsonConvert.DeserializeObject<Dictionary<string, ChallengeData>>(json)!;

            Game.log($"importCanonnChallenge: Importing {data.Count} entries...");
            var count = 0;
            foreach (var challengeEntry in data)
            {
                if (challengeEntry.Value.types_found == null) continue;

                foreach (var foundType in challengeEntry.Value.types_found)
                {
                    // find corresponding entryId...
                    var match = codexRef.Values.FirstOrDefault(e => e.english_name == foundType);
                    if (match == null) continue;
                    if (match.hud_category != challengeEntry.Value.hud_category) Debugger.Break();
                    var entryId = long.Parse(match.entryid);

                    // skip things we already know about
                    if (cmdrCodex.codexFirsts.ContainsKey(entryId))
                        continue;

                    // add a new entry, though we don't know the date or location
                    Game.log($"importCanonnChallenge: Importing {challengeEntry.Value.hud_category}/{foundType}");
                    count++;
                    cmdrCodex.codexFirsts[entryId] = new CodexFirst(DateTime.Now, -1, -1);
                }
            }
            Game.log($"importCanonnChallenge: complete, added {count} new entries");

            cmdrCodex.Save();
            return count;
        }

        internal class ChallengeData
        {
            public int available;
            public string hud_category;
            public float percentage;
            public List<string>? types_available;
            public List<string>? types_found;
            public List<string>? types_missing;
            public int visited;
        }

        #endregion

        #region find nearest bio signals

        private Dictionary<string, QueryNearestResponse> cacheFindNearestSystemWithBio = new();

        public async Task<QueryNearestResponse> findNearestSystemWithBio(StarPos starPos, string bioName, int limit = 5)
        {
            Game.log($"Requesting /query/nearest/codex: starPos: {starPos} for: {bioName}");

            // use cached value if present
            var key = $"{bioName}/{starPos}";
            if (cacheFindNearestSystemWithBio.ContainsKey(key))
                return cacheFindNearestSystemWithBio[key];

            // https://us-central1-canonn-api-236217.cloudfunctions.net/query/nearest/codex?x=7825.40625&y=-101.96875&z=62316.93750&name=Umbrux&limit=3

            var json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/nearest/codex?{starPos.ToUrlParams()}&name={Uri.EscapeDataString(bioName)}&limit={limit}");
            var response = JsonConvert.DeserializeObject<QueryNearestResponse>(json)!;

            // add to cache
            cacheFindNearestSystemWithBio[key] = response;

            return response;
        }

        public class QueryNearestResponse
        {
            public List<Entry> nearest;

            public class Entry
            {
                public double distance;
                public string english_name;
                public long entryid;
                public string system;
                public double x;
                public double y;
                public double z;

                public StarPos toStarPos()
                {
                    return new StarPos(x, y, z, system);
                }
            }
        }

        #endregion
    }

    internal class CanonnBodyBioStats
    {
        public List<CanonnBodyBioStatsBody> bodies;
        public int bodyCount;
        public long id64;
        public string name;
        public DateTime? date;

        public StarPos coords;
    }

    internal class CanonnBodyBioStatsBody
    {
        public double argOfPeriapsis; // 51.386976
        public double ascendingNode; // 150.542857
        public Dictionary<string, float> atmosphereComposition; //	{…}
        public string atmosphereType; // "Thin Ammonia"
        public double axialTilt; // -0.113032
        public int bodyId; // 52
        public double distanceToArrival; //4883.334776
        public double earthMasses; // 0.004436
        public double gravity; // 0.135979096563679
        public long id64; //1873499860443664100
        public bool isLandable; // true
        public Dictionary<string, float> materials; // {…}
        public double meanAnomaly; // 72.999493
        public string name; // "Graea Hypue AA-Z d70 AB 6 c"
        public double orbitalEccentricity; // 0.000627
        public double orbitalInclination; // 0.811855
        public double orbitalPeriod; // 4.61596367811343
        // TODO: parents	[…]
        public double radius; // 1151.512875
        public double rotationalPeriod; // 4.61602240966435
        public bool rotationalPeriodTidallyLocked; // true
        public double semiMajorAxis; // 0.0093306383208345
        public CanonnBodyBioStatsSignals signals; // {…}
        public Dictionary<string, float> solidComposition; // {…}
        public List<Station> stations;
        public string subType; // "Rocky body"
        public double surfacePressure; // 0.00113836642487047
        public double surfaceTemperature; // 158.876053
        public string terraformingState; // "Not terraformable"
        // TODO? timestamps	{…}
        public string type; // "Planet"

        public class Station : ApiSystemDump.System.Station
        {
            public string allegiance;
            public double latitude;
            public double longitude;
        }
    }

    internal class CanonnBodyBioStatsSignals
    {
        public List<string> biology;
        public List<string> genuses;
        public List<string> guesses;
        public Dictionary<string, int> signals;
    }

}
