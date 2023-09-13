namespace BlinkMeasure
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            button1 = new Button();
            numHeight = new NumericUpDown();
            label2 = new Label();
            numWidth = new NumericUpDown();
            numTop = new NumericUpDown();
            label1 = new Label();
            numLeft = new NumericUpDown();
            groupBox2 = new GroupBox();
            txtDiff = new TextBox();
            label5 = new Label();
            frameTwo = new Panel();
            txtB = new TextBox();
            txtG = new TextBox();
            txtR = new TextBox();
            label3 = new Label();
            frameOne = new Panel();
            numDelta = new NumericUpDown();
            button2 = new Button();
            groupBox3 = new GroupBox();
            button3 = new Button();
            label4 = new Label();
            groupBox4 = new GroupBox();
            button5 = new Button();
            txtFolder = new TextBox();
            button4 = new Button();
            radFirst = new RadioButton();
            radLast = new RadioButton();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLeft).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDelta).BeginInit();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(numHeight);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(numWidth);
            groupBox1.Controls.Add(numTop);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(numLeft);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(200, 104);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Capture area:";
            // 
            // button1
            // 
            button1.Location = new Point(96, 75);
            button1.Name = "button1";
            button1.Size = new Size(94, 23);
            button1.TabIndex = 6;
            button1.Text = "Set";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // numHeight
            // 
            numHeight.Location = new Point(146, 46);
            numHeight.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numHeight.Name = "numHeight";
            numHeight.Size = new Size(44, 23);
            numHeight.TabIndex = 5;
            numHeight.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 48);
            label2.Name = "label2";
            label2.Size = new Size(84, 15);
            label2.TabIndex = 4;
            label2.Text = "Width, Height:";
            // 
            // numWidth
            // 
            numWidth.Location = new Point(96, 46);
            numWidth.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numWidth.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numWidth.Name = "numWidth";
            numWidth.Size = new Size(44, 23);
            numWidth.TabIndex = 3;
            numWidth.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // numTop
            // 
            numTop.Location = new Point(146, 17);
            numTop.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numTop.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numTop.Name = "numTop";
            numTop.Size = new Size(44, 23);
            numTop.TabIndex = 2;
            numTop.Value = new decimal(new int[] { 260, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(35, 19);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 1;
            label1.Text = "Top, Left:";
            // 
            // numLeft
            // 
            numLeft.Location = new Point(96, 17);
            numLeft.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numLeft.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numLeft.Name = "numLeft";
            numLeft.Size = new Size(44, 23);
            numLeft.TabIndex = 0;
            numLeft.Value = new decimal(new int[] { 260, 0, 0, 0 });
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(txtDiff);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(frameTwo);
            groupBox2.Controls.Add(txtB);
            groupBox2.Controls.Add(txtG);
            groupBox2.Controls.Add(txtR);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(frameOne);
            groupBox2.Location = new Point(12, 122);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(730, 235);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "Captured image:";
            // 
            // txtDiff
            // 
            txtDiff.BackColor = Color.LightYellow;
            txtDiff.Location = new Point(6, 144);
            txtDiff.Name = "txtDiff";
            txtDiff.ReadOnly = true;
            txtDiff.Size = new Size(71, 23);
            txtDiff.TabIndex = 6;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(9, 126);
            label5.Name = "label5";
            label5.Size = new Size(64, 15);
            label5.TabIndex = 5;
            label5.Text = "Difference:";
            // 
            // frameTwo
            // 
            frameTwo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            frameTwo.BackColor = Color.Black;
            frameTwo.BackgroundImageLayout = ImageLayout.Zoom;
            frameTwo.BorderStyle = BorderStyle.Fixed3D;
            frameTwo.Location = new Point(396, 16);
            frameTwo.Name = "frameTwo";
            frameTwo.Size = new Size(294, 213);
            frameTwo.TabIndex = 1;
            // 
            // txtB
            // 
            txtB.Location = new Point(6, 95);
            txtB.Name = "txtB";
            txtB.ReadOnly = true;
            txtB.Size = new Size(71, 23);
            txtB.TabIndex = 4;
            // 
            // txtG
            // 
            txtG.Location = new Point(6, 66);
            txtG.Name = "txtG";
            txtG.ReadOnly = true;
            txtG.Size = new Size(71, 23);
            txtG.TabIndex = 3;
            // 
            // txtR
            // 
            txtR.Location = new Point(6, 37);
            txtR.Name = "txtR";
            txtR.ReadOnly = true;
            txtR.Size = new Size(71, 23);
            txtR.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 19);
            label3.Name = "label3";
            label3.Size = new Size(68, 15);
            label3.TabIndex = 1;
            label3.Text = "Avg R, G, B:";
            // 
            // frameOne
            // 
            frameOne.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            frameOne.BackColor = Color.Black;
            frameOne.BackgroundImageLayout = ImageLayout.Zoom;
            frameOne.BorderStyle = BorderStyle.FixedSingle;
            frameOne.Location = new Point(96, 16);
            frameOne.Name = "frameOne";
            frameOne.Size = new Size(294, 213);
            frameOne.TabIndex = 0;
            // 
            // numDelta
            // 
            numDelta.Location = new Point(84, 17);
            numDelta.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numDelta.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numDelta.Name = "numDelta";
            numDelta.Size = new Size(44, 23);
            numDelta.TabIndex = 7;
            numDelta.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // button2
            // 
            button2.Location = new Point(6, 46);
            button2.Name = "button2";
            button2.Size = new Size(122, 23);
            button2.TabIndex = 8;
            button2.Text = "Single compare";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(button3);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(button2);
            groupBox3.Controls.Add(numDelta);
            groupBox3.Location = new Point(218, 16);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(139, 100);
            groupBox3.TabIndex = 9;
            groupBox3.TabStop = false;
            groupBox3.Text = "Difference:";
            // 
            // button3
            // 
            button3.Location = new Point(6, 71);
            button3.Name = "button3";
            button3.Size = new Size(122, 23);
            button3.TabIndex = 9;
            button3.Text = "Trigger compare";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(16, 19);
            label4.Name = "label4";
            label4.Size = new Size(62, 15);
            label4.TabIndex = 0;
            label4.Text = "Threshold:";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(radLast);
            groupBox4.Controls.Add(radFirst);
            groupBox4.Controls.Add(button5);
            groupBox4.Controls.Add(txtFolder);
            groupBox4.Controls.Add(button4);
            groupBox4.Location = new Point(363, 16);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(379, 100);
            groupBox4.TabIndex = 9;
            groupBox4.TabStop = false;
            groupBox4.Text = "Measure:";
            // 
            // button5
            // 
            button5.Enabled = false;
            button5.Location = new Point(87, 71);
            button5.Name = "button5";
            button5.Size = new Size(75, 23);
            button5.TabIndex = 2;
            button5.Text = "Stop";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // txtFolder
            // 
            txtFolder.Location = new Point(6, 22);
            txtFolder.Name = "txtFolder";
            txtFolder.Size = new Size(367, 23);
            txtFolder.TabIndex = 1;
            txtFolder.Tag = "folder";
            txtFolder.Text = "c:\\blink-measure";
            // 
            // button4
            // 
            button4.Location = new Point(6, 71);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 0;
            button4.Text = "Start";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // radFirst
            // 
            radFirst.AutoSize = true;
            radFirst.Location = new Point(6, 48);
            radFirst.Name = "radFirst";
            radFirst.Size = new Size(138, 19);
            radFirst.TabIndex = 11;
            radFirst.TabStop = true;
            radFirst.Text = "Delta from first frame";
            radFirst.UseVisualStyleBackColor = true;
            // 
            // radLast
            // 
            radLast.AutoSize = true;
            radLast.Location = new Point(150, 48);
            radLast.Name = "radLast";
            radLast.Size = new Size(136, 19);
            radLast.TabIndex = 12;
            radLast.TabStop = true;
            radLast.Text = "Delta from last frame";
            radLast.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(755, 377);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "Form1";
            Text = "Blink Measure";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLeft).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numDelta).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private NumericUpDown numHeight;
        private Label label2;
        private NumericUpDown numWidth;
        private NumericUpDown numTop;
        private Label label1;
        private NumericUpDown numLeft;
        private GroupBox groupBox2;
        private Button button1;
        private Panel frameOne;
        private TextBox txtB;
        private TextBox txtG;
        private TextBox txtR;
        private Label label3;
        private NumericUpDown numDelta;
        private Button button2;
        private Panel frameTwo;
        private GroupBox groupBox3;
        private Label label4;
        private TextBox txtDiff;
        private Label label5;
        private Button button3;
        private GroupBox groupBox4;
        private Button button4;
        private TextBox txtFolder;
        private Button button5;
        private RadioButton radLast;
        private RadioButton radFirst;
    }
}