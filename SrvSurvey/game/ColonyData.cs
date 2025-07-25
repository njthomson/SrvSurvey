using Newtonsoft.Json;
using SrvSurvey.plotters;

namespace SrvSurvey.game
{
    class ColonyData : Data
    {
        public static Project? localUntrackedProject;

        public static string SystemColonisationShip = "System Colonisation Ship";
        public static string ExtPanelColonisationShip = "$EXT_PANEL_ColonisationShip";
        public static string PlanetaryConstructionSite = "Planetary Construction Site:";
        public static string OrbitalConstructionSite = "Orbital Construction Site:";

        public static bool isConstructionSite(Docked? entry)
        {
            return entry != null && isConstructionSite(entry.StationName, entry.StationServices);
        }

        public static bool isConstructionSite(string stationName, List<string>? stationServices)
        {
            return isConstructionSite(stationName) && stationServices != null && stationServices.Contains("colonisationcontribution") == true;
        }

        public static bool isConstructionSite(string stationName)
        {
            return stationName != null &&
            (
                stationName.StartsWith(PlanetaryConstructionSite, StringComparison.OrdinalIgnoreCase)
                || stationName.StartsWith(OrbitalConstructionSite, StringComparison.OrdinalIgnoreCase)
                || stationName.StartsWith(ExtPanelColonisationShip, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string getDefaultProjectName(Docked lastDocked)
        {
            var defaultName = lastDocked.StationName == ColonyData.SystemColonisationShip
                ? $"Primary port: {lastDocked.StarSystem}"
                : lastDocked.StationName
                    .Replace(ColonyData.ExtPanelColonisationShip + "; ", "")
                    .Replace(PlanetaryConstructionSite, "")
                    .Replace(OrbitalConstructionSite, "")
                    .Trim()
                ?? "";

            return defaultName;
        }

        public static ColonyData Load(string fid, string cmdr)
        {
            Game.log("ColonyData.Load (from disk)");
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
            Game.log("ColonyData.fetchLatest (from network)");
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
                    if (!string.IsNullOrEmpty(this.primaryBuildId) && getProject(this.primaryBuildId) == null)
                    {
                        Game.log($"Not found: primaryBuildId: {primaryBuildId} ?");
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
            if (localProject == null && marketId == ColonyData.localUntrackedProject?.marketId)
            {
                Game.log(diff.formatWithHeader($"Supplying commodities for untracked project: {systemAddress}/{marketId}", "\r\n\t"));
                localProject = ColonyData.localUntrackedProject;
            }

            if (localProject == null)
            {
                Game.log($"Why no project of any kind?");
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

            // update local untracked project, if it matches
            if (proj == null && entry.MarketID == ColonyData.localUntrackedProject?.marketId)
                proj = ColonyData.localUntrackedProject;

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

                    colonisationConstructionDepot = entry, // <-- temp for a few weeks (making up for lost time)
                };

                Game.colony.update(updateProj).continueOnMain(null, savedProj =>
                {
                    // update in-memory track
                    var idx = this.projects.FindIndex(p => p.buildId == savedProj.buildId);
                    if (idx >= 0)
                    {
                        this.projects[idx] = savedProj;
                        this.Save();
                    }
                    else
                    {
                        // we need to re-fetch everything as the cmdr should now have been added to the project
                        this.fetchLatest().justDoIt();
                    }

                    Game.log(savedProj);
                }).justDoIt(() =>
                {
                    Program.getPlotter<PlotBuildCommodities>()?.endPending();
                });
            }
        }

        public async Task updateComplete(ColonisationConstructionDepot entry)
        {
            Game.log($"Is completed marketId: {entry.MarketID} known?");
            var match = this.projects.Find(p => p.marketId == entry.MarketID);
            if (match?.complete == false)
            {
                await Game.colony.markComplete(match.buildId);
                match.complete = true;
                this.Save();
                Program.closePlotter<PlotBuildCommodities>();
                await this.fetchLatest();
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
            foreach (var entry in marketFile.Items)
            {
                var key = entry.Name.Substring(1).Replace("_name;", "");
                if (entry.Producer && (!fc.cargo.ContainsKey(key) || fc.cargo.GetValueOrDefault(key) != entry.Stock))
                {
                    newCargo[key] = entry.Stock;
                }
                else if (!entry.Producer && !entry.Consumer && fc.cargo.GetValueOrDefault(key) > 0)
                {
                    // I think this means the item could be purchased but it ran out of stock
                    newCargo[key] = entry.Stock;
                }
            }

            if (newCargo.Count > 0)
            {
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

        /** Prior mistakes and corrections in the cargo names:
         * 
         *  From > To
         * 
         *  microbialfurnaces > heliostaticfurnaces
         *  landenrichmentsystems > terrainenrichmentsystems
         *  muonimager > mutomimager
         *  combatstabilizers > combatstabilisers
         * 
         **/

        public static Dictionary<string, string[]> mapCargoType = new Dictionary<string, string[]>()
        {
            { "Chemicals", new string[] { "liquidoxygen","pesticides","surfacestabilisers","water" } },
            { "Consumer Items", new string[] { "evacuationshelter","survivalequipment","beer","liquor","wine" } },
            { "Foods", new string[] { "animalmeat","coffee","fish","foodcartridges","fruitandvegetables","grain","tea" } },
            { "Industrial Materials", new string[] { "ceramiccomposites","cmmcomposite","insulatingmembrane","polymers","semiconductors","superconductors" } },
            { "Machinery", new string[] { "buildingfabricators","cropharvesters","emergencypowercells","geologicalequipment", "microbialfurnaces", "heliostaticfurnaces", "mineralextractors","powergenerators","thermalcoolingunits","waterpurifiers" } },
            { "Medicines", new string[] { "agriculturalmedicines","basicmedicines","combatstabilisers", "combatstabilizers" } },
            { "Metals", new string[] { "aluminium","copper","steel","titanium" } },
            { "Technology", new string[] { "advancedcatalysers","bioreducinglichen","computercomponents","hazardousenvironmentsuits","landenrichmentsystems", "terrainenrichmentsystems", "medicaldiagnosticequipment","microcontrollers", "muonimager", "mutomimager", "resonatingseparators","robotics","structuralregulators" } },
            { "Textiles", new string[] { "militarygradefabrics" } },
            { "Waste", new string[] { "biowaste" } },
            { "Weapons", new string[] { "battleweapons","nonlethalweapons","reactivearmour" } },
        };

        public static string getTypeForCargo(string name)
        {
            return mapCargoType.Keys.FirstOrDefault(key => mapCargoType[key].Contains(name))!;
        }

        public static string matchByCargo(Dictionary<string, int> cargo)
        {
            var mapFinal = new Dictionary<string, double>();
            foreach (var name in cargo.Keys) mapFinal.init(name);

            var allCosts = Game.colony.loadDefaultCosts();
            foreach (var site in allCosts)
            {
                var map = new Dictionary<string, double>();
                foreach (var name in cargo.Keys) map.init(name);
                foreach (var name in site.cargo.Keys) map.init(name);

                var sum1 = 0d;
                foreach (var name in map.Keys)
                {
                    var cargoNeed = cargo.GetValueOrDefault(name);
                    var siteNeed = site.cargo.GetValueOrDefault(name);
                    var n1 = siteNeed - cargoNeed;
                    var n2 = n1 * n1;
                    sum1 += n2;
                }
                var sum2 = Math.Sqrt(sum1);

                //var vector = Math.Sqrt(site.cargo.Values.Sum(n => (double)(n * n)));
                Game.log($"{site.buildType}: {sum2}");
                mapFinal[site.buildType] = sum2;
            }

            var matches = mapFinal
                .Where(kv => kv.Value > 0)
                .OrderBy(kv => kv.Value);

            Game.log(matches.Take(5).Select(kv => $"{kv.Key}: {kv.Value}").formatWithHeader($"matchByCargo: top 5"));
            var bestMatch = matches.First();
            return bestMatch.Key;
        }
    }
}
