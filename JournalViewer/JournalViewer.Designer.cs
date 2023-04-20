namespace JournalViewer
{
    partial class JournalViewer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JournalViewer));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblCountRows = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolClose = new System.Windows.Forms.ToolStripSplitButton();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.checkNoFilter = new System.Windows.Forms.CheckBox();
            this.checkJsonContains = new System.Windows.Forms.CheckBox();
            this.txtJsonContains = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dateEnd = new System.Windows.Forms.DateTimePicker();
            this.dateStart = new System.Windows.Forms.DateTimePicker();
            this.checkDateRange = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkEventName = new System.Windows.Forms.CheckBox();
            this.btnFilter = new System.Windows.Forms.Button();
            this.comboEventName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnChooseFile = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.txtFilename = new System.Windows.Forms.TextBox();
            this.splitMiddle = new System.Windows.Forms.SplitContainer();
            this.viewer = new System.Windows.Forms.ListView();
            this.eventName = new System.Windows.Forms.ColumnHeader();
            this.timestamp = new System.Windows.Forms.ColumnHeader();
            this.json = new System.Windows.Forms.ColumnHeader();
            this.tree = new System.Windows.Forms.TreeView();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.statusStrip1.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.panelFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMiddle)).BeginInit();
            this.splitMiddle.Panel1.SuspendLayout();
            this.splitMiddle.Panel2.SuspendLayout();
            this.splitMiddle.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.AddToRecent = false;
            this.openFileDialog.Filter = "Journal files|*.log";
            this.openFileDialog.ShowPinnedPlaces = false;
            this.openFileDialog.Title = "Choose a journal file";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCountRows,
            this.lblStatus,
            this.toolClose});
            this.statusStrip1.Location = new System.Drawing.Point(0, 636);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(911, 24);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblCountRows
            // 
            this.lblCountRows.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblCountRows.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.lblCountRows.Name = "lblCountRows";
            this.lblCountRows.Size = new System.Drawing.Size(40, 19);
            this.lblCountRows.Text = "0 of 0";
            // 
            // lblStatus
            // 
            this.lblStatus.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblStatus.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(804, 19);
            this.lblStatus.Spring = true;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolClose
            // 
            this.toolClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolClose.Image = ((System.Drawing.Image)(resources.GetObject("toolClose.Image")));
            this.toolClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolClose.Name = "toolClose";
            this.toolClose.Size = new System.Drawing.Size(52, 22);
            this.toolClose.Text = "Close";
            this.toolClose.ButtonClick += new System.EventHandler(this.toolClose_ButtonClick);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.panelFilter);
            this.panelTop.Controls.Add(this.txtLog);
            this.panelTop.Controls.Add(this.btnChooseFile);
            this.panelTop.Controls.Add(this.btnOpen);
            this.panelTop.Controls.Add(this.txtFilename);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(911, 196);
            this.panelTop.TabIndex = 19;
            // 
            // panelFilter
            // 
            this.panelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFilter.Controls.Add(this.checkNoFilter);
            this.panelFilter.Controls.Add(this.checkJsonContains);
            this.panelFilter.Controls.Add(this.txtJsonContains);
            this.panelFilter.Controls.Add(this.label3);
            this.panelFilter.Controls.Add(this.dateEnd);
            this.panelFilter.Controls.Add(this.dateStart);
            this.panelFilter.Controls.Add(this.checkDateRange);
            this.panelFilter.Controls.Add(this.label2);
            this.panelFilter.Controls.Add(this.checkEventName);
            this.panelFilter.Controls.Add(this.btnFilter);
            this.panelFilter.Controls.Add(this.comboEventName);
            this.panelFilter.Controls.Add(this.label1);
            this.panelFilter.Location = new System.Drawing.Point(3, 103);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Size = new System.Drawing.Size(908, 87);
            this.panelFilter.TabIndex = 6;
            // 
            // checkNoFilter
            // 
            this.checkNoFilter.AutoSize = true;
            this.checkNoFilter.Location = new System.Drawing.Point(171, 65);
            this.checkNoFilter.Name = "checkNoFilter";
            this.checkNoFilter.Size = new System.Drawing.Size(87, 19);
            this.checkNoFilter.TabIndex = 46;
            this.checkNoFilter.Text = "Ignore filter";
            this.checkNoFilter.UseVisualStyleBackColor = true;
            this.checkNoFilter.CheckedChanged += new System.EventHandler(this.checkNoFilter_CheckedChanged);
            // 
            // checkJsonContains
            // 
            this.checkJsonContains.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkJsonContains.AutoSize = true;
            this.checkJsonContains.Location = new System.Drawing.Point(803, 38);
            this.checkJsonContains.Name = "checkJsonContains";
            this.checkJsonContains.Size = new System.Drawing.Size(102, 19);
            this.checkJsonContains.TabIndex = 45;
            this.checkJsonContains.Text = "hide/highlight";
            this.checkJsonContains.UseVisualStyleBackColor = true;
            // 
            // txtJsonContains
            // 
            this.txtJsonContains.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJsonContains.Location = new System.Drawing.Point(90, 36);
            this.txtJsonContains.Name = "txtJsonContains";
            this.txtJsonContains.Size = new System.Drawing.Size(710, 23);
            this.txtJsonContains.TabIndex = 44;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 15);
            this.label3.TabIndex = 43;
            this.label3.Text = "Json contains:";
            // 
            // dateEnd
            // 
            this.dateEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dateEnd.CustomFormat = "yyyy/MM/dd - HH:mm:ss";
            this.dateEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateEnd.Location = new System.Drawing.Point(648, 2);
            this.dateEnd.Name = "dateEnd";
            this.dateEnd.ShowUpDown = true;
            this.dateEnd.Size = new System.Drawing.Size(152, 23);
            this.dateEnd.TabIndex = 42;
            // 
            // dateStart
            // 
            this.dateStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dateStart.CustomFormat = "yyyy/MM/dd - HH:mm:ss";
            this.dateStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateStart.Location = new System.Drawing.Point(490, 2);
            this.dateStart.Name = "dateStart";
            this.dateStart.ShowUpDown = true;
            this.dateStart.Size = new System.Drawing.Size(152, 23);
            this.dateStart.TabIndex = 41;
            // 
            // checkDateRange
            // 
            this.checkDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkDateRange.AutoSize = true;
            this.checkDateRange.Location = new System.Drawing.Point(803, 5);
            this.checkDateRange.Name = "checkDateRange";
            this.checkDateRange.Size = new System.Drawing.Size(102, 19);
            this.checkDateRange.TabIndex = 40;
            this.checkDateRange.Text = "hide/highlight";
            this.checkDateRange.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(417, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 39;
            this.label2.Text = "Date range:";
            // 
            // checkEventName
            // 
            this.checkEventName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkEventName.AutoSize = true;
            this.checkEventName.Checked = true;
            this.checkEventName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEventName.Location = new System.Drawing.Point(284, 4);
            this.checkEventName.Name = "checkEventName";
            this.checkEventName.Size = new System.Drawing.Size(102, 19);
            this.checkEventName.TabIndex = 38;
            this.checkEventName.Text = "hide/highlight";
            this.checkEventName.UseVisualStyleBackColor = true;
            // 
            // btnFilter
            // 
            this.btnFilter.Location = new System.Drawing.Point(90, 62);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(75, 23);
            this.btnFilter.TabIndex = 37;
            this.btnFilter.Text = "&Filter";
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // comboEventName
            // 
            this.comboEventName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboEventName.FormattingEnabled = true;
            this.comboEventName.Items.AddRange(new object[] {
            "Alpha",
            "Bravo",
            "Charlie"});
            this.comboEventName.Location = new System.Drawing.Point(90, 4);
            this.comboEventName.Name = "comboEventName";
            this.comboEventName.Size = new System.Drawing.Size(188, 23);
            this.comboEventName.Sorted = true;
            this.comboEventName.TabIndex = 36;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 15);
            this.label1.TabIndex = 35;
            this.label1.Text = "Event name:";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 40);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(887, 57);
            this.txtLog.TabIndex = 18;
            // 
            // btnChooseFile
            // 
            this.btnChooseFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseFile.Location = new System.Drawing.Point(871, 11);
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.Size = new System.Drawing.Size(28, 23);
            this.btnChooseFile.TabIndex = 22;
            this.btnChooseFile.Text = "...";
            this.btnChooseFile.UseVisualStyleBackColor = true;
            this.btnChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 11);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(84, 23);
            this.btnOpen.TabIndex = 21;
            this.btnOpen.Text = "&Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // txtFilename
            // 
            this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilename.Location = new System.Drawing.Point(102, 12);
            this.txtFilename.Name = "txtFilename";
            this.txtFilename.Size = new System.Drawing.Size(763, 23);
            this.txtFilename.TabIndex = 20;
            this.txtFilename.Text = "C:\\Users\\grinn\\Saved Games\\Frontier Developments\\Elite Dangerous\\Journal.2023-04-" +
    "16T190328.01.log";
            // 
            // splitMiddle
            // 
            this.splitMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMiddle.Location = new System.Drawing.Point(0, 196);
            this.splitMiddle.Name = "splitMiddle";
            // 
            // splitMiddle.Panel1
            // 
            this.splitMiddle.Panel1.Controls.Add(this.viewer);
            // 
            // splitMiddle.Panel2
            // 
            this.splitMiddle.Panel2.Controls.Add(this.tree);
            this.splitMiddle.Panel2.Controls.Add(this.propertyGrid);
            this.splitMiddle.Size = new System.Drawing.Size(911, 440);
            this.splitMiddle.SplitterDistance = 575;
            this.splitMiddle.TabIndex = 20;
            // 
            // viewer
            // 
            this.viewer.AutoArrange = false;
            this.viewer.CausesValidation = false;
            this.viewer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.timestamp,
            this.eventName,
            this.json});
            this.viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewer.FullRowSelect = true;
            this.viewer.GridLines = true;
            this.viewer.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.viewer.Location = new System.Drawing.Point(0, 0);
            this.viewer.Name = "viewer";
            this.viewer.OwnerDraw = true;
            this.viewer.ShowGroups = false;
            this.viewer.ShowItemToolTips = true;
            this.viewer.Size = new System.Drawing.Size(575, 440);
            this.viewer.TabIndex = 5;
            this.viewer.UseCompatibleStateImageBehavior = false;
            this.viewer.View = System.Windows.Forms.View.Details;
            this.viewer.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.viewer_DrawColumnHeader);
            this.viewer.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.viewer_DrawItem);
            this.viewer.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.viewer_DrawSubItem);
            this.viewer.KeyUp += new System.Windows.Forms.KeyEventHandler(this.viewer_KeyUp);
            this.viewer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.viewer_MouseClick);
            this.viewer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.viewer_MouseDoubleClick);
            // 
            // eventName
            // 
            this.eventName.Text = "Event";
            this.eventName.Width = 160;
            // 
            // timestamp
            // 
            this.timestamp.Text = "Timestamp";
            this.timestamp.Width = 160;
            // 
            // json
            // 
            this.json.Text = "Json";
            // 
            // tree
            // 
            this.tree.CausesValidation = false;
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.FullRowSelect = true;
            this.tree.HideSelection = false;
            this.tree.Location = new System.Drawing.Point(0, 0);
            this.tree.Name = "tree";
            this.tree.Size = new System.Drawing.Size(332, 440);
            this.tree.TabIndex = 19;
            this.tree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tree_KeyUp);
            this.tree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tree_MouseDoubleClick);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(332, 440);
            this.propertyGrid.TabIndex = 18;
            // 
            // JournalViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 660);
            this.Controls.Add(this.splitMiddle);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.statusStrip1);
            this.KeyPreview = true;
            this.Name = "JournalViewer";
            this.Text = "Journal Viewer";
            this.Load += new System.EventHandler(this.JournalViewer_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JournalViewer_KeyUp);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            this.splitMiddle.Panel1.ResumeLayout(false);
            this.splitMiddle.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMiddle)).EndInit();
            this.splitMiddle.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private OpenFileDialog openFileDialog;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblCountRows;
        private ToolStripStatusLabel lblStatus;
        private Panel panelTop;
        private TextBox txtLog;
        private Button btnChooseFile;
        private Button btnOpen;
        private TextBox txtFilename;
        private SplitContainer splitMiddle;
        private ListView viewer;
        private ColumnHeader eventName;
        private ColumnHeader timestamp;
        private ColumnHeader json;
        private PropertyGrid propertyGrid;
        private TreeView tree;
        private ToolStripSplitButton toolClose;
        private Panel panelFilter;
        private CheckBox checkNoFilter;
        private CheckBox checkJsonContains;
        private TextBox txtJsonContains;
        private Label label3;
        private DateTimePicker dateEnd;
        private DateTimePicker dateStart;
        private CheckBox checkDateRange;
        private Label label2;
        private CheckBox checkEventName;
        private Button btnFilter;
        private ComboBox comboEventName;
        private Label label1;
    }
}