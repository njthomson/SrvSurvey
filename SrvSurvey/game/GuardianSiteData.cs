using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.canonn;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal class GuardianSiteData : Data
    {
        private static string rootFolder = Path.Combine(Application.UserAppDataPath, "guardian");

        public static string getFilename(ApproachSettlement entry)
        {
            var index = parseSettlementIdx(entry.Name);
            return getFilename(entry.BodyName, index);
        }

        public static string getFilename(string bodyName, int index)
        {
            var namePart = "ruins"; // TODO: structures?
            return $"{bodyName}-{namePart}-{index}.json";
        }

        public static GuardianSiteData Load(string bodyName, int index)
        {
            string filepath = Path.Combine(rootFolder, Game.activeGame!.fid!, getFilename(bodyName, index));
            return Data.Load<GuardianSiteData>(filepath);
        }

        public static GuardianSiteData Load(ApproachSettlement entry)
        {
            string filepath = Path.Combine(rootFolder, Game.activeGame!.fid!, getFilename(entry));
            Directory.CreateDirectory(rootFolder);
            var data = Data.Load<GuardianSiteData>(filepath);

            // create new if needed
            if (data == null)
            {
                // system name is missing on ApproachSettlement, so take the current system - assuming it's a match
                var cmdr = Game.activeGame.cmdr;
                var systemName = "";

                // might be bad idea?
                if (entry.SystemAddress == cmdr.currentSystemAddress && entry.BodyID == cmdr.currentBodyId)
                    systemName = cmdr.currentSystem;

                data = new GuardianSiteData()
                {
                    name = entry.Name,
                    nameLocalised = entry.Name_Localised,
                    commander = Game.activeGame.Commander!,
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
                };
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

            if (Game.canonn.ruinSummaries != null && data.type == SiteType.Unknown)
            {
                var grSites = Game.canonn.ruinSummaries.Where(_ => _.bodyId == entry.BodyID && _.systemAddress == entry.SystemAddress);

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
        public string notes;

        public Dictionary<string, bool> confirmedPOI = new Dictionary<string, bool>();
        public Dictionary<string, SitePoiStatus> poiStatus = new Dictionary<string, SitePoiStatus>();

        #endregion

        [JsonIgnore]
        public bool isRuins { get => this.name.StartsWith("$Ancient:"); }

        public static int parseSettlementIdx(string name)
        {
            const string ruinsPrefix = "$Ancient:#index=";
            // $Ancient:#index=2;
            if (name.StartsWith(ruinsPrefix))
            {
                return int.Parse(name.Substring(ruinsPrefix.Length, 1));
            }
            throw new Exception("Unkown site type");
        }

        public enum SiteType
        {
            Unknown,
            Alpha,
            Beta,
            Gamma,
            // structures ... ?
        }

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

        public static List<GuardianSiteData> loadAllSites()
        {
            var folder = Path.Combine(rootFolder, Game.settings.lastFid!);
            if (!Directory.Exists(folder))
                return new List<GuardianSiteData>();

            var files = Directory.GetFiles(folder);

            Game.log($"Reading {files.Length} ruins files from disk");
            return files
                .Select(filename => Data.Load<GuardianSiteData>(filename))
                .ToList()!;
        }

    }
}

