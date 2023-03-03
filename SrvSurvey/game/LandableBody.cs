using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    delegate void BioScanEvent();

    class LandableBody
    {
        private Game game = Game.activeGame;

        public readonly string bodyName;
        public readonly int bodyId;
        public double radius;

        public event BioScanEvent bioScanEvent;
        public List<BioScan> completedScans = new List<BioScan>();
        public BioScan scanOne;
        public BioScan scanTwo;
        public Dictionary<string, string> analysedSpecies = new Dictionary<string, string>();

        //public string starSystem;
        //public long systemAddress;
        //public int bodyID;

        public LandableBody(string bodyName, int bodyId)
        {
            this.bodyName = bodyName;
            this.bodyId = bodyId;

            // see if we can get signals for this body
            this.readSAASignalsFound();

            game.journals.onJournalEntry += Journals_onJournalEntry; ;
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry)
        {
            // ignore
        }

        private void onJournalEntry(ScanOrganic entry)
        {
            Game.log($"ScanOrganic: {entry.ScanType}: {entry.Genus} / {entry.Species}");
            this.addBioScan(entry);
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

        public List<ScanSignal> Signals { get; set; }
        public List<ScanGenus> Genuses { get; set; }
        //public List<ScanGenus> Genuses { get; set; }

        public void readSAASignalsFound()
        {
            // find the signals
            var signalsEntry = Game.activeGame.journals.FindEntryByType<SAASignalsFound>(-1, true);
            if (signalsEntry != null)
            {
                this.Signals = new List<ScanSignal>(signalsEntry.Signals);
                this.Genuses = new List<ScanGenus>(signalsEntry.Genuses);
            }

            // see if we can find recent BioScans
            int idx = 0;
            do
            {
                var scan = game.journals.FindEntryByType<ScanOrganic>(idx, false);
                if (scan == null)
                {
                    // no more entries in this file
                    break;
                }

                if (scan.ScanType == ScanType.Analyse && !this.analysedSpecies.ContainsKey(scan.Genus))
                {
                    this.analysedSpecies.Add(scan.Genus, scan.Species_Localised);
                }

                idx = game.journals.Entries.IndexOf(scan) + 1;
            } while (idx < game.journals.Count);

            // TODO: scan across previous journal files?
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
                location = new LatLong2(game.status),
                radius = BioScan.ranges[entry.Genus],
                genus = entry.Genus,
                genusLocalized = entry.Genus_Localized,
                species = entry.Species,
                speciesLocalized = entry.Species_Localised,
                scanType = entry.ScanType,
            };

            Game.log($"Scan: {newScan}");

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
                this.scanOne.scanType = ScanType.Analyse;
                this.completedScans.Add(this.scanOne);
                this.scanOne = null;

                this.scanTwo.scanType = ScanType.Analyse;
                this.completedScans.Add(this.scanTwo);
                this.scanTwo = null;

                this.completedScans.Add(newScan);

                this.analysedSpecies.Add(entry.Genus, entry.Species_Localised);
            }

            if (this.bioScanEvent != null) this.bioScanEvent();
        }

    }
}
