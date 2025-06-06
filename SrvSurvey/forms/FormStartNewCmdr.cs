﻿using SrvSurvey.game;
using System.ComponentModel;
using System.Diagnostics;

namespace SrvSurvey.forms
{
    public partial class FormStartNewCmdr : Form
    {
        public static bool active = false;

        public FormStartNewCmdr(string fid)
        {
            InitializeComponent();
            active = true;

            this.Load += (s, e) =>
            {
                // show current cmdr in top box
                var cmdr = comboCmdr.allCmdrs.GetValueOrDefault(fid);
                txtCmdr.Text = cmdr;

                Program.defer(() =>
                {
                    // enable controls to spawn another SrvSurvey only if there are more game processes than SrvSurvey processes
                    var gameProcs = Process.GetProcessesByName("EliteDangerous64");
                    var surveyProcs = Process.GetProcessesByName("SrvSurvey");

                    if (surveyProcs.Length < gameProcs.Length)
                    {
                        // remove the current Commander from dropdown
                        Game.log(comboCmdr.allCmdrs.formatWithHeader($"FormStartNewCmdr: removing: '{cmdr}' from:", "\n\t"));
                        comboCmdr.Items.Remove(cmdr);
                        if (comboCmdr.Items.Count > 1) comboCmdr.SelectedIndex = 0;

                        btnStart.Enabled = true;
                        comboCmdr.Enabled = true;
                    }

                    btnToggleWindow.Focus();

                    // give focus to the game, then ourselves
                    Application.DoEvents();
                    Elite.setFocusED();
                    Application.DoEvents();
                    this.Activate();
                });
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            active = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboCmdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnStart.Enabled = comboCmdr.Enabled && !string.IsNullOrWhiteSpace(comboCmdr.Text);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // start another process
            var fid = comboCmdr.cmdrFid;
            Game.log($"FormStartNewCmdr: starting new process for: '{comboCmdr.cmdrName}' ({comboCmdr.cmdrFid})");
            Process.Start(Application.ExecutablePath, $"{Program.cmdArgFid} {comboCmdr.cmdrFid}");
            this.Close();
        }

        private void btnToggleWindow_Click(object sender, EventArgs e)
        {
            FormMultiFloatie.useNextWindow();
        }
    }
}
