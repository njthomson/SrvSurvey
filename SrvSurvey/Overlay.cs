using SrvSurvey.game;
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
        /// Set focus on Elite Dangerous
        /// </summary>
        public static void setFocusED()
        {
            var hwnd = getEDWindowHandle();
            SetForegroundWindow(hwnd);
        }

        public static void setFormMinimal(Form form)
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
            var hwndED = Overlay.getEDWindowHandle();
            var hwndActive = Overlay.GetForegroundWindow();

            //if (hwndED != hwndActive || hwndED == IntPtr.Zero)
            //{
            //    return Rectangle.Empty;
            //}

            var r1 = new RECT();
            Overlay.GetWindowRect(hwndED, ref r1);

            var r2 = new RECT();
            Overlay.GetClientRect(hwndED, ref r2);

            var windowTitleHeight = r1.Bottom - r1.Top - r2.Bottom;

            return new Rectangle(
                // use the Window rect for the top left corder
                r1.Left, r1.Top + windowTitleHeight,
                // use the Client rect for the width/height
                r2.Right, r2.Bottom);
        }

        private static IntPtr getEDWindowHandle()
        {
            //this.TopMost = !this.TopMost;
            var procED = Process.GetProcessesByName("EliteDangerous64");
            if (procED.Length == 0)
                return IntPtr.Zero;
            else
                return procED[0].MainWindowHandle;
        }

        public static void floatLeftMiddle(Form form)
        {
            // position form top center above the heading
            var rect = Overlay.getEDWindowRect();

            form.Left = rect.Left + 40;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);
        }

        //private void floatCenterMiddle(Form form)
        //{
        //    // position form top center above the heading
        //    var rect = Overlay.getEDWindowRect();

        //    form.Left = rect.Left + 40;
        //    form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);
        //}

        public static void floatCenterTop(Form form, int fromTop)
        {
            // position form top center above the heading
            var rect = Overlay.getEDWindowRect();

            form.Left = rect.Left + (rect.Width / 2) - (form.Width / 2);
            form.Top = rect.Top + fromTop;
        }

        public static void floatTopRight(Form form, int fromTop, int fromRight)
        {
            // position form top center above the heading
            var rect = Overlay.getEDWindowRect();

            form.Left = rect.Right - form.Width - fromRight;
            form.Top = rect.Top + fromTop;
        }



        [DllImport("User32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("User32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
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
