using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.plotters;
using SrvSurvey.units;
using System.Diagnostics;

namespace SrvSurvey.game
{
    class CommanderSettings : Data
    {
        #region static loading and caching code

        private static readonly Dictionary<string, CommanderSettings> cache = new();

        public static CommanderSettings Load(string fid, bool isOdyssey, string commanderName, bool noCache = false)
        {
            // use cache entry if present
            if (!noCache && cache.TryGetValue(fid, out CommanderSettings? value))
                return value!;

            var mode = isOdyssey ? "live" : "legacy";
            var filepath = Path.Combine(Program.dataFolder, $"{fid}-{mode}.json");

            return Data.Load<CommanderSettings>(filepath)
                ?? new CommanderSettings()
                {
                    filepath = filepath,
                    commander = commanderName,
                    fid = fid,
                    isOdyssey = isOdyssey,
                };
        }

        public override void Save()
        {
            // save colony data into a separate file
            if (this.colonyData != null)
            {
                var json = JsonConvert.SerializeObject(this.colonyData, Formatting.Indented);
                var filepath = this.filepath.Replace("-live.json", "-colony.json");
                saveWithRetry(filepath, json);
            }

            base.Save();
        }

        public static string currentOrLastFid
        {
            get => Game.activeGame?.cmdr?.fid ?? Game.settings.lastFid!;
        }

        public static CommanderSettings LoadCurrentOrLast()
        {
            // use cmdr from active game if possible
            var cmdr = Game.activeGame?.cmdr;

            // otherwise load the last active cmdr
            if (cmdr == null && Game.settings.lastCommander != null && Game.settings.lastFid != null)
                cmdr = CommanderSettings.Load(Game.settings.lastFid, true, Game.settings.lastCommander);

            if (cmdr == null)
                throw new Exception("You must use SrvSurvey at least once before this will work");

            return cmdr;
        }

        /// <summary>
        /// Returns a dictionary of FID to cmdr name 
        /// </summary>
        public static Dictionary<string, string> getAllCmdrs()
        {
            var cmdrs = new Dictionary<string, string>();
            var files = Directory.GetFiles(Program.dataFolder, "F*-live.json");
            foreach (var file in files)
            {
                var cmdr = JsonConvert.DeserializeObject<CommanderSettings>(File.ReadAllText(file));
                if (!string.IsNullOrWhiteSpace(cmdr?.commander))
                    cmdrs.Add(cmdr.fid, cmdr.commander);
            }

            return cmdrs;
        }

        public CommanderCodex loadCodex()
        {
            return CommanderCodex.Load(this.fid, this.commander);
        }

        #endregion

        #region data members

        public string fid;
        public string commander;
        public bool isOdyssey;

        /// <summary> The filename of the active journey </summary>
        public string? activeJourney;

        /// <summary>
        /// The name of the current star system, or body if we are close to one.
        /// </summary>
        public string lastSystemLocation;

        public string currentSystem;
        public long currentSystemAddress;
        public string? currentBody;
        public int currentBodyId;
        public decimal currentBodyRadius;
        public double[] starPos;
        public string galacticRegion = "$Codex_RegionName_18;"; // default to Inner Orion Spur
        public long currentMarketId;

        public string? lastOrganicScan;
        public BioScan? scanOne;
        public BioScan? scanTwo;
        public long organicRewards;

        public long explRewards;
        public double distanceTravelled;
        public int countJumps;
        public int countScans;
        public int countDSS;
        public int countLanded;

        /// <summary>
        /// The heading of the ship when we last touched down, or docked
        /// </summary>
        public decimal lastTouchdownHeading = -1;
        /// <summary>
        /// The location of the ship when we last touched down, or docked
        /// </summary>
        public LatLong2? lastTouchdownLocation;

        public bool migratedNonSystemDataOrganics = false;
        public bool migratedScannedOrganicsInEntryId = false;

        // spherical searching
        public SphereLimit sphereLimit = new SphereLimit();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<ScannedOrganic> scannedOrganics; // retire?

        /// <summary>
        /// A HashSet of union strings of "{systemAddress}_{bodyId}_{entryId}". Used to efficiently track final scans of bio signals, that can be undone upon death.
        /// </summary>
        public HashSet<string> scannedBioEntryIds = new HashSet<string>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, List<LatLong2>>? trackTargets; // retire?

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<TrackMassacre>? trackMassacres;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public BoxelSearch? boxelSearch;

        [JsonIgnore]
        public ColonyData colonyData = new();

        #endregion

        public void applyExplReward(long reward, string reason)
        {
            Game.log($"Gained: +{reward.ToString("N0")} for {reason}");
            this.explRewards += reward;

            Game.activeGame?.fireUpdate(true);
            this.Save();
        }

        public void setMarketId(long newMarketId)
        {
            Game.log($"setMarketId: {newMarketId}");
            this.currentMarketId = newMarketId;
            this.Save();
        }

        public void setTouchdown(LatLong2? location, decimal heading)
        {
            this.lastTouchdownLocation = location;
            this.lastTouchdownHeading = heading;
            this.Save();
        }

        public void clearTouchdown()
        {
            this.setTouchdown(null, -1);
        }

        public void setGalacticRegion(StarPos starPos)
        {
            var region = EliteDangerousRegionMap.RegionMap.FindRegion(starPos.x, starPos.y, starPos.z);
            var regionId = GalacticRegions.getNameFromIdx(region.Id);

            if (this.galacticRegion != regionId)
            {
                Game.log($"Congratulations for entering: {region.Name} ({regionId})");
                this.galacticRegion = regionId;
            }
        }

        /// <summary>
        /// Progress against Ram Tah Mission #1: Decode the Ruins
        /// </summary>
        public HashSet<string> decodeTheRuins = new HashSet<string>();
        public TahMissionStatus decodeTheRuinsMissionActive = TahMissionStatus.NotStarted;

        /// <summary>
        /// Progress against Ram Tah Mission #2: Decode the Ancient Logs
        /// </summary>
        public HashSet<string> decodeTheLogs = new HashSet<string>();
        public TahMissionStatus decodeTheLogsMissionActive = TahMissionStatus.NotStarted;

        public long reCalcOrganicRewards()
        {
            var newTotal = this.scannedBioEntryIds.Sum(_ =>
            {
                var parts = _.Split('_');
                var reward = parts.Length > 3 ? long.Parse(parts[3]) : 0;
                return parts.Length > 4 && parts[4] == bool.FalseString ? reward : reward * 5;
            });
            Game.log($"reCalcOrganicRewards: updated to: {newTotal.ToString("N0")}, was: {this.organicRewards.ToString("N0")}");
            this.organicRewards = newTotal;
            return this.organicRewards;
        }

        [JsonIgnore]
        public bool ramTahActive { get => this.decodeTheRuinsMissionActive == TahMissionStatus.Active || this.decodeTheLogsMissionActive == TahMissionStatus.Active; }

        #region massacre missions

        public void addMassacreMission(MissionAccepted entry)
        {
            if (!TrackMassacre.validMissionNames.Contains(entry.Name)) return;

            // skip if we already have this missionId
            if (this.trackMassacres?.Any(m => m.missionId == entry.MissionID) == true) return;

            Game.log($"Tracking new mission: {entry.KillCount} kills of {entry.TargetFaction} for {entry.Faction} (#{entry.MissionID})");

            this.trackMassacres ??= new();
            this.trackMassacres.Add(new TrackMassacre(entry));
            this.Save();

            Game.activeGame?.fireUpdate();
        }

        public void removeMassacreMission(long missionId, string verb)
        {
            if (this.trackMassacres == null) return;

            var mission = this.trackMassacres.Find(m => m.missionId == missionId);
            if (mission != null)
            {
                Game.log($"{verb} massacre mission: kill {mission.killCount} {mission.targetFaction} for {mission.missionGiver} (#{mission.missionId})");

                this.trackMassacres.Remove(mission);
                if (this.trackMassacres.Count == 0) this.trackMassacres = null;
                this.Save();

                Game.activeGame?.fireUpdate();
            }
        }

        public void trackBounty(Bounty bounty)
        {
            if (this.trackMassacres == null) return;

            var missionGivers = new HashSet<string>();
            var dirty = false;

            foreach (var mission in this.trackMassacres)
            {
                // decrement each tracked massacre who wants this faction killed, and we didn't already count one against that mission giver yet
                if (mission.targetFaction == bounty.VictimFaction && mission.remaining > 0 && !missionGivers.Contains(mission.missionGiver) && !mission.expired)
                {
                    mission.remaining--;
                    Game.log($"Massacre kill of '{mission.targetFaction}' ({bounty.PilotName_Localised}) for '{mission.missionGiver}', remaining: {mission.remaining} of {mission.killCount} (#{mission.missionId})");
                    missionGivers.Add(mission.missionGiver);
                    dirty = true;
                }
            }

            if (dirty)
            {
                this.Save();
                Program.invalidate<PlotMassacre>();
            }
        }

        #endregion

        /// <summary> Returns a StarPos populated for the cmdr's current system </summary>
        public StarPos getCurrentStarPos()
        {
            return new StarPos(this.starPos, this.currentSystem, this.currentSystemAddress);
        }

        /// <summary> Returns a StarRef populated for the cmdr's current system </summary>
        public StarRef getCurrentStarRef()
        {
            return new StarRef(this.starPos, this.currentSystem, this.currentSystemAddress);
        }

        /// <summary> Returns a Boxel populated for the cmdr's current system </summary>
        public Boxel getCurrentBoxel()
        {
            var bx = Boxel.parse(this.currentSystemAddress, this.currentSystem);
            if (bx == null)
            {
                Game.log($"Why no Boxel from: {this.currentSystemAddress} / {this.currentSystem} ?");
                Debugger.Break();
            }
            return bx!;
        }

        public CommanderJourney? loadActiveJourney()
        {
            if (this.activeJourney == null)
                return null;

            var journey = CommanderJourney.Load(this.fid, this.activeJourney);

            if (journey == null)
            {
                // we lost the file?
                Game.log($"Journey file not found: {activeJourney}");
                activeJourney = null;
                this.Save();
            }

            return journey;
        }

        [JsonIgnore]
        public FollowRoute route
        {
            get
            {
                if (this._followRoute == null)
                    this._followRoute = FollowRoute.Load(this.fid);

                return this._followRoute;
            }
        }
        private FollowRoute? _followRoute;

        public async Task loadColonyData()
        {
            var form = Program.getPlotter<PlotBuildCommodities>();

            // set an empty dictionary to make it render "updating..."
            if (form != null)
            {
                form.pendingDiff = new();
                form.Invalidate();
            }

            this.colonyData.projects = await Game.colony.getCmdrProjects(this.commander);
            this.colonyData.prepNeeds();

            //Game.log(this.colonySummary?.buildIds.formatWithHeader($"loadAllBuildProjects: loading: {this.colonySummary?.buildIds.Count} ...", "\r\n\t"));

            Game.log(colonyData.projects.Select(p => p.buildId).formatWithHeader($"colonySummary.buildIds: {colonyData.projects.Count}", "\r\n\t"));
            this.Save();

            if (form != null)
            {
                form.pendingDiff = null;
                form.Invalidate();
            }
        }
    }

    internal class SphereLimit
    {
        public bool active = false;
        public string? centerSystemName = null!;
        public double[]? centerStarPos = null!;
        public double radius = 100;
    }

    internal class TrackTargets
    {
        public string bodyName;
        public Dictionary<string, LatLong2> targets = new Dictionary<string, LatLong2>();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum TahMissionStatus
    {
        NotStarted,
        Active,
        Complete,
    }
}
