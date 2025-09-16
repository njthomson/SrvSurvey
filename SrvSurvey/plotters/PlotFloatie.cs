using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotFloatie : PlotBase2
    {
        #region def + statics

        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotFloatie),
            allowed = allowed,
            ctor = (game, def) => new PlotFloatie(game, def),
            defaultSize = new Size(200, 80),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowFloatie_TEST
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && messages.Count > 0
                ;
        }

        #endregion

        /// <summary>
        /// Make this plotter appear showing the given message for ~6 seconds
        /// </summary>
        public static void showMessage(string msg)
        {
            // exit early if this plotter is disabled
            if (!Game.settings.autoShowFloatie_TEST || Game.activeGame == null) return;

            var existing = messages.Find(_ => _.msg == msg);
            if (existing != null)
                messages.Remove(existing);

            // reset close time, add message and show the window
            closeTime = DateTime.Now.AddSeconds(durationVisible);
            messages.Add(new(msg, closeTime));

            if (plotDef.instance == null)
                PlotBase2.add(Game.activeGame, plotDef);

            var form = plotDef.instance as PlotFloatie;
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

        private PlotFloatie(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.gothic_12B;

            this.timer = new System.Windows.Forms.Timer()
            {
                Interval = 25,
                Enabled = false,
            };
            this.timer.Tick += timer_Tick;
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
                PlotBase2.remove(PlotFloatie.plotDef);
            }
            else
            {
                // otherwise force a redraw, so we see the timer bar change
                this.invalidate();
            }
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            // render each message their own line
            foreach (var msg in messages.ToList())
            {
                tt.draw(N.ten, "► " + msg.msg, C.cyan);
                tt.dtx += N.twenty;
                tt.newLine(+N.four, true);
            }

            // and animate the timer bars
            this.drawTimerBar(g);

            return tt.pad(+N.ten, +N.four);
        }

        private void drawTimerBar(Graphics g)
        {
            var remainingTime = (float)(closeTime - DateTime.Now).TotalMilliseconds;
            var r = 1f / 6000 * remainingTime;

            var x = N.two;
            var y = this.height - N.one;
            var w = (this.width - x - x);

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
            g.DrawLine(GameColors.penGameOrangeDim2, x, N.one, x + ww, N.one);
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
