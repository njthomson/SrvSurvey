using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    internal class PlotSphericalSearch : PlotBase, PlotterForm
    {
        private bool reqInProgress = false;
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

            // do lookup on existing target if there is one
            if (!string.IsNullOrEmpty(game.status.Destination?.Name))
                measureDistanceToSystem(game.status.Destination.Name);
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

        protected override void onJournalEntry(FSDTarget entry)
        {
            measureDistanceToSystem(entry.Name);
        }

        private void measureDistanceToSystem(string systemName)
        {
            this.targetSystemName = systemName;
            this.distance = -1;
            this.reqInProgress = true;
            this.Invalidate();

            Game.edsm.getSystems(systemName).ContinueWith(rslt =>
            {
                if (rslt.IsCompletedSuccessfully)
                {
                    var targetSystem = rslt.Result.FirstOrDefault();
                    if (targetSystem != null && game.cmdr.sphereLimit.centerStarPos != null)
                        this.distance = Util.getSystemDistance(game.cmdr.sphereLimit.centerStarPos, targetSystem.coords.starPos);
                    else
                        this.distance = -2;
                }
                this.reqInProgress = false;
                this.Invalidate();
            });
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

            if (this.reqInProgress)
            {
                TextRenderer.DrawText(g, $"Distance: ...", font, pt, GameColors.Orange, TextFormatFlags.Left);
            }
            else if (this.distance >= 0)
            {
                var td = this.distance.ToString("N2");
                var tc = this.distance < game.cmdr.sphereLimit.radius ? GameColors.Cyan : Color.Red;
                TextRenderer.DrawText(
                    g,
                    $"Distance: {td}ly",
                    font,
                    pt,
                    tc,
                    TextFormatFlags.Left);
            }
            else if (this.distance  == -2)
            {
                TextRenderer.DrawText(
                    g,
                    $"Distance: (unknown system)",
                    font,
                    pt,
                    Color.DarkRed,
                    TextFormatFlags.Left);
            }
        }
    }
}
