namespace SrvSurvey.forms
{
    partial class FormPlayDev
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPlayDev));
            txtJson = new TextBox();
            label1 = new Label();
            comboQuest = new ComboBox();
            comboChapter = new ComboBox();
            label2 = new Label();
            btnParse = new Button();
            statusStrip1 = new StatusStrip();
            menuMore = new ToolStripDropDownButton();
            menuImportFolder = new ToolStripMenuItem();
            menuReadFromFile = new ToolStripMenuItem();
            menuWatchJournal = new ToolStripMenuItem();
            menuStatus = new ToolStripStatusLabel();
            checkWatchFolder = new CheckBox();
            btnRefresh = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // txtJson
            // 
            txtJson.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtJson.Location = new Point(93, 64);
            txtJson.Multiline = true;
            txtJson.Name = "txtJson";
            txtJson.ReadOnly = true;
            txtJson.ScrollBars = ScrollBars.Both;
            txtJson.Size = new Size(677, 317);
            txtJson.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(46, 9);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 0;
            label1.Text = "Quest:";
            // 
            // comboQuest
            // 
            comboQuest.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboQuest.DisplayMember = "Value";
            comboQuest.DropDownStyle = ComboBoxStyle.DropDownList;
            comboQuest.FormattingEnabled = true;
            comboQuest.Location = new Point(93, 6);
            comboQuest.Name = "comboQuest";
            comboQuest.Size = new Size(577, 23);
            comboQuest.TabIndex = 1;
            comboQuest.ValueMember = "Key";
            comboQuest.SelectedIndexChanged += comboQuest_SelectedIndexChanged;
            // 
            // comboChapter
            // 
            comboChapter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboChapter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboChapter.FormattingEnabled = true;
            comboChapter.Location = new Point(93, 35);
            comboChapter.Name = "comboChapter";
            comboChapter.Size = new Size(677, 23);
            comboChapter.TabIndex = 4;
            comboChapter.SelectedIndexChanged += comboChapter_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(52, 38);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 3;
            label2.Text = "View:";
            // 
            // btnParse
            // 
            btnParse.Location = new Point(12, 64);
            btnParse.Name = "btnParse";
            btnParse.Size = new Size(75, 23);
            btnParse.TabIndex = 5;
            btnParse.Text = "&Parse";
            btnParse.UseVisualStyleBackColor = true;
            btnParse.Click += btnParse_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { menuMore, menuStatus });
            statusStrip1.Location = new Point(0, 384);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(782, 22);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // menuMore
            // 
            menuMore.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuMore.DropDownItems.AddRange(new ToolStripItem[] { menuImportFolder, menuReadFromFile, menuWatchJournal });
            menuMore.Image = (Image)resources.GetObject("menuMore.Image");
            menuMore.ImageTransparentColor = Color.Magenta;
            menuMore.Name = "menuMore";
            menuMore.Size = new Size(29, 20);
            menuMore.Text = "...";
            menuMore.DropDownOpening += menuMore_DropDownOpening;
            // 
            // menuImportFolder
            // 
            menuImportFolder.Name = "menuImportFolder";
            menuImportFolder.Size = new Size(197, 22);
            menuImportFolder.Text = "Import quest folder...";
            menuImportFolder.Click += menuImportFolder_Click;
            // 
            // menuReadFromFile
            // 
            menuReadFromFile.Name = "menuReadFromFile";
            menuReadFromFile.Size = new Size(197, 22);
            menuReadFromFile.Text = "Reset to file";
            menuReadFromFile.Click += menuReadFromFile_Click;
            // 
            // menuWatchJournal
            // 
            menuWatchJournal.Name = "menuWatchJournal";
            menuWatchJournal.Size = new Size(197, 22);
            menuWatchJournal.Text = "Watch journal events ...";
            menuWatchJournal.Click += menuWatchJournal_Click;
            // 
            // menuStatus
            // 
            menuStatus.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            menuStatus.BorderStyle = Border3DStyle.SunkenOuter;
            menuStatus.Name = "menuStatus";
            menuStatus.Size = new Size(707, 17);
            menuStatus.Spring = true;
            menuStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // checkWatchFolder
            // 
            checkWatchFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkWatchFolder.AutoSize = true;
            checkWatchFolder.Location = new Point(676, 8);
            checkWatchFolder.Name = "checkWatchFolder";
            checkWatchFolder.Size = new Size(94, 19);
            checkWatchFolder.TabIndex = 2;
            checkWatchFolder.Text = "&Watch folder";
            checkWatchFolder.UseVisualStyleBackColor = true;
            checkWatchFolder.CheckedChanged += checkWatchFolder_CheckedChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(12, 93);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "&Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // FormPlayDev
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 406);
            Controls.Add(btnRefresh);
            Controls.Add(checkWatchFolder);
            Controls.Add(statusStrip1);
            Controls.Add(btnParse);
            Controls.Add(comboChapter);
            Controls.Add(label2);
            Controls.Add(comboQuest);
            Controls.Add(label1);
            Controls.Add(txtJson);
            Name = "FormPlayDev";
            Text = "Quest Debugger";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtJson;
        private Label label1;
        private ComboBox comboQuest;
        private ComboBox comboChapter;
        private Label label2;
        private Button btnParse;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel menuStatus;
        private CheckBox checkWatchFolder;
        private Button btnRefresh;
        private ToolStripDropDownButton menuMore;
        private ToolStripMenuItem menuImportFolder;
        private ToolStripMenuItem menuWatchJournal;
        private ToolStripMenuItem menuReadFromFile;
    }
}