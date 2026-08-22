using Newtonsoft.Json.Linq;
using SrvSurvey.game;

// Behavioral reference:
// https://github.com/EDCD/EDMarketConnector/blob/2b6a0ce1ee3ba60c21f3f4e9fa093046da8825e4/plugins/inara.py
// Copyright (c) EDCD, licensed under GNU GPL v2 or later.

namespace SrvSurvey.net
{
    internal enum InaraStopReason
    {
        Normal,
        KeyCleared,
    }

    internal sealed record InaraCredentials(string Commander, string FrontierId, string ApiKey)
    {
        public override string ToString() => $"InaraCredentials {{ Commander = {Commander}, FrontierId = {FrontierId} }}";
    }

    /// <summary>
    /// Stable identity and mutable credentials for exactly one initialized game session.
    /// </summary>
    internal sealed class InaraSession
    {
        private readonly CommanderSettings settings;

        private InaraSession(
            CommanderSettings settings,
            string commander,
            string frontierId,
            string gameVersion,
            bool isLive,
            bool isBeta)
        {
            this.settings = settings;
            Commander = commander;
            FrontierId = frontierId;
            GameVersion = gameVersion;
            IsLive = isLive;
            IsBeta = isBeta;
        }

        public string Commander { get; }
        public string FrontierId { get; }
        public string GameVersion { get; }
        public bool IsLive { get; }
        public bool IsBeta { get; }

        public static InaraSession? Create(CommanderSettings? settings, string? gameVersion, bool odyssey)
        {
            var commander = settings?.commander?.Trim();
            var frontierId = settings?.fid?.Trim();
            var version = gameVersion?.Trim();
            if (settings == null
                || string.IsNullOrWhiteSpace(commander)
                || string.IsNullOrWhiteSpace(frontierId)
                || string.IsNullOrWhiteSpace(version))
                return null;

            return new InaraSession(
                settings,
                commander,
                frontierId,
                version,
                Inara.IsLiveVersion(version, odyssey),
                Inara.IsBetaVersion(version));
        }

        public InaraCredentials? GetCredentials()
        {
            var apiKey = settings.inaraApiKey?.Trim();
            return string.IsNullOrWhiteSpace(apiKey)
                ? null
                : new InaraCredentials(Commander, FrontierId, apiKey);
        }

        public bool Matches(InaraCredentials credentials) => GetCredentials() == credentials;
    }

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
        bool? IsTaxi);

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
            IReadOnlyCollection<InaraEvent> events)
        {
            var header = new JObject
            {
                ["appName"] = "SrvSurvey",
                ["appVersion"] = appVersion,
                // Inara requires test submissions while this application is being validated.
                ["isBeingDeveloped"] = true,
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
        internal const int MaxPendingEvents = 1000;
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

                trimToCapacity();
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
                trimToCapacity();
            }
        }

        public List<InaraQueuedEvent> TakeCurrent(InaraSession session, out int discarded)
        {
            lock (sync)
            {
                var credentials = session.GetCredentials();
                var current = pending.Where(item => item.Credentials == credentials).ToList();
                discarded = pending.Count - current.Count;
                pending.Clear();
                return current;
            }
        }

        public int DiscardNotCurrent(InaraSession session)
        {
            lock (sync)
            {
                var credentials = session.GetCredentials();
                return pending.RemoveAll(item => item.Credentials != credentials);
            }
        }

        private void trimToCapacity()
        {
            var overflow = pending.Count - MaxPendingEvents;
            if (overflow > 0)
                pending.RemoveRange(0, overflow);
        }
    }
}
