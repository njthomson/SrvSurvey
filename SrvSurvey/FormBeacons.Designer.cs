namespace SrvSurvey
{
    partial class FormBeacons
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBeacons));
            btnShare = new Button();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            comboCurrentSystem = new ComboBox();
            label1 = new Label();
            checkVisited = new CheckBox();
            txtFilter = new TextBox();
            btnFilter = new Button();
            colNotes = new ColumnHeader();
            colLastVisited = new ColumnHeader();
            colArrival = new ColumnHeader();
            colDistance = new ColumnHeader();
            colBody = new ColumnHeader();
            colSystem = new ColumnHeader();
            grid = new ListView();
            colSiteType = new ColumnHeader();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnShare
            // 
            btnShare.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnShare.Location = new Point(878, 7);
            btnShare.Name = "btnShare";
            btnShare.Size = new Size(196, 23);
            btnShare.TabIndex = 18;
            btnShare.Text = "Share your discovered data";
            btnShare.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip1.Location = new Point(0, 364);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1176, 22);
            statusStrip1.TabIndex = 17;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.DisplayStyle = ToolStripItemDisplayStyle.Text;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1161, 17);
            lblStatus.Spring = true;
            lblStatus.Text = "...";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // comboCurrentSystem
            // 
            comboCurrentSystem.FormattingEnabled = true;
            comboCurrentSystem.Location = new Point(691, 6);
            comboCurrentSystem.Name = "comboCurrentSystem";
            comboCurrentSystem.Size = new Size(181, 23);
            comboCurrentSystem.TabIndex = 16;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(549, 10);
            label1.Name = "label1";
            label1.Size = new Size(136, 15);
            label1.TabIndex = 15;
            label1.Text = "Measure distances from:";
            // 
            // checkVisited
            // 
            checkVisited.AutoSize = true;
            checkVisited.Location = new Point(328, 9);
            checkVisited.Name = "checkVisited";
            checkVisited.Size = new Size(88, 19);
            checkVisited.TabIndex = 13;
            checkVisited.Text = "Only visited";
            checkVisited.UseVisualStyleBackColor = true;
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(68, 7);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(254, 23);
            txtFilter.TabIndex = 12;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(12, 7);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(50, 23);
            btnFilter.TabIndex = 11;
            btnFilter.Text = "Filter";
            btnFilter.UseVisualStyleBackColor = true;
            btnFilter.Click += btnFilter_Click;
            // 
            // colNotes
            // 
            colNotes.Text = "Notes";
            colNotes.Width = 160;
            // 
            // colLastVisited
            // 
            colLastVisited.Text = "Last visited";
            colLastVisited.TextAlign = HorizontalAlignment.Center;
            colLastVisited.Width = 90;
            // 
            // colArrival
            // 
            colArrival.Text = "Arrival distance:";
            colArrival.TextAlign = HorizontalAlignment.Right;
            colArrival.Width = 120;
            // 
            // colDistance
            // 
            colDistance.Text = "System distance";
            colDistance.TextAlign = HorizontalAlignment.Right;
            colDistance.Width = 120;
            // 
            // colBody
            // 
            colBody.Text = "Body";
            colBody.Width = 110;
            // 
            // colSystem
            // 
            colSystem.Text = "System";
            colSystem.Width = 110;
            // 
            // grid
            // 
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.Columns.AddRange(new ColumnHeader[] { colSystem, colBody, colDistance, colArrival, colLastVisited, colSiteType, colNotes });
            grid.FullRowSelect = true;
            grid.GridLines = true;
            grid.Location = new Point(0, 35);
            grid.Name = "grid";
            grid.ShowGroups = false;
            grid.Size = new Size(1176, 326);
            grid.TabIndex = 10;
            grid.UseCompatibleStateImageBehavior = false;
            grid.View = View.Details;
            grid.ColumnClick += grid_ColumnClick;
            // 
            // colSiteType
            // 
            colSiteType.Text = "Site type";
            // 
            // FormBeacons
            // 
            AcceptButton = btnFilter;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1176, 386);
            Controls.Add(btnShare);
            Controls.Add(statusStrip1);
            Controls.Add(comboCurrentSystem);
            Controls.Add(label1);
            Controls.Add(checkVisited);
            Controls.Add(txtFilter);
            Controls.Add(btnFilter);
            Controls.Add(grid);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormBeacons";
            Text = "Guardian Beacons and Structures";
            Load += FormBeacons_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnShare;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private ComboBox comboCurrentSystem;
        private Label label1;
        private CheckBox checkVisited;
        private TextBox txtFilter;
        private Button btnFilter;
        private ColumnHeader colNotes;
        private ColumnHeader colLastVisited;
        private ColumnHeader colArrival;
        private ColumnHeader colDistance;
        private ColumnHeader colBody;
        private ColumnHeader colSystem;
        private ListView grid;
        private ColumnHeader colSiteType;
    }
}