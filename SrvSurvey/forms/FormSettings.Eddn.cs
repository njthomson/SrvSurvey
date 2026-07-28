namespace SrvSurvey
{
    internal partial class FormSettings
    {
        private CheckBox2 checkEddnUpload = null!;

        private void initializeEddnControls()
        {
            var inaraControlsPresent = tabExternalData.Controls.ContainsKey("checkInaraUpload");
            checkEddnUpload = new CheckBox2
            {
                AutoSize = true,
                CheckAlign = ContentAlignment.TopLeft,
                CheckColor = SystemColors.ControlText,
                LineColor = SystemColors.ActiveBorder,
                Location = inaraControlsPresent
                    ? new Point(505, 82)
                    : new Point(505, 6),
                Name = "checkEddnUpload",
                TabIndex = inaraControlsPresent ? 41 : 36,
                Tag = "eddnUploadEnabled",
                Text = "Share data with EDDN",
                TextAlign = ContentAlignment.TopLeft,
                UseVisualStyleBackColor = true,
            };
            checkEddnUpload.CheckedChanged += checkEddnUpload_CheckedChanged;
            tabExternalData.Controls.Add(checkEddnUpload);
            checkEddnUpload.BringToFront();

            if (inaraControlsPresent)
            {
                // Inara owns the first three rows of the adjacent sharing column.
                // Keep both opt-ins visible without coupling either integration.
                pictureBox7.Location = new Point(375, 104);
                pictureBox7.Size = new Size(228, 49);
            }
            else
            {
                // Keep EDDN beside the existing Spansh/EDSM/Canonn consent row.
                pictureBox7.Location = new Point(375, 31);
                pictureBox7.Size = new Size(228, 122);
            }
        }
    }
}
