using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Cluster: "Operations that change non-concurrent collections must have exclusive access" from
/// Game.checkModeChange -> status.changed.Add (upstream #1037 #1034).
/// Status.json is parsed on the FileSystemWatcher thread (status.changed.Clear/Add) while journal events
/// on the UI thread call checkModeChange (status.changed.Add("mode")) on the very same HashSet.
/// </summary>
public sealed class StatusRaceReplayTests
{
    [Fact]
    public void Issue1037_StatusFileChurnDuringModeChanges_NoCollectionCorruption()
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", "F0001037")).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        using var host = new ReplayHost("issue1037-status-race", "Journal.2026-08-19T100112.01.log");
        host.Boot(lines.Take(bootEnd), "E2E TESTER");

        // every MainMenu <-> Exploration flip is a mode change => checkModeChange adds "mode" to status.changed
        var music = Enumerable.Range(0, 400)
            .Select(n => $$"""{ "timestamp":"2026-08-19T07:10:{{n % 60:00}}Z", "event":"Music", "MusicTrack":"{{(n % 2 == 0 ? "MainMenu" : "Exploration")}}" }""")
            .ToList();

        // meanwhile the game rewrites Status.json as fast as it can, toggling flags so every parse adds to status.changed
        var stop = false;
        var statusWrites = 0;
        var writer = new Thread(() =>
        {
            while (!Volatile.Read(ref stop))
            {
                var flags = statusWrites % 2 == 0 ? 16777224 : 16777228; // InMainShip|ShieldsUp (+/- LandingGear)
                try
                {
                    host.WriteStatus($$"""{ "timestamp":"2026-08-19T07:10:00Z", "event":"Status", "Flags":{{flags}}, "Flags2":0, "Pips":[4,4,4], "FireGroup":{{statusWrites % 3}}, "GuiFocus":0, "Cargo":0.0, "LegalState":"Clean", "Balance":{{statusWrites}} }""");
                    statusWrites++;
                }
                catch (IOException) { /* file momentarily held by the app's reader - the game hits this too */ }
            }
        });
        writer.Start();
        try
        {
            host.Append(music, 120_000);
        }
        finally
        {
            Volatile.Write(ref stop, true);
            writer.Join();
        }

        var statusParses = host.CountLog("Mode change");
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} musicEvents={music.Count} statusWrites={statusWrites} modeChanges={statusParses}");

        Assert.True(statusWrites > 100, $"writer only managed {statusWrites} Status.json writes - not a real race");
        Assert.True(statusParses >= music.Count / 2, $"only {statusParses} mode changes - music events not driving checkModeChange, see {artifact}");
        Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(5))}");
    }
}
