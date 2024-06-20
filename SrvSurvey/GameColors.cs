using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    static class GameColors
    {
        // IMPORTANT: figure out scaling and fontScaling before anything else

        private static float getScaleFactor()
        {
            switch (Game.settings.plotterScale)
            {
                default:
                case 0: // match OS scaling
                    return Program.control.DeviceDpi / 96f;
                case 1: // force 100%
                    return 1f;
                case 2: // force 110%
                    return 1.1f;
                case 3: // force 120%
                    return 1.2f;
                case 4: // force 125%
                    return 1.25f;
                case 5: // force 130%
                    return 1.3f;
                case 6: // force 140%
                    return 1.4f;
                case 7: // force 150%
                    return 1.5f;
                case 8: // force 160%
                    return 1.6f;
                case 9: // force 170%
                    return 1.7f;
                case 10: // force 175%
                    return 1.75f;
                case 11: // force 180%
                    return 1.8f;
                case 12: // force 190%
                    return 1.9f;
                case 13: // force 200%
                    return 2f;
                case 14: // force 210%
                    return 2.1f;
                case 15: // force 220%
                    return 2.2f;
                case 16: // force 225%
                    return 2.25f;
                case 17: // force 230%
                    return 2.3f;
                case 18: // force 240%
                    return 2.4f;
                case 19: // force 250%
                    return 2.5f;
            }
        }
        public static float scaleFactor = GameColors.getScaleFactor();

        private static float getFontScaleFactor()
        {
            var osScaleFactor = 96f / Program.control.DeviceDpi;
            switch (Game.settings.plotterScale)
            {
                default:
                case 0: // match OS scaling
                    return 1; // Program.control.DeviceDpi / 96f;
                case 1: // force 100%
                    return 1f * osScaleFactor;
                case 2: // force 110%
                    return 1.1f * osScaleFactor;
                case 3: // force 120%
                    return 1.2f * osScaleFactor;
                case 4: // force 125%
                    return 1.25f * osScaleFactor;
                case 5: // force 130%
                    return 1.3f * osScaleFactor;
                case 6: // force 140%
                    return 1.4f * osScaleFactor;
                case 7: // force 150%
                    return 1.5f * osScaleFactor;
                case 8: // force 160%
                    return 1.6f * osScaleFactor;
                case 9: // force 170%
                    return 1.7f * osScaleFactor;
                case 10: // force 175%
                    return 1.75f * osScaleFactor;
                case 11: // force 180%
                    return 1.8f * osScaleFactor;
                case 12: // force 190%
                    return 1.9f * osScaleFactor;
                case 13: // force 200%
                    return 2f * osScaleFactor;
                case 14: // force 210%
                    return 2.1f * osScaleFactor;
                case 15: // force 220%
                    return 2.2f * osScaleFactor;
                case 16: // force 225%
                    return 2.25f * osScaleFactor;
                case 17: // force 230%
                    return 2.3f * osScaleFactor;
                case 18: // force 240%
                    return 2.4f * osScaleFactor;
                case 19: // force 250%
                    return 2.5f * osScaleFactor;
            }
        }
        public static float fontScaleFactor = GameColors.getFontScaleFactor();

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

            GameColors.penOrangeStripe3 = newPen(GameColors.brushOrangeStripe, 3);

            GameColors.shiningPath = new GraphicsPath();
            shiningPath.AddPie(-14.9f, -14.7f, 30, 30, 240, 90);

            GameColors.shiningBrush = new PathGradientBrush(shiningPath)
            {
                CenterColor = Color.Cyan,
                SurroundColors = new Color[] { Color.Transparent },
                CenterPoint = new PointF(0, 0)
            };

            GameColors.shiningCmdrPath = new GraphicsPath();
            shiningCmdrPath.AddPie(-75, -75, 150, 150, 40, 100);

            GameColors.shiningCmdrBrush = new PathGradientBrush(shiningCmdrPath)
            {
                CenterColor = Color.FromArgb(160, 0, 100, 0),
                SurroundColors = new Color[] { Color.Transparent },
                CenterPoint = new PointF(0, 0)
            };
        }

        public static GraphicsPath shiningPath;
        public static PathGradientBrush shiningBrush;
        public static GraphicsPath shiningCmdrPath;
        public static PathGradientBrush shiningCmdrBrush;

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);
        public static Color Orange = Color.FromArgb(255, 255, 111, 0);
        //Color.FromArgb(255, 240, 109, 29);
        public static Color OrangeDim = Color.FromArgb(255, 95, 48, 3);
        public static Color Cyan = Color.FromArgb(255, 84, 223, 237);
        public static Color DarkCyan = Color.DarkCyan;

        public static Pen penBackgroundStripe = newPen(Color.FromArgb(255, 12, 12, 12));

        public static Pen penGameOrange1 = newPen(Orange, 1);
        public static Pen penGameOrange2 = newPen(Orange, 2);
        public static Pen penGameOrange3 = newPen(Orange, 3);
        public static Pen penGameOrange4 = newPen(Orange, 4);
        public static Pen penGameOrange8 = newPen(Orange, 8);

        public static Pen penYellow1Dot = newPen(Color.Yellow, 2, DashStyle.Dot);
        public static Pen penYellow2 = newPen(Color.Yellow, 2, LineCap.Triangle, LineCap.Triangle);
        public static Pen penYellow4 = newPen(Color.Yellow, 4, LineCap.Triangle, LineCap.Triangle);
        public static Pen penYellow8 = newPen(Color.Yellow, 8, LineCap.Triangle, LineCap.Triangle);

        public static Pen penBlack2Dash = newPen(Color.Black, 2, DashStyle.Dash);
        public static Pen penBlack2Dot = newPen(Color.Black, 2, DashStyle.Dot);
        public static Pen penBlack4Dash = newPen(Color.Black, 4, DashStyle.Dash);

        public static Pen penGameOrangeDim1 = newPen(OrangeDim, 1);
        public static Pen penGameOrangeDim1b = newPen(Color.FromArgb(128, OrangeDim), 1);
        public static Pen penGameOrangeDim2 = newPen(OrangeDim, 2);
        public static Pen penGameOrangeDim4 = newPen(OrangeDim, 4, LineCap.Triangle, LineCap.Triangle);

        public static Pen penGreen2 = newPen(Color.Green, 2);
        public static Pen penDarkGreen2 = newPen(Color.DarkGreen, 2);
        public static Pen penLightGreen2 = newPen(Color.LightGreen, 2);

        public static Pen penCyan1 = newPen(Cyan, 1);
        public static Pen penCyan2 = newPen(Cyan, 2);
        public static Pen penCyan2Dotted = newPen(Cyan, 2, DashStyle.Dot);
        public static Pen penCyan4 = newPen(Cyan, 4);
        public static Pen penCyan8 = newPen(Cyan, 8);
        public static Pen penDarkCyan1 = newPen(Color.DarkCyan, 1);
        public static Pen penDarkCyan4 = newPen(Color.DarkCyan, 4);

        public static Pen penRed2 = newPen(Color.Red, 2);
        public static Pen penDarkRed2 = newPen(Color.DarkRed, 2);
        public static Pen penDarkRed3 = newPen(Color.DarkRed, 3);

        public static Pen penRed2Ish = newPen(Color.FromArgb(128, Color.Red), 2);
        public static Pen penDarkRed2Ish = newPen(Color.FromArgb(128, Color.DarkRed), 2);
        public static Pen penRed2DashedIsh = newPen(Color.FromArgb(128, Color.Red), 2, DashStyle.Dash);

        public static Pen penBlack3Dotted = newPen(Color.Black, 3, DashStyle.Dot);
        public static Pen penUnknownBioSignal = newPen(Color.FromArgb(255, 88, 88, 88), 1);
        public static Brush brushUnknownBioSignal = new SolidBrush(Color.FromArgb(255, 88, 88, 88));

        public static Pen penGameOrange1Dotted = newPen(Orange, 1, DashStyle.Dot);
        public static Pen penGameOrange1Dashed= newPen(Orange, 1, DashStyle.Dash);
        public static Pen penGameOrange2Dotted = newPen(Orange, 2, DashStyle.Dot);
        public static Pen penGameOrange2Dashed= newPen(Orange, 2, DashStyle.Dash);
        public static Pen penGameOrange2DashedIsh = newPen(Color.FromArgb(128, Orange), 2, DashStyle.Dash);

        public static Pen penNearestUnknownSitePOI = newPen(Color.FromArgb(96, Color.DarkCyan), 15, LineCap.RoundAnchor, LineCap.Round);

        public static Pen penLime2 = newPen(LimeIsh, 2);
        public static Pen penLime2Dot = newPen(Color.FromArgb(64, GameColors.LimeIsh), 2, DashStyle.Dash);
        public static Pen penLime4 = newPen(LimeIsh, 4);
        public static Pen penLime4Dot = newPen(Color.FromArgb(128, GameColors.LimeIsh), 4, DashStyle.Dash);

        public static Pen penOrangeStripe3;

        public static Brush brushExclusionActive = new HatchBrush(HatchStyle.ZigZag, Color.DarkGreen, Color.Transparent);
        public static Pen penExclusionActive = newPen(Color.FromArgb(96, Color.Green), 20);
        public static Brush brushExclusionDenied = new HatchBrush(HatchStyle.ZigZag, Color.DarkRed, Color.Transparent);
        public static Pen penExclusionDenied = newPen(Color.FromArgb(96, Color.Red), 20);
        public static Brush brushExclusionAbandoned = new HatchBrush(HatchStyle.Divot, Color.DarkBlue, Color.Transparent);
        public static Pen penExclusionAbandoned = newPen(Color.FromArgb(96, Color.Blue), 20);
        public static Brush brushExclusionComplete = new HatchBrush(HatchStyle.Divot, Color.FromArgb(96, Color.SlateBlue), Color.Transparent);
        public static Pen penExclusionComplete = newPen(Color.FromArgb(32, Color.DarkSlateBlue), 20);

        public static Brush brushTrackInactive = new SolidBrush(Color.FromArgb(32, Color.Gray));
        public static Pen penTrackInactive = newPen(Color.FromArgb(64, Color.SlateGray), 12);

        public static Brush brushTracker = new SolidBrush(Color.FromArgb(24, GameColors.Orange));
        public static Pen penTracker = newPen(Color.FromArgb(48, GameColors.Orange), 12);
        public static Pen penTrackerActive = newPen(Color.FromArgb(32, GameColors.Orange), 16, DashStyle.Solid);
        public static Pen penTrackerInactive = newPen(Color.FromArgb(48, Color.SlateGray), 8, DashStyle.Solid);

        public static Brush brushTrackerClose = new SolidBrush(Color.FromArgb(32, Color.DarkCyan));
        public static Pen penTrackerClose = newPen(Color.FromArgb(36, Color.Cyan), 12);

        public static Pen penShipDepartFar = newPen(Color.FromArgb(64, Color.Red), 32, DashStyle.DashDotDot);
        public static Pen penShipDepartNear = newPen(Color.FromArgb(255, Color.Red), 32, DashStyle.DashDotDot);

        public static Pen penMapEditGuideLineGreen = new Pen(Color.Green, 0.5f * scaleFactor) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor, StartCap = LineCap.ArrowAnchor };
        public static Pen penMapEditGuideLineGreenYellow = new Pen(Color.GreenYellow, 0.5f * scaleFactor) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor, StartCap = LineCap.ArrowAnchor };

        public static Brush brushHumanBuilding = new SolidBrush(Color.FromArgb(48, Color.Green));

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
        public static SolidBrush brushGameOrange = new SolidBrush(Orange); //  Color.FromArgb(255, 255, 113, 00));
        public static Brush brushGameOrangeDim = new SolidBrush(OrangeDim); //  Color.FromArgb(255, 255, 113, 00));

        public static SolidBrush brushCyan = new SolidBrush(Cyan); //  Color.FromArgb(255, 255, 113, 00));
        public static Brush brushDarkCyan = new SolidBrush(DarkCyan);

        public static Brush brushOnTarget = new HatchBrush(HatchStyle.Percent50, OrangeDim, Color.Transparent);
        public static Brush brushOffTarget = new HatchBrush(HatchStyle.Percent25, OrangeDim, Color.Transparent);

        public static Brush brushRed = Brushes.Red;

        public static Font fontScreenshotBannerBig = new Font("Century Gothic", 14F, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontScreenshotBannerSmall = new Font("Century Gothic", 10F, FontStyle.Regular, GraphicsUnit.Point);


        public static Pen penPoiRelicUnconfirmed = newPen(Color.Cyan, 4, DashStyle.Dash); // CornflowerBlue ?
        public static Pen penPoiRelicPresent = newPen(Cyan, 2f);
        public static Pen penPoiRelicMissing = newPen(Color.FromArgb(128, 128, 128, 128));

        public static Pen penPoiPuddleUnconfirmed = newPen(Color.Cyan, 3, DashStyle.Dot); // PeachPuff ?
        public static Pen penPoiPuddlePresent = newPen(Color.Orange, 3, DashStyle.Solid);
        public static Pen penPoiPuddleMissing = newPen(Color.DarkRed, 3, DashStyle.Solid);

        public static Pen penObelisk = new Pen(Color.DarkCyan, 0.5f * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
        public static Pen penObeliskActive = new Pen(Color.Cyan, 0.5f * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };

        public static Brush brushShipDismissWarning = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Red, Color.Black);

        public static Color colorPoiMissingDark = Color.FromArgb(128, 64, 64, 64);
        public static Color colorPoiMissingLight = Color.FromArgb(128, 128, 128, 128);
        public static Color relicBlue = Color.FromArgb(255, 66, 44, 255);

        public static Brush brushPoiPresent = new SolidBrush(relicBlue);
        public static Brush brushPoiMissing = new SolidBrush(colorPoiMissingDark);
        public static Pen penPoiMissing = newPen(Color.FromArgb(128, 128, 128, 128));

        public static Brush brushAroundPoiUnknown = new SolidBrush(Color.FromArgb(160, Color.DarkSlateBlue));
        public static Pen penAroundPoiUnknown = newPen(Color.FromArgb(96, Color.Cyan), 1f, DashStyle.Dot);

        internal static class Map
        {
            internal static class Legend
            {
                public static Pen pen = newPen(Color.DarkGray, 4);
                public static Brush brush = Brushes.LightGray;
            }

            public static Pen penCentralCompass = newPen(Color.FromArgb(128, Color.DarkRed), 4, LineCap.Round, LineCap.RoundAnchor);
            public static Pen penCentralRelicTowerHeading = newPen(Color.FromArgb(128, relicBlue), 4, LineCap.Round, LineCap.RoundAnchor);
            public static Pen penRelicTowerHeading = newPen(Color.FromArgb(32, relicBlue), 10, LineCap.Round, LineCap.Round);

            public static Color colorUnknown = Color.FromArgb(128, Color.LightSlateGray);
            public static Color colorAbsent = Color.FromArgb(128, Color.DarkSlateGray);

            private static Pen penUnknown = newPen(colorUnknown, 3);
            private static Pen penAbsent = newPen(colorPoiMissingLight, 3);
            private static Pen penEmpty = newPen(Color.Yellow, 3);
            public static Dictionary<POIType, Dictionary<SitePoiStatus, Pen>> pens = new Dictionary<POIType, Dictionary<SitePoiStatus, Pen>>
            {
                { POIType.relic, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, new Pen(Color.CadetBlue, 3 * scaleFactor) { DashStyle = DashStyle.Solid, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle, } },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },

                { POIType.orb, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, newPen(Color.FromArgb(255, 147, 58, 0) /* orange */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.casket, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, newPen(Color.FromArgb(255, 17, 87, 38) /* green */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.tablet, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, newPen(Color.FromArgb(255, 33, 135, 160) /* blue */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.totem, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, newPen(Color.FromArgb(255, 29, 34, 105) /* purple-ish */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.urn, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, newPen(Color.FromArgb(255, 84, 37, 84) /* pink-ish */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },
                { POIType.unknown, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown, penUnknown },
                    { SitePoiStatus.present, newPen(Color.FromArgb(255, 255, 0, 0) /* red */, 3) },
                    { SitePoiStatus.absent, penAbsent },
                    { SitePoiStatus.empty, penEmpty } } },


                { POIType.obelisk, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown,  new Pen(Cyan, 2 * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.present, new Pen(Color.DodgerBlue, 2 * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.absent, new Pen(colorPoiMissingLight, 2 * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.empty, penEmpty } } },

                { POIType.pylon, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown,  new Pen(Cyan, 2 * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.present, new Pen(Color.DodgerBlue, 2 * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.absent, new Pen(colorPoiMissingLight, 2 * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.empty, penEmpty } } },

                { POIType.component, new Dictionary<SitePoiStatus, Pen> {
                    { SitePoiStatus.unknown,  new Pen(Cyan, 1 * scaleFactor) { DashStyle = DashStyle.Dash, LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.present, new Pen(Color.Lime, 1 * scaleFactor) { DashStyle = DashStyle.Dash, LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.absent, new Pen(colorPoiMissingLight, 1 * scaleFactor) { DashStyle = DashStyle.Dash, LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle } },
                    { SitePoiStatus.empty, penEmpty } } },
        };

            private static Brush brushUnknown = new SolidBrush(Color.DarkSlateGray);
            private static Brush brushAbsent = new SolidBrush(colorPoiMissingDark);
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

        public static void resetFontScale()
        {
            font1 = new Font("Lucida Sans Typewriter", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontSmall = new Font("Lucida Sans Typewriter", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontSmallBold = new Font("Lucida Sans Typewriter", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            fontSmall2 = new Font("Century Gothic", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontSmaller = new Font("Century Gothic", 10F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontMiddle = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontMiddleBold = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            font2 = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontBig = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontBigBold = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            font18 = new Font("Century Gothic", 18F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            font14 = new Font("Century Gothic", 14F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        }

        public static Font font1 = new Font("Lucida Sans Typewriter", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmall = new Font("Lucida Sans Typewriter", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmallBold = new Font("Lucida Sans Typewriter", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontSmall2 = new Font("Century Gothic", 9F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmall2Bold = new Font("Century Gothic", 9F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontSmaller = new Font("Century Gothic", 10F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontMiddle = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontMiddleBold = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font2 = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontBig = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontBigBold = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font18 = new Font("Century Gothic", 18F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font font14 = new Font("Century Gothic", 14F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);

        public static Font fontWingDings = new Font("Wingdings", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontWingDings2 = new Font("Wingdings 2", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmallFonts = new Font("Small Fonts", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);

        #endregion

        public static Pen newPen(Color color)
        {
            return new Pen(color, 1 * GameColors.scaleFactor);
        }

        public static Pen newPen(Color color, float width = 1)
        {
            return new Pen(color, width * GameColors.scaleFactor);
        }

        public static Pen newPen(Brush brush, float width = 1)
        {
            return new Pen(brush, width * GameColors.scaleFactor);
        }

        public static Pen newPen(Color color, float width = 1, DashStyle dashStyle = DashStyle.Dot)
        {
            return new Pen(color, width * GameColors.scaleFactor)
            {
                DashStyle = dashStyle,
            };
        }

        public static Pen newPen(Color color, float width = 1, LineCap startCap = LineCap.Flat, LineCap endCap = LineCap.Flat)
        {
            return new Pen(color, width * GameColors.scaleFactor)
            {
                StartCap = startCap,
                EndCap = endCap,
            };
        }
    }
}
