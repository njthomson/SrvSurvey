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
            btnStart = new Button();
            progress = new ProgressBar();
            lblProgress = new Label();
            btnSystems = new Button();
            lblDesc = new Label();
            linkLabel1 = new LinkLabel();
            lblCmdr = new Label();
            lblStartDate = new Label();
            dateTimePicker = new DateTimePicker();
            comboCmdr = new ComboBox();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(565, 127);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(223, 23);
            btnStart.TabIndex = 1;
            btnStart.Text = "Process journals";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // progress
            // 
            progress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progress.Location = new Point(12, 181);
            progress.Name = "progress";
            progress.Size = new Size(776, 23);
            progress.TabIndex = 2;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(12, 154);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(130, 15);
            lblProgress.TabIndex = 3;
            lblProgress.Text = "Processing ? of ? files ...";
            // 
            // btnSystems
            // 
            btnSystems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSystems.Enabled = false;
            btnSystems.Location = new Point(12, 104);
            btnSystems.Name = "btnSystems";
            btnSystems.Size = new Size(110, 23);
            btnSystems.TabIndex = 4;
            btnSystems.Text = "Process systems";
            btnSystems.UseVisualStyleBackColor = true;
            btnSystems.Visible = false;
            btnSystems.Click += btnSystems_Click;
            // 
            // lblDesc
            // 
            lblDesc.Location = new Point(12, 9);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(776, 38);
            lblDesc.TabIndex = 6;
            lblDesc.Text = resources.GetString("lblDesc.Text");
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(12, 47);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(124, 15);
            linkLabel1.TabIndex = 7;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Learn more in the wiki";
            // 
            // lblCmdr
            // 
            lblCmdr.AutoSize = true;
            lblCmdr.Location = new Point(12, 78);
            lblCmdr.Name = "lblCmdr";
            lblCmdr.Size = new Size(77, 15);
            lblCmdr.TabIndex = 8;
            lblCmdr.Text = "Commander:";
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Location = new Point(499, 78);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(60, 15);
            lblStartDate.TabIndex = 9;
            lblStartDate.Text = "Start date:";
            // 
            // dateTimePicker
            // 
            dateTimePicker.Location = new Point(565, 75);
            dateTimePicker.Name = "dateTimePicker";
            dateTimePicker.Size = new Size(223, 23);
            dateTimePicker.TabIndex = 10;
            dateTimePicker.ValueChanged += dateTimePicker_ValueChanged;
            // 
            // comboCmdr
            // 
            comboCmdr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCmdr.FormattingEnabled = true;
            comboCmdr.Location = new Point(95, 75);
            comboCmdr.Name = "comboCmdr";
            comboCmdr.Size = new Size(398, 23);
            comboCmdr.TabIndex = 11;
            // 
            // FormPostProcess
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 216);
            Controls.Add(comboCmdr);
            Controls.Add(dateTimePicker);
            Controls.Add(lblStartDate);
            Controls.Add(lblCmdr);
            Controls.Add(linkLabel1);
            Controls.Add(lblDesc);
            Controls.Add(btnSystems);
            Controls.Add(lblProgress);
            Controls.Add(progress);
            Controls.Add(btnStart);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormPostProcess";
            Text = "Post Process Journals";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnStart;
        private ProgressBar progress;
        private Label lblProgress;
        private Button btnSystems;
        private Label lblDesc;
        private LinkLabel linkLabel1;
        private Label lblCmdr;
        private Label lblStartDate;
        private DateTimePicker dateTimePicker;
        private ComboBox comboCmdr;
    }
}