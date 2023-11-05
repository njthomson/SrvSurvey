namespace SrvSurvey
{
    partial class FormEditMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditMap));
            tabs = new TabControl();
            tabItems = new TabPage();
            btnSaveEdits = new Button();
            btnRemovePoi = new Button();
            btnNewPoi = new Button();
            listPoi = new ListView();
            colName = new ColumnHeader();
            colDist = new ColumnHeader();
            colAngle = new ColumnHeader();
            colType = new ColumnHeader();
            colRot = new ColumnHeader();
            colStatus = new ColumnHeader();
            groupCurrentPoi = new GroupBox();
            checkPoiPrecision = new CheckBox();
            btnPoiApply = new Button();
            checkApplyPoiLive = new CheckBox();
            comboPoiStatus = new ComboBox();
            comboPoiType = new ComboBox();
            numPoiRot = new NumericUpDown();
            numPoiAngle = new NumericUpDown();
            numPoiDist = new NumericUpDown();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            txtPoiName = new TextBox();
            tabBackground = new TabPage();
            groupBox1 = new GroupBox();
            label13 = new Label();
            label12 = new Label();
            txtDeltaLong = new TextBox();
            txtDeltaLat = new TextBox();
            label11 = new Label();
            btnSaveBackground = new Button();
            checkApplyBackgroundLive = new CheckBox();
            btnApplyImage = new Button();
            btnChooseBackgroundImage = new Button();
            numScaleFactor = new NumericUpDown();
            label3 = new Label();
            numOriginTop = new NumericUpDown();
            numOriginLeft = new NumericUpDown();
            label2 = new Label();
            txtBackgroundImage = new TextBox();
            label1 = new Label();
            mapZoom = new TrackBar();
            label4 = new Label();
            btnFocusGame = new Button();
            checkHighlightAll = new CheckBox();
            lblSiteType = new Label();
            checkHideAllPoi = new CheckBox();
            tabs.SuspendLayout();
            tabItems.SuspendLayout();
            groupCurrentPoi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPoiRot).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPoiAngle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPoiDist).BeginInit();
            tabBackground.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numScaleFactor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numOriginTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numOriginLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mapZoom).BeginInit();
            SuspendLayout();
            // 
            // tabs
            // 
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabs.Controls.Add(tabItems);
            tabs.Controls.Add(tabBackground);
            tabs.Location = new Point(0, 28);
            tabs.Multiline = true;
            tabs.Name = "tabs";
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(859, 433);
            tabs.TabIndex = 0;
            // 
            // tabItems
            // 
            tabItems.BackColor = SystemColors.Control;
            tabItems.Controls.Add(btnSaveEdits);
            tabItems.Controls.Add(btnRemovePoi);
            tabItems.Controls.Add(btnNewPoi);
            tabItems.Controls.Add(listPoi);
            tabItems.Controls.Add(groupCurrentPoi);
            tabItems.Location = new Point(4, 24);
            tabItems.Name = "tabItems";
            tabItems.Padding = new Padding(3);
            tabItems.Size = new Size(851, 405);
            tabItems.TabIndex = 0;
            tabItems.Text = "Site POI";
            // 
            // btnSaveEdits
            // 
            btnSaveEdits.Location = new Point(8, 363);
            btnSaveEdits.Name = "btnSaveEdits";
            btnSaveEdits.Size = new Size(189, 23);
            btnSaveEdits.TabIndex = 4;
            btnSaveEdits.Text = "Save changes";
            btnSaveEdits.UseVisualStyleBackColor = true;
            btnSaveEdits.Click += btnSaveEdits_Click;
            // 
            // btnRemovePoi
            // 
            btnRemovePoi.Location = new Point(91, 334);
            btnRemovePoi.Name = "btnRemovePoi";
            btnRemovePoi.Size = new Size(106, 23);
            btnRemovePoi.TabIndex = 3;
            btnRemovePoi.Text = "Remove POI";
            btnRemovePoi.UseVisualStyleBackColor = true;
            btnRemovePoi.Click += btnRemovePoi_Click;
            // 
            // btnNewPoi
            // 
            btnNewPoi.Location = new Point(8, 334);
            btnNewPoi.Name = "btnNewPoi";
            btnNewPoi.Size = new Size(75, 23);
            btnNewPoi.TabIndex = 2;
            btnNewPoi.Text = "&New POI";
            btnNewPoi.UseVisualStyleBackColor = true;
            btnNewPoi.Click += btnNewPoi_Click;
            // 
            // listPoi
            // 
            listPoi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listPoi.AutoArrange = false;
            listPoi.CausesValidation = false;
            listPoi.Columns.AddRange(new ColumnHeader[] { colName, colDist, colAngle, colType, colRot, colStatus });
            listPoi.FullRowSelect = true;
            listPoi.GridLines = true;
            listPoi.Location = new Point(203, 6);
            listPoi.MultiSelect = false;
            listPoi.Name = "listPoi";
            listPoi.ShowGroups = false;
            listPoi.Size = new Size(648, 399);
            listPoi.TabIndex = 1;
            listPoi.UseCompatibleStateImageBehavior = false;
            listPoi.View = View.Details;
            listPoi.ItemSelectionChanged += listPoi_ItemSelectionChanged;
            listPoi.DoubleClick += listPoi_DoubleClick;
            // 
            // colName
            // 
            colName.Text = "Name";
            // 
            // colDist
            // 
            colDist.Text = "Distance";
            // 
            // colAngle
            // 
            colAngle.Text = "Angle";
            // 
            // colType
            // 
            colType.Text = "Type";
            // 
            // colRot
            // 
            colRot.Text = "Rotation";
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            // 
            // groupCurrentPoi
            // 
            groupCurrentPoi.Controls.Add(checkPoiPrecision);
            groupCurrentPoi.Controls.Add(btnPoiApply);
            groupCurrentPoi.Controls.Add(checkApplyPoiLive);
            groupCurrentPoi.Controls.Add(comboPoiStatus);
            groupCurrentPoi.Controls.Add(comboPoiType);
            groupCurrentPoi.Controls.Add(numPoiRot);
            groupCurrentPoi.Controls.Add(numPoiAngle);
            groupCurrentPoi.Controls.Add(numPoiDist);
            groupCurrentPoi.Controls.Add(label10);
            groupCurrentPoi.Controls.Add(label9);
            groupCurrentPoi.Controls.Add(label8);
            groupCurrentPoi.Controls.Add(label7);
            groupCurrentPoi.Controls.Add(label6);
            groupCurrentPoi.Controls.Add(label5);
            groupCurrentPoi.Controls.Add(txtPoiName);
            groupCurrentPoi.Location = new Point(8, 6);
            groupCurrentPoi.Name = "groupCurrentPoi";
            groupCurrentPoi.Size = new Size(187, 322);
            groupCurrentPoi.TabIndex = 0;
            groupCurrentPoi.TabStop = false;
            groupCurrentPoi.Text = "Current POI:";
            // 
            // checkPoiPrecision
            // 
            checkPoiPrecision.AutoSize = true;
            checkPoiPrecision.Location = new Point(67, 202);
            checkPoiPrecision.Name = "checkPoiPrecision";
            checkPoiPrecision.Size = new Size(94, 19);
            checkPoiPrecision.TabIndex = 13;
            checkPoiPrecision.Text = "&Adjust by 0.1";
            checkPoiPrecision.UseVisualStyleBackColor = true;
            checkPoiPrecision.CheckedChanged += checkPoiPrecision_CheckedChanged;
            // 
            // btnPoiApply
            // 
            btnPoiApply.Location = new Point(6, 185);
            btnPoiApply.Name = "btnPoiApply";
            btnPoiApply.Size = new Size(55, 40);
            btnPoiApply.TabIndex = 14;
            btnPoiApply.Text = "Apply";
            btnPoiApply.UseVisualStyleBackColor = true;
            btnPoiApply.Click += btnPoiApply_Click;
            // 
            // checkApplyPoiLive
            // 
            checkApplyPoiLive.AutoSize = true;
            checkApplyPoiLive.Checked = true;
            checkApplyPoiLive.CheckState = CheckState.Checked;
            checkApplyPoiLive.Location = new Point(67, 185);
            checkApplyPoiLive.Name = "checkApplyPoiLive";
            checkApplyPoiLive.Size = new Size(87, 19);
            checkApplyPoiLive.TabIndex = 12;
            checkApplyPoiLive.Text = "Live update";
            checkApplyPoiLive.UseVisualStyleBackColor = true;
            // 
            // comboPoiStatus
            // 
            comboPoiStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPoiStatus.FormattingEnabled = true;
            comboPoiStatus.Location = new Point(67, 159);
            comboPoiStatus.Name = "comboPoiStatus";
            comboPoiStatus.Size = new Size(100, 23);
            comboPoiStatus.TabIndex = 11;
            comboPoiStatus.SelectedValueChanged += comboPoiStatus_SelectedValueChanged;
            // 
            // comboPoiType
            // 
            comboPoiType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPoiType.FormattingEnabled = true;
            comboPoiType.Location = new Point(67, 101);
            comboPoiType.Name = "comboPoiType";
            comboPoiType.Size = new Size(100, 23);
            comboPoiType.TabIndex = 7;
            comboPoiType.SelectedValueChanged += comboPoiType_SelectedValueChanged;
            // 
            // numPoiRot
            // 
            numPoiRot.DecimalPlaces = 2;
            numPoiRot.Location = new Point(67, 130);
            numPoiRot.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numPoiRot.Minimum = new decimal(new int[] { 1, 0, 0, int.MinValue });
            numPoiRot.Name = "numPoiRot";
            numPoiRot.Size = new Size(100, 23);
            numPoiRot.TabIndex = 9;
            numPoiRot.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPoiRot.ValueChanged += numPoiRot_ValueChanged;
            numPoiRot.KeyDown += numPoiRot_KeyDown;
            // 
            // numPoiAngle
            // 
            numPoiAngle.DecimalPlaces = 2;
            numPoiAngle.Location = new Point(67, 72);
            numPoiAngle.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numPoiAngle.Name = "numPoiAngle";
            numPoiAngle.Size = new Size(100, 23);
            numPoiAngle.TabIndex = 5;
            numPoiAngle.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPoiAngle.ValueChanged += numPoiAngle_ValueChanged;
            numPoiAngle.KeyDown += numPoiAngle_KeyDown;
            // 
            // numPoiDist
            // 
            numPoiDist.DecimalPlaces = 2;
            numPoiDist.Location = new Point(67, 45);
            numPoiDist.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numPoiDist.Name = "numPoiDist";
            numPoiDist.Size = new Size(100, 23);
            numPoiDist.TabIndex = 3;
            numPoiDist.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPoiDist.ValueChanged += numPoiDist_ValueChanged;
            numPoiDist.KeyDown += numPoiDist_KeyDown;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(20, 162);
            label10.Name = "label10";
            label10.Size = new Size(42, 15);
            label10.TabIndex = 10;
            label10.Text = "Status:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 132);
            label9.Name = "label9";
            label9.Size = new Size(55, 15);
            label9.TabIndex = 8;
            label9.Text = "Rotation:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(27, 104);
            label8.Name = "label8";
            label8.Size = new Size(34, 15);
            label8.TabIndex = 6;
            label8.Text = "Type:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(20, 72);
            label7.Name = "label7";
            label7.Size = new Size(41, 15);
            label7.TabIndex = 4;
            label7.Text = "Angle:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 47);
            label6.Name = "label6";
            label6.Size = new Size(55, 15);
            label6.TabIndex = 2;
            label6.Text = "Distance:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(19, 19);
            label5.Name = "label5";
            label5.Size = new Size(42, 15);
            label5.TabIndex = 0;
            label5.Text = "Name:";
            // 
            // txtPoiName
            // 
            txtPoiName.Location = new Point(67, 16);
            txtPoiName.Name = "txtPoiName";
            txtPoiName.Size = new Size(100, 23);
            txtPoiName.TabIndex = 1;
            txtPoiName.TextChanged += txtPoiName_TextChanged;
            // 
            // tabBackground
            // 
            tabBackground.BackColor = SystemColors.Control;
            tabBackground.Controls.Add(groupBox1);
            tabBackground.Controls.Add(btnSaveBackground);
            tabBackground.Controls.Add(checkApplyBackgroundLive);
            tabBackground.Controls.Add(btnApplyImage);
            tabBackground.Controls.Add(btnChooseBackgroundImage);
            tabBackground.Controls.Add(numScaleFactor);
            tabBackground.Controls.Add(label3);
            tabBackground.Controls.Add(numOriginTop);
            tabBackground.Controls.Add(numOriginLeft);
            tabBackground.Controls.Add(label2);
            tabBackground.Controls.Add(txtBackgroundImage);
            tabBackground.Controls.Add(label1);
            tabBackground.Location = new Point(4, 24);
            tabBackground.Name = "tabBackground";
            tabBackground.Padding = new Padding(3);
            tabBackground.Size = new Size(851, 405);
            tabBackground.TabIndex = 1;
            tabBackground.Text = "Background image";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label13);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(txtDeltaLong);
            groupBox1.Controls.Add(txtDeltaLat);
            groupBox1.Controls.Add(label11);
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            groupBox1.Location = new Point(40, 123);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(248, 150);
            groupBox1.TabIndex = 24;
            groupBox1.TabStop = false;
            groupBox1.Text = "Origin alignment:";
            // 
            // label13
            // 
            label13.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            label13.Location = new Point(5, 124);
            label13.Name = "label13";
            label13.Size = new Size(64, 15);
            label13.TabIndex = 4;
            label13.Text = "Delta long:";
            // 
            // label12
            // 
            label12.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            label12.Location = new Point(16, 95);
            label12.Name = "label12";
            label12.Size = new Size(53, 15);
            label12.TabIndex = 3;
            label12.Text = "Delta lat:";
            // 
            // txtDeltaLong
            // 
            txtDeltaLong.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtDeltaLong.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtDeltaLong.Location = new Point(75, 121);
            txtDeltaLong.Name = "txtDeltaLong";
            txtDeltaLong.ReadOnly = true;
            txtDeltaLong.Size = new Size(100, 23);
            txtDeltaLong.TabIndex = 2;
            // 
            // txtDeltaLat
            // 
            txtDeltaLat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtDeltaLat.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtDeltaLat.Location = new Point(75, 92);
            txtDeltaLat.Name = "txtDeltaLat";
            txtDeltaLat.ReadOnly = true;
            txtDeltaLat.Size = new Size(100, 23);
            txtDeltaLat.TabIndex = 1;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            label11.Location = new Point(6, 19);
            label11.Name = "label11";
            label11.Size = new Size(233, 60);
            label11.TabIndex = 0;
            label11.Text = "Face north move your cmdr on foot\r\nuntil deltas reach zero:\r\n\r\n(Switch weapon or tool to force an update)";
            // 
            // btnSaveBackground
            // 
            btnSaveBackground.Location = new Point(411, 123);
            btnSaveBackground.Name = "btnSaveBackground";
            btnSaveBackground.Size = new Size(189, 23);
            btnSaveBackground.TabIndex = 23;
            btnSaveBackground.Text = "Save changes";
            btnSaveBackground.UseVisualStyleBackColor = true;
            btnSaveBackground.Click += btnSaveEdits_Click;
            // 
            // checkApplyBackgroundLive
            // 
            checkApplyBackgroundLive.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkApplyBackgroundLive.AutoSize = true;
            checkApplyBackgroundLive.Checked = true;
            checkApplyBackgroundLive.CheckState = CheckState.Checked;
            checkApplyBackgroundLive.Location = new Point(551, 66);
            checkApplyBackgroundLive.Name = "checkApplyBackgroundLive";
            checkApplyBackgroundLive.Size = new Size(152, 19);
            checkApplyBackgroundLive.TabIndex = 22;
            checkApplyBackgroundLive.Text = "Live update all numbers";
            checkApplyBackgroundLive.UseVisualStyleBackColor = true;
            // 
            // btnApplyImage
            // 
            btnApplyImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApplyImage.Location = new Point(709, 20);
            btnApplyImage.Name = "btnApplyImage";
            btnApplyImage.Size = new Size(75, 68);
            btnApplyImage.TabIndex = 21;
            btnApplyImage.Text = "&Apply";
            btnApplyImage.UseVisualStyleBackColor = true;
            btnApplyImage.Click += btnApplyImage_Click;
            // 
            // btnChooseBackgroundImage
            // 
            btnChooseBackgroundImage.Location = new Point(8, 21);
            btnChooseBackgroundImage.Name = "btnChooseBackgroundImage";
            btnChooseBackgroundImage.Size = new Size(26, 23);
            btnChooseBackgroundImage.TabIndex = 20;
            btnChooseBackgroundImage.Text = "...";
            btnChooseBackgroundImage.UseVisualStyleBackColor = true;
            btnChooseBackgroundImage.Click += btnChooseBackgroundImage_Click;
            // 
            // numScaleFactor
            // 
            numScaleFactor.DecimalPlaces = 2;
            numScaleFactor.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numScaleFactor.Location = new Point(308, 65);
            numScaleFactor.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numScaleFactor.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numScaleFactor.Name = "numScaleFactor";
            numScaleFactor.Size = new Size(120, 23);
            numScaleFactor.TabIndex = 6;
            numScaleFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numScaleFactor.ValueChanged += applyBackgroundImageNumbers;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(308, 47);
            label3.Name = "label3";
            label3.Size = new Size(71, 15);
            label3.TabIndex = 5;
            label3.Text = "Scale factor:";
            // 
            // numOriginTop
            // 
            numOriginTop.Location = new Point(134, 65);
            numOriginTop.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numOriginTop.Name = "numOriginTop";
            numOriginTop.Size = new Size(120, 23);
            numOriginTop.TabIndex = 4;
            numOriginTop.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            numOriginTop.ValueChanged += applyBackgroundImageNumbers;
            // 
            // numOriginLeft
            // 
            numOriginLeft.Location = new Point(8, 65);
            numOriginLeft.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numOriginLeft.Name = "numOriginLeft";
            numOriginLeft.Size = new Size(120, 23);
            numOriginLeft.TabIndex = 3;
            numOriginLeft.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            numOriginLeft.ValueChanged += applyBackgroundImageNumbers;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 47);
            label2.Name = "label2";
            label2.Size = new Size(243, 15);
            label2.TabIndex = 2;
            label2.Text = "Origin point relative to image top left corner:";
            // 
            // txtBackgroundImage
            // 
            txtBackgroundImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBackgroundImage.Location = new Point(40, 21);
            txtBackgroundImage.Name = "txtBackgroundImage";
            txtBackgroundImage.Size = new Size(663, 23);
            txtBackgroundImage.TabIndex = 1;
            txtBackgroundImage.Text = "D:\\code\\SrvSurvey\\SrvSurvey\\images\\bear-background.png";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 3);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 0;
            label1.Text = "Load image:";
            // 
            // mapZoom
            // 
            mapZoom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            mapZoom.Location = new Point(659, -2);
            mapZoom.Minimum = 1;
            mapZoom.Name = "mapZoom";
            mapZoom.Size = new Size(200, 45);
            mapZoom.TabIndex = 3;
            mapZoom.TickStyle = TickStyle.TopLeft;
            mapZoom.Value = 3;
            mapZoom.ValueChanged += trackBar1_ValueChanged;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(592, 7);
            label4.Name = "label4";
            label4.Size = new Size(67, 15);
            label4.TabIndex = 2;
            label4.Text = "Map zoom:";
            // 
            // btnFocusGame
            // 
            btnFocusGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFocusGame.Location = new Point(474, 3);
            btnFocusGame.Name = "btnFocusGame";
            btnFocusGame.Size = new Size(106, 23);
            btnFocusGame.TabIndex = 1;
            btnFocusGame.Text = "Focus on game";
            btnFocusGame.UseVisualStyleBackColor = true;
            btnFocusGame.Click += btnFocusGame_Click;
            // 
            // checkHighlightAll
            // 
            checkHighlightAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkHighlightAll.AutoSize = true;
            checkHighlightAll.Checked = true;
            checkHighlightAll.CheckState = CheckState.Checked;
            checkHighlightAll.Location = new Point(304, 3);
            checkHighlightAll.Name = "checkHighlightAll";
            checkHighlightAll.Size = new Size(164, 19);
            checkHighlightAll.TabIndex = 0;
            checkHighlightAll.Text = "Highlight all POI locations";
            checkHighlightAll.UseVisualStyleBackColor = true;
            checkHighlightAll.CheckedChanged += checkHighlightAll_CheckedChanged;
            // 
            // lblSiteType
            // 
            lblSiteType.AutoSize = true;
            lblSiteType.BorderStyle = BorderStyle.Fixed3D;
            lblSiteType.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblSiteType.Location = new Point(4, 3);
            lblSiteType.Name = "lblSiteType";
            lblSiteType.Size = new Size(77, 17);
            lblSiteType.TabIndex = 25;
            lblSiteType.Text = "Robolobster";
            // 
            // checkHideAllPoi
            // 
            checkHideAllPoi.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkHideAllPoi.AutoSize = true;
            checkHideAllPoi.Location = new Point(210, 3);
            checkHideAllPoi.Name = "checkHideAllPoi";
            checkHideAllPoi.Size = new Size(88, 19);
            checkHideAllPoi.TabIndex = 25;
            checkHideAllPoi.Text = "Hide all POI";
            checkHideAllPoi.UseVisualStyleBackColor = true;
            checkHideAllPoi.CheckedChanged += checkHideAllPoi_CheckedChanged;
            // 
            // FormEditMap
            // 
            AcceptButton = btnApplyImage;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnFocusGame;
            ClientSize = new Size(859, 461);
            Controls.Add(checkHideAllPoi);
            Controls.Add(lblSiteType);
            Controls.Add(checkHighlightAll);
            Controls.Add(btnFocusGame);
            Controls.Add(label4);
            Controls.Add(mapZoom);
            Controls.Add(tabs);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormEditMap";
            Text = "Map editor";
            Activated += FormEditMap_Activated;
            Deactivate += FormEditMap_Deactivate;
            FormClosed += FormEditMap_FormClosed;
            Load += FormEditMap_Load;
            tabs.ResumeLayout(false);
            tabItems.ResumeLayout(false);
            groupCurrentPoi.ResumeLayout(false);
            groupCurrentPoi.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPoiRot).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPoiAngle).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPoiDist).EndInit();
            tabBackground.ResumeLayout(false);
            tabBackground.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numScaleFactor).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOriginTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOriginLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)mapZoom).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabControl tabs;
        private TabPage tabItems;
        private TabPage tabBackground;
        private NumericUpDown numScaleFactor;
        private Label label3;
        private NumericUpDown numOriginTop;
        private NumericUpDown numOriginLeft;
        private Label label2;
        private TextBox txtBackgroundImage;
        private Label label1;
        private Button btnChooseBackgroundImage;
        private Button btnApplyImage;
        private CheckBox checkApplyBackgroundLive;
        private TrackBar mapZoom;
        private Label label4;
        private TextBox txtPoiName;
        private GroupBox groupCurrentPoi;
        private ComboBox comboPoiStatus;
        private ComboBox comboPoiType;
        private NumericUpDown numPoiRot;
        private NumericUpDown numPoiAngle;
        private NumericUpDown numPoiDist;
        private Label label10;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private CheckBox checkApplyPoiLive;
        private Button btnPoiApply;
        private CheckBox checkPoiPrecision;
        private ListView listPoi;
        private ColumnHeader colName;
        private ColumnHeader colDist;
        private ColumnHeader colAngle;
        private ColumnHeader colType;
        private ColumnHeader colRot;
        private ColumnHeader colStatus;
        private Button btnFocusGame;
        private Button btnNewPoi;
        public CheckBox checkHighlightAll;
        private Button btnRemovePoi;
        private Button btnSaveEdits;
        private Button btnSaveBackground;
        private GroupBox groupBox1;
        private Label label13;
        private Label label12;
        public TextBox txtDeltaLong;
        public TextBox txtDeltaLat;
        private Label label11;
        private Label lblSiteType;
        public CheckBox checkHideAllPoi;
    }
}