using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBoxelSearch : SizableForm
    {
        private CommanderSettings cmdr;
        private StarRef from;
        private bool updatingList = false;
        private bool configValid;

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

            txtMainBoxel.Text = bs.boxel?.name;
            checkAutoCopy.Checked = bs.autoCopy;
            updateNextSystem();

            if (bs.active && bs.collapsed)
                toggleListVisibility(true);

            // show warning if key-hooks are not viable
            //linkKeyChords.Visible = !bs.autoCopy && (!Game.settings.keyhook_TEST || string.IsNullOrEmpty(Game.settings.keyActions_TEST?.GetValueOrDefault(KeyAction.copyNextBoxel)));

            if (bs.active && bs.boxel != null)
            {
                bs.reset(bs.boxel, false);
                prepForm();
                menuSiblings.targetButton = btnParent;
            }
            else
            {
                prepForm();
                btnConfig_Click(null!, null!);
            }

            menuSiblings.Opening += menuSiblings_Opening;

            var mw = flowCommands.Right + 20;
            this.MinimumSize = new Size(mw, 188);
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            if (bs.collapsed)
                toggleListVisibility(false);

            // populate inline-dialog
            txtConfigBoxel.Text = bs.boxel?.name ?? cmdr.currentSystem;
            checkSkipVisited.Checked = bs.skipAlreadyVisited;
            checkSpinKnownToSpansh.Checked = bs.skipKnownToSpansh;
            checkCompleteOnFssAllBodies.Checked = bs.completeOnFssAllBodies;
            checkCompleteOnEnterSystem.Checked = !bs.completeOnFssAllBodies;
            comboLowMassCode.Text = bs.lowMassCode.ToString();
            dateStart.Value = bs.startedOn > DateTime.MinValue ? bs.startedOn : DateTime.Today;
            menuSiblings.targetButton = btnConfigNav;

            list.Items.Clear();
            btnToggleList.Enabled = false;
            tableTop.Visible = false;
            tableConfig.Visible = true;
            tableConfig.BringToFront();

            // disable the feature?
            if (bs.active)
            {
                bs.active = false;
                cmdr.Save();
                prepForm();
                Game.log($"Disabling boxel search");

                // close plotter?
                if (!PlotSphericalSearch.allowPlotter)
                    Program.closePlotter<PlotSphericalSearch>();
            }

            this.prepSiblings(bs.current);
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            closeConfig(true);
        }

        private void btnConfigCancel_Click(object sender, EventArgs e)
        {
            closeConfig(false);
        }

        private void closeConfig(bool save)
        {
            // exit early if we don't have a valid boxel
            var bx = Boxel.parse(txtConfigBoxel.Text);
            if (bx != null && save)
            {
                // populate from inline-dialog
                bs.completeOnFssAllBodies = checkCompleteOnFssAllBodies.Checked;
                bs.skipAlreadyVisited = checkSkipVisited.Checked;
                bs.skipKnownToSpansh = checkSpinKnownToSpansh.Checked;
                bs.lowMassCode = comboLowMassCode.Text[0];
                bs.startedOn = dateStart.Value;

                bs.reset(bx, true);
            }
            if (bs.boxel == null || bs.current == null)
            {
                // we cannot continue if there is no boxel
                this.Close();
                return;
            }

            // start finding systems...
            bs.active = true;
            this.saveAndSetCurrent(bs.current, true);
            Game.log($"Enabled boxel search:\r\n\tname: {bs.boxel}");

            // make plotter appear, update or close
            if (PlotSphericalSearch.allowPlotter)
                Program.showPlotter<PlotSphericalSearch>()?.Invalidate();

            // update form controls
            tableConfig.Visible = false;
            tableTop.Visible = true;
            btnToggleList.Enabled = true;
            menuSiblings.targetButton = btnParent;
            txtMainBoxel.Text = bs.boxel.name;
        }

        private void saveAndSetCurrent(Boxel bx, bool force = false)
        {
            var diff = bs.current != bx || force;
            if (diff)
                list.Items.Clear();

            bs.setCurrent(bx, true);
            cmdr.Save();

            if (diff)
            {
                prepForm();
                updateNextSystem();
            }
        }

        private void prepForm()
        {
            this.setStatusText();

            if (bs.active)
            {
                // activate the feature
                numMax.Enabled = true;
                txtCurrent.Text = bs.current?.prefix;

                this.setNumMax();

                // add siblings
                if (bs.current != null)
                    this.prepSiblings(bs.current);
            }
            else
            {
                // disable the feature
                numMax.Enabled = false;

                txtConfigBoxel.ReadOnly = false;
                txtCurrent.Text = "";

                Program.invalidate<PlotSphericalSearch>();
            }

            panelList.Enabled = bs.active;
            numMax.Enabled = bs.active;
            btnParent.Enabled = bs.active;
            btnBoxelEmpty.Enabled = bs.active;
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

        private void menuSiblings_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (menuSiblings.Items.Count == 0) return;

            menuSiblings.Items["paste"].Visible = Clipboard.ContainsText();
        }

        private void prepSiblings(Boxel bx)
        {
            menuSiblings.Items.Clear();

            // show copy current + paste
            var current = bx;
            menuSiblings.Items.Add("Copy", "copy", () =>
            {
                if (tableConfig.Visible)
                    Clipboard.SetText(txtConfigBoxel.Text);
                else
                    Clipboard.SetText(txtCurrent.Text);
            })
                .Image = ImageResources.copy1;

            // paste
            menuSiblings.Items.Add("Paste", "paste", () =>
            {
                if (Clipboard.ContainsText())
                {
                    if (tableConfig.Visible)
                        txtConfigBoxel.Text = Clipboard.GetText();
                    else
                    {
                        txtCurrent.Text = Clipboard.GetText();
                        prepSiblings(Boxel.parse(txtCurrent.Text)!);
                    }
                }
            })
                .Image = ImageResources.paste1;

            if (bx == null) return;
            menuSiblings.Items.Add(new ToolStripSeparator());

            // add parent and siblings
            if (current.massCode != 'h')
            {
                menuSiblings.Items.Add("Move to:")
                    .Enabled = false;

                var parent = current.parent;
                menuSiblings.Items.Add($"Parent: {parent}")
                    .Tag = new ItemTag(null, parent);

                var siblings = parent.children;
                var currentIdx = siblings.IndexOf(current);

                // prior sibling
                if (currentIdx > 0)
                {
                    var prevSibling = siblings[currentIdx - 1];
                    menuSiblings.Items.Add($"Prev: {prevSibling}", new ItemTag(null, prevSibling));
                }
                else
                {
                    menuSiblings.Items.Add("Prev:")
                        .Enabled = false;
                }

                // next sibling
                if (currentIdx < siblings.Count - 1)
                {
                    var nextSibling = siblings[currentIdx + 1];
                    menuSiblings.Items.Add($"Next: {nextSibling}", new ItemTag(null, nextSibling));
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
            var itemAction = e.ClickedItem?.Tag as Action;
            if (itemAction != null)
            {
                itemAction();
                return;
            }

            var itemTag = e.ClickedItem?.Tag as ItemTag;
            if (itemTag == null) return;

            if (tableConfig.Visible)
            {
                // just update the config text box
                txtConfigBoxel.Text = itemTag.boxel.name;
                return;
            }

            this.BeginInvoke(() =>
            {
                this.saveAndSetCurrent(itemTag.boxel);
            });
        }

        private void boxelSearch_changed()
        {
            // adjust visible count to the largest known system
            setNumMax();

            if (bs.nextSystem != null)
                txtCurrent.Text = bs.nextSystem;

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
                        item.Text = sys.name.ToString();

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

                list.EnsureVisible(bs.current.n2);
            }

            this.setStatusText();
            updatingList = false;
        }

        private void btnCopyNext_Click(object sender, EventArgs e)
        {
            this.updateNextSystem();
            if (bs.nextSystem != null)
                Clipboard.SetText(bs.nextSystem);
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
                {
                    if (sys.complete != e.Item.Checked)
                        sys.complete = e.Item.Checked;

                    this.updateNextSystem();
                }
                else
                {
                    // we cannot mark a system complete if we've never been there
                    lblStatus.Text = "Systems must be visited to force complete";
                    if (e.Item.Checked)
                        e.Item.Checked = false;
                }
            }
        }

        private void setStatusText()
        {
            if (bs == null || !bs.active)
                lblStatus.Text = Properties.Misc.FormBoxelSearch_SearchNotActive;
            else
                lblStatus.Text = Properties.Misc.FormBoxelSearch_VisitedCounts.format(bs.countVisited, bs.currentCount + 1, bs.countBoxelsCompleted, bs.countBoxelsTotal);
        }

        private void updateNextSystem()
        {
            bs.setNextToVisit();
            var txt = bs.nextSystem;
            if (txt != null)
            {
                txtCurrent.Text = txt;
                lblStatus.Text = $"Next: {txt}";
            }

            Program.invalidate<PlotSphericalSearch>();
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

        /*
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
        */

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
                this.Height += panelList.Height <= 10
                    ? 300
                    : panelList.Height;
                panelList.Visible = true;
                panelList.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                btnToggleList.Text = Properties.Misc.FormBoxelSearch_HideList;
                bs.collapsed = false;
                cmdr.Save();
            }
        }

        private void txtConfigBoxel_TextChanged(object sender, EventArgs e)
        {
            // confirm text is a valid boxel
            var bx = Boxel.parse(txtConfigBoxel.Text);
            this.configValid = bx != null;
            btnBegin.Enabled = this.configValid;
            comboLowMassCode.Enabled = this.configValid;
            lblBadBoxel.Visible = !this.configValid;

            if (bx == null) return;

            var mc = comboLowMassCode.SelectedItem;

            // populate lower mass code combo
            comboLowMassCode.Items.Clear();
            var mc2 = bx.massCode;
            comboLowMassCode.Items.Add(mc2);
            while (mc2 > 'a')
            {
                mc2--;
                comboLowMassCode.Items.Add(mc2);
            }

            // restore selected value?
            comboLowMassCode.SelectedItem = mc;
            if (comboLowMassCode.SelectedItem == null)
                comboLowMassCode.SelectedIndex = 0;

            // show how many boxels need to be searched
            setTotalChildCount(bx);

            prepSiblings(bx);
        }

        private void checkAutoCopy_CheckedChanged(object sender, EventArgs e)
        {
            bs.autoCopy = checkAutoCopy.Checked;
            cmdr.Save();
        }

        private void ComboFrom_selectedSystemChanged(StarRef? starSystem)
        {
            if (comboFrom.SelectedSystem == null) return;

            this.from = comboFrom.SelectedSystem;
            foreach (ListViewItem item in this.list.Items)
            {
                var sys = item.SubItems[0].Tag as BoxelSearch.System;
                if (sys?.starPos == null) continue;

                var dist = Util.getSystemDistance(from, sys.starPos);
                item.SubItems["dist"]!.Tag = dist;
                item.SubItems["dist"]!.Text = dist.ToString("N2") + " ly";
            }
        }

        private void comboLowMassCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            setTotalChildCount();
        }

        private void setTotalChildCount(Boxel? bx = null)
        {
            bx ??= Boxel.parse(txtConfigBoxel.Text);
            if (bx == null || string.IsNullOrWhiteSpace(comboLowMassCode.Text))
            {
                lblBoxelCount.Text = "";
                return;
            }

            // show how many boxels need to be searched
            var diff = (int)bx.massCode - comboLowMassCode.Text[0];
            var count = Boxel.getTotalChildCount(diff);
            var size = Boxel.getCubeSize(bx.massCode);
            lblBoxelCount.Text = $"Cube size: {size} ly - Boxels: {count}";
        }

        private void checkCompleteOnEnterSystem_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkCompleteOnFssAllBodies.Enabled) return;

            checkCompleteOnFssAllBodies.Enabled = false;
            checkCompleteOnEnterSystem.Enabled = false;

            checkCompleteOnFssAllBodies.Checked = false;
            checkCompleteOnEnterSystem.Checked = true;

            checkCompleteOnFssAllBodies.Enabled = true;
            checkCompleteOnEnterSystem.Enabled = true;
        }

        private void checkCompleteOnFssAllBodies_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkCompleteOnFssAllBodies.Enabled) return;

            checkCompleteOnFssAllBodies.Enabled = false;
            checkCompleteOnEnterSystem.Enabled = false;

            checkCompleteOnFssAllBodies.Checked = true;
            checkCompleteOnEnterSystem.Checked = false;

            checkCompleteOnFssAllBodies.Enabled = true;
            checkCompleteOnEnterSystem.Enabled = true;
        }

        private void menuListCopySystemName_Click(object sender, EventArgs e)
        {
            if (list.SelectedItems.Count == 0) return;
            var txt = list.SelectedItems[0].Text;
            if (!string.IsNullOrEmpty(txt))
                Clipboard.SetText(txt);
        }

        private void panelGraphic_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            var pn2 = new Pen(Color.Black, 1);
            var pb1 = new Pen(Color.Black, 1);
            var pb2 = new Pen(Color.Black, 2);
            var pr3 = new Pen(Color.Blue, 3);

            int s = (int)(panelGraphic.Height * 0.60f); // 200; // square size
            int d = (int)(s * 0.4f); //80; // depth
            int ds = s / 10; // 20; // "depth shear" aka perspective?

            var x = 10 + d;
            var y = 10;

            // back square
            var r1 = new Rectangle(x, y, s, s);
            var m1 = new Point(r1.Width / 2, r1.Height / 2);
            // front square
            var r2 = new Rectangle(x - d, y + d, s + ds, s + ds);
            var m2 = new Point(r2.Width / 2, r2.Height / 2);
            // middle square
            var rm = new Rectangle(x - d / 2, y + d / 2, s + ds / 2, s + ds / 2);
            var mm = new Point(rm.Width / 2, rm.Height / 2);

            labelGraphic.Left = (int)Math.Max(r1.Right, r2.Right) + 20;
            labelGraphic.Width = panelGraphic.Width - labelGraphic.Left;

            // draw back square
            g.DrawRectangle(pn2, r1);
            g.DrawLineR(pb2, r1.Left, r1.Top + m1.Y, r1.Width, 0);
            g.DrawLineR(pb1, r1.Left + m1.X, r1.Top, 0, r1.Height);

            // connect square corners
            g.DrawLine(pn2, r2.Left, r2.Top - 0.5f, r1.Left, r1.Top - 0.5f);
            g.DrawLine(pn2, r2.Right, r2.Top, r1.Right, r1.Top);
            g.DrawLine(pn2, r2.Left, r2.Bottom - 0.5f, r1.Left, r1.Bottom - 0.5f);
            g.DrawLine(pn2, r2.Right, r2.Bottom, r1.Right, r1.Bottom);

            // connect square middles
            g.DrawLine(pb2, r2.Left, r2.Top + m2.Y, r1.Left, r1.Top + m1.Y);
            g.DrawLine(pb2, r2.Right, r2.Top + m2.Y, r1.Right, r1.Top + m1.Y);
            g.DrawLine(pb1, r2.Left + m2.X, r2.Top, r1.Left + m1.X, r1.Top);
            g.DrawLine(pb1, r2.Left + m2.X, r2.Bottom, r1.Left + m1.X, r1.Bottom);

            // draw middle square
            g.DrawRectangle(pb1, rm);
            g.DrawLine(pb2, r2.Left + m2.X, r2.Top + m2.Y, r1.Left + m1.X, r1.Top + m1.Y);
            g.DrawLine(pb2, rm.Left, rm.Top + mm.Y, rm.Right, rm.Top + mm.Y);

            // draw front square
            g.DrawRectangle(pn2, r2);
            g.DrawLineR(pb2, r2.Left, r2.Top + m2.Y, r2.Width, 0);
            g.DrawLineR(pb1, r2.Left + m2.X, r2.Top, 0, r2.Height);

            // front small box
            var rr1 = new Rectangle(rm.Left, rm.Top + mm.Y, mm.X, mm.Y);
            var rr2 = new Rectangle(r2.Left, r2.Top + m2.Y, m2.X, m2.Y);
            g.DrawRectangle(pr3, rr1);
            g.DrawRectangle(pr3, rr2);

            g.DrawLine(pr3, rr2.Left, rr2.Top, rr1.Left, rr1.Top);
            g.DrawLine(pr3, rr2.Right, rr2.Top - 1, rr1.Right, rr1.Top - 1);
            g.DrawLine(pr3, rr2.Left, rr2.Bottom - 1, rr1.Left, rr1.Bottom - 1);
            g.DrawLine(pr3, rr2.Right, rr2.Bottom - 1, rr1.Right, rr1.Bottom - 1);
        }

        private void panelGraphic_SizeChanged(object sender, EventArgs e)
        {
            if (panelGraphic.Visible)
                panelGraphic.Invalidate();
        }
    }

    class ItemTag
    {
        public Spansh.SystemResponse.Result? result;
        public Boxel boxel;
        public StarPos? starPos;

        public Action? onClick;

        public ItemTag(Spansh.SystemResponse.Result? result, Boxel boxel)
        {
            this.result = result;
            this.boxel = boxel;

            this.starPos = result?.toStarPos();
        }

        public string name { get => boxel.ToString(); }
    }
}
