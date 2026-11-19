using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using Xunit;

namespace SrvSurvey.Tests;

public sealed class GuardianMapMarkerOffsetTests
{
    [Fact]
    public void CalculatesSamePortableMapOffsetAsAvalonia()
    {
        var offset = GuardianMapMarkerOffsetCalculator.calculate(
            new LatLong2(0d, 0d),
            new LatLong2(0d, 90d),
            siteHeading: 0,
            planetRadiusMeters: 100);

        Assert.Equal(-Math.PI * 50, offset.x, precision: 8);
        Assert.Equal(0, offset.y, precision: 8);
    }

    [Fact]
    public void RotatesPortableOffsetIntoLegacySurfaceCoordinates()
    {
        var offset = GuardianMapMarkerOffsetCalculator.toSurfaceCoordinates(
            new GuardianMapMarkerOffset(0, 10),
            siteHeading: 90);

        Assert.Equal(10, offset.X, precision: 6);
        Assert.Equal(0, offset.Y, precision: 6);
    }

    [Fact]
    public void RepeatedCorrectionKeepsTheOriginalMapAlignment()
    {
        const double radius = 1_000_000;
        var original = new LatLong2(10d, 20d);
        var firstCorrection = new LatLong2(10.01, 20.02);
        var secondCorrection = new LatLong2(10.02, 20.03);
        var survey = createSurvey(original);

        Assert.True(survey.correctMapAlignment(firstCorrection, radius));
        Assert.True(survey.correctMapAlignment(secondCorrection, radius));

        var expected = GuardianMapMarkerOffsetCalculator.calculate(
            original,
            secondCorrection,
            survey.siteHeading,
            radius);
        Assert.InRange(
            Math.Abs(expected.x - survey.mapMarkerOffset.x),
            0,
            0.1);
        Assert.InRange(
            Math.Abs(expected.y - survey.mapMarkerOffset.y),
            0,
            0.1);
        Assert.Equal(secondCorrection, survey.location);
    }

    [Fact]
    public void HeadingCorrectionKeepsTheSurfaceOffsetFixed()
    {
        const double radius = 1_000_000;
        var survey = createSurvey(new LatLong2(10d, 20d));
        survey.correctMapAlignment(new LatLong2(10.01, 20.02), radius);
        var originalSurfaceOffset = GuardianMapMarkerOffsetCalculator
            .toSurfaceCoordinates(
                survey.mapMarkerOffset,
                survey.siteHeading);

        Assert.True(survey.correctSiteHeading(123));

        var correctedSurfaceOffset = GuardianMapMarkerOffsetCalculator
            .toSurfaceCoordinates(
                survey.mapMarkerOffset,
                survey.siteHeading);
        Assert.InRange(
            Math.Abs(originalSurfaceOffset.X - correctedSurfaceOffset.X),
            0,
            0.1);
        Assert.InRange(
            Math.Abs(originalSurfaceOffset.Y - correctedSurfaceOffset.Y),
            0,
            0.1);
    }

    [Fact]
    public void SurveyJsonUsesAvaloniaCompatibleTopLevelMetadata()
    {
        var survey = createSurvey(new LatLong2(12.5, -44.25));
        survey.mapMarkerOffset = new GuardianMapMarkerOffset(6.5, -3.25);
        survey.localSiteId = 17;
        survey.catalogBodyName = "Test System 3";
        survey.starPos = [1.25, -2.5, 3.75];
        survey.distanceToArrival = 456.5;

        var json = JsonConvert.SerializeObject(survey);
        var root = JObject.Parse(json);

        var metadata = Assert.IsType<JObject>(root["mapMarkerOffset"]);
        Assert.Equal(6.5, metadata["x"]!.Value<double>());
        Assert.Equal(-3.25, metadata["y"]!.Value<double>());
        Assert.Equal(17, root["localSiteId"]!.Value<int>());
        Assert.Equal("Test System 3", root["catalogBodyName"]!.Value<string>());
        Assert.Equal(3, Assert.IsType<JArray>(root["starPos"]).Count);
        Assert.Equal(456.5, root["distanceToArrival"]!.Value<double>());
        Assert.Null(root["body"]);

        var roundTrip = JsonConvert.DeserializeObject<GuardianSiteData>(json)!;
        Assert.Equal(survey.mapMarkerOffset, roundTrip.mapMarkerOffset);
        Assert.Equal(survey.localSiteId, roundTrip.localSiteId);
        Assert.Equal(survey.catalogBodyName, roundTrip.catalogBodyName);
        Assert.Equal(survey.starPos, roundTrip.starPos);
        Assert.Equal(survey.distanceToArrival, roundTrip.distanceToArrival);
    }

    [Fact]
    public void SurveyJsonWithoutOffsetRemainsCompatible()
    {
        var survey = createSurvey(new LatLong2(12.5, -44.25));
        var json = JsonConvert.SerializeObject(survey);

        Assert.DoesNotContain("mapMarkerOffset", json);

        var roundTrip = JsonConvert.DeserializeObject<GuardianSiteData>(json)!;
        Assert.True(roundTrip.mapMarkerOffset.isEmpty);
    }

    private static GuardianSiteData createSurvey(LatLong2 location)
    {
        return new GuardianSiteData
        {
            name = "$Ancient_Tiny_001:#index=1;",
            nameLocalised = "Guardian Structure",
            commander = "Test Commander",
            firstVisited = DateTimeOffset.Parse("2026-08-31T12:00:00Z"),
            lastVisited = DateTimeOffset.Parse("2026-08-31T12:00:00Z"),
            type = GuardianSiteData.SiteType.Unknown,
            index = 1,
            location = location,
            systemAddress = 42,
            systemName = "Test System",
            bodyId = 3,
            bodyName = "Test System 3",
            siteHeading = 37,
            relicTowerHeading = -1,
            notes = string.Empty,
        };
    }
}
