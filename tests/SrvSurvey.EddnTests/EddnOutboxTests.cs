using Newtonsoft.Json;
using System.Net;
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
            Assert.True(File.Exists(path));
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
        Assert.False(File.Exists(path));
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
        var saved = JsonConvert.DeserializeObject<List<EddnQueuedMessage>>(
            await File.ReadAllTextAsync(path));
        Assert.NotNull(saved);
        Assert.All(saved, item => Assert.True(item.nextAttempt >= now.AddMinutes(1)));
        Assert.Equal(1, saved[0].attempts);
        Assert.Equal(0, saved[1].attempts);
        Assert.Contains(logs, line => line.Contains("will retry", StringComparison.Ordinal));

        await queue.processDue();
        Assert.Equal(1, calls);
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
        Assert.False(File.Exists(path));
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
        Assert.False(File.Exists(path));
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

    private static EddnQueuedMessage queued(
        DateTimeOffset created,
        string stationName = "Test Port")
    {
        return new EddnQueuedMessage
        {
            id = Guid.NewGuid(),
            created = created,
            nextAttempt = created,
            environment = "live",
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
}
