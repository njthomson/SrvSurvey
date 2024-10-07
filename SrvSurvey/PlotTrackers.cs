using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        int rowHeight = scaled(20);
        public const int highlightDistance = 150;

        public Dictionary<string, List<TrackingDelta>> trackers = new Dictionary<string, List<TrackingDelta>>();

        private PlotTrackers() : base()
        {
            this.Size = Size.Empty;
        }

        public override bool allow { get => PlotTrackers.allowPlotter; }

        public static bool allowPlotter
        {
            get => PlotGrounded.allowPlotter
                // same as PlotGrounded + do we have any bookmarks?
                && Game.activeGame?.systemBody?.bookmarks?.Count > 0;
        }

        private void setNewHeight()
        {
            if (this.IsDisposed || game?.systemBody?.bookmarks == null) return;

            // adjust height if needed
            var formHeight = scaled(42) + (game.systemBody.bookmarks.Count * rowHeight);
            if (this.Height != formHeight)
            {
                this.Height = formHeight;
                this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
            }

            this.Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(380);
            this.Height = scaled(100);
            base.OnLoad(e);

            this.prepTrackers();
            this.setNewHeight();
            this.initializeOnLoad();
        }

        public void prepTrackers()
        {
            if (this.IsDisposed) return;

            this.trackers.Clear();
            if (game.systemBody?.bookmarks != null)
            {
                foreach (var name in game.systemBody.bookmarks.Keys)
                {
                    if (!this.trackers.ContainsKey(name))
                    {
                        // create group and TrackingDelta's for each location
                        this.trackers.Add(name, new List<TrackingDelta>());

                        foreach (var pos in game.systemBody.bookmarks[name])
                            this.trackers[name].Add(new TrackingDelta(game.systemBody.radius, pos));

                        this.trackers[name].Sort((a, b) => a.distance.CompareTo(b.distance));
                    }
                }
            }

            this.setNewHeight();
            Program.getPlotter<PlotGrounded>()?.Invalidate();

            if (this.trackers.Count == 0)
                this.BeginInvoke(() => Program.closePlotter<PlotTrackers>());
        }

        public override void reposition(Rectangle gameRect)
        {
            // match opacity of PlotGrounded
            this.Opacity = PlotPos.getOpacity(nameof(PlotGrounded));

            if (gameRect != Rectangle.Empty)
            {
                // position ourself under PlotGrounded, if it exists
                var plotGrounded = Program.getPlotter<PlotGrounded>();
                if (plotGrounded != null)
                {
                    plotGrounded.reposition(gameRect);
                    this.Width = plotGrounded.Width;
                    this.Left = plotGrounded.Left;
                    this.Top = plotGrounded.Bottom + (int)four;
                }
                else
                {
                    // otherwise position ourself where PlotGrounded would be
                    PlotPos.reposition(this, gameRect, nameof(PlotGrounded));
                }

                this.Invalidate();
            }
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !PlotGrounded.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotGrounded.allowPlotter)
                this.reposition(Elite.getWindowRect());

            if (game.systemBody == null)
                Program.closePlotter<PlotTrackers>();
            else
                this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            // re-calc distances and re-order TrackingDeltas
            foreach (var name in this.trackers.Keys)
            {
                this.trackers[name].ForEach(_ => _.calc());
                this.trackers[name].Sort((a, b) => a.distance.CompareTo(b.distance));
            }

            base.Status_StatusChanged(blink);
        }

        protected override void onJournalEntry(ScanOrganic entry)
        {
            if (this.IsDisposed || game.systemBody == null) return;

            // if we are close enough to a tracker ... auto remove it
            var prefix = BioScan.prefixes.First(_ => _.Value.Contains(entry.Genus)).Key;
            if (this.trackers.ContainsKey(prefix))
            {
                var td = this.trackers[prefix].FirstOrDefault();
                if (td != null)
                {
                    Game.log($"Distance to nearest '{entry.Genus}' tracker: {Util.metersToString(td.distance)}");

                    //var radius = BioScan.ranges.ContainsKey(entry.Genus) ? BioScan.ranges[entry.Genus] : highlightDistance;
                    if (td.distance < highlightDistance && Game.settings.autoRemoveTrackerOnSampling)
                    {
                        Game.log($"Auto removing tracker for: '{entry.Species_Localised}'/'{entry.Genus}'");
                        game.removeBookmark(prefix, Status.here.clone(), true);
                        this.prepTrackers();
                    }
                }
            }
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game?.systemData == null || game.systemBody == null || game.systemBody.bookmarks == null || game.systemBody.bookmarks?.Count == 0) return;

            g.DrawString($"Tracking {game.systemBody.bookmarks?.Count} targets:", GameColors.fontSmall, GameColors.brushGameOrange, scaled(4), scaled(8));

            var indent = scaled(220 + 80);
            var y = scaled(12);
            foreach (var name in this.trackers.Keys)
            {
                y += rowHeight;

                var x = (float)this.Width - indent;

                BioScan.prefixes.TryGetValue(name, out var fullName);
                var isActive = game.cmdr.scanOne?.genus == null || game.cmdr.scanOne?.genus == fullName;
                var isClose = false;
                var bearingWidth = scaled(75);
                Brush brush;
                foreach (var dd in this.trackers[name])
                {
                    var deg = dd.angle - game.status!.Heading;
                    //var radius = BioScan.ranges.ContainsKey(name) ? BioScan.ranges[name] : highlightDistance;

                    isClose |= dd.distance < highlightDistance;
                    brush = isActive ? GameColors.brushGameOrange : GameColors.brushGameOrangeDim;
                    if (dd.distance < highlightDistance) brush = isActive ? GameColors.brushCyan : GameColors.brushDarkCyan;

                    var pen = isActive ? GameColors.penGameOrange2 : GameColors.penGameOrangeDim2;
                    if (dd.distance < highlightDistance) pen = isActive ? GameColors.penCyan2 : GameColors.penDarkCyan1;

                    this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                    x += bearingWidth;
                }

                string? displayName;
                if (!BioScan.prefixes.ContainsKey(name) || !BioScan.genusNames.TryGetValue(BioScan.prefixes[name], out displayName))
                    displayName = name;

                var sz = g.MeasureString(displayName, GameColors.fontSmall);
                brush = isActive ? GameColors.brushGameOrange : GameColors.brushGameOrangeDim;
                if (isClose) brush = isActive ? GameColors.brushCyan : GameColors.brushDarkCyan;

                g.DrawString(
                    displayName,
                    GameColors.fontSmall,
                    brush,
                    this.Width - indent - sz.Width + scaled(3), y);
            }
        }
    }

}
