using Newtonsoft.Json;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    internal class SystemData : Data
    {
        #region load, close and caching

        private static Dictionary<long, SystemData> cache = new Dictionary<long, SystemData>();

        /// <summary>
        /// Open or create a SystemData object for the a star system by name or address.
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="systemAddress"></param>
        /// <returns></returns>
        public static SystemData Load(string systemName, long systemAddress)
        {
            Game.log($"Loading SystemData for: '{systemName}' ({systemAddress})");
            lock (cache)
            {
                // use cache entry if present
                if (cache.ContainsKey(systemAddress))
                {
                    return cache[systemAddress];
                }

                // try finding files by systemAddress first, then system name
                var path = Path.Combine(Application.UserAppDataPath, "systems", Game.activeGame!.fid!);
                Directory.CreateDirectory(path);
                var files = Directory.GetFiles(path, $"*{systemAddress}.json");
                if (files.Length == 0)
                {
                    files = Directory.GetFiles(path, $"{systemName}*.json");
                }

                // create new if no matches found
                if (files.Length == 0)
                {
                    Game.log($"Nothing found, creating new SystemData for: '{systemName}' ({systemAddress})");
                    cache[systemAddress] = new SystemData()
                    {
                        name = systemName,
                        address = systemAddress,
                    };

                    return cache[systemAddress];
                }
                else if (files.Length > 1)
                {
                    Game.log($"Why are there {files.Length} multiple files for '{systemName}' ({systemAddress})? Using the first one.");
                }

                var filepath = files[0];
                cache[systemAddress] = Data.Load<SystemData>(filepath)!;
                return cache[systemAddress];
            }
        }

        /// <summary>
        /// Close a SystemData object for the a star system by name or address.
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="systemAddress"></param>
        public static void Close(SystemData data)
        {
            Game.log($"Closing and saving SystemData for: '{data.name}' ({data.address})");
            lock (cache)
            {
                // ensure data is saved then remove from the cache
                data.Save();

                if (cache.ContainsValue(data))
                    cache.Remove(data.address);
            }
        }

        #endregion

        #region data members

        /// <summary> SystemName </summary>
        public string name;

        /// <summary> SystemAddress - galactic system address </summary>
        public long address;

        /// <summary> StarPos - array of galactic [ x, y, z ] co-ordinates </summary>
        public double[] pos;

        public string commander;
        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;

        /// <summary> Returns True when all bodies have been found with FSS </summary>
        public int bodyCount;

        /// <summary> True once a FSSDiscoveryScan is received for this system </summary>
        public bool honked;

        /// <summary> A list of all bodies detected in this system </summary>
        public List<SystemBody> bodies;

        #endregion

        /// <summary> Returns True when all bodies have been found with FSS </summary>
        [JsonIgnore]
        public bool fssComplete { get => this.bodies?.Count == this.bodyCount; }

    }

    internal enum SystemBodyType
    {
        Star,
        GasGiant,
        SolidBody,
        LandableBody,
    }

    internal class SystemBody
    {
        #region data members

        /// <summary> BodyName - the full body name, typically with the system name embedded </summary>
        public string name;
        /// <summary> BodyId - id relative within the star system </summary>
        public int id;

        /// <summary> Is this a star, gas-giant, or landable body, etc </summary>
        public SystemBodyType type;

        /// <summary> Has a DSS been done on this body? </summary>
        public bool dssComplete;

        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;


        /// <summary> Location of last touchdown </summary>
        public LatLong2? lastTouchdown;

        public List<string> rings = new List<string>(); // TODO: fix type

        /// <summary> Locations of all scans performed on this body </summary>
        public List<BioScan> scanLocations = new List<BioScan>(); // TODO: fix type

        /// <summary> All the organisms for this body </summary>
        public List<string> organisms = new List<string>(); // TODO: fix type

        #endregion

        [JsonIgnore]
        public bool hasRings { get => this.rings?.Count > 0; }

    }
}
