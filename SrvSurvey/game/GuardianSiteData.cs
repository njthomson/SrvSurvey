using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.plotters;
using SrvSurvey.units;
using System.Diagnostics;
using System.Text;

namespace SrvSurvey.game
{
    [JsonConverter(typeof(GuardianSiteData.JsonConverter))]
    internal class GuardianSiteData : Data
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SiteType
        {
            // ruins ...
            Unknown,
            Alpha,
            Beta,
            Gamma,
            // structures ...
            //    Tiny
            Lacrosse,
            Crossroads,
            Fistbump,
            //    Small
            Hammerbot,
            Bear,
            Bowl,
            Turtle,
            //    Medium
            Robolobster,
            Squid,
            Stickyhand,
        }

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

        public static Dictionary<SiteType, string> mapSiteTypeToSettlementName = new Dictionary<SiteType, string>()
        {
            { SiteType.Lacrosse, "$Ancient_Tiny_001" },
            { SiteType.Crossroads, "$Ancient_Tiny_002" },
            { SiteType.Fistbump, "$Ancient_Tiny_003" },
            { SiteType.Hammerbot, "$Ancient_Small_001" },
            { SiteType.Bear, "$Ancient_Small_002" },
            { SiteType.Bowl, "$Ancient_Small_003" },
            { SiteType.Turtle, "$Ancient_Small_005" },
            { SiteType.Robolobster, "$Ancient_Medium_001" },
            { SiteType.Squid, "$Ancient_Medium_002" },
            { SiteType.Stickyhand, "$Ancient_Medium_003" },
        };

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

        public override string ToString()
        {
            return this.displayName;
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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, Components>? components;
        public List<SitePOI>? rawPoi;

        #endregion

        [JsonIgnore]
        public bool isRuins { get => this.name != null && this.name.StartsWith("$Ancient:"); }

        [JsonIgnore]
        public string displayName
        {
            get
            {
                return this.isRuins
                    ? $"{this.bodyName}, ruins #{this.index} - {this.type}"
                    : $"{this.bodyName}, {this.type}";
            }
        }

        [JsonIgnore]
        public GuardianSitePub? pubData;

        [JsonIgnore]
        public ActiveObelisk? currentObelisk;

        public void setCurrentObelisk(string? name)
        {
            var changed = this.currentObelisk?.name != name;
            if (changed) Game.log($"setCurrentObelisk: {name}");

            if (name == null)
                this.currentObelisk = null;
            else
                this.currentObelisk = this.getActiveObelisk(name, false);

            if (changed)
            {
                FormRamTah.activeForm?.setCurrentObelisk(this.currentObelisk);
                Program.getPlotter<PlotGuardianStatus>()?.Invalidate();
            }
        }

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
            throw new Exception("Unknown site type");
        }

        public SitePoiStatus getPoiStatus(string name)
        {
            // use our own data first
            if (this.poiStatus.ContainsKey(name))
                return this.poiStatus[name];
            else if (name[0] == 'd')
            {
                var cmp = this.components?.GetValueOrDefault(name)?.items[0] ?? GComponent.unknown;
                return cmp == GComponent.unknown ? SitePoiStatus.unknown : SitePoiStatus.present;
            }

            // otherwise check pub data
            if (this.pubData?.poiStatus.ContainsKey(name) == true)
                return this.pubData.poiStatus[name];

            // TODO: components in pubData?

            // otherwise check rawPoi
            if (this.rawPoi?.Any(_ => _.name == name) == true)
                return SitePoiStatus.present;

            return SitePoiStatus.unknown;
        }

        public int? getRelicHeading(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (this.relicHeadings.ContainsKey(name))
                return this.relicHeadings[name];

            var match = this.rawPoi?.FirstOrDefault(_ => _.name == name);
            if (match != null)
                return (int)match.rot;

            if (this.pubData?.relicTowerHeadings.ContainsKey(name) == true)
                return this.pubData.relicTowerHeadings[name];

            return null;
        }

        public ActiveObelisk? getActiveObelisk(string? name, bool addIfMissing = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (addIfMissing) throw new Exception($"Bad obelisk name! (addIfMissing: {addIfMissing})");
                return null;
            }

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

        public void toggleObeliskScanned()
        {
            if (this.currentObelisk == null) return;
            this.setObeliskScanned(this.currentObelisk, !this.currentObelisk.scanned);
        }

        public void setObeliskScanned(ActiveObelisk obelisk, bool scanned)
        {
            obelisk.scanned = scanned;
            this.activeObelisks[obelisk.name] = obelisk;
            Game.log($"Setting obelisk '{obelisk.name}' as scanned: {obelisk.scanned}");
            this.Save();

            var game = Game.activeGame!;
            var haveItems = false;
            if (obelisk.items.Count == 2 && obelisk.items[0] == obelisk.items[1])
                haveItems = game.getInventoryItem(obelisk.items[0].ToString())?.Count > 1;
            else
                haveItems = obelisk.items.All(item => game.getInventoryItem(item.ToString())?.Count > 0);

            if (haveItems)
            {
                var cmdr = Game.activeGame?.cmdr;
                if (cmdr?.ramTahActive == true)
                {
                    var hashSet = this.isRuins ? cmdr.decodeTheRuins : cmdr.decodeTheLogs;
                    if (obelisk.scanned)
                        hashSet.Add(obelisk.msg);
                    else
                        hashSet.Remove(obelisk.msg);
                    this.ramTahRecalc();
                    Game.log($"Recording '{obelisk.msg}' Ram Tah as scanned: {obelisk.scanned}");
                    cmdr.Save();
                }
            }
            else
            {
                Game.log($"Insufficient items - NOT changing Ram Tah obelisk '{obelisk.name}' status");
            }

            Game.activeGame?.systemData?.prepSettlements();

            var plot = Program.getPlotter<PlotGuardians>();
            if (plot?.targetObelisk == obelisk.name)
                plot.setTargetObelisk(null);

            FormBeacons.activeForm?.beginPrepareAllRows();
            FormRamTah.activeForm?.updateChecks();
            Program.invalidateActivePlotters();
        }

        private Dictionary<string, HashSet<string>> _ramTahObelisks;

        public bool ramTahNeeded(string? msg)
        {
            if (msg == null || Game.activeGame == null || !Game.activeGame.cmdr.ramTahActive || this.ramTahObelisks == null)
                return false;
            else
                return this.ramTahObelisks.ContainsKey(msg);
        }

        public void ramTahRecalc()
        {
            this._ramTahObelisks = this.getObelisksForRamTah();

            FormRamTah.activeForm?.updateChecks();
            Program.invalidateActivePlotters();
        }

        [JsonIgnore]
        public Dictionary<string, HashSet<string>> ramTahObelisks
        {
            get
            {
                if (this._ramTahObelisks == null)
                    this._ramTahObelisks = this.getObelisksForRamTah();

                return this._ramTahObelisks;
            }
        }

        public Dictionary<string, HashSet<string>> getObelisksForRamTah()
        {
            // A map of log name to which obelisks contain it, eg: 'H12' => D03, H11
            var rslt = new Dictionary<string, HashSet<string>>();
            var cmdr = Game.activeGame?.cmdr;
            if (cmdr != null)
            {
                var allObelisks = this.activeObelisks.Values.ToList();
                if (this.pubData?.ao != null) allObelisks.AddRange(this.pubData.ao);

                foreach (var ob in allObelisks.OrderBy(_ => _.msg))
                {
                    var hashSet = this.isRuins ? cmdr.decodeTheRuins : cmdr.decodeTheLogs;

                    if (hashSet.Contains(ob.msg)) continue;
                    if (!rslt.ContainsKey(ob.msg)) rslt.Add(ob.msg, new HashSet<string>());
                    rslt[ob.msg].Add(ob.name);
                }
            }

            return rslt;
        }

        public void loadPub()
        {
            if (this.pubData != null)
                return;

            this.pubData = GuardianSitePub.Load(this.bodyName, this.index, this.isRuins);
            if (this.pubData == null)
            {
                Game.log($"Why no pubData for '{this.bodyName}' / '{this.name}'? (Newly discovered Ruins?)\r\nSee: {this.filepath}");
                if (Debugger.IsAttached) Debugger.Break();
                return;
            }

            if (this.type == SiteType.Unknown) this.type = pubData.t;
            if (this.siteHeading == -1 && pubData.sh != -1) this.siteHeading = pubData.sh;
            if (this.relicTowerHeading == -1 && pubData.rh != -1) this.relicTowerHeading = pubData.rh;

            if (this.obeliskGroups.Count == 0)
                foreach (var c in pubData.og)
                    this.obeliskGroups.Add(c);

            // no need to push poiStatus or activeObelisks states - that is handled within `this.getPoiStatus` / `this.getActiveObelisk`
        }

        public bool isSurveyComplete()
        {
            if (this.type == SiteType.Unknown || !GuardianSiteTemplate.sites.ContainsKey(this.type)) return false;

            // the site heading
            if (this.siteHeading == -1) return false;

            // ruins: singular relic tower heading is known
            if (this.isRuins && this.relicTowerHeading == -1) return false;

            // live lat / long
            if (this.location == null) return false;

            var status = this.getCompletionStatus();
            return status.isComplete;
        }

        public bool hasDiscoveredData()
        {
            // Yes - if ...

            // we have some raw POIs
            if (this.rawPoi?.Count > 0) return true;

            if (this.pubData == null) this.loadPub();

            // pubData is missing and we have: site.relic headings, location
            if (pubData!.sh == -1 && this.siteHeading != -1) return true;
            if (this.isRuins && pubData.rh == -1 && this.relicTowerHeading != -1) return true;
            if (pubData.ll == null && this.location != null) return true;

            // we have some POI status not in pubData or is different
            var diffPoiStatus = this.poiStatus.Where(_ => !pubData.poiStatus.ContainsKey(_.Key) || pubData.poiStatus[_.Key] != this.poiStatus[_.Key]).ToDictionary(_ => _.Key, _ => _.Value);
            var alt = pubData.poiStatus.Where(_ => diffPoiStatus.ContainsKey(_.Key)).ToDictionary(_ => _.Key, _ => _.Value);
            if (diffPoiStatus.Any()) return true;

            // we have some relic heading not in pubData (ignore number differences at this time)
            if (this.relicHeadings.Keys.Any(_ => !pubData.relicTowerHeadings.ContainsKey(_))) return true;

            // obelisk group names differ
            if (pubData.og != string.Join("", this.obeliskGroups)) return true;

            // ignore differences in activeObelisks

            return false;
        }

        public SurveyCompletionStatus getCompletionStatus()
        {
            this.loadPub();
            var status = new SurveyCompletionStatus();
            var template = GuardianSiteTemplate.sites[this.type];

            // process all POIs from template and raw
            var poiToProcess = this.rawPoi == null
                ? template.poiSurvey
                : template.poiSurvey.Union(this.rawPoi);

            foreach (var poi in poiToProcess)
            {
                if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk) continue;

                status.maxPoiConfirmed += 1;
                var poiStatus = this.getPoiStatus(poi.name);
                if (poiStatus != SitePoiStatus.unknown)
                    status.countPoiConfirmed += 1;

                if (poi.type == POIType.relic)
                {
                    if (poiStatus == SitePoiStatus.present)
                    {
                        status.countRelicsPresent += 1;

                        if (this.getRelicHeading(poi.name) == null)
                            status.countRelicsNeedingHeading += 1;
                        else if (!this.isRuins)
                            status.score += 1;
                    }
                }
                else if (poiStatus == SitePoiStatus.present && Util.isBasicPoi(poi.type))
                {
                    status.countPuddlesPresent += 1;
                }
            }

            status.score += status.countPoiConfirmed;
            if (this.siteHeading != -1) status.score += 1;

            // compute max score
            status.maxScore = poiToProcess.Count() + 1; // +1 for site heading
            status.maxPuddles = template.poi.Count(_ => Util.isBasicPoi(_.type));

            if (this.isRuins)
            {
                status.maxScore += 1; // one relic heading for site
                if (this.relicTowerHeading != -1) status.score += 1;
            }
            else
                status.maxScore += status.countRelicsPresent; // one relic heading per each tower

            status.progress = (int)(100.0 / status.maxScore * status.score);
            status.isComplete = status.progress == 100;
            return status;
        }

        #region old migration 

        public static void migrateAlphaSites()
        {
            if (!Directory.Exists(rootFolder)) return;

            Game.log("Migrating alpha site data - inverting the headings");

            var folders = Directory.GetDirectories(rootFolder);
            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(Path.Combine(rootFolder, folder), "*ruins*.json");
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

        /// <summary>
        /// Returns a list of all loaded Ruins and optionally Structures, but never Beacons.
        /// </summary>
        public static List<GuardianSiteData> loadAllSites(bool onlyRuins)
        {
            var folder = Path.Combine(rootFolder, Game.settings.lastFid!);
            if (!Directory.Exists(folder))
                return new List<GuardianSiteData>();

            var files = onlyRuins
                ? Directory.GetFiles(folder, "*-ruins-*.json")
                : Directory.GetFiles(folder)
                    .Where(_ => !_.Contains("beacon"))
                    .ToArray();

            Game.log($"Reading {files.Length} guardian sites files from disk");
            return files
                .Select(filename => Data.Load<GuardianSiteData>(filename))
                .ToList()!;
        }

        public static List<GuardianSiteData> loadAllSitesFromAllUsers(bool onlySubmittedData)
        {
            if (!Directory.Exists(GuardianSiteData.rootFolder)) return new List<GuardianSiteData>();

            var files = Directory.GetFiles(GuardianSiteData.rootFolder, "*.json", SearchOption.AllDirectories)
                    .Where(_ => !_.Contains("beacon") && !_.Contains("legacy"))
                    .Where(_ => !onlySubmittedData || _.Contains("guardian\\surveys-")) // skip non-submitted data for now
                    .ToArray();

            Game.log($"Reading {files.Length} guardian sites files from disk");
            return files
                .Select(filename => Data.Load<GuardianSiteData>(filename)!)
                .Where(_ => _.legacy == false)
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
                if (obj == null || !obj.HasValues) return null;

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

                    confirmedPOI = obj["confirmedPOI"]?.ToObject<Dictionary<string, bool>>()!,
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

                var relicHeadings = obj["relicHeadings"];
                if (relicHeadings != null)
                    data.relicHeadings = relicHeadings.ToObject<Dictionary<string, int>>()!;

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
                                msg = foo.Value.msg,
                                scanned = foo.Value.scanned,
                            })
                            .ToList();
                    }
                    else
                        throw new Exception($"Unexpected object type: {activeObelisks.Type} for obeliskGroups");

                    foreach (var _ in obelisks)
                        if (!string.IsNullOrEmpty(_.name))
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
                    if (obj["poiPresent"]?.Value<string>() != null)
                        foreach (var _ in obj["poiPresent"]!.Value<string>()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries))
                            data.poiStatus[_] = SitePoiStatus.present;
                    if (obj["poiAbsent"]?.Value<string>() != null)
                        foreach (var _ in obj["poiAbsent"]!.Value<string>()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries))
                            data.poiStatus[_] = SitePoiStatus.absent;
                    if (obj["poiEmpty"]?.Value<string>() != null)
                        foreach (var _ in obj["poiEmpty"]!.Value<string>()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries))
                            data.poiStatus[_] = SitePoiStatus.empty;
                }

                var rawPoi = obj["rawPoi"];
                if (rawPoi != null)
                    data.rawPoi = rawPoi.ToObject<List<SitePOI>>()!;

                if (Game.settings.guardianComponentMaterials_TEST || Debugger.IsAttached)
                {
                    var rawComponents = obj["components"] as JArray;
                    if (rawComponents != null && rawComponents.Count > 0)
                    {
                        var components = rawComponents.ToObject<List<Components>>();
                        if (components?.Count > 0)
                        {
                            data.components ??= new();
                            foreach (var cmp in components)
                                data.components.Add(cmp.name, cmp);
                        }
                    }
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
                    { "relicTowerHeading", data.relicTowerHeading },
                    { "notes", data.notes },
                };

                var obeliskGroups = string.Join("", data.obeliskGroups);
                obj.Add("obeliskGroups", obeliskGroups);

                var activeObelisks = JToken.FromObject(data.activeObelisks.Values);
                obj.Add("activeObelisks", activeObelisks);

                var relicHeadings = JToken.FromObject(data.relicHeadings);
                obj.Add("relicHeadings", relicHeadings);

                var poiPresent = new List<string>();
                var poiAbsent = new List<string>();
                var poiEmpty = new List<string>();
                var poiToRemove = new HashSet<string>();
                foreach (var _ in data.poiStatus)
                {
                    // ignore any obelisks
                    var poiType = GuardianSiteTemplate.sites[data.type].poi.FirstOrDefault(poi => poi.name == _.Key)?.type;
                    if (poiType == null) { Game.log($"Unknown POI? '{_.Key}' at '{data.displayName}'"); poiToRemove.Add(_.Key); continue; }
                    else if (poiType == POIType.obelisk || poiType == POIType.brokeObelisk) { Game.log($"Ignoring obelisk POI? '{_.Key}' at '{data.displayName}'"); poiToRemove.Add(_.Key); continue; }
                    if (_.Value == SitePoiStatus.present) poiPresent.Add(_.Key);
                    if (_.Value == SitePoiStatus.absent) poiAbsent.Add(_.Key);
                    if (_.Value == SitePoiStatus.empty) poiEmpty.Add(_.Key);
                }
                foreach (var _ in poiToRemove)
                    data.poiStatus.Remove(_);
                poiPresent.Sort();
                poiAbsent.Sort();
                poiEmpty.Sort();
                obj.Add("poiPresent", string.Join(',', poiPresent));
                obj.Add("poiAbsent", string.Join(',', poiAbsent));
                obj.Add("poiEmpty", string.Join(',', poiEmpty));

                if (data.rawPoi != null)
                {
                    var rawPoi = JToken.FromObject(data.rawPoi);
                    obj.Add("rawPoi", rawPoi);
                }

                if (data.components?.Count > 0)
                {
                    var rawComponents = JToken.FromObject(data.components.Values);
                    obj.Add("components", rawComponents);
                }

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
        public List<ObeliskItem> items { get => mapMsgItems.GetValueOrDefault(this.msg) ?? new List<ObeliskItem>(); }

        [JsonIgnore]
        public string msg;
        [JsonIgnore]
        public bool scanned;

        [JsonIgnore]
        public string msgDisplay { get => string.IsNullOrEmpty(this.msg) ? "?" : (Util.getLogNameFromChar(this.msg[0]) + " #" + this.msg.Substring(1)); }

        public static string? orderedRamTahLogs(IEnumerable<ActiveObelisk>? obelisks)
        {
            if (obelisks == null) return null;

            var ordered = obelisks
                .Select(_ => _.msg)
                .ToHashSet()
                .OrderBy(_ => _[0] + _.Substring(1).PadLeft(2, '0'));

            return string.Join(", ", ordered);
        }

        public override string ToString()
        {
            // "{name}-{item1},{item2}-{msg}"
            var txt = new StringBuilder(this.name);
            if (this.scanned) txt.Append("!");

            txt.Append("-");
            if (this.items != null)
                txt.Append(string.Join(',', this.items.Select(_ => _.ToString().Substring(0, 2))));

            txt.Append("-");
            txt.Append(msg);

            return txt.ToString();
        }

        public override bool Equals(object? obj)
        {
            return this.ToString() == obj?.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return false;
            }

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

                var obelisk = new ActiveObelisk()
                {
                    // drop the trailing '!' if scanned
                    name = name,
                    scanned = scanned,
                    msg = parts[2],
                };

                return obelisk;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var obelisk = value as ActiveObelisk;

                if (obelisk == null)
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // create a single string (we need that trailing dash or older builds will fail to parse it)
                var txt = obelisk.ToString() + "-";
                writer.WriteValue(txt);
            }
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

        private static Dictionary<string, List<ObeliskItem>> mapMsgItems = prepLogItems();

        private static Dictionary<string, List<ObeliskItem>> prepLogItems()
        {
            var source = new List<string>()
            {
                // Alpha - Biological
                { "B1-ur-re" },
                { "B2-ur-ur" },
                { "B3-ur-ca" },
                { "B4-ur-ta" },
                { "B5-ur-or" },
                { "B6-ur-to" },
                // Beta - Biological
                { "B7-ur-re" },
                { "B8-ur-ur" },
                { "B9-ur-ca" },
                { "B10-ur-ta" },
                { "B11-ur-or" },
                { "B12-ur-to" },
                // Gamma - Biological
                { "B13-ur" },
                { "B14-ur-ur" },
                { "B15-ur-ca" },
                { "B16-ur-ta" },
                { "B17-ur-or" },
                { "B18-ur-to" },
                { "B19-ur-re" },
                // Alpha - Cultural
                { "C1-to-re" },
                { "C2-to-to" },
                { "C3-to-ca" },
                { "C4-to-ur" },
                { "C5-to-ta" },
                { "C6-to-or" },
                // Beta - Cultural
                { "C7-to" },
                { "C8-to-to" },
                { "C9-to-ca" },
                { "C10-to-ur" },
                { "C11-to-ta" },
                { "C12-to-or" },
                { "C13-to-re" },
                // Gamma - Cultural
                { "C14-to" },
                { "C15-to-to" },
                { "C16-to-ca" },
                { "C17-to-ur" },
                { "C18-to-ta" },
                { "C19-to-or" },
                { "C20-to-re" },
                // Alpha - Historical
                { "H1-ca" },
                { "H2-ca-ca" },
                { "H3-ca-ur" },
                { "H4-ca-ta" },
                { "H5-ca-or" },
                { "H6-ca-to" },
                { "H7-ca-re" },
                // Beta - Historical
                { "H8-ca" },
                { "H9-ca-ca" },
                { "H10-ca-re" },
                { "H11-ca-ta" },
                { "H12-ca-or" },
                { "H13-ca-to" },
                { "H14-ca-re" },
                { "H15-ca-to" },
                { "H16-ca-re" },
                // Gamma - Historical
                { "H17-ca-to" },
                { "H18-ca-ca" },
                { "H19-ca-ur" },
                { "H20-ca-ta" },
                { "H21-ca-or" },  
                // Alpha - Language
                { "L1-ta" },
                { "L2-ta-ta" },
                { "L3-ta-ca" },
                { "L4-ta-ur" },
                { "L5-ta-or" },
                { "L6-ta-ta" },
                { "L7-ta-re" },
                // Beta - Language
                { "L8-ta" },
                { "L9-ta-ta" },
                { "L10-ta-ca" },
                { "L11-ta-ur" },
                { "L12-ta-or" },
                { "L13-ta-to" },
                { "L14-ta-re" },
                // Gamma - Language
                { "L15-ta" },
                { "L16-ta-ta" },
                { "L17-ta-ca" },
                { "L18-ta-ur" },
                { "L19-ta-or" },
                { "L20-ta-to" },
                { "L21-ta-re" },  
                // Alpha - Language
                { "T1-or" },
                { "T2-or-or" },
                { "T3-or-ca" },
                { "T4-or-ur" },
                { "T5-or-ta" },
                { "T6-or-to" },
                // Beta - Language
                { "T7-or" },
                { "T8-or-or" },
                { "T9-or-ca" },
                { "T10-or-ur" },
                { "T11-or-ta" },
                { "T12-or-re" },
                { "T13-or-re" },
                // Gamma - Language
                { "T14-or" },
                { "T15-or-or" },
                { "T16-or-ca" },
                { "T17-or-ur" },
                { "T18-or-ta" },
                { "T19-or-to" },
                { "T20-or-re" },
                // Thargoid logs
                { "#1-se-cy" },
                { "#2-se-ba" },
                { "#3-se-li" },
                { "#4-se-pr" },
                { "#5-se-me" },
                // Civil war logs
                { "#6-ca-or" },
                { "#7-ca-ta" },
                { "#8-ca-to" },
                { "#9-ca-re" },
                { "#10-ca-ur" },
                // Technology logs
                { "#11-or-re" },
                { "#12-or-or" },
                { "#13-or-ca" },
                { "#14-re-to" },
                { "#15-re-ca" },
                { "#16-re-ur" },
                { "#17-re-ta" },
                { "#18-or-to" },
                { "#19-or-ca" },
                { "#20-re-or" },
                { "#21-or-ur" },
                { "#22-or-ta" },
                { "#23-or-re" },
                // Language logs
                { "#24-ta-to" },
                // Body protectorate logs
                { "#25-ur-or" },
                { "#26-ur-to" },
                { "#27-ur-ta" },
                { "#28-ur-ca" },
            };

            var map = new Dictionary<string, List<ObeliskItem>>();
            foreach (var txt in source)
            {
                var parts = txt.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var logMsg = parts[0];
                var items = new List<ObeliskItem>();

                items.Add(mapObeliskItems.GetValueOrDefault(parts[1]));
                if (parts.Length > 2)
                    items.Add(mapObeliskItems.GetValueOrDefault(parts[2]));

                map.Add(logMsg, items);
            }

            return map;
        }
    }

    internal class SystemSettlementSummary
    {
        public SystemBody body;
        public string name;
        public string displayText;
        public string type;
        public string status;
        public string extra;
        public string bluePrint;

        public static SystemSettlementSummary forRuins(SystemData systemData, SystemBody body, int idx)
        {
            if (Game.canonn.allRuins == null) throw new Exception("Why is allRuins not populated?");
            var site = Game.canonn.allRuins.FirstOrDefault(_ => _.systemAddress == systemData.address && _.bodyId == body.id && _.idx == idx);
            if (site == null) throw new Exception($"New Ruins #{idx} found on {body}??");

            var summary = new SystemSettlementSummary()
            {
                body = body,
                name = $"$Ancient:#index={idx};",
                displayText = $"{body.name.Replace(systemData.name, "")}: Ruins #{idx} - {site.siteType}",
            };

            // show survey status if not complete
            if (!site.surveyComplete)
            {
                var siteData = GuardianSiteData.Load(body.name, idx, true);
                if (site.surveyProgress == 0 && siteData == null)
                    summary.status = "Survey: not started";
                else if (siteData?.isSurveyComplete() == false)
                    summary.status = "Survey: incomplete";
            }

            // add required Ram Tah logs, if relevant
            var cmdr = Game.activeGame?.cmdr;
            if (cmdr?.decodeTheRuinsMissionActive == TahMissionStatus.Active)
            {
                var pubData = GuardianSitePub.Load(body.name, idx, site.siteType);
                if (pubData == null) throw new Exception("Why?");

                var logsNeeded = pubData.ao
                    .Where(_ => !cmdr.decodeTheRuins.Contains(_.msg))
                    .Select(_ => _.msg)
                    .OrderBy(_ => _)
                    .ToHashSet();

                if (logsNeeded.Count > 0)
                    summary.extra = "Ram Tah: " + string.Join(" ", logsNeeded);
            }

            return summary;
        }

        public static SystemSettlementSummary forStructure(SystemData systemData, SystemBody body)
        {
            if (Game.canonn.allStructures == null) throw new Exception("Why is allStructures not populated?");
            var site = Game.canonn.allStructures.FirstOrDefault(_ => _.systemAddress == systemData.address && _.bodyId == body.id);
            if (site == null) throw new Exception($"New structure found on {body}??");

            var siteType = Enum.Parse<GuardianSiteData.SiteType>(site.siteType);
            var summary = new SystemSettlementSummary()
            {
                body = body,
                name = GuardianSiteData.mapSiteTypeToSettlementName[siteType] + ":#index=1;",
                displayText = $"{body.name.Replace(systemData.name, "")}: {siteType}",
            };

            // add blue print type
            if (siteType == GuardianSiteData.SiteType.Robolobster || siteType == GuardianSiteData.SiteType.Squid || siteType == GuardianSiteData.SiteType.Stickyhand)
                summary.bluePrint = "Blue print: fighter";
            else if (siteType == GuardianSiteData.SiteType.Turtle)
                summary.bluePrint = "Blue print: module";
            else if (siteType == GuardianSiteData.SiteType.Bear || siteType == GuardianSiteData.SiteType.Hammerbot || siteType == GuardianSiteData.SiteType.Bowl)
                summary.bluePrint = "Blue print: weapon";

            // show survey status if not complete
            if (!site.surveyComplete)
            {
                var siteData = GuardianSiteData.Load(body.name, summary.name);
                if (site.surveyProgress == 0 && siteData == null)
                    summary.status = "Survey: not started";
                else if (siteData?.isSurveyComplete() == false)
                    summary.status = "Survey: incomplete";
            }

            // add required Ram Tah logs, if relevant
            var cmdr = Game.activeGame?.cmdr;
            if (cmdr?.decodeTheLogsMissionActive == TahMissionStatus.Active)
            {
                var pubData = GuardianSitePub.Load(body.name, 1, site.siteType);
                if (pubData == null) throw new Exception("Why?");

                var logsNeeded = pubData.ao
                    .Where(_ => !cmdr.decodeTheLogs.Contains(_.msg))
                    .Select(_ => _.msg)
                    .OrderBy(_ => _)
                    .ToHashSet();

                if (logsNeeded.Count > 0)
                    summary.extra = "Ram Tah: " + string.Join(" ", logsNeeded);
            }

            return summary;
        }
    }

    internal class SurveyCompletionStatus
    {
        public int score;
        public int maxScore;
        public int countPoiConfirmed;
        public int maxPoiConfirmed;
        public int countRelicsPresent;
        public int countPuddlesPresent;
        public int maxPuddles;
        public int countRelicsNeedingHeading;
        public int progress;
        public bool isComplete;
        public string percent { get => this.progress.ToString("0") + "%"; }
    }

    [JsonConverter(typeof(Components.JsonConverter))]
    internal class Components
    {
        public string name;
        public GComponent[] items = new GComponent[3];

        public override string ToString()
        {
            if (name[0] == 'c')
                return $"{name}," + string.Join(',', items);
            else
                return $"{name},{items[0]}";
        }

        public override bool Equals(object? obj)
        {
            return this.ToString() == obj?.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return false;
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt))
                    throw new Exception($"Unexpected value: {txt}");

                var parts = txt.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var tower = new Components()
                {
                    name = parts[0],
                    items = parts[1..].Select(t => Enum.Parse<GComponent>(t)).ToArray(),
                };

                return tower;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var tower = value as Components;

                if (tower == null)
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // create a single string
                var txt = tower.ToString();
                writer.WriteValue(txt);
            }
        }

        public static string to(GComponent cmp)
        {
            switch (cmp)
            {
                case GComponent.unknown: return "?";
                case GComponent.cell: return "Power Cell";
                case GComponent.conduit: return "Power Conduit";
                case GComponent.tech: return "Technology Component";
                default: throw new Exception($"Unexpected component type: {cmp}");
            }
        }
    }

    internal enum GComponent
    {
        unknown,
        /// <summary> Power Cell </summary>
        cell,
        /// <summary> Power Conduit </summary>
        conduit,
        /// <summary> Technology Component </summary>
        tech,
    }
}

