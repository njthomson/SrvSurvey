namespace SrvSurvey
{
    partial class FormRuins
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

            if (game?.status != null)
                game.status.StatusChanged -= Status_StatusChanged;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRuins));
            map = new PictureBox();
            mapContext = new ContextMenuStrip(components);
            mnuName = new ToolStripMenuItem();
            mnuType = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            mnuPresent = new ToolStripMenuItem();
            mnuAbsent = new ToolStripMenuItem();
            mnuEmpty = new ToolStripMenuItem();
            mnuUnknown = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            statusStrip1 = new StatusStrip();
            lblSelectedItem = new ToolStripStatusLabel();
            lblStatus = new ToolStripStatusLabel();
            lblObeliskGroups = new ToolStripStatusLabel();
            lblSurveyCompletion = new ToolStripStatusLabel();
            progressSurvey = new ToolStripProgressBar();
            lblZoom = new ToolStripStatusLabel();
            comboSite = new ComboBox();
            label4 = new Label();
            comboSiteType = new ComboBox();
            panel1 = new Panel();
            checkNotes = new CheckBox();
            splitter = new SplitContainer();
            txtNotes = new TextBox();
            btnSaveNotes = new Button();
            ((System.ComponentModel.ISupportInitialize)map).BeginInit();
            mapContext.SuspendLayout();
            statusStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter).BeginInit();
            splitter.Panel1.SuspendLayout();
            splitter.Panel2.SuspendLayout();
            splitter.SuspendLayout();
            SuspendLayout();
            // 
            // map
            // 
            map.BackColor = SystemColors.ControlDarkDark;
            map.BackgroundImageLayout = ImageLayout.Center;
            map.Cursor = Cursors.Hand;
            map.Dock = DockStyle.Fill;
            map.Location = new Point(0, 0);
            map.Name = "map";
            map.Size = new Size(657, 483);
            map.TabIndex = 1;
            map.TabStop = false;
            map.Click += map_Click;
            map.Paint += map_Paint;
            map.DoubleClick += map_DoubleClick;
            map.MouseDown += map_MouseDown;
            map.MouseMove += map_MouseMove;
            map.MouseUp += map_MouseUp;
            // 
            // mapContext
            // 
            mapContext.Items.AddRange(new ToolStripItem[] { mnuName, mnuType, toolStripSeparator1, mnuPresent, mnuAbsent, mnuEmpty, mnuUnknown, toolStripSeparator2 });
            mapContext.Name = "mapContext";
            mapContext.Size = new Size(126, 148);
            // 
            // mnuName
            // 
            mnuName.Enabled = false;
            mnuName.Name = "mnuName";
            mnuName.Size = new Size(125, 22);
            mnuName.Text = "Name:";
            // 
            // mnuType
            // 
            mnuType.Enabled = false;
            mnuType.Name = "mnuType";
            mnuType.Size = new Size(125, 22);
            mnuType.Text = "Type:";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(122, 6);
            // 
            // mnuPresent
            // 
            mnuPresent.Name = "mnuPresent";
            mnuPresent.Size = new Size(125, 22);
            mnuPresent.Text = "Present";
            mnuPresent.Click += mnuPresent_Click;
            // 
            // mnuAbsent
            // 
            mnuAbsent.Name = "mnuAbsent";
            mnuAbsent.Size = new Size(125, 22);
            mnuAbsent.Text = "Absent";
            mnuAbsent.Click += mnuAbsent_Click;
            // 
            // mnuEmpty
            // 
            mnuEmpty.Name = "mnuEmpty";
            mnuEmpty.Size = new Size(125, 22);
            mnuEmpty.Text = "Empty";
            mnuEmpty.Click += mnuEmpty_Click;
            // 
            // mnuUnknown
            // 
            mnuUnknown.Checked = true;
            mnuUnknown.CheckState = CheckState.Checked;
            mnuUnknown.Name = "mnuUnknown";
            mnuUnknown.Size = new Size(125, 22);
            mnuUnknown.Text = "Unknown";
            mnuUnknown.Click += mnuUnknown_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(122, 6);
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblSelectedItem, lblStatus, lblObeliskGroups, lblSurveyCompletion, progressSurvey, lblZoom });
            statusStrip1.Location = new Point(0, 535);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(870, 24);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblSelectedItem
            // 
            lblSelectedItem.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblSelectedItem.BorderStyle = Border3DStyle.Sunken;
            lblSelectedItem.Name = "lblSelectedItem";
            lblSelectedItem.Size = new Size(54, 19);
            lblSelectedItem.Text = "t1 (relic)";
            // 
            // lblStatus
            // 
            lblStatus.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblStatus.BorderStyle = Border3DStyle.Sunken;
            lblStatus.DisplayStyle = ToolStripItemDisplayStyle.Text;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(411, 19);
            lblStatus.Spring = true;
            lblStatus.Text = "...";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblObeliskGroups
            // 
            lblObeliskGroups.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblObeliskGroups.BorderStyle = Border3DStyle.Sunken;
            lblObeliskGroups.Name = "lblObeliskGroups";
            lblObeliskGroups.Size = new Size(125, 19);
            lblObeliskGroups.Text = "Obelisk groups: A B C";
            // 
            // lblSurveyCompletion
            // 
            lblSurveyCompletion.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblSurveyCompletion.BorderStyle = Border3DStyle.Sunken;
            lblSurveyCompletion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            lblSurveyCompletion.Name = "lblSurveyCompletion";
            lblSurveyCompletion.Size = new Size(127, 19);
            lblSurveyCompletion.Text = "Survey: 55% complete";
            // 
            // progressSurvey
            // 
            progressSurvey.MarqueeAnimationSpeed = 500;
            progressSurvey.Name = "progressSurvey";
            progressSurvey.Size = new Size(50, 18);
            // 
            // lblZoom
            // 
            lblZoom.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblZoom.BorderStyle = Border3DStyle.Sunken;
            lblZoom.DisplayStyle = ToolStripItemDisplayStyle.Text;
            lblZoom.Name = "lblZoom";
            lblZoom.Size = new Size(55, 19);
            lblZoom.Text = "Zoom: 2";
            // 
            // comboSite
            // 
            comboSite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboSite.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSite.DropDownWidth = 404;
            comboSite.FormattingEnabled = true;
            comboSite.Items.AddRange(new object[] { "Alpha template", "Beta template", "Gamma template", "Bear template", "----------------" });
            comboSite.Location = new Point(239, 9);
            comboSite.Name = "comboSite";
            comboSite.Size = new Size(420, 23);
            comboSite.TabIndex = 2;
            comboSite.SelectedIndexChanged += comboSite_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 13);
            label4.Name = "label4";
            label4.Size = new Size(62, 15);
            label4.TabIndex = 0;
            label4.Text = "Select site:";
            // 
            // comboSiteType
            // 
            comboSiteType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSiteType.DropDownWidth = 404;
            comboSiteType.FormattingEnabled = true;
            comboSiteType.Items.AddRange(new object[] { "All", "Alpha", "Beta", "Gamma", "Lacrosse", "Crossroads", "Fistbump", "Hammerbot", "Bear", "Bowl", "Turtle", "Robolobster", "Squid", "Stickyhand" });
            comboSiteType.Location = new Point(80, 9);
            comboSiteType.Name = "comboSiteType";
            comboSiteType.Size = new Size(153, 23);
            comboSiteType.TabIndex = 1;
            comboSiteType.SelectedIndexChanged += comboSiteType_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(checkNotes);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(comboSite);
            panel1.Controls.Add(comboSiteType);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(870, 48);
            panel1.TabIndex = 0;
            // 
            // checkNotes
            // 
            checkNotes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkNotes.AutoSize = true;
            checkNotes.Location = new Point(779, 11);
            checkNotes.Name = "checkNotes";
            checkNotes.Size = new Size(87, 19);
            checkNotes.TabIndex = 3;
            checkNotes.Text = "Show notes";
            checkNotes.UseVisualStyleBackColor = true;
            checkNotes.CheckedChanged += checkNotes_CheckedChanged;
            // 
            // splitter
            // 
            splitter.BorderStyle = BorderStyle.Fixed3D;
            splitter.Dock = DockStyle.Fill;
            splitter.Location = new Point(0, 48);
            splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            splitter.Panel1.Controls.Add(map);
            // 
            // splitter.Panel2
            // 
            splitter.Panel2.Controls.Add(txtNotes);
            splitter.Panel2.Controls.Add(btnSaveNotes);
            splitter.Panel2.Padding = new Padding(2);
            splitter.Size = new Size(870, 487);
            splitter.SplitterDistance = 661;
            splitter.TabIndex = 1;
            // 
            // txtNotes
            // 
            txtNotes.BorderStyle = BorderStyle.None;
            txtNotes.Dock = DockStyle.Fill;
            txtNotes.Location = new Point(2, 25);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Both;
            txtNotes.Size = new Size(197, 456);
            txtNotes.TabIndex = 1;
            // 
            // btnSaveNotes
            // 
            btnSaveNotes.Dock = DockStyle.Top;
            btnSaveNotes.Location = new Point(2, 2);
            btnSaveNotes.Name = "btnSaveNotes";
            btnSaveNotes.Size = new Size(197, 23);
            btnSaveNotes.TabIndex = 0;
            btnSaveNotes.Text = "Save notes";
            btnSaveNotes.UseVisualStyleBackColor = true;
            btnSaveNotes.Click += btnSaveNotes_Click;
            // 
            // FormRuins
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(870, 559);
            Controls.Add(splitter);
            Controls.Add(panel1);
            Controls.Add(statusStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(400, 400);
            Name = "FormRuins";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ruins Map";
            Activated += FormRuins_Activated;
            Load += FormRuins_Load;
            ResizeEnd += FormRuins_ResizeEnd;
            ((System.ComponentModel.ISupportInitialize)map).EndInit();
            mapContext.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            splitter.Panel1.ResumeLayout(false);
            splitter.Panel2.ResumeLayout(false);
            splitter.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitter).EndInit();
            splitter.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private PictureBox map;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblSelectedItem;
        private ComboBox comboSite;
        private Label label4;
        private ToolStripStatusLabel lblSurveyCompletion;
        private ToolStripStatusLabel lblZoom;
        private ComboBox comboSiteType;
        private Panel panel1;
        private SplitContainer splitter;
        private TextBox txtNotes;
        private CheckBox checkNotes;
        private Button btnSaveNotes;
        private ContextMenuStrip mapContext;
        private ToolStripMenuItem mnuName;
        private ToolStripMenuItem mnuType;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnuPresent;
        private ToolStripMenuItem mnuAbsent;
        private ToolStripMenuItem mnuEmpty;
        private ToolStripMenuItem mnuUnknown;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripStatusLabel lblObeliskGroups;
        private ToolStripProgressBar progressSurvey;
    }
}