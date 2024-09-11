namespace SrvSurvey
{
    partial class FormCodexBingo
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
            components = new System.ComponentModel.Container();
            TreeNode treeNode1 = new TreeNode("Node3");
            TreeNode treeNode2 = new TreeNode("Node4");
            TreeNode treeNode3 = new TreeNode("Node0", new TreeNode[] { treeNode1, treeNode2 });
            TreeNode treeNode4 = new TreeNode("Node5");
            TreeNode treeNode5 = new TreeNode("Node1", new TreeNode[] { treeNode4 });
            TreeNode treeNode6 = new TreeNode("Node6");
            TreeNode treeNode7 = new TreeNode("Node7");
            TreeNode treeNode8 = new TreeNode("Node8");
            TreeNode treeNode9 = new TreeNode("Node2", new TreeNode[] { treeNode6, treeNode7, treeNode8 });
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCodexBingo));
            statusStrip1 = new StatusStrip();
            toolBodyName = new ToolStripStatusLabel();
            toolRegionName = new ToolStripStatusLabel();
            toolDiscoveryDate = new ToolStripStatusLabel();
            toolFiller = new ToolStripStatusLabel();
            tree = new TreeView();
            images = new ImageList(components);
            panel1 = new Panel();
            label2 = new Label();
            comboRegion = new ComboBox();
            txtCommander = new TextBox();
            label1 = new Label();
            contextMenu = new ContextMenuStrip(components);
            menuCopyName = new ToolStripMenuItem();
            menuCopyEntryId = new ToolStripMenuItem();
            menuCanonnSeperator = new ToolStripSeparator();
            menuCanonnBioforge = new ToolStripMenuItem();
            menuCanonnResearch = new ToolStripMenuItem();
            menuEDAstro = new ToolStripMenuItem();
            statusStrip1.SuspendLayout();
            panel1.SuspendLayout();
            contextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolBodyName, toolRegionName, toolDiscoveryDate, toolFiller });
            statusStrip1.Location = new Point(0, 426);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 24);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolBodyName
            // 
            toolBodyName.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolBodyName.IsLink = true;
            toolBodyName.Name = "toolBodyName";
            toolBodyName.Size = new Size(20, 19);
            toolBodyName.Text = "...";
            toolBodyName.Click += toolBodyName_Click;
            // 
            // toolRegionName
            // 
            toolRegionName.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolRegionName.Name = "toolRegionName";
            toolRegionName.Size = new Size(20, 19);
            toolRegionName.Text = "...";
            // 
            // toolDiscoveryDate
            // 
            toolDiscoveryDate.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolDiscoveryDate.Name = "toolDiscoveryDate";
            toolDiscoveryDate.Size = new Size(20, 19);
            toolDiscoveryDate.Text = "...";
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(10, 19);
            toolFiller.Text = " ";
            // 
            // tree
            // 
            tree.BackColor = SystemColors.WindowFrame;
            tree.Dock = DockStyle.Fill;
            tree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tree.ForeColor = SystemColors.Info;
            tree.ImageIndex = 0;
            tree.ImageList = images;
            tree.Location = new Point(0, 31);
            tree.Name = "tree";
            treeNode1.Name = "Node3";
            treeNode1.Text = "Node3";
            treeNode2.Name = "Node4";
            treeNode2.Text = "Node4";
            treeNode3.Name = "Node0";
            treeNode3.Text = "Node0";
            treeNode4.Name = "Node5";
            treeNode4.Text = "Node5";
            treeNode5.Name = "Node1";
            treeNode5.Text = "Node1";
            treeNode6.Name = "Node6";
            treeNode6.Text = "Node6";
            treeNode7.Name = "Node7";
            treeNode7.Text = "Node7";
            treeNode8.Name = "Node8";
            treeNode8.Text = "Node8";
            treeNode9.Name = "Node2";
            treeNode9.Text = "Node2";
            tree.Nodes.AddRange(new TreeNode[] { treeNode3, treeNode5, treeNode9 });
            tree.SelectedImageIndex = 0;
            tree.Size = new Size(800, 395);
            tree.TabIndex = 1;
            tree.DrawNode += tree_DrawNode;
            tree.AfterSelect += tree_AfterSelect;
            tree.Layout += tree_Layout;
            tree.MouseDown += tree_MouseDown;
            // 
            // images
            // 
            images.ColorDepth = ColorDepth.Depth8Bit;
            images.ImageSize = new Size(16, 16);
            images.TransparentColor = Color.Transparent;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlDark;
            panel1.Controls.Add(label2);
            panel1.Controls.Add(comboRegion);
            panel1.Controls.Add(txtCommander);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 31);
            panel1.TabIndex = 2;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(516, 7);
            label2.Name = "label2";
            label2.Size = new Size(47, 15);
            label2.TabIndex = 3;
            label2.Text = "Region:";
            // 
            // comboRegion
            // 
            comboRegion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboRegion.DropDownStyle = ComboBoxStyle.DropDownList;
            comboRegion.FlatStyle = FlatStyle.System;
            comboRegion.FormattingEnabled = true;
            comboRegion.Location = new Point(569, 4);
            comboRegion.Name = "comboRegion";
            comboRegion.Size = new Size(219, 23);
            comboRegion.TabIndex = 2;
            comboRegion.SelectedIndexChanged += comboRegion_SelectedIndexChanged;
            // 
            // txtCommander
            // 
            txtCommander.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommander.Location = new Point(86, 3);
            txtCommander.Name = "txtCommander";
            txtCommander.ReadOnly = true;
            txtCommander.Size = new Size(424, 23);
            txtCommander.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 6);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 0;
            label1.Text = "Commander:";
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] { menuCopyName, menuCopyEntryId, menuCanonnSeperator, menuCanonnBioforge, menuCanonnResearch, menuEDAstro });
            contextMenu.Name = "contextMenuStrip";
            contextMenu.Size = new Size(212, 120);
            // 
            // menuCopyName
            // 
            menuCopyName.Name = "menuCopyName";
            menuCopyName.Size = new Size(211, 22);
            menuCopyName.Text = "Copy name";
            menuCopyName.Click += menuCopyName_Click;
            // 
            // menuCopyEntryId
            // 
            menuCopyEntryId.Name = "menuCopyEntryId";
            menuCopyEntryId.Size = new Size(211, 22);
            menuCopyEntryId.Text = "Copy entry Id";
            menuCopyEntryId.Click += menuCopyEntryId_Click;
            // 
            // menuCanonnSeperator
            // 
            menuCanonnSeperator.Name = "menuCanonnSeperator";
            menuCanonnSeperator.Size = new Size(208, 6);
            // 
            // menuCanonnBioforge
            // 
            menuCanonnBioforge.Name = "menuCanonnBioforge";
            menuCanonnBioforge.Size = new Size(211, 22);
            menuCanonnBioforge.Text = "View on Canonn Bioforge";
            menuCanonnBioforge.Click += menuViewOnCanonnBioforge_Click;
            // 
            // menuCanonnResearch
            // 
            menuCanonnResearch.Name = "menuCanonnResearch";
            menuCanonnResearch.Size = new Size(211, 22);
            menuCanonnResearch.Text = "View on Canonn Research";
            menuCanonnResearch.Click += menuViewOnCanonnResearch_Click;
            // 
            // menuEDAstro
            // 
            menuEDAstro.Name = "menuEDAstro";
            menuEDAstro.Size = new Size(211, 22);
            menuEDAstro.Text = "View on ED Astro";
            menuEDAstro.Click += menuEDAstro_Click;
            // 
            // FormCodexBingo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tree);
            Controls.Add(panel1);
            Controls.Add(statusStrip1);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(640, 480);
            Name = "FormCodexBingo";
            Text = "Codex Bingo";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            contextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private TreeView tree;
        private ImageList images;
        private Button button1;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuCopyName;
        private ToolStripMenuItem menuCopyEntryId;
        private ToolStripSeparator menuCanonnSeparator;
        private Panel panel1;
        private TextBox txtCommander;
        private Label label1;
        private ToolStripMenuItem copyNameToolStripMenuItem;
        private ToolStripMenuItem copyEntryIdToolStripMenuItem;
        private ToolStripMenuItem viewOnCanonn;
        private ToolStripMenuItem menuCanonnBioforge;
        private ToolStripMenuItem menuCanonnResearch;
        private ToolStripSeparator menuCanonnSeperator;
        private ToolStripMenuItem menuEDAstro;
        private Label label2;
        private ComboBox comboRegion;
        private ToolStripStatusLabel toolRegionName;
        private ToolStripStatusLabel toolDiscoveryDate;
        private ToolStripStatusLabel toolFiller;
        private ToolStripStatusLabel toolBodyName;
    }
}