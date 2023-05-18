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

        public FormAllRuins()
        {
            InitializeComponent();
            comboSiteType.SelectedIndex = 0;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            FormAllRuins.activeForm = null;
        }

        private void FormAllRuins_Load(object sender, EventArgs e)
        {
            Game.log("FormAllRuins_Load");

            Game.canonn.loadAllRuins().ContinueWith(stuff =>
            {
                Game.log($"Rendering {stuff.Result.Count} ruins");
                var here = new double[]
                {
                    838.75,
                    -197.84375,
                    -111.84375,
                };

                foreach (var entry in stuff.Result)
                {
                    var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;
                    var siteHeading = entry.siteHeading == -1 ? "" : $"{entry.siteHeading}°";
                    var relicTowerHeading = entry.relicTowerHeading == -1 ? "" : $"{entry.relicTowerHeading}°";
                    var hasImages = Directory.Exists(Path.Combine(Game.settings.screenshotTargetFolder!, entry.systemName));

                    var distanceToArrival = entry.distanceToArrival.ToString("N0");

                    entry.systemDistance = Util.getSystemDistance(here, entry.starPos);
                    var distanceToSystem = entry.systemDistance.ToString("N0");

                    var row = new ListViewItem(entry.systemName);
                    row.Tag = entry;

                    // ordering here needs to manually match columns
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
                    this.addRows();
                });
            });
        }

        private void addRows()
        {
            this.grid.Items.Clear();

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



            var sortedRows = this.sortRows(filteredRows);

            this.grid.Items.AddRange(sortedRows.ToArray());
            this.grid.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private IEnumerable<ListViewItem> sortRows(IEnumerable<ListViewItem> rows)
        {
            switch (this.sortColumn)
            {
                case 0: // system name
                default:
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).bodyName);
                case 1: // bodyName
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).bodyName);
                case 2: //systemDistance
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).systemDistance);
                case 3: // distanceToArrival;
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).distanceToArrival);
                case 4: // site type
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).siteType);
                case 5: // last visited
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).lastVisited);
            }

        }

        private void checkVisited_CheckedChanged(object sender, EventArgs e)
        {
            this.addRows();
        }

        private void comboSiteType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.addRows();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            this.addRows();
        }

        private void grid_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (this.sortColumn == e.Column)
                this.sortColumn = -1;
            else
                this.sortColumn = e.Column;

            this.addRows();
        }

        private void grid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && grid.SelectedItems.Count > 0)
            {
                Clipboard.SetText(grid.SelectedItems[0].Text);
            }
        }
    }
}
