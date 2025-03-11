using Newtonsoft.Json;

namespace SrvSurvey.game
{
    public class GameFileWatcher
    {
        private static Dictionary<string, object> mapWatchers = new();

        /// <summary>
        /// Returns the last read/parsed content of the given file
        /// </summary>
        public static Q read<Q>(string filepath) where Q : IWatchedFile
        {
            if (!mapWatchers.ContainsKey(filepath))
                mapWatchers[filepath] = new JsonFileWatcher<Q>(filepath);

            var watcher = (JsonFileWatcher<Q>)mapWatchers[filepath];
            return watcher.value;
        }


        class JsonFileWatcher<T> where T : IWatchedFile
        {
            private string filename;
            private string filepath;
            private FileSystemWatcher? watcher;
            public T value;

            public JsonFileWatcher(string filepath)
            {
                this.filename = Path.GetFileName(filepath);
                this.filepath = filepath;

                // initialize with a default constructed instance

                this.value = this.readFile() ?? Activator.CreateInstance<T>();

                // start watching the status file
                this.watcher = new FileSystemWatcher(Game.settings.watchedJournalFolder, filename);
                this.watcher.NotifyFilter = NotifyFilters.LastWrite;
                this.watcher.Changed += this.fileWatcher_Changed;
                this.watcher.EnableRaisingEvents = true;
            }

            private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
            {
                Program.crashGuard(() =>
                {
                    var obj = this.readFile();

                    // only update our value if we got a new one
                    if (obj != null)
                        this.value = obj;

                    // Maybe?
                    // Program.invalidateActivePlotters();
                });
            }

            private T? readFile()
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
                    Game.log($"{filename} updated");
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

    public interface IWatchedFile { }
    public class WatchedFile : IWatchedFile { }

    public class MarketFile : WatchedFile
    {
        public static string filepath { get => Path.Combine(Game.settings.watchedJournalFolder, "Market.json"); }

        public long MarketId;
        public string StationName;
        public string StationType;
        public string CarrierDockingAccess;
        public string StarSystem;
        public List<Item> Items;

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
}
