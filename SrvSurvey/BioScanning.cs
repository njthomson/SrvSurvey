using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;

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

        public static List<string> match(SystemOrganism? org, SystemBody body, SystemData systemData)
        {
            var potentials = new HashSet<string>();

            // We'll lookup the parent below only if needed
            string? parentStarClass = null;

            var genus = org?.genus;
            //var atmosSphere = body.atmosphereType?.Replace("thin ", "", StringComparison.OrdinalIgnoreCase) ?? ""; // too crude?
            //var atmosphere = body.atmosphereComposition.Keys;

            var galacticRegion = GalacticRegions.current.Replace(" ", "");


            foreach (var foo in stuff)
            {
                if (genus != null && genus != foo.genus) continue;

                //if (foo.speciesPart.Contains("Spira")) Debugger.Break();

                if (foo.galacticRegion?.Contains(galacticRegion) == false) continue;
                if (foo.planetClass?.Contains(body.planetClass!) == false) continue;
                if (foo.maxGravity > 0 && body.surfaceGravity > foo.maxGravity) continue;

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

                if (foo.starClass != null)
                {
                    if (parentStarClass == null)
                        parentStarClass = systemData.getParentStarType(body, true);

                    if (parentStarClass != null && !foo.starClass.Contains(parentStarClass)) continue;
                }

                if (foo.material != null && !foo.material.Any(_ => body.materials.ContainsKey(_))) continue;

                // special cases

                if (foo.speciesPart == "Electricae Radialem" && GalacticNeblulae.distToClosest(systemData.starPos) > 100) continue;
                if (foo.speciesPart == "Clypeus Speculumi" && body.distanceFromArrivalLS > 2500) continue;

                if (foo.speciesPart.Contains("Pluma")) Debugger.Break();
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
                .Replace("HMC", "High metal content body")
                .Replace("MetalRich", "Metal-rich body")
                .Replace("Rocky", "Rocky body")
                .Replace("RockyIce", "Rocky ice world")
            ;

            var foo = new PotentialOrganism()
            {
                genus = $"$Codex_Ent_{parts[0]}_Genus_Name;",
                speciesPart = parts[1],
                planetClass = parts[2] == "*" ? null : parts[2].Split(",").ToList(),
                maxGravity = parts[3] == "*" ? 0 : double.Parse(parts[3]),
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
                foo.minTemperature = double.Parse(parts[6]);

            if (parts.Length > 7 && parts[7] != "*")
                foo.maxTemperature = double.Parse(parts[7]);

            if (parts.Length > 8 && parts[8] != "*")
                foo.galacticRegion = parts[8].Split(",").ToList();

            if (parts.Length > 9 && parts[9] != "*")
                foo.starClass = parts[9].Split(",").ToList();

            if (parts.Length > 10 && parts[10] != "*")
                foo.material = parts[10].Split(",").ToList();

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
             *  9. Valid galactic regions
             * 10. Parent star class
             * 11. Material
             * 
             * 
             * Data is mined from:
             * https://ed-dsn.net/en/conditions-of-emergence-of-exobiological-species-on-planets-a-atmosphere-fine/
             * https://docs.google.com/spreadsheets/d/1Q2ydo7R2ttiTo6_DepAM17HIBxyLLvf-x_HAY1VSGMY/
             */

            // 5x Aleoida - https://canonn.science/codex/aleoida-2/ - https://ed-dsn.net/en/aleoida_en/
            //   genus | species           | body      | gra | atmosphereType| volc | >t  | <t  |gal|star type                  | material 
            { "Aleoids | Aleoida Arcus     | HMC,Rocky | 2.7 | CarbonDioxide | None | 175 | 180 | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Coronamus | HMC,Rocky | 2.7 | CarbonDioxide | None | 180 | 190 | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Gravis    | HMC,Rocky | 2.7 | CarbonDioxide | None | 190 | 196 | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Laminiae  | HMC,Rocky | 2.7 | Ammonia       | *    | 152 | 177 | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Spica     | HMC,Rocky | 2.7 | Ammonia       | *    | 170 | 177 | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" }, // TODO NOT Sagittarius-Carina Arm

            // 13x Bacterium - https://canonn.science/codex/bacteria-2/ - https://ed-dsn.net/en/bacterium_en/
            //     genus | species             | body               | gra | atmosphereType | volcanism             | >t  | <t  |gal|sta| material 
            { "Bacterial | Bacterium Acies     | *                  | *   | Neon           | *                     | 20  | 61  | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Bacterial | Bacterium Alcyoneum | HMC,Rocky          | *   | Ammonia        | *                     | 152 | 177 | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Aurasus   | HMC,Rocky          | *   | CarbonDioxide  | *                     | 145 | 400 | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Bullaris  | *                  | *   | Methane        | *                     | 67  | 109 | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Bacterial | Bacterium Bullaris  | HMC,Rocky          | *   | MethaneRich    | *                     | 73  | 141 | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Bacterial | Bacterium Cerbrus   | HMC,Rocky,RockyIce | *   | SulphurDioxide | *                     | 132 | 500 | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Cerbrus   | HMC,Rocky          | *   | Water          | *                     | 392 | 452 | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Cerbrus   | RockyIce           | *   | WaterRich      | None                  | 231 | 315 | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Bacterial | Bacterium Informem  | *                  | *   | Nitrogen       | *                     | 43  | 150 | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Bacterial | Bacterium Nebulus   | Icy                | *   | Helium         | *                     | 19  | 21  | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Argon          | Nitrogen,Ammonia      | 50  | 172 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | ArgonRich      | Nitrogen,Ammonia      | 80  | 87  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Helium         | Nitrogen,Ammonia      | 20  | 21  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Methane        | Nitrogen,Ammonia      | 84  | 108 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Neon           | Nitrogen,Ammonia      | 20  | 61  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | NeonRich       | Nitrogen,Ammonia      | 20  | 93  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | Nitrogen       | Nitrogen,Ammonia      | 60  | 64  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Omentum   | Icy                | *   | WaterRich      | Nitrogen,Ammonia      | 240 | 307 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy,RockyIce       | 2.5 | Argon          | CarbonDioxide,Methane | 57  | 146 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 5.1 | Helium         | Methane               | 57  | 146 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 0.5 | Methane        | Methane               | 84  | 108 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy,RockyIce       | 6.0 | Neon           | CarbonDioxide,Methane | 20  | 52  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 6.1 | NeonRich       | CarbonDioxide,Methane | 20  | 65  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 2.6 | Nitrogen       | CarbonDioxide,Methane | 61  | 68  | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Scopulum  | Icy                | 3.2 | Oxygen         | CarbonDioxide,Methane | 152 | 210 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Tela      | *                  | 6.1 | *              | *                     | 20  | 607 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" }, // todo? atmos: all but water
            { "Bacterial | Bacterium Verrata   | Icy,Rocky,RockyIce | 6.1 | *              | Water                 | 20  | 442 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" }, // todo? atmos: all but SulphurDioxide
            { "Bacterial | Bacterium Vesicula  | *                  | 5.1 | Argon          | *                     | 50  | 267 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Bacterial | Bacterium Volu      | *                  | 6.0 | Oxygen         | *                     | 143 | 246 | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },

            // 5x Cactoida - https://canonn.science/codex/cactoida/ - https://ed-dsn.net/en/cactoida_en/
            //   genus | species             | body      | gra | atmosType      | vol  | >t  | <t  | galactic regions                                                                                                 | star type
            { "Cactoid | Cactoida Cortexum   | Rocky,HMC | 2.7 | CarbonDioxide  | None | 180 | 196 | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur | A,F,G,M,L,T,TTS,N" },
            { "Cactoid | Cactoida Lapis      | Rocky,HMC | 2.8 | Ammonia        | *    | 160 | 187 | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur,EmpyreanStraits,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss | A,F,G,M,L,T,TTS,N" },
            { "Cactoid | Cactoida Peperatis  | Rocky,HMC | 2.8 | Ammonia        | *    | 160 | 186 | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | A,F,G,M,L,T,TTS,N" },
            { "Cactoid | Cactoida Pullulanta | Rocky,HMC | 2.7 | CarbonDioxide  | None | 180 | 196 | GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae | A,F,G,M,L,T,TTS,N" },
            { "Cactoid | Cactoida Vermis     | Rocky     | 2.7 | SulphurDioxide | *    | 160 | 207 | *                                                                                                                | A,F,G,M,L,T,TTS,N" },
            { "Cactoid | Cactoida Vermis     | Rocky,HMC | 2.7 | Water          | None | 390 | 450 | *                                                                                                                | A,F,G,M,L,T,TTS,N" },

            // 3x Clypeus - https://canonn.science/codex/Clypeus/ - https://ed-dsn.net/en/Clypeus_en/
            { "Clypeus | Clypeus Lacrimam | Rocky | 2.7 | Water,WaterRich,CarbonDioxide,CarbonDioxideRich | * | 190 | * | * | B,A,F,G,K,M,L,T,D,N" },
            { "Clypeus | Clypeus Margaritus | HMC | 2.7 | Water,WaterRich,CarbonDioxide,CarbonDioxideRich | * | 190 | * | * | B,A,F,G,K,M,L,T,D,N" },
            // Clypeus Speculumi has special case for ArrivalDistance > 2500 
            { "Clypeus | Clypeus Speculumi | Rocky | 2.7 | Water,WaterRich,CarbonDioxide,CarbonDioxideRich | * | 190 | * | * | B,A,F,G,K,M,L,T,D,N" },

            // 4x Concha - https://canonn.science/codex/Concha/ - https://ed-dsn.net/en/Concha_en/
            { "Concha | Concha Aureolas | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | B,A,F,G,K,L,Y,W,D,N" },
            { "Concha | Concha Biconcavis | HMC,Rocky | 2.7 | Nitrogen | None | * | * | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Concha | Concha Labiata | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | * | 190 | * | B,A,F,G,K,L,Y,W,D,N" },
            { "Concha | Concha Renibus | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Concha | Concha Renibus | HMC,Rocky | 2.7 | Water,WaterRich | * | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },

            // 2x Electricae - https://canonn.science/codex/Electricae/ - https://ed-dsn.net/en/Electricae_en/
            { "Electricae | Electricae Pluma | Icy | 2.7 | Helium,Neon,NeonRich,Argon,ArgonRich | * | * | * | * | A,D,N | antimony,polonium,ruthenium,technetium,tellurium,yttrium" }, // TODO: lum V+ ???
            // Electricae Radialem has special case for nearest Nebula
            { "Electricae | Electricae Radialem | Icy | 2.7 | Helium,Neon,NeonRich,Argon,ArgonRich | * | * | * | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },

            // 6x Fonticula - https://canonn.science/codex/Fonticulua/ - https://ed-dsn.net/en/Fonticulua_en/
            { "Fonticulus | Fonticulua Campestris | Icy,RockyIce | 2.7 | Argon | * | * | * | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Digitos | Icy,RockyIce | 2.7 | Methane,MethaneRich | * | * | * | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Fluctus | Icy,RockyIce | 2.7 | Oxygen | * | * | * | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Lapida | Icy,RockyIce | 2.7 | Nitrogen | * | * | * | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Segmentatus | Icy,RockyIce | 2.7 | Neon,NeonRich | * | * | * | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },
            { "Fonticulus | Fonticulua Upupam | Icy,RockyIce | 2.7 | ArgonRich | * | * | * | * | O,B,A,F,G,K,M,L,T,TTS,Ae,Y,W,D,N" },

            // 7x Frutexa - https://canonn.science/codex/Frutexa/ - https://ed-dsn.net/en/fruxeta_en/
            { "Frutexa | Frutexa Acus | Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | * | 195 | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur | O,B,F,G,M,L,TTS,W,D,N" },
            { "Frutexa | Frutexa Collum | Rocky | 2.7 | SulphurDioxide | * | * | * | * | O,B,F,G,M,L,TTS,W,D,N" },
            { "Frutexa | Frutexa Fera | Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | * | 195 | GalacticCenter,EmpyreanStraits,NormaExpense,NormaArm,ArcadianStream,Newton'sVault,TheConduit,OuterArm,ErrantMarches,FormidineRift,Kepler'sCrest,Xibalba | O,B,F,G,M,L,TTS,W,D,N" },
            { "Frutexa | Frutexa Flabellum | Rocky | 2.7 | Ammonia | * | * | * |  Acheron,Achilles'sAltar,ArcadianStream,Dryman'sPoint,ElysianShore,EmpyreanStraits,ErrantMarches,GalacticCentre,Hawking'sGap,InnerOrionSpur,InnerOrion-PerseusConflux,Izanami,Kepler'sCrest,Lyra'sSong,MareSomnia,Newton'sVault,NormaArm,Odin'sHold,Orion-CygnusArm,OuterArm,OuterOrionSpur,OuterOrion-PerseusConflux,PerseusArm,Ryker'sHope,VulcanGate,Sagittarius-CarinaArm,SanguineousRim,Temple,Xibalba,Tenebrae,TheAbyss,TheConduit,TheFormidineRift | O,B,F,G,M,L,TTS,W,D,N" }, // NOT Scutum-Centaurus Arm
            { "Frutexa | Frutexa Flammasis | Rocky | 2.7 | Ammonia | * | * | * | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | O,B,F,G,M,L,TTS,W,D,N" },
            { "Frutexa | Frutexa Metallicum | HMC | 2.7 | Ammonia | None | * | * | * | O,B,F,G,M,L,TTS,W,D,N" },
            { "Frutexa | Frutexa Metallicum | HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | None | * | 195 | * | O,B,F,G,M,L,TTS,W,D,N" },
            { "Frutexa | Frutexa Sponsae | Rocky | 2.7 | Water,WaterRich | * | * | * | * | O,B,F,G,M,L,TTS,W,D,N" },

            // 4x Fumerola - https://canonn.science/codex/Fumerola/ - https://ed-dsn.net/en/Fumerola_en/
            { "Fumerolas | Fumerola Aquatis | Icy,Rocky,RockyIce | 2.7 | * | Water | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Fumerolas | Fumerola Carbosis | Icy | 2.7 | * | Carbon,Methane | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Fumerolas | Fumerola Extremus | Rocky,RockyIce| 2.7 | * | Silicate,Iron,Rocky,Metallic | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Fumerolas | Fumerola Nitris | Icy | 2.7 | * | Nitrogen,Ammonia | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },

            // 4x Fungoida - https://canonn.science/codex/Fungoida/ - https://ed-dsn.net/en/fungoida_en/
            { "Fungoids | Fungoida Bullarum | Rocky,RockyIce | 2.7 | Argon,ArgonRich | * | * | * | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Fungoids | Fungoida Gelata | HMC,Rocky | 2.7 | CarbonDioxide | None | 180 | 195 | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" }, // ,CarbonDioxideRich
            { "Fungoids | Fungoida Gelata | HMC,Rocky | 2.7 | Water,WaterRich | None | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Fungoids | Fungoida Setisis | HMC,Rocky,RockyIce | 2.7 | Ammonia,Methane,MethaneRich | * | * | * | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Fungoids | Fungoida Stabitis | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195 | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Fungoids | Fungoida Stabitis | HMC,Rocky | 2.7 | Water,WaterRich | * | * | * | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },

            // 6x Osseus - https://canonn.science/codex/Osseus/ - https://ed-dsn.net/en/Osseus_en/
            { "Osseus | Osseus Cornibus | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 180 | 195 | GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae | O,A,F,G,K,T,TTS,Y" },
            { "Osseus | Osseus Discus | HMC,Rocky,RockyIce, | 2.7 | Water,WaterRich | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Osseus | Osseus Fractus | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 190 | * | O,A,F,G,K,T,TTS,Y" }, // TODO: NOT Perseus Arm but including Odin’s Hold and Empyrean Straits
            { "Osseus | Osseus Pellebantus | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 191 | 197 | * | O,A,F,G,K,T,TTS,Y" }, // TODO: NOT Perseus Arm but including Odin’s Hold and Empyrean Straits
            { "Osseus | Osseus Pumice | HMC,Rocky,RockyIce | 2.7 | Argon,ArgonRich,Methane,MethaneRich,Nitrogen | * | * | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Osseus | Osseus Spiralis | HMC,Rocky | 2.7 | Ammonia | * | * | * | * | O,A,F,G,K,T,TTS,Y" },

            // 3x Recepta - https://canonn.science/codex/Recepta/ - https://ed-dsn.net/en/recepta_en/
            { "Recepta | Recepta Conditivus | * | 2.8 | ~SulphurDioxide | * | * | * | * | * | antimony,polonium,ruthenium,technetium,tellurium,yttrium" },
            { "Recepta | Recepta Deltahedronix | * | 2.8 | ~SulphurDioxide | * | * | * | * | * | cadmium,mercury,molybdenum,niobium,tin,tungsten" },
            { "Recepta | Recepta Umbrux | * | 2.8 | ~SulphurDioxide | * | * | * | * | B,A,F,G,K,M,L,T,TTS,Ae,Y,D" },

            // 8x Stratum - https://canonn.science/codex/stratum/ and https://ed-dsn.net/en/stratum_en/
            { "Stratum | Stratum Araneamus | Rocky | * | SulphurDioxide | * | 165 | * | * | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Cucumisis | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 190 | * | GalacticCenter,EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Excutitus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | 190 | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Frigus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 190 | * | GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Laminamus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | 190 | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Limaxus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | 190 | InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Paleas | Rocky | * | Ammonia,Water,WaterRich,CarbonDioxide,CarbonDioxideRich,Oxygen | * | 165 | * | * | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Tectonicas | HMC | * | Oxygen,Ammonia,Water,WaterRich,CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | * | * | F,K,M,L,T,TTS,Ae,Y,W,D" },

            // 5x Tubus - https://canonn.science/codex/Tubus/ - https://ed-dsn.net/en/tubus_en/
            { "Tubus | Tubus Cavas | Rocky | 1.5 | CarbonDioxide,CarbonDioxideRich | None | 160 | 197 | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
            { "Tubus | Tubus Compagibus | Rocky | 1.5 | CarbonDioxide,CarbonDioxideRich | None | 160 | 197 | GalacticCenter,EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
            { "Tubus | Tubus Conifer | Rocky | 1.5 | CarbonDioxide,CarbonDioxideRich | None | 160 | 196 | GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
            { "Tubus | Tubus Rosarium | Rocky | 1.6 | Ammonia | * | 160 | * | * | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },
            { "Tubus | Tubus Sororibus | HMC | 1.5 | Ammonia,CarbonDioxide,CarbonDioxideRich | None | 160 | 190 | * | O,B,A,F,G,K,M,L,T,TTS,W,D,N" },

            // 15x Tussock - https://canonn.science/codex/tussock/ - https://ed-dsn.net/en/tussock_en/
            { "Tussocks | Tussock Albata | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 175 | 180 | GalacticCenter,EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss" },
            { "Tussocks | Tussock Capillum | Rocky | 2.7 | Argon,ArgonRich,Methane,MethaneRich | *" },
            { "Tussocks | Tussock Caputus | HMC,Rocky | 2.7 | CarbonDioxide | None | 181 | 190 | EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },// ,CarbonDioxideRich
            { "Tussocks | Tussock Catena | HMC,Rocky | 2.7 | Ammonia | * | * | * | InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo" },
            { "Tussocks | Tussock Cultro | HMC,Rocky | 2.7 | Ammonia | * | * | * | GalacticCenter,EmpyreanStraits,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur" },
            { "Tussocks | Tussock Divisa | HMC,Rocky | 2.7 | Ammonia | * | * | * | Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Ignis | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 160 | 170 | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Pennata | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 145 | 155 | Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Pennatis | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | * | 195 | GalacticCenter,EmpyreanStraits,NormaExpense,NormaArm,ArcadianStream,Newton'sVault,TheConduit,OuterArm,ErrantMarches,FormidineRift,Kepler'sCrest,Xibalba" },
            { "Tussocks | Tussock Propagito | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | * | 195 | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo" },
            { "Tussocks | Tussock Serrati | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 170 | 175 | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Stigmasis | HMC,Rocky | 2.7 | SulphurDioxide | *" },
            { "Tussocks | Tussock Triticum | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | None | 190 | 195 | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Ventusa | HMC,Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 190 | 195 | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Virgam | HMC,Rocky | 2.7 | Water,WaterRich | *" },

            // 1x Amphora plant
            // Has special case for systems containing: Earth-like Worlds, Gas Giants with Water-Based Life, and Water Giants
            { "Vents | Amphora Plant | Metal-rich | * | None | * | * | * | * | A" },

            // 8x Anemone
            { "Sphere | Luteolum Anemone | MetalRich,Rocky | * | None | * | * | * | * | A,B,O" },
            { "SphereABCD | Croceum Anemone | body | * | None | * | * | * | * | A,B,O" },
            { "SphereABCD | Puniceum Anemone | body | * | None | * | * | * | * | A,B,O" },
            { "SphereABCD | Roseum Anemone | body | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Blatteum Bioluminescent Anemone | HMC,Metal-rich | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Rubeum Bioluminescent Anemone | * | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Prasinum Bioluminescent Anemone | * | * | None | * | * | * | * | A,B,O" },
            { "SphereEFGH | Roseum Bioluminescent Anemone | HMC,Metal-rich | * | None | * | * | * | * | A,B,O" },

            // 1x Bark Mounds
            { "Cone | Bark Mounds | * | * | None | * | * | * | * | A" },

            // 8x Brain Trees TODO ...
            { "Seed | Roseum Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Gypseeum Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Ostrinum Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Viride Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Lividum Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Aureum Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Puniceum Brain Tree | * | * | None | * | * | * | * | *" },
            { "Seed | Lindigoticum Brain Tree | * | * | None | * | * | * | * | *" },

            // 8x Tubers TODO ...
            { "Tube | Roseum Sinuous Tubers | body | * | None | *" },
            { "Tube | Prasinum Sinuous Tubers | body | * | None | *" },
            { "Tube | Albidum Sinuous Tubers | body | * | None | *" },
            { "Tube | Caeruleum Sinuous Tubers | body | * | None | *" },
            { "Tube | Blatteum Sinuous Tubers | body | * | None | *" },
            { "Tube | Lindigoticum Sinuous Tubers | body | * | None | *" },
            { "Tube | Violaceum Sinuous Tubers | body | * | None | *" },
            { "Tube | Viride Sinuous Tubers | body | * | None | *" },

            // Water,WaterRich,CarbonDioxide,CarbonDioxideRich    Ammonia,CarbonDioxide,CarbonDioxideRich  Methane,MethaneRich  SulphurDioxide

            /* Galactic Regions
             * ----------------
             * 
             * Orion-Cygnus Arm including Odin’s Hold and Galactic Centre:
             *      GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur
             *
             * Outer Arm + Galactic Center and Empyrean Straits:
             *      GalacticCenter,EmpyreanStraits,NormaExpense,NormaArm,ArcadianStream,Newton'sVault,TheConduit,OuterArm,ErrantMarches,FormidineRift,Kepler'sCrest,Xibalba
             *
             * Scutum-Centaurus Arm + Odin's Hold and Galactic Center
             *      GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo
             * 
             * Perseus Arm including Ryker’s Hope and Galactic Centre:
             *      GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae
             * 
             * Sagittarius-Carina Arm including Odin’s Hold and Galactic Centre:
             *      GalacticCenter,EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss
             * 
             * Acheron Achilles'sAltar Aquila'sHalo ArcadianStream Dryman'sPoint ElysianShore EmpyreanStraits ErrantMarches FormorianFrontier GalacticCentre Hawking'sGap HieronymusDelta InnerOrionSpur InnerOrion-PerseusConflux InnerScutum-CentaurusArm Izanami
             * Kepler'sCrest Lyra'sSong MareSomnia Newton'sVault NormaArm NormaExpanse Odin'sHold Orion-CygnusArm OuterArm OuterOrionSpur OuterOrion-PerseusConflux OuterScutum-CentaurusArm PerseusArm Ryker'sHope VulcanGate Sagittarius-CarinaArm SanguineousRim
             * Temple Xibalba Tenebrae TheAbyss TheConduit TheFormidineRift TheVeils TheVoid TrojanBelt
             * 
             * Acheron,Achilles'sAltar,ArcadianStream,Dryman'sPoint,ElysianShore,EmpyreanStraits,ErrantMarches,GalacticCentre,Hawking'sGap,InnerOrionSpur,InnerOrion-PerseusConflux,Izanami,Kepler'sCrest,Lyra'sSong,MareSomnia,Newton'sVault,NormaArm,Odin'sHold,Orion-CygnusArm,OuterArm,OuterOrionSpur,OuterOrion-PerseusConflux,PerseusArm,Ryker'sHope,VulcanGate,Sagittarius-CarinaArm,SanguineousRim,Temple,Xibalba,Tenebrae,TheAbyss,TheConduit,TheFormidineRift
             */

            //              


        };
    }
}

#pragma warning restore CS0649
