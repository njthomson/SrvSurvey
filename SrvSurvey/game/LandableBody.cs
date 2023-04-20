using SrvSurvey.units;
using System;

namespace SrvSurvey.game
{
    delegate void BioScanEvent();

    class LandableBody : IDisposable
    {
        private readonly Game game;
        public readonly string bodyName;
        public readonly int bodyId;
        public long systemAddress;
        public double radius;

        public event BioScanEvent? bioScanEvent;
        public List<BioScan> completedScans = new List<BioScan>();
        public BioScan? scanOne;
        public BioScan? scanTwo;
        public Dictionary<string, string> analysedSpecies = new Dictionary<string, string>();
        public int guardianSiteCount;
        public string? guardianSiteName;
        public LatLong2? guardianSiteLocation;

        public LandableBody(Game game, string bodyName, int bodyId, long systemAddress)
        {
            this.game = game;
            this.bodyName = bodyName;
            this.bodyId = bodyId;
            this.systemAddress = systemAddress;

            // see if we can get signals for this body
            this.game.journals!.search((SAASignalsFound signalsEntry) =>
            {
                if (signalsEntry.BodyName == this.bodyName)
                {
                    this.readSAASignalsFound(signalsEntry);
                    return true;
                }

                return false;
            });

            game.journals!.onJournalEntry += Journals_onJournalEntry;

            if (Game.settings.scanOne?.systemAddress == this.systemAddress && Game.settings.scanOne?.bodyId == this.bodyId)
            {
                this.scanOne = Game.settings.scanOne;
                this.scanTwo = Game.settings.scanTwo;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.game.journals != null)
                {
                    this.game.journals.onJournalEntry -= Journals_onJournalEntry;
                }
            }
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }
        private void onJournalEntry(ScanOrganic entry)
        {
            Game.log($"ScanOrganic: {entry.ScanType}: {entry.Genus} / {entry.Species}");
            this.addBioScan(entry);
        }

        private void onJournalEntry(CodexEntry entry)
        {
            Game.log($"CodexEntry: {entry.Name_Localised}");
            //  CodexEntry - to get the full name of a species

            if (this.bioScanEvent != null) this.bioScanEvent();
        }

        public void onJournalEntry(ApproachSettlement entry)
        {
            // we only care about Guardian settlements
            if (!entry.Name.StartsWith("$Ancient:")) return;

            Game.log($"ApproachSettlement: {entry.Name_Localised}");
            this.guardianSiteName = entry.Name_Localised;
            this.guardianSiteLocation = new LatLong2(entry);

            if (this.bioScanEvent != null) this.bioScanEvent();
        }

        /// <summary>
        /// Meters per 1° of Latitude or Longitude
        /// </summary>
        public double mpd
        {
            get
            {
                var bodyCircumferance = this.radius * Math.PI * 2F;
                return bodyCircumferance / 360D;
            }
        }

        public List<ScanSignal>? Signals { get; set; }
        public List<OrganicSummary>? Genuses { get; set; }

        public void readSAASignalsFound(SAASignalsFound signalsEntry)
        {
            if (signalsEntry == null) return;

            // clear signals if we're at a different body
            if (signalsEntry.SystemAddress == this.systemAddress && signalsEntry.BodyID == this.bodyId)
            {
                this.Signals = new List<ScanSignal>(signalsEntry.Signals);
                if (signalsEntry.Signals?.Count > 0)
                {
                    this.guardianSiteCount = this.Signals.Find(_ => _.Type == "$SAA_SignalType_Guardian;")?.Count ?? 0;
                }

                if (signalsEntry.Genuses?.Count > 0)
                {
                    this.Genuses = new List<OrganicSummary>();
                    foreach (var _ in signalsEntry.Genuses)
                    {
                        var newSummary = new OrganicSummary { Genus = _.Genus, Genus_Localised = _.Genus_Localised, Range = BioScan.ranges[_.Genus] };
                        // Rewards are by Species and we only know the Genus at this point
                        this.Genuses.Add(newSummary);
                    }

                    // see if we can find recent BioScans, traversing prior journal files if needed
                    game.journals!.searchDeep(
                        (ScanOrganic scan) =>
                        {
                            // look for Analyze ScanOrganics.
                            if (scan.SystemAddress == this.systemAddress && scan.Body == this.bodyId && scan.ScanType == ScanType.Analyse && !this.analysedSpecies.ContainsKey(scan.Genus))
                            {
                                this.analysedSpecies.Add(scan.Genus, scan.Species_Localised);
                            }

                            // stop if we've scanned everything
                            return this.analysedSpecies.Count == this.Genuses.Count;
                        },
                        (JournalFile journals) =>
                        {
                            // stop searching older journal files if we see we reached this system
                            return journals.search((FSDJump _) => true);
                        }
                    );
                }
            }

            if (this.bioScanEvent != null) this.bioScanEvent();
        }

        public void addBioScan(ScanOrganic entry)
        {
            if (this.scanOne != null && this.scanOne.genus != entry.Genus)
            {
                // we are changing Genus before the 3rd scan ... start over
                this.scanOne = null;
                this.scanTwo = null;
            }

            var newScan = new BioScan()
            {
                location = Status.here,
                radius = BioScan.ranges[entry.Genus],
                genus = entry.Genus,
                genusLocalized = entry.Genus_Localized,
                species = entry.Species,
                speciesLocalized = entry.Species_Localised,
                scanType = entry.ScanType,
                systemAddress = this.systemAddress,
                bodyId = this.bodyId,
                reward = -1,
            };
            Game.log($"addBioScan: {newScan}");

            if (entry.ScanType == ScanType.Log)
            {
                // replace 1st, clear 2nd
                this.scanOne = newScan;
                this.scanTwo = null;
            }
            else if (this.scanOne != null && this.scanTwo == null)
            {
                this.scanTwo = newScan;
            }
            else if (entry.ScanType == ScanType.Analyse)
            {
                if (this.scanOne != null)
                {
                    this.scanOne.scanType = ScanType.Analyse;
                    this.completedScans.Add(this.scanOne);
                    this.scanOne = null;
                }

                if (this.scanTwo != null)
                {
                    this.scanTwo.scanType = ScanType.Analyse;
                    this.completedScans.Add(this.scanTwo);
                    this.scanTwo = null;
                }

                this.completedScans.Add(newScan);

                if (!this.analysedSpecies.ContainsKey(entry.Genus))
                {
                    this.analysedSpecies.Add(entry.Genus, entry.Species_Localised);
                }
            }
            else if (this.scanOne == null && this.scanTwo == null)
            {
                this.scanOne = newScan;
                this.scanTwo = newScan;
            }

            if (this.scanOne?.reward > 0)
            {
                newScan.reward = (long)this.scanOne?.reward!;

                var summary = this.Genuses!.FirstOrDefault(_ => _.Species == newScan.species);
                if (summary != null)
                    summary.Reward = newScan.reward;
            }
            else
            {
                newScan.reward = Game.codexRef.getRewardForSpecies(entry.Species);
            }

            Game.settings.scanOne = this.scanOne;
            Game.settings.scanTwo = this.scanTwo;
            Game.settings.Save();

            if (this.bioScanEvent != null) this.bioScanEvent();
        }
    }
}
