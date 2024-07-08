using Newtonsoft.Json;
using SrvSurvey.game;
using System.Text;

namespace BioCriteria
{
    internal class Predictor
    {
        public static List<string> predict(SystemBody body)
        {
            if (body.type != SystemBodyType.LandableBody) return new List<string>();
            if (Criteria.allCriteria.Count == 0) Criteria.readCriteria();

            //logOrganism = "Digitos";

            var knownGenus = body.organisms?.Select(o => o.genusLocalized).ToList();

            var parentStar = body.system.getParentStarTypes(body, true).First();
            var brightestParentStar = body.system.getBrightestParentStarType(body);

            // prepare members, converting to suitable units
            var bodyProps = new Dictionary<string, object>
            {
                { "PlanetClass", body.planetClass! },
                { "SurfaceGravity", body.surfaceGravity / 10f },
                { "SurfaceTemperature", body.surfaceTemperature },
                { "SurfacePressure", body.surfacePressure / 100_000f },
                { "Atmosphere", body.atmosphere.Replace(" atmosphere", "") },
                { "AtmosphereType", body.atmosphereType },
                { "AtmosphereComposition", body.atmosphereComposition },
                { "DistanceFromArrivalLS", body.distanceFromArrivalLS },
                { "Volcanism", string.IsNullOrEmpty(body.volcanism) ? "None" : body.volcanism },
                { "Materials", body.materials },
                { "Region", GalacticRegions.currentIdx.ToString() },
                // Take the first parent star(s) AND the "relative hottest" from the parent chain
                { "Star", new List<string>() { parentStar, brightestParentStar }  }
            };
            var predictor = new Predictor(body.name, bodyProps, knownGenus);

            // log extra diagnostics?
            //logBody = "Renibus";
            //logOrganism = "Renibus";

            // predict each criteria recusrively from the master list
            foreach (var criteria in Criteria.allCriteria)
                predictor.predict(criteria, null, null, null, null);

            return predictor.predictions.ToList();
        }

        public readonly string bodyName;
        public readonly Dictionary<string, object> bodyProps;
        public readonly List<string>? knownGenus;
        private HashSet<string> predictions = new HashSet<string>();

        /// <summary> Trace extra diagnostics for a given body </summary>
        public static string? logBody;
        /// <summary> Trace extra diagnostics for a genus, species or variant </summary>
        public static string? logOrganism;

        private Predictor(string bodyName, Dictionary<string, object>? bodyProps, List<string>? knownGenus)
        {
            this.bodyName = bodyName;
            this.knownGenus = knownGenus;
            this.bodyProps = bodyProps ?? new Dictionary<string, object>();
        }

        public void predict(Criteria criteria, string? genus, string? species, string? variant, List<Criteria>? commonChildren)
        {
            // accumulate values from current node or prior stack frames
            commonChildren = criteria.commonChildren ?? commonChildren;
            genus = criteria.genus ?? genus;
            species = criteria.species ?? species;
            variant = criteria.variant ?? variant;

            // stop here if genus names are known and this criteria isn't one of them
            if (genus != null && knownGenus?.Count > 0 && !knownGenus.Contains(genus)) return;

            // evaluate current query
            var currentName = $"{genus} {species} - {variant}";
            var failures = testQuery(criteria.query, $"{genus} {species} {variant}".Trim());

            // add a prediction if no failures and we have genus, species AND variant
            if (failures.Count == 0 && genus != null && species != null && variant != null)
                this.predictions.Add(currentName);

            // continue into children
            var children = criteria.useCommonChildren ? commonChildren : criteria.children;
            if (children?.Count > 0 && failures.Count == 0)
            {
                foreach (var child in children)
                    predict(child, genus, species, variant, commonChildren);
            }
        }

        private List<ClauseFailure> testQuery(List<Clause>? query, string currentName)
        {
            var failures = new List<ClauseFailure>();

            if (query?.Count > 0)
            {
                // test all clauses in the query
                foreach (var clause in query)
                {
                    if (clause == null) continue;

                    var propName = Map.properties.GetValueOrDefault(clause.property) ?? clause.property;
                    if (!bodyProps.ContainsKey(propName)) throw new Exception($"Unexpected property: {propName} ({clause.property})");

                    var bodyValue = bodyProps.GetValueOrDefault(propName);

                    // match some string(s)
                    if (clause.op == Op.Is)
                    {
                        if (clause.values == null) throw new Exception("Missing clause values?");

                        // match a single string
                        if (bodyValue is string)
                        {
                            var bodyValueString = (string)bodyValue;
                            if (clause.property == "body")
                            {
                                // special case for 'body' clauses to use `StartsWith` not `Equals`
                                if (!clause.values.Any(cv => bodyValueString.StartsWith(cv, StringComparison.OrdinalIgnoreCase)))
                                    failures.Add(new ClauseFailure(bodyName, "no match found", clause, bodyValueString));
                            }
                            else if (!clause.values.Any(cv => bodyValueString.Equals(cv, StringComparison.OrdinalIgnoreCase)))
                            {
                                failures.Add(new ClauseFailure(bodyName, "no match found", clause, bodyValueString));
                            }

                            continue;
                        }

                        // match any value from a set of strings
                        var bodyValues = bodyValue as List<string>;
                        if (bodyValue is Dictionary<string, float>)
                            bodyValues = ((Dictionary<string, float>)bodyValue).Keys.ToList();

                        if (bodyValues != null && !clause.values.Any(v => bodyValues.Any(bv => bv.Equals(v, StringComparison.OrdinalIgnoreCase))))
                            failures.Add(new ClauseFailure(bodyName, "no match found", clause, string.Join(',', bodyValues)));

                        continue;
                    }

                    // match min/max
                    if (clause.op == Op.Range)
                    {
                        if (bodyValue is double)
                        {
                            var doubleValue = (double)bodyValue;
                            if (clause.min != null && doubleValue < clause.min)
                            {
                                failures.Add(new ClauseFailure(bodyName, "below min", clause, doubleValue.ToString("n6")));
                                continue;
                            }
                            if (clause.max != null && doubleValue > clause.max)
                            {
                                failures.Add(new ClauseFailure(bodyName, "above max", clause, doubleValue.ToString("n6")));
                                continue;
                            }
                        }

                        continue;
                    }

                    // match compositions - ONLY greater-than-or-equals - other operands not supported (and may not be needed)
                    if (clause.op == Op.Composition)
                    {
                        var bodyCompositions = bodyValue as Dictionary<string, float>;
                        if (bodyCompositions != null)
                        {
                            foreach (var clauseComposition in clause.compositions!)
                            {
                                if (!bodyCompositions.ContainsKey(clauseComposition.Key))
                                {
                                    failures.Add(new ClauseFailure(bodyName, $"'{clauseComposition.Key}' not present", clause, JsonConvert.SerializeObject(bodyCompositions)));
                                }
                                else if (bodyCompositions[clauseComposition.Key] < clauseComposition.Value)
                                {
                                    failures.Add(new ClauseFailure(bodyName, $"'{clauseComposition.Key}' too low", clause, JsonConvert.SerializeObject(bodyCompositions)));
                                }
                            }
                        }
                    }
                }
            }

            // trace extra diagnostics?
            if (failures.Count > 0)
            {
                if (logBody == "*" || (!string.IsNullOrWhiteSpace(logBody) && this.bodyName.Contains(logBody, StringComparison.OrdinalIgnoreCase)))
                    Game.log($"Prediction failures for body: {bodyName} / {currentName}\r\n > " + string.Join("\r\n > ", failures));

                if (logOrganism == "*" || (!string.IsNullOrWhiteSpace(logOrganism) && currentName.Contains(logOrganism, StringComparison.OrdinalIgnoreCase)))
                {
                    var queryTxt = "\t" + string.Join("\r\n\t", query);
                    Game.log($"Prediction failures for organism: {logOrganism} / {currentName}\r\n{queryTxt}\r\n > " + string.Join("\r\n > ", failures));
                }
            }

            return failures;
        }

        public static async Task testSystem(string systemName, string galacticRegion)
        {
            var cmdr = "none";
            var fid = "F101";

            // force current region to be the one for this test
            GalacticRegions.currentIdxOverride = GalacticRegions.getIdxFromDisplayName(galacticRegion);

            // load system data from EDSM
            var system = await Game.edsm.getSystems(systemName);
            var starPos = system.First().coords.starPos;

            var bodies = await Game.edsm.getBodies(systemName);
            var systemData = SystemData.From(bodies, starPos, fid, cmdr);

            var bioStats = await Game.canonn.systemBioStats(systemData.address);


            // predict this system
            foreach (var body in systemData.bodies)
            {
                Predictor.logBody = "";
                Predictor.logOrganism = "";
                var predictions = Predictor.predict(body);

                if (predictions.Count > 0)
                {
                    var realBody = bioStats.bodies.Find(b => b.bodyId == body.id);
                    if (realBody?.signals?.biology != null)
                    {
                        var countSuccess = predictions.Count(p => realBody.signals.biology.Contains(p));
                        var missed = realBody.signals.biology.Where(b => !predictions.Contains(b)).ToList();
                        var countWrong = predictions.Count - countSuccess;
                        var txt = $"\r\n** '{body.name}' ** count: {realBody.signals.biology.Count}, success: {countSuccess}, missed: {missed.Count}, wrong: {countWrong}\r\nREAL:\r\n\t" + string.Join("\r\n\t", realBody.signals.biology) + $"\r\nPREDICTED:\r\n\t" + string.Join("\r\n\t", predictions) + "\r\n";
                        Game.log(txt);
                        if (missed.Count > 0)
                        {
                            Game.log($"Missed: {body.system.address} / '{body.name}' - {body.id}\r\n\t" + string.Join("\r\n\t", missed));
                        }
                    }
                    else
                    {
                        Game.log($"\r\n** '{body.name}' predicted: **\r\n\t" + string.Join("\r\n\t", predictions) + "\r\n");
                    }

                }
            }

            Game.log($"Done: {systemName}");
        }

        public static async Task testSystems()
        {
            var testSystems = new Dictionary<string, string>()
            {
                // top 20 systems
                //// ? { "Col 285 Sector BS-I c10-11", "Inner Orion Spur" },
                //{ "Kyloagh PE-G d11-86", "Orion-Cygnus Arm" },
                //{ "Eol Prou QS-T d3-483", "Inner Scutum-Centaurus Arm" },
                //{ "Synuefai MW-U c19-4", "Inner Orion Spur" },
                ////// ? { "HIP 82068", "Inner Orion Spur" },
                //{ "76 Leonis", "Inner Orion Spur" },
                //{ "Hypio Flyao XP-P e5-54", "Arcadian Stream" },

                // ? { "HIP 56843", "Inner Orion Spur" },

                //{ "HD 221180", "Inner Orion Spur" },
                //{ "Phaa Audst GW-W e1-844", "Odin's Hold" },
                //{ "Pru Aim GR-D d12-2676", "Inner Scutum-Centaurus Arm" },
                //{ "Phua Aub WU-X e1-3527", "Galactic Centre" },
                //{ "Scheau Bluae JC-B d1-13", "Inner Orion-Perseus Conflux" },
                //{ "Synuefue ZX-F d12-49", "Inner Orion Spur" },
                //{ "Dryio Flyuae IY-Q d5-789", "Inner Scutum-Centaurus Arm" },
                //{ "Skaude GD-Q d6-29", "Inner Scutum-Centaurus Arm" },
                //{ "Flyooe Hypue FT-O d7-23", "Inner Orion Spur" },
                //{ "Byoi Aip VE-R d4-58", "Norma Arm" },
                //{ "Clooku HI-R d5-410", "Inner Scutum-Centaurus Arm" },
                //{ "Dumbio GN-B d13-5111", "Odin's Hold" },

                // more from me
                //{ "Graea Hypue AA-Z d33", "Norma Expanse" },
                //{ "Swoiwns OE-O d7-48", "Inner Orion Spur" },
                //{ "Wregoe JA-Z d9", "Inner Orion Spur" },
                //{ "Nyeajeou VP-G b56-0", "Temple" },
                //{ "HIP 17694", "Inner Orion Spur" },
                //{ "BD+47 2267", "Inner Orion Spur" },
                //{ "2MASS J05334575-0441245", "Sanguineous Rim" },
                //{ "Graea Hypue AA-Z d85", "Norma Expanse" },
                //{ "Bleethuae LN-B d1172", "Izanami" },
                //{ "HIP 76045", "Inner Orion Spur" },
                ////{ "HIP 97950", "Inner Orion Spur" }, // This one is odd.
                //{ "GD 140", "Inner Orion Spur" },
                //{ "HR 5716", "Inner Orion Spur" },

                //// from Cupco
                //// { "Swoilz NJ-O d7-10", "Inner Orion Spur" },

                // top 20 bodies
                //{ "Aucoks RX-S d4-6", "Inner Orion Spur" },
                //{ "Hyuedau LV-Y d34", "Achilles's Altar" }, // why star - double N,N ??
                //{ "Drojau BG-W d2-1", "Inner Orion Spur" },
                //{ "Athaiwyg EG-Y c8", "Arcadian Stream" },
                //{ "Flyooe Eohn CS-H b43-0", "Sanguineous Rim" },
                //{ "Lyncis Sector CL-Y c14", "Inner Orion Spur" },
                //{ "Blaa Drye WC-F b58-5", "Temple" },
                { "Blau Eur RZ-O c19-41", "Hawking's Gap" }, // revisit!
                { "Slegeae SU-R b24-0", "Sanguineous Rim" },
                { "Outotz ZQ-K b22-0", "Sanguineous Rim" },
                { "Hegou FB-S c6-0", "Sanguineous Rim" },
                { "Oochoss NM-K b42-1", "Elysian Shore" },
                { "Byoomao CG-D a108-65", "Galactic Centre" },
                { "Groem BL-E c25-0", "Kepler's Crest" },
                { "Wruetheia NL-U b46-0", "Formorian Frontier" },
                { "Blie Eup RQ-O b47-2", "Elysian Shore" },
                { "Dryaa Bloae II-N b54-0", "Outer Arm" },
                { "Nyeakeia ZA-V b33-0", "Hawking's Gap" },
                { "Hegoo FW-E d11-18", "Sanguineous Rim" },
                { "Choomee IF-R c4-7189", "Empyrean Straits" },

                //// from Cmdr Stromb
                ////{ "Hip 48603" , "Inner Orion Spur" },
                ////{ "Spaidau gb-f d11-1" , "Kepler's Crest" },
                ////{ "Ngc 5662 Sector fb-x d1-32" , "Inner Orion Spur" },
                //{ "Pro Eurl ak-a d62", "Inner Orion Spur" },
                ////{ "Wregoe fs-z c27-17", "Inner Orion Spur" },
                ////{ "Byaa Theia xv-e d11-43", "Inner Orion Spur" },
                
                //// these are good
                //{ "Prua Phoe JR-U d3-91", "Inner Scutum-Centaurus Arm" },
                //{ "Prua Phoe LX-S d4-211", "Inner Scutum-Centaurus Arm" },
                //{ "Prua Phoe IR-U d3-178", "Inner Scutum-Centaurus Arm" },
                //{ "Prua Phoe UO-N c8-22", "Inner Scutum-Centaurus Arm" },
                //{ "Prua Phoe PD-R d5-185", "Inner Scutum-Centaurus Arm" },
                //{ "Prua Phoe WI-B d8", "Inner Scutum-Centaurus Arm" },
                //{ "Graea Hypue AA-Z d70", "Norma Expanse" },
                //{ "Graea Hypue AA-Z d67", "Norma Expanse" },
                //{ "Graea Hypue AA-Z d58", "Norma Expanse" },
                //{ "Graea Hypue AA-Z d50", "Norma Expanse" },
                //{ "Graea Hypue AA-Z d48", "Norma Expanse" },
                //{ "Graea Hypue AA-Z d39", "Norma Expanse" },
            };

            var allResults = new StringBuilder();
            foreach (var pair in testSystems)
            {
                await testSystem(pair.Key, pair.Value);
            }

            File.WriteAllText(@"d:\bioBulkResults100.csv", allResults.ToString());
            Game.log("All done: everything!");
        }

    }

    internal class ClauseFailure
    {
        public Clause clause;
        public string bodyName;
        public string bodyValue;
        public string reason;

        public ClauseFailure(string bodyName, string reason, Clause clause, string bodyValue)
        {
            this.bodyName = bodyName;
            this.reason = reason;
            this.clause = clause;
            this.bodyValue = bodyValue;
        }

        public override string ToString()
        {
            return $"{bodyName}: {reason} from '{clause}' in: {bodyValue}";
        }
    }
}
