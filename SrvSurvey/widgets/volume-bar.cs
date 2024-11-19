using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey.widgets
{
    internal class VolumeBar : N
    {

        public static void render(Graphics g, float x, float y, VolColor col, long reward, long maxReward = -1, bool prediction = false)
        {
            var initialMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.Default;

            var ww = eight;
            var yy = y;

            var buckets = new List<long>()
            {
                0,
                (long)Game.settings.bioRingBucketOne * 1_000_000,
                (long)Game.settings.bioRingBucketTwo * 1_000_000,
                (long)Game.settings.bioRingBucketThree * 1_000_000,
            };

            // draw outer dotted box
            g.FillRectangle(C.Brushes.black, x, y - oneTwo, ww, oneTwo);
            g.DrawRectangle(GameColors.Bio.volEdge[col], x, y - oneTwo, ww, oneFive);

            if (reward <= 0)
            {
                // (don't use drawTextAt as that messes with dtx/dty)
                g.DrawString("?", GameColors.fontSmallBold, GameColors.Bio.brushPrediction, x - 0.7f, y - oneOne);
                return;
            }

            foreach (var bucket in buckets)
            {
                if (reward > bucket)
                {
                    g.FillRectangle(GameColors.Bio.volMin[col].brush, x, y, ww, three);
                    g.DrawRectangle(GameColors.Bio.volMin[col].pen, x, y, ww, three);
                }
                else if (maxReward > bucket)
                {
                    g.FillRectangle(GameColors.Bio.volMax[col].brush, x, y, ww, three);
                    g.DrawRectangle(GameColors.Bio.volMax[col].pen, x, y, ww, three);
                }
                y -= four;
            }

            // draw a grey hatch effect if we're drawing a prediction
            if (prediction)
            {
                g.FillRectangle(C.Bio.brushHatch, x + one, y + five, ww - one, oneFour);
                //g.FillRectangle(GameColors.Bio.brushPredictionHatch, x, y + four, ww + one, oneSix);
            }

            if (col == VolColor.White)
            {
                g.DrawRectangle(GameColors.Bio.volEdge[col], x, yy - oneTwo, ww, oneFive);
                g.DrawRectangle(GameColors.Bio.volEdge[col], x, yy - oneTwo, ww, oneFive);
            }

            g.SmoothingMode = initialMode;
        }
    }

    public enum VolColor
    {
        Orange,
        Blue,
        Gold,
        White,
    }
}
