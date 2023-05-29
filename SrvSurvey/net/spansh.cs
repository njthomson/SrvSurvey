using Newtonsoft.Json;
using SrvSurvey.canonn;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SrvSurvey.canonn.GuardianRuinEntry;

namespace SrvSurvey.net
{
    internal class Spansh
    {
        public async Task<GetSystemResponse> getSystem(string systemName)
        {
            Game.log($"Requesting getSystem: {systemName}");

            var json = await new HttpClient().GetStringAsync($"https://spansh.co.uk/api/systems/field_values/system_names?q={systemName}");
            var systems = JsonConvert.DeserializeObject<GetSystemResponse>(json)!;

            Game.log(systems!);

            return systems;
        }
    }
}
