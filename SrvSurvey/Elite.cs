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

        /// <summary> True if we found a game process last time we checked </summary>
        public static bool hadGameProc;
        /// <summary> True if we found 2+ game process last time we checked </summary>
        public static bool hadManyGameProcs;

        private static Process? _gameProc;

        private static Process? getGameProc()
        {
            // clear reference if that process died
            if (_gameProc?.HasExited == true)
                _gameProc = null;

            if (_gameProc == null)
            {
                var edProcs = Process.GetProcessesByName("EliteDangerous64");
                if (edProcs.Length == 0)
                    _gameProc = null;
                else if (edProcs.Length == 1)
                    _gameProc = edProcs[0];
                else
                {
                    if (_procIdx > edProcs.Length - 1)
                        _procIdx = 0;

                    _gameProc = edProcs[_procIdx];
                }

                Elite.hadManyGameProcs = edProcs.Length > 1;
            }

            Elite.hadGameProc = _gameProc != null;
            return _gameProc;
        }

        private static int _procIdx = 0;
        public static int procIdx => _procIdx;

        public static void nextWindow()
        {
            _procIdx += 1;
            _gameProc = null;
            getGameProc();
        }

        public static bool isGameRunning
        {
            get
            {
                if (Program.useLastIfShutdown) return true;
                return getGameProc() != null;
            }
        }

        /// <summary>
        /// Set focus on Elite Dangerous
        /// </summary>
        public static void setFocusED(IntPtr hwnd = 0)
        {
            if (hwnd == IntPtr.Zero)
                hwnd = getWindowHandle();

            if (hwnd != IntPtr.Zero)
            {
                var isIconic = IsIconic(hwnd); // is it minimized?
                if (isIconic)
                    ShowWindow(hwnd, 0x9); // 0x9 : Restore
                else
                    SetForegroundWindow(hwnd);
            }
            else
                Game.log("setFocusED: got Zero!");
        }

        /// <summary>
        /// Will be true when we know the game currently has focus. This is checked/updated every 200ms.
        /// </summary>
        public static bool gameHasFocus { get => focusElite || focusSrvSurvey; }
        public static bool focusSrvSurvey;
        public static bool focusElite;

        /// <summary>
        /// Return the rectangle of the game window
        /// </summary>
        public static Rectangle getWindowRect(bool force = false)
        {
            if (Program.useLastIfShutdown)
            {
                return new Rectangle(80, 80, 640, 480);
            }

            var hwndED = Elite.getWindowHandle();
            var hwndActive = Elite.GetForegroundWindow();
            var isIconic = IsIconic(hwndED); // is it minimized?
            var weHaveFocus = (!Program.control.InvokeRequired && hwndActive == Main.ActiveForm?.Handle) || System.Diagnostics.Debugger.IsAttached || Game.settings.keepOverlays;

            focusElite = hwndActive == hwndED;
            focusSrvSurvey = hwndActive == Main.ActiveForm?.Handle;

            // hide plotters when game is not active (unless we are debugging or forced)
            if (!force && (isIconic || hwndED != hwndActive || hwndED == IntPtr.Zero) && !weHaveFocus)
            {
                return Rectangle.Empty;
            }

            var windowRect = new RECT();
            Elite.GetWindowRect(hwndED, ref windowRect);

            var clientRect = new RECT();
            Elite.GetClientRect(hwndED, ref clientRect);

            var dx = ((windowRect.Right - windowRect.Left) / 2) - ((clientRect.Right - clientRect.Left) / 2);
            var dy = graphicsMode == GraphicsMode.Windowed ? windowTitleHeight : 0;

            var rect = new Rectangle(
                // use the Window rect for the top left corner
                windowRect.Left + dx, windowRect.Top + dy,
                // use the Client rect for the width/height
                clientRect.Right, clientRect.Bottom);

            Elite.gameCenter = new Point(
                rect.Left + (int)((float)rect.Width / 2f),
                rect.Top + (int)((float)rect.Height / 2f));

            return rect;
        }

        private static IntPtr getWindowHandle()
        {
            var p = getGameProc();
            return p == null ? IntPtr.Zero : p.MainWindowHandle;
        }

        public static GraphicsMode getGraphicsMode()
        {
            using (var sr = new StreamReader(new FileStream(displaySettingsXml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                try
                {
                    var doc = XDocument.Load(sr);
                    var element = doc.Element("DisplayConfig")!.Element("FullScreen")!;
                    Elite.graphicsMode = (GraphicsMode)int.Parse(element.Value);
                }
                catch
                {
                    Elite.graphicsMode = GraphicsMode.Windowed;
                }

                return Elite.graphicsMode;
            }
        }

        public static GraphicsMode graphicsMode { get; private set; }

        public static void floatLeftMiddle(Form form, Rectangle rect)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Left + 40;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2);
        }

        public static void floatRightMiddle(Form form, Rectangle rect, int fromRight, int aboveMiddle = 0)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Right - form.Width - fromRight;
            form.Top = rect.Top + (rect.Height / 2) - (form.Height / 2) + aboveMiddle;
        }

        public static void floatCenterTop(Form form, Rectangle rect, int fromTop, int rightOfCenter = 0)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Left + (rect.Width / 2) - (form.Width / 2) + rightOfCenter;
            form.Top = rect.Top + fromTop;
        }

        public static void floatCenterBottom(Form form, Rectangle rect, int fromBottom, int rightOfCenter = 0)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Left + (rect.Width / 2) - (form.Width / 2) + rightOfCenter;
            form.Top = rect.Bottom - form.Height - fromBottom;
        }

        public static void floatLeftTop(Form form, Rectangle rect, int fromTop = 40, int fromLeft = 40)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Left + fromLeft;
            form.Top = rect.Top + fromTop;
        }

        public static void floatRightTop(Form form, Rectangle rect, int fromTop = 20, int fromRight = 20)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Right - form.Width - fromRight;
            form.Top = rect.Top + fromTop;
        }

        public static void floatLeftBottom(Form form, Rectangle rect, int fromBottom = 20, int fromLeft = 40)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Left + fromLeft;
            form.Top = rect.Bottom - form.Height - fromBottom;
        }

        public static void floatRightBottom(Form form, Rectangle rect, int fromBottom = 20, int fromRight = 20)
        {
            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            form.Left = rect.Right - form.Width - fromRight;
            form.Top = rect.Bottom - form.Height - fromBottom;
        }

        [DllImport("User32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("User32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int flags);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        private const int SM_CYCAPTION = 4;
        private const int SM_CYFIXEDFRAME = 8;
        private static int windowTitleHeight = GetSystemMetrics(SM_CYCAPTION) + GetSystemMetrics(SM_CYFIXEDFRAME) * 3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner

        public override string ToString()
        {
            return $"Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}, Width: {Right - Left}, Height: {Bottom - Top}";
        }
    }

    public enum GraphicsMode
    {
        Windowed = 0,
        FullScreen = 1,
        Borderless = 2
    }
}
