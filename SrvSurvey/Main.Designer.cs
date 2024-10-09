
namespace SrvSurvey
{
    partial class Main
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            btnGroundTarget = new Button();
            btnQuit2 = new Button();
            txtTargetLatLong = new TextBox();
            label2 = new Label();
            btnClearTarget = new Button();
            groupBox1 = new GroupBox();
            btnPasteLatLong = new Button();
            lblTrackTargetStatus = new Label();
            groupBox2 = new GroupBox();
            btnCopyLocation = new Button();
            txtCommander = new TextBox();
            txtNearBody = new TextBox();
            txtMode = new TextBox();
            txtLocation = new TextBox();
            txtVehicle = new TextBox();
            groupBox3 = new GroupBox();
            btnBioSummary = new Button();
            checkFirstFootFall = new CheckBox();
            txtBodyBioValues = new TextBox();
            txtBodyBioSignals = new TextBox();
            lblSysBio = new Label();
            txtSystemBioValues = new TextBox();
            txtSystemBioSignals = new TextBox();
            labelSignalsAndRewards = new Label();
            lblBodyBio = new Label();
            txtBioRewards = new TextBox();
            label4 = new Label();
            btnCodexShow = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            btnLogs = new Button();
            btnSettings = new Button();
            linkLabel1 = new LinkLabel();
            lblFullScreen = new Label();
            groupBox4 = new GroupBox();
            btnGuardianThings = new Button();
            btnRuins = new Button();
            btnRuinsMap = new Button();
            btnRuinsOrigin = new Button();
            txtGuardianSite = new TextBox();
            lblGuardianCount = new Label();
            label5 = new Label();
            lblNotInstalled = new LinkLabel();
            btnSphereLimit = new Button();
            linkLabel2 = new LinkLabel();
            btnRamTah = new Button();
            btnPublishGuardian = new Button();
            checkTempHide = new CheckBox();
            groupBox5 = new GroupBox();
            txtDistance = new TextBox();
            txtJumps = new TextBox();
            label6 = new Label();
            label3 = new Label();
            btnResetExploration = new Button();
            label1 = new Label();
            txtExplorationValue = new TextBox();
            textBox2 = new TextBox();
            txtBodies = new TextBox();
            linkNewBuildAvailable = new LinkLabel();
            btnTest = new Button();
            groupCodex = new GroupBox();
            btnCodexBingo = new Button();
            lblBig = new Label();
            btnPublishBio = new Button();
            btnPublishHuman = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            groupCodex.SuspendLayout();
            SuspendLayout();
            // 
            // btnGroundTarget
            // 
            btnGroundTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnGroundTarget.BackColor = SystemColors.ControlDark;
            btnGroundTarget.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnGroundTarget.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnGroundTarget.FlatStyle = FlatStyle.Flat;
            btnGroundTarget.Location = new Point(88, 44);
            btnGroundTarget.Name = "btnGroundTarget";
            btnGroundTarget.Size = new Size(37, 27);
            btnGroundTarget.TabIndex = 3;
            btnGroundTarget.Text = "Set";
            btnGroundTarget.UseVisualStyleBackColor = false;
            btnGroundTarget.Click += btnGroundTarget_Click;
            // 
            // btnQuit2
            // 
            btnQuit2.BackColor = SystemColors.ControlDark;
            btnQuit2.DialogResult = DialogResult.Cancel;
            btnQuit2.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnQuit2.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnQuit2.FlatStyle = FlatStyle.Flat;
            btnQuit2.Location = new Point(350, 593);
            btnQuit2.Name = "btnQuit2";
            btnQuit2.Size = new Size(75, 23);
            btnQuit2.TabIndex = 8;
            btnQuit2.Text = "&Quit";
            btnQuit2.UseVisualStyleBackColor = false;
            btnQuit2.Click += btnQuit_Click;
            // 
            // txtTargetLatLong
            // 
            txtTargetLatLong.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTargetLatLong.BackColor = SystemColors.AppWorkspace;
            txtTargetLatLong.BorderStyle = BorderStyle.FixedSingle;
            txtTargetLatLong.Location = new Point(14, 19);
            txtTargetLatLong.Name = "txtTargetLatLong";
            txtTargetLatLong.ReadOnly = true;
            txtTargetLatLong.Size = new Size(194, 20);
            txtTargetLatLong.TabIndex = 11;
            txtTargetLatLong.Text = "<none>";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 45);
            label2.Name = "label2";
            label2.Size = new Size(0, 12);
            label2.TabIndex = 12;
            // 
            // btnClearTarget
            // 
            btnClearTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClearTarget.BackColor = SystemColors.ControlDark;
            btnClearTarget.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnClearTarget.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnClearTarget.FlatStyle = FlatStyle.Flat;
            btnClearTarget.Location = new Point(131, 44);
            btnClearTarget.Name = "btnClearTarget";
            btnClearTarget.Size = new Size(51, 27);
            btnClearTarget.TabIndex = 13;
            btnClearTarget.Text = "Hide";
            btnClearTarget.UseVisualStyleBackColor = false;
            btnClearTarget.Click += btnClearTarget_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnPasteLatLong);
            groupBox1.Controls.Add(lblTrackTargetStatus);
            groupBox1.Controls.Add(btnClearTarget);
            groupBox1.Controls.Add(btnGroundTarget);
            groupBox1.Controls.Add(txtTargetLatLong);
            groupBox1.Location = new Point(105, 433);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(214, 78);
            groupBox1.TabIndex = 14;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target lat/Long:";
            // 
            // btnPasteLatLong
            // 
            btnPasteLatLong.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPasteLatLong.BackColor = SystemColors.ControlDark;
            btnPasteLatLong.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPasteLatLong.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPasteLatLong.FlatStyle = FlatStyle.Flat;
            btnPasteLatLong.Image = (Image)resources.GetObject("btnPasteLatLong.Image");
            btnPasteLatLong.Location = new Point(186, 44);
            btnPasteLatLong.Name = "btnPasteLatLong";
            btnPasteLatLong.Size = new Size(23, 27);
            btnPasteLatLong.TabIndex = 26;
            btnPasteLatLong.UseVisualStyleBackColor = false;
            btnPasteLatLong.Click += btnPasteLatLong_Click;
            // 
            // lblTrackTargetStatus
            // 
            lblTrackTargetStatus.AutoSize = true;
            lblTrackTargetStatus.BorderStyle = BorderStyle.FixedSingle;
            lblTrackTargetStatus.Location = new Point(14, 49);
            lblTrackTargetStatus.Name = "lblTrackTargetStatus";
            lblTrackTargetStatus.Size = new Size(63, 14);
            lblTrackTargetStatus.TabIndex = 18;
            lblTrackTargetStatus.Text = "<status>";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnCopyLocation);
            groupBox2.Controls.Add(txtCommander);
            groupBox2.Controls.Add(txtNearBody);
            groupBox2.Controls.Add(txtMode);
            groupBox2.Controls.Add(txtLocation);
            groupBox2.Controls.Add(txtVehicle);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(413, 98);
            groupBox2.TabIndex = 14;
            groupBox2.TabStop = false;
            groupBox2.Text = "Commander:";
            // 
            // btnCopyLocation
            // 
            btnCopyLocation.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyLocation.BackColor = SystemColors.ControlDark;
            btnCopyLocation.Enabled = false;
            btnCopyLocation.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCopyLocation.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCopyLocation.FlatStyle = FlatStyle.Flat;
            btnCopyLocation.Image = (Image)resources.GetObject("btnCopyLocation.Image");
            btnCopyLocation.Location = new Point(288, 43);
            btnCopyLocation.Name = "btnCopyLocation";
            btnCopyLocation.Size = new Size(20, 20);
            btnCopyLocation.TabIndex = 27;
            btnCopyLocation.UseVisualStyleBackColor = false;
            btnCopyLocation.Click += btnCopyLocation_Click;
            // 
            // txtCommander
            // 
            txtCommander.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommander.BackColor = SystemColors.AppWorkspace;
            txtCommander.BorderStyle = BorderStyle.FixedSingle;
            txtCommander.Location = new Point(6, 19);
            txtCommander.Name = "txtCommander";
            txtCommander.ReadOnly = true;
            txtCommander.Size = new Size(400, 20);
            txtCommander.TabIndex = 10;
            txtCommander.Text = "GRINNING2002 ?";
            // 
            // txtNearBody
            // 
            txtNearBody.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtNearBody.BackColor = SystemColors.AppWorkspace;
            txtNearBody.BorderStyle = BorderStyle.FixedSingle;
            txtNearBody.Location = new Point(313, 43);
            txtNearBody.Name = "txtNearBody";
            txtNearBody.ReadOnly = true;
            txtNearBody.Size = new Size(93, 20);
            txtNearBody.TabIndex = 14;
            txtNearBody.Text = "LandableBody";
            // 
            // txtMode
            // 
            txtMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMode.BackColor = SystemColors.AppWorkspace;
            txtMode.BorderStyle = BorderStyle.FixedSingle;
            txtMode.Location = new Point(108, 69);
            txtMode.Name = "txtMode";
            txtMode.ReadOnly = true;
            txtMode.Size = new Size(298, 20);
            txtMode.TabIndex = 13;
            txtMode.Text = "<mode>";
            // 
            // txtLocation
            // 
            txtLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLocation.BackColor = SystemColors.AppWorkspace;
            txtLocation.BorderStyle = BorderStyle.FixedSingle;
            txtLocation.Location = new Point(6, 43);
            txtLocation.Name = "txtLocation";
            txtLocation.ReadOnly = true;
            txtLocation.Size = new Size(280, 20);
            txtLocation.TabIndex = 12;
            txtLocation.Text = "<location>";
            // 
            // txtVehicle
            // 
            txtVehicle.BackColor = SystemColors.AppWorkspace;
            txtVehicle.BorderStyle = BorderStyle.FixedSingle;
            txtVehicle.Location = new Point(6, 69);
            txtVehicle.Name = "txtVehicle";
            txtVehicle.ReadOnly = true;
            txtVehicle.Size = new Size(96, 20);
            txtVehicle.TabIndex = 11;
            txtVehicle.Text = "<vehicle>";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnBioSummary);
            groupBox3.Controls.Add(checkFirstFootFall);
            groupBox3.Controls.Add(txtBodyBioValues);
            groupBox3.Controls.Add(txtBodyBioSignals);
            groupBox3.Controls.Add(lblSysBio);
            groupBox3.Controls.Add(txtSystemBioValues);
            groupBox3.Controls.Add(txtSystemBioSignals);
            groupBox3.Controls.Add(labelSignalsAndRewards);
            groupBox3.Controls.Add(lblBodyBio);
            groupBox3.Controls.Add(txtBioRewards);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new Point(12, 197);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(413, 130);
            groupBox3.TabIndex = 14;
            groupBox3.TabStop = false;
            groupBox3.Text = "Bio scanning:";
            // 
            // btnBioSummary
            // 
            btnBioSummary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBioSummary.BackColor = SystemColors.ControlDark;
            btnBioSummary.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnBioSummary.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnBioSummary.FlatStyle = FlatStyle.Flat;
            btnBioSummary.Location = new Point(313, 103);
            btnBioSummary.Name = "btnBioSummary";
            btnBioSummary.Size = new Size(93, 21);
            btnBioSummary.TabIndex = 28;
            btnBioSummary.Text = "Predictions";
            btnBioSummary.UseVisualStyleBackColor = false;
            btnBioSummary.Click += btnBioSummary_Click;
            // 
            // checkFirstFootFall
            // 
            checkFirstFootFall.AutoSize = true;
            checkFirstFootFall.Enabled = false;
            checkFirstFootFall.FlatStyle = FlatStyle.System;
            checkFirstFootFall.Location = new Point(143, 107);
            checkFirstFootFall.Name = "checkFirstFootFall";
            checkFirstFootFall.Size = new Size(128, 17);
            checkFirstFootFall.TabIndex = 27;
            checkFirstFootFall.Text = "First Footfall";
            checkFirstFootFall.UseVisualStyleBackColor = true;
            checkFirstFootFall.CheckedChanged += checkFirstFootFall_CheckedChanged;
            // 
            // txtBodyBioValues
            // 
            txtBodyBioValues.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBodyBioValues.BackColor = SystemColors.AppWorkspace;
            txtBodyBioValues.BorderStyle = BorderStyle.FixedSingle;
            txtBodyBioValues.Enabled = false;
            txtBodyBioValues.Location = new Point(143, 81);
            txtBodyBioValues.Name = "txtBodyBioValues";
            txtBodyBioValues.ReadOnly = true;
            txtBodyBioValues.Size = new Size(263, 20);
            txtBodyBioValues.TabIndex = 26;
            // 
            // txtBodyBioSignals
            // 
            txtBodyBioSignals.BackColor = SystemColors.AppWorkspace;
            txtBodyBioSignals.BorderStyle = BorderStyle.FixedSingle;
            txtBodyBioSignals.Enabled = false;
            txtBodyBioSignals.Location = new Point(69, 81);
            txtBodyBioSignals.Name = "txtBodyBioSignals";
            txtBodyBioSignals.ReadOnly = true;
            txtBodyBioSignals.Size = new Size(77, 20);
            txtBodyBioSignals.TabIndex = 24;
            txtBodyBioSignals.TextAlign = HorizontalAlignment.Center;
            // 
            // lblSysBio
            // 
            lblSysBio.AutoSize = true;
            lblSysBio.Enabled = false;
            lblSysBio.Location = new Point(8, 58);
            lblSysBio.Name = "lblSysBio";
            lblSysBio.Size = new Size(54, 12);
            lblSysBio.TabIndex = 23;
            lblSysBio.Text = "System:";
            // 
            // txtSystemBioValues
            // 
            txtSystemBioValues.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemBioValues.BackColor = SystemColors.AppWorkspace;
            txtSystemBioValues.BorderStyle = BorderStyle.FixedSingle;
            txtSystemBioValues.Enabled = false;
            txtSystemBioValues.Location = new Point(143, 55);
            txtSystemBioValues.Name = "txtSystemBioValues";
            txtSystemBioValues.ReadOnly = true;
            txtSystemBioValues.Size = new Size(263, 20);
            txtSystemBioValues.TabIndex = 22;
            // 
            // txtSystemBioSignals
            // 
            txtSystemBioSignals.BackColor = SystemColors.AppWorkspace;
            txtSystemBioSignals.BorderStyle = BorderStyle.FixedSingle;
            txtSystemBioSignals.Enabled = false;
            txtSystemBioSignals.Location = new Point(69, 55);
            txtSystemBioSignals.Name = "txtSystemBioSignals";
            txtSystemBioSignals.ReadOnly = true;
            txtSystemBioSignals.Size = new Size(77, 20);
            txtSystemBioSignals.TabIndex = 20;
            txtSystemBioSignals.Text = "99 of 99";
            txtSystemBioSignals.TextAlign = HorizontalAlignment.Center;
            // 
            // labelSignalsAndRewards
            // 
            labelSignalsAndRewards.AutoSize = true;
            labelSignalsAndRewards.Location = new Point(71, 38);
            labelSignalsAndRewards.Name = "labelSignalsAndRewards";
            labelSignalsAndRewards.Size = new Size(201, 12);
            labelSignalsAndRewards.TabIndex = 17;
            labelSignalsAndRewards.Text = "Scanned signals and rewards:";
            // 
            // lblBodyBio
            // 
            lblBodyBio.AutoSize = true;
            lblBodyBio.Enabled = false;
            lblBodyBio.Location = new Point(22, 84);
            lblBodyBio.Name = "lblBodyBio";
            lblBodyBio.Size = new Size(40, 12);
            lblBodyBio.TabIndex = 16;
            lblBodyBio.Text = "Body:";
            // 
            // txtBioRewards
            // 
            txtBioRewards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBioRewards.BackColor = SystemColors.AppWorkspace;
            txtBioRewards.BorderStyle = BorderStyle.FixedSingle;
            txtBioRewards.Location = new Point(143, 13);
            txtBioRewards.Name = "txtBioRewards";
            txtBioRewards.ReadOnly = true;
            txtBioRewards.Size = new Size(263, 20);
            txtBioRewards.TabIndex = 15;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 16);
            label4.Name = "label4";
            label4.Size = new Size(131, 12);
            label4.TabIndex = 14;
            label4.Text = "Unclaimed rewards:";
            // 
            // btnCodexShow
            // 
            btnCodexShow.BackColor = SystemColors.ControlDark;
            btnCodexShow.Enabled = false;
            btnCodexShow.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCodexShow.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCodexShow.FlatStyle = FlatStyle.Flat;
            btnCodexShow.Image = (Image)resources.GetObject("btnCodexShow.Image");
            btnCodexShow.Location = new Point(379, 517);
            btnCodexShow.Name = "btnCodexShow";
            btnCodexShow.Size = new Size(46, 36);
            btnCodexShow.TabIndex = 29;
            btnCodexShow.UseVisualStyleBackColor = false;
            btnCodexShow.Click += btnCodexShow_Click;
            // 
            // timer1
            // 
            timer1.Interval = 200;
            timer1.Tick += timer1_Tick;
            // 
            // btnLogs
            // 
            btnLogs.BackColor = SystemColors.ControlDark;
            btnLogs.DialogResult = DialogResult.Cancel;
            btnLogs.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnLogs.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnLogs.FlatStyle = FlatStyle.Flat;
            btnLogs.Location = new Point(93, 593);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(75, 23);
            btnLogs.TabIndex = 15;
            btnLogs.Text = "&Logs";
            btnLogs.UseVisualStyleBackColor = false;
            btnLogs.Click += btnViewLogs_Click;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = SystemColors.ControlDark;
            btnSettings.DialogResult = DialogResult.Cancel;
            btnSettings.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnSettings.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Location = new Point(12, 593);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(75, 23);
            btnSettings.TabIndex = 16;
            btnSettings.Text = "&Settings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point);
            linkLabel1.LinkArea = new LinkArea(13, 12);
            linkLabel1.Location = new Point(12, 541);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(415, 18);
            linkLabel1.TabIndex = 17;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "For guidance see the wiki on GitHub";
            linkLabel1.TextAlign = ContentAlignment.MiddleLeft;
            linkLabel1.UseCompatibleTextRendering = true;
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // lblFullScreen
            // 
            lblFullScreen.BackColor = Color.DarkRed;
            lblFullScreen.Dock = DockStyle.Top;
            lblFullScreen.ForeColor = Color.White;
            lblFullScreen.Location = new Point(0, 0);
            lblFullScreen.Name = "lblFullScreen";
            lblFullScreen.Padding = new Padding(10);
            lblFullScreen.Size = new Size(437, 86);
            lblFullScreen.TabIndex = 18;
            lblFullScreen.Text = "SrvSurvey cannot be used when Elite Dangerous is in Full Screen mode.\r\n\r\nPlease go to Options > Graphics > Display and change setting FULLSCREEN to either BORDERLESS or WINDOWED.";
            lblFullScreen.TextAlign = ContentAlignment.TopCenter;
            lblFullScreen.Visible = false;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(btnGuardianThings);
            groupBox4.Controls.Add(btnRuins);
            groupBox4.Controls.Add(btnRuinsMap);
            groupBox4.Controls.Add(btnRuinsOrigin);
            groupBox4.Controls.Add(txtGuardianSite);
            groupBox4.Controls.Add(lblGuardianCount);
            groupBox4.Controls.Add(label5);
            groupBox4.Location = new Point(12, 333);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(413, 94);
            groupBox4.TabIndex = 19;
            groupBox4.TabStop = false;
            groupBox4.Text = "Guardian sites:";
            // 
            // btnGuardianThings
            // 
            btnGuardianThings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuardianThings.BackColor = SystemColors.ControlDark;
            btnGuardianThings.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnGuardianThings.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnGuardianThings.FlatStyle = FlatStyle.Flat;
            btnGuardianThings.Location = new Point(279, 15);
            btnGuardianThings.Name = "btnGuardianThings";
            btnGuardianThings.Size = new Size(127, 35);
            btnGuardianThings.TabIndex = 19;
            btnGuardianThings.Text = "All Guardian Sites";
            btnGuardianThings.UseVisualStyleBackColor = false;
            btnGuardianThings.Click += btnGuardianThings_Click;
            // 
            // btnRuins
            // 
            btnRuins.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRuins.BackColor = SystemColors.ControlDark;
            btnRuins.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnRuins.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnRuins.FlatStyle = FlatStyle.Flat;
            btnRuins.Location = new Point(279, 55);
            btnRuins.Name = "btnRuins";
            btnRuins.Size = new Size(127, 35);
            btnRuins.TabIndex = 18;
            btnRuins.Text = "Survey Maps";
            btnRuins.UseVisualStyleBackColor = false;
            btnRuins.Click += btnRuins_Click;
            // 
            // btnRuinsMap
            // 
            btnRuinsMap.BackColor = SystemColors.ControlDark;
            btnRuinsMap.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnRuinsMap.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnRuinsMap.FlatStyle = FlatStyle.Flat;
            btnRuinsMap.Location = new Point(16, 57);
            btnRuinsMap.Name = "btnRuinsMap";
            btnRuinsMap.Size = new Size(93, 21);
            btnRuinsMap.TabIndex = 16;
            btnRuinsMap.Text = "Show map";
            btnRuinsMap.UseVisualStyleBackColor = false;
            btnRuinsMap.Click += btnRuinsMap_Click;
            // 
            // btnRuinsOrigin
            // 
            btnRuinsOrigin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRuinsOrigin.BackColor = SystemColors.ControlDark;
            btnRuinsOrigin.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnRuinsOrigin.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnRuinsOrigin.FlatStyle = FlatStyle.Flat;
            btnRuinsOrigin.Location = new Point(115, 57);
            btnRuinsOrigin.Name = "btnRuinsOrigin";
            btnRuinsOrigin.Size = new Size(115, 21);
            btnRuinsOrigin.TabIndex = 15;
            btnRuinsOrigin.Text = "Aerial assist";
            btnRuinsOrigin.UseVisualStyleBackColor = false;
            btnRuinsOrigin.Click += btnRuinsOrigin_Click;
            // 
            // txtGuardianSite
            // 
            txtGuardianSite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtGuardianSite.BackColor = SystemColors.AppWorkspace;
            txtGuardianSite.BorderStyle = BorderStyle.FixedSingle;
            txtGuardianSite.Location = new Point(16, 31);
            txtGuardianSite.Name = "txtGuardianSite";
            txtGuardianSite.ReadOnly = true;
            txtGuardianSite.Size = new Size(256, 20);
            txtGuardianSite.TabIndex = 14;
            txtGuardianSite.Text = "<Guardian site>";
            // 
            // lblGuardianCount
            // 
            lblGuardianCount.AutoSize = true;
            lblGuardianCount.Location = new Point(88, 16);
            lblGuardianCount.Name = "lblGuardianCount";
            lblGuardianCount.Size = new Size(12, 12);
            lblGuardianCount.TabIndex = 3;
            lblGuardianCount.Text = "N";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(14, 16);
            label5.Name = "label5";
            label5.Size = new Size(68, 12);
            label5.TabIndex = 2;
            label5.Text = "Detected:";
            // 
            // lblNotInstalled
            // 
            lblNotInstalled.BackColor = Color.DarkRed;
            lblNotInstalled.Dock = DockStyle.Top;
            lblNotInstalled.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblNotInstalled.ForeColor = Color.White;
            lblNotInstalled.LinkArea = new LinkArea(57, 15);
            lblNotInstalled.LinkColor = Color.FromArgb(128, 255, 255);
            lblNotInstalled.Location = new Point(0, 86);
            lblNotInstalled.Name = "lblNotInstalled";
            lblNotInstalled.Padding = new Padding(10);
            lblNotInstalled.Size = new Size(437, 102);
            lblNotInstalled.TabIndex = 21;
            lblNotInstalled.TabStop = true;
            lblNotInstalled.Text = "This application is intended for use only with the game \"Elite Dangerous\" by Frontier Developments.\r\n\r\nIt does not appear the game is installed.";
            lblNotInstalled.TextAlign = ContentAlignment.TopCenter;
            lblNotInstalled.UseCompatibleTextRendering = true;
            lblNotInstalled.Visible = false;
            lblNotInstalled.LinkClicked += lblNotInstalled_LinkClicked;
            // 
            // btnSphereLimit
            // 
            btnSphereLimit.BackColor = SystemColors.ControlDark;
            btnSphereLimit.Enabled = false;
            btnSphereLimit.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnSphereLimit.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnSphereLimit.FlatStyle = FlatStyle.Flat;
            btnSphereLimit.Location = new Point(325, 478);
            btnSphereLimit.Name = "btnSphereLimit";
            btnSphereLimit.Size = new Size(100, 33);
            btnSphereLimit.TabIndex = 22;
            btnSphereLimit.Text = "Sphere limit";
            btnSphereLimit.UseVisualStyleBackColor = false;
            btnSphereLimit.Click += btnSphereLimit_Click;
            // 
            // linkLabel2
            // 
            linkLabel2.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point);
            linkLabel2.LinkArea = new LinkArea(17, 22);
            linkLabel2.Location = new Point(10, 561);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(427, 29);
            linkLabel2.TabIndex = 23;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "Ask questions at Guardian Science Corps on Discord";
            linkLabel2.UseCompatibleTextRendering = true;
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            // 
            // btnRamTah
            // 
            btnRamTah.BackColor = SystemColors.ControlDark;
            btnRamTah.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnRamTah.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnRamTah.FlatStyle = FlatStyle.Flat;
            btnRamTah.Location = new Point(325, 438);
            btnRamTah.Name = "btnRamTah";
            btnRamTah.Size = new Size(100, 35);
            btnRamTah.TabIndex = 24;
            btnRamTah.Text = "Ram Tah Missions";
            btnRamTah.UseVisualStyleBackColor = false;
            btnRamTah.Click += btnRamTah_Click;
            // 
            // btnPublishGuardian
            // 
            btnPublishGuardian.BackColor = SystemColors.ControlDark;
            btnPublishGuardian.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPublishGuardian.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPublishGuardian.FlatStyle = FlatStyle.Flat;
            btnPublishGuardian.Location = new Point(174, 593);
            btnPublishGuardian.Name = "btnPublishGuardian";
            btnPublishGuardian.Size = new Size(37, 23);
            btnPublishGuardian.TabIndex = 25;
            btnPublishGuardian.Text = "P.G";
            btnPublishGuardian.UseVisualStyleBackColor = false;
            btnPublishGuardian.Visible = false;
            btnPublishGuardian.Click += btnPublishGuardian_Click;
            // 
            // checkTempHide
            // 
            checkTempHide.Location = new Point(12, 517);
            checkTempHide.Name = "checkTempHide";
            checkTempHide.Size = new Size(227, 16);
            checkTempHide.TabIndex = 26;
            checkTempHide.Text = "Temporarily hide all overlays";
            checkTempHide.UseVisualStyleBackColor = true;
            checkTempHide.CheckedChanged += checkTempHide_CheckedChanged;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(txtDistance);
            groupBox5.Controls.Add(txtJumps);
            groupBox5.Controls.Add(label6);
            groupBox5.Controls.Add(label3);
            groupBox5.Controls.Add(btnResetExploration);
            groupBox5.Controls.Add(label1);
            groupBox5.Controls.Add(txtExplorationValue);
            groupBox5.Controls.Add(textBox2);
            groupBox5.Controls.Add(txtBodies);
            groupBox5.Location = new Point(12, 116);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(413, 72);
            groupBox5.TabIndex = 28;
            groupBox5.TabStop = false;
            groupBox5.Text = "Exploration trip counter:";
            // 
            // txtDistance
            // 
            txtDistance.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDistance.BackColor = SystemColors.AppWorkspace;
            txtDistance.BorderStyle = BorderStyle.FixedSingle;
            txtDistance.Location = new Point(313, 16);
            txtDistance.Name = "txtDistance";
            txtDistance.ReadOnly = true;
            txtDistance.Size = new Size(93, 20);
            txtDistance.TabIndex = 33;
            txtDistance.Text = "-";
            txtDistance.TextAlign = HorizontalAlignment.Center;
            // 
            // txtJumps
            // 
            txtJumps.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtJumps.BackColor = SystemColors.AppWorkspace;
            txtJumps.BorderStyle = BorderStyle.FixedSingle;
            txtJumps.Location = new Point(255, 16);
            txtJumps.Name = "txtJumps";
            txtJumps.ReadOnly = true;
            txtJumps.Size = new Size(55, 20);
            txtJumps.TabIndex = 32;
            txtJumps.Text = "-";
            txtJumps.TextAlign = HorizontalAlignment.Center;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(67, 45);
            label6.Name = "label6";
            label6.Size = new Size(54, 12);
            label6.TabIndex = 31;
            label6.Text = "Bodies:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(205, 19);
            label3.Name = "label3";
            label3.Size = new Size(47, 12);
            label3.TabIndex = 30;
            label3.Text = "Jumps:";
            // 
            // btnResetExploration
            // 
            btnResetExploration.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnResetExploration.BackColor = SystemColors.ControlDark;
            btnResetExploration.Enabled = false;
            btnResetExploration.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnResetExploration.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnResetExploration.FlatStyle = FlatStyle.Flat;
            btnResetExploration.Location = new Point(13, 41);
            btnResetExploration.Name = "btnResetExploration";
            btnResetExploration.Size = new Size(49, 21);
            btnResetExploration.TabIndex = 29;
            btnResetExploration.Text = "Reset";
            btnResetExploration.UseVisualStyleBackColor = false;
            btnResetExploration.Click += btnResetExploration_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 19);
            label1.Name = "label1";
            label1.Size = new Size(117, 12);
            label1.TabIndex = 28;
            label1.Text = "Estimated value:";
            // 
            // txtExplorationValue
            // 
            txtExplorationValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtExplorationValue.BackColor = SystemColors.AppWorkspace;
            txtExplorationValue.BorderStyle = BorderStyle.FixedSingle;
            txtExplorationValue.Location = new Point(127, 16);
            txtExplorationValue.Name = "txtExplorationValue";
            txtExplorationValue.ReadOnly = true;
            txtExplorationValue.Size = new Size(72, 20);
            txtExplorationValue.TabIndex = 10;
            txtExplorationValue.Text = "-";
            txtExplorationValue.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBox2.Location = new Point(526, 43);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(93, 20);
            textBox2.TabIndex = 14;
            textBox2.Text = "LandableBody";
            // 
            // txtBodies
            // 
            txtBodies.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBodies.BackColor = SystemColors.AppWorkspace;
            txtBodies.BorderStyle = BorderStyle.FixedSingle;
            txtBodies.Location = new Point(127, 42);
            txtBodies.Name = "txtBodies";
            txtBodies.ReadOnly = true;
            txtBodies.Size = new Size(280, 20);
            txtBodies.TabIndex = 12;
            txtBodies.Text = "-";
            // 
            // linkNewBuildAvailable
            // 
            linkNewBuildAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            linkNewBuildAvailable.BackColor = Color.Transparent;
            linkNewBuildAvailable.Font = new Font("Lucida Sans Typewriter", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            linkNewBuildAvailable.LinkArea = new LinkArea(1, 16);
            linkNewBuildAvailable.Location = new Point(267, 0);
            linkNewBuildAvailable.Name = "linkNewBuildAvailable";
            linkNewBuildAvailable.Size = new Size(170, 18);
            linkNewBuildAvailable.TabIndex = 29;
            linkNewBuildAvailable.TabStop = true;
            linkNewBuildAvailable.Text = "(update available)";
            linkNewBuildAvailable.TextAlign = ContentAlignment.MiddleRight;
            linkNewBuildAvailable.UseCompatibleTextRendering = true;
            linkNewBuildAvailable.Visible = false;
            linkNewBuildAvailable.LinkClicked += linkNewBuildAvailable_LinkClicked;
            // 
            // btnTest
            // 
            btnTest.BackColor = SystemColors.ControlDark;
            btnTest.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnTest.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.Location = new Point(300, 593);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(44, 23);
            btnTest.TabIndex = 30;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = false;
            btnTest.MouseDown += btnTest_MouseDown;
            // 
            // groupCodex
            // 
            groupCodex.Controls.Add(btnCodexBingo);
            groupCodex.Location = new Point(12, 433);
            groupCodex.Name = "groupCodex";
            groupCodex.Size = new Size(87, 78);
            groupCodex.TabIndex = 31;
            groupCodex.TabStop = false;
            groupCodex.Text = "Codex";
            groupCodex.Paint += groupCodex_Paint;
            // 
            // btnCodexBingo
            // 
            btnCodexBingo.BackColor = SystemColors.ControlDark;
            btnCodexBingo.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCodexBingo.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCodexBingo.FlatStyle = FlatStyle.Flat;
            btnCodexBingo.Location = new Point(6, 51);
            btnCodexBingo.Name = "btnCodexBingo";
            btnCodexBingo.Size = new Size(75, 21);
            btnCodexBingo.TabIndex = 20;
            btnCodexBingo.Text = "Bingo";
            btnCodexBingo.UseVisualStyleBackColor = false;
            btnCodexBingo.Click += btnCodexBingo_Click;
            // 
            // lblBig
            // 
            lblBig.AutoSize = true;
            lblBig.Font = new Font("Century Gothic", 21.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblBig.Location = new Point(300, 517);
            lblBig.Name = "lblBig";
            lblBig.Size = new Size(72, 36);
            lblBig.TabIndex = 32;
            lblBig.Text = "20%";
            lblBig.Visible = false;
            // 
            // btnPublishBio
            // 
            btnPublishBio.BackColor = SystemColors.ControlDark;
            btnPublishBio.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPublishBio.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPublishBio.FlatStyle = FlatStyle.Flat;
            btnPublishBio.Location = new Point(217, 593);
            btnPublishBio.Name = "btnPublishBio";
            btnPublishBio.Size = new Size(37, 23);
            btnPublishBio.TabIndex = 33;
            btnPublishBio.Text = "P.B";
            btnPublishBio.UseVisualStyleBackColor = false;
            btnPublishBio.Visible = false;
            btnPublishBio.Click += btnPublishBio_Click;
            // 
            // btnPublishHuman
            // 
            btnPublishHuman.BackColor = SystemColors.ControlDark;
            btnPublishHuman.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPublishHuman.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPublishHuman.FlatStyle = FlatStyle.Flat;
            btnPublishHuman.Location = new Point(260, 593);
            btnPublishHuman.Name = "btnPublishHuman";
            btnPublishHuman.Size = new Size(37, 23);
            btnPublishHuman.TabIndex = 34;
            btnPublishHuman.Text = "P.H";
            btnPublishHuman.UseVisualStyleBackColor = false;
            btnPublishHuman.Visible = false;
            btnPublishHuman.Click += btnPublishHuman_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.AppWorkspace;
            ClientSize = new Size(437, 628);
            Controls.Add(btnPublishHuman);
            Controls.Add(btnPublishBio);
            Controls.Add(lblBig);
            Controls.Add(groupCodex);
            Controls.Add(btnTest);
            Controls.Add(btnCodexShow);
            Controls.Add(linkNewBuildAvailable);
            Controls.Add(groupBox5);
            Controls.Add(checkTempHide);
            Controls.Add(btnPublishGuardian);
            Controls.Add(btnRamTah);
            Controls.Add(linkLabel2);
            Controls.Add(btnSphereLimit);
            Controls.Add(groupBox3);
            Controls.Add(groupBox4);
            Controls.Add(linkLabel1);
            Controls.Add(btnSettings);
            Controls.Add(btnLogs);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(label2);
            Controls.Add(btnQuit2);
            Controls.Add(lblNotInstalled);
            Controls.Add(lblFullScreen);
            Font = new Font("Lucida Sans Typewriter", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Main";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Srv Survey";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            SizeChanged += Main_SizeChanged;
            MouseDoubleClick += Main_MouseDoubleClick;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupCodex.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnGroundTarget;
        private Button btnQuit2;
        private TextBox txtTargetLatLong;
        private Label label2;
        private Button btnClearTarget;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private TextBox txtCommander;
        private GroupBox groupBox3;
        private TextBox txtVehicle;
        private TextBox txtLocation;
        private System.Windows.Forms.Timer timer1;
        private TextBox txtMode;
        private Button btnLogs;
        private Button btnSettings;
        private LinkLabel linkLabel1;
        private Label lblTrackTargetStatus;
        private TextBox txtNearBody;
        private Label lblFullScreen;
        private GroupBox groupBox4;
        private Label lblGuardianCount;
        private Label label5;
        private TextBox txtGuardianSite;
        private LinkLabel lblNotInstalled;
        private TextBox txtBioRewards;
        private Label label4;
        private TextBox txtBodyBioValues;
        private TextBox txtBodyBioSignals;
        private Label lblSysBio;
        private TextBox txtSystemBioValues;
        private TextBox txtSystemBioSignals;
        private Label labelSignalsAndRewards;
        private Label lblBodyBio;
        private Button btnRuinsMap;
        private Button btnRuinsOrigin;
        private Button btnRuins;
        private Button btnSphereLimit;
        private Button btnGuardianThings;
        private LinkLabel linkLabel2;
        private CheckBox checkFirstFootFall;
        private Button btnRamTah;
        private Button btnPublishGuardian;
        private Button btnPasteLatLong;
        private CheckBox checkTempHide;
        private Button btnBioSummary;
        private Button btnCopyLocation;
        private GroupBox groupBox5;
        private Label label1;
        private TextBox txtExplorationValue;
        private TextBox textBox2;
        private TextBox txtBodies;
        private TextBox txtJumps;
        private Label label6;
        private Label label3;
        private Button btnResetExploration;
        private TextBox txtDistance;
        private LinkLabel linkNewBuildAvailable;
        public Button btnCodexShow;
        private Button btnTest;
        private GroupBox groupCodex;
        private Button btnCodexBingo;
        private Label lblBig;
        private Button btnPublishBio;
        private Button btnPublishHuman;
    }
}