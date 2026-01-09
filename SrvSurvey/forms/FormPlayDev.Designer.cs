namespace SrvSurvey.forms
{
    partial class FormPlayDev
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
            txtTitle = new TextBox();
            txtStuff = new TextBox();
            btnTemp = new Button();
            SuspendLayout();
            // 
            // txtTitle
            // 
            txtTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTitle.Location = new Point(0, 2);
            txtTitle.Name = "txtTitle";
            txtTitle.ReadOnly = true;
            txtTitle.Size = new Size(788, 23);
            txtTitle.TabIndex = 0;
            // 
            // txtStuff
            // 
            txtStuff.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtStuff.Location = new Point(0, 60);
            txtStuff.Multiline = true;
            txtStuff.Name = "txtStuff";
            txtStuff.ReadOnly = true;
            txtStuff.ScrollBars = ScrollBars.Both;
            txtStuff.Size = new Size(788, 378);
            txtStuff.TabIndex = 1;
            // 
            // btnTemp
            // 
            btnTemp.Location = new Point(0, 31);
            btnTemp.Name = "btnTemp";
            btnTemp.Size = new Size(75, 23);
            btnTemp.TabIndex = 2;
            btnTemp.Text = "Dump";
            btnTemp.UseVisualStyleBackColor = true;
            btnTemp.Click += btnTemp_Click;
            // 
            // FormPlayDev
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnTemp);
            Controls.Add(txtStuff);
            Controls.Add(txtTitle);
            Name = "FormPlayDev";
            Text = "Quest watcher";
            Load += FormPlayDev_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTitle;
        private TextBox txtStuff;
        private Button btnTemp;
    }
}