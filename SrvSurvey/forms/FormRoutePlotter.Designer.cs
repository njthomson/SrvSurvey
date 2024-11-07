namespace SrvSurvey.forms
{
    partial class FormRoutePlotter
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
            btnPaste = new Button();
            listRoute = new ListView();
            btnClose = new Button();
            SuspendLayout();
            // 
            // btnPaste
            // 
            btnPaste.Location = new Point(0, 0);
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new Size(75, 23);
            btnPaste.TabIndex = 0;
            btnPaste.Text = "&Paste";
            btnPaste.UseVisualStyleBackColor = true;
            // 
            // listRoute
            // 
            listRoute.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listRoute.Location = new Point(12, 29);
            listRoute.Name = "listRoute";
            listRoute.Size = new Size(776, 380);
            listRoute.TabIndex = 1;
            listRoute.UseCompatibleStateImageBehavior = false;
            listRoute.View = View.Details;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(713, 415);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 2;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // FormRoutePlotter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnClose);
            Controls.Add(listRoute);
            Controls.Add(btnPaste);
            Name = "FormRoutePlotter";
            Text = "FormRoutePlotter";
            ResumeLayout(false);
        }

        #endregion

        private Button btnPaste;
        private ListView listRoute;
        private Button btnClose;
    }
}