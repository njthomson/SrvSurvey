using DecimalMath;
using SrvSurvey.game;
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
        private double nearestDist;

        private List<GuardianSiteData> surveyedSites;
        private readonly Dictionary<string, GuardianSiteData?> filteredSites = new();
        private GuardianSiteData? siteData;
        private float angle = 0f;

        public FormRuins(GuardianSiteData? siteData)
        {
            InitializeComponent();
            map.MouseWheel += Map_MouseWheel;
            panelEdit.Visible = false;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formRuinsLocation);

            this.getAllSurveyedRuins();

            comboSiteType.SelectedIndex = 0;
            comboSite.SelectedIndex = 0;
            this.showFilteredSites(siteData ?? game?.systemSite);

            checkNotes.Checked = Game.settings.mapShowNotes;
            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;

            map.BackColor = Color.Black;

            if (game?.status != null)
                game.status.StatusChanged += Status_StatusChanged;
        }

        private void Status_StatusChanged(bool blink)
        {
            map.Invalidate();
            this.showSurveyProgress();
        }

        private void loadMap(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var newSite = name == game?.systemSite?.displayName
                ? game.systemSite
                : filteredSites.GetValueOrDefault(name);

            this.loadMap(newSite);
        }

        private void loadMap(GuardianSiteData? newSite)
        {
            this.siteData = newSite;
            this.siteData?.loadPub();
            this.siteType = this.siteData?.type ?? GuardianSiteData.SiteType.Unknown;


            // are we loading a template?
            switch (comboSite.Text)
            {
                case "Alpha Template": siteType = GuardianSiteData.SiteType.Alpha; break;
                case "Beta Template": siteType = GuardianSiteData.SiteType.Beta; break;
                case "Gamma Template": siteType = GuardianSiteData.SiteType.Gamma; break;
                case "Lacrosse Template": siteType = GuardianSiteData.SiteType.Lacrosse; break;
                case "Crossroads Template": siteType = GuardianSiteData.SiteType.Crossroads; break;
                case "Fistbump Template": siteType = GuardianSiteData.SiteType.Fistbump; break;
                case "Hammerbot Template": siteType = GuardianSiteData.SiteType.Hammerbot; break;
                case "Bear Template": siteType = GuardianSiteData.SiteType.Bear; break;
                case "Bowl Template": siteType = GuardianSiteData.SiteType.Bowl; break;
                case "Turtle Template": siteType = GuardianSiteData.SiteType.Turtle; break;
                case "Robolobster Template": siteType = GuardianSiteData.SiteType.Robolobster; break;
                case "Squid Template": siteType = GuardianSiteData.SiteType.Squid; break;
                case "Stickyhand Template": siteType = GuardianSiteData.SiteType.Stickyhand; break;
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
                    Game.log($"Cannot find background image: {filepath}");
                    this.img = new Bitmap(40, 40);
                }

                // load template
                this.template = SiteTemplate.sites[siteType];
                this.numX.Value = this.template.imageOffset.X;
                this.numY.Value = this.template.imageOffset.Y;
                this.numScale.Value = (decimal)this.template.scaleFactor;
            }
            else
            {
                this.img = new Bitmap(10, 10);
            }

            // reset some numbers
            this.dragOffset.X = -map.Width / 2f;
            this.dragOffset.Y = -map.Height / 2f;
            scale = 0.5f;
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
            lblObeliskGroups.Text = "Obelisk groups: " + (siteData?.obeliskGroups == null ? "" : string.Join("", siteData!.obeliskGroups));

            this.showSurveyProgress();

            //if (this.siteData != null && this.siteData.siteHeading >= 0 && this.siteData.relicTowerHeading >= 0)
            //{
            //    var sh = this.siteData.siteHeading;
            //    var th = this.siteData.relicTowerHeading;
            //    if (sh < th) sh += 360;
            //    var d = sh - th;
            //    if (d > 180) d = 360 - d;
            //    lblStatus.Text += $", diff: {d}°";
            //}
        }

        private void showSurveyProgress()
        {
            if (siteData == null)
            {
                lblSurveyCompletion.Text = $"";
                progressSurvey.Value = 0;
                return;
            }

            var countRelics = siteData.poiStatus.Count(_ => _.Key.StartsWith('t') && _.Value == SitePoiStatus.present);

            // site heading
            var total = +1
                // count of non-obelisk POIs
                + template.countNonObelisks;
            // count relic towers again (for their headings)
            //+ countRelics;
            if (siteData.isRuins)
                total++; // for relic tower headings
            else
                total += countRelics;

            var actual = siteData.poiStatus.Count;
            if (siteData.siteHeading >= 0) actual++;
            if (siteData.isRuins && siteData.relicTowerHeading >= 0) actual++;
            if (!siteData.isRuins) actual += siteData.relicHeadings.Count;
            //    actual += siteData.relicHeadings.Count;
            //else if (siteData.relicTowerHeading >= 0)
            //    actual += countRelics;

            var progress = (100.0 / total * actual).ToString("0");
            lblSurveyCompletion.Text = $"Survey: {progress}%";
            progressSurvey.Maximum = total;
            progressSurvey.Value = actual;

            var countTowers = siteData.poiStatus.Count(_ => _.Key.StartsWith("t") && _.Value == SitePoiStatus.present);
            var countItems = siteData.poiStatus.Count(_ => !_.Key.StartsWith("t") && (_.Value == SitePoiStatus.present || _.Value == SitePoiStatus.empty));
            var siteHeading = this.siteData.siteHeading > -1 ? $"{this.siteData.siteHeading}°" : "?";

            lblStatus.Text = $"Relic Towers: {countTowers}, puddles: {countItems}, site heading: {siteHeading}";
            if (siteData.isRuins)
            {
                var relicTowerHeading = this.siteData.relicTowerHeading > 0 ? $"{this.siteData.relicTowerHeading}°" : "?";
                lblStatus.Text += $", relic tower heading: {relicTowerHeading}";
            }
        }

        private void getAllSurveyedRuins()
        {
            this.surveyedSites = GuardianSiteData.loadAllSites(false);
        }

        private void showFilteredSites(GuardianSiteData? siteData = null)
        {
            if (string.IsNullOrEmpty(comboSiteType.Text) || comboSiteType.Enabled == false) return;

            Enum.TryParse<GuardianSiteData.SiteType>(comboSiteType.Text, true, out GuardianSiteData.SiteType targetType);

            filteredSites.Clear();
            if (siteData != null)
            {
                targetType = siteData.type;
                filteredSites.Add($"{siteData.type} Template", null);
                comboSiteType.Enabled = false;
                comboSiteType.Text = siteData.type.ToString();
                comboSiteType.Enabled = true;
            }
            else if (targetType == GuardianSiteData.SiteType.Unknown)
            {
                filteredSites.Add("Alpha Template", null);
                filteredSites.Add("Beta Template", null);
                filteredSites.Add("Gamma Template", null);

                filteredSites.Add("Lacrosse Template", null);
                filteredSites.Add("Crossroads Template", null);
                filteredSites.Add("Fistbump Template", null);
                filteredSites.Add("Hammerbot Template", null);
                filteredSites.Add("Bear Template", null);
                filteredSites.Add("Bowl Template", null);
                filteredSites.Add("Turtle Template", null);
                filteredSites.Add("Robolobster Template", null);
                filteredSites.Add("Squid Template", null);
                filteredSites.Add("Stickyhand Template", null);
            }
            else
            {
                filteredSites.Add($"{targetType} Template", null);
            }

            foreach (var survey in this.surveyedSites)
            {
                if (targetType == GuardianSiteData.SiteType.Unknown || survey.type == targetType)
                {
                    var prefix = $"{survey.systemAddress} {survey.bodyId}";
                    var name = survey.isRuins
                        ? $"{survey.bodyName ?? prefix}, ruins #{survey.index} - {survey.type}"
                        : $"{survey.bodyName ?? prefix} - {survey.type}";

                    if (filteredSites.ContainsKey(name))
                        Game.log($"Why is {name} here twice?");
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

            if (string.IsNullOrEmpty(comboSite.Text) && comboSite.Items.Count > 0)
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

        private void map_DoubleClick(object sender, EventArgs e)
        {
            // do nothing if these don't match
            if (game.systemSite != null && game.systemSite != this.siteData)
                return;

            if (this.nearestDist < 30)
            {
                var oldStatus = siteData!.getPoiStatus(this.nearestPoi.name);

                if (oldStatus == SitePoiStatus.unknown)
                    setPoiStatus(this.nearestPoi.name, SitePoiStatus.present);
                else if (oldStatus == SitePoiStatus.present)
                    setPoiStatus(this.nearestPoi.name, SitePoiStatus.absent);
                else if (oldStatus == SitePoiStatus.absent)
                    setPoiStatus(this.nearestPoi.name, SitePoiStatus.unknown);
            }
        }

        private void map_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.mousePos = new PointF(
                    e.X - mapCenter.X,
                    e.Y - mapCenter.Y
                );
                map.Invalidate();
                Application.DoEvents();
                if (this.nearestDist < 30)
                    this.prepContext(e);
            }
            else
            {
                mouseDownPoint = e.Location;
                dragging = true;
                map.Cursor = Cursors.SizeAll;
                showStatus();
            }
        }

        private void map_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            map.Cursor = Cursors.Hand;
            showStatus();
        }

        private void prepContext(MouseEventArgs e)
        {
            if (this.nearestDist > 30 || this.siteData == null) return;
            if (this.nearestPoi.type == POIType.brokeObelisk || this.nearestPoi.type == POIType.obelisk || this.nearestPoi.type == POIType.emptyPuddle || this.nearestPoi.name.StartsWith('x')) return;

            mnuName.Text = $"Name: {this.nearestPoi.name}";
            mnuType.Text = $"Type: {this.nearestPoi.type}";

            mnuUnknown.Checked = mnuPresent.Checked = mnuAbsent.Checked = mnuEmpty.Checked = false;

            if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.unknown)
                mnuUnknown.Checked = true;
            else if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.present)
                mnuPresent.Checked = true;
            else if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.absent)
                mnuAbsent.Checked = true;
            else if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.empty)
                mnuEmpty.Checked = true;

            mapContext.Show(this.map, e.X, e.Y - (mnuName.Height * 2));
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
                lblSelectedItem.Text = $"{poi.name} ({poi.type}): {siteData?.getPoiStatus(poi.name)}";
            else
                lblSelectedItem.Text = "";


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
            g.RotateTransform(+angle);
            g.DrawImage(this.img, imgRect);
            g.RotateTransform(-angle);

            if (this.siteData != null)
            {
                var heading = (float)this.siteData.siteHeading;
                if (heading >= 0)
                {
                    g.RotateTransform(-heading);
                    g.DrawLine(Pens.DarkRed, 0, -map.Height * 2, 0, 0);
                    g.RotateTransform(+heading);
                }

                if (siteData.isRuins && this.siteData.relicTowerHeading != -1)
                {
                    heading = (float)(this.siteData.relicTowerHeading - siteData.siteHeading);
                    if (heading != -1)
                    {
                        var rot = heading;
                        g.RotateTransform(+rot);
                        g.DrawLine(Pens.DarkCyan, 0, -map.Height * 2, 0, 0);
                        g.RotateTransform(-rot);
                    }
                }
            }

            if (panelEdit.Visible)
            {
                var pp = new Pen(Color.Yellow, 0.2f) { DashStyle = DashStyle.Dot };
                g.DrawLine(pp, 0, -map.Height * 2, 0, +map.Height * 2);
                g.DrawLine(pp, -map.Width * 2, 0, +map.Width * 2, 0);
            }

            drawArtifacts(g);

            drawLegend(g);

            if (game?.systemSite?.type == this.siteType)
                drawCommander(g);
        }

        private void drawCommander(Graphics g)
        {
            if (game.systemSite == null) return;

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
            if (Game.activeGame?.systemSite == null) return null;

            var siteData = Game.activeGame.systemSite;

            var cd = Util.getDistance(Status.here, siteData.location, (decimal)Game.activeGame.systemBody!.radius);
            var cA = DecimalEx.PiHalf + Util.getBearingRad(siteData.location, Status.here) - (decimal)Util.degToRad(siteData.siteHeading);

            return new PointF(
                (float)(DecimalEx.Cos(cA) * cd),
                (float)(DecimalEx.Sin(cA) * cd)
            );
        }

        private void drawCommander(Graphics g, PointF cp, float r)
        {
            if (game?.systemSite == null || game?.status == null || game.isShutdown) return;

            var p1 = GameColors.penLime4;
            var rect = new RectangleF(cp.X - r, cp.Y - r, r * 2, r * 2);
            g.DrawEllipse(p1, rect);


            var pt = Util.rotateLine(game.status.Heading - game.systemSite.siteHeading - 180, 20);
            g.DrawLine(GameColors.penLime4, cp.X, cp.Y, cp.X + pt.X, cp.Y - pt.Y);
        }

        private void drawArtifacts(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(mapCenter.X - dragOffset.X, mapCenter.Y - dragOffset.Y);
            g.ScaleTransform(this.scale, this.scale);

            this.nearestDist = double.MaxValue;
            var nearestPt = PointF.Empty;
            this.nearestPoi = null!;

            foreach (var foo in template.obeliskGroupNameLocations)
            {
                // skip obelisks in groups not in this site
                if (siteData?.obeliskGroups.Contains(foo.Key[0]) == false) continue;

                var angle = 180 - foo.Value.X;
                var dist = foo.Value.Y;
                var pt = Util.rotateLine((decimal)angle, (decimal)dist);

                //g.DrawLine(Pens.DarkBlue, 0,0, -pt.X, -pt.Y);
                var sz = g.MeasureString(foo.Key, GameColors.fontBigBold);
                g.DrawString(foo.Key, GameColors.fontBig, Brushes.DarkCyan, -pt.X - (sz.Width / 2) + 2, -pt.Y - (sz.Height / 2) + 2);
            }

            // and draw all the POIs
            var poiToRender = siteData?.rawPoi == null
                ? this.template.poi
                : this.template.poi.Union(siteData.rawPoi);

            foreach (var poi in poiToRender)
            {
                // skip obelisks in groups not in this site
                if (siteData?.obeliskGroups?.Count > 0 && (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk) && !siteData.obeliskGroups.Contains(poi.name[0])) continue;

                // calculate render point for POI
                var pt = Util.rotateLine(
                    360 - (decimal)poi.angle,
                    poi.dist);

                // is this the closest POI?
                var x = pt.X * this.scale - mousePos.X - dragOffset.X;
                var y = pt.Y * this.scale - mousePos.Y - dragOffset.Y;
                var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                if (!this.dragging && d < this.nearestDist && d < 30)
                {
                    this.nearestDist = d;
                    this.nearestPoi = poi;
                    nearestPt = new PointF(pt.X, pt.Y);
                }

                //var drawItem = this.siteData == null || this.siteData.poiStatus.GetValueOrDefault(poi.name) != SitePoiStatus.unknown;
                var poiStatus = poi.name.StartsWith("x")
                    ? SitePoiStatus.present
                    : this.siteData?.poiStatus.GetValueOrDefault(poi.name);

                if (poi.type == POIType.relic)
                    drawRelicTower(g, pt, poi, siteData, poiStatus);
                else if (poi.type == POIType.pylon)
                    drawPylon(g, pt, poi, siteData);
                else if (poi.type == POIType.component)
                    drawComponent(g, pt, poi, siteData);
                else if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
                    drawObelisk(g, pt, poiStatus);
                else if (poi.type == POIType.orb || poi.type == POIType.casket || poi.type == POIType.tablet || poi.type == POIType.totem || poi.type == POIType.urn || poi.type == POIType.unknown)
                    drawPuddle(g, pt, poi.type, poiStatus);
                else
                {
                    Game.log($"Unexpected poi.type: {poi.type}");
                }
            }

            // draw highlight over closest POI
            if (this.nearestPoi != null)
                g.DrawEllipse(GameColors.penCyan4, nearestPt.X - 14, nearestPt.Y - 14, 28, 28);
        }

        private static void drawPuddle(Graphics g, PointF pt, POIType poiType, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            var brush = GameColors.Map.brushes[poiType][poiStatus ?? SitePoiStatus.present];
            var pen = GameColors.Map.pens[poiType][poiStatus ?? SitePoiStatus.present];

            var d = 5;
            var rect = new RectangleF(pt.X - d, pt.Y - d, d * 2, d * 2);
            g.FillEllipse(brush, rect);

            g.DrawEllipse(pen, rect);
        }

        private static void drawRelicTower(Graphics g, PointF pt, SitePOI? poi, GuardianSiteData? siteData, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            g.RotateTransform(+180);
            var rot = -1;
            var hasRot = siteData != null && poi != null && siteData.relicHeadings.ContainsKey(poi.name) && poiStatus == SitePoiStatus.present;
            if (hasRot)
                rot = siteData!.relicHeadings[poi!.name] - siteData.siteHeading - 180;
            else if (poi?.name.StartsWith('x') == true && poi.rot != -1 && siteData != null)
                rot = (int)poi.rot - siteData.siteHeading - 180;

            PointF[] points =
            {
                new PointF(0, 0 - 6),
                new PointF(0 + 6, 0 + 6),
                new PointF(0 - 6, 0 + 6),
                new PointF(0, 0 - 6),
            };

            var brush = GameColors.Map.brushes[POIType.relic][poiStatus ?? SitePoiStatus.present];
            var pen = GameColors.Map.pens[POIType.relic][poiStatus ?? SitePoiStatus.present];

            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)+rot);

            if (rot != -1)
            {
                var pp = new Pen(Color.FromArgb(32, Color.Blue), 10);
                g.DrawLine(pp, 0, -2000, 0, 2000);
            }

            g.FillPolygon(brush, points);
            g.DrawLines(pen, points);

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);

            g.RotateTransform(-180);
        }

        private static void drawObelisk(Graphics g, PointF pt, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            // TODO: Account for obelisk rotation
            var brush = Brushes.Blue;
            var pen = Pens.Cyan;

            var points = new PointF[] {
                    new PointF(pt.X +1 - 1.5f, pt.Y +4 - 1.5f),
                    new PointF(pt.X +0 - 1.5f, pt.Y +0 - 1.5f),
                    new PointF(pt.X +4 - 1.5f, pt.Y +1 - 1.5f),
                    new PointF(pt.X +1 - 1.5f, pt.Y +4 - 1.5f),
                    //new PointF(pt.X +.5f, pt.Y +-1.5f),
                };

            g.FillPolygon(brush, points);
            g.DrawLines(pen, points);
        }

        private static void drawPylon(Graphics g, PointF pt, SitePOI poi, GuardianSiteData? siteData, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            g.RotateTransform(+180);
            var rot = poi.rot + 180;

            PointF[] points = {
                    new PointF(0, -3),
                    new PointF(+6, 0),
                    new PointF(0, +3),
                    new PointF(-6, 0),
                    new PointF(0, -3),
                };
            var pp = new Pen(Color.DodgerBlue) // SkyBlue ?
            {
                Width = 2,
                LineJoin = LineJoin.Bevel,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };

            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)+rot);

            g.DrawLines(pp, points);
            g.DrawLine(pp, 0, 0, 0, 3);

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);

            g.RotateTransform(-180);
        }

        private static void drawComponent(Graphics g, PointF pt, SitePOI poi, GuardianSiteData? siteData, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            g.RotateTransform(+180);
            var rot = poi.rot + 180;

            PointF[] points = {
                    new PointF(0, +1),
                    new PointF(-2, -2),
                    new PointF(+2, -2),
                    new PointF(0, +1),
                    new PointF(0, +4),
                    new PointF(-5, -4),
                    new PointF(+5, -4),
                    new PointF(0, +4),
                };
            var pp = new Pen(Color.Lime)
            {
                Width = 1,
                DashStyle = DashStyle.Dash,
                LineJoin = LineJoin.Bevel,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };

            rot -= 45;
            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)+rot);

            g.DrawLines(pp, points);

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);

            g.RotateTransform(-180);
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
            drawRelicTower(g, new PointF(tp.X - 10, tp.Y - 10), null, null);

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
            g.DrawLine(Pens.DarkRed, tp.X - 15, tp.Y - 6, tp.X - 5, tp.Y - 14);

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
                // use the same siteData as the game - if needed
                if (game?.systemSite != null && game.systemSite != this.siteData && game.systemSite.displayName == this.siteData.displayName)
                    this.siteData = game.systemSite;

                this.siteData.notes = txtNotes.Text;
                this.siteData.Save();
            }
        }

        private void FormRuins_Activated(object sender, EventArgs e)
        {
            if (this.siteData == null) return;

            // use the same siteData as the game - if needed
            if (game?.systemSite != null && game.systemSite != this.siteData && game.systemSite.displayName == this.siteData.displayName)
            {
                this.siteData = game.systemSite;
                map.Invalidate();
            }

            if (this.siteData.notes != null && txtNotes.Text != this.siteData.notes)
                txtNotes.Text = this.siteData.notes;
        }

        private void setPoiStatus(string name, SitePoiStatus newStatus)
        {
            if (siteData == null) return;

            if (newStatus == SitePoiStatus.unknown)
            {
                if (siteData.poiStatus.ContainsKey(name))
                    siteData.poiStatus.Remove(name);
            }
            else
            {
                siteData.poiStatus[name] = newStatus;
            }

            // update footer counts

            this.showSurveyProgress();

            siteData.Save();
            map.Invalidate();
            Program.getPlotter<PlotGuardians>()?.Invalidate();
            Elite.setFocusED();
        }

        private void mnuPresent_Click(object sender, EventArgs e)
        {
            setPoiStatus(this.nearestPoi.name, SitePoiStatus.present);
        }

        private void mnuAbsent_Click(object sender, EventArgs e)
        {
            setPoiStatus(this.nearestPoi.name, SitePoiStatus.absent);
        }

        private void mnuEmpty_Click(object sender, EventArgs e)
        {
            setPoiStatus(this.nearestPoi.name, SitePoiStatus.empty);
        }

        private void mnuUnknown_Click(object sender, EventArgs e)
        {
            setPoiStatus(this.nearestPoi.name, SitePoiStatus.unknown);
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {

        }

        private void label4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2 && e.Button == MouseButtons.Right)
            {
                panelEdit.Visible = !panelEdit.Visible;
                map.Invalidate();
            }
        }

        private void numX_ValueChanged(object sender, EventArgs e)
        {
            template.imageOffset.X = (int)numX.Value;
            this.map.Invalidate();
            Program.getPlotter<PlotGuardians>()?.Invalidate();
        }

        private void numY_ValueChanged(object sender, EventArgs e)
        {
            template.imageOffset.Y = (int)numY.Value;
            this.map.Invalidate();
            Program.getPlotter<PlotGuardians>()?.Invalidate();
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            template.scaleFactor = (float)numScale.Value;
            this.map.Invalidate();
            Program.getPlotter<PlotGuardians>()?.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SiteTemplate.SaveEdits();
        }

        private void numA_ValueChanged(object sender, EventArgs e)
        {
            if (numA.Value < 0) numA.Value = 360 - numA.Increment;
            else if (numA.Value >= 360) numA.Value = 0;

            this.angle = (float)numA.Value;
            this.map.Invalidate();
            Program.getPlotter<PlotGuardians>()?.Invalidate();
        }
    }
}
