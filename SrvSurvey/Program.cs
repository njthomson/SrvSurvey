using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    static class Program
    {
        public static Control control { get; private set; }

        public static readonly BindingFlags InstanceProps = System.Reflection.BindingFlags.Public |
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

            Program.control = new Control();
            Program.control.CreateControl();

            Application.Run(new Main());
        }
    }
}
