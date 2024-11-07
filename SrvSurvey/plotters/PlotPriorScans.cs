using DecimalMath;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotPriorScans : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.settings.useExternalData
                && Game.settings.autoLoadPriorScans
                && Game.activeGame?.systemBody != null
                && !Game.activeGame.hidePlottersFromCombatSuits
                && !Game.activeGame.status.Docked
                && !PlotGuardians.allowPlotter && !Program.isPlotter<PlotGuardians>()
                && !PlotHumanSite.allowPlotter && !Program.isPlotter<PlotHumanSite>()
                && Game.activeGame.canonnPoiHasLocalBioSignals()
                && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.SAA, GameMode.Codex)
                ;
        }

        int rowHeight = scaled(20);
        public const int highlightDistance = 150;

        public readonly List<SystemBioSignal> signals = new List<SystemBioSignal>();
        private Font boldFont;

        private PlotPriorScans() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall;
            this.boldFont = new Font(this.Font, FontStyle.Bold);
        }

        public override bool allow { get => PlotPriorScans.allowPlotter; }

        private void setNewHeight()
        {
            var rows = this.signals.Sum(_ =>
            {
                var r = 2 + ((_.trackers.Count - 1) / 3);
                return r;
            });

            // adjust height if needed
            var formHeight = 36 + (rows * rowHeight) + 12;

            if (rows == 0)
                formHeight += 40;

            formHeight = scaled(formHeight);
            if (this.Height != formHeight)
            {
                this.Height = formHeight;
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
            }

            this.mid = this.Size / 2;
            this.Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(308);
            this.Height = scaled(300);
            base.OnLoad(e);

            this.setNewHeight();
            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            this.setPriorScans();
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var showPlotter = PlotPriorScans.allowPlotter;

            if (!showPlotter && game.systemBody != null && game.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter, GameMode.CommsPanel, GameMode.SAA))
                showPlotter = SystemData.isWithinLastDssDuration();

            showPlotter = showPlotter && (!force && this.signals.Count > 0); // keep plotter hidden if no more signals

            if (!force && this.Opacity > 0 && !showPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && showPlotter)
                this.reposition(Elite.getWindowRect());

            if (game.systemBody == null || game.systemBody.bioSignalCount == 0)
                Program.closePlotter<PlotPriorScans>();
            else
                this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            // re-calc distances and re-order TrackingDeltas
            foreach (var signal in this.signals)
            {
                signal.trackers.ForEach(_ => _.calc());
                signal.trackers.Sort((a, b) => a.distance.CompareTo(b.distance));
            }

            base.Status_StatusChanged(blink);
        }

        public void setPriorScans()
        {
            if (this.IsDisposed) return;
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
                    var match = Game.codexRef.matchFromEntryId(poi.entryid.Value);
                    var reward = match.species.reward;
                    if (Game.settings.skipPriorScansLowValue && reward < Game.settings.skipPriorScansLowValueAmount)
                        continue;

                    // extract genus name 
                    var name = poi.english_name;
                    var nameParts = name.Split(' ', 2);
                    var genusName = BioScan.genusNames.FirstOrDefault(_ => _.Value.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase)).Key;
                    var shortName = BioScan.prefixes.FirstOrDefault(_ => _.Value == genusName).Key;

                    // skip things we've already analyzed
                    //var organism = game.systemBody.organisms?.FirstOrDefault(_ => _.genus == genusName);
                    //if (organism?.analyzed == true) continue;

                    // skip anything too close to our own scans or or own trackers
                    var location = new LatLong2((double)poi.latitude, (double)poi.longitude);

                    // TODO: handle Brain Tree's
                    if (Game.settings.hideMyOwnCanonnSignals && game.systemBody.bioScans?.Any(_ => _.status != BioScan.Status.Died && _.genus == genusName && Util.getDistance(_.location, location, game.systemBody.radius) < PlotTrackers.highlightDistance) == true)
                        continue;
                    if (game.systemBody.bookmarks?.Any(marks => marks.Key == shortName && marks.Value.Any(_ => Util.getDistance(_, location, game.systemBody.radius) < PlotTrackers.highlightDistance)) == true)
                        continue;
                    if (Util.isCloseToScan(location, genusName))
                        continue;
                    // TODO: fix this + make it honour the setting
                    //if (game.systemBody.organisms?.FirstOrDefault(_ => _.entryId == poi.entryid)?.analyzed == true)
                    //    continue;

                    // create group and TrackingDelta's for each location
                    var signal = this.signals.FirstOrDefault(_ => _.poiName == poi.english_name);
                    if (signal == null)
                    {
                        // for pre-Odyssey bio's
                        if (!name.Contains("-"))
                            genusName = BioScan.genusNames.FirstOrDefault(_ => _.Value.Equals(nameParts[1], StringComparison.OrdinalIgnoreCase)).Key;

                        signal = new SystemBioSignal
                        {
                            poiName = poi.english_name,
                            genusName = genusName,
                            credits = Util.credits(reward),
                            reward = reward,
                        };
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

            this.setNewHeight();
            this.Invalidate();

            Program.getPlotter<PlotGrounded>()?.Invalidate();
        }

        protected override void onJournalEntry(ScanOrganic entry)
        {
            if (this.IsDisposed || game.systemBody == null) return;

            // TODO: revisit for Brain Trees

            // remove any tracked locations that are close enough to what we scanned
            var match = this.signals.FirstOrDefault(_ => _.genusName == entry.Genus);
            if (match != null)
            {
                match.trackers.RemoveAll(_ => _.distance < PlotTrackers.highlightDistance);

                // remove the whole group if empty or we finished analyzing this species
                if (match.trackers.Count == 0 || entry.ScanType == ScanType.Analyse)
                {
                    this.signals.Remove(match);
                    this.setNewHeight();
                }
            }
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game?.systemBody == null) return;

            this.resetPlotter(g);

            this.drawFooterText("(Locations may not be that close to signals)", GameColors.brushGameOrangeDim, this.Font);

            this.dtx = four;
            this.dty = eight;

            var txt = $"Tracking {this.signals.Count} signals from Canonn:";
            if (Game.settings.skipPriorScansLowValue)
                txt += $" (> {Util.credits(Game.settings.skipPriorScansLowValueAmount)})";
            base.drawTextAt(txt, GameColors.brushGameOrange, GameColors.fontSmall);

            if (this.signals.Count == 0)
            {
                g.DrawString("No un-scanned signals meet criteria", this.Font, GameColors.brushGameOrange, 16f, 36f);
                return;
            }

            var indent = scaled(70f);
            var bearingWidth = scaled(75);

            this.dty = scaled(8f);
            var sortedSignals = this.signals.OrderByDescending(_ => _.reward);
            foreach (var signal in sortedSignals)
            {
                var analyzed = game.systemBody?.organisms?.FirstOrDefault(_ => _.genus == signal.genusName)?.analyzed == true;
                var isActive = game.cmdr.scanOne?.genus == signal.genusName || (game.cmdr.scanOne?.genus == null);
                Brush brush;

                // keep this y value for the label below
                var ly = this.dty += rowHeight;

                // but adjust the general x/y for trackers
                this.dtx = indent;
                this.dty += rowHeight;
                var isClose = false;

                var r = new Rectangle(scaled(8), 0, this.Width - scaled(16), scaled(14));
                foreach (var dd in signal.trackers)
                {
                    if (dtx + bearingWidth > this.Width - 8)
                    {
                        // render next tracker on a new line if need be
                        this.dtx = indent;
                        this.dty += rowHeight;
                    }

                    var isTooCloseToScan = Util.isCloseToScan(dd.Target, signal.genusName);

                    isClose |= dd.distance < highlightDistance && !analyzed && !isTooCloseToScan;
                    var deg = dd.angle - game.status!.Heading;

                    brush = isActive ? GameColors.PriorScans.Active.brush : GameColors.PriorScans.Inactive.brush;
                    var pen = isActive ? GameColors.PriorScans.Active.pen : GameColors.PriorScans.Inactive.pen;

                    if (dd.distance > 1_000_000) // within 1,000km
                    {
                        brush = GameColors.PriorScans.FarAway.brush;
                        pen = GameColors.PriorScans.FarAway.pen;
                    }

                    if (dd.distance < PlotTrackers.highlightDistance)
                    {
                        brush = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush;
                        pen = isActive ? GameColors.PriorScans.CloseActive.pen : GameColors.PriorScans.CloseInactive.pen;
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

                    this.drawBearingTo(dtx, dty, "", (double)dd.distance, (double)deg, brush, pen);
                    dtx += bearingWidth;
                }

                // draw label above trackers - color depending on if any of them are close
                brush = GameColors.brushGameOrangeDim;
                if (isActive) brush = GameColors.PriorScans.CloseActive.brush;
                if (isClose) brush = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush;
                if (analyzed) brush = Brushes.DarkSlateGray;

                var f = this.Font;
                if (isActive) f = this.boldFont;

                r.Y = (int)ly;

                TextRenderer.DrawText(g, signal.poiName, f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Left);
                TextRenderer.DrawText(g, signal.credits, f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Right);

                // strike-through if already analyzed
                // ??

                if (game.status.Altitude > 500) //game.isMode(GameMode.SuperCruising, GameMode.GlideMode))
                {
                    // calculate angle of decline for the nearest location
                    r.Y += rowHeight;
                    var aa = DecimalEx.ToDeg(DecimalEx.ATan(game.status.Altitude / signal.trackers[0].distance));
                    // choose color based on ...
                    if (aa < 10) // .. 0
                        continue; // brush = Brushes.Transparent; // it's probably around the curve of the planet
                    else if (aa < 30) // .. 10
                        brush = Brushes.Orange;
                    else if (aa < 50) // .. 30
                        brush = Brushes.Cyan;
                    else if (aa < 60) // .. 50
                        brush = Brushes.Red;
                    else // > 60
                        brush = Brushes.DarkRed;

                    // draw angle of attack - pie
                    var deg = signal.trackers[0].angle - game.status!.Heading;
                    var attackAngle = deg > 90 && deg < 270 ? 180 - aa : aa;
                    GraphicsPath path = new GraphicsPath();
                    path.AddPie(r.X, r.Y - 22, 36, 36, 180, -(int)attackAngle);
                    g.FillPath(brush, path);
                    var pp = new Pen(brush, 2.2f * GameColors.scaleFactor);
                    g.DrawPath(pp, path);

                    // draw angle of attack - text
                    r.X += scaled(38);

                    r.Width = scaled(r.Width);
                    r.Height = scaled(r.Height);
                    TextRenderer.DrawText(g, $"-{(int)aa}°", f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Left);
                }
            }
        }
    }
}
