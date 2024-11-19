using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotSysStatus : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotSysStatus
                && Game.activeGame != null
                && Game.activeGame.status?.InTaxi != true
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                // show only after honking or we have Canonn data
                && Game.activeGame.systemData != null
                && (Game.activeGame.systemData.honked || Game.activeGame.canonnPoi != null);
        }

        public string? nextSystem;
        private Font boldFont = GameColors.fontMiddleBold;

        private PlotSysStatus() : base()
        {
            this.Font = GameColors.fontMiddle;
        }

        public override bool allow { get => PlotSysStatus.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            // Size set during paint

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onJournalEntry(FSSBodySignals entry)
        {
            Game.log($"PlotSysStatus: FSSBodySignals event: {entry.Bodyname}");
            this.Invalidate();
        }

        protected override void onJournalEntry(FSSDiscoveryScan entry)
        {
            Game.log($"PlotSysStatus: FSSDiscoveryScan event: {entry.SystemName}");
            this.nextSystem = null;
            this.Invalidate();
        }

        protected override void onJournalEntry(Scan entry)
        {
            Game.log($"PlotSysStatus: Scan event: {entry.Bodyname}");
            this.nextSystem = null;
            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            var minViableWidth = scaled(170);
            try
            {
                if (game?.systemData == null || game.status == null || !PlotSysStatus.allowPlotter)
                {
                    this.Opacity = 0;
                    Program.control.BeginInvoke(() => Program.closePlotter<PlotSysStatus>());
                    return;
                }

                this.dty = eight;
                drawTextAt2(six, Misc.PlotSysStatus_Header, GameColors.fontSmall);
                newLine(two, true);
                dtx = six;

                // reduce destination to it's short name
                var destinationBody = game.status.Destination?.Name?.Replace(game.systemData.name, "").Replace(" ", "");

                if (this.nextSystem != null)
                {
                    // render next system only, if populated
                    this.drawTextAt2(Misc.PlotSysStatus_NextSystem);
                    this.drawTextAt2(this.nextSystem, GameColors.Cyan);
                    return;
                }

                var dssRemaining = game.systemData.getDssRemainingNames();
                if (!game.systemData.honked)
                {
                    this.drawTextAt2(Misc.PlotSysStatus_FssNotStarted, GameColors.Cyan);
                }
                else if (!game.systemData.fssComplete)
                {
                    var fssProgress = 100.0 / (float)game.systemData.bodyCount * (float)game.systemData.fssBodyCount;
                    var txt = dssRemaining.Count == 0
                        ? Misc.PlotSysStatus_FssCompleteLong.format((int)fssProgress)
                        : Misc.PlotSysStatus_FssCompleteShort.format((int)fssProgress);
                    this.drawTextAt2(txt, GameColors.Cyan);
                }

                if (dssRemaining.Count > 0)
                {
                    if (dtx > 6) this.drawTextAt2(" ");
                    this.drawTextAt2(Misc.PlotSysStatus_DssRemaining.format(dssRemaining.Count));
                    this.drawRemainingBodies(destinationBody, dssRemaining);
                }
                else if (game.systemData.fssComplete && game.systemData.honked)
                {
                    this.drawTextAt2(Misc.PlotSysStatus_NoDssMeet);
                }
                newLine(true);

                if (!Game.settings.autoShowPlotBioSystem)
                {
                    var bioRemaining = game.systemData.getBioRemainingNames();
                    if (bioRemaining.Count > 0)
                    {
                        this.drawTextAt2(Misc.PlotSysStatus_BioSignals.format(game.systemData.bioSignalsRemaining));
                        this.drawRemainingBodies(destinationBody, bioRemaining);
                    }
                }

                var nonBodySignalCount = game.systemData.nonBodySignalCount;
                if (Game.settings.showNonBodySignals && nonBodySignalCount > 0)
                {
                    var sz = this.drawTextAt2(six, Misc.PlotSysStatus_NonBodySignals.format(nonBodySignalCount), GameColors.fontSmall2);
                    newLine(true);
                }
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.formAdjustSize(six, six);
                }
            }
        }

        /// <summary>
        /// Render names in a horizontal list, highlighting any in the same group as the destination
        /// </summary>
        private void drawRemainingBodies(string? destination, List<string> names)
        {
            const TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

            // draw each remaining body, highlighting color if they are in the same group as the destination, or all of them if no destination
            foreach (var bodyName in names)
            {
                var isLocal = string.IsNullOrEmpty(destination) || bodyName[0] == destination[0];

                var font = this.Font;

                if (destination == bodyName) font = this.boldFont;
                var color = isLocal ? GameColors.Cyan : GameColors.Orange;

                var sz = g.MeasureString(bodyName, font).ToSize();
                var rect = new Rectangle((int)this.dtx, (int)this.dty, sz.Width, sz.Height);

                TextRenderer.DrawText(g, bodyName, font, rect, color, flags);
                this.dtx += sz.Width;
            }
        }
    }
}

