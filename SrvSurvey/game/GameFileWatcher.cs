using Newtonsoft.Json;

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
        public static Q read<Q>(string filepath, bool force = false) where Q : IWatchedFile
        {
            if (!mapWatchers.ContainsKey(filepath))
                mapWatchers[filepath] = new JsonFileWatcher<Q>(filepath, false);

            var watcher = (JsonFileWatcher<Q>)mapWatchers[filepath];

            // force read the file
            if (force)
                watcher.readFile();

            return watcher.value;
        }

        class JsonFileWatcher<T> where T : IWatchedFile
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
                Game.log($"Initializing: {this.filename}");

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
                        this.watcher = new FileSystemWatcher(Game.settings.watchedJournalFolder, filename);
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
                });
            }

            public void readFile()
            {
                var obj = this.parseFile();
                Game.log($"Reading: {this.filename}");

                // only update our value if we got a new one
                if (obj != null)
                    this.value = obj;

                // Maybe?
                // Program.invalidateActivePlotters();
            }

            private T? parseFile()
            {
                if (!File.Exists(this.filepath))
                {
                    Game.log($"Check watched journal folder setting!\r\nCannot find file: {this.filepath}");
                    return default(T);
                }

                // read the file contents ...
                using (var sr = new StreamReader(new FileStream(this.filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var json = sr.ReadToEnd();
                    if (json == null || json == "") return default(T);

                    // ... and parse
                    var obj = JsonConvert.DeserializeObject<T>(json);
                    return obj;
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
                        this.watcher = null;
                    }
                }
            }
        }
    }

    interface IWatchedFile { }
    class WatchedFile : IWatchedFile { }

    class MarketFile : WatchedFile
    {
        public static string filepath { get => Path.Combine(Game.settings.watchedJournalFolder, "Market.json"); }

        public static MarketFile read(bool force)
        {
            return GameFileWatcher.read<MarketFile>(MarketFile.filepath, force);
        }

        public DateTimeOffset timestamp;
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
            public bool Rare;
        }
    }

    // not needed after all?
    class CargoFile2 : WatchedFile
    {
        public static string filepath { get => Path.Combine(Game.settings.watchedJournalFolder, "Cargo.json"); }

        public static CargoFile2 read(bool force)
        {
            return GameFileWatcher.read<CargoFile2>(CargoFile2.filepath, force);
        }

        public DateTimeOffset timestamp;
        public string Vessel;
        public int Count;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<InventoryItem> Inventory = new();


        private Dictionary<string, int> lastInventory = new();

        public Dictionary<string, int> getDiff()
        {
            var diffs = new Dictionary<string, int>();
            if (lastInventory.Count > 0)
            {
                foreach (var entry in this.Inventory)
                {
                    var delta = entry.Count - lastInventory.GetValueOrDefault(entry.Name);
                    if (delta != 0) diffs[entry.Name] = delta;
                }

                foreach (var entry in this.lastInventory)
                {
                    if (!this.Inventory.Any(_ => _.Name == entry.Key))
                        diffs[entry.Key] = -entry.Value;
                }

            }

            return diffs;
        }

        public int getCount(string commodity)
        {
            return this.Inventory.FirstOrDefault(i => i.Name == commodity)?.Count ?? 0;
        }
    }
}
