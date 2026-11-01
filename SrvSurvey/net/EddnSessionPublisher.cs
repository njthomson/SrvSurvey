using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SrvSurvey.net
{
    // EDDN contract: https://github.com/EDCD/EDDN/blob/live/docs/Developers.md
    // Event selection and signal batching follow the proven EDMC integration:
    // https://github.com/EDCD/EDMarketConnector/blob/main/plugins/eddn.py
    /// <summary>
    /// Publishes one immutable Commander session. This object is owned and
    /// disposed by the corresponding Game instance.
    /// </summary>
    internal sealed class EddnSessionPublisher : IDisposable
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

        private readonly object sync = new();
        private readonly object enqueueSync = new();
        private readonly IEddnSessionSink sink;
        private readonly UploadPayloadHeader header;
        private readonly string journalFolder;
        private readonly Action<string> log;
        private readonly Func<string, JObject, CancellationToken, Task<EddnCompanionReadResult>> companionReader;
        private readonly CancellationTokenSource disposal = new();
        private readonly List<JObject> pendingSignals = [];
        private readonly Dictionary<string, string> stationSignatures = new(StringComparer.Ordinal);
        private EddnSignalBatchContext? pendingSignalContext;
        private EddnLocationContext? location;
        private bool isCrewMember;
        private bool accepting = true;
        private bool disposed;

        internal EddnSessionPublisher(
            IEddnSessionSink sink,
            UploadPayloadHeader header,
            string journalFolder,
            EddnLocationContext? initialLocation,
            Action<string>? log = null,
            Func<string, JObject, CancellationToken, Task<EddnCompanionReadResult>>? companionReader = null)
        {
            ArgumentNullException.ThrowIfNull(sink);
            ArgumentNullException.ThrowIfNull(header);
            ArgumentException.ThrowIfNullOrWhiteSpace(journalFolder);
            if (string.IsNullOrWhiteSpace(header.uploaderID))
                throw new ArgumentException("An EDDN session requires a Commander name.", nameof(header));

            this.sink = sink;
            this.header = header.clone();
            this.journalFolder = journalFolder;
            location = initialLocation;
            this.log = log ?? (_ => { });
            this.companionReader = companionReader
                ?? ((folder, journalEvent, cancellationToken) =>
                    EddnCompanionFileReader.read(
                        folder,
                        journalEvent,
                        cancellationToken));
        }

        internal string commander => header.uploaderID;

        internal void onJournalEntry(JObject raw, EddnMessageContext gameContext)
        {
            ArgumentNullException.ThrowIfNull(raw);
            ArgumentNullException.ThrowIfNull(gameContext);

            var eventName = raw.Value<string>("event");
            if (string.IsNullOrWhiteSpace(eventName)) return;

            if (eventName == "LoadGame")
            {
                var eventCommander = raw.Value<string>("Commander");
                if (!string.IsNullOrWhiteSpace(eventCommander)
                    && !eventCommander.Equals(header.uploaderID, StringComparison.OrdinalIgnoreCase))
                {
                    stopForCommanderChange(eventCommander);
                    return;
                }
            }

            SignalBatch? batch = null;
            EddnMessageContext context;
            bool suppressForCrew;
            CancellationToken sessionToken;
            lock (sync)
            {
                if (disposed || !accepting) return;

                if (eventName != "FSSSignalDiscovered" && pendingSignals.Count > 0)
                    batch = takeSignalBatchLocked();

                var eventLocation = EddnMessageSanitizer.getLocation(raw);
                if (eventLocation != null) location = eventLocation;

                if (eventName == "JoinACrew") isCrewMember = true;
                if (eventName is "QuitACrew" or "LoadGame") isCrewMember = false;
                suppressForCrew = isCrewMember;
                context = gameContext with { location = location };
                sessionToken = disposal.Token;
            }

            if (batch != null) publishSignalBatch(batch, sessionToken);

            if (eventName == "FSSSignalDiscovered")
            {
                collectSignal(raw, gameContext, suppressForCrew);
                return;
            }

            if (suppressForCrew) return;
            if (!journalEvents.Contains(eventName)
                && !EddnMessageSanitizer.isCompanionEvent(eventName))
            {
                return;
            }

            if (!sink.tryBeginIngestion(out var generation)) return;

            if (EddnMessageSanitizer.isCompanionEvent(eventName))
            {
                processCompanionFile(
                    new JObject(raw),
                    context,
                    generation,
                    sessionToken).justDoIt();
                return;
            }

            if (EddnMessageSanitizer.tryBuildJournal(
                raw,
                context,
                out var prepared,
                out var reason))
            {
                if (!tryEnqueue(prepared!, generation, sessionToken))
                    log($"EDDN could not queue {prepared!.eventName} for upload.");
            }
            else
            {
                log($"EDDN skipped {eventName}: {reason}");
            }
        }

        private void collectSignal(
            JObject raw,
            EddnMessageContext gameContext,
            bool suppressForCrew)
        {
            if (suppressForCrew || !sink.tryBeginIngestion(out var generation))
            {
                clearSignals();
                return;
            }

            lock (sync)
            {
                if (disposed || !accepting) return;
                pendingSignalContext ??= new EddnSignalBatchContext(
                    location,
                    gameContext.horizons,
                    gameContext.odyssey,
                    generation);

                if (pendingSignalContext.generation != generation)
                {
                    pendingSignals.Clear();
                    pendingSignalContext = new EddnSignalBatchContext(
                        location,
                        gameContext.horizons,
                        gameContext.odyssey,
                        generation);
                }

                pendingSignals.Add(new JObject(raw));
            }
        }

        private void publishSignalBatch(
            SignalBatch batch,
            CancellationToken cancellationToken)
        {
            if (!EddnMessageSanitizer.tryBuildSignalBatch(
                batch.signals,
                batch.context.location,
                batch.context.isHorizons,
                batch.context.isOdyssey,
                out var prepared,
                out var reason))
            {
                if (reason != "no public signals remained after filtering")
                    log($"EDDN skipped FSSSignalDiscovered batch: {reason}");
                return;
            }

            if (!tryEnqueue(
                prepared!,
                batch.context.generation,
                cancellationToken))
                log("EDDN could not queue FSSSignalDiscovered for upload.");
        }

        private async Task processCompanionFile(
            JObject journalEvent,
            EddnMessageContext context,
            long generation,
            CancellationToken cancellationToken)
        {
            var eventName = journalEvent.Value<string>("event") ?? "companion file";
            try
            {
                var read = await companionReader(
                    journalFolder,
                    journalEvent,
                    cancellationToken).ConfigureAwait(false);
                if (!read.isSuccess)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        log($"EDDN skipped {eventName}: {read.error}");
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (!EddnMessageSanitizer.tryBuildCompanion(
                    read.content!,
                    context,
                    out var prepared,
                    out var reason))
                {
                    log($"EDDN skipped {eventName}: {reason}");
                    return;
                }

                var signature = getCompanionSignature(prepared!);
                if (signature != null && !reserveSignature(signature.Value)) return;

                var queued = tryEnqueue(
                    prepared!,
                    generation,
                    cancellationToken);
                if (!queued && signature != null) releaseSignature(signature.Value);
                if (!queued) log($"EDDN could not queue {eventName} for upload.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The owning Game ended; stale companion data must not cross sessions.
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                    log($"EDDN skipped {eventName}: {ex.Message}");
            }
        }

        private static (string key, string value)? getCompanionSignature(
            EddnPreparedMessage prepared)
        {
            if (prepared.eventName == "NavRoute") return null;
            var marketId = prepared.message.Value<long?>("marketId")
                ?? prepared.message.Value<long?>("MarketID")
                ?? 0;
            var comparable = new JObject(prepared.message);
            comparable.Remove("timestamp");
            return (
                prepared.schemaRef + ":" + marketId,
                comparable.ToString(Formatting.None));
        }

        private bool reserveSignature((string key, string value) signature)
        {
            lock (sync)
            {
                if (disposed || !accepting) return false;
                if (stationSignatures.GetValueOrDefault(signature.key) == signature.value)
                    return false;
                stationSignatures[signature.key] = signature.value;
                return true;
            }
        }

        private void releaseSignature((string key, string value) signature)
        {
            lock (sync)
            {
                if (stationSignatures.GetValueOrDefault(signature.key) == signature.value)
                    stationSignatures.Remove(signature.key);
            }
        }

        private bool tryEnqueue(
            EddnPreparedMessage prepared,
            long generation,
            CancellationToken cancellationToken,
            bool allowDisposedBatch = false)
        {
            lock (enqueueSync)
            {
                if (!allowDisposedBatch && cancellationToken.IsCancellationRequested)
                    return false;
                lock (sync)
                {
                    if (!allowDisposedBatch && (disposed || !accepting))
                        return false;
                }

                return sink.tryEnqueue(prepared, header, generation);
            }
        }

        private SignalBatch? takeSignalBatchLocked()
        {
            if (pendingSignals.Count == 0 || pendingSignalContext == null) return null;
            var batch = new SignalBatch(
                pendingSignals.Select(signal => new JObject(signal)).ToArray(),
                pendingSignalContext);
            pendingSignals.Clear();
            pendingSignalContext = null;
            return batch;
        }

        private void clearSignals()
        {
            lock (sync)
            {
                pendingSignals.Clear();
                pendingSignalContext = null;
            }
        }

        private void stopForCommanderChange(string eventCommander)
        {
            lock (enqueueSync)
            {
                lock (sync)
                {
                    if (disposed || !accepting) return;
                    accepting = false;
                    pendingSignals.Clear();
                    pendingSignalContext = null;
                    stationSignatures.Clear();
                }

                disposal.Cancel();
            }

            log(
                $"EDDN stopped session '{header.uploaderID}' after LoadGame identified Commander '{eventCommander}'; "
                + "a new Game session must capture the new Commander before uploads resume.");
        }

        public void Dispose()
        {
            SignalBatch? batch;
            lock (enqueueSync)
            {
                lock (sync)
                {
                    if (disposed) return;
                    disposed = true;
                    accepting = false;
                    batch = takeSignalBatchLocked();
                    stationSignatures.Clear();
                }

                disposal.Cancel();
            }

            if (batch != null)
            {
                if (EddnMessageSanitizer.tryBuildSignalBatch(
                    batch.signals,
                    batch.context.location,
                    batch.context.isHorizons,
                    batch.context.isOdyssey,
                    out var prepared,
                    out _))
                {
                    tryEnqueue(
                        prepared!,
                        batch.context.generation,
                        CancellationToken.None,
                        allowDisposedBatch: true);
                }
            }
            disposal.Dispose();
        }

        private sealed record EddnSignalBatchContext(
            EddnLocationContext? location,
            bool? isHorizons,
            bool? isOdyssey,
            long generation);

        private sealed record SignalBatch(
            IReadOnlyList<JObject> signals,
            EddnSignalBatchContext context);
    }
}
