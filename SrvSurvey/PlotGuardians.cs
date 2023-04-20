using SrvSurvey.game;

namespace SrvSurvey
{
    internal partial class PlotGuardians : PlotBase
    {
        private SiteTemplate? template;
        private Image? siteMap;
        private Image? trails;
        private Image? underlay;
        /// <summary> Site map width </summary>
        private float ux;
        /// <summary> Site map height </summary>
        private float uy;

        private SiteType? siteType;
        //private float siteHeading = 281;
        private float siteHeading = -1;
        private PointF siteTouchdownOffset;
        private PointF commanderOffset;

        private PlotGuardians() : base()
        {
            InitializeComponent();

            //this.Width = 500;
            //this.Height = 500;

            SiteTemplate.Import();
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
            if (this.siteType != null)
                this.loadSiteTemplate();
        }

        private void PlotGuardians_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Invalidate();
        }

        private void PlotGuardians_MouseClick(object sender, MouseEventArgs e)
        {
            Elite.setFocusED();
        }

        protected override void onJournalEntry(SendText entry)
        {
            base.onJournalEntry(entry);

            // try parsing the site type
            SiteType parsedType;
            if (this.siteType == null && Enum.TryParse<SiteType>(entry.Message, out parsedType))
            {
                this.siteType = parsedType;
                this.loadSiteTemplate();
            }

            // try parsing the site heading
            float parsedHeading;
            if (this.siteHeading == -1 && float.TryParse(entry.Message, out parsedHeading))
                this.siteHeading = parsedHeading;

            //if (entry.Message.StartsWith(".site"))
            //{
            //    float heading;
            //    if (float.TryParse(entry.Message.Substring(5), out heading))
            //    {
            //        this.siteHeading = heading;
            //    }
            //}

            this.Invalidate();
        }

        private void loadSiteTemplate()
        {
            if (this.siteType == null) return;

            this.template = SiteTemplate.sites[(SiteType)this.siteType];
            this.scale = 0.5f;

            this.siteMap = Bitmap.FromFile(this.template.backgroundImage);
            this.trails = new Bitmap(this.siteMap.Width, this.siteMap.Height);

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

            // prepare other stuff
            var offset = game.nearBody!.guardianSiteLocation! - touchdownLocation.Target;
            Game.log($"Touchdown offset: {offset}");
            this.siteTouchdownOffset = new PointF(
                (float)(offset.Long * template.scaleFactor),
                (float)(offset.Lat * -template.scaleFactor));
            Game.log(siteTouchdownOffset);
            Game.log($"siteTouchdownOffset: {siteTouchdownOffset}");

            this.Status_StatusChanged();
        }

        protected override void Status_StatusChanged()
        {
            base.Status_StatusChanged();

            // prepare other stuff
            if (template != null)
            {
                var offset = game.nearBody!.guardianSiteLocation! - Status.here;
                this.commanderOffset = new PointF(
                    (float)(offset.Long * template.scaleFactor),
                    (float)(offset.Lat * -template.scaleFactor));
                Game.log($"commanderOffset: {commanderOffset}");

                /* TODO: get trail tacking working
                using (var gg = Graphics.FromImage(this.trails!))
                {
                    gg.FillRectangle(Brushes.Yellow, -10, -10, 20, 20);

                    // shift by underlay size and rotate by site heading
                    gg.TranslateTransform(this.trails!.Width / 2, this.trails.Height / 2);
                    gg.RotateTransform(+this.siteHeading);

                    // draw the site bitmap and trails(?)
                    const float rSize = 5;
                    float x = commanderOffset.X;
                    float y = commanderOffset.Y;

                    var rect = new RectangleF(
                        x - rSize, y - rSize,
                        rSize * 2, rSize * 2
                    );
                    Game.log($"smudge: {rect}");
                    gg.FillRectangle(Brushes.Cyan, rect);
                }
                */

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

            if (this.siteHeading == -1)
            {
                this.drawSiteHeadingHelper();
                return;
            }

            this.drawSiteMap();

            this.drawTouchdownAndSrvLocation();

            this.drawCommander();

            if (this.siteHeading != -1)
                this.drawSiteSummaryFooter($"Site heading: {siteHeading}°");
        }

        private void drawSiteMap()
        {
            if (g == null || this.underlay == null) return;

            // prepare underlay image
            using (var gg = Graphics.FromImage(this.underlay!))
            {
                gg.Clear(Color.Transparent);

                // shift by underlay size
                gg.TranslateTransform(ux, uy);
                // rotate by site heading only
                gg.RotateTransform(+this.siteHeading);
                gg.ScaleTransform(this.template!.mapScale, this.template!.mapScale);

                // draw the site bitmap and trails(?)
                gg.DrawImageUnscaled(this.siteMap!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);
                //int xx = 0; // -this.trails!.Width;
                //int yy = 0; // -this.trails!.Height / 2;
                //gg.DrawImageUnscaled(this.trails!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);
            }

            float x = commanderOffset.X;
            float y = commanderOffset.Y;

            // draw underlay offset 
            g.ResetTransform();
            this.clipToMiddle(4, 26, 4, 24);
            g.TranslateTransform(mid.Width + 0, mid.Height + 0);
            g.RotateTransform(-game.status!.Heading);
            float mapScale = this.template!.mapScale * this.scale;
            g.ScaleTransform(mapScale, mapScale);

            g.DrawImageUnscaled(this.underlay!, -(int)(ux - x), -(int)(uy - y));

            // draw compass rose lines centered on underlay
            g.DrawLine(Pens.DarkRed, -this.Width * 2, y, +this.Width * 2, y);
            g.DrawLine(Pens.DarkRed, x, y, x, +this.Height * 2);
            g.DrawLine(Pens.Red, x, -this.Height * 2, x, y);
            g.ResetClip();
        }

        private void drawSiteHeadingHelper()
        {
            if (g == null) return;
            g.ResetTransform();
            g.ResetClip();
            string msg;
            SizeF sz;
            var tx = 10f;
            var ty = 10f;

            this.drawSiteSummaryFooter($"{game.nearBody!.bodyName}\r\n{game.nearBody!.guardianSiteName}");

            // if we don't know the site type yet ...
            if (this.siteType == null)
            {
                msg = $"Site type unknown!\r\n\r\nSend message:\r\n  alpha\r\n  beta\r\n  gamma";
                sz = g.MeasureString(msg, Game.settings.font1, this.Width);
                g.DrawString(msg, Game.settings.font1, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);

                return;
            }

            msg = $"Site heading unknown!\r\n\r\nSend message:\r\n  <degrees>";
            sz = g.MeasureString(msg, Game.settings.font1, this.Width);
            g.DrawString(msg, Game.settings.font1, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);
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
