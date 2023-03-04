using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public static T showPlotter<T>(Form parent = null) where T : Form
        {
            var formType = typeof(T);

            // only create if missing
            T form;
            if (!activePlotters.ContainsKey(formType.Name))
            {

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
                form.Show(parent);

            return form;
        }

        public static void closePlotter(string name)
        {
            var isActive = Program.activePlotters.ContainsKey(name);
            Game.log($"Clearing lat/long. Active plotter: {isActive}");

            if (isActive)
            {
                var plotter = Program.activePlotters[name];
                plotter.Close();
                plotter.Dispose();
                Program.activePlotters.Remove(plotter.Name);
            }
        }

        #endregion
    }
}
