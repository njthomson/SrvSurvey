using BioCriterias;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Diagnostics;
using Res = Loc.PlotBioStatus;

namespace SrvSurvey.plotters
{
    internal partial class PlotBioStatus : PlotBase2
    {
        #region def + statics

        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotBioStatus),
            allowed = allowed,
            ctor = (game, def) => new PlotBioStatus(game, def),
            defaultSize = new Size(480, 80),
        };

        public static bool allowed(Game game)
        {
            return (SystemData.isWithinLastDssDuration() || Game.settings.autoShowBioSummary)
                && game.systemBody != null
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && game.systemBody.bioSignalCount > 0
                //&& (game.systemStation == null || !Game.settings.autoShowHumanSitesTest)
                && !game.hidePlottersFromCombatSuits
                && !game.status.Docked
                && !game.isShutdown
                && !game.atMainMenu
                && !game.status.InTaxi
                && !game.status.FsdChargingJump
                && (!Game.settings.enableGuardianSites || !PlotGuardians.allowed(game))
                && (!Game.settings.autoShowHumanSitesTest || !PlotHumanSite.allowed(game))
                && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.CommsPanel, GameMode.SAA, GameMode.Codex);
        }

        #endregion

        private string? lastCodexScan;
        public static string? lastEntryId;
        private bool hasImage;
        private double lastTemp;
        private long? lastScanOneEntryId;
        private Clause? temperatureClause;
        private TempRangeDiffs? tempRangeDiffs;

        private PlotBioStatus(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;

            if (game.cmdr.scanOne?.entryId > 0)
            {
                lastEntryId = game.cmdr.scanOne.entryId.ToString();

                var match = Game.codexRef.matchFromEntryId(lastEntryId);
                this.hasImage = match.variant.imageUrl != null;
            }

            tempRangeDiffs = new TempRangeDiffs(width - N.ten, N.twoFour);
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (this.lastTemp != status.Temperature) this.invalidate();
            this.lastTemp = status.Temperature;

            /* still needed? I think not
            // hide ourselves whilst FSD is charging to jump systems
            var allowed = PlotBioStatus.allowed(game);
            if ((status.FsdChargingJump || !allowed || !Elite.gameHasFocus) && !Debugger.IsAttached)
                this.hide();
            else if (this.hidden && allowed && Elite.gameHasFocus)
                this.show(); */

            if (status.changed.Contains("mode"))
            {
                if (game.cmdr.scanOne?.entryId > 0 && this.lastScanOneEntryId != game.cmdr.scanOne.entryId && Debugger.IsAttached)
                    this.prepTemp(game.cmdr.scanOne.entryId);

                this.lastScanOneEntryId = game.cmdr.scanOne?.entryId;

                /* still needed? I think not
                if (game.systemBody == null || game.systemBody.bioSignalCount == 0)
                    PlotBase2.remove(PlotBioStatus.plotDef);*/
            }
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
            }

            this.invalidate();
        }

        protected override void onJournalEntry(ScanOrganic entry)
        {
            this.lastCodexScan = null;
            var match = Game.codexRef.matchFromVariant(entry.Variant);
            lastEntryId = match.entryId.ToString();
            this.hasImage = match.variant.imageUrl != null;

            this.invalidate();
        }

        protected override void onJournalEntry(SendText entry)
        {
            var msg = entry.Message.ToLowerInvariant();

            // show picture, if we have an entryId
            if (msg == MsgCmd.show && lastEntryId != null)
                BaseForm.show<FormShowCodex>();
        }

        #endregion

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (game?.systemBody == null) return frame.Size;
            tt.padVertical = 6;
            tt.dty = N.eight;

            var scanOne = game.cmdr.scanOne;
            if (game.systemBody.organisms?.Count > 0)
            {
                tt.draw(N.four, Res.Header.format(game.systemBody.bioSignalCount, game.systemBody.countAnalyzedBioSignals));

                var organism = scanOne == null ? null : game.systemBody?.organisms?.FirstOrDefault(_ => _.species == scanOne.species);
                if (organism == null)
                {
                    this.showAllGenus(g, tt);

                    // warn if scan is from another body
                    if (scanOne?.body != null && scanOne.body != game.systemBody?.name)
                    {
                        var matchGenus = Game.codexRef.matchFromGenus(scanOne.genus);
                        var sz = tt.drawFooter(Res.WarningStaleScans.format(matchGenus?.locName, scanOne.body), C.red);

                        var y = height - N.twenty;
                        var w = (width - sz.Width - N.oneSix) / 2;
                        g.FillRectangle(GameColors.brushShipDismissWarning, N.four, y, w, N.oneFour);
                        g.FillRectangle(GameColors.brushShipDismissWarning, width - N.four - w, y, w, N.oneFour);
                    }
                }
                else
                    this.showCurrentGenus(g, tt, organism);

                this.drawValueCompletion(g, tt);
            }
            else
            {
                // show a message if cmdr forgot to DSS the body
                tt.drawCentered(N.oneSix, Res.DssRequired, C.cyan, GameColors.fontMiddle);
            }

            if (scanOne == null)
            {
                var allScanned = game.systemBody!.countAnalyzedBioSignals == game.systemBody.bioSignalCount;
                if (allScanned && game.systemBody.firstFootFall)
                    tt.drawFooter(Res.FooterAllScannedFF);
                else if (allScanned)
                    tt.drawFooter(Res.FooterAllScanned);
                else if (this.lastCodexScan != null)
                {
                    tt.drawFooter(this.lastCodexScan, C.cyan);
                    if (lastEntryId != null && !Game.settings.tempRange_TEST)
                        this.drawHasImage(g, this.width - N.threeSix, this.height - N.threeSix);
                }
                else if (game.systemBody.firstFootFall && Random.Shared.NextDouble() > 0.5d)
                    tt.drawFooter(Res.FooterApplyFF, C.cyan);
                else
                    tt.drawFooter(Res.FooterUseCompScanner);
            }

            return PlotBioStatus.plotDef.defaultSize;
        }

        private void showCurrentGenus(Graphics g, TextCursor tt, SystemOrganism organism)
        {
            float y = N.twoEight;

            // left circle - always filled
            var r = new RectangleF(N.eight, y, N.twoFour, N.twoFour);
            g.FillEllipse(GameColors.brushGameOrangeDim, r);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // middle circle - filled after scan two
            r = new RectangleF(N.forty, y, N.twoFour, N.twoFour);
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
            r = new RectangleF(N.sevenTwo, y, N.twoFour, N.twoFour);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // Species name
            var txt = organism.variantLocalized ?? ""; // or species?
            var x = N.oneOhFour;
            var rr = new RectangleF(x, y - N.eight, this.width - x - N.eight - ImageResources.picture.Width, N.forty);

            var f = GameColors.fontBig;
            var sz = g.MeasureString(txt, f, (int)rr.Width);
            if (sz.Height > rr.Height) f = GameColors.font18;
            sz = g.MeasureString(txt, f, (int)rr.Width);
            if (sz.Height > rr.Height) f = GameColors.font16;
            sz = g.MeasureString(txt, f, (int)rr.Width);
            if (sz.Height > rr.Height) f = GameColors.font14;

            rr.Height += N.ten;
            tt.dty = rr.Top;
            tt.drawWrapped(rr.Left, txt, C.cyan, f);

            // Reward
            if (organism.reward > 0)
            {
                var reward = game.systemBody!.firstFootFall ? organism.reward * 5 : organism.reward;
                var txt2 = Util.credits(reward);
                if (game.systemBody.firstFootFall) txt2 += " " + Res.SuffixFF;
                tt.dty = N.sixTwo;
                tt.draw(N.four, txt2, C.cyan);
            }

            this.drawScale(g, tt, organism.range);

            if (Game.settings.tempRange_TEST)
            {
                if (this.temperatureClause?.min != null && this.temperatureClause?.max != null)
                    this.tempRangeDiffs?.renderWithinRange(g, temperatureClause.min.Value, temperatureClause.max.Value);
                else
                    this.tempRangeDiffs?.renderBodyOnly(g);
            }
            else if (lastEntryId != null)
                this.drawHasImage(g, this.width - N.threeFour, N.twoFour);
        }

        private void drawHasImage(Graphics g, float x, float y)
        {
            g.DrawIcon(ImageResources.picture, (int)x, (int)y);
            if (!this.hasImage)
            {
                y += N.two;
                g.DrawLine(GameColors.penDarkRed4, x, y, x + N.thirty, y + N.twoEight);
                g.DrawLine(GameColors.penDarkRed4, x, y + N.twoEight, x + N.thirty, y);
            }
        }

        private void drawScale(Graphics g, TextCursor tt, float dist)
        {
            var txt = Util.metersToString(dist);

            tt.dty = this.height - N.ten - N.ten; // last 10 is for text height
            var sz = tt.drawRight(this.width - N.oneEight, txt, C.cyan);

            var x = tt.dtx - sz.Width - N.ten;
            var y = tt.dty + N.five;

            var bar = PlotBase.scaled(dist * 0.25f);

            g.DrawLine(GameColors.penCyan2, x, y, x - bar, y); // bar
            g.DrawLine(GameColors.penCyan2, x, y - N.four, x, y + N.four); // right edge
            g.DrawLine(GameColors.penCyan2, x - bar, y - N.four, x - bar, y + N.four); // left edge
        }

        private void drawValueCompletion(Graphics g, TextCursor tt)
        {
            // use a simpler percentage
            var percent = 100.0f / (float)game.systemBody!.bioSignalCount * (float)game.systemBody.countAnalyzedBioSignals;
            var txt = $" {percent:N0}%";

            var b = percent < 100 ? GameColors.brushCyan : GameColors.brushGameOrange;

            tt.dty = N.eight;
            var sz = tt.drawRight(this.width - N.oneEight, txt, b.Color);

            float length = N.hundred;
            var x = tt.dtx - length - sz.Width - N.eight;
            var y = tt.dty + N.five;

            // known un-scanned - solid blue line
            g.DrawLine(GameColors.penCyan4, x, y, x + length, y);

            // already scanned value - orange bar
            g.FillRectangle(GameColors.brushGameOrange, x, N.nine, PlotBase.scaled(percent), N.ten);

            // active scan organism value - solid blue bar
            if (game.cmdr.scanOne != null)
                g.FillRectangle(GameColors.brushCyan, x + PlotBase.scaled(percent), N.ten - 0.5f, length / (float)game.systemBody!.bioSignalCount, N.eight);
        }

        private void showAllGenus(Graphics g, TextCursor tt)
        {
            // all the Genus names
            tt.dtx = N.twoFour;
            tt.dty = N.twoTwo;
            var body = game.systemBody;

            if (body?.organisms == null || body.organisms.Count == 0) return;

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

                var sz = TextRenderer.MeasureText(g, txt, this.font);
                if (tt.dtx + sz.Width > this.width - N.oneSix)
                {
                    tt.dtx = N.twoFour;
                    tt.newLine();
                }

                tt.draw(txt, organism.analyzed ? C.orange : C.cyan);

                // strike-through if already analyzed
                if (organism.analyzed)
                    tt.strikeThroughLast();

                tt.dtx += N.eight;
            }

            // geo signals?
            if (body.geoSignalCount > 0 && !Game.settings.hideGeoCountInBioSystem)
            {
                var n = 0;
                while (n < body.geoSignalCount)
                {
                    // TODO: use a widget
                    var txt = Res.GeoN.format(n + 1);

                    var sz = TextRenderer.MeasureText(g, txt, this.font);
                    if (tt.dtx + sz.Width > this.width - N.oneSix)
                    {
                        tt.dtx = N.twoFour;
                        tt.newLine();
                    }

                    tt.draw(txt);

                    // strike-through if already scanned
                    if (n + 1 <= body.geoSignalNames?.Count)
                        tt.strikeThroughLast();

                    tt.dtx += N.eight;
                    n++;
                }
            }

            if (Game.settings.tempRange_TEST)
                this.tempRangeDiffs?.renderBodyOnly(g);
        }

        /*
        private void drawTemperatureBar(Graphics g, long entryId)
        {
            if (game.systemBody == null) return;
            var bodyDefaultTemp = (float)game.systemBody.surfaceTemperature;
            var top = N.twoFour;

            var h = 40f;
            var x = this.width - N.ten;
            g.DrawLine(GameColors.penGameOrange1, x, top, x, top + (h / 2));

            var y1 = 20f;

            // temp range for target organism
            if (this.temperatureClause?.min != null && this.temperatureClause?.max != null)
            {
                var min = temperatureClause.min.Value;
                var max = temperatureClause.max.Value;

                // red and blue bars are fixed at either end of the line
                g.DrawLine(GameColors.penRed2, x - N.ten, top, x + N.two, top);
                g.DrawLine(GameColors.penCyan2, x - N.ten, top + h, x + N.two, top + h);

                // draw other lines between those relative to the temperature range
                var tempRange = max - min;
                var dTemp = (h / tempRange);

                // "default" surface temperature
                var dD = bodyDefaultTemp - min;
                var yD = top + h - (dD * dTemp);
                g.DrawLine(GameColors.penGameOrange2, x - N.five, yD, x + N.five, yD);

                // current cmdr's temp (if outside on foot)
                if (game.status.Temperature > 0)
                {
                    var dCmdr = bodyDefaultTemp - min;
                    var yCmdr = top + h - (dCmdr * dTemp);
                    g.DrawLine(GameColors.penGameOrange2, x - N.five, yCmdr, x + N.five, yCmdr);
                }

                return;
            }

            // temp at cmdr's location
            if (game.status.Temperature > 0)
            {
                // relative line for current live temp
                var y2 = y1 - (float)(game.status.Temperature - game.systemBody.surfaceTemperature);
                g.DrawLine(GameColors.penYellow4, x - N.two, top + y2, x + N.five, top + y2);
            }

            // base line for body "surface temp"
            g.DrawLine(GameColors.penGameOrange2, x - N.five, top + y1, x + N.five, top + y1);
        }
        */

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
