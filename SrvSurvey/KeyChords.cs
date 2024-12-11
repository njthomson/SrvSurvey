using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

        public static string getKeyChordString(Keys key, bool alt, bool ctrl, bool shift)
        {
            var chord = (alt ? "ALT " : "") +
                 (ctrl ? "CTRL " : "") +
                 (shift ? "SHIFT " : "") +
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

                if (!handled && Debugger.IsAttached)
                {
                    Game.log($"Chord:{chord} => ?");
                }
            }));
        }

        #endregion

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
                case KeyAction.showFssInfo: return toggleFSSInfo();
                case KeyAction.showBodyInfo: return toggleBodyInfo();

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
                    PlotJumpInfo.forceShow = true;
                    Program.showPlotter<PlotJumpInfo>();
                }
                else if (PlotJumpInfo.forceShow)
                {
                    PlotJumpInfo.forceShow = false;
                    Program.closePlotter<PlotJumpInfo>();
                }
            }

            return true;
        }

        private static bool copyNextBoxelSystem()
        {
            var nextSystem = Game.activeGame?.cmdr.boxelSearch?.getNextToVisit();
            if (Game.activeGame?.mode == GameMode.GalaxyMap && nextSystem != null)
            {
                Game.log($"Setting next boxel search system to clipboard: {nextSystem}");
                Clipboard.SetText(nextSystem);
            }

            return true;
        }

        private static bool toggleFSSInfo()
        {
            var jumpInfo = Program.getPlotter<PlotFSSInfo>();
            if (jumpInfo == null)
            {
                PlotFSSInfo.forceShow = true;
                Program.showPlotter<PlotFSSInfo>();
            }
            else if (PlotFSSInfo.forceShow)
            {
                PlotFSSInfo.forceShow = false;
                Program.closePlotter<PlotFSSInfo>();
            }

            return true;
        }

        private static bool toggleBodyInfo()
        {
            var jumpInfo = Program.getPlotter<PlotBodyInfo>();
            if (jumpInfo == null && Game.activeGame?.systemBody != null)
            {
                PlotBodyInfo.forceShow = true;
                Program.showPlotter<PlotBodyInfo>();
            }
            else if (PlotBodyInfo.forceShow)
            {
                PlotBodyInfo.forceShow = false;
                Program.closePlotter<PlotBodyInfo>();
            }

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
        /// <summary> Force show PlotFssInfo </summary>
        showFssInfo,
        /// <summary> Force show PlotBodyInfo </summary>
        showBodyInfo,
    }
}
