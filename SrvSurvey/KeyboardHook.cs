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
        internal delegate void HookFired(bool hook, string chord);

        private Task? taskDirectX;
        private HashSet<string> pressed = new();
        private bool cancelPollingTask = false;
        /// <summary> True means we are waiting for all buttons to be released </summary>
        private bool pendingButtonsRelease = false;

        public static bool redirect = false;
        public static event HookFired? buttonsPressed;

        public Guid activeDeviceId { get; private set; }

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

                    if (Game.settings.hookDirectX_TEST && Game.activeGame != null)
                        this.startDirectX(Game.settings.hookDirectXDeviceId_TEST);
                }
            }
        }

        public void startDirectX(Guid? newDeviceId)
        {
            // exit early if the device isn't changing
            if (this.activeDeviceId == newDeviceId || newDeviceId == null) // && this.taskDirectX != null)
                return;

            if (this.taskDirectX == null)
            {
                // spin off long running thread to poll DirectX inputs
                cancelPollingTask = false;
                this.taskDirectX = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        if (newDeviceId == Guid.Empty)
                        {
                            Game.log($"No device specified");
                            return;
                        }

                        await this.doPollDirectX(newDeviceId ?? Guid.Empty);
                    }
                    finally
                    {
                        this.taskDirectX = null;
                        this.activeDeviceId = Guid.Empty;
                    }
                }, TaskCreationOptions.LongRunning);

                Game.log($"DirectX hook activated: {newDeviceId}");
            }
            else
            {
                Game.log($"Why is DirectX task still running? ({newDeviceId})");
                Debugger.Break();
            }
        }

        public void stopDirectX()
        {
            cancelPollingTask = true;
        }

        public void Dispose()
        {
            cancelPollingTask = true;

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

        private async Task doPollDirectX(Guid newDeviceId)
        {
            DirectInput directInput = new DirectInput();
            Joystick? device = null;
            this.pendingButtonsRelease = false;
            this.lastPov = null;
            this.lastTrigger = null;
            this.pressed.Clear();

            while (true)
            {
                try
                {
                    if (cancelPollingTask)
                    {
                        Game.log("DirectX hook disabled");
                        return;
                    }

                    // do we have a device ready to use?
                    if (device == null)
                    {
                        device = new Joystick(directInput, newDeviceId);

                        // Show the device we found
                        Game.log($"Using {device.Capabilities.Type} device '{device.Properties.InstanceName}' by ID: {newDeviceId}");
                        this.activeDeviceId = newDeviceId;
                        device.Properties.BufferSize = 128;
                        Program.control.Invoke(() =>
                        {
                            device.SetCooperativeLevel(Main.form.Handle, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
                        });
                        device.Acquire();
                    }

                    await Task.Delay(1);
                    device.Poll();

                    foreach (var state in device.GetBufferedData())
                    {
                        //Debug.WriteLine(state); // dbg

                        var isButton = state.Offset >= JoystickOffset.Buttons0 && state.Offset <= JoystickOffset.Buttons127;
                        if (isButton)
                            processButtons(state);

                        else if (state.Offset == JoystickOffset.PointOfViewControllers0)
                            processPointOfViewControllers(state);

                        else if (device.Capabilities.Type == DeviceType.Gamepad && state.Offset == JoystickOffset.Z)
                            processGamePadTrigger(state);

                        // reset pending once all buttons have been released
                        if (this.pendingButtonsRelease && pressed.Count == 0)
                        {
                            this.pendingButtonsRelease = false;
                            //Debug.WriteLine($"! resetPending: {device.Information.InstanceName}"); // dbg
                        }
                    }
                }
                catch (Exception ex)
                {
                    // log the error if we think we should have a device
                    if (device != null)
                        Game.log($"beginPollDirectX failed: {ex.Message}");

                    //if ((uint)ex.HResult == 0x8007001E)
                    //    Game.log($"Lost connection to device - start polling loop to reconnect");

                    device = null;

                    // wait 1 second then try again
                    await Task.Delay(1000);

                }
            }
        }

        /// <summary> Triggers key-chord handlers </summary>
        private void processButtonChord()
        {
            pendingButtonsRelease = true;

            //Game.log($">>> HOOK? <<< [ {string.Join(" ", pressed.Order())} ]"); // dbg
            if (redirect)
            {
                fire(true);
            }
            else if (Elite.focusElite)
            {
                var chord = string.Join(" ", pressed.Order());
                KeyChords.processHook(chord);
            }
        }

        private void fire(bool hook)
        {
            if (redirect && buttonsPressed != null)
            {
                var chord = string.Join(" ", pressed.Order());
                //Game.log($">>> FIRE <<< {hook} => [ {chord} ]"); // dbg

                // fire event on the UX thread
                Program.defer(() => buttonsPressed(hook, chord));
            }
        }

        private void processButtons(JoystickUpdate state)
        {
            var isRelease = state.Value == 0;
            var buttonName = "B" + (state.Offset - JoystickOffset.Buttons0).ToString();

            if (isRelease) // a button is released...
            {
                if (!pendingButtonsRelease || !redirect)
                    this.processButtonChord();

                // remove button
                pressed.Remove(buttonName);
            }
            else // a button is pressed...
            {
                pressed.Add(buttonName);
            }

            // tell UX about this combination?
            if (redirect && !pendingButtonsRelease && pressed.Count > 0)
                fire(false);
        }

        private void processPointOfViewControllers(JoystickUpdate state)
        {
            var nextPos = decodePos(state.Value, "Pov");
            if (lastPov != null)
            {
                // fire when releasing
                if (!pendingButtonsRelease || !redirect)
                    processButtonChord();

                pressed.Remove(lastPov);
                lastPov = null;
            }

            if (nextPos != null)
            {
                pressed.Add(nextPos);
                lastPov = nextPos;

                if (redirect) fire(false);
            }
        }
        private string? lastPov;

        private static string? decodePos(int value, string prefix)
        {
            switch (value)
            {
                case 0: return prefix + "U";      // Up
                case 4500: return prefix + "UR";  // Up Right
                case 9000: return prefix + "R";   // Right
                case 13500: return prefix + "DR"; // Down Right
                case 18000: return prefix + "D";  // Down
                case 22500: return prefix + "DL"; // Down Left
                case 27000: return prefix + "L";  // Left
                case 31500: return prefix + "UP"; // Up Left
                case -1:
                default: return null;
            }
        }

        private void processGamePadTrigger(JoystickUpdate state)
        {
            // TODO: Need to switch library if we want to reliably catch BOTH triggers at the same time :(
            string? trigger = null;

            if (state.Value < 200)
                trigger = "RT"; // Right trigger
            else if (state.Value > 65_000)
                trigger = "LT"; // Left trigger

            //if (lastTrigger != null || trigger != null) Debug.WriteLine($">>> {lastTrigger} / {trigger} <<<"); // dbg

            if (lastTrigger != null)
            {
                // fire when releasing
                if (!pendingButtonsRelease || !redirect)
                    processButtonChord();

                pressed.Remove(lastTrigger);
                lastTrigger = null;
            }

            if (trigger != null)
            {
                pressed.Add(trigger);
                lastTrigger = trigger;

                if (redirect) fire(false);
            }
        }
        private string? lastTrigger;

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
