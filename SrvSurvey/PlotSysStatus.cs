using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotSysStatus : PlotBase, PlotterForm
    {

        private PlotSysStatus() : base()
        {
            this.Width = 420;
            this.Height = 42;
            this.BackgroundImageLayout = ImageLayout.Stretch;
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
            get => Game.activeGame != null && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap);
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var targetMode = SystemStatus.showPlotter;
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
            this.Invalidate();
        }

        protected override void onJournalEntry(Scan entry)
        {
            Game.log($"PlotSysStatus: Scan event: {entry.Bodyname}");
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            g.DrawString("Scans needed:", Game.settings.fontSmall, GameColors.brushGameOrange, 4, 7);


            var sys = this.game.systemStatus;

            var brush = GameColors.brushCyan;

            var txt = "All DSS scans complete";
            if (!sys.fssComplete && sys.fssBodies.Count < sys.bodyCount)
            {
                //var fssCompletion = (100.0 / sys.bodyCount * sys.fssBodies.Count).ToString("N0");
                //txt = $"FSS completion: {fssCompletion}%";
                txt = $"FSS incomplete";
            }
            else if (sys.fssComplete && sys.fssBodies.Count == 0)
            {
                txt = "Previously scanned system";
                brush = GameColors.brushGameOrange;
            }
            else if (sys.dssRemaining.Count > 0)
            {
                txt = $"{sys.dssRemaining.Count}x DSS: " + String.Join(", ", sys.dssRemaining);
            }
            else
            {
                brush = GameColors.brushGameOrange;
            }

            var organicScanDiff = sys.sumOrganicSignals - sys.scannedOrganics;
            if (organicScanDiff > 0)
            {
                txt += $" | {organicScanDiff}x Bio: " + String.Join(", ", sys.bioRemaining);
            }

            var font = Game.settings.fontMiddle;
            var sz = g.MeasureString(txt, font);
            g.DrawString(txt, font, brush, 4, 16);

            this.Width = sz.Width > 320 ? (int)sz.Width : 320;
        }
    }
}


