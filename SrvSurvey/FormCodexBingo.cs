using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    public partial class FormCodexBingo : Form
    {
        #region static form loading

        public static FormCodexBingo? activeForm;

        public static void show()
        {
            if (activeForm == null)
                FormCodexBingo.activeForm = new FormCodexBingo();

            Util.showForm(FormCodexBingo.activeForm);
        }

        #endregion

        private CommanderCodex cmdrCodex;
        private int regionIdx;
        private bool closing;
        static Pen pb2 = new Pen(Color.Black, 2f);

        public FormCodexBingo()
        {
            InitializeComponent();
            tree.Nodes.Clear();
            tree.MouseWheel += Tree_MouseWheel;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formCodexBingo);

            // use an empty icon so the TreeView reserves space for it
            images.Images.Add("CodexBlank", Resources.CodexBlank);

            // prep cmdr's codex
            var game = Game.activeGame;
            var fid = game?.fid ?? Game.settings.lastFid;
            var commander = game?.Commander ?? Game.settings.lastCommander;
            txtCommander.Text = commander;
            this.cmdrCodex = CommanderCodex.Load(fid!, commander!);
            this.Text += " - " + this.cmdrCodex.progress.ToString("p0");

            // prep region menu
            fillRegionMenuItems();

            toolFiller.Spring = true;
            toolBodyName.Text = "";
            toolRegionName.Text = "Select a codex discovery...";
            toolDiscoveryDate.Text = "";

            var toolScanOld = new ToolStripButton("Scan old journal files?");
            toolScanOld.Click += ToolScanOld_Click;
            this.statusStrip1.Items.Add(toolScanOld);

        }

        private void ToolScanOld_Click(object? sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath, FormPostProcess.cmdArg);
        }

        private void fillRegionMenuItems()
        {
            comboRegion.Items.Clear();
            comboRegion.Items.Add("Any region");
            comboRegion.SelectedIndex = 0;

            foreach (var entry in GalacticRegions.mapRegions)
            {
                var regionId = GalacticRegions.getIdxFromName(entry.Key);
                comboRegion.Items.Add($"#{regionId} {entry.Value}");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.prepNodesAsync().ContinueWith(t => Game.log("Nodes ready"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.prepNodesAsync().ContinueWith(t => Game.log("Nodes ready"));
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formCodexBingo != rect)
            {
                Game.settings.formCodexBingo = rect;
                Game.settings.Save();
            }

            tree.Invalidate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormCodexBingo.activeForm = null;
        }

        private async Task prepNodesAsync()
        {
            // start with a root node to contain everything else
            var root = new TreeNode()
            {
                Name = "root",
                Text = "The Codex",
                Tag = new CodexTag("The Codex"),
            };
            root.Expand();

            // hierarchy: /hud_category/sub_class/species/color

            // build the hierarchy
            var codex = await Game.codexRef.loadCodexRef(false);
            foreach (var entry in codex.Values)
            {
                var hudCategory = entry.hud_category;
                if (hudCategory == "None") hudCategory += " (More Thargoid)";

                // hierarchy: /hud_category
                var category = root.Nodes.Set(hudCategory, hudCategory);
                category.Tag = new CodexTag(hudCategory);

                var standardCase = true; // entry.sub_class != entry.hud_category; // && entry.sub_class != "Thargoid";
                // hierarchy: /hud_category/sub_class
                TreeNode subClass;
                if (standardCase)
                {
                    subClass = category.Nodes.Set(entry.sub_class, entry.sub_class);
                    subClass.Tag = new CodexTag(entry.sub_class);
                }
                else
                {
                    subClass = category;
                }

                var subClassTag = subClass.Tag as CodexTag;
                var match = Game.codexRef.matchFromEntryId(entry.entryid, true);

                if (entry.platform == "legacy" || entry.hud_category != "Biology")
                {
                    // hierarchy: /hud_category/sub_class/entryId
                    var leaf = subClass.Nodes.Set(entry.entryid, entry.english_name);
                    leaf.Text += $" ({entry.entryid})";
                    var leafTag = new CodexTag(entry.english_name, entry);
                    leaf.Tag = leafTag;

                    if (match?.genus != null)
                    {
                        if (subClassTag.genus == null)
                            subClassTag.genus = match.genus;
                    }
                }
                else
                {
                    if (subClassTag.genus == null)
                        subClassTag.genus = match.genus;

                    // hierarchy: /hud_category/sub_class/species
                    // extract species name and colour variant
                    var parts = entry.english_name.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 4) Debugger.Break();

                    var species = subClass.Nodes.Set(parts[1], parts[1]);
                    if (species.Tag == null)
                        species.Tag = new CodexTag(parts[1]) { species = match.species };

                    var variant = species.Nodes.Set(entry.entryid, parts[3]);
                    variant.Text += $" ({entry.entryid})";
                    variant.Tag = new CodexTag(parts[3], entry) { variant = match.variant };
                }
            }

            // calculate completions
            getNodeCompletion(root);

            // finally, update the tree
            this.BeginInvoke(() =>
            {
                tree.Nodes.Clear();
                tree.Nodes.Add(root);
            });
        }

        private float getNodeCompletion(TreeNode node)
        {
            var codexTag = (CodexTag)node.Tag;

            // leaf nodes are "complete" if checked
            if (node.Nodes.Count == 0)
            {
                if (codexTag.entry == null) throw new Exception("Why?");

                var isDiscovered = cmdrCodex.isDiscoveredInRegion(codexTag.entry.entryid, this.regionIdx);
                //cmdrCodex.isDiscovered(codexTag.entry.entryid)


                codexTag.completion = isDiscovered ? 1 : 0;
            }
            else
            {
                var sum = 0f;
                foreach (TreeNode child in node.Nodes)
                    sum += getNodeCompletion(child);

                var completion = 1f / node.Nodes.Count * sum;

                //var newText = $"{codexTag.text} ({completion.ToString("p1")})";
                //if (node.Text != newText) 
                //    node.Text = newText;

                codexTag.completion = completion;
            }


            return codexTag.completion;
        }

        private void comboRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.Created) return;
            regionIdx = comboRegion.SelectedIndex;

            // re-calculate completions
            getNodeCompletion(tree.Nodes[0]);
            tree.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            tree.Nodes.Clear();
            tree.SuspendLayout();
            this.closing = true;
        }

        private void tree_Layout(object sender, LayoutEventArgs e)
        {
            Game.log($"tree_Layout: {e.AffectedControl?.Name} / {e.AffectedProperty}");

            if (e.AffectedProperty == "Bounds" && e.AffectedControl != null)
                e.AffectedControl.Invalidate();
        }

        private void tree_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = tree.GetNodeAt(e.Location);
                var codexTag = node?.Tag as CodexTag;
                if (node != null && node.Bounds.Contains(e.Location)) // && codexTag?.entry != null)
                {
                    var hasEntryId = codexTag?.entry != null;
                    var hasSpecies = codexTag?.species != null;
                    var hasGenus = codexTag?.genus != null;
                    menuCopyEntryId.Visible = hasEntryId;
                    menuCanonnResearch.Visible = hasEntryId;
                    menuCanonnBioforge.Visible = hasEntryId || hasSpecies;
                    menuEDAstro.Visible = hasGenus;
                    menuCanonnSeperator.Visible = hasEntryId || hasSpecies || hasGenus;


                    Game.log(node.Text);
                    tree.SelectedNode = node;
                    contextMenu.Show(tree.PointToScreen(e.Location));
                }
                else
                {
                    tree.SelectedNode = null;
                }
            }
        }

        private void Tree_MouseWheel(object? sender, MouseEventArgs e)
        {
            contextMenu.Hide();
        }

        private void menuCopyName_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null) return;
            var codexTag = tree.SelectedNode.Tag as CodexTag;
            if (codexTag == null) return;

            if (codexTag.variant != null)
                Clipboard.SetText(codexTag.variant.englishName);
            else if (codexTag.species != null)
                Clipboard.SetText(codexTag.species.englishName);
            else
                Clipboard.SetText(tree.SelectedNode.Text);
        }

        private void menuCopyEntryId_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null) return;
            var codexTag = tree.SelectedNode.Tag as CodexTag;
            if (codexTag?.entry == null) return;

            Clipboard.SetText(codexTag.entry.entryid);
        }

        private void menuViewOnCanonnResearch_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null) return;
            var codexTag = tree.SelectedNode.Tag as CodexTag;
            if (codexTag?.entry == null) return;

            var url = $"https://canonn-science.github.io/Codex-Regions/?entryid={Uri.EscapeDataString(codexTag.entry.entryid)}&hud_category={Uri.EscapeDataString(codexTag.entry.hud_category)}";
            Util.openLink(url);
        }

        private void menuViewOnCanonnBioforge_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null) return;
            var codexTag = tree.SelectedNode.Tag as CodexTag;

            var txt = "";
            if (codexTag?.variant != null)
                txt = codexTag.variant.englishName;
            else if (codexTag?.species != null)
                txt = codexTag.species.englishName;
            else
                txt = codexTag?.text;

            var url = $"https://bioforge.canonn.tech/?entryid={Uri.EscapeDataString(txt)}";
            Util.openLink(url);
        }

        private void menuEDAstro_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null) return;
            var codexTag = tree.SelectedNode.Tag as CodexTag;
            if (codexTag?.genus == null) return;

            var genusName = codexTag.genus.englishName.Replace(' ', '-').ToLowerInvariant();

            // some special cases
            if (genusName.EndsWith("anemone")) genusName = "anemone";
            var url = $"https://edastro.b-cdn.net/mapcharts/organic/organic-{Uri.EscapeDataString(genusName)}-regions.jpg";
            Util.openLink(url);
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            toolBodyName.Text = null;
            toolBodyName.Tag = null;
            toolRegionName.Text = "Select a codex discovery...";
            toolDiscoveryDate.Text = null;


            if (tree.SelectedNode != null)
            {
                var codexTag = tree.SelectedNode.Tag as CodexTag;
                if (codexTag?.entry?.entryid == null) return;

                var entry = cmdrCodex.getEntry(codexTag.entry.entryid, this.regionIdx);
                if (entry == null) return;

                toolDiscoveryDate.Text = entry.time.ToString();
                toolBodyName.Text = "...";
                toolRegionName.Text = "...";
                //txtFoo.Text = "...";

                Game.spansh.getSystemDump(entry.address, true).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        var body = t.Result.bodies?.Find(b => b.bodyId == entry.bodyId);
                        var starPos = t.Result.coords;
                        if (body == null) return;

                        this.BeginInvoke(() =>
                        {
                            toolBodyName.Text = body?.name ?? $"{entry.address} #{entry.bodyId}";
                            //txtFoo.Text = body?.name ?? $"{entry.address} #{entry.bodyId}";

                            //var sz = TextRenderer.MeasureText(txtFoo.Text, txtFoo.Font, Size.Empty, TextFormatFlags.TextBoxControl);
                            //var sz = Graphics.FromHwnd(this.Handle).MeasureString(txtFoo.Text, txtFoo.Font, 10, StringFormat.text);
                            //txtFoo.Width = (int)sz.Width;

                            if (regionIdx == 0)
                            {
                                var region = EliteDangerousRegionMap.RegionMap.FindRegion(starPos.x, starPos.y, starPos.z);
                                toolRegionName.Text = region.Name;
                            }

                            if (entry.bodyId > 0)
                            {
                                var url = $"https://spansh.co.uk/body/{body!.id64}";
                                toolBodyName.Tag = url;
                            }
                        });
                    }
                    else if ((t.Exception?.InnerException as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(t.Exception))
                    {
                        this.BeginInvoke(() =>
                        {
                            // ignore NotFound responses
                            toolBodyName.Text = $"Unknown system: {entry.address}?";
                        });
                    }
                });
            }
        }

        private void tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (this.IsDisposed || this.closing || e.Node?.IsVisible == false) return;
            e.DrawDefault = true;
            var codexTag = e.Node?.Tag as CodexTag;
            if (codexTag == null || e.Node == null) return;

            var bp = Brushes.Orange;
            var bc = Brushes.Lime;
            //Game.log($"tree.AutoScrollOffset: {tree.AutoScrollOffset}");

            //if (e.Bounds.Y == 0 && e.Node?.Text != "The Codex")
            //    return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;



            var b = e.Node.HasChildren() && codexTag.completion < 1 ? bp : bc;

            // completion pie icon
            var r1 = new Rectangle(e.Bounds.X - 17, e.Bounds.Y + 2, 13, 13);
            g.FillRectangle(SystemBrushes.WindowFrame, r1);
            var fillAngle = 360f * codexTag.completion;
            g.FillPie(b, r1, -90, fillAngle);
            g.DrawEllipse(pb2, r1);

            var nodeHasChildren = e.Node.HasChildren();
            if (nodeHasChildren)
            {
                // percent complete text
                var pt = new Point(e.Bounds.Right, e.Bounds.Top);
                g.FillRectangle(SystemBrushes.WindowFrame, pt.X, pt.Y, 60, e.Bounds.Height - 4);
                TextRenderer.DrawText(g, $" ({codexTag.completion.ToString("p1")})", tree.Font, pt, tree.ForeColor);

                // completion right bar
                b = codexTag.completion == 1 ? bc : bp;
                var w = 200f;
                var r2 = new RectangleF(
                    tree.ClientSize.Width - 210,
                    e.Bounds.Y + 1,
                    w * codexTag.completion,
                    e.Bounds.Height - 4
                );

                // fill based on completion
                g.FillRectangle(b, r2);

                // outline the full width
                r2.Width = w;
                g.DrawRectangle(pb2, r2);

                if (r2.Top != (int)r2.Top) Debugger.Break();

                if (/*e.Node.IsExpanded &&*/ !e.Node.HasGrandChildren())
                {
                    var childCount = e.Node.GetNodeCount(false);
                    //float x = r2.Left;
                    var d = (w / childCount) + 0f;
                    for (int n = 0; n < childCount; n++)
                    {
                        //x = r2.Left + (float)Math.Round(n * d) + 0.0f;
                        var x = r2.Left + (n * d) + 0.0f;
                        if (x >= r2.Right) break;
                        g.DrawLine(pb2, x, r2.Top, x, r2.Bottom);
                    }
                }
            }
            else
            {
                //tree.Invalidate(e.Node.Parent.Bounds);
            }

        }

        private void toolBodyName_Click(object sender, EventArgs e)
        {
            var url = toolBodyName.Tag as string;
            if (!string.IsNullOrEmpty(url))
                Util.openLink(url);
        }
    }

    internal class CodexTag
    {
        public string text;
        public RefCodexEntry? entry;
        public float completion;
        public BioVariant? variant;
        public BioSpecies? species;
        public BioGenus? genus;

        public CodexTag(string text, RefCodexEntry? entry = null)
        {
            this.text = text;
            this.entry = entry;
        }
    }

}