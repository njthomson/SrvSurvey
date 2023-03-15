using SrvSurvey.game;

namespace SrvSurvey
{
    delegate void OnJournalEntry(JournalEntry entry, int index);

    class JournalWatcher : JournalFile, IDisposable
    {
        private FileSystemWatcher watcher;
        private bool disposed = false;

        public event OnJournalEntry onJournalEntry;

        public JournalWatcher(string filepath) : base(filepath)
        {
            var folder = Path.GetDirectoryName(filepath);
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.watcher.Changed -= JournalWatcher_Changed;
                this.watcher = null;
            }
        }

        private void JournalWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.disposed) return;

            PlotPulse.LastChanged = DateTime.Now;
            this.readEntries();
        }

        protected override JournalEntry readEntry()
        {
            var entry = base.readEntry();

            if (entry != null && this.onJournalEntry != null && !this.disposed)
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
