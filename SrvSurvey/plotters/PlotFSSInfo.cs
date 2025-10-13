using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotFSSInfo;

namespace SrvSurvey.plotters
{
    internal class PlotFSSInfo : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotFSSInfo),
            allowed = allowed,
            ctor = (game, def) => new PlotFSSInfo(game, def),
            defaultSize = new Size(300, 400),
            invalidationJournalEvents = new() { nameof(FSSAllBodiesFound), }
        };

        public static bool allowed(Game game)
        {
            var guardianSystemDisabled = !Game.settings.enableGuardianSites && !Game.settings.autoShowGuardianSummary;
            return Game.settings.autoShowPlotFSSInfo
                && (
                    game.mode == GameMode.FSS
                    || (PlotFSSInfo.forceShow && !game.fsdJumping) // or a keystroke forced it
                    || (game.mode == GameMode.SystemMap && Game.settings.autoShowPlotFSSInfoInSystemMap && (guardianSystemDisabled || !PlotGuardianSystem.allowed(game))) // hide if Guardian plotter is open
                );
        }

        #endregion

        /// <summary> When true, makes the plotter become visible </summary>
        public static bool forceShow = false;

        public static List<FssEntry> scans = new List<FssEntry>();
        private static string? systemName;

        private PlotFSSInfo(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall2;

            if (systemName != game?.systemData?.name)
            {
                // if we've switched systems, clear and add in any stars
                PlotFSSInfo.scans.Clear();
            }

            // add any bodies discovered whilst outside of FSS
            if (game?.systemData?.bodies.Count > 0)
            {
                var bodies = game.systemData.bodies.OrderBy(_ => _.shortName);
                foreach (var body in bodies)
                {
                    if (body.type == SystemBodyType.Asteroid || scans.Any(_ => _.body == body)) continue;
                    var isInteresting = body.terraformable || body.planetClass?.StartsWith("Water ") == true || body.planetClass?.StartsWith("Ammonia ") == true || body.planetClass?.StartsWith("Earthlike ") == true;
                    if (!isInteresting && body.bioSignalCount == 0 && (body.geoSignalCount == 0 || Game.settings.hideGeoCountInFssInfo) && body.reward < Game.settings.hideFssLowValueAmount) continue;

                    var newScan = new FssEntry
                    {
                        body = body,
                        reward = body.reward,
                        dssReward = Util.GetBodyValue(body, true, false),
                    };
                    scans.Add(newScan);
                }
            }

            systemName = game?.systemData?.name;
        }

        protected override void onJournalEntry(Scan entry)
        {
            // ignore Belt Clusters
            if (entry.BodyName.Contains("Belt Cluster") || game?.systemData == null)
                return;

            // allow time for the body to get created
            Application.DoEvents();
            if (game?.systemData == null) return;

            var body = game.systemData.bodies.Find(_ => _.id == entry.BodyID)!;
            var newScan = new FssEntry
            {
                body = body,
                reward = Util.GetBodyValue(entry, false),
                dssReward = Util.GetBodyValue(entry, true),
            };

            var isInteresting = body.terraformable || body.planetClass?.StartsWith("Water ") == true || body.planetClass?.StartsWith("Ammonia ") == true || body.planetClass?.StartsWith("Earthlike ") == true;
            if (!isInteresting && body.bioSignalCount == 0 && (body.geoSignalCount == 0 || Game.settings.hideGeoCountInFssInfo) && Math.Max(newScan.reward, newScan.dssReward) < Game.settings.hideFssLowValueAmount) return;

            // show this body already present - pull it to the top
            var existingScan = scans.Find(s => s.body == body);
            if (existingScan != null)
                scans.Remove(existingScan);

            scans.Insert(0, newScan);

            this.invalidate();
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (game.systemData == null) return frame.Size;

            // 1st line: system name
            var undiscoveredSystem = game.systemData.bodies.Find(b => b.isMainStar)?.wasDiscovered == false;
            var systemName = undiscoveredSystem
                ? $"⚑ {game.systemData.name}"
                : game.systemData.name;
            if (game.systemData.fssAllBodies) systemName += " ✔️";
            tt.draw(systemName, undiscoveredSystem ? GameColors.Cyan : null, GameColors.Fonts.gothic_12B);
            tt.newLine(+N.eight, true);

            // 2nd line body count / values
            var bodyCount = game.systemData.bodies.Count(_ => _.scanned && _.type != SystemBodyType.Asteroid).ToString();
            if (game.systemData.fssAllBodies)
                tt.draw(N.eight, Res.ScannedAllBodies.format(bodyCount, Util.credits(game.systemData.sumRewards())));
            else
                tt.draw(N.eight, Res.ScannedBodies.format(bodyCount, Util.credits(game.systemData.sumRewards())));

            tt.newLine(true);
            tt.draw(N.eight, Res.HidingBodies.format(Util.credits(Game.settings.hideFssLowValueAmount)), GameColors.OrangeDim);
            tt.newLine(+N.eight, true);

            if (scans.Count == 0)
            {
                tt.draw(N.eight, Res.ScanToPopulate);
                tt.newLine(true);
            }

            foreach (var scan in scans)
            {
                var hasBioSignals = scan.body.bioSignalCount > 0;

                // highlight if DSS value above threshold
                var dssWorthy = Game.settings.skipLowValueDSS && scan.dssReward > Game.settings.skipLowValueAmount;
                // unless it's too far away
                if (dssWorthy && Game.settings.skipHighDistanceDSS && scan.body.distanceFromArrivalLS > Game.settings.skipHighDistanceDSSValue)
                    dssWorthy = false;
                // or a gas giant
                if (dssWorthy && Game.settings.skipGasGiantDSS && scan.body.type == SystemBodyType.Giant)
                    dssWorthy = false;
                if (scan.body.type == SystemBodyType.Star)
                    dssWorthy = false;

                var highlight = dssWorthy | hasBioSignals;

                // 1st line: body name + type
                var planetClass = scan.body.planetClass?.Replace("Sudarsky c", "C");
                var prefix = scan.body.wasDiscovered ? "" : "⚑ ";
                var txt = $"{prefix}{scan.body.shortName} - {planetClass}"; // ◌◎◉☆★☄☼☀⛀⛃✔✨✶✪❓❔❓⛬❗❕ * ❒❱✪❍❌✋❖⟡⦁⦂⧫⇲
                var suffixes = new List<string>();
                if (scan.body.terraformable || scan.body.planetClass?.StartsWith("Earth") == true) suffixes.Add("🌎");
                if (scan.body.type == SystemBodyType.LandableBody) suffixes.Add("🚀");
                if (scan.body.firstFootFall) suffixes.Add("🦶");
                if (suffixes.Count > 0) txt += $" {string.Join(' ', suffixes)}";
                if (scan.body.type == SystemBodyType.Star)
                    txt = $"{prefix}{scan.body.shortName} - {scan.body.starType} Star";

                tt.draw(N.oneSix, txt, highlight ? GameColors.Cyan : null, GameColors.fontSmall2);
                tt.newLine(true);

                // 2nd line: scan values
                var reward = (scan.body.dssComplete ? "✔️ " : "") + Util.credits(scan.body.reward, true); // scan.reward ?

                tt.draw(N.thirty, reward, dssWorthy ? GameColors.Cyan : null);
                if (scan.body.type != SystemBodyType.Star && !scan.body.dssComplete)
                {
                    tt.draw(" | ", GameColors.OrangeDim);
                    tt.draw(scan.dssReward.ToString("N0"), dssWorthy ? GameColors.Cyan : null);
                }

                if (scan.body.bioSignalCount > 0)
                {
                    tt.draw(" | ", GameColors.OrangeDim);
                    var analyzed = scan.body.countAnalyzedBioSignals == scan.body.bioSignalCount;
                    var sz = tt.draw(Res.CountGenus.format(scan.body.bioSignalCount), analyzed ? GameColors.Orange : GameColors.Cyan);
                    if (analyzed)
                        tt.strikeThroughLast();
                }

                if (!Game.settings.hideGeoCountInFssInfo && scan.body.geoSignalCount > 0)
                {
                    tt.draw(" | ", GameColors.OrangeDim);
                    var analyzed = scan.body.geoSignalNames.Count == scan.body.geoSignalCount;
                    var sz = tt.draw(Res.CountGeo.format(scan.body.geoSignalCount), analyzed ? GameColors.Orange : GameColors.Cyan);
                    if (analyzed)
                        tt.strikeThroughLast();

                }

                tt.newLine(+N.four, true);

                // stop if we are starting to overlap any other plotter (allowing 100px of buffer)
                var thisRect = new Rectangle(left, top, (int)tt.frameSize.Width + 100, (int)tt.frameSize.Height + 100);
                var colliding = PlotBase2.active.Any(other => other != this && other.rect.IntersectsWith(thisRect));
                if (colliding) break;
            }

            tt.draw(N.eight, Res.Footer1, GameColors.OrangeDim);
            tt.newLine(true);
            tt.draw(N.eight, Res.Footer2, GameColors.OrangeDim);
            tt.newLine(true);

            return tt.pad(+N.oneEight, +N.ten);
        }

    }

    class FssEntry
    {
        public SystemBody body;
        public long reward;
        public long dssReward;
    }
}
