using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;

namespace SrvSurvey.plotters
{
    internal class PlotPulse : PlotBase2
    {
        #region def + statics

        public static Size plotPulseDefaultSize = new Size(32, 32);

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotPulse),
            allowed = allowed,
            ctor = (game, def) => new PlotPulse(game, def),
            defaultSize = plotPulseDefaultSize,
        };

        public static bool allowed(Game game)
        {
            return !Game.settings.hideJournalWriteTimer;
        }

        private static int pulseTick;
        private static System.Windows.Forms.Timer? timer;

        private static Image pulseBackground;

        #endregion

        private PlotPulse(Game game, PlotDef def) : base(game, def)
        {
            initPlotPulse();
        }

        private void initPlotPulse()
        {
            if (pulseBackground == null)
            {
                // replace the Orange from the bitmap with a themed colour
                var b = new Bitmap(ImageResources.pulse);
                var or = Color.FromArgb(255, 127, 39);

                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                        if (b.GetPixel(x, y) == or)
                            b.SetPixel(x, y, C.orangeDark);

                pulseBackground = b;
            }

            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer()
                {
                    Interval = 500,
                    Enabled = false,
                };

                timer.Tick += (object? sender, EventArgs e) =>
                {
                    if (pulseTick > 0)
                        pulseTick--;
                    else
                        timer.Stop();

                    PlotBase2.invalidate(nameof(PlotPulse));
                };
            }
        }

        public static void resetPulse()
        {
            // skip if timer is disabled
            if (Game.settings.hideJournalWriteTimer) return;

            // reset counter
            pulseTick = 20;
            if (timer?.Enabled == false)
                Program.control.Invoke(() => timer?.Start());
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            // don't render anything when in maps
            if (Game.activeGame?.isMode(GameMode.GalaxyMap, GameMode.SystemMap) == true)
                return plotPulseDefaultSize;

            g.SmoothingMode = SmoothingMode.None;
            g.DrawImage(pulseBackground, Point.Empty);

            // we want a fuzzy outline on this rectangle
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(C.Brushes.orange,
                10, 27 - pulseTick,
                10, pulseTick);

            return plotPulseDefaultSize;
        }
    }
}
