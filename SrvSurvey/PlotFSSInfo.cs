using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotFSSInfo : PlotBase, PlotterForm
    {
        public static List<FssEntry> scans = new List<FssEntry>();
        private static string? systemName;

        private PlotFSSInfo() : base()
        {
            this.Width = scaled(320);
            this.Height = scaled(88);
            this.BackgroundImageLayout = ImageLayout.Stretch;

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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            get => Game.activeGame?.cmdr != null
                && Game.settings.autoShowPlotFSSInfo
                && Game.activeGame.isMode(GameMode.FSS);
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

            this.Invalidate();
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !PlotFSSInfo.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotFSSInfo.allowPlotter)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed || game?.systemData == null) return;

            try
            {
                this.resetPlotter(e.Graphics);

                // 1st line: system name
                drawTextAt(game.systemData.name, GameColors.fontSmallBold);
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
                    var prefix = scan.body.wasDiscovered ? "" : "*";
                    var txt = $"{prefix}{scan.body.shortName} - {planetClass}"; // ◌◎◉☆★☄☼☀⛀⛃✔✨✶✪❓❔❓⛬❗❕ * ❒❱✪❍❌✋❖⟡⦁⦂⧫
                    if (scan.body.terraformable) txt += " (T)";
                    if (scan.body.type == SystemBodyType.LandableBody) txt += " (L)";
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
                drawTextAt(eight, "* Undiscovered | T Terraformable", GameColors.brushGameOrangeDim);
                newLine(true);

                this.formAdjustSize(+oneEight, +ten);
            }
            catch (Exception ex)
            {
                Game.log($"PlotGalMap.OnPaintBackground error: {ex}");
            }
        }

    }

    class FssEntry
    {
        public SystemBody body;
        public long reward;
        public long dssReward;
    }
}
