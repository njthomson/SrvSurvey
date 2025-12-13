using Newtonsoft.Json;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.plotters;
using System.Diagnostics;

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
            var defaultName = lastDocked.StationName.StartsWith(ExtPanelColonisationShip, StringComparison.OrdinalIgnoreCase) || lastDocked.StationName == ColonyData.SystemColonisationShip
                ? $"Primary port"
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

            // save an update if Commander name has changed
            if (data.cmdr != cmdr)
            {
                Game.log($"Updating ColonyData cmdr: {data.cmdr} => {cmdr}");
                data.cmdr = cmdr;
                data.Save();
            }

            return data;
        }

        public async Task fetchLatest(string? buildId = null)
        {
            Game.log("ColonyData.fetchLatest (from network)");
            // Make plotter it render "updating..."
            PlotBuildCommodities.startPending();
            try
            {
                if (buildId == null)
                {
                    // fetch all ACTIVE projects and primaryBuildId
                    await Task.WhenAll(
                        Game.rcc.getPrimary(this.cmdr).continueOnMain(null, newPrimaryBuildId => this.primaryBuildId = newPrimaryBuildId, true),
                        Game.rcc.getCmdrActive(this.cmdr).continueOnMain(null, newProjects => this.projects = newProjects),
                        Game.rcc.getHiddenIDs(this.cmdr).continueOnMain(null, newHiddenIDs => this.hiddenIDs = newHiddenIDs)
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
                    var freshProject = await Game.rcc.getProject(buildId);
                    var idx = this.projects.FindIndex(p => p.buildId == buildId);
                    if (idx >= 0 && freshProject != null)
                        this.projects[idx] = freshProject;
                }

                // request data for all cmdr linked FCs
                var allFCs = await Game.rcc.getAllCmdrFCs(this.cmdr)!;
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
                PlotBuildCommodities.endPending();
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

        /// <summary> FCs linked to some project or directly to the CMDR</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<long, FleetCarrier> linkedFCs = new();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, int> sumCargoLinkedFCs = new();

        /// <summary> Build IDs that should not be rendered in the overlay </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> hiddenIDs = new();

        #endregion

        public List<Project> notHiddenProjects => this.projects.Where(p => !this.hiddenIDs.Contains(p.buildId)).ToList();

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

            var projs = this.projects
                .Where(p => (address == -1 || p.systemAddress == address) && (marketId == -1 || p.marketId == marketId))
                .ToArray();

            return getNeeds(projs);
        }

        public Needs getNeeds(params Project[] projs)
        {
            var needs = new Needs();

            foreach (var project in projs)
            {
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
                PlotBuildCommodities.startPending(diff);

                Game.rcc.contribute(localProject.buildId, this.cmdr, diff).continueOnMain(null, () =>
                {
                    // wait a bit then force plotter to re-render
                    Task.Delay(500).ContinueWith(t => PlotBuildCommodities.endPending());
                });
            }
        }

        public void checkConstructionSiteUponDocking(Docked entry, Game game)
        {
            var proj = this.getProject(entry.SystemAddress, entry.MarketID);
            if (proj == null)
            {
                PlotBuildCommodities.startPending();
                // it's possible someone else might be tracking it?
                Game.rcc.getProject(entry.SystemAddress, entry.MarketID).continueOnMain(null, otherProj =>
                {
                    Game.log($"Found local project untracked by cmdr: {otherProj?.buildName} ({otherProj?.buildId})");
                    ColonyData.localUntrackedProject = otherProj;

                    PlotBuildCommodities.endPending();
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
            var body = game.systemBody;
            if (body != null && (proj.bodyNum != body.id || proj.bodyName != body.name))
            {
                Game.log($"Auto-update bodyName/bodyNum: {proj.bodyName} => {body.name} / {proj.bodyNum} => {body.id}");
                updatedProject ??= new(proj.buildId);
                updatedProject.bodyName = body.name;
                updatedProject.bodyNum = body.id;
            }

            if (updatedProject != null)
            {
                PlotBuildCommodities.startPending();
                Game.rcc.updateProject(updatedProject).continueOnMain(null, () =>
                {
                    PlotBuildCommodities.endPending();
                });
            }

            if (PlotBuildCommodities.allowed(game))
                PlotBuildCommodities.showButCleanFirst(game);
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
                PlotBuildCommodities.startPending();
                var updateProj = new ProjectUpdate(proj.buildId)
                {
                    commodities = needed,
                    maxNeed = entry.ResourcesRequired.Sum(r => r.RequiredAmount),

                    colonisationConstructionDepot = entry, // <-- temp for a few weeks (making up for lost time)
                };

                Game.rcc.updateProject(updateProj).continueOnMain(null, savedProj =>
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
                    PlotBuildCommodities.endPending();
                });
            }
        }

        public async Task updateComplete(ColonisationConstructionDepot entry)
        {
            Game.log($"Is completed marketId: {entry.MarketID} known?");
            var match = this.projects.Find(p => p.marketId == entry.MarketID);
            if (match?.complete == false)
            {
                await Game.rcc.markComplete(match.buildId);
                match.complete = true;
                this.Save();
                PlotBase2.remove(PlotBuildCommodities.def);
                await this.fetchLatest();
            }
        }

        public async Task updateFromMarketFC(MarketFile marketFile)
        {
            if (!linkedFCs.ContainsKey(marketFile.MarketId)) return;

            PlotBuildCommodities.startPending();

            // fetch latest numbers first
            var fc = await Game.rcc.getFC(marketFile.MarketId);
            Game.log($"updateFromMarketFC: Checking FC cargo against market: {fc?.marketId} : {fc?.displayName} ({fc?.name})");
            if (fc == null)
            {
                PlotBuildCommodities.endPending();
                return;
            }

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
                var updatedCargo = await Game.rcc.updateCargoFC(fc.marketId, newCargo);
                if (updatedCargo != null)
                {
                    // apply new numbers and save
                    linkedFCs[fc.marketId].cargo = updatedCargo;

                    var sumCargo = getSumCargoFC(linkedFCs.Values);
                    this.sumCargoLinkedFCs = sumCargo;

                    this.Save();
                }
            }

            await Task.Delay(500);
            PlotBuildCommodities.endPending();
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
            { "Consumer Items", new string[] { "evacuationshelter","survivalequipment" } },
            { "Foods", new string[] { "animalmeat","coffee","fish","foodcartridges","fruitandvegetables","grain","tea" } },
            { "Industrial Materials", new string[] { "ceramiccomposites","cmmcomposite","insulatingmembrane","polymers","semiconductors","superconductors" } },
            { "Legal Drugs", new string[] { "beer","liquor","wine" } },
            { "Machinery", new string[] { "buildingfabricators","cropharvesters","emergencypowercells","geologicalequipment", "microbialfurnaces", "heliostaticfurnaces", "mineralextractors","powergenerators","thermalcoolingunits","waterpurifiers" } },
            { "Medicines", new string[] { "agriculturalmedicines","basicmedicines","combatstabilisers", "combatstabilizers" } },
            { "Metals", new string[] { "aluminium","copper","steel","titanium" } },
            { "Technology", new string[] { "advancedcatalysers", "autofabricators", "bioreducinglichen","computercomponents","hazardousenvironmentsuits","landenrichmentsystems", "terrainenrichmentsystems", "medicaldiagnosticequipment","microcontrollers", "muonimager", "mutomimager", "resonatingseparators","robotics","structuralregulators" } },
            { "Textiles", new string[] { "militarygradefabrics" } },
            { "Waste", new string[] { "biowaste" } },
            { "Weapons", new string[] { "battleweapons","nonlethalweapons","reactivearmour" } },
        };

        public static string matchByCargo(Dictionary<string, int> cargo)
        {
            var mapFinal = new Dictionary<string, double>();
            foreach (var name in cargo.Keys) mapFinal.init(name);

            var allCosts = Game.rcc.loadDefaultCosts();
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

        public static async Task<FleetCarrier?> publishFC(Game game)
        {
            if (game?.lastDocked?.StationType != StationType.FleetCarrier || game.journals == null) return null;

            // pull what we can from Docked, then find the display name
            var newFC = new FleetCarrier()
            {
                marketId = game.lastDocked.MarketID,
                name = game.lastDocked.StationName,
                displayName = "",
                cargo = null!, // we explicitly don't want an empty list here
            };

            // walk through journals until we find ReceiveText or FSSSignalDiscovered
            game.journals.walkDeep(true, entry =>
            {
                // match from station chatter?
                var receiveText = entry as ReceiveText;
                if (receiveText?.From.EndsWith(newFC.name) == true)
                {
                    newFC.displayName = receiveText.From.Replace($" {newFC.name}", "");
                    return true;
                }

                // match from FSSSignalDiscovered?
                var fssSignalDiscovered = entry as FSSSignalDiscovered;
                if (fssSignalDiscovered?.SignalType == "FleetCarrier" && fssSignalDiscovered?.SignalName.EndsWith(newFC.name) == true)
                {
                    newFC.displayName = fssSignalDiscovered.SignalName.Replace($" {newFC.name}", "");
                    return true;
                }

                return false;
            });

            if (newFC.displayName.EndsWith(" |"))
                newFC.displayName = newFC.displayName.Substring(0, newFC.displayName.Length - 2);

            Game.log($"Publishing FC: {newFC}");

            var fc = await Game.rcc.publishFC(newFC);

            // if market data matches this FC, update that too
            if (fc != null && game.marketFile.MarketId == fc.marketId)
                await game.cmdrColony.updateFromMarketFC(game.marketFile);

            return fc;
        }

        public static async Task<List<Bod>?> updateSysBodies(SystemData sys)
        {
            Game.log($"Colony.updateSysBodies: {sys.ToString()} ({sys.fssAllBodies})");
            if (sys?.fssAllBodies != true || Game.activeGame?.journals == null) return null;

            var bods = new List<Bod>();
            var tidalsNeeded = 0;
            foreach (var b in sys.bodies)
            {
                var bod = new Bod
                {
                    num = b.id,
                    name = b.name,
                    distLS = b.distanceFromArrivalLS,
                    parents = b.parents?.Select(bp => bp.id).ToList() ?? new(),

                    type = Bod.BodyType.un,
                    subType = Util.pascal(b.planetClass?.Replace("Sudarsky ", "")),
                    features = new(),
                };

                // map type
                if (b.name.Contains("barycentre"))
                {
                    bod.type = Bod.BodyType.bc;
                    bod.subType = null;
                }
                else if (b.type == SystemBodyType.Star)
                {
                    var starType = Util.flattenStarType(b.starType);
                    if (starType == "H") bod.type = Bod.BodyType.bh;
                    else if (starType == "NS") bod.type = Bod.BodyType.ns;
                    else if (starType == "D") bod.type = Bod.BodyType.bh;
                    else bod.type = Bod.BodyType.st;

                    bod.subType = getSubType(b);
                }
                else if (b.planetClass?.Contains("gas giant", StringComparison.OrdinalIgnoreCase) == true)
                {
                    bod.type = Bod.BodyType.gg;
                }
                else if (b.type == SystemBodyType.Asteroid)
                {
                    bod.type = Bod.BodyType.ac;
                }
                else
                {
                    tidalsNeeded++;
                    bod.type = Bod.mapBodyTypeFromPlanetClass.GetValueOrDefault(b.planetClass!, StringComparison.OrdinalIgnoreCase);
                }
                if (bod.type == Bod.BodyType.un)
                {
                    Game.log($"Why body type not known? {bod.name} => {b.planetClass}");
                    Debugger.Break();
                    return null;
                }

                // map features
                if (b.bioSignalCount > 0) bod.features.Add(BodyFeature.bio);
                if (b.geoSignalCount > 0) bod.features.Add(BodyFeature.geo);
                if (b.hasRings) bod.features.Add(BodyFeature.rings);
                if (!string.IsNullOrEmpty(b.volcanism) && b.volcanism != "No volcanism") bod.features.Add(BodyFeature.volcanism);
                if (b.terraformable) bod.features.Add(BodyFeature.terraformable);
                if (b.type == SystemBodyType.LandableBody) bod.features.Add(BodyFeature.landable);

                // as tidally locked is not stored, will must parse journals to find (done in a batch below)

                bods.Add(bod);
            }

            Game.log($"Parsing journals for {tidalsNeeded} tidally locked status");
            var countJournals = 0;
            Game.activeGame.journals.walkDeep(true, entry =>
            {
                var scan = entry as Scan;
                if (scan == null) return false;

                var match = bods.Find(b => b.name == scan.BodyName);
                if (match == null) { return false; }

                Game.log($"Found #{tidalsNeeded} '{scan.BodyName}' => {scan.TidalLock}");
                if (scan.TidalLock)
                    match.features.Add(BodyFeature.tidal);

                tidalsNeeded--;

                if (tidalsNeeded <= 0)
                    return true;
                else
                    return false;
            }, _ =>
            {
                // search through 100 journal files max
                countJournals++;
                if (countJournals < 100)
                    return false;
                else
                    return true;
            });


            Game.log($"{sys.address} / {sys.name}\n\n" + JsonConvert.SerializeObject(bods, Formatting.Indented));
            return await Game.rcc.updateSysBodies(sys.address, bods);
        }

        private static string getSubType(SystemBody b)
        {
            // TODO: Not really sure what qualified as giant vs super-giant or not...
            switch (b.starType)
            {
                case "A":
                    return b.radius < 82 ? "A (Blue-White) Star" : "A (Blue-White super giant) Star";
                case "B":
                    return b.radius < 450 ? "B (Blue-White) Star" : "B (Blue-White super giant) Star";
                case "H":
                    return "Black Hole";
                case "F":
                    return b.radius < 220 ? "F (White) Star" : "F (White super giant) Star";
                case "G":
                    return b.radius < 150 ? "G (White-Yellow) Star" : "G (White-Yellow super giant) Star";
                case "K":
                    return b.radius < 50 ? "K (Yellow-Orange) Star" : "K (Yellow-Orange giant) Star";
                case "L":
                    return "L (Brown dwarf) Star";
                case "MS":
                    return "MS-type Star";
                case "M":
                    return b.radius < 3.85m ? "M (Red dwarf) Star" : b.radius < 610m ? "M (Red giant) Star" : "M (Red super giant) Star";
                case "N":
                    return "Neutron Star";
                case "O":
                    return "O (Blue-White) Star";
                case "S":
                    return "S-type star";
                case "T":
                    return "T (Brown dwarf) Star";
                case "TTS":
                    return "T Tauri Star";
                case "Y":
                    return "Y (Brown dwarf) Star";
            }

            if (b.starType?.StartsWith("D") == true)
                return $"White Dwarf ({b.starType}) Star";

            if (b.starType?.StartsWith("W") == true)
                return $"Wolf-Rayet ({b.starType}) Star";

            Debugger.Break();
            return $"{b.starType} (?) Star"; // TODO: map the star type
        }

        public static async Task<bool> publishCurrentShip(Game game)
        {
            if (game?.cmdr?.rccApiKey == null)
            {
                Game.log($"ColonyData.publishCurrentShip: cannot publish - missing API Key");
                return false;
            }

            var ship = new CmdrCurrentShip()
            {
                cmdr = game.cmdr.commander,
                name = game.currentShip.name ?? game.currentShip.ident,
                type = game.currentShip.type,
                maxCargo = game.currentShip.cargoCapacity,
                cargo = game.cargoFile.Inventory.ToDictionary(_ => _.Name, _ => _.Count),
            };

            await Game.rcc.publishCurrentShip(ship);

            return true;
        }

    }
}
