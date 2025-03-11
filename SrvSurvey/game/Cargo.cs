using Newtonsoft.Json;
using SrvSurvey.game;

namespace SrvSurvey
{
    class CargoFile : Cargo
    {

        public static readonly string Filename = "Cargo.json";
        public static string Filepath { get => Path.Combine(Game.settings.watchedJournalFolder, CargoFile.Filename); }

        #region properties from file

        // inherited from Cargo

        #endregion

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

        [JsonIgnore]
        public Dictionary<string, int> lastInventory = new();

        #region deserializing + file watching

        private FileSystemWatcher? fileWatcher;

        private CargoFile() { }

        public static CargoFile load(bool watch)
        {
            // if it doesn't exist yet ... create a stub empty file
            if (!File.Exists(CargoFile.Filepath))
            {
                try
                {
                    Game.log($"Creating empty CargoFile.json in: {CargoFile.Filepath}");
                    var isoNow = DateTimeOffset.UtcNow.ToString("s");
                    var json = "{ \"timestamp\":\"" + isoNow + "Z\", \"event\":\"Cargo\", \"Vessel\":\"Ship\", \"Count\":0, \"Inventory\":[] }";
                    File.WriteAllText(CargoFile.Filepath, json);
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to create stub Cargo.json: {ex}");
                }
            }

            var cargo = new CargoFile();
            cargo.parseFile();

            if (watch)
            {
                // start watching the status file
                cargo.fileWatcher = new FileSystemWatcher(Game.settings.watchedJournalFolder, CargoFile.Filename);
                cargo.fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                cargo.fileWatcher.Changed += cargo.fileWatcher_Changed;
                cargo.fileWatcher.EnableRaisingEvents = true;
            }

            return cargo;
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Program.crashGuard(() =>
            {
                this.parseFile();
            });
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
                if (this.fileWatcher != null)
                {
                    this.fileWatcher.Changed -= fileWatcher_Changed;
                    this.fileWatcher = null;
                }
            }
        }

        private void parseFile()
        {
            if (!File.Exists(Status.Filepath))
            {
                Game.log($"Check watched journal folder setting!\r\nCannot find file: {CargoFile.Filepath}");
                return;
            }

            // clone prior inventory
            lastInventory.Clear();
            if (this.Inventory != null)
                foreach (var entry in this.Inventory)
                    lastInventory[entry.Name] = entry.Count;

            // read the file contents ...
            using (var sr = new StreamReader(new FileStream(CargoFile.Filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var json = sr.ReadToEnd();
                if (json == null || json == "") return;

                // ... parse into tmp object ...
                var obj = JsonConvert.DeserializeObject<Cargo>(json);

                // ... assign all property values from tmp object 
                var allProps = typeof(Cargo).GetProperties(Program.InstanceProps);
                foreach (var prop in allProps)
                {
                    if (prop.CanWrite)
                    {
                        prop.SetValue(this, prop.GetValue(obj));
                    }
                }
            }

            Game.log($"CargoFile updated: {this.Count} items in: {this.Vessel}\r\n  " + string.Join("\r\n  ", this.Inventory));
            Program.invalidateActivePlotters();
        }

        #endregion

    }
}

