using SrvSurvey.game;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SrvSurvey
{
    class Elite
    {
        public static readonly string displaySettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Frontier Developments\\Elite Dangerous\\Options\\Graphics");

        public static readonly string displaySettingsXml = Path.Combine(displaySettingsFolder, "DisplaySettings.xml");

        public static readonly string defaultScreenshotFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 
            "Frontier Developments", "Elite Dangerous");

        public static Point gameCenter = Point.Empty;

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
            else
                Game.log("setFocusED: got Zero!");
        }

        public static Rectangle getWindowRect(bool force = false)
        {
            var hwndED = Elite.getWindowHandle();
            var hwndActive = Elite.GetForegroundWindow();

            // hide plotters when game is not active (unless we are debugging or forced)
            if (!force && (hwndED != hwndActive || hwndED == IntPtr.Zero) && !System.Diagnostics.Debugger.IsAttached)
            {
                return Rectangle.Empty;
            }

            var windowRect = new RECT();
            Elite.GetWindowRect(hwndED, ref windowRect);

            var clientRect = new RECT();
            Elite.GetClientRect(hwndED, ref clientRect);

            var windowTitleHeight = windowRect.Bottom - windowRect.Top - clientRect.Bottom;
            if (windowTitleHeight == 0)
                windowTitleHeight = 4;
            
            var rect = new Rectangle(
                // use the Window rect for the top left corder
                windowRect.Left, windowRect.Top + windowTitleHeight,
                // use the Client rect for the width/height
                clientRect.Right, clientRect.Bottom);

            Elite.gameCenter = new Point(
                rect.Left + (int)((float)rect.Width / 2f),
                rect.Top + (int)((float)rect.Height / 2f));

            return rect;
        }

        private static IntPtr getWindowHandle()
        {
            var procED = Process.GetProcessesByName("EliteDangerous64");
            if (procED.Length == 0)
                return IntPtr.Zero;

            if (Game.settings.processIdx > procED.Length - 1)
                Game.settings.processIdx = 0;

            return procED[Game.settings.processIdx].MainWindowHandle;
        }

        public static int getGraphicsMode()
        {
            using (var sr = new StreamReader(new FileStream(displaySettingsXml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                try
                {
                    var doc = XDocument.Load(sr);
                    var element = doc.Element("DisplayConfig")!.Element("FullScreen")!;
                    Game.log(element);
                    return int.Parse(element.Value);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static void floatLeftMiddle(Form form, Rectangle rect)
        {
            // position form top center above the heading
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Left + 40;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);
        }

        public static void floatRightMiddle(Form form, Rectangle rect, int fromRight, int aboveMiddle = 0)
        {
            // position form top center above the heading
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

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

        public static void floatCenterTop(Form form, Rectangle rect, int fromTop, int rightOfCenter = 0)
        {
            // position form top center above the heading
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

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
