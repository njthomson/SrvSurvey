using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static SrvSurvey.canonn.GRReports;

namespace SrvSurvey.plotters
{
    [System.ComponentModel.DesignerCategory("")]
    internal class BigOverlay : Form
    {
        public static BigOverlay current;

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

            this.reposition(Elite.getWindowRect());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Overlays.closeAll();
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

                if (Game.activeGame != null)
                    Overlays.renderAll(Game.activeGame);

                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;
            maskColor = Color.FromArgb(1, 1, 1);
            try
            {
                var g = e.Graphics;
                //g.Clear(maskColor);
                //g.DrawRectangle(Pens.DarkRed, 0, 0, this.Width - 1, this.Height - 1);
                //g.DrawLine(Pens.DarkRed, 0, 0, this.Width - 1, this.Height - 1);
                //g.DrawLine(Pens.DarkRed, this.Width, 0, 0, this.Height - 1);

                //if (this.Opacity != Game.settings.Opacity)
                //    this.Opacity = Game.settings.Opacity;

                var game = Game.activeGame;
                if (game != null)
                {
                    foreach (var plotter in Overlays.active)
                    {
                        // re-render only if needed
                        if (plotter.stale)
                            plotter.render(game);

                        //Game.log($"BigOne draw: {plotter.name}");
                        if (plotter.fade == 1)
                        {

                            g.DrawImageUnscaled(plotter.background, plotter.left, plotter.top);
                            g.DrawImageUnscaled(plotter.frame, plotter.left, plotter.top);
                        }
                        else if (plotter.fade == 0)
                        {
                            //var rect = new Rectangle(plotter.left, plotter.top, sz.Width, 20);
                            //g.FillRectangle(new SolidBrush(Color.FromArgb(140, 0, 0, 0)), rect);

                            Util.deferAfter(20, () => fadeNext2((PlotBase2)plotter, 0.1f), plotter.name);
                        }
                        else
                        {
                            var colorMatrix = new ColorMatrix()
                            {
                                Matrix33 = plotter.fade,
                            };

                            // TODO: remove next time
                            //var ff1 = 1f;
                            //var ff0 = plotter.fade; //1f;
                            //var ff = 0f;
                            //float[][] colorMatrixElements = {
                            //    new float[] {ff1,  0,  0,  0, 0},        // red scaling factor of 1
                            //    new float[] {0,  ff1,  0,  0, 0},        // green scaling factor of 1
                            //    new float[] {0,  0,  ff1,  0, 0},        // blue scaling factor of 1
                            //    new float[] {0,  0,  0,  ff0, 0},        // alpha scaling factor of 1
                            //    new float[] {ff, ff, ff, ff, 1}};        // three translations of 0.2
                            //ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

                            var attr = new ImageAttributes();
                            attr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                            var sz = plotter.frame.Size;
                            var rect = new Rectangle(plotter.left, plotter.top, sz.Width, sz.Height);

                            // we need a black box or the blending looks too bright
                            g.FillRectangle(Brushes.Black, rect);

                            g.DrawImage(plotter.background, rect, 0, 0, sz.Width, sz.Height, GraphicsUnit.Pixel, attr);
                            g.DrawImage(plotter.frame, rect, 0, 0, sz.Width, sz.Height, GraphicsUnit.Pixel, attr);
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

            if (form.fade < 1)
                Util.deferAfter(20, () => fadeNext2(form, delta), form.name);
            else
                if (form.fade > 1) form.fade = 1;

            BigOverlay.current.Invalidate();
        }

        private static void fadeNext(PlotBase2 form, float targetOpacity, long lastTick, float delta)
        {
            //Application.DoEvents();

            //if (!Elite.gameHasFocus || form.forceHide)
            //{
            //    // stop early and hide form if the game loses focus, or the form is being forced hidden
            //    form.setOpacity(0);
            //    return;
            //}
            //form.fading = true;

            var wasFade = form.fade;
            var wasSmaller = form.fade < targetOpacity;

            // (there are 10,000 ticks in a millisecond)
            var nextTick = lastTick + 10_000;

            if (nextTick < DateTime.Now.Ticks)
            {
                var newFade = form.fade + (wasSmaller ? delta : -delta);
                var isSmaller = newFade < targetOpacity;

                Debug.WriteLine($"! delta:{delta}, junk.Opacity:{wasFade}, this.targetOpacity:{targetOpacity}, wasSmaller:{wasSmaller}, isSmaller:{isSmaller}");

                // end animation when we reach target
                if (wasSmaller != isSmaller || form.fade == targetOpacity)
                {
                    form.fade = 1;
                    return;
                }

                form.fade = newFade;
                lastTick = DateTime.Now.Ticks;
                // TODO: animate the location too, just a little?
            }
            else
            {
                Debug.WriteLine($"~ delta:{delta}, junk.Opacity:{wasFade}, this.targetOpacity:{targetOpacity}, skip! {lastTick} // {nextTick} < {DateTime.Now.Ticks}");
            }

            Program.defer(() => fadeNext(form, targetOpacity, lastTick, delta));
        }



    }
}
