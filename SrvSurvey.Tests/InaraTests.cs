using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.forms;
using SrvSurvey.net;
using System.Net;
using System.Text;
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
    public void PersonalKeySerializationIsCommanderScopedAndDevelopmentModeIsHardCoded()
    {
        var settings = new Settings();
        var commanderSettings = commander("Test Commander", "F123456", "personal-key");
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var payload = InaraPayloadBuilder.Build("2.0.95.0", credentials,
            [new InaraEvent("getCommanderProfile", "2026-07-28T12:00:00Z", new JObject())]);

        var globalJson = JObject.FromObject(settings);
        var commanderJson = JObject.FromObject(commanderSettings);
        Assert.DoesNotContain(globalJson.Properties(),
            property => property.Name.StartsWith("inara", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("personal-key", commanderJson.Value<string>("inaraApiKey"));
        Assert.Null(commanderJson["inaraCommanderName"]);
        Assert.True(payload.SelectToken("header.isBeingDeveloped")!.Value<bool>());
    }

    [Fact]
    public void SettingsEditStaysPinnedToTheCapturedCommanderProfile()
    {
        var alpha = commander("Commander Alpha", "F-ALPHA", "alpha-key");
        var beta = commander("Commander Beta", "F-BETA", "beta-key");

        FormInaraIntegration.ApplyApiKey(alpha, " replacement-alpha-key ");

        Assert.Equal("replacement-alpha-key", alpha.inaraApiKey);
        Assert.Equal("beta-key", beta.inaraApiKey);
        Assert.Equal("Commander Alpha", alpha.commander);
        Assert.Equal("F-ALPHA", alpha.fid);

        FormInaraIntegration.ApplyApiKey(alpha, null);
        Assert.Null(alpha.inaraApiKey);
        Assert.Equal("beta-key", beta.inaraApiKey);
    }

    [Fact]
    public void SessionCredentialsStayBoundToTheCapturedCommanderProfile()
    {
        var alpha = new SrvSurvey.game.CommanderSettings
        {
            commander = "Commander Alpha",
            fid = "F-ALPHA",
            inaraApiKey = "alpha-key",
        };
        var beta = new SrvSurvey.game.CommanderSettings
        {
            commander = "Commander Beta",
            fid = "F-BETA",
            inaraApiKey = "beta-key",
        };

        var session = Assert.IsType<InaraSession>(InaraSession.Create(alpha, "4.0.0.1900", true));
        beta.inaraApiKey = "changed-beta-key";

        Assert.Equal(new InaraCredentials("Commander Alpha", "F-ALPHA", "alpha-key"), session.GetCredentials());

        alpha.inaraApiKey = "replacement-alpha-key";
        Assert.Equal(new InaraCredentials("Commander Alpha", "F-ALPHA", "replacement-alpha-key"), session.GetCredentials());
    }

    [Theory]
    [InlineData(null, "F-ALPHA")]
    [InlineData("", "F-ALPHA")]
    [InlineData("Commander Alpha", null)]
    [InlineData("Commander Alpha", "")]
    public void MissingSessionIdentityCannotStartUploader(string? name, string? fid)
    {
        var settings = new CommanderSettings
        {
            commander = name!,
            fid = fid!,
            inaraApiKey = "alpha-key",
        };

        Assert.Null(InaraSession.Create(settings, "4.0.0.1900", true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingGameVersionCannotStartUploader(string? gameVersion)
    {
        var settings = commander("Commander Alpha", "F-ALPHA", "alpha-key");

        Assert.Null(InaraSession.Create(settings, gameVersion, true));
    }

    [Fact]
    public void KeyReplacementDiscardsEventsQueuedUnderTheOldKey()
    {
        var settings = new CommanderSettings
        {
            commander = "Commander Alpha",
            fid = "F-ALPHA",
            inaraApiKey = "alpha-key-1",
        };
        var session = Assert.IsType<InaraSession>(InaraSession.Create(settings, "4.0.0.1900", true));
        var queue = new InaraEventQueue();
        queue.Enqueue(session.GetCredentials()!.ApiKey,
            [new InaraEvent("addCommanderTravelFSDJump", "2026-07-28T12:00:00Z", new JObject())]);

        settings.inaraApiKey = "alpha-key-2";
        var pending = queue.TakeFor(session.GetCredentials()?.ApiKey, out var discarded);

        Assert.Empty(pending);
        Assert.Equal(1, discarded);
    }

    [Fact]
    public void ClearingAKeyDiscardsAllPendingEventsForThatSession()
    {
        var settings = new CommanderSettings
        {
            commander = "Commander Alpha",
            fid = "F-ALPHA",
            inaraApiKey = "alpha-key",
        };
        var session = Assert.IsType<InaraSession>(InaraSession.Create(settings, "4.0.0.1900", true));
        var queue = new InaraEventQueue();
        queue.Enqueue(session.GetCredentials()!.ApiKey,
            [new InaraEvent("setCommanderCredits", "2026-07-28T12:00:00Z", new JObject())]);

        settings.inaraApiKey = null;
        var pending = queue.TakeFor(session.GetCredentials()?.ApiKey, out var discarded);

        Assert.Empty(pending);
        Assert.Equal(1, discarded);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task SequentialSessionInstancesNeverCrossCommanderPayloads()
    {
        var alphaHandler = new RecordingHandler((request, _) => successfulResponse(request));
        var betaHandler = new RecordingHandler((request, _) => successfulResponse(request));
        var alphaSettings = commander("Commander Alpha", "F-ALPHA", "alpha-key");
        var betaSettings = commander("Commander Beta", "F-BETA", "beta-key");

        using (var alpha = Inara.CreateForTests(
            InaraSession.Create(alphaSettings, "4.0.0.1900", true)!, alphaHandler, context with
            {
                Commander = "Commander Alpha",
                FrontierId = "F-ALPHA",
            }))
        {
            alpha.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));
            await alpha.flushAsync();
        }

        using (var beta = Inara.CreateForTests(
            InaraSession.Create(betaSettings, "4.0.0.1900", true)!, betaHandler, context with
            {
                Commander = "Commander Beta",
                FrontierId = "F-BETA",
            }))
        {
            beta.onJournalEntry(loadGame("Commander Beta", "F-BETA", 2000));
            await beta.flushAsync();
        }

        var alphaPayload = Assert.Single(alphaHandler.Bodies);
        var betaPayload = Assert.Single(betaHandler.Bodies);
        assertPayloadIdentity(alphaPayload, "Commander Alpha", "F-ALPHA", "alpha-key");
        assertPayloadIdentity(betaPayload, "Commander Beta", "F-BETA", "beta-key");
        var serializedBeta = betaPayload.ToString(Formatting.None);
        Assert.DoesNotContain("Commander Alpha", serializedBeta, StringComparison.Ordinal);
        Assert.DoesNotContain("alpha-key", serializedBeta, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddingAKeyMidSessionUploadsOnlyTheCapturedCommanderWithWarmedState()
    {
        var handler = new RecordingHandler((request, _) => successfulResponse(request));
        var settings = commander("Commander Alpha", "F-ALPHA", "");
        using var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context with
            {
                Commander = "Commander Alpha",
                FrontierId = "F-ALPHA",
            });

        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1234));
        Assert.Empty(handler.Bodies);

        settings.inaraApiKey = "new-alpha-key";
        inara.onJournalEntry(new JObject
        {
            ["timestamp"] = "2026-07-28T12:01:00Z",
            ["event"] = "Music",
            ["MusicTrack"] = "MainMenu",
        });
        await inara.flushAsync();

        var payload = Assert.Single(handler.Bodies);
        assertPayloadIdentity(payload, "Commander Alpha", "F-ALPHA", "new-alpha-key");
        Assert.Contains(Assert.IsType<JArray>(payload["events"]),
            entry => entry?["eventName"]?.Value<string>() == "setCommanderCredits"
                && entry.SelectToken("eventData.commanderCredits")?.Value<long>() == 1234);
    }

    [Fact]
    public async Task ReplacingAKeyNeverSendsOldQueuedEventsUnderTheNewKey()
    {
        var handler = new RecordingHandler((request, _) => successfulResponse(request));
        var settings = commander("Commander Alpha", "F-ALPHA", "alpha-key-1");
        using var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));

        settings.inaraApiKey = "alpha-key-2";
        inara.onApiKeyChanged();
        inara.onJournalEntry(new JObject
        {
            ["timestamp"] = "2026-07-28T12:02:00Z",
            ["event"] = "FSDJump",
            ["StarSystem"] = "Achenar",
            ["StarPos"] = new JArray(1, 2, 3),
            ["JumpDist"] = 8.5,
        });
        await inara.flushAsync();

        var payload = Assert.Single(handler.Bodies);
        assertPayloadIdentity(payload, "Commander Alpha", "F-ALPHA", "alpha-key-2");
        var serialized = payload.ToString(Formatting.None);
        Assert.DoesNotContain("alpha-key-1", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("setCommanderCredits", serialized, StringComparison.Ordinal);
        Assert.Contains("addCommanderTravelFSDJump", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ClearingAKeyDiscardsPendingEventsAndSkipsFinalUpload()
    {
        var handler = new RecordingHandler((request, _) => successfulResponse(request));
        var settings = commander("Commander Alpha", "F-ALPHA", "alpha-key");
        using var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));

        settings.inaraApiKey = null;
        inara.onApiKeyChanged();
        await inara.StopAsync();

        Assert.Empty(handler.Bodies);
    }

    [Fact]
    public async Task NormalStopFlushesOneFinalCreditReportBeforeDisposingTransport()
    {
        var handler = new RecordingHandler((request, _) => successfulResponse(request));
        var settings = commander("Commander Alpha", "F-ALPHA", "alpha-key");
        var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));
        await inara.flushAsync();
        inara.onJournalEntry(new JObject
        {
            ["timestamp"] = "2026-07-28T12:03:00Z",
            ["event"] = "BuyAmmo",
            ["Cost"] = 50,
        });

        var stopping = inara.StopAsync();
        await stopping;
        await inara.StopAsync();

        Assert.Equal(2, handler.Bodies.Count);
        var finalEvents = Assert.IsType<JArray>(handler.Bodies[1]["events"]);
        var credits = Assert.Single(finalEvents,
            entry => entry?["eventName"]?.Value<string>() == "setCommanderCredits");
        Assert.Equal(950, credits.SelectToken("eventData.commanderCredits")!.Value<long>());
        Assert.True(handler.IsDisposed);
    }

    [Fact]
    public async Task StopWaitsForAnActiveSendAndDoesNotDuplicateFinalData()
    {
        var requestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new RecordingHandler(async (request, _) =>
        {
            requestStarted.TrySetResult();
            await releaseRequest.Task;
            return await successfulResponse(request);
        });
        var settings = commander("Commander Alpha", "F-ALPHA", "alpha-key");
        var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));

        var activeSend = inara.flushAsync();
        await requestStarted.Task;
        inara.onJournalEntry(new JObject
        {
            ["timestamp"] = "2026-07-28T12:04:00Z",
            ["event"] = "Shutdown",
        });
        var stopping = inara.StopAsync();
        var stoppingAgain = inara.StopAsync();

        Assert.False(stopping.IsCompleted);
        Assert.Same(stopping, stoppingAgain);
        releaseRequest.TrySetResult();
        await Task.WhenAll(activeSend, stopping);

        Assert.Single(handler.Bodies);
        await inara.StopAsync();
        Assert.Single(handler.Bodies);
    }

    [Fact]
    public async Task EmptyAndMalformedResponsesRetainTheBatchForRetry()
    {
        var handler = new RecordingHandler((request, requestNumber) => requestNumber switch
        {
            1 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) }),
            2 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ 'header': { 'eventStatus': 200 } }") }),
            3 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ 'header': { 'eventStatus': 200 }, 'events': [ 'not-an-event-result' ] }")
            }),
            _ => successfulResponse(request),
        });
        var settings = commander("Commander Alpha", "F-ALPHA", "alpha-key");
        using var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));

        await inara.flushAsync();
        await inara.flushAsync();
        await inara.flushAsync();
        await inara.flushAsync();

        Assert.Equal(4, handler.Bodies.Count);
        Assert.Equal(
            handler.Bodies[0].SelectToken("events")!.ToString(),
            handler.Bodies[3].SelectToken("events")!.ToString());
    }

    [Fact]
    public async Task UploadDiagnosticsIncludeSafeStatusWithoutLeakingCredentials()
    {
        var logs = new List<string>();
        var handler = new RecordingHandler((request, requestNumber) => requestNumber == 1
            ? Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                ReasonPhrase = "Back off now",
            })
            : successfulResponse(request));
        var settings = commander("Commander Alpha", "F-ALPHA", "secret-alpha-key");
        using var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context, logs.Add);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));

        await inara.flushAsync();
        await inara.flushAsync();

        var log = Assert.Single(logs, entry => entry.Contains("HTTP 429", StringComparison.Ordinal));
        Assert.Contains("Back off now", log, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-alpha-key", string.Join(Environment.NewLine, logs), StringComparison.Ordinal);
        Assert.Equal(2, handler.Bodies.Count);
    }

    [Fact]
    public async Task RejectedEventDiagnosticsAreNamedSanitizedAndTruncated()
    {
        var logs = new List<string>();
        var unsafeText = "rejected\r\n" + new string('x', 400);
        var handler = new RecordingHandler(async (request, _) =>
        {
            var requestBody = JObject.Parse(await request.Content!.ReadAsStringAsync());
            var requestEvents = Assert.IsType<JArray>(requestBody["events"]);
            var responseEvents = new JArray(requestEvents.Select(_ =>
                new JObject { ["eventStatus"] = 500, ["eventStatusText"] = unsafeText }));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(new JObject
                {
                    ["header"] = new JObject { ["eventStatus"] = 200 },
                    ["events"] = responseEvents,
                }.ToString(), Encoding.UTF8, "application/json"),
            };
        });
        var settings = commander("Commander Alpha", "F-ALPHA", "secret-alpha-key");
        using var inara = Inara.CreateForTests(
            InaraSession.Create(settings, "4.0.0.1900", true)!, handler, context, logs.Add);
        inara.onJournalEntry(loadGame("Commander Alpha", "F-ALPHA", 1000));
        for (var index = 0; index < 12; index++)
        {
            inara.onJournalEntry(new JObject
            {
                ["timestamp"] = $"2026-07-28T12:{index + 1:00}:00Z",
                ["event"] = "FSDJump",
                ["StarSystem"] = $"Test System {index}",
                ["StarPos"] = new JArray(index, 0, 0),
                ["JumpDist"] = 5.0,
            });
        }

        await inara.flushAsync();

        var rejection = Assert.Single(logs,
            entry => entry.Contains("Inara rejected ", StringComparison.Ordinal));
        Assert.Contains("getCommanderProfile", rejection, StringComparison.Ordinal);
        Assert.Contains(", and ", rejection, StringComparison.Ordinal);
        Assert.Contains("rejected  ", rejection, StringComparison.Ordinal);
        Assert.DoesNotContain('\r', rejection);
        Assert.DoesNotContain('\n', rejection);
        Assert.DoesNotContain(new string('x', 301), rejection, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-alpha-key", string.Join(Environment.NewLine, logs), StringComparison.Ordinal);
    }

    private static CommanderSettings commander(string name, string fid, string key) => new()
    {
        commander = name,
        fid = fid,
        inaraApiKey = key,
    };

    private static JObject loadGame(string name, string fid, long credits) => new()
    {
        ["timestamp"] = "2026-07-28T12:00:00Z",
        ["event"] = "LoadGame",
        ["Commander"] = name,
        ["FID"] = fid,
        ["Credits"] = credits,
        ["Loan"] = 0,
    };

    private static async Task<HttpResponseMessage> successfulResponse(HttpRequestMessage request)
    {
        var requestBody = JObject.Parse(await request.Content!.ReadAsStringAsync());
        var eventCount = Assert.IsType<JArray>(requestBody["events"]).Count;
        var response = new JObject
        {
            ["header"] = new JObject { ["eventStatus"] = 200 },
            ["events"] = new JArray(Enumerable.Range(0, eventCount)
                .Select(_ => new JObject { ["eventStatus"] = 200 })),
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(response.ToString(), Encoding.UTF8, "application/json"),
        };
    }

    private static void assertPayloadIdentity(JObject payload, string name, string fid, string key)
    {
        Assert.Equal(name, payload.SelectToken("header.commanderName")!.Value<string>());
        Assert.Equal(fid, payload.SelectToken("header.commanderFrontierID")!.Value<string>());
        Assert.Equal(key, payload.SelectToken("header.APIkey")!.Value<string>());
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, int, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public List<JObject> Bodies { get; } = [];
        public bool IsDisposed { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = JObject.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Bodies.Add(body);
            return await respond(request, Bodies.Count);
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
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

    [Fact]
    public void DiagnosticRepresentationsDoNotExposeThePersonalKey()
    {
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var queued = new InaraQueuedEvent(credentials.ApiKey,
            new InaraEvent("getCommanderProfile", "2026-07-28T12:00:00Z", new JObject()));

        Assert.DoesNotContain("personal-key", credentials.ToString());
        Assert.DoesNotContain("personal-key", queued.ToString());
        Assert.Contains("Test Commander", credentials.ToString());
        Assert.Contains("F123456", credentials.ToString());
    }

    [Theory]
    [InlineData("key", true, false, false, true)]
    [InlineData(null, true, false, false, false)]
    [InlineData("key", false, false, false, false)]
    [InlineData("key", true, true, false, false)]
    [InlineData("key", true, false, true, false)]
    public void UploadPolicyRequiresAKeyAndSafeSessionConditions(
        string? apiKey,
        bool live,
        bool beta,
        bool multicrew,
        bool expected)
    {
        Assert.Equal(expected, Inara.CanUpload(apiKey, live, beta, multicrew));
    }

    [Theory]
    [InlineData("key", true, false, true)]
    [InlineData(null, true, false, false)]
    [InlineData("key", false, false, false)]
    [InlineData("key", true, true, false)]
    public void UploadPreparationSkipsInactiveIntegrations(
        string? apiKey,
        bool live,
        bool beta,
        bool expected)
    {
        Assert.Equal(expected, Inara.CanPrepareUpload(apiKey, live, beta));
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
    public void RankProgressAndPromotionSkipNonIntegerProperties()
    {
        var mapper = new InaraEventMapper();
        mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "Rank", "Combat": 5 }
            """), context, false);

        var progressEvents = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:01Z", "event": "Progress", "Combat": 37, "Exploration": "invalid", "CQC": 12.5 }
            """), context, true);
        var progress = Assert.IsType<JArray>(Assert.Single(
            progressEvents, item => item.Name == "setCommanderRankPilot").Data);
        Assert.Equal("combat", Assert.Single(progress.OfType<JObject>()).Value<string>("rankName"));

        var promotionEvents = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:02Z", "event": "Promotion", "Combat": 6, "Exploration": "invalid", "CQC": 7.5 }
            """), context, true);
        var promotion = Assert.Single(promotionEvents, item => item.Name == "setCommanderRankPilot");
        Assert.Equal("combat", promotion.Data.Value<string>("rankName"));
        Assert.Equal(6, promotion.Data.Value<int>("rankValue"));
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
        queue.Enqueue(credentials.ApiKey, initial.Where(item => item.ReplaceKey == "inventory:cargo"));

        var changed = mapper.Process(JObject.Parse("""
            {
              "timestamp": "2026-07-28T12:01:00Z",
              "event": "CargoTransfer",
              "Transfers": [{ "Type": "tea", "Count": 3, "Direction": "toship" }]
            }
            """), context, true);
        queue.Enqueue(credentials.ApiKey, changed.Where(item => item.ReplaceKey == "inventory:cargo"));

        var queued = Assert.Single(queue.TakeAll());
        var cargo = Assert.IsType<JArray>(queued.Event.Data);
        Assert.Equal(5, Assert.Single(cargo.OfType<JObject>()).Value<int>("itemCount"));
    }

    [Fact]
    public void PendingQueueDropsOldestEventsAboveItsCapacity()
    {
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var queue = new InaraEventQueue();
        var events = Enumerable.Range(0, InaraEventQueue.MaxPendingEvents + 5)
            .Select(index => new InaraEvent($"event-{index}", "2026-07-28T12:00:00Z", new JObject()));

        queue.Enqueue(credentials.ApiKey, events);
        var pending = queue.TakeAll();

        Assert.Equal(InaraEventQueue.MaxPendingEvents, pending.Count);
        Assert.Equal("event-5", pending[0].Event.Name);
        Assert.Equal($"event-{InaraEventQueue.MaxPendingEvents + 4}", pending[^1].Event.Name);
    }

    [Fact]
    public void RequeueRetainsDeduplicationAndDropsTheOldestEventsAboveCapacity()
    {
        var credentials = new InaraCredentials("Test Commander", "F123456", "personal-key");
        var queue = new InaraEventQueue();
        queue.Enqueue(credentials.ApiKey, Enumerable.Range(0, InaraEventQueue.MaxPendingEvents - 5)
            .Select(index => new InaraEvent($"new-{index}", "2026-07-28T12:00:00Z", new JObject(),
                index == 0 ? "dedupe" : null)));
        var retained = new[]
            {
                new InaraQueuedEvent(credentials.ApiKey,
                    new InaraEvent("old-duplicate", "2026-07-28T11:00:00Z", new JObject(), "dedupe")),
            }
            .Concat(Enumerable.Range(0, 10)
            .Select(index => new InaraQueuedEvent(credentials.ApiKey,
                new InaraEvent($"old-{index}", "2026-07-28T11:00:00Z", new JObject()))));

        queue.Requeue(retained);
        var pending = queue.TakeAll();

        Assert.Equal(InaraEventQueue.MaxPendingEvents, pending.Count);
        Assert.Equal("old-5", pending[0].Event.Name);
        Assert.Equal($"new-{InaraEventQueue.MaxPendingEvents - 6}", pending[^1].Event.Name);
        Assert.DoesNotContain(pending, item => item.Event.Name == "old-duplicate");
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
            { "timestamp": "2026-07-28T11:59:59Z", "event": "Fileheader", "gameversion": "4.0.0.1900", "build": "r123" }
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Commander": "Test Commander", "Credits": 1000, "Loan": 25 }
            { "timestamp": "2026-07-28T12:00:01Z", "event": "Cargo", "Vessel": "Ship", "Inventory": [{ "Name": "tea", "Count": 2 }] }
            { "timestamp": "2026-07-28T12:00:02Z", "event": "Materials", "Raw": [{ "Name": "iron", "Count": 3 }], "Manufactured": [], "Encoded": [] }
            { "timestamp": "2026-07-28T12:10:00Z", "event": "MarketBuy", "Type": "tea", "Count": 1, "TotalCost": 100 }
            { "timestamp": "2026-07-28T12:11:00Z", "event": "Docked", "StationName": "Galileo", "StarSystem": "Sol" }
            """);
        var entries = Inara.ReadCurrentSession(reader);
        var mapper = new InaraEventMapper();

        Assert.Equal(6, entries.Count);
        Assert.Equal("Fileheader", entries[0].Value<string>("event"));
        Assert.Equal("Test Commander", entries[1].Value<string>("Commander"));

        var seededCount = Inara.SeedState(
            mapper,
            entries,
            context,
            JArray.Parse("""[{ "Name": "tea", "Count": 7 }]"""));
        var firstLiveEvents = mapper.Process(JObject.Parse("""
            { "timestamp": "2026-07-28T12:12:00Z", "event": "Music", "MusicTrack": "Exploration" }
            """), context, true);

        Assert.Equal(6, seededCount);
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
    public void StartupReplayCountsMalformedLinesWithoutRetainingTheirContents()
    {
        using var reader = new StringReader("""
            { "timestamp": "2026-07-28T11:59:59Z", "event": "Fileheader" }
            this is not journal JSON and must not be logged

            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame" }
            """);

        var entries = Inara.ReadCurrentSession(reader, out var malformedCount);

        Assert.Equal(2, entries.Count);
        Assert.Equal(1, malformedCount);
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
    public void MissionDonationIsSubtractedFromTheReconstructedBalance()
    {
        var tracker = new InaraCreditTracker();
        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:00:00Z", "event": "LoadGame", "Credits": 1000, "Loan": 0 }
            """), false);
        Assert.NotNull(tracker.CreateReport("2026-07-28T12:00:00Z", true));

        tracker.Observe(JObject.Parse("""
            { "timestamp": "2026-07-28T12:05:00Z", "event": "MissionCompleted", "Reward": 100, "Donation": 300 }
            """), false);
        var report = tracker.CreateReport("2026-07-28T12:05:00Z", true);

        Assert.NotNull(report);
        Assert.Equal(800, report.Data.Value<long>("commanderCredits"));
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
