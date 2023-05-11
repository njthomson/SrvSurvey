using SrvSurvey.canonn;
using SrvSurvey.units;
using System;

namespace SrvSurvey.game
{
    delegate void BioScanEvent();

    class LandableBody : IDisposable
    {
        private readonly Game game;
        public readonly string systemName;
        public readonly string bodyName;
        public readonly int bodyId;
        public long systemAddress;
        public double radius;
        public BodyData data;

        public event BioScanEvent? bioScanEvent;
        public List<BioScan> completedScans = new List<BioScan>();
        public BioScan? scanOne { get => game.cmdr.scanOne; }
        public BioScan? scanTwo { get => game.cmdr.scanTwo; }
        public Dictionary<string, string> analysedSpecies = new Dictionary<string, string>();
        public GuardianSiteData siteData;
        public HashSet<string> settlements = new HashSet<string>();

        public LandableBody(Game game, string systemName, string bodyName, int bodyId, long systemAddress, double radius)
        {
            if (radius == 0) throw new Exception("Bad radius!");

            this.game = game;
            this.systemName = systemName;
            this.bodyName = bodyName;
            this.bodyId = bodyId;
            this.systemAddress = systemAddress;
            this.radius = radius;
            this.data = BodyData.Load(this);
            this.data.updateScanProgress();
            this.data.lastVisited = DateTimeOffset.UtcNow;

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

            // see if we can find Guardian sites?
            this.findGuardianSites();

            game.journals!.onJournalEntry += Journals_onJournalEntry;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.data != null)
            {
                this.data.Save();
                this.data = null;
            }

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
            Game.log($"CodexEntry: {entry.Name_Localised}, lat/long: {entry.Latitude},{entry.Longitude}");
            //  CodexEntry - to get the full name of a species
            foreach(var genusName in data.organisms.Keys)
            {
                if (entry.Name.StartsWith(genusName.Replace("_Genus_Name;", "")))
                {
                    var organism = data.organisms[genusName];
                    var speciesName = Util.getSpeciesPrefix(entry.Name);
                    var reward = Game.codexRef.getRewardForSpecies(entry.Name);
                    Game.log($"Matched Codex to Organism: {genusName} ({speciesName}: {Util.credits(reward)})");

                    if (organism.reward == 0)
                        organism.reward = reward;

                    if (string.IsNullOrEmpty(organism.variant))
                    {
                        organism.variant = entry.Name;
                        organism.variantLocalized = entry.Name_Localised;
                    }

                    var idx = entry.Name_Localised.LastIndexOf(" - ");
                    if (string.IsNullOrEmpty(organism.species) && idx > 0)
                    {
                        organism.species = speciesName + "Name;";
                        organism.speciesLocalized = entry.Name_Localised.Substring(0, idx);
                    }

                    data.updateScanProgress();
                    this.data.Save();
                    break;
                }
            }

            if (this.bioScanEvent != null) this.bioScanEvent();
        }

        public void onJournalEntry(ApproachSettlement entry)
        {
            // we only care about Guardian settlements
            if (!entry.Name.StartsWith("$Ancient:")) return;

            Game.log($"ApproachSettlement: {entry.Name_Localised}");
            this.siteData = GuardianSiteData.Load(entry);
            if (this.siteData.location.Lat == 0)
            {
                // Legacy mode bug - lat/long is occasionally missing
                this.siteData.location = Status.here.clone();
                this.siteData.Save();
            }

            if (this.bioScanEvent != null) this.bioScanEvent();
        }

        public void findGuardianSites()
        {
            this.settlements.Clear();

            decimal dist = decimal.MaxValue;
            ApproachSettlement? nearestSettlement = null;

            // look for ApproachSettlements against this body in this journal
            game.journals!.searchDeep(
                (ApproachSettlement _) =>
                {
                    if (_.BodyName == this.bodyName && _.Name.StartsWith("$Ancient:") && _.Latitude != 0)
                    {
                        var filename = GuardianSiteData.getFilename(_);
                        this.settlements.Add(filename);

                        // if site is within 20km - make it the active site
                        var td = new TrackingDelta(this.radius, _);
                        if (td.distance < 20000 && td.distance < dist)
                        {
                            dist = td.distance;
                            nearestSettlement = _;
                        }
                    }

                    return false;
                },
                // stop searching older journal files if we see we reached this system
                (JournalFile journals) => journals.search((FSDJump _) => true));
            Game.log($"Found {this.settlements.Count} Guardian sites on: {this.bodyName} / " + string.Join(",", this.settlements));

            if (nearestSettlement != null)
            {
                Game.log($"Nearest site '{nearestSettlement.Name_Localised}' is {Util.metersToString(dist)} away, location: {new LatLong2(nearestSettlement)} vs Here: {Status.here}");
                this.siteData = GuardianSiteData.Load(nearestSettlement);

                this.siteData.lastVisited = DateTimeOffset.UtcNow;
                this.siteData.Save();
            }
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

        public void readSAASignalsFound(SAASignalsFound signalsEntry)
        {
            if (signalsEntry == null) return;

            // clear signals if we're at a different body
            if (signalsEntry.SystemAddress == this.systemAddress && signalsEntry.BodyID == this.bodyId)
            {
                this.Signals = new List<ScanSignal>(signalsEntry.Signals);
                if (signalsEntry.Signals?.Count > 0)
                {
                    // TODO: !!
                    var count = this.Signals.Find(_ => _.Type == "$SAA_SignalType_Guardian;")?.Count ?? 0;
                    Game.log($"What to do with {count} guardian sites?");
                }

                if (signalsEntry.Genuses?.Count > 0)
                {
                    foreach (var genus in signalsEntry.Genuses)
                    {
                        // Rewards are by Species and we only know the Genus at this point
                        this.data.addScanGenus(genus);
                    }

                    // see if we can find recent BioScans, traversing prior journal files if needed
                    game.journals!.searchDeep(
                        (ScanOrganic scan) =>
                        {
                            // look for Analyze ScanOrganics.
                            if (scan.SystemAddress == this.systemAddress && scan.Body == this.bodyId)
                            {
                                this.data.addScanOrganic(scan);
                            }

                            // stop if we've scanned everything
                            return this.data.countAnalyzed == this.data.countOrganisms;
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
            // are we changing organism before the 3rd scan?
            var hash = $"{entry.SystemAddress}|{entry.Body}|{entry.Species}";
            if (game.cmdr.lastOrganicScan != null && hash != game.cmdr.lastOrganicScan)
            {
                // yes - mark current scans as abandoned and start over
                if (game.cmdr.scanOne != null)
                {
                    game.cmdr.scanOne.status = BioScan.Status.Abandoned;
                    data.bioScans.Add(game.cmdr.scanOne);
                }
                if (game.cmdr.scanTwo != null)
                {
                    game.cmdr.scanTwo.status = BioScan.Status.Abandoned;
                    data.bioScans.Add(game.cmdr.scanTwo);
                }

                game.cmdr.scanOne = null;
                game.cmdr.scanTwo = null;
            }
            game.cmdr.lastOrganicScan = hash;

            // add/track organism data
            var organism = this.data.addScanOrganic(entry);

            // add a new bio-scan - assuming we don't have one at this position already
            var bioScan = new BioScan
            {
                location = Status.here.clone(),
                genus = entry.Genus,
                species = entry.Species,
                radius = organism.range,
                status = BioScan.Status.Active,
            };
            Game.log($"new bio scan: {bioScan}");

            if (entry.ScanType == ScanType.Log)
            {
                // replace 1st, clear 2nd
                game.cmdr.scanOne = bioScan;
                game.cmdr.scanTwo = null;
            }
            else if (game.cmdr.scanOne != null && game.cmdr.scanTwo == null)
            {
                // populate 2nd
                game.cmdr.scanTwo = bioScan;
            }
            else if (entry.ScanType == ScanType.Analyse)
            {
                // track rewards successful scan 
                game.cmdr.organicRewards += organism.reward;
                var scanned = new ScannedOrganic
                {
                    reward = organism.reward,
                    genus = entry.Genus,
                    genusLocalized = entry.Genus_Localized,
                    species = entry.Species,
                    speciesLocalized = entry.Species_Localised,
                    bodyName = this.bodyName,
                    bodyId = this.bodyId,
                    system = game.cmdr.currentSystem,
                    systemAddress = this.systemAddress,
                };
                game.cmdr.scannedOrganics.Add(scanned);

                // track scans as completed
                if (game.cmdr.scanOne != null)
                {
                    // (this can happen if there are 2 copies of Elite running at the same time)
                    game.cmdr.scanOne!.status = BioScan.Status.Complete;
                    data.bioScans.Add(game.cmdr.scanOne);
                    game.cmdr.scanTwo!.status = BioScan.Status.Complete;
                    data.bioScans.Add(game.cmdr.scanTwo);
                }
                bioScan.status = BioScan.Status.Complete;
                data.bioScans.Add(bioScan);

                // and clear state
                game.cmdr.lastOrganicScan = null;
                game.cmdr.scanOne = null;
                game.cmdr.scanTwo = null;
            }

            data.updateScanProgress();
            this.data.Save();
            this.game.cmdr.Save();
            if (this.bioScanEvent != null) this.bioScanEvent();
        }

        public OrganicSummary? currentOrganism
        {
            get
            {
                if (game.cmdr.scanOne == null)
                    return null;

                return data.organisms[game.cmdr.scanOne.genus!];
            }
        }
    }
}
