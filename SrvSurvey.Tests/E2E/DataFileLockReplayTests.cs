using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Cluster: IOException "The process cannot access the file ... because it is being used by another process"
/// from Data.Load / readToEndShared (upstream #978 #940 #938). Antivirus, cloud sync or SrvSurvey's own
/// tmp-file-then-replace save can hold a data file for a moment; loading it at that moment crashed.
/// </summary>
public sealed class DataFileLockReplayTests
{
    [Fact]
    public void Issue978_SystemFileBrieflyLockedOnJump_LoadsAfterLockReleases()
    {
        const string fid = "F0000978";
        const string system = "Blau Euq QD-Q d6-0";
        const long address = 9370291003;
        var systemFile = Path.Combine(SrvSurvey.Tests.E2E.ReplayIsolation.DataFolder, "systems", fid, $"{system}_{address}.json");
        ReplayHost.WriteDataFile(Path.Combine("systems", fid, $"{system}_{address}.json"), $$"""
            { "name":"{{system}}", "address":{{address}}, "starPos":[-5300.0,1040.0,4560.0], "commander":"E2E TESTER",
              "firstVisited":"2026-08-01T00:00:00Z", "lastVisited":"2026-08-01T00:00:00Z",
              "bodies":[ { "name":"{{system}} A", "id":0, "type":"Star", "shortName":"A" },
                         { "name":"{{system}} 7 e", "id":42, "type":"LandableBody", "shortName":"7e" } ] }
            """);

        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", fid)).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        using var host = new ReplayHost("issue978-locked-data-file", "Journal.2026-08-19T100112.01.log");
        host.Boot(lines.Take(bootEnd), "E2E TESTER");

        // another process holds the file exclusively for 300ms, starting before the jump lands
        var locker = new FileStream(systemFile, FileMode.Open, FileAccess.Read, FileShare.None);
        var release = Task.Delay(300).ContinueWith(_ => locker.Dispose());
        host.Append([
            """{ "timestamp":"2026-08-19T07:20:00Z", "event":"StartJump", "JumpType":"Hyperspace", "Taxi":false, "StarSystem":"Blau Euq QD-Q d6-0", "SystemAddress":9370291003, "StarClass":"N" }""",
            $$"""{ "timestamp":"2026-08-19T07:20:15Z", "event":"FSDJump", "Taxi":false, "Multicrew":false, "StarSystem":"{{system}}", "SystemAddress":{{address}}, "StarPos":[-5300.0,1040.0,4560.0], "SystemAllegiance":"", "SystemEconomy":"$economy_None;", "SystemSecondEconomy":"$economy_None;", "SystemGovernment":"$government_None;", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "Population":0, "Body":"{{system}} A", "BodyID":0, "BodyType":"Star", "JumpDist":38.2, "FuelUsed":4.1, "FuelLevel":110.0 }""",
        ]);
        release.Wait();

        var lockedDuringLoad = host.CountLog("is being used by another process") + host.CountLog("File locked, retrying");
        var state = host.OnUi(() => (host.game!.systemData?.address, host.game.systemData?.bodies.Any(b => b.id == 42)));
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} lockedDuringLoad={lockedDuringLoad} systemAddress={state.address} seededBodyLoaded={state.Item2}");

        Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
        Assert.True(lockedDuringLoad > 0, $"the app never hit the lock - premise broken, see {artifact}");
        Assert.Equal(address, state.address);
        Assert.True(state.Item2 == true, $"jumped-to system was not loaded from its (briefly locked) file, see {artifact}");
    }
}
