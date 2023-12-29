using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey
{
    internal partial class FormEditMap : Form
    {
        private Game game = Game.activeGame!;
        public SitePOI? poi;

        private GuardianSiteData siteData { get => game?.systemSite!; }
        private PlotGuardians plotter { get => Program.getPlotter<PlotGuardians>()!; }
        private SiteTemplate template { get => plotter.template!; }

        public FormEditMap()
        {
            InitializeComponent();
            checkApplyBackgroundLive.Checked = false;
            listPoi.Sorting = SortOrder.Ascending;

            comboPoiType.Items.AddRange(Enum.GetNames<POIType>());
            comboPoiStatus.Items.AddRange(Enum.GetNames<SitePoiStatus>());
            plotter.formEditMap = this;

            // default the background image path to the default file
            var folder = Path.GetDirectoryName(Application.ExecutablePath)!;
            var filepath = Path.Combine(folder, "images", $"{siteData.type}-background.png".ToLowerInvariant());
            txtBackgroundImage.Text = filepath;

            Util.useLastLocation(this, Game.settings.formMapEditor);
        }

        private void FormEditMap_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (plotter != null)
            {
                plotter.formEditMap = null;
                plotter.forcePoi = null;
                plotter.Invalidate();
            }
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formMapEditor != rect)
            {
                Game.settings.formMapEditor = rect;
                Game.settings.Save();
            }
        }

        private void FormEditMap_Load(object sender, EventArgs e)
        {
            if (plotter?.template == null) return;

            // init background image stuff
            numOriginLeft.Value = (decimal)plotter.template.imageOffset.X;
            numOriginTop.Value = (decimal)plotter.template.imageOffset.Y;
            numScaleFactor.Value = (decimal)plotter.template.scaleFactor;
            checkApplyBackgroundLive.Checked = true;

            // init poi stuff
            loadPoiFromTemplate();
            listPoi.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            lblSiteType.Text = template.name;
        }

        private void loadPoiFromTemplate()
        {
            listPoi.Items.Clear();
            foreach (var poi in template.poi)
            {
                var row = createListViewItemForPoi(poi);
                this.listPoi.Items.Add(row);
            }

            setCurrentPoi(plotter.nearestPoi);
        }

        private ListViewItem createListViewItemForPoi(SitePOI poi)
        {
            var rot = poi.rot.ToString();
            if (poi.type == POIType.relic && siteData.relicHeadings.ContainsKey(poi.name))
                rot = siteData.relicHeadings[poi.name].ToString();

            var status = (siteData.poiStatus.ContainsKey(poi.name) ? siteData.poiStatus[poi.name] : SitePoiStatus.unknown).ToString();
            if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
                status = "n/a";

            var subItems = new ListViewItem.ListViewSubItem[]
            {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Name = "name", Text = poi.name },
                    new ListViewItem.ListViewSubItem { Name = "dist", Text = poi.dist.ToString() },
                    new ListViewItem.ListViewSubItem { Name = "angle", Text = poi.angle.ToString() },
                    new ListViewItem.ListViewSubItem { Name = "type", Text = poi.type.ToString() },
                    new ListViewItem.ListViewSubItem { Name = "rot", Text = rot },
                    new ListViewItem.ListViewSubItem { Name = "status", Text = status },
            };

            var row = new ListViewItem(subItems, 0)
            {
                Name = poi.name,
                Tag = poi,
                Text = poi.name,
            };
            return row;
        }

        private void updateRowFromPoi(SitePOI poi)
        {
            foreach (ListViewItem row in listPoi.Items)
            {
                if (row.Tag != poi) continue;

                row.SubItems[0]!.Text = poi.name;
                row.SubItems[1]!.Text = poi.dist.ToString();
                row.SubItems[2]!.Text = poi.angle.ToString();
                row.SubItems[3]!.Text = poi.type.ToString();

                var rot = poi.rot.ToString();
                if (poi.type == POIType.relic && siteData.relicHeadings.ContainsKey(poi.name))
                    rot = siteData.relicHeadings[poi.name].ToString();
                row.SubItems[4]!.Text = rot;

                var status = (siteData.poiStatus.ContainsKey(poi.name) ? siteData.poiStatus[poi.name] : SitePoiStatus.unknown).ToString();
                if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
                    status = "n/a";
                row.SubItems[5]!.Text = status;

                row.Selected = true;
                row.EnsureVisible();
                return;
            }
            listPoi.Sort();
        }

        private void btnSaveEdits_Click(object sender, EventArgs e)
        {
            // save stuff to a clone template file
            SiteTemplate.SaveEdits();
            this.siteData.Save();

            listPoi.Sort();
            Elite.setFocusED();
        }

        #region edit background image

        private void btnChooseBackgroundImage_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Select background image",
                DefaultExt = "png",
                Filter = "Images|*.png",
                Multiselect = false,
                InitialDirectory = Path.GetDirectoryName(txtBackgroundImage.Text),
                FileName = Path.GetFileName(txtBackgroundImage.Text),
            };

            var rslt = dialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                try
                {
                    txtBackgroundImage.Text = dialog.FileName;

                    // try loading the image, just to force errors here if not an image
                    new Bitmap(dialog.FileName);

                    this.plotter.devRefreshBackground(dialog.FileName);
                }
                catch (Exception ex)
                {
                    Game.log(ex.Message);
                }
            }
        }

        private void applyBackgroundImageNumbers(object sender, EventArgs e)
        {
            if (!checkApplyBackgroundLive.Checked) return;
            if (plotter?.template == null) return;

            plotter.template.scaleFactor = (float)numScaleFactor.Value;
            plotter.template.imageOffset = new Point((int)numOriginLeft.Value, (int)numOriginTop.Value);
            plotter.Invalidate();
        }

        private void btnApplyImage_Click(object sender, EventArgs e)
        {
            if (plotter?.template == null) return;

            var imagePath = txtBackgroundImage.Text;
            if (!File.Exists(imagePath))
            {
                Game.log($"File not found: {imagePath}");
                return;
            }
            this.plotter.devRefreshBackground(imagePath);

            plotter.template.scaleFactor = (float)numScaleFactor.Value;
            plotter.template.imageOffset = new Point((int)numOriginLeft.Value, (int)numOriginTop.Value);
            plotter.Invalidate();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (plotter?.template == null) return;

            switch (mapZoom.Value)
            {
                case 1: plotter.scale = 0.2f; break;
                case 2: plotter.scale = 0.5f; break;
                case 3: plotter.scale = 0.75f; break;
                case 4: plotter.scale = 1f; break;
                case 5: plotter.scale = 1.5f; break;
                case 6: plotter.scale = 2f; break;
                case 7: plotter.scale = 3f; break;
                case 8: plotter.scale = 4f; break;
                case 9: plotter.scale = 5f; break;
                case 10: plotter.scale = 10f; break;
            }

            plotter.Invalidate();
        }

        #endregion

        #region edit POIs

        #endregion

        public void setCurrentPoi(SitePOI? newPoi)
        {
            if (this.poi == newPoi) return;

            Game.log($"setCurrentPoi: {newPoi}");
            this.poi = newPoi;

            // update edit fields
            if (newPoi == null)
            {
                txtPoiName.Text = "";
                numPoiDist.Value = 0;
                numPoiAngle.Value = 0;
                comboPoiType.Text = "";
                numPoiRot.Value = 0;
                comboPoiStatus.Text = "";
            }
            else
            {
                txtPoiName.Text = newPoi.name;
                numPoiDist.Value = (decimal)newPoi.dist;
                numPoiAngle.Value = (decimal)newPoi.angle;
                comboPoiType.Text = newPoi.type.ToString();
                numPoiRot.Value = (decimal)newPoi.rot;
                comboPoiStatus.Text = siteData.poiStatus.ContainsKey(newPoi.name) ? siteData.poiStatus[newPoi.name].ToString() : SitePoiStatus.unknown.ToString();

                // finally - update grid selection if not already matching
                updateRowFromPoi(newPoi);
                //var listedPoi = listPoi.Items.Find(newPoi.name, false);
                //if (listedPoi != null && listedPoi.Length > 0 && !listedPoi[0].Selected)
                //{
                //    listPoi.SelectedItems.Clear();
                //    listedPoi[0].Selected = true;
                //    listedPoi[0].EnsureVisible();
                //}
                //else
                //{
                //    Game.log("setCurrentPoi not found in grid?");
                //}
            }

            plotter.forcePoi = newPoi;
            plotter.Invalidate();
        }

        private void comboPoiType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked) return;

            Enum.TryParse<POIType>(comboPoiType.Text, out poi!.type);
            numPoiRot.Enabled = poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk || poi.type == POIType.pylon || poi.type == POIType.component;
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void txtPoiName_TextChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.name = txtPoiName.Text;
            updateRowFromPoi(poi);
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void numPoiDist_ValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.dist = numPoiDist.Value;
            updateRowFromPoi(poi);
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void numPoiAngle_ValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.angle = numPoiAngle.Value;
            updateRowFromPoi(poi);
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void numPoiRot_ValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.rot = numPoiRot.Value;
            updateRowFromPoi(poi);
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void comboPoiStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;

            Enum.TryParse<SitePoiStatus>(comboPoiStatus.Text, out var newStatus);
            siteData.poiStatus[poi.name] = newStatus;
            if (newStatus == SitePoiStatus.unknown)
                siteData.poiStatus.Remove(poi.name);

            updateRowFromPoi(poi);
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void btnPoiApply_Click(object sender, EventArgs e)
        {
            if (poi == null) return;

            poi.name = txtPoiName.Text;
            poi.dist = numPoiDist.Value;
            poi.angle = numPoiAngle.Value;
            Enum.TryParse<POIType>(comboPoiType.Text, out poi.type);
            poi.rot = numPoiRot.Value;

            Enum.TryParse<SitePoiStatus>(comboPoiStatus.Text, out var newStatus);
            siteData.poiStatus[poi.name] = newStatus;
            if (newStatus == SitePoiStatus.unknown)
                siteData.poiStatus.Remove(poi.name);

            updateRowFromPoi(poi);
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void checkPoiPrecision_CheckedChanged(object sender, EventArgs e)
        {
            var stepValue = checkPoiPrecision.Checked ? 0.1m : 1m;
            numPoiDist.Increment = stepValue;
            numPoiAngle.Increment = stepValue;
        }

        private void numPoiDist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey && e.Control)
                checkPoiPrecision.Checked = !checkPoiPrecision.Checked;
        }
        private void numPoiAngle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey && e.Control)
                checkPoiPrecision.Checked = !checkPoiPrecision.Checked;
        }

        private void numPoiRot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey && e.Control)
                checkPoiPrecision.Checked = !checkPoiPrecision.Checked;
        }

        private void listPoi_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listPoi.SelectedItems.Count > 0)
            {
                var newPoi = listPoi.SelectedItems[0].Tag as SitePOI;
                setCurrentPoi(newPoi!);
            }
        }

        private void btnFocusGame_Click(object sender, EventArgs e)
        {
            Elite.setFocusED();
        }

        private void btnNewPoi_Click(object sender, EventArgs e)
        {
            var dist = ((decimal)Util.getDistance(Status.here, siteData.location, game.systemBody!.radius));
            // dist -= 5.7; // ?
            var angle = ((decimal)new Angle((Util.getBearing(Status.here, siteData.location) - (decimal)siteData.siteHeading)));
            var rot = (decimal)new Angle(game.status.Heading - this.siteData.siteHeading);

            var newPoi = new SitePOI()
            {
                name = $"{(int)DateTime.Now.TimeOfDay.TotalSeconds}",
                dist = dist,
                angle = angle,
                type = POIType.obelisk, // !! .unknown,
                rot = rot,
            };
            // add to the template and ListView
            template.poi.Add(newPoi);
            var row = createListViewItemForPoi(newPoi);
            this.listPoi.Items.Add(row);

            // make it current item and redraw the plotter
            setCurrentPoi(newPoi);
            txtPoiName.Focus();
        }

        private void checkHighlightAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkHighlightAll.Checked)
                checkHideAllPoi.Checked = false;
            plotter.Invalidate();
        }

        private void checkHideAllPoi_CheckedChanged(object sender, EventArgs e)
        {
            if (checkHideAllPoi.Checked)
                checkHighlightAll.Checked = false;
            plotter.Invalidate();
        }

        private void btnRemovePoi_Click(object sender, EventArgs e)
        {
            if (listPoi.SelectedItems.Count > 0)
            {
                var lvi = listPoi.SelectedItems[0]!;
                listPoi.Items.Remove(lvi);
                template.poi.Remove((SitePOI)lvi.Tag);
                setCurrentPoi(null);
            }
        }

        private void FormEditMap_Activated(object sender, EventArgs e)
        {
            this.Text = "(active) Map editor";
        }

        private void FormEditMap_Deactivate(object sender, EventArgs e)
        {
            this.Text = "Map editor";
        }

        private void listPoi_DoubleClick(object sender, EventArgs e)
        {
            numPoiDist.Select();
        }
    }
}
