using Newtonsoft.Json;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SrvSurvey.canonn.GuardianRuinEntry;

namespace SrvSurvey.net.EDSM
{
    internal class EDSM
    {
        public async Task<StarSystem[]> getSystems(string systemName)
        {
            // docs: https://www.edsm.net/en/api-v1
            // https://www.edsm.net/api-v1/systems?showCoordinates=1&systemName=Colonia
            Game.log($"Getting systems by name: {systemName}");

            var json = await new HttpClient().GetStringAsync($"https://www.edsm.net/api-v1/systems?showCoordinates=1&systemName={systemName}");
            return JsonConvert.DeserializeObject<StarSystem[]>(json)!;
        }

    }

    internal class SystemCoords
    {
        public double x;
        public double y;
        public double z;

        [JsonIgnore]
        public double[] starPos { get => new double[] { this.x, this.y, this.z } ;}
    }

    internal class StarSystem
    {
        // [{"name":"Colonia","id":3384966,"id64":3238296097059,"coords":{"x":-9530.5,"y":-910.28125,"z":19808.125},"coordsLocked":true}]

        public string name;
        public long id;
        public long id64;
        public SystemCoords coords;
        public bool coordsLocked;

        public override string ToString()
        {
            return $"{name} ({id64}, [{coords.x},{coords.y},{coords.z}]";
        }
    }
}
