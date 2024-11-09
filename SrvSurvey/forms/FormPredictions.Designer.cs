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
            lblSysEst = new Label();
            tree = new TreeView2();
            checkFF = new CheckBox();
            lblSysEstFF = new Label();
            label1 = new Label();
            txtBodyCount = new TextBox();
            txtSignalCount = new TextBox();
            label2 = new Label();
            txtSysEst = new TextBox();
            txtSysEstFF = new TextBox();
            txtSystem = new TextBox();
            tableTop = new TableLayoutPanel();
            flowCounts = new FlowLayoutPanel();
            statusStrip1 = new StatusStrip();
            tableTop.SuspendLayout();
            flowCounts.SuspendLayout();
            SuspendLayout();
            // 
            // lblSysEst
            // 
            lblSysEst.Anchor = AnchorStyles.Right;
            lblSysEst.AutoSize = true;
            lblSysEst.FlatStyle = FlatStyle.System;
            lblSysEst.Location = new Point(253, 4);
            lblSysEst.Name = "lblSysEst";
            lblSysEst.Size = new Size(96, 15);
            lblSysEst.TabIndex = 1;
            lblSysEst.Text = "System estimate:";
            // 
            // tree
            // 
            tree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tree.BackColor = Color.Black;
            tree.CausesValidation = false;
            tree.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            tree.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            tree.ForeColor = SystemColors.Info;
            tree.FullRowSelect = true;
            tree.HideSelection = false;
            tree.Indent = 19;
            tree.ItemHeight = 22;
            tree.Location = new Point(0, 82);
            tree.Margin = new Padding(0, 6, 0, 0);
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
            tree.Size = new Size(501, 549);
            tree.TabIndex = 2;
            tree.DrawNode += tree_DrawNode;
            tree.AfterSelect += tree_AfterSelect;
            tree.Layout += tree_Layout;
            tree.MouseDown += tree_MouseDown;
            tree.MouseLeave += tree_MouseLeave;
            tree.MouseMove += tree_MouseMove;
            // 
            // checkFF
            // 
            checkFF.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            checkFF.FlatStyle = FlatStyle.System;
            checkFF.Location = new Point(0, 23);
            checkFF.Margin = new Padding(0, 0, 3, 0);
            checkFF.Name = "checkFF";
            checkFF.Size = new Size(247, 23);
            checkFF.TabIndex = 3;
            checkFF.Text = "Apply First Footfall bonus";
            checkFF.UseVisualStyleBackColor = true;
            checkFF.CheckedChanged += checkFF_CheckedChanged;
            // 
            // lblSysEstFF
            // 
            lblSysEstFF.Anchor = AnchorStyles.Right;
            lblSysEstFF.AutoSize = true;
            lblSysEstFF.FlatStyle = FlatStyle.System;
            lblSysEstFF.Location = new Point(263, 27);
            lblSysEstFF.Name = "lblSysEstFF";
            lblSysEstFF.Size = new Size(86, 15);
            lblSysEstFF.TabIndex = 4;
            lblSysEstFF.Text = "With FF bonus:";
            lblSysEstFF.Visible = false;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.FlatStyle = FlatStyle.System;
            label1.Location = new Point(0, 4);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(178, 15);
            label1.TabIndex = 5;
            label1.Text = "Scanned bodies with bio signals:";
            // 
            // txtBodyCount
            // 
            txtBodyCount.BackColor = SystemColors.ScrollBar;
            txtBodyCount.BorderStyle = BorderStyle.FixedSingle;
            txtBodyCount.Location = new Point(178, 0);
            txtBodyCount.Margin = new Padding(0);
            txtBodyCount.Name = "txtBodyCount";
            txtBodyCount.ReadOnly = true;
            txtBodyCount.Size = new Size(72, 23);
            txtBodyCount.TabIndex = 6;
            txtBodyCount.Text = "99 of 99";
            txtBodyCount.TextAlign = HorizontalAlignment.Center;
            // 
            // txtSignalCount
            // 
            txtSignalCount.BackColor = SystemColors.ScrollBar;
            txtSignalCount.BorderStyle = BorderStyle.FixedSingle;
            txtSignalCount.Location = new Point(405, 0);
            txtSignalCount.Margin = new Padding(0);
            txtSignalCount.Name = "txtSignalCount";
            txtSignalCount.ReadOnly = true;
            txtSignalCount.Size = new Size(72, 23);
            txtSignalCount.TabIndex = 8;
            txtSignalCount.Text = "99 of 99";
            txtSignalCount.TextAlign = HorizontalAlignment.Center;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.FlatStyle = FlatStyle.System;
            label2.Location = new Point(256, 4);
            label2.Margin = new Padding(6, 0, 0, 0);
            label2.Name = "label2";
            label2.Size = new Size(149, 15);
            label2.TabIndex = 7;
            label2.Text = "Scanned / total bio signals:";
            // 
            // txtSysEst
            // 
            txtSysEst.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSysEst.BackColor = SystemColors.ScrollBar;
            txtSysEst.BorderStyle = BorderStyle.FixedSingle;
            txtSysEst.Location = new Point(352, 0);
            txtSysEst.Margin = new Padding(0);
            txtSysEst.Name = "txtSysEst";
            txtSysEst.ReadOnly = true;
            txtSysEst.Size = new Size(125, 23);
            txtSysEst.TabIndex = 9;
            txtSysEst.Text = "9999.9 ~ 999.9M cr";
            txtSysEst.TextAlign = HorizontalAlignment.Center;
            // 
            // txtSysEstFF
            // 
            txtSysEstFF.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSysEstFF.BackColor = SystemColors.ScrollBar;
            txtSysEstFF.BorderStyle = BorderStyle.FixedSingle;
            txtSysEstFF.Location = new Point(352, 23);
            txtSysEstFF.Margin = new Padding(0);
            txtSysEstFF.Name = "txtSysEstFF";
            txtSysEstFF.ReadOnly = true;
            txtSysEstFF.Size = new Size(125, 23);
            txtSysEstFF.TabIndex = 11;
            txtSysEstFF.Text = "999.9M cr";
            txtSysEstFF.TextAlign = HorizontalAlignment.Center;
            txtSysEstFF.Visible = false;
            // 
            // txtSystem
            // 
            txtSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystem.BackColor = SystemColors.ScrollBar;
            txtSystem.BorderStyle = BorderStyle.FixedSingle;
            txtSystem.Location = new Point(0, 0);
            txtSystem.Margin = new Padding(0, 0, 3, 0);
            txtSystem.Name = "txtSystem";
            txtSystem.ReadOnly = true;
            txtSystem.Size = new Size(247, 23);
            txtSystem.TabIndex = 12;
            txtSystem.Text = "Dryaa Proae PT-O d7-67 ABC 2 h a";
            // 
            // tableTop
            // 
            tableTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableTop.AutoSize = true;
            tableTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableTop.ColumnCount = 3;
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableTop.ColumnStyles.Add(new ColumnStyle());
            tableTop.ColumnStyles.Add(new ColumnStyle());
            tableTop.Controls.Add(txtSystem, 0, 0);
            tableTop.Controls.Add(txtSysEstFF, 2, 1);
            tableTop.Controls.Add(lblSysEst, 1, 0);
            tableTop.Controls.Add(txtSysEst, 2, 0);
            tableTop.Controls.Add(lblSysEstFF, 1, 1);
            tableTop.Controls.Add(checkFF, 0, 1);
            tableTop.Location = new Point(12, 7);
            tableTop.Margin = new Padding(0);
            tableTop.Name = "tableTop";
            tableTop.RowCount = 2;
            tableTop.RowStyles.Add(new RowStyle());
            tableTop.RowStyles.Add(new RowStyle());
            tableTop.Size = new Size(477, 46);
            tableTop.TabIndex = 13;
            // 
            // flowCounts
            // 
            flowCounts.AutoSize = true;
            flowCounts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowCounts.Controls.Add(label1);
            flowCounts.Controls.Add(txtBodyCount);
            flowCounts.Controls.Add(label2);
            flowCounts.Controls.Add(txtSignalCount);
            flowCounts.Location = new Point(12, 53);
            flowCounts.Margin = new Padding(0);
            flowCounts.Name = "flowCounts";
            flowCounts.Size = new Size(477, 23);
            flowCounts.TabIndex = 14;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = SystemColors.ControlDark;
            statusStrip1.Location = new Point(0, 629);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(501, 22);
            statusStrip1.TabIndex = 15;
            statusStrip1.Text = "statusStrip1";
            // 
            // FormPredictions
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
            ClientSize = new Size(501, 651);
            Controls.Add(flowCounts);
            Controls.Add(tableTop);
            Controls.Add(tree);
            Controls.Add(statusStrip1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "FormPredictions";
            Text = "Bio Predictions";
            tableTop.ResumeLayout(false);
            tableTop.PerformLayout();
            flowCounts.ResumeLayout(false);
            flowCounts.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblSysEst;
        private CheckBox checkFF;
        private Label lblSysEstFF;
        private Label label1;
        private TextBox txtBodyCount;
        private TextBox txtSignalCount;
        private Label label2;
        private TextBox txtSysEst;
        private TextBox txtSysEstFF;
        private TextBox txtSystem;
        private TableLayoutPanel tableTop;
        private FlowLayoutPanel flowCounts;
        private StatusStrip statusStrip1;
        private TreeView2 tree;
    }
}