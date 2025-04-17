using SrvSurvey.game;
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
                var cmdr = comboCmdr.allCmdrs.GetValueOrDefault(fid);
                txtCmdr.Text = cmdr;

                // remove the current Commander
                Game.log(comboCmdr.allCmdrs.formatWithHeader($"FormStartNewCmdr: removing: '{cmdr}' from:", "\n\t"));
                comboCmdr.Items.Remove(cmdr);
                btnStart.Enabled = false;
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
            btnStart.Enabled = !string.IsNullOrWhiteSpace(comboCmdr.Text);
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
