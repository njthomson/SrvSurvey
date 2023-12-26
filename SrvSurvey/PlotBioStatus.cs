using SrvSurvey.game;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    public partial class PlotBioStatus : Form, PlotterForm
    {
        private Game game = Game.activeGame!;
        private string? lastCodexScan;

        private PlotBioStatus()
        {
            InitializeComponent();

            this.Height = 80;
            this.Width = 400;
            this.Cursor = Cursors.Cross;
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatCenterTop(this, gameRect, 0);
        }

        private void PlotBioStatus_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            if (game.systemBody == null)
            {
                Game.log("PlotBioStatus_Load bad!");
                return;
            }

            Game.update += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode, true);

            //this.Opacity = 1;
            game.journals!.onJournalEntry += Journals_onJournalEntry;
            //game.nearBody!.bioScanEvent += NearBody_bioScanEvent; // later
        }

        //private void NearBody_bioScanEvent()
        //{
        //    this.Invalidate();
        //}

        #region journal events

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            if (this.IsDisposed) return;

            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(CodexEntry entry)
        {
            if (entry.Category == "$Codex_Category_Biology;" && game.systemBody?.organisms != null)
            {
                var organism = game.systemBody.organisms?.FirstOrDefault(_ => _.variant == entry.Name);
                if (organism != null)
                {
                    this.lastCodexScan = $"{entry.Name_Localised}";
                    if (game.systemBody.firstFootFall)
                        this.lastCodexScan += $" {Util.credits(organism.reward * 5)} (FF bonus)";
                    else
                        this.lastCodexScan += $" {Util.credits(organism.reward)}";

                    this.Invalidate();
                }
            }
        }

        private void onJournalEntry(ScanOrganic entry)
        {
            this.lastCodexScan = null;
            this.Invalidate();
        }

        #endregion

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var plotterMode = newMode == GameMode.SAA || game.showBodyPlotters;
            if (this.Opacity > 0 && !plotterMode)
            {
                this.Opacity = 0;
            }
            else if (this.Opacity == 0 && plotterMode)
            {
                this.reposition(Elite.getWindowRect());
            }

            if (game.systemBody == null)
                Program.closePlotter<PlotBioStatus>();
        }

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Debugger.IsAttached)
            {
                // use a different cursor if debugging
                this.Cursor = Cursors.No;
            }
            else if (Game.settings.hideOverlaysFromMouse)
            {
                // move the mouse outside the overlay
                System.Windows.Forms.Cursor.Position = Elite.gameCenter;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        #endregion

        private void PlotBioStatus_Paint(object sender, PaintEventArgs e)
        {
            if (this.IsDisposed || game?.systemBody == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (game.systemBody.organisms?.Count > 0)
            {
                g.DrawString(
                    $"Biological signals: {game.systemBody.bioSignalCount} | Analyzed: {game.systemBody.countAnalyzedBioSignals}",
                    GameColors.fontSmall, GameColors.brushGameOrange, 4, 8);

                if (game.cmdr.scanOne == null)
                    this.showAllGenus(g);
                else
                    this.showCurrentGenus(g);

                this.drawValueCompletion(g);
            }
            else
            {
                // show a message if cmdr forgot to DSS the body
                var msg = $"Bio signals detected - DSS Scan required";
                var mid = this.Size / 2;
                var font = GameColors.fontSmall;
                var sz = g.MeasureString(msg, GameColors.fontMiddle);
                var tx = mid.Width - (sz.Width / 2);
                var ty = 16;
                g.DrawString(msg, GameColors.fontMiddle, GameColors.brushCyan, tx, ty);
            }

            if (game.cmdr.scanOne == null)
            {
                var allScanned = game.systemBody!.countAnalyzedBioSignals == game.systemBody.bioSignalCount;
                if (allScanned && game.systemBody.firstFootFall)
                    this.drawFooterText(g, "All signals scanned with FF bonus applied");
                else if (allScanned)
                    this.drawFooterText(g, "All signals scanned", GameColors.brushGameOrange);
                else if (this.lastCodexScan != null)
                    this.drawFooterText(g, this.lastCodexScan, GameColors.brushCyan);
                else if (game.systemBody.firstFootFall)
                    this.drawFooterText(g, "Applying first footfall bonus", GameColors.brushCyan);
                else if (!game.systemBody.wasMapped && game.systemBody.countAnalyzedBioSignals == 0 && Game.settings.useExternalData && Game.settings.autoLoadPriorScans && Program.getPlotter<PlotPriorScans>() == null)
                    this.drawFooterText(g, "Potential first footfall - send '.ff' to confirm", GameColors.brushCyan);
            }
        }

        private void showCurrentGenus(Graphics g)
        {
            var organism = game.systemBody?.organisms?.FirstOrDefault(_ => _.species == game.cmdr.scanOne!.species)!;
            if (organism == null)
            {
                Game.log($"Why no organism found for scan one: {game.cmdr.scanOne!.species}");
                return;
            }

            float y = 28;

            // left circle - always filled
            var r = new RectangleF(8, y, 24, 24);
            g.FillEllipse(GameColors.brushGameOrangeDim, r);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // middle circle - filled after scan two
            r = new RectangleF(40, y, 24, 24);
            if (game.cmdr.scanTwo != null)
            {
                g.FillEllipse(GameColors.brushGameOrangeDim, r);
                g.DrawEllipse(GameColors.penGameOrange2, r);
            }
            else
            {
                g.DrawEllipse(GameColors.penGameOrange2, r);
            }

            // right circle - always empty
            r = new RectangleF(72, y, 24, 24);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // Species name
            var txt = $"{organism.variantLocalized}"; // or species?
            var f = GameColors.fontBig;
            var sz = g.MeasureString(txt, f);
            if (sz.Width > this.Width - 104 - 8) f = GameColors.font18;
            sz = g.MeasureString(txt, f);
            if (sz.Width > this.Width - 104 - 8) f = GameColors.font14;

            g.DrawString(
                txt,
                f, GameColors.brushCyan,
                104, y - 8);

            // Reward
            if (organism.reward > 0)
            {
                var reward = game.systemBody!.firstFootFall ? organism.reward * 5 : organism.reward;
                var txt2 = Util.credits(reward);
                if (game.systemBody.firstFootFall) txt2 += " (FF bonus)";
                g.DrawString(
                    txt2,
                    GameColors.fontSmall, GameColors.brushCyan,
                    4, 62);
            }

            this.drawScale(g, organism.range, 0.25f);
        }

        private void drawScale(Graphics g, float dist, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height + 2;

            g.DrawString(txt, GameColors.fontSmall, GameColors.brushCyan,
                x, y,
                StringFormat.GenericTypographic);

            x -= pad;
            y += pad - 2;

            dist *= scale;

            g.DrawLine(GameColors.penCyan2, x, y, x - dist, y); // bar
            g.DrawLine(GameColors.penCyan2, x, y - 4, x, y + 4); // right edge
            g.DrawLine(GameColors.penCyan2, x - dist, y - 4, x - dist, y + 4); // left edge
        }

        private void drawValueCompletion(Graphics g)
        {
            const float pad = 8;

            g.ResetTransform();

            // use a simpler percentage
            var percent = 100.0f / (float)game.systemBody!.bioSignalCount * (float)game.systemBody.countAnalyzedBioSignals;
            var txt = $"  {(int)percent}%";
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = pad;

            var b = percent < 100 ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, GameColors.fontSmall, b,
                x, y,
                StringFormat.GenericTypographic);

            const float length = 100f;
            //var scannedLength = 20; // ratio * data.sumAnalyzed;

            x = this.Width - pad - txtSz.Width - length;
            y += pad - 2;

            // known unscanned - solid blue line
            g.DrawLine(GameColors.penCyan4, x, y, x + length, y);

            // already scanned value - orange bar
            g.FillRectangle(GameColors.brushGameOrange, x, 9, percent, 10);

            // active scan organism value - solid blue bar
            if (game.cmdr.scanOne != null)
                g.FillRectangle(GameColors.brushCyan, x + percent, 10, length / (float)game.systemBody!.bioSignalCount, 8);

            // old
            /*
            var data = game.nearBody!.data;
            data.updateScanProgress();

            var fullValueKnown = data.sumPotentialEstimate == data.sumPotentialKnown;
            var percent = Math.Round(data.scanProgress * 100);
            var txt = $"  {percent}%";

            // change things if we don't know the full potential value of the body
            if (!fullValueKnown)
            {
                txt = $"? {percent}%";
                if (percent == 100)
                    txt = $"?~100%";
            }

            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = pad;

            var b = data.scanProgress < 1 || !fullValueKnown ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, GameColors.fontSmall, b,
                x, y,
                StringFormat.GenericTypographic);

            const float length = 80f;
            var ratio = length / data.sumPotentialEstimate;

            var knownLength = ratio * data.sumPotentialKnown;
            var estimateLength = ratio * (data.sumPotentialEstimate - data.sumPotentialKnown);
            var scannedLength = ratio * data.sumAnalyzed;
            var activeLength = game.nearBody!.currentOrganism == null ? 0 : ratio * game.nearBody!.currentOrganism!.reward;

            x = this.Width - pad - txtSz.Width - length;
            y += pad - 2;

            // known unscanned - solid blue line
            var l = x;
            var r = x + knownLength;
            g.DrawLine(GameColors.penCyan4, l, y, r, y);

            // estimate unscanned - dotted blue line
            l = x + knownLength;
            r = l + estimateLength;
            g.DrawLine(GameColors.penCyan2Dotted, l, y, r, y);

            // already scanned value - orange bar
            l = x;
            r = l + scannedLength;
            g.FillRectangle(GameColors.brushGameOrange, l, 8, r - l, 12);

            // active scan organism value - solid blue bar
            l = r;
            r = l + activeLength;
            g.FillRectangle(GameColors.brushCyan, l, 10, r - l, 8);
            */
        }

        private void showAllGenus(Graphics g)
        {
            // all the Genus names
            float x = 24;
            float y = 22;

            if (game.systemBody?.organisms == null || game.systemBody.organisms.Count == 0)
            {
                Game.log($"Why is game.systemBody!.organisms NULL ??");
                g.DrawString(
                    "Something is wrong. Please share logs.",
                    GameColors.fontSmall,
                    Brushes.OrangeRed,
                    x, y);

                return;
            }

            var allScanned = true;
            foreach (var organism in game.systemBody!.organisms)
            {
                allScanned &= organism.analyzed;
                var txt = organism.genusLocalized;
                if (txt == null && organism.variantLocalized != null) txt = Util.getGenusDisplayNameFromVariant(organism.variantLocalized);
                if (organism.range > 0 && !organism.analyzed)
                {
                    allScanned &= false;
                    txt += $"|{organism.range}m";
                }

                /* TODO: show rewards here another time - it will require network calls to get the species name before we've visited it directly
                if (organism.Reward > 0)
                {
                    var credits = Util.credits(1234567); // (long)organism.Reward);
                    txt += $"|{credits}";
                }
                //else
                //{
                //    txt += $"|? CR";
                //}
                // */

                var sz = g.MeasureString(txt, GameColors.fontSmall);
                if (x + sz.Width > this.Width - 16)
                {
                    x = 24;
                    y += sz.Height;
                }

                g.DrawString(
                    txt,
                    GameColors.fontSmall,
                    organism.analyzed ? GameColors.brushGameOrange : GameColors.brushCyan,
                    x, y);

                x += sz.Width + 8;
            }
        }

        protected void drawFooterText(Graphics g, string msg, Brush? brush = null)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            var mid = this.Size / 2;
            var font = GameColors.fontSmall;
            var sz = g.MeasureString(msg, font);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 5;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
        }

    }
}
