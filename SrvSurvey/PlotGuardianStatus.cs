using SrvSurvey.game;
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;

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

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

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
            if (this.IsDisposed || PlotGuardians.instance == null || this.game.isShutdown || game.systemSite == null) return;

            base.OnPaintBackground(e);
            this.g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

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
                    var alt = Util.targetAltitudeForSite(game.systemSite!.type).ToString("N0");
                    drawCenterMessage($"Align with site origin and rise to target altitude: {alt}m");
                    drawFooterText("(toggle lights to force update)");
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
                    var msg = "Toggle shields to set Relic Tower heading.";
                    if (game.systemSite.isRuins && game.systemSite.relicTowerHeading != -1)
                        msg += $"\r\nRecorded heading: {game.systemSite!.relicTowerHeading}°";
                    else if (!game.systemSite.isRuins && game.systemSite.relicHeadings.ContainsKey(PlotGuardians.instance.nearestPoi.name))
                        msg += $"\r\nRecorded heading: {game.systemSite.relicHeadings[PlotGuardians.instance.nearestPoi!.name]}°";
                    else
                        msg += "\r\nFace the side with a single large left facing triangle.";
                    drawCenterMessage(msg);
                }
                else
                {
                    drawCenterMessage("Use Profile Analyser near Relic Towers for aiming assistance.\r\nFace the side with a single large left facing triangle.");
                }

                drawFooterText("(toggle weapon to force location update)");
            }
            else if (PlotGuardians.instance.nearestPoi == null)
            {
                drawHeaderText("Move within ~75m to select an item");
                drawFooterText("(toggle lights to force update)");
                drawOptions(
                    "Present",
                    "Absent",
                    "Empty",
                    -2
                );
            }
            else
            {
                drawSelectedItem();
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

            var headerTxt = $"Obelisk {poi.name}: ";
            if (poi.type == POIType.brokeObelisk)
                headerTxt += "Broken";

            this.dtx = 10;
            this.dty = 30;
            if (obelisk != null)
            {
                headerTxt += "Active";

                // show the material reward, or a hint to scan it
                if (obelisk.scanned || (obelisk.data != null && obelisk.data.Count > 0))
                {
                    headerTxt += " - " + string.Join(", ", obelisk.data).ToUpperInvariant();
                    if (obelisk.scanned)
                        this.drawFooterText("You have scanned this obelisk");
                    else
                        this.drawFooterText("You have not scanned this obelisk", GameColors.brushCyan);
                }
                else
                {
                    this.drawFooterText("Scan to populate data material", GameColors.brushCyan);
                }


                this.drawTextAt("Requires:", brush, GameColors.fontMiddle);
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

                    this.drawTextAt(item1, hasItem1 ? brush : Brushes.Red, GameColors.fontMiddle);
                    if (item2 != null)
                    {
                        this.drawTextAt("+ ", brush, GameColors.fontMiddle);
                        this.drawTextAt(item2, hasItem2 ? brush : Brushes.Red, GameColors.fontMiddle);
                    }
                }
                this.drawTextAt($"for {obelisk.msgDisplay}", brush, GameColors.fontMiddle);

                // show current status if Ram Tah mission is active
                if (game.cmdr.ramTahActive)
                {
                    this.dtx = 10;
                    this.dty = 50;
                    if (ramTahNeeded)
                        this.drawTextAt($"(Needed for Ram Tah mission)", brush, GameColors.fontMiddle);
                    else
                        this.drawTextAt($"(Log already acquired)", brush, GameColors.fontMiddle);
                }
            }
            else
            {
                headerTxt += "Inactive";
                this.drawFooterText("Active obelisk groups: " + string.Join(" ", siteData.obeliskGroups));
            }

            this.drawHeaderText(headerTxt);
        }

        private void drawSelectedItem()
        {
            var poi = PlotGuardians.instance?.nearestPoi;
            if (poi == null) return;

            var poiType = poi.type.ToString().ToUpper();
            var poiStatus = game.systemSite!.poiStatus.GetValueOrDefault(poi.name);

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
            var rect = new Rectangle(0, 0, 86, 44);
            rect.Location = ptMain[selectedIndex];
            rect.Offset(-12, -12);
            var p = highlightIdx == selectedIndex ? Pens.Cyan : GameColors.penGameOrange1;
            if (highlightIdx == -2 || (msg3 == null && highlightIdx == 2)) p = Pens.Gray;
            g.DrawRectangle(p, rect);

            if (highlightIdx != -2)
                showSelectionCue();
        }

        private void showSelectionCue()
        {
            // show cue to select
            var triggerTxt = "cockpit mode";
            var b = this.highlightBlink ? GameColors.brushCyan : GameColors.brushGameOrange;
            var times = this.highlightBlink ? "twice" : "once";
            drawFooterText($"(toggle {triggerTxt} {times} to set)", b);
        }

        private void drawCenterMessage(string msg)
        {
            var font = GameColors.fontMiddle;
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

    internal class PlotGuardianBeaconStatus : PlotBaseSelectable, PlotterForm
    {
        private string relatedStructure;
        private bool dataLinkScanned;
        private bool confirmed;
        private bool different;

        private PlotGuardianBeaconStatus() : base()
        {
            this.relatedStructure = Game.canonn.allBeacons.Find(_ => _.systemAddress == game.cmdr.currentSystemAddress)?.relatedStructure!;
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var targetMode = game.isMode(GameMode.Flying, GameMode.CommsPanel);
            if (this.Opacity > 0 && !targetMode)
                this.Opacity = 0;
            else if (this.Opacity == 0 && targetMode)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            if (blink && !this.confirmed)
            {
                this.confirmed = !this.confirmed;
                this.different = this.selectedIndex == 1;

                // add a note that it changed (even though we cannot tell directly what it changed to)
                var data = GuardianBeaconData.Load(game.cmdr.currentSystem);
                if (data != null)
                {
                    data.notes += $"Different: {this.different}, date: {DateTimeOffset.UtcNow}\r\n";
                    data.Save();
                }
            }

            base.Status_StatusChanged(blink);
        }

        protected override void onJournalEntry(DataScanned entry)
        {
            if (entry.Type == "$Datascan_AncientPylon;")
            {
                // A Guardian Beacon
                Game.log($"Scanned data from Guardian Beacon in: {game.cmdr.currentSystem}");
                this.dataLinkScanned = true;
                this.Invalidate();
            }
        }

        protected override void onJournalEntry(SupercruiseEntry entry)
        {
            Program.closePlotter<PlotGuardianStatus>();
            Program.closePlotter<PlotGuardianBeaconStatus>();
        }

        protected override void onJournalEntry(FSDJump entry)
        {
            Program.closePlotter<PlotGuardianBeaconStatus>();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed) return;

            base.OnPaintBackground(e);
            this.g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (!this.dataLinkScanned)
            {
                drawHeaderText("Guardian Beacon");
                drawCenterMessage($"Please activate and scan with Data Link Scanner", GameColors.brushCyan);
            }
            else if (this.different)
            {
                drawHeaderText($"Target body changed!", GameColors.brushCyan);
                drawCenterMessage("Please share a screenshot of your inbox message!", GameColors.brushCyan);
            }
            else if (game.mode == GameMode.CommsPanel)
            {
                if (!this.confirmed)
                {
                    drawHeaderText($"Confirm: {this.relatedStructure} ?", GameColors.brushCyan);
                    drawOptions("Yes", "No", null, game.status.FireGroup
                    );
                }
                else if (!this.different)
                {
                    drawHeaderText($"Confirm: {this.relatedStructure}");
                    drawCenterMessage($"Confirmed");
                    drawFooterText("(thank you)");
                }
            }
            else
            {
                drawHeaderText("Guardian Beacon scanned");

                if (!this.confirmed)
                {
                    drawCenterMessage($"Please confirm target body:\r\n{this.relatedStructure}");
                    drawFooterText("(check your inbox)", GameColors.brushCyan);
                }
                else if (!this.different)
                {
                    drawCenterMessage($"Confirmed target body:\r\n{this.relatedStructure}");
                }
            }
        }
    }
}
