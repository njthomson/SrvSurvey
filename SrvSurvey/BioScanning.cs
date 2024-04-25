using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using SrvSurvey.units;

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
        public List<string>? atmosphereType;
        public List<string>? volcanism;
        public double minTemperature;
        public double maxTemperature;
        public long minArrivalDistance;
        public List<string>? galacticRegion;
        public List<string>? starClass;

        public static List<string> match(SystemOrganism? org, SystemBody body)
        {
            // TODO: Is it okay to use the primary star for the system? Or does it need to be the closest star to the current body?
            var starClass = Game.activeGame?.systemData?.bodies.FirstOrDefault()?.planetClass ?? "?";

            return match(
                org?.genus,
                body.atmosphereType?.Replace("thin ", "", StringComparison.OrdinalIgnoreCase) ?? "", // too crude?
                body.surfaceGravity,
                body.planetClass ?? "",
                body.volcanism?.Replace("major", "").Replace("major", "minor").Replace("major", " volcanism") ?? "",
                body.surfaceTemperature,
                body.distanceFromArrivalLS,
                GalacticRegions.current.Replace(" ", ""),
                starClass
            );
        }

        private static List<string> match(string? genus, string atmosphere, double gravity, string planetClass, string volcanism, double temp, long arrivalDistance, string galacticRegion, string starClass)
        {
            var potentials = new HashSet<string>();

            foreach (var foo in stuff)
            {
                if (genus != null && genus != foo.genus) continue;

                if (foo.galacticRegion?.Contains(galacticRegion) == false) continue;

                if (foo.planetClass?.Contains(planetClass) == false) continue;
                if (foo.maxGravity > 0 && gravity  > foo.maxGravity) continue;
                if (foo.atmosphereType?.Contains(atmosphere) == false) continue;

                if (foo.volcanism?.Any(_ => volcanism.Contains(_, StringComparison.OrdinalIgnoreCase)) == false) continue;

                if (foo.minTemperature > 0 && temp < foo.minTemperature) continue;
                if (foo.maxTemperature > 0 && temp > foo.maxTemperature) continue;

                if (foo.minArrivalDistance > 0 && arrivalDistance < foo.minArrivalDistance) continue;

                if (foo.starClass?.Contains(galacticRegion) == false) continue;

                potentials.Add(foo.speciesPart);
            }

            return potentials.ToList();
        }

        public static implicit operator PotentialOrganism(string bigString)
        {
            var parts = bigString.Split(" | ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // expand some shorted names
            parts[2] = parts[2]
                .Replace("RockeyIce", "Rocky ice")
                .Replace("HMC", "High metal content");

            var foo = new PotentialOrganism()
            {
                genus = $"$Codex_Ent_{parts[0]}_Genus_Name;",
                speciesPart = parts[1],
                planetClass = parts[2] == "*" ? null : parts[2].Split(",").Select(_ => $"{_} body").ToList(),
                maxGravity = parts[3] == "*" ? 0 : double.Parse(parts[3]),
                atmosphereType = parts[4] == "*" ? null : parts[4].Split(",").ToList(),
            };

            // the following are optional
            if (parts.Length > 5 && parts[5] != "*")
                foo.volcanism = parts[5].Split(",").ToList();

            if (parts.Length > 6 && parts[6] != "*")
                foo.minTemperature = double.Parse(parts[6]);

            if (parts.Length > 7 && parts[7] != "*")
                foo.maxTemperature = double.Parse(parts[7]);

            if (parts.Length > 8 && parts[8] != "*")
                foo.minArrivalDistance = long.Parse(parts[8]);

            if (parts.Length > 9 && parts[9] != "*")
                foo.galacticRegion = parts[9].Split(",").ToList();

            if (parts.Length > 10 && parts[10] != "*")
                foo.starClass = parts[10].Split(",").ToList();

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
             *  5. List of atmosphere types
             *  6. List of volcanisms
             *  7. Min surface temperature
             *  8. Max surface temperature
             *  9. Min arrival distance
             * 10. Valid galactic regions
             * 11. Valid star class
             * 
             *  TODO: Parent star types
             * 
             * Data is mined from:
             * https://ed-dsn.net/en/conditions-of-emergence-of-exobiological-species-on-planets-a-atmosphere-fine/
             */

            // 13x Bacterium - https://canonn.science/codex/bacteria-2/
            { "Bacterial | Bacterium Tela | * | * | * | Helium,Iron,Silicate,Ammonia,Water,Nitrogen,Methane" }, // or just drop the volcanism?
            { "Bacterial | Bacterium Nebulus | Icy | * | Helium | *" },
            { "Bacterial | Bacterium Acies | * | * | Neon,NeonRich | *" },
            { "Bacterial | Bacterium Verrata | Icy,RockeyIce,Rocky | * | Neon,NeonRich | Water" },
            { "Bacterial | Bacterium Omentum | Icy | * | Neon,NeonRich,Argon,ArgonRich,Methane,MethaneRich,Helium | Nitrogen,Ammonia" },
            { "Bacterial | Bacterium Scopulum | Icy | * | * | Carbon,Methane" }, // Minor Methane Magma
            { "Bacterial | Bacterium Vesicula | Icy,RockeyIce,HMC | * | Argon,ArgonRich | *" },
            { "Bacterial | Bacterium Bullaris | Icy,RockeyIce,Rocky,HMC | * | Methane,MethaneRich | *" },
            { "Bacterial | Bacterium Informem | Icy,RockeyIce,Rocky,HMC | * | Nitrogen | *" },
            { "Bacterial | Bacterium Volu | Icy,RockeyIce,Rocky,HMC | * | Oxygen | *" },
            { "Bacterial | Bacterium Alcyoneum | Rocky,HMC | * | Ammonia | *" },
            { "Bacterial | Bacterium Aurasus | Rocky,HMC | * | CarbonDioxide,CarbonDioxideRich | *" },
            { "Bacterial | Bacterium Cerbrus | Rocky,HMC | * | Water,WaterRich,SulphurDioxide | *" },

            // 5x Cactoida - https://canonn.science/codex/cactoida/
            { "Cactoid | Cactoida Cortexum | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195 | * | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur" },
            { "Cactoid | Cactoida Pullulanta | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195 | * | GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Cactoid | Cactoida Lapis | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur,EmpyreanStraits,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss" },
            { "Cactoid | Cactoida Peperatis | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo" },
            { "Cactoid | Cactoida Vermis | Rocky,HMC | 2.7 | Water,WaterRich | *" },

            // 3x Clypeus 
            { "Clypeus | Clypeus Lacrimam | Rocky,HMC | 2.7 | Water,WaterRich,CarbonDioxide,CarbonDioxideRich | * | 190" },
            { "Clypeus | Clypeus Margaritus | Rocky,HMC | 2.7 | Water,WaterRich,CarbonDioxide,CarbonDioxideRich | * | 190" },
            { "Clypeus | Clypeus Speculumi | Rocky,HMC | 2.7 | Water,WaterRich,CarbonDioxide,CarbonDioxideRich | * | 190 | * | 2500" },

            // 4x Concha
            { "Concha | Concha Aureolas | * | 2.7 | Ammonia | *" },
            { "Concha | Concha Renibus | Body | 2.7 | Water,WaterRich | *" }, // any temp
            { "Concha | Concha Renibus | Body | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195" },
            { "Concha | Concha Labiata | Body | 2.7 | CarbonDioxide,CarbonDioxideRich | * | * | 190" },
            { "Concha | Concha Biconcavis | * | 2.7 | Nitrogen | *" },

            // 2x Electricae
            /*
            { "Electricae | Electricae Pluma | Icy | 2.7 | Helium,Neon,NeonRich,Argon,ArgonRich | * | * | * | * | A,V" }, // lum V+ ???
            { "Electricae | Electricae Radialem | Icy | 2.7 | Helium,Neon,NeonRich,Argon,ArgonRich | *" }, // needs special case for nearest Nebula
            */

            // 6x Fonticula
            { "Fonticulus | Fonticulua Segmentatus | * | 2.7 | Neon,NeonRich | *" },
            { "Fonticulus | Fonticulua Digitos | Icy,RockeyIce | 2.7 | Methane,MethaneRich | *" },
            { "Fonticulus | Fonticulua Campestris | Icy,RockeyIce | 2.7 | Argon | *" },
            { "Fonticulus | Fonticulua Upupam | Icy,RockeyIce | 2.7 | ArgonRich | *" },
            { "Fonticulus | Fonticulua Lapida | Icy,RockeyIce | 2.7 | Nitrogen | *" },
            { "Fonticulus | Fonticulua Fluctus | Icy,RockeyIce | 2.7 | Oxygen | *" },

            // 7x Frutexa
            { "Frutexa | Frutexa Flabellum | Rocky | 2.7 | Ammonia" }, // TODO: NOT Scutum-Centaurus Arm
            { "Frutexa | Frutexa Flammasis | Rocky | 2.7 | Ammonia | * | * | * | * | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo" },
            { "Frutexa | Frutexa Metallicum | HMC | 2.7 | Ammonia | *" }, // any temp
            { "Frutexa | Frutexa Metallicum | HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | * | 195" },
            { "Frutexa | Frutexa Acus | Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | * | * | 195 | * | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur" },
            { "Frutexa | Frutexa Fera | Rocky | 2.7 | CarbonDioxide,CarbonDioxideRich | * | * | 195 | * | GalacticCenter,EmpyreanStraits,NormaExpense,NormaArm,ArcadianStream,Newton'sVault,TheConduit,OuterArm,ErrantMarches,FormidineRift,Kepler'sCrest,Xibalba" },
            { "Frutexa | Frutexa Sponsae | Rocky | 2.7 | Water,WaterRich | *" },
            { "Frutexa | Frutexa Collum | Rocky | 2.7 | SulphurDioxide | *" }, // not rich?

            // 4x Fumerola
            { "Fumerolas | Fumerola Carbosis | Body | 2.7 | * | Carbon,Methane" },
            { "Fumerolas | Fumerola Extremus | Body | 2.7 | * | Silicate,Iron,Rocky" },
            { "Fumerolas | Fumerola Nitris | Body | 2.7 | * | Nitrogen,Ammonia" },
            { "Fumerolas | Fumerola Aquatis | Body | 2.7 | * | Water" },

            // 4x Fungoida
            { "Fungoids | Fungoida Setisis | Rocky,HMC | 2.7 | Ammonia,Methane,MethaneRich | *" },
            { "Fungoids | Fungoida Bullarum | Rocky,HMC | 2.7 | Argon,ArgonRich | *" },
            { "Fungoids | Fungoida Stabitis | Rocky,HMC | 2.7 | Water,WaterRich | *" }, // any temp
            { "Fungoids | Fungoida Stabitis | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195" },
            { "Fungoids | Fungoida Gelata | Rocky,HMC | 2.7 | Water,WaterRich | *" }, // any temp
            { "Fungoids | Fungoida Gelata | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195" },

            // 6x Osseus
            { "Osseus | Osseus Spiralis | Rocky,HMC | 2.7 | Ammonia | *" },
            { "Osseus | Osseus Discus | Rocky,HMC | 2.7 | Water,WaterRich | *" },
            { "Osseus | Osseus Cornibus | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 195" },
            { "Osseus | Osseus Fractus | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 190" },
            { "Osseus | Osseus Pellebantus | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 190 | 195" },
            { "Osseus | Osseus Pumice | Rocky,RockeyIce,HMC | 2.7 | Argon,ArgonRich,Methane,MethaneRich,Nitrogen | *" },

            // 3x Recepta
            { "Recepta | Recepta Conditivus | Icy,RockeyIce,Rocky,HMC | 2.8 | CarbonDioxide,SulphurDioxide | *" },
            { "Recepta | Recepta Umbrux | Icy,RockeyIce,Rocky,HMC | 2.8 | CarbonDioxide,SulphurDioxide | *" },
            { "Recepta | Recepta Deltahedronix | Rocky,HMC | 2.8 | CarbonDioxide,SulphurDioxide | *" },

            // 8x Stratum - https://canonn.science/codex/stratum/ and https://ed-dsn.net/en/stratum_en/
            { "Stratum | Stratum Tectonicas | HMC | * | Oxygen,Ammonia,Water,WaterRich,CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | * | * | * | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Paleas | Rocky | * | Ammonia,Water,WaterRich,CarbonDioxide,CarbonDioxideRich,Oxygen | * | 165 | * | * | * | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Laminamus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | 190 | * | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Excutitus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | 190 | * | GalacticCenter,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Araneamus | Rocky | * | SulphurDioxide | * | 165 | * | * | * | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Limaxus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 165 | 190 | * | InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Cucumisis | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 190 | * | * | GalacticCenter,EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss | F,K,M,L,T,TTS,Ae,Y,W,D" },
            { "Stratum | Stratum Frigus | Rocky | * | CarbonDioxide,CarbonDioxideRich,SulphurDioxide | * | 190 | * | * | GalacticCenter,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae | F,K,M,L,T,TTS,Ae,Y,W,D" },

            // 5x Tubus
            { "Tubus | Tubus Sororibus | HMC | 1.5 | Ammonia,CarbonDioxide,CarbonDioxideRich | *" },
            { "Tubus | Tubus Rosarium | Rocky | 1.5 | Ammonia | *" },
            { "Tubus | Tubus Conifer | Rocky | 1.5 | CarbonDioxide,CarbonDioxideRich | * | 160 | 190" },
            { "Tubus | Tubus Cavas | Rocky | 1.5 | CarbonDioxide,CarbonDioxideRich | * | 160 | 190" },
            { "Tubus | Tubus Compagibus | Rocky | 1.5 | CarbonDioxide,CarbonDioxideRich | * | 160 | 190" },

            // 15x Tussock - https://canonn.science/codex/tussock/ - https://ed-dsn.net/en/tussock_en/
            { "Tussocks | Tussock Catena | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo" },
            { "Tussocks | Tussock Cultro | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | GalacticCenter,EmpyreanStraits,Odin'sHold,Izanami,InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,OuterOrionSpur" },
            { "Tussocks | Tussock Divisa | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Virgam | Rocky,HMC | 2.7 | Water,WaterRich | *" },
            { "Tussocks | Tussock Capillum | Rocky | 2.7 | Argon,ArgonRich,Methane,MethaneRich | *" },
            { "Tussocks | Tussock Stigmasis | Rocky,HMC | 2.7 | SulphurDioxide | *" },
            { "Tussocks | Tussock Albata | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 175 | 180 | * | GalacticCenter,EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss" },
            { "Tussocks | Tussock Caputus | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 190 | * | EmpyreanStraits,Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Ignis | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 160 | 170 | * | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Pennata | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 145 | 155 | * | Odin'sHold,InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Pennatis | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | * | 195 | * | GalacticCenter,EmpyreanStraits,NormaExpense,NormaArm,ArcadianStream,Newton'sVault,TheConduit,OuterArm,ErrantMarches,FormidineRift,Kepler'sCrest,Xibalba" },
            { "Tussocks | Tussock Propagito | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | * | 195 | * | GalacticCenter,Odin'sHold,InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,TheVoid,Aquila'sHalo" },
            { "Tussocks | Tussock Serrati | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 170 | 175 | * | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Triticum | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 160 | 190 | * | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "Tussocks | Tussock Ventusa | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 190 | 195 | * | InnerScutum-CentaurusArm,InnerOrionSpur,ElysianShore,SanguineousRim,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,TheAbyss,Ryker'sHope,Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },

            // 5x Aleoida - https://canonn.science/codex/aleoida-2/ - https://ed-dsn.net/en/aleoida_en/
            { "Aleoids | Aleoida Spica | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" }, // TODO NOT Sagittarius-Carina Arm
            { "Aleoids | Aleoida Laminiae | Rocky,HMC | 2.7 | Ammonia | * | * | * | * | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Arcus | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 175 | 180 | * | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Coronamus | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 180 | 190 | * | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },
            { "Aleoids | Aleoida Gravis | Rocky,HMC | 2.7 | CarbonDioxide,CarbonDioxideRich | * | 190 | 195 | * | * | B,A,F,K,M,L,T,TTS,Y,W,D,N" },

            // 1x Amphora plant
            { "Aleoids | Aleoida xxx | body | 2.7 | Atmos | *" },

            // 8x Anemone

            // 1x Bark Mounds

            // 8x Brain Trees

            // 8x Tubers

            // template line
            { "Aleoids | Aleoida xxx | body | 2.7 | Atmos | *" },

            // Rocky,HMC
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
             */

        };
    }
}

#pragma warning restore CS0649
