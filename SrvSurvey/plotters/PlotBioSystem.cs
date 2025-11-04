using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using Res = Loc.PlotBioSystem;

namespace SrvSurvey.plotters
{
    internal class PlotBioSystem : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotBioSystem),
            allowed = allowed,
            ctor = (game, def) => new PlotBioSystem(game, def),
            defaultSize = new Size(200, 200),
            invalidationJournalEvents = new() { nameof(CodexEntry), nameof(FSSBodySignals), nameof(Scan), nameof(SAAScanComplete), nameof(SAASignalsFound) },
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotBioSystem
                && game.status != null
                && game.systemData != null
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && game.systemData.bioSignalsTotal > 0
                && !game.status.InTaxi
                && !game.hidePlottersFromCombatSuits
                && (!Game.settings.enableGuardianSites || !PlotGuardians.allowed(game))
                && (
                    game.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                    || (
                        PlotBioSystem.targetBody?.bioSignalCount > 0
                        && game.isMode(GameMode.GlideMode, GameMode.Flying, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel, GameMode.InSrv, GameMode.RolePanel, GameMode.Codex)
                                && (game.systemStation == null || !Game.settings.autoShowHumanSitesTest)
                    )
                );
        }

        #endregion

        private string? lastDestination;
        private SystemBody? durationBody;
        private System.Windows.Forms.Timer durationTimer;
        private float durationCount;
        private float durationTotal;

        private PlotBioSystem(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;
            this.lastDestination = game.status.Destination?.Name;

            durationTimer = new System.Windows.Forms.Timer()
            {
                Interval = 25,
                Enabled = false,
            };
            durationTimer.Tick += DurationTimer_Tick;
        }

        // It's easy for this to overlap with PlotBioSystem ... so shift them up if that is the case
        // TODO: REVISIT !!!
        // Program.getPlotter<PlotPriorScans>()?.avoidPlotBioSystem(this);

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

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (!Game.settings.drawBodyBiosOnlyWhenNear && this.durationTimer.Enabled && game.mode != GameMode.ExternalPanel)
            {
                this.stopTimer();
                this.invalidate();
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
                this.invalidate();
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

            this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            if (game.systemData == null || game.status == null)
            {
                this.hide();
                return frame.Size;
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
                return this.drawBodyBios2(g, tt, body);
            else
                return this.drawSystemBios2(g, tt);
        }

        private SizeF drawBodyBios2(Graphics g, TextCursor tt, SystemBody body)
        {
            tt.draw(N.eight, Res.BodyBio_Header.format(body.shortName, body.bioSignalCount), C.orange);
            tt.newLine(+N.eight, true);

            if (this.durationTimer.Enabled && this.durationCount > 0)
                this.drawTimerBar(g);

            if (body.organisms == null)
            {
                if (!body.dssComplete)
                {
                    tt.dty -= N.four;
                    tt.draw(N.ten, "► " + Res.DssRequired, C.cyan);
                    tt.newLine(+N.four, true);
                }

                this.drawBodyPredictions(g, tt, body);
            }
            else
            {
                // draw a row for each organism
                var first = true;
                foreach (var organism in body.organisms)
                {
                    var highlight = !organism.analyzed && ((game.cmdr.scanOne?.genus == organism.genus && game.cmdr.scanOne.body == body.name) || game.cmdr.scanOne?.genus == null);
                    var col = highlight ? C.cyan : C.orange;

                    var predictions = body.predictions.Values.Where(p => p.species.genus.name == organism.genus).ToList();
                    var potentialFirstDiscovery = predictions.Any(p => !game.cmdrCodex.isDiscovered(p.entryId));

                    // do we already know if this is a first discovery?
                    string? discoveryPrefix = null;
                    if (organism.isCmdrFirst) discoveryPrefix = "⚑";
                    else if (organism.isNewEntry) discoveryPrefix = "⚐";

                    tt.dty = (int)tt.dty;

                    if (first)
                        first = false;
                    else
                        g.DrawLine(GameColors.penGameOrangeDim1, N.eight, tt.dty - N.five, this.width - N.eight, tt.dty - N.five);

                    // if we have predictions - use that renderer
                    if (organism.variant == null && predictions.Count > 0)
                    {
                        //var genus = Game.codexRef.matchFromGenus(organism.genus)!;
                        var prediction = body.genusPredictions.Find(p => p.genus.name == organism.genus);
                        if (prediction == null) { Debugger.Break(); continue; }
                        drawBodyPredictionsRow(g, tt, prediction, highlight);
                        continue;
                    }

                    var yy = tt.dty;

                    // displayName is either genus, or species/variant without the genus prefix
                    var displayName = organism.genusLocalized ?? organism.bioMatch?.genus.locName!;
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
                        displayName = Res.NotPredicted;
                    }

                    var minReward = body.getBioRewardForGenus(organism, true);
                    var maxReward = body.getBioRewardForGenus(organism, false);
                    var volCol = /*highlight ? VolColor.Blue :*/ VolColor.Orange;
                    if (shouldBeGold(discoveryPrefix))
                        volCol = VolColor.Gold;
                    else if (!organism.analyzed)
                    {
                        if (!game.cmdrCodex.isDiscovered(organism.entryId))
                        {
                            discoveryPrefix = "⚑";
                            volCol = VolColor.Gold;
                        }
                        else if (!game.cmdrCodex.isDiscoveredInRegion(organism.entryId, game.cmdr.galacticRegion))
                        {
                            discoveryPrefix = "⚐";
                            if (Game.settings.highlightRegionalFirsts) volCol = VolColor.Gold;
                        }
                    }
                    VolumeBar.render(g, N.oneTwo, yy + N.oneSix, volCol, minReward, maxReward, false);

                    // line 1
                    if (volCol == VolColor.Gold) col = C.Bio.gold;
                    if (displayName?.Length > 0)
                        tt.draw(N.twoEight, displayName, col);

                    // strike-through if already analyzed
                    if (organism.analyzed)
                        tt.strikeThroughLast();

                    tt.newLine(+N.one, true);

                    // 2nd line - right
                    var sz = tt.draw(
                        this.width - N.ten,
                        Util.getMinMaxCredits(minReward, maxReward),
                        highlight ? C.cyan : C.orange,
                        null, true);

                    // 2nd line - left
                    col = highlight ? C.cyan : C.orange;
                    if (!highlight && shouldBeGold(discoveryPrefix)) col = C.Bio.gold;

                    tt.dtx = N.twoEight;
                    if (discoveryPrefix != null)
                        drawEmoji(tt, discoveryPrefix, shouldBeGold(discoveryPrefix) ? C.Bio.gold : col);

                    var leftText = displayName != (organism.genusLocalized ?? organism.bioMatch?.genus.locName) || organism.entryId > 0
                        ? organism.genusLocalized
                        : "?";
                    tt.draw(leftText, col);
                    tt.dtx += sz.Width + N.ten;
                    tt.newLine(+N.eight, true);

                    if (organism.genus == game.cmdr.scanOne?.genus && game.cmdr.scanOne?.body == body.name)
                    {
                        // draw side-bars to highlight this is what we're currently scanning
                        g.DrawLine(GameColors.penCyan4, N.four, yy - N.one, N.four, tt.dty - N.eight);
                        g.DrawLine(GameColors.penCyan4, width - N.four, yy - N.one, width - N.four, tt.dty - N.eight);
                    }

                }
            }

            // summary footer
            tt.dty += N.two;
            var footerTxt = Res.RewardFooter.format(body.getMinMaxBioRewards(false));
            tt.draw(N.eight, footerTxt, C.orange);
            tt.newLine(true);

            if (body.firstFootFall)
            {
                tt.dty += N.two;
                tt.draw(this.width - N.eight, Res.FFBonus.format(body.getMinMaxBioRewards(true)), C.cyan, null, true);
                tt.dtx = tt.lastTextSize.Width;
                tt.newLine(true);
            }

            if (body.geoSignalCount > 0 && !Game.settings.hideGeoCountInBioSystem)
            {
                tt.dty += N.ten;
                g.DrawLine(GameColors.penGameOrangeDim1, N.eight, tt.dty - N.five, this.width - N.eight, tt.dty - N.five);
                tt.dty += N.two;

                tt.draw(N.eight, Res.GeoSignals.format(body.geoSignalCount), C.orange);
                tt.newLine(+N.four, true);

                // geo signals?
                if (body.geoSignals?.Count > 0)
                {
                    var names = body.geoSignalNames.ToList();
                    for (var n = 0; n < body.geoSignalCount; n++)
                    {
                        // TODO: show gold flags if this is a first discovery?
                        if (n < names.Count)
                            tt.draw(N.oneTwo, $"► {names[n]}");
                        else
                            tt.draw(N.oneTwo, $"► ?", C.orangeDark);

                        tt.newLine(+N.four, true);
                    }
                }
            }

            // resize window as necessary
            return tt.pad(+N.ten, +N.ten);
        }

        private void drawEmoji(TextCursor tt, string emoji, Color col, Font? font = null)
        {
            // glyphs change size using TextRenderer, so use a different font and adjust things a little
            font ??= GameColors.Fonts.segoeEmoji_8;

            tt.dty -= N.one;
            tt.draw(emoji, col, font);
            tt.dty += N.one;

            tt.dtx += N.two;
        }

        private void drawTimerBar(Graphics g)
        {
            var r = 1f / this.durationTotal * this.durationCount;

            var x = N.two;
            var y = this.height - N.one;
            var w = (this.width - x - x);
            // slide to the left
            g.DrawLine(GameColors.penGameOrangeDim2, x, y, x + w * r, y);
            g.DrawLine(GameColors.penGameOrangeDim2, x, N.one, x + w * r, N.one);
        }

        private bool shouldBeGold(string? prefix)
        {
            return prefix == "⚑"
                || (prefix == "⚐" && Game.settings.highlightRegionalFirsts);
        }

        private void drawBodyPredictions(Graphics g, TextCursor tt, SystemBody body)
        {
            if (body.predictions == null || body.predictions.Count == 0) return;

            var first = true;
            foreach (var prediction in body.genusPredictions)
            {
                if (first)
                    first = false;
                else
                    g.DrawLine(GameColors.penGameOrangeDim1, N.eight, tt.dty - N.six, this.width - N.eight, tt.dty - N.six);

                drawBodyPredictionsRow(g, tt, prediction, true);
            }
        }

        private static List<string> legacyTailTrimList = new List<string>()
        {
            "Anemone", "Brain Tree", "Sinuous Tubers"
        };

        private void drawBodyPredictionsRow(Graphics g, TextCursor tt, SystemGenusPrediction prediction, bool highlight)
        {
            tt.dtx = (float)Math.Round(tt.dtx);
            tt.dty = (float)Math.Round(tt.dty);
            var yy = tt.dty;
            var genusName = prediction.genus.locName;
            Color col;
            foreach (var species in prediction.species)
            {
                col = highlight ? C.cyan : C.orange;


                // draw an initial ? for predictions
                tt.dtx = N.twoEight;
                tt.draw("?", C.Bio.prediction);

                var isLegacy = !species.Key.genus.odyssey;
                if (!isLegacy)
                    tt.draw($"{species.Key.locName}:", col);

                // TODO: handle legacy species better - put them on the same line as if they were variants
                foreach (var variant in species.Value.OrderBy(v => v.variant.locName))
                {
                    //  🎂 🧁 🍥 ‡† ⁑ ⁂ ※ ⁜‼•🟎 🟂🟎🟒🟈⚑⚐⛿🏁🎌⛳🏴🏳🟎✩✭✪𓇽𓇼 🚕🛺🚐🚗🚜🚛🛬🚀🛩️☀️🌀☄️🔥⚡🌩️🌠☀️
                    // 💫 🧭🧭🌍🌐🌏🌎🗽♨️🌅
                    // 💎🪐🎁🍥🍪🧊⛩️🌋⛰️🗻❄️🎉🧨🎁🧿🎲🕹️📣🎨🧵🔇🔕🎚️🎛️📻📱📺💻🖥️💾📕📖📦📍📎✂️📌📐📈💼🔰🛡️🔨🗡️🔧🧪🚷🧴📵🧽➰🔻🔺🔔🔘🔳🔲🏁🚩🏴✔️✖️❌➕➖➗ℹ️📛⭕☑️📶🔅🔆⚠️⛔🚫🧻↘️⚰️🧯🧰📡🧬⚗️🔩⚙️🔓🗝️🗄️📩🧾📒📰🗞️🏷️📑🔖💡🔋🏮🕯🔌📞☎️💍👑🧶🎯🔮🧿🎈🏆🎖️🌌💫🚧💰
                    col = highlight ? C.cyan : C.orange;
                    if (variant.isRegionalNew)
                        drawEmoji(tt, "☀", C.Bio.white, GameColors.Fonts.segoeEmoji_6);
                    else if (variant.isCmdrNew)
                        drawEmoji(tt, "⚑", C.Bio.gold);
                    else if (variant.isCmdrRegionalNew)
                        drawEmoji(tt, "⚐", variant.isGold ? C.Bio.gold : col);

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

                    if (variant.isRegionalNew) col = C.Bio.white;
                    else if (!highlight && variant.isGold) col = C.Bio.gold;
                    tt.draw(displayName, col, variant.isRegionalNew ? GameColors.fontSmallBold : null);
                }

                // draw a trailing ? for predictions
                tt.draw("?", C.Bio.prediction);
                tt.newLine(+N.one, true);
            }

            var volCol = prediction.isGold ? VolColor.Gold : VolColor.Blue;
            if (prediction.hasRegionalNew) volCol = VolColor.White;
            VolumeBar.render(g, N.oneTwo, yy + N.oneSix, volCol, prediction.min, prediction.max, true);

            // 2nd/last line Right - credit range
            col = highlight ? C.cyan : C.orange;
            var txtRight = " " + Util.getMinMaxCredits(prediction.min, prediction.max);
            var sz = tt.draw(this.width - N.eight, txtRight, col, null, true);

            // 2nd/last line LEFT - genus name
            tt.dtx = N.twoEight;
            if (prediction.hasRegionalNew)
                drawEmoji(tt, "☀", C.Bio.white, GameColors.Fonts.segoeEmoji_6);
            else if (prediction.hasCmdrNew)
                drawEmoji(tt, "⚑", C.Bio.gold);
            else if (prediction.hasCmdrRegionalNew)
                drawEmoji(tt, "⚐", prediction.isGold ? C.Bio.gold : col);

            if (prediction.hasRegionalNew) col = C.Bio.white;
            else if (!highlight && prediction.isGold) col = C.Bio.gold;
            tt.draw($"{prediction.genus.locName}", col, prediction.hasRegionalNew ? GameColors.fontSmallBold : null);
            tt.dtx += sz.Width;
            tt.newLine(+N.ten, true);
        }

        private SizeF drawSystemBios2(Graphics g, TextCursor tt)
        {
            if (game.systemData == null) return frame.Size;

            tt.draw(Res.SysBio_Header.format(game.systemData.bioSignalsTotal), C.orange, GameColors.fontSmall);
            tt.newLine(+N.four, true);

            //var anyFoo = game.systemData.bodies.Any(b => b.id == game.status.Destination?.Body && b.bioSignalCount > 0);
            var destinationBody = game.targetBodyShortName;

            // get widest body name
            var maxNameWidth = 0f;
            var maxBioCount = 0;
            foreach (var body in game.systemData.bodies.ToList())
            {
                if (body.bioSignalCount == 0) continue;
                maxNameWidth = Math.Max(maxNameWidth, g.MeasureString(body.shortName, GameColors.fontMiddle).Width);
                maxBioCount = Math.Max(maxBioCount, body.bioSignalCount);
            }
            var boxLeft = N.oneTwo + maxNameWidth;
            var boxRight = boxLeft + (maxBioCount * N.oneTwo);

            // draw a row for each body
            var sortedBodies = game.systemData.bodies.OrderBy(b => b.shortName).ToList();
            var anyFF = false;
            var fssNeeded = false;
            var incBoxRight = false;
            foreach (var body in sortedBodies)
            {
                if (body.bioSignalCount == 0) continue;
                anyFF |= body.firstFootFall;

                var highlight = body.shortName == destinationBody || (body.countAnalyzedBioSignals != body.bioSignalCount && body.countAnalyzedBioSignals > 0);
                //var highlight = (body == game.systemBody && game.status.hasLatLong) || (game.systemBody == null && body.shortName == destinationBody);
                // || !anyFoo); // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";

                tt.dty = (float)Math.Round(tt.dty);

                // draw body name
                var col = highlight ? C.cyan : C.orange;

                var scansComplete = body.bioSignalCount == body.countAnalyzedBioSignals;
                if (scansComplete) col = C.orangeDark;

                var sz2 = tt.draw(N.eight, body.shortName, col, GameColors.fontMiddle);

                // strike-through if already analyzed
                if (scansComplete)
                    tt.strikeThroughLast();

                tt.dtx = boxLeft;
                tt.dtx += drawBodyBars(g, body, tt.dtx, tt.dty, highlight);

                if (tt.dtx > boxRight)
                    boxRight = tt.dtx;

                // credits
                col = highlight ? C.cyan : C.orange;

                var txt = body.getMinMaxBioRewards(false);

                // show some icon if we know we have signals from Canonn
                if (Game.settings.useExternalData && Game.settings.autoLoadPriorScans)
                {
                    var bodyHasKnownSignals = game.canonnPoi?.codex?.Any(c => c.body != null && body.name.EndsWith(c.body)) ?? false;
                    if (bodyHasKnownSignals)
                    {
                        g.DrawImage(Properties.ImageResources.canonn_16x16, tt.dtx + N.four, tt.dty + N.two, 16, 16);
                        if (!incBoxRight)
                        {
                            boxRight += 10;
                            incBoxRight = true;
                        }
                    }
                }

                if (txt == "") txt = " ";
                tt.dty += N.two;
                tt.draw(this.width - N.ten, txt, col, GameColors.fontSmaller, true);
                tt.dty -= N.two;

                tt.dtx = tt.lastTextSize.Width + boxRight + N.oneTwo;
                tt.newLine(+N.four, true);

                // if we are missing predictions - we need to FSS the system to feed the predictor
                if ((body.organisms?.Count(o => o.entryId > 0) ?? 0) < body.bioSignalCount && body.genusPredictions.Count == 0)
                    fssNeeded = true;
            }

            // fss needed?
            if (fssNeeded)
            {
                tt.dty += N.eight;
                tt.draw(N.six, "► " + (game.systemData.honked ? Res.DssRequired : Res.FssRequired), C.cyan, GameColors.fontSmall);
                tt.newLine(true);
            }

            tt.dty += N.four;
            var footerTxt = Res.RewardFooter.format(Util.getMinMaxCredits(game.systemData.getMinBioRewards(false), game.systemData.getMaxBioRewards(false)));
            tt.draw(N.six, footerTxt, C.orange, GameColors.fontSmall);
            tt.newLine(+N.two, true);

            if (anyFF)
            {
                tt.draw(this.width - N.eight, Res.FFBonus.format(Util.getMinMaxCredits(game.systemData.getMinBioRewards(true), game.systemData.getMaxBioRewards(true))), C.cyan, null, true);
                tt.dtx = tt.lastTextSize.Width;
                tt.newLine(+N.two, true);
            }

            return tt.pad(+N.ten, +N.six);
        }

        public static float drawBodyBars(Graphics g, SystemBody body, float x, float y, bool highlight)
        {
            x = (int)x;
            y = (int)y;
            var ix = x;
            if (body.bioSignalCount == 0) return 0;
            var signalCount = body.bioSignalCount;
            y += N.oneFive;

            // draw outer box indicating how many signals match the body signal count
            g.SmoothingMode = SmoothingMode.Default;
            var w = (body.bioSignalCount * N.oneTwo) + N.two;
            g.DrawRectangle(highlight ? GameColors.penCyan1Dotted : GameColors.penGameOrange1Dotted,
                x - N.three, y - N.oneFive,
                w, N.twoOne);
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

                    x += N.oneTwo;
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
                if (signalCount == 0) x += N.three;

                VolumeBar.render(g, x, y, volCol, genusPrediction.min, genusPrediction.max, true);
                x += N.oneTwo;
                signalCount--;
            }

            if (signalCount > 0)
            {
                var volCol = highlight ? VolColor.Blue : defaultVolCol;
                while (signalCount > 0)
                {
                    VolumeBar.render(g, x, y, volCol, -1, -1, false);
                    x += N.oneTwo;
                    signalCount--;
                }
            }

            if (signalCount < 0) x -= N.two;
            return x - ix;
        }

        public static void drawVolumeBars(Graphics g, float x, float y, VolColor col, long reward, long maxReward = -1, bool prediction = false)
        {
            g.SmoothingMode = SmoothingMode.Default;
            var ww = N.eight;
            var yy = y;

            var buckets = new List<long>()
            {
                0,
                (long)Game.settings.bioRingBucketOne * 1_000_000,
                (long)Game.settings.bioRingBucketTwo * 1_000_000,
                (long)Game.settings.bioRingBucketThree * 1_000_000,
            };

            // draw outer dotted box
            g.FillRectangle(Brushes.Black, x, y - N.oneTwo, ww, N.oneTwo);
            g.DrawRectangle(GameColors.Bio.volEdge[col], x, y - N.oneTwo, ww, N.oneFive);

            if (reward <= 0)
            {
                // (don't use tt.draw as that messes with dtx/dty)
                g.DrawString("?", GameColors.fontSmallBold, GameColors.Bio.brushPrediction, x - 0.7f, y - N.oneOne);
                return;
            }

            foreach (var bucket in buckets)
            {
                if (reward > bucket)
                {
                    g.FillRectangle(GameColors.Bio.volMin[col].brush, x, y, ww, N.three);
                    g.DrawRectangle(GameColors.Bio.volMin[col].pen, x, y, ww, N.three);
                }
                else if (maxReward > bucket)
                {
                    g.FillRectangle(GameColors.Bio.volMax[col].brush, x, y, ww, N.three);
                    g.DrawRectangle(GameColors.Bio.volMax[col].pen, x, y, ww, N.three);
                }
                y -= N.four;
            }

            // draw a grey hatch effect if we're drawing a prediction
            if (prediction)
            {
                g.FillRectangle(GameColors.Bio.brushPredictionHatch, x + N.one, y + N.five, ww - N.one, N.oneFour);
                //g.FillRectangle(GameColors.Bio.brushPredictionHatch, x, y + four, ww + one, oneSix);
            }

            if (col == VolColor.White)
            {
                g.DrawRectangle(GameColors.Bio.volEdge[col], x, yy - N.oneTwo, ww, N.oneFive);
                g.DrawRectangle(GameColors.Bio.volEdge[col], x, yy - N.oneTwo, ww, N.oneFive);
            }

            g.SmoothingMode = SmoothingMode.HighQuality;
        }
    }
}
