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
    internal partial class FormRuins : Form
    {
        /// <summary>
        /// The background map image
        /// </summary>
        private Image img;
        /// <summary>
        /// The the location of the mouse pointer, relative to site origin
        /// </summary>
        private PointF mousePos = new PointF();

        public MapViewData data;

        /// <summary>
        /// The center of the map image control
        /// </summary>
        public PointF mapCenter = new PointF();

        /// <summary>
        /// Offset from dragging the map around
        /// </summary>
        public Point dragOffset = new Point();
        private bool dragging = false;
        private Point mouseDownPoint;

        /// <summary>
        /// The scale factor for rendering
        /// </summary>
        private float scale = 1f;

        private SiteTemplate template;

        private PointF tp;

        private SitePOI nearestPoi;

        public FormRuins()
        {
            InitializeComponent();
            map.MouseWheel += Map_MouseWheel;

            //this.data = this.loadMap(@"D:\grinn\OneDrive\Pictures-x220\Frontier Developments\Elite Dangerous\foo\puddle-alpha-5.png");
            // this.template = SiteTemplate.sites[GuardianSiteData.SiteType.alpha];
            loadTemplates();

            this.data = this.loadMap(@"D:\grinn\OneDrive\Pictures-x220\Frontier Developments\Elite Dangerous\foo\beta.png");
            this.template = SiteTemplate.sites[GuardianSiteData.SiteType.beta];
        }

        private void loadTemplates()
        {
            SiteTemplate.Import(true);
            // ??
            foreach (var poi in SiteTemplate.sites[GuardianSiteData.SiteType.beta].poi)
                poi.angle = new Angle(poi.angle + 180);

        }

        private MapViewData loadMap(string filepath)
        {
            this.data = MapViewData.Load(filepath);

            using (var img = Image.FromFile(filepath))
                this.img = new Bitmap(img);

            this.numImgRotation.Value = (decimal)data.rotation;
            this.numImgScale.Value = (decimal)data.scaleFactor;
            this.txtSiteOrigin.Text = $"{data.siteOrigin.X},{data.siteOrigin.Y}";

            return this.data;
        }

        private void FormRuins_Load(object sender, EventArgs e)
        {
            this.parseOriginText();
            this.windowCalculations();
        }

        private void Map_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                this.scale *= 1.1f;
            else
                this.scale *= 0.9f;

            if (this.scale < 0.1) this.scale = 0.1f;
            if (this.scale > 10) this.scale = 10f;

            showStatus();
            map.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormRuins_ResizeEnd(object sender, EventArgs e)
        {
            // recalculate things once resizing stops
            this.windowCalculations();
        }

        private void windowCalculations()
        {
            this.mapCenter.X = map.Width / 2f;
            this.mapCenter.Y = map.Height / 2f;

            map.Invalidate();
        }

        private void parseOriginText()
        {
            if (string.IsNullOrEmpty(txtSiteOrigin.Text)) return;
            var idx = txtSiteOrigin.Text.IndexOf(",");
            if (idx < 0) return;

            var parts = txtSiteOrigin.Text.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return;

            int x, y;
            if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
            {
                this.data.siteOrigin = new Point(x, y);
                this.data.Save();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            loadTemplates();
            this.template = SiteTemplate.sites[GuardianSiteData.SiteType.beta];


            this.parseOriginText();
            this.windowCalculations();
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                this.dragOffset.X += mouseDownPoint.X - e.Location.X;
                this.dragOffset.Y += mouseDownPoint.Y - e.Location.Y;
                mouseDownPoint = e.Location;
            }
            else
            {
                this.mousePos = new PointF(
                    e.X - mapCenter.X,
                    e.Y - mapCenter.Y
                );
            }
            showStatus();
            map.Invalidate();
        }

        private void map_Click(object sender, EventArgs e)
        {
            //map.Invalidate();
        }

        private void map_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPoint = e.Location;
            dragging = true;
            map.Cursor = Cursors.SizeAll;
            showStatus();
        }

        private void map_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            map.Cursor = Cursors.Cross;
            showStatus();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numImgRotation.Value < 0) numImgRotation.Value += 360;
            if (numImgRotation.Value > 360) numImgRotation.Value -= 360;

            this.data.rotation = (float)numImgRotation.Value;
            this.data.Save();

            map.Invalidate();
            showStatus();
        }

        private void numImgScale_ValueChanged(object sender, EventArgs e)
        {
            data.scaleFactor = (float)numImgScale.Value;
            this.data.Save();

            map.Invalidate();
            showStatus();
        }

        private void showStatus()
        {
            var poi = this.nearestPoi!;
            if (poi != null)
                lblSelectedItem.Text = $"{poi.name} ({poi.type}) d: {poi.dist}, a: {poi.angle}° / " + new Angle(poi.angle + 180);
            else
                lblSelectedItem.Text = "";

            var x = (mousePos.X + dragOffset.X) / this.scale;
            var y = (mousePos.Y + dragOffset.Y) / this.scale;

            var dist = Math.Sqrt(x * x + y * y).ToString("N1");

            var a1 = Util.ToAngle(x, -y);
            var a2 = new Angle(a1 + 180);
            var a3 = ((double)a2).ToString("N2");


            lblStatus.Text = $"x: {x}, y: {y}, scale: {this.scale}, rotation: {data.rotation}° / dist: {dist} / angle: {a3}°";
        }

        private void map_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TranslateTransform(mapCenter.X - dragOffset.X, mapCenter.Y - dragOffset.Y);
            g.ScaleTransform(this.scale, this.scale);

            // apply rotation only to background image
            g.RotateTransform(data.rotation);

            var imgRect = new RectangleF(
                -data.siteOrigin.X * data.scaleFactor,
                -data.siteOrigin.Y * data.scaleFactor,
                img.Width * data.scaleFactor,
                img.Height * data.scaleFactor);
            g.DrawImage(this.img, imgRect);

            g.RotateTransform(-data.rotation);

            var compass = new Pen(Color.FromArgb(100, Color.Red)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Solid };
            g.DrawLine(compass, -map.Width, 0, map.Width, 0);
            g.DrawLine(compass, 0, -map.Height, 0, map.Height);

            drawArtifacts(g);

            drawLegend(g);
        }

        private void drawArtifacts(Graphics g)
        {
            var nearestDist = double.MaxValue;
            var nearestPt = PointF.Empty;
            nearestPoi = null!;

            foreach (var poi in template.poi)
            {
                // calculate render point for POI
                var pt = Util.rotateLine(
                    180 - poi.angle,
                    poi.dist);

                // is this the closest POI?
                var x = pt.X * this.scale - mousePos.X - dragOffset.X;
                var y = pt.Y * this.scale - mousePos.Y - dragOffset.Y;
                var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                if (d < nearestDist)
                {
                    nearestDist = d;
                    this.nearestPoi = poi;
                    nearestPt = new PointF(pt.X, pt.Y);
                }

                if (poi.type == POIType.relic)
                    drawRelicTower(g, pt);
                else
                    drawPuddle(g, pt, poi.type);
            }

            // draw highlight over closest POI
            g.DrawEllipse(GameColors.penCyan4, nearestPt.X - 14, nearestPt.Y - 14, 28, 28);

        }

        private void drawPuddle(Graphics g, PointF pt, POIType poiType)
        {
            var brush = new SolidBrush(Color.Orange);
            switch (poiType)
            {
                case POIType.orb: brush.Color = Color.FromArgb(255, 255, 127, 39); break; // orange
                case POIType.casket: brush.Color = Color.FromArgb(255, 34, 177, 76); break; // green
                case POIType.tablet: brush.Color = Color.FromArgb(255, 153, 217, 234); break; // blue
                case POIType.totem: brush.Color = Color.FromArgb(255, 63, 72, 204); break; // purple-ish
                case POIType.urn: brush.Color = Color.FromArgb(255, 163, 73, 164); break;
            }

            var d = 8;
            var rect = new RectangleF(pt.X - d, pt.Y - d, d * 2, d * 2);
            g.FillEllipse(brush, rect);

            var pen = new Pen(brush.Color, 3);
            switch (poiType)
            {
                case POIType.orb: pen.Color = Color.FromArgb(255, 147, 58, 0); break; // orange
                case POIType.casket: pen.Color = Color.FromArgb(255, 17, 87, 38); break; // green
                case POIType.tablet: pen.Color = Color.FromArgb(255, 33, 135, 160); break; // blue
                case POIType.totem: pen.Color = Color.FromArgb(255, 29, 34, 105); break; // purple-ish
                case POIType.urn: pen.Color = Color.FromArgb(255, 84, 37, 84); break; // pink-ish
            }

            g.DrawEllipse(pen, rect);

        }

        private void drawRelicTower(Graphics g, PointF pt)
        {
            PointF[] points =
            {
            new PointF(pt.X, pt.Y - 8),
            new PointF(pt.X + 8, pt.Y + 8),
            new PointF(pt.X - 8, pt.Y + 8),
            new PointF(pt.X, pt.Y - 8),
            };

            var brush = new SolidBrush(Color.Cyan);
            g.FillPolygon(brush, points);

            var pen = new Pen(Color.CadetBlue, 4)
            {
                DashStyle = DashStyle.Solid,
                StartCap = LineCap.Triangle,
                EndCap = LineCap.Triangle,
            };
            g.DrawLines(pen, points);
        }

        private void drawLegend(Graphics g)
        {
            tp.X = 20;
            tp.Y = 20;
            g.ResetTransform();
            var rect = new RectangleF(tp.X - 5, tp.Y - 5, 100, 140);

            g.FillRectangle(Brushes.LightGray, rect);
            var pen = new Pen(Color.DarkGray, 4);
            g.DrawRectangle(pen, rect);


            drawString(g, "Legend:");
            tp.X += 20;

            drawString(g, "Relic Tower");
            drawRelicTower(g, new PointF(tp.X - 10, tp.Y - 10));

            drawString(g, "Orb");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.orb);

            drawString(g, "Casket");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.casket);

            drawString(g, "Tablet");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.tablet);

            drawString(g, "Totem");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.totem);

            drawString(g, "Urn");
            drawPuddle(g, new PointF(tp.X - 10, tp.Y - 10), POIType.urn);
        }

        private void drawString(Graphics g, string msg)
        {
            var sz = g.MeasureString(msg, this.Font);
            g.DrawString(msg, this.Font, Brushes.Black, tp);
            tp.Y += sz.Height + 1;
        }
    }

    internal class MapViewData : Data
    {
        public static MapViewData Load(string filename)
        {
            var filepath = Path.Combine(Application.UserAppDataPath, "mapViewer", Path.GetFileNameWithoutExtension(filename) + ".json");

            return Data.Load<MapViewData>(filepath)
                ?? new MapViewData()
                {
                    filepath = filepath,
                };
        }

        /// <summary>
        /// The origin point in the image
        /// </summary>
        public Point siteOrigin = new Point();
        /// <summary>
        /// The scale factor for rendering
        /// </summary>
        public float rotation = 0f;
        /// <summary>
        /// The scale factor to apply to the image
        /// </summary>
        public float scaleFactor = 1f;
    }
}
