using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotSphericalSearch : PlotBase, PlotterForm
    {
        private double distance = -1;
        private string targetSystemName;

        private PlotSphericalSearch() : base()
        {
            this.Width = 400;
            this.Height = 80;

            this.BackColor = Color.Black;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = Game.settings.Opacity;
            this.DoubleBuffered = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));

            measureDistanceToSystem();
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatRightTop(this, gameRect);

            this.Invalidate();
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
            var lastSystem = game.navRoute.Route.LastOrDefault();
            if (lastSystem?.StarSystem == null || game.cmdr.sphereLimit.centerStarPos == null) return;

            Game.log($"Measuring distance to: {lastSystem.StarSystem}");
            this.targetSystemName = lastSystem.StarSystem;
            this.distance = Util.getSystemDistance(game.cmdr.sphereLimit.centerStarPos, lastSystem.StarPos);

            this.Invalidate();

        }

        private static Point p1 = new Point(10, 10);
        private static Point p2 = new Point(10, 24);

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            var g = e.Graphics;
            var font = Game.settings.fontMiddle;

            string txt = $"From: {game.cmdr.sphereLimit.centerSystemName}";
            var pt = new Point(8, 8);
            TextRenderer.DrawText(g, txt, font, p1, GameColors.Orange, TextFormatFlags.Left);
            var sz = TextRenderer.MeasureText(txt, font);

            pt.Y += sz.Height;
            TextRenderer.DrawText(g, $"To: {this.targetSystemName}", font, pt, GameColors.Orange, TextFormatFlags.Left);
            pt.Y += sz.Height;

            if (this.distance >= 0)
            {
                var td = this.distance.ToString("N2");
                var limitDist = game.cmdr.sphereLimit.radius.ToString("N2");
                var tc = this.distance < game.cmdr.sphereLimit.radius ? GameColors.Cyan : Color.Red;
                var verb = this.distance < game.cmdr.sphereLimit.radius ? "within" : "exceeds";
                TextRenderer.DrawText(
                    g,
                    $"Distance: {td}ly - {verb} {limitDist} ly",
                    font,
                    pt,
                    tc,
                    TextFormatFlags.Left);
            }
        }
    }

}
