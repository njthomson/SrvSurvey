namespace SrvSurvey.forms
{
    partial class FormRoute
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRoute));
            list = new ListView();
            colSystem = new ColumnHeader();
            colDistance = new ColumnHeader();
            colNotes = new ColumnHeader();
            status = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            toolStrip1 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            btnImportSpanshUrl = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            btnNamesFromClipboard = new ToolStripMenuItem();
            btnNamesFromFile = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            btnActive = new ToolStripButton();
            btnAutoCopy = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            btnSave = new ToolStripButton();
            lblIntro = new Label();
            table = new TableLayoutPanel();
            status.SuspendLayout();
            toolStrip1.SuspendLayout();
            table.SuspendLayout();
            SuspendLayout();
            // 
            // list
            // 
            resources.ApplyResources(list, "list");
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colSystem, colDistance, colNotes });
            list.Name = "list";
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ItemChecked += list_ItemChecked;
            // 
            // colSystem
            // 
            resources.ApplyResources(colSystem, "colSystem");
            // 
            // colDistance
            // 
            resources.ApplyResources(colDistance, "colDistance");
            // 
            // colNotes
            // 
            resources.ApplyResources(colNotes, "colNotes");
            // 
            // status
            // 
            resources.ApplyResources(status, "status");
            status.Items.AddRange(new ToolStripItem[] { lblStatus });
            status.Name = "status";
            // 
            // lblStatus
            // 
            resources.ApplyResources(lblStatus, "lblStatus");
            lblStatus.BorderStyle = Border3DStyle.SunkenOuter;
            lblStatus.Name = "lblStatus";
            lblStatus.Spring = true;
            // 
            // toolStrip1
            // 
            resources.ApplyResources(toolStrip1, "toolStrip1");
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, toolStripSeparator2, btnActive, btnAutoCopy, toolStripSeparator3, btnSave });
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = ToolStripRenderMode.System;
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { btnImportSpanshUrl, toolStripSeparator1, btnNamesFromClipboard, btnNamesFromFile });
            resources.ApplyResources(toolStripDropDownButton1, "toolStripDropDownButton1");
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            // 
            // btnImportSpanshUrl
            // 
            btnImportSpanshUrl.Name = "btnImportSpanshUrl";
            resources.ApplyResources(btnImportSpanshUrl, "btnImportSpanshUrl");
            btnImportSpanshUrl.Click += btnImportSpanshUrl_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // btnNamesFromClipboard
            // 
            btnNamesFromClipboard.Name = "btnNamesFromClipboard";
            resources.ApplyResources(btnNamesFromClipboard, "btnNamesFromClipboard");
            btnNamesFromClipboard.Click += btnNamesFromClipboard_Click;
            // 
            // btnNamesFromFile
            // 
            btnNamesFromFile.Name = "btnNamesFromFile";
            resources.ApplyResources(btnNamesFromFile, "btnNamesFromFile");
            btnNamesFromFile.Click += btnNamesFromFile_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            // 
            // btnActive
            // 
            btnActive.Checked = true;
            btnActive.CheckOnClick = true;
            btnActive.CheckState = CheckState.Checked;
            btnActive.DisplayStyle = ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(btnActive, "btnActive");
            btnActive.Name = "btnActive";
            btnActive.CheckedChanged += btnActive_CheckedChanged;
            // 
            // btnAutoCopy
            // 
            btnAutoCopy.Checked = true;
            btnAutoCopy.CheckOnClick = true;
            btnAutoCopy.CheckState = CheckState.Checked;
            resources.ApplyResources(btnAutoCopy, "btnAutoCopy");
            btnAutoCopy.Name = "btnAutoCopy";
            btnAutoCopy.CheckedChanged += btnAutoCopy_CheckedChanged;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            // 
            // btnSave
            // 
            btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(btnSave, "btnSave");
            btnSave.Name = "btnSave";
            btnSave.Click += btnSave_Click;
            // 
            // lblIntro
            // 
            resources.ApplyResources(lblIntro, "lblIntro");
            lblIntro.Name = "lblIntro";
            // 
            // table
            // 
            resources.ApplyResources(table, "table");
            table.Controls.Add(list, 0, 2);
            table.Controls.Add(status, 0, 3);
            table.Controls.Add(toolStrip1, 0, 1);
            table.Controls.Add(lblIntro, 0, 0);
            table.Name = "table";
            // 
            // FormRoute
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(table);
            Name = "FormRoute";
            status.ResumeLayout(false);
            status.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            table.ResumeLayout(false);
            table.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ListView list;
        private ColumnHeader colSystem;
        private ColumnHeader colDistance;
        private StatusStrip status;
        private ToolStripStatusLabel lblStatus;
        private ColumnHeader colNotes;
        private ToolStrip toolStrip1;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem btnImportSpanshUrl;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem btnNamesFromClipboard;
        private ToolStripMenuItem btnNamesFromFile;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnAutoCopy;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton btnActive;
        private ToolStripButton btnSave;
        private Label lblIntro;
        private TableLayoutPanel table;
    }
}