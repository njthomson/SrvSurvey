using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotFSSInfo;

namespace SrvSurvey.plotters
{
    [ApproxSize(300, 400)]
    internal class PlotFSSInfo : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.activeGame?.cmdr != null
                && Game.settings.autoShowPlotFSSInfo
                && (
                    Game.activeGame.mode == GameMode.FSS
                    || (PlotFSSInfo.forceShow) // or a keystroke forced it
                    || (Game.activeGame.mode == GameMode.SystemMap && Game.settings.autoShowPlotFSSInfoInSystemMap && !PlotGuardianSystem.allowPlotter) // hide if Guardian plotter is open
                );
        }

        public static bool forceShow = false;

        public static List<FssEntry> scans = new List<FssEntry>();
        private static string? systemName;

        private PlotFSSInfo() : base()
        {
            this.Font = GameColors.fontSmall2;

            if (systemName != game?.systemData?.name)
            {
                // if we've switched systems, clear and add in any stars
                PlotFSSInfo.scans.Clear();
            }

            // add any bodies discovered whilst outside of FSS
            if (game?.systemData?.bodies.Count > 0)
            {
                var bodies = game.systemData.bodies.OrderBy(_ => _.shortName);
                foreach (var body in bodies)
                {
                    if (body.type == SystemBodyType.Asteroid || scans.Any(_ => _.body == body)) continue;
                    if (body.bioSignalCount == 0 && body.reward < Game.settings.hideFssLowValueAmount) continue;

                    var newScan = new FssEntry
                    {
                        body = body,
                        reward = body.reward,
                        dssReward = Util.GetBodyValue(body, true, false),
                    };
                    scans.Add(newScan);
                }
            }

            systemName = game?.systemData?.name;
        }

        public override bool allow { get => PlotFSSInfo.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            // Size set during paint

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onJournalEntry(Scan entry)
        {
            // ignore Belt Clusters
            if (entry.Bodyname.Contains("Belt Cluster") || game?.systemData == null)
                return;

            // allow time for the body to get created
            Application.DoEvents();
            if (game?.systemData == null) return;

            var body = game.systemData.bodies.Find(_ => _.id == entry.BodyID)!;
            var newScan = new FssEntry
            {
                body = body,
                reward = Util.GetBodyValue(entry, false),
                dssReward = Util.GetBodyValue(entry, true),
            };

            if (body.bioSignalCount == 0 && (body.geoSignalCount == 0 || Game.settings.hideGeoCountInFssInfo) && newScan.reward < Game.settings.hideFssLowValueAmount && newScan.dssReward < Game.settings.hideFssLowValueAmount) return;

            // show this body already present - pull it to the top
            var existingScan = scans.Find(s => s.body == body);
            if (existingScan != null)
                scans.Remove(existingScan);

            scans.Insert(0, newScan);

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || game?.systemData == null) return;

            this.resetPlotter(e.Graphics);

            // 1st line: system name
            var undiscoveredSystem = game.systemData.bodies.Find(b => b.isMainStar)?.wasDiscovered == false;
            var systemName = undiscoveredSystem
                ? $"⚑ {game.systemData.name}"
                : game.systemData.name;
            if (game.systemData.fssAllBodies) systemName += " ✔️";
            drawTextAt2(systemName, undiscoveredSystem ? GameColors.Cyan : null, GameColors.Fonts.gothic_12B);
            newLine(+eight, true);

            // 2nd line body count / values
            var bodyCount = game.systemData.bodies.Count(_ => _.scanned && _.type != SystemBodyType.Asteroid).ToString();
            if (game.systemData.fssAllBodies)
                drawTextAt2(eight, Res.ScannedAllBodies.format(bodyCount, Util.credits(game.systemData.sumRewards())));
            else
                drawTextAt2(eight, Res.ScannedBodies.format(bodyCount, Util.credits(game.systemData.sumRewards())));

            newLine(true);
            drawTextAt2(eight, Res.HidingBodies.format(Util.credits(Game.settings.hideFssLowValueAmount)), GameColors.OrangeDim);
            newLine(+eight, true);

            if (scans.Count == 0)
            {
                drawTextAt2(eight, Res.ScanToPopulate);
                newLine(true);
            }

            foreach (var scan in scans)
            {
                var hasBioSignals = scan.body.bioSignalCount > 0;

                // highlight if DSS value above threshold
                var dssWorthy = Game.settings.skipLowValueDSS && scan.dssReward > Game.settings.skipLowValueAmount;
                // unless it's too far away
                if (dssWorthy && Game.settings.skipHighDistanceDSS && scan.body.distanceFromArrivalLS > Game.settings.skipHighDistanceDSSValue)
                    dssWorthy = false;
                // or a gas giant
                if (dssWorthy && Game.settings.skipGasGiantDSS && scan.body.type == SystemBodyType.Giant)
                    dssWorthy = false;
                if (scan.body.type == SystemBodyType.Star)
                    dssWorthy = false;

                var highlight = dssWorthy | hasBioSignals;

                // 1st line: body name + type
                var planetClass = scan.body.planetClass?.Replace("Sudarsky c", "C");
                var prefix = scan.body.wasDiscovered ? "" : "⚑ ";
                var txt = $"{prefix}{scan.body.shortName} - {planetClass}"; // ◌◎◉☆★☄☼☀⛀⛃✔✨✶✪❓❔❓⛬❗❕ * ❒❱✪❍❌✋❖⟡⦁⦂⧫⇲
                var suffixes = new List<string>();
                if (scan.body.terraformable || scan.body.planetClass?.StartsWith("Earth") == true) suffixes.Add("🌎");
                if (scan.body.type == SystemBodyType.LandableBody) suffixes.Add("🚀");
                if (scan.body.firstFootFall) suffixes.Add("🦶");
                if (suffixes.Count > 0) txt += $" {string.Join(' ', suffixes)}";
                if (scan.body.type == SystemBodyType.Star)
                    txt = $"{prefix}{scan.body.shortName} - {scan.body.starType} Star";

                drawTextAt2(oneSix, txt, highlight ? GameColors.Cyan : null, GameColors.fontSmall2);
                newLine(true);

                // 2nd line: scan values
                var reward = (scan.body.dssComplete ? "✔️ " : "") + Util.credits(scan.body.reward, true); // scan.reward ?

                drawTextAt2(thirty, reward, dssWorthy ? GameColors.Cyan : null);
                if (scan.body.type != SystemBodyType.Star && !scan.body.dssComplete)
                {
                    drawTextAt2(" | ", GameColors.OrangeDim);
                    drawTextAt2(scan.dssReward.ToString("N0"), dssWorthy ? GameColors.Cyan : null);
                }

                if (scan.body.bioSignalCount > 0)
                {
                    drawTextAt2(" | ", GameColors.OrangeDim);
                    var analyzed = scan.body.countAnalyzedBioSignals == scan.body.bioSignalCount;
                    var sz = drawTextAt2(Res.CountGenus.format(scan.body.bioSignalCount), analyzed ? GameColors.Orange : GameColors.Cyan);
                    if (analyzed)
                        strikeThrough(dtx, dty + two + sz.Height / 2, -sz.Width, false);
                }

                if (!Game.settings.hideGeoCountInFssInfo && scan.body.geoSignalCount > 0)
                {
                    drawTextAt2(" | ", GameColors.OrangeDim);
                    var analyzed = scan.body.geoSignalNames.Count == scan.body.geoSignalCount;
                    var sz = drawTextAt2(Res.CountGeo.format(scan.body.geoSignalCount), analyzed ? GameColors.Orange : GameColors.Cyan);
                    if (analyzed)
                        strikeThrough(dtx, dty + two + sz.Height / 2, -sz.Width, false);

                }

                newLine(+four, true);

                var plotSysStatusTop = Program.getPlotter<PlotSysStatus>()?.Top;
                var plotBioSysTop = Program.getPlotter<PlotBioSystem>()?.Top;
                if (dty > plotSysStatusTop - hundred || dty > plotBioSysTop - hundred)
                    break;
            }

            drawTextAt2(eight, Res.Footer1, GameColors.OrangeDim);
            newLine(true);
            drawTextAt2(eight, Res.Footer2, GameColors.OrangeDim);
            newLine(true);

            this.formAdjustSize(+oneEight, +ten);
        }

    }

    class FssEntry
    {
        public SystemBody body;
        public long reward;
        public long dssReward;
    }
}
