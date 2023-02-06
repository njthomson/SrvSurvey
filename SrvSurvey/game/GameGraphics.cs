using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SrvSurvey.game
{
    static class GameGraphics
    {
        public static Image getBackgroundForForm(Form form)
        {
            var bk = new Bitmap(form.Width, form.Height);
            using (var g = Graphics.FromImage(bk))
            {
                g.FillRectangle(GameColors.brushBackgroundStripe, 0, 0, form.Width, form.Height);

                // top line
                g.DrawLine(GameColors.penGameOrangeDim1, 2, 3, form.Width - 4, 3);
                g.DrawLine(GameColors.penGameOrange1, 2, 4, form.Width - 4, 4);
                g.DrawLine(GameColors.penGameOrangeDim1, 2, 5, form.Width - 4, 5);
                //g.DrawLine(GameColors.penOrangeStripe3, this.Width - 80, 4, this.Width - 4, 4);

                // bottom line
                //g.FillRectangle(GameColors.brushOrangeStripe, 2, this.Height - 6, this.Width - 4, this.Height - 3);
                g.DrawLine(GameColors.penGameOrangeDim1, 2, form.Height - 5, form.Width - 4, form.Height - 5);
                g.DrawLine(GameColors.penGameOrange1, 2, form.Height - 4, form.Width - 4, form.Height - 4);
                g.DrawLine(GameColors.penGameOrangeDim1, 2, form.Height - 3, form.Width - 4, form.Height - 3);
            }

            return bk;
        }
    }
}
