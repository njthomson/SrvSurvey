using SrvSurvey.game;
using static SrvSurvey.game.CommanderJourney;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormJourneyViewer : SizableForm
    {
        public static bool loadJourney(CommanderJourney journey)
        {
            var prior = BaseForm.get<FormJourneyViewer>();
            if (prior?.isDirty == true)
            {
                prior.Activate();
                var rslt = MessageBox.Show("Save changes before switching journeys?", "SrvSurvey", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (rslt == DialogResult.Cancel)
                    return true;

                if (rslt == DialogResult.Yes)
                    prior.btnSave_Click(null!, null!);
            }

            BaseForm.show<FormJourneyViewer>(viewer =>
            {
                viewer.journey = journey;
            });

            return false;
        }

        private CommanderJourney journey;
        private ViewJourneySystem viewSys;

        public FormJourneyViewer()
        {
            InitializeComponent();
            listSystems.Items.Clear();

            this.viewSys = new ViewJourneySystem(this)
            {
                Dock = DockStyle.Fill,
            };
            panelSystem.Controls.Add(this.viewSys);

            menuGalacticTime.Checked = Game.settings.viewJourneyGalacticTime;
            menuTopMost.Checked = Game.settings.viewJourneyTopMost;
            this.TopMost = Game.settings.viewJourneyTopMost;

            //Util.applyTheme(this);
        }

        protected override void beforeShowing()
        {
            base.beforeShowing();

            // load current journey 
            if (this.journey == null)
                this.journey = Game.activeGame?.journey ?? CommanderSettings.LoadCurrentOrLast(true)!.loadActiveJourney()!;
            if (this.journey == null)
                throw new Exception("Why no active journey?");

            this.refresh();

            if (journey.endTime == null)
                tabControl1.SelectTab(1);
        }

        /// <summary>
        /// Call to update in real-time when relevant events happen
        /// </summary>
        public void refresh()
        {
            Game.log("FormJourneyViewer.refresh");

            this.Text = "Journey: " + journey.name;
            this.txtJourneyName.Text = journey.name;
            this.txtDescription.Text = journey.description;

            var duration = DateTime.Now - journey.startTime.LocalDateTime;
            var startedOn = journey.startTime.ToCmdrShortDateTime24Hours();
            this.txtByline.Text = $"CMDR {journey.commander}  |  Set out: {startedOn}, {duration.TotalDays:n0} days ago";

            prepQuickStats();
            prepListSystems();
            viewSys.refreshTexts();
        }

        private void prepQuickStats()
        {
            listQuickStats.Items.Clear();
            foreach (var (label, value) in journey.getQuickStats())
                listQuickStats.Items.Add(label).SubItems.Add(value);

            listQuickStats.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            listQuickStats.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void prepListSystems()
        {
            // order most recent first
            var visitedSystems = journey.visitedSystems.OrderByDescending(sys => sys.arrived).ToList();

            // collect which systems have any notes or photos
            var hasAnyNotes = new HashSet<string>();
            var hasAnyPictures = new HashSet<string>();
            foreach (var sys in visitedSystems)
            {
                if (sys.count.notes > 0) hasAnyNotes.Add(sys.starRef.name);
                if (sys.count.screenshots > 0) hasAnyPictures.Add(sys.starRef.name);
            }

            // walk through journey list, updating/inserting as needed
            for (int idx = 0; idx < visitedSystems.Count; idx++)
            {
                var sys = visitedSystems[idx];
                var listItem = idx < listSystems.Items.Count ? listSystems.Items[idx] : null;

                // insert an item if not a match...
                if (sys != listItem?.Tag as SystemStats)
                {
                    listItem = listSystems.Items.Insert(idx, new ListViewItem()
                    {
                        Text = sys.starRef.name,
                        Name = $"{sys.starRef.name}_{sys.arrived}",
                        Tag = sys
                    });
                    listItem.SubItems.Add("");
                }

                // TODO: a proper mechanism to classify which systems are interesting
                var txt = "";
                if (sys.count.screenshots > 0)
                    txt += $"P";
                else if (hasAnyPictures.Contains(sys.starRef.name))
                    txt += "p";
                if (sys.count.organic > 0)
                    txt += $"B";
                if (sys.count.notes > 0)
                    txt += $"N";
                else if (hasAnyNotes.Contains(sys.starRef.name))
                    txt += "n";
                if (sys.count.codexNew > 0)
                    txt += $"C";
                if (sys.count.touchdown > 0)
                    txt += $"T";

                if (txt.Length > 0)
                    listItem.SubItems[1].Text = txt.Trim();
            }

            listSystems.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);

            if (listSystems.SelectedIndices.Count == 0)
                listSystems.Items[0].Selected = true;
        }

        private void listSystems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listSystems.SelectedItems.Count == 0)
            {
                this.viewSys.Hide();
            }
            else
            {
                this.viewSys.setSystem((SystemStats)listSystems.SelectedItems[0].Tag!);
                this.viewSys.Show();
            }
        }

        private void journey_TextChanged(object sender, EventArgs e)
        {
            updateSaveVisible();
        }

        public bool isDirty => viewSys.dirty || txtDescription.Text != journey.description || txtJourneyName.Text != journey.name;

        public void updateSaveVisible()
        {
            // show the save button if system view is dirty or our two text boxes have changed
            var dirty = this.isDirty;
            menuSaveUpdates.Visible = dirty;
            menuDiscard.Visible = dirty;
            menuConclude.Enabled = !dirty;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // update the journey?
            if (txtDescription.Text != journey.description || txtJourneyName.Text != journey.name)
            {
                Game.log($"Updating journey name/description");
                journey.name = txtJourneyName.Text;
                journey.description = txtDescription.Text;
                journey.Save();
            }

            // update the system?
            if (viewSys.dirty)
                viewSys.saveUpdates();

            this.updateSaveVisible();
        }

        private void menuDiscard_ButtonClick(object sender, EventArgs e)
        {
            // update the journey?
            if (txtDescription.Text != journey.description || txtJourneyName.Text != journey.name)
            {
                Game.log($"Discarding journey name/description");
                txtJourneyName.Text = journey.name;
                txtDescription.Text = journey.description;
            }

            // update the system?
            if (viewSys.dirty)
                viewSys.discardChanges();

            this.updateSaveVisible();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, "This is a dev diagnostic measure. Are you sure you want to re-process journal entries?\r\n\r\nThis might take a while and will remove any tags for Notes or Photos taken in a system", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.Yes)
            {
                this.Controls.Cast<Control>().ToList().ForEach(ctrl => ctrl.Enabled = false);

                journey.visitedSystems.Clear();
                journey.watermark = journey.startTime;

                var journal = new JournalFile(Path.Combine(JournalFile.journalFolder, journey.startingJournal), journey.fid);
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

                var cmdr = CommanderSettings.LoadCurrentOrLast(true)!;
                cmdr.activeJourney = null;
                cmdr.Save();

                this.Close();
            }
        }

        private void menuTopMost_Click(object sender, EventArgs e)
        {
            Game.settings.viewJourneyTopMost = menuTopMost.Checked;
            Game.settings.Save();
            this.TopMost = Game.settings.viewJourneyTopMost;
        }

        private void menuGalacticTime_Click(object sender, EventArgs e)
        {
            Game.settings.viewJourneyGalacticTime = menuGalacticTime.Checked;
            Game.settings.Save();
            this.refresh();

            viewSys.refreshTexts();
        }
    }
}
