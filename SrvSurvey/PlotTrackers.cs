using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;
using System.Text;

namespace SrvSurvey
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        const int rowHeight = 20;
        public const int highlightDistance = 150;

        public Dictionary<string, List<TrackingDelta>> trackers = new Dictionary<string, List<TrackingDelta>>();

        public static void processCommand(string msg, LatLong2 location)
        {
            if (Game.activeGame == null || !(msg.StartsWith(MsgCmd.trackAdd) || msg.StartsWith(MsgCmd.trackRemove) || msg.StartsWith(MsgCmd.trackRemoveLast))) return;
            var cmdr = Game.activeGame.cmdr;

            if (msg.StartsWith("---"))
            {
                // stop showing target tracking
                Program.closePlotter<PlotTrackers>();
                cmdr.trackTargets = null;
                cmdr.Save();
                return;
            }

            string verb = null!;
            var offset = 1;
            if (msg.StartsWith("+")) verb = "add";
            if (msg.StartsWith("-")) verb = "remove";
            if (msg.StartsWith("=")) verb = "removeLast";
            if (msg.StartsWith("--")) { verb = "clear"; offset = 2; }

            var parts = msg.Substring(offset).Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (verb == null || parts.Length == 0) return;
            var name = parts[0]?.ToLowerInvariant()!;

            if (BioScan.prefixes.ContainsKey(name))
                name = BioScan.prefixes[name]; // name is now the full Genus name, eg: $Codex_Ent_Bacterial_Genus_Name;

            // create tracker if needed
            if (cmdr.trackTargets == null)
                cmdr.trackTargets = new Dictionary<string, List<LatLong2>>();

            var form = Program.getPlotter<PlotTrackers>();

            if (verb == "add")
            {
                if (!cmdr.trackTargets.ContainsKey(name))
                    cmdr.trackTargets[name] = new List<LatLong2>();

                // only add if less than 4 entries
                if (cmdr.trackTargets[name].Count < 4)
                {
                    Game.log($"Add to group '{name}': {location}");

                    var pos = location;
                    cmdr.trackTargets[name].Add(pos);
                    cmdr.Save();

                    if (form != null)
                    {
                        if (!form.trackers.ContainsKey(name)) form.trackers[name] = new List<TrackingDelta>();
                        form.trackers[name].Insert(0, new TrackingDelta(Game.activeGame.nearBody!.radius, pos));
                    }
                }
                else
                {
                    Game.log($"Group '{name}' has too many entries");
                }
            }
            else if (cmdr.trackTargets.ContainsKey(name))
            {
                if (verb == "clear" || (verb == "remove" && cmdr.trackTargets[name].Count == 1))
                {
                    // remove the whole group
                    cmdr.trackTargets.Remove(name);
                    cmdr.Save();

                    if (form != null) form.trackers.Remove(name);

                    Game.log($"Removing group '{name}'");
                }
                else if (verb == "remove")
                {
                    // remove the closest entry
                    var radius = (decimal)Game.activeGame.nearBody!.radius;
                    decimal minDist = decimal.MaxValue;
                    LatLong2 minEntry = null!;
                    foreach (var _ in cmdr.trackTargets[name])
                    {
                        var dist = Util.getDistance(_, location, radius);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minEntry = _;
                        }
                    }
                    Game.log($"Removing closest entry from group '{name}': {minEntry}");
                    cmdr.trackTargets[name].Remove(minEntry);
                    cmdr.Save();

                    if (form != null) form.trackers[name].RemoveAt(0);
                }
                else if (verb == "removeLast")
                {
                    // remove the furthest entry
                    var radius = (decimal)Game.activeGame.nearBody!.radius;
                    decimal maxDist = 0;
                    LatLong2 maxEntry = null!;
                    foreach (var _ in cmdr.trackTargets[name])
                    {
                        var dist = Util.getDistance(_, location, radius);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            maxEntry = _;
                        }
                    }

                    Game.log($"Removing furthest entry from group '{name}': {maxEntry}");
                    cmdr.trackTargets[name].Remove(maxEntry);
                    cmdr.Save();

                    if (form != null) form.trackers[name].RemoveAt(form.trackers.Count - 1);
                }
            }
            else
            {
                Game.log($"Group not found: '{name}'");
            }

            if (cmdr.trackTargets.Count > 0)
            {
                // show and adjust height if needed
                form = Program.showPlotter<PlotTrackers>();
                form.setNewHeight();
                Program.showPlotter<PlotGrounded>();
            }
            else
            {
                Program.closePlotter<PlotTrackers>();
            }
        }

        private PlotTrackers() : base()
        {
            this.Width = 380;
            this.Height = 100;
        }

        private void setNewHeight()
        {
            if (game.cmdr.trackTargets == null) return;

            // adjust height if needed
            var formHeight = 42 + (game.cmdr.trackTargets.Count * rowHeight);
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

            this.prepTrackers();
            this.setNewHeight();
            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        private void prepTrackers()
        {
            if (game.cmdr.trackTargets == null) return;

            foreach (var name in game.cmdr.trackTargets.Keys)
            {
                if (!this.trackers.ContainsKey(name))
                {
                    // create group and TrackingDelta's for each location
                    this.trackers.Add(name, new List<TrackingDelta>());

                    foreach (var pos in game.cmdr.trackTargets[name])
                        this.trackers[name].Add(new TrackingDelta(game.nearBody!.radius, pos));
                }
            }
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
            if (plotGrounded != null)
            {
                this.Width = plotGrounded.Width;
                this.Left = gameRect.Right - this.Width - 20;
                this.Top = plotGrounded.Bottom + 4;
            }
            else
            {
                Elite.floatRightMiddle(this, gameRect, 20);
            }

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
                        Game.log($"Auto removing tracker for: '{BioScan.genusNames[entry.Genus]}'/'{entry.Genus}'");
                        var prefix = BioScan.prefixes.First(_ => _.Value.Contains(entry.Genus)).Key;
                        processCommand($"-{prefix}", Status.here.clone());
                    }
                }
            }
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game.nearBody == null) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            g.DrawString($"Tracking {game.cmdr.trackTargets?.Count} targets:", Game.settings.fontSmall, GameColors.brushGameOrange, 4, 8);

            if (game.cmdr.trackTargets == null) return;

            var indent = 220 + 80;
            var y = 12;
            foreach (var name in this.trackers.Keys)
            {
                y += rowHeight;

                var x = (float)this.Width - indent;

                var isActive = game.cmdr.scanOne?.genus == null || game.cmdr.scanOne?.genus == name;
                var isClose = false;
                var bearingWidth = 75;
                Brush brush;
                foreach (var dd in this.trackers[name])
                {
                    var deg = dd.angle - game.status!.Heading;
                    //var radius = BioScan.ranges.ContainsKey(name) ? BioScan.ranges[name] : highlightDistance;

                    isClose |= dd.distance < highlightDistance;
                    brush = isActive ? GameColors.brushGameOrange : GameColors.brushGameOrangeDim;
                    if (dd.distance < highlightDistance) brush = isActive ? GameColors.brushCyan : Brushes.DarkCyan;

                    var pen = isActive ? GameColors.penGameOrange2 : GameColors.penGameOrangeDim2;
                    if (dd.distance < highlightDistance) pen = isActive ? GameColors.penCyan2 : Pens.DarkCyan;

                    this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                    x += bearingWidth;
                }

                string? displayName;
                if (!BioScan.genusNames.TryGetValue(name, out displayName))
                    displayName = name;

                var sz = g.MeasureString(displayName, Game.settings.fontSmall);
                brush = isActive ? GameColors.brushGameOrange : GameColors.brushGameOrangeDim;
                if (isClose) brush = isActive ? GameColors.brushCyan : Brushes.DarkCyan;

                g.DrawString(
                    displayName,
                    Game.settings.fontSmall,
                    brush,
                    this.Width - indent - sz.Width + 3, y);
            }
        }
    }

}
