namespace SrvSurvey.forms
{
    partial class FormPlayComms
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
            btnQuests = new Button();
            btnMsgs = new Button();
            button3 = new Button();
            list = new ListView();
            ID = new ColumnHeader();
            txtStuff = new TextBox();
            SuspendLayout();
            // 
            // btnQuests
            // 
            btnQuests.Location = new Point(8, 12);
            btnQuests.Name = "btnQuests";
            btnQuests.Size = new Size(75, 23);
            btnQuests.TabIndex = 0;
            btnQuests.Text = "Quests";
            btnQuests.UseVisualStyleBackColor = true;
            btnQuests.Click += btnQuests_Click;
            // 
            // btnMsgs
            // 
            btnMsgs.Location = new Point(8, 41);
            btnMsgs.Name = "btnMsgs";
            btnMsgs.Size = new Size(75, 23);
            btnMsgs.TabIndex = 1;
            btnMsgs.Text = "Msgs";
            btnMsgs.UseVisualStyleBackColor = true;
            btnMsgs.Click += btnMsgs_Click;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button3.Location = new Point(8, 276);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 2;
            button3.Text = "Watch ...";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // list
            // 
            list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            list.Columns.AddRange(new ColumnHeader[] { ID });
            list.Location = new Point(89, 12);
            list.Name = "list";
            list.Size = new Size(187, 287);
            list.TabIndex = 3;
            list.UseCompatibleStateImageBehavior = false;
            list.View = View.Details;
            list.SelectedIndexChanged += list_SelectedIndexChanged;
            // 
            // ID
            // 
            ID.Text = "ID";
            ID.Width = 160;
            // 
            // txtStuff
            // 
            txtStuff.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtStuff.Location = new Point(282, 13);
            txtStuff.Multiline = true;
            txtStuff.Name = "txtStuff";
            txtStuff.ReadOnly = true;
            txtStuff.ScrollBars = ScrollBars.Both;
            txtStuff.Size = new Size(444, 286);
            txtStuff.TabIndex = 4;
            // 
            // FormPlayComms
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(738, 311);
            Controls.Add(txtStuff);
            Controls.Add(list);
            Controls.Add(button3);
            Controls.Add(btnMsgs);
            Controls.Add(btnQuests);
            Name = "FormPlayComms";
            Text = "FormPlayComms";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnQuests;
        private Button btnMsgs;
        private Button button3;
        private ListView list;
        private TextBox txtStuff;
        private ColumnHeader ID;
    }
}