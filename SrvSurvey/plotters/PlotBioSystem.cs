using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
    [ApproxSize(200, 200)]
    internal class PlotBioSystem : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotBioSystem
                && Game.activeGame?.status != null
                && Game.activeGame.systemData != null
                && Game.activeGame.systemData.bioSignalsTotal > 0
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.hidePlottersFromCombatSuits
                && (
                    Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                    || (
                        PlotBioSystem.targetBody?.bioSignalCount > 0
                        && Game.activeGame.isMode(GameMode.GlideMode, GameMode.Flying, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel, GameMode.InSrv, GameMode.RolePanel, GameMode.Codex)
                                && (Game.activeGame.systemStation == null || !Game.settings.autoShowHumanSitesTest)
                    )
                );
        }

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

        public override void reposition(Rectangle gameRect)
        {
            base.reposition(gameRect);

            // It's easy for this to overlap with PlotBioSystem ... so shift them up if that is the case
            Program.getPlotter<PlotPriorScans>()?.avoidPlotBioSystem(this);
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
                if (targetBody?.bioSignalCount > 0 && game.isMode(GameMode.ExternalPanel, GameMode.SystemMap, GameMode.Orrery))
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
                }
                this.Invalidate();
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
            if (game == null) return;

            drawTextAt(eight, RES("BodyBio_Header", body.shortName, body.bioSignalCount), GameColors.brushGameOrange);
            newLine(+eight, true);

            if (this.durationTimer.Enabled && this.durationCount > 0)
                this.drawTimerBar();

            if (body.organisms == null)
            {
                if (!body.dssComplete)
                {
                    dty -= four;
                    this.drawTextAt(ten, "► " + RES("DssRequired"), GameColors.brushCyan);
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
                    var highlight = !organism.analyzed && ((game.cmdr.scanOne?.genus == organism.genus && game.cmdr.scanOne.body == body.name) || game.cmdr.scanOne?.genus == null);
                    Brush brush = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;

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
                        g.DrawLine(GameColors.penGameOrangeDim1, eight, dty - five, this.ClientSize.Width - eight, dty - five);

                    // if we have predictions - use that renderer
                    if (organism.variant == null && predictions.Count > 0)
                    {
                        //var genus = Game.codexRef.matchFromGenus(organism.genus)!;
                        var prediction = body.genusPredictions.Find(p => p.genus.name == organism.genus);
                        if (prediction == null) { Debugger.Break(); continue; }
                        drawBodyPredictionsRow(prediction, highlight);
                        continue;
                    }

                    var yy = dty;

                    // displayName is either genus, or species/variant without the genus prefix
                    var displayName = organism.bioMatch?.genus.locName;
                    if (organism.bioMatch?.genus.odyssey == true)
                    {
                        // make it <species>: <color>
                        displayName = organism.bioMatch.species.locName + ": " + organism.bioMatch.variant.locColorName;
                    }
                    else if (!string.IsNullOrEmpty(organism.speciesLocalized))
                    {
                        displayName = organism.speciesLocalized.Replace(organism.genusLocalized + " ", "");
                        foreach (var tail in legacyTailTrimList)
                            if (displayName.EndsWith(tail)) displayName = displayName.Replace(tail, "");
                    }
                    else
                    {
                        // does this ever happen?
                        Debugger.Break();
                    }

                    var minReward = body.getBioRewardForGenus(organism, true);
                    var maxReward = body.getBioRewardForGenus(organism, false);
                    var volCol = /*highlight ? VolColor.Blue :*/ VolColor.Orange;
                    if (shouldBeGold(discoveryPrefix))
                        volCol = VolColor.Gold;
                    else if (!organism.analyzed)
                    {
                        if (Game.activeGame?.cmdrCodex.isDiscovered(organism.entryId) == false)
                        {
                            discoveryPrefix = "⚑";
                            volCol = VolColor.Gold;
                        }
                        else if (Game.activeGame?.cmdrCodex.isDiscoveredInRegion(organism.entryId, game.cmdr.galacticRegion) == false)
                        {
                            discoveryPrefix = "⚐";
                            if (Game.settings.highlightRegionalFirsts) volCol = VolColor.Gold;
                        }
                    }
                    VolumeBar.render(g, oneTwo, yy + oneSix, volCol, minReward, maxReward, false);

                    // line 1
                    if (volCol == VolColor.Gold) brush = GameColors.Bio.brushGold;
                    if (displayName?.Length > 0)
                        this.drawTextAt(twoEight, displayName, brush);
                    if (organism.analyzed)
                    {
                        // strike-through if already analyzed
                        var y = dty + four;
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
                    if (!highlight && shouldBeGold(discoveryPrefix)) brush = GameColors.Bio.brushGold;

                    dtx = twoEight;
                    if (discoveryPrefix != null)
                        drawTextAt(discoveryPrefix, shouldBeGold(discoveryPrefix) ? GameColors.Bio.brushGold : brush);

                    var leftText = displayName != organism.bioMatch?.genus.locName || organism.entryId > 0
                        ? organism.bioMatch?.genus.locName
                        : "?";
                    drawTextAt(leftText, brush);
                    dtx += sz.Width + ten;
                    newLine(+eight, true);

                    if (organism.genus == game.cmdr.scanOne?.genus && game.cmdr.scanOne?.body == body.name)
                    {
                        // draw side-bars to highlight this is what we're currently scanning
                        g.DrawLine(GameColors.penCyan4, four, yy - one, four, dty - eight);
                        g.DrawLine(GameColors.penCyan4, Width - four, yy - one, Width - four, dty - eight);
                    }

                }
            }

            // summary footer
            this.dty += two;
            var footerTxt = RES("RewardFooter", body.getMinMaxBioRewards(false));
            drawTextAt(eight, footerTxt, GameColors.brushGameOrange);
            newLine(true);

            if (body.firstFootFall)
            {
                this.dty += two;
                drawTextAt(this.Width - eight, RES("FFBonus", body.getMinMaxBioRewards(true)), GameColors.brushCyan, null, true);
                dtx = lastTextSize.Width;
                newLine(true);
            }

            if (body.geoSignalCount > 0 && !Game.settings.hideGeoCountInBioSystem)
            {
                dty += ten;
                g.DrawLine(GameColors.penGameOrangeDim1, eight, dty - five, this.ClientSize.Width - eight, dty - five);
                dty += two;

                drawTextAt(eight, RES("GeoSignals", body.geoSignalCount), GameColors.brushGameOrange);
                newLine(+four, true);

                // geo signals?
                if (body.geoSignals?.Count > 0)
                {
                    foreach (var geoName in body.geoSignalNames)
                    {
                        // TODO: show gold flags if this is a first discovery?
                        this.drawTextAt(oneTwo, $"► {geoName}");
                        newLine(+four, true);
                    }
                }
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
            // slide to the left
            g.DrawLine(GameColors.penGameOrangeDim2, x, y, x + w * r, y);
            g.DrawLine(GameColors.penGameOrangeDim2, x, one, x + w * r, one);

            this.formGrow(false, true); // TODO: still needed?
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
            foreach (var prediction in body.genusPredictions)
            {
                if (first)
                    first = false;
                else
                    g.DrawLine(GameColors.penGameOrangeDim1, eight, dty - six, this.ClientSize.Width - eight, dty - six);

                drawBodyPredictionsRow(prediction, true);
            }
        }

        private static List<string> legacyTailTrimList = new List<string>()
        {
            "Anemone", "Brain Tree", "Sinuous Tubers"
        };

        private void drawBodyPredictionsRow(SystemGenusPrediction prediction, bool highlight)
        {
            dtx = (float)Math.Round(dtx);
            dty = (float)Math.Round(dty);
            var yy = dty;
            var genusName = prediction.genus.locName;
            Brush b;
            foreach (var species in prediction.species)
            {
                b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;


                // draw an initial ? for predictions
                dtx = twoEight;
                drawTextAt("?", GameColors.Bio.brushPrediction);

                var isLegacy = !species.Key.genus.odyssey;
                if (!isLegacy)
                    drawTextAt($"{species.Key.locName}:", b);

                // TODO: handle legacy species better - put them on the same line as if they were variants
                foreach (var variant in species.Value)
                {
                    //  🎂 🧁 🍥 ‡† ⁑ ⁂ ※ ⁜‼•🟎 🟂🟎🟒🟈⚑⚐⛿🏁🎌⛳🏴🏳🟎✩✭✪𓇽𓇼 🚕🛺🚐🚗🚜🚛🛬🚀🛩️☀️🌀☄️🔥⚡🌩️🌠☀️
                    // 💫 🧭🧭🌍🌐🌏🌎🗽♨️🌅
                    // 💎🪐🎁🍥🍪🧊⛩️🌋⛰️🗻❄️🎉🧨🎁🧿🎲🕹️📣🎨🧵🔇🔕🎚️🎛️📻📱📺💻🖥️💾📕📖📦📍📎✂️📌📐📈💼🔰🛡️🔨🗡️🔧🧪🚷🧴📵🧽➰🔻🔺🔔🔘🔳🔲🏁🚩🏴✔️✖️❌➕➖➗ℹ️📛⭕☑️📶🔅🔆⚠️⛔🚫🧻↘️⚰️🧯🧰📡🧬⚗️🔩⚙️🔓🗝️🗄️📩🧾📒📰🗞️🏷️📑🔖💡🔋🏮🕯🔌📞☎️💍👑🧶🎯🔮🧿🎈🏆🎖️🌌💫🚧💰
                    b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                    if (variant.isRegionalNew)
                        drawTextAt("☀", GameColors.Bio.brushWhite);
                    else if (variant.isCmdrNew)
                        drawTextAt("⚑", GameColors.Bio.brushGold);
                    else if (variant.isCmdrRegionalNew)
                        drawTextAt($"⚐", variant.isGold ? GameColors.Bio.brushGold : b);

                    var displayName = variant.variant.locColorName;
                    if (isLegacy)
                    {
                        displayName = species.Key.locName;
                        // TODO: shift this logic within BioSpecies?
                        foreach (var tail in legacyTailTrimList)
                        {
                            if (displayName.EndsWith(tail))
                            {
                                displayName = displayName.Replace(tail, "");
                                break;
                            }
                        }
                    }

                    if (variant.isRegionalNew) b = GameColors.Bio.brushWhite;
                    else if (!highlight && variant.isGold) b = GameColors.Bio.brushGold;
                    drawTextAt(displayName, b, variant.isRegionalNew ? GameColors.fontSmallBold : null);
                }

                // draw a trailing ? for predictions
                drawTextAt("?", GameColors.Bio.brushPrediction);
                newLine(+one, true);
            }

            var volCol = prediction.isGold ? VolColor.Gold : VolColor.Blue;
            if (prediction.hasRegionalNew) volCol = VolColor.White;
            VolumeBar.render(g, oneTwo, yy + oneSix, volCol, prediction.min, prediction.max, true);

            // 2nd/last line Right - credit range
            b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
            var txtRight = " " + Util.getMinMaxCredits(prediction.min, prediction.max);
            var sz = drawTextAt(this.Width - eight, txtRight, b, null, true);

            // 2nd/last line LEFT - genus name
            dtx = twoEight;
            if (prediction.hasRegionalNew)
                drawTextAt("☀", GameColors.Bio.brushWhite);
            else if (prediction.hasCmdrNew)
                drawTextAt("⚑", GameColors.Bio.brushGold);
            else if (prediction.hasCmdrRegionalNew)
                drawTextAt("⚐", prediction.isGold ? GameColors.Bio.brushGold : b);

            if (prediction.hasRegionalNew) b = GameColors.Bio.brushWhite;
            else if (!highlight && prediction.isGold) b = GameColors.Bio.brushGold;
            drawTextAt($"{prediction.genus.locName}", b, prediction.hasRegionalNew ? GameColors.fontSmallBold : null);
            dtx += sz.Width;
            newLine(+ten, true);
        }

        private void drawSystemBios2()
        {
            if (game.systemData == null) return;

            this.drawTextAt(RES("SysBio_Header", game.systemData.bioSignalsTotal), GameColors.brushGameOrange, GameColors.fontSmall);
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
            var fssNeeded = false;
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
                Brush b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;

                var scansComplete = body.bioSignalCount == body.countAnalyzedBioSignals;
                if (scansComplete) b = GameColors.brushGameOrangeDim;

                //if (!highlight && potentialFirstDiscovery) b = (SolidBrush)GameColors.Bio.brushGold;
                var sz2 = this.drawTextAt(eight, body.shortName, b, GameColors.fontMiddle);

                if (scansComplete)
                {
                    // strike-through if already analyzed
                    var y = dty + sz2.Height / 2;
                    g.DrawLine(highlight ? GameColors.penCyan1 : GameColors.penGameOrange1, dtx, y, dtx - sz2.Width, y);
                    g.DrawLine(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, dtx + 1, y + 1, dtx - sz2.Width + 1, y + 1);
                }

                dtx = boxLeft;
                dtx += drawBodyBars(g, body, dtx, dty, highlight);

                if (dtx > boxRight)
                    boxRight = dtx;

                // credits
                b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                //if (!highlight && potentialFirstDiscovery) b = (SolidBrush)GameColors.Bio.brushGold;

                var txt = body.getMinMaxBioRewards(false);

                // show some icon if we know we have signals from Canonn
                if (Game.settings.useExternalData && Game.settings.autoLoadPriorScans)
                {
                    var bodyHasKnownSignals = game.canonnPoi?.codex?.Any(c => c.body != null && body.name.EndsWith(c.body)) ?? false;
                    if (bodyHasKnownSignals)
                        g.DrawImage(Properties.ImageResources.canonn_16x16, dtx, dty + two, 16, 16);
                }

                if (txt == "") txt = " ";
                drawTextAt(this.ClientSize.Width - ten, txt, b, GameColors.fontMiddle, true);

                dtx = lastTextSize.Width + boxRight + oneTwo;
                newLine(+four, true);

                // if we are missing predictions - we need to FSS the system to feed the predictor
                if ((body.organisms?.Count(o => o.entryId > 0) ?? 0) < body.bioSignalCount && body.genusPredictions.Count == 0)
                    fssNeeded = true;
            }

            // fss needed?
            if (fssNeeded)
            {
                dty += two;
                this.drawTextAt(six, "► " + RES("FssRequired"), GameColors.brushCyan, GameColors.fontSmall);
                newLine(true);
            }

            this.dty += four;
            var footerTxt = RES("RewardFooter", Util.getMinMaxCredits(game.systemData.getMinBioRewards(false), game.systemData.getMaxBioRewards(false)));
            this.drawTextAt(six, footerTxt, GameColors.brushGameOrange, GameColors.fontSmall);
            newLine(+two, true);

            if (anyFF)
            {
                drawTextAt(this.Width - eight, RES("FFBonus", Util.getMinMaxCredits(game.systemData.getMinBioRewards(true), game.systemData.getMaxBioRewards(true))), GameColors.brushCyan, null, true);
                dtx = lastTextSize.Width;
                newLine(+two, true);
            }

            formAdjustSize(+ten, +six);
        }

        public static float drawBodyBars(Graphics g, SystemBody body, float x, float y, bool highlight)
        {
            x = (int)x;
            y = (int)y;
            var ix = x;
            if (body.bioSignalCount == 0) return 0;
            var signalCount = body.bioSignalCount;
            y += oneFive;

            // draw outer box indicating how many signals match the body signal count
            g.SmoothingMode = SmoothingMode.Default;
            var w = (body.bioSignalCount * oneTwo) + two;
            g.DrawRectangle(highlight ? GameColors.penCyan1Dotted : GameColors.penGameOrange1Dotted,
                x - three, y - oneFive,
                w, twoOne);
            g.SmoothingMode = SmoothingMode.HighQuality;

            // first render known genus - this implies DSS has happened
            if (body.organisms?.Count > 0)
            {
                foreach (var org in body.organisms)
                {
                    var volCol = /* highlight ? VolColor.Blue : */ VolColor.Orange;
                    // genus was scanned
                    if (org.entryId > 0)
                    {
                        if (org.isFirst) volCol = VolColor.Gold;
                        else if (!org.analyzed && Game.activeGame?.cmdrCodex.isDiscovered(org.entryId) == false) volCol = VolColor.Gold;
                        var min = body.getBioRewardForGenus(org, true);
                        var max = body.getBioRewardForGenus(org, false);
                        VolumeBar.render(g, x, y, volCol, min, max, false);
                    }
                    else
                    {
                        // genus not yet scanned - we should have predictions for it though
                        var genusPrediction = body.genusPredictions.Find(g => g.genus.name == org.genus);
                        if (genusPrediction == null)
                        {
                            //Debugger.Break();
                            continue;
                        }
                        if (genusPrediction.hasRegionalNew) volCol = VolColor.White;
                        else if (genusPrediction.isGold) volCol = VolColor.Gold;
                        else volCol = VolColor.Blue;
                        VolumeBar.render(g, x, y, volCol, genusPrediction.min, genusPrediction.max, true);
                    }

                    x += oneTwo;
                    signalCount--;
                }
            }

            // exit if we're done
            if (signalCount == 0) return x - ix;

            // if the count of predicted genus matches the body bio signal count - show bars in orange, otherwise gray
            var countMatches = body.bioSignalCount == body.genusPredictions.Count;
            var defaultVolCol = /*countMatches ? VolColor.Orange :*/ VolColor.Blue;

            // otherwise, draw boxes for all the predicted genus
            foreach (var genusPrediction in body.genusPredictions)
            {
                // skip if we draw this one already
                if (body.organisms?.Any(o => o.genus == genusPrediction.genus.name) == true) continue;

                var volCol = /*highlight ? VolColor.Blue : */ defaultVolCol;
                if (genusPrediction.hasRegionalNew) volCol = VolColor.White;
                else if (genusPrediction.isGold) volCol = VolColor.Gold;

                // skip a few pixels to cross the dotted box
                if (signalCount == 0) x += three;

                VolumeBar.render(g, x, y, volCol, genusPrediction.min, genusPrediction.max, true);
                x += oneTwo;
                signalCount--;
            }

            if (signalCount > 0)
            {
                var volCol = highlight ? VolColor.Blue : defaultVolCol;
                while (signalCount > 0)
                {
                    VolumeBar.render(g, x, y, volCol, -1, -1, false);
                    x += oneTwo;
                    signalCount--;
                }
            }

            if (signalCount < 0) x -= two;
            return x - ix;
        }

        public static void drawVolumeBars(Graphics g, float x, float y, VolColor col, long reward, long maxReward = -1, bool prediction = false)
        {
            g.SmoothingMode = SmoothingMode.Default;
            var ww = eight;
            var yy = y;

            var buckets = new List<long>()
            {
                0,
                (long)Game.settings.bioRingBucketOne * 1_000_000,
                (long)Game.settings.bioRingBucketTwo * 1_000_000,
                (long)Game.settings.bioRingBucketThree * 1_000_000,
            };

            // draw outer dotted box
            g.FillRectangle(Brushes.Black, x, y - oneTwo, ww, oneTwo);
            g.DrawRectangle(GameColors.Bio.volEdge[col], x, y - oneTwo, ww, oneFive);

            if (reward <= 0)
            {
                // (don't use drawTextAt as that messes with dtx/dty)
                g.DrawString("?", GameColors.fontSmallBold, GameColors.Bio.brushPrediction, x - 0.7f, y - oneOne);
                return;
            }

            foreach (var bucket in buckets)
            {
                if (reward > bucket)
                {
                    g.FillRectangle(GameColors.Bio.volMin[col].brush, x, y, ww, three);
                    g.DrawRectangle(GameColors.Bio.volMin[col].pen, x, y, ww, three);
                }
                else if (maxReward > bucket)
                {
                    g.FillRectangle(GameColors.Bio.volMax[col].brush, x, y, ww, three);
                    g.DrawRectangle(GameColors.Bio.volMax[col].pen, x, y, ww, three);
                }
                y -= four;
            }

            // draw a grey hatch effect if we're drawing a prediction
            if (prediction)
            {
                g.FillRectangle(GameColors.Bio.brushPredictionHatch, x + one, y + five, ww - one, oneFour);
                //g.FillRectangle(GameColors.Bio.brushPredictionHatch, x, y + four, ww + one, oneSix);
            }

            if (col == VolColor.White)
            {
                g.DrawRectangle(GameColors.Bio.volEdge[col], x, yy - oneTwo, ww, oneFive);
                g.DrawRectangle(GameColors.Bio.volEdge[col], x, yy - oneTwo, ww, oneFive);
            }

            g.SmoothingMode = SmoothingMode.HighQuality;
        }
    }
}
