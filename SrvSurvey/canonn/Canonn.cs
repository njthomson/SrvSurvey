using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;


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
        private static IEnumerable<GuardianRuinSummary> ruinSummaries;

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

            return summaries;
        }

        public async Task<List<GuardianRuinEntry>> loadAllRuins()
        {
            var summaries = await createRuinSummaries();

            var allRuins = summaries.Select(_ => new GuardianRuinEntry(_)).ToList();

            var folder = Path.Combine(Application.UserAppDataPath, "guardian", "F6985613"); // Game.activeGame!.fid!);
            var files = Directory.GetFiles(folder);

            Game.log($"Reading {files.Length} ruins files from disk");
            foreach (var filename in files)
            {
                var data = Data.Load<GuardianSiteData>(filename)!;
                //Game.log(data);

                // match type (or not) AND Ruins #
                var entry = allRuins.Find(_ => _.systemAddress == data.systemAddress);
                if (entry == null)
                {
                    Game.log($"Why no matcing entry for: {data.systemAddress} ?");
                    continue;
                }

                entry.merge(data);
            }

            return allRuins;
        }

        #endregion
    }

}
