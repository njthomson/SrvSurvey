
using SrvSurvey;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{

    partial class PlotBase
    {
        #region scaling

        protected static float one = scaled(1f);
        protected static float two = scaled(2f);
        protected static float three = scaled(3f);
        protected static float four = scaled(4f);
        protected static float five = scaled(5f);
        protected static float six = scaled(6f);
        protected static float eight = scaled(8f);
        protected static float nine = scaled(9f);
        protected static float ten = scaled(10f);
        protected static float oneOne = scaled(11f);
        protected static float oneTwo = scaled(12f);
        protected static float oneFour = scaled(14f);
        protected static float oneFive = scaled(15f);
        protected static float oneSix = scaled(16f);
        protected static float oneEight = scaled(18f);
        protected static float oneNine = scaled(19f);
        protected static float twenty = scaled(20f);
        protected static float twoOne = scaled(21f);
        protected static float twoTwo = scaled(22f);
        protected static float twoFour = scaled(24f);
        protected static float twoSix = scaled(26f);
        protected static float twoEight = scaled(28f);
        protected static float thirty = scaled(30f);
        protected static float threeTwo = scaled(32f);
        protected static float threeFour = scaled(34f);
        protected static float threeSix = scaled(36f);
        protected static float forty = scaled(40f);
        protected static float fourFour = scaled(44f);
        protected static float fourTwo = scaled(42f);
        protected static float fourSix = scaled(46f);
        protected static float fifty = scaled(50f);
        protected static float fiveTwo = scaled(52f);
        protected static float sixty = scaled(60f);
        protected static float sixTwo = scaled(62f);
        protected static float sixFour = scaled(64f);
        protected static float sixFive = scaled(65f);
        protected static float sevenTwo = scaled(72f);
        protected static float sevenFive = scaled(75f);
        protected static float eighty = scaled(80f);
        protected static float eightSix = scaled(86f);
        protected static float eightEight = scaled(88f);
        protected static float nineSix = scaled(96f);
        protected static float hundred = scaled(100f);
        protected static float oneOhFour = scaled(104f);
        protected static float oneTwenty = scaled(120f);
        protected static float oneSeventy = scaled(170f);
        protected static float oneEightFour = scaled(184);
        protected static float oneNinety = scaled(190f);
        protected static float twoThirty = scaled(230f);
        protected static float fourHundred = scaled(400f);

        public static int scaled(int n)
        {
            return (int)(n * GameColors.scaleFactor);
        }

        public static float scaled(float n)
        {
            return (n * GameColors.scaleFactor);
        }

        public static Rectangle scaled(Rectangle r)
        {
            r.X = scaled(r.X);
            r.Y = scaled(r.Y);
            r.Width = scaled(r.Width);
            r.Height = scaled(r.Height);

            return r;
        }

        public static RectangleF scaled(RectangleF r)
        {
            r.X = scaled(r.X);
            r.Y = scaled(r.Y);
            r.Width = scaled(r.Width);
            r.Height = scaled(r.Height);

            return r;
        }

        public static Point scaled(Point pt)
        {
            pt.X = scaled(pt.X);
            pt.Y = scaled(pt.Y);

            return pt;
        }

        public static SizeF scaled(SizeF sz)
        {
            sz.Width = scaled(sz.Width);
            sz.Height = scaled(sz.Height);

            return sz;
        }

        public static Size scaled(Size sz)
        {
            sz.Width = scaled(sz.Width);
            sz.Height = scaled(sz.Height);

            return sz;
        }

        #endregion
    }
}