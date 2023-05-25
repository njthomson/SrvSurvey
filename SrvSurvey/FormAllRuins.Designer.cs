namespace SrvSurvey
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
            this.grid = new System.Windows.Forms.ListView();
            this.colSiteId = new System.Windows.Forms.ColumnHeader();
            this.colSystem = new System.Windows.Forms.ColumnHeader();
            this.colBody = new System.Windows.Forms.ColumnHeader();
            this.colDistance = new System.Windows.Forms.ColumnHeader();
            this.colArrival = new System.Windows.Forms.ColumnHeader();
            this.colSiteType = new System.Windows.Forms.ColumnHeader();
            this.colIndex = new System.Windows.Forms.ColumnHeader();
            this.colLastVisited = new System.Windows.Forms.ColumnHeader();
            this.colHasImages = new System.Windows.Forms.ColumnHeader();
            this.colSiteHeading = new System.Windows.Forms.ColumnHeader();
            this.colRelicTowerHeading = new System.Windows.Forms.ColumnHeader();
            this.btnFilter = new System.Windows.Forms.Button();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.checkVisited = new System.Windows.Forms.CheckBox();
            this.comboSiteType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCurrentSystem = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grid.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSiteId,
            this.colSystem,
            this.colBody,
            this.colDistance,
            this.colArrival,
            this.colSiteType,
            this.colIndex,
            this.colLastVisited,
            this.colHasImages,
            this.colSiteHeading,
            this.colRelicTowerHeading});
            this.grid.FullRowSelect = true;
            this.grid.GridLines = true;
            this.grid.Location = new System.Drawing.Point(12, 40);
            this.grid.Name = "grid";
            this.grid.ShowGroups = false;
            this.grid.Size = new System.Drawing.Size(889, 356);
            this.grid.TabIndex = 0;
            this.grid.UseCompatibleStateImageBehavior = false;
            this.grid.View = System.Windows.Forms.View.Details;
            this.grid.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.grid_ColumnClick);
            this.grid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.grid_MouseClick);
            // 
            // colSiteId
            // 
            this.colSiteId.Text = "ID";
            this.colSiteId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // colSystem
            // 
            this.colSystem.Text = "System";
            this.colSystem.Width = 110;
            // 
            // colBody
            // 
            this.colBody.Text = "Body";
            this.colBody.Width = 110;
            // 
            // colDistance
            // 
            this.colDistance.Text = "System Distance";
            this.colDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // colArrival
            // 
            this.colArrival.Text = "Arrival distance:";
            this.colArrival.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // colSiteType
            // 
            this.colSiteType.Text = "Site type";
            this.colSiteType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colSiteType.Width = 90;
            // 
            // colIndex
            // 
            this.colIndex.Text = "Ruins #";
            this.colIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // colLastVisited
            // 
            this.colLastVisited.Text = "Last visited";
            this.colLastVisited.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colLastVisited.Width = 90;
            // 
            // colHasImages
            // 
            this.colHasImages.Text = "Has images";
            this.colHasImages.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colHasImages.Width = 80;
            // 
            // colSiteHeading
            // 
            this.colSiteHeading.Text = "Site heading";
            this.colSiteHeading.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colSiteHeading.Width = 90;
            // 
            // colRelicTowerHeading
            // 
            this.colRelicTowerHeading.Text = "Relic tower heading";
            this.colRelicTowerHeading.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colRelicTowerHeading.Width = 130;
            // 
            // btnFilter
            // 
            this.btnFilter.Location = new System.Drawing.Point(12, 11);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(50, 23);
            this.btnFilter.TabIndex = 1;
            this.btnFilter.Text = "Filter";
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // txtFilter
            // 
            this.txtFilter.Location = new System.Drawing.Point(68, 12);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(254, 23);
            this.txtFilter.TabIndex = 2;
            // 
            // checkVisited
            // 
            this.checkVisited.AutoSize = true;
            this.checkVisited.Location = new System.Drawing.Point(328, 14);
            this.checkVisited.Name = "checkVisited";
            this.checkVisited.Size = new System.Drawing.Size(88, 19);
            this.checkVisited.TabIndex = 3;
            this.checkVisited.Text = "Only visited";
            this.checkVisited.UseVisualStyleBackColor = true;
            this.checkVisited.CheckedChanged += new System.EventHandler(this.checkVisited_CheckedChanged);
            // 
            // comboSiteType
            // 
            this.comboSiteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSiteType.FormattingEnabled = true;
            this.comboSiteType.Items.AddRange(new object[] {
            "All types",
            "Alpha",
            "Beta",
            "Gamma"});
            this.comboSiteType.Location = new System.Drawing.Point(422, 12);
            this.comboSiteType.Name = "comboSiteType";
            this.comboSiteType.Size = new System.Drawing.Size(121, 23);
            this.comboSiteType.TabIndex = 4;
            this.comboSiteType.SelectedIndexChanged += new System.EventHandler(this.comboSiteType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 405);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "System distances are based on:";
            // 
            // lblCurrentSystem
            // 
            this.lblCurrentSystem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCurrentSystem.AutoSize = true;
            this.lblCurrentSystem.Location = new System.Drawing.Point(188, 405);
            this.lblCurrentSystem.Name = "lblCurrentSystem";
            this.lblCurrentSystem.Size = new System.Drawing.Size(101, 15);
            this.lblCurrentSystem.TabIndex = 6;
            this.lblCurrentSystem.Text = "<current system>";
            // 
            // FormAllRuins
            // 
            this.AcceptButton = this.btnFilter;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 429);
            this.Controls.Add(this.lblCurrentSystem);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboSiteType);
            this.Controls.Add(this.checkVisited);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.grid);
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "FormAllRuins";
            this.Text = "Guardian Ruins";
            this.Load += new System.EventHandler(this.FormAllRuins_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ListView grid;
        private ColumnHeader colSystem;
        private ColumnHeader colBody;
        private ColumnHeader colSiteType;
        private ColumnHeader colLastVisited;
        private ColumnHeader colSiteHeading;
        private ColumnHeader colRelicTowerHeading;
        private ColumnHeader colHasImages;
        private Button btnFilter;
        private ColumnHeader colDistance;
        private ColumnHeader colArrival;
        private TextBox txtFilter;
        private CheckBox checkVisited;
        private ComboBox comboSiteType;
        private ColumnHeader colIndex;
        private Label label1;
        private Label lblCurrentSystem;
        private ColumnHeader colSiteId;
    }
}