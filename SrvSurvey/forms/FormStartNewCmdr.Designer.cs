﻿namespace SrvSurvey.forms
{
    partial class FormStartNewCmdr
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
            comboCmdr = new ComboCmdr();
            label1 = new Label();
            btnStart = new Button();
            btnCancel = new Button();
            btnToggleWindow = new Button();
            label2 = new Label();
            txtCmdr = new TextBox();
            SuspendLayout();
            // 
            // comboCmdr
            // 
            comboCmdr.cmdrFid = null;
            comboCmdr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCmdr.FormattingEnabled = true;
            comboCmdr.Location = new Point(12, 83);
            comboCmdr.Name = "comboCmdr";
            comboCmdr.Size = new Size(426, 23);
            comboCmdr.TabIndex = 1;
            comboCmdr.SelectedIndexChanged += comboCmdr_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 63);
            label1.Name = "label1";
            label1.Size = new Size(297, 15);
            label1.TabIndex = 0;
            label1.Text = "Choose commander for the next instance of SrvSurvey:";
            // 
            // btnStart
            // 
            btnStart.Location = new Point(282, 120);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 2;
            btnStart.Text = "&Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(363, 120);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "&Close";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnToggleWindow
            // 
            btnToggleWindow.Location = new Point(12, 120);
            btnToggleWindow.Name = "btnToggleWindow";
            btnToggleWindow.Size = new Size(114, 23);
            btnToggleWindow.TabIndex = 4;
            btnToggleWindow.Text = "&Next window";
            btnToggleWindow.UseVisualStyleBackColor = true;
            btnToggleWindow.Click += btnToggleWindow_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(118, 15);
            label2.TabIndex = 5;
            label2.Text = "Current commander:";
            // 
            // txtCmdr
            // 
            txtCmdr.Location = new Point(12, 29);
            txtCmdr.Name = "txtCmdr";
            txtCmdr.ReadOnly = true;
            txtCmdr.Size = new Size(426, 23);
            txtCmdr.TabIndex = 6;
            // 
            // FormStartNewCmdr
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(448, 155);
            Controls.Add(txtCmdr);
            Controls.Add(comboCmdr);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(btnToggleWindow);
            Controls.Add(btnCancel);
            Controls.Add(btnStart);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormStartNewCmdr";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Start another SrvSurvey ?";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboCmdr comboCmdr;
        private Label label1;
        private Button btnStart;
        private Button btnCancel;
        private Button btnToggleWindow;
        private Label label2;
        private TextBox txtCmdr;
    }
}