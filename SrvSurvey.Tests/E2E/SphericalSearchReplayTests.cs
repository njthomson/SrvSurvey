using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Cluster: NullReferenceException in PlotSphericalSearch when opening the galaxy map (upstream #994 #988 #1014).
/// A saved boxel search can be "active" while its boxel fails to parse on load (BoxelSearch.ReadJson => boxel = null).
/// </summary>
public sealed class SphericalSearchReplayTests
{
    private const string statusGalMap = """{ "timestamp":"2026-08-19T07:03:21Z", "event":"Status", "Flags":16777224, "Flags2":0, "Pips":[4,4,4], "FireGroup":0, "GuiFocus":6, "Cargo":0.0, "LegalState":"Clean", "Balance":1 }""";
    private const string statusFlying = """{ "timestamp":"2026-08-19T07:03:00Z", "event":"Status", "Flags":16777224, "Flags2":0, "Pips":[4,4,4], "FireGroup":0, "GuiFocus":0, "Cargo":0.0, "LegalState":"Clean", "Balance":1 }""";

    [Fact]
    public void Issue994_ActiveBoxelSearchWithUnparseableBoxel_GalaxyMapAndNavRouteDoNotCrash()
    {
        const string fid = "F0000994";
        // what a commander file looks like when the stored boxel name can no longer be parsed
        ReplayHost.WriteDataFile($"{fid}-live.json", $$"""
            { "commander":"E2E TESTER", "fid":"{{fid}}", "isOdyssey":true,
              "boxelSearch":{ "active":true, "startedOn":"2026-08-01T00:00:00", "boxel":"Nowhere Sector AB-C d", "current":"Nowhere Sector AB-C d", "lowMassCode":"c", "autoCopy":true } }
            """);

        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", fid)).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        using var host = new ReplayHost("issue994-boxel-search", "Journal.2026-08-19T100112.01.log");
        host.WriteStatus(statusFlying);
        host.Boot(lines.Take(bootEnd), "E2E TESTER");
        var loaded = host.OnUi(() => host.game!.cmdr.boxelSearch != null);

        // #994/#988: open the galaxy map => the plotter's deferred clipboard step runs 250ms later
        host.WriteStatus(statusGalMap);
        host.WaitForLog("Mode change Flying => GalaxyMap");
        Thread.Sleep(1_000);

        // #1014: plot a route in the galaxy map => NavRoute journal event, with a 2+ hop NavRoute.json
        host.WriteGameFile("NavRoute.json", """
            { "timestamp":"2026-08-19T07:03:22Z", "event":"NavRoute", "Route":[
              { "StarSystem":"Blau Euq MX-R d5-4", "SystemAddress":146809244467, "StarPos":[-5334.87500,1042.50000,4548.21875], "StarClass":"K" },
              { "StarSystem":"Blau Euq QD-Q d6-0", "SystemAddress":9370291003, "StarPos":[-5300.0,1040.0,4560.0], "StarClass":"N" } ] }
            """);
        host.Append(["""{ "timestamp":"2026-08-19T07:03:22Z", "event":"NavRoute" }"""]);
        Thread.Sleep(500);

        var state = host.OnUi(() => $"boxelSearchActive={host.game!.cmdr.boxelSearch?.active} boxel={host.game.cmdr.boxelSearch?.boxel?.name ?? "null"} mode={host.game.mode}");
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} searchLoaded={loaded} {state}");

        Assert.True(loaded, $"boxel search not loaded from commander file - premise broken, see {artifact}");
        Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
        Assert.Contains("mode=GalaxyMap", state);
        Assert.Contains("boxelSearchActive=False", state);
        Assert.True(host.CountLog("Deactivating boxel search, cannot parse boxel") > 0, $"broken search was not deactivated visibly, see {artifact}");
    }
}
