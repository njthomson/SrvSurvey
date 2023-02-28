using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    delegate void BioScanEvent();

    /// <summary>
    /// Represents a single sampling using the BioScanner
    /// </summary>
    class BioScan
    {
        public LatLong2 location;
        public long radius;
        public string genus;
        public string species;
        public ScanType scanType;

        public override string ToString()
        {
            return $"{species} ({radius}m): {scanType} @{location}";
        }

        public static Dictionary<string, int> ranges = new Dictionary<string, int>()
        {
            { "$Codex_Ent_Aleoids_Genus_Name;",            150 }, // Aleoida
            { "$Codex_Ent_Sphere_Name;",                   100 }, // Luteolum Anemone
            { "$Codex_Ent_Bacterial_Genus_Name;",          500 }, // Bacterium
            { "$Codex_Ent_Cone_Name;",                     100 }, // Bark Mounds
            { "$Codex_Ent_Brancae_Name;",                  100 }, // Brain Tree
            { "$Codex_Ent_Cactoid_Genus_Name;",            300 }, // Cactoida
            { "$Codex_Ent_Clypeus_Genus_Name;",            150 },
            { "$Codex_Ent_Conchas_Genus_Name;",            150 }, // Concha
            { "$Codex_Ent_Crystalline Shard_Genus_Name;",  100 },
            { "$Codex_Ent_Electricae_Genus_Name;",        1000 }, // Electricae
            { "$Codex_Ent_Fonticulus_Genus_Name;",         500 }, // Fonticulua
            { "$Codex_Ent_Shrubs_Genus_Name;",             150 }, // Frutexa
            { "$Codex_Ent_Fumerola_Genus_Name;",           100 },
            { "$Codex_Ent_Fungoids_Genus_Name;",           300 }, // Fungoida
            { "$Codex_Ent_Osseus_Genus_Name;",             800 }, // Osseus
            { "$Codex_Ent_Recepta_Genus_Name;",            150 },
            { "$Codex_Ent_Sinuous Tuber_Genus_Name;",      100 },
            { "$Codex_Ent_Stratum_Genus_Name;",            500 }, // Stratum
            { "$Codex_Ent_Tubus_Genus_Name;",              800 }, // Tubus
            { "$Codex_Ent_Tussocks_Genus_Name;",           200 }, // Tussock

        };
        /*
                Species Range
        Aleoida 	150m 
        Anemone 	100m 
        Bacterium 	500m 
        Bark Mound	100m 
        Brain Tree 	100m 
        Cactoida 	300m 
        Clypeus 	150m 
        Concha 	150m 
        Crystalline Shard 	100m 
        Electricae 	1000m 
        Fonticulua 	500m 
        Frutexa 	150m 
        Fumerola 	100m 
        Fungoida 	300m 
        Osseus 	800m 
        Recepta 	150m 
        Sinuous Tuber 	100m 
        Stratum 	500m 
        Tubus 	800m 
        Tussock 	200m 
        */
    }

    class Genus : ScanGenus
    {
        public float credits;

    }

    internal class BioScanning
    {
        public List<ScanGenus> genuses;
        public List<BioScan> completed = new List<BioScan>();
        public BioScan scanOne;
        public BioScan scanTwo;
        public event BioScanEvent bioScanEvent;

        private Game game;

        BioScanning(Game game, List<ScanGenus> genuses)
        {
            this.game = game;
            this.genuses = genuses;
        }

        private void addBioScan(ScanOrganic entry)
        {
            var newScan = new BioScan()
            {
                location = new LatLong2(game.status),
                radius = BioScan.ranges[entry.Genus],
                genus = entry.Genus,
                species = entry.Species,
            };
            Game.log($"Scan: {newScan}");

            if (entry.ScanType == ScanType.Log)
            {
                // replace 1st, clear 2nd
                this.scanOne = newScan;
                this.scanTwo = null;
            }
            else if (entry.ScanType == ScanType.Sample)
            {
                this.scanTwo = newScan;
            }
            else if (entry.ScanType == ScanType.Analyse)
            {
                this.completed.Add(scanOne);
                this.completed.Add(scanTwo);
                this.completed.Add(newScan);
            }
            else
            {
                throw new Exception($"Unexpected ScanType: {entry.ScanType}");
            }

            if (this.bioScanEvent != null)
            {
                this.bioScanEvent();
            }
        }


    }
}
