﻿using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
    [System.ComponentModel.DesignerCategory("")]
    [ApproxSize(32, 32)]
    public partial class PlotPulse : Form, PlotterForm
    {
        public static DateTime LastChanged;

        public static void show()
        {
            new PlotPulse().Show();
        }

        private int count = 20;
        private DateTime lastChanged;
        public bool didFirstPaint { get; set; } = true;
        public bool showing { get; set; }
        public bool forceHide { get; set; }
        public bool fading { get; set; }
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
            this.Opacity = 0; // ok
            this.DoubleBuffered = true;
            this.Size = Size.Empty;

            // replace the Orange from the bitmap with a themed colour
            var bb = new Bitmap(this.BackgroundImage!);
            var or = Color.FromArgb(255, 127, 39);
            for (int x = 0; x < bb.Width; x++)
                for (int y = 0; y < bb.Height; y++)
                    if (bb.GetPixel(x, y) == or)
                        bb.SetPixel(x, y, C.orangeDark);
            this.BackgroundImage = bb;

            // Does this cause windows to become visible when alt-tabbing?
            this.Text = this.Name;
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
            this.setOpacity(PlotPos.getOpacity(this));
            if (gameRect != Rectangle.Empty)
                PlotPos.reposition(this, gameRect);
        }

        /// <summary> Set opacity to the given value. </summary>
        public void setOpacity(double newOpacity)
        {
            if (this.Opacity != newOpacity)
                this.Opacity = newOpacity; // ok
        }

        /// <summary> Reset opacity to default it's value </summary>
        public void resetOpacity()
        {
            setOpacity(PlotPos.getOpacity(this));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (PlotPulse.LastChanged > this.lastChanged)
            {
                this.lastChanged = PlotPulse.LastChanged;
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

            if (Game.settings.hideOverlaysFromMouseInFSS_TEST)
            {
                PlotBase.HideAndReturnWhenMouseMoves(this);
            }
            else if (Game.settings.hideOverlaysFromMouse)
            {
                // move the mouse outside the overlay
                Game.log($"OnMouseEnter: {this.Name}. Mouse was:{Cursor.Position}, moved to: {Elite.gameCenter}");
                Cursor.Position = Elite.gameCenter;
            }
            else
            {
                Game.log($"OnMouseEnter: {this.Name}. Mouse is:{Cursor.Position}, would have moved to: {Elite.gameCenter}");
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
            Game.log($"OnActivated: {this.Name}. Mouse is:{Cursor.Position}");

            // plotters are not suppose to receive focus - force it back onto the game if we do
            base.OnActivated(e);

            if (!this.showing || Elite.focusElite)
                Elite.setFocusED();
        }

        private void PlotPulse_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (Game.activeGame?.isMode(GameMode.GalaxyMap, GameMode.SystemMap) == true)
                this.setOpacity(0f);
            else
                this.resetOpacity();

            g.FillRectangle(C.Brushes.orange,
                10, 27 - count,
                10, count);

            if (FormAdjustOverlay.targetName == this.Name)
                PlotBase.ifAdjustmentTarget(g, this);
        }
    }
}
