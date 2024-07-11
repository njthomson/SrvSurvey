using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey.net.EDSM
{
    internal class EDSM
    {
        public async Task<StarSystem[]> getSystems(string systemName)
        {
            // docs: https://www.edsm.net/en/api-v1
            // https://www.edsm.net/api-v1/systems?showCoordinates=1&systemName=Colonia
            Game.log($"Getting systems by name: {systemName}");

            var json = await new HttpClient().GetStringAsync($"https://www.edsm.net/api-v1/systems?showCoordinates=1&showId=1&systemName={Uri.EscapeDataString(systemName)}");
            return JsonConvert.DeserializeObject<StarSystem[]>(json)!;
        }

        public async Task<EdsmSystem> getBodies(string systemName)
        {
            // docs: https://www.edsm.net/en/api-system-v1
            // https://www.edsm.net/api-system-v1/bodies?systemName=Colonia
            Game.log($"Getting system bodies by name: {systemName}");

            var json = await new HttpClient().GetStringAsync($"https://www.edsm.net/api-system-v1/bodies?systemName={Uri.EscapeDataString(systemName)}");
            return JsonConvert.DeserializeObject<EdsmSystem>(json)!;
        }

    }

    internal class StarSystem
    {
        // [{"name":"Colonia","id":3384966,"id64":3238296097059,"coords":{"x":-9530.5,"y":-910.28125,"z":19808.125},"coordsLocked":true}]

        public string name;
        public long id;
        public long id64;
        public StarPos coords;
        public bool coordsLocked;

        public override string ToString()
        {
            return $"{name} ({id64}, [{coords.x},{coords.y},{coords.z}]";
        }
    }

    internal class EdsmRing
    {
        public string name;
        public string type;
        public double mass;
        public double innerRadius;
        public double outerRadius;
    }


    internal class EdsmBody
    {
        public int id;
        public int? bodyId;
        public string name;
        public string type;
        public string subType;
        public long distanceToArrival;
        public List<Dictionary<ParentBodyType, int>> parents;
        public bool isMainStar;
        public bool isScoopable;
        public bool isLandable;
        public long age;
        public string luminosity;
        public double absoluteMagnitude;
        public double solarMasses;
        public double solarRadius;
        public double earthMasses;
        public double? gravity;
        public string? atmosphereType;
        public Dictionary<string, float>? atmosphereComposition;
        public Dictionary<string, float>? materials;
        public string? volcanismType;
        public decimal radius;
        public double surfaceTemperature;
        public double? surfacePressure;
        public double? orbitalPeriod;
        public double? semiMajorAxis;
        public double? orbitalEccentricity;
        public double? orbitalInclination;
        public double? argOfPeriapsis;
        public double rotationalPeriod;
        public bool rotationalPeriodTidallyLocked;
        public double? axialTilt;
        public string? terraformingState;

        public List<EdsmRing> rings;

        public EdsmDiscoverer? discovery;
    }

    internal class EdsmDiscoverer
    {
        public string commander;
        public DateTime date;
    }

    internal class EdsmSystem
    {
        public string name;
        public long id;
        public long id64;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int bodyCount;
        public List<EdsmBody> bodies;

        public override string ToString()
        {
            return $"{name} ({id64})";
        }
    }

}
