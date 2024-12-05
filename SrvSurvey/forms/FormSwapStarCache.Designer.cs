namespace SrvSurvey.forms
{
    partial class FormSwapStarCache
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
            linkEDGalaxy = new LinkLabel();
            btnNo = new FlatButton();
            btnYes = new FlatButton();
            label2 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            comboCmdrs = new ComboCmdr();
            comboSystem = new ComboStarSystem();
            label3 = new Label();
            lblCloseWarning = new Label();
            btnRestore = new FlatButton();
            timer = new System.Windows.Forms.Timer(components);
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label1, 4);
            label1.Location = new Point(6, 3);
            label1.Margin = new Padding(3, 0, 3, 3);
            label1.Name = "label1";
            label1.Size = new Size(533, 45);
            label1.TabIndex = 0;
            label1.Text = "This tool will backup and swap your current VisitedStarsCache.dat with one generated with known systems around your current location. The game cannot be running when the swap happens.\r\n\r\n";
            // 
            // linkEDGalaxy
            // 
            linkEDGalaxy.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(linkEDGalaxy, 2);
            linkEDGalaxy.LinkArea = new LinkArea(24, 40);
            linkEDGalaxy.Location = new Point(89, 112);
            linkEDGalaxy.Margin = new Padding(3, 3, 3, 0);
            linkEDGalaxy.Name = "linkEDGalaxy";
            linkEDGalaxy.Size = new Size(310, 21);
            linkEDGalaxy.TabIndex = 5;
            linkEDGalaxy.TabStop = true;
            linkEDGalaxy.Text = "&Data is generated from: https://edgalaxy.net/visitedstars";
            linkEDGalaxy.UseCompatibleTextRendering = true;
            linkEDGalaxy.LinkClicked += linkEDGalaxy_LinkClicked;
            // 
            // btnNo
            // 
            btnNo.Anchor = AnchorStyles.Left;
            btnNo.Location = new Point(464, 146);
            btnNo.Name = "btnNo";
            btnNo.Size = new Size(75, 23);
            btnNo.TabIndex = 9;
            btnNo.Text = "&Close";
            btnNo.UseVisualStyleBackColor = true;
            btnNo.Click += btnNo_Click;
            // 
            // btnYes
            // 
            btnYes.Anchor = AnchorStyles.Right;
            btnYes.Location = new Point(383, 146);
            btnYes.Name = "btnYes";
            btnYes.Size = new Size(75, 23);
            btnYes.TabIndex = 8;
            btnYes.Text = "&Swap";
            btnYes.UseVisualStyleBackColor = true;
            btnYes.Click += btnYes_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(6, 58);
            label2.Name = "label2";
            label2.Size = new Size(77, 15);
            label2.TabIndex = 1;
            label2.Text = "C&ommander:";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(btnNo, 3, 4);
            tableLayoutPanel1.Controls.Add(btnYes, 2, 4);
            tableLayoutPanel1.Controls.Add(lblCloseWarning, 1, 4);
            tableLayoutPanel1.Controls.Add(linkEDGalaxy, 1, 3);
            tableLayoutPanel1.Controls.Add(btnRestore, 0, 4);
            tableLayoutPanel1.Controls.Add(comboCmdrs, 1, 1);
            tableLayoutPanel1.Controls.Add(comboSystem, 1, 2);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(3);
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(545, 186);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // comboCmdrs
            // 
            comboCmdrs.cmdrFid = null;
            tableLayoutPanel1.SetColumnSpan(comboCmdrs, 3);
            comboCmdrs.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCmdrs.FormattingEnabled = true;
            comboCmdrs.Location = new Point(89, 54);
            comboCmdrs.Name = "comboCmdrs";
            comboCmdrs.Size = new Size(450, 23);
            comboCmdrs.TabIndex = 11;
            // 
            // comboSystem
            // 
            tableLayoutPanel1.SetColumnSpan(comboSystem, 3);
            comboSystem.FormattingEnabled = true;
            comboSystem.Location = new Point(89, 83);
            comboSystem.Name = "comboSystem";
            comboSystem.Size = new Size(450, 23);
            comboSystem.TabIndex = 10;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(35, 87);
            label3.Name = "label3";
            label3.Size = new Size(48, 15);
            label3.TabIndex = 3;
            label3.Text = "S&ystem:";
            // 
            // lblCloseWarning
            // 
            lblCloseWarning.Anchor = AnchorStyles.None;
            lblCloseWarning.AutoSize = true;
            lblCloseWarning.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblCloseWarning.Location = new Point(155, 150);
            lblCloseWarning.Margin = new Padding(3);
            lblCloseWarning.Name = "lblCloseWarning";
            lblCloseWarning.Size = new Size(156, 15);
            lblCloseWarning.TabIndex = 7;
            lblCloseWarning.Text = "Close the game to proceed";
            // 
            // btnRestore
            // 
            btnRestore.Anchor = AnchorStyles.Right;
            btnRestore.Location = new Point(8, 146);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(75, 23);
            btnRestore.TabIndex = 6;
            btnRestore.Text = "&Restore";
            btnRestore.UseVisualStyleBackColor = true;
            btnRestore.Click += btnRestore_Click;
            // 
            // timer
            // 
            timer.Tick += timer_Tick;
            // 
            // FormSwapStarCache
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(547, 187);
            ControlBox = false;
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSwapStarCache";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Swap VisitedStarsCache.dat";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private LinkLabel linkEDGalaxy;
        private FlatButton btnNo;
        private FlatButton btnYes;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label2;
        private System.Windows.Forms.Timer timer;
        private Label lblCloseWarning;
        private Label label3;
        private FlatButton btnRestore;
        private ComboStarSystem comboSystem;
        private ComboCmdr comboCmdrs;
    }
}