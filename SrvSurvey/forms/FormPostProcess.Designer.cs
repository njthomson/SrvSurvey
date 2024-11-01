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
            btnStart = new FlatButton();
            progress = new ProgressBar();
            lblProgress = new Label();
            btnSystems = new FlatButton();
            lblDesc = new Label();
            linkLabel1 = new LinkLabel();
            lblCmdr = new Label();
            lblStartDate = new Label();
            dateTimePicker = new DateTimePicker();
            comboCmdr = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            txtJumpCount = new TextBox();
            txtDistance = new TextBox();
            txtBodyCount = new TextBox();
            txtOrgCount = new TextBox();
            btnLongAgo = new FlatButton();
            btnClose = new FlatButton();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Location = new Point(94, 197);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(293, 25);
            btnStart.TabIndex = 7;
            btnStart.Text = "Process journals";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // progress
            // 
            progress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progress.Location = new Point(12, 253);
            progress.Name = "progress";
            progress.Size = new Size(481, 23);
            progress.TabIndex = 9;
            // 
            // lblProgress
            // 
            lblProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(12, 227);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(130, 15);
            lblProgress.TabIndex = 8;
            lblProgress.Text = "Processing ? of ? files ...";
            // 
            // btnSystems
            // 
            btnSystems.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSystems.Enabled = false;
            btnSystems.FlatStyle = FlatStyle.Flat;
            btnSystems.Location = new Point(12, 316);
            btnSystems.Name = "btnSystems";
            btnSystems.Size = new Size(41, 23);
            btnSystems.TabIndex = 18;
            btnSystems.Text = "Process systems";
            btnSystems.UseVisualStyleBackColor = true;
            btnSystems.Visible = false;
            btnSystems.Click += btnSystems_Click;
            // 
            // lblDesc
            // 
            lblDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
            lblCmdr.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCmdr.AutoSize = true;
            lblCmdr.Location = new Point(12, 142);
            lblCmdr.Name = "lblCmdr";
            lblCmdr.Size = new Size(77, 15);
            lblCmdr.TabIndex = 2;
            lblCmdr.Text = "Commander:";
            // 
            // lblStartDate
            // 
            lblStartDate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStartDate.AutoSize = true;
            lblStartDate.Location = new Point(29, 171);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(60, 15);
            lblStartDate.TabIndex = 4;
            lblStartDate.Text = "Start date:";
            // 
            // dateTimePicker
            // 
            dateTimePicker.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            dateTimePicker.Location = new Point(95, 168);
            dateTimePicker.MinDate = new DateTime(2014, 12, 15, 0, 0, 0, 0);
            dateTimePicker.Name = "dateTimePicker";
            dateTimePicker.Size = new Size(223, 23);
            dateTimePicker.TabIndex = 5;
            dateTimePicker.ValueChanged += dateTimePicker_ValueChanged;
            // 
            // comboCmdr
            // 
            comboCmdr.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            comboCmdr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCmdr.FlatStyle = FlatStyle.System;
            comboCmdr.FormattingEnabled = true;
            comboCmdr.Location = new Point(95, 139);
            comboCmdr.Name = "comboCmdr";
            comboCmdr.Size = new Size(398, 23);
            comboCmdr.TabIndex = 3;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(93, 285);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 10;
            label1.Text = "FSD Jumps:";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(290, 285);
            label2.Name = "label2";
            label2.Size = new Size(126, 15);
            label2.TabIndex = 12;
            label2.Text = "Distance travelled (LY):";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(93, 314);
            label3.Name = "label3";
            label3.Size = new Size(111, 15);
            label3.TabIndex = 14;
            label3.Text = "Bodies approached:";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(290, 314);
            label4.Name = "label4";
            label4.Size = new Size(114, 15);
            label4.TabIndex = 16;
            label4.Text = "Organisms scanned:";
            // 
            // txtJumpCount
            // 
            txtJumpCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtJumpCount.BorderStyle = BorderStyle.FixedSingle;
            txtJumpCount.Location = new Point(213, 282);
            txtJumpCount.Name = "txtJumpCount";
            txtJumpCount.ReadOnly = true;
            txtJumpCount.Size = new Size(71, 23);
            txtJumpCount.TabIndex = 11;
            txtJumpCount.Text = "0";
            txtJumpCount.TextAlign = HorizontalAlignment.Center;
            // 
            // txtDistance
            // 
            txtDistance.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtDistance.BorderStyle = BorderStyle.FixedSingle;
            txtDistance.Location = new Point(422, 282);
            txtDistance.Name = "txtDistance";
            txtDistance.ReadOnly = true;
            txtDistance.Size = new Size(71, 23);
            txtDistance.TabIndex = 13;
            txtDistance.Text = "0";
            txtDistance.TextAlign = HorizontalAlignment.Center;
            // 
            // txtBodyCount
            // 
            txtBodyCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtBodyCount.BorderStyle = BorderStyle.FixedSingle;
            txtBodyCount.Location = new Point(213, 311);
            txtBodyCount.Name = "txtBodyCount";
            txtBodyCount.ReadOnly = true;
            txtBodyCount.Size = new Size(71, 23);
            txtBodyCount.TabIndex = 15;
            txtBodyCount.Text = "0";
            txtBodyCount.TextAlign = HorizontalAlignment.Center;
            // 
            // txtOrgCount
            // 
            txtOrgCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtOrgCount.BorderStyle = BorderStyle.FixedSingle;
            txtOrgCount.Location = new Point(422, 311);
            txtOrgCount.Name = "txtOrgCount";
            txtOrgCount.ReadOnly = true;
            txtOrgCount.Size = new Size(71, 23);
            txtOrgCount.TabIndex = 17;
            txtOrgCount.Text = "0";
            txtOrgCount.TextAlign = HorizontalAlignment.Center;
            // 
            // btnLongAgo
            // 
            btnLongAgo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLongAgo.FlatStyle = FlatStyle.Flat;
            btnLongAgo.Location = new Point(324, 167);
            btnLongAgo.Name = "btnLongAgo";
            btnLongAgo.Size = new Size(169, 25);
            btnLongAgo.TabIndex = 19;
            btnLongAgo.Text = "Set beginning of time";
            btnLongAgo.UseVisualStyleBackColor = true;
            btnLongAgo.Click += btnLongAgo_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Location = new Point(393, 197);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 25);
            btnClose.TabIndex = 20;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // FormPostProcess
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(505, 351);
            Controls.Add(btnClose);
            Controls.Add(btnLongAgo);
            Controls.Add(txtOrgCount);
            Controls.Add(txtBodyCount);
            Controls.Add(txtDistance);
            Controls.Add(txtJumpCount);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
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
        private ComboBox comboCmdr;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox txtJumpCount;
        private TextBox txtDistance;
        private TextBox txtBodyCount;
        private TextBox txtOrgCount;
        private FlatButton btnLongAgo;
        private FlatButton btnClose;
    }
}