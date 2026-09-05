using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Reflection;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnSessionPublisherTests
{
    [Fact]
    public void GameOwnsOnlyTheApplicationLifetimeEddnService()
    {
        var staticEddnFields = typeof(Game)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => field.FieldType == typeof(EDDN));
        var instanceEddnFields = typeof(Game)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => field.FieldType == typeof(EDDN));
        var mainEddnFields = typeof(Main)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => field.FieldType == typeof(EDDN));

        Assert.Single(staticEddnFields);
        Assert.Empty(instanceEddnFields);
        Assert.Empty(mainEddnFields);
    }

    [Fact]
    public void SignalBatchKeepsItsSourceSystemWhenJumpFlushesIt()
    {
        var sink = new RecordingSink();
        using var session = createSession(sink, "Commander A", location("System A", 123));

        session.onJournalEntry(
            JObject.Parse(
                """
                {"timestamp":"2026-08-22T12:00:00Z","event":"FSSSignalDiscovered","SystemAddress":123,"SignalName":"$MULTIPLAYER_SCENARIO42_TITLE;","SignalType":"FleetCarrier"}
                """),
            context());
        session.onJournalEntry(
            JObject.Parse(
                """
                {"timestamp":"2026-08-22T12:00:01Z","event":"FSDJump","StarSystem":"System B","SystemAddress":456,"StarPos":[4,5,6]}
                """),
            context());

        Assert.Equal(2, sink.messages.Count);
        var signals = sink.messages[0].prepared.message;
        Assert.Equal("FSSSignalDiscovered", signals.Value<string>("event"));
        Assert.Equal("System A", signals.Value<string>("StarSystem"));
        Assert.Equal(123, signals.Value<long>("SystemAddress"));
        Assert.Equal([1d, 2d, 3d], signals["StarPos"]!.Values<double>());

        var jump = sink.messages[1].prepared.message;
        Assert.Equal("System B", jump.Value<string>("StarSystem"));
        Assert.Equal(456, jump.Value<long>("SystemAddress"));
    }

    [Fact]
    public void QueuedHeadersRemainBoundToTheirCommanderSession()
    {
        var sink = new RecordingSink();
        using (var first = createSession(sink, "Commander A", location("System A", 123)))
        {
            first.onJournalEntry(jump("System A", 123), context());
        }

        using (var second = createSession(sink, "Commander B", location("System B", 456)))
        {
            second.onJournalEntry(jump("System B", 456), context());
        }

        Assert.Equal(["Commander A", "Commander B"],
            sink.messages.Select(item => item.header.uploaderID));
    }

    [Fact]
    public void DifferentLoadGameCommanderStopsTheOldSession()
    {
        var sink = new RecordingSink();
        var logs = new List<string>();
        using var session = createSession(
            sink,
            "Commander A",
            location("System A", 123),
            logs.Add);

        session.onJournalEntry(
            JObject.Parse(
                """
                {"timestamp":"2026-08-22T12:00:00Z","event":"LoadGame","Commander":"Commander B"}
                """),
            context());
        session.onJournalEntry(jump("System A", 123), context());

        Assert.Empty(sink.messages);
        Assert.Contains(logs, line => line.Contains(
            "a new Game session must capture the new Commander",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task RuntimeGenerationChangeDuringCompanionReadRejectsStaleData()
    {
        var sink = new RecordingSink();
        var readerStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseReader = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var session = new EddnSessionPublisher(
            sink,
            header("Commander A"),
            Path.GetTempPath(),
            location("System A", 123),
            companionReader: async (_, _, cancellationToken) =>
            {
                readerStarted.TrySetResult();
                await releaseReader.Task.WaitAsync(cancellationToken);
                return new EddnCompanionReadResult(
                    JObject.Parse(
                        """
                        {"timestamp":"2026-08-22T12:00:00Z","event":"NavRoute","Route":[]}
                        """),
                    null);
            });

        session.onJournalEntry(
            JObject.Parse(
                """
                {"timestamp":"2026-08-22T12:00:00Z","event":"NavRoute"}
                """),
            context());
        await readerStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        sink.invalidate();
        releaseReader.TrySetResult();
        await sink.enqueueAttempt.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Empty(sink.messages);
    }

    [Fact]
    public async Task JournalCallbackFinishingAfterDisposeDoesNotTouchDisposedState()
    {
        var sink = new BlockingBeginSink();
        var session = createSession(sink, "Commander A", location("System A", 123));
        var publishing = Task.Run(() =>
            session.onJournalEntry(jump("System A", 123), context()));
        await sink.entered.Task.WaitAsync(TimeSpan.FromSeconds(2));

        session.Dispose();
        sink.release.TrySetResult();

        await publishing.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(0, sink.enqueueAttempts);
    }

    private static EddnSessionPublisher createSession(
        IEddnSessionSink sink,
        string commander,
        EddnLocationContext initialLocation,
        Action<string>? log = null)
    {
        return new EddnSessionPublisher(
            sink,
            header(commander),
            Path.GetTempPath(),
            initialLocation,
            log);
    }

    private static UploadPayloadHeader header(string commander)
    {
        return new UploadPayloadHeader(
            commander,
            "4.1.2.3",
            "r123/r0",
            "2.0.95.0");
    }

    private static EddnMessageContext context()
    {
        return new EddnMessageContext(
            null,
            horizons: true,
            odyssey: true);
    }

    private static EddnLocationContext location(string name, long address)
    {
        return new EddnLocationContext(name, address, [1, 2, 3]);
    }

    private static JObject jump(string system, long address)
    {
        return new JObject
        {
            ["timestamp"] = "2026-08-22T12:00:00Z",
            ["event"] = "FSDJump",
            ["StarSystem"] = system,
            ["SystemAddress"] = address,
            ["StarPos"] = new JArray(1, 2, 3),
        };
    }

    private sealed class RecordingSink : IEddnSessionSink
    {
        internal readonly List<(EddnPreparedMessage prepared, UploadPayloadHeader header)> messages = [];
        internal readonly TaskCompletionSource enqueueAttempt = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private long generation = 1;
        private bool enabled = true;

        public bool tryBeginIngestion(out long generation)
        {
            generation = this.generation;
            return enabled;
        }

        public bool tryEnqueue(
            EddnPreparedMessage prepared,
            UploadPayloadHeader header,
            long expectedGeneration)
        {
            enqueueAttempt.TrySetResult();
            if (!enabled || expectedGeneration != generation) return false;
            messages.Add((
                prepared with { message = new JObject(prepared.message) },
                header.clone()));
            return true;
        }

        internal void invalidate()
        {
            generation++;
            enabled = false;
        }
    }

    private sealed class BlockingBeginSink : IEddnSessionSink
    {
        internal readonly TaskCompletionSource entered = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource release = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        internal int enqueueAttempts;

        public bool tryBeginIngestion(out long generation)
        {
            generation = 1;
            entered.TrySetResult();
            release.Task.GetAwaiter().GetResult();
            return true;
        }

        public bool tryEnqueue(
            EddnPreparedMessage prepared,
            UploadPayloadHeader header,
            long expectedGeneration)
        {
            enqueueAttempts++;
            return true;
        }
    }
}
