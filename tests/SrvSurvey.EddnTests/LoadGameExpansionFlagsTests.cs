using Newtonsoft.Json;
using SrvSurvey.game;
using Xunit;

namespace SrvSurvey.net;

public sealed class LoadGameExpansionFlagsTests
{
    [Fact]
    public void MissingExpansionFlagsRemainUnknown()
    {
        var loadGame = JsonConvert.DeserializeObject<LoadGame>(
            """{"event":"LoadGame","Commander":"Test"}""");

        Assert.NotNull(loadGame);
        Assert.Null(loadGame.Horizons);
        Assert.Null(loadGame.Odyssey);
    }

    [Fact]
    public void ExplicitExpansionFlagsPreserveTheirBooleanValues()
    {
        var loadGame = JsonConvert.DeserializeObject<LoadGame>(
            """{"event":"LoadGame","Horizons":false,"Odyssey":true}""");

        Assert.NotNull(loadGame);
        Assert.False(loadGame.Horizons);
        Assert.True(loadGame.Odyssey);
    }
}
