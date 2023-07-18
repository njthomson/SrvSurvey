namespace SrvSurvey
{
    partial class FormSphereLimit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSphereLimit));
            label1 = new Label();
            label2 = new Label();
            btnCancel = new Button();
            txtStarPos = new TextBox();
            label3 = new Label();
            numRadius = new NumericUpDown();
            btnAccept = new Button();
            label4 = new Label();
            comboSystemName = new ComboBox();
            btnDisable = new Button();
            label5 = new Label();
            label6 = new Label();
            txtCurrentSystem = new TextBox();
            txtCurrentDistance = new TextBox();
            ((System.ComponentModel.ISupportInitialize)numRadius).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 59);
            label1.Name = "label1";
            label1.Size = new Size(116, 15);
            label1.TabIndex = 0;
            label1.Text = "Enter central system:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(47, 116);
            label2.Name = "label2";
            label2.Size = new Size(81, 15);
            label2.TabIndex = 2;
            label2.Text = "Sphere radius:";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(417, 215);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtStarPos
            // 
            txtStarPos.Location = new Point(134, 85);
            txtStarPos.Name = "txtStarPos";
            txtStarPos.ReadOnly = true;
            txtStarPos.Size = new Size(358, 23);
            txtStarPos.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(76, 89);
            label3.Name = "label3";
            label3.Size = new Size(52, 15);
            label3.TabIndex = 6;
            label3.Text = "Star pos:";
            // 
            // numRadius
            // 
            numRadius.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numRadius.Location = new Point(134, 114);
            numRadius.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numRadius.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRadius.Name = "numRadius";
            numRadius.Size = new Size(120, 23);
            numRadius.TabIndex = 8;
            numRadius.TextAlign = HorizontalAlignment.Right;
            numRadius.ThousandsSeparator = true;
            numRadius.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // btnAccept
            // 
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAccept.Location = new Point(255, 215);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(75, 23);
            btnAccept.TabIndex = 9;
            btnAccept.Text = "&Activate";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 18);
            label4.Name = "label4";
            label4.Size = new Size(460, 15);
            label4.TabIndex = 10;
            label4.Text = "Spherical limit helps navigating to systems within a given distance of a central system:";
            // 
            // comboSystemName
            // 
            comboSystemName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboSystemName.FormattingEnabled = true;
            comboSystemName.Location = new Point(134, 56);
            comboSystemName.Name = "comboSystemName";
            comboSystemName.Size = new Size(358, 23);
            comboSystemName.TabIndex = 11;
            comboSystemName.SelectedIndexChanged += comboSystemName_SelectedIndexChanged;
            comboSystemName.TextUpdate += comboSystemName_TextUpdate;
            // 
            // btnDisable
            // 
            btnDisable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDisable.Location = new Point(336, 215);
            btnDisable.Name = "btnDisable";
            btnDisable.Size = new Size(75, 23);
            btnDisable.TabIndex = 12;
            btnDisable.Text = "&Disable";
            btnDisable.UseVisualStyleBackColor = true;
            btnDisable.Click += btnDisable_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(38, 146);
            label5.Name = "label5";
            label5.Size = new Size(90, 15);
            label5.TabIndex = 13;
            label5.Text = "Current system:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(73, 175);
            label6.Name = "label6";
            label6.Size = new Size(55, 15);
            label6.TabIndex = 15;
            label6.Text = "Distance:";
            // 
            // txtCurrentSystem
            // 
            txtCurrentSystem.Location = new Point(134, 143);
            txtCurrentSystem.Name = "txtCurrentSystem";
            txtCurrentSystem.ReadOnly = true;
            txtCurrentSystem.Size = new Size(358, 23);
            txtCurrentSystem.TabIndex = 16;
            // 
            // txtCurrentDistance
            // 
            txtCurrentDistance.Location = new Point(134, 172);
            txtCurrentDistance.Name = "txtCurrentDistance";
            txtCurrentDistance.ReadOnly = true;
            txtCurrentDistance.Size = new Size(120, 23);
            txtCurrentDistance.TabIndex = 17;
            // 
            // FormSphereLimit
            // 
            AcceptButton = btnAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(504, 250);
            Controls.Add(txtCurrentDistance);
            Controls.Add(txtCurrentSystem);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(btnDisable);
            Controls.Add(comboSystemName);
            Controls.Add(label4);
            Controls.Add(btnAccept);
            Controls.Add(numRadius);
            Controls.Add(txtStarPos);
            Controls.Add(label3);
            Controls.Add(btnCancel);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSphereLimit";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Spherical limit";
            ((System.ComponentModel.ISupportInitialize)numRadius).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Button btnCancel;
        private TextBox txtStarPos;
        private Label label3;
        private NumericUpDown numRadius;
        private Button btnAccept;
        private Label label4;
        private ComboBox comboSystemName;
        private Button btnDisable;
        private Label label5;
        private Label label6;
        private TextBox txtCurrentSystem;
        private TextBox txtCurrentDistance;
    }
}