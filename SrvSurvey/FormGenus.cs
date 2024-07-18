using SrvSurvey.game;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

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

            if (game?.systemData == null) return;

            if (filterIdx < 2)
            {
                foreach (var body in game.systemData.bodies)
                    if (body.bioSignalCount > 0 || (Debugger.IsAttached && (body.type == SystemBodyType.LandableBody || body.type == SystemBodyType.SolidBody) && body.atmosphereType != "None"))
                        body.predictSpecies();
            }
            else if (targetBody != null)
            {
                targetBody.predictSpecies();
            }

            // show something if no bio signals
            if (game.systemData.bioSignalsTotal == 0)
                this.addWarningLabel("Current system has no detected bio signals.");

            // warn first if DSS is incomplete
            if (game.systemData?.fssComplete == false)
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
            if (filterIdx > 1 && this.targetBody?.bioSignalCount == 0)
                this.addWarningLabel("Current body has no detected bio signals.", "Change filter to system or everything?");


            addSystemSummary();

            //if (Game.settings.formGenusShowRingGuide)
            //    addRingGuide();

            if (filterIdx > 1)
                toolSignalCount.Text = $"Signals: {this.targetBody?.bioSignalCount ?? '?'}";
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

        private void addSystemSummary()
        {
            if (game?.systemData == null) return;

            var panel = new GenusPanel()
            {
                isBodySummary = true,
                showTopLine = false,
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
                Tag = game.systemData,
                Text = $"{game.systemData.name} summary:",
                Font = new Font(this.fontGenus, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(4, 5, 3, 0),
                Padding = new Padding(0),
            });

            var bodies = game.systemData.bodies.OrderBy(b => b.shortName);
            foreach (var body in bodies)
            {
                if (this.filterIdx > 1 && this.targetBody != null && this.targetBody != body) continue;
                if (body.bioSignalCount == 0) continue;

                var txt = $"{body.shortName}: {body.bioSignalCount}x signals ({Util.lsToString(body.distanceFromArrivalLS)})" + "\r\n";
                //txt += $"\r\n({foo.body.planetClass}, {foo.body.atmosphereType}, {(foo.body.surfaceGravity/10f).ToString("N2")}g, {foo.body.surfaceTemperature.ToString("N2")}K ({(foo.body.surfacePressure / 100_000f).ToString("N4")}) (atm), volcanism [{foo.body.volcanism}]\r\nmats [{string.Join(", ", foo.body.materials.Keys)}])";

                // ⚐⚑ ☐⮽☒⮽ ✅❎❎ ✔✘ ⦿⦾

                // known species
                if (body.organisms?.Any(o => o.species != null) == true)
                {
                    txt += "  " + string.Join("\r\n  ", body.organisms
                        .Where(o => o.reward > 0)
                        .OrderBy(o => o.genusLocalized)
                        .Select(o =>
                    {
                        var prefix = o.analyzed ? "✅ " : "☐ ";
                        if (o.isCmdrFirst) prefix += "⚑ ";
                        else if (o.isNewEntry) prefix += "⚐ ";
                        return $"\t{prefix}{o.variantLocalized}           {Util.credits(o.reward, true)}";
                        // TODO: remove reward from here and render  on the right hand side
                    })) + "\r\n";
                }
                // predicted species
                if (body.predictions != null)
                {
                    txt += "  " + string.Join("\r\n  ", body.predictions
                        .Where(p => body.organisms?.Any(o => o.species == p.Value.species.name) != true)
                        .OrderBy(p => p.Value.englishName)
                        .Select(p =>
                        {
                            // TODO: add tracking for regional firsts?
                            var prefix = game.cmdrCodex.isDiscovered(p.Value.entryId) ? "": "⚑ ";
                            return $"\t☐ {prefix}{p.Value.englishName} ?        {Util.credits(p.Value.reward, true)}";
                            // TODO: remove reward from here and render  on the right hand side
                        }));
                }

                var lbl = new Label()
                {
                    Tag = body,
                    Text = txt + "\r\n",
                    Font = this.fontGenus,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Margin = new Padding(14, 0, 3, 0),
                    Padding = new Padding(0),
                };
                if (body == this.targetBody)
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
            game?.systemBody?.predictSpecies();
        }

        private void menuOpenEDSM_Click(object sender, EventArgs e)
        {
            // TODO: look-up the EDSM system ID first?
            //Util.openLink("https://www.edsm.net/en/system/id/21623090/name/Prua+Phoe+UO-N+c8-22");
            Util.openLink("https://www.edsm.net/api-system-v1/bodies?systemName=" + Uri.EscapeDataString(game.systemData!.name));
        }

        private void menuOpenCanonnCodex_Click(object sender, EventArgs e)
        {
            Util.openLink("https://canonn-science.github.io/canonn-signals/?system=" + Uri.EscapeDataString(game.systemData!.name));
        }

        private void menuOpenCanonnDump_Click(object sender, EventArgs e)
        {
            Util.openLink("https://us-central1-canonn-api-236217.cloudfunctions.net/query/getSystemPoi?odyssey=Y&system=" + Uri.EscapeDataString(game.systemData!.name));
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            //// get real data from Canonn and compare with what we predict
            //if (game.systemData?.name == null) return;

            //Game.canonn.getSystemPoi(game.systemData.name, game.cmdr.commander).ContinueWith(r =>
            //{
            //    var txt = new StringBuilder();

            //    var comp1 = new Dictionary<string, HashSet<long>>();
            //    if (r.Result.codex == null) return;
            //    foreach (var _ in r.Result.codex)
            //    {
            //        if (_.hud_category != "Biology") continue;
            //        if (!comp1.ContainsKey(_.body)) comp1.Add(_.body, new HashSet<long>());

            //        if (comp1[_.body].Contains(_.entryid ?? -1)) continue;
            //        comp1[_.body].Add(_.entryid ?? -1);

            //        //var body = this.summary?.bodyGroups.Find(b => b.body.name.EndsWith(_.body));
            //        //if (body == null)
            //        //    txt.AppendLine($"Body not known: {_.body}");
            //        //else
            //        //{
            //        //    var match = body.species.Find(s => _.entryid!.ToString().StartsWith(s.bioRef.entryIdPrefix));
            //        //    txt.AppendLine(match == null
            //        //        ? $">>> No match for {_.body} {_.english_name} ({_.entryid})"
            //        //        : $"Match: {_.body} {_.english_name}");
            //        //}
            //    }

            //    foreach (var _ in comp1)
            //    {
            //        txt.AppendLine(_.Key);

            //        foreach (var entryId in _.Value)
            //        {
            //            var body = this.summary?.bodyGroups.Find(b => b.body.name.EndsWith(_.Key));
            //            if (body == null)
            //                txt.AppendLine($"\tBody not known: {_.Key}");
            //            else
            //            {
            //                var match = body.species.Find(s => entryId.ToString().StartsWith(s.bioRef.entryIdPrefix));
            //                if (match == null)
            //                    txt.AppendLine($"\t>>> No match for {_.Key} {Game.codexRef.matchFromEntryId(entryId).species.englishName} ({entryId})");
            //                else
            //                    txt.AppendLine($"\tMatch: {_.Key} {match.bioRef.englishName}");
            //            }
            //        }

            //    }

            //    //var txt = new StringBuilder();


            //    Game.log(txt);
            //});
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
            // the reward per species
            if (this.Controls.Count > 1)
            {
                // draw system rewards
                var title = (Label)this.Controls[0];
                var systemData = title.Tag as SystemData;
                if (systemData != null)
                {
                    TextRenderer.DrawText(
                        g,
                        Util.getMinMaxCredits(systemData.minBioRewards, systemData.maxBioRewards),
                        title.Font,
                        new Rectangle(this.Width - 204, title.Top + title.Padding.Top, 200, title.Height),
                        title.ForeColor,
                        TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
                    );
                }

                // for each body label
                for (int n = 1; n < this.Controls.Count; n++)
                {
                    var lbl = (Label)this.Controls[n];
                    var body = lbl.Tag as SystemBody;
                    if (body == null) continue;

                    if (n % 2 != 0)
                        g.FillRectangle(GameColors.brushTrackInactive, 0, lbl.Top - 2, this.ClientSize.Width, lbl.Height);

                    TextRenderer.DrawText(
                        g,
                        body.minMaxBioRewards,
                        lbl.Font,
                        new Rectangle(this.Width - 204, lbl.Top + lbl.Padding.Top, 200, lbl.Height),
                        lbl.ForeColor,
                        TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
                    );

                    //if (body.organisms != null)
                    //{
                    //    var x = lbl.Right + 4;
                    //    var y = lbl.Top;
                    //    foreach (var org in body.organisms)
                    //    {
                    //        var reward = org.reward;
                    //        if (reward == 0)
                    //            reward = foo.species.Find(s => s.predicted && s.bodies.Contains(foo.body))?.reward ?? 0;

                    //        PlotBase.drawBioRing(g, org.genus, x, y, reward, !org.analyzed, 38);
                    //        // x += 24;
                    //        x += 48;
                    //    }
                    //}
                }
            }

        }

        private void drawRingsAndCredits(Graphics g)
        {
            //var genus = this.Tag as SummaryGenus;
            //if (genus == null)
            //{
            //    PlotBase.drawBioRing(g, null, this.ClientSize.Width - 38 - 8, 7, -1, false, 38);
            //    return;
            //}

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
            //PlotBase.drawBioRing(g, genus.name, this.ClientSize.Width - 38 - 8, 7, -1, false, 38);

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