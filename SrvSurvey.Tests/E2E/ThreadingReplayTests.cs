using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Regression guards for work moved from thread-pool continuations onto the UI thread (P2 of the crash scan):
/// Touchdown (cmdr.setTouchdown + Save) and Util.deferAfter. (fetchSystemDataEnd/predictSystemSpecies need external
/// data, which the harness turns off - they are not covered.) The races could not be reproduced on demand; these only
/// prove the moved work still happens, on the UI thread, without errors.
/// </summary>
public sealed class ThreadingReplayTests
{
    [Fact]
    public void TouchdownStillRecordedAfterMovingToUiThread()
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", "F0002738")).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;
        var touchdown = Array.FindIndex(lines, l => l.Contains("\"event\":\"Touchdown\""));

        using var host = new ReplayHost("p2-touchdown-ui-thread", "Journal.2026-08-19T100112.01.log");
        host.Boot(lines.Take(bootEnd), "E2E TESTER");
        host.Append(lines[bootEnd..(touchdown + 1)]);
        var recorded = host.WaitForLog(" ago, at: ", 2_000); // logged by the deferred Touchdown handler

        // deferAfter from a non-UI thread must still run (it now hops to the UI thread first)
        var ran = new ManualResetEventSlim();
        var ranOnUi = false;
        Task.Run(() => Util.deferAfter(50, () => { ranOnUi = !Program.control.InvokeRequired; ran.Set(); })).Wait();
        var deferred = ran.Wait(2_000);

        var state = host.OnUi(() => { var t = host.game!.cmdr.lastTouchdownLocation; return (lat: t == null ? (decimal?)null : (decimal)t.Lat, lng: t == null ? (decimal?)null : (decimal)t.Long); });
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} touchdownLogged={recorded} cmdrTouchdown={state.lat},{state.lng} deferAfterRan={deferred} onUi={ranOnUi}");

        Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
        Assert.True(recorded, $"Touchdown never processed, see {artifact}");
        Assert.Equal(-46.624676m, state.lat);
        Assert.Equal(66.149605m, state.lng);
        Assert.True(deferred && ranOnUi, $"deferAfter from a background thread did not run on the UI thread, see {artifact}");
    }
}
