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
                && game.isMode(GameMode.Flying, GameMode.SuperCruising, GameMode.GlideMode, GameMode.InSrv, GameMode.OnFoot, GameMode.OnFoot, GameMode.InTaxi, GameMode.CommsPanel, GameMode.InFighter, GameMode.Docked, GameMode.Landed, GameMode.FSDJumping, GameMode.StationServices)
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

        public override void setPosition(Rectangle? gameRect = null, string? name = null)
        {
            // get initial position
            var pt = PlotPos.getPlotterLocation(name ?? this.name, this.size, gameRect ?? Rectangle.Empty, true);
            this.left = pt.X;
            this.top = pt.Y;

            var thisRect = new Rectangle(left, top, this.width + 10, this.height + 10);
            var collider = PlotBase2.active.FirstOrDefault(other => other != this && other.rect.IntersectsWith(thisRect));
            if (collider != null)
                this.top = collider.bottom + 4;

            // this removes the need to check during rendering itself
            plotDef.form?.checkLocationAndSize();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var countUnread = game.cmdrPlay?.messagesUnread ?? -1;
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
                drawEnvelope(g, N.ten, tt.dty + 1, N.twoFour, C.Pens.orange2r);
                tt.draw(N.fourFour, countUnread.ToString(), GameColors.Fonts.console_16);
                tt.newLine(N.four, true);
            }

            drawLogo(g, this.width - N.twenty, N.eight, showStripe, 16);

            tt.setMinHeight(32);
            return tt.pad(N.twoFour, N.two);
        }

        #region logos

        private static void drawLogo32(Graphics g, float x, float y, bool highlight = false)
        {
            drawLogo(g, x, y, highlight, 16);
        }

        private static void drawLogo16(Graphics g, float x, float y, bool highlight = false)
        {
            drawLogo(g, x, y, highlight, 16);
        }

        public static void drawLogo(Graphics g, float x, float y, bool highlight, float sz)
        {
            var half = sz / 2;

            var fat = sz >= 36;
            var p2 = highlight ? (fat ? C.Pens.cyan4 : C.Pens.cyan2) : (fat ? C.Pens.orange4 : C.Pens.orange2);
            var p1 = highlight ? (fat ? C.Pens.cyan2 : C.Pens.cyan1) : (fat ? C.Pens.orange2 : C.Pens.orange1);
            var b = highlight ? C.Brushes.cyanDark : C.Brushes.orangeDark;

            g.FillRectangle(b, x, y, half, half);
            g.FillRectangle(b, x + half, y + half, half, half);
            g.DrawLine(p2, x + half, y, x + half, y + sz);
            g.DrawLine(p2, x, y + half, x + sz, y + half);
            // diagonals
            g.DrawLine(p1, x, y + half, x + half, y + sz);
            g.DrawLine(p1, x + half, y, x + sz, y + half);
        }

        public static void drawEnvelope(Graphics g, float x, float y, float sz, Pen p)
        {
            var downHeight = sz * 0.4f;
            var widthHalf = sz / 2f;

            // mail envelope
            g.DrawRectangle(p, x, y, sz, sz * 0.7f);
            g.DrawLine(p, x, y, x + widthHalf, y + downHeight);
            g.DrawLine(p, x + sz, y, x + widthHalf, y + downHeight);
        }

        public static void drawPage(Graphics g, float x, float y, float sz, Pen p)
        {
            var szh = sz * 0.6f;
            var szw = sz * 1.75f;
            var szwh = szw / 2f;

            // page
            var ws = sz * 0.3f;
            var wl = sz * 0.55f;

            var h = sz * 0.16f;
            g.DrawRectangle(p, x, y, sz * 0.8f, sz);
            g.DrawLineR(p, x + 6, (int)Math.Floor(y + h), ws, 0);
            g.DrawLineR(p, x + 6, (int)Math.Floor(y + h * 2), wl, 0);
            g.DrawLineR(p, x + 6, (int)Math.Floor(y + h * 3), wl, 0);
            g.DrawLineR(p, x + 6, (int)Math.Floor(y + h * 4), wl, 0);
            g.DrawLineR(p, x + 6 + wl, (int)Math.Floor(y + h * 5), -ws, 0);
        }

        public static void drawBackArrow(Graphics g, float x, float y, float sz, Pen p)
        {
            var bit = sz * 0.2f;
            var stick = sz * 0.4f;
            var stick2 = sz * 0.8f;
            //var widthHalf = sz / 2f;

            y += bit;
            // mail envelope
            g.DrawLineR(p, x, y, stick2, 0);
            g.DrawLineR(p, x + stick2, y, 0, stick);

            g.DrawLineR(p, x, y, bit, bit);
            g.DrawLineR(p, x, y, bit, -bit);
        }

        #endregion
    }
}
