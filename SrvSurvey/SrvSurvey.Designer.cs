
namespace SrvSurvey
{
    partial class SrvSurvey
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
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.txtLatLong = new System.Windows.Forms.TextBox();
            this.txtHeading = new System.Windows.Forms.TextBox();
            this.txtMode = new System.Windows.Forms.TextBox();
            this.txtSettlementLocation = new System.Windows.Forms.TextBox();
            this.txtSettlement = new System.Windows.Forms.TextBox();
            this.numSiteHeading = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboType = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblMousePos = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblMeters = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.comboCmdr = new System.Windows.Forms.ComboBox();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.radioNorth = new System.Windows.Forms.RadioButton();
            this.radioSite = new System.Windows.Forms.RadioButton();
            this.radioSRV = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.numSiteHeading)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            this.SuspendLayout();
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(712, 65);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(459, 718);
            this.txtMsg.TabIndex = 1;
            // 
            // txtLatLong
            // 
            this.txtLatLong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLatLong.Location = new System.Drawing.Point(975, 14);
            this.txtLatLong.Name = "txtLatLong";
            this.txtLatLong.ReadOnly = true;
            this.txtLatLong.Size = new System.Drawing.Size(196, 20);
            this.txtLatLong.TabIndex = 5;
            this.txtLatLong.Text = "Lat: -000.000000, Long: -000.000000";
            this.txtLatLong.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtHeading
            // 
            this.txtHeading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeading.Location = new System.Drawing.Point(907, 14);
            this.txtHeading.Name = "txtHeading";
            this.txtHeading.ReadOnly = true;
            this.txtHeading.Size = new System.Drawing.Size(47, 20);
            this.txtHeading.TabIndex = 6;
            this.txtHeading.Text = "360°";
            this.txtHeading.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMode
            // 
            this.txtMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMode.Location = new System.Drawing.Point(797, 14);
            this.txtMode.Name = "txtMode";
            this.txtMode.ReadOnly = true;
            this.txtMode.Size = new System.Drawing.Size(48, 20);
            this.txtMode.TabIndex = 7;
            this.txtMode.Text = "-------";
            this.txtMode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtSettlementLocation
            // 
            this.txtSettlementLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSettlementLocation.Location = new System.Drawing.Point(975, 39);
            this.txtSettlementLocation.Name = "txtSettlementLocation";
            this.txtSettlementLocation.ReadOnly = true;
            this.txtSettlementLocation.Size = new System.Drawing.Size(196, 20);
            this.txtSettlementLocation.TabIndex = 8;
            this.txtSettlementLocation.Text = "Lat: -000.000000, Long: -000.000000";
            this.txtSettlementLocation.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtSettlement
            // 
            this.txtSettlement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSettlement.Location = new System.Drawing.Point(526, 38);
            this.txtSettlement.Name = "txtSettlement";
            this.txtSettlement.ReadOnly = true;
            this.txtSettlement.Size = new System.Drawing.Size(319, 20);
            this.txtSettlement.TabIndex = 9;
            this.txtSettlement.Text = "----";
            this.txtSettlement.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numSiteHeading
            // 
            this.numSiteHeading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numSiteHeading.Location = new System.Drawing.Point(907, 39);
            this.numSiteHeading.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numSiteHeading.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numSiteHeading.Name = "numSiteHeading";
            this.numSiteHeading.Size = new System.Drawing.Size(62, 20);
            this.numSiteHeading.TabIndex = 0;
            this.numSiteHeading.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSiteHeading.ValueChanged += new System.EventHandler(this.numSiteHeading_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(851, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Heading:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(851, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Heading:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(523, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Commander:";
            // 
            // comboType
            // 
            this.comboType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboType.FormattingEnabled = true;
            this.comboType.Items.AddRange(new object[] {
            "Unknown",
            "Alpha",
            "Beta",
            "Gamma"});
            this.comboType.Location = new System.Drawing.Point(441, 38);
            this.comboType.Name = "comboType";
            this.comboType.Size = new System.Drawing.Size(79, 21);
            this.comboType.TabIndex = 18;
            this.comboType.TextChanged += new System.EventHandler(this.comboType_TextChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 19;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMousePos,
            this.lblMeters,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 786);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1183, 24);
            this.statusStrip1.TabIndex = 22;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblMousePos
            // 
            this.lblMousePos.AutoSize = false;
            this.lblMousePos.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblMousePos.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lblMousePos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblMousePos.Name = "lblMousePos";
            this.lblMousePos.Size = new System.Drawing.Size(196, 19);
            this.lblMousePos.Text = "Lat: -000.000000, Long: -000.000000";
            // 
            // lblMeters
            // 
            this.lblMeters.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblMeters.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.lblMeters.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblMeters.Name = "lblMeters";
            this.lblMeters.Size = new System.Drawing.Size(46, 19);
            this.lblMeters.Text = "1099m";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = false;
            this.lblStatus.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblStatus.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lblStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(375, 19);
            this.lblStatus.Text = "...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox1.ErrorImage = null;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(12, 65);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(694, 717);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // comboCmdr
            // 
            this.comboCmdr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCmdr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCmdr.FormattingEnabled = true;
            this.comboCmdr.Location = new System.Drawing.Point(595, 14);
            this.comboCmdr.Name = "comboCmdr";
            this.comboCmdr.Size = new System.Drawing.Size(196, 21);
            this.comboCmdr.TabIndex = 23;
            // 
            // numScale
            // 
            this.numScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numScale.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numScale.Location = new System.Drawing.Point(93, 14);
            this.numScale.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numScale.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(62, 20);
            this.numScale.TabIndex = 25;
            this.numScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numScale.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numScale.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Up is:";
            // 
            // radioNorth
            // 
            this.radioNorth.AutoSize = true;
            this.radioNorth.Location = new System.Drawing.Point(60, 66);
            this.radioNorth.Name = "radioNorth";
            this.radioNorth.Size = new System.Drawing.Size(51, 17);
            this.radioNorth.TabIndex = 28;
            this.radioNorth.Text = "North";
            this.radioNorth.UseVisualStyleBackColor = true;
            this.radioNorth.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radioSite
            // 
            this.radioSite.AutoSize = true;
            this.radioSite.Checked = true;
            this.radioSite.Location = new System.Drawing.Point(111, 66);
            this.radioSite.Name = "radioSite";
            this.radioSite.Size = new System.Drawing.Size(84, 17);
            this.radioSite.TabIndex = 29;
            this.radioSite.TabStop = true;
            this.radioSite.Text = "Site heading";
            this.radioSite.UseVisualStyleBackColor = true;
            this.radioSite.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radioSRV
            // 
            this.radioSRV.AutoSize = true;
            this.radioSRV.Location = new System.Drawing.Point(201, 66);
            this.radioSRV.Name = "radioSRV";
            this.radioSRV.Size = new System.Drawing.Size(88, 17);
            this.radioSRV.TabIndex = 30;
            this.radioSRV.Text = "SRV heading";
            this.radioSRV.UseVisualStyleBackColor = true;
            this.radioSRV.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // SrvSurvey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1183, 810);
            this.Controls.Add(this.radioSRV);
            this.Controls.Add(this.radioSite);
            this.Controls.Add(this.radioNorth);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numScale);
            this.Controls.Add(this.comboCmdr);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.comboType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numSiteHeading);
            this.Controls.Add(this.txtSettlement);
            this.Controls.Add(this.txtSettlementLocation);
            this.Controls.Add(this.txtMode);
            this.Controls.Add(this.txtHeading);
            this.Controls.Add(this.txtLatLong);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtMsg);
            this.Location = new System.Drawing.Point(-1600, 20);
            this.Name = "SrvSurvey";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Srv Survey";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SrvSurvey_FormClosing);
            this.Load += new System.EventHandler(this.SrvBuddy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numSiteHeading)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.TextBox txtLatLong;
        private System.Windows.Forms.TextBox txtHeading;
        private System.Windows.Forms.TextBox txtMode;
        private System.Windows.Forms.TextBox txtSettlementLocation;
        private System.Windows.Forms.TextBox txtSettlement;
        private System.Windows.Forms.NumericUpDown numSiteHeading;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboType;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblMousePos;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox comboCmdr;
        private System.Windows.Forms.ToolStripStatusLabel lblMeters;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioNorth;
        private System.Windows.Forms.RadioButton radioSite;
        private System.Windows.Forms.RadioButton radioSRV;
    }
}

