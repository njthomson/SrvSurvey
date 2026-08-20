namespace SrvSurvey.forms
{
    partial class FormInaraIntegration
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            labelMessage = new Label();
            checkUpload = new CheckBox2();
            labelCommander = new Label();
            txtCommander = new TextBox();
            labelApiKey = new Label();
            txtApiKey = new TextBox2();
            linkApiKey = new LinkLabel();
            linkTerms = new LinkLabel();
            btnSave = new DrawButton();
            btnCancel = new DrawButton();
            SuspendLayout();
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Location = new Point(12, 12);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(465, 15);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "Enter your Inara API key and choose whether to share commander data with Inara.";
            // 
            // checkUpload
            // 
            checkUpload.AutoSize = true;
            checkUpload.CheckAlign = ContentAlignment.TopLeft;
            checkUpload.CheckColor = SystemColors.ControlText;
            checkUpload.LineColor = SystemColors.ActiveBorder;
            checkUpload.Location = new Point(18, 42);
            checkUpload.Name = "checkUpload";
            checkUpload.Size = new Size(269, 19);
            checkUpload.TabIndex = 1;
            checkUpload.Text = "Upload supported commander data to Inara";
            checkUpload.TextAlign = ContentAlignment.TopLeft;
            checkUpload.UseVisualStyleBackColor = true;
            checkUpload.CheckedChanged += checkUpload_CheckedChanged;
            // 
            // labelCommander
            // 
            labelCommander.AutoSize = true;
            labelCommander.Location = new Point(17, 76);
            labelCommander.Name = "labelCommander";
            labelCommander.Size = new Size(77, 15);
            labelCommander.TabIndex = 2;
            labelCommander.Text = "Commander:";
            // 
            // txtCommander
            // 
            txtCommander.Location = new Point(100, 72);
            txtCommander.Name = "txtCommander";
            txtCommander.Size = new Size(498, 23);
            txtCommander.TabIndex = 3;
            // 
            // labelApiKey
            // 
            labelApiKey.AutoSize = true;
            labelApiKey.Location = new Point(44, 106);
            labelApiKey.Name = "labelApiKey";
            labelApiKey.Size = new Size(50, 15);
            labelApiKey.TabIndex = 4;
            labelApiKey.Text = "API Key:";
            // 
            // txtApiKey
            // 
            txtApiKey.BackColor = SystemColors.Window;
            txtApiKey.BorderColor = SystemColors.ActiveBorder;
            txtApiKey.BorderStyle = BorderStyle.FixedSingle;
            txtApiKey.ForeColor = SystemColors.WindowText;
            txtApiKey.Location = new Point(100, 102);
            txtApiKey.Multiline = false;
            txtApiKey.Name = "txtApiKey";
            txtApiKey.Padding = new Padding(3);
            txtApiKey.PasswordChar = '*';
            txtApiKey.ScrollBars = ScrollBars.None;
            txtApiKey.SelectionStart = 0;
            txtApiKey.Size = new Size(498, 23);
            txtApiKey.TabIndex = 5;
            txtApiKey.UseEdgeButton = TextBox2.EdgeButton.Paste;
            // 
            // linkApiKey
            // 
            linkApiKey.BackColor = Color.Transparent;
            linkApiKey.LinkArea = new LinkArea(21, 41);
            linkApiKey.Location = new Point(100, 132);
            linkApiKey.Name = "linkApiKey";
            linkApiKey.Size = new Size(498, 21);
            linkApiKey.TabIndex = 6;
            linkApiKey.TabStop = true;
            linkApiKey.Text = "Get your API key at: https://inara.cz/elite/cmdr-settings-api/";
            linkApiKey.TextAlign = ContentAlignment.TopRight;
            linkApiKey.UseCompatibleTextRendering = true;
            linkApiKey.LinkClicked += linkApiKey_LinkClicked;
            // 
            // linkTerms
            // 
            linkTerms.AutoSize = true;
            linkTerms.BackColor = Color.Transparent;
            linkTerms.Location = new Point(18, 176);
            linkTerms.Name = "linkTerms";
            linkTerms.Size = new Size(151, 15);
            linkTerms.TabIndex = 7;
            linkTerms.TabStop = true;
            linkTerms.Text = "Read Inara Terms of Service";
            linkTerms.LinkClicked += linkTerms_LinkClicked;
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
            btnSave.Location = new Point(415, 170);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(88, 27);
            btnSave.TabIndex = 8;
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
            btnCancel.Location = new Point(509, 170);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FormInaraIntegration
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(614, 210);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(linkTerms);
            Controls.Add(linkApiKey);
            Controls.Add(txtApiKey);
            Controls.Add(labelApiKey);
            Controls.Add(txtCommander);
            Controls.Add(labelCommander);
            Controls.Add(checkUpload);
            Controls.Add(labelMessage);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormInaraIntegration";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Inara Integration";
            Load += FormInaraIntegration_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelMessage;
        private CheckBox2 checkUpload;
        private Label labelCommander;
        private TextBox txtCommander;
        private Label labelApiKey;
        private TextBox2 txtApiKey;
        private LinkLabel linkApiKey;
        private LinkLabel linkTerms;
        private DrawButton btnSave;
        private DrawButton btnCancel;
    }
}
