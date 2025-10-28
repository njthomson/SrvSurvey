using SrvSurvey.game;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace SrvSurvey.widgets
{
    static class GameColors
    {
        public static Color Orange = Game.settings.defaultOrange;
        public static Color OrangeDim = Game.settings.defaultOrangeDim;
        public static Color Cyan = Game.settings.defaultCyan;
        public static Color DarkCyan = Game.settings.defaultDarkCyan;

        public static class Defaults
        {
            public static Color Orange = Color.FromArgb(255, 255, 111, 0);
            public static Color OrangeDim = Color.FromArgb(255, 95, 48, 3);
            public static Color Cyan = Color.FromArgb(255, 84, 223, 237);
            public static Color DarkCyan = Color.FromArgb(255, 0, 139, 139);  // DarkCyan
        }

        // IMPORTANT: figure out scaling and fontScaling before anything else

        private static float getScaleFactor()
        {
            if (Program.control == null || Game.settings == null) return 1f;

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
                case 20: // force 90%
                    return 0.9f;
                case 21: // force 80%
                    return 0.8f;
                case 22: // force 75%
                    return 0.75f;
                case 23: // force 70%
                    return 0.7f;
                case 24: // force 60%
                    return 0.6f;
                case 25: // force 50%
                    return 0.5f;
            }
        }
        public static float scaleFactor = GameColors.getScaleFactor();

        private static float getFontScaleFactor()
        {
            if (Program.control == null || Game.settings == null) return 1f;

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
                case 20: // force 90%
                    return 0.9f * osScaleFactor;
                case 21: // force 80%
                    return 0.8f * osScaleFactor;
                case 22: // force 75%
                    return 0.75f * osScaleFactor;
                case 23: // force 70%
                    return 0.7f * osScaleFactor;
                case 24: // force 60%
                    return 0.6f * osScaleFactor;
                case 25: // force 50%
                    return 0.5f * osScaleFactor;
            }
        }
        public static float fontScaleFactor = GameColors.getFontScaleFactor();

        static GameColors()
        {
            loadLocalFonts();

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
                CenterColor = Cyan,
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

            var boxSize = 100;
            var img = new Bitmap(boxSize * 2, boxSize * 2);
            using (var g = Graphics.FromImage(img))
            {
                g.FillRectangle(Brushes.Black, 0, 0, boxSize, boxSize);
                g.FillRectangle(Brushes.Black, boxSize, boxSize, boxSize, boxSize);
            }
            brushGroundChecks = new TextureBrush(img)
            {
                WrapMode = WrapMode.TileFlipXY,
            };
        }

        public static GraphicsPath shiningPath;
        public static PathGradientBrush shiningBrush;
        public static GraphicsPath shiningCmdrPath;
        public static PathGradientBrush shiningCmdrBrush;

        /// <summary>
        /// Large black checkered boxes for map backgrounds
        /// </summary>
        public static TextureBrush brushGroundChecks;

        /// <summary>
        /// Returns a brush for large black checkered boxes for map backgrounds, adjusted by the given scale and location on the current body.
        /// </summary>
        public static Brush adjustGroundChecks(float scale)
        {
            var game = Game.activeGame;
            if (game?.status == null || !game.status.hasLatLong) return Brushes.Transparent;
            if (game.dropPoint == null) game.dropPoint = Status.here.clone();

            brushGroundChecks.ResetTransform();
            brushGroundChecks.RotateTransform(360 - game.status.Heading);

            var offset = (PointF)Util.getOffset(game.status.PlanetRadius, game.dropPoint, 0);
            var x = Math.Floor(-offset.X * scale);
            var y = Math.Floor(offset.Y * scale);
            brushGroundChecks.TranslateTransform((int)x, (int)y);

            return GameColors.brushGroundChecks;
        }

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);

        public static Pen penBackgroundStripe = newPen(Color.FromArgb(255, 12, 12, 12));

        public static Pen penGameOrange1 = newPen(Orange, 1);
        public static Pen penGameOrange2 = newPen(Orange, 2);
        public static Pen penGameOrange3 = newPen(Orange, 3);
        public static Pen penGameOrange4 = newPen(Orange, 4);
        public static Pen penGameOrange8 = newPen(Orange, 8);

        public static Pen penYellow1Dot = newPen(Color.Yellow, 2, DashStyle.Dot);
        public static Pen penYellow2 = newPen(Color.Yellow, 2, LineCap.Triangle, LineCap.Triangle);
        public static Pen penYellow4 = newPen(Color.Yellow, 4, LineCap.Triangle, LineCap.Triangle);
        public static Pen penYellow4Dot = newPen(Color.Yellow, 4, DashStyle.Dot);
        public static Pen penYellow8 = newPen(Color.Yellow, 8, LineCap.Triangle, LineCap.Triangle);

        public static Pen penBlack2 = newPen(Color.Black, 2);
        public static Pen penBlack2Dash = newPen(Color.Black, 2, DashStyle.Dash);
        public static Pen penBlack2Dot = newPen(Color.Black, 2, DashStyle.Dot);
        public static Pen penBlack4Dash = newPen(Color.Black, 4, DashStyle.Dash);

        public static Pen penGameOrangeDim1 = newPen(OrangeDim, 1);
        public static Pen penGameOrangeDim1Dashed = newPen(OrangeDim, 1, DashStyle.Dash);
        public static Pen penGameOrangeDim1Dotted = newPen(OrangeDim, 1, DashStyle.Dot);
        public static Pen penGameOrangeDim1b = newPen(Color.FromArgb(128, OrangeDim), 1);
        public static Pen penGameOrangeDim2 = newPen(OrangeDim, 2);
        public static Pen penGameOrangeDim4 = newPen(OrangeDim, 4, LineCap.Triangle, LineCap.Triangle);

        public static Pen penGreen2 = newPen(Color.Green, 2);
        public static Pen penDarkGreen2 = newPen(Color.DarkGreen, 2);
        public static Pen penLightGreen2 = newPen(Color.LightGreen, 2);

        public static Pen penCyan1 = newPen(Cyan, 1);
        public static Pen penCyan2 = newPen(Cyan, 2);
        public static Pen penCyan1Dotted = newPen(Cyan, 1, DashStyle.Dot);
        public static Pen penCyan2Dotted = newPen(Cyan, 2, DashStyle.Dot);
        public static Pen penCyan4 = newPen(Cyan, 4);
        public static Pen penCyan8 = newPen(Cyan, 8);
        public static Pen penDarkCyan1 = newPen(DarkCyan, 1);
        public static Pen penDarkCyan2 = newPen(DarkCyan, 2);
        public static Pen penDarkCyan4 = newPen(DarkCyan, 4);

        public static Pen penRed1 = newPen(Color.Red, 1);
        public static Pen penRed2 = newPen(Color.Red, 2);
        public static Pen penDarkRed1 = newPen(Color.DarkRed, 1);
        public static Pen penDarkRed2 = newPen(Color.DarkRed, 2);
        public static Pen penDarkRed3 = newPen(Color.DarkRed, 3);
        public static Pen penDarkRed4 = newPen(Color.DarkRed, 4);

        public static Pen penRed2Ish = newPen(Color.FromArgb(128, Color.Red), 2);
        public static Pen penDarkRed2Ish = newPen(Color.FromArgb(128, Color.DarkRed), 2);
        public static Pen penRed2DashedIsh = newPen(Color.FromArgb(128, Color.Red), 2, DashStyle.Dash);
        public static Pen penRed2DashedIshIsh = newPen(Color.FromArgb(64, Color.Red), 2, DashStyle.Dash);

        public static Pen penBlack3Dotted = newPen(Color.Black, 3, DashStyle.Dot);
        public static Pen penUnknownBioSignal = newPen(Color.FromArgb(255, 88, 88, 88), 1);
        public static Brush brushUnknownBioSignal = new SolidBrush(Color.FromArgb(255, 88, 88, 88));

        public static Pen penGameOrange1Dotted = newPen(Orange, 1, DashStyle.Dot);
        public static Pen penGameOrange1Dashed = newPen(Orange, 1, DashStyle.Dash);
        public static Pen penGameOrange2Dotted = newPen(Orange, 2, DashStyle.Dot);
        public static Pen penGameOrange2Dashed = newPen(Orange, 2, DashStyle.Dash);
        public static Pen penGameOrange2DashedIsh = newPen(Color.FromArgb(128, Orange), 2, DashStyle.Dash);

        public static Pen penNearestUnknownSitePOI = newPen(Color.FromArgb(96, DarkCyan), 15, LineCap.RoundAnchor, LineCap.Round);

        public static Pen penLime2 = newPen(LimeIsh, 2);
        public static Pen penLime2Dot = newPen(Color.FromArgb(64, GameColors.LimeIsh), 2, DashStyle.Dash);
        public static Pen penLime4 = newPen(LimeIsh, 4);
        public static Pen penLime4Dot = newPen(Color.FromArgb(128, GameColors.LimeIsh), 4, DashStyle.Dash);
        public static Brush brushLimeIsh = new SolidBrush(GameColors.LimeIsh);

        public static Pen penOrangeStripe3;

        internal class Bio
        {
            public static Color gold = Color.Gold;
            public static Brush brushPrediction = Brushes.DimGray;
            public static Brush brushPredictionHatch = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Color.FromArgb(242, 64, 64, 64), Color.Transparent);
            public static Brush brushGold = new SolidBrush(gold);
            public static Brush brushWhite = Brushes.White;

            // NOTE: Grey intentionally matches Blue

            public static Dictionary<VolColor, Pen> volEdge = new Dictionary<VolColor, Pen>()
            {
                { VolColor.Blue, newPen(Color.FromArgb(96, DarkCyan), 1.9f, DashStyle.Dot) },
                { VolColor.Gold, newPen(Color.FromArgb(96, gold), 1.9f, DashStyle.Dot) },
                { VolColor.White, newPen(Color.FromArgb(96, Color.White), 1.9f, DashStyle.Dot) },
                { VolColor.Orange, newPen(Color.FromArgb(96, Orange), 1.9f, DashStyle.Dot) },
            };

            public static Dictionary<VolColor, PenBrush> volMin = new Dictionary<VolColor, PenBrush>()
            {
                { VolColor.Blue, new PenBrush(newPen(DarkCyan, 1), brushCyan) },
                { VolColor.Gold, new PenBrush(newPen(gold, 1), Brushes.DarkGoldenrod) },
                { VolColor.White, new PenBrush(newPen(Color.Gray, 1), new SolidBrush(Color.FromArgb(255, 244, 244, 244))) },
                { VolColor.Orange, new PenBrush(newPen(OrangeDim, 1), GameColors.brushGameOrange) },
            };

            public static Dictionary<VolColor, PenBrush> volMax = new Dictionary<VolColor, PenBrush>()
            {
                { VolColor.Blue, new PenBrush(newPen(DarkCyan, 1), new SolidBrush(Color.FromArgb(180, DarkCyan))) },
                { VolColor.Gold, new PenBrush(newPen(Color.FromArgb(144, 214, 164, 11), 1), new SolidBrush(Color.FromArgb(144, 184, 134, 11))) },
                { VolColor.White, new PenBrush(newPen(Color.FromArgb(144, Color.White), 1), new SolidBrush(Color.FromArgb(140, 184, 184, 184))) },
                { VolColor.Orange, new PenBrush(newPen(Color.FromArgb(124, GameColors.Orange)), new SolidBrush(Color.FromArgb(140, OrangeDim))) },
            };

            //// gray
            //minFill = new SolidBrush(Color.FromArgb(255, 86, 86, 86));
            //maxFill = new SolidBrush(Color.FromArgb(140, 38, 38, 38));
            //outerEdge = GameColors.newPen(Color.FromArgb(96, 182, 182, 182), 1.9f, DashStyle.Dot);
            //minEdge = GameColors.newPen(Color.FromArgb(255, 44, 44, 44));
            //maxEdge = GameColors.newPen(Color.FromArgb(124, 96, 96, 96));

            //// dim orange?
            //minFill = GameColors.brushGameOrangeDim;
            //maxFill = new SolidBrush(Color.FromArgb(140, GameColors.OrangeDim));
            //outerEdge = GameColors.newPen(Color.FromArgb(96, GameColors.Orange), 1.9f, DashStyle.Dot);
            //minEdge = GameColors.newPen(Color.FromArgb(124, 45, 28, 3), 1);
            //maxEdge = GameColors.penGameOrangeDim1;
        }

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

        public static Brush brushTrackerClose = new SolidBrush(Color.FromArgb(32, DarkCyan));
        public static Pen penTrackerClose = newPen(Color.FromArgb(36, Cyan), 12);

        public static Pen penShipDepartFar = newPen(Color.FromArgb(64, Color.Red), 32, DashStyle.DashDotDot);
        public static Pen penShipDepartNear = newPen(Color.FromArgb(255, Color.Red), 32, DashStyle.DashDotDot);

        public static Pen penMapEditGuideLineGreen = new Pen(Color.Green, 0.5f * scaleFactor) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor, StartCap = LineCap.ArrowAnchor };
        public static Pen penMapEditGuideLineGreenYellow = new Pen(Color.GreenYellow, 0.5f * scaleFactor) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor, StartCap = LineCap.ArrowAnchor };

        public static Brush brushHumanBuilding = new SolidBrush(Color.FromArgb(255, 0, 32, 0));

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
                public static Color color = Color.FromArgb(32, Cyan);
                public static Brush brush = GameColors.brushCyan;
                public static Pen pen = GameColors.penCyan2;
                public static Pen penRadar = new Pen(Color.FromArgb(48, Cyan)) { Width = 16, DashStyle = DashStyle.Solid };
            }
            /// <summary> dark cyan </summary>
            internal static class CloseInactive
            {
                public static Color color = Color.FromArgb(32, DarkCyan);
                public static Brush brush = brushDarkCyan;
                public static Pen pen = newPen(DarkCyan);
                public static Pen penRadar = new Pen(Color.FromArgb(80, DarkCyan)) { Width = 8, DashStyle = DashStyle.Dot };
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
                public static Pen pen = newPen(Color.DarkSlateGray);
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
        public static SolidBrush brushDarkCyan = new SolidBrush(DarkCyan);

        public static Brush brushOnTarget = new HatchBrush(HatchStyle.Percent50, OrangeDim, Color.Transparent);
        public static Brush brushOffTarget = new HatchBrush(HatchStyle.Percent25, OrangeDim, Color.Transparent);

        public static Color red = Color.Red;
        public static Brush brushRed = Brushes.Red;

        public static Font fontScreenshotBannerBig = new Font("Century Gothic", 14F, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontScreenshotBannerSmall = new Font("Century Gothic", 10F, FontStyle.Regular, GraphicsUnit.Point);

        public static Pen penPoiRelicUnconfirmed = newPen(Cyan, 4, DashStyle.Dash); // CornflowerBlue ?
        public static Pen penPoiRelicPresent = newPen(Cyan, 2f);
        public static Pen penPoiRelicMissing = newPen(Color.FromArgb(128, 128, 128, 128));

        public static Pen penPoiPuddleUnconfirmed = newPen(Cyan, 3, DashStyle.Dot); // PeachPuff ?
        public static Pen penPoiPuddlePresent = newPen(Color.Orange, 3, DashStyle.Solid);
        public static Pen penPoiPuddleMissing = newPen(Color.DarkRed, 3, DashStyle.Solid);

        public static Pen penObelisk = new Pen(DarkCyan, 0.5f * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
        public static Pen penObeliskActive = new Pen(Cyan, 0.5f * scaleFactor) { LineJoin = LineJoin.Bevel, StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };

        public static Brush brushShipDismissWarning = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Red, Color.Black);

        public static Color colorPoiMissingDark = Color.FromArgb(128, 64, 64, 64);
        public static Color colorPoiMissingLight = Color.FromArgb(128, 128, 128, 128);
        public static Color relicBlue = Color.FromArgb(255, 66, 44, 255);

        public static Brush brushPoiPresent = new SolidBrush(relicBlue);
        public static Brush brushPoiMissing = new SolidBrush(colorPoiMissingDark);
        public static Pen penPoiMissing = newPen(Color.FromArgb(128, 128, 128, 128));

        public static Brush brushAroundPoiUnknown = new SolidBrush(Color.FromArgb(160, Color.DarkSlateBlue));
        public static Pen penAroundPoiUnknown = newPen(Color.FromArgb(96, Cyan), 1f, DashStyle.Dot);

        /// <summary Guardian map related </summary>
        internal static class Map
        {
            public static PenBrush legend = new PenBrush(
                newPen(Color.DarkGray, 4),
                Brushes.Black);

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
                    { SitePoiStatus.present, new SolidBrush(Cyan) },
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

            public static Pen penComponentUnknown = new Pen(colorUnknown)
            {
                Width = 1 * GameColors.scaleFactor,
                DashStyle = DashStyle.Dash,
                LineJoin = LineJoin.Bevel,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };
            public static Pen penComponentPresent = new Pen(Color.Lime)
            {
                Width = 1 * GameColors.scaleFactor,
                DashStyle = DashStyle.Dash,
                LineJoin = LineJoin.Bevel,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };
            public static Pen penComponentAbsent = new Pen(colorAbsent)
            {
                Width = 1 * GameColors.scaleFactor,
                DashStyle = DashStyle.Dash,
                LineJoin = LineJoin.Bevel,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };
        }

        public static Pen penCollectedMatDot = new Pen(Color.Yellow, 0.2f);

        /// <summary>
        /// For PlotHumanSite
        /// </summary>
        public static class HumanSite
        {
            public static PenBrush powerIcon = new PenBrush(
                newPen(Cyan, 0.5f, LineCap.Triangle, LineCap.Triangle),
                brushCyan);

            public static Pen penDoorEdge = newPen(Color.Navy);

            public static Dictionary<int, PenBrush> siteLevel = new Dictionary<int, PenBrush>
            {
                { 0, new PenBrush(Color.Green, 0.5f, LineCap.Triangle) },
                { 1, new PenBrush(Color.SkyBlue, 0.5f, LineCap.Triangle) },
                { 2, new PenBrush(Color.DarkOrange, 0.5f, LineCap.Triangle) },
                { 3, new PenBrush(Color.Red, 0.5f, LineCap.Triangle) },

                // For processed POIs, like DataTerminals that have already been scanned
                { -1, new PenBrush(Color.Gray, 0.5f, LineCap.Triangle) },
            };

            public static PenBrush landingPad = new PenBrush(
                newPen(Color.SlateGray),
                Brushes.SlateGray);

            public static Pen penOuterLimit = newPen(Color.FromArgb(64, Color.Gray), 8, DashStyle.DashDotDot);
            public static Pen penOuterLimitWarningArrow = penGameOrangeDim4;

            public static Brush brushTextFade = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
        }

        /// <summary>
        /// Colors for buildings at human settlements
        /// </summary>
        public static class Building
        {
            public static Brush HAB = new SolidBrush(Color.FromArgb(255, 0, 32, 0)); // green
            public static Brush CMD = new SolidBrush(Color.FromArgb(255, 48, 0, 0)); // red
            public static Brush POW = new SolidBrush(Color.FromArgb(255, 32, 32, 64)); // purple
            public static Brush EXT = new SolidBrush(Color.FromArgb(255, 0, 0, 72)); // blue
            public static Brush STO = new SolidBrush(Color.FromArgb(255, 50, 30, 20)); // brown
            public static Brush RES = new SolidBrush(Color.FromArgb(255, 50, 20, 10)); // darker brown?
            public static Brush IND = new SolidBrush(Color.FromArgb(255, 60, 40, 10)); // lighter brown
            public static Brush MED = new SolidBrush(Color.FromArgb(255, 32, 32, 32)); // grey
            public static Brush LAB = new SolidBrush(Color.FromArgb(255, 0, 32, 32)); // dark green
        }

        /// <summary>
        /// Colors for PlotJumpInfo
        /// </summary>
        public static class Route
        {
            public static Pen penNext2 = newPen(Cyan, 2, LineCap.Triangle, LineCap.Triangle);
            public static Pen penNext4 = newPen(Cyan, 4, LineCap.Triangle, LineCap.Triangle);
            public static Pen penNeutronAhead = newPen(DarkCyan, 4, LineCap.Triangle, LineCap.Triangle, DashStyle.Dash);
            public static Pen penNeutronBehind = newPen(DarkCyan, 4, LineCap.Triangle, LineCap.Triangle);
            public static Pen penAhead = newPen(OrangeDim, 4, DashStyle.Dot);
            public static Pen penBehind = newPen(OrangeDim, 4, LineCap.Triangle, LineCap.Triangle);
        }

        /// <summary>
        /// Colors used in icons
        /// </summary>
        public static class Icon
        {
            public static Brush lightBlue = new SolidBrush(Color.FromArgb(255, 0, 255, 222));
            public static Brush darkBlue = new SolidBrush(Color.FromArgb(255, 0, 94, 176));
        }

        #region Fonts

        public static void resetFontScale()
        {
            font1 = new Font("Lucida Console", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontSmall = new Font("Lucida Console", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            fontSmallBold = new Font("Lucida Console", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
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

        public static Font font1 = new Font("Lucida Console", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmall = new Font("Lucida Console", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmallBold = new Font("Lucida Console", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);

        public static Font fontSmall2 = new Font("Century Gothic", 9F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontSmall2Bold = new Font("Century Gothic", 9F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font fontSmaller = new Font("Century Gothic", 10F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontMiddle = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontMiddleBold = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font2 = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontBig = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font fontBigBold = new Font("Century Gothic", 22F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font18 = new Font("Century Gothic", 18F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font font16 = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
        public static Font font16b = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font14 = new Font("Century Gothic", 14F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);

        internal static class Fonts
        {
            public static Font wingdings_4B = new Font("Wingdings", 4F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            public static Font wingdings2_8 = new Font("Wingdings 2", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font wingdings2_6 = new Font("Wingdings 2", 6F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font wingdings2_4 = new Font("Wingdings 2", 4F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);

            public static Font console_7 = new Font("Lucida Console", 7F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font console_8 = new Font("Lucida Console", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font console_8B = new Font("Lucida Console", 8F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            public static Font console_16 = new Font("Lucida Console", 16F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);

            public static Font gothic_9 = new Font("Century Gothic", 9F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font gothic_9B = new Font("Century Gothic", 9F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            public static Font gothic_10 = new Font("Century Gothic", 10F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font gothic_10B = new Font("Century Gothic", 10F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            public static Font gothic_12 = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font gothic_12B = new Font("Century Gothic", 12F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            public static Font gothic_14B = new Font("Century Gothic", 14F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);
            public static Font gothic_16B = new Font("Century Gothic", 16F * fontScaleFactor, FontStyle.Bold, GraphicsUnit.Point);

            // TODO: confirm these Pixel sized fonts scale properly with large fonts
            public static Font typewriter_p4 = new Font("Lucida Console", 4F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Pixel);
            public static Font typewriter_p6 = new Font("Lucida Console", 6F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Pixel);

            public static Font segoeEmoji_6 = new Font("Segoe UI Emoji", 6F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            public static Font segoeEmoji_8 = new Font("Segoe UI Emoji", 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            /// <summary> Not scaled </summary>
            public static Font segoeEmoji_16_ns = new Font("Segoe UI Emoji", 16F);
        }

        private static void loadLocalFonts()
        {
            var filepath = Path.Combine(Application.StartupPath, "seguiemj.ttf");
            var ttfExists = File.Exists(filepath);
            Game.log($"GameColors: Loading local fonts, exists: '{filepath}' => {ttfExists}");
            if (!ttfExists) return;

            // Load the font from file + replace references that should use it
            pfc = new PrivateFontCollection();
            pfc.AddFontFile("seguiemj.ttf");

            Fonts.segoeEmoji_6 = new Font(pfc.Families[0], 6F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            Fonts.segoeEmoji_8 = new Font(pfc.Families[0], 8F * fontScaleFactor, FontStyle.Regular, GraphicsUnit.Point);
            Fonts.segoeEmoji_16_ns = new Font(pfc.Families[0], 16F);
            Game.log($"GameColors: Loading local fonts: success!");
        }

        private static PrivateFontCollection? pfc;

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

        public static Pen newPen(Color color, float width = 1, LineCap startCap = LineCap.Flat, LineCap endCap = LineCap.Flat, DashStyle dashStyle = DashStyle.Dot)
        {
            return new Pen(color, width * GameColors.scaleFactor)
            {
                StartCap = startCap,
                EndCap = endCap,
                DashStyle = dashStyle,
            };
        }
    }

    /// <summary>
    /// A related Pen and Brush
    /// </summary>
    internal class PenBrush
    {
        public Pen pen;
        public Brush brush;

        public PenBrush(Pen pen, Brush brush)
        {
            this.pen = pen;
            this.brush = brush;
        }

        public PenBrush(Color color, float penWidth, LineCap endCaps)
        {
            this.pen = GameColors.newPen(color, penWidth, endCaps, endCaps);
            this.brush = new SolidBrush(color);
        }
    }
}