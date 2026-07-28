using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnTransportTests
{
    [Fact]
    public void CodexMessageMatchesTheSchemaSurface()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10,"Name":"$Codex_Ent_Bacterial_01_Name;","Name_Localised":"Bacterium","BodyID":4,"IsNewEntry":true,"NewTraitsDiscovered":true}
            """);

        var message = EddnMessageSanitizer.codexEntry(
            raw,
            [1.5, -2, 3],
            odyssey: true,
            horizons: true,
            statusBodyName: "Test A 1",
            trackedBodyName: "Test A 1",
            trackedBodyId: 4);

        Assert.Equal("Test A", message.Value<string>("System"));
        Assert.Null(message["StarSystem"]);
        Assert.Null(message["Name_Localised"]);
        Assert.Null(message["IsNewEntry"]);
        Assert.Null(message["NewTraitsDiscovered"]);
        Assert.Equal("Test A 1", message.Value<string>("BodyName"));
        Assert.Equal(4, message.Value<int>("BodyID"));
        Assert.Equal([1.5, -2, 3], message["StarPos"]!.Values<double>());
        Assert.True(message.Value<bool>("odyssey"));
        Assert.True(message.Value<bool>("horizons"));
    }

    [Fact]
    public void CodexBodyIdRequiresStatusAndJournalAgreement()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10,"BodyID":4}
            """);

        var message = EddnMessageSanitizer.codexEntry(
            raw,
            [1.5, -2, 3],
            odyssey: null,
            horizons: null,
            statusBodyName: "Test A 2",
            trackedBodyName: "Test A 1",
            trackedBodyId: 4);

        Assert.Null(message["BodyName"]);
        Assert.Null(message["BodyID"]);
        Assert.Null(message["odyssey"]);
        Assert.Null(message["horizons"]);
    }

    [Fact]
    public async Task LiveUploadUsesExactHttp11AndCanonicalHeaderNames()
    {
        RecordedRequest? recorded = null;
        var transport = createTransport(async request =>
        {
            recorded = await record(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var result = await transport.upload(
            message(),
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header(),
            "live");

        Assert.True(result.isSuccess);
        Assert.Equal("live", result.environment);
        Assert.NotNull(recorded);
        Assert.Equal("https://live.example.test/upload/", recorded.uri.ToString());
        Assert.Equal(HttpVersion.Version11, recorded.version);
        Assert.Equal(HttpMethod.Post, recorded.method);
        Assert.StartsWith("application/json", recorded.contentType);

        var payload = JObject.Parse(recorded.content);
        Assert.Equal(
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            payload.Value<string>("$schemaRef"));
        var payloadHeader = Assert.IsType<JObject>(payload["header"]);
        Assert.Equal("4.1.2.3", payloadHeader.Value<string>("gameversion"));
        Assert.Equal("r123/r0 ", payloadHeader.Value<string>("gamebuild"));
        Assert.Null(payloadHeader["gameVersion"]);
    }

    [Fact]
    public async Task DevelopmentUploadSerializesTheTestSchemaReference()
    {
        RecordedRequest? recorded = null;
        var transport = createTransport(async request =>
        {
            recorded = await record(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var result = await transport.upload(
            message(),
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header(),
            "dev");

        Assert.True(result.isSuccess);
        Assert.Equal("dev", result.environment);
        Assert.NotNull(recorded);
        Assert.Equal("https://dev.example.test/upload/", recorded.uri.ToString());
        Assert.Equal(
            "https://eddn.edcd.io/schemas/dockinggranted/1/test",
            JObject.Parse(recorded.content).Value<string>("$schemaRef"));
    }

    [Fact]
    public async Task OversizedPayloadIsSkippedWithoutARequest()
    {
        var calls = 0;
        var transport = createTransport(request =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var oversized = message();
        oversized["detail"] = new string('x', EddnTransport.MaximumPayloadBytes);

        var result = await transport.upload(
            oversized,
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header(),
            "live");

        Assert.Equal(0, calls);
        Assert.False(result.isSuccess);
        Assert.Null(result.statusCode);
        Assert.Contains("exceeded", result.skipReason);
    }

    [Fact]
    public async Task FailureResponseDetailIsBounded()
    {
        var transport = createTransport(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(new string('x', 10_000)),
            }));

        var result = await transport.upload(
            message(),
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header(),
            "live");

        Assert.False(result.isSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.statusCode);
        Assert.Equal(EddnTransport.MaximumResponseDetailBytes, result.responseDetail.Length);
    }

    [Theory]
    [InlineData(null, "live")]
    [InlineData("", "live")]
    [InlineData("unexpected", "live")]
    [InlineData(" BETA ", "beta")]
    [InlineData("DEV", "dev")]
    public void EnvironmentIsRestrictedToKnownDestinations(
        string? value,
        string expected)
    {
        Assert.Equal(expected, EddnTransport.normalizeEnvironment(value));
    }

    private static EddnTransport createTransport(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
    {
        return new EddnTransport(
            new HttpClient(new StubHandler(response)),
            new Dictionary<string, Uri>(StringComparer.Ordinal)
            {
                ["dev"] = new("https://dev.example.test/upload/"),
                ["beta"] = new("https://beta.example.test/upload/"),
                ["live"] = new("https://live.example.test/upload/"),
            });
    }

    private static UploadPayloadHeader header()
    {
        return new UploadPayloadHeader(
            "Test Cmdr",
            "4.1.2.3",
            "r123/r0 ",
            "2.0.95.0");
    }

    private static JObject message()
    {
        return JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"DockingGranted","MarketID":1,"StationName":"Test Port"}
            """);
    }

    private static async Task<RecordedRequest> record(HttpRequestMessage request)
    {
        return new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Version,
            request.Content?.Headers.ContentType?.ToString() ?? string.Empty,
            await request.Content!.ReadAsStringAsync());
    }

    private sealed record RecordedRequest(
        HttpMethod method,
        Uri uri,
        Version version,
        string contentType,
        string content);

    private sealed class StubHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return response(request);
        }
    }
}
