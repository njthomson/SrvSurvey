using Newtonsoft.Json;
using SrvSurvey.canonn;
using SrvSurvey.game;

namespace SrvSurvey.net
{
    internal class Spansh
    {
        private static HttpClient client;

        static Spansh()
        {
            Spansh.client = new HttpClient();
            //Spansh.client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            Spansh.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        public async Task<GetSystemResponse> getSystem(string systemName)
        {
            Game.log($"Requesting api/systems: {systemName}");

            var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/systems/field_values/system_names?q={Uri.EscapeDataString(systemName)}");
            var systems = JsonConvert.DeserializeObject<GetSystemResponse>(json)!;
            return systems;
        }

        public async Task<ApiSystemDumpSystem> getSystemDump(long systemAddress)
        {
            Game.log($"Requesting getSystem: {systemAddress}");

            var json = await Spansh.client.GetStringAsync($"https://spansh.co.uk/api/dump/{systemAddress}/");
            var systemDump = JsonConvert.DeserializeObject<ApiSystemDump>(json)!;
            return systemDump.system;
        }

    }
    internal class ApiSystemDump
    {
        public ApiSystemDumpSystem system;
    }

    internal class ApiSystemDumpSystem
    {
        public string allegiance;
        public int bodyCount;
        public List<ApiSystemDumpBody> bodies;
        public Coords coords;
        public DateTimeOffset date;
        public string government;
        public long id64;
        public string name;
        public string primaryEconomy;
        public string secondaryEconomy;
        public string security;
        // TODO: Stations[]
    }

    internal class ApiSystemDumpBody
    {
        public double? absoluteMagnitude;
        public long? age;
        public double? axialTilt;
        public int bodyId;
        public long? distanceToArrival;
        public long? id64;
        public string luminosity;
        public bool? mainStar;
        public string name;
        public double? rotationalPeriod;
        public bool? rotationalPeriodTidallyLocked;
        public double? solarMasses;
        public double? solarRadius;
        public string? spectralClass;
        // TODO stations[]
        public string subType;
        public double? surfaceTemperature;
        public string type;
        public DateTimeOffset updateTime;

        public double? earthMasses;
        public decimal? radius;
        public double? gravity;
        public string? atmosphereType;
        public string? volcanismType;
        public bool? isLandable;
        public string? terraformingState;
        
        public ApiSystemDumpSignals? signals;

        // TODO? parents[]

        public List<ApiSystemDumpRing>? rings;
    }

    internal class ApiSystemDumpSignals
    {
        public List<string>? genuses;
        public Dictionary<string, int>? signals;
        public DateTimeOffset updateTime;
    }

    internal class ApiSystemDumpRing
    {
        public string name;
        public string type;
        public double mass;
        public double innerRadius;
        public double outerRadius;
        public ApiSystemDumpSignals? signals;
    }

}
