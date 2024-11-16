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
            btnPaste = new FlatButton();
            listRoute = new ListView();
            btnClose = new FlatButton();
            textBox1 = new TextBox();
            textBox2 = new TextBox2();
            SuspendLayout();
            // 
            // btnPaste
            // 
            btnPaste.AutoSize = true;
            btnPaste.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPaste.Font = new Font("Segoe UI Emoji", 5.25F, FontStyle.Regular, GraphicsUnit.Point);
            btnPaste.Image = Properties.ImageResources.copy1;
            btnPaste.Location = new Point(188, 25);
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new Size(24, 24);
            btnPaste.TabIndex = 0;
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.Click += btnPaste_Click;
            // 
            // listRoute
            // 
            listRoute.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listRoute.Location = new Point(12, 167);
            listRoute.Name = "listRoute";
            listRoute.Size = new Size(776, 242);
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
            // textBox1
            // 
            textBox1.Location = new Point(23, 29);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 3;
            textBox1.Text = "text1";
            // 
            // textBox2
            // 
            textBox2.BackColor = SystemColors.Window;
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox2.ForeColor = SystemColors.WindowText;
            textBox2.Location = new Point(23, 80);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(360, 44);
            textBox2.TabIndex = 4;
            textBox2.Text = "text2";
            textBox2.TextAlign = HorizontalAlignment.Center;
            textBox2.UseClearButton = true;
            // 
            // FormRoutePlotter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(btnClose);
            Controls.Add(listRoute);
            Controls.Add(btnPaste);
            Name = "FormRoutePlotter";
            Text = "FormRoutePlotter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlatButton btnPaste;
        private ListView listRoute;
        private FlatButton btnClose;
        private TextBox textBox1;
        private TextBox2 textBox2;
    }
}