using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;

namespace SrvSurvey.forms
{
    partial class FormNewProject : SizableForm
    {
        private Game game => Game.activeGame!;

        public FormNewProject()
        {
            InitializeComponent();
            this.Icon = Icons.ruler;

            var allCosts = Game.colony.loadDefaultCosts();
            var sourceCosts = allCosts
                .Where(r => (r.location == "orbital") == (game.status.PlanetRadius == 0))
                .ToDictionary(c => c.buildType, c => c.ToString());
            this.comboBuildType.DataSource = new BindingSource(sourceCosts, null);
            this.comboBuildType.DisplayMember = "Value";
            this.comboBuildType.ValueMember = "Key";
            this.comboBuildType.Enabled = true;
            this.comboBuildType.DropDownWidth = N.s(500);

            txtArchitect.Text = game.Commander;

            var lastDocked = game.lastDocked;
            if (lastDocked != null)
                txtName.Text = ColonyData.getDefaultProjectName(lastDocked);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            createProject();
        }

        private void createProject()
        {
            var lastDocked = game.lastDocked;
            if (lastDocked == null || game == null) return;

            var lastDepot = game.lastColonisationConstructionDepot;
            if (lastDepot == null)
            {
                Game.log("No game.lastColonisationConstructionDepot");
                MessageBox.Show(this, "Required items not known. Please enter Construction Services and try again.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

            try
            {
                var createProject = new ProjectCreate
                {
                    buildType = comboBuildType.SelectedValue?.ToString() ?? "",
                    buildName = txtName.Text ?? "primary-port",
                    architectName = txtArchitect.Text,
                    factionName = lastDocked.StationFaction.Name ?? "",
                    notes = txtNotes.Text,
                    isPrimaryPort = lastDocked.StationName.StartsWith(ColonyData.ExtPanelColonisationShip),

                    marketId = lastDocked.MarketID,
                    systemAddress = lastDocked.SystemAddress,
                    systemName = lastDocked.StarSystem,
                    starPos = game.systemData!.starPos,
                    bodyNum = game.systemBody?.id,
                    bodyName = game.systemBody?.name,

                    colonisationConstructionDepot = lastDepot,

                    // add current cmdrs
                    commanders = new() { { game.Commander!, new() } },                    
                };

                if (lastDepot != null && game.lastDocked != null && lastDepot.MarketID == game.lastDocked.MarketID)
                {
                    var needed = lastDepot.ResourcesRequired.ToDictionary(r => r.Name.Substring(1).Replace("_name;", ""), r => r.RequiredAmount - r.ProvidedAmount);
                    createProject.commodities = needed;
                    createProject.maxNeed = lastDepot.ResourcesRequired.Sum(r => r.RequiredAmount);
                }

                Game.log($"Creating: '{createProject.buildName}' in '{createProject.systemName}' ({createProject.systemAddress}/{createProject.marketId})");

                this.setChildrenEnabled(false, btnCancel);

                Game.colony.create(createProject).continueOnMain(this, newProject =>
                {
                    if (newProject == null)
                    {
                        // fetch latest - maybe someone just created the project before us?
                        game.cmdrColony.fetchLatest().justDoIt();

                        MessageBox.Show(this, "Project creation did not succeed. See logs for more details", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                        return;
                    }

                    Game.log($"New build created: {newProject.buildId}");

                    // open edit page in browser
                    Util.openLink($"{ColonyNet.uxUri}/#build={newProject.buildId}");

                    game.cmdrColony.fetchLatest().continueOnMain(this, () =>
                    {
                        this.Close();
                    });
                }, true);
            }
            catch (Exception ex)
            {
                FormErrorSubmit.Show(ex);
                return;
            }
        }
    }
}
