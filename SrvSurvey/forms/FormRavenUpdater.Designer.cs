namespace SrvSurvey.forms
{
    partial class FormRavenUpdater
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
                if (game.status != null) game.status.StatusChanged -= Status_StatusChanged;
                if (game.journals != null) game.journals.onJournalEntry -= Journals_onJournalEntry;
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
            lblDesc = new Label();
            lblTask = new Label();
            list = new ListView();
            colName = new ColumnHeader();
            colBody = new ColumnHeader();
            colBuildType = new ColumnHeader();
            colType = new ColumnHeader();
            colStatus = new ColumnHeader();
            btnClose = new Button();
            btnMerge = new Button();
            btnSubmit = new Button();
            panel1 = new Panel();
            comboStatus = new ComboBox();
            comboBuildSubType = new ComboBox();
            comboBuildType = new ComboBox();
            comboBody = new ComboBox();
            txtName = new TextBox();
            label1 = new Label();
            btnNext = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblDesc
            // 
            lblDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblDesc.BorderStyle = BorderStyle.Fixed3D;
            lblDesc.Location = new Point(0, 0);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(691, 400);
            lblDesc.TabIndex = 0;
            lblDesc.Text = "Follow the instructions:";
            // 
            // lblTask
            // 
            lblTask.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTask.BorderStyle = BorderStyle.Fixed3D;
            lblTask.Location = new Point(0, 9);
            lblTask.Name = "lblTask";
            lblTask.Size = new Size(680, 51);
            lblTask.TabIndex = 1;
            lblTask.Text = "Discovery scan needed ...";
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colName, colBody, colBuildType, colType, colStatus });
            list.Location = new Point(0, 92);
            list.Name = "list";
            list.Size = new Size(680, 223);
            list.TabIndex = 2;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ItemChecked += list_ItemChecked;
            list.SelectedIndexChanged += list_SelectedIndexChanged;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 220;
            // 
            // colBody
            // 
            colBody.Text = "Body";
            colBody.Width = 80;
            // 
            // colBuildType
            // 
            colBuildType.Text = "BuildType";
            colBuildType.Width = 120;
            // 
            // colType
            // 
            colType.Text = "Type";
            colType.Width = 160;
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            colStatus.Width = 80;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(605, 350);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 3;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnMerge
            // 
            btnMerge.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMerge.Enabled = false;
            btnMerge.Location = new Point(12, 362);
            btnMerge.Name = "btnMerge";
            btnMerge.Size = new Size(75, 23);
            btnMerge.TabIndex = 4;
            btnMerge.Text = "&Merge";
            btnMerge.UseVisualStyleBackColor = true;
            btnMerge.Click += btnMerge_Click;
            // 
            // btnSubmit
            // 
            btnSubmit.Location = new Point(605, 321);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(75, 23);
            btnSubmit.TabIndex = 5;
            btnSubmit.Text = "&Submit";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panel1.Controls.Add(comboStatus);
            panel1.Controls.Add(comboBuildSubType);
            panel1.Controls.Add(comboBuildType);
            panel1.Controls.Add(comboBody);
            panel1.Controls.Add(txtName);
            panel1.Controls.Add(label1);
            panel1.Enabled = false;
            panel1.Location = new Point(93, 321);
            panel1.Name = "panel1";
            panel1.Size = new Size(387, 117);
            panel1.TabIndex = 6;
            // 
            // comboStatus
            // 
            comboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboStatus.FormattingEnabled = true;
            comboStatus.Location = new Point(151, 56);
            comboStatus.Name = "comboStatus";
            comboStatus.Size = new Size(146, 23);
            comboStatus.TabIndex = 9;
            comboStatus.SelectedIndexChanged += comboStatus_SelectedIndexChanged;
            // 
            // comboBuildSubType
            // 
            comboBuildSubType.DisplayMember = "Value";
            comboBuildSubType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildSubType.Enabled = false;
            comboBuildSubType.FormattingEnabled = true;
            comboBuildSubType.Location = new Point(226, 85);
            comboBuildSubType.Name = "comboBuildSubType";
            comboBuildSubType.Size = new Size(136, 23);
            comboBuildSubType.TabIndex = 7;
            comboBuildSubType.ValueMember = "Key";
            comboBuildSubType.SelectedIndexChanged += comboBuildSubType_SelectedIndexChanged;
            // 
            // comboBuildType
            // 
            comboBuildType.DisplayMember = "Value";
            comboBuildType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildType.Enabled = false;
            comboBuildType.FormattingEnabled = true;
            comboBuildType.Location = new Point(3, 85);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(217, 23);
            comboBuildType.TabIndex = 6;
            comboBuildType.ValueMember = "Key";
            comboBuildType.SelectedIndexChanged += comboBuildType_SelectedIndexChanged;
            // 
            // comboBody
            // 
            comboBody.DisplayMember = "Value";
            comboBody.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBody.FormattingEnabled = true;
            comboBody.Location = new Point(3, 56);
            comboBody.Name = "comboBody";
            comboBody.Size = new Size(142, 23);
            comboBody.TabIndex = 1;
            comboBody.ValueMember = "Key";
            comboBody.SelectedIndexChanged += comboBody_SelectedIndexChanged;
            // 
            // txtName
            // 
            txtName.Location = new Point(3, 27);
            txtName.Name = "txtName";
            txtName.Size = new Size(359, 23);
            txtName.TabIndex = 0;
            txtName.TextChanged += txtName_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 9);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 8;
            label1.Text = "Merge into:";
            // 
            // btnNext
            // 
            btnNext.Location = new Point(524, 321);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(75, 23);
            btnNext.TabIndex = 7;
            btnNext.Text = "&Next";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // FormRavenUpdater
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(692, 450);
            Controls.Add(btnNext);
            Controls.Add(panel1);
            Controls.Add(btnSubmit);
            Controls.Add(btnMerge);
            Controls.Add(btnClose);
            Controls.Add(list);
            Controls.Add(lblTask);
            Controls.Add(lblDesc);
            Name = "FormRavenUpdater";
            Text = "Raven Colonial Updater";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblDesc;
        private Label lblTask;
        private ListView list;
        private Button btnClose;
        private Button btnMerge;
        private Button btnSubmit;
        private ColumnHeader colName;
        private ColumnHeader colBody;
        private ColumnHeader colType;
        private ColumnHeader colBuildType;
        private Panel panel1;
        private ComboBox comboBody;
        private TextBox txtName;
        private Label label1;
        private ComboBox comboBuildSubType;
        private ComboBox comboBuildType;
        private ColumnHeader colStatus;
        private ComboBox comboStatus;
        private Button btnNext;
    }
}