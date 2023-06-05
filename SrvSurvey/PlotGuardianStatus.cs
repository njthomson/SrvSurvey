using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotGuardianStatus : PlotBase, PlotterForm
    {
        private int selectedIndex = 0;
        private Point[] ptMain;
        private Point[] ptLetter;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool highlightBlink = false;

        private PlotGuardianStatus() : base()
        {
            this.Width = 500;
            this.Height = 108;

            timer.Tick += Timer_Tick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Tick -= Timer_Tick;
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));

            const int blockWidth = 100;
            const int blockTop = 45;
            const int letterOffset = 10;
            ptMain = new Point[]
            {
                new Point((int)(this.mid.Width - (blockWidth * 1.5f)) , blockTop ),
                new Point((int)(this.mid.Width - (blockWidth * 0.5f)), blockTop ),
                new Point((int)(this.mid.Width + (blockWidth * 0.5f)) , blockTop )
            };
            ptLetter = new Point[]
            {
                new Point((int)(this.mid.Width - (blockWidth * 1.5f) - letterOffset) , blockTop - letterOffset),
                new Point((int)(this.mid.Width - (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset),
                new Point((int)(this.mid.Width + (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset)
            };
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatCenterTop(this, gameRect, 0);

            this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            this.selectedIndex = game.status.FireGroup % 3;

            if (!blink && timer.Enabled == false)
            {
                // if we are within a blink detection window - highlight the footer
                var duration = DateTime.Now - game.status.lastblinkChange;
                if (duration.TotalMilliseconds < Game.settings.blinkDuration)
                {
                    this.highlightBlink = true;
                    timer.Interval = Game.settings.blinkDuration;
                    timer.Start();
                }
            }

            base.Status_StatusChanged(blink);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            this.highlightBlink = false;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || PlotGuardians.instance == null) return;

            base.OnPaintBackground(e);
            this.g = e.Graphics;

            if (game.vehicle == ActiveVehicle.Foot)
            {
                drawCenterMessage("Use Profile Analyser near Relic Towers aiming assistance");
                drawFooterText("(toggle lights to force update)");
                return;
            }

            switch (PlotGuardians.instance.mode)
            {
                case Mode.siteType:
                    drawSiteType();
                    return;

                case Mode.heading:
                    drawCenterMessage("\r\nAlign with buttess");
                    showSelectionCue();
                    return;

                case Mode.origin:
                    var alt = Util.targetAltitudeForSite(game.nearBody!.siteData.type).ToString("N0");
                    drawCenterMessage($"Align with site origin and rise to target altitude: {alt}m");
                    drawFooterText("(toggle lights to force update)");
                    return;
            }

            if (PlotGuardians.instance.nearestPoi != null)
            {
                drawSelectedItem();
            }
            else
            {
                drawCenterMessage("Move within ~75m to select an item");
                drawFooterText("(toggle lights to force update)");
            }
        }

        private void drawSelectedItem()
        {
            var poi = PlotGuardians.instance?.nearestPoi;
            if (poi == null) return;

            var poiType = poi.type.ToString().ToUpper();
            var poiStatus = game.nearBody?.siteData.poiStatus.GetValueOrDefault(poi.name);

            drawHeaderText($"{poiType} ({poi.name}) : {poiStatus}");

            var nextStatus = (SitePoiStatus)(game.status.FireGroup % 3) + 1;
            var highlightIdx = poiStatus == SitePoiStatus.unknown || (nextStatus != poiStatus) ? game.status.FireGroup % 3 : -1;

            drawOptions(
                "Present",
                "Absent",
                poi.type != POIType.relic ? "Empty" : null,
                highlightIdx
            );
        }

        private void drawOptions(string msg1, string msg2, string? msg3, int highlightIdx)
        {
            var c = highlightIdx == 0 ? GameColors.Cyan : GameColors.Orange;
            TextRenderer.DrawText(g, "A:", Game.settings.fontSmall, ptLetter[0], c);
            TextRenderer.DrawText(g, msg1, Game.settings.fontMiddle, ptMain[0], c);

            c = highlightIdx == 1 ? GameColors.Cyan : GameColors.Orange;
            TextRenderer.DrawText(g, "B:", Game.settings.fontSmall, ptLetter[1], c);
            TextRenderer.DrawText(g, msg2, Game.settings.fontMiddle, ptMain[1], c);

            if (msg3 != null)
            {
                c = highlightIdx == 2 ? GameColors.Cyan : GameColors.Orange;
                TextRenderer.DrawText(g, "C:", Game.settings.fontSmall, ptLetter[2], c);
                TextRenderer.DrawText(g, msg3, Game.settings.fontMiddle, ptMain[2], c);
            }

            // show selection rectangle
            var rect = new Rectangle(0, 0, 86, 44);
            rect.Location = ptMain[selectedIndex];
            rect.Offset(-12, -12);
            var p = highlightIdx == selectedIndex ? Pens.Cyan : GameColors.penGameOrange1;
            g.DrawRectangle(p, rect);

            showSelectionCue();
        }

        private void showSelectionCue()
        {
            // show cue to select
            var triggerTxt = "cockpit mode";
            var b = this.highlightBlink ? GameColors.brushCyan : GameColors.brushGameOrange;
            drawFooterText($"(toggle {triggerTxt} twice to set)", b);
        }

        private void drawCenterMessage(string msg)
        {
            var font = Game.settings.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = 34;

            g.DrawString(msg, font, GameColors.brushGameOrange, tx, ty);
        }

        private void drawSiteType()
        {
            drawHeaderText($"Site type unknown", GameColors.brushCyan);

            drawOptions(
                "Alpha",
                "Beta",
                "Gamma",
                game.status.FireGroup
            );
        }
    }
}
