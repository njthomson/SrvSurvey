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
                && Game.activeGame?.cmdr.colonyData.allCommodities.Count > 0
                && (
                    (Game.activeGame.isMode(GameMode.StationServices) && Game.activeGame.marketEventSeen)
                    || (PlotBodyInfo.forceShow && !Game.activeGame.fsdJumping)
                )
                ;
        }

        /// <summary> When true, makes the plotter become visible </summary>
        public static bool forceShow = false;

        public Dictionary<string, int>? pendingDiff;

        private PlotBuildCommodities() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.gothic_10;
        }

        public override bool allow { get => PlotBuildCommodities.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(100);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || game.cmdr.colonyData == null) return;

            drawTextAt2(ten, "Commodities needed:", GameColors.Fonts.gothic_12B);
            newLine(+ten, true);

            foreach(var project in game.cmdr.colonyData.projects)
            {
                drawTextAt2(twenty, "► " + project.systemName);
                newLine(true);
            }
            dty += ten;

            var w1 = ten + Util.maxWidth(this.Font, game.cmdr.colonyData.allCommodities.Keys.ToArray());
            var sz = TextRenderer.MeasureText("88888", this.Font, Size.Empty);

            //var indent = ten + w1 + w2;
            var bb = new SolidBrush(Color.FromArgb(255, 12, 12, 12));

            var flip = false;
            var hasPin = false;
            foreach (var (name, count) in game.cmdr.colonyData.allCommodities)
            {
                if (flip) g.FillRectangle(bb, four, dty - one, this.Width - eight, sz.Height + one);
                var col = C.orange;
                var ff = GameColors.Fonts.gothic_10;
                var nameTxt = name;
                if (game.cmdr.colonyData.assigned.ContainsKey(name))
                {
                    nameTxt = name + " 📌";
                    //col = C.cyan;
                    hasPin = true;
                }

                if (pendingDiff?.ContainsKey(name) == true)
                {
                    // highlight what we just supplied
                    ff = GameColors.Fonts.gothic_10B;
                    col = C.cyan;
                    nameTxt = "► " + name;
                }
                else if (game.cargoFile.Inventory.Find(i => i.Name == name) != null)
                {
                    // highlight things in cargo hold
                    col = C.cyan;
                }


                drawTextAt2(this.Width - 8, count.ToString("N0"), col, ff, true);
                drawTextAt2(ten, nameTxt, col, ff);
                newLine(true);

                flip = !flip;
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

            this.formAdjustSize(+ten + sz.Width, +ten);
        }

    }
}
