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
            lblTitle = new Label();
            lblCmdr = new Label();
            lblNoImage = new Label();
            linkSubmitImage = new LinkLabel();
            panelSubmit = new Panel();
            toolOpenCanonn = new ToolStripStatusLabel();
            statusStrip = new StatusStrip();
            toolChange = new ToolStripDropDownButton();
            somethingToolStripMenuItem = new ToolStripMenuItem();
            toolFiller = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolOpenBioforge = new ToolStripStatusLabel();
            lblLoading = new Label();
            panelSubmit.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
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
            lblTitle.DoubleClick += lblTitle_DoubleClick;
            // 
            // lblCmdr
            // 
            lblCmdr.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCmdr.AutoSize = true;
            lblCmdr.Location = new Point(0, 345);
            lblCmdr.Name = "lblCmdr";
            lblCmdr.Size = new Size(67, 17);
            lblCmdr.TabIndex = 10;
            lblCmdr.Text = "cmdr: Foo";
            lblCmdr.TextAlign = ContentAlignment.MiddleRight;
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
            // statusStrip
            // 
            statusStrip.AllowMerge = false;
            statusStrip.BackColor = SystemColors.Control;
            statusStrip.Items.AddRange(new ToolStripItem[] { toolChange, toolFiller, toolOpenCanonn, toolStripStatusLabel1, toolOpenBioforge });
            statusStrip.Location = new Point(0, 365);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(561, 22);
            statusStrip.TabIndex = 4;
            statusStrip.Text = "statusStrip1";
            // 
            // toolChange
            // 
            toolChange.DropDownItems.AddRange(new ToolStripItem[] { somethingToolStripMenuItem });
            toolChange.Image = (Image)resources.GetObject("toolChange.Image");
            toolChange.ImageTransparentColor = Color.Magenta;
            toolChange.Name = "toolChange";
            toolChange.Size = new Size(113, 20);
            toolChange.Text = "Change image";
            // 
            // somethingToolStripMenuItem
            // 
            somethingToolStripMenuItem.Name = "somethingToolStripMenuItem";
            somethingToolStripMenuItem.Size = new Size(131, 22);
            somethingToolStripMenuItem.Text = "something";
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Overflow = ToolStripItemOverflow.Never;
            toolFiller.Size = new Size(225, 17);
            toolFiller.Spring = true;
            toolFiller.Text = " ";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(12, 17);
            toolStripStatusLabel1.Text = "/";
            // 
            // toolOpenBioforge
            // 
            toolOpenBioforge.IsLink = true;
            toolOpenBioforge.Name = "toolOpenBioforge";
            toolOpenBioforge.Size = new Size(52, 17);
            toolOpenBioforge.Text = "Bioforge";
            toolOpenBioforge.Click += toolOpenBioforge_Click;
            // 
            // lblLoading
            // 
            lblLoading.Anchor = AnchorStyles.None;
            lblLoading.Location = new Point(246, 187);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(68, 17);
            lblLoading.TabIndex = 9;
            lblLoading.Text = "Loading ...";
            // 
            // FormShowCodex
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(561, 387);
            Controls.Add(lblTitle);
            Controls.Add(lblLoading);
            Controls.Add(panelSubmit);
            Controls.Add(statusStrip);
            Controls.Add(lblCmdr);
            Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.DarkOrange;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormShowCodex";
            Text = "Canonn Codex";
            Load += FormShowCodex_Load;
            MouseDown += FormShowCodex_MouseDown;
            MouseMove += FormShowCodex_MouseMove;
            MouseUp += FormShowCodex_MouseUp;
            panelSubmit.ResumeLayout(false);
            panelSubmit.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblTitle;
        private Label lblNoImage;
        private LinkLabel linkSubmitImage;
        private Panel panelSubmit;
        private ToolStripStatusLabel toolOpenCanonn;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolFiller;
        private ToolStripDropDownButton toolChange;
        private Label lblLoading;
        private Label lblCmdr;
        private ToolStripMenuItem somethingToolStripMenuItem;
        private ToolStripStatusLabel toolOpenBioforge;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}