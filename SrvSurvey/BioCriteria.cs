using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey;
using SrvSurvey.game;
using SrvSurvey.net;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BioCriterias
{
    [JsonConverter(typeof(BioCriteria.JsonConverter))]
    class BioCriteria
    {
        public string? genus;
        public string? species;
        public string? variant;
        public List<Clause> query;

        public List<BioCriteria> children;

        public bool useCommonChildren;
        public List<BioCriteria>? commonChildren;

        #region static loading code

        private static string devSrcFolder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..", "bio-criteria");

        public readonly static List<BioCriteria> allCriteria = new List<BioCriteria>();

        public override string ToString()
        {
            return $"{genus}{species}{variant} queries:{query?.Count}";
        }

        public static void readCriteria()
        {
            Game.log("readCriteria");
            allCriteria.Clear();

            // use source files if debugging, otherwise published files
            var folder = Debugger.IsAttached ? devSrcFolder : Git.pubBioCriteriaFolder;
            var files = Directory.GetFiles(folder, "*.json");

            foreach (var filepath in files)
            {
                var json = File.ReadAllText(filepath);
                try
                {
                    var criteria = JsonConvert.DeserializeObject<BioCriteria>(json)!;
                    allCriteria.Add(criteria);
                }
                catch (Exception ex)
                {
                    // One of the .json files failed to parse?
                    Game.log($"Bad json in '{filepath}': {ex}");
                    Debugger.Break();
                    FormErrorSubmit.Show(ex);
                }
            }

            // post process all criteria, confirming no node has children and useCommonChildren
            foreach (var criteria in allCriteria)
                checkChildren(criteria);
        }

        private static void checkChildren(BioCriteria criteria)
        {
            if (criteria.children?.Count > 0)
            {
                // confirm we do not have useCommonChildren AND children
                if (criteria.useCommonChildren)
                    throw new Exception($"Bad criteria has both 'children' and 'useCommonChildren': " + JsonConvert.SerializeObject(criteria));

                foreach (var child in criteria.children)
                    checkChildren(child);
            }
        }

        #endregion
        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JToken>(reader);
                if (obj == null || !obj.HasValues) return null;

                var query = obj["query"]?.ToObject<List<Clause>>();
                var children = obj["children"]?.ToObject<List<BioCriteria>>();
                var commonChildren = obj["commonChildren"]?.ToObject<List<BioCriteria>>();

                var citeria = new BioCriteria
                {
                    genus = obj["genus"]?.Value<string>(),
                    species = obj["species"]?.Value<string>(),
                    variant = obj["variant"]?.Value<string>(),
                    useCommonChildren = obj["useCommonChildren"]?.Value<bool>() ?? false,
                    query = query!,
                    children = children!,
                    commonChildren = commonChildren,
                };

                return citeria;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as BioCriteria;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                var obj = new JObject();
                if (data.genus != null) obj["genus"] = data.genus;
                if (data.species != null) obj["species"] = data.species;
                if (data.variant != null) obj["variant"] = data.variant;
                if (data.useCommonChildren) obj["useCommonChildren"] = data.useCommonChildren;
                if (data.query?.Count > 0)
                {
                    // pad property names to match the longest one
                    var maxKeyLength = data.query.Max(q => q.op == Op.Comment ? 0 : q.property.Length);
                    var paddedQueries = data.query.Select(q => q.op == Op.Comment ? q.ToString() : q.ToString().Replace(q.property, q.property.PadLeft(maxKeyLength)));
                    obj["query"] = JArray.FromObject(paddedQueries);
                }
                if (data.children?.Count > 0) obj["children"] = JArray.FromObject(data.children);

                obj.WriteTo(writer);
            }
        }

    }

    [JsonConverter(typeof(Clause.JsonConverter))]
    class Clause
    {
        private string raw;
        public string property;
        public Op op;
        public List<string>? values;
        public float? min;
        public float? max;
        public Dictionary<string, float>? compositions;

        public override string ToString()
        {
            return raw;
        }

        public static Clause createRange(string property, float? min, float? max)
        {
            var clause = new Clause()
            {
                raw = $"{property} [{min} ~ {max}]",
                property = property,
                op = Op.Range,
                min = min,
                max = max,
            };
            return clause;
        }

        public static Clause createComment(string comment)
        {
            var clause = new Clause()
            {
                raw = $"# {comment}",
                property = "",
                op = Op.Comment,
            };
            return clause;
        }

        public static Clause createIs(string property, string value)
        {
            return createIs(property, new List<string>() { value });
        }

        public static Clause createIs(string property, List<string> values)
        {
            //var mappedValues = values.Select(v => Map.values.ContainsKey(v) ? Map.values[v] : v);

            var mappedValues = values.Select(v => Map.values.ContainsValue(v) ? Map.values.First(p => p.Value == v).Key : v);

            var clause = new Clause()
            {
                raw = $"{property} [{string.Join(',', mappedValues)}]",
                property = property,
                op = Op.Is,
                values = values,
            };
            return clause;
        }

        public static Clause createCompositions(string property, Dictionary<string, float> compositions)
        {
            var clause = new Clause()
            {
                raw = $"{property} [{string.Join(" | ", compositions.Select(c => $"{c.Key} >= {c.Value}"))}]",
                property = property,
                op = Op.Composition,
                compositions = compositions,
            };
            return clause;
        }

        public static Clause parse(string txt)
        {
            // "<property name> [<condition values>]"
            // eg: "temp [146 ~ 196]"
            // eg: "gravity [ ~ 0.28]"
            // eg: "pressure [0.01 ~ ]"
            // eg: "atmosphere [Thin Ammonia]"
            // eg: "atmosType [CarbonDioxide,SulphurDioxide,Water]"
            // eg: "atmosComp [SulphurDioxide > 0.99]"

            var r0 = new Regex(@"\s*(\w+)\s*!?\[(.+)\]");
            var m0 = r0.Match(txt);

            var property = m0.Groups[1].Value;
            var valTxt = m0.Groups[2].Value;

            var clause = new Clause()
            {
                raw = txt,
                property = property,
            };

            if (valTxt.Contains('~'))
            {
                // numeric ranges
                clause.op = Op.Range;
                var parts = valTxt.Split('~', StringSplitOptions.TrimEntries);
                if (!string.IsNullOrEmpty(parts[0]))
                    clause.min = float.Parse(parts[0], CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(parts[1]))
                    clause.max = float.Parse(parts[1], CultureInfo.InvariantCulture);
            }
            else if (valTxt.Contains(">="))
            {
                // compositions
                clause.op = Op.Composition;
                clause.compositions = new Dictionary<string, float>();
                var regCompo = new Regex(@"([\w\s]+)(>=)\s*([\.\d]+)");

                var parts = valTxt.Split('|', StringSplitOptions.TrimEntries);
                foreach (var part in parts)
                {
                    var matches = regCompo.Match(part);
                    if (matches.Groups.Count < 4) throw new Exception($"Bad Composition clause: {part}");
                    if (matches.Groups[2].Value != ">=") throw new Exception($"Unsupported Composition operand: {matches.Groups[2].Value}");
                    var thing = matches.Groups[1].Value.Trim();
                    var amount = float.Parse(matches.Groups[3].Value, CultureInfo.InvariantCulture);
                    // TODO: do we need anything other than >= ?
                    clause.compositions.Add(thing, amount);
                }
            }
            else
            {
                // sets of strings
                clause.op = txt.Contains("![") ? Op.Not : Op.Is;
                clause.values = valTxt.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => Map.values.ContainsKey(v) ? Map.values[v]! : v)
                    .ToList();
            }

            if (clause.values == null && clause.min == null && clause.max == null && clause.compositions == null) throw new Exception($"Bad criteria: {txt}");
            return clause;
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader)?.Trim();
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");
                if (txt.StartsWith("#")) return null;

                return parse(txt);
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as Clause;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // write a single string
                string txt;
                if (data.op == Op.Is)
                    txt = $"{data.property} [{string.Join(',', data.values!)}]";
                else if (data.op == Op.Not)
                    txt = $"{data.property} ![{string.Join(',', data.values!)}]";
                else if (data.op == Op.Composition)
                    txt = $"{data.property} [{string.Join(" | ", data.compositions!.Select(p => $"{p.Key} => {p.Value}"))}]";
                else
                    txt = $"{data.property} [{data.min} ~ {data.max}]";

                writer.WriteValue(txt);
            }
        }
    }

    enum Op
    {
        /// <summary>
        /// String property value is one of the values
        /// </summary>
        Is,
        /// <summary>
        /// String property value is NOT one of the values
        /// </summary>
        Not,
        /// <summary>
        /// Numeric property value is between min and/or max values
        /// </summary>
        Range,
        /// <summary>
        /// Compositions
        /// </summary>
        Composition,
        /// <summary>
        /// A comment to be ignored
        /// </summary>
        Comment,
    }

    class Map
    {
        /// <summary>
        /// A mapping of query property names to the members of Scan journal entries
        /// </summary>
        public static Dictionary<string, string> properties = new Dictionary<string, string>()
        {
            { "body", "PlanetClass" },
            { "gravity", "SurfaceGravity" },
            { "temp", "SurfaceTemperature" },
            { "pressure", "SurfacePressure" },

            { "atmosphere", "Atmosphere" },
            { "atmosType", "AtmosphereType" },
            { "atmosComp", "AtmosphereComposition" },

            { "dist", "DistanceFromArrivalLS" },

            { "volcanism", "Volcanism" },
            { "mats", "Materials" },

            // Property values are an array of numbers matching galactic regions or NOT matching, eg: [1, 2, 3] or ![1, 2, 3]
            { "regions", "Region" },

            // Some parent star(s)
            { "star", "Star" },

            // The immediate parent star(s) for the body
            { "parentStar", "ParentStar" },

            // The primary star for the system
            { "primaryStar", "PrimaryStar" },

            // Property value is distance from a known nebula in LY, eg: [~ 100]
            { "nebulae", "Nebulae" },

            // Property value is not needed. The condition is if we're within a known Guardian bubble., eg: []
            { "guardian", "Guardian" },
        };

        public static Dictionary<string, string> values = new Dictionary<string, string>()
        {
            { "Icy", "Icy body" },
            { "Rocky", "Rocky body" },
            { "RockyIce", "Rocky ice " }, // might be "... body" or "... world"
            { "HMC", "High metal content " }, // might be "... body" or "... world"
        };
    }
}


