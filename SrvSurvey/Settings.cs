using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    class Settings
    {
        public float Opacity = 0.5f;

        public Color GameOrange = Color.FromArgb(255, 255, 113, 00); // #FF7100

        public Font font1 = new System.Drawing.Font("Lucida Sans Typewriter", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        public Font font2 = new System.Drawing.Font("Century Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    }

    static class GameColors
    {
        static GameColors()
        {
            // prepare brush for plotter backgrounds: grey/black horizontal stripes
            var bm = new Bitmap(1, 3);
            using (var g = Graphics.FromImage(bm))
            {
                g.DrawLine(GameColors.penBackgroundStripe, 0, 0, 1, 0);
            }
            GameColors.brushBackgroundStripe = new TextureBrush(bm, WrapMode.Tile);

            // prepare brush for plotter top/bottom stripes of bright/dim orange
            bm = new Bitmap(1, 3);
            using (var g = Graphics.FromImage(bm))
            {
                g.DrawLine(GameColors.penGameOrangeDim1, 0, 0, 1, 0);
                g.DrawLine(GameColors.penGameOrange1, 0, 1, 1, 1);
                g.DrawLine(GameColors.penGameOrangeDim1, 0, 2, 1, 2);
            }
            GameColors.brushOrangeStripe = new TextureBrush(bm, WrapMode.TileFlipX);
            GameColors.penOrangeStripe3 = new Pen(GameColors.brushOrangeStripe, 3);

            // prepare brush for bio-scan exclusion circles: red cross-hatch
            bm = new Bitmap(5, 5);
            using (var g = Graphics.FromImage(bm))
            {
                g.DrawLine(Pens.DarkRed, 0, 0, bm.Width, bm.Height);
                g.DrawLine(Pens.DarkRed, bm.Width, 0, 0, bm.Height);
            }
            GameColors.brushExclusionActive = new TextureBrush(bm, WrapMode.Tile);
            GameColors.penExclusionActive = new Pen(Color.FromArgb(96, Color.Red), 20);

            // prepare brush for bio-scan exclusion circles: red cross-hatch
            bm = new Bitmap(5, 5);
            using (var g = Graphics.FromImage(bm))
            {
                g.DrawLine(Pens.DarkGreen, 0, 0, bm.Width, bm.Height);
                g.DrawLine(Pens.DarkGreen, bm.Width, 0, 0, bm.Height);
            }
            GameColors.brushExclusionComplete = new TextureBrush(bm, WrapMode.Tile);
            GameColors.penExclusionComplete = new Pen(Color.FromArgb(96, Color.Green), 20);
        }

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);
        public static Color Orange = Color.FromArgb(255, 186, 113, 4);

        public static Pen penBackgroundStripe = new Pen(Color.FromArgb(255, 12, 12, 12));
        public static Pen penGameOrange1 = new Pen(Orange, 1); //255, 113, 00), 2);
        public static Pen penGameOrange2 = new Pen(Orange, 2); //255, 113, 00), 2);
        public static Pen penGameOrange4 = new Pen(Orange, 8); //255, 113, 00), 2);
        public static Pen penGameOrangeDim1 = new Pen(Color.FromArgb(255, 95, 48, 3), 1);

        public static Pen Lime2 = new Pen(LimeIsh, 2);


        public static Pen penOrangeStripe3;

        public static Brush brushExclusionActive;
        public static Pen penExclusionActive;
        public static Brush brushExclusionComplete;
        public static Pen penExclusionComplete;

        public static Brush brushBackgroundStripe;
        public static Brush brushOrangeStripe;
        public static Brush brushGameOrange = new SolidBrush(Orange); //  Color.FromArgb(255, 255, 113, 00));


    }
}
