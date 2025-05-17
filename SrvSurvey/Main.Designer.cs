
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

                this.stopHooks();

                if (this.game != null)
                    this.removeGame();
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
            btnClearTarget = new Button();
            groupBox1 = new GroupBox();
            btnPasteLatLong = new Button();
            lblTrackTargetStatus = new Label();
            groupCmdr = new GroupBox();
            btnNextWindow = new Button();
            btnCopyLocation = new Button();
            txtCommander = new TextBox();
            txtNearBody = new TextBox();
            txtMode = new TextBox();
            txtLocation = new TextBox();
            txtVehicle = new TextBox();
            groupBox3 = new GroupBox();
            btnPredictions = new Button();
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
            lblNotInstalled = new LinkLabel();
            linkLabel2 = new LinkLabel();
            checkTempHide = new CheckBox();
            groupBox5 = new GroupBox();
            txtExplorationValue = new TextBox();
            txtDistance = new TextBox();
            txtJumps = new TextBox();
            label6 = new Label();
            label3 = new Label();
            btnResetExploration = new Button();
            label1 = new Label();
            txtBodies = new TextBox();
            linkNewBuildAvailable = new LinkLabel();
            groupCodex = new GroupBox();
            btnCodexBingo = new Button();
            menuSearchTools = new ButtonContextMenuStrip(components);
            menuSpherical = new ToolStripMenuItem();
            menuBoxel = new ToolStripMenuItem();
            btnSearch = new FlatButton();
            lblBig = new Label();
            comboDev = new ComboBox();
            menuJourney = new ButtonContextMenuStrip(components);
            menuSetLatLong = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuJourneyBegin = new ToolStripMenuItem();
            menuJourneyNotes = new ToolStripMenuItem();
            menuJourneyReview = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuFollowRoute = new ToolStripMenuItem();
            btnTravel = new FlatButton();
            btnColonize = new FlatButton();
            btnGuardian = new FlatButton();
            menuColonize = new ButtonContextMenuStrip(components);
            menuRefreshProjects = new ToolStripMenuItem();
            menuColonizeLine1 = new ToolStripSeparator();
            menuMyProjects = new ToolStripMenuItem();
            menuCurrentProject = new ToolStripMenuItem();
            menuPrimaryProject = new ToolStripMenuItem();
            menuColonizeLine2 = new ToolStripSeparator();
            menuNewProject = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            menuColonizeWiki = new ToolStripMenuItem();
            menuGuardians = new ButtonContextMenuStrip(components);
            btnGuardianThings = new ToolStripMenuItem();
            btnRuins = new ToolStripMenuItem();
            btnRamTah = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            btnRuinsMap = new ToolStripMenuItem();
            btnRuinsOrigin = new ToolStripMenuItem();
            toolTip1 = new ToolTip(components);
            notifyIcon = new NotifyIcon(components);
            menuNotify = new ContextMenuStrip(components);
            menuNotifyCmdr = new ToolStripMenuItem();
            menuNotifyNextWindow = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            menuNotifyCopy = new ToolStripMenuItem();
            menuNotifyShowMain = new ToolStripMenuItem();
            menuNotifyLogs = new ToolStripMenuItem();
            menuNotifySettings = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            menuNotifyClose = new ToolStripMenuItem();
            groupBox1.SuspendLayout();
            groupCmdr.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox5.SuspendLayout();
            groupCodex.SuspendLayout();
            menuSearchTools.SuspendLayout();
            menuJourney.SuspendLayout();
            menuColonize.SuspendLayout();
            menuGuardians.SuspendLayout();
            menuNotify.SuspendLayout();
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
            btnGroundTarget.TabIndex = 2;
            btnGroundTarget.Text = "Set";
            btnGroundTarget.UseVisualStyleBackColor = false;
            btnGroundTarget.Click += btnGroundTarget_Click;
            // 
            // btnQuit2
            // 
            btnQuit2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnQuit2.BackColor = SystemColors.ControlDark;
            btnQuit2.DialogResult = DialogResult.Cancel;
            btnQuit2.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnQuit2.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnQuit2.FlatStyle = FlatStyle.Flat;
            btnQuit2.Location = new Point(350, 513);
            btnQuit2.Name = "btnQuit2";
            btnQuit2.Size = new Size(75, 23);
            btnQuit2.TabIndex = 16;
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
            txtTargetLatLong.TabIndex = 0;
            txtTargetLatLong.Text = "<none>";
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
            btnClearTarget.TabIndex = 3;
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
            groupBox1.Location = new Point(430, 342);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(214, 78);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target lat/Long:";
            groupBox1.Visible = false;
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
            btnPasteLatLong.TabIndex = 4;
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
            lblTrackTargetStatus.TabIndex = 1;
            lblTrackTargetStatus.Text = "<status>";
            // 
            // groupCmdr
            // 
            groupCmdr.Controls.Add(btnNextWindow);
            groupCmdr.Controls.Add(btnCopyLocation);
            groupCmdr.Controls.Add(txtCommander);
            groupCmdr.Controls.Add(txtNearBody);
            groupCmdr.Controls.Add(txtMode);
            groupCmdr.Controls.Add(txtLocation);
            groupCmdr.Controls.Add(txtVehicle);
            groupCmdr.Location = new Point(12, 12);
            groupCmdr.Name = "groupCmdr";
            groupCmdr.Size = new Size(413, 98);
            groupCmdr.TabIndex = 2;
            groupCmdr.TabStop = false;
            groupCmdr.Text = "Commander:";
            // 
            // btnNextWindow
            // 
            btnNextWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNextWindow.BackColor = SystemColors.ControlDark;
            btnNextWindow.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnNextWindow.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnNextWindow.FlatStyle = FlatStyle.Flat;
            btnNextWindow.Font = new Font("Segoe UI Emoji", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnNextWindow.Image = Properties.ImageResources.next_window;
            btnNextWindow.Location = new Point(386, 19);
            btnNextWindow.Name = "btnNextWindow";
            btnNextWindow.Size = new Size(20, 20);
            btnNextWindow.TabIndex = 6;
            toolTip1.SetToolTip(btnNextWindow, "Switch overlays to the next game window");
            btnNextWindow.UseVisualStyleBackColor = false;
            btnNextWindow.Visible = false;
            btnNextWindow.Click += btnNextWindow_Click;
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
            btnCopyLocation.TabIndex = 2;
            toolTip1.SetToolTip(btnCopyLocation, "Copy current system/body name to clipboard");
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
            txtCommander.TabIndex = 0;
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
            txtNearBody.TabIndex = 3;
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
            txtMode.TabIndex = 5;
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
            txtLocation.TabIndex = 1;
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
            txtVehicle.TabIndex = 4;
            txtVehicle.Text = "<vehicle>";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnPredictions);
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
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "Bio scanning:";
            // 
            // btnPredictions
            // 
            btnPredictions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPredictions.BackColor = SystemColors.ControlDark;
            btnPredictions.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnPredictions.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnPredictions.FlatStyle = FlatStyle.Flat;
            btnPredictions.Location = new Point(313, 103);
            btnPredictions.Name = "btnPredictions";
            btnPredictions.Size = new Size(93, 21);
            btnPredictions.TabIndex = 0;
            btnPredictions.Text = "Predictions";
            toolTip1.SetToolTip(btnPredictions, "View which species are expected in the current system");
            btnPredictions.UseVisualStyleBackColor = false;
            btnPredictions.Click += btnPredictions_Click;
            // 
            // checkFirstFootFall
            // 
            checkFirstFootFall.AutoSize = true;
            checkFirstFootFall.Enabled = false;
            checkFirstFootFall.FlatStyle = FlatStyle.System;
            checkFirstFootFall.Location = new Point(143, 107);
            checkFirstFootFall.Name = "checkFirstFootFall";
            checkFirstFootFall.Size = new Size(128, 17);
            checkFirstFootFall.TabIndex = 10;
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
            txtBodyBioValues.TabIndex = 9;
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
            txtBodyBioSignals.TabIndex = 8;
            txtBodyBioSignals.TextAlign = HorizontalAlignment.Center;
            // 
            // lblSysBio
            // 
            lblSysBio.AutoSize = true;
            lblSysBio.Enabled = false;
            lblSysBio.Location = new Point(8, 58);
            lblSysBio.Name = "lblSysBio";
            lblSysBio.Size = new Size(54, 12);
            lblSysBio.TabIndex = 3;
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
            txtSystemBioValues.TabIndex = 5;
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
            txtSystemBioSignals.TabIndex = 4;
            txtSystemBioSignals.Text = "99 of 99";
            txtSystemBioSignals.TextAlign = HorizontalAlignment.Center;
            // 
            // labelSignalsAndRewards
            // 
            labelSignalsAndRewards.AutoSize = true;
            labelSignalsAndRewards.Location = new Point(71, 38);
            labelSignalsAndRewards.Name = "labelSignalsAndRewards";
            labelSignalsAndRewards.Size = new Size(201, 12);
            labelSignalsAndRewards.TabIndex = 2;
            labelSignalsAndRewards.Text = "Scanned signals and rewards:";
            // 
            // lblBodyBio
            // 
            lblBodyBio.AutoSize = true;
            lblBodyBio.Enabled = false;
            lblBodyBio.Location = new Point(22, 84);
            lblBodyBio.Name = "lblBodyBio";
            lblBodyBio.Size = new Size(40, 12);
            lblBodyBio.TabIndex = 6;
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
            txtBioRewards.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 16);
            label4.Name = "label4";
            label4.Size = new Size(131, 12);
            label4.TabIndex = 0;
            label4.Text = "Unclaimed rewards:";
            // 
            // btnCodexShow
            // 
            btnCodexShow.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCodexShow.BackColor = SystemColors.ControlDark;
            btnCodexShow.Enabled = false;
            btnCodexShow.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnCodexShow.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnCodexShow.FlatStyle = FlatStyle.Flat;
            btnCodexShow.Image = (Image)resources.GetObject("btnCodexShow.Image");
            btnCodexShow.Location = new Point(379, 437);
            btnCodexShow.Name = "btnCodexShow";
            btnCodexShow.Size = new Size(46, 36);
            btnCodexShow.TabIndex = 12;
            toolTip1.SetToolTip(btnCodexShow, "See stock images of species expected on this planet.");
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
            btnLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogs.BackColor = SystemColors.ControlDark;
            btnLogs.DialogResult = DialogResult.Cancel;
            btnLogs.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnLogs.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnLogs.FlatStyle = FlatStyle.Flat;
            btnLogs.Location = new Point(93, 513);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(75, 23);
            btnLogs.TabIndex = 14;
            btnLogs.Text = "&Logs";
            btnLogs.UseVisualStyleBackColor = false;
            btnLogs.Click += btnLogs_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSettings.BackColor = SystemColors.ControlDark;
            btnSettings.DialogResult = DialogResult.Cancel;
            btnSettings.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnSettings.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Location = new Point(12, 513);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(75, 23);
            btnSettings.TabIndex = 13;
            btnSettings.Text = "&Settings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel1.Font = new Font("Lucida Sans Typewriter", 9F);
            linkLabel1.LinkArea = new LinkArea(13, 12);
            linkLabel1.Location = new Point(12, 461);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(415, 18);
            linkLabel1.TabIndex = 9;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "For guidance see the wiki on GitHub";
            linkLabel1.TextAlign = ContentAlignment.MiddleLeft;
            linkLabel1.UseCompatibleTextRendering = true;
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // lblFullScreen
            // 
            lblFullScreen.BackColor = Color.OrangeRed;
            lblFullScreen.Dock = DockStyle.Top;
            lblFullScreen.ForeColor = Color.Black;
            lblFullScreen.Location = new Point(0, 0);
            lblFullScreen.Name = "lblFullScreen";
            lblFullScreen.Padding = new Padding(10);
            lblFullScreen.Size = new Size(437, 86);
            lblFullScreen.TabIndex = 0;
            lblFullScreen.Tag = "NoTheme";
            lblFullScreen.Text = "SrvSurvey cannot be used when Elite Dangerous is in Full Screen mode.\r\n\r\nPlease go to Options > Graphics > Display and change setting FULLSCREEN to either BORDERLESS or WINDOWED.";
            lblFullScreen.TextAlign = ContentAlignment.TopCenter;
            lblFullScreen.Visible = false;
            // 
            // lblNotInstalled
            // 
            lblNotInstalled.BackColor = Color.OrangeRed;
            lblNotInstalled.Dock = DockStyle.Top;
            lblNotInstalled.Font = new Font("Lucida Sans Typewriter", 9F);
            lblNotInstalled.ForeColor = Color.Black;
            lblNotInstalled.LinkArea = new LinkArea(57, 15);
            lblNotInstalled.LinkColor = Color.FromArgb(128, 255, 255);
            lblNotInstalled.Location = new Point(0, 86);
            lblNotInstalled.Name = "lblNotInstalled";
            lblNotInstalled.Padding = new Padding(10);
            lblNotInstalled.Size = new Size(437, 102);
            lblNotInstalled.TabIndex = 1;
            lblNotInstalled.TabStop = true;
            lblNotInstalled.Tag = "NoTheme";
            lblNotInstalled.Text = "This application is intended for use only with the game \"Elite Dangerous\" by Frontier Developments.\r\n\r\nIt does not appear the game is installed.";
            lblNotInstalled.TextAlign = ContentAlignment.TopCenter;
            lblNotInstalled.UseCompatibleTextRendering = true;
            lblNotInstalled.Visible = false;
            lblNotInstalled.LinkClicked += lblNotInstalled_LinkClicked;
            // 
            // linkLabel2
            // 
            linkLabel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel2.Font = new Font("Lucida Sans Typewriter", 9F);
            linkLabel2.LinkArea = new LinkArea(17, 22);
            linkLabel2.Location = new Point(10, 481);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(417, 29);
            linkLabel2.TabIndex = 10;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "Ask questions at Guardian Science Corps on Discord";
            linkLabel2.UseCompatibleTextRendering = true;
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            // 
            // checkTempHide
            // 
            checkTempHide.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkTempHide.Location = new Point(12, 437);
            checkTempHide.Name = "checkTempHide";
            checkTempHide.Size = new Size(227, 16);
            checkTempHide.TabIndex = 8;
            checkTempHide.Text = "Temporarily hide all overlays";
            checkTempHide.UseVisualStyleBackColor = true;
            checkTempHide.CheckedChanged += checkTempHide_CheckedChanged;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(txtExplorationValue);
            groupBox5.Controls.Add(txtDistance);
            groupBox5.Controls.Add(txtJumps);
            groupBox5.Controls.Add(label6);
            groupBox5.Controls.Add(label3);
            groupBox5.Controls.Add(btnResetExploration);
            groupBox5.Controls.Add(label1);
            groupBox5.Controls.Add(txtBodies);
            groupBox5.Location = new Point(12, 116);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(413, 72);
            groupBox5.TabIndex = 3;
            groupBox5.TabStop = false;
            groupBox5.Text = "Exploration trip counter:";
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
            txtExplorationValue.TabIndex = 1;
            txtExplorationValue.Text = "-";
            txtExplorationValue.TextAlign = HorizontalAlignment.Center;
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
            txtDistance.TabIndex = 4;
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
            txtJumps.TabIndex = 3;
            txtJumps.Text = "-";
            txtJumps.TextAlign = HorizontalAlignment.Center;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(67, 45);
            label6.Name = "label6";
            label6.Size = new Size(54, 12);
            label6.TabIndex = 6;
            label6.Text = "Bodies:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(205, 19);
            label3.Name = "label3";
            label3.Size = new Size(47, 12);
            label3.TabIndex = 2;
            label3.Text = "Jumps:";
            // 
            // btnResetExploration
            // 
            btnResetExploration.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnResetExploration.BackColor = SystemColors.ControlDark;
            btnResetExploration.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnResetExploration.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnResetExploration.FlatStyle = FlatStyle.Flat;
            btnResetExploration.Location = new Point(8, 40);
            btnResetExploration.Name = "btnResetExploration";
            btnResetExploration.Size = new Size(53, 21);
            btnResetExploration.TabIndex = 5;
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
            label1.TabIndex = 0;
            label1.Text = "Estimated value:";
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
            txtBodies.TabIndex = 7;
            txtBodies.Text = "-";
            // 
            // linkNewBuildAvailable
            // 
            linkNewBuildAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            linkNewBuildAvailable.BackColor = Color.Transparent;
            linkNewBuildAvailable.Font = new Font("Lucida Sans Typewriter", 8.25F);
            linkNewBuildAvailable.LinkArea = new LinkArea(1, 16);
            linkNewBuildAvailable.Location = new Point(265, 0);
            linkNewBuildAvailable.Name = "linkNewBuildAvailable";
            linkNewBuildAvailable.Size = new Size(170, 18);
            linkNewBuildAvailable.TabIndex = 17;
            linkNewBuildAvailable.TabStop = true;
            linkNewBuildAvailable.Text = "(update available)";
            linkNewBuildAvailable.TextAlign = ContentAlignment.MiddleRight;
            linkNewBuildAvailable.UseCompatibleTextRendering = true;
            linkNewBuildAvailable.Visible = false;
            linkNewBuildAvailable.LinkClicked += linkNewBuildAvailable_LinkClicked;
            // 
            // groupCodex
            // 
            groupCodex.Controls.Add(btnCodexBingo);
            groupCodex.Location = new Point(10, 334);
            groupCodex.Name = "groupCodex";
            groupCodex.Size = new Size(87, 92);
            groupCodex.TabIndex = 6;
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
            btnCodexBingo.Location = new Point(6, 65);
            btnCodexBingo.Name = "btnCodexBingo";
            btnCodexBingo.Size = new Size(75, 21);
            btnCodexBingo.TabIndex = 0;
            btnCodexBingo.Text = "Bingo";
            toolTip1.SetToolTip(btnCodexBingo, "See how much of everything you have scanned");
            btnCodexBingo.UseVisualStyleBackColor = false;
            btnCodexBingo.Click += btnCodexBingo_Click;
            // 
            // menuSearchTools
            // 
            menuSearchTools.BackColor = SystemColors.ControlLight;
            menuSearchTools.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            menuSearchTools.Items.AddRange(new ToolStripItem[] { menuSpherical, menuBoxel });
            menuSearchTools.Name = "menuSearchTools";
            menuSearchTools.RenderMode = ToolStripRenderMode.System;
            menuSearchTools.Size = new Size(203, 112);
            menuSearchTools.targetButton = btnSearch;
            // 
            // menuSpherical
            // 
            menuSpherical.Image = Properties.ImageResources.spherical_48;
            menuSpherical.ImageScaling = ToolStripItemImageScaling.None;
            menuSpherical.ImageTransparentColor = Color.White;
            menuSpherical.Name = "menuSpherical";
            menuSpherical.Size = new Size(202, 54);
            menuSpherical.Text = "Spherical";
            menuSpherical.ToolTipText = "Search within a spherical area of space, around a central location.";
            menuSpherical.Click += menuSpherical_Click;
            // 
            // menuBoxel
            // 
            menuBoxel.Image = Properties.ImageResources.boxel_48;
            menuBoxel.ImageScaling = ToolStripItemImageScaling.None;
            menuBoxel.ImageTransparentColor = Color.White;
            menuBoxel.Name = "menuBoxel";
            menuBoxel.Size = new Size(202, 54);
            menuBoxel.Text = "Boxel";
            menuBoxel.ToolTipText = "Search every system within a named boxel.";
            menuBoxel.Click += menuBoxel_Click;
            // 
            // btnSearch
            // 
            btnSearch.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnSearch.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnSearch.Font = new Font("Segoe UI Emoji", 15.75F);
            btnSearch.Location = new Point(102, 340);
            btnSearch.Margin = new Padding(2, 4, 2, 4);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(150, 40);
            btnSearch.TabIndex = 22;
            btnSearch.Text = "🔎 Search";
            toolTip1.SetToolTip(btnSearch, "Tools to search for systems in spheres or cubes of space");
            btnSearch.UseVisualStyleBackColor = false;
            // 
            // lblBig
            // 
            lblBig.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblBig.AutoSize = true;
            lblBig.Font = new Font("Century Gothic", 21.75F, FontStyle.Bold);
            lblBig.Location = new Point(300, 437);
            lblBig.Name = "lblBig";
            lblBig.Size = new Size(72, 36);
            lblBig.TabIndex = 11;
            lblBig.Text = "20%";
            lblBig.Visible = false;
            // 
            // comboDev
            // 
            comboDev.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            comboDev.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDev.FlatStyle = FlatStyle.System;
            comboDev.FormattingEnabled = true;
            comboDev.Location = new Point(174, 515);
            comboDev.Name = "comboDev";
            comboDev.Size = new Size(170, 20);
            comboDev.TabIndex = 15;
            comboDev.Visible = false;
            comboDev.SelectedIndexChanged += comboDev_SelectedIndexChanged;
            // 
            // menuJourney
            // 
            menuJourney.BackColor = SystemColors.ControlLight;
            menuJourney.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            menuJourney.Items.AddRange(new ToolStripItem[] { menuSetLatLong, toolStripSeparator1, menuJourneyBegin, menuJourneyNotes, menuJourneyReview, toolStripSeparator2, menuFollowRoute });
            menuJourney.Name = "menuJourney";
            menuJourney.RenderMode = ToolStripRenderMode.System;
            menuJourney.ShowImageMargin = false;
            menuJourney.Size = new Size(203, 146);
            menuJourney.targetButton = btnTravel;
            menuJourney.Opening += menuJourney_Opening;
            // 
            // menuSetLatLong
            // 
            menuSetLatLong.Name = "menuSetLatLong";
            menuSetLatLong.Size = new Size(202, 26);
            menuSetLatLong.Text = "Target lat/long ...";
            menuSetLatLong.ToolTipText = "Clear or set target lat/long co-ordinated";
            menuSetLatLong.Click += btnGroundTarget_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(199, 6);
            // 
            // menuJourneyBegin
            // 
            menuJourneyBegin.Name = "menuJourneyBegin";
            menuJourneyBegin.Size = new Size(202, 26);
            menuJourneyBegin.Text = "Start a new journey ...";
            menuJourneyBegin.Click += menuJourneyBegin_Click;
            // 
            // menuJourneyNotes
            // 
            menuJourneyNotes.Name = "menuJourneyNotes";
            menuJourneyNotes.Size = new Size(202, 26);
            menuJourneyNotes.Text = "System Notes ...";
            menuJourneyNotes.Click += menuJourneyNotes_Click;
            // 
            // menuJourneyReview
            // 
            menuJourneyReview.Name = "menuJourneyReview";
            menuJourneyReview.Size = new Size(202, 26);
            menuJourneyReview.Text = "View Journey ...";
            menuJourneyReview.Click += menuJourneyReview_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(199, 6);
            // 
            // menuFollowRoute
            // 
            menuFollowRoute.Name = "menuFollowRoute";
            menuFollowRoute.Size = new Size(202, 26);
            menuFollowRoute.Text = "Follow a route ...";
            menuFollowRoute.Click += menuFollowRoute_Click;
            // 
            // btnTravel
            // 
            btnTravel.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnTravel.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnTravel.Font = new Font("Segoe UI Emoji", 15.75F);
            btnTravel.Location = new Point(102, 384);
            btnTravel.Margin = new Padding(2, 4, 2, 4);
            btnTravel.Name = "btnTravel";
            btnTravel.Size = new Size(150, 40);
            btnTravel.TabIndex = 24;
            btnTravel.Text = "🚀 Travel";
            toolTip1.SetToolTip(btnTravel, "Tools to navigate to specific locations, or keep records of a long journey or expedition");
            btnTravel.UseVisualStyleBackColor = false;
            // 
            // btnColonize
            // 
            btnColonize.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnColonize.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnColonize.Font = new Font("Segoe UI Emoji", 15.75F);
            btnColonize.Location = new Point(256, 384);
            btnColonize.Margin = new Padding(2, 4, 2, 4);
            btnColonize.Name = "btnColonize";
            btnColonize.Size = new Size(169, 40);
            btnColonize.TabIndex = 25;
            btnColonize.Text = "🚧 Colonise";
            toolTip1.SetToolTip(btnColonize, "Tools and features to help with system colonisation");
            btnColonize.UseVisualStyleBackColor = false;
            btnColonize.Click += btnColonize_Click;
            // 
            // btnGuardian
            // 
            btnGuardian.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            btnGuardian.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
            btnGuardian.Font = new Font("Segoe UI Emoji", 15.75F);
            btnGuardian.Location = new Point(256, 340);
            btnGuardian.Margin = new Padding(2, 4, 2, 4);
            btnGuardian.Name = "btnGuardian";
            btnGuardian.Size = new Size(169, 40);
            btnGuardian.TabIndex = 23;
            btnGuardian.Text = "🗿 Guardian";
            btnGuardian.TextImageRelation = TextImageRelation.ImageBeforeText;
            toolTip1.SetToolTip(btnGuardian, "Tools to help find, navigate and survey Guardian sites");
            btnGuardian.UseVisualStyleBackColor = true;
            // 
            // menuColonize
            // 
            menuColonize.BackColor = SystemColors.ControlLight;
            menuColonize.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            menuColonize.Items.AddRange(new ToolStripItem[] { menuRefreshProjects, menuColonizeLine1, menuMyProjects, menuCurrentProject, menuPrimaryProject, menuColonizeLine2, menuNewProject, toolStripSeparator6, menuColonizeWiki });
            menuColonize.Name = "menuColonize";
            menuColonize.RenderMode = ToolStripRenderMode.System;
            menuColonize.Size = new Size(220, 250);
            menuColonize.targetButton = btnColonize;
            // 
            // menuRefreshProjects
            // 
            menuRefreshProjects.Name = "menuRefreshProjects";
            menuRefreshProjects.Size = new Size(219, 38);
            menuRefreshProjects.Text = "Refresh data";
            menuRefreshProjects.ToolTipText = "Re-fetch colonisation data";
            menuRefreshProjects.Click += menuRefreshProjects_Click;
            // 
            // menuColonizeLine1
            // 
            menuColonizeLine1.Name = "menuColonizeLine1";
            menuColonizeLine1.Size = new Size(216, 6);
            // 
            // menuMyProjects
            // 
            menuMyProjects.Image = Properties.ImageResources.rcc_32;
            menuMyProjects.ImageScaling = ToolStripItemImageScaling.None;
            menuMyProjects.Name = "menuMyProjects";
            menuMyProjects.Size = new Size(219, 38);
            menuMyProjects.Text = "My projects";
            menuMyProjects.ToolTipText = "View all your current projects";
            menuMyProjects.Click += menuMyProjects_Click;
            // 
            // menuCurrentProject
            // 
            menuCurrentProject.Image = Properties.ImageResources.ruler;
            menuCurrentProject.ImageScaling = ToolStripItemImageScaling.None;
            menuCurrentProject.Name = "menuCurrentProject";
            menuCurrentProject.Size = new Size(219, 38);
            menuCurrentProject.Text = "Local project";
            menuCurrentProject.ToolTipText = "View the local or your primary project";
            menuCurrentProject.Click += menuCurrentProject_Click;
            // 
            // menuPrimaryProject
            // 
            menuPrimaryProject.Name = "menuPrimaryProject";
            menuPrimaryProject.Size = new Size(219, 38);
            menuPrimaryProject.Text = "Set primary";
            menuPrimaryProject.Click += menuPrimaryProject_Click;
            // 
            // menuColonizeLine2
            // 
            menuColonizeLine2.Name = "menuColonizeLine2";
            menuColonizeLine2.Size = new Size(216, 6);
            // 
            // menuNewProject
            // 
            menuNewProject.Image = Properties.ImageResources.ruler;
            menuNewProject.ImageScaling = ToolStripItemImageScaling.None;
            menuNewProject.Name = "menuNewProject";
            menuNewProject.Size = new Size(219, 38);
            menuNewProject.Text = "New project ...";
            menuNewProject.ToolTipText = "Create a new build project";
            menuNewProject.Click += menuNewProject_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(216, 6);
            // 
            // menuColonizeWiki
            // 
            menuColonizeWiki.Name = "menuColonizeWiki";
            menuColonizeWiki.Size = new Size(219, 38);
            menuColonizeWiki.Text = "See the wiki";
            menuColonizeWiki.Click += menuColonizeWiki_Click;
            // 
            // menuGuardians
            // 
            menuGuardians.BackColor = SystemColors.ControlLight;
            menuGuardians.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            menuGuardians.Items.AddRange(new ToolStripItem[] { btnGuardianThings, btnRuins, btnRamTah, toolStripSeparator5, btnRuinsMap, btnRuinsOrigin });
            menuGuardians.Name = "menuGuardians";
            menuGuardians.RenderMode = ToolStripRenderMode.System;
            menuGuardians.Size = new Size(242, 200);
            menuGuardians.targetButton = btnGuardian;
            // 
            // btnGuardianThings
            // 
            btnGuardianThings.Image = Properties.ImageResources.ticket;
            btnGuardianThings.ImageScaling = ToolStripItemImageScaling.None;
            btnGuardianThings.Name = "btnGuardianThings";
            btnGuardianThings.Size = new Size(241, 38);
            btnGuardianThings.Text = "Guardian Sites";
            btnGuardianThings.Click += btnGuardianThings_Click;
            // 
            // btnRuins
            // 
            btnRuins.Image = Properties.ImageResources.maps;
            btnRuins.ImageScaling = ToolStripItemImageScaling.None;
            btnRuins.Name = "btnRuins";
            btnRuins.Size = new Size(241, 38);
            btnRuins.Text = "Survey Maps";
            btnRuins.Click += btnRuins_Click;
            // 
            // btnRamTah
            // 
            btnRamTah.Image = Properties.ImageResources.moai;
            btnRamTah.ImageScaling = ToolStripItemImageScaling.None;
            btnRamTah.Name = "btnRamTah";
            btnRamTah.Size = new Size(241, 38);
            btnRamTah.Text = "Ram Tah Mission";
            btnRamTah.Click += btnRamTah_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(238, 6);
            // 
            // btnRuinsMap
            // 
            btnRuinsMap.Name = "btnRuinsMap";
            btnRuinsMap.Size = new Size(241, 38);
            btnRuinsMap.Text = "Show Map";
            btnRuinsMap.Click += btnRuinsMap_Click;
            // 
            // btnRuinsOrigin
            // 
            btnRuinsOrigin.Name = "btnRuinsOrigin";
            btnRuinsOrigin.Size = new Size(241, 38);
            btnRuinsOrigin.Text = "Aerial Assist";
            btnRuinsOrigin.Click += btnRuinsOrigin_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = menuNotify;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "SrvSurvey\r\nDouble click to restore";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += menuNotifyShowMain_Click;
            // 
            // menuNotify
            // 
            menuNotify.Items.AddRange(new ToolStripItem[] { menuNotifyCmdr, menuNotifyNextWindow, toolStripSeparator4, menuNotifyCopy, menuNotifyShowMain, menuNotifyLogs, menuNotifySettings, toolStripSeparator3, menuNotifyClose });
            menuNotify.Name = "menuNotify";
            menuNotify.Size = new Size(145, 170);
            // 
            // menuNotifyCmdr
            // 
            menuNotifyCmdr.Enabled = false;
            menuNotifyCmdr.Name = "menuNotifyCmdr";
            menuNotifyCmdr.Size = new Size(144, 22);
            menuNotifyCmdr.Text = "Cmdr: xxx";
            menuNotifyCmdr.ToolTipText = "Commander tracked by this SrvSurvey";
            // 
            // menuNotifyNextWindow
            // 
            menuNotifyNextWindow.Name = "menuNotifyNextWindow";
            menuNotifyNextWindow.Size = new Size(144, 22);
            menuNotifyNextWindow.Text = "Next window";
            menuNotifyNextWindow.ToolTipText = "When there are multiple game windows - shift overlays to the next game window";
            menuNotifyNextWindow.Visible = false;
            menuNotifyNextWindow.Click += menuNotifyNextWindow_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(141, 6);
            // 
            // menuNotifyCopy
            // 
            menuNotifyCopy.Name = "menuNotifyCopy";
            menuNotifyCopy.Size = new Size(144, 22);
            menuNotifyCopy.Text = "Copy current";
            menuNotifyCopy.ToolTipText = "Copy current system/body name to clipboard";
            menuNotifyCopy.Visible = false;
            menuNotifyCopy.Click += btnCopyLocation_Click;
            // 
            // menuNotifyShowMain
            // 
            menuNotifyShowMain.Name = "menuNotifyShowMain";
            menuNotifyShowMain.Size = new Size(144, 22);
            menuNotifyShowMain.Text = "Show ...";
            menuNotifyShowMain.ToolTipText = "Show SrvSurvey main window";
            menuNotifyShowMain.Click += menuNotifyShowMain_Click;
            // 
            // menuNotifyLogs
            // 
            menuNotifyLogs.Name = "menuNotifyLogs";
            menuNotifyLogs.Size = new Size(144, 22);
            menuNotifyLogs.Text = "Logs ...";
            menuNotifyLogs.Click += btnLogs_Click;
            // 
            // menuNotifySettings
            // 
            menuNotifySettings.Name = "menuNotifySettings";
            menuNotifySettings.Size = new Size(144, 22);
            menuNotifySettings.Text = "Settings ...";
            menuNotifySettings.Click += btnSettings_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(141, 6);
            // 
            // menuNotifyClose
            // 
            menuNotifyClose.Name = "menuNotifyClose";
            menuNotifyClose.Size = new Size(144, 22);
            menuNotifyClose.Text = "Close";
            menuNotifyClose.ToolTipText = "Close SrvSurvey";
            menuNotifyClose.Click += btnQuit_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.AppWorkspace;
            ClientSize = new Size(437, 548);
            Controls.Add(btnColonize);
            Controls.Add(btnTravel);
            Controls.Add(btnGuardian);
            Controls.Add(btnSearch);
            Controls.Add(comboDev);
            Controls.Add(lblBig);
            Controls.Add(groupCodex);
            Controls.Add(btnCodexShow);
            Controls.Add(linkNewBuildAvailable);
            Controls.Add(groupBox5);
            Controls.Add(checkTempHide);
            Controls.Add(linkLabel2);
            Controls.Add(groupBox3);
            Controls.Add(linkLabel1);
            Controls.Add(btnSettings);
            Controls.Add(btnLogs);
            Controls.Add(groupCmdr);
            Controls.Add(groupBox1);
            Controls.Add(btnQuit2);
            Controls.Add(lblNotInstalled);
            Controls.Add(lblFullScreen);
            Font = new Font("Lucida Sans Typewriter", 8.25F);
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
            groupCmdr.ResumeLayout(false);
            groupCmdr.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupCodex.ResumeLayout(false);
            menuSearchTools.ResumeLayout(false);
            menuJourney.ResumeLayout(false);
            menuColonize.ResumeLayout(false);
            menuGuardians.ResumeLayout(false);
            menuNotify.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnGroundTarget;
        private Button btnQuit2;
        private TextBox txtTargetLatLong;
        private Button btnClearTarget;
        private GroupBox groupBox1;
        private GroupBox groupCmdr;
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
        private LinkLabel linkLabel2;
        private CheckBox checkFirstFootFall;
        private Button btnPasteLatLong;
        public CheckBox checkTempHide;
        private Button btnPredictions;
        private Button btnCopyLocation;
        private GroupBox groupBox5;
        private Label label1;
        private TextBox txtExplorationValue;
        private TextBox txtBodies;
        private TextBox txtJumps;
        private Label label6;
        private Label label3;
        private Button btnResetExploration;
        private TextBox txtDistance;
        private LinkLabel linkNewBuildAvailable;
        public Button btnCodexShow;
        private GroupBox groupCodex;
        private Button btnCodexBingo;
        private Label lblBig;
        private ComboBox comboDev;
        private ButtonContextMenuStrip menuSearchTools;
        private ToolStripMenuItem menuSpherical;
        private ToolStripMenuItem menuBoxel;
        private ButtonContextMenuStrip menuJourney;
        private ToolStripMenuItem menuJourneyBegin;
        private ToolStripMenuItem menuJourneyNotes;
        private ToolStripMenuItem menuJourneyReview;
        private ToolStripMenuItem menuSetLatLong;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuFollowRoute;
        private FlatButton btnColonize;
        private FlatButton btnTravel;
        private FlatButton btnGuardian;
        private FlatButton btnSearch;
        private ButtonContextMenuStrip menuColonize;
        private ToolStripMenuItem menuMyProjects;
        private ToolStripMenuItem menuNewProject;
        private ToolStripMenuItem menuCurrentProject;
        private ToolStripSeparator menuColonizeLine2;
        private ToolStripSeparator menuColonizeLine1;
        private ToolStripMenuItem menuRefreshProjects;
        private ButtonContextMenuStrip menuGuardians;
        private ToolStripMenuItem btnGuardianThings;
        private ToolStripMenuItem btnRuins;
        private ToolStripMenuItem btnRamTah;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem btnRuinsMap;
        private ToolStripMenuItem btnRuinsOrigin;
        private Button btnNextWindow;
        private ToolTip toolTip1;
        private ToolStripMenuItem menuPrimaryProject;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip menuNotify;
        private ToolStripMenuItem menuNotifyClose;
        private ToolStripMenuItem menuNotifyShowMain;
        private ToolStripMenuItem menuNotifyCopy;
        private ToolStripMenuItem menuNotifyCmdr;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem menuNotifyNextWindow;
        private ToolStripMenuItem menuNotifyLogs;
        private ToolStripMenuItem menuNotifySettings;
        private ToolStripMenuItem menuColonizeWiki;
        private ToolStripSeparator toolStripSeparator6;
    }
}