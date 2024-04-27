namespace SrvSurvey
{
    partial class FormPostProcess
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPostProcess));
            statusStrip1 = new StatusStrip();
            btnStart = new Button();
            progress = new ProgressBar();
            lblProgress = new Label();
            btnSystems = new Button();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // btnStart
            // 
            btnStart.Location = new Point(12, 12);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(111, 23);
            btnStart.TabIndex = 1;
            btnStart.Text = "Process journals";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // progress
            // 
            progress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progress.Location = new Point(12, 41);
            progress.Name = "progress";
            progress.Size = new Size(776, 23);
            progress.TabIndex = 2;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(129, 16);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(130, 15);
            lblProgress.TabIndex = 3;
            lblProgress.Text = "Processing ? of ? files ...";
            // 
            // btnSystems
            // 
            btnSystems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSystems.Location = new Point(678, 12);
            btnSystems.Name = "btnSystems";
            btnSystems.Size = new Size(110, 23);
            btnSystems.TabIndex = 4;
            btnSystems.Text = "Process systems";
            btnSystems.UseVisualStyleBackColor = true;
            btnSystems.Click += btnSystems_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(12, 70);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Size = new Size(776, 355);
            textBox1.TabIndex = 5;
            // 
            // FormPostProcess
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox1);
            Controls.Add(btnSystems);
            Controls.Add(lblProgress);
            Controls.Add(progress);
            Controls.Add(btnStart);
            Controls.Add(statusStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormPostProcess";
            Text = "Post Process Journals";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private Button btnStart;
        private ProgressBar progress;
        private Label lblProgress;
        private Button btnSystems;
        private TextBox textBox1;
    }
}