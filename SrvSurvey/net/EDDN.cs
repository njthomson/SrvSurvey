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

        private static bool logAllUploads;

        private readonly object sync = new();
        private readonly EddnTransport transport;
        private readonly EddnOutbox outbox;
        private readonly List<JObject> pendingSignals = [];
        private readonly Dictionary<string, string> stationSignatures = new(StringComparer.Ordinal);
        private Game? currentGame;
        private UploadPayloadHeader? header;
        private EddnLocationContext? location;
        private bool isCrewMember;
        private bool publishingSuspended;
        private bool processDetectionWarningReported;
        private bool consentReadWarningReported;
        private long sessionGeneration;

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
                },
                runtimeUploadAllowed: isRuntimeUploadAllowed);
            outbox.setEnabled(
                Game.settings.eddnUploadEnabled,
                discardPendingWhenDisabled: !Game.settings.eddnUploadEnabled);
        }

        internal void beginSession(Game game, UploadPayloadHeader? sessionHeader)
        {
            ArgumentNullException.ThrowIfNull(game);
            lock (sync)
            {
                sessionGeneration++;
                currentGame = game;
                header = sessionHeader?.clone();
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

            if (Game.settings.eddnUploadEnabled)
                refreshRuntimeSafety();
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
                    sessionGeneration++;
                    pendingSignals.Clear();
                    stationSignatures.Clear();
                }
            }
            if (enabled)
            {
                refreshRuntimeSafety();
                enabled = Game.settings.eddnUploadEnabled;
            }
            outbox.setEnabled(enabled, discardPendingWhenDisabled: !enabled);
        }

        internal void refreshRuntimeSafety()
        {
            if (!Game.settings.eddnUploadEnabled) return;

            var settingsPath = Path.Combine(Program.dataFolder, "settings.json");
            if (!EddnConsentFile.tryRead(
                settingsPath,
                out var persistedEnabled,
                out var consentError))
            {
                bool shouldLog;
                lock (sync)
                {
                    shouldLog = !consentReadWarningReported;
                    consentReadWarningReported = true;
                }

                // A transient settings read must not destroy the queue, but it
                // also must not allow uploads without confirmed current consent.
                var changed = setSuspended(
                    true,
                    "EDDN sharing is paused because current consent could not be read; pending uploads were preserved.");
                if (shouldLog && !changed)
                    Game.log($"EDDN sharing is paused because current consent could not be read: {consentError}");
                return;
            }

            lock (sync) consentReadWarningReported = false;
            if (!persistedEnabled)
            {
                Game.settings.eddnUploadEnabled = false;
                setEnabled(false);
                Game.log("EDDN sharing was disabled by another SrvSurvey instance; pending uploads were discarded.");
                return;
            }

            refreshGameProcessSafety();
        }

        private void refreshGameProcessSafety()
        {
            try
            {
                var suspended = Elite.refreshManyGameProcs();
                lock (sync) processDetectionWarningReported = false;
                setSuspended(suspended);
            }
            catch (Exception ex)
            {
                bool shouldLog;
                lock (sync)
                {
                    shouldLog = !processDetectionWarningReported;
                    processDetectionWarningReported = true;
                }

                // Fail closed: if process attribution cannot be checked, neither
                // shared Status.json nor companion files are safe to publish.
                var changed = setSuspended(
                    true,
                    "EDDN sharing is paused because running Elite instances could not be checked; pending uploads were preserved.");
                if (shouldLog && !changed)
                    Game.log($"EDDN sharing is paused because running Elite instances could not be checked: {ex.Message}");
            }
        }

        private bool isRuntimeUploadAllowed()
        {
            lock (sync)
            {
                if (publishingSuspended || !Game.settings.eddnUploadEnabled)
                    return false;
            }

            if (!EddnConsentFile.tryRead(
                Path.Combine(Program.dataFolder, "settings.json"),
                out var persistedEnabled,
                out _)
                || !persistedEnabled)
            {
                return false;
            }

            try
            {
                return !Elite.refreshManyGameProcs();
            }
            catch
            {
                return false;
            }
        }

        internal bool setSuspended(bool suspended, string? pauseMessage = null)
        {
            bool changed;
            lock (sync)
            {
                changed = publishingSuspended != suspended;
                if (!changed) return false;
                publishingSuspended = suspended;
                if (suspended)
                {
                    sessionGeneration++;
                    pendingSignals.Clear();
                    stationSignatures.Clear();
                }
            }

            // Suspension is operational rather than consent. Keep the durable
            // queue intact, but cancel any active request and block new work.
            outbox.setSuspended(suspended);
            Game.log(suspended
                ? pauseMessage
                    ?? "EDDN sharing is paused while multiple Elite instances are active; pending uploads were preserved."
                : "EDDN sharing resumed after runtime attribution and consent checks passed.");
            return true;
        }

        internal void endSession(Game game)
        {
            ArgumentNullException.ThrowIfNull(game);
            lock (sync)
            {
                if (!ReferenceEquals(currentGame, game)) return;
                sessionGeneration++;
                currentGame = null;
                header = null;
                location = null;
                isCrewMember = false;
                pendingSignals.Clear();
                stationSignatures.Clear();
            }
        }

        internal void onJournalEntry(Game game, JObject raw)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(raw);

            var eventName = raw.Value<string>("event");
            if (string.IsNullOrWhiteSpace(eventName)) return;

            if (Game.settings.eddnUploadEnabled) refreshRuntimeSafety();
            var enabled = Game.settings.eddnUploadEnabled;
            outbox.setEnabled(enabled, discardPendingWhenDisabled: false);

            EddnPreparedMessage? signalBatch = null;
            UploadPayloadHeader? batchHeader = null;
            string? batchReason = null;
            EddnMessageContext context;
            bool suppressForCrew;
            bool suppressBatchForCrew;
            UploadPayloadHeader? uploadHeader;
            long generation;
            bool suspended;
            lock (sync)
            {
                if (!ReferenceEquals(currentGame, game)) return;
                generation = sessionGeneration;
                suspended = publishingSuspended;
                updateHeader(raw);
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
                uploadHeader = header?.clone();
                if (eventName == "FSSSignalDiscovered")
                {
                    if (enabled && !suspended && !suppressForCrew && header != null)
                        pendingSignals.Add(new JObject(raw));
                    return;
                }
            }

            if (signalBatch != null
                && batchHeader != null
                && enabled
                && !suspended
                && !suppressBatchForCrew)
                enqueueForSession(
                    signalBatch,
                    batchHeader,
                    Game.settings.eddnUseTestSchemas,
                    generation);
            else if (batchReason != null && batchReason != "no public signals remained after filtering")
                Game.log($"EDDN skipped FSSSignalDiscovered batch: {batchReason}");

            if (!enabled || suspended || uploadHeader == null || suppressForCrew) return;

            if (EddnMessageSanitizer.isCompanionEvent(eventName))
            {
                processCompanionFile(
                    new JObject(raw),
                    context,
                    uploadHeader,
                    Game.settings.eddnUseTestSchemas,
                    generation).justDoIt();
                return;
            }

            if (!journalEvents.Contains(eventName)) return;
            if (EddnMessageSanitizer.tryBuildJournal(
                raw,
                context,
                out var prepared,
                out var reason))
            {
                enqueueForSession(
                    prepared!,
                    uploadHeader,
                    Game.settings.eddnUseTestSchemas,
                    generation);
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
            bool useTestSchemas,
            long generation)
        {
            var eventName = journalEvent.Value<string>("event") ?? "companion file";
            try
            {
                var read = await EddnCompanionFileReader.read(
                    Game.settings.watchedJournalFolder,
                    journalEvent).ConfigureAwait(false);
                if (!read.isSuccess)
                {
                    if (isCurrentSession(generation))
                        Game.log($"EDDN skipped {eventName}: {read.error}");
                    return;
                }

                if (!isCurrentSession(generation)) return;
                if (!EddnMessageSanitizer.tryBuildCompanion(
                    read.content!,
                    context,
                    out var prepared,
                    out var reason))
                {
                    Game.log($"EDDN skipped {eventName}: {reason}");
                    return;
                }

                var queueResult = enqueueCompanionForSession(
                    prepared!,
                    uploadHeader,
                    useTestSchemas,
                    generation);
                if (queueResult == CompanionQueueResult.Duplicate)
                {
                    if (logAllUploads) Game.log($"EDDN skipped unchanged {eventName} data.");
                }
                else if (queueResult == CompanionQueueResult.Failed)
                    Game.log($"EDDN could not queue {eventName} for upload.");
            }
            catch (OperationCanceledException)
            {
                if (isCurrentSession(generation))
                    Game.log($"EDDN stopped reading {eventName}.json.");
            }
            catch (Exception ex)
            {
                if (isCurrentSession(generation))
                    Game.log($"EDDN skipped {eventName}: {ex.Message}");
            }
        }

        private CompanionQueueResult enqueueCompanionForSession(
            EddnPreparedMessage prepared,
            UploadPayloadHeader uploadHeader,
            bool useTestSchemas,
            long generation)
        {
            lock (sync)
            {
                if (!isCurrentSessionLocked(generation)) return CompanionQueueResult.Stale;
                if (prepared.eventName == "NavRoute")
                    return enqueueLocked(prepared, uploadHeader, useTestSchemas)
                        ? CompanionQueueResult.Queued
                        : CompanionQueueResult.Failed;

                var marketId = prepared.message.Value<long?>("marketId")
                    ?? prepared.message.Value<long?>("MarketID")
                    ?? 0;
                var key = prepared.schemaRef + ":" + marketId;
                var comparable = new JObject(prepared.message);
                comparable.Remove("timestamp");
                var signature = comparable.ToString(Formatting.None);
                if (stationSignatures.GetValueOrDefault(key) == signature)
                    return CompanionQueueResult.Duplicate;
                if (!enqueueLocked(prepared, uploadHeader, useTestSchemas))
                    return CompanionQueueResult.Failed;

                stationSignatures[key] = signature;
                return CompanionQueueResult.Queued;
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

        private void enqueueForSession(
            EddnPreparedMessage prepared,
            UploadPayloadHeader uploadHeader,
            bool useTestSchemas,
            long generation)
        {
            bool active;
            bool queued;
            lock (sync)
            {
                active = isCurrentSessionLocked(generation);
                queued = active && enqueueLocked(prepared, uploadHeader, useTestSchemas);
            }

            if (active && !queued)
                Game.log($"EDDN could not queue {prepared.eventName} for upload.");
        }

        private bool enqueueLocked(
            EddnPreparedMessage prepared,
            UploadPayloadHeader uploadHeader,
            bool useTestSchemas)
        {
            var queued = transport.prepare(
                prepared.message,
                prepared.schemaRef,
                uploadHeader,
                useTestSchemas);
            return outbox.enqueue(queued);
        }

        private bool isCurrentSession(long generation)
        {
            lock (sync) return isCurrentSessionLocked(generation);
        }

        private bool isCurrentSessionLocked(long generation)
        {
            return currentGame != null
                && sessionGeneration == generation
                && !publishingSuspended
                && Game.settings.eddnUploadEnabled;
        }

        private void updateHeader(JObject raw)
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

        private enum CompanionQueueResult
        {
            Stale,
            Duplicate,
            Queued,
            Failed,
        }
    }
}
