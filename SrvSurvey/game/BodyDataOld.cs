using Newtonsoft.Json;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal class BodyDataOld : Data
    {
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

        public static void migrate_ScannedOrganics_Into_ScannedBioEntryIds(CommanderSettings cmdr) // keep for migration purposes
        {
            Game.log($"Migrate cmdr 'scannedOrganics' into 'scannedBioEntryIds', scannedOrganics: {cmdr.scannedOrganics?.Count}, scannedBioEntryIds: {cmdr.scannedBioEntryIds.Count} ...");
            if (!(cmdr.scannedOrganics?.Count > 0)) return;

            // first migrate any 'scannedBioEntryIds' entries without rewards or firstFoot entries
            var foo = cmdr.scannedBioEntryIds.Select(oldEntry =>
            {
                var parts = oldEntry.Split('_').ToList();

                if (parts.Count == 3)
                {
                    // append the reward
                    var bioMatch = Game.codexRef.matchFromEntryId1(parts[2]);
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
                    suffix = Game.codexRef.matchFromSpecies(oldScanEntry.species!)!.variants[0].entryIdSuffix;

                var speciesRef = Game.codexRef.matchFromSpecies(oldScanEntry.species!)!;
                var entryId = speciesRef.entryIdPrefix + suffix;

                var prefix = $"{oldScanEntry.systemAddress}_{oldScanEntry.bodyId}_{entryId}";
                var match = newScannedBioEntryIds.FirstOrDefault(_ => _.StartsWith(prefix) && _.Contains($"_{oldScanEntry.reward}_"));
                if (match == null)
                {
                    if (entryId.Length == 5) entryId += "00";
                    var newScannedEntryId = $"{oldScanEntry.systemAddress}_{oldScanEntry.bodyId}_{entryId}_{oldScanEntry.reward}_{bool.FalseString}";
                    newScannedBioEntryIds.Insert(0, newScannedEntryId);
                }
            }
            cmdr.scannedBioEntryIds = new HashSet<string>(newScannedBioEntryIds);
            cmdr.reCalcOrganicRewards();

            Game.log("Migrate cmdr 'scannedOrganics' into 'scannedBioEntryIds' - complete");
            cmdr.migratedScannedOrganicsInEntryId = true;
            cmdr.Save();
        }
    }
}
