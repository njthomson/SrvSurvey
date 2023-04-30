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
        public string lastSystemLocation;
        public string? lastOrganicScan;
        public BioScan? scanOne;
        public BioScan? scanTwo;
        public long organicRewards;
        public List<ScannedOrganic> scannedOrganics = new List<ScannedOrganic>();
    }
}
