using SrvSurvey.game;
using System.Data;
using static SrvSurvey.game.CommanderJourney;

namespace SrvSurvey.forms
{
    internal partial class ViewJourneySystem : UserControl
    {
        public SystemData systemData { get; private set; }
        public SystemStats systemStats { get; private set; }
        public bool dirty { get; private set; }

        private FormJourneyViewer viewForm;

        public ViewJourneySystem(FormJourneyViewer viewForm)
        {
            InitializeComponent();
            this.viewForm = viewForm;
        }

        public void refreshTexts()
        {
            if (systemStats == null || systemData == null) return;

            txtNotes.Text = systemData.notes;

            var arrivedTime = Game.settings.viewJourneyGalacticTime ? systemStats.arrived.ToGalacticShortDateTime24Hours() : systemStats.arrived.ToLocalShortDateTime24Hours();
            var departedTime = Game.settings.viewJourneyGalacticTime ? systemStats.departed?.ToGalacticShortDateTime24Hours() : systemStats.departed?.ToLocalShortDateTime24Hours();
            if (departedTime != null)
                txtStuff.Text  = $"Arrived: {arrivedTime} - Departed: " + departedTime;
            else
                txtStuff.Text  = $"Arrived: {arrivedTime} - " + Util.timeSpanToString(DateTimeOffset.Now - systemStats.arrived);
        }

        public void setSystem(SystemStats sys)
        {
            this.systemStats = sys;
            this.systemData = SystemData.From(sys.starRef, CommanderSettings.currentOrLastFid);

            txtSystemName.Text = systemData.name;
            this.refreshTexts();

            // TODO: make a better panel of system stats
            txtRoughStats.Text = "";

            // bio stuff
            if (sys.count.rewardBio > 0)
            {
                txtRoughStats.Text += $"► Bio rewards: {Util.credits(sys.count.rewardBio)} from {sys.count.organic} scans:\r\n";
                var lines = sys.codexScanned?
                    .Select(id => Game.codexRef.matchFromEntryId(id.ToString(), true)?.variant.englishName)
                    .Where(t => t != null)
                    .Select(t => $"   - {t}");
                if (lines != null)
                    txtRoughStats.Text += string.Join("\r\n", lines);
                txtRoughStats.Text += "\r\n";
            }

            if (sys.count.rewardExp > 0) txtRoughStats.Text += $"► Exploration rewards: {Util.credits(sys.count.rewardExp)}\r\n";

            // new codex scans
            var regionalFirsts = sys.codexNew?.Where(k => !txtRoughStats.Text.Contains(k)).ToList();
            if (regionalFirsts?.Count > 0)
            {
                txtRoughStats.Text += $"► First regional scan of:\r\n";
                var lines = regionalFirsts.Select(key => $"  - {key}\r\n");
                if (lines != null) txtRoughStats.Text += string.Join("", lines);
            }

            // touchdowns?
            if (sys.landedOn?.Count > 0)
                txtRoughStats.Text += $"► Touched down {sys.count.touchdown} times across {sys.landedOn.Count} bodies.\r\n";

            // interesting signals?
            if (sys.fssSignals?.Count > 0 || sys.saaSignals?.Count > 0)
            {
                txtRoughStats.Text += $"► Signals encountered:\r\n";
                var lines = sys.fssSignals?.Keys.Select(key => $"  - {key}: {sys.fssSignals[key]}");
                if (lines != null) txtRoughStats.Text += string.Join("", lines);
                lines = sys.saaSignals?.Keys.Select(key => $"  - {key}: {sys.saaSignals[key]}");
                if (lines != null) txtRoughStats.Text += string.Join("", lines);
                txtRoughStats.Text += "\r\n";
            }

            // exploration stuff
            var bodyNotStarCount = sys.count.bodyCount - sys.count.stars;
            var txt = "";
            if (sys.count.bodyScans == sys.count.bodyCount && sys.count.bodyCount > 0)
                txt = $"► FSS all {sys.count.bodyCount} bodies";
            else
                txt = $"► FSS {sys.count.bodyScans} of {sys.count.bodyCount} bodies";
            if (sys.count.dss == bodyNotStarCount && sys.count.bodyCount > 0)
                txt += ", DSS all bodies.";
            else if (sys.count.dss > 0)
                txt += $", DSS {sys.count.dss} of {bodyNotStarCount} bodies.";
            else
                txt += $", No DSS.";
            txtRoughStats.Text += txt + "\r\n";

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

        public void saveUpdates()
        {
            Game.log($"ViewJourneySystem.saveUpdates: {this.systemData.name}");

            // inc count if we're in that system currently
            if (systemData == Game.activeGame?.systemData)
                this.systemStats.count.notes++;

            this.systemData.notes = txtNotes.Text;
            this.systemData.Save();
            this.dirty = false;
        }

        public void discardChanges()
        {
            Game.log($"ViewJourneySystem.discard: {this.systemData.name}");
            txtNotes.Text = this.systemData.notes;
            this.dirty = false;
        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {
            this.dirty = this.systemData.notes != txtNotes.Text;
            viewForm.updateSaveVisible();
        }
    }
}
