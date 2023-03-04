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

namespace SrvSurvey
{
    partial class PlotTrackTarget : Form, PlotterForm
    {
        LandableBody targetBody;
        LatLong2 targetLocation;
        private Game game = Game.activeGame;

        private TrackingDelta td;

        private PlotTrackTarget()
        {
            InitializeComponent();
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            if (game.mode == GameMode.OnFoot)
            {
                this.Left = gameRect.Right - this.Width;
                this.Top = gameRect.Top + 60;
            }
            else
            {
                this.Left = gameRect.Left + (gameRect.Width / 2) - (this.Width / 2);
                this.Top = gameRect.Top + 160;
            }
        }

        public void setTarget(LandableBody targetBody, LatLong2 targetLocation)
        {
            this.targetBody = targetBody;
            this.targetLocation = targetLocation;

            this.td = new TrackingDelta(targetBody.radius, game.location, targetLocation);
            //this.textBox1.Text = targetBody.name + " / " + targetLocation.ToString();

            game.status.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

            this.plotPrep();

            // force immediate calculation
            this.calculate();
        }

        private void PlotGroundTarget_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            //this.Opacity = Game.settings.Opacity;

            this.reposition(Overlay.getEDWindowRect());
        }

        private void Game_modeChanged(GameMode newMode)
        {
            if (game.nearBody != null && game.mode != GameMode.SuperCruising && game.mode != GameMode.Flying && game.mode != GameMode.Landed && game.mode != GameMode.InSrv && game.mode != GameMode.OnFoot && game.mode != GameMode.GlideMode)
                this.Opacity = 0;
            else
                this.reposition(Overlay.getEDWindowRect());
        }

        private void Status_StatusChanged()
        {
            this.td.Point2 = game.location;
            this.calculate();
        }

        private void calculate()
        {
            // exit early if we're not in the right mode
            if (game.nearBody != null && game.mode != GameMode.SuperCruising && game.mode != GameMode.Flying && game.mode != GameMode.Landed && game.mode != GameMode.InSrv && game.mode != GameMode.OnFoot && game.mode != GameMode.GlideMode)
                return;


            //this.Text = td.ToString();
            //textBox2.Text = td.angle.ToString() + " / " + td.MetersToString(td.distance);
            //Game.log(td);
            this.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void plotPrep()
        {
            // prep for plotting
            var p1 = new Point(0, -50);

            pp = new GraphicsPath(FillMode.Alternate);
            //pp.AddLine(p1, new Point(30, 0));
            //pp.AddLine(p1, new Point(-30, 0));
            pp.AddLine(p1, new Point(20, 30));
            pp.AddLine(p1, new Point(-20, 30));

            tt = new GraphicsPath(FillMode.Winding);
            tt.AddPolygon(new Point[] {
                new Point(0, -50),
                new Point(18, 30),
                new Point(-18,  30),
            });
            tt.PathPoints[1].X += 20;
            //tt.AddLine(p1, new Point(12, 48));
            //tt.AddLine(p1, new Point(-12, 48));
            //tt.AddLine(p1, new Point(-12, 48));

        }

        GraphicsPath pp;
        GraphicsPath tt;

        Font font1 = new System.Drawing.Font("Lucida Sans Typewriter", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        private static Pen pen1 = new Pen(Color.FromArgb(255, 249, 165, 0), 3);
        private void plotPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            // assuming panel is 200 x 200
            //g.Clear(Color.Black);

            g.TranslateTransform(100, 60);
            //g.DrawLine(Pens.Red, 0, -100, 0, 100);

            var sz = g.MeasureString(((int)this.td.angle).ToString(), font1);
            var tx = 0 - (sz.Width / 2);
            var ty = 55; // - (sz.Height / 2);
            g.DrawString(this.td.angle.ToString(), this.font1, Brushes.Orange, tx, ty);

            var txt = Util.metersToString(td.distance + game.status.Altitude);
            sz = g.MeasureString(txt, font1);
            tx = 0 - (sz.Width / 2);
            ty = 75;
            g.DrawString(txt, this.font1, Brushes.Orange, tx, ty);

            // rotate so the arrow aligns "up" is forwards/infront of us
            var da = (int)td.angle - game.status.Heading;
            g.RotateTransform(da);

            // clip to prevent filling
            float h = (float)(td.complete);
            var clipR = new RectangleF(-100, -60, 200, 100 - h);
            g.Clip = new Region(clipR);
            g.FillPath(Brushes.Yellow, tt);

            // draw teh rest unclipped
            g.Clip = new Region();
            g.DrawPath(pen1, pp);
        }

        //private void floatCenterTop()
        //{
        //    // position form top center above the heading
        //    var rect = Overlay.getEDWindowRect();

        //    this.Left = rect.Left + (rect.Width / 2) - (this.Width / 2);
        //    this.Top = rect.Top + 160;
        //}

        //private void floatTopRight()
        //{
        //    // position form top center above the heading
        //    var rect = Overlay.getEDWindowRect();

        //    this.Left = rect.Right - this.Width;
        //    this.Top = rect.Top + 60;
        //}
    }
}
