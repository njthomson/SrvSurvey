using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotSysStatus : PlotBase, PlotterForm
    {
        public string? nextSystem;
        private Font boldFont = Game.settings.fontMiddleBold;

        private PlotSysStatus() : base()
        {
            this.Width = 420;
            this.Height = 48;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = Game.settings.fontMiddle;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ??
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            //Elite.floatLeftTop(this, gameRect, 4, 10);
            Elite.floatLeftBottom(this, gameRect, 10, 60);

            this.Invalidate();
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.CommsPanel);
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var targetMode = this.game.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.CommsPanel);
            if (this.Opacity > 0 && !targetMode)
                this.Opacity = 0;
            else if (this.Opacity == 0 && targetMode)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected override void onJournalEntry(FSSBodySignals entry)
        {
            Game.log($"PlotSysStatus: FSSBodySignals event: {entry.Bodyname}");
            this.Invalidate();
        }

        protected override void onJournalEntry(FSSDiscoveryScan entry)
        {
            Game.log($"PlotSysStatus: Scan event: {entry.SystemName}");
            this.nextSystem = null;
            this.Invalidate();
        }

        protected override void onJournalEntry(Scan entry)
        {
            Game.log($"PlotSysStatus: Scan event: {entry.Bodyname}");
            this.nextSystem = null;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            g.DrawString("System survey remaining:", Game.settings.fontSmall, GameColors.brushGameOrange, 4, 7);

            this.dtx = 6.0f;
            this.dty = 19.0f;

            var sys = this.game.systemStatus;
            var destinationBody = game.status.Destination?.Name?.Replace(sys.name, "").Replace(" ", "");

            try
            {
                if (this.nextSystem != null)
                {
                    // render next system only, if populated
                    this.drawTextAt("Next system:");
                    this.drawTextAt(this.nextSystem, GameColors.brushCyan);
                    return;
                }

                if (!sys.fssComplete && sys.fssBodies.Count < sys.bodyCount)
                {
                    this.drawTextAt("FSS incomplete", GameColors.brushCyan);
                }
                else if (sys.fssComplete && sys.fssBodies.Count == 0)
                {
                    this.drawTextAt("No scans required");
                }
                else if (sys.dssRemaining.Count > 0)
                {
                    this.drawTextAt($"{sys.dssRemaining.Count}x bodies: ");
                    this.drawRemainingBodies(destinationBody, sys.dssRemaining);
                }
                else
                {
                    this.drawTextAt("DSS scans complete");
                }

                var organicScanDiff = sys.sumOrganicSignals - sys.scannedOrganics;
                if (organicScanDiff > 0)
                {
                    this.drawTextAt($"| {organicScanDiff}x Bio signals on: ");
                    //this.drawTextAt($"| Bio signals: ");
                    //var names = sys.bioRemaining.Select(_ => $"{_}:0").ToList();
                    //var txt = $"{sys.bioRemaining}x0";
                    this.drawRemainingBodies(destinationBody, sys.bioRemaining);
                }
            }
            finally
            {
                // resize window to fit as necessary
                this.Width = this.dtx > 170 ? (int)this.dtx + 6 : 170;
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


