using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private PlotPulse()
        {
            InitializeComponent();
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
        }

        private void PlotPulse_Load(object sender, EventArgs e)
        {
            this.Width = 32;
            this.Height = 32;

            // position ourselves in the bottom left corner of the ED window
            this.reposition(Elite.getWindowRect(true));
        }

        public void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            this.Left = gameRect.Left + 12;
            this.Top = gameRect.Bottom - this.Height - 8;
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Debugger.IsAttached)
                Elite.setFocusED();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Debugger.IsAttached)
                // use a different cursor if debugging
                this.Cursor = Cursors.No;
            else
                // otherwise hide the cursor entirely
                Cursor.Hide();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // restore the cursor when it leaves
            Cursor.Show();
        }

        #endregion

        private void PlotPulse_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            g.FillRectangle(GameColors.brushGameOrange,
                10, 27 - count,
                10, count);
        }
    }
}
