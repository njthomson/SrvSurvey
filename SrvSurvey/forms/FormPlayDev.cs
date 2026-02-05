using Lua;
using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.quests;
using System.Data;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayDev : SizableForm
    {
        public PlayState cmdrPlay;
        private PlayQuest pq;
        private FileSystemWatcher? folderWatcher;
        private string? selectedQuest;
        private string? selectedView;
        private bool importing;

        public FormPlayDev()
        {
            InitializeComponent();
            checkWatchFolder.Enabled = false;

            loadPlayState().justDoIt();

            BaseForm.applyThemeWithCustomControls(this);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            menuWatch.Enabled = Game.activeGame != null;
        }

        private async Task loadPlayState(bool reset = false)
        {
            var folderWatches = cmdrPlay?.activeQuests
                .Where(pq => pq.watchFolder != null)
                .ToDictionary(pq => pq.id, pq => pq.watchFolder);

            this.setChildrenEnabled(false);
            if (Game.activeGame?.cmdrPlay == null)
            {
                var newState = await PlayState.loadAsync(CommanderSettings.currentOrLastFid);
                if (newState != null)
                {
                    this.cmdrPlay = newState;
                    this.setChildrenEnabled(true, checkWatchFolder, btnRun, txtCode);
                    initComboQuests();
                }
            }
            else
            {
                if (reset)
                    await Game.activeGame.resetCmdrPlay();

                this.cmdrPlay = Game.activeGame.cmdrPlay;
                initComboQuests();
                this.setChildrenEnabled(true, checkWatchFolder, btnRun, txtCode);
            }

            if (folderWatches?.Count > 0 && cmdrPlay != null)
            {
                foreach (var x in folderWatches)
                {
                    var match = cmdrPlay.get(x.Key);
                    if (match != null)
                        match.watchFolder = x.Value;
                }
            }

            // and reload Comms window, if needed
            var comms = BaseForm.get<FormPlayComms>();
            if (comms != null)
            {
                comms.Close();
                BaseForm.show<FormPlayComms>(f => f.cmdrPlay = this.cmdrPlay!);
                this.Activate();
            }
        }

        public void onQuestChanged(PlayQuest? pq = null)
        {
            if (selectedQuest == pq?.id)
                this.showDebug();
        }

        private void initComboQuests()
        {
            var mapQuests = cmdrPlay.activeQuests.ToDictionary(pq => pq, pq => $"{pq.id} : {pq.quest.title}");
            if (mapQuests.Count == 0)
                return;

            var match = mapQuests.Keys.FirstOrDefault(x => x.id == selectedQuest);
            comboQuest.DataSource = new BindingSource(mapQuests, null!);

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
                return pq.chapters.FirstOrDefault(c => c.id == view.Substring(9));
            else
                return null;
        }

        private void comboChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            showDebug();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            showDebug();
        }

        private void showDebug()
        {
            txtJson.ReadOnly = false;

            selectedView = comboChapter.SelectedItem as string;
            var chapter = getSelectedChapter(selectedView);
            if (chapter != null)
            {
                chapter.pullScriptVars();
                txtJson.Text = JsonConvert.SerializeObject(chapter.vars, Formatting.Indented);
                btnRestartChapter.Enabled = !chapter.active;
                btnStopChapter.Enabled = chapter.active;
                btnRun.Enabled = txtCode.Enabled = true;
            }
            else if (selectedView == "Objectives")
            {
                txtJson.Text = JsonConvert.SerializeObject(pq.objectives, Formatting.Indented);
                btnRestartChapter.Enabled = btnStopChapter.Enabled = false;
                btnRun.Enabled = txtCode.Enabled = false;
            }
            else if (selectedView == "Messages")
            {
                txtJson.Text = JsonConvert.SerializeObject(pq.msgs, Formatting.Indented);
                btnRestartChapter.Enabled = btnStopChapter.Enabled = false;
                btnRun.Enabled = txtCode.Enabled = false;
            }

            menuStatus.Text = $"{selectedView} (at: {DateTime.Now:T})";
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
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, LuaValue>>(txtJson.Text)!;
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

                PlayState.updateUI(pq);
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
                folderImport(picker.SelectedPath).justDoIt();
        }

        private void menuWatch_Click(object sender, EventArgs e)
        {
            if (Game.activeGame != null)
                BaseForm.show<FormPlayJournal>();
        }

        private void menuReadFromFile_Click(object sender, EventArgs e)
        {
            var priorView = this.selectedView;

            this.loadPlayState(true).continueOnMain(this, () =>
            {
                if (priorView != null && comboChapter.Items.Contains(priorView))
                    comboChapter.SelectedItem = priorView;
            });
        }

        private void menuComms_Click(object sender, EventArgs e)
        {
            FormPlayComms.toggleForm();
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
            folderWatcher.Dispose();
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
                folderImport(folder).justDoIt();
            });
        }

        private async Task folderImport(string folder)
        {
            var priorView = this.selectedView;

            importing = true;
            menuStatus.Text = $"Importing folder: {folder} ...";
            var oldPQ = cmdrPlay.activeQuests.Find(pq => pq.watchFolder == folder);

            var newPQ = await cmdrPlay.sideLoad(folder);

            menuStatus.Text = newPQ == null
                ? $"Import failed: {folder}"
                : $"Imported: {folder}";

            if (newPQ != null)
            {
                selectedQuest = newPQ.id;
                if (oldPQ != null)
                {
                    // preserve state from previous PlayQuest
                    foreach (var (k, v) in oldPQ.objectives) newPQ.objectives[k] = v;
                    //foreach (var (k, v) in oldPQ.vars) newPQ.vars[k] = v;

                    foreach (var oc in oldPQ.chapters)
                    {
                        var match = newPQ.chapters.FirstOrDefault(c => c.id == oc.id);
                        if (match == null) continue;
                        match.startTime = oc.startTime;
                        match.endTime = oc.endTime;
                        foreach (var (k, v) in oc.vars) match.vars[k] = v;
                    }

                    foreach (var om in oldPQ.msgs)
                    {
                        var idx = newPQ.msgs.FindIndex(m => m.id == om.id);
                        if (idx == -1)
                            newPQ.msgs.Add(om);
                        else
                            newPQ.msgs[idx] = om;
                    }
                    cmdrPlay.Save();
                }
            }

            Program.defer(() =>
            {
                initComboQuests();
                PlayState.updateUI(newPQ);

                if (priorView != null && comboChapter.Items.Contains(priorView))
                    comboChapter.SelectedItem = priorView;
            });
            importing = false;
        }

        private void btnRestartChapter_Click(object sender, EventArgs e)
        {
            var chapter = getSelectedChapter(selectedView);
            chapter?.start().continueOnMain(this, () =>
            {
                cmdrPlay.Save();
                PlayState.updateUI(chapter.pq);
            });
        }

        private void btnStopChapter_Click(object sender, EventArgs e)
        {
            var chapter = getSelectedChapter(selectedView);
            if (chapter == null) return;
            chapter.stop();
            cmdrPlay.Save();
            PlayState.updateUI(chapter.pq);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            doRunCode().justDoIt();
        }

        private async Task doRunCode()
        {
            var chapter = getSelectedChapter(selectedView);
            if (chapter == null) return;

            menuStatus.Text = "Running code ...";
            txtCode.ReadOnly = true;
            btnRun.Enabled = false;
            try
            {
                var json = await chapter.runDebug(txtCode.Text);
                PlayState.updateUI(chapter.pq);
                menuStatus.Text = json;
            }
            catch (Exception ex)
            {
                menuStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                txtCode.ReadOnly = false;
                btnRun.Enabled = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Enter | Keys.Shift))
            {
                doRunCode().justDoIt();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
