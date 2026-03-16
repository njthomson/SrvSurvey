using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using SrvSurvey.units;
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
        public string genus;
        public string species;
        [JsonConverter(typeof(StringEnumConverter))]
        public Status status;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long entryId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string body;

        public override string ToString()
        {
            return $"{species} ({radius}m): @{location}";
        }

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
            // New stuff
            { "$Codex_Ent_Ingensradices_Unicus_Name;",     "Radicoida"},
        };

        public enum Status
        {
            Complete,
            Active,
            Abandoned,
            Died,
        }

        public static void setUnclaimedSystemBioScansAsDied(List<string> scannedBioEntryIds, string fid)
        {
            // parse/group prior scans by system
            var scansBySystem = new Dictionary<long, List<ScannedBioEntryId>>();
            foreach (var hash in scannedBioEntryIds)
            {
                var parsed = new ScannedBioEntryId(hash);
                scansBySystem.init(parsed.id64).Add(parsed);
            }

            // open each system, update bioScan entries' status to Died as needed
            foreach (var (id64, scans) in scansBySystem)
            {
                var sys = SystemData.Load("", id64, fid, null, true);
                if (sys == null) continue;
                Game.log($"Purging {scans.Count} bio scans from: {sys.name}");

                foreach (var scan in scans)
                {
                    var bs = sys
                        .bodies.Find(b => b.id == scan.bodyNum)
                        ?.bioScans?.Find(bs => bs.entryId == scan.entryId);

                    if (bs != null && bs.status != Status.Abandoned)
                        bs.status = Status.Died;
                }

                // close the system - unless it's our current system
                if (sys != Game.activeGame?.systemData)
                    SystemData.Close(sys);
                else
                    sys.Save();
            }
        }
    }

    class ScannedBioEntryId
    {
        public long id64;
        public int bodyNum;
        public long entryId;
        public long reward;
        public bool firstFootfall;

        public ScannedBioEntryId(string hash)
        {
            // {sys.address}_{body.id}_{organism.entryId}_{organism.reward}_{firstFootfall}
            var parts = hash.Split('_', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            this.id64 = long.Parse(parts[0]);
            this.bodyNum = int.Parse(parts[1]);
            this.entryId = long.Parse(parts[2]);
            this.reward = long.Parse(parts[3]);
            this.firstFootfall = bool.Parse(parts[4]);
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
