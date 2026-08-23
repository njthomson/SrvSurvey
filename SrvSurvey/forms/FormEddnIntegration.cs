using SrvSurvey.net;
using SrvSurvey.Properties;
using SrvSurvey.game;

namespace SrvSurvey.forms
{
    [Draggable]
    internal sealed class FormEddnIntegration : FixedForm
    {
        private readonly EDDN eddn;
        private readonly CheckBox2 checkEnable;
        private readonly Label labelStatus;

        internal FormEddnIntegration(EDDN eddn)
        {
            ArgumentNullException.ThrowIfNull(eddn);
            this.eddn = eddn;

            Icon = Icons.set_square;
            Text = "EDDN Sharing";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(664, 501);

            var heading = new Label
            {
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(16, 16),
                Text = "Share public Elite Dangerous data with EDDN",
            };

            var disclosure = new Label
            {
                Location = new Point(16, 48),
                Size = new Size(632, 328),
                Text =
                    "When enabled, SrvSurvey sends public game-world data from supported new journal events "
                    + "and the Market, Outfitting, Shipyard, Fleet Carrier Materials, and navigation-route "
                    + "companion files to the Elite Dangerous Data Network (EDDN).\r\n\r\n"
                    + "Each upload includes the Commander name for the current game session as EDDN's uploader "
                    + "identifier, plus game and SrvSurvey version information. EDDN obfuscates the uploader "
                    + "identifier before relaying data. SrvSurvey removes Commander-specific event fields and "
                    + "does not upload journal history read during startup. No EDDN account, API key, or access "
                    + "token is required.\r\n\r\n"
                    + "Failed uploads are stored in a local retry queue. Disabling sharing deletes pending "
                    + "uploads. This is one global setting for all Commander sessions. Uploads pause while "
                    + "multiple Elite clients are running because shared companion files cannot be attributed "
                    + "safely.\r\n\r\n"
                    + "Enable EDDN uploads in only one application at a time—for example, SrvSurvey or EDMC—to "
                    + "avoid duplicate submissions.",
            };

            checkEnable = new CheckBox2
            {
                AutoSize = true,
                CheckColor = SystemColors.ControlText,
                LineColor = SystemColors.ActiveBorder,
                Location = new Point(20, 388),
                Text = "I choose to share supported data with EDDN",
                Checked = Game.settings.eddnUploadEnabled,
            };

            labelStatus = new Label
            {
                AutoSize = true,
                Location = new Point(20, 420),
                Text = statusText(),
            };

            var btnSave = new DrawButton
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DrawBorder = true,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black,
                Location = new Point(466, 458),
                Size = new Size(88, 27),
                Text = "&Save",
            };
            btnSave.Click += save_Click;

            var btnCancel = new DrawButton
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel,
                DrawBorder = true,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black,
                Location = new Point(560, 458),
                Size = new Size(88, 27),
                Text = "&Cancel",
            };
            btnCancel.Click += (_, _) => Close();

            AcceptButton = btnSave;
            CancelButton = btnCancel;
            Controls.AddRange([
                heading,
                disclosure,
                checkEnable,
                labelStatus,
                btnSave,
                btnCancel,
            ]);

            BaseForm.applyThemeWithCustomControls(this);
        }

        private string statusText()
        {
            var pending = eddn.pendingCount;
            return pending == 0
                ? "Local retry queue: empty"
                : $"Local retry queue: {pending:N0} pending upload(s)";
        }

        private void save_Click(object? sender, EventArgs e)
        {
            var enabled = checkEnable.Checked;
            if (Game.settings.eddnUploadEnabled
                && !enabled
                && eddn.pendingCount > 0
                && MessageBox.Show(
                    "Disabling EDDN sharing will delete all pending uploads. Continue?",
                    "Disable EDDN sharing?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            var previous = Game.settings.eddnUploadEnabled;
            try
            {
                Game.settings.eddnUploadEnabled = enabled;
                Game.settings.Save();
                eddn.setEnabled(enabled);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Game.settings.eddnUploadEnabled = previous;
                List<string> rollbackErrors = [];
                try
                {
                    Game.settings.Save();
                }
                catch (Exception rollbackError)
                {
                    rollbackErrors.Add($"Consent rollback could not be saved: {rollbackError.Message}");
                }

                try
                {
                    eddn.setEnabled(previous);
                }
                catch (Exception rollbackError)
                {
                    rollbackErrors.Add($"EDDN runtime rollback failed: {rollbackError.Message}");
                }

                var detail = rollbackErrors.Count == 0
                    ? $"The previous sharing choice was restored.\r\n\r\n{ex.Message}"
                    : $"The previous sharing choice could not be fully restored.\r\n\r\n"
                        + $"Original error: {ex.Message}\r\n"
                        + string.Join("\r\n", rollbackErrors);
                MessageBox.Show(
                    $"SrvSurvey could not save the EDDN sharing choice. {detail}",
                    "EDDN sharing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                labelStatus.Text = statusText();
            }
        }
    }
}
