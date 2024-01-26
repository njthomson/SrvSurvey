using SrvSurvey.game;
using System.Data;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace SrvSurvey
{
    public partial class FormShareData : Form
    {
        public static FormShareData? activeForm;

        public static void show(Form parent)
        {
            if (activeForm == null)
                FormShareData.activeForm = new FormShareData();

            Util.showForm(FormShareData.activeForm, parent);
        }

        private static string shareFolder = Path.Combine(Program.dataFolder, "share");

        private string zipFilepath;

        private FormShareData()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void FormShareData_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormShareData.activeForm = null;
        }

        private void FormShareData_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Game.settings.lastFid)) throw new Exception("prepLocalDataForSharing with no lastFid?");

            this.BeginInvoke(() =>
            {
                // prepare a folder
                var folder = Path.Combine(shareFolder, Game.settings.lastFid);
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
                Directory.CreateDirectory(folder);
                Game.log($"Preparing local data for sharing into folder: {folder} ...");

                // copy all site files that have new data
                var sites = GuardianSiteData.loadAllSites(false)
                    .Where(_ => _.hasDiscoveredData());

                foreach (var site in sites)
                {
                    list.Items.Add(site.displayName);
                    var newPath = Path.Combine(folder, Path.GetFileName(site.filepath));
                    File.Copy(site.filepath, newPath);
                }

                // make a hash of the sites, so we don't keep creating new files (unless something changes)
                var bytes = Encoding.UTF8.GetBytes(string.Join(',', sites.Select(_ => _.displayName)));

                var hash = Convert.ToHexString(MD5.Create().ComputeHash(bytes)).ToLowerInvariant();

                // bundle into a .zip file
                this.zipFilepath = Path.Combine(shareFolder, $"surveys-{Game.settings.lastFid}-{hash}.zip");
                if (File.Exists(this.zipFilepath)) File.Delete(this.zipFilepath);
                ZipFile.CreateFromDirectory(folder, this.zipFilepath, CompressionLevel.SmallestSize, false);

                Game.log($"{sites.Count()} sites ready to be shared: {this.zipFilepath}");

                label1.Text = $"Congratulations on discovering new data for {sites.Count()} Guardian sites:";
                txtZipFile.Text = this.zipFilepath;
                txtZipFile.SelectionStart = this.zipFilepath.Length;
            });
        }

        private void btnOpenDiscord_Click(object sender, EventArgs e)
        {
            // directly open channel
            Util.openLink("discord://-/channels/1055035389791969352/1200547428303122522");
        }

        private void linkDiscordChannel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // open invite to channel
            Util.openLink("https://discord.gg/9PhBwwDAbV");
        }

        private void btnViewZipFile_Click(object sender, EventArgs e)
        {
            Util.openLink(shareFolder);
        }

        private void btnCopyZipFile_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.zipFilepath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dropList = new System.Collections.Specialized.StringCollection();
            dropList.Add(this.zipFilepath);
            Clipboard.SetFileDropList(dropList);
        }
    }
}
