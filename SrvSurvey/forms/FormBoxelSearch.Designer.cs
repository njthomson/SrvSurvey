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
            list = new CheckedListBox();
            txtSystemName = new TextBox();
            btnSearch = new Button();
            lblMaxNum = new Label();
            txtNext = new TextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list.FormattingEnabled = true;
            list.Location = new Point(12, 70);
            list.Name = "list";
            list.Size = new Size(600, 364);
            list.TabIndex = 0;
            // 
            // txtSystemName
            // 
            txtSystemName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemName.Location = new Point(93, 12);
            txtSystemName.Name = "txtSystemName";
            txtSystemName.Size = new Size(519, 23);
            txtSystemName.TabIndex = 1;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(12, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "&Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // lblMaxNum
            // 
            lblMaxNum.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblMaxNum.AutoSize = true;
            lblMaxNum.Location = new Point(521, 44);
            lblMaxNum.Name = "lblMaxNum";
            lblMaxNum.Size = new Size(52, 15);
            lblMaxNum.TabIndex = 3;
            lblMaxNum.Text = "Last: xxx";
            // 
            // txtNext
            // 
            txtNext.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNext.Location = new Point(93, 41);
            txtNext.Name = "txtNext";
            txtNext.Size = new Size(422, 23);
            txtNext.TabIndex = 5;
            // 
            // button1
            // 
            button1.Location = new Point(12, 41);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 6;
            button1.Text = "Copy &Next";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // FormBoxelSearch
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 445);
            Controls.Add(button1);
            Controls.Add(txtNext);
            Controls.Add(lblMaxNum);
            Controls.Add(btnSearch);
            Controls.Add(txtSystemName);
            Controls.Add(list);
            Name = "FormBoxelSearch";
            Text = "Boxel Search";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox list;
        private TextBox txtSystemName;
        private Button btnSearch;
        private Label lblMaxNum;
        private TextBox txtNext;
        private Button button1;
    }
}