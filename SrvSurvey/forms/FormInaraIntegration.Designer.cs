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
            labelWarning = new Label();
            labelCommander = new Label();
            txtCommander = new TextBox();
            labelApiKey = new Label();
            txtApiKey = new TextBox2();
            linkApiKey = new LinkLabel();
            linkTerms = new LinkLabel();
            btnClear = new DrawButton();
            btnOk = new DrawButton();
            btnCancel = new DrawButton();
            SuspendLayout();
            //
            // labelMessage
            //
            labelMessage.AutoSize = true;
            labelMessage.Location = new Point(12, 12);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(532, 15);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "Saving an API key enables supported Inara uploads only for the commander shown below.";
            //
            // labelWarning
            //
            labelWarning.AutoSize = true;
            labelWarning.Location = new Point(12, 38);
            labelWarning.Name = "labelWarning";
            labelWarning.Size = new Size(487, 15);
            labelWarning.TabIndex = 1;
            labelWarning.Text = "Enable Inara uploads in only one application at a time to avoid duplicate commander events.";
            //
            // labelCommander
            //
            labelCommander.AutoSize = true;
            labelCommander.Location = new Point(17, 73);
            labelCommander.Name = "labelCommander";
            labelCommander.Size = new Size(77, 15);
            labelCommander.TabIndex = 2;
            labelCommander.Text = "Commander:";
            //
            // txtCommander
            //
            txtCommander.Location = new Point(100, 69);
            txtCommander.Name = "txtCommander";
            txtCommander.ReadOnly = true;
            txtCommander.Size = new Size(538, 23);
            txtCommander.TabIndex = 3;
            txtCommander.TabStop = false;
            //
            // labelApiKey
            //
            labelApiKey.AutoSize = true;
            labelApiKey.Location = new Point(44, 107);
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
            txtApiKey.Location = new Point(100, 103);
            txtApiKey.Multiline = false;
            txtApiKey.Name = "txtApiKey";
            txtApiKey.Padding = new Padding(3);
            txtApiKey.PasswordChar = '*';
            txtApiKey.ScrollBars = ScrollBars.None;
            txtApiKey.SelectionStart = 0;
            txtApiKey.Size = new Size(538, 23);
            txtApiKey.TabIndex = 5;
            txtApiKey.UseEdgeButton = TextBox2.EdgeButton.Paste;
            //
            // linkApiKey
            //
            linkApiKey.BackColor = Color.Transparent;
            linkApiKey.LinkArea = new LinkArea(21, 41);
            linkApiKey.Location = new Point(100, 133);
            linkApiKey.Name = "linkApiKey";
            linkApiKey.Size = new Size(538, 21);
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
            linkTerms.Location = new Point(18, 186);
            linkTerms.Name = "linkTerms";
            linkTerms.Size = new Size(151, 15);
            linkTerms.TabIndex = 7;
            linkTerms.TabStop = true;
            linkTerms.Text = "Read Inara Terms of Service";
            linkTerms.LinkClicked += linkTerms_LinkClicked;
            //
            // btnClear
            //
            btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClear.AnimateOnPress = false;
            btnClear.BackColorDisabled = Color.Empty;
            btnClear.BackColorHover = Color.Empty;
            btnClear.BackColorPressed = Color.Empty;
            btnClear.DrawBorder = true;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.ForeColor = Color.Black;
            btnClear.ForeColorDisabled = Color.Empty;
            btnClear.ForeColorHover = Color.Empty;
            btnClear.ForeColorPressed = Color.Empty;
            btnClear.Location = new Point(371, 180);
            btnClear.Margin = new Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(88, 27);
            btnClear.TabIndex = 8;
            btnClear.Text = "C&lear Key";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            //
            // btnOk
            //
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.AnimateOnPress = false;
            btnOk.BackColorDisabled = Color.Empty;
            btnOk.BackColorHover = Color.Empty;
            btnOk.BackColorPressed = Color.Empty;
            btnOk.DrawBorder = true;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.ForeColor = Color.Black;
            btnOk.ForeColorDisabled = Color.Empty;
            btnOk.ForeColorHover = Color.Empty;
            btnOk.ForeColorPressed = Color.Empty;
            btnOk.Location = new Point(465, 180);
            btnOk.Margin = new Padding(4, 3, 4, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(88, 27);
            btnOk.TabIndex = 9;
            btnOk.Text = "&OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
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
            btnCancel.Location = new Point(559, 180);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            //
            // FormInaraIntegration
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(664, 220);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(btnClear);
            Controls.Add(linkTerms);
            Controls.Add(linkApiKey);
            Controls.Add(txtApiKey);
            Controls.Add(labelApiKey);
            Controls.Add(txtCommander);
            Controls.Add(labelCommander);
            Controls.Add(labelWarning);
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
        private Label labelWarning;
        private Label labelCommander;
        private TextBox txtCommander;
        private Label labelApiKey;
        private TextBox2 txtApiKey;
        private LinkLabel linkApiKey;
        private LinkLabel linkTerms;
        private DrawButton btnClear;
        private DrawButton btnOk;
        private DrawButton btnCancel;
    }
}
