using SrvSurvey.game;
using SrvSurvey.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                //.OrderBy(c => c.ToString())
                .ToDictionary(c => c.buildType, c => c.ToString());
            this.comboBuildType.DataSource = new BindingSource(sourceCosts, null);
            this.comboBuildType.DisplayMember = "Value";
            this.comboBuildType.ValueMember = "Key";
            this.comboBuildType.Enabled = true;

            txtArchitect.Text = game.Commander;

            var lastDocked = game.lastDocked;
            if (lastDocked != null)
            {
                var newName = lastDocked.StationName == ColonyData.SystemColonisationShip
                    ? "Primary port"
                    : lastDocked.StationName
                        .Replace("Planetary Construction Site:", "")
                        .Replace("Orbital Construction Site:", "")
                        .Trim()
                    ?? "";

                txtName.Text = newName;
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
            if (lastDocked == null) return;

            try
            {
                var createProject = new ProjectCreate
                {
                    buildType = comboBuildType.SelectedValue?.ToString() ?? "",
                    buildName = txtName.Text ?? "primary-port",
                    architectName = txtArchitect.Text,
                    factionName = lastDocked.StationFaction.Name ?? "",
                    notes = txtNotes.Text,

                    marketId = lastDocked.MarketID,
                    systemAddress = lastDocked.SystemAddress,
                    systemName = lastDocked.StarSystem,
                    starPos = game.systemData!.starPos,
                    bodyNum = game.systemBody?.id,
                    bodyName = game.systemBody?.name,

                    // add current cmdrs
                    commanders = new() { { game.Commander!, new() } }
                };

                Game.log($"Creating: '{createProject.buildName}' in '{createProject.systemName}' ({createProject.systemAddress}/{createProject.marketId})");

                this.setChildrenEnabled(false, btnCancel);

                Game.colony.create(createProject).continueOnMain(this, newProject =>
                {
                    Game.log($"New build created: {newProject.buildId}");

                    // open edit page in browser
                    Util.openLink($"{ColonyNet.uxUri}/#build={newProject.buildId}");

                    game.cmdrColony.fetchLatest().continueOnMain(this, () =>
                    {
                        this.Close();
                    });
                });
            }
            catch (Exception ex)
            {
                FormErrorSubmit.Show(ex);
                return;
            }
        }


    }
}
