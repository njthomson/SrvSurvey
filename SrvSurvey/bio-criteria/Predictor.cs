using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioCriteria
{
    internal class Predictor
    {
        #region static loading code

        private static string rootFolder = @"D:\code\SrvSurvey\SrvSurvey\bio-criteria\";

        private static List<Criteria> allCriteria = new List<Criteria>();

        public static void readCriteria()
        {
            Game.log("readCriteria");
            var files = Directory.GetFiles(rootFolder, "*.json");

            foreach (var filepath in files)
            {
                var json = File.ReadAllText(filepath);

                var criteria = JsonConvert.DeserializeObject<Criteria>(json)!;
                allCriteria.Add(criteria);
            }
        }

        #endregion

        public static string? logStuff;

        public Dictionary<string, object> props = new Dictionary<string, object>();
        public string bodyName;
        public List<string>? knownGenus;
        private HashSet<string> potentials = new HashSet<string>();

        private Predictor(string bodyName, List<string>? knownGenus) 
        {
            this.bodyName = bodyName;
            this.knownGenus = knownGenus;
        }

        public static List<string> predict(SystemBody body)
        {
            if (body.planetClass == null) return new List<string>();
            if (allCriteria.Count == 0) readCriteria();

            logStuff = null; // "Renibus";

            var knownGenus = body.organisms?.Select(o => o.genusLocalized).ToList();

            // prepare members, or convert to suitable units
            var predictor = new Predictor(body.name, knownGenus);

            // prepare members, or convert to suitable units
            predictor.props.Add("PlanetClass", body.planetClass!);
            predictor.props.Add("SurfaceGravity", body.surfaceGravity / 10f);
            predictor.props.Add("SurfaceTemperature", body.surfaceTemperature);
            predictor.props.Add("SurfacePressure", body.surfacePressure / 100_000f);
            predictor.props.Add("Atmosphere", body.atmosphere.Replace(" atmosphere", ""));
            predictor.props.Add("AtmosphereType", body.atmosphereType);
            predictor.props.Add("AtmosphereComposition", body.atmosphereComposition);
            predictor.props.Add("DistanceFromArrivalLS", body.distanceFromArrivalLS);
            predictor.props.Add("Volcanism", string.IsNullOrEmpty(body.volcanism) ? "None" : body.volcanism);
            predictor.props.Add("Materials", body.materials);
            predictor.props.Add("Region", GalacticRegions.currentIdx.ToString());
            predictor.props.Add("Star", body.system.getParentStarTypes(body, true));

            foreach (var criteria in allCriteria)
            {
                predictor.predict(criteria, null, null, null);
            }

            Game.log($"** {body.name} Potentials: **\r\n\t" + string.Join("\r\n\t", predictor.potentials) + "\r\n");
            return predictor.potentials.ToList();
        }

        public void predict(Criteria criteria, string? genus, string? species, string? variant)
        {
            genus = criteria.genus ?? genus;
            species = criteria.species ?? species;
            variant = criteria.variant ?? variant;

            if (genus != null && knownGenus?.Count > 0 && !knownGenus.Contains(genus)) 
                return;

            // match criteria
            var failures = new List<string>();
            if (criteria.query?.Count > 0)
            {
                foreach (var clause in criteria.query)
                {
                    if (clause == null) continue;
                    var propName = Map.properties.GetValueOrDefault(clause.property) ?? clause.property;
                    if (!props.ContainsKey(propName)) throw new Exception($"Unexpected property: {propName} ({clause.property})");

                    var bodyValue = props.GetValueOrDefault(propName);

                    // match some string
                    if (clause.op == Op.Is)
                    {
                        if (clause.values == null) throw new Exception("Missing clause values?");

                        // match a single string
                        var bodyValueString = bodyValue as string;
                        if (bodyValueString != null && !clause.values.Any(cv => bodyValueString.Equals(cv, StringComparison.OrdinalIgnoreCase)))
                        {
                            failures.Add($"'{clause}' failed: body '{propName}' has '{bodyValue}'");
                            continue;
                        }

                        // match a set of strings
                        var bodyValues = bodyValue as List<string>;
                        if (bodyValue is Dictionary<string, float>)
                            bodyValues = ((Dictionary<string, float>)bodyValue).Keys.ToList();

                        if (bodyValues != null && !clause.values.Any(v => bodyValues.Any(bv => bv.Equals(v, StringComparison.OrdinalIgnoreCase))))
                        {
                            failures.Add($"'{clause}' failed: body '{propName}' has '{string.Join(',', bodyValues)}'");
                            continue;
                        }
                    }

                    // match min/max
                    if (clause.op == Op.Range)
                    {
                        if (bodyValue is double)
                        {
                            if (clause.min != null && (double)bodyValue < clause.min)
                            {
                                failures.Add($"'{clause}' failed: body '{propName}' has '{bodyValue}'");
                                continue;
                            }
                            if (clause.max != null && (double)bodyValue > clause.max)
                            {
                                failures.Add($"'{clause}' failed: body '{propName}' has '{bodyValue}'");
                                continue;
                            }
                        }

                    }

                    // match compositions >=
                    if (clause.op == Op.Composition)
                    {
                        var bodyCompositions = bodyValue as Dictionary<string, float>;
                        if (bodyCompositions != null)
                        {
                            foreach (var clauseComposition in clause.compositions!)
                            {
                                if (!bodyCompositions.ContainsKey(clauseComposition.Key))
                                {
                                    failures.Add($"'{clause}' failed: body '{propName}' has '{JsonConvert.SerializeObject(bodyCompositions)}'");
                                    continue;
                                }
                                else if (bodyCompositions[clauseComposition.Key] < clauseComposition.Value)
                                {
                                    failures.Add($"'{clause}' failed: body {propName} has '{JsonConvert.SerializeObject(bodyCompositions)}'");
                                    continue;
                                }
                            }
                        }
                    }
                }

                var key = $"{genus} {species} - {variant}";
                if (logStuff != null && key.Contains(logStuff, StringComparison.OrdinalIgnoreCase))
                    if (failures.Count > 0)
                        Game.log($"?> '{key}':\r\n" + string.Join("\r\n\t", failures));

                if (failures.Count == 0 && genus != null && species != null && variant != null)
                    this.potentials.Add(key);
            }

            if (criteria.children?.Count > 0 && failures.Count == 0)
                foreach (var child in criteria.children)
                    predict(child, genus, species, variant);
        }

    }
}
