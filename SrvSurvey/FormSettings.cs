using SrvSurvey.game;
using System.Diagnostics;
using System.Reflection;

namespace SrvSurvey
{
    public partial class FormSettings : Form
    {
        private Game? game = Game.activeGame;

        private readonly Dictionary<string, FieldInfo> map = new Dictionary<string, FieldInfo>();

        public FormSettings()
        {
            InitializeComponent();

            // build a map of fields on the setting objects by name
            foreach (var fieldInfo in typeof(Settings).GetRuntimeFields())
                this.map.Add(fieldInfo.Name, fieldInfo);

            // only show this button if there are multiple copies of EliteDangerous running at the same time
            var procED = Process.GetProcessesByName("EliteDangerous64");
            btnNextProc.Visible = procED.Length > 1;
            this.Text += $" ({Program.releaseVersion})";

            this.linkDataFiles.Visible = Debugger.IsAttached;

            var osScaleFactor = (this.DeviceDpi / 96f * 100).ToString("0");
            this.comboOverlayScale.Items[0] = $"Match Windows OS scale ({osScaleFactor}%)";

            // keep these hidden from official app-store builds for now
            if (Program.isAppStoreBuild)
            {
                // Human settlements aren't ready for App store yet
                tabControl1.Controls.Remove(tabSettlements);
            }

            // Not themed - this is always light.
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
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
            checkHideMyOwnCanonnSignals.Enabled = checkUseSystemData.Checked;

            checkBodyInfoMap.Enabled = checkBodyInfoOrbit.Enabled = checkBodyInfo.Checked;

            if (game == null)
            {
                btnClearUnclaimed.Enabled = false;
                btnClearTrackers.Enabled = false;
            }
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
                            ((ComboBox)ctrl).SelectedIndex = Convert.ToInt32(map[name].GetValue(Game.settings));
                            break;

                        case nameof(RadioButton):
                            ((RadioButton)ctrl).Checked = (bool)map[name].GetValue(Game.settings)!;
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

            // TODO: handle radio's better?
            radioUseRadius.Checked = !radioUseSmall.Checked;
            panelBannerColor.BackColor = Game.settings.screenshotBannerColor;
            panelTheme.BackColor = Game.settings.defaultOrange;
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
                            if (((NumericUpDown)ctrl).DecimalPlaces == 0)
                                val = Convert.ToInt32(((NumericUpDown)ctrl).Value);
                            else
                                val = Convert.ToDouble(((NumericUpDown)ctrl).Value);
                            break;

                        case nameof(ComboBox):
                            val = Convert.ToInt32(((ComboBox)ctrl).SelectedIndex);
                            break;

                        case nameof(RadioButton):
                            val = ((RadioButton)ctrl).Checked;
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

            // restart the app if these are different:
            var restartApp = !sameCmdr
                || comboOverlayScale.SelectedIndex != Game.settings.plotterScale
                || this.checkEnableGuardianFeatures.Checked != Game.settings.enableGuardianSites
                || this.linkJournalFolder.Text != Game.settings.watchedJournalFolder
                || this.panelTheme.BackColor != Game.settings.defaultOrange;

            updateSettingsFromForm(this);

            // special case for comboCmdrs
            Game.settings.preferredCommander = comboCmdr.SelectedIndex > 0 ? comboCmdr.Text : null;
            Game.settings.screenshotBannerColor = panelBannerColor.BackColor;
            if (Game.settings.defaultOrange != panelTheme.BackColor
                // but not if we're resetting
                && panelTheme.BackColor != Color.FromArgb(255, 255, 111, 0))
            {
                // generate a new dimmer colour
                Game.settings.defaultOrangeDim = Color.FromArgb(
                    255,
                    (int)((float)panelTheme.BackColor.R * 0.6),
                    (int)((float)panelTheme.BackColor.G * 0.6),
                    (int)((float)panelTheme.BackColor.B * 0.6)
                );
            }
            Game.settings.defaultOrange = panelTheme.BackColor;

            Game.settings.Save();
            this.DialogResult = DialogResult.OK;

            // kill current process and reload
            if (restartApp)
            {
                try
                {
                    Application.DoEvents();
                    Process.Start(Application.ExecutablePath);

                    Application.DoEvents();
                    Application.DoEvents();
                    Process.GetCurrentProcess().Kill();
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
            var senderCheckbox = sender as CheckBox;
            if (senderCheckbox?.Parent == null) return;

            foreach (Control ctrl in senderCheckbox.Parent!.Controls)
                if (ctrl != senderCheckbox)
                    ctrl.Enabled = senderCheckbox.Checked;

            checkGalMapPlotter.Enabled = senderCheckbox.Checked;
        }

        private void chooseScreenshotFolder(LinkLabel linkLabel, string title)
        {
            var dialog = new FolderBrowserDialog()
            {
                Description = title,
                UseDescriptionForTitle = true,
                SelectedPath = Game.settings.screenshotSourceFolder ?? Elite.defaultScreenshotFolder,
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

        private void linkScreenshotFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = (LinkLabel)sender;
            Game.log($"Opening screenshot folder:\r\n{linkLabel.Text}");
            Util.openLink(linkLabel.Text);
        }

        private void linkDataFiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(Program.dataFolder);
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
                PlotPos.reset();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog.Color = panelBannerColor.BackColor;
            colorDialog.FullOpen = true;

            var rslt = colorDialog.ShowDialog(this);
            if (rslt == DialogResult.OK)
            {
                panelBannerColor.BackColor = colorDialog.Color;
            }
        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            numMinBioDuration.Enabled = checkBox18.Checked;
        }

        private void picBucket1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            PlotBioSystem.drawVolumeBars(e.Graphics, 7.5f, 16.5f, false, 1);
        }

        private void picBucket2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            PlotBioSystem.drawVolumeBars(e.Graphics, 7.5f, 16.5f, false, (long)Game.settings.bioRingBucketTwo * 1_000_000);
        }

        private void picBucket3_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            PlotBioSystem.drawVolumeBars(e.Graphics, 7.5f, 16.5f, false, (long)Game.settings.bioRingBucketThree * 1_000_000);
        }

        private void picBucket4_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2, 2);
            PlotBioSystem.drawVolumeBars(e.Graphics, 7.5f, 16.5f, false, 1 + (long)Game.settings.bioRingBucketThree * 1_000_000);
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

        private void linkResetTheme_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelTheme.BackColor = Color.FromArgb(255, 255, 111, 0);
            Game.settings.defaultOrangeDim = Color.FromArgb(255, 95, 48, 3);
        }

        private void btnPostProcess_Click(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath, FormPostProcess.cmdArg);
        }
    }
}
