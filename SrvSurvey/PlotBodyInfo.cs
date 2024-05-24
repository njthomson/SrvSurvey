using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotBodyInfo : PlotBase, PlotterForm
    {
        private string lastDestination;

        private PlotBodyInfo() : base()
        {
            this.Width = scaled(320);
            this.Height = scaled(88);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Font = GameColors.fontSmall2;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        public static bool allowPlotter
        {
            get => Game.activeGame?.targetBody != null
                && Game.settings.autoShowPlotBodyInfoTest
                && !PlotGuardianSystem.allowPlotter // hide if Guardian plotter is open
                && (
                    // any time during DSS or ... 
                    Game.activeGame.mode == GameMode.SAA
                    // ... or in the SystemMap and sub-setting allows
                    || (Game.activeGame.isMode(GameMode.SystemMap, GameMode.Orrery) && Game.settings.autoShowPlotBodyInfoInMapTest)
                    // ... or when super cruising/gliding close to a body and sub-setting allows
                    || (Game.activeGame.isMode(GameMode.SuperCruising, GameMode.GlideMode) && Game.activeGame.status.hasLatLong && Game.settings.autoShowPlotBodyInfoInOrbitTest)
                );
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

            this.Invalidate();
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !PlotBodyInfo.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotBodyInfo.allowPlotter)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            base.Status_StatusChanged(blink);

            var destination = $"{game.status.Destination?.System}/{game.status.Destination?.Body}/{game.status.Destination?.Name}";
            if (destination != this.lastDestination)
            {
                // re-render if destination has changed
                this.lastDestination = destination;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed || game == null) return;

            // use current body, or targetBody if in SystemMap
            var body = Game.activeGame?.isMode(GameMode.SystemMap, GameMode.Orrery) == true
                ? game.targetBody
                : game.systemBody;
            if (body == null || !PlotBodyInfo.allowPlotter)
            {
                Game.log($"Hiding PlotBodyInfo - no valid target");
                this.Opacity = 0;
                return;
            }

            var temp = body.surfaceTemperature.ToString("N0");
            var gravity = (body.surfaceGravity / 10f).ToString("N2");

            try
            {
                this.resetPlotter(e.Graphics);

                // body name
                drawTextAt($"{body.name}", GameColors.fontMiddleBold);
                newLine(+2, true);
                var planetish = body.type != SystemBodyType.Star && body.type != SystemBodyType.Asteroid;

                if (body.type == SystemBodyType.Unknown)
                {
                    // we don't know enough about this body - exit early
                    drawTextAt(eight, "Scan required", GameColors.brushCyan);
                    newLine(+2, true);
                    formAdjustSize(+ten, +oneFour);
                    return;
                }

                // terraformable, undisdiscovered?
                var subStatus = new List<string>();
                if (body.terraformable)
                    subStatus.Add("Terraformable");
                if (!body.wasDiscovered)
                    subStatus.Add("Undiscovered");
                else if (!body.wasMapped)
                    subStatus.Add("Unmapped");

                if (subStatus.Count > 0)
                {
                    drawTextAt(this.ClientSize.Width - ten, $"( {string.Join(", ", subStatus)} )", GameColors.brushCyan, null, true);
                    newLine(+2);
                }

                // reward
                var txt = $"Scan value: {Util.credits(body.reward)}";
                var highlight = Game.settings.skipLowValueDSS && body.reward > Game.settings.skipLowValueAmount;
                if (!body.dssComplete && planetish)
                {
                    var dssReward = Util.GetBodyValue(body, true, true);
                    txt += $" (with DSS: {Util.credits(dssReward)})";
                    highlight = Game.settings.skipLowValueDSS && dssReward > Game.settings.skipLowValueAmount;
                }
                drawTextAt(eight, txt, highlight ? GameColors.brushCyan : null);
                newLine(true);

                // temp | planetClass
                if (body.type != SystemBodyType.Asteroid)
                {
                    dty += four;
                    drawTextAt(eight, $"Temp: {temp}°K");
                    drawTextAt(oneTwenty, $" | {body.planetClass}");
                    newLine(true);
                }

                // gravity | pressure
                if (planetish)
                {
                    var isHighGravity = body.surfaceGravity >= Game.settings.highGravityWarningLevel * 10;
                    drawTextAt(eight, $"Gravity: {gravity}g", isHighGravity ? GameColors.brushRed : null);
                    var pressure = (body.surfacePressure / 100_000f).ToString("N2") + "(atm)";
                    if (pressure == "0.00(atm)") pressure = "None";
                    drawTextAt(oneTwenty, $" | Pressure: {pressure}");
                    newLine(+four, true);
                }

                // bio signals
                if (body.bioSignalCount > 0)
                {
                    dty -= four;
                    drawTextAt(eight, $"Bio signals: {body.bioSignalCount}", GameColors.brushCyan);
                    drawTextAt($"( value: {body.minMaxBioRewards} cr )", GameColors.brushCyan);
                    newLine(+four, true);
                }

                // geo signals ?

                // volcanism
                if (planetish && body.type != SystemBodyType.Giant)
                {
                    drawTextAt(eight, $"Volcanism:");

                    if (string.IsNullOrEmpty(body.volcanism) || body.volcanism == "No volcanism")
                        drawTextAt(eightEight, $"None");
                    else
                        drawTextAt(eightEight, Util.camel(body.volcanism.Replace("volcanism", "")));
                    newLine(+four, true);
                }

                // atmosphere
                if (planetish)
                {
                    var atmos = string.IsNullOrEmpty(body.atmosphere) || body.atmosphere == "No atmosphere" ? "None" : Util.camel(body.atmosphere.Replace(" atmosphere", ""));
                    drawTextAt(eight, $"Atmosphere:");
                    drawTextAt(eightEight, atmos);
                    newLine(+two, true);
                    if (body.atmosphereComposition != null)
                    {
                        foreach (var atm in body.atmosphereComposition)
                        {
                            var name = char.ToUpperInvariant(atm.Key[0]) + atm.Key.Substring(1);
                            var value = atm.Value.ToString("N2").PadLeft(5);
                            drawTextAt(fourFour, $"{name}:");
                            drawTextAt(twoThirty, $"{value} %", null, null, true);
                            newLine(true);
                        }
                    }
                    dty += four;

                    // materials
                    if (body.materials != null)
                    {
                        drawTextAt(eight, $"Materials:");
                        foreach (var mat in body.materials.OrderByDescending(_ => _.Value))
                        {
                            var name = Util.camel(mat.Key);
                            var value = mat.Value.ToString("N2").PadLeft(5);

                            var font = Util.isMatLevelThree(mat.Key) || Util.isMatLevelFour(mat.Key) ? GameColors.fontSmall2Bold : null;
                            drawTextAt(eightEight, $"{name}:", font);
                            drawTextAt(twoThirty, $"{value} %", null, font, true);
                            //if (Util.isMatLevelThree(mat.Key) || Util.isMatLevelFour(mat.Key)) drawTextAt($"≡");
                            newLine(true);
                        }
                    }
                    dty += four;
                }

                // rings
                if (body.hasRings)
                {
                    drawTextAt(eight, $"Rings:");
                    foreach (var ring in body.rings)
                    {
                        var ringName = ring.name.Substring(body.name.Length + 1, 1);
                        drawTextAt(eighty, $"{ringName} - " + SystemRing.decode(ring.ringClass));
                        newLine(true);
                    }
                }

                // resize window as necessary
                formAdjustSize(+ten, +oneFour);
            }
            catch (Exception ex)
            {
                Game.log($"PlotGalMap.OnPaintBackground error: {ex}");
            }
        }

    }

}
