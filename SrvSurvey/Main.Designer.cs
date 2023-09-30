
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
            lblTrackTargetStatus = new Label();
            groupBox2 = new GroupBox();
            txtNearBody = new TextBox();
            txtMode = new TextBox();
            txtLocation = new TextBox();
            txtVehicle = new TextBox();
            txtCommander = new TextBox();
            groupBox3 = new GroupBox();
            txtBodyBioValues = new TextBox();
            txtBodyBioScanned = new TextBox();
            txtBodyBioSignals = new TextBox();
            lblSysBio = new Label();
            txtSystemBioValues = new TextBox();
            txtSystemBioScanned = new TextBox();
            txtSystemBioSignals = new TextBox();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            txtBioRewards = new TextBox();
            label4 = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            btnLogs = new Button();
            btnSettings = new Button();
            linkLabel1 = new LinkLabel();
            lblFullScreen = new Label();
            groupBox4 = new GroupBox();
            btnRuins = new Button();
            btnAllRuins = new Button();
            btnRuinsMap = new Button();
            btnRuinsOrigin = new Button();
            txtGuardianSite = new TextBox();
            lblGuardianCount = new Label();
            label5 = new Label();
            lblNotInstalled = new LinkLabel();
            btnSphereLimit = new Button();
            btnGuarduanThings = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // btnGroundTarget
            // 
            btnGroundTarget.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGroundTarget.BackColor = SystemColors.ControlLight;
            btnGroundTarget.FlatStyle = FlatStyle.System;
            btnGroundTarget.Location = new Point(82, 45);
            btnGroundTarget.Name = "btnGroundTarget";
            btnGroundTarget.Size = new Size(119, 21);
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
            btnQuit2.Font = new Font("Century Gothic", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            btnQuit2.Location = new Point(308, 483);
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
            txtTargetLatLong.Size = new Size(281, 20);
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
            btnClearTarget.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearTarget.BackColor = SystemColors.ControlLight;
            btnClearTarget.FlatStyle = FlatStyle.System;
            btnClearTarget.Location = new Point(207, 45);
            btnClearTarget.Name = "btnClearTarget";
            btnClearTarget.Size = new Size(90, 21);
            btnClearTarget.TabIndex = 13;
            btnClearTarget.Text = "Hide";
            btnClearTarget.UseVisualStyleBackColor = false;
            btnClearTarget.Click += btnClearTarget_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(lblTrackTargetStatus);
            groupBox1.Controls.Add(btnClearTarget);
            groupBox1.Controls.Add(btnGroundTarget);
            groupBox1.Controls.Add(txtTargetLatLong);
            groupBox1.Location = new Point(12, 352);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(303, 78);
            groupBox1.TabIndex = 14;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target lat/Long:";
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
            groupBox2.Controls.Add(txtNearBody);
            groupBox2.Controls.Add(txtMode);
            groupBox2.Controls.Add(txtLocation);
            groupBox2.Controls.Add(txtVehicle);
            groupBox2.Controls.Add(txtCommander);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(371, 98);
            groupBox2.TabIndex = 14;
            groupBox2.TabStop = false;
            groupBox2.Text = "Commander:";
            // 
            // txtNearBody
            // 
            txtNearBody.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtNearBody.Location = new Point(278, 43);
            txtNearBody.Name = "txtNearBody";
            txtNearBody.ReadOnly = true;
            txtNearBody.Size = new Size(86, 20);
            txtNearBody.TabIndex = 14;
            txtNearBody.Text = "<near/far>";
            // 
            // txtMode
            // 
            txtMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMode.Location = new Point(108, 69);
            txtMode.Name = "txtMode";
            txtMode.ReadOnly = true;
            txtMode.Size = new Size(256, 20);
            txtMode.TabIndex = 13;
            txtMode.Text = "<mode>";
            // 
            // txtLocation
            // 
            txtLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLocation.Location = new Point(16, 43);
            txtLocation.Name = "txtLocation";
            txtLocation.ReadOnly = true;
            txtLocation.Size = new Size(256, 20);
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
            // txtCommander
            // 
            txtCommander.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommander.Location = new Point(16, 19);
            txtCommander.Name = "txtCommander";
            txtCommander.ReadOnly = true;
            txtCommander.Size = new Size(348, 20);
            txtCommander.TabIndex = 10;
            txtCommander.Text = "<cmdr name>";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(txtBodyBioValues);
            groupBox3.Controls.Add(txtBodyBioScanned);
            groupBox3.Controls.Add(txtBodyBioSignals);
            groupBox3.Controls.Add(lblSysBio);
            groupBox3.Controls.Add(txtSystemBioValues);
            groupBox3.Controls.Add(txtSystemBioScanned);
            groupBox3.Controls.Add(txtSystemBioSignals);
            groupBox3.Controls.Add(label9);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(txtBioRewards);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new Point(12, 116);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(371, 130);
            groupBox3.TabIndex = 14;
            groupBox3.TabStop = false;
            groupBox3.Text = "Bio scanning";
            // 
            // txtBodyBioValues
            // 
            txtBodyBioValues.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBodyBioValues.Location = new Point(208, 91);
            txtBodyBioValues.Name = "txtBodyBioValues";
            txtBodyBioValues.ReadOnly = true;
            txtBodyBioValues.Size = new Size(156, 20);
            txtBodyBioValues.TabIndex = 26;
            // 
            // txtBodyBioScanned
            // 
            txtBodyBioScanned.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBodyBioScanned.Location = new Point(148, 91);
            txtBodyBioScanned.Name = "txtBodyBioScanned";
            txtBodyBioScanned.ReadOnly = true;
            txtBodyBioScanned.Size = new Size(54, 20);
            txtBodyBioScanned.TabIndex = 25;
            // 
            // txtBodyBioSignals
            // 
            txtBodyBioSignals.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBodyBioSignals.Location = new Point(88, 91);
            txtBodyBioSignals.Name = "txtBodyBioSignals";
            txtBodyBioSignals.ReadOnly = true;
            txtBodyBioSignals.Size = new Size(54, 20);
            txtBodyBioSignals.TabIndex = 24;
            // 
            // lblSysBio
            // 
            lblSysBio.AutoSize = true;
            lblSysBio.Location = new Point(15, 68);
            lblSysBio.Name = "lblSysBio";
            lblSysBio.Size = new Size(54, 12);
            lblSysBio.TabIndex = 23;
            lblSysBio.Text = "System:";
            // 
            // txtSystemBioValues
            // 
            txtSystemBioValues.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemBioValues.Location = new Point(208, 65);
            txtSystemBioValues.Name = "txtSystemBioValues";
            txtSystemBioValues.ReadOnly = true;
            txtSystemBioValues.Size = new Size(156, 20);
            txtSystemBioValues.TabIndex = 22;
            // 
            // txtSystemBioScanned
            // 
            txtSystemBioScanned.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemBioScanned.Location = new Point(148, 65);
            txtSystemBioScanned.Name = "txtSystemBioScanned";
            txtSystemBioScanned.ReadOnly = true;
            txtSystemBioScanned.Size = new Size(54, 20);
            txtSystemBioScanned.TabIndex = 21;
            // 
            // txtSystemBioSignals
            // 
            txtSystemBioSignals.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemBioSignals.Location = new Point(88, 65);
            txtSystemBioSignals.Name = "txtSystemBioSignals";
            txtSystemBioSignals.ReadOnly = true;
            txtSystemBioSignals.Size = new Size(54, 20);
            txtSystemBioSignals.TabIndex = 20;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(205, 50);
            label9.Name = "label9";
            label9.Size = new Size(159, 12);
            label9.TabIndex = 19;
            label9.Text = "Total / scanned value:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(148, 50);
            label8.Name = "label8";
            label8.Size = new Size(54, 12);
            label8.TabIndex = 18;
            label8.Text = "Scanned";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(88, 50);
            label7.Name = "label7";
            label7.Size = new Size(54, 12);
            label7.TabIndex = 17;
            label7.Text = "Signals";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(16, 94);
            label6.Name = "label6";
            label6.Size = new Size(40, 12);
            label6.TabIndex = 16;
            label6.Text = "Body:";
            // 
            // txtBioRewards
            // 
            txtBioRewards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBioRewards.Location = new Point(143, 13);
            txtBioRewards.Name = "txtBioRewards";
            txtBioRewards.ReadOnly = true;
            txtBioRewards.Size = new Size(221, 20);
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
            btnLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLogs.DialogResult = DialogResult.Cancel;
            btnLogs.FlatStyle = FlatStyle.System;
            btnLogs.Font = new Font("Century Gothic", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            btnLogs.Location = new Point(93, 483);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(75, 23);
            btnLogs.TabIndex = 15;
            btnLogs.Text = "&Logs";
            btnLogs.UseVisualStyleBackColor = true;
            btnLogs.Click += btnViewLogs_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSettings.DialogResult = DialogResult.Cancel;
            btnSettings.FlatStyle = FlatStyle.System;
            btnSettings.Font = new Font("Century Gothic", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            btnSettings.Location = new Point(12, 483);
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
            linkLabel1.LinkArea = new LinkArea(16, 38);
            linkLabel1.Location = new Point(10, 441);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(373, 39);
            linkLabel1.TabIndex = 17;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "See guidance at https://njthomson.github.io/SrvSurvey/";
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
            lblFullScreen.Size = new Size(395, 86);
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
            groupBox4.Controls.Add(btnAllRuins);
            groupBox4.Controls.Add(btnRuinsMap);
            groupBox4.Controls.Add(btnRuinsOrigin);
            groupBox4.Controls.Add(txtGuardianSite);
            groupBox4.Controls.Add(lblGuardianCount);
            groupBox4.Controls.Add(label5);
            groupBox4.Location = new Point(12, 252);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(371, 94);
            groupBox4.TabIndex = 19;
            groupBox4.TabStop = false;
            groupBox4.Text = "Guardian sites: (experimental)";
            // 
            // btnRuins
            // 
            btnRuins.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRuins.BackColor = SystemColors.ControlLight;
            btnRuins.FlatStyle = FlatStyle.System;
            btnRuins.Location = new Point(309, 56);
            btnRuins.Name = "btnRuins";
            btnRuins.Size = new Size(56, 32);
            btnRuins.TabIndex = 18;
            btnRuins.Text = "Ruins Map";
            btnRuins.UseVisualStyleBackColor = false;
            btnRuins.Click += btnRuins_Click;
            // 
            // btnAllRuins
            // 
            btnAllRuins.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAllRuins.BackColor = SystemColors.ControlLight;
            btnAllRuins.FlatStyle = FlatStyle.System;
            btnAllRuins.Location = new Point(240, 55);
            btnAllRuins.Name = "btnAllRuins";
            btnAllRuins.Size = new Size(55, 35);
            btnAllRuins.TabIndex = 17;
            btnAllRuins.Text = "All Ruins";
            btnAllRuins.UseVisualStyleBackColor = false;
            btnAllRuins.Click += btnAllRuins_Click;
            // 
            // btnRuinsMap
            // 
            btnRuinsMap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            btnRuinsOrigin.Location = new Point(108, 57);
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
            txtGuardianSite.Size = new Size(218, 20);
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
            lblNotInstalled.Size = new Size(395, 102);
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
            btnSphereLimit.Location = new Point(321, 352);
            btnSphereLimit.Name = "btnSphereLimit";
            btnSphereLimit.Size = new Size(55, 78);
            btnSphereLimit.TabIndex = 22;
            btnSphereLimit.Text = "Sphere limit";
            btnSphereLimit.UseVisualStyleBackColor = false;
            btnSphereLimit.Click += btnSphereLimit_Click;
            // 
            // btnGuarduanThings
            // 
            btnGuarduanThings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuarduanThings.BackColor = SystemColors.ControlLight;
            btnGuarduanThings.FlatStyle = FlatStyle.System;
            btnGuarduanThings.Location = new Point(240, 16);
            btnGuarduanThings.Name = "btnGuarduanThings";
            btnGuarduanThings.Size = new Size(124, 34);
            btnGuarduanThings.TabIndex = 19;
            btnGuarduanThings.Text = "All Beacons and Structures";
            btnGuarduanThings.UseVisualStyleBackColor = false;
            btnGuarduanThings.Click += btnGuarduanThings_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnQuit2;
            ClientSize = new Size(395, 518);
            Controls.Add(btnSphereLimit);
            Controls.Add(lblNotInstalled);
            Controls.Add(groupBox3);
            Controls.Add(groupBox4);
            Controls.Add(lblFullScreen);
            Controls.Add(linkLabel1);
            Controls.Add(btnSettings);
            Controls.Add(btnLogs);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(label2);
            Controls.Add(btnQuit2);
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
            DoubleClick += Main_DoubleClick;
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
        private TextBox txtBodyBioScanned;
        private TextBox txtBodyBioSignals;
        private Label lblSysBio;
        private TextBox txtSystemBioValues;
        private TextBox txtSystemBioScanned;
        private TextBox txtSystemBioSignals;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Button btnRuinsMap;
        private Button btnRuinsOrigin;
        private Button btnAllRuins;
        private Button btnRuins;
        private Button btnSphereLimit;
        private Button btnGuarduanThings;
    }
}