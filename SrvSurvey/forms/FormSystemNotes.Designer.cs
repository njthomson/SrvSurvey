namespace SrvSurvey.forms
{
    partial class FormSystemNotes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSystemNotes));
            label1 = new Label();
            txtSystem = new TextBox();
            btnCancel = new FlatButton();
            txtNotes = new TextBox();
            btnSave = new FlatButton();
            statusStrip1 = new StatusStrip();
            menuOptions = new ToolStripDropDownButton();
            menuAlwaysOnTop = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            viewSystemOnToolStripMenuItem = new ToolStripMenuItem();
            viewOnCanonnSignalsToolStripMenuItem = new ToolStripMenuItem();
            viewOnSpanshToolStripMenuItem = new ToolStripMenuItem();
            viewOnEDSMToolStripMenuItem = new ToolStripMenuItem();
            linkOpenImagesFolder = new LinkLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 3;
            label1.Text = "System:";
            // 
            // txtSystem
            // 
            txtSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystem.Location = new Point(66, 6);
            txtSystem.Name = "txtSystem";
            txtSystem.ReadOnly = true;
            txtSystem.Size = new Size(357, 23);
            txtSystem.TabIndex = 4;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(348, 400);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtNotes
            // 
            txtNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNotes.Location = new Point(12, 35);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(411, 359);
            txtNotes.TabIndex = 0;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(267, 400);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 1;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { menuOptions });
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(435, 22);
            statusStrip1.TabIndex = 5;
            statusStrip1.Text = "statusStrip1";
            // 
            // menuOptions
            // 
            menuOptions.DisplayStyle = ToolStripItemDisplayStyle.Text;
            menuOptions.DropDownItems.AddRange(new ToolStripItem[] { menuAlwaysOnTop, toolStripSeparator1, viewSystemOnToolStripMenuItem, viewOnCanonnSignalsToolStripMenuItem, viewOnSpanshToolStripMenuItem, viewOnEDSMToolStripMenuItem });
            menuOptions.Image = (Image)resources.GetObject("menuOptions.Image");
            menuOptions.ImageTransparentColor = Color.Magenta;
            menuOptions.Name = "menuOptions";
            menuOptions.Size = new Size(57, 20);
            menuOptions.Text = "More...";
            // 
            // menuAlwaysOnTop
            // 
            menuAlwaysOnTop.Checked = true;
            menuAlwaysOnTop.CheckOnClick = true;
            menuAlwaysOnTop.CheckState = CheckState.Checked;
            menuAlwaysOnTop.Name = "menuAlwaysOnTop";
            menuAlwaysOnTop.Size = new Size(175, 22);
            menuAlwaysOnTop.Text = "Always on top";
            menuAlwaysOnTop.Click += menuAlwaysOnTop_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(172, 6);
            // 
            // viewSystemOnToolStripMenuItem
            // 
            viewSystemOnToolStripMenuItem.Enabled = false;
            viewSystemOnToolStripMenuItem.Name = "viewSystemOnToolStripMenuItem";
            viewSystemOnToolStripMenuItem.Size = new Size(175, 22);
            viewSystemOnToolStripMenuItem.Text = "Open system ...";
            // 
            // viewOnCanonnSignalsToolStripMenuItem
            // 
            viewOnCanonnSignalsToolStripMenuItem.Image = Properties.ImageResources.canonn_16x16;
            viewOnCanonnSignalsToolStripMenuItem.Name = "viewOnCanonnSignalsToolStripMenuItem";
            viewOnCanonnSignalsToolStripMenuItem.Size = new Size(175, 22);
            viewOnCanonnSignalsToolStripMenuItem.Text = "On Canonn Signals";
            viewOnCanonnSignalsToolStripMenuItem.Click += viewOnCanonnSignalsToolStripMenuItem_Click;
            // 
            // viewOnSpanshToolStripMenuItem
            // 
            viewOnSpanshToolStripMenuItem.Image = Properties.ImageResources.spansh_16x16;
            viewOnSpanshToolStripMenuItem.Name = "viewOnSpanshToolStripMenuItem";
            viewOnSpanshToolStripMenuItem.Size = new Size(175, 22);
            viewOnSpanshToolStripMenuItem.Text = "On Spansh";
            viewOnSpanshToolStripMenuItem.Click += viewOnSpanshToolStripMenuItem_Click;
            // 
            // viewOnEDSMToolStripMenuItem
            // 
            viewOnEDSMToolStripMenuItem.Name = "viewOnEDSMToolStripMenuItem";
            viewOnEDSMToolStripMenuItem.Size = new Size(175, 22);
            viewOnEDSMToolStripMenuItem.Text = "On EDSM";
            viewOnEDSMToolStripMenuItem.Click += viewOnEDSMToolStripMenuItem_Click;
            // 
            // linkOpenImagesFolder
            // 
            linkOpenImagesFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkOpenImagesFolder.AutoSize = true;
            linkOpenImagesFolder.Location = new Point(12, 404);
            linkOpenImagesFolder.Name = "linkOpenImagesFolder";
            linkOpenImagesFolder.Size = new Size(153, 15);
            linkOpenImagesFolder.TabIndex = 6;
            linkOpenImagesFolder.TabStop = true;
            linkOpenImagesFolder.Text = "View images for this system";
            linkOpenImagesFolder.Visible = false;
            linkOpenImagesFolder.LinkClicked += linkOpenImagesFolder_LinkClicked;
            // 
            // FormSystemNotes
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(435, 450);
            Controls.Add(linkOpenImagesFolder);
            Controls.Add(statusStrip1);
            Controls.Add(btnSave);
            Controls.Add(txtNotes);
            Controls.Add(btnCancel);
            Controls.Add(txtSystem);
            Controls.Add(label1);
            Name = "FormSystemNotes";
            Text = "System notes";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtSystem;
        private FlatButton btnCancel;
        private TextBox txtNotes;
        private FlatButton btnSave;
        private StatusStrip statusStrip1;
        private ToolStripDropDownButton menuOptions;
        private ToolStripMenuItem menuAlwaysOnTop;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem viewOnCanonnSignalsToolStripMenuItem;
        private ToolStripMenuItem viewOnEDSMToolStripMenuItem;
        private ToolStripMenuItem viewOnSpanshToolStripMenuItem;
        private ToolStripMenuItem viewSystemOnToolStripMenuItem;
        private LinkLabel linkOpenImagesFolder;
    }
}