using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    [ApproxSize(200, 80)]
    internal class PlotBuildCommodities : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            // show in any mode, so long as we have some messages so show
            get => Game.settings.buildProjects_TEST
                && Game.activeGame?.cmdrColony?.projects.Count > 0
                && (
                    (PlotBuildCommodities.forceShow && !Game.activeGame.fsdJumping)
                    || (Game.activeGame.isMode(GameMode.StationServices) && (
                            Game.activeGame.marketEventSeen || Game.activeGame.cmdrColony.has(Game.activeGame.lastDocked))
                    )
                )
                ;
        }

        /// <summary> When true, makes the plotter become visible </summary>
        public static bool forceShow = false;

        private static Brush brushBackgroundStripe = new SolidBrush(Color.FromArgb(255, 12, 12, 12));
        private static Size szBigNumbers;

        public Dictionary<string, int>? pendingDiff;
        private string? localConstructionShipProjectTitle;

        private PlotBuildCommodities() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.gothic_10;

            if (szBigNumbers == Size.Empty)
                szBigNumbers = TextRenderer.MeasureText(12345.ToString("N0"), this.Font, Size.Empty);
        }

        public override bool allow { get => PlotBuildCommodities.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(300);
            this.Height = scaled(600);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            var colonyData = game.cmdrColony;
            if (this.IsDisposed || colonyData == null || game.systemData == null) return;

            var localMarketId = game.lastDocked?.MarketID ?? -1;
            var relevantProjects = colonyData.projects;
            var headerText = $"Commodities for {relevantProjects.Count} projects:";
            var projectNames = new HashSet<string>();
            var dockedAtConstructionSite = game.lastDocked?.StationServices?.Contains("colonisationcontribution") == true;

            // if we are in a system to deliver supplies - show only those
            var effectiveAddress = -1L;
            var effectiveMarketId = -1L;

            if (game.lastDocked != null && dockedAtConstructionSite)
            {
                var proj = game.cmdrColony.getProject(game.lastDocked.SystemAddress, game.lastDocked.MarketID);
                if (proj != null)
                {
                    effectiveAddress = proj.systemAddress;
                    effectiveMarketId = proj.marketId;
                    headerText = $"► {proj.buildName} ({proj.buildType})";
                }
            }
            else if (!string.IsNullOrWhiteSpace(colonyData.primaryBuildId))
            {
                var primaryProject = colonyData.getProject(colonyData.primaryBuildId);
                if (primaryProject != null)
                {
                    effectiveAddress = primaryProject.systemAddress;
                    effectiveMarketId = primaryProject.marketId;
                    headerText = $"► {primaryProject.buildName} ({primaryProject.buildType})\r\n     ► {primaryProject.systemName}";
                }
            }
            else
            {
                var localProjects = colonyData.projects.Where(p => p.systemAddress == game.systemData.address).ToList();
                if (localProjects.Any())
                {
                    // show name/type of projects in this system ...
                    projectNames = localProjects.Select((p, n) => $"{p.buildName} ({p.buildType} #{n})").ToHashSet();
                    headerText = "Local projects:";

                    // ... but if we are docked at a colonization ship - use the corresponding project name/type
                    if (ColonyData.isConstructionSite(game.lastDocked))
                    {
                        if (this.localConstructionShipProjectTitle == null)
                        {
                            // get name/type of projects in this system
                            var localProject = localProjects.FirstOrDefault(p => p.marketId == localMarketId);
                            if (localProject != null)
                            {
                                var idx = localProjects.IndexOf(localProject);
                                // remember this for future renders
                                if (idx < projectNames.Count)
                                    this.localConstructionShipProjectTitle = "► " + projectNames.ToList()[idx];
                            }
                        }

                        if (this.localConstructionShipProjectTitle != null)
                        {
                            headerText = this.localConstructionShipProjectTitle;
                            projectNames.Clear();
                            effectiveAddress = game.lastDocked?.SystemAddress ?? -1;
                            effectiveMarketId = localMarketId;
                        }
                    }
                    else
                    {
                        // TODO: sum for multiple local projects
                        effectiveAddress = game.lastDocked?.SystemAddress ?? -1;
                    }
                }
                else
                {
                    // show system names for all projects
                    projectNames = colonyData.projects.Select(p => $"{p.systemName}").ToHashSet();
                }
            }

            var needs = colonyData.getLocalNeeds(effectiveAddress, effectiveMarketId);


            // start rendering...
            drawTextAt2(ten, headerText, GameColors.Fonts.gothic_12B);
            newLine(+ten, true);

            // show relevant project names
            if (projectNames.Any())
            {
                foreach (var name in projectNames)
                {
                    drawTextAt2(twenty, "► " + name);
                    newLine(true);
                }
                dty += ten;
            }

            // prep 3 columns: first zero width (that will grow), last 2 large enough for a big number
            var hasPin = needs.assigned.Count > 0;
            var shipHasAnyCargo = game.cargoFile.Inventory.Count > 0;
            var rightIndent = shipHasAnyCargo ? szBigNumbers.Width : 0;
            var pinWidth = hasPin ? oneSix : 0;
            var columns = new Dictionary<int, float>() {
                { 0, 0 },
                { 1, rightIndent },
                { 2, rightIndent },
            };

            // render list headers: <commodity name>, <need>, <have>
            var xNeed = this.Width - eight - rightIndent - pinWidth;
            drawTextAt2(xNeed, "Need", C.orangeDark, null, true).widestColumn(1, columns);
            if (shipHasAnyCargo) drawTextAt2(this.Width - eight, "Have", C.orangeDark, null, true).widestColumn(2, columns);
            drawTextAt2(ten, "Commodity", C.orangeDark, null).widestColumn(0, columns);
            newLine(true);

            // draw cargo needs depending where we are
            if (dockedAtConstructionSite)
                drawNeedsAlpha(needs, columns, rightIndent, xNeed);
            else
                drawNeedsGrouped(needs, columns, rightIndent, xNeed);

            // grow size of this plotter?
            var formWidthNeeded = columns.Values.Sum() + pinWidth + ten;
            if (this.formSize.Width < formWidthNeeded)
                this.formSize.Width = formWidthNeeded;

            // show how many runs remaining
            dty += ten;

            var sumNeed = needs.commodities.Values.Sum();
            drawTextAt2(ten, $"► {sumNeed:N0} remaining");
            newLine(true);
            if (game.currentShip.cargoCapacity > 0)
            {
                var tripsNeeded = Math.Ceiling(sumNeed / (double)game.currentShip.cargoCapacity);
                drawTextAt2(ten, $"► {tripsNeeded} trips in this ship");
                newLine(true);
            }

            if (pendingDiff != null)
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

        private void drawNeedsAlpha(ColonyData.Needs needs, Dictionary<int, float> columns, float rightIndent, float xNeed)
        {
            // alpha sort commodity names if at construction site
            var commodityNames = needs.commodities.Keys.Order();

            var flip = false;
            foreach (var name in commodityNames)
            {
                var needCount = needs.commodities[name];
                if (needCount == 0) continue;

                var needTxt = needCount.ToString("N0");
                var nameTxt = name;

                if (flip) g.FillRectangle(brushBackgroundStripe, four, dty - one, this.Width - eight, szBigNumbers.Height + one);
                flip = !flip;

                var col = C.orange;
                var ff = GameColors.Fonts.gothic_10;

                var cargoCount = game.cargoFile.getCount(name);

                if (pendingDiff?.ContainsKey(name) == true)
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
                    .widestColumn(2, columns);
                }

                // warn if we have more than needed
                if (cargoCount > needCount) col = C.red;

                // render the name
                var sz2 = drawTextAt2(ten, nameTxt, col, ff)
                    .widestColumn(0, columns);

                // draw assigned pin behind the need number
                if (needs.assigned.Contains(name))
                    drawTextAt2(xNeed, "📌", col, ff);

                dtx = this.Width - 20;

                newLine(true);
            }
        }

        private void drawNeedsGrouped(ColonyData.Needs needs, Dictionary<int, float> columns, float rightIndent, float xNeed)
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


                    if (pendingDiff?.ContainsKey(name) == true)
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
                        .widestColumn(2, columns);
                    }

                    // draw assigned pin behind the need number
                    if (needs.assigned.Contains(name))
                        drawTextAt2(xNeed, "📌", col, ff);

                    // warn if we have more than needed
                    if (cargoCount > needCount) col = C.red;

                    // render the name
                    var sz2 = drawTextAt2(twenty, nameTxt, col, ff)
                    .widestColumn(0, columns);

                    newLine(true);
                }
            }
        }

    }
}
