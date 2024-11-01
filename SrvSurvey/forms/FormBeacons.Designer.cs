namespace SrvSurvey
{
    partial class FormBeacons
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBeacons));
            TreeNode treeNode1 = new TreeNode("All Sites");
            TreeNode treeNode2 = new TreeNode("Beacons");
            TreeNode treeNode3 = new TreeNode("Alpha");
            TreeNode treeNode4 = new TreeNode("Beta");
            TreeNode treeNode5 = new TreeNode("Gamma");
            TreeNode treeNode6 = new TreeNode("All Ruins", new TreeNode[] { treeNode3, treeNode4, treeNode5 });
            TreeNode treeNode7 = new TreeNode("Lacrosse");
            TreeNode treeNode8 = new TreeNode("Crossroads");
            TreeNode treeNode9 = new TreeNode("Fistbump");
            TreeNode treeNode10 = new TreeNode("Hammerbot");
            TreeNode treeNode11 = new TreeNode("Bear");
            TreeNode treeNode12 = new TreeNode("Bowl");
            TreeNode treeNode13 = new TreeNode("Turtle");
            TreeNode treeNode14 = new TreeNode("Robolobster");
            TreeNode treeNode15 = new TreeNode("Squid");
            TreeNode treeNode16 = new TreeNode("Stickyhand");
            TreeNode treeNode17 = new TreeNode("All Structures", new TreeNode[] { treeNode7, treeNode8, treeNode9, treeNode10, treeNode11, treeNode12, treeNode13, treeNode14, treeNode15, treeNode16 });
            btnShare = new FlatButton();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            srvSurveyRamTahHelpersToolStripMenuItem = new ToolStripMenuItem();
            decodingTheAncientRuinsToolStripMenuItem = new ToolStripMenuItem();
            ramTah2DecryptingTheGuardianLogsToolStripMenuItem = new ToolStripMenuItem();
            comboCurrentSystem = new ComboBox();
            label1 = new Label();
            txtFilter = new TextBox();
            btnFilter = new FlatButton();
            colNotes = new ColumnHeader();
            colLastVisited = new ColumnHeader();
            colArrival = new ColumnHeader();
            colDistance = new ColumnHeader();
            colBody = new ColumnHeader();
            colSystem = new ColumnHeader();
            grid = new ListView();
            colId = new ColumnHeader();
            colSiteType = new ColumnHeader();
            colIndex = new ColumnHeader();
            colImages = new ColumnHeader();
            colSurveyComplete = new ColumnHeader();
            colRamTah = new ColumnHeader();
            comboVisited = new ComboBox();
            checkRamTah = new CheckBox();
            btnSiteTypes = new FlatButton();
            txtSiteTypes = new TextBox();
            treeSiteTypes = new TreeView();
            panelSiteTypes = new Panel();
            btnSetSiteTypes = new FlatButton();
            contextMenu = new ContextMenuStrip(components);
            menuOpenSiteSurvey = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            copyToolStripMenuItem = new ToolStripMenuItem();
            copySystemNameToolStripMenuItem = new ToolStripMenuItem();
            systemAddressToolStripMenuItem = new ToolStripMenuItem();
            copyBodyNameToolStripMenuItem = new ToolStripMenuItem();
            copyStarPosToolStripMenuItem = new ToolStripMenuItem();
            copyLatlongToolStripMenuItem = new ToolStripMenuItem();
            notesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            openSystemInEDSMToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuOpenImagesFolder = new ToolStripMenuItem();
            menuOpenDataFile = new ToolStripMenuItem();
            menuOpenPubData = new ToolStripMenuItem();
            checkOnlyNeeded = new CheckBox();
            statusStrip1.SuspendLayout();
            panelSiteTypes.SuspendLayout();
            contextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // btnShare
            // 
            btnShare.FlatStyle = FlatStyle.Flat;
            btnShare.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnShare.Location = new Point(736, 36);
            btnShare.Name = "btnShare";
            btnShare.Size = new Size(208, 23);
            btnShare.TabIndex = 18;
            btnShare.Text = "Share your discovered data **";
            btnShare.UseVisualStyleBackColor = true;
            btnShare.Visible = false;
            btnShare.Click += btnShare_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, toolStripDropDownButton1 });
            statusStrip1.Location = new Point(0, 431);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1176, 22);
            statusStrip1.TabIndex = 17;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.DisplayStyle = ToolStripItemDisplayStyle.Text;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1064, 17);
            lblStatus.Spring = true;
            lblStatus.Text = "...";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { srvSurveyRamTahHelpersToolStripMenuItem, decodingTheAncientRuinsToolStripMenuItem, ramTah2DecryptingTheGuardianLogsToolStripMenuItem });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(97, 20);
            toolStripDropDownButton1.Text = "Guidance links";
            // 
            // srvSurveyRamTahHelpersToolStripMenuItem
            // 
            srvSurveyRamTahHelpersToolStripMenuItem.Name = "srvSurveyRamTahHelpersToolStripMenuItem";
            srvSurveyRamTahHelpersToolStripMenuItem.Size = new Size(303, 22);
            srvSurveyRamTahHelpersToolStripMenuItem.Text = "SrvSurvey Ram Tah helpers";
            srvSurveyRamTahHelpersToolStripMenuItem.Click += srvSurveyRamTahHelpersToolStripMenuItem_Click;
            // 
            // decodingTheAncientRuinsToolStripMenuItem
            // 
            decodingTheAncientRuinsToolStripMenuItem.Name = "decodingTheAncientRuinsToolStripMenuItem";
            decodingTheAncientRuinsToolStripMenuItem.Size = new Size(303, 22);
            decodingTheAncientRuinsToolStripMenuItem.Text = "Ram Tah #1 - Decoding the Ancient Ruins";
            decodingTheAncientRuinsToolStripMenuItem.Click += decodingTheAncientRuinsToolStripMenuItem_Click;
            // 
            // ramTah2DecryptingTheGuardianLogsToolStripMenuItem
            // 
            ramTah2DecryptingTheGuardianLogsToolStripMenuItem.Name = "ramTah2DecryptingTheGuardianLogsToolStripMenuItem";
            ramTah2DecryptingTheGuardianLogsToolStripMenuItem.Size = new Size(303, 22);
            ramTah2DecryptingTheGuardianLogsToolStripMenuItem.Text = "Ram Tah #2 - Decrypting the Guardian Logs";
            ramTah2DecryptingTheGuardianLogsToolStripMenuItem.Click += ramTah2DecryptingTheGuardianLogsToolStripMenuItem_Click;
            // 
            // comboCurrentSystem
            // 
            comboCurrentSystem.BackColor = SystemColors.ScrollBar;
            comboCurrentSystem.FormattingEnabled = true;
            comboCurrentSystem.Location = new Point(549, 6);
            comboCurrentSystem.Name = "comboCurrentSystem";
            comboCurrentSystem.Size = new Size(181, 23);
            comboCurrentSystem.TabIndex = 16;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(407, 10);
            label1.Name = "label1";
            label1.Size = new Size(136, 15);
            label1.TabIndex = 15;
            label1.Text = "Measure distances from:";
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(68, 7);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(254, 23);
            txtFilter.TabIndex = 12;
            // 
            // btnFilter
            // 
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Location = new Point(12, 7);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(50, 23);
            btnFilter.TabIndex = 11;
            btnFilter.Text = "Filter:";
            btnFilter.UseVisualStyleBackColor = true;
            btnFilter.Click += btnFilter_Click;
            // 
            // colNotes
            // 
            colNotes.Text = "Notes";
            colNotes.Width = 160;
            // 
            // colLastVisited
            // 
            colLastVisited.Text = "Last visited";
            colLastVisited.TextAlign = HorizontalAlignment.Center;
            colLastVisited.Width = 70;
            // 
            // colArrival
            // 
            colArrival.Text = "Arrival distance:";
            colArrival.TextAlign = HorizontalAlignment.Right;
            colArrival.Width = 96;
            // 
            // colDistance
            // 
            colDistance.Text = "System distance";
            colDistance.TextAlign = HorizontalAlignment.Right;
            colDistance.Width = 96;
            // 
            // colBody
            // 
            colBody.Text = "Body";
            colBody.Width = 55;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 170;
            // 
            // grid
            // 
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.Columns.AddRange(new ColumnHeader[] { colId, colSystem, colBody, colDistance, colArrival, colLastVisited, colSiteType, colIndex, colImages, colSurveyComplete, colRamTah, colNotes });
            grid.FullRowSelect = true;
            grid.Location = new Point(0, 65);
            grid.Name = "grid";
            grid.ShowGroups = false;
            grid.Size = new Size(1176, 363);
            grid.TabIndex = 10;
            grid.UseCompatibleStateImageBehavior = false;
            grid.View = View.Details;
            grid.ColumnClick += grid_ColumnClick;
            grid.SelectedIndexChanged += grid_SelectedIndexChanged;
            grid.DoubleClick += menuOpenSiteSurvey_Click;
            grid.MouseClick += grid_MouseClick;
            // 
            // colId
            // 
            colId.Text = "ID";
            colId.Width = 45;
            // 
            // colSiteType
            // 
            colSiteType.Text = "Site type";
            colSiteType.Width = 80;
            // 
            // colIndex
            // 
            colIndex.Text = "Index";
            colIndex.TextAlign = HorizontalAlignment.Center;
            colIndex.Width = 41;
            // 
            // colImages
            // 
            colImages.Text = "Has images";
            colImages.TextAlign = HorizontalAlignment.Center;
            colImages.Width = 73;
            // 
            // colSurveyComplete
            // 
            colSurveyComplete.Text = "Survey status";
            colSurveyComplete.TextAlign = HorizontalAlignment.Center;
            colSurveyComplete.Width = 100;
            // 
            // colRamTah
            // 
            colRamTah.Text = "Ram Tah Logs";
            colRamTah.Width = 160;
            // 
            // comboVisited
            // 
            comboVisited.DropDownStyle = ComboBoxStyle.DropDownList;
            comboVisited.FormattingEnabled = true;
            comboVisited.Items.AddRange(new object[] { "All", "Visited", "Unvisited" });
            comboVisited.Location = new Point(328, 7);
            comboVisited.Name = "comboVisited";
            comboVisited.Size = new Size(73, 23);
            comboVisited.TabIndex = 20;
            comboVisited.SelectedIndexChanged += btnFilter_Click;
            // 
            // checkRamTah
            // 
            checkRamTah.AutoSize = true;
            checkRamTah.Location = new Point(736, 9);
            checkRamTah.Name = "checkRamTah";
            checkRamTah.Size = new Size(131, 19);
            checkRamTah.TabIndex = 19;
            checkRamTah.Text = "Show Ram Tah Logs";
            checkRamTah.UseVisualStyleBackColor = true;
            checkRamTah.CheckedChanged += checkRamTah_CheckedChanged;
            // 
            // btnSiteTypes
            // 
            btnSiteTypes.FlatStyle = FlatStyle.Flat;
            btnSiteTypes.Location = new Point(12, 36);
            btnSiteTypes.Name = "btnSiteTypes";
            btnSiteTypes.Size = new Size(50, 23);
            btnSiteTypes.TabIndex = 23;
            btnSiteTypes.Text = "Type:";
            btnSiteTypes.UseVisualStyleBackColor = true;
            btnSiteTypes.Click += btnSiteTypes_Click;
            // 
            // txtSiteTypes
            // 
            txtSiteTypes.Location = new Point(68, 36);
            txtSiteTypes.Name = "txtSiteTypes";
            txtSiteTypes.ReadOnly = true;
            txtSiteTypes.Size = new Size(662, 23);
            txtSiteTypes.TabIndex = 24;
            txtSiteTypes.Text = "All Ruins and Structures";
            // 
            // treeSiteTypes
            // 
            treeSiteTypes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeSiteTypes.CheckBoxes = true;
            treeSiteTypes.FullRowSelect = true;
            treeSiteTypes.HideSelection = false;
            treeSiteTypes.Location = new Point(0, 32);
            treeSiteTypes.Name = "treeSiteTypes";
            treeNode1.Name = "nAllSites";
            treeNode1.Text = "All Sites";
            treeNode2.Name = "nBeacons";
            treeNode2.Text = "Beacons";
            treeNode3.Checked = true;
            treeNode3.Name = "nAlpha";
            treeNode3.Text = "Alpha";
            treeNode4.Checked = true;
            treeNode4.Name = "nBeta";
            treeNode4.Text = "Beta";
            treeNode5.Checked = true;
            treeNode5.Name = "nGamma";
            treeNode5.Text = "Gamma";
            treeNode6.Checked = true;
            treeNode6.Name = "nAllRuins";
            treeNode6.Text = "All Ruins";
            treeNode7.Checked = true;
            treeNode7.Name = "nLacrosse";
            treeNode7.Text = "Lacrosse";
            treeNode8.Checked = true;
            treeNode8.Name = "nCrossroads";
            treeNode8.Text = "Crossroads";
            treeNode9.Checked = true;
            treeNode9.Name = "nFistbump";
            treeNode9.Text = "Fistbump";
            treeNode10.Checked = true;
            treeNode10.Name = "nHammerbot";
            treeNode10.Text = "Hammerbot";
            treeNode11.Checked = true;
            treeNode11.Name = "nBear";
            treeNode11.Text = "Bear";
            treeNode12.Checked = true;
            treeNode12.Name = "nBowl";
            treeNode12.Text = "Bowl";
            treeNode13.Checked = true;
            treeNode13.Name = "nTurtle";
            treeNode13.Text = "Turtle";
            treeNode14.Checked = true;
            treeNode14.Name = "nRobolobster";
            treeNode14.Text = "Robolobster";
            treeNode15.Checked = true;
            treeNode15.Name = "nSquid";
            treeNode15.Text = "Squid";
            treeNode16.Checked = true;
            treeNode16.Name = "nStickyhand";
            treeNode16.Text = "Stickyhand";
            treeNode17.Checked = true;
            treeNode17.Name = "nAllStructures";
            treeNode17.Text = "All Structures";
            treeSiteTypes.Nodes.AddRange(new TreeNode[] { treeNode1, treeNode2, treeNode6, treeNode17 });
            treeSiteTypes.ShowPlusMinus = false;
            treeSiteTypes.Size = new Size(432, 306);
            treeSiteTypes.TabIndex = 26;
            treeSiteTypes.AfterCheck += treeSiteTypes_AfterCheck;
            treeSiteTypes.NodeMouseDoubleClick += treeSiteTypes_NodeMouseDoubleClick;
            // 
            // panelSiteTypes
            // 
            panelSiteTypes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelSiteTypes.BorderStyle = BorderStyle.Fixed3D;
            panelSiteTypes.Controls.Add(treeSiteTypes);
            panelSiteTypes.Controls.Add(btnSetSiteTypes);
            panelSiteTypes.Location = new Point(43, 65);
            panelSiteTypes.Name = "panelSiteTypes";
            panelSiteTypes.Size = new Size(436, 342);
            panelSiteTypes.TabIndex = 27;
            panelSiteTypes.Visible = false;
            // 
            // btnSetSiteTypes
            // 
            btnSetSiteTypes.FlatStyle = FlatStyle.Flat;
            btnSetSiteTypes.Location = new Point(3, 3);
            btnSetSiteTypes.Name = "btnSetSiteTypes";
            btnSetSiteTypes.Size = new Size(75, 23);
            btnSetSiteTypes.TabIndex = 27;
            btnSetSiteTypes.Text = "Okay";
            btnSetSiteTypes.UseVisualStyleBackColor = true;
            btnSetSiteTypes.Click += btnSetSiteTypes_Click;
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] { menuOpenSiteSurvey, toolStripSeparator3, copyToolStripMenuItem, copySystemNameToolStripMenuItem, systemAddressToolStripMenuItem, copyBodyNameToolStripMenuItem, copyStarPosToolStripMenuItem, copyLatlongToolStripMenuItem, notesToolStripMenuItem, toolStripSeparator1, openSystemInEDSMToolStripMenuItem, toolStripSeparator2, menuOpenImagesFolder, menuOpenDataFile, menuOpenPubData });
            contextMenu.Name = "contextMenu";
            contextMenu.Size = new Size(198, 286);
            // 
            // menuOpenSiteSurvey
            // 
            menuOpenSiteSurvey.Name = "menuOpenSiteSurvey";
            menuOpenSiteSurvey.Size = new Size(197, 22);
            menuOpenSiteSurvey.Text = "Open site survey";
            menuOpenSiteSurvey.Click += menuOpenSiteSurvey_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(194, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Enabled = false;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new Size(197, 22);
            copyToolStripMenuItem.Text = "Copy:";
            // 
            // copySystemNameToolStripMenuItem
            // 
            copySystemNameToolStripMenuItem.Name = "copySystemNameToolStripMenuItem";
            copySystemNameToolStripMenuItem.Size = new Size(197, 22);
            copySystemNameToolStripMenuItem.Text = "System name";
            copySystemNameToolStripMenuItem.Click += copySystemNameToolStripMenuItem_Click;
            // 
            // systemAddressToolStripMenuItem
            // 
            systemAddressToolStripMenuItem.Name = "systemAddressToolStripMenuItem";
            systemAddressToolStripMenuItem.Size = new Size(197, 22);
            systemAddressToolStripMenuItem.Text = "System address";
            systemAddressToolStripMenuItem.Click += systemAddressToolStripMenuItem_Click;
            // 
            // copyBodyNameToolStripMenuItem
            // 
            copyBodyNameToolStripMenuItem.Name = "copyBodyNameToolStripMenuItem";
            copyBodyNameToolStripMenuItem.Size = new Size(197, 22);
            copyBodyNameToolStripMenuItem.Text = "Body name";
            copyBodyNameToolStripMenuItem.Click += copyBodyNameToolStripMenuItem_Click;
            // 
            // copyStarPosToolStripMenuItem
            // 
            copyStarPosToolStripMenuItem.Name = "copyStarPosToolStripMenuItem";
            copyStarPosToolStripMenuItem.Size = new Size(197, 22);
            copyStarPosToolStripMenuItem.Text = "Star pos (x, y, z)";
            copyStarPosToolStripMenuItem.Click += copyStarPosToolStripMenuItem_Click;
            // 
            // copyLatlongToolStripMenuItem
            // 
            copyLatlongToolStripMenuItem.Name = "copyLatlongToolStripMenuItem";
            copyLatlongToolStripMenuItem.Size = new Size(197, 22);
            copyLatlongToolStripMenuItem.Text = "Lat/long";
            copyLatlongToolStripMenuItem.Click += copyLatlongToolStripMenuItem_Click;
            // 
            // notesToolStripMenuItem
            // 
            notesToolStripMenuItem.Name = "notesToolStripMenuItem";
            notesToolStripMenuItem.Size = new Size(197, 22);
            notesToolStripMenuItem.Text = "Notes";
            notesToolStripMenuItem.Click += notesToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(194, 6);
            // 
            // openSystemInEDSMToolStripMenuItem
            // 
            openSystemInEDSMToolStripMenuItem.Name = "openSystemInEDSMToolStripMenuItem";
            openSystemInEDSMToolStripMenuItem.Size = new Size(197, 22);
            openSystemInEDSMToolStripMenuItem.Text = "View at Canonn Signals";
            openSystemInEDSMToolStripMenuItem.Click += openSystemInEDSMToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(194, 6);
            // 
            // menuOpenImagesFolder
            // 
            menuOpenImagesFolder.Name = "menuOpenImagesFolder";
            menuOpenImagesFolder.Size = new Size(197, 22);
            menuOpenImagesFolder.Text = "Open images folder";
            menuOpenImagesFolder.Click += menuOpenImagesFolder_Click;
            // 
            // menuOpenDataFile
            // 
            menuOpenDataFile.Name = "menuOpenDataFile";
            menuOpenDataFile.Size = new Size(197, 22);
            menuOpenDataFile.Text = "Open data file";
            menuOpenDataFile.Click += openDataFileToolStripMenuItem_Click;
            // 
            // menuOpenPubData
            // 
            menuOpenPubData.Name = "menuOpenPubData";
            menuOpenPubData.Size = new Size(197, 22);
            menuOpenPubData.Text = "Open pubData file";
            menuOpenPubData.Click += menuOpenPubData_Click;
            // 
            // checkOnlyNeeded
            // 
            checkOnlyNeeded.AutoSize = true;
            checkOnlyNeeded.Location = new Point(873, 9);
            checkOnlyNeeded.Name = "checkOnlyNeeded";
            checkOnlyNeeded.Size = new Size(123, 19);
            checkOnlyNeeded.TabIndex = 28;
            checkOnlyNeeded.Text = "Show only needed";
            checkOnlyNeeded.UseVisualStyleBackColor = true;
            checkOnlyNeeded.Visible = false;
            checkOnlyNeeded.CheckedChanged += checkOnlyNeeded_CheckedChanged;
            // 
            // FormBeacons
            // 
            AcceptButton = btnFilter;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.AppWorkspace;
            ClientSize = new Size(1176, 453);
            Controls.Add(checkOnlyNeeded);
            Controls.Add(panelSiteTypes);
            Controls.Add(txtSiteTypes);
            Controls.Add(btnSiteTypes);
            Controls.Add(comboVisited);
            Controls.Add(checkRamTah);
            Controls.Add(btnShare);
            Controls.Add(statusStrip1);
            Controls.Add(comboCurrentSystem);
            Controls.Add(label1);
            Controls.Add(txtFilter);
            Controls.Add(btnFilter);
            Controls.Add(grid);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormBeacons";
            Text = "Ruins, Structures and Beacons";
            Load += FormBeacons_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panelSiteTypes.ResumeLayout(false);
            contextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlatButton btnShare;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private Label label1;
        private TextBox txtFilter;
        private FlatButton btnFilter;
        private ColumnHeader colNotes;
        private ColumnHeader colLastVisited;
        private ColumnHeader colArrival;
        private ColumnHeader colDistance;
        private ColumnHeader colBody;
        private ColumnHeader colSystem;
        private ListView grid;
        private ColumnHeader colSiteType;
        public ComboBox comboCurrentSystem;
        private ComboBox comboVisited;
        private CheckBox checkRamTah;
        private FlatButton btnSiteTypes;
        private TextBox txtSiteTypes;
        private TreeView treeSiteTypes;
        private Panel panelSiteTypes;
        private FlatButton btnSetSiteTypes;
        private ColumnHeader colRamTah;
        private ColumnHeader colId;
        private ColumnHeader colIndex;
        private ColumnHeader colImages;
        private ColumnHeader colSurveyComplete;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem copySystemNameToolStripMenuItem;
        private ToolStripMenuItem copyBodyNameToolStripMenuItem;
        private ToolStripMenuItem copyStarPosToolStripMenuItem;
        private ToolStripMenuItem copyLatlongToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem notesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem openSystemInEDSMToolStripMenuItem;
        private ToolStripMenuItem menuOpenSiteSurvey;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuOpenDataFile;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem menuOpenImagesFolder;
        private ToolStripMenuItem systemAddressToolStripMenuItem;
        private ToolStripMenuItem menuOpenPubData;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem decodingTheAncientRuinsToolStripMenuItem;
        private ToolStripMenuItem ramTah2DecryptingTheGuardianLogsToolStripMenuItem;
        private ToolStripMenuItem srvSurveyRamTahHelpersToolStripMenuItem;
        private CheckBox checkOnlyNeeded;
    }
}