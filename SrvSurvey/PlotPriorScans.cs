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
            var formHeight = 36 + (rows * rowHeight);
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
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;

            var plotGrounded = Program.getPlotter<PlotGrounded>();
            var plotTrackers = Program.getPlotter<PlotTrackers>();

            //if (plotTrackers != null)
            //{
            //    this.Width = plotTrackers.Width;
            //    this.Left = gameRect.Right - this.Width - 20;
            //    this.Top = plotTrackers.Bottom + 4;
            //}
            //else if (plotGrounded != null)
            //{
            //    this.Width = plotGrounded.Width;
            //    this.Left = gameRect.Right - this.Width - 20;
            //    this.Top = plotGrounded.Bottom + 4;
            //}
            //else
            {
                //Elite.floatRightMiddle(this, gameRect, 20);
                Elite.floatRightTop(this, gameRect, 20);
            }

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

        public void setPriorScans(List<Codex> localPoi)
        {
            this.signals.Clear();

            foreach (var poi in localPoi)
            {
                var name = poi.english_name;
                if (poi.latitude != null && poi.longitude != null && poi.entryid != null)
                {
                    // create group and TrackingDelta's for each location
                    var signal = this.signals.FirstOrDefault(_ => _.poiName == poi.english_name);
                    if (signal == null)
                    {
                        var nameParts = name.Split(' ', 2);
                        var genusName = BioScan.genusNames.FirstOrDefault(_ => _.Value.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase)).Key;
                        // for pre-Odyssey bio's
                        if (!name.Contains("-"))
                            genusName = BioScan.genusNames.FirstOrDefault(_ => _.Value.Equals(nameParts[1], StringComparison.OrdinalIgnoreCase)).Key;

                        var reward = Game.codexRef.getRewardForEntryId(poi.entryid.ToString()!);

                        signal = new SystemBioSignal
                        {
                            poiName = poi.english_name,
                            genusName = genusName,
                            credits = Util.credits(reward),
                        };
                        this.signals.Add(signal);
                    }

                    //if (!this.trackers.ContainsKey(name))
                    //    this.trackers.Add(name, new List<TrackingDelta>());

                    var location = new LatLong2((double)poi.latitude, (double)poi.longitude);
                    signal.trackers.Add(new TrackingDelta(game.nearBody!.radius, location));
                }
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
            foreach (var signal in this.signals)
            {
                var analyzed = signal.genusName != null && game.nearBody.data.organisms.ContainsKey(signal.genusName) && game.nearBody.data.organisms[signal.genusName].analyzed;
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
                //var rr = new Rectangle(this.Width - 208, 0, 200, 14);
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

                    brush = isActive ? Brushes.Lime : Brushes.SlateGray;
                    //if (isClose) brush = isActive ? Brushes.Lime : Brushes.LightSeaGreen;

                    var pen = isActive ? GameColors.penLime2 : Pens.SlateGray;
                    //if (isClose) pen = isActive ? GameColors.penLime2 : Pens.LightSeaGreen;

                    if (dd.distance > 1_000_000) // within 50km
                    {
                        brush = Brushes.ForestGreen;
                        pen = Pens.ForestGreen;
                    }
                    else
                    {
                        isCloseIsh = true;
                    }
                    if (dd.distance < PlotTrackers.highlightDistance)
                    {
                        brush = isActive ? Brushes.Yellow : Brushes.Olive;
                        pen = isActive ? Pens.Yellow : Pens.Olive;
                    }

                    if (analyzed || isTooCloseToScan)
                    {
                        brush = Brushes.DarkSlateGray;
                        pen = Pens.DarkSlateGray;
                    }

                    this.drawBearingTo(dtx, dty, "", (double)dd.distance, (double)deg, brush, pen);
                    dtx += bearingWidth;
                }

                // draw label above trackers - color depending on if any of them are close
                //if (isClose) brush = isActive ? Brushes.Lime : Brushes.DarkGreen;
                brush = isCloseIsh ? Brushes.LightSlateGray : Brushes.ForestGreen;
                if (isActive) brush = Brushes.Lime;
                if (isClose) brush = isActive ? Brushes.Yellow : Brushes.Olive;
                if (analyzed) brush = Brushes.DarkSlateGray;

                var f = this.Font;
                if (isActive) f = new Font(this.Font, FontStyle.Bold);

                r.Y = (int)ly;
                TextRenderer.DrawText(g, signal.poiName, f, r, ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Left);
                TextRenderer.DrawText(g, signal.credits, f, r , ((SolidBrush)brush).Color, TextFormatFlags.NoPadding | TextFormatFlags.Right);
            }
        }
    }
}
