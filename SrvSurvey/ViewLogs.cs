using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    public partial class ViewLogs : Form
    {
        private static ViewLogs? activeForm;

        public static void show(List<string> logs)
        {
            if (activeForm == null)
                ViewLogs.activeForm = new ViewLogs(logs);

            ViewLogs.activeForm.Show();
        }

        /// <summary>
        /// Append the given string to the log viewer, if it is active.
        /// </summary>
        public static void append(string txt)
        {
            if (ViewLogs.activeForm != null)
            {
                Program.control!.Invoke((MethodInvoker)delegate
                {
                    ViewLogs.activeForm.txtLogs.Text += "\r\n" + txt;
                    ViewLogs.activeForm.scrollToEnd();
                });
            }
        }

        public List<string> logs;

        private ViewLogs(List<string> logs)
        {
            this.logs = logs;
            InitializeComponent();

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formLogsLocation);
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

        private void ViewLogs_FormClosed(object sender, FormClosedEventArgs e)
        {
            ViewLogs.activeForm = null;
        }

        private void ViewLogs_Shown(object sender, EventArgs e)
        {
            scrollToEnd();
        }

        private void ViewLogs_ResizeEnd(object sender, EventArgs e)
        {
            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formLogsLocation != rect)
            {
                Game.settings.formLogsLocation = rect;
                Game.settings.Save();
            }
        }
    }
}
