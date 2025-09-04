using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    [ApproxSize(180, 200)]
    internal class PlotFootCombat : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => (Game.settings.autoShowFootCombat_TEST)
                && Game.activeGame?.systemStation != null
                && Game.activeGame.lastApproachSettlement?.StationFaction.FactionState == "War"
                && Game.activeGame.systemStation.name == Game.activeGame.lastApproachSettlement.Name
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && Game.activeGame.status.Altitude < 100
                //&& Game.activeGame.isMode(GameMode.OnFoot)
                ;
        }

        private int countKills;
        //private int countDied;
        private long sumCredits;

        private PlotFootCombat() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.console_8;
        }

        public override bool allow { get => PlotFootCombat.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(100);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onJournalEntry(FactionKillBond entry)
        {
            this.countKills++;
            this.sumCredits += entry.Reward;

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game?.systemStation == null) return;
            if (game.mode == GameMode.MainMenu) Program.closePlotter<PlotFootCombat>(true);

            drawTextAt2(eight, $"Combat Zone: {game.systemStation.name}");
            //drawTextAt2(game.systemStation.name, GameColors.Fonts.console_8B);
            newLine(eight, true);

            drawTextAt2(eight, $"Kills: {this.countKills}");

            drawTextAt2(eighty, $"Bonds: {this.sumCredits.ToString("N0")}");
            newLine(eight, true);


            this.formAdjustSize(oneSix, four);
        }
    }
}
