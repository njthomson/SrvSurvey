using Newtonsoft.Json;
using SrvSurvey.game;
using System.ComponentModel;
using System.Diagnostics;

namespace SrvSurvey
{
    public partial class FormPostProcess : Form
    {
        public static FormPostProcess? activeForm;

        public static void show()
        {
            if (activeForm == null)
                FormPostProcess.activeForm = new FormPostProcess();

            Util.showForm(FormPostProcess.activeForm);
        }

        private Dictionary<string, string> allCmdrs;

        private string targetCmdrName;
        private string targetCmdrFid;
        private DateTime targetStartTime = DateTime.MinValue;

        private bool processingFiles = false;
        private bool processingSystems = false;
        private bool cancelprocessing = false;

        public long lastSystemAddress;
        public int? lastBodyId;

        private FormPostProcess()
        {
            InitializeComponent();
            Util.useLastLocation(this, Game.settings!.formPostProcess, true);

            // load potential cmdr's
            this.allCmdrs = CommanderSettings.getAllCmdrs();
            this.findCmdrs();

            // default to 7 days ago, at midnight
            dateTimePicker.Value = DateTime.Now.Subtract(new TimeSpan(6, 23, 59, 59) + DateTime.Now.TimeOfDay);
            this.targetStartTime = dateTimePicker.Value;

            btnStart.Enabled = !Elite.isGameRunning;
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

        private void findCmdrs()
        {
            var cmdrs = this.allCmdrs
                .Values
                .Order()
                .ToArray();

            comboCmdr.Items.Clear();
            comboCmdr.Items.AddRange(cmdrs);

            if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
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

                Task.Run(() => Program.crashGuard(() => this.postProcessJournals()));
            }
            else
            {
                this.cancelprocessing = true;
                btnStart.Text = "Stopping";
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

        private void postProcessJournals()
        {
            this.processingFiles = true;
            var lastDate = "";
            var countJumps = 0;
            var countBodies = 0;
            var countOrganisms = 0;
            try
            {
                // get list of files to process, date filtered and ordered oldest first
                var files = Directory.GetFiles(JournalFile.journalFolder, "*.log")
                    .Where(filepath => File.GetLastWriteTime(filepath) > this.targetStartTime)
                    .OrderBy(filepath => File.GetLastWriteTime(filepath))
                    .ToArray();

                var cmdr = CommanderSettings.Load(this.targetCmdrFid!, true, this.targetCmdrName!);
                var cmdrCodex = CommanderCodex.Load(this.targetCmdrFid!, this.targetCmdrName!);

                this.Invoke(new Action(() =>
                {
                    lblProgress.Text = $"Processing #{0} of {files.Length} journal files ... (~?, jumps: {countJumps}, approached bodies: {countBodies}, organisms scanned: {countOrganisms})";
                    progress.Maximum = files.Length;
                    progress.Value = 0;
                }));

                // read each journal file...
                for (var n = 0; n < files.Length; n++)
                {
                    this.Invoke(new Action(() =>
                    {
                        lblProgress.Text = $"Processing #{n} of {files.Length} journal files ... (~{lastDate}, jumps: {countJumps}, approached bodies: {countBodies}, organisms scanned: {countOrganisms})";
                        progress.Maximum = files.Length;
                        progress.Value = n;

                        if (this.cancelprocessing)
                            lblProgress.Text = "Stopped. " + lblProgress.Text;
                    }));

                    if (Elite.isGameRunning) this.cancelprocessing = true;
                    if (this.cancelprocessing) break;

                    // skip if wrong cmdr
                    var filepath = files[n];

                    // skip if before timestamp
                    var lastWriteTime = File.GetLastWriteTime(filepath);
                    if (lastWriteTime < this.targetStartTime) continue;

                    var journal = new JournalFile(filepath, this.targetCmdrName);
                    if (journal.cmdrName != this.targetCmdrName) continue;
                    lastDate = journal.timestamp.ToShortDateString();

                    if (journal.isOdyssey)
                    {
                        // if Odyssey ... process all exploration events
                        SystemData? sysData = null;
                        foreach (var entry in journal.Entries)
                        {
                            if (entry.@event == nameof(FSDJump)) countJumps++;
                            if (entry.@event == nameof(ApproachBody)) countBodies++;
                            if (entry.@event == nameof(ScanOrganic) && ((ScanOrganic)entry).ScanType == ScanType.Analyse) countOrganisms++;

                            var starterEvent = entry as ISystemDataStarter;
                            if (starterEvent != null)
                            {
                                if (sysData != null)
                                {
                                    sysData.Save();
                                    SystemData.Close(sysData);
                                }

                                sysData = SystemData.From(starterEvent, this.targetCmdrFid, this.targetCmdrName);
                            }

                            var locationEvent = entry as SrvSurvey.Location;
                            if (sysData != null && locationEvent != null && locationEvent.SystemAddress != sysData.address)
                            {
                                sysData.Save();
                                SystemData.Close(sysData);
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
                        foreach (var entry in journal.Entries)
                        {
                            if (entry.@event == nameof(FSDJump)) countJumps++;
                            if (entry.@event == nameof(ApproachBody)) countBodies++;
                            if (entry.@event == nameof(ScanOrganic) && ((ScanOrganic)entry).ScanType == ScanType.Analyse) countOrganisms++;

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

                                if (codexEntry.SubCategory_Localised == "Organic structures" && (codexEntry.BodyID == null || codexEntry.BodyID == 0))
                                    Debugger.Break(); // TODO: Does this ever happen?

                                cmdrCodex.trackCodex(codexEntry.Name_Localised, codexEntry.EntryID, codexEntry.timestamp, codexEntry.SystemAddress, codexEntry.BodyID, galacticRegionId);
                            }

                        }
                    }
                }
            }
            finally
            {
                this.processingFiles = false;
                this.cancelprocessing = false;

                this.BeginInvoke(new Action(() =>
                {
                    //lblProgress.Text = $"Completed journal processing.";
                    lblProgress.Text = $"Final tally: ~{lastDate}, jumps: {countJumps}, approached bodies: {countBodies}, organisms scanned: {countOrganisms}";

                    btnStart.Text = "Begin";
                    btnSystems.Enabled = true;
                    progress.Value = progress.Minimum;
                    comboCmdr.Enabled = true;
                    dateTimePicker.Enabled = true;
                    btnLongAgo.Enabled = true;
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
                    progress.Value = progress.Minimum;
                }));
            }
        }
    }

    class SpeciesSummary
    {
        public string name;
        public int count;
        public Dictionary<string, int> atmosComps = new Dictionary<string, int>();

    }
}
