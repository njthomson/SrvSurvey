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
            toolEverything = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolInSystem = new ToolStripMenuItem();
            toolInSystemNote = new ToolStripMenuItem();
            toolSizer = new ToolStripSplitButton();
            toolSizeSmall = new ToolStripMenuItem();
            toolSizeMedium = new ToolStripMenuItem();
            toolFiller = new ToolStripStatusLabel();
            toolRewardLabel = new ToolStripStatusLabel();
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
            uberPanel.Size = new Size(741, 642);
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolSystemFilter, toolSizer, toolFiller, toolRewardLabel, toolRewardValue });
            statusStrip1.Location = new Point(0, 642);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(741, 24);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(39, 19);
            toolStripStatusLabel1.Text = "Show:";
            // 
            // toolSystemFilter
            // 
            toolSystemFilter.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolSystemFilter.DropDownItems.AddRange(new ToolStripItem[] { toolEverything, toolStripSeparator1, toolInSystem, toolInSystemNote });
            toolSystemFilter.Image = (Image)resources.GetObject("toolSystemFilter.Image");
            toolSystemFilter.ImageTransparentColor = Color.Magenta;
            toolSystemFilter.Name = "toolSystemFilter";
            toolSystemFilter.Size = new Size(68, 22);
            toolSystemFilter.Text = "... stuff ...";
            // 
            // toolEverything
            // 
            toolEverything.CheckOnClick = true;
            toolEverything.Name = "toolEverything";
            toolEverything.Size = new Size(272, 22);
            toolEverything.Text = "Everything";
            toolEverything.Click += toolInSystem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(269, 6);
            // 
            // toolInSystem
            // 
            toolInSystem.CheckOnClick = true;
            toolInSystem.Name = "toolInSystem";
            toolInSystem.Size = new Size(272, 22);
            toolInSystem.Text = "Species in current system";
            toolInSystem.Click += toolInSystem_Click;
            // 
            // toolInSystemNote
            // 
            toolInSystemNote.Enabled = false;
            toolInSystemNote.Name = "toolInSystemNote";
            toolInSystemNote.Size = new Size(272, 22);
            toolInSystemNote.Text = "(If current system has any bio signals)";
            // 
            // toolSizer
            // 
            toolSizer.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolSizer.DropDownItems.AddRange(new ToolStripItem[] { toolSizeSmall, toolSizeMedium });
            toolSizer.Image = (Image)resources.GetObject("toolSizer.Image");
            toolSizer.ImageTransparentColor = Color.Magenta;
            toolSizer.Margin = new Padding(20, 2, 0, 0);
            toolSizer.Name = "toolSizer";
            toolSizer.Size = new Size(69, 22);
            toolSizer.Text = "Font size";
            toolSizer.ButtonClick += toolStripSplitButton1_ButtonClick;
            // 
            // toolSizeSmall
            // 
            toolSizeSmall.Name = "toolSizeSmall";
            toolSizeSmall.Size = new Size(119, 22);
            toolSizeSmall.Text = "Small";
            toolSizeSmall.Click += toolSizeSmall_Click;
            // 
            // toolSizeMedium
            // 
            toolSizeMedium.Name = "toolSizeMedium";
            toolSizeMedium.Size = new Size(119, 22);
            toolSizeMedium.Text = "Medium";
            toolSizeMedium.Click += toolSizeMedium_Click;
            // 
            // toolFiller
            // 
            toolFiller.AutoSize = false;
            toolFiller.Name = "toolFiller";
            toolFiller.Size = new Size(338, 19);
            toolFiller.Spring = true;
            // 
            // toolRewardLabel
            // 
            toolRewardLabel.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolRewardLabel.BorderStyle = Border3DStyle.Sunken;
            toolRewardLabel.Name = "toolRewardLabel";
            toolRewardLabel.Size = new Size(139, 19);
            toolRewardLabel.Text = "System reward estimate:";
            // 
            // toolRewardValue
            // 
            toolRewardValue.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolRewardValue.BorderStyle = Border3DStyle.Sunken;
            toolRewardValue.Name = "toolRewardValue";
            toolRewardValue.Size = new Size(53, 19);
            toolRewardValue.Text = "555M cr";
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
            Text = "Exo Biology Genus Guide";
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
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolInSystemNote;
        private ToolStripSplitButton toolSizer;
        private ToolStripStatusLabel toolFiller;
        private ToolStripStatusLabel toolRewardLabel;
        private ToolStripStatusLabel toolRewardValue;
        private ToolStripMenuItem toolSizeSmall;
        private ToolStripMenuItem toolSizeMedium;
    }
}