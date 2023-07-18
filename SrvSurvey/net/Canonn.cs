using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.net.EDSM;
using SrvSurvey.units;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace SrvSurvey.canonn
{
    internal class Canonn
    {
        public void init()
        {
            // load static ruins summaries
            var json = File.ReadAllText(Canonn.allRuinsStaticPath);
            this.allRuins = JsonConvert.DeserializeObject<List<GuardianRuinSummary>>(json)!;
            Game.log($"Loaded {this.allRuins.Count} ruins.");

            /*
             if (Debugger.IsAttached)
                 await Game.canonn.prepareNewSummaries();
             // */

            //createRuinSummaries().ContinueWith((stuff) =>
            //{
            //    if (stuff.IsCompletedSuccessfully)
            //        ruinSummaries = stuff.Result;
            //    else
            //        Game.log($"Something bad happened in Canonn.init? {stuff.Exception}");
            //});
        }

        #region getSystemPoi

        public static async Task<SystemPoi> getSystemPoi(string systemName)
        {
            Game.log($"Requesting getSystemPoi: {systemName}");

            var json = await new HttpClient().GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/getSystemPoi?system={systemName}");
            var systemPoi = JsonConvert.DeserializeObject<SystemPoi>(json)!;

            Game.log(systemPoi!);

            return systemPoi;
        }

        public static async void biostats(long systemAddress, int bodyId)
        {
            var json = await new HttpClient().GetStringAsync($"https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/biostats?id={systemAddress}");

            JToken biostats = JsonConvert.DeserializeObject<JToken>(json)!;

            var bodies = biostats["system"]!["bodies"]!.Value<JArray>()!;

            var body = bodies[bodyId - 1].ToObject<Planet>()!;

            Game.log(body);
        }

        #endregion

        #region get all Guardian Ruins from GRSites

        private static string allRuinsRefPath = Path.Combine(Application.UserAppDataPath, "allRuins.json");
        private static string ruinSummariesPath = Path.Combine(Application.UserAppDataPath, "ruinSummaries.json");
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
            var summaries = this.allRuins; // new
            //var summaries = this.ruinSummaries; // old
            if (summaries == null)
            {
                Game.log("Why no ruinSummaries?");
                return new List<GuardianRuinEntry>();
            }

            var newEntries = new List<GuardianRuinEntry>();

            var allRuins = summaries.Select(_ => new GuardianRuinEntry(_)).ToList();
            var folder = Path.Combine(Application.UserAppDataPath, "guardian", Game.settings.lastFid!);
            if (Directory.Exists(folder))
            {
                var files = Directory.GetFiles(folder);

                Game.log($"Reading {files.Length} ruins files from disk");
                foreach (var filename in files)
                {
                    var data = Data.Load<GuardianSiteData>(filename)!;

                    var matches = allRuins.Where(_ => _.systemAddress == data.systemAddress
                        && _.bodyId == data.bodyId
                        && string.Equals(_.siteType.ToString(), data.type.ToString(), StringComparison.OrdinalIgnoreCase)
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
                            Game.log($"Unknown Guardian body: {data.bodyName} {data.name}");
                            // TODO: fabricate an entry from logs?
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

        #endregion

        #region parse Excel sheet of Ruins

        private static string allRuinsStaticPathDbg = "D:\\code\\SrvSurvey\\SrvSurvey\\allRuins.json";
        private static string allRuinsStaticPath = Debugger.IsAttached ? allRuinsStaticPathDbg : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "allRuins.json");
        public List<GuardianRuinSummary> allRuins { get; private set; }

        private List<GuardianRuinSummary> newSummaries;

        public async Task prepareNewSummaries()
        {
            if (!File.Exists(allRuinsStaticPathDbg)) return;

            // re-read XML if no partial progress
            if (!File.Exists(allRuinsStaticPathDbg))
            {
                readXmlSheet(allRuinsStaticPathDbg);
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

        public void readXmlSheet(string filepath)
        {
            this.newSummaries = new List<GuardianRuinSummary>();

            var doc = XDocument.Load(@"d:\code\Catalog of Guardian Structures and Ruins.xml");
            var table = doc.Root?.Elements().Where(_ => _.Name.LocalName == "Worksheet" && _.FirstAttribute?.Value == "Ruins").First().Elements()!;
            var rows = table.Elements(XName.Get("Row", "urn:schemas-microsoft-com:office:spreadsheet")).Skip(1);

            foreach (var row in rows)
                processXmlRow(row);

            // save new summaries
            this.saveNewSummaries();
        }

        private void processXmlRow(XElement row)
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
                var json = await new HttpClient().GetStringAsync($"https://api.canonn.tech/grreports?_limit=1&systemName={systemName}");
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
                var json = await new HttpClient().GetStringAsync($"https://api.canonn.tech/grreports?_limit=1&bodyName={bodyName}");
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

        #endregion
    }
}
