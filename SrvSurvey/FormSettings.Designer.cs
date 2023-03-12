
namespace SrvSurvey
{
    partial class FormSettings
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCommander = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBioStatusAutoShow = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.trackOpacity = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.numOpacity = new System.Windows.Forms.NumericUpDown();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.checkFocusOnMinimize = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOpacity)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 408);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(394, 42);
            this.panel1.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(226, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(307, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.checkFocusOnMinimize);
            this.tabPage1.Controls.Add(this.numOpacity);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.trackOpacity);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.txtCommander);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(386, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Preferred Commander:";
            // 
            // txtCommander
            // 
            this.txtCommander.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommander.Location = new System.Drawing.Point(17, 29);
            this.txtCommander.Name = "txtCommander";
            this.txtCommander.Size = new System.Drawing.Size(361, 20);
            this.txtCommander.TabIndex = 1;
            this.txtCommander.Tag = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.checkBioStatusAutoShow);
            this.groupBox1.Location = new System.Drawing.Point(11, 75);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(367, 72);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bio scanning";
            // 
            // checkBioStatusAutoShow
            // 
            this.checkBioStatusAutoShow.AutoSize = true;
            this.checkBioStatusAutoShow.Checked = true;
            this.checkBioStatusAutoShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBioStatusAutoShow.Location = new System.Drawing.Point(6, 19);
            this.checkBioStatusAutoShow.Name = "checkBioStatusAutoShow";
            this.checkBioStatusAutoShow.Size = new System.Drawing.Size(174, 17);
            this.checkBioStatusAutoShow.TabIndex = 0;
            this.checkBioStatusAutoShow.Tag = "autoShowBioSummary";
            this.checkBioStatusAutoShow.Text = "Show biological signal summary";
            this.checkBioStatusAutoShow.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(6, 42);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(193, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Tag = "autoShowBioPlot";
            this.checkBox1.Text = "Show sample scan exclusion zones";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // trackOpacity
            // 
            this.trackOpacity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackOpacity.LargeChange = 10;
            this.trackOpacity.Location = new System.Drawing.Point(155, 153);
            this.trackOpacity.Maximum = 100;
            this.trackOpacity.Name = "trackOpacity";
            this.trackOpacity.Size = new System.Drawing.Size(223, 45);
            this.trackOpacity.SmallChange = 5;
            this.trackOpacity.TabIndex = 4;
            this.trackOpacity.TickFrequency = 10;
            this.trackOpacity.Scroll += new System.EventHandler(this.trackOpacity_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 162);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Overlay opacity:";
            // 
            // numOpacity
            // 
            this.numOpacity.Location = new System.Drawing.Point(97, 160);
            this.numOpacity.Name = "numOpacity";
            this.numOpacity.Size = new System.Drawing.Size(52, 20);
            this.numOpacity.TabIndex = 6;
            this.numOpacity.Tag = "Opacity";
            this.numOpacity.ValueChanged += new System.EventHandler(this.numOpacity_ValueChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(394, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // checkFocusOnMinimize
            // 
            this.checkFocusOnMinimize.AutoSize = true;
            this.checkFocusOnMinimize.Checked = true;
            this.checkFocusOnMinimize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkFocusOnMinimize.Location = new System.Drawing.Point(17, 204);
            this.checkFocusOnMinimize.Name = "checkFocusOnMinimize";
            this.checkFocusOnMinimize.Size = new System.Drawing.Size(304, 17);
            this.checkFocusOnMinimize.TabIndex = 2;
            this.checkFocusOnMinimize.Tag = "focusGameOnMinimize";
            this.checkFocusOnMinimize.Text = "Set focus on Elite Dangerous when minimizing SRVSurvey.";
            this.checkFocusOnMinimize.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(394, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SrvSurvey Settings";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.panel1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOpacity)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.NumericUpDown numOpacity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackOpacity;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBioStatusAutoShow;
        private System.Windows.Forms.TextBox txtCommander;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.CheckBox checkFocusOnMinimize;
    }
}