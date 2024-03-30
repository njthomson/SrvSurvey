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
                && Game.activeGame != null
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.Flying, GameMode.Landed)
                // show only after honking or we have Canonn data
                && Game.activeGame.systemData != null
                && Game.activeGame.systemData.bioSignalsTotal > 0
                && (Game.activeGame.systemData.honked || Game.activeGame.canonnPoi != null);
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
                if (this.IsDisposed || game.systemData == null) return;

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                this.dtx = six;
                this.dty = eight;
                var sz = new SizeF(six, six);

                this.dty += this.drawTextAt($"System bio signals: {game.systemData.bioSignalsTotal}", GameColors.brushGameOrange, GameColors.fontSmall).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;
                this.dtx = six;

                var destinationBody = game.status.Destination?.Name?.Replace(game.systemData.name, "").Replace(" ", "");


                foreach (var body in game.systemData.bodies)
                {
                    if (body.bioSignalCount == 0) continue;

                    var bodyName = body.name.Replace(game.systemData.name, "").Replace(" ", "");
                    var highlight = bodyName == destinationBody; // body.countAnalyzedBioSignals == body.bioSignalCount || bodyName == "1 f";

                    // draw body name
                    this.drawTextAt(bodyName, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, GameColors.fontMiddle);
                    this.dtx = forty;

                    // and a box for each signal
                    var boxCount = body.bioSignalCount;
                    for (var n = 0; n < boxCount; n++)
                    {
                        var fill = 0f;
                        if (n < body.organisms?.Count)
                        {
                            if (body.organisms[n].reward > 15_000_000)
                                fill = 10;
                            else if (body.organisms[n].reward > 7_000_000)
                                fill = 7;
                            else if (body.organisms[n].reward > 3_000_000)
                                fill = 4;
                            else if (body.organisms[n].reward > 0)
                                fill = 1;
                            //{
                            //    //var reward = Game.codexRef.getRewardForSpecies(body.bioScans[n].species!);
                            //    //fill = ten * body.organisms[n].reward / 20_000_000f;
                            //}
                        }

                        if (fill == 0)
                            g.DrawString("?", GameColors.fontSmall, highlight ? GameColors.brushCyan : GameColors.brushGameOrange, this.dtx + one, this.dty + six);
                        else
                            g.FillRectangle(highlight ? GameColors.brushDarkCyan : GameColors.brushGameOrangeDim, dtx, dty + one + six + ten - fill, oneOne, fill);

                        g.DrawRectangle(highlight ? GameColors.penCyan1 : GameColors.penGameOrange1, dtx, dty + six, oneOne, oneOne);

                        dtx += oneSix;
                        if (this.dtx > sz.Width) sz.Width = this.dtx;
                    }

                    this.dtx = six;
                    this.dty += oneEight;
                }

                this.dty += eight;
                var sysEstimate = game.systemData.bodies.Sum(_ => _.sumPotentialEstimate);
                var footerTxt = $"Rewards: ~{Util.credits(sysEstimate, true)}";
                if (game.systemData.bodies.Any(_ => _.bioSignalCount > 0 && _.organisms?.All(o => o.species != null) != true))
                    footerTxt += "?";

                this.dty += this.drawTextAt(footerTxt, GameColors.brushGameOrange, GameColors.fontSmall).Height;
                if (this.dtx > sz.Width) sz.Width = this.dtx;

                // resize window as necessary
                sz.Width += ten;
                sz.Height = this.dty + ten;
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
    }
}

