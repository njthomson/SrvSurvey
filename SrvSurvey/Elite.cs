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
    class Elite
    {
        public static bool isGameRunning
        {
            get
            {
                var procED = Process.GetProcessesByName("EliteDangerous64");
                return procED.Length > 0;
            }
        }

        /// <summary>
        /// Set focus on Elite Dangerous
        /// </summary>
        public static void setFocusED()
        {
            var hwnd = getWindowHandle();
            if (hwnd != IntPtr.Zero)
                SetForegroundWindow(hwnd);
        }

        public static Rectangle getWindowRect()
        {
            var hwndED = Elite.getWindowHandle();
            var hwndActive = Elite.GetForegroundWindow();

            // hide plotters when game is not active (unless we are debugging)
            if ((hwndED != hwndActive || hwndED == IntPtr.Zero) && !System.Diagnostics.Debugger.IsAttached)
            {
                return Rectangle.Empty;
            }

            var windowRect = new RECT();
            Elite.GetWindowRect(hwndED, ref windowRect);

            var clientRect = new RECT();
            Elite.GetClientRect(hwndED, ref clientRect);

            var windowTitleHeight = windowRect.Bottom - windowRect.Top - clientRect.Bottom;

            return new Rectangle(
                // use the Window rect for the top left corder
                windowRect.Left, windowRect.Top + windowTitleHeight,
                // use the Client rect for the width/height
                clientRect.Right, clientRect.Bottom);
        }

        private static IntPtr getWindowHandle()
        {
            var procED = Process.GetProcessesByName("EliteDangerous64");
            if (procED.Length == 0)
                return IntPtr.Zero;
            else
                return procED[0].MainWindowHandle;
        }

        public static void floatLeftMiddle(Form form)
        {
            // position form top center above the heading
            var rect = Elite.getWindowRect();

            form.Left = rect.Left + 40;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);
        }

        public static void floatRightMiddle(Form form, int fromRight, int aboveMiddle = 0)
        {
            // position form top center above the heading
            var rect = Elite.getWindowRect();

            form.Left = rect.Right - form.Width - fromRight;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2) + aboveMiddle;
        }

        //private void floatCenterMiddle(Form form)
        //{
        //    // position form top center above the heading
        //    var rect = Overlay.getEDWindowRect();

        //    form.Left = rect.Left + 40;
        //    form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);
        //}

        public static void floatCenterTop(Form form, int fromTop, int rightOfCenter = 0)
        {
            // position form top center above the heading
            var rect = Elite.getWindowRect();

            form.Left = rect.Left + (rect.Width / 2) - (form.Width / 2) + rightOfCenter;
            form.Top = rect.Top + fromTop;
        }

        public static void floatTopRight(Form form, int fromTop, int fromRight)
        {
            // position form top center above the heading
            var rect = Elite.getWindowRect();

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
