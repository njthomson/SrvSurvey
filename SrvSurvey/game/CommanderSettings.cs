using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    class CommanderSettings : Data
    {
        public static CommanderSettings Load(string fid, bool isOdyssey, string commanderName)
        {
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

        public string fid;
        public string commander;
        public bool isOdyssey;

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

        public string? lastOrganicScan;
        public BioScan? scanOne;
        public BioScan? scanTwo;
        public long organicRewards;
        public List<ScannedOrganic> scannedOrganics = new List<ScannedOrganic>();
        /// <summary>
        /// A HashSet of union strings of "{systemAddress}_{bodyId}_{entryId}". Used to efficiently track final scans of bio signals, that can be undone upon death.
        /// </summary>
        public HashSet<string> scannedBioEntryIds = new HashSet<string>();

        // spherical searching
        public SphereLimit sphereLimit = new SphereLimit();

        public Dictionary<string, List<LatLong2>>? trackTargets;

        public bool migratedNonSystemDataOrganics = false;
        public bool migratedScannedOrganicsInEntryId = false;

        public long currentMarketId;

        /// <summary>
        /// The heading of the ship when we last touched down, or docked
        /// </summary>
        public decimal lastTouchdownHeading = -1;
        /// <summary>
        /// The location of the ship when we last touched down, or docked
        /// </summary>
        public LatLong2? lastTouchdownLocation;

        public void setMarketId(long newMarketId)
        {
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
