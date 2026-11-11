using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using System.Net;
using System.Reflection;
using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Start-up with no network (DNS failure: HttpRequestException without a status code), as Main.afterMainLoad does it.
/// Upstream: #1045 (prepCodexNotFounds timeout), #1038 #1005 (403/HttpRequestException in codex init), #963 #847 (Git refresh DNS failure).
/// </summary>
public sealed class OfflineReplayTests
{
    /// <summary> What HttpClient throws when DNS lookup fails </summary>
    private sealed class OfflineHandler : HttpMessageHandler
    {
        public int calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref calls);
            throw new HttpRequestException($"No such host is known. ({request.RequestUri?.Host}:443)");
        }
    }

    private static HttpClient swapClient(Type type, HttpClient client)
    {
        var field = type.GetField("client", BindingFlags.NonPublic | BindingFlags.Static)!;
        var prior = (HttpClient)field.GetValue(null)!;
        field.SetValue(null, client);
        return prior;
    }

    [Fact]
    public void Issue1045_CodexRefInitOfflineAfterAWeek_UsesFilesOnDisk()
    {
        using var host = new ReplayHost("issue1045-codex-offline", "Journal.2026-08-19T100112.01.log"); // boots the UI thread + data folder
        var dataFolder = ReplayIsolation.DataFolder;
        var nebulaePath = Path.Combine(dataFolder, "nebulae.json");
        var logStart = host.OnUi(() => Game.logs.Count);

        var offline = new OfflineHandler();
        var prior = swapClient(typeof(CodexRef), new HttpClient(offline));
        var priorDownload = Game.settings.lastCodexRefDownload;
        Exception? failure = null;
        try
        {
            // a week since the last download => init forces a fresh download of everything
            Game.settings.lastCodexRefDownload = DateTime.Now.AddDays(-8);
            try
            {
                Task.Run(() => Game.codexRef.init(false)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        }
        finally
        {
            swapClient(typeof(CodexRef), prior);
            Game.settings.lastCodexRefDownload = priorDownload;
        }

        var genusCount = Game.codexRef.genus?.Count ?? 0;
        var nebulaeKept = File.Exists(nebulaePath);
        // every file init falls back on must survive the failed refresh
        var codexRefKept = new FileInfo(Path.Combine(dataFolder, "codexRef.json")) is { Exists: true, Length: > 1000 };
        var bioRefKept = new FileInfo(Path.Combine(dataFolder, "bioRef.json")) is { Exists: true, Length: > 1000 };
        var logs = host.OnUi(() => Game.logs.Skip(logStart).ToList());
        var artifact = host.WriteArtifact($"initFailure={failure?.GetType().Name}:{failure?.Message} networkCalls={offline.calls} genus={genusCount} nebulaeKept={nebulaeKept} codexRefKept={codexRefKept} bioRefKept={bioRefKept}");

        Assert.True(offline.calls >= 3, $"init did not try the network ({offline.calls} calls) - premise broken, see {artifact}");
        Assert.True(failure == null, $"codexRef.init failed offline: {failure}");
        Assert.True(genusCount > 0, $"no bio genus loaded from disk, see {artifact}");
        Assert.True(nebulaeKept, $"nebulae.json was deleted although the re-download failed, see {artifact}");
        Assert.True(codexRefKept && bioRefKept, $"codexRef.json/bioRef.json lost after the failed refresh, see {artifact}");
    }

    [Fact]
    public void Issue963_GitRefreshOffline_NoErrorDialog()
    {
        using var host = new ReplayHost("issue963-git-offline", "Journal.2026-08-19T100112.01.log");
        var offline = new OfflineHandler();
        var prior = swapClient(typeof(Git), new HttpClient(offline));
        bool? updateAvailable = null;
        try
        {
            updateAvailable = Task.Run(() => Game.git.refreshPublishedData()).GetAwaiter().GetResult();
            host.OnUi(() => { }); // let any deferred Oops dialog reach the log
        }
        finally
        {
            swapClient(typeof(Git), prior);
        }

        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} networkCalls={offline.calls} updateAvailable={updateAvailable}");

        Assert.True(offline.calls >= 1, $"refreshPublishedData never hit the network - premise broken, see {artifact}");
        Assert.True(errors.Count == 0, $"offline start-up raised {errors.Count} error(s) a player would see, see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
    }
}
