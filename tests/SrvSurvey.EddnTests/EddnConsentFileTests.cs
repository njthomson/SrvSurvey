using Xunit;

namespace SrvSurvey.net;

public sealed class EddnConsentFileTests
{
    [Fact]
    public void MissingSettingsFileIsTreatedAsOptOut()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "SrvSurvey-EddnTests-" + Guid.NewGuid().ToString("N"),
            "settings.json");

        Assert.True(EddnConsentFile.tryRead(path, out var enabled, out var error));
        Assert.False(enabled);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("{\"eddnUploadEnabled\":true}", true)]
    [InlineData("{\"eddnUploadEnabled\":false}", false)]
    [InlineData("{}", false)]
    public void ReadsExplicitApplicationWideConsent(string json, bool expected)
    {
        using var file = new TemporarySettingsFile(json);

        Assert.True(EddnConsentFile.tryRead(file.path, out var enabled, out var error));
        Assert.Equal(expected, enabled);
        Assert.Null(error);
    }

    [Fact]
    public void MalformedSettingsFailClosedWithoutClaimingOptOut()
    {
        using var file = new TemporarySettingsFile("{");

        Assert.False(EddnConsentFile.tryRead(file.path, out var enabled, out var error));
        Assert.False(enabled);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Theory]
    [InlineData("{\"eddnUploadEnabled\":\"yes\"}")]
    [InlineData("{\"eddnUploadEnabled\":{}}")]
    [InlineData("{\"eddnUploadEnabled\":null}")]
    public void NonBooleanConsentFailsClosedWithoutThrowing(string json)
    {
        using var file = new TemporarySettingsFile(json);

        Assert.False(EddnConsentFile.tryRead(file.path, out var enabled, out var error));
        Assert.False(enabled);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    private sealed class TemporarySettingsFile : IDisposable
    {
        internal readonly string path = Path.Combine(
            Path.GetTempPath(),
            "SrvSurvey-EddnTests-" + Guid.NewGuid().ToString("N"),
            "settings.json");

        internal TemporarySettingsFile(string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public void Dispose()
        {
            var folder = Path.GetDirectoryName(path)!;
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }
}
