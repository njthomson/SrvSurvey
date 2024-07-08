using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotBioSystem : PlotBase, PlotterForm
    {
        private PlotBioSystem() : base()
        {
            this.Width = scaled(420);
            this.Height = scaled(88);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = GameColors.fontSmall;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
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

        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotBioSystem
                && Game.activeGame?.status != null
                && Game.activeGame.systemData != null
                && Game.activeGame.systemData.bioSignalsTotal > 0
                && !Game.activeGame.status.InTaxi
                && !Game.activeGame.status.OnFootSocial
                && !Game.activeGame.hidePlottersFromCombatSuits
                && (Game.activeGame.humanSite == null || Game.activeGame.mode == GameMode.ExternalPanel) // why was this commented? For external panel?
                && (
                    Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                    || (
                            Game.activeGame.systemBody?.bioSignalCount > 0
                            && Game.activeGame.status.hasLatLong == true
                            && Game.activeGame.isMode(GameMode.GlideMode, GameMode.Flying, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel, GameMode.InSrv)
                        )
                );
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !PlotBioSystem.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotBioSystem.allowPlotter)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();

            if (newMode == GameMode.MainMenu) this.Close();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            try
            {
                if (this.IsDisposed || game.systemData == null || game.status == null || !PlotBioSystem.allowPlotter)
                {
                    this.Opacity = 0;
                    return;
                }

                resetPlotter(e.Graphics);
                var showBodyBios = !game.isMode(GameMode.ExternalPanel, GameMode.SystemMap, GameMode.Orrery)
                    && game.status.hasLatLong
                    && game.systemBody?.bioSignalCount > 0
                    && (game.targetBody == null || game.targetBody == game.systemBody);

                if (showBodyBios)
                    this.drawBodyBios2();
                else
                    this.drawSystemBios2();
            }
            catch (Exception ex)
            {
                Game.log($"PlotBioSystem.OnPaintBackground error: {ex}");
            }
        }

        #region ring based UX

        private SizeF drawBodyBios()
        {
            this.dtx = six;
            this.dty = eight;
            var sz = new SizeF(six, six);

            if (game?.systemBody?.organisms == null)
            {
                this.dty += this.drawTextAt($"Body bio signals: {game?.systemBody?.bioSignalCount}", GameColors.brushGameOrange, GameColors.fontSmall).Height + two;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
                this.dtx = six;
                this.dty += six;

                this.dty += this.drawTextAt($"DSS required", GameColors.brushCyan, GameColors.fontSmall).Height + two;
            }
            else
            {
                this.dty += this.drawTextAt($"Body bio signals: {game.systemBody.bioSignalCount}", GameColors.brushGameOrange, GameColors.fontSmall).Height + two;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
                this.dtx = six;
                this.dty += six;

                foreach (var organism in game.systemBody.organisms)
                {
                    var highlight = !organism.analyzed && (game.cmdr.scanOne?.genus == organism.genus || game.cmdr.scanOne?.genus == null);
                    var brush = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;

                    PlotBase.drawBioRing(this.g, organism.genus, dtx, dty, organism.reward, highlight);
                    //g.DrawEllipse(GameColors.penGameOrangeDim1, dtx - 1.5f, dty - 0.5f, 22, 22);
                    //g.DrawEllipse(GameColors.penGameOrangeDim1, dtx - 1, dty + 1, 20, 20);
                    dtx = thirty;

                    //if (organism.analyzed)
                    //    brush = GameColors.brushGameOrangeDim;

                    // draw body name
                    var displayName = organism.genusLocalized;
                    if (!string.IsNullOrEmpty(organism.variantLocalized))
                        displayName = organism.variantLocalized;
                    else if (!string.IsNullOrEmpty(organism.speciesLocalized))
                        displayName = organism.speciesLocalized;

                    this.drawTextAt(displayName, brush, GameColors.fontSmall);
                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                    //this.dtx = forty;

                    this.dtx = forty;
                    this.dty += oneSix;
                    var reward = game.systemBody!.firstFootFall ? organism.reward * 5 : organism.reward;

                    var rewardTxt = reward > 0 ? Util.credits(reward) : "?";
                    this.drawTextAt(
                        $"{Util.metersToString((decimal)organism.range, false)}         {rewardTxt}", //brush, GameColors.fontSmall);
                        highlight ? GameColors.brushDarkCyan : GameColors.brushGameOrangeDim,
                        GameColors.fontSmall);
                    if (this.dtx > sz.Width) sz.Width = this.dtx;

                    this.dtx = six;
                    this.dty += oneSix;
                }

                this.dty += two;
                var rewardEstimate = (long)game.systemBody.sumPotentialEstimate;
                var footerTxt = "?";
                if (rewardEstimate > 0)
                {
                    footerTxt = $"Rewards: ~{Util.credits(rewardEstimate, true)}";
                    if (game.systemBody.firstFootFall) footerTxt += " (FF bonus)";

                    if (game.systemBody.bioSignalCount > 0 && game.systemBody.organisms?.All(o => o.species != null) != true)
                        footerTxt += "?";
                }
                this.dty += this.drawTextAt(footerTxt, GameColors.brushGameOrange, GameColors.fontSmall).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
            }

            // resize window as necessary
            sz.Width += ten;
            sz.Height = this.dty + ten;

            return sz;
        }

        private SizeF drawSystemBios()
        {
            this.dtx = six;
            this.dty = eight;
            var sz = new SizeF(six, six);

            this.dty += this.drawTextAt($"System bio signals: {game.systemData!.bioSignalsTotal}", GameColors.brushGameOrange, GameColors.fontSmall).Height + two;
            if (this.dtx > sz.Width) sz.Width = this.dtx;
            this.dtx = six;

            var anyFoo = game.systemData.bodies.Any(b => b.id == game.status.Destination?.Body && b.bioSignalCount > 0);
            var destinationBody = game.targetBodyShortName;

            foreach (var body in game.systemData.bodies)
            {
                if (body.bioSignalCount == 0) continue;

                var bodyName = body.name.Replace(game?.systemData?.name!, "").Replace(" ", "");
                var highlight = body.countAnalyzedBioSignals != body.bioSignalCount && (bodyName == destinationBody || !anyFoo); // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";

                // draw body name
                var txt = $"{bodyName} {body.bioSignalCount}x";
                var sz2 = this.drawTextAt(txt, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle);
                // TODO: Indent to match the widest string
                this.dtx = oneTwo + sz2.Width;

                // and a ring for each signal
                var signalCount = body.bioSignalCount;
                var sortedOrganisms = body.organisms?.OrderBy(_ => _.genus)?.ToList();
                for (var n = 0; n < signalCount; n++)
                {
                    string? genus = null;
                    long reward = -1;
                    if (sortedOrganisms != null && n < sortedOrganisms.Count)
                    {
                        genus = sortedOrganisms[n].genus;
                        reward = sortedOrganisms[n].reward;
                    }

                    if (genus == null) continue;
                    PlotBase.drawBioRing(this.g, genus, dtx, dty, reward, highlight);

                    dtx += twoFour;
                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                }

                var bodySummary = game?.systemData?.bioSummary?.bodyGroups.Find(_ => _.body == body);
                if (bodySummary != null)
                {
                    foreach (var foo in bodySummary.species)
                    {
                        var genus = Util.getGenusNameFromVariant(foo.name) + "Genus_Name;";
                        PlotBase.drawBioRing(this.g, genus, dtx, dty, foo.bioRef.reward, highlight);

                        dtx += twoFour;
                    }

                    var txt2 = $" {Util.credits(bodySummary.minReward, true)}";
                    this.drawTextAt(txt2, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle);
                }

                if (this.dtx > sz.Width) sz.Width = this.dtx;


                this.dtx = six;
                this.dty += twoFour;
            }

            this.dty += two;
            var sysEstimate = game.systemData.bodies.Sum(_ => _.sumPotentialEstimate);
            var footerTxt = $"Rewards: ~{Util.credits(sysEstimate, true)}";
            if (game.systemData.bodies.Any(_ => _.bioSignalCount > 0 && _.organisms?.All(o => o.species != null) != true))
                footerTxt += "?";

            this.dty += this.drawTextAt(footerTxt, GameColors.brushGameOrange, GameColors.fontSmall).Height;
            if (this.dtx > sz.Width) sz.Width = this.dtx;

            // resize window as necessary
            sz.Width += ten;
            sz.Height = this.dty + ten;

            return sz;
        }

        #endregion

        private void drawBodyBios2()
        {
            if (game?.systemBody == null) return;
            var body = game.systemBody;

            drawTextAt(eight, $"Body {body.shortName} bio signals: {body.bioSignalCount}", GameColors.brushGameOrange);
            newLine(+eight, true);

            if (body.organisms == null)
            {
                this.drawTextAt(ten, $"DSS required", GameColors.brushCyan);
                newLine(+eight, true);
            }
            else
            {
                // draw a row for each organism
                var first = true;
                foreach (var organism in body.organisms)
                {
                    var highlight = !organism.analyzed && (game.cmdr.scanOne?.genus == organism.genus || game.cmdr.scanOne?.genus == null);
                    var brush = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
                    dty = (int)dty;

                    if (first)
                        first = false;
                    else
                        g.DrawLine(GameColors.penGameOrangeDim1, four, dty - four, this.ClientSize.Width - eight, dty - four);

                    var minReward = body.getBioRewardForGenus(organism, true);
                    var maxReward = body.getBioRewardForGenus(organism, false);
                    drawVolumeBars(g, oneTwo, dty + oneSix, highlight, minReward, maxReward);

                    // displayName is either genus, or species/variant without the genus prefix
                    var displayName = organism.genusLocalized;
                    if (!string.IsNullOrEmpty(organism.variantLocalized))
                        displayName = organism.variantLocalized.Replace(organism.genusLocalized + " ", "");
                    else if (!string.IsNullOrEmpty(organism.speciesLocalized))
                        displayName = organism.speciesLocalized.Replace(organism.genusLocalized + " ", "");
                    else
                    {
                        // if we have a matching prediction - show the variant name without the genus prefix
                        var match = body.predictions.Values.FirstOrDefault(p => p.species.genus.name == organism.genus);
                        if (match != null)
                            displayName = "? " + match.englishName.Replace(match.species.genus.englishName, "").Trim() + " ?";
                    }

                    // line 1
                    var sz2 = this.drawTextAt(twoEight, displayName, brush);
                    if (organism.analyzed)
                    {
                        // strike-through if already analyzed
                        var y = dty + six;
                        g.DrawLine(GameColors.penGameOrange1, twoEight, y, dtx, y);
                        g.DrawLine(GameColors.penGameOrangeDim1, twoEight + 1, y + 1, dtx + 1, y + 1);
                    }
                    newLine(+one, true);

                    // 2nd line - right
                    if (body.firstFootFall)
                    {
                        minReward *= 5;
                        maxReward *= 5;
                    }
                    var sz = drawTextAt(
                        this.ClientSize.Width - ten,
                        Util.getMinMaxCredits(minReward, maxReward),
                        highlight ? GameColors.brushCyan : GameColors.brushGameOrange,
                        null, true);

                    // 2nd line - left
                    drawTextAt(
                        twoEight,
                        displayName != organism.genusLocalized ? organism.genusLocalized : "?",
                        highlight ? GameColors.brushCyan : GameColors.brushGameOrange);
                    dtx += sz.Width + ten;
                    newLine(+eight, true);
                }
            }

            this.dty += two;
            var footerTxt = $"Rewards: {body.minMaxBioRewards}";
            if (body.firstFootFall) footerTxt += " (FF bonus)";
            drawTextAt(eight, footerTxt, GameColors.brushGameOrange);
            newLine(true);

            // resize window as necessary
            formAdjustSize(+ten, +ten);
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
            foreach (var body in sortedBodies)
            {
                if (body.bioSignalCount == 0) continue;

                var highlight = body.shortName == destinationBody || (body.countAnalyzedBioSignals != body.bioSignalCount && body.countAnalyzedBioSignals > 0); // || !anyFoo); // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";
                dty = (float)Math.Round(dty);

                // draw body name
                var sz2 = this.drawTextAt(eight, body.shortName, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle);
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
                        drawVolumeBars(g, x, dty + oneFive, highlight, min, max);
                        x += oneTwo;
                    }
                    signalCount -= body.organisms.Count;
                }

                // and draw more boxes for those we don't
                // using the first and last for the min/max of all the potentials
                // and ? boxes for anything in between
                if (signalCount == 1)
                {
                    //long min = body.predictions.Count > 0 ? body.predictions.Values.Min(p => p.reward) : -1;
                    long min = body.predictions.Count > 0 ? body.predictions.Values.Min(p => p.reward) : 0;
                    long max = body.predictions.Count > 0 ? body.predictions.Values.Max(p => p.reward) : -1;
                    drawVolumeBars(g, x, dty + oneFive, highlight, min, max);
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

                    drawVolumeBars(g, x, dty + oneFive, highlight, min, max);
                    x += oneTwo;
                }

                // credits
                drawTextAt(this.ClientSize.Width - ten, " " + body.minMaxBioRewards, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle, true);
                dtx = lastTextSize.Width + boxRight + ten;
                newLine(+four, true);
            }

            this.dty += four;
            var footerTxt = $"Rewards: {Util.getMinMaxCredits(game.systemData.minBioRewards, game.systemData.maxBioRewards)}";
            this.drawTextAt(six, footerTxt, GameColors.brushGameOrange, GameColors.fontSmall);
            newLine(+two, true);


            formAdjustSize(+ten, +six);
        }

        public static void drawVolumeBars(Graphics g, float x, float y, bool highlight, long reward, long maxReward = -1)
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

