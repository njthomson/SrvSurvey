
namespace SrvSurvey
{
    partial class JUNK
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
            this.toolRight = new System.Windows.Forms.StatusStrip();
            this.toolCommander = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.toolRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolRight
            // 
            this.toolRight.AutoSize = false;
            this.toolRight.BackColor = System.Drawing.SystemColors.ControlDark;
            this.toolRight.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolRight.Font = new System.Drawing.Font("Lucida Sans Typewriter", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolCommander,
            this.toolStatus});
            this.toolRight.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolRight.Location = new System.Drawing.Point(0, 0);
            this.toolRight.Name = "toolRight";
            this.toolRight.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.toolRight.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolRight.Size = new System.Drawing.Size(26, 538);
            this.toolRight.TabIndex = 0;
            this.toolRight.Text = "statusStrip1";
            this.toolRight.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical270;
            // 
            // toolCommander
            // 
            this.toolCommander.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolCommander.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.toolCommander.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolCommander.Name = "toolCommander";
            this.toolCommander.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolCommander.Size = new System.Drawing.Size(19, 107);
            this.toolCommander.Text = "Grinning2001";
            // 
            // toolStatus
            // 
            this.toolStatus.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStatus.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.toolStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStatus.Name = "toolStatus";
            this.toolStatus.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStatus.Size = new System.Drawing.Size(19, 67);
            this.toolStatus.Text = "Offline";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(450, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(467, 131);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(116, 20);
            this.textBox1.TabIndex = 2;
            // 
            // JUNK
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(679, 538);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toolRight);
            this.Font = new System.Drawing.Font("Lucida Sans Typewriter", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "JUNK";
            this.Text = "JUNK";
            this.Load += new System.EventHandler(this.JUNK_Load);
            this.toolRight.ResumeLayout(false);
            this.toolRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip toolRight;
        private System.Windows.Forms.ToolStripStatusLabel toolStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolCommander;
    }
}