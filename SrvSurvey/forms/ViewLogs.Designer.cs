
namespace SrvSurvey
{
    partial class ViewLogs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewLogs));
            txtLogs = new TextBox();
            btnClose = new FlatButton();
            btnClear = new FlatButton();
            btnOpenFolder = new FlatButton();
            btnCopy = new FlatButton();
            tableBottom = new TableLayoutPanel();
            tableBottom.SuspendLayout();
            SuspendLayout();
            // 
            // txtLogs
            // 
            txtLogs.BackColor = SystemColors.ScrollBar;
            txtLogs.BorderStyle = BorderStyle.FixedSingle;
            txtLogs.CausesValidation = false;
            resources.ApplyResources(txtLogs, "txtLogs");
            txtLogs.HideSelection = false;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            // 
            // btnClose
            // 
            resources.ApplyResources(btnClose, "btnClose");
            btnClose.Name = "btnClose";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnClear
            // 
            resources.ApplyResources(btnClear, "btnClear");
            btnClear.Name = "btnClear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnOpenFolder
            // 
            resources.ApplyResources(btnOpenFolder, "btnOpenFolder");
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // btnCopy
            // 
            resources.ApplyResources(btnCopy, "btnCopy");
            btnCopy.Name = "btnCopy";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // tableBottom
            // 
            resources.ApplyResources(tableBottom, "tableBottom");
            tableBottom.BackColor = SystemColors.ControlDark;
            tableBottom.Controls.Add(btnClear, 0, 0);
            tableBottom.Controls.Add(btnClose, 3, 0);
            tableBottom.Controls.Add(btnOpenFolder, 2, 0);
            tableBottom.Controls.Add(btnCopy, 1, 0);
            tableBottom.Name = "tableBottom";
            // 
            // ViewLogs
            // 
            AcceptButton = btnClose;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            Controls.Add(txtLogs);
            Controls.Add(tableBottom);
            Name = "ViewLogs";
            SizeGripStyle = SizeGripStyle.Show;
            Load += ViewLogs_Load;
            Shown += ViewLogs_Shown;
            tableBottom.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLogs;
        private FlatButton btnClose;
        private FlatButton btnClear;
        private FlatButton btnCopy;
        private FlatButton btnOpenFolder;
        private TableLayoutPanel tableBottom;
    }
}