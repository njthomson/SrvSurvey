using Newtonsoft.Json;
using SrvSurvey.plotters;
using static SrvSurvey.plotters.PlotVerticalStripe;

namespace SrvSurvey.game
{
    class ColonyData : Data
    {
        public static string SystemColonisationShip = "System Colonisation Ship";

        public static bool isConstructionSite(Docked? entry)
        {
            return entry != null && entry.StationServices?.Contains("colonisationcontribution") == true;
        }

        public static ColonyData Load(string fid, string cmdr)
        {
            var filepath = Path.Combine(Program.dataFolder, $"{fid}-colony.json");

            var data = Data.Load<ColonyData>(filepath);
            if (data == null)
                data = new ColonyData() { filepath = filepath };

            // migrate from original file format
            data.cmdr ??= cmdr;

            return data;
        }

        public async Task fetchLatest()
        {
            var form = Program.getPlotter<PlotBuildCommodities>();

            // set an empty dictionary to make it render "updating..."
            if (form != null)
            {
                form.pendingDiff = new();
                form.Invalidate();
            }

            var summary = await Game.colony.getCmdrSummary(this.cmdr);
            this.primaryBuildId = summary.primaryBuildId;
            // ACTIVE projects only
            this.projects = summary.projects.Where(p => !p.complete).ToList();

            // extract all marketId's from linked FCs
            this.allLinkedFCs.Clear();
            this.projects.ForEach(p => p.linkedFC.ForEach(fc => this.allLinkedFCs[fc.marketId] = fc));

            this.prepNeeds();

            //Game.log(this.colonySummary?.buildIds.formatWithHeader($"loadAllBuildProjects: loading: {this.colonySummary?.buildIds.Count} ...", "\r\n\t"));

            Game.log(this.projects.Select(p => p.buildId).formatWithHeader($"colonySummary.buildIds: {this.projects.Count}", "\r\n\t"));
            this.Save();

            if (form != null)
            {
                form.pendingDiff = null;
                form.Invalidate();
            }
        }

        #region instance data

        public List<Project> projects = new();

        public string cmdr;
        public string? primaryBuildId;
        public bool fcTracking = true;
        public Dictionary<string, int> fcCommodities = new();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<long, ProjectFC> allLinkedFCs = new();

        #endregion

        [JsonIgnore]
        public Needs allNeeds;

        public void prepNeeds()
        {
            allNeeds = getLocalNeeds(-1, -1);
            Game.log("ColonyData.prepNeeds: done");
        }

        public Project? getProject(string buildId)
        {
            if (buildId == null) return null;
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

        public void supplyNeeds(long systemAddress, long marketId, Dictionary<string, int> diff)
        {
            foreach (var name in diff.Keys) diff[name] *= -1;

            var localProject = this.getProject(systemAddress, marketId);
            if (localProject == null)
            {
                Game.log(diff.formatWithHeader($"TODO! Supplying commodities for untracked project: {systemAddress}/{marketId}", "\r\n\t"));
                // TODO: call the API but do no tracking
                return;
            }
            else
            {
                Game.log(diff.formatWithHeader($"Supplying commodities for: {localProject.buildId} ({systemAddress}/{marketId})", "\r\n\t"));

                var form = Program.getPlotter<PlotBuildCommodities>();
                if (form != null)
                {
                    form.pendingDiff = diff;
                    form.Invalidate();
                }

                Game.colony.supply(localProject.buildId, this.cmdr, diff).continueOnMain(null, updatedCommodities =>
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
