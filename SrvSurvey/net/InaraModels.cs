using Newtonsoft.Json.Linq;

namespace SrvSurvey.net
{
    internal sealed record InaraCredentials(string Commander, string FrontierId, string ApiKey);

    internal sealed record InaraContext(
        string? Commander,
        string? FrontierId,
        string? SystemName,
        string? StationName,
        string? BodyName,
        string? ShipType,
        long? ShipId,
        string? ShipName,
        string? ShipIdent,
        bool IsTaxi);

    internal sealed record InaraEvent(
        string Name,
        string Timestamp,
        JToken Data,
        string? ReplaceKey = null);

    internal sealed record InaraQueuedEvent(InaraCredentials Credentials, InaraEvent Event);

    internal static class InaraPayloadBuilder
    {
        public static JObject Build(
            string appVersion,
            InaraCredentials credentials,
            IReadOnlyCollection<InaraEvent> events,
            bool isBeingDeveloped)
        {
            var header = new JObject
            {
                ["appName"] = "SrvSurvey",
                ["appVersion"] = appVersion,
                ["isBeingDeveloped"] = isBeingDeveloped,
                ["APIkey"] = credentials.ApiKey,
                ["commanderName"] = credentials.Commander,
            };

            if (!string.IsNullOrWhiteSpace(credentials.FrontierId))
                header["commanderFrontierID"] = credentials.FrontierId;

            return new JObject
            {
                ["header"] = header,
                ["events"] = new JArray(events.Select(entry => new JObject
                {
                    ["eventName"] = entry.Name,
                    ["eventTimestamp"] = entry.Timestamp,
                    ["eventData"] = entry.Data.DeepClone(),
                })),
            };
        }
    }

    internal sealed class InaraEventQueue
    {
        private readonly object sync = new();
        private readonly List<InaraQueuedEvent> pending = new();

        public int Count
        {
            get
            {
                lock (sync)
                    return pending.Count;
            }
        }

        public void Enqueue(InaraCredentials credentials, IEnumerable<InaraEvent> events)
        {
            lock (sync)
            {
                foreach (var entry in events)
                {
                    if (!string.IsNullOrWhiteSpace(entry.ReplaceKey))
                    {
                        pending.RemoveAll(item =>
                            item.Credentials == credentials
                            && item.Event.ReplaceKey == entry.ReplaceKey);
                    }

                    pending.Add(new InaraQueuedEvent(credentials, entry));
                }
            }
        }

        public List<InaraQueuedEvent> TakeAll()
        {
            lock (sync)
            {
                var copy = pending.ToList();
                pending.Clear();
                return copy;
            }
        }

        public void Requeue(IEnumerable<InaraQueuedEvent> events)
        {
            lock (sync)
            {
                var retained = events
                    .Where(item => string.IsNullOrWhiteSpace(item.Event.ReplaceKey)
                        || !pending.Any(current => current.Credentials == item.Credentials
                            && current.Event.ReplaceKey == item.Event.ReplaceKey))
                    .ToList();
                pending.InsertRange(0, retained);
            }
        }
    }
}
