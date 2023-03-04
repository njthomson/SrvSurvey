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
    public partial class PlotBioStatus : Form
    {
        private Game game;

        private LatLong2 touchdownLocation;
        private Angle touchdownHeading;
        private TrackingDelta td;

        private string currentGenus;
        private int scanCount;


        private PlotBioStatus()
        {
            InitializeComponent();
            var foo = typeof(PlotBioStatus);

            this.Height = 80;
            this.Width = 400;
        }

        private void PlotBioStatus_Load(object sender, EventArgs e)
        {
            game = Game.activeGame;
            game.status.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

            // force a mode switch, that will initialize
            this.Game_modeChanged(game.mode);
            this.Status_StatusChanged();

            //this.Opacity = 1;
            game.journals.onJournalEntry += Journals_onJournalEntry;
            game.nearBody.bioScanEvent += NearBody_bioScanEvent;
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

        private void onJournalEntry(JournalEntry entry)
        {
            // ignore
        }

        private void onJournalEntry(Touchdown entry)
        {
            this.touchdownLocation = game.touchdownLocation;
            this.touchdownHeading = game.touchdownHeading;
            Game.log($"re-touchdownLocation: {this.touchdownLocation}, heading {this.touchdownHeading}");
            this.Invalidate();
        }

        private void onJournalEntry(Liftoff entry)
        {
            Game.log($"re-liftoff");
            this.Invalidate();
        }

        private void onJournalEntry(ScanOrganic entry)
        {
            Game.log($"ScanOrganic: {entry.ScanType}: {entry.Genus} / {entry.Species}");
            this.Invalidate();
        }

        #endregion

        private void Game_modeChanged(GameMode newMode)
        {
            if (game.isLanded)
            {
                if (this.td == null)
                {
                    this.initialize();
                }

                this.Opacity = 0.5;
                Overlay.floatCenterTop(this, 0);
                //this.floatLeftMiddle();
            }
            else
            {
                this.Opacity = 0;
            }
        }

        private void floatLeftMiddle()
        {
            // position form top center above the heading
            var rect = Overlay.getEDWindowRect();

            this.Left = rect.Left + 40;
            this.Top = rect.Top + (rect.Height / 2) - (this.Height / 2);
        }

        private void Status_StatusChanged()
        {
            //throw new NotImplementedException();
            //this.currentLocation = new LatLong2(game.status);
            if (this.td != null)
            {
                this.td.Point1 = new LatLong2(game.status);
                this.Invalidate();
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
            Overlay.setFocusED();
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            //// prepare ship graphic
            //var gp = new GraphicsPath(FillMode.Winding);

            //gp.AddPolygon(new Point[] {
            //    // nose
            //    new Point(4-6, 0 -10),
            //    new Point(8-6, 0-10),
            //    // right side
            //    new Point(12-6, 8-10),
            //    new Point(12-6, 12-10),
            //    new Point(8-6, 12-10),
            //    //new Point(10, 20),
            //    new Point(12-6, 20-10),
            //    // bottom horiz
            //    new Point(0-6, 20-10),
            //    // left side
            //    new Point(4-6, 12-10),
            //    //new Point(5, 15),
            //    new Point(0-6, 12-10),
            //    new Point(0-6, 8-10),
            //    new Point(4-6, 0-10),
            //});
            //this.ship = gp;

            // prepare SRV graphic
            // TODO: ...

            this.currentGenus = "Crystalline Shard";
            this.scanCount = 1;
        }

        private GraphicsPath ship;
        private Font font = new Font(Game.settings.font2.FontFamily, 8f);
        private Font fontBig = new Font(Game.settings.font2.FontFamily, 22f);
        private Font fontSmall = new Font(Game.settings.font2.FontFamily, 6f);


        private void PlotBioStatus_Paint(object sender, PaintEventArgs e)
        {
            if (game.nearBody.Genuses == null) return;

            var g = e.Graphics;

            g.DrawString(
                "Biological signals:",
                this.font, GameColors.brushGameOrange, 4, 4);

            g.DrawString(
                $"{game.nearBody.analysedSpecies.Count} of {game.nearBody.Genuses.Count}",
                this.fontBig, GameColors.brushGameOrange, 4, 21);

            if (game.nearBody.scanOne == null)
                this.showAllGenus(g);
            else 
                this.showCurrentGenus(g);

        }

        private void showCurrentGenus(Graphics g)
        {
            // all the Genus names
            float x = 110;
            float y = 16;

            g.DrawString(
                $"{game.nearBody.scanOne.speciesLocalized}",
                this.fontBig, GameColors.brushGameOrange,
                x, y);


            this.drawScale(g, game.nearBody.scanOne.radius, 0.25f);

        }

        private void drawScale(Graphics g, float dist, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            var txt = Util.metersToString(dist);
            var txtSz = g.MeasureString(txt, this.font);
            float w = this.Width / 2;
            //var r = new RectangleF(8, this.Height - 8 - txtSz.Height, w, txtSz.Height);
            var x = this.Width - pad - txtSz.Width;
            var y = this.Height - pad - txtSz.Height;

            g.DrawString(txt, this.font, GameColors.brushGameOrange,
                x, //this.Width - pad - txtSz.Width, // x
                y, //this.Height - pad - txtSz.Height, // y
                   //2 * x + dist, y + 1 - txtSz.Height / 2, 
                StringFormat.GenericTypographic);



            //x = 8;
            //y = this.Height - 16;

            x -= pad;
            y += pad - 2;

            dist *= scale;
            //g.ScaleTransform(scale, scale);

            g.DrawLine(GameColors.penGameOrange2, x, y, x - dist, y);
            g.DrawLine(GameColors.penGameOrange2, x, y - 4, x, y + 4);
            g.DrawLine(GameColors.penGameOrange2, x - dist, y - 4, x - dist, y + 4);
        }

        private void showAllGenus(Graphics g)
        {
            // all the Genus names
            float x = 110;
            float y = 16;
            //foreach (var name in BioScan2.ranges.Keys)
            foreach (var genus in game.nearBody.Genuses)
            {
                var sz = g.MeasureString(genus.Genus_Localised, this.font);

                if (x + sz.Width > this.Width - 16)
                {
                    x = 110;
                    y += sz.Height;
                }

                var analysed = game.nearBody.analysedSpecies.ContainsKey(genus.Genus);

                g.DrawString(
                    genus.Genus_Localised,
                    this.font,
                    analysed ? Brushes.Green : GameColors.brushGameOrange,
                    x, y);

                x += sz.Width + 8;
            }
        }
    }

}
