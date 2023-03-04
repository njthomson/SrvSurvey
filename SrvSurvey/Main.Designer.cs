
namespace SrvSurvey
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnBioScan = new System.Windows.Forms.Button();
            this.btnGroundTarget = new System.Windows.Forms.Button();
            this.toolRight = new System.Windows.Forms.StatusStrip();
            this.lblMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnViewLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.btnQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnQuit2 = new System.Windows.Forms.Button();
            this.txtTargetLatLong = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClearTarget = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.txtVehicle = new System.Windows.Forms.TextBox();
            this.txtCommander = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblAnalyzedCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtGenuses = new System.Windows.Forms.TextBox();
            this.lblBioSignalCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolRight.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBioScan
            // 
            this.btnBioScan.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBioScan.Location = new System.Drawing.Point(52, 412);
            this.btnBioScan.Name = "btnBioScan";
            this.btnBioScan.Size = new System.Drawing.Size(164, 21);
            this.btnBioScan.TabIndex = 1;
            this.btnBioScan.Text = "Bio Scanning";
            this.btnBioScan.UseVisualStyleBackColor = false;
            this.btnBioScan.Click += new System.EventHandler(this.btnBioScan_Click);
            // 
            // btnGroundTarget
            // 
            this.btnGroundTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGroundTarget.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnGroundTarget.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGroundTarget.Location = new System.Drawing.Point(136, 45);
            this.btnGroundTarget.Name = "btnGroundTarget";
            this.btnGroundTarget.Size = new System.Drawing.Size(119, 21);
            this.btnGroundTarget.TabIndex = 3;
            this.btnGroundTarget.Text = "Set target";
            this.btnGroundTarget.UseVisualStyleBackColor = false;
            this.btnGroundTarget.Click += new System.EventHandler(this.btnGroundTarget_Click);
            // 
            // toolRight
            // 
            this.toolRight.Font = new System.Drawing.Font("Lucida Sans Typewriter", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMode,
            this.toolStripDropDownButton1});
            this.toolRight.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolRight.Location = new System.Drawing.Point(0, 468);
            this.toolRight.Name = "toolRight";
            this.toolRight.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.toolRight.Size = new System.Drawing.Size(398, 22);
            this.toolRight.TabIndex = 7;
            this.toolRight.Text = "statusStrip1";
            this.toolRight.DoubleClick += new System.EventHandler(this.btnViewLogs_Click);
            // 
            // lblMode
            // 
            this.lblMode.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblMode.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lblMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblMode.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMode.Name = "lblMode";
            this.lblMode.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.lblMode.Size = new System.Drawing.Size(58, 17);
            this.lblMode.Text = "Offline";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnViewLogs,
            this.btnQuit,
            this.settingsToolStripMenuItem});
            this.toolStripDropDownButton1.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(39, 20);
            this.toolStripDropDownButton1.Text = "...";
            this.toolStripDropDownButton1.ToolTipText = "More logs";
            // 
            // btnViewLogs
            // 
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new System.Drawing.Size(133, 22);
            this.btnViewLogs.Text = "View logs";
            this.btnViewLogs.Click += new System.EventHandler(this.btnViewLogs_Click);
            // 
            // btnQuit
            // 
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(133, 22);
            this.btnQuit.Text = "Quit";
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // btnQuit2
            // 
            this.btnQuit2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuit2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnQuit2.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuit2.Location = new System.Drawing.Point(296, 412);
            this.btnQuit2.Name = "btnQuit2";
            this.btnQuit2.Size = new System.Drawing.Size(75, 23);
            this.btnQuit2.TabIndex = 8;
            this.btnQuit2.Text = "&Quit";
            this.btnQuit2.UseVisualStyleBackColor = true;
            this.btnQuit2.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // txtTargetLatLong
            // 
            this.txtTargetLatLong.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTargetLatLong.Location = new System.Drawing.Point(16, 19);
            this.txtTargetLatLong.Name = "txtTargetLatLong";
            this.txtTargetLatLong.ReadOnly = true;
            this.txtTargetLatLong.Size = new System.Drawing.Size(335, 20);
            this.txtTargetLatLong.TabIndex = 11;
            this.txtTargetLatLong.Text = "<none>";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 12);
            this.label2.TabIndex = 12;
            // 
            // btnClearTarget
            // 
            this.btnClearTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearTarget.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnClearTarget.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClearTarget.Location = new System.Drawing.Point(261, 45);
            this.btnClearTarget.Name = "btnClearTarget";
            this.btnClearTarget.Size = new System.Drawing.Size(90, 21);
            this.btnClearTarget.TabIndex = 13;
            this.btnClearTarget.Text = "Clear";
            this.btnClearTarget.UseVisualStyleBackColor = false;
            this.btnClearTarget.Click += new System.EventHandler(this.btnClearTarget_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnClearTarget);
            this.groupBox1.Controls.Add(this.btnGroundTarget);
            this.groupBox1.Controls.Add(this.txtTargetLatLong);
            this.groupBox1.Location = new System.Drawing.Point(12, 274);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 78);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Target lat/Long:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.txtLocation);
            this.groupBox2.Controls.Add(this.txtVehicle);
            this.groupBox2.Controls.Add(this.txtCommander);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(357, 69);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Commander:";
            // 
            // txtLocation
            // 
            this.txtLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLocation.Location = new System.Drawing.Point(108, 43);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.ReadOnly = true;
            this.txtLocation.Size = new System.Drawing.Size(243, 20);
            this.txtLocation.TabIndex = 12;
            this.txtLocation.Text = "<location>";
            // 
            // txtVehicle
            // 
            this.txtVehicle.Location = new System.Drawing.Point(16, 43);
            this.txtVehicle.Name = "txtVehicle";
            this.txtVehicle.ReadOnly = true;
            this.txtVehicle.Size = new System.Drawing.Size(86, 20);
            this.txtVehicle.TabIndex = 11;
            this.txtVehicle.Text = "<vehicle>";
            // 
            // txtCommander
            // 
            this.txtCommander.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommander.Location = new System.Drawing.Point(16, 19);
            this.txtCommander.Name = "txtCommander";
            this.txtCommander.ReadOnly = true;
            this.txtCommander.Size = new System.Drawing.Size(335, 20);
            this.txtCommander.TabIndex = 10;
            this.txtCommander.Text = "<cmdr name>";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lblAnalyzedCount);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.txtGenuses);
            this.groupBox3.Controls.Add(this.lblBioSignalCount);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 100);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(357, 119);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Bio scanning";
            // 
            // lblAnalyzedCount
            // 
            this.lblAnalyzedCount.AutoSize = true;
            this.lblAnalyzedCount.Location = new System.Drawing.Point(208, 25);
            this.lblAnalyzedCount.Name = "lblAnalyzedCount";
            this.lblAnalyzedCount.Size = new System.Drawing.Size(12, 12);
            this.lblAnalyzedCount.TabIndex = 13;
            this.lblAnalyzedCount.Text = "N";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(134, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "Analyzed:";
            // 
            // txtGenuses
            // 
            this.txtGenuses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGenuses.Location = new System.Drawing.Point(15, 49);
            this.txtGenuses.Margin = new System.Windows.Forms.Padding(12);
            this.txtGenuses.Multiline = true;
            this.txtGenuses.Name = "txtGenuses";
            this.txtGenuses.ReadOnly = true;
            this.txtGenuses.Size = new System.Drawing.Size(335, 54);
            this.txtGenuses.TabIndex = 11;
            this.txtGenuses.Text = "<genuses>";
            // 
            // lblBioSignalCount
            // 
            this.lblBioSignalCount.AutoSize = true;
            this.lblBioSignalCount.Location = new System.Drawing.Point(88, 25);
            this.lblBioSignalCount.Name = "lblBioSignalCount";
            this.lblBioSignalCount.Size = new System.Drawing.Size(12, 12);
            this.lblBioSignalCount.TabIndex = 1;
            this.lblBioSignalCount.Text = "N";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Detected:";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnQuit2;
            this.ClientSize = new System.Drawing.Size(398, 490);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBioScan);
            this.Controls.Add(this.btnQuit2);
            this.Controls.Add(this.toolRight);
            this.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Text = "Srv Survey";
            this.Load += new System.EventHandler(this.Main_Load);
            this.DoubleClick += new System.EventHandler(this.Main_DoubleClick);
            this.toolRight.ResumeLayout(false);
            this.toolRight.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnBioScan;
        private System.Windows.Forms.StatusStrip toolRight;
        private System.Windows.Forms.ToolStripStatusLabel lblMode;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem btnViewLogs;
        private System.Windows.Forms.ToolStripMenuItem btnQuit;
        private System.Windows.Forms.Button btnGroundTarget;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.Button btnQuit2;
        private System.Windows.Forms.TextBox txtTargetLatLong;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnClearTarget;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtCommander;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtGenuses;
        private System.Windows.Forms.Label lblBioSignalCount;
        private System.Windows.Forms.TextBox txtVehicle;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.Label lblAnalyzedCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer timer1;
    }
}