using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using System.Diagnostics;

namespace SrvSurvey
{
    internal static class KeyChords
    {
        #region infrastructure

        private static Game? game { get => Game.activeGame; }

        public static string? keyToString(Keys key)
        {
            switch (key)
            {
                case Keys.Back: return "Backspace";
                case Keys.OemMinus: return "-";
                case Keys.Oemplus: return "+";

                // These aren't allowed to be a key-chord in their own right
                case Keys.Menu:
                case Keys.ControlKey:
                case Keys.ShiftKey:
                    return null;

                default: return key.ToString();
            }
        }

        public static string getKeyChordString(Keys key)
        {
            var chord = (Control.ModifierKeys.HasFlag(Keys.Alt) ? "ALT " : "") +
                 (Control.ModifierKeys.HasFlag(Keys.Control) ? "CTRL " : "") +
                 (Control.ModifierKeys.HasFlag(Keys.Shift) ? "SHIFT " : "") +
                 keyToString(key);

            return chord;
        }

        public static void processHook(string chord)
        {
            if (Game.settings.keyActions_TEST == null || string.IsNullOrEmpty(chord)) return;

            // do the action async, so we're not wasting synchronous time during the keyboard hook
            Program.control.BeginInvoke(() => Program.crashGuard(() =>
            {
                // does this chord match any actions?
                var handled = false;
                foreach (var action in Game.settings.keyActions_TEST)
                {
                    if (action.Value == chord)
                    {
                        Game.log($"Chord:{chord} => {action.Key}");
                        handled = doKeyAction(action.Key);
                        if (handled) break;
                    }
                }

                // this is annoying, disable for now                
                if (false && !handled && Debugger.IsAttached)
                    Game.log($"Chord:{chord} => ?");
            }));
        }

        #endregion

        public readonly static Dictionary<KeyAction, string>? defaultKeys = new()
        {
            { KeyAction.toggleAllVisibility, "ALT F2" },
            { KeyAction.mapZoomIn, "CTRL +" },
            { KeyAction.mapZoomOut, "CTRL -" },
            { KeyAction.mapZoomAuto, "CTRL SHIFT Backspace" },
            { KeyAction.mapBeHuge, "CTRL Backspace" },
            { KeyAction.showJumpInfo, "ALT D" },
            { KeyAction.pasteGalMap, "" },
            { KeyAction.copyNextBoxel, "CTRL C" },
            { KeyAction.showFssInfo, "ALT F" },
            { KeyAction.showBodyInfo, "ALT B" },
            { KeyAction.showColonyShopping, "ALT S" },
            { KeyAction.refreshColonyData, "ALT CTRL S" },
            { KeyAction.showSystemNotes, "CTRL SHIFT N" },
            { KeyAction.track1, "ALT CTRL F1" },
            { KeyAction.track2, "ALT CTRL F2" },
            { KeyAction.track3, "ALT CTRL F3" },
            { KeyAction.track4, "ALT CTRL F4" },
            { KeyAction.track5, "ALT CTRL F5" },
            { KeyAction.track6, "ALT CTRL F6" },
            { KeyAction.track7, "ALT CTRL F7" },
            { KeyAction.track8, "ALT CTRL F8" },
        };

        public static bool doKeyAction(KeyAction keyAction)
        {
            switch (keyAction)
            {
                case KeyAction.toggleAllVisibility:
                    // toggle the checkbox on the main form
                    Main.form.checkTempHide.Checked = !Main.form.checkTempHide.Checked;
                    break;

                case KeyAction.mapZoomIn: return adjustMapZooms(true);
                case KeyAction.mapZoomOut: return adjustMapZooms(false);
                case KeyAction.mapZoomAuto: return adjustMapAutoZoom();
                case KeyAction.mapBeHuge: return adjustMapHugeness();
                case KeyAction.showJumpInfo: return toggleJumpInfo();
                case KeyAction.copyNextBoxel: return copyNextBoxelSystem();
                case KeyAction.pasteGalMap: return pasteGalMap();
                case KeyAction.showFssInfo: return toggleFSSInfo();
                case KeyAction.showBodyInfo: return toggleBodyInfo();
                case KeyAction.showColonyShopping: return toggleColonyShopping();
                case KeyAction.refreshColonyData: return Game.activeGame?.cmdr?.loadColonyData().justDoIt() ?? true;
                case KeyAction.showSystemNotes: return showSystemNotes();
                case KeyAction.track1: return trackLocation(1);
                case KeyAction.track2: return trackLocation(2);
                case KeyAction.track3: return trackLocation(3);
                case KeyAction.track4: return trackLocation(4);
                case KeyAction.track5: return trackLocation(5);
                case KeyAction.track6: return trackLocation(6);
                case KeyAction.track7: return trackLocation(7);
                case KeyAction.track8: return trackLocation(8);

                default:
                    Game.log($"Unsupported key action: {keyAction}");
                    Debugger.Break();
                    break;
            }

            return true;
        }

        private static bool adjustMapZooms(bool zoomIn)
        {
            Program.getPlotter<PlotHumanSite>()?.adjustZoom(zoomIn);
            Program.getPlotter<PlotGuardians>()?.adjustZoom(zoomIn);
            return true;
        }

        private static bool adjustMapAutoZoom()
        {
            if (game != null && Program.isPlotter<PlotHumanSite>())
            {
                PlotHumanSite.autoZoom = true;
                Program.getPlotter<PlotHumanSite>()?.setZoom(game.mode);
            }
            if (game != null && Program.isPlotter<PlotGuardians>())
            {
                PlotGuardians.autoZoom = true;
                Program.getPlotter<PlotGuardians>()?.setMapScale();
            }
            return true;
        }

        private static bool adjustMapHugeness()
        {
            PlotHumanSite.toggleHugeness();

            // TODO: Handle Guardian maps?
            return true;
        }

        private static bool toggleJumpInfo()
        {
            // we need a route for this to work
            if (game?.navRoute.Route.Count > 0)
            {
                var jumpInfo = Program.getPlotter<PlotJumpInfo>();
                if (jumpInfo == null)
                {
                    // force show if no plotter
                    PlotJumpInfo.forceShow = true;
                    Program.showPlotter<PlotJumpInfo>();
                }
                else if (PlotJumpInfo.forceShow)
                {
                    // unforce (hide)
                    PlotJumpInfo.forceShow = false;
                    Program.closePlotter<PlotJumpInfo>();
                }
                else
                {
                    // the plotter exists and was not forced ... toggle forceHide on it
                    jumpInfo.forceHide = !jumpInfo.forceHide;
                    if (!jumpInfo.forceHide) PlotJumpInfo.forceShow = false;
                }
            }
            else
            {
                // unforce if there's no route currently
                PlotJumpInfo.forceShow = false;
            }

            return true;
        }

        private static bool copyNextBoxelSystem()
        {
            var nextSystem = Game.activeGame?.cmdr.boxelSearch?.nextSystem;
            if (Game.activeGame?.mode == GameMode.GalaxyMap && nextSystem != null)
            {
                Game.log($"Setting next boxel search system to clipboard: {nextSystem}");
                Clipboard.SetText(nextSystem);
                Program.invalidate<PlotSphericalSearch>();
            }

            return true;
        }

        private static bool pasteGalMap()
        {
            if (Game.activeGame?.mode != GameMode.GalaxyMap) return true;

            string? keysToSend = null;

            var cmdr = Game.activeGame.cmdr;

            // use next hop in route?
            if (keysToSend == null && cmdr?.route?.active == true)
                keysToSend = cmdr.route.nextHop?.name;

            // if boxel searching is active, but it has clipboard auto-copy disabled
            // AND we're within the boxel itself - send the next boxel system NOT what is in the clipboard
            var bs = cmdr?.boxelSearch;
            if (bs?.active == true && bs?.autoCopy == false && bs.nextSystem != null && bs.current?.containsChild(Boxel.parse(Game.activeGame?.systemData?.name)) == true)
                keysToSend = bs.nextSystem;

            // use what ever text is in the clip board
            if (keysToSend == null)
                keysToSend = Clipboard.GetText(TextDataFormat.Text);

            if (keysToSend != null)
            {
                Game.log($"Paste in gal-map: {keysToSend}");

                Elite.setFocusED();
                Program.defer(() =>
                {
                    SendKeys.SendWait(keysToSend);
                });
            }

            return true;
        }

        private static bool toggleFSSInfo()
        {
            var fssInfo = Program.getPlotter<PlotFSSInfo>();
            if (fssInfo == null)
            {
                // force show if no plotter
                PlotFSSInfo.forceShow = true;
                Program.showPlotter<PlotFSSInfo>();
            }
            else if (PlotFSSInfo.forceShow)
            {
                // unforce (hide)
                PlotFSSInfo.forceShow = false;
                Program.closePlotter<PlotFSSInfo>();
            }
            else
            {
                // the plotter exists and was not forced ... toggle forceHide on it
                fssInfo.forceHide = !fssInfo.forceHide;
                if (!fssInfo.forceHide) PlotFSSInfo.forceShow = false;
            }

            return true;
        }

        private static bool toggleBodyInfo()
        {
            // exit early if there's no relevant body
            var targetBody = Game.activeGame?.systemBody ?? Game.activeGame?.targetBody;
            if (targetBody == null)
            {
                PlotBodyInfo.forceShow = false;
                return true;
            }

            var bodyInfo = Program.getPlotter<PlotBodyInfo>();
            if (bodyInfo == null)
            {
                // force show if no plotter
                PlotBodyInfo.forceShow = true;
                Program.showPlotter<PlotBodyInfo>();
            }
            else if (PlotBodyInfo.forceShow)
            {
                // unforce (hide)
                PlotBodyInfo.forceShow = false;
                Program.closePlotter<PlotBodyInfo>();
            }
            else
            {
                // the plotter exists and was not forced ... toggle forceHide on it
                bodyInfo.forceHide = !bodyInfo.forceHide;
                if (!bodyInfo.forceHide) PlotBodyInfo.forceShow = false;
            }

            return true;
        }

        private static bool toggleColonyShopping()
        {
            var plotter = Program.getPlotter<PlotBuildCommodities>();
            if (plotter == null)
            {
                // force show if no plotter
                PlotBuildCommodities.forceShow = true;
                Program.showPlotter<PlotBuildCommodities>();
            }
            else if (PlotBuildCommodities.forceShow)
            {
                // unforce (hide)
                PlotBuildCommodities.forceShow = false;
                Program.closePlotter<PlotBuildCommodities>();
            }
            else
            {
                // the plotter exists and was not forced ... toggle forceHide on it
                plotter.forceHide = !plotter.forceHide;
                if (!plotter.forceHide) PlotBuildCommodities.forceShow = false;
            }

            return true;
        }

        private static bool showSystemNotes()
        {
            if (Game.activeGame?.isShutdown == false && Game.activeGame.fsdJumping == false && Game.activeGame.systemData != null)
            {
                Program.defer(() => BaseForm.show<FormSystemNotes>().Activate());
            }

            return true;
        }

        private static bool trackLocation(int n)
        {
            if (Game.activeGame != null)
                Game.activeGame.toggleBookmark($"#{n}", Status.here.clone());

            return true;
        }
    }

    /// <summary>
    /// Things that can be done by key presses
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    enum KeyAction
    {
        /// <summary> Hide or show all plotters </summary>
        toggleAllVisibility,
        /// <summary> Zoom in on any map </summary>
        mapZoomIn,
        /// <summary> Zoom out on any map </summary>
        mapZoomOut,
        /// <summary> Reset zoom to automatic level </summary>
        mapZoomAuto,
        /// <summary> Make map plotter become huge or normal sized </summary>
        mapBeHuge,
        /// <summary> Force show PlotJumpInfo </summary>
        showJumpInfo,
        /// <summary> Copy the next system name for boxel searches </summary>
        copyNextBoxel,
        /// <summary> Paste when in the gal-map </summary>
        pasteGalMap,
        /// <summary> Force show PlotFssInfo </summary>
        showFssInfo,
        /// <summary> Force show PlotBodyInfo </summary>
        showBodyInfo,
        /// <summary> Make the system notes window appear </summary>
        showSystemNotes,
        /// <summary> Make the PlotBuildNew appear </summary>
        showColonyShopping,
        /// <summary> Force refresh ColonyData  </summary>
        refreshColonyData,
        /// <summary> Track the current location as #1 </summary>
        track1,
        /// <summary> Track the current location as #2 </summary>
        track2,
        /// <summary> Track the current location as #3 </summary>
        track3,
        /// <summary> Track the current location as #4 </summary>
        track4,
        /// <summary> Track the current location as #5 </summary>
        track5,
        /// <summary> Track the current location as #6 </summary>
        track6,
        /// <summary> Track the current location as #7 </summary>
        track7,
        /// <summary> Track the current location as #8 </summary>
        track8,
    }
}
