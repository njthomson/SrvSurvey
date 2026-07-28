using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Net;
using System.Text;

namespace SrvSurvey.net
{
    /// <summary>
    /// Collects and batches opted-in commander updates for Inara.
    /// </summary>
    internal sealed class Inara : IDisposable
    {
        internal const string Endpoint = "https://inara.cz/inapi/v1/";
        private static readonly TimeSpan sendInterval = TimeSpan.FromSeconds(35);
        private readonly HttpClient? client;
        private readonly InaraEventMapper mapper = new();
        private readonly InaraEventQueue queue = new();
        private readonly System.Threading.Timer? timer;
        private Game? currentGame;
        private int sending;

        public Inara()
        {
            HttpClient? configuredClient = null;
            System.Threading.Timer? configuredTimer = null;
            try
            {
                configuredClient = new HttpClient(Util.getResilienceHandler())
                {
                    Timeout = TimeSpan.FromSeconds(20),
                };
                configuredClient.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
                configuredTimer = new System.Threading.Timer(_ => sendPendingAsync().justDoIt(), null, sendInterval, sendInterval);
            }
            catch (Exception ex)
            {
                configuredTimer?.Dispose();
                configuredClient?.Dispose();
                configuredTimer = null;
                configuredClient = null;
                RunIsolated(() => Game.log($"Inara initialization was disabled without affecting SrvSurvey ({ex.GetType().Name})."));
            }

            client = configuredClient;
            timer = configuredTimer;
        }

        public void Dispose()
        {
            RunIsolated(() => timer?.Dispose());
            RunIsolated(() => client?.Dispose());
        }

        public void onGameInitialized(Game game)
        {
            if (client == null) return;

            RunIsolated(
                () => onGameInitializedCore(game),
                ex =>
                {
                    mapper.Reset();
                    currentGame = game;
                    Game.log($"Inara startup seeding was skipped without affecting SrvSurvey ({ex.GetType().Name}).");
                });
        }

        private void onGameInitializedCore(Game game)
        {
            mapper.Reset();
            currentGame = game;
            var credentials = getCredentials(game);
            var canPrepareUpload = CanPrepareUpload(
                Game.settings.inaraUpload,
                credentials?.ApiKey,
                IsLiveVersion(getGameVersion(game), game.journals?.isOdyssey == true),
                IsBetaVersion(getGameVersion(game)));
            var multiboxing = canPrepareUpload && isMultiboxing();

            var filepath = game.journals?.filepath;
            if (string.IsNullOrWhiteSpace(filepath)) return;

            try
            {
                using var reader = Data.openSharedStreamReader(filepath);
                var entries = ReadCurrentSession(reader);
                JArray? cargoInventory = null;
                if (canPrepareUpload && !multiboxing)
                {
                    var cargoFile = game.cargoFile;
                    cargoInventory = string.Equals(cargoFile.Vessel, "Ship", StringComparison.OrdinalIgnoreCase)
                        ? JArray.FromObject(cargoFile.Inventory ?? [])
                        : null;
                }

                var seededCount = SeedState(
                    mapper,
                    entries,
                    createContext(game, canPrepareUpload && !multiboxing),
                    cargoInventory);
                Game.log($"Inara seeded current state from {seededCount} journal event(s).");
                if (multiboxing)
                    Game.log("Inara multi-box mode: shared Cargo.json, ShipLocker.json, and Status.json data is suppressed.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Game.log($"Inara could not seed current journal state ({ex.GetType().Name}).");
            }
        }

        public void onJournalEntry(Game game, JObject raw)
        {
            if (client == null) return;

            RunIsolated(
                () => onJournalEntryCore(game, raw),
                ex => Game.log($"Inara ignored {raw["event"]?.ToString() ?? "unknown"} without affecting other journal processing ({ex.GetType().Name})."));
        }

        private void onJournalEntryCore(Game game, JObject raw)
        {
            // Manual calls made while Game reconstructs state from journal history must never upload.
            if (!Game.ready || Game.activeGame != game) return;

            if (!ReferenceEquals(currentGame, game))
            {
                mapper.Reset();
                currentGame = game;
            }

            var credentials = getCredentials(game);
            var gameVersion = getGameVersion(game);
            var isLive = IsLiveVersion(gameVersion, game.journals?.isOdyssey == true);
            var isBeta = IsBetaVersion(gameVersion);
            var canPrepareUpload = CanPrepareUpload(
                Game.settings.inaraUpload,
                credentials?.ApiKey,
                isLive,
                isBeta);

            if (!canPrepareUpload)
            {
                // Keep journal-derived state warm so enabling Inara mid-session is safe,
                // without reading shared sidecars/status or enumerating game processes.
                mapper.Process(raw, createContext(game, false), false);
                return;
            }

            var multiboxing = isMultiboxing();
            raw = addSidecarData(game, raw, !multiboxing);

            var canCollect = CanUpload(
                Game.settings.inaraUpload,
                credentials?.ApiKey,
                isLive,
                isBeta,
                mapper.InMulticrew);

            var context = createContext(game, !multiboxing);

            var events = mapper.Process(raw, context, canCollect);
            if (credentials != null && events.Count > 0)
            {
                queue.Enqueue(credentials, events);
                Game.log($"Inara queued {events.Count} event(s): {string.Join(", ", events.Select(e => e.Name).Distinct())}");
            }

            if (raw.Value<string>("event") == "Shutdown")
                sendPendingAsync().justDoIt();
        }

        internal static IReadOnlyList<JObject> ReadCurrentSession(TextReader reader)
        {
            var entries = new List<JObject>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                JObject entry;
                try
                {
                    entry = JObject.Parse(line);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (entry.Value<string>("event") == "LoadGame")
                    entries.Clear();
                entries.Add(entry);
            }

            return entries;
        }

        internal static int SeedState(
            InaraEventMapper mapper,
            IEnumerable<JObject> entries,
            InaraContext context,
            JArray? cargoInventory)
        {
            var count = 0;
            var timestamp = DateTime.UtcNow.ToString("O");
            foreach (var entry in entries)
            {
                mapper.Process(entry, context, false);
                timestamp = entry.Value<string>("timestamp") ?? timestamp;
                count++;
            }

            if (cargoInventory != null)
            {
                mapper.Process(new JObject
                {
                    ["timestamp"] = timestamp,
                    ["event"] = "Cargo",
                    ["Vessel"] = "Ship",
                    ["Inventory"] = cargoInventory.DeepClone(),
                }, context, false);
            }

            return count;
        }

        private static InaraContext createContext(Game game, bool allowSharedStatus) => new(
            game.Commander,
            game.fid,
            game.systemData?.name ?? game.cmdr?.currentSystem,
            game.systemStation?.name ?? game.lastDocked?.StationName,
            allowSharedStatus ? game.systemBody?.name : null,
            game.currentShip?.type,
            game.currentShip?.id,
            game.currentShip?.name,
            game.currentShip?.ident,
            allowSharedStatus ? game.status?.InTaxi == true : null);

        private static bool isMultiboxing() => DetectMultiboxing(
            Elite.hadManyGameProcs,
            countGameProcesses,
            ex => Game.log($"Inara could not count Elite processes and conservatively enabled multi-box suppression ({ex.GetType().Name})."));

        private static int countGameProcesses()
        {
            var gameProcesses = Elite.GetGameProcs();
            try
            {
                return gameProcesses.Length;
            }
            finally
            {
                foreach (var process in gameProcesses)
                {
                    try { process.Dispose(); }
                    catch { /* best effort only */ }
                }
            }
        }

        internal static bool DetectMultiboxing(
            bool alreadyDetected,
            Func<int> countGameProcesses,
            Action<Exception>? onError = null)
        {
            if (alreadyDetected) return true;

            try
            {
                return countGameProcesses() > 1;
            }
            catch (Exception ex)
            {
                try { onError?.Invoke(ex); }
                catch { /* optional diagnostics must not escape */ }
                return true;
            }
        }

        internal static bool RunIsolated(Action action, Action<Exception>? onError = null)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                try { onError?.Invoke(ex); }
                catch { /* optional diagnostics must not escape */ }
                return false;
            }
        }

        internal static bool CanPrepareUpload(bool optedIn, string? apiKey, bool isLive, bool isBeta) =>
            optedIn
            && !string.IsNullOrWhiteSpace(apiKey)
            && isLive
            && !isBeta;

        internal static bool CanUpload(bool optedIn, string? apiKey, bool isLive, bool isBeta, bool inMulticrew) =>
            CanPrepareUpload(optedIn, apiKey, isLive, isBeta)
            && !inMulticrew;

        internal static bool IsBetaVersion(string? gameVersion)
        {
            if (string.IsNullOrWhiteSpace(gameVersion)) return false;
            return gameVersion.Contains("beta", StringComparison.OrdinalIgnoreCase)
                || gameVersion.Contains("alpha", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsLiveVersion(string? gameVersion, bool odyssey)
        {
            if (odyssey) return true;
            if (string.IsNullOrWhiteSpace(gameVersion)) return false;

            var numeric = new string(gameVersion.TakeWhile(character => char.IsDigit(character) || character == '.').ToArray());
            return Version.TryParse(numeric.TrimEnd('.'), out var version) && version.Major >= 4;
        }

        internal async Task flushAsync() => await sendPendingAsync();

        private static string? getGameVersion(Game game) =>
            game.journals?.Entries.OfType<Fileheader>().FirstOrDefault()?.gameversion
            ?? game.journals?.Entries.OfType<LoadGame>().LastOrDefault()?.gameversion;

        private static InaraCredentials? getCredentials(Game game)
        {
            var commander = game.Commander;
            var frontierId = game.fid;
            var apiKey = game.cmdr?.inaraApiKey?.Trim();
            if (string.IsNullOrWhiteSpace(commander) || string.IsNullOrWhiteSpace(apiKey)) return null;
            return new InaraCredentials(commander, frontierId ?? string.Empty, apiKey);
        }

        private static JObject addSidecarData(Game game, JObject raw, bool allowSharedSidecars)
        {
            var eventName = raw.Value<string>("event");
            var needsCargoSidecar = eventName == "Cargo"
                && raw.Value<string>("Vessel") == "Ship"
                && raw["Inventory"] is not JArray;
            var needsLockerSidecar = eventName == "ShipLocker"
                && new[] { "Items", "Components", "Data", "Consumables" }
                    .Any(type => raw[type] is not JArray);

            if (!allowSharedSidecars && (needsCargoSidecar || needsLockerSidecar))
            {
                Game.log($"Inara ignored shared {eventName} sidecar data while multi-boxing.");
                return raw;
            }

            if (needsCargoSidecar)
            {
                var augmented = (JObject)raw.DeepClone();
                augmented["Inventory"] = JArray.FromObject(game.cargoFile.Inventory ?? []);
                return augmented;
            }

            if (needsLockerSidecar)
            {
                try
                {
                    var journalFolder = Path.GetDirectoryName(game.journals?.filepath);
                    var filepath = journalFolder == null ? null : Path.Combine(journalFolder, "ShipLocker.json");
                    if (filepath != null && File.Exists(filepath))
                    {
                        using var reader = Data.openSharedStreamReader(filepath);
                        var sidecar = JObject.Parse(reader.ReadToEnd());
                        sidecar["event"] = eventName;
                        sidecar["timestamp"] = raw["timestamp"]?.DeepClone();
                        return sidecar;
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
                {
                    Game.log($"Inara could not read ShipLocker.json ({ex.GetType().Name}).");
                }
            }

            return raw;
        }

        private async Task sendPendingAsync()
        {
            var uploadClient = client;
            if (uploadClient == null) return;

            if (Interlocked.Exchange(ref sending, 1) != 0) return;
            try
            {
                var pending = queue.TakeAll();
                if (pending.Count == 0) return;

                foreach (var group in pending.GroupBy(item => item.Credentials))
                {
                    var batch = group.ToList();
                    if (!Game.settings.inaraUpload)
                        continue;

                    var activeGame = Game.activeGame;
                    if (activeGame != null
                        && string.Equals(activeGame.Commander, group.Key.Commander, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(activeGame.cmdr?.inaraApiKey?.Trim(), group.Key.ApiKey, StringComparison.Ordinal))
                    {
                        Game.log($"Inara discarded {batch.Count} queued event(s) after the commander API key changed.");
                        continue;
                    }

                    try
                    {
                        var payload = InaraPayloadBuilder.Build(
                            Program.releaseVersion,
                            group.Key,
                            batch.Select(item => item.Event).ToList(),
                            Game.settings.inaraDeveloperTestMode);
                        using var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                        using var response = await uploadClient.PostAsync(Endpoint, content);

                        if (isTransient(response.StatusCode))
                        {
                            queue.Requeue(batch);
                            Game.log($"Inara upload deferred after HTTP {(int)response.StatusCode}; {batch.Count} event(s) retained.");
                            continue;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            Game.log($"Inara rejected {batch.Count} event(s) with HTTP {(int)response.StatusCode}.");
                            continue;
                        }

                        var body = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(body))
                        {
                            queue.Requeue(batch);
                            Game.log($"Inara returned an empty response; {batch.Count} event(s) retained.");
                            continue;
                        }

                        var result = JObject.Parse(body);
                        var headerStatus = result.SelectToken("header.eventStatus")?.Value<int?>();
                        var responseEvents = result["events"] as JArray;
                        var responseIsComplete = headerStatus != null
                            && responseEvents?.Count == batch.Count
                            && responseEvents.OfType<JObject>().All(eventResult => eventResult["eventStatus"] != null);
                        if (!responseIsComplete)
                        {
                            queue.Requeue(batch);
                            Game.log($"Inara returned an incomplete response; {batch.Count} event(s) retained.");
                            continue;
                        }

                        if (headerStatus is >= 400)
                        {
                            Game.log($"Inara rejected a batch of {batch.Count} event(s) with API status {headerStatus}.");
                            continue;
                        }

                        var failedEvents = responseEvents!.OfType<JObject>()
                            .Select((eventResult, index) => new { index, status = eventResult.Value<int?>("eventStatus") })
                            .Where(item => item.status is >= 400)
                            .ToList();
                        if (failedEvents.Count > 0)
                        {
                            var names = failedEvents
                                .Where(item => item.index < batch.Count)
                                .Select(item => batch[item.index].Event.Name)
                                .Distinct();
                            Game.log($"Inara rejected {failedEvents.Count} event(s): {string.Join(", ", names)}.");
                        }
                        else
                        {
                            Game.log($"Inara accepted {batch.Count} event(s).");
                        }
                    }
                    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                    {
                        queue.Requeue(batch);
                        Game.log($"Inara upload deferred ({ex.GetType().Name}); {batch.Count} event(s) retained.");
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref sending, 0);
            }
        }

        private static bool isTransient(HttpStatusCode status) =>
            status == HttpStatusCode.RequestTimeout
            || status == HttpStatusCode.TooManyRequests
            || (int)status >= 500;
    }
}
