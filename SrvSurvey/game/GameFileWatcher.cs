using Newtonsoft.Json;
using SrvSurvey.plotters;
using System.Diagnostics;

namespace SrvSurvey.game
{
    class GameFileWatcher
    {
        private static Dictionary<string, object> mapWatchers = new();

        /// <summary>
        /// Returns the last read/parsed content of the given file, watching for future changes
        /// </summary>
        public static Q watch<Q>(string filepath) where Q : IWatchedFile
        {
            if (!mapWatchers.ContainsKey(filepath))
                mapWatchers[filepath] = new JsonFileWatcher<Q>(filepath, true);

            var watcher = (JsonFileWatcher<Q>)mapWatchers[filepath];

            // start watching if not already
            if (!watcher.watching) watcher.watching = true;
            return watcher.value;
        }

        /// <summary>
        /// Returns the content of the given file, re-reading if forced
        /// </summary>
        public static Q read<Q>(string filepath, bool force = false, DateTimeOffset? timestamp = null) where Q : IWatchedFile
        {
            if (!mapWatchers.ContainsKey(filepath))
                mapWatchers[filepath] = new JsonFileWatcher<Q>(filepath, false);

            var watcher = (JsonFileWatcher<Q>)mapWatchers[filepath];

            // force read the file
            if (force)
                watcher.readFile(timestamp);

            return watcher.value;
        }

        class JsonFileWatcher<T> : IDisposable where T : IWatchedFile
        {
            private readonly string filename;
            private readonly string filepath;
            private FileSystemWatcher? watcher;
            private bool _watching;
            public T value;

            public JsonFileWatcher(string filepath, bool watch)
            {
                this.filename = Path.GetFileName(filepath);
                this.filepath = filepath;

                // read the file or initialize with a default constructed instance if not found
                this.value = this.parseFile() ?? Activator.CreateInstance<T>();
                Game.log($"JsonFileWatcher: initializing: {this.filename}");

                // initialize things AFTER we've performed the first read
                this.value.preRead();

                this.watching = watch;
            }

            public bool watching
            {
                get => this._watching;
                set
                {
                    if (value && !this._watching)
                    {
                        // start watching
                        Game.log($"JsonFileWatcher: watching: {filepath}");
                        var folder = Path.GetDirectoryName(filepath)!;
                        this.watcher = new FileSystemWatcher(folder, filename);
                        this.watcher.NotifyFilter = NotifyFilters.LastWrite;
                        this.watcher.Changed += this.fileWatcher_Changed;
                        this.watcher.EnableRaisingEvents = true;
                    }
                    else if (!value && this._watching)
                    {
                        // stop watching
                        if (this.watcher != null)
                        {
                            this.watcher.Changed -= fileWatcher_Changed;
                            this.watcher = null;
                        }
                    }

                    this._watching = value;
                }
            }

            private void fileWatcher_Changed(object _sender, FileSystemEventArgs _e)
            {
                Program.crashGuard(() =>
                {
                    this.readFile();
                    PlotBase2.renderAll(Game.activeGame, true);
                });
            }

            public void readFile(DateTimeOffset? timestamp = null)
            {
                // File I/O stays outside any cargo lock
                var obj = this.parseFile();
                Game.log($"**** Reading: {this.filename}");

                // ignore parsed data if we are given a timestamp but it does not match
                if (timestamp != null && obj?.timestamp != timestamp)
                    return;

                // only update our value if we got a new one
                if (obj != null)
                {
                    // For cargo, hold SyncRoot across snapshot + replace so lastInventory and
                    // the live instance cannot race with CargoTransfer / journal mutations.
                    // Never call Game.log while SyncRoot is held (UI Invoke deadlock risk).
                    if (typeof(T) == typeof(CargoFile2))
                    {
                        string? preReadLog;
                        lock (CargoFile2.SyncRoot)
                        {
                            preReadLog = ((CargoFile2)(object)this.value).preReadUnlocked();
                            this.value = obj;
                        }
                        if (preReadLog != null)
                            Game.log(preReadLog);
                    }
                    else
                    {
                        this.value.preRead();
                        this.value = obj;
                    }
                }
            }

            private T? parseFile()
            {
                if (!File.Exists(this.filepath))
                {
                    Game.log($"Check watched journal folder setting!\r\nCannot find file: {this.filepath}");
                    return default(T);
                }

                try
                {
                    // read the file contents ...
                    using var sr = Data.openSharedStreamReader(this.filepath);
                    var json = sr.ReadToEnd();
                    if (json == null || json == "") return default(T);

                    // ... and parse
                    var obj = JsonConvert.DeserializeObject<T>(json);
                    return obj;
                }
                catch (Exception ex)
                {
                    Game.log($"Error reading/parsing file: {this.filepath}\r\n{ex}");
                    return default(T);
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
                    if (this.watcher != null)
                    {
                        this.watcher.Changed -= fileWatcher_Changed;
                        this.watcher.Dispose();
                        this.watcher = null;
                    }
                }
            }
        }
    }

    interface IWatchedFile
    {
        public DateTimeOffset timestamp { get; }
        public string @event { get; }

        public void preRead();
    }

    class WatchedFile : IWatchedFile
    {
        public DateTimeOffset timestamp { get; set; }
        public string @event { get; set; }

        public virtual void preRead() { /* no-op */ }
    }

    class MarketFile : WatchedFile
    {
        public static string filepath { get => Path.Combine(Game.settings.watchedJournalFolder, "Market.json"); }

        public static MarketFile read(bool force)
        {
            return GameFileWatcher.read<MarketFile>(MarketFile.filepath, force);
        }

        public long MarketId;
        public string StationName;
        public string StationType;
        public string CarrierDockingAccess;
        public string StarSystem;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Item> Items = new();

        public class Item
        {
            public long id;
            public string Name;
            public string Name_Localised;
            public string Category;
            public string Category_Localised;
            public int BuyPrice;
            public int SellPrice;
            public int MeanPrice;
            public int StockBracket;
            public int DemandBracket;
            public int Stock;
            public int Demand;
            public bool Producer;
            public bool Consumer;
            public bool Rare;
        }
    }

    /// <summary>
    /// In-memory cargo state backed by Cargo.json.
    /// Inventory / lastInventory updates are serialized with <see cref="SyncRoot"/> so squadron-FC
    /// diff tracking cannot race with CargoTransfer mutations or FileSystemWatcher reloads.
    /// </summary>
    class CargoFile2 : WatchedFile
    {
        public static string filepath { get => Path.Combine(Game.settings.watchedJournalFolder, "Cargo.json"); }

        /// <summary>Shared lock for all Inventory / lastInventory readers and writers.</summary>
        public static object SyncRoot { get; } = new object();

        public static CargoFile2 read(bool force, DateTimeOffset? timestamp = null)
        {
            return GameFileWatcher.read<CargoFile2>(CargoFile2.filepath, force, timestamp);
        }

        public string Vessel;
        public int Count;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<InventoryItem> Inventory = new();

        private static Dictionary<string, int> lastInventory = new();

        /// <summary>
        /// When true, <see cref="preRead"/> will not overwrite <see cref="lastInventory"/>.
        /// Set by <see cref="captureBeforeSnapshot"/> so CargoTransfer can freeze the true before-state
        /// before it mutates live inventory for the UI; cleared by <see cref="getDiff"/>.
        /// </summary>
        private static bool preserveLastInventory;

        /// <summary>True when a transfer path has frozen lastInventory until getDiff consumes it.</summary>
        public static bool HasPreservedSnapshot
        {
            get { lock (SyncRoot) return preserveLastInventory; }
        }

        /// <summary>
        /// Capture the current inventory as the "before" snapshot for a later getDiff, and hold it
        /// across subsequent preRead/file reloads. Call this under or outside the lock before any
        /// mutation that would otherwise corrupt the before-state (e.g. CargoTransfer on a squadron FC).
        /// </summary>
        public void captureBeforeSnapshot()
        {
            string logMsg;
            lock (SyncRoot)
            {
                copyInventoryToLastUnlocked();
                preserveLastInventory = true;
                // Build log text under the lock; emit after release (Game.log may Invoke UI).
                logMsg = lastInventory.formatWithHeader($"**** captureBeforeSnapshot: (lastInventory.Count: {lastInventory.Count})", "\r\n\t");
            }
            Game.log(logMsg);
        }

        /// <summary>
        /// Drop a held before-snapshot without computing a diff (e.g. when skipNextCargoEvent
        /// intentionally suppresses squadron supplyFC processing).
        /// </summary>
        public void clearPreservedSnapshot()
        {
            var dropped = false;
            lock (SyncRoot)
            {
                if (preserveLastInventory)
                {
                    preserveLastInventory = false;
                    dropped = true;
                }
            }
            if (dropped)
                Game.log("**** clearPreservedSnapshot: dropped held lastInventory");
        }

        public override void preRead()
        {
            // Log only after SyncRoot is released — Game.log can block on the UI thread.
            var logMsg = preReadWithLock();
            if (logMsg != null)
                Game.log(logMsg);
        }

        /// <summary>
        /// Snapshot-before-replace work for callers that already hold <see cref="SyncRoot"/>.
        /// Returns a log line to emit after the lock is released (never log while locked).
        /// </summary>
        internal string? preReadUnlocked()
        {
            if (preserveLastInventory)
                return $"**** preRead: preserving lastInventory ({lastInventory.Count} entries) for pending squadron/diff snapshot";

            // clone current inventory before the watcher replaces this instance
            copyInventoryToLastUnlocked();
            return lastInventory.formatWithHeader($"**** preRead: (lastInventory.Count: {lastInventory.Count})", "\r\n\t");
        }

        private string? preReadWithLock()
        {
            lock (SyncRoot)
                return preReadUnlocked();
        }

        /// <summary>
        /// Ship cargo delta: current Inventory − lastInventory (non-zero entries only).
        /// Clears any preserved snapshot. Call while the new inventory is already applied;
        /// release SyncRoot before network I/O or logging.
        /// </summary>
        public Dictionary<string, int> getDiff()
        {
            Dictionary<string, int> diffs;
            int beforeCount;
            int afterCount;
            Dictionary<string, int>? beforeDump = null;
            Dictionary<string, int>? afterDump = null;

            lock (SyncRoot)
            {
                diffs = new Dictionary<string, int>();
                var inventory = this.Inventory ?? new List<InventoryItem>();

                foreach (var entry in inventory)
                {
                    var delta = entry.Count - lastInventory.GetValueOrDefault(entry.Name);
                    if (delta != 0) diffs[entry.Name] = delta;
                }

                foreach (var entry in lastInventory)
                    if (!inventory.Any(_ => _.Name == entry.Key))
                        diffs[entry.Key] = -entry.Value;

                beforeCount = lastInventory.Count;
                afterCount = inventory.Count;

                // Full dumps only while debugging — keeps the lock short and avoids heavy logs in normal play.
                if (Debugger.IsAttached)
                {
                    beforeDump = new Dictionary<string, int>(lastInventory);
                    afterDump = inventory.ToDictionary(i => i.Name, i => i.Count);
                }

                preserveLastInventory = false;
            }

            // Logging is outside the lock: Game.log can synchronously Invoke the UI thread.
            if (beforeDump != null && afterDump != null)
            {
                Game.log(beforeDump.formatWithHeader($"**** getDiff before (lastInventory.Count: {beforeCount})", "\r\n\t"));
                Game.log(afterDump.formatWithHeader($"**** getDiff after (Inventory.Count: {afterCount})", "\r\n\t"));
            }
            else
            {
                Game.log($"**** getDiff summary: beforeEntries={beforeCount}, afterEntries={afterCount}, diffEntries={diffs.Count}");
            }
            Game.log(diffs.formatWithHeader("**** getDiff:", "\r\n\t"));

            return diffs;
        }

        /// <summary>Apply a journal Cargo event's inventory fields under the shared lock.</summary>
        public void applyJournalInventory(DateTimeOffset timestamp, string vessel, int count, List<InventoryItem>? inventory)
        {
            lock (SyncRoot)
            {
                this.timestamp = timestamp;
                this.Vessel = vessel;
                this.Count = count;
                this.Inventory = inventory ?? new List<InventoryItem>();
            }
        }

        public int getCount(string commodity)
        {
            lock (SyncRoot)
            {
                return this.Inventory.FirstOrDefault(i => i.Name == commodity)?.Count ?? 0;
            }
        }

        private void copyInventoryToLastUnlocked()
        {
            lastInventory.Clear();
            if (this.Inventory != null)
                foreach (var entry in this.Inventory)
                    lastInventory[entry.Name] = entry.Count;
        }
    }

    internal class SystemNickNames : WatchedFile
    {
        /* A sample .json file
{
  "timestamp": "2026-01-01T00:00:00Z",
  "event": "SystemNickNames",
  "map": {
    "Sol": "Birthplace of Humanity",
    "abc": "xyz"
  }
}
        */

        private static readonly string filepath = Path.Combine(Program.dataFolder, "system-nick-names.json");

        /// <summary> Returns a nick-name for a given system or the name if no match is found </summary>
        public static string get(string? systemName)
        {
            if (systemName == null) return null!;

            if (!Game.settings.useSystemNickNames)
                return systemName;

            var match = instance.map.GetValueOrDefault(systemName, StringComparison.OrdinalIgnoreCase)
                ?? ravenMap.GetValueOrDefault(systemName, StringComparison.OrdinalIgnoreCase);

            return match ?? systemName;
        }

        private static SystemNickNames instance => GameFileWatcher.watch<SystemNickNames>(SystemNickNames.filepath);

        /// <summary> A map of real system names to alternate nick-names </summary>
        public Dictionary<string, string> map = [];
        public static Dictionary<string, string> ravenMap = [];
    }
}
