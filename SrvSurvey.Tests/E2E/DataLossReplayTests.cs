using SrvSurvey.game;
using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// P3 of the crash scan: player data that used to be silently replaced.
/// </summary>
public sealed class DataLossReplayTests
{
    /// <summary>
    /// A guardian site file that fails to parse (or stays locked) made GuardianSiteData.Load treat the site as new and
    /// Save() an empty site over it - losing the player's survey on the next approach.
    /// </summary>
    [Fact]
    public void UnreadableGuardianSiteFile_IsNotOverwrittenOnApproach()
    {
        const string fid = "F0003001";
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", fid)).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        // a survey file the current code cannot parse (eg: written by a newer/older build, or damaged)
        var siteRelPath = Path.Combine("guardian", fid, "Blau Euq MX-R d5-4 1 g-ruins-1.json");
        const string surveyJson = """{ "name":"$Ancient:#index=1;", "commander":"E2E TESTER", "poiStatus": { "t1": "present" """;
        ReplayHost.WriteDataFile(siteRelPath, surveyJson);
        var sitePath = Path.Combine(ReplayIsolation.DataFolder, siteRelPath);

        using var host = new ReplayHost("p3-guardian-site-no-overwrite", "Journal.2026-08-19T100112.01.log");
        host.Boot(lines.Take(bootEnd), "E2E TESTER");
        host.Append([
            """{ "timestamp":"2026-08-19T07:07:30Z", "event":"ApproachBody", "StarSystem":"Blau Euq MX-R d5-4", "SystemAddress":146809244467, "Body":"Blau Euq MX-R d5-4 1 g", "BodyID":12 }""",
            """{ "timestamp":"2026-08-19T07:08:00Z", "event":"ApproachSettlement", "Name":"$Ancient:#index=1;", "Name_Localised":"Ancient Ruins (1)", "SystemAddress":146809244467, "BodyID":12, "BodyName":"Blau Euq MX-R d5-4 1 g", "Latitude":-46.6, "Longitude":66.1 }""",
        ]);
        Thread.Sleep(300);

        var after = File.ReadAllText(sitePath);
        var parseFailureLogged = host.CountLog("Failed to read data from");
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} parseFailureLogged={parseFailureLogged} fileUnchanged={after == surveyJson} sizeAfter={after.Length}");

        Assert.True(parseFailureLogged > 0, $"the app never tried to load the site file - premise broken, see {artifact}");
        Assert.True(after == surveyJson, $"the unreadable survey file was overwritten, see {artifact}:\r\n{after}");
        Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
    }

    /// <summary>
    /// Same site, but the file is locked (anti-virus, cloud sync) for longer than Data.readToEndShared retries (~750ms):
    /// the load must fail visibly rather than be mistaken for "no survey yet" and overwritten.
    /// </summary>
    [Fact]
    public void LockedGuardianSiteFile_IsNotOverwrittenOnApproach()
    {
        const string fid = "F0003002";
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", fid)).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        var siteRelPath = Path.Combine("guardian", fid, "Blau Euq MX-R d5-4 1 g-ruins-1.json");
        const string surveyJson = "{ \"name\":\"$Ancient:#index=1;\", \"commander\":\"E2E TESTER\" }";
        ReplayHost.WriteDataFile(siteRelPath, surveyJson);
        var sitePath = Path.Combine(ReplayIsolation.DataFolder, siteRelPath);

        using var host = new ReplayHost("p3-guardian-site-locked", "Journal.2026-08-19T100112.01.log");
        host.Boot(lines.Take(bootEnd), "E2E TESTER");
        using (new FileStream(sitePath, FileMode.Open, FileAccess.Read, FileShare.None)) // held for the whole approach
        {
            host.Append([
                """{ "timestamp":"2026-08-19T07:08:00Z", "event":"ApproachSettlement", "Name":"$Ancient:#index=1;", "Name_Localised":"Ancient Ruins (1)", "SystemAddress":146809244467, "BodyID":12, "BodyName":"Blau Euq MX-R d5-4 1 g", "Latitude":-46.6, "Longitude":66.1 }""",
            ]);
            Thread.Sleep(300);
        }
        Thread.Sleep(700); // any (delayed) save retry would land now

        var after = File.ReadAllText(sitePath);
        var lockSeen = host.CountLog("File locked, retrying");
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"lockSeen={lockSeen} errorsShown={errors.Count} fileUnchanged={after == surveyJson}");

        Assert.True(lockSeen > 0, $"the app never hit the lock - premise broken, see {artifact}");
        Assert.True(after == surveyJson, $"the locked survey file was overwritten, see {artifact}: {after}");
        Assert.Contains(errors, e => e.Contains("IOException")); // visible, not silent
    }

    /// <summary> Any data file that fails to parse is kept as .bad, as callers may create and save a fresh object over it (eg: CommanderSettings) </summary>
    [Fact]
    public void UnparseableCommanderFile_IsKeptAsBad()
    {
        const string fid = "F0003003";
        const string broken = "{ \"commander\":\"E2E TESTER\", \"fid\":\"F0003003\", \"countJumps\": 4242, ";
        ReplayHost.WriteDataFile($"{fid}-live.json", broken);
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", fid)).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;

        using var host = new ReplayHost("p3-cmdr-file-kept-as-bad", "Journal.2026-08-19T100112.01.log");
        host.Boot(lines.Take(bootEnd), "E2E TESTER");

        var badPath = Path.Combine(ReplayIsolation.DataFolder, $"{fid}-live.json.bad");
        var kept = File.Exists(badPath) ? File.ReadAllText(badPath) : null;
        var artifact = host.WriteArtifact($"badKept={kept == broken}");

        Assert.True(host.CountLog("Failed to read data from") > 0, $"commander file did not fail to parse - premise broken, see {artifact}");
        Assert.True(kept == broken, $"the unparseable commander file was not kept, see {artifact}");
    }

    /// <summary> SystemData's cache was keyed by system only: a 2nd commander in the same session got the 1st one's data (and saved into it) </summary>
    [Fact]
    public void SystemDataCache_IsPerCommander()
    {
        const string name = "Cache Test AB-C d1-1";
        const long address = 77777;
        foreach (var (fid, body) in new[] { ("F000CA01", "alpha"), ("F000CA02", "beta") })
            ReplayHost.WriteDataFile(Path.Combine("systems", fid, $"{name}_{address}.json"), $$"""
                { "name":"{{name}}", "address":{{address}}, "starPos":[0,0,0], "commander":"{{fid}}", "bodies":[ { "name":"{{name}} {{body}}", "id":1, "shortName":"{{body}}" } ] }
                """);

        using var host = new ReplayHost("p3-systemdata-cache-per-cmdr", "unused.log");
        var (first, second) = host.OnUi(() =>
        {
            var a = SystemData.Load(name, address, "F000CA01", "Cmdr A", true);
            var b = SystemData.Load(name, address, "F000CA02", "Cmdr B", true);
            return (a?.bodies.FirstOrDefault()?.shortName, b?.bodies.FirstOrDefault()?.shortName);
        });
        var artifact = host.WriteArtifact($"cmdrA={first} cmdrB={second}");

        Assert.Equal("alpha", first);
        Assert.True(second == "beta", $"commander B got commander A's system data ({second}), see {artifact}");
    }
}
