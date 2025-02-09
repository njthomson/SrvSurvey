namespace SrvSurvey
{
    partial class MainView
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
            txtLocation = new TextBox();
            SuspendLayout();
            // 
            // txtLocation
            // 
            txtLocation.BackColor = Color.Black;
            txtLocation.BorderStyle = BorderStyle.None;
            txtLocation.ForeColor = Color.Yellow;
            txtLocation.Location = new Point(36, 54);
            txtLocation.Name = "txtLocation";
            txtLocation.Size = new Size(362, 16);
            txtLocation.TabIndex = 0;
            txtLocation.Text = "hello";
            // 
            // MainView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(txtLocation);
            Name = "MainView";
            Size = new Size(429, 453);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLocation;
    }
}
