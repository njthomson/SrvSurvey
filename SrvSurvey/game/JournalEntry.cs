using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;

namespace SrvSurvey
{
    // See details from:
    // https://elite-journal.readthedocs.io/en/latest/

    class JournalEntry : IJournalEntry
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset timestamp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string @event { get; set; }
    }

    class LocationEntry : JournalEntry, ILocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    class Fileheader : JournalEntry
    {
        // { "timestamp":"2023-01-05T23:38:49Z", "event":"Fileheader", "part":1, "language":"English/UK", "Odyssey":true, "gameversion":"4.0.0.1476", "build":"r289925/r0 " }

        public int part { get; set; }
        public string language { get; set; }
        public bool Odyssey { get; set; }
        public string gameversion { get; set; }
        public string build { get; set; }
    }

    class Commander : JournalEntry
    {
        // { "timestamp":"2023-01-05T23:40:11Z", "event":"Commander", "FID":"F10171085", "Name":"GRINNING2002" }

        public string FID { get; set; }
        public string Name { get; set; }
    }

    class LoadGame : JournalEntry
    {
        // { "timestamp":"2023-03-07T06:38:34Z", "event":"LoadGame", "FID":"F10171085", "Commander":"GRINNING2002", "Horizons":true, "Odyssey":true, "Ship":"Dolphin", "ShipID":24, "ShipName":"", "ShipIdent":"", "FuelLevel":15.500000, "FuelCapacity":16.000000, "StartLanded":true, "GameMode":"Solo", "Credits":1597404741, "Loan":0, "language":"English/UK", "gameversion":"4.0.0.1477", "build":"r291050/r0 " }

        public string FID { get; set; }
        public string Commander { get; set; }
        public bool Horizons { get; set; }
        public bool Odyssey { get; set; }
        public string Ship { get; set; }
        public string? Ship_Localised { get; set; }
        public int ShipID { get; set; }
        public string ShipName { get; set; }
        public string ShipIdent { get; set; }
        public double FuelLevel { get; set; }
        public double FuelCapacity { get; set; }
        public bool StartLanded { get; set; }
        public string GameMode { get; set; }
        public double Credits { get; set; }
        public double Loan { get; set; }
        public string language { get; set; }
        public string gameversion { get; set; }
        public string build { get; set; }
    }

    class Location : LocationEntry, IFactions, ISystemAddress, ISystemDataStarter, IStarRef
    {
        // { "timestamp":"2023-01-11T03:44:33Z", "event":"Location", "Latitude":-15.325527, "Longitude":6.507746, "DistFromStarLS":513.174241, "Docked":false, "InSRV":true, "StarSystem":"Col 173 Sector JX-K b24-0", "SystemAddress":684107179361, "StarPos":[993.06250,-188.18750,-173.53125], "SystemAllegiance":"Guardian", "SystemEconomy":"$economy_None;", "SystemEconomy_Localised":"None", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", "SystemGovernment":"$government_None;", "SystemGovernment_Localised":"None", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "SystemSecurity_Localised":"Anarchy", "Population":0, "Body":"Col 173 Sector JX-K b24-0 B 4", "BodyID":26, "BodyType":"Planet" }
        // { "timestamp":"2024-04-10T03:18:13Z", "event":"Location", "DistFromStarLS":18.841613, "Docked":true, "StationName":"Oyekan Prospecting Hub", "StationType":"OnFootSettlement", "MarketID":3888520448, "StationFaction":{ "Name":"Yaurnai Jet Hand Gang" }, "StationGovernment":"$government_Anarchy;", "StationGovernment_Localised":"Anarchy", "StationServices":[ "dock", "autodock", "blackmarket", "commodities", "contacts", "missions", "refuel", "repair", "engineer", "missionsgenerated", "facilitator", "flightcontroller", "stationoperations", "stationMenu" ], "StationEconomy":"$economy_Extraction;", "StationEconomy_Localised":"Extraction", "StationEconomies":[ { "Name":"$economy_Extraction;", "Name_Localised":"Extraction", "Proportion":1.000000 } ], "Taxi":false, "Multicrew":false, "StarSystem":"Yaurnai", "SystemAddress":669612713401, "StarPos":[-99.65625,116.56250,43.40625], "SystemAllegiance":"Independent", "SystemEconomy":"$economy_Extraction;", "SystemEconomy_Localised":"Extraction", "SystemSecondEconomy":"$economy_Industrial;", "SystemSecondEconomy_Localised":"Industrial", "SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Corporate", "SystemSecurity":"$SYSTEM_SECURITY_medium;", "SystemSecurity_Localised":"Medium Security", "Population":3143129, "Body":"Yaurnai 1", "BodyID":8, "BodyType":"Planet", "Factions":[ { "Name":"Yaurnai Independent Bridge", "FactionState":"None", "Government":"Patronage", "Influence":0.158052, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":2.970000 }, { "Name":"Stardreamer Systems", "FactionState":"None", "Government":"Corporate", "Influence":0.031809, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":100.000000 }, { "Name":"Yaurnai Partners", "FactionState":"None", "Government":"Corporate", "Influence":0.169980, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":1.980000 }, { "Name":"Raven Colonial Corporation", "FactionState":"Expansion", "Government":"Corporate", "Influence":0.471173, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "SquadronFaction":true, "MyReputation":100.000000, "ActiveStates":[ { "State":"Expansion" } ] }, { "Name":"Movement for Yaurnai Liberals", "FactionState":"None", "Government":"Democracy", "Influence":0.119284, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":4.620000 }, { "Name":"Yaurnai Jet Hand Gang", "FactionState":"None", "Government":"Anarchy", "Influence":0.049702, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":5.940000, "RecoveringStates":[ { "State":"PirateAttack", "Trend":0 } ] } ], "SystemFaction":{ "Name":"Raven Colonial Corporation", "FactionState":"Expansion" } }

        public double DistFromStarLS { get; set; }
        public bool Docked { get; set; }
        public bool InSrv { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public double[] StarPos { get; set; }
        public string SystemAllegiance { get; set; }
        public string SystemEconomy { get; set; }
        public string SystemEconomy_Localised { get; set; }
        public string SystemSecondEconomy { get; set; }
        public string SystemSecondEconomy_Localised { get; set; }
        public string SystemGovernment { get; set; }
        public string SystemGovernment_Localised { get; set; }
        public string SystemSecurity { get; set; }
        public string SystemSecurity_Localised { get; set; }
        public double Population { get; set; }
        public string Body { get; set; }
        public int BodyID { get; set; }
        public FSDJumpBodyType BodyType { get; set; }

        public string StationName;
        public StationType StationType;

        public long MarketID;
        // StationFaction ?
        public string? StationGovernment;
        public string? StationGovernment_Localised;
        public List<string>? StationServices;
        public string? StationEconomy;
        public string? StationEconomy_Localised;
        public List<SystemFaction>? Factions { get; set; }
        // StationEconomies ?
        public NamedFaction SystemFaction;
    }

    class Loadout : JournalEntry
    {
        // { "timestamp":"2024-04-15T21:09:12Z", "event":"Loadout", "Ship":"dolphin", "ShipID":35, "ShipName":"delphini", "ShipIdent":"GR-02D", "HullValue":1337323, "ModulesValue":17298866, "HullHealth":1.000000, "UnladenMass":182.292557, "CargoCapacity":16, "MaxJumpRange":56.538074, "FuelCapacity":{ "Main":16.000000, "Reserve":0.500000 }, "Rebuy":931810, "Modules":[ { "Slot":"TinyHardpoint2", "Item":"hpt_plasmapointdefence_turret_tiny", "On":true, "Priority":0, "AmmoInClip":12, "AmmoInHopper":10000, "Health":1.000000, "Value":18546, "Engineering":{ "Engineer":"Ram Tah", "EngineerID":300110, "BlueprintID":128731485, "BlueprintName":"Misc_LightWeight", "Level":5, "Quality":0.733000, "Modifiers":[ { "Label":"Mass", "Value":0.088350, "OriginalValue":0.500000, "LessIsGood":1 }, { "Label":"Integrity", "Value":15.000000, "OriginalValue":30.000000, "LessIsGood":0 } ] } }, { "Slot":"PaintJob", "Item":"paintjob_dolphin_ember_white", "On":true, "Priority":1, "Health":1.000000 }, { "Slot":"Armour", "Item":"dolphin_armour_grade1", "On":true, "Priority":1, "Health":1.000000 }, { "Slot":"PowerPlant", "Item":"int_guardianpowerplant_size4", "On":true, "Priority":1, "Health":1.000000, "Value":1517619 }, { "Slot":"MainEngines", "Item":"int_engine_size4_class2", "On":true, "Priority":0, "Health":1.000000, "Value":52329 }, { "Slot":"FrameShiftDrive", "Item":"int_hyperdrive_size4_class5", "On":true, "Priority":0, "Health":1.000000, "Value":1610080, "Engineering":{ "Engineer":"Felicity Farseer", "EngineerID":300100, "BlueprintID":128673694, "BlueprintName":"FSD_LongRange", "Level":5, "Quality":0.631000, "Modifiers":[ { "Label":"Mass", "Value":13.000000, "OriginalValue":10.000000, "LessIsGood":1 }, { "Label":"Integrity", "Value":85.000000, "OriginalValue":100.000000, "LessIsGood":0 }, { "Label":"PowerDraw", "Value":0.517500, "OriginalValue":0.450000, "LessIsGood":1 }, { "Label":"FSDOptimalMass", "Value":794.377502, "OriginalValue":525.000000, "LessIsGood":0 } ] } }, { "Slot":"LifeSupport", "Item":"int_lifesupport_size4_class2", "On":true, "Priority":0, "Health":1.000000, "Value":28373 }, { "Slot":"PowerDistributor", "Item":"int_guardianpowerdistributor_size3", "On":true, "Priority":0, "Health":1.000000, "Value":311364 }, { "Slot":"Radar", "Item":"int_sensors_size3_class2", "On":true, "Priority":0, "Health":1.000000, "Value":10133, "Engineering":{ "Engineer":"Lori Jameson", "EngineerID":300230, "BlueprintID":128740672, "BlueprintName":"Sensor_LightWeight", "Level":4, "Quality":0.819300, "Modifiers":[ { "Label":"Mass", "Value":0.754200, "OriginalValue":2.000000, "LessIsGood":1 }, { "Label":"Integrity", "Value":30.600000, "OriginalValue":51.000000, "LessIsGood":0 }, { "Label":"SensorTargetScanAngle", "Value":24.000000, "OriginalValue":30.000000, "LessIsGood":0 } ] } }, { "Slot":"FuelTank", "Item":"int_fueltank_size4_class3", "On":true, "Priority":1, "Health":1.000000 }, { "Slot":"Slot01_Size5", "Item":"int_fuelscoop_size5_class5", "On":true, "Priority":0, "Health":1.000000, "Value":9164432 }, { "Slot":"Slot02_Size4", "Item":"int_guardianfsdbooster_size4", "On":true, "Priority":1, "Health":1.000000, "Value":3163887 }, { "Slot":"Slot03_Size4", "Item":"int_corrosionproofcargorack_size4_class1", "On":true, "Priority":1, "Health":1.000000, "Value":91970 }, { "Slot":"Slot04_Size3", "Item":"int_shieldgenerator_size3_class2", "On":true, "Priority":0, "Health":1.000000, "Value":16508 }, { "Slot":"Slot05_Size2", "Item":"int_repairer_size2_class5", "On":true, "Priority":0, "Health":1.000000, "Value":1279395 }, { "Slot":"Slot06_Size2", "Item":"int_buggybay_size2_class2", "On":true, "Priority":0, "Health":1.000000, "Value":21060 }, { "Slot":"Slot07_Size2", "Item":"int_detailedsurfacescanner_tiny", "On":true, "Priority":0, "Health":1.000000, "Engineering":{ "Engineer":"Lei Cheung", "EngineerID":300120, "BlueprintID":128740151, "BlueprintName":"Sensor_Expanded", "Level":5, "Quality":1.000000, "Modifiers":[ { "Label":"PowerDraw", "Value":0.000000, "OriginalValue":0.000000, "LessIsGood":1 }, { "Label":"DSS_PatchRadius", "Value":40.000000, "OriginalValue":20.000000, "LessIsGood":0 } ] } }, { "Slot":"Slot08_Size1", "Item":"int_supercruiseassist", "On":true, "Priority":2, "Health":1.000000 }, { "Slot":"Slot09_Size1", "Item":"int_dockingcomputer_advanced", "On":true, "Priority":2, "Health":1.000000, "Value":13170 }, { "Slot":"PlanetaryApproachSuite", "Item":"int_planetapproachsuite_advanced", "On":true, "Priority":1, "Health":1.000000 }, { "Slot":"VesselVoice", "Item":"voicepack_verity", "On":true, "Priority":1, "Health":1.000000 }, { "Slot":"ShipCockpit", "Item":"dolphin_cockpit", "On":true, "Priority":1, "Health":1.000000 }, { "Slot":"CargoHatch", "Item":"modularcargobaydoor", "On":true, "Priority":4, "Health":1.000000 } ] }

        public string Ship;
        public int ShipID;
        public string ShipName;
        public string ShipIDent;
        public double MaxJumpRange;
        public int CargoCapacity;
    }

    class Died : JournalEntry
    {
        // { "timestamp":"2023-11-13T11:24:26Z", "event":"Died" }
    }

    class Resurrect : JournalEntry
    {
        // { "timestamp":"2024-09-22T04:56:50Z", "event":"Resurrect", "Option":"handin", "Cost":5400, "Bankrupt":false }
        public string Option;
        public long Cost;
        public bool Bankrupt;
    }

    class ApproachSettlement : LocationEntry, ISystemAddress
    {
        // { "timestamp":"2023-01-06T00:14:38Z", "event":"ApproachSettlement", "Name":"$Ancient_Tiny_001:#index=1;", "Name_Localised":"Guardian Structure", "SystemAddress":2881788519801, "BodyID":7, "BodyName":"Col 173 Sector IJ-G b27-1 A 1", "Latitude":-33.219879, "Longitude":87.628571 }
        // { "timestamp":"2024-04-09T06:44:33Z", "event":"ApproachSettlement", "Name":"Oyekan Prospecting Hub", "MarketID":3888520448, "StationFaction":{ "Name":"Yaurnai Jet Hand Gang" }, "StationGovernment":"$government_Anarchy;", "StationGovernment_Localised":"Anarchy", "StationServices":[ "dock", "autodock", "blackmarket", "commodities", "contacts", "missions", "refuel", "repair", "engineer", "missionsgenerated", "facilitator", "flightcontroller", "stationoperations", "stationMenu" ], "StationEconomy":"$economy_Extraction;", "StationEconomy_Localised":"Extraction", "StationEconomies":[ { "Name":"$economy_Extraction;", "Name_Localised":"Extraction", "Proportion":1.000000 } ], "SystemAddress":669612713401, "BodyID":8, "BodyName":"Yaurnai 1", "Latitude":47.955894, "Longitude":-107.683449 }
        // { "timestamp":"2024-06-16T21:51:24Z", "event":"ApproachSettlement", "Name":"Omenuko Extraction Base", "MarketID":3928215040, "StationFaction":{ "Name":"Yami & Co" }, "StationGovernment":"$government_Corporate;", "StationGovernment_Localised":"Corporate", "StationAllegiance":"Federation", "StationServices":[ "dock", "autodock", "commodities", "contacts", "missions", "refuel", "repair", "engineer", "missionsgenerated", "facilitator", "flightcontroller", "stationoperations", "searchrescue", "stationMenu" ], "StationEconomy":"$economy_Extraction;", "StationEconomy_Localised":"Extraction", "StationEconomies":[ { "Name":"$economy_Extraction;", "Name_Localised":"Extraction", "Proportion":1.000000 } ], "SystemAddress":2868367467953, "BodyID":5, "BodyName":"Yami A 1", "Latitude":0.550911, "Longitude":-31.254198 }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long SystemAddress { get; set; }
        public int BodyID { get; set; }
        public string BodyName { get; set; }

        public long MarketID;
        public NamedFaction StationFaction;
        public List<string>? StationServices;
        public string? StationGovernment;
        public string? StationGovernment_Localised;
        public string? StationAllegiance;
        public string? StationEconomy;
        public string? StationEconomy_Localised;
        // StationEconomies ?
    }

    class NamedFaction
    {
        public string Name;
        public string? FactionState;
    }

    class DockingRequested : JournalEntry
    {
        // { "timestamp":"2024-02-10T07:10:27Z", "event":"DockingRequested", "MarketID":3888520704, "StationName":"Kvitka Synthetics Workshop", "StationType":"OnFootSettlement", "LandingPads":{ "Small":0, "Medium":0, "Large":1 } }

        public long MarketID;
        public string StationName;
        public StationType StationType;
        public LandingPads LandingPads;
    }

    class DockingDenied : JournalEntry
    {
        // { "timestamp":"2024-05-31T05:32:39Z", "event":"DockingDenied", "Reason":"NoSpace", "MarketID":3928216064, "StationName":"Ndaitwah Manufacturing Hub", "StationType":"OnFootSettlement" }

        public string Reason;
        public long MarketID;
        public string StationName;
    }

    class DockingGranted : JournalEntry
    {
        // { "timestamp":"2024-04-09T06:45:09Z", "event":"DockingGranted", "LandingPad":1, "MarketID":3888520448, "StationName":"Oyekan Prospecting Hub", "StationType":"OnFootSettlement" }

        public int LandingPad;
        public long MarketID;
        public string StationName;
        public StationType StationType;
    }

    class DockingCancelled : JournalEntry
    {
        // { "timestamp":"2024-09-08T05:09:39Z", "event":"DockingCancelled", "MarketID":3928215040, "StationName":"Omenuko Extraction Base", "StationType":"OnFootSettlement" }

        public long MarketID;
        public string StationName;
        public StationType StationType;
    }

    class Docked : JournalEntry
    {
        // { "timestamp":"2024-04-09T06:45:51Z", "event":"Docked", "StationName":"Oyekan Prospecting Hub", "StationType":"OnFootSettlement", "Taxi":false, "Multicrew":false, "StarSystem":"Yaurnai", "SystemAddress":669612713401, "MarketID":3888520448, "StationFaction":{ "Name":"Yaurnai Jet Hand Gang" }, "StationGovernment":"$government_Anarchy;", "StationGovernment_Localised":"Anarchy", "StationServices":[ "dock", "autodock", "blackmarket", "commodities", "contacts", "missions", "refuel", "repair", "engineer", "missionsgenerated", "facilitator", "flightcontroller", "stationoperations", "stationMenu" ], "StationEconomy":"$economy_Extraction;", "StationEconomy_Localised":"Extraction", "StationEconomies":[ { "Name":"$economy_Extraction;", "Name_Localised":"Extraction", "Proportion":1.000000 } ], "DistFromStarLS":18.841609, "LandingPads":{ "Small":1, "Medium":0, "Large":0 } }
        // { "timestamp":"2024-06-28T02:10:59Z", "event":"Docked", "StationName":"Crellin Genetics Installation", "StationType":"OnFootSettlement", "Taxi":false, "Multicrew":false, "StarSystem":"Lacaille 9352", "SystemAddress":11666070709673, "MarketID":3854458880, "StationFaction":{ "Name":"Quam Singulari" }, "StationGovernment":"$government_Democracy;", "StationGovernment_Localised":"Democracy", "StationAllegiance":"Federation", "StationServices":[ "dock", "autodock", "blackmarket", "commodities", "contacts", "exploration", "missions", "refuel", "repair", "engineer", "missionsgenerated", "facilitator", "flightcontroller", "stationoperations", "searchrescue", "stationMenu" ], "StationEconomy":"$economy_HighTech;", "StationEconomy_Localised":"High Tech", "StationEconomies":[ { "Name":"$economy_HighTech;", "Name_Localised":"High Tech", "Proportion":1.000000 } ], "DistFromStarLS":1062.028204, "Wanted":true, "ActiveFine":true, "LandingPads":{ "Small":1, "Medium":0, "Large":0 } }

        public string StationName;
        public StationType StationType;
        public bool Taxi;
        public bool Multicrew;
        public string StarSystem;
        public long SystemAddress;
        public long MarketID;
        // StationFaction ?
        public string? StationGovernment;
        public string? StationGovernment_Localised;
        public List<string>? StationServices;
        public string? StationEconomy;
        public string? StationEconomy_Localised;
        // StationEconomies ?
        public double DistFromStarLS;
        public bool Wanted;
        public bool ActiveFine;
        public LandingPads LandingPads;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum StationType
    {
        Bernal,
        Coriolis,
        Ocellus,
        Orbis,
        Outpost,
        MegaShip,
        FleetCarrier,
        OnFootSettlement,
        CraterOutpost,
        CraterPort,
        AsteroidBase,
        SurfaceStation,
    }

    class Undocked : JournalEntry
    {
        // { "timestamp":"2024-04-09T06:38:15Z", "event":"Undocked", "StationName":"Bellamy Extraction Base", "StationType":"OnFootSettlement", "MarketID":3888519680, "Taxi":false, "Multicrew":false }

        public string StationName;
        public StationType StationType;
        public long MarketID;
        public bool Taxi;
        public bool Multicrew;
    }

    internal class LandingPads
    {
        public int Large;
        public int Medium;
        public int Small;

        public override string ToString()
        {
            return $"Small:{this.Small}, Medium:{this.Medium}, Large:{this.Large}";
        }
    }

    class Touchdown : LocationEntry, ISystemAddress
    {

        // { "timestamp":"2023-01-06T00:34:41Z", "event":"Touchdown", "PlayerControlled":true, "Taxi":false, "Multicrew":false, "StarSystem":"Synuefe NL-N c23-4", "SystemAddress":1184840454858, "Body":"Synuefe NL-N c23-4 B 3", "BodyID":18, "OnStation":false, "OnPlanet":true, "Latitude":5.602152, "Longitude":-148.097961, "NearestDestination":"$Ancient:#index=3;", "NearestDestination_Localised":"Ancient Ruins (3)" }

        public bool PlayerControlled { get; set; }
        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }

        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }

        public string Body { get; set; }
        public int BodyID { get; set; }

        public bool OnStation { get; set; }
        public bool OnPlanet { get; set; }

        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }
    }

    class Liftoff : LocationEntry, ISystemAddress
    {
        // { "timestamp":"2023-01-06T06:05:03Z", "event":"Liftoff", "PlayerControlled":true, "Taxi":false, "Multicrew":false, "StarSystem":"Synuefe NL-N c23-4", "SystemAddress":1184840454858, "Body":"Synuefe NL-N c23-4 B 3", "BodyID":18, "OnStation":false, "OnPlanet":true, "Latitude":-30.542755, "Longitude":-24.187830, "NearestDestination":"$Ancient:#index=2;", "NearestDestination_Localised":"Ancient Ruins (2)" }

        public bool PlayerControlled { get; set; }
        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }

        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }

        public string Body { get; set; }
        public int BodyID { get; set; }

        public bool OnStation { get; set; }
        public bool OnPlanet { get; set; }

        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }
    }

    class LaunchSRV : JournalEntry
    {
        // { "timestamp":"2023-01-06T00:34:49Z", "event":"LaunchSRV", "SRVType":"testbuggy", "SRVType_Localised":"SRV Scarab", "Loadout":"starter", "ID":25, "PlayerControlled":true }

        public string SRVType { get; set; }
        public string SRVType_Localised { get; set; }
        public string Loadout { get; set; }
        public int ID { get; set; }
        public bool PlayerControlled { get; set; }
    }

    class DockSRV : JournalEntry
    {
        // { "timestamp":"2023-01-06T02:06:11Z", "event":"DockSRV", "SRVType":"testbuggy", "SRVType_Localised":"SRV Scarab", "ID":25 }

        public string SRVType { get; set; }
        public string SRVType_Localised { get; set; }
        public int ID { get; set; }
    }

    class CodexEntry : LocationEntry, ISystemAddress
    {
        // { "timestamp":"2023-01-06T00:46:41Z", "event":"CodexEntry", "EntryID":3200200, "Name":"$Codex_Ent_Guardian_Data_Logs_Name;", "Name_Localised":"Guardian Codex", "SubCategory":"$Codex_SubCategory_Guardian;", "SubCategory_Localised":"Guardian objects", "Category":"$Codex_Category_Civilisations;", "Category_Localised":"Xenological", "Region":"$Codex_RegionName_18;", "Region_Localised":"Inner Orion Spur", "System":"Synuefe NL-N c23-4", "SystemAddress":1184840454858, "BodyID":18, "NearestDestination":"$Ancient:#index=3;", "NearestDestination_Localised":"Ancient Ruins (3)", "Latitude":5.613951, "Longitude":-148.100403, "IsNewEntry":true, "VoucherAmount":50000 }

        public long EntryID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }

        public string SubCategory { get; set; }
        public string SubCategory_Localised { get; set; }
        public string Category { get; set; }
        public string Category_Localised { get; set; }
        public string Region { get; set; }
        public string Region_Localised { get; set; }

        public string System { get; set; }
        public long SystemAddress { get; set; }
        public int? BodyID { get; set; }

        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }

        public bool IsNewEntry { get; set; }
        public int VoucherAmount { get; set; }
    }

    class DataScanned : JournalEntry
    {
        // { "timestamp":"2023-10-07T04:26:43Z", "event":"DataScanned", "Type":"$Datascan_ANCIENTCODEX;", "Type_Localised":"Ancient Codex" }
        public string Type;
        public string Type_Localised;
    }

    class SendText : JournalEntry
    {
        // { "timestamp":"2023-01-07T05:44:32Z", "event":"SendText", "To":"local", "Message":"totem", "Sent":true }

        public string To { get; set; }

        public string Message { get; set; }

        public bool Sent { get; set; }
    }

    class SupercruiseExit : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-03-12T06:16:56Z", "event":"SupercruiseExit", "Taxi":false, "Multicrew":false, "StarSystem":"Synuefe EN-H d11-96", "SystemAddress":3309179996515, "Body":"Synuefe EN-H d11-96 2", "BodyID":9, "BodyType":"Planet" }

        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }
        public string Starsystem { get; set; }
        public long SystemAddress { get; set; }
        public string Body { get; set; }
        public int BodyID { get; set; }
        public FSDJumpBodyType BodyType { get; set; }
    }

    class SupercruiseEntry : JournalEntry
    {
        public string Starsystem { get; set; }
    }

    class Music : JournalEntry
    {
        // { "timestamp":"2023-01-12T05:11:56Z", "event":"Music", "MusicTrack":"MainMenu" }
        public string MusicTrack { get; set; }
    }

    class StartJump : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-01-24T05:06:43Z", "event":"StartJump", "JumpType":"Hyperspace", "StarSystem":"Maridwyn", "SystemAddress":13866167838129, "StarClass":"M" }

        public string JumpType { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public string StarClass { get; set; }
    }

    class FSDJump : JournalEntry, IFactions, ISystemAddress, ISystemDataStarter, IStarRef
    {
        // { "timestamp":"2023-01-24T05:07:01Z", "event":"FSDJump", "Taxi":false, "Multicrew":false, "StarSystem":"Maridwyn", "SystemAddress":13866167838129, "StarPos":[90.46875,16.40625,21.62500], "SystemAllegiance":"Federation", "SystemEconomy":"$economy_Agri;", "SystemEconomy_Localised":"Agriculture", "SystemSecondEconomy":"$economy_Refinery;", "SystemSecondEconomy_Localised":"Refinery", "SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Corporate", "SystemSecurity":"$SYSTEM_SECURITY_high;", "SystemSecurity_Localised":"High Security", "Population":4058074576, "Body":"Maridwyn A", "BodyID":1, "BodyType":"Star", "Powers":[ "Felicia Winters" ], "PowerplayState":"Exploited", "JumpDist":8.278, "FuelUsed":0.091548, "FuelLevel":13.458453, "Factions":[ { "Name":"Social Maridwyn Green Party", "FactionState":"None", "Government":"Democracy", "Influence":0.027559, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"p Velorum Crimson Creative Int", "FactionState":"None", "Government":"Corporate", "Influence":0.059055, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":4.187500 }, { "Name":"Maridwyn Co", "FactionState":"None", "Government":"Corporate", "Influence":0.487205, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, "RecoveringStates":[ { "State":"Boom", "Trend":0 } ] }, { "Name":"Maridwyn Constitution Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.041339, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"Maridwyn Gold Electronics Ltd", "FactionState":"None", "Government":"Corporate", "Influence":0.021654, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"United Maridwyn Law Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.040354, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"Federal Reclamation Co", "FactionState":"Expansion", "Government":"Corporate", "Influence":0.322835, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":97.425003, "ActiveStates":[ { "State":"Expansion" } ] } ], "SystemFaction":{ "Name":"Maridwyn Co" } }
        // { "timestamp":"2024-06-17T04:33:26Z", "event":"FSDJump", "Taxi":false, "Multicrew":false, "StarSystem":"HIP 82206", "SystemAddress":83651269338, "StarPos":[-111.25000,93.18750,18.21875], "SystemAllegiance":"Federation", "SystemEconomy":"$economy_Industrial;", "SystemEconomy_Localised":"Industrial", "SystemSecondEconomy":"$economy_Extraction;", "SystemSecondEconomy_Localised":"Extraction", "SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Corporate", "SystemSecurity":"$SYSTEM_SECURITY_medium;", "SystemSecurity_Localised":"Medium Security", "Population":4807913, "Body":"HIP 82206", "BodyID":0, "BodyType":"Star", "JumpDist":17.333, "FuelUsed":0.780109, "FuelLevel":11.299892, "Factions":[ { "Name":"HIP 82206 Transport Corporation", "FactionState":"None", "Government":"Corporate", "Influence":0.465932, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":47.840000 }, { "Name":"Ugrasin Purple Advanced Industry", "FactionState":"None", "Government":"Corporate", "Influence":0.103206, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":2.970000 }, { "Name":"Aobriguites Blue Bridge Corp", "FactionState":"Election", "Government":"Corporate", "Influence":0.065130, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":-48.794998 }, { "Name":"HIP 82206 Blue Crew", "FactionState":"None", "Government":"Anarchy", "Influence":0.063126, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"Allied HIP 82206 Defence Force", "FactionState":"None", "Government":"Dictatorship", "Influence":0.063126, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"HIP 82206 for Equality", "FactionState":"None", "Government":"Democracy", "Influence":0.092184, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"Elite Secret Service", "FactionState":"None", "Government":"Corporate", "Influence":0.147295, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":-97.620003 } ], "SystemFaction":{ "Name":"HIP 82206 Transport Corporation" } }

        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public double[] StarPos { get; set; } // [90.46875,16.40625,21.62500]
        public string SystemAllegiance { get; set; } // Federation"
        public string SystemEconomy { get; set; } //$economy_Agri;
        public string SystemEconomy_Localised { get; set; } // "Agriculture"
        public string SystemSecondEconomy { get; set; } // $economy_Refinery;
        public string SystemSecondEconomy_Localised { get; set; } // Refinery
        public string SystemGovernment { get; set; }// $government_Corporate;
        public string SystemGovernment_Localised { get; set; } // Corporate
        public string SystemSecurity { get; set; } // $SYSTEM_SECURITY_high;
        public string SystemSecurity_Localised { get; set; } // High Security
        public long Population { get; set; }// 4058074576,
        public string Body { get; set; } // "Maridwyn A",
        public int BodyID { get; set; } // 1,
        public FSDJumpBodyType BodyType { get; set; }  // "Star"
        public List<string> Powers { get; set; } // [ "Felicia Winters" ]
        public string PowerplayState { get; set; } // Exploited
        public double JumpDist { get; set; }// 8.278
        public float FuelUsed { get; set; } // 0.091548
        public float FuelLevel { get; set; } // 13.458453
        public List<SystemFaction>? Factions { get; set; }

        // "Factions":[
        //      { "Name":"Social Maridwyn Green Party", "FactionState":"None", "Government":"Democracy", "Influence":0.027559, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"p Velorum Crimson Creative Int", "FactionState":"None", "Government":"Corporate", "Influence":0.059055, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":4.187500 },
        //      { "Name":"Maridwyn Co", "FactionState":"None", "Government":"Corporate", "Influence":0.487205, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, "RecoveringStates":[ { "State":"Boom", "Trend":0 } ] },
        //      { "Name":"Maridwyn Constitution Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.041339, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"Maridwyn Gold Electronics Ltd", "FactionState":"None", "Government":"Corporate", "Influence":0.021654, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"United Maridwyn Law Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.040354, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"Federal Reclamation Co", "FactionState":"Expansion", "Government":"Corporate", "Influence":0.322835, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":97.425003, "ActiveStates":[ { "State":"Expansion" } ] }
        //  ],
        public NamedFaction SystemFaction;
    }

    class SystemFaction
    {
        // { "Name":"Social Maridwyn Green Party", "FactionState":"None", "Government":"Democracy", "Influence":0.027559, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }
        // { "Name":"Federal Reclamation Co", "FactionState":"Expansion", "Government":"Corporate", "Influence":0.322835, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":97.425003, "ActiveStates":[ { "State":"Expansion" } ] }
        // { "Name":"Maridwyn Co", "FactionState":"None", "Government":"Corporate", "Influence":0.487205, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, "RecoveringStates":[ { "State":"Boom", "Trend":0 } ] },
        public string Name;
        public string FactionState;
        public string Government;
        public double Influence;
        public string Allegiance;
        public string Happiness;
        public string Happiness_Localised;
        public double MyReputation;
        public List<SystemFactionState>? RecoveringStates;
        public List<SystemFactionState>? ActiveStates;
    }

    class SystemFactionState
    {
        // { "State":"Expansion" }
        // { "State":"Boom", "Trend":0 }
        public string State;
        //public double Trend;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    enum FSDJumpBodyType
    {
        BaryCentre,
        Star,
        Planet,
        PlanetaryRing,
        StellarRing,
        Station,
        AsteroidCluster,
        /// <summary>
        /// BaryCenter
        /// </summary>
        Null,
        SmallBody,
    }


    class CarrierJump : JournalEntry, ISystemAddress, ISystemDataStarter, IStarRef
    {
        // { "timestamp":"2023-12-01T04:50:10Z", "event":"CarrierJump", "Docked":true, "StationName":"H6B-5HQ", "StationType":"FleetCarrier", "MarketID":3708733696, "StationFaction":{ "Name":"FleetCarrier" }, "StationGovernment":"$government_Carrier;", "StationGovernment_Localised":"Private Ownership", "StationServices":[ "dock", "autodock", "commodities", "contacts", "exploration", "outfitting", "crewlounge", "rearm", "refuel", "repair", "shipyard", "engineer", "flightcontroller", "stationoperations", "stationMenu", "carriermanagement", "carrierfuel", "livery", "socialspace", "bartender", "vistagenomics" ], "StationEconomy":"$economy_Carrier;", "StationEconomy_Localised":"Private Enterprise", "StationEconomies":[ { "Name":"$economy_Carrier;", "Name_Localised":"Private Enterprise", "Proportion":1.000000 } ], "Taxi":false, "Multicrew":false, "StarSystem":"Wregoe VG-U b17-0", "SystemAddress":681155568793, "StarPos":[764.15625,174.68750,-675.90625], "SystemAllegiance":"", "SystemEconomy":"$economy_None;", "SystemEconomy_Localised":"None", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", "SystemGovernment":"$government_None;", "SystemGovernment_Localised":"None", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "SystemSecurity_Localised":"Anarchy", "Population":0, "Body":"Wregoe VG-U b17-0 A", "BodyID":1, "BodyType":"Star" }

        public bool Docked { get; set; }
        public string StationName { get; set; }
        public StationType StationType { get; set; }
        public long MarketID { get; set; }
        // StationFaction // TODO: { "Name":"FleetCarrier" }
        public string StationGovernment { get; set; }
        public string StationGovernment_Localised { get; set; }
        public List<string> StationServices { get; set; }
        public string StationEconomy { get; set; }
        public string StationEconomy_Localised { get; set; }
        // StationEconomies // TODO: [ { "Name":"$economy_Carrier;", "Name_Localised":"Private Enterprise", "Proportion":1.000000 } ],
        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public double[] StarPos { get; set; }
        public string SystemAllegiance { get; set; }
        public string SystemEconomy { get; set; }
        public string SystemEconomy_Localised { get; set; }
        public string SystemSecondEconomy { get; set; }
        public string SystemSecondEconomy_Localised { get; set; }
        public string SystemGovernment { get; set; }
        public string SystemGovernment_Localised { get; set; }
        public string SystemSecurity { get; set; }
        public string SystemSecurity_Localised { get; set; }
        public long Population { get; set; }
        public string Body { get; set; }
        public int BodyID { get; set; }
        public FSDJumpBodyType BodyType { get; set; }
    }

    class CarrierJumpRequest : JournalEntry
    {
        // { "timestamp":"2023-12-01T05:49:05Z", "event":"CarrierJumpRequest", "CarrierID":3708733696, "SystemName":"Synuefe OR-D c15-8", "Body":"Synuefe OR-D c15-8", "SystemAddress":2283613950594, "BodyID":0, "DepartureTime":"2023-12-01T06:05:10Z" }

        public long CarrierID;
        public string SystemName;
        public string Body;
        public long SystemAddress;
        public int BodyID;
        public DateTimeOffset DepartureTime;
    }

    class Shutdown : JournalEntry
    {
        // { "timestamp":"2023-01-24T06:58:26Z", "event":"Shutdown" }
    }

    enum ScanType
    {
        Log,
        Sample,
        Analyse
    }

    class ScanOrganic : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-05-10T20:03:22Z", "event":"ScanOrganic", "ScanType":"Log", "Genus":"$Codex_Ent_Fonticulus_Genus_Name;", "Genus_Localised":"Fonticulua", "Species":"$Codex_Ent_Fonticulus_02_Name;", "Species_Localised":"Fonticulua Campestris", "Variant":"$Codex_Ent_Fonticulus_02_M_Name;", "Variant_Localised":"Fonticulua Campestris - Amethyst", "SystemAddress":1419209836875, "Body":2 }

        public ScanType ScanType { get; set; }
        public string Genus { get; set; }
        public string Genus_Localized { get; set; }
        public string Species { get; set; }
        public string Species_Localised { get; set; }
        public string Variant { get; set; }
        public string Variant_Localised { get; set; }
        public long SystemAddress { get; set; }
        public int Body { get; set; }
    }

    static class SignalType
    {
        public static string Biological = "$SAA_SignalType_Biological;";
        public static string Geological = "$SAA_SignalType_Geological;";
        public static string Human = "$SAA_SignalType_Human;";
    }

    class ScanSignal
    {
        public string Type { get; set; }
        public string Type_Localised { get; set; }
        public int Count { get; set; }
    }

    class ScanGenus
    {
        public string Genus { get; set; }
        public string Genus_Localised { get; set; }
    }

    class SAASignalsFound : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-02-07T06:12:43Z", "event":"SAASignalsFound", "BodyName":"Synuefe TP-F b44-0 AB 7", "SystemAddress":682228131193, "BodyID":14, "Signals":[ { "Type":"$SAA_SignalType_Biological;", "Type_Localised":"Biological", "Count":6 } ], "Genuses":[ { "Genus":"$Codex_Ent_Bacterial_Genus_Name;", "Genus_Localised":"Bacterium" }, { "Genus":"$Codex_Ent_Cactoid_Genus_Name;", "Genus_Localised":"Cactoida" }, { "Genus":"$Codex_Ent_Fungoids_Genus_Name;", "Genus_Localised":"Fungoida" }, { "Genus":"$Codex_Ent_Stratum_Genus_Name;", "Genus_Localised":"Stratum" }, { "Genus":"$Codex_Ent_Shrubs_Genus_Name;", "Genus_Localised":"Frutexa" }, { "Genus":"$Codex_Ent_Tussocks_Genus_Name;", "Genus_Localised":"Tussock" } ] }

        public string BodyName { get; set; }
        public long SystemAddress { get; set; }
        public int BodyID { get; set; }
        public List<ScanSignal> Signals { get; set; }
        public List<ScanGenus> Genuses { get; set; }
    }

    class SAAScanComplete : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-03-26T22:34:35Z", "event":"SAAScanComplete", "BodyName":"Col 173 Sector PF-E b28-3 B 7", "SystemAddress":7279566464385, "BodyID":17, "ProbesUsed":4, "EfficiencyTarget":4 }
        public string BodyName { get; set; }
        public long SystemAddress { get; set; }
        public int BodyID { get; set; }
        public int ProbesUsed { get; set; }
        public int EfficiencyTarget { get; set; }
    }

    class ApproachBody : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-02-07T06:27:26Z", "event":"ApproachBody", "StarSystem":"Synuefe TP-F b44-0", "SystemAddress":682228131193, "Body":"Synuefe TP-F b44-0 AB 7", "BodyID":14 }

        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public string Body { get; set; }
        public int BodyID { get; set; }
    }

    class LeaveBody : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-02-07T06:29:32Z", "event":"LeaveBody", "StarSystem":"Synuefe TP-F b44-0", "SystemAddress":682228131193, "Body":"Synuefe TP-F b44-0 AB 7", "BodyID":14 }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public string Body { get; set; }
        public int BodyID { get; set; }
    }

    class FSSDiscoveryScan : JournalEntry
    {
        // { "timestamp":"2023-09-04T00:06:12Z", "event":"FSSDiscoveryScan", "Progress":0.520163, "BodyCount":19, "NonBodyCount":10, "SystemName":"Abriama", "SystemAddress":7267219350913 }
        public double Progress;
        public int BodyCount;
        public int NonBodyCount;
        public string SystemName;
        public long SystemAddress;
    }

    class FSSSignalDiscovered : JournalEntry
    {
        // { "timestamp":"2024-10-08T01:23:34Z", "event":"FSSSignalDiscovered", "SystemAddress":6681123623626, "SignalName":"HYPERION CLASS CARRIER XLH-N1H", "SignalType":"FleetCarrier", "IsStation":true }
        // { "timestamp":"2024-10-08T01:54:14Z", "event":"FSSSignalDiscovered", "SystemAddress":4752154823011, "SignalName":"Morrison Prospect", "SignalType":"Outpost" }
        // { "timestamp":"2024-10-08T01:54:14Z", "event":"FSSSignalDiscovered", "SystemAddress":4752154823011, "SignalName":"$Warzone_PointRace_Med:#index=1;", "SignalName_Localised":"Conflict Zone [Medium Intensity]", "SignalType":"Combat" }
        // { "timestamp":"2024-10-08T01:54:14Z", "event":"FSSSignalDiscovered", "SystemAddress":4752154823011, "SignalName":"CEN-770 Lowell-class Researcher", "SignalType":"Megaship" }
        // { "timestamp":"2024-10-08T01:54:14Z", "event":"FSSSignalDiscovered", "SystemAddress":4752154823011, "SignalName":"Pure Mathematic Services", "SignalType":"Installation" }
        // { "timestamp":"2024-10-08T01:54:20Z", "event":"FSSSignalDiscovered", "SystemAddress":4752154823011, "SignalName":"$MULTIPLAYER_SCENARIO42_TITLE;", "SignalName_Localised":"Nav Beacon", "SignalType":"NavBeacon" }
        // { "timestamp":"2024-01-07T07:07:25Z", "event":"FSSSignalDiscovered", "SystemAddress":11675196990889, "SignalName":"$Fixed_Event_Life_Cloud;", "SignalName_Localised":"Notable stellar phenomena", "SignalType":"Codex" }
        // { "timestamp":"2024-10-07T03:07:22Z", "event":"FSSSignalDiscovered", "SystemAddress":6681123623626, "SignalName":"$USS_HighGradeEmissions;", "SignalName_Localised":"Unidentified signal source", "SignalType":"USS", "USSType":"$USS_Type_ValuableSalvage;", "USSType_Localised":"Encoded emissions", "SpawningState":"", "SpawningFaction":"Independent Deciat Green Party", "ThreatLevel":0, "TimeRemaining":2242.791016 }

        public long SystemAddress;
        public string SignalName;
        public string? SignalName_Localised;
        public string SignalType;

        public string? USSType;
        public string? USSType_Localised;
        public string? SpawningState;
        public string? SpawningFaction;
        public int? ThreatLevel;
        /// <summary> Count of seconds </summary>
        public double? TimeRemaining;

        public bool? IsStation;
    }

    class FSSAllBodiesFound : JournalEntry
    {
        // { "timestamp":"2023-09-04T02:29:13Z", "event":"FSSAllBodiesFound", "SystemName":"HIP 51721", "SystemAddress":358596317890, "Count":10 }
        public string SystemName;
        public long SystemAddress;
        public int Count;
    }

    class ScanBaryCentre : JournalEntry
    {
        // { "timestamp":"2023-11-08T06:03:27Z", "event":"ScanBaryCentre", "StarSystem":"Graea Hypue AA-Z d58", "SystemAddress":2003140677259, "BodyID":2, "SemiMajorAxis":57069017887115.476563, "Eccentricity":0.042389, "OrbitalInclination":-11.191447, "Periapsis":322.189732, "OrbitalPeriod":330483412742.614746, "AscendingNode":105.734586, "MeanAnomaly":159.496060 }
        public string StarSystem;
        public long SystemAddress;
        public int BodyID;
        public double SemiMajorAxis;
        public double Eccentricity;
        public double OrbitalInclination;
        public double Periapsis;
        public double OrbitalPeriod;
        public double AscendingNode;
        public double MeanAnomaly;
    }

    class Scan : JournalEntry, ISystemAddress
    {
        // See: https://elite-journal.readthedocs.io/en/latest/Exploration/#scan
        // { "timestamp":"2023-02-07T06:12:44Z", "event":"Scan", "ScanType":"Detailed", "BodyName":"Synuefe TP-F b44-0 AB 7", "BodyID":14, "Parents":[ {"Null":1}, {"Null":0} ], "StarSystem":"Synuefe TP-F b44-0", "SystemAddress":682228131193, "DistanceFromArrivalLS":163.640473, "TidalLock":false, "TerraformState":"", "PlanetClass":"High metal content body", "Atmosphere":"thin ammonia atmosphere", "AtmosphereType":"Ammonia", "AtmosphereComposition":[ { "Name":"Ammonia", "Percent":100.000000 } ], "Volcanism":"", "MassEM":0.012165, "Radius":1506425.250000, "SurfaceGravity":2.136633, "SurfaceTemperature":171.117615, "SurfacePressure":218.443268, "Landable":true, "Materials":[ { "Name":"iron", "Percent":22.081503 }, { "Name":"nickel", "Percent":16.701525 }, { "Name":"sulphur", "Percent":15.735665 }, { "Name":"carbon", "Percent":13.232062 }, { "Name":"manganese", "Percent":9.119436 }, { "Name":"phosphorus", "Percent":8.471395 }, { "Name":"zinc", "Percent":6.000926 }, { "Name":"germanium", "Percent":4.639404 }, { "Name":"molybdenum", "Percent":1.441909 }, { "Name":"ruthenium", "Percent":1.363674 }, { "Name":"tungsten", "Percent":1.212496 } ], "Composition":{ "Ice":0.000000, "Rock":0.672827, "Metal":0.327173 }, "SemiMajorAxis":51394448280.334473, "Eccentricity":0.173554, "OrbitalInclination":-164.752899, "Periapsis":243.912489, "OrbitalPeriod":9480342.149734, "AscendingNode":-98.783954, "MeanAnomaly":62.655478, "RotationPeriod":127770.138485, "AxialTilt":-0.440350, "WasDiscovered":true, "WasMapped":true }
        // { "timestamp":"2023-11-12T04:44:01Z", "event":"Scan", "ScanType":"Detailed", "BodyName":"Graea Hypue AA-Z d70 AB 6 e", "BodyID":54, "Parents":[ {"Planet":48}, {"Null":47}, { "Null":1}, { "Null":0} ], "StarSystem":"Graea Hypue AA-Z d70", "SystemAddress":2415457537675, "DistanceFromArrivalLS":4889.201779, "TidalLock":true, "TerraformState":"", "PlanetClass":"Rocky body", "Atmosphere":"thin carbon dioxide atmosphere", "AtmosphereType":"CarbonDioxide", "AtmosphereComposition":[ { "Name":"CarbonDioxide", "Percent":99.009911 }, { "Name":"SulphurDioxide", "Percent":0.990099 } ], "Volcanism":"", "MassEM":0.016760, "Radius":1780979.625000, "SurfaceGravity":2.106024, "SurfaceTemperature":193.735855, "SurfacePressure":8721.671875, "Landable":true, "Materials":[ { "Name":"iron", "Percent":19.016521 }, { "Name":"sulphur", "Percent":18.705187 }, { "Name":"carbon", "Percent":15.729125 }, { "Name":"nickel", "Percent":14.383301 }, { "Name":"phosphorus", "Percent":10.070056 }, { "Name":"chromium", "Percent":8.552361 }, { "Name":"manganese", "Percent":7.853629 }, { "Name":"zirconium", "Percent":2.208210 }, { "Name":"cadmium", "Percent":1.476719 }, { "Name":"antimony", "Percent":1.174460 }, { "Name":"mercury", "Percent":0.830420 } ], "Composition":{ "Ice":0.000000, "Rock":0.912026, "Metal":0.087974 }, "SemiMajorAxis":2361911833.286285, "Eccentricity":0.003561, "OrbitalInclination":0.742554, "Periapsis":74.570264, "OrbitalPeriod":877842.348814, "AscendingNode":-1.043759, "MeanAnomaly":10.245327, "RotationPeriod":877853.401681, "AxialTilt":-0.099346, "WasDiscovered":false, "WasMapped":false }

        /*
         * A basic scan on a planet will include body name, planet class, orbital data, rotation period, mass, radius, surface gravity; 
         * but will exclude tidal lock, terraform state, atmosphere, volcanism, surface pressure and temperature, available materials, and details of rings.
         * The info for a star will be largely the same whether a basic scanner or detailed scanner is used.
         */

        // Star specific ...

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ScanType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StarSystem { get; set; } // name
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long SystemAddress { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Bodyname { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int BodyID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long DistanceFromArrivalLS { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StarType { get; set; } // Stellar classification (for a star) – see §15.2
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Subclass { get; set; } // Star's heat classification 0..9
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double StellarMass { get; set; } //  mass as multiple of Sol's mass
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public decimal Radius { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double AbsoluteMagnitude { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double RotationPeriod { get; set; } // (seconds)
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double SurfaceTemperature { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Luminosity { get; set; } //  see §15.9
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double Age_MY { get; set; } // age in millions of years

        public class Ring
        {
            // "Rings":[ { "Name":"Stuemeae UY-S b3-86 7 A Ring", "RingClass":"eRingClass_Icy", "MassMT":1.036e+07, "InnerRad":2.2544e+07, "OuterRad":5.479e+07 } ]

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string Name;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string RingClass;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public double MassMT;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public double InnerRad;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public double OuterRad;
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<Ring> Rings { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool WasDiscovered { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool WasMapped { get; set; }

        // Planet specific ...

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        //public BodyParents Parents { get; set; }
        public List<Dictionary<ParentBodyType, int>> Parents { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool TidalLock { get; set; } // 1 if tidally locked
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string TerraformState { get; set; } // Terraformable, Terraforming, Terraformed, or null
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string PlanetClass { get; set; } // see §15.3
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double MassEM { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Atmosphere { get; set; } // see §15.4
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string AtmosphereType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<Composition> AtmosphereComposition { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Volcanism { get; set; } // see §15.5
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double SurfaceGravity { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double SurfacePressure { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool Landable { get; set; } // : true (if landable)

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<Composition> Materials { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, float> Composition { get; set; }

        /* TODO: Materials: JSON array with objects with material names and percentage occurrence
        Composition: structure containing info on solid composition
        Ice
        Rock
        Metal
        */

        /* TODO: Rings: [array of info] – if rings present
        ReserveLevel: (Pristine/Major/Common/Low/Depleted) – if rings present
        */

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double Axial { get; set; } // tilt If rotating:

        // Orbital Parameters for any Star/Planet/Moon(except main star of single-star system)

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double SemiMajorAxis { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double Eccentricity { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double OrbitalInclination { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double Periapsis { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double OrbitalPeriod { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double MeanAnomaly { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double AscendingNode { get; set; }
    }

    class Composition
    {
        // "AtmosphereComposition":[ { "Name":"Ammonia", "Percent":100.000000 } ]
        public string Name;
        public float Percent;
    }

    class FSSBodySignals : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-08-18T23:10:13Z", "event":"FSSBodySignals", "BodyName":"Wregoe LH-T b5-0 1", "BodyID":5, "SystemAddress":684377515057, "Signals":[ { "Type":"$SAA_SignalType_Biological;", "Type_Localised":"Biological", "Count":1 } ] }
        public long SystemAddress { get; set; }
        public string Bodyname { get; set; }
        public int BodyID { get; set; }
        public List<ScanSignal> Signals { get; set; }
    }

    class Disembark : JournalEntry, ISystemAddress
    {
        // { "timestamp":"2023-03-03T06:20:22Z", "event":"Disembark", "SRV":true, "Taxi":false, "Multicrew":false, "ID":26, "StarSystem":"Synuefe TP-F b44-0", "SystemAddress":682228131193, "Body":"Synuefe TP-F b44-0 AB 7", "BodyID":14, "OnStation":false, "OnPlanet":true }
        public bool SRV { get; set; }
        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }

        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public string Body { get; set; }
        public int BodyID { get; set; }

        public bool OnStation { get; set; }
        public bool OnPlanet { get; set; }
    }

    class Embark : Disembark
    {
        // { "timestamp":"2023-03-03T06:21:00Z", "event":"Embark", "SRV":true, "Taxi":false, "Multicrew":false, "ID":26, "StarSystem":"Synuefe TP-F b44-0", "SystemAddress":682228131193, "Body":"Synuefe TP-F b44-0 AB 7", "BodyID":14, "OnStation":false, "OnPlanet":true }
    }

    class BioData
    {
        public string Genus { get; set; } // "$Codex_Ent_Electricae_Genus_Name;"
        public string Genus_Localised { get; set; }  // "Electricae"
        public string Species { get; set; }  // "$Codex_Ent_Electricae_02_Name;"
        public string Species_Localised { get; set; } // "Electricae Radialem"
        public long Value { get; set; } // 6284600
        public long Bonus { get; set; } // 0
    }

    class SellOrganicData : JournalEntry
    {
        // { "timestamp":"2023-04-17T17:42:30Z", "event":"SellOrganicData", "MarketID":3708733696, "BioData":[ { "Genus":"$Codex_Ent_Cone_Name;", "Genus_Localised":"Bark Mounds", "Species":"$Codex_Ent_Cone_Name;", "Species_Localised":"Bark Mounds", "Value":1471900, "Bonus":5887600 }, { "Genus":"$Codex_Ent_Electricae_Genus_Name;", "Genus_Localised":"Electricae", "Species":"$Codex_Ent_Electricae_02_Name;", "Species_Localised":"Electricae Radialem", "Value":6284600, "Bonus":0 }, { "Genus":"$Codex_Ent_Fumerolas_Genus_Name;", "Genus_Localised":"Fumerola", "Species":"$Codex_Ent_Fumerolas_04_Name;", "Species_Localised":"Fumerola Aquatis", "Value":6284600, "Bonus":0 }, { "Genus":"$Codex_Ent_Fonticulus_Genus_Name;", "Genus_Localised":"Fonticulua", "Species":"$Codex_Ent_Fonticulus_02_Name;", "Species_Localised":"Fonticulua Campestris", "Value":1000000, "Bonus":0 } ] }

        public long MarketID { get; set; }
        public List<BioData> BioData { get; set; }
    }

    class SuitLoadout : JournalEntry
    {
        // { "timestamp":"2023-05-10T18:16:37Z", "event":"SuitLoadout", "SuitID":1722825297017949, "SuitName":"explorationsuit_class5", "SuitName_Localised":"$ExplorationSuit_Class1_Name;", "SuitMods":[ "suit_increasedsprintduration", "suit_improvedjumpassist", "suit_nightvision", "suit_increasedbatterycapacity" ], "LoadoutID":4293000005, "LoadoutName":"Green!", "Modules":[ { "SlotName":"PrimaryWeapon1", "SuitModuleID":1723805638978815, "ModuleName":"wpn_m_assaultrifle_plasma_fauto", "ModuleName_Localised":"Manticore Oppressor", "Class":3, "WeaponMods":[ "weapon_accuracy" ] }, { "SlotName":"SecondaryWeapon", "SuitModuleID":1723541854154251, "ModuleName":"wpn_s_pistol_plasma_charged", "ModuleName_Localised":"Manticore Tormentor", "Class":5, "WeaponMods":[ "weapon_suppression_pressurised", "weapon_handling", "weapon_stability", "weapon_backpackreloading" ] } ] }

        public long SuitID;
        public string SuitName;
        public string SuitName_Localised;
        public List<string> SuitMods;
        public long LoadoutID;
        public string LoadoutName;

        // TODO: Modules
    }

    class SwitchSuitLoadout : SuitLoadout
    {
        // { "timestamp":"2023-05-10T18:28:28Z", "event":"SwitchSuitLoadout", "SuitID":1723541811099765, "SuitName":"tacticalsuit_class5", "SuitName_Localised":"$TacticalSuit_Class1_Name;", "SuitMods":[ "suit_increasedsprintduration", "suit_improvedarmourrating", "suit_increasedammoreserves", "suit_increasedshieldregen" ], "LoadoutID":4293000000, "LoadoutName":"solder / general", "Modules":[ { "SlotName":"PrimaryWeapon1", "SuitModuleID":1704060320072893, "ModuleName":"wpn_m_assaultrifle_laser_fauto", "ModuleName_Localised":"TK Aphelion", "Class":5, "WeaponMods":[ "weapon_clipsize", "weapon_backpackreloading", "weapon_handling" ] }, { "SlotName":"PrimaryWeapon2", "SuitModuleID":1722824106905372, "ModuleName":"wpn_m_assaultrifle_kinetic_fauto", "ModuleName_Localised":"Karma AR-50", "Class":5, "WeaponMods":[ "weapon_stability", "weapon_handling", "weapon_clipsize", "weapon_headshotdamage" ] }, { "SlotName":"SecondaryWeapon", "SuitModuleID":1723541854154251, "ModuleName":"wpn_s_pistol_plasma_charged", "ModuleName_Localised":"Manticore Tormentor", "Class":5, "WeaponMods":[ "weapon_suppression_pressurised", "weapon_handling", "weapon_stability", "weapon_backpackreloading" ] } ] }

    }

    class Screenshot : JournalEntry
    {
        // { "timestamp":"2023-05-11T05:25:31Z", "event":"Screenshot", "Filename":"\\ED_Pictures\\Screenshot_0465.bmp", "Width":1920, "Height":1080, "System":"Bleethuae LN-B d1172", "Body":"Bleethuae LN-B d1172 4 b" }
        // { "timestamp":"2023-05-13T02:35:48Z", "event":"Screenshot", "Filename":"\\ED_Pictures\\Screenshot_0522.bmp", "Width":3440, "Height":1440, "System":"IC 2391 Sector YE-A d103", "Body":"IC 2391 Sector YE-A d103 B 1", "Latitude":-54.580612, "Longitude":25.089542, "Heading":187, "Altitude":1612.171509 }

        public string Filename;
        public int Width;
        public int Height;
        public string System;
        public string Body;

        // keeping these as simple strings, so it is easier to detect if they are missing, from legacy mode
        public string Latitude;
        public string Longitude;
        public string Heading;
        public string Altitude;
    }

    class FSDTarget : JournalEntry
    {
        // { "timestamp":"2023-05-29T06:09:58Z", "event":"FSDTarget", "Name":"Col 173 Sector MC-T b20-3", "SystemAddress":7282519516481, "StarClass":"M", "RemainingJumpsInRoute":1 }

        public string Name;
        public long SystemAddress;
        public string StarClass;
        public int RemainingJumpsInRoute;
    }

    class NavRoute : JournalEntry
    {
        // { "timestamp":"2023-08-18T13:34:53Z", "event":"NavRoute" }
    }

    class NavRouteClear : JournalEntry
    {
        // { "timestamp":"2023-08-18T17:59:50Z", "event":"NavRouteClear" }
    }

    class SupercruiseDestinationDrop : JournalEntry
    {
        // { "timestamp":"2023-06-07T00:52:52Z", "event":"SupercruiseDestinationDrop", "Type":"Chiao Enterprise", "Threat":0, "MarketID":3223505152 }
        // { "timestamp":"2023-07-19T05:48:03Z", "event":"SupercruiseDestinationDrop", "Type":"Guardian Beacon", "Threat":0 }

        public string Type;
        public int Threat;
        public string? MarketID;
    }

    class MaterialCollected : JournalEntry
    {
        // { "timestamp":"2023-10-21T21:36:39Z", "event":"MaterialCollected", "Category":"Encoded", "Name":"ancienttechnologicaldata", "Name_Localised":"Pattern Epsilon Obelisk Data", "Count":3 }

        public string Category;
        public string Name;
        public string Name_Localised;
        public int Count;
    }

    class Backpack : JournalEntry
    {
        // { "timestamp":"2023-12-13T18:25:08Z", "event":"Backpack", "Items":[  ], "Components":[  ], "Consumables":[ { "Name":"healthpack", "Name_Localised":"Medkit", "OwnerID":0, "Count":1 }, { "Name":"energycell", "Name_Localised":"Energy Cell", "OwnerID":0, "Count":3 }, { "Name":"amm_grenade_emp", "Name_Localised":"Shield Disruptor", "OwnerID":0, "Count":1 }, { "Name":"amm_grenade_frag", "Name_Localised":"Frag Grenade", "OwnerID":0, "Count":1 }, { "Name":"amm_grenade_shield", "Name_Localised":"Shield Projector", "OwnerID":0, "Count":1 }, { "Name":"bypass", "Name_Localised":"E-Breach", "OwnerID":0, "Count":1 } ], "Data":[  ] }

        // TODO: all the fields - this is just being used as a trigger currently
    }

    class Missions : JournalEntry
    {
        // { "timestamp":"2024-01-04T03:58:00Z", "event":"Missions", "Active":[ { "MissionID":949733662, "Name":"Mission_HackMegaship_name", "PassengerMission":false, "Expires":0 } ], "Failed":[  ], "Complete":[  ] }
        public List<Mission> Active;
        public List<Mission> Failed;
        public List<Mission> Complete;
    }

    class Mission
    {
        public decimal MissionID;
        public string Name;
        public bool PassengerMission;
        public long Expires;
    }

    class MissionAccepted : JournalEntry
    {
        // { "timestamp":"2024-01-04T04:30:06Z", "event":"MissionAccepted", "Faction":"Meene General Industries", "Name":"Mission_TheDead", "LocalisedName":"Decoding the Ancient Ruins", "Wing":false, "Influence":"None", "Reputation":"None", "MissionID":949931893 }
        // { "timestamp":"2022-07-23T03:46:26Z", "event":"MissionAccepted", "Faction":"Raven Colonial Corporation", "Name":"Mission_Massacre", "LocalisedName":"Kill Grabru Crimson Family faction Pirates", "TargetType":"$MissionUtil_FactionTag_Pirate;", "TargetType_Localised":"Pirates", "TargetFaction":"Grabru Crimson Family", "KillCount":7, "DestinationSystem":"Grabru", "DestinationStation":"Henize Platform", "Expiry":"2022-07-24T08:06:52Z", "Wing":false, "Influence":"++", "Reputation":"++", "Reward":5401452, "MissionID":879230525 }

        public string Faction;
        public string Name;
        public long MissionID;
        public string LocalisedName;
        public bool Wing;
        public string Influence;
        public string Reputation;
        public long Reward;

        public string? DestinationSystem;
        public string? DestinationStation;
        public DateTimeOffset? Expiry;

        public string? TargetType;
        public string? TargetType_Localised;
        public string? TargetFaction;
        public int KillCount;
    }

    class MissionCompleted : JournalEntry
    {
        // { "timestamp":"2023-03-22T04:22:48Z", "event":"MissionCompleted", "Faction":"Xihe Energy Industry", "Name":"Mission_Assassinate_Legal_War_name", "MissionID":920770802, "TargetType":"$MissionUtil_FactionTag_VenerableGeneral;", "TargetType_Localised":"Venerable General", "TargetFaction":"Psi Capricorni Purple Brothers", "DestinationSystem":"Psi Capricorni", "DestinationStation":"Shaw Holdings", "Target":"Zaphod Quagmire", "Reward":1344814, "MaterialsReward":[ { "Name":"EmbeddedFirmware", "Name_Localised":"Modified Embedded Firmware", "Category":"$MICRORESOURCE_CATEGORY_Encoded;", "Category_Localised":"Encoded", "Count":5 } ], "FactionEffects":[ { "Faction":"Xihe Energy Industry", "Effects":[ { "Effect":"$MISSIONUTIL_Interaction_Summary_EP_up;", "Effect_Localised":"The economic status of $#MinorFaction; has improved in the $#System; system.", "Trend":"UpGood" } ], "Influence":[ { "SystemAddress":1183364125402, "Trend":"UpGood", "Influence":"++" } ], "ReputationTrend":"UpGood", "Reputation":"++" }, { "Faction":"", "Effects":[ { "Effect":"$MISSIONUTIL_Interaction_Summary_EP_down;", "Effect_Localised":"The economic status of $#MinorFaction; has declined in the $#System; system.", "Trend":"DownBad" } ], "Influence":[ { "SystemAddress":525873432939, "Trend":"DownBad", "Influence":"+" } ], "ReputationTrend":"DownBad", "Reputation":"+" } ] }

        public string Faction;
        public string Name;
        public long MissionID;

        // TODO: the rest...
    }

    class MissionFailed : JournalEntry
    {
        // { "timestamp":"2022-03-20T05:41:32Z", "event":"MissionFailed", "Name":"Mission_OnFoot_AssassinationIllegal_MB_name", "MissionID":855110856 }

        public string Name;
        public long MissionID;
        public string LocalisedName;
        public long Fine;

        // TODO: the rest...
    }

    class MissionAbandoned : JournalEntry
    {
        // { "timestamp":"2024-01-04T04:55:12Z", "event":"MissionAbandoned", "Name":"Mission_TheDead_name", "LocalisedName":"Decoding the Ancient Ruins", "MissionID":949931893 }
        public long MissionID;
        public string Name;
        public string LocalisedName;
    }

    class Bounty : JournalEntry
    {
        // { "timestamp":"2022-07-23T04:43:32Z", "event":"Bounty", "Rewards":[ { "Faction":"Grabru Blue Power Partners", "Reward":145590 } ], "Target":"viper", "Target_Localised":"Viper Mk III", "TotalReward":145590, "VictimFaction":"Grabru Crimson Family" }
        // { "timestamp":"2024-11-07T04:59:06Z", "event":"Bounty", "Rewards":[ { "Faction":"Marauders Shadowcouncil", "Reward":170158 } ], "PilotName":"$npc_name_decorate:#name=William Kershaw;", "PilotName_Localised":"William Kershaw", "Target":"cobramkiii", "Target_Localised":"Cobra Mk III", "TotalReward":170158, "VictimFaction":"Posse of HIP 97337" }

        public List<RewardEntry> Rewards;
        public string PilotName;
        public string PilotName_Localised;
        public string Target;
        public string Target_Localised;
        public long TotalReward;
        public string VictimFaction;

        public class RewardEntry
        {
            public string Faction;
            public long Reward;
        }
    }

    class Cargo : JournalEntry
    {
        // { "timestamp":"2024-01-04T19:16:45Z", "event":"Cargo", "Vessel":"SRV", "Count":3, "Inventory":[ { "Name":"ancienttablet", "Name_Localised":"Guardian Tablet", "Count":1, "Stolen":0 }, { "Name":"ancienturn", "Name_Localised":"Guardian Urn", "Count":1, "Stolen":0 }, { "Name":"ancienttotem", "Name_Localised":"Guardian Totem", "Count":1, "Stolen":0 } ] }
        public string Vessel { get; set; }
        public int Count { get; set; }
        public List<InventoryItem> Inventory { get; set; }

        public int getCount(string commodity)
        {
            return this.Inventory.FirstOrDefault(i => i.Name == commodity)?.Count ?? 0;
        }
    }

    class CargoDepot : JournalEntry
    {
        // { "timestamp":"2025-02-07T05:56:22Z", "event":"CargoDepot", "MissionID":1002252135, "UpdateType":"Deliver", "CargoType":"Bertrandite", "Count":368, "StartMarketID":0, "EndMarketID":3227103232, "ItemsCollected":0, "ItemsDelivered":736, "TotalItemsToDeliver":912, "Progress":0.000000 }
        public long MissionID;
        public string UpdateType;
        public string CargoType;
        public int Count;
        public long StartMarketID;
        public long EndMarketID;
        public int ItemsCollected;
        public int ItemsDelivered;
        public int TotalItemsToDeliver;
        public double Progress;
    }

    class InventoryItem
    {
        // { "Name":"ancienttablet", "Name_Localised":"Guardian Tablet", "Count":1, "Stolen":0 }
        public string Name;
        public string Name_Localised;
        public int Count;
        public bool Stolen;

        public override string ToString()
        {
            return $"{Name}: {Count}";
        }

        public InventoryItem() { }

        public InventoryItem(string name, string nameLocalized)
        {
            this.Name = name;
            this.Name_Localised = nameLocalized;
        }
    }

    class CollectCargo : JournalEntry
    {
        // { "timestamp":"2024-01-04T20:42:18Z", "event":"CollectCargo", "Type":"ancienttablet", "Type_Localised":"Guardian Tablet", "Stolen":false }
        public string Type;
        public string Type_Localised;
        public bool Stolen;
    }

    class EjectCargo : JournalEntry
    {
        // { "timestamp":"2024-01-04T21:06:06Z", "event":"EjectCargo", "Type":"ancientrelic", "Type_Localised":"Guardian Relic", "Count":1, "Abandoned":false }
        public string Type;
        public string Type_Localised;
        public int Count;
        public bool Abandoned;
    }

    class CargoTransfer : JournalEntry
    {
        // { "timestamp":"2023-08-17T23:31:00Z", "event":"CargoTransfer", "Transfers":[ { "Type":"ancienttablet", "Type_Localised":"Guardian Tablet", "Count":1, "Direction":"toship" }, { "Type":"ancientrelic", "Type_Localised":"Guardian Relic", "Count":2, "Direction":"toship" }, { "Type":"ancienttotem", "Type_Localised":"Guardian Totem", "Count":1, "Direction":"toship" } ] }
        public List<TransferItem> Transfers;
    }

    class TransferItem
    {
        // { "Type":"ancienttablet", "Type_Localised":"Guardian Tablet", "Count":1, "Direction":"toship" }
        public string Type;
        public string Type_Localised;
        public int Count;
        public string Direction;

        public override string ToString()
        {
            return $"{Type}: {Count} => {Direction}";
        }
    }

    class Materials : JournalEntry
    {
        // { "timestamp":"2024-03-18T00:04:26Z", "event":"Materials", "Raw":[ { "Name":"sulphur", "Count":3 }, { "Name":"zirconium", "Count":6 }, { "Name":"manganese", "Count":9 }, { "Name":"vanadium", "Count":15 }, { "Name":"tungsten", "Count":6 }, { "Name":"zinc", "Count":18 }, { "Name":"chromium", "Count":15 } ], "Manufactured":[ { "Name":"highdensitycomposites", "Name_Localised":"High Density Composites", "Count":3 }, { "Name":"configurablecomponents", "Name_Localised":"Configurable Components", "Count":5 }, { "Name":"phasealloys", "Name_Localised":"Phase Alloys", "Count":3 }, { "Name":"heatconductionwiring", "Name_Localised":"Heat Conduction Wiring", "Count":3 }, { "Name":"crystalshards", "Name_Localised":"Crystal Shards", "Count":3 }, { "Name":"hybridcapacitors", "Name_Localised":"Hybrid Capacitors", "Count":3 }, { "Name":"chemicalprocessors", "Name_Localised":"Chemical Processors", "Count":2 }, { "Name":"biotechconductors", "Name_Localised":"Biotech Conductors", "Count":4 }, { "Name":"refinedfocuscrystals", "Name_Localised":"Refined Focus Crystals", "Count":4 } ], "Encoded":[ { "Name":"encryptionarchives", "Name_Localised":"Atypical Encryption Archives", "Count":15 }, { "Name":"adaptiveencryptors", "Name_Localised":"Adaptive Encryptors Capture", "Count":6 }, { "Name":"consumerfirmware", "Name_Localised":"Modified Consumer Firmware", "Count":9 }, { "Name":"embeddedfirmware", "Name_Localised":"Modified Embedded Firmware", "Count":8 }, { "Name":"dataminedwake", "Name_Localised":"Datamined Wake Exceptions", "Count":12 }, { "Name":"disruptedwakeechoes", "Name_Localised":"Atypical Disrupted Wake Echoes", "Count":29 }, { "Name":"hyperspacetrajectories", "Name_Localised":"Eccentric Hyperspace Trajectories", "Count":18 }, { "Name":"compactemissionsdata", "Name_Localised":"Abnormal Compact Emissions Data", "Count":3 }, { "Name":"fsdtelemetry", "Name_Localised":"Anomalous FSD Telemetry", "Count":6 } ] }

        public List<MaterialEntry> Raw;
        public List<MaterialEntry> Manufactured;
        public List<MaterialEntry> Encoded;

        public MaterialEntry? getMaterialEntry(string category, string name)
        {
            if (category == "Raw")
                return this.Raw.Find(m => m.Name == name);
            else if (category == "Encoded")
                return this.Encoded.Find(m => m.Name == name);
            else if (category == "Manufactured")
                return this.Manufactured.Find(m => m.Name == name);
            else
                throw new Exception("Unexpected category: " + category);
        }
    }

    public class MaterialEntry
    {
        public string Name;
        /// <summary> Not present for raw materials </summary>
        public string? Name_Localised;
        public int Count;

        [JsonIgnore]
        public string displayName => Name_Localised ?? Name;

        public override string ToString()
        {
            return $"{Name}: {Count}";
        }
    }

    class MaterialTrade : JournalEntry
    {
        // { "timestamp":"2024-02-11T01:42:25Z", "event":"MaterialTrade", "MarketID":3222950656, "TraderType":"manufactured", "Paid":{ "Material":"conductivepolymers", "Material_Localised":"Conductive Polymers", "Category":"Manufactured", "Quantity":1 }, "Received":{ "Material":"conductivecomponents", "Material_Localised":"Conductive Components", "Category":"Manufactured", "Quantity":9 } }
        // { "timestamp":"2024-10-06T06:32:14Z", "event":"MaterialTrade", "MarketID":3229698816, "TraderType":"raw", "Paid":{ "Material":"zinc", "Category":"Raw", "Quantity":36 }, "Received":{ "Material":"selenium", "Category":"Raw", "Quantity":1 } }

        public long MarketID;
        public string TraderType;
        public MaterialCost Paid;
        public MaterialCost Received;

        public class MaterialCost
        {
            public string Material; // conductivepolymers"
            /// <summary> Not present for raw materials </summary>
            public string? Material_Localised; // Conductive Polymers"
            public string Category; // Manufactured
            public int Quantity; // 1

            [JsonIgnore]
            public string displayName => Material_Localised ?? Material;
        }
    }

    class TechnologyBroker : JournalEntry
    {
        // { "timestamp":"2024-06-02T02:17:55Z", "event":"TechnologyBroker", "BrokerType":"rescue", "MarketID":129020543, "ItemsUnlocked":[ { "Name":"Hpt_ATMultiCannon_Gimbal_Large", "Name_Localised":"Enhanced AX Multi-Cannon" } ], "Commodities":[  ], "Materials":[ { "Name":"iron", "Count":11, "Category":"Raw" }, { "Name":"zirconium", "Count":16, "Category":"Raw" }, { "Name":"tg_biomechanicalconduits", "Name_Localised":"Bio-Mechanical Conduits", "Count":9, "Category":"Manufactured" }, { "Name":"tg_weaponparts", "Name_Localised":"Weapon Parts", "Count":17, "Category":"Manufactured" }, { "Name":"tg_shipsystemsdata", "Name_Localised":"Ship Systems Data", "Count":6, "Category":"Encoded" }, { "Name":"tg_wreckagecomponents", "Name_Localised":"Wreckage Components", "Count":12, "Category":"Manufactured" } ] }
        // { "timestamp":"2023-09-19T05:02:55Z", "event":"TechnologyBroker", "BrokerType":"rescue", "MarketID":129021823, "ItemsUnlocked":[ { "Name":"Hpt_CausticSinkLauncher_Turret_Tiny", "Name_Localised":"Caustic Sink Launcher" } ], "Commodities":[ { "Name":"thargoidgeneratortissuesample", "Name_Localised":"Caustic Tissue Sample", "Count":5 } ], "Materials":[ { "Name":"galvanisingalloys", "Name_Localised":"Galvanising Alloys", "Count":10, "Category":"Manufactured" }, { "Name":"chemicalstorageunits", "Name_Localised":"Chemical Storage Units", "Count":15, "Category":"Manufactured" }, { "Name":"tg_causticshard", "Name_Localised":"Caustic Shard", "Count":20, "Category":"Manufactured" }, { "Name":"tg_causticgeneratorparts", "Name_Localised":"Corrosive Mechanisms", "Count":10, "Category":"Manufactured" } ] }

        public string BrokerType;
        public long MarketID;
        public List<UnlockedItem> ItemsUnlocked;
        public List<MaterialEntry> Commodities;
        public List<Material> Materials;

        public class UnlockedItem
        {
            public string Name;
            public string? Name_Localised;
        }

        public class Material
        {
            public string Name;
            /// <summary> Not present for raw materials </summary>
            public string? Name_Localised;
            public int Count;
            public string Category;

            [JsonIgnore]
            public string displayName => Name_Localised ?? Name;
        }
    }

    class MultiSellExplorationData : JournalEntry
    {
        // { "timestamp":"2024-02-10T04:26:31Z", "event":"MultiSellExplorationData", "Discovered":[ { "SystemName":"Chu I", "NumBodies":3 }, { "SystemName":"Herculis Sector JC-V b2-2", "NumBodies":4 }, { "SystemName":"Sadhant", "NumBodies":4 }, { "SystemName":"Herculis Sector GW-W b1-4", "NumBodies":1 } ], "BaseValue":84069, "Bonus":0, "TotalEarnings":84069 }

        public List<SoldSystem> Discovered;

        public long BaseValue;
        public long Bonus;
        public long TotalEarnings;
    }

    class SoldSystem
    {
        public string SystemName;
        public int NumBodies;
    }

    class SellExplorationData : JournalEntry
    {
        // { "timestamp":"2021-12-12T04:30:03Z", "event":"SellExplorationData", "Systems":[ "Phreia Flyou RX-S b49-3" ], "Discovered":[ "Phreia Flyou RX-S b49-3" ], "BaseValue":2350, "Bonus":2519, "TotalEarnings":3724 }

        public List<string> Systems;
        public List<string> Discovered;
        public long BaseValue;
        public long Bonus;
        public long TotalEarnings;
    }

    class BackpackChange : JournalEntry
    {
        // { "timestamp":"2024-06-16T06:06:47Z", "event":"BackpackChange", "Added":[ { "Name":"evacuationprotocols", "Name_Localised":"Evacuation Protocols", "OwnerID":0, "Count":2, "Type":"Data" } ] }
        // { "timestamp":"2024-05-15T03:26:32Z", "event":"BackpackChange", "Removed":[ { "Name":"energycell", "Name_Localised":"Energy Cell", "OwnerID":0, "Count":1, "Type":"Consumable" } ] }
        public List<BackpackChange_Entry> Added;
        public List<BackpackChange_Entry> Removed;
    }

    class BackpackChange_Entry
    {
        // { "Name":"energycell", "Name_Localised":"Energy Cell", "OwnerID":0, "Count":1, "Type":"Consumable" }
        public string Name;
        public string Name_Localised;
        public long OwnerID;
        public int Count;
        public string Type;
    }

    class CollectItems : JournalEntry
    {
        // { "timestamp":"2024-06-16T22:49:59Z", "event":"CollectItems", "Name":"graphene", "Type":"Component", "OwnerID":0, "Count":1, "Stolen":true }
        public string Name;
        public string Type;
        public long OwnerID;
        public int Count;
        public bool Stolen;
    }

    class UseConsumable : JournalEntry
    {
        // { "timestamp":"2024-05-15T03:26:32Z", "event":"UseConsumable", "Name":"energycell", "Name_Localised":"Energy Cell", "Type":"Consumable" }
        public string Name;
        public string Name_Localised;
        public string Type;
    }

    class MarketBuy : JournalEntry
    {

        // { "timestamp":"2025-03-04T16:10:26Z", "event":"MarketBuy", "MarketID":3708733696, "Type":"insulatingmembrane", "Type_Localised":"Insulating Membrane", "Count":32, "BuyPrice":10605, "TotalCost":339360 }
        public long MarketId;
        public string Type;
        public string Type_Localised;
        public int Count;
        public int BuyPrice;
        public long TotalCost;
    }

    class Market : JournalEntry
    {
        // { "timestamp":"2025-03-04T21:54:50Z", "event":"Market", "MarketID":3708733696, "StationName":"H6B-5HQ", "StationType":"FleetCarrier", "CarrierDockingAccess":"squadronfriends", "StarSystem":"Sedimo" }
        // { "timestamp":"2025-03-04T18:39:33Z", "event":"Market", "MarketID":3528735744, "StationName":"Hume Beacon", "StationType":"CraterOutpost", "StarSystem":"Sedimo" }

        public long MarketId;
        public string StationName;
        public string StationType;
        public string CarrierDockingAccess;
        public string StarSystem;
    }

    class Interdicted: JournalEntry
    {
        // { "timestamp":"2025-01-31T04:13:05Z", "event":"Interdicted", "Submitted":false, "Interdictor":"Geno Garon", "IsPlayer":true, "CombatRank":10 }

        public bool Submitted;
        public string Interdictor;
        public bool IsPlayer;
        public int CombatRank;
    }
}