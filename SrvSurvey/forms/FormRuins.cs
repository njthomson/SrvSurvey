using DecimalMath;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal partial class FormRuins : BaseForm
    {
        #region showing and position tracking 

        public static FormRuins? activeForm { get; private set; }

        public static void show(GuardianSiteData? siteData = null)
        {
            if (activeForm == null)
                FormRuins.activeForm = new FormRuins(siteData);

            Util.showForm(FormRuins.activeForm);
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

        private GuardianSiteTemplate template;

        private PointF tp;

        private SitePOI nearestPoi;
        private double nearestDist;

        private List<GuardianSiteData> surveyedSites;
        private readonly Dictionary<string, GuardianSiteData?> filteredSites = new();
        private GuardianSiteData? siteData;
        private float angle = 0f;
        private bool populatingCombos = false;

        private GuardianSiteData? initialData;

        public FormRuins(GuardianSiteData? siteData)
        {
            // TODO: Remove parameter constructor and add a method "setSiteData" - so we can use BaseForm from here
            InitializeComponent();
            this.Icon = Icons.maps;
            map.MouseWheel += Map_MouseWheel;
            panelEdit.Visible = false;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formRuinsLocation);

            this.getAllSurveyedRuins();

            this.initialData = siteData;
            this.showFilteredSites(siteData ?? game?.systemSite);

            checkNotes.Checked = Game.settings.mapShowNotes;
            checkShowLegend.Checked = Game.settings.mapShowLegend;

            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;

            map.BackColor = Color.Black;

            if (game?.status != null)
                game.status.StatusChanged += Status_StatusChanged;

            prepTowerMenus(toolTowerTop);
            prepTowerMenus(toolTowerMiddle);
            prepTowerMenus(toolTowerBottom);
            Util.applyTheme(this);
        }

        private void prepTowerMenus(ToolStripMenuItem menuItem)
        {
            menuItem.DropDownItems.Clear();
            foreach (var cmp in Enum.GetValues<GComponent>())
            {
                var subItem = new ToolStripMenuItem()
                {
                    Name = cmp.ToString(),
                    Text = Components.to(cmp),
                    Tag = cmp,
                };
                subItem.Click += SubItem_Click;
                menuItem.DropDownItems.Add(subItem);
            }

            foreach (ToolStripMenuItem subItem in menuItem.DropDownItems)
                subItem.Click += SubItem_Click;
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

        public bool isRuins { get => siteType == GuardianSiteData.SiteType.Alpha || siteType == GuardianSiteData.SiteType.Beta || siteType == GuardianSiteData.SiteType.Gamma; }

        private void loadMap(GuardianSiteData? newSite)
        {
            var oldSiteType = this.siteData?.type;
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

            if (oldSiteType != this.siteType)
            {
                if (this.siteType == GuardianSiteData.SiteType.Alpha || this.siteType == GuardianSiteData.SiteType.Beta || this.siteType == GuardianSiteData.SiteType.Gamma || this.siteType == GuardianSiteData.SiteType.Stickyhand)
                    scale = 0.5f;
                else
                    scale = 1f;
            }

            if (siteType != GuardianSiteData.SiteType.Unknown)
            {
                var filename = $"{siteType}-background.png".ToLowerInvariant();
                var filepath = Path.Combine(Application.StartupPath, "images", filename);
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
                this.template = GuardianSiteTemplate.sites[siteType];
                this.numX.Value = this.template.imageOffset.X;
                this.numY.Value = this.template.imageOffset.Y;
                this.numScale.Value = (decimal)this.template.scaleFactor;
            }
            else
            {
                this.img = new Bitmap(10, 10);
            }

            // reset some numbers
            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;
            this.dragOffset.X = -map.Width / 2f;
            this.dragOffset.Y = -map.Height / 2f;
            txtNotes.Text = this.siteData?.notes;
            txtSystem.Text = this.siteData?.bodyName;

            showStatus();
            map.Invalidate();

            lblObeliskGroups.Text = "Obelisk groups: " + (siteData?.obeliskGroups == null ? "" : string.Join("", siteData!.obeliskGroups));
            if (newSite != null)
            {
                lblLastVisited.Text = newSite.lastVisited.ToString("d");
            }
            checkNotes.Enabled = newSite?.filepath != null;

            this.showSurveyProgress();
        }

        private void showSurveyProgress()
        {
            if (siteData == null)
            {
                lblSurveyCompletion.Text = null;
                lblStatus.Text = null;
                progressSurvey.Value = 0;
                return;
            }

            var status = siteData.getCompletionStatus();
            var siteHeading = this.siteData.siteHeading > -1 ? $"{this.siteData.siteHeading}°" : "?";

            lblStatus.Text = $"Relic Towers: {status.countRelicsPresent}, puddles: {status.countPuddlesPresent}, site heading: {siteHeading}";
            if (siteData.isRuins)
            {
                var relicTowerHeading = this.siteData.relicTowerHeading > 0 ? $"{this.siteData.relicTowerHeading}°" : "?";
                lblStatus.Text += $", relic tower heading: {relicTowerHeading}";
            }
            lblSurveyCompletion.Text = $"Survey: {status.percent}";
            progressSurvey.Maximum = status.maxScore;
            progressSurvey.Value = status.score;
        }

        private void getAllSurveyedRuins()
        {
            this.surveyedSites = GuardianSiteData.loadAllSites(false);
        }

        public void showFilteredSites(GuardianSiteData? siteData = null)
        {
            if (comboSiteType.Enabled == false) return;
            populatingCombos = true;

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
                        ? $"{survey.bodyName ?? prefix}, ruins #{survey.index}"
                        : $"{survey.bodyName ?? prefix}";

                    if (targetType == GuardianSiteData.SiteType.Unknown)
                        name += $" - {survey.type}";

                    if (game?.systemSite?.displayName == survey.displayName)
                        name += " [ active survey ]";

                    if (filteredSites.ContainsKey(name))
                        Game.log($"Why is {name} here twice?");
                    else
                        filteredSites.Add(name, survey);
                }
            }

            if (this.initialData != null && !filteredSites.Values.Any(s => s?.displayName == this.initialData.displayName) && (targetType == GuardianSiteData.SiteType.Unknown || initialData.type == targetType))
            {
                var prefix = $"{initialData.systemAddress} {initialData.bodyId}";
                var name = initialData.isRuins
                    ? $"{initialData.bodyName ?? prefix}, ruins #{initialData.index}"
                    : $"{initialData.bodyName ?? prefix}";

                if (targetType == GuardianSiteData.SiteType.Unknown)
                    name += $" - {initialData.type}";

                filteredSites.Add(name, initialData);
                comboSite.Text = initialData.displayName;
            }

            comboSite.DataSource = filteredSites.Keys.ToList();

            // pre-load current site, if we're in one
            if (siteData != null)
            {
                //this.loadMap(game.nearBody?.siteData);
                var match = this.filteredSites.FirstOrDefault(_ => _.Value != null && _.Value.systemAddress == siteData.systemAddress && _.Value.bodyId == siteData.bodyId && _.Value.index == siteData.index);
                comboSite.Text = match.Key;
            }
            else if (string.IsNullOrEmpty(comboSite.Text) && comboSite.Items.Count > 0)
            {
                comboSite.SelectedIndex = 0;
            }
            else
            {
                comboSiteType.SelectedIndex = 0;
                comboSite.SelectedIndex = 0;
            }

            loadMap(comboSite.Text);
            populatingCombos = false;
        }

        private void comboSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingCombos) return;
            this.loadMap(comboSite.Text);
        }

        private void comboSiteType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingCombos) return;
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

                map.Invalidate();
            }
            else
            {
                this.mousePos = new PointF(
                    e.X - mapCenter.X,
                    e.Y - mapCenter.Y
                );
                var poi0 = this.nearestPoi;
                findNearestArtifact(this.mousePos);
                if (poi0 != this.nearestPoi)
                {
                    showStatus();
                    map.Invalidate();
                }
            }
        }

        private void findNearestArtifact(PointF mPos)
        {
            if (this.template == null) return;

            this.nearestDist = double.MaxValue;
            this.nearestPoi = null!;

            var poiToRender = siteData?.rawPoi == null
                ? this.template.poi
                : this.template.poi.Union(siteData.rawPoi);

            foreach (var poi in poiToRender)
            {
                // skip obelisks in groups not in this site
                if (siteData?.obeliskGroups?.Count > 0 && (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk) && !siteData.obeliskGroups.Contains(poi.name[0])) continue;

                // calculate render point for POI
                var pt = (PointF)Util.rotateLine(
                    360 - (decimal)poi.angle,
                    poi.dist);

                // is this the closest POI?
                var x = pt.X * this.scale - mPos.X - dragOffset.X;
                var y = pt.Y * this.scale - mPos.Y - dragOffset.Y;
                var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                if (!this.dragging && d < this.nearestDist && d < 30)
                {
                    this.nearestDist = d;
                    this.nearestPoi = poi;
                }
            }
        }

        private void map_Click(object sender, EventArgs e)
        {
            //map.Invalidate();
        }

        private void map_DoubleClick(object sender, EventArgs e)
        {
            // do nothing if these don't match
            if (game?.systemSite == null || game.systemSite != this.siteData || siteData?.filepath == null)
                return;

            if (this.nearestDist < 30 && this.siteData != null)
            {
                var oldStatus = this.siteData.getPoiStatus(this.nearestPoi.name);

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
            if (e.Button == MouseButtons.Right && siteData?.filepath != null)
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
                // start dragging
                mouseDownPoint = e.Location;
                dragging = true;
                map.Cursor = Cursors.SizeAll;
                showStatus();
            }
        }

        private void map_MouseUp(object sender, MouseEventArgs e)
        {
            // stop dragging
            dragging = false;
            map.Cursor = Cursors.Hand;
            showStatus();
        }

        private void prepContext(MouseEventArgs e)
        {
            if (this.nearestDist > 30 || this.siteData == null) return;
            if (this.nearestPoi.type == POIType.brokeObelisk || this.nearestPoi.type == POIType.obelisk || this.nearestPoi.type == POIType.emptyPuddle || this.nearestPoi.name.StartsWith('x')) return;

            mnuName.Text = $"{this.nearestPoi.displayType} ({this.nearestPoi.name})";

            mnuUnknown.Checked = mnuPresent.Checked = mnuAbsent.Checked = mnuEmpty.Checked = false;

            if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.unknown)
                mnuUnknown.Checked = true;
            else if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.present)
                mnuPresent.Checked = true;
            else if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.absent)
                mnuAbsent.Checked = true;
            else if (this.siteData!.getPoiStatus(this.nearestPoi.name) == SitePoiStatus.empty)
                mnuEmpty.Checked = true;

            var couldBeEmpty = false;
            switch (this.nearestPoi.type)
            {
                case POIType.casket:
                case POIType.orb:
                case POIType.tablet:
                case POIType.totem:
                    couldBeEmpty = true; break;
            }

            mnuEmpty.Visible = couldBeEmpty;

            // prep component items
            var isTower = Game.settings.guardianComponentMaterials_TEST && this.nearestPoi.type == POIType.component;
            var isDestructablePanel = Game.settings.guardianComponentMaterials_TEST && this.nearestPoi.type == POIType.destructablePanel;
            toolStripTower.Visible = isTower || isDestructablePanel;
            toolTowers.Visible = isTower || isDestructablePanel;
            toolTowerTop.Visible = isTower || isDestructablePanel;
            toolTowerMiddle.Visible = isTower;
            toolTowerBottom.Visible = isTower;

            if (Game.settings.guardianComponentMaterials_TEST)
            {
                prepComponentSubItems(toolTowerTop, 0);
                if (isTower)
                {
                    prepComponentSubItems(toolTowerMiddle, 1);
                    prepComponentSubItems(toolTowerBottom, 2);
                }
            }

            mapContext.Show(this.map, e.X, e.Y - (mnuName.Height * 2));
        }

        private void prepComponentSubItems(ToolStripMenuItem subItem, int idx)
        {
            var cmp = siteData?.components?.GetValueOrDefault(nearestPoi.name)?.items[idx] ?? GComponent.unknown;

            foreach (ToolStripMenuItem child in subItem.DropDownItems)
                child.Checked = (GComponent)child.Tag! == cmp;

            if (idx == 0)
                subItem.Text = $"Top: {Components.to(cmp)}";
            else if (idx == 1)
                subItem.Text = $"Middle: {Components.to(cmp)}";
            else if (idx == 2)
                subItem.Text = $"Bottom: {Components.to(cmp)}";
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
            var poi = this.nearestPoi;
            if (poi != null)
            {
                if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
                    lblSelectedItem.Text = $"{poi.displayType} : {poi.name}";
                else
                    lblSelectedItem.Text = $"{poi.displayType} : {poi.name} : {siteData?.getPoiStatus(poi.name)}";
            }
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
                if (siteData.isRuins && this.siteData.relicTowerHeading != -1)
                {
                    var relicHeading = (float)(this.siteData.relicTowerHeading - siteData.siteHeading);
                    if (relicHeading != -1)
                    {
                        var rot = relicHeading;
                        g.RotateTransform(+rot);
                        g.DrawLine(GameColors.Map.penCentralRelicTowerHeading, 0, -map.Height * 2, 0, 0);
                        g.RotateTransform(-rot);
                    }
                }

                var heading = (float)this.siteData.siteHeading;
                if (heading >= 0)
                {
                    g.RotateTransform(-heading);
                    g.DrawLine(GameColors.Map.penCentralCompass, 0, -map.Height * 2, 0, 0);
                    g.RotateTransform(+heading);
                }
            }

            if (panelEdit.Visible)
            {
                var pp = GameColors.newPen(Color.Yellow, 0.2f, DashStyle.Dot);
                g.DrawLine(pp, 0, -map.Height * 2, 0, +map.Height * 2);
                g.DrawLine(pp, -map.Width * 2, 0, +map.Width * 2, 0);
            }

            //if (template?.destructablePanels?.Count > 0 && false)
            //    this.drawDestructablePanels(g);
            drawArtifacts(g);

            if (game?.systemSite?.displayName == this.siteData?.displayName)
                drawCommander(g);

            if (checkShowLegend.Checked)
                drawLegend(g);

            if (this.img.Width < 50)
            {
                g.ResetTransform();
                // indicate that we have no map to render for this site type
                var txt = $"[ Map not available for {GuardianSiteData.getStructureTypeFromName(template.name)} ]";
                var sz = g.MeasureString(txt, GameColors.fontSmall2Bold);
                var x = (map.Width / 2) - (sz.Width / 2);
                var y = map.Height - sz.Height;
                g.FillRectangle(Brushes.Black, x, y, sz.Width, sz.Height);
                g.DrawString(txt, GameColors.fontSmall2Bold, Brushes.Red, x, y);
            }
        }

        private void drawCommander(Graphics g)
        {
            if (game?.systemSite == null || game?.status == null || game.isShutdown || this.siteData == null) return;

            g.ResetTransform();

            // indicate that we're actively surveying this site
            var sz = g.MeasureString("[ survey active ]", GameColors.fontSmall2Bold);
            g.FillRectangle(Brushes.Black, map.Width - sz.Width, 0, sz.Width, sz.Height);
            g.DrawString("[ survey active ]", GameColors.fontSmall2Bold, Brushes.Lime, map.Width - sz.Width, 0);

            g.TranslateTransform(mapCenter.X - dragOffset.X, mapCenter.Y - dragOffset.Y);
            g.ScaleTransform(this.scale, this.scale);

            var cd = Util.getDistance(Status.here, this.siteData.location, (decimal)this.game.systemBody!.radius);
            var cA = DecimalEx.PiHalf + Util.getBearingRad(siteData.location, Status.here) - (decimal)Util.degToRad(siteData.siteHeading);

            var cp = new PointF(
                (float)(DecimalEx.Cos(cA) * cd),
                (float)(DecimalEx.Sin(cA) * cd)
            );

            g.TranslateTransform(-cp.X, -cp.Y);
            g.RotateTransform(game.status.Heading - siteData.siteHeading - 180);
            g.FillPath(GameColors.shiningCmdrBrush, GameColors.shiningCmdrPath);

            var d = isRuins ? 10f : 4f;
            var dd = d * 2;
            var p = isRuins ? GameColors.penLime4 : GameColors.penLime2;
            g.DrawEllipse(p, -d, -d, dd, dd);
            g.DrawLine(p, 0, 0, 0, dd);
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
                var pt = (PointF)Util.rotateLine((decimal)angle, (decimal)dist);

                //g.DrawLine(Pens.DarkBlue, 0,0, -pt.X, -pt.Y);
                var sz = g.MeasureString(foo.Key, GameColors.fontBigBold);
                g.DrawString(foo.Key, GameColors.fontBig, GameColors.brushDarkCyan, -pt.X - (sz.Width / 2) + 2, -pt.Y - (sz.Height / 2) + 2);
            }

            // and draw all the POIs
            var poiToRender = siteData?.rawPoi == null
                ? this.template.poi
                : this.template.poi.Union(siteData.rawPoi);

            foreach (var poi in poiToRender)
            {
                // skip obelisks in groups not in this site TODO: unnecessary?
                if (siteData?.obeliskGroups?.Count > 0 && (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk) && !siteData.obeliskGroups.Contains(poi.name[0])) continue;

                // calculate render point for POI
                var pt = (PointF)Util.rotateLine(
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

                var poiStatus = siteData?.getPoiStatus(poi.name) ?? SitePoiStatus.present;

                // anything unknown gets a blue circle underneath with dots
                if (poiStatus == SitePoiStatus.unknown && poi.type != POIType.obelisk && poi.type != POIType.brokeObelisk && poi.type != POIType.destructablePanel)
                    drawUnknownCircle(g, pt.X, -pt.Y, poi.type, this.isRuins);

                if (poi.type == POIType.relic)
                    drawRelicTower(g, pt, poi, siteData, poiStatus);
                else if (poi.type == POIType.pylon)
                    drawPylon(g, pt, poi.rot, poiStatus);
                else if (poi.type == POIType.component)
                    drawComponent(g, pt.X, pt.Y, poi.rot, siteData?.components?.GetValueOrDefault(poi.name), poiStatus);
                else if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
                    drawObelisk(g, pt.X, pt.Y, (float)poi.rot, siteData?.getActiveObelisk(poi.name));
                else if (poi.type == POIType.destructablePanel)
                    drawDestructablePanel(g, pt, siteData?.components?.GetValueOrDefault(poi.name)?.items[0] ?? GComponent.unknown);
                else if (Util.isBasicPoi(poi.type))
                    drawPuddle(g, pt, poi.type, poiStatus);
                else
                {
                    Game.log($"Unexpected poi.type: {poi.type}");
                }
            }

            // draw highlight over closest POI
            if (this.nearestPoi != null)
                g.DrawEllipse(GameColors.penLime4Dot, nearestPt.X - 14, nearestPt.Y - 14, 28, 28);
        }

        private static void drawUnknownCircle(Graphics g, float x, float y, POIType poiType, bool isRuins)
        {
            g.Adjust(x, y, 0, () =>
            {
                var d = 10f;
                if (poiType == POIType.relic) d = 16f;
                if (isRuins) d *= 1.6f;
                var dd = d / 2f;

                // anything unknown gets a blue circle underneath with dots
                g.FillEllipse(GameColors.brushAroundPoiUnknown, -dd - 5, -dd - 5, d + 10, d + 10);
                g.DrawEllipse(GameColors.penAroundPoiUnknown, -dd - 4, -dd - 4, d + 8, d + 8);
            });
        }

        private void drawPuddle(Graphics g, PointF pt, POIType poiType, SitePoiStatus? poiStatus = SitePoiStatus.present, bool forLegend = false)
        {
            var brush = GameColors.Map.brushes[poiType][poiStatus ?? SitePoiStatus.present];
            var pen = GameColors.Map.pens[poiType][poiStatus ?? SitePoiStatus.present];

            var d = this.isRuins && poiStatus != SitePoiStatus.unknown && !forLegend ? 8 : 5;
            var rect = new RectangleF(pt.X - d, pt.Y - d, d * 2, d * 2);

            g.FillEllipse(brush, rect);
            g.DrawEllipse(pen, rect);
        }

        private static PointF[] relicPoints =
        {
            new PointF(0, 0 - 8),
            new PointF(0 + 8, 0 + 8),
            new PointF(0 - 8, 0 + 8),
            new PointF(0, 0 - 8),
        };

        private static void drawRelicTower(Graphics g, PointF pt, SitePOI? poi, GuardianSiteData? siteData, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            g.RotateTransform(+180);
            var rot = siteData?.getRelicHeading(poi?.name);
            if (siteData != null && poi != null && rot != null)
                rot = rot - siteData.siteHeading - 180;


            var brush = GameColors.Map.brushes[POIType.relic][poiStatus ?? SitePoiStatus.present];
            var pen = GameColors.Map.pens[POIType.relic][poiStatus ?? SitePoiStatus.present];

            g.TranslateTransform(-pt.X, -pt.Y);

            if (rot != null)
            {
                g.RotateTransform((float)+rot);
                g.DrawLine(GameColors.Map.penRelicTowerHeading, 0, -2000, 0, 2000);
            }

            g.FillPolygon(brush, relicPoints);
            g.DrawLines(pen, relicPoints);

            if (rot != null)
                g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);

            g.RotateTransform(-180);
        }

        private static PointF[] obeliskPoints = new PointF[]
        {
            new PointF(+1 - 1.5f, +4 - 1.5f),
            new PointF(+0 - 1.5f, +0 - 1.5f),
            new PointF(+4 - 1.5f, +1 - 1.5f),
            new PointF(+1 - 1.5f, +4 - 1.5f),
            new PointF(+0 - 1.5f, +0 - 1.5f)
            //new PointF(pt.X +.5f, pt.Y +-1.5f),
        };

        private static void drawObelisk(Graphics g, float x, float y, float rot, ActiveObelisk? activeObelisk = null)
        {
            // TODO: Account for obelisk rotation
            var brush = Brushes.Blue;
            var pen = GameColors.penDarkCyan1;

            g.Adjust(x, -y, rot + 180, () =>
            {
                // show dithered arc for active obelisks - changing the colour if scanned or is relevant for Ram Tah
                if (activeObelisk != null)
                {
                    GameColors.shiningBrush.CenterColor = GameColors.Orange;
                    g.FillPath(GameColors.shiningBrush, GameColors.shiningPath);
                }

                g.FillPolygon(brush, obeliskPoints);
                g.DrawLines(pen, obeliskPoints);
            });
        }

        private static PointF[] pylonPoints =
        {
            new PointF(0, -3),
            new PointF(+6, 0),
            new PointF(0, +3),
            new PointF(-6, 0),
            new PointF(0, -3),
        };

        private static void drawPylon(Graphics g, PointF pt, decimal rot, SitePoiStatus? poiStatus = SitePoiStatus.present)
        {
            var pp = new Pen(GameColors.Map.colorUnknown)
            {
                Width = 2 * GameColors.scaleFactor,
                LineJoin = LineJoin.Bevel,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };

            switch (poiStatus)
            {
                case SitePoiStatus.present: pp.Color = Color.DodgerBlue; break; // SkyBlue ?
                case SitePoiStatus.absent: pp.Color = GameColors.Map.colorAbsent; break;
            }

            g.Adjust(180, -pt.X, pt.Y, (float)rot + 180f, () =>
            {
                g.DrawLines(pp, pylonPoints);
                g.DrawLine(pp, 0, 0, 0, 3);
            });
        }

        private static PointF[] componentPoints =
        {
            new PointF(0, +1),
            new PointF(-2, -2),
            new PointF(+2, -2),
            new PointF(0, +1),
            new PointF(0, +4),
            new PointF(-5, -4),
            new PointF(+5, -4),
            new PointF(0, +4),
        };

        private static void drawComponent(Graphics g, float x, float y, decimal rot, Components? components, SitePoiStatus poiStatus = SitePoiStatus.present)
        {
            var pp = GameColors.Map.pens[POIType.component][poiStatus];
            if (poiStatus == SitePoiStatus.unknown) pp = GameColors.Map.penComponentUnknown;

            g.Adjust(180, -x, y, (float)rot - 45f + 180f, () =>
            {
                g.DrawLines(pp, componentPoints);
            });

            if (Game.settings.guardianComponentMaterials_TEST && components != null)
            {
                // rotate relative to window, NOT site heading or cmdr's direction
                g.Adjust(180, -x, y, 30, () =>
                {
                    // top, middle, bottom
                    drawComponentMaterial(g, components.items[0], 0);
                    drawComponentMaterial(g, components.items[1], 242);
                    drawComponentMaterial(g, components.items[2], 122);
                });
            }
        }

        private static void drawComponentMaterial(Graphics g, GComponent cmp, float rot)
        {
            if (cmp == GComponent.unknown) return;

            var bb = getComponentBrush(cmp);
            g.RotateTransform(+rot);
            g.FillEllipse(bb, -2, 6, 4, 4);
            g.DrawEllipse(Pens.Black, -2, 6, 4, 4);
            g.RotateTransform(-rot);
        }

        private static Brush getComponentBrush(GComponent cmp)
        {
            switch (cmp)
            {
                case GComponent.unknown: return Brushes.Transparent;
                case GComponent.cell: return Brushes.Lime;
                case GComponent.conduit: return Brushes.Cyan;
                case GComponent.tech: return Brushes.OrangeRed;
            }

            throw new Exception($"Exexpected GComponent: {cmp}");
        }

        private static void drawDestructablePanel(Graphics g, PointF pt, GComponent cmp)
        {
            if (!Game.settings.guardianComponentMaterials_TEST) return;

            g.Adjust(0, pt.X, -pt.Y, 0, () =>
            {
                if (cmp == GComponent.unknown)
                {
                    // anything unknown gets a blue circle underneath with dots
                    g.FillEllipse(GameColors.brushAroundPoiUnknown, -5, -5, +10, +10);
                    g.DrawEllipse(GameColors.penAroundPoiUnknown, -4.5f, -4.5f, +9, +9);

                    g.DrawRectangle(GameColors.penCyan1, -2, -2, 4, 4);
                }
                else
                {
                    g.FillRectangle(getComponentBrush(cmp), -2, -2, 4, 4);
                    g.DrawRectangle(Pens.Black, -2, -2, 4, 4);
                }
            });
        }

        private void drawDestructablePanels(Graphics g)
        {
            if (template?.destructablePanels == null || siteData == null || true) return;

            foreach (var poi in template.destructablePanels)
            {
                // calculate render point for POI
                //var deg = 180 - siteData.siteHeading - poi.angle;
                //var pt = (PointF)Util.rotateLine(deg, poi.dist);

                var pt = (PointF)Util.rotateLine(
                    360 - (decimal)poi.angle,
                    poi.dist);

                var cmp = siteData.components?.GetValueOrDefault(poi.name)?.items[0] ?? GComponent.unknown;

                g.Adjust(0, pt.X, -pt.Y, 0, () =>
                {
                    var bb = getComponentBrush(cmp);

                    if (cmp == GComponent.unknown)
                    {
                        bb = Brushes.Yellow;
                        // anything unknown gets a blue circle underneath with dots
                        g.FillEllipse(GameColors.brushAroundPoiUnknown, -5, -5, +10, +10);
                        g.DrawEllipse(GameColors.penAroundPoiUnknown, -4.5f, -4.5f, +9, +9);

                        g.DrawRectangle(GameColors.penCyan1, -2, -2, 4, 4);
                    }
                    else
                    {
                        g.FillRectangle(bb, -2, -2, 4, 4);
                    }
                });
            }
        }

        private void drawLegend(Graphics g)
        {
            tp.X = 20;
            tp.Y = 20;
            g.ResetTransform();

            var rect = new RectangleF(tp.X - 5, tp.Y - 5,
                PlotBase.scaled(isRuins ? 114 : 130),
                PlotBase.scaled(isRuins ? 236 : 270));

            g.FillRectangle(GameColors.Map.legend.brush, rect);
            g.DrawRectangle(GameColors.Map.legend.pen, rect);

            drawString(g, "Legend:");
            tp.X += PlotBase.scaled(21);

            drawString(g, "Relic Tower");
            drawRelicTower(g, new PointF(tp.X - 10, tp.Y - 10), null, null);

            drawString(g, "Orb");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.orb, SitePoiStatus.present, true);

            drawString(g, "Casket");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.casket, SitePoiStatus.present, true);

            drawString(g, "Tablet");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.tablet, SitePoiStatus.present, true);

            drawString(g, "Totem");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.totem, SitePoiStatus.present, true);

            drawString(g, "Urn");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.urn, SitePoiStatus.present, true);

            drawString(g, "Empty puddle");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.totem, SitePoiStatus.empty, true);

            drawString(g, "Obelisk");
            drawObelisk(g, tp.X - 10, tp.Y - 10, 0, null);

            if (!isRuins)
            {
                drawString(g, "Energy pylon");
                drawPylon(g, new PointF(tp.X - 10, tp.Y - 10), 0, SitePoiStatus.present);

                drawString(g, "Component tower");
                drawComponent(g, tp.X - 10, tp.Y - 10, 0, null, SitePoiStatus.present);
            }

            drawString(g, "Site heading");
            g.DrawLine(GameColors.Map.penCentralCompass, tp.X - 16, tp.Y - 6, tp.X - 6, tp.Y - 14);

            drawString(g, "Tower heading");
            g.DrawLine(GameColors.Map.penCentralRelicTowerHeading, tp.X - 16, tp.Y - 6, tp.X - 6, tp.Y - 14);

            drawString(g, "Survey needed");
            drawUnknownCircle(g, tp.X - 11, -tp.Y + 8, POIType.totem, isRuins);
        }

        private void drawString(Graphics g, string msg)
        {
            var sz = g.MeasureString(msg, this.Font);
            g.DrawString(msg, this.Font, Brushes.Wheat, tp);
            tp.Y += sz.Height + PlotBase.scaled(1);
        }

        private void checkNotes_CheckedChanged(object sender, EventArgs e)
        {
            Game.settings.mapShowNotes = checkNotes.Checked;
            Game.settings.Save();

            splitter.Panel2Collapsed = this.siteData == null || !Game.settings.mapShowNotes;
            this.Invalidate();
        }

        private void checkShowLegend_CheckedChanged(object sender, EventArgs e)
        {
            Game.settings.mapShowLegend = checkShowLegend.Checked;
            Game.settings.Save();

            map.Invalidate();
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

        private void SubItem_Click(object? sender, EventArgs e)
        {
            var subItem = sender as ToolStripMenuItem;
            if (subItem == null || siteData == null || (nearestPoi.type != POIType.component && nearestPoi.type != POIType.destructablePanel)) return;

            siteData.components ??= new();

            if (!siteData.components.ContainsKey(nearestPoi.name))
                siteData.components[nearestPoi.name] = new Components() { name = nearestPoi.name };

            var idx = 0;
            if (subItem.OwnerItem!.Name!.EndsWith("Middle"))
                idx = 1;
            else if (subItem.OwnerItem.Name.EndsWith("Bottom"))
                idx = 2;

            var cmp = (GComponent)subItem.Tag!;
            siteData.components[nearestPoi.name].items[idx] = cmp;

            // TODO: just call prepComponentSubItems ??

            if (idx == 0)
                subItem.OwnerItem.Text = $"Top: {Components.to(cmp)}";
            else if (idx == 1)
                subItem.OwnerItem.Text = $"Middle: {Components.to(cmp)}";
            else if (idx == 2)
                subItem.OwnerItem.Text = $"Bottom: {Components.to(cmp)}";

            foreach (ToolStripMenuItem sibling in subItem.GetCurrentParent()!.Items)
                sibling.Checked = false;

            subItem.Checked = true;

            PlotBase2.invalidate(nameof(PlotGuardians));
            siteData.Save();
            Elite.setFocusED();
        }

        private void setPoiStatus(string name, SitePoiStatus newStatus)
        {
            if (siteData?.filepath == null) return;

            siteData.poiStatus[name] = newStatus;

            // update footer counts
            this.showSurveyProgress();

            siteData.Save();
            map.Invalidate();
            PlotBase2.invalidate(nameof(PlotGuardians));
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
            PlotBase2.invalidate(nameof(PlotGuardians));
        }

        private void numY_ValueChanged(object sender, EventArgs e)
        {
            template.imageOffset.Y = (int)numY.Value;
            this.map.Invalidate();
            PlotBase2.invalidate(nameof(PlotGuardians));
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            template.scaleFactor = (float)numScale.Value;
            this.map.Invalidate();
            PlotBase2.invalidate(nameof(PlotGuardians));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GuardianSiteTemplate.SaveEdits();
        }

        private void numA_ValueChanged(object sender, EventArgs e)
        {
            if (numA.Value < 0) numA.Value = 360 - numA.Increment;
            else if (numA.Value >= 360) numA.Value = 0;

            this.angle = (float)numA.Value;
            this.map.Invalidate();
            PlotBase2.invalidate(nameof(PlotGuardians));
        }
    }
}
