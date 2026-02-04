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
            comboQuest = new ThemedComboBox();
            comboChapter = new ThemedComboBox();
            label2 = new Label();
            btnParse = new DrawButton();
            statusStrip1 = new StatusStrip();
            menuMore = new ToolStripDropDownButton();
            menuImportFolder2 = new ToolStripMenuItem();
            menuReadFromFile = new ToolStripMenuItem();
            menuStatus = new ToolStripStatusLabel();
            menuWatch = new ToolStripDropDownButton();
            menuComms = new ToolStripDropDownButton();
            checkWatchFolder = new CheckBox();
            btnRefresh = new DrawButton();
            btnRestartChapter = new DrawButton();
            btnStopChapter = new DrawButton();
            txtCode = new TextBox();
            btnRun = new DrawButton();
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
            txtJson.Size = new Size(728, 244);
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
            comboQuest.Size = new Size(628, 23);
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
            comboChapter.Size = new Size(474, 23);
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
            btnParse.AnimateOnPress = false;
            btnParse.BackColorDisabled = Color.Empty;
            btnParse.BackColorHover = Color.Empty;
            btnParse.BackColorPressed = Color.Empty;
            btnParse.DrawBorder = true;
            btnParse.FlatStyle = FlatStyle.Flat;
            btnParse.ForeColor = Color.Black;
            btnParse.ForeColorDisabled = Color.Empty;
            btnParse.ForeColorHover = Color.Empty;
            btnParse.ForeColorPressed = Color.Empty;
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { menuMore, menuStatus, menuWatch, menuComms });
            statusStrip1.Location = new Point(0, 386);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.ShowItemToolTips = true;
            statusStrip1.Size = new Size(833, 22);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // menuMore
            // 
            menuMore.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuMore.DropDownItems.AddRange(new ToolStripItem[] { menuImportFolder2, menuReadFromFile });
            menuMore.Image = (Image)resources.GetObject("menuMore.Image");
            menuMore.ImageTransparentColor = Color.Magenta;
            menuMore.Name = "menuMore";
            menuMore.Size = new Size(58, 20);
            menuMore.Text = "Load ...";
            // 
            // menuImportFolder2
            // 
            menuImportFolder2.Name = "menuImportFolder2";
            menuImportFolder2.Size = new Size(185, 22);
            menuImportFolder2.Text = "Import quest folder...";
            menuImportFolder2.Click += menuImportFolder_Click;
            // 
            // menuReadFromFile
            // 
            menuReadFromFile.Name = "menuReadFromFile";
            menuReadFromFile.Size = new Size(185, 22);
            menuReadFromFile.Text = "Reset to file";
            menuReadFromFile.Click += menuReadFromFile_Click;
            // 
            // menuStatus
            // 
            menuStatus.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            menuStatus.BorderStyle = Border3DStyle.SunkenOuter;
            menuStatus.Name = "menuStatus";
            menuStatus.Size = new Size(638, 17);
            menuStatus.Spring = true;
            menuStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // menuWatch
            // 
            menuWatch.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuWatch.ImageTransparentColor = Color.Magenta;
            menuWatch.Name = "menuWatch";
            menuWatch.ShowDropDownArrow = false;
            menuWatch.Size = new Size(57, 20);
            menuWatch.Text = "( watch )";
            menuWatch.ToolTipText = "Open quest watch window";
            menuWatch.Click += menuWatch_Click;
            // 
            // menuComms
            // 
            menuComms.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuComms.ImageTransparentColor = Color.Magenta;
            menuComms.Name = "menuComms";
            menuComms.ShowDropDownArrow = false;
            menuComms.Size = new Size(65, 20);
            menuComms.Text = "( comms )";
            menuComms.ToolTipText = "Open quest comms window";
            menuComms.Click += menuComms_Click;
            // 
            // checkWatchFolder
            // 
            checkWatchFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkWatchFolder.AutoSize = true;
            checkWatchFolder.Location = new Point(727, 8);
            checkWatchFolder.Name = "checkWatchFolder";
            checkWatchFolder.Size = new Size(94, 19);
            checkWatchFolder.TabIndex = 2;
            checkWatchFolder.Text = "&Watch folder";
            checkWatchFolder.UseVisualStyleBackColor = true;
            checkWatchFolder.CheckedChanged += checkWatchFolder_CheckedChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.AnimateOnPress = false;
            btnRefresh.BackColorDisabled = Color.Empty;
            btnRefresh.BackColorHover = Color.Empty;
            btnRefresh.BackColorPressed = Color.Empty;
            btnRefresh.DrawBorder = true;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.ForeColor = Color.Black;
            btnRefresh.ForeColorDisabled = Color.Empty;
            btnRefresh.ForeColorHover = Color.Empty;
            btnRefresh.ForeColorPressed = Color.Empty;
            btnRefresh.Location = new Point(12, 93);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "&Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnRestartChapter
            // 
            btnRestartChapter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRestartChapter.AnimateOnPress = false;
            btnRestartChapter.BackColorDisabled = Color.Empty;
            btnRestartChapter.BackColorHover = Color.Empty;
            btnRestartChapter.BackColorPressed = Color.Empty;
            btnRestartChapter.DrawBorder = true;
            btnRestartChapter.FlatStyle = FlatStyle.Flat;
            btnRestartChapter.ForeColor = Color.Black;
            btnRestartChapter.ForeColorDisabled = Color.Empty;
            btnRestartChapter.ForeColorHover = Color.Empty;
            btnRestartChapter.ForeColorPressed = Color.Empty;
            btnRestartChapter.Location = new Point(573, 34);
            btnRestartChapter.Name = "btnRestartChapter";
            btnRestartChapter.Size = new Size(148, 25);
            btnRestartChapter.TabIndex = 9;
            btnRestartChapter.Text = "(re) Start chapter";
            btnRestartChapter.UseVisualStyleBackColor = true;
            btnRestartChapter.Click += btnRestartChapter_Click;
            // 
            // btnStopChapter
            // 
            btnStopChapter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStopChapter.AnimateOnPress = false;
            btnStopChapter.BackColorDisabled = Color.Empty;
            btnStopChapter.BackColorHover = Color.Empty;
            btnStopChapter.BackColorPressed = Color.Empty;
            btnStopChapter.DrawBorder = true;
            btnStopChapter.FlatStyle = FlatStyle.Flat;
            btnStopChapter.ForeColor = Color.Black;
            btnStopChapter.ForeColorDisabled = Color.Empty;
            btnStopChapter.ForeColorHover = Color.Empty;
            btnStopChapter.ForeColorPressed = Color.Empty;
            btnStopChapter.Location = new Point(728, 34);
            btnStopChapter.Name = "btnStopChapter";
            btnStopChapter.Size = new Size(94, 25);
            btnStopChapter.TabIndex = 10;
            btnStopChapter.Text = "Stop";
            btnStopChapter.UseVisualStyleBackColor = true;
            btnStopChapter.Click += btnStopChapter_Click;
            // 
            // txtCode
            // 
            txtCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtCode.Location = new Point(93, 314);
            txtCode.Multiline = true;
            txtCode.Name = "txtCode";
            txtCode.ScrollBars = ScrollBars.Both;
            txtCode.Size = new Size(728, 69);
            txtCode.TabIndex = 11;
            // 
            // btnRun
            // 
            btnRun.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRun.AnimateOnPress = false;
            btnRun.BackColorDisabled = Color.Empty;
            btnRun.BackColorHover = Color.Empty;
            btnRun.BackColorPressed = Color.Empty;
            btnRun.DrawBorder = true;
            btnRun.FlatStyle = FlatStyle.Flat;
            btnRun.ForeColor = Color.Black;
            btnRun.ForeColorDisabled = Color.Empty;
            btnRun.ForeColorHover = Color.Empty;
            btnRun.ForeColorPressed = Color.Empty;
            btnRun.Location = new Point(12, 314);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(75, 23);
            btnRun.TabIndex = 12;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // FormPlayDev
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(833, 408);
            Controls.Add(btnRun);
            Controls.Add(txtCode);
            Controls.Add(btnStopChapter);
            Controls.Add(btnRestartChapter);
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
        private ThemedComboBox comboQuest;
        private ThemedComboBox comboChapter;
        private Label label2;
        private DrawButton btnParse;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel menuStatus;
        private CheckBox checkWatchFolder;
        private DrawButton btnRefresh;
        private ToolStripDropDownButton menuMore;
        private ToolStripMenuItem menuImportFolder2;
        private ToolStripMenuItem menuReadFromFile;
        private ToolStripDropDownButton menuWatch;
        private ToolStripDropDownButton menuComms;
        private DrawButton btnRestartChapter;
        private DrawButton btnStopChapter;
        private TextBox txtCode;
        private DrawButton btnRun;
    }
}
