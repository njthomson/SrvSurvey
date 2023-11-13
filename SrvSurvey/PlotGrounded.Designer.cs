
using SrvSurvey.game;

namespace SrvSurvey
{
    partial class PlotGrounded
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            Game.update -= Game_modeChanged;
            if (game?.status != null)
                game.status.StatusChanged -= Status_StatusChanged;

            if (game?.journals != null)
                game.journals.onJournalEntry -= Journals_onJournalEntry;
            if (game?.nearBody != null)
                game.nearBody.bioScanEvent -= NearBody_bioScanEvent;

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // PlotGrounded
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoValidate = AutoValidate.Disable;
            BackColor = Color.Black;
            CausesValidation = false;
            ClientSize = new Size(533, 769);
            ControlBox = false;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(5, 6, 5, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PlotGrounded";
            Opacity = 0.5D;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "PlotGrounded";
            TopMost = true;
            Load += PlotGrounded_Load;
            Paint += PlotGrounded_Paint;
            ResumeLayout(false);
        }

        #endregion
    }
}