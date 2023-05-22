namespace SrvSurvey
{
    partial class FormRuins
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
            this.btnUpdate = new System.Windows.Forms.Button();
            this.map = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.button2 = new System.Windows.Forms.Button();
            this.txtSiteOrigin = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numImgRotation = new System.Windows.Forms.NumericUpDown();
            this.numImgScale = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSelectedItem = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.map)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numImgRotation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numImgScale)).BeginInit();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(12, 11);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 0;
            this.btnUpdate.Text = "&Apply";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // map
            // 
            this.map.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.map.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.map.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.map.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.map.Cursor = System.Windows.Forms.Cursors.Cross;
            this.map.Location = new System.Drawing.Point(0, 41);
            this.map.Name = "map";
            this.map.Size = new System.Drawing.Size(1550, 848);
            this.map.TabIndex = 1;
            this.map.TabStop = false;
            this.map.Click += new System.EventHandler(this.map_Click);
            this.map.Paint += new System.Windows.Forms.PaintEventHandler(this.map_Paint);
            this.map.MouseDown += new System.Windows.Forms.MouseEventHandler(this.map_MouseDown);
            this.map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.map_MouseMove);
            this.map.MouseUp += new System.Windows.Forms.MouseEventHandler(this.map_MouseUp);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblSelectedItem,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 890);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1550, 24);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblStatus.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lblStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(122, 19);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(1463, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Close";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtSiteOrigin
            // 
            this.txtSiteOrigin.Location = new System.Drawing.Point(195, 11);
            this.txtSiteOrigin.Name = "txtSiteOrigin";
            this.txtSiteOrigin.Size = new System.Drawing.Size(71, 23);
            this.txtSiteOrigin.TabIndex = 2;
            this.txtSiteOrigin.Text = "636,357";
            this.txtSiteOrigin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Site origin offset:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(272, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Image rotation:";
            // 
            // numImgRotation
            // 
            this.numImgRotation.Cursor = System.Windows.Forms.Cursors.NoMoveVert;
            this.numImgRotation.DecimalPlaces = 2;
            this.numImgRotation.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numImgRotation.Location = new System.Drawing.Point(366, 13);
            this.numImgRotation.Maximum = new decimal(new int[] {
            361,
            0,
            0,
            0});
            this.numImgRotation.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numImgRotation.Name = "numImgRotation";
            this.numImgRotation.Size = new System.Drawing.Size(56, 23);
            this.numImgRotation.TabIndex = 4;
            this.numImgRotation.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numImgRotation.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // numImgScale
            // 
            this.numImgScale.Cursor = System.Windows.Forms.Cursors.NoMoveVert;
            this.numImgScale.DecimalPlaces = 1;
            this.numImgScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numImgScale.Location = new System.Drawing.Point(549, 13);
            this.numImgScale.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numImgScale.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numImgScale.Name = "numImgScale";
            this.numImgScale.Size = new System.Drawing.Size(56, 23);
            this.numImgScale.TabIndex = 8;
            this.numImgScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numImgScale.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numImgScale.ValueChanged += new System.EventHandler(this.numImgScale_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(437, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Image scale factor:";
            // 
            // lblSelectedItem
            // 
            this.lblSelectedItem.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblSelectedItem.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lblSelectedItem.Name = "lblSelectedItem";
            this.lblSelectedItem.Size = new System.Drawing.Size(54, 19);
            this.lblSelectedItem.Text = "t1 (relic)";
            // 
            // FormRuins
            // 
            this.AcceptButton = this.btnUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(1550, 914);
            this.Controls.Add(this.numImgScale);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numImgRotation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSiteOrigin);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.map);
            this.Controls.Add(this.btnUpdate);
            this.Name = "FormRuins";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormRuins";
            this.Load += new System.EventHandler(this.FormRuins_Load);
            this.ResizeEnd += new System.EventHandler(this.FormRuins_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.map)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numImgRotation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numImgScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnUpdate;
        private PictureBox map;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private Button button2;
        private TextBox txtSiteOrigin;
        private Label label1;
        private Label label2;
        private NumericUpDown numImgRotation;
        private NumericUpDown numImgScale;
        private Label label3;
        private ToolStripStatusLabel lblSelectedItem;
    }
}