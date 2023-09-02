using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;

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

        public static Pen penLime2 = new Pen(LimeIsh, 2);
        public static Pen penLime4 = new Pen(LimeIsh, 4);

        public static Pen penOrangeStripe3;

        public static Brush brushExclusionActive;
        public static Pen penExclusionActive;
        public static Brush brushExclusionDenied;
        public static Pen penExclusionDenied;
        public static Brush brushExclusionAbandoned;
        public static Pen penExclusionAbandoned;
        public static Brush brushExclusionComplete;
        public static Pen penExclusionComplete;

        public static Brush brushTrackInactive = new SolidBrush(Color.FromArgb(32, Color.Gray));
        public static Pen penTrackInactive = new Pen(Color.FromArgb(64, Color.SlateGray)) { Width = 12 };

        public static Brush brushTracker = new SolidBrush(Color.FromArgb(24, GameColors.Orange));
        public static Pen penTracker = new Pen(Color.FromArgb(48, GameColors.Orange)) { Width = 12 };

        public static Brush brushTrackerClose = new SolidBrush(Color.FromArgb(32, Color.DarkCyan));
        public static Pen penTrackerClose = new Pen(Color.FromArgb(36, Color.Cyan)) { Width = 12 };

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

        public static Font fontScreenshotBannerBig = new Font("Century Gothic", 14F, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontScreenshotBannerSmall = new Font("Century Gothic", 10F, FontStyle.Regular, GraphicsUnit.Point);


        public static Pen penPoiRelicUnconfirmed = new Pen(Color.Cyan, 4) { DashStyle = DashStyle.Dash, }; // CornflowerBlue ?
        public static Pen penPoiRelicPresent = new Pen(Color.Orange, 4) { DashStyle = DashStyle.Dash, };
        public static Pen penPoiRelicMissing = new Pen(Color.DarkRed, 4) { DashStyle = DashStyle.Dash, };

        public static Pen penPoiPuddleUnconfirmed = new Pen(Color.Cyan, 3) { DashStyle = DashStyle.Dot, }; // PeachPuff ?
        public static Pen penPoiPuddlePresent = new Pen(Color.Orange, 3) { DashStyle = DashStyle.Solid, };
        public static Pen penPoiPuddleMissing = new Pen(Color.DarkRed, 3) { DashStyle = DashStyle.Solid, };

        internal static class Map
        {
            internal static class Legend
            {
                public static Pen pen = new Pen(Color.DarkGray, 4);
                public static Brush brush = Brushes.LightGray;
            }

            private static Pen penUnknown = new Pen(Color.LightSlateGray, 3);
            private static Pen penAbsent = new Pen(Color.DarkSlateGray, 3); // new Pen(Color.DarkRed, 3);
            private static Pen penEmpty = new Pen(Color.Yellow, 3);
            public static Dictionary<POIType, Dictionary<SitePoiStatus, Pen>> pens = new Dictionary<POIType, Dictionary<SitePoiStatus, Pen>>
            {
                { POIType.relic, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.CadetBlue, 4) { DashStyle = DashStyle.Solid, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle, } },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },

                { POIType.orb, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.FromArgb(255, 147, 58, 0) /* orange */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.casket, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.FromArgb(255, 17, 87, 38) /* green */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.tablet, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.FromArgb(255, 33, 135, 160) /* blue */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.totem, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.FromArgb(255, 29, 34, 105) /* purple-ish */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.urn, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.FromArgb(255, 84, 37, 84) /* pink-ish */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
            };

            private static Brush brushUnknown = new SolidBrush(Color.DarkSlateGray);
            private static Brush brushAbsent = new SolidBrush(Color.Black);
            private static Brush brushEmpty = new SolidBrush(Color.Gold);
            public static Dictionary<POIType, Dictionary<SitePoiStatus, Brush>> brushes = new Dictionary<POIType, Dictionary<SitePoiStatus, Brush>>
            {
                { POIType.relic, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.Cyan) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },

                { POIType.orb, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.FromArgb(255, 255, 127, 39) /* orange */) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },
                { POIType.casket, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.FromArgb(255, 34, 177, 76) /* green */) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },
                { POIType.tablet, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.FromArgb(255, 153, 217, 234) /* blue */) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },
                { POIType.totem, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.FromArgb(255, 63, 72, 204) /* purple-ish */) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },
                { POIType.urn, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.FromArgb(255, 163, 73, 164) /* pink-ish */) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },
            };
        }

        internal static class Theme
        {
            /*
            ForeColor
            Window
            Control
             
             */

        }
    }
}
