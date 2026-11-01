using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SrvSurvey.net
{
    internal sealed record EddnCompanionReadResult(JObject? content, string? error)
    {
        internal bool isSuccess => content != null;
    }

    internal static class EddnCompanionFileReader
    {
        private static readonly TimeSpan[] retryDelays =
        [
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(400),
        ];

        internal static async Task<EddnCompanionReadResult> read(
            string journalFolder,
            JObject journalEvent,
            CancellationToken cancellationToken = default,
            IReadOnlyList<TimeSpan>? retrySchedule = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(journalFolder);
            ArgumentNullException.ThrowIfNull(journalEvent);

            var eventName = journalEvent.Value<string>("event");
            if (!EddnMessageSanitizer.isCompanionEvent(eventName))
                return new EddnCompanionReadResult(
                    null,
                    "the journal event does not identify a supported companion file");

            var filepath = Path.Combine(journalFolder, eventName + ".json");
            var delays = retrySchedule ?? retryDelays;
            string? lastError = null;
            for (var attempt = 0; attempt <= delays.Count; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (!File.Exists(filepath))
                    {
                        lastError = $"{eventName}.json was not found";
                    }
                    else
                    {
                        using var stream = new FileStream(
                            filepath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite);
                        using var reader = new StreamReader(stream);
                        using var jsonReader = new JsonTextReader(reader);
                        var content = await JObject.LoadAsync(
                            jsonReader,
                            cancellationToken).ConfigureAwait(false);
                        if (content.Value<string>("event") != eventName)
                        {
                            lastError = $"{eventName}.json contained a different event";
                        }
                        else if (!matchesMarket(journalEvent, content))
                        {
                            lastError = $"{eventName}.json did not match the event's MarketID";
                        }
                        else if (!isCurrent(journalEvent, content))
                        {
                            lastError = $"{eventName}.json was older than the journal event";
                        }
                        else
                        {
                            return new EddnCompanionReadResult(content, null);
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException or JsonException)
                {
                    lastError = $"{eventName}.json could not be read: {ex.Message}";
                }

                if (attempt < delays.Count)
                    await Task.Delay(delays[attempt], cancellationToken).ConfigureAwait(false);
            }

            return new EddnCompanionReadResult(
                null,
                lastError ?? $"{eventName}.json could not be read");
        }

        private static bool matchesMarket(JObject journalEvent, JObject content)
        {
            var expected = journalEvent.Value<long?>("MarketID");
            return !expected.HasValue
                || expected <= 0
                || content.Value<long?>("MarketID") == expected;
        }

        private static bool isCurrent(JObject journalEvent, JObject content)
        {
            if (!DateTimeOffset.TryParse(
                    journalEvent.Value<string>("timestamp"),
                    out var eventTimestamp)
                || !DateTimeOffset.TryParse(
                    content.Value<string>("timestamp"),
                    out var fileTimestamp))
            {
                return true;
            }

            return fileTimestamp >= eventTimestamp;
        }
    }
}
