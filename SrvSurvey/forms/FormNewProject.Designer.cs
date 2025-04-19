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
            SuspendLayout();
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(52, 73);
            label6.Name = "label6";
            label6.Size = new Size(58, 15);
            label6.TabIndex = 40;
            label6.Text = "Architect:";
            // 
            // txtArchitect
            // 
            txtArchitect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtArchitect.Location = new Point(119, 70);
            txtArchitect.Name = "txtArchitect";
            txtArchitect.Size = new Size(384, 23);
            txtArchitect.TabIndex = 41;
            // 
            // txtNotes
            // 
            txtNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNotes.Location = new Point(119, 100);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(384, 127);
            txtNotes.TabIndex = 42;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(69, 103);
            label7.Name = "label7";
            label7.Size = new Size(41, 15);
            label7.TabIndex = 43;
            label7.Text = "Notes:";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(119, 41);
            txtName.Name = "txtName";
            txtName.Size = new Size(384, 23);
            txtName.TabIndex = 39;
            txtName.Text = "primary-port";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(33, 44);
            label5.Name = "label5";
            label5.Size = new Size(80, 15);
            label5.TabIndex = 38;
            label5.Text = "Project name:";
            // 
            // comboBuildType
            // 
            comboBuildType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBuildType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildType.Enabled = false;
            comboBuildType.FormattingEnabled = true;
            comboBuildType.Location = new Point(119, 12);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(384, 23);
            comboBuildType.TabIndex = 45;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 16);
            label2.Name = "label2";
            label2.Size = new Size(105, 15);
            label2.TabIndex = 44;
            label2.Text = "Construction type:";
            // 
            // btnCreate
            // 
            btnCreate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCreate.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCreate.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCreate.Location = new Point(347, 244);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 46;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCancel.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCancel.Location = new Point(428, 244);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 47;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FormNewProject
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(517, 281);
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
            Name = "FormNewProject";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New build project";
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
    }
}