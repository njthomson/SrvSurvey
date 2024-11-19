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
        public static Image getBackgroundForForm(Form form)
        {
            return getBackgroundForForm(form.Bounds);
        }

        public static Image getBackgroundForForm(Rectangle rect)
        {
            var bk = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(bk))
            {
                g.FillRectangle(GameColors.brushBackgroundStripe, 0, 0, rect.Width, rect.Height);

                // top line
                g.DrawLine(GameColors.penGameOrangeDim1, 2, 3, rect.Width - 4, 3);
                g.DrawLine(GameColors.penGameOrange1, 2, 4, rect.Width - 4, 4);
                g.DrawLine(GameColors.penGameOrangeDim1, 2, 5, rect.Width - 4, 5);
                //g.DrawLine(GameColors.penOrangeStripe3, this.Width - 80, 4, this.Width - 4, 4);

                // bottom line
                //g.FillRectangle(GameColors.brushOrangeStripe, 2, this.Height - 6, this.Width - 4, this.Height - 3);
                g.DrawLine(GameColors.penGameOrangeDim1, 2, rect.Height - 5, rect.Width - 4, rect.Height - 5);
                g.DrawLine(GameColors.penGameOrange1, 2, rect.Height - 4, rect.Width - 4, rect.Height - 4);
                g.DrawLine(GameColors.penGameOrangeDim1, 2, rect.Height - 3, rect.Width - 4, rect.Height - 3);
            }

            return bk;
        }
    }
}
