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
            btnCancel = new FlatButton();
            txtStarPos = new TextBox();
            label3 = new Label();
            numRadius = new NumericUpDown();
            btnAccept = new FlatButton();
            label4 = new Label();
            comboSystemName = new ComboStarSystem();
            btnDisable = new FlatButton();
            label5 = new Label();
            label6 = new Label();
            txtCurrentSystem = new TextBox();
            txtCurrentDistance = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)numRadius).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // btnCancel
            // 
            resources.ApplyResources(btnCancel, "btnCancel");
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtStarPos
            // 
            txtStarPos.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(txtStarPos, "txtStarPos");
            txtStarPos.Name = "txtStarPos";
            txtStarPos.ReadOnly = true;
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // numRadius
            // 
            numRadius.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            resources.ApplyResources(numRadius, "numRadius");
            numRadius.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numRadius.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRadius.Name = "numRadius";
            numRadius.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // btnAccept
            // 
            resources.ApplyResources(btnAccept, "btnAccept");
            btnAccept.Name = "btnAccept";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.FlatStyle = FlatStyle.System;
            label4.Name = "label4";
            // 
            // comboSystemName
            // 
            resources.ApplyResources(comboSystemName, "comboSystemName");
            comboSystemName.FormattingEnabled = true;
            comboSystemName.Name = "comboSystemName";
            comboSystemName.SelectedSystem = null;
            comboSystemName.updateOnJump = false;
            comboSystemName.SelectedIndexChanged += comboSystemName_SelectedIndexChanged;
            // 
            // btnDisable
            // 
            resources.ApplyResources(btnDisable, "btnDisable");
            btnDisable.Name = "btnDisable";
            btnDisable.UseVisualStyleBackColor = true;
            btnDisable.Click += btnDisable_Click;
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            label6.Name = "label6";
            // 
            // txtCurrentSystem
            // 
            txtCurrentSystem.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(txtCurrentSystem, "txtCurrentSystem");
            txtCurrentSystem.Name = "txtCurrentSystem";
            txtCurrentSystem.ReadOnly = true;
            // 
            // txtCurrentDistance
            // 
            txtCurrentDistance.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(txtCurrentDistance, "txtCurrentDistance");
            txtCurrentDistance.Name = "txtCurrentDistance";
            txtCurrentDistance.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(txtCurrentDistance, 1, 4);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(txtCurrentSystem, 1, 3);
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            tableLayoutPanel1.Controls.Add(label6, 0, 4);
            tableLayoutPanel1.Controls.Add(comboSystemName, 1, 0);
            tableLayoutPanel1.Controls.Add(label5, 0, 3);
            tableLayoutPanel1.Controls.Add(numRadius, 1, 2);
            tableLayoutPanel1.Controls.Add(txtStarPos, 1, 1);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(flowLayoutPanel1, "flowLayoutPanel1");
            flowLayoutPanel1.Controls.Add(btnAccept);
            flowLayoutPanel1.Controls.Add(btnDisable);
            flowLayoutPanel1.Controls.Add(btnCancel);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // FormSphereLimit
            // 
            AcceptButton = btnAccept;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            Controls.Add(flowLayoutPanel1);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(label4);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSphereLimit";
            ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)numRadius).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private FlatButton btnCancel;
        private TextBox txtStarPos;
        private Label label3;
        private NumericUpDown numRadius;
        private FlatButton btnAccept;
        private Label label4;
        private ComboStarSystem comboSystemName;
        private FlatButton btnDisable;
        private Label label5;
        private Label label6;
        private TextBox txtCurrentSystem;
        private TextBox txtCurrentDistance;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}