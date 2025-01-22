using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Diagnostics;
using System.Reflection;

namespace SrvSurvey
{
    class JournalFile
    {

        public static string journalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous\");

        private static readonly Dictionary<string, Type> typeMap;

        static JournalFile()
        {
            // build a map of all types derived from JournalEntry
            typeMap = new Dictionary<string, Type>();

            var journalEntryType = typeof(JournalEntry);
            var journalDerivedTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(_ => journalEntryType.IsAssignableFrom(_));

            foreach (var journalType in journalDerivedTypes)
                typeMap.Add(journalType.Name, journalType);
        }

        public List<IJournalEntry> Entries { get; } = new List<IJournalEntry>();

        protected StreamReader reader;
        public readonly string filepath;
        public readonly DateTime timestamp;
        public string? cmdrName { get; private set; }
        public string? cmdrFid { get; private set; }
        public readonly bool isOdyssey;

        public bool isShutdown { get; private set; }

        public JournalFile(string filepath, string? targetCmdr = null)
        {
            Game.log($"Reading: {Path.GetFileName(filepath)}");

            this.filepath = filepath;
            this.timestamp = File.GetLastWriteTime(filepath);

            this.reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            this.readEntries(targetCmdr);


            // assume Odyssey if we don't have the Fileheader line yet.
            this.isOdyssey = true;
            if (this.Entries.Count > 0 && this.Entries[0].@event == nameof(Fileheader))
                this.isOdyssey = ((Fileheader)this.Entries[0]).Odyssey;
        }

        public int Count { get => this.Entries.Count; }

        public void readEntries(string? targetCmdr = null)
        {
            while (!this.reader.EndOfStream)
            {
                var entry = this.readEntry();

                // stop early if not target cmdr
                if (targetCmdr != null && entry is Commander && ((Commander)entry).Name != targetCmdr)
                    break;
            }
        }

        protected virtual JournalEntry? readEntry()
        {
            // read next entry, add to list or skip if it's blank
            var entry = this.parseNextEntry();

            if (entry != null)
            {
                this.Entries.Add(entry);
                if (entry.@event == nameof(Shutdown) && !Program.useLastIfShutdown) this.isShutdown = true;

                if (this.cmdrName == null && entry.@event == nameof(Commander))
                {
                    var commanderEntry = (Commander)entry;
                    this.cmdrName = commanderEntry.Name;
                    this.cmdrFid = commanderEntry.FID;
                }
            }

            return entry;
        }

        private JournalEntry? parseNextEntry()
        {
            try
            {
                var json = reader.ReadLine()!;
                JToken entry = JsonConvert.DeserializeObject<JToken>(json)!;
                if (entry == null) return null;

                var eventName = entry["event"]!.Value<string>()!;
                if (typeMap.ContainsKey(eventName))
                {
                    return entry.ToObject(typeMap[eventName]) as JournalEntry;
                }
            }
            catch (Exception ex)
            {
                Game.log($"parseNextEntry error: {ex.Message}\n{ex.StackTrace}");
            }

            // ignore anything else
            return null;
        }

        public T? FindEntryByType<T>(int index, bool searchUp) where T : IJournalEntry
        {
            if (index == -1) index = this.Entries.Count - 1;

            int n = index;
            while (n >= 0 && n < this.Entries.Count)
            {
                if (this.Entries[n].GetType() == typeof(T))
                {
                    return (T)this.Entries[n];
                }
                n += searchUp ? -1 : +1;
            }

            return default(T);
        }

        public bool search<T>(Func<T, bool> func) where T : IJournalEntry
        {
            int idx = this.Count - 1;
            do
            {
                var entry = this.FindEntryByType<T>(idx, true);
                if (entry == null)
                {
                    // no more entries in this file
                    break;
                }

                // do something with the entry, exit if finished
                var finished = func(entry);
                if (finished) return finished;

                // otherwise keep going
                idx = this.Entries.IndexOf(entry) - 1;
            } while (idx >= 0);

            // if we run out of entries, we don't know if we're necessarily finished
            return false;
        }

        public void searchDeep<T>(Func<T, bool> func, Func<JournalFile, bool>? finishWhen = null) where T : JournalEntry
        {
            var count = 0;
            var journal = this;

            // search older journals
            while (journal != null)
            {
                ++count;
                // search this journal
                var finished = journal.search(func);
                if (finished) break;

                if (finishWhen != null)
                {
                    finished = finishWhen(journal);
                    if (finished) break;
                }

                var priorFilepath = JournalFile.getCommanderJournalBefore(this.cmdrName, this.isOdyssey, journal.timestamp);
                journal = priorFilepath == null ? null : new JournalFile(priorFilepath);
            };

            Game.log($"searchJournalsDeep: count: {count}");
        }

        public void walkDeep(int index, bool searchUp, Func<IJournalEntry, bool> func, Func<JournalFile, bool>? finishWhen = null)
        {
            if (string.IsNullOrEmpty(this.cmdrName)) Debugger.Break();

            var count = 0;
            var journal = this;

            // search older journals
            while (journal != null)
            {
                ++count;
                // walk this journal
                var finished = journal.walk(index, searchUp, func);
                if (finished) break;

                if (finishWhen != null)
                {
                    finished = finishWhen(journal);
                    if (finished) break;
                }

                var priorFilepath = JournalFile.getCommanderJournalBefore(this.cmdrName, this.isOdyssey, journal.timestamp);
                journal = priorFilepath == null ? null : new JournalFile(priorFilepath);
            };

            Game.log($"walkDeep: count: {count}");
        }

        public bool walk(int index, bool searchUp, Func<IJournalEntry, bool> func)
        {
            int idx = index;
            if (idx == -1)
                idx = this.Count - 1;

            // the end is either the first or last element
            var endIdx = searchUp ? 0 : this.Count;

            while (idx != endIdx)
            {
                if (idx < 0 || idx >= this.Entries.Count) break;
                var finished = func(this.Entries[idx]);
                if (finished) return true;

                // increment index and go round again
                if (searchUp)
                    idx--;
                else
                    idx++;
            }

            return false;
        }

        public static string? getCommanderJournalBefore(string? cmdr, bool isOdyssey, DateTime timestamp)
        {
            var manyFiles = new DirectoryInfo(Game.settings.watchedJournalFolder)
                .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(_ => _.LastWriteTimeUtc);

            var journalFiles = manyFiles
                .Where(_ => _.LastWriteTime < timestamp)
                .Select(_ => _.FullName);

            if (journalFiles.Count() == 0) return null;

            if (string.IsNullOrWhiteSpace(cmdr))
            {
                // use the most recent journal file
                return journalFiles.First();
            }

            var CMDR = cmdr.ToUpper();

            var filename = journalFiles.FirstOrDefault((filepath) =>
            {
                using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line == null) break;

                        if (line.Contains("\"event\":\"Fileheader\"") && !line.ToUpperInvariant().Contains($"\"Odyssey\":{isOdyssey}".ToUpperInvariant()))
                        {
                            Game.log($"getCommanderJournalBefore: wrong isOdyssey: {isOdyssey} - skip: {filepath}");
                            return false;
                        }

                        if (line.Contains("\"event\":\"Commander\""))
                        {
                            // no need to process further lines
                            Game.log($"getCommanderJournalBefore: expected cmdr: {cmdr}, found '{line}' from: {filepath}");
                            return line.ToUpper().Contains($"\"NAME\":\"{CMDR}\"");
                        }
                    }

                    // it might be the right cmdr - we cannot tell yet as the game has not started
                    Game.log($"getCommanderJournalBefore: no cmdr found yet from: {filepath}");

                    // but confirm this is the current game log by failing to read it without sharing (proving the game has the file locked open)
                    try
                    {
                        File.ReadAllText(filepath);
                        // this file is NOT locked open - do not use it
                        return false;
                    }
                    catch (IOException)
                    {
                        // yup, it's locked open - we'll use this file until we know who the cmdr is
                        return true;
                    }
                }
            });

            // TODO: As we already loaded the journal into memory, it would be nice to use that rather than reload it again from JournalWatcher
            return filename;
        }

        /// <summary>
        /// Force append a shutdown entry - to be used if the game crashes
        /// </summary>
        public void fakeShutdown()
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("s");
                File.AppendAllText(this.filepath, @$"{{ ""timestamp"":""{timestamp}Z"", ""event"":""Shutdown"" }}");
            }
            catch (Exception ex)
            {
                Game.log($"Error in fakeShutdown: {ex.Message}");
            }
        }
    }
}
