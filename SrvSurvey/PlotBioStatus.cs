using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private Game game = Game.activeGame;

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
            this.Game_modeChanged(game.mode);

            //this.Opacity = 1;
            game.journals.onJournalEntry += Journals_onJournalEntry;
            game.nearBody.bioScanEvent += NearBody_bioScanEvent;

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

        private void Game_modeChanged(GameMode newMode)
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
        }

        private void PlotBioStatus_Click(object sender, EventArgs e)
        {
            Elite.setFocusED();
        }

        private void PlotBioStatus_Paint(object sender, PaintEventArgs e)
        {
            if (game?.nearBody?.Genuses == null) return;

            var g = e.Graphics;

            g.DrawString(
                $"Biological signals: {game.nearBody.Genuses.Count} | Analyzed: {game.nearBody.analysedSpecies.Count}",
                Game.settings.fontSmall, GameColors.brushGameOrange, 4, 6);

            if (game.nearBody.scanOne == null)
                this.showAllGenus(g);
            else
                this.showCurrentGenus(g);

        }

        private void showCurrentGenus(Graphics g)
        {
            // all the Genus names
            float x = 110;
            float y = 28;

            var r = new RectangleF(8, y, 24, 24);
            g.FillEllipse(GameColors.brushGameOrangeDim, r);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            r = new RectangleF(40, y, 24, 24);
            if (game.nearBody.scanTwo != null)
            {
                g.FillEllipse(GameColors.brushGameOrangeDim, r);
                g.DrawEllipse(GameColors.penGameOrange2, r);
            }
            else
            {
                g.DrawEllipse(GameColors.penGameOrange2, r);
            }

            r = new RectangleF(72, y, 24, 24);
            g.DrawEllipse(GameColors.penGameOrange2, r);

            g.DrawString(
                $"{game.nearBody.scanOne.speciesLocalized}",
                Game.settings.fontBig, GameColors.brushCyan,
                104, y - 8);

            this.drawScale(g, game.nearBody.scanOne.radius, 0.25f);
        }

        private void drawScale(Graphics g, float dist, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, Game.settings.fontSmall);
            float w = this.Width / 2;
            //var r = new RectangleF(8, this.Height - 8 - txtSz.Height, w, txtSz.Height);
            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height;

            g.DrawString(txt, Game.settings.fontSmall, GameColors.brushCyan,
                x, //this.Width - pad - txtSz.Width, // x
                y, //this.Height - pad - txtSz.Height, // y
                   //2 * x + dist, y + 1 - txtSz.Height / 2, 
                StringFormat.GenericTypographic);

            x -= pad;
            y += pad - 2;

            dist *= scale;

            g.DrawLine(GameColors.penCyan2, x, y, x - dist, y);
            g.DrawLine(GameColors.penCyan2, x, y - 4, x, y + 4);
            g.DrawLine(GameColors.penCyan2, x - dist, y - 4, x - dist, y + 4);
        }

        private void showAllGenus(Graphics g)
        {
            //g.DrawString(
            //    $"{game.nearBody.analysedSpecies.Count}",
            //    this.fontBig, GameColors.brushCyan, 4, 21);

            // all the Genus names
            float x = 32;
            float y = 24;
            //foreach (var name in BioScan2.ranges.Keys)
            foreach (var genus in game.nearBody.Genuses)
            {
                var analysed = game.nearBody.analysedSpecies.ContainsKey(genus.Genus);

                var txt = genus.Genus_Localised;
                // TODO: show distance?
                //if (!analysed) txt += " (50m)";

                var sz = g.MeasureString(txt, Game.settings.fontSmall);

                if (x + sz.Width > this.Width - 16)
                {
                    x = 110;
                    y += sz.Height;
                }

                g.DrawString(
                    txt,
                    Game.settings.fontSmall,
                    analysed ? GameColors.brushGameOrange : GameColors.brushCyan,
                    x, y);

                x += sz.Width + 8;
            }
        }
    }
}
