﻿namespace SrvSurvey.forms
{
    partial class FormBuildAssign
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
            label1 = new Label();
            label2 = new Label();
            txtCmdr = new TextBox();
            btnAssign = new Button();
            comboCommodity = new ComboBox();
            btnRemove = new Button();
            label3 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 49);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 0;
            label1.Text = "Commander:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 78);
            label2.Name = "label2";
            label2.Size = new Size(74, 15);
            label2.TabIndex = 1;
            label2.Text = "Commodity:";
            // 
            // txtCmdr
            // 
            txtCmdr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCmdr.Location = new Point(98, 46);
            txtCmdr.Name = "txtCmdr";
            txtCmdr.Size = new Size(284, 23);
            txtCmdr.TabIndex = 2;
            // 
            // btnAssign
            // 
            btnAssign.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAssign.Location = new Point(226, 106);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(75, 23);
            btnAssign.TabIndex = 4;
            btnAssign.Text = "Assign";
            btnAssign.UseVisualStyleBackColor = true;
            btnAssign.Click += btnAssign_Click;
            // 
            // comboCommodity
            // 
            comboCommodity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboCommodity.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCommodity.FormattingEnabled = true;
            comboCommodity.Location = new Point(98, 75);
            comboCommodity.Name = "comboCommodity";
            comboCommodity.Size = new Size(284, 23);
            comboCommodity.TabIndex = 5;
            // 
            // btnRemove
            // 
            btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRemove.Location = new Point(307, 106);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(75, 23);
            btnRemove.TabIndex = 6;
            btnRemove.Text = "Remove";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // label3
            // 
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(370, 34);
            label3.TabIndex = 7;
            label3.Text = "Assign a commodity to commanders to help groups stay organized.";
            // 
            // FormBuildAssign
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(394, 141);
            Controls.Add(label3);
            Controls.Add(btnRemove);
            Controls.Add(comboCommodity);
            Controls.Add(btnAssign);
            Controls.Add(txtCmdr);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "FormBuildAssign";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Assign Commander Commodity";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox txtCmdr;
        private Button btnAssign;
        private ComboBox comboCommodity;
        private Button btnRemove;
        private Label label3;
    }
}