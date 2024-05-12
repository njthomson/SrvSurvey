using Newtonsoft.Json;
using SrvSurvey.game;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

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

        private string? targetCmdrName = "grinning2001"; // Game.settings?.lastCommander;
        private string? targetCmdrFid = "F6985613"; // Game.settings?.lastFid;

        private bool processingFiles = false;
        private bool processingSystems = false;
        private bool cancelprocessing = false;

        private FormPostProcess()
        {
            InitializeComponent();
            Util.useLastLocation(this, Game.settings!.formPostProcess);
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
            FormGenus.activeForm = null;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!this.processingFiles)
            {
                btnSystems.Enabled = false;
                btnStart.Text = "Stop";
                // up to 813 of 2349
                Task.Run(() => Program.crashGuard(() => this.postProcessJournals()));
            }
            else
            {
                this.cancelprocessing = true;
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
            try
            {
                // get list of files to process
                var files = Directory.GetFiles(JournalFile.journalFolder, "*.log");

                var lastDate = "";
                var countJumps = 0;
                var countBodies = 0;
                var countOrganisms = 0;
                var cmdr = CommanderSettings.Load(this.targetCmdrFid!, true, this.targetCmdrName!);

                // read each journal file...
                for (var n = 0; n < files.Length; n++)
                {
                    if (this.cancelprocessing) break;
                    this.BeginInvoke(new Action(() =>
                    {
                        lblProgress.Text = $"Processing {n} of {files.Length} journal files ... (jumps: {countJumps}, bodies: {countBodies}, organisms: {countOrganisms}) {lastDate}";
                        progress.Maximum = files.Length;
                        progress.Value = n;
                    }));

                    // skip if wrong cmdr
                    var filepath = files[n];
                    var journal = new JournalFile(filepath, this.targetCmdrName);
                    if (journal.CommanderName != this.targetCmdrName) continue;
                    lastDate = $" ({journal.timestamp.ToShortDateString()})";
                    if (!journal.isOdyssey) continue;

                    // process exploration events
                    SystemData? sysData = null;
                    foreach (var entry in journal.Entries)
                    {
                        var starterEvent = entry as ISystemDataStarter;
                        if (starterEvent != null) // && starterEvent.@event != nameof(Location))
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

                        if (entry.@event == nameof(FSDJump)) countJumps++;
                        if (entry.@event == nameof(ApproachBody)) countBodies++;
                        if (entry.@event == nameof(ScanOrganic) && ((ScanOrganic)entry).ScanType == ScanType.Analyse) countOrganisms++;
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
                    btnStart.Text = "Begin";
                    btnSystems.Enabled = true;
                    progress.Value = progress.Minimum;
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
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            var json = JsonConvert.SerializeObject(summaries, Formatting.Indented);
                            textBox1.Text = json;
                        }
                        catch { }
                    }));
                }
            }
            finally
            {
                this.processingSystems = false;
                this.cancelprocessing = false;

                this.BeginInvoke(new Action(() =>
                {
                    var json = JsonConvert.SerializeObject(summaries.Values.OrderBy(_ => _.name), Formatting.Indented);
                    textBox1.Text = json;

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
