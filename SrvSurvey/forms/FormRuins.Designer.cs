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
            map = new PictureBox();
            mapContext = new ContextMenuStrip(components);
            mnuName = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            mnuPresent = new ToolStripMenuItem();
            mnuAbsent = new ToolStripMenuItem();
            mnuEmpty = new ToolStripMenuItem();
            mnuUnknown = new ToolStripMenuItem();
            toolStripTower = new ToolStripSeparator();
            toolTowers = new ToolStripMenuItem();
            toolTowerTop = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolTowerMiddle = new ToolStripMenuItem();
            toolTowerBottom = new ToolStripMenuItem();
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
            checkShowLegend = new CheckBox();
            checkNotes = new CheckBox();
            splitter = new SplitContainer();
            panelEdit = new Panel();
            label5 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            numA = new NumericUpDown();
            button1 = new FlatButton();
            numScale = new NumericUpDown();
            numY = new NumericUpDown();
            numX = new NumericUpDown();
            txtSystem = new TextBox();
            btnSaveNotes = new FlatButton();
            lblLastVisited = new Label();
            label6 = new Label();
            txtNotes = new TextBox();
            ((System.ComponentModel.ISupportInitialize)map).BeginInit();
            mapContext.SuspendLayout();
            statusStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter).BeginInit();
            splitter.Panel1.SuspendLayout();
            splitter.Panel2.SuspendLayout();
            splitter.SuspendLayout();
            panelEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX).BeginInit();
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
            map.Size = new Size(666, 482);
            map.TabIndex = 1;
            map.TabStop = false;
            map.Tag = "NoTheme";
            map.Click += map_Click;
            map.Paint += map_Paint;
            map.DoubleClick += map_DoubleClick;
            map.MouseDown += map_MouseDown;
            map.MouseMove += map_MouseMove;
            map.MouseUp += map_MouseUp;
            // 
            // mapContext
            // 
            mapContext.Items.AddRange(new ToolStripItem[] { mnuName, toolStripSeparator1, mnuPresent, mnuAbsent, mnuEmpty, mnuUnknown, toolStripTower, toolTowers, toolTowerTop, toolTowerMiddle, toolTowerBottom });
            mapContext.Name = "mapContext";
            mapContext.Size = new Size(173, 214);
            // 
            // mnuName
            // 
            mnuName.Enabled = false;
            mnuName.Name = "mnuName";
            mnuName.Size = new Size(172, 22);
            mnuName.Text = "Name:";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(169, 6);
            // 
            // mnuPresent
            // 
            mnuPresent.Name = "mnuPresent";
            mnuPresent.Size = new Size(172, 22);
            mnuPresent.Text = "Present";
            mnuPresent.Click += mnuPresent_Click;
            // 
            // mnuAbsent
            // 
            mnuAbsent.Name = "mnuAbsent";
            mnuAbsent.Size = new Size(172, 22);
            mnuAbsent.Text = "Absent";
            mnuAbsent.Click += mnuAbsent_Click;
            // 
            // mnuEmpty
            // 
            mnuEmpty.Name = "mnuEmpty";
            mnuEmpty.Size = new Size(172, 22);
            mnuEmpty.Text = "Empty";
            mnuEmpty.Click += mnuEmpty_Click;
            // 
            // mnuUnknown
            // 
            mnuUnknown.Checked = true;
            mnuUnknown.CheckState = CheckState.Checked;
            mnuUnknown.Name = "mnuUnknown";
            mnuUnknown.Size = new Size(172, 22);
            mnuUnknown.Text = "Unknown";
            mnuUnknown.Click += mnuUnknown_Click;
            // 
            // toolStripTower
            // 
            toolStripTower.Name = "toolStripTower";
            toolStripTower.Size = new Size(169, 6);
            // 
            // toolTowers
            // 
            toolTowers.Enabled = false;
            toolTowers.Name = "toolTowers";
            toolTowers.Size = new Size(172, 22);
            toolTowers.Text = "Component Tower";
            // 
            // toolTowerTop
            // 
            toolTowerTop.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem3 });
            toolTowerTop.Name = "toolTowerTop";
            toolTowerTop.Size = new Size(172, 22);
            toolTowerTop.Text = "Top: abc";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(107, 22);
            toolStripMenuItem1.Text = "AB cd";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(107, 22);
            toolStripMenuItem2.Text = "asd aa";
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(107, 22);
            toolStripMenuItem3.Text = "dd ee";
            // 
            // toolTowerMiddle
            // 
            toolTowerMiddle.Name = "toolTowerMiddle";
            toolTowerMiddle.Size = new Size(172, 22);
            toolTowerMiddle.Text = "Middle: def";
            // 
            // toolTowerBottom
            // 
            toolTowerBottom.Name = "toolTowerBottom";
            toolTowerBottom.Size = new Size(172, 22);
            toolTowerBottom.Text = "Bottom: xyz";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblSelectedItem, lblStatus, lblObeliskGroups, lblSurveyCompletion, progressSurvey, lblZoom });
            statusStrip1.Location = new Point(0, 534);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(882, 24);
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
            lblStatus.Size = new Size(423, 19);
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
            comboSite.Size = new Size(432, 23);
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
            label4.MouseDoubleClick += label4_MouseDoubleClick;
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
            panel1.Controls.Add(checkShowLegend);
            panel1.Controls.Add(checkNotes);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(comboSite);
            panel1.Controls.Add(comboSiteType);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(882, 48);
            panel1.TabIndex = 0;
            // 
            // checkShowLegend
            // 
            checkShowLegend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkShowLegend.AutoSize = true;
            checkShowLegend.Location = new Point(691, 11);
            checkShowLegend.Name = "checkShowLegend";
            checkShowLegend.Size = new Size(94, 19);
            checkShowLegend.TabIndex = 4;
            checkShowLegend.Text = "Show legend";
            checkShowLegend.UseVisualStyleBackColor = true;
            checkShowLegend.CheckedChanged += checkShowLegend_CheckedChanged;
            // 
            // checkNotes
            // 
            checkNotes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkNotes.AutoSize = true;
            checkNotes.Location = new Point(791, 11);
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
            splitter.Panel1.Controls.Add(panelEdit);
            splitter.Panel1.Controls.Add(map);
            // 
            // splitter.Panel2
            // 
            splitter.Panel2.Controls.Add(txtSystem);
            splitter.Panel2.Controls.Add(btnSaveNotes);
            splitter.Panel2.Controls.Add(lblLastVisited);
            splitter.Panel2.Controls.Add(label6);
            splitter.Panel2.Controls.Add(txtNotes);
            splitter.Panel2.Padding = new Padding(2);
            splitter.Size = new Size(882, 486);
            splitter.SplitterDistance = 670;
            splitter.TabIndex = 1;
            // 
            // panelEdit
            // 
            panelEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panelEdit.BorderStyle = BorderStyle.Fixed3D;
            panelEdit.Controls.Add(label5);
            panelEdit.Controls.Add(label3);
            panelEdit.Controls.Add(label2);
            panelEdit.Controls.Add(label1);
            panelEdit.Controls.Add(numA);
            panelEdit.Controls.Add(button1);
            panelEdit.Controls.Add(numScale);
            panelEdit.Controls.Add(numY);
            panelEdit.Controls.Add(numX);
            panelEdit.Location = new Point(3, 445);
            panelEdit.Name = "panelEdit";
            panelEdit.Size = new Size(416, 34);
            panelEdit.TabIndex = 4;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(263, 5);
            label5.Name = "label5";
            label5.Size = new Size(41, 15);
            label5.TabIndex = 8;
            label5.Text = "Angle:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(163, 5);
            label3.Name = "label3";
            label3.Size = new Size(37, 15);
            label3.TabIndex = 7;
            label3.Text = "Scale:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(83, 7);
            label2.Name = "label2";
            label2.Size = new Size(17, 15);
            label2.TabIndex = 6;
            label2.Text = "Y:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(17, 15);
            label1.TabIndex = 5;
            label1.Text = "X:";
            // 
            // numA
            // 
            numA.DecimalPlaces = 3;
            numA.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            numA.Location = new Point(310, 3);
            numA.Maximum = new decimal(new int[] { 361, 0, 0, 0 });
            numA.Minimum = new decimal(new int[] { 1, 0, 0, int.MinValue });
            numA.Name = "numA";
            numA.Size = new Size(51, 23);
            numA.TabIndex = 4;
            numA.ValueChanged += numA_ValueChanged;
            // 
            // button1
            // 
            button1.Location = new Point(367, 1);
            button1.Name = "button1";
            button1.Size = new Size(38, 23);
            button1.TabIndex = 3;
            button1.Text = "save";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // numScale
            // 
            numScale.DecimalPlaces = 3;
            numScale.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numScale.Location = new Point(206, 3);
            numScale.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numScale.Name = "numScale";
            numScale.Size = new Size(51, 23);
            numScale.TabIndex = 2;
            numScale.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numScale.ValueChanged += numScale_ValueChanged;
            // 
            // numY
            // 
            numY.Location = new Point(106, 3);
            numY.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numY.Name = "numY";
            numY.Size = new Size(51, 23);
            numY.TabIndex = 1;
            numY.TextAlign = HorizontalAlignment.Right;
            numY.Value = new decimal(new int[] { 555, 0, 0, 0 });
            numY.ValueChanged += numY_ValueChanged;
            // 
            // numX
            // 
            numX.Location = new Point(26, 3);
            numX.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numX.Name = "numX";
            numX.Size = new Size(51, 23);
            numX.TabIndex = 0;
            numX.TextAlign = HorizontalAlignment.Right;
            numX.Value = new decimal(new int[] { 555, 0, 0, 0 });
            numX.ValueChanged += numX_ValueChanged;
            // 
            // txtSystem
            // 
            txtSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystem.BorderStyle = BorderStyle.FixedSingle;
            txtSystem.Location = new Point(5, 5);
            txtSystem.Name = "txtSystem";
            txtSystem.ReadOnly = true;
            txtSystem.Size = new Size(197, 23);
            txtSystem.TabIndex = 4;
            // 
            // btnSaveNotes
            // 
            btnSaveNotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSaveNotes.Location = new Point(5, 53);
            btnSaveNotes.Name = "btnSaveNotes";
            btnSaveNotes.Size = new Size(195, 23);
            btnSaveNotes.TabIndex = 0;
            btnSaveNotes.Text = "Save notes";
            btnSaveNotes.UseVisualStyleBackColor = true;
            btnSaveNotes.Click += btnSaveNotes_Click;
            // 
            // lblLastVisited
            // 
            lblLastVisited.AutoSize = true;
            lblLastVisited.Location = new Point(76, 33);
            lblLastVisited.Name = "lblLastVisited";
            lblLastVisited.Size = new Size(85, 15);
            lblLastVisited.TabIndex = 3;
            lblLastVisited.Text = "April 40th 2099";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(2, 33);
            label6.Name = "label6";
            label6.Size = new Size(68, 15);
            label6.TabIndex = 2;
            label6.Text = "Last visited:";
            // 
            // txtNotes
            // 
            txtNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNotes.BorderStyle = BorderStyle.FixedSingle;
            txtNotes.Location = new Point(5, 82);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Both;
            txtNotes.Size = new Size(197, 398);
            txtNotes.TabIndex = 1;
            // 
            // FormRuins
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(882, 558);
            Controls.Add(splitter);
            Controls.Add(panel1);
            Controls.Add(statusStrip1);
            MinimumSize = new Size(400, 400);
            Name = "FormRuins";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Guardian Maps";
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
            panelEdit.ResumeLayout(false);
            panelEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numA).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX).EndInit();
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
        private FlatButton btnSaveNotes;
        private ContextMenuStrip mapContext;
        private ToolStripMenuItem mnuName;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnuPresent;
        private ToolStripMenuItem mnuAbsent;
        private ToolStripMenuItem mnuEmpty;
        private ToolStripMenuItem mnuUnknown;
        private ToolStripStatusLabel lblObeliskGroups;
        private ToolStripProgressBar progressSurvey;
        private Panel panelEdit;
        private NumericUpDown numScale;
        private NumericUpDown numY;
        private NumericUpDown numX;
        private FlatButton button1;
        private NumericUpDown numA;
        private Label label5;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label label6;
        private Label lblLastVisited;
        private CheckBox checkShowLegend;
        private TextBox txtSystem;
        private ToolStripSeparator toolStripTower;
        private ToolStripMenuItem toolTowerTop;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolTowers;
        private ToolStripMenuItem toolTowerMiddle;
        private ToolStripMenuItem toolTowerBottom;
    }
}