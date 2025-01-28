using SrvSurvey.game;

namespace SrvSurvey.forms
{
    [Draggable]
    internal partial class FormJourneyBegin : FixedForm
    {
        private CommanderSettings cmdr;
        private long startSystemAddress;
        private FSDJump? startingJump;
        private JournalFile? startingJournal;

        public FormJourneyBegin()
        {
            InitializeComponent();
            this.cmdr = CommanderSettings.LoadCurrentOrLast();
            txtCurrentSystem.Text = cmdr.currentSystem;
            comboStartFrom.Enabled = false;
            lblWarning.Text = null;
            btnAccept.Enabled = false;

            Util.applyTheme(this);
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            this.startSystemAddress = 0;
            this.Close();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            beginJourney();
        }

        private void beginJourney()
        {
            var name = txtName.Text;
            var start = DateTimeOffset.Now;

            // confirm valid name
            var rejectReason = CommanderJourney.nameIsValid(this.cmdr.fid, name);
            // confirm starting system is selected
            if (rejectReason == null && radioSystem.Checked && comboStartFrom.SelectedSystem == null)
                rejectReason = "No system selected";
            if (startingJump == null)
                rejectReason = "No starting jump?";
            if (startingJournal == null)
                rejectReason = "No startingJournal?";

            if (rejectReason != null)
            {
                setWarning(rejectReason);
                return;
            }
            // create journey file + update cmdr settings with that filename
            var journey = CommanderJourney.Create(this.cmdr.fid, this.cmdr.commander, name, startingJump!);
            journey.Save();

            Task.Run(() => journey.doProcessPastJournals(this.startingJournal!));

            if (Game.activeGame != null) Game.activeGame.journey = journey;
            this.cmdr.activeJourney = Path.GetFileNameWithoutExtension(journey.filepath);
            this.cmdr.Save();
            this.Close();
        }

        private void setWarning(string? txt)
        {
            lblWarning.Text = txt;
            lblWarning.Visible = !string.IsNullOrEmpty(lblWarning.Text);
            btnAccept.Enabled = !lblWarning.Visible;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            // make sure name has no filename illegal characters
            setWarning(CommanderJourney.nameIsValid(this.cmdr.fid, txtName.Text));
        }

        private void radioNow_CheckedChanged(object sender, EventArgs e)
        {
            comboStartFrom.Enabled = radioSystem.Checked;
            txtCurrentSystem.Enabled = !radioSystem.Checked;

            Util.deferAfter(100, () =>
            {
                long nextSystemAddress = 0;
                if (radioSystem.Checked)
                {
                    if (comboStartFrom.SelectedSystem == null)
                        comboStartFrom.SelectedSystem = cmdr.getCurrentStarRef();

                    nextSystemAddress = comboStartFrom.SelectedSystem.id64;
                    lblLastVisited.Text = $"Last visited '{comboStartFrom.SelectedSystem.name}' ...?";

                }
                else if (radioNow.Checked)
                {
                    nextSystemAddress = cmdr.currentSystemAddress;
                    lblLastVisited.Text = $"Last visited '{cmdr.currentSystem}' ...?";
                }

                if (startingJump == null || nextSystemAddress != startingJump?.SystemAddress)
                {
                    btnAccept.Enabled = false;
                    Task.Run(() => doFindLastTimeInSystem(nextSystemAddress));
                }
            });
        }

        private void comboStartFrom_selectedSystemChanged(SrvSurvey.units.StarRef starSystem)
        {
            var data = SystemData.Load(starSystem.name, starSystem.id64, cmdr.fid, true);
            if (data == null)
            {
                lblLastVisited.Visible = false;
                setWarning($"You nave not visited: {starSystem.name}");
                return;
            }

            setWarning(null);
            var duration = DateTimeOffset.Now - data.lastVisited;
            lblLastVisited.Text = $"Last visited '{starSystem.name}' {Util.timeSpanToString(duration)} ...?";
            lblLastVisited.Visible = true;

            btnAccept.Enabled = false;
            Util.deferAfter(1000, () =>
            {
                btnAccept.Enabled = false;
                Task.Run(() => doFindLastTimeInSystem(starSystem.id64));
            });
        }

        private FSDJump? doFindLastTimeInSystem(long id64)
        {
            this.startSystemAddress = id64;
            var myStartAddress = startSystemAddress;

            var filepath = JournalFile.getSiblingCommanderJournal(cmdr.commander, true, DateTime.Now);
            var journal = new JournalWatcher(filepath!);

            var countJumps = 0;
            this.startingJump = null;
            this.startingJournal = journal.walkDeep(true, (entry) =>
            {
                // exit early if we no longer need to search for this
                if (startSystemAddress == 0 || myStartAddress != startSystemAddress) return true;

                var fsdJumpEntry = entry as FSDJump;
                if (fsdJumpEntry != null)
                {
                    countJumps++;
                    if (fsdJumpEntry.SystemAddress == id64)
                    {
                        this.startingJump = fsdJumpEntry;
                        return true;
                    }
                }
                return false;
            });

            Program.defer(() =>
            {
                setWarning(null);
                if (this.startingJump != null)
                {
                    var txt = $"Last visited '{startingJump.StarSystem}' ";
                    if (countJumps > 1) txt += $"{countJumps} jumps, ";
                    lblLastVisited.Text = txt + Util.timeSpanToString(DateTimeOffset.Now - startingJump.timestamp);
                }
                else if (myStartAddress == startSystemAddress)
                    lblLastVisited.Text = "";
            });


            return this.startingJump;
        }
    }
}

