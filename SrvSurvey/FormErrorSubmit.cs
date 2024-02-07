using Newtonsoft.Json;
using SrvSurvey.game;
using System.Data;
using System.Net;

namespace SrvSurvey
{
    public partial class FormErrorSubmit : Form
    {
        public static void Show(Exception ex)
        {
            if (Program.control.InvokeRequired)
            {
                Program.control.Invoke(() => FormErrorSubmit.Show(ex));
                return;
            }

            var form = new FormErrorSubmit(ex);
            form.ShowDialog(Main.ActiveForm);
        }

        public Exception ex;

        private FormErrorSubmit(Exception ex)
        {
            this.ex = ex;
            InitializeComponent();

            // for privacy reasons, this must be unchecked by default
            checkIncludeLogs.Checked = false;
        }

        private void ErrorSubmit_Load(object sender, EventArgs e)
        {
            // show stack information on the form
            txtStack.Text = ex.GetType().Name + ":" + ex.Message + "\r\n\r\n" + ex.StackTrace;

            var lineCount = Math.Min(Game.logs.Count, 10);
            checkIncludeLogs.Text += $" (last {lineCount})";

            if (!string.IsNullOrEmpty(Game.activeGame?.journals?.filepath) && File.Exists(Game.activeGame.journals.filepath))
            {
                linkJournal.Text = $"Current journal file: " + Path.GetFileName(Game.activeGame.journals.filepath);
            }
            else
            {
                linkJournal.Visible = false;
                btnCopyJournalPath.Visible = false;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            var form = new Dictionary<string, string>();
            form.Add("title", $"{ex.GetType().Name} \"{ex.Message}\" at {DateTimeOffset.Now}");
            form.Add("what-happened", txtSteps.Text);
            form.Add("version", Game.releaseVersion);
            form.Add("exception-message", ex.Message);
            form.Add("exception-stack", ex.StackTrace!);

            if (checkIncludeLogs.Checked)
            {
                var lines = String.Join('\n', Game.logs.TakeLast(20));

                // and include the version of the game if possible
                var gameFileHeader = Game.activeGame?.journals?.Entries.FirstOrDefault();
                if (gameFileHeader != null) lines = JsonConvert.SerializeObject(gameFileHeader) + "\r\n" + lines;

                form.Add("logs", lines);
            }

            var query = "template=crash-report.yml&" + String.Join(
                "&",
                form.Select(part => $"{part.Key}={WebUtility.UrlEncode(part.Value)}")
                );


            var url = new UriBuilder("https://github.com/njthomson/SrvSurvey/issues/new")
            {
                Scheme = "https",
                Query = query,
            };

            Util.openLink(url.ToString());
        }

        private void linkMain_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://github.com/njthomson/SrvSurvey/issues");
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            ViewLogs.show(Game.logs);
        }

        private void FormErrorSubmit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                btnClose.Focus();
            }
        }

        private void linkDiscord_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://discord.gg/QZsMu2SkSA");
        }

        private void linkJournal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Game.activeGame?.journals?.filepath) && File.Exists(Game.activeGame.journals.filepath))
                Util.openLink(Game.activeGame.journals.filepath);
        }

        private void btnCopyStack_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtStack.Text);
        }

        private void btnCopyJournalPath_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Game.activeGame?.journals?.filepath) && File.Exists(Game.activeGame.journals.filepath))
            {
                Clipboard.SetText(Game.activeGame.journals.filepath);
            }
        }
    }
}
