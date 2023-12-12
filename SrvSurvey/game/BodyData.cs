using Newtonsoft.Json;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal class BodyData : Data
    {
        public static BodyData Load(LandableBody nearBody)
        {
            var path = Path.Combine(Application.UserAppDataPath, "organic", Game.activeGame!.fid!);
            Directory.CreateDirectory(path);

            var filepath = Path.Combine(path, $"{nearBody.bodyName}.json");
            return Data.Load<BodyData>(filepath)
                ?? new BodyData()
                {
                    filepath = filepath,
                    commander = Game.activeGame!.Commander!,
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
        public string commander;
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
                    variant = scan.Variant,
                    variantLocalized = scan.Variant_Localised,
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
                if (organism.reward > 0)
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

        public static void migrate_ScannedOrganics_Into_ScannedBioEntryIds(CommanderSettings cmdr)
        {
            Game.log("Migrate cmdr 'scannedOrganics' into 'scannedBioEntryIds' ...");

            // first migrate any 'scannedBioEntryIds' entries without rewards or firstFoot entries
            var foo = cmdr.scannedBioEntryIds.Select(oldEntry =>
            {
                var parts = oldEntry.Split('_').ToList();

                if (parts.Count == 3)
                {
                    // append the reward
                    var bioMatch = Game.codexRef.matchFromEntryId(parts[2]);
                    parts.Add(bioMatch.species.reward.ToString());
                }

                if (parts.Count == 4)
                {
                    // append firstFoot (false as we no longer know)
                    parts.Add(bool.FalseString);
                }

                return string.Join("_", parts);
            }).ToList();
            cmdr.scannedBioEntryIds = new HashSet<string>(foo);
            cmdr.Save();

            // now migrate cmdr 'scannedOrganics' into 'scannedBioEntryIds', maintaining the order from 'scannedOrganics'
            var newScannedBioEntryIds = cmdr.scannedBioEntryIds.ToList();
            for (var n = cmdr.scannedOrganics.Count; n > 0; n--)
            {
                var oldScanEntry = cmdr.scannedOrganics[n - 1];

                // we did not track variant previously, so we'll use "" which will match any variant of the species (and their will only be 1 variant per species on a body ... apart from legacy organisms, but that's okay the species is enough to know the full entryId)
                var suffix = "";

                var isLegacyGenus = Game.codexRef.isLegacyGenus(oldScanEntry.genus!, oldScanEntry.species!);
                if (isLegacyGenus)
                    suffix = Game.codexRef.matchFromSpecies(oldScanEntry.species!).variants[0].entryIdSuffix;

                var speciesRef = Game.codexRef.matchFromSpecies(oldScanEntry.species!);
                var entryId = speciesRef.entryIdPrefix + suffix;

                var prefix = $"{oldScanEntry.systemAddress}_{oldScanEntry.bodyId}_{entryId}";
                var match = newScannedBioEntryIds.FirstOrDefault(_ => _.StartsWith(prefix) && _.Contains($"_{oldScanEntry.reward}_"));
                if (match == null)
                {
                    var newScannedEntryId = $"{oldScanEntry.systemAddress}_{oldScanEntry.bodyId}_{entryId}_{oldScanEntry.reward}_{bool.FalseString}";
                    newScannedBioEntryIds.Insert(0, newScannedEntryId);
                }
            }
            cmdr.scannedBioEntryIds = new HashSet<string>(newScannedBioEntryIds);

            Game.log("Migrate cmdr 'scannedOrganics' into 'scannedBioEntryIds' - complete");
            cmdr.migratedScannedOrganicsInEntryId = true;
            cmdr.Save();
        }
    }
}
