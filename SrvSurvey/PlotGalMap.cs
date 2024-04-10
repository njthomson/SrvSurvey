using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotGalMap : PlotBase, PlotterForm
    {
        private string? targetSystemName;
        private string? discoveryStatus;

        private PlotGalMap() : base()
        {
            this.Width = scaled(200);
            this.Height = scaled(20);

            this.Font = GameColors.fontMiddle;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap;
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

            this.Invalidate();
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (this.IsDisposed) return;


            // lookup if target system has been discovered
            var target = game.navRoute.Route.LastOrDefault();
            if (target != null)
            {
                this.lookupSystem(target.StarSystem).ContinueWith(r =>
                {
                    if (r.Exception != null) Util.isFirewallProblem(r.Exception);
                });
            }
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (this.IsDisposed) return;

            this.Invalidate();
        }

        private async Task lookupSystem(string systemName)
        {
            // lookup in EDSM
            var response = await Game.edsm.getBodies(systemName);
            if (response == null)
            {
                // system is not known to EDSM
                this.discoveryStatus = "unknown system";
            }
            else if (response.bodyCount > response.bodies.Count)
            {
                // system is known but not fully discovered
                this.discoveryStatus = $"partially discovered, {response.bodies.Count} of {response.bodyCount} bodies";
            }
            else
            {
                // system is fully discovered
                this.discoveryStatus = "fully discovered";
            }

            // TODO: lookup in Canonn for bio data too?
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;

            try
            {
                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                this.dtx = eight;
                this.dty = ten;

                this.dty += this.drawTextAt($"Target: {this.targetSystemName}").Height;
                this.dtx = eight;
                this.dty += this.drawTextAt($"Status: {this.discoveryStatus}").Height;

            }
            catch (Exception ex)
            {
                Game.log($"PlotGalMap.OnPaintBackground error: {ex}");
            }
        }
    }

}
