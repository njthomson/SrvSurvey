using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBuildNew : SizableForm
    {
        private CommanderSettings cmdr;
        private string? key;
        private Dictionary<string, Dictionary<string, int>> allCosts;
        private Project project;
        private Font fontB;
        private List<Control> lastCtrls = new();
        private Dictionary<string, int> pendingCommodities = new();

        public FormBuildNew()
        {
            InitializeComponent();
            this.fontB = new Font(this.Font, FontStyle.Bold);

            this.allCosts = Game.colony.loadDefaultCosts();

            this.cmdr = CommanderSettings.LoadCurrentOrLast();
            linkLink.Text = cmdr.currentSystem;

            // show create button if docked at the right type of station

            comboBuildType.Items.AddRange(allCosts.Keys.ToArray());

            btnAccept.Enabled = false;
            btnJoin.Enabled = false;
            lblNot.Visible = true;
            comboBuildType.Enabled = false;
            txtArchitect.Enabled = false;
            txtName.Enabled = false;
            btnAssign.Enabled = false;
            panelList.Enabled = false;
        }

        private void FormBuildNew_Load(object sender, EventArgs e)
        {
            Program.defer(() =>
            {
                comboBuildType.SelectedIndex = 0;
                prepButtons();
            });
        }

        private void prepButtons()
        {
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

                if (lastDocked?.StationName == ColonyData.SystemColonisationShip && cmdr.currentMarketId != 0)
                {
                    lblNot.Visible = false;
                    txtArchitect.Enabled = true;
                    txtName.Enabled = true;
                    panelList.Enabled = true;

                    Game.colony.load(lastDocked.SystemAddress, lastDocked.MarketID).continueOnMain(this, project =>
                    {
                        if (project == null)
                        {
                            // project is not tracked - enable to start tracking
                            btnAccept.Enabled = true;
                            comboBuildType.Enabled = true;
                        }
                        else
                        {
                            this.project = project;

                            txtName.Text = project.buildName;
                            txtArchitect.Text = project.architectName;
                            comboBuildType.Text = project.buildType;

                            btnAccept.Text = "Update";
                            btnAccept.Enabled = true;
                            btnAssign.Enabled = true;

                            pendingCommodities = project.commodities;
                            prepCommodityRows();

                            if (!project.commanders?.Keys.Contains(cmdr.commander) == true)
                                btnJoin.Enabled = true;
                        }

                    }, true);
                }
            }
        }

        private void prepCommodityRows()
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
            foreach (var (commodity, need) in pendingCommodities)
            {
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var backColor = flip ? table.BackColor : SystemColors.Info;
                flip = !flip;

                var lbl = new Label()
                {
                    Text = commodity,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Height = 19,
                    Margin = new Padding(8, 0, 0, 1),
                    Padding = Padding.Empty,
                    BackColor = backColor,
                    TextAlign = ContentAlignment.MiddleLeft,
                    //Tag = rowCount,
                    Tag = backColor,
                };

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

                var ctrls = new List<Control>();
                ctrls.Add(lbl);
                ctrls.Add(numNeed);
                ctrls.AddRange(numNeed.Controls.Cast<Control>());
                foreach (var ctrl in ctrls)
                {
                    ctrl.MouseEnter += new EventHandler(mouseEnter);
                    ctrl.MouseLeave += new EventHandler(mouseLeave);
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
            foreach(var ctrl in lastCtrls)
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
            if (comboBuildType.SelectedItem == null) return;
            if (this.project == null)
                createProject();
            else
                updateProject();
        }

        private void updateProject()
        {
            try
            {
                //var data = JsonConvert.DeserializeObject<Dictionary<string, int>>(txtJson.Text)!;
                var updateProject = new ProjectUpdate
                {
                    buildId = project.buildId,
                    buildName = txtName.Text,
                    architectName = txtArchitect.Text,

                    factionName = "",
                    notes = "",

                    commodities = pendingCommodities
                    //data.ToDictionary(_ => _.Key, _ => new CommodityCount(data[_.Key], project.commodities.GetValueOrDefault(_.Key)?.total ?? data[_.Key])),
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
                //var data = JsonConvert.DeserializeObject<Dictionary<string, int>>(txtJson.Text)!;
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

                    commodities = pendingCommodities,

                    commanders = new Dictionary<string, HashSet<string>>() { { cmdr.commander, new() } },
                };

                Game.log($"Creating: {project}");
                btnAccept.Enabled = false;
                Game.colony.create(project).continueOnMain(this, saved =>
                {
                    Game.log($"New build created: {saved.buildId}");
                    Game.log(saved);

                    this.cmdr.loadColonyData().continueOnMain(this, () =>
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
                this.cmdr.loadColonyData().continueOnMain(this, () =>
                {
                    MessageBox.Show(this, "You are now contributing to this build project", "SrvSurvey");
                    this.Close();
                });
            });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;

            var buildType = comboBuildType.SelectedItem?.ToString();
            if (buildType != null && allCosts.ContainsKey(buildType))
            {
                pendingCommodities = allCosts[buildType].OrderBy(kv => kv.Key).ToDictionary(_ => _.Key, _ => _.Value);
                prepCommodityRows();
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
            var url = this.project == null
                ? $"{Colony.svcUri}/Find?q={cmdr.currentSystem}"
                : $"{Colony.svcUri}/Project?bid={project.buildId}";
            Util.openLink(url);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(null!, null!);
        }

        private Rectangle lastTableClip;
        private void table_Paint(object sender, PaintEventArgs e)
        {
            //Game.log($"Draw table: {table.RowCount} / {mouseRow} / {e.ClipRectangle}");
            //if (e.ClipRectangle.Height < 50) return;
            //lastTableClip = e.ClipRectangle;
            //e.Graphics.SetClip(e.ClipRectangle);

            //for (var n = 0; n < table.RowCount; n += 1)
            //{
            //    var lbl = table.GetControlFromPosition(0, n);
            //    if (lbl == null) continue;
            //    //if (!e.ClipRectangle.Contains(lbl.Bounds)) continue;

            //    //if (n == mouseRow)
            //    //    e.Graphics.FillRectangle(Brushes.Cyan, 0, lbl.Top + 1, table.Width, lbl.Height - 1);
            //    //else if (n % 2 == 0)
            //    //    e.Graphics.FillRectangle(SystemBrushes.Info, 0, lbl.Top + 1, table.Width, lbl.Height - 1);

            //    //e.Graphics.DrawLine(Pens.Red, 0, lbl.Top, table.Width, lbl.Top);
            //}

            //e.Graphics.DrawRectangle(Pens.Red, e.ClipRectangle);
        }
    }
}
