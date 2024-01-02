using SrvSurvey.game;
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
using System.Windows.Forms.VisualStyles;

namespace SrvSurvey
{
    public partial class FormRamTah : Form
    {
        public FormRamTah()
        {
            InitializeComponent();

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formRamTah);

            this.prepRuinsRows();
            this.prepLogCheckboxes();
        }

        private void prepLogCheckboxes()
        {
            var cmdr = Game.activeGame?.cmdr;
            if (cmdr == null) return;

            checkLog1.Checked = cmdr.decodeTheRuins[1];
            checkLog2.Checked = cmdr.decodeTheRuins[2];
            checkLog3.Checked = cmdr.decodeTheRuins[3];
            checkLog4.Checked = cmdr.decodeTheRuins[4];
            checkLog5.Checked = cmdr.decodeTheRuins[5];
            checkLog6.Checked = cmdr.decodeTheRuins[6];
            checkLog7.Checked = cmdr.decodeTheRuins[7];
            checkLog8.Checked = cmdr.decodeTheRuins[8];
            checkLog9.Checked = cmdr.decodeTheRuins[9];

            checkLog10.Checked = cmdr.decodeTheRuins[10];
            checkLog11.Checked = cmdr.decodeTheRuins[11];
            checkLog12.Checked = cmdr.decodeTheRuins[12];
            checkLog13.Checked = cmdr.decodeTheRuins[13];
            checkLog14.Checked = cmdr.decodeTheRuins[14];
            checkLog15.Checked = cmdr.decodeTheRuins[15];
            checkLog16.Checked = cmdr.decodeTheRuins[16];
            checkLog17.Checked = cmdr.decodeTheRuins[17];
            checkLog18.Checked = cmdr.decodeTheRuins[18];
            checkLog19.Checked = cmdr.decodeTheRuins[19];

            checkLog20.Checked = cmdr.decodeTheRuins[20];
            checkLog21.Checked = cmdr.decodeTheRuins[21];
            checkLog22.Checked = cmdr.decodeTheRuins[22];
            checkLog23.Checked = cmdr.decodeTheRuins[23];
            checkLog24.Checked = cmdr.decodeTheRuins[24];
            checkLog25.Checked = cmdr.decodeTheRuins[25];
            checkLog26.Checked = cmdr.decodeTheRuins[26];
            checkLog27.Checked = cmdr.decodeTheRuins[27];
            checkLog28.Checked = cmdr.decodeTheRuins[28];
        }

        private void prepRuinsRows()
        {
            var cmdr = Game.activeGame?.cmdr;

            // inject 21 rows with cmdr's state
            listRuins.Items.Clear();
            for (int n = 1; n <= 21; n++)
            {
                var subItems = new ListViewItem.ListViewSubItem[]
                {
                    // ordering here needs to manually match columns
                    new ListViewItem.ListViewSubItem { Text = n.ToString() },
                    new ListViewItem.ListViewSubItem { Tag = false },
                    new ListViewItem.ListViewSubItem { Tag = true },
                    new ListViewItem.ListViewSubItem { Tag = false },
                    new ListViewItem.ListViewSubItem { Tag = false },
                    new ListViewItem.ListViewSubItem { Tag = false },
                };

                if (n > 19) { subItems[1].Tag = null; } // Biology ends at 19
                if (n > 20) { subItems[2].Tag = null; subItems[5].Tag = null; } // Biology + Technology end at 20

                var item = new ListViewItem(subItems, 0);
                listRuins.Items.Add(item);
            }

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

        private Point getBoundsCenter(Rectangle r, Size sz)
        {
            var pt = new Point(
                (r.Width / 2) - (sz.Width / 2),
                (r.Height / 2) - (sz.Height / 2)
            );

            return pt;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // background + edges
            if (e.Item?.Selected == true)
                e.Graphics.FillRectangle(SystemBrushes.HotTrack, e.Bounds);
            else
                e.DrawBackground();
            e.Graphics.DrawRectangle(SystemPens.ControlDark, e.Bounds);

            if (e.ColumnIndex == 0)
            {
                TextRenderer.DrawText(e.Graphics, e.SubItem?.Text, this.Font, e.Bounds, SystemColors.WindowText, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                //e.DrawText(TextFormatFlags.Left);
                //e.DrawDefault = true;
            }
            else
            {
                var tag = e.Item?.SubItems[e.ColumnIndex].Tag as bool?;
                if (tag != null)
                {
                    var checkState = tag == true ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    var pt = this.getBoundsCenter(e.Bounds, CheckBoxRenderer.GetGlyphSize(e.Graphics, checkState));
                    pt.Offset(e.Bounds.Location);
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, pt, checkState);
                }
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();

            var txt = listRuins.Columns[e.ColumnIndex].Text;
            var flags = e.ColumnIndex == 0
                ? TextFormatFlags.Right | TextFormatFlags.VerticalCenter
                : TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

            TextRenderer.DrawText(e.Graphics, txt, this.Font, e.Bounds, SystemColors.WindowText, flags);
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //e.DrawBackground();
            var txt = tabControl1.TabPages[e.Index].Text;
            TabRenderer.DrawTabItem(e.Graphics, e.Bounds, txt, this.Font, false, TabItemState.Normal);
            //TextRenderer.DrawText(e.Graphics, txt, this.Font, e.Bounds, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            //TabRenderer.DrawTabPage(e.Graphics, e.Bounds);
        }
    }
}
