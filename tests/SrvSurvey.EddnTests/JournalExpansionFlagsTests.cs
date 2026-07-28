using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using Xunit;

namespace SrvSurvey.net;

public sealed class JournalExpansionFlagsTests
{
    [Fact]
    public void MissingOdysseyFlagRemainsUnknown()
    {
        var flags = JournalExpansionFlags.fromLoadGame(
            JObject.Parse("""{"event":"LoadGame","Horizons":true}"""));

        Assert.True(flags.horizons);
        Assert.Null(flags.odyssey);
    }

    [Fact]
    public void ExplicitExpansionFlagsPreserveTheirBooleanValues()
    {
        var flags = JournalExpansionFlags.fromLoadGame(
            JObject.Parse("""{"event":"LoadGame","Horizons":false,"Odyssey":false}"""));

        Assert.False(flags.horizons);
        Assert.False(flags.odyssey);
    }

    [Fact]
    public void NonBooleanExpansionFlagsAreNotInvented()
    {
        var flags = JournalExpansionFlags.fromLoadGame(
            JObject.Parse("""{"event":"LoadGame","Horizons":"true","Odyssey":null}"""));

        Assert.Null(flags.horizons);
        Assert.Null(flags.odyssey);
    }
}
