using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotMiniTrack : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotMiniTrack_TEST
                && Game.activeGame?.systemBody != null
                && Game.activeGame.status.hasLatLong
                && !Game.activeGame.status.Docked
                && hasQuickTrackers
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.RolePanel)
                ;
        }

        public static bool hasQuickTrackers
        {
            get => Game.activeGame?.systemBody?.bookmarks?.Keys.Any(k => k[0] == '#') == true;
        }

        private PlotMiniTrack() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.console_8;
        }

        public override bool allow { get => PlotMiniTrack.allowPlotter; }

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
            drawTextAt2(eight, "Mini track:");
            newLine(eight, true);

            //this.formAdjustSize(oneSix, four);
        }
    }
}
