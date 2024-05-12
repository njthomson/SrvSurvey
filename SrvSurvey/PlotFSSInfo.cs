﻿using SrvSurvey.game;

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
                PlotFSSInfo.scans.Clear();

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
                && Game.settings.autoShowPlotFSSInfoTest
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
            if (entry.Bodyname.Contains("Belt Cluster") || !string.IsNullOrEmpty(entry.StarType) || game?.systemData == null)
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

                drawTextAt(game.systemData.name, GameColors.fontSmallBold);
                newLine(+eight, true);
                var bodyCount = game.systemData.bodies.Count(_ => _.scanned && _.type != SystemBodyType.Asteroid);
                drawTextAt(eight, $"Scanned {bodyCount} bodies: {game.systemData.sumRewards().ToString("N0")} cr");
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

                    var highlight = dssWorthy | hasBioSignals;


                    var planetClass = scan.body.planetClass?.Replace("Sudarsky c", "C");
                    var txt = $"{scan.body.shortName} - {planetClass}";
                    drawTextAt(oneSix, txt, highlight ? GameColors.brushCyan : null);
                    newLine(true);

                    drawTextAt(thirty, $"{scan.reward.ToString("N0")} |");
                    drawTextAt(scan.dssReward.ToString("N0"), dssWorthy ? GameColors.brushCyan : null);

                    if (scan.body.bioSignalCount > 0)
                    {
                        drawTextAt($"| ");
                        drawTextAt($"{scan.body.bioSignalCount} Genus", GameColors.brushCyan);
                    }
                    newLine(+four, true);

                    var plotSysStatusTop = Program.getPlotter<PlotSysStatus>()?.Top;
                    var plotBioSysTop = Program.getPlotter<PlotBioSystem>()?.Top;
                    if (dty > plotSysStatusTop - hundred || dty > plotBioSysTop - hundred)
                        break;
                }

                newLine();
                drawTextAt(eight, "( scan value | DSS value )", GameColors.brushGameOrangeDim);
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
