using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotGuardianSystem : PlotBase, PlotterForm
    {
        private Font boldFont = GameColors.fontMiddleBold;

        private PlotGuardianSystem() : base()
        {
            this.Width = 420;
            this.Height = 88;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = GameColors.fontMiddle;
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

            Elite.floatLeftTop(this, gameRect, 4, 20);

            this.Invalidate();
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.CommsPanel);
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game.systemData == null) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;

            base.OnPaintBackground(e);

            this.dtx = 6.0f;
            this.dty = 6.0f;
            var sz = new SizeF(6, 6);

            var sites = game?.systemData?.settlements;
            this.drawTextAt($"Guardian sites: {sites?.Count ?? 0}", GameColors.brushGameOrange, GameColors.fontSmall);
            if (this.dtx > sz.Width) sz.Width = this.dtx;

            if (sites?.Count > 0)
            {
                this.dty = 20f;
                foreach (var site in sites)
                {
                    var txt = $"{site.displayText}{site.type}";

                    var highlight = game?.status?.Destination?.Body == site.body.id;
                    if (highlight && game?.status?.Destination.Name.StartsWith("$Ancient") == true && game?.status?.Destination.Name != site.name)
                        highlight = false;
                    var brush = highlight ? GameColors.brushCyan : null;

                    // draw main text (bigger font)
                    this.dtx = 8f;
                    this.dty += this.drawTextAt(txt, brush).Height;
                    if (this.dtx > sz.Width) sz.Width = this.dtx;

                    // draw status (smaller font)
                    if (site.status != null)
                    {
                        this.dtx = 20f;
                        this.dty += this.drawTextAt("- " + site.status, brush, GameColors.fontSmall).Height + 2;
                        if (this.dtx > sz.Width) sz.Width = this.dtx;
                    }

                    // draw extra (smaller font)
                    if (site.extra != null)
                    {
                        this.dtx = 20f;
                        this.dty += this.drawTextAt("- " + site.extra, brush, GameColors.fontSmall).Height;
                        if (this.dtx > sz.Width) sz.Width = this.dtx;
                    }
                }
            }

            sz.Width += 10;
            sz.Height = this.dty + 10f;

            if (this.Size != sz.ToSize())
            {
                this.Size = sz.ToSize();
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
                this.Invalidate();
            }
            Game.log(sz);
        }
    }
}

