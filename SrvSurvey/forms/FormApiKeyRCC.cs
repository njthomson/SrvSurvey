using SrvSurvey.game;
using SrvSurvey.Properties;

namespace SrvSurvey.forms
{
    [Draggable]
    internal partial class FormApiKeyRCC : FixedForm
    {
        private CommanderSettings? cmdrSettings;
        private string? validApiKey;

        public FormApiKeyRCC()
        {
            InitializeComponent();
            this.Icon = Icons.set_square;
            BaseForm.applyThemeWithCustomControls(this);

            if (this.cmdrSettings == null)
                this.cmdrSettings = CommanderSettings.LoadCurrentOrLast();

            if (this.cmdrSettings != null)
            {
                if (string.IsNullOrEmpty(this.cmdrSettings.rccApiKey))
                {
                    txtRavenCmdr.Text = this.cmdrSettings.commander + " ?";
                }
                else
                {
                    txtRavenApiKey.Text = this.cmdrSettings.rccApiKey;
                }
            }
        }

        private void FormRCC_Load(object sender, EventArgs e)
        {
            txtRavenApiKey.Focus();
        }

        private void txtRavenApiKey_TextChanged2(object sender, EventArgs e)
        {
            checkApiKey();
        }

        private void checkApiKey()
        {
            if (!txtRavenApiKey.Enabled) return;
            //checkTrackAndPublishShipCargo.Enabled = false;

            var apiKey = txtRavenApiKey.Text;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // clear current value
                validApiKey = null;
            }
            else
            {
                txtRavenCmdr.Text = "...";
                txtRavenCmdr.BackColor = SystemColors.Control;
                txtRavenCmdr.ForeColor = SystemColors.ControlText;

                Util.deferAfter(500, async () =>
                {
                    var cmdr = await Game.rcc.getCmdrByApiKey(apiKey);
                    if (string.IsNullOrEmpty(cmdr))
                    {
                        txtRavenCmdr.Text = "(invalid key)";
                        validApiKey = null;
                        txtRavenCmdr.BackColor = Color.Yellow;
                        txtRavenCmdr.ForeColor = Color.Black;
                    }
                    else
                    {
                        txtRavenCmdr.Text = cmdr;
                        validApiKey = apiKey;

                        if (!string.IsNullOrWhiteSpace(this.cmdrSettings?.commander))
                        {
                            if (this.cmdrSettings.commander.Equals(cmdr, StringComparison.OrdinalIgnoreCase))
                            {
                                txtRavenCmdr.Text += " ✔️";
                                txtRavenCmdr.BackColor = Color.Lime;
                                //checkTrackAndPublishShipCargo.Enabled = true;
                            }
                            else
                            {
                                txtRavenCmdr.Text += $" ✖️";
                            }
                        }
                    }
                });
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openLink("https://ravencolonial.com/user");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // save the new API key, then close
            if (this.cmdrSettings != null)
            {
                this.cmdrSettings.rccApiKey = this.validApiKey;
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

        private void FormRCC_Deactivate(object sender, EventArgs e)
        {
            txtRavenApiKey.PasswordChar = '*';
        }

        private void FormRCC_Activated(object sender, EventArgs e)
        {
            txtRavenApiKey.PasswordChar =  '\0';

        }
    }
}
