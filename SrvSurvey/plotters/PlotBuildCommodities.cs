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

            // if we are in a system to deliver supplies - show only those
            var effectiveAddress = -1L;
            var effectiveMarketId = -1L;

            if (!string.IsNullOrWhiteSpace(colonyData.primaryBuildId))
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
                    if (game.lastDocked?.StationName == ColonyData.SystemColonisationShip)
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

            var hasPin = drawNeeds(needs);

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

        private bool drawNeeds(ColonyData.Needs needs)
        {
            var hasLocalCargo = game.cargoFile.Inventory.Count > 0;
            var needIndent = hasLocalCargo ? szBigNumbers.Width : 0;

            // prep 3 columns of zero width
            var columns = new Dictionary<int, float>() { { 0, 0 }, { 1, needIndent }, { 2, needIndent } };

            // render list headers: need, have, commodity
            drawTextAt2(this.Width - eight - needIndent, "Need", C.orangeDark, null, true).widestColumn(1, columns);
            if (hasLocalCargo)
                drawTextAt2(this.Width - eight, "Have", C.orangeDark, null, true).widestColumn(2, columns);

            drawTextAt2(ten, "Commodity", C.orangeDark, null).widestColumn(0, columns);
            newLine(true);

            var localMarketItems = game.lastDocked == null || game.lastDocked.StationName == ColonyData.SystemColonisationShip || game.lastDocked.StationType == StationType.PlanetaryConstructionDepot
                ? new()
                : game.marketFile.Items
                    .Where(i => i.Stock > 0)
                    .Select(_ => _.Name.Substring(1).Replace("_name;", ""))
                    .ToHashSet();

            var flip = false;
            var hasPin = false;
            var sumNeed = 0;
            foreach (var (name, count) in needs.commodities)
            {
                if (flip) g.FillRectangle(brushBackgroundStripe, four, dty - one, this.Width - eight, szBigNumbers.Height + one);
                var col = C.orange;
                var ff = GameColors.Fonts.gothic_10;

                sumNeed += count;
                var nameTxt = name;
                var cargoCount = hasLocalCargo ? game.cargoFile.getCount(name) : 0;
                var inHold = game.cargoFile.Inventory.Find(i => i.Name == name) != null;

                if (needs.assigned.Contains(name))
                {
                    nameTxt = name + " 📌";
                    hasPin = true;
                }
                if (pendingDiff?.ContainsKey(name) == true)
                {
                    // highlight what we just supplied
                    ff = GameColors.Fonts.gothic_10B;
                    col = C.cyan;
                    nameTxt = "► " + name;
                }
                else if (inHold)
                {
                    // highlight things in cargo hold
                    col = C.cyan;
                }
                if (count == 0)
                {
                    col = C.orangeDark;
                    nameTxt += " ✔️";
                }
                else if (cargoCount > count)
                {
                    // warn if we have more than needed
                    col = C.red;
                    nameTxt += " ✋";
                }
                else if (cargoCount == count)
                {
                    col = C.green;
                    nameTxt += " ✔️";
                }
                else if (localMarketItems.Count > 0 && !localMarketItems.Contains(name))
                {
                    // make needed items missing in the current market dark red
                    col = C.redDark;
                    nameTxt += " ❌";
                }

                // render needed count
                drawTextAt2(this.Width - eight - needIndent, count.ToString("N0"), inHold ? C.cyan : col, ff, true)
                    .widestColumn(1, columns);

                // render cargo count
                if (cargoCount > 0)
                {
                    drawTextAt2(this.Width - eight, cargoCount.ToString("N0"), inHold ? C.cyan : col, ff, true)
                    .widestColumn(2, columns);
                }

                // render the name
                var sz2 = drawTextAt2(ten, nameTxt, col, ff)
                    .widestColumn(0, columns);

                if (count == 0 && false)
                    strikeThrough(dtx + four, dty + one + Util.centerIn(szBigNumbers.Height, 0), -sz2.Width - six, true);

                newLine(true);
                flip = !flip;
            }

            var formWidthNeeded = columns.Values.Sum();
            if (this.formSize.Width < formWidthNeeded)
                this.formSize.Width = formWidthNeeded;

            // show how many runs remaining
            dty += ten;

            drawTextAt2(ten, $"► {sumNeed:N0} remaining");
            newLine(true);
            if (game.currentShip.cargoCapacity > 0)
            {
                var foo = Math.Ceiling(sumNeed / (double)game.currentShip.cargoCapacity);
                drawTextAt2(ten, $"► {foo} trips in this ship");
                newLine(true);
            }

            return hasPin;
        }

    }
}
