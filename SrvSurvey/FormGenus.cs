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

        public SystemBody targetBody;

        private Game game = Game.activeGame!;
        private Font fontGenus;
        private Font fontSpecies;

        private int filterIdx = 1;
        private SystemBioSummary? summary;

        private FormGenus()
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
                return !this.summary.genusGroups.Any(g => g.species.Any(s => s.name == speciesName && s.bodies.Contains(this.targetBody))); // hasSpeciesOnBody
        }

        public void deferPopulateGenus()
        {
            Task.Delay(100).ContinueWith((_) =>
            {
                this.BeginInvoke(() => this.populateGenus());
            });
        }

        public void populateGenus()
        {
            Game.log("populateGenus");

            this.targetBody = game?.targetBody!;

            toolRewardValue.Visible = game?.systemData != null; // this.filterIdx > 0;
            Application.DoEvents();

            uberPanel.SuspendLayout();
            uberPanel.Controls.Clear();

            //if (game?.systemData != null && game.systemData.bioSummary == null)
            game.systemData.summarizeBioSystem();

            this.summary = game?.systemData?.bioSummary;// .summarizeBioSystem();

            // show something if the game is not running or there's no summary
            if (filterIdx == 0 && game == null)
                this.addWarningLabel("Game is not running.", "Change filter to show everything?");
            else if (filterIdx > 0 && (this.summary == null || this.summary.genusGroups.Count == 0))
                this.addWarningLabel("Current system has no detected bio signals.", "Change filter to show everything?");

            var genusGroups = filterIdx == 0
                ? Game.codexRef.summarizeEverything()
                : summary?.genusGroups;

            // warn first if DSS is incomplete
            if (game?.systemData?.fssComplete == false)
            {
                if (!game.systemData.honked)
                {
                    this.addWarningLabel(null, $"Discovery scan needed");
                }
                else
                {
                    var fssProgress = 100.0 / (float)game.systemData.bodyCount * (float)game.systemData.fssBodyCount;
                    this.addWarningLabel(null, $"System not fully scanned: FSS {(int)fssProgress}% complete");
                }
            }

            // populate known genus groups
            var first = true;
            if (genusGroups != null)
            {
                foreach (var genus in genusGroups)
                {
                    // skip genus when filtering to current body and it's not on it
                    if (filterIdx > 1 && !genus.hasBody(this.targetBody))
                        continue;

                    var panel = createGenusPanel(genus);
                    if (panel == null) continue;

                    if (first)
                    {
                        panel.showTopLine = false;
                        first = false;
                    }

                    uberPanel.Controls.Add(panel);
                }
            }

            if (filterIdx > 1 && (this.summary == null || first))
                this.addWarningLabel("Current body has no detected bio signals.", "Change filter to system or everything?");

            if (summary != null && summary.bodiesWithUnknowns.Count > 0 && (this.filterIdx == 1 || this.summary.bodiesWithUnknowns.Contains(this.targetBody)))
            {
                var panel = createUnknownGenusPanel();
                panel.showTopLine = !first;
                uberPanel.Controls.Add(panel);
            }

            // show estimate(s)
            if (this.summary != null)
            {
                summary.calcMinMax(filterIdx == 2 ? this.targetBody : null);
                var prefix = filterIdx == 2 ? "Body: " : "System: ";
                if (summary.minReward == this.summary.maxReward)
                    toolRewardValue.Text = prefix + Util.credits(summary.minReward);
                else
                    toolRewardValue.Text = prefix + Util.credits(summary.minReward, true) + " ~ " + Util.credits(this.summary.maxReward);
            }

            // TODO: some kind of setting?
            addBodySummary();

            if (Game.settings.formGenusShowRingGuide)
                addRingGuide();

            if (filterIdx > 1)
            {
                toolSignalCount.Text = $"Signals: {this.targetBody?.bioSignalCount ?? '?'}";
            }
            else
                toolSignalCount.Text = $"Signals: {game?.systemData?.bioSignalsTotal}";

            uberPanel.ResumeLayout(true);
            uberPanel.Invalidate(true);
        }

        private void addWarningLabel(string? line1, string? line2 = null)
        {
            if (line1 != null)
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

        private GenusPanel createGenusPanel(SummaryGenus genus)
        {
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
                Text = genus.displayName,
                Font = this.fontGenus,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 0, 3, 0),
                Padding = new Padding(0, 3, 0, 3),
                UseCompatibleTextRendering = false,
            };
            panel.Controls.Add(lbl);

            // highlight genus if it's on the current/target body
            var genusBodyMatcher = filterIdx == 0 && this.summary != null
                ? this.summary.genusGroups.Find(_ => _.name == genus.name)
                : genus;

            if (genusBodyMatcher != null && genusBodyMatcher.hasBody(this.targetBody!))
                lbl.ForeColor = GameColors.Cyan;

            // add a row for each species...
            foreach (var species in genus.species.OrderBy(_ => -_.reward))
            {
                if (filterIdx > 1 && !species.bodies.Contains(this.targetBody))
                    continue;
                createSpeciesRow(species, panel, genusBodyMatcher ?? genus);
            }

            foreach (var species in genus.predictions.OrderBy(_ => -_.reward))
            {
                if (filterIdx > 1 && !species.bodies.Contains(this.targetBody))
                    continue;
                createSpeciesRow(species, panel, genusBodyMatcher ?? genus);
            }

            //// add another row when we know the genus but not the species?
            //var matchUnknownToBody = this.summary?.hasUnknownOnBody(genus.name, this.targetBodyShortName) == true;
            //if (this.summary?.hasUnknownSpecies(genus.name) == true && (filterIdx == 1 || matchUnknownToBody))
            //{
            //    var lbl3 = new Label()
            //    {
            //        Text = $"Unknown (on: {string.Join(", ", this.summary.unknownSpeciesBodies[genus.name])})",
            //        ForeColor = GameColors.OrangeDim,
            //        Font = this.fontSpecies,
            //        AutoSize = true,
            //        BackColor = Color.Transparent,
            //        Margin = new Padding(36, 0, 0, 0),
            //        Padding = new Padding(0),
            //    };
            //    panel.Controls.Add(lbl3);

            //    // highlight if any of these are on the current/target body
            //    if (matchUnknownToBody)
            //        lbl3.ForeColor = GameColors.Cyan;
            //}

            return panel;
        }

        private void createSpeciesRow(SummarySpecies species, GenusPanel panel, SummaryGenus genusBodyMatcher)
        {
            // species label
            var lbl2 = new Label()
            {
                Tag = species,
                Text = species.displayName,
                Font = this.fontSpecies,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(36, 0, 0, 0),
                Padding = new Padding(0, 3, 0, 3),
                UseCompatibleTextRendering = false,

            };
            panel.Controls.Add(lbl2);

            if (species.predicted)
            {
                lbl2.Text += "?";
                lbl2.Font = new Font(lbl2.Font, FontStyle.Italic);
            }

            // show which bodies have this species
            if (species.bodies.Count > 0)
            {
                lbl2.Text += species.predicted ? " (predicted on: " : " (on: ";
                lbl2.Text += string.Join(", ", species.bodies.Select(_ => _.shortName)) + ")";
            }

            // highlight species if it's on the current/target body
            var speciesBodyMatcher = filterIdx == 0 && genusBodyMatcher != null
                ? genusBodyMatcher.species.Find(_ => _.name == species.name) ?? genusBodyMatcher.predictions.Find(_ => _.name == species.name)
                : species;

            if (speciesBodyMatcher != null && speciesBodyMatcher.bodies.Contains(this.targetBody!))
                lbl2.ForeColor = GameColors.Cyan;
        }

        private GenusPanel createUnknownGenusPanel()
        {
            if (this.summary == null) throw new Exception("Why no summary?");

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


            // add another row as we also do not know the species
            //var matchUnknownToBody = this.summary?.hasUnknownOnBody(SystemBioSummary.unknown, this.targetBodyShortName) == true;
            var lbl2 = new Label()
            {
                Text = $"Unknowns (on: {string.Join(", ", this.summary.bodiesWithUnknowns.Select(_ => _.shortName))})",
                ForeColor = GameColors.Orange,
                Font = this.fontSpecies,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(36, 0, 0, 0),
                Padding = new Padding(0),
            };
            panel.Controls.Add(lbl2);

            // highlight genus if it's on the current/target body
            if (this.summary.bodiesWithUnknowns.Contains(this.targetBody))
            {
                // Not sure about this...
                lbl.ForeColor = GameColors.Cyan;
                lbl2.ForeColor = GameColors.Cyan;
            }

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

        private void addBodySummary()
        {
            if (this.summary == null) return;

            var panel = new GenusPanel()
            {
                isBodySummary = true,
                //showTopLine = false,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(3, 4, 28, 8),
            };

            panel.Controls.Add(new Label()
            {
                Text = $"{this.summary.systemName} summary:",
                Font = new Font(this.fontGenus, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 5, 3, 0),
                Padding = new Padding(0),
            });

            foreach (var foo in summary.bodyGroups)
            {
                if (this.filterIdx > 1 && this.targetBody != null && this.targetBody != foo.body) continue;

                var txt = $"{foo.body.shortName}: {foo.body.bioSignalCount}x signals, {Util.lsToString(foo.body.distanceFromArrivalLS)}";
                txt += "\r\n  " + string.Join("\r\n  ", foo.species.Select(_ =>
                {
                    var prefix = foo.body.organisms?.Find(o => o.species == _.bioRef.name)?.analyzed == true ? "-" : ">";
                    return $"\t{prefix}{_.bioRef.englishName}{(_.predicted ? "?" : "")}";
                }));

                var lbl = new Label()
                {
                    Tag = foo,
                    Text = txt,
                    Font = this.fontGenus,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Margin = new Padding(14, 0, 3, 0),
                    Padding = new Padding(0),
                };
                if (foo.body == this.targetBody)
                    lbl.ForeColor = GameColors.Cyan;

                panel.Controls.Add(lbl);

            }

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
            // cheap & cheerful refresh
            this.populateGenus();
        }
    }

    class GenusPanel : FlowLayoutPanel
    {
        public bool showTopLine = true;
        public bool isRingGuide = false;
        public bool isBodySummary = false;

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
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            g.FillRectangle(GameColors.brushBackgroundStripe, 0, 0, this.Width, this.Height);

            // the top line
            if (this.showTopLine)
                g.DrawLine(GameColors.penOrangeStripe3, 4, 2, this.Width, 2);

            if (this.isRingGuide)
                this.drawRingGuide(e.Graphics);
            else if (this.isBodySummary)
                this.drawBodySummary(e.Graphics);
            else
                this.drawRingsAndCredits(e.Graphics);
        }

        private void drawBodySummary(Graphics g)
        {
            //var y = -30;
            //PlotBase.drawBioRing(g, "$Codex_Ent_Tubus_Genus_Name;", 150, y + 30, -1, false, 38);

            // the reward per species
            if (this.Controls.Count > 1)
            {
                for (int n = 1; n < this.Controls.Count; n++)
                {
                    var lbl = (Label)this.Controls[n];
                    var foo = lbl.Tag as SummaryBody;
                    if (foo == null) continue;

                    if (n % 2 == 0)
                        g.FillRectangle(GameColors.brushTrackInactive, 0, lbl.Top - 2, this.ClientSize.Width, lbl.Height);

                    var minReward = foo.minReward;
                    var maxReward = foo.maxReward;

                    if (foo.body.firstFootFall)
                    {
                        minReward *= 5;
                        maxReward *= 5;
                    }

                    var txt = Util.credits(minReward, true);
                    if (minReward != maxReward)
                        txt += $" ~ {Util.credits(maxReward, true)}";

                    TextRenderer.DrawText(
                        g,
                        txt,
                        lbl.Font,
                        new Rectangle(this.Width - 200, lbl.Top + lbl.Padding.Top, 200, lbl.Height),
                        lbl.ForeColor,
                        TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
                    );

                    if (foo.body.organisms != null)
                    {
                        var x = lbl.Right + 4;
                        var y = lbl.Top;
                        foreach (var org in foo.body.organisms)
                        {
                            var reward = org.reward;
                            if (reward == 0)
                                reward = foo.species.Find(s => s.predicted && s.bodies.Contains(foo.body))?.reward ?? 0;

                            PlotBase.drawBioRing(g, org.genus, x, y, reward, !org.analyzed, 38);
                            // x += 24;
                            x += 48;
                        }
                    }
                }
            }

        }

        private void drawRingsAndCredits(Graphics g)
        {
            var genus = this.Tag as SummaryGenus;
            if (genus == null)
            {
                PlotBase.drawBioRing(g, null, this.ClientSize.Width - 38 - 8, 7, -1, false, 38);
                return;
            }

            // the reward per species
            if (this.Controls.Count > 1)
            {
                for (int n = 1; n < this.Controls.Count; n++)
                {
                    var lbl = (Label)this.Controls[n];
                    var species = lbl.Tag as SummarySpecies;
                    if (species == null) continue;

                    if (n % 2 == 0)
                        g.FillRectangle(GameColors.brushTrackInactive, 0, lbl.Top, this.ClientSize.Width, lbl.Height);

                    var reward = species.bodies.Any(_ => _.firstFootFall) ? species.reward * 5 : species.reward;
                    var txt = Util.credits(reward, true);
                    TextRenderer.DrawText(
                        g,
                        txt,
                        lbl.Font,
                        new Point(this.Width - 170, lbl.Top + lbl.Padding.Top),
                        lbl.ForeColor,
                        TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
                    );
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