using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;

// EDDN uploader contract:
// https://github.com/EDCD/EDDN/blob/live/docs/Developers.md
// Message organization is modelled after EDMarketConnector's plugins/eddn.py.

namespace SrvSurvey.net
{
    /// <summary>Coordinates opt-in journal and companion-file publication to EDDN.</summary>
    internal sealed class EDDN
    {
        private static readonly HashSet<string> journalEvents = new(StringComparer.Ordinal)
        {
            "CodexEntry",
            "ApproachSettlement",
            "DockingGranted",
            "DockingDenied",
            "FSSAllBodiesFound",
            "FSSBodySignals",
            "FSSDiscoveryScan",
            "NavBeaconScan",
            "ScanBaryCentre",
            "Docked",
            "FSDJump",
            "CarrierJump",
            "Scan",
            "Location",
            "SAASignalsFound",
        };

        public static UploadPayloadHeader? header;
        private static bool logAllUploads;

        private readonly object sync = new();
        private readonly EddnTransport transport;
        private readonly EddnOutbox outbox;
        private readonly List<JObject> pendingSignals = [];
        private readonly Dictionary<string, string> stationSignatures = new(StringComparer.Ordinal);
        private EddnLocationContext? location;
        private bool isCrewMember;

        internal EDDN()
        {
            transport = new EddnTransport(userAgent: Program.userAgent);
            outbox = new EddnOutbox(
                Path.Combine(Program.dataFolder, "eddn-outbox-v1.json"),
                transport,
                message =>
                {
                    if (logAllUploads || !message.Contains(" uploaded ", StringComparison.Ordinal))
                        Game.log(message);
                });
            outbox.setEnabled(
                Game.settings.eddnUploadEnabled,
                discardPendingWhenDisabled: !Game.settings.eddnUploadEnabled);
        }

        internal void beginSession(Game game)
        {
            ArgumentNullException.ThrowIfNull(game);
            lock (sync)
            {
                pendingSignals.Clear();
                stationSignatures.Clear();
                isCrewMember = false;
                location = game.systemData is { address: > 0, starPos.Length: 3 }
                    ? new EddnLocationContext(
                        game.systemData.name,
                        game.systemData.address,
                        game.systemData.starPos.ToArray())
                    : null;
            }

            outbox.setEnabled(
                Game.settings.eddnUploadEnabled,
                discardPendingWhenDisabled: !Game.settings.eddnUploadEnabled);
        }

        internal void setEnabled(bool enabled)
        {
            lock (sync)
            {
                if (!enabled)
                {
                    pendingSignals.Clear();
                    stationSignatures.Clear();
                }
            }
            outbox.setEnabled(enabled, discardPendingWhenDisabled: !enabled);
        }

        internal void onJournalEntry(Game game, JObject raw)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(raw);

            var eventName = raw.Value<string>("event");
            if (string.IsNullOrWhiteSpace(eventName)) return;

            updateHeader(raw);
            var enabled = Game.settings.eddnUploadEnabled;
            outbox.setEnabled(enabled, discardPendingWhenDisabled: false);

            EddnPreparedMessage? signalBatch = null;
            UploadPayloadHeader? batchHeader = null;
            string? batchReason = null;
            EddnMessageContext context;
            bool suppressForCrew;
            bool suppressBatchForCrew;
            lock (sync)
            {
                var eventLocation = EddnMessageSanitizer.getLocation(raw);
                suppressBatchForCrew = isCrewMember;
                if (eventName != "FSSSignalDiscovered" && pendingSignals.Count > 0)
                {
                    var batchLocation = eventLocation ?? location;
                    EddnMessageSanitizer.tryBuildSignalBatch(
                        pendingSignals,
                        batchLocation,
                        game.journals?.isGameHorizons,
                        game.journals?.isGameOdyssey,
                        out signalBatch,
                        out batchReason!);
                    pendingSignals.Clear();
                    batchHeader = header?.clone();
                }

                if (eventLocation != null) location = eventLocation;
                if (eventName == "JoinACrew") isCrewMember = true;
                if (eventName is "QuitACrew" or "LoadGame") isCrewMember = false;
                suppressForCrew = isCrewMember;

                context = createContext(game);
                if (eventName == "FSSSignalDiscovered")
                {
                    if (enabled && !suppressForCrew && header != null)
                        pendingSignals.Add(new JObject(raw));
                    return;
                }
            }

            if (signalBatch != null && batchHeader != null && enabled && !suppressBatchForCrew)
                enqueue(signalBatch, batchHeader, Game.settings.eddnEnvironment);
            else if (batchReason != null && batchReason != "no public signals remained after filtering")
                Game.log($"EDDN skipped FSSSignalDiscovered batch: {batchReason}");

            var uploadHeader = header?.clone();
            if (!enabled || uploadHeader == null || suppressForCrew) return;

            if (EddnMessageSanitizer.isCompanionEvent(eventName))
            {
                processCompanionFile(
                    new JObject(raw),
                    context,
                    uploadHeader,
                    Game.settings.eddnEnvironment).justDoIt();
                return;
            }

            if (!journalEvents.Contains(eventName)) return;
            if (EddnMessageSanitizer.tryBuildJournal(
                raw,
                context,
                out var prepared,
                out var reason))
            {
                enqueue(prepared!, uploadHeader, Game.settings.eddnEnvironment);
            }
            else
            {
                Game.log($"EDDN skipped {eventName}: {reason}");
            }
        }

        private async Task processCompanionFile(
            JObject journalEvent,
            EddnMessageContext context,
            UploadPayloadHeader uploadHeader,
            string? environment)
        {
            var eventName = journalEvent.Value<string>("event") ?? "companion file";
            try
            {
                var read = await EddnCompanionFileReader.read(
                    Game.settings.watchedJournalFolder,
                    journalEvent);
                if (!read.isSuccess)
                {
                    Game.log($"EDDN skipped {eventName}: {read.error}");
                    return;
                }

                if (!Game.settings.eddnUploadEnabled) return;
                if (!EddnMessageSanitizer.tryBuildCompanion(
                    read.content!,
                    context,
                    out var prepared,
                    out var reason))
                {
                    Game.log($"EDDN skipped {eventName}: {reason}");
                    return;
                }

                if (isDuplicateStationMessage(prepared!))
                {
                    if (logAllUploads) Game.log($"EDDN skipped unchanged {eventName} data.");
                    return;
                }

                enqueue(prepared!, uploadHeader, environment);
            }
            catch (OperationCanceledException)
            {
                Game.log($"EDDN stopped reading {eventName}.json.");
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                Game.log($"EDDN skipped {eventName}: {ex.Message}");
            }
        }

        private bool isDuplicateStationMessage(EddnPreparedMessage prepared)
        {
            if (prepared.eventName == "NavRoute") return false;

            var marketId = prepared.message.Value<long?>("marketId")
                ?? prepared.message.Value<long?>("MarketID")
                ?? 0;
            var key = prepared.schemaRef + ":" + marketId;
            var comparable = new JObject(prepared.message);
            comparable.Remove("timestamp");
            var signature = comparable.ToString(Formatting.None);
            lock (sync)
            {
                if (stationSignatures.GetValueOrDefault(key) == signature) return true;
                stationSignatures[key] = signature;
                return false;
            }
        }

        private EddnMessageContext createContext(Game game)
        {
            return new EddnMessageContext(
                location,
                game.journals?.isGameHorizons,
                game.journals?.isGameOdyssey,
                game.status?.BodyName,
                game.systemBody?.name,
                game.systemBody?.id,
                game.systemBody?.type is SystemBodyType.Giant
                    or SystemBodyType.SolidBody
                    or SystemBodyType.LandableBody
                    ? "Planet"
                    : null);
        }

        private void enqueue(
            EddnPreparedMessage prepared,
            UploadPayloadHeader uploadHeader,
            string? environment)
        {
            var queued = transport.prepare(
                prepared.message,
                prepared.schemaRef,
                uploadHeader,
                environment);
            if (!outbox.enqueue(queued) && Game.settings.eddnUploadEnabled)
                Game.log($"EDDN could not queue {prepared.eventName} for upload.");
        }

        private static void updateHeader(JObject raw)
        {
            var eventName = raw.Value<string>("event");
            if (eventName == "Fileheader" && header != null)
            {
                header.gameversion = raw.Value<string>("gameversion") ?? string.Empty;
                header.gamebuild = raw.Value<string>("build") ?? string.Empty;
            }
            else if (eventName == "LoadGame")
            {
                var commander = raw.Value<string>("Commander");
                if (!string.IsNullOrWhiteSpace(commander))
                {
                    header = new UploadPayloadHeader(
                        commander,
                        header?.gameversion ?? raw.Value<string>("gameversion"),
                        header?.gamebuild ?? raw.Value<string>("build"),
                        Program.releaseVersion);
                }
            }
        }
    }
}
