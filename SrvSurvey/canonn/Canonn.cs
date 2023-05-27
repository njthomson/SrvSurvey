using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Text;


namespace SrvSurvey.canonn
{
    internal class Canonn
    {
        public void init()
        {
            createRuinSummaries().ContinueWith(stuff =>
            {
                if (stuff.IsCompletedSuccessfully)
                    ruinSummaries = stuff.Result;
                else
                    Game.log($"Something bad happened in Canonn.init? {stuff.Exception}");
            });
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
        public IEnumerable<GuardianRuinSummary> ruinSummaries { get; private set; }

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
                Game.log($"{response.StatusCode} : {response.IsSuccessStatusCode}");
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
            if (ruinSummaries == null)
            {
                Game.log("Why no ruinSummaries?");
                return new List<GuardianRuinEntry>();
            }

            var newEntries = new List<GuardianRuinEntry>();

            var allRuins = ruinSummaries.Select(_ => new GuardianRuinEntry(_)).ToList();
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
                            Game.log(dist);
                            if (dist < 0.0005M)
                            {
                                entry = match;
                                //Game.log($"Matched {data.name} on distance!");
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(data.systemName) && matches.Any())
                    {
                        data.systemName = matches.First().systemName;
                        data.bodyName = data.systemName + " " + matches.First().bodyName;
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
                            var newEntry = GuardianRuinEntry.from(data, matches.First());
                            newEntries.Add(newEntry);
                        }
                    }
                }

                // remove an unmatchable entry for each new one
                foreach (var newEntry in newEntries)
                {
                    var victim = allRuins.First(_ => _.systemAddress == newEntry.systemAddress && _.bodyId == newEntry.bodyId && string.Compare(_.siteType, newEntry.siteType, true) == 0);
                    allRuins.Remove(victim);
                    allRuins.Add(newEntry);
                }
            }

            return allRuins;
        }

        #endregion
    }

}
