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
        private Angle touchdownHeading;
        private TrackingDelta td;


        public PlotGrounded()
        {
            InitializeComponent();

            game.status.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

        }

        private void PlotGrounded_Load(object sender, EventArgs e)
        {
            this.initialize();

            this.Game_modeChanged(game.mode);
            this.Status_StatusChanged();


            //this.Opacity = 1;
            game.journals.onJournalEntry += Journals_onJournalEntry;
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

        #endregion

        private Boolean isSuitableMode
        {
            get
            {
                return //true || // <== tmp!
                    game.isLanded;
            }
        }

        private void Game_modeChanged(GameMode newMode)
        {
            if (this.isSuitableMode)
            {
                this.Opacity = 0.5;

                this.floatLeftMiddle();
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
            this.td.Point1 = new LatLong2(game.status);
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

            this.td = new TrackingDelta(game.nearBody.radius, new LatLong2(game.status), this.touchdownLocation);
        }

        private GraphicsPath ship;
        private Font font = new Font(Game.settings.font1.FontFamily, 8f);


        private void PlotGrounded_Paint(object sender, PaintEventArgs e)
        {
            const float scale = 0.25f;

            var g = e.Graphics;

            Game.log(td);

            // draw basic compass cross hairs centered in the window
            g.ResetTransform();
            const float pad = 8;

            float w = this.Width / 2;
            float h = this.Height / 2;
            //var r = new RectangleF(-w, -h + 8, w * 2, +h * 2);
            var r = new RectangleF(0, pad, this.Width, this.Height- pad * 2);
            g.Clip = new Region(r);
            //g.FillRectangle(Brushes.Cyan, r);
            g.TranslateTransform(w, h);

            // think about this ...
            //g.RotateTransform(360 - game.status.Heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);


            // draw touchdown marker
            if (game.isLanded)
            {
                // delta to ship
                g.ResetTransform();
                g.TranslateTransform(w, h);
                g.ScaleTransform(scale, scale);

                //g.ScaleTransform(scale, scale);
                g.TranslateTransform((float)td.dx, -(float)td.dy);

                const float touchdownSize = 32f;
                g.FillEllipse(GameColors.brushOrangeStripe, -touchdownSize, -touchdownSize, touchdownSize, touchdownSize);

                //g.RotateTransform(this.touchdownHeading);
                //g.FillPath(Brushes.RoyalBlue, this.ship);

            }

            // draw current location pointer (always at center of plot + unscaled)
            var locSz = 5f;

            g.ResetTransform();
            g.TranslateTransform(w, h);
            //g.ScaleTransform(scale, scale);
            g.DrawEllipse(GameColors.Lime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.Lime2, 0, 0, +dx, -dy);

            drawFromTouchdownText(g);

            drawScale(g, scale);
        }

        private void drawScale(Graphics g, float scale)
        {
            const float pad = 8;

            g.ResetTransform();

            var dist = 100f;

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
            g.DrawLine(GameColors.penGameOrange2, x - dist, y - 4, x - dist, y + 4); ;


        }

        private void drawFromTouchdownText(Graphics g)
        {
            // distance from ship
            g.ResetTransform();
            var txt = $"Touchdown dist: "
                + Util.metersToString(this.td.distance)
                + " heading: "
                + this.td.angle.ToString();
            //Util. .metersToString(this.td.dx, true)
            //+ ", " + Util.metersToString(this.td.dy, true)
            //+ ")"
            //;

            var txtSz = g.MeasureString(txt, this.font);

            var foo = new StringFormat(); // StringFormatFlags);
            var r = new RectangleF(8, 8, this.Width - 20, txtSz.Height);
            g.DrawString(txt, this.font, GameColors.brushGameOrange, r, foo); // this.Height - 20);
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
}
