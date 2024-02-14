
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
            txtCommander = new TextBox();
            txtNearBody = new TextBox();
            txtMode = new TextBox();
            txtLocation = new TextBox();
            txtVehicle = new TextBox();
            groupBox3 = new GroupBox();
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
            timer1 = new System.Windows.Forms.Timer(components);
            btnLogs = new Button();
            btnSettings = new Button();
            linkLabel1 = new LinkLabel();
            lblFullScreen = new Label();
            groupBox4 = new GroupBox();
            btnGuarduanThings = new Button();
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
            btnPublish = new Button();
            checkTempHide = new CheckBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // btnGroundTarget
            // 
            btnGroundTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnGroundTarget.BackColor = SystemColors.ControlLight;
            btnGroundTarget.FlatStyle = FlatStyle.System;
            btnGroundTarget.Location = new Point(127, 44);
            btnGroundTarget.Name = "btnGroundTarget";
            btnGroundTarget.Size = new Size(91, 27);
            btnGroundTarget.TabIndex = 3;
            btnGroundTarget.Text = "Set target";
            btnGroundTarget.UseVisualStyleBackColor = false;
            btnGroundTarget.Click += btnGroundTarget_Click;
            // 
            // btnQuit2
            // 
            btnQuit2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnQuit2.DialogResult = DialogResult.Cancel;
            btnQuit2.FlatStyle = FlatStyle.System;
            btnQuit2.Location = new Point(350, 512);
            btnQuit2.Name = "btnQuit2";
            btnQuit2.Size = new Size(75, 23);
            btnQuit2.TabIndex = 8;
            btnQuit2.Text = "&Quit";
            btnQuit2.UseVisualStyleBackColor = true;
            btnQuit2.Click += btnQuit_Click;
            // 
            // txtTargetLatLong
            // 
            txtTargetLatLong.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTargetLatLong.Location = new Point(16, 19);
            txtTargetLatLong.Name = "txtTargetLatLong";
            txtTargetLatLong.ReadOnly = true;
            txtTargetLatLong.Size = new Size(285, 20);
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
            btnClearTarget.BackColor = SystemColors.ControlLight;
            btnClearTarget.FlatStyle = FlatStyle.System;
            btnClearTarget.Location = new Point(222, 44);
            btnClearTarget.Name = "btnClearTarget";
            btnClearTarget.Size = new Size(53, 27);
            btnClearTarget.TabIndex = 13;
            btnClearTarget.Text = "Hide";
            btnClearTarget.UseVisualStyleBackColor = false;
            btnClearTarget.Click += btnClearTarget_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(btnPasteLatLong);
            groupBox1.Controls.Add(lblTrackTargetStatus);
            groupBox1.Controls.Add(btnClearTarget);
            groupBox1.Controls.Add(btnGroundTarget);
            groupBox1.Controls.Add(txtTargetLatLong);
            groupBox1.Location = new Point(12, 352);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(307, 78);
            groupBox1.TabIndex = 14;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target lat/Long:";
            // 
            // btnPasteLatLong
            // 
            btnPasteLatLong.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPasteLatLong.Image = (Image)resources.GetObject("btnPasteLatLong.Image");
            btnPasteLatLong.Location = new Point(279, 44);
            btnPasteLatLong.Name = "btnPasteLatLong";
            btnPasteLatLong.Size = new Size(23, 27);
            btnPasteLatLong.TabIndex = 26;
            btnPasteLatLong.UseVisualStyleBackColor = true;
            btnPasteLatLong.Click += btnPasteLatLong_Click;
            // 
            // lblTrackTargetStatus
            // 
            lblTrackTargetStatus.AutoSize = true;
            lblTrackTargetStatus.BorderStyle = BorderStyle.Fixed3D;
            lblTrackTargetStatus.Location = new Point(14, 49);
            lblTrackTargetStatus.Name = "lblTrackTargetStatus";
            lblTrackTargetStatus.Size = new Size(63, 14);
            lblTrackTargetStatus.TabIndex = 18;
            lblTrackTargetStatus.Text = "<status>";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
            // txtCommander
            // 
            txtCommander.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommander.Location = new Point(16, 19);
            txtCommander.Name = "txtCommander";
            txtCommander.ReadOnly = true;
            txtCommander.Size = new Size(390, 20);
            txtCommander.TabIndex = 10;
            txtCommander.Text = "<cmdr name>";
            // 
            // txtNearBody
            // 
            txtNearBody.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            txtLocation.Location = new Point(16, 43);
            txtLocation.Name = "txtLocation";
            txtLocation.ReadOnly = true;
            txtLocation.Size = new Size(291, 20);
            txtLocation.TabIndex = 12;
            txtLocation.Text = "<location>";
            // 
            // txtVehicle
            // 
            txtVehicle.Location = new Point(16, 69);
            txtVehicle.Name = "txtVehicle";
            txtVehicle.ReadOnly = true;
            txtVehicle.Size = new Size(86, 20);
            txtVehicle.TabIndex = 11;
            txtVehicle.Text = "<vehicle>";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
            groupBox3.Location = new Point(12, 116);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(413, 130);
            groupBox3.TabIndex = 14;
            groupBox3.TabStop = false;
            groupBox3.Text = "Bio scanning";
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
            txtBodyBioValues.Enabled = false;
            txtBodyBioValues.Location = new Point(143, 81);
            txtBodyBioValues.Name = "txtBodyBioValues";
            txtBodyBioValues.ReadOnly = true;
            txtBodyBioValues.Size = new Size(263, 20);
            txtBodyBioValues.TabIndex = 26;
            // 
            // txtBodyBioSignals
            // 
            txtBodyBioSignals.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBodyBioSignals.Enabled = false;
            txtBodyBioSignals.Location = new Point(69, 81);
            txtBodyBioSignals.Name = "txtBodyBioSignals";
            txtBodyBioSignals.ReadOnly = true;
            txtBodyBioSignals.Size = new Size(110, 20);
            txtBodyBioSignals.TabIndex = 24;
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
            txtSystemBioValues.Enabled = false;
            txtSystemBioValues.Location = new Point(143, 55);
            txtSystemBioValues.Name = "txtSystemBioValues";
            txtSystemBioValues.ReadOnly = true;
            txtSystemBioValues.Size = new Size(263, 20);
            txtSystemBioValues.TabIndex = 22;
            // 
            // txtSystemBioSignals
            // 
            txtSystemBioSignals.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemBioSignals.Enabled = false;
            txtSystemBioSignals.Location = new Point(69, 55);
            txtSystemBioSignals.Name = "txtSystemBioSignals";
            txtSystemBioSignals.ReadOnly = true;
            txtSystemBioSignals.Size = new Size(110, 20);
            txtSystemBioSignals.TabIndex = 20;
            txtSystemBioSignals.Text = "99 of 99";
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
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // btnLogs
            // 
            btnLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogs.DialogResult = DialogResult.Cancel;
            btnLogs.FlatStyle = FlatStyle.System;
            btnLogs.Location = new Point(93, 512);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(75, 23);
            btnLogs.TabIndex = 15;
            btnLogs.Text = "&Logs";
            btnLogs.UseVisualStyleBackColor = true;
            btnLogs.Click += btnViewLogs_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSettings.DialogResult = DialogResult.Cancel;
            btnSettings.FlatStyle = FlatStyle.System;
            btnSettings.Location = new Point(12, 512);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(75, 23);
            btnSettings.TabIndex = 16;
            btnSettings.Text = "&Settings";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            linkLabel1.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point);
            linkLabel1.LinkArea = new LinkArea(13, 12);
            linkLabel1.Location = new Point(10, 462);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(415, 18);
            linkLabel1.TabIndex = 17;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "For guidance see the wiki on GitHub";
            linkLabel1.UseCompatibleTextRendering = true;
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // lblFullScreen
            // 
            lblFullScreen.BackColor = Color.DarkRed;
            lblFullScreen.Dock = DockStyle.Top;
            lblFullScreen.ForeColor = Color.White;
            lblFullScreen.Location = new Point(0, 102);
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
            groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox4.Controls.Add(btnGuarduanThings);
            groupBox4.Controls.Add(btnRuins);
            groupBox4.Controls.Add(btnRuinsMap);
            groupBox4.Controls.Add(btnRuinsOrigin);
            groupBox4.Controls.Add(txtGuardianSite);
            groupBox4.Controls.Add(lblGuardianCount);
            groupBox4.Controls.Add(label5);
            groupBox4.Location = new Point(12, 252);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(413, 94);
            groupBox4.TabIndex = 19;
            groupBox4.TabStop = false;
            groupBox4.Text = "Guardian sites: (experimental)";
            // 
            // btnGuarduanThings
            // 
            btnGuarduanThings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuarduanThings.BackColor = SystemColors.ControlLight;
            btnGuarduanThings.FlatStyle = FlatStyle.System;
            btnGuarduanThings.Location = new Point(282, 16);
            btnGuarduanThings.Name = "btnGuarduanThings";
            btnGuarduanThings.Size = new Size(124, 33);
            btnGuarduanThings.TabIndex = 19;
            btnGuarduanThings.Text = "All Guardian Sites";
            btnGuarduanThings.UseVisualStyleBackColor = false;
            btnGuarduanThings.Click += btnGuarduanThings_Click;
            // 
            // btnRuins
            // 
            btnRuins.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRuins.BackColor = SystemColors.ControlLight;
            btnRuins.FlatStyle = FlatStyle.System;
            btnRuins.Location = new Point(283, 55);
            btnRuins.Name = "btnRuins";
            btnRuins.Size = new Size(123, 35);
            btnRuins.TabIndex = 18;
            btnRuins.Text = "Survey Maps";
            btnRuins.UseVisualStyleBackColor = false;
            btnRuins.Click += btnRuins_Click;
            // 
            // btnRuinsMap
            // 
            btnRuinsMap.BackColor = SystemColors.ControlLight;
            btnRuinsMap.FlatStyle = FlatStyle.System;
            btnRuinsMap.Location = new Point(16, 57);
            btnRuinsMap.Name = "btnRuinsMap";
            btnRuinsMap.Size = new Size(86, 21);
            btnRuinsMap.TabIndex = 16;
            btnRuinsMap.Text = "Show map";
            btnRuinsMap.UseVisualStyleBackColor = false;
            btnRuinsMap.Click += btnRuinsMap_Click;
            // 
            // btnRuinsOrigin
            // 
            btnRuinsOrigin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRuinsOrigin.BackColor = SystemColors.ControlLight;
            btnRuinsOrigin.FlatStyle = FlatStyle.System;
            btnRuinsOrigin.Location = new Point(151, 57);
            btnRuinsOrigin.Name = "btnRuinsOrigin";
            btnRuinsOrigin.Size = new Size(126, 21);
            btnRuinsOrigin.TabIndex = 15;
            btnRuinsOrigin.Text = "Aerial assist";
            btnRuinsOrigin.UseVisualStyleBackColor = false;
            btnRuinsOrigin.Click += btnRuinsOrigin_Click;
            // 
            // txtGuardianSite
            // 
            txtGuardianSite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtGuardianSite.Location = new Point(16, 31);
            txtGuardianSite.Name = "txtGuardianSite";
            txtGuardianSite.ReadOnly = true;
            txtGuardianSite.Size = new Size(260, 20);
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
            lblNotInstalled.Location = new Point(0, 0);
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
            btnSphereLimit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSphereLimit.BackColor = SystemColors.ControlLight;
            btnSphereLimit.FlatStyle = FlatStyle.System;
            btnSphereLimit.Location = new Point(325, 397);
            btnSphereLimit.Name = "btnSphereLimit";
            btnSphereLimit.Size = new Size(100, 33);
            btnSphereLimit.TabIndex = 22;
            btnSphereLimit.Text = "Sphere limit";
            btnSphereLimit.UseVisualStyleBackColor = false;
            btnSphereLimit.Click += btnSphereLimit_Click;
            // 
            // linkLabel2
            // 
            linkLabel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            linkLabel2.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point);
            linkLabel2.LinkArea = new LinkArea(17, 22);
            linkLabel2.Location = new Point(10, 480);
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
            btnRamTah.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRamTah.BackColor = SystemColors.ControlLight;
            btnRamTah.FlatStyle = FlatStyle.System;
            btnRamTah.Location = new Point(325, 357);
            btnRamTah.Name = "btnRamTah";
            btnRamTah.Size = new Size(100, 33);
            btnRamTah.TabIndex = 24;
            btnRamTah.Text = "Ram Tah Missions";
            btnRamTah.UseVisualStyleBackColor = false;
            btnRamTah.Click += btnRamTah_Click;
            // 
            // btnPublish
            // 
            btnPublish.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPublish.Location = new Point(174, 512);
            btnPublish.Name = "btnPublish";
            btnPublish.Size = new Size(75, 23);
            btnPublish.TabIndex = 25;
            btnPublish.Text = "Publish";
            btnPublish.UseVisualStyleBackColor = true;
            btnPublish.Visible = false;
            btnPublish.Click += btnPublish_Click;
            // 
            // checkTempHide
            // 
            checkTempHide.AutoSize = true;
            checkTempHide.Location = new Point(12, 436);
            checkTempHide.Name = "checkTempHide";
            checkTempHide.Size = new Size(227, 16);
            checkTempHide.TabIndex = 26;
            checkTempHide.Text = "Temporarily hide all overlays";
            checkTempHide.UseVisualStyleBackColor = true;
            checkTempHide.CheckedChanged += checkTempHide_CheckedChanged;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnQuit2;
            ClientSize = new Size(437, 547);
            Controls.Add(checkTempHide);
            Controls.Add(btnPublish);
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
            Controls.Add(lblFullScreen);
            Controls.Add(lblNotInstalled);
            Font = new Font("Lucida Sans Typewriter", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Main";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Srv Survey";
            FormClosed += Main_FormClosed;
            Load += Main_Load;
            ResizeEnd += Main_ResizeEnd;
            SizeChanged += Main_SizeChanged;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
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
        private Button btnGuarduanThings;
        private LinkLabel linkLabel2;
        private CheckBox checkFirstFootFall;
        private Button btnRamTah;
        private Button btnPublish;
        private Button btnPasteLatLong;
        private CheckBox checkTempHide;
    }
}