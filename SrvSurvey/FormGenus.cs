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

        public string? targetBody;

        private Game game = Game.activeGame!;
        private Font fontGenus;
        private Font fontSpecies;

        private bool filtered = true;
        private long minEstimate;
        private long maxEstimate;

        public FormGenus()
        {
            InitializeComponent();
            toolFiller.Width = 20;
            uberPanel.Controls.Clear();
            uberPanel.BackColor = Color.Black;
            uberPanel.ForeColor = GameColors.Orange;

            Util.useLastLocation(this, Game.settings.formGenusGuideLocation);

            this.fontGenus = new Font(this.Font.FontFamily, 18f);
            this.fontSpecies = new Font(this.Font.FontFamily, 10f);
            toolRewardLabel.Font = toolRewardValue.Font = new Font(this.Font.FontFamily, 12f, FontStyle.Bold);

            // TODO: setting?
            filtered = true;
            toolSystemFilter.Text = toolSystemFilter.DropDownItems[filtered ? 2 : 0].Text;

            toolRewardLabel.Visible = false;
            toolRewardValue.Visible = filtered;
        }

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

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    base.OnPaintBackground(e);
        //    try
        //    {
        //        e.Graphics.DrawString("hello", this.Font, Brushes.Cyan, 44, 44);

        //    }
        //    catch (Exception ex)
        //    {
        //        Game.log($"FormGenus.OnPaintBackground error: {ex}");
        //    }
        //}

        private void FormGenus_Load(object sender, EventArgs e)
        {
            this.populateGenus();
        }

        public void populateGenus()
        {
            //toolRewardLabel.Visible = filtered;
            toolRewardValue.Visible = filtered;
            Application.DoEvents();

            uberPanel.SuspendLayout();
            uberPanel.Controls.Clear();
            this.targetBody = game.targetBodyShortName;

            var summary = game?.systemData?.summarizeBioSystem();
            this.minEstimate = summary?.minReward ?? 0;
            this.maxEstimate = summary?.maxReward ?? 0;

            var first = true;
            foreach (var genus in Game.codexRef.genus)
            {
                var panel = createGenusPanel(genus, summary);
                if (panel == null) continue;

                if (first)
                {
                    panel.showTopLine = false;
                    first = false;
                }

                uberPanel.Controls.Add(panel);
            }

            if (minEstimate == maxEstimate)
                toolRewardValue.Text = Util.credits(this.minEstimate);
            else
                toolRewardValue.Text = Util.credits(this.minEstimate, true) + " ~ " + Util.credits(this.maxEstimate);

            uberPanel.ResumeLayout(true);
            uberPanel.Invalidate(true);
        }

        private GenusPanel? createGenusPanel(BioGenus genus, SystemBioSummary? summary)
        {
            // skip if not in system?
            if (filtered && summary?.hasGenus(genus.name) != true) return null;
            var currentBodyShortName = Game.activeGame?.targetBodyShortName ?? "";

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
            if (summary?.genusBodies.GetValueOrDefault(genus.name)?.Contains(currentBodyShortName) == true)
                lbl.ForeColor = GameColors.Cyan;

            foreach (var species in genus.species.OrderBy(_ => -_.reward))
            {
                // skip if not known on a specific body
                if (filtered && summary?.hasSpecies(species.name) == false) continue;

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

                if (filtered && summary?.speciesBodies[species.name].Count > 0)
                    lbl2.Text += " (on: " + string.Join(", ", summary.speciesBodies[species.name]) + ")";

                if (summary?.speciesBodies.GetValueOrDefault(species.name)?.Contains(currentBodyShortName) == true)
                    lbl2.ForeColor = GameColors.Cyan;
            }

            if (filtered && summary?.hasUnknownSpecies(genus.name) == true)
            {
                // unknown bodies label
                var lbl3 = new Label()
                {
                    Text = $"Unknown (on: {string.Join(", ", summary.unknownSpeciesBodies[genus.name])})",
                    Font = this.fontSpecies,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Margin = new Padding(36, 0, 0, 0),
                    Padding = new Padding(0),
                };
                panel.Controls.Add(lbl3);

                if (summary?.unknownSpeciesBodies.GetValueOrDefault(genus.name)?.Contains(currentBodyShortName) == true)
                    lbl3.ForeColor = GameColors.Cyan;
            }

            return panel;
        }

        private void toolInSystem_Click(object sender, EventArgs e)
        {
            var tool = (ToolStripItem)sender;
            toolSystemFilter.Text = tool.Text;

            this.filtered = tool.Name != nameof(toolEverything);
            toolInSystem.Checked = this.filtered;
            toolEverything.Checked = !this.filtered;

            this.populateGenus();
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            this.populateGenus();
        }

        private void toolSizeSmall_Click(object sender, EventArgs e)
        {
            uberPanel.SuspendLayout();

            this.fontGenus = new Font(this.Font.FontFamily, 12f);
            this.fontSpecies = new Font(this.Font.FontFamily, 8f);

            foreach (GenusPanel panel in uberPanel.Controls)
                panel.SuspendLayout();

            foreach (GenusPanel panel in uberPanel.Controls)
                panel.updateFontSizes(this.fontGenus, this.fontSpecies);

            foreach (GenusPanel panel in uberPanel.Controls)
                panel.ResumeLayout();

            uberPanel.Invalidate(true);
            uberPanel.ResumeLayout();
        }

        private void toolSizeMedium_Click(object sender, EventArgs e)
        {
            uberPanel.SuspendLayout();

            this.fontGenus = new Font(this.Font.FontFamily, 18f);
            this.fontSpecies = new Font(this.Font.FontFamily, 10f);

            foreach (GenusPanel panel in uberPanel.Controls)
                panel.SuspendLayout();

            foreach (GenusPanel panel in uberPanel.Controls)
                panel.updateFontSizes(this.fontGenus, this.fontSpecies);

            foreach (GenusPanel panel in uberPanel.Controls)
                panel.ResumeLayout();

            uberPanel.Invalidate(true);
            uberPanel.ResumeLayout();
        }
    }

    class GenusPanel : FlowLayoutPanel
    {
        public bool showTopLine = true;

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

            var genus = this.Tag as BioGenus;
            if (genus == null) return;

            // the top line
            if (this.showTopLine)
                g.DrawLine(GameColors.penOrangeStripe3, 4, 2, this.Width, 2);

            // the reward per species
            //var reward = genus.minReward;
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

                    var b = GameColors.brushGameOrange;

                    var currentBodyShortName = Game.activeGame?.targetBodyShortName ?? "";
                    if (ctrl.ForeColor == ((SolidBrush)GameColors.brushCyan).Color)
                    {
                        b = GameColors.brushCyan;
                        //if (species != null)
                        //    reward = (int)Math.Max(reward, species.reward);
                    }

                    g.DrawString(
                        txt,
                        ctrl.Font,
                        b,
                        this.Width - 170, this.Controls[n].Top);
                }
            }

            // the ring graphic
            // TODO: could we highlight the min/max reward?
            PlotBase.drawBioRing(g, genus.name, this.ClientSize.Width - 38 - 8, 10, -1, null, 38, genus.maxReward);
        }
    }
}