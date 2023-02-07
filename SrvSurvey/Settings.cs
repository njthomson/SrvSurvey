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
    }

    static class GameColors
    {
        static GameColors()
        {
            // prepare brush for backgrounds
            var bm = new Bitmap(1, 3);
            using (var g = Graphics.FromImage(bm))
            {
                g.DrawLine(GameColors.penBackgroundStripe, 0, 0, 1, 0);
            }
            GameColors.brushBackgroundStripe = new TextureBrush(bm, WrapMode.Tile);

            // ...
            bm = new Bitmap(1, 3);
            using (var g = Graphics.FromImage(bm))
            {
                g.DrawLine(GameColors.penGameOrangeDim1, 0, 0, 1, 0);
                g.DrawLine(GameColors.penGameOrange1, 0, 1, 1, 1);
                g.DrawLine(GameColors.penGameOrangeDim1, 0, 2, 1, 2);
            }
            GameColors.brushOrangeStripe = new TextureBrush(bm, WrapMode.TileFlipX);

            GameColors.penOrangeStripe3 = new Pen(GameColors.brushOrangeStripe, 3);
        }

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);

        public static Pen penBackgroundStripe = new Pen(Color.FromArgb(255, 12, 12, 12));
        public static Pen penGameOrange1 = new Pen(Color.FromArgb(255, 186, 113, 4), 1); //255, 113, 00), 2);
        public static Pen penGameOrange2 = new Pen(Color.FromArgb(255, 186, 113, 4), 2); //255, 113, 00), 2);
        public static Pen penGameOrangeDim1 = new Pen(Color.FromArgb(255, 95, 48, 3), 1);

        public static Pen Lime2 = new Pen(LimeIsh, 2);


        public static Pen penOrangeStripe3;


        public static Brush brushBackgroundStripe;
        public static Brush brushOrangeStripe;
        public static Brush brushGameOrange = new SolidBrush(Color.FromArgb(255, 186, 113, 4)); //  Color.FromArgb(255, 255, 113, 00));


    }
}
