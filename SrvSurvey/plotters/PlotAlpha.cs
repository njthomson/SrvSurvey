using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotAlpha : PlotBase2
    {
        public static int foo = 10;

        public static void register()
        {
            Overlays.register(new Overlays.Factors
            {
                name = nameof(PlotAlpha),
                factors = new() { "mode" },
                showMe = showMe,
                ctor = () => new PlotAlpha(),
                renderFactors = new() { nameof(Status.GuiFocus) }, // "mode"
            });
        }

        public static bool showMe(Game game)
        {
            var valid = Game.settings.autoShowPlotStationInfo_TEST
                && game.mode == GameMode.ExternalPanel;

            //Game.log($"PlotAlpha.showMe: {valid}");
            return valid;
        }

        public PlotAlpha() : base(240, 140)
        {
            this.left = 20;
            this.top = 20;
        }

        protected override Size doRender(Game game, Graphics g)
        {
            //g.DrawRectangle(Pens.Red, 0, 0, this.width - 1, this.height - 1);
            //g.DrawLine(Pens.Red, 0, 0, this.width - 1, this.height - 1);
            //g.DrawLine(Pens.Red, this.width, 0, 0, this.height - 1);

            g.DrawString(DateTime.Now.ToString(), GameColors.Fonts.console_16, C.Brushes.orange, 10, 20);

            var w = 10 + foo;
            g.FillRectangle(C.Brushes.orangeDark, 10, 50, w, 50);

            //return new Size(w + 50, 140);
            return new Size(this.width, this.height);
        }
    }

    internal class PlotBravo: PlotBase2
    {
        public static int foo = 10;

        public static void register()
        {
            Overlays.register(new Overlays.Factors
            {
                name = nameof(PlotBravo),
                factors = new() { "mode", StatusFlags.LightsOn.ToString() },
                showMe = showMe,
                ctor = () => new PlotBravo(),
                renderFactors = new() { nameof(Status.GuiFocus), StatusFlags.LightsOn.ToString() },
            });
        }

        public static bool showMe(Game game)
        {
            var valid = Game.settings.autoShowPlotStationInfo_TEST
                //&& game.mode == GameMode.ExternalPanel;
                && game.status.lightsOn;

            return valid;
        }

        public PlotBravo() : base(240, 140)
        {
            this.left = 420;
            this.top = 20;
        }

        protected override Size doRender(Game game, Graphics g)
        {
            //g.DrawRectangle(Pens.Red, 0, 0, this.width - 1, this.height - 1);
            //g.DrawLine(Pens.Red, 0, 0, this.width - 1, this.height - 1);
            //g.DrawLine(Pens.Red, this.width, 0, 0, this.height - 1);

            g.DrawString(DateTime.Now.ToString(), GameColors.Fonts.console_16, C.Brushes.orange, 10, 20);

            var w = 10 + foo;
            g.FillRectangle(C.Brushes.orangeDark, 10, 50, w, 50);

            //return new Size(w + 50, 140);
            return new Size(this.width, this.height);
        }
    }


}
