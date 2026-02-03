
namespace SrvSurvey
{
    partial class FormGroundTarget
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGroundTarget));
            label2 = new Label();
            label3 = new Label();
            txtLat = new TextBox2();
            txtLong = new TextBox2();
            btnBegin = new DrawButton();
            btnCancel = new DrawButton();
            label6 = new Label();
            button1 = new DrawButton();
            btnTargetCurrent = new DrawButton();
            btnPaste = new DrawButton();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // txtLat
            // 
            resources.ApplyResources(txtLat, "txtLat");
            txtLat.Name = "txtLat";
            // 
            // txtLong
            // 
            resources.ApplyResources(txtLong, "txtLong");
            txtLong.Name = "txtLong";
            // 
            // btnBegin
            // 
            resources.ApplyResources(btnBegin, "btnBegin");
            btnBegin.Name = "btnBegin";
            btnBegin.UseVisualStyleBackColor = true;
            btnBegin.DrawBorder = true;
            btnBegin.Click += btnBegin_Click;
            // 
            // btnCancel
            // 
            resources.ApplyResources(btnCancel, "btnCancel");
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DrawBorder = true;
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            label6.Name = "label6";
            // 
            // button1
            // 
            resources.ApplyResources(button1, "button1");
            button1.Name = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.DrawBorder = true;
            button1.Click += button1_Click;
            // 
            // btnTargetCurrent
            // 
            resources.ApplyResources(btnTargetCurrent, "btnTargetCurrent");
            btnTargetCurrent.Name = "btnTargetCurrent";
            btnTargetCurrent.UseVisualStyleBackColor = true;
            btnTargetCurrent.DrawBorder = true;
            btnTargetCurrent.Click += btnTargetCurrent_Click;
            // 
            // btnPaste
            // 
            resources.ApplyResources(btnPaste, "btnPaste");
            btnPaste.Name = "btnPaste";
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.DrawBorder = true;
            btnPaste.Click += btnPaste_Click;
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(flowLayoutPanel1, "flowLayoutPanel1");
            flowLayoutPanel1.Controls.Add(btnBegin);
            flowLayoutPanel1.Controls.Add(btnCancel);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(button1, 0, 2);
            tableLayoutPanel1.Controls.Add(btnTargetCurrent, 1, 2);
            tableLayoutPanel1.Controls.Add(btnPaste, 2, 2);
            tableLayoutPanel1.Controls.Add(txtLong, 1, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(txtLat, 1, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // FormGroundTarget
            // 
            AcceptButton = btnBegin;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnCancel;
            Controls.Add(tableLayoutPanel1);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(label6);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormGroundTarget";
            ShowInTaskbar = false;
            Load += FormGroundTarget_Load;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label2;
        private Label label3;
        private TextBox2 txtLat;
        private TextBox2 txtLong;
        private DrawButton btnBegin;
        private DrawButton btnCancel;
        private Label label6;
        private DrawButton button1;
        private DrawButton btnTargetCurrent;
        private DrawButton btnPaste;
        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel1;
    }
}