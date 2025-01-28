namespace SrvSurvey.forms
{
    partial class FormJourneyViewer
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
            btnClose = new FlatButton();
            groupQuickStats = new GroupBox();
            listQuickStats = new ListView();
            colMetricName = new ColumnHeader();
            colMetricCount = new ColumnHeader();
            groupQuickStats.SuspendLayout();
            SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(680, 321);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // groupQuickStats
            // 
            groupQuickStats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            groupQuickStats.Controls.Add(listQuickStats);
            groupQuickStats.Location = new Point(12, 12);
            groupQuickStats.Name = "groupQuickStats";
            groupQuickStats.Size = new Size(331, 321);
            groupQuickStats.TabIndex = 1;
            groupQuickStats.TabStop = false;
            groupQuickStats.Text = "Quick stats:";
            // 
            // listQuickStats
            // 
            listQuickStats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listQuickStats.BackColor = SystemColors.Menu;
            listQuickStats.Columns.AddRange(new ColumnHeader[] { colMetricName, colMetricCount });
            listQuickStats.FullRowSelect = true;
            listQuickStats.GridLines = true;
            listQuickStats.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listQuickStats.Location = new Point(6, 22);
            listQuickStats.MultiSelect = false;
            listQuickStats.Name = "listQuickStats";
            listQuickStats.ShowGroups = false;
            listQuickStats.ShowItemToolTips = true;
            listQuickStats.Size = new Size(319, 293);
            listQuickStats.TabIndex = 0;
            listQuickStats.UseCompatibleStateImageBehavior = false;
            listQuickStats.View = View.Details;
            // 
            // colMetricName
            // 
            colMetricName.Text = "Metric";
            colMetricName.Width = 160;
            // 
            // colMetricCount
            // 
            colMetricCount.Text = "Count";
            colMetricCount.TextAlign = HorizontalAlignment.Right;
            // 
            // FormJourneyViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(767, 356);
            Controls.Add(groupQuickStats);
            Controls.Add(btnClose);
            Name = "FormJourneyViewer";
            Text = "Journey: ";
            groupQuickStats.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlatButton btnClose;
        private GroupBox groupQuickStats;
        private ListView listQuickStats;
        private ColumnHeader colMetricName;
        private ColumnHeader colMetricCount;
    }
}