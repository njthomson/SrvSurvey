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

namespace SrvSurvey
{
    delegate void OnJournalEntry(JournalEntry entry, int index);

    class JournalWatcher
    {
        public List<JournalEntry> Entries { get; } = new List<JournalEntry>();
        public event OnJournalEntry onJournalEntry;

        private StreamReader reader;
        private FileSystemWatcher watcher;
        public readonly string filepath;
        public readonly DateTime timestamp;

        public JournalWatcher(string filepath, bool watch = true)
        {
            Game.log($"Reading journal: {Path.GetFileName(filepath)}");

            this.filepath = filepath;
            this.timestamp = File.GetLastWriteTime(filepath);

            this.reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            this.readEntries();

            if (watch)
            {
                var folder = Path.GetDirectoryName(filepath);
                var filename = Path.GetFileName(filepath);
                this.watcher = new FileSystemWatcher(folder, filename);
                this.watcher.Changed += JournalWatcher_Changed;
                this.watcher.NotifyFilter = NotifyFilters.LastWrite;
                this.watcher.EnableRaisingEvents = true;
            }
        }

        private void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //Game.log($"!?-->");

            PlotPulse.LastChanged = DateTime.Now;
            this.readEntries();
        }

        public int Count { get => this.Entries.Count; }

        public void readEntries()
        {
            while (!this.reader.EndOfStream)
            {
                this.readEntry();
            }
        }

        public void readEntry()
        {
            // read next entry, add to list or skip if it's blank
            var entry = this.parseNextEntry();
            if (entry == null) return;

            this.Entries.Add(entry);

            if (this.onJournalEntry != null)
            {
                Program.control.Invoke((MethodInvoker)delegate
                {
                    this.onJournalEntry(entry, this.Entries.Count - 1);
                });
            }
        }

        private JournalEntry parseNextEntry()
        {
            var json = reader.ReadLine();
            JToken entry = JsonConvert.DeserializeObject<JToken>(json);

            switch (entry["event"].ToString())
            {
                case nameof(Fileheader): return entry.ToObject<Fileheader>();
                case nameof(Commander): return entry.ToObject<Commander>();
                case nameof(Location): return entry.ToObject<Location>();
                case nameof(ApproachSettlement): return entry.ToObject<ApproachSettlement>();
                case nameof(Touchdown): return entry.ToObject<Touchdown>();
                case nameof(Liftoff): return entry.ToObject<Liftoff>();
                case nameof(LaunchSRV): return entry.ToObject<LaunchSRV>();
                case nameof(DockSRV): return entry.ToObject<DockSRV>();
                case nameof(SendText): return entry.ToObject<SendText>();
                case nameof(CodexEntry): return entry.ToObject<CodexEntry>();
                case nameof(SupercruiseExit): return entry.ToObject<SupercruiseExit>();
                case nameof(SupercruiseEntry): return entry.ToObject<SupercruiseEntry>();
                case nameof(Music): return entry.ToObject<Music>();
                case nameof(StartJump): return entry.ToObject<StartJump>();
                case nameof(FSDJump): return entry.ToObject<FSDJump>();
                case nameof(Shutdown): return entry.ToObject<Shutdown>();
                case nameof(ScanOrganic): return entry.ToObject<ScanOrganic>();
                case nameof(SAASignalsFound): return entry.ToObject<SAASignalsFound>();
                case nameof(ApproachBody): return entry.ToObject<ApproachBody>();
                case nameof(LeaveBody): return entry.ToObject<LeaveBody>();
                case nameof(Scan): return entry.ToObject<Scan>();
                case nameof(Embark): return entry.ToObject<Embark>();
                case nameof(Disembark): return entry.ToObject<Disembark>();

                default:
                    // ignore anything else
                    return null;
            }
        }

        public T FindEntryByType<T>(int index, bool searchUp) where T : JournalEntry
        {
            if (index == -1) index = this.Entries.Count - 1;

            int n = index;
            while (n >= 0 && n < this.Entries.Count)
            {
                if (this.Entries[n] is T)
                {
                    return this.Entries[n] as T;
                }
                n += searchUp ? -1 : +1;
            }

            return null;
        }

        public int GetSettlementStartIndexBefore(int fromIndex)
        {
            // search back from TouchDown
            for (int n = fromIndex; n >= 0; n--)
            {
                if (this.Entries[n].@event == nameof(SupercruiseExit))
                {
                    return n;
                }
                var sendText = this.Entries[n] as SendText;
                if (sendText != null && sendText.Message == "start survey")
                {
                    return n;
                }
            }

            // if nothing found, search forwards until next Liftoff
            for (int n = fromIndex; n < this.Entries.Count; n++)
            {
                if (this.Entries[n].@event == nameof(Liftoff))
                {
                    return -1;
                }
                var sendText = this.Entries[n] as SendText;
                if (sendText != null && sendText.Message == "start survey")
                {
                    return n;
                }
            }

            return -1;

        }
    }


}
