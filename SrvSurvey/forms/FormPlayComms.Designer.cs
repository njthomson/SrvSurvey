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
            btnQuests = new DrawButton();
            btnMsgs = new DrawButton();
            tlist = new TableLayoutPanel();
            panel2 = new Panel();
            bigPanel = new Panel();
            tlist.SuspendLayout();
            bigPanel.SuspendLayout();
            SuspendLayout();
            // 
            // btnQuests
            // 
            btnQuests.ForeColor = Color.Black;
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
            btnMsgs.ForeColor = Color.Black;
            btnMsgs.Location = new Point(8, 41);
            btnMsgs.Name = "btnMsgs";
            btnMsgs.Size = new Size(75, 23);
            btnMsgs.TabIndex = 1;
            btnMsgs.Text = "Msgs";
            btnMsgs.UseVisualStyleBackColor = true;
            btnMsgs.Click += btnMsgs_Click;
            // 
            // tlist
            // 
            tlist.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlist.ColumnCount = 1;
            tlist.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlist.Controls.Add(panel2, 0, 1);
            tlist.Dock = DockStyle.Fill;
            tlist.Location = new Point(0, 0);
            tlist.Name = "tlist";
            tlist.RowCount = 4;
            tlist.RowStyles.Add(new RowStyle());
            tlist.RowStyles.Add(new RowStyle());
            tlist.RowStyles.Add(new RowStyle());
            tlist.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlist.Size = new Size(782, 442);
            tlist.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(192, 192, 255);
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(776, 24);
            panel2.TabIndex = 0;
            // 
            // bigPanel
            // 
            bigPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            bigPanel.Controls.Add(tlist);
            bigPanel.Location = new Point(135, 12);
            bigPanel.Name = "bigPanel";
            bigPanel.Size = new Size(782, 442);
            bigPanel.TabIndex = 2;
            // 
            // FormPlayComms
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(929, 466);
            Controls.Add(btnMsgs);
            Controls.Add(btnQuests);
            Controls.Add(bigPanel);
            Name = "FormPlayComms";
            Text = "FormPlayComms";
            tlist.ResumeLayout(false);
            bigPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void BtnQuests_KeyPress(object sender, KeyPressEventArgs e)
        {
            throw new NotImplementedException();
        }


        #endregion

        private DrawButton btnQuests;
        private DrawButton btnMsgs;
        private TableLayoutPanel tlist;
        private Panel panel2;
        private Panel bigPanel;
    }
}