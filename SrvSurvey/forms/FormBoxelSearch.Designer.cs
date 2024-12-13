namespace SrvSurvey.forms
{
    partial class FormBoxelSearch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBoxelSearch));
            txtSystemName = new TextBox();
            btnSearch = new Button();
            lblMaxNum = new Label();
            txtNext = new TextBox();
            btnCopyNext = new Button();
            list = new ListView();
            colSystem = new ColumnHeader();
            colDistance = new ColumnHeader();
            colNotes = new ColumnHeader();
            label1 = new Label();
            label2 = new Label();
            comboFrom = new ComboStarSystem();
            status = new StatusStrip();
            btnToggleList = new ToolStripSplitButton();
            lblStatus = new ToolStripStatusLabel();
            menuMore = new ToolStripDropDownButton();
            menuHelpLink = new ToolStripMenuItem();
            checkAutoCopy = new CheckBox();
            numMax = new NumericUpDown();
            panelList = new Panel();
            tableTop = new TableLayoutPanel();
            linkKeyChords = new LinkLabel();
            status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).BeginInit();
            panelList.SuspendLayout();
            tableTop.SuspendLayout();
            SuspendLayout();
            // 
            // txtSystemName
            // 
            resources.ApplyResources(txtSystemName, "txtSystemName");
            txtSystemName.Name = "txtSystemName";
            // 
            // btnSearch
            // 
            resources.ApplyResources(btnSearch, "btnSearch");
            btnSearch.Name = "btnSearch";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // lblMaxNum
            // 
            resources.ApplyResources(lblMaxNum, "lblMaxNum");
            lblMaxNum.Name = "lblMaxNum";
            // 
            // txtNext
            // 
            resources.ApplyResources(txtNext, "txtNext");
            txtNext.Name = "txtNext";
            // 
            // btnCopyNext
            // 
            resources.ApplyResources(btnCopyNext, "btnCopyNext");
            btnCopyNext.Name = "btnCopyNext";
            btnCopyNext.UseVisualStyleBackColor = true;
            btnCopyNext.Click += btnCopyNext_Click;
            // 
            // list
            // 
            resources.ApplyResources(list, "list");
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colSystem, colDistance, colNotes });
            list.FullRowSelect = true;
            list.Name = "list";
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ItemChecked += list_ItemChecked;
            // 
            // colSystem
            // 
            resources.ApplyResources(colSystem, "colSystem");
            // 
            // colDistance
            // 
            resources.ApplyResources(colDistance, "colDistance");
            // 
            // colNotes
            // 
            resources.ApplyResources(colNotes, "colNotes");
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            tableTop.SetColumnSpan(label1, 4);
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // comboFrom
            // 
            resources.ApplyResources(comboFrom, "comboFrom");
            comboFrom.FormattingEnabled = true;
            comboFrom.Name = "comboFrom";
            comboFrom.SelectedIndexChanged += comboFrom_SelectedIndexChanged;
            // 
            // status
            // 
            status.Items.AddRange(new ToolStripItem[] { btnToggleList, lblStatus, menuMore });
            resources.ApplyResources(status, "status");
            status.Name = "status";
            // 
            // btnToggleList
            // 
            btnToggleList.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToggleList.DropDownButtonWidth = 0;
            resources.ApplyResources(btnToggleList, "btnToggleList");
            btnToggleList.Name = "btnToggleList";
            btnToggleList.ButtonClick += btnToggleList_ButtonClick;
            // 
            // lblStatus
            // 
            lblStatus.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblStatus.BorderStyle = Border3DStyle.SunkenOuter;
            lblStatus.Name = "lblStatus";
            resources.ApplyResources(lblStatus, "lblStatus");
            lblStatus.Spring = true;
            // 
            // menuMore
            // 
            menuMore.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuMore.DropDownItems.AddRange(new ToolStripItem[] { menuHelpLink });
            resources.ApplyResources(menuMore, "menuMore");
            menuMore.Name = "menuMore";
            // 
            // menuHelpLink
            // 
            menuHelpLink.Name = "menuHelpLink";
            resources.ApplyResources(menuHelpLink, "menuHelpLink");
            menuHelpLink.Click += menuHelpLink_Click;
            // 
            // checkAutoCopy
            // 
            resources.ApplyResources(checkAutoCopy, "checkAutoCopy");
            tableTop.SetColumnSpan(checkAutoCopy, 2);
            checkAutoCopy.Name = "checkAutoCopy";
            checkAutoCopy.UseVisualStyleBackColor = true;
            checkAutoCopy.CheckedChanged += checkAutoCopy_CheckedChanged;
            // 
            // numMax
            // 
            resources.ApplyResources(numMax, "numMax");
            numMax.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numMax.Name = "numMax";
            // 
            // panelList
            // 
            resources.ApplyResources(panelList, "panelList");
            tableTop.SetColumnSpan(panelList, 4);
            panelList.Controls.Add(comboFrom);
            panelList.Controls.Add(label2);
            panelList.Controls.Add(list);
            panelList.Name = "panelList";
            // 
            // tableTop
            // 
            resources.ApplyResources(tableTop, "tableTop");
            tableTop.Controls.Add(label1, 0, 0);
            tableTop.Controls.Add(panelList, 0, 4);
            tableTop.Controls.Add(btnSearch, 0, 1);
            tableTop.Controls.Add(numMax, 3, 1);
            tableTop.Controls.Add(txtSystemName, 1, 1);
            tableTop.Controls.Add(txtNext, 1, 2);
            tableTop.Controls.Add(btnCopyNext, 0, 2);
            tableTop.Controls.Add(lblMaxNum, 2, 1);
            tableTop.Controls.Add(checkAutoCopy, 2, 2);
            tableTop.Controls.Add(linkKeyChords, 0, 3);
            tableTop.Name = "tableTop";
            // 
            // linkKeyChords
            // 
            resources.ApplyResources(linkKeyChords, "linkKeyChords");
            tableTop.SetColumnSpan(linkKeyChords, 4);
            linkKeyChords.Name = "linkKeyChords";
            linkKeyChords.TabStop = true;
            linkKeyChords.LinkClicked += linkKeyChords_LinkClicked;
            // 
            // FormBoxelSearch
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableTop);
            Controls.Add(status);
            Name = "FormBoxelSearch";
            status.ResumeLayout(false);
            status.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).EndInit();
            panelList.ResumeLayout(false);
            panelList.PerformLayout();
            tableTop.ResumeLayout(false);
            tableTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtSystemName;
        private Button btnSearch;
        private Label lblMaxNum;
        private TextBox txtNext;
        private Button btnCopyNext;
        private ListView list;
        private ColumnHeader colSystem;
        private ColumnHeader colDistance;
        private Label label1;
        private Label label2;
        private ComboStarSystem comboFrom;
        private ColumnHeader colNotes;
        private StatusStrip status;
        private CheckBox checkAutoCopy;
        private NumericUpDown numMax;
        private Panel panelList;
        private ToolStripSplitButton btnToggleList;
        private ToolStripStatusLabel lblStatus;
        private ToolStripDropDownButton menuMore;
        private ToolStripMenuItem menuHelpLink;
        private TableLayoutPanel tableTop;
        private LinkLabel linkKeyChords;
    }
}