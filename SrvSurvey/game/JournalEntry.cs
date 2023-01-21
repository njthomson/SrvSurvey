using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    class JournalEntry
    {
        public DateTime timestamp { get; set; }
        public string @event { get; set; }
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

    class Location : JournalEntry, ILocation
    {
        // { "timestamp":"2023-01-11T03:44:33Z", "event":"Location", "Latitude":-15.325527, "Longitude":6.507746, "DistFromStarLS":513.174241, "Docked":false, "InSRV":true, "StarSystem":"Col 173 Sector JX-K b24-0", "SystemAddress":684107179361, "StarPos":[993.06250,-188.18750,-173.53125], "SystemAllegiance":"Guardian", "SystemEconomy":"$economy_None;", "SystemEconomy_Localised":"None", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", "SystemGovernment":"$government_None;", "SystemGovernment_Localised":"None", "SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "SystemSecurity_Localised":"Anarchy", "Population":0, "Body":"Col 173 Sector JX-K b24-0 B 4", "BodyID":26, "BodyType":"Planet" }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistFromStarLS { get; set; }
        public bool Docked { get; set; }
        public bool InSrv { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        // public xyz StarPos
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
        public string BodyType { get; set; }
    }

    class ApproachSettlement : JournalEntry, ILocation
    {
        // { "timestamp":"2023-01-06T00:14:38Z", "event":"ApproachSettlement", "Name":"$Ancient_Tiny_001:#index=1;", "Name_Localised":"Guardian Structure", "SystemAddress":2881788519801, "BodyID":7, "BodyName":"Col 173 Sector IJ-G b27-1 A 1", "Latitude":-33.219879, "Longitude":87.628571 }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long SystemAddress { get; set; }
        public int BodyID { get; set; }
        public string BodyName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    class Touchdown : JournalEntry, ILocation
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
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }
    }

    class Liftoff : Touchdown, ILocation
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

    class CodexEntry : JournalEntry, ILocation
    {
        // { "timestamp":"2023-01-06T00:46:41Z", "event":"CodexEntry", "EntryID":3200200, "Name":"$Codex_Ent_Guardian_Data_Logs_Name;", "Name_Localised":"Guardian Codex", "SubCategory":"$Codex_SubCategory_Guardian;", "SubCategory_Localised":"Guardian objects", "Category":"$Codex_Category_Civilisations;", "Category_Localised":"Xenological", "Region":"$Codex_RegionName_18;", "Region_Localised":"Inner Orion Spur", "System":"Synuefe NL-N c23-4", "SystemAddress":1184840454858, "BodyID":18, "NearestDestination":"$Ancient:#index=3;", "NearestDestination_Localised":"Ancient Ruins (3)", "Latitude":5.613951, "Longitude":-148.100403, "IsNewEntry":true, "VoucherAmount":50000 }

        public string EntryID { get; set; }
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

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool IsNewEntry { get; set; }
        public int VoucherAmount { get; set; }
    }

    class SendText : JournalEntry
    {
        // { "timestamp":"2023-01-07T05:44:32Z", "event":"SendText", "To":"local", "Message":"totem", "Sent":true }

        public string To { get; set; }

        public string Message { get; set; }

        public bool Sent { get; set; }
    }

    class SupercruiseExit : JournalEntry
    {
        public string Starsystem { get; set; }
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
}
