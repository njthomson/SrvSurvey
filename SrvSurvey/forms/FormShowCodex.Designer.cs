using SrvSurvey.game;

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

                Game.update -= Game_update;
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
            statusStrip = new StatusStrip();
            toolCanonn = new ToolStripStatusLabel();
            toolFiller = new ToolStripStatusLabel();
            toolMore = new ToolStripDropDownButton();
            viewVariantToolStripMenuItem = new ToolStripMenuItem();
            viewOnCanonnSignalsToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            viewSystemToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            viewOnSpanshToolStripMenuItem = new ToolStripMenuItem();
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
            lblTitle.Font = new Font("Century Gothic", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(84, 2);
            lblTitle.Margin = new Padding(0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(177, 28);
            lblTitle.TabIndex = 3;
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
            lblCmdr.TabIndex = 5;
            lblCmdr.Text = "cmdr: Foo";
            lblCmdr.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblNoImage
            // 
            lblNoImage.Anchor = AnchorStyles.Top;
            lblNoImage.AutoSize = true;
            lblNoImage.FlatStyle = FlatStyle.System;
            lblNoImage.Font = new Font("Century Gothic", 14.25F);
            lblNoImage.Location = new Point(12, 10);
            lblNoImage.Name = "lblNoImage";
            lblNoImage.Size = new Size(398, 44);
            lblNoImage.TabIndex = 0;
            lblNoImage.Text = "No image has been shared for this variant.\r\nWould you like to submit one?";
            lblNoImage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // linkSubmitImage
            // 
            linkSubmitImage.Anchor = AnchorStyles.Top;
            linkSubmitImage.AutoSize = true;
            linkSubmitImage.Font = new Font("Century Gothic", 12F);
            linkSubmitImage.LinkColor = Color.FromArgb(255, 128, 0);
            linkSubmitImage.Location = new Point(109, 84);
            linkSubmitImage.Name = "linkSubmitImage";
            linkSubmitImage.Size = new Size(186, 21);
            linkSubmitImage.TabIndex = 1;
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
            panelSubmit.TabIndex = 3;
            panelSubmit.Visible = false;
            // 
            // statusStrip
            // 
            statusStrip.AllowMerge = false;
            statusStrip.BackColor = SystemColors.Control;
            statusStrip.Items.AddRange(new ToolStripItem[] { toolCanonn, toolFiller, toolMore });
            statusStrip.Location = new Point(0, 539);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(784, 22);
            statusStrip.TabIndex = 6;
            statusStrip.TabStop = true;
            statusStrip.Text = "statusStrip1";
            // 
            // toolCanonn
            // 
            toolCanonn.ForeColor = SystemColors.ActiveCaptionText;
            toolCanonn.Image = Properties.ImageResources.canonn_16x16;
            toolCanonn.Name = "toolCanonn";
            toolCanonn.Size = new Size(161, 17);
            toolCanonn.Text = "Images curtesy of Canonn";
            // 
            // toolFiller
            // 
            toolFiller.Name = "toolFiller";
            toolFiller.Overflow = ToolStripItemOverflow.Never;
            toolFiller.Size = new Size(548, 17);
            toolFiller.Spring = true;
            toolFiller.Text = " ";
            // 
            // toolMore
            // 
            toolMore.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolMore.DropDownItems.AddRange(new ToolStripItem[] { viewVariantToolStripMenuItem, viewOnCanonnSignalsToolStripMenuItem, toolStripMenuItem1, toolStripSeparator1, viewSystemToolStripMenuItem, toolStripMenuItem2, viewOnSpanshToolStripMenuItem });
            toolMore.ForeColor = SystemColors.ActiveCaptionText;
            toolMore.Image = (Image)resources.GetObject("toolMore.Image");
            toolMore.ImageTransparentColor = Color.Magenta;
            toolMore.Name = "toolMore";
            toolMore.Size = new Size(60, 20);
            toolMore.Text = "&More ...";
            // 
            // viewVariantToolStripMenuItem
            // 
            viewVariantToolStripMenuItem.Enabled = false;
            viewVariantToolStripMenuItem.Name = "viewVariantToolStripMenuItem";
            viewVariantToolStripMenuItem.Size = new Size(185, 22);
            viewVariantToolStripMenuItem.Text = "Open Variant ...";
            // 
            // viewOnCanonnSignalsToolStripMenuItem
            // 
            viewOnCanonnSignalsToolStripMenuItem.Image = Properties.ImageResources.canonn_16x16;
            viewOnCanonnSignalsToolStripMenuItem.Name = "viewOnCanonnSignalsToolStripMenuItem";
            viewOnCanonnSignalsToolStripMenuItem.Size = new Size(185, 22);
            viewOnCanonnSignalsToolStripMenuItem.Text = "On Canonn Research";
            viewOnCanonnSignalsToolStripMenuItem.Click += toolOpenCanonn_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Image = Properties.ImageResources.canonn_16x16;
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(185, 22);
            toolStripMenuItem1.Text = "On Canonn Bioforge";
            toolStripMenuItem1.Click += toolOpenBioforge_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(182, 6);
            // 
            // viewSystemToolStripMenuItem
            // 
            viewSystemToolStripMenuItem.Enabled = false;
            viewSystemToolStripMenuItem.Name = "viewSystemToolStripMenuItem";
            viewSystemToolStripMenuItem.Size = new Size(185, 22);
            viewSystemToolStripMenuItem.Text = "Open System ...";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Image = Properties.ImageResources.canonn_16x16;
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(185, 22);
            toolStripMenuItem2.Text = "On Canonn Signals";
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            // 
            // viewOnSpanshToolStripMenuItem
            // 
            viewOnSpanshToolStripMenuItem.Image = Properties.ImageResources.spansh_16x16;
            viewOnSpanshToolStripMenuItem.Name = "viewOnSpanshToolStripMenuItem";
            viewOnSpanshToolStripMenuItem.Size = new Size(185, 22);
            viewOnSpanshToolStripMenuItem.Text = "On Spansh";
            viewOnSpanshToolStripMenuItem.Click += viewOnSpanshToolStripMenuItem_Click;
            // 
            // lblLoading
            // 
            lblLoading.Anchor = AnchorStyles.None;
            lblLoading.FlatStyle = FlatStyle.System;
            lblLoading.Location = new Point(358, 274);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(68, 17);
            lblLoading.TabIndex = 4;
            lblLoading.Text = "Loading ...";
            lblLoading.Visible = false;
            // 
            // btnPrevBody
            // 
            btnPrevBody.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPrevBody.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPrevBody.Font = new Font("Segoe UI", 9.75F);
            btnPrevBody.Location = new Point(203, 0);
            btnPrevBody.Margin = new Padding(0, 0, 2, 0);
            btnPrevBody.Name = "btnPrevBody";
            btnPrevBody.Size = new Size(24, 29);
            btnPrevBody.TabIndex = 1;
            btnPrevBody.Text = "◀️";
            btnPrevBody.UseVisualStyleBackColor = true;
            btnPrevBody.Click += btnPrevBody_Click;
            // 
            // lblBodyName
            // 
            lblBodyName.Anchor = AnchorStyles.Left;
            lblBodyName.AutoSize = true;
            lblBodyName.Font = new Font("Century Gothic", 12F);
            lblBodyName.Location = new Point(0, 4);
            lblBodyName.Margin = new Padding(0);
            lblBodyName.Name = "lblBodyName";
            lblBodyName.Size = new Size(203, 21);
            lblBodyName.TabIndex = 0;
            lblBodyName.Text = "Syrai Thae GD-M c7-0 xxx";
            // 
            // btnNextBody
            // 
            btnNextBody.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnNextBody.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnNextBody.Font = new Font("Segoe UI", 9.75F);
            btnNextBody.Location = new Point(229, 0);
            btnNextBody.Margin = new Padding(0, 0, 2, 0);
            btnNextBody.Name = "btnNextBody";
            btnNextBody.Size = new Size(24, 29);
            btnNextBody.TabIndex = 2;
            btnNextBody.Text = "▶️";
            btnNextBody.UseVisualStyleBackColor = true;
            btnNextBody.Click += btnNextBody_Click;
            // 
            // btnNextBio
            // 
            btnNextBio.Anchor = AnchorStyles.Left;
            btnNextBio.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnNextBio.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnNextBio.Font = new Font("Segoe UI", 9.75F);
            btnNextBio.Location = new Point(58, 2);
            btnNextBio.Margin = new Padding(2);
            btnNextBio.Name = "btnNextBio";
            btnNextBio.Size = new Size(24, 29);
            btnNextBio.TabIndex = 2;
            btnNextBio.Text = "▶️";
            btnNextBio.UseVisualStyleBackColor = true;
            btnNextBio.Click += btnNextBio_Click;
            // 
            // btnPrevBio
            // 
            btnPrevBio.Anchor = AnchorStyles.Left;
            btnPrevBio.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPrevBio.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPrevBio.Font = new Font("Segoe UI", 9.75F);
            btnPrevBio.Location = new Point(2, 2);
            btnPrevBio.Margin = new Padding(2);
            btnPrevBio.Name = "btnPrevBio";
            btnPrevBio.Size = new Size(24, 29);
            btnPrevBio.TabIndex = 0;
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
            flowTop.TabIndex = 1;
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
            flowBodyParts.TabIndex = 2;
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
            table.TabIndex = 0;
            // 
            // btnMenu
            // 
            btnMenu.Anchor = AnchorStyles.Left;
            btnMenu.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnMenu.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnMenu.Font = new Font("Segoe UI", 12F);
            btnMenu.Location = new Point(30, 2);
            btnMenu.Margin = new Padding(2);
            btnMenu.Name = "btnMenu";
            btnMenu.Size = new Size(24, 29);
            btnMenu.TabIndex = 1;
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
            lblDetails.TabIndex = 4;
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
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Black;
            ClientSize = new Size(784, 561);
            Controls.Add(flowBodyParts);
            Controls.Add(table);
            Controls.Add(flowTop);
            Controls.Add(lblLoading);
            Controls.Add(lblCmdr);
            Controls.Add(panelSubmit);
            Controls.Add(statusStrip);
            Font = new Font("Century Gothic", 9F);
            ForeColor = Color.DarkOrange;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(640, 400);
            Name = "FormShowCodex";
            Text = "Codex Viewer";
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
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolFiller;
        private Label lblLoading;
        private Label lblCmdr;
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
        private ToolStripDropDownButton toolMore;
        private ToolStripMenuItem viewOnCanonnSignalsToolStripMenuItem;
        private ToolStripMenuItem viewOnSpanshToolStripMenuItem;
        private ToolStripMenuItem viewVariantToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem viewSystemToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripStatusLabel toolCanonn;
    }
}