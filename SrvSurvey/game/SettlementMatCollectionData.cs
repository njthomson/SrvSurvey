using Newtonsoft.Json;
using SrvSurvey.canonn;
using System.Globalization;

namespace SrvSurvey.game
{
    internal class SettlementMatCollectionData : Data
    {
        #region static loading code

        private static string rootFolder = Path.Combine(Program.dataFolder, "footMatStats");

        private static string getFilename(ApproachSettlement entry)
        {
            return $"{entry.SystemAddress}-{entry.MarketID}-{entry.Name}.json";
        }

        public static SettlementMatCollectionData Load(CanonnStation station)
        {
            var fid = Game.activeGame?.fid ?? Game.settings.lastFid!;
            var folder = Path.Combine(rootFolder, fid!);
            if (!Util.isOdyssey) folder = Path.Combine(folder, "legacy");

            Directory.CreateDirectory(folder);
            var filepath = Directory.GetFiles(folder, $"{station.systemAddress}-{station.marketId}-*.json").LastOrDefault();

            // attempt to load latest entry
            SettlementMatCollectionData? data = null;
            if (filepath != null && File.Exists(filepath))
                data = Data.Load<SettlementMatCollectionData>(filepath);

            if (data?.completed == true)
            {
                // if we undocked, those stats are complete - start a fresh batch
                data = null;
            }

            if (data == null)
            {
                var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HHmmss");

                // if needed, create an empty object
                data = new SettlementMatCollectionData()
                {
                    filepath = Path.Combine(folder, $"{station.systemAddress}-{station.marketId}-{timestamp}.json"),
                    name = station.name,
                    marketId = station.marketId,
                    subType = station.subType,
                };
            }

            return data;
        }

        #endregion

        // Settlement data from ApproachSettlement journal, eg:
        //   { "timestamp":"2024-06-16T21:51:24Z", "event":"ApproachSettlement", "Name":"Omenuko Extraction Base", "MarketID":3928215040, "StationFaction":{ "Name":"Yami & Co" }, "StationGovernment":"$government_Corporate;", "StationGovernment_Localised":"Corporate", "StationAllegiance":"Federation", "StationServices":[ "dock", "autodock", "commodities", "contacts", "missions", "refuel", "repair", "engineer", "missionsgenerated", "facilitator", "flightcontroller", "stationoperations", "searchrescue", "stationMenu" ], "StationEconomy":"$economy_Extraction;", "StationEconomy_Localised":"Extraction", "StationEconomies":[ { "Name":"$economy_Extraction;", "Name_Localised":"Extraction", "Proportion":1.000000 } ], "SystemAddress":2868367467953, "BodyID":5, "BodyName":"Yami A 1", "Latitude":0.550911, "Longitude":-31.254198 }

        public string name;
        public long marketId;
        public long systemAddress;
        public int bodyId;
        public bool completed;

        public string factionName;
        public string stationGovernment;
        public string stationAllegiance;
        public string stationEconomy; // the primary one we see in game UX

        // SrvSurvey calculates this to match https://www.quizengine.co.uk/omg/live/omg_1.1.html, etc
        public int subType;

        // Star-system data - from FSDJump event
        //   { "timestamp":"2024-06-16T07:41:53Z", "event":"FSDJump", "Taxi":false, "Multicrew":false, "StarSystem":"Saurami", "SystemAddress":669344212409, "StarPos":[-123.31250,99.12500,39.12500], "SystemAllegiance":"Independent", "SystemEconomy":"$economy_Extraction;", "SystemEconomy_Localised":"Extraction", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", "SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Corporate", "SystemSecurity":"$SYSTEM_SECURITY_low;", "SystemSecurity_Localised":"Low Security", "Population":1300, "Body":"Saurami A", "BodyID":2, "BodyType":"Star", "JumpDist":14.703, "FuelUsed":0.966514, "FuelLevel":15.033485, "Factions":[ { "Name":"Workers of Saurami League", "FactionState":"None", "Government":"Confederacy", "Influence":0.064000, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":17.900000 }, { "Name":"Stardreamer Systems", "FactionState":"None", "Government":"Corporate", "Influence":0.015000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.371600, "PendingStates":[ { "State":"Retreat", "Trend":0 } ] }, { "Name":"Gundiae Allied Organisation", "FactionState":"None", "Government":"Corporate", "Influence":0.038000, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":20.000000 }, { "Name":"Aobriguites Blue Bridge Corp", "FactionState":"None", "Government":"Corporate", "Influence":0.039000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":55.552898 }, { "Name":"Raven Colonial Corporation", "FactionState":"Boom", "Government":"Corporate", "Influence":0.783000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "SquadronFaction":true, "MyReputation":100.000000, "PendingStates":[ { "State":"Expansion", "Trend":0 } ], "RecoveringStates":[ { "State":"PublicHoliday", "Trend":0 } ], "ActiveStates":[ { "State":"Boom" } ] }, { "Name":"Workers of HIP 85414 Free", "FactionState":"Retreat", "Government":"Democracy", "Influence":0.020000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":-52.000000, "ActiveStates":[ { "State":"Retreat" } ] }, { "Name":"Steven Gordon Jolliffe", "FactionState":"None", "Government":"Corporate", "Influence":0.041000, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":70.000000 } ], "SystemFaction":{ "Name":"Raven Colonial Corporation", "FactionState":"Boom" } }
        public string systemAllegiance;
        public string systemEconomy;
        public string systemSecondEconomy;
        public string systemGovernment;
        public string systemSecurity;
        public long population;
        public string systemFactionName;
        public string systemFactionState;

        // Manually entered:
        public int threatLevel = -1; // from system map, or from external panel: 0 = empty shield, 1 = half filled shield, 2 = filled shield

        // Auto-collected:
        public int totalMatCount = 0;

        // Mats Collected 
        public Dictionary<string, int> countMats = new Dictionary<string, int>();

        // Mat types Collected 
        public Dictionary<string, int> countTypes = new Dictionary<string, int>();

        // Mat types Collected 
        public Dictionary<string, int> countBuildings= new Dictionary<string, int>();

        public List<CollectedMaterial> matLocations = new List<CollectedMaterial>();

        public void track(BackpackChange_Entry item, PointF pf, string? building)
        {
            this.totalMatCount += item.Count;

            // increment basic count of Mat
            if (!this.countMats.ContainsKey(item.Name)) this.countMats[item.Name] = 0;
            this.countMats[item.Name] += item.Count;

            // increment count of Mat type
            if (!this.countTypes.ContainsKey(item.Type)) this.countTypes[item.Type] = 0;
            this.countTypes[item.Type] += item.Count;

            // increment count per building
            if (building != null)
            {
                if (!this.countBuildings.ContainsKey(building)) this.countBuildings[building] = 0;
                this.countBuildings[building] += item.Count;
            }

            // and track the location
            this.matLocations.Add(new CollectedMaterial(item.Name, item.Type, pf));
        }

        public void track(CollectItems entry, PointF pf, string? building)
        {
            this.totalMatCount += entry.Count;

            // increment basic count of Mat
            if (!this.countMats.ContainsKey(entry.Name)) this.countMats[entry.Name] = 0;
            this.countMats[entry.Name] += entry.Count;

            // increment count of Mat type
            if (!this.countTypes.ContainsKey(entry.Type)) this.countTypes[entry.Type] = 0;
            this.countTypes[entry.Type] += entry.Count;

            // increment count per building
            if (building != null)
            {
                if (!this.countBuildings.ContainsKey(building)) this.countBuildings[building] = 0;
                this.countBuildings[building] += entry.Count;
            }

            // and track the location
            this.matLocations.Add(new CollectedMaterial(entry.Name, entry.Type, pf));
        }
    }

    #region JSON serializer

    [JsonConverter(typeof(CollectedMaterial.JsonConverter))]
    class CollectedMaterial
    {
        public string name;
        public string type;
        public float x;
        public float y;

        public CollectedMaterial() { }

        public CollectedMaterial(string name, string type, PointF pf)
        {
            this.name = name;
            this.type = type;
            this.x = pf.X;
            this.y = pf.Y;
        }

        public override string ToString()
        {
            return $"{name}_{type}_{x.ToString(CultureInfo.InvariantCulture)}_{y.ToString(CultureInfo.InvariantCulture)}";
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                // "{name}_{type}_{x}_{y}"
                // eg: "circuitboard_Component_12.3_45.6"
                var parts = txt.Split('_');
                var item = new CollectedMaterial()
                {
                    name = parts[0],
                    type = parts[1],
                    x = float.Parse(parts[2], CultureInfo.InvariantCulture),
                    y = float.Parse(parts[3], CultureInfo.InvariantCulture),
                };

                return item;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as CollectedMaterial;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // create a single string
                var txt = data.ToString();
                writer.WriteValue(txt);
            }
        }
    }

    #endregion
}
