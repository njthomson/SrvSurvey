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

        bool showStripe = false;

        private PlotQuestMini(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.console_8;
        }

        protected override Bitmap getBackgroundImage(Size sz)
        {
            return GameGraphics.getBackgroundImage(sz, !showStripe);
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var countUnread = game.cmdrPlay?.activeQuests.Sum(pq => pq.unreadMessageCount) ?? -1;
            var hasUnreadMsgs = countUnread > 0;
            var newShowStripe = hasUnreadMsgs;
            if (newShowStripe != showStripe)
            {
                showStripe = newShowStripe;
                background = getBackgroundImage(this.size);
                g.DrawImage(background, 0, 0);
            }

            if (hasUnreadMsgs)
            {
                tt.dty = N.ten;
                drawMsgsN(g, N.ten, tt.dty, N.oneSix, false);
                tt.draw(N.fourFour, countUnread.ToString(), GameColors.Fonts.console_16);
                tt.newLine(N.four, true);
            }

            drawLogo16(g, this.width - N.twenty, N.ten, showStripe);

            tt.setMinHeight(32);
            return tt.pad(N.twoFour, N.two);
        }

        #region logos

        private static void drawLogo32(Graphics g, float x, float y, bool highlight = false)
        {
            drawLogoN(g, x, y, highlight, 16);
        }

        private static void drawLogo16(Graphics g, float x, float y, bool highlight = false)
        {
            drawLogoN(g, x, y, highlight, 8);
        }

        private static void drawLogoN(Graphics g, float x, float y, bool highlight, float sz)
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

        public static void drawMsgsN(Graphics g, float x, float y, float sz, bool highlight)
        {
            var szh = sz / 2f;
            var szw = sz * 1.75f;
            var szwh = szw / 2f;

            //var p = highlight ? penOrangeDark : penOrange;
            var p2 = highlight ? C.Pens.cyan2r : C.Pens.orange2r;
            //var p1 = highlight ? C.Pens.cyan1 : C.Pens.orange1;

            // mail envelope
            g.DrawRectangle(p2, x, y, szw, sz);
            g.DrawLine(p2, x, y, x + szwh, y + szh);
            g.DrawLine(p2, x + szw, y, x + szwh, y + szh);
        }

        #endregion
    }
}
