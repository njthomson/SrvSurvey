namespace SrvSurvey.forms
{
    partial class FormJourneyEdit
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
            btnReject = new FlatButton();
            btnAccept = new FlatButton();
            txtName = new TextBox();
            lblName = new Label();
            label1 = new Label();
            txtDescription = new TextBox();
            SuspendLayout();
            // 
            // btnReject
            // 
            btnReject.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReject.Location = new Point(472, 200);
            btnReject.Name = "btnReject";
            btnReject.Size = new Size(75, 23);
            btnReject.TabIndex = 15;
            btnReject.Text = "Cancel";
            btnReject.UseVisualStyleBackColor = true;
            btnReject.Click += btnReject_Click;
            // 
            // btnAccept
            // 
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAccept.Location = new Point(391, 200);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(75, 23);
            btnAccept.TabIndex = 14;
            btnAccept.Text = "Start";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(92, 12);
            txtName.Name = "txtName";
            txtName.Size = new Size(451, 23);
            txtName.TabIndex = 13;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(12, 15);
            lblName.Name = "lblName";
            lblName.Size = new Size(42, 15);
            lblName.TabIndex = 12;
            lblName.Text = "Name:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 44);
            label1.Name = "label1";
            label1.Size = new Size(70, 15);
            label1.TabIndex = 20;
            label1.Text = "Description:";
            // 
            // txtDescription
            // 
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtDescription.Location = new Point(92, 41);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(451, 141);
            txtDescription.TabIndex = 21;
            // 
            // FormJourneyEdit
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(559, 235);
            Controls.Add(txtDescription);
            Controls.Add(label1);
            Controls.Add(btnReject);
            Controls.Add(btnAccept);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Name = "FormJourneyEdit";
            Text = "Edit Journey";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private FlatButton btnReject;
        private FlatButton btnAccept;
        private TextBox txtName;
        private Label lblName;
        private Label label1;
        private TextBox txtDescription;
    }
}