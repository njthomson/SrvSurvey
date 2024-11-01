namespace SrvSurvey
{
    partial class FormGenus
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGenus));
            imageList1 = new ImageList(components);
            uberPanel = new FlowLayoutPanel();
            groupBox1 = new GroupBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolSystemFilter = new ToolStripDropDownButton();
            toolShowGuide = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolEverything = new ToolStripMenuItem();
            toolInSystem = new ToolStripMenuItem();
            toolOnBody = new ToolStripMenuItem();
            toolFont = new ToolStripDropDownButton();
            smallToolStripMenuItem = new ToolStripMenuItem();
            largeToolStripMenuItem = new ToolStripMenuItem();
            toolFiller = new ToolStripStatusLabel();
            toolStripSplitButton1 = new ToolStripSplitButton();
            menuOpenCanonnDump = new ToolStripMenuItem();
            menuOpenCanonnCodex = new ToolStripMenuItem();
            menuOpenEDSM = new ToolStripMenuItem();
            toolSignalCount = new ToolStripStatusLabel();
            toolRewardValue = new ToolStripStatusLabel();
            uberPanel.SuspendLayout();
            groupBox1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth8Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "bacterium38.png");
            imageList1.Images.SetKeyName(1, "osseus38.png");
            // 
            // uberPanel
            // 
            uberPanel.AutoScroll = true;
            uberPanel.BackColor = Color.DimGray;
            uberPanel.BorderStyle = BorderStyle.Fixed3D;
            uberPanel.Controls.Add(groupBox1);
            uberPanel.Controls.Add(groupBox2);
            uberPanel.Controls.Add(groupBox3);
            uberPanel.Dock = DockStyle.Fill;
            uberPanel.FlowDirection = FlowDirection.TopDown;
            uberPanel.Location = new Point(0, 0);
            uberPanel.Margin = new Padding(0);
            uberPanel.Name = "uberPanel";
            uberPanel.Size = new Size(741, 636);
            uberPanel.TabIndex = 1;
            uberPanel.WrapContents = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(30, 3, 3, 3);
            groupBox1.Size = new Size(480, 130);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // label1
            // 
            label1.BackColor = Color.LightGray;
            label1.Location = new Point(33, 31);
            label1.Name = "label1";
            label1.Size = new Size(428, 30);
            label1.TabIndex = 0;
            label1.Text = "label1";
            // 
            // groupBox2
            // 
            groupBox2.Dock = DockStyle.Top;
            groupBox2.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            groupBox2.Location = new Point(3, 139);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(480, 149);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "groupBox2";
            // 
            // groupBox3
            // 
            groupBox3.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            groupBox3.Location = new Point(3, 294);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(480, 149);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "groupBox3";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolSystemFilter, toolFont, toolFiller, toolStripSplitButton1, toolSignalCount, toolRewardValue });
            statusStrip1.Location = new Point(0, 636);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(741, 30);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(39, 25);
            toolStripStatusLabel1.Text = "Show:";
            toolStripStatusLabel1.Click += toolStripStatusLabel1_Click;
            // 
            // toolSystemFilter
            // 
            toolSystemFilter.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolSystemFilter.DropDownItems.AddRange(new ToolStripItem[] { toolShowGuide, toolStripSeparator1, toolEverything, toolInSystem, toolOnBody });
            toolSystemFilter.Image = (Image)resources.GetObject("toolSystemFilter.Image");
            toolSystemFilter.ImageTransparentColor = Color.Magenta;
            toolSystemFilter.Name = "toolSystemFilter";
            toolSystemFilter.Size = new Size(68, 28);
            toolSystemFilter.Text = "... stuff ...";
            // 
            // toolShowGuide
            // 
            toolShowGuide.CheckOnClick = true;
            toolShowGuide.Name = "toolShowGuide";
            toolShowGuide.Size = new Size(155, 22);
            toolShowGuide.Text = "Ring Guide";
            toolShowGuide.Click += toolShowGuide_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(152, 6);
            // 
            // toolEverything
            // 
            toolEverything.CheckOnClick = true;
            toolEverything.Name = "toolEverything";
            toolEverything.Size = new Size(155, 22);
            toolEverything.Text = "Everything";
            toolEverything.Click += toolFilter_Click;
            // 
            // toolInSystem
            // 
            toolInSystem.CheckOnClick = true;
            toolInSystem.Name = "toolInSystem";
            toolInSystem.Size = new Size(155, 22);
            toolInSystem.Text = "Current System";
            toolInSystem.Click += toolFilter_Click;
            // 
            // toolOnBody
            // 
            toolOnBody.Name = "toolOnBody";
            toolOnBody.Size = new Size(155, 22);
            toolOnBody.Text = "Current Body";
            toolOnBody.Click += toolFilter_Click;
            // 
            // toolFont
            // 
            toolFont.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolFont.DropDownItems.AddRange(new ToolStripItem[] { smallToolStripMenuItem, largeToolStripMenuItem });
            toolFont.Image = (Image)resources.GetObject("toolFont.Image");
            toolFont.ImageTransparentColor = Color.Magenta;
            toolFont.Name = "toolFont";
            toolFont.Size = new Size(69, 28);
            toolFont.Text = "Font: size";
            // 
            // smallToolStripMenuItem
            // 
            smallToolStripMenuItem.Name = "smallToolStripMenuItem";
            smallToolStripMenuItem.Size = new Size(103, 22);
            smallToolStripMenuItem.Text = "Small";
            smallToolStripMenuItem.Click += toolSizeSmall_Click;
            // 
            // largeToolStripMenuItem
            // 
            largeToolStripMenuItem.Name = "largeToolStripMenuItem";
            largeToolStripMenuItem.Size = new Size(103, 22);
            largeToolStripMenuItem.Text = "Large";
            largeToolStripMenuItem.Click += toolSizeLarge_Click;
            // 
            // toolFiller
            // 
            toolFiller.AutoSize = false;
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(104, 25);
            // 
            // toolStripSplitButton1
            // 
            toolStripSplitButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripSplitButton1.DropDownItems.AddRange(new ToolStripItem[] { menuOpenCanonnDump, menuOpenCanonnCodex, menuOpenEDSM });
            toolStripSplitButton1.Image = (Image)resources.GetObject("toolStripSplitButton1.Image");
            toolStripSplitButton1.ImageTransparentColor = Color.Magenta;
            toolStripSplitButton1.Name = "toolStripSplitButton1";
            toolStripSplitButton1.Size = new Size(32, 28);
            toolStripSplitButton1.Text = "toolStripSplitButton1";
            toolStripSplitButton1.ButtonClick += toolStripSplitButton1_ButtonClick;
            // 
            // menuOpenCanonnDump
            // 
            menuOpenCanonnDump.Name = "menuOpenCanonnDump";
            menuOpenCanonnDump.Size = new Size(202, 22);
            menuOpenCanonnDump.Text = "Open Canonn dump";
            menuOpenCanonnDump.Click += menuOpenCanonnDump_Click;
            // 
            // menuOpenCanonnCodex
            // 
            menuOpenCanonnCodex.Name = "menuOpenCanonnCodex";
            menuOpenCanonnCodex.Size = new Size(202, 22);
            menuOpenCanonnCodex.Text = "Open on Canonn Codex";
            menuOpenCanonnCodex.Click += menuOpenCanonnCodex_Click;
            // 
            // menuOpenEDSM
            // 
            menuOpenEDSM.Name = "menuOpenEDSM";
            menuOpenEDSM.Size = new Size(202, 22);
            menuOpenEDSM.Text = "Open on EDSM";
            menuOpenEDSM.Click += menuOpenEDSM_Click;
            // 
            // toolSignalCount
            // 
            toolSignalCount.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolSignalCount.BorderStyle = Border3DStyle.Sunken;
            toolSignalCount.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            toolSignalCount.Name = "toolSignalCount";
            toolSignalCount.Size = new Size(100, 25);
            toolSignalCount.Text = "Signals: 999";
            // 
            // toolRewardValue
            // 
            toolRewardValue.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolRewardValue.BorderStyle = Border3DStyle.Sunken;
            toolRewardValue.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            toolRewardValue.Name = "toolRewardValue";
            toolRewardValue.Size = new Size(74, 25);
            toolRewardValue.Text = "555M cr";
            toolRewardValue.Visible = false;
            // 
            // FormGenus
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(741, 666);
            Controls.Add(uberPanel);
            Controls.Add(statusStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormGenus";
            Text = "System Bio Signals";
            Load += FormGenus_Load;
            uberPanel.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ImageList imageList1;
        private FlowLayoutPanel uberPanel;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Label label1;
        private StatusStrip statusStrip1;
        private ToolStripDropDownButton toolSystemFilter;
        private ToolStripMenuItem toolEverything;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem toolInSystem;
        private ToolStripStatusLabel toolFiller;
        private ToolStripStatusLabel toolRewardValue;
        private ToolStripMenuItem toolOnBody;
        private ToolStripDropDownButton toolFont;
        private ToolStripMenuItem smallToolStripMenuItem;
        private ToolStripMenuItem largeToolStripMenuItem;
        private ToolStripMenuItem toolShowGuide;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripStatusLabel toolSignalCount;
        private ToolStripSplitButton toolStripSplitButton1;
        private ToolStripMenuItem menuOpenCanonnDump;
        private ToolStripMenuItem menuOpenCanonnCodex;
        private ToolStripMenuItem menuOpenEDSM;
    }
}