using BioCriterias;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Text;

namespace SrvSurvey.net
{
    internal partial class Spansh
    {
        private static HttpClient client;

        static Spansh()
        {
            Spansh.client = new HttpClient();
            //Spansh.client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            Spansh.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        public async Task<GetSystemResponse> getSystems(string systemName)
        {
            return await NetCache.query(systemName, async () =>
            {
                Game.log($"Searching Spansh api/systems by name: {systemName}");

                // https://spansh.co.uk/api/systems/field_values/system_names?q=Colonia
                var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/systems/field_values/system_names?q={Uri.EscapeDataString(systemName)}");
                var systems = JsonConvert.DeserializeObject<GetSystemResponse>(json)!;
                return systems;
            });
        }

        public Task<long> getSystemAddress(string systemName)
        {
            return NetCache.query(systemName, async () =>
            {
                Game.log($"Searching Spansh api/systems for SystemAddress by name: {systemName}");

                // https://spansh.co.uk/api/systems/field_values/system_names?q=Colonia
                var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/systems/field_values/system_names?q={Uri.EscapeDataString(systemName)}");
                var systems = JsonConvert.DeserializeObject<GetSystemResponse>(json)!;

                var firstMatch = systems.min_max.FirstOrDefault();

                if (firstMatch?.name == systemName)
                    return firstMatch.id64;

                return 0;
            });
        }

        public async Task<ApiSystem.Record> getSystem(long systemAddress)
        {
            Game.log($"Requesting Spansh api/system/{systemAddress}");

            // https://spansh.co.uk/api/system/3238296097059 (Colonia)
            var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/system/{systemAddress}");
            var system = JsonConvert.DeserializeObject<ApiSystem>(json)!;
            return system.record;
        }

        public Task<ApiSystemDump.System> getSystemDump(long systemAddress, bool useCache = false)
        {
            return NetCache.query(systemAddress, async () =>
            {
                var cacheFilename = Path.Combine(BioPredictor.netCache, $"getSystemDump-{systemAddress}.json");
                string json;
                useCache = useCache || BioPredictor.useTestCache;
                if (useCache && File.Exists(cacheFilename))
                {
                    json = File.ReadAllText(cacheFilename);
                }
                else
                {
                    // https://spansh.co.uk/api/dump/3238296097059 (Colonia)
                    Game.log($"Requesting Spansh api/dump/{systemAddress}");
                    json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/dump/{systemAddress}/");
                    if (useCache)
                    {
                        Directory.CreateDirectory(BioPredictor.netCache);
                        File.WriteAllText(cacheFilename, json);
                    }
                }

                var systemDump = JsonConvert.DeserializeObject<ApiSystemDump>(json)!;
                return systemDump.system;
            });
        }

        public async Task<SystemResponse> getBoxelSystems(string systemName, StarRef? from = null)
        {
            var cacheKey = $"{systemName}{from}";
            return await NetCache.query(cacheKey, async () =>
            {
                Game.log($"getBoxelSystems: {systemName}");

                var q = new SystemQuery
                {
                    page = 0,
                    size = 50,
                    sort = new() { new("name", SortOrder.asc) },
                    filters = new() { { "name", new SystemQuery.Value(systemName) } }
                };

                if (from != null) q.reference_coords = from;

                var obj = await this.querySystems(q);
                Game.log(obj);
                return obj;
            });
        }

        public async Task getMinorFactionSystems()
        {
            Game.log($"Requesting api/systems/search for factions by body");

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
            if (responseText == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + responseText);
            }

            var results = JsonConvert.DeserializeObject<SystemsSearchResults>(responseText)!;

            var foos = new List<MinorFactionSystem>();
            foreach (var system in results.results)
            {
                var foo = new MinorFactionSystem
                {
                    name = system.name,
                    coords = new MinorFactionSystem.Coords { x = system.x, y = system.y, z = system.z },
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
            if (responseText == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + responseText);
            }

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

        public async Task<JObject> queryBodies(string queryJson)
        {
            //Game.log($"Posting Spansh api/bodies/search with queryJson");

            var body = new StringContent(queryJson, Encoding.ASCII, "application/json");

            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/bodies/search", body);
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + responseText);
            }

            var obj = JsonConvert.DeserializeObject<JObject>(responseText)!;
            return obj;
        }

        public async Task<JObject> querySystems(string queryJson)
        {
            //Game.log($"Posting Spansh api/systems/search with queryJson");

            var body = new StringContent(queryJson, Encoding.ASCII, "application/json");

            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/systems/search", body);
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + responseText);
            }

            var obj = JsonConvert.DeserializeObject<JObject>(responseText)!;
            return obj;
        }

        public async Task<SearchApiResults?> buildMissingVariantsQuery(StarPos starPos, string genus, string species, List<string> variantColors)
        {
            var json = CriteriaBuilder.buildMissingVariantsForSpecies(starPos, genus, species, variantColors);

            var response = await Game.spansh.queryBodies(json);
            var results = response.ToObject<SearchApiResults>();
            Game.log($"Found {results?.count} total hits from: https://spansh.co.uk/bodies/search/{results?.search_reference}/1");

            return results;
        }
    }

    internal class GetSystemResponse
    {
        // {"min_max":[{"id64":10477373803,"name":"Sol","x":0.0,"y":0.0,"z":0.0},{"id64":1458376315610,"name":"Solati","x":66.53125,"y":29.1875,"z":34.6875},{"id64":5059379007779,"name":"Solitude","x":-9497.65625,"y":-911.0,"z":19807.625},{"id64":5267550898539,"name":"Solibamba","x":99.5625,"y":40.125,"z":26.8125},{"id64":11538024121505,"name":"Sollaro","x":-9528.625,"y":-885.59375,"z":19815.4375}],"values":["Sol","Solati","Solitude","Solibamba","Sollaro"]}

        public List<StarRef> min_max;
        public List<string> values;
    }

    internal static class CriteriaBuilder
    {
        public static int limitMinCount = 5;
        public static double limitMinRatio = 0.2d;
        public static double limitVolcanismTypes = 5;

        private static bool shouldForceVolcanismNone = false;
        private static bool forceVolcanismNone = false;

        public static void buildWholeSet()
        {
            // define ...
            var species = "Bacterium/Bacterium Tela";
            var atmosTypes = new List<string>()
            {
                //"No atmosphere",
                //"Ammonia",
                //"Ammonia-rich",
                //"Argon",
                //"Argon-rich",
                //"Carbon dioxide",
                //"Carbon dioxide-rich",
                //"Helium",
                //"Methane",
                //"Methane-rich",
                //"Neon",
                //"Neon-rich",
                //"Nitrogen",
                //"Oxygen",
                "Sulphur dioxide",
                //"Water",
                //"Water-rich",
            };

            var start = DateTime.Now;
            CriteriaBuilder.buildSpecies(species, atmosTypes).ContinueWith(task =>
            {
                Game.log($"CriteriaBuilder.buildWholeSet => {task.Status}");
                if (task.Exception != null) Game.log(task.Exception);
                Game.log($"** Done: {species} ** {DateTime.Now.Subtract(start)}");
            });
        }

        public async static Task<string> buildSpecies(string species, List<string> atmosTypes)
        {
            var bodyTypes = new List<string>()
            {
                "Icy body",
                "Rocky body",
                "Rocky Ice world",
                "High metal content world",
                //"Metal-rich body",
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
                var atmosTypeValue = atmosType == "No atmosphere" ? "None" : Util.compositionToCamel(atmosType);
                atmos.query.Add(Clause.createIs("atmosType", atmosTypeValue));
                parent.children.Add(atmos);

                // ... and a node for each body
                foreach (var bodyType in bodyTypes)
                {
                    shouldForceVolcanismNone = false;
                    var child = await buildForBodyType(species, atmosType, bodyType, atmosCount);
                    if (child != null)
                    {
                        atmos.children.Add(child);

                        if (shouldForceVolcanismNone)
                        {
                            child.query.Add(Clause.createComment("Excluding volcanism:none"));

                            // generate a sibling clause that has `volcanism:none` in all criteria
                            forceVolcanismNone = true;
                            var child2 = await buildForBodyType(species, atmosType, bodyType, atmosCount);
                            if (child2 != null)
                            {
                                atmos.children.Add(child2);
                                child2.query.Add(Clause.createComment("Forcing only volcanism:none"));
                            }
                            forceVolcanismNone = false;
                        }
                    }
                }

                if (atmos.children.Count == 1)
                {
                    // we don't need to use the children array if there is only one
                    atmos.query.AddRange(atmos.children.First().query);
                    atmos.useCommonChildren = true;
                    atmos.children.Clear();
                }
                else if (atmos.children.Count > 1)
                {
                    // shift common clauses up to the parent?
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

            if (parent.children.Count == 1)
            {
                // we don't need to use the children array if there is only one
                var firstChild = parent.children.First();
                parent.query.AddRange(firstChild.query);
                parent.useCommonChildren = firstChild.useCommonChildren;
                parent.children.Clear();
                parent.children.AddRange(firstChild.children);
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
                { "atmosphere", getAtmosphereFilterValue(atmosType) },
                { "subtype", bodyType },
                { "landmarks", getLandmarksFilterValue(species) },
                //{ "surface_temperature", "160.00 <=> 177.00" }, // TODO: Support one day
            };

            var response = await Game.spansh.queryBodies(buildQuery(filters, $"atmosphere_composition/{atmosType}", SortOrder.asc));
            var fullCount = response["count"]!.Value<int>();
            if (fullCount == 0) return null;

            var atmosComp = response["results"]!.ToArray().First()["atmosphere_composition"]?.FirstOrDefault(ac => ac["name"]!.Value<string>() == atmosType)!;
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
                value = 97.5f;
            }
            else
            {
                // otherwise round down to nearest 5
                var d = value % 5;
                value = value - d;
            }

            // Check for Sulphur dioxide too? This is common for Carbon dioxide
            if (atmosType == "Carbon dioxide" && value >= 97.5f)
            {
                response = await Game.spansh.queryBodies(buildQuery(filters, $"atmosphere_composition/{atmosType}", SortOrder.desc));
                fullCount = response["count"]!.Value<int>();
                if (fullCount > 0)
                {
                    atmosComp = response["results"]!.ToArray().First()["atmosphere_composition"]!.FirstOrDefault(ac => ac["name"]!.Value<string>() == atmosType)!;
                    value = atmosComp["share"]!.Value<float>();

                    var response2 = await Game.spansh.queryBodies(buildQuery(filters, $"atmosphere_composition/Sulphur dioxide", SortOrder.asc));
                    var fullCount2 = response2["count"]!.Value<int>();
                    if (fullCount2 > limitMinCount)
                    {
                        var atmosComp2 = response2["results"]!.ToArray().First()["atmosphere_composition"]!.FirstOrDefault(ac => ac["name"]!.Value<string>() == "Sulphur dioxide")!;
                        var value2 = atmosComp2["share"]!.Value<float>();
                        if (value2 > 2.5f) Debugger.Break();

                        var compositions = new Dictionary<string, float>();
                        compositions.Add("CarbonDioxide", floor(value, 2));

                        if (value2 >= 0.96f && value2 <= 2.5f)
                        {
                            compositions.Add("SulphurDioxide", floor(value2, 2));
                            return Clause.createCompositions("atmosComp", compositions);
                        }
                        else
                            Debugger.Break();
                    }
                }
            }

            var clause = Clause.createCompositions("atmosComp", new Dictionary<string, float>() { { name, value } });
            return clause;
        }

        private static async Task<Clause?> buildVolcanismClause(string species, string atmosType, string bodyType)
        {
            if (forceVolcanismNone)
                return Clause.createIs("volcanism", "None");

            var type = species.Split(' ').First();
            var filters = new Dictionary<string, string>()
            {
                { "atmosphere", getAtmosphereFilterValue(atmosType) },
                { "subtype", bodyType },
                { "landmarks", getLandmarksFilterValue(species) },
            };

            // any hits with "No volcanism" ?
            filters["volcanism_type"] = "No volcanism";
            var response = await Game.spansh.queryBodies(buildQuery(filters, "volcanism_type", SortOrder.asc));
            var countNoVolcanism = response["count"]!.Value<int>();

            // any hits with anything but "No volcanism" ?
            filters["volcanism_type"] = string.Join(',', allVolcanisms);
            //Game.log(filters["volcanism_type"]);
            response = await Game.spansh.queryBodies(buildQuery(filters, "volcanism_type", SortOrder.asc));
            var countWithVolcanism = response["count"]!.Value<int>();
            var withSomeVolcanism = response["results"]!.ToList();

            // zero record with any volcanism
            if (countNoVolcanism > 0 && countWithVolcanism == 0)
            {
                return Clause.createIs("volcanism", "None");
            }

            // We saw some volcanism, find which specific kinds
            var matchedVolcanism = new HashSet<string>();
            var potentialVolcanism = new HashSet<string>(allVolcanisms);
            if (withSomeVolcanism.Count > 0)
            {
                var moreVolcanism = withSomeVolcanism;
                var totalCount = response["count"]!.Value<float>();
                var lastCount = totalCount;

                // extract the first ...
                string volcanism = moreVolcanism.First()["volcanism_type"]!.Value<string>()!;
                do
                {
                    // ... query again without that
                    potentialVolcanism.Remove(volcanism);
                    filters["volcanism_type"] = string.Join(',', potentialVolcanism);
                    response = await Game.spansh.queryBodies(buildQuery(filters, "volcanism_type", SortOrder.asc));
                    moreVolcanism = response["results"]!.ToList();

                    var count = response["count"]!.Value<float>();
                    var delta = lastCount - count;
                    var ratio = 100f / totalCount * delta;
                    Game.log($"{species}/{atmosType}/{bodyType} => {volcanism} => {delta} ({ratio.ToString("n1")}% of {countWithVolcanism})");
                    if (ratio > 5 && delta > 3)
                        matchedVolcanism.Add(volcanism);

                    // just say "Some" if more than .. ?
                    if (false && matchedVolcanism.Count > limitVolcanismTypes)
                    {
                        matchedVolcanism.Clear();
                        matchedVolcanism.Add("Some");
                        break;
                    }

                    // take the next first
                    lastCount = count;
                    volcanism = moreVolcanism.FirstOrDefault()?["volcanism_type"]?.Value<string>()!;

                } while (volcanism != null);
            }

            if (countNoVolcanism > 0 && matchedVolcanism.FirstOrDefault() != "Some")
            {
                // alpha sort but add None first
                var list = matchedVolcanism.Order().ToList();

                Game.log($"shouldForceVolcanismNone? countNoVolcanism:{countNoVolcanism} vs countWithVolcanism: {countWithVolcanism}, list.count:{list.Count}");
                if (countNoVolcanism > (countWithVolcanism / 10f) && list.Count > 0)
                {
                    shouldForceVolcanismNone = true;
                }
                else
                {
                    list.Insert(0, "None");
                }

                var clause = Clause.createIs("volcanism", list);
                return clause;
            }

            if (countNoVolcanism == 0 && countWithVolcanism > 0)
            {
                if (matchedVolcanism.Count > 0)
                {
                    var clause = Clause.createIs("volcanism", matchedVolcanism.Order().ToList());
                    return clause;
                }
                else
                {
                    // This happens with counts too low for ratios, etc above, but there was some volcanism still
                    var allSeenVolcanism = allVolcanisms.Where(v => !potentialVolcanism.Contains(v)).Order().ToList();
                    var clause = Clause.createIs("volcanism", allSeenVolcanism); // , "Some");
                    return clause;
                }
            }

            // otherwise, it appears volcanism is not a factor
            return null;
        }

        private static float floor(float value, int decimals)
        {
            var q = Math.Pow(10, decimals);
            var newValue = Math.Floor(value * q) / q;
            return (float)newValue;
        }

        private static float ceiling(float value, int decimals)
        {
            var q = Math.Pow(10, decimals);
            var newValue = Math.Ceiling(value * q) / q;
            return (float)newValue;
        }

        private static async Task<Clause?> buildRangeClause(string species, string atmosType, string bodyType, string property, float minLimit, float maxLimit)
        {
            // get min and max values
            var objMin = await Game.spansh.queryBodies(buildQuery(atmosType, bodyType, species, property, SortOrder.asc));
            if (objMin["count"]!.Value<int>() == 0) return null;
            var minRaw = objMin["results"]!.ToArray().First()[property]!.Value<float>();

            var objMax = await Game.spansh.queryBodies(buildQuery(atmosType, bodyType, species, property, SortOrder.desc));
            var maxRaw = objMax["results"]!.ToArray().First()[property]!.Value<float>();

            // special processing?

            if (property == "gravity")
            {
                // round to nearest 3 decimal places
                minRaw = floor(minRaw, 3);
                maxRaw = ceiling(maxRaw, 3);
            }

            if (property == "surface_pressure")
            {
                // round to nearest 4 decimal places
                minRaw = floor(minRaw, 4);
                maxRaw = ceiling(maxRaw, 4);
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
            var response = await Game.spansh.queryBodies(buildQuery(atmosType, bodyType, species, "gravity", SortOrder.asc));
            var totalCount = response["count"]!.Value<int>();
            return totalCount;
        }

        private static string getLandmarksFilterValue(string speciesType)
        {
            if (speciesType.Contains('/'))
                return speciesType;

            var parts = speciesType.Split(' ').ToList();
            speciesType = speciesType.Replace('_', ' ');
            var typeValue = parts.Count == 1 ? speciesType : $"{parts[0]}/{speciesType}";
            return typeValue;
        }

        private static string getAtmosphereFilterValue(string atmosType)
        {
            if (atmosType == "No atmosphere")
                return atmosType;
            else
                return $"Thin {atmosType}";
        }

        private static string buildQuery(string? atmosType, string? bodyType, string speciesType, string sortField, SortOrder sortOrder)
        {
            var filters = new Dictionary<string, string>()
            {
                { "landmarks", getLandmarksFilterValue(speciesType) },
            };

            if (atmosType != null)
                filters.Add("atmosphere", getAtmosphereFilterValue(atmosType));
            if (bodyType != null)
                filters.Add("subtype", bodyType);

            if (forceVolcanismNone)
                filters.Add("volcanism_type", "No volcanism");

            return buildQuery(filters, sortField, sortOrder);
        }

        private static string buildQuery(Dictionary<string, string> filters, string sortField, SortOrder sortOrder, string referenceSystem = "")
        {
            // filters ...
            var partFilters = new List<string>();

            foreach (var f in filters)
            {
                if (f.Value.Contains('/'))
                {
                    var fs = f.Value.Split(',');
                    foreach (var f2 in fs)
                    {
                        // eg: "landmarks":[{"type":"Stratum","subtype":["Stratum Tectonicas"]}]
                        var parts = f2.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var clause = $"\"{f.Key}\":[{{\"type\":\"{parts[0]}\",\"subtype\":[\"{parts[1]}\"]}}]";
                        partFilters.Add(clause);
                    }
                }
                else if (f.Value.Contains('~'))
                {
                    // eg: "distance":{"min":"0","max":"100"}
                    var values = f.Value.Split('~');
                    var clause = $"\"{f.Key}\":{{\"min\":{values.First()},\"max\":{values.Last()}}}";
                    partFilters.Add(clause);
                }
                else
                {
                    // eg: "atmosphere":{"value":["Thin Carbon dioxide"]}
                    var values = JsonConvert.SerializeObject(f.Value.Split(','));
                    var valueKey = f.Key == "landmarks" ? "type" : "value";
                    var clause = $"\"{f.Key}\":{{\"{valueKey}\":{values}}}";
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

            if (referenceSystem != "")
                referenceSystem = $",\"reference_system\":\"{referenceSystem}\"";

            // final result ...
            // eg: {"filters":{"subtype":{"value":["Rocky body"]},"atmosphere":{"value":["Thin Carbon dioxide"]},"volcanism_type":{"value":["No volcanism"]},"landmarks":[{"type":"Bacterium","subtype":["Bacterium Tela"]}]},"sort":[{"materials":[{"name":"Carbon","direction":"desc"}]}],"size":10,"page":0}
            var jsonFilters = string.Join(",", partFilters);
            var queryJson = $"{{\"filters\":{{{jsonFilters}}},\"sort\":[{jsonSorts}],\"size\":1,\"page\":0{referenceSystem}}}";
            return queryJson;
        }

        private static List<string> allVolcanisms = new List<string>()
        {
            "Carbon Dioxide Geysers", "Major Carbon Dioxide Geysers", "Major Metallic Magma", "Major Rocky Magma", "Major Silicate Vapour Geysers", "Major Water Geysers", "Major Water Magma", "Metallic Magma", "Minor Ammonia Magma", "Minor Carbon Dioxide Geysers", "Minor Metallic Magma", "Minor Methane Magma", "Minor Nitrogen Magma", "Minor Rocky Magma", "Minor Silicate Vapour Geysers", "Minor Water Geysers", "Minor Water Magma", "Rocky Magma", "Silicate Vapour Geysers", "Water Geysers", "Water Magma"
        };

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
            { "Metal-rich body", "MRB" },
        };

        public static void countNebularSystems()
        {
            var start = DateTime.Now;
            countNebularSystemsAsync().ContinueWith(task =>
            {
                Game.log($"CriteriaBuilder.buildWholeSet => {task.Status}");
                if (task.Exception != null) Game.log(task.Exception);
                Game.log($"** Done ** {DateTime.Now.Subtract(start)}");
            });
        }

        private static async Task countNebularSystemsAsync()
        {
            // get relevant nebula
            var csv = File.ReadAllText(@"d:\code\nebulae-coordinates.csv");
            var nebula = csv.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                //.Take(50) // tmp!!
                .Select(line => line.Split(','))
                .ToDictionary(_ => _[1], _ => _[5]); // planetary, procgen, real

            var filters = new Dictionary<string, string>()
            {
                { "distance", $"0~100" },
            };
            Game.log($"Nebula count: {nebula.Count}");

            var txt = new StringBuilder();
            var n = 0;
            var skipping = true;
            foreach (var neb in nebula.AsParallel()) // .SkipWhile(_ => skipping = _.Key != "Dryi Auscs FV-Y e1478"))
            {
                var name = neb.Key;
                if (name[0] == '"') name = name.Substring(1, name.Length - 2);
                Game.log($"#{++n} of {nebula.Count}: {name} ({neb.Value})");
                if (skipping) skipping = !name.Equals("Scaulia zu-y e6560", StringComparison.OrdinalIgnoreCase);
                if (skipping) continue;
                if (neb.Value != "planetary") continue;

                try
                {
                    var q = buildQuery(filters, "distance", SortOrder.desc, name);
                    var response = await Game.spansh.querySystems(q);
                    var countSystems = response["count"]!.Value<int>();
                    var maxDistance = response["results"]!.First()["distance"]!.Value<double>();

                    txt.AppendLine($"{name}, {neb.Value}, {countSystems}, {maxDistance}\r\n");
                    File.AppendAllText(@"d:\code\nebulae-counts.csv", $"{name}, {neb.Value}, {countSystems}, {maxDistance}\r\n");
                }
                catch (Exception ex)
                {
                    //Game.log($"{neb.Key} => {ex.Message}");
                    Game.log(name + " => " + ex.Message);
                    Debugger.Break();
                }
            }
            // {"filters":{"distance":{"min":"0","max":"100"}},"sort":[],"size":10,"page":0,"reference_system":"Oumbaiqs WE-R d4-1"}:

            Game.log("----");
            Game.log("Nebulae results:\r\n\r\n" + txt + "\r\n");
            Game.log("----");
        }

        public static string buildMissingVariantsForSpecies(StarPos starPos, string genus, string species, List<string> variantColors)
        {
            var filters = new List<string>();
            // TODO: limit distance?
            //filters.Add($"\"distance\": {{ \"min\": \"0\", \"max\": \"1000\" }}");

            // filter: landmarks
            var joinedVariants = string.Join(',', variantColors.Select(v => $"\"{Util.pascal(v)}\""));
            genus = Util.pascal(genus);
            species = Util.pascalAllWords(species);
            filters.Add($"\"landmarks\": [ {{ \"type\": \"{genus}\", \"subtype\": [ \"{species}\" ], \"variant\": [ {joinedVariants} ] }} ]");

            var jsonSorts = "{ \"distance\": { \"direction\": \"asc\" } }";
            var refCoords = $"\"reference_coords\": {{ \"x\": {starPos.x}, \"y\": {starPos.y}, \"z\": {starPos.z} }}";

            // join final json ...
            var jsonFilters = string.Join(",", filters);
            var queryJson = $"{{\"filters\":{{{jsonFilters}}},\"sort\":[{jsonSorts}],\"size\":10,\"page\":0,{refCoords}}}";
            return queryJson;
        }
    }

    internal class ApiSystemDump
    {
        public System system;

        internal class System
        {
            public string allegiance;
            public int bodyCount;
            public List<Body> bodies;
            public StarPos coords;
            public DateTimeOffset date;
            public string government;
            public long id64;
            public string name;
            public string primaryEconomy;
            public string secondaryEconomy;
            public string security;
            public List<Station> stations;
            public List<MinorFactionPresence> factions;

            public override string ToString()
            {
                return $"{name} ({id64})";
            }

            public class Body
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

                public Signals? signals;

                // TODO? parents[]

                public List<Ring>? rings;
                public List<Station> stations;

                public override string ToString()
                {
                    return $"{name} ({bodyId})";
                }

                internal class Ring
                {
                    public string name;
                    public string type;
                    public double mass;
                    public double innerRadius;
                    public double outerRadius;
                    public Signals? signals;
                }

                internal class Signals
                {
                    public List<string>? genuses;
                    public Dictionary<string, int>? signals;
                    public DateTimeOffset updateTime;
                }
            }

            public class Station
            {
                public string controllingFaction;
                public string controllingFactionState;
                public double distanceToArrival;
                public Dictionary<string, float>? economies;
                public string government;
                public long id;
                public LandingPads landingPads;
                public Market market;
                public string name;
                public string primaryEconomy;
                public List<string> services;
                public Shipyard shipyard;
                public Outfitting outfitting;
                public string type;
                public DateTimeOffset updateTime;

                public override string ToString()
                {
                    return $"{name} ({id})";
                }

                public class Market
                {
                    public List<Commodity> commodities;
                    public List<string> prohibitedCommodities;
                    public DateTime updateTime;

                    public class Commodity
                    {
                        public int buyPrice;
                        public string category;
                        public long commodityId;
                        public int demand;
                        public string name;
                        public int sellPrice;
                        public int supply;
                        public string symbol;
                    }
                }

                public class Outfitting
                {
                    public List<Module> modules;
                    public DateTime updateTime;

                    public class Module
                    {
                        public string category;
                        public int @class;
                        public long moduleId;
                        public string name;
                        public string rating;
                        public string? ship;
                        public string symbol;
                    }
                }

                public class Shipyard
                {
                    public List<Ship> ships;
                    public DateTime updateTime;

                    public class Ship
                    {
                        public string name;
                        public long shipId;
                        public string symbol;
                    }
                }
            }

            /// <summary> Get stations from all bodies into a single list </summary>
            public List<ApiSystemDump.System.Station> getAllStations()
            {
                // get stations from all bodies into a single list
                var allStations = new List<ApiSystemDump.System.Station>(this.stations);
                this.bodies.ForEach(b =>
                {
                    if (b.stations.Count > 0)
                        allStations.AddRange(b.stations);
                });

                return allStations;
            }
        }
    }

    class SystemsSearchResults
    {
        public int count;
        public int from;
        public int size;
        public List<Result> results;

        public class Result
        {
            public string name;
            public double x;
            public double y;
            public double z;
            public long population;
            public List<MinorFactionPresence> minor_faction_presences;
        }
    }

    class MinorFactionSystem
    {
        public string name;
        public Coords coords;
        public string infos;
        public List<int> cat;

        internal class Coords
        {
            public double x;
            public double y;
            public double z;
        }
    }

    class MinorFactionPresence
    {
        public float influence;
        public string name;
        public string state;
        public string? government;
        public string? allegiance;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortOrder
    {
        asc,
        desc,
    }

    internal class ApiSystem
    {
        public Record record;

        internal class Record
        {
            public string allegiance;
            public int body_count;
            public List<Body> bodies;
            public double x;
            public double y;
            public double z;
            public DateTimeOffset? updated_at;
            public string government;
            public long id64;
            public string name;
            public string primary_economy;
            public string secondary_economy;
            public string security;
            public List<Station>? stations;
            public List<MinorFactionPresence>? minor_faction_presences;

            public class Body
            {
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public double distance_to_arrival;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public long estimated_mapping_value;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public long estimated_scan_value;
                public long id;
                public long id64;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public bool is_main_star;
                public string name;
                public string subtype;
                public string terraforming_state;
                public string type;
            }

            internal class Station
            {
                public double distance_to_arrival;
                public bool has_large_pads;
                public bool has_market;
                public bool has_outfitting;
                public bool has_shipyard;
                public int large_pads;
                public string market_id;
                public int medium_pads;
                public string name;
                public int small_pads;
                public string type;
            }
        }
    }

    internal class SearchApiResults
    {
        public int count;
        public int from;
        public List<Body> results;
        // public XXX search; TODO: describe the initial search?
        public string search_reference;
        public int size;

        public class Body
        {
            public double distance; // LY from reference point
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public double distance_to_arrival;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public long estimated_mapping_value;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public long estimated_scan_value;
            public string id;
            public long id64;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public bool is_main_star;
            public string name;
            public string subtype;
            public string terraforming_state;
            public string type;
            public List<Signal>? signals;
            public List<Landmark>? landmarks;

            // TODO: any other fields?

            public long system_id64;
            public string system_name;
            public string system_region;
            public double system_x;
            public double system_y;
            public double system_z;

            public StarPos toStarPos()
            {
                return new StarPos(system_x, system_y, system_z, system_name);
            }

            public class Signal
            {
                public string name;
                public int count;
            }

            public class Landmark
            {
                public int id;
                public double latitude;
                public double longitude;
                public string subtype;
                public string type;
                public long value;
                public string variant;
            }
        }
    }
}
