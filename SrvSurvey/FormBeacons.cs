using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using static SrvSurvey.game.GuardianSiteData;

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
        private int sortColumn = 3;
        private bool sortUp = false;

        private readonly LookupStarSystem starSystemLookup;
        private bool updatingTree = false;

        public FormBeacons()
        {
            InitializeComponent();
            this.treeSiteTypes.Nodes[2].Expand(); // ruins
            this.treeSiteTypes.Nodes[3].Expand(); // structures
            this.comboVisited.SelectedIndex = 0;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formBeaconsLocation);

            this.starSystemLookup = new LookupStarSystem(comboCurrentSystem);
            this.starSystemLookup.onSystemMatch += StarSystemLookup_starSystemMatch;

            // hide this from everyone else
            menuOpenDataFile.Visible = Debugger.IsAttached;
            menuOpenPubData.Visible = Debugger.IsAttached;
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
            this.grid.Columns[10].Width = 0;
            this.star = Util.getRecentStarSystem();
            comboCurrentSystem.Text = star.systemName;

            this.BeginInvoke(() =>
            {
                this.beginPrepareAllRows().ContinueWith((rslt) =>
                {
                    // no-op
                });
            });
        }

        private Task beginPrepareAllRows()
        {
            // ignore events whilst checkbox is disabled
            if (!checkRamTah.Enabled) return Task.CompletedTask;
            checkRamTah.Enabled = false;

            var incRamTahLogs = checkRamTah.Checked;
            // the first load of Ram Tah data can be slow and needs to be async
            checkRamTah.ThreeState = true;
            checkRamTah.CheckState = CheckState.Indeterminate;

            List<GuardianGridEntry> allSites = new List<GuardianGridEntry>();
            return Task.Run(() =>
            {
                try
                {
                    // reload all Ruins, optionally including Ram Tah logs
                    allSites.AddRange(Game.canonn.loadAllStructures(incRamTahLogs));
                    allSites.AddRange(Game.canonn.loadAllRuins(incRamTahLogs));

                    this.Invoke(() =>
                    {
                        this.endPrepareAllRows(allSites);

                        checkRamTah.ThreeState = false;
                        this.grid.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        if (!incRamTahLogs) this.grid.Columns[10].Width = 0;

                        this.BeginInvoke(() =>
                        {
                            checkRamTah.CheckState = incRamTahLogs ? CheckState.Checked : CheckState.Unchecked;
                            checkRamTah.Enabled = true;
                        });

                    });
                }
                catch (Exception ex)
                {
                    Game.log($"beginPrepareAllRows: error: {ex}");
                    FormErrorSubmit.Show(ex);
                }
            });
        }

        private void endPrepareAllRows(List<GuardianGridEntry> allSites)
        {
            this.grid.SuspendLayout();
            this.grid.BeginUpdate();
            var allBeacons = Game.canonn.loadAllBeacons();

            Game.log($"Rendering {Game.canonn.allBeacons.Count} beacons, {Game.canonn.allStructures.Count} structures");
            this.rows.Clear();
            var countNewData = 0;

            foreach (var entry in allBeacons)
            {
                var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;
                var distanceToArrival = entry.distanceToArrival.ToString("N0");

                entry.systemDistance = Util.getSystemDistance(this.star.pos, entry.starPos);
                var distanceToSystem = entry.systemDistance.ToString("N0");

                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = $"GB {entry.siteID}" },
                    new ListViewItem.ListViewSubItem { Text = entry.systemName, Name = "systemName" },
                    new ListViewItem.ListViewSubItem { Text = entry.bodyName },
                    new ListViewItem.ListViewSubItem { Text = "x ly", Name = "distanceToSystem" },
                    new ListViewItem.ListViewSubItem { Text = $"{distanceToArrival} ls" },
                    new ListViewItem.ListViewSubItem { Text = lastVisited },
                    new ListViewItem.ListViewSubItem { Text = entry.siteType },
                    new ListViewItem.ListViewSubItem { Text = "" }, // idx
                    new ListViewItem.ListViewSubItem { Text = "" }, // hasImages
                    new ListViewItem.ListViewSubItem { Text = "" }, // surveyComplete
                    new ListViewItem.ListViewSubItem { Text = "" }, // ramTahLogs
                    new ListViewItem.ListViewSubItem { Text = entry.notes ?? "" },
                };

                var row = new ListViewItem(subItems, 0) { Tag = entry, };
                this.rows.Add(row);
            }

            foreach (var entry in allSites)
            {
                var lastVisited = entry.lastVisited == DateTimeOffset.MinValue ? "" : entry.lastVisited.ToString("d")!;
                var distanceToArrival = entry.distanceToArrival.ToString("N0");

                entry.systemDistance = Util.getSystemDistance(this.star.pos, entry.starPos);
                var distanceToSystem = entry.systemDistance.ToString("N0");

                var hasImages = siteHasImages(entry);

                var surveyComplete = entry.surveyComplete ? "complete" : "";
                if (entry.surveyProgress > 0)
                    surveyComplete = entry.surveyProgress.ToString("0") + "%";

                if (entry.hasDiscoveredData)
                {
                    countNewData++;
                    surveyComplete += " **"; // discovered data";
                }

                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = entry.isRuins ?  $"GR {entry.siteID}" : $"GS {entry.siteID}" },
                    new ListViewItem.ListViewSubItem { Text = entry.systemName, Name = "systemName" },
                    new ListViewItem.ListViewSubItem { Text = entry.bodyName },
                    new ListViewItem.ListViewSubItem { Text = "x ly", Name = "distanceToSystem" },
                    new ListViewItem.ListViewSubItem { Text = $"{distanceToArrival} ls" },
                    new ListViewItem.ListViewSubItem { Text = lastVisited },
                    new ListViewItem.ListViewSubItem { Text = entry.siteType },
                    new ListViewItem.ListViewSubItem { Text = entry.idx > 0 ? $"#{entry.idx}" : "" },
                    new ListViewItem.ListViewSubItem { Text = hasImages ? "yes" : "" },
                    new ListViewItem.ListViewSubItem { Text = surveyComplete },
                    new ListViewItem.ListViewSubItem { Text = entry.ramTahLogs },
                    new ListViewItem.ListViewSubItem { Text = entry.notes ?? "" },
                };

                var row = new ListViewItem(subItems, 0) { Tag = entry, };
                this.rows.Add(row);
            }

            calculateDistances();

            this.showAllRows();

            this.grid.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            if (!checkRamTah.Checked) this.grid.Columns[10].Width = 0;

            this.BeginInvoke(() =>
            {
                this.grid.EndUpdate();
                this.grid.ResumeLayout();

                if (countNewData > 0 && !string.IsNullOrEmpty(Game.settings.lastFid))
                    btnShare.Visible = true;
            });
        }

        private void calculateDistances()
        {
            foreach (var row in this.rows)
            {
                var entry = (GuardianGridEntry)row.Tag;
                entry.systemDistance = Util.getSystemDistance(this.star.pos, entry.starPos);
                row.SubItems["distanceToSystem"]!.Text = entry.systemDistance.ToString("N0") + " ly";

                // TODO: highlight the row that is the current target destination
                //if (entry.systemAddress == Game.activeGame?.navRoute.Route.LastOrDefault()?.SystemAddress)
                //    row.Font = new Font(grid.Font, FontStyle.Bold);
                //else
                //    row.Font = grid.Font;
            }
        }

        private void showAllRows()
        {
            this.grid.Items.Clear();
            var countVisited = 0;
            var countSurveyed = 0;

            // apply filter
            var filteredRows = this.rows.Where(row =>
            {
                var entry = (GuardianGridEntry)row.Tag;
                var keep = true;

                // if only visited
                if (comboVisited.SelectedIndex == 1 && entry.lastVisited == DateTimeOffset.MinValue) keep = false;
                // if only un visited
                if (comboVisited.SelectedIndex == 2 && entry.lastVisited != DateTimeOffset.MinValue) keep = false;

                // site type
                if (entry.siteType == "Beacon" && !treeSiteTypes.Nodes[1].Checked) keep = false;

                if (entry.siteType == "Alpha" && !treeSiteTypes.Nodes[2].Nodes[0].Checked) keep = false;
                if (entry.siteType == "Beta" && !treeSiteTypes.Nodes[2].Nodes[1].Checked) keep = false;
                if (entry.siteType == "Gamma" && !treeSiteTypes.Nodes[2].Nodes[2].Checked) keep = false;

                if (entry.siteType == "Lacrosse" && !treeSiteTypes.Nodes[3].Nodes[0].Checked) keep = false;
                if (entry.siteType == "Crossroads" && !treeSiteTypes.Nodes[3].Nodes[1].Checked) keep = false;
                if (entry.siteType == "Fistbump" && !treeSiteTypes.Nodes[3].Nodes[2].Checked) keep = false;
                if (entry.siteType == "Hammerbot" && !treeSiteTypes.Nodes[3].Nodes[3].Checked) keep = false;
                if (entry.siteType == "Bear" && !treeSiteTypes.Nodes[3].Nodes[4].Checked) keep = false;
                if (entry.siteType == "Bowl" && !treeSiteTypes.Nodes[3].Nodes[5].Checked) keep = false;
                if (entry.siteType == "Turtle" && !treeSiteTypes.Nodes[3].Nodes[6].Checked) keep = false;
                if (entry.siteType == "Robolobster" && !treeSiteTypes.Nodes[3].Nodes[7].Checked) keep = false;
                if (entry.siteType == "Squid" && !treeSiteTypes.Nodes[3].Nodes[8].Checked) keep = false;
                if (entry.siteType == "Stickyhand" && !treeSiteTypes.Nodes[3].Nodes[9].Checked) keep = false;


                if (keep == true && !string.IsNullOrWhiteSpace(txtFilter.Text))
                {
                    // if the system name, bodyName or Notes contains or systemAddress ...
                    keep &= entry.systemName.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase) // system
                        || row.SubItems[row.SubItems.Count - 1].Text.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase) // notes
                        || row.SubItems[6].Text.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase) // site type
                        || row.SubItems[0].Text.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase) // site Canonn ID
                        || entry.systemAddress.ToString().Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase) // numeric system address
                        || (checkRamTah.Checked && entry.ramTahLogs.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)); // Ram Tah logs
                }

                if (keep == true)
                {
                    if (entry.lastVisited != DateTimeOffset.MinValue) countVisited++;
                    if (entry.surveyComplete) countSurveyed++;
                }

                return keep;
            });

            // apply sort
            var sortedRows = this.sortRows(filteredRows);

            if (this.sortUp)
                sortedRows = sortedRows.Reverse();

            this.grid.Items.AddRange(sortedRows.ToArray());
            var rowCount = this.grid.Items.Count;
            var percentVisited = (100.0 / (float)rowCount * (float)countVisited).ToString("0");
            var percentSurveyed = (100.0 / (float)rowCount * (float)countSurveyed).ToString("0");
            this.lblStatus.Text = $"{rowCount} of {this.rows.Count} rows | visited: {countVisited} ({percentVisited}%) | surveys complete: {countSurveyed} ({percentSurveyed}%)";
        }

        private bool siteHasImages(GuardianGridEntry entry)
        {
            var imageFilenamePrefix = $"{entry.systemName.ToUpper()} {entry.bodyName}".ToUpperInvariant();
            var imageFilenameSuffix = entry.isRuins ? $", Ruins{entry.idx}".ToUpperInvariant() : entry.siteType;

            var folder = Path.Combine(Game.settings.screenshotTargetFolder!, entry.systemName);
            if (!Directory.Exists(folder)) return false;

            var files = Directory.GetFiles(folder, "*.png").Select(_ => Path.GetFileNameWithoutExtension(_));
            var hasImages = files.Any(_ => _.StartsWith(imageFilenamePrefix, StringComparison.OrdinalIgnoreCase) && _.Contains(imageFilenameSuffix, StringComparison.OrdinalIgnoreCase));
            return hasImages;
        }

        private IEnumerable<ListViewItem> sortRows(IEnumerable<ListViewItem> rows)
        {
            switch (this.sortColumn)
            {
                case 0: // site ID
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).siteID);
                case 1: // system name
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).systemName);
                case 2: // bodyName
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).bodyName);
                case 3: //systemDistance
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).systemDistance);
                case 4: // distanceToArrival;
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).distanceToArrival);
                case 5: // last visited
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).lastVisited);
                case 6: // siteType
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).siteType);
                case 7: // index
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).idx);
                case 8: // has images
                    return rows.OrderBy(row => row.SubItems[8].Text);
                case 9: // survey complete
                    return rows.OrderBy(row =>
                    {
                        var entry = (GuardianGridEntry)row.Tag;
                        if (entry.surveyComplete) return 100;
                        else if (entry.surveyStatus == null) return 0;
                        else return entry.surveyStatus.progress;
                    });
                case 10: // Ram Tah
                    return rows.OrderBy(row => ((GuardianGridEntry)row.Tag).ramTahLogs);
                case 11: // notes
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
                    pos = match.coords,
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
                // show right-click context menu, when clicking on some item
                var item = this.grid.GetItemAt(e.X, e.Y)!;
                var entry = item.Tag as GuardianGridEntry;
                if (entry == null) return;

                menuOpenSiteSurvey.Enabled = entry.siteType != "Beacon";
                menuOpenDataFile.Enabled = File.Exists(entry.filepath);
                notesToolStripMenuItem.Enabled = !string.IsNullOrEmpty(entry.notes);

                var folder = Path.Combine(Game.settings.screenshotTargetFolder!, entry.systemName);
                menuOpenImagesFolder.Enabled = Directory.Exists(folder);

                this.contextMenu.Show(this.grid, e.X, e.Y);
            }
        }

        private void checkRamTah_CheckedChanged(object sender, EventArgs e)
        {
            this.beginPrepareAllRows();
        }

        private void btnSiteTypes_Click(object sender, EventArgs e)
        {
            this.togglePanel(true);
        }

        private void btnSetSiteTypes_Click(object sender, EventArgs e)
        {
            this.togglePanel(false);
            showAllRows();
        }

        private void togglePanel(bool show)
        {
            panelSiteTypes.Visible = show;

            // disable everything else whilst tree view is up
            foreach (Control ctrl in this.Controls)
                if (ctrl != this.panelSiteTypes)
                    ctrl.Enabled = !show;
        }

        private void treeSiteTypes_AfterCheck(object sender, TreeViewEventArgs e)
        {
            this.toggleNode(e.Node);
        }

        private void treeSiteTypes_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.toggleNode(e.Node);
        }

        private void toggleNode(TreeNode? node)
        {
            if (node == null || updatingTree) return;

            this.BeginInvoke(() =>
            {
                try
                {
                    updatingTree = true;

                    if (node.Name == "nAllSites")
                    {
                        treeSiteTypes.Nodes[1].Checked = node.Checked;
                        treeSiteTypes.Nodes[2].Checked = node.Checked;
                        treeSiteTypes.Nodes[3].Checked = node.Checked;
                        foreach (TreeNode child in treeSiteTypes.Nodes[2].Nodes) child.Checked = node.Checked;
                        foreach (TreeNode child in treeSiteTypes.Nodes[3].Nodes) child.Checked = node.Checked;
                    }
                    else if (node.Name == "nAllRuins" || node.Name == "nAllStructures")
                    {
                        foreach (TreeNode child in node.Nodes) child.Checked = node.Checked;
                    }
                    else if (node.Parent?.Name == "nAllRuins" || node.Parent?.Name == "nAllStructures")
                    {
                        var allSiblings = node.Parent.Nodes.Cast<TreeNode>();
                        var allChecked = allSiblings.All(_ => _.Checked);
                        node.Parent.Checked = allChecked;
                    }

                    // check all node if top 3 are checked
                    treeSiteTypes.Nodes[0].Checked = treeSiteTypes.Nodes[1].Checked && treeSiteTypes.Nodes[2].Checked && treeSiteTypes.Nodes[3].Checked;

                    // summarize checked states
                    var txt = new StringBuilder();
                    if (treeSiteTypes.Nodes[0].Checked || treeSiteTypes.Nodes[2].Checked && treeSiteTypes.Nodes[3].Checked)
                        txt.Append("All Ruins, Structures");
                    else if (treeSiteTypes.Nodes[2].Checked)
                    {
                        txt.Append("All Ruins");
                        var others = string.Join(", ", treeSiteTypes.Nodes[3].Nodes.Cast<TreeNode>().Where(_ => _.Checked).Select(_ => _.Text));
                        if (!string.IsNullOrEmpty(others)) txt.Append(", " + others);
                    }
                    else if (treeSiteTypes.Nodes[3].Checked)
                    {
                        txt.Append("All Structures");
                        var others = string.Join(", ", treeSiteTypes.Nodes[2].Nodes.Cast<TreeNode>().Where(_ => _.Checked).Select(_ => _.Text));
                        if (!string.IsNullOrEmpty(others)) txt.Append(", " + others);
                    }
                    else
                    {
                        var types = new List<TreeNode>();
                        types.AddRange(treeSiteTypes.Nodes[2].Nodes.Cast<TreeNode>());
                        types.AddRange(treeSiteTypes.Nodes[3].Nodes.Cast<TreeNode>());
                        txt.Append(string.Join(", ", types.Where(_ => _.Checked).Select(_ => _.Text)));
                    }
                    if (treeSiteTypes.Nodes[1].Checked)
                    {
                        if (txt.Length > 0)
                            txt.Append(" and ");
                        txt.Append("Beacons");
                    }

                    txtSiteTypes.Text = txt.ToString();
                    btnSiteTypes.Enabled = !string.IsNullOrWhiteSpace(txtSiteTypes.Text);
                }
                finally
                {
                    updatingTree = false;
                }
            });
        }

        private void copySystemNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy system name
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            Clipboard.SetText(entry.systemName);
        }

        private void systemAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy system address
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            Clipboard.SetText(entry.systemAddress.ToString());
        }

        private void copyBodyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy body name
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            Clipboard.SetText(entry.fullBodyName);
        }

        private void copyStarPosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy star pos
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            Clipboard.SetText($"x: {entry.starPos[0]}, y: {entry.starPos[1]}, z: {entry.starPos[2]}");
        }

        private void copyLatlongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy lat/long
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            Clipboard.SetText($"{entry.latitude}, {entry.longitude}");
        }

        private void notesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy notes
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            if (!string.IsNullOrEmpty(entry.notes))
                Clipboard.SetText(entry.notes);
        }

        private void openSystemInEDSMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;

            // https://canonn-science.github.io/canonn-signals/?system=Synuefe%20EN-H%20d11-106
            Util.openLink($"https://canonn-science.github.io/canonn-signals/?system={Uri.EscapeDataString(entry.systemName)}");
        }

        private void menuOpenSiteSurvey_Click(object sender, EventArgs e)
        {
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            if (entry.siteType == "Beacon") return;

            var siteData = GuardianSiteData.Load($"{entry.systemName} {entry.bodyName}", entry.idx, entry.isRuins);

            if (siteData == null)
            {
                // contruct a new siteData from what we have here and it's pubData
                var siteType = Enum.Parse<SiteType>(entry.siteType);
                siteData = new GuardianSiteData()
                {
                    type = siteType,
                    index = entry.idx,
                    location = new units.LatLong2(entry.latitude, entry.longitude),
                    systemAddress = entry.systemAddress,
                    systemName = entry.systemName,
                    bodyId = entry.bodyId,
                    bodyName = $"{entry.systemName} {entry.bodyName}",
                    poiStatus = new Dictionary<string, SitePoiStatus>(),
                };
                siteData.loadPub();
            }

            FormRuins.show(siteData);
        }

        private void openDataFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;

            if (File.Exists(entry.filepath))
                Util.openLink(entry.filepath);
            else
                Game.log($"No filepath found for: {entry.fullBodyNameWithIdx}");
        }

        private void menuOpenPubData_Click(object sender, EventArgs e)
        {
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;
            var pubDataPath = Path.Combine(Git.pubGuardianFolder, GuardianSiteData.getFilename(entry.fullBodyName, entry.idx, entry.isRuins));

            if (File.Exists(pubDataPath))
            {
                var filename = GuardianSiteData.getFilename($"{entry.systemName} {entry.bodyName}", entry.idx, entry.isRuins);
                var pubPath = Path.Combine(Git.pubGuardianFolder, filename);
                Util.openLink(pubPath);
            }
            else
            {
                Game.log($"No pubData file found for: {entry.fullBodyNameWithIdx}");
            }
        }

        private void menuOpenImagesFolder_Click(object sender, EventArgs e)
        {
            if (this.grid.SelectedItems.Count == 0) return;
            var entry = (GuardianGridEntry)this.grid.SelectedItems[0].Tag;

            var folder = Path.Combine(Game.settings.screenshotTargetFolder!, entry.systemName);
            if (Directory.Exists(folder))
                Util.openLink(folder);
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            FormShareData.show(this);
        }

    }
}
