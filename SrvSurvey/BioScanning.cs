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
        public Status status;

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
}

#pragma warning restore CS0649
