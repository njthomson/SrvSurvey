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

            this.Height = PlotBase.scaled(80);
            this.Width = PlotBase.scaled(480);
            this.Cursor = Cursors.Cross;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Elite.setFocusED();
        }

        public static bool allowPlotter
        {
            get => Game.settings.autoShowBioSummary &&
                Game.activeGame?.systemBody != null &&
                !Game.activeGame.hidePlottersFromCombatSuits &&
                !Game.activeGame.isShutdown &&
                !Game.activeGame.atMainMenu &&
                Game.activeGame.humanSite == null &&
                !Game.activeGame.showGuardianPlotters && !Program.isPlotter<PlotGuardians>() &&
                Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.SAA, GameMode.Codex) &&
                !Game.activeGame.status.InTaxi;
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);
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

            game.journals!.onJournalEntry += Journals_onJournalEntry;
        }

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

            var showPlotter = PlotBioStatus.allowPlotter || SystemData.isWithinLastDssDuration();

            if (this.Opacity > 0 && !showPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && showPlotter)
                this.reposition(Elite.getWindowRect());

            if (game.systemBody == null || game.systemBody.bioSignalCount == 0)
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
                    GameColors.fontSmall, GameColors.brushGameOrange, PlotBase.scaled(4), PlotBase.scaled(8));

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
                var ty = PlotBase.scaled(16);
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

            float y = PlotBase.scaled(28);

            // left circle - always filled
            var twoFour = PlotBase.scaled(24);
            var eight = PlotBase.scaled(8);
            var r = new RectangleF(eight, y, twoFour, twoFour);
            g.FillEllipse(GameColors.brushGameOrangeDim, r);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // middle circle - filled after scan two
            r = new RectangleF(PlotBase.scaled(40), y, twoFour, twoFour);
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
            r = new RectangleF(PlotBase.scaled(72), y, twoFour, twoFour);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // Species name
            var txt = $"{organism.variantLocalized}"; // or species?
            var f = GameColors.fontBig;
            var sz = g.MeasureString(txt, f);
            var oneOhFour = false && Game.settings.autoShowPlotBioSystemTest ? PlotBase.scaled(104 + 42) : PlotBase.scaled(104);
            if (sz.Width > this.Width - oneOhFour - eight) f = GameColors.font18;
            sz = g.MeasureString(txt, f);
            if (sz.Width > this.Width - oneOhFour - eight) f = GameColors.font14;

            var x = oneOhFour;
            if (false && Game.settings.autoShowPlotBioSystemTest)
            {
                var reward = organism.reward;
                PlotBase.drawBioRing(g, organism.genus, 104, y - 7, reward, true, 38);
            }

            g.DrawString(
                txt,
                f, GameColors.brushCyan,
                x, y - eight);

            // Reward
            if (organism.reward > 0)
            {
                var reward = game.systemBody!.firstFootFall ? organism.reward * 5 : organism.reward;
                var txt2 = Util.credits(reward);
                if (game.systemBody.firstFootFall) txt2 += " (FF bonus)";
                g.DrawString(
                    txt2,
                    GameColors.fontSmall, GameColors.brushCyan,
                    PlotBase.scaled(4), PlotBase.scaled(62));
            }

            this.drawScale(g, organism.range);
        }

        private void drawScale(Graphics g, float dist)
        {
            float pad = PlotBase.scaled(8);

            g.ResetTransform();

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var two = PlotBase.scaled(2);
            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height + two;

            g.DrawString(txt, GameColors.fontSmall, GameColors.brushCyan,
                x, y,
                StringFormat.GenericTypographic);

            x -= pad;
            y += pad - two;

            var bar = PlotBase.scaled(dist * 0.25f);

            var four = PlotBase.scaled(4);
            g.DrawLine(GameColors.penCyan2, x, y, x - bar, y); // bar
            g.DrawLine(GameColors.penCyan2, x, y - four, x, y + four); // right edge
            g.DrawLine(GameColors.penCyan2, x - bar, y - four, x - bar, y + four); // left edge
        }

        private void drawValueCompletion(Graphics g)
        {
            float pad = PlotBase.scaled(8);

            g.ResetTransform();

            // use a simpler percentage
            var percent = 100.0f / (float)game.systemBody!.bioSignalCount * (float)game.systemBody.countAnalyzedBioSignals;
            var txt = $" {(int)percent}%";
            var txtSz = g.MeasureString(txt, GameColors.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = pad;

            var b = percent < 100 ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, GameColors.fontSmall, b,
                x, y,
                StringFormat.GenericTypographic);

            float length = PlotBase.scaled(100f);
            //var scannedLength = 20; // ratio * data.sumAnalyzed;

            x = this.Width - pad - txtSz.Width - length;
            y += pad - PlotBase.scaled(2);

            // known un-scanned - solid blue line
            g.DrawLine(GameColors.penCyan4, x, y, x + length, y);

            // already scanned value - orange bar
            g.FillRectangle(GameColors.brushGameOrange, x, PlotBase.scaled(9), PlotBase.scaled(percent), PlotBase.scaled(10));

            // active scan organism value - solid blue bar
            if (game.cmdr.scanOne != null)
                g.FillRectangle(GameColors.brushCyan, x + PlotBase.scaled(percent), PlotBase.scaled(10), length / (float)game.systemBody!.bioSignalCount, PlotBase.scaled(8));
        }

        private void showAllGenus(Graphics g)
        {
            // all the Genus names
            float x = PlotBase.scaled(24);
            float y = PlotBase.scaled(22);

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
                if (x + sz.Width > this.Width - PlotBase.scaled(16))
                {
                    x = PlotBase.scaled(24);
                    y += sz.Height;
                }

                if (false && Game.settings.autoShowPlotBioSystemTest)
                {
                    var reward = -1; // organism.reward;
                    PlotBase.drawBioRing(g, organism.genus, x, y - 2, reward, false, 12);
                    x += 13;
                }

                g.DrawString(
                    txt,
                    GameColors.fontSmall,
                    organism.analyzed ? GameColors.brushGameOrange : GameColors.brushCyan,
                    x, y);

                if (organism.analyzed)
                {
                    // strike-through if already analyzed
                    var ly = y + sz.Height * .45f;
                    g.DrawLine(GameColors.penGameOrange1, x, ly, x + sz.Width, ly);
                    g.DrawLine(GameColors.penGameOrangeDim1, x + 1, ly + 1, x + sz.Width + 1, ly + 1);
                }

                x += sz.Width + PlotBase.scaled(8);
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
