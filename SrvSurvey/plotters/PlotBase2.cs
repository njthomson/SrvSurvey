using SrvSurvey.game;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SrvSurvey.plotters
{
    internal interface IPlotBase2
    {
        string name { get; }
        int left { get; }
        int top { get; }
        int width { get; }
        int height { get; }
        float fade { get; set; }
        bool stale { get; }
        Image frame { get; }
        Image background { get; }

        Image render(Game game);
    }

    internal abstract class PlotBase2 : IPlotBase2
    {
        public string name { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public float fade { get; set; }

        public bool stale { get; set; }
        public Image frame { get; set; }
        public Image background { get; set; }

        public PlotBase2(int w, int h)
        {
            this.name = GetType().Name;

            this.stale = true;
            this.setSize(w, h);
            this.frame = new Bitmap(w, h);
        }

        public void setSize(int width, int height)
        {
            var changed = this.width != width || this.height != height;
            if (changed)
            {
                this.width = width;
                this.height = height;
                this.invalidate();
            }
        }

        public virtual void close()
        {
            // override as necessary
        }

        public void invalidate()
        {
            //Game.log($"invalidate: {this.name}");
            this.stale = true;
        }

        protected abstract Size doRender(Game game, Graphics g);

        public Image render(Game game)
        {
            //Game.log($"Render: {this.name}, stale: {this.stale}");
            var renderCount = 0;
            Bitmap nextFrame = null!;
            do
            {
                renderCount++;
                nextFrame = new Bitmap(this.width, this.height);
                using (var g = Graphics.FromImage(nextFrame))
                {
                    var sz = this.doRender(game, g);
                    this.stale = false;
                    this.setSize(sz.Width, sz.Height);
                }
                if (renderCount > 6) Debugger.Break();
            } while (stale || renderCount > 10);

            //Game.log($"Render: {this.name}, renderCount: {renderCount}");
            this.frame = nextFrame;

            // re-render background only when size has changed
            if (this.background == null || this.background.Size != this.frame.Size)
                this.background = GameGraphics.getBackgroundImage(frame.Size);

            return this.frame;
        }
    }

    internal static class Overlays
    {
        static Overlays()
        {
            // start by collecting all non-abstract classes derived from PlotBase2
            var allPlotterTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(_ => typeof(PlotBase2).IsAssignableFrom(_) && !_.IsAbstract)
                .ToList();

            Game.log(allPlotterTypes.Count);
            foreach (var foo in allPlotterTypes)
            {
                Game.log(foo.Name);
                var func = foo.GetMethod("register", BindingFlags.Static | BindingFlags.Public);
                if (func != null)
                    func.Invoke(null, null);
                else
                    Debugger.Break();
            }
            //Overlays.register()

        }

        private static Dictionary<string, Factors> overlays = new Dictionary<string, Factors>();

        public static void register(Factors def)
        {
            overlays[def.name] = def;
        }

        internal class Factors
        {
            public string name;
            /// <summary>
            /// Factors that govern if this plotter should be shown or hidden
            /// </summary>
            public HashSet<string> factors;
            public Func<Game, bool> showMe;
            public PlotBase2? instance;
            public Func<PlotBase2> ctor;
            public HashSet<string>? renderFactors;
            public PlotContainer? form;
        }

        /*public static PlotBase2? add(string name)
        {
            var def = overlays.GetValueOrDefault(name);
            if (def != null)
                return add(def);

            Game.log($"Why no def?");
            Debugger.Break();
            return null;
        }*/

        public static PlotBase2? add(Factors def)
        {
            if (def.instance != null)
            {
                Game.log($"Overlays.add: {def.name} - why already present?");
                Debugger.Break();
                remove(def);
            }

            Game.log($"Overlays.add: {def.name}");
            def.instance = def.ctor();

            return def.instance;
        }

        public static Factors? get(string name)
        {
            var def = overlays.GetValueOrDefault(name);
            return def;
        }

        /*public static void remove(string name)
        {
            var def = overlays.GetValueOrDefault(name);
            if (def != null)
            {
                remove(def);
                return;
            }

            Game.log($"Why no def?");
            Debugger.Break();
        }*/

        public static void remove(Factors def)
        {
            if (def.instance != null)
            {
                Game.log($"Overlays.remove: {def.name}");

                if (Game.settings.useNotOneOverlay_TEST)
                {
                    def.form = new PlotContainer(def);
                    Program.closePlotter(def.name);
                }

                def.instance.close();
                def.instance = null;
            }
        }

        public static void invalidate(string name)
        {
            var def = overlays.GetValueOrDefault(name);
            def?.instance?.invalidate();

            BigOverlay.current?.Invalidate();
        }

        public static void renderAll(Game game)
        {
            if (game.isShutdown || game.status == null) return;

            // check if we need to create or remove any
            foreach (var def in overlays.Values)
            {
                var relevant = def.factors.Any(x => game.status.changed.Contains(x));

                var shouldShow = def.showMe(game);
                //Game.log($"relevant? {def.name} => {relevant} / {def.instance != null} / shouldShow: {shouldShow}");

                // only attempt to create or destroy if something relevant changed
                if (relevant)
                {
                    // create or destroy as needed
                    if (shouldShow && def.instance == null)
                        add(def);
                    else if (!shouldShow && def.instance != null)
                        remove(def);
                }

                // render every time, unless we have factors to consider
                var shouldRender = def.renderFactors == null || def.renderFactors.Any(x => game.status.changed.Contains(x));
                if (shouldRender)
                    def.instance?.render(game);

                if (Game.settings.useNotOneOverlay_TEST && def.instance != null)
                {
                    if (def.form == null)
                    {
                        def.form = new PlotContainer(def);
                        Program.showPlotter<PlotContainer>(null, def.name);
                    }

                    def.form?.doRender();
                }
            }

            if (!Game.settings.useNotOneOverlay_TEST)
            {
                BigOverlay.current?.Invalidate();
            }
        }

        public static void closeAll()
        {
            Game.log($"Overlays.closeAll");

            foreach (var def in overlays.Values)
                remove(def);

            BigOverlay.current?.Invalidate();
        }

        /// <summary>
        /// The list of active plotters to be rendered
        /// </summary>
        public static List<IPlotBase2> active
        {
            get => overlays.Values
                .Where(def => def.instance != null)
                .Select(def => def.instance!)
                .ToList<IPlotBase2>();
        }

    }

    internal class PlotContainer : PlotBase
    {
        private readonly Overlays.Factors def;

        public PlotContainer(Overlays.Factors def)
        {
            this.def = def;
            this.Name = def.name;

            this.Left = def.instance!.left;
            this.Top = def.instance.top;
        }

        protected override void Dispose(bool disposing)
        {
            this.def.form = null;

            base.Dispose(disposing);
        }

        public override bool allow => Game.activeGame == null ? false : def.showMe(Game.activeGame);

        public void doRender()
        {
            if (def.instance == null) return;

            this.Invalidate();

            if (this.Width != def.instance.width || this.Height != def.instance.height)
                this.SetBounds(this.Left, this.Top, def.instance.width, def.instance.height);
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || def.instance == null || Game.activeGame == null) return;

            if (def.instance.frame == null)
                def.instance.render(Game.activeGame);

            g.DrawImage(def.instance.background, 0, 0);
            g.DrawImage(def.instance.frame!, 0, 0);
        }

    }
}
