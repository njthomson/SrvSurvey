using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey.widgets
{
    /// <summary>
    /// Repository of all named colors, pens and brushes
    /// </summary>
    internal static class C
    {
        // seed with default values
        private static readonly Theme theme = Theme.loadTheme();

        /// <summary> Get a named colour </summary>
        public static Color c(string name)
        {
            if (!theme.ContainsKey(name))
            {
                // named color not found, pull from the original theme?
                var defaultTheme = Theme.loadTheme(true);
                if (!defaultTheme.ContainsKey(name))
                    throw new Exception($"Unexpected color name: {name}");

                // update theme and save
                theme[name] = defaultTheme[name];
                // TODO: save?
            }
            return theme[name];
        }

        #region common colors

        public static Color orange = c("orange");
        public static Color orangeDark = c("orangeDark");

        public static Color cyan = c("cyan");
        public static Color cyanDark = c("cyanDark");

        public static Color red = c("red");
        public static Color redDark = c("redDark");

        public static Color yellow = c("yellow");
        public static Color green = c("green");

        #endregion

        public static class Pens
        {
            public static Pen orange1 = orange.toPen(1);
            public static Pen orange2 = orange.toPen(2);
            public static Pen orange4 = orange.toPen(4);

            public static Pen orangeDark1 = orangeDark.toPen(1);

            public static Pen cyan1 = cyan.toPen(1);

            public static Pen cyanDark1 = cyanDark.toPen(1);

            public static Pen black1 = Color.Black.toPen(1);
        }

        public static class Brushes
        {
            public static Brush orange = C.orange.toBrush();
            public static Brush orangeDark = C.orangeDark.toBrush();
            public static Brush cyan = C.cyan.toBrush();
            public static Brush cyanDark = C.cyanDark.toBrush();

            public static Brush black = Color.Black.toBrush();
        }

        internal class Bio
        {
            public static Color gold = c("bio.gold");
            public static Color goldDark = c("bio.goldDark");
            public static Brush brushGold = gold.toBrush();
            public static Pen penGold4 = gold.toPen(4f);
            public static Pen penGoldDark1 = goldDark.toPen(1f);

            public static Brush brushUnknown = c("bio.unknown").toBrush();
            public static Brush brushHatch = new HatchBrush(HatchStyle.DarkUpwardDiagonal, c("bio.hatch"), Color.Transparent);
        }
    }

    static class DrawingExtensions
    {
        public static SolidBrush toBrush(this Color color)
        {
            return new SolidBrush(color);
        }

        public static Pen toPen(this Color color, float width)
        {
            return new Pen(color, width * GameColors.scaleFactor);
        }

        public static Pen toPen(this Color color, float width, LineCap lineCap)
        {
            return new Pen(color, width * GameColors.scaleFactor)
            {
                StartCap = lineCap,
                EndCap = lineCap,
            };
        }

        public static Pen toPen(this Color color, float width, DashStyle dashStyle)
        {
            return new Pen(color, width * GameColors.scaleFactor)
            {
                DashStyle = dashStyle,
            };
        }

        /// <summary>
        /// Draw a line relative to the first point.
        /// </summary>
        public static void DrawLineR(this Graphics g, Pen pen, float x, float y, float dx, float dy)
        {
            g.DrawLine(pen, x, y, x + dx, y + dy);
        }


        /// <summary>
        /// Adjust graphics transform, calls the lambda then reverses the adjustments.
        /// </summary>
        public static void Adjust(this Graphics g, float rot, Action func)
        {
            DrawingExtensions.Adjust(g, 0, 0, 0, rot, func);
        }

        /// <summary>
        /// Adjust graphics transform, calls the lambda then reverses the adjustments.
        /// </summary>
        public static void Adjust(this Graphics g, PointF pf, float rot, Action func)
        {
            DrawingExtensions.Adjust(g, 0, pf.X, pf.Y, rot, func);
        }
        /// <summary>
        /// Adjust graphics transform, calls the lambda then reverses the adjustments.
        /// </summary>
        public static void Adjust(this Graphics g, float x, float y, float rot, Action func)
        {
            DrawingExtensions.Adjust(g, 0, x, y, rot, func);
        }

        /// <summary>
        /// Adjust graphics transform, calls the lambda then reverses the adjustments.
        /// </summary>
        public static void Adjust(this Graphics g, float rot1, float x, float y, float rot2, Action func)
        {
            g.RotateTransform(+rot1);
            // Y value only is inverted
            g.TranslateTransform(+x, -y);
            g.RotateTransform(+rot2);

            func();

            g.RotateTransform(-rot2);
            g.TranslateTransform(-x, +y);
            g.RotateTransform(-rot1);
        }

        public static Size drawTextRight(this Graphics g, string text, Font font, Color col, int tx, int ty, int w = 0, int h = 0)
        {
            var flags = TextCursor.defaultTextFlags | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.NoFullWidthCharacterBreak; // | TextFormatFlags.ExternalLeading
            if (w == 0)
                flags |= TextFormatFlags.SingleLine;

            // measure size
            var sz = TextRenderer.MeasureText(g, text, font, new Size(w, 0), flags);

            // adjust vertical to fit within height constraint
            var dx = h == 0 ? 0 : Util.centerIn(h, sz.Height);

            // draw text within constraints
            var r = new Rectangle(tx - sz.Width, ty + dx, sz.Width, sz.Height);
            TextRenderer.DrawText(g, text, font, r, col, flags);

            //g.DrawRectangle(Pens.Lime, r);
            return sz;
        }


    }

    /// <summary>
    /// Returns alternating colors each time .next() is called. Ideal for row back colors.
    /// </summary>
    public class AlternatingColors
    {
        private int count = 0;

        public Color even;
        public Color odd;

        public AlternatingColors(Color even, Color odd)
        {
            this.even = even;
            this.odd = odd;
        }

        public Color next()
        {
            count++;
            if (count % 2 == 0)
                return this.even;
            else
                return this.odd;
        }

        public static AlternatingColors gridBackColors
        {
            get => Game.settings.darkTheme
                        ? new AlternatingColors(SystemColors.ControlDarkDark, SystemColors.WindowFrame)
                        : new AlternatingColors(SystemColors.Window, SystemColors.Control);
        }
    }
}
