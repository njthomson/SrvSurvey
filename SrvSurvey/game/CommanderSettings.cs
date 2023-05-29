namespace SrvSurvey.game
{
    class CommanderSettings : Data
    {
        public static CommanderSettings Load(string fid, bool isOdyssey, string commanderName)
        {
            var mode = isOdyssey ? "live" : "legacy";
            var filepath = Path.Combine(Application.UserAppDataPath, $"{fid}-{mode}.json");

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
        public double currentBodyRadius;
        public double[] starPos;

        public string? lastOrganicScan;
        public BioScan? scanOne;
        public BioScan? scanTwo;
        public long organicRewards;
        public List<ScannedOrganic> scannedOrganics = new List<ScannedOrganic>();

        // spherical searching
        public SphereLimit sphereLimit = new SphereLimit();
    }

    internal class SphereLimit
    {
        public bool active = false;
        public string centerSystemName = null!;
        public double[] centerStarPos = null!;
        public double radius = 100;
    }

}
