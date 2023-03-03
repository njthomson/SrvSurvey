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
    public partial class PlotGrounded : Form
    {
        private Game game = Game.activeGame;

        private LatLong2 touchdownLocation;
        private TrackingDelta srvLocation;

        private Angle touchdownHeading;
        private TrackingDelta td;
        private List<BioScan> bioScans = new List<BioScan>();


        public PlotGrounded()
        {
            InitializeComponent();

            //this.Height = 200;
            //this.Width = 200;
        }

        private void PlotGrounded_Load(object sender, EventArgs e)
        {
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
        //private void onJournalEntry(ScanOrganic entry)
        //{
        //    Game.log($"ScanOrganic: {entry.ScanType}: {entry.Genus} / {entry.Species}");
        //    this.addBioScan(entry);
        //}

        private void onJournalEntry(SendText entry)
        {
            switch (entry.Message)
            {
                case "11":

                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Log,
                        Genus = "$Codex_Ent_Sphere_Name;",
                        Species = "Anemone Foo"
                    });
                    return;
                case "12":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Sample,
                        Genus = "$Codex_Ent_Sphere_Name;",
                        Species = "Anemone Foo"
                    });
                    return;
                case "13":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Analyse,
                        Genus = "$Codex_Ent_Sphere_Name;",
                        Species = "Anemone Foo"
                    });
                    return;

                case "21":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Log,
                        Genus = "$Codex_Ent_Stratum_Genus_Name;",
                        Species = "Stratum Tectonicas"
                    });
                    return;
                case "22":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Sample,
                        Genus = "$Codex_Ent_Stratum_Genus_Name;",
                        Species = "Stratum Tectonicas"
                    });
                    return;
                case "23":
                    game.nearBody.addBioScan(new ScanOrganic
                    {
                        ScanType = ScanType.Analyse,
                        Genus = "$Codex_Ent_Stratum_Genus_Name;",
                        Species = "Stratum Tectonicas"
                    });
                    return;
            }
            //}
            //var newScan = new BioScan()
            //{
            //    location = new LatLong2(game.status),
            //    radius = BioScan.ranges[fakeGenus],
            //    genus = fakeGenus,
            //};
            //this.bioScans.Add(newScan);
            //Game.log($"Fake scan: {newScan}");
            this.Invalidate();


            if (entry.Message == "==")
            {
                // mark all as completed
                foreach (var scan in this.bioScans)
                    scan.scanType = ScanType.Analyse;

                this.Invalidate();
            }

        }

        private void onJournalEntry(Disembark entry)
        {
            Game.log($"Disembark");
            if (entry.SRV)
            {
                this.srvLocation = new TrackingDelta(game.nearBody.radius, new LatLong2(game.status), new LatLong2(game.status));
            }
            this.Invalidate();
        }
        private void onJournalEntry(Embark entry)
        {
            Game.log($"Embark");
            if (entry.SRV)
            {
                this.srvLocation = null;
            }
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
                Overlay.floatTopRight(this, 160, 20);
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
            }
            if (this.srvLocation != null)
            {
                this.srvLocation.Point1 = new LatLong2(game.status);
            }

            this.Invalidate();
        }

        private void PlotGrounded_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);

            // prepare ship graphic
            var gp = new GraphicsPath(FillMode.Winding);

            gp.AddPolygon(new Point[] {
                // nose
                new Point(4-6, 0 -10),
                new Point(8-6, 0-10),
                // right side
                new Point(12-6, 8-10),
                new Point(12-6, 12-10),
                new Point(8-6, 12-10),
                //new Point(10, 20),
                new Point(12-6, 20-10),
                // bottom horiz
                new Point(0-6, 20-10),
                // left side
                new Point(4-6, 12-10),
                //new Point(5, 15),
                new Point(0-6, 12-10),
                new Point(0-6, 8-10),
                new Point(4-6, 0-10),
            });
            this.ship = gp;

            // prepare SRV graphic
            // TODO: ...

            // get landing location
            this.touchdownLocation = game.touchdownLocation;
            this.touchdownHeading = game.touchdownHeading;
            Game.log($"touchdownLocation: {this.touchdownLocation}, heading {this.touchdownHeading}");

            if (this.touchdownLocation != null)
            {
                this.td = new TrackingDelta(
                    game.nearBody.radius,
                    new LatLong2(game.status),
                    this.touchdownLocation);
            }
        }

        private GraphicsPath ship;
        private Font font = new Font(Game.settings.font1.FontFamily, 8f);


        private void PlotGrounded_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            const float scale = 0.25f;

            // draw basic compass cross hairs centered in the window
            float w = this.Width / 2;
            float h = this.Height / 2;

            this.drawCompassLines(g);

            this.drawBioScans(g, scale);

            // draw touchdown marker
            if (game.isLanded && td != null)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(w, h);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status.Heading);

                const float touchdownSize = 64f;
                var rect = new RectangleF((float)td.dx - touchdownSize, (float)-td.dy - touchdownSize, touchdownSize * 2, touchdownSize * 2);
                g.FillEllipse(GameColors.brushShipLocation, rect);
            }

            // draw SRV marker
            if (game.isLanded && this.srvLocation != null)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(w, h);
                g.ScaleTransform(scale, scale);
                g.RotateTransform(360 - game.status.Heading);

                const float srvSize = 32f;
                var rect = new RectangleF((float)srvLocation.dx - srvSize, (float)-srvLocation.dy - srvSize, srvSize * 2, srvSize * 2);
                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }


            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(w, h);
            g.RotateTransform(360 - game.status.Heading);

            var locSz = 5f;
            g.DrawEllipse(GameColors.Lime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.Lime2, 0, 0, +dx, -dy);

            drawScale(g, scale);
        }

        private void drawCompassLines(Graphics g)
        {
            g.ResetTransform();
            const float pad = 8;

            float w = this.Width / 2;
            float h = this.Height / 2;
            var r = new RectangleF(0, pad, this.Width, this.Height - pad * 2);
            g.Clip = new Region(r);
            g.TranslateTransform(w, h);

            // draw compass rose lines
            g.RotateTransform(360 - game.status.Heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);
        }

        private void drawBioScans(Graphics g, float scale)
        {
            if (game.nearBody == null || this.td == null) return;

            float w = this.Width / 2;
            float h = this.Height / 2;

            // delta to ship
            g.ResetTransform();
            g.TranslateTransform(w, h);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(360 - game.status.Heading);

            // use the same Tracking delta for all bioScans against the same currentLocation
            var currentLocation = new LatLong2(this.game.status);
            var d = new TrackingDelta(game.nearBody.radius, currentLocation, currentLocation);
            foreach (var scan in game.nearBody.completedScans)
            {
                drawBioScan(g, d, scan);
            }

            var txt = $"Distances: {Util.metersToString(this.td.distance)}";

            if (game.nearBody.scanOne != null)
            {
                drawBioScan(g, d, game.nearBody.scanOne);
                txt += $" / { Util.metersToString(d.distance)}";
            }
            if (game.nearBody.scanTwo != null)
            {
                drawBioScan(g, d, game.nearBody.scanTwo);
                txt += $" / {Util.metersToString(d.distance)}";
            }

            // draw distances along the top
            g.ResetTransform();
            var txtSz = g.MeasureString(txt, this.font);
            var r = new RectangleF(8, 8, this.Width - 20, txtSz.Height);
            g.DrawString(txt, this.font, GameColors.brushGameOrange, r, StringFormat.GenericTypographic);;
        }

        private void drawBioScan(Graphics g, TrackingDelta d, BioScan scan)
        {
            var fudge = 10;

            d.Point2 = scan.location;
            var rect = new RectangleF((float)-d.dx - scan.radius, (float)d.dy - scan.radius, scan.radius * 2, scan.radius * 2);
            Game.log($"d.dx: {rect.X}, d.dy: {rect.Y}");

            var complete = scan.scanType == ScanType.Analyse;
            g.FillEllipse(complete ? GameColors.brushExclusionComplete : GameColors.brushExclusionActive, rect);
            rect.Inflate(fudge, fudge);
            g.DrawEllipse(complete ? GameColors.penExclusionComplete : GameColors.penExclusionActive, rect);
            rect.Inflate(fudge, fudge);
            g.DrawEllipse(complete ? GameColors.penExclusionComplete : GameColors.penExclusionActive, rect);
        }


        private void drawScale(Graphics g, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            float dist = game.nearBody.scanOne?.radius ?? 100;

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

        private void PlotGrounded_DoubleClick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void PlotGrounded_Click(object sender, EventArgs e)
        {
            Overlay.setFocusED();
        }

    }

    class BioScan0
    {
        public long radius;
        public string genus;
        public string species;

        public static Dictionary<string, int> ranges = new Dictionary<string, int>()
        {
            { "Aleoida",            150 },
            { "Anemone",            100 },
            { "Bacterium",          500 },
            { "Bark Mound",         100 },
            { "Brain Tree",         100 },
            { "Cactoida",           300 },
            { "Clypeus",            150 },
            { "Concha",             150 },
            { "Crystalline Shard",  100 },
            { "Electricae",        1000 },
            { "Fonticulua",         500 },
            { "Frutexa",            150 },
            { "Fumerola",           100 },
            { "Fungoida",           300 },
            { "Osseus",             800 },
            { "Recepta",            150 },
            { "Sinuous Tuber",      100 },
            { "Stratum",            500 },
            { "Tubus",              800 },
            { "Tussock",            200 },

        };
        /*
                Species Range
        Aleoida 	150m 
        Anemone 	100m 
        Bacterium 	500m 
        Bark Mound	100m 
        Brain Tree 	100m 
        Cactoida 	300m 
        Clypeus 	150m 
        Concha 	150m 
        Crystalline Shard 	100m 
        Electricae 	1000m 
        Fonticulua 	500m 
        Frutexa 	150m 
        Fumerola 	100m 
        Fungoida 	300m 
        Osseus 	800m 
        Recepta 	150m 
        Sinuous Tuber 	100m 
        Stratum 	500m 
        Tubus 	800m 
        Tussock 	200m 
        */

    }
}
