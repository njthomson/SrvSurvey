
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
            txtLat = new TextBox();
            txtLong = new TextBox();
            btnBegin = new Button();
            btnCancel = new Button();
            label6 = new Label();
            button1 = new Button();
            btnTargetCurrent = new Button();
            btnPaste = new Button();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 81);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(50, 15);
            label2.TabIndex = 7;
            label2.Text = "Latitute:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(14, 111);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(64, 15);
            label3.TabIndex = 1;
            label3.Text = "Longitude:";
            // 
            // txtLat
            // 
            txtLat.Location = new Point(88, 77);
            txtLat.Margin = new Padding(4, 3, 4, 3);
            txtLat.Name = "txtLat";
            txtLat.Size = new Size(186, 23);
            txtLat.TabIndex = 0;
            txtLat.Text = "+12.34";
            // 
            // txtLong
            // 
            txtLong.Location = new Point(88, 107);
            txtLong.Margin = new Padding(4, 3, 4, 3);
            txtLong.Name = "txtLong";
            txtLong.Size = new Size(186, 23);
            txtLong.TabIndex = 2;
            txtLong.Text = "-10.0";
            // 
            // btnBegin
            // 
            btnBegin.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBegin.Location = new Point(312, 183);
            btnBegin.Margin = new Padding(4, 3, 4, 3);
            btnBegin.Name = "btnBegin";
            btnBegin.Size = new Size(88, 27);
            btnBegin.TabIndex = 4;
            btnBegin.Text = "&Set target";
            btnBegin.UseVisualStyleBackColor = true;
            btnBegin.Click += btnBegin_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(406, 183);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(14, 10);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(464, 45);
            label6.TabIndex = 6;
            label6.Text = "Got a tip from a mysterious stranger to go to some Lat/Long on some planet or moon?\r\n\r\nEnter some Lat/Long position and guidance will appear when you approach.";
            // 
            // button1
            // 
            button1.Location = new Point(18, 136);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(88, 27);
            button1.TabIndex = 3;
            button1.Text = "&Clear target";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnTargetCurrent
            // 
            btnTargetCurrent.Location = new Point(114, 136);
            btnTargetCurrent.Margin = new Padding(4, 3, 4, 3);
            btnTargetCurrent.Name = "btnTargetCurrent";
            btnTargetCurrent.Size = new Size(145, 27);
            btnTargetCurrent.TabIndex = 8;
            btnTargetCurrent.Text = "&Target current location";
            btnTargetCurrent.UseVisualStyleBackColor = true;
            btnTargetCurrent.Click += btnTargetCurrent_Click;
            // 
            // btnPaste
            // 
            btnPaste.Image = (Image)resources.GetObject("btnPaste.Image");
            btnPaste.ImageAlign = ContentAlignment.MiddleLeft;
            btnPaste.Location = new Point(266, 136);
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new Size(87, 27);
            btnPaste.TabIndex = 9;
            btnPaste.Text = "   Paste";
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.Click += btnPaste_Click;
            // 
            // FormGroundTarget
            // 
            AcceptButton = btnBegin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(507, 223);
            Controls.Add(btnPaste);
            Controls.Add(btnTargetCurrent);
            Controls.Add(button1);
            Controls.Add(label6);
            Controls.Add(btnCancel);
            Controls.Add(btnBegin);
            Controls.Add(txtLong);
            Controls.Add(txtLat);
            Controls.Add(label3);
            Controls.Add(label2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormGroundTarget";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Set lat/long co-ordinates";
            Load += FormGroundTarget_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label2;
        private Label label3;
        private TextBox txtLat;
        private TextBox txtLong;
        private Button btnBegin;
        private Button btnCancel;
        private Label label6;
        private Button button1;
        private Button btnTargetCurrent;
        private Button btnPaste;
    }
}