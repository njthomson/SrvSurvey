using SrvSurvey.game;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotBioSystem : PlotBase, PlotterForm
    {
        private string? lastDestination;
        private SystemBody? durationBody;
        private System.Windows.Forms.Timer durationTimer;
        private float durationCount;
        private float durationTotal;

        private PlotBioSystem() : base()
        {
            this.Font = GameColors.fontSmall;
            this.lastDestination = game.status.Destination?.Name;

            durationTimer = new System.Windows.Forms.Timer()
            {
                Interval = 25,
                Enabled = false,
            };
            durationTimer.Tick += DurationTimer_Tick;
        }

        public override bool allow { get => PlotBioSystem.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            // Size set during paint

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        public static SystemBody? targetBody
        {
            get
            {
                var game = Game.activeGame;
                if (game == null || game.isShutdown)
                    return null;

                // assuming we're not in any of these modes ...
                if (game.isMode(GameMode.ExternalPanel, GameMode.SystemMap, GameMode.Orrery))
                    return null;

                SystemBody? body = null;
                var targetBody = game.targetBody;

                if (!Game.settings.drawBodyBiosOnlyWhenNear)
                    body = targetBody ?? game.systemBody; // new behaviour: use target body, or local if no target
                else if (targetBody == null || targetBody == game.systemBody)
                    body = game.systemBody; // old behaviour: use local body, unless target body is something and different (then use use system mode)

                // ignore body if it has no bio signals
                if (body != null && body.bioSignalCount == 0)
                    body = null;

                return body;
            }
        }

        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotBioSystem
                && Game.activeGame?.status != null
                && Game.activeGame.systemData != null
                && Game.activeGame.systemData.bioSignalsTotal > 0
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.status.OnFootSocial
                && !Game.activeGame.hidePlottersFromCombatSuits
                && (
                    Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                    || (
                        PlotBioSystem.targetBody?.bioSignalCount > 0
                        && Game.activeGame.isMode(GameMode.GlideMode, GameMode.Flying, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel, GameMode.InSrv, GameMode.RolePanel)
                    )
                );
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;
            base.Status_StatusChanged(blink);

            if (!Game.settings.drawBodyBiosOnlyWhenNear && this.durationTimer.Enabled && game.mode != GameMode.ExternalPanel)
            {
                this.stopTimer();
                this.Invalidate();
            }

            // if the targetBody has recent changed ...
            var targetBody = game.targetBody;
            if (game.status.Destination?.Name != this.lastDestination)
            {
                this.lastDestination = game.status.Destination?.Name;
                if (this.durationTimer.Enabled)
                {
                    Game.log($"Kill duration timer, from: {this.durationBody?.name}");
                    this.durationTimer.Stop();
                    this.durationBody = null;
                }

                // ... and it has bio signals ...
                if (targetBody?.bioSignalCount > 0)
                {
                    // ... show it's details for ~5 seconds
                    this.durationBody = targetBody;
                    var signalCount = targetBody.predictions.Count;
                    if (signalCount == 0 && targetBody.organisms != null)
                        signalCount = targetBody.organisms.Count;
                    this.durationTotal = 2_000 * signalCount;
                    this.durationCount = this.durationTotal;
                    this.durationTimer.Start();
                    Game.log($"Start duration timer, for: {this.durationBody?.name}");
                    this.Invalidate();
                }
            }
        }

        private void stopTimer()
        {
            this.durationCount = 0;
            this.durationTimer.Stop();
            this.durationBody = null;
        }

        private void DurationTimer_Tick(object? sender, EventArgs e)
        {
            this.durationCount -= this.durationTimer.Interval;
            //Game.log($"Poll duration timer, from: {this.durationBody?.name} ({this.durationCount})");

            if (this.durationCount <= 0)
                this.stopTimer();

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || game.systemData == null || game.status == null || !PlotBioSystem.allowPlotter)
            {
                this.Opacity = 0;
                return;
            }

            var body = PlotBioSystem.targetBody;

            // if we're FSS'ing - show predictions for the last body scanned, if it has any bio signals
            if (game.mode == GameMode.FSS && game.systemData.lastFssBody != null)
            {
                if (game.systemData.lastFssBody.bioSignalCount > 0)
                    body = game.systemData.lastFssBody;
                else
                    body = null;
            }

            // keep showing the body we recently switched to, if within ~5 seconds
            if (this.durationBody != null && this.durationTimer.Enabled)
                body = this.durationBody;

            if (body != null)
                this.drawBodyBios2(body);
            else
                this.drawSystemBios2();
        }

        private void drawBodyBios2(SystemBody body)
        {
            drawTextAt(eight, $"Body {body.shortName} bio signals: {body.bioSignalCount}", GameColors.brushGameOrange);
            newLine(+eight, true);

            if (this.durationTimer.Enabled && this.durationCount > 0)
                this.drawTimerBar();

            if (body.organisms == null)
            {
                if (!body.dssComplete)
                {
                    dty -= four;
                    this.drawTextAt(ten, $"DSS required", GameColors.brushCyan);
                    newLine(+four, true);
                }

                this.drawBodyPredictions(body);
            }
            else
            {
                // draw a row for each organism
                var first = true;
                foreach (var organism in body.organisms)
                {
                    var highlight = !organism.analyzed && (game.cmdr.scanOne?.genus == organism.genus || game.cmdr.scanOne?.genus == null);
                    var brush = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;

                    var predictions = body.predictions.Values.Where(p => p.species.genus.name == organism.genus).ToList();
                    var potentialFirstDiscovery = predictions.Any(p => !game.cmdrCodex.isDiscovered(p.entryId));

                    // do we already know if this is a first discovery?
                    string? discoveryPrefix = null;
                    if (organism.isCmdrFirst) discoveryPrefix = "⚑";
                    else if (organism.isNewEntry) discoveryPrefix = "⚐";

                    dty = (int)dty;

                    if (first)
                        first = false;
                    else
                        g.DrawLine(GameColors.penGameOrangeDim1, eight, dty - four, this.ClientSize.Width - eight, dty - four);

                    // if we have predictions - use that renderer
                    if (organism.variant == null && predictions.Count > 0)
                    {
                        var genus = Game.codexRef.matchFromGenus(organism.genus)!;
                        drawBodyPredictionsRow(genus, body.genusPredictions[genus], highlight);
                        continue;
                    }

                    var yy = dty;

                    // displayName is either genus, or species/variant without the genus prefix
                    var displayName = organism.genusLocalized;
                    if (!string.IsNullOrEmpty(organism.variantLocalized))
                    {
                        displayName = organism.variantLocalized.Replace(organism.genusLocalized + " ", "");
                        if (displayName.EndsWith("Anemone")) displayName = displayName.Replace("Anemone", "");
                        if (displayName.EndsWith("Brain Tree")) displayName = displayName.Replace("Brain Tree", "");
                    }
                    else if (!string.IsNullOrEmpty(organism.speciesLocalized))
                    {
                        displayName = organism.speciesLocalized.Replace(organism.genusLocalized + " ", "");
                        foreach (var tail in legacyTailTrimList)
                            if (displayName.EndsWith(tail)) displayName = displayName.Replace(tail, "");
                    }
                    else
                    {
                        /*
                        // if we have a matching prediction - show the variant name without the genus prefix
                        if (predictions.Count < 0)
                        {
                            var genus = Game.codexRef.matchFromGenus(organism.genus)!;
                            drawBodyPredictionsRow(genus, body.genusPredictions[genus], highlight);
                            // maybe?
                        }
                        else if (predictions.Count > 0)
                        {
                            var firstColor = true;
                            var lastSpecies = "";
                            foreach (var match in predictions)
                            {
                                string? prefix = null;
                                if (!game.cmdrCodex.isDiscovered(match.entryId))
                                {
                                    prefix = "⚑";
                                    discoveryPrefix = "⚑";
                                }
                                else if (!game.cmdrCodex.isDiscoveredInRegion(match.entryId, game.cmdr.galacticRegion))
                                {
                                    prefix = "⚐";
                                    if (discoveryPrefix == null) discoveryPrefix = "⚐";
                                }

                                var isLegacy = !match.species.genus.odyssey;
                                if (isLegacy)
                                {
                                    if (match.species.name != lastSpecies && lastSpecies != "")
                                    {
                                        this.drawTextAt("?", brush);
                                        newLine(+one, true);
                                    }
                                    var legacyEnglishName = match.species.englishName;
                                    if (legacyEnglishName.EndsWith("Anemone")) legacyEnglishName = legacyEnglishName.Replace("Anemone", "");
                                    if (legacyEnglishName.EndsWith("Brain Tree")) legacyEnglishName = legacyEnglishName.Replace("Brain Tree", "");
                                    if (prefix != null)
                                        this.drawTextAt(twoEight, prefix + legacyEnglishName, highlight ? brush : Brushes.Gold);
                                    else
                                        this.drawTextAt(twoEight, legacyEnglishName, brush);

                                    lastSpecies = match.species.name;
                                    continue;
                                }

                                if (match.species.name != lastSpecies)
                                {
                                    if (lastSpecies != "")
                                    {
                                        this.drawTextAt("?", brush);
                                        newLine(+one, true);
                                    }
                                    var speciesName = match.species.englishName.Replace(match.species.genus.englishName, "").Trim() + ":";
                                    this.drawTextAt(twoEight, speciesName, brush);
                                    lastSpecies = match.species.name;
                                    firstColor = true;
                                }

                                if (firstColor)
                                    firstColor = false;
                                else
                                {
                                    dtx -= four;
                                    this.drawTextAt(",", brush);
                                }

                                dtx -= two;
                                if (prefix != null)
                                    this.drawTextAt(prefix + match.colorName, highlight ? brush : Brushes.Gold);
                                else
                                    this.drawTextAt(match.colorName, brush);
                            }
                            this.drawTextAt("?", brush);

                            // TODO: show colour blocks or something?
                            displayName = ""; // string.Join("/", matches.Select(m => m.colorName.Substring(0, 2)));
                        }
                        */
                    }

                    var minReward = body.getBioRewardForGenus(organism, true);
                    var maxReward = body.getBioRewardForGenus(organism, false);
                    drawVolumeBars(g, oneTwo, yy + oneSix, highlight, minReward, maxReward, shouldBeGold(discoveryPrefix));

                    // line 1
                    if (displayName.Length > 0)
                        this.drawTextAt(twoEight, displayName, brush);
                    if (organism.analyzed)
                    {
                        // strike-through if already analyzed
                        var y = dty + six;
                        g.DrawLine(GameColors.penGameOrange1, twoEight, y, dtx, y);
                        g.DrawLine(GameColors.penGameOrangeDim1, twoEight + 1, y + 1, dtx + 1, y + 1);
                        //g.DrawLine(GameColors.penGameOrange1, twoEight, y, this.ClientSize.Width - oneTwo, y);
                        //g.DrawLine(GameColors.penGameOrangeDim1, twoEight + 1, y + 1, this.ClientSize.Width - oneTwo + 1, y + 1);
                    }
                    newLine(+one, true);

                    // 2nd line - right
                    var sz = drawTextAt(
                        this.ClientSize.Width - ten,
                        Util.getMinMaxCredits(minReward, maxReward),
                        highlight ? GameColors.brushCyan : GameColors.brushGameOrange,
                        null, true);

                    // 2nd line - left
                    brush = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                    if (!highlight && shouldBeGold(discoveryPrefix)) brush = (SolidBrush)Brushes.Gold;

                    dtx = twoEight;
                    if (discoveryPrefix != null)
                        drawTextAt(discoveryPrefix, shouldBeGold(discoveryPrefix) ? Brushes.Gold : brush);

                    var leftText = displayName != organism.genusLocalized || organism.entryId > 0
                        ? organism.genusLocalized
                        : "?";
                    drawTextAt(leftText, brush);
                    dtx += sz.Width + ten;
                    newLine(+eight, true);
                }
            }

            // summary footer
            this.dty += two;
            var footerTxt = $"Rewards: {body.getMinMaxBioRewards(false)}";
            //            if (body.firstFootFall) footerTxt += "\r\n(Applying FF bonus)";
            drawTextAt(eight, footerTxt, GameColors.brushGameOrange);
            newLine(true);

            if (body.firstFootFall)
            {
                this.dty += two;
                drawTextAt(this.Width - eight, $"(FF bonus: {body.getMinMaxBioRewards(true)})", GameColors.brushCyan, null, true);
                dtx = lastTextSize.Width;
                newLine(true);
            }

            // resize window as necessary
            formAdjustSize(+ten, +ten);
        }

        private void drawTimerBar()
        {
            var r = 1f / this.durationTotal * this.durationCount;

            var x = two;
            var y = this.Height - one;
            var w = (this.Width - x - x);
            //Game.log($"r: {r}, w: {w}");
            g.DrawLine(GameColors.penGameOrangeDim2, x, y, x + w * r, y);
            g.DrawLine(GameColors.penGameOrangeDim2, x, one, x + w * r, one);

            //var x = four;
            //var y = ClientSize.Height - oneTwo;
            //var h = this.Height - twoFour;
            //g.DrawLine(GameColors.penGameOrangeDim4, x, y, x, y - h * r);

            this.formGrow(false, true);
        }

        private bool shouldBeGold(string? prefix)
        {
            return prefix == "⚑"
                || (prefix == "⚐" && Game.settings.highlightRegionalFirsts);
        }

        private void drawBodyPredictions(SystemBody body)
        {
            if (body.predictions == null || body.predictions.Count == 0) return;

            var first = true;
            foreach (var genus in body.genusPredictions)
            {
                if (first)
                    first = false;
                else
                    g.DrawLine(GameColors.penGameOrangeDim1, eight, dty - six, this.ClientSize.Width - eight, dty - six);

                drawBodyPredictionsRow(genus.Key, genus.Value, false);
            }
        }

        private static List<string> legacyTailTrimList = new List<string>()
        {
            "Anemone", "Brain Tree", "Sinuous Tubers"
        };

        private void drawBodyPredictionsRow(BioGenus genus, Dictionary<BioSpecies, List<BioVariant>> predictedSpecies, bool highlight)
        {
            var yy = dty;
            var min = 20_000_000L;
            var max = 0L;
            var genusName = genus.englishName;
            string? genusPrefix = null;
            Brush b;
            foreach (var species in predictedSpecies)
            {
                var isLegacy = !species.Key.genus.odyssey;
                b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                if (isLegacy)
                {
                    //var legacyEnglishName = species.Key.englishName;
                    //if (legacyEnglishName.EndsWith("Anemone")) legacyEnglishName = legacyEnglishName.Replace("Anemone", "");
                    //if (legacyEnglishName.EndsWith("Brain Tree")) legacyEnglishName = legacyEnglishName.Replace("Brain Tree", "");
                    //if (legacyEnglishName.EndsWith("Sinuous Tubers")) legacyEnglishName = legacyEnglishName.Replace("Sinuous Tubers", "");

                    // ---
                    //if (!game.cmdrCodex.isDiscovered(match.entryId))
                    //{
                    //    prefix = "⚑";
                    //    discoveryPrefix = "⚑";
                    //}
                    //else if (!game.cmdrCodex.isDiscoveredInRegion(match.entryId, game.cmdr.galacticRegion))
                    //{
                    //    prefix = "⚐";
                    //    if (discoveryPrefix == null) discoveryPrefix = "⚐";
                    //}

                    // ---
                    dtx = twoEight;
                    //drawTextAt(twoEight, legacyEnglishName, b);
                }
                else
                {
                    var speciesName = species.Key.englishName.Replace(genusName, "").Trim();
                    drawTextAt(twoEight, $"{speciesName}:", b);
                }

                if (species.Key.reward < min) min = species.Key.reward;
                if (species.Key.reward > max) max = species.Key.reward;

                // TODO: handle legacy species better - put them on the same line as if they were variants
                foreach (var variant in species.Value)
                {
                    b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                    if (!game.cmdrCodex.isDiscovered(variant.entryId))
                    {
                        if (!highlight) b = Brushes.Gold;
                        drawTextAt("⚑", Brushes.Gold);
                        genusPrefix = "⚑";
                    }
                    else if (!game.cmdrCodex.isDiscoveredInRegion(variant.entryId, game.cmdr.galacticRegion))
                    {
                        if (!highlight && Game.settings.highlightRegionalFirsts) b = Brushes.Gold;
                        drawTextAt($"⚐", Game.settings.highlightRegionalFirsts ? Brushes.Gold : b);
                        if (genusPrefix == null)
                            genusPrefix = "⚐";
                    }

                    var displayName = variant.colorName;
                    if (isLegacy)
                    {
                        displayName = species.Key.englishName;
                        foreach(var tail in legacyTailTrimList)
                            if (displayName.EndsWith(tail)) displayName = displayName.Replace(tail, "");
                    }

                    drawTextAt(displayName, b);
                }

                //b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                //drawTextAt("?", b);
                newLine(+one, true);
            }

            drawVolumeBars(g, oneTwo, yy + oneSix, highlight, min, max, shouldBeGold(genusPrefix));

            // 2nd/last line Right - credit range
            b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
            var txtRight = " " + Util.getMinMaxCredits(min, max);
            var sz = drawTextAt(this.Width - eight, txtRight, b, null, true);

            // 2nd/last line LEFT - genus name
            dtx = twoEight;
            if (genusPrefix != null)
            {
                if (!highlight && shouldBeGold(genusPrefix)) b = Brushes.Gold;
                drawTextAt(genusPrefix, shouldBeGold(genusPrefix) ? Brushes.Gold : null);
            }
            drawTextAt($"{genus.englishName}", b);
            dtx += sz.Width;
            newLine(+ten, true);
        }

        private void drawSystemBios2()
        {
            if (game.systemData == null) return;

            this.drawTextAt($"System bio signals: {game.systemData.bioSignalsTotal}", GameColors.brushGameOrange, GameColors.fontSmall);
            newLine(+four, true);

            //var anyFoo = game.systemData.bodies.Any(b => b.id == game.status.Destination?.Body && b.bioSignalCount > 0);
            var destinationBody = game.targetBodyShortName;

            // get widest body name
            var maxNameWidth = 0f;
            var maxBioCount = 0;
            foreach (var body in game.systemData.bodies)
            {
                if (body.bioSignalCount == 0) continue;
                maxNameWidth = Math.Max(maxNameWidth, g.MeasureString(body.shortName, GameColors.fontMiddle).Width);
                maxBioCount = Math.Max(maxBioCount, body.bioSignalCount);
            }
            var boxLeft = oneTwo + maxNameWidth;
            var boxRight = boxLeft + (maxBioCount * oneTwo);

            // draw a row for each body
            var sortedBodies = game.systemData.bodies.OrderBy(b => b.shortName).ToList();
            var anyFF = false;
            foreach (var body in sortedBodies)
            {
                if (body.bioSignalCount == 0) continue;
                anyFF |= body.firstFootFall;
                var potentialFirstDiscovery = body.predictions.Values.Any(p => (!game.cmdrCodex.isDiscoveredInRegion(p.entryId, game.cmdr.galacticRegion) && Game.settings.highlightRegionalFirsts) || !game.cmdrCodex.isDiscovered(p.entryId));


                var highlight = body.shortName == destinationBody || (body.countAnalyzedBioSignals != body.bioSignalCount && body.countAnalyzedBioSignals > 0);
                //var highlight = (body == game.systemBody && game.status.hasLatLong) || (game.systemBody == null && body.shortName == destinationBody);
                // || !anyFoo); // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";

                dty = (float)Math.Round(dty);

                // draw body name
                var b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                //if (!highlight && potentialFirstDiscovery) b = (SolidBrush)Brushes.Gold;
                var sz2 = this.drawTextAt(eight, body.shortName, b, GameColors.fontMiddle);
                if (body.bioSignalCount == body.countAnalyzedBioSignals)
                {
                    // strike-through if already analyzed
                    var y = dty + sz2.Height / 2;
                    g.DrawLine(highlight ? GameColors.penCyan1 : GameColors.penGameOrange1, dtx, y, dtx - sz2.Width, y);
                    g.DrawLine(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, dtx + 1, y + 1, dtx - sz2.Width + 1, y + 1);
                }

                // draw a box for each signal we know about
                var signalCount = body.bioSignalCount;
                var x = boxLeft;
                if (body.organisms?.Count > 0)
                {
                    // draw volume boxes for what we do know...
                    foreach (var org in body.organisms)
                    {
                        var min = body.getBioRewardForGenus(org, true);
                        var max = body.getBioRewardForGenus(org, false);
                        var isGold = org.isFirst;
                        if (org.entryId == 0)
                        {
                            var matchGenus = Game.codexRef.matchFromGenus(org.genus)!;
                            isGold |= body.genusPredictions[matchGenus].Any(s => s.Value.Any(v => !game.cmdrCodex.isDiscovered(v.entryId)));
                            if (Game.settings.highlightRegionalFirsts)
                                isGold |= body.genusPredictions[matchGenus].Any(s => s.Value.Any(v => !game.cmdrCodex.isDiscoveredInRegion(v.entryId, game.cmdr.galacticRegion)));
                        }
                        drawVolumeBars(g, x, dty + oneFive, highlight, min, max, isGold);
                        x += oneTwo;
                    }
                    signalCount -= body.organisms.Count;
                }

                // ... and draw more boxes for those we don't
                // using the first and last for the min/max of all the potentials
                // and ? boxes for anything in between
                if (signalCount == 1)
                {
                    //long min = body.predictions.Count > 0 ? body.predictions.Values.Min(p => p.reward) : -1;
                    long min = body.predictions.Count > 0 ? body.predictions.Values.Min(p => p.reward) : 0;
                    long max = body.predictions.Count > 0 ? body.predictions.Values.Max(p => p.reward) : -1;
                    drawVolumeBars(g, x, dty + oneFive, highlight, min, max, potentialFirstDiscovery);
                    x += oneTwo;
                }
                else if (signalCount > 1)
                {
                    // first is min
                    long min = body.predictions.Count > 0 ? body.predictions.Values.Min(p => p.reward) : 0;
                    drawVolumeBars(g, x, dty + oneFive, highlight, min);
                    x += oneTwo;

                    for (var n = 1; n < signalCount - 1; n++)
                    {
                        drawVolumeBars(g, x, dty + oneFive, highlight, -1, -1);
                        x += oneTwo;
                    }

                    // last is max
                    long max = body.predictions.Count > 0 ? body.predictions.Values.Max(p => p.reward) : 0;
                    drawVolumeBars(g, x, dty + oneFive, highlight, min, max, potentialFirstDiscovery);
                    x += oneTwo;
                }

                // credits
                b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                //if (!highlight && potentialFirstDiscovery) b = (SolidBrush)Brushes.Gold;

                var txt = body.getMinMaxBioRewards(false);
                if (txt == "") txt = " ";
                drawTextAt(this.ClientSize.Width - ten, txt, b, GameColors.fontMiddle, true);

                dtx = lastTextSize.Width + boxRight + oneTwo;
                newLine(+four, true);
            }

            this.dty += four;
            var footerTxt = $"Rewards: {Util.getMinMaxCredits(game.systemData.getMinBioRewards(false), game.systemData.getMaxBioRewards(false))}";
            this.drawTextAt(six, footerTxt, GameColors.brushGameOrange, GameColors.fontSmall);
            newLine(+two, true);

            if (anyFF)
            {
                drawTextAt(this.Width - eight, $"(FF bonus: {Util.getMinMaxCredits(game.systemData.getMinBioRewards(true), game.systemData.getMaxBioRewards(true))})", GameColors.brushCyan, null, true);
                dtx = lastTextSize.Width;
                newLine(+two, true);
            }

            formAdjustSize(+ten, +six);
        }

        public static void drawVolumeBars(Graphics g, float x, float y, bool highlight, long reward, long maxReward = -1, bool isNewEntry = false)
        {
            var ww = eight;
            var bb = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
            //var bb2 = new HatchBrush(HatchStyle.DarkUpwardDiagonal, highlight ? Color.FromArgb(200, GameColors.DarkCyan) : GameColors.Orange, Color.Black);
            var bb2 = new SolidBrush(highlight ? Color.FromArgb(180, GameColors.DarkCyan) : Color.FromArgb(140, GameColors.OrangeDim)); //GameColors.OrangeDim);

            var pp = highlight ? Pens.DarkCyan : GameColors.penGameOrangeDim1;
            var pp2 = highlight ? Pens.DarkCyan : GameColors.newPen(Color.FromArgb(124, GameColors.Orange));

            // draw outer dotted box
            g.FillRectangle(Brushes.Black, x, y - 12, ww, 12);
            var ppp = highlight
                ? GameColors.newPen(Color.FromArgb(96, GameColors.DarkCyan), 1.9f, DashStyle.Dot)
                : GameColors.newPen(Color.FromArgb(96, GameColors.Orange), 1.9f, DashStyle.Dot);

            if (isNewEntry)
            {
                bb = (SolidBrush)Brushes.DarkGoldenrod;
                pp = Pens.Gold;
                ppp = GameColors.newPen(Color.FromArgb(96, Color.Gold), 1.9f, DashStyle.Dot);
                bb2 = (SolidBrush)Brushes.DarkGoldenrod;
                pp2 = Pens.Gold;
            }

            g.DrawRectangle(ppp, x, y - oneTwo, ww, oneFive);

            if (reward <= 0)
            {
                // negative reward means we don't know anything about this yet
                //g.FillRectangle(highlight ? GameColors.brushDarkCyan : GameColors.brushGameOrangeDim, x, y, ww, 3);
                //g.DrawRectangle(pp, x, y - 12, ww, 15);

                // (don't use drawTextAt as that messes with dtx/dty)
                g.DrawString("?", GameColors.fontSmallBold, bb, x - 0.5f, y - oneOne);
            }
            else
            {
                //if (reward == 0)
                //{
                //    if (maxReward == -1)
                //    {
                //        // we have no predictions - show the lowest bar populated
                //        g.FillRectangle(bb2, x, y, ww, three);
                //        g.DrawRectangle(pp, x, y, ww, three);
                //    }
                //    return;
                //}

                // 1st/lowest bar
                if (reward > 0)
                {
                    g.FillRectangle(bb, x, y, ww, three);
                    g.DrawRectangle(pp, x, y, ww, three);
                }
                else if (maxReward > 0)
                {
                    g.FillRectangle(bb2, x, y, ww, three);
                    g.DrawRectangle(pp2, x, y, ww, three);
                }
                else return;
                y -= 4;

                // 2nd bar up
                if (reward > Game.settings.bioRingBucketOne * 1_000_000)
                {
                    g.FillRectangle(bb, x, y, ww, three);
                    g.DrawRectangle(pp, x, y, ww, three);
                }
                else if (maxReward > Game.settings.bioRingBucketOne * 1_000_000)
                {
                    g.FillRectangle(bb2, x, y, ww, three);
                    g.DrawRectangle(pp2, x, y, ww, three);
                }
                else return;
                y -= 4;

                // 3rd bar
                if (reward > Game.settings.bioRingBucketTwo * 1_000_000)
                {
                    g.FillRectangle(bb, x, y, ww, three);
                    g.DrawRectangle(pp, x, y, ww, three);
                }
                else if (maxReward > Game.settings.bioRingBucketTwo * 1_000_000)
                {
                    g.FillRectangle(bb2, x, y, ww, three);
                    g.DrawRectangle(pp2, x, y, ww, three);
                }
                else return;
                y -= 4;

                // 4th bar
                if (reward > Game.settings.bioRingBucketThree * 1_000_000)
                {
                    g.FillRectangle(bb, x, y, ww, three);
                    g.DrawRectangle(pp, x, y, ww, three);
                }
                else if (maxReward > Game.settings.bioRingBucketThree * 1_000_000)
                {
                    g.FillRectangle(bb2, x, y, ww, three);
                    g.DrawRectangle(pp2, x, y, ww, three);
                }
                else return;
            }
        }
    }
}

