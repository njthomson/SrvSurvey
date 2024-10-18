using SrvSurvey.game;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SrvSurvey
{
    internal class KeyboardHook
    {
        const int WH_KEYBOARD_LL = 13;

        private HookHandlerDelegate hookProcessor;
        private IntPtr hookId;

        internal delegate IntPtr HookHandlerDelegate(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        public event KeyEventHandler KeyUp;

        public KeyboardHook()
        {
            hookProcessor = new HookHandlerDelegate(HookCallback);

            using (var mainModule = Process.GetCurrentProcess().MainModule)
            {
                if (mainModule != null)
                {
                    hookId = NativeMethods.SetWindowsHookEx(
                        WH_KEYBOARD_LL,
                        hookProcessor,
                        NativeMethods.GetModuleHandle(mainModule.ModuleName),
                        0);
                }
            }
        }

        public void Dispose()
        {
            NativeMethods.UnhookWindowsHookEx(hookId);
        }

        private bool ctrlPressed = false;
        private bool shiftPressed = false;
        private bool altPressed = false;

        private IntPtr HookCallback(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {

            if (Elite.focusElite)
            {
                var keyUp = lParam.flags >= 128;
                var keys = (Keys)lParam.vkCode;

                if (keys == Keys.LControlKey || keys == Keys.RControlKey)
                    this.ctrlPressed = !keyUp;
                if (keys == Keys.LShiftKey || keys == Keys.RShiftKey)
                    this.shiftPressed = !keyUp;
                if (keys == Keys.LMenu || keys == Keys.RMenu)
                    this.altPressed = !keyUp;
                else if (keyUp)
                {
                    if (shiftPressed) keys = keys | Keys.Shift;
                    if (ctrlPressed) keys = keys | Keys.Control;
                    if (altPressed) keys = keys | Keys.Alt;

                    var e = new KeyEventArgs(keys);
                    if (this.KeyUp != null)
                        this.KeyUp(null, e);

                    //var tt = (ctrlPressed ? "CTRL " : "") +
                    //     (shiftPressed ? "SHIFT " : "") +
                    //     keys.ToString();
                    //Game.log($"!! " + tt);
                }
            }
            //Game.log($">> {lParam.vkCode} / {lParam.flags} / {lParam.scanCode} | {ctrlPressed} | {shiftPressed} | {altPressed}");

            //Pass key to next application
            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, ref lParam);
        }

        private class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, HookHandlerDelegate lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
        }

        internal struct KBDLLHOOKSTRUCT
        {
            // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            public int vkCode;
            public int scanCode;
            public int flags;
            int time;
            int dwExtraInfo;
        }
    }
}
