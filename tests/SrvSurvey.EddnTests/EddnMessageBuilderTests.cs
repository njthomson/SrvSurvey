using Newtonsoft.Json.Linq;
using Xunit;

namespace SrvSurvey.net;

public sealed class EddnMessageBuilderTests
{
    [Fact]
    public void CodexMessageUsesItsDedicatedSchemaAndElidesPersonalData()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10,"Name":"$Codex_Ent_Bacterial_01_Name;","Name_Localised":"Bacterium","Region":"$Codex_RegionName_18;","Category":"$Codex_Category_Biology;","SubCategory":"$Codex_SubCategory_Organic_Structures;","IsNewEntry":true,"NewTraitsDiscovered":true,"Traits":["$Codex_Ent_Trait_Name;"]}
            """);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(statusBody: "Test A 1", trackedBody: "Test A 1", trackedBodyId: 4),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.NotNull(prepared);
        Assert.Equal("https://eddn.edcd.io/schemas/codexentry/1", prepared.schemaRef);
        Assert.Equal("Test A", prepared.message.Value<string>("System"));
        Assert.Null(prepared.message["StarSystem"]);
        Assert.Null(prepared.message["Name_Localised"]);
        Assert.Null(prepared.message["IsNewEntry"]);
        Assert.Null(prepared.message["NewTraitsDiscovered"]);
        Assert.Equal("Test A 1", prepared.message.Value<string>("BodyName"));
        Assert.Equal(4, prepared.message.Value<int>("BodyID"));
        Assert.Equal([1.5, -2, 3], prepared.message["StarPos"]!.Values<double>());
        Assert.True(prepared.message.Value<bool>("odyssey"));
        Assert.True(prepared.message.Value<bool>("horizons"));
    }

    [Fact]
    public void CodexBodyContextRequiresStatusAndTrackedBodyAgreement()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10}
            """);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(statusBody: "Test A 2", trackedBody: "Test A 1", trackedBodyId: 4),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Null(prepared!.message["BodyName"]);
        Assert.Null(prepared.message["BodyID"]);
    }

    [Fact]
    public void CodexMessagePreservesJournalBodyIdentityOverCurrentContext()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10,"BodyName":"Test A 9","BodyID":9}
            """);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(statusBody: "Test A 1", trackedBody: "Test A 1", trackedBodyId: 4),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Equal("Test A 9", prepared!.message.Value<string>("BodyName"));
        Assert.Equal(9, prepared.message.Value<int>("BodyID"));
    }

    [Fact]
    public void CodexMessageDoesNotCombineConflictingJournalAndContextBodyIdentity()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10,"BodyName":"Test A 9"}
            """);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(statusBody: "Test A 1", trackedBody: "Test A 1", trackedBodyId: 4),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Equal("Test A 9", prepared!.message.Value<string>("BodyName"));
        Assert.Null(prepared.message["BodyID"]);
    }

    [Fact]
    public void CodexRejectsInvalidTraitsAndStaleLocation()
    {
        var invalidTrait = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"CodexEntry","System":"Test A","SystemAddress":123,"EntryID":10,"Traits":[""]}
            """);
        Assert.False(EddnMessageSanitizer.tryBuildJournal(
            invalidTrait,
            context(),
            out _,
            out var traitReason));
        Assert.Contains("Traits", traitReason);

        var stale = new JObject(invalidTrait)
        {
            ["Traits"] = new JArray("$Codex_Ent_Trait_Name;"),
            ["SystemAddress"] = 999,
        };
        Assert.False(EddnMessageSanitizer.tryBuildJournal(
            stale,
            context(),
            out _,
            out var locationReason));
        Assert.Contains("tracked system", locationReason);
    }

    public static TheoryData<string> GenericEvents => new()
    {
        "Docked",
        "FSDJump",
        "CarrierJump",
        "Scan",
        "Location",
        "SAASignalsFound",
    };

    [Theory]
    [MemberData(nameof(GenericEvents))]
    public void GenericJournalEventsUseTheJournalSchema(string eventName)
    {
        var raw = new JObject
        {
            ["timestamp"] = "2026-07-28T12:00:00Z",
            ["event"] = eventName,
            ["SystemAddress"] = 123,
        };
        if (eventName != "SAASignalsFound") raw["StarSystem"] = "Test A";
        if (eventName is "FSDJump" or "CarrierJump" or "Location")
            raw["StarPos"] = new JArray(1.5, -2, 3);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Equal("https://eddn.edcd.io/schemas/journal/1", prepared!.schemaRef);
        Assert.Equal("Test A", prepared.message.Value<string>("StarSystem"));
        Assert.Equal([1.5, -2, 3], prepared.message["StarPos"]!.Values<double>());
    }

    [Fact]
    public void GenericJournalMessageRemovesSchemaAndPrivacyFieldsRecursively()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"FSDJump","StarSystem":"Test A","StarPos":[1.5,-2,3],"SystemAddress":123,"FuelLevel":20,"Wanted":false,"Factions":[{"Name":"Test Faction","Name_Localised":"Localized","MyReputation":75,"HomeSystem":"Sol"}]}
            """);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Null(prepared!.message["FuelLevel"]);
        Assert.Null(prepared.message["Wanted"]);
        var faction = Assert.IsType<JObject>(prepared.message["Factions"]![0]);
        Assert.Equal("Test Faction", faction.Value<string>("Name"));
        Assert.Null(faction["Name_Localised"]);
        Assert.Null(faction["MyReputation"]);
        Assert.Null(faction["HomeSystem"]);
    }

    [Fact]
    public void UnknownExpansionFlagsAreOmittedFromTheMessage()
    {
        var raw = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"Location","StarSystem":"Test A","StarPos":[1.5,-2,3],"SystemAddress":123}
            """);

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            new EddnMessageContext(location(), horizons: true, odyssey: null),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.True(prepared!.message.Value<bool>("horizons"));
        Assert.Null(prepared.message["odyssey"]);
    }

    public static TheoryData<string, string, string> DedicatedJournalEvents => new()
    {
        {
            "FSSDiscoveryScan",
            "fssdiscoveryscan/1",
            "{\"SystemName\":\"Test A\",\"SystemAddress\":123,\"BodyCount\":4,\"NonBodyCount\":2,\"Progress\":0.5}"
        },
        {
            "NavBeaconScan",
            "navbeaconscan/1",
            "{\"SystemAddress\":123,\"NumBodies\":4}"
        },
        {
            "ScanBaryCentre",
            "scanbarycentre/1",
            "{\"StarSystem\":\"Test A\",\"SystemAddress\":123,\"BodyID\":4}"
        },
        {
            "FSSAllBodiesFound",
            "fssallbodiesfound/1",
            "{\"SystemName\":\"Test A\",\"SystemAddress\":123,\"Count\":4}"
        },
        {
            "FSSBodySignals",
            "fssbodysignals/1",
            "{\"SystemAddress\":123,\"BodyID\":4,\"BodyName\":\"Test A 1\",\"Signals\":[{\"Type\":\"$SAA_SignalType_Biological;\",\"Type_Localised\":\"Biological\",\"Count\":2}]}"
        },
        {
            "ApproachSettlement",
            "approachsettlement/1",
            "{\"SystemAddress\":123,\"Name\":\"Test Base\",\"BodyID\":4,\"BodyName\":\"Test A 1\",\"Latitude\":1.25,\"Longitude\":-2.5}"
        },
        {
            "DockingDenied",
            "dockingdenied/1",
            "{\"MarketID\":42,\"StationName\":\"Test Port\",\"Reason\":\"NoSpace\"}"
        },
        {
            "DockingGranted",
            "dockinggranted/1",
            "{\"MarketID\":42,\"StationName\":\"Test Port\",\"LandingPad\":3}"
        },
    };

    [Theory]
    [MemberData(nameof(DedicatedJournalEvents))]
    public void DedicatedJournalEventsUseTheirCurrentSchemas(
        string eventName,
        string schema,
        string fields)
    {
        var raw = JObject.Parse(fields);
        raw["timestamp"] = "2026-07-28T12:00:00Z";
        raw["event"] = eventName;

        var built = EddnMessageSanitizer.tryBuildJournal(
            raw,
            context(),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Equal("https://eddn.edcd.io/schemas/" + schema, prepared!.schemaRef);
        Assert.DoesNotContain(
            prepared.message.DescendantsAndSelf().OfType<JProperty>(),
            property => property.Name.EndsWith("_Localised", StringComparison.Ordinal));
        if (eventName == "FSSDiscoveryScan") Assert.Null(prepared.message["Progress"]);
    }

    [Fact]
    public void MarketFileMapsCanonicalPublicCommodityDataIncludingAnEmptyMarket()
    {
        var market = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"Market","MarketID":42,"StationName":"Test Port","StationType":"FleetCarrier","StarSystem":"Test A","CarrierDockingAccess":"all","Items":[{"id":1,"Name":"$gold_name;","Name_Localised":"Gold","Category":"$MARKET_category_metals;","MeanPrice":100,"BuyPrice":90,"Stock":10,"StockBracket":2,"SellPrice":80,"Demand":0,"DemandBracket":0,"Producer":true},{"Name":"illegal","Legality":"Illegal","MeanPrice":1,"BuyPrice":1,"Stock":1,"StockBracket":1,"SellPrice":1,"Demand":1,"DemandBracket":1},{"Name":"hidden","Category":"NonMarketable","MeanPrice":1,"BuyPrice":1,"Stock":1,"StockBracket":1,"SellPrice":1,"Demand":1,"DemandBracket":1}]}
            """);

        var built = EddnMessageSanitizer.tryBuildCompanion(
            market,
            context(),
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Equal("https://eddn.edcd.io/schemas/commodity/3", prepared!.schemaRef);
        var commodity = Assert.IsType<JObject>(Assert.Single((JArray)prepared.message["commodities"]!));
        Assert.Equal("gold", commodity.Value<string>("name"));
        Assert.Null(commodity["id"]);
        Assert.Equal(["Producer"], commodity["statusFlags"]!.Values<string>());
        Assert.Contains(
            "\"timestamp\":\"2026-07-28T12:00:00Z\"",
            prepared.message.ToString(Newtonsoft.Json.Formatting.None));

        market["Items"] = new JArray();
        Assert.True(EddnMessageSanitizer.tryBuildCompanion(
            market,
            context(),
            out var empty,
            out reason), reason);
        Assert.Empty((JArray)empty!.message["commodities"]!);
    }

    [Fact]
    public void OutfittingAndShipyardFilesFilterAndDeduplicatePublicNames()
    {
        var outfitting = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"Outfitting","MarketID":42,"StationName":"Test Port","StarSystem":"Test A","Horizons":true,"Items":[{"Name":"int_fighterbay_size5_class1"},{"Name":"INT_FIGHTERBAY_SIZE5_CLASS1"},{"Name":"hpt_beamlaser_fixed_small"},{"Name":"int_planetapproachsuite"},{"Name":"cockpit_livery"},{"Name":"Int_DroneControl_Collection_Size1_Class1","SKU":"ODYSSEY"}]}
            """);
        Assert.True(EddnMessageSanitizer.tryBuildCompanion(
            outfitting,
            context(),
            out var modules,
            out var reason), reason);
        Assert.Equal(
            ["Hpt_beamlaser_fixed_small", "Int_fighterbay_size5_class1"],
            modules!.message["modules"]!.Values<string>());
        Assert.True(modules.message.Value<bool>("horizons"));

        var shipyard = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"Shipyard","MarketID":42,"StationName":"Test Port","StarSystem":"Test A","Horizons":true,"AllowCobraMkIV":false,"PriceList":[{"ShipType":"Krait_MkII"},{"ShipType":"krait_mkii"},{"ShipType":"adder"}]}
            """);
        Assert.True(EddnMessageSanitizer.tryBuildCompanion(
            shipyard,
            context(),
            out var ships,
            out reason), reason);
        Assert.Equal(["Krait_MkII", "adder"], ships!.message["ships"]!.Values<string>());
        Assert.False(ships.message.Value<bool>("allowCobraMkIV"));
    }

    [Fact]
    public void FleetCarrierMaterialsAndNavRouteUseCompanionFileContents()
    {
        var materials = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"FCMaterials","MarketID":42,"CarrierName":"TEST CARRIER","CarrierID":"ABC-123","Items":[{"id":1,"Name":"$memorychip_name;","Name_Localised":"Memory Chip","Price":600,"Stock":5,"Demand":0}]}
            """);
        Assert.True(EddnMessageSanitizer.tryBuildCompanion(
            materials,
            context(),
            out var fc,
            out var reason), reason);
        Assert.Equal("https://eddn.edcd.io/schemas/fcmaterials_journal/1", fc!.schemaRef);
        Assert.Null(fc.message["Items"]![0]!["Name_Localised"]);

        var route = JObject.Parse(
            """
            {"timestamp":"2026-07-28T12:00:00Z","event":"NavRoute","Route":[{"StarSystem":"Test A","SystemAddress":123,"StarPos":[1.5,-2,3],"StarClass":"K","Unexpected":"removed"}]}
            """);
        Assert.True(EddnMessageSanitizer.tryBuildCompanion(
            route,
            context(),
            out var nav,
            out reason), reason);
        Assert.Equal("https://eddn.edcd.io/schemas/navroute/1", nav!.schemaRef);
        Assert.Null(nav.message["Route"]![0]!["Unexpected"]);

        route["Route"] = new JArray();
        Assert.True(EddnMessageSanitizer.tryBuildCompanion(
            route,
            context(),
            out var cleared,
            out reason), reason);
        Assert.Empty((JArray)cleared!.message["Route"]!);
    }

    [Fact]
    public void SignalBatchDropsMissionSignalsAndPrivateOrUnsupportedFields()
    {
        var pending = new[]
        {
            JObject.Parse(
                """
                {"timestamp":"2026-07-28T12:00:00Z","event":"FSSSignalDiscovered","SystemAddress":123,"SignalName":"$MULTIPLAYER_SCENARIO42_TITLE;","SignalName_Localised":"Signal","SignalType":"FleetCarrier","TimeRemaining":900}
                """),
            JObject.Parse(
                """
                {"timestamp":"2026-07-28T12:00:01Z","event":"FSSSignalDiscovered","SystemAddress":123,"SignalName":"Mission","USSType":"$USS_Type_MissionTarget;"}
                """),
            JObject.Parse(
                """
                {"timestamp":"2026-07-28T12:00:02Z","event":"FSSSignalDiscovered","SystemAddress":999,"SignalName":"Wrong system"}
                """),
        };

        var built = EddnMessageSanitizer.tryBuildSignalBatch(
            pending,
            location(),
            horizons: true,
            odyssey: true,
            out var prepared,
            out var reason);

        Assert.True(built, reason);
        Assert.Equal("https://eddn.edcd.io/schemas/fsssignaldiscovered/1", prepared!.schemaRef);
        var signal = Assert.IsType<JObject>(Assert.Single((JArray)prepared.message["signals"]!));
        Assert.Equal("$MULTIPLAYER_SCENARIO42_TITLE;", signal.Value<string>("SignalName"));
        Assert.Null(signal["SignalName_Localised"]);
        Assert.Null(signal["TimeRemaining"]);
        Assert.Null(signal["SystemAddress"]);
        Assert.Null(signal["event"]);
        Assert.Contains(
            "\"timestamp\":\"2026-07-28T12:00:00Z\"",
            prepared.message.ToString(Newtonsoft.Json.Formatting.None));
    }

    private static EddnMessageContext context(
        string? statusBody = null,
        string? trackedBody = null,
        int? trackedBodyId = null)
    {
        return new EddnMessageContext(
            location(),
            horizons: true,
            odyssey: true,
            statusBody,
            trackedBody,
            trackedBodyId,
            trackedBodyId.HasValue ? "Planet" : null);
    }

    private static EddnLocationContext location()
    {
        return new EddnLocationContext("Test A", 123, [1.5, -2, 3]);
    }
}
