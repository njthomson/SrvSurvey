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
            if (BaseForm.get<ViewLogs>() != null)
            {
                Program.control!.Invoke((MethodInvoker)delegate
                {
                    try
                    {
                        var activeForm = BaseForm.get<ViewLogs>();
                        if (activeForm != null)
                        {
                            activeForm.txtLogs.Text += "\r\n" + txt;
                            activeForm.scrollToEnd();
                        }
                    }
                    catch { }
                });
            }
        }

        public List<string> logs;

        public ViewLogs()
        {
            this.logs = Game.logs;
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
            this.logs.Clear();

            txtLogs.Text = "";
            Game.log("Logs reset");
        }

        private void ViewLogs_Load(object sender, EventArgs e)
        {
            txtLogs.Text = String.Join("\r\n", this.logs);
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
