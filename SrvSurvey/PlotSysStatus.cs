﻿using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotSysStatus : PlotBase, PlotterForm
    {
        public string? nextSystem;
        private Font boldFont = GameColors.fontMiddleBold;

        private PlotSysStatus() : base()
        {
            this.Width = scaled(48);
            this.Height = scaled(48);
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.Name = nameof(PlotSysStatus);

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
            get => Game.settings.autoShowPlotSysStatus
                && Game.activeGame != null
                && Game.activeGame.status?.InTaxi != true
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                // show only after honking or we have Canonn data
                && Game.activeGame.systemData != null
                && (Game.activeGame.systemData.honked || Game.activeGame.canonnPoi != null);
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            //var targetMode = this.game.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap, GameMode.CommsPanel);
            if (this.Opacity > 0 && !PlotSysStatus.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotSysStatus.allowPlotter)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected override void onJournalEntry(FSSBodySignals entry)
        {
            Game.log($"PlotSysStatus: FSSBodySignals event: {entry.Bodyname}");
            this.Invalidate();
        }

        protected override void onJournalEntry(FSSDiscoveryScan entry)
        {
            Game.log($"PlotSysStatus: FSSDiscoveryScan event: {entry.SystemName}");
            this.nextSystem = null;
            this.Invalidate();
        }

        protected override void onJournalEntry(Scan entry)
        {
            Game.log($"PlotSysStatus: Scan event: {entry.Bodyname}");
            this.nextSystem = null;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var minViableWidth = scaled(170);
            try
            {
                if (this.IsDisposed || game?.systemData == null || game.status == null || !PlotSysStatus.allowPlotter)
                {
                    this.Opacity = 0;
                    return;
                }

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;

                this.dtx = scaled(6.0f);
                this.dty = scaled(19.0f);

                // reduce destination to it's short name
                var destinationBody = game.status.Destination?.Name?.Replace(game.systemData.name, "").Replace(" ", "");

                if (this.nextSystem != null)
                {
                    // render next system only, if populated
                    this.drawTextAt("Next system:");
                    this.drawTextAt(this.nextSystem, GameColors.brushCyan);
                    return;
                }

                var dssRemaining = game.systemData.getDssRemainingNames();

                if (!game.systemData.honked)
                {
                    this.drawTextAt($"FSS not started", GameColors.brushCyan);
                }
                else if (!game.systemData.fssComplete)
                {
                    var fssProgress = 100.0 / (float)game.systemData.bodyCount * (float)game.systemData.fssBodyCount;
                    this.drawTextAt($"FSS {(int)fssProgress}% complete", GameColors.brushCyan);
                }
                else if (dssRemaining.Count > 0)
                {
                    this.drawTextAt($"{dssRemaining.Count}x bodies: ");
                    this.drawRemainingBodies(destinationBody, dssRemaining);
                }
                else
                {
                    this.drawTextAt("No DSS meet criteria");
                }

                if (!Game.settings.autoShowPlotBioSystem)
                {
                    var bioRemaining = game.systemData.getBioRemainingNames();
                    if (bioRemaining.Count > 0)
                    {
                        this.drawTextAt($"| {game.systemData.bioSignalsRemaining}x Bio signals on: ");
                        this.drawRemainingBodies(destinationBody, bioRemaining);
                    }
                }

                var headerTxt = "";
                if (false && game.systemData.fssComplete && (Game.settings.skipLowValueDSS || Game.settings.skipHighDistanceDSS || !Game.settings.skipRingsDSS))
                {
                    minViableWidth += scaled(74);
                    headerTxt += "(filtered)";
                    //if (Game.settings.skipLowValueDSS)
                    //{
                    //    headerTxt += $" >{Util.credits(Game.settings.skipLowValueAmount)}";
                    //    minViableWidth += 80;
                    //}
                    //if (!Game.settings.skipRingsDSS)
                    //{
                    //    headerTxt += " +Rings";
                    //    minViableWidth += 45;
                    //}
                    //if (Game.settings.skipHighDistanceDSS)
                    //{
                    //    headerTxt += $" <{Game.settings.skipHighDistanceDSSValue / 1000}K LS";
                    //    minViableWidth += 60;
                    //}
                    //headerTxt += ")";
                }
                g.DrawString($"System survey remaining: {headerTxt}", GameColors.fontSmall, GameColors.brushGameOrange, scaled(4), scaled(7));
            }
            catch (Exception ex)
            {
                Game.log($"PlotSysStatus.OnPaintBackground error: {ex}");
            }
            finally
            {
                // resize window to fit as necessary
                this.Width = Math.Max((int)this.dtx, minViableWidth);
            }
        }

        /// <summary>
        /// Render names in a horizontal list, highlighting any in the same group as the destination
        /// </summary>
        private void drawRemainingBodies(string? destination, List<string> names)
        {
            const TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

            // draw each remaining body, highlighting color if they are in the same group as the destination, or all of them if no destination
            foreach (var bodyName in names)
            {
                var isLocal = string.IsNullOrEmpty(destination) || bodyName[0] == destination[0];

                var font = this.Font;

                if (destination == bodyName) font = this.boldFont;
                var color = isLocal ? GameColors.Cyan : GameColors.Orange;

                var sz = g.MeasureString(bodyName, font).ToSize();
                var rect = new Rectangle((int)this.dtx, (int)this.dty, sz.Width, sz.Height);

                TextRenderer.DrawText(g, bodyName, font, rect, color, flags);
                this.dtx += sz.Width;
            }
        }
    }
}

