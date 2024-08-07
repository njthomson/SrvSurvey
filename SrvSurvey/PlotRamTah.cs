﻿using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotRamTah : PlotBase, PlotterForm
    {
        private PlotRamTah() : base()
        {
            this.Size = Size.Empty;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = GameColors.fontMiddle;
        }

        public override bool allow { get => PlotRamTah.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(420);
            this.Height = scaled(88);
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            // TODO: show this earlier, like on approach?
            get => Game.settings.autoShowRamTah
                && Game.activeGame != null
                && Game.activeGame.cmdr.ramTahActive
                && !Game.activeGame.hidePlottersFromCombatSuits
                && PlotGuardians.allowPlotter
                && Game.activeGame.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed, GameMode.Flying, GameMode.InFighter, GameMode.CommsPanel, GameMode.InternalPanel)
                ;
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game?.systemSite == null) return;

            this.dtx = six;
            this.dty = eight;
            var sz = new SizeF(six, six);

            var ramTahObelisks = game.systemSite.ramTahObelisks;
            this.drawTextAt($"Unscanned Ram Tah logs: {ramTahObelisks?.Count ?? 0}", GameColors.fontSmall);
            if (this.dtx > sz.Width) sz.Width = this.dtx;
            this.dty = twoFour;

            if (ramTahObelisks?.Count > 0)
            {
                var targetObelisk = PlotGuardians.instance?.targetObelisk;
                foreach (var bar in ramTahObelisks)
                {
                    var obelisk = game.systemSite.getActiveObelisk(bar.Value.First());
                    if (obelisk == null || string.IsNullOrEmpty(obelisk.name) || string.IsNullOrEmpty(bar.Key)) continue;

                    // first, do we have the items needed?
                    var item1 = obelisk.items.First().ToString();
                    var hasItem1 = game.getInventoryItem(item1)?.Count >= 1;

                    var item2 = obelisk.items.Count > 1 ? obelisk.items.Last().ToString() : null;
                    var hasItem2 = item2 == null ? true : game.getInventoryItem(item2)?.Count >= (item1 == item2 ? 2 : 1);

                    var isTargetObelisk = targetObelisk != null && bar.Value.Contains(targetObelisk);
                    var isCurrentObelisk = bar.Value.Any(_ => _ == game.systemSite.currentObelisk?.name);
                    var brush = GameColors.brushGameOrange;
                    if (isTargetObelisk && !isCurrentObelisk && game.systemSite.currentObelisk != null)
                        brush = Brushes.DarkCyan as SolidBrush;
                    else if (isCurrentObelisk || isTargetObelisk)
                        brush = GameColors.brushCyan;

                    // change colours if items are missing? Perhaps overkill?
                    //var brush = (hasItem1 && hasItem2)
                    //    ? isTargetObelisk ? GameColors.brushCyan : GameColors.brushGameOrange
                    //    : isTargetObelisk ? Brushes.DarkCyan : GameColors.brushGameOrangeDim;

                    // draw main text (bigger font)
                    this.dtx = oneFour;
                    var logName = $"{Util.getLogNameFromChar(bar.Key[0])} #{bar.Key.Substring(1)}:";
                    this.drawTextAt(logName, brush, GameColors.fontMiddle);
                    this.dty += six;

                    this.drawRamTahDot(0, 0, item1);
                    this.drawTextAt(item1, hasItem1 ? brush : Brushes.Red, GameColors.fontSmall);

                    if (item2 != null)
                    {
                        this.drawTextAt("+", brush, GameColors.fontSmall);
                        this.dtx += two;
                        this.drawRamTahDot(0, 0, item2);
                        this.drawTextAt(item2, hasItem2 ? brush : Brushes.Red, GameColors.fontSmall);
                    }

                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                    this.dty += oneSix;

                    // draw each obelisk name, highlighting the target one
                    this.dtx = twoFour;
                    foreach (var ob in bar.Value)
                    {
                        if (targetObelisk == ob || game.systemSite.currentObelisk?.name == ob)
                            this.drawTextAt(ob, brush, GameColors.fontSmallBold);
                        else
                            this.drawTextAt(ob, GameColors.fontSmall);
                    }

                    this.dty += oneFour;
                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                }

                this.dtx = eight;
                this.dty += ten;
                this.dty += this.drawTextAt("Set target obelisk with '.to <A01>'", GameColors.fontSmall).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }
            else
            {
                this.dtx = twoFour;
                this.dty += this.drawTextAt($"All logs at this site have\r\nalready been scanned.", GameColors.brushGameOrange, GameColors.fontMiddle).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }

            // resize window as necessary
            sz.Width += ten;
            sz.Height = this.dty + ten;
            if (this.Size != sz.ToSize())
            {
                this.Size = sz.ToSize();
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
                this.reposition(Elite.getWindowRect());
            }
        }
    }
}

