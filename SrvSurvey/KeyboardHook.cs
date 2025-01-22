using SharpDX.DirectInput;
using SrvSurvey.game;
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

        internal delegate IntPtr HookHandlerDelegate(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
        internal delegate void HookFired(bool pressing, string chord);

        private Task? taskDirectX;
        public HashSet<JoystickOffset> pressed = new();
        private bool cancelDirectX = false;
        private bool resetPending = false;

        public static bool redirect = false;
        public static event HookFired? fired;

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
                    Game.log("KeyboardHook activated");

                    if (Game.settings.hookDirectX_TEST)
                        this.startDirectX();
                }
            }
        }

        public void startDirectX()
        {
            if (this.taskDirectX == null)
            {
                // spin off long running thread to poll DirectX inputs
                cancelDirectX = false;
                this.taskDirectX = Task.Factory.StartNew(this.beginPollDirectX, TaskCreationOptions.LongRunning);
                Game.log("DirectX hook activated");
            }
        }

        public void stopDirectX()
        {
            cancelDirectX = true;
        }

        public void Dispose()
        {
            Game.log("KeyboardHook disabled");
            NativeMethods.UnhookWindowsHookEx(hookId);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (Elite.focusElite)
            {
                var keyUp = lParam.flags >= 128;
                var keys = (Keys)lParam.vkCode;

                if (keyUp)
                {
                    // if it's any other key coming up: invoke our standard processor and maybe the event
                    var chord = KeyChords.getKeyChordString(keys);
                    if (!redirect)
                        KeyChords.processHook(chord);

                    if (this.KeyUp != null)
                    {
                        this.KeyUp(null, new KeyEventArgs(keys));
                    }
                }
            }

            // pass key to next application
            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, ref lParam);
        }

        private async Task beginPollDirectX()
        {
            var directInput = new DirectInput();

            // todo: support more than one device
            var guid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                guid = deviceInstance.InstanceGuid;
            if (guid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    guid = deviceInstance.InstanceGuid;

            // exit early if nothing found
            if (guid == Guid.Empty) return;

            var joystick = new Joystick(directInput, guid);
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

            // begin polling...
            while (true)
            {
                if (cancelDirectX)
                {
                    Game.log("DirectX hook disabled");
                    this.taskDirectX = null;
                    return;
                }

                await Task.Delay(1);
                joystick.Poll();

                foreach (var state in joystick.GetBufferedData())
                {
                    var isButton = state.Offset >= JoystickOffset.Buttons0 && state.Offset <= JoystickOffset.Buttons127;
                    if (!isButton) continue; // TODO: !

                    //Debug.WriteLine(state); // tmp?
                    var isRelease = false;
                    var isPressed = false;

                    if (isButton)
                    {
                        isRelease = state.Value == 0;
                        isPressed = !isRelease;
                    }

                    // TODO: Interpret what "pressed" means for other input things
                    //if (state.Offset == JoystickOffset.PointOfViewControllers0)
                    //{
                    //    isRelease = state.Value == 0;
                    //    isPressed = !isRelease;
                    //}

                    // ready to fire?
                    if (isRelease && !resetPending)
                        this.processButtonChord();

                    // track which button is changing state
                    var changed = false;
                    if (isPressed)
                    {
                        pressed.Add(state.Offset);
                        if (pressed.Count == 1) fire(true, "");
                        changed = true;
                    }
                    else if (isRelease)
                    {
                        pressed.Remove(state.Offset);

                        // reset pending once all buttons have been released?
                        if (this.resetPending && pressed.Count == 0)
                        {
                            this.resetPending = false;
                            //Debug.WriteLine("! resetPending"); // tmp?
                        }
                        changed = true;
                    }

                    if (changed && redirect && !resetPending && pressed.Count > 0)
                    {
                        var chord = string.Join(", ", pressed.Order());
                        fire(true, chord);
                    }
                }
            }
        }

        private void fire(bool pressing, string chord)
        {
            Program.defer(() =>
            {
                //Game.log($">>> FIRE <<< {pressing} => [ {chord} ]");
                if (fired != null) fired(pressing, chord);
            });

        }

        private void processButtonChord()
        {
            var chord = string.Join(", ", pressed.Order());
            //Game.log($">>> HOOK <<< [ {chord} ]");
            resetPending = true;

            if (redirect)
                fire(false, chord);
            else
                KeyChords.processHook(chord);
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
