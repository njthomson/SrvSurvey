using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SrvSurvey.net
{
    internal sealed class EddnTransport
    {
        internal const int MaximumPayloadBytes = 1024 * 1024;
        internal const int MaximumUncompressedPayloadBytes = 10 * 1024 * 1024;
        internal const int MaximumResponseDetailBytes = 2048;

        private static readonly IReadOnlyDictionary<string, Uri> defaultEndpoints =
            new Dictionary<string, Uri>(StringComparer.Ordinal)
            {
                ["dev"] = new("https://dev.eddn.edcd.io:4432/upload/"),
                ["beta"] = new("https://beta.eddn.edcd.io:4431/upload/"),
                ["live"] = new("https://eddn.edcd.io:4430/upload/"),
            };

        private readonly HttpClient client;
        private readonly IReadOnlyDictionary<string, Uri> endpoints;

        internal EddnTransport(
            HttpClient? client = null,
            IReadOnlyDictionary<string, Uri>? endpoints = null,
            string? userAgent = null)
        {
            this.client = client ?? createClient(userAgent ?? "SrvSurvey");
            this.endpoints = endpoints ?? defaultEndpoints;
            validateEndpoints(this.endpoints);
        }

        internal EddnQueuedMessage prepare(
            JObject message,
            string schemaRef,
            UploadPayloadHeader header,
            string? environment)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaRef);
            ArgumentNullException.ThrowIfNull(header);

            var normalizedEnvironment = normalizeEnvironment(environment);
            if (normalizedEnvironment != "live" && !schemaRef.EndsWith("/test", StringComparison.Ordinal))
                schemaRef += "/test";

            return new EddnQueuedMessage
            {
                id = Guid.NewGuid(),
                created = DateTimeOffset.UtcNow,
                nextAttempt = DateTimeOffset.UtcNow,
                environment = normalizedEnvironment,
                schemaRef = schemaRef,
                header = header.clone(),
                message = new JObject(message),
            };
        }

        internal async Task<EddnUploadResult> upload(
            JObject message,
            string schemaRef,
            UploadPayloadHeader header,
            string? environment,
            CancellationToken cancellationToken = default)
        {
            return await upload(
                prepare(message, schemaRef, header, environment),
                cancellationToken);
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
                    queued.environment,
                    queued.schemaRef,
                    $"the encoded message exceeded {MaximumUncompressedPayloadBytes:N0} uncompressed bytes");
            }

            var compressed = compress(payloadBytes);
            if (compressed.Length > MaximumPayloadBytes)
            {
                return EddnUploadResult.skipped(
                    queued.environment,
                    queued.schemaRef,
                    $"the compressed message exceeded {MaximumPayloadBytes:N0} bytes");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoints[queued.environment])
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
                cancellationToken);
            var detail = response.IsSuccessStatusCode
                ? string.Empty
                : await readBoundedResponse(response.Content, cancellationToken);

            return new EddnUploadResult(
                queued.environment,
                queued.schemaRef,
                response.StatusCode,
                response.ReasonPhrase ?? string.Empty,
                detail,
                null);
        }

        internal static string normalizeEnvironment(string? value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "dev" => "dev",
                "beta" => "beta",
                _ => "live",
            };
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
            using var stream = await content.ReadAsStreamAsync(cancellationToken);
            var buffer = new byte[MaximumResponseDetailBytes];
            var total = 0;
            while (total < buffer.Length)
            {
                var read = await stream.ReadAsync(
                    buffer.AsMemory(total, buffer.Length - total),
                    cancellationToken);
                if (read == 0) break;
                total += read;
            }

            return Encoding.UTF8.GetString(buffer, 0, total);
        }

        private static void validateEndpoints(IReadOnlyDictionary<string, Uri> endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);
            foreach (var environment in new[] { "dev", "beta", "live" })
            {
                if (!endpoints.TryGetValue(environment, out var endpoint)
                    || !endpoint.IsAbsoluteUri
                    || endpoint.Scheme != Uri.UriSchemeHttps)
                {
                    throw new ArgumentException(
                        $"The EDDN {environment} endpoint must be an absolute HTTPS URI.",
                        nameof(endpoints));
                }
            }
        }
    }

    internal sealed record EddnUploadResult(
        string environment,
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
            string environment,
            string schemaRef,
            string reason)
        {
            return new EddnUploadResult(
                environment,
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
        public string environment = "live";
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
