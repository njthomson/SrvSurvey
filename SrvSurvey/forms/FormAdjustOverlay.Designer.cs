namespace SrvSurvey.forms
{
    partial class FormAdjustOverlay
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
            lblChoose = new Label();
            comboPlotter = new ComboBox();
            btnCancel = new Button();
            btnAccept = new Button();
            checkLeft = new CheckBox();
            checkCenter = new CheckBox();
            checkRight = new CheckBox();
            checkTop = new CheckBox();
            checkMiddle = new CheckBox();
            checkBottom = new CheckBox();
            labelOffset = new Label();
            numY = new NumericUpDown();
            numX = new NumericUpDown();
            groupBox1 = new GroupBox();
            flowLayoutPanel5 = new FlowLayoutPanel();
            checkOpacity = new CheckBox();
            numOpacity = new NumericUpDown();
            btnReset = new Button();
            flowLayoutPanel4 = new FlowLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            checkHScreen = new CheckBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            checkVScreen = new CheckBox();
            flowLayoutPanel3 = new FlowLayoutPanel();
            lblAdvise = new Label();
            checkShowAll = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX).BeginInit();
            groupBox1.SuspendLayout();
            flowLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOpacity).BeginInit();
            flowLayoutPanel4.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // lblChoose
            // 
            lblChoose.AutoSize = true;
            lblChoose.FlatStyle = FlatStyle.System;
            lblChoose.Location = new Point(12, 9);
            lblChoose.Name = "lblChoose";
            lblChoose.Size = new Size(107, 15);
            lblChoose.TabIndex = 0;
            lblChoose.Text = "Choose &an overlay:";
            // 
            // comboPlotter
            // 
            comboPlotter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboPlotter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPlotter.FlatStyle = FlatStyle.System;
            comboPlotter.FormattingEnabled = true;
            comboPlotter.Items.AddRange(new object[] { "Choose an overlay..." });
            comboPlotter.Location = new Point(143, 6);
            comboPlotter.Name = "comboPlotter";
            comboPlotter.Size = new Size(269, 23);
            comboPlotter.TabIndex = 1;
            comboPlotter.DropDown += comboPlotter_DropDown;
            comboPlotter.SelectedIndexChanged += comboPlotter_SelectedIndexChanged;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(84, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnAccept
            // 
            btnAccept.DialogResult = DialogResult.OK;
            btnAccept.FlatStyle = FlatStyle.Flat;
            btnAccept.Location = new Point(3, 3);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(75, 23);
            btnAccept.TabIndex = 0;
            btnAccept.Text = "&Save";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // checkLeft
            // 
            checkLeft.AutoSize = true;
            checkLeft.FlatStyle = FlatStyle.System;
            checkLeft.Location = new Point(3, 3);
            checkLeft.Name = "checkLeft";
            checkLeft.Size = new Size(52, 20);
            checkLeft.TabIndex = 0;
            checkLeft.Tag = "Left";
            checkLeft.Text = "Left";
            checkLeft.UseVisualStyleBackColor = true;
            checkLeft.CheckedChanged += checkHorizontal_CheckedChanged;
            // 
            // checkCenter
            // 
            checkCenter.AutoSize = true;
            checkCenter.FlatStyle = FlatStyle.System;
            checkCenter.Location = new Point(61, 3);
            checkCenter.Name = "checkCenter";
            checkCenter.Size = new Size(67, 20);
            checkCenter.TabIndex = 1;
            checkCenter.Tag = "Center";
            checkCenter.Text = "Center";
            checkCenter.UseVisualStyleBackColor = true;
            checkCenter.CheckedChanged += checkHorizontal_CheckedChanged;
            // 
            // checkRight
            // 
            checkRight.AutoSize = true;
            checkRight.FlatStyle = FlatStyle.System;
            checkRight.Location = new Point(134, 3);
            checkRight.Name = "checkRight";
            checkRight.Size = new Size(60, 20);
            checkRight.TabIndex = 2;
            checkRight.Tag = "Right";
            checkRight.Text = "Right";
            checkRight.UseVisualStyleBackColor = true;
            checkRight.CheckedChanged += checkHorizontal_CheckedChanged;
            // 
            // checkTop
            // 
            checkTop.AutoSize = true;
            checkTop.FlatStyle = FlatStyle.System;
            checkTop.Location = new Point(3, 3);
            checkTop.Name = "checkTop";
            checkTop.Size = new Size(51, 20);
            checkTop.TabIndex = 0;
            checkTop.Tag = "Top";
            checkTop.Text = "Top";
            checkTop.UseVisualStyleBackColor = true;
            checkTop.CheckedChanged += checkVertical_CheckedChanged;
            // 
            // checkMiddle
            // 
            checkMiddle.AutoSize = true;
            checkMiddle.FlatStyle = FlatStyle.System;
            checkMiddle.Location = new Point(3, 29);
            checkMiddle.Name = "checkMiddle";
            checkMiddle.Size = new Size(69, 20);
            checkMiddle.TabIndex = 1;
            checkMiddle.Tag = "Middle";
            checkMiddle.Text = "Middle";
            checkMiddle.UseVisualStyleBackColor = true;
            checkMiddle.CheckedChanged += checkVertical_CheckedChanged;
            // 
            // checkBottom
            // 
            checkBottom.AutoSize = true;
            checkBottom.FlatStyle = FlatStyle.System;
            checkBottom.Location = new Point(3, 55);
            checkBottom.Name = "checkBottom";
            checkBottom.Size = new Size(72, 20);
            checkBottom.TabIndex = 2;
            checkBottom.Tag = "Bottom";
            checkBottom.Text = "Bottom";
            checkBottom.UseVisualStyleBackColor = true;
            checkBottom.CheckedChanged += checkVertical_CheckedChanged;
            // 
            // labelOffset
            // 
            labelOffset.Anchor = AnchorStyles.Right;
            labelOffset.AutoSize = true;
            labelOffset.FlatStyle = FlatStyle.System;
            labelOffset.Location = new Point(3, 7);
            labelOffset.Name = "labelOffset";
            labelOffset.Size = new Size(65, 15);
            labelOffset.TabIndex = 0;
            labelOffset.Text = "&Offset X, Y:";
            // 
            // numY
            // 
            numY.Location = new Point(130, 3);
            numY.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numY.Minimum = new decimal(new int[] { 9999, 0, 0, int.MinValue });
            numY.Name = "numY";
            numY.Size = new Size(50, 23);
            numY.TabIndex = 2;
            numY.TextAlign = HorizontalAlignment.Right;
            numY.Value = new decimal(new int[] { 9999, 0, 0, 0 });
            numY.ValueChanged += num_ValueChanged;
            // 
            // numX
            // 
            numX.Location = new Point(74, 3);
            numX.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numX.Minimum = new decimal(new int[] { 9999, 0, 0, int.MinValue });
            numX.Name = "numX";
            numX.Size = new Size(50, 23);
            numX.TabIndex = 1;
            numX.TextAlign = HorizontalAlignment.Right;
            numX.Value = new decimal(new int[] { 9999, 0, 0, 0 });
            numX.ValueChanged += num_ValueChanged;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(flowLayoutPanel5);
            groupBox1.Controls.Add(btnReset);
            groupBox1.Controls.Add(flowLayoutPanel4);
            groupBox1.Controls.Add(flowLayoutPanel1);
            groupBox1.Controls.Add(flowLayoutPanel2);
            groupBox1.Location = new Point(15, 58);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(400, 163);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            // 
            // flowLayoutPanel5
            // 
            flowLayoutPanel5.AutoSize = true;
            flowLayoutPanel5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel5.Controls.Add(checkOpacity);
            flowLayoutPanel5.Controls.Add(numOpacity);
            flowLayoutPanel5.Location = new Point(130, 94);
            flowLayoutPanel5.Name = "flowLayoutPanel5";
            flowLayoutPanel5.Size = new Size(182, 23);
            flowLayoutPanel5.TabIndex = 5;
            // 
            // checkOpacity
            // 
            checkOpacity.Anchor = AnchorStyles.Right;
            checkOpacity.AutoSize = true;
            checkOpacity.FlatStyle = FlatStyle.System;
            checkOpacity.Location = new Point(0, 1);
            checkOpacity.Margin = new Padding(0);
            checkOpacity.Name = "checkOpacity";
            checkOpacity.Size = new Size(132, 20);
            checkOpacity.TabIndex = 4;
            checkOpacity.Tag = "Top";
            checkOpacity.Text = "Override opacity %";
            checkOpacity.UseVisualStyleBackColor = true;
            checkOpacity.CheckedChanged += checkOpacity_CheckedChanged;
            // 
            // numOpacity
            // 
            numOpacity.Enabled = false;
            numOpacity.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numOpacity.Location = new Point(132, 0);
            numOpacity.Margin = new Padding(0);
            numOpacity.Name = "numOpacity";
            numOpacity.Size = new Size(50, 23);
            numOpacity.TabIndex = 1;
            numOpacity.TextAlign = HorizontalAlignment.Right;
            numOpacity.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numOpacity.ValueChanged += numOpacity_ValueChanged;
            // 
            // btnReset
            // 
            btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReset.AutoSize = true;
            btnReset.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnReset.Location = new Point(349, 132);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(45, 25);
            btnReset.TabIndex = 3;
            btnReset.Text = "&Reset";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // flowLayoutPanel4
            // 
            flowLayoutPanel4.AutoSize = true;
            flowLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel4.Controls.Add(labelOffset);
            flowLayoutPanel4.Controls.Add(numX);
            flowLayoutPanel4.Controls.Add(numY);
            flowLayoutPanel4.Location = new Point(131, 59);
            flowLayoutPanel4.Name = "flowLayoutPanel4";
            flowLayoutPanel4.Size = new Size(183, 29);
            flowLayoutPanel4.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(checkLeft);
            flowLayoutPanel1.Controls.Add(checkCenter);
            flowLayoutPanel1.Controls.Add(checkRight);
            flowLayoutPanel1.Controls.Add(checkHScreen);
            flowLayoutPanel1.Location = new Point(94, 17);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(270, 26);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // checkHScreen
            // 
            checkHScreen.AutoSize = true;
            checkHScreen.FlatStyle = FlatStyle.System;
            checkHScreen.Location = new Point(200, 3);
            checkHScreen.Name = "checkHScreen";
            checkHScreen.Size = new Size(67, 20);
            checkHScreen.TabIndex = 3;
            checkHScreen.Tag = "OS";
            checkHScreen.Text = "Screen";
            checkHScreen.UseVisualStyleBackColor = true;
            checkHScreen.CheckedChanged += checkHorizontal_CheckedChanged;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.Controls.Add(checkTop);
            flowLayoutPanel2.Controls.Add(checkMiddle);
            flowLayoutPanel2.Controls.Add(checkBottom);
            flowLayoutPanel2.Controls.Add(checkVScreen);
            flowLayoutPanel2.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel2.Location = new Point(14, 48);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(78, 104);
            flowLayoutPanel2.TabIndex = 1;
            // 
            // checkVScreen
            // 
            checkVScreen.AutoSize = true;
            checkVScreen.FlatStyle = FlatStyle.System;
            checkVScreen.Location = new Point(3, 81);
            checkVScreen.Name = "checkVScreen";
            checkVScreen.Size = new Size(67, 20);
            checkVScreen.TabIndex = 3;
            checkVScreen.Tag = "OS";
            checkVScreen.Text = "Screen";
            checkVScreen.UseVisualStyleBackColor = true;
            checkVScreen.CheckedChanged += checkVertical_CheckedChanged;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            flowLayoutPanel3.AutoSize = true;
            flowLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel3.Controls.Add(btnAccept);
            flowLayoutPanel3.Controls.Add(btnCancel);
            flowLayoutPanel3.Location = new Point(253, 274);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(162, 29);
            flowLayoutPanel3.TabIndex = 4;
            // 
            // lblAdvise
            // 
            lblAdvise.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblAdvise.Location = new Point(15, 228);
            lblAdvise.Name = "lblAdvise";
            lblAdvise.Size = new Size(400, 39);
            lblAdvise.TabIndex = 3;
            lblAdvise.Text = "\"Screen\" based positions will be relative to the top / left corner of your primary monitor.";
            // 
            // checkShowAll
            // 
            checkShowAll.AutoSize = true;
            checkShowAll.FlatStyle = FlatStyle.System;
            checkShowAll.Location = new Point(143, 35);
            checkShowAll.Margin = new Padding(0);
            checkShowAll.Name = "checkShowAll";
            checkShowAll.Size = new Size(122, 20);
            checkShowAll.TabIndex = 2;
            checkShowAll.Text = "Show all overlays";
            checkShowAll.UseVisualStyleBackColor = true;
            checkShowAll.CheckedChanged += checkShowAll_CheckedChanged;
            // 
            // FormAdjustOverlay
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnCancel;
            ClientSize = new Size(427, 315);
            Controls.Add(lblAdvise);
            Controls.Add(flowLayoutPanel3);
            Controls.Add(checkShowAll);
            Controls.Add(groupBox1);
            Controls.Add(comboPlotter);
            Controls.Add(lblChoose);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAdjustOverlay";
            StartPosition = FormStartPosition.Manual;
            Text = "Adjust Overlays";
            TopMost = true;
            ((System.ComponentModel.ISupportInitialize)numY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            flowLayoutPanel5.ResumeLayout(false);
            flowLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOpacity).EndInit();
            flowLayoutPanel4.ResumeLayout(false);
            flowLayoutPanel4.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            flowLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblChoose;
        private ComboBox comboPlotter;
        private Button btnCancel;
        private Button btnAccept;
        private CheckBox checkLeft;
        private CheckBox checkCenter;
        private CheckBox checkRight;
        private CheckBox checkTop;
        private CheckBox checkMiddle;
        private CheckBox checkBottom;
        private Label labelOffset;
        private NumericUpDown numY;
        private NumericUpDown numX;
        private GroupBox groupBox1;
        private FlowLayoutPanel flowLayoutPanel4;
        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private FlowLayoutPanel flowLayoutPanel3;
        private CheckBox checkHScreen;
        private CheckBox checkVScreen;
        private Button btnReset;
        private Label lblAdvise;
        private CheckBox checkOpacity;
        private FlowLayoutPanel flowLayoutPanel5;
        private NumericUpDown numOpacity;
        private CheckBox checkShowAll;
    }
}