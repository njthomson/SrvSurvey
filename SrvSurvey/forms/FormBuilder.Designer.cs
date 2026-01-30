using System.Diagnostics;

namespace SrvSurvey
{
    partial class FormBuilder
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

            if (game.status != null)
                game.status.StatusChanged -= Status_StatusChanged;
            
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBuilder));
            btnStartPolygon = new Button();
            btnAddPoint = new Button();
            btnEndPolygon = new Button();
            button4 = new Button();
            btnCommitBuilding = new Button();
            txtBuildingName = new TextBox();
            btnStartCircle = new Button();
            btnEndCircle = new Button();
            numCircle = new NumericUpDown();
            button8 = new Button();
            btnBattery = new Button();
            groupBox1 = new GroupBox();
            floorThree = new CheckBox();
            floorTwo = new CheckBox();
            floorOne = new CheckBox();
            levelThree = new CheckBox();
            levelTwo = new CheckBox();
            levelZero = new CheckBox();
            levelOne = new CheckBox();
            btnDoor2 = new Button();
            comboPOI = new ComboBox();
            button14 = new Button();
            button12 = new Button();
            btnDataTerminal = new Button();
            btnMedkit = new Button();
            label2 = new Label();
            label1 = new Label();
            groupBox2 = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)numCircle).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnStartPolygon
            // 
            btnStartPolygon.Enabled = false;
            btnStartPolygon.Location = new Point(6, 22);
            btnStartPolygon.Name = "btnStartPolygon";
            btnStartPolygon.Size = new Size(75, 23);
            btnStartPolygon.TabIndex = 0;
            btnStartPolygon.Text = "start";
            btnStartPolygon.UseVisualStyleBackColor = true;
            btnStartPolygon.Click += btnStartPolygon_Click;
            // 
            // btnAddPoint
            // 
            btnAddPoint.Enabled = false;
            btnAddPoint.Location = new Point(87, 22);
            btnAddPoint.Name = "btnAddPoint";
            btnAddPoint.Size = new Size(75, 23);
            btnAddPoint.TabIndex = 1;
            btnAddPoint.Text = "next";
            btnAddPoint.UseVisualStyleBackColor = true;
            btnAddPoint.Click += btnAddPoint_Click;
            // 
            // btnEndPolygon
            // 
            btnEndPolygon.Enabled = false;
            btnEndPolygon.Location = new Point(168, 22);
            btnEndPolygon.Name = "btnEndPolygon";
            btnEndPolygon.Size = new Size(75, 23);
            btnEndPolygon.TabIndex = 2;
            btnEndPolygon.Text = "end/add";
            btnEndPolygon.UseVisualStyleBackColor = true;
            btnEndPolygon.Click += btnEndPolygon_Click;
            // 
            // button4
            // 
            button4.Enabled = false;
            button4.Location = new Point(249, 22);
            button4.Name = "button4";
            button4.Size = new Size(71, 23);
            button4.TabIndex = 3;
            button4.Text = "remove last";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // btnCommitBuilding
            // 
            btnCommitBuilding.Enabled = false;
            btnCommitBuilding.Location = new Point(6, 103);
            btnCommitBuilding.Name = "btnCommitBuilding";
            btnCommitBuilding.Size = new Size(108, 23);
            btnCommitBuilding.TabIndex = 4;
            btnCommitBuilding.Text = "add building";
            btnCommitBuilding.UseVisualStyleBackColor = true;
            btnCommitBuilding.Click += btnCommitBuilding_Click;
            // 
            // txtBuildingName
            // 
            txtBuildingName.Enabled = false;
            txtBuildingName.Location = new Point(120, 103);
            txtBuildingName.Name = "txtBuildingName";
            txtBuildingName.Size = new Size(261, 23);
            txtBuildingName.TabIndex = 5;
            // 
            // btnStartCircle
            // 
            btnStartCircle.Enabled = false;
            btnStartCircle.Location = new Point(6, 60);
            btnStartCircle.Name = "btnStartCircle";
            btnStartCircle.Size = new Size(75, 23);
            btnStartCircle.TabIndex = 6;
            btnStartCircle.Text = "circle";
            btnStartCircle.UseVisualStyleBackColor = true;
            btnStartCircle.Click += btnStartCircle_Click;
            // 
            // btnEndCircle
            // 
            btnEndCircle.Enabled = false;
            btnEndCircle.Location = new Point(168, 60);
            btnEndCircle.Name = "btnEndCircle";
            btnEndCircle.Size = new Size(75, 23);
            btnEndCircle.TabIndex = 7;
            btnEndCircle.Text = "add";
            btnEndCircle.UseVisualStyleBackColor = true;
            btnEndCircle.Click += btnEndCircle_Click;
            // 
            // numCircle
            // 
            numCircle.Enabled = false;
            numCircle.Location = new Point(87, 60);
            numCircle.Name = "numCircle";
            numCircle.Size = new Size(75, 23);
            numCircle.TabIndex = 8;
            numCircle.TextAlign = HorizontalAlignment.Right;
            numCircle.ValueChanged += numCircle_ValueChanged;
            // 
            // button8
            // 
            button8.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button8.Location = new Point(329, 287);
            button8.Name = "button8";
            button8.Size = new Size(75, 23);
            button8.TabIndex = 9;
            button8.Text = "Export";
            button8.UseVisualStyleBackColor = true;
            button8.Click += btnExport_Click;
            // 
            // btnBattery
            // 
            btnBattery.Location = new Point(162, 42);
            btnBattery.Name = "btnBattery";
            btnBattery.Size = new Size(52, 23);
            btnBattery.TabIndex = 10;
            btnBattery.Text = "Battery";
            btnBattery.UseVisualStyleBackColor = true;
            btnBattery.Click += btnNamedPoi_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(floorThree);
            groupBox1.Controls.Add(floorTwo);
            groupBox1.Controls.Add(floorOne);
            groupBox1.Controls.Add(levelThree);
            groupBox1.Controls.Add(levelTwo);
            groupBox1.Controls.Add(levelZero);
            groupBox1.Controls.Add(levelOne);
            groupBox1.Controls.Add(btnDoor2);
            groupBox1.Controls.Add(comboPOI);
            groupBox1.Controls.Add(button14);
            groupBox1.Controls.Add(button12);
            groupBox1.Controls.Add(btnDataTerminal);
            groupBox1.Controls.Add(btnMedkit);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(btnBattery);
            groupBox1.Location = new Point(12, 160);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(387, 113);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "Add POI:";
            // 
            // floorThree
            // 
            floorThree.AutoSize = true;
            floorThree.Location = new Point(336, 17);
            floorThree.Name = "floorThree";
            floorThree.Size = new Size(32, 19);
            floorThree.TabIndex = 29;
            floorThree.Text = "3";
            floorThree.UseVisualStyleBackColor = true;
            floorThree.CheckedChanged += floorOne_CheckedChanged;
            // 
            // floorTwo
            // 
            floorTwo.AutoSize = true;
            floorTwo.Location = new Point(298, 17);
            floorTwo.Name = "floorTwo";
            floorTwo.Size = new Size(32, 19);
            floorTwo.TabIndex = 28;
            floorTwo.Text = "2";
            floorTwo.UseVisualStyleBackColor = true;
            floorTwo.CheckedChanged += floorOne_CheckedChanged;
            // 
            // floorOne
            // 
            floorOne.AutoSize = true;
            floorOne.Checked = true;
            floorOne.CheckState = CheckState.Checked;
            floorOne.Location = new Point(260, 17);
            floorOne.Name = "floorOne";
            floorOne.Size = new Size(32, 19);
            floorOne.TabIndex = 26;
            floorOne.Text = "1";
            floorOne.UseVisualStyleBackColor = true;
            floorOne.CheckedChanged += floorOne_CheckedChanged;
            // 
            // levelThree
            // 
            levelThree.AutoSize = true;
            levelThree.Location = new Point(163, 17);
            levelThree.Name = "levelThree";
            levelThree.Size = new Size(32, 19);
            levelThree.TabIndex = 25;
            levelThree.Text = "3";
            levelThree.UseVisualStyleBackColor = true;
            levelThree.CheckedChanged += levelZero_CheckedChanged;
            // 
            // levelTwo
            // 
            levelTwo.AutoSize = true;
            levelTwo.Location = new Point(125, 17);
            levelTwo.Name = "levelTwo";
            levelTwo.Size = new Size(32, 19);
            levelTwo.TabIndex = 24;
            levelTwo.Text = "2";
            levelTwo.UseVisualStyleBackColor = true;
            levelTwo.CheckedChanged += levelZero_CheckedChanged;
            // 
            // levelZero
            // 
            levelZero.AutoSize = true;
            levelZero.Checked = true;
            levelZero.CheckState = CheckState.Checked;
            levelZero.Location = new Point(49, 17);
            levelZero.Name = "levelZero";
            levelZero.Size = new Size(32, 19);
            levelZero.TabIndex = 23;
            levelZero.Text = "0";
            levelZero.UseVisualStyleBackColor = true;
            levelZero.CheckedChanged += levelZero_CheckedChanged;
            // 
            // levelOne
            // 
            levelOne.AutoSize = true;
            levelOne.Location = new Point(87, 17);
            levelOne.Name = "levelOne";
            levelOne.Size = new Size(32, 19);
            levelOne.TabIndex = 22;
            levelOne.Text = "1";
            levelOne.UseVisualStyleBackColor = true;
            levelOne.CheckedChanged += levelZero_CheckedChanged;
            // 
            // btnDoor2
            // 
            btnDoor2.Location = new Point(6, 42);
            btnDoor2.Name = "btnDoor2";
            btnDoor2.Size = new Size(57, 23);
            btnDoor2.TabIndex = 21;
            btnDoor2.Text = "Door";
            btnDoor2.UseVisualStyleBackColor = true;
            btnDoor2.Click += btnDoor;
            // 
            // comboPOI
            // 
            comboPOI.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPOI.FormattingEnabled = true;
            comboPOI.Items.AddRange(new object[] { "Power", "Alarm", "Auth", "Personel", "Ship", "Turrets", "Synthesis Automation Unit", "Sample Containment Unit", "Catalyst Management Unit", "Environmental Regulation Unit" });
            comboPOI.Location = new Point(69, 71);
            comboPOI.Name = "comboPOI";
            comboPOI.Size = new Size(261, 23);
            comboPOI.TabIndex = 20;
            // 
            // button14
            // 
            button14.Location = new Point(6, 71);
            button14.Name = "button14";
            button14.Size = new Size(57, 23);
            button14.TabIndex = 19;
            button14.Text = "Add:";
            button14.UseVisualStyleBackColor = true;
            button14.Click += button14_Click;
            // 
            // button12
            // 
            button12.Location = new Point(278, 42);
            button12.Name = "button12";
            button12.Size = new Size(52, 23);
            button12.TabIndex = 17;
            button12.Text = "Atmos";
            button12.UseVisualStyleBackColor = true;
            button12.Click += btnNamedPoi_Click;
            // 
            // btnDataTerminal
            // 
            btnDataTerminal.Location = new Point(69, 42);
            btnDataTerminal.Name = "btnDataTerminal";
            btnDataTerminal.Size = new Size(87, 23);
            btnDataTerminal.TabIndex = 16;
            btnDataTerminal.Text = "Data terminal";
            btnDataTerminal.UseVisualStyleBackColor = true;
            btnDataTerminal.Click += btnDataTerminal_Click;
            // 
            // btnMedkit
            // 
            btnMedkit.Location = new Point(220, 42);
            btnMedkit.Name = "btnMedkit";
            btnMedkit.Size = new Size(52, 23);
            btnMedkit.TabIndex = 15;
            btnMedkit.Text = "Medkit";
            btnMedkit.UseVisualStyleBackColor = true;
            btnMedkit.Click += btnNamedPoi_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 17);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 14;
            label2.Text = "Level:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(202, 17);
            label1.Name = "label1";
            label1.Size = new Size(52, 15);
            label1.TabIndex = 13;
            label1.Text = "|    Floor:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnStartPolygon);
            groupBox2.Controls.Add(btnAddPoint);
            groupBox2.Controls.Add(btnEndPolygon);
            groupBox2.Controls.Add(numCircle);
            groupBox2.Controls.Add(button4);
            groupBox2.Controls.Add(btnEndCircle);
            groupBox2.Controls.Add(btnCommitBuilding);
            groupBox2.Controls.Add(btnStartCircle);
            groupBox2.Controls.Add(txtBuildingName);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(387, 142);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            groupBox2.Text = "Add buildings:";
            // 
            // FormBuilder
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(414, 322);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(button8);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormBuilder";
            Text = "Human site builder";
            ((System.ComponentModel.ISupportInitialize)numCircle).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnStartPolygon;
        private Button btnAddPoint;
        private Button btnEndPolygon;
        private Button button4;
        private Button btnCommitBuilding;
        private TextBox txtBuildingName;
        private Button btnStartCircle;
        private Button btnEndCircle;
        private NumericUpDown numCircle;
        private Button button8;
        private Button btnBattery;
        private GroupBox groupBox1;
        private Label label2;
        private Label label1;
        private Button button14;
        private Button button12;
        private Button btnDataTerminal;
        private Button btnMedkit;
        private Button btnDoor2;
        private ComboBox comboPOI;
        private CheckBox floorThree;
        private CheckBox floorTwo;
        private CheckBox floorOne;
        private CheckBox levelThree;
        private CheckBox levelTwo;
        private CheckBox levelZero;
        private CheckBox levelOne;
        private GroupBox groupBox2;
    }
}