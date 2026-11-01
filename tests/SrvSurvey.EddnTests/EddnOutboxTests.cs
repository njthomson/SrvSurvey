using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;
using System.Text;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnOutboxTests
{
    [Fact]
    public async Task QueueIsPersistedBeforeSendingAndSurvivesRestart()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        using (var first = outbox(
            path,
            EddnTransportTests.createTransport(_ => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK))),
            () => now))
        {
            first.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(first.enqueue(queued(now)));
            Assert.True(Directory.Exists(storePath(path)));
            Assert.Equal(1, first.pendingCount);
        }

        var calls = 0;
        using var restarted = outbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                calls++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }),
            () => now);
        Assert.Equal(1, restarted.pendingCount);
        restarted.setEnabled(true, discardPendingWhenDisabled: false);

        await restarted.processDue();

        Assert.Equal(1, calls);
        Assert.Equal(0, restarted.pendingCount);
        Assert.False(Directory.Exists(storePath(path)));
    }

    [Fact]
    public void EnqueuePersistsOneIndependentFilePerMessage()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        using var queue = outbox(
            path,
            EddnTransportTests.createTransport(_ => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK))),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);

        Assert.True(queue.enqueue(queued(now, "First Port")));
        var firstFile = Assert.Single(Directory.GetFiles(storePath(path), "*.json"));
        using var firstFileLease = new FileStream(
            firstFile,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        Assert.True(queue.enqueue(queued(now.AddSeconds(1), "Second Port")));
        Assert.Equal(2, Directory.GetFiles(storePath(path), "*.json").Length);
    }

    [Fact]
    public void CorruptMessageFileDoesNotBlockOtherPendingMessages()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var transport = EddnTransportTests.createTransport(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)));
        using (var first = outbox(path, transport, () => now))
        {
            first.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(first.enqueue(queued(now)));
        }

        File.WriteAllText(
            Path.Combine(storePath(path), "corrupt.json"),
            "{not valid json");
        var logs = new List<string>();
        using var restarted = new EddnOutbox(
            path,
            transport,
            logs.Add,
            () => now,
            automaticProcessing: false);

        Assert.Equal(1, restarted.pendingCount);
        Assert.Single(Directory.GetFiles(storePath(path), "corrupt.json.bad-*"));
        Assert.Contains(logs, line => line.Contains(
            "could not load a pending upload",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task MessageLimitLoadsOldestValidFilesFirstAndContinuesInLaterBatches()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var store = storePath(path);
        Directory.CreateDirectory(store);
        var now = DateTimeOffset.UtcNow;
        var newest = queued(now.AddSeconds(-1), "Newest Port");
        newest.id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        writeQueued(store, newest);
        var oldest = queued(now.AddSeconds(-2), "Oldest Port");
        oldest.id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        writeQueued(store, oldest);
        var logs = new List<string>();
        var deliveredStations = new List<string?>();

        using var queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(async request =>
            {
                deliveredStations.Add(await readStationName(request));
                return new HttpResponseMessage(HttpStatusCode.OK);
            }),
            logs.Add,
            () => now,
            automaticProcessing: false,
            maximumPendingMessages: 1);

        Assert.Equal(1, queue.pendingCount);
        Assert.Equal(2, Directory.GetFiles(store, "*.json").Length);
        Assert.Empty(Directory.GetFiles(store, "*.bad-*"));
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        await queue.processDue();

        Assert.Equal(new[] { "Oldest Port", "Newest Port" }, deliveredStations);
        Assert.Equal(0, queue.pendingCount);
        Assert.False(Directory.Exists(store));
        Assert.Contains(logs, line => line.Contains(
            "stopped loading pending uploads",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task StorageLimitLeavesOversizedValidFileUnchanged()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var store = storePath(path);
        Directory.CreateDirectory(store);
        writeQueued(store, queued(DateTimeOffset.UtcNow));
        var logs = new List<string>();
        var calls = 0;

        using var queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                calls++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }),
            logs.Add,
            automaticProcessing: false,
            maximumStoreBytes: 1);

        Assert.Equal(0, queue.pendingCount);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        await queue.processDue();

        Assert.Equal(0, calls);
        Assert.Single(Directory.GetFiles(store, "*.json"));
        Assert.Empty(Directory.GetFiles(store, "*.bad-*"));
        Assert.Contains(logs, line => line.Contains(
            "stopped loading pending uploads",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task LegacyArrayQueueMigratesToPerMessageTestSchemaStore()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var legacy = queued(now);
        await File.WriteAllTextAsync(
            path,
            JsonConvert.SerializeObject(new[] { legacy }));
        Uri? requestedUri = null;
        using var queue = outbox(
            path,
            EddnTransportTests.createTransport(request =>
            {
                requestedUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(
                    HttpStatusCode.ServiceUnavailable));
            }),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);

        await queue.processDue();

        Assert.Equal("https://live.example.test/upload/", requestedUri?.ToString());
        var saved = Assert.Single(loadSaved(path));
        Assert.EndsWith("/test", saved.schemaRef, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TransientFailureWaitsAtLeastOneMinuteAndPreservesOrder()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var calls = 0;
        var logs = new List<string>();
        using var queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                calls++;
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            }),
            logs.Add,
            () => now,
            automaticProcessing: false);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now, "first")));
        Assert.True(queue.enqueue(queued(now.AddSeconds(1), "second")));

        await queue.processDue();

        Assert.Equal(1, calls);
        Assert.Equal(2, queue.pendingCount);
        var saved = loadSaved(path);
        Assert.True(saved[0].nextAttempt >= now.AddMinutes(1));
        Assert.Equal(now.AddSeconds(1), saved[1].nextAttempt);
        Assert.Equal(1, saved[0].attempts);
        Assert.Equal(0, saved[1].attempts);
        Assert.Contains(logs, line => line.Contains("will retry", StringComparison.Ordinal));

        await queue.processDue();
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task RetriedHeadDoesNotBlockNewlyQueuedMessage()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var calls = 0;
        using var queue = outbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                calls++;
                return Task.FromResult(new HttpResponseMessage(
                    calls == 1
                        ? HttpStatusCode.ServiceUnavailable
                        : HttpStatusCode.OK));
            }),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now, "First Port")));

        await queue.processDue();
        Assert.Equal(1, calls);
        Assert.True(queue.enqueue(queued(now, "Second Port")));

        await queue.processDue();
        Assert.Equal(2, calls);
        Assert.Equal(1, queue.pendingCount);

        now = now.AddMinutes(1);
        await queue.processDue();

        Assert.Equal(3, calls);
        Assert.Equal(0, queue.pendingCount);
    }

    [Fact]
    public async Task SuspensionPreservesPendingMessagesUntilResumed()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var calls = 0;
        using var queue = outbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                calls++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now)));

        queue.setSuspended(true);
        await queue.processDue();

        Assert.Equal(0, calls);
        Assert.Equal(1, queue.pendingCount);
        Assert.True(Directory.Exists(storePath(path)));
        Assert.False(queue.enqueue(queued(now, "Blocked Port")));

        queue.setSuspended(false);
        await queue.processDue();

        Assert.Equal(1, calls);
        Assert.Equal(0, queue.pendingCount);
        Assert.False(Directory.Exists(storePath(path)));
    }

    [Fact]
    public async Task SuspensionCancelsActiveUploadWithoutMutatingRetryState()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var handler = new CancelThenSucceedHandler();
        using var client = new HttpClient(handler);
        var transport = new EddnTransport(
            client,
            new Uri("https://live.example.test/upload/"));
        using var queue = outbox(path, transport, () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now)));
        var processing = queue.processDue();
        await handler.Entered.WaitAsync(TimeSpan.FromSeconds(2));

        queue.setSuspended(true);
        await processing.WaitAsync(TimeSpan.FromSeconds(2));

        var saved = Assert.Single(loadSaved(path));
        Assert.Equal(0, saved.attempts);
        Assert.Equal(now, saved.nextAttempt);

        queue.setSuspended(false);
        await queue.processDue();

        Assert.Equal(2, handler.Calls);
        Assert.Equal(0, queue.pendingCount);
    }

    [Fact]
    public async Task RuntimeGateBlocksDeliveryWithoutDroppingTheQueue()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var mayUpload = false;
        var calls = 0;
        using var queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                calls++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }),
            utcNow: () => now,
            automaticProcessing: false,
            runtimeUploadAllowed: () => mayUpload);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now)));

        await queue.processDue();

        Assert.Equal(0, calls);
        Assert.Equal(1, queue.pendingCount);
        Assert.True(Directory.Exists(storePath(path)));

        mayUpload = true;
        await queue.processDue();

        Assert.Equal(1, calls);
        Assert.Equal(0, queue.pendingCount);
    }

    [Fact]
    public async Task RuntimeGateNeverRunsWhileTheQueueLockIsHeld()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var callbackCouldInspectQueue = false;
        EddnOutbox? queue = null;
        queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))),
            utcNow: () => now,
            automaticProcessing: false,
            runtimeUploadAllowed: () =>
            {
                var inspection = Task.Run(() => queue!.pendingCount);
                callbackCouldInspectQueue = inspection.Wait(TimeSpan.FromSeconds(1));
                return true;
            });
        using (queue)
        {
            queue.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(queue.enqueue(queued(now)));

            await queue.processDue();

            Assert.True(callbackCouldInspectQueue);
        }
    }

    [Fact]
    public void OnlyOneProcessCanOwnAndRewriteAnOutbox()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var transport = EddnTransportTests.createTransport(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var first = outbox(path, transport, () => now);
        var second = outbox(path, transport, () => now);
        try
        {
            first.setEnabled(true, discardPendingWhenDisabled: false);
            second.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(first.hasExclusiveOwnership);
            Assert.False(second.hasExclusiveOwnership);
            Assert.True(first.enqueue(queued(now, "First Port")));
            Assert.False(second.enqueue(queued(now, "Second Port")));
            Assert.Single(loadSaved(path));

            first.Dispose();
            second.setEnabled(true, discardPendingWhenDisabled: false);

            Assert.True(second.hasExclusiveOwnership);
            Assert.Equal(1, second.pendingCount);
            Assert.True(second.enqueue(queued(now, "Second Port")));
            Assert.Equal(
                2,
                loadSaved(path).Count);
        }
        finally
        {
            second.Dispose();
            first.Dispose();
        }
    }

    [Fact]
    public async Task DisabledOwnerKeepsLeaseUntilActiveUploadStops()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var handler = new HoldAfterCancellationHandler();
        using var client = new HttpClient(handler);
        var transport = new EddnTransport(
            client,
            new Uri("https://live.example.test/upload/"));
        var first = outbox(path, transport, () => now);
        EddnOutbox? second = null;
        try
        {
            first.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(first.enqueue(queued(now)));
            var processing = first.processDue();
            await handler.Entered.WaitAsync(TimeSpan.FromSeconds(2));

            first.setEnabled(false, discardPendingWhenDisabled: false);
            await handler.Cancelled.WaitAsync(TimeSpan.FromSeconds(2));
            second = outbox(
                path,
                EddnTransportTests.createTransport(_ =>
                    Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))),
                () => now);

            Assert.False(second.hasExclusiveOwnership);

            handler.Release();
            await processing.WaitAsync(TimeSpan.FromSeconds(2));
            second.setEnabled(true, discardPendingWhenDisabled: false);

            Assert.True(second.hasExclusiveOwnership);
            Assert.Equal(1, second.pendingCount);
        }
        finally
        {
            handler.Release();
            second?.Dispose();
            first.Dispose();
        }
    }

    [Fact]
    public void InvalidPersistedQueueIsQuarantinedWithoutThrowing()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        File.WriteAllText(
            path,
            "[{\"id\":\"00000000-0000-0000-0000-000000000000\",\"schemaRef\":null}]");
        var logs = new List<string>();

        using var queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))),
            logs.Add,
            automaticProcessing: false);

        Assert.Equal(0, queue.pendingCount);
        Assert.False(Directory.Exists(storePath(path)));
        Assert.Single(Directory.GetFiles(folder.path, "eddn-outbox-v1.json.bad-*"));
        Assert.Contains(logs, line => line.Contains(
            "invalid or excessive entries",
            StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.RequestEntityTooLarge)]
    [InlineData(HttpStatusCode.UpgradeRequired)]
    public async Task PermanentGatewayRejectionIsDropped(HttpStatusCode statusCode)
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        using var queue = outbox(
            path,
            EddnTransportTests.createTransport(_ => Task.FromResult(
                new HttpResponseMessage(statusCode))),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now)));

        await queue.processDue();

        Assert.Equal(0, queue.pendingCount);
        Assert.False(Directory.Exists(storePath(path)));
    }

    [Fact]
    public void DisablingSharingDeletesPendingUploads()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        using var queue = outbox(
            path,
            EddnTransportTests.createTransport(_ => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK))),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now)));

        queue.setEnabled(false, discardPendingWhenDisabled: true);

        Assert.Equal(0, queue.pendingCount);
        Assert.False(Directory.Exists(storePath(path)));
    }

    [Fact]
    public async Task UploadLoggingNeverRunsWhileTheQueueLockIsHeld()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var callbackCouldInspectQueue = false;
        EddnOutbox? queue = null;
        queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK))),
            _ =>
            {
                var inspection = Task.Run(() => queue!.pendingCount);
                callbackCouldInspectQueue = inspection.Wait(TimeSpan.FromSeconds(1));
            },
            () => now,
            automaticProcessing: false);
        using (queue)
        {
            queue.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(queue.enqueue(queued(now)));

            await queue.processDue();

            Assert.True(callbackCouldInspectQueue);
        }
    }

    [Fact]
    public void DisableLoggingNeverRunsWhileTheQueueLockIsHeld()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var callbackCouldInspectQueue = false;
        EddnOutbox? queue = null;
        queue = new EddnOutbox(
            path,
            EddnTransportTests.createTransport(_ => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK))),
            _ =>
            {
                var inspection = Task.Run(() => queue!.pendingCount);
                callbackCouldInspectQueue = inspection.Wait(TimeSpan.FromSeconds(1));
            },
            () => now,
            automaticProcessing: false);
        using (queue)
        {
            queue.setEnabled(true, discardPendingWhenDisabled: false);
            Assert.True(queue.enqueue(queued(now)));

            queue.setEnabled(false, discardPendingWhenDisabled: true);

            Assert.True(callbackCouldInspectQueue);
        }
    }

    [Fact]
    public async Task DisposeDoesNotRaceAnActiveUpload()
    {
        using var folder = new TemporaryFolder();
        var path = Path.Combine(folder.path, "eddn-outbox-v1.json");
        var now = DateTimeOffset.Parse("2026-07-28T12:00:00Z");
        var enteredTransport = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseTransport = new TaskCompletionSource<HttpResponseMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var queue = outbox(
            path,
            EddnTransportTests.createTransport(_ =>
            {
                enteredTransport.SetResult();
                return releaseTransport.Task;
            }),
            () => now);
        queue.setEnabled(true, discardPendingWhenDisabled: false);
        Assert.True(queue.enqueue(queued(now)));
        var processing = queue.processDue();
        await enteredTransport.Task;

        queue.Dispose();
        releaseTransport.SetResult(new HttpResponseMessage(HttpStatusCode.OK));

        await processing;
    }

    private static EddnOutbox outbox(
        string path,
        EddnTransport transport,
        Func<DateTimeOffset> clock)
    {
        return new EddnOutbox(
            path,
            transport,
            utcNow: clock,
            automaticProcessing: false);
    }

    private static string storePath(string legacyPath) => legacyPath + ".d";

    private static List<EddnQueuedMessage> loadSaved(string legacyPath)
    {
        return Directory.EnumerateFiles(storePath(legacyPath), "*.json")
            .Select(path => JsonConvert.DeserializeObject<EddnQueuedMessage>(
                File.ReadAllText(path))!)
            .OrderBy(message => message.created)
            .ToList();
    }

    private static void writeQueued(string store, EddnQueuedMessage message)
    {
        File.WriteAllText(
            Path.Combine(store, message.id.ToString("N") + ".json"),
            JsonConvert.SerializeObject(message));
    }

    private static async Task<string?> readStationName(HttpRequestMessage request)
    {
        var compressed = await request.Content!.ReadAsByteArrayAsync();
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        var payload = Newtonsoft.Json.Linq.JObject.Parse(
            await reader.ReadToEndAsync());
        return payload["message"]?["StationName"]?.ToObject<string>();
    }

    private static EddnQueuedMessage queued(
        DateTimeOffset created,
        string stationName = "Test Port")
    {
        return new EddnQueuedMessage
        {
            id = Guid.NewGuid(),
            created = created,
            nextAttempt = created,
            schemaRef = "https://eddn.edcd.io/schemas/dockinggranted/1",
            header = EddnTransportTests.header(),
            message = new Newtonsoft.Json.Linq.JObject
            {
                ["timestamp"] = "2026-07-28T12:00:00Z",
                ["event"] = "DockingGranted",
                ["MarketID"] = 1,
                ["StationName"] = stationName,
            },
        };
    }

    private sealed class TemporaryFolder : IDisposable
    {
        internal readonly string path = Path.Combine(
            Path.GetTempPath(),
            "SrvSurvey-EddnTests-" + Guid.NewGuid().ToString("N"));

        internal TemporaryFolder()
        {
            Directory.CreateDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
    }

    private sealed class CancelThenSucceedHandler : HttpMessageHandler
    {
        private int calls;

        internal Task Entered => entered.Task;

        internal int Calls => calls;

        private readonly TaskCompletionSource entered = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                entered.TrySetResult();
                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    cancellationToken);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class HoldAfterCancellationHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource entered = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource cancelled = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource release = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        internal Task Entered => entered.Task;

        internal Task Cancelled => cancelled.Task;

        internal void Release() => release.TrySetResult();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            entered.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                cancelled.TrySetResult();
                await release.Task;
                throw;
            }

            throw new InvalidOperationException("The cancellation test transport completed unexpectedly.");
        }
    }
}
