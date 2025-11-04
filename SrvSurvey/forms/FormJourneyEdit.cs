using SrvSurvey.game;


namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormJourneyEdit : FixedForm
    {
        private CommanderJourney journey;
        public FormJourneyEdit()
        {
            InitializeComponent();
            this.journey = Game.activeGame?.journey ?? CommanderSettings.LoadCurrentOrLast(true)!.loadActiveJourney()!;
            if (this.journey == null) throw new Exception("Why no active journey?");

            updateFormFromJourney();

            Util.applyTheme(this);
        }

        private void updateFormFromJourney()
        {
            txtName.Text = journey.name;
            txtDescription.Text = journey.description;
        }

        private void updateJourney()
        {
            journey.name = txtName.Text;
            journey.description = txtDescription.Text;

            journey.Save();
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            this.updateJourney();
            this.Close();
        }


    }
}
