using DecimalMath;
using SrvSurvey.game;
using SrvSurvey.units;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal partial class FormRuins : Form
    {
        #region showing and position tracking 

        private static FormRuins? activeForm;

        public static void show(GuardianSiteData? siteData = null)
        {
            if (activeForm == null)
                FormRuins.activeForm = new FormRuins(siteData);

            if (FormRuins.activeForm.Visible == false)
                FormRuins.activeForm.Show();
            else
                FormRuins.activeForm.Activate();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            FormRuins.activeForm = null;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formRuinsLocation != rect)
            {
                Game.settings.formRuinsLocation = rect;
                Game.settings.Save();
            }
        }

        #endregion

        protected Game game = Game.activeGame!;

        /// <summary>
        /// The background map image
        /// </summary>
        private Image img;
        /// <summary>
        /// The the location of the mouse pointer, relative to site origin
        /// </summary>
        private PointF mousePos;

        /// <summary>
        /// The center of the map image control
        /// </summary>
        public PointF mapCenter;

        /// <summary>
        /// Offset from dragging the map around
        /// </summary>
        public PointF dragOffset;
        private bool dragging = false;
        private Point mouseDownPoint;

        /// <summary>
        /// The scale factor for rendering
        /// </summary>
        private float scale = 0.5f;

        private GuardianSiteData.SiteType siteType = GuardianSiteData.SiteType.Unknown;

        private SiteTemplate template;

        private PointF tp;

        private SitePOI nearestPoi;

        private List<GuardianSiteData> surveyedSites;
        private readonly Dictionary<string, GuardianSiteData?> filteredSites = new();
        private GuardianSiteData? siteData;

        public FormRuins(GuardianSiteData? siteData)
        {
            InitializeComponent();
            map.MouseWheel += Map_MouseWheel;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formRuinsLocation);

            this.getAllSurveyedRuins();

            comboSiteType.SelectedIndex = 0;
            comboSite.SelectedIndex = 0;
            this.showFilteredSites(siteData ?? game?.nearBody?.siteData);

            checkNotes.Checked = Game.settings.mapShowNotes;
            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;

            if (game?.status != null)
                game.status.StatusChanged += Status_StatusChanged;
        }

        private void Status_StatusChanged(bool blink)
        {
            map.Invalidate();
        }

        private void loadMap(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var newSite = filteredSites.GetValueOrDefault(name);
            this.loadMap(newSite);
        }

        private void loadMap(GuardianSiteData? newSite)
        {
            this.siteData = newSite;

            this.siteType = this.siteData?.type ?? GuardianSiteData.SiteType.Unknown;


            // are we loading a template?
            switch (comboSite.Text)
            {
                case "Alpha Template": siteType = GuardianSiteData.SiteType.Alpha; break;
                case "Beta Template": siteType = GuardianSiteData.SiteType.Beta; break;
                case "Gamma Template": siteType = GuardianSiteData.SiteType.Gamma; break;
            }

            if (siteType != GuardianSiteData.SiteType.Unknown)
            {
                var filename = $"{siteType}-background.png".ToLowerInvariant();
                var filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "images", filename);
                if (File.Exists(filepath))
                {
                    using (var img = Bitmap.FromFile(filepath))
                        this.img = new Bitmap(img);
                }
                else
                {
                    Game.log($"Unexpected! Cannot find background image: {filepath}");
                    this.img = new Bitmap(40, 40);
                }

                // load template
                this.template = SiteTemplate.sites[siteType];
            }
            else
            {
                this.img = new Bitmap(10, 10);
            }

            // reset some numbers
            //this.dragOffset.X = -map.Width / 2f;
            //this.dragOffset.Y = -map.Height / 2f;
            //scale = 0.5f;
            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;
            txtNotes.Text = this.siteData?.notes;

            showStatus();
            map.Invalidate();

            var countTowers = newSite == null
                ? template.poi.Count(_ => _.type == POIType.relic)
                : newSite.poiStatus.Count(_ => _.Key.StartsWith("t") && _.Value == SitePoiStatus.present);
            var countItems = newSite == null
                ? template.poi.Count(_ => _.type != POIType.relic)
                : newSite.poiStatus.Count(_ => !_.Key.StartsWith("t") && (_.Value == SitePoiStatus.present || _.Value == SitePoiStatus.empty));

            var siteHeading = this.siteData?.siteHeading > 0 - 1 ? $"{this.siteData.siteHeading}°" : "?";
            var relicTowerHeading = this.siteData?.relicTowerHeading > 0 ? $"{this.siteData.relicTowerHeading}°" : "?";
            lblStatus.Text = $"Relic Towers: {countTowers}, puddles: {countItems}, site heading: {siteHeading}, relic tower heading: {relicTowerHeading}";

            if (this.siteData != null && this.siteData.siteHeading >= 0 && this.siteData.relicTowerHeading >= 0)
            {
                var sh = this.siteData.siteHeading;
                var th = this.siteData.relicTowerHeading;
                if (sh < th) sh += 360;
                var d = sh - th;
                if (d > 180) d = 360 - d;
                lblStatus.Text += $", diff: {d}°";
            }
        }

        private void getAllSurveyedRuins()
        {
            this.surveyedSites = GuardianSiteData.loadAllSites();
        }

        private void showFilteredSites(GuardianSiteData? siteData = null)
        {
            if (string.IsNullOrEmpty(comboSiteType.Text)) return;

            Enum.TryParse<GuardianSiteData.SiteType>(comboSiteType.Text, true, out GuardianSiteData.SiteType targetType);

            filteredSites.Clear();
            if (targetType == GuardianSiteData.SiteType.Unknown)
            {
                filteredSites.Add("Alpha Template", null);
                filteredSites.Add("Beta Template", null);
                filteredSites.Add("Gamma Template", null);
            }

            filteredSites.Add($"{targetType} Template", null);
            foreach (var survey in this.surveyedSites)
            {
                if (targetType == GuardianSiteData.SiteType.Unknown || survey.type == targetType)
                {
                    var prefix = $"{survey.systemAddress} {survey.bodyId}";
                    var name = $"{survey.bodyName ?? prefix}, ruins #{survey.index} - {survey.type}";

                    if (filteredSites.ContainsKey(name))
                        Game.log($"Why? {name}");
                    else
                        filteredSites.Add(name, survey);
                }
            }

            comboSite.DataSource = filteredSites.Keys.ToList();

            // pre-load current site, if we're in one
            if (siteData != null)
            {
                //this.loadMap(game.nearBody?.siteData);
                var match = this.filteredSites.FirstOrDefault(_ => _.Value != null && _.Value.systemAddress == siteData.systemAddress && _.Value.bodyId == siteData.bodyId && _.Value.index == siteData.index);

                comboSite.Text = match.Key;
            }

            if (string.IsNullOrEmpty(comboSite.Text))
                comboSite.SelectedIndex = 0;
        }

        private void comboSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.loadMap(comboSite.Text);
        }

        private void comboSiteType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.showFilteredSites();
        }

        private void FormRuins_Load(object sender, EventArgs e)
        {
            this.dragOffset.X = -map.Width / 2f;
            this.dragOffset.Y = -map.Height / 2f;
        }

        private void Map_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                this.scale *= 1.1f;
            else
                this.scale *= 0.9f;

            if (this.scale < 0.3) this.scale = 0.3f;
            if (this.scale > 10) this.scale = 10f;

            showStatus();
            map.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormRuins_ResizeEnd(object sender, EventArgs e)
        {
            // recalculate things once resizing stops?
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                this.dragOffset.X += mouseDownPoint.X - e.Location.X;
                this.dragOffset.Y += mouseDownPoint.Y - e.Location.Y;
                mouseDownPoint = e.Location;
            }
            else
            {
                this.mousePos = new PointF(
                    e.X - mapCenter.X,
                    e.Y - mapCenter.Y
                );
            }
            showStatus();
            map.Invalidate();
        }

        private void map_Click(object sender, EventArgs e)
        {
            //map.Invalidate();
        }

        private void map_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPoint = e.Location;
            dragging = true;
            map.Cursor = Cursors.SizeAll;
            showStatus();
        }

        private void map_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            map.Cursor = Cursors.Hand;
            showStatus();
        }

        #region image dev - no longer needed?

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            //loadTemplates();
            //this.template = SiteTemplate.sites[GuardianSiteData.SiteType.beta];

            //this.parseOriginText();
            //this.windowCalculations();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //if (numImgRotation.Value < 0) numImgRotation.Value += 360;
            //if (numImgRotation.Value > 360) numImgRotation.Value -= 360;

            //this.data.rotation = (float)numImgRotation.Value;
            //this.data.Save();

            //map.Invalidate();
            //showStatus();
        }

        private void numImgScale_ValueChanged(object sender, EventArgs e)
        {
            //data.scaleFactor = (float)numImgScale.Value;
            //this.data.Save();

            //map.Invalidate();
            //showStatus();
        }

        #endregion

        private void showStatus()
        {
            var poi = this.nearestPoi!;
            if (poi != null)
                lblSelectedItem.Text = $"{poi.name} ({poi.type}) d: {poi.dist}, a: {poi.angle}° / " + new Angle(poi.angle + 180);
            else
                lblSelectedItem.Text = "";

            var x = (mousePos.X + dragOffset.X) / this.scale;
            var y = (mousePos.Y + dragOffset.Y) / this.scale;

            var dist = Math.Sqrt(x * x + y * y).ToString("N1");

            var a1 = Util.ToAngle(x, -y);
            var a2 = new Angle(a1);

            lblDist.Text = $"Dist: {dist}";
            lblAngle.Text = $"Angle: " + a2.ToString();
            lblMouseX.Text = "X: " + x.ToString("N1");
            lblMouseY.Text = "Y: " + (-y).ToString("N1");
            lblZoom.Text = "Zoom: " + this.scale.ToString("N1");
        }

        private void map_Paint(object sender, PaintEventArgs e)
        {
            if (this.template == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.Bicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.TranslateTransform(mapCenter.X - dragOffset.X, mapCenter.Y - dragOffset.Y);
            g.ScaleTransform(this.scale, this.scale);

            var imgRect = new RectangleF(
                -template.imageOffset.X * template.scaleFactor,
                -template.imageOffset.Y * template.scaleFactor,
                img.Width * template.scaleFactor,
                img.Height * template.scaleFactor);
            g.DrawImage(this.img, imgRect);

            if (this.siteData != null)
            {
                var heading = (float)this.siteData.siteHeading;
                if (heading >= 0)
                {
                    g.RotateTransform(+heading);
                    g.DrawLine(Pens.Red, 0, -map.Height * 2, 0, 0);
                    g.RotateTransform(-heading);
                }

                heading = (float)this.siteData.relicTowerHeading;
                if (heading >= 0)
                {
                    g.RotateTransform(+heading);
                    g.DrawLine(Pens.DarkCyan, 0, -map.Height * 2, 0, 0);
                    g.RotateTransform(-heading);
                }
            }

            drawArtifacts(g);

            drawLegend(g);

            if (game?.nearBody?.siteData != null && game.nearBody.siteData.type == this.siteType)
                drawCommander(g);
        }

        private void drawCommander(Graphics g)
        {
            if (game.nearBody?.siteData == null) return;

            g.ResetTransform();
            g.TranslateTransform(mapCenter.X - dragOffset.X, mapCenter.Y - dragOffset.Y);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(180);

            var cp = calcCmdrToSite();
            if (cp != null)
                drawCommander(g, (PointF)cp, 10f);
        }

        public static PointF? calcCmdrToSite()
        {
            if (Game.activeGame?.nearBody?.siteData == null) return null;

            var siteData = Game.activeGame.nearBody.siteData;

            var cd = Util.getDistance(Status.here, siteData.location, (decimal)Game.activeGame.nearBody.radius);
            var cA = DecimalEx.PiHalf + Util.getBearingRad(siteData.location, Status.here) - (decimal)Util.degToRad(siteData.siteHeading);

            return new PointF(
                (float)(DecimalEx.Cos(cA) * cd),
                (float)(DecimalEx.Sin(cA) * cd)
            );
        }

        private void drawCommander(Graphics g, PointF cp, float r)
        {
            if (game?.nearBody?.siteData == null) return;

            var p1 = GameColors.penLime4;
            var rect = new RectangleF(cp.X - r, cp.Y - r, r * 2, r * 2);
            g.DrawEllipse(p1, rect);


            var pt = Util.rotateLine(game.status.Heading - game.nearBody.siteData.siteHeading - 180, 20);
            g.DrawLine(GameColors.penLime4, cp.X, cp.Y, cp.X + pt.X, cp.Y - pt.Y);
        }

        private void drawArtifacts(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(mapCenter.X - dragOffset.X, mapCenter.Y - dragOffset.Y);
            g.ScaleTransform(this.scale, this.scale);

            var nearestDist = double.MaxValue;
            var nearestPt = PointF.Empty;
            nearestPoi = null!;

            foreach (var poi in template.poi)
            {
                // calculate render point for POI
                var pt = Util.rotateLine(
                    360 - poi.angle,
                    poi.dist);

                // is this the closest POI?
                var x = pt.X * this.scale - mousePos.X - dragOffset.X;
                var y = pt.Y * this.scale - mousePos.Y - dragOffset.Y;
                var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                if (!this.dragging && d < nearestDist)
                {
                    nearestDist = d;
                    this.nearestPoi = poi;
                    nearestPt = new PointF(pt.X, pt.Y);
                }

                //var drawItem = this.siteData == null || this.siteData.poiStatus.GetValueOrDefault(poi.name) != SitePoiStatus.unknown;
                var poiStatus = this.siteData?.poiStatus.GetValueOrDefault(poi.name);
                if (poi.type == POIType.relic)
                    drawRelicTower(g, pt, poiStatus);
                else
                    drawPuddle(g, pt, poi.type, poiStatus);
            }

            // draw highlight over closest POI
            if (this.nearestPoi != null)
                g.DrawEllipse(GameColors.penCyan4, nearestPt.X - 14, nearestPt.Y - 14, 28, 28);
        }

        private static void drawPuddle(Graphics g, PointF pt, POIType poiType, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            var brush = GameColors.Map.brushes[poiType][poiStatus ?? SitePoiStatus.present];
            var pen = GameColors.Map.pens[poiType][poiStatus ?? SitePoiStatus.present];

            var d = 8;
            var rect = new RectangleF(pt.X - d, pt.Y - d, d * 2, d * 2);
            g.FillEllipse(brush, rect);

            g.DrawEllipse(pen, rect);
        }

        private static void drawRelicTower(Graphics g, PointF pt, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            PointF[] points =
            {
                new PointF(pt.X, pt.Y - 8),
                new PointF(pt.X + 8, pt.Y + 8),
                new PointF(pt.X - 8, pt.Y + 8),
                new PointF(pt.X, pt.Y - 8),
            };

            var brush = GameColors.Map.brushes[POIType.relic][poiStatus ?? SitePoiStatus.present];
            var pen = GameColors.Map.pens[POIType.relic][poiStatus ?? SitePoiStatus.present];

            g.FillPolygon(brush, points);
            g.DrawLines(pen, points);
        }

        private void drawLegend(Graphics g)
        {
            tp.X = 20;
            tp.Y = 20;
            g.ResetTransform();
            var rect = new RectangleF(tp.X - 5, tp.Y - 5, 114, 209);

            g.FillRectangle(GameColors.Map.Legend.brush, rect);
            g.DrawRectangle(GameColors.Map.Legend.pen, rect);

            drawString(g, "Legend:");
            tp.X += 20;

            drawString(g, "Relic Tower");
            drawRelicTower(g, new PointF(tp.X - 10, tp.Y - 10));

            drawString(g, "\r\nOrb");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.orb);

            drawString(g, "Casket");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.casket);

            drawString(g, "Tablet");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.tablet);

            drawString(g, "Totem");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.totem);

            drawString(g, "Urn");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.urn);

            drawString(g, "Empty puddle");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.totem, SitePoiStatus.empty);

            drawString(g, "Site heading");
            g.DrawLine(Pens.Red, tp.X - 15, tp.Y - 6, tp.X - 5, tp.Y - 14);

            drawString(g, "Tower heading");
            g.DrawLine(Pens.DarkCyan, tp.X - 15, tp.Y - 6, tp.X - 5, tp.Y - 14);
        }

        private void drawString(Graphics g, string msg)
        {
            var sz = g.MeasureString(msg, this.Font);
            g.DrawString(msg, this.Font, Brushes.Black, tp);
            tp.Y += sz.Height + 1;
        }

        private void checkNotes_CheckedChanged(object sender, EventArgs e)
        {
            Game.settings.mapShowNotes = checkNotes.Checked;
            Game.settings.Save();

            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;
        }

        private void btnSaveNotes_Click(object sender, EventArgs e)
        {
            if (this.siteData != null)
            {
                // reload from file before saving - to avoid clobbering updates during survey
                this.siteData = GuardianSiteData.Load(this.siteData.bodyName, this.siteData.index, true)!;
                this.siteData.notes = txtNotes.Text;
                this.siteData.Save();
            }
        }
    }
}
