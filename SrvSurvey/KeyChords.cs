using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using System.Diagnostics;

namespace SrvSurvey
{
    internal static class KeyChords
    {
        private static Game? game { get => Game.activeGame; }

        public static void processHook(string chord)
        {
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
    }
}
