using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Globalization;

#pragma warning disable CS0649

namespace SrvSurvey
{
    /// <summary>
    /// Represents a single sampling using the BioScanner
    /// </summary>
    class BioScan
    {
        public LatLong2 location;
        public float radius;
        public string? genus;
        public string? species;
        [JsonConverter(typeof(StringEnumConverter))]
        public Status status;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long entryId;

        public override string ToString()
        {
            return $"{species} ({radius}m): @{location}";
        }

        public static Dictionary<string, int> ranges = new Dictionary<string, int>()
        {
            // Distances from https://elite-dangerous.fandom.com/wiki/Genetic_Sampler

            // From Odyssey
            { "$Codex_Ent_Aleoids_Genus_Name;",            150 }, // Aleoida
            { "$Codex_Ent_Bacterial_Genus_Name;",          500 }, // Bacterium
            { "$Codex_Ent_Cactoid_Genus_Name;",            300 }, // Cactoida
            { "$Codex_Ent_Clypeus_Genus_Name;",            150 }, // Clypeus
            { "$Codex_Ent_Conchas_Genus_Name;",            150 }, // Concha
            { "$Codex_Ent_Electricae_Genus_Name;",        1000 }, // Electricae
            { "$Codex_Ent_Fonticulus_Genus_Name;",         500 }, // Fonticulua
            { "$Codex_Ent_Shrubs_Genus_Name;",             150 }, // Frutexa
            { "$Codex_Ent_Fumerolas_Genus_Name;",          100 }, // Fumerola
            { "$Codex_Ent_Fungoids_Genus_Name;",           300 }, // Fungoida
            { "$Codex_Ent_Osseus_Genus_Name;",             800 }, // Osseus
            { "$Codex_Ent_Recepta_Genus_Name;",            150 }, // Recepta
            { "$Codex_Ent_Stratum_Genus_Name;",            500 }, // Stratum
            { "$Codex_Ent_Tubus_Genus_Name;",              800 }, // Tubus
            { "$Codex_Ent_Tussocks_Genus_Name;",           200 }, // Tussock
            // From Horizons
            { "$Codex_Ent_Vents_Name;",                    100 }, // Amphora Plant
            { "$Codex_Ent_Sphere_Name;",                   100 }, // Anemone
            { "$Codex_Ent_Cone_Name;",                     100 }, // Bark Mounds
            { "$Codex_Ent_Brancae_Name;",                  100 }, // Brain Tree
            { "$Codex_Ent_Ground_Struct_Ice_Name;",        100 }, // Crystalline Shards
            { "$Codex_Ent_Tube_Name;",                     100 }, // Sinuous Tubers
        };

        public static Dictionary<string, string> prefixes = new Dictionary<string, string>()
        {
            // From Odyssey
            { "ale", "$Codex_Ent_Aleoids_Genus_Name;"   }, // Aleoida
            { "bac", "$Codex_Ent_Bacterial_Genus_Name;" }, // Bacterium
            { "cac", "$Codex_Ent_Cactoid_Genus_Name;"   }, // Cactoida
            { "cly", "$Codex_Ent_Clypeus_Genus_Name;"}, // Clypeus
            { "con", "$Codex_Ent_Conchas_Genus_Name;"}, // Concha
            { "ele", "$Codex_Ent_Electricae_Genus_Name;"}, // Electricae
            { "fon", "$Codex_Ent_Fonticulus_Genus_Name;"}, // Fonticulua
            { "fru", "$Codex_Ent_Shrubs_Genus_Name;"}, // Frutexa
            { "fum", "$Codex_Ent_Fumerolas_Genus_Name;"}, // Fumerola
            { "fun", "$Codex_Ent_Fungoids_Genus_Name;"}, // Fungoida
            { "oss", "$Codex_Ent_Osseus_Genus_Name;"}, // Osseus
            { "rec", "$Codex_Ent_Recepta_Genus_Name;"}, // Recepta
            { "str", "$Codex_Ent_Stratum_Genus_Name;"}, // Stratum
            { "tub", "$Codex_Ent_Tubus_Genus_Name;"}, // Tubus
            { "tus", "$Codex_Ent_Tussocks_Genus_Name;" }, // Tussock
            // From Horizons
            { "amp", "$Codex_Ent_Vents_Name;"}, // Amphora Plant
            { "ane", "$Codex_Ent_Sphere_Name;"}, // Anemone
            { "bar", "$Codex_Ent_Cone_Name;"}, // Bark Mounds
            { "bra", "$Codex_Ent_Brancae_Name;"}, // Brain Tree
            { "cry", "$Codex_Ent_Ground_Struct_Ice_Name;"}, // Crystalline Shards
            { "sin", "$Codex_Ent_Tube_Name;"}, // Sinuous Tubers
        };

        public static Dictionary<string, string> genusNames = new Dictionary<string, string>()
        {
            // From Odyssey
            { "$Codex_Ent_Aleoids_Genus_Name;",            "Aleoida" },
            { "$Codex_Ent_Bacterial_Genus_Name;",          "Bacterium" },
            { "$Codex_Ent_Cactoid_Genus_Name;",            "Cactoida" },
            { "$Codex_Ent_Clypeus_Genus_Name;",            "Clypeus" },
            { "$Codex_Ent_Conchas_Genus_Name;",            "Concha" },
            { "$Codex_Ent_Electricae_Genus_Name;",         "Electricae" },
            { "$Codex_Ent_Fonticulus_Genus_Name;",         "Fonticulua" },
            { "$Codex_Ent_Shrubs_Genus_Name;",             "Frutexa" },
            { "$Codex_Ent_Fumerolas_Genus_Name;",          "Fumerola" },
            { "$Codex_Ent_Fungoids_Genus_Name;",           "Fungoida" },
            { "$Codex_Ent_Osseus_Genus_Name;",             "Osseus" },
            { "$Codex_Ent_Recepta_Genus_Name;",            "Recepta" },
            { "$Codex_Ent_Stratum_Genus_Name;",            "Stratum" },
            { "$Codex_Ent_Tubus_Genus_Name;",              "Tubus" },
            { "$Codex_Ent_Tussocks_Genus_Name;",           "Tussock" },
            // From Horizons
            { "$Codex_Ent_Vents_Name;",                    "Amphora Plant"},
            { "$Codex_Ent_Sphere_Name;",                   "Anemone"},
            { "$Codex_Ent_Cone_Name;",                     "Bark Mounds"},
            { "$Codex_Ent_Brancae_Name;",                  "Brain Tree" },
            { "$Codex_Ent_Ground_Struct_Ice_Name;",        "Crystalline Shards"},
            { "$Codex_Ent_Tube_Name;",                     "Sinuous Tubers"},
        };

        public enum Status
        {
            Complete,
            Active,
            Abandoned,
            Died,
        }
    }

    class OrganicSummary
    {
        public string? genus;
        public string? genusLocalized;
        public string? species;
        public string? speciesLocalized;
        public string? variant;
        public string? variantLocalized;
        public long reward;
        public int range;
        public bool analyzed;
    }

    class ScannedOrganic
    {
        public string? genus;
        public string? genusLocalized;
        public string? species;
        public string? speciesLocalized;
        public long reward;
        public string system;
        public long systemAddress;
        public int bodyId;
        public string bodyName;
    }

    class PotentialOrganism
    {
        public string genus;
        public string speciesPart;
        public List<string>? planetClass;
        public double maxGravity;
        public List<string>? atmosphereComposition;
        public List<string>? atmosphereType;
        public List<string>? volcanism;
        public bool noVolcanism;
        public double minTemperature;
        public double maxTemperature;
        public List<string>? galacticRegion;
        public List<string>? starClass;
        public List<string>? material;

        public static List<string> match(SystemBody body, string? genus)
        {
            var potentials = new HashSet<string>();

            var parentStarTypes = body.system.getParentStarTypes(body, true); //.Take(1).ToList();
            Game.log($"Body: {body.name} => parentStarClass: " + string.Join(',', parentStarTypes));

            //var atmosSphere = body.atmosphereType?.Replace("thin ", "", StringComparison.OrdinalIgnoreCase) ?? ""; // too crude?
            //var atmosphere = body.atmosphereComposition.Keys;

            var galacticRegion = GalacticRegions.current.Replace(" ", "");
            var surfacePressure = body.surfacePressure / 10_000f;
            var surfaceGravity = body.surfaceGravity / 10;

            //if (body.name.Contains("3 e")) Debugger.Break();


            foreach (var foo in stuff)
            {
                if (genus != null && genus != foo.genus) continue;
                //if (foo.speciesPart.Contains("Flam")) Debugger.Break();

                //if (body.name.Contains("AB 1 c")) Debugger.Break();
                //if (body.name.Contains("5 c") && foo.speciesPart.Contains("Pennata")) Debugger.Break();

                if (foo.galacticRegion?.Contains(galacticRegion) == false) continue;
                if (foo.planetClass?.Any(pc => body.planetClass?.Contains(pc) == true) == false) continue;
                if (foo.maxGravity > 0 && surfaceGravity > foo.maxGravity) continue;

                if (body.atmosphereType == "None" && foo.atmosphereType?.FirstOrDefault() != "None") continue;
                if (foo.atmosphereType?.Count > 0 && foo.atmosphereType?.Contains(body.atmosphereType) == false) continue;
                if (foo.atmosphereComposition?.Count > 0 && !foo.atmosphereComposition.Any(_ => body.atmosphereComposition.ContainsKey(_))) continue;

                //var volcanism = body.volcanism?.Replace("major", "").Replace("minor", "").Replace(" volcanism", "") ?? "";
                if (string.IsNullOrEmpty(body.volcanism) || body.volcanism == "No volcanism")
                {
                    // body has no volcanism
                    if (foo.volcanism != null) continue;
                    // !foo.noVolcanism && 
                }
                else
                {
                    // body has volcanism
                    if (foo.noVolcanism || foo.volcanism?.Any(_ => body.volcanism?.Contains(_, StringComparison.OrdinalIgnoreCase) == true) == false) continue;
                }
                //if (foo.noVolcanism && !string.IsNullOrEmpty(body.volcanism) && body.volcanism != "No volcanism") continue;
                //if (string.IsNullOrEmpty(body.volcanism) && foo.volcanism?.Any(_ => body.volcanism?.Contains(_, StringComparison.OrdinalIgnoreCase) == true) == false) continue;

                if (foo.minTemperature > 0 && body.surfaceTemperature < foo.minTemperature) continue;
                if (foo.maxTemperature > 0 && body.surfaceTemperature > foo.maxTemperature) continue;

                if (foo.starClass != null && !foo.starClass.Any(_ => parentStarTypes.Contains(_)))
                {
                    //parentStarTypes = systemData.getParentStarTypes(body, true).ToList();

                    continue;
                }

                // material must be present and more than 0.0001
                if (foo.material != null && !foo.material.Any(_ => body.materials?.ContainsKey(_) == true && body.materials.GetValueOrDefault(_) > 0.0001)) continue;

                // special cases
                if (foo.speciesPart == "Electricae Radialem" && GalacticNeblulae.distToClosest(body.system.starPos) > 100) continue;
                if (foo.speciesPart == "Clypeus Speculumi" && body.distanceFromArrivalLS < 2000) continue; // not 2500 ?
                if (foo.speciesPart == "Bacterium Tela" && surfacePressure > 0.1) continue;
                if (foo.speciesPart == "Bacterium Tela" && body.atmosphere.Contains("thin ammonia") && surfaceGravity > 0.27f) continue;


                // atmospheric special cases
                if (foo.speciesPart.StartsWith("Recepta") && body.atmosphereComposition.GetValueOrDefault("SulphurDioxide") < 1f) continue;
                if (foo.speciesPart.StartsWith("Tussock Cultro") && body.atmosphereComposition.GetValueOrDefault("Ammonia") != 100f) continue;
                // Tussock may be wrong ?!
                //if (foo.speciesPart.StartsWith("Tussock") && body.atmosphereComposition.GetValueOrDefault("SulphurDioxide") < 0.9f) continue;

                var theseSpecies = new List<string>()
                {
                    "Frutexa Acus",
                    "Fungoida Stabitis", // This needs to allow for Water and Ammonia
                    "Stratum Excutitus",
                    // "Stratum Limaxus" ??
                    "Cactoida Cortexum",
                };
                if (theseSpecies.Any(_ => foo.speciesPart.StartsWith(_)))
                {
                    var sulphurDioxide = body.atmosphereComposition.GetValueOrDefault("SulphurDioxide");
                    var carbonDioxide = body.atmosphereComposition.GetValueOrDefault("CarbonDioxide");
                    if (sulphurDioxide < 0.99f && carbonDioxide != 100) continue;
                }

                // TODO: add distance check for Brain Tree's
                if (foo.speciesPart.Contains("Brain")) continue;

                //if (foo.speciesPart.Contains("Tussock")) Debugger.Break();
                //if (body.name.Contains("AB 1 c")) Debugger.Break();
                potentials.Add(foo.speciesPart);
            }

            return potentials.ToList();
        }

        public static implicit operator PotentialOrganism(string bigString)
        {
            var parts = bigString.Split(" | ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // expand some shorted names
            parts[2] = parts[2]
                .Replace("Icy", "Icy body")
                .Replace("HMC", "High metal content")
                .Replace("MetalRich", "Metal-rich body")
                .Replace("RockyIce", "Rocky ice world")
                .Replace("Rocky", "Rocky body")
                .Replace("Rocky body ice world", "Rocky ice")
            ;

            var foo = new PotentialOrganism()
            {
                genus = $"$Codex_Ent_{parts[0]}_Genus_Name;",
                speciesPart = parts[1],
                planetClass = parts[2] == "*" ? null : parts[2].Split(",").ToList(),
                maxGravity = parts[3] == "*" ? 0 : double.Parse(parts[3], CultureInfo.InvariantCulture),
                atmosphereType = parts[4] == "*" ? null : parts[4].Split(",").Where(_ => !_.StartsWith("~")).ToList(),
            };
            foo.atmosphereComposition = parts[4] == "*" ? null : parts[4].Split(",").Where(_ => _.StartsWith("~")).ToList();

            if (parts.Length > 5)
            {
                if (parts[5] == "None")
                    foo.noVolcanism = true;
                else if (parts[5] != "*")
                    foo.volcanism = parts[5].Split(",").ToList();
            }

            if (parts.Length > 6 && parts[6] != "*")
                foo.minTemperature = double.Parse(parts[6], CultureInfo.InvariantCulture);

            if (parts.Length > 7 && parts[7] != "*")
                foo.maxTemperature = double.Parse(parts[7], CultureInfo.InvariantCulture);

            if (parts.Length > 8 && parts[8] != "*")
                foo.starClass = parts[8].Split(",").ToList();

            if (parts.Length > 9 && parts[9] != "*")
            {
                parts[9] = parts[9]
                    .Replace("ant+", "antimony,polonium,ruthenium,technetium,tellurium,yttrium")
                    .Replace("cad+", "cadmium,mercury,molybdenum,niobium,tin,tungsten");
                foo.material = parts[9].Split(",").ToList();
            }

            if (parts.Length > 10 && parts[10] != "*")
            {
                foo.galacticRegion = new List<string>();

                foreach (var region in parts[10].Split(","))
                {
                    if (region.StartsWith("!~"))
                    {
                        var excludeRegions = GalacticRegions.mapArms[region.Substring(1)];
                        var includeRegions = GalacticRegions.mapRegions.Values.Where(_ => !excludeRegions.Contains(_.Replace(" ", "")));
                        foo.galacticRegion.AddRange(includeRegions.Select(_ => _.Replace(" ", "")));

                    }
                    else if (region.StartsWith("~"))
                    {
                        foo.galacticRegion.AddRange(GalacticRegions.mapArms[region].Split(','));
                    }
                    else
                    {
                        foo.galacticRegion.Add(region);
                    }
                }
            }

            return foo;
        }

        public static List<PotentialOrganism> stuff = new List<PotentialOrganism>()
        {
            /*
             * Matching columns are:
             *  1. Genus name part
             *  2. Species name
             *  3. List of valid body types
             *  4. Max gravity
             *  5. List of atmosphere types, or "None", or ?prefix with ~ for atmosphereComposition matches?
             *  6. List of volcanisms
             *  7. Min surface temperature
             *  8. Max surface temperature
             *  9. Parent star class
             * 10. Material
             * 11. Valid galactic regions
             * 
             * 
             * Data is mined from:
             * https://ed-dsn.net/en/conditions-of-emergence-of-exobiological-species-on-planets-a-atmosphere-fine/
             * https://docs.google.com/spreadsheets/d/1Q2ydo7R2ttiTo6_DepAM17HIBxyLLvf-x_HAY1VSGMY/
             */

            // 5x Aleoida - https://canonn.science/codex/aleoida-2/ - https://ed-dsn.net/en/aleoida_en/
            //   genus | species           | body      | <g  | atmosphereType| volc | >t  | <t  | star type | material | galactic regions
            { "Aleoids | Aleoida Arcus     | HMC,Rocky | 2.7 | CarbonDioxide | None | 175 | 180 | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Coronamus | HMC,Rocky | 2.7 | CarbonDioxide | None | 180 | 190 | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Gravis    | HMC,Rocky | 2.7 | CarbonDioxide | None | 190 | 196 | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Laminiae  | HMC,Rocky | 2.7 | Ammonia       | *    | 152 | 177 | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Spica     | HMC,Rocky | 2.7 | Ammonia       | *    | 170 | 177 | B,A,F,K,M,L,T,TTS,Y,W,D,N" }, // TODO NOT Sagittarius-Carina Arm

            // 13x Bacterium - https://canonn.science/codex/bacteria-2/ - https://ed-dsn.net/en/bacterium_en/
            //     genus | species             | body               | <g  | atmosphereType | volcanism             | >t  | <t  |sta| mats | galactic regions
            { "Bacterial | Bacterium Acies     | *                  | *   | Neon           | *                     |  20 |  61 | * | ant+" },
            { "Bacterial | Bacterium Alcyoneum | HMC,Rocky          | *   | Ammonia        | *                     | 152 | 177 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Aurasus   | HMC,Rocky          | *   | CarbonDioxide  | *                     | 145 | 400 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Bullaris  | *                  | *   | Methane        | *                     |  67 | 109 | * | ant+" },
            { "Bacterial | Bacterium Bullaris  | HMC,Rocky          | *   | MethaneRich    | *                     |  73 | 141 | * | ant+" },
            { "Bacterial | Bacterium Cerbrus   | HMC,Rocky,RockyIce | *   | SulphurDioxide | *                     | 132 | 500 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Cerbrus   | HMC,Rocky          | *   | Water          | *                     | 392 | 452 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Cerbrus   | RockyIce           | *   | WaterRich      | None                  | 231 | 315 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Informem  | *                  | *   | Nitrogen       | *                     |  43 | 150 | * | ant+" },
            { "Bacterial | Bacterium Nebulus   | Icy                | *   | Helium         | *                     |  19 |  21 | * | ant+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Argon          | Nitrogen,Ammonia      |  50 | 172 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | ArgonRich      | Nitrogen,Ammonia      |  80 |  87 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Helium         | Nitrogen,Ammonia      |  20 |  21 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Methane        | Nitrogen,Ammonia      |  84 | 108 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Neon           | Nitrogen,Ammonia      |  20 |  61 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | NeonRich       | Nitrogen,Ammonia      |  20 |  93 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Nitrogen       | Nitrogen,Ammonia      |  60 |  64 | * | cad+" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | WaterRich      | Nitrogen,Ammonia      | 240 | 307 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy,RockyIce       | 2.5 | Argon          | CarbonDioxide,Methane |  57 | 146 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 5.1 | Helium         | Methane               |  57 | 146 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 0.5 | Methane        | Methane               |  84 | 108 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy,RockyIce       | 6.0 | Neon           | CarbonDioxide,Methane |  20 |  52 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 6.1 | NeonRich       | CarbonDioxide,Methane |  20 |  65 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 2.6 | Nitrogen       | CarbonDioxide,Methane |  61 |  68 | * | cad+" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 3.2 | Oxygen         | CarbonDioxide,Methane | 152 | 210 | * | cad+" },
            //{ "Bacterial | Bacterium Tela      | *                  | 6.1 | *              | *                     |  20 | 607 | * | cad+" }, // todo? atmos: all but water.
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | SulphurDioxide | *                     |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | CarbonDioxide  | *                     |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Water          | *                     |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Ammonia        | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Argon          | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Helium         | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Methane        | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Neon,NeonRich  | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Methane        | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Nitrogen       | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | Oxygen         | None                  |  20 | 607 | * | cad+" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | WaterRich      | None                  |  20 | 607 | * | cad+" },
                                                                                    // These have another criteria that is not at all obvious.
                                                                                    // Maybe Thin Ammonia has mas gravity of 0.27? - probably not?
                                                                                    // OR Thin Neon or Ammonia MUST have Volcanism
                                                                                    // OR rather: SulphurDioxide and CarbonDioxide ThinWater allow No Volcanism <<---
            { "Bacterial | Bacterium Verrata   | Icy,Rocky,RockyIce | 6.1 | *              | Water                 |  20 | 442 | * | cad+" }, // todo? atmos: all but SulphurDioxide
            { "Bacterial | Bacterium Vesicula  | *                  | 5.1 | Argon          | *                     |  50 | 267 | * | cad+" },
            { "Bacterial | Bacterium Volu      | *                  | 6.0 | Oxygen         | *                     | 143 | 246 | * | ant+" },

            // 5x Cactoida - https://canonn.science/codex/cactoida/ - https://ed-dsn.net/en/cactoida_en/
            //   genus | species             | body      | <g  | atmosType      | vol  | >t  | <t  | star type         |mts| galactic regions
            { "Cactoid | Cactoida Cortexum   | Rocky,HMC | 2.7 | CarbonDioxide  | None | 180 | 196 | O,A,F,G,M,L,T,TTS,Y,W,D,N | * | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Cactoid | Cactoida Lapis      | Rocky,HMC | 2.8 | Ammonia        | *    | 160 | 187 | O,A,F,G,M,L,T,TTS,Y,W,D,N | * | ~OrionCygnusArm,~SagittariusCarinaArm,Odin'sHold,GalacticCentre" },
            { "Cactoid | Cactoida Peperatis  | Rocky,HMC | 2.8 | Ammonia        | *    | 160 | 186 | O,A,F,G,M,L,T,TTS,Y,W,D,N | * | ~ScutumCentaurusArm,Odin'sHold,GalacticCenter,Orion-CygnusArm" },
            { "Cactoid | Cactoida Pullulanta | Rocky,HMC | 2.7 | CarbonDioxide  | None | 180 | 196 | O,A,F,G,M,L,T,TTS,Y,W,D,N | * | ~PerseusArm,Ryker'sHope,GalacticCentre" },
            { "Cactoid | Cactoida Vermis     | Rocky     | 2.7 | SulphurDioxide | *    | 160 | 207 | O,A,F,G,M,L,T,TTS,Y,W,D,N | * | *" },
            { "Cactoid | Cactoida Vermis     | Rocky,HMC | 2.7 | Water          | None | 390 | 450 | O,A,F,G,M,L,T,TTS,Y,W,D,N | * | *" },

            // 3x Clypeus - https://canonn.science/codex/Clypeus/ - https://ed-dsn.net/en/Clypeus_en/
            //   genus | species            | body  | <g  | atmosType     | vol  | >t  | <t  | star type           |mts| galactic regions
            { "Clypeus | Clypeus Lacrimam   | Rocky | 2.4 | CarbonDioxide | None | 190 | 199 | B,A,F,G,K,M,L,T,D,N" },
            { "Clypeus | Clypeus Lacrimam   | Rocky | 0.6 | Water         | *    | 394 | 452 | B,A,F,G,K,M,L,T,D,N" },
            { "Clypeus | Clypeus Margaritus | HMC   | 2.7 | CarbonDioxide | None | 190 | 196 | B,A,F,G,K,M,L,T,D,N" },
            { "Clypeus | Clypeus Margaritus | HMC   | 0.6 | Water         | None | 390 | 451 | B,A,F,G,K,M,L,T,D,N" },
            // Clypeus Speculumi has special case for ArrivalDistance > 2500 
            { "Clypeus | Clypeus Speculumi  | Rocky | 2.4 | CarbonDioxide | None | 190 | 196 | B,A,F,G,K,M,L,T,D,N" },
            { "Clypeus | Clypeus Speculumi  | Rocky | 0.63 | Water         | *    | 392 | 452 | B,A,F,G,K,M,L,T,D,N" },

            // 4x Concha - https://canonn.science/codex/Concha/ - https://ed-dsn.net/en/Concha_en/
            //  genus  | species           | body      | <g  | atmosType     | volcanism| >t  | <t  | star type |mats| galactic regions
            { "Conchas | Concha Aureolas   | HMC,Rocky | 2.7 | Ammonia       | *        | 152 | 177 | B,A,F,G,K,L,Y,W,D,N" },
            { "Conchas | Concha Biconcavis | HMC,Rocky | 2.7 | Nitrogen      | None     |  42 | 51  | * | ant+" },
            { "Conchas | Concha Labiata    | HMC,Rocky | 2.6 | CarbonDioxide | None     | 150 | 199 | B,A,F,G,K,L,Y,W,D,N" },
            { "Conchas | Concha Renibus    | HMC,Rocky | 2.7 | Ammonia       | Silicate | 163 | 177 | * | cad+" },
            { "Conchas | Concha Renibus    | HMC,Rocky | 2.7 | CarbonDioxide | None     | 180 | 196 | * | cad+" },
            { "Conchas | Concha Renibus    | HMC,Rocky | 2.7 | Methane       | Silicate |  79 | 102 | * | cad+" },
            { "Conchas | Concha Renibus    | HMC,Rocky | 2.7 | Water         | *        | 390 | 452 | * | cad+" },

            // 2x Electricae - https://canonn.science/codex/Electricae/ - https://ed-dsn.net/en/Electricae_en/
            //      genus | species             | bod | <g  | atmosType |vol| >t  | <t  | star  | mats | galactic regions
            { "Electricae | Electricae Pluma    | Icy | 2.7 | Argon     | * |  50 | 150 | A,D,N | ant+" }, // TODO: lum V+ ???
            { "Electricae | Electricae Pluma    | Icy | 2.7 | ArgonRich | * |  65 | 121 | A,D,N | ant+" }, // TODO: lum V+ ???
            { "Electricae | Electricae Pluma    | Icy | 2.7 | Neon      | * |  20 |  41 | A,D,N | ant+" }, // TODO: lum V+ ???
            { "Electricae | Electricae Pluma    | Icy | 2.7 | NeonRich  | * |  61 |  69 | A,D,N | ant+" }, // TODO: lum V+ ???
            // Electricae Radialem has special case for nearest Nebula
            { "Electricae | Electricae Radialem | Icy | 2.8 | Argon     | * |  50 | 149 | *     | ant+" },
            { "Electricae | Electricae Radialem | Icy | 2.7 | ArgonRich | * |  61 | 110 | *     | ant+" },
            { "Electricae | Electricae Radialem | Icy | 2.7 | NeonRich  | * |  61 |  68 | *     | ant+" },

            // 6x Fonticula - https://canonn.science/codex/Fonticulua/ - https://ed-dsn.net/en/Fonticulua_en/
            //      genus | species                | body type    | <g  | atmosType |vol| >t  | <t  | star  | mats | galactic regions
            { "Fonticulus | Fonticulua Campestris  | Icy,RockyIce | 2.8 | Argon     | * |  50 | 150 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Digitos     | Icy,RockyIce | 0.6 | Methane   | * |  83 | 109 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Fluctus     | Icy          | 2.7 | Oxygen    | * | 143 | 198 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Lapida      | Icy,RockyIce | 2.8 | Nitrogen  | * |  50 |  81 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Segmentatus | Icy          | 2.7 | Neon      | * |  50 |  57 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Segmentatus | Icy          | 2.8 | NeonRich  | * |  58 |  75 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Upupam      | Icy,RockyIce | 2.8 | ArgonRich | * |  60 | 121 | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },

            // 7x Frutexa - https://canonn.science/codex/Frutexa/ - https://ed-dsn.net/en/fruxeta_en/
            //  genus | species            | body      | <g   | atmosType      | volc | >t  | <t  | star  | mats | galactic regions
            { "Shrubs | Frutexa Acus       | Rocky     | 2.4  | CarbonDioxide  | None | 146 | 196 | O,B,F,G,M,L,TTS,W,D,N | * | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Shrubs | Frutexa Collum     | HMC,Rocky | 2.8  | SulphurDioxide | *    | 132 | 167 | O,B,F,G,M,L,TTS,W,D,N" },
            { "Shrubs | Frutexa Fera       | Rocky     | 2.4  | CarbonDioxide  | None | 146 | 196 | O,B,F,G,M,L,TTS,W,D,N | * | ~OuterArm,EmpyreanStraits,GalacticCentre" },
            { "Shrubs | Frutexa Flabellum  | Rocky     | 2.7  | Ammonia        | *    | 152 | 177 | O,B,F,G,M,L,TTS,W,D,N | * | !~ScutumCentaurusArm" },
            { "Shrubs | Frutexa Flammasis  | Rocky     | 2.7  | Ammonia        | *    | 152 | 186 | O,B,F,G,M,L,TTS,W,D,N | * | ~ScutumCentaurusArm,Odin'sHold,GalacticCenter,Orion-CygnusArm,InnerOrion-PerseusConflux" },
            { "Shrubs | Frutexa Metallicum | HMC       | 2.8  | Ammonia        | None | 152 | 176 | O,B,F,G,M,L,TTS,W,D,N" },
            { "Shrubs | Frutexa Metallicum | HMC       | 2.7  | CarbonDioxide  | None | 146 | 196 | O,B,F,G,M,L,TTS,W,D,N" },
            { "Shrubs | Frutexa Metallicum | HMC       | 0.52 | Water          | None | 390 | 400 | O,B,F,G,M,L,TTS,W,D,N" },
            { "Shrubs | Frutexa Sponsae    | Rocky     | 0.6  | Water          | *    | 392 | 452 | O,B,F,G,M,L,TTS,W,D,N" },

            // 4x Fumerola - https://canonn.science/codex/Fumerola/ - https://ed-dsn.net/en/Fumerola_en/
            //     genus | species           | body               | <g  | atmosType      | volc                    | >t  | <t  |sta| mats | galactic regions
            { "Fumerolas | Fumerola Aquatis  | Icy,Rocky,RockyIce | 2.7 | *              | Water                   |  20 | 447 | * | cad+" }, // TODO atmos: all but Helium
            { "Fumerolas | Fumerola Carbosis | Icy,RockyIce       | 0.5 | Argon          | CarbonDioxide,Methane   |  56 | 147 | * | cad+" },
            { "Fumerolas | Fumerola Carbosis | Icy                | 2.7 | Methane        | MethaneMagma            |  84 | 109 | * | cad+" },
            { "Fumerolas | Fumerola Carbosis | Icy                | 2.6 | Neon           | CarbonDioxide,Methane   |  40 |  53 | * | cad+" },
            { "Fumerolas | Fumerola Carbosis | Icy                | 2.7 | Nitrogen       | CarbonDioxide,Methane   |  57 |  75 | * | cad+" },
            { "Fumerolas | Fumerola Carbosis | Icy                | 2.7 | Oxygen         | CarbonDioxideGeysers    | 164 | 177 | * | cad+" },
            { "Fumerolas | Fumerola Carbosis | Icy,RockyIce       | 2.7 | SulphurDioxide | CarbonDioxide,Methane   | 149 | 272 | * | cad+" },
            { "Fumerolas | Fumerola Extremus | HMC,Rocky,RockyIce | 0.8 | Ammonia        | Metallic,Rocky,Silicate | 161 | 177 | * | cad+" },
            { "Fumerolas | Fumerola Extremus | Rocky,RockyIce     | 2.7 | Argon          | Metallic,Rocky,Silicate |  54 | 121 | * | cad+" },
            { "Fumerolas | Fumerola Extremus | HMC,Rocky          | 1.3 | Methane        | Metallic,Silicate       |  77 | 109 | * | cad+" },
            { "Fumerolas | Fumerola Extremus | Rocky,RockyIce     | 2.7 | SulphurDioxide | Metallic,Rocky,Silicate | 147 | 204 | * | cad+" },
            { "Fumerolas | Fumerola Nitris   | Icy                | 2.7 | Argon          | Ammonia,Nitrogen        |  50 | 141 | * | cad+" },
            { "Fumerolas | Fumerola Nitris   | Icy                | 2.4 | ArgonRich      | NitrogenMagma           |  81 |  85 | * | cad+" },
            { "Fumerolas | Fumerola Nitris   | Icy                | 0.5 | Methane        | NitrogenMagma           |  83 | 109 | * | cad+" },
            { "Fumerolas | Fumerola Nitris   | Icy                | 2.7 | NeonRich       | NitrogenMagma           |  60 |  67 | * | cad+" },
            { "Fumerolas | Fumerola Nitris   | Icy                | 2.7 | Nitrogen       | Ammonia,Nitrogen        |  60 |  81 | * | cad+" },
            { "Fumerolas | Fumerola Nitris   | Icy                | 2.7 | SulphurDioxide | Ammonia,Nitrogen        | 161 | 249 | * | cad+" },

            // 4x Fungoida - https://canonn.science/codex/Fungoida/ - https://ed-dsn.net/en/fungoida_en/
            //    genus | species           | body               | <g  | atmosType     | volc     | >t  | <t  |sta| mats | galactic regions
            { "Fungoids | Fungoida Bullarum | HMC,Rocky,RockyIce | 2.8 | Argon         | *        |  50 | 132 | * | ant+" },
            { "Fungoids | Fungoida Bullarum | HMC,Rocky,RockyIce | 2.7 | Nitrogen      | None     |  50 |  70 | * | ant+" },
            { "Fungoids | Fungoida Gelata   | Rocky,RockyIce     | 0.7 | Ammonia       | *        | 161 | 177 | * | cad+" },
            { "Fungoids | Fungoida Gelata   | HMC,Rocky          | 2.7 | CarbonDioxide | None     | 180 | 199 | * | cad+" },
            { "Fungoids | Fungoida Gelata   | HMC                | 1.2 | Methane       | *        |  79 | 107 | * | cad+" },
            { "Fungoids | Fungoida Gelata   | HMC,Rocky          | 0.63 | Water         | None     | 390 | 453 | * | cad+" },
            { "Fungoids | Fungoida Setisis  | HMC,Rocky,RockyIce | 2.7 | Ammonia       | *        | 152 | 177 | * | ant+" },
            { "Fungoids | Fungoida Setisis  | HMC,Rocky,RockyIce | 2.7 | Methane       | *        |  67 | 109 | * | ant+" },
            { "Fungoids | Fungoida Stabitis | Rocky,RockyIce     | 0.5 | Ammonia       | *        | 158 | 177 | * | cad+ | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Fungoids | Fungoida Stabitis | HMC,Rocky          | 2.7 | CarbonDioxide | None     | 180 | 196 | * | cad+ | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Fungoids | Fungoida Stabitis | HMC                | 1.3 | Methane       | Silicate |  78 | 109 | * | cad+ | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Fungoids | Fungoida Stabitis | HMC,Rocky          | 4.84 | Water         | None     | 392 | 452 | * | cad+ | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },

            // 6x Osseus - https://canonn.science/codex/Osseus/ - https://ed-dsn.net/en/Osseus_en/
            //  genus | species            | body               | <g  | atmosType     | volc | >t  | <t  | star class        |mat| galactic regions
            { "Osseus | Osseus Cornibus    | HMC,Rocky          | 2.7 | CarbonDioxide | *    | 180 | 196 | O,A,F,G,K,T,TTS,Y | * | ~PerseusArm,Ryker'sHope,GalacticCentre" },
            { "Osseus | Osseus Discus      | HMC,Rocky,RockyIce | 0.9 | Ammonia       | *    | 161 | 177 | *                 | cad+" },
            { "Osseus | Osseus Discus      | RockyIce           | 2.7 | Argon         | *    |  68 | 119 | *                 | cad+" },
            { "Osseus | Osseus Discus      | Rocky              | 1.3 | Methane       | *    |  78 | 109 | *                 | cad+" },
            { "Osseus | Osseus Discus      | HMC,Rocky          | 2.7 | Water         | *    | 390 | 452 | *                 | cad+" },
            { "Osseus | Osseus Fractus     | HMC,Rocky          | 2.7 | CarbonDioxide | *    | 180 | 190 | O,A,F,G,K,T,TTS,Y | * | !~PerseusArm,Odin'sHold,EmpyreanStraits,InnerScutum-CentaurusArm" },
            { "Osseus | Osseus Pellebantus | HMC,Rocky          | 2.7 | CarbonDioxide | None | 191 | 197 | O,A,F,G,K,T,TTS,Y | * | !~PerseusArm,Odin'sHold,EmpyreanStraits,InnerScutum-CentaurusArm" },
            { "Osseus | Osseus Pumice      | HMC,Rocky,RockyIce | 2.8 | Argon         | *    |  50 | 134 | *                 | ant+" },
            { "Osseus | Osseus Pumice      | RockyIce           | 2.8 | ArgonRich     | None |  61 |  79 | *                 | ant+" },
            { "Osseus | Osseus Pumice      | HMC,Rocky,RockyIce | 2.7 | Methane       | *    |  67 | 109 | *                 | ant+" },
            { "Osseus | Osseus Pumice      | HMC,Rocky,RockyIce | 2.7 | Nitrogen      | None |  42 |  71 | *                 | ant+" },
            { "Osseus | Osseus Spiralis    | HMC,Rocky,RockyIce | 2.8 | Ammonia       | *    | 160 | 177 | O,A,F,G,K,T,TTS,Y" },

            // 3x Recepta - https://canonn.science/codex/Recepta/ - https://ed-dsn.net/en/recepta_en/
            //   genus | species               | body          | <g  | atmosType      | volc | >t  | <t  |sta| mats | galactic regions
            { "Recepta | Recepta Conditivus    | Icy,Rocky     | 2.7 | CarbonDioxide  | None | 150 | 194 | * | ant+" },
            { "Recepta | Recepta Conditivus    | Icy           | 2.7 | Oxygen         | *    | 154 | 170 | * | ant+" },
            { "Recepta | Recepta Conditivus    | *             | 2.8 | SulphurDioxide | *    | 132 | 272 | * | ant+" },
            { "Recepta | Recepta Deltahedronix | HMC,Rocky,Icy | 2.7 | CarbonDioxide  | *    | 150 | 195 | * | cad+" },
            { "Recepta | Recepta Deltahedronix | *             | 2.8 | SulphurDioxide | *    | 132 | 272 | * | cad+" },
            { "Recepta | Recepta Umbrux        | *             | 2.8 | SulphurDioxide | *    | 132 | 273 | B,A,F,G,K,M,L,T,TTS,Ae,Y,D" },
            { "Recepta | Recepta Umbrux        | *             | 2.8 | CarbonDioxide  | *    | 132 | 273 | B,A,F,G,K,M,L,T,TTS,Ae,Y,D" }, // EvilHorse
            // Recepta needs SulphurDioxide >= 0.9 in Atmospheric composition

            // 8x Stratum - https://canonn.science/codex/stratum/ and https://ed-dsn.net/en/stratum_en/
            //   genus | species            | body  | <g  | atmosType         | volc | >t  | <t  | star type              |mat| galactic regions
            { "Stratum | Stratum Araneamus  | Rocky | 5.4 | SulphurDioxide    | *    | 165 | 373 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Cucumisis  | Rocky | 5.9 | CarbonDioxide     | None | 191 | 371 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~SagittariusCarinaArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Cucumisis  | Rocky | 5.6 | CarbonDioxideRich | *    | 121 | 246 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~SagittariusCarinaArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Cucumisis  | Rocky | 4.6 | Oxygen            | *    | 165 | 190 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~SagittariusCarinaArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Cucumisis  | Rocky | 4.6 | Oxygen?           | *    | 200 | 245 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~SagittariusCarinaArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Cucumisis  | Rocky | 5.8 | SulphurDioxide    | *    | 191 | 373 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~SagittariusCarinaArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Excutitus  | Rocky | 4.7 | CarbonDioxide     | None | 165 | 190 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Excutitus  | Rocky | 4.1 | SulphurDioxide    | *    | 165 | 190 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Frigus     | Rocky | 5.4 | CarbonDioxide     | *    | 191 | 368 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~PerseusArm,Ryker'sHope" },
            { "Stratum | Stratum Frigus     | Rocky | 5.5 | CarbonDioxideRich | None | 205 | 246 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~PerseusArm,Ryker'sHope" },
            { "Stratum | Stratum Frigus     | Rocky | 4.5 | Oxygen            | *    | 207 | 237 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~PerseusArm,Ryker'sHope" },
            { "Stratum | Stratum Frigus     | Rocky | 5.4 | SulphurDioxide    | *    | 191 | 371 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~PerseusArm,Ryker'sHope" },
            { "Stratum | Stratum Laminamus  | Rocky | 3.4 | Ammonia           | *    | 161 | 177 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~OrionCygnusArm,Odin'sHold,GalacticCentre" },
            { "Stratum | Stratum Limaxus    | Rocky | 3.9 | CarbonDioxide     | *    | 165 | 190 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~ScutumCentaurusArm,Orion-CygnusArm" },
            { "Stratum | Stratum Limaxus    | Rocky | 4.8 | Oxygen            | *    | 165 | 190 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~ScutumCentaurusArm,Orion-CygnusArm" },
            { "Stratum | Stratum Limaxus    | Rocky | 4.0 | SulphurDioxide    | *    | 165 | 190 | F,K,M,L,T,TTS,Ae,Y,W,D | * | ~ScutumCentaurusArm,Orion-CygnusArm" },
            { "Stratum | Stratum Paleas     | Rocky | 3.4 | Ammonia           | *    | 165 | 177 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Paleas     | Rocky | 5.8 | CarbonDioxide     | *    | 165 | 419 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Paleas     | Rocky | 5.9 | CarbonDioxideRich | *    | 186 | 257 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Paleas     | Rocky | 5.8 | Oxygen            | *    | 169 | 246 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Paleas     | Rocky | 0.6 | SulphurDioxide    | *    | 394 | 450 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            // { "Stratum | Stratum Paleas     | Rocky | 0.6 | Water | *    | 394 | 450 | F,K,M,L,T,TTS,Ae,Y,W,D" }, // EvilHorse
            { "Stratum | Stratum Tectonicas | HMC   | 3.7 | Ammonia           | *    | 165 | 177 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 5.2 | Argon             | *    | 173 | 182 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 5.6 | ArgonRich         | *    | 167 | 232 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 6.1 | CarbonDioxide     | *    | 165 | 430 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 6.1 | CarbonDioxideRich | *    | 165 | 260 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 5.1 | Oxygen            | *    | 165 | 246 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 6.1 | SulphurDioxide    | *    | 165 | 450 | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC   | 0.63 | Water             | *    | 392 | 450 | F,K,M,L,T,TTS,Ae,Y,W,D" },

            // 5x Tubus - https://canonn.science/codex/Tubus/ - https://ed-dsn.net/en/tubus_en/
            // genus | species          | body  | <g  | atmosType     | volc | >t  | <t  | star type                   |mats| galactic regions
            { "Tubus | Tubus Cavas      | Rocky | 1.5 | CarbonDioxide | None | 160 | 197 | O,B,A,F,G,K,M,L,T,TTS,W,D,N | * | ~ScutumCentaurusArm,Odin'sHold,GalacticCentre,,,EmpyreanStraits,NormaArm,InnerOrion-PerseusConflux,TempleElysianShore,SanguineousRim,Orion-CygnusArm" },
            { "Tubus | Tubus Compagibus | Rocky | 1.5 | CarbonDioxide | None | 160 | 197 | O,B,A,F,G,K,M,L,T,TTS,W,D,N | * | ~SagittariusCarinaArm,Odin'sHold,GalacticCentre,EmpyreanStraits,NormaArm,InnerOrion-PerseusConflux,TempleElysianShore,SanguineousRim,InnerScutum-CentaurusArm" },
            { "Tubus | Tubus Conifer    | Rocky | 1.5 | CarbonDioxide | None | 160 | 196 | O,B,A,F,G,K,M,L,T,TTS,W,D,N | * | ~PerseusArm,Ryker'sHope,GalacticCentre" },
            { "Tubus | Tubus Rosarium   | Rocky | 1.6 | Ammonia       | *    | 160 | 177 | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
            { "Tubus | Tubus Sororibus  | HMC   | 1.6 | Ammonia       | None | 160 | 177 | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
            { "Tubus | Tubus Sororibus  | HMC   | 1.5 | CarbonDioxide | None | 160 | 194 | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
                    /* All Region:
         * Acheron Achilles'sAltar Aquila'sHalo ArcadianStream Dryman'sPoint ElysianShore EmpyreanStraits ErrantMarches FormorianFrontier GalacticCentre Hawking'sGap HieronymusDelta InnerOrionSpur InnerOrion-PerseusConflux InnerScutum-CentaurusArm Izanami
         * Kepler'sCrest Lyra'sSong MareSomnia Newton'sVault NormaArm NormaExpanse Odin'sHold Orion-CygnusArm OuterArm OuterOrionSpur OuterOrion-PerseusConflux OuterScutum-CentaurusArm PerseusArm Ryker'sHope VulcanGate Sagittarius-CarinaArm SanguineousRim
         * Temple Xibalba Tenebrae TheAbyss TheConduit TheFormidineRift TheVeils TheVoid TrojanBelt
         */

            // 15x Tussock - https://canonn.science/codex/tussock/ - https://ed-dsn.net/en/tussock_en/
            //    genus | species           | body           | <g  | atmosType      | volc | >t  | <t  | star type          |mat| galactic regions
            { "Tussocks | Tussock Albata    | HMC,Rocky      | 2.7 | CarbonDioxide  | None | 175 | 180 | F,G,K,M,L,T,Y,W,D | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Capillum  | Rocky          | 2.8 | Argon          | *    |  80 | 129" },
            { "Tussocks | Tussock Capillum  | Rocky,RockyIce | 2.7 | Methane        | *    |  90 | 109" },
            { "Tussocks | Tussock Caputus   | HMC,Rocky      | 2.7 | CarbonDioxide  | None | 181 | 190 | F,G,K,M,L,T,Y,W,D | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Catena    | HMC,Rocky      | 2.7 | Ammonia        | *    | 152 | 177 | F,G,K,M,L,T,Y,W,D | * | ~ScutumCentaurusArm,Orion-CygnusArm" },
            { "Tussocks | Tussock Cultro    | HMC,Rocky      | 2.8 | Ammonia        | *    | 152 | 177 | F,G,K,M,L,T,Y,W,D | * | ~OrionCygnusArm,Odin'sHold,EmpyreanStraits,GalacticCentre" },
            { "Tussocks | Tussock Divisa    | HMC,Rocky      | 2.7 | Ammonia        | *    | 152 | 177 | F,G,K,M,L,T,Y,W,D | * | ~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Ignis     | HMC,Rocky      | 1.9 | CarbonDioxide  | None | 160 | 170 | F,G,K,M,L,T,Y,W,D | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Pennata   | HMC,Rocky      | 0.9 | CarbonDioxide  | None | 145 | 154 | F,G,K,M,L,T,Y,W,D,N | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Pennatis  | HMC,Rocky      | 2.7 | CarbonDioxide  | None | 147 | 196 | F,G,K,M,L,T,Y,W,D | * | ~OuterArm,EmpyreanStraits,GalacticCentre" },
            { "Tussocks | Tussock Propagito | HMC,Rocky      | 2.7 | CarbonDioxide  | None | 145 | 197 | F,G,K,M,L,T,Y,W,D | * | ~ScutumCentaurusArm,Odin'sHold,GalacticCentre,Orion-CygnusArm" },
            { "Tussocks | Tussock Serrati   | HMC,Rocky      | 2.3 | CarbonDioxide  | None | 171 | 178 | F,G,K,M,L,T,Y,W,D | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Stigmasis | HMC,Rocky      | 2.8 | SulphurDioxide | *    | 132 | 207" },
            { "Tussocks | Tussock Triticum  | HMC,Rocky      | 2.7 | CarbonDioxide  | None | 191 | 196 | F,G,K,M,L,T,Y,W,D | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Ventusa   | HMC,Rocky      | 1.6 | CarbonDioxide  | None | 155 | 188 | F,G,K,M,L,T,Y,W,D | * | ~SagittariusCarinaArm,~PerseusArm,Ryker'sHope" },
            { "Tussocks | Tussock Virgam    | HMC,Rocky      | 0.63 | Water          | *    | 390 | 450" },
            // Tussock needs SulphurDioxide >= 0.9 in Atmospheric composition

            // 1x Amphora plant
            // Has special case for systems containing: Earth-like Worlds, Gas Giants with Water-Based Life, and Water Giants
            // genus | species       | body               | <g  | atmosType      | volc                    | >t  | <t  |sta| mats | galactic regions
            { "Vents | Amphora Plant | Metal-rich | * | None | * | * | * | * | A" },

            // 8x Anemone
            // genus      | species                         | body               | <g  | atmosType      | volc                    | >t  | <t  |sta| mats | galactic regions
            { "Sphere     | Luteolum Anemone                | MetalRich,Rocky | * | None | * | * | * | * | A,B,O" },
            { "SphereABCD | Croceum Anemone                 | body | * | None | * | * | * | * | A,B,O" },
            { "SphereABCD | Puniceum Anemone                | body | * | None | * | * | * | * | A,B,O" },
            { "SphereABCD | Roseum Anemone                  | body | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Blatteum Bioluminescent Anemone | HMC,Metal-rich | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Rubeum Bioluminescent Anemone   | * | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Prasinum Bioluminescent Anemone | * | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Roseum Bioluminescent Anemone   | HMC,Metal-rich | * | None | * | * | * | * | A,B,O" },

            // 1x Bark Mounds
            // genus| species     |bod|<g | atm  |vol|>t |<t |sta| mats | galactic regions
            { "Cone | Bark Mounds | * | * | None | * | * | * | A" },

            // 8x Brain Trees TODO ...
            // genus| species                 |bod|<g | atm  |vol|>t |<t |sta| mats | galactic regions
            { "Seed | Roseum Brain Tree       | * | * | None | * | * | * | * | *" },
            { "Seed | Gypseeum Brain Tree     | * | * | None | * | * | * | * | *" },
            { "Seed | Ostrinum Brain Tree     | * | * | None | * | * | * | * | *" },
            { "Seed | Viride Brain Tree       | * | * | None | * | * | * | * | *" },
            { "Seed | Lividum Brain Tree      | * | * | None | * | * | * | * | *" },
            { "Seed | Aureum Brain Tree       | * | * | None | * | * | * | * | *" },
            { "Seed | Puniceum Brain Tree     | * | * | None | * | * | * | * | *" },
            { "Seed | Lindigoticum Brain Tree | * | * | None | * | * | * | * | *" },

            // 8x Tubers TODO ...
            // genus| species                     |bod|<g | atm  |vol|>t |<t |sta| mats | galactic regions
            { "Tube | Roseum Sinuous Tubers       | body | * | None | *" },
            { "Tube | Prasinum Sinuous Tubers     | body | * | None | *" },
            { "Tube | Albidum Sinuous Tubers      | body | * | None | *" },
            { "Tube | Caeruleum Sinuous Tubers    | body | * | None | *" },
            { "Tube | Blatteum Sinuous Tubers     | body | * | None | *" },
            { "Tube | Lindigoticum Sinuous Tubers | body | * | None | *" },
            { "Tube | Violaceum Sinuous Tubers    | body | * | None | *" },
            { "Tube | Viride Sinuous Tubers       | body | * | None | *" },
        };
    }
}

#pragma warning restore CS0649
