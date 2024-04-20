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
            get => Game.settings.autoShowPlotBioSystem
                && Game.activeGame?.status != null
                && Game.activeGame.systemData != null
                && Game.activeGame.systemData.bioSignalsTotal > 0
                && Game.activeGame.status?.InTaxi != true
                //&& Game.activeGame.systemBody != null
                && (
                //Game.activeGame.isMode(GameMode.SuperCruising, GameMode.GlideMode, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel)
                //|| (Game.activeGame.mode == GameMode.Flying && Game.activeGame.status.hasLatLong)
                (Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap))
                || (Game.activeGame.systemBody?.bioSignalCount > 0 && Game.activeGame.status.hasLatLong && Game.activeGame.isMode(GameMode.GlideMode, GameMode.SAA, GameMode.FSS, GameMode.Flying, GameMode.Landed, GameMode.OnFoot, GameMode.CommsPanel))
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

                var sz = (game.status.hasLatLong && game.systemBody?.bioSignalCount > 0) && game.mode != GameMode.ExternalPanel
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

                this.dty += this.drawTextAt($"um ..?", GameColors.brushGameOrange, GameColors.fontSmall).Height + two;
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

                    long reward2 = organism.reward;
                    PlotBase.drawBioRing(this.g, organism.genus, dtx, dty, reward2, brush);
                    g.DrawEllipse(GameColors.penGameOrangeDim1, dtx - 1.5f, dty - 0.5f, 22, 22);
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

                    this.drawTextAt($"{Util.metersToString((decimal)organism.range, false)}         {Util.credits(reward)}", //brush, GameColors.fontSmall);
                        highlight ? GameColors.brushDarkCyan : GameColors.brushGameOrangeDim, GameColors.fontSmall);
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

            var destinationBody = game.status.Destination?.Name?.Replace(game.systemData.name, "").Replace(" ", "");

            foreach (var body in game.systemData.bodies)
            {
                if (body.bioSignalCount == 0) continue;

                var bodyName = body.name.Replace(game.systemData.name, "").Replace(" ", "");
                var highlight = bodyName == destinationBody; // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";
                                                             //highlight = bodyName == "4a";

                // draw body name
                var txt = bodyName;
                this.drawTextAt(txt, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle);
                // TODO: Indent to match the widest string
                this.dtx = forty;

                // and a box for each signal
                var signalCount = body.bioSignalCount;
                var sortedOrganisms = body.organisms?.OrderBy(_ => _.genus)?.ToList();
                for (var n = 0; n < signalCount; n++)
                {
                    var b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;

                    string? genus = null;
                    long reward = -1;
                    if (sortedOrganisms != null && n < sortedOrganisms.Count)
                    {
                        genus = sortedOrganisms[n].genus;
                        reward = sortedOrganisms[n].reward;
                        if (sortedOrganisms[n].analyzed)
                            b = Brushes.DarkGreen;
                    }

                    PlotBase.drawBioRing(this.g, genus, dtx, dty, reward, b);
                    g.DrawEllipse(GameColors.penGameOrangeDim1, dtx - 1, dty + 0.5f, 20, 20);
                    dtx += twoTwo;

                    if (this.dtx > sz.Width) sz.Width = this.dtx;
                }

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

        private void drawSignalRing(SystemOrganism org, bool highlight)
        {
            g.FillEllipse(Brushes.Black, dtx - one, dty, 20, 20);

            var img = Util.getBioImage(org.genus);
            g.DrawImage(img, dtx, dty + two, 18, 18);

            var x = dtx + four;
            var y = dty + 5.7f;

            if (org.reward > 0)
            {
                /* center dot
                // Create a path that consists of a single ellipse.
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(x, y, 10, 10);

                // Use the path to construct a brush.
                PathGradientBrush pthGrBrush = new PathGradientBrush(path);
                if (org.reward > 7_000_000)
                    pthGrBrush.CenterColor = GameColors.Orange;
                else if (org.reward > 3_000_000)
                    pthGrBrush.CenterColor = GameColors.OrangeDim;
                else if (org.reward > 0)
                    pthGrBrush.CenterColor = Color.FromArgb(255, 35, 18, 3);

                pthGrBrush.SurroundColors = new Color[] { Color.Transparent };

                g.FillEllipse(pthGrBrush, x, y, 10, 10);
                // */

                //* moon phases
                var b1 = new SolidBrush(Color.FromArgb(255, 24, 24, 24));
                var b0 = new SolidBrush(Color.FromArgb(255, 55, 28, 3));
                g.FillEllipse(b0, x, y, 10, 10);

                var b2 = GameColors.brushGameOrange;

                ////// pacman
                ////if (org.genus == "$Codex_Ent_Tubus_Genus_Name;")
                ////    g.FillPie(GameColors.brushGameOrange, x, y, 10, 10, 225, -270);
                ////else if (org.reward > 7_000_000)
                ////    g.FillPie(GameColors.brushGameOrange, x, y, 10, 10, 0, 180);
                ////else if (org.reward > 3_000_000)
                ////    //g.FillEllipse(b2, x, y, 10, 10);
                ////    g.FillPie(GameColors.brushGameOrange, x, y, 10, 10, 45, 90);
                //if (org.genus == "$Codex_Ent_Tubus_Genus_Name;")
                //    g.FillPie(GameColors.brushGameOrange, x + 1, y + 1, 8, 8, -90, 360);
                //else if (org.reward > 7_000_000)
                //    g.FillPie(GameColors.brushGameOrange, x + 1, y + 1, 8, 8, -30, 240);
                //else if (org.reward > 3_000_000)
                //    //g.FillEllipse(b2, x, y, 10, 10);
                //    g.FillPie(GameColors.brushGameOrange, x + 1, y + 1, 8, 8, 30, 120);


                //// quadrants
                if (org.genus == "$Codex_Ent_Tubus_Genus_Name;")
                    g.FillPie(highlight ? GameColors.brushCyan : GameColors.brushGameOrange, x + 1, y + 1, 8, 8, -90, 360);
                else if (org.reward > 7_000_000)
                    g.FillPie(highlight ? GameColors.brushCyan : GameColors.brushGameOrange, x + 1, y + 1, 8, 8, -90, 240);
                else if (org.reward > 3_000_000)
                    //g.FillEllipse(b2, x, y, 10, 10);
                    g.FillPie(highlight ? GameColors.brushCyan : GameColors.brushGameOrange, x + 1, y + 1, 8, 8, -90, 120);
                else
                    //g.FillEllipse(b2, x, y, 10, 10);
                    g.FillPie(highlight ? GameColors.brushCyan : GameColors.brushGameOrange, x, y, 10, 10, -90, 20);
                // */
            }

            dtx += img.Width + four;
        }

        private void drawSignalBox(SystemOrganism org, bool highlight)
        {
            // Filled squares by value
            var fill = 0f;
            if (org.reward > 15_000_000)
                fill = 10;
            else if (org.reward > 7_000_000)
                fill = 7;
            else if (org.reward > 3_000_000)
                fill = 4;
            else if (org.reward > 0)
                fill = 1;
            //{
            //    //var reward = Game.codexRef.getRewardForSpecies(body.bioScans[n].species!);
            //    //fill = ten * body.organisms[n].reward / 20_000_000f;
            //}

            if (fill == 0)
                g.DrawString("?", GameColors.fontSmall, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, this.dtx + one, this.dty + six);
            else
                g.FillRectangle(highlight ? GameColors.brushDarkCyan : GameColors.brushGameOrangeDim, dtx, dty + one + six + ten - fill, oneOne, fill);

            g.DrawRectangle(highlight ? GameColors.penCyan1 : GameColors.penGameOrange1, dtx, dty + six, oneOne, oneOne);

            dtx += oneSix;
        }
    }
}

