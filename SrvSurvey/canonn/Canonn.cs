using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SrvSurvey.canonn
{
    internal static class Canonn
    {
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
    }

}
