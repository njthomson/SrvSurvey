namespace SrvSurvey
{
    partial class FormShowCodex
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormShowCodex));
            panelTop = new Panel();
            lblTitle = new Label();
            lblNoImage = new Label();
            linkSubmitImage = new LinkLabel();
            panelSubmit = new Panel();
            toolOpenCanonn = new ToolStripStatusLabel();
            statusStrip1 = new StatusStrip();
            toolFiller = new ToolStripStatusLabel();
            toolImageCredit = new ToolStripStatusLabel();
            panelTop.SuspendLayout();
            panelSubmit.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.Controls.Add(lblTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(584, 31);
            panelTop.TabIndex = 3;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Century Gothic", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(177, 28);
            lblTitle.TabIndex = 4;
            lblTitle.Text = "Aleoida Arcus";
            // 
            // lblNoImage
            // 
            lblNoImage.Anchor = AnchorStyles.Top;
            lblNoImage.AutoSize = true;
            lblNoImage.Font = new Font("Century Gothic", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            lblNoImage.Location = new Point(12, 10);
            lblNoImage.Name = "lblNoImage";
            lblNoImage.Size = new Size(398, 44);
            lblNoImage.TabIndex = 5;
            lblNoImage.Text = "No image has been shared for this variant.\r\nWould you like to submit one?";
            lblNoImage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // linkSubmitImage
            // 
            linkSubmitImage.Anchor = AnchorStyles.Top;
            linkSubmitImage.AutoSize = true;
            linkSubmitImage.Font = new Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point);
            linkSubmitImage.LinkColor = Color.FromArgb(255, 128, 0);
            linkSubmitImage.Location = new Point(109, 84);
            linkSubmitImage.Name = "linkSubmitImage";
            linkSubmitImage.Size = new Size(186, 21);
            linkSubmitImage.TabIndex = 6;
            linkSubmitImage.TabStop = true;
            linkSubmitImage.Text = "Open submission page";
            linkSubmitImage.TextAlign = ContentAlignment.MiddleCenter;
            linkSubmitImage.LinkClicked += linkSubmitImage_LinkClicked;
            // 
            // panelSubmit
            // 
            panelSubmit.Controls.Add(linkSubmitImage);
            panelSubmit.Controls.Add(lblNoImage);
            panelSubmit.Location = new Point(12, 46);
            panelSubmit.Name = "panelSubmit";
            panelSubmit.Size = new Size(450, 151);
            panelSubmit.TabIndex = 7;
            panelSubmit.Visible = false;
            // 
            // toolOpenCanonn
            // 
            toolOpenCanonn.ForeColor = SystemColors.ControlText;
            toolOpenCanonn.IsLink = true;
            toolOpenCanonn.Name = "toolOpenCanonn";
            toolOpenCanonn.Size = new Size(144, 17);
            toolOpenCanonn.Text = "View on Canonn Research";
            toolOpenCanonn.Click += toolStripStatusLabel1_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = SystemColors.Control;
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolOpenCanonn, toolFiller, toolImageCredit });
            statusStrip1.Location = new Point(0, 339);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(584, 22);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(366, 17);
            toolFiller.Spring = true;
            toolFiller.Text = " ";
            // 
            // toolImageCredit
            // 
            toolImageCredit.Name = "toolImageCredit";
            toolImageCredit.Size = new Size(59, 17);
            toolImageCredit.Text = "cmdr: foo";
            toolImageCredit.TextAlign = ContentAlignment.MiddleRight;
            // 
            // FormShowCodex
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(584, 361);
            Controls.Add(panelSubmit);
            Controls.Add(statusStrip1);
            Controls.Add(panelTop);
            Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormShowCodex";
            Text = "Canonn Codex";
            Load += FormShowCodex_Load;
            MouseDown += FormShowCodex_MouseDown;
            MouseMove += FormShowCodex_MouseMove;
            MouseUp += FormShowCodex_MouseUp;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelSubmit.ResumeLayout(false);
            panelSubmit.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel panelTop;
        private Label lblTitle;
        private Label lblNoImage;
        private LinkLabel linkSubmitImage;
        private Panel panelSubmit;
        private ToolStripStatusLabel toolOpenCanonn;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolFiller;
        private ToolStripStatusLabel toolImageCredit;
    }
}