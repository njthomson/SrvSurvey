using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotJumpInfo : PlotBase, PlotterForm
    {
        private PlotJumpInfo() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall2;
        }

        public override bool allow { get => PlotJumpInfo.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(320);
            this.Height = scaled(88);
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            get => Game.activeGame?.status != null
                && Game.settings.autoShowPlotJumpInfoTest
                && (
                    // when super crusing and the FSD is charging, or ...
                    (Game.activeGame.mode == GameMode.SuperCruising && Game.activeGame.status.FsdCharging)
                    // whilst in whitch space, jumping to next system
                    || Game.activeGame.mode == GameMode.FSDJumping

                    // TODO: when NOT in super cruise, can we tell the FSD is charging for system jump vs just charging for super cruise?
                );
        }

        protected override void onJournalEntry(FSDJump entry)
        {
            // remove this plotter once we arrive in some system
            Program.closePlotter<PlotJumpInfo>();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            drawTextAt($"Hello world", GameColors.fontMiddleBold);
            // TODO: ...
        }

    }

}
