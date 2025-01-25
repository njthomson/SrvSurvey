using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => PlotGrounded.allowPlotter
                // same as PlotGrounded + do we have any bookmarks?
                && Game.activeGame?.systemBody?.bookmarks?.Count > 0;
        }

        public const int highlightDistance = 150;

        public Dictionary<string, List<TrackingDelta>> trackers = new Dictionary<string, List<TrackingDelta>>();

        private PlotTrackers() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(380);
            this.Height = scaled(100);
            base.OnLoad(e);

            this.prepTrackers();
            this.initializeOnLoad();
        }

        public override bool allow { get => PlotTrackers.allowPlotter; }

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

            Program.invalidate<PlotGrounded>();

            if (this.trackers.Count == 0)
                Program.defer(() => Program.closePlotter<PlotTrackers>());
        }

        public override void reposition(Rectangle gameRect)
        {
            // match opacity of PlotGrounded
            this.Opacity = PlotPos.getOpacity(nameof(PlotGrounded));

            if (gameRect != Rectangle.Empty)
            {
                // position ourselves under PlotGrounded, if it exists
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
                    // otherwise position ourselves where PlotGrounded would be
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

            this.drawTextAt(eight, $"Tracking {game.systemBody.bookmarks?.Count} targets:", GameColors.fontSmall);
            newLine(+eight, true);

            // measure width of all names
            var names = this.trackers.Keys.Select(name => getDisplayName(name)).ToArray();
            var maxW = Util.maxWidth(this.Font, names);
            var indent = maxW + ten;
            const int bearingWidth = 75;
            indent += (this.Width - indent) % bearingWidth;
            if (indent < sixty) indent = sixty;

            foreach (var name in this.trackers.Keys)
            {
                var x = indent;

                var fullName = BioScan.prefixes.GetValueOrDefault(name);
                var isActive = game.cmdr.scanOne?.genus == null || game.cmdr.scanOne?.genus == fullName;
                var isClose = false;
                Brush brush;
                foreach (var dd in this.trackers[name])
                {
                    isClose |= dd.distance < highlightDistance;
                    brush = isActive ? GameColors.brushGameOrange : GameColors.brushGameOrangeDim;
                    if (dd.distance < highlightDistance) brush = isActive ? GameColors.brushCyan : GameColors.brushDarkCyan;

                    var pen = isActive ? GameColors.penGameOrange2 : GameColors.penGameOrangeDim2;
                    if (dd.distance < highlightDistance) pen = isActive ? GameColors.penCyan2 : GameColors.penDarkCyan1;

                    var deg = dd.angle - game.status!.Heading;
                    this.drawBearingTo(x, dty, "", (double)dd.distance, (double)deg, brush, pen);
                    x += bearingWidth;
                    if (x > this.Width - bearingWidth) break;
                }

                var displayName = getDisplayName(name);

                var col = isActive ? GameColors.Orange : GameColors.OrangeDim;
                if (isClose) col = isActive ? GameColors.Cyan : GameColors.DarkCyan;

                this.drawTextAt2(indent, displayName, col, null, true);
                newLine(+nine, true);
            }

            this.formSize.Width = this.Width;
            formAdjustSize(0, +four);
        }

        private string getDisplayName(string name)
        {
            var prefix = BioScan.prefixes.GetValueOrDefault(name);
            if (prefix != null)
                return BioScan.genusNames.GetValueOrDefault(prefix) ?? name;
            else
                return name;
        }
    }
}
