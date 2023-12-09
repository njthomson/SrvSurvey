using DecimalMath;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotPriorScans : PlotBase, PlotterForm
    {
        const int rowHeight = 20;
        public const int highlightDistance = 150;

        public readonly List<SystemBioSignal> signals = new List<SystemBioSignal>();

        private PlotPriorScans() : base()
        {
            this.Width = 380;
            this.Height = 300;
            this.Font = GameColors.fontSmall;
        }

        private void setNewHeight()
        {
            if (this.signals.Count == 0) return;

            var rows = this.signals.Sum(_ =>
            {
                var r = 2 + ((_.trackers.Count - 1) / 4);
                return r;
            });

            // adjust height if needed
            var formHeight = 36 + (rows * rowHeight) + 12;
            if (this.Height != formHeight)
            {
                this.Height = formHeight;
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
            }

            this.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ??
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.setNewHeight();
            this.initialize();
            this.reposition(Elite.getWindowRect(true));

            this.setPriorScans();
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            Elite.floatLeftMiddle(this, gameRect);

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
            if (game.systemData == null || game.systemBody == null || game.canonnPoi == null) throw new Exception("Why?");

            var currentBody = game.systemBody.name.Replace(game.systemData.name, "").Trim();
            var bioPoi = game.canonnPoi.codex.Where(_ => _.body == currentBody && _.hud_category == "Biology" && _.latitude != null && _.longitude != null).ToList();
            Game.log($"Found {bioPoi.Count} organic signals from Canonn for: {game.systemBody.name}");

            this.signals.Clear();
            foreach (var poi in bioPoi)
            {
                // skip anything with value is too low
                var reward = Game.codexRef.getRewardForEntryId(poi.entryid.ToString()!);
                if (Game.settings.skipPriorScansLowValue && reward < Game.settings.skipPriorScansLowValueAmount)
                    continue;

                if (poi.latitude != null && poi.longitude != null && poi.entryid != null)
                {
                    // extract genus name 
                    var name = poi.english_name;
                    var nameParts = name.Split(' ', 2);
                    var genusName = BioScan.genusNames.FirstOrDefault(_ => _.Value.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase)).Key;
                    var shortName = BioScan.prefixes.FirstOrDefault(_ => _.Value == genusName).Key;

                    // skip things we've already analyzed
                    var organism = game.systemBody.organisms.FirstOrDefault(_ => _.genus == genusName);
                    if (organism?.analyzed == true) continue;

                    // skip anything too close to our own scans or or own trackers
                    var location = new LatLong2((double)poi.latitude, (double)poi.longitude);
                    var tooClose = game.systemBody.bioScans?.Any(_ => _.genus == genusName && Util.getDistance(_.location, location, game.systemBody.radius) < PlotTrackers.highlightDistance) == true
                     || game.systemBody.bookmarks?.Any(marks => marks.Key == shortName && marks.Value.Any(_ => Util.getDistance(_, location, game.systemBody.radius) < PlotTrackers.highlightDistance)) == true
                     || Util.isCloseToScan(location, genusName);
                    if (tooClose == true) continue;

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

                    signal.trackers.Add(new TrackingDelta(game.nearBody!.radius, location));
                }
            }

            // stop here and close the plotter if there's nothing to show
            if (this.signals.Count == 0)
            {
                Game.log($"Zero prior scans, self closing PlotPriorScans");
                Program.closePlotter<PlotPriorScans>();
                return;
            }

            // force a numeric sort?
            this.signals.Sort((a, b) => b.credits.CompareTo(a.credits));

            foreach (var signal in this.signals)
            {
                // calc distances and sort by closest firs
                signal.trackers.ForEach(_ => _.calc());
                signal.trackers.Sort((a, b) => a.distance.CompareTo(b.distance));

                // remove any scans within 100m of another
                for (var n = 1; n < signal.trackers.Count; n++)
                {
                    var relativeDistance = signal.trackers[n].distance - signal.trackers[n - 1].distance;
                    if (relativeDistance < 100)
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
                    this.signals.Remove(match);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game.nearBody == null) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            this.dtx = 4;
            this.dty = 8;
            base.drawTextAt($"Tracking {this.signals.Count} signals from Canonn:", GameColors.brushGameOrange, GameColors.fontSmall);

            var indent = 70;
            var bearingWidth = 75;

            this.dty = 8;
            var sortedSignals = this.signals.OrderByDescending(_ => _.reward);
            foreach (var signal in sortedSignals)
            {
                var analyzed = game.systemBody?.organisms?.FirstOrDefault(_ => _.genus == signal.genusName)?.analyzed == true;
                var isActive = (game.cmdr.scanOne?.genus == null) || game.cmdr.scanOne?.genus == signal.genusName;
                Brush brush;

                // keep this y value for the label below
                var ly = this.dty += rowHeight;

                // but adjust the general x/y for trackers
                this.dtx = indent;
                this.dty += rowHeight;
                var isClose = false;
                var isCloseIsh = false;

                var r = new Rectangle(8, 0, this.Width - 16, 14);
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

                    if (dd.distance > 1_000_000) // within 50km
                    {
                        brush = GameColors.PriorScans.FarAway.brush;
                        pen = GameColors.PriorScans.FarAway.pen;
                    }
                    else
                    {
                        isCloseIsh = true;
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

                    this.drawBearingTo(dtx, dty, "", (double)dd.distance, (double)deg, brush, pen);
                    dtx += bearingWidth;
                }

                // draw label above trackers - color depending on if any of them are close
                //if (isClose) brush = isActive ? Brushes.Lime : Brushes.DarkGreen;
                brush = GameColors.brushGameOrangeDim;
                if (isActive) brush = GameColors.PriorScans.Active.brush;
                if (isClose) brush = isActive ? GameColors.PriorScans.CloseActive.brush : GameColors.PriorScans.CloseInactive.brush;
                if (analyzed) brush = Brushes.DarkSlateGray;

                var f = this.Font;
                if (isActive) f = new Font(this.Font, FontStyle.Bold);

                r.Y = (int)ly;
                TextRenderer.DrawText(g, signal.poiName, f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Left);
                TextRenderer.DrawText(g, signal.credits, f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Right);

                if (game.status.Altitude > 500) //game.isMode(GameMode.SuperCruising, GameMode.GlideMode))
                {
                    // calculate angle of decline for the nearest location
                    r.Y += rowHeight;
                    var aa = DecimalEx.ToDeg(DecimalEx.ATan(game.status.Altitude / signal.trackers[0].distance));
                    // choose color based on ...
                    if (aa < 10) // .. 0
                        brush = Brushes.Transparent; // it's probably around the curve of the planet
                    else if (aa < 30) // .. 10
                        brush = Brushes.Orange;
                    else if (aa < 50) // .. 30
                        brush = Brushes.Cyan;
                    else if (aa < 60) // .. 50
                        brush = Brushes.Red;
                    else // > 60
                        brush = Brushes.DarkRed;

                    TextRenderer.DrawText(g, $"-{(int)aa}°", f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Left);
                }
            }

            this.drawFooterText("(Tracked locations may not be that close to signals)", GameColors.brushGameOrangeDim, this.Font);
        }
    }
}
