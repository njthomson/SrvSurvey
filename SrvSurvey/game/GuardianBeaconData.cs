using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.units;
using System.Security.Cryptography;

namespace SrvSurvey.game
{
    internal class GuardianBeaconData : Data
    {
        private static string rootFolder = Path.Combine(Program.dataFolder, "guardian");

        public static string getFilepath(string systemName)
        {
            var fid = Game.activeGame?.fid ?? Game.settings.lastFid!;
            var folder = Path.Combine(rootFolder, fid!);
            if (!Util.isOdyssey) folder = Path.Combine(folder, "legacy");

            var filepath = Path.Combine(folder, $"{systemName}-beacon.json");
            Directory.CreateDirectory(folder);
            return filepath;
        }

        public static GuardianBeaconData? Load(string systemName)
        {
            var filepath = GuardianBeaconData.getFilepath(systemName);
            return GuardianBeaconData.Load<GuardianBeaconData>(filepath);
        }

        public static GuardianBeaconData Load(CodexEntry entry)
        {
            var filepath = GuardianBeaconData.getFilepath(entry.System);
            var data = GuardianBeaconData.Load<GuardianBeaconData>(filepath);

            // create new if needed
            if (data == null)
            {
                // body name is missing on CodexEntry, so we get it from the master list
                var bodyName = Game.canonn.allBeacons.Find(_ => _.systemAddress == entry.SystemAddress && _.bodyId == entry.BodyID)?.bodyName!;

                data = new GuardianBeaconData()
                {
                    commander = Game.activeGame?.Commander!,
                    filepath = filepath,
                    systemAddress = entry.SystemAddress,
                    systemName = entry.System,
                    bodyId = entry.BodyID ?? -1,
                    bodyName = bodyName,
                    firstVisited = DateTimeOffset.UtcNow,
                    lastVisited = DateTimeOffset.UtcNow,
                    legacy = !Util.isOdyssey,
                    scannedLocations = new Dictionary<DateTimeOffset, LatLong2>(),
                };
                data.Save();
            }

            return data;
        }

        #region data members

        public string commander;
        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;
        public string systemName;
        public long systemAddress;
        public string bodyName;
        public int bodyId;
        public string notes;

        public bool legacy = false;

        public Dictionary<DateTimeOffset, LatLong2> scannedLocations;

        #endregion
    }
}

