using Newtonsoft.Json;
using SrvSurvey.forms;
using SrvSurvey.game;
using System.Data;
using System.Net;

namespace SrvSurvey
{
    [System.ComponentModel.DesignerCategory("Form")]
    internal partial class FormErrorSubmit : BaseForm
    {
        public static void Show(Exception ex)
        {
            // log but do nothing if the main form has already started to close itself
            if (Main.isClosing)
            {
                Game.log($"FormErrorSubmit.show: {ex}");
                return;
            }

            if (Program.control.InvokeRequired)
            {
                Program.control.Invoke(() => FormErrorSubmit.Show(ex));
                return;
            }

            Game.log($"FormErrorSubmit.show: {ex}");
            var form = new FormErrorSubmit(ex);
            form.ShowDialog(Main.ActiveForm);
        }

        public Exception ex;
        private int logLineCount = 0;
        private string recentLogs;

        private FormErrorSubmit(Exception ex)
        {
            this.ex = ex;
            InitializeComponent();

            // for privacy reasons, this must be unchecked by default
            txtSteps.Height = txtStack.Height;
            checkIncludeLogs.Checked = false;

            // capture recent logs NOW as the error presumably just happened vs later when people notice the error dialog
            this.recentLogs = String.Join('\n', Game.logs.TakeLast(this.logLineCount));
            // and include the version of the game if possible
            var gameFileHeader = Game.activeGame?.journals?.Entries.FirstOrDefault();
            if (gameFileHeader != null) this.recentLogs = JsonConvert.SerializeObject(gameFileHeader) + "\r\n" + this.recentLogs;

            // Not themed
        }

        private void ErrorSubmit_Load(object sender, EventArgs e)
        {
            // show stack information on the form
            txtStack.Text = ex.ToString();

            this.logLineCount = Math.Min(Game.logs.Count, 20);
            checkIncludeLogs.Text += $" (last {this.logLineCount})";

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
            form.Add("version", Program.releaseVersion);
            form.Add("exception-message", ex.Message);
            form.Add("exception-stack", ex.StackTrace!);

            if (checkIncludeLogs.Checked)
                form.Add("logs", this.recentLogs);

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
            BaseForm.show<ViewLogs>();
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
