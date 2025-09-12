using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Reflection;

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
        bool hidden { get; set; }

        Image frame { get; }
        Image background { get; }

        Image render();
    }

    internal class PlotDef
    {
        public string name;
        public Func<Game, PlotDef, PlotBase2> ctor;
        /// <summary> Returns true if the plotter is allowed to be shown </summary>
        public Func<Game, bool> allowed;
        public Size defaultSize;

        /// <summary> Factors that govern if this plotter is allowed </summary>
        public HashSet<string> factors;
        /// <summary> Optional factors that govern if this plotter would be re-rendered - for plotters who do not listen to status changes directly </summary>
        public HashSet<string>? renderFactors;

        public PlotBase2? instance;
        public PlotContainer? form;
    }

    internal abstract class PlotBase2 : IPlotBase2
    {
        protected Game game;
        protected PlotDef def;
        public string name { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public float fade { get; set; }

        public bool stale { get; set; }
        public Image frame { get; set; }
        public Image background { get; set; }

        public Font font = GameColors.Fonts.gothic_10;
        public Color color = C.orange;

        public PlotBase2(Game game, PlotDef def)
        {
            this.game = game;
            this.def = def;
            this.name = def.name;

            this.width = def.defaultSize.Width;
            this.height = def.defaultSize.Height;

            // get initial position
            var pt = PlotPos.getPlotterLocation(this.name, new Size(this.width, this.height), Rectangle.Empty);
            this.left = pt.X;
            this.top = pt.Y;

            this.frame = new Bitmap(this.width, this.height);
            this.stale = true;
        }

        private void setSize(int width, int height)
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

        #region rendering

        public void invalidate()
        {
            //Game.log($"invalidate: {this.name}");
            this.stale = true;

            if (Game.settings.useNotOneOverlay_TEST)
                Program.invalidate(this.name);
            else
                BigOverlay.invalidate();
        }

        public bool hidden
        {
            get => this._hidden;
            set
            {
                if (value) this.hide();
                else this.show();
            }
        }
        private bool _hidden;

        /// <summary>
        /// Stop rendering this plotter without destroying it
        /// </summary>
        public void hide()
        {
            this._hidden = true;
            this.invalidate();
        }

        /// <summary>
        /// Start rendering if hidden, or invalidates if already visible
        /// </summary>
        public void show()
        {
            if (this._hidden)
            {
                this._hidden = false;
                this.fade = 0;
            }

            this.invalidate();
        }

        protected abstract SizeF doRender(Game game, Graphics g);

        /// <summary> Generate a new frame image </summary>
        public Image render()
        {
            if (game.isShutdown || game.status == null) return this.frame;

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
                    this.setSize((int)sz.Width, (int)sz.Height);
                }
                if (renderCount > 6) Debugger.Break();
            } while (stale && renderCount < 10);

            //Game.log($"Render: {this.name}, renderCount: {renderCount}");
            this.frame = nextFrame;

            // re-create background when missing or size has changed
            if (this.background == null || this.background.Size != this.frame.Size)
                this.background = GameGraphics.getBackgroundImage(frame.Size);

            return this.frame;
        }


        #endregion

        #region static def and instance mgmt

        static PlotBase2()
        {
            // start by collecting all non-abstract classes derived from PlotBase2
            var allPlotterTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(_ => typeof(PlotBase2).IsAssignableFrom(_) && !_.IsAbstract)
                .ToList();

            Game.log($"Registering {allPlotterTypes.Count} plotters");

            foreach (var type in allPlotterTypes)
            {
                var def = type
                    .GetField("plotDef", BindingFlags.Public | BindingFlags.Static)
                    ?.GetValue(null) as plotters.PlotDef;

                if (def == null)
                    throw new Exception($"Def not found: {type.Name}");
                else
                    defs[def.name] = def;
            }
        }

        private static Dictionary<string, PlotDef> defs = new Dictionary<string, PlotDef>();

        public static void add(Game game, PlotDef def)
        {
            if (def.instance != null)
            {
                Game.log($"Overlays.add: {def.name} - why already present?");
                Debugger.Break();
                remove(def);
            }

            Game.log($"Overlays.add: {def.name}");
            def.instance = def.ctor(game, def);
        }

        public static PlotDef? get(string name)
        {
            var def = defs.GetValueOrDefault(name);
            return def;
        }

        public static T? getPlotter<T>() where T : PlotBase2
        {
            var name = typeof(T).Name;
            return defs.GetValueOrDefault(name)?.instance as T;
        }

        public static void remove(PlotDef def)
        {
            if (def.instance != null)
            {
                Game.log($"Overlays.remove: {def.name}");

                if (Game.settings.useNotOneOverlay_TEST)
                {
                    Program.closePlotter(def.name);
                    def.form = null;
                }

                def.instance.close();
                def.instance = null;

                BigOverlay.invalidate();
            }
        }

        public static void invalidate(string name)
        {
            var def = defs.GetValueOrDefault(name);
            def?.instance?.invalidate();
        }

        public static void renderAll(Game game)
        {
            if (game.isShutdown || game.status == null) return;

            // check if we need to create or remove any
            var invalidateBig = false;
            foreach (var def in defs.Values)
            {
                var relevant = def.factors.Any(x => game.status.changed.Contains(x));

                var shouldShow = def.allowed(game);
                //Game.log($"relevant? {def.name} => {relevant} / {def.instance != null} / shouldShow: {shouldShow}");

                // only attempt to create or destroy if something relevant changed
                if (relevant)
                {
                    // create or destroy as needed
                    if (shouldShow && def.instance == null)
                    {
                        invalidateBig = true;
                        add(game, def);
                    }
                    else if (!shouldShow && def.instance != null)
                    {
                        invalidateBig = true;
                        remove(def);
                    }
                }

                if (shouldShow)
                {
                    // render if stale or if a relevant factor changed
                    var shouldRender = def.renderFactors != null && def.renderFactors.Any(x => game.status.changed.Contains(x));
                    if (def.instance!.stale || shouldRender)
                    {
                        invalidateBig = true;
                        def.instance.render();
                    }

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
            }

            if (invalidateBig)
                BigOverlay.current?.Invalidate();
        }

        public static void closeAll()
        {
            Game.log($"Overlays.closeAll");

            foreach (var def in defs.Values)
                remove(def);

            BigOverlay.invalidate();
        }

        /// <summary>
        /// The list of active plotters to be rendered
        /// </summary>
        public static List<IPlotBase2> active
        {
            get => defs.Values
                .Where(def => def.instance != null)
                .Select(def => def.instance!)
                .ToList<IPlotBase2>();
        }

        #endregion

        protected virtual void onStatusChange(Status status) { /* override as needed */ }

        /// <summary> Gives every active plotter a chance to process status changes </summary>
        public static void statusChanged()
        {
            foreach (var def in defs.Values)
                def.instance?.onStatusChange(def.instance.game.status);
        }
    }

    internal class PlotContainer : PlotBase
    {
        private readonly PlotDef def;

        public PlotContainer(PlotDef def)
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

        public override bool allow => Game.activeGame == null ? false : def.allowed(Game.activeGame);

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

            if (def.instance.frame == null || def.instance.stale)
                def.instance.render();

            g.DrawImage(def.instance.background, 0, 0);
            g.DrawImage(def.instance.frame!, 0, 0);
        }

    }
}
