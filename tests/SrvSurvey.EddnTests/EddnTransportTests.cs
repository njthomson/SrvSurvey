using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnTransportTests
{
    [Fact]
    public async Task UploadUsesGzipExactHttp11AndNoAuthentication()
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
            header());

        Assert.True(result.isSuccess);
        Assert.NotNull(recorded);
        Assert.Equal("https://live.example.test/upload/", recorded.uri.ToString());
        Assert.Equal(HttpVersion.Version11, recorded.version);
        Assert.Equal(HttpVersionPolicy.RequestVersionExact, recorded.versionPolicy);
        Assert.Equal(HttpMethod.Post, recorded.method);
        Assert.StartsWith("application/json", recorded.contentType);
        Assert.Equal("gzip", recorded.contentEncoding);
        Assert.Null(recorded.authorization);

        var payload = JObject.Parse(recorded.content);
        Assert.Equal(
            "https://eddn.edcd.io/schemas/dockinggranted/1/test",
            payload.Value<string>("$schemaRef"));
        var payloadHeader = Assert.IsType<JObject>(payload["header"]);
        Assert.Equal("Test Cmdr", payloadHeader.Value<string>("uploaderID"));
        Assert.Equal("4.1.2.3", payloadHeader.Value<string>("gameversion"));
        Assert.Equal("r123/r0 ", payloadHeader.Value<string>("gamebuild"));
        Assert.Null(payloadHeader["gameVersion"]);
    }

    [Fact]
    public async Task FixedTestSchemaPolicyStillUsesTheLiveGateway()
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
            header());

        Assert.True(result.isSuccess);
        Assert.NotNull(recorded);
        Assert.Equal("https://live.example.test/upload/", recorded.uri.ToString());
        Assert.Equal(
            "https://eddn.edcd.io/schemas/dockinggranted/1/test",
            JObject.Parse(recorded.content).Value<string>("$schemaRef"));
    }

    [Fact]
    public async Task ExistingTestSchemaSuffixIsNotDuplicated()
    {
        RecordedRequest? recorded = null;
        var transport = createTransport(async request =>
        {
            recorded = await record(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        await transport.upload(
            message(),
            "https://eddn.edcd.io/schemas/dockinggranted/1/test",
            header());

        Assert.NotNull(recorded);
        Assert.Equal(
            "https://eddn.edcd.io/schemas/dockinggranted/1/test",
            JObject.Parse(recorded.content).Value<string>("$schemaRef"));
    }

    [Theory]
    [InlineData("https://eddn.edcd.io/schemas/dockinggranted/1", false, "https://eddn.edcd.io/schemas/dockinggranted/1")]
    [InlineData("https://eddn.edcd.io/schemas/dockinggranted/1/test", false, "https://eddn.edcd.io/schemas/dockinggranted/1")]
    [InlineData("https://eddn.edcd.io/schemas/dockinggranted/1", true, "https://eddn.edcd.io/schemas/dockinggranted/1/test")]
    [InlineData("https://eddn.edcd.io/schemas/dockinggranted/1/test", true, "https://eddn.edcd.io/schemas/dockinggranted/1/test")]
    public void SchemaPolicyNormalizesLiveAndTestReferences(
        string schemaRef,
        bool useTestSchemas,
        string expected)
    {
        Assert.Equal(
            expected,
            EddnTransport.applySchemaPolicy(schemaRef, useTestSchemas));
    }

    [Fact]
    public async Task QueuedMessageCannotSelectAnotherGateway()
    {
        RecordedRequest? recorded = null;
        var transport = createTransport(async request =>
        {
            recorded = await record(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var queued = transport.prepare(
            message(),
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header());
        var result = await transport.upload(queued);

        Assert.True(result.isSuccess);
        Assert.NotNull(recorded);
        Assert.Equal("https://live.example.test/upload/", recorded.uri.ToString());
        Assert.Equal(
            "https://eddn.edcd.io/schemas/dockinggranted/1/test",
            JObject.Parse(recorded.content).Value<string>("$schemaRef"));
    }

    [Fact]
    public async Task OversizedUncompressedPayloadIsSkippedWithoutARequest()
    {
        var calls = 0;
        var transport = createTransport(request =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var oversized = message();
        oversized["detail"] = new string(
            'x',
            EddnTransport.MaximumUncompressedPayloadBytes);

        var result = await transport.upload(
            oversized,
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header());

        Assert.Equal(0, calls);
        Assert.False(result.isSuccess);
        Assert.Null(result.statusCode);
        Assert.Contains("uncompressed", result.skipReason);
    }

    [Fact]
    public async Task OversizedCompressedPayloadIsSkippedWithoutARequest()
    {
        var calls = 0;
        var transport = createTransport(request =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var oversized = message();
        oversized["detail"] = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(EddnTransport.MaximumPayloadBytes + 64_000));

        var result = await transport.upload(
            oversized,
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header());

        Assert.Equal(0, calls);
        Assert.False(result.isSuccess);
        Assert.Null(result.statusCode);
        Assert.Contains("compressed", result.skipReason);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.RequestEntityTooLarge, false)]
    [InlineData(HttpStatusCode.UpgradeRequired, false)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    public async Task RetryClassificationMatchesTheGatewayContract(
        HttpStatusCode statusCode,
        bool expectedRetryable)
    {
        var transport = createTransport(_ => Task.FromResult(
            new HttpResponseMessage(statusCode)));

        var result = await transport.upload(
            message(),
            "https://eddn.edcd.io/schemas/dockinggranted/1",
            header());

        Assert.Equal(expectedRetryable, result.isRetryable);
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
            header());

        Assert.False(result.isSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.statusCode);
        Assert.Equal(EddnTransport.MaximumResponseDetailBytes, result.responseDetail.Length);
    }

    internal static EddnTransport createTransport(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
    {
        return new EddnTransport(
            new HttpClient(new StubHandler(response)),
            new Uri("https://live.example.test/upload/"));
    }

    internal static UploadPayloadHeader header()
    {
        return new UploadPayloadHeader(
            "Test Cmdr",
            "4.1.2.3",
            "r123/r0 ",
            "2.0.95.0");
    }

    internal static JObject message()
    {
        return JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"DockingGranted","MarketID":1,"StationName":"Test Port"}
            """);
    }

    private static async Task<RecordedRequest> record(HttpRequestMessage request)
    {
        var compressed = await request.Content!.ReadAsByteArrayAsync();
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        return new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Version,
            request.VersionPolicy,
            request.Content.Headers.ContentType?.ToString() ?? string.Empty,
            string.Join(',', request.Content.Headers.ContentEncoding),
            request.Headers.Authorization?.ToString(),
            await reader.ReadToEndAsync());
    }

    private sealed record RecordedRequest(
        HttpMethod method,
        Uri uri,
        Version version,
        HttpVersionPolicy versionPolicy,
        string contentType,
        string contentEncoding,
        string? authorization,
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
