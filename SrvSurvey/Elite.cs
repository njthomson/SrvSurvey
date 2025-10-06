using SrvSurvey.game;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SrvSurvey
{
    static class Elite
    {
        static Elite()
        {
            var allGameProcs = GetGameProcs().ToList();
            if (allGameProcs.Count > 1)
            {
                // if there are multiple game processes but only one running for the local user - pick that by default
                var firstLocal = allGameProcs
                    .Where(p => p.IsLocalUserProcess())
                    .FirstOrDefault();
                if (firstLocal != null)
                {
                    _procIdx = allGameProcs.IndexOf(firstLocal);
                    Game.log($"Init procIdx: {_procIdx}, of {allGameProcs.Count}");
                    if (_procIdx < 0) _procIdx = 0;
                }
            }
        }

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

        public static Process[] GetGameProcs()
        {
            return Process.GetProcessesByName("EliteDangerous64");
        }

        private static Process? getGameProc()
        {
            // clear reference if that process died
            if (_gameProc?.HasExited == true)
                _gameProc = null;

            if (_gameProc == null)
            {
                var edProcs = Elite.GetGameProcs();
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

                // we poll for the process 5 times a second - assume game is still running if that found a process
                if (Elite.hadGameProc) return true;

                return getGameProc() != null;
            }
        }

        /// <summary>
        /// Set focus on Elite Dangerous
        /// </summary>
        public static void setFocusED(IntPtr hwnd = 0, bool noLog = false)
        {
            if (hwnd == IntPtr.Zero)
                hwnd = getWindowHandle();

            if (!noLog) Game.log($"setFocusED: hwnd: {hwnd}");

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
        public static bool eliteMinimized;
        public static Rectangle lastRect;

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
            eliteMinimized = IsIconic(hwndED); // is it minimized?
            var weHaveFocus = (!Program.control.InvokeRequired && hwndActive == Main.ActiveForm?.Handle) || System.Diagnostics.Debugger.IsAttached || Game.settings.keepOverlays;

            focusElite = hwndActive == hwndED;
            focusSrvSurvey = hwndActive == Main.ActiveForm?.Handle;

            //Debug.WriteLine($"hwndED: {hwndED}, hwndActive: {hwndActive}, eliteMinimized: {eliteMinimized}, focusElite: {focusElite}, focusSrvSurvey: {focusSrvSurvey}");

            if (eliteMinimized)
            {
                lastRect = Rectangle.Empty;
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

            lastRect = rect;
            return rect;
        }

        public static IntPtr getWindowHandle()
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

        public static void setOnTopGame(Form? form)
        {
            if (form == null) return;

            Game.log($">> setOnTopGame: {form.Text}");
            var hwndED = Elite.getWindowHandle();
            SetWindowLongPtr64(form.Handle, GWLP_HWNDPARENT, hwndED);

            //Elite.SetWindowPos(form.Handle, hwndED, targetRect.Left,
            //    targetRect.Top,
            //    targetRect.Right - targetRect.Left,
            //    targetRect.Bottom - targetRect.Top,
            //    SWP_NOACTIVATE | SWP_SHOWWINDOW);


            // toggle parenting twice, so our window is ontop of where the game window is
            //Elite.SetWindowPos(form.Handle, hwndED, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_NOOWNERZORDER);
            //Elite.SetWindowPos(hwndED, form.Handle, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_NOOWNERZORDER);
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

        private const int GWLP_HWNDPARENT = -8;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /*
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_ASYNCWINDOWPOS = 0x4000;
        public const uint SWP_NOOWNERZORDER = 0x0200;

        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        */

        #region getting Process user-name

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        const uint TOKEN_QUERY = 0x0008;

        public static bool IsLocalUserProcess(this Process process)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, TOKEN_QUERY, out tokenHandle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
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
