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
            btnClose = new FlatButton();
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
            btnSave = new FlatButton();
            btnProcess = new Button();
            btnConclude = new Button();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(739, 553);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 26);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // listQuickStats
            // 
            listQuickStats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            listQuickStats.BackColor = SystemColors.Menu;
            listQuickStats.Columns.AddRange(new ColumnHeader[] { colMetricName, colMetricCount });
            listQuickStats.FullRowSelect = true;
            listQuickStats.GridLines = true;
            listQuickStats.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listQuickStats.Location = new Point(553, 0);
            listQuickStats.Margin = new Padding(3, 0, 0, 0);
            listQuickStats.MultiSelect = false;
            listQuickStats.Name = "listQuickStats";
            listQuickStats.ShowGroups = false;
            listQuickStats.ShowItemToolTips = true;
            listQuickStats.Size = new Size(235, 495);
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
            listSystems.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2 });
            listSystems.Location = new Point(0, 0);
            listSystems.Name = "listSystems";
            listSystems.Size = new Size(197, 494);
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
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 14);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(802, 533);
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
            tabPage1.Size = new Size(794, 500);
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
            txtByline.Size = new Size(547, 34);
            txtByline.TabIndex = 1;
            txtByline.Text = "cmdr Stuff | Jan 2nd 2025 ~ Dec 31st 3304";
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
            txtJourneyName.Size = new Size(547, 34);
            txtJourneyName.TabIndex = 0;
            txtJourneyName.Text = "journey name";
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
            txtDescription.ShortcutsEnabled = false;
            txtDescription.Size = new Size(547, 420);
            txtDescription.TabIndex = 2;
            txtDescription.Text = "journey description";
            txtDescription.TextChanged += journey_TextChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(splitContainer1);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(794, 500);
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
            splitContainer1.Size = new Size(788, 494);
            splitContainer1.SplitterDistance = 197;
            splitContainer1.TabIndex = 4;
            // 
            // panelSystem
            // 
            panelSystem.BackColor = SystemColors.Control;
            panelSystem.Dock = DockStyle.Fill;
            panelSystem.Location = new Point(0, 0);
            panelSystem.Margin = new Padding(0);
            panelSystem.Name = "panelSystem";
            panelSystem.Size = new Size(587, 494);
            panelSystem.TabIndex = 0;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(658, 553);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 26);
            btnSave.TabIndex = 3;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Visible = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnProcess
            // 
            btnProcess.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnProcess.Location = new Point(16, 553);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(139, 26);
            btnProcess.TabIndex = 1;
            btnProcess.Text = "Re-process journals";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcess_Click;
            // 
            // btnConclude
            // 
            btnConclude.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnConclude.Location = new Point(161, 553);
            btnConclude.Name = "btnConclude";
            btnConclude.Size = new Size(131, 26);
            btnConclude.TabIndex = 2;
            btnConclude.Text = "Conclude journey";
            btnConclude.UseVisualStyleBackColor = true;
            btnConclude.Click += btnConclude_Click;
            // 
            // FormJourneyViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(826, 593);
            Controls.Add(btnConclude);
            Controls.Add(btnProcess);
            Controls.Add(btnSave);
            Controls.Add(tabControl1);
            Controls.Add(btnClose);
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
            ResumeLayout(false);
        }

        #endregion

        private FlatButton btnClose;
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
        private FlatButton btnSave;
        private Button btnProcess;
        private Button btnConclude;
        private ColumnHeader colInterest;
    }
}