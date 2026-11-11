using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Cluster: PlotJumpInfo.initFromRoute crashes when the FSD starts charging (upstream #985 #986).
/// </summary>
public sealed class JumpInfoReplayTests
{
    // Flying in normal space, FSD charging for a hyperspace jump => PlotJumpInfo.allowed
    private const string statusCharging = """{ "timestamp":"2026-08-19T07:03:00Z", "event":"Status", "Flags":16908296, "Flags2":524288, "Pips":[4,4,4], "FireGroup":0, "GuiFocus":0, "Cargo":0.0, "LegalState":"Clean", "Balance":1 }""";
    private const string statusFlying = """{ "timestamp":"2026-08-19T07:02:59Z", "event":"Status", "Flags":16777224, "Flags2":0, "Pips":[4,4,4], "FireGroup":0, "GuiFocus":0, "Cargo":0.0, "LegalState":"Clean", "Balance":1 }""";

    private static (ReplayHost host, string state) chargeFsd(string name, string fid, string navRouteJson)
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", fid)).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        var host = new ReplayHost(name, "Journal.2026-08-19T100112.01.log");
        host.WriteGameFile("NavRoute.json", navRouteJson);
        host.WriteStatus(statusFlying);
        host.Boot(lines.Take(bootEnd), "E2E TESTER");

        host.WriteStatus(statusCharging);
        // the plotter is created from the Status.json change: wait for the attempt, then let the UI thread settle
        host.WaitForLog("Overlays.add: PlotJumpInfo");
        Thread.Sleep(500);
        var state = host.OnUi(() => $"fsdChargingJump={host.game!.status.FsdChargingJump} mode={host.game.mode} routeIsNull={host.game.navRoute.Route == null}");
        return (host, state);
    }

    /// <summary> #985: NavRoute.json without a "Route" array left NavRouteFile.Route null => ArgumentNullException 'source' </summary>
    [Fact]
    public void Issue985_NavRouteFileWithoutRoute_ChargingFsdShowsJumpInfo()
    {
        var (host, state) = chargeFsd("issue985-navroute-no-route", "F0000985", """{ "timestamp":"2026-08-19T07:02:00Z", "event":"NavRouteClear" }""");
        using (host)
        {
            // render a frame the way the overlay does, so doRender runs over the odd route data too
            var shown = host.OnUi(() => SrvSurvey.plotters.PlotBase2.getPlotter<SrvSurvey.plotters.PlotJumpInfo>()?.render() != null);
            var errors = host.AppErrors;
            var artifact = host.WriteArtifact($"errors={errors.Count} jumpInfoShown={shown} {state}");

            Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
            Assert.True(shown, $"PlotJumpInfo never created/rendered ({state}), see {artifact}");
        }
    }

    /// <summary> #986: a followed route whose hops have no id64 (allowed by the saved format) => "Nullable object must have a value" </summary>
    [Fact]
    public void Issue986_FollowedRouteHopsWithoutId64_ChargingFsdShowsJumpInfo()
    {
        ReplayHost.WriteDataFile(Path.Combine("routes", "F0000986.json"), """
            { "active":true, "autoCopy":false, "last":-1, "hops":[
              { "name":"Blau Euq MX-R d5-4", "x":-5334.875, "y":1042.5, "z":4548.21875 },
              { "name":"Blau Euq QD-Q d6-0" },
              { "name":"Blau Euq QD-Q d6-1", "id64":1234567 } ] }
            """);
        var (host, state) = chargeFsd("issue986-route-no-id64", "F0000986", """{ "timestamp":"2026-08-19T07:02:00Z", "event":"NavRouteClear", "Route":[ ] }""");
        using (host)
        {
            // render a frame the way the overlay does, so doRender runs over the odd route data too
            var shown = host.OnUi(() => SrvSurvey.plotters.PlotBase2.getPlotter<SrvSurvey.plotters.PlotJumpInfo>()?.render() != null);
            var routeLoaded = host.OnUi(() => host.game!.cmdr.route.hops.Count);
            var errors = host.AppErrors;
            var artifact = host.WriteArtifact($"errors={errors.Count} jumpInfoShown={shown} followedHops={routeLoaded} {state}");

            Assert.Equal(3, routeLoaded);
            Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
            Assert.True(shown, $"PlotJumpInfo never created/rendered ({state}), see {artifact}");
        }
    }
}
