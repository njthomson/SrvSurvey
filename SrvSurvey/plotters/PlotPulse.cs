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
            return !Game.settings.hideJournalWriteTimer
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && !game.isMode(GameMode.GalaxyMap, GameMode.SystemMap);
        }

        private static int pulseTick;
        private static System.Windows.Forms.Timer? timer;
        private static DateTime? superCruiseOverDriveStopped;

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
                    else if (superCruiseOverDriveStopped == null)
                        timer.Stop();

                    if (superCruiseOverDriveStopped < DateTime.UtcNow.AddSeconds(-10))
                        superCruiseOverDriveStopped = null;

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

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);

            if (status.changed.Contains(nameof(StatusFlags2.SCOverdrive)) && status.SCOverdrive == false)
                superCruiseOverDriveStopped = status.timestamp;
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            // don't render anything when in maps
            if (this.game.isMode(GameMode.GalaxyMap, GameMode.SystemMap) == true)
                return plotPulseDefaultSize;

            g.SmoothingMode = SmoothingMode.None;
            g.DrawImage(pulseBackground, Point.Empty);

            // we want a fuzzy outline on this rectangle
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (pulseTick >= 0)
            {
                g.FillRectangle(C.Brushes.orange,
                    10, 27 - pulseTick,
                    10, pulseTick);
            }

            // show a red dot when SuperCruise Overdrive is active, that slowly falls, turning cyan once it can be used again
            if (game.status.SCOverdrive)
            {
                g.FillEllipse(C.Brushes.red, width - 10f, height - 32f, 8, 8);
            }
            else if (superCruiseOverDriveStopped != null)
            {
                var diff = (DateTime.UtcNow - superCruiseOverDriveStopped.Value).TotalSeconds;
                var b = diff > 9 ? C.Brushes.cyan : C.Brushes.menuGold;
                g.FillEllipse(b, width - 10f, height - 31f - (float)diff * -2, 8, 8);
            }

            return plotPulseDefaultSize;
        }
    }
}
