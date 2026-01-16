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
            txtNewCode = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtStatusFile = new TextBox();
            treeJournals = new TreeView();
            statusStrip1 = new StatusStrip();
            menuSpring = new ToolStripStatusLabel();
            btnReplay = new Button();
            tableDebug.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tableDebug
            // 
            tableDebug.ColumnCount = 2;
            tableDebug.ColumnStyles.Add(new ColumnStyle());
            tableDebug.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableDebug.Controls.Add(btnReplay, 0, 2);
            tableDebug.Controls.Add(txtNewCode, 1, 2);
            tableDebug.Controls.Add(label1, 0, 0);
            tableDebug.Controls.Add(label2, 0, 1);
            tableDebug.Controls.Add(txtStatusFile, 1, 0);
            tableDebug.Controls.Add(treeJournals, 1, 1);
            tableDebug.Dock = DockStyle.Fill;
            tableDebug.Location = new Point(0, 0);
            tableDebug.Name = "tableDebug";
            tableDebug.RowCount = 3;
            tableDebug.RowStyles.Add(new RowStyle());
            tableDebug.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableDebug.RowStyles.Add(new RowStyle());
            tableDebug.Size = new Size(791, 467);
            tableDebug.TabIndex = 0;
            // 
            // txtNewCode
            // 
            txtNewCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNewCode.Location = new Point(177, 418);
            txtNewCode.Multiline = true;
            txtNewCode.Name = "txtNewCode";
            txtNewCode.ScrollBars = ScrollBars.Both;
            txtNewCode.Size = new Size(611, 46);
            txtNewCode.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(91, 14);
            label1.TabIndex = 0;
            label1.Text = "Status file:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 89);
            label2.Name = "label2";
            label2.Size = new Size(168, 14);
            label2.TabIndex = 1;
            label2.Text = "Recent journal entries:";
            // 
            // txtStatusFile
            // 
            txtStatusFile.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtStatusFile.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtStatusFile.Location = new Point(177, 3);
            txtStatusFile.Multiline = true;
            txtStatusFile.Name = "txtStatusFile";
            txtStatusFile.ReadOnly = true;
            txtStatusFile.ScrollBars = ScrollBars.Both;
            txtStatusFile.Size = new Size(611, 83);
            txtStatusFile.TabIndex = 2;
            txtStatusFile.Text = "Destination:\r\nFlags:\r\nFlags2:\r\nLat/Long:\r\n";
            // 
            // treeJournals
            // 
            treeJournals.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeJournals.FullRowSelect = true;
            treeJournals.HideSelection = false;
            treeJournals.Location = new Point(177, 92);
            treeJournals.Name = "treeJournals";
            treeJournals.Size = new Size(611, 320);
            treeJournals.TabIndex = 3;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { menuSpring });
            statusStrip1.Location = new Point(0, 467);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(791, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // menuSpring
            // 
            menuSpring.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            menuSpring.BorderStyle = Border3DStyle.SunkenOuter;
            menuSpring.Name = "menuSpring";
            menuSpring.Size = new Size(776, 17);
            menuSpring.Spring = true;
            // 
            // btnReplay
            // 
            btnReplay.Location = new Point(3, 418);
            btnReplay.Name = "btnReplay";
            btnReplay.Size = new Size(75, 23);
            btnReplay.TabIndex = 2;
            btnReplay.Text = "Replay";
            btnReplay.UseVisualStyleBackColor = true;
            btnReplay.Click += btnReplay_Click;
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
        private Button btnReplay;
    }
}