using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.Properties;

namespace SrvSurvey
{
    [Draggable, TrackPosition]
    internal partial class ViewLogs : SizableForm
    {
        /// <summary>
        /// Append the given string to the log viewer, if it is active.
        /// </summary>
        public static void append(string txt)
        {
            var control = Program.control;
            if (control == null || control.IsDisposed || !control.IsHandleCreated) return;

            if (control.InvokeRequired)
            {
                try
                {
                    control.BeginInvoke((MethodInvoker)(() => appendOnMainThread(txt)));
                }
                catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
                {
                    // The application is shutting down or the UI handle was recreated.
                }
                return;
            }

            appendOnMainThread(txt);
        }

        private static void appendOnMainThread(string txt)
        {
            try
            {
                var activeForm = BaseForm.get<ViewLogs>();
                if (activeForm == null || activeForm.IsDisposed) return;
                activeForm.txtLogs.AppendText("\r\n" + txt);
                activeForm.scrollToEnd();
            }
            catch
            {
                // The log viewer is diagnostic-only and can close at any time.
            }
        }

        public ViewLogs()
        {
            InitializeComponent();
            this.Icon = Icons.page;

            // Not themed
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Game.clearLogs();

            txtLogs.Text = "";
            Game.log("Logs reset");
        }

        private void ViewLogs_Load(object sender, EventArgs e)
        {
            txtLogs.Text = String.Join("\r\n", Game.getLogSnapshot());
            txtLogs.SelectionStart = txtLogs.Text.Length;
        }

        private void scrollToEnd()
        {
            txtLogs.SelectionStart = txtLogs.Text.Length;
            txtLogs.SelectionLength = 0;
            txtLogs.ScrollToCaret();
        }

        private void ViewLogs_Shown(object sender, EventArgs e)
        {
            scrollToEnd();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtLogs.Text);
            Game.log("Logs copied");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(Game.logFolder);
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            Util.openLink(Game.logFolder);
        }
    }
}
