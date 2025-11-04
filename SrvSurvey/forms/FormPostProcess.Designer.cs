namespace SrvSurvey
{
    partial class FormPostProcess
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPostProcess));
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "FSD Jumps:", "-" }, -1);
            ListViewItem listViewItem2 = new ListViewItem(new string[] { "Distance travelled (ly):", "-" }, -1);
            ListViewItem listViewItem3 = new ListViewItem(new string[] { "Bodies approached:", "-" }, -1);
            ListViewItem listViewItem4 = new ListViewItem(new string[] { "Organisms scanned:", "-" }, -1);
            ListViewItem listViewItem5 = new ListViewItem(new string[] { "Cargo bought:", "-" }, -1);
            ListViewItem listViewItem6 = new ListViewItem(new string[] { "Cargo sold:", "-" }, -1);
            ListViewItem listViewItem7 = new ListViewItem(new string[] { "Cargo transferred:", "-" }, -1);
            ListViewItem listViewItem8 = new ListViewItem(new string[] { "Cargo collected:", "-" }, -1);
            ListViewItem listViewItem9 = new ListViewItem(new string[] { "Cargo contributed:", "-" }, -1);
            ListViewItem listViewItem10 = new ListViewItem(new string[] { "Docked:", "-" }, -1);
            ListViewItem listViewItem11 = new ListViewItem(new string[] { "Touchdown:", "-" }, -1);
            ListViewItem listViewItem12 = new ListViewItem(new string[] { "Died:", "-" }, -1);
            btnStart = new FlatButton();
            progress = new ProgressBar();
            lblProgress = new Label();
            btnSystems = new FlatButton();
            lblDesc = new Label();
            linkLabel1 = new LinkLabel();
            lblCmdr = new Label();
            lblStartDate = new Label();
            dateTimePicker = new DateTimePicker();
            comboCmdr = new ComboCmdr();
            btnLongAgo = new FlatButton();
            btnClose = new FlatButton();
            listStats = new ListView();
            colName = new ColumnHeader();
            colValue = new ColumnHeader();
            checkTrailblazers = new CheckBox();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(95, 261);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(293, 25);
            btnStart.TabIndex = 7;
            btnStart.Text = "&Process journals";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // progress
            // 
            progress.Location = new Point(12, 232);
            progress.Name = "progress";
            progress.Size = new Size(481, 23);
            progress.TabIndex = 10;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(12, 206);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(130, 15);
            lblProgress.TabIndex = 9;
            lblProgress.Text = "Processing ? of ? files ...";
            // 
            // btnSystems
            // 
            btnSystems.Enabled = false;
            btnSystems.Location = new Point(452, 109);
            btnSystems.Name = "btnSystems";
            btnSystems.Size = new Size(41, 23);
            btnSystems.TabIndex = 19;
            btnSystems.Text = "Process systems";
            btnSystems.UseVisualStyleBackColor = true;
            btnSystems.Visible = false;
            btnSystems.Click += btnSystems_Click;
            // 
            // lblDesc
            // 
            lblDesc.Location = new Point(12, 9);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(481, 97);
            lblDesc.TabIndex = 0;
            lblDesc.Text = resources.GetString("lblDesc.Text");
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(12, 106);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(124, 15);
            linkLabel1.TabIndex = 1;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Learn more in the wiki";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // lblCmdr
            // 
            lblCmdr.AutoSize = true;
            lblCmdr.Location = new Point(12, 142);
            lblCmdr.Name = "lblCmdr";
            lblCmdr.Size = new Size(77, 15);
            lblCmdr.TabIndex = 2;
            lblCmdr.Text = "Commander:";
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Location = new Point(29, 171);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(60, 15);
            lblStartDate.TabIndex = 4;
            lblStartDate.Text = "Start date:";
            // 
            // dateTimePicker
            // 
            dateTimePicker.Location = new Point(95, 168);
            dateTimePicker.MinDate = new DateTime(2014, 12, 15, 0, 0, 0, 0);
            dateTimePicker.Name = "dateTimePicker";
            dateTimePicker.Size = new Size(223, 23);
            dateTimePicker.TabIndex = 5;
            dateTimePicker.ValueChanged += dateTimePicker_ValueChanged;
            // 
            // comboCmdr
            // 
            comboCmdr.cmdrFid = null;
            comboCmdr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCmdr.FlatStyle = FlatStyle.System;
            comboCmdr.FormattingEnabled = true;
            comboCmdr.Location = new Point(95, 139);
            comboCmdr.Name = "comboCmdr";
            comboCmdr.Size = new Size(398, 23);
            comboCmdr.TabIndex = 3;
            comboCmdr.SelectedIndexChanged += comboCmdr_SelectedIndexChanged;
            // 
            // btnLongAgo
            // 
            btnLongAgo.Location = new Point(324, 167);
            btnLongAgo.Name = "btnLongAgo";
            btnLongAgo.Size = new Size(169, 25);
            btnLongAgo.TabIndex = 6;
            btnLongAgo.Text = "&Set beginning of time";
            btnLongAgo.UseVisualStyleBackColor = true;
            btnLongAgo.Click += btnLongAgo_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(394, 261);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 25);
            btnClose.TabIndex = 8;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // listStats
            // 
            listStats.Columns.AddRange(new ColumnHeader[] { colName, colValue });
            listStats.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4, listViewItem5, listViewItem6, listViewItem7, listViewItem8, listViewItem9, listViewItem10, listViewItem11, listViewItem12 });
            listStats.Location = new Point(510, 9);
            listStats.Name = "listStats";
            listStats.Size = new Size(228, 258);
            listStats.TabIndex = 21;
            listStats.UseCompatibleStateImageBehavior = false;
            listStats.View = View.Details;
            // 
            // colName
            // 
            colName.Text = "Statistic";
            colName.Width = 140;
            // 
            // colValue
            // 
            colValue.Text = "Value";
            colValue.TextAlign = HorizontalAlignment.Right;
            // 
            // checkTrailblazers
            // 
            checkTrailblazers.AutoSize = true;
            checkTrailblazers.Checked = true;
            checkTrailblazers.CheckState = CheckState.Checked;
            checkTrailblazers.Location = new Point(585, 273);
            checkTrailblazers.Name = "checkTrailblazers";
            checkTrailblazers.Size = new Size(153, 19);
            checkTrailblazers.TabIndex = 22;
            checkTrailblazers.Text = "Compare for Trailblazers";
            checkTrailblazers.UseVisualStyleBackColor = true;
            // 
            // FormPostProcess
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(751, 297);
            Controls.Add(checkTrailblazers);
            Controls.Add(listStats);
            Controls.Add(btnClose);
            Controls.Add(btnLongAgo);
            Controls.Add(comboCmdr);
            Controls.Add(dateTimePicker);
            Controls.Add(lblStartDate);
            Controls.Add(lblCmdr);
            Controls.Add(linkLabel1);
            Controls.Add(lblDesc);
            Controls.Add(btnSystems);
            Controls.Add(lblProgress);
            Controls.Add(progress);
            Controls.Add(btnStart);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormPostProcess";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SrvSurvey - Old Journal Processor";
            Click += btnLongAgo_Click;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private FlatButton btnStart;
        private ProgressBar progress;
        private Label lblProgress;
        private FlatButton btnSystems;
        private Label lblDesc;
        private LinkLabel linkLabel1;
        private Label lblCmdr;
        private Label lblStartDate;
        private DateTimePicker dateTimePicker;
        private ComboCmdr comboCmdr;
        private FlatButton btnLongAgo;
        private FlatButton btnClose;
        private ListView listStats;
        private ColumnHeader colName;
        private ColumnHeader colValue;
        private CheckBox checkTrailblazers;
    }
}