using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotRamTah;

namespace SrvSurvey.plotters
{
    [ApproxSize(200, 280)]
    internal class PlotRamTah : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            // TODO: show this earlier, like on approach?
            get => Game.settings.autoShowRamTah
                && Game.settings.enableGuardianSites
                && Game.activeGame?.systemBody != null
                && Game.activeGame.cmdr.ramTahActive
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.status?.hasLatLong == true
                && Game.activeGame.systemSite?.location != null
                && Game.activeGame.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed, GameMode.Flying, GameMode.InFighter, GameMode.CommsPanel, GameMode.InternalPanel)
                ;
        }

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

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game?.systemSite == null) return;

            this.dtx = six;
            this.dty = eight;
            var sz = new SizeF(six, six);

            var ramTahObelisks = game.systemSite.ramTahObelisks;
            this.drawTextAt(Res.Header.format(ramTahObelisks?.Count ?? 0), GameColors.fontSmall);
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
                    Brush brush = GameColors.brushGameOrange;
                    if (isTargetObelisk && !isCurrentObelisk && game.systemSite.currentObelisk != null)
                        brush = GameColors.brushDarkCyan;
                    else if (isCurrentObelisk || isTargetObelisk)
                        brush = GameColors.brushCyan;
                    // change colours if items are missing? Perhaps overkill?
                    //else if (!hasItem1 || !hasItem2)
                    //    brush = GameColors.brushRed;

                    // draw main text (bigger font)
                    this.dtx = oneFour;
                    var logName = $"{Util.getLogNameFromChar(bar.Key[0])} #{bar.Key.Substring(1)}:";
                    var sz2 = this.drawTextAt(logName, brush, GameColors.fontMiddle);
                    this.dty += six;
                    if (!hasItem1 || !hasItem2)
                    {
                        // strike-through the log name if we do not have sufficient items
                        var p1 = GameColors.penGameOrange1;
                        var p2 = GameColors.penGameOrangeDim1;
                        if (isCurrentObelisk || isTargetObelisk)
                        {
                            p1 = GameColors.penCyan1;
                            p2 = GameColors.penDarkCyan1;
                        }

                        g.DrawLine(p1, dtx - eight, dty + sz.Height - 1, dtx - sz2.Width, dty + sz.Height - 1);
                        g.DrawLine(p1, dtx - eight + 1, dty + sz.Height, dtx - sz2.Width + 1, dty + sz.Height);
                    }

                    this.drawRamTahDot(0, 0, item1);
                    var item1Text = Util.getLoc(item1);
                    this.drawTextAt(item1Text, hasItem1 ? GameColors.brushGameOrange : Brushes.Red, GameColors.fontSmall);

                    if (item2 != null)
                    {
                        this.drawTextAt("+", brush, GameColors.fontSmall);
                        this.dtx += two;
                        this.drawRamTahDot(0, 0, item2);
                        var item2Text = Util.getLoc(item2);
                        this.drawTextAt(item2Text, hasItem2 ? GameColors.brushGameOrange : Brushes.Red, GameColors.fontSmall);
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
                            this.drawTextAt(ob, brush, GameColors.fontSmall);
                    }

                    this.dty += oneFour;
                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                }

                this.dtx = eight;
                this.dty += ten;
                this.dty += this.drawTextAt(Res.Footer, GameColors.fontSmall).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }
            else
            {
                this.dtx = twoFour;
                this.dty += this.drawTextAt(Res.NoNewLogs, GameColors.brushGameOrange, GameColors.fontMiddle).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }

            // resize window as necessary
            sz.Width += ten;
            sz.Height = this.dty + ten;
            if (this.Size != sz.ToSize())
            {
                this.Size = sz.ToSize();
                this.BackgroundImage = GameGraphics.getBackgroundImage(this);
                this.reposition(Elite.getWindowRect());
            }
        }
    }
}

