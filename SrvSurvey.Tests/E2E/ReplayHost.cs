using SrvSurvey.game;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

// SrvSurvey is full of static game state - E2E sessions must never overlap
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Redirects every SrvSurvey data path into a throw-away folder BEFORE any SrvSurvey type
/// with a static path initializer (Settings, Game, CodexRef, ...) gets touched.
/// A real %appdata%\SrvSurvey must never be read or written by the tests.
/// </summary>
internal static class ReplayIsolation
{
    public static readonly string Root = Path.Combine(Path.GetTempPath(), "SrvSurveyE2E", DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Environment.ProcessId);
    public static readonly string DataFolder = Path.Combine(Root, "data");

    [ModuleInitializer]
    internal static void Init()
    {
        Directory.CreateDirectory(DataFolder);
        Program.dataFolder = DataFolder;
        Program.dataFolder2 = DataFolder;
        typeof(Program).GetField("dataRootFolder", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, Root);
    }
}

/// <summary>
/// Hosts the real SrvSurvey Game object on a dedicated STA/WinForms thread (as Program.Main would)
/// and feeds it journal + Status.json files through the same FileSystemWatcher pipeline the app uses.
/// </summary>
internal sealed class ReplayHost : IDisposable
{
    private static Thread? uiThread;
    private static Control? ui;
    private static readonly object initLock = new();

    public readonly string Name;
    public readonly string JournalFolder;
    public readonly string JournalPath;
    public Game? game;
    private int logStart;

    public ReplayHost(string name, string journalFileName)
    {
        this.Name = name;
        this.JournalFolder = Path.Combine(ReplayIsolation.Root, "journals-" + name);
        Directory.CreateDirectory(this.JournalFolder);
        this.JournalPath = Path.Combine(this.JournalFolder, journalFileName);
        EnsureUiThread();
        this.logStart = OnUi(() => Game.logs.Count);
    }

    public static string RepoRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SrvSurvey.sln"))) dir = dir.Parent;
            return dir?.FullName ?? throw new Exception("Cannot find SrvSurvey.sln above " + AppContext.BaseDirectory);
        }
    }

    public static string BioCriteriaFolder => Path.Combine(ReplayIsolation.DataFolder, "pub", "bio-criteria");

    public static string Fixture(string name) => Path.Combine(RepoRoot, "SrvSurvey.Tests", "E2E", "Fixtures", name);

    private static void EnsureUiThread()
    {
        lock (initLock)
        {
            if (ui != null) return;

            var ready = new ManualResetEventSlim();
            Exception? bootError = null;
            uiThread = new Thread(() =>
            {
                try
                {
                    // mirror Program.Main
                    var onThreadException = typeof(Program).GetMethod("Application_ThreadException", BindingFlags.NonPublic | BindingFlags.Static)!;
                    Application.ThreadException += (ThreadExceptionEventHandler)Delegate.CreateDelegate(typeof(ThreadExceptionEventHandler), onThreadException);
                    Newtonsoft.Json.JsonConvert.DefaultSettings = () => new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new CustomContractResolver() };
                    var control = new Control();
                    control.CreateControl();
                    typeof(Program).GetProperty(nameof(Program.control))!.SetValue(null, control);

                    // seed the offline reference data the app would otherwise download
                    File.Copy(Path.Combine(RepoRoot, "docs", "codexRef.json"), Path.Combine(ReplayIsolation.DataFolder, "codexRef.json"), true);
                    File.Copy(Path.Combine(RepoRoot, "docs", "nebulae.json"), Path.Combine(ReplayIsolation.DataFolder, "nebulae.json"), true);
                    File.WriteAllText(Path.Combine(ReplayIsolation.DataFolder, "codexNotFound.json"), "{}"); // else fetched from a Google sheet
                    System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(RepoRoot, "data", "bio-criteria.zip"), BioCriteriaFolder, true); // else downloaded by Git.refreshPublishedData

                    Settings.Load();
                    AssertIsolated();
                    foreach (var name in new[] { nameof(Program.scaleFactor), nameof(Program.fontScaleFactor) })
                    {
                        var getter = typeof(Program).GetMethod("get" + char.ToUpper(name[0]) + name[1..], BindingFlags.NonPublic | BindingFlags.Static)!;
                        typeof(Program).GetProperty(name)!.SetValue(null, getter.Invoke(null, null));
                    }
                    Game.settings.useExternalData = false;
                    Game.settings.eddnUpload = false;
                    Game.settings.lastCodexRefDownload = DateTime.Now;
                    // off the UI thread: its awaits would otherwise wait on this blocked SynchronizationContext
                    Task.Run(() => Game.codexRef.init(false)).GetAwaiter().GetResult();

                    // FormErrorSubmit.Show only logs (no modal Oops dialog) while this is set
                    Main.isClosing = true;
                    Program.useLastIfShutdown = true;

                    ui = control;
                }
                catch (Exception ex)
                {
                    bootError = ex;
                }
                finally
                {
                    ready.Set();
                }
                if (bootError == null) Application.Run();
            });
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.IsBackground = true;
            uiThread.Name = "E2E-UI";
            uiThread.Start();
            if (!ready.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException("E2E boot did not finish within 60s");
            if (bootError != null) throw new Exception("E2E boot failed", bootError);
        }
    }

    private static void AssertIsolated()
    {
        var logPath = (string)typeof(Game).GetField("logPath", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        foreach (var path in new[] { logPath, Program.dataFolder })
            if (!path.StartsWith(ReplayIsolation.Root, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"E2E isolation broken, path escapes the temp root: {path}");
    }

    public T OnUi<T>(Func<T> func) => (T)ui!.Invoke(func);
    public void OnUi(Action func) => ui!.Invoke(func);

    /// <summary> Write Status.json the way the game does (whole file replaced) </summary>
    public void WriteStatus(string json) => WriteGameFile("Status.json", json);

    /// <summary> Write one of the side files the game keeps next to the journals (Status.json, NavRoute.json, ...) </summary>
    public void WriteGameFile(string name, string json)
    {
        File.WriteAllText(Path.Combine(this.JournalFolder, name), json);
    }

    /// <summary> Write a file into SrvSurvey's own (isolated) data folder, eg: routes/F123.json </summary>
    public static void WriteDataFile(string relativePath, string json)
    {
        var path = Path.Combine(ReplayIsolation.DataFolder, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
    }

    /// <summary> Wait until the UI thread drained pending work and the given log fragment appeared (or time out) </summary>
    public bool WaitForLog(string fragment, int timeoutMs = 5_000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (CountLog(fragment) > 0) return true;
            Thread.Sleep(50);
        }
        return false;
    }

    /// <summary> Write the initial journal lines and boot a Game from them, exactly as Main does when it sees a journal </summary>
    public void Boot(IEnumerable<string> initialLines, string cmdr)
    {
        File.WriteAllLines(this.JournalPath, initialLines);
        if (!File.Exists(Path.Combine(this.JournalFolder, "Status.json")))
            WriteStatus("{ \"timestamp\":\"2026-01-01T00:00:00Z\", \"event\":\"Status\", \"Flags\":0 }");

        // SystemData caches by address only (not by commander) - start every session cold so tests cannot leak bodies into each other
        OnUi(() => ((System.Collections.IDictionary)typeof(SystemData).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!).Clear());
        this.logStart = OnUi(() => Game.logs.Count);
        this.game = OnUi(() =>
        {
            Game.settings.watchedJournalFolder = this.JournalFolder;
            return new Game(cmdr);
        });
        if (this.game.isShutdown) throw new Exception("Game booted as shutdown - fixture is missing LoadGame/Commander?");
    }

    /// <summary> Append journal lines and wait until the live watcher pipeline has dispatched all of them </summary>
    public void Append(IList<string> lines, int timeoutMs = 15_000)
    {
        if (lines.Count == 0) return;
        var lastEvent = Newtonsoft.Json.Linq.JObject.Parse(lines.Last())["event"]!.ToString();
        var marker = $"Game.event => {lastEvent} ";
        var expected = CountLog(marker) + lines.Count(l => Newtonsoft.Json.Linq.JObject.Parse(l)["event"]!.ToString() == lastEvent);

        using (var fs = new FileStream(this.JournalPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
            foreach (var line in lines) sw.WriteLine(line);

        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        while (CountLog(marker) < expected)
        {
            if (sw2.ElapsedMilliseconds > timeoutMs)
                throw new TimeoutException($"Journal watcher dispatched {CountLog(marker)}/{expected} '{lastEvent}' within {timeoutMs}ms");
            Thread.Sleep(50);
            // FileSystemWatcher can get stale - the app pokes it too
            OnUi(() => this.game!.journals?.poke());
        }
        // let deferred BeginInvoke work on the UI thread drain
        OnUi(() => { });
        OnUi(() => { });
    }

    public List<string> Logs => OnUi(() => Game.logs.Skip(this.logStart).ToList());

    public int CountLog(string fragment) => Logs.Count(l => l.Contains(fragment));

    /// <summary>
    /// Every exception the app caught and reported: journal event handlers, crashGuard, UI thread exceptions, the Oops dialog
    /// (logged instead of shown while Main.isClosing) and the swallowed ones in Status.json parsing and fire-and-forget tasks.
    /// </summary>
    public List<string> AppErrors => Logs.Where(l =>
        l.Contains("Exception processing event")
        || l.Contains("FormErrorSubmit.show:")
        || l.Contains("crashGuard:")
        || l.Contains("parseStatusFile: System.")
        // Task.justDoIt() swallows faulted tasks into this log line - network trouble is environmental, anything else is a bug
        || (l.Contains("isFirewallProblem?") && !l.Contains("HttpRequestException") && !l.Contains("TaskCanceledException") && !l.Contains("SocketException"))).ToList();

    /// <summary> Write the session's slice of the SrvSurvey log as a re-checkable artifact </summary>
    public string WriteArtifact(string summary)
    {
        var dir = Environment.GetEnvironmentVariable("SRVSURVEY_E2E_ARTIFACTS") ?? Path.Combine(ReplayIsolation.Root, "artifacts");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{this.Name}.log");
        File.WriteAllText(path, $"# {this.Name}\r\n# {summary}\r\n# data root: {ReplayIsolation.Root}\r\n\r\n" + string.Join("\r\n", Logs));
        return path;
    }

    public void Dispose()
    {
        // mirror Main.removeGame
        if (this.game != null) OnUi(() =>
        {
            Program.closeAllPlotters();
            SrvSurvey.plotters.PlotBase2.closeAll();
            this.game.Dispose();
        });
    }
}
