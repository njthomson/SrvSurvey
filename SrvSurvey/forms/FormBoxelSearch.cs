using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.plotters;
using SrvSurvey.units;
using static System.Net.Mime.MediaTypeNames;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBoxelSearch : SizableForm
    {
        private CommanderSettings cmdr;
        private StarRef from;
        private bool updatingList = false;

        private BoxelSearch bs { get => cmdr.boxelSearch!; }

        public FormBoxelSearch()
        {
            InitializeComponent();

            // load current or last cmdr, and their boxel search details
            this.cmdr = CommanderSettings.LoadCurrentOrLast();

            // default distance measuring from cmdr's current system
            this.from = cmdr.getCurrentStarRef();
            this.comboFrom.SetText(this.cmdr.currentSystem);
            this.comboFrom.updateOnJump = true;
            this.comboFrom.selectedSystemChanged += ComboFrom_selectedSystemChanged;

            // ensure we have a setting value
            cmdr.boxelSearch ??= new();

            bs.changed += boxelSearch_changed;

            checkAutoCopy.Checked = bs.autoCopy;
            checkSkipVisited.Checked = bs.skipAlreadyVisited;
            checkSpinKnownToSpansh.Checked = bs.skipKnownToSpansh;

            if (bs.collapsed)
                toggleListVisibility(true);

            // show warning if key-hooks are not viable
            //linkKeyChords.Visible = !bs.autoCopy && (!Game.settings.keyhook_TEST || string.IsNullOrEmpty(Game.settings.keyActions_TEST?.GetValueOrDefault(KeyAction.copyNextBoxel)));

            prepForm();
            bs.reset(bs.boxel, bs.active);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!bs.active)
            {
                // exit early if we don't have a valid boxel
                var bx = Boxel.parse(txtTopBoxel.Text);
                if (bx == null) return;

                bs.reset(bx, true);

                // start finding systems...
                this.saveAndSetCurrent(bs.current, true);
                Game.log($"Enabled boxel search:\r\n\tname: {bs.boxel}");

                // make plotter appear, update or close
                if (PlotSphericalSearch.allowPlotter)
                    Program.showPlotter<PlotSphericalSearch>()?.Invalidate();
            }
            else
            {
                // disable the feature?
                bs.active = false;
                cmdr.Save();
                prepForm();
                Game.log($"Disabling boxel search");

                // close plotter?
                if (!PlotSphericalSearch.allowPlotter)
                    Program.closePlotter<PlotSphericalSearch>();
            }
        }

        private void saveAndSetCurrent(Boxel bx, bool force = false)
        {
            var diff = bs.current != bx || force;
            if (diff)
                list.Items.Clear();

            bs.setCurrent(bx, true);
            cmdr.Save();

            if (diff)
                prepForm();
        }

        private void prepForm()
        {
            txtTopBoxel.Text = bs.boxel.name;
            this.setStatusText();

            if (bs.active)
            {
                // activate the feature
                btnSearch.Text = Properties.Misc.FormBoxelSearch_Disable;
                checkSkipVisited.Hide();
                checkSpinKnownToSpansh.Hide();
                numMax.Enabled = true;

                txtTopBoxel.ReadOnly = true;
                txtTopBoxel.Text = bs.boxel.name;
                txtCurrent.Text = bs.current.prefix;

                this.setNumMax();

                // add siblings
                this.prepSiblings();
            }
            else
            {
                // disable the feature
                btnSearch.Text = Properties.Misc.FormBoxelSearch_Activate;
                checkSkipVisited.Show();
                checkSpinKnownToSpansh.Show();
                numMax.Enabled = false;

                txtTopBoxel.ReadOnly = false;
                txtCurrent.Text = "";

                Program.invalidate<PlotSphericalSearch>();
            }

            panelList.Enabled = bs.active;
            numMax.Enabled = bs.active;
            btnParent.Enabled = bs.active;
            btnBoxelEmpty.Enabled = bs.active;
            btnPaste.Enabled = bs.active;
            btnCopyNext.Enabled = bs.active;
            txtCurrent.Enabled = bs.active;
        }

        private void setNumMax()
        {
            numMax.Enabled = false;

            if (!bs.currentEmpty)
            {
                var newMin = bs.currentEmpty ? 0 : bs.currentMax + 1;
                var newValue = Math.Max(bs.currentCount, newMin);

                // we need to be careful not to make the new minimum less than the current value or vice-versa
                if (newMin < numMax.Value)
                {
                    if (numMax.Minimum != newValue) numMax.Minimum = newMin;
                    if (numMax.Value != newValue) numMax.Value = newValue;
                }
                else
                {
                    if (numMax.Value != newValue) numMax.Value = newValue;
                    if (numMax.Minimum != newValue) numMax.Minimum = newMin;
                }
            }

            numMax.Enabled = true;
        }

        private void prepSiblings()
        {
            if (bs?.current == null) return;

            menuSiblings.Items.Clear();

            // show current
            var current = bs.current;
            menuSiblings.Items.Add(current.name)
                .Enabled = false;
            menuSiblings.Items.Add(new ToolStripSeparator());


            // add parent and siblings
            if (current.massCode != 'h')
            {
                //menuSiblings.Items.Add("Parent boxel:")
                //    .Enabled = false;

                var parent = current.parent;
                menuSiblings.Items.Add($"Parent: {parent}")
                    .Tag = new ItemTag(null, parent);

                var siblings = parent.children;
                var currentIdx = siblings.IndexOf(current);

                // prior sibling
                if (currentIdx > 0)
                {
                    //menuSiblings.Items.Add(new ToolStripSeparator());
                    //menuSiblings.Items.Add("Previous boxel:")
                    //    .Enabled = false;

                    var prevSibling = siblings[currentIdx - 1];
                    menuSiblings.Items.Add("Prev: " + prevSibling.ToString())
                        .Tag = new ItemTag(null, prevSibling);
                }
                else
                {
                    menuSiblings.Items.Add("Prev:")
                        .Enabled = false;
                }

                // next sibling
                if (currentIdx < siblings.Count - 1)
                {
                    //menuSiblings.Items.Add(new ToolStripSeparator());
                    //menuSiblings.Items.Add("Next boxel:")
                    //    .Enabled = false;

                    var nextSibling = siblings[currentIdx + 1];
                    menuSiblings.Items.Add("Next: " + nextSibling.ToString())
                        .Tag = new ItemTag(null, nextSibling);
                }
                else
                {
                    menuSiblings.Items.Add("Next:")
                        .Enabled = false;
                }
            }

            // add children
            if (current.massCode != 'a')
            {
                menuSiblings.Items.Add(new ToolStripSeparator());
                menuSiblings.Items.Add("Child boxels:")
                    .Enabled = false;

                foreach (var child in current.children)
                {
                    menuSiblings.Items.Add(child.ToString())
                        .Tag = new ItemTag(null, child);
                }
            }

        }

        private void menuSiblings_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var itemTag = e.ClickedItem?.Tag as ItemTag;
            if (itemTag == null) return;

            this.BeginInvoke(() =>
            {
                this.saveAndSetCurrent(itemTag.boxel);

                if (bs.autoCopy)
                    Clipboard.SetText(bs.getNextToVisit());
            });
        }

        private void boxelSearch_changed()
        {
            // adjust visible count to the largest known system
            setNumMax();

            this.prepList();
        }

        private void prepList()
        {
            if (this.IsDisposed || !list.Enabled) return;
            updatingList = true;

            if (bs.currentEmpty)
            {
                list.Items.Clear();
                list.Items.Add("Empty system");
                list.CheckBoxes = false;
            }
            else
            {
                list.CheckBoxes = true;
                var knownSystems = bs.systems.OrderBy(sys => sys.name.n2).ToList();
                var hasNotes = false;
                var max = (int)numMax.Value;
                for (int n = 0; n < max; n++)
                {
                    var bx = bs.current.to(n);

                    var item = n < list.Items.Count
                        ? list.Items[n]
                        : list.Items.Add(new ListViewItem
                        {
                            Name = bx.name,
                            Text = bx.name,
                        });

                    var sys = knownSystems.FirstOrDefault(sys => sys.name.n2 == n);
                    item.SubItems[0].Tag = sys;

                    if (item.Tag == null || item.SubItems.Count == 1)
                    {
                        item.Tag = bx;

                        item.SubItems.Add("?", "dist");
                        item.SubItems.Add("", "notes");
                    }

                    if (sys != null)
                    {
                        if (item.Checked != sys.complete)
                            item.Checked = sys.complete;

                        // set distance
                        var subDist = item.SubItems["dist"];
                        if (subDist!.Tag == null && sys.starPos != null)
                        {
                            var d = Util.getSystemDistance(from, sys.starPos);
                            subDist.Text = d.ToString("N2") + " ly";
                            subDist.Tag = d;
                        }

                        // update notes
                        // notes // TODO: Properties.Misc.FormBoxelSearch_UndiscoveredSystem;
                        var notes = sys?.visitedAt != null ? $"Visited: {sys.visitedAt.Value.LocalDateTime}" : null;
                        if (sys?.spanshUpdated != null)
                            notes += (notes == null ? "" : ", ") + $"Spansh: {sys.spanshUpdated.Value.LocalDateTime}";
                        item.SubItems["notes"]!.Text = notes;
                    }

                    if (!hasNotes)
                        hasNotes |= item.SubItems.Count == 3 && !string.IsNullOrEmpty(item.SubItems[2].Text);
                }

                // trim off any excess rows
                while (list.Items.Count > max)
                    list.Items.RemoveAt(max);

                // TODO: Make this flicker less
                if (list.Items.Count > 0) list.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                if (hasNotes) list.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            this.setStatusText();
            updatingList = false;
        }

        /*
        private ListViewItem createListItemFromResult(ItemTag tag, bool visited)
        {
            var listItem = new ListViewItem()
            {
                Name = tag.name,
                Text = tag.name,
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
            var target = cmdr.boxelSearch?.boxel?.to(n)!;
            var tag = new ItemTag(null, target);

            var listItem = new ListViewItem()
            {
                Name = target.ToString(),
                Text = target.ToString(),
                Tag = tag,
                Checked = visited,
            };

            // see if we can get a StarPos from local files
            var sysData = SystemData.Load(target.ToString(), 0, cmdr.fid);
            if (sysData != null)
            {
                tag.starPos = sysData.starPos;
                listItem.Checked = true;
            }

            // distance
            var sub1 = listItem.SubItems.Add("?");
            sub1.Name = "dist";
            sub1.Tag = 0;

            if (tag.starPos != null)
                setDistance(listItem);

            // notes
            var notes = Properties.Misc.FormBoxelSearch_UndiscoveredSystem;
            var sub2 = listItem.SubItems.Add(notes);
            sub2.Name = "notes";
            if (sysData != null)
                sub2.Text = $"Visited: {sysData.lastVisited.LocalDateTime}";

            return listItem;
        }
        */

        private void btnCopyNext_Click(object sender, EventArgs e)
        {
            var txt = bs.getNextToVisit();
            Clipboard.SetText(txt);
            lblStatus.Text = $"Next: {txt}";
        }

        private void numMax_ValueChanged(object sender, EventArgs e)
        {
            if (!numMax.Enabled) return;
            numMax.Enabled = false;

            bs.setCurrentCount((int)numMax.Value);

            Util.deferAfter(50, () =>
            {
                cmdr.Save();
                this.prepList();
            });

            numMax.Enabled = true;
        }

        private void list_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!list.Enabled || updatingList) return;

            var bx = e.Item.Tag as Boxel;
            if (bx != null)
            {
                var sys = bs.systems.FirstOrDefault(sys => sys.name.n2 == bx.n2);
                if (sys != null)
                    sys.complete = e.Item.Checked;
            }
        }

        public void markVisited(string name, bool visited)
        {
            var item = list.Items[name];
            if (item != null)
            {
                item.Checked = visited;
                if (visited)
                    item.SubItems["Notes"]!.Text = Properties.Misc.FormBoxelSearch_Visited.format(DateTime.Now);

                //if (string.IsNullOrEmpty(item.SubItems["dist"].Text))
                //{

                //}
                // TODO: txtNext.Text = boxelSearch.getNextToVisit();
            }

            this.setStatusText();
        }

        private void setStatusText()
        {
            if (bs == null || !bs.active)
            {
                lblStatus.Text = Properties.Misc.FormBoxelSearch_SearchNotActive;
            }
            else
            {
                lblStatus.Text = Properties.Misc.FormBoxelSearch_VisitedCounts.format(bs.countVisited, bs.currentCount);

                // tmp ?
                lblStatus.Text += " / " + bs.calcProgress();
            }
        }

        private void comboFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: remove!
        }

        private void measureDistances(StarRef starRef)
        {
            this.from = starRef;
            foreach (ListViewItem item in this.list.Items)
                setDistance(item);
        }

        private void setDistance(ListViewItem item)
        {
            var sys = item.SubItems[0].Tag as BoxelSearch.System;
            if (sys?.starPos == null) return;

            var dist = Util.getSystemDistance(from, sys.starPos);
            item.SubItems["dist"]!.Tag = dist;
            item.SubItems["dist"]!.Text = dist.ToString("N2") + " ly";
        }

        private void menuHelpLink_Click(object sender, EventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki/Searching-Space");
        }

        private void btnBoxelEmpty_Click(object sender, EventArgs e)
        {
            list.Items.Clear();
            bs.toggleEmpty();
            cmdr.Save();

            // auto trigger nav menu to assist in selecting the next boxel to search
            if (bs.currentEmpty)
            {
                menuSiblings.ShowOnTarget();
            }
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            var bx = Boxel.parse(Clipboard.GetText());
            if (bx == null) return;

            // exit early if pasted system is not contained within by our top boxel
            if (bs.boxel.containsChild(bx))
            {
                lblWarning.Hide();
                this.saveAndSetCurrent(bx);
            }
            else
            {
                // TODO: update warning text
                lblWarning.Show();
            }
        }

        private void btnToggleList_ButtonClick(object sender, EventArgs e)
        {
            toggleListVisibility(panelList.Visible);
        }

        private void toggleListVisibility(bool hide)
        {
            if (bs == null) return;

            if (hide)
            {
                panelList.Visible = false;
                panelList.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                this.Height -= panelList.Height;
                this.MaximumSize = new Size(5000, this.Height);
                btnToggleList.Text = Properties.Misc.FormBoxelSearch_ShowList;
                bs.collapsed = true;
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
                bs.collapsed = false;
                cmdr.Save();
            }
        }

        private void txtTopBoxel_TextChanged(object sender, EventArgs e)
        {
            var bx = Boxel.parse(txtTopBoxel.Text);
            btnSearch.Enabled = bx != null;
        }

        private void checkAutoCopy_CheckedChanged(object sender, EventArgs e)
        {
            bs.autoCopy = checkAutoCopy.Checked;
            cmdr.Save();

            // show warning if key-hooks are not viable
            //linkKeyChords.Visible = bs?.autoCopy != true && (!Game.settings.keyhook_TEST || string.IsNullOrEmpty(Game.settings.keyActions_TEST?.GetValueOrDefault(KeyAction.copyNextBoxel)));
        }

        private void checkSkipVisited_CheckedChanged(object sender, EventArgs e)
        {
            bs.skipAlreadyVisited = checkSkipVisited.Checked;
            cmdr.Save();
        }

        private void checkSpinKnownToSpansh_CheckedChanged(object sender, EventArgs e)
        {
            bs.skipKnownToSpansh = checkSpinKnownToSpansh.Checked;
            cmdr.Save();
        }

        private void ComboFrom_selectedSystemChanged(StarRef? starSystem)
        {
            Game.log($"!!!!! {starSystem}");
            if (comboFrom.SelectedSystem == null) return;

            measureDistances(comboFrom.SelectedSystem);
        }


    }

    class ItemTag
    {
        public Spansh.SystemResponse.Result? result;
        public Boxel boxel;
        public StarPos? starPos;

        public ItemTag(Spansh.SystemResponse.Result? result, Boxel boxel)
        {
            this.result = result;
            this.boxel = boxel;

            this.starPos = result?.toStarPos();
        }

        public string name { get => boxel.ToString(); }
    }

    /*
    class ToolStripItem2 : ToolStripItem
    {
        private string? header;
        private int textHeight;

        public ToolStripItem2(string text, string header, ItemTag tag) : base(text, null, null, text)
        {
            this.header = header;
            this.Tag = tag;
        }

        private int measureTextHeight(Graphics g)
        {
            return TextRenderer.MeasureText("H", this.Font).Height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (header == null)
            {
                base.OnPaint(e);
                return;
            }
            var g = e.Graphics;
            if (textHeight == 0) textHeight = measureTextHeight(g);

            // draw main text
            var y = Util.centerIn(this.Bounds.Height, textHeight);
            BaseWidget.renderText(g, this.Text, 3, y, this.Font, this.ForeColor);

            // draw box if we're hot
            if (this.Selected)
            {
                Game.log(e.ClipRectangle);
                g.DrawLine(Pens.Red, -20, 0, 20, 20);
            }
        }
    }

    class ToolStripRenderer2 : ToolStripRenderer
    {
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            Game.log(e.Text);
            base.OnRenderItemText(e);
        }
    }
    */
}
