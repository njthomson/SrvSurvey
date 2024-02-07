namespace SrvSurvey
{
    partial class FormErrorSubmit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormErrorSubmit));
            linkMain = new LinkLabel();
            label2 = new Label();
            txtStack = new TextBox();
            txtSteps = new TextBox();
            btnSubmit = new Button();
            checkIncludeLogs = new CheckBox();
            btnLogs = new Button();
            btnClose = new Button();
            linkToDiscord = new LinkLabel();
            linkJournal = new LinkLabel();
            btnCopyJournalPath = new Button();
            btnCopyStack = new Button();
            SuspendLayout();
            // 
            // linkMain
            // 
            linkMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            linkMain.LinkArea = new LinkArea(90, 45);
            linkMain.Location = new Point(13, 9);
            linkMain.Margin = new Padding(4, 0, 4, 0);
            linkMain.Name = "linkMain";
            linkMain.Size = new Size(488, 29);
            linkMain.TabIndex = 6;
            linkMain.TabStop = true;
            linkMain.Text = "Oops! This is embarrasing. Please submit this error report, or enter details manually at: https://github.com/njthomson/SrvSurvey/issues";
            linkMain.UseCompatibleTextRendering = true;
            linkMain.LinkClicked += linkMain_LinkClicked;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(13, 191);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(264, 12);
            label2.TabIndex = 8;
            label2.Text = "Share what was happening just before:";
            // 
            // txtStack
            // 
            txtStack.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtStack.Font = new Font("Lucida Sans Typewriter", 6F, FontStyle.Regular, GraphicsUnit.Point);
            txtStack.Location = new Point(14, 41);
            txtStack.Margin = new Padding(4, 3, 4, 3);
            txtStack.Multiline = true;
            txtStack.Name = "txtStack";
            txtStack.ReadOnly = true;
            txtStack.ScrollBars = ScrollBars.Both;
            txtStack.Size = new Size(487, 141);
            txtStack.TabIndex = 7;
            // 
            // txtSteps
            // 
            txtSteps.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSteps.Location = new Point(14, 206);
            txtSteps.Margin = new Padding(4, 3, 4, 3);
            txtSteps.Multiline = true;
            txtSteps.Name = "txtSteps";
            txtSteps.ScrollBars = ScrollBars.Both;
            txtSteps.Size = new Size(487, 149);
            txtSteps.TabIndex = 0;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmit.Location = new Point(157, 443);
            btnSubmit.Margin = new Padding(4, 3, 4, 3);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(224, 23);
            btnSubmit.TabIndex = 3;
            btnSubmit.Text = "&Create issue on GitHub.com";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // checkIncludeLogs
            // 
            checkIncludeLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkIncludeLogs.AutoSize = true;
            checkIncludeLogs.Location = new Point(14, 361);
            checkIncludeLogs.Name = "checkIncludeLogs";
            checkIncludeLogs.Size = new Size(164, 16);
            checkIncludeLogs.TabIndex = 1;
            checkIncludeLogs.Text = "Include verbose logs";
            checkIncludeLogs.UseVisualStyleBackColor = true;
            // 
            // btnLogs
            // 
            btnLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogs.Location = new Point(14, 443);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(110, 23);
            btnLogs.TabIndex = 2;
            btnLogs.Text = "View logs";
            btnLogs.UseVisualStyleBackColor = true;
            btnLogs.Click += btnLogs_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(389, 443);
            btnClose.Margin = new Padding(4, 3, 4, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(112, 23);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // linkToDiscord
            // 
            linkToDiscord.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            linkToDiscord.LinkArea = new LinkArea(108, 14);
            linkToDiscord.Location = new Point(14, 401);
            linkToDiscord.Margin = new Padding(4, 0, 4, 0);
            linkToDiscord.Name = "linkToDiscord";
            linkToDiscord.Size = new Size(488, 29);
            linkToDiscord.TabIndex = 10;
            linkToDiscord.TabStop = true;
            linkToDiscord.Text = "(Submitting on GitHub.com requires an account. Issues may also be reported on our Discord server in channel #error-reports)";
            linkToDiscord.UseCompatibleTextRendering = true;
            linkToDiscord.LinkClicked += linkDiscord_LinkClicked;
            // 
            // linkJournal
            // 
            linkJournal.AutoSize = true;
            linkJournal.LinkArea = new LinkArea(22, 200);
            linkJournal.Location = new Point(14, 380);
            linkJournal.Margin = new Padding(4, 0, 4, 0);
            linkJournal.Name = "linkJournal";
            linkJournal.Size = new Size(373, 18);
            linkJournal.TabIndex = 11;
            linkJournal.TabStop = true;
            linkJournal.Text = "Current journal file: Journal.0000-00-00T000000.00.log";
            linkJournal.UseCompatibleTextRendering = true;
            linkJournal.LinkClicked += linkJournal_LinkClicked;
            // 
            // btnCopyJournalPath
            // 
            btnCopyJournalPath.Image = (Image)resources.GetObject("btnCopyJournalPath.Image");
            btnCopyJournalPath.Location = new Point(394, 375);
            btnCopyJournalPath.Name = "btnCopyJournalPath";
            btnCopyJournalPath.Size = new Size(23, 23);
            btnCopyJournalPath.TabIndex = 27;
            btnCopyJournalPath.UseVisualStyleBackColor = true;
            btnCopyJournalPath.Click += btnCopyJournalPath_Click;
            // 
            // btnCopyStack
            // 
            btnCopyStack.Image = (Image)resources.GetObject("btnCopyStack.Image");
            btnCopyStack.Location = new Point(457, 43);
            btnCopyStack.Name = "btnCopyStack";
            btnCopyStack.Size = new Size(23, 23);
            btnCopyStack.TabIndex = 28;
            btnCopyStack.UseVisualStyleBackColor = true;
            btnCopyStack.Click += btnCopyStack_Click;
            // 
            // FormErrorSubmit
            // 
            AcceptButton = btnSubmit;
            AutoScaleDimensions = new SizeF(7F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(514, 478);
            Controls.Add(btnCopyStack);
            Controls.Add(btnCopyJournalPath);
            Controls.Add(linkJournal);
            Controls.Add(linkToDiscord);
            Controls.Add(btnClose);
            Controls.Add(btnLogs);
            Controls.Add(checkIncludeLogs);
            Controls.Add(btnSubmit);
            Controls.Add(txtSteps);
            Controls.Add(txtStack);
            Controls.Add(label2);
            Controls.Add(linkMain);
            Font = new Font("Lucida Sans Typewriter", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(503, 435);
            Name = "FormErrorSubmit";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Oops...";
            Load += ErrorSubmit_Load;
            KeyDown += FormErrorSubmit_KeyDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private LinkLabel linkMain;
        private Label label2;
        private TextBox txtStack;
        private TextBox txtSteps;
        private Button btnSubmit;
        private CheckBox checkIncludeLogs;
        private Button btnLogs;
        private Button btnClose;
        private LinkLabel linkToDiscord;
        private LinkLabel linkJournal;
        private Button btnCopyJournalPath;
        private Button btnCopyStack;
    }
}