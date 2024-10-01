using Newtonsoft.Json;
using SrvSurvey.game;
using System.ComponentModel;
using System.Diagnostics;

namespace SrvSurvey
{
    public partial class FormPostProcess : DraggableForm
    {
        public static string cmdArg = "-scan-old";
        public static FormPostProcess? activeForm;

        private Dictionary<string, string> allCmdrs;

        private string targetCmdrName;
        private string targetCmdrFid;
        private DateTime targetStartTime = DateTime.MinValue;

        private bool processingFiles = false;
        private bool processingSystems = false;
        private bool cancelprocessing = false;

        public long lastSystemAddress;
        public int? lastBodyId;

        public FormPostProcess(string? fid)
        {
            InitializeComponent();
            Util.useLastLocation(this, Game.settings!.formPostProcess, true);
            FormPostProcess.activeForm = this;

            // load potential cmdr's
            this.findCmdrs(fid);

            // default to 7 days ago, at midnight
            this.dateTimePicker.Value = DateTime.Now.Subtract(new TimeSpan(6, 23, 59, 59) + DateTime.Now.TimeOfDay);
            this.targetStartTime = dateTimePicker.Value;
            this.btnStart.Enabled = false;

            Util.applyTheme(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // async initialize things, block process starting until they are done
            this.BeginInvoke(() =>
            {
                Game.codexRef.init(false).ContinueWith(t => this.BeginInvoke(() =>
                {
                    btnStart.Enabled = true;
                    btnStart.Focus();
                }));
            });
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formPostProcess != rect)
            {
                Game.settings.formPostProcess = rect;
                Game.settings.Save();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormPostProcess.activeForm = null;
        }

        private void findCmdrs(string? fid)
        {
            this.allCmdrs = CommanderSettings.getAllCmdrs();
            var cmdrs = this.allCmdrs
                .Values
                .Order()
                .ToArray();

            comboCmdr.Items.Clear();
            comboCmdr.Items.AddRange(cmdrs);

            if (!string.IsNullOrEmpty(fid) && allCmdrs.ContainsKey(fid))
                comboCmdr.SelectedItem = allCmdrs[fid];
            else if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
                comboCmdr.SelectedItem = Game.settings.preferredCommander;
            else if (!string.IsNullOrEmpty(Game.settings.lastCommander))
                comboCmdr.SelectedItem = Game.settings.lastCommander;
            else
                comboCmdr.SelectedIndex = 0;
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            this.targetStartTime = dateTimePicker.Value;
        }

        private void btnLongAgo_Click(object sender, EventArgs e)
        {
            // Original release date for Elite Dangerous
            dateTimePicker.Value = new DateTime(2014, 12, 15);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!this.processingFiles)
            {
                // set cmdr details first
                this.targetCmdrName = comboCmdr.Text;
                this.targetCmdrFid = this.allCmdrs.First(_ => _.Value == this.targetCmdrName).Key;

                comboCmdr.Enabled = false;
                dateTimePicker.Enabled = false;
                btnLongAgo.Enabled = false;
                btnSystems.Enabled = false;
                btnStart.Text = "Stop";
                btnClose.Enabled = false;

                Task.Run(() => Program.crashGuard(() => this.postProcessJournals()));
            }
            else
            {
                this.cancelprocessing = true;
                btnStart.Text = "Stopping";
                btnStart.Enabled = false;
            }
        }

        private void btnSystems_Click(object sender, EventArgs e)
        {
            if (!this.processingSystems)
            {
                btnStart.Enabled = false;
                btnSystems.Text = "Stop";
                Task.Run(() => Program.crashGuard(() => this.postProcessSystems()));
            }
            else
            {
                this.cancelprocessing = true;
            }
        }

        private DateTimeOffset getTimeFromFilepath(string filepath)
        {
            try
            {
                var parts = filepath.Split('.');
                var time = filepath.Contains('-')
                    ? DateTimeOffset.ParseExact(parts[1], "yyyy-MM-ddTHHmmss", null)
                    : DateTimeOffset.ParseExact(parts[1], "yyMMddHHmmss", null);
                return time;
            }
            catch (Exception ex)
            {
                Game.log(ex);
                return DateTimeOffset.MinValue; // returning this means the file will be filtered out
            }
        }

        private void postProcessJournals()
        {
            this.processingFiles = true;
            var lastDate = "";
            var countJumps = 0;
            var countDistance = 0d;
            var countBodies = 0;
            var countOrganisms = 0;
            var startTime = DateTime.Now;
            try
            {
                // get list of files to process, date filtered and ordered oldest first
                var files = Directory.GetFiles(JournalFile.journalFolder, "Journal.*.log")
                    .Where(filepath => getTimeFromFilepath(filepath) > this.targetStartTime)
                    .OrderBy(filepath => getTimeFromFilepath(filepath))
                    .ToArray();

                var cmdr = CommanderSettings.Load(this.targetCmdrFid!, true, this.targetCmdrName!);
                var cmdrCodex = CommanderCodex.Load(this.targetCmdrFid!, this.targetCmdrName!);

                this.Invoke(new Action(() =>
                {
                    lblProgress.Text = $"Processing #{0} of {files.Length} journal files ... (from: ?, elapsed: )";
                    txtJumpCount.Text = countJumps.ToString("N0");
                    txtDistance.Text = countDistance.ToString("N0");
                    txtBodyCount.Text = countBodies.ToString("N0");
                    txtOrgCount.Text = countOrganisms.ToString("N0");

                    progress.Maximum = files.Length;
                    progress.Value = 0;
                    Data.suppressLoadingMsg = true;
                }));

                // read each journal file...
                for (var n = 0; n < files.Length; n++)
                {
                    if (this.cancelprocessing) break;
                    this.Invoke(new Action(() =>
                    {
                        var elapsed = DateTime.Now - startTime;
                        lblProgress.Text = $"Processing #{n} of {files.Length} journal files ... (from: {lastDate}, elapsed: {elapsed.ToString("mm\\:ss")})";
                        txtJumpCount.Text = countJumps.ToString("N0");
                        txtDistance.Text = countDistance.ToString("N0");
                        txtBodyCount.Text = countBodies.ToString("N0");
                        txtOrgCount.Text = countOrganisms.ToString("N0");
                        progress.Maximum = files.Length;
                        progress.Value = n;

                        if (this.cancelprocessing)
                            lblProgress.Text = "Stopped. " + lblProgress.Text;
                    }));


                    // skip if wrong cmdr
                    var filepath = files[n];

                    // skip if before timestamp
                    var lastWriteTime = File.GetLastWriteTime(filepath);
                    if (lastWriteTime < this.targetStartTime) continue;

                    var journal = new JournalFile(filepath, this.targetCmdrName);
                    if (journal.cmdrName != this.targetCmdrName) continue;
                    // ignore files that are not shutdown, unless they were opened more than 2 days ago - we will process those                    
                    if (!journal.isShutdown && journal.timestamp > DateTime.Now.AddDays(-2)) continue;

                    lastDate = journal.timestamp.ToShortDateString();

                    if (journal.isOdyssey)
                    {
                        // if Odyssey ... process all exploration events
                        SystemData? sysData = null;
                        foreach (var entry in journal.Entries)
                        {
                            if (this.cancelprocessing) break;

                            if (entry.@event == nameof(FSDJump))
                            {
                                countJumps++;
                                countDistance += ((FSDJump)entry).JumpDist;
                            }
                            if (entry.@event == nameof(ApproachBody)) countBodies++;
                            if (entry.@event == nameof(ScanOrganic) && ((ScanOrganic)entry).ScanType == ScanType.Analyse) countOrganisms++;

                            var starterEvent = entry as ISystemDataStarter;
                            if (starterEvent != null)
                            {
                                if (sysData != null)
                                {
                                    sysData.Save();
                                }

                                sysData = SystemData.From(starterEvent, this.targetCmdrFid, this.targetCmdrName);
                            }

                            // process play-forward entry
                            if (sysData != null && SystemData.journalEventTypes.Contains(entry.@event))
                                sysData.Journals_onJournalEntry(entry);

                            // special case for tracking codex first entries, as the natural code path will not handle them
                            var approachBodyEntry = entry as SrvSurvey.ApproachBody;
                            if (approachBodyEntry != null)
                            {
                                this.lastSystemAddress = approachBodyEntry.SystemAddress;
                                this.lastBodyId = approachBodyEntry.BodyID;
                            }

                            // special case for tracking codex first entries, as the natural code path will not handle them (as there is no `Game.activeGame`)
                            var codexEntry = entry as SrvSurvey.CodexEntry;
                            if (codexEntry != null)
                            {
                                var galacticRegionId = sysData != null
                                        ? EliteDangerousRegionMap.RegionMap.FindRegion(sysData.starPos).Id
                                        : GalacticRegions.getIdxFromName(codexEntry.Region);

                                cmdrCodex.trackCodex(codexEntry.Name_Localised, codexEntry.EntryID, codexEntry.timestamp, codexEntry.SystemAddress, codexEntry.BodyID, galacticRegionId);
                            }
                        }
                    }
                    else
                    {
                        // for pre-Odyssey - we're only going to process Codex events for first finds
                        double[] lastStarPos = new double[0];
                        var lastBodyId = 0;
                        foreach (var entry in journal.Entries)
                        {
                            if (this.cancelprocessing) break;

                            if (entry.@event == nameof(FSDJump))
                            {
                                countJumps++;
                                countDistance += ((FSDJump)entry).JumpDist;
                            }
                            if (entry.@event == nameof(ApproachBody))
                            {
                                countBodies++;
                                lastBodyId = ((ApproachBody)entry).BodyID;
                            }
                            if (entry.@event == nameof(ScanOrganic) && ((ScanOrganic)entry).ScanType == ScanType.Analyse) countOrganisms++;
                            if (entry.@event == nameof(LeaveBody))
                                lastBodyId = 0;
                            if (entry.@event == nameof(Location))
                                lastBodyId = ((Location)entry).BodyID;

                            // take the last StarPos value from anything that has it
                            var propStarPos = entry.GetType().GetProperty("StarPos");
                            if (propStarPos != null)
                            {
                                var starPos = propStarPos.GetValue(entry) as double[];
                                if (starPos?.Length > 0)
                                    lastStarPos = starPos;
                            }

                            // check/track if this is a first discovery
                            var codexEntry = entry as SrvSurvey.CodexEntry;
                            if (codexEntry != null)
                            {
                                var galacticRegionId = lastStarPos.Length > 0
                                    ? EliteDangerousRegionMap.RegionMap.FindRegion(lastStarPos).Id
                                    : GalacticRegions.getIdxFromName(codexEntry.Region);

                                //if (codexEntry.SubCategory_Localised == "Organic structures" && (codexEntry.BodyID == null || codexEntry.BodyID == 0) && lastBodyId == 0)
                                //    Debugger.Break(); // TODO: Does this ever happen?

                                cmdrCodex.trackCodex(codexEntry.Name_Localised, codexEntry.EntryID, codexEntry.timestamp, codexEntry.SystemAddress, codexEntry.BodyID ?? lastBodyId, galacticRegionId);
                            }

                        }
                    }

                    SystemData.CloseAll();
                }
            }
            finally
            {
                this.processingFiles = false;
                this.cancelprocessing = false;

                this.BeginInvoke(new Action(() =>
                {
                    var elapsed = DateTime.Now - startTime;
                    lblProgress.Text = $"Final entry: from: {lastDate}, elapsed: {elapsed.ToString("mm\\:ss")}";
                    txtJumpCount.Text = countJumps.ToString("N0");
                    txtDistance.Text = countDistance.ToString("N0");
                    txtBodyCount.Text = countBodies.ToString("N0");
                    txtOrgCount.Text = countOrganisms.ToString("N0");

                    btnStart.Text = "Begin";
                    btnStart.Enabled = true;
                    btnClose.Enabled = true;
                    btnSystems.Enabled = true;
                    progress.Value = progress.Maximum;
                    comboCmdr.Enabled = true;
                    dateTimePicker.Enabled = true;
                    btnLongAgo.Enabled = true;
                    Data.suppressLoadingMsg = false;
                }));
            }
        }

        private void postProcessSystems()
        {
            this.processingSystems = true;
            var summaries = new Dictionary<string, SpeciesSummary>();

            try
            {
                // get list of files to process
                var folder = Path.Combine(Program.dataFolder, "systems", this.targetCmdrFid!);
                var files = Directory.GetFiles(folder, "*.json");

                var lastDate = "";
                var countJumps = 0;
                var countBodies = 0;
                var countOrganisms = 0;


                // read each system file...
                for (var n = 0; n < files.Length; n++)
                {
                    if (this.cancelprocessing) break;
                    this.BeginInvoke(new Action(() =>
                    {
                        if (this.IsDisposed) return;

                        lblProgress.Text = $"Processing {n} of {files.Length} system files ... (jumps: {countJumps}, bodies: {countBodies}, organisms: {countOrganisms}) {lastDate}";
                        progress.Maximum = files.Length;
                        progress.Value = n;
                    }));

                    // skip if wrong cmdr
                    var filepath = files[n];
                    var sysData = SystemData.Load<SystemData>(filepath)!;

                    countOrganisms += sysData.bodies.Sum(_ => _.organisms?.Count ?? 0);

                    foreach (var body in sysData.bodies)
                    {
                        if (body.organisms == null || body.organisms.Count == 0) continue;

                        foreach (var org in body.organisms)
                        {
                            if (org.speciesLocalized == null) continue;

                            var key = org.speciesLocalized;
                            var summary = summaries.GetValueOrDefault(key);
                            if (summary == null)
                            {
                                summary = new SpeciesSummary() { name = key };
                                summaries.Add(key, summary);
                            }

                            summary.count++;

                            if (body.atmosphereComposition != null)
                            {
                                var parts = string.Join(',', body.atmosphereComposition.Keys);
                                if (!summary.atmosComps.ContainsKey(parts)) summary.atmosComps.Add(parts, 0);
                                summary.atmosComps[parts]++;
                            }
                        }
                    }

                    //genusCounts = genusCounts.OrderBy(_ => _.Key).ToDictionary();
                    //var txt = new StringBuilder();
                    //foreach(var key in genusCounts.Keys.Order())
                    //{
                    //    txt.AppendLine($"{key}:");
                    //    var val = genusCounts[key];
                    //    txt.AppendLine($"\t{val.}");

                    //}

                    SystemData.Close(sysData);
                    //this.BeginInvoke(new Action(() =>
                    //{
                    //    try
                    //    {
                    //        var json = JsonConvert.SerializeObject(summaries, Formatting.Indented);
                    //        textBox1.Text = json;
                    //    }
                    //    catch { }
                    //}));
                }
            }
            finally
            {
                this.processingSystems = false;
                this.cancelprocessing = false;

                this.BeginInvoke(new Action(() =>
                {
                    var json = JsonConvert.SerializeObject(summaries.Values.OrderBy(_ => _.name), Formatting.Indented);
                    //textBox1.Text = json;

                    //lblProgress.Text = $"Completed journal processing.";
                    btnSystems.Text = "Begin";
                    btnStart.Enabled = true;
                    btnClose.Enabled = true;
                    progress.Value = progress.Minimum;
                }));
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki/Old-Journal-Processor");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    class SpeciesSummary
    {
        public string name;
        public int count;
        public Dictionary<string, int> atmosComps = new Dictionary<string, int>();

    }
}
