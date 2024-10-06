using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.Properties;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    public partial class FormCodexBingo : Form
    {
        #region static form loading

        public static FormCodexBingo? activeForm;
        private Brush treeBackBrush;

        public static void show()
        {
            if (activeForm == null)
                FormCodexBingo.activeForm = new FormCodexBingo();

            Util.showForm(FormCodexBingo.activeForm);
        }

        #endregion

        private readonly static Brush brushPartial = Brushes.Orange;
        private readonly static Brush brushComplete = Brushes.Lime;

        private Dictionary<string, string> allCmdrs;
        private CommanderCodex cmdrCodex;
        private int regionIdx;
        private float onePoint;

        public FormCodexBingo()
        {
            InitializeComponent();
            treeBackBrush = new SolidBrush(tree.BackColor);
            tree.Nodes.Clear();
            tree.MouseWheel += Tree_MouseWheel;
            tree.TreeViewNodeSorter = new NodeSorter();

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formCodexBingo);

            // use an empty icon so the TreeView reserves space for it
            images.Images.Add("CodexBlank", Resources.CodexBlank);

            // prep combos
            findCmdrs();

            // set initial text to status strip members
            toolFiller.Spring = true;
            toolBodyName.Text = "";
            toolRegionName.Text = "Select a codex discovery...";
            toolDiscoveryDate.Text = "";

            // Not themed - this is always dark.
        }

        private void findCmdrs()
        {
            this.allCmdrs = CommanderSettings.getAllCmdrs();
            var cmdrs = this.allCmdrs
                .Values
                .Order()
                .ToArray();

            comboCmdr.Items.Clear();
            comboCmdr.Items.AddRange(cmdrs);

            if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
                comboCmdr.SelectedItem = Game.settings.preferredCommander;
            else if (!string.IsNullOrEmpty(Game.settings.lastCommander))
                comboCmdr.SelectedItem = Game.settings.lastCommander;
            else
                comboCmdr.SelectedIndex = 0;
        }

        private void comboCmdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var commander = comboCmdr.Text!;
            var fid = this.allCmdrs.First(_ => _.Value == commander).Key!;
            Game.log($"Loading completions for: {commander} ({fid})");

            // prep cmdr's codex, using live one if present
            if (Game.activeGame?.cmdrCodex?.fid == fid)
                this.cmdrCodex = Game.activeGame.cmdrCodex;
            else
                this.cmdrCodex = CommanderCodex.Load(fid, commander);

            fillRegionMenuItems();

            this.calcCompletions();
        }

        private void fillRegionMenuItems()
        {
            comboRegion.Items.Clear();
            comboRegion.Items.Add("All regions");
            comboRegion.SelectedIndex = 0;

            foreach (var entry in GalacticRegions.mapRegions)
            {
                var regionId = GalacticRegions.getIdxFromName(entry.Key);
                var txt = $"#{regionId} {entry.Value}";

                if (Game.activeGame?.cmdr != null && Game.activeGame.cmdr.fid == this.cmdrCodex.fid)
                {
                    var cmdrCurrentRegionId = GalacticRegions.getIdxFromName(Game.activeGame.cmdr.galacticRegion);
                    if (regionId == cmdrCurrentRegionId)
                        txt += " ⭐";
                }

                comboRegion.Items.Add(txt);
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

                // hierarchy: /hud_category/sub_class
                var subClassText = entry.sub_class;
                if (subClassText == "Shrubs") subClassText += " (Frutexa)";
                var subClass = category.Nodes.Set(entry.sub_class, subClassText);
                subClass.Tag = new CodexTag(entry.sub_class);
                var subClassTag = (CodexTag)subClass.Tag;

                var match = Game.codexRef.matchFromEntryId(entry.entryid, true);
                if (entry.platform == "legacy" || entry.hud_category != "Biology")
                {
                    var leafParent = subClass;
                    if (entry.english_name.Contains("Mollusc"))
                    {
                        // Add an extra node to split apart the many types Molluscs
                        var groupName = entry.english_name.Substring(entry.english_name.IndexOf(' ') + 1);
                        leafParent = subClass.Nodes.Set(groupName, groupName);
                        leafParent.Tag = new CodexTag(groupName) { species = groupName };
                    }

                    // hierarchy: /hud_category/sub_class/entryId
                    var leafName = entry.english_name;
                    if (leafName.EndsWith(" " + leafParent.Text) && leafName != leafParent.Text && leafParent.Text != "Tubers")
                        leafName = leafName.Replace(" " + leafParent.Text, "");

                    var leaf = leafParent.Nodes.Set(entry.entryid, leafName);
                    var leafTag = new CodexTag(entry.english_name, entry);
                    leaf.Tag = leafTag;

                    if (match?.genus != null && subClassTag.genus == null)
                        subClassTag.genus = match.genus.englishName;
                }
                else
                {
                    if (subClassTag.genus == null)
                        subClassTag.genus = match.genus.englishName;

                    // hierarchy: /hud_category/sub_class/species
                    // extract species name and colour variant
                    var parts = entry.english_name.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 4) Debugger.Break();

                    var species = subClass.Nodes.Set(parts[1], parts[1]);
                    if (species.Tag == null)
                        species.Tag = new CodexTag(parts[1]) { species = match.species.englishName };

                    var variant = species.Nodes.Set(entry.entryid, parts[3]);
                    variant.Tag = new CodexTag(parts[3], entry) { variant = match.variant.englishName };
                }
            }

            // finally, update the tree
            this.BeginInvoke(() =>
            {
                tree.Nodes.Clear();
                tree.Nodes.Add(root);

                // calculate completions
                this.calcCompletions();
            });
        }

        public void calcCompletions()
        {
            // re-calculate completions
            if (!this.IsDisposed && tree.Nodes.Count > 0)
            {
                var root = calcNodeCompletion(tree.Nodes[0]);
                this.Text = "Codex Bingo - " + root.completion.ToString("p2");
                this.onePoint = 1f / root.countTotal;
                toolFiller.Text = $"(one entry is " + this.onePoint.ToString("p2") + ")";
                tree.Invalidate();
            }
        }

        private CodexTag calcNodeCompletion(TreeNode node)
        {
            var codexTag = (CodexTag)node.Tag;

            if (node.Nodes.Count == 0)
            {
                // leaf nodes...
                if (codexTag.entry == null) throw new Exception("Why?");

                var isDiscovered = cmdrCodex.isDiscoveredInRegion(codexTag.entry.entryid, this.regionIdx);
                codexTag.countTotal = 1;
                codexTag.countDiscovered = isDiscovered ? 1 : 0;
                codexTag.completion = codexTag.countDiscovered;
            }
            else
            {
                // sum discoveries and totals from children
                codexTag.countTotal = 0;
                codexTag.countDiscovered = 0;
                foreach (TreeNode child in node.Nodes)
                {
                    var childTag = calcNodeCompletion(child);
                    codexTag.countTotal += childTag.countTotal;
                    codexTag.countDiscovered += childTag.countDiscovered;
                }
                // calc our calculation based on those totals
                codexTag.completion = 1f / codexTag.countTotal * codexTag.countDiscovered;
            }

            return codexTag;
        }

        private void comboRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.Created) return;
            regionIdx = comboRegion.SelectedIndex;

            this.calcCompletions();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            tree.Nodes.Clear();
        }

        private void tree_Layout(object sender, LayoutEventArgs e)
        {
            // force a redraw if 'bounds' has changes (happens when scroll bars are auto added or removed)
            if (e.AffectedControl != null && e.AffectedProperty == "Bounds")
                e.AffectedControl.Invalidate();
        }

        private void tree_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = tree.GetNodeAt(e.Location);
                if (node != null && node.Bounds.Contains(e.Location))
                {
                    var codexTag = node?.Tag as CodexTag;
                    var hasEntryId = codexTag?.entry != null;
                    var hasSpecies = codexTag?.species != null;
                    var hasGenus = codexTag?.genus != null;

                    menuCopyEntryId.Visible = hasEntryId;
                    menuCanonnResearch.Visible = hasEntryId;
                    menuCanonnBioforge.Visible = hasEntryId || hasSpecies;
                    menuEDAstro.Visible = hasGenus;
                    menuCanonnSeparator.Visible = hasEntryId || hasSpecies || hasGenus;

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

        private CodexTag? getSelectedNodeCodexTag()
        {
            if (tree.SelectedNode == null) return null;
            return tree.SelectedNode.Tag as CodexTag;
        }

        private void menuCopyName_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();

            if (codexTag?.variant != null)
                Clipboard.SetText(codexTag.variant);
            else if (codexTag?.species != null)
                Clipboard.SetText(codexTag.species);
            else if (tree.SelectedNode != null)
                Clipboard.SetText(tree.SelectedNode.Text);
        }

        private void menuCopyEntryId_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.entry == null) return;

            Clipboard.SetText(codexTag.entry.entryid);
        }

        private void menuViewOnCanonnResearch_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.entry == null) return;

            var url = $"https://canonn-science.github.io/Codex-Regions/?entryid={Uri.EscapeDataString(codexTag.entry.entryid)}&hud_category={Uri.EscapeDataString(codexTag.entry.hud_category)}";
            Util.openLink(url);
        }

        private void menuViewOnCanonnBioforge_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            if (codexTag == null) return;

            string txt;
            if (codexTag.variant != null)
                txt = codexTag.variant;
            else if (codexTag.species != null)
                txt = codexTag.species;
            else
                txt = codexTag.text;

            if (string.IsNullOrEmpty(txt)) return;

            var url = $"https://bioforge.canonn.tech/?entryid={Uri.EscapeDataString(txt)}";
            Util.openLink(url);
        }

        private void menuEDAstro_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.genus == null) return;

            var genusName = codexTag.genus.Replace(' ', '-').ToLowerInvariant();

            // some special cases
            if (genusName.EndsWith("anemone")) genusName = "anemone";

            var url = $"https://edastro.b-cdn.net/mapcharts/organic/organic-{Uri.EscapeDataString(genusName)}-regions.jpg";
            Util.openLink(url);
        }

        private void toolBodyName_Click(object sender, EventArgs e)
        {
            var url = toolBodyName.Tag as string;
            if (string.IsNullOrEmpty(url)) return;

            Util.openLink(url);
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            toolRegionName.Text = "Select a codex discovery...";
            toolBodyName.IsLink = false;
            toolBodyName.Text = null;
            toolBodyName.Tag = null;
            toolDiscoveryDate.Text = null;

            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.entry == null) return;

            // get cmdr's personal discovery date and location
            var entry = cmdrCodex.getEntry(codexTag.entry.entryid, this.regionIdx);
            if (entry == null) return;


            if (entry.address == -1)
            {
                // data imported from Canonn Challenge - not much we can do with this
                toolBodyName.IsLink = false;
                toolBodyName.Text = "?";
                toolRegionName.Text = "Imported from Canonn Challenge";
                toolDiscoveryDate.Text = "?";
                return;
            }
            else
            {
                // show data and start (cached) Spansh lookup for the system name from systemAddress/bodyId
                toolDiscoveryDate.Text = entry.time.ToString();
                toolBodyName.Text = $"{entry.address} / {entry.bodyId} ...";
                toolRegionName.Text = "..?";
            }

            Game.spansh.getSystemDump(entry.address, true).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    string? bodyName = null;
                    string? url = null;
                    if (entry.bodyId >= 0)
                    {
                        var body = t.Result.bodies?.Find(b => b.bodyId == entry.bodyId);
                        if (body?.id64 != null)
                        {
                            // link to the body
                            bodyName = body.name ?? $"{entry.address} #{entry.bodyId}";
                            url = $"https://spansh.co.uk/body/{Uri.EscapeDataString(body.id64.Value.ToString())}";
                        }
                    }
                    if (bodyName == null || url == null)
                    {
                        // link to just the system
                        bodyName = $"{t.Result.name} ?";
                        url = $"https://spansh.co.uk/system/{Uri.EscapeDataString(entry.address.ToString())}";
                    }

                    this.BeginInvoke(() =>
                    {
                        // set galactic region
                        var starPos = t.Result.coords;
                        var region = EliteDangerousRegionMap.RegionMap.FindRegion(starPos.x, starPos.y, starPos.z);
                        toolRegionName.Text = region.Name;

                        // set body name, with URL for clicking
                        toolBodyName.Text = bodyName;
                        if (!string.IsNullOrEmpty(url))
                        {
                            toolBodyName.IsLink = true;
                            toolBodyName.Tag = url;
                        }
                    });
                }
                else if (t.Exception?.InnerException is HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound } || Util.isFirewallProblem(t.Exception))
                {
                    this.BeginInvoke(() =>
                    {
                        // ignore NotFound responses
                        toolBodyName.Text = $"Unknown system: {entry.address}?";
                    });
                }
            });
        }

        private void toolImportFromJournal_Click(object sender, EventArgs e)
        {
            // start watching for file changes first
            var watcher = this.reactToOldJournalChanges();

            // spawn child process to do the importing
            var proc = Process.Start(Application.ExecutablePath, $"{Program.cmdArgScanOld} {cmdrCodex.fid}");

            // when that child process ends, reload + silently re-calculate one more time
            proc.WaitForExitAsync().ContinueWith(t =>
            {
                this.BeginInvoke(() =>
                {
                    Game.log($"Process old journals child processed ended, re-calculating completions for cmdr: {cmdrCodex.commander}");
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                    cmdrCodex.reload();
                    this.calcCompletions();
                });
            });
        }

        private FileSystemWatcher reactToOldJournalChanges()
        {
            var watcher = new FileSystemWatcher(Program.dataFolder, $"{cmdrCodex.fid}-codex*.json");

            var lastChangeTime = DateTime.Now;
            watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) =>
            {
                Program.crashGuard(() =>
                {
                    if (!this.IsHandleCreated || this.IsDisposed) return;

                    // ignore changes for other cmdrs
                    var fileFID = e.Name?.Split('-', 2)?.FirstOrDefault();
                    if (fileFID != cmdrCodex.fid) return;

                    // ignore if we updated within the last second
                    var duration = DateTime.Now - lastChangeTime;
                    if (duration.TotalSeconds < 1) return;

                    // silently reload and re-calculate completions
                    Game.log($"Process old journals child processed touched files, re-calculating completions for cmdr: {cmdrCodex.commander}");
                    cmdrCodex.reload();
                    this.calcCompletions();
                    lastChangeTime = DateTime.Now;
                }, true);
            });

            Game.log($"Watching: {watcher.Filter}");
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void toolImportFromCanonn_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            Game.canonn.importCanonnChallenge(this.cmdrCodex).ContinueWith(t =>
            {
                this.BeginInvoke(() =>
                {
                    // re-calculate completions
                    this.calcCompletions();
                    this.Enabled = true;

                    MessageBox.Show(this, $"Added {t.Result} new entries from Canonn Challenge data for commander: {cmdrCodex.commander}", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            });
        }

        private void toolCanonnChallenge_Click(object sender, EventArgs e)
        {
            Util.openLink("https://canonn.science/codex/canonn-challenge/");
        }

        protected int scaleBy(int n)
        {
            return (int)(this.DeviceDpi / 96f * n);
        }

        private void tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;
            if (this.IsDisposed || e.Node == null || e.Node.Tag == null || e.Node.IsVisible == false || this.tree.Nodes.Count == 0) return;
            var codexTag = e.Node.Tag as CodexTag;
            if (codexTag == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var b = codexTag.isComplete ? brushComplete : brushPartial;

            // completion pie icon
            var rPie = new Rectangle(e.Bounds.X - 17, e.Bounds.Y + scaleBy(3), 13, 13);
            g.FillRectangle(this.treeBackBrush, rPie);
            var fillAngle = 360f * codexTag.completion;
            g.FillPie(b, rPie, -90, fillAngle);
            g.DrawEllipse(GameColors.penBlack2, rPie);

            // don't draw anything else if we have no children
            if (!e.Node.HasChildren()) return;

            // percent complete text
            var pt = new Point(e.Bounds.Right, e.Bounds.Top);
            g.FillRectangle(this.treeBackBrush, pt.X, pt.Y, 60, e.Bounds.Height - 4);
            var txt = $" ({codexTag.completion.ToString("p1")})";
            if (tree.Nodes.Count > 0 && e.Node == tree.Nodes[0])
                txt = $" ({codexTag.completion.ToString("p2")})";
            TextRenderer.DrawText(g, txt, tree.Font, pt, tree.ForeColor);

            // completion right bar
            b = codexTag.completion == 1 ? brushComplete : brushPartial;
            var barWidth = 200f;
            var rBar = new RectangleF(
                tree.ClientSize.Width - 210,
                e.Bounds.Y + 1,
                barWidth * codexTag.completion,
                e.Bounds.Height - 4
            );

            // fill based on completion
            g.FillRectangle(b, rBar);

            // outline the full width
            rBar.Width = barWidth;
            g.DrawRectangle(GameColors.penBlack2, rBar);

            // draw segment lines if the children are all leaf nodes
            if (!e.Node.HasGrandChildren())
            {
                var childCount = e.Node.Nodes.Count;
                var delta = (barWidth / childCount);
                for (int n = 0; n < childCount; n++)
                {
                    var x = rBar.Left + (n * delta);
                    g.DrawLine(GameColors.penBlack2, x, rBar.Top, x, rBar.Bottom);
                }
            }
        }
    }

    internal class CodexTag
    {
        public string text;
        public RefCodexEntry? entry;
        public float completion;
        public string? variant;
        public string? species;
        public string? genus;

        public int countTotal;
        public int countDiscovered;

        public CodexTag(string text, RefCodexEntry? entry = null)
        {
            this.text = text;
            this.entry = entry;
        }

        public bool isComplete { get => this.completion == 1f; }
    }

    public class NodeSorter : IComparer
    {
        public int Compare(object? left, object? right)
        {
            var leftNode = (TreeNode?)left;
            var rightNode = (TreeNode?)right;
            return string.Compare(leftNode?.Text ?? "", rightNode?.Text ?? "");
        }
    }
}