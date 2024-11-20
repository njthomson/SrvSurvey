using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.units;
using System.Data;
using static SrvSurvey.canonn.Canonn;

namespace SrvSurvey.forms
{
    internal partial class FormNearestSystems : FixedForm
    {
        public static void show(StarPos referencePos, string bioSignal)
        {
            FormNearestSystems.starPos = referencePos;
            FormNearestSystems.bioSignal = bioSignal;

            var formExists = BaseForm.get<FormNearestSystems>() != null;

            var form = BaseForm.show<FormNearestSystems>();

            if (formExists)
                form.prepList();
        }

        private static StarPos starPos;
        private static string bioSignal;

        public FormNearestSystems()
        {
            this.isDraggable = true;
            InitializeComponent();
            this.Icon = Icons.set_square;

            list.FullRowSelect = true;
            list.MultiSelect = false;

            this.list.Items.Clear();
            this.list.Items.Add(new ListViewItem(new string[] { "...", "..." }, 0, SystemColors.MenuText, SystemColors.Control, this.list.Font));
            this.list.Items.Add(new ListViewItem(new string[] { "...", "..." }, 0, SystemColors.MenuText, SystemColors.Window, this.list.Font));
            this.list.Items.Add(new ListViewItem(new string[] { "...", "..." }, 0, SystemColors.MenuText, SystemColors.Control, this.list.Font));

            if (Game.settings.darkTheme)
            {
                this.list.Items[0].BackColor = SystemColors.ControlDarkDark;
                this.list.Items[1].BackColor = SystemColors.WindowFrame;
                this.list.Items[2].BackColor = SystemColors.ControlDarkDark;
            }


            Util.applyTheme(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.prepList();
        }

        private QueryNearestResponse.Entry? selectedEntry
        {
            get
            {
                if (list.SelectedIndices.Count == 0) return null;
                var entry = list.Items[list.SelectedIndices[0]].Tag as QueryNearestResponse.Entry;
                return entry;
            }
        }

        private void prepList()
        {
            if (string.IsNullOrEmpty(FormNearestSystems.bioSignal) || FormNearestSystems.starPos == null) throw new Exception("Bad arguments for FormNearestSystems");

            txtSystem.Text = FormNearestSystems.starPos.systemName ?? FormNearestSystems.starPos.ToString();
            txtContaining.Text = FormNearestSystems.bioSignal;

            Game.canonn.findNearestSystemWithBio(FormNearestSystems.starPos, FormNearestSystems.bioSignal).ContinueWith(task =>
            {
                if (task.Result == null || task.Exception != null || this.IsDisposed) return;

                Program.defer(() =>
                {
                    var items = new List<ListViewItem>();
                    var result = task.Result;

                    list.SuspendLayout();
                    list.Items.Clear();

                    if (task.Result.nearest?.Count > 0)
                    {
                        var count = 0;
                        foreach (var entry in task.Result.nearest.Take(5))
                        {
                            count++;

                            var item = new ListViewItem()
                            {
                                Name = entry.system,
                                Text = entry.system,
                                Tag = entry,
                            };
                            if (Game.settings.darkTheme)
                                item.BackColor = (count % 2) == 0 ? SystemColors.ControlDarkDark : SystemColors.WindowFrame;
                            else
                                item.BackColor = (count % 2) == 0 ? SystemColors.Window : SystemColors.Control;

                            item.SubItems.AddRange(new string[] { $"{entry.distance.ToString("N1")} ly", "..." });

                            Game.canonn.getSystemPoi(entry.system, Game.activeGame?.cmdr?.commander ?? Game.settings.lastCommander!).ContinueWith(subTask =>
                            {
                                if (this.IsDisposed) return;

                                Program.defer(() =>
                                {
                                    if (this.IsDisposed) return;

                                    if (subTask.Result == null || subTask.Exception != null)
                                        item.SubItems[2].Text = "";
                                    else
                                        item.SubItems[2].Text = this.getSystemNotes(subTask.Result);

                                    list.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
                                });
                            });

                            list.Items.Add(item);
                        }
                    }

                    list.ResumeLayout();
                });

            });

            // make the API call...

        }

        private string getSystemNotes(SystemPoi result)
        {
            var systemEntryIds = result.codex.Select(c => c.entryid).ToHashSet();
            var backupText = $"System bio signals: {systemEntryIds.Count}";

            var summary = new Dictionary<string, HashSet<string>>();
            foreach (var codexEntry in result.codex)
            {
                if (codexEntry.hud_category != "Biology") continue;

                if (codexEntry.body == null)
                    return backupText;

                var key = codexEntry.body.Replace(" ", "");

                if (!summary.ContainsKey(key)) summary[key] = new();
                summary[key].Add(codexEntry.english_name);
            }

            if (summary.Count == 0)
                return "No bio signals in system";

            var parts = summary.Select(b => $"{b.Key}: {b.Value.Count} signals");
            var text = "Body " + string.Join(", ", parts);
            return text;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuCopyName_Click(object sender, EventArgs e)
        {
            var systemName = this.selectedEntry?.system;
            if (string.IsNullOrEmpty(systemName)) return;

            Game.log($"Setting to clipboard: {systemName}");
            Clipboard.SetText(systemName);
        }

        private void copyStarPos_Click(object sender, EventArgs e)
        {
            var starPos = this.selectedEntry?.toStarPos();
            if (starPos == null) return;

            Game.log($"Setting to clipboard: {starPos}");
            Clipboard.SetText(starPos.ToString());
        }

        private void viewOnCanonn_Click(object sender, EventArgs e)
        {
            var systemName = this.selectedEntry?.system;
            if (string.IsNullOrEmpty(systemName)) return;

            var url = $"https://signals.canonn.tech/?system=" + Uri.EscapeDataString(systemName);
            Util.openLink(url);
        }

        private void viewOnSpansh_Click(object sender, EventArgs e)
        {
            var systemName = this.selectedEntry?.system;
            if (string.IsNullOrEmpty(systemName)) return;

            var foo = Game.spansh.getSystemAddress(systemName).ContinueWith(t =>
            {
                if (t.Exception != null || t.Result == null) return;

                var url = $"https://spansh.co.uk/system/{t.Result}";
                Util.openLink(url);
            });
        }


        private void list_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var item = list.GetItemAt(e.X, e.Y);
            if (item == null) return;

            contextMenu.Show(list, e.X, e.Y);
        }
    }
}
