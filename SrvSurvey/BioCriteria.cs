using Newtonsoft.Json;
using SrvSurvey;
using SrvSurvey.game;
using SrvSurvey.net;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BioCriteria
{
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
    }

    [JsonConverter(typeof(Clause.JsonConverter))]
    class Clause
    {
        private string raw;
        public string property;
        public Op op;
        public List<string>? values;
        public double? min;
        public double? max;
        public Dictionary<string, float>? compositions;

        public override string ToString()
        {
            return raw;
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader)?.Trim();
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");
                if (txt.StartsWith("#")) return null;

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
                        clause.min = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(parts[1]))
                        clause.max = double.Parse(parts[1], CultureInfo.InvariantCulture);
                }
                else if (valTxt.Contains('<') || valTxt.Contains('>'))
                {
                    // compositions
                    clause.op = Op.Composition;
                    clause.compositions = new Dictionary<string, float>();
                    var regCompo = new Regex(@"([\w\s]+)(>=)\s*([\.\d]+)");

                    var parts = valTxt.Split(',', StringSplitOptions.TrimEntries);
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

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as Clause;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // write a single string
                string txt;
                if (data.op == Op.Is)
                    txt = $"{data.property} [{string.Join(',', data.values!)}]";
                if (data.op == Op.Not)
                    txt = $"{data.property} ![{string.Join(',', data.values!)}]";
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


