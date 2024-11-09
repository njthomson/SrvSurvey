using SrvSurvey.forms;
using SrvSurvey.game;
using System.Drawing.Drawing2D;

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
            BaseForm.get<FormPredictions>()?.prepNodes();
        }

        protected Game game = Game.activeGame!;

        // TODO: Move to GameColors ...
        private readonly static Brush brushPartial = Brushes.Orange;
        private readonly static Brush brushComplete = Brushes.GreenYellow;
        private Brush brushRowOdd = new SolidBrush(Color.FromArgb(255, 24, 24, 24));
        private Brush brushRowEven = new SolidBrush(Color.FromArgb(255, 16, 16, 16));
        private Pen penRowEven = new Pen(Color.FromArgb(255, 64, 64, 64), 2f);
        private Brush treeBackBrush;
        private Font nodeBig = new Font("Lucida Console", 13f, FontStyle.Regular, GraphicsUnit.Point);
        private Font nodeMiddle = new Font("Lucida Console", 10f, FontStyle.Regular, GraphicsUnit.Point);
        private Font nodeMiddleI = new Font("Lucida Console", 10f, FontStyle.Italic, GraphicsUnit.Point);
        private Font nodeSmall = new Font("Lucida Console", 8f, FontStyle.Regular, GraphicsUnit.Point);

        private TreeNode? hoverNode;
        private float maxBodyTextWidth = 0;

        public FormPredictions()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            treeBackBrush = new SolidBrush(tree.BackColor);

            tree.MouseWheel += Tree_MouseWheel;
            tree.TreeViewNodeSorter = new NodeSorter();
            tree.ForeColor = GameColors.Orange;

            this.prepNodes();


            // Not themed - this is always dark.
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.MinimumSize = new Size(flowCounts.Right + flowCounts.Left * 2, 0);
        }

        private void prepNodes()
        {
            if (game?.systemData == null) return;

            txtSystem.Text = game.systemData.name;

            // count of signals scanned
            var systemTotal = game.systemData.bodies.Sum(_ => _.bioSignalCount);
            var systemScanned = game.systemData.bodies.Sum(_ => _.countAnalyzedBioSignals);
            txtSignalCount.Text = $"{systemScanned} of {systemTotal}";

            var bioBodyCompleteCount = game.systemData.bodies.Count(b => b.bioSignalCount > 0 && b.countAnalyzedBioSignals == b.bioSignalCount);
            var bioBodyCount = game.systemData.bodies.Count(b => b.bioSignalCount > 0);
            txtBodyCount.Text = $"{bioBodyCompleteCount} of {bioBodyCount}";

            //var sysEstimate = game.systemData.bodies.Sum(body => body.firstFootFall ? body.maxBioRewards * 5 : body.maxBioRewards);
            //var sysActual = game.systemData.bodies.Sum(body => body.sumAnalyzed);

            txtSysEst.Text = Util.getMinMaxCredits(game.systemData.getMinBioRewards(false), game.systemData.getMaxBioRewards(false));
            txtSysEstFF.Text = Util.getMinMaxCredits(game.systemData.getMinBioRewards(true), game.systemData.getMaxBioRewards(true));

            // Add a node for each body that has bio signals, and a child for each signal
            var bodyTexts = new List<string>();
            tree.Nodes.Clear();
            foreach (var body in game.systemData.bodies)
            {
                if (body.bioSignalCount == 0) continue;

                var node = createBodyNode(body);
                bodyTexts.Add(node.Text);
                tree.Nodes.Add(node);
            }

            // remove last inner blank node from last body, add one sibling to is
            if (tree.Nodes.Count > 0)
            {
                var lastBody = tree.Nodes[tree.Nodes.Count - 1];
                lastBody.Nodes.RemoveAt(lastBody.Nodes.Count - 1);
                tree.Nodes.Add(new TreeNode("ZZ"));
            }

            // get widest body name
            this.maxBodyTextWidth = Util.maxWidth(nodeBig, bodyTexts.ToArray());

            tree.ExpandAll();
        }

        private TreeNode createBodyNode(SystemBody body)
        {
            var node = new TreeNode
            {
                Name = body.shortName,
                Text = "Body: " + body.name.Replace(body.system.name + " ", ""),
                //Text = body.name.Replace(body.system.name + " ", "") + $": {body.bioSignalCount} signals",
                Tag = body,
                ToolTipText = body.name,
                NodeFont = nodeBig
            };
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
                    node.Nodes.Add(createPredictionNodes(genus));

            node.Nodes.Add(new TreeNode("Z"));
            return node;
        }

        private TreeNode createKnownNode(SystemOrganism org)
        {
            var child = new TreeNode
            {
                Text = org.variantLocalized ?? org.speciesLocalized ?? org.genusLocalized,
                Tag = org,
                NodeFont = nodeMiddle,
            };

            return child;
        }

        private TreeNode createPredictionNodes(SystemGenusPrediction genus)
        {
            // create a node for each genus, species and variant?
            var node = new TreeNode
            {
                Text = genus.genus.englishName,
                Tag = genus,
                NodeFont = nodeMiddleI,
            };

            foreach (var entry in genus.species)
            {
                var foo = new Tuple<BioSpecies, List<SystemVariantPrediction>>(entry.Key, entry.Value);
                var child = new TreeNode
                {
                    Text = entry.Key.englishName.Replace(entry.Key.genus.englishName + " ", ""),
                    Tag = foo,
                    NodeFont = nodeMiddleI,
                };
                node.Nodes.Add(child);

                //var variantNames = genus.species[species].Select(v =>
                //{
                //    return v.prefix + v.variant.colorName;
                //});

                //var variants = new TreeNode
                //{
                //    Text = " ", //string.Join(", ", variantNames),
                //    Tag = genus.species[species],
                //    NodeFont = nodeMiddleI,
                //    Checked = true,
                //};
                //child.Nodes.Add(variants);
            }

            return node;
        }

        private void checkFF_CheckedChanged(object sender, EventArgs e)
        {
            lblSysEstFF.Visible = txtSysEstFF.Visible = checkFF.Checked;
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
            if (e.Node == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var node = e.Node;
            var idx = node.VisibleIndex();

            var highlight = node == hoverNode;

            var body = node.Tag as SystemBody;
            var org = node.Tag as SystemOrganism;
            var genus = node.Tag as SystemGenusPrediction;

            // erase background
            var odd = (idx % 2) == 0;
            var backBrush = odd ? brushRowEven : brushRowOdd;
            //if (isFocused && text != "Z") backBrush = Brushes.Navy;
            if (node == hoverNode && node.Text != "Z" && node.Text != "ZZ") backBrush = Brushes.Black;
            g.FillRectangle(backBrush, e.Bounds);

            if (node.Text == "Z") return;
            if (node.Text == "ZZ")
            {
                var brushColor = ((SolidBrush)backBrush).Color;
                if (tree.BackColor != brushColor)
                    tree.BackColor = brushColor;
                return;
            }

            // calculate indented text position
            var sz = TextRenderer.MeasureText(g, "N", e.Node.NodeFont, Size.Empty, Util.textFlags);
            var pt = new Point(
                e.Node.Level * tree.Indent,
                e.Bounds.Y + Util.centerIn(e.Bounds.Height, sz.Height)
            );

            if (body != null)
                drawBodyNode(g, pt, node, highlight, body);
            else if (org != null)
                drawKnownNode(g, pt, node, highlight, org);
            else if (genus != null)
                drawGenusNode(g, pt, node, highlight, genus);
            else
                drawSpeciesNode(g, pt, node, highlight);

            //// draw +/- graphic
            //if (node.HasChildren())
            //{
            //    drawExpando(g, pt.X, e.Bounds.Y, e.Bounds.Height, !node.IsExpanded);
            //    pt.X += 8;
            //}
            //pt.X += 12;

            //// draw completion pie
            //if (body != null) //!isSpeciesOrVariants)
            //{
            //    var pieFill = 0f;
            //    if (body != null) pieFill = 1f / body.bioSignalCount * body.countAnalyzedBioSignals;
            //    else if (org != null && false)
            //    {
            //        if (org.analyzed)
            //            pieFill = 1f;
            //        else if (org.genus == game?.cmdr?.scanTwo?.genus)
            //            pieFill = 0.66f;
            //        else if (org.genus == game?.cmdr?.scanOne?.genus)
            //            pieFill = 0.33f;
            //    }

            //    drawNodePie(g, pt, pieFill, highlight);
            //    pt.X += 18;
            //}
            //// bar for known organism
            //if (org?.reward > 0)
            //{
            //    pt.X += 8;
            //    var y = e.Bounds.Top + 15;
            //    var vc = highlight ? VolColor.Blue : VolColor.Orange;
            //    if (org.isFirst) vc = VolColor.Gold;
            //    PlotBioSystem.drawVolumeBars(g, pt.X, y, vc, org.reward);
            //    pt.X += 14;
            //}
            //// bar for predicted genus
            //if (genus != null)
            //{
            //    //pt.X += 8;
            //    var y = e.Bounds.Top + 15;
            //    var vc = VolColor.Blue; // highlight ? VolColor.Blue : VolColor.Orange;
            //    if (genus.isGold) vc = VolColor.Gold;
            //    PlotBioSystem.drawVolumeBars(g, pt.X, y, vc, genus.min, genus.max, true);
            //    pt.X += 14;
            //}

            //var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;
            //if (org?.volCol == VolColor.Gold || genus?.isGold == true)
            //    foreColor = GameColors.Bio.gold;

            //// draw visible text
            //if (variants != null && false)
            //{
            //    foreach (var item in variants)
            //    {
            //        var subText = item.prefix + item.variant.colorName;
            //        var subColor = foreColor;
            //        if (item.isGold) subColor = GameColors.Bio.gold;
            //        TextRenderer.DrawText(g, subText, e.Node.NodeFont, pt, subColor, Util.textFlags);

            //        var sz2 = TextRenderer.MeasureText(g, subText, e.Node.NodeFont); //, Size.Empty, TextFormatFlags.NoPadding);
            //        pt.X += sz2.Width;
            //    }
            //}
            ////else if (text[0] == '-')
            ////{
            ////    pt.X += 4;
            ////    // render body info
            ////    TextRenderer.DrawText(g, text.Substring(1), e.Node.NodeFont, pt, foreColor, Util.textFlags);
            ////}
            //else
            //{
            //    // just render the node's text
            //    TextRenderer.DrawText(g, text, e.Node.NodeFont, pt, foreColor, Util.textFlags);

            //    if (org?.analyzed == true)
            //    {
            //        var subColor = highlight ? GameColors.DarkCyan : GameColors.OrangeDim;
            //        if (org.isFirst) subColor = GameColors.Bio.gold;
            //        TextRenderer.DrawText(g, "✔", e.Node.NodeFont, new Point(8, pt.Y), subColor, Util.textFlags);
            //    }
            //    else if (org?.analyzed == false || genus != null)
            //    {
            //        g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, 10, pt.Y + 2, 10, 10);
            //    }
            //    if (species != null && variants != null)
            //    {
            //        //g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, 10, pt.Y + 2, 10, 10);
            //        //var body2 = node.Parent.Parent.Tag as SystemBody;
            //        //body2.genusPredictions[]
            //        //g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, 10, pt.Y + 2, 10, 10);
            //        pt.X += sz.Width;
            //        pt.Y += 2;
            //        foreach (var item in variants)
            //        {
            //            var subText = item.prefix + item.variant.colorName;
            //            var subColor = foreColor;
            //            if (item.isGold) subColor = GameColors.Bio.gold;
            //            TextRenderer.DrawText(g, subText, nodeSmall, pt, subColor, Util.textFlags);

            //            var sz2 = TextRenderer.MeasureText(g, subText, e.Node.NodeFont); //, Size.Empty, TextFormatFlags.NoPadding);
            //            pt.X += sz2.Width;
            //        }
            //        pt.Y -= 2;

            //    }
            //}
            ////if (isFocused)
            ////{
            ////    var rr = new Rectangle(pt, sz);
            ////    rr.Inflate(2, 1);
            ////    var focusColor = highlight ? GameColors.DarkCyan : GameColors.OrangeDim;
            ////    //ControlPaint.DrawFocusRectangle(g, rr, Color.Red, Color.Transparent);
            ////    g.SmoothingMode = SmoothingMode.None;
            ////    g.DrawRectangle(highlight ? GameColors.penCyan1Dotted : GameColors.penGameOrange1Dotted, rr);
            ////}

            //// draw credit estimates
            //string? textRight = null;
            //if (body != null) textRight = body.getMinMaxBioRewards(false);
            //if (org != null) textRight = Util.credits(org.reward, true);
            //if (species != null) textRight = Util.credits(species.reward, true);
            //if (genus != null && !node.IsExpanded) textRight = Util.getMinMaxCredits(genus.min, genus.max);

            //if (textRight != null)
            //    g.drawTextRight(textRight, node.NodeFont, foreColor, e.Bounds.Right - 4, e.Bounds.Top + 1, 0, e.Bounds.Height);


            //if (body != null)
            //{
            //    var x = pt.X + (int)this.maxBodyTextWidth + 20;

            //    // bio bars - if a body and collapsed
            //    if (!node.IsExpanded)
            //    {
            //        var y = e.Bounds.Top;
            //        x += 10 + (int)PlotBioSystem.drawBodyBars(g, body, x, y, false);
            //    }

            //    // distance to arrival
            //    var subColor = highlight ? GameColors.DarkCyan : GameColors.OrangeDim;
            //    TextRenderer.DrawText(g, Util.lsToString(body.distanceFromArrivalLS), nodeMiddle, new Point(x, pt.Y + 3), subColor, Util.textFlags);
            //}
        }

        private bool shouldHighlight(TreeNode? node)
        {
            if (node == null) return false;

            // highlight if we match the target body but we're not on a planet
            var body = node.Tag as SystemBody;
            if (body == game.targetBody && !game.status.hasLatLong)
                return true;
            else
                return false;
        }

        private void drawSpeciesNode(Graphics g, Point pt, TreeNode node, bool highlight)
        {
            if (shouldHighlight(node?.Parent?.Parent)) highlight = true;

            var tuple = (Tuple<BioSpecies, List<SystemVariantPrediction>>)node.Tag;
            var species = tuple?.Item1;
            var variants = tuple?.Item2;
            if (species == null || variants == null)
                return;

            pt.X += 16;

            // render the node's text
            var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;
            //if (genus.isGold) foreColor = GameColors.Bio.gold;
            var text = $"{node.Text}: ";
            TextRenderer.DrawText(g, text, nodeSmall, pt, foreColor, Util.textFlags);
            var sz = TextRenderer.MeasureText(g, text, nodeSmall, Size.Empty, Util.textFlags);

            pt.X += sz.Width;
            //pt.Y += 2;
            foreach (var item in variants)
            {
                var subText = item.prefix + item.variant.colorName;
                var subColor = foreColor;
                if (item.isGold) subColor = GameColors.Bio.gold;
                TextRenderer.DrawText(g, subText, nodeSmall, pt, subColor, Util.textFlags);

                var sz2 = TextRenderer.MeasureText(g, subText, nodeSmall); //, Size.Empty, TextFormatFlags.NoPadding);
                pt.X += sz2.Width;
            }
            //pt.Y -= 2;

            // draw credit estimates
            //drawCreditEstimate(g, node, foreColor, );
            g.drawTextRight(Util.credits(species.reward, true), node.NodeFont, foreColor, tree.ClientSize.Width - 60, node.Bounds.Top + 1, 0, node.Bounds.Height);
        }

        private void drawGenusNode(Graphics g, Point pt, TreeNode node, bool highlight, SystemGenusPrediction genus)
        {
            if (shouldHighlight(node.Parent)) highlight = true;

            //g.DrawRectangle(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, 10, pt.Y + 2, 10, 10);
            g.DrawRectangle(GameColors.penDarkCyan1, 10, pt.Y + 2, 10, 10);

            // draw +/- graphic
            drawExpando(g, pt.X, node.Bounds.Y, node.Bounds.Height, !node.IsExpanded);
            pt.X += 20;

            // volume bar
            var y = node.Bounds.Top + 15;
            var vc = VolColor.Blue; // highlight ? VolColor.Blue : VolColor.Orange;
            if (genus.isGold) vc = VolColor.Gold;
            PlotBioSystem.drawVolumeBars(g, pt.X, y, vc, genus.min, genus.max, true);
            pt.X += 14;

            // render the node's text
            var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;
            if (genus.isGold) foreColor = GameColors.Bio.gold;
            TextRenderer.DrawText(g, $"{node.Text} ?", node.NodeFont, pt, foreColor, Util.textFlags);

            // draw credit estimates?
            if (!node.IsExpanded)
                drawCreditEstimate(g, node, foreColor, Util.getMinMaxCredits(genus.min, genus.max));
        }

        private void drawKnownNode(Graphics g, Point pt, TreeNode node, bool highlight, SystemOrganism org)
        {
            if (shouldHighlight(node.Parent)) highlight = true;

            // volume bar
            pt.X += 20;
            var y = node.Bounds.Top + 15;
            var vc = highlight ? VolColor.Blue : VolColor.Orange;
            if (org.isFirst) vc = VolColor.Gold;
            PlotBioSystem.drawVolumeBars(g, pt.X, y, vc, org.reward);
            pt.X += 14;

            if (game?.cmdr?.scanOne?.genus == org.genus)
            {
                highlight = true;

                // draw side-bars to highlight this is what we're currently scanning
                g.DrawLine(GameColors.penCyan4, 2, node.Bounds.Top, 2, node.Bounds.Bottom);
                var r = tree.ClientSize.Width - 3;
                g.DrawLine(GameColors.penCyan4, r, node.Bounds.Top, r, node.Bounds.Bottom);
            }

            // render the node's text
            var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;
            if (org.volCol == VolColor.Gold) foreColor = GameColors.Bio.gold;
            var text = org.prefix + node.Text;
            TextRenderer.DrawText(g, text, node.NodeFont, pt, foreColor, Util.textFlags);

            if (org.analyzed)
            {
                var subColor = highlight ? GameColors.DarkCyan : GameColors.OrangeDim;
                if (org.isFirst) subColor = GameColors.Bio.gold;
                TextRenderer.DrawText(g, "✔", node.NodeFont, new Point(8, pt.Y), subColor, Util.textFlags);
            }
            else //if (org?.analyzed == false || genus != null)
            {
                g.DrawRectangle(GameColors.penDarkCyan1, 10, pt.Y + 2, 10, 10);
            }

            // draw credit estimates
            drawCreditEstimate(g, node, foreColor, Util.credits(org.reward, true));
        }

        private void drawBodyNode(Graphics g, Point pt, TreeNode node, bool highlight, SystemBody body)
        {
            if (shouldHighlight(node)) highlight = true;

            // draw +/- graphic
            drawExpando(g, pt.X, node.Bounds.Y, node.Bounds.Height, !node.IsExpanded);
            pt.X += 22;

            // draw completion pie
            var pieFill = 1f / body.bioSignalCount * body.countAnalyzedBioSignals;

            // completion pie
            drawNodePie(g, pt, pieFill, highlight);
            pt.X += 18;

            // render the node's text
            var foreColor = highlight ? GameColors.Cyan : GameColors.Orange;
            var text = node.Text;
            if (body.firstFootFall)
                text += "🦶";
            TextRenderer.DrawText(g, text, node.NodeFont, pt, foreColor, Util.textFlags);

            pt.X += (int)this.maxBodyTextWidth + 20;
            var x = pt.X + (int)this.maxBodyTextWidth + 20;

            //TextRenderer.DrawText(g, $"{body.bioSignalCount} signals", nodeMiddle, pt, foreColor, Util.textFlags);

            // bio bars - if a body and collapsed
            if (!node.IsExpanded)
                pt.X += 10 + (int)PlotBioSystem.drawBodyBars(g, body, pt.X, node.Bounds.Top, false);

            // distance to arrival
            pt.Y += 3;
            var subColor = highlight ? GameColors.DarkCyan : GameColors.OrangeDim;
            TextRenderer.DrawText(g, Util.lsToString(body.distanceFromArrivalLS), nodeMiddle, pt, subColor, Util.textFlags);


            // draw credit estimates
            drawCreditEstimate(g, node, foreColor, body.getMinMaxBioRewards(false));
        }

        private void drawCreditEstimate(Graphics g, TreeNode node, Color color, string text)
        {
            g.drawTextRight(text, node.NodeFont, color, tree.ClientSize.Width - 8, node.Bounds.Top + 1, 0, node.Bounds.Height);
        }

        private void drawNodePie(Graphics g, Point pt, float fill, bool highlight)
        {
            // completion pie icon
            var rPie = new Rectangle(pt.X, pt.Y + 2, 13, 13);

            var fillAngle = 360f * fill;
            var b = fill == 1f ? Brushes.Lime : brushPartial;
            g.FillPie(b, rPie, -90, fillAngle);
            g.DrawEllipse(penRowEven, rPie);
        }

        private void drawExpando(Graphics g, int x, int y, int h, bool plus)
        {
            const int ww = 8;

            int dx = 8;
            var dy = Util.centerIn(h, ww);
            var r = new Rectangle(x + dx, y + dy, ww, ww);
            var m = Util.centerIn(ww, 1);

            g.FillRectangle(Brushes.Black, r);

            var p = Pens.Gray; // SystemPens.Info
            g.DrawLine(p, r.Left, r.Top + m, r.Right, r.Top + m);

            if (plus)
                g.DrawLine(p, r.Left + m, r.Top, r.Left + m, r.Bottom);
        }
    }
}
