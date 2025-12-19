using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SrvSurvey.plotters
{
    [System.ComponentModel.DesignerCategory("")]
    internal class BigOverlay : Form
    {
        public static BigOverlay? current;

        public static void init(Game game)
        {
            current?.Close();

            // do not create anything if this setting is enabled, but we should still render as a convenience to make separate forms re-render
            if (Game.settings.disableLargeOverlay)
            {
                PlotBase2.renderAll(game, true);
                return;
            }

            var hwnd = Elite.getWindowHandle();

            Game.log($"BigOverlay.create, disableWindowParentIsGame: {Game.settings.disableWindowParentIsGame}, hwnd: {hwnd}");
            var bigOverlay = new BigOverlay();
            if (Game.settings.disableWindowParentIsGame)
            {
                bigOverlay.TopMost = true;
                bigOverlay.Show();
            }
            else
                bigOverlay.Show(new Win32Window() { Handle = hwnd });

            PlotBase2.renderAll(game, true);
        }

        public static void close()
        {
            current?.Close();
            current = null;
        }

        /// <summary> Invalidates the big overlay, using Program.defer </summary>
        public static void invalidate()
        {
            if (BigOverlay.current == null) return;

            Program.defer(() =>
            {
                if (BigOverlay.current?.IsHandleCreated == true && !BigOverlay.current.IsDisposed && !BigOverlay.current.Disposing)
                {
                    var newVisible = !Elite.eliteMinimized && !Program.tempHideAllPlotters && (!Game.settings.disableWindowParentIsGame || Elite.focusElite);
                    if (BigOverlay.current.Visible != newVisible)
                        BigOverlay.current.Visible = newVisible;
                    BigOverlay.current.Invalidate();
                }
            });
        }

        private Color maskColor = Game.settings.bigOverlayMaskColor;

        private BigOverlay()
        {
            BigOverlay.current = this;

            this.Name = this.GetType().Name;
            this.Text = this.Name;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.Size = new Size(640, 640);

            // This needs to be a colour that won't naturally appear
            this.BackColor = maskColor;

            if (Game.settings.disableBetterAlphaBlending)
            {
                this.TransparencyKey = maskColor;
                this.AllowTransparency = true;
            }
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            PlotBase2.closeAll();
            BigOverlay.current = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PlotBase2.closeAll();
            BigOverlay.current = null;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 // WS_EX_TRANSPARENT
                    + 0x00080000 // WS_EX_LAYERED
                    + 0x08000000 // WS_EX_NOACTIVATE
                    + 0x00000004 // WS_EX_NOPARENTNOTIFY
                    ;
                return cp;
            }
        }

        public void reposition(Rectangle gameRect)
        {
            Game.log($"bigOverlay @{gameRect} / {this.Visible}");

            this.SuspendLayout();
            this.Visible = !Elite.eliteMinimized && !Program.tempHideAllPlotters;

            var rect = new Rectangle(gameRect.Left + 1, gameRect.Top + 1, gameRect.Width - 2, gameRect.Height - 2);
            this.Location = rect.Location;
            this.Size = rect.Size;
            this.ResumeLayout();

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
                if (game == null) return;

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

                        g.DrawImage(plotter.frame, rect, 0, 0, sz.Width, sz.Height, GraphicsUnit.Pixel, attr);
                    }

                    if (plotter.name == FormAdjustOverlay.targetName)
                    {
                        var sz = plotter.frame.Size;
                        var rect = new Rectangle(plotter.left, plotter.top, sz.Width, sz.Height);
                        g.DrawRectangle(GameColors.penYellow4, rect);
                    }
                }

                // if streaming setting is active - clobber that hidden window with our own contents
                if (PlotBase.windowOne != null && PlotBase.backOne != null)
                {
                    this.DrawToBitmap(PlotBase.backOne, new Rectangle(Point.Empty, this.Size));
                    PlotBase.windowOne.Invalidate();
                }

                //g.DrawRectangle(Pens.Blue, 0, 0, this.Width - 1, this.Height - 1); // diagnostic
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
