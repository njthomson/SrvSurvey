using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.quests;
using System.Data;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayDev : SizableForm
    {
        private PlayState cmdrPlay;
        private PlayQuest pq;
        private FileSystemWatcher? folderWatcher;
        private string? selectedQuest;
        private string? selectedView;
        private bool importing;

        public FormPlayDev()
        {
            InitializeComponent();

            this.setChildrenEnabled(false);
            if (Game.activeGame?.cmdrPlay == null)
            {
                PlayState.load(CommanderSettings.currentOrLastFid).continueOnMain(this, rslt =>
                {
                    this.cmdrPlay = rslt;
                    this.setChildrenEnabled(true);
                    initComboQuests();
                });
            }
            else
            {
                this.cmdrPlay = Game.activeGame.cmdrPlay;
                initComboQuests();
                this.setChildrenEnabled(true);
            }

            Util.applyTheme(this, true);
        }

        private void initComboQuests()
        {
            var mapQuests = cmdrPlay.activeQuests.ToDictionary(pq => pq, pq => $"{pq.id} : {pq.quest.title}");
            if (mapQuests.Count == 0)
                return;

            var match = mapQuests.Keys.FirstOrDefault(x => x.id == selectedQuest);
            comboQuest.DataSource = new BindingSource(mapQuests, null);

            if (match != null)
                comboQuest.SelectedValue = match;
            else
                comboQuest.SelectedIndex = 0;
        }

        private void comboQuest_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (!comboQuest.Enabled || comboQuest.SelectedItem == null) return;
            pq = (PlayQuest)comboQuest.SelectedValue!;
            selectedQuest = pq.id;
            selectedView = null;

            //var mapView = new Dictionary<object, string>(); // cmdrPlay.activeQuests.ToDictionary(pq => pq, pq => $"{pq.id} : {pq.quest.title}");

            comboChapter.Items.Clear();
            comboChapter.Items.Add("Objectives");
            comboChapter.Items.AddRange(pq.quest.chapters.Keys.Select(x => $"Chapter: {x}").ToArray());
            comboChapter.Items.Add("Messages");
            comboChapter.SelectedIndex = 0;
            comboChapter.Enabled = true;

            checkWatchFolder.Enabled = false;
            checkWatchFolder.Checked = pq.watchFolder != null && pq.watchFolder == folderWatcher?.Path;
            checkWatchFolder.Enabled = pq.watchFolder != null && Directory.Exists(pq.watchFolder);
        }

        private PlayChapter? getSelectedChapter(string? view)
        {
            if (view?.StartsWith("Chapter: ") == true)
                return pq.chapters.Find(c => c.id == view.Substring(9));
            else
                return null;
        }

        private void comboChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            showJson();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            showJson();
            menuStatus.Text = $"Refreshed: {selectedView} (at: {DateTime.Now:T})";
        }

        private void showJson()
        {
            //if (!comboChapter.Enabled) return;
            txtJson.ReadOnly = false;

            selectedView = comboChapter.SelectedItem as string;
            var chapter = getSelectedChapter(selectedView);
            if (chapter != null)
            {
                txtJson.Text = JsonConvert.SerializeObject(chapter.vars, Formatting.Indented);
            }
            else if (selectedView == "Objectives")
            {
                txtJson.Text = JsonConvert.SerializeObject(pq.objectives, Formatting.Indented);
            }
            else if (selectedView == "Messages")
            {
                txtJson.Text = JsonConvert.SerializeObject(pq.msgs, Formatting.Indented);
            }
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtJson.Text)) return;

            try
            {
                var view = comboChapter.SelectedItem as string;
                var chapter = getSelectedChapter(view);
                if (chapter != null)
                {
                    // only keep changes to variables
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(txtJson.Text)!;
                    var unknownVar = obj.Keys.FirstOrDefault(k =>
                    {
                        var x = !chapter.vars.ContainsKey(k);
                        return x;
                    });
                    if (unknownVar != null)
                    {
                        menuStatus.Text = $"Cannot add new variable: {unknownVar} = {obj[unknownVar]}";
                        return;
                    }
                    chapter.vars = obj;
                    chapter.pushScriptVars();
                    txtJson.Text = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                else if (view == "Objectives")
                {
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, PlayObjective>>(txtJson.Text);
                    pq.objectives = obj!;
                    txtJson.Text = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                else if (view == "Messages")
                {
                    var obj = JsonConvert.DeserializeObject<List<PlayMsg>>(txtJson.Text)!;
                    foreach (var msg in obj) msg.parent = pq;
                    pq.msgs = obj;
                    txtJson.Text = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }

                menuStatus.Text = $"Parsed and updated: {view}";
                pq.parent.Save();

                BaseForm.get<FormPlayComms>()?.onQuestChanged(pq);
                PlotBase2.invalidate(nameof(PlotQuestMini));
            }
            catch (Exception ex)
            {
                Game.log($"btnParse_Click: {ex.Message}");
                menuStatus.Text = $"Parsing failed: {ex.Message}";
            }
        }

        private void checkWatchFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkWatchFolder.Enabled) return;

            if (checkWatchFolder.Checked)
            {
                if (pq.watchFolder != null || Directory.Exists(pq.watchFolder))
                    startWatching(pq.watchFolder);
            }
            else
            {
                stopWatching();
            }
        }

        private void menuImportFolder_Click(object sender, EventArgs e)
        {
            var picker = new FolderBrowserDialog()
            {
                Description = "Select folder containing quest definition files",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false,
            };

            var rslt = picker.ShowDialog(this);
            if (rslt == DialogResult.OK)
                folderImport(picker.SelectedPath);
        }

        private void menuWatchJournal_Click(object sender, EventArgs e)
        {
            if (Game.activeGame != null)
                BaseForm.show<FormPlayJournal>();
        }

        private void menuOpenComms_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormPlayComms>(f => f.cmdrPlay = this.cmdrPlay);
        }

        private void menuMore_DropDownOpening(object sender, EventArgs e)
        {
            menuWatchJournal.Enabled = Game.activeGame != null;
        }

        private void stopWatching()
        {
            if (folderWatcher == null) return;

            menuStatus.Text = $"Stop watching: {folderWatcher.Path}";

            folderWatcher.EnableRaisingEvents = false;
            folderWatcher.Created -= FolderWatcher_Changed;
            folderWatcher.Changed -= FolderWatcher_Changed;
            folderWatcher.Renamed -= FolderWatcher_Changed;
            folderWatcher.Deleted -= FolderWatcher_Changed;
            folderWatcher = null;

            checkWatchFolder.Checked = false;
        }

        private void startWatching(string folder)
        {
            checkWatchFolder.Enabled = false;

            if (folderWatcher != null) stopWatching();

            folderWatcher = new FileSystemWatcher(folder);
            folderWatcher.Filter = "*";
            folderWatcher.Created += FolderWatcher_Changed;
            folderWatcher.Changed += FolderWatcher_Changed;
            folderWatcher.Renamed += FolderWatcher_Changed;
            folderWatcher.Deleted += FolderWatcher_Changed;

            folderWatcher.EnableRaisingEvents = true;
            menuStatus.Text = $"Watching: {folder} ...";

            checkWatchFolder.Checked = true;
            checkWatchFolder.Enabled = true;
        }

        private void FolderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (importing) return;

            var folder = Path.GetDirectoryName(e.FullPath)!;
            menuStatus.Text = $"Changes in: {folder}";
            Util.deferAfter(500, () =>
            {
                folderImport(folder);
            });
        }

        private void folderImport(string folder)
        {

            importing = true;
            menuStatus.Text = $"Importing folder: {folder} ...";
            cmdrPlay.importFolder(folder, (newPQ) =>
            {
                menuStatus.Text = newPQ == null
                    ? $"Import failed: {folder}"
                    : $"Imported: {folder}";

                if (newPQ != null)
                    selectedQuest = newPQ.id;

                Program.defer(() => initComboQuests());
                importing = false;
            }).@catch(ex =>
            {
                Game.log($"Import failed: {ex.Message}\r\n\t{ex.StackTrace}");
                menuStatus.Text = $"Import failed: {ex.Message}";
            }).justDoIt();
        }

    }
}
