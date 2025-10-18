namespace SrvSurvey.forms
{
    partial class FormMyProjects
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
            list = new ListView();
            colProject = new ColumnHeader();
            colProgress = new ColumnHeader();
            colType = new ColumnHeader();
            colSystem = new ColumnHeader();
            btnClose = new FlatButton();
            btnSave = new FlatButton();
            btnRefresh = new FlatButton();
            label1 = new Label();
            linkLabel1 = new LinkLabel();
            lblTotals = new Label();
            SuspendLayout();
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colProject, colProgress, colType, colSystem });
            list.Location = new Point(12, 35);
            list.Name = "list";
            list.Size = new Size(790, 225);
            list.TabIndex = 0;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ItemChecked += list_ItemChecked;
            // 
            // colProject
            // 
            colProject.Text = "Project";
            colProject.Width = 200;
            // 
            // colProgress
            // 
            colProgress.Text = "Progress";
            colProgress.Width = 80;
            // 
            // colType
            // 
            colType.Text = "Type";
            colType.Width = 260;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 220;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(727, 266);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 1;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(646, 266);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 2;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRefresh.Location = new Point(565, 266);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "&Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(619, 15);
            label1.TabIndex = 4;
            label1.Text = "Select your preferred projects to show in the shopping overlay. These can also be changed on the page linked below.";
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(371, 270);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(177, 15);
            linkLabel1.TabIndex = 5;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "https://ravencolonial.com/build";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // lblTotals
            // 
            lblTotals.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTotals.AutoSize = true;
            lblTotals.Location = new Point(12, 274);
            lblTotals.Name = "lblTotals";
            lblTotals.Size = new Size(22, 15);
            lblTotals.TabIndex = 6;
            lblTotals.Text = "---";
            // 
            // FormMyProjects
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(814, 301);
            Controls.Add(lblTotals);
            Controls.Add(linkLabel1);
            Controls.Add(label1);
            Controls.Add(btnRefresh);
            Controls.Add(btnSave);
            Controls.Add(btnClose);
            Controls.Add(list);
            MinimumSize = new Size(830, 340);
            Name = "FormMyProjects";
            Text = "My build projects";
            Load += FormMyProjects_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView list;
        private ColumnHeader colProject;
        private ColumnHeader colType;
        private FlatButton btnClose;
        private FlatButton btnSave;
        private FlatButton btnRefresh;
        private ColumnHeader colSystem;
        private Label label1;
        private LinkLabel linkLabel1;
        private ColumnHeader colProgress;
        private Label lblTotals;
    }
}