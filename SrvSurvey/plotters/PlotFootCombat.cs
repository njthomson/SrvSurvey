using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotFootCombat : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotFootCombat),
            allowed = allowed,
            ctor = (game, def) => new PlotFootCombat(game, def),
            defaultSize = new Size(180, 200), // Not 100, 80?
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowFootCombat_TEST
                && game.systemStation != null
                && game.lastApproachSettlement?.StationFaction.FactionState == "War"
                && game.systemStation.name == game.lastApproachSettlement.Name
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && game.status.Altitude < 100
                //&& Game.activeGame.isMode(GameMode.OnFoot)
                ;
        }

        #endregion

        private int countKills;
        //private int countDied;
        private long sumCredits;

        private PlotFootCombat(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.console_8;
        }

        protected override void onJournalEntry(FactionKillBond entry)
        {
            this.countKills++;
            this.sumCredits += entry.Reward;

            this.invalidate();
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (game?.systemStation == null) return frame.Size;

            tt.draw(N.eight, $"Combat Zone: {game.systemStation.name}");
            //drawTextAt2(game.systemStation.name, GameColors.Fonts.console_8B);
            tt.newLine(N.eight, true);

            tt.draw(N.eight, $"Kills: {this.countKills}");

            tt.draw(N.eighty, $"Bonds: {this.sumCredits.ToString("N0")}");
            tt.newLine(N.eight, true);


            return tt.pad(N.oneSix, N.four);
        }
    }
}
