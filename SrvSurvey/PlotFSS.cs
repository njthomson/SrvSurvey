﻿using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing;
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;

namespace SrvSurvey
{
    internal class PlotFSS : PlotBase, PlotterForm
    {
        private FSSBodySignals? lastFSSBodySignals;

        private string? lastBodyName;
        private string? lastInitialValue;
        private string? lastMappedValue;
        private string? lastNotes;
        private bool lastWasDiscovered;

        private PlotFSS() : base()
        {
            this.Width = 420;
            this.Height = 96;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ??
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            Elite.floatCenterTop(this, gameRect, 20);

            this.Invalidate();
        }

        protected override void onJournalEntry(FSSBodySignals entry)
        {
            Game.log($"PlotFSS: FSSBodySignals event: {entry.Bodyname}");
            this.lastFSSBodySignals = entry;
        }

        protected override void onJournalEntry(Scan entry)
        {
            Game.log($"PlotFSS: Scan event: {entry.Bodyname}");

            this.Invalidate();

            // ignore Belt Clusters
            if (entry.Bodyname.Contains("Belt Cluster") || !string.IsNullOrEmpty(entry.StarType))
                return;

            this.lastBodyName = entry.Bodyname;
            this.lastWasDiscovered = entry.WasDiscovered;
            this.lastInitialValue = Util.GetBodyValue(entry, false).ToString("N0"); // 123.ToString("#.## M");
            this.lastMappedValue = Util.GetBodyValue(entry, true).ToString("N0"); // 456.ToString("#.## M");
            this.lastNotes = "";

            if (this.lastFSSBodySignals?.BodyID == entry.BodyID)
            {
                var bioSignal = this.lastFSSBodySignals?.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");

                if (bioSignal != null)
                {
                    this.lastNotes = $"{bioSignal.Count} bio signals";

                    var hasVulcanism = !string.IsNullOrEmpty(entry.Volcanism);
                    // TODO: consider check for zero or low atmosphere?
                    // var lowAtmosphere = this.lastScan?.AtmosphereType == "None";
                    if (hasVulcanism) // && lowAtmosphere)
                    {
                        this.lastNotes += " | Candidate for Brain Trees";
                    }
                }
            }

            if (!entry.WasDiscovered)
                this.lastBodyName += " (undiscovered)";

            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed) return;
            Game.log($"PlotFSS: render");

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            var brush = this.lastWasDiscovered ? GameColors.brushGameOrange : GameColors.brushCyan;

            g.DrawString($"Last scan:    {this.lastBodyName}", Game.settings.fontSmaller, brush, 4, 8);

            if (!string.IsNullOrEmpty(this.lastBodyName))
            {
                //if (!this.lastWasDiscovered)
                //    g.DrawString("(undiscovered)", Game.settings.fontSmall2, GameColors.brushCyan, 330, 8);

                var msg = $"Estimated value:    {this.lastInitialValue} cr\r\nWith surface scan:    {this.lastMappedValue} cr";
                g.DrawString(msg, Game.settings.fontMiddle, brush, 18, 28);

                if (!string.IsNullOrEmpty(this.lastNotes))
                    g.DrawString(this.lastNotes, Game.settings.fontMiddle, GameColors.brushCyan, 18, 65);
            }
        }
    }
}
