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
            btnSubmit = new FlatButton();
            checkIncludeLogs = new CheckBox();
            btnLogs = new FlatButton();
            btnClose = new FlatButton();
            linkToDiscord = new LinkLabel();
            linkJournal = new LinkLabel();
            btnCopyJournalPath = new FlatButton();
            btnCopyStack = new FlatButton();
            SuspendLayout();
            // 
            // linkMain
            // 
            linkMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            linkMain.LinkArea = new LinkArea(90, 45);
            linkMain.Location = new Point(13, 12);
            linkMain.Margin = new Padding(4, 0, 4, 0);
            linkMain.Name = "linkMain";
            linkMain.Size = new Size(505, 40);
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
            label2.Location = new Point(15, 241);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(234, 17);
            label2.TabIndex = 8;
            label2.Text = "Share what was happening just before:";
            // 
            // txtStack
            // 
            txtStack.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtStack.Font = new Font("Lucida Sans Typewriter", 6F);
            txtStack.Location = new Point(15, 57);
            txtStack.Margin = new Padding(4, 5, 4, 5);
            txtStack.Multiline = true;
            txtStack.Name = "txtStack";
            txtStack.ReadOnly = true;
            txtStack.ScrollBars = ScrollBars.Both;
            txtStack.Size = new Size(533, 179);
            txtStack.TabIndex = 7;
            // 
            // txtSteps
            // 
            txtSteps.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSteps.Location = new Point(14, 263);
            txtSteps.Margin = new Padding(4, 5, 4, 5);
            txtSteps.Multiline = true;
            txtSteps.Name = "txtSteps";
            txtSteps.ScrollBars = ScrollBars.Both;
            txtSteps.Size = new Size(533, 209);
            txtSteps.TabIndex = 0;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmit.AutoSize = true;
            btnSubmit.Location = new Point(204, 593);
            btnSubmit.Margin = new Padding(4, 5, 4, 5);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(224, 34);
            btnSubmit.TabIndex = 3;
            btnSubmit.Text = "&Create issue on GitHub.com";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // checkIncludeLogs
            // 
            checkIncludeLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkIncludeLogs.AutoSize = true;
            checkIncludeLogs.Location = new Point(15, 480);
            checkIncludeLogs.Margin = new Padding(3, 5, 3, 5);
            checkIncludeLogs.Name = "checkIncludeLogs";
            checkIncludeLogs.Size = new Size(148, 21);
            checkIncludeLogs.TabIndex = 1;
            checkIncludeLogs.Text = "Include verbose logs";
            checkIncludeLogs.UseVisualStyleBackColor = true;
            // 
            // btnLogs
            // 
            btnLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogs.AutoSize = true;
            btnLogs.Location = new Point(15, 593);
            btnLogs.Margin = new Padding(3, 5, 3, 5);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(110, 34);
            btnLogs.TabIndex = 2;
            btnLogs.Text = "View logs";
            btnLogs.UseVisualStyleBackColor = true;
            btnLogs.Click += btnLogs_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.AutoSize = true;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(436, 593);
            btnClose.Margin = new Padding(4, 5, 4, 5);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(112, 34);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // linkToDiscord
            // 
            linkToDiscord.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            linkToDiscord.LinkArea = new LinkArea(108, 14);
            linkToDiscord.Location = new Point(15, 535);
            linkToDiscord.Margin = new Padding(4, 0, 4, 0);
            linkToDiscord.Name = "linkToDiscord";
            linkToDiscord.Size = new Size(534, 41);
            linkToDiscord.TabIndex = 10;
            linkToDiscord.TabStop = true;
            linkToDiscord.Text = "(Submitting on GitHub.com requires an account. Issues may also be reported on our Discord server in channel #error-reports)";
            linkToDiscord.UseCompatibleTextRendering = true;
            linkToDiscord.LinkClicked += linkDiscord_LinkClicked;
            // 
            // linkJournal
            // 
            linkJournal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkJournal.AutoSize = true;
            linkJournal.LinkArea = new LinkArea(22, 200);
            linkJournal.Location = new Point(15, 506);
            linkJournal.Margin = new Padding(4, 0, 4, 0);
            linkJournal.Name = "linkJournal";
            linkJournal.Size = new Size(325, 22);
            linkJournal.TabIndex = 11;
            linkJournal.TabStop = true;
            linkJournal.Text = "Current journal file: Journal.0000-00-00T000000.00.log";
            linkJournal.UseCompatibleTextRendering = true;
            linkJournal.LinkClicked += linkJournal_LinkClicked;
            // 
            // btnCopyJournalPath
            // 
            btnCopyJournalPath.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCopyJournalPath.Image = (Image)resources.GetObject("btnCopyJournalPath.Image");
            btnCopyJournalPath.Location = new Point(347, 504);
            btnCopyJournalPath.Margin = new Padding(3, 5, 3, 5);
            btnCopyJournalPath.Name = "btnCopyJournalPath";
            btnCopyJournalPath.Size = new Size(23, 26);
            btnCopyJournalPath.TabIndex = 27;
            btnCopyJournalPath.UseVisualStyleBackColor = true;
            btnCopyJournalPath.Click += btnCopyJournalPath_Click;
            // 
            // btnCopyStack
            // 
            btnCopyStack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyStack.Image = (Image)resources.GetObject("btnCopyStack.Image");
            btnCopyStack.Location = new Point(524, 26);
            btnCopyStack.Margin = new Padding(3, 5, 3, 5);
            btnCopyStack.Name = "btnCopyStack";
            btnCopyStack.Size = new Size(23, 26);
            btnCopyStack.TabIndex = 28;
            btnCopyStack.UseVisualStyleBackColor = true;
            btnCopyStack.Click += btnCopyStack_Click;
            // 
            // FormErrorSubmit
            // 
            AcceptButton = btnSubmit;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 637);
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
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(503, 600);
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
        private FlatButton btnSubmit;
        private CheckBox checkIncludeLogs;
        private FlatButton btnLogs;
        private FlatButton btnClose;
        private LinkLabel linkToDiscord;
        private LinkLabel linkJournal;
        private FlatButton btnCopyJournalPath;
        private FlatButton btnCopyStack;
    }
}