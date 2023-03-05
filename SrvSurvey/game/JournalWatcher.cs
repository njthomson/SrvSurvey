using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Windows.Forms;
using SrvSurvey.game;
using System.Reflection;

namespace SrvSurvey
{
    delegate void OnJournalEntry(JournalEntry entry, int index);

    class JournalFile
    {
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

        public List<JournalEntry> Entries { get; } = new List<JournalEntry>();

        protected StreamReader reader;
        public readonly string filepath;
        public readonly DateTime timestamp;
        public readonly string CommanderName;

        public JournalFile(string filepath)
        {
            Game.log($"Reading journal: {Path.GetFileName(filepath)}");

            this.filepath = filepath;
            this.timestamp = File.GetLastWriteTime(filepath);

            this.reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            this.readEntries();

            var entry = this.FindEntryByType<Commander>(0, false);
            this.CommanderName = entry?.Name;
        }

        public int Count { get => this.Entries.Count; }

        public void readEntries()
        {
            while (!this.reader.EndOfStream)
            {
                this.readEntry();
            }
        }

        protected virtual JournalEntry readEntry()
        {
            // read next entry, add to list or skip if it's blank
            var entry = this.parseNextEntry();

            if (entry != null)
                this.Entries.Add(entry);

            return entry;
        }

        private JournalEntry parseNextEntry()
        {
            var json = reader.ReadLine();
            JToken entry = JsonConvert.DeserializeObject<JToken>(json);

            var eventName = entry["event"].Value<string>();
            if (typeMap.ContainsKey(eventName))
            {
                return entry.ToObject(typeMap[eventName]) as JournalEntry;
            }

            // ignore anything else
            return null;
        }

        public T FindEntryByType<T>(int index, bool searchUp) where T : JournalEntry
        {
            if (index == -1) index = this.Entries.Count - 1;

            int n = index;
            while (n >= 0 && n < this.Entries.Count)
            {
                if (this.Entries[n].GetType() == typeof(T))
                {
                    return this.Entries[n] as T;
                }
                n += searchUp ? -1 : +1;
            }

            return null;
        }

        public bool search<T>(Func<T, bool> func) where T : JournalEntry
        {
            // see if we can find recent BioScans
            int idx = 0;
            do
            {
                var entry = this.FindEntryByType<T>(idx, false);
                if (entry == null)
                {
                    // no more entries in this file
                    break;
                }

                // do something with the entry, exit if finished
                var finished = func(entry);
                if (finished) return finished;

                // otherwise keep going
                idx = this.Entries.IndexOf(entry) + 1;
            } while (idx < this.Count);

            // if we run out of entries, we don't know if we're necessarily finished
            return false;
        }

        public void searchDeep<T>(Func<T, bool> func, Func<JournalFile, bool> finishWhen = null) where T : JournalEntry
        {
            var count = 0;
            var journals = this;

            // search older journals
            while (journals != null)
            {
                ++count;
                // search journals
                var finished = journals.search(func);
                if (finished) break;

                if (finishWhen != null)
                {
                    finished = finishWhen(journals);
                    if (finished) break;
                }

                var priorFilepath = JournalFile.getCommanderJournalBefore(this.CommanderName, journals.timestamp);
                journals = priorFilepath == null ? null : new JournalFile(priorFilepath);
            };

            Game.log($"searchJournalsDeep: count: {count}");
        }

        public static string getCommanderJournalBefore(string cmdr, DateTime timestamp)
        {
            var manyFiles = new DirectoryInfo(SrvSurvey.journalFolder)
                .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(_ => _.LastWriteTimeUtc);

            var journalFiles = manyFiles
                .Where(_ => _.LastWriteTime < timestamp)
                .Select(_ => _.FullName);

            if (journalFiles.Count() == 0) return null;

            if (cmdr == null)
            {
                // use the most recent journal file
                return journalFiles.First();
            }

            var CMDR = cmdr.ToUpper();

            var filename = journalFiles.FirstOrDefault((filepath) =>
            {
                // TODO: Use some streaming reader to save reading the whole file up front?
                using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // TODO: allow for non-Odyssey
                        if (line.Contains("\"event\":\"Fileheader\"") && line.Contains("\"Odyssey\":false"))
                            return false;

                        if (line.Contains("\"event\":\"Commander\""))
                            // no need to process further lines
                            return line.ToUpper().Contains($"\"NAME\":\"{CMDR}\"");
                    }
                    return false;
                }
            });

            // TODO: As we already loaded the journal into memory, it would be nice to use that rather than reload it again from JournalWatcher
            return filename;
        }
    }

    class JournalWatcher : JournalFile
    {
        private FileSystemWatcher watcher;

        public event OnJournalEntry onJournalEntry;

        public JournalWatcher(string filepath) : base(filepath)
        {
            var folder = Path.GetDirectoryName(filepath);
            var filename = Path.GetFileName(filepath);
            this.watcher = new FileSystemWatcher(folder, filename);
            this.watcher.Changed += JournalWatcher_Changed;
            this.watcher.NotifyFilter = NotifyFilters.LastWrite;
            this.watcher.EnableRaisingEvents = true;
        }

        private void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            PlotPulse.LastChanged = DateTime.Now;
            this.readEntries();
        }

        protected override JournalEntry readEntry()
        {
            var entry = base.readEntry();

            if (entry != null && this.onJournalEntry != null)
            {
                Program.control.Invoke((MethodInvoker)delegate
                {
                    this.onJournalEntry(entry, this.Entries.Count - 1);
                });
            }

            return entry;
        }
    }
}
