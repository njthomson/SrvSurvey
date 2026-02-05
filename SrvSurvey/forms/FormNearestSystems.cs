using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.Properties;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Data;
using static SrvSurvey.canonn.Canonn;

namespace SrvSurvey.forms
{
    [Draggable]
    internal partial class FormNearestSystems : FixedForm
    {
        /// <summary>
        /// Using Canonn API
        /// </summary>
        public static void show(StarPos refPos, string bioSignal, string cmdr)
        {
            if (refPos == null || string.IsNullOrEmpty(bioSignal)) throw new Exception("Bad arguments for FormNearestSystems");

            var form = new FormNearestSystems();
            form.txtSystem.Text = refPos.systemName ?? refPos.ToString();
            form.txtContaining.Text = bioSignal;
            form.Text = bioSignal;
            form.Show();

            // make the API call...
            Game.canonn.findNearestSystemWithBio(refPos, bioSignal).continueOnMain(form, result =>
            {
                form.list.SuspendLayout();
                form.list.Items.Clear();

                if (result.nearest?.Count > 0)
                {
                    var altCols = AlternatingColors.gridBackColors;
                    foreach (var entry in result.nearest.Take(5))
                    {
                        var item = new ListViewItem()
                        {
                            Name = entry.system,
                            Text = entry.system,
                            Tag = entry,
                            BackColor = altCols.next(),
                        };
                        item.SubItems.Add($"{entry.distance.ToString("N1")} ly");
                        item.SubItems.Add($"...");

                        Game.canonn.getSystemPoi(entry.system, cmdr).continueOnMain(form, subResult =>
                        {
                            item.SubItems[2].Text = getSystemNotes(subResult);
                            form.list.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
                        });

                        form.list.Items.Add(item);
                    }
                }

                form.list.ResumeLayout();
            });
        }

        private static string getSystemNotes(SystemPoi result)
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

        /// <summary>
        /// Using Spansh API
        /// </summary>
        public static void show(StarPos refPos, List<BioVariant> variants)
        {
            if (variants.Count == 0) return;

            // prep data
            var genus = variants[0].species.genus.englishName;
            var species = variants[0].species.englishName;
            var variantColors = variants.Select(v => v.colorName).ToList();

            // show + populate form before API call
            var form = new FormNearestSystems();
            form.txtSystem.Text = refPos.systemName ?? refPos.ToString();
            form.txtContaining.Text = species + ": " + string.Join(", ", variantColors);
            form.Text = form.txtContaining.Text;
            form.Show();

            // call the API ...
            Game.spansh.buildMissingVariantsQuery(refPos, genus, species, variantColors).continueOnMain(form, response =>
            {
                form.spanshReference = response!.search_reference;
                form.linkSpanshSearch.Visible = true;

                form.list.SuspendLayout();
                form.list.Items.Clear();

                if (response.results?.Count > 0)
                {
                    var altCols = AlternatingColors.gridBackColors;
                    foreach (var result in response.results)
                    {
                        // skip systems with multiple bodies
                        if (form.list.Items.ContainsKey(result.system_name)) continue;

                        // add a row for this system/body
                        var item = new ListViewItem(new string[] { result.system_name })
                        {
                            Name = result.system_name,
                            Text = result.system_name,
                            Tag = result,
                            BackColor = altCols.next(),
                        };
                        // distance
                        item.SubItems.Add($"{result.distance.ToString("N1")} ly");
                        //var d2 = Util.getSystemDistance(Game.activeGame!.cmdr.starPos, result.toStarPos());
                        //Game.log($"** {result.name} => {result.distance} vs {d2}");

                        var prefix = "";
                        if (result.landmarks?.Count > 0)
                        {
                            var colors = result.landmarks.Where(l => l.subtype == species).Select(l => l.variant).ToHashSet();
                            prefix = string.Join(", ", colors);
                        }
                        // notes                            
                        var notes = $"{prefix} - body: {result.name.Replace(result.system_name + " ", "")}, dist to arrival: {Util.lsToString(result.distance_to_arrival)}";
                        var countBioSignals = result.signals?.FirstOrDefault(s => s.name == "Biological")?.count;
                        if (countBioSignals > 0)
                            notes += $", {countBioSignals} bio signals";
                        item.SubItems.Add(notes);

                        // stop after 5 rows
                        form.list.Items.Add(item);
                        if (form.list.Items.Count > 5) break;
                    }
                }

                form.list.ResumeLayout();
            });
        }

        private string? spanshReference;

        public FormNearestSystems()
        {
            InitializeComponent();
            this.Icon = Icons.set_square;

            list.FullRowSelect = true;
            list.MultiSelect = false;

            this.list.Items.Clear();
            var altCols = AlternatingColors.gridBackColors;
            for (var n=0; n < 5; n++)
            {
                var item = new ListViewItem(
                    new string[] { "...", "...", "..." },
                    0,
                    Game.settings.darkTheme ? SystemColors.Info : SystemColors.ControlText,
                    altCols.next(),
                    this.list.Font);

                this.list.Items.Add(item);
            }

            BaseForm.applyThemeWithCustomControls(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private QueryNearestResponse.Entry? selectedCanonnEntry
        {
            get
            {
                if (list.SelectedIndices.Count == 0) return null;
                var entry = list.Items[list.SelectedIndices[0]].Tag as QueryNearestResponse.Entry;
                return entry;
            }
        }

        private SearchApiResults.Body? selectedSpanshEntry
        {
            get
            {
                if (list.SelectedIndices.Count == 0) return null;
                var entry = list.Items[list.SelectedIndices[0]].Tag as SearchApiResults.Body;
                return entry;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuCopyName_Click(object sender, EventArgs e)
        {
            var systemName = selectedCanonnEntry?.system ?? selectedSpanshEntry?.system_name;
            if (string.IsNullOrEmpty(systemName)) return;

            Game.log($"Setting to clipboard: {systemName}");
            Clipboard.SetText(systemName);
        }

        private void copyStarPos_Click(object sender, EventArgs e)
        {
            var starPos = selectedCanonnEntry?.toStarPos() ?? selectedSpanshEntry?.toStarPos();
            if (starPos == null) return;

            Game.log($"Setting to clipboard: {starPos}");
            Clipboard.SetText(starPos.ToString());
        }

        private void viewOnCanonn_Click(object sender, EventArgs e)
        {
            var systemName = selectedCanonnEntry?.system ?? selectedSpanshEntry?.system_name;
            if (string.IsNullOrEmpty(systemName)) return;

            var url = $"https://signals.canonn.tech/?system=" + Uri.EscapeDataString(systemName);
            Util.openLink(url);
        }

        private void viewOnSpansh_Click(object sender, EventArgs e)
        {
            var address = selectedSpanshEntry?.system_id64;
            if (address.HasValue)
            {
                var url = $"https://spansh.co.uk/system/{address}";
                Util.openLink(url);
                return;
            }

            var systemName = selectedCanonnEntry?.system ?? selectedSpanshEntry?.system_name;
            if (string.IsNullOrEmpty(systemName)) return;

            Game.spansh.getSystemAddress(systemName).continueOnMain(this, result =>
            {
                var url = $"https://spansh.co.uk/system/{result}";
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

        private void linkSpanshSearch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (spanshReference != null)
            {
                var url = $"https://spansh.co.uk/bodies/search/{spanshReference}/1";
                Util.openLink(url);
            }
        }
    }
}
