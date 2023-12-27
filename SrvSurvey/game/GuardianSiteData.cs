using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.units;
using System.Text;

namespace SrvSurvey.game
{
    [JsonConverter(typeof(GuardianSiteData.JsonConverter))]
    internal class GuardianSiteData : Data
    {
        private static string rootFolder = Path.Combine(Program.dataFolder, "guardian");

        public static string getFilename(ApproachSettlement entry)
        {
            var index = parseSettlementIdx(entry.Name);
            var isRuins = entry.Name.StartsWith("$Ancient:#index=");
            return getFilename(entry.BodyName, index, isRuins);
        }

        public static string getFilename(string bodyName, int index, bool isRuins)
        {
            var namePart = isRuins ? "ruins" : "structure";
            return $"{bodyName}-{namePart}-{index}.json";
        }

        public static GuardianSiteData? Load(string bodyName, string settlementName)
        {
            var isRuins = settlementName.StartsWith("$Ancient:") == true;
            var idx = GuardianSiteData.parseSettlementIdx(settlementName);
            return Load(bodyName, idx, isRuins);
        }

        public static GuardianSiteData? Load(string bodyName, int index, bool isRuins)
        {
            var fid = Game.activeGame?.fid ?? Game.settings.lastFid!;
            var folder = Path.Combine(rootFolder, fid!);
            if (!Util.isOdyssey) folder = Path.Combine(folder, "legacy");

            var filename = getFilename(bodyName, index, isRuins);
            var filepath = Path.Combine(folder, filename);
            return Data.Load<GuardianSiteData>(filepath);
        }

        public static GuardianSiteData Load(ApproachSettlement entry)
        {
            var fid = Game.activeGame?.fid ?? Game.settings.lastFid!;
            var folder = Path.Combine(rootFolder, fid!);
            if (!Util.isOdyssey) folder = Path.Combine(folder, "legacy");

            var filename = getFilename(entry);
            var filepath = Path.Combine(folder, filename);

            Directory.CreateDirectory(folder);
            var data = Data.Load<GuardianSiteData>(filepath);

            // create new if needed
            if (data == null)
            {
                // system name is missing on ApproachSettlement, so take the current system - assuming it's a match
                var cmdr = Game.activeGame?.cmdr!;
                var systemName = "";

                // might be bad idea?
                if (entry.SystemAddress == cmdr.currentSystemAddress && entry.BodyID == cmdr.currentBodyId)
                    systemName = cmdr.currentSystem;

                data = new GuardianSiteData()
                {
                    name = entry.Name,
                    nameLocalised = entry.Name_Localised,
                    commander = cmdr.commander,
                    type = SiteType.Unknown,
                    index = parseSettlementIdx(entry.Name),
                    filepath = filepath,
                    location = entry,
                    systemAddress = entry.SystemAddress,
                    systemName = systemName,
                    bodyId = entry.BodyID,
                    bodyName = entry.BodyName,
                    firstVisited = DateTimeOffset.UtcNow,
                    poiStatus = new Dictionary<string, SitePoiStatus>(),
                    legacy = !Util.isOdyssey,
                };

                // Structures have their type in the name
                if (entry.Name.StartsWith("$Ancient_"))
                    data.type = getStructureTypeFromName(entry.Name);

                data.Save();
                return data;
            }

            var save = false;

            // for existing files ... migrate/adjust data as needed
            if (data.poiStatus == null)
            {
                data.poiStatus = new Dictionary<string, SitePoiStatus>();
                save = true;
            }

            // Migrate old data
            if (data.poiStatus.Count == 0 && data.confirmedPOI != null && data.confirmedPOI.Count > 0)
            {
                // populate new dictionary, then remove the old one
                foreach (var poi in data.confirmedPOI)
                {
                    data.poiStatus.Add(
                        poi.Key,
                        poi.Value ? SitePoiStatus.present : SitePoiStatus.absent
                    );
                }
                data.confirmedPOI = null!;
                save = true;
            }

            if (Game.canonn.allRuins != null && data.type == SiteType.Unknown)
            {
                var grSites = Game.canonn.allRuins.Where(_ => _.bodyId == entry.BodyID && _.systemAddress == entry.SystemAddress);

                if (grSites.Any())
                {
                    var grSite = grSites.FirstOrDefault(_ => _.idx == data.index);
                    if (grSite != null)
                    {
                        Game.log($"Matched grSite: #GR{grSite?.siteID} on index (type: {grSite!.siteType})");
                    }
                    else
                    {
                        // TODO: match by lat/long?
                        Game.log($"TODO: match by lat/long?");
                    }

                    //var grSite = Game.canonn.ruinSummaries.FirstOrDefault(_ => _.bodyId == entry.BodyID && _.systemAddress == entry.SystemAddress);
                    if (grSite != null)
                    {
                        // can we use site type from it?
                        if (!string.IsNullOrEmpty(grSite.siteType))
                            if (Enum.TryParse<SiteType>(grSite.siteType, true, out data.type))
                                save = true;
                    }
                }
            }

            if (save)
            {
                data.Save();
            }
            return data;
        }

        public static SiteType getStructureTypeFromName(string settlementName)
        {
            var name = settlementName.Substring(0, settlementName.IndexOf(":#"));
            switch (name)
            {
                case "$Ancient_Tiny_001": return SiteType.Lacrosse;
                case "$Ancient_Tiny_002": return SiteType.Crossroads;
                case "$Ancient_Tiny_003": return SiteType.Fistbump;

                case "$Ancient_Small_001": return SiteType.Hammerbot;
                case "$Ancient_Small_002": return SiteType.Bear;
                case "$Ancient_Small_003": return SiteType.Bowl;
                // "$Ancient_Small_004" is unused
                case "$Ancient_Small_005": return SiteType.Turtle;

                case "$Ancient_Medium_001": return SiteType.Robolobster;
                case "$Ancient_Medium_002": return SiteType.Squid;
                case "$Ancient_Medium_003": return SiteType.Stickyhand;

                default:
                    throw new Exception($"Unexpected settlementName: {settlementName}");
            }
        }

        #region data members

        public string name;
        public string nameLocalised;
        public string commander;
        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;
        [JsonConverter(typeof(StringEnumConverter))]
        public SiteType type;
        public int index;
        public LatLong2 location;
        public long systemAddress;
        public string systemName;
        public int bodyId;
        public string bodyName;
        public int siteHeading = -1;
        public int relicTowerHeading = -1;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? notes;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool legacy = false;

        public Dictionary<string, bool> confirmedPOI = new Dictionary<string, bool>();
        public Dictionary<string, SitePoiStatus> poiStatus = new Dictionary<string, SitePoiStatus>();
        public Dictionary<string, int> relicHeadings = new Dictionary<string, int>();
        public Dictionary<string, ActiveObelisk> activeObelisks = new Dictionary<string, ActiveObelisk>();
        public HashSet<char> obeliskGroups = new HashSet<char>();

        #endregion

        [JsonIgnore]
        public bool isRuins { get => this.name != null && this.name.StartsWith("$Ancient:"); }

        [JsonIgnore]
        public string displayName { get => $"{this.bodyName}, ruins #{this.index} - {this.type}"; }

        public static int parseSettlementIdx(string name)
        {
            const string ruinsPrefix = "$Ancient:#index=";
            const string structurePrefix = "$Ancient_";

            // $Ancient:#index=2;
            if (name.StartsWith(ruinsPrefix))
            {
                return int.Parse(name.Substring(ruinsPrefix.Length, 1));
            }
            // $Ancient_Tiny_003:#index=1;
            if (name.StartsWith(structurePrefix))
            {
                return int.Parse(name.Substring(name.IndexOf("=") + 1, 1));
            }
            throw new Exception("Unkown site type");
        }

        public enum SiteType
        {
            // ruins ...
            Unknown,
            Alpha,
            Beta,
            Gamma,
            // structures ...
            Lacrosse,
            Crossroads,
            Fistbump,
            Hammerbot,
            Bear,
            Bowl,
            Turtle,
            Robolobster,
            Squid,
            Stickyhand,
        }

        public SitePoiStatus getPoiStatus(string name)
        {
            // use our own data first
            if (this.poiStatus.ContainsKey(name))
                return this.poiStatus[name];

            // otherwise check pub data
            if (this.pubData?.pp?.Contains(name) == true)
                return SitePoiStatus.present;
            if (this.pubData?.pa?.Contains(name) == true)
                return SitePoiStatus.absent;
            if (this.pubData?.pe?.Contains(name) == true)
                return SitePoiStatus.empty;

            return SitePoiStatus.unknown;
        }

        public ActiveObelisk? getActiveObelisk(string name, bool addIfMissing = false)
        {
            // use our own data first
            if (this.activeObelisks.ContainsKey(name))
                return this.activeObelisks[name];

            // otherwise check pub data
            var obelisk = this.pubData?.ao?.FirstOrDefault(_ => _.name == name);

            if (addIfMissing)
            {
                if (obelisk == null)
                {
                    Game.log($"Creating obelisk '{name}' for: '{this.bodyName}' ({this.bodyId}), idx: {this.index} ({this.type})");
                    obelisk = new ActiveObelisk();
                }

                this.activeObelisks[name] = obelisk;
            }

            return obelisk;
        }

        [JsonIgnore]
        private GuardianSitePub? pubData;

        public void loadPub()
        {
            if (this.pubData != null)
                return;

            var pubPath = Path.Combine(Program.dataFolder, "pub", "guardian", Path.GetFileName(this.filepath));
            if (!File.Exists(pubPath))
                return;

            Game.log($"Reading pubData for '{this.bodyName}' #{this.index} ({this.type}");
            var json = File.ReadAllText(pubPath);
            this.pubData = JsonConvert.DeserializeObject<GuardianSitePub>(json)!;

            if (this.type == SiteType.Unknown) this.type = pubData.t;
            if (this.siteHeading == -1) this.siteHeading = pubData.sh;
            if (this.relicTowerHeading == -1) this.relicTowerHeading = pubData.rh;
            if (this.obeliskGroups.Count == 0)
            {
                foreach (var c in pubData.og)
                    this.obeliskGroups.Add(c);
            }
        }

        public void publishSite()
        {
            var pubPath = Path.Combine(Program.dataFolder, "pub", "guardian", Path.GetFileName(this.filepath));
            if (!File.Exists(pubPath))
            {
                Game.log($"Creating pubData file for site '{this.bodyName}' #{this.index}");
                this.pubData = new GuardianSitePub()
                {
                    t = this.type,
                    idx = this.index,
                    //sa = this.systemAddress,
                    //bi = this.bodyId,
                    ll = this.location,
                    sh = this.siteHeading,
                    rh = this.relicTowerHeading,
                };
            }
            else
            {
                this.pubData = JsonConvert.DeserializeObject<GuardianSitePub>(File.ReadAllText(pubPath))!;
            }

            this.pubData.og = string.Join("", this.obeliskGroups);
            this.pubData.ao = new List<ActiveObelisk>(this.activeObelisks.Values);

            var poiPresent = new List<string>();
            var poiAbsent = new List<string>();
            var poiEmpty = new List<string>();
            foreach (var _ in this.poiStatus)
            {
                if (_.Value == SitePoiStatus.present) poiPresent.Add(_.Key);
                if (_.Value == SitePoiStatus.absent) poiAbsent.Add(_.Key);
                if (_.Value == SitePoiStatus.empty) poiEmpty.Add(_.Key);
            }
            this.pubData.pp = string.Join(',', poiPresent);
            this.pubData.pa = string.Join(',', poiAbsent);
            this.pubData.pe = string.Join(',', poiEmpty);


            var json = JsonConvert.SerializeObject(pubData, Formatting.Indented);
            File.WriteAllText(pubPath, json);
            Game.log($"Updated pubData file for site '{this.bodyName}' #{this.index}");
        }

        #region old migration 

        public static void migrateAlphaSites()
        {
            if (!Directory.Exists(rootFolder)) return;

            Game.log("Migrating alpha site data - inverting the headings");

            var folders = Directory.GetDirectories(rootFolder);
            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(Path.Combine(rootFolder, folder));
                foreach (var file in files)
                {
                    string filepath = Path.Combine(rootFolder, folder, file);
                    var data = Data.Load<GuardianSiteData>(filepath)!;

                    if (data.type == SiteType.Alpha)
                    {
                        data.siteHeading = new Angle(data.siteHeading - 180);
                        data.Save();
                    }
                }
            }

            Game.settings.migratedAlphaSiteHeading = true;
            Game.settings.Save();
        }

        //public static void migrateLiveLegacyLocations()
        //{
        //    if (!Directory.Exists(rootFolder)) return;

        //    Game.log("Migrating site location data - using liveLocation and legacyLocation");

        //    var folders = Directory.GetDirectories(rootFolder);
        //    foreach (var folder in folders)
        //    {
        //        var files = Directory.GetFiles(Path.Combine(rootFolder, folder));
        //        foreach (var file in files)
        //        {
        //            string filepath = Path.Combine(rootFolder, folder, file);
        //            var txt = File.ReadAllText(filepath);
        //            var newTxt = txt.Replace("\"location\":", "\"liveLocation\":");

        //            //File.WriteAllText(filepath, newTxt);
        //        }
        //    }

        //    //Game.settings.migratedLiveAndLegacyLocations = true;
        //    Game.settings.Save();
        //}

        #endregion

        public static List<GuardianSiteData> loadAllSites(bool onlyRuins)
        {
            var folder = Path.Combine(rootFolder, Game.settings.lastFid!);
            if (!Directory.Exists(folder))
                return new List<GuardianSiteData>();

            var files = onlyRuins
                ? Directory.GetFiles(folder, "*-ruins-*.json")
                : Directory.GetFiles(folder);

            Game.log($"Reading {files.Length} ruins files from disk");
            return files
                .Select(filename => Data.Load<GuardianSiteData>(filename))
                .ToList()!;
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return false;
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JToken>(reader);
                if (obj == null || !obj.HasValues)
                    return null;

                // read the simple fields
                var data = new GuardianSiteData()
                {
                    name = obj["name"]!.Value<string>()!,
                    nameLocalised = obj["nameLocalised"]!.Value<string>()!,
                    commander = obj["commander"]!.Value<string>()!,
                    firstVisited = obj["firstVisited"]!.Value<DateTime>()!,
                    lastVisited = obj["lastVisited"]!.Value<DateTime>()!,
                    type = Enum.Parse<SiteType>(obj["type"]!.Value<string>()!, true),
                    index = obj["index"]!.Value<int>()!,

                    location = obj["location"]!.ToObject<LatLong2>()!,
                    systemAddress = obj["systemAddress"]!.Value<long>()!,
                    systemName = obj["systemName"]!.Value<string>()!,
                    bodyId = obj["bodyId"]!.Value<int>()!,
                    bodyName = obj["bodyName"]!.Value<string>()!,
                    siteHeading = obj["siteHeading"]!.Value<int>()!,
                    relicTowerHeading = obj["relicTowerHeading"]!.Value<int>()!,

                    notes = obj["notes"]?.Value<string>(),
                    legacy = obj["legacy"]?.Value<bool>() ?? false,

                    // confirmedPOI - officially retired now :)
                    obeliskGroups = new HashSet<char>(),
                };

                // read obelisk groups - in either format
                var obeliskGroups = obj["obeliskGroups"];
                if (obeliskGroups != null)
                {
                    if (obeliskGroups.Type == JTokenType.String)
                    {
                        // read characters from a single string
                        foreach (var c in obeliskGroups.Value<string>()!)
                            data.obeliskGroups.Add(c);
                    }
                    else if (obeliskGroups.Type == JTokenType.Array)
                    {
                        // read old list format - alpha sorting 
                        foreach (var c in obeliskGroups.Values<char>()!)
                            data.obeliskGroups.Add(c);

                        var foo = obeliskGroups.Values<char>()!.ToList();
                        foo.Sort();
                        data.obeliskGroups = new HashSet<char>(foo);
                    }
                    else
                        throw new Exception($"Unexpected object type: {obeliskGroups.Type} for obeliskGroups");
                }

                // read active obelisks groups - in either format
                var activeObelisks = obj["activeObelisks"];
                if (activeObelisks?.HasValues == true)
                {
                    List<ActiveObelisk> obelisks;

                    if (activeObelisks.Type == JTokenType.Array)
                    {
                        // read from a list of encoded strings
                        obelisks = activeObelisks.ToObject<List<ActiveObelisk>>()!;
                    }
                    else if (activeObelisks.Type == JTokenType.Object)
                    {
                        // read old list format
                        obelisks = activeObelisks
                            .ToObject<Dictionary<string, ActiveObelisk0>>()!
                            .Select(foo => new ActiveObelisk()
                            {
                                name = foo.Key,
                                items = foo.Value.items,
                                msg = foo.Value.msg,
                                data = foo.Value.data,
                                scanned = foo.Value.scanned,
                            })
                            .ToList();
                    }
                    else
                        throw new Exception($"Unexpected object type: {activeObelisks.Type} for obeliskGroups");

                    foreach (var _ in obelisks)
                        data.activeObelisks.Add(_.name, _);
                }

                // read active obelisks groups - in either format
                var poiStatus = obj["poiStatus"];
                if (poiStatus?.HasValues == true)
                {
                    if (poiStatus.Type == JTokenType.Object)
                    {
                        // read old list format
                        data.poiStatus = poiStatus.ToObject<Dictionary<string, SitePoiStatus>>()!;
                    }
                    else
                        throw new Exception($"Unexpected object type: {poiStatus.Type} for obeliskGroups");
                }
                else
                {
                    // read new format using 3 distinct arrays per status
                    if (obj["poiPresent"] != null)
                        foreach (var _ in obj["poiPresent"]!.Value<string>()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries))
                            data.poiStatus[_] = SitePoiStatus.present;
                    if (obj["poiAbsent"] != null)
                        foreach (var _ in obj["poiAbsent"]!.Value<string>()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries))
                            data.poiStatus[_] = SitePoiStatus.absent;
                    if (obj["poiAbsent"] != null)
                        foreach (var _ in obj["poiEmpty"]!.Value<string>()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries))
                            data.poiStatus[_] = SitePoiStatus.empty;
                }

                //Game.log($"Reading: {data.bodyName} #{data.index}   ** ** ** ** {data.poiStatus.Count}");
                return data;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as GuardianSiteData;

                if (data == null)
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");

                var obj = new JObject
                {
                    { "name", data.name },
                    { "nameLocalised", data.nameLocalised },
                    { "commander", data.commander },
                    { "firstVisited", data.firstVisited },
                    { "lastVisited", data.lastVisited },
                    { "type", data.type.ToString() },
                    { "index", data.index },

                    { "location", JToken.FromObject(data.location) },
                    { "systemAddress", data.systemAddress },
                    { "systemName", data.systemName },
                    { "bodyId", data.bodyId },
                    { "bodyName", data.bodyName },
                    { "siteHeading", data.siteHeading },
                    { "relicTowerHeading", data.relicTowerHeading }
                };

                var obeliskGroups = string.Join("", data.obeliskGroups);
                obj.Add("obeliskGroups", obeliskGroups);

                var activeObelisks = JToken.FromObject(data.activeObelisks.Values);
                obj.Add("activeObelisks", activeObelisks);

                var poiPresent = new List<string>();
                var poiAbsent = new List<string>();
                var poiEmpty = new List<string>();
                foreach (var _ in data.poiStatus)
                {
                    if (_.Value == SitePoiStatus.present) poiPresent.Add(_.Key);
                    if (_.Value == SitePoiStatus.absent) poiAbsent.Add(_.Key);
                    if (_.Value == SitePoiStatus.empty) poiEmpty.Add(_.Key);
                }
                obj.Add("poiPresent", string.Join(',', poiPresent));
                obj.Add("poiAbsent", string.Join(',', poiAbsent));
                obj.Add("poiEmpty", string.Join(',', poiEmpty));

                //Game.log($"Writing: {data.bodyName} #{data.index}   ** ** ** ** {data.poiStatus.Count}");
                obj.WriteTo(writer);
            }
        }

    }

    // keeping for migration purposes only
    internal class ActiveObelisk0
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<ObeliskItem> items = new List<ObeliskItem>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<ObeliskData> data = new HashSet<ObeliskData>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string msg;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool scanned;
    }

    [JsonConverter(typeof(ActiveObelisk.JsonConverter))]
    internal class ActiveObelisk
    {
        [JsonIgnore]
        public string name;
        [JsonIgnore]
        public List<ObeliskItem> items = new List<ObeliskItem>();
        [JsonIgnore]
        public HashSet<ObeliskData> data = new HashSet<ObeliskData>();
        [JsonIgnore]
        public string msg;
        [JsonIgnore]
        public bool scanned;

        public override string ToString()
        {
            // "{name}-{item1},{item2}-{msg}-{data1},{data2},{data3}"
            var txt = new StringBuilder(this.name);
            if (this.scanned) txt.Append("!");

            txt.Append("-");
            txt.Append(string.Join(',', this.items.Select(_ => _.ToString().Substring(0, 2))));

            txt.Append("-");
            txt.Append(msg);

            txt.Append("-");
            txt.Append(string.Join(',', this.data.Select(_ => _.ToString()[0])));
            return txt.ToString();
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return false;
            }

            private static Dictionary<string, ObeliskItem> mapObeliskItems = new Dictionary<string, ObeliskItem>()
            {
                { "ca", ObeliskItem.casket },
                { "or", ObeliskItem.orb },
                { "re", ObeliskItem.relic },
                { "ta", ObeliskItem.tablet },
                { "to", ObeliskItem.totem },
                { "ur", ObeliskItem.urn },

                { "se", ObeliskItem.sensor },
                { "pr", ObeliskItem.probe },
                { "li", ObeliskItem.link },
                { "cy", ObeliskItem.cyclops },
                { "ba", ObeliskItem.basilisk },
                { "me", ObeliskItem.medusa },
            };

            private static Dictionary<string, ObeliskData> mapObeliskData = new Dictionary<string, ObeliskData>()
            {
                { "a", ObeliskData.alpha },
                { "b", ObeliskData.beta },
                { "e", ObeliskData.epsilon },
                { "d", ObeliskData.delta },
                { "g", ObeliskData.gamma },
            };

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt))
                    throw new Exception($"Unexpected value: {txt}");

                // "{name}{! if scanned}-{item1},{item2}-{msg}-{data1}-{data2},{data3}"
                // eg: "F07!-ca,to-H15-b,e"
                var parts = txt.Split('-');
                var scanned = parts[0].EndsWith('!');
                var name = scanned ? parts[0].Substring(0, parts[0].Length - 1) : parts[0];
                var items = parts[1]
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(_ => mapObeliskItems[_])
                    .ToList();
                var data = parts[3]
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(_ => mapObeliskData[_])
                    .ToHashSet();

                var obelisk = new ActiveObelisk()
                {
                    // drop the trailing '!' if scanned
                    name = name,
                    scanned = scanned,
                    items = items,
                    msg = parts[2],
                    data = data,
                };

                return obelisk;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var obelisk = value as ActiveObelisk;

                if (obelisk == null)
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // create a single string
                var txt = obelisk.ToString();
                writer.WriteValue(txt);
            }
        }

    }
}

