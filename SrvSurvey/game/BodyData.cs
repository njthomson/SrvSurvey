using Newtonsoft.Json;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

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

            this.Save();
            return organism;
        }

        [JsonIgnore]
        public int countOrganisms { get => this.organisms.Count; }
        [JsonIgnore]
        public int countAnalyzed { get => this.organisms.Values.Where(_ => _.analyzed).Count(); }
        [JsonIgnore]
        public long sumOrganicPotentialValue { get => this.organisms.Values.Sum(_ => _.reward); }
        [JsonIgnore]
        public long sumOrganicScannedValue { get => this.organisms.Values.Sum(_ => _.analyzed ? _.reward : 0); }
        [JsonIgnore]
        public bool isFullPotentialKnown { get => !this.organisms.Values.Any(_ => _.reward == 0); }

        [JsonIgnore]
        public float bodyScanValueProgress
        {
            get
            {
                // todo: make this single-pass, rather than iterating of the same array 3 times
                var total = sumOrganicPotentialValue;
                var scanned = sumOrganicScannedValue * 1f;

                var progress = 0f;
                if (total != 0 && scanned != 0)
                    progress = 1f / total * scanned;

                return progress;
            }
        }
    }
}
