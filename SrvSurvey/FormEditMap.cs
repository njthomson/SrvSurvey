using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    internal partial class FormEditMap : Form
    {
        private Game game = Game.activeGame!;
        public SitePOI poi;

        private GuardianSiteData siteData { get => game?.nearBody?.siteData!; }
        private PlotGuardians plotter { get => Program.getPlotter<PlotGuardians>(); }
        private SiteTemplate template { get => plotter.template; }

        public FormEditMap()
        {
            InitializeComponent();
            checkApplyBackgroundLive.Checked = false;

            comboPoiType.Items.AddRange(Enum.GetNames<POIType>());
            comboPoiStatus.Items.AddRange(Enum.GetNames<SitePoiStatus>());
            plotter.formEditMap = this;

            Util.useLastLocation(this, Game.settings.formMapEditor);
        }

        private void FormEditMap_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (plotter != null)
            {
                plotter.formEditMap = null;
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
            var status = siteData.poiStatus.ContainsKey(poi.name) ? siteData.poiStatus[poi.name] : SitePoiStatus.unknown;
            var subItems = new ListViewItem.ListViewSubItem[]
            {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = poi.name },
                    new ListViewItem.ListViewSubItem { Text = poi.dist.ToString() },
                    new ListViewItem.ListViewSubItem { Text = poi.angle.ToString() },
                    new ListViewItem.ListViewSubItem { Text = poi.type.ToString() },
                    new ListViewItem.ListViewSubItem { Text = poi.rot.ToString() },
                    new ListViewItem.ListViewSubItem { Text = status.ToString() },
            };

            var row = new ListViewItem(subItems, 0)
            {
                Name = poi.name,
                Tag = poi,
            };
            return row;
        }

        private void btnSaveEdits_Click(object sender, EventArgs e)
        {
            // save stuff to a clone template file
            SiteTemplate.SaveEdits();
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

        public void setCurrentPoi(SitePOI newPoi)
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
                var listedPoi = listPoi.Items.Find(newPoi.name, false);
                if (listedPoi != null && listedPoi.Length > 0 && !listedPoi[0].Selected)
                {
                    listPoi.SelectedItems.Clear();
                    listedPoi[0].Selected = true;
                    listedPoi[0].EnsureVisible();
                }
                else
                {
                    Game.log("setCurrentPoi not found in grid?");
                }
            }

            plotter.forcePoi = newPoi;
            plotter.Invalidate();
        }

        private void comboPoiType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked) return;

            Enum.TryParse<POIType>(comboPoiType.Text, out poi.type);
            numPoiRot.Enabled = poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk || poi.type == POIType.pylon || poi.type == POIType.component;
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void txtPoiName_TextChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.name = txtPoiName.Text;
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void numPoiDist_ValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.dist = (float)numPoiDist.Value;
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void numPoiAngle_ValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.angle = (float)numPoiAngle.Value;
            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void numPoiRot_ValueChanged(object sender, EventArgs e)
        {
            if (!checkApplyPoiLive.Checked || poi == null) return;
            poi.rot = (float)numPoiRot.Value;
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

            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void btnPoiApply_Click(object sender, EventArgs e)
        {
            if (poi == null) return;

            poi.name = txtPoiName.Text;
            poi.dist = (float)numPoiDist.Value;
            poi.angle = (float)numPoiAngle.Value;
            Enum.TryParse<POIType>(comboPoiType.Text, out poi.type);
            poi.rot = (float)numPoiRot.Value;

            Enum.TryParse<SitePoiStatus>(comboPoiStatus.Text, out var newStatus);
            siteData.poiStatus[poi.name] = newStatus;
            if (newStatus == SitePoiStatus.unknown)
                siteData.poiStatus.Remove(poi.name);

            plotter.forcePoi = poi;
            plotter.Invalidate();
        }

        private void checkPoiPrecision_CheckedChanged(object sender, EventArgs e)
        {
            var stepValue = checkPoiPrecision.Checked ? 0.1m : 1m;
            numPoiDist.Increment = stepValue;
            numPoiAngle.Increment = stepValue;
            numPoiRot.Increment = stepValue;
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
                setCurrentPoi(newPoi);
            }
        }

        private void btnFocusGame_Click(object sender, EventArgs e)
        {
            Elite.setFocusED();
        }

        private void btnNewPoi_Click(object sender, EventArgs e)
        {
            var dist = ((double)Util.getDistance(Status.here, siteData.location, (decimal)game.nearBody!.radius));
            var angle = ((float)new Angle((Util.getBearing(Status.here, siteData.location) - siteData.siteHeading)));
            var rot = (int)new Angle(game.status.Heading - this.siteData.siteHeading);

            var newPoi = new SitePOI()
            {
                name = $"{(int)DateTime.Now.TimeOfDay.TotalSeconds}",
                dist = (float)dist,
                angle = (float)angle,
                type = POIType.unknown,
                rot = rot,
            };
            // add to the template and ListView
            template.poi.Add(newPoi);
            var row = createListViewItemForPoi(newPoi);
            this.listPoi.Items.Add(row);

            // make it current item and redraw the plotter
            setCurrentPoi(newPoi);
        }

        private void checkHighlightAll_CheckedChanged(object sender, EventArgs e)
        {
            plotter.Invalidate();
        }

        private void btnRemovePoi_Click(object sender, EventArgs e)
        {
            if (listPoi.SelectedItems.Count > 0)
            {
                var lvi = listPoi.SelectedItems[0];
                listPoi.Items.Remove(lvi);
                template.poi.Remove(lvi.Tag as SitePOI);
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
