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
            btnConfig = new Button();
            lblMaxNum = new Label();
            txtCurrent = new TextBox();
            btnCopyNext = new Button();
            list = new ListView();
            colSystem = new ColumnHeader();
            colDistance = new ColumnHeader();
            colNotes = new ColumnHeader();
            contextList = new ContextMenuStrip(components);
            menuListCopySystemName = new ToolStripMenuItem();
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
            btnPaste = new Button();
            panelSiblings = new Panel();
            lblWarning = new Label();
            label3 = new Label();
            checkSkipVisited = new CheckBox();
            checkSpinKnownToSpansh = new CheckBox();
            menuSiblings = new ButtonContextMenuStrip(components);
            tableConfig = new TableLayoutPanel();
            label4 = new Label();
            checkCompleteOnEnterSystem = new CheckBox();
            checkCompleteOnFssAllBodies = new CheckBox();
            btnBegin = new Button();
            label5 = new Label();
            comboLowMassCode = new ComboBox();
            label6 = new Label();
            btnPasteTopBoxel = new Button();
            labelBoxelCount = new Label();
            btnConfigCancel = new Button();
            dateStart = new DateTimePicker();
            contextList.SuspendLayout();
            status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).BeginInit();
            panelList.SuspendLayout();
            tableTop.SuspendLayout();
            tableConfig.SuspendLayout();
            SuspendLayout();
            // 
            // txtTopBoxel
            // 
            resources.ApplyResources(txtTopBoxel, "txtTopBoxel");
            tableConfig.SetColumnSpan(txtTopBoxel, 3);
            txtTopBoxel.Name = "txtTopBoxel";
            txtTopBoxel.TextChanged += txtTopBoxel_TextChanged;
            // 
            // btnConfig
            // 
            resources.ApplyResources(btnConfig, "btnConfig");
            btnConfig.Name = "btnConfig";
            btnConfig.UseVisualStyleBackColor = true;
            btnConfig.Click += btnConfig_Click;
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
            list.ContextMenuStrip = contextList;
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
            // contextList
            // 
            contextList.Items.AddRange(new ToolStripItem[] { menuListCopySystemName });
            contextList.Name = "contextList";
            resources.ApplyResources(contextList, "contextList");
            // 
            // menuListCopySystemName
            // 
            menuListCopySystemName.Name = "menuListCopySystemName";
            resources.ApplyResources(menuListCopySystemName, "menuListCopySystemName");
            menuListCopySystemName.Click += menuListCopySystemName_Click;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
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
            checkAutoCopy.Name = "checkAutoCopy";
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
            tableTop.Controls.Add(btnConfig, 0, 1);
            tableTop.Controls.Add(numMax, 3, 1);
            tableTop.Controls.Add(lblMaxNum, 2, 1);
            tableTop.Controls.Add(panelList, 0, 7);
            tableTop.Controls.Add(btnBoxelEmpty, 1, 6);
            tableTop.Controls.Add(btnParent, 0, 6);
            tableTop.Controls.Add(txtCurrent, 1, 4);
            tableTop.Controls.Add(btnPaste, 0, 4);
            tableTop.Controls.Add(checkAutoCopy, 2, 6);
            tableTop.Controls.Add(btnCopyNext, 2, 4);
            tableTop.Controls.Add(panelSiblings, 4, 6);
            tableTop.Controls.Add(lblWarning, 1, 5);
            tableTop.Controls.Add(label3, 1, 8);
            tableTop.Controls.Add(label1, 1, 0);
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
            // checkSkipVisited
            // 
            resources.ApplyResources(checkSkipVisited, "checkSkipVisited");
            tableConfig.SetColumnSpan(checkSkipVisited, 2);
            checkSkipVisited.Name = "checkSkipVisited";
            checkSkipVisited.UseVisualStyleBackColor = true;
            // 
            // checkSpinKnownToSpansh
            // 
            resources.ApplyResources(checkSpinKnownToSpansh, "checkSpinKnownToSpansh");
            tableConfig.SetColumnSpan(checkSpinKnownToSpansh, 2);
            checkSpinKnownToSpansh.Name = "checkSpinKnownToSpansh";
            checkSpinKnownToSpansh.UseVisualStyleBackColor = true;
            // 
            // menuSiblings
            // 
            menuSiblings.Name = "menuSiblings";
            resources.ApplyResources(menuSiblings, "menuSiblings");
            menuSiblings.targetButton = btnParent;
            menuSiblings.ItemClicked += menuSiblings_ItemClicked;
            // 
            // tableConfig
            // 
            resources.ApplyResources(tableConfig, "tableConfig");
            tableConfig.BackColor = SystemColors.AppWorkspace;
            tableConfig.Controls.Add(txtTopBoxel, 0, 0);
            tableConfig.Controls.Add(checkSkipVisited, 0, 2);
            tableConfig.Controls.Add(checkSpinKnownToSpansh, 0, 3);
            tableConfig.Controls.Add(label4, 0, 4);
            tableConfig.Controls.Add(checkCompleteOnEnterSystem, 0, 5);
            tableConfig.Controls.Add(checkCompleteOnFssAllBodies, 1, 5);
            tableConfig.Controls.Add(btnBegin, 0, 6);
            tableConfig.Controls.Add(label5, 0, 1);
            tableConfig.Controls.Add(comboLowMassCode, 1, 1);
            tableConfig.Controls.Add(label6, 1, 7);
            tableConfig.Controls.Add(btnPasteTopBoxel, 3, 0);
            tableConfig.Controls.Add(labelBoxelCount, 2, 1);
            tableConfig.Controls.Add(btnConfigCancel, 0, 7);
            tableConfig.Controls.Add(dateStart, 1, 6);
            tableConfig.Name = "tableConfig";
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            tableConfig.SetColumnSpan(label4, 2);
            label4.Name = "label4";
            // 
            // checkCompleteOnEnterSystem
            // 
            resources.ApplyResources(checkCompleteOnEnterSystem, "checkCompleteOnEnterSystem");
            checkCompleteOnEnterSystem.Name = "checkCompleteOnEnterSystem";
            checkCompleteOnEnterSystem.UseVisualStyleBackColor = true;
            checkCompleteOnEnterSystem.CheckedChanged += checkCompleteOnEnterSystem_CheckedChanged;
            // 
            // checkCompleteOnFssAllBodies
            // 
            resources.ApplyResources(checkCompleteOnFssAllBodies, "checkCompleteOnFssAllBodies");
            checkCompleteOnFssAllBodies.Name = "checkCompleteOnFssAllBodies";
            checkCompleteOnFssAllBodies.UseVisualStyleBackColor = true;
            checkCompleteOnFssAllBodies.CheckedChanged += checkCompleteOnFssAllBodies_CheckedChanged;
            // 
            // btnBegin
            // 
            resources.ApplyResources(btnBegin, "btnBegin");
            btnBegin.Name = "btnBegin";
            btnBegin.UseVisualStyleBackColor = true;
            btnBegin.Click += btnBegin_Click;
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            // 
            // comboLowMassCode
            // 
            comboLowMassCode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLowMassCode.FormattingEnabled = true;
            comboLowMassCode.Items.AddRange(new object[] { resources.GetString("comboLowMassCode.Items"), resources.GetString("comboLowMassCode.Items1"), resources.GetString("comboLowMassCode.Items2"), resources.GetString("comboLowMassCode.Items3"), resources.GetString("comboLowMassCode.Items4"), resources.GetString("comboLowMassCode.Items5"), resources.GetString("comboLowMassCode.Items6") });
            resources.ApplyResources(comboLowMassCode, "comboLowMassCode");
            comboLowMassCode.Name = "comboLowMassCode";
            comboLowMassCode.SelectedIndexChanged += comboLowMassCode_SelectedIndexChanged;
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            tableConfig.SetColumnSpan(label6, 2);
            label6.Name = "label6";
            // 
            // btnPasteTopBoxel
            // 
            btnPasteTopBoxel.Image = Properties.ImageResources.paste1;
            resources.ApplyResources(btnPasteTopBoxel, "btnPasteTopBoxel");
            btnPasteTopBoxel.Name = "btnPasteTopBoxel";
            btnPasteTopBoxel.UseVisualStyleBackColor = true;
            btnPasteTopBoxel.Click += btnPasteTopBoxel_Click;
            // 
            // labelBoxelCount
            // 
            resources.ApplyResources(labelBoxelCount, "labelBoxelCount");
            labelBoxelCount.Name = "labelBoxelCount";
            // 
            // btnConfigCancel
            // 
            resources.ApplyResources(btnConfigCancel, "btnConfigCancel");
            btnConfigCancel.Name = "btnConfigCancel";
            btnConfigCancel.UseVisualStyleBackColor = true;
            // 
            // dateStart
            // 
            resources.ApplyResources(dateStart, "dateStart");
            dateStart.Name = "dateStart";
            // 
            // FormBoxelSearch
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(status);
            Controls.Add(tableTop);
            Controls.Add(tableConfig);
            Name = "FormBoxelSearch";
            contextList.ResumeLayout(false);
            status.ResumeLayout(false);
            status.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).EndInit();
            panelList.ResumeLayout(false);
            panelList.PerformLayout();
            tableTop.ResumeLayout(false);
            tableTop.PerformLayout();
            tableConfig.ResumeLayout(false);
            tableConfig.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtTopBoxel;
        private Button btnConfig;
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
        private TableLayoutPanel tableConfig;
        private Button btnBegin;
        private Label label4;
        private CheckBox checkCompleteOnEnterSystem;
        private CheckBox checkCompleteOnFssAllBodies;
        private Label label5;
        private ComboBox comboLowMassCode;
        private Button btnPasteTopBoxel;
        private Label label6;
        private Label labelBoxelCount;
        private ContextMenuStrip contextList;
        private ToolStripMenuItem menuListCopySystemName;
        private Button btnConfigCancel;
        private DateTimePicker dateStart;
    }
}