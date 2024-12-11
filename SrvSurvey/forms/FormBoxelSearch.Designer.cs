namespace SrvSurvey.forms
{
    partial class FormBoxelSearch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBoxelSearch));
            txtSystemName = new TextBox();
            btnSearch = new Button();
            lblMaxNum = new Label();
            txtNext = new TextBox();
            btnCopyNext = new Button();
            list = new ListView();
            colSystem = new ColumnHeader();
            colDistance = new ColumnHeader();
            colNotes = new ColumnHeader();
            label1 = new Label();
            label2 = new Label();
            comboFrom = new ComboStarSystem();
            status = new StatusStrip();
            btnToggleList = new ToolStripSplitButton();
            lblStatus = new ToolStripStatusLabel();
            checkAutoCopy = new CheckBox();
            numMax = new NumericUpDown();
            panelList = new Panel();
            status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).BeginInit();
            panelList.SuspendLayout();
            SuspendLayout();
            // 
            // txtSystemName
            // 
            txtSystemName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemName.Location = new Point(93, 27);
            txtSystemName.Name = "txtSystemName";
            txtSystemName.Size = new Size(450, 23);
            txtSystemName.TabIndex = 2;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(12, 27);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "&Begin";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // lblMaxNum
            // 
            lblMaxNum.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblMaxNum.AutoSize = true;
            lblMaxNum.Location = new Point(549, 31);
            lblMaxNum.Name = "lblMaxNum";
            lblMaxNum.Size = new Size(65, 15);
            lblMaxNum.TabIndex = 3;
            lblMaxNum.Text = "Maximum:";
            // 
            // txtNext
            // 
            txtNext.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNext.Enabled = false;
            txtNext.Location = new Point(93, 56);
            txtNext.Name = "txtNext";
            txtNext.Size = new Size(420, 23);
            txtNext.TabIndex = 6;
            // 
            // btnCopyNext
            // 
            btnCopyNext.Enabled = false;
            btnCopyNext.Location = new Point(12, 56);
            btnCopyNext.Name = "btnCopyNext";
            btnCopyNext.Size = new Size(75, 23);
            btnCopyNext.TabIndex = 5;
            btnCopyNext.Text = "Copy &Next";
            btnCopyNext.UseVisualStyleBackColor = true;
            btnCopyNext.Click += btnCopyNext_Click;
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.CheckBoxes = true;
            list.Columns.AddRange(new ColumnHeader[] { colSystem, colDistance, colNotes });
            list.Enabled = false;
            list.FullRowSelect = true;
            list.Location = new Point(3, 0);
            list.Name = "list";
            list.Size = new Size(672, 303);
            list.TabIndex = 0;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.ItemChecked += list1_ItemChecked;
            // 
            // colSystem
            // 
            colSystem.Text = "System Name";
            colSystem.Width = 160;
            // 
            // colDistance
            // 
            colDistance.Text = "Distance";
            colDistance.TextAlign = HorizontalAlignment.Right;
            colDistance.Width = 120;
            // 
            // colNotes
            // 
            colNotes.Text = "Notes";
            colNotes.Width = 360;
            // 
            // label1
            // 
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(465, 15);
            label1.TabIndex = 0;
            label1.Text = "Search all systems in a given boxel. Begin by pasting any system name from that boxel.";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 312);
            label2.Name = "label2";
            label2.Size = new Size(136, 15);
            label2.TabIndex = 1;
            label2.Text = "Measure distances from:";
            // 
            // comboFrom
            // 
            comboFrom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            comboFrom.FormattingEnabled = true;
            comboFrom.Location = new Point(163, 309);
            comboFrom.Name = "comboFrom";
            comboFrom.Size = new Size(512, 23);
            comboFrom.TabIndex = 2;
            comboFrom.SelectedIndexChanged += comboFrom_SelectedIndexChanged;
            // 
            // status
            // 
            status.Items.AddRange(new ToolStripItem[] { btnToggleList, lblStatus });
            status.Location = new Point(0, 421);
            status.Name = "status";
            status.Size = new Size(678, 24);
            status.TabIndex = 9;
            status.Text = "statusStrip1";
            // 
            // btnToggleList
            // 
            btnToggleList.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToggleList.DropDownButtonWidth = 0;
            btnToggleList.Image = (Image)resources.GetObject("btnToggleList.Image");
            btnToggleList.ImageTransparentColor = Color.Magenta;
            btnToggleList.Name = "btnToggleList";
            btnToggleList.Size = new Size(70, 22);
            btnToggleList.Text = "⏶ Hide list";
            btnToggleList.ButtonClick += btnToggleList_ButtonClick;
            // 
            // lblStatus
            // 
            lblStatus.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            lblStatus.BorderStyle = Border3DStyle.SunkenOuter;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(593, 19);
            lblStatus.Spring = true;
            lblStatus.Text = "-";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // checkAutoCopy
            // 
            checkAutoCopy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkAutoCopy.AutoSize = true;
            checkAutoCopy.Enabled = false;
            checkAutoCopy.Location = new Point(519, 58);
            checkAutoCopy.Name = "checkAutoCopy";
            checkAutoCopy.Size = new Size(147, 19);
            checkAutoCopy.TabIndex = 7;
            checkAutoCopy.Text = "&Auto copy next system";
            checkAutoCopy.UseVisualStyleBackColor = true;
            // 
            // numMax
            // 
            numMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numMax.Location = new Point(620, 27);
            numMax.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numMax.Name = "numMax";
            numMax.Size = new Size(46, 23);
            numMax.TabIndex = 4;
            numMax.TextAlign = HorizontalAlignment.Right;
            // 
            // panelList
            // 
            panelList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelList.Controls.Add(comboFrom);
            panelList.Controls.Add(label2);
            panelList.Controls.Add(list);
            panelList.Enabled = false;
            panelList.Location = new Point(0, 85);
            panelList.Name = "panelList";
            panelList.Size = new Size(678, 335);
            panelList.TabIndex = 8;
            // 
            // FormBoxelSearch
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(678, 445);
            Controls.Add(panelList);
            Controls.Add(numMax);
            Controls.Add(checkAutoCopy);
            Controls.Add(status);
            Controls.Add(label1);
            Controls.Add(btnCopyNext);
            Controls.Add(txtNext);
            Controls.Add(lblMaxNum);
            Controls.Add(btnSearch);
            Controls.Add(txtSystemName);
            Name = "FormBoxelSearch";
            Text = "Boxel Search";
            status.ResumeLayout(false);
            status.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMax).EndInit();
            panelList.ResumeLayout(false);
            panelList.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtSystemName;
        private Button btnSearch;
        private Label lblMaxNum;
        private TextBox txtNext;
        private Button btnCopyNext;
        private ListView list;
        private ColumnHeader colSystem;
        private ColumnHeader colDistance;
        private Label label1;
        private Label label2;
        private ComboStarSystem comboFrom;
        private ColumnHeader colNotes;
        private StatusStrip status;
        private CheckBox checkAutoCopy;
        private NumericUpDown numMax;
        private Panel panelList;
        private ToolStripSplitButton btnToggleList;
        private ToolStripStatusLabel lblStatus;
    }
}