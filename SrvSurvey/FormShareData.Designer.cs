namespace SrvSurvey
{
    partial class FormShareData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormShareData));
            btnClose = new Button();
            label1 = new Label();
            list = new ListBox();
            label2 = new Label();
            linkDiscordChannel = new LinkLabel();
            btnCopyZipFile = new Button();
            btnViewZipFile = new Button();
            txtZipFile = new TextBox();
            btnOpenDiscord = new Button();
            button1 = new Button();
            SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(447, 261);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 9;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(332, 15);
            label1.TabIndex = 0;
            label1.Text = "Congratulations on discovering new data for X Guardian sites:";
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            list.FormattingEnabled = true;
            list.ItemHeight = 15;
            list.Location = new Point(12, 27);
            list.Name = "list";
            list.Size = new Size(510, 109);
            list.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 139);
            label2.Name = "label2";
            label2.Size = new Size(168, 15);
            label2.TabIndex = 2;
            label2.Text = "These have been bundled into:";
            // 
            // linkDiscordChannel
            // 
            linkDiscordChannel.AutoSize = true;
            linkDiscordChannel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            linkDiscordChannel.LinkArea = new LinkArea(41, 14);
            linkDiscordChannel.Location = new Point(12, 228);
            linkDiscordChannel.Name = "linkDiscordChannel";
            linkDiscordChannel.Size = new Size(355, 21);
            linkDiscordChannel.TabIndex = 7;
            linkDiscordChannel.TabStop = true;
            linkDiscordChannel.Text = "Please join our Discord and share in the submit-surveys channel.";
            linkDiscordChannel.UseCompatibleTextRendering = true;
            linkDiscordChannel.LinkClicked += linkDiscordChannel_LinkClicked;
            // 
            // btnCopyZipFile
            // 
            btnCopyZipFile.Location = new Point(118, 186);
            btnCopyZipFile.Name = "btnCopyZipFile";
            btnCopyZipFile.Size = new Size(221, 23);
            btnCopyZipFile.TabIndex = 5;
            btnCopyZipFile.Text = "Copy zip file location to clipboard";
            btnCopyZipFile.UseVisualStyleBackColor = true;
            btnCopyZipFile.Click += btnCopyZipFile_Click;
            // 
            // btnViewZipFile
            // 
            btnViewZipFile.Location = new Point(12, 186);
            btnViewZipFile.Name = "btnViewZipFile";
            btnViewZipFile.Size = new Size(96, 23);
            btnViewZipFile.TabIndex = 4;
            btnViewZipFile.Text = "View zip file";
            btnViewZipFile.UseVisualStyleBackColor = true;
            btnViewZipFile.Click += btnViewZipFile_Click;
            // 
            // txtZipFile
            // 
            txtZipFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtZipFile.Location = new Point(12, 157);
            txtZipFile.Name = "txtZipFile";
            txtZipFile.ReadOnly = true;
            txtZipFile.Size = new Size(510, 23);
            txtZipFile.TabIndex = 3;
            txtZipFile.Text = "C:\\Users\\grinn\\AppData\\Roaming\\SrvSurvey\\SrvSurvey\\1.1.0.0.share-F123456.zip";
            // 
            // btnOpenDiscord
            // 
            btnOpenDiscord.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOpenDiscord.Location = new Point(283, 261);
            btnOpenDiscord.Name = "btnOpenDiscord";
            btnOpenDiscord.Size = new Size(158, 23);
            btnOpenDiscord.TabIndex = 8;
            btnOpenDiscord.Text = "Open Discord channel";
            btnOpenDiscord.UseVisualStyleBackColor = true;
            btnOpenDiscord.Click += btnOpenDiscord_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(345, 186);
            button1.Name = "button1";
            button1.Size = new Size(177, 23);
            button1.TabIndex = 6;
            button1.Text = "Copy zip file to clipboard";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // FormShareData
            // 
            AcceptButton = btnOpenDiscord;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(534, 296);
            Controls.Add(button1);
            Controls.Add(btnOpenDiscord);
            Controls.Add(txtZipFile);
            Controls.Add(btnViewZipFile);
            Controls.Add(btnCopyZipFile);
            Controls.Add(linkDiscordChannel);
            Controls.Add(label2);
            Controls.Add(list);
            Controls.Add(label1);
            Controls.Add(btnClose);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormShareData";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Share new data";
            FormClosed += FormShareData_FormClosed;
            Load += FormShareData_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnClose;
        private Label label1;
        private ListBox list;
        private Label label2;
        private LinkLabel linkDiscordChannel;
        private Button btnCopyZipFile;
        private Button btnViewZipFile;
        private TextBox txtZipFile;
        private Button btnOpenDiscord;
        private Button button1;
    }
}