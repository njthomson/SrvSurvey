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

    internal class SystemPoi
    {
        public List<SystemPoi_FSSsignals> FSSsignals;
        public List<SystemPoi_SAAsignals> SAAsignals;
        public string cmdrName;
        public List<Codex> codex;
        public string odyssey;
        public string system;

        public class Codex
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
    }

    internal class SystemPoi_FSSsignals
    {
        // "isStation": null,
        public string signalname; // $Fixed_Event_Life_Cloud;
        public string signalnamelocalised; // Notable stellar phenomena"
    }

    internal class SystemPoi_SAAsignals
    {
        public string body; //: "B 1",
        public int count; //: 3,
        public string english_name; //: "Guardian",
        public string hud_category; //: "Guardian"
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

        public class CanonnBioStatHistograms
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
    }

    internal class SystemBioSignal
    {
        public string poiName;
        public string genusName;
        public string credits;
        public long reward;
        public List<TrackingDelta> trackers = new List<TrackingDelta>();
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
        public int? reward; //          1593700
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

    internal class GRReports
    {
        public Data data;

        internal class Data
        {
            public List<Report> grreports;

            internal class Report
            {
                public DateTimeOffset updated_at;
                public string type;
                public double latitude;
                public double longitude;
                public string cmdrName;
                public string bodyName;
                public int frontierID;
            }
        }
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
        //[JsonIgnore] // TODO: Remove once enough people are on recent builds?
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
        //[JsonIgnore] // TODO: Remove once enough people are on recent builds?
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
