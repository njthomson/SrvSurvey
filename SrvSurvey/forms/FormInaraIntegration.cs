using SrvSurvey.game;
using SrvSurvey.Properties;

namespace SrvSurvey.forms
{
    [Draggable]
    internal partial class FormInaraIntegration : FixedForm
    {
        internal const string ApiKeyUrl = "https://inara.cz/elite/cmdr-settings-api/";
        internal const string TermsUrl = "https://inara.cz/elite/policies/";

        private readonly CommanderSettings? cmdrSettings;

        public FormInaraIntegration()
        {
            InitializeComponent();
            this.Icon = Icons.set_square;
            BaseForm.applyThemeWithCustomControls(this);

            this.cmdrSettings = CommanderSettings.LoadCurrentOrLast();
            this.checkUpload.Checked = Game.settings.inaraUpload;

            if (this.cmdrSettings != null)
            {
                this.txtCommander.Text = string.IsNullOrWhiteSpace(this.cmdrSettings.inaraCommanderName)
                    ? this.cmdrSettings.commander
                    : this.cmdrSettings.inaraCommanderName;
                this.txtApiKey.Text = this.cmdrSettings.inaraApiKey ?? string.Empty;
            }
            else
            {
                this.txtCommander.Text = CommanderSettings.currentOrLastCmdrName ?? string.Empty;
                this.checkUpload.Enabled = false;
                this.btnSave.Enabled = false;
            }

            this.updateControlState();
        }

        private void FormInaraIntegration_Load(object sender, EventArgs e)
        {
            if (this.txtCommander.Enabled && string.IsNullOrWhiteSpace(this.txtCommander.Text))
                this.txtCommander.Focus();
            else if (this.txtApiKey.Enabled)
                this.txtApiKey.Focus();
        }

        private void checkUpload_CheckedChanged(object sender, EventArgs e)
        {
            this.updateControlState();
        }

        private void updateControlState()
        {
            var enabled = this.cmdrSettings != null && this.checkUpload.Checked;
            this.labelCommander.Enabled = enabled;
            this.txtCommander.Enabled = enabled;
            this.labelApiKey.Enabled = enabled;
            this.txtApiKey.Enabled = enabled;
        }

        private void linkApiKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(ApiKeyUrl);
        }

        private void linkTerms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(TermsUrl);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.checkUpload.Checked && string.IsNullOrWhiteSpace(this.txtApiKey.Text))
            {
                MessageBox.Show(this, "Enter your Inara API key before enabling uploads.", "Inara Integration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtApiKey.Focus();
                return;
            }

            Game.settings.inaraUpload = this.checkUpload.Checked;
            Game.settings.Save();

            if (this.cmdrSettings != null)
            {
                var commanderName = this.txtCommander.Text.Trim();
                this.cmdrSettings.inaraCommanderName = string.IsNullOrWhiteSpace(commanderName)
                    || string.Equals(commanderName, this.cmdrSettings.commander, StringComparison.Ordinal)
                    ? null
                    : commanderName;
                this.cmdrSettings.inaraApiKey = string.IsNullOrWhiteSpace(this.txtApiKey.Text)
                    ? null
                    : this.txtApiKey.Text.Trim();
                this.cmdrSettings.Save();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
