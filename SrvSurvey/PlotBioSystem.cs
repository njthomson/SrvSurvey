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

            this.Font = GameColors.fontMiddle;
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
            get => Game.settings.autoShowPlotBioSystemTest
                && Game.activeGame?.status != null
                && Game.activeGame.systemData != null
                && Game.activeGame.systemData.bioSignalsTotal > 0
                && Game.activeGame.status?.InTaxi != true
                //&& Game.activeGame.systemBody != null
                && (
                //Game.activeGame.isMode(GameMode.SuperCruising, GameMode.GlideMode, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel)
                //|| (Game.activeGame.mode == GameMode.Flying && Game.activeGame.status.hasLatLong)
                (Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap))
                || (Game.activeGame.systemBody?.bioSignalCount > 0 && Game.activeGame?.status?.hasLatLong == true && Game.activeGame.isMode(GameMode.GlideMode, GameMode.SAA, GameMode.FSS, GameMode.Flying, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel))
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
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            try
            {
                if (this.IsDisposed || game.systemData == null || game.status == null || !PlotBioSystem.allowPlotter) return;

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                var sz = (game.status.hasLatLong && game.systemBody?.bioSignalCount > 0) && !game.isMode(GameMode.ExternalPanel, GameMode.SystemMap)
                    ? this.drawBodyBios()
                    : this.drawSystemBios();

                if (this.Size != sz.ToSize())
                {
                    this.Size = sz.ToSize();
                    this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
                    this.Invalidate();
                    this.reposition(Elite.getWindowRect());
                }
            }
            catch (Exception ex)
            {
                Game.log($"PlotBioSystem.OnPaintBackground error: {ex}");
            }
        }

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

                var bodyName = body.name.Replace(game.systemData.name, "").Replace(" ", "");
                var highlight = body.countAnalyzedBioSignals != body.bioSignalCount && (bodyName == destinationBody ||  !anyFoo); // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";

                // draw body name
                var txt = $"{bodyName} {body.bioSignalCount}x";
                var sz2 = this.drawTextAt(txt, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle);
                // TODO: Indent to match the widest string
                this.dtx = oneTwo + sz2.Width;

                // and a box for each signal
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

                var bodySummary = game.systemData?.bioSummary?.bodyGroups.Find(_ => _.body == body);
                if (bodySummary != null)
                {
                    foreach(var foo in bodySummary.species)
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
    }
}

