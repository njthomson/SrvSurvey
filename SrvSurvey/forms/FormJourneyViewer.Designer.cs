namespace SrvSurvey.forms
{
    partial class FormJourneyViewer
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
            ListViewItem listViewItem1 = new ListViewItem("Praea Euq GQ-P c5-8");
            ListViewItem listViewItem2 = new ListViewItem("Swoilz OO-X d2-10");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormJourneyViewer));
            listQuickStats = new ListView();
            colMetricName = new ColumnHeader();
            colMetricCount = new ColumnHeader();
            listSystems = new ListView();
            colSystem = new ColumnHeader();
            colInterest = new ColumnHeader();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            txtByline = new TextBox2();
            txtJourneyName = new TextBox2();
            txtDescription = new TextBox();
            tabPage2 = new TabPage();
            splitContainer1 = new SplitContainer();
            panelSystem = new Panel();
            status = new StatusStrip();
            menuStatus = new ToolStripStatusLabel();
            menuSaveUpdates = new ToolStripSplitButton();
            menuDiscard = new ToolStripSplitButton();
            menuOptions = new ToolStripDropDownButton();
            menuTopMost = new ToolStripMenuItem();
            menuGalacticTime = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuReprocessJournals = new ToolStripMenuItem();
            menuConclude = new ToolStripMenuItem();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            status.SuspendLayout();
            SuspendLayout();
            // 
            // listQuickStats
            // 
            listQuickStats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            listQuickStats.BackColor = SystemColors.Menu;
            listQuickStats.Columns.AddRange(new ColumnHeader[] { colMetricName, colMetricCount });
            listQuickStats.FullRowSelect = true;
            listQuickStats.GridLines = true;
            listQuickStats.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listQuickStats.Location = new Point(456, 0);
            listQuickStats.Margin = new Padding(3, 0, 0, 0);
            listQuickStats.MultiSelect = false;
            listQuickStats.Name = "listQuickStats";
            listQuickStats.ShowGroups = false;
            listQuickStats.ShowItemToolTips = true;
            listQuickStats.Size = new Size(362, 538);
            listQuickStats.TabIndex = 3;
            listQuickStats.UseCompatibleStateImageBehavior = false;
            listQuickStats.View = View.Details;
            // 
            // colMetricName
            // 
            colMetricName.Text = "Metric";
            colMetricName.Width = 160;
            // 
            // colMetricCount
            // 
            colMetricCount.Text = "Count";
            colMetricCount.TextAlign = HorizontalAlignment.Right;
            // 
            // listSystems
            // 
            listSystems.Columns.AddRange(new ColumnHeader[] { colSystem, colInterest });
            listSystems.Dock = DockStyle.Fill;
            listSystems.FullRowSelect = true;
            listSystems.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listSystems.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2 });
            listSystems.Location = new Point(0, 0);
            listSystems.Name = "listSystems";
            listSystems.Size = new Size(203, 534);
            listSystems.TabIndex = 0;
            listSystems.UseCompatibleStateImageBehavior = false;
            listSystems.View = View.Details;
            listSystems.SelectedIndexChanged += listSystems_SelectedIndexChanged;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 160;
            // 
            // colInterest
            // 
            colInterest.Text = "...";
            // 
            // tabControl1
            // 
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(826, 571);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(txtByline);
            tabPage1.Controls.Add(txtJourneyName);
            tabPage1.Controls.Add(listQuickStats);
            tabPage1.Controls.Add(txtDescription);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(818, 538);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Overview";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtByline
            // 
            txtByline.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtByline.BackColor = SystemColors.Control;
            txtByline.Font = new Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtByline.ForeColor = SystemColors.WindowText;
            txtByline.Location = new Point(0, 37);
            txtByline.Margin = new Padding(0, 3, 0, 3);
            txtByline.Name = "txtByline";
            txtByline.Padding = new Padding(3, 0, 3, 0);
            txtByline.ReadOnly = true;
            txtByline.Size = new Size(453, 34);
            txtByline.TabIndex = 1;
            txtByline.Text = "cmdr Stuff | Jan 2nd 2025 ~ Dec 31st 3304";
            txtByline.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // txtJourneyName
            // 
            txtJourneyName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtJourneyName.BackColor = SystemColors.Window;
            txtJourneyName.BorderStyle = BorderStyle.FixedSingle;
            txtJourneyName.Font = new Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtJourneyName.ForeColor = SystemColors.WindowText;
            txtJourneyName.Location = new Point(0, 0);
            txtJourneyName.Margin = new Padding(0);
            txtJourneyName.Name = "txtJourneyName";
            txtJourneyName.Padding = new Padding(3, 0, 3, 0);
            txtJourneyName.Size = new Size(453, 34);
            txtJourneyName.TabIndex = 0;
            txtJourneyName.Text = "journey name";
            txtJourneyName.UseEdgeButton = TextBox2.EdgeButton.None;
            txtJourneyName.TextChanged2 += journey_TextChanged;
            // 
            // txtDescription
            // 
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtDescription.Location = new Point(0, 75);
            txtDescription.Margin = new Padding(0);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(453, 463);
            txtDescription.TabIndex = 2;
            txtDescription.Text = "journey description";
            txtDescription.TextChanged += journey_TextChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(splitContainer1);
            tabPage2.Location = new Point(4, 27);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(818, 540);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Visited Systems";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = SystemColors.ControlDark;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listSystems);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = SystemColors.WindowFrame;
            splitContainer1.Panel2.Controls.Add(panelSystem);
            splitContainer1.Size = new Size(812, 534);
            splitContainer1.SplitterDistance = 203;
            splitContainer1.TabIndex = 4;
            // 
            // panelSystem
            // 
            panelSystem.BackColor = SystemColors.Control;
            panelSystem.Dock = DockStyle.Fill;
            panelSystem.Location = new Point(0, 0);
            panelSystem.Margin = new Padding(0);
            panelSystem.Name = "panelSystem";
            panelSystem.Size = new Size(605, 534);
            panelSystem.TabIndex = 0;
            // 
            // status
            // 
            status.Items.AddRange(new ToolStripItem[] { menuStatus, menuSaveUpdates, menuDiscard, menuOptions });
            status.Location = new Point(0, 571);
            status.Name = "status";
            status.Size = new Size(826, 22);
            status.TabIndex = 5;
            status.Text = "statusStrip1";
            // 
            // menuStatus
            // 
            menuStatus.Name = "menuStatus";
            menuStatus.Size = new Size(737, 17);
            menuStatus.Spring = true;
            menuStatus.Text = "-";
            // 
            // menuSaveUpdates
            // 
            menuSaveUpdates.BackColor = SystemColors.ActiveCaption;
            menuSaveUpdates.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuSaveUpdates.DropDownButtonWidth = 0;
            menuSaveUpdates.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            menuSaveUpdates.Image = (Image)resources.GetObject("menuSaveUpdates.Image");
            menuSaveUpdates.ImageTransparentColor = Color.Magenta;
            menuSaveUpdates.Name = "menuSaveUpdates";
            menuSaveUpdates.Size = new Size(87, 20);
            menuSaveUpdates.Text = "&Save changes";
            menuSaveUpdates.Visible = false;
            menuSaveUpdates.ButtonClick += btnSave_Click;
            // 
            // menuDiscard
            // 
            menuDiscard.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuDiscard.DropDownButtonWidth = 0;
            menuDiscard.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            menuDiscard.Image = (Image)resources.GetObject("menuDiscard.Image");
            menuDiscard.ImageTransparentColor = Color.Magenta;
            menuDiscard.Name = "menuDiscard";
            menuDiscard.Size = new Size(53, 20);
            menuDiscard.Text = "&Discard";
            menuDiscard.Visible = false;
            menuDiscard.ButtonClick += menuDiscard_ButtonClick;
            // 
            // menuOptions
            // 
            menuOptions.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuOptions.DropDownItems.AddRange(new ToolStripItem[] { menuTopMost, menuGalacticTime, toolStripSeparator1, menuReprocessJournals, menuConclude });
            menuOptions.Image = (Image)resources.GetObject("menuOptions.Image");
            menuOptions.ImageTransparentColor = Color.Magenta;
            menuOptions.Name = "menuOptions";
            menuOptions.Size = new Size(74, 20);
            menuOptions.Text = "Options ...";
            // 
            // menuTopMost
            // 
            menuTopMost.Checked = true;
            menuTopMost.CheckOnClick = true;
            menuTopMost.CheckState = CheckState.Checked;
            menuTopMost.Name = "menuTopMost";
            menuTopMost.Size = new Size(204, 22);
            menuTopMost.Text = "Top most";
            menuTopMost.Click += menuTopMost_Click;
            // 
            // menuGalacticTime
            // 
            menuGalacticTime.Checked = true;
            menuGalacticTime.CheckOnClick = true;
            menuGalacticTime.CheckState = CheckState.Checked;
            menuGalacticTime.Name = "menuGalacticTime";
            menuGalacticTime.Size = new Size(204, 22);
            menuGalacticTime.Text = "Galactic time zone";
            menuGalacticTime.Click += menuGalacticTime_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(201, 6);
            // 
            // menuReprocessJournals
            // 
            menuReprocessJournals.Name = "menuReprocessJournals";
            menuReprocessJournals.Size = new Size(204, 22);
            menuReprocessJournals.Text = "Re-process journal items";
            menuReprocessJournals.Click += btnProcess_Click;
            // 
            // menuConclude
            // 
            menuConclude.Name = "menuConclude";
            menuConclude.Size = new Size(204, 22);
            menuConclude.Text = "Conclude journey";
            menuConclude.Click += btnConclude_Click;
            // 
            // FormJourneyViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(826, 593);
            Controls.Add(tabControl1);
            Controls.Add(status);
            Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "FormJourneyViewer";
            Text = "Journey: ";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            status.ResumeLayout(false);
            status.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListView listQuickStats;
        private ColumnHeader colMetricName;
        private ColumnHeader colMetricCount;
        private ListView listSystems;
        private ColumnHeader colSystem;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox2 txtJourneyName;
        private TextBox txtDescription;
        private TextBox2 txtByline;
        private SplitContainer splitContainer1;
        private Panel panelSystem;
        private ColumnHeader colInterest;
        private StatusStrip status;
        private ToolStripDropDownButton menuOptions;
        private ToolStripMenuItem menuTopMost;
        private ToolStripMenuItem menuGalacticTime;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuReprocessJournals;
        private ToolStripStatusLabel menuStatus;
        private ToolStripMenuItem menuConclude;
        private ToolStripSplitButton menuDiscard;
        private ToolStripSplitButton menuSaveUpdates;
    }
}