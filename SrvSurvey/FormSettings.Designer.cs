
namespace SrvSurvey
{
    partial class FormSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            panel1 = new Panel();
            btnNextProc = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            tabPage1 = new TabPage();
            linkDataFiles = new LinkLabel();
            checkMinimizeOnStart = new CheckBox();
            checkHideOverlayOnMouseOver = new CheckBox();
            label4 = new Label();
            checkHidePlottersFromWeapons = new CheckBox();
            checkFocusOnMinimize = new CheckBox();
            numOpacity = new NumericUpDown();
            label2 = new Label();
            trackOpacity = new TrackBar();
            txtCommander = new TextBox();
            label1 = new Label();
            tabControl1 = new TabControl();
            tabPage4 = new TabPage();
            checkBox7 = new CheckBox();
            checkBox6 = new CheckBox();
            checkBox5 = new CheckBox();
            btnClearUnclaimed = new Button();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            checkBox1 = new CheckBox();
            checkBioStatusAutoShow = new CheckBox();
            tabPage3 = new TabPage();
            checkBox3 = new CheckBox();
            label9 = new Label();
            comboGuardianWindowSize = new ComboBox();
            checkBox2 = new CheckBox();
            numAltGamma = new NumericUpDown();
            numAltBeta = new NumericUpDown();
            numAltAlpha = new NumericUpDown();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            checkRuinsMeasurementGrid = new CheckBox();
            label3 = new Label();
            checkEnableGuardianFeatures = new CheckBox();
            tabPage5 = new TabPage();
            checkDeleteScreenshotOriginal = new CheckBox();
            pictureBox3 = new PictureBox();
            checkUseGuardianAerialScreenshotsFolder = new CheckBox();
            linkTargetScreenshotFolder = new LinkLabel();
            btnChooseScreenshotSourceFolder = new Button();
            btnChooseScreenshotTargetFolder = new Button();
            lblScreenshotSource = new Label();
            lblScreenshotTarget = new Label();
            linkScreenshotSourceFolder = new LinkLabel();
            checkAddBanner = new CheckBox();
            checkProcessScreenshots = new CheckBox();
            tabPage6 = new TabPage();
            checkBox11 = new CheckBox();
            numMinScanValue = new NumericUpDown();
            checkBox10 = new CheckBox();
            checkBox9 = new CheckBox();
            pictureBox5 = new PictureBox();
            checkBox8 = new CheckBox();
            pictureBox4 = new PictureBox();
            checkBox4 = new CheckBox();
            tabPage2 = new TabPage();
            linkAboutTwo = new LinkLabel();
            linkAboutOne = new LinkLabel();
            label10 = new Label();
            panel1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOpacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackOpacity).BeginInit();
            tabControl1.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAltGamma).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAltBeta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAltAlpha).BeginInit();
            tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMinScanValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnNextProc);
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(btnCancel);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 405);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(460, 48);
            panel1.TabIndex = 1;
            // 
            // btnNextProc
            // 
            btnNextProc.Location = new Point(4, 16);
            btnNextProc.Name = "btnNextProc";
            btnNextProc.Size = new Size(243, 23);
            btnNextProc.TabIndex = 2;
            btnNextProc.Text = "Use alternate EliteDangerous window";
            btnNextProc.UseVisualStyleBackColor = true;
            btnNextProc.Visible = false;
            btnNextProc.Click += btnNextProc_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(264, 14);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(88, 27);
            btnSave.TabIndex = 3;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(358, 14);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            tabPage1.BorderStyle = BorderStyle.Fixed3D;
            tabPage1.Controls.Add(linkDataFiles);
            tabPage1.Controls.Add(checkMinimizeOnStart);
            tabPage1.Controls.Add(checkHideOverlayOnMouseOver);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(checkHidePlottersFromWeapons);
            tabPage1.Controls.Add(checkFocusOnMinimize);
            tabPage1.Controls.Add(numOpacity);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(trackOpacity);
            tabPage1.Controls.Add(txtCommander);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Margin = new Padding(4, 3, 4, 3);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 3, 4, 3);
            tabPage1.Size = new Size(452, 377);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "General";
            // 
            // linkDataFiles
            // 
            linkDataFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            linkDataFiles.AutoSize = true;
            linkDataFiles.Location = new Point(417, 12);
            linkDataFiles.Name = "linkDataFiles";
            linkDataFiles.Size = new Size(30, 15);
            linkDataFiles.TabIndex = 12;
            linkDataFiles.TabStop = true;
            linkDataFiles.Text = "data";
            linkDataFiles.Visible = false;
            linkDataFiles.LinkClicked += linkDataFiles_LinkClicked;
            // 
            // checkMinimizeOnStart
            // 
            checkMinimizeOnStart.Checked = true;
            checkMinimizeOnStart.CheckState = CheckState.Checked;
            checkMinimizeOnStart.Location = new Point(8, 108);
            checkMinimizeOnStart.Margin = new Padding(4, 3, 4, 3);
            checkMinimizeOnStart.Name = "checkMinimizeOnStart";
            checkMinimizeOnStart.Size = new Size(431, 19);
            checkMinimizeOnStart.TabIndex = 10;
            checkMinimizeOnStart.Tag = "focusGameOnStart";
            checkMinimizeOnStart.Text = "Set focus on Elite Dangerous when starting Srv Survey.";
            checkMinimizeOnStart.UseVisualStyleBackColor = true;
            // 
            // checkHideOverlayOnMouseOver
            // 
            checkHideOverlayOnMouseOver.AutoSize = true;
            checkHideOverlayOnMouseOver.Location = new Point(8, 255);
            checkHideOverlayOnMouseOver.Name = "checkHideOverlayOnMouseOver";
            checkHideOverlayOnMouseOver.Size = new Size(246, 19);
            checkHideOverlayOnMouseOver.TabIndex = 9;
            checkHideOverlayOnMouseOver.Tag = "hideOverlaysFromMouse";
            checkHideOverlayOnMouseOver.Text = "Prevent mouse entering overlay windows.";
            checkHideOverlayOnMouseOver.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Location = new Point(8, 193);
            label4.Name = "label4";
            label4.Size = new Size(427, 34);
            label4.TabIndex = 8;
            label4.Text = "Players using mouse and keyboard have reported some issues with overlay windows trapping focus from the game.";
            // 
            // checkHidePlottersFromWeapons
            // 
            checkHidePlottersFromWeapons.Checked = true;
            checkHidePlottersFromWeapons.CheckState = CheckState.Checked;
            checkHidePlottersFromWeapons.Location = new Point(8, 230);
            checkHidePlottersFromWeapons.Margin = new Padding(4, 3, 4, 3);
            checkHidePlottersFromWeapons.Name = "checkHidePlottersFromWeapons";
            checkHidePlottersFromWeapons.Size = new Size(431, 19);
            checkHidePlottersFromWeapons.TabIndex = 7;
            checkHidePlottersFromWeapons.Tag = "hidePlottersFromCombatSuits";
            checkHidePlottersFromWeapons.Text = "Disable overlays when on foot in Maverick and Dominator suits.";
            checkHidePlottersFromWeapons.UseVisualStyleBackColor = true;
            // 
            // checkFocusOnMinimize
            // 
            checkFocusOnMinimize.Checked = true;
            checkFocusOnMinimize.CheckState = CheckState.Checked;
            checkFocusOnMinimize.Location = new Point(8, 133);
            checkFocusOnMinimize.Margin = new Padding(4, 3, 4, 3);
            checkFocusOnMinimize.Name = "checkFocusOnMinimize";
            checkFocusOnMinimize.Size = new Size(431, 19);
            checkFocusOnMinimize.TabIndex = 2;
            checkFocusOnMinimize.Tag = "focusGameOnMinimize";
            checkFocusOnMinimize.Text = "Set focus on Elite Dangerous when minimizing Srv Survey.";
            checkFocusOnMinimize.UseVisualStyleBackColor = true;
            // 
            // numOpacity
            // 
            numOpacity.Location = new Point(109, 68);
            numOpacity.Margin = new Padding(4, 3, 4, 3);
            numOpacity.Name = "numOpacity";
            numOpacity.Size = new Size(61, 23);
            numOpacity.TabIndex = 6;
            numOpacity.Tag = "plotterOpacity";
            numOpacity.ValueChanged += numOpacity_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 70);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(92, 15);
            label2.TabIndex = 5;
            label2.Text = "Overlay opacity:";
            // 
            // trackOpacity
            // 
            trackOpacity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackOpacity.LargeChange = 10;
            trackOpacity.Location = new Point(178, 68);
            trackOpacity.Margin = new Padding(4, 3, 4, 3);
            trackOpacity.Maximum = 100;
            trackOpacity.Name = "trackOpacity";
            trackOpacity.Size = new Size(258, 45);
            trackOpacity.SmallChange = 5;
            trackOpacity.TabIndex = 4;
            trackOpacity.TickFrequency = 10;
            trackOpacity.Scroll += trackOpacity_Scroll;
            // 
            // txtCommander
            // 
            txtCommander.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommander.Location = new Point(8, 33);
            txtCommander.Margin = new Padding(4, 3, 4, 3);
            txtCommander.Name = "txtCommander";
            txtCommander.Size = new Size(432, 23);
            txtCommander.TabIndex = 1;
            txtCommander.Tag = "preferredCommander";
            // 
            // label1
            // 
            label1.Location = new Point(8, 12);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(430, 18);
            label1.TabIndex = 0;
            label1.Text = "Preferred Commander: (others will be ignored)";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(4, 3, 4, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(460, 405);
            tabControl1.TabIndex = 0;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(checkBox7);
            tabPage4.Controls.Add(checkBox6);
            tabPage4.Controls.Add(checkBox5);
            tabPage4.Controls.Add(btnClearUnclaimed);
            tabPage4.Controls.Add(pictureBox2);
            tabPage4.Controls.Add(pictureBox1);
            tabPage4.Controls.Add(checkBox1);
            tabPage4.Controls.Add(checkBioStatusAutoShow);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(452, 377);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Bio Scanning";
            // 
            // checkBox7
            // 
            checkBox7.AutoSize = true;
            checkBox7.Checked = true;
            checkBox7.CheckState = CheckState.Checked;
            checkBox7.Location = new Point(10, 264);
            checkBox7.Margin = new Padding(4, 3, 4, 3);
            checkBox7.Name = "checkBox7";
            checkBox7.Size = new Size(372, 19);
            checkBox7.TabIndex = 13;
            checkBox7.Tag = "autoRemoveTrackerOnSampling";
            checkBox7.Text = "Auto remove tracker location sampling an organism within 250m.";
            checkBox7.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            checkBox6.AutoSize = true;
            checkBox6.Checked = true;
            checkBox6.CheckState = CheckState.Checked;
            checkBox6.Location = new Point(40, 239);
            checkBox6.Margin = new Padding(4, 3, 4, 3);
            checkBox6.Name = "checkBox6";
            checkBox6.Size = new Size(295, 19);
            checkBox6.TabIndex = 12;
            checkBox6.Tag = "skipAnalyzedCompBioScans";
            checkBox6.Text = "But not if that organism has already been analyzed.";
            checkBox6.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Checked = true;
            checkBox5.CheckState = CheckState.Checked;
            checkBox5.Location = new Point(10, 217);
            checkBox5.Margin = new Padding(4, 3, 4, 3);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new Size(376, 19);
            checkBox5.TabIndex = 11;
            checkBox5.Tag = "autoTrackCompBioScans";
            checkBox5.Text = "Auto add tracker location when Composition scanning organisms.";
            checkBox5.UseVisualStyleBackColor = true;
            // 
            // btnClearUnclaimed
            // 
            btnClearUnclaimed.Location = new Point(10, 348);
            btnClearUnclaimed.Name = "btnClearUnclaimed";
            btnClearUnclaimed.Size = new Size(154, 23);
            btnClearUnclaimed.TabIndex = 6;
            btnClearUnclaimed.Text = "Clear unclaimed rewards";
            btnClearUnclaimed.UseVisualStyleBackColor = true;
            btnClearUnclaimed.Click += btnClearUnclaimed_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackgroundImage = (Image)resources.GetObject("pictureBox2.BackgroundImage");
            pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox2.Location = new Point(226, 67);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(109, 132);
            pictureBox2.TabIndex = 8;
            pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox1.Location = new Point(212, 11);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(154, 50);
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Location = new Point(10, 67);
            checkBox1.Margin = new Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(209, 19);
            checkBox1.TabIndex = 5;
            checkBox1.Tag = "autoShowBioPlot";
            checkBox1.Text = "Show sample scan exclusion zones";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBioStatusAutoShow
            // 
            checkBioStatusAutoShow.AutoSize = true;
            checkBioStatusAutoShow.Checked = true;
            checkBioStatusAutoShow.CheckState = CheckState.Checked;
            checkBioStatusAutoShow.Location = new Point(10, 11);
            checkBioStatusAutoShow.Margin = new Padding(4, 3, 4, 3);
            checkBioStatusAutoShow.Name = "checkBioStatusAutoShow";
            checkBioStatusAutoShow.Size = new Size(197, 19);
            checkBioStatusAutoShow.TabIndex = 4;
            checkBioStatusAutoShow.Tag = "autoShowBioSummary";
            checkBioStatusAutoShow.Text = "Show biological signal summary";
            checkBioStatusAutoShow.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.BorderStyle = BorderStyle.Fixed3D;
            tabPage3.Controls.Add(checkBox3);
            tabPage3.Controls.Add(label9);
            tabPage3.Controls.Add(comboGuardianWindowSize);
            tabPage3.Controls.Add(checkBox2);
            tabPage3.Controls.Add(numAltGamma);
            tabPage3.Controls.Add(numAltBeta);
            tabPage3.Controls.Add(numAltAlpha);
            tabPage3.Controls.Add(label8);
            tabPage3.Controls.Add(label7);
            tabPage3.Controls.Add(label6);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(checkRuinsMeasurementGrid);
            tabPage3.Controls.Add(label3);
            tabPage3.Controls.Add(checkEnableGuardianFeatures);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(452, 377);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Guardian sites";
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Checked = true;
            checkBox3.CheckState = CheckState.Checked;
            checkBox3.Location = new Point(11, 252);
            checkBox3.Margin = new Padding(4, 3, 4, 3);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(289, 19);
            checkBox3.TabIndex = 14;
            checkBox3.Tag = "rotateAndTruncateAlphaAerialScreenshots";
            checkBox3.Text = "Rotate Alpha site screenshots by 90° and truncate.";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(11, 226);
            label9.Name = "label9";
            label9.Size = new Size(72, 15);
            label9.TabIndex = 13;
            label9.Text = "Overlay size:";
            // 
            // comboGuardianWindowSize
            // 
            comboGuardianWindowSize.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGuardianWindowSize.FormattingEnabled = true;
            comboGuardianWindowSize.Items.AddRange(new object[] { "Small - 300 x 400", "Medium - 500 x 500", "Large - 600 x 700", "Huge - 800 x 1000", "Massive - 1200 x 1200" });
            comboGuardianWindowSize.Location = new Point(89, 223);
            comboGuardianWindowSize.Name = "comboGuardianWindowSize";
            comboGuardianWindowSize.Size = new Size(177, 23);
            comboGuardianWindowSize.TabIndex = 12;
            comboGuardianWindowSize.Tag = "idxGuardianPlotter";
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Checked = true;
            checkBox2.CheckState = CheckState.Checked;
            checkBox2.Location = new Point(10, 198);
            checkBox2.Margin = new Padding(4, 3, 4, 3);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(256, 19);
            checkBox2.TabIndex = 11;
            checkBox2.Tag = "disableAerialAlignmentGrid";
            checkBox2.Text = "Disable aerial screenshot alignment overlay.";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // numAltGamma
            // 
            numAltGamma.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numAltGamma.Location = new Point(293, 144);
            numAltGamma.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numAltGamma.Name = "numAltGamma";
            numAltGamma.Size = new Size(51, 23);
            numAltGamma.TabIndex = 10;
            numAltGamma.Tag = "aerialAltGamma";
            numAltGamma.Value = new decimal(new int[] { 1500, 0, 0, 0 });
            // 
            // numAltBeta
            // 
            numAltBeta.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numAltBeta.Location = new Point(178, 144);
            numAltBeta.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numAltBeta.Name = "numAltBeta";
            numAltBeta.Size = new Size(51, 23);
            numAltBeta.TabIndex = 9;
            numAltBeta.Tag = "aerialAltBeta";
            numAltBeta.Value = new decimal(new int[] { 1400, 0, 0, 0 });
            // 
            // numAltAlpha
            // 
            numAltAlpha.Location = new Point(82, 144);
            numAltAlpha.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numAltAlpha.Name = "numAltAlpha";
            numAltAlpha.Size = new Size(51, 23);
            numAltAlpha.TabIndex = 8;
            numAltAlpha.Tag = "aerialAltAlpha";
            numAltAlpha.Value = new decimal(new int[] { 1200, 0, 0, 0 });
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(235, 146);
            label8.Name = "label8";
            label8.Size = new Size(52, 15);
            label8.TabIndex = 7;
            label8.Text = "Gamma:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(139, 146);
            label7.Name = "label7";
            label7.Size = new Size(33, 15);
            label7.TabIndex = 6;
            label7.Tag = "";
            label7.Text = "Beta:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(35, 146);
            label6.Name = "label6";
            label6.Size = new Size(41, 15);
            label6.TabIndex = 5;
            label6.Text = "Alpha:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 111);
            label5.Name = "label5";
            label5.Size = new Size(377, 30);
            label5.TabIndex = 4;
            label5.Text = "Preferred alititude for aerial screenshots:\r\n(Adjust these so that each site comfortably fits in your size of monitor)";
            // 
            // checkRuinsMeasurementGrid
            // 
            checkRuinsMeasurementGrid.AutoSize = true;
            checkRuinsMeasurementGrid.Checked = true;
            checkRuinsMeasurementGrid.CheckState = CheckState.Checked;
            checkRuinsMeasurementGrid.Location = new Point(10, 173);
            checkRuinsMeasurementGrid.Margin = new Padding(4, 3, 4, 3);
            checkRuinsMeasurementGrid.Name = "checkRuinsMeasurementGrid";
            checkRuinsMeasurementGrid.Size = new Size(231, 19);
            checkRuinsMeasurementGrid.TabIndex = 3;
            checkRuinsMeasurementGrid.Tag = "disableRuinsMeasurementGrid";
            checkRuinsMeasurementGrid.Text = "Disable site heading assistance overlay.";
            checkRuinsMeasurementGrid.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.Location = new Point(11, 28);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(430, 83);
            label3.TabIndex = 2;
            label3.Text = "Guardian site features are still experimental and may not function properly.\r\nKnown issues:\r\n- Only some Relic Towers and puddle locations are known for Gamma Ruins.";
            // 
            // checkEnableGuardianFeatures
            // 
            checkEnableGuardianFeatures.AutoSize = true;
            checkEnableGuardianFeatures.Checked = true;
            checkEnableGuardianFeatures.CheckState = CheckState.Checked;
            checkEnableGuardianFeatures.Location = new Point(10, 6);
            checkEnableGuardianFeatures.Margin = new Padding(4, 3, 4, 3);
            checkEnableGuardianFeatures.Name = "checkEnableGuardianFeatures";
            checkEnableGuardianFeatures.Size = new Size(189, 19);
            checkEnableGuardianFeatures.TabIndex = 1;
            checkEnableGuardianFeatures.Tag = "enableGuardianSites";
            checkEnableGuardianFeatures.Text = "Enable Guardian Ruins features";
            checkEnableGuardianFeatures.UseVisualStyleBackColor = true;
            checkEnableGuardianFeatures.CheckedChanged += disableEverythingElse_CheckedChanged;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(checkDeleteScreenshotOriginal);
            tabPage5.Controls.Add(pictureBox3);
            tabPage5.Controls.Add(checkUseGuardianAerialScreenshotsFolder);
            tabPage5.Controls.Add(linkTargetScreenshotFolder);
            tabPage5.Controls.Add(btnChooseScreenshotSourceFolder);
            tabPage5.Controls.Add(btnChooseScreenshotTargetFolder);
            tabPage5.Controls.Add(lblScreenshotSource);
            tabPage5.Controls.Add(lblScreenshotTarget);
            tabPage5.Controls.Add(linkScreenshotSourceFolder);
            tabPage5.Controls.Add(checkAddBanner);
            tabPage5.Controls.Add(checkProcessScreenshots);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(452, 377);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Screenshots";
            // 
            // checkDeleteScreenshotOriginal
            // 
            checkDeleteScreenshotOriginal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            checkDeleteScreenshotOriginal.CheckAlign = ContentAlignment.TopLeft;
            checkDeleteScreenshotOriginal.Checked = true;
            checkDeleteScreenshotOriginal.CheckState = CheckState.Checked;
            checkDeleteScreenshotOriginal.Location = new Point(7, 145);
            checkDeleteScreenshotOriginal.Margin = new Padding(4, 3, 4, 3);
            checkDeleteScreenshotOriginal.Name = "checkDeleteScreenshotOriginal";
            checkDeleteScreenshotOriginal.Size = new Size(435, 19);
            checkDeleteScreenshotOriginal.TabIndex = 23;
            checkDeleteScreenshotOriginal.Tag = "deleteScreenshotOriginal";
            checkDeleteScreenshotOriginal.Text = "Remove original files after conversion";
            checkDeleteScreenshotOriginal.UseVisualStyleBackColor = true;
            // 
            // pictureBox3
            // 
            pictureBox3.BackgroundImage = (Image)resources.GetObject("pictureBox3.BackgroundImage");
            pictureBox3.BackgroundImageLayout = ImageLayout.None;
            pictureBox3.Location = new Point(65, 192);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(364, 104);
            pictureBox3.TabIndex = 22;
            pictureBox3.TabStop = false;
            // 
            // checkUseGuardianAerialScreenshotsFolder
            // 
            checkUseGuardianAerialScreenshotsFolder.BackgroundImageLayout = ImageLayout.None;
            checkUseGuardianAerialScreenshotsFolder.CheckAlign = ContentAlignment.TopLeft;
            checkUseGuardianAerialScreenshotsFolder.Checked = true;
            checkUseGuardianAerialScreenshotsFolder.CheckState = CheckState.Checked;
            checkUseGuardianAerialScreenshotsFolder.Location = new Point(7, 305);
            checkUseGuardianAerialScreenshotsFolder.Margin = new Padding(4, 3, 4, 3);
            checkUseGuardianAerialScreenshotsFolder.Name = "checkUseGuardianAerialScreenshotsFolder";
            checkUseGuardianAerialScreenshotsFolder.Size = new Size(419, 66);
            checkUseGuardianAerialScreenshotsFolder.TabIndex = 21;
            checkUseGuardianAerialScreenshotsFolder.Tag = "useGuardianAerialScreenshotsFolder";
            checkUseGuardianAerialScreenshotsFolder.Text = resources.GetString("checkUseGuardianAerialScreenshotsFolder.Text");
            checkUseGuardianAerialScreenshotsFolder.UseVisualStyleBackColor = true;
            // 
            // linkTargetScreenshotFolder
            // 
            linkTargetScreenshotFolder.BorderStyle = BorderStyle.FixedSingle;
            linkTargetScreenshotFolder.Location = new Point(42, 104);
            linkTargetScreenshotFolder.Name = "linkTargetScreenshotFolder";
            linkTargetScreenshotFolder.Size = new Size(387, 35);
            linkTargetScreenshotFolder.TabIndex = 20;
            linkTargetScreenshotFolder.TabStop = true;
            linkTargetScreenshotFolder.Tag = "screenshotTargetFolder";
            linkTargetScreenshotFolder.Text = "C:\\xxx\\Pictures\\Frontier Developments\\Elite Dangerous\\";
            linkTargetScreenshotFolder.LinkClicked += linkScreenshotFolder_LinkClicked;
            // 
            // btnChooseScreenshotSourceFolder
            // 
            btnChooseScreenshotSourceFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChooseScreenshotSourceFolder.Location = new Point(10, 47);
            btnChooseScreenshotSourceFolder.Name = "btnChooseScreenshotSourceFolder";
            btnChooseScreenshotSourceFolder.Size = new Size(26, 35);
            btnChooseScreenshotSourceFolder.TabIndex = 19;
            btnChooseScreenshotSourceFolder.Text = "...";
            btnChooseScreenshotSourceFolder.UseVisualStyleBackColor = true;
            btnChooseScreenshotSourceFolder.Click += btnChooseScreenshotSourceFolder_Click;
            // 
            // btnChooseScreenshotTargetFolder
            // 
            btnChooseScreenshotTargetFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChooseScreenshotTargetFolder.Location = new Point(10, 104);
            btnChooseScreenshotTargetFolder.Name = "btnChooseScreenshotTargetFolder";
            btnChooseScreenshotTargetFolder.Size = new Size(26, 35);
            btnChooseScreenshotTargetFolder.TabIndex = 18;
            btnChooseScreenshotTargetFolder.Text = "...";
            btnChooseScreenshotTargetFolder.UseVisualStyleBackColor = true;
            btnChooseScreenshotTargetFolder.Click += btnChooseScreenshotTargetFolder_Click;
            // 
            // lblScreenshotSource
            // 
            lblScreenshotSource.AutoSize = true;
            lblScreenshotSource.Location = new Point(7, 31);
            lblScreenshotSource.Name = "lblScreenshotSource";
            lblScreenshotSource.Size = new Size(164, 15);
            lblScreenshotSource.TabIndex = 17;
            lblScreenshotSource.Text = "Read screenshots from folder:";
            // 
            // lblScreenshotTarget
            // 
            lblScreenshotTarget.AutoSize = true;
            lblScreenshotTarget.Location = new Point(10, 86);
            lblScreenshotTarget.Name = "lblScreenshotTarget";
            lblScreenshotTarget.Size = new Size(202, 15);
            lblScreenshotTarget.TabIndex = 16;
            lblScreenshotTarget.Text = "Save converted screenshots in folder:";
            // 
            // linkScreenshotSourceFolder
            // 
            linkScreenshotSourceFolder.BorderStyle = BorderStyle.FixedSingle;
            linkScreenshotSourceFolder.Location = new Point(41, 47);
            linkScreenshotSourceFolder.Name = "linkScreenshotSourceFolder";
            linkScreenshotSourceFolder.Size = new Size(387, 35);
            linkScreenshotSourceFolder.TabIndex = 15;
            linkScreenshotSourceFolder.TabStop = true;
            linkScreenshotSourceFolder.Tag = "screenshotSourceFolder";
            linkScreenshotSourceFolder.Text = "C:\\xxx\\Pictures\\Frontier Developments\\Elite Dangerous\\";
            linkScreenshotSourceFolder.LinkClicked += linkScreenshotFolder_LinkClicked;
            // 
            // checkAddBanner
            // 
            checkAddBanner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            checkAddBanner.CheckAlign = ContentAlignment.TopLeft;
            checkAddBanner.Checked = true;
            checkAddBanner.CheckState = CheckState.Checked;
            checkAddBanner.Location = new Point(7, 170);
            checkAddBanner.Margin = new Padding(4, 3, 4, 3);
            checkAddBanner.Name = "checkAddBanner";
            checkAddBanner.Size = new Size(435, 19);
            checkAddBanner.TabIndex = 14;
            checkAddBanner.Tag = "addBannerToScreenshots";
            checkAddBanner.Text = "Embed locations details within image, if known. Eg:";
            checkAddBanner.UseVisualStyleBackColor = true;
            // 
            // checkProcessScreenshots
            // 
            checkProcessScreenshots.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            checkProcessScreenshots.CheckAlign = ContentAlignment.TopLeft;
            checkProcessScreenshots.Checked = true;
            checkProcessScreenshots.CheckState = CheckState.Checked;
            checkProcessScreenshots.Location = new Point(7, 6);
            checkProcessScreenshots.Margin = new Padding(4, 3, 4, 3);
            checkProcessScreenshots.Name = "checkProcessScreenshots";
            checkProcessScreenshots.Size = new Size(435, 19);
            checkProcessScreenshots.TabIndex = 13;
            checkProcessScreenshots.Tag = "processScreenshots";
            checkProcessScreenshots.Text = "Convert .bmp screenshots into .png files";
            checkProcessScreenshots.UseVisualStyleBackColor = true;
            checkProcessScreenshots.CheckedChanged += disableEverythingElse_CheckedChanged;
            // 
            // tabPage6
            // 
            tabPage6.BackColor = SystemColors.Control;
            tabPage6.Controls.Add(label10);
            tabPage6.Controls.Add(checkBox11);
            tabPage6.Controls.Add(numMinScanValue);
            tabPage6.Controls.Add(checkBox10);
            tabPage6.Controls.Add(checkBox9);
            tabPage6.Controls.Add(pictureBox5);
            tabPage6.Controls.Add(checkBox8);
            tabPage6.Controls.Add(pictureBox4);
            tabPage6.Controls.Add(checkBox4);
            tabPage6.Location = new Point(4, 24);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3);
            tabPage6.Size = new Size(452, 377);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "Exploration";
            // 
            // checkBox11
            // 
            checkBox11.AutoSize = true;
            checkBox11.Checked = true;
            checkBox11.CheckState = CheckState.Checked;
            checkBox11.Location = new Point(31, 184);
            checkBox11.Name = "checkBox11";
            checkBox11.Size = new Size(236, 19);
            checkBox11.TabIndex = 19;
            checkBox11.Tag = "skipLowValueDSS";
            checkBox11.Text = "Skip bodies with estimated value below:";
            checkBox11.UseVisualStyleBackColor = true;
            // 
            // numMinScanValue
            // 
            numMinScanValue.Increment = new decimal(new int[] { 100000, 0, 0, 0 });
            numMinScanValue.Location = new Point(281, 183);
            numMinScanValue.Maximum = new decimal(new int[] { 6000000, 0, 0, 0 });
            numMinScanValue.Name = "numMinScanValue";
            numMinScanValue.Size = new Size(88, 23);
            numMinScanValue.TabIndex = 18;
            numMinScanValue.Tag = "skipLowValueAmount";
            numMinScanValue.TextAlign = HorizontalAlignment.Right;
            numMinScanValue.ThousandsSeparator = true;
            numMinScanValue.Value = new decimal(new int[] { 2000000, 0, 0, 0 });
            // 
            // checkBox10
            // 
            checkBox10.AutoSize = true;
            checkBox10.Checked = true;
            checkBox10.CheckState = CheckState.Checked;
            checkBox10.Location = new Point(31, 159);
            checkBox10.Margin = new Padding(4, 3, 4, 3);
            checkBox10.Name = "checkBox10";
            checkBox10.Size = new Size(114, 19);
            checkBox10.TabIndex = 16;
            checkBox10.Tag = "skipRingsDSS";
            checkBox10.Text = "Skip DSS of rings";
            checkBox10.UseVisualStyleBackColor = true;
            // 
            // checkBox9
            // 
            checkBox9.AutoSize = true;
            checkBox9.Checked = true;
            checkBox9.CheckState = CheckState.Checked;
            checkBox9.Location = new Point(31, 134);
            checkBox9.Margin = new Padding(4, 3, 4, 3);
            checkBox9.Name = "checkBox9";
            checkBox9.Size = new Size(141, 19);
            checkBox9.TabIndex = 15;
            checkBox9.Tag = "skipGasGiantDSS";
            checkBox9.Text = "Skip DSS of gas giants";
            checkBox9.UseVisualStyleBackColor = true;
            // 
            // pictureBox5
            // 
            pictureBox5.BackgroundImage = (Image)resources.GetObject("pictureBox5.BackgroundImage");
            pictureBox5.BackgroundImageLayout = ImageLayout.None;
            pictureBox5.Location = new Point(193, 129);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(256, 49);
            pictureBox5.TabIndex = 14;
            pictureBox5.TabStop = false;
            // 
            // checkBox8
            // 
            checkBox8.AutoSize = true;
            checkBox8.Checked = true;
            checkBox8.CheckState = CheckState.Checked;
            checkBox8.Location = new Point(10, 109);
            checkBox8.Margin = new Padding(4, 3, 4, 3);
            checkBox8.Name = "checkBox8";
            checkBox8.Size = new Size(339, 19);
            checkBox8.TabIndex = 13;
            checkBox8.Tag = "autoShowPlotSysStatus";
            checkBox8.Text = "Show system scans, FSS, body DSS and bio scans remaining";
            checkBox8.UseVisualStyleBackColor = true;
            // 
            // pictureBox4
            // 
            pictureBox4.BackgroundImage = (Image)resources.GetObject("pictureBox4.BackgroundImage");
            pictureBox4.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox4.Location = new Point(212, 6);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(198, 84);
            pictureBox4.TabIndex = 12;
            pictureBox4.TabStop = false;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Checked = true;
            checkBox4.CheckState = CheckState.Checked;
            checkBox4.Location = new Point(10, 6);
            checkBox4.Margin = new Padding(4, 3, 4, 3);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new Size(188, 19);
            checkBox4.TabIndex = 11;
            checkBox4.Tag = "autoShowPlotFSS";
            checkBox4.Text = "Show exploration values in FSS";
            checkBox4.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.BorderStyle = BorderStyle.Fixed3D;
            tabPage2.Controls.Add(linkAboutTwo);
            tabPage2.Controls.Add(linkAboutOne);
            tabPage2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(452, 377);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "About";
            // 
            // linkAboutTwo
            // 
            linkAboutTwo.Dock = DockStyle.Bottom;
            linkAboutTwo.LinkArea = new LinkArea(83, 21);
            linkAboutTwo.Location = new Point(3, 310);
            linkAboutTwo.Name = "linkAboutTwo";
            linkAboutTwo.Size = new Size(442, 60);
            linkAboutTwo.TabIndex = 2;
            linkAboutTwo.TabStop = true;
            linkAboutTwo.Text = "SrvSurvey is not an official tool for \"Elite Dangerous\" and is not affiliated with Frontier Developments. All trademarks and copyright are acknowledged as the property of their respective owners.\r\n";
            linkAboutTwo.TextAlign = ContentAlignment.BottomCenter;
            linkAboutTwo.UseCompatibleTextRendering = true;
            // 
            // linkAboutOne
            // 
            linkAboutOne.Dock = DockStyle.Top;
            linkAboutOne.LinkArea = new LinkArea(65, 15);
            linkAboutOne.Location = new Point(3, 3);
            linkAboutOne.Name = "linkAboutOne";
            linkAboutOne.Size = new Size(442, 286);
            linkAboutOne.TabIndex = 1;
            linkAboutOne.TabStop = true;
            linkAboutOne.Text = resources.GetString("linkAboutOne.Text");
            linkAboutOne.UseCompatibleTextRendering = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(375, 185);
            label10.Name = "label10";
            label10.Size = new Size(42, 15);
            label10.TabIndex = 20;
            label10.Text = "credits";
            // 
            // FormSettings
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(460, 453);
            Controls.Add(tabControl1);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormSettings";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "SrvSurvey Settings";
            Load += FormSettings_Load;
            panel1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOpacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackOpacity).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numAltGamma).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAltBeta).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAltAlpha).EndInit();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            tabPage6.ResumeLayout(false);
            tabPage6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMinScanValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private Button btnSave;
        private Button btnCancel;
        private TabPage tabPage1;
        private NumericUpDown numOpacity;
        private Label label2;
        private TrackBar trackOpacity;
        private TextBox txtCommander;
        private Label label1;
        private TabControl tabControl1;
        private CheckBox checkFocusOnMinimize;
        private TabPage tabPage2;
        private LinkLabel linkAboutOne;
        private LinkLabel linkAboutTwo;
        private TabPage tabPage3;
        private Label label3;
        private CheckBox checkEnableGuardianFeatures;
        private Button btnNextProc;
        private CheckBox checkRuinsMeasurementGrid;
        private CheckBox checkHidePlottersFromWeapons;
        private TabPage tabPage4;
        private Button btnClearUnclaimed;
        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private CheckBox checkBox1;
        private CheckBox checkBioStatusAutoShow;
        private TabPage tabPage5;
        private CheckBox checkUseGuardianAerialScreenshotsFolder;
        private LinkLabel linkTargetScreenshotFolder;
        private Button btnChooseScreenshotSourceFolder;
        private Button btnChooseScreenshotTargetFolder;
        private Label lblScreenshotSource;
        private Label lblScreenshotTarget;
        private LinkLabel linkScreenshotSourceFolder;
        private CheckBox checkAddBanner;
        private CheckBox checkProcessScreenshots;
        private PictureBox pictureBox3;
        private CheckBox checkDeleteScreenshotOriginal;
        private CheckBox checkHideOverlayOnMouseOver;
        private Label label4;
        private CheckBox checkMinimizeOnStart;
        private LinkLabel linkDataFiles;
        private NumericUpDown numAltGamma;
        private NumericUpDown numAltBeta;
        private NumericUpDown numAltAlpha;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private CheckBox checkBox2;
        private Label label9;
        private ComboBox comboGuardianWindowSize;
        private CheckBox checkBox3;
        private CheckBox checkBox5;
        private CheckBox checkBox6;
        private TabPage tabPage6;
        private PictureBox pictureBox4;
        private CheckBox checkBox4;
        private CheckBox checkBox7;
        private PictureBox pictureBox5;
        private CheckBox checkBox8;
        private CheckBox checkBox9;
        private CheckBox checkBox10;
        private CheckBox checkBox11;
        private NumericUpDown numMinScanValue;
        private Label label10;
    }
}