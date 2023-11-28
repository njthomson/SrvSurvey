
namespace SrvSurvey
{
    partial class ViewLogs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewLogs));
            txtLogs = new TextBox();
            btnClose = new Button();
            btnClear = new Button();
            panel1 = new Panel();
            btnCopy = new Button();
            linkLabel1 = new LinkLabel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtLogs
            // 
            txtLogs.BorderStyle = BorderStyle.FixedSingle;
            txtLogs.CausesValidation = false;
            txtLogs.Dock = DockStyle.Fill;
            txtLogs.HideSelection = false;
            txtLogs.Location = new Point(0, 0);
            txtLogs.Margin = new Padding(4, 3, 4, 3);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.ScrollBars = ScrollBars.Both;
            txtLogs.Size = new Size(933, 479);
            txtLogs.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(842, 7);
            btnClose.Margin = new Padding(4, 3, 4, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(88, 27);
            btnClose.TabIndex = 1;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClear.Location = new Point(4, 7);
            btnClear.Margin = new Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(88, 27);
            btnClear.TabIndex = 2;
            btnClear.Text = "&Reset";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(linkLabel1);
            panel1.Controls.Add(btnCopy);
            panel1.Controls.Add(btnClear);
            panel1.Controls.Add(btnClose);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 479);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(933, 40);
            panel1.TabIndex = 3;
            // 
            // btnCopy
            // 
            btnCopy.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCopy.Location = new Point(100, 7);
            btnCopy.Margin = new Padding(4, 3, 4, 3);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(88, 27);
            btnCopy.TabIndex = 3;
            btnCopy.Text = "C&opy";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(195, 13);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(95, 15);
            linkLabel1.TabIndex = 4;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Open logs folder";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // ViewLogs
            // 
            AcceptButton = btnClose;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(933, 519);
            Controls.Add(txtLogs);
            Controls.Add(panel1);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(600, 400);
            Name = "ViewLogs";
            SizeGripStyle = SizeGripStyle.Show;
            Text = "Srv Survey Logs";
            FormClosed += ViewLogs_FormClosed;
            Load += ViewLogs_Load;
            Shown += ViewLogs_Shown;
            ResizeEnd += ViewLogs_ResizeEnd;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLogs;
        private Button btnClose;
        private Button btnClear;
        private Panel panel1;
        private Button btnCopy;
        private LinkLabel linkLabel1;
    }
}