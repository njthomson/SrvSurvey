using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.Properties;
using SrvSurvey.widgets;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    partial class FormNewProject : SizableForm
    {
        private Game game => Game.activeGame!;

        private List<SystemSite> systemSites = new();
        private List<ColonyCost2> allCosts = Game.rcc.loadDefaultCosts()
            .OrderBy(c => c.tier)
            .ToList();

        public FormNewProject()
        {
            InitializeComponent();
            this.Icon = Icons.ruler;

            var isOrbital = game.status.PlanetRadius == 0;
            this.radioOrbital.Checked = isOrbital;
            this.radioSurface.Checked = !isOrbital;

            this.comboBuildType.Enabled = true;
            this.comboBuildType.DropDownWidth = N.s(500);

            // use map of bodyNum / name (type)
            var bodyMap = game.systemData!.bodies
                .Where(_ => _.type != SystemBodyType.Unknown && _.type != SystemBodyType.Barycentre && _.type != SystemBodyType.PlanetaryRing)
                .ToDictionary(b => b.id, b => $"{b.name} ({b.planetClass})");
            this.comboBody.DataSource = new BindingSource(bodyMap, null);
            if (game.systemBody != null)
                this.comboBody.SelectedValue = game.systemBody.id;

            txtArchitect.Text = game.Commander;

            if (game.lastDocked == null) return;

            txtName.Text = ColonyData.getDefaultProjectName(game.lastDocked);

            comboSystemSite.Text = "Loading ...";
            Game.rcc.getSystemSites(game.lastDocked.StarSystem).continueOnMain(this, sites =>
            {
                this.systemSites.Clear();
                this.systemSites.AddRange(sites);
                var mapSites = new Dictionary<string, string>() { { "", "None" } };

                foreach (var site in sites)
                    if (site.status != SystemSite.Status.complete)
                        mapSites.Add(site.id, $"{site.name} ({site.buildType})");

                comboSystemSite.DataSource = new BindingSource(mapSites, null);
                comboSystemSite.SelectedIndex = 0;
                comboSystemSite.Enabled = true;

                // pre-select if there is only one, drop the combo if there are choices
                if (mapSites.Count == 2)
                    comboSystemSite.SelectedIndex = 1;
                if (systemSites.Any(s => s.status != SystemSite.Status.complete))
                    comboSystemSite.DroppedDown = true;
            });

            Game.rcc.getSystemArchitect(game.lastDocked.StarSystem).continueOnMain(this, architect =>
            {
                if (!string.IsNullOrWhiteSpace(architect))
                    txtArchitect.Text = architect;
            });
        }

        private void radioOrbital_CheckedChanged(object sender, EventArgs e)
        {
            var isOrbital = radioOrbital.Checked;

            var sourceCosts = allCosts
                .Where(r => (r.location == "orbital") == isOrbital)
                .ToDictionary(c => c.buildType, c => $"Tier {c.tier}: {c.displayName}");

            comboBuildType.DataSource = new BindingSource(sourceCosts, null);
            comboBuildSubType.Items.Clear();
            comboBuildSubType.Items.AddRange(allCosts.First().layouts.ToArray());
            comboBuildSubType.SelectedIndex = 0;
        }

        private void comboBuildType_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBuildSubType.Items.Clear();

            var buildType = comboBuildType.SelectedValue?.ToString() ?? "";
            var match = allCosts.FirstOrDefault(c => c.buildType == buildType);
            if (match != null)
            {
                comboBuildSubType.Items.AddRange(match.layouts.ToArray());
                comboBuildSubType.SelectedIndex = 0;
            }

            comboBuildSubType.Enabled = true;
        }

        private void comboSystemSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (game.lastDocked == null) return;

            var siteId = comboSystemSite.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(siteId))
            {
                // stick with default name
                txtName.Text = ColonyData.getDefaultProjectName(game.lastDocked);
                comboBuildType.Enabled = true;
                comboBuildSubType.Enabled = true;
                radioOrbital.Enabled = true;
                radioSurface.Enabled = true;
                comboBuildType.SelectedIndex = 0;

                if (game.systemBody == null)
                    radioOrbital.Checked = true;
                else
                    radioSurface.Checked = true;
            }
            else
            {
                var siteMatch = systemSites.FirstOrDefault(s => s.id == siteId);
                if (siteMatch != null)
                {
                    var costMatch = allCosts.Find(c => c.layouts.Contains(siteMatch.buildType, StringComparer.OrdinalIgnoreCase));
                    if (costMatch != null)
                    {
                        // if this is a primary site - use the system site name, otherwise switch it
                        if (game.lastDocked.StationName.StartsWith(ColonyData.ExtPanelColonisationShip, StringComparison.OrdinalIgnoreCase))
                            txtName.Text = siteMatch.name;

                        comboBuildType.SelectedValue = costMatch.buildType;
                        comboBuildSubType.Text = siteMatch.buildType;

                        comboBuildType.Enabled = false;
                        comboBuildSubType.Enabled = false;
                        radioOrbital.Enabled = false;
                        radioSurface.Enabled = false;
                        radioOrbital.Checked = costMatch.location == "orbital";
                    }
                }
            }
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
                    buildType = comboBuildSubType.Text.ToLowerInvariant(),
                    buildName = txtName.Text ?? "primary-port",
                    architectName = txtArchitect.Text,
                    factionName = lastDocked.StationFaction.Name ?? "",
                    notes = txtNotes.Text,
                    isPrimaryPort = lastDocked.StationName.StartsWith(ColonyData.ExtPanelColonisationShip),

                    marketId = lastDocked.MarketID,
                    systemAddress = lastDocked.SystemAddress,
                    systemName = lastDocked.StarSystem,
                    starPos = game.systemData!.starPos,
                    bodyNum = this.comboBody.SelectedValue as int? ?? game.systemBody?.id,

                    colonisationConstructionDepot = lastDepot,

                    // add current cmdrs
                    commanders = new() { { game.Commander!, new() } },

                    systemSiteId = comboSystemSite.SelectedValue?.ToString(),
                };

                if (createProject.bodyNum >= 0)
                    createProject.bodyName = game.systemData.bodies.Find(b => b.id == createProject.bodyNum)?.name;

                if (lastDepot != null && game.lastDocked != null && lastDepot.MarketID == game.lastDocked.MarketID)
                {
                    var needed = lastDepot.ResourcesRequired.ToDictionary(r => r.Name.Substring(1).Replace("_name;", ""), r => r.RequiredAmount - r.ProvidedAmount);
                    createProject.commodities = needed;
                    createProject.maxNeed = lastDepot.ResourcesRequired.Sum(r => r.RequiredAmount);
                }

                Game.log($"Creating: '{createProject.buildName}' in '{createProject.systemName}' ({createProject.systemAddress}/{createProject.marketId})");

                this.setChildrenEnabled(false, btnCancel);

                Game.rcc.create(createProject).continueOnMain(this, newProject =>
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
                    Util.openLink($"{RavenColonial.uxUri}/#build={newProject.buildId}");

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
