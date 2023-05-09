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
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatCenterTop(this, 0);
        }

        private void PlotBioStatus_Load(object sender, EventArgs e)
        {
            this.initialize();
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

            this.reposition(Elite.getWindowRect());
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

        private void PlotBioStatus_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void PlotBioStatus_DoubleClick(object sender, EventArgs e)
        {
            this.Invalidate();
            Elite.setFocusED();
        }

        private void PlotBioStatus_Click(object sender, EventArgs e)
        {
            Elite.setFocusED();
        }

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

            var scanProgress = game.nearBody.data.bodyScanValueProgress;
            var fullValueKnown = game.nearBody.data.isFullPotentialKnown;

            var percent = Math.Round(scanProgress * 100);
            var txt = $"{percent}%";
            var handicap = 0;
            var barPen =  GameColors.penGameOrange2;

            // change things if we don't know the full potential value of the body
            if (!fullValueKnown)
            {
                handicap = 15;
                txt = $"? {percent}%";
                barPen = GameColors.penGameOrange2Dotted;

                if (percent == 100)
                    txt = $"?~100%";
            }

            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);
            var x = this.Width - pad - txtSz.Width;
            var y = pad;

            var b = scanProgress < 1 || !fullValueKnown ? GameColors.brushCyan : GameColors.brushGameOrange;
            g.DrawString(txt, Game.settings.fontSmall, b,
                x, y,
                StringFormat.GenericTypographic);

            x = this.Width - 50;
            y += pad - 2;

            var length = 80f; // bar length in pixels
            var remaining = length * scanProgress;

            // orange lines
            var l = x - length;
            g.DrawLine(barPen, l, y, x, y); // bar
            g.DrawLine(GameColors.penGameOrange2, x - handicap, y - 4, x - handicap, y + 4); // right edge
            g.DrawLine(GameColors.penGameOrange2, l, y - 4, l, y + 4); // left edge

            // blue lines
            if (scanProgress < 1 || !fullValueKnown)
            {
                var r = x - length + remaining - handicap;
                if (r < l) r += handicap; // don't right edge go before left
                g.DrawLine(GameColors.penCyan2, l, y, r, y); // bar
                g.DrawLine(GameColors.penCyan2, r, y - 4, r, y + 4); // right edge
                g.DrawLine(GameColors.penCyan2, l, y - 4, l, y + 4); // left edge
            }
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
