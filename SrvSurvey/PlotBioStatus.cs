using SrvSurvey.game;
using SrvSurvey.Properties;
using System.Diagnostics;

namespace SrvSurvey
{
    internal partial class PlotBioStatus : PlotBase, PlotterForm
    {
        private string? lastCodexScan;
        public static string? lastEntryId;
        private bool hasImage;

        private PlotBioStatus() : base()
        {
            if (game.cmdr.scanOne?.entryId > 0)
            {
                lastEntryId = game.cmdr.scanOne.entryId.ToString();

                var match = Game.codexRef.matchFromEntryId(lastEntryId);
                this.hasImage = match.variant.imageUrl != null;
            }
        }

        public override bool allow { get => PlotBioStatus.allowPlotter; }

        public static bool allowPlotter
        {
            get => SystemData.isWithinLastDssDuration() 
                || Game.settings.autoShowBioSummary
                && Game.activeGame?.systemBody != null
                && Game.activeGame.systemBody.bioSignalCount > 0
                && Game.activeGame.systemStation == null
                && !Game.activeGame.hidePlottersFromCombatSuits
                && !Game.activeGame.isShutdown
                && !Game.activeGame.atMainMenu
                && !Game.activeGame.status.OnFootSocial
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.status.FsdChargingJump
                && !PlotGuardians.allowPlotter && !Program.isPlotter<PlotGuardians>()
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.SAA, GameMode.Codex);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = PlotBase.scaled(480);
            this.Height = PlotBase.scaled(80);
            this.initializeOnLoad();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            // hide ourself whilst FSD is charging to jump systems
            if (game.status.FsdChargingJump || !this.allow)
                this.Opacity = 0;
            else if (this.Opacity == 0 && !game.status.FsdChargingJump && Elite.getWindowRect().Y > -30000)
                this.Opacity = PlotPos.getOpacity(this);
        }

        #region journal events

        protected override void onJournalEntry(CodexEntry entry)
        {
            if (entry.SubCategory == "$Codex_SubCategory_Organic_Structures;" && game.systemBody != null && game.status.hasLatLong)
            {
                lastEntryId = entry.EntryID.ToString();
                var match = Game.codexRef.matchFromEntryId(lastEntryId);
                if (game.cmdr.scanOne == null)
                    this.hasImage = match.variant.imageUrl != null;

                this.lastCodexScan = entry.Name_Localised;
                if (game.systemBody.firstFootFall)
                    this.lastCodexScan += $" {Util.credits(match.species.reward * 5)} (FF bonus)";
                else
                    this.lastCodexScan += $" {Util.credits(match.species.reward)}";

                this.Invalidate();
            }
        }

        protected override void onJournalEntry(ScanOrganic entry)
        {
            this.lastCodexScan = null;
            var match = Game.codexRef.matchFromVariant(entry.Variant);
            lastEntryId = match.entryId.ToString();
            this.hasImage = match.variant.imageUrl != null;

            this.Invalidate();
        }

        protected override void onJournalEntry(SendText entry)
        {
            var msg = entry.Message.ToLowerInvariant();

            // show picture, if we have an entryId
            if (msg == MsgCmd.show && lastEntryId != null)
            {
                FormShowCodex.show(lastEntryId);
            }
        }

        #endregion

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            base.Game_modeChanged(newMode, force);

            if (game.systemBody == null || game.systemBody.bioSignalCount == 0)
                Program.closePlotter<PlotBioStatus>();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game?.systemBody == null) return;

            var scanOne = game.cmdr.scanOne;
            if (game.systemBody.organisms?.Count > 0)
            {
                g.DrawString(
                    $"Biological signals: {game.systemBody.bioSignalCount} | Analyzed: {game.systemBody.countAnalyzedBioSignals}",
                    GameColors.fontSmall, GameColors.brushGameOrange, four, eight);

                var organism = scanOne == null ? null : game.systemBody?.organisms?.FirstOrDefault(_ => _.species == scanOne.species);
                if (organism == null)
                {
                    this.showAllGenus(g);

                    // warn if scan is from another body
                    if (scanOne?.body != null && scanOne.body != game.systemBody?.name)
                    {
                        var match = Game.codexRef.matchFromGenus(scanOne.genus);
                        this.drawFooterText(g, $"WARNING: Incomplete {match?.englishName} scans from {scanOne.body}", GameColors.brushRed);
                    }
                }
                else
                    this.showCurrentGenus(g, organism);

                this.drawValueCompletion(g);
            }
            else
            {
                // show a message if cmdr forgot to DSS the body
                var msg = $"Bio signals detected - DSS Scan required";
                var mid = this.Size / 2;
                var font = GameColors.fontSmall;
                var sz = g.MeasureString(msg, GameColors.fontMiddle);
                var tx = mid.Width - (sz.Width / 2);
                var ty = oneSix;
                g.DrawString(msg, GameColors.fontMiddle, GameColors.brushCyan, tx, ty);
            }

            if (scanOne == null)
            {
                var allScanned = game.systemBody!.countAnalyzedBioSignals == game.systemBody.bioSignalCount;
                if (allScanned && game.systemBody.firstFootFall)
                    this.drawFooterText(g, "All signals scanned with FF bonus applied");
                else if (allScanned)
                    this.drawFooterText(g, "All signals scanned", GameColors.brushGameOrange);
                else if (this.lastCodexScan != null)
                {
                    this.drawFooterText(g, this.lastCodexScan, GameColors.brushCyan);
                    if (lastEntryId != null)
                        this.drawHasImage(g, this.Width - threeSix, this.Height - threeSix);
                }
                else if (game.systemBody.firstFootFall)
                    this.drawFooterText(g, "Applying first footfall bonus", GameColors.brushCyan);
                else if (!game.systemBody.wasMapped && game.systemBody.countAnalyzedBioSignals == 0 && Game.settings.useExternalData && Game.settings.autoLoadPriorScans && Program.getPlotter<PlotPriorScans>() == null)
                    this.drawFooterText(g, "Potential first footfall - send '.ff' to confirm", GameColors.brushCyan);
            }
        }

        private void showCurrentGenus(Graphics g, SystemOrganism organism)
        {
            float y = twoEight;

            // left circle - always filled
            var r = new RectangleF(eight, y, twoFour, twoFour);
            g.FillEllipse(GameColors.brushGameOrangeDim, r);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // middle circle - filled after scan two
            r = new RectangleF(forty, y, twoFour, twoFour);
            if (game.cmdr.scanTwo != null)
            {
                g.FillEllipse(GameColors.brushGameOrangeDim, r);
                g.DrawEllipse(GameColors.penGameOrange2, r);
            }
            else
            {
                g.DrawEllipse(GameColors.penGameOrange2, r);
            }

            // right circle - always empty
            r = new RectangleF(sevenTwo, y, twoFour, twoFour);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // Species name
            var txt = $"{organism.variantLocalized}"; // or species?
            var f = GameColors.fontBig;
            var sz = g.MeasureString(txt, f);
            if (sz.Width > this.Width - oneOhFour - eight - threeTwo) f = GameColors.font18;
            sz = g.MeasureString(txt, f);
            if (sz.Width > this.Width - oneOhFour - eight - threeTwo) f = GameColors.font14;

            var x = oneOhFour;
            g.DrawString(
                txt,
                f, GameColors.brushCyan,
                x, y - eight);

            // Reward
            if (organism.reward > 0)
            {
                var reward = game.systemBody!.firstFootFall ? organism.reward * 5 : organism.reward;
                var txt2 = Util.credits(reward);
                if (game.systemBody.firstFootFall) txt2 += " (FF bonus)";
                g.DrawString(
                    txt2,
                    GameColors.fontSmall, GameColors.brushCyan,
                    four, sixTwo);
            }

            this.drawScale(g, organism.range);

            if (lastEntryId != null)
                this.drawHasImage(g, this.Width - threeFour, twoFour);
        }

        private void drawHasImage(Graphics g, float x, float y)
        {
            g.DrawIcon(Resources.picture, (int)x, (int)y);
            if (!this.hasImage)
            {
                y += two;
                g.DrawLine(GameColors.penDarkRed4, x, y, x + thirty, y + twoEight);
                g.DrawLine(GameColors.penDarkRed4, x, y + twoEight, x + thirty, y);
            }
        }

        private void drawScale(Graphics g, float dist)
        {
            float pad = eight;

            g.ResetTransform();

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height + two;

            g.DrawString(txt, GameColors.fontSmall, GameColors.brushCyan,
                x, y,
                StringFormat.GenericTypographic);

            x -= pad;
            y += pad - two;

            var bar = PlotBase.scaled(dist * 0.25f);

            g.DrawLine(GameColors.penCyan2, x, y, x - bar, y); // bar
            g.DrawLine(GameColors.penCyan2, x, y - four, x, y + four); // right edge
            g.DrawLine(GameColors.penCyan2, x - bar, y - four, x - bar, y + four); // left edge
        }

        private void drawValueCompletion(Graphics g)
        {
            float pad = eight;

            g.ResetTransform();

            // use a simpler percentage
            var percent = 100.0f / (float)game.systemBody!.bioSignalCount * (float)game.systemBody.countAnalyzedBioSignals;
            var txt = $" {(int)percent}%";
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = pad;

            var b = percent < 100 ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, GameColors.fontSmall, b,
                x, y,
                StringFormat.GenericTypographic);

            float length = hundred;
            //var scannedLength = 20; // ratio * data.sumAnalyzed;

            x = this.Width - pad - txtSz.Width - length;
            y += pad - two;

            // known un-scanned - solid blue line
            g.DrawLine(GameColors.penCyan4, x, y, x + length, y);

            // already scanned value - orange bar
            g.FillRectangle(GameColors.brushGameOrange, x, nine, PlotBase.scaled(percent), ten);

            // active scan organism value - solid blue bar
            if (game.cmdr.scanOne != null)
                g.FillRectangle(GameColors.brushCyan, x + PlotBase.scaled(percent), ten, length / (float)game.systemBody!.bioSignalCount, eight);
        }

        private void showAllGenus(Graphics g)
        {
            // all the Genus names
            float x = twoFour;
            float y = twoTwo;

            if (game.systemBody?.organisms == null || game.systemBody.organisms.Count == 0)
            {
                Game.log($"Why is game.systemBody!.organisms NULL ??");
                g.DrawString(
                    "Something is wrong. Please share logs.",
                    GameColors.fontSmall,
                    Brushes.OrangeRed,
                    x, y);

                return;
            }

            var allScanned = true;
            foreach (var organism in game.systemBody!.organisms)
            {
                allScanned &= organism.analyzed;
                var txt = organism.genusLocalized;
                if (txt == null && organism.variantLocalized != null) txt = Util.getGenusDisplayNameFromVariant(organism.variantLocalized);
                if (organism.genus == "$Codex_Ent_Brancae_Name;" && organism.speciesLocalized != null) txt = organism.speciesLocalized;
                if (organism.range > 0 && !organism.analyzed)
                {
                    allScanned &= false;
                    txt += $"|{organism.range}m";
                }

                /* TODO: show rewards here another time - it will require network calls to get the species name before we've visited it directly
                if (organism.Reward > 0)
                {
                    var credits = Util.credits(1234567); // (long)organism.Reward);
                    txt += $"|{credits}";
                }
                //else
                //{
                //    txt += $"|? CR";
                //}
                // */

                var sz = g.MeasureString(txt, GameColors.fontSmall);
                if (x + sz.Width > this.Width - oneSix)
                {
                    x = twoFour;
                    y += sz.Height;
                }

                g.DrawString(
                    txt,
                    GameColors.fontSmall,
                    organism.analyzed ? GameColors.brushGameOrange : GameColors.brushCyan,
                    x, y);

                if (organism.analyzed)
                {
                    // strike-through if already analyzed
                    var ly = y + sz.Height * .45f;
                    g.DrawLine(GameColors.penGameOrange1, x, ly, x + sz.Width, ly);
                    g.DrawLine(GameColors.penGameOrangeDim1, x + 1, ly + 1, x + sz.Width + 1, ly + 1);
                }

                x += sz.Width + eight;
            }
        }

        protected void drawFooterText(Graphics g, string msg, Brush? brush = null)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            var mid = this.Size / 2;
            var font = GameColors.fontSmall;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 5;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
        }

    }
}
