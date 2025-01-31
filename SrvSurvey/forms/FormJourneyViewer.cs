using SrvSurvey.game;
using static SrvSurvey.game.CommanderJourney;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormJourneyViewer : SizableForm
    {
        private CommanderJourney journey;

        public FormJourneyViewer()
        {
            InitializeComponent();
            this.journey = Game.activeGame?.journey ?? CommanderSettings.LoadCurrentOrLast().loadActiveJourney()!;
            if (this.journey == null) throw new Exception("Why no active journey?");

            this.Text += journey.name;

            txtJourneyName.Text = journey.name;
            txtDescription.Text = journey.description;
            var duration = DateTime.Now - journey.startTime.LocalDateTime;
            var startedOn = journey.startTime.LocalDateTime.AddYears(1286);
            txtByline.Text = $"CMDR {journey.commander}  |  Set out: {startedOn.Date.ToShortDateString()}  |  {duration.TotalDays:n0} days ago";

            prepQuickStats();
            prepListSystems();

            if (journey.endTime == null)
                tabControl1.SelectTab(1);

            //Util.applyTheme(this);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void prepQuickStats()
        {
            listQuickStats.Items.Clear();

            foreach (var (label, value) in journey.getQuickStats())
                listQuickStats.Items.Add(label).SubItems.Add(value);

            listQuickStats.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void prepListSystems()
        {
            listSystems.Items.Clear();

            foreach (var sys in journey.visitedSystems.OrderByDescending(sys => sys.arrived))
            {
                var item = listSystems.Items.Add(sys.starRef.name);
                item.Name = $"{sys.starRef.name}_{sys.arrived}";
                item.Tag = sys;

                // TODO: a proper mechanism to classify which systems are interesting
                var txt = "";
                if (sys.count.screenshots > 0)
                    txt += $"P";
                if (sys.count.organic > 0)
                    txt += $"B";
                if (sys.count.notes > 0)
                    txt += $"N";
                if (sys.count.codexNew > 0)
                    txt += $"C";
                if (sys.count.touchdown > 0)
                    txt += $"T";

                if (txt.Length > 0)
                    item.SubItems.Add(txt.Trim());
            }

            listSystems.Items[0].Selected = true;
        }

        private void listSystems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listSystems.SelectedItems.Count == 0)
            {
                panelSystem.Controls.Clear();
            }
            else
            {
                var sys = listSystems.SelectedItems[0].Tag as SystemStats;
                if (sys == null) return;

                ViewJourneySystem viewSys = null!;
                if (panelSystem.Controls.Count == 0)
                {
                    viewSys = new ViewJourneySystem()
                    {
                        Dock = DockStyle.Fill,
                    };
                    panelSystem.Controls.Add(viewSys);
                }
                else
                {
                    viewSys = (ViewJourneySystem)panelSystem.Controls[0];
                }

                viewSys.setSystem(sys);
            }
        }

        private void journey_TextChanged(object sender, EventArgs e)
        {
            btnSave.Visible = txtDescription.Text != journey.description || txtJourneyName.Text != journey.name;
            btnConclude.Visible = !btnSave.Visible;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            journey.name = txtJourneyName.Text;
            journey.description = txtDescription.Text;

            journey.Save();
            btnSave.Visible = false;
            btnConclude.Visible = true;
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, "Are you sure you want to re-process journal entries?\r\n\r\nThis might take a while.", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.Yes)
            {
                this.Controls.Cast<Control>().ToList().ForEach(ctrl => ctrl.Enabled = false);

                journey.visitedSystems.Clear();
                journey.watermark = journey.startTime;

                var journal = new JournalFile(Path.Combine(JournalFile.journalFolder, journey.startingJournal), journey.commander);
                journey.doCatchup(journal)
                    .continueOnMain(this, () =>
                    {
                        prepQuickStats();
                        prepListSystems();

                        this.Controls.Cast<Control>().ToList().ForEach(ctrl => ctrl.Enabled = true);
                    });
            }
        }

        private void btnConclude_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, "Are you sure you want conclude this journey?", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.Yes)
            {
                Game.log($"Concluding journey: {journey.name}");
                journey.endTime = DateTimeOffset.Now;
                journey.Save();

                var cmdr = CommanderSettings.LoadCurrentOrLast();
                cmdr.activeJourney = null;
                cmdr.Save();

                this.Close();
            }
        }
    }
}
