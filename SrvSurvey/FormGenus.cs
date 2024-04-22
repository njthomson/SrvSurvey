using SrvSurvey.game;
using System.ComponentModel;
using System.Data;

namespace SrvSurvey
{
    internal partial class FormGenus : Form
    {
        public static FormGenus? activeForm;

        public static void show()
        {
            if (activeForm == null)
                FormGenus.activeForm = new FormGenus();

            Util.showForm(FormGenus.activeForm);
        }

        public string? targetBodyShortName;

        private Game game = Game.activeGame!;
        private Font fontGenus;
        private Font fontSpecies;

        private int filterIdx = 1;
        private SystemBioSummary? summary;

        public FormGenus()
        {
            InitializeComponent();
            toolFiller.Width = 20;
            toolFiller.Spring = true;
            uberPanel.Controls.Clear();
            uberPanel.BackColor = Color.Black;
            uberPanel.ForeColor = GameColors.Orange;

            Util.useLastLocation(this, Game.settings.formGenusGuideLocation);

            // filter
            this.toolRewardValue.Visible = this.filterIdx > 0;
            this.filterIdx = Game.settings.formGenusFilter;
            this.toolSystemFilter.Text = toolSystemFilter.DropDownItems[Game.settings.formGenusFilter + 2].Text;
            ((ToolStripMenuItem)toolSystemFilter.DropDownItems[Game.settings.formGenusFilter + 2]).Checked = true;

            this.toolShowGuide.Checked = Game.settings.formGenusShowRingGuide;


            // font ...
            if (Game.settings.formGenusFontSize == 0)
            {
                // small
                toolFont.Text = "Font: small";
                this.fontGenus = new Font(this.Font.FontFamily, 12f);
                this.fontSpecies = new Font(this.Font.FontFamily, 8f);
            }
            else
            {
                // large
                toolFont.Text = "Font: large";
                this.fontGenus = new Font(this.Font.FontFamily, 18f);
                this.fontSpecies = new Font(this.Font.FontFamily, 10f);
            }
        }

        private bool filtered { get => this.filterIdx > 0; }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formGenusGuideLocation != rect)
            {
                Game.settings.formGenusGuideLocation = rect;
                Game.settings.Save();
            }

            uberPanel.Invalidate(true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormGenus.activeForm = null;
        }

        private void FormGenus_Load(object sender, EventArgs e)
        {
            this.populateGenus();
        }

        public void startingJump(string line2)
        {
            uberPanel.SuspendLayout();
            uberPanel.Controls.Clear();

            this.addWarningLabel("Jumping to next system...", line2);

            uberPanel.ResumeLayout(true);
            uberPanel.Invalidate(true);
        }

        public bool shouldRefresh(string speciesName)
        {
            if (this.filterIdx == 0 || this.summary == null)
                return true;
            else
                return !this.summary.hasSpeciesOnBody(speciesName, this.targetBodyShortName);
        }

        public void deferPopulateGenus()
        {
            Task.Delay(100).ContinueWith((_) =>
            {
                this.BeginInvoke(() => this.populateGenus());
            });
        }

        public void populateGenus(string? targetBodyShortName = null)
        {
            this.targetBodyShortName = game?.targetBodyShortName;

            toolRewardValue.Visible = this.filterIdx > 0;
            Application.DoEvents();

            uberPanel.SuspendLayout();
            uberPanel.Controls.Clear();

            this.summary = game?.systemData?.summarizeBioSystem();

            // show something if the game is not running or there's no summary
            if (filterIdx == 0 && game == null)
                this.addWarningLabel("Game is not running.", "Change filter to show everything?");
            else if (filterIdx > 0 && (this.summary == null || this.summary.genusBodies.Count == 0))
                this.addWarningLabel("Current system has no detected bio signals.", "Change filter to show everything?");
            else if (filterIdx > 1 && (this.summary == null || this.summary.genusBodies.Count == 0))
                this.addWarningLabel("Current body has no detected bio signals.", "Change filter to system or everything?");

            // populate (filtered) rows
            var first = true;
            foreach (var genus in Game.codexRef.genus)
            {
                var panel = createGenusPanel(genus);
                if (panel == null) continue;

                if (first)
                {
                    panel.showTopLine = false;
                    first = false;
                }

                uberPanel.Controls.Add(panel);
            }

            if (this.summary?.hasGenus("Unknown") == true)
            {
                var panel = createUnknownGenusPanel();
                panel.showTopLine = !first;
                uberPanel.Controls.Add(panel);
            }

            // show estimate(s)
            if (this.summary != null)
                if (this.summary.minReward == this.summary.maxReward)
                    toolRewardValue.Text = Util.credits(this.summary.minReward);
                else
                    toolRewardValue.Text = Util.credits(this.summary.minReward, true) + " ~ " + Util.credits(this.summary.maxReward);

            if (Game.settings.formGenusShowRingGuide)
                addRingGuide();

            uberPanel.ResumeLayout(true);
            uberPanel.Invalidate(true);
        }

        private void addWarningLabel(string line1, string? line2 = null)
        {
            uberPanel.Controls.Add(new Label()
            {
                Text = line1,
                Font = this.fontGenus,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 0, 3, 0),
                Padding = new Padding(0),
            });
            if (line2 != null)
            {
                uberPanel.Controls.Add(new Label()
                {
                    Text = line2,
                    Font = this.fontSpecies,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Margin = new Padding(4, 0, 3, 0),
                    Padding = new Padding(0),
                });
            }
        }

        private GenusPanel? createGenusPanel(BioGenus genus)
        {
            // skip if genus not present?
            if (this.filterIdx > 0 && this.summary?.hasGenus(genus.name) != true) return null;

            var panel = new GenusPanel()
            {
                Tag = genus,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(3, 4, 28, 0),
            };

            // genus header label
            var lbl = new Label()
            {
                Text = genus.englishName,
                Font = this.fontGenus,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 0, 3, 0),
                Padding = new Padding(0),
            };
            panel.Controls.Add(lbl);

            // highlight genus if it's on the current/target body
            if (this.summary?.hasGenusOnBody(genus.name, this.targetBodyShortName) == true)
                lbl.ForeColor = GameColors.Cyan;

            // add a row for each species...
            foreach (var species in genus.species.OrderBy(_ => -_.reward))
            {
                // skip if not known on a system body, or the current one
                if (filterIdx > 0 && this.summary?.hasSpecies(species.name) == false) continue;
                var matchSpeciesToBody = this.summary?.hasSpeciesOnBody(species.name, this.targetBodyShortName) == true;
                if (filterIdx > 1 && !matchSpeciesToBody) continue;

                var txt = genus.odyssey
                    ? species.englishName.Replace(genus.englishName, "").Replace(" ", "")
                    : species.englishName.Replace("Brain Tree", "").Replace("Sinuous Tubers", "").Replace("Anemone", "");

                // species label
                var lbl2 = new Label()
                {
                    Tag = species,
                    Text = txt,
                    Font = this.fontSpecies,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Margin = new Padding(36, 0, 0, 0),
                    Padding = new Padding(0),
                };
                panel.Controls.Add(lbl2);

                // show which bodies have this species
                if (filtered && this.summary?.speciesBodies[species.name].Count > 0)
                    lbl2.Text += " (on: " + string.Join(", ", this.summary.speciesBodies[species.name]) + ")";

                // highlight species if it's on the current/target body
                if (matchSpeciesToBody)
                    lbl2.ForeColor = GameColors.Cyan;
            }

            // add another row when we know the genus but not the species
            var matchUnknownToBody = this.summary?.hasUnknownOnBody(genus.name, this.targetBodyShortName) == true;
            if (this.summary?.hasUnknownSpecies(genus.name) == true && (filterIdx == 1 || matchUnknownToBody))
            {
                var lbl3 = new Label()
                {
                    Text = $"Unknown (on: {string.Join(", ", this.summary.unknownSpeciesBodies[genus.name])})",
                    ForeColor = GameColors.OrangeDim,
                    Font = this.fontSpecies,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Margin = new Padding(36, 0, 0, 0),
                    Padding = new Padding(0),
                };
                panel.Controls.Add(lbl3);

                // highlight if any of these are on the current/target body
                if (matchUnknownToBody)
                    lbl3.ForeColor = GameColors.Cyan;
            }

            // ignore genus when filtering to current body and it's not on it
            if (filterIdx > 1 && panel.Controls.Count == 1)
                return null;

            return panel;
        }

        private GenusPanel createUnknownGenusPanel()
        {
            var panel = new GenusPanel()
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(3, 4, 28, 0),
            };

            // genus header label
            var lbl = new Label()
            {
                Text = SystemBioSummary.unknown,
                Font = this.fontGenus,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 0, 3, 0),
                Padding = new Padding(0),
            };
            panel.Controls.Add(lbl);

            // highlight genus if it's on the current/target body
            if (this.summary?.hasGenusOnBody(SystemBioSummary.unknown, this.targetBodyShortName) == true)
                lbl.ForeColor = GameColors.Cyan;

            // add another row as we also do not know the species
            var matchUnknownToBody = this.summary?.hasUnknownOnBody(SystemBioSummary.unknown, this.targetBodyShortName) == true;
            var lbl3 = new Label()
            {
                Text = $"Unknowns (on: {string.Join(", ", this.summary!.unknownSpeciesBodies[SystemBioSummary.unknown])})",
                ForeColor = GameColors.Orange,
                Font = this.fontSpecies,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(36, 0, 0, 0),
                Padding = new Padding(0),
            };
            panel.Controls.Add(lbl3);

            // highlight if any of these are on the current/target body
            if (matchUnknownToBody)
                lbl3.ForeColor = GameColors.Cyan;

            return panel;
        }

        private void addRingGuide()
        {
            var panel = new GenusPanel()
            {
                isRingGuide = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(3, 4, 28, 0),
            };

            panel.Controls.Add(new Label()
            {
                Text = "Ring graphic guide:",
                Font = new Font(this.Font, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 5, 3, 0),
                Padding = new Padding(0),
            });

            panel.Controls.Add(new Label()
            {
                Text = $"Inner ring matches genus. Outer ring increases with species reward value:\r\n(See Settings > Bio Scanning to adjust reward value groupings)",
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 0, 3, 0),
                Padding = new Padding(0)
            });

            var bucketOne = Util.credits((long)Game.settings.bioRingBucketOne * 1_000_000);
            var bucketTwo = Util.credits((long)Game.settings.bioRingBucketTwo * 1_000_000);
            var bucketThree = Util.credits((long)Game.settings.bioRingBucketThree * 1_000_000);
            panel.Controls.Add(new Label()
            {
                Text = $"   0% Reward unknown\r\n 25% < {bucketOne}\r\n 50% < {bucketTwo}\r\n 75% < {bucketThree}\r\n100% > {bucketThree}",
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 0, 3, 0),
                Padding = new Padding(4, 5, 3, 100)
            });

            uberPanel.Controls.Add(panel);
        }

        private void toolFilter_Click(object sender, EventArgs e)
        {
            // ignore top 2 items
            var tool = (ToolStripItem)sender;
            var idx = toolSystemFilter.DropDownItems.IndexOf(tool) - 2;
            if (idx < 0) return;

            this.filterIdx = idx;
            toolSystemFilter.Text = tool.Text;
            toolEverything.Checked = this.filterIdx == 0;
            toolInSystem.Checked = this.filterIdx == 1;
            toolOnBody.Checked = this.filterIdx == 2;

            Game.settings.formGenusFilter = this.filterIdx;
            Game.settings.Save();

            this.populateGenus();
        }

        private void toolShowGuide_Click(object sender, EventArgs e)
        {
            Game.settings.formGenusShowRingGuide = toolShowGuide.Checked;
            Game.settings.Save();

            this.populateGenus();
        }


        private void toolSizeSmall_Click(object sender, EventArgs e)
        {
            // switch to small font
            this.fontGenus = new Font(this.Font.FontFamily, 12f);
            this.fontSpecies = new Font(this.Font.FontFamily, 8f);

            toolFont.Text = "Font: small";
            this.populateGenus();

            Game.settings.formGenusFontSize = 0;
            Game.settings.Save();
        }

        private void toolSizeLarge_Click(object sender, EventArgs e)
        {
            // switch to large font
            this.fontGenus = new Font(this.Font.FontFamily, 18f);
            this.fontSpecies = new Font(this.Font.FontFamily, 10f);

            toolFont.Text = "Font: large";
            this.populateGenus();

            Game.settings.formGenusFontSize = 1;
            Game.settings.Save();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            this.populateGenus();
        }
    }

    class GenusPanel : FlowLayoutPanel
    {
        public bool showTopLine = true;
        public bool isRingGuide = false;

        public GenusPanel()
        {
        }

        public void updateFontSizes(Font fontGenus, Font fontSpecies)
        {
            // the header label
            this.Controls[0].Font = fontGenus;

            for (int n = 1; n < this.Controls.Count; n++)
                this.Controls[n].Font = fontSpecies;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var sz = base.GetPreferredSize(proposedSize);
            if (this.Parent != null)
            {
                sz.Width = this.Parent.Width - this.Padding.Right;
            }

            return sz;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate(true);
        }

        //protected override void OnPaint(PaintEventArgs e)
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            var g = e.Graphics;
            g.FillRectangle(GameColors.brushBackgroundStripe, 0, 0, this.Width, this.Height);

            // the top line
            if (this.showTopLine)
                g.DrawLine(GameColors.penOrangeStripe3, 4, 2, this.Width, 2);

            if (this.isRingGuide)
                this.drawRingGuide(e.Graphics);
            else
                this.drawRingsAndCredits(e.Graphics);
        }

        private void drawRingsAndCredits(Graphics g)
        {
            var genus = this.Tag as BioGenus;
            if (genus == null)
            {
                PlotBase.drawBioRing(g, null, this.ClientSize.Width - 38 - 8, 7, -1, false, 38);
                return;
            }

            // the reward per species
            var minReward = genus.minReward;
            if (this.Controls.Count > 1)
            {
                for (int n = 1; n < this.Controls.Count; n++)
                {
                    var ctrl = (Label)this.Controls[n];
                    var species = ctrl.Tag as BioSpecies;
                    var txt = species == null
                        ? Util.credits(genus.minReward, true) + " ~ " + Util.credits(genus.maxReward)
                        : Util.credits(species.reward);

                    if (n % 2 == 0)
                        g.FillRectangle(GameColors.brushTrackInactive, 0, ctrl.Top, this.ClientSize.Width, ctrl.Height);

                    var b = species == null ? GameColors.brushGameOrangeDim : GameColors.brushGameOrange;

                    //var currentBodyShortName = Game.activeGame?.targetBodyShortName ?? "";
                    if (ctrl.ForeColor == ((SolidBrush)GameColors.brushCyan).Color)
                    {
                        b = GameColors.brushCyan;
                    }

                    if (species != null)
                        minReward = (int)Math.Max(minReward, species.reward);

                    g.DrawString(
                        txt,
                        ctrl.Font,
                        b,
                        this.Width - 170, this.Controls[n].Top);
                }
            }

            // the ring graphic
            PlotBase.drawBioRing(g, genus.name, this.ClientSize.Width - 38 - 8, 7, -1, false, 38);

            // TODO: could we highlight the min/max reward?
            //if (this.Controls.Count == 2)
            //    PlotBase.drawBioRing(g, genus.name, this.ClientSize.Width - 38 - 8, 7, minReward, true, 38); //, genus.maxReward);
            //else
            //    PlotBase.drawBioRing(g, genus.name, this.ClientSize.Width - 38 - 8, 7, minReward, true, 38, genus.maxReward);
        }

        private void drawRingGuide(Graphics g)
        {
            var y = this.Controls[2].Top;

            PlotBase.drawBioRing(g, "$Codex_Ent_Tubus_Genus_Name;", 150, y + 30, -1, false, 38);
            PlotBase.drawBioRing(g, "$Codex_Ent_Tubus_Genus_Name;", 220, y + 30, 1, false, 38);
            PlotBase.drawBioRing(g, "$Codex_Ent_Tubus_Genus_Name;", 290, y + 30, 1 + (long)Game.settings.bioRingBucketOne * 1_000_000, false, 38);
            PlotBase.drawBioRing(g, "$Codex_Ent_Tubus_Genus_Name;", 360, y + 30, 1 + (long)Game.settings.bioRingBucketTwo * 1_000_000, false, 38);
            PlotBase.drawBioRing(g, "$Codex_Ent_Tubus_Genus_Name;", 430, y + 30, 1 + (long)Game.settings.bioRingBucketThree * 1_000_000, false, 38);
        }
    }
}