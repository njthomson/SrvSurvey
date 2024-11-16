namespace SrvSurvey.forms
{
    partial class FormNearestSystems
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
            components = new System.ComponentModel.Container();
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "Pyria Eurl YG-J c23-0", "975.97 ly", "4 other bio signals in this system" }, -1, Color.Empty, SystemColors.ControlLight, null);
            ListViewItem listViewItem2 = new ListViewItem("bbb");
            ListViewItem listViewItem3 = new ListViewItem(new string[] { "ccc" }, -1, Color.Empty, SystemColors.ButtonFace, null);
            ListViewItem listViewItem4 = new ListViewItem("ddd");
            ListViewItem listViewItem5 = new ListViewItem("eee");
            list = new ListView();
            colSystem = new ColumnHeader();
            colDistance = new ColumnHeader();
            colNotes = new ColumnHeader();
            contextMenu = new ContextMenuStrip(components);
            viewOnCanonn = new ToolStripMenuItem();
            viewOnSpansh = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuCopyName = new ToolStripMenuItem();
            copyStarPos = new ToolStripMenuItem();
            btnClose = new Button();
            label1 = new Label();
            txtSystem = new TextBox();
            label2 = new Label();
            txtContaining = new TextBox();
            label3 = new Label();
            contextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.Columns.AddRange(new ColumnHeader[] { colSystem, colDistance, colNotes });
            list.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4, listViewItem5 });
            list.Location = new Point(12, 70);
            list.Name = "list";
            list.Size = new Size(776, 160);
            list.TabIndex = 0;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.DoubleClick += menuCopyName_Click;
            list.MouseDown += list_MouseDown;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 180;
            // 
            // colDistance
            // 
            colDistance.Text = "Distance";
            colDistance.TextAlign = HorizontalAlignment.Center;
            colDistance.Width = 80;
            // 
            // colNotes
            // 
            colNotes.Text = "Notes";
            colNotes.Width = 460;
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] { viewOnCanonn, viewOnSpansh, toolStripSeparator1, menuCopyName, copyStarPos });
            contextMenu.Name = "contextMenuStrip1";
            contextMenu.Size = new Size(179, 98);
            // 
            // viewOnCanonn
            // 
            viewOnCanonn.Image = Properties.ImageResources.canonn_16x16;
            viewOnCanonn.Name = "viewOnCanonn";
            viewOnCanonn.Size = new Size(178, 22);
            viewOnCanonn.Text = "View on Canonn";
            viewOnCanonn.Click += viewOnCanonn_Click;
            // 
            // viewOnSpansh
            // 
            viewOnSpansh.Image = Properties.ImageResources.spansh_16x16;
            viewOnSpansh.Name = "viewOnSpansh";
            viewOnSpansh.Size = new Size(178, 22);
            viewOnSpansh.Text = "View on Spansh";
            viewOnSpansh.Click += viewOnSpansh_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(175, 6);
            // 
            // menuCopyName
            // 
            menuCopyName.Image = Properties.ImageResources.copy1;
            menuCopyName.Name = "menuCopyName";
            menuCopyName.Size = new Size(178, 22);
            menuCopyName.Text = "Copy system name";
            menuCopyName.Click += menuCopyName_Click;
            // 
            // copyStarPos
            // 
            copyStarPos.Image = Properties.ImageResources.copy1;
            copyStarPos.Name = "copyStarPos";
            copyStarPos.Size = new Size(178, 22);
            copyStarPos.Text = "Copy star pos x, y, z";
            copyStarPos.Click += copyStarPos_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(713, 236);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 1;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(109, 15);
            label1.TabIndex = 2;
            label1.Text = "Nearest systems to:";
            // 
            // txtSystem
            // 
            txtSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystem.Location = new Point(127, 12);
            txtSystem.Name = "txtSystem";
            txtSystem.ReadOnly = true;
            txtSystem.Size = new Size(661, 23);
            txtSystem.TabIndex = 3;
            txtSystem.Text = "Pyra Dryoae ET-O d7-7";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(52, 44);
            label2.Name = "label2";
            label2.Size = new Size(69, 15);
            label2.TabIndex = 4;
            label2.Text = "Containing:";
            // 
            // txtContaining
            // 
            txtContaining.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtContaining.Location = new Point(127, 41);
            txtContaining.Name = "txtContaining";
            txtContaining.ReadOnly = true;
            txtContaining.Size = new Size(661, 23);
            txtContaining.TabIndex = 5;
            txtContaining.Text = "Recepta Umbrux - Orange";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(12, 236);
            label3.Name = "label3";
            label3.Size = new Size(343, 15);
            label3.TabIndex = 6;
            label3.Text = "Double-click to copy system name, right click for more options:";
            // 
            // FormNearestSystems
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 265);
            Controls.Add(label3);
            Controls.Add(txtContaining);
            Controls.Add(label2);
            Controls.Add(txtSystem);
            Controls.Add(label1);
            Controls.Add(btnClose);
            Controls.Add(list);
            Name = "FormNearestSystems";
            Text = "Nearest systems";
            contextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView list;
        private Button btnClose;
        private Label label1;
        private TextBox txtSystem;
        private ColumnHeader colSystem;
        private ColumnHeader colDistance;
        private ColumnHeader colNotes;
        private Label label2;
        private TextBox txtContaining;
        private Label label3;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuCopyName;
        private ToolStripMenuItem copyStarPos;
        private ToolStripMenuItem viewOnSpansh;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem viewOnCanonn;
    }
}