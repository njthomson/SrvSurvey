
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnNextProc = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.checkMinimizeOnStart = new System.Windows.Forms.CheckBox();
            this.checkHideOverlayOnMouseOver = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkHidePlottersFromWeapons = new System.Windows.Forms.CheckBox();
            this.checkFocusOnMinimize = new System.Windows.Forms.CheckBox();
            this.numOpacity = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.trackOpacity = new System.Windows.Forms.TrackBar();
            this.txtCommander = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnClearUnclaimed = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBioStatusAutoShow = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.checkRuinsMeasurementGrid = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkEnableGuardianFeatures = new System.Windows.Forms.CheckBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.checkDeleteScreenshotOriginal = new System.Windows.Forms.CheckBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.checkUseGuardianAerialScreenshotsFolder = new System.Windows.Forms.CheckBox();
            this.linkTargetScreenshotFolder = new System.Windows.Forms.LinkLabel();
            this.btnChooseScreenshotSourceFolder = new System.Windows.Forms.Button();
            this.btnChooseScreenshotTargetFolder = new System.Windows.Forms.Button();
            this.lblScreenshotSource = new System.Windows.Forms.Label();
            this.lblScreenshotTarget = new System.Windows.Forms.Label();
            this.linkScreenshotSourceFolder = new System.Windows.Forms.LinkLabel();
            this.checkAddBanner = new System.Windows.Forms.CheckBox();
            this.checkProcessScreenshots = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.linkAboutTwo = new System.Windows.Forms.LinkLabel();
            this.linkAboutOne = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnNextProc);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 405);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(460, 48);
            this.panel1.TabIndex = 1;
            // 
            // btnNextProc
            // 
            this.btnNextProc.Location = new System.Drawing.Point(4, 16);
            this.btnNextProc.Name = "btnNextProc";
            this.btnNextProc.Size = new System.Drawing.Size(243, 23);
            this.btnNextProc.TabIndex = 2;
            this.btnNextProc.Text = "Use alternate EliteDangerous window";
            this.btnNextProc.UseVisualStyleBackColor = true;
            this.btnNextProc.Visible = false;
            this.btnNextProc.Click += new System.EventHandler(this.btnNextProc_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(264, 14);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 27);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(358, 14);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 27);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage1.Controls.Add(this.checkMinimizeOnStart);
            this.tabPage1.Controls.Add(this.checkHideOverlayOnMouseOver);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.checkHidePlottersFromWeapons);
            this.tabPage1.Controls.Add(this.checkFocusOnMinimize);
            this.tabPage1.Controls.Add(this.numOpacity);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.trackOpacity);
            this.tabPage1.Controls.Add(this.txtCommander);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Size = new System.Drawing.Size(452, 377);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            // 
            // checkMinimizeOnStart
            // 
            this.checkMinimizeOnStart.Checked = true;
            this.checkMinimizeOnStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMinimizeOnStart.Location = new System.Drawing.Point(8, 108);
            this.checkMinimizeOnStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkMinimizeOnStart.Name = "checkMinimizeOnStart";
            this.checkMinimizeOnStart.Size = new System.Drawing.Size(431, 19);
            this.checkMinimizeOnStart.TabIndex = 10;
            this.checkMinimizeOnStart.Tag = "focusGameOnStart";
            this.checkMinimizeOnStart.Text = "Set focus on Elite Dangerous when starting Srv Survey.";
            this.checkMinimizeOnStart.UseVisualStyleBackColor = true;
            // 
            // checkHideOverlayOnMouseOver
            // 
            this.checkHideOverlayOnMouseOver.AutoSize = true;
            this.checkHideOverlayOnMouseOver.Location = new System.Drawing.Point(7, 348);
            this.checkHideOverlayOnMouseOver.Name = "checkHideOverlayOnMouseOver";
            this.checkHideOverlayOnMouseOver.Size = new System.Drawing.Size(246, 19);
            this.checkHideOverlayOnMouseOver.TabIndex = 9;
            this.checkHideOverlayOnMouseOver.Tag = "hideOverlaysFromMouse";
            this.checkHideOverlayOnMouseOver.Text = "Prevent mouse entering overlay windows.";
            this.checkHideOverlayOnMouseOver.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 286);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(427, 34);
            this.label4.TabIndex = 8;
            this.label4.Text = "Players using mouse and keyboard have reported some issues with overlay windows t" +
    "rapping focus from the game.";
            // 
            // checkHidePlottersFromWeapons
            // 
            this.checkHidePlottersFromWeapons.Checked = true;
            this.checkHidePlottersFromWeapons.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkHidePlottersFromWeapons.Location = new System.Drawing.Point(7, 323);
            this.checkHidePlottersFromWeapons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkHidePlottersFromWeapons.Name = "checkHidePlottersFromWeapons";
            this.checkHidePlottersFromWeapons.Size = new System.Drawing.Size(431, 19);
            this.checkHidePlottersFromWeapons.TabIndex = 7;
            this.checkHidePlottersFromWeapons.Tag = "hidePlottersFromCombatSuits";
            this.checkHidePlottersFromWeapons.Text = "Disable overlays when on foot in Maverick and Dominator suits.";
            this.checkHidePlottersFromWeapons.UseVisualStyleBackColor = true;
            // 
            // checkFocusOnMinimize
            // 
            this.checkFocusOnMinimize.Checked = true;
            this.checkFocusOnMinimize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkFocusOnMinimize.Location = new System.Drawing.Point(8, 133);
            this.checkFocusOnMinimize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkFocusOnMinimize.Name = "checkFocusOnMinimize";
            this.checkFocusOnMinimize.Size = new System.Drawing.Size(431, 19);
            this.checkFocusOnMinimize.TabIndex = 2;
            this.checkFocusOnMinimize.Tag = "focusGameOnMinimize";
            this.checkFocusOnMinimize.Text = "Set focus on Elite Dangerous when minimizing Srv Survey.";
            this.checkFocusOnMinimize.UseVisualStyleBackColor = true;
            // 
            // numOpacity
            // 
            this.numOpacity.Location = new System.Drawing.Point(109, 68);
            this.numOpacity.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numOpacity.Name = "numOpacity";
            this.numOpacity.Size = new System.Drawing.Size(61, 23);
            this.numOpacity.TabIndex = 6;
            this.numOpacity.Tag = "Opacity";
            this.numOpacity.ValueChanged += new System.EventHandler(this.numOpacity_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 70);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Overlay opacity:";
            // 
            // trackOpacity
            // 
            this.trackOpacity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackOpacity.LargeChange = 10;
            this.trackOpacity.Location = new System.Drawing.Point(178, 68);
            this.trackOpacity.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.trackOpacity.Maximum = 100;
            this.trackOpacity.Name = "trackOpacity";
            this.trackOpacity.Size = new System.Drawing.Size(258, 45);
            this.trackOpacity.SmallChange = 5;
            this.trackOpacity.TabIndex = 4;
            this.trackOpacity.TickFrequency = 10;
            this.trackOpacity.Scroll += new System.EventHandler(this.trackOpacity_Scroll);
            // 
            // txtCommander
            // 
            this.txtCommander.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommander.Location = new System.Drawing.Point(8, 33);
            this.txtCommander.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtCommander.Name = "txtCommander";
            this.txtCommander.Size = new System.Drawing.Size(432, 23);
            this.txtCommander.TabIndex = 1;
            this.txtCommander.Tag = "preferredCommander";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(430, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Preferred Commander: (others will be ignored)";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(460, 405);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnClearUnclaimed);
            this.tabPage4.Controls.Add(this.pictureBox2);
            this.tabPage4.Controls.Add(this.pictureBox1);
            this.tabPage4.Controls.Add(this.checkBox1);
            this.tabPage4.Controls.Add(this.checkBioStatusAutoShow);
            this.tabPage4.Location = new System.Drawing.Point(4, 24);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(452, 377);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Bio Scanning";
            // 
            // btnClearUnclaimed
            // 
            this.btnClearUnclaimed.Location = new System.Drawing.Point(10, 348);
            this.btnClearUnclaimed.Name = "btnClearUnclaimed";
            this.btnClearUnclaimed.Size = new System.Drawing.Size(154, 23);
            this.btnClearUnclaimed.TabIndex = 6;
            this.btnClearUnclaimed.Text = "Clear unclaimed rewards";
            this.btnClearUnclaimed.UseVisualStyleBackColor = true;
            this.btnClearUnclaimed.Click += new System.EventHandler(this.btnClearUnclaimed_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.BackgroundImage")));
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox2.Location = new System.Drawing.Point(226, 67);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(109, 132);
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(212, 11);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(154, 50);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(10, 67);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(209, 19);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Tag = "autoShowBioPlot";
            this.checkBox1.Text = "Show sample scan exclusion zones";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBioStatusAutoShow
            // 
            this.checkBioStatusAutoShow.AutoSize = true;
            this.checkBioStatusAutoShow.Checked = true;
            this.checkBioStatusAutoShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBioStatusAutoShow.Location = new System.Drawing.Point(10, 11);
            this.checkBioStatusAutoShow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBioStatusAutoShow.Name = "checkBioStatusAutoShow";
            this.checkBioStatusAutoShow.Size = new System.Drawing.Size(197, 19);
            this.checkBioStatusAutoShow.TabIndex = 4;
            this.checkBioStatusAutoShow.Tag = "autoShowBioSummary";
            this.checkBioStatusAutoShow.Text = "Show biological signal summary";
            this.checkBioStatusAutoShow.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage3.Controls.Add(this.checkRuinsMeasurementGrid);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.checkEnableGuardianFeatures);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(452, 377);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Guardian sites";
            // 
            // checkRuinsMeasurementGrid
            // 
            this.checkRuinsMeasurementGrid.AutoSize = true;
            this.checkRuinsMeasurementGrid.Checked = true;
            this.checkRuinsMeasurementGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRuinsMeasurementGrid.Location = new System.Drawing.Point(10, 64);
            this.checkRuinsMeasurementGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkRuinsMeasurementGrid.Name = "checkRuinsMeasurementGrid";
            this.checkRuinsMeasurementGrid.Size = new System.Drawing.Size(231, 19);
            this.checkRuinsMeasurementGrid.TabIndex = 3;
            this.checkRuinsMeasurementGrid.Tag = "disableRuinsMeasurementGrid";
            this.checkRuinsMeasurementGrid.Text = "Disable site heading measurement grid";
            this.checkRuinsMeasurementGrid.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 28);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(430, 33);
            this.label3.TabIndex = 2;
            this.label3.Text = "Guardian site features are still extremely experimental and likely to have issues" +
    ".";
            // 
            // checkEnableGuardianFeatures
            // 
            this.checkEnableGuardianFeatures.AutoSize = true;
            this.checkEnableGuardianFeatures.Checked = true;
            this.checkEnableGuardianFeatures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEnableGuardianFeatures.Location = new System.Drawing.Point(10, 6);
            this.checkEnableGuardianFeatures.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkEnableGuardianFeatures.Name = "checkEnableGuardianFeatures";
            this.checkEnableGuardianFeatures.Size = new System.Drawing.Size(178, 19);
            this.checkEnableGuardianFeatures.TabIndex = 1;
            this.checkEnableGuardianFeatures.Tag = "enableGuardianSites";
            this.checkEnableGuardianFeatures.Text = "Enable experimental features";
            this.checkEnableGuardianFeatures.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.checkDeleteScreenshotOriginal);
            this.tabPage5.Controls.Add(this.pictureBox3);
            this.tabPage5.Controls.Add(this.checkUseGuardianAerialScreenshotsFolder);
            this.tabPage5.Controls.Add(this.linkTargetScreenshotFolder);
            this.tabPage5.Controls.Add(this.btnChooseScreenshotSourceFolder);
            this.tabPage5.Controls.Add(this.btnChooseScreenshotTargetFolder);
            this.tabPage5.Controls.Add(this.lblScreenshotSource);
            this.tabPage5.Controls.Add(this.lblScreenshotTarget);
            this.tabPage5.Controls.Add(this.linkScreenshotSourceFolder);
            this.tabPage5.Controls.Add(this.checkAddBanner);
            this.tabPage5.Controls.Add(this.checkProcessScreenshots);
            this.tabPage5.Location = new System.Drawing.Point(4, 24);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(452, 377);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Screenshots";
            // 
            // checkDeleteScreenshotOriginal
            // 
            this.checkDeleteScreenshotOriginal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkDeleteScreenshotOriginal.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkDeleteScreenshotOriginal.Checked = true;
            this.checkDeleteScreenshotOriginal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDeleteScreenshotOriginal.Location = new System.Drawing.Point(7, 145);
            this.checkDeleteScreenshotOriginal.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkDeleteScreenshotOriginal.Name = "checkDeleteScreenshotOriginal";
            this.checkDeleteScreenshotOriginal.Size = new System.Drawing.Size(435, 19);
            this.checkDeleteScreenshotOriginal.TabIndex = 23;
            this.checkDeleteScreenshotOriginal.Tag = "deleteScreenshotOriginal";
            this.checkDeleteScreenshotOriginal.Text = "Remove original files after conversion";
            this.checkDeleteScreenshotOriginal.UseVisualStyleBackColor = true;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox3.BackgroundImage")));
            this.pictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox3.Location = new System.Drawing.Point(65, 192);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(364, 104);
            this.pictureBox3.TabIndex = 22;
            this.pictureBox3.TabStop = false;
            // 
            // checkUseGuardianAerialScreenshotsFolder
            // 
            this.checkUseGuardianAerialScreenshotsFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.checkUseGuardianAerialScreenshotsFolder.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkUseGuardianAerialScreenshotsFolder.Checked = true;
            this.checkUseGuardianAerialScreenshotsFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkUseGuardianAerialScreenshotsFolder.Location = new System.Drawing.Point(7, 305);
            this.checkUseGuardianAerialScreenshotsFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkUseGuardianAerialScreenshotsFolder.Name = "checkUseGuardianAerialScreenshotsFolder";
            this.checkUseGuardianAerialScreenshotsFolder.Size = new System.Drawing.Size(419, 66);
            this.checkUseGuardianAerialScreenshotsFolder.TabIndex = 21;
            this.checkUseGuardianAerialScreenshotsFolder.Tag = "useGuardianAerialScreenshotsFolder";
            this.checkUseGuardianAerialScreenshotsFolder.Text = resources.GetString("checkUseGuardianAerialScreenshotsFolder.Text");
            this.checkUseGuardianAerialScreenshotsFolder.UseVisualStyleBackColor = true;
            // 
            // linkTargetScreenshotFolder
            // 
            this.linkTargetScreenshotFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.linkTargetScreenshotFolder.Location = new System.Drawing.Point(42, 104);
            this.linkTargetScreenshotFolder.Name = "linkTargetScreenshotFolder";
            this.linkTargetScreenshotFolder.Size = new System.Drawing.Size(387, 35);
            this.linkTargetScreenshotFolder.TabIndex = 20;
            this.linkTargetScreenshotFolder.TabStop = true;
            this.linkTargetScreenshotFolder.Tag = "screenshotTargetFolder";
            this.linkTargetScreenshotFolder.Text = "C:\\xxx\\Pictures\\Frontier Developments\\Elite Dangerous\\";
            this.linkTargetScreenshotFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkScreenshotFolder_LinkClicked);
            // 
            // btnChooseScreenshotSourceFolder
            // 
            this.btnChooseScreenshotSourceFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseScreenshotSourceFolder.Location = new System.Drawing.Point(10, 47);
            this.btnChooseScreenshotSourceFolder.Name = "btnChooseScreenshotSourceFolder";
            this.btnChooseScreenshotSourceFolder.Size = new System.Drawing.Size(26, 35);
            this.btnChooseScreenshotSourceFolder.TabIndex = 19;
            this.btnChooseScreenshotSourceFolder.Text = "...";
            this.btnChooseScreenshotSourceFolder.UseVisualStyleBackColor = true;
            this.btnChooseScreenshotSourceFolder.Click += new System.EventHandler(this.btnChooseScreenshotSourceFolder_Click);
            // 
            // btnChooseScreenshotTargetFolder
            // 
            this.btnChooseScreenshotTargetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseScreenshotTargetFolder.Location = new System.Drawing.Point(10, 104);
            this.btnChooseScreenshotTargetFolder.Name = "btnChooseScreenshotTargetFolder";
            this.btnChooseScreenshotTargetFolder.Size = new System.Drawing.Size(26, 35);
            this.btnChooseScreenshotTargetFolder.TabIndex = 18;
            this.btnChooseScreenshotTargetFolder.Text = "...";
            this.btnChooseScreenshotTargetFolder.UseVisualStyleBackColor = true;
            this.btnChooseScreenshotTargetFolder.Click += new System.EventHandler(this.btnChooseScreenshotTargetFolder_Click);
            // 
            // lblScreenshotSource
            // 
            this.lblScreenshotSource.AutoSize = true;
            this.lblScreenshotSource.Location = new System.Drawing.Point(7, 31);
            this.lblScreenshotSource.Name = "lblScreenshotSource";
            this.lblScreenshotSource.Size = new System.Drawing.Size(164, 15);
            this.lblScreenshotSource.TabIndex = 17;
            this.lblScreenshotSource.Text = "Read screenshots from folder:";
            // 
            // lblScreenshotTarget
            // 
            this.lblScreenshotTarget.AutoSize = true;
            this.lblScreenshotTarget.Location = new System.Drawing.Point(10, 86);
            this.lblScreenshotTarget.Name = "lblScreenshotTarget";
            this.lblScreenshotTarget.Size = new System.Drawing.Size(202, 15);
            this.lblScreenshotTarget.TabIndex = 16;
            this.lblScreenshotTarget.Text = "Save converted screenshots in folder:";
            // 
            // linkScreenshotSourceFolder
            // 
            this.linkScreenshotSourceFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.linkScreenshotSourceFolder.Location = new System.Drawing.Point(41, 47);
            this.linkScreenshotSourceFolder.Name = "linkScreenshotSourceFolder";
            this.linkScreenshotSourceFolder.Size = new System.Drawing.Size(387, 35);
            this.linkScreenshotSourceFolder.TabIndex = 15;
            this.linkScreenshotSourceFolder.TabStop = true;
            this.linkScreenshotSourceFolder.Tag = "screenshotSourceFolder";
            this.linkScreenshotSourceFolder.Text = "C:\\xxx\\Pictures\\Frontier Developments\\Elite Dangerous\\";
            this.linkScreenshotSourceFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkScreenshotFolder_LinkClicked);
            // 
            // checkAddBanner
            // 
            this.checkAddBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkAddBanner.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkAddBanner.Checked = true;
            this.checkAddBanner.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAddBanner.Location = new System.Drawing.Point(7, 170);
            this.checkAddBanner.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkAddBanner.Name = "checkAddBanner";
            this.checkAddBanner.Size = new System.Drawing.Size(435, 19);
            this.checkAddBanner.TabIndex = 14;
            this.checkAddBanner.Tag = "addBannerToScreenshots";
            this.checkAddBanner.Text = "Embed locations details within image, if known. Eg:";
            this.checkAddBanner.UseVisualStyleBackColor = true;
            // 
            // checkProcessScreenshots
            // 
            this.checkProcessScreenshots.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkProcessScreenshots.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkProcessScreenshots.Checked = true;
            this.checkProcessScreenshots.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkProcessScreenshots.Location = new System.Drawing.Point(7, 6);
            this.checkProcessScreenshots.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkProcessScreenshots.Name = "checkProcessScreenshots";
            this.checkProcessScreenshots.Size = new System.Drawing.Size(435, 19);
            this.checkProcessScreenshots.TabIndex = 13;
            this.checkProcessScreenshots.Tag = "processScreenshots";
            this.checkProcessScreenshots.Text = "Convert .bmp screenshots into .png files";
            this.checkProcessScreenshots.UseVisualStyleBackColor = true;
            this.checkProcessScreenshots.CheckedChanged += new System.EventHandler(this.checkProcessSAcreenshots_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage2.Controls.Add(this.linkAboutTwo);
            this.tabPage2.Controls.Add(this.linkAboutOne);
            this.tabPage2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(452, 377);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "About";
            // 
            // linkAboutTwo
            // 
            this.linkAboutTwo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkAboutTwo.LinkArea = new System.Windows.Forms.LinkArea(83, 21);
            this.linkAboutTwo.Location = new System.Drawing.Point(3, 310);
            this.linkAboutTwo.Name = "linkAboutTwo";
            this.linkAboutTwo.Size = new System.Drawing.Size(442, 60);
            this.linkAboutTwo.TabIndex = 2;
            this.linkAboutTwo.TabStop = true;
            this.linkAboutTwo.Text = "SrvSurvey is not an official tool for \"Elite Dangerous\" and is not affiliated wit" +
    "h Frontier Developments. All trademarks and copyright are acknowledged as the pr" +
    "operty of their respective owners.\r\n";
            this.linkAboutTwo.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.linkAboutTwo.UseCompatibleTextRendering = true;
            // 
            // linkAboutOne
            // 
            this.linkAboutOne.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkAboutOne.LinkArea = new System.Windows.Forms.LinkArea(65, 15);
            this.linkAboutOne.Location = new System.Drawing.Point(3, 3);
            this.linkAboutOne.Name = "linkAboutOne";
            this.linkAboutOne.Size = new System.Drawing.Size(442, 286);
            this.linkAboutOne.TabIndex = 1;
            this.linkAboutOne.TabStop = true;
            this.linkAboutOne.Text = resources.GetString("linkAboutOne.Text");
            this.linkAboutOne.UseCompatibleTextRendering = true;
            // 
            // FormSettings
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(460, 453);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SrvSurvey Settings";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.panel1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOpacity)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.NumericUpDown numOpacity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackOpacity;
        private System.Windows.Forms.TextBox txtCommander;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.CheckBox checkFocusOnMinimize;
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
    }
}