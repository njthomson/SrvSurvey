using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotBodyInfo;

namespace SrvSurvey.plotters
{
    [ApproxSize(320, 280)]
    internal class PlotBodyInfo : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.activeGame?.targetBody != null
                && Game.activeGame.systemData != null
                && Game.settings.autoShowPlotBodyInfo
                && !PlotGuardianSystem.allowPlotter // hide if Guardian plotter is open
                && (!Game.settings.autoHidePlotBodyInfoInBubble || Util.getSystemDistance(Game.activeGame.systemData.starPos, Util.sol) > Game.settings.bodyInfoBubbleSize)
                && (
                    // any time during DSS or ... 
                    (Game.activeGame.mode == GameMode.SAA && Game.activeGame.systemBody != null)
                    // ... or in the SystemMap and sub-setting allows
                    || (Game.activeGame.isMode(GameMode.SystemMap, GameMode.Orrery) && Game.settings.autoShowPlotBodyInfoInMap && !Game.settings.autoShowPlotFSSInfoInSystemMap)
                    // ... or when super cruising/gliding close to a body and sub-setting allows
                    || (Game.activeGame.isMode(GameMode.SuperCruising, GameMode.GlideMode) && Game.activeGame.status.hasLatLong && Game.settings.autoShowPlotBodyInfoInOrbit)
                    || (Game.activeGame.isMode(GameMode.Flying, GameMode.Landed, GameMode.InSrv) && Game.activeGame.status.hasLatLong && Game.settings.autoShowPlotBodyInfoAtSurface && Game.activeGame.status.hudInAnalysisMode)
                    // or a keystroke forced it
                    || (PlotBodyInfo.forceShow && !Game.activeGame.fsdJumping)
                );
        }

        /// <summary> When true, makes the plotter become visible IF there is a valid body to show </summary>
        public static bool forceShow = false;

        private string lastDestination;
        private bool withinHumanBubble;

        private PlotBodyInfo() : base()
        {
            this.Font = GameColors.fontSmall2;
            if (Game.activeGame?.systemData != null)
                this.withinHumanBubble = Util.getSystemDistance(Game.activeGame.systemData.starPos, Util.sol) < Game.settings.bodyInfoBubbleSize;
        }

        public override bool allow { get => PlotBodyInfo.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(320);
            this.Height = scaled(480);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            base.Status_StatusChanged(blink);

            var destination = $"{game.status.Destination?.System}/{game.status.Destination?.Body}/{game.status.Destination?.Name}";
            if (destination != this.lastDestination)
            {
                // re-render if destination has changed
                this.lastDestination = destination;
                this.Invalidate();
            }

            if (!this.allow)
                Program.closePlotter<PlotBodyInfo>(true);
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || game == null) return;

            // use current body, or targetBody if in SystemMap
            var body = Game.activeGame?.isMode(GameMode.SystemMap, GameMode.Orrery) == true || PlotBodyInfo.forceShow
                ? game.targetBody
                : game.systemBody;
            
            if (body == null || !PlotBodyInfo.allowPlotter)
            {
                Game.log($"Closing PlotBodyInfo due to no valid target body");
                this.setOpacity(0);
                Program.closePlotter<PlotBodyInfo>(true);
                return;
            }

            var temp = body.surfaceTemperature.ToString("N0");
            var gravity = (body.surfaceGravity / 10f).ToString("N3");

            // body name
            var bodyName = body.wasDiscovered
                ? body.name
                : $"⚑ {body.name}";
            drawTextAt2(bodyName, GameColors.fontMiddleBold);
            newLine(+2, true);
            var planetish = body.type != SystemBodyType.Star && body.type != SystemBodyType.Asteroid;

            if (body.type == SystemBodyType.Unknown)
            {
                // we don't know enough about this body - exit early
                drawTextAt2(eight, Res.ScanRequired, GameColors.Cyan);
                newLine(+2, true);
                formAdjustSize(+ten, +oneFour);
                return;
            }

            // terraformable, undiscovered?
            var subStatus = new List<string>();
            if (body.terraformable || body.planetClass?.StartsWith("Earth") == true)
                subStatus.Add("🌎 " + Res.Terraformable);
            if (!body.wasDiscovered && !body.wasMapped)
                subStatus.Add("⚑ " + Res.Undiscovered);
            else if (!body.wasMapped && body.dssComplete)
                subStatus.Add("✔️ " + Res.FirstMapped);
            else if (body.scanned && !body.wasMapped && !this.withinHumanBubble)
                subStatus.Add(Res.Unmapped);

            if (subStatus.Count > 0)
            {
                drawTextAt2(this.ClientSize.Width - ten, $"( {string.Join(", ", subStatus)} )", GameColors.Cyan, null, true);
                newLine(+2);
            }

            // reward
            var txt = Res.ScanValue + ": " + (body.dssComplete ? "✔️ " : "") + Util.credits(body.reward);
            var highlight = Game.settings.skipLowValueDSS && body.reward > Game.settings.skipLowValueAmount;
            if (!body.dssComplete && planetish)
            {
                var dssReward = Util.GetBodyValue(body, true, true);
                txt += Res.WithDSS.format(Util.credits(dssReward));
                highlight = Game.settings.skipLowValueDSS && dssReward > Game.settings.skipLowValueAmount;
            }
            drawTextAt2(eight, txt, highlight ? GameColors.Cyan : null);
            newLine(true);

            // temp | planetClass
            var tempText = Res.Temp.format(temp);
            var gravText = Res.Gravity.format(gravity);
            var indent1 = twenty + Util.maxWidth(this.Font, tempText, gravText);
            if (body.type != SystemBodyType.Asteroid)
            {
                dty += four;
                drawTextAt2(eight, tempText);
                drawTextAt2(indent1, body.type == SystemBodyType.Star ? Res.StarClass.format(body.starType) : body.planetClass!);
                newLine(+one, true);
            }

            // gravity | pressure
            if (planetish)
            {
                var isHighGravity = body.surfaceGravity >= Game.settings.highGravityWarningLevel * 10;
                // if (body.surfaceGravity > 2.69) gravity = "🚫 " + gravity; // show a warning icon if body gravity is too high to exit ships/SRV
                drawTextAt2(eight, gravText, isHighGravity ? GameColors.red : null);
                var pressure = Res.PressureValue.format((body.surfacePressure / 100_000f).ToString("N4"));
                if (pressure == Res.PressureValue.format(0.ToString("N4"))) pressure = Res.None;
                if (pressure != Res.None || body.type == SystemBodyType.LandableBody)
                    drawTextAt2(indent1, Res.Pressure.format(pressure));
                newLine(+four, true);
            }

            // bio signals
            if (body.bioSignalCount > 0)
            {
                dty -= three;
                drawTextAt2(eight, Res.BioSignals.format(body.bioSignalCount, body.getMinMaxBioRewards(false)), GameColors.Cyan);
                newLine(+four, true);
            }

            // geo signals
            if (body.geoSignalCount > 0)
            {
                dty -= three;
                drawTextAt2(eight, Res.GeoSignals.format(body.geoSignalCount), GameColors.Cyan);
                newLine(+four, true);
            }

            var indent2 = ten + Util.maxWidth(this.Font, Res.Volcanism, Res.Atmosphere, Res.Materials);

            // volcanism
            if (planetish && body.type != SystemBodyType.Giant)
            {
                drawTextAt2(eight, Res.Volcanism);

                if (string.IsNullOrEmpty(body.volcanism) || body.volcanism == "No volcanism")
                    drawTextAt2(indent2, Res.None);
                else
                    drawTextAt2(indent2, Util.pascal(body.volcanism.Replace("volcanism", "")));
                newLine(+four, true);
            }

            // atmosphere
            if (planetish)
            {
                var atmos = string.IsNullOrEmpty(body.atmosphere) || body.atmosphere == "No atmosphere" ? Res.None : Util.pascal(body.atmosphere.Replace(" atmosphere", ""));
                if (body.atmosphereType == "EarthLike") atmos = Res.EarthLike;
                if (atmos == "None" && body.type != SystemBodyType.LandableBody) atmos = " ";
                drawTextAt2(eight, Res.Atmosphere);
                drawTextAt2(indent2, atmos);
                newLine(+two, true);
                if (body.atmosphereComposition != null)
                {
                    foreach (var atm in body.atmosphereComposition)
                    {
                        var name = char.ToUpperInvariant(atm.Key[0]) + atm.Key.Substring(1);
                        var value = atm.Value.ToString("N2").PadLeft(5);
                        drawTextAt2(fourFour, $"{name}:");
                        drawTextAt2(indent2 + oneForty, $"{value} %", null, null, true);
                        newLine(true);
                    }
                }
                dty += six;

                // materials
                if (body.materials != null)
                {
                    drawTextAt2(eight, Res.Materials);
                    foreach (var mat in body.materials.OrderByDescending(_ => _.Value))
                    {
                        var name = Util.pascal(mat.Key);
                        var value = mat.Value.ToString("N2").PadLeft(5);

                        var font = Util.isMatLevelThree(mat.Key) || Util.isMatLevelFour(mat.Key) ? GameColors.fontSmall2Bold : null;
                        drawTextAt2(indent2, $"{name}:", font);
                        drawTextAt2(indent2 + oneForty, $"{value} %", null, font, true);
                        //if (Util.isMatLevelThree(mat.Key) || Util.isMatLevelFour(mat.Key)) drawTextAt2($"≡");
                        newLine(true);
                    }
                }
                dty += four;
            }

            // star stuff?
            if (body.starType != null)
            {
                // ?
            }

            // rings
            if (body.hasRings)
            {
                drawTextAt2(eight, Res.Rings);
                foreach (var ring in body.rings)
                {
                    var ringName = ring.name.Substring(body.name.Length + 1, 1);
                    drawTextAt2(eighty, $"{ringName} - " + SystemRing.decode(ring.ringClass));
                    newLine(true);
                }
            }

            // resize window as necessary
            formAdjustSize(+ten, +oneFour);
        }
    }
}
