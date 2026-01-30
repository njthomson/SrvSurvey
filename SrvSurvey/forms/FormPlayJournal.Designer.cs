namespace SrvSurvey.forms
{
    partial class FormPlayJournal
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

            if (game?.status != null) game.status.StatusChanged -= Status_StatusChanged;
            if (game?.journals != null) game.journals.onRawJournalEntry -= Journals_onRawJournalEntry;

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableDebug = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            txtStatusFile = new TextBox();
            treeJournals = new TreeView();
            txtNewCode = new TextBox();
            btnCopyCode = new Button();
            btnReplay = new Button();
            statusStrip1 = new StatusStrip();
            menuComms = new ToolStripDropDownButton();
            menuDev = new ToolStripDropDownButton();
            menuSpring = new ToolStripStatusLabel();
            tableDebug.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tableDebug
            // 
            tableDebug.ColumnCount = 2;
            tableDebug.ColumnStyles.Add(new ColumnStyle());
            tableDebug.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableDebug.Controls.Add(label1, 0, 0);
            tableDebug.Controls.Add(label2, 0, 1);
            tableDebug.Controls.Add(txtStatusFile, 1, 0);
            tableDebug.Controls.Add(treeJournals, 1, 1);
            tableDebug.Controls.Add(txtNewCode, 1, 3);
            tableDebug.Controls.Add(btnCopyCode, 0, 3);
            tableDebug.Controls.Add(btnReplay, 0, 2);
            tableDebug.Dock = DockStyle.Fill;
            tableDebug.Location = new Point(0, 0);
            tableDebug.Name = "tableDebug";
            tableDebug.RowCount = 4;
            tableDebug.RowStyles.Add(new RowStyle());
            tableDebug.RowStyles.Add(new RowStyle());
            tableDebug.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableDebug.RowStyles.Add(new RowStyle());
            tableDebug.Size = new Size(791, 467);
            tableDebug.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(38, 10);
            label1.Margin = new Padding(10);
            label1.Name = "label1";
            label1.Size = new Size(140, 14);
            label1.TabIndex = 0;
            label1.Text = "Status file values:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 99);
            label2.Margin = new Padding(10);
            label2.Name = "label2";
            label2.Size = new Size(168, 14);
            label2.TabIndex = 1;
            label2.Text = "Recent journal entries:";
            // 
            // txtStatusFile
            // 
            txtStatusFile.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtStatusFile.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtStatusFile.Location = new Point(191, 3);
            txtStatusFile.Multiline = true;
            txtStatusFile.Name = "txtStatusFile";
            txtStatusFile.ReadOnly = true;
            txtStatusFile.ScrollBars = ScrollBars.Both;
            txtStatusFile.Size = new Size(597, 83);
            txtStatusFile.TabIndex = 2;
            txtStatusFile.Text = "Destination:\r\nFlags:\r\nFlags2:\r\nLat/Long:\r\n";
            // 
            // treeJournals
            // 
            treeJournals.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeJournals.FullRowSelect = true;
            treeJournals.HideSelection = false;
            treeJournals.Location = new Point(191, 92);
            treeJournals.Name = "treeJournals";
            tableDebug.SetRowSpan(treeJournals, 2);
            treeJournals.Size = new Size(597, 276);
            treeJournals.TabIndex = 3;
            treeJournals.NodeMouseClick += treeJournals_NodeMouseClick;
            // 
            // txtNewCode
            // 
            txtNewCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNewCode.Location = new Point(191, 374);
            txtNewCode.Multiline = true;
            txtNewCode.Name = "txtNewCode";
            txtNewCode.ScrollBars = ScrollBars.Both;
            txtNewCode.Size = new Size(597, 90);
            txtNewCode.TabIndex = 1;
            txtNewCode.Text = "1\r\n2\r\n3\r\n4\r\n5\r\n6";
            // 
            // btnCopyCode
            // 
            btnCopyCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyCode.AutoSize = true;
            btnCopyCode.Location = new Point(98, 381);
            btnCopyCode.Margin = new Padding(10);
            btnCopyCode.Name = "btnCopyCode";
            btnCopyCode.Size = new Size(80, 25);
            btnCopyCode.TabIndex = 4;
            btnCopyCode.Text = "Copy code";
            btnCopyCode.UseVisualStyleBackColor = true;
            btnCopyCode.Click += btnCopyCode_Click;
            // 
            // btnReplay
            // 
            btnReplay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnReplay.AutoSize = true;
            btnReplay.Location = new Point(77, 133);
            btnReplay.Margin = new Padding(10);
            btnReplay.Name = "btnReplay";
            btnReplay.Size = new Size(101, 25);
            btnReplay.TabIndex = 5;
            btnReplay.Text = "Replay event";
            btnReplay.UseVisualStyleBackColor = true;
            btnReplay.Click += btnReplay_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { menuComms, menuDev, menuSpring });
            statusStrip1.Location = new Point(0, 467);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.ShowItemToolTips = true;
            statusStrip1.Size = new Size(791, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
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
            // menuDev
            // 
            menuDev.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuDev.ImageTransparentColor = Color.Magenta;
            menuDev.Name = "menuDev";
            menuDev.ShowDropDownArrow = false;
            menuDev.Size = new Size(44, 20);
            menuDev.Text = "( dev )";
            menuDev.ToolTipText = "Open quests dev window";
            menuDev.Click += menuDev_Click;
            // 
            // menuSpring
            // 
            menuSpring.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            menuSpring.BorderStyle = Border3DStyle.SunkenOuter;
            menuSpring.Name = "menuSpring";
            menuSpring.Size = new Size(636, 17);
            menuSpring.Spring = true;
            // 
            // FormPlayJournal
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(791, 489);
            Controls.Add(tableDebug);
            Controls.Add(statusStrip1);
            Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "FormPlayJournal";
            Text = "Journal watcher";
            tableDebug.ResumeLayout(false);
            tableDebug.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel tableDebug;
        private Label label1;
        private Label label2;
        private TextBox txtStatusFile;
        private TreeView treeJournals;
        private TextBox txtNewCode;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel menuSpring;
        private Button btnCopyCode;
        private Button btnReplay;
        private ToolStripDropDownButton menuDev;
        private ToolStripDropDownButton menuComms;
    }
}