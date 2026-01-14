namespace SrvSurvey.forms
{
    partial class FormPlayComms
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
            btnQuests = new Button();
            btnMsgs = new Button();
            list0 = new ListView();
            ID = new ColumnHeader();
            txtStuff = new TextBox();
            tlist = new TableLayoutPanel();
            panel3 = new Panel();
            panel2 = new Panel();
            panel1 = new Panel();
            tlist.SuspendLayout();
            SuspendLayout();
            // 
            // btnQuests
            // 
            btnQuests.Location = new Point(8, 12);
            btnQuests.Name = "btnQuests";
            btnQuests.Size = new Size(75, 23);
            btnQuests.TabIndex = 0;
            btnQuests.Text = "Quests";
            btnQuests.UseVisualStyleBackColor = true;
            btnQuests.Click += btnQuests_Click;
            // 
            // btnMsgs
            // 
            btnMsgs.Location = new Point(8, 41);
            btnMsgs.Name = "btnMsgs";
            btnMsgs.Size = new Size(75, 23);
            btnMsgs.TabIndex = 1;
            btnMsgs.Text = "Msgs";
            btnMsgs.UseVisualStyleBackColor = true;
            btnMsgs.Click += btnMsgs_Click;
            // 
            // list0
            // 
            list0.Columns.AddRange(new ColumnHeader[] { ID });
            list0.Location = new Point(89, 12);
            list0.Name = "list0";
            list0.Size = new Size(187, 97);
            list0.TabIndex = 3;
            list0.UseCompatibleStateImageBehavior = false;
            list0.View = View.Details;
            list0.SelectedIndexChanged += list0_SelectedIndexChanged;
            // 
            // ID
            // 
            ID.Text = "ID";
            ID.Width = 160;
            // 
            // txtStuff
            // 
            txtStuff.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtStuff.Location = new Point(282, 13);
            txtStuff.Multiline = true;
            txtStuff.Name = "txtStuff";
            txtStuff.ReadOnly = true;
            txtStuff.ScrollBars = ScrollBars.Both;
            txtStuff.Size = new Size(511, 96);
            txtStuff.TabIndex = 4;
            // 
            // tlist
            // 
            tlist.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tlist.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlist.ColumnCount = 1;
            tlist.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlist.Controls.Add(panel3, 0, 2);
            tlist.Controls.Add(panel2, 0, 1);
            tlist.Controls.Add(panel1, 0, 0);
            tlist.Location = new Point(89, 115);
            tlist.Name = "tlist";
            tlist.RowCount = 4;
            tlist.RowStyles.Add(new RowStyle());
            tlist.RowStyles.Add(new RowStyle());
            tlist.RowStyles.Add(new RowStyle());
            tlist.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlist.Size = new Size(704, 188);
            tlist.TabIndex = 5;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = Color.FromArgb(192, 255, 192);
            panel3.Location = new Point(3, 112);
            panel3.Name = "panel3";
            panel3.Size = new Size(698, 24);
            panel3.TabIndex = 7;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(192, 192, 255);
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Location = new Point(3, 82);
            panel2.Name = "panel2";
            panel2.Size = new Size(698, 24);
            panel2.TabIndex = 7;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(192, 255, 255);
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(698, 73);
            panel1.TabIndex = 6;
            // 
            // FormPlayComms
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(805, 338);
            Controls.Add(tlist);
            Controls.Add(txtStuff);
            Controls.Add(list0);
            Controls.Add(btnMsgs);
            Controls.Add(btnQuests);
            Name = "FormPlayComms";
            Text = "FormPlayComms";
            tlist.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnQuests;
        private Button btnMsgs;
        private ListView list0;
        private TextBox txtStuff;
        private ColumnHeader ID;
        private TableLayoutPanel tlist;
        private Panel panel3;
        private Panel panel2;
        private Panel panel1;
    }
}