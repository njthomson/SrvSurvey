using SrvSurvey.game;
using System.Reflection;

namespace SrvSurvey
{

    static class Program
    {
        public static Control control { get; private set; }

        public static readonly BindingFlags InstanceProps =
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetProperty |
            System.Reflection.BindingFlags.SetProperty;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(true);
            Application.ThreadException += Application_ThreadException;

            // create some control for invoking back onto the UI thread
            Program.control = new Control();
            Program.control.CreateControl();

            Application.Run(new Main());
            //Application.Run(new FormRuins()); // tmp!
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            game.Game.log("Exception: " + e.Exception.Message);
            game.Game.log("Inner exception: " + e.Exception.InnerException?.Message);
            game.Game.log("Stack: " + e.Exception.StackTrace);

            FormErrorSubmit.Show(e.Exception);
        }

        #region plotter tracking

        private static Dictionary<string, Form> activePlotters = new Dictionary<string, Form>();

        public static T showPlotter<T>() where T : Form
        {
            var formType = typeof(T);

            // only create if missing
            T form;
            if (!activePlotters.ContainsKey(formType.Name))
            {
                Game.log($"Program.Creating plotter: {formType.Name}");

                // Get the public instance constructor that takes zero parameters
                ConstructorInfo ctor = formType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    CallingConventions.HasThis, new Type[0], null)!;

                // create and force form.Name to match class name
                form = (T)ctor.Invoke(null);
                form.Name = formType.Name;

                // add to list, then show
                activePlotters.Add(formType.Name, form);
            }
            else
            {
                form = (T)activePlotters[formType.Name];
            }

            // don't try showing a disposed form
            if (form.IsDisposed) return null!;

            // show form if not visible
            if (!form.Visible)
            {
                Game.log($"Program.Showing plotter: {formType.Name}");
                form.Show();
            }

            return form;
        }

        public static void closePlotter<T>() where T : Form
        {
            closePlotter(typeof(T).Name);
        }

        private static void closePlotter(string name)
        {
            var isActive = Program.activePlotters.ContainsKey(name);

            if (isActive)
            {
                Game.log($"Program.Closing plotter: {name}");

                var plotter = Program.activePlotters[name];
                Program.activePlotters.Remove(plotter.Name);
                plotter.Close();
            }
        }

        /// <summary>
        /// Returns True if the plotter type is active
        /// </summary>
        public static bool isPlotter<T>() where T : Form
        {
            return activePlotters.ContainsKey(typeof(T).Name);
        }

        /// <summary>
        /// Returns True if the plotter type is active
        /// </summary>
        public static T? getPlotter<T>() where T : Form
        {
            return activePlotters.ContainsKey(typeof(T).Name) ? activePlotters[typeof(T).Name] as T : null;
        }

        public static void closeAllPlotters()
        {
            Game.log($"Program.CloseAllPlotters");

            var names = Program.activePlotters.Keys.ToArray();
            foreach (string name in names)
                Program.closePlotter(name);
        }

        public static void repositionPlotters(Rectangle rect)
        {
            Game.log($"Program.repositionPlotters: {activePlotters.Count}, rect{rect}");

            foreach (PlotterForm form in activePlotters.Values)
                form.reposition(rect);
        }

        public static void hideActivePlotters()
        {
            Game.log($"Program.hideActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Opacity = 0;
        }

        public static void showActivePlotters()
        {
            Game.log($"Program.showActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Opacity = Game.settings.Opacity;
        }

        public static void invalidateActivePlotters()
        {
            Game.log($"Program.hideActivePlotters: {activePlotters.Count}");

            foreach (PlotterForm form in activePlotters.Values)
                form.Invalidate();
        }

        #endregion
    }

    interface PlotterForm
    {
        void reposition(Rectangle gameRect);
        double Opacity { get; set; }
        void Invalidate();
    }
}
