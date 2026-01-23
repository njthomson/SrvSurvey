using Newtonsoft.Json;
using SharpDX.DirectInput;
using SrvSurvey.game;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SrvSurvey
{
    internal class KeyboardHook
    {
        const int WH_KEYBOARD_LL = 13;

        public static event KeyEventHandler KeyUp;

        private readonly HookHandlerDelegate hookProcessor;
        private readonly IntPtr hookId;

        internal delegate IntPtr HookHandlerDelegate(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
        internal delegate void HookFired(bool hook, string chord, int analog);

        private Task? taskDirectX;
        private HashSet<string> pressed = new();
        private bool cancelPollingTask = false;
        /// <summary> True means we are waiting for all buttons to be released </summary>
        private bool pendingButtonsRelease = false;

        public static bool redirect = false;
        public static bool analogs = false;
        public static event HookFired? buttonsPressed;
        private static int taskCount = 0;

        public Guid activeDeviceId { get; private set; }

        public KeyboardHook()
        {
            hookProcessor = new HookHandlerDelegate(hookCallback);

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
            startDirectX(newDeviceId, false);
        }

        private void startDirectX(Guid? newDeviceId, bool retrying)
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
                    taskCount++;
                    Game.log($"startDirectX ({taskCount})");

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
                        taskCount--;
                        Game.log($"DirectX hook concluded for: {newDeviceId} ({taskCount})");
                        this.taskDirectX = null;
                        this.activeDeviceId = Guid.Empty;
                    }
                }, TaskCreationOptions.LongRunning);

                Game.log($"DirectX hook activated: {newDeviceId}");
            }
            else if (!retrying && this.activeDeviceId == Guid.Empty && newDeviceId != Guid.Empty)
            {
                Game.log($"DirectX task still running. current: {this.activeDeviceId}, new: {newDeviceId}, taskCount: {taskCount}");
                cancelPollingTask = true;

                // signal to stop existing thread that is waiting for non-connected device and start over with new one
                Task.Delay(1000).ContinueWith((tt) =>
                {
                    Game.log($"DirectX task was running. Starting retry ... (current: {this.activeDeviceId}, new: {newDeviceId}, taskCount: {taskCount})");
                    startDirectX(newDeviceId, true);
                });
            }
            else
            {
                Game.log($"Why is DirectX task still running? (current: {this.activeDeviceId}, new: {newDeviceId}, taskCount: {taskCount})");
                Debugger.Break();
            }
        }

        public void stopDirectX()
        {
            Game.log($"DirectX hook request stop (current: {this.activeDeviceId})");
            cancelPollingTask = true;
        }

        public void resetChord()
        {
            this.pressed.Clear();
        }

        public void Dispose()
        {
            cancelPollingTask = true;

            Game.log("KeyboardHook disabled");
            NativeMethods.UnhookWindowsHookEx(hookId);
        }

        private IntPtr hookCallback(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (Game.settings.keyhook_TEST && (Elite.focusElite || Elite.focusSrvSurvey))
            {
                var keyUp = lParam.flags >= 128;
                var keys = (Keys)lParam.vkCode;

                if (keyUp)
                {
                    // if it's any other key coming up: invoke our standard processor and maybe the event
                    var chord = KeyChords.getKeyChordString(keys);
                    if (!redirect)
                        KeyChords.processHook(chord);

                    if (KeyUp != null)
                        KeyUp(null, new KeyEventArgs(keys));
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
                    if (cancelPollingTask || newDeviceId == Guid.Empty)
                    {
                        Game.log("DirectX hook disabled");
                        gamepad = null;
                        gamepadCapabilities = null;
                        return;
                    }

                    // do we have a device ready to use?
                    if (device == null)
                    {
                        if (!directInput.IsDeviceAttached(newDeviceId))
                        {
                            // try again in +1 seconds?
                            await Task.Delay(1_000);
                            continue;
                        }

                        device = new Joystick(directInput, newDeviceId);
                        gamepad = null;
                        gamepadCapabilities = null;

                        if (device.Capabilities.Type == DeviceType.Gamepad && !Game.settings.hookDirectXNotXInput_TEST)
                        {
                            // start with use the index from devices ... this helps when there are multiples. However suppose #0 powers down, it will disappear but we still need to use #1 below ... this is why we have that while loop
                            var gamePads = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
                            var idx = gamePads.ToList().FindIndex(d => d.InstanceGuid == device.Information.InstanceGuid);
                            while (gamepad?.IsConnected != true && idx < 5)
                                gamepad = new SharpDX.XInput.Controller((SharpDX.XInput.UserIndex)idx++);

                            if (gamepad?.IsConnected == true)
                            {
                                // we found something
                                gamepadCapabilities = gamepad.GetCapabilities(SharpDX.XInput.DeviceQueryType.Any);
                                Game.log($"initXInput: UserIndex: {gamepad.UserIndex} / IsConnected: {gamepad.IsConnected} (assumed deviceId: {newDeviceId})\nCapabilities: {JsonConvert.SerializeObject(gamepadCapabilities)}");
                                this.activeDeviceId = newDeviceId;
                            }
                            else
                            {
                                gamepad = null;
                                Game.log($"initXInput: Failed to find an XInput connected gamepad. Falling back to DirectInput...");
                            }
                        }

                        if (gamepad == null)
                        {
                            // Show the device we found
                            Game.log($"Using {device.Capabilities.Type}/{device.Capabilities.Subtype} device '{device.Properties.InstanceName}' by ID: {newDeviceId}");
                            this.activeDeviceId = newDeviceId;
                            device.Properties.BufferSize = 128;
                            Program.control.Invoke(() =>
                            {
                                device.SetCooperativeLevel(Main.form.Handle, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
                            });
                            device.Acquire();
                        }
                    }

                    await Task.Delay(1);
                    // use XInput instead?
                    if (gamepad != null)
                    {
                        this.pollXInput();
                        continue;
                    }
                    device.Poll();

                    foreach (var state in device.GetBufferedData())
                    {
                        //Debug.WriteLine(state); // dbg

                        /*
                        if (analogs)
                        {
                            if (state.Offset == JoystickOffset.X) processAxis2("RX", state, 6000);
                            if (state.Offset == JoystickOffset.Y) processAxis2("RY", state, 6000);
                            if (state.Offset == JoystickOffset.RotationZ) processAxis2("LY", state, 6000);
                        }//*/

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
                        Game.log($"beginPollDirectX failed: {ex.Message}\r\n\t{ex.StackTrace}");

                    //if ((uint)ex.HResult == 0x8007001E)
                    //    Game.log($"Lost connection to device - start polling loop to reconnect");

                    device = null;

                    // wait 1 second then try again
                    await Task.Delay(1000);
                }
            }
        }

        private readonly Dictionary<string, double> lastAxis = new();

        private void processAxis2(string name, JoystickUpdate state, int deadzone)
        {
            // avoid deadzone?
            var current = state.Value - 32_000;
            if (deadzone == -1)
            {
                if (current < 10) return;
            }
            else if (current > -deadzone && current < deadzone)
            {
                return;
            }

            if (buttonsPressed != null && redirect && analogs)
            {
                // fire event on the UX thread
                Program.control.Invoke(() => buttonsPressed(true, name, current));
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
                Program.defer(() =>
                {
                    if (buttonsPressed != null) buttonsPressed(hook, chord, 0);
                });
            }
        }

        private void processButtons(JoystickUpdate state)
        {
            var isRelease = state.Value == 0;
            var buttonName = "B" + (state.Offset - JoystickOffset.Buttons0 + 1).ToString();

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

        // --- XInput relates code ---

        private SharpDX.XInput.Controller? gamepad;
        private SharpDX.XInput.Capabilities? gamepadCapabilities;
        private SharpDX.XInput.State lastState;

        private int spinCount;
        private void pollXInput()
        {
            if (gamepad == null) return;
            if (!gamepad.GetState(out var state)) return;

            if (analogs)
            {
                if (state.PacketNumber != lastState.PacketNumber || ++spinCount > 4)
                {
                    processGamePadAxis2("LX", lastState.Gamepad.LeftThumbX, state.Gamepad.LeftThumbX, 3000);
                    processGamePadAxis2("LY", lastState.Gamepad.LeftThumbY, state.Gamepad.LeftThumbY, 3000);
                    processGamePadAxis2("RX", lastState.Gamepad.RightThumbX, state.Gamepad.RightThumbX, 3000);
                    processGamePadAxis2("RY", lastState.Gamepad.RightThumbY, state.Gamepad.RightThumbY, 3000);

                    processGamePadAxis2("LT", lastState.Gamepad.LeftTrigger, state.Gamepad.LeftTrigger, -1);
                    processGamePadAxis2("RT", lastState.Gamepad.RightTrigger, state.Gamepad.RightTrigger, -1);

                    spinCount = 0;
                }
            }

            // skip if no changes from before
            if (state.PacketNumber == lastState.PacketNumber) return;

            //Debug.WriteLine(JsonConvert.SerializeObject(state, Formatting.Indented));  // dbg

            // process buttons
            if (lastState.Gamepad.Buttons != state.Gamepad.Buttons)
            {
                foreach (var btn in buttonsForXInput)
                {
                    var wasPressed = lastState.Gamepad.Buttons.HasFlag(btn);
                    var isPressed = state.Gamepad.Buttons.HasFlag(btn);
                    if (wasPressed == isPressed) continue;

                    var name = mapXInputNames[btn.ToString()];
                    //Debug.WriteLine($"{name}: {wasPressed} / {isPressed}"); // dbg

                    if (isPressed)
                    {
                        pressed.Add(name);

                        if (!pendingButtonsRelease)
                            fire(false);
                    }
                    else if (pressed.Contains(name))
                    {
                        if (!pendingButtonsRelease || !redirect)
                            this.processButtonChord();

                        pressed.Remove(name);

                        if (!pendingButtonsRelease)
                            fire(false);
                    }
                }
            }

            // check trigger axis
            processGamePadAxis("LT", lastState.Gamepad.LeftTrigger, state.Gamepad.LeftTrigger);
            processGamePadAxis("RT", lastState.Gamepad.RightTrigger, state.Gamepad.RightTrigger);

            // TODO: It's viable to support thumb axis ... but skipping as we didn't previously

            if (pendingButtonsRelease)
                pendingButtonsRelease = pressed.Count > 0;

            lastState = state;
        }

        private void processGamePadAxis2(string name, short last, short current, int deadzone)
        {
            // avoid deadzone?
            if (deadzone == -1)
            {
                if (current < 10) return;
            }
            else if (current > -deadzone && current < deadzone)
                return;

            if (buttonsPressed != null && redirect && analogs)
            {
                // fire event on the UX thread
                Program.control.Invoke(() => buttonsPressed(true, name, current));
            }
        }

        private void processGamePadAxis(string name, byte last, byte current)
        {
            if (last == current) return;

            //Debug.WriteLine($"{name}: {last} v {current}"); // dbg

            var triggered = current >= 250; // allow with 5 to trigger
            if (triggered)
            {
                pressed.Add(name);

                if (!pendingButtonsRelease)
                    fire(false);
            }
            else if (pressed.Contains(name))
            {
                if (!pendingButtonsRelease || !redirect)
                    this.processButtonChord();

                pressed.Remove(name);

                if (!pendingButtonsRelease)
                    fire(false);
            }
        }

        private static IEnumerable<SharpDX.XInput.GamepadButtonFlags> buttonsForXInput = Enum.GetValues<SharpDX.XInput.GamepadButtonFlags>().Skip(1);
        private static Dictionary<string, string> mapXInputNames = new Dictionary<string, string>()
        {
            { "DPadUp", "PovU" },
            { "DPadDown", "PovD" },
            { "DPadLeft", "PovL" },
            { "DPadRight", "PovR" },
            { "Start", "B8" },
            { "Back", "B7" },
            { "LeftThumb", "B9" },
            { "RightThumb", "B10" },
            { "LeftShoulder", "B5" },
            { "RightShoulder", "B6" },
            { "A", "B1" },
            { "B", "B2" },
            { "X", "B3" },
            { "Y", "B4" },
            { "LeftTrigger", "LT" },
            { "RightTrigger", "RT" },
        };

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
