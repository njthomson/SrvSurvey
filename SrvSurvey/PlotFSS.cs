using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotFSS : PlotBase, PlotterForm
    {
        private FSSBodySignals? lastFSSBodySignals;

        private static long lastSystemAddress;
        private static string? lastBodyName;
        private static string? lastInitialValue;
        private static string? lastMappedValue;
        private static string? lastNotes;
        private static bool lastWasDiscovered;

        private PlotFSS() : base()
        {
            if (game.systemData == null) throw new Exception("Why no SystemData when creating PlotFSS?");
            // game.systemData.lastFssBody = game.systemData.bodies.Find(b => b.shortName == "1a"); // tmp
            var lastFssBody = game.systemData.lastFssBody;

            if (lastFssBody != null)
            {
                lastBodyName = lastFssBody.name;
                if (!lastFssBody.wasDiscovered) lastBodyName += " (undiscovered)";

                lastInitialValue = Util.GetBodyValue(lastFssBody, false, false).ToString("N0");
                lastMappedValue = Util.GetBodyValue(lastFssBody, true, true).ToString("N0");
                if (lastFssBody.bioSignalCount > 0)
                    lastNotes = $"{lastFssBody.bioSignalCount} bio signals:";
            }
            else if (lastSystemAddress > 0 && lastSystemAddress != game.systemData!.address)
            {
                lastBodyName = null;
                lastInitialValue = null;
                lastMappedValue = null;
                lastNotes = null;
            }

            lastSystemAddress = game.systemData.address;
        }

        public override bool allow { get => PlotFSS.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(420);
            this.Height = scaled(96);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            get => Game.activeGame?.cmdr != null
                && Game.settings.autoShowPlotFSS
                && Game.activeGame.isMode(GameMode.FSS);
        }

        protected override void onJournalEntry(FSSBodySignals entry)
        {
            Game.log($"PlotFSS: FSSBodySignals event: {entry.Bodyname}");
            this.lastFSSBodySignals = entry;
        }

        protected override void onJournalEntry(Scan entry)
        {
            Game.log($"PlotFSS: Scan event: {entry.Bodyname}");

            // ignore Belt Clusters
            if (entry.Bodyname.Contains("Belt Cluster") || !string.IsNullOrEmpty(entry.StarType))
                return;

            lastBodyName = entry.Bodyname;
            lastWasDiscovered = entry.WasDiscovered;
            lastInitialValue = Util.GetBodyValue(entry, false).ToString("N0"); // 123.ToString("#.## M");
            lastMappedValue = Util.GetBodyValue(entry, true).ToString("N0"); // 456.ToString("#.## M");
            lastNotes = "";

            if (this.lastFSSBodySignals?.BodyID == entry.BodyID)
            {
                var bioSignal = this.lastFSSBodySignals?.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");

                if (bioSignal != null)
                {
                    lastNotes = $"{bioSignal.Count} bio signals:";

                    //var hasVulcanism = !string.IsNullOrEmpty(entry.Volcanism);
                    //// TODO: consider check for zero or low atmosphere?
                    //// var lowAtmosphere = this.lastScan?.AtmosphereType == "None";
                    //if (hasVulcanism) // && lowAtmosphere)
                    //{
                    //    lastNotes += " | Candidate for Brain Trees?";
                    //}
                }
            }

            if (!entry.WasDiscovered)
                lastBodyName += " (undiscovered)";

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            var brush = lastWasDiscovered ? GameColors.brushGameOrange : GameColors.brushCyan;

            dty = 8;
            drawTextAt(four, $"Last scan:    {lastBodyName}", brush, GameColors.fontSmaller);

            if (!string.IsNullOrEmpty(lastBodyName))
            {
                var msg = $"Estimated value:    {lastInitialValue} cr\r\nWith surface scan:    {lastMappedValue} cr";
                dty = twoSix;
                drawTextAt(oneEight, msg, GameColors.brushGameOrange, GameColors.fontMiddle);

                if (!string.IsNullOrEmpty(lastNotes))
                {
                    dty = sixFour;
                    drawTextAt(oneEight, lastNotes, GameColors.brushCyan, GameColors.fontMiddle);

                    // draw volume bars from predictions
                    if (game?.systemData?.lastFssBody?.genusPredictions != null)
                    {
                        dtx += six;
                        dtx += PlotBioSystem.drawBodyBars(g, game.systemData.lastFssBody, dtx, dty + two, true);

                        var txt = " " + game.systemData.lastFssBody.getMinMaxBioRewards(false);
                        drawTextAt(txt, GameColors.brushCyan, GameColors.fontMiddle);
                    }
                }
            }
        }
    }
}

