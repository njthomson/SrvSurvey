using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0649

namespace SrvSurvey
{
    /// <summary>
    /// Represents a single sampling using the BioScanner
    /// </summary>
    class BioScan
    {
        public LatLong2? location;
        public long radius;
        public string? genus;
        public string? genusLocalized;
        public string? species;
        public string? speciesLocalized;
        public ScanType scanType;
        public double systemAddress;
        public int bodyId;
        public long reward;

        public override string ToString()
        {
            return $"{species} ({radius}m): {scanType} @{location}";
        }

        public static Dictionary<string, int> ranges = new Dictionary<string, int>()
        {
            // Distances from https://elite-dangerous.fandom.com/wiki/Genetic_Sampler

            // From Odyssey
            { "$Codex_Ent_Aleoids_Genus_Name;",            150 }, // Aleoida
            { "$Codex_Ent_Bacterial_Genus_Name;",          500 }, // Bacterium
            { "$Codex_Ent_Brancae_Name;",                  100 }, // Brain Tree
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
            { "$Codex_Ent_Tube_Name;",                     100 }, // Sinuous Tubers
            { "$Codex_Ent_Ground_Struct_Ice_Name;",        100 }, // Crystalline Shards
            { "$Codex_Ent_Sphere_Name;",                   100 }, // Anemone
            { "$Codex_Ent_Cone_Name;",                     100 }, // Bark Mounds
        };
    }

    class OrganicSummary : ScanGenus
    {
        public string? Species;
        public long? Reward;
        public int Range;
    }
}

#pragma warning restore CS0649
