using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotSysStatus;

namespace SrvSurvey.plotters
{
    internal class PlotSysStatus : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotSysStatus),
            allowed = allowed,
            ctor = (game, def) => new PlotSysStatus(game, def),
            defaultSize = new Size(170, 40),
            invalidationJournalEvents = new() { nameof(FSSBodySignals), nameof(FSSDiscoveryScan), nameof(Scan), },
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotSysStatus
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && game.status?.InTaxi != true
                // show only after honking or we have Canonn data
                && game.systemData != null
                && (game.systemData.honked || game.canonnPoi != null)
                && game.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                ;
        }


        #endregion

        private Font boldFont = GameColors.fontMiddleBold;

        private PlotSysStatus(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontMiddle;
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (status.changed.Contains(nameof(Status.Destination)))
                this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            var minViableWidth = N.s(170);
            if (game?.systemData == null || game.status == null) // still needed --> || !PlotSysStatus.allowed(game))
            {
                this.hide();
                return frame.Size;
            }

            tt.dty = N.eight;
            tt.draw(N.six, Res.Header, GameColors.fontSmall);
            tt.newLine(0, true);
            tt.dtx = N.six;

            // reduce destination to it's short name
            var destinationBody = game.status.Destination?.Name?.Replace(game.systemData.name, "").Replace(" ", "");

            var dssRemaining = game.systemData.getDssRemainingNames();
            if (!game.systemData.honked)
            {
                tt.draw(Res.FssNotStarted, GameColors.Cyan);
            }
            else if (!game.systemData.fssComplete)
            {
                var fssProgress = 100.0 / (float)game.systemData.bodyCount * (float)game.systemData.fssBodyCount;
                var txt = dssRemaining.Count == 0
                    ? Res.FssCompleteLong.format((int)fssProgress)
                    : Res.FssCompleteShort.format((int)fssProgress);
                tt.draw(txt, GameColors.Cyan);
            }

            if (dssRemaining.Count > 0)
            {
                if (tt.dtx > 6) tt.draw(" ");
                tt.draw(Res.DssRemaining.format(dssRemaining.Count));
                this.drawRemainingBodies(g, tt, destinationBody, dssRemaining);
            }
            else if (game.systemData.fssComplete && game.systemData.honked)
            {
                tt.draw(Res.NoDssMeet);
            }
            tt.newLine(true);

            if (!Game.settings.autoShowPlotBioSystem)
            {
                var bioRemaining = game.systemData.getBioRemainingNames();
                if (bioRemaining.Count > 0)
                {
                    tt.draw(Res.BioSignals.format(game.systemData.bioSignalsRemaining));
                    this.drawRemainingBodies(g, tt, destinationBody, bioRemaining);
                }
            }

            var nonBodySignalCount = game.systemData.nonBodySignalCount;
            if (Game.settings.showNonBodySignals && nonBodySignalCount > 0)
            {
                var sz = tt.draw(N.six, Res.NonBodySignals.format(nonBodySignalCount), GameColors.fontSmall2);
                tt.newLine(true);
            }

#if DEBUG
            this.drawRouteDebug(g, tt);
#else
            this.drawVisitRoute(g, tt, destinationBody);
#endif

            return tt.pad(N.six, N.eight);
        }

        private void drawVisitRoute(Graphics g, TextCursor tt, string? destination)
        {
            var routeBodies = getVisitRouteBodies();
            if (routeBodies.Count > 0)
            {
                tt.draw(N.six, Res.VisitRoute, GameColors.fontSmall2);
                tt.dtx += N.four;
                this.drawRouteBodies(g, tt, destination, routeBodies);
                tt.newLine(true);
            }
        }

#if DEBUG
        private void drawRouteDebug(Graphics g, TextCursor tt)
        {
            if (game?.systemData == null) return;
            var sd = game.systemData;
            var df = GameColors.fontSmall2;
            var dim = GameColors.Orange;

            // Body type breakdown
            var total = sd.bodies.Count;
            var stars = sd.bodies.Count(b => b.type == SystemBodyType.Star);
            var giants = sd.bodies.Count(b => b.type == SystemBodyType.Giant);
            var solid = sd.bodies.Count(b => b.type == SystemBodyType.SolidBody);
            var landable = sd.bodies.Count(b => b.type == SystemBodyType.LandableBody);
            var bary = sd.bodies.Count(b => b.type == SystemBodyType.Barycentre);
            var other = total - stars - giants - solid - landable - bary;

            tt.draw(N.six, $"dbg: {total} bodies (S{stars} G{giants} R{solid} L{landable} B{bary}" + (other > 0 ? $" ?{other})" : ")"), dim, df);
            tt.newLine(true);

            // DSS filter breakdown
            var scannable = sd.bodies.Where(b => b.type == SystemBodyType.Giant || b.type == SystemBodyType.SolidBody || b.type == SystemBodyType.LandableBody).ToList();
            var notDss = scannable.Where(b => !b.dssComplete).ToList();
            var afterSkips = sd.getDssRemainingBodyIds();

            tt.draw(N.six, $"dbg: scannable={scannable.Count} !dss={notDss.Count} after-filters={afterSkips.Count}", dim, df);
            tt.newLine(true);

            // Show which bodies are not-dss with their types and arrival distance
            if (notDss.Count > 0)
            {
                var bodyList = string.Join(" ", notDss.Select(b =>
                {
                    var tag = b.type == SystemBodyType.Giant ? "G" : b.type == SystemBodyType.LandableBody ? "L" : "S";
                    var bio = b.bioSignalCount > 0 ? $"+{b.bioSignalCount}bio" : "";
                    var dist = $"{b.distanceFromArrivalLS:F0}ls";
                    var hasOrb = b.orbitalEpoch.HasValue ? "" : " !orb";
                    return $"{b.shortName}({tag}{bio} {dist}{hasOrb})";
                }));
                tt.draw(N.six, $"dbg !dss: {bodyList}", dim, df);
                tt.newLine(true);
            }

            // Orbital data availability
            var withOrbit = sd.bodies.Count(b => b.semiMajorAxis > 0 && b.orbitalPeriod > 0 && b.orbitalEpoch.HasValue);
            tt.draw(N.six, $"dbg: orbit data={withOrbit}/{total}", dim, df);
            tt.newLine(true);

            // Route calculation with hop distances
            if (afterSkips.Count > 0)
            {
                sd.CalculateOptimalRoute(afterSkips);
                var routed = sd.bodies
                    .Where(b => b.visitOrder > 0)
                    .OrderBy(b => b.visitOrder)
                    .ToList();

                if (routed.Count > 0)
                {
                    var calc = sd.lastCalculator;
                    // Show route with real 3D hop distances
                    var parts = new List<string>();
                    parts.Add(routed[0].shortName);
                    for (int i = 1; i < routed.Count; i++)
                    {
                        var hopLs = calc?.GetDistanceLightSeconds(routed[i - 1].id, routed[i].id) ?? 0;
                        parts.Add($"-({hopLs:F0} ls)-> {routed[i].shortName}");
                    }
                    tt.draw(N.six, "Route: ", dim, df);
                    tt.draw(string.Join(" ", parts), GameColors.Cyan, this.font);
                    tt.newLine(true);

                    // Compare: total orbital route distance vs naive arrival-distance sort
                    if (calc != null)
                    {
                        var starId = sd.bodies.FirstOrDefault(b => b.isMainStar)?.id ?? 0;

                        // Orbital route total distance
                        double orbTotal = calc.GetDistanceLightSeconds(starId, routed[0].id);
                        for (int i = 1; i < routed.Count; i++)
                            orbTotal += calc.GetDistanceLightSeconds(routed[i - 1].id, routed[i].id);

                        // Naive route: sort by arrival distance from star
                        var naive = routed.OrderBy(b => b.distanceFromArrivalLS).ToList();
                        double naiveTotal = calc.GetDistanceLightSeconds(starId, naive[0].id);
                        for (int i = 1; i < naive.Count; i++)
                            naiveTotal += calc.GetDistanceLightSeconds(naive[i - 1].id, naive[i].id);
                        var naiveRoute = string.Join(" > ", naive.Select(b => b.shortName));

                        tt.draw(N.six, $"dbg 3D total: {orbTotal:F0}ls vs naive({naiveRoute}): {naiveTotal:F0}ls", dim, df);
                        tt.newLine(true);

                        // Show 3D distances from star
                        var distInfo = string.Join(" ", routed.Select(b =>
                        {
                            var d = calc.GetDistanceLightSeconds(starId, b.id);
                            return $"{b.shortName}={d:F0}ls";
                        }));
                        tt.draw(N.six, $"dbg 3d dist from star: {distInfo}", dim, df);
                        tt.newLine(true);
                    }
                }
                else
                {
                    tt.draw(N.six, "dbg: route returned 0 bodies", dim, df);
                    tt.newLine(true);
                }
            }
        }
#endif

        private List<(string name, int order)> getVisitRouteBodies()
        {
            if (game?.systemData == null)
                return new();

            var remaining = game.systemData.getDssRemainingBodyIds();
            if (remaining.Count == 0)
                return new();

            game.systemData.CalculateOptimalRoute(remaining);

            return game.systemData.bodies
                .Where(b => b.visitOrder > 0)
                .OrderBy(b => b.visitOrder)
                .Select(b => (name: b.shortName, order: b.visitOrder))
                .ToList();
        }

        private void drawRouteBodies(Graphics g, TextCursor tt, string? destination, List<(string name, int order)> bodies)
        {
            for (int idx = 0; idx < bodies.Count; idx++)
            {
                var (bodyName, order) = bodies[idx];
                if (string.IsNullOrWhiteSpace(bodyName)) continue;

                if (idx > 0) tt.draw(" > ", GameColors.Orange);

                var isLocal = string.IsNullOrEmpty(destination) || bodyName[0] == destination[0];

                var useFont = this.font;
                if (destination == bodyName)
                {
                    useFont = this.boldFont;
                    if (!Program.isLinux) tt.dty += 1;
                }
                var useColor = isLocal ? GameColors.Cyan : GameColors.Orange;

                tt.draw(bodyName, useColor, useFont);
                if (destination == bodyName && !Program.isLinux) tt.dty -= 1;
            }
        }

        /// <summary>
        /// Render names in a horizontal list, highlighting any in the same group as the destination
        /// </summary>
        private void drawRemainingBodies(Graphics g, TextCursor tt, string? destination, List<string> names)
        {
            // adjust for the fact that bold makes rendering shift up a pixel or two

            // draw each remaining body, highlighting color if they are in the same group as the destination, or all of them if no destination
            foreach (var bodyName in names)
            {
                if (string.IsNullOrWhiteSpace(bodyName)) continue;
                var isLocal = string.IsNullOrEmpty(destination) || bodyName[0] == destination[0];

                var useFont = this.font;
                if (destination == bodyName)
                {
                    useFont = this.boldFont;
                    if (!Program.isLinux) tt.dty += 1;
                }
                var useColor = isLocal ? GameColors.Cyan : GameColors.Orange;

                tt.draw(bodyName, useColor, useFont);
                if (destination == bodyName && !Program.isLinux) tt.dty -= 1;
                tt.dtx += N.four;
            }

            // revert adjustments
        }
    }
}

