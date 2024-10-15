using SrvSurvey.game;
using System.Diagnostics;
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
                if (targetBody?.bioSignalCount > 0 && game.isMode(GameMode.ExternalPanel, GameMode.SystemMap))
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
                        g.DrawLine(GameColors.penGameOrangeDim1, eight, dty - four, this.ClientSize.Width - eight, dty - four);

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

                    var minReward = body.getBioRewardForGenus(organism, true);
                    var maxReward = body.getBioRewardForGenus(organism, false);
                    var volCol = /*highlight ? VolColor.Blue :*/ VolColor.Orange;
                    if (shouldBeGold(discoveryPrefix)) volCol = VolColor.Gold;
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
                    drawVolumeBars(g, oneTwo, yy + oneSix, volCol, minReward, maxReward, false);

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
                    if (!highlight && shouldBeGold(discoveryPrefix)) brush = GameColors.Bio.brushGold;

                    dtx = twoEight;
                    if (discoveryPrefix != null)
                        drawTextAt(discoveryPrefix, shouldBeGold(discoveryPrefix) ? GameColors.Bio.brushGold : brush);

                    var leftText = displayName != organism.genusLocalized || organism.entryId > 0
                        ? organism.genusLocalized
                        : "?";
                    drawTextAt(leftText, brush);
                    dtx += sz.Width + ten;
                    newLine(+eight, true);

                    if (organism.genus == game.cmdr.scanOne?.genus)
                    {
                        // draw side-bars to highlight this is what we're currently scanning
                        g.DrawLine(GameColors.penCyan4, four, yy - one, four, dty - eight);
                        g.DrawLine(GameColors.penCyan4, Width - four, yy - one, Width - four, dty - eight);
                    }

                }
            }

            // summary footer
            this.dty += two;
            var footerTxt = $"Rewards: {body.getMinMaxBioRewards(false)}";
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
            var genusName = prediction.genus.englishName;
            Brush b;
            foreach (var species in prediction.species)
            {
                b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;


                // draw an initial ? for predictions
                dtx = twoEight;
                drawTextAt("?", GameColors.Bio.brushPrediction);

                var isLegacy = !species.Key.genus.odyssey;
                if (!isLegacy)
                    drawTextAt($"{species.Key.displayName}:", b);

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

                    var displayName = variant.variant.colorName;
                    if (isLegacy)
                    {
                        displayName = species.Key.englishName;
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
            drawVolumeBars(g, oneTwo, yy + oneSix, volCol, prediction.min, prediction.max, true);

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
            drawTextAt($"{prediction.genus.englishName}", b, prediction.hasRegionalNew ? GameColors.fontSmallBold : null);
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

                /*
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
                        var volCol = highlight ? VolColor.Blue : VolColor.Orange;
                        if (org.entryId == 0)
                        {
                            // genus not yet scanned - we should have predictions for it though
                            volCol = VolColor.Grey;
                            var matchGenus = Game.codexRef.matchFromGenus(org.genus)!;
                            var isGold = body.genusPredictions[matchGenus].Any(s => s.Value.Any(v => !game.cmdrCodex.isDiscovered(v.entryId)));
                            if (Game.settings.highlightRegionalFirsts)
                                isGold |= body.genusPredictions[matchGenus].Any(s => s.Value.Any(v => !game.cmdrCodex.isDiscoveredInRegion(v.entryId, game.cmdr.galacticRegion)));
                            if (isGold) volCol = VolColor.Gold;
                        }
                        else if (org.isFirst)
                        {
                            // genus was scanned
                            volCol = VolColor.Gold;
                        }
                        drawVolumeBars(g, x, dty + oneFive, volCol, min, max);
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
                    var volCol = highlight ? VolColor.Blue : VolColor.Orange;
                    if (potentialFirstDiscovery) volCol = VolColor.Gold;
                    drawVolumeBars(g, x, dty + oneFive, volCol, min, max);
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
                // */

                // credits
                b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                //if (!highlight && potentialFirstDiscovery) b = (SolidBrush)GameColors.Bio.brushGold;

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
                        drawVolumeBars(g, x, y, volCol, min, max, false);
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
                        drawVolumeBars(g, x, y, volCol, genusPrediction.min, genusPrediction.max, true);
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

                drawVolumeBars(g, x, y, volCol, genusPrediction.min, genusPrediction.max, true);
                x += oneTwo;
                signalCount--;
            }

            if (signalCount > 0)
            {
                var volCol = highlight ? VolColor.Blue : defaultVolCol;
                while (signalCount > 0)
                {
                    drawVolumeBars(g, x, y, volCol, -1, -1, false);
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
