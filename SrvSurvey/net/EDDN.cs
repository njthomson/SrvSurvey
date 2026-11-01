using SrvSurvey.game;

namespace SrvSurvey.net
{
    /// <summary>
    /// Application-lifetime EDDN delivery service. Commander and journal state
    /// belong to <see cref="EddnSessionPublisher"/>, while this service owns the
    /// one durable outbox and its network safety checks.
    /// </summary>
    internal sealed class EDDN : IEddnSessionSink, IDisposable
    {
        private readonly object sync = new();
        private readonly EddnTransport transport;
        private readonly EddnOutbox outbox;
        private bool publishingSuspended;
        private bool processDetectionWarningReported;
        private bool consentReadWarningReported;
        private bool disposed;
        private long ingestionGeneration;

        internal EDDN()
        {
            transport = new EddnTransport(userAgent: Program.userAgent);
            outbox = new EddnOutbox(
                Path.Combine(Program.dataFolder, "eddn-outbox-v1.json"),
                transport,
                Game.log,
                runtimeUploadAllowed: isRuntimeUploadAllowed);
            outbox.setEnabled(
                Game.settings.eddnUploadEnabled,
                discardPendingWhenDisabled: !Game.settings.eddnUploadEnabled);
        }

        internal int pendingCount => outbox.pendingCount;

        internal void setEnabled(bool enabled)
        {
            lock (sync)
            {
                if (disposed) return;
                if (!enabled || enabled != Game.settings.eddnUploadEnabled)
                    ingestionGeneration++;
            }

            var runtimePublishingAllowed = enabled;
            if (enabled)
            {
                refreshRuntimeSafety();
                lock (sync)
                    runtimePublishingAllowed = !disposed
                        && !publishingSuspended
                        && Game.settings.eddnUploadEnabled;
            }

            outbox.setEnabled(
                enabled,
                discardPendingWhenDisabled: !enabled);
            if (enabled)
                outbox.setSuspended(!runtimePublishingAllowed);
        }

        internal void refreshRuntimeSafety(bool? hasMultipleEliteProcesses = null)
        {
            lock (sync)
            {
                if (disposed || !Game.settings.eddnUploadEnabled) return;
            }

            if (!EddnConsentFile.tryRead(
                Path.Combine(Program.dataFolder, "settings.json"),
                out var persistedEnabled,
                out var consentError))
            {
                bool shouldLog;
                lock (sync)
                {
                    shouldLog = !consentReadWarningReported;
                    consentReadWarningReported = true;
                }

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

            refreshGameProcessSafety(hasMultipleEliteProcesses);
        }

        internal bool setSuspended(bool suspended, string? pauseMessage = null)
        {
            lock (sync)
            {
                if (disposed || publishingSuspended == suspended) return false;
                publishingSuspended = suspended;
                ingestionGeneration++;
            }

            outbox.setSuspended(suspended);
            Game.log(suspended
                ? pauseMessage
                    ?? "EDDN sharing is paused while multiple Elite instances are active; pending uploads were preserved."
                : "EDDN sharing resumed after runtime attribution and consent checks passed.");
            return true;
        }

        bool IEddnSessionSink.tryBeginIngestion(out long generation)
        {
            lock (sync)
            {
                generation = ingestionGeneration;
                return !disposed
                    && !publishingSuspended
                    && Game.settings.eddnUploadEnabled;
            }
        }

        bool IEddnSessionSink.tryEnqueue(
            EddnPreparedMessage prepared,
            UploadPayloadHeader header,
            long expectedGeneration)
        {
            ArgumentNullException.ThrowIfNull(prepared);
            ArgumentNullException.ThrowIfNull(header);

            lock (sync)
            {
                if (disposed
                    || publishingSuspended
                    || !Game.settings.eddnUploadEnabled
                    || ingestionGeneration != expectedGeneration)
                {
                    return false;
                }
            }

            var queued = transport.prepare(
                prepared.message,
                prepared.schemaRef,
                header);
            return outbox.enqueue(queued);
        }

        private void refreshGameProcessSafety(bool? hasMultipleEliteProcesses)
        {
            try
            {
                var suspended = hasMultipleEliteProcesses ?? Elite.refreshManyGameProcs();
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
                if (disposed
                    || publishingSuspended
                    || !Game.settings.eddnUploadEnabled)
                {
                    return false;
                }
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

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                ingestionGeneration++;
            }

            outbox.Dispose();
            transport.Dispose();
        }
    }

    /// <summary>Small boundary consumed by a single Game-owned EDDN session.</summary>
    internal interface IEddnSessionSink
    {
        bool tryBeginIngestion(out long generation);

        bool tryEnqueue(
            EddnPreparedMessage prepared,
            UploadPayloadHeader header,
            long expectedGeneration);
    }
}
