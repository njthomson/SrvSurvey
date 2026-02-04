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
            lblTask = new Label();
            list = new ListView();
            colName = new ColumnHeader();
            colBody = new ColumnHeader();
            colBuildType = new ColumnHeader();
            colType = new ColumnHeader();
            colStatus = new ColumnHeader();
            btnSubmit = new Button();
            panel1 = new Panel();
            comboStatus = new ThemedComboBox();
            comboBuildSubType = new ThemedComboBox();
            comboBuildType = new ThemedComboBox();
            comboBody = new ThemedComboBox();
            txtName = new TextBox();
            label1 = new Label();
            label4 = new Label();
            label3 = new Label();
            label5 = new Label();
            btnRemove = new Button();
            btnNext = new Button();
            statusStrip1 = new StatusStrip();
            toolLinkRC = new ToolStripStatusLabel();
            toolFiller = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolLinkWiki = new ToolStripStatusLabel();
            lblPhase = new Label();
            btnReload = new Button();
            panel1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lblTask
            // 
            lblTask.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTask.BackColor = SystemColors.Info;
            lblTask.BorderStyle = BorderStyle.Fixed3D;
            lblTask.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTask.Location = new Point(0, 33);
            lblTask.Name = "lblTask";
            lblTask.Size = new Size(774, 66);
            lblTask.TabIndex = 0;
            lblTask.Text = "This is an interactive tool.\r\nPlease follow the instructions here or see the wiki for more guidance.\r\nClick Next to continue...";
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.Columns.AddRange(new ColumnHeader[] { colName, colBody, colBuildType, colType, colStatus });
            list.FullRowSelect = true;
            list.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            list.Location = new Point(0, 102);
            list.Name = "list";
            list.ShowGroups = false;
            list.Size = new Size(774, 206);
            list.TabIndex = 2;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.SelectedIndexChanged += list_SelectedIndexChanged;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 200;
            // 
            // colBody
            // 
            colBody.Text = "Body";
            colBody.Width = 130;
            // 
            // colBuildType
            // 
            colBuildType.Text = "BuildType";
            colBuildType.Width = 160;
            // 
            // colType
            // 
            colType.Text = "Type";
            colType.Width = 180;
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            colStatus.Width = 80;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmit.Enabled = false;
            btnSubmit.Location = new Point(667, 384);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(95, 53);
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
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label5);
            panel1.Enabled = false;
            panel1.Location = new Point(12, 314);
            panel1.Name = "panel1";
            panel1.Size = new Size(387, 123);
            panel1.TabIndex = 6;
            // 
            // comboStatus
            // 
            comboStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboStatus.FormattingEnabled = true;
            comboStatus.Location = new Point(240, 56);
            comboStatus.Name = "comboStatus";
            comboStatus.Size = new Size(146, 23);
            comboStatus.TabIndex = 5;
            comboStatus.SelectedIndexChanged += comboStatus_SelectedIndexChanged;
            // 
            // comboBuildSubType
            // 
            comboBuildSubType.DisplayMember = "Value";
            comboBuildSubType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildSubType.Enabled = false;
            comboBuildSubType.FormattingEnabled = true;
            comboBuildSubType.Location = new Point(211, 97);
            comboBuildSubType.Name = "comboBuildSubType";
            comboBuildSubType.Size = new Size(175, 23);
            comboBuildSubType.TabIndex = 8;
            comboBuildSubType.ValueMember = "Key";
            comboBuildSubType.SelectedIndexChanged += comboBuildSubType_SelectedIndexChanged;
            // 
            // comboBuildType
            // 
            comboBuildType.DisplayMember = "Value";
            comboBuildType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildType.DropDownWidth = 302;
            comboBuildType.Enabled = false;
            comboBuildType.FormattingEnabled = true;
            comboBuildType.Location = new Point(1, 97);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(204, 23);
            comboBuildType.TabIndex = 7;
            comboBuildType.ValueMember = "Key";
            comboBuildType.SelectedIndexChanged += comboBuildType_SelectedIndexChanged;
            // 
            // comboBody
            // 
            comboBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBody.DisplayMember = "Value";
            comboBody.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBody.FormattingEnabled = true;
            comboBody.Location = new Point(1, 56);
            comboBody.Name = "comboBody";
            comboBody.Size = new Size(233, 23);
            comboBody.TabIndex = 3;
            comboBody.ValueMember = "Key";
            comboBody.SelectedIndexChanged += comboBody_SelectedIndexChanged;
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(1, 16);
            txtName.Name = "txtName";
            txtName.Size = new Size(385, 23);
            txtName.TabIndex = 1;
            txtName.TextChanged += txtName_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(39, 13);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(0, 40);
            label4.Name = "label4";
            label4.Size = new Size(72, 13);
            label4.TabIndex = 2;
            label4.Text = "Parent body:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(0, 81);
            label3.Name = "label3";
            label3.Size = new Size(61, 13);
            label3.TabIndex = 6;
            label3.Text = "Build type:";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.Location = new Point(241, 40);
            label5.Name = "label5";
            label5.Size = new Size(42, 13);
            label5.TabIndex = 4;
            label5.Text = "Status:";
            // 
            // btnRemove
            // 
            btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemove.Location = new Point(405, 384);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(95, 53);
            btnRemove.Enabled = false;
            btnRemove.TabIndex = 8;
            btnRemove.Text = "&Delete this site";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNext.Location = new Point(404, 314);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(95, 53);
            btnNext.TabIndex = 3;
            btnNext.Text = "&Next";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.GripStyle = ToolStripGripStyle.Visible;
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolLinkRC, toolFiller, toolStripStatusLabel1, toolLinkWiki });
            statusStrip1.Location = new Point(0, 440);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(774, 24);
            statusStrip1.TabIndex = 7;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolLinkRC
            // 
            toolLinkRC.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolLinkRC.BorderStyle = Border3DStyle.SunkenOuter;
            toolLinkRC.IsLink = true;
            toolLinkRC.Name = "toolLinkRC";
            toolLinkRC.Size = new Size(26, 19);
            toolLinkRC.Text = "---";
            toolLinkRC.TextAlign = ContentAlignment.MiddleLeft;
            toolLinkRC.Click += toolLinkRC_Click;
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(624, 19);
            toolFiller.Spring = true;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolStripStatusLabel1.BorderStyle = Border3DStyle.SunkenOuter;
            toolStripStatusLabel1.IsLink = true;
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(56, 19);
            toolStripStatusLabel1.Text = "Visual ID";
            toolStripStatusLabel1.Click += toolStripStatusLabel1_Click;
            // 
            // toolLinkWiki
            // 
            toolLinkWiki.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolLinkWiki.BorderStyle = Border3DStyle.SunkenOuter;
            toolLinkWiki.IsLink = true;
            toolLinkWiki.Name = "toolLinkWiki";
            toolLinkWiki.Size = new Size(53, 19);
            toolLinkWiki.Text = "See wiki";
            toolLinkWiki.Click += toolLinkWiki_Click;
            // 
            // lblPhase
            // 
            lblPhase.AutoSize = true;
            lblPhase.Location = new Point(12, 9);
            lblPhase.Name = "lblPhase";
            lblPhase.Size = new Size(38, 15);
            lblPhase.TabIndex = 1;
            lblPhase.Text = "Phase";
            // 
            // btnReload
            // 
            btnReload.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReload.Enabled = false;
            btnReload.Location = new Point(667, 314);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(95, 53);
            btnReload.TabIndex = 4;
            btnReload.Text = "&Reload";
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // FormRavenUpdater
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(774, 464);
            Controls.Add(btnRemove);
            Controls.Add(btnReload);
            Controls.Add(lblPhase);
            Controls.Add(statusStrip1);
            Controls.Add(btnNext);
            Controls.Add(panel1);
            Controls.Add(btnSubmit);
            Controls.Add(list);
            Controls.Add(lblTask);
            MinimumSize = new Size(790, 500);
            Name = "FormRavenUpdater";
            Text = "Raven Colonial Updater (Experimental)";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblTask;
        private ListView list;
        private Button btnSubmit;
        private ColumnHeader colName;
        private ColumnHeader colBody;
        private ColumnHeader colType;
        private ColumnHeader colBuildType;
        private Panel panel1;
        private ThemedComboBox comboBody;
        private TextBox txtName;
        private Label label1;
        private ThemedComboBox comboBuildSubType;
        private ThemedComboBox comboBuildType;
        private ColumnHeader colStatus;
        private ThemedComboBox comboStatus;
        private Button btnNext;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolLinkRC;
        private Label lblPhase;
        private ToolStripStatusLabel toolLinkWiki;
        private ToolStripStatusLabel toolFiller;
        private Label label5;
        private Label label4;
        private Label label3;
        private Button btnReload;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button btnRemove;
    }
}
