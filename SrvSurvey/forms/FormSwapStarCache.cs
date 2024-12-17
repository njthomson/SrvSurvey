using SrvSurvey.game;
using SrvSurvey.Properties;
using System.Text;

namespace SrvSurvey.forms
{
    [Draggable]
    internal partial class FormSwapStarCache : FixedForm
    {
        public static void showDialog(IWin32Window parent)
        {
            // load current or last commander
            var cmdr = CommanderSettings.LoadCurrentOrLast();

            if (cmdr == null)
            {
                MessageBox.Show($"You must use SrvSurvey at least once before this will work", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new FormSwapStarCache(cmdr, cmdr.currentSystem);
            form.ShowDialog(parent);
        }

        private static string folderDownload = Path.Combine(Program.dataFolder, "starCache");
        private static string folderGameData = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Frontier Developments", "Elite Dangerous"));
        private static string cacheFilename = "VisitedStarsCache.dat";

        private string systemName;

        private string pathOriginal;
        private string pathBackup;

        private FormSwapStarCache(CommanderSettings cmdr, string? systemName)
        {
            InitializeComponent();
            this.Icon = Icons.sphere;
            Util.applyTheme(this);

            this.comboCmdrs.cmdrFid = cmdr.fid;
            this.systemName = systemName ?? cmdr.currentSystem ?? "Sol";
            this.comboSystem.Text = this.systemName;

            var fidNum = cmdr.fid.Substring(1);
            this.pathOriginal = Path.Combine(folderGameData, fidNum, cacheFilename);
            this.pathBackup = Path.Combine(folderGameData, fidNum, "backup-" + cacheFilename);
            btnRestore.Visible = File.Exists(this.pathBackup);

            comboSystem.SelectedIndexChanged += (sender, e) =>
            {
                this.systemName = comboSystem.SelectedSystem?.ToString()!;
            };

            comboCmdrs.SelectedIndexChanged += (sender, e) =>
            {
                var fidNum = comboCmdrs.cmdrFid.Substring(1);
                this.pathOriginal = Path.Combine(folderGameData, fidNum, cacheFilename);
                this.pathBackup = Path.Combine(folderGameData, fidNum, "backup-" + cacheFilename);
            };

            timer_Tick(null!, null!);
            timer.Start();
        }

        private void linkEDGalaxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://edgalaxy.net/visitedstars");
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var gameIsRunning = Elite.isGameRunning;

            btnYes.Enabled = !gameIsRunning;
            lblCloseWarning.Visible = gameIsRunning;
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show($"This will restore your original backup of: {cacheFilename}\r\nAre you sure you sure?", "SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.No) return;

            Game.log($"Restoring:\r\n\t{pathBackup}\r\nOver:\r\n\t{pathOriginal}");
            File.Move(pathBackup, pathOriginal, true);

            MessageBox.Show($"Restore complete, you may now restart the game", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            this.swapStarCache();
        }

        private void swapStarCache()
        {
            timer.Stop();
            btnNo.Enabled = false;
            btnYes.Enabled = false;
            btnRestore.Enabled = false;
            linkEDGalaxy.Focus();
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            // first - backup the original
            this.backupCurrentCacheFile();

            // download the data
            getStarCacheFile().continueOnMain(this, (downloadFilePath) =>
            {
                // clobber game cache file
                Game.log($"Copy:\r\n\t{downloadFilePath}\r\nOver:\r\n\t{pathOriginal}");

                File.Copy(downloadFilePath, pathOriginal, true);

                MessageBox.Show($"Swap complete, you may now restart the game", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Program.defer(() => this.Close());
            });
        }

        private void backupCurrentCacheFile()
        {
            if (File.Exists(pathBackup))
            {
                Game.log($"Backup file already exists for: {this.comboCmdrs.cmdrName} ({this.comboCmdrs.cmdrFid})");
            }
            else
            {
                Game.log($"Backing up {cacheFilename} for: {this.comboCmdrs.cmdrName} ({this.comboCmdrs.cmdrFid})");
                File.Copy(pathOriginal, pathBackup, true);
            }
        }

        private async Task<string> getStarCacheFile()
        {
            Game.log($"Getting StarCache for: {systemName}");
            // https://edgalaxy.net/visitedstars

            // request data
            var body = new StringContent(
                $"system={Uri.EscapeDataString(systemName)}",
                Encoding.ASCII,
                "application/x-www-form-urlencoded");
            var response = await new HttpClient().PostAsync("https://edgalaxy.net/visitedstars", body);

            // prep folder
            Directory.CreateDirectory(folderDownload);

            // write to a local file
            var filepath = Path.Combine(folderDownload, $"{systemName}.dat");
            using (var ws = File.Create(filepath))
            {
                using (var ss = response.Content.ReadAsStream())
                    await ss.CopyToAsync(ws);
                ws.Close();
            }

            return filepath;
        }
    }
}
