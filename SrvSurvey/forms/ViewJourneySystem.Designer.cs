namespace SrvSurvey.forms
{
    partial class ViewJourneySystem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewJourneySystem));
            txtSystemName = new TextBox2();
            txtNotes = new TextBox2();
            txtStuff = new TextBox2();
            imageList1 = new ImageList(components);
            flowImages = new FlowLayoutPanel();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            label1 = new Label();
            split = new SplitContainer();
            txtRoughStats = new TextBox2();
            checkJourneyImagesOnly = new CheckBox2();
            table = new TableLayoutPanel();
            flowImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)split).BeginInit();
            split.Panel1.SuspendLayout();
            split.Panel2.SuspendLayout();
            split.SuspendLayout();
            table.SuspendLayout();
            SuspendLayout();
            // 
            // txtSystemName
            // 
            txtSystemName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSystemName.BackColor = SystemColors.Control;
            txtSystemName.Font = new Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtSystemName.ForeColor = SystemColors.WindowText;
            txtSystemName.Location = new Point(0, 0);
            txtSystemName.Margin = new Padding(0);
            txtSystemName.Name = "txtSystemName";
            txtSystemName.Padding = new Padding(3, 0, 3, 3);
            txtSystemName.ReadOnly = true;
            txtSystemName.Size = new Size(723, 34);
            txtSystemName.TabIndex = 0;
            txtSystemName.Text = "system name";
            txtSystemName.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // txtNotes
            // 
            txtNotes.Dock = DockStyle.Fill;
            txtNotes.Location = new Point(0, 0);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(419, 217);
            txtNotes.TabIndex = 1;
            txtNotes.TextChanged += txtNotes_TextChanged;
            // 
            // txtStuff
            // 
            txtStuff.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtStuff.BackColor = SystemColors.Control;
            txtStuff.ForeColor = SystemColors.WindowText;
            txtStuff.Location = new Point(0, 37);
            txtStuff.Margin = new Padding(0, 3, 0, 3);
            txtStuff.Name = "txtStuff";
            txtStuff.Padding = new Padding(3, 0, 3, 0);
            txtStuff.ReadOnly = true;
            txtStuff.Size = new Size(723, 20);
            txtStuff.TabIndex = 1;
            txtStuff.Text = "stuff";
            txtStuff.UseEdgeButton = TextBox2.EdgeButton.None;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "canonn-128x128.png");
            imageList1.Images.SetKeyName(1, "SrvSurvey-big.png");
            // 
            // flowImages
            // 
            flowImages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowImages.AutoScroll = true;
            flowImages.BorderStyle = BorderStyle.Fixed3D;
            flowImages.Controls.Add(pictureBox1);
            flowImages.Controls.Add(pictureBox2);
            flowImages.Controls.Add(pictureBox3);
            flowImages.Location = new Point(0, 32);
            flowImages.Margin = new Padding(0);
            flowImages.Name = "flowImages";
            flowImages.Size = new Size(723, 315);
            flowImages.TabIndex = 2;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(255, 128, 128);
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(320, 181);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.FromArgb(128, 255, 128);
            pictureBox2.Location = new Point(329, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(320, 181);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox3.Location = new Point(3, 190);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(320, 181);
            pictureBox3.TabIndex = 2;
            pictureBox3.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(0, 2);
            label1.Margin = new Padding(3, 0, 3, 3);
            label1.Name = "label1";
            label1.Size = new Size(163, 19);
            label1.TabIndex = 0;
            label1.Text = "Photos in this system:";
            // 
            // split
            // 
            split.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            split.BackColor = SystemColors.ButtonShadow;
            split.Location = new Point(0, 60);
            split.Margin = new Padding(0);
            split.Name = "split";
            split.Orientation = Orientation.Horizontal;
            // 
            // split.Panel1
            // 
            split.Panel1.Controls.Add(txtNotes);
            split.Panel1.Controls.Add(txtRoughStats);
            // 
            // split.Panel2
            // 
            split.Panel2.BackColor = SystemColors.Control;
            split.Panel2.Controls.Add(checkJourneyImagesOnly);
            split.Panel2.Controls.Add(flowImages);
            split.Panel2.Controls.Add(label1);
            split.Size = new Size(723, 577);
            split.SplitterDistance = 217;
            split.SplitterWidth = 5;
            split.TabIndex = 5;
            // 
            // txtRoughStats
            // 
            txtRoughStats.Dock = DockStyle.Right;
            txtRoughStats.Location = new Point(419, 0);
            txtRoughStats.Multiline = true;
            txtRoughStats.Name = "txtRoughStats";
            txtRoughStats.ReadOnly = true;
            txtRoughStats.ScrollBars = ScrollBars.Vertical;
            txtRoughStats.Size = new Size(304, 217);
            txtRoughStats.TabIndex = 2;
            // 
            // checkJourneyImagesOnly
            // 
            checkJourneyImagesOnly.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkJourneyImagesOnly.AutoSize = true;
            checkJourneyImagesOnly.Enabled = false;
            checkJourneyImagesOnly.Location = new Point(525, 7);
            checkJourneyImagesOnly.Name = "checkJourneyImagesOnly";
            checkJourneyImagesOnly.Size = new Size(198, 21);
            checkJourneyImagesOnly.TabIndex = 1;
            checkJourneyImagesOnly.Text = "Only images from this journey";
            checkJourneyImagesOnly.UseVisualStyleBackColor = true;
            // 
            // table
            // 
            table.ColumnCount = 1;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.Controls.Add(txtSystemName, 0, 0);
            table.Controls.Add(split, 0, 2);
            table.Controls.Add(txtStuff, 0, 1);
            table.Dock = DockStyle.Fill;
            table.Location = new Point(0, 0);
            table.Name = "table";
            table.RowCount = 3;
            table.RowStyles.Add(new RowStyle());
            table.RowStyles.Add(new RowStyle());
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            table.Size = new Size(723, 637);
            table.TabIndex = 6;
            // 
            // ViewJourneySystem
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(table);
            DoubleBuffered = true;
            Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "ViewJourneySystem";
            Size = new Size(723, 637);
            flowImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            split.Panel1.ResumeLayout(false);
            split.Panel1.PerformLayout();
            split.Panel2.ResumeLayout(false);
            split.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)split).EndInit();
            split.ResumeLayout(false);
            table.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox2 txtSystemName;
        private TextBox2 txtNotes;
        private TextBox2 txtStuff;
        private ImageList imageList1;
        private FlowLayoutPanel flowImages;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private Label label1;
        private SplitContainer split;
        private CheckBox2 checkJourneyImagesOnly;
        private TableLayoutPanel table;
        private TextBox2 txtRoughStats;
    }
}
