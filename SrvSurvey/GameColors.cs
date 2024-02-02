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

            GameColors.shiningPath = new GraphicsPath();
            shiningPath.AddPie(-14.9f, -14.7f, 30, 30, 240, 90);

            GameColors.shiningBrush = new PathGradientBrush(shiningPath)
            {
                CenterColor = Color.Cyan,
                SurroundColors = new Color[] { Color.Transparent },
                CenterPoint = new PointF(0, 0)
            };

        }

        public static GraphicsPath shiningPath;
        public static PathGradientBrush shiningBrush;

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);
        //public static Color Orange = Color.FromArgb(255, 172, 81, 1); // ?? Color.FromArgb(255, 186, 113, 4);
        public static Color Orange = Color.FromArgb(255, 186, 113, 4);
        public static Color OrangeDim = Color.FromArgb(255, 95, 48, 3);
        public static Color Cyan = Color.FromArgb(255, 84, 223, 237);
        public static Color DarkCyan = Color.DarkCyan;

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
        public static Pen penDarkGreen2 = new Pen(Color.DarkGreen, 2);
        public static Pen penLightGreen2 = new Pen(Color.LightGreen, 2);

        public static Pen penCyan2 = new Pen(Cyan, 2);
        public static Pen penCyan2Dotted = new Pen(Cyan, 2) { DashStyle = DashStyle.Dot };
        public static Pen penCyan4 = new Pen(Cyan, 4);
        public static Pen penCyan8 = new Pen(Cyan, 8);
        public static Pen penDarkCyan4 = new Pen(Color.DarkCyan, 4);

        public static Pen penRed2 = new Pen(Color.Red, 2);
        public static Pen penDarkRed2 = new Pen(Color.DarkRed, 2);
        public static Pen penDarkRed3 = new Pen(Color.DarkRed, 3);

        public static Pen penBlack3Dotted = new Pen(Color.Black, 3) { DashStyle = DashStyle.Dot };

        public static Pen penGameOrange2Dotted = new Pen(Orange, 2)
        {
            DashStyle = DashStyle.Dot
        };

        public static Pen penNearestUnknownSitePOI = new Pen(Color.FromArgb(96, Color.DarkCyan), 15)
        {
            EndCap = LineCap.Round,
            StartCap = LineCap.RoundAnchor,
        };

        public static Pen penLime2 = new Pen(LimeIsh, 2);
        public static Pen penLime2Dot = new Pen(Color.FromArgb(64, GameColors.LimeIsh), 2) { DashStyle = DashStyle.Dash };
        public static Pen penLime4 = new Pen(LimeIsh, 4);
        public static Pen penLime4Dot = new Pen(Color.FromArgb(128, GameColors.LimeIsh), 4) { DashStyle = DashStyle.Dash };

        public static Pen penOrangeStripe3;

        public static Brush brushExclusionActive = new HatchBrush(HatchStyle.ZigZag, Color.DarkGreen, Color.Transparent);
        public static Pen penExclusionActive = new Pen(Color.FromArgb(96, Color.Green), 20);
        public static Brush brushExclusionDenied = new HatchBrush(HatchStyle.ZigZag, Color.DarkRed, Color.Transparent);
        public static Pen penExclusionDenied = new Pen(Color.FromArgb(96, Color.Red), 20);
        public static Brush brushExclusionAbandoned = new HatchBrush(HatchStyle.Divot, Color.DarkBlue, Color.Transparent);
        public static Pen penExclusionAbandoned = new Pen(Color.FromArgb(96, Color.Blue), 20);
        public static Brush brushExclusionComplete = new HatchBrush(HatchStyle.Divot, Color.FromArgb(96, Color.SlateBlue), Color.Transparent);
        public static Pen penExclusionComplete = new Pen(Color.FromArgb(32, Color.DarkSlateBlue), 20);

        public static Brush brushTrackInactive = new SolidBrush(Color.FromArgb(32, Color.Gray));
        public static Pen penTrackInactive = new Pen(Color.FromArgb(64, Color.SlateGray)) { Width = 12 };

        public static Brush brushTracker = new SolidBrush(Color.FromArgb(24, GameColors.Orange));
        public static Pen penTracker = new Pen(Color.FromArgb(48, GameColors.Orange)) { Width = 12 };

        public static Brush brushTrackerClose = new SolidBrush(Color.FromArgb(32, Color.DarkCyan));
        public static Pen penTrackerClose = new Pen(Color.FromArgb(36, Color.Cyan)) { Width = 12 };

        internal static class PriorScans
        {
            /// <summary> orange </summary>
            internal static class Active
            {
                public static Color color = Color.FromArgb(32, GameColors.Orange);
                public static Brush brush = GameColors.brushGameOrange;
                public static Pen pen = GameColors.penGameOrange2;
                public static Pen penRadar = new Pen(Color.FromArgb(32, GameColors.Orange)) { Width = 16, DashStyle = DashStyle.Solid };
            }
            /// <summary> orange dim or gray </summary>
            internal static class Inactive
            {
                public static Color color = Color.FromArgb(32, Color.Gray); // GameColors.OrangeDim);
                public static Brush brush = GameColors.brushGameOrangeDim;
                public static Pen pen = GameColors.penGameOrangeDim2;
                public static Pen penRadar = new Pen(Color.FromArgb(32, GameColors.OrangeDim)) { Width = 16, DashStyle = DashStyle.Solid };
            }
            /// <summary> cyan </summary>
            internal static class CloseActive
            {
                public static Color color = Color.FromArgb(32, GameColors.Cyan);
                public static Brush brush = GameColors.brushCyan;
                public static Pen pen = GameColors.penCyan2;
                public static Pen penRadar = new Pen(Color.FromArgb(48, GameColors.Cyan)) { Width = 16, DashStyle = DashStyle.Solid };
            }
            /// <summary> dark cyan </summary>
            internal static class CloseInactive
            {
                public static Color color = Color.FromArgb(32, GameColors.DarkCyan);
                public static Brush brush = Brushes.DarkCyan;
                public static Pen pen = Pens.DarkCyan;
                public static Pen penRadar = new Pen(Color.FromArgb(80, GameColors.DarkCyan)) { Width = 8, DashStyle = DashStyle.Dot };
            }
            /// <summary> orange dim </summary>
            internal static class FarAway
            {
                public static Brush brush = GameColors.brushGameOrangeDim;
                public static Pen pen = GameColors.penGameOrangeDim2;
            }
            /// <summary> dark slate gray </summary>
            internal static class Analyzed
            {
                public static Brush brush = Brushes.DarkSlateGray;
                public static Pen pen = Pens.DarkSlateGray;
            }
        }

        public static Brush brushShipLocation = new HatchBrush(HatchStyle.SmallCheckerBoard, Cyan, Color.Transparent);
        public static Brush brushShipFormerLocation = new HatchBrush(HatchStyle.Divot, Cyan, Color.Transparent);
        public static Brush brushSrvLocation = new HatchBrush(HatchStyle.SmallCheckerBoard, Color.FromArgb(128, GameColors.Orange), Color.Transparent);

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

        public static Brush brushShipDismissWarning = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Red, Color.Black);

        internal static class Map
        {
            internal static class Legend
            {
                public static Pen pen = new Pen(Color.DarkGray, 4);
                public static Brush brush = Brushes.LightGray;
            }

            public static Pen penCentralCompass = new Pen(Color.FromArgb(128, Color.DarkRed), 4) { EndCap = LineCap.RoundAnchor, StartCap = LineCap.Round };
            public static Pen penCentralRelicTowerHeading = new Pen(Color.FromArgb(128, Color.DarkRed), 4) { EndCap = LineCap.RoundAnchor, StartCap = LineCap.Round };
            public static Pen penRelicTowerHeading = new Pen(Color.FromArgb(32, Color.Blue), 10) { EndCap = LineCap.Round, StartCap = LineCap.Round };

            public static Color colorUnknown = Color.FromArgb(128, Color.LightSlateGray);
            public static Color colorAbsent = Color.FromArgb(128, Color.DarkSlateGray);
            private static Pen penUnknown = new Pen(colorUnknown, 3);
            private static Pen penAbsent = new Pen(colorAbsent, 3); // new Pen(Color.DarkRed, 3);
            private static Pen penEmpty = new Pen(Color.Yellow, 3);
            public static Dictionary<POIType, Dictionary<SitePoiStatus, Pen>> pens = new Dictionary<POIType, Dictionary<SitePoiStatus, Pen>>
            {
                { POIType.relic, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.CadetBlue, 3) { DashStyle = DashStyle.Solid, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle, } },
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
                { POIType.unknown, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.FromArgb(255, 255, 0, 0) /* red */, 3) },
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
                { POIType.unknown, new Dictionary<SitePoiStatus, Brush> {
                    { SitePoiStatus.unknown, brushUnknown },
                    { SitePoiStatus.present, new SolidBrush(Color.FromArgb(255, 100, 0, 0) /* red */) },
                    { SitePoiStatus.absent, brushAbsent },
                    { SitePoiStatus.empty, brushEmpty } } },
            };
        }

        #region Fonts

        public static float fontScaleFactor = 96f / Program.control.CreateGraphics().DpiX;

        public static Font font1 = new Font("Lucida Sans Typewriter", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmall = new Font("Lucida Sans Typewriter", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmallBold = new Font("Lucida Sans Typewriter", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontSmall2 = new Font("Century Gothic", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmaller = new Font("Century Gothic", 10F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontMiddle = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontMiddleBold = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font2 = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontBig = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontBigBold = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font18 = new Font("Century Gothic", 18F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font font14 = new Font("Century Gothic", 14F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);

        #endregion

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
