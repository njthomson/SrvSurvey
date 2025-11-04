using SrvSurvey.game;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormSystemNotes : SizableForm
    {
        private SystemData systemData;

        public FormSystemNotes()
        {
            InitializeComponent();

            // Always on top?
            this.TopMost = Game.settings.systemNotesTopMost;
            menuAlwaysOnTop.Checked = Game.settings.systemNotesTopMost;

            // load the system
            if (Game.activeGame?.systemData != null)
            {
                this.systemData = Game.activeGame.systemData;
            }
            else
            {
                var cmdr = CommanderSettings.LoadCurrentOrLast(true)!;
                this.systemData = SystemData.Load(cmdr.currentSystem, cmdr.currentSystemAddress, cmdr.fid, cmdr.commander, false)!;
            }
            if (this.systemData == null) throw new Exception("Why no SystemData?");

            // show contents
            txtSystem.Text = systemData.name;
            txtNotes.Text = systemData.notes;
            txtNotes.SelectionStart = 0;

            // do we have any images?
            linkOpenImagesFolder.Visible = Directory.Exists(systemData.folderImages);

            Util.applyTheme(this);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // inc notes count if game is active and we're in this system
            var journey = Game.activeGame?.journey;
            if (systemData.address == journey?.currentSystem?.starRef.id64)
            {
                journey.currentSystem.count.notes += 1;
                journey.Save();
            }

            systemData.notes = txtNotes.Text;
            systemData.Save();
            this.Close();

            // if journey viewer is open - make it refresh
            BaseForm.get<FormJourneyViewer>()?.refresh();
        }

        private void menuAlwaysOnTop_Click(object sender, EventArgs e)
        {
            Game.settings.systemNotesTopMost = menuAlwaysOnTop.Checked;
            Game.settings.Save();

            this.TopMost = Game.settings.systemNotesTopMost;
        }

        private void viewOnCanonnSignalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Util.openLink("https://canonn-science.github.io/canonn-signals/?system=" + Uri.EscapeDataString(systemData!.name));
        }

        private void viewOnSpanshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Util.openLink($"https://spansh.co.uk/system/{systemData!.address}");
        }

        private void viewOnEDSMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Util.openLink($"https://www.edsm.net/en/system?systemID64={systemData!.address}");
        }

        private void linkOpenImagesFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(systemData.folderImages))
                Util.openLink(systemData.folderImages);
        }
    }
}
