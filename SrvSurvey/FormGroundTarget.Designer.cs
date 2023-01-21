
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBody = new System.Windows.Forms.TextBox();
            this.txtLat = new System.Windows.Forms.TextBox();
            this.txtLong = new System.Windows.Forms.TextBox();
            this.txtDist = new System.Windows.Forms.TextBox();
            this.btnBegin = new System.Windows.Forms.Button();
            this.txtSystem = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target body:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Latitute:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(237, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Longitude:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(82, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Distance to target:";
            // 
            // txtBody
            // 
            this.txtBody.Location = new System.Drawing.Point(85, 38);
            this.txtBody.Name = "txtBody";
            this.txtBody.Size = new System.Drawing.Size(361, 20);
            this.txtBody.TabIndex = 4;
            this.txtBody.Text = "V902 Centauri A 1";
            // 
            // txtLat
            // 
            this.txtLat.Location = new System.Drawing.Point(85, 64);
            this.txtLat.Name = "txtLat";
            this.txtLat.Size = new System.Drawing.Size(146, 20);
            this.txtLat.TabIndex = 5;
            this.txtLat.Text = "10.0";
            // 
            // txtLong
            // 
            this.txtLong.Location = new System.Drawing.Point(300, 64);
            this.txtLong.Name = "txtLong";
            this.txtLong.Size = new System.Drawing.Size(146, 20);
            this.txtLong.TabIndex = 7;
            this.txtLong.Text = "40.0";
            // 
            // txtDist
            // 
            this.txtDist.Location = new System.Drawing.Point(182, 102);
            this.txtDist.Name = "txtDist";
            this.txtDist.Size = new System.Drawing.Size(145, 20);
            this.txtDist.TabIndex = 6;
            // 
            // btnBegin
            // 
            this.btnBegin.Location = new System.Drawing.Point(371, 149);
            this.btnBegin.Name = "btnBegin";
            this.btnBegin.Size = new System.Drawing.Size(75, 23);
            this.btnBegin.TabIndex = 8;
            this.btnBegin.Text = "&Begin";
            this.btnBegin.UseVisualStyleBackColor = true;
            this.btnBegin.Click += new System.EventHandler(this.btnBegin_Click);
            // 
            // txtSystem
            // 
            this.txtSystem.Location = new System.Drawing.Point(85, 12);
            this.txtSystem.Name = "txtSystem";
            this.txtSystem.Size = new System.Drawing.Size(361, 20);
            this.txtSystem.TabIndex = 10;
            this.txtSystem.Text = "V902 Centauri";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Target system:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(15, 149);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormGroundTarget
            // 
            this.AcceptButton = this.btnBegin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(458, 184);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtSystem);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnBegin);
            this.Controls.Add(this.txtLong);
            this.Controls.Add(this.txtDist);
            this.Controls.Add(this.txtLat);
            this.Controls.Add(this.txtBody);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormGroundTarget";
            this.Text = "Select ground target";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBody;
        private System.Windows.Forms.TextBox txtLat;
        private System.Windows.Forms.TextBox txtLong;
        private System.Windows.Forms.TextBox txtDist;
        private System.Windows.Forms.Button btnBegin;
        private System.Windows.Forms.TextBox txtSystem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnCancel;
    }
}