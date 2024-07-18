using BioCriterias;
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
            //Game.log($"Querying ");
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

        // ---

        //public string buildQuery(Dictionary<string, string> filters, string sortField, SortOrder sortOrder)
        //{
        //    // filters ...
        //    var partFilters = new List<string>();

        //    foreach (var f in filters.Where(f => !f.Value.Contains("/")))
        //    {
        //        // eg: ""atmosphere"": { ""value"": [ ""Thin Carbon dioxide"" ] }
        //        var values = JsonConvert.SerializeObject(f.Value.Split(','));
        //        var clause = $"\"{f.Key}\":{{\"value\":{values}}}";
        //        partFilters.Add(clause);
        //    }

        //    //partFilters.AddRange().Select(f =>
        //    //{
        //    //    var values = JsonConvert.SerializeObject(f.Value.Split(','));
        //    //    return $"\"{f.Key}\": {{ \"value\": [ {values} ] }}";
        //    //}));

        //    foreach (var f1 in filters.Where(f => f.Value.Contains("/")))
        //    {
        //        var fs = f1.Value.Split(',');
        //        foreach (var f2 in fs)
        //        {
        //            // eg: ""landmarks"": [ { ""type"": ""Stratum"", ""subtype"": [ ""Stratum Tectonicas"" ] } ]
        //            var parts = f2.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //            var clause = $"\"{f1.Key}\":[{{\"type\":\"{parts[0]}\",\"subtype\":[\"{parts[1]}\"]}}]";
        //            partFilters.Add(clause);
        //        }
        //    }

        //    //partFilters.AddRange(filters.Where(f => f.Value.Contains("/")).Select(f =>
        //    //{

        //    //    var parts = sortField.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //    //    return $"\"{f.Key}\": [ {{ \"type\": \"{parts[0]}\", \"subtype\": [ \"{parts[1]}\" ] }} ]";
        //    //}));

        //    //// eg: ""landmarks"": [ { ""type"": ""Stratum"", ""subtype"": [ ""Stratum Tectonicas"" ] } ]
        //    //partFilters.AddRange(subTypes.Select(f =>
        //    //{
        //    //    // TODO: handle multiples?
        //    //    var type = f.Value.Split(' ').First();
        //    //    return $"\"{f.Key}\": [ {{ \"type\": \"{type}\", \"subtype\": [ \"{f.Value}\" ] }} ]";
        //    //}));

        //    // sorting ...
        //    var dir = sortOrder == SortOrder.asc ? "asc" : "desc";
        //    string jsonSorts = "";
        //    if (sortField.Contains("/"))
        //    {
        //        // eg: { "atmosphere_composition": [ { "name": "Carbon dioxide", "direction"": "asc"" } ] }";
        //        var parts = sortField.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //        jsonSorts = $"{{\"{parts[0]}\":[{{\"name\":\"{parts[1]}\",\"direction\":\"{dir}\"}}]}}";
        //    }
        //    else
        //    {
        //        // eg: {"gravity":{"direction":"asc"}}
        //        jsonSorts = $"{{\"{sortField}\":{{\"direction\":\"{dir}\"}}}}";
        //    }

        //    // final result ...
        //    // eg: {"filters":{"subtype":{"value":["Rocky body"]},"atmosphere":{"value":["Thin Carbon dioxide"]},"volcanism_type":{"value":["No volcanism"]},"landmarks":[{"type":"Bacterium","subtype":["Bacterium Tela"]}]},"sort":[{"materials":[{"name":"Carbon","direction":"desc"}]}],"size":10,"page":0}
        //    var jsonFilters = string.Join(",", partFilters);
        //    var queryJson = $"{{\"filters\":{{{jsonFilters}}},\"sort\":[{jsonSorts}],\"size\":1,\"page\":0}}";
        //    return queryJson;
        //}

        public async Task<JObject> runQuery(string queryJson)
        {
            //Game.log($"Querying ");
            var body = new StringContent(queryJson, Encoding.ASCII, "application/json");

            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/bodies/search", body);
            var responseText = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<JObject>(responseText)!;
            return obj;
        }
    }

    internal class CriteriaBuilder
    {
        public static int limitMinCount = 5;
        public static double limitMinRatio = 0.2d;

        public async static Task<string> buildWholeSet(string species)
        {
            // define ...
            var atmosTypes = new List<string>()
            {
                "Ammonia",
                //"Ammonia-rich",
                //"Argon",
                //"ArgonRich",
                "Carbon dioxide",
                //"Carbon dioxide-rich",
                //"Helium",
                "Methane",
                //"Methane-rich",
                //"Neon",
                //"Neon-rich",
                //"Nitrogen",
                //"Oxygen",
                //"Sulphur dioxide",
                "Water",
                //"Water-rich",
            };
            var bodyTypes = new List<string>()
            {
                "Icy body",
                "Rocky body",
                "Rocky Ice world",
                "High metal content world",
            };

            // build ...
            var parent = new BioCriteria()
            {
                species = species.Split(" ").Last(),
                children = new List<BioCriteria>(),
                query = new List<Clause>(),
            };
            var maxCount = await getSpeciesTotalCount(species);
            Game.log($"Max count: {species} => {maxCount}");
            parent.query.Add(Clause.createComment($"hit count: {maxCount}"));

            foreach (var atmosType in atmosTypes)
            {
                // create a node per atmosphere type ...
                var atmos = new BioCriteria()
                {
                    query = new List<Clause>(),
                    children = new List<BioCriteria>(),
                };

                var atmosCount = await getSpeciesTotalCount(species, atmosType);
                Game.log($"Atmos count: {species}/{atmosType} => {atmosCount}");
                if (atmosCount < limitMinCount) continue;
                atmos.query.Add(Clause.createComment($"hit count: {atmosCount}"));
                atmos.query.Add(Clause.createIs("atmosType", Util.compositionToCamel(atmosType)));
                parent.children.Add(atmos);

                // ... and a node for each body
                foreach (var bodyType in bodyTypes)
                {
                    var child = await buildForBodyType(species, atmosType, bodyType, atmosCount);
                    if (child != null)
                        atmos.children.Add(child);
                }

                // shift common nodes up to the parent?
                if (atmos.children.Count > 1)
                {
                    var commonClauses = new List<Clause>();
                    foreach (var clause in atmos.children.First().query)
                    {
                        if (clause.op == Op.Comment) continue;

                        var txt = clause.ToString();
                        var allChildren = atmos.children.All(child => child.query.Any(q => q.ToString() == txt));
                        if (allChildren)
                            commonClauses.Add(clause);
                    }

                    foreach (var clause in commonClauses)
                    {
                        var txt = clause.ToString();
                        atmos.query.Add(clause);
                        atmos.children.ForEach(child => child.query.RemoveAll(q => q.ToString() == txt));
                    }
                }
            }

            var json = JsonConvert.SerializeObject(parent, Formatting.Indented) + ",";
            Clipboard.SetText(json);
            return json;
        }

        private static async Task<BioCriteria?> buildForBodyType(string species, string atmosType, string bodyType, int maxCount)
        {
            Game.log($"buildFor: {species}/{atmosType}/{bodyType} ...");
            var criteria = new BioCriteria()
            {
                useCommonChildren = true,
                query = new List<Clause>(),
            };

            // add these by default
            var hitCount = await getSpeciesTotalCount(species, atmosType, bodyType);
            criteria.query.Add(Clause.createComment($"hit count: {hitCount}"));
            criteria.query.Add(Clause.createIs("body", bodyMap[bodyType]));

            // Compare ratio of hits against maxCount - exit early if this is below 0.2%
            var ratio = 100.0d / (double)maxCount * (double)hitCount;
            Game.log($"Body count: {species}/{atmosType}/{bodyType} => {hitCount} vs {maxCount} => ratio: {ratio.ToString("N1")}%");
            if (ratio < limitMinRatio || hitCount < limitMinCount) return null;

            // gravity
            var cg = await buildRangeClause(species, atmosType, bodyType, "gravity", 0.04f, float.MaxValue);
            if (cg != null) criteria.query.Add(cg);
            else return null; // exit early if this permutation is bogus

            // temp
            var ct = await buildRangeClause(species, atmosType, bodyType, "surface_temperature", 20, float.MaxValue);
            if (ct != null) criteria.query.Add(ct);

            // pressure
            var cp = await buildRangeClause(species, atmosType, bodyType, "surface_pressure", 0.001f, 0.097f);
            if (cp != null) criteria.query.Add(cp);

            // any volcanism?
            var vc = await buildVolcanismClause(species, atmosType, bodyType);
            if (vc != null) criteria.query.Add(vc);

            // any atmospheric composition? (skip for "-rich" cases)
            if (!atmosType.Contains("rich"))
            {
                var va = await buildAtmosphericCompositionClause(species, atmosType, bodyType);
                if (va != null) criteria.query.Add(va);
            }

            return criteria;
        }

        private static async Task<Clause?> buildAtmosphericCompositionClause(string species, string atmosType, string bodyType)
        {
            var type = species.Split(' ').First();
            var filters = new Dictionary<string, string>()
            {
                { "atmosphere", $"Thin {atmosType}" },
                { "subtype", bodyType },
                { "landmarks", $"{type}/{species}" },
            };

            var response = await Game.spansh.runQuery(buildQuery(filters, $"atmosphere_composition/{atmosType}", SortOrder.asc));
            var fullCount = response["count"]!.Value<int>();
            if (fullCount == 0) return null;

            var atmosComp = response["results"]!.ToArray().First()["atmosphere_composition"]!.FirstOrDefault(ac => ac["name"]!.Value<string>() == atmosType)!;
            if (atmosComp == null) return null;

            var name = Util.compositionToCamel(atmosComp["name"]!.Value<string>()!);
            var value = atmosComp["share"]!.Value<float>();

            // adjust values?

            // assume no correlation if less than ~60%
            if (value < 60)
                return null;

            // assume 100%?
            if (value > 98)
            {
                value = 100;
            }
            else if (value > 97 && value < 98)
            {
                // This is common for Carbon dioxide
                value = 97.5f;
            }
            else
            {
                // otherwise round down to nearest 5
                var d = value % 5;
                value = value - d;
            }

            var clause = Clause.createCompositions("atmosComp", new Dictionary<string, float>() { { name, value } });
            return clause;
        }

        private static async Task<Clause?> buildVolcanismClause(string species, string atmosType, string bodyType)
        {
            var type = species.Split(' ').First();
            var filters = new Dictionary<string, string>()
            {
                { "atmosphere", $"Thin {atmosType}" },
                { "subtype", bodyType },
                { "landmarks", $"{type}/{species}" },
            };

            // any hits with "No volcanism" ?
            filters["volcanism_type"] = "No volcanism";
            var response = await Game.spansh.runQuery(buildQuery(filters, "volcanism_type", SortOrder.asc));
            var withNoVolcanism = response["results"]!.ToList();

            // any hits with anything but "No volcanism" ?
            filters["volcanism_type"] = string.Join(',', allVolcanisms);
            //Game.log(filters["volcanism_type"]);
            response = await Game.spansh.runQuery(buildQuery(filters, "volcanism_type", SortOrder.asc));
            var withSomeVolcanism = response["results"]!.ToList();

            // zero record with any volcanism
            if (withNoVolcanism.Count > 0 && withSomeVolcanism.Count == 0)
            {
                return Clause.createIs("volcanism", "None");
            }

            // We saw some volcanism, find the rest of them
            var matchedVolcanism = new HashSet<string>();
            if (withSomeVolcanism.Count > 0)
            {
                var moreVolcanism = withSomeVolcanism;
                string volcanism;
                do
                {
                    volcanism = moreVolcanism.First()["volcanism_type"]!.Value<string>()!;
                    matchedVolcanism.Add(volcanism);

                    // query again without that
                    filters["volcanism_type"] = string.Join(',', allVolcanisms.Where(v => !matchedVolcanism.Contains(v)));
                    //Game.log(filters["volcanism_type"]);
                    response = await Game.spansh.runQuery(buildQuery(filters, "volcanism_type", SortOrder.asc));
                    moreVolcanism = response["results"]!.ToList();

                    // just say "Some" if more than 4
                    if (matchedVolcanism.Count > 4)
                    {
                        matchedVolcanism.Clear();
                        matchedVolcanism.Add("Some");
                        break;
                    }

                } while (moreVolcanism.Count > 0);
            }

            if (withNoVolcanism.Count == 0 && matchedVolcanism.Count > 0)
            {
                var clause = Clause.createIs("volcanism", matchedVolcanism.ToList());
                return clause;
            }

            // otherwise, it appears volcanism is not a factor
            return null;
        }

        private static async Task<Clause?> buildRangeClause(string species, string atmosType, string bodyType, string property, float minLimit, float maxLimit)
        {
            // get min and max values
            var objMin = await Game.spansh.runQuery(buildQuery(atmosType, bodyType, species, property, SortOrder.asc));
            if (objMin["count"]!.Value<int>() == 0) return null;
            var minRaw = objMin["results"]!.ToArray().First()[property]!.Value<float>();

            var objMax = await Game.spansh.runQuery(buildQuery(atmosType, bodyType, species, property, SortOrder.desc));
            var maxRaw = objMax["results"]!.ToArray().First()[property]!.Value<float>();

            // special processing?

            if (property == "gravity")
            {
                // round to nearest 3 decimal places
                minRaw = (float)Math.Floor(minRaw * 1_000) / 1_000;
                maxRaw = (float)Math.Ceiling(maxRaw * 1_000) / 1_000;
            }

            if (property == "surface_pressure")
            {
                // round to nearest 3 decimal places
                minRaw = (float)Math.Floor(minRaw * 10_000) / 10_000;
                maxRaw = (float)Math.Ceiling(maxRaw * 10_000) / 10_000;
            }

            if (property == "surface_temperature")
            {
                // round to nearest whole number
                minRaw = (float)Math.Floor(minRaw);
                maxRaw = (float)Math.Ceiling(maxRaw);
            }

            var clause = Clause.createRange(
                propMap[property],
                minRaw >= minLimit ? minRaw : null,
                maxRaw <= maxLimit ? maxRaw : null);

            if (clause.min == null && clause.max == null)
                return null;

            return clause;
        }

        private static async Task<int> getSpeciesTotalCount(string species, string? atmosType = null, string? bodyType = null)
        {
            // get a count of species without atmosphere or body filters. Exit early if this is below 1%
            var response = await Game.spansh.runQuery(buildQuery(atmosType, bodyType, species, "gravity", SortOrder.asc));
            var totalCount = response["count"]!.Value<int>();
            return totalCount;
        }

        private static string buildQuery(string? atmosType, string? bodyType, string speciesType, string sortField, SortOrder sortOrder)
        {
            var type = speciesType.Split(' ').First();
            var filters = new Dictionary<string, string>()
            {
                { "landmarks", $"{type}/{speciesType}" },
            };

            if (atmosType != null)
                filters.Add("atmosphere", $"Thin {atmosType}");
            if (bodyType != null)
                filters.Add("subtype", bodyType);

            return buildQuery(filters, sortField, sortOrder);
        }

        private static string buildQuery(Dictionary<string, string> filters, string sortField, SortOrder sortOrder)
        {
            // filters ...
            var partFilters = new List<string>();

            foreach (var f in filters.Where(f => !f.Value.Contains("/")))
            {
                // eg: ""atmosphere"": { ""value"": [ ""Thin Carbon dioxide"" ] }
                var values = JsonConvert.SerializeObject(f.Value.Split(','));
                var clause = $"\"{f.Key}\":{{\"value\":{values}}}";
                partFilters.Add(clause);
            }

            foreach (var f1 in filters.Where(f => f.Value.Contains("/")))
            {
                var fs = f1.Value.Split(',');
                foreach (var f2 in fs)
                {
                    // eg: ""landmarks"": [ { ""type"": ""Stratum"", ""subtype"": [ ""Stratum Tectonicas"" ] } ]
                    var parts = f2.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var clause = $"\"{f1.Key}\":[{{\"type\":\"{parts[0]}\",\"subtype\":[\"{parts[1]}\"]}}]";
                    partFilters.Add(clause);
                }
            }

            // sorting ...
            var dir = sortOrder == SortOrder.asc ? "asc" : "desc";
            string jsonSorts = "";
            if (sortField.Contains("/"))
            {
                // eg: { "atmosphere_composition": [ { "name": "Carbon dioxide", "direction"": "asc"" } ] }";
                var parts = sortField.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                jsonSorts = $"{{\"{parts[0]}\":[{{\"name\":\"{parts[1]}\",\"direction\":\"{dir}\"}}]}}";
            }
            else
            {
                // eg: {"gravity":{"direction":"asc"}}
                jsonSorts = $"{{\"{sortField}\":{{\"direction\":\"{dir}\"}}}}";
            }

            // final result ...
            // eg: {"filters":{"subtype":{"value":["Rocky body"]},"atmosphere":{"value":["Thin Carbon dioxide"]},"volcanism_type":{"value":["No volcanism"]},"landmarks":[{"type":"Bacterium","subtype":["Bacterium Tela"]}]},"sort":[{"materials":[{"name":"Carbon","direction":"desc"}]}],"size":10,"page":0}
            var jsonFilters = string.Join(",", partFilters);
            var queryJson = $"{{\"filters\":{{{jsonFilters}}},\"sort\":[{jsonSorts}],\"size\":1,\"page\":0}}";
            return queryJson;
        }

        private static List<string> allVolcanisms = new List<string>()
        { "Carbon Dioxide Geysers", "Major Carbon Dioxide Geysers", "Major Metallic Magma", "Major Rocky Magma", "Major Silicate Vapour Geysers", "Major Water Geysers", "Major Water Magma", "Metallic Magma", "Minor Ammonia Magma", "Minor Carbon Dioxide Geysers", "Minor Metallic Magma", "Minor Methane Magma", "Minor Nitrogen Magma", "Minor Rocky Magma", "Minor Silicate Vapour Geysers", "Minor Water Geysers", "Minor Water Magma", "Rocky Magma", "Silicate Vapour Geysers", "Water Geysers", "Water Magma" };
        private static Dictionary<string, string> propMap = new Dictionary<string, string>()
        {
            { "gravity", "gravity" },
            { "surface_temperature", "temp" },
            { "surface_pressure", "pressure" },
            { "HMC", "High metal content world" },
        };
        private static Dictionary<string, string> bodyMap = new Dictionary<string, string>()
        {
            { "Icy body", "Icy" },
            { "Rocky body", "Rocky" },
            { "Rocky Ice world", "RockyIce" },
            { "High metal content world", "HMC" },
        };
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

    public enum SortOrder
    {
        asc,
        desc,
    }
}
