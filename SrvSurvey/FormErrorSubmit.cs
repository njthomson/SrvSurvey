using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using SrvSurvey.game;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace SrvSurvey
{
    public partial class FormErrorSubmit : Form
    {
        public static void Show(Exception ex)
        {
            var form = new FormErrorSubmit()
            {
                ex = ex
            };

            form.ShowDialog();
        }

        public Exception ex;

        private FormErrorSubmit()
        {
            InitializeComponent();
        }

        private void ErrorSubmit_Load(object sender, EventArgs e)
        {
            txtStack.Text = ex.GetType().Name + ":" +ex.Message + "\r\n\r\n" + ex.StackTrace;
            var lineCount = Game.logs.ToString().Split('\n').Length;
            checkIncludeLogs.Text += $" ({lineCount} lines)";
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

            var form = new Dictionary<string, string>();
            form.Add("title", $"{ex.GetType().Name} \"{ex.Message}\" at {DateTimeOffset.Now}");
            form.Add("what-happened", txtSteps.Text);
            form.Add("version", Application.ProductVersion);
            form.Add("exception-message", ex.Message);
            form.Add("exception-stack", ex.StackTrace);

            if (checkIncludeLogs.Checked)
            {
                form.Add("logs", Game.logs.ToString());
            }

            var query = "template=crash-report.yml&" + String.Join(
                "&",
                form.Select(part => $"{part.Key}={WebUtility.UrlEncode(part.Value)}")
                );


            var url = new UriBuilder("https://github.com/njthomson/SrvSurvey/issues/new")
            {
                Scheme= "https",
                Query = query,
            };

            Process.Start(url.ToString());
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/njthomson/SrvSurvey/issues");
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            ViewLogs.show(Game.logs);
        }
    }
}
