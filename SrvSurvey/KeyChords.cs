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
            { KeyAction.showStationInfo, "ALT I" },
            { KeyAction.showColonyShopping, "ALT S" },
            { KeyAction.refreshColonyData, "ALT CTRL S" },
            { KeyAction.collapseColonyData, "ALT SHIFT S" },
            { KeyAction.showSystemNotes, "CTRL SHIFT N" },
            { KeyAction.track1, "ALT CTRL F1" },
            { KeyAction.track2, "ALT CTRL F2" },
            { KeyAction.track3, "ALT CTRL F3" },
            { KeyAction.track4, "ALT CTRL F4" },
            { KeyAction.track5, "ALT CTRL F5" },
            { KeyAction.track6, "ALT CTRL F6" },
            { KeyAction.track7, "ALT CTRL F7" },
            { KeyAction.track8, "ALT CTRL F8" },
            { KeyAction.nextWindow, "ALT CTRL W" },
            { KeyAction.streamOne, "ALT CTRL O" },
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
                case KeyAction.showStationInfo: return toggleStationInfo();
                case KeyAction.showColonyShopping: return toggleColonyShopping();
                case KeyAction.refreshColonyData: return Game.activeGame?.cmdrColony.fetchLatest().justDoIt() ?? true;
                case KeyAction.collapseColonyData: return toggleCollapseColonyRows();
                case KeyAction.showSystemNotes: return showSystemNotes();
                case KeyAction.track1: return trackLocation(1);
                case KeyAction.track2: return trackLocation(2);
                case KeyAction.track3: return trackLocation(3);
                case KeyAction.track4: return trackLocation(4);
                case KeyAction.track5: return trackLocation(5);
                case KeyAction.track6: return trackLocation(6);
                case KeyAction.track7: return trackLocation(7);
                case KeyAction.track8: return trackLocation(8);
                case KeyAction.nextWindow: return focusNextGameWindow();
                case KeyAction.streamOne: return toggleStreamOne();

                default:
                    Game.log($"Unsupported key action: {keyAction}");
                    Debugger.Break();
                    break;
            }

            return true;
        }

        private static bool adjustMapZooms(bool zoomIn)
        {
            if (game == null) return true;

            Program.getPlotter<PlotHumanSite>()?.adjustZoom(zoomIn);
            Program.getPlotter<PlotGuardians>()?.adjustZoom(zoomIn);
            return true;
        }

        private static bool adjustMapAutoZoom()
        {
            if (game == null) return true;

            if (Program.isPlotter<PlotHumanSite>())
            {
                PlotHumanSite.autoZoom = true;
                Program.getPlotter<PlotHumanSite>()?.setZoom(game.mode);
            }
            if (Program.isPlotter<PlotGuardians>())
            {
                PlotGuardians.autoZoom = true;
                Program.getPlotter<PlotGuardians>()?.setMapScale();
            }
            return true;
        }

        private static bool adjustMapHugeness()
        {
            if (game == null) return true;

            PlotHumanSite.toggleHugeness();

            // TODO: Handle Guardian maps?
            return true;
        }

        private static bool toggleJumpInfo()
        {
            if (game == null) return true;

            // we need a route for this to work
            if (game.navRoute.Route.Count > 0)
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
            if (game == null) return true;

            var nextSystem = game.cmdr.boxelSearch?.nextSystem;
            if (game.mode == GameMode.GalaxyMap && nextSystem != null)
            {
                Game.log($"Setting next boxel search system to clipboard: {nextSystem}");
                Clipboard.SetText(nextSystem);
                Program.invalidate<PlotSphericalSearch>();
            }

            return true;
        }

        private static bool pasteGalMap()
        {
            if (game?.mode != GameMode.GalaxyMap) return true;

            string? keysToSend = null;

            var cmdr = game.cmdr;

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
            if (game == null) return true;

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
            if (game == null) return true;

            // exit early if there's no relevant body
            var targetBody = game?.systemBody ?? game?.targetBody;
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

        private static bool toggleStationInfo()
        {
            if (game == null) return true;

            var stationInfo = PlotBase2.getPlotter<PlotStationInfo>();
            if (stationInfo == null)
            {
                // force show if no plotter
                PlotStationInfo.forceShow = true;
                PlotBase2.add(game, PlotStationInfo.plotDef);
            }
            else if (PlotStationInfo.forceShow)
            {
                // unforce (hide)
                PlotStationInfo.forceShow = false;
                PlotBase2.remove(PlotStationInfo.plotDef);
            }
            else
            {
                // the plotter exists and was not forced ... toggle forceHide on it
                stationInfo.hidden = !stationInfo.hidden;
                if (!stationInfo.hidden) PlotStationInfo.forceShow = false;
            }

            return true;
        }

        private static bool toggleColonyShopping()
        {
            if (game == null || !Game.settings.autoShowPlotBuildCommodities) return true;

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

        private static bool toggleCollapseColonyRows()
        {
            if (game == null) return true;

            // only toggle if the plotter is active
            var plotter = Program.getPlotter<PlotBuildCommodities>();
            if (plotter != null)
            {
                PlotBuildCommodities.toggleCollapse = !PlotBuildCommodities.toggleCollapse;
                plotter.Invalidate();
            }

            return true;
        }

        private static bool showSystemNotes()
        {
            if (game == null) return true;

            if (game?.isShutdown == false && game.fsdJumping == false && game.systemData != null)
            {
                Program.defer(() => BaseForm.show<FormSystemNotes>().Activate());
            }

            return true;
        }

        private static bool trackLocation(int n)
        {
            if (game == null) return true;

            game.toggleBookmark($"#{n}", Status.here.clone());

            return true;
        }

        private static bool focusNextGameWindow()
        {
            var doForceNextWindow = new Action<int>(async (int count) =>
            {
                while (--count > 0)
                {
                    Application.DoEvents();
                    Application.DoEvents();
                    FormMultiFloatie.focusNextWindow();
                    Application.DoEvents();
                    Application.DoEvents();

                    await Task.Delay(20);
                }
            });

            doForceNextWindow(8);

            return true;
        }

        private static bool toggleStreamOne()
        {
            if (Game.settings.streamOneOverlay)
            {
                Game.settings.streamOneOverlay = false;
                PlotBase.stopWindowOne();
            }
            else
            {
                Game.settings.streamOneOverlay = true;
                PlotBase.startWindowOne();
                Program.invalidateActivePlotters();
            }

            Game.settings.Save();
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
        /// <summary> Force show PlotStationInfo </summary>
        showStationInfo,
        /// <summary> Make the system notes window appear </summary>
        showSystemNotes,
        /// <summary> Make the PlotBuildNew appear </summary>
        showColonyShopping,
        /// <summary> Force refresh ColonyData </summary>
        refreshColonyData,
        /// <summary> Collapse ColonyData rows if enough on FCs </summary>
        collapseColonyData,
        /// <summary> Set focus on the next game window </summary>
        nextWindow,
        /// <summary> Toggle setting streamOneOverlay </summary>
        streamOne,
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
