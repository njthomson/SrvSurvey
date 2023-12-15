using SrvSurvey.game;
using System.Reflection;

namespace SrvSurvey
{
    static class Program
    {
        public static Control control { get; private set; }
        public static string dataFolder = Path.GetFullPath(Path.Combine(Application.UserAppDataPath, "..", "2.0.0.0"));
        private static string dataRootFolder = Path.GetFullPath(Path.Combine(dataFolder, ".."));

        public static readonly BindingFlags InstanceProps =
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetProperty |
            System.Reflection.BindingFlags.SetProperty;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(true);
            Application.ThreadException += Application_ThreadException;

            // create some control for invoking back onto the UI thread
            Program.control = new Control();
            Program.control.CreateControl();

            Application.Run(new Main());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            game.Game.log("Exception: " + e.Exception.Message);
            game.Game.log("Inner exception: " + e.Exception.InnerException?.Message);
            game.Game.log("Stack: " + e.Exception.StackTrace);

            FormErrorSubmit.Show(e.Exception);
        }

        #region plotter tracking

        private static Dictionary<string, Form> activePlotters = new Dictionary<string, Form>();

        public static T showPlotter<T>() where T : Form
        {
            var formType = typeof(T);

            // only create if missing
            T form;
            if (!activePlotters.ContainsKey(formType.Name))
            {
                Game.log($"Program.Creating plotter: {formType.Name}");

                // Get the public instance constructor that takes zero parameters
                ConstructorInfo ctor = formType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    CallingConventions.HasThis, new Type[0], null)!;

                // create and force form.Name to match class name
                form = (T)ctor.Invoke(null);
                form.Name = formType.Name;

                // add to list, then show
                activePlotters.Add(formType.Name, form);
            }
            else
            {
                form = (T)activePlotters[formType.Name];
            }

            // don't try showing a disposed form
            if (form.IsDisposed) return null!;

            // show form if not visible
            if (!form.Visible)
            {
                Game.log($"Program.Showing plotter: {formType.Name}");
                form.Show();
            }

            return form;
        }

        public static void closePlotter<T>() where T : Form
        {
            closePlotter(typeof(T).Name);
        }

        private static void closePlotter(string name)
        {
            var isActive = Program.activePlotters.ContainsKey(name);

            if (isActive)
            {
                Game.log($"Program.Closing plotter: {name}");

                var plotter = Program.activePlotters[name];
                Program.activePlotters.Remove(plotter.Name);
                plotter.Close();
            }
        }

        /// <summary>
        /// Returns True if the plotter type is active
        /// </summary>
        public static bool isPlotter<T>() where T : Form
        {
            return activePlotters.ContainsKey(typeof(T).Name);
        }

        /// <summary>
        /// Returns True if the plotter type is active
        /// </summary>
        public static T? getPlotter<T>() where T : Form
        {
            return activePlotters.ContainsKey(typeof(T).Name) ? activePlotters[typeof(T).Name] as T : null;
        }

        public static void closeAllPlotters()
        {
            Game.log($"Program.CloseAllPlotters");

            var names = Program.activePlotters.Keys.ToArray();
            foreach (string name in names)
                Program.closePlotter(name);
        }

        public static void repositionPlotters(Rectangle rect)
        {
            Game.log($"Program.repositionPlotters: {activePlotters.Count}, rect{rect}");

            foreach (PlotterForm form in activePlotters.Values)
                form.reposition(rect);
        }

        public static void hideActivePlotters()
        {
            Game.log($"Program.hideActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Opacity = 0;
        }

        public static void showActivePlotters()
        {
            Game.log($"Program.showActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Opacity = Game.settings.Opacity;
        }

        public static void invalidateActivePlotters()
        {
            Game.log($"Program.hideActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Invalidate();
        }

        #endregion

        #region migrate data files

        public static bool isMigrationValid()
        {
            // do we have any settings files in any folder already?
            var migrationValid = Directory.EnumerateFiles(dataRootFolder, "settings.json", SearchOption.AllDirectories).Any();
            return migrationValid;
        }

        public static void migrateToNewDataFolder()
        {
            Game.log($"migrateToNewDataFolder: old data into common folder: {Program.dataFolder} ...");
            Directory.CreateDirectory(Program.dataFolder);

            var rootFolder = new DirectoryInfo(Path.GetFullPath(Path.Combine(Program.dataFolder, "..")));
            var oldFolders = rootFolder.EnumerateDirectories()
                .Where(_ => !_.Name.EndsWith("2.0.0.0"))
                // order by oldest first
                .OrderBy(_ => File.GetLastWriteTime(Path.Combine(_.FullName, "settings.json")))
                .ToList();
            Game.log($"Migrating old folders:\r\n  " + string.Join("\r\n  ", oldFolders));

            // move core files from the most recent folder only
            moveCoreFiles(oldFolders.Last().FullName);

            oldFolders.ForEach(_ => mergeScannedBioEntryIds(_.FullName));
            oldFolders.ForEach(_ => mergeChildFiles(_.FullName));

            Game.log($"migrateToNewDataFolder: old data into common folder - complete");
        }

        private static void moveCoreFiles(string oldFolder)
        {
            Game.log($"> moveCoreFiles: {oldFolder}");
            var filenames = new List<string>();

            var settingsFilepath = Path.Combine(oldFolder, "settings.json");
            if (File.Exists(settingsFilepath)) filenames.Add(settingsFilepath);

            filenames.AddRange(Directory.EnumerateFiles(oldFolder, "*-legacy.json"));
            filenames.AddRange(Directory.EnumerateFiles(oldFolder, "*-live.json"));

            Game.log(string.Join("\r\n", filenames));

            filenames.ForEach(_ =>
            {
                Game.log($">> moveCoreFile: {_}");
                File.Copy(_, _.Replace(oldFolder, dataFolder), true);
            });
        }

        private static void mergeScannedBioEntryIds(string oldFolder)
        {
            var cmdrFiles = Directory.EnumerateFiles(oldFolder, "*-live.json");
            foreach (var cmdrFile in cmdrFiles)
            {
                var oldCmdr = Data.Load<CommanderSettings>(cmdrFile)!;
                var newCmdr = Data.Load<CommanderSettings>(cmdrFile.Replace(oldFolder, dataFolder))!;
                if (newCmdr == null) newCmdr = CommanderSettings.Load(oldCmdr.fid, oldCmdr.isOdyssey, oldCmdr.commander);

                Game.log($">> mergeScannedBioEntryIds: {cmdrFile}");

                // merge prior scans, both types, and update total
                BodyData.migrate_ScannedOrganics_Into_ScannedBioEntryIds(oldCmdr);
                foreach (var old in oldCmdr.scannedBioEntryIds)
                    newCmdr.scannedBioEntryIds.Add(old);

                newCmdr.reCalcOrganicRewards();

                // force these migrations to happen again
                newCmdr.migratedNonSystemDataOrganics = false;
                newCmdr.migratedScannedOrganicsInEntryId = false;
                newCmdr.Save();
            }
        }

        private static void mergeChildFiles(string oldFolder)
        {
            var allOldFiles = new List<string>();
            if (Directory.Exists(Path.Combine(oldFolder, "guardian")))
                allOldFiles.AddRange(Directory.EnumerateFiles(Path.Combine(oldFolder, "guardian"), "*.*", SearchOption.AllDirectories));
            if (Directory.Exists(Path.Combine(oldFolder, "guardian-beacon")))
                allOldFiles.AddRange(Directory.EnumerateFiles(Path.Combine(oldFolder, "guardian-beacon"), "*.*", SearchOption.AllDirectories));
            if (Directory.Exists(Path.Combine(oldFolder, "organic")))
                allOldFiles.AddRange(Directory.EnumerateFiles(Path.Combine(oldFolder, "organic"), "*.*", SearchOption.AllDirectories));
            if (Directory.Exists(Path.Combine(oldFolder, "systems")))
                allOldFiles.AddRange(Directory.EnumerateFiles(Path.Combine(oldFolder, "systems"), "*.*", SearchOption.AllDirectories));

            Game.log($"> Merging {allOldFiles.Count} files (or clobbering if they are larger)");

            foreach (var oldFile in allOldFiles)
            {
                var newFile = oldFile.Replace(oldFolder, dataFolder);
                Directory.CreateDirectory(Path.GetDirectoryName(newFile)!);

                if (!File.Exists(newFile))
                {
                    // copy old if it does not exist
                    File.Copy(oldFile, newFile, true);
                }
                else
                {
                    // if old is larger, overwrite new with old
                    var oldLength = new FileInfo(oldFile).Length;
                    var newLength = new FileInfo(newFile).Length;
                    if (oldLength > newLength)
                        File.Copy(oldFile, newFile, true);
                }
            }
            Game.log($"> Merging {allOldFiles.Count} files - complete");
        }

        #endregion
    }

    interface PlotterForm
    {
        void reposition(Rectangle gameRect);
        double Opacity { get; set; }
        void Invalidate();
    }
}
