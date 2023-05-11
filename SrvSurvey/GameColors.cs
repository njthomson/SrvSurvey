using System;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
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

            // prepare brush for bio-scan exclusion circles: green cross-hatch
            GameColors.brushExclusionActive = new HatchBrush(HatchStyle.ZigZag, Color.DarkGreen, Color.Transparent);
            GameColors.penExclusionActive = new Pen(Color.FromArgb(96, Color.Green), 20);

            // prepare brush for bio-scan exclusion circles: red cross-hatch
            GameColors.brushExclusionDenied = new HatchBrush(HatchStyle.ZigZag, Color.DarkRed, Color.Transparent);
            GameColors.penExclusionDenied = new Pen(Color.FromArgb(96, Color.Red), 20);

            // prepare brush for bio-scan exclusion circles: blue cross-hatch
            GameColors.brushExclusionAbandoned = new HatchBrush(HatchStyle.Divot, Color.DarkBlue, Color.Transparent);
            GameColors.penExclusionAbandoned = new Pen(Color.FromArgb(96, Color.Blue), 20);

            // prepare brush for bio-scan exclusion circles: dark blue/grey cross-hatch
            GameColors.brushExclusionComplete = new HatchBrush(HatchStyle.Divot, Color.SlateBlue, Color.Transparent);
            GameColors.penExclusionComplete = new Pen(Color.FromArgb(96, Color.DarkSlateBlue), 20);

            // prepare brush for ship location
            GameColors.brushShipLocation = new HatchBrush(HatchStyle.SmallCheckerBoard, Cyan, Color.Transparent);
            GameColors.brushShipFormerLocation = new HatchBrush(HatchStyle.Divot, Cyan, Color.Transparent);
            GameColors.brushSrvLocation = new HatchBrush(HatchStyle.SmallCheckerBoard, Orange, Color.Transparent);
        }

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);
        public static Color Orange = Color.FromArgb(255, 186, 113, 4);
        public static Color OrangeDim = Color.FromArgb(255, 95, 48, 3);
        public static Color Cyan = Color.FromArgb(255, 84, 223, 237);

        public static Pen penBackgroundStripe = new Pen(Color.FromArgb(255, 12, 12, 12));

        public static Pen penGameOrange1 = new Pen(Orange, 1);
        public static Pen penGameOrange2 = new Pen(Orange, 2);
        public static Pen penGameOrange3 = new Pen(Orange, 3);
        public static Pen penGameOrange4 = new Pen(Orange, 4);
        public static Pen penGameOrange8 = new Pen(Orange, 8);

        public static Pen penYellow2 = new Pen(Color.Yellow, 2) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle, };
        public static Pen penYellow4 = new Pen(Color.Yellow, 4) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle, };
        public static Pen penYellow8 = new Pen(Color.Yellow, 8) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle, };

        public static Pen penGameOrangeDim1 = new Pen(OrangeDim, 1);
        public static Pen penGameOrangeDim2 = new Pen(OrangeDim, 2);

        public static Pen penGreen2 = new Pen(Color.Green, 2);
        public static Pen penLightGreen2 = new Pen(Color.LightGreen, 2);

        public static Pen penCyan2 = new Pen(Cyan, 2);
        public static Pen penCyan2Dotted = new Pen(Cyan, 2) { DashStyle = DashStyle.Dot };
        public static Pen penCyan4 = new Pen(Cyan, 4);
        public static Pen penCyan8 = new Pen(Cyan, 8);

        public static Pen penGameOrange2Dotted = new Pen(Orange, 2)
        {
            DashStyle = DashStyle.Dot
        };

        public static Pen Lime2 = new Pen(LimeIsh, 2);

        public static Pen penOrangeStripe3;

        public static Brush brushExclusionActive;
        public static Pen penExclusionActive;
        public static Brush brushExclusionDenied;
        public static Pen penExclusionDenied;
        public static Brush brushExclusionAbandoned;
        public static Pen penExclusionAbandoned;
        public static Brush brushExclusionComplete;
        public static Pen penExclusionComplete;

        public static Brush brushShipLocation;
        public static Brush brushShipFormerLocation;
        public static Brush brushSrvLocation;


        public static Brush brushBackgroundStripe;
        public static Brush brushOrangeStripe;
        public static Brush brushGameOrange = new SolidBrush(Orange); //  Color.FromArgb(255, 255, 113, 00));
        public static Brush brushGameOrangeDim = new SolidBrush(OrangeDim); //  Color.FromArgb(255, 255, 113, 00));

        public static Brush brushCyan = new SolidBrush(Cyan); //  Color.FromArgb(255, 255, 113, 00));
        public static Brush brushOnTarget = new HatchBrush(HatchStyle.Percent50, OrangeDim, Color.Transparent);
        public static Brush brushOffTarget = new HatchBrush(HatchStyle.Percent25, OrangeDim, Color.Transparent);
    }
}
