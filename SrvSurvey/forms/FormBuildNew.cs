using SrvSurvey.game;
using SrvSurvey.Properties;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBuildNew : SizableForm
    {
        private Game game => Game.activeGame!;
        private CommanderSettings cmdr; // TODO: <-- retire?
        private ColonyData colonyData;

        private Project project;
        private bool dirty = false;

        private string? key;
        private List<ColonyCost2> allCosts;
        private Font fontB;
        private Font lblFont;
        private List<Control> lastCtrls = new();
        private Dictionary<string, int> pendingCommodities = new();
        private Project? untrackedProject;

        public FormBuildNew()
        {
            InitializeComponent();
            this.Icon = Icons.ruler;
            this.fontB = new Font(this.Font, FontStyle.Bold);
            this.lblFont = new Font("Segoe UI Emoji", this.Font.Size);
            this.table.Controls.Clear();

            this.cmdr = CommanderSettings.LoadCurrentOrLast(); // Remove?
            this.colonyData = game.cmdrColony;

            this.allCosts = Game.colony.loadDefaultCosts();
            var sourceCosts = allCosts.ToDictionary(c => c.buildType, c => $"Tier {c.tier}: {c.displayName} ({string.Join(", ", c.layouts)})");
            this.comboBuildType.DataSource = new BindingSource(sourceCosts, null);
            this.comboBuildType.DisplayMember = "Value";
            this.comboBuildType.ValueMember = "Key";

            // fill combo with known projects
            setComboProjectBindingSource();
            this.comboProjects.DisplayMember = "Value";
            this.comboProjects.ValueMember = "Key";

            this.chooseInitialProject();
        }

        private void chooseInitialProject()
        {
            if (colonyData.primaryBuildId != null)
                comboProjects.SelectedValue = colonyData.primaryBuildId;
            if (comboProjects.SelectedItem == null && comboProjects.Items.Count > 0)
                comboProjects.SelectedIndex = 0;

            // if we are currently at a construction ship - select that one
            if (game.lastDocked != null && ColonyData.isConstructionSite(game.lastDocked))
            {
                // do we know about this one already?
                var match = colonyData.projects.Find(p => p.marketId == game.lastDocked.MarketID);
                Game.log($"FormBuildNew.ctor: match: {match?.buildId}");
                if (match != null)
                {
                    comboProjects.SelectedValue = match.buildId;
                    this.setEnabled(true);
                    return;
                }
                else
                {
                    // not one we know about - make a network request for it...
                    this.setEnabled(false);
                    Game.colony.load(game.lastDocked.SystemAddress, game.lastDocked.MarketID).continueOnMain(this, newProject =>
                    {
                        if (newProject != null)
                        {
                            // it is tracked, but not by this cmdr
                            this.untrackedProject = newProject;
                        }
                        else if (ColonyData.isConstructionSite(game.lastDocked))
                        {
                            var newName = game.lastDocked.StationName == ColonyData.SystemColonisationShip
                            ? "Primary port"
                            : game.lastDocked.StationName
                                .Replace("Planetary Construction Site:", "")
                                .Replace("Orbital Construction Site:", "")
                            ?? "";

                            // is not tracked by anyone
                            this.untrackedProject = new()
                            {
                                buildId = "",
                                buildType = "plutus",
                                buildName = newName,
                                marketId = game.lastDocked.MarketID,
                                systemAddress = game.lastDocked.SystemAddress,
                                systemName = game.lastDocked.StarSystem,
                                starPos = game.systemData!.starPos,
                                bodyNum = game.systemBody?.id,
                                bodyName = game.systemBody?.name,
                                commanders = new Dictionary<string, HashSet<string>>() { { cmdr.commander, new() } },
                            };
                            comboBuildType.Enabled = true;
                            comboBuildType.SelectedValue = this.untrackedProject.buildType;
                        }

                        setComboProjectBindingSource(this.untrackedProject?.buildId);

                        Game.log($"FormBuildNew.ctor: found untracked build: {newProject?.buildId}");
                        this.setEnabled(true);

                        setProject(newProject);
                    }, true);
                }
            }
        }

        private void setComboProjectBindingSource(string? selectBuildId = null)
        {
            var selectedValue = comboProjects.SelectedValue;

            // alpha sort known projects
            var list = colonyData.projects
                .OrderBy(p => $"{p.systemName} {p.buildName}")
                .ToList();

            // inject the untracked project at beginning of list
            if (this.untrackedProject != null)
                list.Insert(0, untrackedProject);

            var dic = list.ToDictionary(p => p.buildId, p => p.buildId == "" ? "(new) " + p.ToString() : p.ToString());
            if (dic.Count > 0)
            {
                this.comboProjects.DataSource = new BindingSource(dic, null);

                if (selectBuildId != null)
                    comboProjects.SelectedValue = selectBuildId;
                else if (selectedValue != null)
                    comboProjects.SelectedValue = selectedValue;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // TODO: dirty check?

            // reload data from network
            var selectedValue = comboProjects.SelectedValue as string;

            this.setEnabled(false);
            colonyData.fetchLatest().continueOnMain(this, () =>
            {
                if (this.project != null)
                {
                    var refreshedProject = colonyData.getProject(this.project.buildId);
                    setComboProjectBindingSource(selectedValue);
                    setProject(refreshedProject, true);
                }
                this.setEnabled(true);
            });
        }

        private void setEnabled(bool enable)
        {
            this.setChildrenEnabled(enable, comboBuildType);

            if (enable && untrackedProject?.buildId == "")
                comboBuildType.Enabled = true;

        }

        private void comboProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            Game.log(comboProjects.SelectedItem);
            if (comboProjects.SelectedItem is KeyValuePair<string, string>)
            {
                var pair = (KeyValuePair<string, string>)comboProjects.SelectedItem;
                if (pair.Key == untrackedProject?.buildId)
                    setProject(untrackedProject);
                else
                    setProject(colonyData.getProject(pair.Key));
            }
        }

        private void setProject(Project? project, bool force = false)
        {
            if (project == null || (this.project?.buildId == project.buildId && !force)) return;
            this.project = project;

            txtName.Text = project.buildName;
            txtArchitect.Text = project.architectName;
            txtFaction.Text = project.factionName;
            txtNotes.Text = project.notes;
            linkLink.Text = project.systemName;
            comboBuildType.Text = project.buildType;
            checkPrimary.Checked = colonyData.primaryBuildId == project.buildId;

            listCmdrs.Items.Clear();
            listCmdrs.Items.AddRange(project.commanders.Keys.ToArray());

            prepCommodityRows(project);

            btnAccept.Text = project == this.untrackedProject
                ? "Create Project"
                : "Update Project";
            dirty = false;
        }

        private void prepCommodityRows(Project proj)
        {
            var start = DateTime.Now;
            table.SuspendLayout();
            table.Controls.Clear();

            table.RowCount = pendingCommodities.Count + 2;
            table.RowStyles.Clear();
            int rowCount = 0;

            // Add Header controls
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(new Label()
            {
                Text = "Commodity",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Font = fontB,
            }, 0, rowCount);
            table.Controls.Add(new Label()
            {
                Text = "Need",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = fontB,
            }, 1, rowCount);
            rowCount++;

            var flip = false;
            foreach (var commodity in proj.commodities.Keys.Order())
            {
                var need = proj.commodities[commodity];

                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var backColor = flip ? table.BackColor : SystemColors.Info;
                flip = !flip;

                var lbl = new Label()
                {
                    Text = commodity,
                    Font = this.lblFont,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Height = 19,
                    Margin = new Padding(8, 0, 0, 1),
                    Padding = Padding.Empty,
                    BackColor = backColor,
                    TextAlign = ContentAlignment.MiddleLeft,
                    //Tag = rowCount,
                    Tag = backColor,
                };

                if (proj.commanders.Any(c => c.Value.Contains(commodity)))
                    lbl.Text += " 📌";

                var numNeed = new NumericUpDown()
                {
                    Width = 100,
                    Minimum = 0,
                    Maximum = 500_000,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Margin = new Padding(0, 0, 6, 0),
                    TextAlign = HorizontalAlignment.Right,
                    BorderStyle = BorderStyle.None,
                    BackColor = backColor,
                    Tag = backColor,
                    ThousandsSeparator = true,
                };
                numNeed.Value = need;
                numNeed.ValueChanged += (object? sender, EventArgs e) =>
                {
                    pendingCommodities[commodity] = (int)numNeed.Value;
                };
                numNeed.Enter += num_Enter;

                var mouseEnter = (object? sender, EventArgs e) =>
                {
                    // restore colour on last controls
                    restoreLastCtrls();

                    // change color on new controls
                    lastCtrls.Add(lbl);
                    lastCtrls.Add(numNeed);
                    lastCtrls.AddRange(numNeed.Controls.Cast<Control>());

                    lastCtrls.ForEach(c => c.BackColor = Color.Cyan);
                };
                var mouseLeave = (object? sender, EventArgs e) =>
                {
                    // restore colour on last controls
                    restoreLastCtrls();
                };
                var mouseDown = (object? sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        var c = sender as Control;
                        //var menu = new ContextMenuStrip();
                        //menu.Items.Add(new ToolStripMenuItem("Assign to Commander"));
                        //menu.Items.Add(new ToolStripMenuItem("Remove"));
                        //contextMenu.Show(tree.PointToScreen(e.Location));
                        if (c != null)
                        {
                            menuCommodities.Tag = commodity;
                            menuCommodities.Show(c, e.Location);
                        }
                    }
                };

                var ctrls = new List<Control>();
                ctrls.Add(lbl);
                ctrls.Add(numNeed);
                ctrls.AddRange(numNeed.Controls.Cast<Control>());
                foreach (var ctrl in ctrls)
                {
                    ctrl.MouseEnter += new EventHandler(mouseEnter);
                    ctrl.MouseLeave += new EventHandler(mouseLeave);
                    ctrl.MouseDown += new MouseEventHandler(mouseDown);
                }

                numNeed.Height = lbl.Height;
                table.Controls.Add(lbl, 0, rowCount);
                table.Controls.Add(numNeed, 1, rowCount);
                rowCount++;
            }

            // add empty row/label at the end to consume any spare space
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.DoubleBuffered = true;

            table.ResumeLayout();
            Game.log($"Done: {(DateTime.Now - start).TotalMilliseconds}");
        }

        private void restoreLastCtrls()
        {
            foreach (var ctrl in lastCtrls)
            {
                var tag = ctrl.Tag ?? ctrl.Parent?.Tag;
                if (tag is Color)
                    ctrl.BackColor = (Color)tag;
            }

            lastCtrls.Clear();
        }

        private void num_Enter(object? sender, EventArgs e)
        {
            var numCtrl = sender as NumericUpDown;
            if (numCtrl != null)
            {
                numCtrl.Select(0, numCtrl.Text.Length);
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            comboBuildType.Enabled = false;

            if (comboBuildType.SelectedItem == null) return;
            if (this.project.buildId == "")
                createProject();
            else
                updateProject();
        }

        private void updateProject()
        {
            try
            {
                this.setEnabled(false);
                Game.log($"Updating: {project.buildName} ({project.buildId})");
                var updateProject = new ProjectUpdate
                {
                    buildId = project.buildId,
                    buildName = txtName.Text,
                    architectName = txtArchitect.Text,
                    factionName = txtFaction.Text,
                    notes = txtNotes.Text,

                    commodities = pendingCommodities

                    // TODO: add/remove commanders
                };

                // TODO: do in parallel!
                // if marking as primary - write this at the same time
                if (checkPrimary.Checked)
                {
                    colonyData.primaryBuildId = project.buildId;
                    Game.colony.setPrimary(colonyData.cmdr, project.buildId).justDoIt();
                }
                else if (colonyData.primaryBuildId == project.buildId)
                {
                    colonyData.primaryBuildId = "";
                    Game.colony.setPrimary(colonyData.cmdr, project.buildId).justDoIt();
                }

                // cmdrs to add and remove
                var cmdrsToAdd = listCmdrs.Items.Cast<string>().Where(c => !project.commanders.ContainsKey(c)).ToList();
                var cmdrsToRemove = project.commanders.Keys.Where(c => !listCmdrs.Items.Contains(c)).ToList();
                cmdrsToAdd.ForEach(cmdr => Game.colony.linkCmdr(project.buildId, cmdr).justDoIt());
                cmdrsToRemove.ForEach(cmdr => Game.colony.unlinkCmdr(project.buildId, cmdr).justDoIt());

                Game.colony.update(updateProject).continueOnMain(this, savedProject =>
                {
                    processServerResponse(savedProject);
                });
            }
            catch (Exception ex)
            {
                FormErrorSubmit.Show(ex);
                return;
            }
        }

        private void processServerResponse(Project serverProject)
        {
            untrackedProject = null;

            // replace the reference and save
            var originalProject = colonyData.getProject(serverProject.buildId);
            if (originalProject == null)
            {
                var match = serverProject.commanders.GetValueOrDefault(colonyData.cmdr, StringComparison.OrdinalIgnoreCase);
                if (match == null)
                {
                    // ignore updates for projects we are not tracking
                }
                else
                {
                    // start tracking
                    colonyData.projects.Add(serverProject);
                }
            }
            else
            {
                var idx = colonyData.projects.IndexOf(originalProject);
                colonyData.projects[idx] = serverProject;
            }
            colonyData.Save();
            setComboProjectBindingSource(serverProject.buildId);
            Game.log($"Updated project: {serverProject.buildName} {serverProject.buildId}");
            setProject(serverProject);
            this.setEnabled(true);
        }

        private void createProject()
        {
            try
            {
                var createProject = new ProjectCreate
                {
                    buildType = comboBuildType.SelectedValue?.ToString() ?? "",
                    buildName = txtName.Text ?? "primary-port",
                    architectName = txtArchitect.Text,
                    factionName = game.lastDocked?.StationFaction.Name ?? "", // txtFaction.Text,
                    notes = txtNotes.Text,

                    marketId = project.marketId,
                    systemAddress = cmdr.currentSystemAddress,
                    systemName = cmdr.currentSystem,
                    starPos = cmdr.starPos,
                    bodyNum = project.bodyNum,
                    bodyName = project.bodyName,

                    commodities = pendingCommodities,

                    // add all cmdrs
                    commanders = listCmdrs.Items.Cast<string>().ToDictionary(_ => _, _ => new HashSet<string>()),
                };

                Game.log($"Creating: '{createProject.buildName}' in '{createProject.systemName}' ({createProject.systemAddress}/{createProject.marketId})");
                setEnabled(false);
                Game.colony.create(createProject).continueOnMain(this, saved =>
                {
                    processServerResponse(saved);
                    //Game.log($"New build created: {saved.buildId}");
                    //Game.log(saved);

                    //game.cmdrColony.fetchLatest().continueOnMain(this, () =>
                    //{
                    //    this.Close();
                    //});
                });
            }
            catch (Exception ex)
            {
                FormErrorSubmit.Show(ex);
                return;
            }
        }

        private void comboBuildType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed || comboBuildType.Enabled == false || untrackedProject == null) return;

            var buildType = comboBuildType.SelectedValue?.ToString();
            if (buildType == null) return;
            var match = allCosts.Find(c => c.layouts.Contains(buildType, StringComparer.OrdinalIgnoreCase));
            if (match == null) return;

            untrackedProject.commodities = match.cargo;
            prepCommodityRows(untrackedProject);
        }

        private void linkLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = this.project == null || this.project.buildId == ""
                ? $"{ColonyNet.uxUri}#find={cmdr.currentSystem}"
                : $"{ColonyNet.uxUri}#build={project.buildId}";
            Util.openLink(url);
        }

        private void btnCmdrAdd_Click(object sender, EventArgs e)
        {
            var suggestCmdr = listCmdrs.Items.Contains(colonyData.cmdr) ? "" : colonyData.cmdr;
            var form = new FormBuildAddCmdr(suggestCmdr);
            var rslt = form.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                listCmdrs.Items.Add(form.commander);
                dirty = true;
            }
        }

        private void btnCmdrRemove_Click(object sender, EventArgs e)
        {
            var cmdr = listCmdrs.SelectedItem as string;
            if (cmdr == null) return;

            if (listCmdrs.Items.Contains(cmdr))
            {
                listCmdrs.Items.Remove(cmdr);
                dirty = true;
            }
        }

        private void menuCommodities_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var commodity = menuCommodities.Tag as string;
            if (project?.commanders == null || commodity == null || project.buildId == "")
            {
                e.Cancel = true;
                return;
            }

            while (menuCommodities.Items.Count > 1) menuCommodities.Items.RemoveAt(1);

            foreach (var (cmdr, assigned) in project.commanders)
            {
                var item = (ToolStripMenuItem)menuCommodities.Items.Add(cmdr, cmdr, null!);
                item.Checked = assigned.Contains(commodity);
                item.Click += (s, e) =>
                {
                    Game.log($"menuCommodities: buildId: {project.buildId}, cmdr: {cmdr}, commodity: {commodity}, checked: {item.Checked}");
                    if (item.Checked)
                    {
                        Game.colony.unAssign(project.buildId, cmdr, commodity).continueOnMain(this, () =>
                        {
                            project.commanders?.GetValueOrDefault(cmdr, StringComparison.OrdinalIgnoreCase)?.Remove(commodity);
                            setProject(project, true);
                        });
                    }
                    else
                    {
                        Game.colony.assign(project.buildId, cmdr, commodity).continueOnMain(this, () =>
                        {
                            project.commanders?.GetValueOrDefault(cmdr, StringComparison.OrdinalIgnoreCase)?.Add(commodity);
                            setProject(project, true);
                        });
                    }

                };
            }

        }

        private void linkRaven_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(ColonyNet.uxUri);
        }
    }
}
