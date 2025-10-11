using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Numerics;

namespace SrvSurvey.plotters
{
    internal class PlotAdjustVR : PlotBase2
    {
        #region def + statics

        public static PlotDef plotDef = new PlotDef()
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

        public static void show()
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

        private PlotAdjustVR(Game game, PlotDef def) : base(game, def)
        {
            font = GameColors.Fonts.gothic_12;

            allNames = PlotPos.getAllPlotterNames().Order().ToList();
            targetName = allNames.First();
            FormAdjustOverlay.targetName = targetName;
            pp = getPP();

            KeyboardHook.analogs = true;
            KeyboardHook.redirect = true;
            KeyboardHook.buttonsPressed += KeyboardHook_buttonsPressed;
        }

        protected override void onClose()
        {
            base.onClose();
            PlotBase2.remove(PlotAdjustFake.plotDef);
            PlotAdjustFake.plotDef.name = nameof(PlotAdjustFake);

            KeyboardHook.buttonsPressed -= KeyboardHook_buttonsPressed;
            KeyboardHook.redirect = false;
            KeyboardHook.analogs = false;
            FormAdjustOverlay.targetName = null;
        }

        private PlotPos.VR getPP()
        {
            var pPos = PlotPos.get(targetName);
            if (pPos!.vr == null)
            {
                pPos.vr = new() { s = 10, p = new Vector3(-8, 10, 45), r = new Vector3() };
            }
            return pPos.vr;
        }

        private void setNextOverlay(string nextTarget)
        {
            // remove fake?
            PlotBase2.remove(PlotAdjustFake.plotDef);

            // change name here, then invalidate old one (removing the yellow box)
            FormAdjustOverlay.targetName = nextTarget;
            PlotBase2.invalidate(targetName);

            targetName = nextTarget;
            pp = getPP();

            var targetDef = PlotBase2.get(nextTarget);
            if (targetDef != null && targetDef.instance == null)
            {
                // we need a fake to stand
                PlotAdjustFake.plotDef.name = nextTarget;
                PlotAdjustFake.plotDef.defaultSize = PlotPos.typicalSize.ContainsKey(nextTarget) ? PlotPos.typicalSize[nextTarget] : new Size(200, 100);
                var instance = PlotBase2.add(game, PlotAdjustFake.plotDef);
                PlotBase2.renderAll(game);
            }
        }

        private void KeyboardHook_buttonsPressed(bool hook, string chord, short analog)
        {
            // only process when buttons are released
            if (!hook) return;

            if (chord == "B1")
            {
                if (confirmation == "exit")
                {
                    // save changes and exit
                    PlotPos.saveCustomPositions();
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
                        pp.p.X += (float)analog / 100_000;
                }
                else if (chord == "RY")
                {
                    if (analog > -4000 && analog < 4000) return; // deadzone
                    if (rotate)
                        pp.r.X += (float)analog / 50_000;
                    else
                        pp.p.Y += (float)analog / 100_000;
                }
                else if (chord == "LY")
                {
                    if (analog > -4000 && analog < 4000) return; // deadzone
                    if (rotate)
                        pp.r.Z += (float)analog / 50_000;
                    else
                        pp.p.Z += (float)analog / 100_000;
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
                    if (pp.s < 2) pp.s = 2;
                }
            }
            else
            {
                //Debug.WriteLine($"?? {chord} / {analog}");
            }

            this.invalidate();
            PlotBase2.invalidate(targetName, nameof(PlotAdjustFake));
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            tt.flags |= TextFormatFlags.VerticalCenter;

            tt.drawCentered("Adjust VR Overlays");
            tt.newLine(N.twenty, true);


            // show confirmations
            if (confirmation != null)
            {
                tt.dty -= N.ten;
                if (confirmation == "exit")
                {
                    tt.drawCentered("Save and exit?", C.cyan, GameColors.Fonts.gothic_16B);
                    tt.newLine(N.four, true);
                    tt.drawCentered("Press A to confirm?", C.cyanDark, GameColors.Fonts.gothic_10);
                    tt.newLine(N.ten, true);
                }
                else if (confirmation == "cancel")
                {
                    tt.drawCentered("Exit without save?", C.cyan, GameColors.Fonts.gothic_16B);
                    tt.newLine(N.four, true);
                    tt.drawCentered("Press A to confirm?", C.cyanDark, GameColors.Fonts.gothic_10);
                    tt.newLine(N.ten, true);
                }
                else if (confirmation == "reset")
                {
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
            tt.draw(N.sixty, "(Change with PoV < >)", C.orangeDark, GameColors.Fonts.gothic_9);
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
        public static PlotDef plotDef = new PlotDef()
        {
            name = nameof(PlotAdjustFake),
            allowed = allowed,
            ctor = (game, def) => new PlotAdjustFake(game, def),
            defaultSize = new Size(300, 80),
        };

        public static bool allowed(Game game)
        {
            var foo = FormAdjustOverlay.targetName == plotDef.name;
            return foo;
        }

        private PlotAdjustFake(Game game, PlotDef def) : base(game, def) { }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            tt.drawCentered(plotDef.name, C.cyan, GameColors.Fonts.gothic_14B);
            return this.size;
        }
    }
}
