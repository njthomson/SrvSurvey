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
            txtConfigBoxel = new TextBox2();
            btnConfig = new FlatButton();
            lblMaxNum = new Label();
            txtCurrent = new TextBox2();
            btnCopyNext = new FlatButton();
            list = new ListView();
            colSystem = new ColumnHeader();
            colDistance = new ColumnHeader();
            colCompleted = new ColumnHeader();
            colSpansh = new ColumnHeader();
            contextList = new ContextMenuStrip(components);
            menuListCopySystemName = new ToolStripMenuItem();
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
            label1 = new Label();
            txtMainBoxel = new TextBox2();
            btnParent = new FlatButton();
            label3 = new Label();
            flowCommands = new FlowLayoutPanel();
            btnBoxelEmpty = new FlatButton();
            label7 = new Label();
            checkSkipVisited = new CheckBox();
            checkSpinKnownToSpansh = new CheckBox();
            menuSiblings = new ButtonContextMenuStrip(components);
            tableConfig = new TableLayoutPanel();
            lblConfigHeader = new Label();
            label5 = new Label();
            label6 = new Label();
            btnBegin = new FlatButton();
            btnConfigNav = new FlatButton();
            panelGraphic = new Panel();
            labelGraphic = new Label();
            btnConfigCancel = new FlatButton();
            checkCompleteOnFssAllBodies = new CheckBox();
            label4 = new Label();
            checkCompleteOnEnterSystem = new CheckBox();
            dateStart = new DateTimePicker();
            comboLowMassCode = new ComboBox();
            lblBadBoxel = new Label();
            lblBoxelCount = new Label();
            contextList.SuspendLayout();
            status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).BeginInit();
            panelList.SuspendLayout();
            tableTop.SuspendLayout();
            flowCommands.SuspendLayout();
            tableConfig.SuspendLayout();
            panelGraphic.SuspendLayout();
            SuspendLayout();
            // 
            // txtConfigBoxel
            // 
            resources.ApplyResources(txtConfigBoxel, "txtConfigBoxel");
            txtConfigBoxel.BackColor = SystemColors.ControlLight;
            txtConfigBoxel.BorderStyle = BorderStyle.FixedSingle;
            tableConfig.SetColumnSpan(txtConfigBoxel, 4);
            txtConfigBoxel.ForeColor = SystemColors.WindowText;
            txtConfigBoxel.Name = "txtConfigBoxel";
            txtConfigBoxel.UseEdgeButton = TextBox2.EdgeButton.None;
            txtConfigBoxel.TextChanged += txtConfigBoxel_TextChanged;
            // 
            // btnConfig
            // 
            resources.ApplyResources(btnConfig, "btnConfig");
            btnConfig.Name = "btnConfig";
            tableTop.SetRowSpan(btnConfig, 2);
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
            txtCurrent.BackColor = SystemColors.ScrollBar;
            txtCurrent.BorderStyle = BorderStyle.FixedSingle;
            tableTop.SetColumnSpan(txtCurrent, 4);
            txtCurrent.ForeColor = SystemColors.WindowText;
            txtCurrent.Name = "txtCurrent";
            txtCurrent.ReadOnly = true;
            txtCurrent.UseEdgeButton = TextBox2.EdgeButton.None;
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
            list.BackColor = SystemColors.WindowFrame;
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colSystem, colDistance, colCompleted, colSpansh });
            list.ContextMenuStrip = contextList;
            list.ForeColor = SystemColors.Info;
            list.FullRowSelect = true;
            list.Name = "list";
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ColumnClick += list_ColumnClick;
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
            // colCompleted
            // 
            resources.ApplyResources(colCompleted, "colCompleted");
            // 
            // colSpansh
            // 
            resources.ApplyResources(colSpansh, "colSpansh");
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
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // comboFrom
            // 
            resources.ApplyResources(comboFrom, "comboFrom");
            comboFrom.BackColor = SystemColors.ScrollBar;
            comboFrom.FormattingEnabled = true;
            comboFrom.Name = "comboFrom";
            comboFrom.SelectedSystem = null;
            comboFrom.updateOnJump = false;
            // 
            // status
            // 
            status.BackColor = SystemColors.ControlDark;
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
            lblStatus.BorderStyle = Border3DStyle.SunkenInner;
            lblStatus.Margin = new Padding(0, 1, 0, 2);
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
            checkAutoCopy.CheckedChanged += checkAutoCopy_CheckedChanged;
            // 
            // numMax
            // 
            resources.ApplyResources(numMax, "numMax");
            numMax.BackColor = SystemColors.ScrollBar;
            numMax.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numMax.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMax.Name = "numMax";
            numMax.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numMax.ValueChanged += numMax_ValueChanged;
            // 
            // panelList
            // 
            resources.ApplyResources(panelList, "panelList");
            tableTop.SetColumnSpan(panelList, 7);
            panelList.Controls.Add(comboFrom);
            panelList.Controls.Add(label2);
            panelList.Controls.Add(list);
            panelList.Name = "panelList";
            // 
            // tableTop
            // 
            resources.ApplyResources(tableTop, "tableTop");
            tableTop.Controls.Add(panelList, 0, 3);
            tableTop.Controls.Add(txtCurrent, 2, 1);
            tableTop.Controls.Add(label1, 0, 0);
            tableTop.Controls.Add(txtMainBoxel, 2, 0);
            tableTop.Controls.Add(btnParent, 1, 1);
            tableTop.Controls.Add(label3, 0, 4);
            tableTop.Controls.Add(flowCommands, 0, 2);
            tableTop.Controls.Add(btnConfig, 6, 0);
            tableTop.Controls.Add(label7, 0, 1);
            tableTop.Name = "tableTop";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            tableTop.SetColumnSpan(label1, 2);
            label1.Name = "label1";
            // 
            // txtMainBoxel
            // 
            resources.ApplyResources(txtMainBoxel, "txtMainBoxel");
            txtMainBoxel.BackColor = SystemColors.ScrollBar;
            txtMainBoxel.BorderStyle = BorderStyle.FixedSingle;
            tableTop.SetColumnSpan(txtMainBoxel, 4);
            txtMainBoxel.ForeColor = SystemColors.WindowText;
            txtMainBoxel.Name = "txtMainBoxel";
            txtMainBoxel.ReadOnly = true;
            txtMainBoxel.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // btnParent
            // 
            resources.ApplyResources(btnParent, "btnParent");
            btnParent.Name = "btnParent";
            btnParent.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            tableTop.SetColumnSpan(label3, 7);
            label3.FlatStyle = FlatStyle.System;
            label3.Name = "label3";
            // 
            // flowCommands
            // 
            resources.ApplyResources(flowCommands, "flowCommands");
            tableTop.SetColumnSpan(flowCommands, 7);
            flowCommands.Controls.Add(checkAutoCopy);
            flowCommands.Controls.Add(btnCopyNext);
            flowCommands.Controls.Add(btnBoxelEmpty);
            flowCommands.Controls.Add(lblMaxNum);
            flowCommands.Controls.Add(numMax);
            flowCommands.Name = "flowCommands";
            // 
            // btnBoxelEmpty
            // 
            resources.ApplyResources(btnBoxelEmpty, "btnBoxelEmpty");
            btnBoxelEmpty.Name = "btnBoxelEmpty";
            btnBoxelEmpty.UseVisualStyleBackColor = true;
            btnBoxelEmpty.Click += btnBoxelEmpty_Click;
            // 
            // label7
            // 
            resources.ApplyResources(label7, "label7");
            label7.Name = "label7";
            // 
            // checkSkipVisited
            // 
            resources.ApplyResources(checkSkipVisited, "checkSkipVisited");
            tableConfig.SetColumnSpan(checkSkipVisited, 3);
            checkSkipVisited.Name = "checkSkipVisited";
            checkSkipVisited.UseVisualStyleBackColor = true;
            // 
            // checkSpinKnownToSpansh
            // 
            resources.ApplyResources(checkSpinKnownToSpansh, "checkSpinKnownToSpansh");
            tableConfig.SetColumnSpan(checkSpinKnownToSpansh, 3);
            checkSpinKnownToSpansh.Name = "checkSpinKnownToSpansh";
            checkSpinKnownToSpansh.UseVisualStyleBackColor = true;
            // 
            // menuSiblings
            // 
            menuSiblings.Name = "menuSiblings";
            resources.ApplyResources(menuSiblings, "menuSiblings");
            menuSiblings.targetButton = null;
            menuSiblings.ItemClicked += menuSiblings_ItemClicked;
            // 
            // tableConfig
            // 
            resources.ApplyResources(tableConfig, "tableConfig");
            tableConfig.Controls.Add(lblConfigHeader, 0, 0);
            tableConfig.Controls.Add(txtConfigBoxel, 0, 1);
            tableConfig.Controls.Add(checkSkipVisited, 0, 4);
            tableConfig.Controls.Add(checkSpinKnownToSpansh, 0, 5);
            tableConfig.Controls.Add(label5, 0, 3);
            tableConfig.Controls.Add(label6, 0, 6);
            tableConfig.Controls.Add(btnBegin, 2, 9);
            tableConfig.Controls.Add(btnConfigNav, 4, 1);
            tableConfig.Controls.Add(panelGraphic, 0, 10);
            tableConfig.Controls.Add(btnConfigCancel, 3, 9);
            tableConfig.Controls.Add(checkCompleteOnFssAllBodies, 1, 8);
            tableConfig.Controls.Add(label4, 0, 7);
            tableConfig.Controls.Add(checkCompleteOnEnterSystem, 0, 8);
            tableConfig.Controls.Add(dateStart, 1, 6);
            tableConfig.Controls.Add(comboLowMassCode, 1, 3);
            tableConfig.Controls.Add(lblBadBoxel, 0, 2);
            tableConfig.Controls.Add(lblBoxelCount, 2, 3);
            tableConfig.Name = "tableConfig";
            // 
            // lblConfigHeader
            // 
            resources.ApplyResources(lblConfigHeader, "lblConfigHeader");
            tableConfig.SetColumnSpan(lblConfigHeader, 3);
            lblConfigHeader.Name = "lblConfigHeader";
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            label6.Name = "label6";
            // 
            // btnBegin
            // 
            resources.ApplyResources(btnBegin, "btnBegin");
            btnBegin.Name = "btnBegin";
            btnBegin.UseVisualStyleBackColor = true;
            btnBegin.Click += btnBegin_Click;
            // 
            // btnConfigNav
            // 
            resources.ApplyResources(btnConfigNav, "btnConfigNav");
            btnConfigNav.Name = "btnConfigNav";
            btnConfigNav.UseVisualStyleBackColor = true;
            // 
            // panelGraphic
            // 
            resources.ApplyResources(panelGraphic, "panelGraphic");
            panelGraphic.BackColor = SystemColors.ScrollBar;
            panelGraphic.BorderStyle = BorderStyle.FixedSingle;
            tableConfig.SetColumnSpan(panelGraphic, 5);
            panelGraphic.Controls.Add(labelGraphic);
            panelGraphic.Name = "panelGraphic";
            panelGraphic.SizeChanged += panelGraphic_SizeChanged;
            panelGraphic.Paint += panelGraphic_Paint;
            // 
            // labelGraphic
            // 
            resources.ApplyResources(labelGraphic, "labelGraphic");
            labelGraphic.Name = "labelGraphic";
            // 
            // btnConfigCancel
            // 
            resources.ApplyResources(btnConfigCancel, "btnConfigCancel");
            tableConfig.SetColumnSpan(btnConfigCancel, 2);
            btnConfigCancel.Name = "btnConfigCancel";
            btnConfigCancel.UseVisualStyleBackColor = true;
            btnConfigCancel.Click += btnConfigCancel_Click;
            // 
            // checkCompleteOnFssAllBodies
            // 
            resources.ApplyResources(checkCompleteOnFssAllBodies, "checkCompleteOnFssAllBodies");
            tableConfig.SetColumnSpan(checkCompleteOnFssAllBodies, 2);
            checkCompleteOnFssAllBodies.Name = "checkCompleteOnFssAllBodies";
            checkCompleteOnFssAllBodies.UseVisualStyleBackColor = true;
            checkCompleteOnFssAllBodies.CheckedChanged += checkCompleteOnFssAllBodies_CheckedChanged;
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
            // dateStart
            // 
            resources.ApplyResources(dateStart, "dateStart");
            tableConfig.SetColumnSpan(dateStart, 2);
            dateStart.Name = "dateStart";
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
            // lblBadBoxel
            // 
            resources.ApplyResources(lblBadBoxel, "lblBadBoxel");
            tableConfig.SetColumnSpan(lblBadBoxel, 5);
            lblBadBoxel.Name = "lblBadBoxel";
            // 
            // lblBoxelCount
            // 
            resources.ApplyResources(lblBoxelCount, "lblBoxelCount");
            tableConfig.SetColumnSpan(lblBoxelCount, 2);
            lblBoxelCount.Name = "lblBoxelCount";
            // 
            // FormBoxelSearch
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
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
            flowCommands.ResumeLayout(false);
            flowCommands.PerformLayout();
            tableConfig.ResumeLayout(false);
            tableConfig.PerformLayout();
            panelGraphic.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox2 txtConfigBoxel;
        private FlatButton btnConfig;
        private Label lblMaxNum;
        private TextBox2 txtCurrent;
        private FlatButton btnCopyNext;
        private ListView list;
        private ColumnHeader colSystem;
        private ColumnHeader colDistance;
        private Label label2;
        private ComboStarSystem comboFrom;
        private ColumnHeader colCompleted;
        private StatusStrip status;
        private CheckBox checkAutoCopy;
        private NumericUpDown numMax;
        private Panel panelList;
        private ToolStripSplitButton btnToggleList;
        private ToolStripStatusLabel lblStatus;
        private ToolStripDropDownButton menuMore;
        private ToolStripMenuItem menuHelpLink;
        private TableLayoutPanel tableTop;
        private FlatButton btnParent;
        private ButtonContextMenuStrip menuSiblings;
        private FlatButton btnBoxelEmpty;
        private CheckBox checkSkipVisited;
        private CheckBox checkSpinKnownToSpansh;
        private Label label3;
        private TableLayoutPanel tableConfig;
        private FlatButton btnBegin;
        private Label label4;
        private CheckBox checkCompleteOnEnterSystem;
        private CheckBox checkCompleteOnFssAllBodies;
        private Label label5;
        private ComboBox comboLowMassCode;
        private Label lblConfigHeader;
        private Label lblBoxelCount;
        private ContextMenuStrip contextList;
        private ToolStripMenuItem menuListCopySystemName;
        private FlatButton btnConfigCancel;
        private DateTimePicker dateStart;
        private FlatButton btnConfigNav;
        private Label label6;
        private Label label1;
        private TextBox2 txtMainBoxel;
        private Panel panelGraphic;
        private Label labelGraphic;
        private FlowLayoutPanel flowCommands;
        private Label lblBadBoxel;
        private ColumnHeader colSpansh;
        private Label label7;
    }
}