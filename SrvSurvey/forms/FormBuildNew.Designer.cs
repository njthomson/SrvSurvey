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
            label3 = new Label();
            btnAccept = new Button();
            txtJson = new TextBox();
            btnJoin = new Button();
            label4 = new Label();
            textSystem = new TextBox();
            lblNot = new Label();
            txtName = new TextBox();
            label5 = new Label();
            txtArchitect = new TextBox();
            label6 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(69, 63);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 5;
            label1.Text = "System:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 92);
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
            comboBuildType.Location = new Point(123, 89);
            comboBuildType.Name = "comboBuildType";
            comboBuildType.Size = new Size(329, 23);
            comboBuildType.TabIndex = 8;
            comboBuildType.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 176);
            label3.Margin = new Padding(3);
            label3.Name = "label3";
            label3.Size = new Size(124, 15);
            label3.TabIndex = 0;
            label3.Text = "Commodities needed:";
            // 
            // btnAccept
            // 
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAccept.Location = new Point(377, 475);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(75, 23);
            btnAccept.TabIndex = 3;
            btnAccept.Text = "Create";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // txtJson
            // 
            txtJson.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtJson.Location = new Point(12, 197);
            txtJson.Multiline = true;
            txtJson.Name = "txtJson";
            txtJson.ScrollBars = ScrollBars.Vertical;
            txtJson.Size = new Size(440, 272);
            txtJson.TabIndex = 1;
            // 
            // btnJoin
            // 
            btnJoin.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnJoin.Location = new Point(12, 475);
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
            label4.Size = new Size(440, 51);
            label4.TabIndex = 4;
            label4.Text = "Create a build project only when docked at the Colonization ship. Only one person needs to do this.\r\nMake sure the numbers below match what is actually needed.";
            // 
            // textSystem
            // 
            textSystem.Location = new Point(123, 63);
            textSystem.Name = "textSystem";
            textSystem.ReadOnly = true;
            textSystem.Size = new Size(329, 23);
            textSystem.TabIndex = 9;
            // 
            // lblNot
            // 
            lblNot.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblNot.AutoSize = true;
            lblNot.Location = new Point(108, 479);
            lblNot.Name = "lblNot";
            lblNot.Size = new Size(207, 15);
            lblNot.TabIndex = 10;
            lblNot.Text = "Not at Colonization Construction ship";
            // 
            // txtName
            // 
            txtName.Location = new Point(123, 118);
            txtName.Name = "txtName";
            txtName.Size = new Size(329, 23);
            txtName.TabIndex = 12;
            txtName.Text = "primary-port";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(37, 121);
            label5.Name = "label5";
            label5.Size = new Size(80, 15);
            label5.TabIndex = 11;
            label5.Text = "Project name:";
            // 
            // txtArchitect
            // 
            txtArchitect.Location = new Point(123, 147);
            txtArchitect.Name = "txtArchitect";
            txtArchitect.Size = new Size(329, 23);
            txtArchitect.TabIndex = 14;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(59, 150);
            label6.Name = "label6";
            label6.Size = new Size(58, 15);
            label6.TabIndex = 13;
            label6.Text = "Architect:";
            // 
            // FormBuildNew
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(465, 503);
            Controls.Add(txtArchitect);
            Controls.Add(label6);
            Controls.Add(txtName);
            Controls.Add(label5);
            Controls.Add(lblNot);
            Controls.Add(textSystem);
            Controls.Add(label4);
            Controls.Add(btnJoin);
            Controls.Add(txtJson);
            Controls.Add(btnAccept);
            Controls.Add(label3);
            Controls.Add(comboBuildType);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "FormBuildNew";
            Text = "New Build Project";
            Load += FormBuildNew_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox comboBuildType;
        private Label label3;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Commodity;
        private DataGridViewTextBoxColumn Needed;
        private Button btnAccept;
        private TextBox txtJson;
        private Button btnJoin;
        private Label label4;
        private TextBox textSystem;
        private Label lblNot;
        private TextBox txtName;
        private Label label5;
        private TextBox txtArchitect;
        private Label label6;
    }
}