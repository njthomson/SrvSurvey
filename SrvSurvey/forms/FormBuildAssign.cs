using SrvSurvey.game;

namespace SrvSurvey.forms
{
    public partial class FormBuildAssign : Form
    {
        private string buildId;
        private string cmdr;

        public FormBuildAssign(string buildId, string cmdr, List<string> commodities)
        {
            this.buildId = buildId;
            this.cmdr = cmdr;

            InitializeComponent();

            txtCmdr.Text = cmdr;
            comboCommodity.Items.AddRange(commodities.ToArray());
        }

        private void btnAssign_Click(object sender, EventArgs e)
        {
            var commodity = comboCommodity.SelectedItem?.ToString();
            if (commodity != null)
            {
                Game.colony.assign(buildId, cmdr, commodity).continueOnMain(this, () =>
                {
                    this.Close();
                });
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var commodity = comboCommodity.SelectedItem?.ToString();
            if (commodity != null)
            {
                Game.colony.unAssign(buildId, cmdr, commodity).continueOnMain(this, () =>
                {
                    this.Close();
                });
            }
        }
    }
}
