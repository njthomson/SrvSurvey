using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.widgets;
using System.Diagnostics;

namespace SrvSurvey.plotters
{
    internal class PlotBuildCommodities : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotBuildCommodities),
            allowed = allowed,
            ctor = (game, def) => new PlotBuildCommodities(game, def),
            defaultSize = new Size(200, 400),
            invalidationJournalEvents = new() { nameof(Cargo), nameof(CargoTransfer), nameof(CollectCargo), nameof(EjectCargo), nameof(MarketSell), nameof(MarketBuy), },
        };

        public static bool allowed(Game game)
        {
            return Game.settings.buildProjects_TEST
                && Game.settings.autoShowPlotBuildCommodities
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && (
                    // forced but not jumping or in external panel
                    (PlotBuildCommodities.forceShow && !game.fsdJumping && !game.isMode(GameMode.ExternalPanel))
                    // in station services, there are some projects and we've entered a market since docking ... or docked site is a tracked build site
                    || (game.isMode(GameMode.StationServices)
                        && ((game.marketEventSeen && game.cmdrColony.projects.Any()) || game.cmdrColony.has(game.lastDocked)) // TODO: I think this is redundant as we will show at any construction site (tracked or otherwise)
                        )
                    // docked at a construction site and not in gal-map
                    || (ColonyData.isConstructionSite(game.lastDocked) && !game.isMode(GameMode.GalaxyMap))
                    // when docked on a Squadron FC and are within The Bank
                    || (game.musicTrack == "Squadrons" && game.lastDocked?.StationServices?.Contains("squadronBank") == true)
                    // setting allows, we have some projects and looking at right panel
                    || (Game.settings.buildProjectsOnRightScreen && game.isMode(GameMode.InternalPanel) && game.cmdrColony.projects.Any())
                )
                ;
        }

        #endregion

        public static void showButCleanFirst(Game game)
        {
            var form = PlotBase2.getPlotter<PlotBuildCommodities>();
            if (form != null && PlotBuildCommodities.forceShow)
            {
                form.setHeaderTextAndNeeds();
                form.invalidate();
            }
            else if (form == null)
            {
                // just show the plotter
                PlotBase2.add(game, PlotBuildCommodities.def);
            }
            else
            {
                form.show();
            }
        }

        /// <summary> When true, makes the plotter become visible </summary>
        public static bool forceShow = false;

        /// <summary> Manual toggle to force collapsing (or not) rows when FCs have enough </summary>
        public static bool toggleCollapse = false;

        private static Brush brushBackgroundStripe = new SolidBrush(Color.FromArgb(255, 12, 12, 12));
        private static Size szBigNumbers;

        private int pendingUpdates = 0;
        private Dictionary<string, int> pendingDiff = new();

        private string? headerText;
        private ColonyData.Needs? needs;
        private HashSet<string> projectNames = new();
        private List<FleetCarrier> cargoLinkedFCs = new();
        private Dictionary<string, int> sumCargoLinkedFCs = new();

        private PlotBuildCommodities(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.gothic_10;

            if (szBigNumbers == Size.Empty)
                szBigNumbers = TextRenderer.MeasureText(123456.ToString("N0"), this.font, Size.Empty);

            Properties.Commodities.ResourceManager.IgnoreCase = true;
            Properties.CommodityCategories.ResourceManager.IgnoreCase = true;
        }

        public static void startPending(Dictionary<string, int>? diff = null)
        {
            var plotter = PlotBase2.getPlotter<PlotBuildCommodities>();
            if (plotter == null) return;

            plotter.pendingUpdates++;
            plotter.pendingDiff = diff ?? new();

            plotter.invalidate();
        }

        public static void endPending()
        {
            var plotter = PlotBase2.getPlotter<PlotBuildCommodities>();
            if (plotter == null) return;

            plotter.pendingUpdates--;
            if (plotter.pendingUpdates < 0) plotter.pendingUpdates = 0;

            plotter.invalidate();
            plotter.setHeaderTextAndNeeds();
        }

        public void setHeaderTextAndNeeds()
        {
            var isDockedAtConstructionSite = Game.activeGame?.lastDocked != null
                && ColonyData.isConstructionSite(Game.activeGame.lastDocked)
                && Game.activeGame?.isMode(GameMode.StationServices) == true;

            setHeaderTextAndNeeds(isDockedAtConstructionSite);
        }

        private void setHeaderTextAndNeeds(bool dockedAtConstructionSite)
        {
            var colonyData = game.cmdrColony;
            if (colonyData == null || game.systemData == null) return;

            var effectiveAddress = -1L;
            var effectiveMarketId = -1L;
            List<Project>? relevantProjects = null;
            var relevantFCs = new HashSet<long>();

            if (game.lastDocked != null && dockedAtConstructionSite)
            {
                // docked at a construction site
                effectiveAddress = game.lastDocked.SystemAddress;
                effectiveMarketId = game.lastDocked.MarketID;

                // use project name, or default name if not tracked
                var proj = game.cmdrColony.getProject(game.lastDocked.SystemAddress, game.lastDocked.MarketID);
                if (proj != null)
                {
                    // is tracked
                    headerText = $"{proj.buildName} ({proj.buildType})";
                    // use only FCs linked to this project
                    relevantFCs = proj.linkedFC.Select(fc => fc.marketId).ToHashSet();
                }
                else
                {
                    // not tracked
                    headerText = ColonyData.getDefaultProjectName(game.lastDocked);
                    // use any and all FCs
                    relevantFCs = colonyData.linkedFCs.Keys.ToHashSet();
                }
                relevantProjects = new();
            }
            else if (!string.IsNullOrWhiteSpace(colonyData.primaryBuildId))
            {
                var primaryProject = colonyData.getProject(colonyData.primaryBuildId);
                if (primaryProject != null)
                {
                    relevantProjects = new() { primaryProject };
                    headerText = $"{primaryProject.buildName} ({primaryProject.buildType})";
                    relevantFCs = primaryProject.linkedFC.Select(fc => fc.marketId).ToHashSet();
                }
                else
                {
                    Game.log($"Why no matching primaryBuildId?");
                }
            }

            if (relevantProjects == null)
            {
                var localProjects = colonyData.projects.Where(p => p.systemAddress == game.systemData.address).ToList();
                if (localProjects.Any())
                {
                    // show only local projects, when there are some in the current system
                    effectiveAddress = localProjects.First().systemAddress;
                    relevantProjects = localProjects;
                    headerText = $"{localProjects.Count} local projects:";
                }
                else
                {
                    // otherwise show all projects anywhere
                    relevantProjects = colonyData.projects;
                    headerText = $"{relevantProjects.Count} total projects:";
                }

                // use only FCs linked to any of the projects
                relevantFCs = relevantProjects.SelectMany(p => p.linkedFC.Select(fc => fc.marketId)).ToHashSet();
            }

            projectNames.Clear();
            if (relevantProjects.Count == 1)
            {
                // if there is only 1 project - treat it as primary
                var onlyProject = relevantProjects.First();
                effectiveAddress = onlyProject.systemAddress;
                effectiveMarketId = onlyProject.marketId;
                headerText = $"{onlyProject.buildName} ({onlyProject.buildType})";

                // include system name if not local (and not already in the name)
                if (game.systemData.address != onlyProject.systemAddress && !onlyProject.buildName.Contains(onlyProject.systemName, StringComparison.OrdinalIgnoreCase)) headerText += $"\r\n     ► {onlyProject.systemName}";
            }
            else if (relevantProjects.Any())
            {
                projectNames = relevantProjects.Select(p => $"{p.buildName} ({p.buildType})").ToHashSet();
            }

            // calculate effective needs
            //Game.log($"Get effective needs from: {effectiveAddress} / {effectiveMarketId}");
            this.needs = colonyData.getLocalNeeds(effectiveAddress, effectiveMarketId);

            // sum cargo across related FCs and keep references to them
            this.cargoLinkedFCs = colonyData.linkedFCs.Values.Where(fc => relevantFCs.Contains(fc.marketId)).ToList();
            this.sumCargoLinkedFCs = ColonyData.getSumCargoFC(cargoLinkedFCs);
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var colonyData = game.cmdrColony;
            if (colonyData == null || game.systemData == null) return frame.Size;

            var localMarketId = game.lastDocked?.MarketID ?? -1;
            var dockedAtConstructionSite = ColonyData.isConstructionSite(game.lastDocked);
            var dockedAtSquadFC = game.lastDocked?.StationServices?.Contains("squadronBank") == true;

            // calculate header text and needs only when needed
            if (this.headerText == null || this.needs == null || (colonyData.primaryBuildId == null && ((projectNames.Count > 0) == dockedAtConstructionSite)))
                this.setHeaderTextAndNeeds(dockedAtConstructionSite);

            // exit early if we failed to populate things correctly
            if (this.headerText == null || this.needs == null)
            {
                Debugger.Break();
                tt.draw(N.ten, "Oops!", GameColors.Fonts.gothic_12B);
                tt.newLine(+N.ten, true);
                return frame.Size;
            }

            // start rendering...
            if (game.lastColonisationConstructionDepot?.ConstructionComplete == true)
            {
                headerText = game.lastDocked?.StationName ?? "";
                headerText = headerText.Substring(headerText.IndexOf(":") + 2);
                tt.draw("🚧  ", C.yellow, GameColors.Fonts.gothic_10);
                tt.draw(headerText, GameColors.Fonts.gothic_12B);
                tt.draw("  🚧", C.yellow, GameColors.Fonts.gothic_10);
                tt.newLine(+N.ten, true);

                tt.draw(N.ten, "☑️ Construction complete ☑️", C.green, GameColors.Fonts.gothic_10);
                tt.newLine(+N.ten, true);

                return tt.pad(+N.ten, +N.ten);
            }

            tt.dtx = 10;
            if (dockedAtConstructionSite) tt.draw("🚧  ", C.yellow, GameColors.Fonts.gothic_10);
            tt.draw(headerText, GameColors.Fonts.gothic_12B);
            if (dockedAtConstructionSite) tt.draw("  🚧", C.yellow, GameColors.Fonts.gothic_10);
            tt.newLine(+N.ten, true);

            if (projectNames.Any())
            {
                // show relevant project names
                foreach (var name in projectNames)
                {
                    tt.draw(N.twenty, "► " + name, GameColors.Fonts.gothic_9);
                    tt.newLine(true);
                }
                tt.dty += N.ten;
            }

            // show warning if docked at untracked project
            if (dockedAtConstructionSite && !colonyData.has(game.lastDocked))
            {
                var msg = ColonyData.localUntrackedProject == null
                    ? "Untracked project"
                    : "Not a member of this project";
                tt.draw(N.ten, "⚠️ ", C.yellow, GameColors.Fonts.gothic_10);
                tt.draw(msg, C.cyan, GameColors.Fonts.gothic_10);
                tt.draw(" ⚠️", C.yellow, GameColors.Fonts.gothic_10);
                tt.newLine(+N.ten, true);
            }

            // show warning if docked at an untracked FC
            if (game.lastDocked?.StationEconomy == "$economy_Carrier;" && !game.cmdrColony.linkedFCs.ContainsKey(game.lastDocked.MarketID))
            {
                tt.draw(N.ten, "⚠️ Untracked Fleet Carrier", C.yellow, GameColors.Fonts.gothic_10);
                tt.newLine(+N.ten, true);
            }

            // prep 3 columns: first zero width (that will grow), last 2 large enough for a big number
            var hasPin = needs!.assigned.Count > 0;
            var haveAnyCargo = game.cargoFile.Inventory.Count > 0;
            var showFCs = Game.settings.buildProjectsShowSumFC_TEST && this.sumCargoLinkedFCs.Count > 0;
            if (showFCs && Game.settings.buildProjectsInlineSumFC_TEST) haveAnyCargo |= this.sumCargoLinkedFCs.Count > 0;

            var rightIndent = szBigNumbers.Width;
            var pinWidth = hasPin ? N.oneSix : 0;
            var columns = new Dictionary<int, float>() {
                { 0, 0 },           // name of the commodity
                { 1, rightIndent }, // Need column
                { 2, showFCs ? rightIndent : 0 }, // FCs column
                { 3, haveAnyCargo ? rightIndent : 0 }, // Have column
            };

            // render list headers: <commodity name>, <need>, <have>
            var xNeed = this.width - N.eight - (haveAnyCargo ? rightIndent : 0) - pinWidth;
            var xFC = 0f;
            if (showFCs)
            {
                if (Game.settings.buildProjectsInlineSumFC_TEST)
                {
                    xFC = this.width - N.eight;
                }
                else
                {
                    xFC = xNeed;
                    xNeed = xFC - szBigNumbers.Width;
                }
            }

            tt.draw(xNeed, "Need", C.orangeDark, null, true).widestColumn(1, columns);
            if (showFCs && !Game.settings.buildProjectsInlineSumFC_TEST) tt.draw(xFC, $"{cargoLinkedFCs.Count} FCs", C.orangeDark, null, true).widestColumn(2, columns);
            if (haveAnyCargo) tt.draw(this.width - N.eight, Game.settings.buildProjectsInlineSumFC_TEST ? "Have" : "Ship", C.orangeDark, null, true).widestColumn(3, columns);
            tt.draw(N.ten, "Commodity", C.orangeDark, null).widestColumn(0, columns);
            tt.newLine(true);

            // draw cargo needs depending where we are
            if (dockedAtConstructionSite || dockedAtSquadFC)
                drawNeedsAlpha(g, tt, needs, columns, rightIndent, xNeed, xFC);
            else
                drawNeedsGrouped(g, tt, needs, columns, rightIndent, xNeed, xFC);

            // sum needs depending where we are
            var sumNeed = dockedAtConstructionSite
                ? game.lastColonisationConstructionDepot?.ResourcesRequired.Sum(r => r.RequiredAmount - r.ProvidedAmount) ?? 0
                : needs.commodities.Values.Sum();

            // grow size of this plotter?
            var formWidthNeeded = columns.Values.Sum() + pinWidth + N.twenty;
            tt.setMinWidth(formWidthNeeded);

            tt.dty += N.ten;

            // show relevant FCs
            if (cargoLinkedFCs.Count > 0)
            {
                tt.drawWrapped(N.ten, this.width, $"► {cargoLinkedFCs.Count} FCs:  " + string.Join("  ", cargoLinkedFCs.Select(fc => fc.name)));
                tt.newLine(true);
            }

            // show how many runs remaining
            var remainingTxt = $"► {sumNeed:N0} remaining";
            if (game.currentShip.cargoCapacity > 0)
            {
                var tripsNeeded = Math.Ceiling(sumNeed / (double)game.currentShip.cargoCapacity);
                remainingTxt += $" ► {tripsNeeded:N0} trips in this ship";
            }
            tt.draw(N.ten, remainingTxt);
            tt.newLine(true);

            // show footer if we are actively updating against the service
            if (this.pendingUpdates > 0)
            {
                tt.dty += N.six;
                tt.draw(N.ten, "► Updating...", C.cyan, GameColors.Fonts.gothic_10B);
                tt.newLine(+N.ten, true);
            }

            if (hasPin)
            {
                tt.dty += N.eight;
                tt.draw(N.ten, "📌 Assigned commodities", C.orangeDark);
                tt.newLine(true);
            }

            return tt.pad(+N.ten, +N.ten);
        }

        private void drawNeedsAlpha(Graphics g, TextCursor tt, ColonyData.Needs needs, Dictionary<int, float> columns, float rightIndent, float xNeed, float xFC)
        {
            // alpha sort commodity names if at construction site
            var commodityNames = needs.commodities.Keys.Order();
            var required = game.lastColonisationConstructionDepot == null
                ? needs.commodities
                : game.lastColonisationConstructionDepot?.ResourcesRequired
                    ?.ToDictionary(r => r.Name.Substring(1).Replace("_name;", ""), r => r.RequiredAmount - r.ProvidedAmount);

            if (required == null) return;

            var flip = false;
            foreach (var item in required)
            {
                var name = item.Key;
                var needCount = item.Value;
                if (needCount == 0) continue;

                var needTxt = needCount.ToString("N0");
                var nameTxt = Properties.Commodities.ResourceManager.GetString(name) ?? name;

                if (flip) g.FillRectangle(brushBackgroundStripe, N.four, tt.dty - N.one, this.width - N.eight, szBigNumbers.Height + N.one);
                flip = !flip;

                var col = C.orange;
                var ff = GameColors.Fonts.gothic_9;

                var cargoCount = game.cargoFile.getCount(name);
                var isPending = pendingUpdates > 0 && pendingDiff.ContainsKey(name);
                if (isPending)
                {
                    // highlight what we just supplied
                    ff = GameColors.Fonts.gothic_9B;
                    col = C.cyan;
                    needTxt = "...";
                }

                var shipHasEnough = cargoCount >= needCount;
                if (cargoCount > needCount)
                {
                    // warn if we have more than needed
                    col = C.green;
                    nameTxt += " ✋";
                }
                else if (cargoCount == needCount)
                {
                    col = C.green;
                }

                // render needed count
                tt.draw(xNeed, needTxt, col, ff, true)
                    .widestColumn(1, columns);

                // render SHIP cargo count
                if (cargoCount > 0)
                {
                    tt.draw(this.width - N.eight, cargoCount.ToString("N0"), col, ff, true)
                        .widestColumn(3, columns);
                }

                // (skip FC numbers if sharing 2nd column and we already rendered there)
                var fcHasEnough = false;
                if (xFC > 0 && Game.settings.buildProjectsShowSumFC_TEST && (!Game.settings.buildProjectsInlineSumFC_TEST || cargoCount > 0))
                {
                    // show amount on all FCs in same column?
                    var fcAmount = this.sumCargoLinkedFCs.GetValueOrDefault(name);
                    if (fcAmount >= 0)
                    {
                        if (Game.settings.buildProjectsShowSumFCDelta_TEST)
                        {
                            // show delta amount?
                            var diff = (fcAmount - needCount);
                            var diffTxt = diff.ToString("N0");
                            if (diffTxt[0] != '-' && diffTxt[0] != '0') diffTxt = '+' + diffTxt;
                            var cc = diff >= 0 ? C.green : C.red;

                            // if sharing a column ... make FC counts darker
                            if (Game.settings.buildProjectsInlineSumFC_TEST)
                                cc = cc = diff >= 0 ? C.greenDark : C.redDark;

                            if (isPending) { diffTxt = "..."; cc = C.cyan; }
                            tt.draw(xFC, diffTxt, cc, ff, true)
                                .widestColumn(2, columns);
                        }
                        else
                        {
                            // show sum total
                            var diff = fcAmount;
                            var diffTxt = diff.ToString("N0");
                            var cc = fcAmount >= needCount ? C.green : C.red;
                            fcHasEnough = fcAmount >= needCount;

                            // if sharing a column ... make FC counts darker
                            if (Game.settings.buildProjectsInlineSumFC_TEST)
                                cc = fcAmount >= needCount ? C.greenDark : C.redDark;
                            else if (fcAmount == 0)
                                cc = C.redDark;

                            if (isPending) { diffTxt = "..."; cc = C.cyan; }
                            tt.draw(xFC, diffTxt, cc, ff, true)
                                .widestColumn(2, columns);
                        }
                    }
                }

                // render the name
                var sz2 = tt.draw(N.twenty, nameTxt, col, ff)
                    .widestColumn(0, columns);

                if (isPending)
                    tt.draw(N.two, "►", C.cyan, ff);
                else if (shipHasEnough || fcHasEnough)
                    tt.draw(N.two, "✔️", shipHasEnough ? C.green : C.greenDark, ff);

                // draw assigned pin behind the need number
                if (needs.assigned.Contains(name))
                    tt.draw(xNeed, "📌", col, ff);

                tt.dtx = this.width - N.twenty;

                tt.newLine(+N.one, true);
            }
        }

        private void drawNeedsGrouped(Graphics g, TextCursor tt, ColonyData.Needs needs, Dictionary<int, float> columns, float rightIndent, float xNeed, float xFC)
        {
            var isDocked = game.lastDocked != null;
            var dockedAtLinkedFC = game.lastDocked != null && game.cmdrColony.linkedFCs.Keys.Contains(game.lastDocked.MarketID);
            var localMarketValid = game.marketFile.MarketId == game.lastDocked?.MarketID && game.marketFile.timestamp > game.lastDocked.timestamp;
            var localMarketItems = !localMarketValid ? new() : game.marketFile.Items
                .Where(i => i.Stock > 0)
                .Select(_ => _.Name.Substring(1).Replace("_name;", ""))
                .ToHashSet();

            var canCollapse = Game.settings.buildProjectsShowSumFC_TEST && !dockedAtLinkedFC && Game.settings.buildProjectsCollapseGroupsWithFCEnough_TEST != toggleCollapse;

            var cargoTypes = ColonyData.mapCargoType.Keys
                .OrderBy(cargoType => Properties.CommodityCategories.ResourceManager.GetString(cargoType) ?? cargoType, StringComparer.OrdinalIgnoreCase);

            foreach (var cargoType in cargoTypes)
            {
                // skip types with nothing needed
                var sum = ColonyData.mapCargoType[cargoType].Sum(name => needs.commodities.GetValueOrDefault(name));
                if (sum == 0) continue;

                // when allowed, see if we have enough of everything in this group
                var collapseGroup = canCollapse && ColonyData.mapCargoType[cargoType].All(name =>
                {
                    var needCount = needs.commodities.GetValueOrDefault(name);
                    var onFCs = this.sumCargoLinkedFCs.GetValueOrDefault(name);
                    var inShipCargo = game.cargoFile.Inventory.Any(inv => inv.Name == name);
                    return onFCs >= needCount && !inShipCargo;
                });

                // render the type name - with a line to the right
                var cargoTypeTxt = Properties.CommodityCategories.ResourceManager.GetString(cargoType) ?? cargoType;
                tt.draw(N.ten, cargoTypeTxt, collapseGroup ? C.greenDark : C.orangeDark, GameColors.Fonts.gothic_10);

                if (collapseGroup)
                    tt.draw("  ✔️", C.greenDark, GameColors.Fonts.gothic_10);

                var lx = (int)tt.dtx + N.eight;
                var ly = (int)tt.dty + N.one + Util.centerIn(szBigNumbers.Height, 2);
                g.DrawLine(C.Pens.orangeDark2, lx, ly, this.width - N.four, ly);
                tt.newLine(true);

                if (collapseGroup) continue;

                // then each commodity in the type
                var flip = true;

                // order the commodities by their translated name, with ordinal comparison to match the in-game sorting ("Équipement" comes after "Vêtements" with the game in French, go ask Frontier)
                var commodities = ColonyData.mapCargoType[cargoType]
                    .OrderBy(name => Properties.Commodities.ResourceManager.GetString(name) ?? name, StringComparer.OrdinalIgnoreCase);
                foreach (var name in commodities)
                {
                    // skip anything not needed
                    var needCount = needs.commodities.GetValueOrDefault(name);
                    if (needCount == 0) continue;
                    //if (!needs.commodities.ContainsKey(name)) continue;

                    if (flip) g.FillRectangle(brushBackgroundStripe, N.four, tt.dty - N.one, this.width - N.eight, szBigNumbers.Height + N.one);
                    flip = !flip;

                    var needTxt = needCount.ToString("N0");
                    var nameTxt = Properties.Commodities.ResourceManager.GetString(name) ?? name;

                    var haveEnough = false;

                    var cargoCount = game.cargoFile.getCount(name);

                    var col = C.orange;
                    var ff = GameColors.Fonts.gothic_9;
                    var inLocalMarket = localMarketValid && localMarketItems.Contains(name);

                    var isPending = pendingUpdates > 0 && pendingDiff.ContainsKey(name);
                    if (isPending)
                    {
                        // highlight what we just supplied
                        ff = GameColors.Fonts.gothic_9B;
                        col = C.cyan;
                        needTxt = "...";
                    }

                    if (cargoCount > needCount)
                    {
                        // warn if we have more than needed
                        col = C.green;
                        nameTxt += " ✋";
                    }
                    else if (cargoCount == needCount)
                    {
                        col = C.green;
                        haveEnough = true;
                    }
                    else if (isDocked && localMarketValid && localMarketItems.Count > 0 && !inLocalMarket)
                    {
                        // make needed items missing in the current market dimmer
                        col = C.orangeDark;
                    }

                    // render needed count
                    var inHold = cargoCount > 0;
                    tt.draw(xNeed, needTxt, col, ff, true)
                        .widestColumn(1, columns);

                    // render SHIP cargo count
                    if (cargoCount > 0)
                    {
                        tt.draw(this.width - N.eight, cargoCount.ToString("N0"), col, ff, true)
                            .widestColumn(3, columns);
                    }

                    var almost = false;
                    var colAlmost = C.yellow;

                    // (skip FC numbers if sharing 2nd column and we already rendered there)
                    if (xFC > 0 && Game.settings.buildProjectsShowSumFC_TEST && (!Game.settings.buildProjectsInlineSumFC_TEST || cargoCount == 0))
                    {
                        // show amount on all FCs in same column?
                        var fcAmount = this.sumCargoLinkedFCs.GetValueOrDefault(name, 0);
                        if (fcAmount >= 0)
                        {
                            var showDelta = Game.settings.buildProjectsShowSumFCDelta_TEST;

                            if (Game.settings.buildProjectsHighlightAlmostFC_TEST && isDocked && !this.cargoLinkedFCs.Any(fc => fc.marketId == game.lastDocked?.MarketID))
                            {
                                // highlight if we could get enough on our ship such that FCs then have enough
                                var deficit = needCount - fcAmount;
                                if (inLocalMarket && fcAmount < needCount && game.currentShip.cargoCapacity > deficit)
                                {
                                    showDelta = true;
                                    almost = true;
                                    if (cargoCount >= deficit) colAlmost = C.green;
                                    col = colAlmost;
                                }
                            }

                            if (showDelta)
                            {
                                // show delta amount?
                                var diff = (fcAmount - needCount);
                                var diffTxt = diff.ToString("N0");
                                if (diffTxt[0] != '-' && diffTxt[0] != '0') diffTxt = '+' + diffTxt;
                                var cc = diff >= 0 ? C.green : C.red;

                                // if sharing a column ... make FC counts darker
                                if (Game.settings.buildProjectsInlineSumFC_TEST)
                                    cc = diff >= 0 ? C.greenDark : C.redDark;

                                // highlight and make and bold if almost there
                                if (almost) cc = colAlmost;
                                if (isPending) { diffTxt = "..."; cc = C.cyan; }

                                tt.draw(xFC, diffTxt, cc, almost ? GameColors.Fonts.gothic_10B : ff, true)
                                    .widestColumn(2, columns);
                            }
                            else
                            {
                                // show sum total
                                var diff = fcAmount;
                                var diffTxt = diff.ToString("N0");
                                // if sharing a column ... make FC counts darker
                                var cc = Game.settings.buildProjectsInlineSumFC_TEST ? C.redDark : C.red;
                                if (fcAmount >= needCount)
                                {
                                    cc = Game.settings.buildProjectsInlineSumFC_TEST ? C.greenDark : C.green;
                                    haveEnough = true;
                                }

                                if (fcAmount == 0) cc = C.redDark;

                                if (isPending) { diffTxt = "..."; cc = C.cyan; }
                                tt.draw(xFC, diffTxt, cc, ff, true)
                                    .widestColumn(2, columns);
                            }

                            // keep the green check if we have enough on our ship and FCs
                            if (fcAmount + cargoCount >= needCount)
                                haveEnough = true;
                        }
                    }

                    // draw assigned pin behind the need number
                    if (needs.assigned.Contains(name))
                        tt.draw(xNeed, "📌", col, ff);

                    if (almost)
                        nameTxt += " 🏁";

                    // render the name
                    var sz2 = tt.draw(N.twenty, nameTxt, col, ff)
                            .widestColumn(0, columns);

                    if (isPending)
                        tt.draw(N.two, "►", C.cyan, ff);
                    else if (haveEnough && !nameTxt.EndsWith("❌"))
                        tt.draw(N.two, "✔️", col == C.green ? C.green : C.greenDark, ff);

                    tt.newLine(true);
                }
            }
        }

    }
}
