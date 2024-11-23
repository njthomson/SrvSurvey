using Newtonsoft.Json;
using SrvSurvey;
using SrvSurvey.game;
using System.Diagnostics;

namespace BioCriterias
{
    internal class BioPredictor
    {
        public static float matsMinimalThreshold = 0.25f;
        public static bool useTestCache = false;
        public static string netCache = Path.Combine(Program.dataFolder, "netCache");

        public static List<Clause> predictTarget(SystemBody body, string? targetVariant)
        {
            var predictor = predict(body, targetVariant);
            return predictor?.targetClauses ?? new();
        }

        public static List<string> predict(SystemBody body)
        {
            var predictor = predict(body, null);
            return predictor?.predictions.ToList() ?? new ();
        }

        private static BioPredictor? predict(SystemBody body, string? targetVariant = null)
        {
            if (body.type != SystemBodyType.LandableBody) return null;
            if (body.parents == null || body.parents.Count == 0) return null;
            lock (BioCriteria.allCriteria)
            {
                if (BioCriteria.allCriteria.Count == 0 || Debugger.IsAttached) BioCriteria.readCriteria();
            }

            var parentStar = body.system.getParentStarTypes(body, true).FirstOrDefault();
            if (parentStar == null)
            {
                Game.log($"Why null from getParentStarTypes? For {body.name}");
                parentStar = "";
            }
            var brightestParentStar = body.system.getBrightestParentStarType(body);

            var primaryStar = body.system.bodies.FirstOrDefault(b => b.isMainStar);
            if (primaryStar == null)
                Game.log($"Why null from bodies.Find(b => b.isMainStar)? For {body.name}");
            var primaryStarType = Util.flattenStarType(primaryStar?.starType);

            var parentStars = new List<string>() { parentStar, brightestParentStar }
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            // --- alternate "brightest" ---
            var parentStars2 = body.system.getParentStars(body, false);
            if (parentStars2.Count > 0)
            {
                var parentsByBrightness = parentStars2
                    .ToDictionary(s => s, s => s.getRelativeBrightness(body.distanceFromArrivalLS))
                    .OrderByDescending(s => s.Value)
                    .Take(2);

                var brightest = parentsByBrightness.First();
                parentStars.Clear();
                parentStars.Add(Util.flattenStarType(brightest.Key.starType));

                if (parentStars2.Count >= 2)
                {
                    var nextBrightest = parentsByBrightness.Last();
                    var delta = nextBrightest.Value / brightest.Value;
                    Game.log($"{brightest.Key.name}: {brightest.Value} vs {nextBrightest.Key.name}: {nextBrightest.Value} => {delta}");

                    if (delta > 0.93d)
                        parentStars.Add(Util.flattenStarType(nextBrightest.Key.starType));
                }
            }

            if (parentStars.All(s => s == null))
            {
                Game.log($"Parent stars are not right?! body: {body.name}");
                return null;
            }

            //var sumSemiMajorAxis = body.sumSemiMajorAxis(0);
            //var sumSemiMajorAxisLs = Util.mToLS(sumSemiMajorAxis);

            // calc distance to nearest Guardian bubble
            var withinGuardianBubble = Game.codexRef.isWithinGuardianBubble(body.system.starPos);

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
                { "DistanceFromArrivalLS", (double)body.distanceFromArrivalLS },
                { "Volcanism", string.IsNullOrEmpty(body.volcanism) ? "None" : body.volcanism },
                { "Materials", body.materials },
                { "Region", GalacticRegions.currentIdx.ToString() },
                // Take the first parent star(s) AND the "relative hottest" from the parent chain
                { "Star", parentStars },
                { "ParentStar", parentStar },
                { "PrimaryStar", primaryStarType },
                { "Nebulae", body.system.nebulaDist },
                { "Guardian", withinGuardianBubble.ToString() },

            };
            var predictor = new BioPredictor(body.name, bodyProps, targetVariant);

            // add known genus and species names
            if (body.organisms?.Count > 0)
            {
                foreach (var org in body.organisms)
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
                        predictor.knownSpecies[match.genus.englishName] = match.species.englishName;
                    }
                }

                predictor.allGenusKnown = body.organisms.Count == body.bioSignalCount;
            }

            // log extra diagnostics?
            //logOrganism = "Informem";

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

            //if (species?.Contains("Bullaris") == true) Debugger.Break();

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

            //if (this.bodyName.Contains(" 6") && currentName?.Contains("Stabitis") == true) Debugger.Break();
            //if (currentName?.Contains("Brain") == true) Debugger.Break();

            //if (genus != null && species != null && variant != null) Debugger.Break();

            // add a prediction if no failures and we have genus, species AND variant
            if (failures.Count == 0 && genus != null && species != null && variant != null)
            {
                targetMatch = targetVariant == currentName;

                if (targetVariant == null || targetVariant == currentName)
                    this.predictions.Add(currentName);

            }

            // continue into children
            var children = criteria.useCommonChildren ? commonChildren : criteria.children;
            if (children?.Count > 0 && failures.Count == 0)
            {
                foreach (var child in children)
                {
                    var childMatch = predict(child, genus, species, variant, commonChildren);
                    if (childMatch && criteria.query?.Count > 0)
                        targetClauses.AddRange(criteria.query);
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
                    if (!bodyProps.ContainsKey(propName)) throw new Exception($"Unexpected property: {propName} ({clause.property})");

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
                    Game.log($"Prediction failures for organism: {logOrganism} / {currentName}\r\n{queryTxt}\r\n > " + string.Join("\r\n > ", failures));
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
                Game.log($"testCompositionQuery: Unexpected bodyValue type: {bodyValue?.GetType().Name ?? "(is null)"} ({clause})");
                //Debugger.Break();
            }
        }

        #region automated tests

        private static async Task testSystem(long address)
        {
            var cmdr = "none";
            var fid = "F101";

            // load known bio signals from Canonn, and body data from Spansh
            var pendingBioStats = Game.canonn.systemBioStats(address);
            var pendingSpanshDump = Game.spansh.getSystemDump(address);
            await Task.WhenAll(pendingBioStats);

            var bioStats = pendingBioStats.Result;
            var systemData = SystemData.From(pendingSpanshDump.Result, fid, cmdr);

            // force current region to be the one for this test
            var region = EliteDangerousRegionMap.RegionMap.FindRegion(bioStats.coords.x, bioStats.coords.y, bioStats.coords.z);
            GalacticRegions.currentIdxOverride = region.Id;

            if (systemData.nebulaDist == 0)
                await systemData.getNebulaDist();

            // predict this system
            foreach (var body in systemData.bodies)
            {
                BioPredictor.logOrganism = "";
                var predictions = BioPredictor.predict(body); // <---

                if (predictions.Count > 0)
                {
                    var realBody = bioStats.bodies.Find(b => b.bodyId == body.id);
                    if (realBody?.signals?.biology != null)
                    {
                        var countSuccess = predictions.Count(p => realBody.signals.biology.Contains(p));
                        var missed = realBody.signals.biology.Where(b => !predictions.Contains(b)).Order().ToList();
                        var wrong = predictions.Where(p => !realBody.signals.biology.Contains(p)).Order().ToList();
                        var countWrong = predictions.Count - countSuccess;
                        var txt = $"\r\n** {body.system.address} '{body.name}' ({body.id}) - actual count: {realBody.signals.biology.Count}, success: {countSuccess}, missed: {missed.Count}, wrong: {wrong.Count} **\r\nREAL:\r\n\t" + string.Join("\r\n\t", realBody.signals.biology) + $"\r\nPREDICTED WRONG:\r\n\t" + string.Join("\r\n\t", wrong) + "\r\n";

                        if (missed.Count > 0)
                        {
                            txt += $"MISSED: \r\n\t" + string.Join("\r\n\t", missed);
                            BioPredictor.logOrganism = missed.First().Split(" ")[1]; // revert for non-legacy organisms
                            Game.log(txt);
                            Debugger.Break();
                        }
                        else if (wrong.Count > 5)
                        {
                            Game.log(txt);
                        }
                        else
                        {
                            Game.log(txt);
                        }
                    }
                    else
                    {
                        Game.log($"\r\n** {body.system.address} '{body.name}' ({body.id}) nothing real, but predicted: **\r\n\t" + string.Join("\r\n\t", predictions) + "\r\n");
                        //Debugger.Break();
                    }

                }
            }

            Game.log($"Done: {systemData.name} ({address})");
        }

        public static async Task testSystemsAsync()
        {
            var testSystems = new List<long>()
            {
                ///* these are good */
                //3136055823779, //    Prua Phoe JR-U d3-91     - Inner Scutum-Centaurus Arm
                //7259190873515, //    Prua Phoe LX-S d4-211    - Inner Scutum-Centaurus Arm
                //6125336284579, //    Prua Phoe IR-U d3-178    - Inner Scutum-Centaurus Arm
                //6121703676746, //    Prua Phoe UO-N c8-22     - Inner Scutum-Centaurus Arm
                //6365837675955, //    Prua Phoe PD-R d5-185    - Inner Scutum-Centaurus Arm
                //284180729219, //     Prua Phoe WI-B d8        - Inner Scutum-Centaurus Arm
                //2415457537675, //    Graea Hypue AA-Z d70     - Norma Expanse
                //2312378322571, //    Graea Hypue AA-Z d67     - Norma Expanse
                //2003140677259, //    Graea Hypue AA-Z d58     - Norma Expanse
                //1728262770315, //    Graea Hypue AA-Z d50     - Norma Expanse
                //1659543293579, //    Graea Hypue AA-Z d48     - Norma Expanse
                //1350305648267, //    Graea Hypue AA-Z d39     - Norma Expanse
                //7373867459, //       Ushosts LC-M d7-0        - Elysian Shore
                //113170581619, //     Slegi XV-C d13-3         - Elysian Shore
                //8055311831762, //    NLTT 55164               - Inner Orion Spur
                //721911088556658, //  Eorld Byoe BQ-G c13-2626 - Ryker's Hope
                //1005802506067, //    Heart Sector ZE-A d29    - Elysian Shore
                //2789153444971, //    Phimbo GC-D d12-81       - Perseus Arm
                //33682769023907, //   Phroi Pra PP-V d3-980    - Galactic Centre

                ///* more from me */
                //1144147218059, //    Graea Hypue AA-Z d33     - Norma Expanse
                //1659576977859, //    Swoiwns OE-O d7-48       - Inner Orion Spur
                //319933188363, //     Wregoe JA-Z d9           - Inner Orion Spur
                //546399072737, //     Nyeajeou VP-G b56-0      - Temple
                //241824687268, //     HIP 17694                - Inner Orion Spur
                //83718378202, //      BD+47 2267               - Inner Orion Spur
                //2878029308905, //    2MASS J05334575-0441245  - Sanguineous Rim
                //2930853613195, //    Graea Hypue AA-Z d85     - Norma Expanse
                //40280107390979, //   Bleethuae LN-B d1172     - Izanami
                //83718410970, //      HIP 76045                - Inner Orion Spur
                //2557619442410, //    HIP 97950                - Inner Orion Spur
                //52850328756, //      GD 140                   - Inner Orion Spur
                //125860586676, //     HR 5716                  - Inner Orion Spur
                //7373867459, //       Ushosts LC-M d7-0        - Elysian Shore
                //113170581619, //     Slegi XV-C d13-3         - Elysian Shore
                //8055311831762, //    NLTT 55164               - Inner Orion Spur
                //721911088556658, //  Eorld Byoe BQ-G c13-2626 - Ryker's Hope
                //1005802506067, //    Heart Sector ZE-A d29    - Elysian Shore
                //2789153444971, //    Phimbo GC-D d12-81       - Perseus Arm
                //33682769023907, //   Phroi Pra PP-V d3-980    - Galactic Centre

                ///* New places to try */
                //147547244739, //     Outorst OC-M d7-4         - Elysian Shore
                //79347697283, //      Cyoidai VI-B d2           - Sanguineous Rim
                //8084881608371, //    Graea Hypue IS-R d5-235   - Norma Expanse // legacy in atmosphere :/
                //37790682707, //      Bleae Phlai AK-I d9-1     - Errant Marches
                //10887906389, //      Eor Audst LM-W f1-20      - Odin's Hold
                //234056927058952, //  Phroi Pri GM-W a1-13      - Galactic Centre
                //2009339794090, //    Synuefe FO-T c19-7        - Inner Orion Spur
                //5264816150115, //    Hypaa Bliae ND-H d11-153  - Outer Orion-Perseus Conflux
                //3464481251, //       Pidgio GS-H d11-0         - Errant Marches
                //51239337267043, //   Blua Eaec ED-H d11-1491   - Inner Scutum-Centaurus Arm
                //683033437569, //     Col 173 Sector VV-D b28-0 - Inner Orion Spur
                //113808345931, //     Blu Euq NH-L d8-3         - Inner Orion Spur
                //305709086413707, //  Stuemeae FG-Y d8897       - Galactic Centre
                //184943642675, //     Heguae NL-P d5-5          - Sanguineous Rim
                //7373867459, //       Ushosts LC-M d7-0         - Elysian Shore
                //113170581619, //     Slegi XV-C d13-3          - Elysian Shore
                //8055311831762, //    NLTT 55164                - Inner Orion Spur
                //721911088556658, //  Eorld Byoe BQ-G c13-2626  - Ryker's Hope
                //1005802506067, //    Heart Sector ZE-A d29     - Elysian Shore
                //2789153444971, //    Phimbo GC-D d12-81        - Perseus Arm
                //33682769023907, //   Phroi Pra PP-V d3-980     - Galactic Centre

                ///* top 20 bodies */
                //216887347755, //     Aucoks RX-S d4-6         - Inner Orion Spur
                ///* 1182953163019,*///Hyuedau LV-Y d34         - Achilles's Altar - Did someone really see Tussock Virgam - Emerald, not Yellow? Otherwise this is good */
                //43847125659, //      Drojau BG-W d2-1         - Inner Orion Spur
                //2302134985738, //    Athaiwyg EG-Y c8         - Arcadian Stream
                //672833020273, //     Flyooe Eohn CS-H b43-0   - Sanguineous Rim
                //3931941933746, //    Lyncis Sector CL-Y c14   - Inner Orion Spur
                //11548763827697, //   Blaa Drye WC-F b58-5     - Temple
                //11360960255658, //   Blau Eur RZ-O c19-41     - Hawking's Gap
                //721151664337, //     Slegeae SU-R b24-0       - Sanguineous Rim
                //674712855233, //     Outotz ZQ-K b22-0        - Sanguineous Rim
                //102509547578, //     Hegou FB-S c6-0          - Sanguineous Rim
                //2851187073897, //    Oochoss NM-K b42-1       - Elysian Shore
                //1148829126400920, // Byoomao CG-D a108-65     - Galactic Centre
                //111098727130, //     Groem BL-E c25-0         - Kepler's Crest
                //612973965713, //     Wruetheia NL-U b46-0     - Formorian Frontier
                //4879485709721, //    Blie Eup RQ-O b47-2      - Elysian Shore
                //265348273105, //     Dryaa Bloae II-N b54-0   - Outer Arm
                //787453456673, //     Nyeakeia ZA-V b33-0      - Hawking's Gap
                //629372094563, //     Hegoo FW-E d11-18        - Sanguineous Rim
                //1976177703003690, // Choomee IF-R c4-7189     - Empyrean Straits
                //7373867459, //       Ushosts LC-M d7-0        - Elysian Shore
                //113170581619, //     Slegi XV-C d13-3         - Elysian Shore
                //8055311831762, //    NLTT 55164               - Inner Orion Spur
                //721911088556658, //  Eorld Byoe BQ-G c13-2626 - Ryker's Hope
                //1005802506067, //    Heart Sector ZE-A d29    - Elysian Shore
                //2789153444971, //    Phimbo GC-D d12-81       - Perseus Arm
                //33682769023907, //   Phroi Pra PP-V d3-980    - Galactic Centre

                ///* top 20 systems */
                //3107241202402, //    Col 285 Sector BS-I c10-11 - Inner Orion Spur
                //2962579378659, //    Kyloagh PE-G d11-86        - Orion-Cygnus Arm
                //16604217544995, //   Eol Prou QS-T d3-483       - Inner Scutum-Centaurus Arm
                //1182223274666, //    Synuefai MW-U c19-4        - Inner Orion Spur
                //1453569624435, //    HIP 82068                  - Inner Orion Spur - HIP 82068 9 f is missing an atmosphere
                //358999069386, //     76 Leonis                  - Inner Orion Spur
                //233444419892, //     Hypio Flyao XP-P e5-54     - Arcadian Stream
                //10612427019, //      HIP 56843                  - Inner Orion Spur
                //10376464763, //      HD 221180                  - Inner Orion Spur
                //3626137373140, //    Phaa Audst GW-W e1-844     - Odin's Hold
                //91956533317099, //   Pru Aim GR-D d12-2676      - Inner Scutum-Centaurus Arm - revisit ABCD 1 a - Clypeus Speculumi distance calculation needs fixing
                //15149635267028, //   Phua Aub WU-X e1-3527      - Galactic Centre
                //455962777099, //     Scheau Bluae JC-B d1-13    - Odin's Hold
                //1693617998187, //    Synuefue ZX-F d12-49       - Inner Orion Spur
                //27118431768755, //   Dryio Flyuae IY-Q d5-789   - Inner Scutum-Centaurus Arm
                //1005903105339, //    Skaude GD-Q d6-29          - Inner Scutum-Centaurus Arm
                //800801672259, //     Flyooe Hypue FT-O d7-23    - Inner Orion Spur
                //2004164284331, //    Byoi Aip VE-R d4-58        - Norma Arm // Stratum Emerald star F vs N ??
                //14096678161971, //   Clooku HI-R d5-410         - Inner Scutum-Centaurus Arm
                //175621288252019, //  Dumbio GN-B d13-5111       - Odin's Hold

                ///* more ad-hoc systems */
                //7373867459, //       Ushosts LC-M d7-0        - Elysian Shore
                //113170581619, //     Slegi XV-C d13-3         - Elysian Shore
                //8055311831762, //    NLTT 55164               - Inner Orion Spur
                //721911088556658, //  Eorld Byoe BQ-G c13-2626 - Ryker's Hope
                //1005802506067, //    Heart Sector ZE-A d29    - Elysian Shore
                //2789153444971, //    Phimbo GC-D d12-81       - Perseus Arm
                //33682769023907, //   Phroi Pra PP-V d3-980    - Galactic Centre
                //27011785954, //     Cumbou YH-F c26-0

                ///* Aleoida Coronamus - Lime (L star systems) */
                //2492825675329, // Pra Dryoo UL-X b7-1
                //633272537650, // Synuefai EA-U c5-2
                //962207294841, // Hyuedeae UG-W b43-0
                //1726677521610, // Bleae Thaa XX-H c23-6
                //516869988849, // Slegue TP-Z b57-0

                /* Bark Mounds */
                // resume here !!!
                //13876099622273, // Pencil Sector MR-W b1-6
                //2036007784483, // Eulail RX-T d3-59
                //869487643043, // IC 4604 Sector DL-Y d25

                //// Amphora Plant
                //13648186819, // Eifoqs XZ-N d7-0
                //82032053243, // Pyroifa DX-A d14-2
                //320570575667, // Blaa Dryou FN-R d5-9
                //150969781115, // Blaea Euq OO-Z d13-4

                ///* Luteolum Anemone */
                //52837737636, // HR 326
                //4998038101, // HIP 42398
                //1238889013, // HD 37127

                ///* Prasinum Bioluminescent Anemone */
                //284175090653, // Floawns OS-U f2-529
                //36011151, // GCRV 950

                ///* Brain Trees */
                //802563263091, // Eta Carina Sector EL-Y d23
                //835329116475, // Col 132 Sector BM-M d7-24
                //1797418617131, // Col 140 Sector BQ-Y d52

                ///* Tubers */
                //143518344886673, // Blua Hypa PI-A b47-65 (partially useful)

                ///* Shards */
                //100562634522, // Aidoms MT-U c2-0 (partially useful)

                /* Fonticulua Campestris */
                77409424274, // Prae Drye XN-W c16-0 A 3
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
            Game.log($"All done: everything - count: {testSystems.Count}, duration: {DateTime.Now - startTime}");
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
            return $"{bodyName}: {reason} from '{clause}' in: {bodyValue}";
        }
    }
}
