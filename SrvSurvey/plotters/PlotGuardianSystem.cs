using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotGuardianSystem;

namespace SrvSurvey.plotters
{
    internal class PlotGuardianSystem : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotGuardianSystem),
            allowed = allowed,
            ctor = (game, def) => new PlotGuardianSystem(game, def),
            defaultSize = new Size(300, 200), // Not 320, 88 ?
        };

        public static bool allowed(Game game)
        {
            // Game.settings.enableGuardianSites && Game.settings.autoShowGuardianSummary && PlotGuardianSystem.allowPlotter && game?.systemData?.settlements.Count > 0
            return Game.settings.enableGuardianSites
                && Game.settings.autoShowGuardianSummary
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && Game.activeGame?.systemData != null
                && Game.activeGame.systemData.settlements.Count > 0
                && !PlotFSSInfo.forceShow && !PlotBodyInfo.forceShow
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap);
        }

        #endregion

        private PlotGuardianSystem(Game game, PlotDef def) : base(game, def)
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
            if (game.systemData == null) return frame.Size;

            var sites = game?.systemData?.settlements;
            tt.draw(Res.Header.format(sites?.Count ?? 0), C.orange, GameColors.fontSmall);
            tt.newLine(N.two, true);

            if (sites?.Count > 0)
            {
                foreach (var site in sites)
                {
                    var highlight = game?.status?.Destination?.Body == site.body.id;
                    if (highlight && game?.status?.Destination.Name.StartsWith("$Ancient") == true && game?.status?.Destination.Name != site.name)
                        highlight = false;
                    var col = highlight ? C.cyan : C.orange;

                    // draw main text (bigger font)
                    tt.draw(N.eight, site.displayText, col);
                    tt.newLine(N.two, true);

                    // draw status (smaller font)
                    if (site.bluePrint != null)
                    {
                        tt.draw(N.twenty, "► " + Res.BluePrintLine.format(site.bluePrint), col, GameColors.fontSmall);
                        tt.newLine(N.two, true);
                    }

                    if (site.status != null)
                    {
                        tt.draw(N.twenty, "► " + Res.SurveyLine.format(site.status), col, GameColors.fontSmall);
                        tt.newLine(N.two, true);
                    }

                    // draw extra (smaller font)
                    if (site.extra != null)
                    {
                        tt.draw(N.twenty, "► " + site.extra, col, GameColors.fontSmall);
                        tt.newLine(N.two, true);
                    }
                }
            }

            return tt.pad(N.ten, N.ten);
        }
    }
}

