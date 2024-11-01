namespace SrvSurvey
{
    partial class FormSetKeyChord
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSetKeyChord));
            textChord = new TextBox();
            btnCancel = new Button();
            btnAccept = new Button();
            label1 = new Label();
            lblInvalid = new Label();
            btnDisable = new Button();
            flowButtons = new FlowLayoutPanel();
            flowButtons.SuspendLayout();
            SuspendLayout();
            // 
            // textChord
            // 
            resources.ApplyResources(textChord, "textChord");
            textChord.Name = "textChord";
            textChord.PreviewKeyDown += textChord_PreviewKeyDown;
            // 
            // btnCancel
            // 
            resources.ApplyResources(btnCancel, "btnCancel");
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            resources.ApplyResources(btnAccept, "btnAccept");
            btnAccept.DialogResult = DialogResult.OK;
            btnAccept.Name = "btnAccept";
            btnAccept.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.FlatStyle = FlatStyle.System;
            label1.Name = "label1";
            // 
            // lblInvalid
            // 
            resources.ApplyResources(lblInvalid, "lblInvalid");
            lblInvalid.FlatStyle = FlatStyle.System;
            lblInvalid.Name = "lblInvalid";
            // 
            // btnDisable
            // 
            resources.ApplyResources(btnDisable, "btnDisable");
            btnDisable.Name = "btnDisable";
            btnDisable.UseVisualStyleBackColor = true;
            btnDisable.Click += btnDisable_Click;
            // 
            // flowButtons
            // 
            resources.ApplyResources(flowButtons, "flowButtons");
            flowButtons.Controls.Add(btnAccept);
            flowButtons.Controls.Add(btnDisable);
            flowButtons.Controls.Add(btnCancel);
            flowButtons.Name = "flowButtons";
            // 
            // FormSetKeyChord
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowButtons);
            Controls.Add(lblInvalid);
            Controls.Add(label1);
            Controls.Add(textChord);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSetKeyChord";
            ShowInTaskbar = false;
            KeyDown += FormSetKeyChord_KeyDown;
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textChord;
        private Button btnCancel;
        private Button btnAccept;
        private Label label1;
        private Label lblInvalid;
        private Button btnDisable;
        private FlowLayoutPanel flowButtons;
    }
}