using SrvSurvey.game;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormJourneyList : SizableForm
    {
        private List<CommanderJourney> journeys = new();

        public FormJourneyList()
        {
            InitializeComponent();
            btnOpen.Enabled = false;
        }

        private void FormJourneyList_Load(object sender, EventArgs e)
        {
            Task.Run(() => Program.crashGuard(() => this.doLoadJourneys()));
        }

        private void doLoadJourneys()
        {
            var fid = CommanderSettings.currentOrLastFid;
            var folder = Path.Combine(CommanderJourney.dataFolder, fid);
            var files = Directory.GetFiles(folder, "*.json");

            foreach (var filepath in files)
            {
                var filename = Path.GetFileNameWithoutExtension(filepath);
                var journey = CommanderJourney.Load(fid, filename);
                if (journey != null)
                    journeys.Add(journey);
            }

            Program.defer(() =>
            {
                if (this.IsDisposed) return;

                list.Items.Clear();
                var first = true;
                foreach (var journey in journeys)
                {
                    var item = list.Items.Add(journey.name, journey.name, journey);
                    item.SubItems.Add(journey.startTime.ToCmdrShortDateTime24Hours());
                    item.SubItems.Add(journey.watermark.ToCmdrShortDateTime24Hours());

                    if (first) item.Selected = true;
                    first = false;
                }
            });
        }

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSummary.Text = "";
            if (list.SelectedItems != null && list.SelectedItems.Count > 0)
            {
                var journey = list.SelectedItems[0].Tag as CommanderJourney;
                if (journey != null)
                {
                    txtSummary.Text = string.IsNullOrWhiteSpace(journey.description) ? "(no description)" : journey.description;
                    btnOpen.Enabled = true;
                    return;
                }
            }
            btnOpen.Enabled = false;
        }

        private void openSelectedJourney()
        {
            if (list.SelectedItems == null || list.SelectedItems.Count == 0) return;

            var journey = list.SelectedItems[0].Tag as CommanderJourney;
            if (journey == null) return;

            var cancelled = FormJourneyViewer.loadJourney(journey);
            if (cancelled)
                this.Activate();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openSelectedJourney();
        }

        private void list_DoubleClick(object sender, EventArgs e)
        {
            openSelectedJourney();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
