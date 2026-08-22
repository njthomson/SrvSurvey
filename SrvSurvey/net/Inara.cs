using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Net;
using System.Text;

// Behavioral reference:
// https://github.com/EDCD/EDMarketConnector/blob/2b6a0ce1ee3ba60c21f3f4e9fa093046da8825e4/plugins/inara.py
// Copyright (c) EDCD, licensed under GNU GPL v2 or later.

namespace SrvSurvey.net
{
    /// <summary>
    /// Collects and batches Inara updates for exactly one initialized game session.
    /// </summary>
    internal sealed class Inara : IDisposable
    {
        internal const string Endpoint = "https://inara.cz/inapi/v1/";
        private static readonly TimeSpan sendInterval = TimeSpan.FromSeconds(35);
        private readonly Game? game;
        private readonly InaraSession session;
        private readonly HttpClient client;
        private readonly InaraContext? testContext;
        private readonly Action<string> log;
        private readonly InaraEventMapper mapper = new();
        private readonly InaraEventQueue queue = new();
        private readonly SemaphoreSlim sendGate = new(1, 1);
        private readonly object ingestionSync = new();
        private readonly object stopSync = new();
        private System.Threading.Timer? timer;
        private Task? stopTask;
        private long retryNotBeforeUtcTicks;
        private int retryAttempt;
        private int stopping;
        private int disposed;

        private Inara(
            Game? game,
            InaraSession session,
            HttpClient client,
            InaraContext? testContext = null,
            Action<string>? log = null)
        {
            this.game = game;
            this.session = session;
            this.client = client;
            this.testContext = testContext;
            this.log = log ?? (message => Game.log(message));
        }

        internal static Inara CreateForTests(
            InaraSession session,
            HttpMessageHandler handler,
            InaraContext context,
            Action<string>? log = null,
            bool disposeHandler = false) =>
            new(null, session, new HttpClient(handler, disposeHandler)
            {
                Timeout = TimeSpan.FromSeconds(20),
            }, context, log ?? (_ => { }));

        public static Inara? Create(Game game)
        {
            HttpClient? client = null;
            Inara? inara = null;
            try
            {
                var filepath = game.journals?.filepath;
                if (string.IsNullOrWhiteSpace(filepath))
                    throw new InvalidOperationException("The initialized game has no journal filepath.");

                var fileheader = game.journals?.Entries.FirstOrDefault() as Fileheader;
                if (fileheader == null)
                    throw new InvalidOperationException("The initialized game journal does not start with Fileheader.");

                var session = InaraSession.Create(game.cmdr, fileheader.gameversion, game.journals?.isOdyssey == true);
                if (session == null)
                    throw new InvalidOperationException("The initialized game has no commander name, Frontier ID, or game version.");

                client = new HttpClient(Util.getResilienceHandler())
                {
                    Timeout = TimeSpan.FromSeconds(20),
                };
                client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);

                inara = new Inara(game, session, client);
                inara.seedCurrentSession(filepath);
                inara.timer = new System.Threading.Timer(
                    _ => inara.sendPendingAsync().justDoIt(),
                    null,
                    sendInterval,
                    sendInterval);
                return inara;
            }
            catch (Exception ex)
            {
                try { inara?.StopAsync(InaraStopReason.KeyCleared).GetAwaiter().GetResult(); }
                catch (Exception cleanupEx) { Game.log($"Inara initialization cleanup failed:\r\n{cleanupEx}"); }
                try { client?.Dispose(); }
                catch (Exception cleanupEx) { Game.log($"Inara HTTP cleanup failed:\r\n{cleanupEx}"); }
                Game.log($"Inara initialization was disabled without affecting SrvSurvey:\r\n{ex}");
                return null;
            }
        }

        private void seedCurrentSession(string filepath)
        {
            try
            {
                using var reader = Data.openSharedStreamReader(filepath);
                var entries = ReadCurrentSession(reader, out var malformedCount);
                if (malformedCount > 0)
                    log($"Inara skipped {malformedCount} malformed journal entr{(malformedCount == 1 ? "y" : "ies")} while seeding.");

                var credentials = session.GetCredentials();
                var canPrepareUpload = CanPrepareUpload(credentials?.ApiKey, session.IsLive, session.IsBeta);
                var multiboxing = canPrepareUpload && Elite.hadManyGameProcs;
                JArray? cargoInventory = null;
                if (canPrepareUpload && !multiboxing)
                {
                    var cargoFile = game!.cargoFile;
                    cargoInventory = string.Equals(cargoFile.Vessel, "Ship", StringComparison.OrdinalIgnoreCase)
                        ? JArray.FromObject(cargoFile.Inventory ?? [])
                        : null;
                }

                var seededCount = SeedState(
                    mapper,
                    entries,
                    createContext(canPrepareUpload && !multiboxing),
                    cargoInventory);
                log($"Inara seeded current state from {seededCount} journal event(s).");
                if (multiboxing)
                    log("Inara multi-box mode: shared Cargo.json, ShipLocker.json, and Status.json data is suppressed.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                mapper.Reset();
                log($"Inara could not seed current journal state without affecting SrvSurvey:\r\n{ex}");
            }
        }

        public void onJournalEntry(JObject raw)
        {
            if (Volatile.Read(ref stopping) != 0) return;

            lock (ingestionSync)
            {
                if (Volatile.Read(ref stopping) != 0) return;
                try
                {
                    onJournalEntryCore(raw);
                }
                catch (Exception ex)
                {
                    var eventName = raw["event"]?.ToString() ?? "unknown";
                    log($"Inara ignored {eventName} without affecting other journal processing:\r\n{ex}");
                }
            }
        }

        public void onApiKeyChanged()
        {
            resetRetryDelay();
            var discarded = queue.DiscardNotCurrent(session);
            if (discarded > 0)
                log($"Inara discarded {discarded} queued event(s) after the commander API key changed or was cleared.");
        }

        private void onJournalEntryCore(JObject raw)
        {
            var credentials = session.GetCredentials();
            var canPrepareUpload = CanPrepareUpload(credentials?.ApiKey, session.IsLive, session.IsBeta);
            if (!canPrepareUpload)
            {
                // Keep journal-derived state warm so adding a key mid-session is safe.
                mapper.Process(raw, createContext(false), false);
                return;
            }

            var multiboxing = Elite.hadManyGameProcs;
            raw = addSidecarData(raw, !multiboxing);
            var canCollect = CanUpload(
                credentials?.ApiKey,
                session.IsLive,
                session.IsBeta,
                mapper.InMulticrew);
            var events = mapper.Process(raw, createContext(!multiboxing), canCollect);
            if (credentials != null && events.Count > 0)
            {
                queue.Enqueue(credentials, events);
                log($"Inara queued {events.Count} event(s): {string.Join(", ", events.Select(e => e.Name).Distinct())}");
            }

            if (raw.Value<string>("event") == "Shutdown")
                sendPendingAsync().justDoIt();
        }

        internal static IReadOnlyList<JObject> ReadCurrentSession(TextReader reader) =>
            ReadCurrentSession(reader, out _);

        internal static IReadOnlyList<JObject> ReadCurrentSession(TextReader reader, out int malformedCount)
        {
            var entries = new List<JObject>();
            malformedCount = 0;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    entries.Add(JObject.Parse(line));
                }
                catch (JsonException)
                {
                    malformedCount++;
                }
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

        private InaraContext createContext(bool allowSharedStatus)
        {
            if (testContext != null)
                return testContext with { IsTaxi = allowSharedStatus ? testContext.IsTaxi : null };

            return new(
                session.Commander,
                session.FrontierId,
                game!.systemData?.name ?? game.cmdr.currentSystem,
                game.systemStation?.name ?? game.lastDocked?.StationName,
                allowSharedStatus ? game.systemBody?.name : null,
                game.currentShip?.type,
                game.currentShip?.id,
                game.currentShip?.name,
                game.currentShip?.ident,
                allowSharedStatus ? game.status?.InTaxi : null);
        }

        internal static bool CanPrepareUpload(string? apiKey, bool isLive, bool isBeta) =>
            !string.IsNullOrWhiteSpace(apiKey)
            && isLive
            && !isBeta;

        internal static bool CanUpload(string? apiKey, bool isLive, bool isBeta, bool inMulticrew) =>
            CanPrepareUpload(apiKey, isLive, isBeta)
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

        internal async Task flushAsync()
        {
            if (Volatile.Read(ref stopping) != 0 || Volatile.Read(ref disposed) != 0) return;
            await sendGate.WaitAsync();
            try
            {
                if (Volatile.Read(ref disposed) == 0)
                    await sendPendingCoreAsync();
            }
            finally
            {
                sendGate.Release();
            }
        }

        private JObject addSidecarData(JObject raw, bool allowSharedSidecars)
        {
            if (game == null) return raw;

            var eventName = raw.Value<string>("event");
            var needsCargoSidecar = eventName == "Cargo"
                && raw.Value<string>("Vessel") == "Ship"
                && raw["Inventory"] is not JArray;
            var needsLockerSidecar = eventName == "ShipLocker"
                && new[] { "Items", "Components", "Data", "Consumables" }
                    .Any(type => raw[type] is not JArray);

            if (!allowSharedSidecars && (needsCargoSidecar || needsLockerSidecar))
            {
                log($"Inara ignored shared {eventName} sidecar data while multi-boxing.");
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
                    log($"Inara could not read ShipLocker.json:\r\n{ex}");
                }
            }

            return raw;
        }

        private async Task sendPendingAsync()
        {
            if (Volatile.Read(ref stopping) != 0 || Volatile.Read(ref disposed) != 0) return;
            if (DateTime.UtcNow.Ticks < Interlocked.Read(ref retryNotBeforeUtcTicks)) return;
            if (!await sendGate.WaitAsync(0)) return;
            try
            {
                await sendPendingCoreAsync();
            }
            finally
            {
                sendGate.Release();
            }
        }

        private async Task sendPendingCoreAsync()
        {
            var pending = queue.TakeCurrent(session, out var discarded);
            if (discarded > 0)
                log($"Inara discarded {discarded} queued event(s) after the commander API key changed or was cleared.");
            if (pending.Count == 0) return;

            foreach (var group in pending.GroupBy(item => item.Credentials))
            {
                var batch = group.ToList();
                if (!session.Matches(group.Key))
                {
                    log($"Inara discarded {batch.Count} queued event(s) after the commander API key changed.");
                    continue;
                }

                try
                {
                    var payload = InaraPayloadBuilder.Build(
                        Program.releaseVersion,
                        group.Key,
                        batch.Select(item => item.Event).ToList());
                    using var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync(Endpoint, content);

                    if (isTransient(response.StatusCode))
                    {
                        queue.Requeue(batch);
                        scheduleRetry();
                        log($"Inara upload deferred after HTTP {(int)response.StatusCode} {response.ReasonPhrase}; {batch.Count} event(s) retained.");
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        resetRetryDelay();
                        log($"Inara rejected {batch.Count} event(s) with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
                        continue;
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(body))
                    {
                        queue.Requeue(batch);
                        scheduleRetry();
                        log($"Inara returned an empty response; {batch.Count} event(s) retained.");
                        continue;
                    }

                    var result = JObject.Parse(body);
                    var headerStatus = result.SelectToken("header.eventStatus")?.Value<int?>();
                    var responseEvents = result["events"] as JArray;
                    var responseIsComplete = headerStatus != null
                        && responseEvents?.Count == batch.Count
                        && responseEvents.All(token => token is JObject eventResult && eventResult["eventStatus"] != null);
                    if (!responseIsComplete)
                    {
                        queue.Requeue(batch);
                        scheduleRetry();
                        log($"Inara returned an incomplete response; {batch.Count} event(s) retained.");
                        continue;
                    }

                    resetRetryDelay();
                    var headerText = safeStatusText(result.SelectToken("header.eventStatusText")?.Value<string>());
                    if (headerStatus is >= 400)
                    {
                        log($"Inara rejected a batch of {batch.Count} event(s) with API status {headerStatus}: {headerText}");
                        continue;
                    }

                    var typedEvents = responseEvents!.Cast<JObject>().ToList();
                    var failedEvents = typedEvents
                        .Select((eventResult, index) => new
                        {
                            index,
                            status = eventResult.Value<int?>("eventStatus"),
                            text = safeStatusText(eventResult.Value<string>("eventStatusText")),
                        })
                        .Where(item => item.status is >= 400)
                        .ToList();
                    if (failedEvents.Count > 0)
                    {
                        var failures = failedEvents
                            .Where(item => item.index < batch.Count)
                            .Select(item => $"{batch[item.index].Event.Name} ({item.status}: {item.text})")
                            .Distinct();
                        log($"Inara rejected {failedEvents.Count} event(s): {string.Join(", ", failures)}.");
                    }
                    else
                    {
                        log($"Inara accepted {batch.Count} event(s).");
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or ObjectDisposedException)
                {
                    queue.Requeue(batch);
                    scheduleRetry();
                    log($"Inara upload deferred; {batch.Count} event(s) retained:\r\n{ex}");
                }
            }
        }

        internal static TimeSpan CalculateRetryDelay(int attempt, int jitterMilliseconds)
        {
            var boundedAttempt = Math.Clamp(attempt, 1, 5);
            var baseSeconds = Math.Min(35 * (1 << (boundedAttempt - 1)), 300);
            var boundedJitter = Math.Clamp(jitterMilliseconds, 0, 5_000);
            return TimeSpan.FromSeconds(baseSeconds) + TimeSpan.FromMilliseconds(boundedJitter);
        }

        private void scheduleRetry()
        {
            var attempt = Math.Min(Interlocked.Increment(ref retryAttempt), 5);
            var delay = CalculateRetryDelay(attempt, Random.Shared.Next(0, 5_001));
            Interlocked.Exchange(ref retryNotBeforeUtcTicks, DateTime.UtcNow.Add(delay).Ticks);
            try { timer?.Change(delay, sendInterval); }
            catch (ObjectDisposedException) { /* shutdown owns the disposed timer */ }
        }

        private void resetRetryDelay()
        {
            Interlocked.Exchange(ref retryAttempt, 0);
            Interlocked.Exchange(ref retryNotBeforeUtcTicks, 0);
        }

        private static string safeStatusText(string? value)
        {
            var normalized = value?.Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (string.IsNullOrWhiteSpace(normalized)) return "no status text";
            return normalized.Length <= 300 ? normalized : normalized[..300];
        }

        private static bool isTransient(HttpStatusCode status) =>
            status == HttpStatusCode.RequestTimeout
            || status == HttpStatusCode.TooManyRequests
            || (int)status >= 500;

        public Task StopAsync(InaraStopReason reason)
        {
            lock (stopSync)
                return stopTask ??= stopCoreAsync(reason);
        }

        private async Task stopCoreAsync(InaraStopReason reason)
        {
            Volatile.Write(ref stopping, 1);
            timer?.Dispose();
            timer = null;

            // An entry that passed the initial stopping check must finish before the
            // final queue snapshot and flush are taken.
            lock (ingestionSync) { }

            await sendGate.WaitAsync();
            try
            {
                if (reason == InaraStopReason.Normal)
                {
                    var credentials = session.GetCredentials();
                    if (credentials != null && CanUpload(
                        credentials.ApiKey,
                        session.IsLive,
                        session.IsBeta,
                        mapper.InMulticrew))
                    {
                        var finalEvents = mapper.Process(new JObject
                        {
                            ["timestamp"] = DateTime.UtcNow.ToString("O"),
                            ["event"] = "Shutdown",
                        }, createContext(!Elite.hadManyGameProcs), true);
                        if (finalEvents.Count > 0)
                            queue.Enqueue(credentials, finalEvents);
                    }
                }

                await sendPendingCoreAsync();
            }
            catch (Exception ex)
            {
                try { log($"Inara shutdown did not complete cleanly:\r\n{ex}"); }
                catch { /* shutdown diagnostics must not escape */ }
            }
            finally
            {
                Interlocked.Exchange(ref disposed, 1);
                try { client.Dispose(); }
                catch { /* best-effort cleanup */ }
                sendGate.Release();
            }
        }

        public void Dispose()
        {
            try
            {
                StopAsync(InaraStopReason.Normal).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                try { log($"Inara shutdown did not complete cleanly:\r\n{ex}"); }
                catch { /* disposal must not escape */ }
            }
        }
    }
}
