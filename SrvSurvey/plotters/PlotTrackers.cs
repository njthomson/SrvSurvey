using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using Res = Loc.PlotTrackers;

namespace SrvSurvey.plotters
{
    internal class PlotTrackers : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotTrackers),
            allowed = allowed,
            ctor = (game, def) => new PlotTrackers(game, def),
            defaultSize = new Size(380, 100),
        };

        public static bool allowed(Game game)
        {
            return PlotGrounded.allowed(game)
                // same as PlotGrounded + do we have any bookmarks?
                && (
                    (!Game.settings.autoShowPlotMiniTrack_TEST && Game.activeGame?.systemBody?.bookmarks?.Count > 0)
                || (Game.settings.autoShowPlotMiniTrack_TEST && Game.activeGame?.systemBody?.bookmarks?.Keys.Count(k => k[0] != '#') > 0)
                );
        }

        #endregion

        public const int highlightDistance = 150;

        public Dictionary<string, List<TrackingDelta>> trackers = new Dictionary<string, List<TrackingDelta>>();

        private PlotTrackers(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;

            this.prepTrackers();
        }

        public void prepTrackers()
        {
            if (this.isClosed) return;

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

            PlotBase2.invalidate(nameof(PlotGrounded));

            if (this.trackers.Count == 0)
                Program.defer(() => PlotBase2.remove(PlotTrackers.def));
        }

        public override void setSize(int width, int height)
        {
            var plotGrounded = PlotBase2.getPlotter<PlotGrounded>();
            if (plotGrounded != null)
                width = plotGrounded.width;

            base.setSize(width, height);
        }

        public override void setPosition(Rectangle? gameRect = null, string? name = null)
        {
            // NOT calling base class

            if (gameRect != Rectangle.Empty)
            {
                // position ourselves under PlotGrounded, if it exists
                var plotGrounded = PlotBase2.getPlotter<PlotGrounded>();
                if (plotGrounded != null)
                {
                    this.left = plotGrounded.left;
                    this.top = plotGrounded.bottom + (int)N.four;
                }
                else
                {
                    // otherwise position ourselves where PlotGrounded would be
                    base.setPosition(gameRect, nameof(PlotGrounded));
                }

                this.invalidate();
            }
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            // re-calc distances and re-order TrackingDeltas
            foreach (var name in this.trackers.Keys)
            {
                this.trackers[name].ForEach(_ => _.calc());
                this.trackers[name].Sort((a, b) => a.distance.CompareTo(b.distance));
            }

            if (status.changed.Contains(nameof(Status.Heading)) || status.changed.Contains(nameof(Status.Latitude)) || status.changed.Contains(nameof(Status.Longitude)))
                this.invalidate();
        }

        protected override void onJournalEntry(ScanOrganic entry)
        {
            if (game.systemBody == null) return;

            // if we are close enough to a tracker ... auto remove it
            if (this.trackers.ContainsKey(entry.Genus))
            {
                var td = this.trackers[entry.Genus].FirstOrDefault();
                if (td != null)
                {
                    Game.log($"Distance to nearest '{entry.Genus}' tracker: {Util.metersToString(td.distance)}");

                    //var radius = BioScan.ranges.ContainsKey(entry.Genus) ? BioScan.ranges[entry.Genus] : highlightDistance;
                    if (td.distance < highlightDistance && Game.settings.autoRemoveTrackerOnSampling)
                    {
                        Game.log($"Auto removing tracker for: '{entry.Species_Localised}'/'{entry.Genus}'");
                        game.removeBookmark(entry.Genus, Status.here.clone(), true);
                        this.prepTrackers();
                    }
                }
            }
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            if (game?.systemData == null || game.systemBody == null || game.systemBody.bookmarks == null || game.systemBody.bookmarks?.Count == 0) return frame.Size;

            tt.draw(N.eight, Res.HeaderLine.format(game.systemBody.bookmarks?.Count ?? 0), GameColors.fontSmall);
            tt.newLine(+N.eight, true);

            // measure width of all names
            var names = this.trackers.Keys.Select(name => getDisplayName(name)).ToArray();
            var maxW = Util.maxWidth(this.font, names);
            var indent = maxW + N.ten;
            const int bearingWidth = 75;
            indent += (this.width - indent) % bearingWidth;
            if (indent < N.sixty) indent = N.sixty;

            foreach (var name in this.trackers.Keys)
            {
                // skip quick trackers if PlotMiniTrack is active
                if (name[0] == '#' && PlotBase2.isPlotter<PlotMiniTrack>()) continue;

                var x = indent + N.eight;
                var isActive = game.cmdr.scanOne?.genus == null || game.cmdr.scanOne?.genus == name;
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
                    BaseWidget.renderBearingTo(g, x, tt.dty, N.five, (double)deg, Util.metersToString(dd.distance), brush, pen);
                    x += bearingWidth;
                    if (x > this.width - bearingWidth) break;
                }

                var displayName = getDisplayName(name);

                var col = isActive ? GameColors.Orange : GameColors.OrangeDim;
                if (isClose) col = isActive ? GameColors.Cyan : GameColors.DarkCyan;

                tt.draw(indent, displayName, col, null, true);
                tt.newLine(+N.nine, true);
            }

            return tt.pad(0, +N.four);
        }

        private string getDisplayName(string name)
        {
            var genus = Game.codexRef.genus.Find(g => g.name == name);
            return genus?.locName ?? genus?.englishName ?? name;
        }
    }
}
