using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    [Draggable, TrackPosition]
    internal partial class FormCodexBingo : SizableForm
    {
        private Dictionary<string, string> edAstroLinks = new()
        {
            { "root", "https://edastro.com/mapcharts/codex.html" },
            { "*Anomaly", "https://edastro.b-cdn.net/mapcharts/codex/codex-anomalies-regions.jpg" },
            { "*Mollusc", "https://edastro.b-cdn.net/mapcharts/codex/codex-molluscs-regions.jpg" },
            { "Lagrange*", "https://edastro.b-cdn.net/mapcharts/codex/codex-lagrangeclouds-regions.jpg" },
            { "Storm*", "https://edastro.b-cdn.net/mapcharts/codex/codex-lagrangeclouds-regions.jpg" },
            { "*Crystals", "https://edastro.b-cdn.net/mapcharts/codex/codex-crystals-regions.jpg" },
            { "Guardian", "https://edastro.b-cdn.net/mapcharts/codex/codex-aliens-regions.jpg" },
            { "Thargoid", "https://edastro.b-cdn.net/mapcharts/codex/codex-aliens-regions.jpg" },
        };

        private Brush treeBackBrush;
        private readonly static Brush brushPartial = Brushes.Orange;
        private readonly static Brush brushComplete = Brushes.Lime;

        private CommanderCodex cmdrCodex;
        private int regionIdx;
        private float onePoint;

        public FormCodexBingo()
        {
            InitializeComponent();
            this.Icon = Icons.prize;

            treeBackBrush = new SolidBrush(tree.BackColor);
            tree.Nodes.Clear();
            tree.MouseWheel += Tree_MouseWheel;
            tree.ShowNodeToolTips = true;
            tree.TreeViewNodeSorter = new NodeSorter();
            toolImport.DropDownDirection = ToolStripDropDownDirection.AboveLeft;

            // use an empty icon so the TreeView reserves space for it
            images.Images.Add("CodexBlank", ImageResources.CodexBlank);

            // set initial text to status strip members
            toolFiller.Spring = true;
            toolBodyName.Text = "";
            toolRegionName.Text = "Select a codex discovery...";
            toolDiscoveryDate.Text = "";

            comboRegion.Enabled = false;

            // Not themed - this is always dark.
        }

        private CommanderSettings? currentCmdr
        {
            get => this.cmdrCodex == null ? null : Game.activeGame?.cmdr ?? CommanderSettings.Load(this.cmdrCodex.fid, true, this.cmdrCodex.commander);
        }

        private void comboCmdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var commander = comboCmdr.cmdrName;
            var fid = comboCmdr.cmdrFid;
            toolUndiscovered.Enabled = fid != null;
            toolImportFromCanonn.Enabled = fid != null;
            toolImportFromJournal.Enabled = fid != null;
            if (fid == null) return;

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

            comboRegion.Enabled = currentCmdr != null;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            tree.Invalidate();
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
                if (subClassText == "Shrubs") subClassText = "Frutexa (Shrubs)";
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
                        species.Tag = new CodexTag(parts[1]) { species = match.species.englishName, reward = match.species.reward };

                    var variant = species.Nodes.Set(entry.entryid, parts[3]);
                    variant.Tag = new CodexTag(parts[3], entry) { variant = match.variant.englishName };
                    variant.ToolTipText = match.variant.englishName;
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
            if (!this.IsDisposed && tree.Nodes.Count > 0 && this.cmdrCodex != null)
            {
                var root = calcNodeCompletion(tree.Nodes[0]);
                this.Text = "Codex Bingo - " + root.completion.ToString("p2");
                this.onePoint = 1f / root.countTotal;
                toolFiller.Text = $"{root.countTotal - root.countDiscovered} scans to go, each is " + this.onePoint.ToString("p2");
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
                node.ToolTipText = $"Discovered {codexTag.countDiscovered} of {codexTag.countTotal} - " + codexTag.completion.ToString("p2");

                if (codexTag.reward > 0)
                    node.ToolTipText += "\r\nReward: " + Util.credits(codexTag.reward);
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
                    var codexTag = node.Tag as CodexTag;
                    var hasEntryId = codexTag?.entry != null;
                    var hasSpecies = codexTag?.species != null;
                    var hasGenus = codexTag?.genus != null;

                    menuNearestSeparator.Visible = hasEntryId;
                    menuFindNearest.Visible = hasEntryId;
                    menuSpanshNearest.Visible = false;

                    menuCopyEntryId.Visible = hasEntryId;
                    menuCanonnResearch.Visible = hasEntryId;
                    menuCanonnBioforge.Visible = hasEntryId || hasSpecies;
                    menuEDAstro.Visible = hasGenus;
                    menuCanonnSeparator.Visible = hasEntryId || hasSpecies || hasGenus;

                    menuPreScannedSeperator.Visible = hasEntryId && cmdrCodex != null;
                    menuPreScanned.Visible = hasEntryId && cmdrCodex != null;
                    
                    // some special cases for ED Astro links
                    if (edAstroLinks.Keys.Any(k => node.Name.matches(k)))
                        menuEDAstro.Visible = true;

                    // require incomplete children to activate this one
                    if (hasSpecies)
                    {
                        var missingChildCount = this.getIncompleteChildTags(node).Count;
                        if (missingChildCount > 0)
                        {
                            menuSpanshNearest.Text = $"Find systems with {missingChildCount} missing variants ...";
                            Game.log($">?> {menuSpanshNearest.Text}");
                            menuSpanshNearest.Visible = true;
                        }
                    }

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

        private CodexTag? getSelectedNodeCodexTag(TreeNode? node = null)
        {
            node ??= tree.SelectedNode;
            if (node == null) return null;
            return node.Tag as CodexTag;
        }

        private List<CodexTag> getIncompleteChildTags(TreeNode node)
        {
            var incomplete = new List<CodexTag>();

            if (node?.Nodes.Count > 0)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    var childTag = getSelectedNodeCodexTag(child);
                    if (childTag?.entry != null && !childTag.isComplete)
                        incomplete.Add(childTag);
                }
            }

            return incomplete;
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
            // is this a special link?
            var nodeName = tree.SelectedNode.Name;
            var link = edAstroLinks.FirstOrDefault(pair => nodeName.matches(pair.Key)).Value;
            if (link != null)
            {
                Util.openLink(link);
                return;
            }

            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.genus == null) return;

            var genusName = codexTag.genus.Replace(' ', '-').ToLowerInvariant();

            // some special cases
            if (genusName.EndsWith("anemone")) genusName = "anemone";

            var url = $"https://edastro.b-cdn.net/mapcharts/organic/organic-{Uri.EscapeDataString(genusName)}-regions.jpg";
            Util.openLink(url);
        }

        private void menuFindNearest_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            var searchTerm = codexTag?.variant ?? codexTag?.text;
            if (searchTerm == null) return;

            var refPos = this.currentCmdr?.getCurrentStarPos() ?? StarPos.Sol;
            FormNearestSystems.show(refPos, searchTerm, this.currentCmdr?.commander ?? "");
        }

        private void menuSpanshNearest_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.species != null)
            {
                var variantNames = this.getIncompleteChildTags(tree.SelectedNode)
                    .Select(t => Game.codexRef.matchFromEntryId(t.entry!.entryid).variant)
                    .ToList();

                var refPos = this.currentCmdr?.getCurrentStarPos() ?? StarPos.Sol;
                FormNearestSystems.show(refPos, variantNames);
            }
        }

        private void menuPreScanned_Click(object sender, EventArgs e)
        {
            var codexTag = getSelectedNodeCodexTag();
            if (codexTag?.entry == null) return;

            var rslt = MessageBox.Show(
                this,
                "This is a manual override for when you no longer have journal files to prove something has been scanned." +
                $"\n\nCan you confirm you have scanned the following?" +
                $"\n\n{codexTag.text} (#{codexTag.entry.entryid})",
                "SrvSurvey",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            var entryId = long.Parse(codexTag.entry.entryid);
            var priorEntry = cmdrCodex.codexFirsts.GetValueOrDefault(entryId);

            var dirty = false;
            if (rslt == DialogResult.Yes)
            {
                // mark this entry as Scanned but without a location
                if (priorEntry == null)
                {
                    cmdrCodex.codexFirsts[entryId] = new CodexFirst(DateTime.Now, -1, -1);
                    cmdrCodex.Save();
                    dirty = true;
                }
            }
            else if (rslt == DialogResult.No)
            {
                if (priorEntry != null && priorEntry.address == -1)
                {
                    // remove the entry
                    cmdrCodex.codexFirsts.Remove(entryId);
                    cmdrCodex.Save();
                    dirty = true;
                }
            }

            // re-calculate completions
            if (dirty)
                Program.defer(() => this.calcCompletions());
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
            var entry = cmdrCodex?.getEntry(codexTag.entry.entryid, this.regionIdx);
            if (entry == null) return;


            if (entry.address == -1)
            {
                // data imported from Canonn Challenge - not much we can do with this
                toolBodyName.IsLink = false;
                toolBodyName.Text = "?";
                toolRegionName.Text = "Unknown location";
                toolDiscoveryDate.Text = "?";
                return;
            }
            else
            {
                // show data and start (cached) Spansh lookup for the system name from systemAddress/bodyId
                toolDiscoveryDate.Text = entry.time.ToCmdrShortDateTime24Hours();
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
            if (cmdrCodex == null) return;

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
                Program.crashGuard(true, () =>
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
                });
            });

            Game.log($"Watching: {watcher.Filter}");
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void toolImportFromCanonn_Click(object sender, EventArgs e)
        {
            if (cmdrCodex == null) return;

            this.Enabled = false;
            Game.canonn.importCanonnChallenge(this.cmdrCodex).ContinueWith(t =>
            {
                Program.defer(() =>
                {
                    // re-calculate completions
                    this.calcCompletions();
                    this.Enabled = true;

                    MessageBox.Show(this, $"Added {t.Result} new entries from Canonn Challenge data for commander: {cmdrCodex.commander}", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            });
        }

        private void toolUndiscovered_Click(object sender, EventArgs e)
        {
            if (this.currentCmdr != null)
            {
                var url = $"https://canonn-science.github.io/undiscovered-codex/?cmdr={Uri.EscapeDataString(this.currentCmdr.commander)}&System={this.currentCmdr.currentSystem}";
                Util.openLink(url);
            }
        }

        private void toolCanonnChallenge_Click(object sender, EventArgs e)
        {
            Util.openLink("https://canonn.science/codex/canonn-challenge/");
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
        public long reward;

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
        public static NodeSorter ByText { get => new NodeSorter() { sortByName = false }; }
        public static NodeSorter ByName { get => new NodeSorter() { sortByName = true }; }

        private bool sortByName;

        public int Compare(object? left, object? right)
        {
            var leftNode = (TreeNode?)left;
            var rightNode = (TreeNode?)right;
            if (sortByName)
                return string.Compare(leftNode?.Name ?? "", rightNode?.Name ?? "");
            else
                return string.Compare(leftNode?.Text ?? "", rightNode?.Text ?? "");
        }
    }
}