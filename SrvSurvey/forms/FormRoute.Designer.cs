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
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colSystem, colDistance, colNotes });
            list.Location = new Point(0, 65);
            list.Margin = new Padding(0);
            list.Name = "list";
            list.Size = new Size(584, 274);
            list.TabIndex = 0;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ItemChecked += list_ItemChecked;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 200;
            // 
            // colDistance
            // 
            colDistance.Text = "Distance";
            colDistance.Width = 80;
            // 
            // colNotes
            // 
            colNotes.Text = "Notes";
            colNotes.Width = 160;
            // 
            // status
            // 
            status.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            status.Dock = DockStyle.None;
            status.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            status.Items.AddRange(new ToolStripItem[] { lblStatus });
            status.Location = new Point(0, 339);
            status.Name = "status";
            status.Size = new Size(584, 22);
            status.TabIndex = 3;
            status.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = false;
            lblStatus.BorderStyle = Border3DStyle.SunkenOuter;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(569, 17);
            lblStatus.Spring = true;
            // 
            // toolStrip1
            // 
            toolStrip1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, toolStripSeparator2, btnActive, btnAutoCopy, toolStripSeparator3, btnSave });
            toolStrip1.Location = new Point(1, 40);
            toolStrip1.Margin = new Padding(1, 0, 1, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(0);
            toolStrip1.RenderMode = ToolStripRenderMode.System;
            toolStrip1.Size = new Size(582, 25);
            toolStrip1.TabIndex = 8;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { btnImportSpanshUrl, toolStripSeparator1, btnNamesFromClipboard, btnNamesFromFile });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(83, 22);
            toolStripDropDownButton1.Text = "Import...";
            // 
            // btnImportSpanshUrl
            // 
            btnImportSpanshUrl.Name = "btnImportSpanshUrl";
            btnImportSpanshUrl.Size = new Size(291, 22);
            btnImportSpanshUrl.Text = "Spansh route URL from clipboard";
            btnImportSpanshUrl.Click += btnImportSpanshUrl_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(288, 6);
            // 
            // btnNamesFromClipboard
            // 
            btnNamesFromClipboard.Name = "btnNamesFromClipboard";
            btnNamesFromClipboard.Size = new Size(291, 22);
            btnNamesFromClipboard.Text = "System names from clipboard";
            btnNamesFromClipboard.Click += btnNamesFromClipboard_Click;
            // 
            // btnNamesFromFile
            // 
            btnNamesFromFile.Name = "btnNamesFromFile";
            btnNamesFromFile.Size = new Size(291, 22);
            btnNamesFromFile.Text = "System names from file";
            btnNamesFromFile.Click += btnNamesFromFile_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // btnActive
            // 
            btnActive.Checked = true;
            btnActive.CheckOnClick = true;
            btnActive.CheckState = CheckState.Checked;
            btnActive.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnActive.Image = (Image)resources.GetObject("btnActive.Image");
            btnActive.ImageTransparentColor = Color.Magenta;
            btnActive.Name = "btnActive";
            btnActive.Size = new Size(114, 22);
            btnActive.Text = "✔️ Route active";
            btnActive.CheckedChanged += btnActive_CheckedChanged;
            // 
            // btnAutoCopy
            // 
            btnAutoCopy.Checked = true;
            btnAutoCopy.CheckOnClick = true;
            btnAutoCopy.CheckState = CheckState.Checked;
            btnAutoCopy.ImageTransparentColor = Color.Magenta;
            btnAutoCopy.Name = "btnAutoCopy";
            btnAutoCopy.Size = new Size(170, 22);
            btnAutoCopy.Text = "✔️ Auto copy in Gal-Map";
            btnAutoCopy.CheckedChanged += btnAutoCopy_CheckedChanged;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // btnSave
            // 
            btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSave.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.Image = (Image)resources.GetObject("btnSave.Image");
            btnSave.ImageTransparentColor = Color.Magenta;
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(95, 22);
            btnSave.Text = "Save changes";
            btnSave.Click += btnSave_Click;
            // 
            // lblIntro
            // 
            lblIntro.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblIntro.AutoSize = true;
            lblIntro.Location = new Point(6, 6);
            lblIntro.Margin = new Padding(6);
            lblIntro.Name = "lblIntro";
            lblIntro.Size = new Size(572, 28);
            lblIntro.TabIndex = 9;
            lblIntro.Text = "Use routes to manually plot your way across the galaxy. Begin by importing a route, then look for guidance in the top-right corner in the Gal-Map.";
            // 
            // table
            // 
            table.ColumnCount = 1;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.Controls.Add(list, 0, 2);
            table.Controls.Add(status, 0, 3);
            table.Controls.Add(toolStrip1, 0, 1);
            table.Controls.Add(lblIntro, 0, 0);
            table.Dock = DockStyle.Fill;
            table.Location = new Point(0, 0);
            table.Name = "table";
            table.RowCount = 4;
            table.RowStyles.Add(new RowStyle());
            table.RowStyles.Add(new RowStyle());
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            table.RowStyles.Add(new RowStyle());
            table.Size = new Size(584, 361);
            table.TabIndex = 10;
            // 
            // FormRoute
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 361);
            Controls.Add(table);
            Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "FormRoute";
            Text = "Follow a route";
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