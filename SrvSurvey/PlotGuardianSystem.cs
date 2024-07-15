using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotGuardianSystem : PlotBase, PlotterForm
    {
        private PlotGuardianSystem() : base()
        {
            this.Width = scaled(420);
            this.Height = scaled(88);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = GameColors.fontMiddle;
        }

        public override bool allow { get => PlotGuardianSystem.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            // Game.settings.enableGuardianSites && Game.settings.autoShowGuardianSummary && PlotGuardianSystem.allowPlotter && game?.systemData?.settlements.Count > 0
            get => Game.settings.enableGuardianSites
                && Game.settings.autoShowGuardianSummary
                && Game.activeGame?.systemData != null
                && Game.activeGame.systemData.settlements.Count > 0
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap);
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

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game.systemData == null) return;

            this.dtx = six;
            this.dty = eight;
            var sz = new SizeF(six, six);

            var sites = game?.systemData?.settlements;
            this.drawTextAt($"Guardian sites: {sites?.Count ?? 0}", GameColors.brushGameOrange, GameColors.fontSmall);
            if (this.dtx > sz.Width) sz.Width = this.dtx;

            if (sites?.Count > 0)
            {
                this.dty = twenty;
                foreach (var site in sites)
                {
                    var txt = $"{site.displayText}{site.type}";

                    var highlight = game?.status?.Destination?.Body == site.body.id;
                    if (highlight && game?.status?.Destination.Name.StartsWith("$Ancient") == true && game?.status?.Destination.Name != site.name)
                        highlight = false;
                    var brush = highlight ? GameColors.brushCyan : null;

                    // draw main text (bigger font)
                    this.dtx = eight;
                    this.dty += this.drawTextAt(txt, brush).Height;
                    if (this.dtx > sz.Width) sz.Width = this.dtx;

                    // draw status (smaller font)
                    if (site.bluePrint != null)
                    {
                        this.dtx = twenty;
                        this.dty += this.drawTextAt("- " + site.bluePrint, brush, GameColors.fontSmall).Height + two;
                        if (this.dtx > sz.Width) sz.Width = this.dtx;
                    }

                    if (site.status != null)
                    {
                        this.dtx = twenty;
                        this.dty += this.drawTextAt("- " + site.status, brush, GameColors.fontSmall).Height + two;
                        if (this.dtx > sz.Width) sz.Width = this.dtx;
                    }

                    // draw extra (smaller font)
                    if (site.extra != null)
                    {
                        this.dtx = twenty;
                        this.dty += this.drawTextAt("- " + site.extra, brush, GameColors.fontSmall).Height;
                        if (this.dtx > sz.Width) sz.Width = this.dtx;
                    }
                }
            }

            // resize window as necessary
            sz.Width += ten;
            sz.Height = this.dty + ten;
            if (this.Size != sz.ToSize())
            {
                this.Size = sz.ToSize();
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
                this.Invalidate();
            }
        }
    }
}

