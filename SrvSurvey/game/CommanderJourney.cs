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

        public static CommanderJourney? Load(string fid, string filename)
        {
            var folder = Path.Combine(CommanderJourney.dataFolder, fid);
            Directory.CreateDirectory(folder);

            var filepath = Path.Combine(folder, $"{filename}.json");

            Game.log($"Loading journey: /journey/{fid}/{filename}.json");
            var journey = Data.Load<CommanderJourney>(filepath)!;
            if (journey == null) return null;

            // prep certain properties after loading
            journey.currentSystem = journey.visitedSystems.LastOrDefault();

            return journey;
        }

        public CommanderJourney() { }

        public CommanderJourney(string fid, DateTimeOffset timestamp)
        {
            this.fid = fid;
            this.startTime = timestamp.AddMilliseconds(-10);
            this.watermark = timestamp.AddMilliseconds(-10);

            var folder = Path.Combine(CommanderJourney.dataFolder, fid);
            Directory.CreateDirectory(folder);

            var filename = timestamp.ToIsoFileString() + ".json";
            this.filepath = Path.Combine(folder, filename);
            if (File.Exists(filepath))
                throw new Exception($"Cannot create: ./journey/{fid}/{filename}");

            Game.log($"Journey.load: Creating a new journey: '{name}' ({filename})");
        }


        /// <summary>
        /// Returns null if name is valid, otherwise a reason why it is not.
        /// </summary>
        public static string? validate(string fid, string name, FSDJump? startingJump = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Name is blank";

            // make sure name has no filename illegal characters
            //if (Util.hasIllegalFilenameCharacters(name))
            //    return "Name cannot contain characters \\ / : * ? \" < > |";

            // TODO: open existing files to make sure name is not already in use

            // confirm no journey started at the same time
            if (startingJump != null)
            {
                var folder = Path.Combine(CommanderJourney.dataFolder, fid);
                var filename = startingJump.timestamp.ToIsoFileString() + ".json";
                var filepath = Path.Combine(folder, filename);
                if (File.Exists(filepath))
                    return "A prior journey started from there";
            }

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

        public string startingJournal;
        public DateTimeOffset startTime;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset? endTime;

        /// <summary> The time stamp from the last journal entry we processed </summary>
        public DateTimeOffset watermark;

        // keep this last in the .json
        public List<SystemStats> visitedSystems = new();

        #endregion

        [JsonIgnore]
        public bool postProcessing;

        [JsonIgnore]
        public SystemStats? currentSystem;

        #region batch journal processing

        /*
        public void doProcessPastJournals(JournalFile journal)
        {
            Game.log($"doProcessPastJournals: starting from: {journal.Entries.FirstOrDefault()?.timestamp}");
            if (this.postProcessing) throw new Exception("Why are we double processing?");
            this.postProcessing = true;

            try
            {
                journal.walkDeep(false, (entry) =>
                {
                    // ignore anything before ...
                    if (entry.timestamp >= this.watermark)
                    {
                        this.onJournalEntry((dynamic)entry);
                        this.watermark = entry.timestamp;
                    }
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
        */
        public Task doCatchup(JournalFile journal)
        {
            var started = DateTime.Now;
            Game.log($"!! doCatchup: starting from watermark: {this.watermark}, {journal.Entries.FirstOrDefault()?.timestamp}");
            if (this.postProcessing) throw new Exception("Why are we double processing?");
            this.postProcessing = true;

            return Task.Run(() =>
            {
                try
                {
                    // rewind to the water mark
                    var startingJournal = journal.walkDeep(true, (entry) =>
                    {
                        return entry.timestamp < this.watermark;
                    });

                    // now process forwards from that point
                    var first = true;
                    startingJournal.walkDeep(false, (entry) =>
                    {
                        // ignore anything before ...
                        if (first && entry.timestamp <= this.watermark) return false;
                        first = false;
                        if (entry.timestamp < this.watermark) return false;

                        this.onJournalEntry((dynamic)entry);
                        this.watermark = entry.timestamp;
                        return false;
                    });

                }
                finally
                {
                    this.Save();
                    Game.log($"!! doCatchup: complete: watermark: {this.watermark}, duration: {DateTime.Now - started}");
                    this.postProcessing = false;
                }
            });
        }

        #endregion

        #region journal processing

        public void processJournalEntry(IJournalEntry entry, bool autoSave)
        {
            // don't process anything here if we're catching up - it'll get to it eventually
            if (this.postProcessing) return;

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

        private void onJournalEntry(StartJump entry)
        {
            // close out the current system
            if (currentSystem != null)
            {
                currentSystem.departed = entry.timestamp;
                currentSystem = null;
            }
        }

        private void onJournalEntry(FSSDiscoveryScan entry)
        {
            if (currentSystem == null) return;

            // HONK!
            currentSystem.count.bodyCount = entry.BodyCount;
        }

        private void onJournalEntry(FSSAllBodiesFound entry)
        {
            if (currentSystem == null) return;

            // FSS complete
            currentSystem.count.bodyCount = entry.Count;
        }

        public void onJournalEntry(Scan entry)
        {
            if (currentSystem == null) return;

            // ignore asteroid clusters
            if (entry.PlanetClass == null && entry.StarType == null) return;

            // Effectively (but not entirely) FSS scans
            currentSystem.count.bodyScans++;

            if (entry.StarType != null)
                currentSystem.count.stars++;

            var reward = Util.GetBodyValue(entry, false);
            currentSystem.count.rewardExp += reward;
        }

        public void onJournalEntry(SAAScanComplete entry)
        {
            if (currentSystem == null) return;

            // DSS complete
            currentSystem.count.dss++;

            // NOTE: When re-processing journal entries, it's possible SystemData is more populated that we know about at this point :(

            var sysData = SystemData.Load(currentSystem.starRef.name, currentSystem.starRef.id64, this.fid, false);
            var body = sysData?.bodies.Find(b => b.id == entry.BodyID);
            if (sysData == null || body == null) throw new Exception("Why no body?");

            // increment by difference between the FSS and DSS reward (and we are going to assume they FSS'd first)
            var rewardFss = Util.GetBodyValue(body, false, false);
            var rewardDss = Util.GetBodyValue(body, true, entry.ProbesUsed <= entry.EfficiencyTarget);
            var delta = rewardDss - rewardDss;
            currentSystem.count.rewardExp += delta;

            // UGH. Perhaps we should do this upon exiting a system?
            //// have we mapped all mappable bodies in the system?
            //var countMappableBodies = sysData.bodies.Count(_ => _.type == SystemBodyType.LandableBody || _.type == SystemBodyType.SolidBody || _.type == SystemBodyType.Giant);
            //if (this.dssBodyCount == countMappableBodies && !this.dssAllBodies)
            //{
            //    this.dssAllBodies = true;
            //    if (Game.activeGame?.systemData == this)
            //    {
            //        var bonus = countMappableBodies * 10_000;
            //        Game.activeGame.cmdr.applyExplReward(bonus, $"DSS mapped all valid bodies");
            //    }
            //}
        }

        private void onJournalEntry(Touchdown entry)
        {
            if (currentSystem == null) return;

            currentSystem.count.touchdown++;

            // track how many times we land on each body
            currentSystem.landedOn ??= new();
            var shortName = entry.Body.Replace(entry.StarSystem, "").Replace(" ", "");
            if (!currentSystem.landedOn.ContainsKey(shortName)) currentSystem.landedOn[shortName] = new();
            currentSystem.landedOn[shortName]++;
        }

        private void onJournalEntry(FSSBodySignals entry)
        {
            if (currentSystem == null) return;
            currentSystem.saaSignals ??= new();

            // Biological, Geological, Human, Thargoid, etc
            foreach (var signal in entry.Signals)
            {
                var key = signal.Type.Replace("$SAA_SignalType_", "").Replace(";", "");
                if (!currentSystem.saaSignals.ContainsKey(key)) currentSystem.saaSignals[key] = 0;
                currentSystem.saaSignals[key] += signal.Count;
            }
        }

        private void onJournalEntry(FSSSignalDiscovered entry)
        {
            if (currentSystem == null) return;
            currentSystem.fssSignals ??= new();

            var key = entry.SignalType;
            if (!currentSystem.fssSignals.ContainsKey(key)) currentSystem.fssSignals[key] = 0;
            currentSystem.fssSignals[key] += 1;
        }


        public void onJournalEntry(CodexEntry entry)
        {
            if (currentSystem == null) return;
            currentSystem.codexScanned ??= new();

            var alreadyScanned = currentSystem.codexScanned.Contains(entry.EntryID);

            // get a count of distinct things scanned
            currentSystem.codexScanned.Add(entry.EntryID);

            // new for galactic region
            if (entry.IsNewEntry)
            {
                currentSystem.count.codexNew += 1;

                currentSystem.codexNew ??= new();
                currentSystem.codexNew.Add(entry.Name_Localised);
            }

            // TODO: track cmdr first scans here? Or just join against that data when viewing?

            // count per sub-category, eg: "Organic structures"
            if (!alreadyScanned && !string.IsNullOrWhiteSpace(entry.SubCategory_Localised))
            {
                currentSystem.subCats ??= new();
                if (!currentSystem.subCats.ContainsKey(entry.SubCategory_Localised)) currentSystem.subCats[entry.SubCategory_Localised] = new();
                currentSystem.subCats[entry.SubCategory_Localised] += 1;
            }
        }

        public void onJournalEntry(ScanOrganic entry)
        {
            if (currentSystem == null) return;

            // count the 3rd and final scan
            if (entry.ScanType == ScanType.Analyse)
            {
                currentSystem.count.organic++;

                var species = Game.codexRef.matchFromSpecies(entry.Species);
                currentSystem.count.rewardBio += species.reward;
            }

            // TODO: Maybe count the others?
        }

        public void onJournalEntry(Screenshot entry)
        {
            if (currentSystem == null) return;

            currentSystem.count.screenshots += 1;
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

                countBodiesLanded += sys.landedOn?.Count ?? 0;
                countTotalLandings += sys.landedOn?.Values.Sum() ?? 0;
                countCodexScans += sys.codexScanned?.Count ?? 0;
                if (sys.fssAllBodies) countFssComplete += 1;

                // TODO: saaSignals
                // TODO: fssSignals
                if (sys.subCats != null)
                {
                    foreach (var (key, value) in sys.subCats)
                    {
                        if (!totalSubCats.ContainsKey(key)) totalSubCats[key] = 0;
                        totalSubCats[key] += value;
                    }
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
                { "Screenshots taken:", totalCounts.screenshots.ToString("N0") },
                { "Organisms scanned:", totalCounts.organic.ToString("N0") },
                { "Exobiology rewards:", totalCounts.rewardBio.ToString("N0") },
                { "Exploration rewards:", "~" + totalCounts.rewardExp.ToString("N0") },
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
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public DateTimeOffset? departed;

            public Counts count;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public Dictionary<string, int>? landedOn;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public HashSet<long>? codexScanned;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public HashSet<string>? codexNew;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public Dictionary<string, int>? subCats;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public Dictionary<string, int>? saaSignals;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public Dictionary<string, int>? fssSignals;

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
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int bodyScans;
                /// <summary> Count of bodies DSS scanned </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int dss;
                /// <summary> Count of NEW codex scans </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int codexNew;
                /// <summary> Count 3rd ScanOrganic events </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int organic;
                /// <summary> Count 3rd ScanOrganic events </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int touchdown;
                /// <summary> Count of bodies in system </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int bodyCount;
                /// <summary> Count of screenshots taken </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int screenshots;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int notes;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int rewardBio;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int rewardExp;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
                public int stars;

                public override string ToString()
                {
                    return $"bodyScans:{bodyScans}, dss:{dss}, codexNew:{codexNew}, organic:{organic}, touchdown:{touchdown}, bodyCount:{bodyCount}, screenshots:{screenshots}, notes:{notes}, rewardBio:{rewardBio}, rewardExp:{rewardExp}, stars:{stars}";
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
                        screenshots = c1.screenshots + c2.screenshots,
                        notes = c1.notes + c2.notes,
                        rewardBio = c1.rewardBio + c2.rewardBio,
                        rewardExp = c1.rewardExp + c2.rewardExp,
                        stars = c1.stars + c2.stars,
                    };
                }
            }
        }

        #endregion
    }
}
