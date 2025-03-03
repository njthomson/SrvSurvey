using Newtonsoft.Json;
using SharpDX.DirectInput;
using SrvSurvey.game;
using System.Linq;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBuildNew : SizableForm
    {
        private CommanderSettings cmdr;
        private string? key;
        private Dictionary<string, Dictionary<string, int>> allCosts;
        private Project project;

        public FormBuildNew()
        {
            InitializeComponent();
            this.allCosts = Game.colony.loadDefaultCosts();

            this.cmdr = CommanderSettings.LoadCurrentOrLast();


            textSystem.Text = cmdr.currentSystem;

            // show create button if docked at the right type of station

            comboBuildType.Items.AddRange(allCosts.Keys.ToArray());
            comboBuildType.SelectedIndex = 0;

            prepButtons();
        }

        private void prepButtons()
        {
            txtJson.Enabled = false;
            btnAccept.Enabled = false;
            btnJoin.Enabled = false;
            lblNot.Visible = true;
            comboBuildType.Enabled = false;
            txtArchitect.Enabled = false;
            txtName.Enabled = false;
            btnAssign.Enabled = false;
            linkLink.Visible = false;


            var game = Game.activeGame;
            if (game != null && game.isMode(GameMode.Docked, GameMode.StationServices))
            {
                Docked? lastDocked = null;
                game.journals?.walkDeep(true, entry =>
                {
                    if (entry is Undocked) return true;
                    lastDocked = entry as Docked;
                    if (lastDocked != null) return true;
                    return false;
                });

                if (lastDocked?.StationName == "System Colonisation Ship" && cmdr.currentMarketId != 0)
                {
                    lblNot.Visible = false;
                    txtArchitect.Enabled = true;
                    txtName.Enabled = true;

                    Game.colony.load(lastDocked.SystemAddress, lastDocked.MarketID).continueOnMain(this, project =>
                    {
                        if (project == null)
                        {
                            // project is not tracked - enable to start tracking
                            btnAccept.Enabled = true;
                            txtJson.Enabled = true;
                            comboBuildType.Enabled = true;
                        }
                        else
                        {
                            this.project = project;

                            linkLink.Visible = true;
                            txtName.Text = project.buildName;
                            txtArchitect.Text = project.architectName;
                            comboBuildType.Text = project.buildType;
                            txtJson.Text = JsonConvert.SerializeObject(project.commodities.ToDictionary(_ => _.Key, _ => _.Value.need), Formatting.Indented);
                            txtJson.Enabled = true;

                            btnAccept.Text = "Update";
                            btnAccept.Enabled = true;
                            btnAssign.Enabled = true;

                            if (!project.commanders?.Keys.Contains(cmdr.commander) == true)
                                btnJoin.Enabled = true;
                        }

                    }, true);

                    //var key1 = $"{lastDocked.SystemAddress}/{lastDocked.MarketID}";
                    //if (game.cmdr.colonySummary.has(lastDocked.SystemAddress, lastDocked.MarketID))
                    //{
                    //    Game.log($"Already contributing to build: {key1}");
                    //}
                    //else
                    //{
                    //    btnAccept.Enabled = true;
                    //    textJson.Enabled = true;

                    //    Game.colony.load(cmdr.currentSystemAddress, cmdr.currentMarketId).continueOnMain(this, build =>
                    //    {
                    //        Game.log($"Contribute to build? {key} => {build?.buildId}");
                    //        this.key = build?.buildId;
                    //        btnJoin.Enabled = build != null;
                    //    });
                    //}
                }
            }
        }

        private void FormBuildNew_Load(object sender, EventArgs e)
        {
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (txtJson.Text == null || comboBuildType.SelectedItem == null) return;
            if (this.project == null)
                createProject();
            else
                updateProject();
        }

        private void updateProject()
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, int>>(txtJson.Text)!;
                var updateProject = new ProjectUpdate
                {
                    buildId = project.buildId,
                    buildName = txtName.Text,
                    architectName = txtArchitect.Text,

                    factionName = "",
                    notes = "",

                    commodities = data.ToDictionary(_ => _.Key, _ => new CommodityCount(data[_.Key], project.commodities[_.Key].total)),
                };

                Game.log($"Updating: {project}");
                btnAccept.Enabled = false;
                Game.colony.update(updateProject).continueOnMain(this, saved =>
                {
                    Game.log($"Updated project: {saved.buildId}");
                    Game.log(saved);

                    // TODO: revisit
                    //cmdr.colonySummary.buildIds.Add(saved.buildId);
                    cmdr.Save();
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                FormErrorSubmit.Show(ex);
                return;
            }
        }

        private void createProject()
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, int>>(txtJson.Text)!;
                var project = new ProjectCreate
                {
                    buildType = (string)comboBuildType.SelectedItem,
                    buildName = txtName.Text ?? "primary-port",
                    architectName = txtArchitect.Text,

                    marketId = cmdr.currentMarketId,
                    factionName = "",
                    systemAddress = cmdr.currentSystemAddress,
                    systemName = cmdr.currentSystem,
                    starPos = cmdr.starPos,
                    notes = "",

                    commodities = data,

                    commanders = new Dictionary<string, HashSet<string>>() { { cmdr.commander, new() } },
                };

                Game.log($"Creating: {project}");
                btnAccept.Enabled = false;
                Game.colony.create(project).continueOnMain(this, saved =>
                {
                    Game.log($"New build created: {saved.buildId}");
                    Game.log(saved);

                    this.cmdr.loadColonySummary().continueOnMain(this, cmdrSummary =>
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.project == null) return;
            btnAccept.Enabled = false;
            btnJoin.Enabled = false;

            Game.log($"Start contributing to: {project.buildId}");

            // confirm the build project exists first
            Game.colony.link(project.buildId, cmdr.commander).continueOnMain(this, build =>
            {
                // TODO: revisit
                //cmdr.colonySummary.has.buildIds.Add(key);
                //cmdr.Save();

                Game.log($"Contributing success!");
                this.cmdr.loadColonySummary().continueOnMain(this, cmdrSummary =>
                {
                    MessageBox.Show(this, "You are now contributing to this build project", "SrvSurvey");
                    this.Close();
                });
            });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var buildType = comboBuildType.SelectedItem?.ToString();
            if (buildType != null && allCosts.ContainsKey(buildType))
            {
                var parts = allCosts[buildType].OrderBy(kv => kv.Key).ToDictionary(_ => _.Key, _ => _.Value);
                txtJson.Text = JsonConvert.SerializeObject(parts, Formatting.Indented);
                txtJson.SelectionStart = 0;
                txtJson.SelectionLength = 0;
            }
        }

        private void btnAssign_Click(object sender, EventArgs e)
        {
            var buildType = comboBuildType.SelectedItem?.ToString();
            if (buildType != null && allCosts.ContainsKey(buildType))
            {
                var form = new FormBuildAssign(this.project.buildId, cmdr.commander, allCosts[buildType].Keys.ToList());
                form.ShowDialog(this);
            }
        }

        private void linkLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = $"{Colony.svcUri}/Project?bid={project.buildId}";
            Util.openLink(url);
        }
    }
}
