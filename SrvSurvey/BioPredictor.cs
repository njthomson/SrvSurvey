using Newtonsoft.Json;
using SrvSurvey;
using SrvSurvey.game;
using System.Diagnostics;
using System.Text;

namespace BioCriterias
{
    internal class BioPredictor
    {
        public static float matsMinimalThreshold = 0.25f;
        public static bool useTestCache = false;
        public static string netCache = Path.Combine(Program.dataFolder, "netCache");
        public static string folderMissedReports = Path.Combine(Program.dataFolder, "missedPrediction");

        public static List<Clause> predictTarget(SystemBody body, string? targetVariant)
        {
            var predictor = predict(body, targetVariant);
            return predictor?.targetClauses ?? new();
        }

        public static List<string> predict(SystemBody body)
        {
            var predictor = predict(body, null);
            return predictor?.predictions.ToList() ?? new();
        }

        private static BioPredictor? predict(SystemBody body, string? targetVariant = null)
        {
            if (body == null || body.type != SystemBodyType.LandableBody) return null;
            if (body.parents == null || body.parents.Count == 0) return null;
            lock (BioCriteria.allCriteria)
            {
                if (BioCriteria.allCriteria.Count == 0 || Debugger.IsAttached) BioCriteria.readCriteria();
            }

            // get the primary star for the system
            var primaryStar = body.system.bodies.FirstOrDefault(b => b.isMainStar);
            if (primaryStar == null)
            {
                Game.log($"Why no primary star? For {body.name}");
                Debugger.Break();
            }
            var primaryStarType = Util.flattenStarType(primaryStar?.starType) ?? "";

            // calculate relative brightness for all parent stars
            var parentsByBrightness = body.system.getParentStars(body, false)
                .ToDictionary(s => s, s => body.getRelativeBrightness(s))
                .Where(s => s.Value > 0)
                .OrderByDescending(s => s.Value)
                .ToDictionary(_ => _.Key, _ => _.Value);
            Game.log($"Radiant stars for '{body.name}': \r\n" + string.Join("\r\n", parentsByBrightness.Select(_ => $"  > {_.Key.shortName} ({_.Key.starType}) : {_.Value}")) + "\r\n");

            if (parentsByBrightness.Count == 0)
            {
                Game.log($"Why no parent stars? For {body.name}");
                // Debugger.Break();
                return null;
            }

            // take the 1st brightest parent star
            var brightest = parentsByBrightness.First();
            var parentStarTypes = new List<string>();
            parentStarTypes.Add(Util.flattenStarType(brightest.Key.starType));

            // consider the 2nd if the type is different but the value is really close
            if (parentsByBrightness.Count > 1)
            {
                var nextBrightest = parentsByBrightness.Skip(1).First();
                var nextBrightestType = Util.flattenStarType(nextBrightest.Key.starType);
                if (parentStarTypes[0] != nextBrightestType)
                {
                    var delta = nextBrightest.Value / brightest.Value;
                    if (false && delta < 0.01d) // DO NOT allow a 2nd if they're really close
                    {
                        Game.log($"{brightest.Key.name} ({brightest.Key.starType}): {brightest.Value:N10} vs {nextBrightest.Key.name} ({nextBrightest.Key.starType}): {nextBrightest.Value:N10} => {delta}");
                        parentStarTypes.Add(Util.flattenStarType(nextBrightest.Key.starType));
                    }
                }
            }

            if (parentStarTypes.All(s => s == null))
            {
                Game.log($"Parent stars are not right?! body: {body.name}");
                Debugger.Break();
                return null;
            }

            // calc distance to nearest Guardian bubble
            var withinGuardianBubble = Game.codexRef.isWithinGuardianBubble(body.system.starPos);

            // when there is a single entry - force that atmosphereComposition to 100% 
            var atmosphereComposition = body.atmosphereComposition?.ToDictionary(x => x.Key, x => x.Value);
            if (atmosphereComposition?.Count == 1)
            {
                var key = atmosphereComposition.Keys.First();
                atmosphereComposition[key] = 100;
            }

            // prepare members, converting to suitable units
            var bodyProps = new Dictionary<string, object>
            {
                { "PlanetClass", body.planetClass! },
                { "SurfaceGravity", body.surfaceGravity / 10f },
                { "SurfaceTemperature", body.surfaceTemperature },
                { "SurfacePressure", body.surfacePressure / 100_000f },
                { "Atmosphere", body.atmosphere.Replace(" atmosphere", "") },
                { "AtmosphereType", body.atmosphereType },
                { "AtmosphereComposition", atmosphereComposition! },
                { "DistanceFromArrivalLS", (double)body.distanceFromArrivalLS },
                { "Volcanism", string.IsNullOrEmpty(body.volcanism) ? "None" : body.volcanism },
                { "Materials", body.materials },
                { "Region", GalacticRegions.currentIdx.ToString() },
                { "Star", parentStarTypes },
                { "PrimaryStar", primaryStarType },
                { "Nebulae", body.system.nebulaDist },
                { "Guardian", withinGuardianBubble.ToString() },

            };
            var predictor = new BioPredictor(body.name, bodyProps, targetVariant);

            // add known genus and species names
            if (body.organisms?.Count > 0)
            {
                foreach (var org in body.organisms.ToList())
                {
                    if (org.genus != null)
                    {
                        var match = Game.codexRef.matchFromGenus(org.genus)!;
                        predictor.knownGenus.Add(match.englishName);
                    }

                    if (org.species != null)
                    {
                        var match = Game.codexRef.matchFromSpecies2(org.species);
                        // TODO: handle Brain Tree's
                        if (match != null)
                            predictor.knownSpecies[match.genus.englishName] = match.species.englishName;
                    }
                }

                predictor.allGenusKnown = body.organisms.Count == body.bioSignalCount;
            }

            // log extra diagnostics?
            //logOrganism = "Cornibus Indigo";

            // predict each criteria recursively from the master list
            foreach (var criteria in BioCriteria.allCriteria)
                predictor.predict(criteria, null, null, null, null);

            return predictor;
        }

        public readonly string bodyName;
        private bool allGenusKnown;
        public readonly Dictionary<string, object> bodyProps;
        public readonly List<string> knownGenus = new();
        public readonly Dictionary<string, string> knownSpecies = new();
        private readonly HashSet<string> predictions = new();
        private readonly string? targetVariant;
        private readonly List<Clause> targetClauses = new();

        /// <summary> Trace extra diagnostics for a genus, species or variant </summary>
        public static string? logOrganism;

        private BioPredictor(string bodyName, Dictionary<string, object>? bodyProps, string? targetVariant)
        {
            this.bodyName = bodyName;
            this.bodyProps = bodyProps ?? new Dictionary<string, object>();
            this.targetVariant = targetVariant;
        }

        private bool predict(BioCriteria criteria, string? genus, string? species, string? variant, List<BioCriteria>? commonChildren)
        {
            // accumulate values from current node or prior stack frames
            commonChildren = criteria.commonChildren ?? commonChildren;
            genus = criteria.genus ?? genus;
            species = criteria.species ?? species;
            variant = criteria.variant ?? variant;

            //if (species?.Contains("Tectonicas") == true) Debugger.Break();

            if (targetVariant == null)
            {
                // stop here if genus names are known and this criteria isn't one of them
                if (!string.IsNullOrEmpty(genus) && this.allGenusKnown && knownGenus?.Count > 0 && !knownGenus.Contains(genus))
                    return false;
                // or stop here if we already scanned some species from this genus
                // TODO: handle Brain Tree's
                if (genus != null && species != null && knownSpecies?.Count > 0 && knownSpecies.ContainsKey(genus))
                    return false;
            }

            // evaluate current query
            var currentName = (variant == "" ? species! : $"{genus} {species} - {variant}").Trim();
            var failures = testQuery(criteria.query, $"{genus} {species} {variant}".Trim());
            var targetMatch = false;

            //if (this.bodyName.Contains("BC 2") && currentName?.Contains("Tectonicas") == true) Debugger.Break();
            //if (currentName.Contains("Cornibus -") == true) Debugger.Break();

            //if (genus != null && species != null && variant != null) Debugger.Break();

            // add a prediction if no failures and we have genus, species AND variant
            if (failures.Count == 0 && genus != null && species != null && variant != null)
            {
                targetMatch = targetVariant == currentName;

                if (targetVariant == null || targetVariant == currentName)
                {
                    //if (this.bodyName.Contains("BC 2") && currentName?.Contains("Tectonicas") == true) Debugger.Break();
                    this.predictions.Add(currentName);
                }
            }

            // continue into children
            var children = criteria.useCommonChildren ? commonChildren : criteria.children;
            if (children?.Count > 0 && failures.Count == 0)
            {
                foreach (var child in children)
                {
                    //if (this.bodyName.Contains("BC 2") && currentName?.EndsWith("Tectonicas -") == true) Debugger.Break();
                    var childMatch = predict(child, genus, species, variant, commonChildren);
                    if (childMatch && criteria.query?.Count > 0)
                    {
                        //if (this.bodyName.Contains("BC 2") && currentName?.Contains("Tectonicas") == true) Debugger.Break();
                        targetClauses.AddRange(criteria.query);
                    }
                }
            }

            return targetMatch;
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
                    //if (clause.ToString().Contains("star")) System.Diagnostics.Debugger.Break();
                    //if (currentName.Contains("informem", StringComparison.OrdinalIgnoreCase)) System.Diagnostics.Debugger.Break();

                    var propName = Map.properties.GetValueOrDefault(clause.property) ?? clause.property;
                    if (!bodyProps.ContainsKey(propName)) throw new Exception($"Unexpected property: '{propName}' ({clause.property}) from: {query} / {currentName}");

                    var bodyValue = bodyProps.GetValueOrDefault(propName);

                    switch (clause.op)
                    {
                        // match SOME string(s)
                        case Op.Is: this.testIsQuery(clause, bodyValue, failures); break;
                        // match ALL string(s)
                        case Op.All: this.testAllQuery(clause, bodyValue, failures); break;
                        // match NO string(s)
                        case Op.Not: this.testNotQuery(clause, bodyValue, failures); break;
                        // match min/max
                        case Op.Range: this.testRangeQuery(clause, bodyValue, failures); break;
                        // match compositions - ONLY greater-than-or-equals - other operands not yet supported (and may not be needed)
                        case Op.Composition: this.testCompositionQuery(clause, bodyValue, failures); break;
                        case Op.Comment: /* no-op */ break;
                        default:
                            throw new Exception($"Unexpected clause operation: {clause.op}");
                    }
                }
            }

            // trace extra diagnostics?
            if (failures.Count > 0)
            {
                if (logOrganism == "*" || (!string.IsNullOrWhiteSpace(logOrganism) && currentName.EndsWith(logOrganism, StringComparison.OrdinalIgnoreCase)))
                {
                    var queryTxt = "\t" + string.Join("\r\n\t", query!);
                    Game.log($"Prediction failures on '{bodyName}' for organism: {logOrganism} / {currentName}{queryTxt}\r\n ? " + string.Join("\r\n ? ", failures) + "\r\n");
                }
            }

            return failures;
        }

        private void testIsQuery(Clause clause, object? bodyValue, List<ClauseFailure> failures)
        {
            if (clause.values == null) throw new Exception("Missing clause values?");

            if (clause.property == "mats" && bodyValue is Dictionary<string, float>)
            {
                var bodyMats = (Dictionary<string, float>)bodyValue;
                if (!clause.values.Any(v => bodyMats.Any(bv => bv.Key.Equals(v, StringComparison.OrdinalIgnoreCase) && bv.Value > matsMinimalThreshold)))
                    failures.Add(new ClauseFailure(bodyName, "No mats multi match found", clause, string.Join(',', bodyMats)));

                return;
            }

            // Special handling for region/arms queries
            if ((clause.property == "regions") && bodyValue is string)
            {
                if (int.TryParse((string)bodyValue, out var currentRegionId))
                {
                    var allowedRegionIds = new HashSet<int>();

                    // Parse comma-separated region IDs from each clause value
                    foreach (var regionString in clause.values)
                    {
                        var ids = regionString.Split(',');
                        foreach (var id in ids)
                        {
                            if (int.TryParse(id.Trim(), out var regionId))
                            {
                                allowedRegionIds.Add(regionId);
                            }
                        }
                    }

                    if (!allowedRegionIds.Contains(currentRegionId))
                    {
                        failures.Add(new ClauseFailure(bodyName, "No region match", clause, currentRegionId.ToString()));
                    }
                }
                return;
            }

            // match any clause value from a set of bodyValue strings
            var bodyValues = bodyValue as List<string>;
            if (bodyValue is Dictionary<string, float>)
                bodyValues = ((Dictionary<string, float>)bodyValue).Keys.ToList();

            if (bodyValues != null)
            {
                if (!clause.values.Any(v => bodyValues.Any(bv => bv.Equals(v, StringComparison.OrdinalIgnoreCase))))
                    failures.Add(new ClauseFailure(bodyName, "No multi match found", clause, string.Join(',', bodyValues)));

                return;
            }

            // otherwise ...
            // match any clause value to a singular string bodyValue
            if (bodyValue is string)
            {
                var bodyValueString = (string)bodyValue;
                if (clause.property == "body") // body type
                {
                    // special case for 'body' clauses to use `StartsWith` not `Equals`
                    if (!clause.values.Any(cv => bodyValueString.StartsWith(cv, StringComparison.OrdinalIgnoreCase)))
                        failures.Add(new ClauseFailure(bodyName, "No startsWith match body", clause, bodyValueString));
                }
                else if (clause.property == "volcanism")
                {
                    // special case for 'volcanism' clauses to use `Contains` not `Equals`
                    if (clause.values[0] == "Some")
                    {
                        if (bodyValueString == "None")
                            failures.Add(new ClauseFailure(bodyName, "No match Volcanism Some vs None", clause, bodyValueString));
                    }
                    else if (!clause.values.Any(cv => bodyValueString.Contains(cv, StringComparison.OrdinalIgnoreCase)))
                        failures.Add(new ClauseFailure(bodyName, "No match Volcanism parts", clause, bodyValueString));
                }
                else if (!clause.values.Any(cv => bodyValueString.Equals(cv, StringComparison.OrdinalIgnoreCase)))
                {
                    failures.Add(new ClauseFailure(bodyName, "No single match found", clause, bodyValueString));
                }

                return;
            }

            Game.log($"testIsQuery: Unexpected bodyValue type: {bodyValue?.GetType().Name ?? "(is null)"}");
            //Debugger.Break();
        }

        private void testAllQuery(Clause clause, object? bodyValue, List<ClauseFailure> failures)
        {
            if (clause.values == null) throw new Exception("Missing clause values?");

            // convert singular bodyValue strings into a list
            if (bodyValue is string)
                bodyValue = new List<string>() { (string)bodyValue };

            // match ALL clause values from a set of bodyValue strings
            var bodyValues = bodyValue as List<string>;
            if (bodyValue is Dictionary<string, float>)
                bodyValues = ((Dictionary<string, float>)bodyValue).Keys.ToList();

            if (bodyValues != null)
            {
                if (bodyValues != null && !clause.values.All(v => bodyValues.Any(bv => bv.Equals(v, StringComparison.OrdinalIgnoreCase))))
                    failures.Add(new ClauseFailure(bodyName, "Not ALL found", clause, string.Join(',', bodyValues)));
            }
            else
            {
                Game.log($"testAllQuery: Unexpected bodyValue type: {bodyValue?.GetType().Name ?? "(is null)"}");
                Debugger.Break();
            }
        }

        private void testNotQuery(Clause clause, object? bodyValue, List<ClauseFailure> failures)
        {
            if (clause.values == null) throw new Exception("Missing clause values?");

            // Special handling for region/arms queries
            if ((clause.property == "regions") && bodyValue is string)
            {
                if (int.TryParse((string)bodyValue, out var currentRegionId))
                {
                    var disallowedRegionIds = new HashSet<int>();

                    // Parse comma-separated region IDs from each clause value
                    foreach (var regionString in clause.values)
                    {
                        var ids = regionString.Split(',');
                        foreach (var id in ids)
                        {
                            if (int.TryParse(id.Trim(), out var regionId))
                            {
                                disallowedRegionIds.Add(regionId);
                            }
                        }
                    }

                    if (disallowedRegionIds.Contains(currentRegionId))
                    {
                        failures.Add(new ClauseFailure(bodyName, "Match with disallowed region", clause, currentRegionId.ToString()));
                    }
                }
                return;
            }

            // convert singular bodyValue strings into a list
            if (bodyValue is string)
                bodyValue = new List<string>() { (string)bodyValue };

            // match NO value from a set of strings
            var bodyValues = bodyValue as List<string>;
            if (bodyValue is Dictionary<string, float>)
                bodyValues = ((Dictionary<string, float>)bodyValue).Keys.ToList();

            if (bodyValues != null)
            {
                if (clause.values.Any(v => bodyValues.Any(bv => bv.Equals(v, StringComparison.OrdinalIgnoreCase))))
                    failures.Add(new ClauseFailure(bodyName, "Must NOT have", clause, string.Join(',', bodyValues)));
            }
            else
            {
                Game.log($"testNotQuery: Unexpected bodyValue type: {bodyValue?.GetType().Name ?? "(is null)"}");
                Debugger.Break();
            }
        }

        private void testRangeQuery(Clause clause, object? bodyValue, List<ClauseFailure> failures)
        {
            if (bodyValue is double)
            {
                var doubleValue = (double)bodyValue;
                if (clause.min != null && doubleValue < clause.min)
                {
                    failures.Add(new ClauseFailure(bodyName, "Below min", clause, doubleValue.ToString("n6")));
                    return;
                }
                if (clause.max != null && doubleValue > clause.max)
                {
                    failures.Add(new ClauseFailure(bodyName, "Above max", clause, doubleValue.ToString("n6")));
                    return;
                }
            }
            else
            {
                throw new Exception($"Non-numeric body property '{bodyValue}' ({bodyValue?.GetType().Name}) for clause '{clause}'");
            }
        }

        private void testCompositionQuery(Clause clause, object? bodyValue, List<ClauseFailure> failures)
        {
            var bodyCompositions = bodyValue as Dictionary<string, float>;
            if (bodyCompositions != null)
            {
                // multiple composition clauses should be OR'd (unlike other clauses that use AND)
                var localFailures = new List<ClauseFailure>();
                foreach (var clauseComposition in clause.compositions!)
                {
                    if (!bodyCompositions.ContainsKey(clauseComposition.Key))
                    {
                        localFailures.Add(new ClauseFailure(bodyName, $"'{clauseComposition.Key}' not present", clause, JsonConvert.SerializeObject(bodyCompositions)));
                    }
                    else if (bodyCompositions[clauseComposition.Key] < clauseComposition.Value)
                    {
                        localFailures.Add(new ClauseFailure(bodyName, $"'{clauseComposition.Key}' too low", clause, JsonConvert.SerializeObject(bodyCompositions)));
                    }
                }
                // we need all these composition conditions to fail for this whole clause to fail
                if (localFailures.Count == clause.compositions.Count)
                    failures.AddRange(localFailures);
            }
            else
            {
                //Game.log($"testCompositionQuery: Unexpected bodyValue type: {bodyValue?.GetType().Name ?? "(is null)"} ({clause})");
                //Debugger.Break();
            }
        }

        public static void logMissedPredictions(SystemBody body, List<string> missedGenus)
        {
            var header = missedGenus
                .Select(g => BioScan.genusNames.GetValueOrDefault(g) ?? g)
                .formatWithHeader($"Missed {missedGenus.Count} prediction(s) on body: {body.shortName}, {body.system.name} (id64: {body.system.address})", "\r\n- ");
            Game.log(header);

            // prep a report for this system
            var txt = new StringBuilder();
            txt.AppendLine(header);
            txt.AppendLine();
            txt.AppendLine("System json:");
            txt.AppendLine(JsonConvert.SerializeObject(body.system, Formatting.Indented));
            txt.AppendLine();

            // write to a file
            var n = 0;
            var filepath = "";
            do
            {
                filepath = Path.Combine(folderMissedReports, $"{body.system.name}_{++n}.txt");
            } while (File.Exists(filepath));
            Data.saveWithRetry(filepath, txt.ToString(), true);
        }

        #region automated tests

        private static async Task testSystem(long address)
        {
            var cmdr = "none";
            var fid = "F101";

            // load known bio signals from Canonn, and body data from Spansh
            var pendingBioStats = Game.canonn.systemBioStats(address);
            var pendingSpanshDump = Game.spansh.getSystemDump(address);
            await Task.WhenAll(pendingBioStats, pendingSpanshDump);

            var bioStats = pendingBioStats.Result;
            var systemData = SystemData.From(pendingSpanshDump.Result, fid, cmdr);

            // force current region to be the one for this test
            var region = EliteDangerousRegionMap.RegionMap.FindRegion(bioStats.coords.x, bioStats.coords.y, bioStats.coords.z);
            GalacticRegions.currentIdxOverride = region!.Id;

            if (systemData.nebulaDist == 0)
                await systemData.getNebulaDist();

            // predict this system
            foreach (var body in systemData.bodies.ToList())
            {
                var realBody = bioStats.bodies.Find(b => b.bodyId == body.id);
                if (realBody?.signals?.genuses == null || realBody.signals.genuses.Count == 0) continue; // skip bodies without known bio signals
                if (realBody?.signals?.biology == null)
                {
                    Game.log($"** Missing realBody?.signals?.biology for: {body.name} (system id64: {systemData.address})");
                    continue;
                }

                BioPredictor.logOrganism = "";
                var predictions = BioPredictor.predict(body); // <--- drag execution up to here

                var countSuccess = predictions.Count(p => realBody.signals.biology.Contains(p));
                var missed = realBody.signals.biology.Where(b => !predictions.Contains(b)).Order().ToList();
                var wrong = predictions.Where(p => !realBody.signals.biology.Contains(p)).Order().ToList();
                var countWrong = predictions.Count - countSuccess;
                var txt = $"\r\n** {body.system.address} '{body.name}' ({body.id}) - actual count: {realBody.signals.biology.Count}, success: {countSuccess}, missed: {missed.Count}, wrong: {wrong.Count} **\r\nREAL:\r\n\t" + string.Join("\r\n\t", realBody.signals.biology) + $"\r\nPREDICTED WRONG:\r\n\t" + string.Join("\r\n\t", wrong) + "\r\n";

                if (predictions.Count > 0 && missed.Count > 0)
                {
                    txt += $"MISSED: \r\n\t" + string.Join("\r\n\t", missed);
                    BioPredictor.logOrganism = missed.First().Split(" ")[1]; // for legacy bio's - comment out .split(..)
                    Game.log(txt);
                    Debugger.Break(); // When this hits. Clear the debug console and drag the execution point up to the "BioPredictor.predict(body)" line above
                }
                else if (wrong.Count > 4)
                {
                    // TODO: write these to some file?
                    Game.log(txt);
                }
                else
                {
                    Game.log(txt);
                }
            }

            Game.log($"Done: {systemData.name} ({address})");
        }

        public static async Task testMissedSystem()
        {
            // paste test details here:
            var filename = "xxx";
            var target = "xxx";
            var missed = "xxx";
            BioPredictor.logOrganism = missed;

            // run the test ...
            var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", filename);
            var txt = File.ReadAllText(filepath);
            var json = txt.Split("System json:").Last().Trim();
            var systemData = JsonConvert.DeserializeObject<SystemData>(json)!;
            SystemData.prep(systemData);

            // force current region to be the one for this test
            var region = EliteDangerousRegionMap.RegionMap.FindRegion(systemData.starPos[0], systemData.starPos[1], systemData.starPos[2]);
            GalacticRegions.currentIdxOverride = region!.Id;
            if (systemData.nebulaDist == 0) await systemData.getNebulaDist();
            var body = systemData.bodies.Find(b => b.shortName.like(target))!;

            Game.log($"**** https://signals.canonn.tech/?system={systemData.address}");

            var predictions = BioPredictor.predict(body); // <--- drag execution up to here

            if (!predictions.Any(t => t.Contains(missed, StringComparison.OrdinalIgnoreCase)))
            {
                Game.log(predictions.formatWithHeader($"Missed: '{missed}' from:\r\n"));
                Debugger.Break();
            }

            Game.log("All done.");
        }

        public static async Task testSystemsAsync()
        {
            var testSystems = new List<long>()
            {
                /* these are good */
                3136055823779, //    prua phoe jr-u d3-91     - inner scutum-centaurus arm
                7259190873515, //    prua phoe lx-s d4-211    - inner scutum-centaurus arm
                6125336284579, //    prua phoe ir-u d3-178    - inner scutum-centaurus arm
                6121703676746, //    prua phoe uo-n c8-22     - inner scutum-centaurus arm
                6365837675955, //    prua phoe pd-r d5-185    - inner scutum-centaurus arm
                284180729219, //     prua phoe wi-b d8        - inner scutum-centaurus arm
                2415457537675, //    graea hypue aa-z d70     - norma expanse
                2312378322571, //    graea hypue aa-z d67     - norma expanse
                2003140677259, //    graea hypue aa-z d58     - norma expanse
                1728262770315, //    graea hypue aa-z d50     - norma expanse
                1659543293579, //    graea hypue aa-z d48     - norma expanse
                1350305648267, //    graea hypue aa-z d39     - norma expanse

                /* more from me */
                1144147218059, //    graea hypue aa-z d33     - norma expanse
                1659576977859, //    swoiwns oe-o d7-48       - inner orion spur // bc1 is trouble? needs help
                319933188363, //     wregoe ja-z d9           - inner orion spur
                546399072737, //     nyeajeou vp-g b56-0      - temple
                241824687268, //     hip 17694                - inner orion spur
                83718378202, //      bd+47 2267               - inner orion spur
                2878029308905, //    2mass j05334575-0441245  - sanguineous rim
                2930853613195, //    graea hypue aa-z d85     - norma expanse
                40280107390979, //   bleethuae ln-b d1172     - izanami
                83718410970, //      hip 76045                - inner orion spur
                //2557619442410, //    hip 97950                - inner orion spur <-- abc 1 f, g, h: no parent star?
                52850328756, //      gd 140                   - inner orion spur
                125860586676, //     hr 5716                  - inner orion spur
                //7373867459, //       ushosts lc-m d7-0        - elysian shore <-- ab 1 e: no parent star?
                //113170581619, //     slegi xv-c d13-3         - elysian shore <-- ab 3 a: no parent star?
                8055311831762, //    nltt 55164               - inner orion spur
                721911088556658, //  eorld byoe bq-g c13-2626 - ryker's hope
                1005802506067, //    heart sector ze-a d29    - elysian shore
                2789153444971, //    phimbo gc-d d12-81       - perseus arm
                33682769023907, //   phroi pra pp-v d3-980    - galactic centre


                /* new places to try */
                147547244739, //     outorst oc-m d7-4         - elysian shore
                79347697283, //      cyoidai vi-b d2           - sanguineous rim
                /*8084881608371,*/ //    graea hypue is-r d5-235   - norma expanse // legacy in atmosphere :/ fails to predict brain tree's - this is on purpose
                37790682707, //      bleae phlai ak-i d9-1     - errant marches
                10887906389, //      eor audst lm-w f1-20      - odin's hold
                234056927058952, //  phroi pri gm-w a1-13      - galactic centre
                2009339794090, //    synuefe fo-t c19-7        - inner orion spur
                5264816150115, //    hypaa bliae nd-h d11-153  - outer orion-perseus conflux
                3464481251, //       pidgio gs-h d11-0         - errant marches
                51239337267043, //   blua eaec ed-h d11-1491   - inner scutum-centaurus arm
                683033437569, //     col 173 sector vv-d b28-0 - inner orion spur <-- b 3: many are wrong? needs help
                113808345931, //     blu euq nh-l d8-3         - inner orion spur
                305709086413707, //  stuemeae fg-y d8897       - galactic centre
                184943642675, //     heguae nl-p d5-5          - sanguineous rim <-- ab 1 b: many are wrong? needs help

                /* top 20 bodies */
                216887347755, //     aucoks rx-s d4-6         - inner orion spur
                /* 1182953163019,*///hyuedau lv-y d34         - achilles's altar - did someone really see tussock virgam - emerald, not yellow? otherwise this is good */
                43847125659, //      drojau bg-w d2-1         - inner orion spur
                2302134985738, //    athaiwyg eg-y c8         - arcadian stream
                672833020273, //     flyooe eohn cs-h b43-0   - sanguineous rim
                //3931941933746, //    lyncis sector cl-y c14   - inner orion spur <-- abc 1 c,d, etc: no parent stars?
                11548763827697, //   blaa drye wc-f b58-5     - temple
                11360960255658, //   blau eur rz-o c19-41     - hawking's gap
                721151664337, //     slegeae su-r b24-0       - sanguineous rim
                674712855233, //     outotz zq-k b22-0        - sanguineous rim
                //102509547578, //     hegou fb-s c6-0          - sanguineous rim <-- bodies with no parent stars
                2851187073897, //    oochoss nm-k b42-1       - elysian shore
                1148829126400920, // byoomao cg-d a108-65     - galactic centre
                111098727130, //     groem bl-e c25-0         - kepler's crest
                612973965713, //     wruetheia nl-u b46-0     - formorian frontier
                4879485709721, //    blie eup rq-o b47-2      - elysian shore
                265348273105, //     dryaa bloae ii-n b54-0   - outer arm
                787453456673, //     nyeakeia za-v b33-0      - hawking's gap
                629372094563, //     hegoo fw-e d11-18        - sanguineous rim
                1976177703003690, // choomee if-r c4-7189     - empyrean straits
                //7373867459, //       ushosts lc-m d7-0        - elysian shore <-- ab 1 e: no parent star?
                //113170581619, //     slegi xv-c d13-3         - elysian shore <-- bodies with no parent stars
                8055311831762, //    nltt 55164               - inner orion spur
                721911088556658, //  eorld byoe bq-g c13-2626 - ryker's hope
                1005802506067, //    heart sector ze-a d29    - elysian shore
                2789153444971, //    phimbo gc-d d12-81       - perseus arm
                33682769023907, //   phroi pra pp-v d3-980    - galactic centre

                ///* top 20 systems */
                //3107241202402, //    col 285 sector bs-i c10-11 - inner orion spur <-- bodies with no parent stars
                2962579378659, //    kyloagh pe-g d11-86        - orion-cygnus arm
                16604217544995, //   eol prou qs-t d3-483       - inner scutum-centaurus arm
                1182223274666, //    synuefai mw-u c19-4        - inner orion spur
                1453569624435, //    hip 82068                  - inner orion spur - hip 82068 9 f is missing an atmosphere
                358999069386, //     76 leonis                  - inner orion spur
                233444419892, //     hypio flyao xp-p e5-54     - arcadian stream
                10612427019, //      hip 56843                  - inner orion spur
                10376464763, //      hd 221180                  - inner orion spur
                //3626137373140, //    phaa audst gw-w e1-844     - odin's hold <-- bodies with no parent stars
                91956533317099, //   pru aim gr-d d12-2676      - inner scutum-centaurus arm - revisit abcd 1 a - clypeus speculumi distance calculation needs fixing  <-- bodies with no parent stars
                //15149635267028, //   phua aub wu-x e1-3527      - galactic centre <-- bodies with no parent stars
                455962777099, //     scheau bluae jc-b d1-13    - odin's hold
                1693617998187, //    synuefue zx-f d12-49       - inner orion spur
                //27118431768755, //   dryio flyuae iy-q d5-789   - inner scutum-centaurus arm <-- bodies with no parent stars
                1005903105339, //    skaude gd-q d6-29          - inner scutum-centaurus arm
                800801672259, //     flyooe hypue ft-o d7-23    - inner orion spur
                2004164284331, //    byoi aip ve-r d4-58        - norma arm // stratum emerald star f vs n ?? needs help
                14096678161971, //   clooku hi-r d5-410         - inner scutum-centaurus arm
                175621288252019, //  dumbio gn-b d13-5111       - odin's hold

                /* more ad-hoc systems */
                //113170581619, //     slegi xv-c d13-3         - elysian shore <-- bodies with no parent stars
                8055311831762, //    nltt 55164               - inner orion spur
                721911088556658, //  eorld byoe bq-g c13-2626 - ryker's hope
                1005802506067, //    heart sector ze-a d29    - elysian shore
                2789153444971, //    phimbo gc-d d12-81       - perseus arm
                33682769023907, //   phroi pra pp-v d3-980    - galactic centre
                27011785954, //     cumbou yh-f c26-0

                /* aleoida coronamus - lime (l star systems) */
                2492825675329, // pra dryoo ul-x b7-1
                633272537650, // synuefai ea-u c5-2
                962207294841, // hyuedeae ug-w b43-0
                1726677521610, // bleae thaa xx-h c23-6
                516869988849, // slegue tp-z b57-0

                /* bark mounds */
                // resume here !!!
                13876099622273, // pencil sector mr-w b1-6
                2036007784483, // eulail rx-t d3-59
                869487643043, // ic 4604 sector dl-y d25 <-- wrong star l vs g for fonticulua campestris? needs help

                // amphora plant
                13648186819, // eifoqs xz-n d7-0
                82032053243, // pyroifa dx-a d14-2
                320570575667, // blaa dryou fn-r d5-9
                150969781115, // blaea euq oo-z d13-4

                /* luteolum anemone */
                52837737636, // hr 326
                4998038101, // hip 42398
                1238889013, // hd 37127

                /* prasinum bioluminescent anemone */
                284175090653, // floawns os-u f2-529
                36011151, // gcrv 950

                /* brain trees */
                802563263091, // eta carina sector el-y d23
                //835329116475, // col 132 sector bm-m d7-24 <-- bodies with no parent stars
                1797418617131, // col 140 sector bq-y d52

                /* tubers */
                143518344886673, // blua hypa pi-a b47-65 (partially useful)

                /* shards */
                100562634522, // aidoms mt-u c2-0 (partially useful)

                /* fonticulua campestris */
                77409424274, // prae drye xn-w c16-0 a 3

                361481876986, //       greae flyao uf-d c29-1   - check 1 d
                3650755408786, //      prae pruae eg-y c16-13 // ? why no fonticular campestris - amethyst 
                6406178542290, // guathiti 8 a // no fumerola nitris lime


                675416645714, // eolls ploe yk-l c9-2 - has root barycentre but alas no concrete bio data for comparisons
                84431081539, // eolls ploe ox-l d7-2

                353504315603, // oochody yf-l d9-10 // consistently wrong star choice?
                49786130467, // eock flyao xy-s d3-1 // single star system

                113053059083, // slegi av-y d3 // not predicted: stratum tectonicas green / due to a neutron being deemed the hottest but it should have been the local m stars
                2282674557658, // vodyakamana // <-- bc4 fails because argon is < 100% but there's no nitrogen in the atmosphere (or anything else?!)
                1050522316081, // ihad bk-l b35-0 // <-- we are missing data about the star and canonn has incomplete bio data for ihad bk-l b35-0 1 a

                2519946200947, // qiefoea kz-d d13-73 // d4 osseus not found or d3 stratum not found
                10393127859, // chaloa pi-r d5-0 // missing stratum tectonicas green, cactoida cortexum amethyst & frutexa metallicum grey ? 
                7269366113697, // icz zj-z b3
            };

            Game.log($"Testing {testSystems.Count} systems ...");
            var startTime = DateTime.Now;
            useTestCache = true;
            try
            {
                // in series
                foreach (var address in testSystems) await testSystem(address);

                //// in parallel
                //await Task.WhenAll(testSystems.Select(a => testSystem(a)));
            }
            finally
            {
                useTestCache = false;
            }
            Game.log($"\r\n**\r\n** All done: everything - count: {testSystems.Count}, duration: {DateTime.Now - startTime}\r\n**");
        }

        #endregion
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
            return $"{bodyName}: {reason} from '{clause}' with '{bodyValue}'";
        }
    }
}
