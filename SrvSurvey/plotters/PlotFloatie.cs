using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    [ApproxSize(200, 80)]
    internal class PlotFloatie : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            // show in any mode, so long as we have some messages so show
            get => Game.settings.autoShowFloatie_TEST
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && Game.activeGame != null
                && messages.Count > 0
                ;
        }

        /// <summary>
        /// Make this plotter appear showing the given message for ~6 seconds
        /// </summary>
        public static void showMessage(string msg)
        {
            // exit early if this plotter is disabled
            if (!Game.settings.autoShowFloatie_TEST) return;

            var existing = messages.Find(_ => _.msg == msg);
            if (existing != null)
                messages.Remove(existing);

            // reset close time, add message and show the window
            closeTime = DateTime.Now.AddSeconds(durationVisible);
            messages.Add(new(msg, closeTime));

            var form = Program.showPlotter<PlotFloatie>();
            if (form != null)
            {
                form.timer.Start();
                Game.log($"PlotFloatie.showMessage: total messages: {messages.Count}\n\t► " + string.Join("\n\t► ", messages.Select(m => m.msg)));
            }
        }

        private static int durationVisible = 6; // seconds
        private static DateTime closeTime;
        private static List<Message> messages = new();

        private System.Windows.Forms.Timer timer;

        private PlotFloatie() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.gothic_12B;

            this.timer = new System.Windows.Forms.Timer()
            {
                Interval = 25,
                Enabled = false,
            };
            this.timer.Tick += timer_Tick;
        }

        public override bool allow { get => PlotFloatie.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(100);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        private void timer_Tick(object? sender, EventArgs e)
        {
            // remove any expired messages
            messages = messages
                .Where(m => m.expires > DateTime.Now)
                .ToList();

            if (messages.Count == 0)
            {
                // close when nothing left to show
                Game.log($"PlotFloatie: no more messages");
                this.timer.Stop();
                Program.closePlotter<PlotFloatie>();
            }
            else
            {
                // otherwise force a redraw, so we see the timer bar change
                this.Invalidate();
            }
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed) return;

            // render each message their own line
            foreach (var msg in messages.ToList())
            {
                drawTextAt2(ten, "► " + msg.msg, C.cyan);
                dtx += twenty;
                newLine(+four, true);
            }

            // and animate the timer bars
            this.drawTimerBar();

            this.formAdjustSize(+ten, +four);
        }

        private void drawTimerBar()
        {
            var remainingTime = (float)(closeTime - DateTime.Now).TotalMilliseconds;
            var r = 1f / 6000 * remainingTime;

            var x = two;
            var y = this.Height - one;
            var w = (this.Width - x - x);

            //// slide to the right
            //var ww = w * r;
            //g.DrawLine(GameColors.penGameOrangeDim2, w, y, w - ww, y);
            //g.DrawLine(GameColors.penGameOrangeDim2, w, one, w - ww, one);

            //// slide in from the edges
            //var ww = w * r;
            //x = Util.centerIn(w, ww);
            //g.DrawLine(GameColors.penGameOrangeDim2, x, y, x + ww, y);
            //g.DrawLine(GameColors.penGameOrangeDim2, x, one, x + ww, one);

            // slide out from the center
            var ww = w - w * r;
            x = Util.centerIn(w, ww);
            g.DrawLine(GameColors.penGameOrangeDim2, x, y, x + ww, y);
            g.DrawLine(GameColors.penGameOrangeDim2, x, one, x + ww, one);
        }

        class Message
        {
            public string msg;
            public DateTime expires;

            public Message(string msg, DateTime expires)
            {
                this.msg = msg;
                this.expires = expires;
            }
        }
    }
}
