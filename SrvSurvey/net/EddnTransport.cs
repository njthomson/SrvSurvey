using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SrvSurvey.net
{
    internal sealed class EddnTransport : IDisposable
    {
        internal const int MaximumPayloadBytes = 1024 * 1024;
        internal const int MaximumUncompressedPayloadBytes = 10 * 1024 * 1024;
        internal const int MaximumResponseDetailBytes = 2048;
        internal const bool testSchemasEnabled = true;

        private static readonly Uri defaultEndpoint =
            new("https://eddn.edcd.io:4430/upload/");

        private readonly HttpClient client;
        private readonly Uri endpoint;
        private readonly bool ownsClient;

        internal EddnTransport(
            HttpClient? client = null,
            Uri? endpoint = null,
            string? userAgent = null)
        {
            ownsClient = client == null;
            this.client = client ?? createClient(userAgent ?? "SrvSurvey");
            this.endpoint = endpoint ?? defaultEndpoint;
            validateEndpoint(this.endpoint);
        }

        internal EddnQueuedMessage prepare(
            JObject message,
            string schemaRef,
            UploadPayloadHeader header)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaRef);
            ArgumentNullException.ThrowIfNull(header);

            schemaRef = applySchemaPolicy(schemaRef, testSchemasEnabled);

            return new EddnQueuedMessage
            {
                id = Guid.NewGuid(),
                created = DateTimeOffset.UtcNow,
                nextAttempt = DateTimeOffset.UtcNow,
                schemaRef = schemaRef,
                header = header.clone(),
                message = new JObject(message),
            };
        }

        internal static string applySchemaPolicy(
            string schemaRef,
            bool useTestSchemas)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaRef);
            var liveSchemaRef = schemaRef.EndsWith("/test", StringComparison.Ordinal)
                ? schemaRef[..^"/test".Length]
                : schemaRef;
            return useTestSchemas ? liveSchemaRef + "/test" : liveSchemaRef;
        }

        internal async Task<EddnUploadResult> upload(
            JObject message,
            string schemaRef,
            UploadPayloadHeader header,
            CancellationToken cancellationToken = default)
        {
            return await upload(
                prepare(message, schemaRef, header),
                cancellationToken).ConfigureAwait(false);
        }

        internal async Task<EddnUploadResult> upload(
            EddnQueuedMessage queued,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(queued);

            var payload = JsonConvert.SerializeObject(
                queued.toPayload(),
                Formatting.None);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            if (payloadBytes.Length > MaximumUncompressedPayloadBytes)
            {
                return EddnUploadResult.skipped(
                    queued.schemaRef,
                    $"the encoded message exceeded {MaximumUncompressedPayloadBytes:N0} uncompressed bytes");
            }

            var compressed = compress(payloadBytes);
            if (compressed.Length > MaximumPayloadBytes)
            {
                return EddnUploadResult.skipped(
                    queued.schemaRef,
                    $"the compressed message exceeded {MaximumPayloadBytes:N0} bytes");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint)
            {
                Version = HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Content = new ByteArrayContent(compressed),
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8",
            };
            request.Content.Headers.ContentEncoding.Add("gzip");

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            var detail = response.IsSuccessStatusCode
                ? string.Empty
                : await readBoundedResponse(response.Content, cancellationToken).ConfigureAwait(false);

            return new EddnUploadResult(
                queued.schemaRef,
                response.StatusCode,
                response.ReasonPhrase ?? string.Empty,
                detail,
                null);
        }

        private static byte[] compress(byte[] payload)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
                gzip.Write(payload);
            return output.ToArray();
        }

        private static HttpClient createClient(string userAgent)
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            return client;
        }

        private static async Task<string> readBoundedResponse(
            HttpContent content,
            CancellationToken cancellationToken)
        {
            using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var buffer = new byte[MaximumResponseDetailBytes];
            var total = 0;
            while (total < buffer.Length)
            {
                var read = await stream.ReadAsync(
                    buffer.AsMemory(total, buffer.Length - total),
                    cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                total += read;
            }

            return Encoding.UTF8.GetString(buffer, 0, total);
        }

        private static void validateEndpoint(Uri endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            if (!endpoint.IsAbsoluteUri
                || endpoint.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException(
                    "The EDDN live endpoint must be an absolute HTTPS URI.",
                    nameof(endpoint));
            }
        }

        public void Dispose()
        {
            if (ownsClient) client.Dispose();
        }
    }

    internal sealed record EddnUploadResult(
        string schemaRef,
        HttpStatusCode? statusCode,
        string reasonPhrase,
        string responseDetail,
        string? skipReason)
    {
        internal bool isSuccess => statusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices;

        internal bool isRetryable => skipReason == null
            && !isSuccess
            && statusCode is not (
                HttpStatusCode.BadRequest
                or HttpStatusCode.RequestEntityTooLarge
                or HttpStatusCode.UpgradeRequired);

        internal static EddnUploadResult skipped(
            string schemaRef,
            string reason)
        {
            return new EddnUploadResult(
                schemaRef,
                null,
                string.Empty,
                string.Empty,
                reason);
        }
    }

    internal sealed class EddnQueuedMessage
    {
        public Guid id;
        public DateTimeOffset created;
        public DateTimeOffset nextAttempt;
        public int attempts;
        public string schemaRef = string.Empty;
        public UploadPayloadHeader header = new();
        public JObject message = new();

        internal JObject toPayload()
        {
            return new JObject
            {
                ["$schemaRef"] = schemaRef,
                ["header"] = JObject.FromObject(header),
                ["message"] = new JObject(message),
            };
        }
    }

    internal sealed class UploadPayloadHeader
    {
        public string uploaderID = string.Empty;
        public string softwareName = "SrvSurvey";
        public string softwareVersion = string.Empty;
        public string gameversion = string.Empty;
        public string gamebuild = string.Empty;

        public UploadPayloadHeader() { }

        internal UploadPayloadHeader(
            string uploaderID,
            string? gameVersion,
            string? gameBuild,
            string softwareVersion)
        {
            this.uploaderID = uploaderID;
            this.gameversion = gameVersion ?? string.Empty;
            this.gamebuild = gameBuild ?? string.Empty;
            this.softwareVersion = softwareVersion;
        }

        internal UploadPayloadHeader clone()
        {
            return new UploadPayloadHeader(
                uploaderID,
                gameversion,
                gamebuild,
                softwareVersion)
            {
                softwareName = softwareName,
            };
        }
    }
}
