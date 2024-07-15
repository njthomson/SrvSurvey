using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotSphericalSearch : PlotBase, PlotterForm
    {
        private double distance = -1;
        private string targetSystemName;

        private PlotSphericalSearch() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontMiddle;
        }

        public override bool allow { get => PlotSphericalSearch.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(400);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            this.BeginInvoke(() =>
            {
                measureDistanceToSystem();
            });
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap
                && Game.activeGame.cmdr.sphereLimit.active;
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (this.IsDisposed || game.cmdr.sphereLimit.centerStarPos == null) return;

            measureDistanceToSystem();
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (this.IsDisposed || game.cmdr.sphereLimit.centerStarPos == null) return;

            this.distance = -1;
            this.targetSystemName = "N/A";
            this.Invalidate();
        }

        private void measureDistanceToSystem()
        {
            if (game.cmdr.sphereLimit.centerStarPos == null) return;

            var lastSystem = game.navRoute.Route.LastOrDefault();
            if (lastSystem?.StarSystem == null)
            {
                if (game.systemData == null) return;

                lastSystem = new RouteEntry()
                {
                    StarSystem = game.systemData.name,
                    StarPos = game.systemData.starPos,
                };
            }

            Game.log($"Measuring distance to: {lastSystem.StarSystem}");
            this.targetSystemName = lastSystem.StarSystem;
            this.distance = Util.getSystemDistance(game.cmdr.sphereLimit.centerStarPos, lastSystem.StarPos);

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
                this.dty += this.drawTextAt($"From: {game.cmdr.sphereLimit.centerSystemName}").Height;
                this.dtx = eight;
                this.dty += this.drawTextAt($"To: {this.targetSystemName}").Height;

                if (this.distance >= 0)
                {
                    var dist = this.distance.ToString("N2");
                    var limitDist = game.cmdr.sphereLimit.radius.ToString("N2");
                    var tc = this.distance < game.cmdr.sphereLimit.radius ? GameColors.brushCyan : GameColors.brushRed;
                    var verb = this.distance < game.cmdr.sphereLimit.radius ? "within" : "exceeds";

                    this.dtx = eight;
                    this.drawTextAt($"Distance: {dist}ly - {verb} {limitDist} ly", tc);
                }
        }
    }

}
