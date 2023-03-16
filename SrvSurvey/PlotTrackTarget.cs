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
        private Game game = Game.activeGame;

        private TrackingDelta td;

        private PlotTrackTarget()
        {
            InitializeComponent();
            this.TopMost = true;

            this.Height = 100;
            this.Width = 100;
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
                // roughly above the helmet lat/long indicator
                Elite.floatCenterTop(this, 0, 440);
            }
            else
            {
                // roughly above the game heading indicator
                Elite.floatCenterTop(this, 160);
            }
        }

        private void PlotGroundTarget_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect());
        }

        private void initialize()
        {
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
            this.td = new TrackingDelta(game.nearBody.radius, Game.settings.targetLatLong);

            game.status.StatusChanged += Status_StatusChanged;
            game.modeChanged += Game_modeChanged;

            this.plotPrep();

            this.calculate();
        }

        private void Game_modeChanged(GameMode newMode)
        {
            Game.log($"?? {game.vehicle} / {newMode} / {game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode)}");
            // Landed / OnFoot / InSrv
            
            if (this.Opacity == 0 && game.showBodyPlotters)
            {
                this.reposition(Elite.getWindowRect());
            }
            else if (this.Opacity != 0 && !game.showBodyPlotters)
            {
                this.Opacity = 0;
            }


            //if (game.nearBody != null && game.mode != GameMode.SuperCruising && game.mode != GameMode.Flying && game.mode != GameMode.Landed && game.mode != GameMode.InSrv && game.mode != GameMode.OnFoot && game.mode != GameMode.GlideMode)
            //    this.Opacity = 0;
            //else
            //    this.reposition(Overlay.getEDWindowRect());
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
            pp.AddLine(p1, new Point(20, 30));
            pp.AddLine(p1, new Point(-20, 30));

            tt = new GraphicsPath(FillMode.Winding);
            tt.AddPolygon(new Point[] {
                new Point(0, -50),
                new Point(18, 30),
                new Point(-18,  30),
            });
            tt.PathPoints[1].X += 20;
        }

        GraphicsPath pp;
        GraphicsPath tt;

        private static Pen pen1 = new Pen(Color.FromArgb(255, 249, 165, 0), 3);
        private void plotPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            float w = this.Width / 2;
            float h = this.Height / 2;

            // draw heading text (center bottom)
            var headingTxt = ((int)this.td.angle).ToString();
            var sz = g.MeasureString(headingTxt, Game.settings.fontSmall);
            var tx = w - (sz.Width / 2);
            var ty = this.Height - sz.Height - 6;
            g.DrawString(this.td.angle.ToString(), Game.settings.fontSmall, Brushes.Orange, tx, ty);

            // draw distance text (top left corner
            var txt = "Distance: " + Util.metersToString(td.distance + game.status.Altitude);
            g.DrawString(txt, Game.settings.fontSmall, Brushes.Orange, 4, 10);

            // assuming panel is 200 x 200
            g.TranslateTransform(w, h);

            // rotate so the arrow aligns "up" is forwards/infront of us
            var da = (int)td.angle - game.status.Heading;
            g.RotateTransform(da);
            g.ScaleTransform(0.3F, 0.3F);

            // clip to prevent filling
            float fill = (float)(td.complete);
            var clipR = new RectangleF(-100, -60, 200, 100 - fill);
            g.Clip = new Region(clipR);
            g.FillPath(Brushes.Yellow, tt);

            // draw thd rest unclipped
            g.Clip = new Region();
            g.DrawPath(GameColors.penGameOrange4, pp);
        }

        private void PlotTrackTarget_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Invalidate();
        }

        private void PlotTrackTarget_MouseClick(object sender, MouseEventArgs e)
        {
            Elite.setFocusED();
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
