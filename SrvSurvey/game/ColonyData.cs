using Newtonsoft.Json;
using SrvSurvey.plotters;

namespace SrvSurvey.game
{

    class ColonyData
    {
        public static string SystemColonisationShip = "System Colonisation Ship";

        public List<Project> projects = new();
        //[JsonIgnore]
        //public Dictionary<string, int> allCommodities = new();
        //[JsonIgnore]
        //public Dictionary<string, List<Project>> assigned = new();

        [JsonIgnore]
        public Needs allNeeds;

        private string cmdr => Game.activeGame?.Commander!;

        public void prepNeeds()
        {
            allNeeds = getLocalNeeds(-1, -1);
            Game.log("ColonyData.prepNeeds: done");
        }

        public Project? getProject(string buildId)
        {
            return projects?.FirstOrDefault(p => p.buildId == buildId);
        }

        public Project? getProject(long id64, long marketId)
        {
            return projects?.FirstOrDefault(p => p.systemAddress == id64 && p.marketId == marketId);
        }

        public string? getBuildId(long id64, long marketId)
        {
            return projects?.FirstOrDefault(p => p.systemAddress == id64 && p.marketId == marketId)?.buildId;
        }

        public bool has(Docked? docked)
        {
            if (docked == null)
                return false;
            else
                return has(docked.SystemAddress, docked.MarketID);
        }

        public bool has(long address, long marketId)
        {
            return projects.Any(p => p.systemAddress == address && p.marketId == marketId);
        }

        public Needs getLocalNeeds(long address, long marketId)
        {
            var needs = new Needs();

            foreach (var project in projects)
            {
                if (address != -1 && project.systemAddress != address) continue;
                if (marketId != -1 && project.marketId != marketId) continue;

                // prep aggregated commodity needs
                foreach (var (commodity, need) in project.commodities)
                {
                    needs.commodities.init(commodity);
                    needs.commodities[commodity] += need;
                }

                // prep assignments
                var assignment = project.commanders?.GetValueOrDefault(cmdr, StringComparison.OrdinalIgnoreCase);
                if (assignment != null)
                    foreach (var commodity in assignment)
                        needs.assigned.Add(commodity);
            }

            return needs;
        }

        public void supplyNeeds(Docked? lastDocked, Dictionary<string, int> diff)
        {
            if (lastDocked == null) return;

            foreach (var name in diff.Keys) diff[name] *= -1;

            var localProject = this.getProject(lastDocked.SystemAddress, lastDocked.MarketID);
            if (localProject == null)
            {
                Game.log(diff.formatWithHeader($"TODO! Supplying commodities for untracked project: {lastDocked.SystemAddress}/{lastDocked.MarketID}", "\r\n\t"));
                // TODO: call the API but do no tracking
                return;
            }
            else
            {
                Game.log(diff.formatWithHeader($"Supplying commodities for: {localProject.buildId} ({lastDocked.SystemAddress}/{lastDocked.MarketID})", "\r\n\t"));

                var form = Program.getPlotter<PlotBuildCommodities>();
                if (form != null)
                {
                    form.pendingDiff = diff;
                    form.Invalidate();
                }

                Game.colony.supply(localProject.buildId, diff).continueOnMain(null, updatedCommodities =>
                {
                    // update local numbers
                    foreach (var (name, count) in updatedCommodities)
                        localProject.commodities[name] = count;

                    Game.log(updatedCommodities.formatWithHeader($"Local updates applied:", "\r\n\t"));

                    // wait a bit then force plotter to re-render
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (form != null && !form.IsDisposed)
                        {
                            form.pendingDiff = null;
                            form.Invalidate();
                        }
                    });
                });
            }
        }

        public class Needs
        {
            public Dictionary<string, int> commodities = new();
            public HashSet<string> assigned = new();
        }
    }
}
