using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey.canonn
{
    internal class StarSystem
    {
        public string systemName;
        public StarPos pos;

        public static StarSystem Sol = new StarSystem
        {
            systemName = "Sol",
            pos = new StarPos(0, 0, 0),
        };
    }

    internal class Region
    {
        public string name;
        public int region;
    }

    internal class Timestamps
    {
        public DateTimeOffset distanceToArrival; //: 2023-03-18T18:29:28, 
        public DateTimeOffset meanAnomaly; //: 2023-03-18T18:29:28
    }

    internal enum CanonnType
    {
        Star,
        Planet,
        Barycentre
    }

    internal class BodyBase
    {
        public long id64; // : 36036076585428353, 
        public CanonnType type; // : Star, 
        public DateTimeOffset updateTime; // : 2023-03-18 18:29:28+00
        public int bodyId; // : 1, 
        public string name; // : Col 173 Sector PF-E b28-3 A, 
        public double distanceToArrival; // : 0.0, 
        public Timestamps timestamps;
        // timestamps: {
        //   distanceToArrival: 2023-03-18T18:29:28, 
        //   meanAnomaly: 2023-03-18T18:29:28
        // }, 
    }

    internal class Star : BodyBase
    {
        public double absoluteMagnitude; // : 9.629074, 
        public int age; // : 6018, 
        public double argOfPeriapsis; // : 269.337595, 
        public double ascendingNode; // : -132.232712, 
        public double axialTilt; // : 0.0, 
        public string luminosity; // : Va, 
        public bool mainStar; // : true, 
        public double meanAnomaly; // : 346.993797, 
        public double orbitalEccentricity; // : 0.250834, 
        public double orbitalInclination; // : 18.490047, 
        public double orbitalPeriod; // : 4112.48752088458, 
        //parents: [
        //  {
        //    Null: 0
        //  }
        //], 
        public double rotationalPeriod; // : 2.46148771125, 
        public bool rotationalPeriodTidallyLocked; // : false, 
        public double semiMajorAxis; // : 1.61398112830458, 
        public double solarMasses; // : 0.332031, 
        public double solarRadius; // : 0.489135712437096, 
        public string spectralClass; // : M5, 
                                     // stations: [], 
        public string subType; // : M (Red dwarf) Star, 
        public double surfaceTemperature; // : 2737.0, 
    }

    internal class Signals
    {
        public List<string> biology;
        public List<string> genuses;
        public List<string> geology;
        public List<string> guardian;
        public Dictionary<string, int> signals;
    }

    internal class Planet : BodyBase
    {
        public double argOfPeriapsis; // : 132.02895, 
        public double ascendingNode; // : -179.836934, 
        public string atmosphereType; // : null, 
        public double axialTilt; // : 0.165382, 

        public double earthMasses; // : 0.006787, 
        public double gravity; // : 0.178385846844091, 
        public bool isLandable; // : true, 
        public Dictionary<string, double> materials;
        /*materials: {
          Cadmium: 1.622977, 
          Carbon: 12.454605, 
          Chromium: 9.399409, 
          Iron: 20.899969, 
          Manganese: 8.631474, 
          Nickel: 15.807859, 
          Niobium: 1.428401, 
          Phosphorus: 7.973651, 
          Ruthenium: 1.290706, 
          Sulphur: 14.811105, 
          Zinc: 5.679829
        },*/
        public double meanAnomaly; // : 244.993055, 

        public double orbitalEccentricity; // : 0.025323, 
        public double orbitalInclination; // : -7.49158, 
        public double orbitalPeriod; // : 0.946358901759259, 
        public List<Dictionary<string, int>> parents;
        /*parents: [
          {
            Null: 8
          }, 
          {
            Star: 2
          }, 
          {
            Null: 0
          }
        ],*/
        public double radius; // : 1243.482625, 
        public double rotationalPeriod; // : 1.39955643063657, 
        public bool rotationalPeriodTidallyLocked; // : true, 
        public double semiMajorAxis; // : 2.88837836692284e-05, 
        public Signals signals;
        /*signals: {
          biology: [
            Roseum Brain Tree
          ], 
          genuses: [
            $Codex_Ent_Brancae_Name;
          ], 
          geology: [
            Silicate Vapour Fumarole, 
            Silicate Magma Lava Spout, 
            Silicate Vapour Gas Vent
          ], 
          guardian: [
            Guardian Codex, 
            Guardian Data Terminal, 
            Guardian Relic Tower, 
            Guardian Pylon, 
            Guardian Sentinel
          ], 
          signals: {
            $SAA_SignalType_Biological;: 1, 
            $SAA_SignalType_Geological;: 3, 
            $SAA_SignalType_Guardian;: 3
          }, 
          updateTime: 2023-03-26 00:57:36+00
        }, */
        public Dictionary<string, double> solidComposition;
        /*solidComposition: {
          Ice: 0.0, 
          Metal: 32.9064, 
          Rock: 67.0936
        },*/
        //stations: [], 
        public string subType; // : High metal content world, 
        public double surfacePressure; // : 0.0, 
        public double surfaceTemperature; // : 285.962128, 
        public string terraformingState; // : Not terraformable, 
        public string volcanismType; // : Minor Silicate Vapour Geysers
    }

    internal class System
    {
        public string allegiance;
        public List<BodyBase> bodies;
        public int bodyCount;
        public StarPos coords;
        public DateTimeOffset date;
        public string government;
        public long id64;
        public string name;
        public long population;
        public string primaryEconomy;
        public Region region;
        public string secondaryEconomy;
        public string security;
    }

    internal class BioStats
    {
        public System system;
    }
    internal class FSSsignals
    {
        // "isStation": null,
        public string signalname; // $Fixed_Event_Life_Cloud;
        public string signalnamelocalised; // Notable stellar phenomena"
    }


    internal class SAAsignals
    {
        public string body; //: "B 1",
        public int count; //: 3,
        public string english_name; //: "Guardian",
        public string hud_category; //: "Guardian"
    }

    internal class Codex
    {
        public string body; //: "A 4",
        public string english_name; //: "Bacterium Cerbrus - Teal",
        public long? entryid; //: 2321207,
        public string hud_category; //: "Biology",
        public long? index_id; // : null,
        public double? latitude; //: null,
        public double? longitude; //: null,
        public bool scanned; //: "false"
    }

    internal class SystemPoi
    {
        public List<FSSsignals> FSSsignals;
        public List<SAAsignals> SAAsignals;
        public string cmdrName;
        public List<Codex> codex;
        public string odyssey;
        public string system;
    }

    internal class CanonnBioStats
    {
        // atmosComposition
        public List<string> atmosphereType;
        public List<string> bodies;
        public long count;
        public string fdevname;
        public CanonnBioStatHistograms histograms;
        public string hud_category;
        public string id;
        public List<string> localStars;
        public List<string> materials;
        public double? maxd;
        public double? maxg;
        public double? maxp;
        public double? maxt;
        public double? mind;
        public double? ming;
        public double? minp;
        public double? mint;
        public string name;
        public string? platform;
        public List<string> primaryStars;
        public List<string> regions;
        public long reward;
        public List<string> solidComposition;
        public List<string> systemBodyTypes;
        public List<string> volcanism;
    }

    internal class CanonnBioStatHistograms
    {
        public Dictionary<string, int> atmos_types;
        public Dictionary<string, int> body_types;
        public List<MinMaxValue> distance;
        public List<MinMaxValue> gravity;
        public Dictionary<string, int> local_stars;
        public Dictionary<string, int> materials;
        public List<MinMaxValue> pressure;
        public Dictionary<string, int> primary_stars;
        public Dictionary<string, int> system_bodies;
        public List<MinMaxValue> temperature;
        public Dictionary<string, int> volcanic_body_types;

        public class MinMaxValue
        {
            public double min;
            public double max;
            public double value;
        }
    }

    internal class SystemBioSignal
    {
        public string poiName;
        public string genusName;
        public string credits;
        public long reward;
        public List<TrackingDelta> trackers = new List<TrackingDelta>();
    }

    internal class OrganicRewards
    {
        public long entryid; //         2100201
        //public string name; //         "$Codex_Ent_Seed_Name;"
        public long reward; //          1593700
    }

    internal class RefCodexEntry
    {
        // https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/ref
        public string category; //     "$Codex_Category_Biology;"
        public string english_name; // "Roseum Brain Tree"
        public string entryid; //         2100201 // this should remain a string as we sub-string on it
        public string hud_category; // "Biology"
        public string name; //         "$Codex_Ent_Seed_Name;"
        public string platform; //     "legacy"
        public long? reward; //          1593700
        public string sub_category; // "$Codex_SubCategory_Organic_Structures;"
        public string sub_class; //    "Brain Tree"

        public string dump; // a url to download a .csv file
        public string image_cmdr;
        public string image_url;
    }

    internal class GRSitesData
    {
        public GRSites data;
    }

    internal class GRSites
    {
        public List<GRSite> grsites;
    }

    internal class GRSite
    {
        // { "siteID": 62, "latitude": 2.4, "longitude": 132.05, "frontierID": null, "system": { "id": "826", "systemName": "COL 173 SECTOR OE-P D6-11", "edsmID": 9822862}, "body": { "bodyName": "COL 173 SECTOR OE-P D6-11 C 2", "edsmID": 2237539 }, "type": { "type": "Gamma"} },

        public string id;
        public int siteID;
        public DateTimeOffset updated_at;
        public DateTimeOffset created_at;
        public double latitude;
        public double longitude;
        public int? frontierID;
        public System system;
        public Body body;
        public Type type;
        public string visible;
        public string verified;


        public class System
        {
            public string id;
            public long id64;
            public string systemName;
            public long edsmID;
            public double edsmCoordX;
            public double edsmCoordY;
            public double edsmCoordZ;
        }

        public class Body
        {
            public string bodyName;
            public int bodyID;
            public long edsmID;
            public double distanceToArrival;
        }

        public class Type
        {
            public string type;
        }
    }

    internal class GRReportsData
    {
        public GRReports data;
    }

    internal class GRReports
    {
        public List<GRReport> grreports;
    }


    internal class GRReport
    {
        public DateTimeOffset updated_at;
        public string type;
        public double latitude;
        public double longitude;
        public string cmdrName;
        public string bodyName;
        public int frontierID;
    }

    /// <summary>
    /// Data pulled from allRuins.json
    /// </summary>
    internal class GuardianRuinSummary
    {
        /// <summary> The ID number used by Canonn in GRSites. </summary>
        public int siteID;
        public string systemName;
        public long systemAddress;
        public string bodyName;
        public int bodyId;
        public string siteType;
        /// <summary> The site number, 1 based. </summary>
        public int idx;
        public DateTimeOffset lastUpdated;
        public double distanceToArrival;
        public double[] starPos;
        public double latitude = double.NaN;
        public double longitude = double.NaN;
        public int siteHeading = -1;
        public int relicTowerHeading = -1;
        /// <summary> A percentage of completion </summary>
        public int surveyProgress;
        [JsonIgnore]
        public bool surveyComplete { get => surveyProgress == 100; }

        // properties whose values differ live from legacy
        public double legacyLatitude = double.NaN;
        public double legacyLongitude = double.NaN;
        public int legacySiteHeading = -1;
        public int legacyRelicTowerHeading = -1;

        [JsonIgnore]
        public string fullBodyName { get => $"{this.systemName} {this.bodyName}"; }

        [JsonIgnore]
        public string fullBodyNameWithIdx { get => $"{this.systemName} {this.bodyName}, Ruins #{this.idx}"; }

        [JsonIgnore]
        public bool missingLiveLatLong { get => Double.IsNaN(this.latitude) || Double.IsNaN(this.longitude) || (this.latitude == 0 && this.longitude == 0); }
        [JsonIgnore]
        public bool missingLegacyLatLong { get => Double.IsNaN(this.legacyLatitude) || Double.IsNaN(this.legacyLongitude) || (this.legacyLatitude == 0 && this.legacyLongitude == 0); }

        public static GuardianRuinSummary from(GRSite _)
        {
            return new GuardianRuinSummary
            {
                siteID = _.siteID,
                systemName = _.system.systemName,
                systemAddress = _.system.id64,
                bodyName = _.body.bodyName.Replace(_.system.systemName, "").Trim(),
                bodyId = _.body.bodyID,
                siteType = _.type.type,
                idx = _.frontierID ?? 0,
                distanceToArrival = _.body.distanceToArrival,
                starPos = new double[] {
                    _.system.edsmCoordX,
                    _.system.edsmCoordY,
                    _.system.edsmCoordZ
                },
                latitude = _.latitude,
                longitude = _.longitude,
            };
        }
    }

    /// <summary>
    /// Summaries augmented from own data
    /// </summary>
    internal class GuardianRuinEntry : GuardianRuinSummary
    {
        public static string PleaseShareMessage = "** discovered data - please share **\r\n";

        public DateTimeOffset lastVisited;
        public double systemDistance;
        public string notes = "";

        public GuardianRuinEntry(GuardianRuinSummary summary)
        {
            if (summary != null)
            {
                base.siteID = summary.siteID;
                base.systemName = summary.systemName;
                base.systemAddress = summary.systemAddress;
                base.bodyName = summary.bodyName;
                base.bodyId = summary.bodyId;
                base.siteType = summary.siteType;
                base.idx = summary.idx;
                base.distanceToArrival = summary.distanceToArrival;
                base.starPos = summary.starPos;
                base.latitude = summary.latitude;
                base.longitude = summary.longitude;
                base.siteHeading = summary.siteHeading;
                base.relicTowerHeading = summary.relicTowerHeading;
                base.legacyLatitude = summary.legacyLatitude;
                base.legacyLongitude = summary.legacyLongitude;
                base.legacySiteHeading = summary.legacySiteHeading;
                base.legacyRelicTowerHeading = summary.legacyRelicTowerHeading;
                base.surveyProgress = summary.surveyProgress;
            }
        }

        public string name
        {
            get => $"{this.systemName} {this.bodyName} {this.idx}";
        }

        /*
        public void merge(GuardianSiteData data)
        {
            // TODO: revisit 
            //if (this.missingLiveLatLong || (this.siteHeading < 0 && data.siteHeading >= 0) || (this.relicTowerHeading < 0 && data.relicTowerHeading >= 0))
            //    this.notes += PleaseShareMessage;

            this.latitude = data.location.Lat;
            this.longitude = data.location.Long;
            this.siteHeading = data.siteHeading;
            this.relicTowerHeading = data.relicTowerHeading;
            if (data.lastVisited != DateTimeOffset.MinValue)
                this.lastVisited = data.lastVisited;
            this.idx = data.index;
            if (string.IsNullOrEmpty(this.notes))
                this.notes = data.notes!;
            else
                this.notes += " | " + data.notes;
            if (!this.surveyComplete)
                this.surveyComplete = data.isSurveyComplete();
        }
        */
    }

    #region spansh types

    internal class GetSystemResponse
    {
        // {"min_max":[{"id64":10477373803,"name":"Sol","x":0.0,"y":0.0,"z":0.0},{"id64":1458376315610,"name":"Solati","x":66.53125,"y":29.1875,"z":34.6875},{"id64":5059379007779,"name":"Solitude","x":-9497.65625,"y":-911.0,"z":19807.625},{"id64":5267550898539,"name":"Solibamba","x":99.5625,"y":40.125,"z":26.8125},{"id64":11538024121505,"name":"Sollaro","x":-9528.625,"y":-885.59375,"z":19815.4375}],"values":["Sol","Solati","Solitude","Solibamba","Sollaro"]}

        public List<GetSystemMinMax> min_max;
        public List<string> values;
    }

    internal class GetSystemMinMax
    {
        public long id64;
        public string name;
        public double x;
        public double y;
        public double z;
    }

    #endregion


    internal class GuardianSiteSummary
    {
        /// <summary> The ID number used by Canonn in GSSites, etc </summary>
        public int siteID;
        public string systemName;
        public long systemAddress = -1;
        public string bodyName;
        public int bodyId = -1;
        public double distanceToArrival;
        public double[] starPos;
        public string siteType;
        public double latitude = double.NaN;
        public double longitude = double.NaN;
        public int siteHeading = -1;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int idx;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int surveyProgress;
        [JsonIgnore]
        public bool surveyComplete { get => this.surveyProgress == 100; }

        [JsonIgnore]
        public bool isRuins { get => siteType == "Alpha" || siteType == "Beta" || siteType == "Gamma"; }
    }

    internal class GuardianBeaconSummary : GuardianSiteSummary
    {
        public string relatedStructure;
        public double relatedStructureDist;
    }

    internal class GuardianGridEntry : GuardianSiteSummary
    {
        public DateTimeOffset lastVisited;
        public double systemDistance;
        public string notes = "";
        public string ramTahLogs = "";

        public double relatedStructureDist;
        public string filepath;
        public bool hasDiscoveredData;
        public SurveyCompletionStatus? surveyStatus;

        public string fullBodyName { get => $"{this.systemName} {this.bodyName}"; }

        public string fullBodyNameWithIdx { get => $"{this.systemName} {this.bodyName}, Ruins #{this.idx}"; }

        public GuardianGridEntry(GuardianBeaconSummary summary)
        {
            if (summary != null)
            {
                base.systemName = summary.systemName;
                base.systemAddress = summary.systemAddress;
                base.bodyName = summary.bodyName;
                base.bodyId = summary.bodyId;
                base.distanceToArrival = summary.distanceToArrival;
                base.starPos = summary.starPos;
                this.relatedStructureDist = summary.relatedStructureDist;
                base.siteType = "Beacon";
                this.notes = $"Structure: {summary.relatedStructure}, dist: " + summary.relatedStructureDist.ToString("N2") + " ly";
                base.surveyProgress = summary.surveyProgress;
            }
        }

        public GuardianGridEntry(GuardianSiteSummary summary)
        {
            if (summary != null)
            {
                base.siteID = summary.siteID;
                base.systemName = summary.systemName;
                base.systemAddress = summary.systemAddress;
                base.bodyName = summary.bodyName;
                base.bodyId = summary.bodyId;
                base.idx = 1; // summary.idx;
                base.distanceToArrival = summary.distanceToArrival;
                base.starPos = summary.starPos;
                base.siteType = summary.siteType;
                base.latitude = summary.latitude;
                base.longitude = summary.longitude;
                base.surveyProgress = summary.surveyProgress;
            }
        }

        public GuardianGridEntry(GuardianRuinSummary summary)
        {
            if (summary != null)
            {
                base.siteID = summary.siteID;
                base.systemName = summary.systemName;
                base.systemAddress = summary.systemAddress;
                base.bodyName = summary.bodyName;
                base.bodyId = summary.bodyId;
                base.siteType = summary.siteType;
                base.idx = summary.idx;
                base.distanceToArrival = summary.distanceToArrival;
                base.starPos = summary.starPos;
                base.latitude = summary.latitude;
                base.longitude = summary.longitude;
                base.surveyProgress = summary.surveyProgress;
            }
        }

        public void merge(GuardianBeaconData data)
        {
            this.lastVisited = data.lastVisited;
            this.notes += data.notes;

            this.filepath = data.filepath;
        }

        public void merge(GuardianSiteData data)
        {
            if (data.lastVisited != DateTimeOffset.MinValue)
                this.lastVisited = data.lastVisited;
            this.notes += data.notes;

            if (!this.surveyComplete)
            {
                this.surveyStatus = data.getCompletionStatus();
                this.surveyProgress = this.surveyStatus.progress;
            }

            this.filepath = data.filepath;
            this.hasDiscoveredData = data.hasDiscoveredData();
        }
    }
}
