using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SrvSurvey.plotters
{
    [System.ComponentModel.DesignerCategory("")]
    internal class BigOverlay : Form
    {
        public static BigOverlay? current;

        /// <summary> Invalidates the big overlay, using Program.defer </summary>
        public static void invalidate()
        {
            if (BigOverlay.current != null)
                Program.defer(() => BigOverlay.current?.Invalidate());
        }

        private static Color maskColor = Color.FromArgb(1, 1, 1);

        public BigOverlay()
        {
            BigOverlay.current = this;

            this.TopMost = true;
            this.Cursor = Cursors.Cross;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.Name = this.GetType().Name;
            this.ResizeRedraw = true;
            this.Size = new Size(640, 640);

            // This needs to be a colour that won't naturally appear
            this.BackColor = maskColor;
            this.TransparencyKey = maskColor;

            this.Opacity = Game.settings.Opacity;

            this.Text = "SrvSurveyOne";

            this.initPlotPulse();
            this.reposition(Elite.getWindowRect());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PlotBase2.closeAll();
            BigOverlay.current = null!;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 + 0x00080000 + 0x08000000; // WS_EX_TRANSPARENT + WS_EX_LAYERED + WS_EX_NOACTIVATE
                return cp;
            }
        }

        public void reposition(Rectangle rect)
        {
            Game.log($"bigOne @ {rect} / {this.Visible} / {this.Opacity}");

            if (rect.X <= -30_000)
            {
                this.Hide();
            }
            else
            {
                this.SuspendLayout();
                this.Location = rect.Location;
                this.Size = rect.Size;
                this.ResumeLayout();

                // get PlotPulse location
                this.ptPlotPulse = PlotPos.getPlotterLocation("PlotPulse", plotPulseDefaultSize, rect, true);

                if (Game.activeGame != null)
                {
                    foreach (var plotter in PlotBase2.active)
                        plotter.setPosition(rect);

                    PlotBase2.renderAll(Game.activeGame);
                }

                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;

            try
            {
                var g = e.Graphics;

                var game = Game.activeGame;
                if (game != null)
                {
                    if (!Game.settings.hideJournalWriteTimer)
                        this.renderPulse(g);

                    foreach (var plotter in PlotBase2.active)
                    {
                        // skip anything not visible
                        if (plotter.hidden) continue;

#if DEBUG
                        if (plotter.stale) // comment for easier debugging of rendering code
                            plotter.render();
#else
                        // re-render only if needed
                        if (plotter.stale) plotter.render();
#endif

                        if (plotter.fade == 0)
                        {
                            // start fading in
                            Util.deferAfter(20, () => fadeNext2((PlotBase2)plotter, 0.1f), plotter.name);
                        }
                        else if (plotter.fade == 1)
                        {
                            // fading has finished
                            g.DrawImageUnscaled(plotter.background, plotter.left, plotter.top);
                            g.DrawImageUnscaled(plotter.frame, plotter.left, plotter.top);
                        }
                        else
                        {
                            // draw faded image
                            var attr = new ImageAttributes();
                            var colorMatrix = new ColorMatrix() { Matrix33 = plotter.fade, };
                            attr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                            var sz = plotter.frame.Size;
                            var rect = new Rectangle(plotter.left, plotter.top, sz.Width, sz.Height);

                            // we need a black box or the blending looks too bright
                            g.FillRectangle(Brushes.Black, rect);

                            g.DrawImage(plotter.background, rect, 0, 0, sz.Width, sz.Height, GraphicsUnit.Pixel, attr);
                            g.DrawImage(plotter.frame, rect, 0, 0, sz.Width, sz.Height, GraphicsUnit.Pixel, attr);
                        }

                        if (plotter.name == FormAdjustOverlay.targetName)
                        {
                            var sz = plotter.frame.Size;
                            var rect = new Rectangle(plotter.left, plotter.top, sz.Width, sz.Height);
                            g.DrawRectangle(GameColors.penYellow4, rect);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.log($"{this.GetType().Name}.OnPaintBackground error: {ex}");
            }
        }

        private static void fadeNext2(PlotBase2 form, float delta)
        {
            form.fade += delta;

            // until we reach 1, re-render every 20ms
            if (form.fade < 1)
                Util.deferAfter(20, () => fadeNext2(form, delta), form.name);
            else if (form.fade > 1)
                form.fade = 1;

            BigOverlay.current?.Invalidate();
        }

        #region PlotPulse

        public static Size plotPulseDefaultSize = new Size(32, 32);
        private static int pulseTick;
        private static System.Windows.Forms.Timer? timer;

        private Image pulseBackground;
        private Point ptPlotPulse;

        private void initPlotPulse()
        {
            // replace the Orange from the bitmap with a themed colour
            var b = new Bitmap(ImageResources.pulse);
            var or = Color.FromArgb(255, 127, 39);

            for (int x = 0; x < b.Width; x++)
                for (int y = 0; y < b.Height; y++)
                    if (b.GetPixel(x, y) == or)
                        b.SetPixel(x, y, C.orangeDark);

            this.pulseBackground = b;

            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer()
                {
                    Interval = 500,
                    Enabled = false,
                };

                timer.Tick += (object? sender, EventArgs e) =>
                {
                    if (pulseTick > 0)
                        pulseTick--;
                    else
                        timer.Stop();

                    BigOverlay.invalidate();
                };
            }
        }

        /// <summary> Resets pulse timer from any journal file update </summary>
        public static void resetPulse()
        {
            // skip if timer is disabled
            if (Game.settings.hideJournalWriteTimer) return;

            // reset counter
            pulseTick = 20;
            Program.control.Invoke(() => timer?.Start());
        }

        private void renderPulse(Graphics g)
        {
            // don't render anything when in maps
            if (Game.activeGame?.isMode(GameMode.GalaxyMap, GameMode.SystemMap) == true)
                return;

            g.DrawImage(pulseBackground, ptPlotPulse);

            // we want a fuzzy outline on this rectangle
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(C.Brushes.orange,
                ptPlotPulse.X + 10, ptPlotPulse.Y + 27 - pulseTick,
                10, pulseTick);

            g.SmoothingMode = SmoothingMode.Default;
        }

        #endregion
    }
}
