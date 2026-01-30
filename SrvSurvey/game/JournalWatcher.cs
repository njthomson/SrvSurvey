using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.plotters;

namespace SrvSurvey
{
    delegate void OnJournalEntry(JournalEntry entry, int index);
    delegate void OnRawJournalEntry(JObject entry, int index);

    class JournalWatcher : JournalFile, IDisposable
    {
        private FileSystemWatcher? watcher;
        private bool disposed = false;

        public event OnJournalEntry? onJournalEntry;
        public event OnRawJournalEntry? onRawJournalEntry;

        public JournalWatcher(string filepath) : base(filepath)
        {
            var folder = Path.GetDirectoryName(filepath)!;
            var filename = Path.GetFileName(filepath);
            this.watcher = new FileSystemWatcher(folder, filename);
            this.watcher.Changed += JournalWatcher_Changed;
            this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            this.watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            this.disposed = true;
        }

        public void poke()
        {
            // FileSystemWatcher sometimes gets stale and needs to be poked, forcing a flush of any pending file writes
            FileInfo fi = new FileInfo(this.filepath);
            using FileStream stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            stream.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.watcher != null)
                {
                    this.watcher.Changed -= JournalWatcher_Changed;
                    this.watcher.Dispose();
                    this.watcher = null;
                }
            }
        }

        private void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.disposed) return;
            Program.crashGuard(() =>
            {
                PlotPulse.resetPulse();
                this.readEntries();
            });
        }

        protected override JObject? readRaw()
        {
            var raw = base.readRaw();

            if (raw != null && this.onRawJournalEntry != null && !this.disposed)
            {
                Program.control!.Invoke((MethodInvoker)delegate
                {
                    if (raw != null && this.onRawJournalEntry != null && !this.disposed)
                    {
                        this.onRawJournalEntry(raw, this.Entries.Count - 1);
                    }
                });
            }

            return raw;
        }

        protected override JournalEntry? readEntry()
        {
            var entry = base.readEntry();

            if (entry != null && this.onJournalEntry != null && !this.disposed)
            {
                Program.control!.Invoke((MethodInvoker)delegate
                {
                    if (entry != null && this.onJournalEntry != null && !this.disposed)
                    {
                        this.onJournalEntry(entry, this.Entries.Count - 1);
                    }
                });
            }

            return entry;
        }
    }
}
