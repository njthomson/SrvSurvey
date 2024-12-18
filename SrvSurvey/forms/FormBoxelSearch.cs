﻿using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.plotters;
using SrvSurvey.units;
using System.Text.RegularExpressions;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBoxelSearch : SizableForm
    {
        private CommanderSettings cmdr;
        private StarPos from;
        private BoxelSearchDef boxelSearch { get => cmdr.boxelSearch!; }

        public FormBoxelSearch()
        {
            InitializeComponent();

            // load current or last cmdr
            this.cmdr = CommanderSettings.LoadCurrentOrLast();

            // default distance measuring from cmdr's current system
            this.from = cmdr.getCurrentStarPos();
            this.comboFrom.Enabled = false;
            this.comboFrom.Text = cmdr.boxelSearch?.name ?? this.cmdr.currentSystem;
            this.comboFrom.Enabled = true;
            checkAutoCopy.Checked = boxelSearch?.autoCopy ?? true;

            if (cmdr.boxelSearch?.collapsed == true)
                toggleListVisibility(true);

            // show warning if key-hooks are not viable
            linkKeyChords.Visible = boxelSearch?.autoCopy != true && (!Game.settings.keyhook_TEST || string.IsNullOrEmpty(Game.settings.keyActions_TEST?.GetValueOrDefault(KeyAction.copyNextBoxel)));

            prepForm();
        }

        private void btnToggleList_ButtonClick(object sender, EventArgs e)
        {
            toggleListVisibility(panelList.Visible);
        }

        private void toggleListVisibility(bool hide)
        {
            if (hide)
            {
                panelList.Visible = false;
                panelList.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                this.Height -= panelList.Height;
                this.MaximumSize = new Size(5000, this.Height);
                btnToggleList.Text = Properties.Misc.FormBoxelSearch_ShowList;
                cmdr.boxelSearch!.collapsed = true;
                cmdr.Save();
            }
            else
            {
                this.MaximumSize = Size.Empty;
                this.Height += panelList.Height <= 1
                    ? 300
                    : panelList.Height;
                panelList.Visible = true;
                panelList.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                btnToggleList.Text = Properties.Misc.FormBoxelSearch_HideList;
                cmdr.boxelSearch!.collapsed = false;
                cmdr.Save();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (cmdr.boxelSearch?.active != true)
            {
                if (string.IsNullOrWhiteSpace(txtSystemName.Text)) return;

                // stop here if not a generated name
                var systemName = SystemName.parse(txtSystemName.Text);
                if (!systemName.generatedName)
                {
                    MessageBox.Show(this, Properties.Misc.FormBoxelSearch_NotViableMessage, "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // activate the feature
                if (cmdr.boxelSearch == null || systemName.prefix != cmdr.boxelSearch.sysName.prefix)
                {
                    cmdr.boxelSearch = new()
                    {
                        name = systemName.name,
                    };
                }
                cmdr.boxelSearch.max = (int)Math.Max(numMax.Value, systemName.num);
                cmdr.boxelSearch.active = true;
                cmdr.Save();

                Program.showPlotter<PlotSphericalSearch>();
                Game.log($"Enabled boxel search:\r\n\tname: {cmdr.boxelSearch.name}\r\n\tmax: {cmdr.boxelSearch.max}");
            }
            else
            {
                // disable the feature
                Game.log($"Disabling boxel search");
                if (cmdr.boxelSearch != null)
                    cmdr.boxelSearch.active = false;
                cmdr.Save();
            }

            prepForm();
        }

        private void prepForm()
        {
            this.setStatusText();

            if (cmdr.boxelSearch?.active == true)
            {
                txtSystemName.Enabled = numMax.Enabled = false;
                txtSystemName.Text = boxelSearch.name;
                numMax.Value = boxelSearch.max;
                btnSearch.Text = Properties.Misc.FormBoxelSearch_Disable;
                searchSystems();

                Program.showPlotter<PlotSphericalSearch>()?.Invalidate();
            }
            else
            {
                // disable the feature
                btnCopyNext.Enabled = txtNext.Enabled = checkAutoCopy.Enabled = panelList.Enabled = false;
                txtSystemName.Enabled = numMax.Enabled = true;
                //txtSystemName.Text = cmdr.currentSystem;
                btnSearch.Text = Properties.Misc.FormBoxelSearch_Activate;
                txtNext.Text = "";

                var systemName = SystemName.parse(txtSystemName.Text);
                numMax.Value = Math.Max(systemName.generatedName ? systemName.num : 0, numMax.Value);

                Program.invalidate<PlotSphericalSearch>();
            }
        }

        private void searchSystems()
        {
            if (!boxelSearch.sysName.generatedName) return;

            // disable UX
            btnSearch.Enabled = false;
            btnCopyNext.Enabled = txtNext.Enabled = checkAutoCopy.Enabled = panelList.Enabled = false;
            txtNext.Text = "";
            list.Items.Clear();
            list.Enabled = false;

            var query = boxelSearch.sysName.prefix + "*";
            var from = comboFrom.SelectedSystem ?? new StarPos(cmdr.starPos, cmdr.currentSystem).toReference();

            Game.spansh.getBoxelSystems(query, from).continueOnMain(this, response =>
            {
                list.Items.Clear();

                //if (response.results.Count == 0)
                //{
                //    btnSearch.Enabled = true;
                //    list.Enabled = true;
                //    return;
                //}

                var visited = boxelSearch.visited?.Split(",").ToHashSet() ?? new HashSet<string>();

                // convert results to ListViewItem
                var maxNum = 0;
                var realTags = new List<ItemTag>();
                foreach (var result in response.results)
                {
                    var parsed = SystemName.parse(result.name);
                    if (parsed.num > maxNum) maxNum = parsed.num;

                    var itemTag = new ItemTag(result, SystemName.parse(result.name));
                    realTags.Add(itemTag);
                }
                realTags = realTags.OrderBy(t => t.systemName.num).ToList();

                if (numMax.Value < maxNum)
                {
                    numMax.Value = maxNum;
                    boxelSearch.max = maxNum;
                    cmdr.Save();
                }
                else
                {
                    maxNum = (int)numMax.Value;
                }

                // populate list in order for known and unknown systems
                var countVisited = 0;
                for (int n = 0; n <= maxNum; n++)
                {
                    var isSystemKnown = realTags.Count > 0 && n == realTags[0].systemName.num;

                    var isVisited = visited.Contains(n.ToString());
                    if (isVisited) countVisited++;

                    var listItem = isSystemKnown
                        ? this.createListItemFromResult(realTags[0], isVisited)
                        : this.createEmptyListItem(n, isVisited);

                    list.Items.Add(listItem);
                    if (isSystemKnown) realTags.RemoveAt(0);

                    // pre-check current system
                    if (listItem.Name == cmdr.currentSystem)
                        cmdr.markBoxelSystemVisited(cmdr.currentSystem);
                }
                list.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                list.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
                this.setStatusText();

                txtNext.Text = boxelSearch.getNextToVisit();

                // enable relevant controls
                btnCopyNext.Enabled = txtNext.Enabled = checkAutoCopy.Enabled = panelList.Enabled = true;
                list.Enabled = true;
                btnSearch.Enabled = true;
                btnCopyNext.Focus();
            });
        }

        private ListViewItem createListItemFromResult(ItemTag tag, bool visited)
        {
            var listItem = new ListViewItem()
            {
                Name = tag.systemName.name,
                Text = tag.systemName.name,
                Tag = tag,
                Checked = visited,
            };
            // distance
            var sub1 = listItem.SubItems.Add("");
            sub1.Name = "dist";
            setDistance(listItem);

            // notes
            var notes = tag.result?.bodies?.Count > 0
                ? Properties.Misc.FormBoxelSearch_SpanshUpdatedBodies.format(tag.result!.updated_at.Date.ToShortDateString(), tag.result.bodies.Count)
                : Properties.Misc.FormBoxelSearch_SpanshUpdated.format(tag.result!.updated_at.Date.ToShortDateString());
            var sub2 = listItem.SubItems.Add(notes);
            sub2.Name = "notes";

            return listItem;
        }

        private ListViewItem createEmptyListItem(int n, bool visited)
        {
            var targetName = cmdr.boxelSearch!.sysName.to(n);
            var tag = new ItemTag(null, targetName);

            var listItem = new ListViewItem()
            {
                Name = targetName.name,
                Text = targetName.name,
                Tag = tag,
                Checked = visited,
            };
            // distance
            var sub1 = listItem.SubItems.Add("?");
            sub1.Name = "dist";
            sub1.Tag = 0;

            // notes
            var notes = Properties.Misc.FormBoxelSearch_UndiscoveredSystem;
            var sub2 = listItem.SubItems.Add(notes);
            sub2.Name = "notes";

            return listItem;
        }

        private void btnCopyNext_Click(object sender, EventArgs e)
        {
            txtNext.Text = boxelSearch.getNextToVisit();
            if (!string.IsNullOrWhiteSpace(txtNext.Text))
                Clipboard.SetText(txtNext.Text);
        }

        private void list_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!list.Enabled) return;
            list.Enabled = false;

            cmdr.markBoxelSystemVisited(e.Item.Name, !e.Item.Checked);
            Program.defer(() => list.Enabled = true);
        }

        public void markVisited(string name, bool visited)
        {
            var item = list.Items[name];
            if (item != null)
            {
                item.Checked = visited;
                if (visited)
                    item.SubItems["Notes"]!.Text = Properties.Misc.FormBoxelSearch_Visited.format(DateTime.Now);

                txtNext.Text = boxelSearch.getNextToVisit();
            }

            this.setStatusText();
        }

        private void setStatusText()
        {
            if (boxelSearch == null || !boxelSearch.active)
            {
                lblStatus.Text = Properties.Misc.FormBoxelSearch_SearchNotActive;
            }
            else
            {
                var countVisited = boxelSearch.visited?.Split(',').Length ?? 0;
                lblStatus.Text = Properties.Misc.FormBoxelSearch_VisitedCounts.format(countVisited, boxelSearch.max + 1);
            }
        }

        private void comboFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboFrom.SelectedSystem == null) return;

            this.from = comboFrom.SelectedSystem.toStarPos();
            measureDistances();
        }

        private void measureDistances()
        {
            foreach (ListViewItem item in this.list.Items)
                setDistance(item);
        }

        private void setDistance(ListViewItem item)
        {
            var tag = item?.Tag as ItemTag;
            if (item == null || tag == null || tag.result == null) return;

            var dist = Util.getSystemDistance(from, tag.result.ToStarPos());
            item.SubItems["dist"]!.Tag = dist;
            item.SubItems["dist"]!.Text = dist.ToString("N2") + " ly";
        }

        private void checkAutoCopy_CheckedChanged(object sender, EventArgs e)
        {
            if (boxelSearch != null && checkAutoCopy.Enabled)
            {
                boxelSearch.autoCopy = checkAutoCopy.Checked;
                cmdr.Save();

                // show warning if key-hooks are not viable
                linkKeyChords.Visible = boxelSearch?.autoCopy != true && (!Game.settings.keyhook_TEST || string.IsNullOrEmpty(Game.settings.keyActions_TEST?.GetValueOrDefault(KeyAction.copyNextBoxel)));
            }
        }

        private void menuHelpLink_Click(object sender, EventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki/Searching-Space");
        }

        private void linkKeyChords_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki/Searching-Space#auto-copy");
        }
    }

    public class SystemName
    {
        // See https://forums.frontier.co.uk/threads/marxs-guide-to-boxels-subsectors.618286/ or http://disc.thargoid.space/Sector_Naming

        private static Regex nameParts = new Regex(@"(.+) (\w\w-\w) (\w)(\d+)-?(\d+)?$", RegexOptions.Singleline);

        public static SystemName parse(string systemName)
        {
            var parts = nameParts.Match(systemName);

            // not a match
            if (parts.Groups.Count != 5 && parts.Groups.Count != 6)
                return new SystemName { name = systemName, generatedName = false };

            var name = new SystemName
            {
                name = systemName,
                generatedName = true,
                sector = parts.Groups[1].Value,
                subSector = parts.Groups[2].Value,
                massCode = parts.Groups[3].Value,
                id = int.Parse(parts.Groups[4].Value),
            };

            if (parts.Groups.Count == 6 && parts.Groups[5].Success)
                name.num = int.Parse(parts.Groups[5].Value);
            else
                name.hasTrailingDash = false;

            return name;
        }

        /// <summary> The whole name </summary>
        public string name;

        /// <summary> True if the system name confirms to generated naming conventions </summary>
        public bool generatedName;

        /// <summary> The initial sector, eg: 'Thuechu' from 'Thuechu YV-T d4-12' </summary>
        public string sector;
        /// <summary> The sub-sector portion, eg: 'YV-T' from 'Thuechu YV-T d4-12' </summary>
        public string subSector;
        /// <summary> The mass code portion, eg: 'd' from 'Thuechu YV-T d4-12' </summary>
        public string massCode;
        /// <summary> The initial number portion, eg:  '4' from 'Thuechu YV-T d4-12' </summary>
        public int id;
        /// <summary> The final number portion, eg:  '12' from 'Thuechu YV-T d4-12' </summary>
        public int num;

        private bool hasTrailingDash = true;

        public string prefix
        {
            get
            {
                if (hasTrailingDash)
                    return $"{sector} {subSector} {massCode}{id}-";
                else
                    return $"{sector} {subSector} {massCode}";
            }
        }

        public override string ToString()
        {
            return name;
        }

        public SystemName to(int newNum)
        {
            return new SystemName
            {
                name = $"{this.prefix}{newNum}",
                generatedName = true,
                sector = this.sector,
                subSector = this.subSector,
                massCode = this.massCode,
                id = this.id,
                num = newNum,
            };
        }
    }

    class ItemTag : Tuple<Spansh.SystemResponse.Result?, SystemName>
    {
        public ItemTag(Spansh.SystemResponse.Result? item1, SystemName item2) : base(item1, item2) { }
        public Spansh.SystemResponse.Result? result { get => Item1; }
        public SystemName systemName { get => Item2; }
    }
}
