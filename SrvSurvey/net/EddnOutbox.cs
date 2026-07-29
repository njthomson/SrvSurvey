using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

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
        private const int maximumPendingMessages = 4096;
        private const long maximumStoreBytes = 64L * 1024 * 1024;

        private readonly string filepath;
        private readonly string ownershipPath;
        private readonly EddnTransport transport;
        private readonly Action<string> log;
        private readonly Func<DateTimeOffset> utcNow;
        private readonly bool automaticProcessing;
        private readonly Func<bool> runtimeUploadAllowed;
        private readonly object sync = new();
        private readonly SemaphoreSlim processing = new(1, 1);
        private readonly System.Threading.Timer timer;
        private readonly CancellationTokenSource shutdown = new();
        private CancellationTokenSource activityCancellation = new();
        private List<EddnQueuedMessage> pending;
        private FileStream? ownershipLease;
        private bool enabled;
        private bool suspended;
        private bool ownershipWarningReported;
        private volatile bool disposed;

        internal EddnOutbox(
            string filepath,
            EddnTransport transport,
            Action<string>? log = null,
            Func<DateTimeOffset>? utcNow = null,
            bool automaticProcessing = true,
            Func<bool>? runtimeUploadAllowed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
            ArgumentNullException.ThrowIfNull(transport);
            this.filepath = filepath;
            ownershipPath = getOwnershipPath(filepath);
            this.transport = transport;
            this.log = log ?? (_ => { });
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
            this.automaticProcessing = automaticProcessing;
            this.runtimeUploadAllowed = runtimeUploadAllowed ?? (() => true);
            pending = [];
            timer = new System.Threading.Timer(
                _ => triggerProcessing(),
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);

            List<string> ownershipLogs = [];
            lock (sync)
            {
                tryAcquireOwnershipLocked(ownershipLogs);
            }

            writeLogs(ownershipLogs);
        }

        internal int pendingCount
        {
            get
            {
                lock (sync) return pending.Count;
            }
        }

        internal bool hasExclusiveOwnership
        {
            get
            {
                lock (sync) return ownershipLease is not null;
            }
        }

        internal void setEnabled(bool value, bool discardPendingWhenDisabled)
        {
            var changed = false;
            string? persistenceLog = null;
            string? sharingLog = null;
            CancellationTokenSource? cancellation = null;
            List<string> ownershipLogs = [];
            var canSchedule = false;
            var acquiredOwnership = false;
            lock (sync)
            {
                if (disposed) return;
                if (value || discardPendingWhenDisabled)
                {
                    var hadOwnership = ownershipLease is not null;
                    tryAcquireOwnershipLocked(ownershipLogs);
                    acquiredOwnership = !hadOwnership
                        && ownershipLease is not null;
                }

                changed = enabled != value;
                enabled = value;
                if (!enabled)
                {
                    cancellation = replaceActivityCancellationLocked();
                    if (discardPendingWhenDisabled
                        && ownershipLease is not null
                        && pending.Count > 0)
                    {
                        var count = pending.Count;
                        pending.Clear();
                        persistenceLog = deleteStore();
                        sharingLog = $"EDDN discarded {count:N0} pending upload(s) because sharing was disabled.";
                    }

                }

                canSchedule = enabled
                    && !suspended
                    && ownershipLease is not null;
                if (canSchedule
                    && (changed || acquiredOwnership)
                    && automaticProcessing)
                {
                    schedule(startupDelay);
                }
                else if (!canSchedule)
                {
                    stopTimer();
                }
            }

            cancellation?.Cancel();
            if (!value) releaseOwnershipIfIdle();
            writeLogs(ownershipLogs);
            writeLog(persistenceLog);
            writeLog(sharingLog);
        }

        internal void setSuspended(bool value)
        {
            CancellationTokenSource? cancellation = null;
            List<string> ownershipLogs = [];
            var shouldSchedule = false;
            lock (sync)
            {
                if (disposed || suspended == value) return;
                suspended = value;
                if (suspended)
                {
                    cancellation = replaceActivityCancellationLocked();
                }
                else if (enabled)
                {
                    tryAcquireOwnershipLocked(ownershipLogs);
                    shouldSchedule = ownershipLease is not null;
                }

                if (suspended)
                {
                    stopTimer();
                }
                else if (shouldSchedule && automaticProcessing)
                {
                    schedule(TimeSpan.Zero);
                }
            }

            cancellation?.Cancel();
            writeLogs(ownershipLogs);
        }

        internal bool enqueue(EddnQueuedMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            string? persistenceLog = null;
            var queued = false;
            lock (sync)
            {
                if (!enabled
                    || suspended
                    || disposed
                    || ownershipLease is null)
                {
                    return false;
                }

                if (pending.Count >= maximumPendingMessages)
                {
                    persistenceLog =
                        $"EDDN did not queue {eventName(message)} because the local backlog reached {maximumPendingMessages:N0} messages.";
                }
                else
                {
                    pending.Add(message);
                    if (!save(out persistenceLog))
                    {
                        pending.Remove(message);
                    }
                    else
                        queued = true;
                }
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
                    bool mayUpload;
                    try
                    {
                        mayUpload = runtimeUploadAllowed();
                    }
                    catch
                    {
                        // Runtime attribution and consent checks fail closed.
                        mayUpload = false;
                    }

                    if (!mayUpload)
                    {
                        lock (sync)
                        {
                            if (enabled
                                && !suspended
                                && !disposed
                                && ownershipLease is not null)
                            {
                                schedule(startupDelay);
                            }
                        }
                        return;
                    }

                    EddnQueuedMessage? next;
                    CancellationToken activityToken;
                    lock (sync)
                    {
                        if (!enabled
                            || suspended
                            || disposed
                            || ownershipLease is null)
                        {
                            return;
                        }

                        var now = utcNow();
                        next = pending.FirstOrDefault();
                        if (next == null)
                        {
                            scheduleNextLocked(now);
                            return;
                        }

                        if (next.nextAttempt > now)
                        {
                            schedule(next.nextAttempt - now);
                            return;
                        }

                        activityToken = activityCancellation.Token;
                    }

                    EddnUploadResult? result = null;
                    Exception? failure = null;
                    try
                    {
                        using var combined = CancellationTokenSource.CreateLinkedTokenSource(
                            shutdown.Token,
                            activityToken,
                            cancellationToken);
                        result = await transport.upload(next, combined.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (
                        cancellationToken.IsCancellationRequested)
                    {
                        throw;
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
                        if (!enabled || suspended || disposed)
                        {
                            return;
                        }

                        if (!pending.Any(item => item.id == next.id)) continue;
                        if (retry)
                        {
                            next.attempts++;
                            var retryAt = utcNow() + getRetryDelay(next.attempts);
                            next.nextAttempt = retryAt;
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
                                resultLog = $"EDDN uploaded {eventName(next)} to the live gateway.";
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
                releaseOwnershipIfInactive();
                processing.Release();
            }
        }

        internal void clear()
        {
            string? persistenceLog;
            lock (sync)
            {
                if (ownershipLease is null) return;
                pending.Clear();
                persistenceLog = deleteStore();
            }
            writeLog(persistenceLog);
        }

        public void Dispose()
        {
            CancellationTokenSource? cancellation;
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                enabled = false;
                suspended = true;
                cancellation = replaceActivityCancellationLocked();
            }
            cancellation.Cancel();
            shutdown.Cancel();
            timer.Dispose();

            if (processing.Wait(0))
            {
                try
                {
                    releaseOwnershipIfInactive();
                }
                finally
                {
                    processing.Release();
                }
            }

            // processDue may still be between its disposed check and WaitAsync, or
            // may still need to release the semaphore. The active worker releases
            // the store lease from its finally block before another process can use it.
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
            if (!enabled
                || suspended
                || ownershipLease is null
                || pending.Count == 0)
            {
                stopTimer();
                return;
            }

            schedule(pending[0].nextAttempt - now);
        }

        private List<EddnQueuedMessage> load(List<string> messages)
        {
            if (!File.Exists(filepath)) return [];
            try
            {
                if (new FileInfo(filepath).Length > maximumStoreBytes)
                {
                    throw new InvalidDataException(
                        $"the queue exceeded {maximumStoreBytes / 1024 / 1024:N0} MiB");
                }

                var json = File.ReadAllText(filepath);
                var loaded = JsonConvert.DeserializeObject<List<EddnQueuedMessage>>(json) ?? [];
                if (loaded.Count > maximumPendingMessages
                    || loaded.Any(item => !isValid(item)))
                {
                    throw new InvalidDataException(
                        "the queue contained invalid or excessive entries");
                }

                // Earlier PR builds persisted beta/dev as destinations.
                // Preserve their test intent while normalizing delivery to the
                // documented, always-available Live gateway.
                foreach (var item in loaded)
                {
                    if (item.environment is "beta" or "dev"
                        && !item.schemaRef.EndsWith(
                            "/test",
                            StringComparison.Ordinal))
                    {
                        item.schemaRef += "/test";
                    }

                    item.environment = "live";
                }

                return loaded;
            }
            catch (Exception ex) when (
                ex is IOException
                    or JsonException
                    or UnauthorizedAccessException
                    or InvalidDataException)
            {
                var backup = filepath + ".bad-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                try
                {
                    File.Move(filepath, backup);
                    messages.Add($"EDDN moved an unreadable queue to: {backup}");
                }
                catch (Exception moveError) when (moveError is IOException or UnauthorizedAccessException)
                {
                    messages.Add($"EDDN could not preserve its unreadable queue: {moveError.Message}");
                }
                messages.Add($"EDDN could not load its pending uploads: {ex.Message}");
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
                var json = JsonConvert.SerializeObject(pending, Formatting.Indented);
                if (Encoding.UTF8.GetByteCount(json) > maximumStoreBytes)
                {
                    errorLog =
                        $"EDDN did not grow its local queue beyond {maximumStoreBytes / 1024 / 1024:N0} MiB.";
                    return false;
                }

                File.WriteAllText(
                    temporary,
                    json);
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

        private void writeLogs(IEnumerable<string> messages)
        {
            foreach (var message in messages)
            {
                writeLog(message);
            }
        }

        private bool tryAcquireOwnershipLocked(List<string> messages)
        {
            if (ownershipLease is not null) return true;
            try
            {
                var folder = Path.GetDirectoryName(ownershipPath);
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
                ownershipLease = new FileStream(
                    ownershipPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None);
                pending = load(messages);
                if (ownershipWarningReported)
                {
                    messages.Add(
                        "EDDN acquired the local outbox after the other SrvSurvey instance released it.");
                    ownershipWarningReported = false;
                }

                return true;
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                if (!ownershipWarningReported)
                {
                    messages.Add(
                        "EDDN uploads are paused because another SrvSurvey instance owns the local outbox.");
                    ownershipWarningReported = true;
                }

                return false;
            }
        }

        private CancellationTokenSource replaceActivityCancellationLocked()
        {
            var previous = activityCancellation;
            activityCancellation = new CancellationTokenSource();
            return previous;
        }

        private void releaseOwnershipIfIdle()
        {
            if (!processing.Wait(0)) return;
            try
            {
                releaseOwnershipIfInactive();
            }
            finally
            {
                processing.Release();
            }
        }

        private void releaseOwnershipIfInactive()
        {
            lock (sync)
            {
                if (enabled && !disposed) return;
                ownershipLease?.Dispose();
                ownershipLease = null;
            }
        }

        private static bool isValid(EddnQueuedMessage? message)
        {
            return message is not null
                && message.id != Guid.Empty
                && message.created != default
                && message.nextAttempt != default
                && message.attempts >= 0
                && message.environment is "live" or "beta" or "dev"
                && !string.IsNullOrWhiteSpace(message.schemaRef)
                && message.schemaRef.StartsWith(
                    "https://eddn.edcd.io/schemas/",
                    StringComparison.Ordinal)
                && message.header is not null
                && !string.IsNullOrWhiteSpace(message.header.uploaderID)
                && message.message is not null
                && !string.IsNullOrWhiteSpace(
                    message.message.Value<string>("event"));
        }

        private static string getOwnershipPath(string filepath)
        {
            var normalizedPath = Path.GetFullPath(filepath);
            if (OperatingSystem.IsWindows())
            {
                normalizedPath = normalizedPath.ToUpperInvariant();
            }

            var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPath)));
            return Path.Combine(
                Path.GetTempPath(),
                "SrvSurvey",
                "eddn-outbox-locks",
                hash + ".lock");
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
