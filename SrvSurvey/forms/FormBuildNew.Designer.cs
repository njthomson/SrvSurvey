namespace SrvSurvey.forms
{
    partial class FormBuildNew
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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            label2 = new Label();
            comboBuildType = new ComboBox();
            btnAccept = new Button();
            label4 = new Label();
            txtName = new TextBox();
            label5 = new Label();
            txtArchitect = new TextBox();
            label6 = new Label();
            linkLink = new LinkLabel();
            table = new TableLayoutPanel();
            textBox1 = new TextBox();
            panelList = new Panel();
            comboProjects = new ComboBox();
            label3 = new Label();
            btnRefresh = new Button();
            txtNotes = new TextBox();
            listCmdrs = new ListBox();
            label7 = new Label();
            label8 = new Label();
            btnCmdrAdd = new Button();
            btnCmdrRemove = new Button();
            txtFaction = new TextBox();
            label9 = new Label();
            checkPrimary = new CheckBox();
            menuCommodities = new ContextMenuStrip(components);
            tagAssignCommanderToolStripMenuItem = new ToolStripMenuItem();
            panel1 = new Panel();
            linkRaven = new LinkLabel();
            table.SuspendLayout();
            panelList.SuspendLayout();
            menuCommodities.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(67, 138);
            label1.Name = "label1";
            label1.Size = new Size(53, 15);
            label1.TabIndex = 5;
            label1.Text = "Location";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 164);
            label2.Name = "label2";
            label2.Size = new Size(105, 15);
            label2.TabIndex = 7;
            label2.Text = "Construction type:";
            // 
            // comboBuildType
            // 
            comboBuildType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBuildType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildType.Enabled = false;
            comboBuildType.FormattingEnabled = true;
            comboBuildType.Location = new Point(124, 160);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(348, 23);
            comboBuildType.TabIndex = 8;
            comboBuildType.SelectedIndexChanged += comboBuildType_SelectedIndexChanged;
            // 
            // btnAccept
            // 
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAccept.Location = new Point(345, 514);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(128, 29);
            btnAccept.TabIndex = 3;
            btnAccept.Text = "Update Project";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // label4
            // 
            label4.Location = new Point(13, 9);
            label4.Name = "label4";
            label4.Size = new Size(462, 41);
            label4.TabIndex = 4;
            label4.Text = "Add or edit a build project when docked at the Colonisation ship. Numbers will need to be checked when creating. Follow the link to track progress online.";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(124, 83);
            txtName.Name = "txtName";
            txtName.Size = new Size(348, 23);
            txtName.TabIndex = 12;
            txtName.Text = "primary-port";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(38, 86);
            label5.Name = "label5";
            label5.Size = new Size(80, 15);
            label5.TabIndex = 11;
            label5.Text = "Project name:";
            // 
            // txtArchitect
            // 
            txtArchitect.Location = new Point(60, 13);
            txtArchitect.Name = "txtArchitect";
            txtArchitect.Size = new Size(348, 23);
            txtArchitect.TabIndex = 14;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(-4, 16);
            label6.Name = "label6";
            label6.Size = new Size(58, 15);
            label6.TabIndex = 13;
            label6.Text = "Architect:";
            // 
            // linkLink
            // 
            linkLink.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            linkLink.BorderStyle = BorderStyle.FixedSingle;
            linkLink.Location = new Point(124, 134);
            linkLink.Name = "linkLink";
            linkLink.Size = new Size(348, 23);
            linkLink.TabIndex = 16;
            linkLink.TabStop = true;
            linkLink.Text = "link";
            linkLink.TextAlign = ContentAlignment.MiddleLeft;
            linkLink.LinkClicked += linkLink_LinkClicked;
            // 
            // table
            // 
            table.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            table.AutoSize = true;
            table.ColumnCount = 2;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            table.Controls.Add(textBox1, 0, 0);
            table.Location = new Point(0, -1);
            table.Margin = new Padding(0, 0, 6, 0);
            table.Name = "table";
            table.RowCount = 1;
            table.RowStyles.Add(new RowStyle());
            table.Size = new Size(451, 245);
            table.TabIndex = 17;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(3, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 0;
            // 
            // panelList
            // 
            panelList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelList.AutoScroll = true;
            panelList.BorderStyle = BorderStyle.FixedSingle;
            panelList.Controls.Add(table);
            panelList.Location = new Point(13, 187);
            panelList.Name = "panelList";
            panelList.Size = new Size(459, 319);
            panelList.TabIndex = 18;
            // 
            // comboProjects
            // 
            comboProjects.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            comboProjects.FormattingEnabled = true;
            comboProjects.Location = new Point(124, 53);
            comboProjects.Name = "comboProjects";
            comboProjects.Size = new Size(286, 23);
            comboProjects.TabIndex = 19;
            comboProjects.SelectedIndexChanged += comboProjects_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(60, 56);
            label3.Name = "label3";
            label3.Size = new Size(52, 15);
            label3.TabIndex = 20;
            label3.Text = "Projects:";
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(416, 53);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(56, 23);
            btnRefresh.TabIndex = 22;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(60, 156);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(348, 240);
            txtNotes.TabIndex = 23;
            // 
            // listCmdrs
            // 
            listCmdrs.ItemHeight = 15;
            listCmdrs.Location = new Point(60, 71);
            listCmdrs.Name = "listCmdrs";
            listCmdrs.ScrollAlwaysVisible = true;
            listCmdrs.Size = new Size(348, 79);
            listCmdrs.Sorted = true;
            listCmdrs.TabIndex = 24;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(7, 156);
            label7.Name = "label7";
            label7.Size = new Size(41, 15);
            label7.TabIndex = 25;
            label7.Text = "Notes:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(-28, 71);
            label8.Name = "label8";
            label8.Size = new Size(82, 15);
            label8.TabIndex = 26;
            label8.Text = "Commanders:";
            // 
            // btnCmdrAdd
            // 
            btnCmdrAdd.Location = new Point(31, 89);
            btnCmdrAdd.Name = "btnCmdrAdd";
            btnCmdrAdd.Size = new Size(23, 23);
            btnCmdrAdd.TabIndex = 27;
            btnCmdrAdd.Text = "+";
            btnCmdrAdd.UseVisualStyleBackColor = true;
            btnCmdrAdd.Click += btnCmdrAdd_Click;
            // 
            // btnCmdrRemove
            // 
            btnCmdrRemove.Location = new Point(31, 118);
            btnCmdrRemove.Name = "btnCmdrRemove";
            btnCmdrRemove.Size = new Size(23, 23);
            btnCmdrRemove.TabIndex = 28;
            btnCmdrRemove.Text = "-";
            btnCmdrRemove.UseVisualStyleBackColor = true;
            btnCmdrRemove.Click += btnCmdrRemove_Click;
            // 
            // txtFaction
            // 
            txtFaction.Location = new Point(60, 42);
            txtFaction.Name = "txtFaction";
            txtFaction.Size = new Size(348, 23);
            txtFaction.TabIndex = 30;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(-26, 45);
            label9.Name = "label9";
            label9.Size = new Size(82, 15);
            label9.TabIndex = 29;
            label9.Text = "Minor faction:";
            // 
            // checkPrimary
            // 
            checkPrimary.AutoSize = true;
            checkPrimary.Location = new Point(124, 112);
            checkPrimary.Name = "checkPrimary";
            checkPrimary.Size = new Size(173, 19);
            checkPrimary.TabIndex = 31;
            checkPrimary.Text = "Primary/active build project";
            checkPrimary.UseVisualStyleBackColor = true;
            // 
            // menuCommodities
            // 
            menuCommodities.Items.AddRange(new ToolStripItem[] { tagAssignCommanderToolStripMenuItem });
            menuCommodities.Name = "menuCommodities";
            menuCommodities.RenderMode = ToolStripRenderMode.System;
            menuCommodities.ShowCheckMargin = true;
            menuCommodities.ShowImageMargin = false;
            menuCommodities.Size = new Size(206, 26);
            menuCommodities.Opening += menuCommodities_Opening;
            // 
            // tagAssignCommanderToolStripMenuItem
            // 
            tagAssignCommanderToolStripMenuItem.Enabled = false;
            tagAssignCommanderToolStripMenuItem.Name = "tagAssignCommanderToolStripMenuItem";
            tagAssignCommanderToolStripMenuItem.Size = new Size(205, 22);
            tagAssignCommanderToolStripMenuItem.Text = "Tag/Assign Commander:";
            // 
            // panel1
            // 
            panel1.Controls.Add(label6);
            panel1.Controls.Add(txtArchitect);
            panel1.Controls.Add(txtNotes);
            panel1.Controls.Add(txtFaction);
            panel1.Controls.Add(listCmdrs);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(btnCmdrRemove);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(btnCmdrAdd);
            panel1.Enabled = false;
            panel1.Location = new Point(13, 112);
            panel1.Name = "panel1";
            panel1.Size = new Size(39, 41);
            panel1.TabIndex = 32;
            panel1.Visible = false;
            // 
            // linkRaven
            // 
            linkRaven.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkRaven.AutoSize = true;
            linkRaven.Location = new Point(12, 521);
            linkRaven.Name = "linkRaven";
            linkRaven.Size = new Size(145, 15);
            linkRaven.TabIndex = 33;
            linkRaven.TabStop = true;
            linkRaven.Text = "https//:ravencolonial.com";
            linkRaven.LinkClicked += linkRaven_LinkClicked;
            // 
            // FormBuildNew
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 553);
            Controls.Add(linkRaven);
            Controls.Add(panel1);
            Controls.Add(checkPrimary);
            Controls.Add(btnRefresh);
            Controls.Add(label3);
            Controls.Add(comboProjects);
            Controls.Add(panelList);
            Controls.Add(linkLink);
            Controls.Add(txtName);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(btnAccept);
            Controls.Add(comboBuildType);
            Controls.Add(label2);
            Controls.Add(label1);
            MinimumSize = new Size(500, 400);
            Name = "FormBuildNew";
            Text = "Colony Build Projects";
            table.ResumeLayout(false);
            table.PerformLayout();
            panelList.ResumeLayout(false);
            panelList.PerformLayout();
            menuCommodities.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox comboBuildType;
        private Button btnAccept;
        private Label label4;
        private TextBox txtName;
        private Label label5;
        private TextBox txtArchitect;
        private Label label6;
        private LinkLabel linkLink;
        private TableLayoutPanel table;
        private Panel panelList;
        private TextBox textBox1;
        private ComboBox comboProjects;
        private Label label3;
        private Button btnRefresh;
        private TextBox txtNotes;
        private ListBox listCmdrs;
        private Label label7;
        private Label label8;
        private Button btnCmdrAdd;
        private Button btnCmdrRemove;
        private TextBox txtFaction;
        private Label label9;
        private CheckBox checkPrimary;
        private ContextMenuStrip menuCommodities;
        private ToolStripMenuItem tagAssignCommanderToolStripMenuItem;
        private Panel panel1;
        private LinkLabel linkRaven;
    }
}