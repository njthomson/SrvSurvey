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

        public static void show(StringBuilder logs)
        {
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
                ViewLogs.activeForm.txtLogs.Text += txt + "\r\n";
                ViewLogs.activeForm.scrollToEnd();
            }
        }

        public StringBuilder logs;

        private ViewLogs(StringBuilder logs)
        {
            this.logs = logs;
            InitializeComponent();

            // can we fit in our last location
            if (Game.settings.logsLocation != Rectangle.Empty)
                this.useLastLocation();
        }

        private void useLastLocation()
        {
            // position ourself within the bound of which ever screen is chosen
            var rect = Game.settings.logsLocation;
            var pt = rect.Location;
            var r = Screen.GetBounds(pt);
            if (pt.X < r.Left) pt.X = r.Left;
            if (pt.X + this.Width > r.Right) pt.X = r.Right - this.Width;

            if (pt.Y < r.Top) pt.Y = r.Top;
            if (pt.Y + this.Height > r.Bottom) pt.Y = r.Bottom - this.Height;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = pt;
            this.Size = rect.Size;
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
            txtLogs.Text = this.logs.ToString();
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
            if (Game.settings.logsLocation != rect)
            {
                Game.settings.logsLocation = rect;
                Game.settings.Save();
            }
        }
    }
}
