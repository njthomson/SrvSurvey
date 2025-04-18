using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Diagnostics;

namespace SrvSurvey.plotters
{
    [ApproxSize(200, 400)]
    internal class PlotBuildCommodities : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            // show in any mode, so long as we have some messages so show
            get => Game.settings.buildProjects_TEST
                && Game.activeGame?.cmdrColony?.projects.Count > 0
                && (
                    (PlotBuildCommodities.forceShow && !Game.activeGame.fsdJumping)
                    || (Game.activeGame.isMode(GameMode.StationServices)
                        && (Game.activeGame.marketEventSeen || Game.activeGame.cmdrColony.has(Game.activeGame.lastDocked))
                    )
                    || (ColonyData.isConstructionSite(Game.activeGame.lastDocked))
                )
                ;
        }

        public static void showButCleanFirst()
        {
            var form = Program.getPlotter<PlotBuildCommodities>();
            if (form != null && PlotBuildCommodities.forceShow)
            {
                form.setHeaderTextAndNeeds();
            }
            else
            {
                // just show the plotter
                Program.showPlotter<PlotBuildCommodities>();
            }
        }

        /// <summary> When true, makes the plotter become visible </summary>
        public static bool forceShow = false;

        private static Brush brushBackgroundStripe = new SolidBrush(Color.FromArgb(255, 12, 12, 12));
        private static Size szBigNumbers;

        private int pendingUpdates = 0;
        private Dictionary<string, int> pendingDiff = new();

        private string? headerText;
        private ColonyData.Needs? needs;
        private HashSet<string> projectNames = new();

        private PlotBuildCommodities() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.gothic_10;

            if (szBigNumbers == Size.Empty)
                szBigNumbers = TextRenderer.MeasureText(123456.ToString("N0"), this.Font, Size.Empty);
        }

        public override bool allow { get => PlotBuildCommodities.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(500);
            this.Height = scaled(1000);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        public void startPending(Dictionary<string, int>? diff = null)
        {
            this.pendingUpdates++;
            if (diff != null) this.pendingDiff = diff;

            if (!this.IsDisposed) this.Invalidate();
        }

        public void endPending()
        {
            this.pendingUpdates--;
            if (this.pendingUpdates < 0) this.pendingUpdates = 0;

            if (!this.IsDisposed)
            {
                this.Invalidate();
                this.setHeaderTextAndNeeds();
            }
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
            if (this.IsDisposed || colonyData == null || game.systemData == null) return;

            var effectiveAddress = -1L;
            var effectiveMarketId = -1L;
            List<Project>? relevantProjects = null;

            if (game.lastDocked != null && dockedAtConstructionSite)
            {
                effectiveAddress = game.lastDocked.SystemAddress;
                effectiveMarketId = game.lastDocked.MarketID;

                // use project name, or default name if not tracked
                var proj = game.cmdrColony.getProject(game.lastDocked.SystemAddress, game.lastDocked.MarketID);
                if (proj != null)
                    headerText = $"{proj.buildName} ({proj.buildType})";
                else
                    headerText = ColonyData.getDefaultProjectName(game.lastDocked);
                relevantProjects = new();
            }
            else if (!string.IsNullOrWhiteSpace(colonyData.primaryBuildId))
            {
                var primaryProject = colonyData.getProject(colonyData.primaryBuildId);
                if (primaryProject != null)
                {
                    relevantProjects = new() { primaryProject };
                    headerText = $"{primaryProject.buildName} ({primaryProject.buildType})";
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
            Game.log($"Get effective needs from: {effectiveAddress} / {effectiveMarketId}");
            this.needs = colonyData.getLocalNeeds(effectiveAddress, effectiveMarketId);
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            var colonyData = game.cmdrColony;
            if (this.IsDisposed || colonyData == null || game.systemData == null) return;

            var localMarketId = game.lastDocked?.MarketID ?? -1;
            var dockedAtConstructionSite = ColonyData.isConstructionSite(game.lastDocked);

            // calculate header text and needs only when needed
            if (this.headerText == null || this.needs == null || ((projectNames.Count > 0) == dockedAtConstructionSite))
                this.setHeaderTextAndNeeds(dockedAtConstructionSite);

            // exit early if we failed to populate things correctly
            if (this.headerText == null || this.needs == null)
            {
                Debugger.Break();
                drawTextAt2(ten, "Oops!", GameColors.Fonts.gothic_12B);
                newLine(+ten, true);
                return;
            }


            // start rendering...
            drawTextAt2(ten, headerText, GameColors.Fonts.gothic_12B);
            newLine(+ten, true);

            if (projectNames.Any())
            {
                // show relevant project names
                foreach (var name in projectNames)
                {
                    drawTextAt2(twenty, "► " + name);
                    newLine(true);
                }
                dty += ten;
            }

            // show warning if docked at an untracked FC
            if (dockedAtConstructionSite)
            {
                drawTextAt2(ten, "🚧 Docked at construction site", C.yellow, GameColors.Fonts.gothic_10);
                newLine(+ten, true);
            }

            // show warning if docked at untracked project
            if (dockedAtConstructionSite && !colonyData.has(game.lastDocked))
            {
                var msg = ColonyData.localUntrackedProject == null
                    ? "⚠️ Untracked project"
                    : "⚠️ Not a member of this project";
                drawTextAt2(ten, msg, C.yellow, GameColors.Fonts.gothic_10);
                newLine(+ten, true);
            }

            // show warning if docked at an untracked FC
            if (game.lastDocked?.StationEconomy == "$economy_Carrier;" && !game.cmdrColony.linkedFCs.ContainsKey(game.lastDocked.MarketID))
            {
                drawTextAt2(ten, "⚠️ Untracked Fleet Carrier", C.yellow, GameColors.Fonts.gothic_10);
                newLine(+ten, true);
            }

            // prep 3 columns: first zero width (that will grow), last 2 large enough for a big number
            var hasPin = needs!.assigned.Count > 0;
            var haveAnyCargo = game.cargoFile.Inventory.Count > 0;
            var showFCs = Game.settings.buildProjectsShowSumFC_TEST && colonyData.sumCargoLinkedFCs.Count > 0;
            if (showFCs && Game.settings.buildProjectsInlineSumFC_TEST) haveAnyCargo |= colonyData.sumCargoLinkedFCs.Count > 0;

            var rightIndent = haveAnyCargo ? szBigNumbers.Width : 0;
            var pinWidth = hasPin ? oneSix : 0;
            var columns = new Dictionary<int, float>() {
                { 0, 0 },           // name of the commodity
                { 1, rightIndent }, // Need column
                { 2, rightIndent }, // FCs column
                { 3, rightIndent }, // Have column
            };

            // render list headers: <commodity name>, <need>, <have>
            var xNeed = this.Width - eight - rightIndent - pinWidth;
            var xFC = 0f;
            if (showFCs)
            {
                if (Game.settings.buildProjectsInlineSumFC_TEST)
                {
                    xFC = this.Width - eight;
                }
                else
                {
                    xFC = xNeed;
                    xNeed = xFC - szBigNumbers.Width;
                }
            }

            drawTextAt2(xNeed, "Need", C.orangeDark, null, true).widestColumn(1, columns);
            if (showFCs && !Game.settings.buildProjectsInlineSumFC_TEST) drawTextAt2(xFC, "FCs", C.orangeDark, null, true).widestColumn(2, columns);
            if (haveAnyCargo) drawTextAt2(this.Width - eight, Game.settings.buildProjectsInlineSumFC_TEST ? "Have" : "Ship", C.orangeDark, null, true).widestColumn(3, columns);
            drawTextAt2(ten, "Commodity", C.orangeDark, null).widestColumn(0, columns);
            newLine(true);

            // draw cargo needs depending where we are
            var sumNeed = 0;
            if (dockedAtConstructionSite)
            {
                drawNeedsAlpha(needs, columns, rightIndent, xNeed, xFC);
                sumNeed = game.lastColonisationConstructionDepot?.ResourcesRequired.Sum(r => r.RequiredAmount - r.ProvidedAmount) ?? 0;
            }
            else
            {
                drawNeedsGrouped(needs, columns, rightIndent, xNeed, xFC);
                sumNeed = needs.commodities.Values.Sum();
            }

            // grow size of this plotter?
            var formWidthNeeded = columns.Values.Sum() + pinWidth + ten;
            if (this.formSize.Width < formWidthNeeded)
                this.formSize.Width = formWidthNeeded;

            // show how many runs remaining
            dty += ten;

            drawTextAt2(ten, $"► {sumNeed:N0} remaining");
            newLine(true);
            if (game.currentShip.cargoCapacity > 0)
            {
                var tripsNeeded = Math.Ceiling(sumNeed / (double)game.currentShip.cargoCapacity);
                drawTextAt2(ten, $"► {tripsNeeded:N0} trips in this ship");
                newLine(true);
            }

            // show footer if we are actively updating against the service
            if (this.pendingUpdates > 0)
            {
                dty += six;
                drawTextAt2(ten, "► Updating...", C.cyan, GameColors.Fonts.gothic_10B);
                newLine(+ten, true);
            }

            if (hasPin)
            {
                dty += eight;
                drawTextAt2(ten, "📌 Assigned commodities", C.orangeDark);
                newLine(true);
            }

            this.formAdjustSize(+ten, +ten);
        }

        private void drawNeedsAlpha(ColonyData.Needs needs, Dictionary<int, float> columns, float rightIndent, float xNeed, float xFC)
        {
            // alpha sort commodity names if at construction site
            var commodityNames = needs.commodities.Keys.Order();
            var required = game.lastColonisationConstructionDepot?.ResourcesRequired;
            if (required == null) return;

            var flip = false;
            foreach (var item in required)
            {
                var name = item.Name.Substring(1).Replace("_name;", "");
                var needCount = item.RequiredAmount - item.ProvidedAmount;
                if (needCount == 0) continue;

                var needTxt = needCount.ToString("N0");
                var nameTxt = item.Name_Localised;

                if (flip) g.FillRectangle(brushBackgroundStripe, four, dty - one, this.Width - eight, szBigNumbers.Height + one);
                flip = !flip;

                var col = C.orange;
                var ff = GameColors.Fonts.gothic_10;

                var cargoCount = game.cargoFile.getCount(name);

                if (pendingUpdates > 0 && pendingDiff.ContainsKey(name))
                {
                    // highlight what we just supplied
                    ff = GameColors.Fonts.gothic_10B;
                    col = C.cyan;
                    nameTxt = "► " + name;
                    needTxt = "...";
                }
                else if (cargoCount > 0)
                {
                    // highlight things in cargo hold
                    col = C.cyan;
                }

                /*if (needCount == 0)
                {
                    col = C.orangeDark;
                    nameTxt += " ✔️";
                }
                else*/
                if (cargoCount > needCount)
                {
                    // warn if we have more than needed
                    col = C.green;
                    nameTxt += " ✋";
                }
                else if (cargoCount == needCount)
                {
                    col = C.green;
                    nameTxt += " ✔️";
                }

                // render needed count
                var inHold = cargoCount > 0;
                drawTextAt2(xNeed, needTxt, col, ff, true)
                    .widestColumn(1, columns);

                // render cargo count
                if (cargoCount > 0)
                {
                    drawTextAt2(this.Width - eight, cargoCount.ToString("N0"), col, ff, true)
                        .widestColumn(3, columns);
                }

                // (skip FC numbers if sharing 2nd column and we already rendered there)
                if (xFC > 0 && Game.settings.buildProjectsShowSumFC_TEST && (!Game.settings.buildProjectsInlineSumFC_TEST || cargoCount > 0))
                {
                    // show amount on all FCs in same column?
                    var fcAmount = game.cmdrColony.sumCargoLinkedFCs.GetValueOrDefault(name, -1);
                    if (fcAmount > -1)
                    {
                        if (Game.settings.buildProjectsShowSumFCDelta_TEST)
                        {
                            // show delta amount?
                            var diff = (fcAmount - needCount);
                            var diffTxt = diff.ToString("N0");
                            if (diffTxt[0] != '-' && diffTxt[0] != '0') diffTxt = '+' + diffTxt;
                            var cc = diff > 0 ? C.green : C.red;

                            // if sharing a column ... make FC counts darker
                            if (Game.settings.buildProjectsInlineSumFC_TEST)
                                cc = cc = diff > 0 ? C.greenDark : C.redDark;

                            drawTextAt2(xFC, diffTxt, cc, ff, true)
                                .widestColumn(2, columns);
                        }
                        else
                        {
                            // show sum total
                            var diff = fcAmount;
                            var diffTxt = diff.ToString("N0");
                            var cc = fcAmount > needCount ? C.green : C.red;

                            // if sharing a column ... make FC counts darker
                            if (Game.settings.buildProjectsInlineSumFC_TEST)
                                cc = fcAmount > needCount ? C.greenDark : C.redDark;

                            drawTextAt2(xFC, diffTxt, cc, ff, true)
                                .widestColumn(2, columns);
                        }
                    }
                }

                // warn if we have more than needed
                //if (cargoCount > needCount) col = C.red;

                // render the name
                var sz2 = drawTextAt2(ten, nameTxt, col, ff)
                    .widestColumn(0, columns);

                // draw assigned pin behind the need number
                if (needs.assigned.Contains(name))
                    drawTextAt2(xNeed, "📌", col, ff);

                dtx = this.Width - twenty;

                newLine(+one, true);
            }
        }

        private void drawNeedsGrouped(ColonyData.Needs needs, Dictionary<int, float> columns, float rightIndent, float xNeed, float xFC)
        {
            var localMarketItems = game.marketFile.Items
                .Where(i => i.Stock > 0)
                .Select(_ => _.Name.Substring(1).Replace("_name;", ""))
                .ToHashSet();

            foreach (var cargoType in ColonyData.mapCargoType.Keys)
            {
                // skip types with nothing needed
                var sum = ColonyData.mapCargoType[cargoType].Sum(name => needs.commodities.GetValueOrDefault(name));
                if (sum == 0) continue;

                // render the type name - with a line to the right
                var sz1 = drawTextAt2(ten, cargoType, C.orangeDark, GameColors.Fonts.gothic_10);
                var lx = (int)dtx + eight;
                var ly = (int)dty + one + Util.centerIn(szBigNumbers.Height, 2);
                g.DrawLine(C.Pens.orangeDark2, lx, ly, this.Width - four, ly);
                newLine(true);

                // then each commodity in the type
                var flip = true;
                foreach (var name in ColonyData.mapCargoType[cargoType])
                {
                    // skip anything not needed
                    var needCount = needs.commodities.GetValueOrDefault(name);
                    if (needCount == 0) continue;
                    //if (!needs.commodities.ContainsKey(name)) continue;

                    if (flip) g.FillRectangle(brushBackgroundStripe, four, dty - one, this.Width - eight, szBigNumbers.Height + one);
                    flip = !flip;

                    var needTxt = needCount.ToString("N0");
                    var nameTxt = name;

                    var cargoCount = game.cargoFile.getCount(name);

                    var col = C.orange;
                    var ff = GameColors.Fonts.gothic_10;

                    if (pendingUpdates > 0 && pendingDiff.ContainsKey(name))
                    {
                        // highlight what we just supplied
                        ff = GameColors.Fonts.gothic_10B;
                        col = C.cyan;
                        nameTxt = "► " + name;
                        needTxt = "...";
                    }
                    else if (cargoCount > 0)
                    {
                        // highlight things in cargo hold
                        col = C.cyan;
                    }

                    /*if (needCount == 0)
                    {
                        col = C.orangeDark;
                        nameTxt += " ✔️";
                    }
                    else*/
                    if (cargoCount > needCount)
                    {
                        // warn if we have more than needed
                        col = C.green;
                        nameTxt += " ✋";
                    }
                    else if (cargoCount == needCount)
                    {
                        col = C.green;
                        nameTxt += " ✔️";
                    }
                    else if (game.lastDocked?.timestamp < game.marketFile.timestamp && localMarketItems.Count > 0 && !localMarketItems.Contains(name))
                    {
                        // make needed items missing in the current market red
                        if (cargoCount == 0) col = C.red;
                        nameTxt += " ❌";
                    }

                    // render needed count
                    var inHold = cargoCount > 0;
                    drawTextAt2(xNeed, needTxt, col, ff, true)
                        .widestColumn(1, columns);

                    // render cargo count
                    if (cargoCount > 0)
                    {
                        drawTextAt2(this.Width - eight, cargoCount.ToString("N0"), col, ff, true)
                            .widestColumn(3, columns);
                    }

                    // (skip FC numbers if sharing 2nd column and we already rendered there)
                    if (xFC > 0 && Game.settings.buildProjectsShowSumFC_TEST && (!Game.settings.buildProjectsInlineSumFC_TEST || cargoCount == 0))
                    {
                        // show amount on all FCs in same column?
                        var fcAmount = game.cmdrColony.sumCargoLinkedFCs.GetValueOrDefault(name, -1);
                        if (fcAmount > -1)
                        {
                            if (Game.settings.buildProjectsShowSumFCDelta_TEST)
                            {
                                // show delta amount?
                                var diff = (fcAmount - needCount);
                                var diffTxt = diff.ToString("N0");
                                if (diffTxt[0] != '-' && diffTxt[0] != '0') diffTxt = '+' + diffTxt;
                                var cc = diff > 0 ? C.green : C.red;

                                // if sharing a column ... make FC counts darker
                                if (Game.settings.buildProjectsInlineSumFC_TEST)
                                    cc = diff > 0 ? C.greenDark : C.redDark;

                                drawTextAt2(xFC, diffTxt, cc, ff, true)
                                    .widestColumn(2, columns);
                            }
                            else
                            {
                                // show sum total
                                var diff = fcAmount;
                                var diffTxt = diff.ToString("N0");
                                var cc = fcAmount > needCount ? C.green : C.red;

                                // if sharing a column ... make FC counts darker
                                if (Game.settings.buildProjectsInlineSumFC_TEST)
                                    cc = fcAmount > needCount ? C.greenDark : C.redDark;

                                drawTextAt2(xFC, diffTxt, cc, ff, true)
                                    .widestColumn(2, columns);
                            }
                        }
                    }

                    // draw assigned pin behind the need number
                    if (needs.assigned.Contains(name))
                        drawTextAt2(xNeed, "📌", col, ff);

                    // warn if we have more than needed
                    //if (cargoCount > needCount) col = C.red;

                    // render the name
                    var sz2 = drawTextAt2(twenty, nameTxt, col, ff)
                    .widestColumn(0, columns);

                    newLine(true);
                }
            }
        }

    }
}
