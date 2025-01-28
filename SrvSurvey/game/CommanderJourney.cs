using Newtonsoft.Json;
using SrvSurvey.units;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey.game
{
    /// <summary>
    /// Represents a named journey/expedition, recording statistics from a concrete start/stop time frame.
    /// </summary>
    internal class CommanderJourney : Data
    {
        #region static loading and house keeping/crud code

        private static readonly string dataFolder = Path.Combine(Program.dataFolder, $"journey");

        public static CommanderJourney Load(string fid, string filename)
        {
            var folder = Path.Combine(CommanderJourney.dataFolder, fid);
            Directory.CreateDirectory(folder);

            var filepath = Path.Combine(folder, $"{filename}.json");

            Game.log($"Loading journey: /journey/{fid}/{filename}.json");
            var journey = Data.Load<CommanderJourney>(filepath)!;
            if (journey == null) throw new Exception($"Cannot find: ./journey/{fid}/{filename}.json");

            // prep certain properties after loading
            journey.currentSystem = journey.visitedSystems.Last();

            return journey;
        }

        public static CommanderJourney Create(string fid, string cmdr, string name, FSDJump startingJump)
        {
            var folder = Path.Combine(CommanderJourney.dataFolder, fid);
            Directory.CreateDirectory(folder);

            var filename = startingJump.timestamp.ToIsoFileString() + ".json";
            var filepath = Path.Combine(folder, filename);
            if (File.Exists(filepath))
                throw new Exception($"Cannot create: ./journey/{fid}/{filename}");

            Game.log($"Journey.load: Creating a new journey: '{name}' ({filename})");
            var journey = new CommanderJourney()
            {
                filepath = filepath,
                fid = fid,
                commander = cmdr,
                name = name,

                watermark = startingJump.timestamp,
            };

            return journey;
        }


        /// <summary>
        /// Returns null if name is valid, otherwise a reason why it is not.
        /// </summary>
        public static string? nameIsValid(string fid, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Name is blank";

            // make sure name has no filename illegal characters
            //if (Util.hasIllegalFilenameCharacters(name))
            //    return "Name cannot contain characters \\ / : * ? \" < > |";

            // TODO: open existing files to make sure name is not already in use
            //var folder = Path.Combine(CommanderJourney.dataFolder, fid);
            //if (Directory.GetFiles(folder, $"{name}.json").Any())
            //    return "That name has been used before";

            return null;
        }

        /*public void rename(string newName)
        {
            Game.log($"Journey.rename: '{this.name}' => '{newName}'");
            var originalFilepath = this.filepath;

            // first: update filepath and save
            this.filepath = Path.Combine(CommanderJourney.dataFolder, $"{newName}.json");
            this.name = newName;
            this.Save();

            // then: delete the old file
            if (File.Exists(originalFilepath))
                File.Delete(originalFilepath);
        }*/

        #endregion

        #region stored data members

        public string fid;
        public string commander;
        public string name;
        public string description;
        /// <summary> The time stamp from the last journal entry we processed </summary>
        public DateTimeOffset watermark;

        // keep this last in the .json
        public List<SystemStats> visitedSystems = new();

        #endregion

        [JsonIgnore]
        public bool postProcessing;

        [JsonIgnore]
        public SystemStats currentSystem;

        #region journal processing

        public void doProcessPastJournals(JournalFile journal)
        {
            Game.log($"doProcessPastJournals: starting from: {journal.Entries.FirstOrDefault()?.timestamp}");
            if (this.postProcessing) throw new Exception("Why are we double processing?");
            this.postProcessing = true;

            try
            {
                journal.walkDeep(false, (entry) =>
                {
                    this.processJournalEntry(entry, false);
                    return false;
                });

                this.Save();
            }
            finally
            {
                this.postProcessing = false;
            }

            Game.log($"doProcessPastJournals: complete");
        }

        public void processJournalEntry(IJournalEntry entry, bool autoSave)
        {
            // ignore anything before ...
            if (entry.timestamp < this.watermark) return;

            this.onJournalEntry((dynamic)entry);

            this.watermark = entry.timestamp;
            if (autoSave) this.Save();
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It is necessary")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "It is necessary")]
        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(FSDJump entry)
        {
            // close out the current system
            if (currentSystem != null)
            {
                currentSystem.departed = entry.timestamp;
            }

            currentSystem = new SystemStats(entry);
            visitedSystems.Add(currentSystem);
        }

        private void onJournalEntry(FSSDiscoveryScan entry)
        {
            // HONK!
            currentSystem.count.honks += 1;
            currentSystem.count.bodyCount = entry.BodyCount;
        }

        private void onJournalEntry(FSSAllBodiesFound entry)
        {
            // FSS complete
            currentSystem.count.bodyCount = entry.Count;
        }

        public void onJournalEntry(Scan entry)
        {
            // Effectively (but not entirely) FSS scans
            currentSystem.count.bodyScans++;
        }

        public void onJournalEntry(SAAScanComplete entry)
        {
            // DSS complete
            currentSystem.count.dss++;
        }

        private void onJournalEntry(Touchdown entry)
        {
            currentSystem.count.touchdown++;

            // track how many times we land on each body
            var shortName = entry.Body.Replace(entry.StarSystem, "").Replace(" ", "");
            if (!currentSystem.landedOn.ContainsKey(shortName)) currentSystem.landedOn[shortName] = new();
            currentSystem.landedOn[shortName] += 1;
        }

        public void onJournalEntry(CodexEntry entry)
        {
            var alreadyScanned = currentSystem.codexScanned.Contains(entry.EntryID);

            // get a count of distinct things scanned
            currentSystem.codexScanned.Add(entry.EntryID);

            // new for galactic region
            if (entry.IsNewEntry)
                currentSystem.count.codexNew += 1;

            // TODO: track cmdr first scans here? Or just join against that data when viewing?

            // count per sub-category, eg: "Organic structures"
            if (!alreadyScanned && !string.IsNullOrWhiteSpace(entry.SubCategory_Localised))
            {
                if (!currentSystem.subCats.ContainsKey(entry.SubCategory_Localised)) currentSystem.subCats[entry.SubCategory_Localised] = new();
                currentSystem.subCats[entry.SubCategory_Localised] += 1;
            }
        }

        public void onJournalEntry(ScanOrganic entry)
        {
            // count the 3rd and final scan
            if (entry.ScanType == ScanType.Analyse)
                currentSystem.count.organic++;

            // TODO: Maybe count the others?
        }

        #endregion

        #region stats calculations

        public Dictionary<string, string> getQuickStats()
        {
            var systemNames = new HashSet<string>();
            var totalCounts = new SystemStats.Counts();
            var totalSubCats = new Dictionary<string, int>();
            double totalDistance = 0;
            int countBodiesLanded = 0;
            int countTotalLandings = 0;
            int countCodexScans = 0;
            int countFssComplete = 0;

            StarRef lastStarRef = this.visitedSystems.First().starRef;
            foreach (var sys in visitedSystems)
            {
                systemNames.Add(sys.starRef.name);
                totalCounts += sys.count;

                totalDistance += sys.starRef.getDistanceFrom(lastStarRef);
                lastStarRef = sys.starRef;

                countBodiesLanded += sys.landedOn.Count;
                countTotalLandings += sys.landedOn.Values.Sum();
                countCodexScans += sys.codexScanned.Count;
                if (sys.fssAllBodies) countFssComplete += 1;

                foreach (var (key, value) in sys.subCats)
                {
                    if (!totalSubCats.ContainsKey(key)) totalSubCats[key] = 0;
                    totalSubCats[key] += value;
                }
            }

            var stats = new Dictionary<string, string>()
            {
                { "FSD jumps:", visitedSystems.Count.ToString("N0") },
                { "Total distance:", totalDistance.ToString("N1") + "ly" },
                { "Systems visited:", systemNames.Count.ToString("N0") },
                { "Systems FSS completed:", countFssComplete.ToString("N0") },
                { "Bodies scanned:", totalCounts.bodyScans.ToString("N0") },
                { "Total DSS:", totalCounts.dss.ToString("N0") },
                { "Landed on bodies:", countBodiesLanded.ToString("N0") },
                { "Count touchdowns:", countTotalLandings.ToString("N0") },
                { "Organisms scanned:", totalCounts.organic.ToString("N0") },
                { "Codex scans:", countCodexScans.ToString("N0") },
                { "NEW Codex scans:", totalCounts.codexNew.ToString("N0") },
            };

            foreach (var (key, value) in totalSubCats)
                stats.Add($"Category: {key}:", value.ToString("N0"));

            return stats;
        }

        #endregion

        #region Timeline entry classes

        public class SystemStats
        {
            public StarRef starRef;
            public DateTimeOffset arrived;
            public DateTimeOffset? departed;

            public Counts count;
            public Dictionary<string, int> landedOn = new();
            public HashSet<long> codexScanned = new();
            public Dictionary<string, int> subCats = new();

            public SystemStats() { }

            public SystemStats(FSDJump entry)
            {
                this.starRef = StarRef.from(entry);
                this.arrived = entry.timestamp;
            }

            public override string ToString()
            {
                return $"{starRef.name} ({arrived} ~ {arrived})";
            }

            [JsonIgnore]
            public bool fssAllBodies => count.bodyCount == count.bodyScans;

            public struct Counts
            {
                /// <summary> Count of bodies scanned </summary>
                public int bodyScans;
                /// <summary> Count of bodies DSS scanned </summary>
                public int dss;
                /// <summary> Count of NEW codex scans </summary>
                public int codexNew;
                /// <summary> Count 3rd ScanOrganic events </summary>
                public int organic;
                /// <summary> Count 3rd ScanOrganic events </summary>
                public int touchdown;
                /// <summary> Count of bodies in system </summary>
                public int bodyCount;
                /// <summary> Count of honks (FSSDiscoveryScan) </summary>
                public int honks; // <-- maybe not worth it?

                public override string ToString()
                {
                    return $"bodyScans: {bodyScans}, dss: {dss}, codexNew: {codexNew}, organic: {organic}, touchdown: {touchdown}";
                }

                public static Counts operator +(Counts c1, Counts c2)
                {
                    return new Counts
                    {
                        bodyScans = c1.bodyScans + c2.bodyScans,
                        dss = c1.dss + c2.dss,
                        codexNew = c1.codexNew + c2.codexNew,
                        organic = c1.organic + c2.organic,
                        touchdown = c1.touchdown + c2.touchdown,
                        bodyCount = c1.bodyCount + c2.bodyCount,
                        honks = c1.honks + c2.honks,
                    };
                }
            }
        }

        /* MAYBE NOT ?
        
        private void appendTimeEntry(BaseTimeEntry timeEntry)
        {

        }

        abstract class BaseTimeEntry
        {
            public DateTimeOffset time;

            public abstract string text { get; }
        }

        class FsdJumpEntry : BaseTimeEntry
        {
            public long address;
            public string name;
            public override string text => $"Jumped to {name}";
        }

        class LandedEntry : BaseTimeEntry
        {
            public int id;
            public string name;
            public override string text => $"Landed on {name}";
        }
        */

        #endregion

    }
}
