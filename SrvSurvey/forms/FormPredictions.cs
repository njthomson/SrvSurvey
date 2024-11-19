using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Drawing;
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;

namespace SrvSurvey
{
    internal partial class FormPredictions : SizableForm
    {
        public static void invalidate()
        {
            BaseForm.get<FormPredictions>()?.tree.Invalidate();
        }

        public static void refresh()
        {
            var form = BaseForm.get<FormPredictions>();
            if (form != null) Util.deferAfter(50, form.prepNodes);
            //if (form != null) form.prepNodes();
            //if (form != null) form.doTreeViewMode();
        }

        protected Game game = Game.activeGame!;

        // TODO: Move to GameColors ...
        private readonly static Brush brushPartial = Brushes.Orange;
        private readonly static Brush brushComplete = Brushes.GreenYellow;
        private Brush brushRowOdd = new SolidBrush(Color.FromArgb(255, 24, 24, 24));
        private Brush brushRowEven = new SolidBrush(Color.FromArgb(255, 16, 16, 16));
        private Pen penRowEven = new Pen(Color.FromArgb(255, 64, 64, 64), 2f);
        private Brush treeBackBrush;

        private Font nodeBig = new Font("Lucida Console", 12f, FontStyle.Regular, GraphicsUnit.Point);
        private Font nodeMiddle = new Font("Lucida Console", 10f, FontStyle.Regular, GraphicsUnit.Point);
        private Font nodeMiddleI = new Font("Lucida Console", 10f, FontStyle.Italic, GraphicsUnit.Point);
        private Font nodeSmall2 = new Font("Lucida Console", 9f, FontStyle.Regular, GraphicsUnit.Point);
        private Font nodeSmall = new Font("Lucida Console", 8f, FontStyle.Regular, GraphicsUnit.Point);

        private TreeNode? hoverNode;
        private float maxBodyTextWidth = 0;
        private static TreeViewMode treeMode = TreeViewMode.Everything;
        private bool currentBodyOnly = false;
        private string? lastDestination;
        private string? lastCurrentBodyName;

        public FormPredictions()
        {
            InitializeComponent();
            this.isDraggable = true;
            this.Icon = Icons.dna;
            this.toolMore.DropDownDirection = ToolStripDropDownDirection.AboveLeft;

            this.currentBodyOnly = Game.settings.formPredictionsCurrentBodyOnly;

            this.statusStrip1.ForeColor = SystemColors.WindowText;
            this.toolFiller.Spring = true;
            this.treeBackBrush = new SolidBrush(tree.BackColor);

            tree.MouseWheel += Tree_MouseWheel;
            tree.TreeViewNodeSorter = NodeSorter.ByName;
            tree.ForeColor = GameColors.Orange;

            // apply themed colours
            this.BackColor = Color.Black;
            this.ForeColor = GameColors.Orange;
            foreach (Control ctrl in this.tableTop.Controls)
            {
                ctrl.BackColor = Color.Black;
                ctrl.ForeColor = GameColors.Orange;
            }

            FlatButton.applyGameTheme(btnExpandAll, btnCollapseAll, btnCurrentBody);

            this.prepNodes();

            // Not themed - this is always dark.
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.MinimumSize = new Size(flowCounts.Right + flowCounts.Left * 2, 0);

            Game.update += Game_update;
        }

        private void Game_update(GameMode newMode, bool force)
        {
            if (this.IsDisposed || game == null) return;
            if (newMode == GameMode.MainMenu)
            {
                Game.log("Closing FormPredictions at MainMenu");
                this.Close();
                return;
            }

            //Game.log($"---> {game.targetBodyShortName} / {game.systemBody?.name}");

            // repaint if destination changed
            if (lastDestination != game.status.Destination?.Name)
                invalidate();
            lastDestination = game.status.Destination?.Name;

            // repaint if current body changed
            if (lastCurrentBodyName != game.systemBody?.name)
                doTreeViewMode();
            lastCurrentBodyName = game.systemBody?.name;
        }

        private void prepNodes()
        {
            if (game?.systemData == null)
            {
                txtSystem.Text = null;
                txtScanCount.Text = null;
                txtSysActual.Text = null;
                txtSysEst.Text = null;
                txtSysEstFF.Text = null;

                tree.Nodes.Clear();

                btnCollapseAll.Enabled = false;
                btnExpandAll.Enabled = false;
                btnCurrentBody.Enabled = false;
                toolMore.Enabled = false;

                return;
            }

            try
            {
                Game.log("FormPredictions.prepNodes()");
                tree.doNotPaint = true;
                tree.Hide();

                txtSystem.Text = game.systemData.name;
                btnCollapseAll.Enabled = true;
                btnExpandAll.Enabled = true;
                btnCurrentBody.Enabled = true;
                toolMore.Enabled = true;

                // count of signals scanned
                var systemTotal = game.systemData.bodies.Sum(_ => _.bioSignalCount);
                var systemScanned = game.systemData.bodies.Sum(_ => _.countAnalyzedBioSignals);
                txtScanCount.Text = $"{systemScanned} of {systemTotal}";

                var sysActual = game.systemData.bodies.Sum(body => body.sumAnalyzed);
                txtSysActual.Text = Util.credits(sysActual, true);

                txtSysEst.Text = Util.getMinMaxCredits(game.systemData.getMinBioRewards(false), game.systemData.getMaxBioRewards(false));

                txtSysEstFF.Text = Util.getMinMaxCredits(game.systemData.getMinBioRewards(true), game.systemData.getMaxBioRewards(true));

                // Add a node for each body that has bio signals, and a child for each signal
                var bodyTexts = new List<string>();
                var newNodes = new List<TreeNode>();
                foreach (var body in game.systemData.bodies)
                {
                    if (body.bioSignalCount == 0) continue;

                    var node = createBodyNode(body);
                    bodyTexts.Add(node.Text + "🦶");
                    newNodes.Add(node);
                }

                // remove last inner blank node from last body, add one sibling to is
                if (tree.Nodes.Count > 0 && false)
                {
                    var lastBody = tree.Nodes[tree.Nodes.Count - 1];
                    lastBody.Nodes.RemoveAt(lastBody.Nodes.Count - 1);
                    newNodes.Add(new TreeNode("ZZ"));
                }
                tree.Nodes.Clear();
                tree.Nodes.AddRange(newNodes.ToArray());

                // get widest body name
                this.maxBodyTextWidth = Util.maxWidth(nodeBig, bodyTexts.ToArray());
            }
            finally
            {
                tree.doNotPaint = false;
                Game.log("FormPredictions.prepNodes() end");
                tree.Show();
            }
            doTreeViewMode();
        }

        private TreeNode createBodyNode(SystemBody body)
        {
            var node = new TreeNode
            {
                Name = body.shortName,
                //Text = "Body: " + body.name.Replace(body.system.name + " ", ""),
                Text = $"{body.shortName}: {body.bioSignalCount} signals",
                Tag = body,
                ToolTipText = $"{body.name}\r\n- Distance to arrival: {Util.lsToString(body.distanceFromArrivalLS)}",
                NodeFont = nodeBig
            };
            if (body.firstFootFall)
                node.ToolTipText += $"\r\n- First footfall confirmed";
            //var bodyText = $"-{body.planetClass}, {body.surfaceTemperature:0}K, {body.atmosphere}";
            //node.Nodes.Add(new TreeNode(bodyText) { NodeFont = nodeSmall });

            // add any known organisms
            if (body.organisms?.Count > 0)
                foreach (var org in body.organisms)
                    if (org.entryId > 0)
                        node.Nodes.Add(createKnownNode(org));

            // add any predictions
            if (body.genusPredictions?.Count > 0)
                foreach (var genus in body.genusPredictions)
                    createPredictionNodes(genus, node); //node.Nodes.Add(createPredictionNodes(genus, node));

            node.Nodes.Add(new TreeNode("Z") { Name = "Z", Checked = true });
            return node;
        }

        private TreeNode createKnownNode(SystemOrganism org)
        {
            var text = org.variantLocalized ?? org.speciesLocalized ?? org.genusLocalized;
            var child = new TreeNode
            {
                Name = text,
                Text = text,
                ToolTipText = $"{text}\r\n- Radius: {Util.metersToString((decimal)org.range)}",
                Tag = org,
                NodeFont = nodeSmall2,
            };
            if (org.reward > 0)
                child.ToolTipText += $"\r\n- Reward: {Util.credits(org.reward)}";
            if (org.isCmdrFirst)
                child.ToolTipText += $"\r\n- Your first ever discovery ⚑";
            else if (org.isNewEntry)
                child.ToolTipText += $"\r\n- Your first discovery in this region ⚐";

            return child;
        }

        private TreeNode createPredictionNodes(SystemGenusPrediction genus, TreeNode parentNode)
        {
            //// create a node for each genus, species and variant?
            //var node = new TreeNode
            //{
            //    Name = genus.genus.englishName,
            //    Text = genus.genus.englishName,
            //    Tag = genus,
            //    NodeFont = nodeMiddleI,
            //};

            bool first = true;
            foreach (var entry in genus.species)
            {
                var text = first
                    ? entry.Key.englishName
                    : entry.Key.englishName.Replace(entry.Key.genus.englishName, "".PadLeft(entry.Key.genus.englishName.Length, ' '));

                var foo = new Foo(entry.Key, entry.Value);
                var child = new TreeNode
                {
                    Name = entry.Key.englishName,
                    Text = text,
                    ToolTipText = $"{text} (predicted)\r\n- Radius: {Util.metersToString((decimal)entry.Key.genus.dist)}\r\n- Reward: {Util.credits(entry.Key.reward)}",
                    Tag = foo,
                    NodeFont = nodeSmall2,
                    Checked = !first,
                };
                //if (entry.Value..isCmdrFirst)
                //    child.ToolTipText += $"\r\n- Your first ever discovery ⚑";
                //else if (org.isNewEntry)
                //    child.ToolTipText += $"\r\n- Your first discovery in this region ⚐";

                parentNode.Nodes.Add(child);
                first = false;
            }

            return parentNode;
        }

        private void tree_Layout(object sender, LayoutEventArgs e)
        {
            // force a redraw if 'bounds' has changes (happens when scroll bars are auto added or removed)
            if (e.AffectedControl != null && e.AffectedProperty == "Bounds")
                e.AffectedControl.Invalidate();
        }

        private void Tree_MouseWheel(object? sender, MouseEventArgs e)
        {
            // TODO: ...
            //contextMenu.Hide();
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // TODO: ...
        }

        private void tree_MouseMove(object sender, MouseEventArgs e)
        {
            var node = tree.GetNodeAt(e.Location);

            if (this.hoverNode != node)
            {
                this.hoverNode = node;
                tree.Invalidate();
            }
        }

        private void tree_MouseLeave(object sender, EventArgs e)
        {
            if (this.hoverNode != null)
            {
                this.hoverNode = null;
                tree.Invalidate();
            }
        }

        private void tree_MouseDown(object sender, MouseEventArgs e)
        {
            var node = tree.GetNodeAt(e.Location);
            if (node != null)
                tree.SelectedNode = node;

            // TODO: ...
            //if (e.Button == MouseButtons.Right)
            //{
            //    if (node != null && node.Bounds.Contains(e.Location))
            //    {
            //        //var codexTag = node?.Tag as CodexTag;

            //        //tree.SelectedNode = node;
            //        //contextMenu.Show(tree.PointToScreen(e.Location));
            //    }
            //    else
            //    {
            //        tree.SelectedNode = null;
            //    }
            //}
        }

        private void tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            //Debug.WriteLine($"OnPaint >> {e.Node.Name} / {tree.doNotPaint} / {tree.Visible}");
            if (e.Node == null || tree.doNotPaint || !tree.Visible) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            var node = e.Node;
            var idx = node.VisibleIndex();

            // erase background
            //if (e.Node.Checked) idx--;
            var odd = (idx % 2) == 0;
            var backBrush = odd ? brushRowEven : brushRowOdd;
            //if (isFocused && text != "Z") backBrush = Brushes.Navy;
            if (node == hoverNode && node.Text != "Z" && node.Text != "ZZ") backBrush = Brushes.Black;
            g.FillRectangle(backBrush, 0, e.Bounds.Y, tree.ClientSize.Width, tree.ItemHeight);

            if (node.Text == "Z") return;
            if (node.Text == "ZZ")
            {
                var brushColor = ((SolidBrush)backBrush).Color;
                if (tree.BackColor != brushColor)
                    tree.BackColor = brushColor;
                return;
            }

            if (node.Tag is SystemBody)
                drawBodyNode(g, node);
            else if (node.Tag is SystemOrganism)
                drawKnownNode(g, node);
            //else if (genus != null)
            //    drawGenusNode(g, pt, node, highlight, genus);
            else
                drawSpeciesNode(g, node);
        }

        private bool isCurrentBody(TreeNode? node)
        {
            if (node == null) return false;

            // highlight if we match the target body but we're not on a planet
            var body = node.Tag as SystemBody;
            if (body == game.systemBody)
                return true;
            else if (game.mode == GameMode.FSS && game.systemData?.lastFssBody == body)
            {
                node.EnsureVisible();
                return true;
            }
            else
                return false;
        }


        private bool highlightCurrentBody(TreeNode? node)
        {
            if (node == null) return false;

            // highlight if we match the target body but we're not on a planet
            var body = node.Tag as SystemBody;
            if (body == game.targetBody && !game.status.hasLatLong)
                return true;
            else
                return false;
        }

        private void drawSpeciesNode(Graphics g, TreeNode node)
        {
            var highlight = highlightCurrentBody(node.Parent?.Parent)
                || (game != null && game.targetBody == node.Parent?.Tag && !game.status.hasLatLong)
                || node == hoverNode;

            if (isCurrentBody(node.Parent))
                drawSideBars(g, node, highlight && node != hoverNode);

            var tuple = (Foo)node.Tag;
            var species = tuple?.Item1;
            var variants = tuple?.Item2;
            if (species == null || variants == null) return;

            var sz = TextRenderer.MeasureText(g, "N", node.NodeFont, Size.Empty, Util.textFlags);
            var pt = new Point(
                node.Level * tree.Indent + 20,
                node.Bounds.Y + Util.centerIn(node.Bounds.Height, sz.Height)
            );

            // volume bar
            var y = node.Bounds.Top + 15;
            var vc = VolColor.Blue;
            if (variants.Any(v => v.isGold)) vc = VolColor.Gold;
            VolumeBar.render(g, pt.X, y, vc, species.reward, 0, true);
            pt.X += 15;

            // render the node's text
            var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;

            TextRenderer.DrawText(g, "? ", node.NodeFont, pt, Color.DimGray, Util.textFlags);
            pt.X += TextRenderer.MeasureText(g, "? ", node.NodeFont, Size.Empty, Util.textFlags).Width;

            var text = $"{node.Text}: ";
            TextRenderer.DrawText(g, text, node.NodeFont, pt, foreColor, Util.textFlags);
            sz = TextRenderer.MeasureText(g, text, node.NodeFont, Size.Empty, Util.textFlags);

            pt.X += sz.Width;
            foreach (var item in variants)
            {
                var subText = item.prefix + item.variant.colorName;
                var subColor = foreColor;
                if (item.isGold) subColor = GameColors.Bio.gold;
                TextRenderer.DrawText(g, subText, node.NodeFont, pt, subColor, Util.textFlags);

                var sz2 = TextRenderer.MeasureText(g, subText, node.NodeFont);
                pt.X += sz2.Width;
            }

            // draw credit estimates
            g.drawTextRight(Util.credits(species.reward, true), node.NodeFont, foreColor, tree.ClientSize.Width - 60, node.Bounds.Top + 1, 0, node.Bounds.Height);
        }

        /*
        private void drawGenusNode(Graphics g, Point pt, TreeNode node, bool highlight, SystemGenusPrediction genus)
        {
            if (highlightCurrentBody(node.Parent)) highlight = true;

            if (isCurrentBody(node.Parent))
                drawSideBars(g, node, highlight);

            // not a check mark
            //g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, 10, pt.Y + 2, 10, 10);
            g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, 10, pt.Y + 2, 10, 10);

            // draw +/- graphic
            drawExpando(g, pt.X, node.Bounds.Y, node.Bounds.Height, !node.IsExpanded);
            pt.X += 20;

            // volume bar
            var y = node.Bounds.Top + 15;
            var vc = VolColor.Blue; // highlight ? VolColor.Blue : VolColor.Orange;
            if (genus.isGold) vc = VolColor.Gold;
            VolumeBar.render(g, pt.X, y, vc, genus.min, genus.max, true);
            pt.X += 14;

            // render the node's text
            var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;
            if (genus.isGold) foreColor = GameColors.Bio.gold;
            TextRenderer.DrawText(g, $"{node.Text} ?", node.NodeFont, pt, foreColor, Util.textFlags);

            // draw credit estimates?
            if (!node.IsExpanded)
                drawCreditEstimate(g, node, foreColor, Util.getMinMaxCredits(genus.min, genus.max));
        }
        */

        private void drawKnownNode(Graphics g, TreeNode node)
        {
            var org = (SystemOrganism)node.Tag;
            var highlight = highlightCurrentBody(node.Parent)
                || (game.cmdr.scanOne?.genus == org.genus && org.body == game.systemBody);

            // draw side-bars to highlight this is what we're currently scanning
            if (isCurrentBody(node.Parent))
                drawSideBars(g, node, highlight);

            highlight |= node == hoverNode;

            var tt = new TextCursor(g, tree)
            {
                dtx = node.Level * tree.Indent + 8,
                dty = node.Bounds.Y,
                font = node.NodeFont,
                centerIn = tree.ItemHeight,
            };

            // draw volume bar first, as it does not shift the cursor
            var vc = highlight ? VolColor.Blue : VolColor.Orange;
            if (org.isFirst) vc = VolColor.Gold;
            VolumeBar.render(g, tt.dtx + 12, tt.dty + 15, vc, org.reward);

            // draw the prefix flag?
            tt.color = highlight ? GameColors.Cyan : GameColors.Orange;
            if (org.isFirst) tt.color = GameColors.Bio.gold;
            if (org.prefix != null)
                tt.draw(org.prefix, nodeBig);

            // draw credits
            tt.draw(tree.ClientSize.Width - 8, Util.credits(org.reward, true), null, null, true);

            // render the node's text
            tt.draw(53, node.Text);

            // draw checkmark if done, or dots if not
            tt.dty -= 1;
            if (org.analyzed)
                tt.draw(14, "✔", tt.color == C.orange ? C.orangeDark : null);
            else if (game.cmdr.scanOne?.genus == org.genus)
                tt.draw(org.genus == game.cmdr.scanTwo?.genus ? " ⚫⚫⚪" : " ⚫⚪⚪", C.cyan);

        }

        private void drawBodyNode(Graphics g, TreeNode node)
        {
            var body = (SystemBody)node.Tag;
            var highlight = highlightCurrentBody(node)
                || node == hoverNode;

            var tt = new TextCursor(g, tree)
            {
                dtx = node.Level * tree.Indent,
                dty = node.Bounds.Y + 1,
                color = highlight ? C.cyan : C.orange,
                font = node.NodeFont,
                centerIn = tree.ItemHeight,
            };

            if (!node.IsExpanded && body.genusPredictions.Any(g => g.isGold))
                tt.color = GameColors.Bio.gold;

            if (isCurrentBody(node))
                drawSideBars(g, node, highlight && node != hoverNode);

            // draw +/- graphic
            drawExpando(g, node.Level * tree.Indent, node.Bounds.Y, node.Bounds.Height, !node.IsExpanded);

            // draw check if all scanned, or empty box if not
            if (body.bioSignalCount == body.countAnalyzedBioSignals)
                tt.draw(22, "✔");
            else
                g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, tt.dtx + 25, tt.dty + 6, 10, 10);

            // draw credits
            tt.draw(tree.ClientSize.Width - 8, body.getMinMaxBioRewards(false), null, null, true);

            // render the node's text
            var text = node.Text;
            if (body.firstFootFall) text += " 🦶 ";
            tt.draw(40, text);

            // draw body bars if collapsed
            tt.dtx = (int)this.maxBodyTextWidth + 62;
            tt.dty += 1;
            if (!node.IsExpanded)
                tt.dtx += 10 + (int)PlotBioSystem.drawBodyBars(g, body, tt.dtx, node.Bounds.Top, false);

            // and distance-to-arrival at the end
            tt.color = highlight ? GameColors.DarkCyan : GameColors.OrangeDim;
            tt.draw(Util.lsToString(body.distanceFromArrivalLS), nodeMiddle);
        }

        private void drawCreditEstimate(Graphics g, TreeNode node, Color color, string text)
        {
            //var font = node.Tag is SystemBody ? node.NodeFont : nodeSmall;
            var font = node.NodeFont;
            g.drawTextRight(text, font, color, tree.ClientSize.Width - 8, node.Bounds.Top + 1, 0, node.Bounds.Height);
        }

        private void drawSideBars(Graphics g, TreeNode node, bool highlight)
        {
            // draw side-bars to highlight this is what we're currently scanning
            var r = tree.ClientSize.Width - 3;
            g.DrawLine(highlight ? GameColors.penCyan4 : GameColors.penGameOrange2, 2, node.Bounds.Top, 2, node.Bounds.Bottom);
            g.DrawLine(highlight ? GameColors.penCyan4 : GameColors.penGameOrange2, r, node.Bounds.Top, r, node.Bounds.Bottom);
        }

        private void drawNodePie(Graphics g, Point pt, float fill, bool highlight)
        {
            // completion pie icon
            var rPie = new Rectangle(pt.X, pt.Y + 2, 13, 13);

            var fillAngle = 360f * fill;
            //var b = highlight ? GameColors.brushCyan : GameColors.brushGameOrange;
            //var p = highlight ? GameColors.penDarkCyan2 : GameColors.penGameOrangeDim2;

            Brush b = highlight ? GameColors.brushDarkCyan : GameColors.brushGameOrange;
            Pen p = highlight ? GameColors.penCyan2 : GameColors.penGameOrangeDim2;

            if (fill == 0 && !highlight)
            {
                b = GameColors.brushCyan;
                p = GameColors.penDarkCyan2;
            }
            else if (fill == 1)
            {
                b = GameColors.brushGameOrange;
                p = GameColors.penGameOrangeDim2;
            }

            g.FillPie(b, rPie, -90, fillAngle);
            g.DrawEllipse(p, rPie);
        }

        private void drawExpando(Graphics g, int x, int y, int h, bool plus)
        {
            const int ww = 8;

            int dx = 8;
            var dy = Util.centerIn(h, ww);
            var r = new Rectangle(x + dx, y + dy, ww, ww);
            var m = Util.centerIn(ww, 1);

            g.FillRectangle(Brushes.Black, r);

            var p = Pens.Gray;
            g.DrawLine(p, r.Left, r.Top + m, r.Right, r.Top + m);

            if (plus)
                g.DrawLine(p, r.Left + m, r.Top, r.Left + m, r.Bottom);
        }

        private void doTreeViewMode()
        {
            try
            {
                Game.log("doTreeViewMode - start");
                tree.doNotPaint = true;
                tree.Hide();

                TreeNode? makeVisible = null;
                makeVisible = tree.Nodes[2];

                foreach (TreeNode node in tree.Nodes)
                {
                    if (node.Tag == game.systemBody)
                        makeVisible = node;

                    var shouldExpand = treeMode == TreeViewMode.Everything;
                    if (currentBodyOnly && game.systemBody != null)
                        shouldExpand = node.Tag == game.systemBody;

                    if (shouldExpand && !node.IsExpanded)
                        node.Expand();
                    else if (!shouldExpand && node.IsExpanded)
                        node.Collapse();
                }

                if (makeVisible != null)
                    makeVisible.EnsureVisible();
            }
            finally
            {
                Game.log("doTreeViewMode - end");
                tree.doNotPaint = false;
                tree.Show();
            }
        }

        private void viewOnCanonnSignalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Util.openLink("https://canonn-science.github.io/canonn-signals/?system=" + Uri.EscapeDataString(game.systemData!.name));
        }

        private void viewOnSpanshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var url = $"https://spansh.co.uk/system/{game.systemData!.address}";
            Util.openLink(url);
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            treeMode = TreeViewMode.Everything;
            doTreeViewMode();
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            treeMode = TreeViewMode.BodiesOnly;
            doTreeViewMode();
        }

        private void btnCurrentBody_Click(object sender, EventArgs e)
        {
            currentBodyOnly = !currentBodyOnly;
            btnCurrentBody.Text = currentBodyOnly ? "❌ Current body only" : "  Current body only";
            doTreeViewMode();

            Game.settings.formPredictionsCurrentBodyOnly = currentBodyOnly;
            Game.settings.Save();
        }

        enum TreeViewMode
        {
            None,
            Everything,
            HidePredictedSpecies,
            BodiesOnly,
        }
    }

    class Foo : Tuple<BioSpecies, List<SystemVariantPrediction>>
    {
        public Foo(BioSpecies item1, List<SystemVariantPrediction> item2) : base(item1, item2)
        {
        }
    }
}
