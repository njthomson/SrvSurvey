using BioCriterias;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Diagnostics;

namespace SrvSurvey.plotters
{
    [ApproxSize(480, 80)]
    internal partial class PlotBioStatus : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => SystemData.isWithinLastDssDuration()
                || Game.settings.autoShowBioSummary
                && Game.activeGame?.systemBody != null
                && Game.activeGame.systemBody.bioSignalCount > 0
                //&& (Game.activeGame.systemStation == null || !Game.settings.autoShowHumanSitesTest)
                && !Game.activeGame.hidePlottersFromCombatSuits
                && !Game.activeGame.status.Docked
                && !Game.activeGame.isShutdown
                && !Game.activeGame.atMainMenu
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.status.FsdChargingJump
                && !PlotGuardians.allowPlotter && !Program.isPlotter<PlotGuardians>()
                && !PlotHumanSite.allowPlotter && !Program.isPlotter<PlotHumanSite>()
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.CommsPanel, GameMode.SAA, GameMode.Codex);
        }

        private string? lastCodexScan;
        public static string? lastEntryId;
        private bool hasImage;
        private double lastTemp;
        private long? lastScanOneEntryId;
        private Clause? temperatureClause;
        private TempRangeDiffs? tempRangeDiffs;

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

        protected override void OnLoad(EventArgs e)
        {
            this.Width = PlotBase.scaled(480);
            this.Height = PlotBase.scaled(80);
            this.initializeOnLoad();

            tempRangeDiffs = new TempRangeDiffs(this.ClientSize.Width - ten, N.twoFour);
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            if (this.lastTemp != game.status.Temperature)
                this.Invalidate();
            this.lastTemp = game.status.Temperature;

            // hide ourselves whilst FSD is charging to jump systems
            if ((game.status.FsdChargingJump || !this.allow || !Elite.gameHasFocus) && !Debugger.IsAttached)
                this.Opacity = 0;
            else if (this.Opacity == 0 && this.allow)
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
                BaseForm.show<FormShowCodex>();
        }

        #endregion

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            base.Game_modeChanged(newMode, force);

            if (game.cmdr.scanOne?.entryId > 0 && this.lastScanOneEntryId != game.cmdr.scanOne.entryId && Debugger.IsAttached)
                this.prepTemp(game.cmdr.scanOne.entryId);
            this.lastScanOneEntryId = game.cmdr.scanOne?.entryId;

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
                    RES("Header", game.systemBody.bioSignalCount, game.systemBody.countAnalyzedBioSignals),
                    GameColors.fontSmall, GameColors.brushGameOrange, four, eight);

                var organism = scanOne == null ? null : game.systemBody?.organisms?.FirstOrDefault(_ => _.species == scanOne.species);
                if (organism == null)
                {
                    this.showAllGenus(g);

                    // warn if scan is from another body
                    if (scanOne?.body != null && scanOne.body != game.systemBody?.name)
                    {
                        var matchGenus = Game.codexRef.matchFromGenus(scanOne.genus);
                        var sz = this.drawFooterText(g, RES("WarningStaleScans", matchGenus?.locName, scanOne.body), GameColors.brushRed);

                        var y = ClientSize.Height - twenty;
                        var w = (ClientSize.Width - sz.Width - oneSix) / 2;
                        g.FillRectangle(GameColors.brushShipDismissWarning, four, y, w, oneFour);
                        g.FillRectangle(GameColors.brushShipDismissWarning, ClientSize.Width - four - w, y, w, oneFour);
                    }
                }
                else
                    this.showCurrentGenus(g, organism);

                this.drawValueCompletion(g);
            }
            else
            {
                // show a message if cmdr forgot to DSS the body
                var msg = RES("DssRequired");
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
                    this.drawFooterText(g, RES("FooterAllScannedFF"));
                else if (allScanned)
                    this.drawFooterText(g, RES("FooterAllScanned"), GameColors.brushGameOrange);
                else if (this.lastCodexScan != null)
                {
                    this.drawFooterText(g, this.lastCodexScan, GameColors.brushCyan);
                    if (lastEntryId != null && !Game.settings.tempRange_TEST)
                        this.drawHasImage(g, this.Width - threeSix, this.Height - threeSix);
                }
                else if (game.systemBody.firstFootFall && Random.Shared.NextDouble() > 0.5d)
                    this.drawFooterText(g, RES("FooterApplyFF"), GameColors.brushCyan);
                //else if (!game.systemBody.wasMapped && game.systemBody.countAnalyzedBioSignals == 0 && Game.settings.useExternalData && Game.settings.autoLoadPriorScans && Program.getPlotter<PlotPriorScans>() == null)
                //    this.drawFooterText(g, "Potential first footfall - send '.ff' to confirm", GameColors.brushCyan);
                else
                    this.drawFooterText(g, RES("FooterUseCompScanner"));
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
            var txt = organism.variantLocalized ?? ""; // or species?
            var x = oneOhFour;
            var rr = new RectangleF(x, y - eight, this.Width - x - eight - ImageResources.picture.Width, forty);

            var f = GameColors.fontBig;
            var sz = g.MeasureString(txt, f, (int)rr.Width);
            if (sz.Height > rr.Height) f = GameColors.font18;
            sz = g.MeasureString(txt, f, (int)rr.Width);
            if (sz.Height > rr.Height) f = GameColors.font16;
            sz = g.MeasureString(txt, f, (int)rr.Width);
            if (sz.Height > rr.Height) f = GameColors.font14;

            rr.Height += ten;
            g.DrawString(
                txt,
                f, GameColors.brushCyan,
                rr); // x, y - eight);

            // Reward
            if (organism.reward > 0)
            {
                var reward = game.systemBody!.firstFootFall ? organism.reward * 5 : organism.reward;
                var txt2 = Util.credits(reward);
                if (game.systemBody.firstFootFall) txt2 += " " + RES("SuffixFF");
                g.DrawString(
                    txt2,
                    GameColors.fontSmall, GameColors.brushCyan,
                    four, sixTwo);
            }

            this.drawScale(g, organism.range);

            if (Game.settings.tempRange_TEST)
            {
                if (this.temperatureClause?.min != null && this.temperatureClause?.max != null)
                    this.tempRangeDiffs?.renderWithinRange(g, temperatureClause.min.Value, temperatureClause.max.Value);
                else
                    this.tempRangeDiffs?.renderBodyOnly(g);
            }
            else if (lastEntryId != null)
                this.drawHasImage(g, this.Width - threeFour, twoFour);
        }

        private void drawHasImage(Graphics g, float x, float y)
        {
            g.DrawIcon(ImageResources.picture, (int)x, (int)y);
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
            var x = this.Width - pad - txtSz.Width - ten;
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
            var txt = $" {percent:N0}%";
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width - ten;
            var y = pad;

            var b = percent < 100 ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, GameColors.fontSmall, b,
                x, y,
                StringFormat.GenericTypographic);

            float length = hundred;
            //var scannedLength = 20; // ratio * data.sumAnalyzed;

            x = this.Width - pad - txtSz.Width - length - ten;
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
            var body = game.systemBody;

            if (body?.organisms == null || body.organisms.Count == 0)
            {
                Game.log($"Why is game.systemBody!.organisms NULL ??");
                g.DrawString(
                    RES("SomethingWrong"),
                    GameColors.fontSmall,
                    Brushes.OrangeRed,
                    x, y);

                return;
            }

            var allScanned = true;
            foreach (var organism in body.organisms)
            {
                // TODO: use a widget
                allScanned &= organism.analyzed;
                var txt = organism.genusLocalized ?? organism.bioMatch?.genus.locName;
                if (txt == null && organism.variantLocalized != null) txt = Util.getGenusDisplayNameFromVariant(organism.variantLocalized); // TODO: <-- revisit!
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
                    var ly = (int)(y + sz.Height * .35f);
                    g.DrawLine(GameColors.penGameOrange1, x, ly, x + sz.Width, ly);
                    g.DrawLine(GameColors.penGameOrangeDim1, x + 1, ly + 1, x + sz.Width + 1, ly + 1);
                }

                x += sz.Width + eight;
            }

            // geo signals?
            if (body.geoSignalCount > 0 && Debugger.IsAttached)
            {
                var n = 0;
                while (n < body.geoSignalCount)
                {
                    // TODO: use a widget
                    var txt = RES("GeoN", n + 1);

                    var sz = g.MeasureString(txt, GameColors.fontSmall);
                    if (x + sz.Width > this.Width - oneSix)
                    {
                        x = twoFour;
                        y += sz.Height;
                    }

                    g.DrawString(
                        txt,
                        GameColors.fontSmall,
                        true ? GameColors.brushGameOrange : GameColors.brushCyan,
                        x, y
                    );

                    if (n + 1 <= body.geoSignals?.Count)
                    {
                        // strike-through if already analyzed
                        var ly = (int)(y + sz.Height * .35f);
                        g.DrawLine(GameColors.penGameOrange1, x, ly, x + sz.Width, ly);
                        g.DrawLine(GameColors.penGameOrangeDim1, x + 1, ly + 1, x + sz.Width + 1, ly + 1);
                    }

                    x += sz.Width + eight;
                    n++;
                }
            }

            if (Game.settings.tempRange_TEST)
                this.tempRangeDiffs?.renderBodyOnly(g);
        }

        protected SizeF drawFooterText(Graphics g, string msg, Brush? brush = null)
        {
            if (g == null) return SizeF.Empty;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            var mid = this.Size / 2;
            var font = GameColors.fontSmall;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 5;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
            return sz;
        }

        private void drawTemperatureBar(long entryId)
        {
            if (game.systemBody == null) return;
            var bodyDefaultTemp = (float)game.systemBody.surfaceTemperature;
            var top = twoFour;

            var h = 40f;
            var x = this.ClientSize.Width - ten;
            g.DrawLine(GameColors.penGameOrange1, x, top, x, top + (h / 2));

            var y1 = 20f;

            // temp range for target organism
            if (this.temperatureClause?.min != null && this.temperatureClause?.max != null)
            {
                var min = temperatureClause.min.Value;
                var max = temperatureClause.max.Value;

                // red and blue bars are fixed at either end of the line
                g.DrawLine(GameColors.penRed2, x - ten, top, x + two, top);
                g.DrawLine(GameColors.penCyan2, x - ten, top + h, x + two, top + h);

                // draw other lines between those relative to the temperature range
                var tempRange = max - min;
                var dTemp = (h / tempRange);

                // "default" surface temperature
                var dD = bodyDefaultTemp - min;
                var yD = top + h - (dD * dTemp);
                g.DrawLine(GameColors.penGameOrange2, x - five, yD, x + five, yD);

                // current cmdr's temp (if outside on foot)
                if (game.status.Temperature > 0)
                {
                    var dCmdr = bodyDefaultTemp - min;
                    var yCmdr = top + h - (dCmdr * dTemp);
                    g.DrawLine(GameColors.penGameOrange2, x - five, yCmdr, x + five, yCmdr);
                }

                return;
            }

            // temp at cmdr's location
            if (game.status.Temperature > 0)
            {
                // relative line for current live temp
                var y2 = y1 - (float)(game.status.Temperature - game.systemBody.surfaceTemperature);
                g.DrawLine(GameColors.penYellow4, x - two, top + y2, x + five, top + y2);
            }

            // base line for body "surface temp"
            g.DrawLine(GameColors.penGameOrange2, x - five, top + y1, x + five, top + y1);
        }

        private void prepTemp(long entryId)
        {
            var match = Game.codexRef.matchFromEntryId(entryId);

            var clauses = BioPredictor.predictTarget(game!.systemBody!, match.variant.englishName);
            this.temperatureClause = clauses.FirstOrDefault(c => c?.property == "temp");
            Game.log(temperatureClause);

            if (game.systemBody == null || temperatureClause == null) return;
            var bodyDefaultTemp = game.systemBody.surfaceTemperature;
            if (bodyDefaultTemp < temperatureClause.min || bodyDefaultTemp > temperatureClause.max)
            {
                Game.log($"Unexpected!\r\nBody surface temperature: {bodyDefaultTemp}\r\n{entryId} min temp: {temperatureClause.min}\r\n{entryId} max temp: {temperatureClause.max}");
                Debugger.Break(); // does this ever happen?
            }
        }
    }
}
