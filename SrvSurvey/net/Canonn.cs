using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.units;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SrvSurvey.game.GuardianSiteData;

namespace SrvSurvey.canonn
{
    internal class Canonn
    {
        private static HttpClient client;
        private static string allRuinsStaticPathDbg = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..", "allRuins.json");//"D:\\code\\SrvSurvey\\SrvSurvey\\allRuins.json";
        //private static string allRuinsStaticPath = Debugger.IsAttached ? allRuinsStaticPathDbg : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "allRuins.json");
        private static string allRuinsStaticPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "allRuins.json");
        private static string allBeaconsStaticPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "allBeacons.json");
        private static string allStructuresStaticPathDbg = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..", "allStructures.json"); //"D:\\code\\SrvSurvey\\SrvSurvey\\allStructures.json";
        //private static string allStructuresStaticPath = Debugger.IsAttached ? allStructuresStaticPathDbg : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "allStructures.json");
        private static string allStructuresStaticPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "allStructures.json");
        public List<GuardianRuinSummary> allRuins { get; private set; }
        public List<GuardianBeaconSummary> allBeacons { get; private set; }
        public List<GuardianStructureSummary> allStructures { get; private set; }

        public void init(bool devReload = false)
        {
            Canonn.client = new HttpClient();
            Canonn.client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            Canonn.client.DefaultRequestHeaders.Add("user-agent", $"SrvSurvey-{Game.releaseVersion}");

            // load static ruins summaries
            var pubAllRuinsPath = Path.Combine(Git.pubDataFolder, "allRuins.json");
            this.allRuins = File.Exists(pubAllRuinsPath) && !devReload
                    ? JsonConvert.DeserializeObject<List<GuardianRuinSummary>>(File.ReadAllText(pubAllRuinsPath))!
                     : JsonConvert.DeserializeObject<List<GuardianRuinSummary>>(File.ReadAllText(Canonn.allRuinsStaticPath))!;
            this.allBeacons = JsonConvert.DeserializeObject<List<GuardianBeaconSummary>>(File.ReadAllText(Canonn.allBeaconsStaticPath))!;
            var pubAllStructuresPath = Path.Combine(Git.pubDataFolder, "allStructures.json");
            this.allStructures = File.Exists(pubAllStructuresPath) && !devReload
                ? JsonConvert.DeserializeObject<List<GuardianStructureSummary>>(File.ReadAllText(pubAllStructuresPath))!
                : JsonConvert.DeserializeObject<List<GuardianStructureSummary>>(File.ReadAllText(Canonn.allStructuresStaticPath))!;
            Game.log($"Loaded {this.allRuins.Count} ruins, {this.allBeacons.Count} beacons, {this.allStructures.Count} beacons");

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

        public async Task<SystemPoi> getSystemPoi(string systemName, string cmdrName)
        {
            Game.log($"Requesting getSystemPoi: {systemName}");

            var json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/getSystemPoi?system={systemName}&odyssey=Y&cmdr={cmdrName}");
            var systemPoi = JsonConvert.DeserializeObject<SystemPoi>(json)!;

            return systemPoi;
        }

        public static async void biostats(long systemAddress, int bodyId)
        {
            var json = await client.GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/biostats?id={systemAddress}");

            JToken biostats = JsonConvert.DeserializeObject<JToken>(json)!;

            var bodies = biostats["system"]!["bodies"]!.Value<JArray>()!;

            var body = bodies[bodyId - 1].ToObject<Planet>()!;

            Game.log(body);
        }

        #endregion

        #region get all Guardian Ruins from GRSites

        private static string allRuinsRefPath = Path.Combine(Program.dataFolder, "allRuins.json");
        private static string ruinSummariesPath = Path.Combine(Program.dataFolder, "ruinSummaries.json");
        //public IEnumerable<GuardianRuinSummary> ruinSummaries { get; private set; }

        public async Task<List<GRReport>> getRuinsReports(string bodyName, int idx, bool descending, string? cmdr = null)
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


            var response = await new HttpClient().PostAsync("https://api.canonn.tech/graphql", body);
            //Game.log($"{response.StatusCode} : {response.IsSuccessStatusCode}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                File.WriteAllText(allRuinsRefPath, json);
            else
                Game.log(json);

            var obj = JsonConvert.DeserializeObject<GRReportsData>(json)!;
            return obj.data.grreports;
        }

        public async Task<List<GRReport>> getRuinsReportsByCmdr(string cmdrName)
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

            var response = await new HttpClient().PostAsync("https://api.canonn.tech/graphql", body);
            //Game.log($"{response.StatusCode} : {response.IsSuccessStatusCode}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                File.WriteAllText(allRuinsRefPath, json);
            else
                Game.log(json);

            var obj = JsonConvert.DeserializeObject<GRReportsData>(json)!;
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


                var response = await new HttpClient().PostAsync("https://api.canonn.tech/graphql", body);
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

        public List<GuardianRuinEntry> loadAllRuins()
        {
            var summaries = this.allRuins;

            if (summaries == null)
            {
                Game.log("Why no ruinSummaries?");
                return new List<GuardianRuinEntry>();
            }

            var newEntries = new List<GuardianRuinEntry>();

            var allRuins = summaries.Select(_ => new GuardianRuinEntry(_)).ToList();
            var folder = Path.Combine(Program.dataFolder, "guardian", Game.settings.lastFid!);
            if (Directory.Exists(folder))
            {
                var files = Directory.GetFiles(folder, "*-ruins-*.json");

                Game.log($"Reading {files.Length} ruins files from disk");
                foreach (var filename in files)
                {
                    var data = Data.Load<GuardianSiteData>(filename);
                    if (data == null) throw new Exception($"Why no siteData for: {filename}");

                    var matches = allRuins.Where(_ => _.systemAddress == data.systemAddress
                        && _.bodyId == data.bodyId
                        && string.Equals(_.siteType, data.type.ToString(), StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    GuardianRuinEntry? entry = null;

                    // take the first, assiming only 1 ruin on the body
                    if (matches.Count == 1)
                        entry = matches.First();

                    // if more than 1, take the one matching the Ruins #
                    if (entry == null)
                        entry = matches.FirstOrDefault(_ => _.idx == data.index);

                    // try matching if there's only 1 unmatched entry of this type
                    if (entry == null)
                    {
                        var match = matches.Where(_ => _.idx == 0 && string.Compare(_.siteType, data.type.ToString(), true) == 0);
                        if (match.Count() == 1)
                            entry = match.First();
                    }

                    // if still no match, take the one that is really close by lat/long
                    if (entry == null)
                    {
                        foreach (var match in matches)
                        {
                            if (match.latitude == 0 && match.longitude == 0) continue;

                            var dist = Util.getDistance(data.location, new LatLong2(match.latitude, match.longitude), 1);
                            if (dist < 0.0005M)
                            {
                                entry = match;
                                Game.log($"Matched {data.bodyName} #{data.index} on distance!");
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(data.systemName) && matches.Any())
                    {
                        var firstMatch = matches.First();
                        data.systemName = firstMatch.systemName;
                        data.bodyName = firstMatch.fullBodyName;
                        data.Save();
                    }
                    if (string.IsNullOrEmpty(data.commander) && !string.IsNullOrEmpty(Game.activeGame?.Commander))
                    {
                        data.commander = Game.activeGame.Commander;
                        data.Save();
                    }

                    if (entry != null)
                    {
                        entry.merge(data);
                    }
                    else
                    {
                        // create an entry to represent our own data
                        if (matches.Count == 0)
                        {
                            Game.log($"Unknown Guardian body: {data.bodyName} {data.name}. Adding entry without starPos or distanceToArrival");
                            // TODO: fabricate an entry from logs?
                            //var newSummary = new GuardianRuinSummary
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

                            //entry = new GuardianRuinEntry(newSummary);
                            //allRuins.Add(entry);
                        }
                        else
                        {
                            Game.log($"No exact match for: {data.bodyName} {data.name}");
                            //var newEntry = GuardianRuinEntry.from(data, matches.First());
                            //newEntries.Add(newEntry);
                        }
                    }
                }

                // remove an unmatchable entry for each new one
                //foreach (var newEntry in newEntries)
                //{
                //    var victim = allRuins.First(_ => _.systemAddress == newEntry.systemAddress && _.bodyId == newEntry.bodyId && string.Compare(_.siteType, newEntry.siteType, true) == 0);
                //    allRuins.Remove(victim);
                //    allRuins.Add(newEntry);
                //}
            }

            return allRuins;
        }


        public List<GuardianGridEntry> loadAllStructures()
        {
            var summaries = this.allStructures;

            var newEntries = new List<GuardianGridEntry>();

            var allStructures = summaries.Select(_ => new GuardianGridEntry(_)).ToList();
            var folder = Path.Combine(Program.dataFolder, "guardian", Game.settings.lastFid!);
            if (Directory.Exists(folder))
            {
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

                    // take the first, assiming only 1 ruin on the body
                    if (matches.Count == 1)
                        entry = matches.First();
                    else
                        throw new Exception("Why structure match?");

                    if (entry != null)
                    {
                        entry.merge(data);
                    }
                }

                // remove an unmatchable entry for each new one
                //foreach (var newEntry in newEntries)
                //{
                //    var victim = allRuins.First(_ => _.systemAddress == newEntry.systemAddress && _.bodyId == newEntry.bodyId && string.Compare(_.siteType, newEntry.siteType, true) == 0);
                //    allRuins.Remove(victim);
                //    allRuins.Add(newEntry);
                //}
            }

            return allStructures;
        }

        public List<GuardianGridEntry> loadAllBeacons()
        {
            var summaries = this.allBeacons;

            var newEntries = new List<GuardianGridEntry>();

            var allStructures = summaries.Select(_ => new GuardianGridEntry(_)).ToList();
            var folder = Path.Combine(Program.dataFolder, "guardian", Game.settings.lastFid!);
            if (Directory.Exists(folder))
            {
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
                var spanshResponse = await Game.spansh.getSystem(ruins.systemName);
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
                        ruins.bodyId = matchedBody.bodyId;
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

        private bool getLatLong(List<GRReport> reports, GuardianRuinSummary ruins)
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
                ruins.legacyLatitude = double.Parse(parts[0]);
                ruins.legacyLongitude = double.Parse(parts[1]);
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
                ruins.latitude = double.Parse(parts[0]);
                ruins.longitude = double.Parse(parts[1]);
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

            var leftParts = left.Split(',').Select(_ => double.Parse(_)).ToList();
            var rightParts = right.Split(',').Select(_ => double.Parse(_)).ToList();

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
                var json = await client.GetStringAsync($"https://api.canonn.tech/grreports?_limit=1&systemName={systemName}");
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
                var json = await client.GetStringAsync($"https://api.canonn.tech/grreports?_limit=1&bodyName={bodyName}");
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
                    var structPos = rslt.FirstOrDefault()?.coords.starPos;

                    if (structPos == null || structPos.Length == 0)
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

        public void readXmlSheetRuins2()
        {
            var doc = XDocument.Load(@"d:\code\Guardian Ruin Survey.xml");

            var obeliskGroupings = new Dictionary<string, string>();
            parseRuinObeliskGroupings(doc, "Alpha - Groups Present", obeliskGroupings);
            parseRuinObeliskGroupings(doc, "Beta - Groups Present", obeliskGroupings);
            parseRuinObeliskGroupings(doc, "Gamma - Groups Present", obeliskGroupings);

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
                    ruinSummary.latitude = site.ll.Lat;
                    ruinSummary.longitude = site.ll.Long;
                }
                // siteHeading
                if (site.sh != -1 && !Util.isClose(ruinSummary.siteHeading, site.sh, 1))
                    ruinSummary.siteHeading = site.sh;
                // relicTowerHeading 
                if (site.rh != -1 && !Util.isClose(ruinSummary.relicTowerHeading, site.rh, 1))
                    ruinSummary.relicTowerHeading = site.rh;

                // re-compute surveyComplete
                {
                    // The survey is complete when we know:
                    var complete = true;

                    // the site and relic tower headings
                    complete &= ruinSummary.siteHeading >= 0;
                    complete &= ruinSummary.relicTowerHeading >= 0;
                    // live lat / long
                    complete &= !double.IsNaN(ruinSummary.latitude);
                    complete &= !double.IsNaN(ruinSummary.longitude);
                    // status of all POI
                    var allPoi = new List<string>();
                    if (site.pp != null) allPoi.AddRange(site.pp.Split(","));
                    if (site.pa != null) allPoi.AddRange(site.pa.Split(","));
                    if (site.pe != null) allPoi.AddRange(site.pe.Split(","));
                    var allPoiHash = allPoi.ToHashSet();
                    if (allPoiHash.Count != allPoi.Count)
                    {
                        Game.log($"Bad data? {pubPath}");
                        Game.log(string.Join(',', allPoiHash.Order()));
                        Game.log(string.Join(',', allPoi.Order()));
                        Debugger.Break();
                    }

                    var template = SiteTemplate.sites[site.t];
                    var totalPoiCount = template.poi.Count(_ => _.type != POIType.obelisk && _.type != POIType.brokeObelisk);
                    if (allPoi.Count > totalPoiCount)
                    {
                        Game.log($"Bad data? {pubPath}");
                        Clipboard.SetText(pubPath);
                        Debugger.Break();
                    }

                    var allPoiMatched = template.poi
                        .Where(_ => _.type != POIType.obelisk && _.type != POIType.brokeObelisk)
                        .All(_ => allPoiHash.Contains(_.name));
                    complete &= allPoiMatched;

                    if (string.IsNullOrEmpty(site.og))
                    {
                        complete &= false;
                    }
                    else
                    {
                        // every obelisk group has at least 1 active obelisk
                        foreach (var g in site.og)
                            complete &= site.ao.Any(_ => _.name.StartsWith(g.ToString().ToUpperInvariant()));
                    }

                    ruinSummary.surveyComplete = complete;
                }


                if (ruinSummary.bodyId == -1)
                {
                    throw new NotImplementedException("TODO?");
                }
                if (ruinSummary.systemAddress == -1)
                {
                    throw new NotImplementedException("TODO?");
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
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(2);

            GuardianSitePub site = null!;

            foreach (var row in rows)
            {
                var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();
                try
                {
                    // start a new site?
                    var siteId = cells[1].Value;
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        siteId = siteId.Substring(0, 5);
                        var systemName = cells[3].Value;
                        var bodyName = cells[10].Value;
                        var idx = int.Parse(cells[21].Value);

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
                            if (site.t == SiteType.Unknown) site.t = Enum.Parse<SiteType>(cells[22].Value, true);
                            if (string.IsNullOrWhiteSpace(site.og)) site.og = obeliskGroupings[siteId].Replace(" ", "").Replace(",", "");
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
                                t = Enum.Parse<SiteType>(cells[22].Value, true),
                                og = obeliskGroupings[siteId].Replace(" ", "").Replace(",", ""),
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
                                distanceToArrival = double.Parse(cells[11].Value),
                                siteType = site.t.ToString(),
                                idx = site.idx,

                                // populated from pubData file ...
                                //   latitude
                                //   longitude
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
                                double.Parse(cells[5].Value),
                                double.Parse(cells[6].Value),
                                double.Parse(cells[7].Value),
                            };
                        }
                        // update legacy lat/long if missing
                        if (double.IsNaN(ruinSummary.legacyLatitude) || double.IsNaN(ruinSummary.legacyLongitude))
                        {
                            ruinSummary.legacyLatitude = double.Parse(cells[17].Value);
                            ruinSummary.legacyLongitude = double.Parse(cells[18].Value);
                        }

                        continue;
                    }

                    var bank = cells[25].Value;
                    if (string.IsNullOrEmpty(bank))
                    {
                        Game.log($"Empty bank. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var bankIdx = cells[26].Value;
                    if (bankIdx == "19*B")
                    {
                        Game.log($"Ignore rows about 19*B. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var item1 = cells[27].Value;
                    var item2 = cells[28].Value;
                    var correctCombo = cells[34].Value;
                    if (!string.IsNullOrEmpty(correctCombo) && correctCombo.StartsWith("Correct Combo:"))
                    {
                        Game.log($"Using 'Correct Combo:' => '{correctCombo}' replacing: '{item1}' / '{item2}' on row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        var parts = correctCombo.Substring(correctCombo.IndexOf(':')+1).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        item1 = parts[0];
                        item2 = parts[2];
                    }

                    if (string.IsNullOrEmpty(item1) || item1 == "-")
                    {
                        // skip rows without items
                        Game.log($"Uncertain items. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }

                    var msgType = cells[30].Value;
                    if (msgType.EndsWith("?"))
                    {
                        // skip rows without items
                        Game.log($"Uncertain msgType. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var msgNum = int.Parse(cells[31].Value);

                    var activeObelisk = new ActiveObelisk()
                    {
                        name = bank.ToUpperInvariant() + int.Parse(bankIdx).ToString("00"),
                        msg = $"{msgType.ToUpperInvariant()[0]}{msgNum}",
                    };
                    activeObelisk.items.Add(Enum.Parse<ObeliskItem>(item1, true));
                    if (!string.IsNullOrEmpty(item2) && item2 != "-")
                        activeObelisk.items.Add(Enum.Parse<ObeliskItem>(item2, true));

                    // remove any prior entries of the same name but otherwise different
                    var existingButDiff = site.ao.FirstOrDefault(_ => _.name == activeObelisk.name && _.ToString(true) != activeObelisk.ToString(true));
                    if (existingButDiff != null)
                        site.ao.Remove(existingButDiff);
                    
                    if (!site.ao.Any(_ => _.ToString(true) == activeObelisk.ToString(true)))
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
            var doc = XDocument.Load(@"d:\code\Guardian Structure Survey.xml");

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
                    siteSummary.latitude = site.ll.Lat;
                    siteSummary.longitude = site.ll.Long;
                }
                // siteHeading
                if (siteSummary.siteHeading == -1 && site.sh != -1 && siteSummary.siteHeading != site.sh)
                    siteSummary.siteHeading = site.sh;

                if (siteSummary.siteID <= 0)
                    siteSummary.siteID = int.Parse(site.sid.Substring(2));

                // recompute surveyComplete
                {
                    // The survey is complete when we know:
                    var complete = true;

                    // the site heading
                    complete &= siteSummary.siteHeading >= 0;

                    // live lat / long
                    complete &= !double.IsNaN(siteSummary.latitude);
                    complete &= !double.IsNaN(siteSummary.longitude);

                    // status of all POI
                    if (!SiteTemplate.sites.ContainsKey(site.t))
                        complete = false;
                    else
                    {
                        var template = SiteTemplate.sites[site.t];
                        var sumCountNonObelisks = template.poi
                            .Where(_ => _.type != POIType.obelisk && _.type != POIType.brokeObelisk)
                            .Count();
                        var sumCountPOI = (site.pp?.Split(",").Length ?? 0) + (site.pa?.Split(",").Length ?? 0) + (site.pe?.Split(",").Length ?? 0);
                        complete &= sumCountPOI == sumCountNonObelisks; // TODO: Compare with template count
                    }
                    // every obelisk group has at least 1 active obelisk
                    foreach (var g in site.og)
                        complete &= site.ao.Any(_ => _.name.StartsWith(g.ToString().ToUpperInvariant()));

                    siteSummary.surveyComplete = complete;
                }

                // match some Ruins?
                if (siteSummary.systemAddress < 0)
                    siteSummary.systemAddress = this.allRuins.FirstOrDefault(_ => _.systemName == siteSummary.systemName)?.systemAddress ?? -1;
                // match some other structure?
                if (siteSummary.systemAddress < 0)
                    siteSummary.systemAddress = this.allStructures.FirstOrDefault(_ => _.systemName == siteSummary.systemName)?.systemAddress ?? -1;

                // match some Ruins?
                if (siteSummary.bodyId < 0)
                    siteSummary.bodyId = this.allRuins.FirstOrDefault(_ => _.systemName == siteSummary.systemName && _.bodyName == siteSummary.bodyName)?.bodyId ?? -1;
                if (siteSummary.bodyId < 0)
                    siteSummary.bodyId = this.allStructures.FirstOrDefault(_ => _.systemName == siteSummary.systemName && _.bodyName == siteSummary.bodyName)?.bodyId ?? -1;

                if (siteSummary.systemAddress < 0 || siteSummary.bodyId < 0)
                {
                    var rslt = await Game.edsm.getBodies(siteSummary.systemName);
                    siteSummary.systemAddress = rslt.id64;
                    siteSummary.bodyId = rslt.bodies.FirstOrDefault(_ => _.name == $"{siteSummary.systemName} {siteSummary.bodyName}")?.bodyId ?? -1;
                }

                if (siteSummary.systemAddress < 0 || siteSummary.bodyId < 0)
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
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(2);

            GuardianSitePub site = null!;

            foreach (var row in rows)
            {
                var cells = row.Elements(XName.Get("Cell", "urn:schemas-microsoft-com:office:spreadsheet")).ToList();
                try
                {
                    // start a new site?
                    var siteId = cells[1].Value;
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        siteId = siteId.Replace(" ", "").Substring(0, 5);
                        var systemName = cells[2].Value;
                        var bodyName = cells[9].Value;
                        var idx = 1;
                        var siteTypeCell = cells[17].Value;
                        var siteType = siteTypeCell.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();
                        var latitude = double.Parse(cells[15].Value);
                        var longitude = double.Parse(cells[16].Value);

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
                            siteSummary = new GuardianStructureSummary()
                            {
                                // siteID is below
                                systemName = site.systemName,
                                bodyName = site.bodyName,
                                distanceToArrival = double.Parse(cells[10].Value),
                                siteType = site.t.ToString(),
                                idx = site.idx,
                                latitude = double.Parse(cells[15].Value),
                                longitude = double.Parse(cells[16].Value),

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
                                double.Parse(cells[3].Value),
                                double.Parse(cells[4].Value),
                                double.Parse(cells[5].Value),
                            };
                        }

                        continue;
                    }

                    // collect obelisk items and logs
                    var bank = cells[18].Value;
                    if (string.IsNullOrEmpty(bank))
                    {
                        Game.log($"Empty bank. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }
                    var bankIdx = cells[19].Value;

                    var item1 = cells[20].Value;
                    var item2 = cells[21].Value;
                    if (string.IsNullOrEmpty(item1) || item1 == "-")
                    {
                        // skip rows without items
                        Game.log($"Uncertain items. Skipping row:" + string.Join(", ", cells.Select(_ => _.Value)));
                        continue;
                    }

                    var msgNum = int.Parse(cells[22].Value);

                    var activeObelisk = new ActiveObelisk()
                    {
                        name = bank.ToUpperInvariant() + int.Parse(bankIdx).ToString("00"),
                        msg = $"#{msgNum}",
                    };
                    activeObelisk.items.Add(Enum.Parse<ObeliskItem>(item1, true));
                    if (!string.IsNullOrEmpty(item2) && item2 != "-")
                        activeObelisk.items.Add(Enum.Parse<ObeliskItem>(item2, true));

                    // remove any prior entries of the same name but otherwise different
                    var existingButDiff = site.ao.FirstOrDefault(_ => _.name == activeObelisk.name && _.ToString(true) != activeObelisk.ToString(true));
                    if (existingButDiff != null)
                        site.ao.Remove(existingButDiff);

                    if (!site.ao.Any(_ => _.ToString(true) == activeObelisk.ToString(true)))
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

        private List<GuardianStructureSummary> newStructures;

        public void readXmlSheetStructures()
        {
            this.newStructures = new List<GuardianStructureSummary>();

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
            var starPos = new double[3] /* x, y, z */ { double.Parse(cells[17].Value), double.Parse(cells[18].Value), double.Parse(cells[19].Value) };
            var siteType = cells[4].Value;

            var summary = new GuardianStructureSummary
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
                this.newStructures = JsonConvert.DeserializeObject<List<GuardianStructureSummary>>(json)!;
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
                var spanshResponse = await Game.spansh.getSystem(ruins.systemName);
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

                if (ruins.bodyId < 0 || ruins.distanceToArrival < 0 && ruins.systemName != "HIP 41730")
                {
                    // find a match in EDSM?
                    Game.log($"EDSM lookup bodies: {ruins.systemName}");
                    var edsmResponse = await Game.edsm.getBodies(ruins.systemName);
                    var fullBodyName = $"{ruins.systemName} {ruins.bodyName}";
                    var matchedBody = edsmResponse.bodies.FirstOrDefault(_ => _.name.Equals(fullBodyName, StringComparison.OrdinalIgnoreCase));
                    if (ruins.bodyId <= 0 && matchedBody?.bodyId > 0)
                    {
                        ruins.bodyId = matchedBody.bodyId;
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
    }
}
