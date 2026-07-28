using Newtonsoft.Json.Linq;
using SrvSurvey.net;
using Xunit;

namespace SrvSurvey.Tests;

public sealed class InaraTests
{
    private static readonly InaraContext context = new(
        "Test Commander",
        "F123456",
        "Sol",
        "Galileo",
        "Earth",
        "CobraMkIII",
        42,
        "Surveyor",
        "SRV-42",
        false);

    [Fact]
    public void UploadIsOptInByDefault()
    {
        var settings = new Settings();
        var loadedExistingSettings = new JObject().ToObject<Settings>();

        Assert.False(settings.inaraUpload);
        Assert.False(loadedExistingSettings!.inaraUpload);
    }

    [Fact]
    public void PersonalKeyIsStoredPerCommanderRatherThanGlobally()
    {
        Assert.Null(typeof(Settings).GetField("inaraApiKey"));
        Assert.NotNull(typeof(SrvSurvey.game.CommanderSettings).GetField("inaraApiKey"));
    }

    [Fact]
    public void PayloadUsesOnlyTheCommandersPersonalKey()
    {
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var events = new[] { new InaraEvent("getCommanderProfile", "2026-07-28T12:00:00Z", new JObject()) };

        var payload = InaraPayloadBuilder.Build("2.0.95.0", credentials, events);
        var header = Assert.IsType<JObject>(payload["header"]);

        Assert.Equal("SrvSurvey", header.Value<string>("appName"));
        Assert.Equal("personal-key", header.Value<string>("APIkey"));
        Assert.Equal("Test Commander", header.Value<string>("commanderName"));
        Assert.Equal("F123456", header.Value<string>("commanderFrontierID"));
        Assert.True(header.Value<bool>("isBeingDeveloped"));
        Assert.Null(header["applicationKey"]);
        Assert.Null(header["applicationAccessToken"]);
    }

    [Theory]
    [InlineData(true, "key", true, false, false, true)]
    [InlineData(false, "key", true, false, false, false)]
    [InlineData(true, null, true, false, false, false)]
    [InlineData(true, "key", false, false, false, false)]
    [InlineData(true, "key", true, true, false, false)]
    [InlineData(true, "key", true, false, true, false)]
    public void UploadPolicyRequiresExplicitSafeConditions(
        bool optedIn,
        string? apiKey,
        bool live,
        bool beta,
        bool multicrew,
        bool expected)
    {
        Assert.Equal(expected, Inara.CanUpload(optedIn, apiKey, live, beta, multicrew));
    }

    [Theory]
    [InlineData("4.0.0.1900", false, true)]
    [InlineData("4.1.0.100", false, true)]
    [InlineData("3.8.0.0", false, false)]
    [InlineData(null, true, true)]
    [InlineData(null, false, false)]
    public void LiveGalaxyDetectionIncludesHorizonsFour(
        string? gameVersion,
        bool odyssey,
        bool expected)
    {
        Assert.Equal(expected, Inara.IsLiveVersion(gameVersion, odyssey));
    }

    [Theory]
    [InlineData("4.0.0.1900", false)]
    [InlineData("4.0.0.1900 Beta", true)]
    [InlineData("Odyssey Alpha", true)]
    public void BetaBuildsAreDetected(string gameVersion, bool expected)
    {
        Assert.Equal(expected, Inara.IsBetaVersion(gameVersion));
    }

    [Fact]
    public void FsdJumpMapsToInaraTravelEvent()
    {
        var mapper = new InaraEventMapper();
        var journal = JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "FSDJump",
              "StarSystem": "Alpha Centauri",
              "StarPos": [3.03125, -0.09375, 3.15625],
              "JumpDist": 4.37
            }
            """);

        var mapped = mapper.Process(journal, context, true);
        var jump = Assert.Single(mapped, item => item.Name == "addCommanderTravelFSDJump");

        Assert.Equal("Alpha Centauri", jump.Data.Value<string>("starsystemName"));
        Assert.Equal(4.37, jump.Data.Value<double>("jumpDistance"));
        Assert.Equal(42, jump.Data.Value<long>("shipGameID"));
        Assert.Equal("CobraMkIII", jump.Data.Value<string>("shipType"));
    }

    [Fact]
    public void TaxiJumpDoesNotClaimTheCommandersShip()
    {
        var mapper = new InaraEventMapper();
        var journal = JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "FSDJump",
              "StarSystem": "Alpha Centauri",
              "StarPos": [3.03125, -0.09375, 3.15625],
              "JumpDist": 4.37
            }
            """);

        var mapped = mapper.Process(journal, context with { IsTaxi = true }, true);
        var jump = Assert.Single(mapped, item => item.Name == "addCommanderTravelFSDJump");

        Assert.True(jump.Data.Value<bool>("isTaxiShuttle"));
        Assert.Null(jump.Data["shipGameID"]);
        Assert.Null(jump.Data["shipType"]);
    }

    [Fact]
    public void RankAndProgressAreCombinedLikeEdmc()
    {
        var mapper = new InaraEventMapper();
        mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "Rank", "Combat": 5, "Exploration": 4 }
            """), context, false);

        var mapped = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:01Z", "event": "Progress", "Combat": 37, "Exploration": 82 }
            """), context, true);
        var ranks = Assert.IsType<JArray>(Assert.Single(mapped, item => item.Name == "setCommanderRankPilot").Data);

        var combat = Assert.Single(ranks.OfType<JObject>(), item => item.Value<string>("rankName") == "combat");
        Assert.Equal(5, combat.Value<int>("rankValue"));
        Assert.Equal(0.37, combat.Value<double>("rankProgress"), 5);
        Assert.Contains(ranks.OfType<JObject>(), item => item.Value<string>("rankName") == "explore");
    }

    [Fact]
    public void MissionAndCombatEventsAreMapped()
    {
        var mapper = new InaraEventMapper();
        var mission = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "MissionAccepted",
              "Name": "Mission_Delivery",
              "MissionID": 1234,
              "Faction": "Sol Workers' Party",
              "Influence": "+",
              "Reputation": "++",
              "DestinationSystem": "Barnard's Star",
              "Commodity": "$Tea_Name;",
              "Count": 12
            }
            """), context, true);
        var accepted = Assert.Single(mission, item => item.Name == "addCommanderMission");
        Assert.Equal(1234, accepted.Data.Value<long>("missionGameID"));
        Assert.Equal("Sol", accepted.Data.Value<string>("starsystemNameOrigin"));

        var combat = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:01:00Z",
              "event": "PVPKill",
              "Victim": "Test Opponent"
            }
            """), context, true);
        var kill = Assert.Single(combat, item => item.Name == "addCommanderCombatKill");
        Assert.Equal("Test Opponent", kill.Data.Value<string>("opponentName"));
        Assert.Equal("Sol", kill.Data.Value<string>("starsystemName"));
    }

    [Fact]
    public void CargoSnapshotsAreUpdatedAndReplaceOlderQueuedSnapshots()
    {
        var mapper = new InaraEventMapper();
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var queue = new InaraEventQueue();

        var initial = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "Cargo",
              "Vessel": "Ship",
              "Inventory": [{ "Name": "tea", "Count": 2 }]
            }
            """), context, true);
        queue.Enqueue(credentials, initial.Where(item => item.ReplaceKey == "inventory:cargo"));

        var changed = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:01:00Z",
              "event": "CargoTransfer",
              "Transfers": [{ "Type": "tea", "Count": 3, "Direction": "toship" }]
            }
            """), context, true);
        queue.Enqueue(credentials, changed.Where(item => item.ReplaceKey == "inventory:cargo"));

        var queued = Assert.Single(queue.TakeAll());
        var cargo = Assert.IsType<JArray>(queued.Event.Data);
        Assert.Equal(5, Assert.Single(cargo.OfType<JObject>()).Value<int>("itemCount"));
    }

    [Fact]
    public void MulticrewSuppressesUploadsUntilCrewIsLeft()
    {
        var mapper = new InaraEventMapper();
        mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "JoinACrew" }
            """), context, true);

        var suppressed = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:01:00Z", "event": "FSDJump", "StarSystem": "Sirius", "StarPos": [6.25, -1.25, -5.75], "JumpDist": 8.6 }
            """), context, true);
        Assert.True(mapper.InMulticrew);
        Assert.Empty(suppressed);

        mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:02:00Z", "event": "QuitACrew" }
            """), context, true);
        var resumed = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:03:00Z", "event": "FSDJump", "StarSystem": "Sirius", "StarPos": [6.25, -1.25, -5.75], "JumpDist": 8.6 }
            """), context, true);

        Assert.False(mapper.InMulticrew);
        Assert.Contains(resumed, item => item.Name == "addCommanderTravelFSDJump");
    }
}
