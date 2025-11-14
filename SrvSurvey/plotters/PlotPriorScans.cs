using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using Res = Loc.PlotPriorScans;

namespace SrvSurvey.plotters
{
    internal class PlotPriorScans : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotPriorScans),
            allowed = allowed,
            ctor = (game, def) => new PlotPriorScans(game, def),
            defaultSize = new Size(308, 300),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.useExternalData
                && Game.settings.autoLoadPriorScans
                && game.systemBody != null
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && !game.hidePlottersFromCombatSuits
                && !game.status.Docked
                && (!Game.settings.enableGuardianSites || !PlotGuardians.allowed(game))
                && (!Game.settings.autoShowHumanSitesTest || !PlotHumanSite.allowed(game))
                && (!Game.settings.autoShowPlotStationInfo_TEST || !PlotStationInfo.allowed(game))
                && game.canonnPoiHasLocalBioSignals()
                && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.SAA, GameMode.Codex)
                ;
        }

        #endregion

        int rowHeight = N.s(20);
        public const int highlightDistance = 150;

        public readonly List<SystemBioSignal> signals = new List<SystemBioSignal>();
        private Font boldFont;

        private PlotPriorScans(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;
            this.boldFont = new Font(this.font, FontStyle.Bold);

            this.setPriorScans();
        }

        // It's easy for this to overlap with PlotBioSystem ... so shift ourselves up if that is the case
        //var bioSys = Program.getPlotter<PlotBioSystem>(); TODO: REVISIT !!!
        //if (bioSys != null) avoidPlotBioSystem(bioSys);

        public void avoidPlotBioSystem(PlotBioSystem bioSys)
        {
            var middle = this.left + this.width / 2;
            if (bioSys.top < this.bottom && bioSys.left < middle && bioSys.right > middle && bioSys.bottom > this.bottom)
            {
                var delta = this.bottom - bioSys.top;
                this.top -= delta + 8;
            }
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            // re-calc distances and re-order TrackingDeltas
            foreach (var signal in this.signals)
            {
                signal.trackers.ForEach(_ => _.calc());
                signal.trackers.Sort((a, b) => a.distance.CompareTo(b.distance));
            }

            if (status.changed.Contains(nameof(Status.Heading)) || status.changed.Contains(nameof(Status.Latitude)) || status.changed.Contains(nameof(Status.Longitude)))
                this.invalidate();
        }

        public void setPriorScans()
        {
            if (game.systemData == null || game.systemBody == null || game.canonnPoi == null) throw new Exception("Why?");

            var currentBody = game.systemBody.name.Replace(game.systemData.name, "").Trim();
            var bioPoi = game.canonnPoi.codex.Where(_ => _.body == currentBody && _.hud_category == "Biology" && _.latitude != null && _.longitude != null).ToList();
            Game.log($"Found {bioPoi.Count} organic signals from Canonn for: {game.systemBody.name}");

            this.signals.Clear();
            foreach (var poi in bioPoi)
            {
                if (poi.latitude != null && poi.longitude != null && poi.entryid != null)
                {
                    // skip anything with value is too low
                    var match = Game.codexRef.matchFromEntryId2(poi.entryid.Value);
                    if (match == null) continue;
                    var reward = match.species.reward;
                    if (Game.settings.skipPriorScansLowValue && reward < Game.settings.skipPriorScansLowValueAmount)
                        continue;

                    // skip things we've already analyzed
                    if (Game.settings.hideMyOwnCanonnSignals && game.systemBody.organisms?.FirstOrDefault(_ => _.entryId == poi.entryid)?.analyzed == true)
                        continue;

                    // skip anything too close to our own scans or or own trackers
                    var location = new LatLong2((double)poi.latitude, (double)poi.longitude);

                    if (Game.settings.hideMyOwnCanonnSignals && game.systemBody.bioScans != null && game.systemBody.bioScans.Any(_ => _.status != BioScan.Status.Died && _.species == match.species.name && Util.getDistance(_.location, location, game.systemBody.radius) < PlotTrackers.highlightDistance))
                        continue;
                    if (Util.isCloseToScan(location, match.species.name))
                        continue;

                    // create group and TrackingDelta's for each location
                    var signal = this.signals.FirstOrDefault(_ => _.entryId == poi.entryid);
                    if (signal == null)
                    {

                        signal = new SystemBioSignal
                        {
                            match = match,
                            entryId = poi.entryid.Value,
                            displayName = match.variant.locName,
                            credits = Util.credits(reward),
                            reward = reward,
                        };

                        // for pre-Odyssey bio's
                        if (!match.genus.odyssey)
                            signal.displayName = $"{match.genus.locName} - {match.variant.locColorName}";
                        else if (match.entryId == 2460101) // Radicoida Unica
                            signal.displayName = $"{match.genus.englishName} - {match.species.locName}";

                        this.signals.Add(signal);
                    }

                    signal.trackers.Add(new TrackingDelta(game.systemBody!.radius, location));
                }
            }

            // sort most profitable topmost
            this.signals.Sort((a, b) => b.credits.CompareTo(a.credits));

            foreach (var signal in this.signals)
            {
                // calc distances and sort by closest firs
                signal.trackers.ForEach(_ => _.calc());
                signal.trackers.Sort((a, b) => a.distance.CompareTo(b.distance));

                // remove any scans too close to each other
                for (var n = 1; n < signal.trackers.Count; n++)
                {
                    var relativeDistance = signal.trackers[n].distance - signal.trackers[n - 1].distance;
                    if (relativeDistance < PlotTrackers.highlightDistance)
                    {
                        signal.trackers.RemoveAt(n);
                        n--;
                    }
                }
            }

            this.invalidate();

            PlotBase2.invalidate(nameof(PlotGrounded));
        }

        protected override void onJournalEntry(ScanOrganic entry)
        {
            if (game.systemBody == null) return;

            // TODO: revisit for Brain Trees

            // remove any tracked locations that are close enough to what we scanned
            var match = this.signals.FirstOrDefault(_ => _.match.species.name == entry.Species);
            if (match != null)
            {
                Application.DoEvents();
                match.trackers.RemoveAll(_ => _.distance < PlotTrackers.highlightDistance);

                // remove the whole group if empty or we finished analyzing this species
                if (Game.settings.hideMyOwnCanonnSignals && match.trackers.Count == 0 || entry.ScanType == ScanType.Analyse)
                {
                    this.signals.Remove(match);
                    this.invalidate();
                }
            }
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            if (game?.systemBody == null) return frame.Size;

            tt.dty = N.eight;

            var txt = Res.Header.format(this.signals.Count);
            tt.draw(N.eight, txt);
            if (Game.settings.skipPriorScansLowValue)
                tt.draw($" (> {Util.credits(Game.settings.skipPriorScansLowValueAmount)})", C.orangeDark);
            tt.newLine(N.eight, true);

            if (this.signals.Count == 0)
            {
                tt.dty = N.threeSix;
                tt.draw(N.oneSix, Res.NoMoreSignals, C.orangeDark);
                tt.newLine(N.eight, true);

                return tt.pad(N.oneSix, +N.ten);
            }

            var indent = N.eighty;
            var bearingWidth = N.sevenFive;

            var sortedSignals = this.signals.OrderByDescending(_ => _.reward);
            foreach (var signal in sortedSignals)
            {
                var analyzed = game.systemBody?.organisms?.FirstOrDefault(_ => _.species == signal.match.species.name)?.analyzed == true;
                var isActive = (game.cmdr.scanOne?.species == signal.match.species.name && game.cmdr.scanOne?.body == game.systemBody?.name) || (game.cmdr.scanOne?.genus == null);
                Brush brush;

                // draw signal title + credits
                var col = isActive ? C.cyan : C.orange;
                if (analyzed) col = C.orangeDark;

                var sz = tt.draw(this.width - N.six, signal.credits, col, null, true);
                tt.draw(N.six, signal.displayName, col);
                tt.dtx += sz.Width + N.oneSix;

                // strike-through if already analyzed
                if (analyzed)
                {
                    var y = Util.centerIn(tt.lastTextSize.Height, 2);
                    g.DrawLine(GameColors.PriorScans.Analyzed.pen, N.ten, tt.dty + y, this.width - N.ten, tt.dty + y);
                }
                tt.newLine(+N.six, true);

                // but adjust the general x/y for trackers
                tt.dtx = indent;
                var isClose = false;

                if (game.status.Altitude > 500)
                {
                    var first = signal.trackers.First();
                    var gt = new GroundTarget(this.font);
                    gt.renderAngleOfAttack(g, N.eight, tt.dty - N.two, game.status.PlanetRadius, first.Target, Status.here, false);
                }

                foreach (var dd in signal.trackers)
                {
                    if (tt.dtx + bearingWidth > this.width - 8)
                    {
                        if (analyzed)
                            g.DrawLine(GameColors.PriorScans.Analyzed.pen, indent, tt.dty + N.four, tt.dtx, tt.dty + N.four);

                        // render next tracker on a new line if need be
                        tt.dtx = indent;
                        tt.newLine(N.ten, true);
                    }

                    var isTooCloseToScan = Util.isCloseToScan(dd.Target, signal.match.species.name);

                    isClose |= dd.distance < highlightDistance && !analyzed && !isTooCloseToScan;
                    var deg = dd.angle - game.status!.Heading;

                    brush = isActive ? GameColors.PriorScans.Active.brush : GameColors.PriorScans.Inactive.brush;
                    var pen = isActive ? GameColors.PriorScans.Active.pen : GameColors.PriorScans.Inactive.pen;
                    col = C.orange;

                    if (dd.distance > 1_000_000) // within 1,000km
                    {
                        brush = GameColors.PriorScans.FarAway.brush;
                        pen = GameColors.PriorScans.FarAway.pen;
                        col = C.orangeDark;
                    }

                    if (dd.distance < PlotTrackers.highlightDistance)
                    {
                        brush = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush;
                        pen = isActive ? GameColors.PriorScans.CloseActive.pen : GameColors.PriorScans.CloseInactive.pen;
                        col = C.cyan;
                    }

                    if (analyzed || isTooCloseToScan)
                    {
                        brush = GameColors.PriorScans.Analyzed.brush;
                        pen = GameColors.PriorScans.Analyzed.pen;
                    }

                    if (signal.trackers.IndexOf(dd) == 0 && game.isMode(GameMode.SuperCruising, GameMode.GlideMode))
                    {
                        if ((deg > 330 || deg < 30))
                            pen = GameColors.penCyan2;
                        else if (deg > 90 && deg < 270)
                            pen = GameColors.penDarkRed2;
                        else if (deg > 60 && deg < 300)
                            pen = GameColors.penRed2;
                    }

                    // the X value for the next bearing
                    var nx = tt.dtx + bearingWidth;
                    BaseWidget.renderBearingTo(g, tt.dtx, tt.dty, N.five, (double)deg, brush, pen);
                    tt.dtx += N.oneSix;
                    tt.draw(Util.metersToString(dd.distance), col);

                    tt.dtx = nx;
                }

                if (analyzed)
                    g.DrawLine(GameColors.PriorScans.Analyzed.pen, indent, tt.dty + N.four, tt.dtx, tt.dty + N.four);

                // draw label above trackers - color depending on if any of them are close
                brush = GameColors.brushGameOrangeDim;
                if (isActive) brush = GameColors.PriorScans.CloseActive.brush;
                if (isClose) brush = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush;
                if (analyzed) brush = Brushes.DarkSlateGray;

                tt.newLine(N.ten, true);
            }

            tt.dty += N.four;
            tt.draw(N.eight, Res.Footer, C.orangeDark);
            tt.dtx += N.eight;
            tt.newLine(+N.ten, true);

            return tt.pad();
        }
    }
}
