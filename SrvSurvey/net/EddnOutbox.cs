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
        private const int defaultMaximumPendingMessages = 4096;
        private const long defaultMaximumStoreBytes = 64L * 1024 * 1024;

        private readonly string filepath;
        private readonly string storeFolder;
        private readonly string ownershipPath;
        private readonly EddnTransport transport;
        private readonly Action<string> log;
        private readonly Func<DateTimeOffset> utcNow;
        private readonly bool automaticProcessing;
        private readonly Func<bool> runtimeUploadAllowed;
        private readonly int maximumPendingMessages;
        private readonly long maximumStoreBytes;
        private readonly object sync = new();
        private readonly SemaphoreSlim processing = new(1, 1);
        private readonly System.Threading.Timer timer;
        private readonly CancellationTokenSource shutdown = new();
        private CancellationTokenSource activityCancellation = new();
        private List<EddnQueuedMessage> pending;
        private readonly Dictionary<Guid, long> persistedBytes = [];
        private readonly HashSet<Guid> loadCycleIds = [];
        private long storeBytes;
        private FileStream? ownershipLease;
        private bool enabled;
        private bool suspended;
        private bool loadingTruncated;
        private bool ownershipWarningReported;
        private volatile bool disposed;

        internal EddnOutbox(
            string filepath,
            EddnTransport transport,
            Action<string>? log = null,
            Func<DateTimeOffset>? utcNow = null,
            bool automaticProcessing = true,
            Func<bool>? runtimeUploadAllowed = null,
            int maximumPendingMessages = defaultMaximumPendingMessages,
            long maximumStoreBytes = defaultMaximumStoreBytes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
            ArgumentNullException.ThrowIfNull(transport);
            if (maximumPendingMessages <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumPendingMessages));
            if (maximumStoreBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumStoreBytes));
            this.filepath = filepath;
            storeFolder = filepath + ".d";
            ownershipPath = getOwnershipPath(filepath);
            this.transport = transport;
            this.log = log ?? (_ => { });
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
            this.automaticProcessing = automaticProcessing;
            this.runtimeUploadAllowed = runtimeUploadAllowed ?? (() => true);
            this.maximumPendingMessages = maximumPendingMessages;
            this.maximumStoreBytes = maximumStoreBytes;
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
                        && (pending.Count > 0 || loadingTruncated))
                    {
                        var count = pending.Count;
                        var includedUnloadedFiles = loadingTruncated;
                        pending.Clear();
                        persistedBytes.Clear();
                        loadCycleIds.Clear();
                        storeBytes = 0;
                        loadingTruncated = false;
                        persistenceLog = deleteStore();
                        sharingLog = includedUnloadedFiles
                            ? "EDDN discarded all pending uploads because sharing was disabled."
                            : $"EDDN discarded {count:N0} pending upload(s) because sharing was disabled.";
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
                    if (!persistMessage(message, out persistenceLog))
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
                        next = nextDueLocked(now);
                        if (next == null)
                        {
                            scheduleNextLocked(now);
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
                    List<string> reloadLogs = [];
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
                            persistMessage(next, out persistenceLog);
                            var detail = failure?.Message
                                ?? result?.responseDetail
                                ?? result?.reasonPhrase
                                ?? "request failed";
                            resultLog = $"EDDN upload for {eventName(next)} will retry after {retryAt:u}: {singleLine(detail)}";
                            scheduleNextLocked(utcNow());
                        }
                        else
                        {
                            pending.RemoveAll(item => item.id == next.id);
                            persistenceLog = deleteMessage(next);
                            if (pending.Count == 0)
                            {
                                if (loadingTruncated)
                                    pending = load(
                                        reloadLogs,
                                        continueTruncatedLoad: true);
                                else
                                    persistenceLog ??= deleteStore();
                            }

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
                    writeLogs(reloadLogs);

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
                persistedBytes.Clear();
                loadCycleIds.Clear();
                storeBytes = 0;
                loadingTruncated = false;
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

            schedule(pending.Min(item => item.nextAttempt) - now);
        }

        private EddnQueuedMessage? nextDueLocked(DateTimeOffset now)
        {
            // Give every new message one attempt in durable creation order.
            // Retried messages then use their own due time. This prevents one
            // persistently retryable message from blocking unrelated uploads.
            return pending
                .Where(item => item.nextAttempt <= now)
                .OrderBy(item => item.attempts == 0 ? 0 : 1)
                .ThenBy(item => item.attempts == 0 ? item.created : item.nextAttempt)
                .ThenBy(item => item.created)
                .FirstOrDefault();
        }

        private List<EddnQueuedMessage> load(
            List<string> messages,
            bool continueTruncatedLoad = false)
        {
            if (!continueTruncatedLoad) loadCycleIds.Clear();
            persistedBytes.Clear();
            storeBytes = 0;
            loadingTruncated = false;
            var loaded = loadMessageFiles(messages, loadCycleIds);
            migrateLegacyStore(loaded, messages);
            foreach (var item in loaded) loadCycleIds.Add(item.id);
            if (!loadingTruncated) loadCycleIds.Clear();
            return loaded
                .OrderBy(item => item.created)
                .ToList();
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

        private List<EddnQueuedMessage> loadMessageFiles(
            List<string> messages,
            HashSet<Guid> ids)
        {
            if (!Directory.Exists(storeFolder)) return [];
            List<(EddnQueuedMessage item, long length)> candidates = [];
            var seenIds = new HashSet<Guid>(ids);
            foreach (var path in Directory.EnumerateFiles(storeFolder, "*.json"))
            {
                try
                {
                    var length = new FileInfo(path).Length;
                    if (length <= 0)
                    {
                        throw new InvalidDataException(
                            "the queue contained an empty entry");
                    }

                    var item = JsonConvert.DeserializeObject<EddnQueuedMessage>(
                        File.ReadAllText(path));
                    normalize(item);
                    if (!isValid(item) || !seenIds.Add(item!.id))
                        throw new InvalidDataException("the queue contained an invalid or duplicate entry");

                    candidates.Add((item, length));
                }
                catch (Exception ex) when (
                    ex is IOException
                        or JsonException
                        or UnauthorizedAccessException
                        or InvalidDataException)
                {
                    quarantine(path, messages);
                    messages.Add($"EDDN could not load a pending upload: {ex.Message}");
                }
            }

            List<EddnQueuedMessage> loaded = [];
            foreach (var candidate in candidates.OrderBy(candidate => candidate.item.created))
            {
                if (loaded.Count >= maximumPendingMessages)
                {
                    loadingTruncated = true;
                    messages.Add(
                        $"EDDN stopped loading pending uploads after reaching the {maximumPendingMessages:N0}-message limit; remaining files were left unchanged.");
                    break;
                }

                if (candidate.length > maximumStoreBytes - storeBytes)
                {
                    loadingTruncated = true;
                    messages.Add(
                        $"EDDN stopped loading pending uploads after reaching the {maximumStoreBytes / 1024 / 1024:N0} MiB storage limit; remaining files were left unchanged.");
                    break;
                }

                loaded.Add(candidate.item);
                persistedBytes[candidate.item.id] = candidate.length;
                storeBytes += candidate.length;
            }

            return loaded;
        }

        private void migrateLegacyStore(
            List<EddnQueuedMessage> loaded,
            List<string> messages)
        {
            if (!File.Exists(filepath)) return;
            try
            {
                if (new FileInfo(filepath).Length > maximumStoreBytes)
                    throw new InvalidDataException(
                        $"the queue exceeded {maximumStoreBytes / 1024 / 1024:N0} MiB");

                var legacy = JsonConvert.DeserializeObject<List<EddnQueuedMessage>>(
                    File.ReadAllText(filepath)) ?? [];
                if (legacy.Count + loaded.Count > maximumPendingMessages)
                    throw new InvalidDataException("the queue contained excessive entries");

                var ids = loaded.Select(item => item.id).ToHashSet();
                var migrated = true;
                foreach (var item in legacy)
                {
                    normalize(item);
                    if (!isValid(item))
                        throw new InvalidDataException("the queue contained invalid or excessive entries");
                    if (!ids.Add(item.id)) continue;
                    loaded.Add(item);
                    if (!persistMessage(item, out var error))
                    {
                        migrated = false;
                        if (error != null) messages.Add(error);
                    }
                }

                if (migrated) File.Delete(filepath);
            }
            catch (Exception ex) when (
                ex is IOException
                    or JsonException
                    or UnauthorizedAccessException
                    or InvalidDataException)
            {
                quarantine(filepath, messages);
                messages.Add($"EDDN could not load its legacy pending uploads: {ex.Message}");
            }
        }

        private bool persistMessage(EddnQueuedMessage message, out string? errorLog)
        {
            errorLog = null;
            try
            {
                Directory.CreateDirectory(storeFolder);
                var json = JsonConvert.SerializeObject(message, Formatting.None);
                var bytes = Encoding.UTF8.GetByteCount(json);
                var previousBytes = persistedBytes.GetValueOrDefault(message.id);
                if (storeBytes - previousBytes + bytes > maximumStoreBytes)
                {
                    errorLog =
                        $"EDDN did not grow its local queue beyond {maximumStoreBytes / 1024 / 1024:N0} MiB.";
                    return false;
                }

                var path = messagePath(message.id);
                var temporary = path + ".tmp";
                File.WriteAllText(temporary, json);
                File.Move(temporary, path, true);
                persistedBytes[message.id] = bytes;
                storeBytes = storeBytes - previousBytes + bytes;
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errorLog = $"EDDN could not persist a pending upload: {ex.Message}";
                return false;
            }
        }

        private string? deleteMessage(EddnQueuedMessage message)
        {
            try
            {
                var path = messagePath(message.id);
                if (File.Exists(path)) File.Delete(path);
                var temporary = path + ".tmp";
                if (File.Exists(temporary)) File.Delete(temporary);
                storeBytes -= persistedBytes.GetValueOrDefault(message.id);
                persistedBytes.Remove(message.id);
                return null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return $"EDDN could not remove a delivered upload: {ex.Message}";
            }
        }

        private string? deleteStore()
        {
            try
            {
                if (File.Exists(filepath)) File.Delete(filepath);
                var temporary = filepath + ".tmp";
                if (File.Exists(temporary)) File.Delete(temporary);
                if (Directory.Exists(storeFolder)) Directory.Delete(storeFolder, recursive: true);
                persistedBytes.Clear();
                storeBytes = 0;
                return null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return $"EDDN could not remove its empty queue: {ex.Message}";
            }
        }

        private string messagePath(Guid id)
        {
            return Path.Combine(storeFolder, id.ToString("N") + ".json");
        }

        private static void normalize(EddnQueuedMessage? item)
        {
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.schemaRef)) return;
            item.schemaRef = EddnTransport.applySchemaPolicy(
                item.schemaRef,
                EddnTransport.testSchemasEnabled);
        }

        private static void quarantine(string path, List<string> messages)
        {
            var backup = path
                + ".bad-"
                + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
                + "-"
                + Guid.NewGuid().ToString("N");
            try
            {
                File.Move(path, backup);
                messages.Add($"EDDN moved an unreadable queue entry to: {backup}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                messages.Add($"EDDN could not preserve an unreadable queue entry: {ex.Message}");
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
