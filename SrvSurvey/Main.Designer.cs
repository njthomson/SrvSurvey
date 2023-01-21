
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnSurvey = new System.Windows.Forms.Button();
            this.btnBioScan = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnGroundTarget = new System.Windows.Forms.Button();
            this.toolRight = new System.Windows.Forms.StatusStrip();
            this.lblCommander = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnViewLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.btnQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnQuit2 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.toolRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSurvey
            // 
            this.btnSurvey.Location = new System.Drawing.Point(3, 3);
            this.btnSurvey.Name = "btnSurvey";
            this.btnSurvey.Size = new System.Drawing.Size(164, 21);
            this.btnSurvey.TabIndex = 0;
            this.btnSurvey.Text = "Survey Ruins";
            this.btnSurvey.UseVisualStyleBackColor = true;
            this.btnSurvey.Click += new System.EventHandler(this.btnSurvey_Click);
            // 
            // btnBioScan
            // 
            this.btnBioScan.Location = new System.Drawing.Point(3, 30);
            this.btnBioScan.Name = "btnBioScan";
            this.btnBioScan.Size = new System.Drawing.Size(164, 21);
            this.btnBioScan.TabIndex = 1;
            this.btnBioScan.Text = "Bio Scanning";
            this.btnBioScan.UseVisualStyleBackColor = true;
            this.btnBioScan.Click += new System.EventHandler(this.btnBioScan_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSettings);
            this.panel1.Controls.Add(this.btnGroundTarget);
            this.panel1.Controls.Add(this.btnSurvey);
            this.panel1.Controls.Add(this.btnBioScan);
            this.panel1.Location = new System.Drawing.Point(33, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(173, 190);
            this.panel1.TabIndex = 6;
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(3, 84);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(164, 21);
            this.btnSettings.TabIndex = 4;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnGroundTarget
            // 
            this.btnGroundTarget.Location = new System.Drawing.Point(3, 57);
            this.btnGroundTarget.Name = "btnGroundTarget";
            this.btnGroundTarget.Size = new System.Drawing.Size(164, 21);
            this.btnGroundTarget.TabIndex = 3;
            this.btnGroundTarget.Text = "Ground Target";
            this.btnGroundTarget.UseVisualStyleBackColor = true;
            this.btnGroundTarget.Click += new System.EventHandler(this.btnGroundTarget_Click);
            // 
            // toolRight
            // 
            this.toolRight.AutoSize = false;
            this.toolRight.BackColor = System.Drawing.SystemColors.ControlDark;
            this.toolRight.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolRight.Font = new System.Drawing.Font("Lucida Sans Typewriter", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCommander,
            this.lblMode,
            this.toolStripDropDownButton1});
            this.toolRight.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolRight.Location = new System.Drawing.Point(0, 0);
            this.toolRight.Name = "toolRight";
            this.toolRight.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.toolRight.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolRight.Size = new System.Drawing.Size(30, 229);
            this.toolRight.TabIndex = 7;
            this.toolRight.Text = "statusStrip1";
            this.toolRight.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical270;
            // 
            // lblCommander
            // 
            this.lblCommander.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblCommander.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lblCommander.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblCommander.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCommander.Name = "lblCommander";
            this.lblCommander.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.lblCommander.Size = new System.Drawing.Size(16, 93);
            this.lblCommander.Text = "Grinning2001";
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
            this.lblMode.Size = new System.Drawing.Size(16, 58);
            this.lblMode.Text = "Offline";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnViewLogs,
            this.btnQuit,
            this.settingsToolStripMenuItem});
            this.toolStripDropDownButton1.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(16, 37);
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
            this.btnQuit2.Location = new System.Drawing.Point(275, 194);
            this.btnQuit2.Name = "btnQuit2";
            this.btnQuit2.Size = new System.Drawing.Size(75, 23);
            this.btnQuit2.TabIndex = 8;
            this.btnQuit2.Text = "&Quit";
            this.btnQuit2.UseVisualStyleBackColor = true;
            this.btnQuit2.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CancelButton = this.btnQuit2;
            this.ClientSize = new System.Drawing.Size(362, 229);
            this.Controls.Add(this.btnQuit2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolRight);
            this.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.ShowIcon = false;
            this.Text = "SrvSurvey";
            this.Load += new System.EventHandler(this.Main_Load);
            this.DoubleClick += new System.EventHandler(this.Main_DoubleClick);
            this.panel1.ResumeLayout(false);
            this.toolRight.ResumeLayout(false);
            this.toolRight.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSurvey;
        private System.Windows.Forms.Button btnBioScan;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip toolRight;
        private System.Windows.Forms.ToolStripStatusLabel lblCommander;
        private System.Windows.Forms.ToolStripStatusLabel lblMode;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem btnViewLogs;
        private System.Windows.Forms.ToolStripMenuItem btnQuit;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnGroundTarget;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.Button btnQuit2;
    }
}