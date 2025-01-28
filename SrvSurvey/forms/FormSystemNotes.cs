using SrvSurvey.game;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormSystemNotes : SizableForm
    {
        private SystemData sysData;

        public FormSystemNotes()
        {
            InitializeComponent();

            var cmdr = CommanderSettings.LoadCurrentOrLast();
            this.sysData = Game.activeGame?.systemData ?? SystemData.Load(cmdr.currentSystem, cmdr.currentSystemAddress, cmdr.fid, false)!;

            if (this.sysData == null) throw new Exception("Why no SystemData?");

            txtSystem.Text = sysData.name;
            txtNotes.Text = sysData.notes;
            txtNotes.SelectionStart = 0;

            Util.applyTheme(this);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            sysData.notes = txtNotes.Text;
            sysData.Save();
            this.Close();
        }
    }
}
