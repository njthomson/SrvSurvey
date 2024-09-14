using SrvSurvey.game;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

namespace SrvSurvey
{
    public partial class FormRamTah : Form
    {
        public static FormRamTah? activeForm;
        private Color checkedColor = Color.Turquoise;
        private List<CheckBox> checkLogs;

        public static void show()
        {
            if (activeForm == null)
                FormRamTah.activeForm = new FormRamTah();

            Util.showForm(FormRamTah.activeForm);
        }

        private CommanderSettings? cmdr { get => Game.activeGame?.cmdr; }

        private FormRamTah()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formRamTah.Location);

            txtRuinsMissionActive.Text = this.cmdr?.decodeTheRuinsMissionActive.ToString() ?? "Unknown";
            txtRuinsMissionActive.BackColor = this.cmdr?.decodeTheRuinsMissionActive == TahMissionStatus.Active ? checkedColor : SystemColors.Control;

            txtLogsMissionActive.Text = this.cmdr?.decodeTheLogsMissionActive.ToString() ?? "Unknown";
            txtLogsMissionActive.BackColor = this.cmdr?.decodeTheLogsMissionActive == TahMissionStatus.Active ? checkedColor : SystemColors.Control;

            this.checkLogs = new List<CheckBox>()
            {
                checkLog1, checkLog2, checkLog3, checkLog4, checkLog5,
                checkLog6, checkLog7, checkLog8, checkLog9, checkLog10,
                checkLog11, checkLog12, checkLog13, checkLog14, checkLog15,
                checkLog16, checkLog17, checkLog18, checkLog19, checkLog20,
                checkLog21, checkLog22, checkLog23, checkLog24, checkLog25,
                checkLog26, checkLog27, checkLog28
            };

            this.prepRuinsRows();
            this.updateChecks();

            this.setCurrentObelisk(Game.activeGame?.systemSite?.currentObelisk);

            // auto select 2nd tab if only the 2nd mission is active, or if we're at a structure
            var only2ndMissionActive = this.cmdr?.decodeTheRuinsMissionActive != TahMissionStatus.Active && this.cmdr?.decodeTheLogsMissionActive == TahMissionStatus.Active;
            if (only2ndMissionActive || Game.activeGame?.systemSite?.isRuins == false)
                tabControl1.SelectedIndex = 1;

            Util.applyTheme(this);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formRamTah != rect)
            {
                Game.settings.formRamTah = rect;
                Game.settings.Save();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormRamTah.activeForm = null;
        }

        private void prepLogCheckboxes()
        {
            foreach (var checkbox in this.checkLogs)
            {
                var idx = int.Parse(checkbox.Name.Substring(8));
                var shouldCheck = cmdr?.decodeTheLogs.Contains($"#{idx}") == true;
                if (checkbox.Checked != shouldCheck) checkbox.Checked = shouldCheck;
                if (checkbox.Checked)
                    checkbox.BackColor = checkedColor;
                else if (Game.activeGame?.systemSite?.currentObelisk?.msg == $"#{idx}")
                    checkbox.BackColor = Color.Lime;
                else
                    checkbox.BackColor = Color.Transparent;
            }
        }

        private void checkLog_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cmdr == null || Elite.isGameRunning != true) return;

            var checkbox = sender as System.Windows.Forms.CheckBox;
            if (checkbox != null)
            {
                checkbox.BackColor = checkbox.Checked ? checkedColor : Color.Transparent;

                var idx = int.Parse(checkbox.Name.Substring(8));
                if (checkbox.Checked)
                    this.cmdr.decodeTheLogs.Add($"#{idx}");
                else
                    this.cmdr.decodeTheLogs.Remove($"#{idx}");

                if (this.cmdr?.decodeTheLogsMissionActive == TahMissionStatus.Active)
                {
                    var logsProgress = (100.0 / 28 * cmdr?.decodeTheLogs.Count ?? 0).ToString("0");
                    txtLogsMissionActive.Text = $"{this.cmdr?.decodeTheLogsMissionActive} - {logsProgress}%";
                }

                //// update header labels to match
                //lblThargoids.BackColor = checkLog1.Checked && checkLog2.Checked && checkLog3.Checked && checkLog4.Checked && checkLog5.Checked
                //    ? checkedColor : Color.Transparent;

                //lblCivilWar.BackColor = checkLog6.Checked && checkLog7.Checked && checkLog8.Checked && checkLog9.Checked && checkLog10.Checked
                //    ? checkedColor : Color.Transparent;

                //lblTechnology.BackColor = checkLog11.Checked && checkLog12.Checked && checkLog13.Checked && checkLog14.Checked && checkLog15.Checked
                //    && checkLog16.Checked && checkLog17.Checked && checkLog18.Checked && checkLog19.Checked && checkLog20.Checked
                //    && checkLog21.Checked && checkLog22.Checked && checkLog23.Checked
                //    ? checkedColor : Color.Transparent;

                //lblLanguage.BackColor = checkLog24.Checked ? checkedColor : Color.Transparent;

                //lblBodyProtectorate.BackColor = checkLog25.Checked && checkLog26.Checked && checkLog27.Checked && checkLog28.Checked
                //    ? checkedColor : Color.Transparent;
            }
        }

        private void prepRuinsRows()
        {
            // inject 21 rows with cmdr's state
            listLogs.Items.Clear();
            for (int n = 1; n <= 21; n++)
            {
                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = n.ToString() },
                    new ListViewItem.ListViewSubItem { Name = $"B{n}" },
                    new ListViewItem.ListViewSubItem { Name = $"C{n}" },
                    new ListViewItem.ListViewSubItem { Name = $"H{n}" },
                    new ListViewItem.ListViewSubItem { Name = $"L{n}" },
                    new ListViewItem.ListViewSubItem { Name = $"T{n}" },
                };

                if (n > 19) { subItems[1].Name = null; } // Biology ends at 19
                if (n > 20) { subItems[2].Name = null; subItems[5].Name = null; } // Biology + Technology end at 20

                var item = new ListViewItem(subItems, 0);
                listLogs.Items.Add(item);
            }
        }

        private void listRuins_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.cmdr == null || Elite.isGameRunning != true) return;

            var row = listLogs.GetItemAt(e.X, e.Y);
            if (row != null)
            {
                var subItem = row.GetSubItemAt(e.X, e.Y);
                if (!string.IsNullOrEmpty(subItem.Name))
                {
                    if (cmdr.decodeTheRuins.Contains(subItem.Name))
                        cmdr.decodeTheRuins.Remove(subItem.Name);
                    else
                        cmdr.decodeTheRuins.Add(subItem.Name);

                    //listRuins.Invalidate(new Rectangle(0, subItem.Bounds.Y, subItem.Bounds.Width, 24));
                    listLogs.Invalidate();

                    if (this.cmdr?.decodeTheRuinsMissionActive == TahMissionStatus.Active)
                    {
                        var ruinsProgress = (100.0 / 101 * cmdr?.decodeTheRuins.Count ?? 0).ToString("0");
                        txtRuinsMissionActive.Text = $"{this.cmdr?.decodeTheRuinsMissionActive} - {ruinsProgress}%";
                    }
                }
            }

        }

        private Point getBoundsCenter(Rectangle r, Size sz)
        {
            var pt = new Point(
                (r.Width / 2) - (sz.Width / 2),
                (r.Height / 2) - (sz.Height / 2)
            );

            return pt;
        }

        private void listLogs_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var name = e.Item?.SubItems[e.ColumnIndex].Name;
            // background + edges;
            if (!string.IsNullOrEmpty(name) && this.cmdr?.decodeTheRuins.Contains(name) == true)
            {
                var b = e.ItemIndex % 2 == 1
                    ? Brushes.DarkTurquoise
                    : Brushes.Turquoise;
                e.Graphics.FillRectangle(b, e.Bounds);
            }
            else if (e.Item?.Selected == true)
            {
                e.Graphics.FillRectangle(SystemBrushes.HotTrack, e.Bounds);
            }
            else if (e.ItemIndex % 2 == 1)
            {
                var b = Game.settings.darkTheme ? SystemBrushes.ControlDark : SystemBrushes.ControlLight;
                e.Graphics.FillRectangle(b, e.Bounds);
            }
            else
            {
                e.DrawBackground();
            }

            if (Game.activeGame?.systemSite?.currentObelisk?.msg == name)
            {
                // draw some obvious highlight when the current obelisk yields this log
                var r = Rectangle.Inflate(e.Bounds, -2, -2);
                e.Graphics.DrawRectangle(GameColors.penBlack3Dotted, r);
            }

            if (e.Item?.Selected == true) // .ItemState == ListViewItemStates.Selected)
                e.DrawFocusRectangle(e.Bounds);

            e.Graphics.DrawRectangle(SystemPens.ControlDark, e.Bounds);

            if (e.ColumnIndex == 0)
            {
                TextRenderer.DrawText(e.Graphics, e.SubItem?.Text, this.Font, e.Bounds, listLogs.ForeColor, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
            else
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var checkState = this.cmdr?.decodeTheRuins.Contains(name) == true ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    var pt = this.getBoundsCenter(e.Bounds, CheckBoxRenderer.GetGlyphSize(e.Graphics, checkState));
                    pt.Offset(e.Bounds.Location);
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, pt, checkState);
                }
            }
        }

        private static Dictionary<char, int> completedCounts = new Dictionary<char, int>()
        {
            { 'B', 19 },
            { 'C', 20 },
            { 'L', 21 },
            { 'H', 21 },
            { 'T', 20 },
        };

        private void listLogs_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            var txt = listLogs.Columns[e.ColumnIndex].Text;
            var countCompleted = this.cmdr == null ? 0 : this.cmdr.decodeTheRuins.Count(_ => _[0] == txt[0]);
            if (completedCounts.ContainsKey(txt[0]) && countCompleted >= completedCounts[txt[0]])
                e.Graphics.FillRectangle(Brushes.Lime, e.Bounds);
            else
                e.DrawBackground();

            var flags = e.ColumnIndex == 0
                ? TextFormatFlags.Right | TextFormatFlags.VerticalCenter
                : TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

            TextRenderer.DrawText(e.Graphics, txt, this.Font, e.Bounds, SystemColors.WindowText, flags);
        }

        private void listLogs_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.cmdr?.Save();
            Game.activeGame?.systemSite?.ramTahRecalc();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://canonn.science/codex/ram-tahs-mission/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://canonn.science/codex/ram-tah-decrypting-the-guardian-logs/");
        }

        private void btnResetRuins_Click(object sender, EventArgs e)
        {
            if (this.cmdr == null) return;

            var rslt = MessageBox.Show(
                this,
                $"Are you sure you want to clear your progress of Decode the Ancient Ruins, {this.cmdr.decodeTheRuins.Count} logs?",
                "SrvSurvey",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (rslt == DialogResult.Yes)
            {
                this.cmdr.decodeTheRuins.Clear();
                this.prepRuinsRows();
            }
        }

        private void btnResetLogs_Click(object sender, EventArgs e)
        {
            if (this.cmdr == null) return;

            var rslt = MessageBox.Show(
                this,
                $"Are you sure you want to clear your progress of Decode the Ancient Logs, {this.cmdr.decodeTheLogs.Count} logs?",
                "SrvSurvey",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (rslt == DialogResult.Yes)
            {
                this.cmdr.decodeTheLogs.Clear();
                this.prepLogCheckboxes();
            }
        }

        internal void setCurrentObelisk(ActiveObelisk? obelisk)
        {
            if (obelisk == null)
            {
                txtObelisk.Enabled = lblObelisk.Enabled = btnToggleObelisk.Enabled = false;
                txtObelisk.Text = null;
            }
            else
            {
                txtObelisk.Enabled = lblObelisk.Enabled = btnToggleObelisk.Enabled = true;
                txtObelisk.Text = $"{obelisk.name}: {obelisk.items[0]}"
                    + (obelisk.items.Count > 1 ? $" + {obelisk.items[1]}" : null)
                    + $" for {obelisk.msgDisplay}";
            }

            this.updateChecks();
        }

        private void btnToggleObelisk_Click(object sender, EventArgs e)
        {
            // toggle it!
            var siteData = Game.activeGame?.systemSite;
            if (siteData == null || siteData.currentObelisk == null) return;

            siteData.toggleObeliskScanned();
            Elite.setFocusED();
        }

        public void updateChecks()
        {
            this.listLogs.Invalidate();
            this.prepLogCheckboxes();

            if (this.cmdr?.decodeTheRuinsMissionActive == TahMissionStatus.Active)
            {
                var ruinsProgress = (100.0 / 101 * cmdr?.decodeTheRuins.Count ?? 0).ToString("0");
                txtRuinsMissionActive.Text = $"{this.cmdr?.decodeTheRuinsMissionActive} - {ruinsProgress}%";
            }

            if (this.cmdr?.decodeTheLogsMissionActive == TahMissionStatus.Active)
            {
                var logsProgress = (100.0 / 28 * cmdr?.decodeTheLogs.Count ?? 0).ToString("0");
                txtLogsMissionActive.Text = $"{this.cmdr?.decodeTheLogsMissionActive} - {logsProgress}%";
            }
        }

        private void FormRamTah_Load(object sender, EventArgs e)
        {
            Elite.setFocusED();
        }
    }
}
