using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SrvSurvey.plotters
{
    [System.ComponentModel.DesignerCategory("")]
    internal class BigOverlay : Form
    {
        public static BigOverlay? current;

        public static void create(Game game)
        {
            current?.Close();

            // do not create anything if this setting is enabled
            if (Game.settings.disableBigOverlay)
            {
                PlotBase2.renderAll(game, true);
                return;
            }

            Game.log("BigOverlay.create");
            var bigOverlay = new BigOverlay();
            bigOverlay.Show(new Win32Window() { Handle = Elite.getWindowHandle() });

            PlotBase2.renderAll(game, true);
        }

        /// <summary> Invalidates the big overlay, using Program.defer </summary>
        public static void invalidate()
        {
            Program.defer(() =>
            {
                if (BigOverlay.current != null)
                {
                    BigOverlay.current.Visible = !Elite.eliteMinimized && !Program.tempHideAllPlotters;
                    BigOverlay.current.Invalidate();
                }
            });
        }

        private Color maskColor = Game.settings.bigOverlayMaskColor;

        private BigOverlay()
        {
            BigOverlay.current = this;

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

            if (Game.settings.disableBetterAlphaBlending)
            {
                this.TransparencyKey = maskColor;
                this.AllowTransparency = true;
            }

            this.Text = "SrvSurveyOne";

            this.initPlotPulse();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            System.Windows.Forms.Cursor.Show();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int exStyle = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(this.Handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_LAYERED);

            setOpacity(Game.settings.plotterOpacity);
            reposition(Elite.getWindowRect());
            System.Windows.Forms.Cursor.Show();
        }

        /// <summary> Set opacity from 0 to 100 % </summary>
        public void setOpacity(float opacity)
        {
            if (Game.settings.disableBetterAlphaBlending)
            {
                this.Opacity = opacity * 0.01;
            }
            else
            {
                // Set transparency: 128 = 50% opacity
                var uui = (uint)ColorTranslator.ToWin32(maskColor);
                var opacity2 = (byte)Math.Round(255f / 100f * opacity);
                NativeMethods.SetLayeredWindowAttributes(this.Handle, uui, opacity2, NativeMethods.LWA_ALPHA | NativeMethods.LWA_COLORKEY);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PlotBase2.closeAll();
            BigOverlay.current = null!;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 + 0x00080000 + 0x08000000; // WS_EX_TRANSPARENT + WS_EX_LAYERED + WS_EX_NOACTIVATE
                return cp;
            }
        }

        public void reposition(Rectangle gameRect)
        {
            Game.log($"bigOverlay @{gameRect} / {this.Visible}");

            this.SuspendLayout();
            this.Visible = !Elite.eliteMinimized && !Program.tempHideAllPlotters;
            this.Location = gameRect.Location;
            this.Size = gameRect.Size;
            this.ResumeLayout();

            // get PlotPulse location
            this.ptPlotPulse = PlotPos.getPlotterLocation("PlotPulse", plotPulseDefaultSize, gameRect, true);

            if (Game.activeGame != null)
            {
                foreach (var plotter in PlotBase2.active)
                    plotter.setPosition(gameRect);

                PlotBase2.renderAll(Game.activeGame);
            }

            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;

            try
            {
                var g = e.Graphics;
                g.Clear(maskColor);

                var game = Game.activeGame;
                if (game != null)
                {
                    if (!Game.settings.hideJournalWriteTimer)
                        this.renderPulse(g);

                    g.SmoothingMode = SmoothingMode.None;
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

                        if (plotter.fade == 1 || PlotBase.windowOne != null)
                        {
                            // not fading
                            g.DrawImageUnscaled(plotter.background, plotter.left, plotter.top);
                            g.DrawImageUnscaled(plotter.frame, plotter.left, plotter.top);
                        }
                        else if (plotter.fade == 0)
                        {
                            // start fading in
                            Util.deferAfter(20, () => fadeNext2((PlotBase2)plotter, 0.1f), plotter.name);
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

                // if streaming setting is active - clobber that hidden window with our own contents
                if (PlotBase.windowOne != null && PlotBase.backOne != null)
                {
                    this.DrawToBitmap(PlotBase.backOne, new Rectangle(Point.Empty, this.Size));
                    PlotBase.windowOne.Invalidate();
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

            g.SmoothingMode = SmoothingMode.None;
            g.DrawImage(pulseBackground, ptPlotPulse);

            // we want a fuzzy outline on this rectangle
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillRectangle(C.Brushes.orange,
                ptPlotPulse.X + 10, ptPlotPulse.Y + 27 - pulseTick,
                10, pulseTick);
        }

        #endregion

        class NativeMethods
        {
            public const int WS_EX_LAYERED = 0x80000;
            public const int GWL_EXSTYLE = -20;
            public const int LWA_COLORKEY = 0x1;
            public const int LWA_ALPHA = 0x2;

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        }
    }
}
