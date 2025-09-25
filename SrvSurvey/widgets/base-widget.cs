using SrvSurvey.game;

namespace SrvSurvey.widgets
{
    internal class BaseWidget : N
    {
        protected Game? game { get => Game.activeGame; }

        // Same as Util.textFlags
        public static readonly TextFormatFlags defaultTextFlags = TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsTranslateTransform;

        public static void renderText(Graphics g, string? txt, float x, float y, Font font, Color? color = null, TextFormatFlags? flags = null)
        {
            renderText(g, txt, new Point((int)x, (int)y), font, color, flags);
        }

        public static void renderText(Graphics g, string? txt, int x, int y, Font font, Color? color = null, TextFormatFlags? flags = null)
        {
            renderText(g, txt, new Point(x, y), font, color, flags);
        }

        public static void renderText(Graphics g, string? txt, Point pt, Font font, Color? color = null, TextFormatFlags? flags = null)
        {
            if (string.IsNullOrEmpty(txt)) return;

            color ??= C.orange;
            flags ??= BaseWidget.defaultTextFlags;
            TextRenderer.DrawText(g, txt, font, pt, color.Value, flags.Value);
        }

        public static void renderBearingTo(Graphics g, float x, float y, float r, double deg, Brush? brush = null, Pen? pen = null)
        {
            renderBearingTo(g, x, y, r, deg, null, brush, pen, null);
        }

        public static void renderBearingTo(Graphics g, float x, float y, float r, double deg, string? msg = null, Brush? brush = null, Pen? pen = null, Font? font = null)
        {
            if (g == null) return;
            font ??= GameColors.fontSmall;
            brush ??= C.Brushes.orange;
            pen ??= C.Pens.orange2;

            // draw circle
            var rect = new RectangleF(x, y, r * 2, r * 2);
            g.DrawEllipse(pen, rect);

            // draw pointing hand
            x += r;
            y += r;
            var pt = Util.rotateLine(deg, r * 1.8f);
            g.DrawLine(pen, x, y, x + pt.X, y - pt.Y);

            if (msg != null)
            {
                x += two + rect.Width;
                // center text vertically at middle of circle
                var sz = g.MeasureString(msg, font);
                var dy = rect.Height > sz.Height
                    ? Util.centerIn(rect.Height, sz.Height)
                    : Util.centerIn(sz.Height, rect.Height);
                y += -r + dy;

                g.DrawString(msg, GameColors.fontSmall, brush, x, y);
            }
        }
    }

    /// <summary>
    /// A collection of constants pre-scaled
    /// </summary>
    internal class N
    {
        #region util functions

        public static int s(int n)
        {
            return (int)(n * GameColors.scaleFactor);
        }

        public static float s(float n)
        {
            return (n * GameColors.scaleFactor);
        }

        public static Rectangle s(Rectangle r)
        {
            r.X = s(r.X);
            r.Y = s(r.Y);
            r.Width = s(r.Width);
            r.Height = s(r.Height);

            return r;
        }

        public static RectangleF s(RectangleF r)
        {
            r.X = s(r.X);
            r.Y = s(r.Y);
            r.Width = s(r.Width);
            r.Height = s(r.Height);

            return r;
        }

        public static Point s(Point pt)
        {
            pt.X = s(pt.X);
            pt.Y = s(pt.Y);

            return pt;
        }

        public static SizeF s(SizeF sz)
        {
            sz.Width = s(sz.Width);
            sz.Height = s(sz.Height);

            return sz;
        }

        #endregion

        public static float one = s(1f);
        public static float two = s(2f);
        public static float three = s(3f);
        public static float four = s(4f);
        public static float five = s(5f);
        public static float six = s(6f);
        public static float eight = s(8f);
        public static float nine = s(9f);
        public static float ten = s(10f);
        public static float oneOne = s(11f);
        public static float oneTwo = s(12f);
        public static float oneFour = s(14f);
        public static float oneFive = s(15f);
        public static float oneSix = s(16f);
        public static float oneEight = s(18f);
        public static float oneNine = s(19f);
        public static float twenty = s(20f);
        public static float twoOne = s(21f);
        public static float twoTwo = s(22f);
        public static float twoFour = s(24f);
        public static float twoFourty = s(240f);
        public static float twoSix = s(26f);
        public static float twoEight = s(28f);
        public static float thirty = s(30f);
        public static float threeTwo = s(32f);
        public static float threeFour = s(34f);
        public static float threeSix = s(36f);
        public static float threeEight = s(38f);
        public static float forty = s(40f);
        public static float fourFour = s(44f);
        public static float fourTwo = s(42f);
        public static float fourSix = s(46f);
        public static float fifty = s(50f);
        public static float fiveTwo = s(52f);
        public static float sixty = s(60f);
        public static float sixTwo = s(62f);
        public static float sixFour = s(64f);
        public static float sixFive = s(65f);
        public static float sevenTwo = s(72f);
        public static float sevenFive = s(75f);
        public static float eighty = s(80f);
        public static float eightSix = s(86f);
        public static float eightEight = s(88f);
        public static float nineSix = s(96f);
        public static float hundred = s(100f);
        public static float oneOhFour = s(104f);
        public static float oneTwenty = s(120f);
        public static float oneForty = s(140f);
        public static float oneSeventy = s(170f);
        public static float oneEightFour = s(184);
        public static float oneNinety = s(190f);
        public static float twoThirty = s(230f);
        public static float fourHundred = s(400f);
    }
}
