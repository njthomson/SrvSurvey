using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey
{
    class NavRouteFile
    {

        public static readonly string Filename = "NavRoute.json";
        public static string Filepath { get => Path.Combine(Game.settings.watchedJournalFolder, NavRouteFile.Filename); }

        #region properties from file

        public DateTime timestamp { get; set; }
        public string @event { get; set; }
        public List<RouteEntry> Route { get; set; }

        #endregion

        #region deserializing + file watching

        private FileSystemWatcher? fileWatcher;

        private NavRouteFile()
        {
        }

        public static NavRouteFile load(bool watch)
        {
            // if it doesn't exist yet ... create a stub empty file
            if (!File.Exists(NavRouteFile.Filepath))
            {
                try
                {
                    Game.log($"Creating empty NavRoute.json in: {NavRouteFile.Filepath}");
                    var isoNow = DateTimeOffset.UtcNow.ToString("s");
                    var json = "{ \"timestamp\":\"" + isoNow + "Z\", \"event\":\"NavRouteClear\", \"Route\":[] }";
                    File.WriteAllText(NavRouteFile.Filepath, json);
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to create stub NavRoute.json: {ex}");
                }
            }

            var route = new NavRouteFile();
            route.parseFile();

            if (watch)
            {
                // start watching the status file
                route.fileWatcher = new FileSystemWatcher(Game.settings.watchedJournalFolder, NavRouteFile.Filename);
                route.fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                route.fileWatcher.Changed += route.FileWatcher_Changed;
                route.fileWatcher.EnableRaisingEvents = true;
            }

            return route;
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.parseFile();
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

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            PlotPulse.LastChanged = DateTime.Now;
            this.parseFile();
        }

        private void parseFile()
        {
            if (!File.Exists(Status.Filepath))
            {
                Game.log($"Check watched journal folder setting!\r\nCannot find file: {NavRouteFile.Filepath}");
                return;
            }

            // read the file contents ...
            using (var sr = new StreamReader(new FileStream(NavRouteFile.Filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var json = sr.ReadToEnd();
                if (json == null || json == "") return;

                // ... parse into tmp object ...
                var obj = JsonConvert.DeserializeObject<NavRouteFile>(json);

                // ... assign all property values from tmp object 
                var allProps = typeof(NavRouteFile).GetProperties(Program.InstanceProps);
                foreach (var prop in allProps)
                {
                    if (prop.CanWrite)
                    {
                        prop.SetValue(this, prop.GetValue(obj));
                    }
                }
            }
        }

        #endregion

    }

    internal class RouteEntry
    {
        // { "StarSystem":"Wregoe BU-Y b2-0", "SystemAddress":685451322393, "StarPos":[1077.37500,400.56250,-993.37500], "StarClass":"M" }, 
        public string StarSystem { get; set; }
        public double SystemAddress { get; set; }
        public double[] StarPos { get; set; }
        public string StarClass { get; set; }
    }

}

