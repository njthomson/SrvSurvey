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

                if (bs != null)
                    bs.changed -= boxelSearch_changed;
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBoxelSearch));
            txtTopBoxel = new TextBox();
            btnSearch = new Button();
            lblMaxNum = new Label();
            txtCurrent = new TextBox();
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
            btnBoxelEmpty = new Button();
            btnParent = new Button();
            checkSkipVisited = new CheckBox();
            checkSpinKnownToSpansh = new CheckBox();
            btnPaste = new Button();
            panelSiblings = new Panel();
            lblWarning = new Label();
            label3 = new Label();
            menuSiblings = new ButtonContextMenuStrip(components);
            status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).BeginInit();
            panelList.SuspendLayout();
            tableTop.SuspendLayout();
            SuspendLayout();
            // 
            // txtTopBoxel
            // 
            resources.ApplyResources(txtTopBoxel, "txtTopBoxel");
            txtTopBoxel.Name = "txtTopBoxel";
            txtTopBoxel.TextChanged += txtTopBoxel_TextChanged;
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
            // txtCurrent
            // 
            resources.ApplyResources(txtCurrent, "txtCurrent");
            txtCurrent.Name = "txtCurrent";
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
            comboFrom.SelectedSystem = null;
            comboFrom.updateOnJump = false;
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
            numMax.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMax.Name = "numMax";
            numMax.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numMax.ValueChanged += numMax_ValueChanged;
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
            tableTop.Controls.Add(btnSearch, 0, 1);
            tableTop.Controls.Add(numMax, 3, 1);
            tableTop.Controls.Add(txtTopBoxel, 1, 1);
            tableTop.Controls.Add(lblMaxNum, 2, 1);
            tableTop.Controls.Add(panelList, 0, 7);
            tableTop.Controls.Add(btnBoxelEmpty, 1, 6);
            tableTop.Controls.Add(btnParent, 0, 6);
            tableTop.Controls.Add(txtCurrent, 1, 4);
            tableTop.Controls.Add(checkSkipVisited, 1, 2);
            tableTop.Controls.Add(checkSpinKnownToSpansh, 1, 3);
            tableTop.Controls.Add(btnPaste, 0, 4);
            tableTop.Controls.Add(checkAutoCopy, 2, 6);
            tableTop.Controls.Add(btnCopyNext, 2, 4);
            tableTop.Controls.Add(panelSiblings, 4, 6);
            tableTop.Controls.Add(lblWarning, 1, 5);
            tableTop.Controls.Add(label3, 1, 8);
            tableTop.Name = "tableTop";
            // 
            // btnBoxelEmpty
            // 
            resources.ApplyResources(btnBoxelEmpty, "btnBoxelEmpty");
            btnBoxelEmpty.Name = "btnBoxelEmpty";
            btnBoxelEmpty.UseVisualStyleBackColor = true;
            btnBoxelEmpty.Click += btnBoxelEmpty_Click;
            // 
            // btnParent
            // 
            resources.ApplyResources(btnParent, "btnParent");
            btnParent.Name = "btnParent";
            btnParent.UseVisualStyleBackColor = true;
            // 
            // checkSkipVisited
            // 
            resources.ApplyResources(checkSkipVisited, "checkSkipVisited");
            tableTop.SetColumnSpan(checkSkipVisited, 3);
            checkSkipVisited.Name = "checkSkipVisited";
            checkSkipVisited.UseVisualStyleBackColor = true;
            checkSkipVisited.CheckedChanged += checkSkipVisited_CheckedChanged;
            // 
            // checkSpinKnownToSpansh
            // 
            resources.ApplyResources(checkSpinKnownToSpansh, "checkSpinKnownToSpansh");
            tableTop.SetColumnSpan(checkSpinKnownToSpansh, 3);
            checkSpinKnownToSpansh.Name = "checkSpinKnownToSpansh";
            checkSpinKnownToSpansh.UseVisualStyleBackColor = true;
            checkSpinKnownToSpansh.CheckedChanged += checkSpinKnownToSpansh_CheckedChanged;
            // 
            // btnPaste
            // 
            resources.ApplyResources(btnPaste, "btnPaste");
            btnPaste.Name = "btnPaste";
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.Click += btnPaste_Click;
            // 
            // panelSiblings
            // 
            resources.ApplyResources(panelSiblings, "panelSiblings");
            panelSiblings.BackColor = SystemColors.AppWorkspace;
            panelSiblings.Name = "panelSiblings";
            tableTop.SetRowSpan(panelSiblings, 2);
            // 
            // lblWarning
            // 
            resources.ApplyResources(lblWarning, "lblWarning");
            lblWarning.Name = "lblWarning";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.FlatStyle = FlatStyle.System;
            label3.Name = "label3";
            // 
            // menuSiblings
            // 
            menuSiblings.Name = "menuSiblings";
            resources.ApplyResources(menuSiblings, "menuSiblings");
            menuSiblings.targetButton = btnParent;
            menuSiblings.ItemClicked += menuSiblings_ItemClicked;
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
        private TextBox txtTopBoxel;
        private Button btnSearch;
        private Label lblMaxNum;
        private TextBox txtCurrent;
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
        private Button btnParent;
        private ButtonContextMenuStrip menuSiblings;
        private Button btnBoxelEmpty;
        private Panel panelSiblings;
        private CheckBox checkSkipVisited;
        private CheckBox checkSpinKnownToSpansh;
        private Button btnPaste;
        private Label lblWarning;
        private Label label3;
    }
}