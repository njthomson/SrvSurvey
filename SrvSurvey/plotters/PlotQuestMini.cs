using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotQuestMini : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotQuestMini),
            allowed = allowed,
            ctor = (game, def) => new PlotQuestMini(game, def),
            defaultSize = new Size(180, 200),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.enableQuests
                && game.cmdrPlay?.activeQuests.Count > 0
                && game.isMode(GameMode.Flying, GameMode.SuperCruising, GameMode.GlideMode, GameMode.InSrv, GameMode.OnFoot, GameMode.OnFoot, GameMode.InTaxi, GameMode.CommsPanel, GameMode.InFighter, GameMode.Docked, GameMode.Landed, GameMode.FSDJumping)
                ;
        }

        #endregion

        private PlotQuestMini(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.console_8;
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var countUnread = game.cmdrPlay?.activeQuests.Sum(pq => pq.unreadMessageCount) ?? -1;
            if (countUnread > 0)
            {
                tt.draw(N.eight, $"Unread: {countUnread}");
                tt.newLine(N.four, true);
            }

            drawLogo16(g, this.width - 20, 10, true);

            tt.setMinHeight(30);
            return tt.pad(N.twoFour, N.six);
        }

        #region quest logo

        private static void drawLogo32(Graphics g, int x, int y, bool highlight = false)
        {
            drawLogoN(g, x, y, highlight, 16);
        }

        private static void drawLogo16(Graphics g, int x, int y, bool highlight = false)
        {
            drawLogoN(g, x, y, highlight, 8);
        }

        private static void drawLogoN(Graphics g, int x, int y, bool highlight, int sz)
        {
            var sz2 = sz * 2;

            var b = highlight ? C.Brushes.cyanDark : C.Brushes.orangeDark;
            var p2 = highlight ? C.Pens.cyan2 : C.Pens.orange2;
            var p1 = highlight ? C.Pens.cyan1 : C.Pens.orange1;

            g.FillRectangle(b, x, y, sz, sz);
            g.FillRectangle(b, x + sz, y + sz, sz, sz);
            g.DrawLine(p2, x + sz, y, x + sz, y + sz2);
            g.DrawLine(p2, x, y + sz, x + sz2, y + sz);
            // diagonals
            g.DrawLine(p1, x, y + sz, x + sz, y + sz2);
            g.DrawLine(p1, x + sz, y, x + sz2, y + sz);
        }

        #endregion
    }
}
