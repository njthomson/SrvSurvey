using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    public enum Mode
    {
        siteType,   // ask about site type
        heading,    // ask about site heading
        map,        // show site map
        origin,     // show alignment to site origin
    }

    internal partial class PlotGuardians : PlotBase
    {
        private SiteTemplate? template;
        private Image? siteMap;
        private Image? trails;
        private Image? underlay;
        public Image? headingGuidance;

        /// <summary> Site map width </summary>
        private float ux;
        /// <summary> Site map height </summary>
        private float uy;

        private Mode mode;
        private PointF siteTouchdownOffset;
        private PointF commanderOffset;

        private GuardianSiteData siteData { get => game.nearBody?.siteData; }

        private PlotGuardians() : base()
        {
            InitializeComponent();

            //this.Width = 500;
            //this.Height = 500;

            SiteTemplate.Import();

            this.scale = 0.3f;

            this.nextMode();
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatLeftMiddle(this);
        }

        private void PlotGuardians_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect());
        }

        protected override void initialize()
        {
            base.initialize();

            this.nextMode();
        }

        private void PlotGuardians_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Invalidate();
        }

        private void PlotGuardians_MouseClick(object sender, MouseEventArgs e)
        {
            this.Invalidate();
            Elite.setFocusED();
        }

        private void nextMode()
        {
            /*************************************************************
             * Sequence is:
             * A) Site type
             * B) Site heading
             * C) Show the map
             */

            if (siteData.type == GuardianSiteData.SiteType.unknown)
            {
                // we need to know the site type before anything else
                this.mode = Mode.siteType;
            }
            else if (siteData.siteHeading == -1 || siteData.siteHeading == 0)
            {
                // then we need to know the site heading
                this.mode = Mode.heading;
            }
            else
            {
                // we can show the map after that
                this.mode = Mode.map;
                this.loadSiteTemplate();
            }

            Game.log($"Guardian site: nextMode: {this.mode}");
            this.Invalidate();

            // show or hide the heading vertical stripe helper
            if (this.mode == Mode.heading)
                Program.showPlotter<PlotVertialStripe>();
            else
                Program.closePlotter(nameof(PlotVertialStripe));
        }

        protected override void onJournalEntry(SendText entry)
        {
            base.onJournalEntry(entry);
            var msg = entry.Message.ToLowerInvariant();

            if (siteData == null)
            {
                Game.log($"Why no game.nearBody?.siteData?");
                return;
            }


            // switch to/from offset/screenshot mode
            if (msg == ".site offset" && this.mode == Mode.map)
            {
                Game.log($"Changing site origin mode");
                this.mode = Mode.origin;
                this.Invalidate();
                return;
            }
            if (msg == ".site map" && this.mode == Mode.origin)
            {
                Game.log($"Changing site map mode");
                this.mode = Mode.map;
                this.Invalidate();
                return;
            }

            // try parsing the text as site type?
            GuardianSiteData.SiteType parsedType;
            if (siteData.type == GuardianSiteData.SiteType.unknown && Enum.TryParse<GuardianSiteData.SiteType>(msg, out parsedType))
            {
                Game.log($"Changing site type from '{siteData.type}' to '{parsedType}'");
                siteData.type = parsedType;
                siteData.Save();

                this.loadSiteTemplate();
                this.nextMode();
                return;
            }

            // try parsing the site heading
            var changeHeading = false;
            int newHeading = -1;
            if (this.mode == Mode.heading)
                changeHeading = int.TryParse(msg, out newHeading);
            if ((this.mode == Mode.heading && msg == "heading") || msg == "/set heading")
            {
                if (this.mode != Mode.heading)
                {
                    // move into heading mode
                    this.mode = Mode.heading;
                    this.Invalidate();
                    Program.showPlotter<PlotVertialStripe>();
                    return;
                }
                // use current ship heading
                newHeading = game.status.Heading;
                changeHeading = true;
            }
            if (msg.StartsWith("/set heading "))
                changeHeading = int.TryParse(msg.Substring(14), out newHeading);

            if (changeHeading)
            {
                Game.log($"Changing site heading from '{siteData.siteHeading}' to '{newHeading}'");
                siteData.siteHeading = (int)newHeading;
                siteData.Save();

                this.nextMode();
                return;
            }

            if (this.template != null && entry.Message.StartsWith(".s"))
            {
                // temporary
                float heading;
                if (float.TryParse(entry.Message.Substring(2), out heading))
                {
                    this.template.scaleFactor = heading;
                }
            }

            if (this.template != null && entry.Message.StartsWith("sf"))
            {
                // temporary
                float newScale;
                if (float.TryParse(entry.Message.Substring(3), out newScale))
                {
                    Game.log($"scaleFactor: {this.template.scaleFactor} => {newScale}");
                    this.template.scaleFactor = newScale;
                    this.Status_StatusChanged();
                }
            }

            this.Invalidate();
        }

        private void loadSiteTemplate()
        {
            if (siteData.type == GuardianSiteData.SiteType.unknown) return;

            this.template = SiteTemplate.sites[siteData.type];

            //this.siteMap = Bitmap.FromFile(Path.Combine("images", this.template.backgroundImage));
            this.siteMap = Bitmap.FromFile(Path.Combine(this.template.backgroundImage));
            this.trails = new Bitmap(this.siteMap.Width, this.siteMap.Height);

            var filepath = Path.Combine("images", $"gamma-heading-guide.png");// $"{siteData.type}-heading-guide.png");
            if (File.Exists(filepath))
                using (var img = Bitmap.FromFile(filepath))
                    this.headingGuidance = new Bitmap(img);

            // Temporary until trail tracking works
            using (var gg = Graphics.FromImage(this.trails))
            {
                //gg.Clear(Color.Blue);
                //gg.FillRectangle(Brushes.Blue, -400, -400, 800, 800);
                gg.DrawRectangle(GameColors.penCyan8, 0, 0, this.trails.Width, this.trails.Height);
                gg.DrawLine(Pens.Blue, 0, 0, trails.Width, trails.Height);
                gg.DrawLine(Pens.Blue, trails.Width, 0, 0, trails.Height);
                //gg.FillRectangle(Brushes.Red, (this.trails.Width / 2), (this.trails.Height / 2), 40, 40);
            }

            // prepare underlay
            this.underlay = new Bitmap(this.siteMap.Width * 2, this.siteMap.Height * 2);
            this.ux = this.siteMap.Width;
            this.uy = this.siteMap.Height;

            //// prepare other stuff
            //var offset = game.nearBody!.guardianSiteLocation! - touchdownLocation.Target;
            //Game.log($"Touchdown offset: {offset}");
            //this.siteTouchdownOffset = new PointF(
            //    (float)(offset.Long * template.scaleFactor),
            //    (float)(offset.Lat * -template.scaleFactor));
            //Game.log(siteTouchdownOffset);
            //Game.log($"siteTouchdownOffset: {siteTouchdownOffset}");

            this.Status_StatusChanged();
        }

        protected override void Status_StatusChanged()
        {
            base.Status_StatusChanged();

            // prepare other stuff
            if (template != null && siteData != null)
            {

                //var offset = game.nearBody!.guardianSiteLocation! - Status.here;
                //this.commanderOffset = new PointF(
                //    (float)(offset.Long * template.scaleFactor),
                //    (float)(offset.Lat * -template.scaleFactor));
                //Game.log($"commanderOffset old: {commanderOffset}");
                var td = new TrackingDelta(game.nearBody.radius, siteData.location);
                var ss = 1f;
                this.commanderOffset = new PointF(
                    (float)td.dx * ss,
                    -(float)td.dy * ss);
                Game.log($"commanderOffset: {commanderOffset}");


                /* TODO: get trail tacking working
                using (var gg = Graphics.FromImage(this.trails!))
                {
                    gg.ResetTransform();
                    //gg.FillRectangle(Brushes.Yellow, -10, -10, 20, 20);

                    // shift by underlay size
                    //gg.TranslateTransform(ux, uy);
                    //// rotate by site heading only
                    //gg.RotateTransform(+this.siteHeading);
                    //gg.ScaleTransform(this.template!.scaleFactor, this.template!.scaleFactor);


                    // shift by underlay size and rotate by site heading
                    //gg.TranslateTransform(this.trails!.Width / 2, this.trails.Height / 2);
                    gg.TranslateTransform(this.template.imageOffset.X, this.template.imageOffset.Y);
                    gg.RotateTransform(-this.siteHeading);

                    // draw the site bitmap and trails(?)
                    const float rSize = 5;
                    float x = -commanderOffset.X;
                    float y = -commanderOffset.Y;

                    var rect = new RectangleF(
                        x, y, //x - rSize, y - rSize,
                        rSize * 2, rSize * 2
                    );
                    Game.log($"smudge: {rect}");
                    gg.FillEllipse(Brushes.Navy, rect);
                }
                // */

                /*
                using (var g = Graphics.FromImage(this.plotSmudge))
                {
                    g.TranslateTransform(this.siteTemplate.imageOffset.X, this.siteTemplate.imageOffset.Y);

                    float rotation = (float)numSiteHeading.Value;
                    rotation = rotation % 360;
                    g.RotateTransform(-rotation);

                    PointF dSrv = (PointF)(this.pointSrv - this.survey.pointSettlement);
                    g.FillEllipse(Stationary.TireTracks,
                        dSrv.X - 5, dSrv.Y - 5, //  + srvFix.Y, 
                        10, 10);
                    //g.DrawLine(Pens.Pink, dSrv.X, dSrv.Y, 0, 0);

                }
                */
            }

            this.Invalidate();
        }

        private void PlotGuardians_Paint(object sender, PaintEventArgs e)
        {
            this.g = e.Graphics;
            switch (this.mode)
            {
                case Mode.siteType:
                    this.drawSiteTypeHelper();
                    return;

                case Mode.heading:
                    this.drawSiteHeadingHelper();
                    return;

                case Mode.origin:
                    this.drawTrackOrigin();
                    return;

                case Mode.map:
                    this.drawSiteMap();
                    return;
            }
        }

        private void drawTrackOrigin()
        {
            g.ResetTransform();
            this.clipToMiddle(4, 26, 4, 24);
            g.TranslateTransform(mid.Width, mid.Height);
            g.RotateTransform(360 - siteData.siteHeading);
            var sI = 10;
            var sO = 20;
            var vr = 5;

            // calculate deltas from site origin to ship
            var td = new TrackingDelta(game.nearBody!.radius, siteData.location!);
            var x = -(float)td.dx;
            var y = (float)td.dy;

            // adjust scale depending on distance from target
            var sf = 1f;
            if (td.distance < 40)
                sf = 3f;
            else if (td.distance < 70)
                sf = 2f;
            else if (td.distance > 170)
                sf = 0.5f;
            g.ScaleTransform(sf, sf);

            var siteBrush = GameColors.brushOffTarget; // brushOffTarget;
            var sitePenI = GameColors.penGameOrangeDim2;
            var sitePenO = GameColors.penGameOrangeDim2;
            var shipPen = GameColors.penGreen2;
            if (td.distance < 10)
            {
                //siteBrush = GameColors.brushOnTarget;
                sitePenI = GameColors.penGameOrange2;
                shipPen = GameColors.penCyan2;
            }
            if (td.distance < 20)
            {
                siteBrush = new HatchBrush(HatchStyle.Percent50, GameColors.OrangeDim, Color.Transparent); // GameColors.brushOnTarget;
                sitePenO = GameColors.penGameOrange2;
            }

            // draw origin marker
            g.FillEllipse(siteBrush, -sO, -sO, +sO * 2, +sO * 2);
            g.DrawEllipse(sitePenO, -sO, -sO, +sO * 2, +sO * 2);
            g.DrawEllipse(sitePenI, -sI, -sI, +sI + sI, +sI + sI);

            // draw moving ship circle marker, showing ship heading
            var rect = new RectangleF(
                    x - vr, y - vr,
                    +vr * 2, +vr * 2);
            g.DrawEllipse(shipPen, rect);
            var dHeading = new Angle(game.status.Heading);
            //var dHeading = new Angle(game.status.Heading) - siteData.siteHeading;
            //var dHeading = new Angle(siteData.siteHeading) - game.status.Heading;
            var dx = (float)Math.Sin(dHeading.radians) * 10F;
            var dy = (float)Math.Cos(dHeading.radians) * 10F;
            g.DrawLine(shipPen, x, y, x + dx, y - dy);
            this.drawCompassLines(siteData.siteHeading);

            Point[] pps = { new Point((int)x, (int)y) };
            g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pps);

            g.ResetTransform();
            this.clipToMiddle(4, 26, 4, 24);
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(sf, sf);

            var pp = pps[0]; // new Point((int)pps[0].X, (int)pps[0].Y); // new Point((int)sz.Width, -(int)sz.Height);
            pp.Offset(-mid.Width, -mid.Height);

            var cs = 6; // cap size

            // vertical arrow
            if (Math.Abs(pp.Y) > 25)
            {
                var nn = pp.Y < 0 ? -1 : +1; // toggle up/down
                var z1 = new Size(0, 15 * -nn);
                var z2 = new Size(0, -pp.Y);
                var p1 = Point.Add(pp, z1);
                var p2 = Point.Add(pp, z2);

                g.DrawLine(GameColors.penCyan2, p1, p2);
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X - cs, p2.Y + (cs * nn));
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X + cs, p2.Y + (cs * nn));
            }

            // horizontal arrow
            if (Math.Abs(pp.X) > 25)
            {
                var nn = pp.X < 0 ? +1 : -1; ; // toggle left/right
                var z1 = new Size(15 * nn, 0);
                var z2 = new Size(-pp.X, 0);
                var p1 = Point.Add(pp, z1);
                var p2 = Point.Add(pp, z2);

                g.DrawLine(GameColors.penCyan2, p1, p2);
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X - (cs * nn), p2.Y - cs);
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X - (cs * nn), p2.Y + cs);
            }

            var alt = game.status.Altitude.ToString("N0");
            var footerTxt = $"Offset: {pp.X}m, {pp.Y}m  | Alt: {alt}m";

            // header text and rotation arrows
            this.drawOriginRotationGuide();
            this.drawFooterText(footerTxt);

        }

        private void drawOriginRotationGuide()
        {
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);

            var adjustAngle = siteData.siteHeading - game.status.Heading;

            if (Math.Abs(adjustAngle) > 1)
            {
                var ax = 30f;
                var ay = -mid.Height * 0.8f;
                var ad = 80;
                var ac = 10;
                if (adjustAngle < 0)
                {
                    ax *= -1;
                    ac *= -1;
                    ad *= -1;
                }
                var pp = GameColors.penCyan2;
                g.DrawLine(pp, ax, ay, ax + ad, ay);
                g.DrawLine(pp, ax + ad, ay, ax + ad - ac, ay - 10);
                g.DrawLine(pp, ax + ad, ay, ax + ad - ac, ay + 10);
            }

            var headerTxt = $"Site heading: {siteData.siteHeading}° | Rotate ship ";
            headerTxt += adjustAngle > 0 ? "right" : "left";
            headerTxt += $" {adjustAngle}°";

            this.drawHeaderText(headerTxt);
        }

        private void drawSiteMap()
        {
            if (g == null || this.underlay == null) return;

            // prepare underlay image
            using (var gg = Graphics.FromImage(this.underlay!))
            {
                gg.ResetTransform();
                //gg.Clear(Color.Black);
                //gg.RotateTransform(360 - game.status!.Heading);

                // shift by underlay size
                gg.TranslateTransform(ux, uy);
                // rotate by site heading only
                gg.RotateTransform(+siteData.siteHeading);
                gg.ScaleTransform(this.template!.scaleFactor, this.template!.scaleFactor);

                // draw the site bitmap and trails(?)
                gg.DrawImageUnscaled(this.siteMap!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);
                //int xx = 0; // -this.trails!.Width;
                //int yy = 0; // -this.trails!.Height / 2;
                //gg.DrawImageUnscaled(this.trails!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);
            }

            float x = commanderOffset.X;
            float y = commanderOffset.Y;

            this.clipToMiddle(4, 26, 4, 24);

            g.DrawImageUnscaled(this.underlay!, -(int)(ux - x), -(int)(uy - y));

            // draw compass rose lines centered on underlay
            g.DrawLine(Pens.DarkRed, -this.Width * 2, y, +this.Width * 2, y);
            g.DrawLine(Pens.DarkRed, x, y, x, +this.Height * 2);
            g.DrawLine(Pens.Red, x, -this.Height * 2, x, y);
            g.ResetClip();

            //this.drawTouchdownAndSrvLocation();

            this.drawCommander();

            if (siteData.siteHeading != -1)
            {
                //this.drawSiteSummaryFooter($"Site heading: {siteHeading}°");
                var m = Util.getDistance(Status.here, siteData.location!, (decimal)game.nearBody.radius);
                //var a = Util.getBearing(Status.here, game.nearBody!.guardianSiteLocation!);
                //this.drawSiteSummaryFooter($"{Math.Round(m)}m / {Math.Round(a)}°");

                var ttd = new TrackingDelta(game.nearBody.radius, siteData.location);
                this.drawSiteSummaryFooter($"{Math.Round(m)}m / {ttd}");
            }
        }

        private void drawSiteTypeHelper()
        {
            if (g == null) return;
            g.ResetTransform();
            g.ResetClip();
            string msg;
            SizeF sz;
            var tx = 10f;
            var ty = 10f;

            this.drawSiteSummaryFooter($"{game.nearBody!.bodyName}\r\n{siteData.name}");

            // if we don't know the site type yet ...
            msg = $"Site type unknown!\r\n\r\nSend message:\r\n  alpha\r\n  beta\r\n  gamma";
            sz = g.MeasureString(msg, Game.settings.font1, this.Width);
            g.DrawString(msg, Game.settings.font1, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);
        }

        private void drawSiteHeadingHelper()
        {
            if (g == null) return;
            g.ResetTransform();
            g.ResetClip();
            string msg;
            var tx = 10f;
            var ty = 10f;

            //this.drawSiteSummaryFooter($"{game.nearBody!.bodyName}\r\n{siteData.name}");

            var isRuins = siteData.type == GuardianSiteData.SiteType.alpha || siteData.type == GuardianSiteData.SiteType.beta || siteData.type == GuardianSiteData.SiteType.gamma;
            msg = $"Need site heading. Send message:\r\n\r\n  <degrees>\r\n\r\nTo use current ship heading send:\r\n\r\n  heading\r\n\r\n";
            if (isRuins)
                msg += $"For {siteData.type} sites, align with this buttress:";
            var sz = g.MeasureString(msg, Game.settings.fontMiddle, this.Width);

            g.DrawString(msg, Game.settings.fontMiddle, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);

            // show location of helpful buttress
            if (isRuins)
                g.DrawImage(this.headingGuidance, 40, 20 + sz.Height); //, 200, 200);
        }

        private void drawSiteSummaryFooter(string msg)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();
            var sz = g.MeasureString(msg, Game.settings.fontSmall);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 6;
            g.DrawString(msg, Game.settings.fontSmall, GameColors.brushGameOrange, tx, ty);
        }
    }
}
