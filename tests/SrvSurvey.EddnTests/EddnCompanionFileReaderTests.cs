using Newtonsoft.Json.Linq;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnCompanionFileReaderTests
{
    [Fact]
    public async Task ReadsTheMatchingCurrentCompanionFile()
    {
        using var folder = new TemporaryFolder();
        await File.WriteAllTextAsync(
            Path.Combine(folder.path, "Market.json"),
            """
            {"timestamp":"2026-07-28T12:00:01Z","event":"Market","MarketID":42,"Items":[]}
            """);
        var notification = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"Market","MarketID":42}
            """);

        var result = await EddnCompanionFileReader.read(
            folder.path,
            notification,
            retrySchedule: []);

        Assert.True(result.isSuccess, result.error);
        Assert.Equal(42, result.content!.Value<long>("MarketID"));
    }

    [Theory]
    [InlineData(
        "{\"timestamp\":\"2026-07-28T12:00:01Z\",\"event\":\"Market\",\"MarketID\":99,\"Items\":[]}",
        "MarketID")]
    [InlineData(
        "{\"timestamp\":\"2026-07-28T11:59:59Z\",\"event\":\"Market\",\"MarketID\":42,\"Items\":[]}",
        "older")]
    [InlineData(
        "{\"timestamp\":\"2026-07-28T12:00:01Z\",\"event\":\"Shipyard\",\"MarketID\":42,\"Items\":[]}",
        "different event")]
    public async Task RejectsAStaleOrMismatchedFile(string file, string expectedError)
    {
        using var folder = new TemporaryFolder();
        await File.WriteAllTextAsync(Path.Combine(folder.path, "Market.json"), file);
        var notification = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"Market","MarketID":42}
            """);

        var result = await EddnCompanionFileReader.read(
            folder.path,
            notification,
            retrySchedule: []);

        Assert.False(result.isSuccess);
        Assert.Contains(expectedError, result.error);
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
