using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotBodyInfo;

namespace SrvSurvey.plotters
{
    internal class PlotBodyInfo : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotBodyInfo),
            allowed = allowed,
            ctor = (game, def) => new PlotBodyInfo(game, def),
            defaultSize = new Size(320, 280),
        };

        public static bool allowed(Game game)
        {
            var guardianSystemDisabled = !Game.settings.enableGuardianSites && !Game.settings.autoShowGuardianSummary;
            return game.targetBody != null
                && game.systemData != null
                && Game.settings.autoShowPlotBodyInfo
                && (guardianSystemDisabled || !PlotGuardianSystem.allowed(game))
                && (!Game.settings.autoHidePlotBodyInfoInBubble || Util.getSystemDistance(game.systemData.starPos, Util.sol) > Game.settings.bodyInfoBubbleSize)
                && (
                    // any time during DSS or ... 
                    (game.mode == GameMode.SAA && game.systemBody != null)
                    // ... or in the SystemMap and sub-setting allows
                    || (game.isMode(GameMode.SystemMap, GameMode.Orrery) && Game.settings.autoShowPlotBodyInfoInMap && !Game.settings.autoShowPlotFSSInfoInSystemMap)
                    // ... or when super cruising/gliding close to a body and sub-setting allows
                    || (game.isMode(GameMode.SuperCruising, GameMode.GlideMode) && game.status.hasLatLong && Game.settings.autoShowPlotBodyInfoInOrbit)
                    || (game.isMode(GameMode.Flying, GameMode.Landed, GameMode.InSrv) && game.status.hasLatLong && Game.settings.autoShowPlotBodyInfoAtSurface && game.status.hudInAnalysisMode)
                    // or a keystroke forced it
                    || (PlotBodyInfo.forceShow && !game.fsdJumping)
                );
        }

        #endregion

        /// <summary> When true, makes the plotter become visible IF there is a valid body to show </summary>
        public static bool forceShow = false;

        private string lastDestination;
        private bool withinHumanBubble;

        private PlotBodyInfo(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall2;
            if (Game.activeGame?.systemData != null)
                this.withinHumanBubble = Util.getSystemDistance(Game.activeGame.systemData.starPos, Util.sol) < Game.settings.bodyInfoBubbleSize;
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            var destination = $"{game.status.Destination?.System}/{game.status.Destination?.Body}/{game.status.Destination?.Name}";
            if (destination != this.lastDestination)
            {
                // re-render if destination has changed
                this.lastDestination = destination;
                this.invalidate();
            }
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            // use current body, or targetBody if in SystemMap
            var body = game.isMode(GameMode.SystemMap, GameMode.Orrery) || PlotBodyInfo.forceShow
                ? game.targetBody
                : game.systemBody;
            
            if (body == null)
            {
                Game.log($"Closing PlotBodyInfo due to no valid target body");
                this.hide();
                return frame.Size;
            }

            var temp = body.surfaceTemperature.ToString("N0");
            var gravity = (body.surfaceGravity / 10f).ToString("N3");

            // body name
            var bodyName = body.wasDiscovered
                ? body.name
                : $"⚑ {body.name}";
            tt.draw(bodyName, GameColors.fontMiddleBold);
            tt.newLine(+2, true);
            var planetish = body.type != SystemBodyType.Star && body.type != SystemBodyType.Asteroid && body.type != SystemBodyType.PlanetaryRing;

            if (body.type == SystemBodyType.Unknown)
            {
                // we don't know enough about this body - exit early
                tt.draw(N.eight, Res.ScanRequired, GameColors.Cyan);
                tt.newLine(+2, true);
                return tt.pad(+N.ten, +N.oneFour);
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
                tt.draw(this.width - N.ten, $"( {string.Join(", ", subStatus)} )", GameColors.Cyan, null, true);
                tt.newLine(+2);
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
            tt.draw(N.eight, txt, highlight ? GameColors.Cyan : null);
            tt.newLine(true);

            // temp | planetClass
            var tempText = Res.Temp.format(temp);
            var gravText = Res.Gravity.format(gravity);
            var indent1 = N.twenty + Util.maxWidth(this.font, tempText, gravText);
            if (body.type != SystemBodyType.Asteroid)
            {
                tt.dty += N.four;
                tt.draw(N.eight, tempText);
                tt.draw(indent1, body.type == SystemBodyType.Star ? Res.StarClass.format(body.starType) : body.planetClass!);
                tt.newLine(+N.one, true);
            }

            // gravity | pressure
            if (planetish)
            {
                var isHighGravity = body.surfaceGravity >= Game.settings.highGravityWarningLevel * 10;
                // if (body.surfaceGravity > 2.69) gravity = "🚫 " + gravity; // show a warning icon if body gravity is too high to exit ships/SRV
                tt.draw(N.eight, gravText, isHighGravity ? GameColors.red : null);
                var pressure = Res.PressureValue.format((body.surfacePressure / 100_000f).ToString("N4"));
                if (pressure == Res.PressureValue.format(0.ToString("N4"))) pressure = Res.None;
                if (pressure != Res.None || body.type == SystemBodyType.LandableBody)
                    tt.draw(indent1, Res.Pressure.format(pressure));
                tt.newLine(+N.four, true);
            }

            // bio signals
            if (body.bioSignalCount > 0)
            {
                tt.dty -= N.three;
                tt.draw(N.eight, Res.BioSignals.format(body.bioSignalCount, body.getMinMaxBioRewards(false)), GameColors.Cyan);
                tt.newLine(+N.four, true);
            }

            // geo signals
            if (body.geoSignalCount > 0)
            {
                tt.dty -= N.three;
                tt.draw(N.eight, Res.GeoSignals.format(body.geoSignalCount), GameColors.Cyan);
                tt.newLine(+N.four, true);
            }

            var indent2 = N.ten + Util.maxWidth(this.font, Res.Volcanism, Res.Atmosphere, Res.Materials);

            // volcanism
            if (planetish && body.type != SystemBodyType.Giant)
            {
                tt.draw(N.eight, Res.Volcanism);

                if (string.IsNullOrEmpty(body.volcanism) || body.volcanism == "No volcanism")
                    tt.draw(indent2, Res.None);
                else
                    tt.draw(indent2, Util.pascal(body.volcanism.Replace("volcanism", "")));
                tt.newLine(+N.four, true);
            }

            // atmosphere
            if (planetish)
            {
                var atmos = string.IsNullOrEmpty(body.atmosphere) || body.atmosphere == "No atmosphere" ? Res.None : Util.pascal(body.atmosphere.Replace(" atmosphere", ""));
                if (body.atmosphereType == "EarthLike") atmos = Res.EarthLike;
                if (atmos == "None" && body.type != SystemBodyType.LandableBody) atmos = " ";
                tt.draw(N.eight, Res.Atmosphere);
                tt.draw(indent2, atmos);
                tt.newLine(+N.two, true);
                if (body.atmosphereComposition != null)
                {
                    foreach (var atm in body.atmosphereComposition)
                    {
                        var name = char.ToUpperInvariant(atm.Key[0]) + atm.Key.Substring(1);
                        var value = atm.Value.ToString("N2").PadLeft(5);
                        tt.draw(N.fourFour, $"{name}:");
                        tt.draw(indent2 + N.oneForty, $"{value} %", null, null, true);
                        tt.newLine(true);
                    }
                }
                tt.dty += N.six;

                // materials
                if (body.materials != null)
                {
                    tt.draw(N.eight, Res.Materials);
                    foreach (var mat in body.materials.OrderByDescending(_ => _.Value))
                    {
                        var name = Util.pascal(mat.Key);
                        var value = mat.Value.ToString("N2").PadLeft(5);

                        var font = Util.isMatLevelThree(mat.Key) || Util.isMatLevelFour(mat.Key) ? GameColors.fontSmall2Bold : null;
                        tt.draw(indent2, $"{name}:", font!);
                        tt.draw(indent2 + N.oneForty, $"{value} %", null, font, true);
                        //if (Util.isMatLevelThree(mat.Key) || Util.isMatLevelFour(mat.Key)) tt.draw($"≡");
                        tt.newLine(true);
                    }
                }
                tt.dty += N.four;
            }

            // star stuff?
            if (body.starType != null)
            {
                // ?
            }

            // rings
            if (body.hasRings)
            {
                tt.draw(N.eight, Res.Rings);
                foreach (var ring in body.rings)
                {
                    var ringName = ring.name.Substring(body.name.Length + 1, 1);
                    tt.draw(N.eighty, $"{ringName} - " + SystemRing.decode(ring.ringClass));
                    tt.newLine(true);
                }
            }

            // resize window as necessary
            return tt.pad(+N.ten, +N.oneFour);
        }
    }
}
