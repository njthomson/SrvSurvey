using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;

            // create some control for invoking back onto the UI thread
            Program.control = new Control();
            Program.control.CreateControl();

            Application.Run(new Main());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            game.Game.log("Exception: " + e.Exception.Message);
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
                Game.log($"Creating plotter: {formType.Name}");

                // Get the public instance constructor that takes zero parameters
                var ctor = formType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    CallingConventions.HasThis, new Type[0], null);

                // create and force form.Name to match class name
                form = ctor.Invoke(null) as T;
                form.Name = formType.Name;

                // add to list, then show
                activePlotters.Add(formType.Name, form);
            }
            else
            {
                form = activePlotters[formType.Name] as T;
            }

            // show form if not visible
            if (!form.Visible)
            {
                Game.log($"Showing plotter: {formType.Name}");
                form.Show();
            }

            return form;
        }

        public static void closePlotter(string name)
        {
            var isActive = Program.activePlotters.ContainsKey(name);

            if (isActive)
            {
                Game.log($"Closing plotter: {name}");

                var plotter = Program.activePlotters[name];
                plotter.Close();
                plotter.Dispose();
                Program.activePlotters.Remove(plotter.Name);
            }
        }

        public static void closeAllPlotters()
        {
            Game.log($"closeAllPlotters");

            var names = Program.activePlotters.Keys.ToArray();
            foreach (string name in names)
            {
                Program.closePlotter(name);
            }
        }

        public static void repositionPlotters()
        {
            var rect = Overlay.getEDWindowRect();

            foreach(PlotterForm form in activePlotters.Values)
            {
                form.reposition(rect);
            }
        }

        #endregion
    }

    interface PlotterForm
    {
        void reposition(Rectangle gameRect);
    }
}
