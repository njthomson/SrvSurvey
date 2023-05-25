using SrvSurvey.canonn;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SrvSurvey
{
    internal partial class FormAllRuins : Form
    {
        private static FormAllRuins? activeForm;

        private double[] here;

        public static void show()
        {
            if (activeForm == null)
                FormAllRuins.activeForm = new FormAllRuins();

            if (FormAllRuins.activeForm.Visible == false)
                FormAllRuins.activeForm.Show();
            else
                FormAllRuins.activeForm.Activate();
        }

        private List<ListViewItem> rows = new List<ListViewItem>();
        private int sortColumn;
        private bool sortUp = false;

        public FormAllRuins()
        {
            InitializeComponent();
            comboSiteType.SelectedIndex = 0;

            // default sort by system distance
            this.sortColumn = 3;


            // can we fit in our last location
            if (Game.settings.logsLocation != Rectangle.Empty)
                Util.useLastLocation(this, Game.settings.allRuinsLocation);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            FormAllRuins.activeForm = null;
        }

        private void FormAllRuins_Load(object sender, EventArgs e)
        {
            if (Game.activeGame?.cmdr != null)
            {
                lblCurrentSystem.Text = Game.activeGame.cmdr.currentSystem;
                here = Game.activeGame.cmdr.starPos;
            }
            else
            {
                lblCurrentSystem.Text = "Sol (current unknown)";
                here = new double[3];
            }

            this.prepareAllRuins();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.allRuinsLocation != rect)
            {
                Game.settings.allRuinsLocation = rect;
                Game.settings.Save();
            }
        }

        private void prepareAllRuins()
        {
            Game.log("prepareAllRuins");

            var allRuins = Game.canonn.loadAllRuins();

            Game.log($"Rendering {allRuins.Count} ruins");

            foreach (var entry in allRuins)
            {
                var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;
                var siteHeading = entry.siteHeading == -1 ? "" : $"{entry.siteHeading}°";
                var relicTowerHeading = entry.relicTowerHeading == -1 ? "" : $"{entry.relicTowerHeading}°";

                // confirm there are images specific to this Ruins
                var hasImages = ruinsHasImages(entry);

                var distanceToArrival = entry.distanceToArrival.ToString("N0");

                entry.systemDistance = Util.getSystemDistance(here, entry.starPos);
                var distanceToSystem = entry.systemDistance.ToString("N0");

                var siteID = entry.siteID == -1 ? "?" : entry.siteID.ToString();
                var row = new ListViewItem(siteID);
                row.Tag = entry;

                // ordering here needs to manually match columns
                row.SubItems.Add(entry.systemName);
                row.SubItems.Add(entry.bodyName);
                row.SubItems.Add($"{distanceToSystem} ly");
                row.SubItems.Add($"{distanceToArrival} ls");
                row.SubItems.Add(entry.siteType);
                row.SubItems.Add(entry.idx > 0 ? $"#{entry.idx}" : "");
                row.SubItems.Add(lastVisited);
                row.SubItems.Add(hasImages ? "yes" : "");
                row.SubItems.Add(siteHeading);
                row.SubItems.Add(relicTowerHeading);

                this.rows.Add(row);
            }

            Program.control!.Invoke((MethodInvoker)delegate
            {
                this.showAllRows();
            });
        }

        private bool ruinsHasImages(GuardianRuinEntry entry)
        {
            var imageFilenamePrefix = $"{entry.systemName.ToUpper()} {entry.bodyName}".ToUpperInvariant();
            var imageFilenameSuffix = $", Ruins{entry.idx}".ToUpperInvariant();

            var folder = Path.Combine(Game.settings.screenshotTargetFolder!, entry.systemName);
            if (!Directory.Exists(folder)) return false;

            var files = Directory.GetFiles(folder, "*.png");
            var match = files.FirstOrDefault(_ => _.ToUpperInvariant().Contains(imageFilenamePrefix) && _.ToUpperInvariant().Contains(imageFilenameSuffix));
            return match != null;
        }

        private void showAllRows()
        {
            this.grid.Items.Clear();

            // apply filter
            var filteredRows = this.rows.Where(row =>
            {
                var entry = (GuardianRuinEntry)row.Tag;

                if (checkVisited.Checked && entry.lastVisited == DateTimeOffset.MinValue)
                    return false;

                if (comboSiteType.SelectedIndex != 0 && comboSiteType.Text != entry.siteType)
                    return false;

                if (!string.IsNullOrEmpty(txtFilter.Text) && !entry.systemName.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            });

            // apply sort
            var sortedRows = this.sortRows(filteredRows);

            if (this.sortUp)
                sortedRows = sortedRows.Reverse();

            this.grid.Items.AddRange(sortedRows.ToArray());
            this.grid.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private IEnumerable<ListViewItem> sortRows(IEnumerable<ListViewItem> rows)
        {
            switch (this.sortColumn)
            {
                case 0: // siteID
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).siteID);
                case 1: // system name
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).systemName);
                case 2: // bodyName
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).bodyName);
                case 3: //systemDistance
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).systemDistance);
                case 4: // distanceToArrival;
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).distanceToArrival);
                case 5: // site type
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).siteType);
                case 6: // Ruins #
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).idx);
                case 7: // last visited
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).lastVisited);
                case 8: // has images
                    return rows.OrderBy(row => row.SubItems[8].Text);
                case 9: // site heading
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).siteHeading);
                case 10: // relic tower heading
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).relicTowerHeading);

                default:
                    Game.log($"Unexpected sort column: {this.sortColumn}");
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).systemName);
            }

        }

        private void checkVisited_CheckedChanged(object sender, EventArgs e)
        {
            this.showAllRows();
        }

        private void comboSiteType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.showAllRows();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            this.showAllRows();
        }

        private void grid_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (this.sortColumn == e.Column)
            {
                this.sortUp = !this.sortUp;
            }
            else
            {
                this.sortColumn = e.Column;
            }

            this.showAllRows();
        }

        private void grid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && grid.SelectedItems.Count > 0)
            {
                Clipboard.SetText(grid.SelectedItems[0].SubItems[1].Text);
            }
        }
    }
}
