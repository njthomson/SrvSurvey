using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Numerics;

namespace SrvSurvey.plotters
{
    internal class PlotAdjustVR : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotAdjustVR),
            allowed = allowed,
            ctor = (game, def) => new PlotAdjustVR(game, def),
            defaultSize = new Size(300, 80),
        };

        public static bool allowed(Game game)
        {
            return force;
        }

        public static bool force;

        public static void start()
        {
            PlotAdjustVR.force = true;

            VR.init();
            Application.DoEvents();
            PlotBase2.renderAll(Game.activeGame, true);
        }

        #endregion

        private List<string> allNames;
        private string targetName;
        private PlotPos.VR pp;
        private bool rotate;
        private string? confirmation;
        private bool confirming => confirmation != null;
        private bool selfClosing;
        public static string? overrideName;

        private PlotAdjustVR(Game game, PlotDef def) : base(game, def)
        {
            font = GameColors.Fonts.gothic_12;
            overrideName = "";

            allNames = PlotPos.getAllPlotterNames().Order().ToList();
            targetName = allNames.First();
            FormAdjustOverlay.targetName = targetName;
            pp = getPP();

            KeyboardHook.analogs = true;
            KeyboardHook.redirect = true;
            KeyboardHook.buttonsPressed += KeyboardHook_buttonsPressed;

            selfClosing = VR.app == null || !Game.settings.hookDirectX_TEST || Game.settings.hookDirectXDeviceId_TEST == Guid.Empty;
            if (selfClosing)
                Util.deferAfter(5000, () => PlotBase2.remove(PlotAdjustVR.def));
        }

        protected override void onClose()
        {
            base.onClose();
            overrideName = null;
            KeyboardHook.redirect = false;
            KeyboardHook.analogs = false;
            KeyboardHook.buttonsPressed -= KeyboardHook_buttonsPressed;
            PlotBase2.remove(PlotAdjustFake.def);
            PlotAdjustFake.def.name = nameof(PlotAdjustFake);

            FormAdjustOverlay.targetName = null;
            force = false;
        }

        private PlotPos.VR getPP()
        {
            var pPos = PlotPos.get(targetName);
            if (pPos!.vr == null)
            {
                pPos.vr = new() { s = 10, p = new Vector3(-8, 10, 45), r = new Vector3() };
            }

            // do we have an override?
            if (overrideName != null && VR.overrides.GetValueOrDefault(overrideName)?.TryGetValue(targetName, out var alternatePP) == true)
                return alternatePP;

            return pPos.vr;
        }

        private void setNextOverlay(string nextTarget)
        {
            // remove fake?
            PlotBase2.remove(PlotAdjustFake.def);

            // change name here, then invalidate old one (removing the yellow box)
            FormAdjustOverlay.targetName = nextTarget;
            PlotBase2.invalidate(targetName);

            targetName = nextTarget;
            pp = getPP();

            var targetDef = PlotBase2.get(nextTarget);
            if (targetDef != null && targetDef.instance == null)
            {
                // we need a fake to stand
                PlotAdjustFake.def.name = nextTarget;
                PlotAdjustFake.def.defaultSize = PlotPos.typicalSize.ContainsKey(nextTarget) ? PlotPos.typicalSize[nextTarget] : new Size(200, 100);
                var instance = PlotBase2.add(game, PlotAdjustFake.def);
                PlotBase2.renderAll(game);
            }
        }

        private void KeyboardHook_buttonsPressed(bool hook, string chord, int analog)
        {
            // only process when buttons are released and we want these events
            if (!hook || !KeyboardHook.redirect || !KeyboardHook.analogs) return;

            if (chord == Game.settings.keyActions_TEST?.GetValueOrDefault(KeyAction.adjustVR))
            {
                force = false;
                PlotBase2.renderAll(game, true);
                return;
            }

            if (chord == "B1")
            {
                if (confirmation == "exit")
                {
                    // save changes and exit
                    PlotPos.saveCustomPositions();
                    VR.saveOverlayOverrides();
                    force = false;
                    PlotBase2.renderAll(game, true);
                }
                else if (confirmation == "cancel")
                {
                    // close without saving 
                    force = false;
                    PlotBase2.renderAll(game, true);
                }
                else if (confirmation == "reset")
                {
                    // reset current overlay
                    PlotPos.resetVRToDefault(targetName);
                    confirmation = null;
                }
            }
            else if (chord == "B2")
            {
                if (confirming)
                    confirmation = null;
                else if (!confirming)
                    confirmation = "cancel";
            }
            else if (chord == "B3")
            {
                if (!confirming)
                    confirmation = "exit";
            }
            else if (chord == "B2 B3")
            {
                if (!confirming)
                    confirmation = "reset";
            }
            else if (!confirming)
            {
                if (chord == "PovR")
                {
                    // next overlay
                    var idx = allNames.IndexOf(targetName) + 1;
                    if (idx >= allNames.Count) idx = 0;
                    setNextOverlay(allNames[idx]);
                }
                else if (chord == "PovL")
                {
                    PlotBase2.invalidate(targetName);

                    // next overlay
                    var idx = allNames.IndexOf(targetName) - 1;
                    if (idx < 0) idx = allNames.Count - 1;
                    setNextOverlay(allNames[idx]);
                }
                else if (chord == "PovU")
                {
                    // toggle overrideName
                    if (string.IsNullOrWhiteSpace(overrideName))
                    {
                        overrideName = VR.vrMode;
                        // do we have an override?
                        var overrides = VR.overrides.init(overrideName);

                        // default to cloned current data if no current
                        if (!overrides.ContainsKey(targetName))
                            overrides[targetName] = PlotPos.VR.parse(pp.ToString())!;
                    }
                    else
                    {
                        overrideName = "";
                    }
                    pp = getPP();
                }
                else if (chord == "B4")
                {
                    // toggle rotate or relocate
                    rotate = !rotate;
                }
                else if (chord == "RX")
                {
                    if (analog > -4000 && analog < 4000) return; // deadzone
                    if (rotate)
                        pp.r.Y += (float)analog / 50_000;
                    else
                        pp.p.X += (float)analog / 200_000;
                }
                else if (chord == "RY")
                {
                    if (analog > -4000 && analog < 4000) return; // deadzone
                    if (rotate)
                        pp.r.X += (float)analog / 50_000;
                    else
                        pp.p.Y += (float)analog / 200_000;
                }
                else if (chord == "LY")
                {
                    if (analog > -4000 && analog < 4000) return; // deadzone
                    if (rotate)
                        pp.r.Z += (float)analog / 50_000;
                    else
                        pp.p.Z += (float)analog / 200_000;
                }
                else if (chord == "RT")
                {
                    // adjust scale
                    if (analog < 10) return; // deadzone
                    var d = (float)analog / 1000;
                    pp.s += d;
                    if (pp.s > 50) pp.s = 50;

                }
                else if (chord == "LT")
                {
                    // adjust scale
                    if (analog < 10) return; // deadzone
                    var d = (float)analog / 1000;
                    pp.s -= d;
                    if (pp.s < 0.1f) pp.s = 0.1f;
                }
            }
            else
            {
                //Debug.WriteLine($"?? {chord} / {analog}");
            }

            this.invalidate();
            PlotBase2.invalidate(targetName, nameof(PlotAdjustFake));
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            tt.setMinWidth(260);
            tt.flags |= TextFormatFlags.VerticalCenter;

            tt.drawCentered("Adjust VR Overlays");
            tt.newLine(N.twenty, true);

            if (selfClosing)
            {
                tt.dty -= N.ten;

                if (VR.app == null)
                {
                    tt.setMinWidth(tt.drawCentered("VR is not active", C.red).Width);
                    tt.newLine(N.ten, true);
                }
                if (!Game.settings.hookDirectX_TEST)
                {
                    tt.setMinWidth(tt.drawCentered("Please enable controller/joystick key chords", C.red).Width);
                    tt.newLine(N.ten, true);
                }

                if (Game.settings.hookDirectXDeviceId_TEST == Guid.Empty)
                {
                    tt.setMinWidth(tt.drawCentered("No controller or joystick selected", C.red).Width);
                    tt.newLine(N.ten, true);
                }

                tt.setMinWidth(tt.drawCentered($"This will self close after 5 seconds").Width);
                tt.newLine(N.ten, true);
                return tt.pad(N.twenty, N.ten);
            }

            // show confirmations
            if (confirmation != null)
            {
                tt.dty -= N.ten;
                if (confirmation == "exit")
                {
                    g.FillRectangle(Brushes.Navy, 2, tt.dty - N.four, this.width - 4, N.sixty);
                    tt.drawCentered("Save and exit?", C.cyan, GameColors.Fonts.gothic_16B);
                    tt.newLine(N.four, true);
                    tt.drawCentered("Press A to confirm?", C.cyanDark, GameColors.Fonts.gothic_10);
                    tt.newLine(N.ten, true);
                }
                else if (confirmation == "cancel")
                {
                    g.FillRectangle(Brushes.Navy, 2, tt.dty - N.four, this.width - 4, N.sixty);
                    tt.drawCentered("Exit without save?", C.cyan, GameColors.Fonts.gothic_16B);
                    tt.newLine(N.four, true);
                    tt.drawCentered("Press A to confirm?", C.cyanDark, GameColors.Fonts.gothic_10);
                    tt.newLine(N.ten, true);
                }
                else if (confirmation == "reset")
                {
                    g.FillRectangle(Brushes.Navy, 2, tt.dty - N.four, this.width - 4, N.sixty);
                    tt.drawCentered("Reset overlay?", C.cyan, GameColors.Fonts.gothic_16B);
                    tt.newLine(N.four, true);
                    tt.drawCentered("Press A to confirm?", C.cyanDark, GameColors.Fonts.gothic_10);
                    tt.newLine(N.ten, true);
                }
                tt.dty += N.ten;
            }

            // which overlay are we editing?
            tt.draw(N.ten, "Overlay: ", confirming ? C.orangeDark : null);
            tt.draw(targetName, confirming ? C.cyanDark : C.cyan, GameColors.Fonts.gothic_12B);
            tt.newLine(true);
            tt.draw(N.ten, "Override: ", confirming ? C.orangeDark : null, GameColors.Fonts.gothic_10);
            tt.draw(string.IsNullOrWhiteSpace(overrideName) ? "<none>" : overrideName, confirming ? C.cyanDark : C.cyan, GameColors.Fonts.gothic_10B);
            tt.newLine(true);
            tt.draw(N.sixty, "(Change with PoV < ^ >)", C.orangeDark, GameColors.Fonts.gothic_9);
            tt.newLine(N.twenty, true);

            // scale
            var c = confirming ? C.orangeDark : C.orange;
            tt.draw(N.ten, "Scale: ", c);
            tt.flags |= TextFormatFlags.VerticalCenter;

            tt.draw($"{pp.s:0.00}", c, GameColors.Fonts.gothic_12B);
            tt.newLine(N.twenty, true);

            // location
            c = confirming || rotate ? C.orangeDark : C.orange;
            tt.draw(N.ten, "Location: ", c);
            tt.newLine(N.four, true);
            tt.draw(40, $"X: {pp.p.X:0.0}", c, GameColors.Fonts.gothic_12B);
            tt.draw(120, $"Y: {pp.p.Y:0.0}", c, GameColors.Fonts.gothic_12B);
            tt.draw(200, $"Z: {pp.p.Z:0.0}", c, GameColors.Fonts.gothic_12B);
            tt.newLine(N.twenty, true);

            // rotation
            c = !confirming && rotate ? C.orange : C.orangeDark;
            tt.draw(N.ten, "Rotation: ", c);
            tt.newLine(N.four, true);
            tt.draw(40, $"X: {pp.r.X:0.0}", c, GameColors.Fonts.gothic_12B);
            tt.draw(100, $"Y: {pp.r.Y:0.0}", c, GameColors.Fonts.gothic_12B);
            tt.draw(160, $"Z: {pp.r.Z:0.0}", c, GameColors.Fonts.gothic_12B);
            tt.newLine(N.twenty, true);

            // show guidance
            tt.draw(N.ten, "LT or RT : adjust Scale", C.cyanDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);
            tt.draw(N.ten, "Axis: adjust location or rotation", C.cyanDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);
            tt.draw(N.ten, "Y : toggles location vs rotation", C.cyanDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);
            tt.draw(N.ten, "X : Save changes and exit", C.cyanDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);
            tt.draw(N.ten, "B : Exit without saving", C.cyanDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);
            tt.draw(N.ten, "X + B : Reset current overlay", C.cyanDark, GameColors.Fonts.gothic_9);
            tt.newLine(true);

            return tt.pad(N.ten, N.ten);
        }
    }

    internal class PlotAdjustFake : PlotBase2
    {
        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotAdjustFake),
            allowed = allowed,
            ctor = (game, def) => new PlotAdjustFake(game, def),
            defaultSize = new Size(300, 80),
        };

        public static bool allowed(Game game)
        {
            var foo = FormAdjustOverlay.targetName == def.name;
            return foo;
        }

        private PlotAdjustFake(Game game, PlotDef def) : base(game, def) { }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            tt.drawCentered(def.name, C.cyan, GameColors.Fonts.gothic_14B);
            return this.size;
        }
    }
}
