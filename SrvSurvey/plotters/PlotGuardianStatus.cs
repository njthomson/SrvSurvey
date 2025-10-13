using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotGuardianStatus;

namespace SrvSurvey.plotters
{
    // TODO: As one of the first overlays made, this one is well due for a refactor. Alas at the time of making it work
    // for localization, I'm just making it work, but it would be worth a going over before too long.

    internal class PlotGuardianStatus : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotGuardianStatus),
            allowed = allowed,
            ctor = (game, def) => new PlotGuardianStatus(game, def),
            defaultSize = new Size(500, 108),
        };

        public static bool allowed(Game game)
        {
                return PlotGuardians.allowed(game)
                || (game.mode == GameMode.GlideMode && PlotGuardianStatus.glideSite != null);
        }

        #endregion

        private int selectedIndex = 0;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer(); // TODO: remove? It's in the base class
        private bool highlightBlink = false;
        public static GuardianSiteData? glideSite;
        private float blockWidth;

        private PlotGuardianStatus(Game game, PlotDef def) : base(game, def)
        {
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                this.highlightBlink = false;
                this.invalidate();
            };

            var ww = Util.maxWidth(GameColors.fontMiddle, Res.ChoosePresent, Res.ChooseAbsent, Res.ChooseEmpty);
            this.blockWidth = (ww + N.six) * 1.30f;
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (PlotGuardianStatus.glideSite != null && !status.GlideMode && status.changed.Contains("Flags2"))
                PlotGuardianStatus.glideSite = null;

            if (status.changed.Contains(nameof(Status.FireGroup)))
                this.selectedIndex = game.status.FireGroup % 3;

            var blink = status.changed.Contains("blink");
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

            this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            tt.padVertical = 6;
            tt.setMinWidth(def.defaultSize.Width);

            if (PlotGuardianStatus.glideSite != null && game.mode == GameMode.GlideMode)
            {
                this.drawOnApproach(g, tt);
                return new SizeF(tt.frameSize.Width, this.height);
            }

            var plotGuardians = PlotBase2.getPlotter<PlotGuardians>();
            if (plotGuardians == null || game.systemSite == null) return frame.Size;

            switch (plotGuardians.mode)
            {
                case Mode.siteType:
                    drawSiteType(g, tt);
                    return new SizeF(tt.frameSize.Width, this.height);

                case Mode.heading:
                    drawCenterMessage(tt, "\r\n" + Res.AlignWithButtress);
                    showSelectionCue(tt);
                    return new SizeF(tt.frameSize.Width, this.height);

                case Mode.origin:
                    var alt = Util.targetAltitudeForSite(game.systemSite!.type).ToString("N0");
                    drawCenterMessage(tt, Res.AlignAndRise.format(alt));
                    var sz = tt.drawFooter(Res.ToggleLightsUpdateHint);
                    tt.setMinWidth(sz.Width);
                    return new SizeF(tt.frameSize.Width, this.height);
            }

            var nearestPoi = plotGuardians.nearestPoi;
            if (nearestPoi != null && (nearestPoi.type == POIType.obelisk || nearestPoi.type == POIType.brokeObelisk))
            {
                drawNearObelisk(g, tt, nearestPoi);
            }
            else if (game.vehicle == ActiveVehicle.Foot)
            {
                if (game.status.SelectedWeapon == "$humanoid_companalyser_name;" && !string.IsNullOrEmpty(nearestPoi?.name))
                {
                    var col = highlightBlink ? C.cyan : C.orange;
                    var msg = Res.ToggleShieldsForRelicTower;
                    var angle = game.systemSite.getRelicHeading(nearestPoi!.name);
                    if (game.systemSite.isRuins && game.systemSite.relicTowerHeading != -1)
                        msg += "\r\n" + Res.RelicTowerHeadingKnown.format(game.systemSite!.relicTowerHeading);
                    else if (!game.systemSite.isRuins && angle != null)
                        msg += "\r\n" + Res.RelicTowerHeadingKnown.format(angle);
                    else
                        msg += "\r\n" + Res.RelicTowerFaceHint;
                    drawCenterMessage(tt, msg, col);
                }
                else
                {
                    drawCenterMessage(tt, Res.RelicTowerNonToolHint);
                }

                var sz = tt.drawFooter(Res.RelicTowerFootHint);
                tt.setMinWidth(sz.Width);
            }
            else if (nearestPoi == null)
            {
                var sz = tt.drawHeader(Res.NoNearPoiHeader);
                tt.setMinWidth(sz.Width);

                sz = tt.drawFooter(Res.ToggleLightsUpdateHint);
                tt.setMinWidth(sz.Width);

                drawOptions(g, tt,
                    Res.ChoosePresent,
                    Res.ChooseAbsent,
                    Res.ChooseEmpty,
                    -2
                );
            }
            else
            {
                drawSelectedItem(g, tt, nearestPoi);
            }

            return new SizeF(tt.frameSize.Width, this.height);
        }

        private void drawNearObelisk(Graphics g, TextCursor tt, SitePOI poi)
        {
            var siteData = game.systemSite;
            if (siteData == null || poi == null) return;

            var obelisk = siteData.getActiveObelisk(poi.name);
            var ramTahNeeded = siteData.ramTahNeeded(obelisk?.msg);
            var col = ramTahNeeded ? C.cyan : C.orange;

            // assume active, unless ...
            var headerStatus = Res.ObeliskActive;
            if (poi.type == POIType.brokeObelisk)
                headerStatus = Res.ObeliskBroken;

            SizeF sz;
            tt.dtx = N.ten;
            tt.dty = N.twoSix;
            if (obelisk != null)
            {
                headerStatus = Res.ObeliskActive;

                // show hint to scan it
                if (obelisk.scanned)
                    sz = tt.drawFooter(Res.YouHaveScanned);
                else
                    sz = tt.drawFooter(Res.YouHaveNotScanned, C.cyan);
                tt.setMinWidth(sz.Width);

                tt.draw(Res.RequiresPrefix, col, GameColors.fontMiddle);
                if (obelisk.items == null)
                {
                    tt.draw("??", col, GameColors.fontMiddle);
                }
                else
                {
                    // first, do we have the items needed?
                    var item1 = obelisk.items.First().ToString();
                    var hasItem1 = game.getInventoryItem(item1)?.Count >= 1;

                    var item2 = obelisk.items.Count > 1 ? obelisk.items.Last().ToString() : null;
                    var hasItem2 = item2 == null ? true : game.getInventoryItem(item2)?.Count >= (item1 == item2 ? 2 : 1);

                    tt.dtx += PlotRamTah.drawRamTahDot(g, tt.dtx, tt.dty + N.five, item1);
                    tt.draw(Util.getLoc(item1), hasItem1 ? col : C.red, GameColors.fontMiddle);
                    if (item2 != null)
                    {
                        tt.dtx += N.two;
                        tt.draw("+", col, GameColors.fontMiddle);
                        tt.dtx += N.four;
                        tt.dtx += PlotRamTah.drawRamTahDot(g, tt.dtx, tt.dty + N.five, item2);
                        tt.draw(Util.getLoc(item2), hasItem2 ? col : C.red, GameColors.fontMiddle);
                    }
                }
                tt.draw(" " + Res.RequiresSuffix.format(obelisk.msgDisplay), col, GameColors.fontMiddle);

                // show current status if Ram Tah mission is active
                if (game.cmdr.ramTahActive)
                {
                    tt.dtx = N.ten;
                    tt.dty = N.fourSix;
                    if (ramTahNeeded)
                        tt.draw("► " + Res.LineRamTahNeeded, col, GameColors.fontMiddle);
                    else
                        tt.draw("► " + Res.LineRamTahAcquired, col, GameColors.fontMiddle);
                }

                if (game.guardianMatsFull)
                {
                    tt.dtx = N.ten;
                    tt.dty += N.twoTwo;

                    if (this.highlightBlink)
                        sz = tt.draw(Res.MatsFullOne, C.cyan, GameColors.fontSmaller);
                    else
                        sz = tt.draw(Res.MatsFullTwo, C.orange, GameColors.fontSmaller);
                    tt.setMinWidth(sz.Width);
                }
            }
            else
            {
                headerStatus = Res.ObeliskInactive;
                sz = tt.drawFooter(Res.FooterActiveObeliskGroups.format(string.Join(" ", siteData.obeliskGroups)));
                tt.setMinWidth(sz.Width);
            }

            sz = tt.drawHeader(Res.NearObeliskHeader.format(poi.name, headerStatus));
            tt.setMinWidth(sz.Width);
        }

        private void drawSelectedItem(Graphics g, TextCursor tt, SitePOI poi)
        {
            if (poi == null) return;

            var poiStatus = game.systemSite!.poiStatus.GetValueOrDefault(poi.name);
            var poiStatusText = Util.getLoc(poiStatus);
            var sz = tt.drawHeader(Util.getLoc(poi.type).ToUpper() + $" ({poi.name}) : " + poiStatusText);
            tt.setMinWidth(sz.Width);

            var nextStatus = (SitePoiStatus)(game.status.FireGroup % 3) + 1;
            var highlightIdx = poiStatus == SitePoiStatus.unknown || (nextStatus != poiStatus) ? game.status.FireGroup % 3 : -1;

            // only things in puddles can be empty
            var allowEmpty = poi.type == POIType.orb || poi.type == POIType.casket || poi.type == POIType.tablet || poi.type == POIType.totem || poi.type == POIType.urn;
            drawOptions(g,tt,
                Res.ChoosePresent,
                Res.ChooseAbsent,
                allowEmpty ? Res.ChooseEmpty : null,
                highlightIdx
            );
        }

        private void drawOptions(Graphics g, TextCursor tt, string msg1, string msg2, string? msg3, int highlightIdx)
        {
            int blockTop = N.s(45);
            int letterOffset = N.s(10);

            var mw = this.width / 2;
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
                ptMain[selectedIndex].X - N.oneTwo, ptMain[selectedIndex].Y - N.oneTwo,
                this.blockWidth, N.fourFour);
            var p = highlightIdx == selectedIndex ? Pens.Cyan : GameColors.penGameOrange1;
            if (highlightIdx == -2 || (msg3 == null && highlightIdx == 2)) p = Pens.Gray;
            g.DrawRectangle(p, rect);

            if (highlightIdx != -2)
                showSelectionCue(tt);

            // make sure we're wide enough for 3x blocks
            var ww = (this.blockWidth * 3) + (N.oneTwo * 5);
            tt.setMinWidth(ww);
        }

        private void showSelectionCue(TextCursor tt)
        {
            // show cue to select
            var col = this.highlightBlink ? C.cyan : C.orange;
            var footerTxt = this.highlightBlink
                ? Res.FooterToggleModeTwice
                : Res.FooterToggleModeOnce;

            var sz = tt.drawFooter(footerTxt, col);
            tt.setMinWidth(sz.Width);
        }

        private SizeF drawCenterMessage(TextCursor tt, string msg, Color? col = null, Font? font = null)
        {
            font ??= GameColors.fontMiddle;
            return tt.drawCentered(N.threeFour, msg, col, font);
        }

        private void drawSiteType(Graphics g, TextCursor tt)
        {
            var sz = tt.drawHeader(Res.HeaderUnknownSiteType, C.cyan);
            tt.setMinWidth(sz.Width);

            drawOptions(g, tt,
                Properties.Guardian.Alpha,
                Properties.Guardian.Beta,
                Properties.Guardian.Gamma,
                game.status.FireGroup
            );
        }

        private void drawOnApproach(Graphics g, TextCursor tt)
        {
            var site = PlotGuardianStatus.glideSite;
            if (site == null) return;

            SizeF sz;
            if (site.isRuins)
            {
                sz = tt.drawHeader(Res.OnApproachHeaderRuins, C.orange, GameColors.fontMiddle);
                tt.setMinWidth(sz.Width);
                sz = drawCenterMessage(tt, Res.OnApproachMiddleRuins.format(site.index, Util.getLoc(site.type.ToString())), C.cyan);
                tt.setMinWidth(sz.Width);
            }
            else
            {
                sz = tt.drawHeader(Res.OnApproachHeaderStructure);
                tt.setMinWidth(sz.Width);
                var msg = $"{site.type} ";

                if (site.type == GuardianSiteData.SiteType.Robolobster || site.type == GuardianSiteData.SiteType.Squid || site.type == GuardianSiteData.SiteType.Stickyhand)
                    msg = Res.OnApproachMiddleStructure.format(site.type, Properties.Guardian.BluePrintFighter);
                else if (site.type == GuardianSiteData.SiteType.Turtle)
                    msg = Res.OnApproachMiddleStructure.format(site.type, Properties.Guardian.BluePrintModule);
                else if (site.type == GuardianSiteData.SiteType.Bear || site.type == GuardianSiteData.SiteType.Hammerbot || site.type == GuardianSiteData.SiteType.Bowl)
                    msg = Res.OnApproachMiddleStructure.format(site.type, Properties.Guardian.BluePrintWeapon);
                else
                    msg = Res.OnApproachMiddleStructureNoBluePrint.format(site.type);

                // TODO: Highlight if the obelisk groups are not known?

                sz = drawCenterMessage(tt, msg, C.cyan);
                tt.setMinWidth(sz.Width);
            }
            sz = tt.drawFooter(Res.OnApproachFooter);
            tt.setMinWidth(sz.Width);
        }
    }
}
