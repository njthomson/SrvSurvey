using Newtonsoft.Json;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal class BodyData : Data
    {
        public static BodyData Load(LandableBody nearBody)
        {
            var path = Path.Combine(Application.UserAppDataPath, "organic");
            Directory.CreateDirectory(path);

            var filepath = Path.Combine(path, $"{nearBody.bodyName}.json");
            return Data.Load<BodyData>(filepath)
                ?? new BodyData()
                {
                    filepath = filepath,
                    firstVisited = DateTimeOffset.UtcNow,
                    systemName = nearBody.systemName,
                    systemAddress = nearBody.systemAddress,
                    bodyName = nearBody.bodyName,
                    bodyId = nearBody.bodyId,
                };
        }

        #region data members

        public string systemName;
        public string bodyName;
        public int bodyId;
        public long systemAddress;

        /// <summary> All the scans performed on this body </summary>
        public List<BioScan> bioScans = new List<BioScan>();

        /// <summary> All the organisms for this body </summary>
        public Dictionary<string, OrganicSummary> organisms = new Dictionary<string, OrganicSummary>();

        public LatLong2 lastTouchdown;
        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;

        #endregion

        public OrganicSummary addScanGenus(ScanGenus scan)
        {
            // add only if missing
            if (!this.organisms.ContainsKey(scan.Genus))
            {
                var newSummary = new OrganicSummary
                {
                    range = BioScan.ranges[scan.Genus],
                    genus = scan.Genus,
                    genusLocalized = scan.Genus_Localised,
                };

                this.organisms.Add(scan.Genus, newSummary);
                this.updateScanProgress();
                this.Save();

                return newSummary;
            }
            else
            {
                return this.organisms[scan.Genus];
            }
        }

        public OrganicSummary addScanOrganic(ScanOrganic scan)
        {
            // add/augment organic summary data
            OrganicSummary organism;
            if (this.organisms.ContainsKey(scan.Genus))
            {
                organism = this.organisms[scan.Genus];
                organism.species = scan.Species;
                organism.speciesLocalized = scan.Species_Localised;
                organism.reward = Game.codexRef.getRewardForSpecies(scan.Species);

                if (scan.ScanType == ScanType.Analyse)
                    organism.analyzed = true;
            }
            else
            {
                organism = new OrganicSummary
                {
                    range = BioScan.ranges[scan.Genus],
                    genus = scan.Genus,
                    genusLocalized = scan.Genus_Localized,
                    species = scan.Species,
                    speciesLocalized = scan.Species_Localised,
                    analyzed = scan.ScanType == ScanType.Analyse,
                    reward = Game.codexRef.getRewardForSpecies(scan.Species),
                };

                this.organisms.Add(scan.Genus, organism);
            }

            this.updateScanProgress();
            this.Save();
            return organism;
        }

        [JsonIgnore]
        public int countOrganisms;

        [JsonIgnore]
        public long sumPotentialKnown;
        [JsonIgnore]
        public long sumPotentialEstimate;
        [JsonIgnore]
        public long sumAnalyzed;
        [JsonIgnore]
        public int countAnalyzed;
        [JsonIgnore]
        public float scanProgress;

        public void updateScanProgress()
        {
            sumPotentialKnown = 0;
            sumPotentialEstimate = 0;
            sumAnalyzed = 0;
            scanProgress = 0;
            countAnalyzed = 0;
            countOrganisms = 0;

            foreach (var organism in this.organisms.Values)
            {
                countOrganisms++;

                // sum known rewards
                if (organism.reward > 0 )
                {
                    sumPotentialKnown += organism.reward;
                    sumPotentialEstimate += organism.reward;
                }
                else
                {
                    // substituting 1M if not known
                    sumPotentialEstimate += 1000000;
                }

                // sum analyzed rewards
                if (organism.analyzed)
                {
                    countAnalyzed++;
                    sumAnalyzed += organism.reward;
                }
            }

            scanProgress = 1f / sumPotentialEstimate * sumAnalyzed;
        }
    }
}
