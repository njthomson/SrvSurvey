using Newtonsoft.Json.Linq;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnSettingsMigrationTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("live", false)]
    [InlineData("unexpected", false)]
    [InlineData(" BETA ", true)]
    [InlineData("DEV", true)]
    public void LegacyEnvironmentPreservesOnlyTestIntent(
        string? environment,
        bool expected)
    {
        var settings = new JObject();
        if (environment is not null)
            settings["eddnEnvironment"] = environment;

        Assert.Equal(expected, EddnSettingsMigration.useTestSchemas(settings));
    }

    [Theory]
    [InlineData(false, "dev")]
    [InlineData(true, "live")]
    public void ReplacementSettingWinsOverLegacyEnvironment(
        bool useTestSchemas,
        string environment)
    {
        var settings = new JObject
        {
            ["eddnUseTestSchemas"] = useTestSchemas,
            ["eddnEnvironment"] = environment,
        };

        Assert.Equal(
            useTestSchemas,
            EddnSettingsMigration.useTestSchemas(settings));
    }

    [Fact]
    public void MalformedReplacementFailsBackToLiveSchemas()
    {
        var settings = new JObject
        {
            ["eddnUseTestSchemas"] = "not-a-boolean",
            ["eddnEnvironment"] = "dev",
        };

        Assert.False(EddnSettingsMigration.useTestSchemas(settings));
    }
}
