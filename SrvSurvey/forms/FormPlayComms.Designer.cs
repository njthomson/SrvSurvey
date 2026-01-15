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
            btnDev = new DrawButton();
            btnWatch = new DrawButton();
            btnClose = new DrawButton();
            tlist.SuspendLayout();
            bigPanel.SuspendLayout();
            SuspendLayout();
            // 
            // btnQuests
            // 
            btnQuests.BackColor = Color.FromArgb(255, 128, 0);
            btnQuests.ForeColor = Color.Black;
            btnQuests.Location = new Point(12, 48);
            btnQuests.Name = "btnQuests";
            btnQuests.Size = new Size(72, 72);
            btnQuests.TabIndex = 0;
            btnQuests.UseVisualStyleBackColor = false;
            btnQuests.Click += btnQuests_Click;
            btnQuests.Paint += btnQuests_Paint;
            btnQuests.Enter += leftButtons_Enter;
            // 
            // btnMsgs
            // 
            btnMsgs.BackColor = Color.FromArgb(255, 128, 0);
            btnMsgs.ForeColor = Color.Black;
            btnMsgs.Location = new Point(12, 148);
            btnMsgs.Name = "btnMsgs";
            btnMsgs.Size = new Size(72, 72);
            btnMsgs.TabIndex = 1;
            btnMsgs.UseMnemonic = false;
            btnMsgs.UseVisualStyleBackColor = false;
            btnMsgs.Click += btnMsgs_Click;
            btnMsgs.Paint += btnMsgs_Paint;
            btnMsgs.Enter += leftButtons_Enter;
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
            tlist.Size = new Size(688, 340);
            tlist.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(192, 192, 255);
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(682, 24);
            panel2.TabIndex = 0;
            // 
            // bigPanel
            // 
            bigPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            bigPanel.Controls.Add(tlist);
            bigPanel.Location = new Point(100, 48);
            bigPanel.Name = "bigPanel";
            bigPanel.Size = new Size(688, 340);
            bigPanel.TabIndex = 2;
            // 
            // btnDev
            // 
            btnDev.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDev.ForeColor = Color.Black;
            btnDev.Location = new Point(12, 306);
            btnDev.Name = "btnDev";
            btnDev.Size = new Size(72, 23);
            btnDev.TabIndex = 3;
            btnDev.Text = "dev";
            btnDev.UseVisualStyleBackColor = true;
            btnDev.Click += btnDev_Click;
            btnDev.Enter += leftButtons_Enter;
            // 
            // btnWatch
            // 
            btnWatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnWatch.Enabled = false;
            btnWatch.ForeColor = Color.Black;
            btnWatch.Location = new Point(12, 277);
            btnWatch.Name = "btnWatch";
            btnWatch.Size = new Size(72, 23);
            btnWatch.TabIndex = 2;
            btnWatch.Text = "watch";
            btnWatch.UseVisualStyleBackColor = true;
            btnWatch.Click += btnWatch_Click;
            btnWatch.Enter += leftButtons_Enter;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClose.ForeColor = Color.Black;
            btnClose.Location = new Point(12, 365);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(72, 23);
            btnClose.TabIndex = 4;
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            btnClose.Paint += btnClose_Paint;
            btnClose.Enter += leftButtons_Enter;
            // 
            // FormPlayComms
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(800, 400);
            ControlBox = false;
            Controls.Add(btnClose);
            Controls.Add(btnWatch);
            Controls.Add(btnDev);
            Controls.Add(btnMsgs);
            Controls.Add(btnQuests);
            Controls.Add(bigPanel);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(800, 400);
            Name = "FormPlayComms";
            ShowIcon = false;
            Text = "Quests";
            Activated += FormPlayComms_Activated;
            Paint += FormPlayComms_Paint;
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
        private DrawButton btnDev;
        private DrawButton btnWatch;
        private DrawButton btnClose;
    }
}