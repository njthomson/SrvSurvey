using SrvSurvey.game;

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
            prepQuickStats();

            Util.applyTheme(this);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void prepQuickStats()
        {
            listQuickStats.Items.Clear();

            foreach(var (label, value) in journey.getQuickStats())
                listQuickStats.Items.Add(label).SubItems.Add(value);

            listQuickStats.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
        }
    }
}
