namespace SrvSurvey
{
    internal partial class FormSettings
    {
        private CheckBox2 checkEddnUpload = null!;
        private CheckBox2 checkEddnTestSchemas = null!;

        private void initializeEddnControls()
        {
            var inaraControlsPresent = tabExternalData.Controls.ContainsKey("checkInaraUpload");
            var eddnTop = inaraControlsPresent ? 82 : 6;
            checkEddnUpload = new CheckBox2
            {
                AutoSize = true,
                CheckAlign = ContentAlignment.TopLeft,
                CheckColor = SystemColors.ControlText,
                LineColor = SystemColors.ActiveBorder,
                Location = new Point(505, eddnTop),
                Name = "checkEddnUpload",
                TabIndex = inaraControlsPresent ? 41 : 36,
                Tag = "eddnUploadEnabled",
                Text = "Share data with EDDN (Live)",
                TextAlign = ContentAlignment.TopLeft,
                UseVisualStyleBackColor = true,
            };
            checkEddnUpload.CheckedChanged += checkEddnUpload_CheckedChanged;
            tabExternalData.Controls.Add(checkEddnUpload);
            checkEddnUpload.BringToFront();

            checkEddnTestSchemas = new CheckBox2
            {
                AutoSize = true,
                CheckAlign = ContentAlignment.TopLeft,
                CheckColor = SystemColors.ControlText,
                LineColor = SystemColors.ActiveBorder,
                Location = new Point(505, eddnTop + 23),
                Name = "checkEddnTestSchemas",
                TabIndex = inaraControlsPresent ? 42 : 37,
                Tag = "eddnUseTestSchemas",
                Text = "Use /test schemas (dev only)",
                TextAlign = ContentAlignment.TopLeft,
                UseVisualStyleBackColor = true,
            };
            tabExternalData.Controls.Add(checkEddnTestSchemas);
            checkEddnTestSchemas.BringToFront();

            if (inaraControlsPresent)
            {
                // Inara owns the first three rows of the adjacent sharing column.
                // Keep both opt-ins visible without coupling either integration.
                pictureBox7.Location = new Point(375, 127);
                pictureBox7.Size = new Size(228, 26);
            }
            else
            {
                // Keep EDDN beside the existing Spansh/EDSM/Canonn consent row.
                pictureBox7.Location = new Point(375, 54);
                pictureBox7.Size = new Size(228, 99);
            }
        }
    }
}
