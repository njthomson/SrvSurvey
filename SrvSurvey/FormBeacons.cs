using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.net.EDSM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    public partial class FormBeacons : Form
    {
        public static FormBeacons? activeForm;

        private canonn.StarSystem star;

        public static void show()
        {
            if (activeForm == null)
                FormBeacons.activeForm = new FormBeacons();

            Util.showForm(FormBeacons.activeForm);
        }

        private List<ListViewItem> rows = new List<ListViewItem>();
        // default sort by system distance
        private int sortColumn = 2;
        private bool sortUp = false;

        private readonly LookupStarSystem starSystemLookup;

        public FormBeacons()
        {
            InitializeComponent();

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formBeaconsLocation);

            this.starSystemLookup = new LookupStarSystem(comboCurrentSystem);
            this.starSystemLookup.onSystemMatch += StarSystemLookup_starSystemMatch;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormBeacons.activeForm = null;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formBeaconsLocation != rect)
            {
                Game.settings.formBeaconsLocation = rect;
                Game.settings.Save();
            }
        }

        private void FormBeacons_Load(object sender, EventArgs e)
        {
            this.star = Util.getRecentStarSystem();
            comboCurrentSystem.Text = star.systemName;

            this.prepareAllRows();
            this.grid.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void prepareAllRows()
        {
            var allBeacons = Game.canonn.loadAllBeacons();
            var allStructures = Game.canonn.loadAllStructures();

            Game.log($"Rendering {Game.canonn.allBeacons.Count} beacons, {Game.canonn.allStructures.Count} structures");

            foreach (var entry in allBeacons)
            {
                var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;
                var distanceToArrival = entry.distanceToArrival.ToString("N0");

                entry.systemDistance = Util.getSystemDistance(this.star.pos, entry.starPos);
                var distanceToSystem = entry.systemDistance.ToString("N0");

                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = entry.systemName, Name = "systemName" },
                    new ListViewItem.ListViewSubItem { Text = entry.bodyName },
                    new ListViewItem.ListViewSubItem { Text = "x ly", Name = "distanceToSystem" },
                    new ListViewItem.ListViewSubItem { Text = $"{distanceToArrival} ls" },
                    new ListViewItem.ListViewSubItem { Text = lastVisited },
                    new ListViewItem.ListViewSubItem { Text = entry.siteType },
                    new ListViewItem.ListViewSubItem { Text = entry.notes ?? "" },
                };

                var row = new ListViewItem(subItems, 0) { Tag = entry, };
                this.rows.Add(row);
            }

            foreach (var entry in allStructures)
            {
                var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;
                var distanceToArrival = entry.distanceToArrival.ToString("N0");

                entry.systemDistance = Util.getSystemDistance(this.star.pos, entry.starPos);
                var distanceToSystem = entry.systemDistance.ToString("N0");

                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = entry.systemName, Name = "systemName" },
                    new ListViewItem.ListViewSubItem { Text = entry.bodyName },
                    new ListViewItem.ListViewSubItem { Text = "x ly", Name = "distanceToSystem" },
                    new ListViewItem.ListViewSubItem { Text = $"{distanceToArrival} ls" },
                    new ListViewItem.ListViewSubItem { Text = lastVisited },
                    new ListViewItem.ListViewSubItem { Text = entry.siteType },
                    new ListViewItem.ListViewSubItem { Text = entry.notes ?? "" },
                };

                var row = new ListViewItem(subItems, 0) { Tag = entry, };
                this.rows.Add(row);
            }

            calculateDistances();

            Program.control!.Invoke((MethodInvoker)delegate
            {
                this.showAllRows();
            });
        }

        private void calculateDistances()
        {
            foreach (var row in this.rows)
            {
                var entry = (GuardianGridEntry)row.Tag;
                entry.systemDistance = Util.getSystemDistance(this.star.pos, entry.starPos);
                row.SubItems["distanceToSystem"]!.Text = entry.systemDistance.ToString("N0") + " ly";
            }
        }

        private void showAllRows()
        {
            this.grid.Items.Clear();

            // apply filter
            var filteredRows = this.rows.Where(row =>
            {
                var entry = (GuardianGridEntry)row.Tag;

                if (checkVisited.Checked && entry.lastVisited == DateTimeOffset.MinValue)
                    return false;

                if (!string.IsNullOrEmpty(txtFilter.Text))
                {
                    // if the system name, bodyName or Notes contains or systemAddress ...
                    return entry.systemName.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)
                        || row.SubItems[row.SubItems.Count - 1].Text.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)
                        || row.SubItems[5].Text.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)
                        || entry.systemAddress.ToString() == txtFilter.Text;
                }

                return true;
            });

            // apply sort
            var sortedRows = this.sortRows(filteredRows);

            if (this.sortUp)
                sortedRows = sortedRows.Reverse();

            this.grid.Items.AddRange(sortedRows.ToArray());
            this.lblStatus.Text = $"{this.grid.Items.Count} rows.";
        }

        private IEnumerable<ListViewItem> sortRows(IEnumerable<ListViewItem> rows)
        {
            switch (this.sortColumn)
            {
                case 0: // system name
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).systemName);
                case 1: // bodyName
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).bodyName);
                case 2: //systemDistance
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).systemDistance);
                case 3: // distanceToArrival;
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).distanceToArrival);
                case 4: // last visited
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).lastVisited);
                case 5: // relatedStructure
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).siteType);
                case 6: // notes
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).notes);

                default:
                    Game.log($"Unexpected sort column: {this.sortColumn}");
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).systemName);
            }
        }

        internal void StarSystemLookup_starSystemMatch(net.EDSM.StarSystem? match)
        {
            if (match == null)
            {
                this.star = canonn.StarSystem.Sol;
            }
            else
            {
                // update currentSystem
                this.star = new canonn.StarSystem
                {
                    systemName = match.name,
                    pos = match.coords.starPos
                };
            }

            // and recalculate distances
            calculateDistances();

            showAllRows();
        }

        private void grid_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (this.sortColumn == e.Column)
                this.sortUp = !this.sortUp;
            else
                this.sortColumn = e.Column;

            this.showAllRows();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            this.showAllRows();
        }

        private void grid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && grid.SelectedItems.Count > 0)
            {
                Clipboard.SetText(grid.SelectedItems[0].SubItems[0].Text);
            }
        }
    }
}
