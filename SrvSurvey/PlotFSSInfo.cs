using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotFSSInfo : PlotBase, PlotterForm
    {
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

        public static bool allowPlotter
        {
            get => Game.activeGame?.cmdr != null
                && Game.settings.autoShowPlotFSSInfo
                && Game.activeGame.isMode(GameMode.FSS);
        }

        protected override void onJournalEntry(Scan entry)
        {
            // ignore Belt Clusters
            if (entry.Bodyname.Contains("Belt Cluster") || game?.systemData == null)
                return;
            // allow time for the body to get created
            Application.DoEvents();

            var body = game.systemData.bodies.Find(_ => _.id == entry.BodyID)!;
            var newScan = new FssEntry
            {
                body = body,
                reward = Util.GetBodyValue(entry, false),
                dssReward = Util.GetBodyValue(entry, true),
            };

            if (body.bioSignalCount == 0 && newScan.reward < Game.settings.hideFssLowValueAmount && newScan.dssReward < Game.settings.hideFssLowValueAmount) return;

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
                ? $"♦ {game.systemData.name}"
                : game.systemData.name;
            drawTextAt(systemName, undiscoveredSystem ? GameColors.brushCyan : null, GameColors.fontSmallBold);
            newLine(+eight, true);

            // 2nd line body count / values
            var bodyCount = game.systemData.bodies.Count(_ => _.scanned && _.type != SystemBodyType.Asteroid);
            drawTextAt(eight, $"Scanned {bodyCount} bodies: {Util.credits(game.systemData.sumRewards())}");
            newLine(true);
            drawTextAt(eight, $"( Hiding bodies < {Util.credits(Game.settings.hideFssLowValueAmount)} )", GameColors.brushGameOrangeDim);
            newLine(+eight, true);

            if (scans.Count == 0)
            {
                drawTextAt(eight, "(scan to populate)");
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
                var prefix = scan.body.wasDiscovered ? "" : "♦ ";
                var txt = $"{prefix}{scan.body.shortName} - {planetClass}"; // ◌◎◉☆★☄☼☀⛀⛃✔✨✶✪❓❔❓⛬❗❕ * ❒❱✪❍❌✋❖⟡⦁⦂⧫⇲
                var suffixes = new List<string>();
                if (scan.body.terraformable) suffixes.Add("T");
                if (scan.body.type == SystemBodyType.LandableBody) suffixes.Add("L");
                if (suffixes.Count > 0) txt += $" ({string.Join(',', suffixes)})";
                if (scan.body.type == SystemBodyType.Star)
                    txt = $"{prefix}{scan.body.shortName} - {scan.body.starType} Star";

                drawTextAt(oneSix, txt, highlight ? GameColors.brushCyan : null, GameColors.fontSmall2);
                newLine(true);

                // 2nd line: scan values
                var reward = scan.body.reward.ToString("N0"); // scan.reward ?
                drawTextAt(thirty, $"{reward}", dssWorthy ? GameColors.brushCyan : null);
                if (scan.body.type != SystemBodyType.Star && !scan.body.dssComplete)
                {
                    drawTextAt("| ", GameColors.brushGameOrangeDim);
                    drawTextAt(scan.dssReward.ToString("N0"), dssWorthy ? GameColors.brushCyan : null);
                }

                if (scan.body.bioSignalCount > 0)
                {
                    drawTextAt("| ", GameColors.brushGameOrangeDim);
                    drawTextAt($"{scan.body.bioSignalCount} Genus", GameColors.brushCyan);
                }
                newLine(+four, true);

                var plotSysStatusTop = Program.getPlotter<PlotSysStatus>()?.Top;
                var plotBioSysTop = Program.getPlotter<PlotBioSystem>()?.Top;
                if (dty > plotSysStatusTop - hundred || dty > plotBioSysTop - hundred)
                    break;
            }

            drawTextAt(eight, "Scan value | DSS value", GameColors.brushGameOrangeDim);
            newLine(true);
            drawTextAt(eight, "♦ Undiscovered", GameColors.brushGameOrangeDim);
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
