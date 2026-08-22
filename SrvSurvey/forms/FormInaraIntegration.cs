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
            : this(CommanderSettings.LoadCurrentOrLast())
        {
        }

        internal FormInaraIntegration(CommanderSettings? cmdrSettings)
        {
            InitializeComponent();
            this.Icon = Icons.set_square;
            BaseForm.applyThemeWithCustomControls(this);

            // Keep this exact FID-keyed profile for the lifetime of the dialog. If the
            // active game changes while the dialog is open, Save still targets this one.
            this.cmdrSettings = cmdrSettings;
            if (cmdrSettings != null)
            {
                this.txtCommander.Text = cmdrSettings.commander;
                this.txtApiKey.Text = cmdrSettings.inaraApiKey ?? string.Empty;
            }
            else
            {
                this.txtCommander.Text = CommanderSettings.currentOrLastCmdrName ?? string.Empty;
                this.txtApiKey.Enabled = false;
                this.btnOk.Enabled = false;
                this.btnClear.Enabled = false;
            }
        }

        internal static void ApplyApiKey(CommanderSettings settings, string? apiKey)
        {
            var trimmed = apiKey?.Trim();
            settings.inaraApiKey = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private void FormInaraIntegration_Load(object sender, EventArgs e)
        {
            if (this.txtApiKey.Enabled)
                this.txtApiKey.Focus();
        }

        private void linkApiKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(ApiKeyUrl);
        }

        private void linkTerms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink(TermsUrl);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (this.cmdrSettings == null) return;
            if (string.IsNullOrWhiteSpace(this.txtApiKey.Text))
            {
                MessageBox.Show(this,
                    "Enter this commander's Inara API key, or use Clear Key to disable uploads.",
                    "Inara Integration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                this.txtApiKey.Focus();
                return;
            }

            ApplyApiKey(this.cmdrSettings, this.txtApiKey.Text);
            this.cmdrSettings.Save();
            Game.activeGame?.onInaraApiKeyChanged(this.cmdrSettings);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (this.cmdrSettings == null) return;
            var answer = MessageBox.Show(this,
                $"Clear the Inara API key for {this.cmdrSettings.commander} and discard pending Inara uploads for this session?",
                "Clear Inara API Key",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            ApplyApiKey(this.cmdrSettings, null);
            this.cmdrSettings.Save();
            Game.activeGame?.onInaraApiKeyChanged(this.cmdrSettings);
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
