﻿using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;
using System.Text;

namespace SrvSurvey
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        int rowHeight = scaled(20);
        public const int highlightDistance = 150;

        public Dictionary<string, List<TrackingDelta>> trackers = new Dictionary<string, List<TrackingDelta>>();

        /*
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

            var parts = msg.Substring(offset).Split(' ', 1, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

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
                    Game.log($"Group '{name}' has too many entries. Ignoring location: {location}");
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
        */

        private PlotTrackers() : base()
        {
            this.Width = scaled(380);
            this.Height = scaled(100);
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
            base.OnLoad(e);
            var gameRect = Elite.getWindowRect(true);

            this.prepTrackers();
            this.setNewHeight();
            this.initialize();
            this.reposition(gameRect);
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
                Program.closePlotter<PlotTrackers>();
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
                        Game.log($"Auto removing tracker for: '{BioScan.genusNames[entry.Genus]}'/'{entry.Genus}'");
                        game.removeBookmark(prefix, Status.here.clone(), true);
                        this.prepTrackers();
                        // processCommand($"-{prefix}", Status.here.clone()); // TODO: retire
                    }
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                if (this.IsDisposed || game?.systemData == null || game.systemBody == null || game.systemBody.bookmarks == null || game.systemBody.bookmarks?.Count == 0) return;

                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;
                base.OnPaintBackground(e);

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
                        if (dd.distance < highlightDistance) brush = isActive ? GameColors.brushCyan : Brushes.DarkCyan;

                        var pen = isActive ? GameColors.penGameOrange2 : GameColors.penGameOrangeDim2;
                        if (dd.distance < highlightDistance) pen = isActive ? GameColors.penCyan2 : Pens.DarkCyan;

                        this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                        x += bearingWidth;
                    }

                    string? displayName;
                    if (!BioScan.prefixes.ContainsKey(name) || !BioScan.genusNames.TryGetValue(BioScan.prefixes[name], out displayName))
                        displayName = name;

                    var sz = g.MeasureString(displayName, GameColors.fontSmall);
                    brush = isActive ? GameColors.brushGameOrange : GameColors.brushGameOrangeDim;
                    if (isClose) brush = isActive ? GameColors.brushCyan : Brushes.DarkCyan;

                    g.DrawString(
                        displayName,
                        GameColors.fontSmall,
                        brush,
                        this.Width - indent - sz.Width + scaled(3), y);
                }
            }
            catch (Exception ex)
            {
                Game.log($"PlotTrackers.OnPaintBackground error: {ex}");
            }
        }
    }

}
