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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SrvSurvey
{
    internal partial class PlotGuardians : PlotBase
    {
        private SiteTemplate? template;
        private Image? siteMap;
        private Image? trails;
        private Image? underlay;
        private float ux;
        private float uy;

        private float siteHeading = 281;
        private PointF siteTouchdownOffset;
        private PointF commanderOffset;

        private PlotGuardians() : base()
        {
            InitializeComponent();
            SiteTemplate.Import();
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatLeftMiddle(this);
        }

        private void PlotGuardians_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect());
        }

        protected override void initialize()
        {
            base.initialize();
            this.loadSiteTemplate(SiteType.alpha);
        }

        private void PlotGuardians_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Invalidate();
        }

        private void PlotGuardians_MouseClick(object sender, MouseEventArgs e)
        {
            Elite.setFocusED();
        }

        protected override void onJournalEntry(SendText entry)
        {
            Game.log($"SendText {entry.Message}");
            if (entry.Message.StartsWith("!"))
            {
                this.siteHeading = float.Parse(entry.Message.Substring(1));
            }
            this.Invalidate();
        }

        private void loadSiteTemplate(SiteType type)
        {
            this.template = SiteTemplate.sites[type];
            this.scale = this.template.mapScale;

            this.siteMap = Bitmap.FromFile(this.template.backgroundImage);

            // prepare underlay
            this.underlay = new Bitmap(this.siteMap.Width * 2, this.siteMap.Height * 2);
            this.ux = this.siteMap.Width;
            this.uy = this.siteMap.Height;

            // prepare other stuff
            var foo = game.nearBody!.guardianSiteLocation! - touchdownLocation.Point2;
            Game.log(foo);
            this.siteTouchdownOffset = new PointF(
                (float)(foo.Long * template.scaleFactor),
                (float)(foo.Lat * -template.scaleFactor));
            Game.log(siteTouchdownOffset);
            Game.log($"siteTouchdownOffset: {siteTouchdownOffset}");
        }

        protected override void Status_StatusChanged()
        {
            base.Status_StatusChanged();

            // prepare other stuff
            if (template != null)
            {
                var foo = game.nearBody!.guardianSiteLocation! - game.status!.here;
                //Game.log(foo);
                this.commanderOffset = new PointF(
                    (float)(foo.Long * template.scaleFactor),
                    (float)(foo.Lat * -template.scaleFactor));
                Game.log($"commanderOffset: {commanderOffset}");
            }
        }

        private void PlotGuardians_Paint(object sender, PaintEventArgs e)
        {
            this.g = e.Graphics;

            this.drawSiteMap();

            float x = (float)touchdownLocation.dx; // +commanderOffset.X;
            float y = (float)touchdownLocation.dy;

            //this.drawTouchdownAndSrvLocation(x, y);

            this.drawCommander();

            this.drawSiteSummaryFooter();
        }

        private void drawSiteMap()
        {
            float mapScale = this.template!.mapScale;

            // prepare underlay image
            using (var gg = Graphics.FromImage(this.underlay!))
            {
                gg.Clear(Color.Transparent);
                // shift by underlay size
                gg.TranslateTransform(ux, uy);
                // rotate by site heading only
                gg.RotateTransform(+this.siteHeading);

                // draw the site bitmap and trails(?)
                gg.DrawImageUnscaled(this.siteMap!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);
                //g.RotateTransform(+dda);
                //g.DrawImageUnscaled(this.trails, -this.template.imageOffset.X, -this.template.imageOffset.Y);
            }

            float x = commanderOffset.X;
            float y = commanderOffset.Y;
            //float x = +commanderOffset.X + siteTouchdownOffset.X;
            //float y = +commanderOffset.Y + siteTouchdownOffset.Y;
            //float x = + siteTouchdownOffset.X;
            //float y = + siteTouchdownOffset.Y;

            // draw underlay offset 
            g.ResetTransform();
            this.clipToMiddle(4, 24, 4, 24);
            g.TranslateTransform(mid.Width, mid.Height);
            g.RotateTransform(-game.status.Heading);
            g.ScaleTransform(mapScale, mapScale);
            g.DrawImageUnscaled(this.underlay, -(int)(ux - x), -(int)(uy - y));

            // draw compass rose lines centered on underlay
            //g.ResetTransform();
            //g.TranslateTransform(mid.Width , mid.Height );
            //g.RotateTransform(-game.status!.Heading);
            //g.ScaleTransform(mapScale, mapScale);

            g.DrawLine(Pens.DarkRed, -this.Width * 2, y, +this.Width * 2, y);
            g.DrawLine(Pens.DarkRed, x, y, x, +this.Height * 2);
            g.DrawLine(Pens.Red, x, -this.Height * 2, x, y);
        }


        private void drawSiteSummaryFooter()
        {
            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();
            var headingTxt = $"Site heading: {siteHeading}°";
            var sz = g.MeasureString(headingTxt, Game.settings.fontSmall);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 6;
            g.DrawString(headingTxt, Game.settings.fontSmall, Brushes.Orange, tx, ty);
        }
    }
}
