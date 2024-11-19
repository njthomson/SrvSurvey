using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey.widgets
{
    /// <summary>
    /// Renders temperature range differences on a given Graphics object
    /// </summary>
    internal class TempRangeDiffs : N
    {
        #region stationary

        // TODO: make static
        private Pen penOrange1 = C.Pens.orange1;
        private Pen penDefault = C.Pens.orange2;
        private Pen penCmdr = C.yellow.toPen(4, LineCap.Triangle);
        private Pen penMax = C.red.toPen(2, LineCap.Triangle);
        private Pen penMin = C.cyan.toPen(2, LineCap.Triangle);

        #endregion

        /// <summary> The height of the bar </summary>
        public float height = forty;
        public float left = twenty;
        public float top = twenty;

        private float bottom { get => top + height; }

        public TempRangeDiffs(float left, float top)
        {
            this.left = left;
            this.top = top;
        }

        private Game? game { get => Game.activeGame; }

        private void drawBackgroundBar(Graphics g)
        {
            // the vertical background line, +/- 2px around the height
            g.DrawLine(penOrange1, left, top - two, left, bottom + two);
        }

        /// <summary>
        /// Render cmdr's temp relative to the body's "default" surface temperature (there is no scaling)
        /// </summary>
        /// <param name="g"></param>
        public void renderBodyOnly(Graphics g)
        {
            if (game?.systemBody == null || game.status.Temperature == 0) return;

            drawBackgroundBar(g);

            // We're assuming the difference in cmdr's temperature naturally fits within +/- 20 ... otherwise we need to start scaling?
            var cmdrTemp = game.status.Temperature;

            // relative line for current live temp
            var yCmdr = top + height - (float)(cmdrTemp - game.systemBody.surfaceTemperature);
            if (yCmdr < top) yCmdr = top;
            if (yCmdr > bottom) yCmdr = bottom;

            g.DrawLineR(penCmdr, left - two, yCmdr, +ten, 0);

            // "default" surface temperature is at the middle
            var yD = top + (height / 2f);
            g.DrawLineR(penDefault, left - five, yD, +ten, 0);
        }

        /// <summary>
        /// Render body "default" surface temperature, and optionally the cmdr's live temp, relative to a given min/max, scaled to fix within height.
        /// </summary>
        public void renderWithinRange(Graphics g, float min, float max)
        {
            if (game?.systemBody == null || game.status.Temperature == 0) return;

            var bodyDefaultTemp = (float)game.systemBody.surfaceTemperature;

            drawBackgroundBar(g);

            // red and blue bars are fixed at either end of the line
            g.DrawLineR(penMax, left - eight, top, oneTwo, 0);
            g.DrawLineR(penMin, left - eight, bottom, +ten, 0);

            // draw other lines between those relative to the temperature range
            var tempRange = max - min;
            var dTemp = (height / tempRange);

            // "default" surface temperature
            var dD = bodyDefaultTemp - min;
            var yD = top + height - (dD * dTemp);
            g.DrawLineR(penDefault, left - five, yD, +ten, 0);

            // current cmdr's temp (if outside on foot)
            if (game.status.Temperature > 0)
            {
                var dCmdr = bodyDefaultTemp - min;
                var yCmdr = top + height - (dCmdr * dTemp);
                g.DrawLineR(penCmdr, left - two, yCmdr, +ten, 0);
            }
        }
    }
}
