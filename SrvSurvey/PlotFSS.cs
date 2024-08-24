using SrvSurvey.game;
using System.Drawing.Drawing2D;

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
            if (lastSystemAddress > 0 && lastSystemAddress != game.systemData!.address)
            {
                lastBodyName = null;
                lastInitialValue = null;
                lastInitialValue = null;
                lastMappedValue = null;
                lastNotes = null;
            }

            lastSystemAddress = game.systemData!.address;
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
                    lastNotes = $"{bioSignal.Count} bio signals";

                    var hasVulcanism = !string.IsNullOrEmpty(entry.Volcanism);
                    // TODO: consider check for zero or low atmosphere?
                    // var lowAtmosphere = this.lastScan?.AtmosphereType == "None";
                    if (hasVulcanism) // && lowAtmosphere)
                    {
                        lastNotes += " | Candidate for Brain Trees";
                    }
                }
            }

            if (!entry.WasDiscovered)
                lastBodyName += " (undiscovered)";

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;

            var brush = lastWasDiscovered ? GameColors.brushGameOrange : GameColors.brushCyan;

            g.DrawString($"Last scan:    {lastBodyName}", GameColors.fontSmaller, brush, four, eight);

            if (!string.IsNullOrEmpty(lastBodyName))
            {
                //if (!this.lastWasDiscovered)
                //    g.DrawString("(undiscovered)", GameColors.fontSmall2, GameColors.brushCyan, 330, 8);

                var msg = $"Estimated value:    {lastInitialValue} cr\r\nWith surface scan:    {lastMappedValue} cr";
                g.DrawString(msg, GameColors.fontMiddle, brush, oneEight, twoEight);

                if (!string.IsNullOrEmpty(lastNotes))
                {
                    var txt = lastNotes;
                    var bodySummary = game.systemData?.bioSummary?.bodyGroups.Find(_ => _.body.name == lastBodyName);
                    if (bodySummary != null)
                    {
                        txt += $" ~{Util.credits(bodySummary.minReward, true)}";
                        if (bodySummary.minReward != bodySummary.maxReward)
                            txt += $" ~{Util.credits(bodySummary.maxReward, true)}";
                    }

                    g.DrawString(txt, GameColors.fontMiddle, GameColors.brushCyan, oneEight, sixFive);
                }
            }
        }
    }
}

