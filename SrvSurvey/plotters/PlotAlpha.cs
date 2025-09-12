using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotAlpha : PlotBase2
    {
        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotAlpha),
            allowed = allowed,
            ctor = (game, def) => new PlotAlpha(game, def),
            defaultSize = new Size(240, 140),
            factors = new() { "mode" },
            renderFactors = new() { nameof(Status.GuiFocus) }, // "mode"
        };

        public static int foo = 10;

        public static bool allowed(Game game)
        {
            return false && Game.settings.autoShowPlotStationInfo_TEST
                && game.mode == GameMode.InternalPanel;
        }

        public PlotAlpha(Game game, PlotDef def) : base(game, def)
        {
            this.left = 20;
            this.top = 20;
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
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
        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotBravo),
            allowed = allowed,
            ctor = (game, def) => new PlotBravo(game, def),
            defaultSize = new Size(240, 140),
            factors = new() { "mode", StatusFlags.LightsOn.ToString() },
            renderFactors = new() { StatusFlags.LightsOn.ToString() }
        };

        public static int foo = 10;

        public static bool allowed(Game game)
        {
            return false && Game.settings.autoShowPlotStationInfo_TEST
                //&& game.mode == GameMode.ExternalPanel;
                && game.status.lightsOn;
        }

        public PlotBravo(Game game, PlotDef def) : base(game, def)
        {
            this.left = 420;
            this.top = 20;
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
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
