using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.quests;
using SrvSurvey.units;
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
                && game.isMode(GameMode.Flying, GameMode.SuperCruising, GameMode.GlideMode, GameMode.InSrv, GameMode.OnFoot, GameMode.OnFootInStation, GameMode.InTaxi, GameMode.CommsPanel, GameMode.InFighter, GameMode.Docked, GameMode.Landed, GameMode.FSDJumping, GameMode.StationServices, GameMode.ExternalPanel)
                ;
        }

        #endregion

        bool showStripe = false;
        bool hasBodyMarkers = false;

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

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);

            if (hasBodyMarkers)
                if (status.changed.Contains(nameof(Status.Heading)) || status.changed.Contains(nameof(Status.Latitude)) || status.changed.Contains(nameof(Status.Longitude)))
                    this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            drawLogo(g, N.eight, N.eight, showStripe, 16);

            var pq = game.cmdrPlay?.activeQuests.FirstOrDefault();
            var countUnread = game.cmdrPlay?.messagesUnread ?? -1;
            var hasUnreadMsgs = countUnread > 0;
            var newShowStripe = hasUnreadMsgs; // || pq?.objectives.Any() == true;
            if (newShowStripe != showStripe)
            {
                showStripe = newShowStripe;
                background = getBackgroundImage(this.size);
                g.DrawImage(background, 0, 0);
            }
            tt.dty = N.six;

            if (pq != null)
            {
                //tt.dty -= N.two;
                foreach (var (key, obj) in pq.objectives)
                    if (obj.state == PlayObjective.State.visible)
                        PanelQuest.drawObjective(g, tt, C.orange, key, obj, pq, false, null, N.twenty);

                tt.dty -= N.four;
                //tt.newLine(true);
            }

            if (hasUnreadMsgs)
            {
                tt.dty += N.six;
                //drawEnvelope(g, N.ten, tt.dty + 1, N.twoFour, C.Pens.orange2r);
                tt.draw(N.threeTwo, $"Unread messages: {countUnread}", C.cyan, GameColors.Fonts.gothic_9);
                tt.newLine(N.four, true);
            }

            hasBodyMarkers = false;
            if (game.cmdrPlay?.activeQuests.Count > 0)
                drawQuestMarkers(g, tt);

            tt.setMinWidth(24);
            tt.setMinHeight(32);
            return tt.pad(N.ten, N.two);
        }

        private void drawQuestMarkers(Graphics g, TextCursor tt)
        {
            if (game.cmdrPlay?.activeQuests == null) return;
            var cmdr = Status.here;
            var radius = game.status.PlanetRadius;

            foreach (var pq in game.cmdrPlay.activeQuests)
            {
                if (pq.bodyLocations.Count == 0) continue;
                hasBodyMarkers = true;
                tt.setMinWidth(N.oneForty);

                foreach (var (key, ll3) in pq.bodyLocations)
                {
                    tt.dty += N.six;

                    var target = LatLong2.from(ll3);
                    var angle2d = Util.getBearing(cmdr, target);
                    var dist2d = Util.getDistance(cmdr, target, radius);

                    var b = C.Brushes.menuGold;
                    var p = C.Pens.menuGold3r;
                    if ((double)dist2d < ll3.size)
                    {
                        b = C.Brushes.cyan;
                        p = C.Pens.cyan3r;
                    }

                    if ((double)dist2d < ll3.size)
                        drawCheckMark(g, N.eight, tt.dty, 12, p);

                    tt.draw(N.threeTwo, Util.metersToString(dist2d), p.Color);

                    var deg = angle2d - game.status.Heading;
                    if (deg < 0) deg += 360;
                    if (dist2d == 0) deg += game.status.Heading;

                    BaseWidget.renderBearingTo(g, tt.dtx + N.ten, tt.dty, N.eight, (double)deg, key, b, p);
                    tt.newLine(N.ten, true);
                }
            }
        }

        #region logos

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

            y += bit;
            g.DrawLineR(p, x, y, stick2, 0);
            g.DrawLineR(p, x + stick2, y, 0, stick);

            g.DrawLineR(p, x, y, bit, bit);
            g.DrawLineR(p, x, y, bit, -bit);
        }

        public static void drawCheckMark(Graphics g, float x, float y, float sz, Pen p)
        {
            var h = 1 + (sz * 0.4f);

            y += h;
            g.DrawLineR(p, x, y, h, h);
            g.DrawLineR(p, x + h, y+ h, sz, -sz);
        }

        #endregion
    }
}
