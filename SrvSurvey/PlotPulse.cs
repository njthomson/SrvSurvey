using SrvSurvey.game;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    public partial class PlotPulse : Form, PlotterForm
    {
        public static DateTime LastChanged;

        public static void show()
        {
            new PlotPulse().Show();
        }

        private int count = 20;
        private DateTime lastchanged;
        public bool didFirstPaint { get; set; } = true;
        public bool showing { get; set; }

        private PlotPulse()
        {
            InitializeComponent();
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
            this.Name = this.GetType().Name;
            this.BackColor = Color.Black;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0;
            this.DoubleBuffered = true;
            this.Size = Size.Empty;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Width = 32;
            this.Height = 32;

            // position ourselves in the bottom left corner of the ED window
            this.reposition(Elite.getWindowRect(true));
        }

        public void reposition(Rectangle gameRect)
        {
            this.Opacity = PlotPos.getOpacity(this);
            if (gameRect != Rectangle.Empty)
                PlotPos.reposition(this, gameRect);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (PlotPulse.LastChanged > this.lastchanged)
            {
                this.lastchanged = PlotPulse.LastChanged;
                this.count = 20;
            }
            else if (count > 0)
            {
                count--;
            }

            this.Invalidate();

        }

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Debugger.IsAttached)
            {
                // use a different cursor if debugging
                this.Cursor = Cursors.No;
            }
            else if (Game.settings.hideOverlaysFromMouse)
            {
                // move the mouse outside the overlay
                System.Windows.Forms.Cursor.Position = Elite.gameCenter;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        #endregion

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!this.showing || Elite.focusElite)
            {
                Elite.setFocusED();
            }
        }

        private void PlotPulse_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(GameColors.brushGameOrange,
                10, 27 - count,
                10, count);
        }
    }
}
