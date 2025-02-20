using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotGuardianStatus;

namespace SrvSurvey.plotters
{
    // TODO: As one of the first overlays made, this one is well due for a refactor. Alas at the time of making it work
    // for localization, I'm just making it work, but it would be worth a going over before too long.

    [ApproxSize(500, 108)]
    internal class PlotGuardianStatus : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => PlotGuardians.allowPlotter
                || (
                    Game.activeGame?.mode == GameMode.GlideMode
                    && PlotGuardianStatus.glideSite != null
                );
        }

        private int selectedIndex = 0;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer(); // TODO: remove? It's in the base class
        private bool highlightBlink = false;
        public static GuardianSiteData? glideSite;
        private float blockWidth;
        private float maxTextWidth;

        private PlotGuardianStatus() : base()
        {
            this.Width = scaled(500);
            this.Height = scaled(108);

            timer.Tick += Timer_Tick;
        }

        public override bool allow { get => PlotGuardianStatus.allowPlotter; }

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

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            var ww = Util.maxWidth(GameColors.fontMiddle, Res.ChoosePresent, Res.ChooseAbsent, Res.ChooseEmpty);
            this.blockWidth = ww * 1.30f;
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (PlotGuardianStatus.glideSite != null && newMode != GameMode.GlideMode)
                PlotGuardianStatus.glideSite = null;

            base.Game_modeChanged(newMode, force);
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            this.selectedIndex = game.status.FireGroup % 3;

            if (!blink && timer.Enabled == false)
            {
                // if we are within a blink detection window - highlight the footer
                var duration = DateTime.Now - game.status.lastBlinkChange;
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

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            try
            {
                if (PlotGuardianStatus.glideSite != null && game.mode == GameMode.GlideMode)
                {
                    this.drawOnApproach();
                    return;
                }

                if (PlotGuardians.instance == null || game.systemSite == null) return;
                maxTextWidth = 0;

                switch (PlotGuardians.instance.mode)
                {
                    case Mode.siteType:
                        drawSiteType();
                        return;

                    case Mode.heading:
                        drawCenterMessage("\r\n" + Res.AlignWithButtress);
                        showSelectionCue();
                        return;

                    case Mode.origin:
                        var alt = Util.targetAltitudeForSite(game.systemSite!.type).ToString("N0");
                        drawCenterMessage(Res.AlignAndRise.format(alt));
                        var sz = drawFooterText(Res.ToggleLightsUpdateHint);
                        if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
                        return;
                }

                if (PlotGuardians.instance.nearestPoi != null && (PlotGuardians.instance.nearestPoi.type == POIType.obelisk || PlotGuardians.instance.nearestPoi.type == POIType.brokeObelisk))
                {
                    drawNearObelisk();
                }
                else if (game.vehicle == ActiveVehicle.Foot)
                {
                    if (game.status.SelectedWeapon == "$humanoid_companalyser_name;" && !string.IsNullOrEmpty(PlotGuardians.instance.nearestPoi?.name))
                    {
                        var msg = Res.ToggleShieldsForRelicTower;
                        var angle = game.systemSite.getRelicHeading(PlotGuardians.instance.nearestPoi!.name);
                        if (game.systemSite.isRuins && game.systemSite.relicTowerHeading != -1)
                            msg += "\r\n" + Res.RelicTowerHeadingKnown.format(game.systemSite!.relicTowerHeading);
                        else if (!game.systemSite.isRuins && angle != null)
                            msg += "\r\n" + Res.RelicTowerHeadingKnown.format(angle);
                        else
                            msg += "\r\n" + Res.RelicTowerFaceHint;
                        drawCenterMessage(msg);
                    }
                    else
                    {
                        drawCenterMessage(Res.RelicTowerNonToolHint);
                    }

                    var sz = drawFooterText(Res.RelicTowerFootHint);
                    if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
                }
                else if (PlotGuardians.instance.nearestPoi == null)
                {
                    var sz = drawHeaderText(Res.NoNearPoiHeader);
                    if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;

                    sz = drawFooterText(Res.ToggleLightsUpdateHint);
                    if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;

                    drawOptions(
                        Res.ChoosePresent,
                        Res.ChooseAbsent,
                        Res.ChooseEmpty,
                        -2
                    );
                }
                else
                {
                    drawSelectedItem();
                }
            }
            finally
            {
                if (maxTextWidth > this.Width)
                    this.formAdjustSize(maxTextWidth, this.Height);
            }
        }

        private void drawNearObelisk()
        {
            var poi = PlotGuardians.instance?.nearestPoi;
            var siteData = game.systemSite;
            if (siteData == null || poi == null) return;

            var obelisk = siteData.getActiveObelisk(poi.name);
            var ramTahNeeded = siteData.ramTahNeeded(obelisk?.msg);
            var brush = ramTahNeeded ? GameColors.brushCyan : null;

            // assume active, unless ...
            var headerStatus = Res.ObeliskActive;
            if (poi.type == POIType.brokeObelisk)
                headerStatus = Res.ObeliskBroken;

            SizeF sz;
            this.dtx = ten;
            this.dty = twoSix;
            if (obelisk != null)
            {
                headerStatus = Res.ObeliskActive;

                // show hint to scan it
                if (obelisk.scanned)
                    sz = this.drawFooterText(Res.YouHaveScanned);
                else
                    sz = this.drawFooterText(Res.YouHaveNotScanned, GameColors.brushCyan);
                if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;

                this.drawTextAt(Res.RequiresPrefix, brush, GameColors.fontMiddle);
                if (obelisk.items == null)
                {
                    this.drawTextAt("??", brush, GameColors.fontMiddle);
                }
                else
                {
                    // first, do we have the items needed?
                    var item1 = obelisk.items.First().ToString();
                    var hasItem1 = game.getInventoryItem(item1)?.Count >= 1;

                    var item2 = obelisk.items.Count > 1 ? obelisk.items.Last().ToString() : null;
                    var hasItem2 = item2 == null ? true : game.getInventoryItem(item2)?.Count >= (item1 == item2 ? 2 : 1);

                    this.drawRamTahDot(0, five, item1);
                    this.drawTextAt(Util.getLoc(item1), hasItem1 ? brush : Brushes.Red, GameColors.fontMiddle);
                    if (item2 != null)
                    {
                        this.drawTextAt("+", brush, GameColors.fontMiddle);
                        this.dtx += two;
                        this.drawRamTahDot(0, five, item2);
                        this.drawTextAt(Util.getLoc(item2), hasItem2 ? brush : Brushes.Red, GameColors.fontMiddle);
                    }
                }
                this.drawTextAt(Res.RequiresSuffix.format(obelisk.msgDisplay), brush, GameColors.fontMiddle);

                // show current status if Ram Tah mission is active
                if (game.cmdr.ramTahActive)
                {
                    this.dtx = ten;
                    this.dty = fourSix;
                    if (ramTahNeeded)
                        this.drawTextAt("► " + Res.LineRamTahNeeded, brush, GameColors.fontMiddle);
                    else
                        this.drawTextAt("► " + Res.LineRamTahAcquired, brush, GameColors.fontMiddle);
                }

                if (game.guardianMatsFull)
                {
                    this.dtx = ten;
                    this.dty += twoTwo;

                    if (this.highlightBlink)
                        sz = this.drawTextAt(Res.MatsFullOne, GameColors.brushCyan, GameColors.fontSmaller);
                    else
                        sz = this.drawTextAt(Res.MatsFullTwo, GameColors.brushGameOrange, GameColors.fontSmaller);
                    if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
                }
            }
            else
            {
                headerStatus = Res.ObeliskInactive;
                sz = this.drawFooterText(Res.FooterActiveObeliskGroups.format(string.Join(" ", siteData.obeliskGroups)));
                if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
            }

            sz = this.drawHeaderText(Res.NearObeliskHeader.format(poi.name, headerStatus));
            if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
        }

        private void drawSelectedItem()
        {
            var poi = PlotGuardians.instance?.nearestPoi;
            if (poi == null) return;

            var poiStatus = game.systemSite!.poiStatus.GetValueOrDefault(poi.name);
            var poiStatusText = Util.getLoc(poiStatus);
            var sz = drawHeaderText(Util.getLoc(poi.type).ToUpper() + $" ({poi.name}) : " + poiStatusText);
            if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;

            var nextStatus = (SitePoiStatus)(game.status.FireGroup % 3) + 1;
            var highlightIdx = poiStatus == SitePoiStatus.unknown || (nextStatus != poiStatus) ? game.status.FireGroup % 3 : -1;

            drawOptions(
                Res.ChoosePresent,
                Res.ChooseAbsent,
                poi.type != POIType.relic ? Res.ChooseEmpty : null,
                highlightIdx
            );
        }

        private void drawOptions(string msg1, string msg2, string? msg3, int highlightIdx)
        {
            int blockTop = scaled(45);
            int letterOffset = scaled(10);

            var mw = this.Width / 2;
            var ptMain = new Point[]
            {
                new Point((int)(mw - (blockWidth * 1.5f)) , blockTop ),
                new Point((int)(mw - (blockWidth * 0.5f)), blockTop ),
                new Point((int)(mw + (blockWidth * 0.5f)) , blockTop )
            };

            var ptLetter = new Point[]
            {
                new Point((int)(mw - (blockWidth * 1.5f) - letterOffset) , blockTop - letterOffset),
                new Point((int)(mw - (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset),
                new Point((int)(mw + (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset)
            };

            var c = highlightIdx == 0 ? GameColors.Cyan : GameColors.Orange;
            if (highlightIdx == -2) c = Color.Gray;
            TextRenderer.DrawText(g, "A:", GameColors.fontSmall, ptLetter[0], c);
            TextRenderer.DrawText(g, msg1, GameColors.fontMiddle, ptMain[0], c);

            c = highlightIdx == 1 ? GameColors.Cyan : GameColors.Orange;
            if (highlightIdx == -2) c = Color.Gray;
            TextRenderer.DrawText(g, "B:", GameColors.fontSmall, ptLetter[1], c);
            TextRenderer.DrawText(g, msg2, GameColors.fontMiddle, ptMain[1], c);

            if (msg3 != null)
            {
                c = highlightIdx == 2 ? GameColors.Cyan : GameColors.Orange;
                if (highlightIdx == -2) c = Color.Gray;
                TextRenderer.DrawText(g, "C:", GameColors.fontSmall, ptLetter[2], c);
                TextRenderer.DrawText(g, msg3, GameColors.fontMiddle, ptMain[2], c);
            }

            // show selection rectangle
            var rect = new RectangleF(
                ptMain[selectedIndex].X - oneTwo, ptMain[selectedIndex].Y - oneTwo,
                this.blockWidth, fourFour);
            var p = highlightIdx == selectedIndex ? Pens.Cyan : GameColors.penGameOrange1;
            if (highlightIdx == -2 || (msg3 == null && highlightIdx == 2)) p = Pens.Gray;
            g.DrawRectangle(p, rect);

            if (highlightIdx != -2)
                showSelectionCue();
        }

        private void showSelectionCue()
        {
            // show cue to select
            var b = this.highlightBlink ? GameColors.brushCyan : GameColors.brushGameOrange;
            var footerTxt = this.highlightBlink
                ? Res.FooterToggleModeTwice
                : Res.FooterToggleModeOnce;
            var sz = drawFooterText(footerTxt, b);
            if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
        }

        private void drawCenterMessage(string msg)
        {
            var font = GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = Util.centerIn(this.Width, sz.Width);
            var ty = threeFour;

            g.DrawString(msg, font, GameColors.brushGameOrange, tx, ty);
            if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
        }

        private void drawSiteType()
        {
            var sz = drawHeaderText(Res.HeaderUnknownSiteType, GameColors.brushCyan);
            if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;

            drawOptions(
                Properties.Guardian.Alpha,
                Properties.Guardian.Beta,
                Properties.Guardian.Gamma,
                game.status.FireGroup
            );
        }

        private void drawOnApproach()
        {
            var site = PlotGuardianStatus.glideSite;
            if (site == null) return;

            SizeF sz;
            if (site.isRuins)
            {
                sz = drawHeaderText(Res.OnApproachHeaderRuins);
                if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
                drawCenterMessage(Res.OnApproachMiddleRuins.format(site.index, Util.getLoc(site.type.ToString())), GameColors.brushCyan);
            }
            else
            {
                sz = drawHeaderText(Res.OnApproachHeaderStructure);
                if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
                var msg = $"{site.type} ";

                if (site.type == GuardianSiteData.SiteType.Robolobster || site.type == GuardianSiteData.SiteType.Squid || site.type == GuardianSiteData.SiteType.Stickyhand)
                    msg = Res.OnApproachMiddleStructure.format(site.type, Properties.Guardian.BluePrintFighter);
                else if (site.type == GuardianSiteData.SiteType.Turtle)
                    msg = Res.OnApproachMiddleStructure.format(site.type, Properties.Guardian.BluePrintModule);
                else if (site.type == GuardianSiteData.SiteType.Bear || site.type == GuardianSiteData.SiteType.Hammerbot || site.type == GuardianSiteData.SiteType.Bowl)
                    msg = Res.OnApproachMiddleStructure.format(site.type, Properties.Guardian.BluePrintWeapon);
                else
                    msg = Res.OnApproachMiddleStructureNoBluePrint.format(site.type);

                drawCenterMessage(msg, GameColors.brushCyan);
            }
            sz = drawFooterText(Res.OnApproachFooter);
            if (sz.Width > maxTextWidth) maxTextWidth = sz.Width;
        }
    }
}
