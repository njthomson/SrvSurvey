using SrvSurvey.game;
using SrvSurvey.plotters;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

[assembly: System.Resources.NeutralResourcesLanguage("en")]

namespace SrvSurvey
{
    static class Program
    {
        public static Control control { get; private set; }
        public static string dataFolder = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SrvSurvey", "SrvSurvey", "1.1.0.0"));
        public static string dataFolder2 = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming", "SrvSurvey", "SrvSurvey", "1.1.0.0"));
        public static bool isAppStoreBuild = Assembly.GetExecutingAssembly().Location.Contains("NosmohtSoftware");
#if DEBUG
        public static string releaseVersion = $"1.{DateTime.Now.Year}.{(DateTime.Now.Month * 100) + DateTime.Now.Day}.{(DateTime.Now.Hour * 100) + DateTime.Now.Minute}";
#else
        public static string releaseVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version!;
#endif
        public static string userAgent = $"SrvSurvey-{Program.releaseVersion}";
        public static bool useLastIfShutdown = false;
        public static bool isLinux = false;

        public static string cmdArgScanOld = "-scan-old";
        public static string cmdArgRestart = "-restart";
        public static string cmdArgLinux = "-linux";

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
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                if (!string.IsNullOrEmpty(Game.settings.lang))
                {
                    var culture = System.Globalization.CultureInfo.CreateSpecificCulture(Game.settings.lang);
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = culture;
                }

                Application.EnableVisualStyles();
                Application.SetHighDpiMode(HighDpiMode.PerMonitor);
                Application.SetCompatibleTextRenderingDefault(true);
                Application.ThreadException += Application_ThreadException;

                // create some control for invoking back onto the UI thread
                Program.control = new Control();
                Program.control.CreateControl();

                // BEFORE we touch the main code - check if we're an AppStore build using a relocated data folder, meaning are we using:
                // %USERPROFILE%\AppData\Local\Packages\35333NosmohtSoftware.142860789C73F_p4c193bsm1z5a\LocalCache\Roaming\SrvSurvey
                // instead of the correct folder:
                // %appdata%\SrvSurvey\...
                if (Program.checkAndMigrateAppStoreRoamingFolder())
                    return;

                Program.isLinux = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WINELOADER")) || args.Any(a => a == Program.cmdArgLinux);
                var invokePostProcessor = args.Any(a => a == Program.cmdArgScanOld);
                var restarted = args.Any(a => a == Program.cmdArgRestart);
                if (!restarted && !invokePostProcessor)
                {
                    var processes = Process.GetProcessesByName("SrvSurvey");
                    if (processes.Length > 1)
                    {
                        MessageBox.Show("SrvSurvey is already running.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        var otherProcess = processes.FirstOrDefault(p => p.Id != Process.GetCurrentProcess().Id);
                        // push prior process to the foreground
                        if (otherProcess != null)
                        {
                            // TODO: check if it is minimized?
                            Elite.SetForegroundWindow(otherProcess.MainWindowHandle);
                        }

                        return;
                    }
                }

                Form mainForm = invokePostProcessor ? new FormPostProcess(args.LastOrDefault()) : new Main();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                Game.log($"Failed to boot SrvSurvey: {ex.Message}\r\n{ex.StackTrace}");
                MessageBox.Show($"An unexpected error occurred. Please report the following on https://github.com/njthomson/SrvSurvey/issues:\r\n\r\n{ex}", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            game.Game.log("Exception: " + e.Exception.Message);
            game.Game.log("Inner exception: " + e.Exception.InnerException?.Message);
            game.Game.log("Stack: " + e.Exception.StackTrace);

            FormErrorSubmit.Show(e.Exception);
        }

        public static void crashGuard(Action func)
        {
            crashGuard(false, func);
        }

        public static void crashGuard(bool beginInvoke, Action func)
        {
            try
            {
                if (beginInvoke)
                {
                    control.BeginInvoke(func);
                }
                else
                {
                    func();
                }
            }
            catch (Exception ex)
            {
                Game.log($"crashGuard {ex}");
                Program.control.BeginInvoke(() =>
                {
                    FormErrorSubmit.Show(ex);
                });
            }
        }

        #region plotter tracking

        private static Dictionary<string, PlotterForm> activePlotters = new Dictionary<string, PlotterForm>();

        public static T? showPlotter<T>(Rectangle? gameRect = null) where T : PlotterForm
        {
            var formType = typeof(T);

            // exit early if the game does not have focus
            if (!Elite.focusElite) 
            //if (!Debugger.IsAttached && (!Elite.focusElite || Elite.focusSrvSurvey)) // Maybe not "|| Elite.focusSrvSurvey" ?
                return (T?)activePlotters.GetValueOrDefault(formType.Name);

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

                //// have new plotters fade in
                ////form.Size = Size.Empty;
                ////form.Opacity = 0;
                //form.Load += new EventHandler((object? sender, EventArgs e) =>
                //{
                //    // using the quicker fade-out duration
                //    //form.Opacity = 0;
                //    control.BeginInvoke(() =>
                //    {
                //        Util.fadeOpacity((Form)(object)form, 1, Game.settings.fadeInDuration);
                //    });

                //    //Util.fadeOpacity((Form)(object)form, 1, 500); // 100ms
                //});

                // add to list, then show
                activePlotters.Add(formType.Name, form);
            }
            else
            {
                form = (T)activePlotters[formType.Name];

                // don't try showing a disposed form
                if (form.IsDisposed)
                {
                    activePlotters.Remove(formType.Name);
                    return showPlotter<T>(gameRect);
                }

                form.Invalidate();
            }

            // show form if not visible
            if (!form.Visible)
            {
                Game.log($"Program.Showing plotter: {formType.Name}");
                gameRect ??= Elite.getWindowRect();
                form.reposition(gameRect.Value);

                form.showing = true;
                try
                {
                    form.Show();
                }
                finally
                {
                    form.showing = false;
                }
            }

            return form;
        }

        public static void closePlotter<T>(bool async = false) where T : Form
        {
            if (async)
                Program.control.BeginInvoke(() => closePlotter(typeof(T).Name));
            else
                closePlotter(typeof(T).Name);
        }

        private static void closePlotter(string name)
        {
            var plotter = Program.activePlotters.GetValueOrDefault(name);

            if (plotter != null)
            {
                Game.log($"Program.Closing plotter: {name}");
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
            return activePlotters.GetValueOrDefault(typeof(T).Name) as T;
        }

        public static Form? getPlotter(string name)
        {
            return activePlotters.GetValueOrDefault(name) as Form;
        }

        public static string[] getAllPlotterNames()
        {
            return activePlotters.Keys.Order().ToArray();
        }

        public static void closeAllPlotters(bool exceptPlotPulse = false, bool exceptJumpInfo = false)
        {
            Game.log($"Program.CloseAllPlotters");

            var names = Program.activePlotters.Keys.ToArray();
            foreach (string name in names)
            {
                if (name == nameof(PlotJumpInfo) && exceptJumpInfo) continue;
                Program.closePlotter(name);
            }

            // we want to reset this plotter, so create it again
            if (exceptPlotPulse && !Game.settings.hideJournalWriteTimer)
                Program.showPlotter<PlotPulse>();
        }

        public static void repositionPlotters()
        {
            repositionPlotters(Elite.getWindowRect());
        }

        public static void repositionPlotters(Rectangle rect)
        {
            if (rect.X < -30_000 || rect.Y < -30_000 || rect.Width == 0 || rect.Height == 0) return;
            Game.log($"Program.repositionPlotters: {activePlotters.Count}, rect{rect}");

            foreach (PlotterForm form in activePlotters.Values)
                form.reposition(rect);
        }

        public static void hideActivePlotters()
        {
            //Game.log($"Program.hideActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Opacity = 0;
        }

        public static void showActivePlotters()
        {
            if (Program.tempHideAllPlotters || !Elite.gameHasFocus) return;

            //Game.log($"Program.showActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Opacity = PlotPos.getOpacity(form);
        }

        public static void invalidate<T>(bool defer = false) where T : Form
        {
            if (defer)
            {
                Program.defer(() => Program.invalidate<T>(false));
                return;
            }

            var plotter = activePlotters.GetValueOrDefault(typeof(T).Name) as T;
            plotter?.Invalidate();
        }

        public static void invalidateActivePlotters()
        {
            if (Program.control.InvokeRequired)
                Program.control.BeginInvoke(() => Program.invalidateActivePlotters());

            // Game.log($"Program.invalidateActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Invalidate();
        }

        public static bool tempHideAllPlotters = false;

        #endregion

        #region migrate data files

        public static List<string> getMigratableFolders()
        {
            // do we have any settings files in any folder already?
            var folders = Directory.EnumerateFiles(dataRootFolder, "settings.json", SearchOption.AllDirectories)
                .Where(_ => !_.EndsWith("\\SrvSurvey\\1.1.0.0\\settings.json"))
                .Select(_ => Path.GetDirectoryName(_)!)
                .ToList();

            return folders;
        }

        public static void migrateToNewDataFolder()
        {
            try
            {
                // make a .zip backup of everything before beginning
                var datepart = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var zipBackupPath = Path.GetFullPath(Path.Combine(dataRootFolder, "..", $"pre-migration-backup-{datepart}.zip"));
                ZipFile.CreateFromDirectory(dataRootFolder, zipBackupPath, CompressionLevel.SmallestSize, false);

                Game.log($"migrateToNewDataFolder: old data into common folder: {Program.dataFolder} ...");
                Directory.CreateDirectory(Program.dataFolder);

                var rootFolder = new DirectoryInfo(Path.GetFullPath(Path.Combine(Program.dataFolder, "..")));
                var oldFolders = getMigratableFolders()
                    // order by oldest first
                    .OrderBy(_ => File.GetLastWriteTime(Path.Combine(_, "settings.json")))
                    .ToList();
                Game.log($"Migrating old folders:\r\n  " + string.Join("\r\n  ", oldFolders));

                // move core files from the most recent folder only
                moveCoreFiles(oldFolders.Last());

                oldFolders.ForEach(_ => mergeScannedBioEntryIds(_));
                oldFolders.ForEach(_ => mergeChildFiles(_));
            }
            catch (Exception ex)
            {
                Game.log($"migrateToNewDataFolder: exception: {ex}");
            }
            finally
            {
                Game.log($"migrateToNewDataFolder: old data into common folder - complete");
            }
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
                if (newCmdr == null) newCmdr = CommanderSettings.Load(oldCmdr.fid, oldCmdr.isOdyssey, oldCmdr.commander, true);

                Game.log($">> mergeScannedBioEntryIds: {cmdrFile}");

                // merge prior scans, both types, and update total
                BodyDataOld.migrate_ScannedOrganics_Into_ScannedBioEntryIds(oldCmdr);
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

        public static async Task migrate_BodyData_Into_SystemData()
        {
            Game.log($"migrate_BodyData_Into_SystemData ...");

            var cmdrFiles = Directory.EnumerateFiles(dataFolder, "*-live.json");
            foreach (var cmdrFile in cmdrFiles)
            {
                var newCmdr = Data.Load<CommanderSettings>(cmdrFile)!;
                await SystemData.migrate_BodyData_Into_SystemData(newCmdr);
            }

            Game.log($"migrateToNewDataFolder - complete");
        }

        #endregion

        #region migrate AppStore folders

        public static bool checkAndMigrateAppStoreRoamingFolder()
        {
            var redirectedRoamingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages/35333NosmohtSoftware.142860789C73F_p4c193bsm1z5a/LocalCache/Roaming/SrvSurvey/SrvSurvey");
            var redirectedRoamingFolder2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages/35333NosmohtSoftware.142860789C73F_p4c193bsm1z5a/LocalCache/Roaming/SrvSurvey/SrvSurvey-");

            try
            {
                //// tmp
                //var count1 = Directory.Exists(redirectedRoamingFolder) ? Directory.GetFiles(redirectedRoamingFolder, "*.*", SearchOption.AllDirectories).Length : -1;
                //var count2 = Directory.Exists(redirectedRoamingFolder2) ? Directory.GetFiles(redirectedRoamingFolder2, "*.*", SearchOption.AllDirectories).Length : -1;
                //var count3 = Directory.Exists(dataFolder) ? Directory.GetFiles(dataFolder, "*.*", SearchOption.AllDirectories).Length : -1;
                //var count4 = Directory.Exists(dataFolder2) ? Directory.GetFiles(dataFolder2, "*.*", SearchOption.AllDirectories).Length : -1;
                //MessageBox.Show($"A) counts: {count1}, {count2}, {count3}, {count4}\r\n\r\n{dataFolder}\r\n{dataFolder2}");
                //// tmp

                if (Directory.Exists(redirectedRoamingFolder) && !Directory.Exists(redirectedRoamingFolder2))
                {
                    // first - rename redirected folder with a trailing '-' so it won't cause problems on the next run
                    Directory.Move(redirectedRoamingFolder, $"{redirectedRoamingFolder}-");

                    // ensure the real folder exists
                    var info1 = new ProcessStartInfo("cmd.exe", $"/c \"md %appdata%\\SrvSurvey\\SrvSurvey\\1.0.0.0\"");
                    Process.Start(info1)?.WaitForExit();

                    MessageBox.Show($"SrvSurvey needs to make a one time update and will now restart.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // restart regardless
                    forceRestart();
                    return true;
                }
                else if (Directory.Exists(redirectedRoamingFolder) && Directory.Exists(redirectedRoamingFolder2))
                {
                    // there will be trouble if both folders exist, so rename to avoid that
                    Directory.Move(redirectedRoamingFolder, $"x{redirectedRoamingFolder}");
                }

                var hasAppStoreRedirectedRoamingFolder2 = Directory.Exists(redirectedRoamingFolder2);
                if (hasAppStoreRedirectedRoamingFolder2)
                {
                    var filenames = Directory.GetFiles(redirectedRoamingFolder2, "*.*", SearchOption.AllDirectories);
                    hasAppStoreRedirectedRoamingFolder2 = filenames.Length > 0;
                }

                // exit here if we don't have this folder
                if (!hasAppStoreRedirectedRoamingFolder2) return false;

                var realRoamingFolder = dataFolder;
                var hasRealRoamingFolder = Directory.Exists(realRoamingFolder);
                if (hasRealRoamingFolder)
                {
                    var filenames = Directory.GetFiles(realRoamingFolder, "*.*", SearchOption.AllDirectories);
                    hasRealRoamingFolder = filenames.Length > 0;
                }

                // move user data if it is in the redirected folder
                if (!hasRealRoamingFolder)
                {
                    // but folders still redirected - must do the move this way
                    var info2 = new ProcessStartInfo("cmd.exe", $"/c \"move /Y {redirectedRoamingFolder2} %appdata%\\SrvSurvey & rd %appdata%\\SrvSurvey\\SrvSurvey /s /q & move /Y %appdata%\\SrvSurvey\\SrvSurvey- %appdata%\\SrvSurvey\\SrvSurvey");
                    Process.Start(info2)?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                var count1 = Directory.Exists(redirectedRoamingFolder) ? Directory.GetFiles(redirectedRoamingFolder, "*.*", SearchOption.AllDirectories).Length : -1;
                var count2 = Directory.Exists(redirectedRoamingFolder2) ? Directory.GetFiles(redirectedRoamingFolder2, "*.*", SearchOption.AllDirectories).Length : -1;
                var count3 = Directory.Exists(dataFolder) ? Directory.GetFiles(dataFolder, "*.*", SearchOption.AllDirectories).Length : -1;
                var count4 = Directory.Exists(dataFolder2) ? Directory.GetFiles(dataFolder2, "*.*", SearchOption.AllDirectories).Length : -1;

                MessageBox.Show($"An unexpected error occurred. Please report the following on https://github.com/njthomson/SrvSurvey/issues:\r\n\r\nDiagnostic counts: [{count1}, {count2}, {count3}, {count4}]\n\n {ex.Message}\n\n{ex.StackTrace}", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.GetCurrentProcess().Kill();
                return true;
            }

            return false;
        }

        #endregion

        public static void forceRestart()
        {
            // force a restart
            Application.DoEvents();
            Application.DoEvents();
            Process.Start(Application.ExecutablePath, Program.cmdArgRestart);
            Application.DoEvents();
            Process.GetCurrentProcess().Kill();
        }

        /// <summary> Defer the given action behind BeginInvoke </summary>
        public static void defer(Action func)
        {
            Program.control.BeginInvoke(func);
        }
    }

    interface PlotterForm
    {
        void reposition(Rectangle gameRect);
        double Opacity { get; set; }
        void Invalidate();
        int Width { get; set; }
        int Height { get; set; }
        Point Location { get; set; }
        Size Size { get; set; }
        bool Visible { get; set; }
        string Name { get; set; }
        bool IsDisposed { get; }
        void Close();
        void Show();
        event EventHandler? Load;
        bool didFirstPaint { get; set; }
        bool showing { get; set; }
    }
}
