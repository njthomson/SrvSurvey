using BioCriteria;
using Newtonsoft.Json;
using SrvSurvey.game;
using System.Globalization;
using System.Text;

namespace SrvSurvey
{
    /// <summary>
    /// Represents criteria to predict a variant of a species
    /// </summary>
    internal class BioPrediction
    {
        #region data members

        //public int entryId;
        public string genus;
        //public string species;
        public string variant;
        public string displayName;
        public string color;
        public ColorFactor colorFactor;
        public string colorMatch;
        public long countSeen;

        public List<string> atmospheres; // eg: "thin carbon dioxide atmosphere" NOT atmosphereType
        public List<string> bodyTypes;
        public List<string> volcanism;
        public double minT;
        public double maxT;
        public double minG;
        public double maxG;
        public double minP;
        public double maxP;
        public double minD;
        public double maxD;
        public List<int> galacticRegions;

        #endregion

        private static List<BioPrediction> allCriteria;

        private static List<string> colorMatsAntYtt = "antimony,polonium,ruthenium,technetium,tellurium,yttrium".Split(",").ToList();
        private static List<string> colorMatsCadTun = "cadmium,mercury,molybdenum,niobium,tin,tungsten".Split(",").ToList();

        public static async Task buildPredictionMap()
        {
            allCriteria = new List<BioPrediction>();

            foreach (var genusRef in Game.codexRef.genus)
            {
                var genusName = genusRef.englishName;
                if (genusName == "Brain Trees") genusName = "Brain Tree";

                var stats = await game.Game.canonn.biostats(genusName);
                foreach (var _ in stats.Values)
                {
                    Game.log($"> {_.name}");
                    var bioMatch = Game.codexRef.matchFromVariant(_.id);

                    var regions = _.regions.Select(r =>
                    {
                        var key = GalacticRegions.mapRegions.First(rr => rr.Value == r).Key;
                        // eg: "$Codex_RegionName_1;"
                        return int.Parse(key.Replace(";", "").Substring(18), CultureInfo.InvariantCulture);
                    });

                    var color = _.name.Split(' ').Last();
                    var parts = _.id.Replace("$Codex_Ent_", "").Replace("_Name;", "").Split('_');
                    var genus = parts.First();

                    var colorMatch = parts.Last();
                    var colorMatchLowerCase = parts.Last().ToLowerInvariant();

                    var colorFactor = ColorFactor.None;
                    if (colorMatsAntYtt.Contains(colorMatchLowerCase))
                        colorFactor = ColorFactor.AntYtt;
                    else if (colorMatsCadTun.Contains(colorMatchLowerCase))
                        colorFactor = ColorFactor.CadTun;
                    else if (bioMatch.genus.odyssey)
                        colorFactor = ColorFactor.Star;

                    // ignore histogram data if it's less than 1% of the total count?
                    var p1 = (float)_.count / 100f;
                    var atmosTypes = _.histograms.atmos_types
                        .Where(at => at.Value > p1 && at.Value > 3)
                        .Select(at => at.Key)
                        .ToList();
                    var bodyTypes = _.histograms.body_types
                        .Where(at => at.Value > p1 && at.Value > 3)
                        .Select(at => at.Key)
                        .ToList();
                    var volcanicTypes = _.histograms.volcanic_body_types
                        .Where(at => at.Value > p1 && at.Value > 3)
                        .Select(at => at.Key.Split(" - ").Last())
                        .ToHashSet()
                        .ToList();

                    if (!bioMatch.genus.odyssey)
                    {
                        // Whilst legacy organisms have been found on bodies with atmospheres, pretend otherwise as this causes too many false positives
                        atmosTypes = new List<string>() { "No atmosphere" };
                        // They also have no colored variants
                        colorFactor = ColorFactor.None;
                        colorMatch = null;
                    }

                    var criteria = new BioPrediction()
                    {
                        genus = genus,
                        variant = _.id,
                        displayName = _.name,
                        countSeen = _.count,
                        color = color,
                        colorFactor = colorFactor,
                        colorMatch = colorMatch ?? "",
                        bodyTypes = bodyTypes,
                        volcanism = volcanicTypes,
                        atmospheres = atmosTypes,
                        galacticRegions = regions.ToList(),
                        minT = _.mint ?? 0,
                        minG = _.ming ?? 0,
                        minP = _.minp ?? 0,
                        minD = _.mind ?? 0,
                        maxT = _.maxt ?? 0,
                        maxG = _.maxg ?? 0,
                        maxP = _.maxp ?? 0,
                        maxD = _.maxd ?? 0,
                    };
                    allCriteria.Add(criteria);
                }
            }

            // save out to file
            var json = new StringBuilder();
            json.AppendLine("[");
            foreach (var _ in allCriteria)
                json.AppendLine(JsonConvert.SerializeObject(_, Formatting.None) + ",");
            json.AppendLine("]");

            var filepath = "d:\\allBioCriteria.json";
            File.WriteAllText(filepath, json.ToString());

            Game.log($"> done!");
        }

        public static void loadPredictionMap()
        {
            var filepath = "d:\\allBioCriteria.json";
            var json = File.ReadAllText(filepath);

            allCriteria = JsonConvert.DeserializeObject<List<BioPrediction>>(json)!;
        }

        public static List<string> predict(SystemBody body, SystemData systemData)
        {
            if (body == null) return null!;

            if (allCriteria == null)
                loadPredictionMap();

            var galacticRegion = GalacticRegions.currentIdx;

            var predictions = new List<string>();
            var surfaceGravity = body.surfaceGravity / 10f;
            var surfacePressure = body.surfacePressure / 10_000f;
            var bodyHasVolcanism = string.IsNullOrEmpty(body.volcanism) && body.volcanism == "No volcanism";

            var parentStarTypes = systemData.getParentStarTypes(body, false);
            Game.log($"Body: {body.name} => parentStarClass: " + string.Join(',', parentStarTypes));

            //string? parentStarClass = systemData!.getParentStarType(body, true);
            //Game.log($"Body: {body.name} => parentStarClass:{parentStarClass}");

            foreach (var _ in allCriteria!)
            {
                // Ignore legacy for now...
                var bioMatch = Game.codexRef.matchFromVariant(_.variant);
                if (!bioMatch.genus.odyssey) continue;

                //if (_.displayName.Contains("Bacterium Aurasus")) System.Diagnostics.Debugger.Break();

                // correct star type?
                if (_.colorFactor == ColorFactor.Star && !parentStarTypes.Contains(_.colorMatch)) continue;

                // correct material ...
                if (_.colorFactor == ColorFactor.AntYtt || _.colorFactor == ColorFactor.CadTun)
                {
                    //if (!body.materials.ContainsKey(_.colorFactor.ToLowerInvariant())) continue;

                    var matsGroup = _.colorFactor == ColorFactor.AntYtt ? colorMatsAntYtt : colorMatsCadTun;

                    var topRelevantMat = body.materials.Where(m => matsGroup.Contains(m.Key))
                        .OrderBy(m => m.Value)
                        .FirstOrDefault()
                        .Key;

                    if (topRelevantMat != null && _.colorMatch.ToLowerInvariant() != topRelevantMat) continue;
                }

                // atmosphere ...
                if (!_.atmospheres.Contains(body.atmosphere)) continue;
                // Whilst legacy organisms have been found on bodies with atmospheres ... this makes too many false positives :(
                if (!bioMatch.genus.odyssey && body.atmosphere != "No atmosphere") continue;

                // body volcanism ...
                var criteriaNoVolcanism = _.volcanism.Count == 1 && _.volcanism[0] != "No volcanism";
                if (bodyHasVolcanism != criteriaNoVolcanism) continue;
                if (bodyHasVolcanism && !_.volcanism.Contains(body.volcanism!)) continue;

                // simple criteria
                if (!_.bodyTypes.Contains(body.planetClass!)) continue;
                if (_.galacticRegions?.Contains(galacticRegion) == false) continue;
                if (body.surfaceTemperature < _.minT || body.surfaceTemperature > _.maxT) continue;
                //if (surfacePressure < _.minP || surfacePressure > _.maxP) continue;
                if (surfaceGravity < _.minG || surfaceGravity > _.maxG) continue;

                // TODO: measure distance to closest Ruins
                if (_.displayName.Contains("Tree")) continue;

                predictions.Add(_.displayName);
            }

            //Game.log($"Predictions for: {body.name}\r\n > " + string.Join("\r\n > ", predictions));

            return predictions;
        }

        public static async Task testManyNewVsOld()
        {
            var txt3 = new StringBuilder();
            txt3.AppendLine(string.Join(',', new List<string>()
            {
                "Body",
                "Region",
                "Actual count",
                "M1 count",
                "M1 correct",
                "M1 missed",
                "M1 false",
                "M2 count",
                "M2 correct",
                "M2 missed",
                "M2 false",
                "M1 missed species",
                "M2 missed species",
            }));

            var testCases = new Dictionary<string, string>()
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

                //// more from me
                //{ "Graea Hypue AA-Z d33", "Norma Expanse" },
                //{ "Swoiwns OE-O d7-48", "Inner Orion Spur" },
                //{ "Wregoe JA-Z d9", "Inner Orion Spur" },
                //{ "Nyeajeou VP-G b56-0", "Temple" },
                //{ "HIP 17694", "Inner Orion Spur" },
                //{ "BD+47 2267", "Inner Orion Spur" },
                { "2MASS J05334575-0441245", "Sanguineous Rim" },
                //{ "Graea Hypue AA-Z d85", "Norma Expanse" },
                //{ "Bleethuae LN-B d1172", "Izanami" },
                //{ "HIP 76045", "Inner Orion Spur" },
                //{ "HIP 97950", "Inner Orion Spur" },
                //{ "GD 140", "Inner Orion Spur" },
                //{ "HR 5716", "Inner Orion Spur" },

                //// from Cupco
                //// { "Swoilz NJ-O d7-10", "Inner Orion Spur" },

                //// top 20 bodies
                //{ "Aucoks RX-S d4-6", "Inner Orion Spur" },
                //{ "Hyuedau LV-Y d34", "Achilles's Altar" },
                //{ "Drojau BG-W d2-1", "Inner Orion Spur" },
                //{ "Athaiwyg EG-Y c8", "Arcadian Stream" },
                //{ "Flyooe Eohn CS-H b43-0", "Sanguineous Rim" },
                //{ "Lyncis Sector CL-Y c14", "Inner Orion Spur" },
                //{ "Blaa Drye WC-F b58-5", "Temple" },
                //{ "Blau Eur RZ-O c19-41", "Hawking's Gap" },
                //{ "Slegeae SU-R b24-0", "Sanguineous Rim" },
                //{ "Outotz ZQ-K b22-0", "Sanguineous Rim" },
                //{ "Hegou FB-S c6-0", "Sanguineous Rim" },
                //{ "Oochoss NM-K b42-1", "Elysian Shore" },
                //{ "Byoomao CG-D a108-65", "Galactic Centre" },
                //{ "Groem BL-E c25-0", "Kepler's Crest" },
                //{ "Wruetheia NL-U b46-0", "Formorian Frontier" },
                //{ "Blie Eup RQ-O b47-2", "Elysian Shore" },
                //{ "Dryaa Bloae II-N b54-0", "Outer Arm" },
                //{ "Nyeakeia ZA-V b33-0", "Hawking's Gap" },
                //{ "Hegoo FW-E d11-18", "Sanguineous Rim" },
                //{ "Choomee IF-R c4-7189", "Empyrean Straits" },

                //// from Cmdr Stromb
                ////{ "Hip 48603" , "Inner Orion Spur" },
                ////{ "Spaidau gb-f d11-1" , "Kepler's Crest" },
                ////{ "Ngc 5662 Sector fb-x d1-32" , "Inner Orion Spur" },
                //{ "Pro Eurl ak-a d62", "Inner Orion Spur" },
                ////{ "Wregoe fs-z c27-17", "Inner Orion Spur" },
                ////{ "Byaa Theia xv-e d11-43", "Inner Orion Spur" },

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

            foreach (var pair in testCases)
            {
                var txt = await testNewVsOld(pair.Key, pair.Value);
                txt3.Append(txt);
            }

            File.WriteAllText(@"d:\bioBulkResults7.csv", txt3.ToString());
            Game.log("All done: everything!");
        }

        public static async Task<string?> testNewVsOld(string systemName, string galacticRegion)
        {
            var cmdr = "GRINNING2002";
            var fid = "F10171085";

            //GalacticRegions.current = galacticRegion;
            //GalacticRegions.currentIdx = GalacticRegions.getIdxFromName(GalacticRegions.mapRegions.FirstOrDefault(_ => _.Value == galacticRegion).Key);

            var bodies = await Game.edsm.getBodies(systemName);
            var system = await Game.edsm.getSystems(systemName);
            var starPos = system.First().coords.starPos;

            var systemData = SystemData.From(bodies, starPos, fid, cmdr);

            var poi = await Game.canonn.getSystemPoi(systemData.name, cmdr);
            if (poi.codex == null) return null;

            var list00 = new Dictionary<string, HashSet<string>>();

            foreach (var c in poi.codex)
            {
                if (c.hud_category != "Biology") continue;
                var bodyName = $"{poi.system} {c.body}";

                if (!list00.ContainsKey(bodyName)) list00.Add(bodyName, new HashSet<string>());
                if (list00[bodyName].Contains(c.english_name)) continue;
                list00[bodyName].Add(c.english_name);
            }

            var results = new List<List<string>>();
            /* columns
             * body name
             * actual count
             * 
             * method1 predict count
             * method1 correct
             * method1 missed
             * 
             * method2 predict count
             * method2 correct
             * method2 missed
             * 
             * actual species
             * 
             * method1 correct
             * method1 missed
             * method1 false
             * 
             * method2 correct
             * method2 missed
             * method2 false
             */
            if (poi.SAAsignals == null) return $"{systemName}, {galacticRegion}, ???\r\n";

            foreach (var s in poi.SAAsignals)
            {
                if (s.hud_category != "Biology") continue;

                var row = new List<string>();
                results.Add(row);

                var bodyName = $"{poi.system} {s.body}";
                row.Add(bodyName);
                row.Add(galacticRegion);

                var txt = new StringBuilder();
                var targetBody = systemData.bodies.Find(_ => _.name == bodyName)!;
                if (!list00.ContainsKey(bodyName)) continue;

                var list0 = list00[bodyName];
                row.Add(list0.Count.ToString()); // actual count
                txt.AppendLine($"Body: {bodyName} - count: {list0.Count}");
                foreach (var o in list0)
                    txt.AppendLine($"> {o}");

                var list1 = BioPrediction.predict(targetBody, systemData);
                row.Add(list1.Count.ToString()); // method1 predicted count
                txt.AppendLine($"\r\nMethod 1:");
                var countMatch = 0;
                foreach (var o in list1)
                {
                    var match = list0.Any(b => b.Contains(o));
                    if (match) countMatch++;

                    txt.Append(match ? "+ " : "X ");
                    txt.AppendLine(o);
                }
                row.Add(countMatch.ToString()); // method1 correct count
                txt.AppendLine($"== len: {list1.Count} matched: {countMatch} of {list0.Count}, missed:");
                row.Add($"{list0.Count - countMatch}"); // method1 missed count
                row.Add($"{list1.Count - countMatch}"); // method1 false count
                if (countMatch < list0.Count)
                {
                    var missed = list0.Where(a => !list1.Contains(a));
                    txt.AppendLine($"  > " + string.Join("\r\n  > ", missed));
                }

                var list2 = Predictor.predict(targetBody); // PotentialOrganism.match(targetBody, null);
                row.Add(list2.Count.ToString()); // method2 predicted count
                txt.AppendLine($"\r\nMethod 2:");
                countMatch = 0;
                foreach (var o in list2)
                {
                    var match = list0.Any(b => b.Contains(o));
                    if (match) countMatch++;

                    txt.Append(match ? "+ " : "X ");
                    txt.AppendLine(o);
                }
                row.Add(countMatch.ToString()); // method1 correct count
                txt.AppendLine($"== len: {list2.Count} matched: {countMatch} of {list0.Count}, missed:");
                row.Add($"{list0.Count - countMatch}"); // method2 missed count
                row.Add($"{list2.Count - countMatch}"); // method2 false count
                if (countMatch < list0.Count)
                {
                    var missed = list0.Where(a => !list2.Any(b => a.Contains(b)));
                    txt.AppendLine($"  > " + string.Join("\r\n  > ", missed));
                }

                row.Add(string.Join(" | ", list0.Where(a => !list1.Any(b => a.Contains(b))))); // missed species 1
                row.Add(string.Join(" | ", list0.Where(a => !list2.Any(b => a.Contains(b))))); // missed species 2

                Game.log("\r\n" + txt.ToString());
                Game.log("!");
            }

            Game.log($"All done: {systemName}");


            var txt2 = new StringBuilder();
            foreach (var row in results)
                txt2.AppendLine(string.Join(',', row));

            return txt2.ToString();
        }
    }

    enum ColorFactor
    {
        None,
        Star,
        AntYtt,
        CadTun,
    }
}
