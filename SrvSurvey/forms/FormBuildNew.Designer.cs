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
            label1 = new Label();
            label2 = new Label();
            comboBuildType = new ComboBox();
            btnAccept = new Button();
            btnJoin = new Button();
            label4 = new Label();
            lblNot = new Label();
            txtName = new TextBox();
            label5 = new Label();
            txtArchitect = new TextBox();
            label6 = new Label();
            btnAssign = new Button();
            linkLink = new LinkLabel();
            table = new TableLayoutPanel();
            textBox1 = new TextBox();
            panelList = new Panel();
            table.SuspendLayout();
            panelList.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(68, 66);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 5;
            label1.Text = "System:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 153);
            label2.Name = "label2";
            label2.Size = new Size(105, 15);
            label2.TabIndex = 7;
            label2.Text = "Construction type:";
            // 
            // comboBuildType
            // 
            comboBuildType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBuildType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuildType.FormattingEnabled = true;
            comboBuildType.Location = new Point(124, 150);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(215, 23);
            comboBuildType.TabIndex = 8;
            comboBuildType.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // btnAccept
            // 
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAccept.Location = new Point(396, 471);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(75, 23);
            btnAccept.TabIndex = 3;
            btnAccept.Text = "Create";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // btnJoin
            // 
            btnJoin.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnJoin.Location = new Point(12, 471);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(75, 23);
            btnJoin.TabIndex = 2;
            btnJoin.Text = "Contribute";
            btnJoin.UseVisualStyleBackColor = true;
            btnJoin.Click += button1_Click;
            // 
            // label4
            // 
            label4.Location = new Point(13, 9);
            label4.Name = "label4";
            label4.Size = new Size(458, 35);
            label4.TabIndex = 4;
            label4.Text = "Add or edit a build project when docked at the Colonization ship. Numbers will need to be checked when creating. Follow the link to track progress online.";
            // 
            // lblNot
            // 
            lblNot.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblNot.AutoSize = true;
            lblNot.Location = new Point(123, 475);
            lblNot.Name = "lblNot";
            lblNot.Size = new Size(207, 15);
            lblNot.TabIndex = 10;
            lblNot.Text = "Not at Colonization Construction ship";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(123, 92);
            txtName.Name = "txtName";
            txtName.Size = new Size(348, 23);
            txtName.TabIndex = 12;
            txtName.Text = "primary-port";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(37, 95);
            label5.Name = "label5";
            label5.Size = new Size(80, 15);
            label5.TabIndex = 11;
            label5.Text = "Project name:";
            // 
            // txtArchitect
            // 
            txtArchitect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtArchitect.Location = new Point(123, 121);
            txtArchitect.Name = "txtArchitect";
            txtArchitect.Size = new Size(348, 23);
            txtArchitect.TabIndex = 14;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(59, 124);
            label6.Name = "label6";
            label6.Size = new Size(58, 15);
            label6.TabIndex = 13;
            label6.Text = "Architect:";
            // 
            // btnAssign
            // 
            btnAssign.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAssign.Location = new Point(346, 150);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(126, 23);
            btnAssign.TabIndex = 15;
            btnAssign.Text = "Assign Commodity";
            btnAssign.UseVisualStyleBackColor = true;
            btnAssign.Click += btnAssign_Click;
            // 
            // linkLink
            // 
            linkLink.BorderStyle = BorderStyle.FixedSingle;
            linkLink.Location = new Point(123, 65);
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
            table.Size = new Size(452, 245);
            table.TabIndex = 17;
            table.Paint += table_Paint;
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
            panelList.Location = new Point(12, 179);
            panelList.Name = "panelList";
            panelList.Size = new Size(460, 286);
            panelList.TabIndex = 18;
            // 
            // FormBuildNew
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 499);
            Controls.Add(panelList);
            Controls.Add(linkLink);
            Controls.Add(btnAssign);
            Controls.Add(txtArchitect);
            Controls.Add(label6);
            Controls.Add(txtName);
            Controls.Add(label5);
            Controls.Add(lblNot);
            Controls.Add(label4);
            Controls.Add(btnJoin);
            Controls.Add(btnAccept);
            Controls.Add(comboBuildType);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "FormBuildNew";
            Text = "New Build Project";
            Load += FormBuildNew_Load;
            table.ResumeLayout(false);
            table.PerformLayout();
            panelList.ResumeLayout(false);
            panelList.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox comboBuildType;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Commodity;
        private DataGridViewTextBoxColumn Needed;
        private Button btnAccept;
        private Button btnJoin;
        private Label label4;
        private Label lblNot;
        private TextBox txtName;
        private Label label5;
        private TextBox txtArchitect;
        private Label label6;
        private Button btnAssign;
        private LinkLabel linkLink;
        private TableLayoutPanel table;
        private Panel panelList;
        private TextBox textBox1;
    }
}