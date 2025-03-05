using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{

    class ColonyData
    {
        public List<Project> projects = new();
        [JsonIgnore]
        public Dictionary<string, int> allCommodities = new();
        [JsonIgnore]
        public Dictionary<string, List<Project>> assigned = new();

        public void prepNeeds(string cmdr)
        {
            allCommodities.Clear();
            assigned.Clear();
            foreach (var project in projects)
            {
                // prep aggregated commodity needs
                foreach (var (commodity, need) in project.commodities)
                {
                    if (!allCommodities.ContainsKey(commodity)) allCommodities[commodity] = 0;
                    allCommodities[commodity] += need;
                }

                // prep assignments
                var assignment = project.commanders?.GetValueOrDefault(cmdr);
                if (assignment != null)
                    foreach (var commodity in assignment)
                        assigned.init(commodity).Add(project);
            }

            Game.log("ColonyData.prepNeeds: done");
        }

        public bool has(string buildId)
        {
            return projects.Any(p => p.buildId == buildId);
        }

        public bool has(long id64, long marketId)
        {
            return projects.Any(p => p.systemAddress == id64 && p.marketId == marketId);
        }

        [JsonIgnore]
        public string[] buildIds => projects?.Select(p => p.buildId).ToArray() ?? new string[0];

        public string? getBuildId(long id64, long marketId)
        {
            return projects?.FirstOrDefault(p => p.systemAddress == id64 && p.marketId == marketId)?.buildId;
        }

    }
}
