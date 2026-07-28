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
        private readonly HttpClient client;
        private readonly InaraEventMapper mapper = new();
        private readonly InaraEventQueue queue = new();
        private readonly System.Threading.Timer timer;
        private Game? currentGame;
        private int sending;

        public Inara()
        {
            client = new HttpClient(Util.getResilienceHandler())
            {
                Timeout = TimeSpan.FromSeconds(20),
            };
            client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
            timer = new System.Threading.Timer(_ => sendPendingAsync().justDoIt(), null, sendInterval, sendInterval);
        }

        public void Dispose()
        {
            timer.Dispose();
            client.Dispose();
        }

        public void onJournalEntry(Game game, JObject raw)
        {
            // Manual calls made while Game reconstructs state from journal history must never upload.
            if (!Game.ready || Game.activeGame != game) return;

            if (!ReferenceEquals(currentGame, game))
            {
                mapper.Reset();
                currentGame = game;
            }

            raw = addSidecarData(game, raw);

            var credentials = getCredentials(game);
            var canCollect = CanUpload(
                Game.settings.inaraUpload,
                credentials?.ApiKey,
                IsLiveVersion(getGameVersion(game), game.journals?.isOdyssey == true),
                IsBetaVersion(getGameVersion(game)),
                mapper.InMulticrew);

            var context = new InaraContext(
                game.Commander,
                game.fid,
                game.systemData?.name ?? game.cmdr?.currentSystem,
                game.systemStation?.name ?? game.lastDocked?.StationName,
                game.systemBody?.name,
                game.currentShip?.type,
                game.currentShip?.id,
                game.currentShip?.name,
                game.currentShip?.ident,
                game.status?.InTaxi == true);

            var events = mapper.Process(raw, context, canCollect);
            if (credentials != null && events.Count > 0)
            {
                queue.Enqueue(credentials, events);
                Game.log($"Inara queued {events.Count} event(s): {string.Join(", ", events.Select(e => e.Name).Distinct())}");
            }

            if (raw.Value<string>("event") == "Shutdown")
                sendPendingAsync().justDoIt();
        }

        internal static bool CanUpload(bool optedIn, string? apiKey, bool isLive, bool isBeta, bool inMulticrew) =>
            optedIn
            && !string.IsNullOrWhiteSpace(apiKey)
            && isLive
            && !isBeta
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

        private static JObject addSidecarData(Game game, JObject raw)
        {
            var eventName = raw.Value<string>("event");
            if (eventName == "Cargo"
                && raw.Value<string>("Vessel") == "Ship"
                && raw["Inventory"] is not JArray)
            {
                var augmented = (JObject)raw.DeepClone();
                augmented["Inventory"] = JArray.FromObject(game.cargoFile.Inventory ?? []);
                return augmented;
            }

            if (eventName == "ShipLocker"
                && new[] { "Items", "Components", "Data", "Consumables" }.Any(type => raw[type] is not JArray))
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
                        var payload = InaraPayloadBuilder.Build(Program.releaseVersion, group.Key, batch.Select(item => item.Event).ToList());
                        using var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                        using var response = await client.PostAsync(Endpoint, content);

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
