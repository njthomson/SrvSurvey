using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotRamTah : PlotBase, PlotterForm
    {
        private PlotRamTah() : base()
        {
            this.Width = 420;
            this.Height = 88;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = GameColors.fontMiddle;
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

            Elite.floatRightMiddle(this, gameRect, 10);

            this.Invalidate();
        }

        public static bool allowPlotter
        {
            // TODO: show this earlier, like on approach?
            get => Game.settings.autoShowRamTah && Game.activeGame != null && Game.activeGame.showGuardianPlotters && Game.activeGame.cmdr.ramTahActive;
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var targetMode = !game.hidePlottersFromCombatSuits && game.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed, GameMode.Flying, GameMode.InFighter, GameMode.CommsPanel, GameMode.InternalPanel);
            if (this.Opacity > 0 && !targetMode)
                this.Opacity = 0;
            else if (this.Opacity == 0 && targetMode)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game?.systemSite == null) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;

            base.OnPaintBackground(e);

            this.dtx = 6.0f;
            this.dty = 8.0f;
            var sz = new SizeF(6, 6);

            var ramTahObelisks = game.systemSite.ramTahObelisks;
            this.drawTextAt($"Unscanned Ram Tah logs: {ramTahObelisks?.Count ?? 0}", GameColors.fontSmall);
            if (this.dtx > sz.Width) sz.Width = this.dtx;
            this.dty = 24f;

            if (ramTahObelisks?.Count > 0)
            {
                var targetObelisk = PlotGuardians.instance?.targetObelisk;
                foreach (var bar in ramTahObelisks)
                {
                    var obelisk = game.systemSite.getActiveObelisk(bar.Value.First());
                    if (obelisk == null) continue;

                    // first, do we have the items needed?
                    var item1 = obelisk.items.First().ToString();
                    var hasItem1 = game.getInventoryItem(item1)?.Count >= 1;

                    var item2 = obelisk.items.Count > 1 ? obelisk.items.Last().ToString() : null;
                    var hasItem2 = item2 == null ? true : game.getInventoryItem(item2)?.Count >= (item1 == item2 ? 2 : 1);

                    var isTargetObelisk = targetObelisk != null && bar.Value.Contains(targetObelisk);
                    var brush = isTargetObelisk ? GameColors.brushCyan : GameColors.brushGameOrange;

                    // change colours if items are missing? Perhaps overkill?
                    //var brush = (hasItem1 && hasItem2)
                    //    ? isTargetObelisk ? GameColors.brushCyan : GameColors.brushGameOrange
                    //    : isTargetObelisk ? Brushes.DarkCyan : GameColors.brushGameOrangeDim;

                    // draw main text (bigger font)
                    this.dtx = 14f;
                    var logName = $"{Util.getLogNameFromChar(bar.Key[0])} #{bar.Key.Substring(1)}:";
                    this.drawTextAt(logName, brush, GameColors.fontMiddle);
                    this.dty += 6;

                    this.drawTextAt(item1, hasItem1 ? brush : Brushes.Red, GameColors.fontSmall);

                    if (item2 != null)
                    {
                        this.drawTextAt("+ ", brush, GameColors.fontSmall);
                        this.drawTextAt(item2, hasItem2 ? brush : Brushes.Red, GameColors.fontSmall);
                    }

                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                    this.dty += 16;

                    // draw each obelisk name, highlighting the target one
                    this.dtx = 24f;
                    foreach (var ob in bar.Value)
                    {
                        if (targetObelisk == ob)
                            this.drawTextAt(ob, brush , GameColors.fontSmallBold);
                        else
                            this.drawTextAt(ob, GameColors.fontSmall);
                    }

                    this.dty += 14;
                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                }

                this.dtx = 8f;
                this.dty += 10;
                this.dty += this.drawTextAt("Set target obelisk with '.to <A01>'", GameColors.fontSmall).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }
            else
            {
                this.dtx = 24f;
                this.dty += this.drawTextAt($"All logs already scanned.", GameColors.brushGameOrange, GameColors.fontMiddle).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }

            // resize window as necessary
            sz.Width += 10;
            sz.Height = this.dty + 10f;
            if (this.Size != sz.ToSize())
            {
                this.Size = sz.ToSize();
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
                this.reposition(Elite.getWindowRect());
            }

        }
    }
}

