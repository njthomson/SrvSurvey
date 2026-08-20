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
    public void UploadIsOptInAndDeveloperModeDefaultsOn()
    {
        var settings = new Settings();
        var loadedExistingSettings = new JObject().ToObject<Settings>();
        var manuallyDisabled = JObject.Parse("{ 'inaraDeveloperTestMode': false }").ToObject<Settings>();

        Assert.False(settings.inaraUpload);
        Assert.True(settings.inaraDeveloperTestMode);
        Assert.False(loadedExistingSettings!.inaraUpload);
        Assert.True(loadedExistingSettings.inaraDeveloperTestMode);
        Assert.False(manuallyDisabled!.inaraDeveloperTestMode);
        var serialized = JObject.FromObject(manuallyDisabled);
        Assert.NotNull(serialized["inaraDeveloperTestMode"]);
        Assert.False(serialized.Value<bool>("inaraDeveloperTestMode"));
    }

    [Fact]
    public void PersonalKeyIsStoredPerCommanderRatherThanGlobally()
    {
        Assert.Null(typeof(Settings).GetField("inaraApiKey"));
        Assert.NotNull(typeof(SrvSurvey.game.CommanderSettings).GetField("inaraApiKey"));
        Assert.NotNull(typeof(SrvSurvey.game.CommanderSettings).GetField("inaraCommanderName"));
    }

    [Theory]
    [InlineData(null, "Journal Commander", "Journal Commander")]
    [InlineData("", "Journal Commander", "Journal Commander")]
    [InlineData("  Corrected Commander  ", "Journal Commander", "Corrected Commander")]
    public void CommanderNameCanBeOverriddenPerCommander(string? configuredName, string journalName, string expected)
    {
        Assert.Equal(expected, Inara.resolveCommanderName(configuredName, journalName));
    }

    [Fact]
    public void PayloadUsesOnlyTheCommandersPersonalKey()
    {
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var events = new[] { new InaraEvent("getCommanderProfile", "2026-07-28T12:00:00Z", new JObject()) };

        var payload = InaraPayloadBuilder.Build("2.0.95.0", credentials, events, false);
        var header = Assert.IsType<JObject>(payload["header"]);

        Assert.Equal("SrvSurvey", header.Value<string>("appName"));
        Assert.Equal("personal-key", header.Value<string>("APIkey"));
        Assert.Equal("Test Commander", header.Value<string>("commanderName"));
        Assert.Equal("F123456", header.Value<string>("commanderFrontierID"));
        Assert.False(header.Value<bool>("isBeingDeveloped"));
        Assert.Null(header["applicationKey"]);
        Assert.Null(header["applicationAccessToken"]);
    }

    [Fact]
    public void DeveloperTestModeIsSentOnlyWhenEnabled()
    {
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var events = new[] { new InaraEvent("getCommanderProfile", "2026-07-28T12:00:00Z", new JObject()) };

        var payload = InaraPayloadBuilder.Build("2.0.95.0", credentials, events, true);
        var header = Assert.IsType<JObject>(payload["header"]);

        Assert.True(header.Value<bool>("isBeingDeveloped"));
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
    [InlineData(true, "key", true, false, true)]
    [InlineData(false, "key", true, false, false)]
    [InlineData(true, null, true, false, false)]
    [InlineData(true, "key", false, false, false)]
    [InlineData(true, "key", true, true, false)]
    public void UploadPreparationSkipsInactiveIntegrations(
        bool optedIn,
        string? apiKey,
        bool live,
        bool beta,
        bool expected)
    {
        Assert.Equal(expected, Inara.CanPrepareUpload(optedIn, apiKey, live, beta));
    }

    [Fact]
    public void OptionalIntegrationFailuresAreContained()
    {
        var reported = false;

        var succeeded = Inara.RunIsolated(
            () => throw new InvalidOperationException("test failure"),
            _ =>
            {
                reported = true;
                throw new Exception("diagnostic failure");
            });

        Assert.False(succeeded);
        Assert.True(reported);
    }

    [Fact]
    public void KnownMultiboxStateAvoidsAnotherProcessEnumeration()
    {
        var enumerated = false;

        var multiboxing = Inara.DetectMultiboxing(true, () =>
        {
            enumerated = true;
            return 1;
        });

        Assert.True(multiboxing);
        Assert.False(enumerated);
    }

    [Fact]
    public void FailedProcessDetectionUsesConservativeSuppression()
    {
        var multiboxing = Inara.DetectMultiboxing(
            false,
            () => throw new InvalidOperationException("process query failed"));

        Assert.True(multiboxing);
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
    public void UnknownTaxiStateDoesNotClaimTheCommandersShipWhileMultiboxing()
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

        var mapped = mapper.Process(journal, context with { IsTaxi = null }, true);
        var jump = Assert.Single(mapped, item => item.Name == "addCommanderTravelFSDJump");

        Assert.Null(jump.Data["isTaxiShuttle"]);
        Assert.Null(jump.Data["shipGameID"]);
        Assert.Null(jump.Data["shipType"]);
    }

    [Fact]
    public void JournalTaxiFlagRemainsAuthoritativeWhileSharedStatusIsSuppressed()
    {
        var mapper = new InaraEventMapper();
        var journal = JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "FSDJump",
              "StarSystem": "Alpha Centauri",
              "StarPos": [3.03125, -0.09375, 3.15625],
              "JumpDist": 4.37,
              "Taxi": true
            }
            """);

        var mapped = mapper.Process(journal, context with { IsTaxi = null }, true);
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
    public void IncompleteSidecarEventsDoNotResetInventoriesWhileMultiboxing()
    {
        var mapper = new InaraEventMapper();
        mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "Cargo",
              "Vessel": "Ship",
              "Inventory": [{ "Name": "tea", "Count": 2 }]
            }
            """), context, true);

        var cargo = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:01:00Z", "event": "Cargo", "Vessel": "Ship", "Count": 9 }
            """), context, true);
        var locker = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:02:00Z", "event": "ShipLocker" }
            """), context, true);

        Assert.DoesNotContain(cargo, item => item.Name == "setCommanderInventoryCargo");
        Assert.DoesNotContain(locker, item => item.Name is "resetCommanderInventory" or "setCommanderInventory");
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

    [Fact]
    public void CreditsLoanAndAssetsComeFromJournalSnapshots()
    {
        var mapper = new InaraEventMapper();
        mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:00Z",
              "event": "LoadGame",
              "Credits": 1250000,
              "Loan": 25000
            }
            """), context, false);

        var mapped = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:05Z",
              "event": "Statistics",
              "Bank_Account": { "Current_Wealth": 8400000 },
              "Combat": { "Bounties_Claimed": 12 }
            }
            """), context, true);
        var credits = Assert.Single(mapped, item => item.Name == "setCommanderCredits");

        Assert.Equal(1250000, credits.Data.Value<long>("commanderCredits"));
        Assert.Equal(25000, credits.Data.Value<long>("commanderLoan"));
        Assert.Equal(8400000, credits.Data.Value<long>("commanderAssets"));
        Assert.Equal("credits", credits.ReplaceKey);

        var repeatedStatistics = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:00:10Z",
              "event": "Statistics",
              "Bank_Account": { "Current_Wealth": 8400000 }
            }
            """), context, true);
        Assert.DoesNotContain(repeatedStatistics, item => item.Name == "setCommanderCredits");

        mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:10:00Z", "event": "MarketSell", "TotalSale": 250 }
            """), context, true);
        var hourly = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T13:00:05Z", "event": "Music", "MusicTrack": "Exploration" }
            """), context, true);
        var hourlyCredits = Assert.Single(hourly, item => item.Name == "setCommanderCredits");
        Assert.Equal(1250250, hourlyCredits.Data.Value<long>("commanderCredits"));
        Assert.Null(hourlyCredits.Data["commanderAssets"]);
    }

    [Fact]
    public void StartupReplaySeedsCurrentStateWithoutUploadingJournalHistory()
    {
        using var reader = new StringReader("""
            { "timestamp": "2026-07-28T11:00:00Z", "event": "LoadGame", "Commander": "Old Commander", "Credits": 10 }
            { "timestamp": "2026-07-28T11:05:00Z", "event": "FSDJump", "StarSystem": "Old System", "StarPos": [0, 0, 0] }
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Commander": "Test Commander", "Credits": 1000, "Loan": 25 }
            { "timestamp": "2026-07-28T12:00:01Z", "event": "Cargo", "Vessel": "Ship", "Inventory": [{ "Name": "tea", "Count": 2 }] }
            { "timestamp": "2026-07-28T12:00:02Z", "event": "Materials", "Raw": [{ "Name": "iron", "Count": 3 }], "Manufactured": [], "Encoded": [] }
            { "timestamp": "2026-07-28T12:10:00Z", "event": "MarketBuy", "Type": "tea", "Count": 1, "TotalCost": 100 }
            { "timestamp": "2026-07-28T12:11:00Z", "event": "Docked", "StationName": "Galileo", "StarSystem": "Sol" }
            """);
        var entries = Inara.ReadCurrentSession(reader);
        var mapper = new InaraEventMapper();

        Assert.Equal(5, entries.Count);
        Assert.Equal("Test Commander", entries[0].Value<string>("Commander"));

        var seededCount = Inara.SeedState(
            mapper,
            entries,
            context,
            JArray.Parse("""[{ "Name": "tea", "Count": 7 }]"""));
        var firstLiveEvents = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:12:00Z", "event": "Music", "MusicTrack": "Exploration" }
            """), context, true);

        Assert.Equal(5, seededCount);
        Assert.DoesNotContain(firstLiveEvents, item => item.Name.StartsWith("addCommanderTravel"));

        var credits = Assert.Single(firstLiveEvents, item => item.Name == "setCommanderCredits");
        Assert.Equal(900, credits.Data.Value<long>("commanderCredits"));
        Assert.Equal(25, credits.Data.Value<long>("commanderLoan"));

        var cargo = Assert.IsType<JArray>(Assert.Single(
            firstLiveEvents,
            item => item.Name == "setCommanderInventoryCargo").Data);
        Assert.Equal(7, Assert.Single(cargo.OfType<JObject>()).Value<int>("itemCount"));

        var materials = Assert.IsType<JArray>(Assert.Single(
            firstLiveEvents,
            item => item.Name == "setCommanderInventoryMaterials").Data);
        Assert.Equal(3, Assert.Single(materials.OfType<JObject>()).Value<int>("itemCount"));
    }

    [Fact]
    public void MultiboxStartupDoesNotTreatSharedCargoAsAuthoritative()
    {
        var entries = new[]
        {
            JObject.Parse("""
                { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Commander": "Test Commander", "Credits": 1000 }
                """),
            JObject.Parse("""
                { "timestamp": "2026-07-28T12:00:01Z", "event": "Cargo", "Vessel": "Ship", "Count": 7 }
                """),
        };
        var mapper = new InaraEventMapper();

        Inara.SeedState(mapper, entries, context with { IsTaxi = null }, null);
        var firstLiveEvents = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:02Z", "event": "Music", "MusicTrack": "Exploration" }
            """), context with { IsTaxi = null }, true);

        Assert.DoesNotContain(firstLiveEvents, item => item.Name == "setCommanderInventoryCargo");
    }

    [Fact]
    public void CreditTransactionsAreCoalescedToTheDocumentedHourlyCadence()
    {
        var mapper = new InaraEventMapper();
        var startup = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Credits": 1000, "Loan": 0 }
            """), context, true);
        Assert.Single(startup, item => item.Name == "setCommanderCredits");

        var purchase = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:10:00Z", "event": "MarketBuy", "Type": "tea", "Count": 1, "TotalCost": 100 }
            """), context, true);
        Assert.DoesNotContain(purchase, item => item.Name == "setCommanderCredits");

        var crewWage = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:30:00Z", "event": "NpcCrewPaidWage", "Amount": 25 }
            """), context, true);
        Assert.DoesNotContain(crewWage, item => item.Name == "setCommanderCredits");

        var voucher = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:59:00Z", "event": "RedeemVoucher", "Amount": 50 }
            """), context, true);
        Assert.DoesNotContain(voucher, item => item.Name == "setCommanderCredits");

        var hourly = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T13:00:00Z", "event": "Music", "MusicTrack": "Exploration" }
            """), context, true);
        var report = Assert.Single(hourly, item => item.Name == "setCommanderCredits");
        Assert.Equal(925, report.Data.Value<long>("commanderCredits"));
    }

    [Fact]
    public void ShutdownFlushesAChangedBalanceBeforeTheHourlyWindow()
    {
        var mapper = new InaraEventMapper();
        mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Credits": 1000, "Loan": 0 }
            """), context, true);

        var sale = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:05:00Z", "event": "MarketSell", "Type": "tea", "Count": 1, "TotalSale": 250 }
            """), context, true);
        Assert.DoesNotContain(sale, item => item.Name == "setCommanderCredits");

        var shutdown = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:06:00Z", "event": "Shutdown" }
            """), context, true);
        var report = Assert.Single(shutdown, item => item.Name == "setCommanderCredits");
        Assert.Equal(1250, report.Data.Value<long>("commanderCredits"));
    }

    [Fact]
    public void MulticrewTransactionsDoNotChangeTheTrackedCommanderBalance()
    {
        var tracker = new InaraCreditTracker();
        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Credits": 1000, "Loan": 0 }
            """), false);
        Assert.NotNull(tracker.CreateReport("2026-07-28T12:00:00Z", true));

        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:05:00Z", "event": "MarketBuy", "TotalCost": 400 }
            """), true);
        Assert.False(tracker.HasUnreportedChanges);
        Assert.Null(tracker.CreateReport("2026-07-28T13:05:00Z", false));
    }

    [Fact]
    public void CarrierBankTransferUsesTheExactJournalBalance()
    {
        var tracker = new InaraCreditTracker();
        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Credits": 1000, "Loan": 0 }
            """), false);
        Assert.NotNull(tracker.CreateReport("2026-07-28T12:00:00Z", true));

        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:10:00Z", "event": "CarrierBankTransfer", "PlayerBalance": 375 }
            """), false);
        var report = tracker.CreateReport("2026-07-28T12:11:00Z", true);

        Assert.NotNull(report);
        Assert.Equal(375, report.Data.Value<long>("commanderCredits"));
    }

    [Fact]
    public void ImpossibleJournalBalanceIsNotUploadedUntilAnExactValueArrives()
    {
        var tracker = new InaraCreditTracker();
        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Credits": 100, "Loan": 0 }
            """), false);
        Assert.NotNull(tracker.CreateReport("2026-07-28T12:00:00Z", true));

        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:05:00Z", "event": "MarketBuy", "TotalCost": 500 }
            """), false);
        Assert.Null(tracker.CreateReport("2026-07-28T13:05:00Z", true));

        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T13:10:00Z", "event": "CarrierBankTransfer", "PlayerBalance": 20 }
            """), false);
        var recovered = tracker.CreateReport("2026-07-28T13:10:00Z", true);
        Assert.NotNull(recovered);
        Assert.Equal(20, recovered.Data.Value<long>("commanderCredits"));
    }
}
