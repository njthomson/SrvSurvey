namespace SrvSurvey.forms
{
    partial class FormApiKeyRCC
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
            txtRavenApiKey = new TextBox2();
            txtRavenCmdr = new TextBox();
            label31 = new Label();
            label30 = new Label();
            linkLabel3 = new LinkLabel();
            btnSave = new DrawButton();
            btnCancel = new DrawButton();
            label1 = new Label();
            SuspendLayout();
            // 
            // txtRavenApiKey
            // 
            txtRavenApiKey.BackColor = SystemColors.Window;
            txtRavenApiKey.BorderColor = SystemColors.ActiveBorder;
            txtRavenApiKey.BorderStyle = BorderStyle.FixedSingle;
            txtRavenApiKey.ForeColor = SystemColors.WindowText;
            txtRavenApiKey.Location = new Point(100, 81);
            txtRavenApiKey.Multiline = false;
            txtRavenApiKey.Name = "txtRavenApiKey";
            txtRavenApiKey.Padding = new Padding(3);
            txtRavenApiKey.PasswordChar = '\0';
            txtRavenApiKey.ScrollBars = ScrollBars.None;
            txtRavenApiKey.SelectionStart = 0;
            txtRavenApiKey.Size = new Size(422, 23);
            txtRavenApiKey.TabIndex = 1;
            txtRavenApiKey.UseEdgeButton = TextBox2.EdgeButton.Paste;
            txtRavenApiKey.TextChanged2 += txtRavenApiKey_TextChanged2;
            // 
            // txtRavenCmdr
            // 
            txtRavenCmdr.Location = new Point(100, 52);
            txtRavenCmdr.Name = "txtRavenCmdr";
            txtRavenCmdr.ReadOnly = true;
            txtRavenCmdr.Size = new Size(422, 23);
            txtRavenCmdr.TabIndex = 7;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new Point(17, 55);
            label31.Name = "label31";
            label31.Size = new Size(77, 15);
            label31.TabIndex = 6;
            label31.Text = "Commander:";
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new Point(44, 84);
            label30.Name = "label30";
            label30.Size = new Size(50, 15);
            label30.TabIndex = 0;
            label30.Text = "API Key:";
            // 
            // linkLabel3
            // 
            linkLabel3.BackColor = Color.Transparent;
            linkLabel3.LinkArea = new LinkArea(21, 100);
            linkLabel3.Location = new Point(17, 107);
            linkLabel3.Name = "linkLabel3";
            linkLabel3.Size = new Size(505, 21);
            linkLabel3.TabIndex = 2;
            linkLabel3.TabStop = true;
            linkLabel3.Text = "Get your API key at: https://ravencolonial.com/user";
            linkLabel3.TextAlign = ContentAlignment.TopRight;
            linkLabel3.UseCompatibleTextRendering = true;
            linkLabel3.LinkClicked += linkLabel3_LinkClicked;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.AnimateOnPress = false;
            btnSave.BackColorDisabled = Color.Empty;
            btnSave.BackColorHover = Color.Empty;
            btnSave.BackColorPressed = Color.Empty;
            btnSave.DrawBorder = true;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.ForeColor = Color.Black;
            btnSave.ForeColorDisabled = Color.Empty;
            btnSave.ForeColorHover = Color.Empty;
            btnSave.ForeColorPressed = Color.Empty;
            btnSave.Location = new Point(339, 144);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(88, 27);
            btnSave.TabIndex = 3;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.AnimateOnPress = false;
            btnCancel.BackColorDisabled = Color.Empty;
            btnCancel.BackColorHover = Color.Empty;
            btnCancel.BackColorPressed = Color.Empty;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.DrawBorder = true;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.Black;
            btnCancel.ForeColorDisabled = Color.Empty;
            btnCancel.ForeColorHover = Color.Empty;
            btnCancel.ForeColorPressed = Color.Empty;
            btnCancel.Location = new Point(433, 144);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(403, 30);
            label1.TabIndex = 5;
            label1.Text = "Set your API key for Raven Colonial to authorize operations from SrvSurvey.\r\nThis should match the Commander you are currently playing as.";
            // 
            // FormRCC
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(534, 183);
            Controls.Add(label1);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            Controls.Add(txtRavenApiKey);
            Controls.Add(txtRavenCmdr);
            Controls.Add(label31);
            Controls.Add(linkLabel3);
            Controls.Add(label30);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormRCC";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Raven Colonial API Key";
            Activated += FormRCC_Activated;
            Deactivate += FormRCC_Deactivate;
            Load += FormRCC_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox2 txtRavenApiKey;
        private TextBox txtRavenCmdr;
        private Label label31;
        private Label label30;
        private LinkLabel linkLabel3;
        private DrawButton btnSave;
        private DrawButton btnCancel;
        private Label label1;
    }
}