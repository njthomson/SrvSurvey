namespace SrvSurvey.forms
{
    partial class FormNewProject
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
            label6 = new Label();
            txtArchitect = new TextBox();
            txtNotes = new TextBox();
            label7 = new Label();
            txtName = new TextBox();
            label5 = new Label();
            comboBuildType = new ComboBox();
            label2 = new Label();
            btnCreate = new FlatButton();
            btnCancel = new FlatButton();
            comboBuildSubType = new ComboBox();
            comboSystemSite = new ComboBox();
            label1 = new Label();
            radioOrbital = new RadioButton();
            radioSurface = new RadioButton();
            label3 = new Label();
            SuspendLayout();
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(52, 151);
            label6.Name = "label6";
            label6.Size = new Size(58, 15);
            label6.TabIndex = 10;
            label6.Text = "Architect:";
            // 
            // txtArchitect
            // 
            txtArchitect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtArchitect.Location = new Point(119, 148);
            txtArchitect.Name = "txtArchitect";
            txtArchitect.Size = new Size(384, 23);
            txtArchitect.TabIndex = 11;
            // 
            // txtNotes
            // 
            txtNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNotes.Location = new Point(119, 178);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(384, 127);
            txtNotes.TabIndex = 13;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(69, 181);
            label7.Name = "label7";
            label7.Size = new Size(41, 15);
            label7.TabIndex = 12;
            label7.Text = "Notes:";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(119, 119);
            txtName.Name = "txtName";
            txtName.Size = new Size(384, 23);
            txtName.TabIndex = 9;
            txtName.Text = "primary-port";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(33, 122);
            label5.Name = "label5";
            label5.Size = new Size(80, 15);
            label5.TabIndex = 8;
            label5.Text = "Project name:";
            // 
            // comboBuildType
            // 
            comboBuildType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBuildType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildType.Enabled = false;
            comboBuildType.FormattingEnabled = true;
            comboBuildType.Location = new Point(119, 65);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(242, 23);
            comboBuildType.TabIndex = 4;
            comboBuildType.SelectedIndexChanged += comboBuildType_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 69);
            label2.Name = "label2";
            label2.Size = new Size(105, 15);
            label2.TabIndex = 3;
            label2.Text = "Construction type:";
            // 
            // btnCreate
            // 
            btnCreate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCreate.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCreate.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCreate.Location = new Point(347, 318);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 14;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCancel.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCancel.Location = new Point(428, 318);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // comboBuildSubType
            // 
            comboBuildSubType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBuildSubType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildSubType.Enabled = false;
            comboBuildSubType.FormattingEnabled = true;
            comboBuildSubType.Location = new Point(367, 65);
            comboBuildSubType.Name = "comboBuildSubType";
            comboBuildSubType.Size = new Size(136, 23);
            comboBuildSubType.TabIndex = 5;
            // 
            // comboSystemSite
            // 
            comboSystemSite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboSystemSite.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSystemSite.Enabled = false;
            comboSystemSite.FormattingEnabled = true;
            comboSystemSite.Location = new Point(119, 36);
            comboSystemSite.Name = "comboSystemSite";
            comboSystemSite.Size = new Size(384, 23);
            comboSystemSite.TabIndex = 2;
            comboSystemSite.SelectedIndexChanged += comboSystemSite_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 39);
            label1.Name = "label1";
            label1.Size = new Size(74, 15);
            label1.TabIndex = 1;
            label1.Text = "Planned site:";
            // 
            // radioOrbital
            // 
            radioOrbital.AutoSize = true;
            radioOrbital.Location = new Point(119, 94);
            radioOrbital.Name = "radioOrbital";
            radioOrbital.Size = new Size(61, 19);
            radioOrbital.TabIndex = 6;
            radioOrbital.TabStop = true;
            radioOrbital.Text = "Orbital";
            radioOrbital.UseVisualStyleBackColor = true;
            radioOrbital.CheckedChanged += radioOrbital_CheckedChanged;
            // 
            // radioSurface
            // 
            radioSurface.AutoSize = true;
            radioSurface.Location = new Point(202, 94);
            radioSurface.Name = "radioSurface";
            radioSurface.Size = new Size(64, 19);
            radioSurface.TabIndex = 7;
            radioSurface.TabStop = true;
            radioSurface.Text = "Surface";
            radioSurface.UseVisualStyleBackColor = true;
            radioSurface.CheckedChanged += radioOrbital_CheckedChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(431, 15);
            label3.TabIndex = 0;
            label3.Text = "Choose a planned site from Raven Colonial, or enter build details for this project.";
            // 
            // FormNewProject
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(517, 355);
            Controls.Add(label3);
            Controls.Add(radioSurface);
            Controls.Add(radioOrbital);
            Controls.Add(label1);
            Controls.Add(comboSystemSite);
            Controls.Add(comboBuildSubType);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(comboBuildType);
            Controls.Add(label2);
            Controls.Add(label6);
            Controls.Add(txtArchitect);
            Controls.Add(txtNotes);
            Controls.Add(label7);
            Controls.Add(txtName);
            Controls.Add(label5);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormNewProject";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "comboBuildType";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label6;
        private TextBox txtArchitect;
        private TextBox txtNotes;
        private Label label7;
        private TextBox txtName;
        private Label label5;
        private ComboBox comboBuildType;
        private Label label2;
        private FlatButton btnCreate;
        private FlatButton btnCancel;
        private ComboBox comboBuildSubType;
        private ComboBox comboSystemSite;
        private Label label1;
        private RadioButton radioOrbital;
        private RadioButton radioSurface;
        private Label label3;
    }
}