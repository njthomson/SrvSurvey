using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SrvSurvey
{
    internal class KeyboardHook
    {
        const int WH_KEYBOARD_LL = 13;

        public event KeyEventHandler KeyUp;

        private readonly HookHandlerDelegate hookProcessor;
        private readonly IntPtr hookId;
        private bool ctrlPressed = false;
        private bool shiftPressed = false;
        private bool altPressed = false;

        internal delegate IntPtr HookHandlerDelegate(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

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

        private IntPtr HookCallback(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (Elite.focusElite)
            {
                var keyUp = lParam.flags >= 128;
                var keys = (Keys)lParam.vkCode;

                // track Alt, Control and Shift keys going up and down
                if (keys == Keys.LControlKey || keys == Keys.RControlKey)
                    this.ctrlPressed = !keyUp;
                else if (keys == Keys.LShiftKey || keys == Keys.RShiftKey)
                    this.shiftPressed = !keyUp;
                else if (keys == Keys.LMenu || keys == Keys.RMenu)
                    this.altPressed = !keyUp;
                else if (keyUp)
                {
                    // if it's any other key coming up: invoke our standard processor and maybe the event
                    var chord = KeyChords.getKeyChordString(keys, altPressed, ctrlPressed, shiftPressed);
                    KeyChords.processHook(chord);

                    if (this.KeyUp != null)
                    {
                        if (altPressed) keys |= Keys.Alt;
                        if (ctrlPressed) keys |= Keys.Control;
                        if (shiftPressed) keys |= Keys.Shift;
                        this.KeyUp(null, new KeyEventArgs(keys));
                    }
                }
            }
            //Pass key to next application
            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, ref lParam);
        }

        #region native methods

        private class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
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
            //int time;
            //int dwExtraInfo;
        }

        #endregion
    }
}
