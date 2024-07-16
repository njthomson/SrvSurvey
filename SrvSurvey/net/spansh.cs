using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Text;

namespace SrvSurvey.net
{
    internal class Spansh
    {
        private static HttpClient client;

        static Spansh()
        {
            Spansh.client = new HttpClient();
            //Spansh.client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            Spansh.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        public async Task<GetSystemResponse> getSystem(string systemName)
        {
            Game.log($"Requesting api/systems: {systemName}");

            var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/systems/field_values/system_names?q={Uri.EscapeDataString(systemName)}");
            var systems = JsonConvert.DeserializeObject<GetSystemResponse>(json)!;
            return systems;
        }

        public async Task<ApiSystemDumpSystem> getSystemDump(long systemAddress)
        {
            Game.log($"Requesting getSystem: {systemAddress}");

            var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/dump/{systemAddress}/");
            var systemDump = JsonConvert.DeserializeObject<ApiSystemDump>(json)!;
            return systemDump.system;
        }

        public async Task getMinorFactionSystems()
        {
            Game.log($"Requesting foo");

            var factions = new List<string>()
            {
                "Raven Colonial Corporation",
                "Steven Gordon Jolliffe",
                "Elite Secret Service",
                "The Blue Brotherhood",
                "Guardians of Cygnus"
            };

            var factionsTxt = string.Join(",", factions.Select(_ => $"\"{_}\""));
            var json = "{\"filters\":{\"minor_faction_presences\":{\"value\":[" + factionsTxt + "]}},\"sort\":[{\"distance\":{\"direction\":\"asc\"}}],\"size\":200,\"page\":0,\"reference_system\":\"Banka\"}";

            var body = new StringContent(json, Encoding.ASCII, "application/json");

            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/systems/search", body);
            var responseText = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<SystemsSearchResults>(responseText)!;

            var foos = new List<MinorFactionSystem>();
            foreach (var system in results.results)
            {
                var foo = new MinorFactionSystem
                {
                    name = system.name,
                    coords = new MinorFactionSystemCoords { x = system.x, y = system.y, z = system.z },
                    cat = new List<int> { },
                    infos = $"Population: {system.population.ToString("N0")}%3Cbr%20%3E"
                };

                foreach (var fac in system.minor_faction_presences)
                {
                    var idx = factions.IndexOf(fac.name);
                    if (idx != -1)
                    {
                        foo.cat.Add(idx);
                        foo.infos += "%3Cbr%20%3E" + $"{fac.name}: {fac.influence.ToString("N2")}%";
                    }
                }

                switch (system.name)
                {
                    case "Banka": // Raven Colonial Corporation
                    case "Hungalun": //Steven Gordon Jolliffe
                    case "Loperada": // Elite Secret Service
                    case "Bhumians": // The Blue Brotherhood
                    case "Dyavansana": // Guardians of Cygnus
                        foo.cat[0] = 99;
                        break;
                }

                foos.Add(foo);
            }

            var txt = JsonConvert.SerializeObject(foos, Formatting.None)
                .Replace("]},", "]},\r\n");
            txt = txt.Insert(1, "\r\n");
            txt = txt.Insert(txt.Length - 1, "\r\n");
            Clipboard.SetText(txt);

            Game.log($"Processed {results.size} of {results.count} results");
        }

        public async Task getClause(string genus, string species, string gas)
        {
            Game.log($"Querying ");
            var queryJson = @"{ ""filters"": { ""atmosphere"": { ""value"": [ ""Thin Carbon dioxide"" ] }, ""landmarks"": [ { ""type"": ""Stratum"", ""subtype"": [ ""Stratum Tectonicas"" ] } ] }, ""sort"": [ { ""atmosphere_composition"": [ { ""name"": ""Carbon dioxide"", ""direction"": ""asc"" } ] } ], ""size"": 1, ""page"": 0 }";
            var query = JsonConvert.DeserializeObject<JObject>(queryJson)!;

            // replace key parts with parameters
            query["filters"]!["landmarks"]![0]!["type"] = genus;
            query["filters"]!["landmarks"]![0]!["subtype"]![0] = $"{genus} {species}";
            query["filters"]!["atmosphere"]!["value"]![0] = $"Thin {gas}";
            query["sort"]![0]!["atmosphere_composition"]![0]!["name"] = gas;


            var json = JsonConvert.SerializeObject(query);
            var body = new StringContent(json, Encoding.ASCII, "application/json");

            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/bodies/search", body);
            var responseText = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<SystemsSearchResults>(responseText)!;
            var obj = JsonConvert.DeserializeObject<JObject>(responseText)!;

            if (obj["count"]!.Value<int>() == 0)
            {
                Game.log("No results?");
                return;
            }

            var atmosComp = obj["results"]!.ToArray().First()["atmosphere_composition"]!.First(ac => ac["name"]!.Value<string>() == gas)!;
            var name = Util.compositionToCamel(atmosComp["name"]!.Value<string>()!);
            var value = atmosComp["share"]!.Value<float>()!.ToString("n2");
            if (value == "100.00") value = "100";

            var clause = $"\"atmosComp [{name} >= {value}]\",";

            Game.log($"'{gas}' clause for '{genus} {species}' : {clause}");
            Clipboard.SetText(clause);
        }

    }

    internal class ApiSystemDump
    {
        public ApiSystemDumpSystem system;
    }

    internal class ApiSystemDumpSystem
    {
        public string allegiance;
        public int bodyCount;
        public List<ApiSystemDumpBody> bodies;
        public StarPos coords;
        public DateTimeOffset date;
        public string government;
        public long id64;
        public string name;
        public string primaryEconomy;
        public string secondaryEconomy;
        public string security;
        public List<Station> stations;

        public class Station
        {
            public string controllingFaction;
            public string controllingFactionState;
            public double distanceToArrival;
            // TODO: economies
            public string government;
            public long id;
            public LandingPads landingPads;
            // TODO: market
            public string name;
            public string primaryEconomy;
            public List<string> services;
            public string type;
            public DateTime updateTime;
        }
    }

    internal class ApiSystemDumpBody
    {
        public double? absoluteMagnitude;
        public long? age;
        public double? axialTilt;
        public int bodyId;
        public double? distanceToArrival;
        public double? semiMajorAxis;
        public long? id64;
        public string luminosity;
        public bool? mainStar;
        public string name;
        public double? rotationalPeriod;
        public bool? rotationalPeriodTidallyLocked;
        public double? solarMasses;
        public double? solarRadius;
        public List<Dictionary<ParentBodyType, int>> parents;
        public string? spectralClass;
        // TODO stations[]
        public string subType;
        public double? surfaceTemperature;
        public double? surfacePressure;
        public string type;
        public DateTimeOffset updateTime;

        public double? earthMasses;
        public decimal? radius;
        public double? gravity;
        public string? atmosphereType;
        public Dictionary<string, float>? atmosphereComposition;
        public Dictionary<string, float>? materials;
        public string? volcanismType;
        public bool? isLandable;
        public string? terraformingState;

        public ApiSystemDumpSignals? signals;

        // TODO? parents[]

        public List<ApiSystemDumpRing>? rings;
        public List<ApiSystemDumpStation> stations;
    }

    internal class ApiSystemDumpStation
    {
        public long id;
        public string government;
        public string name;
        public string type;
        public string primaryEconomy;
        public Dictionary<string, int> landingPads;
        // TODO: market ?
        // TODO: outfitting ?
        public List<string> services;
    }

    internal class ApiSystemDumpSignals
    {
        public List<string>? genuses;
        public Dictionary<string, int>? signals;
        public DateTimeOffset updateTime;
    }

    internal class ApiSystemDumpRing
    {
        public string name;
        public string type;
        public double mass;
        public double innerRadius;
        public double outerRadius;
        public ApiSystemDumpSignals? signals;
    }

    class SystemsSearchResults
    {
        public int count;
        public int from;
        public int size;
        public List<SystemsSearchResult> results;
    }

    class SystemsSearchResult
    {
        public string name;
        public double x;
        public double y;
        public double z;
        public long population;
        public List<MinorFactionPresence> minor_faction_presences;
    }



    class MinorFactionSystem
    {
        public string name;
        public MinorFactionSystemCoords coords;
        public string infos;
        public List<int> cat;
    }

    class MinorFactionPresence
    {
        public float influence;
        public string name;
        public string state;
    }

    internal class MinorFactionSystemCoords
    {
        public double x;
        public double y;
        public double z;
    }
}
