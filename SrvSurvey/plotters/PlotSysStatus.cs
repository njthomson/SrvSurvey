using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotSysStatus;

namespace SrvSurvey.plotters
{
    internal class PlotSysStatus : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotSysStatus),
            allowed = allowed,
            ctor = (game, def) => new PlotSysStatus(game, def),
            defaultSize = new Size(170, 40),
            invalidationJournalEvents = new() { nameof(FSSBodySignals), nameof(FSSDiscoveryScan), nameof(Scan), },
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotSysStatus
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && game.status?.InTaxi != true
                && game.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                // show only after honking or we have Canonn data
                && game.systemData != null
                && (game.systemData.honked || game.canonnPoi != null);
        }


        #endregion

        private Font boldFont = GameColors.fontMiddleBold;

        private PlotSysStatus(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontMiddle;
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (status.changed.Contains(nameof(Status.Destination)))
                this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var minViableWidth = N.s(170);
            if (game?.systemData == null || game.status == null) // still needed --> || !PlotSysStatus.allowed(game))
            {
                this.hide();
                return frame.Size;
            }

            tt.dty = N.eight;
            tt.draw(N.six, Res.Header, GameColors.fontSmall);
            tt.newLine(0, true);
            tt.dtx = N.six;

            // reduce destination to it's short name
            var destinationBody = game.status.Destination?.Name?.Replace(game.systemData.name, "").Replace(" ", "");

            var dssRemaining = game.systemData.getDssRemainingNames();
            if (!game.systemData.honked)
            {
                tt.draw(Res.FssNotStarted, GameColors.Cyan);
            }
            else if (!game.systemData.fssComplete)
            {
                var fssProgress = 100.0 / (float)game.systemData.bodyCount * (float)game.systemData.fssBodyCount;
                var txt = dssRemaining.Count == 0
                    ? Res.FssCompleteLong.format((int)fssProgress)
                    : Res.FssCompleteShort.format((int)fssProgress);
                tt.draw(txt, GameColors.Cyan);
            }

            if (dssRemaining.Count > 0)
            {
                if (tt.dtx > 6) tt.draw(" ");
                tt.draw(Res.DssRemaining.format(dssRemaining.Count));
                this.drawRemainingBodies(g, tt, destinationBody, dssRemaining);
            }
            else if (game.systemData.fssComplete && game.systemData.honked)
            {
                tt.draw(Res.NoDssMeet);
            }
            tt.newLine(true);

            if (!Game.settings.autoShowPlotBioSystem)
            {
                var bioRemaining = game.systemData.getBioRemainingNames();
                if (bioRemaining.Count > 0)
                {
                    tt.draw(Res.BioSignals.format(game.systemData.bioSignalsRemaining));
                    this.drawRemainingBodies(g, tt, destinationBody, bioRemaining);
                }
            }

            var nonBodySignalCount = game.systemData.nonBodySignalCount;
            if (Game.settings.showNonBodySignals && nonBodySignalCount > 0)
            {
                var sz = tt.draw(N.six, Res.NonBodySignals.format(nonBodySignalCount), GameColors.fontSmall2);
                tt.newLine(true);
            }

            return tt.pad(N.six, N.eight);
        }

        /// <summary>
        /// Render names in a horizontal list, highlighting any in the same group as the destination
        /// </summary>
        private void drawRemainingBodies(Graphics g, TextCursor tt, string? destination, List<string> names)
        {
            // adjust for the fact that bold makes rendering shift up a pixel or two
            var flags = tt.flags;
            tt.flags = TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.VerticalCenter;
            var dy = (int)Math.Ceiling(tt.lastTextSize.Height / 2);
            tt.dty += dy;

            // draw each remaining body, highlighting color if they are in the same group as the destination, or all of them if no destination
            foreach (var bodyName in names)
            {
                if (bodyName == null) continue;
                var isLocal = string.IsNullOrEmpty(destination) || bodyName[0] == destination[0];

                var useFont = this.font;
                if (destination == bodyName) useFont = this.boldFont;
                var useColor = isLocal ? GameColors.Cyan : GameColors.Orange;

                tt.draw(bodyName, useColor, useFont);
                tt.dtx += 4;
            }

            // revert adjustments
            tt.dty -= dy;
            tt.flags = flags;
        }
    }
}

