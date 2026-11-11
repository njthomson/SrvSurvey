using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Cluster: "Why no this.systemBody?" / "Why no this.systemData?" thrown from Game.onJournalEntry(ScanOrganic).
/// Upstream issues: #1050 #1044 #1030 #1018 #1015 #1012 #1011 #1009 #1007 #993 #935 #902 #1046 #678
/// </summary>
public sealed class ScanOrganicReplayTests
{
    private const long systemAddress = 146809244467;
    private const int bodyId = 12;
    private const string bodyName = "Blau Euq MX-R d5-4 1 g";
    private const string species = "$Codex_Ent_Electricae_01_Name;";

    // Player standing on the body next to the Touchdown point (values from the #1044 journal)
    private static readonly string statusOnFoot = $$"""
        { "timestamp":"2026-08-19T07:09:20Z", "event":"Status", "Flags":2097152, "Flags2":17, "Oxygen":1.0, "Health":1.0, "Temperature":80.0, "SelectedWeapon":"", "Gravity":0.07, "Latitude":-46.624676, "Longitude":66.149605, "Heading":10, "Altitude":0, "BodyName":"{{bodyName}}", "PlanetRadius":1658541.375 }
        """.ReplaceLineEndings(" ");

    private static readonly string statusInShip = """
        { "timestamp":"2026-08-19T07:02:10Z", "event":"Status", "Flags":16777224, "Flags2":0, "Pips":[4,4,4], "FireGroup":0, "GuiFocus":0, "Fuel":{ "FuelMain":118.8, "FuelReservoir":1.1 }, "Cargo":3.0, "LegalState":"Clean", "Balance":1 }
        """.ReplaceLineEndings(" ");

    private static int indexOf(string[] lines, string evt, int from = 0)
        => Array.FindIndex(lines, from, l => l.Contains($"\"event\":\"{evt}\""));

    /// <summary>
    /// #1044 real journal: a 2 second "Music: MainMenu" mid-session (no LoadGame follows) used to wipe systemData,
    /// nothing re-created it, so the later ScanOrganic threw and bio tracking stopped for the rest of the system.
    /// </summary>
    [Fact]
    public void Issue1044_TransientMainMenuMusic_DoesNotLoseSystemBeforeScanOrganic()
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"));
        var bootEnd = indexOf(lines, "Music") + 1; // Fileheader .. Location, Music:NoTrack
        var disembark = indexOf(lines, "Disembark");
        var scanOrganic = indexOf(lines, "ScanOrganic");
        Assert.True(bootEnd > 0 && disembark > bootEnd && scanOrganic == lines.Length - 1, "fixture shape changed");
        Assert.Contains(lines.Take(disembark), l => l.Contains("\"MusicTrack\":\"MainMenu\""));

        using var host = new ReplayHost("issue1044-mainmenu-music", "Journal.2026-08-19T100112.01.log");
        host.WriteStatus(statusInShip);
        host.Boot(lines.Take(bootEnd), "E2E TESTER");

        host.Append(lines[bootEnd..(disembark + 1)]);
        host.WriteStatus(statusOnFoot);
        host.Append(lines[(disembark + 1)..scanOrganic]);
        host.Append(lines[scanOrganic..]);

        var game = host.game!;
        var state = host.OnUi(() => (game.systemData?.address, game.systemBody?.id, game.cmdr.scanOne?.species));
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} systemAddress={state.address} bodyId={state.id} scanOne={state.species}");

        Assert.True(errors.Count == 0, $"app raised {errors.Count} event exception(s), see {artifact}:\r\n{string.Join("\r\n", errors)}");
        Assert.True(host.CountLog("Left MainMenu without a new Location") > 0, $"systemData was not re-initialized by the MainMenu path, see {artifact}");
        Assert.Equal(systemAddress, state.address);
        Assert.Equal(bodyId, state.id);
        Assert.Equal(species, state.species);
    }

    /// <summary>
    /// Last resort in Game.onJournalEntry(ScanOrganic): the body is in no journal event at all (not even Location),
    /// but Status.json says which body we stand on. Used to throw "Why no this.systemBody?" on every scan.
    /// </summary>
    [Fact]
    public void ScanOrganicOnBodyOnlyStatusKnows_TracksTheScan()
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", "F0001050")).ToArray();
        var bootEnd = indexOf(lines, "Music") + 1; // in normal space next to the main star
        var scanOrganic = lines[indexOf(lines, "ScanOrganic")];

        using var host = new ReplayHost("scanorganic-body-from-status", "Journal.2026-08-19T100112.01.log");
        host.WriteStatus(statusInShip);
        host.Boot(lines.Take(bootEnd), "E2E TESTER");
        var knownBefore = host.OnUi(() => host.game!.systemData!.bodies.Any(b => b.id == bodyId));

        host.WriteStatus(statusOnFoot);
        host.Append([scanOrganic]);

        var game = host.game!;
        var state = host.OnUi(() => (game.systemBody?.id, game.systemBody?.name, game.cmdr.scanOne?.species));
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} bodyKnownBefore={knownBefore} bodyId={state.id} body={state.name} scanOne={state.species}");

        Assert.False(knownBefore, $"body {bodyId} already known - premise broken, see {artifact}");
        Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
        Assert.Equal(bodyId, state.id);
        Assert.Equal(bodyName, state.name);
        Assert.Equal(species, state.species);
    }

    /// <summary>
    /// #902/#935/#993: player landed and logged off on machine A, logs in on machine B where the journal starts
    /// with Location on the body surface and has never seen a Scan of that body, then scans an organism.
    /// </summary>
    [Fact]
    public void Issue902_ResumeOnOtherMachineOnBody_ScanOrganicTracksTheBody()
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"));
        // own FID so commander state saved by other tests cannot satisfy the asserts
        var header = lines.Take(indexOf(lines, "LoadGame") + 1).Select(l => l.Replace("F0000001", "F0000902")); // Fileheader, Commander, ..., LoadGame
        var scan = lines[indexOf(lines, "ScanOrganic")];
        string[] resumed =
        [
            $$"""{ "timestamp":"2026-08-19T08:00:10Z", "event":"Location", "DistFromStarLS":646.9, "Docked":false, "Latitude":-46.624676, "Longitude":66.149605, "Taxi":false, "Multicrew":false, "StarSystem":"Blau Euq MX-R d5-4", "SystemAddress":{{systemAddress}}, "StarPos":[-5334.87500,1042.50000,4548.21875], "SystemAllegiance":"", "SystemEconomy":"$economy_None;", "SystemSecondEconomy":"$economy_None;", "SystemGovernment":"$government_None;", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "Population":0, "Body":"{{bodyName}}", "BodyID":{{bodyId}}, "BodyType":"Planet" }""",
            """{ "timestamp":"2026-08-19T08:00:10Z", "event":"Music", "MusicTrack":"NoTrack" }""",
        ];
        string[] live =
        [
            $$"""{ "timestamp":"2026-08-19T08:01:00Z", "event":"Disembark", "SRV":false, "Taxi":false, "Multicrew":false, "ID":33, "StarSystem":"Blau Euq MX-R d5-4", "SystemAddress":{{systemAddress}}, "Body":"{{bodyName}}", "BodyID":{{bodyId}}, "OnStation":false, "OnPlanet":true }""",
            """{ "timestamp":"2026-08-19T08:01:01Z", "event":"Music", "MusicTrack":"OnFoot" }""",
        ];

        using var host = new ReplayHost("issue902-other-machine", "Journal.2026-08-19T110000.01.log");
        host.WriteStatus(statusOnFoot.Replace("\"Flags2\":17", "\"Flags2\":0").Replace("\"Flags\":2097152", "\"Flags\":18874378")); // landed, in ship
        host.Boot(header.Concat(resumed), "E2E TESTER");

        host.Append(live);
        host.WriteStatus(statusOnFoot);
        host.Append([scan.Replace("2026-08-19T07:15:53Z", "2026-08-19T08:02:00Z")]);

        var game = host.game!;
        var state = host.OnUi(() => (game.systemData?.address, game.systemBody?.id, game.cmdr.scanOne?.species));
        var errors = host.AppErrors;
        var artifact = host.WriteArtifact($"errors={errors.Count} systemAddress={state.address} bodyId={state.id} scanOne={state.species}");

        Assert.True(errors.Count == 0, $"app raised {errors.Count} event exception(s), see {artifact}:\r\n{string.Join("\r\n", errors)}");
        Assert.Equal(systemAddress, state.address);
        Assert.Equal(bodyId, state.id);
        Assert.Equal(species, state.species);
    }
}
