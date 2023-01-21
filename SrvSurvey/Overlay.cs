using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    class Overlay
    {
        /// <summary>
        /// Toggle if the target form is in overlay mode or not.
        /// </summary>
        public static void setOverlay(Form form)
        {
            form.TopMost = true;
            form.Opacity = 0.5;

            // position form just in from the left edge, half way down
            var rect = Overlay.getEDWindowRect();

            form.Left = rect.Left + 40;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);

            setFormMinimal(form);

            // and put focus onto ED
            setFocusED();
        }

        /// <summary>
        /// Set focus on Elite Dangerous
        /// </summary>
        public static void setFocusED()
        {
            var hwnd = getEDWindowHandle();
            SetForegroundWindow(hwnd);
        }

        public static void setFormMinimal (Form form)
        {
            if (form.ControlBox == false)
            {
                form.Text = "Srv Survey";
                form.ControlBox = true;
                form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                form.SizeGripStyle = SizeGripStyle.Show;
            }
            else
            {
                form.Text = "";
                form.ControlBox = false;
                form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                form.SizeGripStyle = SizeGripStyle.Hide;
            }
        }

        public static Rectangle getEDWindowRect()
        {
            var handleED = Overlay.getEDWindowHandle();

            var r1 = new RECT();
            Overlay.GetWindowRect(handleED, ref r1);
            var r2 = new RECT();
            Overlay.GetClientRect(handleED, ref r2);
            return new Rectangle(r1.Left, r1.Top, r2.Right, r2.Bottom );
        }

        private static IntPtr getEDWindowHandle()
        {
            //this.TopMost = !this.TopMost;
            var procED = Process.GetProcessesByName("EliteDangerous64");
            var handleED = procED[0].MainWindowHandle;

            return handleED;
        }

        [DllImport("User32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("User32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }
}
