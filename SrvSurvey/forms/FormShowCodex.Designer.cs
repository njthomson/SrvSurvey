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
            components = new System.ComponentModel.Container();
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
            btnPrevBody = new FlatButton();
            lblBodyName = new Label();
            btnNextBody = new FlatButton();
            btnNextBio = new FlatButton();
            btnPrevBio = new FlatButton();
            flowTop = new FlowLayoutPanel();
            flowBodyParts = new FlowLayoutPanel();
            table = new TableLayoutPanel();
            btnMenu = new FlatButton();
            lblDetails = new Label();
            menuStrip = new ContextMenuStrip(components);
            panelSubmit.SuspendLayout();
            statusStrip.SuspendLayout();
            flowBodyParts.SuspendLayout();
            table.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Left;
            lblTitle.AutoSize = true;
            lblTitle.FlatStyle = FlatStyle.System;
            lblTitle.Font = new Font("Century Gothic", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(84, 2);
            lblTitle.Margin = new Padding(0);
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
            lblCmdr.FlatStyle = FlatStyle.System;
            lblCmdr.Location = new Point(0, 519);
            lblCmdr.Margin = new Padding(3, 3, 3, 0);
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
            lblNoImage.FlatStyle = FlatStyle.System;
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
            panelSubmit.Location = new Point(29, 85);
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
            toolOpenCanonn.Click += toolOpenCanonn_Click;
            // 
            // statusStrip
            // 
            statusStrip.AllowMerge = false;
            statusStrip.BackColor = SystemColors.Control;
            statusStrip.Items.AddRange(new ToolStripItem[] { toolChange, toolFiller, toolOpenCanonn, toolStripStatusLabel1, toolOpenBioforge });
            statusStrip.Location = new Point(0, 539);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(784, 22);
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
            toolFiller.Size = new Size(448, 17);
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
            lblLoading.FlatStyle = FlatStyle.System;
            lblLoading.Location = new Point(358, 274);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(68, 17);
            lblLoading.TabIndex = 9;
            lblLoading.Text = "Loading ...";
            lblLoading.Visible = false;
            // 
            // btnPrevBody
            // 
            btnPrevBody.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPrevBody.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPrevBody.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnPrevBody.Location = new Point(203, 0);
            btnPrevBody.Margin = new Padding(0, 0, 2, 0);
            btnPrevBody.Name = "btnPrevBody";
            btnPrevBody.Size = new Size(24, 29);
            btnPrevBody.TabIndex = 11;
            btnPrevBody.Text = "◀️";
            btnPrevBody.UseVisualStyleBackColor = true;
            btnPrevBody.Click += btnPrevBody_Click;
            // 
            // lblBodyName
            // 
            lblBodyName.Anchor = AnchorStyles.Left;
            lblBodyName.AutoSize = true;
            lblBodyName.Font = new Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point);
            lblBodyName.Location = new Point(0, 4);
            lblBodyName.Margin = new Padding(0);
            lblBodyName.Name = "lblBodyName";
            lblBodyName.Size = new Size(203, 21);
            lblBodyName.TabIndex = 12;
            lblBodyName.Text = "Syrai Thae GD-M c7-0 xxx";
            // 
            // btnNextBody
            // 
            btnNextBody.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnNextBody.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnNextBody.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnNextBody.Location = new Point(229, 0);
            btnNextBody.Margin = new Padding(0, 0, 2, 0);
            btnNextBody.Name = "btnNextBody";
            btnNextBody.Size = new Size(24, 29);
            btnNextBody.TabIndex = 13;
            btnNextBody.Text = "▶️";
            btnNextBody.UseVisualStyleBackColor = true;
            btnNextBody.Click += btnNextBody_Click;
            // 
            // btnNextBio
            // 
            btnNextBio.Anchor = AnchorStyles.Left;
            btnNextBio.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnNextBio.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnNextBio.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnNextBio.Location = new Point(58, 2);
            btnNextBio.Margin = new Padding(2);
            btnNextBio.Name = "btnNextBio";
            btnNextBio.Size = new Size(24, 29);
            btnNextBio.TabIndex = 15;
            btnNextBio.Text = "▶️";
            btnNextBio.UseVisualStyleBackColor = true;
            btnNextBio.Click += btnNextBio_Click;
            // 
            // btnPrevBio
            // 
            btnPrevBio.Anchor = AnchorStyles.Left;
            btnPrevBio.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPrevBio.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPrevBio.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnPrevBio.Location = new Point(2, 2);
            btnPrevBio.Margin = new Padding(2);
            btnPrevBio.Name = "btnPrevBio";
            btnPrevBio.Size = new Size(24, 29);
            btnPrevBio.TabIndex = 14;
            btnPrevBio.Text = "◀️";
            btnPrevBio.UseVisualStyleBackColor = true;
            btnPrevBio.Click += btnPrevBio_Click;
            // 
            // flowTop
            // 
            flowTop.AutoSize = true;
            flowTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowTop.BackColor = Color.RosyBrown;
            flowTop.Dock = DockStyle.Top;
            flowTop.Location = new Point(0, 0);
            flowTop.Name = "flowTop";
            flowTop.Size = new Size(784, 0);
            flowTop.TabIndex = 16;
            // 
            // flowBodyParts
            // 
            flowBodyParts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowBodyParts.AutoSize = true;
            flowBodyParts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowBodyParts.Controls.Add(lblBodyName);
            flowBodyParts.Controls.Add(btnPrevBody);
            flowBodyParts.Controls.Add(btnNextBody);
            flowBodyParts.Location = new Point(529, 85);
            flowBodyParts.Margin = new Padding(0);
            flowBodyParts.Name = "flowBodyParts";
            flowBodyParts.Size = new Size(255, 29);
            flowBodyParts.TabIndex = 18;
            flowBodyParts.WrapContents = false;
            // 
            // table
            // 
            table.AutoSize = true;
            table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table.ColumnCount = 4;
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.Controls.Add(btnMenu, 1, 0);
            table.Controls.Add(lblTitle, 3, 0);
            table.Controls.Add(btnNextBio, 2, 0);
            table.Controls.Add(btnPrevBio, 0, 0);
            table.Controls.Add(lblDetails, 3, 1);
            table.Dock = DockStyle.Top;
            table.Location = new Point(0, 0);
            table.Margin = new Padding(0);
            table.Name = "table";
            table.RowCount = 2;
            table.RowStyles.Add(new RowStyle());
            table.RowStyles.Add(new RowStyle());
            table.Size = new Size(784, 50);
            table.TabIndex = 19;
            // 
            // btnMenu
            // 
            btnMenu.Anchor = AnchorStyles.Left;
            btnMenu.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnMenu.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnMenu.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            btnMenu.Location = new Point(30, 2);
            btnMenu.Margin = new Padding(2);
            btnMenu.Name = "btnMenu";
            btnMenu.Size = new Size(24, 29);
            btnMenu.TabIndex = 20;
            btnMenu.Text = "⏷";
            btnMenu.UseVisualStyleBackColor = true;
            btnMenu.MouseDown += btnMenu_MouseDown;
            btnMenu.MouseEnter += btnMenu_MouseEnter;
            // 
            // lblDetails
            // 
            lblDetails.Anchor = AnchorStyles.Left;
            lblDetails.AutoSize = true;
            lblDetails.FlatStyle = FlatStyle.System;
            lblDetails.Location = new Point(84, 33);
            lblDetails.Margin = new Padding(0);
            lblDetails.Name = "lblDetails";
            lblDetails.Size = new Size(265, 17);
            lblDetails.TabIndex = 19;
            lblDetails.Text = "Confirmed | Range: 200m | Reward: 44M cr";
            lblDetails.TextAlign = ContentAlignment.MiddleRight;
            // 
            // menuStrip
            // 
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(61, 4);
            menuStrip.Closed += menuStrip_Closed;
            // 
            // FormShowCodex
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(784, 561);
            Controls.Add(flowBodyParts);
            Controls.Add(table);
            Controls.Add(flowTop);
            Controls.Add(lblLoading);
            Controls.Add(lblCmdr);
            Controls.Add(panelSubmit);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.DarkOrange;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(640, 400);
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
            flowBodyParts.ResumeLayout(false);
            flowBodyParts.PerformLayout();
            table.ResumeLayout(false);
            table.PerformLayout();
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
        private FlatButton btnPrevBody;
        private Label lblBodyName;
        private FlatButton btnNextBody;
        private FlatButton btnNextBio;
        private FlatButton btnPrevBio;
        private FlowLayoutPanel flowTop;
        private FlowLayoutPanel flowBodyParts;
        private TableLayoutPanel table;
        private Label lblDetails;
        private FlatButton btnMenu;
        private ContextMenuStrip menuStrip;
    }
}