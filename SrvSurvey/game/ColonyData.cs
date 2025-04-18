using Newtonsoft.Json;
using SrvSurvey.plotters;

namespace SrvSurvey.game
{
    class ColonyData : Data
    {
        public static Project? localUntrackedProject;

        public static string SystemColonisationShip = "System Colonisation Ship";
        public static string ExtPanelColonisationShip = "$EXT_PANEL_ColonisationShip:#index=1;";
        public static string PlanetaryConstructionSite = "Planetary Construction Site:";
        public static string OrbitalConstructionSite = "Orbital Construction Site:";

        public static bool isConstructionSite(Docked? entry)
        {
            return entry != null && isConstructionSite(entry.StationName, entry.StationServices);
        }

        public static bool isConstructionSite(string stationName, List<string>? stationServices)
        {
            return stationName != null && stationServices != null && stationServices.Contains("colonisationcontribution") == true &&
            (
                stationName.StartsWith(PlanetaryConstructionSite, StringComparison.OrdinalIgnoreCase)
                || stationName.StartsWith(OrbitalConstructionSite, StringComparison.OrdinalIgnoreCase)
                || stationName.Equals(ExtPanelColonisationShip, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string getDefaultProjectName(Docked lastDocked)
        {
            var defaultName = lastDocked.StationName == ColonyData.SystemColonisationShip || lastDocked.StationName == ColonyData.ExtPanelColonisationShip
                ? $"Primary port: {lastDocked.StarSystem}"
                : lastDocked.StationName
                    .Replace(PlanetaryConstructionSite, "")
                    .Replace(OrbitalConstructionSite, "")
                    .Trim()
                ?? "";

            return defaultName;
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

        public async Task fetchLatest(string? buildId = null)
        {
            // Make plotter it render "updating..."
            Program.getPlotter<PlotBuildCommodities>()?.startPending();
            try
            {
                if (buildId == null)
                {
                    // fetch all ACTIVE projects and primaryBuildId
                    await Task.WhenAll(
                        Game.colony.getPrimary(this.cmdr).continueOnMain(null, newPrimaryBuildId => this.primaryBuildId = newPrimaryBuildId, true),
                        Game.colony.getCmdrActive(this.cmdr).continueOnMain(null, newProjects => this.projects = newProjects)
                    );
                    if (this.primaryBuildId != null && getProject(this.primaryBuildId) == null)
                    {
                        Game.log($"Not found: primaryBuildId: ${primaryBuildId} ?");
                        this.primaryBuildId = null;
                        //Debugger.Break();
                    }
                }
                else
                {
                    // fetch just the given project
                    var freshProject = await Game.colony.getProject(buildId);
                    var idx = this.projects.FindIndex(p => p.buildId == buildId);
                    if (idx >= 0 && freshProject != null)
                        this.projects[idx] = freshProject;
                }

                // request data for all cmdr linked FCs
                var allFCs = await Game.colony.getAllCmdrFCs(this.cmdr)!;
                this.linkedFCs = allFCs.ToDictionary(fc => fc.marketId, fc => fc);

                // sum their respective cargo
                var sumCargo = getSumCargoFC(allFCs);
                this.sumCargoLinkedFCs = sumCargo;

                this.prepNeeds();

                //Game.log(this.colonySummary?.buildIds.formatWithHeader($"loadAllBuildProjects: loading: {this.colonySummary?.buildIds.Count} ...", "\r\n\t"));

                Game.log(this.projects.Select(p => $"{p.buildId} : {p.buildName}").formatWithHeader($"colonySummary.builds: {this.projects.Count}", "\r\n\t"));
                Game.log(this.linkedFCs.Select(fc => $"{fc.Value.displayName} ({fc.Value.name})").formatWithHeader($"colonySummary.linkedFCs: {this.linkedFCs.Count}", "\r\n\t"));

                this.Save();
            }
            finally
            {
                Program.getPlotter<PlotBuildCommodities>()?.endPending();
            }
        }

        public static Dictionary<string, int> getSumCargoFC(IEnumerable<FleetCarrier> FCs)
        {
            // sum their respective cargo
            var sumCargo = new Dictionary<string, int>();
            foreach (var fc in FCs)
            {
                foreach (var (commodity, need) in fc.cargo)
                {
                    sumCargo.init(commodity);
                    sumCargo[commodity] += need;
                }
            }

            return sumCargo;
        }

        #region instance data

        public List<Project> projects = new();

        public string cmdr;
        public string? primaryBuildId;
        public bool fcTracking = true;
        public Dictionary<string, int> fcCommodities = new();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<long, FleetCarrier> linkedFCs = new();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, int> sumCargoLinkedFCs = new();

        #endregion

        [JsonIgnore]
        public Needs allNeeds;

        public void prepNeeds()
        {
            allNeeds = getLocalNeeds(-1, -1);
            Game.log("ColonyData.prepNeeds: done");
        }

        public Project? getProject(string? buildId)
        {
            if (buildId == null) return null;
            return projects?.FirstOrDefault(p => p.buildId == buildId);
        }

        public Project? getProject(long id64, long marketId)
        {
            return projects?.FirstOrDefault(p => p.systemAddress == id64 && p.marketId == marketId);
        }

        public string? getBuildId(Docked? entry)
        {
            if (entry == null)
                return null;
            else
                return projects?.FirstOrDefault(p => p.systemAddress == entry.SystemAddress && p.marketId == entry.MarketID)?.buildId;
        }

        public string? getBuildId(long id64, long marketId)
        {
            return projects?.FirstOrDefault(p => p.systemAddress == id64 && p.marketId == marketId)?.buildId;
        }

        public bool has(string? buildId)
        {
            return getProject(buildId) != null;
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

        public void contributeNeeds(long systemAddress, long marketId, Dictionary<string, int> diff)
        {
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
                Program.getPlotter<PlotBuildCommodities>()?.startPending(diff);

                Game.colony.contribute(localProject.buildId, this.cmdr, diff).continueOnMain(null, () =>
                {
                    // wait a bit then force plotter to re-render
                    Task.Delay(500).ContinueWith(t => Program.getPlotter<PlotBuildCommodities>()?.endPending());
                });
            }
        }

        public void checkConstructionSiteUponDocking(Docked entry, SystemBody? body)
        {
            var proj = this.getProject(entry.SystemAddress, entry.MarketID);
            if (proj == null)
            {
                // it's possible someone else might be tracking it?
                Game.colony.getProject(entry.SystemAddress, entry.MarketID).continueOnMain(null, otherProj =>
                {
                    Game.log($"Found local project untracked by cmdr: {otherProj?.buildName} ({otherProj?.buildId})");
                    ColonyData.localUntrackedProject = otherProj;
                });
                return;
            }

            ProjectUpdate? updatedProject = null;

            // update faction name?
            if (proj.factionName != entry.StationFaction.Name)
            {
                Game.log($"Auto-update factionName: {proj.factionName} => {entry.StationFaction.Name}");
                updatedProject ??= new(proj.buildId);
                updatedProject.factionName = entry.StationFaction.Name;
            }

            // update body name/num
            if (body != null && (proj.bodyNum != body.id || proj.bodyName != body.name))
            {
                Game.log($"Auto-update bodyName/bodyNum: {proj.bodyName} => {body.name} / {proj.bodyNum} => {body.id}");
                updatedProject ??= new(proj.buildId);
                updatedProject.bodyName = body.name;
                updatedProject.bodyNum = body.id;
            }

            if (updatedProject != null)
                Game.colony.update(updatedProject).justDoIt();
        }

        public void updateNeeds(ColonisationConstructionDepot entry, long id64)
        {
            var proj = this.getProject(id64, entry.MarketID);
            if (proj == null) return;

            var needed = entry.ResourcesRequired.ToDictionary(
                r => r.Name.Substring(1).Replace("_name;", ""),
                r => r.RequiredAmount - r.ProvidedAmount
            );
            var totalDiff = needed.Keys
                .Select(k => needed[k] - proj.commodities.GetValueOrDefault(k, 0)).Sum();

            if (totalDiff != 0)
            {
                Program.getPlotter<PlotBuildCommodities>()?.startPending();
                var updateProj = new ProjectUpdate(proj.buildId)
                {
                    commodities = needed,
                    maxNeed = entry.ResourcesRequired.Sum(r => r.RequiredAmount),
                };

                Game.colony.update(updateProj).continueOnMain(null, savedProj =>
                {
                    // update in-memory track
                    var idx = this.projects.FindIndex(p => p.buildId == savedProj.buildId);
                    this.projects[idx] = savedProj;
                    this.Save();

                    Game.log(savedProj);
                    Program.getPlotter<PlotBuildCommodities>()?.endPending();
                }).justDoIt();
            }
        }

        public async Task updateFromMarketFC(MarketFile marketFile)
        {
            if (!linkedFCs.ContainsKey(marketFile.MarketId)) return;

            Program.getPlotter<PlotBuildCommodities>()?.startPending();

            // fetch latest numbers first
            var fc = await Game.colony.getFC(marketFile.MarketId);
            Game.log($"updateFromMarketFC: Checking FC cargo against market: {fc.marketId} : {fc.displayName} ({fc.name})");

            // if the FC has something for sale with a different count than we think ... update it
            var newCargo = new Dictionary<string, int>();
            foreach(var entry in marketFile.Items)
            {
                if (!entry.Producer) continue;
                var key = entry.Name.Substring(1).Replace("_name;", "");
                if (!fc.cargo.ContainsKey(key) || fc.cargo.GetValueOrDefault(key) != entry.Stock)
                {
                    newCargo[key] = entry.Stock;
                }
            }

            if (newCargo.Count > 0)
            {
                Program.getPlotter<PlotBuildCommodities>()?.endPending();
                Program.getPlotter<PlotBuildCommodities>()?.startPending(newCargo);

                var updatedCargo = await Game.colony.updateCargoFC(fc.marketId, newCargo);
                // apply new numbers and save
                linkedFCs[fc.marketId].cargo = updatedCargo;

                var sumCargo = getSumCargoFC(linkedFCs.Values);
                this.sumCargoLinkedFCs = sumCargo;

                this.Save();
            }

            await Task.Delay(500);
            Program.getPlotter<PlotBuildCommodities>()?.endPending();
        }

        public class Needs
        {
            public Dictionary<string, int> commodities = new();
            public HashSet<string> assigned = new();
        }

        public static Dictionary<string, string[]> mapCargoType = new Dictionary<string, string[]>()
        {
            { "Chemicals", new string[] { "liquidoxygen","pesticides","surfacestabilisers","water" } },
            { "Consumer Items", new string[] { "evacuationshelter","survivalequipment","beer","liquor","wine" } },
            { "Foods", new string[] { "animalmeat","coffee","fish","foodcartridges","fruitandvegetables","grain","tea" } },
            { "Industrial Materials", new string[] { "ceramiccomposites","cmmcomposite","insulatingmembrane","polymers","semiconductors","superconductors" } },
            { "Machinery", new string[] { "buildingfabricators","cropharvesters","emergencypowercells","geologicalequipment","microbialfurnaces","mineralextractors","powergenerators","thermalcoolingunits","waterpurifiers" } },
            { "Medicines", new string[] { "agriculturalmedicines","basicmedicines","combatstabilisers" } },
            { "Metals", new string[] { "aluminium","copper","steel","titanium" } },
            { "Technology", new string[] { "advancedcatalysers","bioreducinglichen","computercomponents","hazardousenvironmentsuits","landenrichmentsystems","medicaldiagnosticequipment","microcontrollers","muonimager","resonatingseparators","robotics","structuralregulators" } },
            { "Textiles", new string[] { "militarygradefabrics" } },
            { "Waste", new string[] { "biowaste" } },
            { "Weapons", new string[] { "battleweapons","nonlethalweapons","reactivearmour" } },
        };

        public static string getTypeForCargo(string name)
        {
            return mapCargoType.Keys.FirstOrDefault(key => mapCargoType[key].Contains(name))!;
        }
    }
}
