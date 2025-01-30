using SrvSurvey.game;
using System.Data;
using static SrvSurvey.game.CommanderJourney;

namespace SrvSurvey.forms
{
    internal partial class ViewJourneySystem : UserControl
    {
        private SystemData systemData;

        public ViewJourneySystem()
        {
            InitializeComponent();
        }

        public void setSystem(SystemStats systemStats)
        {
            this.systemData = SystemData.From(systemStats.starRef, CommanderSettings.currentOrLastFid);

            txtSystemName.Text = systemData.name;
            txtNotes.Text = systemData.notes;
            txtStuff.Text = $"Arrived: {systemStats.arrived.LocalDateTime.ToShortDateString()} Departed: {systemStats.departed?.LocalDateTime.ToShortDateString()}"; // => {systemStats.departed!.Value - systemStats.arrived}";

            // TODO: make a better panel of system stats
            txtRoughStats.Text = "";
            if (systemStats.count.codexNew > 0) txtRoughStats.Text += $"New codex scans: {systemStats.count.codexNew}\r\n";
            if (systemStats.count.organic > 0) txtRoughStats.Text += $"Organic scans: {systemStats.count.organic}\r\n";
            if (systemStats.landedOn?.Count > 0) txtRoughStats.Text += $"Landed on:\r\n ► {string.Join("\r\n ► ", systemStats.landedOn.Keys)}\r\n";
            if (systemStats.fssSignals?.Count > 0) txtRoughStats.Text += $"Signals:\r\n ► {string.Join("\r\n ► ", systemStats.fssSignals.Keys)}\r\n";
            if (systemStats.saaSignals?.Count > 0) txtRoughStats.Text += $"Signals:\r\n ► {string.Join("\r\n ► ", systemStats.saaSignals.Keys)}\r\n";
            if (systemStats.codexScanned?.Count > 0) txtRoughStats.Text += $"Scanned:\r\n ► {string.Join("\r\n ► ", systemStats.codexScanned.Select(id => Game.codexRef.matchFromEntryId(id.ToString(), true)?.variant.englishName ?? id.ToString()))}\r\n";

            if (systemStats.fssAllBodies)
                txtRoughStats.Text += $"FSS all bodies\r\n";
            else
                txtRoughStats.Text += $"FSS {systemStats.count.bodyScans} of {systemStats.count.bodyCount} bodies\r\n";

            if (txtRoughStats.Text.Length == 0)
                txtRoughStats.Text = "ugh";

            // how many images?
            flowImages.Controls.Clear();
            if (Directory.Exists(this.systemData.folderImages))
            {
                var imageFiles = Directory.GetFiles(this.systemData.folderImages, "*.png");
                flowImages.SuspendLayout();

                foreach (var filepath in imageFiles)
                    flowImages.Controls.Add(createThumbnail(filepath));

                flowImages.ResumeLayout();
            }
        }

        private PictureBox createThumbnail(string filepath)
        {
            var pb = new PictureBox()
            {
                BackColor = Color.Black,
                BackgroundImageLayout = ImageLayout.Zoom,
                Margin = new Padding(6),
                Width = 160,
                Height = 80,
                Tag = filepath
            };

            pb.DoubleClick += (object? sender, EventArgs e) =>
            {
                Util.openLink(filepath);
            };

            var form = this.FindForm();
            Task.Delay(10).ContinueWith(t =>
            {
                var img = Image.FromFile(filepath);
                return img;
            }).continueOnMain(form, img =>
            {
                pb.BackgroundImage = img;
                pb.Width = img.Width / 10;
                pb.Height = img.Height / 10;
            });

            return pb;
        }

        private void btnSaveNotes_Click(object sender, EventArgs e)
        {
            this.systemData.notes = txtNotes.Text;
            this.systemData.Save();
            btnSaveNotes.Visible = false;
        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {
            btnSaveNotes.Visible = this.systemData.notes != txtNotes.Text;
        }
    }
}
