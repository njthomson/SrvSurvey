using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.canonn;
using SrvSurvey.net;
using SrvSurvey.units;
using static SrvSurvey.game.GuardianSiteData;

namespace SrvSurvey.game
{
    /// <summary>
    /// Which vehicle are we currently in?
    /// </summary>
    enum ActiveVehicle
    {
        Unknown,
        MainShip,
        Fighter,
        SRV,
        Foot,
        Taxi,

        Docked, // meaning - not in any of the above
    }

    /// <summary>
    /// A union of various enums and states that should be mutually exclusive.
    /// </summary>
    enum GameMode
    {
        // These are an exact match to GuiFocus from Status
        NoFocus = 0, // meaning ... playing the game
        InternalPanel, //(right hand side)
        ExternalPanel, // (left hand side)
        CommsPanel, // (top)
        RolePanel, // (bottom)
        StationServices,
        GalaxyMap,
        SystemMap,
        Orrery,
        FSS,
        SAA,
        Codex,

        // These are extra

        // game is not running
        Offline,

        // Commander is in a vehicle
        InFighter,
        InSrv,
        InTaxi,
        /// <summary> Not in a station </summary>
        OnFoot,
        /// <summary> Walking around in a station, social spaces, landing pads - within some enclosed space </summary>
        OnFootInStation,

        // Commander is in main ship, that is ...
        SuperCruising,
        GlideMode,     // gliding to a planet surface
        Flying,   // in deep space
                  // TODO: FlyingSurface, // either at planet or deep space

        // in the MainShip but not flying
        Landed,

        // At a landing pad
        Docked,

        MainMenu,
        FSDJumping,
        CarrierMgmt,

        Unknown,
    }

    delegate void GameModeChanged(GameMode newMode, bool force);
    delegate void StatusFileChanged(bool blink);

    public interface ILocation
    {
        double Longitude { get; set; }
        double Latitude { get; set; }
    }

    interface ISystemAddress
    {
        public long SystemAddress { get; set; }
    }

    interface ISystemDataStarter
    {
        DateTime timestamp { get; set; }
        string @event { get; set; }
        string StarSystem { get; set; }
        long SystemAddress { get; set; }
        double[] StarPos { get; set; }
        string Body { get; set; }
        int BodyID { get; set; }
        FSDJumpBodyType BodyType { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    enum ParentBodyType
    {
        /// <summary>
        /// BaryCenter
        /// </summary>
        Null,
        Star,
        Planet,
        Ring,
        Asteroid, // Sadly many BaryCenters got classified as this
    }

    internal class BodyParent
    {
        public ParentBodyType type;
        public int id;

        public BodyParent()
        {
        }

        public BodyParent(Dictionary<ParentBodyType, int> parent)
        {
            this.type = parent.First().Key;
            this.id = parent.First().Value;
        }

        public override string ToString()
        {
            return $"{this.type}: {this.id}";
        }
    }

    internal class BodyParents : List<BodyParent>
    {
        public static implicit operator BodyParents(List<Dictionary<ParentBodyType, int>> entry)
        {
            var parents = new BodyParents();

            if (entry != null)
                foreach (var p in entry)
                    parents.Add(new BodyParent(p));

            return parents;
        }

        // TODO: add a custom json serializer so it looks the same way as in journal entries?
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum SystemBodyType
    {
        Unknown,
        Star,
        Giant,
        SolidBody,
        LandableBody,
        Asteroid,
        PlanetaryRing,
        Barycentre,
    }

    public enum SuitType
    {
        flightSuite,
        artiemis,
        maverick,
        dominator,
    }

    internal class BioMatch
    {
        public BioMatch(BioGenus genus, BioSpecies species, BioVariant variant)
        {
            this.genus = genus;
            this.species = species;
            this.variant = variant;
        }

        public BioGenus genus;
        public BioSpecies species;
        public BioVariant variant;

        public long entryId { get => long.Parse(species.entryIdPrefix + variant?.entryIdSuffix); }
    }

    internal class BioGenus
    {
        public string name;   // $Codex_Ent_Tussocks_Genus_Name;
        public string englishName; // Tussock
        public int dist; // how far before next scan
        public bool odyssey; // is new for Odyssey?

        public List<BioSpecies> species;

        public override string ToString()
        {
            return $"'{this.name}' ({this.englishName}";
        }

        public string shortName
        {
            get => this.englishName.Substring(0, 3).ToLowerInvariant(); // tus
        }

        private void calcMinMax()
        {
            _minValue = long.MaxValue;


            foreach (var foo in this.species)
            {
                _minValue = (long)Math.Min(_minValue, foo.reward);
                _maxValue = (long)Math.Max(_maxValue, foo.reward);
            }
        }

        private long _minValue;
        private long _maxValue;

        public long minReward
        {
            get
            {
                if (_minValue == 0)
                    calcMinMax();
                return _minValue;
            }
        }

        public long maxReward
        {
            get
            {
                if (_maxValue == 0)
                    calcMinMax();
                return _maxValue;
            }
        }

        public BioSpecies? matchSpecies(string speciesName)
        {
            foreach (var speciesRef in this.species)
                if (speciesRef.name == speciesName || speciesRef.englishName.Equals(speciesName, StringComparison.Ordinal))
                    return speciesRef;

            return null;
        }
    }

    internal class BioSpecies
    {
        [JsonIgnore]
        public BioGenus genus;

        public string name;   // $Codex_Ent_Stratum_07_Name;
        public string englishName; // Stratum Tectonicas
        public long reward; // 19010800
        public string entryIdPrefix; // 24207

        public List<BioVariant> variants;

        public override string ToString()
        {
            return $"'{this.name}' ({this.englishName}";
        }

        /// <summary>
        /// The englishName value without the genus part
        /// </summary>
        public string displayName
        {
            get
            {
                if (this.englishName != genus.englishName)
                    return this.englishName.Replace(genus.englishName, "").Trim();
                else
                    return this.englishName;
            }
        }
    }

    internal class BioVariant
    {
        [JsonIgnore]
        public BioSpecies species;

        public string name;   // $Codex_Ent_Stratum_07_M_Name;
        public string englishName; // Stratum Tectonicas - Green
        public string entryIdSuffix; // 03

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string imageUrl;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string imageCmdr;

        [JsonIgnore]
        public string entryId { get => this.species.entryIdPrefix + this.entryIdSuffix; }

        [JsonIgnore]
        public long reward { get => this.species.reward; }

        [JsonIgnore]
        public string localImgName
        {
            get
            {
                if (!this.species.genus.odyssey)
                    return $"{this.species.genus.englishName}-{this.species.displayName}".Replace(' ', '-');

                var parts = this.englishName.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4)
                    return $"{parts[0]}-{parts[1]}-{parts[3]}".ToLowerInvariant();
                else
                    return $"{parts.Last()}-{parts.First()}".ToLowerInvariant();
            }
        }

        [JsonIgnore]
        public string colorName
        {
            get
            {
                var parts = this.englishName.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                return parts.Last();
            }
        }

        public override string ToString()
        {
            return $"'{this.name}' ({this.englishName}";
        }
    }

    internal class GuardianSitePub
    {
        public static GuardianSitePub? Load(string bodyName, int index, string siteType)
        {
            return Load(bodyName, index, Enum.Parse<GuardianSiteData.SiteType>(siteType, true));
        }

        public static GuardianSitePub? Load(string bodyName, int index, SiteType siteType)
        {
            var isRuins = siteType == SiteType.Alpha || siteType == SiteType.Beta || siteType == SiteType.Gamma;
            var filename = GuardianSiteData.getFilename(bodyName, index, isRuins);

            var pubPath = Path.Combine(Git.pubGuardianFolder, filename);
            if (!File.Exists(pubPath))
            {
                Game.log($"No pubData for: {pubPath}");
                return null;
            }

            if (!Data.suppressLoadingMsg)
                Game.log($"Reading pubData for '{bodyName}' #{index} ({siteType})");
            var json = File.ReadAllText(pubPath);

            var pubData = JsonConvert.DeserializeObject<GuardianSitePub>(json);
            return pubData;
        }

        public static List<GuardianSitePub> Find(string systemName)
        {
            var sites = new List<GuardianSitePub>();

            if (Directory.Exists(Git.pubGuardianFolder))
            {
                var files = Directory.GetFiles(Git.pubGuardianFolder, $"{systemName}*.json");
                foreach (var filepath in files)
                {
                    var filename = Path.GetFileName(filepath);
                    if (!Data.suppressLoadingMsg)
                        Game.log($"Reading pubData for '{filename}'");

                    var json = File.ReadAllText(filepath);
                    var pubData = JsonConvert.DeserializeObject<GuardianSitePub>(json)!;
                    var idx = filename.IndexOf("-ruins");
                    if (idx < 0) idx = filename.IndexOf("-structure");
                    pubData.bodyName = filename.Substring(0, idx);
                    sites.Add(pubData);
                }
            }

            return sites;
        }

        [JsonIgnore]
        public string systemName;
        [JsonIgnore]
        public string bodyName;

        /// <summary> Canonn SiteId </summary>
        public string sid;

        /// <summary> Index of ruins </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int idx;

        /// <summary> Site heading </summary>
        public int sh = -1;

        /// <summary> Site type, eg: Alpha, Beta, Bear, Robolobster, etc </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SiteType t;

        /// <summary> Relic tower heading, if applicable </summary>
        public int rh = -1;

        /// <summary> Lat/Long location </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public LatLong2 ll;

        /// <summary> POI status : absent </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string pa;

        /// <summary> POI status : present </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string pp;

        /// <summary> POI status : present </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string pe;

        /// <summary>
        /// Obelisk groups - a bunch of letters, to be split into a HashSet
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string og;

        /// <summary>
        /// Active Obelisks - a string of this encoding:
        /// 
        /// "{name}{! if scanned}-{item1},{item2}-{msg}-{data1}-{data2},{data3}"
        /// 
        /// Eg:
        /// "F07!-ca,to-H15-b,e"
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<ActiveObelisk> ao;

        /// <summary>
        /// Relic tower headings - a bunch of "t11:123" to be split into a Dictionary
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string rth;

        [JsonIgnore]
        public bool isRuins { get => this.t == SiteType.Alpha || this.t == SiteType.Beta || this.t == SiteType.Gamma; }

        [JsonIgnore]
        public Dictionary<string, int> relicTowerHeadings
        {
            get
            {
                if (this._relicTowerHeadings == null)
                {
                    this._relicTowerHeadings = new Dictionary<string, int>();

                    if (!string.IsNullOrEmpty(this.rth))
                    {
                        var towers = this.rth.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        foreach (var tower in towers)
                        {
                            var parts = tower.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length != 2) throw new Exception("Corrupt rth?");

                            this._relicTowerHeadings[parts[0]] = int.Parse(parts[1]);
                        }
                    }

                }

                return this._relicTowerHeadings;
            }
        }
        private Dictionary<string, int> _relicTowerHeadings;

        [JsonIgnore]
        public Dictionary<string, SitePoiStatus> poiStatus
        {
            get
            {
                if (this._poiStatus == null)
                {
                    this._poiStatus = new Dictionary<string, SitePoiStatus>();

                    // status of all POI
                    var allPoi = new List<string>();
                    if (this.pp != null) this.pp.Split(",").ToList().ForEach(_ => this._poiStatus[_] = SitePoiStatus.present);
                    if (this.pa != null) this.pa.Split(",").ToList().ForEach(_ => this._poiStatus[_] = SitePoiStatus.absent);
                    if (this.pe != null) this.pe.Split(",").ToList().ForEach(_ => this._poiStatus[_] = SitePoiStatus.empty);
                }
                return this._poiStatus;
            }
        }
        private Dictionary<string, SitePoiStatus> _poiStatus;

        public bool isSurveyComplete()
        {
            if (this.t == SiteType.Unknown || !SiteTemplate.sites.ContainsKey(this.t)) return false;
            var template = SiteTemplate.sites[this.t];

            // the site heading
            if (this.sh == -1) return false;

            // ruins: singular relic tower heading is known
            if (this.isRuins && this.rh == -1) return false;

            // live lat / long
            if (this.ll == null) return false;

            // status for all POI
            if (this.poiStatus.Count < template.poiSurvey.Count) return false;

            // structures: all present relic towers have a heading
            if (!this.isRuins && this.relicTowerHeadings.Count < this.poiStatus?.Keys.Count(_ => template.relicTowerNames.Contains(_))) return false;

            return true;
        }

        public SurveyCompletionStatus getCompletionStatus()
        {
            var status = new SurveyCompletionStatus();
            var template = SiteTemplate.sites[this.t];

            foreach (var poi in template.poiSurvey)
            {
                if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk) continue;
                if (!this.poiStatus.ContainsKey(poi.name)) continue;

                status.maxPoiConfirmed += 1;
                var poiStatus = this.poiStatus[poi.name];
                if (poiStatus != SitePoiStatus.unknown)
                    status.countPoiConfirmed += 1;

                if (poi.type == POIType.relic)
                {
                    if (poiStatus == SitePoiStatus.present)
                    {
                        status.countRelicsPresent += 1;

                        if (!this.relicTowerHeadings.ContainsKey(poi.name))
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
            if (this.sh != -1) status.score += 1;

            // compute max score
            status.maxScore = template.poiSurvey.Count() + 1; // +1 for site heading
            status.maxPuddles = template.poi.Count(_ => Util.isBasicPoi(_.type));

            if (this.isRuins)
            {
                status.maxScore += 1; // one relic heading for site
                if (this.rh != -1) status.score += 1;
            }
            else
                status.maxScore += status.countRelicsPresent; // one relic heading per each tower

            status.progress = (int)(100.0 / status.maxScore * status.score);
            status.isComplete = status.progress == 100;
            return status;
        }
    }
}
