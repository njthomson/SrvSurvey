using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SrvSurvey
{
    public partial class PlotBioStatus : Form, PlotterForm
    {
        private Game game = Game.activeGame!;

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

            if (game.nearBody == null)
            {
                Game.log("PlotBioStatus_Load bad!");
                return;
            }

            game.modeChanged += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode, true);

            //this.Opacity = 1;
            game.journals!.onJournalEntry += Journals_onJournalEntry;
            game.nearBody!.bioScanEvent += NearBody_bioScanEvent;
        }

        private void NearBody_bioScanEvent()
        {
            this.Invalidate();
        }

        #region journal events

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        #endregion

        private void Game_modeChanged(GameMode newMode, bool force)
        {
            var plotterMode = newMode == GameMode.SAA || game.showBodyPlotters;
            if (this.Opacity > 0 && !plotterMode)
            {
                this.Opacity = 0;
            }
            else if (this.Opacity == 0 && plotterMode)
            {
                this.reposition(Elite.getWindowRect());
            }
        }

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // TODO: restore
            //if (Debugger.IsAttached)
            //    // use a different cursor if debugging
            //    this.Cursor = Cursors.No;
            //else
            //    // otherwise hide the cursor entirely
            //    Cursor.Hide();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // restore the cursor when it leaves
            // TODO: restore
            //Cursor.Show();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Game.log("OnMouseDown");
            this.Invalidate();

            if (!Debugger.IsAttached)
                Elite.setFocusED();
        }

        #endregion

        private void PlotBioStatus_Paint(object sender, PaintEventArgs e)
        {
            if (game?.nearBody?.data?.organisms == null) return;

            var g = e.Graphics;

            g.DrawString(
                $"Biological signals: {game.nearBody.data.countOrganisms} | Analyzed: {game.nearBody.data.countAnalyzed}", // | {Util.credits(game.cmdr.organicRewards)}",
                Game.settings.fontSmall, GameColors.brushGameOrange, 4, 8);

            if (game.nearBody.currentOrganism == null)
                this.showAllGenus(g);
            else
                this.showCurrentGenus(g);

            this.drawValueCompletion(g);
        }

        private void showCurrentGenus(Graphics g)
        {
            var organism = game.nearBody!.currentOrganism!;

            float y = 28;

            // left circle - always filled
            var r = new RectangleF(8, y, 24, 24);
            g.FillEllipse(GameColors.brushGameOrangeDim, r);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            // middle circle - filled after scan two
            r = new RectangleF(40, y, 24, 24);
            if (game.nearBody!.scanTwo != null)
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
            g.DrawString(
                $"{organism.speciesLocalized}",
                Game.settings.fontBig, GameColors.brushCyan,
                104, y - 8);

            // Reward
            if (organism.reward > 0)
            {
                g.DrawString(
                    Util.credits(organism.reward),
                    Game.settings.fontSmall, GameColors.brushCyan,
                    4, 62);
            }

            this.drawScale(g, organism.range, 0.25f);
        }

        private void drawScale(Graphics g, float dist, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height + 2;

            g.DrawString(txt, Game.settings.fontSmall, GameColors.brushCyan,
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

            var data = game.nearBody!.data;
            data.updateScanProgress();


            //var scanProgress = game.nearBody!.data.bodyScanValueProgress;
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

            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = pad;

            var b = data.scanProgress < 1 || !fullValueKnown ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, Game.settings.fontSmall, b,
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
        }

        private void showAllGenus(Graphics g)
        {
            // all the Genus names
            float x = 24;
            float y = 22;

            foreach (var organism in game.nearBody!.data.organisms.Values)
            {
                var txt = organism.genusLocalized;
                if (organism.range > 0 && !organism.analyzed)
                {
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

                var sz = g.MeasureString(txt, Game.settings.fontSmall);
                if (x + sz.Width > this.Width - 16)
                {
                    x = 24;
                    y += sz.Height;
                }

                g.DrawString(
                    txt,
                    Game.settings.fontSmall,
                    organism.analyzed ? GameColors.brushGameOrange : GameColors.brushCyan,
                    x, y);

                x += sz.Width + 8;
            }
        }
    }
}
