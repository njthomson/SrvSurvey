using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.plotters;
using SrvSurvey.Properties;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormMyProjects : SizableForm
    {
        private static List<ColonyCost2> allCosts = Game.rcc.loadDefaultCosts();

        private Game game => Game.activeGame!;
        private readonly string cmdr;

        private HashSet<string> hiddenIDs;
        private List<Project> projects;

        public FormMyProjects()
        {
            this.Icon = Icons.set_square;
            InitializeComponent();

            this.cmdr = CommanderSettings.currentOrLastCmdrName!;
            if (game?.cmdrColony != null)
                setData(game.cmdrColony.hiddenIDs, game.cmdrColony.projects);

            BaseForm.applyThemeWithCustomControls(this);
        }

        private void FormMyProjects_Load(object sender, EventArgs e)
        {
            this.loadData().justDoIt();
        }

        private async Task loadData()
        {
            btnRefresh.Enabled = false;
            btnSave.Enabled = false;
            PlotBuildCommodities.startPending();

            // get hidden IDs first
            var buildIDs = await Game.rcc.getHiddenIDs(cmdr);

            await Game.rcc.getCmdrActive(cmdr).continueOnMain(this, data => setData(buildIDs, data));
        }

        private void setData(List<string> buildIDs, List<Project> projects)
        {
            btnRefresh.Enabled = false;
            btnSave.Enabled = false;
            try
            {
                this.hiddenIDs = buildIDs.ToHashSet();
                if (game?.cmdrColony != null) game.cmdrColony.hiddenIDs = buildIDs;
                this.projects = projects;

                list.Items.Clear();

                var sorted = projects.OrderBy(p => $"{p.systemName}~${p.buildName}");
                foreach (var proj in sorted)
                {
                    var match = allCosts.Find(c => c.layouts.Contains(proj.buildType, StringComparer.OrdinalIgnoreCase));
                    var typeTxt = match == null ? "?" : $"{match.displayName} ({proj.buildType})";
                    var remaining = proj.maxNeed - proj.sumNeed;
                    var p = 1f / proj.maxNeed * remaining;

                    var item = list.Items.Add(proj.buildName, proj.buildId, proj);
                    item.SubItems.Add($"{p:P0} of {proj.maxNeed:N0}");
                    item.SubItems.Add(typeTxt);
                    item.SubItems.Add(proj.systemName);

                    item.Checked = !hiddenIDs.Contains(proj.buildId);
                }
                list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                updateTotals();
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnSave.Enabled = true;
                list.Enabled = true;

                if (game != null && PlotBuildCommodities.allowed(game))
                    PlotBuildCommodities.endPending();
            }
        }

        private void updateTotals()
        {
            var totalRemaining = 0;
            foreach (ListViewItem item in list.Items)
            {
                var proj = item.Tag as Project;
                if (!item.Checked || proj == null) continue;

                totalRemaining += proj.sumNeed;
            }

            lblTotals.Text = $"Cargo required: {totalRemaining:N0}";
            if (game?.currentShip.cargoCapacity > 0)
            {
                var tripsNeeded = Math.Ceiling(totalRemaining / (double)game.currentShip.cargoCapacity);
                lblTotals.Text += $" | {tripsNeeded:N0} trips in current ship";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            btnRefresh.Enabled = false;
            btnSave.Enabled = false;
            PlotBuildCommodities.startPending();

            Game.rcc.setHiddenIDs(this.cmdr, this.hiddenIDs).continueOnMain(this, buildIDs =>
            {
                setData(buildIDs, this.projects);
                this.Close();
            });
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.loadData().justDoIt();
        }

        private void list_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!btnRefresh.Enabled) return;
            var proj = e.Item.Tag as Project;
            if (proj == null) return;

            if (e.Item.Checked)
                hiddenIDs.Remove(proj.buildId);
            else
                hiddenIDs.Add(proj.buildId);

            updateTotals();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://ravencolonial.com/build");
        }
    }
}
