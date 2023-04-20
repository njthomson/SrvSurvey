using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Text;
using Microsoft.Win32;

namespace JournalViewer
{
    public partial class JournalViewer : Form
    {
        public static string journalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous\");

        private string[]? lines;
        private List<ListViewItem> rows = new List<ListViewItem>();
        private DateTime startTime, endTime;

        public JournalViewer()
        {
            InitializeComponent();

            this.openFileDialog.InitialDirectory = JournalViewer.journalFolder;
            this.comboEventName.Text = "FSD";
            this.txtJsonContains.Text = "53455564252137";
            this.checkJsonContains.Checked = true;
            this.propertyGrid.Hide();
        }

        private void JournalViewer_Load(object sender, EventArgs e)
        {
            this.openJournal(this.txtFilename.Text);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            var rslt = this.openFileDialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                this.txtFilename.Text = this.openFileDialog.FileName;
                this.openJournal(this.txtFilename.Text);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.openJournal(this.txtFilename.Text);
        }

        private void openJournal(string filepath)
        {
            this.lines = File.ReadAllLines(filepath);
            this.txtLog.Text = $"Reading: {filepath}";

            this.rows.Clear();
            this.comboEventName.Items.Clear();

            foreach (var json in lines)
            {
                var entry = JsonConvert.DeserializeObject<JToken>(json)!;

                var tag = new TagBlob(json);

                if (!this.comboEventName.Items.Contains(tag.eventName))
                    this.comboEventName.Items.Add(tag.eventName);

                var row = new ListViewItem(tag.timestamp);
                row.SubItems.Add(tag.eventName);
                row.SubItems.Add(tag.subJson);
                row.Tag = tag;

                this.rows.Add(row);
                this.viewer.Items.Add(row);
            }

            // extract start and end times
            this.startTime = ((TagBlob)this.rows[0].Tag).time;
            this.endTime = ((TagBlob)this.rows[this.rows.Count - 1].Tag).time;
            this.dateStart.Value = this.startTime;
            this.dateEnd.Value = this.endTime;

            this.viewer.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.lblCountRows.Text = $"{this.rows.Count} rows";

            this.filterRows();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            this.filterRows();
        }

        private void filterRows()
        {
            var isTimeFiltered = this.dateStart.Value != this.startTime || this.dateEnd.Value != this.endTime;

            var filteredRows = checkNoFilter.Checked
                ? new List<ListViewItem>(this.rows)
                : this.rows.Where(item =>
                {
                    var tag = (TagBlob)item.Tag;

                    // remove rows outside time range
                    if (this.checkDateRange.Checked && !(tag.time >= dateStart.Value && tag.time <= dateEnd.Value))
                        return false;

                    // remove rows by event name
                    if (this.checkEventName.Checked && !string.IsNullOrEmpty(comboEventName.Text) && !tag.eventName.Contains(comboEventName.Text, StringComparison.OrdinalIgnoreCase))
                        return false;

                    // remove rows by json contains
                    if (this.checkJsonContains.Checked && !string.IsNullOrEmpty(txtJsonContains.Text) && !tag.subJson.Contains(txtJsonContains.Text, StringComparison.OrdinalIgnoreCase))
                        return false;

                    return true;
                });

            this.viewer.Items.Clear();
            this.viewer.Items.AddRange(filteredRows.ToArray());
            this.lblCountRows.Text = $"{this.viewer.Items.Count} of {this.rows.Count} rows";

            foreach (ListViewItem item in this.viewer.Items)
            {
                item.BackColor = SystemColors.Window;
                item.UseItemStyleForSubItems = true;

                var tag = (TagBlob)item.Tag;
                tag.hit = false;

                // highlight time-range
                if (isTimeFiltered && !this.checkDateRange.Checked && tag.time >= dateStart.Value && tag.time <= dateEnd.Value)
                    item.BackColor = Color.LightYellow;

                // highlight event name matches
                if (!string.IsNullOrEmpty(comboEventName.Text) && !this.checkEventName.Checked && tag.eventName.Contains(comboEventName.Text, StringComparison.OrdinalIgnoreCase))
                    item.BackColor = Color.Yellow;

                // highlight json contains
                if (!string.IsNullOrEmpty(txtJsonContains.Text) && tag.subJson.Contains(txtJsonContains.Text, StringComparison.OrdinalIgnoreCase))
                {
                    item.SubItems[2].BackColor = Color.Aquamarine;
                    //tag.hit = true;
                    item.UseItemStyleForSubItems = false;
                }
            }
        }

        private void viewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.viewer.SelectedItems.Count == 0) return;

            var tag = (TagBlob)this.viewer.SelectedItems[0].Tag;
            //var foo = new Foo(); // Dictionary<string, dynamic>();
            //foo.eventName = tag.eventName;
            //foo.timestamp = tag.time;

            //tag; // .obj.ToDictionary<string, string>(_ => _.t);

            //object foo = new Foo(tag.obj);

            //dynamic foo = new ExpandoObject();
            //foo.a = "abv";
            //foo.bb = "cc";
            //this.memberNames = obj.Properties().Select(_ => _.Name).ToList();

            //var foo = new JTypeDescriptor(tag.obj);
            //this.propertyGrid.SelectedObject = foo;

            //var eek = JPropertyDescriptor. . ("foo");
            //eek.

            //txtLog.Text = String.Join(", ", foo.GetType().GetProperties().Select(_ => _.Name));

            //File.WriteAllText("d:\\foo.json", tag.json);
            //this.web.DocumentText = tag.json;
            //this.web.Navigate("file://d:\\foo.json");

            this.tree.Nodes.Clear();
            this.setNodes(this.tree.Nodes, tag.obj);
            //foreach (var prop in tag.obj.Properties())
            //{
            //    if (prop.Value.HasValues)
            //    {
            //        var node = this.tree.Nodes.Add($"{prop.Name}: ...");
            //    }
            //    else
            //    {
            //        this.tree.Nodes.Add($"{prop.Name}: {prop.Value}");
            //    }
            //    //child.na
            //}
            //var a = this.tree.Nodes.Add("hello");
            //a.Nodes.Add("world", "again");
            //a.Nodes.Add("more", "stuff");

            //var b = this.tree.Nodes.Add("more more");
            //b.Nodes.Add("world", "again");

        }

        private string formatNode(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    var array = token as JArray;
                    return $"[{array!.Count}]";

                //case JTokenType.Property:
                //    var prop = token as JProperty;
                //    return $"[{prop.Name}: {prop.Value}";

                case JTokenType.String:
                case JTokenType.Date:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    return $"{token}";

                default:
                    return $"{token.Type}? {token}";

            }
        }

        private void setNodes(TreeNodeCollection nodes, JObject obj)
        {
            foreach (var prop in obj.Properties())
            {
                if (prop.Value.HasValues) // && prop.Type == JTokenType.Object)
                {
                    if (prop.Value.Type == JTokenType.Array)
                    {
                        var array = (JArray)prop.Value;
                        var child = nodes.Add($"{prop.Name}: " + formatNode(prop.Value));
                        foreach (var item in array)
                        {
                            var subChild = child.Nodes.Add($"{item}");
                            if (item!.Type == JTokenType.Object)
                            {
                                this.setNodes(subChild.Nodes, (JObject)item);
                            }
                        }

                        //this.setNodes(child.Nodes, prop.Value);
                    }
                    else if (prop.Value.Type == JTokenType.Object)
                    {
                        var child = nodes.Add($"{prop.Name}: {formatNode(prop.Value)}");
                    }
                }
                else
                {
                    nodes.Add($"{prop.Name}: {formatNode(prop.Value)}");
                }
            }
        }

        private void JournalViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.filterRows();
            }
        }

        private void toolClose_ButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void copyText(string txt)
        {
            Clipboard.SetText(txt);
            lblStatus.Text = $"Copied: {txt}";
        }

        private void tree_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                copyText(this.tree.SelectedNode.Text);
            }
        }

        private void viewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                if (this.viewer.SelectedItems.Count == 0) return;


                var txt = new StringBuilder();
                foreach (ListViewItem item in this.viewer.SelectedItems)
                {
                    var tag = (TagBlob)item.Tag;
                    txt.AppendLine(tag.json);
                }

                copyText(txt.ToString());
            }
        }

        private void tree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.viewer.SelectedItems.Count == 0) return;
            if (this.tree.SelectedNode.Nodes.Count > 0) return;

            copyText(this.tree.SelectedNode.Text);
        }

        private void checkNoFilter_CheckedChanged(object sender, EventArgs e)
        {
            this.filterRows();
            btnFilter.Enabled = !checkNoFilter.Checked;
        }

        private void viewer_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //Debug.WriteLine($"DrawItem #{e.ItemIndex} / {e.State} / {e.Item!.Text}");

            if (e.Item.UseItemStyleForSubItems)
                e.DrawDefault = true;

            //e.DrawBackground();
            //e.DrawText(TextFormatFlags.Default);
            //e.Bounds.Offset(e.Item.Position);

            //var rect = new Rectangle(
            //    e.Item.Position.X, e.Item.Position.Y,
            //    viewer.Columns[1].Width, e.Item.Bounds.Height
            //);

            //            e.Graphics.FillRectangle(SystemBrushes.HotTrack, rect);
            //e.Graphics.DrawString(e.Item.Text, e.Item.Font, SystemBrushes.ControlText, rect);
        }

        private void viewer_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            //if (e.ColumnIndex == 0) return;


            //var tag = e.Item.Tag as TagBlob;

            //if (!tag.hit)
            //{
            //    e.DrawDefault = true;
            //}
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;


            var rect = new Rectangle(
                e.Bounds.X, e.Bounds.Y,
                viewer.Columns[e.ColumnIndex].Width, e.Bounds.Height
            );
            Debug.WriteLine($"DrawSubItem #{e.ItemIndex}.{e.ColumnIndex} / {e.ItemState}/ {rect} / {e.SubItem!.Text}");
            if (e.ItemState == 0) return;

            //var backColor = (e.ItemState & ListViewItemStates.Selected) > 0 ?  : Brushes.Lime;
            var backColor = Brushes.Lime; // SystemBrushes.Window; // e.Item.BackColor;
            var foreColor = SystemColors.WindowText;
            if ((e.ItemState & (ListViewItemStates.Hot | ListViewItemStates.Focused)) > 0)
            {
                backColor = SystemBrushes.Highlight;
                foreColor = SystemColors.HighlightText;
            }

            if (e.ColumnIndex != 2)
            {
                rect.Offset(0, 0);
                //rect.Inflate(-1, -1);
                //rect.Height -= 4;

                //e.DrawDefault = true;
                //e.Graphics.FillRectangle(SystemBrushes.Window, e.SubItem.Bounds);
                backColor = e.ColumnIndex == 0 ? Brushes.RoyalBlue : Brushes.RosyBrown;

                if ((e.ItemState & (ListViewItemStates.Hot | ListViewItemStates.Focused)) > 0)
                {
                    backColor = SystemBrushes.Highlight;
                }

                e.Graphics.FillRectangle(backColor, rect);
                TextRenderer.DrawText(e.Graphics, e.SubItem!.Text, e.Item!.Font, rect, foreColor, Color.Transparent, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                if (e.ItemState == 0)
                {
                    rect.Offset(+1, -1);
                    rect.Inflate(+1, 0);

                    e.Graphics.DrawRectangle(SystemPens.MenuBar, rect);
                }
                return;
            }

            e.Graphics.FillRectangle(backColor, e.SubItem.Bounds);
            var parts = e.SubItem!.Text.Split(txtJsonContains.Text);
            Size sz;

            for (int n = 0; n < parts.Length; n++)
            {
                var part = parts[n];
                sz = TextRenderer.MeasureText(e.Graphics, part, e.Item!.Font, rect.Size, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(e.Graphics, part, e.Item!.Font, rect, foreColor, Color.Transparent, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl);
                rect.X += sz.Width;
                rect.Width -= sz.Width;
                if (rect.Width <= 10) break;

                if (n == part.Length - 1) continue;
                sz = TextRenderer.MeasureText(e.Graphics, txtJsonContains.Text, e.Item!.Font, rect.Size, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(e.Graphics, txtJsonContains.Text, e.Item!.Font, rect, SystemColors.WindowText, Color.Cyan, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                rect.X += sz.Width;
                rect.Width -= sz.Width;

                if (rect.Width <= 10) break;
            }

            if (e.ItemState == 0)
            {
                rect.Offset(+1, -1);
                rect.Inflate(+1, 0);

                e.Graphics.DrawRectangle(SystemPens.MenuBar, rect);
            }

            //TextRenderer.DrawText(e.Graphics, parts[0], e.Item!.Font, rect, e.Item.ForeColor, Color.Cyan, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            //rect.X = e.Bounds.X;
            //e.Graphics.FillRectangle(Brushes.Lime, e.SubItem.Bounds);
            //TextRenderer.DrawText(e.Graphics, e.SubItem!.Text, e.Item!.Font, rect, e.Item.ForeColor, Color.Transparent, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private void viewer_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            //e.Graphics.FillRectangle(SystemBrushes.Window, e.Header.Bounds);
            //e.Graphics.DrawString(e.Header.Text, this.viewer.Font, SystemBrushes.ControlText, e.SubItem.Bounds);
        }

        private void viewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.viewer.SelectedItems.Count == 0) return;

            var selectedItem = this.viewer.SelectedItems[0];

            var subItem = selectedItem.GetSubItemAt(e.X, e.Y);
            if (subItem != null)
            {
                copyText(subItem.Text);
            }
        }
    }

    class TagBlob
    {
        public JObject obj { get; set; }
        public DateTime time;
        public string timestamp { get; set; }
        public string eventName { get; set; }
        public string json;
        public string subJson { get; set; }
        public bool hit;

        public TagBlob(string json)
        {
            this.json = json;
            this.obj = JsonConvert.DeserializeObject<JObject>(json)!;

            this.time = this.obj["timestamp"]!.Value<DateTime>()!;
            this.timestamp = this.obj["timestamp"]!.Value<string>()!;
            this.eventName = this.obj["event"]!.Value<string>()!;

            const string prefix = "event\":\"";
            var i1 = json.IndexOf("\"", json.IndexOf(prefix) + prefix.Length);
            var i2 = json.IndexOf("\"", i1 + 1);
            this.subJson = "{ " + json.Substring(Math.Max(i1, i2));
        }
    }
}