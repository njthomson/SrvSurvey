namespace SrvSurvey.forms
{
    partial class FormJourneyList
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
            list = new ListView();
            colName = new ColumnHeader();
            colStart = new ColumnHeader();
            colEnd = new ColumnHeader();
            lblHeader = new Label();
            btnOpen = new Button();
            btnClose = new Button();
            txtSummary = new TextBox();
            SuspendLayout();
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.Columns.AddRange(new ColumnHeader[] { colName, colStart, colEnd });
            list.Location = new Point(12, 39);
            list.Name = "list";
            list.Size = new Size(505, 271);
            list.TabIndex = 0;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.SelectedIndexChanged += list_SelectedIndexChanged;
            list.DoubleClick += list_DoubleClick;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 200;
            // 
            // colStart
            // 
            colStart.Text = "Started";
            colStart.Width = 160;
            // 
            // colEnd
            // 
            colEnd.Text = "Ended";
            colEnd.Width = 160;
            // 
            // lblHeader
            // 
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(12, 9);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(147, 15);
            lblHeader.TabIndex = 1;
            lblHeader.Text = "Review your past journeys:";
            // 
            // btnOpen
            // 
            btnOpen.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOpen.Location = new Point(692, 326);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(75, 23);
            btnOpen.TabIndex = 2;
            btnOpen.Text = "&Open";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(773, 326);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 3;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // txtSummary
            // 
            txtSummary.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtSummary.Location = new Point(523, 39);
            txtSummary.Multiline = true;
            txtSummary.Name = "txtSummary";
            txtSummary.ReadOnly = true;
            txtSummary.ScrollBars = ScrollBars.Both;
            txtSummary.Size = new Size(325, 271);
            txtSummary.TabIndex = 4;
            // 
            // FormJourneyList
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(860, 361);
            Controls.Add(txtSummary);
            Controls.Add(btnClose);
            Controls.Add(btnOpen);
            Controls.Add(lblHeader);
            Controls.Add(list);
            MinimumSize = new Size(800, 400);
            Name = "FormJourneyList";
            Text = "Past Journeys";
            Load += FormJourneyList_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView list;
        private ColumnHeader colName;
        private ColumnHeader colStart;
        private ColumnHeader colEnd;
        private Label lblHeader;
        private Button btnOpen;
        private Button btnClose;
        private TextBox txtSummary;
    }
}