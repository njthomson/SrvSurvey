using Newtonsoft.Json;

namespace SrvSurvey.net
{
    /// <summary>
    /// A durable, ordered EDDN replay queue modelled after EDMC's sender queue.
    /// Messages are persisted before the first network attempt and transient
    /// failures wait at least one minute before another attempt.
    /// </summary>
    internal sealed class EddnOutbox : IDisposable
    {
        private static readonly TimeSpan startupDelay = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan sendSpacing = TimeSpan.FromMilliseconds(400);
        private static readonly TimeSpan minimumRetryDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan maximumRetryDelay = TimeSpan.FromMinutes(30);

        private readonly string filepath;
        private readonly EddnTransport transport;
        private readonly Action<string> log;
        private readonly Func<DateTimeOffset> utcNow;
        private readonly bool automaticProcessing;
        private readonly object sync = new();
        private readonly SemaphoreSlim processing = new(1, 1);
        private readonly System.Threading.Timer timer;
        private readonly CancellationTokenSource shutdown = new();
        private List<EddnQueuedMessage> pending;
        private bool enabled;
        private volatile bool disposed;

        internal EddnOutbox(
            string filepath,
            EddnTransport transport,
            Action<string>? log = null,
            Func<DateTimeOffset>? utcNow = null,
            bool automaticProcessing = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
            ArgumentNullException.ThrowIfNull(transport);
            this.filepath = filepath;
            this.transport = transport;
            this.log = log ?? (_ => { });
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
            this.automaticProcessing = automaticProcessing;
            pending = load();
            timer = new System.Threading.Timer(
                _ => triggerProcessing(),
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
        }

        internal int pendingCount
        {
            get
            {
                lock (sync) return pending.Count;
            }
        }

        internal void setEnabled(bool value, bool discardPendingWhenDisabled)
        {
            var changed = false;
            string? persistenceLog = null;
            string? sharingLog = null;
            lock (sync)
            {
                changed = enabled != value;
                enabled = value;
                if (!enabled && discardPendingWhenDisabled && pending.Count > 0)
                {
                    var count = pending.Count;
                    pending.Clear();
                    persistenceLog = deleteStore();
                    sharingLog = $"EDDN discarded {count:N0} pending upload(s) because sharing was disabled.";
                }
            }

            writeLog(persistenceLog);
            writeLog(sharingLog);

            if (value && changed && automaticProcessing)
                schedule(startupDelay);
            else if (!value)
                stopTimer();
        }

        internal bool enqueue(EddnQueuedMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            string? persistenceLog = null;
            var queued = false;
            lock (sync)
            {
                if (!enabled || disposed) return false;
                pending.Add(message);
                if (!save(out persistenceLog))
                {
                    pending.Remove(message);
                }
                else
                    queued = true;
            }

            writeLog(persistenceLog);
            if (!queued) return false;

            if (automaticProcessing) schedule(TimeSpan.Zero);
            return true;
        }

        internal async Task processDue(CancellationToken cancellationToken = default)
        {
            if (!await processing.WaitAsync(0, cancellationToken).ConfigureAwait(false)) return;
            try
            {
                while (true)
                {
                    EddnQueuedMessage? next;
                    lock (sync)
                    {
                        if (!enabled || disposed) return;
                        var now = utcNow();
                        next = pending
                            .Where(item => item.nextAttempt <= now)
                            .OrderBy(item => item.created)
                            .FirstOrDefault();
                        if (next == null)
                        {
                            scheduleNextLocked(now);
                            return;
                        }
                    }

                    EddnUploadResult? result = null;
                    Exception? failure = null;
                    try
                    {
                        using var combined = CancellationTokenSource.CreateLinkedTokenSource(
                            shutdown.Token,
                            cancellationToken);
                        result = await transport.upload(next, combined.Token).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is HttpRequestException or IOException or OperationCanceledException)
                    {
                        failure = ex;
                    }

                    var retry = failure != null || result?.isRetryable == true;
                    string? persistenceLog = null;
                    string? resultLog = null;
                    var stopAfterResult = false;
                    lock (sync)
                    {
                        if (!pending.Any(item => item.id == next.id)) continue;
                        if (retry)
                        {
                            next.attempts++;
                            var retryAt = utcNow() + getRetryDelay(next.attempts);
                            foreach (var item in pending)
                                if (item.nextAttempt < retryAt) item.nextAttempt = retryAt;
                            save(out persistenceLog);
                            var detail = failure?.Message
                                ?? result?.responseDetail
                                ?? result?.reasonPhrase
                                ?? "request failed";
                            resultLog = $"EDDN upload for {eventName(next)} will retry after {retryAt:u}: {singleLine(detail)}";
                            scheduleNextLocked(utcNow());
                            stopAfterResult = true;
                        }
                        else
                        {
                            pending.RemoveAll(item => item.id == next.id);
                            if (pending.Count == 0)
                                persistenceLog = deleteStore();
                            else
                                save(out persistenceLog);

                            if (result?.isSuccess == true)
                            {
                                resultLog = $"EDDN uploaded {eventName(next)} to {next.environment}.";
                            }
                            else
                            {
                                var detail = result?.skipReason
                                    ?? result?.responseDetail
                                    ?? result?.reasonPhrase
                                    ?? "request was rejected";
                                resultLog = $"EDDN dropped {eventName(next)} without retry: {singleLine(detail)}";
                            }
                        }
                    }

                    // Logging can ultimately marshal to the UI. Never invoke it while
                    // holding the queue lock or Settings can deadlock against this worker.
                    writeLog(persistenceLog);
                    writeLog(resultLog);
                    if (stopAfterResult) return;

                    await Task.Delay(sendSpacing, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                processing.Release();
            }
        }

        internal void clear()
        {
            string? persistenceLog;
            lock (sync)
            {
                pending.Clear();
                persistenceLog = deleteStore();
            }
            writeLog(persistenceLog);
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                enabled = false;
            }
            shutdown.Cancel();
            timer.Dispose();

            // processDue may still be between its disposed check and WaitAsync, or
            // may still need to release the semaphore. These primitives own no native
            // resources in this usage, so allowing GC to reclaim them avoids a dispose race.
        }

        private void triggerProcessing()
        {
            if (disposed) return;
            processDue().ContinueWith(
                task => writeLog($"EDDN queue processing failed: {singleLine(task.Exception?.GetBaseException().Message)}"),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }

        private void schedule(TimeSpan delay)
        {
            if (disposed || !automaticProcessing) return;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
            try
            {
                timer.Change(delay, Timeout.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException) when (disposed)
            {
                // Dispose won the race after the check above.
            }
        }

        private void scheduleNextLocked(DateTimeOffset now)
        {
            if (!enabled || pending.Count == 0)
            {
                stopTimer();
                return;
            }

            var next = pending.Min(item => item.nextAttempt);
            schedule(next - now);
        }

        private List<EddnQueuedMessage> load()
        {
            if (!File.Exists(filepath)) return [];
            try
            {
                var json = File.ReadAllText(filepath);
                return JsonConvert.DeserializeObject<List<EddnQueuedMessage>>(json) ?? [];
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                var backup = filepath + ".bad-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                try
                {
                    File.Move(filepath, backup);
                    writeLog($"EDDN moved an unreadable queue to: {backup}");
                }
                catch (Exception moveError) when (moveError is IOException or UnauthorizedAccessException)
                {
                    writeLog($"EDDN could not preserve its unreadable queue: {moveError.Message}");
                }
                writeLog($"EDDN could not load its pending uploads: {ex.Message}");
                return [];
            }
        }

        private void stopTimer()
        {
            try
            {
                timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException) when (disposed)
            {
                // Dispose won the race after the caller checked the queue state.
            }
        }

        private bool save(out string? errorLog)
        {
            errorLog = null;
            try
            {
                var folder = Path.GetDirectoryName(filepath);
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
                var temporary = filepath + ".tmp";
                File.WriteAllText(
                    temporary,
                    JsonConvert.SerializeObject(pending, Formatting.Indented));
                File.Move(temporary, filepath, true);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errorLog = $"EDDN could not persist a pending upload: {ex.Message}";
                return false;
            }
        }

        private string? deleteStore()
        {
            try
            {
                if (File.Exists(filepath)) File.Delete(filepath);
                var temporary = filepath + ".tmp";
                if (File.Exists(temporary)) File.Delete(temporary);
                return null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return $"EDDN could not remove its empty queue: {ex.Message}";
            }
        }

        private void writeLog(string? message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            try
            {
                log(message);
            }
            catch
            {
                // Diagnostics must never stop or poison the durable upload queue.
            }
        }

        private static TimeSpan getRetryDelay(int attempts)
        {
            var multiplier = Math.Pow(2, Math.Clamp(attempts - 1, 0, 10));
            var delay = TimeSpan.FromTicks((long)(minimumRetryDelay.Ticks * multiplier));
            return delay > maximumRetryDelay ? maximumRetryDelay : delay;
        }

        private static string eventName(EddnQueuedMessage message)
        {
            return message.message.Value<string>("event")
                ?? message.schemaRef.Split('/').Reverse().Skip(1).FirstOrDefault()
                ?? "message";
        }

        private static string singleLine(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "request failed";
            var text = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
            return text.Length <= EddnTransport.MaximumResponseDetailBytes
                ? text
                : text[..EddnTransport.MaximumResponseDetailBytes];
        }
    }
}
