using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SrvSurvey
{
    // See details from:
    // https://elite-journal.readthedocs.io/en/latest/

    class JournalEntry
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime timestamp { get; set; }
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
        public double ShipID { get; set; }
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

    class Location : LocationEntry, ISystemAddress, ISystemDataStarter
    {
        // { "timestamp":"2023-01-11T03:44:33Z", "event":"Location", "Latitude":-15.325527, "Longitude":6.507746, "DistFromStarLS":513.174241, "Docked":false, "InSRV":true, "StarSystem":"Col 173 Sector JX-K b24-0", "SystemAddress":684107179361, "StarPos":[993.06250,-188.18750,-173.53125], "SystemAllegiance":"Guardian", "SystemEconomy":"$economy_None;", "SystemEconomy_Localised":"None", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", "SystemGovernment":"$government_None;", "SystemGovernment_Localised":"None", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "SystemSecurity_Localised":"Anarchy", "Population":0, "Body":"Col 173 Sector JX-K b24-0 B 4", "BodyID":26, "BodyType":"Planet" }

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
        public BodyType BodyType { get; set; }
    }

    class Died : JournalEntry
    {
        // { "timestamp":"2023-11-13T11:24:26Z", "event":"Died" }
    }

    class ApproachSettlement : LocationEntry, ISystemAddress
    {
        // { "timestamp":"2023-01-06T00:14:38Z", "event":"ApproachSettlement", "Name":"$Ancient_Tiny_001:#index=1;", "Name_Localised":"Guardian Structure", "SystemAddress":2881788519801, "BodyID":7, "BodyName":"Col 173 Sector IJ-G b27-1 A 1", "Latitude":-33.219879, "Longitude":87.628571 }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long SystemAddress { get; set; }
        public int BodyID { get; set; }
        public string BodyName { get; set; }
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

    class Liftoff : Touchdown
    {
        // { "timestamp":"2023-01-06T06:05:03Z", "event":"Liftoff", "PlayerControlled":true, "Taxi":false, "Multicrew":false, "StarSystem":"Synuefe NL-N c23-4", "SystemAddress":1184840454858, "Body":"Synuefe NL-N c23-4 B 3", "BodyID":18, "OnStation":false, "OnPlanet":true, "Latitude":-30.542755, "Longitude":-24.187830, "NearestDestination":"$Ancient:#index=2;", "NearestDestination_Localised":"Ancient Ruins (2)" }
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
        public int BodyID { get; set; }

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
        public string BodyType { get; set; }
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

    class FSDJump : JournalEntry, ISystemAddress, ISystemDataStarter
    {
        // { "timestamp":"2023-01-24T05:07:01Z", "event":"FSDJump", "Taxi":false, "Multicrew":false, "StarSystem":"Maridwyn", "SystemAddress":13866167838129, "StarPos":[90.46875,16.40625,21.62500], "SystemAllegiance":"Federation", "SystemEconomy":"$economy_Agri;", "SystemEconomy_Localised":"Agriculture", "SystemSecondEconomy":"$economy_Refinery;", "SystemSecondEconomy_Localised":"Refinery", "SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Corporate", "SystemSecurity":"$SYSTEM_SECURITY_high;", "SystemSecurity_Localised":"High Security", "Population":4058074576, "Body":"Maridwyn A", "BodyID":1, "BodyType":"Star", "Powers":[ "Felicia Winters" ], "PowerplayState":"Exploited", "JumpDist":8.278, "FuelUsed":0.091548, "FuelLevel":13.458453, "Factions":[ { "Name":"Social Maridwyn Green Party", "FactionState":"None", "Government":"Democracy", "Influence":0.027559, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"p Velorum Crimson Creative Int", "FactionState":"None", "Government":"Corporate", "Influence":0.059055, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":4.187500 }, { "Name":"Maridwyn Co", "FactionState":"None", "Government":"Corporate", "Influence":0.487205, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, "RecoveringStates":[ { "State":"Boom", "Trend":0 } ] }, { "Name":"Maridwyn Constitution Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.041339, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"Maridwyn Gold Electronics Ltd", "FactionState":"None", "Government":"Corporate", "Influence":0.021654, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"United Maridwyn Law Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.040354, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, { "Name":"Federal Reclamation Co", "FactionState":"Expansion", "Government":"Corporate", "Influence":0.322835, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":97.425003, "ActiveStates":[ { "State":"Expansion" } ] } ], "SystemFaction":{ "Name":"Maridwyn Co" } }

        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public double[] StarPos { get; set; } // [90.46875,16.40625,21.62500]
        public string SystemAllegiance { get; set; } // Federation"
        public string SystemEconomy { get; set; } //$economy_Agri;
        public string SystemEconomy_Localised { get; set; } // "Agriculture"
        public string SystemSecondEconomy { get; set; } // $economy_Refinery;
                                                        // public string SystemSecondEconomy_Localised { get; set; } // Refinery
        public string SystemGovernment { get; set; }// $government_Corporate;
        public string SystemGovernment_Localised { get; set; } // Corporate
        public string SystemSecurity { get; set; } // $SYSTEM_SECURITY_high;
        public string SystemSecurity_Localised { get; set; } // High Security
        public long Population { get; set; }// 4058074576,
        public string Body { get; set; } // "Maridwyn A",
        public int BodyID { get; set; } // 1,
        public BodyType BodyType { get; set; }  // "Star"

        public List<string> Powers { get; set; } // [ "Felicia Winters" ]
        public string PowerplayState { get; set; } // Exploited
        public float JumpDist { get; set; }// 8.278
        public float FuelUsed { get; set; } // 0.091548
        public float FuelLevel { get; set; } // 13.458453
        // "Factions":[
        //      { "Name":"Social Maridwyn Green Party", "FactionState":"None", "Government":"Democracy", "Influence":0.027559, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"p Velorum Crimson Creative Int", "FactionState":"None", "Government":"Corporate", "Influence":0.059055, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":4.187500 },
        //      { "Name":"Maridwyn Co", "FactionState":"None", "Government":"Corporate", "Influence":0.487205, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, "RecoveringStates":[ { "State":"Boom", "Trend":0 } ] },
        //      { "Name":"Maridwyn Constitution Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.041339, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"Maridwyn Gold Electronics Ltd", "FactionState":"None", "Government":"Corporate", "Influence":0.021654, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"United Maridwyn Law Party", "FactionState":"None", "Government":"Dictatorship", "Influence":0.040354, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 },
        //      { "Name":"Federal Reclamation Co", "FactionState":"Expansion", "Government":"Corporate", "Influence":0.322835, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":97.425003, "ActiveStates":[ { "State":"Expansion" } ] }
        //  ],
        // "SystemFaction":{ "Name":"Maridwyn Co" } }

    }


    class CarrierJump : JournalEntry, ISystemAddress, ISystemDataStarter
    {
        // { "timestamp":"2023-12-01T04:50:10Z", "event":"CarrierJump", "Docked":true, "StationName":"H6B-5HQ", "StationType":"FleetCarrier", "MarketID":3708733696, "StationFaction":{ "Name":"FleetCarrier" }, "StationGovernment":"$government_Carrier;", "StationGovernment_Localised":"Private Ownership", "StationServices":[ "dock", "autodock", "commodities", "contacts", "exploration", "outfitting", "crewlounge", "rearm", "refuel", "repair", "shipyard", "engineer", "flightcontroller", "stationoperations", "stationMenu", "carriermanagement", "carrierfuel", "livery", "socialspace", "bartender", "vistagenomics" ], "StationEconomy":"$economy_Carrier;", "StationEconomy_Localised":"Private Enterprise", "StationEconomies":[ { "Name":"$economy_Carrier;", "Name_Localised":"Private Enterprise", "Proportion":1.000000 } ], "Taxi":false, "Multicrew":false, "StarSystem":"Wregoe VG-U b17-0", "SystemAddress":681155568793, "StarPos":[764.15625,174.68750,-675.90625], "SystemAllegiance":"", "SystemEconomy":"$economy_None;", "SystemEconomy_Localised":"None", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", "SystemGovernment":"$government_None;", "SystemGovernment_Localised":"None", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "SystemSecurity_Localised":"Anarchy", "Population":0, "Body":"Wregoe VG-U b17-0 A", "BodyID":1, "BodyType":"Star" }

        public bool Docked { get; set; }
        public string StationName { get; set; }
        public string StationType { get; set; }
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
        public BodyType BodyType { get; set; }
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

    class FSSAllBodiesFound : JournalEntry
    {
        // { "timestamp":"2023-09-04T02:29:13Z", "event":"FSSAllBodiesFound", "SystemName":"HIP 51721", "SystemAddress":358596317890, "Count":10 }
        public string SystemName;
        public long SystemAddress;
        public int Count;
    }

    class Scan : JournalEntry, ISystemAddress
    {
        // See: https://elite-journal.readthedocs.io/en/latest/Exploration/#scan
        // { "timestamp":"2023-02-07T06:12:44Z", "event":"Scan", "ScanType":"Detailed", "BodyName":"Synuefe TP-F b44-0 AB 7", "BodyID":14, "Parents":[ {"Null":1}, {"Null":0} ], "StarSystem":"Synuefe TP-F b44-0", "SystemAddress":682228131193, "DistanceFromArrivalLS":163.640473, "TidalLock":false, "TerraformState":"", "PlanetClass":"High metal content body", "Atmosphere":"thin ammonia atmosphere", "AtmosphereType":"Ammonia", "AtmosphereComposition":[ { "Name":"Ammonia", "Percent":100.000000 } ], "Volcanism":"", "MassEM":0.012165, "Radius":1506425.250000, "SurfaceGravity":2.136633, "SurfaceTemperature":171.117615, "SurfacePressure":218.443268, "Landable":true, "Materials":[ { "Name":"iron", "Percent":22.081503 }, { "Name":"nickel", "Percent":16.701525 }, { "Name":"sulphur", "Percent":15.735665 }, { "Name":"carbon", "Percent":13.232062 }, { "Name":"manganese", "Percent":9.119436 }, { "Name":"phosphorus", "Percent":8.471395 }, { "Name":"zinc", "Percent":6.000926 }, { "Name":"germanium", "Percent":4.639404 }, { "Name":"molybdenum", "Percent":1.441909 }, { "Name":"ruthenium", "Percent":1.363674 }, { "Name":"tungsten", "Percent":1.212496 } ], "Composition":{ "Ice":0.000000, "Rock":0.672827, "Metal":0.327173 }, "SemiMajorAxis":51394448280.334473, "Eccentricity":0.173554, "OrbitalInclination":-164.752899, "Periapsis":243.912489, "OrbitalPeriod":9480342.149734, "AscendingNode":-98.783954, "MeanAnomaly":62.655478, "RotationPeriod":127770.138485, "AxialTilt":-0.440350, "WasDiscovered":true, "WasMapped":true }

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

        // TODO: Parents: Array of BodyType:BodyID pairs

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
        // TODO: AtmosphereComposition: [array of info]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Volcanism { get; set; } // see §15.5
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double SurfaceGravity { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double SurfacePressure { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool Landable { get; set; } // : true (if landable)

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
        public long MissionID;
        public string Name;
        public bool PassengerMission;
        public long Expires;
    }

    class MissionAccepted : JournalEntry
    {
        // { "timestamp":"2024-01-04T04:30:06Z", "event":"MissionAccepted", "Faction":"Meene General Industries", "Name":"Mission_TheDead", "LocalisedName":"Decoding the Ancient Ruins", "Wing":false, "Influence":"None", "Reputation":"None", "MissionID":949931893 }

        public string Faction;
        public string Name;
        public long MissionID;
        public string LocalisedName;
        public bool Wing;
        public string Influence;
        public string Reputation;
        public long Reward;

        // TODO: the rest...
    }

    class MissionCompleted: JournalEntry
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

    class MissionAbandoned: JournalEntry
    {
        // { "timestamp":"2024-01-04T04:55:12Z", "event":"MissionAbandoned", "Name":"Mission_TheDead_name", "LocalisedName":"Decoding the Ancient Ruins", "MissionID":949931893 }
        public string Name;
        public long MissionID;
        public string LocalisedName;
    }

}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
