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
            toolMore = new ToolStripDropDownButton();
            viewSystemOnToolStripMenuItem = new ToolStripMenuItem();
            viewOnCanonnSignalsToolStripMenuItem = new ToolStripMenuItem();
            viewOnSpanshToolStripMenuItem = new ToolStripMenuItem();
            viewOnEDSMToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuLargeFont = new ToolStripMenuItem();
            menuFontLarge = new ToolStripMenuItem();
            menuFontMedium = new ToolStripMenuItem();
            menuFontSmall = new ToolStripMenuItem();
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
            lblSysEst.Location = new Point(7, 31);
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
            tree.Font = new Font("Lucida Console", 9.75F);
            tree.ForeColor = SystemColors.Info;
            tree.FullRowSelect = true;
            tree.HideSelection = false;
            tree.Indent = 19;
            tree.ItemHeight = 22;
            tree.Location = new Point(0, 102);
            tree.Margin = new Padding(0, 5, 0, 0);
            tree.Name = "tree";
            tree.ShowNodeToolTips = true;
            tree.Size = new Size(484, 283);
            tree.TabIndex = 2;
            tree.TabStop = false;
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
            lblSysEstFF.Location = new Point(21, 52);
            lblSysEstFF.Name = "lblSysEstFF";
            lblSysEstFF.Size = new Size(103, 12);
            lblSysEstFF.TabIndex = 3;
            lblSysEstFF.Text = "With FF bonus:";
            // 
            // txtScanCount
            // 
            txtScanCount.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtScanCount.BackColor = Color.Black;
            txtScanCount.BorderStyle = BorderStyle.FixedSingle;
            txtScanCount.ForeColor = Color.Red;
            txtScanCount.Location = new Point(375, 49);
            txtScanCount.Margin = new Padding(0, 1, 0, 1);
            txtScanCount.Name = "txtScanCount";
            txtScanCount.ReadOnly = true;
            txtScanCount.Size = new Size(107, 19);
            txtScanCount.TabIndex = 8;
            txtScanCount.Text = "99 of 99";
            txtScanCount.TextAlign = HorizontalAlignment.Center;
            txtScanCount.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // lblScanCount
            // 
            lblScanCount.Anchor = AnchorStyles.Right;
            lblScanCount.AutoSize = true;
            lblScanCount.FlatStyle = FlatStyle.System;
            lblScanCount.Location = new Point(265, 52);
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
            txtSysEst.Location = new Point(127, 28);
            txtSysEst.Margin = new Padding(0, 1, 0, 1);
            txtSysEst.Name = "txtSysEst";
            txtSysEst.ReadOnly = true;
            txtSysEst.Size = new Size(120, 19);
            txtSysEst.TabIndex = 2;
            txtSysEst.Text = "9999.9 ~ 999.9M cr";
            txtSysEst.TextAlign = HorizontalAlignment.Center;
            txtSysEst.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // txtSysEstFF
            // 
            txtSysEstFF.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSysEstFF.BackColor = Color.Black;
            txtSysEstFF.BorderStyle = BorderStyle.FixedSingle;
            txtSysEstFF.ForeColor = Color.Red;
            txtSysEstFF.Location = new Point(127, 49);
            txtSysEstFF.Margin = new Padding(0, 1, 0, 1);
            txtSysEstFF.Name = "txtSysEstFF";
            txtSysEstFF.ReadOnly = true;
            txtSysEstFF.Size = new Size(120, 19);
            txtSysEstFF.TabIndex = 4;
            txtSysEstFF.Text = "9999.9 ~ 999.9M cr";
            txtSysEstFF.TextAlign = HorizontalAlignment.Center;
            txtSysEstFF.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // txtSystem
            // 
            txtSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystem.BackColor = Color.Black;
            txtSystem.BorderStyle = BorderStyle.FixedSingle;
            tableTop.SetColumnSpan(txtSystem, 4);
            txtSystem.Font = new Font("Lucida Console", 12F);
            txtSystem.ForeColor = Color.Red;
            txtSystem.Location = new Point(0, 0);
            txtSystem.Margin = new Padding(0, 0, 0, 1);
            txtSystem.Name = "txtSystem";
            txtSystem.Padding = new Padding(2, 0, 2, 0);
            txtSystem.ReadOnly = true;
            txtSystem.Size = new Size(482, 26);
            txtSystem.TabIndex = 0;
            txtSystem.Text = "Dryaa Proae PT-O d7-67 ABC 2 h a";
            txtSystem.UseEdgeButton = TextBox2.EdgeButton.None;
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
            tableTop.TabIndex = 1;
            // 
            // lblSysActual
            // 
            lblSysActual.Anchor = AnchorStyles.Right;
            lblSysActual.AutoSize = true;
            lblSysActual.FlatStyle = FlatStyle.System;
            lblSysActual.Location = new Point(269, 31);
            lblSysActual.Name = "lblSysActual";
            lblSysActual.Size = new Size(103, 12);
            lblSysActual.TabIndex = 5;
            lblSysActual.Text = "System actual:";
            // 
            // txtSysActual
            // 
            txtSysActual.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSysActual.BackColor = Color.Black;
            txtSysActual.BorderStyle = BorderStyle.FixedSingle;
            txtSysActual.ForeColor = Color.Red;
            txtSysActual.Location = new Point(375, 28);
            txtSysActual.Margin = new Padding(0, 1, 0, 1);
            txtSysActual.Name = "txtSysActual";
            txtSysActual.ReadOnly = true;
            txtSysActual.Size = new Size(107, 19);
            txtSysActual.TabIndex = 6;
            txtSysActual.Text = "999.9M cr";
            txtSysActual.TextAlign = HorizontalAlignment.Center;
            txtSysActual.UseEdgeButton = TextBox2.EdgeButton.None;
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
            flowCounts.TabIndex = 0;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = SystemColors.ControlDark;
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolFiller, toolMore });
            statusStrip1.Location = new Point(0, 385);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(484, 22);
            statusStrip1.TabIndex = 3;
            statusStrip1.TabStop = true;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.DoubleClick += statusStrip1_DoubleClick;
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(10, 17);
            toolFiller.Text = " ";
            // 
            // toolMore
            // 
            toolMore.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolMore.DropDownItems.AddRange(new ToolStripItem[] { viewSystemOnToolStripMenuItem, viewOnCanonnSignalsToolStripMenuItem, viewOnSpanshToolStripMenuItem, viewOnEDSMToolStripMenuItem, toolStripSeparator1, menuLargeFont });
            toolMore.Image = (Image)resources.GetObject("toolMore.Image");
            toolMore.ImageTransparentColor = Color.Magenta;
            toolMore.Name = "toolMore";
            toolMore.Size = new Size(60, 20);
            toolMore.Text = "&More ...";
            // 
            // viewSystemOnToolStripMenuItem
            // 
            viewSystemOnToolStripMenuItem.Enabled = false;
            viewSystemOnToolStripMenuItem.Name = "viewSystemOnToolStripMenuItem";
            viewSystemOnToolStripMenuItem.Size = new Size(175, 22);
            viewSystemOnToolStripMenuItem.Text = "Open system ...";
            // 
            // viewOnCanonnSignalsToolStripMenuItem
            // 
            viewOnCanonnSignalsToolStripMenuItem.Image = Properties.ImageResources.canonn_16x16;
            viewOnCanonnSignalsToolStripMenuItem.Name = "viewOnCanonnSignalsToolStripMenuItem";
            viewOnCanonnSignalsToolStripMenuItem.Size = new Size(175, 22);
            viewOnCanonnSignalsToolStripMenuItem.Text = "On Canonn Signals";
            viewOnCanonnSignalsToolStripMenuItem.Click += viewOnCanonnSignalsToolStripMenuItem_Click;
            // 
            // viewOnSpanshToolStripMenuItem
            // 
            viewOnSpanshToolStripMenuItem.Image = Properties.ImageResources.spansh_16x16;
            viewOnSpanshToolStripMenuItem.Name = "viewOnSpanshToolStripMenuItem";
            viewOnSpanshToolStripMenuItem.Size = new Size(175, 22);
            viewOnSpanshToolStripMenuItem.Text = "On Spansh";
            viewOnSpanshToolStripMenuItem.Click += viewOnSpanshToolStripMenuItem_Click;
            // 
            // viewOnEDSMToolStripMenuItem
            // 
            viewOnEDSMToolStripMenuItem.Name = "viewOnEDSMToolStripMenuItem";
            viewOnEDSMToolStripMenuItem.Size = new Size(175, 22);
            viewOnEDSMToolStripMenuItem.Text = "On EDSM";
            viewOnEDSMToolStripMenuItem.Click += viewOnEDSMToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(172, 6);
            // 
            // menuLargeFont
            // 
            menuLargeFont.DropDownItems.AddRange(new ToolStripItem[] { menuFontLarge, menuFontMedium, menuFontSmall });
            menuLargeFont.Name = "menuLargeFont";
            menuLargeFont.Size = new Size(175, 22);
            menuLargeFont.Text = "Font size ...";
            // 
            // menuFontLarge
            // 
            menuFontLarge.Name = "menuFontLarge";
            menuFontLarge.Size = new Size(119, 22);
            menuFontLarge.Tag = "3";
            menuFontLarge.Text = "Large";
            menuFontLarge.Click += menuSetFontSize_Click;
            // 
            // menuFontMedium
            // 
            menuFontMedium.Name = "menuFontMedium";
            menuFontMedium.Size = new Size(119, 22);
            menuFontMedium.Tag = "2";
            menuFontMedium.Text = "Medium";
            menuFontMedium.Click += menuSetFontSize_Click;
            // 
            // menuFontSmall
            // 
            menuFontSmall.Name = "menuFontSmall";
            menuFontSmall.Size = new Size(119, 22);
            menuFontSmall.Tag = "1";
            menuFontSmall.Text = "Small";
            menuFontSmall.Click += menuSetFontSize_Click;
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
            panelTop.TabIndex = 0;
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
            flowButtons.TabIndex = 1;
            // 
            // btnExpandAll
            // 
            btnExpandAll.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnExpandAll.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnExpandAll.Location = new Point(3, 3);
            btnExpandAll.Name = "btnExpandAll";
            btnExpandAll.Size = new Size(111, 23);
            btnExpandAll.TabIndex = 0;
            btnExpandAll.Text = "&Expand all";
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
            btnCollapseAll.Text = "&Collapse all";
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
            btnCurrentBody.Text = "  Current &body only";
            btnCurrentBody.UseVisualStyleBackColor = true;
            btnCurrentBody.Click += btnCurrentBody_Click;
            // 
            // FormPredictions
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Black;
            ClientSize = new Size(484, 407);
            Controls.Add(tree);
            Controls.Add(flowButtons);
            Controls.Add(panelTop);
            Controls.Add(statusStrip1);
            Font = new Font("Lucida Console", 9F);
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
        private ToolStripDropDownButton toolMore;
        private ToolStripStatusLabel toolFiller;
        private ToolStripMenuItem viewOnCanonnSignalsToolStripMenuItem;
        private ToolStripMenuItem viewOnSpanshToolStripMenuItem;
        private FlowLayoutPanel flowButtons;
        private FlatButton btnExpandAll;
        private FlatButton btnCollapseAll;
        private FlatButton btnCurrentBody;
        private ToolStripMenuItem viewSystemOnToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuLargeFont;
        private ToolStripMenuItem menuFontLarge;
        private ToolStripMenuItem menuFontMedium;
        private ToolStripMenuItem menuFontSmall;
        private ToolStripMenuItem viewOnEDSMToolStripMenuItem;
    }
}