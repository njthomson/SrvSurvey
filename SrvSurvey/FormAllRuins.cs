using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using System.ComponentModel;
using System.Data;

namespace SrvSurvey
{
    internal partial class FormAllRuins : Form
    {
        public static FormAllRuins? activeForm;

        private double[] currentSystem;

        public static void show()
        {
            if (activeForm == null)
                FormAllRuins.activeForm = new FormAllRuins();

            Util.showForm(FormAllRuins.activeForm);
        }

        private List<ListViewItem> rows = new List<ListViewItem>();
        // default sort by system distance
        private int sortColumn = 3;
        private bool sortUp = false;

        private readonly LookupStarSystem starSystemLookup;

        public FormAllRuins()
        {
            InitializeComponent();
            comboSiteType.SelectedIndex = 0;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formAllRuinsLocation);

            this.starSystemLookup = new LookupStarSystem(comboCurrentSystem);
            this.starSystemLookup.onSystemMatch += StarSystemLookup_starSystemMatch;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormAllRuins.activeForm = null;
        }

        private void FormAllRuins_Load(object sender, EventArgs e)
        {
            var star = Util.getRecentStarSystem();
            comboCurrentSystem.Text = star.systemName;
            currentSystem = star.pos;

            this.prepareAllRows();
            this.grid.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            // do we have any lat/long's worth sharing?
            btnShare.Visible = this.getLatLongsToShare().Count > 0;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formAllRuinsLocation != rect)
            {
                Game.settings.formAllRuinsLocation = rect;
                Game.settings.Save();
            }
        }

        public void StarSystemLookup_starSystemMatch(net.EDSM.StarSystem? match)
        {
            if (match == null)
            {
                comboCurrentSystem.Text = "Sol";
                currentSystem = new double[3] { 0, 0, 0 };
            }
            else
            {
                // update currentSystem
                this.currentSystem = match.coords.starPos;
            }

            // and recalculate distances
            calculateDistances();

            showAllRows();
        }

        private void calculateDistances()
        {
            foreach (var row in this.rows)
            {
                var entry = (GuardianRuinEntry)row.Tag;
                entry.systemDistance = Util.getSystemDistance(currentSystem, entry.starPos);
                row.SubItems["distanceToSystem"]!.Text = entry.systemDistance.ToString("N0") + " ly";
            }
        }

        private void prepareAllRows()
        {
            var allRuins = Game.canonn.loadAllRuins();

            Game.log($"Rendering {allRuins.Count} ruins.");

            foreach (var entry in allRuins)
            {
                var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;

                // confirm there are images specific to this Ruins
                var hasImages = ruinsHasImages(entry);

                var distanceToArrival = entry.distanceToArrival.ToString("N0");

                entry.systemDistance = Util.getSystemDistance(currentSystem, entry.starPos);
                var distanceToSystem = entry.systemDistance.ToString("N0");

                var siteID = entry.siteID == -1 ? "?" : $"GR" + entry.siteID.ToString("000");

                var notes = entry.notes ?? "";
                if (entry.missingLiveLatLong)
                    notes = "(missing live lat/long co-ordinates) " + notes;

                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = siteID },
                    new ListViewItem.ListViewSubItem { Text = entry.systemName, Name = "systemName" },
                    new ListViewItem.ListViewSubItem { Text = entry.bodyName },
                    new ListViewItem.ListViewSubItem { Text = "x ly", Name = "distanceToSystem" },
                    new ListViewItem.ListViewSubItem { Text = $"{distanceToArrival} ls" },
                    new ListViewItem.ListViewSubItem { Text = entry.siteType },
                    new ListViewItem.ListViewSubItem { Text = entry.idx > 0 ? $"#{entry.idx}" : "" },
                    new ListViewItem.ListViewSubItem { Text = lastVisited },
                    new ListViewItem.ListViewSubItem { Text = hasImages ? "yes" : "" },
                    new ListViewItem.ListViewSubItem { Text = entry.surveyComplete ? "yes" : "" },
                    new ListViewItem.ListViewSubItem { Text = notes },
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

                if (!string.IsNullOrEmpty(txtFilter.Text))
                {
                    // if the system name, bodyName or Notes contains or systemAddress ...
                    return entry.fullBodyNameWithIdx.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)
                        || row.SubItems[row.SubItems.Count - 1].Text.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)
                        || entry.systemAddress.ToString() == txtFilter.Text;
                }

                return true;
            });

            // apply sort
            var sortedRows = this.sortRows(filteredRows);

            if (this.sortUp)
                sortedRows = sortedRows.Reverse();

            this.grid.Items.AddRange(sortedRows.ToArray());
            this.lblStatus.Text = $"{this.grid.Items.Count} rows. Right click a row to copy the system name to the clipboard. Double click a row to open that specific ruins map.";
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
                case 9: // survey complete
                    return rows.OrderBy(row => row.SubItems[9].Text);
                case 10: // notes
                    return rows.OrderBy(row => ((GuardianRuinEntry)row.Tag).notes);

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
                this.sortUp = !this.sortUp;
            else
                this.sortColumn = e.Column;

            this.showAllRows();
        }

        private void grid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && grid.SelectedItems.Count > 0)
            {
                Clipboard.SetText(grid.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void grid_DoubleClick(object sender, EventArgs e)
        {
            // open the form for this row?
            if (grid.SelectedItems.Count > 0)
            {
                var entry = (GuardianRuinEntry)grid.SelectedItems[0].Tag;
                var siteData = GuardianSiteData.Load($"{entry.systemName} {entry.bodyName}", entry.idx, true);
                FormRuins.show(siteData);
            }
        }

        private List<string> getLatLongsToShare()
        {
            var lines = new List<string>();
            foreach (ListViewItem item in this.grid.Items)
            {
                var entry = (GuardianRuinEntry)item.Tag;

                if (entry.notes.Contains(GuardianRuinEntry.PleaseShareMessage))
                    lines.Add($"{entry.systemName}, {entry.bodyName}, #{entry.idx} - {entry.siteType}\r\n  \"latitude\": {entry.latitude},\r\n  \"longitude\": {entry.longitude},\r\n  \"siteHeading\": {entry.siteHeading},\r\n  \"relicTowerHeading\": {entry.relicTowerHeading},\r\n");
            }

            return lines;
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            var lines = getLatLongsToShare();
            var rslt = MessageBox.Show(this, $"You have visited {lines.Count} ruins with missing data in public records. Are you willing to share your discoveries?\r\n\r\nClick Yes to have put them in your clipboard.\r\n\r\nPlease share them with grinning2000 on Discord, thank you.", "Share discovered data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.Yes)
            {
                Clipboard.SetText($"Discovered data for {lines.Count} Guardian Ruins:\r\n\r\n" + string.Join("\r\n", lines));
            }
        }
    }
}
