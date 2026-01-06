using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using SrvSurvey.widgets;

namespace SrvSurvey.game
{
    static class GameGraphics
    {
        public static Image getBackgroundImage(Control form)
        {
            return getBackgroundImage(form.ClientSize);
        }

        public static Bitmap getBackgroundImage(Size sz, bool noStripes = false)
        {
            var bk = new Bitmap(sz.Width, sz.Height);
            using (var g = Graphics.FromImage(bk))
            {
                g.FillRectangle(Brushes.Black, 0, 0, sz.Width, sz.Height);
                g.FillRectangle(GameColors.brushBackgroundStripe, 0, 0, sz.Width, sz.Height);

                if (!noStripes)
                {
                    // top line
                    g.DrawLine(GameColors.penGameOrangeDim1, 2, 3, sz.Width - 4, 3);
                    g.DrawLine(GameColors.penGameOrange1, 2, 4, sz.Width - 4, 4);
                    g.DrawLine(GameColors.penGameOrangeDim1, 2, 5, sz.Width - 4, 5);
                    //g.DrawLine(GameColors.penOrangeStripe3, this.Width - 80, 4, this.Width - 4, 4);

                    // bottom line
                    //g.FillRectangle(GameColors.brushOrangeStripe, 2, this.Height - 6, this.Width - 4, this.Height - 3);
                    g.DrawLine(GameColors.penGameOrangeDim1, 2, sz.Height - 5, sz.Width - 4, sz.Height - 5);
                    g.DrawLine(GameColors.penGameOrange1, 2, sz.Height - 4, sz.Width - 4, sz.Height - 4);
                    g.DrawLine(GameColors.penGameOrangeDim1, 2, sz.Height - 3, sz.Width - 4, sz.Height - 3);
                }
            }

            return bk;
        }
    }
}
