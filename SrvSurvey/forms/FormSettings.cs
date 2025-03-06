using SharpDX.DirectInput;
using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Reflection;

namespace SrvSurvey
{
    public partial class FormSettings : Form
    {
        private static Game? game { get => Game.activeGame; }

        private readonly Dictionary<string, FieldInfo> map = new();
        private Color colorScreenshotBanner = Game.settings.screenshotBannerColor;
        private Dictionary<KeyAction, string>? nextKeyActions;

        private int codexImageCount;
        private string codexImageSize;
        private Dictionary<Guid, string> devices = new();

        public FormSettings()
        {
            // TODO: Load images dynamically - to speed up form initial load times?
            InitializeComponent();
            this.Icon = Icons.spanner;
            comboLang.SelectedIndex = 0;
            comboLang.Items.AddRange(Localize.supportedLanguages.Keys.ToArray());

            // build a map of fields on the setting objects by name
            foreach (var fieldInfo in typeof(Settings).GetRuntimeFields())
                this.map.Add(fieldInfo.Name, fieldInfo);

            // only show this button if there are multiple copies of EliteDangerous running at the same time
            var procED = Process.GetProcessesByName("EliteDangerous64");
            btnNextProc.Visible = procED.Length > 1;
            this.Text += $" ({Program.releaseVersion})";

            var osScaleFactor = (this.DeviceDpi / 96f * 100).ToString("0");
            this.comboOverlayScale.Items[0] = $"Match Windows OS scale ({osScaleFactor}%)";

            // keep these hidden from official app-store builds for now
            if (Program.isAppStoreBuild)
            {
                // Human settlements aren't ready for App store yet
                tabControl.Controls.Remove(tabSettlements);

                btnSwapCache.Hide();
            }

            // Not themed - this is always light.
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            this.findDevices(Game.settings.hookDirectXDeviceId_TEST);
            updateFormFromSettings(this);

            // disable controls based on settings
            checkUseBioData.Enabled = checkUseSystemData.Checked;
            checkShowPriorScans.Enabled = checkUseSystemData.Checked;
            checkSkipCheapSignals.Enabled = checkUseSystemData.Checked && checkShowPriorScans.Checked;
            numPriorScanMinValue.Enabled = checkUseSystemData.Checked && checkShowPriorScans.Checked && checkSkipCheapSignals.Checked;
            lblPriorScansCredits.Enabled = checkUseSystemData.Checked && checkShowPriorScans.Checked && checkSkipCheapSignals.Checked;

            checkShowCanonnOnRadar.Enabled = checkUseSystemData.Checked && checkShowPriorScans.Checked;
            radioUseSmall.Enabled = checkUseSystemData.Checked && checkShowPriorScans.Checked && checkShowCanonnOnRadar.Checked;
            radioUseRadius.Enabled = checkUseSystemData.Checked && checkShowPriorScans.Checked && checkShowCanonnOnRadar.Checked;
            checkGalMapPlotter.Enabled = checkUseSystemData.Checked;
            checkPlotJumpInfo.Enabled = checkUseSystemData.Checked;
            checkHideMyOwnCanonnSignals.Enabled = checkUseSystemData.Checked;

            checkBodyInfoMap.Enabled = checkBodyInfoOrbit.Enabled = checkBodyInfo.Checked;

            checkKeyChordsDirectX.Enabled = checkKeyChords.Checked;

            if (game == null)
            {
                btnClearUnclaimed.Enabled = false;
                btnClearTrackers.Enabled = false;
            }

            // Count how many image files are in the codex download cache
            Task.Run(() =>
            {
                var files = Directory.GetFiles(Game.settings.downloadCodexImageFolder);
                this.codexImageCount = files.Length;

                var size = files.Sum(file => new FileInfo(file).Length);
                return size;
            }).continueOnMain(this, size =>
            {
                this.codexImageSize = (size / 1_073_741_824D).ToString("N3").TrimEnd('0');
                lblCodexImagesSize.Text += $" {codexImageSize} GB";
            });
        }

        /// <summary>
        /// Building a dictionary of device Id to Name + populate combo box
        /// </summary>
        private void findDevices(Guid selectDeviceId)
        {
            // abuse .Tag - code below checks if it is something to avoid processing SelectedIndexChange events
            comboDirectXDevice.Tag = "X";

            devices.Clear();
            devices.Add(Guid.Empty, "(None)");

            var directInput = new DirectInput();
            foreach (var device in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                devices.Add(device.InstanceGuid, device.InstanceName);
            foreach (var device in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                devices.Add(device.InstanceGuid, device.InstanceName);

            comboDirectXDevice.DataSource = new BindingSource(devices, null);
            comboDirectXDevice.DisplayMember = "Value";
            comboDirectXDevice.ValueMember = "Key";

            comboDirectXDevice.SelectedValue = selectDeviceId;
            comboDirectXDevice.Tag = null;
        }

        private void findCmdrs()
        {
            var cmdrs = CommanderSettings.getAllCmdrs()
                .Values
                .Order()
                .ToArray();

            comboCmdr.Items.Clear();
            comboCmdr.Items.Add("(no preference)");
            comboCmdr.Items.AddRange(cmdrs);

            if (string.IsNullOrEmpty(Game.settings.preferredCommander))
                comboCmdr.SelectedIndex = 0;
            else
                comboCmdr.SelectedItem = Game.settings.preferredCommander;
        }

        private void prepKeyChords()
        {
            listKeys.Items.Clear();
            foreach (var key in Enum.GetValues<KeyAction>())
            {
                var desc = Properties.KeyChords.ResourceManager.GetString(key.ToString(), Thread.CurrentThread.CurrentUICulture);
                var item = new ListViewItem()
                {
                    Name = key.ToString(),
                    Text = key.ToString(),
                    ToolTipText = desc,
                };
                item.SubItems.Add(Game.settings.keyActions_TEST?.GetValueOrDefault(key) ?? "");
                item.SubItems.Add(desc);
                listKeys.Items.Add(item);
            }

            listKeys.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listKeys.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listKeys.Columns[2].Width = listKeys.Width - listKeys.Columns[0].Width - listKeys.Columns[1].Width - SystemInformation.VerticalScrollBarWidth;
            listKeys.Enabled = Game.settings.keyhook_TEST;
        }

        private void updateFormFromSettings(Control parentControl)
        {
            foreach (Control ctrl in parentControl.Controls)
            {
                if (!string.IsNullOrWhiteSpace(ctrl.Tag?.ToString()))
                {
                    if (!map.ContainsKey(ctrl.Tag.ToString()!))
                    {
                        throw new Exception($"Missing setting: {ctrl.Tag}");
                    }

                    var name = ctrl.Tag.ToString()!;
                    // Game.log($"Read setting: {name} => {map[name].GetValue(Game.settings)}");

                    switch (ctrl.GetType().Name)
                    {
                        case nameof(LinkLabel):
                        case nameof(TextBox):
                            ctrl.Text = (string)map[name].GetValue(Game.settings)!;
                            break;

                        case nameof(CheckBox):
                            ((CheckBox)ctrl).Checked = (bool)map[name].GetValue(Game.settings)!;
                            break;

                        case nameof(NumericUpDown):
                            ((NumericUpDown)ctrl).Value = Convert.ToDecimal(map[name].GetValue(Game.settings));
                            break;

                        case nameof(ComboBox):
                            if (map[name].FieldType == typeof(int) || map[name].FieldType == typeof(float))
                                ((ComboBox)ctrl).SelectedIndex = Convert.ToInt32(map[name].GetValue(Game.settings));
                            else if (map[name].FieldType == typeof(string))
                                ((ComboBox)ctrl).Text = map[name].GetValue(Game.settings)?.ToString();
                            else
                                throw new NotSupportedException(name);
                            break;

                        case nameof(RadioButton):
                            ((RadioButton)ctrl).Checked = (bool)map[name].GetValue(Game.settings)!;
                            break;

                        case nameof(ListView):
                            if (ctrl.Tag != null)
                            {
                                var subSettings = map[name].GetValue(Game.settings);
                                foreach (ListViewItem item in ((ListView)ctrl).Items)
                                    item.Checked = subSettings!.GetType()!.GetField((string)item.Tag!)!.GetValue(subSettings) as bool? ?? false;
                            }
                            break;

                        default:
                            throw new Exception($"Unexpected control type: {ctrl.GetType().Name}");
                    }
                }

                // recurse if there are children
                if (ctrl.HasChildren)
                    updateFormFromSettings(ctrl);
            }

            // load potential cmdr's
            this.findCmdrs();

            // and manually set the following
            var langName = Localize.nameFromCode(Game.settings.lang);
            if (string.IsNullOrEmpty(langName))
                comboLang.SelectedIndex = 0;
            else
                comboLang.Text = langName;

            // TODO: handle radio's better?
            radioUseRadius.Checked = !radioUseSmall.Checked;
            colorScreenshotBanner = Game.settings.screenshotBannerColor;
            panelTheme.BackColor = Game.settings.defaultOrange;
            panelTheme2.BackColor = Game.settings.defaultCyan;

            this.nextKeyActions = Game.settings.keyActions_TEST == null
                ? new()
                : new(Game.settings.keyActions_TEST!);

            this.prepKeyChords();
        }

        private void updateAllSettingsFromForm()
        {
            // recurse through various controls
            updateSettingsFromForm(this);

            // and manually set the following
            Game.settings.lang = Localize.codeFromName(comboLang.Text);
            var deviceId = comboDirectXDevice.SelectedValue as Guid?;
            Game.settings.hookDirectXDeviceId_TEST = deviceId ?? Guid.Empty;

            Game.settings.keyActions_TEST ??= new();
            foreach (ListViewItem item in listKeys.Items)
            {
                var keyAction = Enum.Parse<KeyAction>(item.Name);
                Game.settings.keyActions_TEST[keyAction] = item.SubItems[1].Text;
            }
        }

        private void updateSettingsFromForm(Control parentControl)
        {
            foreach (Control ctrl in parentControl.Controls)
            {
                if (ctrl.Tag != null && map.ContainsKey(ctrl.Tag.ToString()!))
                {
                    var name = ctrl.Tag.ToString()!;

                    object? val = null;
                    switch (ctrl.GetType().Name)
                    {
                        case nameof(LinkLabel):
                        case nameof(TextBox):
                            val = ctrl.Text;
                            break;

                        case nameof(CheckBox):
                            val = ((CheckBox)ctrl).Checked;
                            break;

                        case nameof(NumericUpDown):
                            if (map[name].FieldType == typeof(int))
                                val = Convert.ToInt32(((NumericUpDown)ctrl).Value);
                            else if (map[name].FieldType == typeof(float))
                                val = Convert.ToSingle(((NumericUpDown)ctrl).Value);
                            else
                                val = Convert.ToDouble(((NumericUpDown)ctrl).Value);
                            break;

                        case nameof(ComboBox):
                            if (map[name].FieldType == typeof(int) || map[name].FieldType == typeof(float))
                                val = Convert.ToInt32(((ComboBox)ctrl).SelectedIndex);
                            else if (map[name].FieldType == typeof(string))
                                val = ((ComboBox)ctrl).Text;
                            else
                                throw new NotSupportedException(name);
                            break;

                        case nameof(RadioButton):
                            val = ((RadioButton)ctrl).Checked;
                            break;

                        case nameof(ListView):
                            if (ctrl.Tag != null)
                            {
                                var subSettings = map[name].GetValue(Game.settings)!;
                                foreach (ListViewItem item in ((ListView)ctrl).Items)
                                    subSettings.GetType()!.GetField((string)item.Tag!)!.SetValue(subSettings, item.Checked);
                                return;
                            }
                            break;


                        default:
                            throw new Exception($"Unexpected control type: {ctrl.GetType().Name}");
                    }

                    // Game.log($"Write setting: {name} => {val}");
                    map[name].SetValue(Game.settings, val);
                }

                // recurse if there are children
                if (ctrl.HasChildren)
                    updateSettingsFromForm(ctrl);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var sameCmdr = (string.IsNullOrWhiteSpace(Game.settings.preferredCommander) && comboCmdr.SelectedIndex == 0)
                || (comboCmdr.Text == Game.settings.preferredCommander);

            var langChanged = Game.settings.lang != Localize.codeFromName(comboLang.Text);

            // restart the app if these are different:
            var restartApp = !sameCmdr || langChanged
                || checkColonization.Checked != Game.settings.buildProjects_TEST
                || comboOverlayScale.SelectedIndex != Game.settings.plotterScale
                || this.checkEnableGuardianFeatures.Checked != Game.settings.enableGuardianSites
                || this.linkJournalFolder.Text != Game.settings.watchedJournalFolder
                || this.panelTheme2.BackColor != Game.settings.defaultCyan
                || this.panelTheme.BackColor != Game.settings.defaultOrange;

            updateAllSettingsFromForm();

            // special case for comboCmdrs
            Game.settings.preferredCommander = comboCmdr.SelectedIndex > 0 ? comboCmdr.Text : null;
            Game.settings.screenshotBannerColor = colorScreenshotBanner;

            // ratio for darker colours
            var rr = 0.5f;
            // primary theme color
            if (Game.settings.defaultOrange != panelTheme.BackColor
                // but not if we're resetting
                && panelTheme.BackColor != GameColors.Defaults.Orange)
            {
                // generate a new dimmer colour
                Game.settings.defaultOrangeDim = Color.FromArgb(
                    255,
                    (int)((float)panelTheme.BackColor.R * rr),
                    (int)((float)panelTheme.BackColor.G * rr),
                    (int)((float)panelTheme.BackColor.B * rr)
                );
            }
            Game.settings.defaultOrange = panelTheme.BackColor;

            // secondary theme color
            if (Game.settings.defaultCyan != panelTheme2.BackColor
                // but not if we're resetting
                && panelTheme2.BackColor != GameColors.Defaults.Cyan)
            {
                // generate a new dimmer colour
                Game.settings.defaultDarkCyan = Color.FromArgb(
                    255,
                    (int)((float)panelTheme2.BackColor.R * rr),
                    (int)((float)panelTheme2.BackColor.G * rr),
                    (int)((float)panelTheme2.BackColor.B * rr)
                );
            }
            Game.settings.defaultCyan = panelTheme2.BackColor;

            Game.settings.Save();
            this.DialogResult = DialogResult.OK;

            // kill current process and reload
            if (restartApp)
            {
                try
                {
                    Program.forceRestart();
                }
                catch { /* swallow */ }
            }
            else
            {
                // close all plotters (except PlotPulse)
                Program.closeAllPlotters(true);
                Program.getPlotter<PlotPulse>()?.reposition(Elite.getWindowRect());
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // ensure basic keyboard hooks are in the correct state
            var keyHookSettingsValid = Game.settings.keyhook_TEST && Game.settings.keyActions_TEST != null;
            if (keyHookSettingsValid)
                Main.form.startHooks();
            else
                Main.form.stopHooks();

            // ensure DirectX hooks are in the correct state
            if (Game.settings.hookDirectX_TEST && Game.settings.keyhook_TEST && Game.activeGame != null)
                Main.form.hook.startDirectX(comboDirectXDevice.SelectedValue as Guid?);
            else
                Main.form.hook?.stopDirectX();
        }

        private void trackOpacity_Scroll(object sender, EventArgs e)
        {
            if (numOpacity.Value != trackOpacity.Value)
                numOpacity.Value = trackOpacity.Value;
        }

        private void numOpacity_ValueChanged(object sender, EventArgs e)
        {
            if (numOpacity.Value != trackOpacity.Value)
                trackOpacity.Value = (int)numOpacity.Value;
        }

        private void btnNextProc_Click(object sender, EventArgs e)
        {
            Program.hideActivePlotters();

            // increment process idx and make plotters adjust
            Elite.nextWindow();
            Application.DoEvents();
            Elite.setFocusED();
        }

        private void btnClearUnclaimed_Click(object sender, EventArgs e)
        {
            if (game == null) return;

            var rslt = MessageBox.Show($"Are you sure you want to clear {Util.credits(game.cmdr.organicRewards)} from {game.cmdr.scannedBioEntryIds.Count} organisms?", "Clear unclaimed rewards", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (rslt == DialogResult.Yes)
            {
                Game.log($"Clearing in {game.cmdr.organicRewards} unclaimed rewards.");
                game.cmdr.organicRewards = 0;
                game.cmdr.scannedOrganics?.Clear(); // retire?
                game.cmdr.scannedBioEntryIds.Clear();
                game.cmdr.Save();
            }
        }

        private void disableEverythingElse_CheckedChanged(object sender, EventArgs e)
        {
            disableEverythingElse(sender as CheckBox);
        }

        private void checkProcessScreenshots_CheckedChanged(object sender, EventArgs e)
        {
            disableEverythingElse(sender as CheckBox, groupCodexImages);
        }

        private void disableEverythingElse(CheckBox? senderCheckbox, params Control[] except)
        {
            if (senderCheckbox?.Parent == null) return;

            foreach (Control ctrl in senderCheckbox.Parent!.Controls)
                if (ctrl != senderCheckbox && !except.Contains(ctrl))
                    ctrl.Enabled = senderCheckbox.Checked;

            checkGalMapPlotter.Enabled = senderCheckbox.Checked;
            checkPlotJumpInfo.Enabled = senderCheckbox.Checked;
        }

        private void chooseScreenshotFolder(LinkLabel linkLabel, string title)
        {
            var dialog = new FolderBrowserDialog()
            {
                Description = title,
                UseDescriptionForTitle = true,
                SelectedPath = linkLabel.Text,
            };

            var rslt = dialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
                linkLabel.Text = dialog.SelectedPath;
        }

        private void btnChooseScreenshotSourceFolder_Click(object sender, EventArgs e)
        {
            this.chooseScreenshotFolder(linkScreenshotSourceFolder, "Choose source folder screenshots");
        }

        private void btnChooseScreenshotTargetFolder_Click(object sender, EventArgs e)
        {
            this.chooseScreenshotFolder(linkTargetScreenshotFolder, "Choose destination folder screenshots");
        }

        private void menuCodexChange_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                Description = "Choose download cache folder",
                UseDescriptionForTitle = true,
                SelectedPath = linkCodexCache.Text,
            };

            var rslt = dialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
                linkCodexCache.Text = dialog.SelectedPath;
        }

        private void linkScreenshotFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var folder = ((LinkLabel)sender).Text;

            Game.log($"Opening screenshot folder:\r\n{folder}");
            if (Directory.Exists(folder))
            {
                Util.openLink(folder);
                return;
            }

            // ask the user if we should create the folder?
            var rslt = MessageBox.Show(
                this,
                Properties.Misc.Settings_CreateFolderNotFound.format(folder),
                "SrvSurvey",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (rslt == DialogResult.Yes)
            {
                Directory.CreateDirectory(folder);
                Util.openLink(folder);
            }
        }

        private void btnChooseJournalFolder_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                Description = "Choose folder for journal files",
                UseDescriptionForTitle = true,
                SelectedPath = Game.settings.watchedJournalFolder,
            };

            var rslt = dialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
                linkJournalFolder.Text = dialog.SelectedPath;
        }

        private void linkJournalFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(linkJournalFolder.Text);
        }

        private void btnClearTrackers_Click(object sender, EventArgs e)
        {
            game?.clearAllBookmarks();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/wiki#when-bio-scanning");
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            numMinScanValue.Enabled = checkBox11.Checked;
        }

        private void checkSkipCheapSignals_CheckedChanged(object sender, EventArgs e)
        {
            numPriorScanMinValue.Enabled = checkSkipCheapSignals.Checked;
            lblPriorScansCredits.Enabled = checkSkipCheapSignals.Checked;
        }

        private void checkShowCanonnOnRadar_CheckedChanged(object sender, EventArgs e)
        {
            radioUseSmall.Enabled =
            radioUseRadius.Enabled =
                checkShowCanonnOnRadar.Checked;
        }

        private void checkShowPriorScans_CheckedChanged(object sender, EventArgs e)
        {
            checkSkipCheapSignals.Enabled =
            numPriorScanMinValue.Enabled =
            lblPriorScansCredits.Enabled =
            radioUseSmall.Enabled =
            radioUseRadius.Enabled =
            checkShowCanonnOnRadar.Enabled =
            checkHideMyOwnCanonnSignals.Enabled =
                checkShowPriorScans.Checked;
        }

        private void btnResetOverlays_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(
                this,
                "This will reset any custom overlay positions.\r\n\r\nAre you sure?",
                "SrvSurvey",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rslt == DialogResult.Yes)
                PlotPos.resetAll();
        }

        private void btnAdjustOverlays_Click(object sender, EventArgs e)
        {
            var form = new FormAdjustOverlay();

            Elite.setFocusED();

            Program.defer(() =>
            {
                var er = Elite.getWindowRect();
                if (er != Rectangle.Empty)
                {
                    form.StartPosition = FormStartPosition.Manual;

                    form.Left = er.Left + (er.Width / 2) - (form.Width / 2);
                    form.Top = er.Top + (er.Height / 2) - (form.Height / 2);
                }
                else
                {
                    form.StartPosition = FormStartPosition.CenterScreen;
                }
                form.Show();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorScreenshotBanner;
            colorDialog.FullOpen = true;

            var rslt = colorDialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                colorScreenshotBanner = colorDialog.Color;
                pictureBox3.Invalidate();
            }
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            pictureBox3.BorderStyle = BorderStyle.Fixed3D;
            var r = new Rectangle(1, 1, pictureBox3.Width - 4, pictureBox3.Height - 4);
            g.FillRectangle(Brushes.Black, r);

            var pt = new Point(5, 0);
            var bigTxt = "Body: <nearest body name>";
            TextRenderer.DrawText(g, bigTxt, GameColors.fontScreenshotBannerBig, pt, colorScreenshotBanner);

            var sz = TextRenderer.MeasureText(g, bigTxt, GameColors.fontScreenshotBannerBig);
            pt.Y += sz.Height;

            var txt = $"System: <system name>\r\nCmdr: <commander name>  -  <time stamp>"
                + "\r\n  Ancient Ruins #<n> - <site type>"
                + "\r\n  Lat: +123.456789° Long: -12.345678°";
            TextRenderer.DrawText(g, txt, GameColors.fontScreenshotBannerSmall, pt, colorScreenshotBanner, TextFormatFlags.Default);
        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            numMinBioDuration.Enabled = checkBox18.Checked;
        }

        private void picBucket1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            VolumeBar.render(e.Graphics, 7.5f, 16.5f, VolColor.Orange, 1);
        }

        private void picBucket2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            VolumeBar.render(e.Graphics, 7.5f, 16.5f, VolColor.Orange, (long)Game.settings.bioRingBucketTwo * 1_000_000);
        }

        private void picBucket3_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            VolumeBar.render(e.Graphics, 7.5f, 16.5f, VolColor.Orange, (long)Game.settings.bioRingBucketThree * 1_000_000);
        }

        private void picBucket4_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            VolumeBar.render(e.Graphics, 7.5f, 16.5f, VolColor.Orange, 1 + (long)Game.settings.bioRingBucketThree * 1_000_000);
        }

        private void checkBodyInfo_CheckedChanged(object sender, EventArgs e)
        {
            checkBodyInfoMap.Enabled = checkBodyInfoOrbit.Enabled = checkBodyInfo.Checked;
        }

        private void linkResetWatchFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkJournalFolder.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous\");
        }

        private void btnTheme_Click(object sender, EventArgs e)
        {
            colorTheme.Color = panelTheme.BackColor;
            colorTheme.FullOpen = true;

            var rslt = colorTheme.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                panelTheme.BackColor = colorTheme.Color;
            }
        }

        private void btnTheme2_Click(object sender, EventArgs e)
        {
            colorTheme.Color = panelTheme2.BackColor;
            colorTheme.FullOpen = true;

            var rslt = colorTheme.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                panelTheme2.BackColor = colorTheme.Color;
            }
        }

        private void linkResetTheme_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelTheme.BackColor = GameColors.Defaults.Orange;
            Game.settings.defaultOrangeDim = GameColors.Defaults.OrangeDim;
        }

        private void linkResetTheme2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelTheme2.BackColor = GameColors.Defaults.Cyan;
            Game.settings.defaultDarkCyan = GameColors.Defaults.DarkCyan;
        }

        private void btnPostProcess_Click(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath, $"{Program.cmdArgScanOld} {Game.settings.lastFid}");
        }

        private void checkHumanSitePlotter_CheckedChanged(object sender, EventArgs e)
        {
            var senderCheckbox = sender as CheckBox;
            if (senderCheckbox?.Parent == null) return;

            foreach (Control ctrl in senderCheckbox.Parent!.Controls)
                if (ctrl != senderCheckbox)
                    ctrl.Enabled = senderCheckbox.Checked;
        }

        private void numHumanSitePlotterSize_ValueChanged(object sender, EventArgs e)
        {
            // live adjust plotter size if it exists
            var plotter = Program.getPlotter<PlotHumanSite>();
            if (plotter != null)
            {
                plotter.Width = (int)numHumanSitePlotterWidth.Value;
                plotter.Height = (int)numHumanSitePlotterHeight.Value;
                plotter.BackgroundImage = GameGraphics.getBackgroundImage(plotter);
                plotter.reposition(Elite.getWindowRect());
                plotter.Invalidate();
            }
        }

        private void listKeys_DoubleClick(object sender, EventArgs e)
        {
            if (listKeys.SelectedItems.Count != 1) return;
            var item = listKeys.SelectedItems[0] as ListViewItem;
            if (item == null || Game.settings.keyActions_TEST == null) return;

            var keyAction = Enum.Parse<KeyAction>(item.Name);
            var currentChord = Game.settings.keyActions_TEST.GetValueOrDefault(keyAction);

            var dialog = new FormSetKeyChord(currentChord);
            var rslt = dialog.ShowDialog();
            if (rslt == DialogResult.OK)
            {
                Game.log($"Updating {keyAction} from '{currentChord}' to '{dialog.keyChord}'");
                item.SubItems[1].Text = dialog.keyChord;
            }
        }

        private void checkKeyChords_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkKeyChords.Visible) return;

            listKeys.Enabled = checkKeyChords.Checked;
            checkKeyChordsDirectX.Enabled = checkKeyChords.Checked;

            checkKeyChordsDirectX_CheckedChanged(sender, e);
        }

        private void checkKeyChordsDirectX_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkKeyChordsDirectX.Visible) return;

            if (checkKeyChords.Checked && checkKeyChordsDirectX.Checked)
            {
                Main.form.hook ??= new KeyboardHook();
                Main.form.hook.startDirectX(comboDirectXDevice.SelectedValue as Guid?);
            }
            else
            {
                Main.form.hook?.stopDirectX();
            }
        }

        private void btnSwapCache_Click(object sender, EventArgs e)
        {
            FormSwapStarCache.showDialog(this);
        }

        private void menuCodexReset_Click(object sender, EventArgs e)
        {
            linkCodexCache.Text = CodexRef.defaultCodexImagesFolder;
        }

        private void menuClearCodexCache_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, $"Are you sure you want to remove {this.codexImageCount} files? ({this.codexImageSize} GB)", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (rslt == DialogResult.Yes)
            {
                checkPreDownloadCodexImages.Checked = false;
                if (CodexRef.taskDownloadAllCodexImages != null)
                    CodexRef.taskDownloadAllCodexImages = null;

                var files = Directory.GetFiles(Game.settings.downloadCodexImageFolder);
                Game.log($"Deleting {files.Length} codex image cache files...");
                foreach (var file in files)
                    File.Delete(file);
            }
        }

        private void checkPreDownloadCodexImages_CheckedChanged(object sender, EventArgs e)
        {
            if (checkPreDownloadCodexImages.Visible && checkPreDownloadCodexImages.Checked)
                MessageBox.Show(this, "Codex images will be pre-downloaded after you next restart SrvSurvey. This will happen in the background and requires ~1GB of disk space.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboDirectXDevice_DropDown(object sender, EventArgs e)
        {
            var newDeviceId = (comboDirectXDevice.SelectedValue as Guid?) ?? Guid.Empty;
            this.findDevices(newDeviceId);
        }

        private void comboDirectXDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboDirectXDevice.Visible || comboDirectXDevice.Tag != null) return;

            var newDeviceId = (comboDirectXDevice.SelectedValue as Guid?) ?? Guid.Empty;
            if (Main.form.hook.activeDeviceId != newDeviceId)
            {
                Main.form.hook.stopDirectX();
                Task.Delay(100)
                    .ContinueWith(t => Main.form.hook.startDirectX(newDeviceId));
            }
        }
    }
}
