﻿namespace SrvSurvey
{
    partial class FormAllRuins
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAllRuins));
            grid = new ListView();
            colSiteId = new ColumnHeader();
            colSystem = new ColumnHeader();
            colBody = new ColumnHeader();
            colDistance = new ColumnHeader();
            colArrival = new ColumnHeader();
            colSiteType = new ColumnHeader();
            colIndex = new ColumnHeader();
            colLastVisited = new ColumnHeader();
            colHasImages = new ColumnHeader();
            colSurveyComplete = new ColumnHeader();
            colNotes = new ColumnHeader();
            btnFilter = new Button();
            txtFilter = new TextBox();
            comboSiteType = new ComboBox();
            label1 = new Label();
            comboCurrentSystem = new ComboBox();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            btnShare = new Button();
            checkRamTah = new CheckBox();
            comboVisited = new ComboBox();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // grid
            // 
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.Columns.AddRange(new ColumnHeader[] { colSiteId, colSystem, colBody, colDistance, colArrival, colSiteType, colIndex, colLastVisited, colHasImages, colSurveyComplete, colNotes });
            grid.FullRowSelect = true;
            grid.GridLines = true;
            grid.Location = new Point(0, 40);
            grid.Name = "grid";
            grid.ShowGroups = false;
            grid.Size = new Size(1339, 365);
            grid.TabIndex = 0;
            grid.UseCompatibleStateImageBehavior = false;
            grid.View = View.Details;
            grid.ColumnClick += grid_ColumnClick;
            grid.DoubleClick += grid_DoubleClick;
            grid.MouseClick += grid_MouseClick;
            // 
            // colSiteId
            // 
            colSiteId.Text = "ID";
            colSiteId.TextAlign = HorizontalAlignment.Right;
            colSiteId.Width = 40;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 165;
            // 
            // colBody
            // 
            colBody.Text = "Body";
            colBody.Width = 55;
            // 
            // colDistance
            // 
            colDistance.Text = "System distance";
            colDistance.TextAlign = HorizontalAlignment.Right;
            colDistance.Width = 100;
            // 
            // colArrival
            // 
            colArrival.Text = "Arrival distance:";
            colArrival.TextAlign = HorizontalAlignment.Right;
            colArrival.Width = 100;
            // 
            // colSiteType
            // 
            colSiteType.Text = "Site type";
            colSiteType.TextAlign = HorizontalAlignment.Center;
            colSiteType.Width = 65;
            // 
            // colIndex
            // 
            colIndex.Text = "Ruins #";
            colIndex.TextAlign = HorizontalAlignment.Center;
            colIndex.Width = 55;
            // 
            // colLastVisited
            // 
            colLastVisited.Text = "Last visited";
            colLastVisited.TextAlign = HorizontalAlignment.Center;
            colLastVisited.Width = 72;
            // 
            // colHasImages
            // 
            colHasImages.Text = "Has images";
            colHasImages.TextAlign = HorizontalAlignment.Center;
            colHasImages.Width = 75;
            // 
            // colSurveyComplete
            // 
            colSurveyComplete.Text = "Survey complete";
            colSurveyComplete.TextAlign = HorizontalAlignment.Center;
            colSurveyComplete.Width = 100;
            // 
            // colNotes
            // 
            colNotes.Text = "Notes";
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(12, 13);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(50, 23);
            btnFilter.TabIndex = 1;
            btnFilter.Text = "Filter";
            btnFilter.UseVisualStyleBackColor = true;
            btnFilter.Click += btnFilter_Click;
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(68, 13);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(166, 23);
            txtFilter.TabIndex = 2;
            // 
            // comboSiteType
            // 
            comboSiteType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSiteType.FormattingEnabled = true;
            comboSiteType.Items.AddRange(new object[] { "All types", "Alpha", "Beta", "Gamma" });
            comboSiteType.Location = new Point(319, 13);
            comboSiteType.Name = "comboSiteType";
            comboSiteType.Size = new Size(121, 23);
            comboSiteType.TabIndex = 4;
            comboSiteType.SelectedIndexChanged += comboSiteType_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(446, 16);
            label1.Name = "label1";
            label1.Size = new Size(136, 15);
            label1.TabIndex = 5;
            label1.Text = "Measure distances from:";
            // 
            // comboCurrentSystem
            // 
            comboCurrentSystem.FormattingEnabled = true;
            comboCurrentSystem.Location = new Point(588, 13);
            comboCurrentSystem.Name = "comboCurrentSystem";
            comboCurrentSystem.Size = new Size(181, 23);
            comboCurrentSystem.TabIndex = 7;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip1.Location = new Point(0, 407);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1339, 22);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.DisplayStyle = ToolStripItemDisplayStyle.Text;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1324, 17);
            lblStatus.Spring = true;
            lblStatus.Text = "...";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnShare
            // 
            btnShare.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnShare.Location = new Point(1174, 11);
            btnShare.Name = "btnShare";
            btnShare.Size = new Size(153, 23);
            btnShare.TabIndex = 9;
            btnShare.Text = "Share your discovered data";
            btnShare.UseVisualStyleBackColor = true;
            btnShare.Click += btnShare_Click;
            // 
            // checkRamTah
            // 
            checkRamTah.AutoSize = true;
            checkRamTah.Location = new Point(775, 15);
            checkRamTah.Name = "checkRamTah";
            checkRamTah.Size = new Size(131, 19);
            checkRamTah.TabIndex = 10;
            checkRamTah.Text = "Show Ram Tah Logs";
            checkRamTah.UseVisualStyleBackColor = true;
            checkRamTah.CheckedChanged += checkRamTah_CheckedChanged;
            // 
            // comboVisited
            // 
            comboVisited.DropDownStyle = ComboBoxStyle.DropDownList;
            comboVisited.FormattingEnabled = true;
            comboVisited.Items.AddRange(new object[] { "All", "Visited", "Unvisited" });
            comboVisited.Location = new Point(240, 13);
            comboVisited.Name = "comboVisited";
            comboVisited.Size = new Size(73, 23);
            comboVisited.TabIndex = 11;
            comboVisited.SelectedIndexChanged += comboSiteType_SelectedIndexChanged;
            // 
            // FormAllRuins
            // 
            AcceptButton = btnFilter;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1339, 429);
            Controls.Add(comboVisited);
            Controls.Add(checkRamTah);
            Controls.Add(btnShare);
            Controls.Add(statusStrip1);
            Controls.Add(comboCurrentSystem);
            Controls.Add(label1);
            Controls.Add(comboSiteType);
            Controls.Add(txtFilter);
            Controls.Add(btnFilter);
            Controls.Add(grid);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 400);
            Name = "FormAllRuins";
            Text = "Guardian Ruins";
            Load += FormAllRuins_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView grid;
        private ColumnHeader colSystem;
        private ColumnHeader colBody;
        private ColumnHeader colSiteType;
        private ColumnHeader colLastVisited;
        private ColumnHeader colSurveyComplete;
        private ColumnHeader colHasImages;
        private Button btnFilter;
        private ColumnHeader colDistance;
        private ColumnHeader colArrival;
        private TextBox txtFilter;
        private ComboBox comboSiteType;
        private ColumnHeader colIndex;
        private Label label1;
        private ColumnHeader colSiteId;
        private ColumnHeader colNotes;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private Button btnShare;
        public ComboBox comboCurrentSystem;
        private CheckBox checkRamTah;
        private ComboBox comboVisited;
    }
}