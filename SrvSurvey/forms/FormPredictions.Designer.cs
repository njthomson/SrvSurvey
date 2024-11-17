using SrvSurvey.game;

namespace SrvSurvey
{
    partial class FormPredictions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                Game.update -= Game_update;
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TreeNode treeNode1 = new TreeNode("Node5");
            TreeNode treeNode2 = new TreeNode("Node6");
            TreeNode treeNode3 = new TreeNode("Node0", new TreeNode[] { treeNode1, treeNode2 });
            TreeNode treeNode4 = new TreeNode("Node7");
            TreeNode treeNode5 = new TreeNode("Node8");
            TreeNode treeNode6 = new TreeNode("Node9");
            TreeNode treeNode7 = new TreeNode("Node1", new TreeNode[] { treeNode4, treeNode5, treeNode6 });
            TreeNode treeNode8 = new TreeNode("Node10");
            TreeNode treeNode9 = new TreeNode("Node2", new TreeNode[] { treeNode8 });
            TreeNode treeNode10 = new TreeNode("Node11");
            TreeNode treeNode11 = new TreeNode("Node12");
            TreeNode treeNode12 = new TreeNode("Node13");
            TreeNode treeNode13 = new TreeNode("Node14");
            TreeNode treeNode14 = new TreeNode("Node15");
            TreeNode treeNode15 = new TreeNode("Node3", new TreeNode[] { treeNode10, treeNode11, treeNode12, treeNode13, treeNode14 });
            TreeNode treeNode16 = new TreeNode("Node4");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPredictions));
            lblSysEst = new Label();
            tree = new TreeView2();
            lblSysEstFF = new Label();
            txtScanCount = new TextBox2();
            lblScanCount = new Label();
            txtSysEst = new TextBox2();
            txtSysEstFF = new TextBox2();
            txtSystem = new TextBox2();
            tableTop = new TableLayoutPanel();
            lblSysActual = new Label();
            txtSysActual = new TextBox2();
            flowCounts = new FlowLayoutPanel();
            statusStrip1 = new StatusStrip();
            toolFiller = new ToolStripStatusLabel();
            toolStripDropDownButton2 = new ToolStripDropDownButton();
            viewOnCanonnSignalsToolStripMenuItem = new ToolStripMenuItem();
            viewOnSpanshToolStripMenuItem = new ToolStripMenuItem();
            panelTop = new Panel();
            flowButtons = new FlowLayoutPanel();
            btnExpandAll = new FlatButton();
            btnCollapseAll = new FlatButton();
            btnCurrentBody = new FlatButton();
            tableTop.SuspendLayout();
            statusStrip1.SuspendLayout();
            panelTop.SuspendLayout();
            flowButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblSysEst
            // 
            lblSysEst.Anchor = AnchorStyles.Right;
            lblSysEst.AutoSize = true;
            lblSysEst.FlatStyle = FlatStyle.System;
            lblSysEst.Location = new Point(15, 31);
            lblSysEst.Name = "lblSysEst";
            lblSysEst.Size = new Size(117, 12);
            lblSysEst.TabIndex = 1;
            lblSysEst.Text = "System estimate:";
            // 
            // tree
            // 
            tree.BackColor = Color.Black;
            tree.CausesValidation = false;
            tree.Dock = DockStyle.Fill;
            tree.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            tree.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            tree.ForeColor = SystemColors.Info;
            tree.FullRowSelect = true;
            tree.HideSelection = false;
            tree.Indent = 19;
            tree.ItemHeight = 22;
            tree.Location = new Point(0, 102);
            tree.Margin = new Padding(0, 5, 0, 0);
            tree.Name = "tree";
            treeNode1.Name = "Node5";
            treeNode1.Text = "Node5";
            treeNode2.Name = "Node6";
            treeNode2.Text = "Node6";
            treeNode3.Name = "Node0";
            treeNode3.Text = "Node0";
            treeNode4.Name = "Node7";
            treeNode4.Text = "Node7";
            treeNode5.Name = "Node8";
            treeNode5.Text = "Node8";
            treeNode6.Name = "Node9";
            treeNode6.Text = "Node9";
            treeNode7.Name = "Node1";
            treeNode7.Text = "Node1";
            treeNode8.Name = "Node10";
            treeNode8.Text = "Node10";
            treeNode9.Name = "Node2";
            treeNode9.Text = "Node2";
            treeNode10.Name = "Node11";
            treeNode10.Text = "Node11";
            treeNode11.Name = "Node12";
            treeNode11.Text = "Node12";
            treeNode12.Name = "Node13";
            treeNode12.Text = "Node13";
            treeNode13.Name = "Node14";
            treeNode13.Text = "Node14";
            treeNode14.Name = "Node15";
            treeNode14.Text = "Node15";
            treeNode15.Name = "Node3";
            treeNode15.Text = "Node3";
            treeNode16.Name = "Node4";
            treeNode16.Text = "Node4";
            tree.Nodes.AddRange(new TreeNode[] { treeNode3, treeNode7, treeNode9, treeNode15, treeNode16 });
            tree.ShowNodeToolTips = true;
            tree.Size = new Size(484, 283);
            tree.TabIndex = 2;
            tree.DrawNode += tree_DrawNode;
            tree.AfterSelect += tree_AfterSelect;
            tree.Layout += tree_Layout;
            tree.MouseDown += tree_MouseDown;
            tree.MouseLeave += tree_MouseLeave;
            tree.MouseMove += tree_MouseMove;
            // 
            // lblSysEstFF
            // 
            lblSysEstFF.Anchor = AnchorStyles.Right;
            lblSysEstFF.AutoSize = true;
            lblSysEstFF.FlatStyle = FlatStyle.System;
            lblSysEstFF.Location = new Point(29, 52);
            lblSysEstFF.Name = "lblSysEstFF";
            lblSysEstFF.Size = new Size(103, 12);
            lblSysEstFF.TabIndex = 4;
            lblSysEstFF.Text = "With FF bonus:";
            // 
            // txtScanCount
            // 
            txtScanCount.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtScanCount.BackColor = Color.Black;
            txtScanCount.BorderStyle = BorderStyle.FixedSingle;
            txtScanCount.ForeColor = Color.Red;
            txtScanCount.Location = new Point(391, 49);
            txtScanCount.Margin = new Padding(0, 1, 0, 1);
            txtScanCount.Name = "txtScanCount";
            txtScanCount.ReadOnly = true;
            txtScanCount.Size = new Size(91, 19);
            txtScanCount.TabIndex = 8;
            txtScanCount.Text = "99 of 99";
            txtScanCount.TextAlign = HorizontalAlignment.Center;
            // 
            // lblScanCount
            // 
            lblScanCount.Anchor = AnchorStyles.Right;
            lblScanCount.AutoSize = true;
            lblScanCount.FlatStyle = FlatStyle.System;
            lblScanCount.Location = new Point(281, 52);
            lblScanCount.Margin = new Padding(6, 0, 0, 0);
            lblScanCount.Name = "lblScanCount";
            lblScanCount.Size = new Size(110, 12);
            lblScanCount.TabIndex = 7;
            lblScanCount.Text = "Scannd signals:";
            // 
            // txtSysEst
            // 
            txtSysEst.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSysEst.BackColor = Color.Black;
            txtSysEst.BorderStyle = BorderStyle.FixedSingle;
            txtSysEst.ForeColor = Color.Red;
            txtSysEst.Location = new Point(135, 28);
            txtSysEst.Margin = new Padding(0, 1, 0, 1);
            txtSysEst.Name = "txtSysEst";
            txtSysEst.ReadOnly = true;
            txtSysEst.Size = new Size(120, 19);
            txtSysEst.TabIndex = 9;
            txtSysEst.Text = "9999.9 ~ 999.9M cr";
            txtSysEst.TextAlign = HorizontalAlignment.Center;
            // 
            // txtSysEstFF
            // 
            txtSysEstFF.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSysEstFF.BackColor = Color.Black;
            txtSysEstFF.BorderStyle = BorderStyle.FixedSingle;
            txtSysEstFF.ForeColor = Color.Red;
            txtSysEstFF.Location = new Point(135, 49);
            txtSysEstFF.Margin = new Padding(0, 1, 0, 1);
            txtSysEstFF.Name = "txtSysEstFF";
            txtSysEstFF.ReadOnly = true;
            txtSysEstFF.Size = new Size(120, 19);
            txtSysEstFF.TabIndex = 11;
            txtSysEstFF.Text = "9999.9 ~ 999.9M cr";
            txtSysEstFF.TextAlign = HorizontalAlignment.Center;
            // 
            // txtSystem
            // 
            txtSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystem.BackColor = Color.Black;
            txtSystem.BorderStyle = BorderStyle.FixedSingle;
            tableTop.SetColumnSpan(txtSystem, 4);
            txtSystem.Font = new Font("Lucida Console", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtSystem.ForeColor = Color.Red;
            txtSystem.Location = new Point(0, 0);
            txtSystem.Margin = new Padding(0, 0, 0, 1);
            txtSystem.Name = "txtSystem";
            txtSystem.Padding = new Padding(2, 0, 2, 0);
            txtSystem.ReadOnly = true;
            txtSystem.Size = new Size(482, 26);
            txtSystem.TabIndex = 12;
            txtSystem.Text = "Dryaa Proae PT-O d7-67 ABC 2 h a";
            // 
            // tableTop
            // 
            tableTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableTop.ColumnCount = 4;
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49.99999F));
            tableTop.ColumnStyles.Add(new ColumnStyle());
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0000076F));
            tableTop.ColumnStyles.Add(new ColumnStyle());
            tableTop.Controls.Add(lblSysActual, 2, 1);
            tableTop.Controls.Add(txtScanCount, 3, 2);
            tableTop.Controls.Add(txtSystem, 0, 0);
            tableTop.Controls.Add(lblScanCount, 2, 2);
            tableTop.Controls.Add(txtSysActual, 3, 1);
            tableTop.Controls.Add(lblSysEst, 0, 1);
            tableTop.Controls.Add(txtSysEst, 1, 1);
            tableTop.Controls.Add(txtSysEstFF, 1, 2);
            tableTop.Controls.Add(lblSysEstFF, 0, 2);
            tableTop.ForeColor = Color.FromArgb(255, 128, 0);
            tableTop.Location = new Point(0, 0);
            tableTop.Margin = new Padding(0);
            tableTop.Name = "tableTop";
            tableTop.RowCount = 4;
            tableTop.RowStyles.Add(new RowStyle());
            tableTop.RowStyles.Add(new RowStyle());
            tableTop.RowStyles.Add(new RowStyle());
            tableTop.RowStyles.Add(new RowStyle());
            tableTop.Size = new Size(482, 78);
            tableTop.TabIndex = 13;
            // 
            // lblSysActual
            // 
            lblSysActual.Anchor = AnchorStyles.Right;
            lblSysActual.AutoSize = true;
            lblSysActual.FlatStyle = FlatStyle.System;
            lblSysActual.Location = new Point(285, 31);
            lblSysActual.Name = "lblSysActual";
            lblSysActual.Size = new Size(103, 12);
            lblSysActual.TabIndex = 15;
            lblSysActual.Text = "System actual:";
            // 
            // txtSysActual
            // 
            txtSysActual.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSysActual.BackColor = Color.Black;
            txtSysActual.BorderStyle = BorderStyle.FixedSingle;
            txtSysActual.ForeColor = Color.Red;
            txtSysActual.Location = new Point(391, 28);
            txtSysActual.Margin = new Padding(0, 1, 0, 1);
            txtSysActual.Name = "txtSysActual";
            txtSysActual.ReadOnly = true;
            txtSysActual.Size = new Size(91, 19);
            txtSysActual.TabIndex = 13;
            txtSysActual.Text = "999.9M cr";
            txtSysActual.TextAlign = HorizontalAlignment.Center;
            // 
            // flowCounts
            // 
            flowCounts.Anchor = AnchorStyles.Top;
            flowCounts.AutoSize = true;
            flowCounts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowCounts.Location = new Point(2, 83);
            flowCounts.Margin = new Padding(0, 5, 0, 0);
            flowCounts.Name = "flowCounts";
            flowCounts.Size = new Size(0, 0);
            flowCounts.TabIndex = 14;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = SystemColors.ControlDark;
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolFiller, toolStripDropDownButton2 });
            statusStrip1.Location = new Point(0, 385);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(484, 22);
            statusStrip1.TabIndex = 15;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(10, 17);
            toolFiller.Text = " ";
            // 
            // toolStripDropDownButton2
            // 
            toolStripDropDownButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] { viewOnCanonnSignalsToolStripMenuItem, viewOnSpanshToolStripMenuItem });
            toolStripDropDownButton2.Image = (Image)resources.GetObject("toolStripDropDownButton2.Image");
            toolStripDropDownButton2.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            toolStripDropDownButton2.Size = new Size(60, 20);
            toolStripDropDownButton2.Text = "More ...";
            // 
            // viewOnCanonnSignalsToolStripMenuItem
            // 
            viewOnCanonnSignalsToolStripMenuItem.Image = Properties.ImageResources.canonn_16x16;
            viewOnCanonnSignalsToolStripMenuItem.Name = "viewOnCanonnSignalsToolStripMenuItem";
            viewOnCanonnSignalsToolStripMenuItem.Size = new Size(201, 22);
            viewOnCanonnSignalsToolStripMenuItem.Text = "View on Canonn Signals";
            viewOnCanonnSignalsToolStripMenuItem.Click += viewOnCanonnSignalsToolStripMenuItem_Click;
            // 
            // viewOnSpanshToolStripMenuItem
            // 
            viewOnSpanshToolStripMenuItem.Image = Properties.ImageResources.spansh_16x16;
            viewOnSpanshToolStripMenuItem.Name = "viewOnSpanshToolStripMenuItem";
            viewOnSpanshToolStripMenuItem.Size = new Size(201, 22);
            viewOnSpanshToolStripMenuItem.Text = "View on Spansh";
            viewOnSpanshToolStripMenuItem.Click += viewOnSpanshToolStripMenuItem_Click;
            // 
            // panelTop
            // 
            panelTop.BorderStyle = BorderStyle.FixedSingle;
            panelTop.Controls.Add(tableTop);
            panelTop.Controls.Add(flowCounts);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Margin = new Padding(3, 2, 3, 2);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(0, 5, 0, 0);
            panelTop.Size = new Size(484, 71);
            panelTop.TabIndex = 17;
            // 
            // flowButtons
            // 
            flowButtons.AutoSize = true;
            flowButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowButtons.BorderStyle = BorderStyle.FixedSingle;
            flowButtons.Controls.Add(btnExpandAll);
            flowButtons.Controls.Add(btnCollapseAll);
            flowButtons.Controls.Add(btnCurrentBody);
            flowButtons.Dock = DockStyle.Top;
            flowButtons.Location = new Point(0, 71);
            flowButtons.Name = "flowButtons";
            flowButtons.Size = new Size(484, 31);
            flowButtons.TabIndex = 18;
            // 
            // btnExpandAll
            // 
            btnExpandAll.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnExpandAll.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnExpandAll.Location = new Point(3, 3);
            btnExpandAll.Name = "btnExpandAll";
            btnExpandAll.Size = new Size(111, 23);
            btnExpandAll.TabIndex = 0;
            btnExpandAll.Text = "Expand all";
            btnExpandAll.UseVisualStyleBackColor = true;
            btnExpandAll.Click += btnExpandAll_Click;
            // 
            // btnCollapseAll
            // 
            btnCollapseAll.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCollapseAll.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCollapseAll.Location = new Point(120, 3);
            btnCollapseAll.Name = "btnCollapseAll";
            btnCollapseAll.Size = new Size(111, 23);
            btnCollapseAll.TabIndex = 1;
            btnCollapseAll.Text = "Collapse all";
            btnCollapseAll.UseVisualStyleBackColor = true;
            btnCollapseAll.Click += btnCollapseAll_Click;
            // 
            // btnCurrentBody
            // 
            btnCurrentBody.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCurrentBody.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCurrentBody.Location = new Point(237, 3);
            btnCurrentBody.Name = "btnCurrentBody";
            btnCurrentBody.Size = new Size(164, 23);
            btnCurrentBody.TabIndex = 2;
            btnCurrentBody.Text = "Expand current body";
            btnCurrentBody.UseVisualStyleBackColor = true;
            btnCurrentBody.Click += btnCurrentBody_Click;
            // 
            // FormPredictions
            // 
            AutoScaleDimensions = new SizeF(7F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(484, 407);
            Controls.Add(tree);
            Controls.Add(flowButtons);
            Controls.Add(panelTop);
            Controls.Add(statusStrip1);
            Font = new Font("Lucida Console", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.FromArgb(255, 128, 0);
            Margin = new Padding(3, 2, 3, 2);
            Name = "FormPredictions";
            Text = "Bio Predictions";
            tableTop.ResumeLayout(false);
            tableTop.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            flowButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblSysEst;
        private Label lblSysEstFF;
        private TextBox2 txtScanCount;
        private Label lblScanCount;
        private TextBox2 txtSysEst;
        private TextBox2 txtSysEstFF;
        private TextBox2 txtSystem;
        private TableLayoutPanel tableTop;
        private FlowLayoutPanel flowCounts;
        private StatusStrip statusStrip1;
        private TreeView2 tree;
        private Panel panelTop;
        private Label lblSysActual;
        private TextBox2 txtSysActual;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripStatusLabel toolFiller;
        private ToolStripMenuItem viewOnCanonnSignalsToolStripMenuItem;
        private ToolStripMenuItem viewOnSpanshToolStripMenuItem;
        private FlowLayoutPanel flowButtons;
        private FlatButton btnExpandAll;
        private FlatButton btnCollapseAll;
        private FlatButton btnCurrentBody;
    }
}