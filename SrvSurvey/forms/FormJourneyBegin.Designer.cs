namespace SrvSurvey.forms
{
    partial class FormJourneyBegin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormJourneyBegin));
            lblDesc = new Label();
            lblName = new Label();
            txtName = new TextBox();
            btnAccept = new FlatButton();
            btnReject = new FlatButton();
            radioNow = new RadioButton();
            radioSystem = new RadioButton();
            comboStartFrom = new ComboStarSystem();
            lblWarning = new Label();
            txtCurrentSystem = new TextBox();
            lblLastVisited = new Label();
            SuspendLayout();
            // 
            // lblDesc
            // 
            lblDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblDesc.FlatStyle = FlatStyle.System;
            lblDesc.Location = new Point(12, 9);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(535, 91);
            lblDesc.TabIndex = 0;
            lblDesc.Text = resources.GetString("lblDesc.Text");
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.FlatStyle = FlatStyle.System;
            lblName.Location = new Point(16, 106);
            lblName.Name = "lblName";
            lblName.Size = new Size(42, 15);
            lblName.TabIndex = 1;
            lblName.Text = "Name:";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(96, 103);
            txtName.Name = "txtName";
            txtName.Size = new Size(451, 23);
            txtName.TabIndex = 2;
            txtName.TextChanged += txtName_TextChanged;
            // 
            // btnAccept
            // 
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAccept.Location = new Point(391, 231);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(75, 23);
            btnAccept.TabIndex = 9;
            btnAccept.Text = "Start";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // btnReject
            // 
            btnReject.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReject.Location = new Point(472, 231);
            btnReject.Name = "btnReject";
            btnReject.Size = new Size(75, 23);
            btnReject.TabIndex = 10;
            btnReject.Text = "Cancel";
            btnReject.UseVisualStyleBackColor = true;
            btnReject.Click += btnReject_Click;
            // 
            // radioNow
            // 
            radioNow.AutoSize = true;
            radioNow.FlatStyle = FlatStyle.System;
            radioNow.Location = new Point(96, 132);
            radioNow.Name = "radioNow";
            radioNow.Size = new Size(152, 20);
            radioNow.TabIndex = 3;
            radioNow.Text = "This journey starts now";
            radioNow.UseVisualStyleBackColor = true;
            radioNow.CheckedChanged += radioNow_CheckedChanged;
            // 
            // radioSystem
            // 
            radioSystem.AutoSize = true;
            radioSystem.FlatStyle = FlatStyle.System;
            radioSystem.Location = new Point(96, 157);
            radioSystem.Name = "radioSystem";
            radioSystem.Size = new Size(190, 20);
            radioSystem.TabIndex = 5;
            radioSystem.Text = "This journey started in system:";
            radioSystem.UseVisualStyleBackColor = true;
            radioSystem.CheckedChanged += radioNow_CheckedChanged;
            // 
            // comboStartFrom
            // 
            comboStartFrom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboStartFrom.FlatStyle = FlatStyle.System;
            comboStartFrom.FormattingEnabled = true;
            comboStartFrom.Location = new Point(286, 156);
            comboStartFrom.Name = "comboStartFrom";
            comboStartFrom.SelectedSystem = null;
            comboStartFrom.Size = new Size(261, 23);
            comboStartFrom.TabIndex = 6;
            comboStartFrom.updateOnJump = false;
            comboStartFrom.selectedSystemChanged += comboStartFrom_selectedSystemChanged;
            // 
            // lblWarning
            // 
            lblWarning.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblWarning.FlatStyle = FlatStyle.System;
            lblWarning.Location = new Point(16, 231);
            lblWarning.Name = "lblWarning";
            lblWarning.Size = new Size(369, 23);
            lblWarning.TabIndex = 8;
            lblWarning.Text = "---";
            lblWarning.TextAlign = ContentAlignment.MiddleRight;
            lblWarning.Visible = false;
            // 
            // txtCurrentSystem
            // 
            txtCurrentSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCurrentSystem.Location = new Point(286, 131);
            txtCurrentSystem.Name = "txtCurrentSystem";
            txtCurrentSystem.ReadOnly = true;
            txtCurrentSystem.Size = new Size(261, 23);
            txtCurrentSystem.TabIndex = 4;
            // 
            // lblLastVisited
            // 
            lblLastVisited.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLastVisited.FlatStyle = FlatStyle.System;
            lblLastVisited.Location = new Point(96, 182);
            lblLastVisited.Name = "lblLastVisited";
            lblLastVisited.Size = new Size(451, 22);
            lblLastVisited.TabIndex = 7;
            lblLastVisited.Text = "Last visited XX 2 days ago";
            lblLastVisited.TextAlign = ContentAlignment.MiddleRight;
            lblLastVisited.Visible = false;
            // 
            // FormJourneyBegin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(559, 266);
            Controls.Add(lblLastVisited);
            Controls.Add(txtCurrentSystem);
            Controls.Add(lblWarning);
            Controls.Add(comboStartFrom);
            Controls.Add(radioSystem);
            Controls.Add(radioNow);
            Controls.Add(btnReject);
            Controls.Add(btnAccept);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Controls.Add(lblDesc);
            Name = "FormJourneyBegin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Begin a new journey...";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDesc;
        private Label lblName;
        private TextBox txtName;
        private FlatButton btnAccept;
        private FlatButton btnReject;
        private RadioButton radioNow;
        private RadioButton radioSystem;
        private ComboStarSystem comboStartFrom;
        private Label lblWarning;
        private TextBox txtCurrentSystem;
        private Label lblLastVisited;
    }
}