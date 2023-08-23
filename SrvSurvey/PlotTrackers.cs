using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        const int rowHeight = 20;
        const int highlightDistance = 250;

        public Dictionary<string, List<TrackingDelta>> trackers = new Dictionary<string, List<TrackingDelta>>();

        public static void processCommand(string msg)
        {
            if (Game.activeGame == null || !(msg.StartsWith(MsgCmd.trackAdd) || msg.StartsWith(MsgCmd.trackRemove))) return;
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
            if (msg.StartsWith("--")) { verb = "clear"; offset = 2; }

            var parts = msg.Substring(offset).Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (verb == null || parts.Length == 0) return;
            var name = parts[0];

            // auto-expand 3 letter genus into their full names
            switch (name.ToLowerInvariant())
            {
                // From Odyssey
                case "ale": name = "Aleoids"; break; // Aleoida
                case "bac": name = "Bacterial"; break; // Bacterium
                case "cac": name = "Cactoid"; break; // Cactoida
                case "cly": name = "Clypeus"; break; // Clypeus
                case "con": name = "Clypeus"; break; // Concha
                case "ele": name = "Conchas"; break; // Electricae
                case "fon": name = "Electricae"; break; // Fonticulua
                case "fru": name = "Shrubs"; break; // Frutexa
                case "fum": name = "Fumerolas"; break; // Fumerola
                case "fun": name = "Fungoids"; break; // Fungoida
                case "oss": name = "Osseus"; break; // Osseus
                case "rec": name = "Recepta"; break; // Recepta
                case "str": name = "Stratum"; break; // Stratum
                case "tub": name = "Tubus"; break; // Tubus
                case "tus": name = "Tussocks"; break; // Tussock

                // From Horizons
                case "amp": name = "Vents"; break; // Amphora Plant
                case "bra": name = "Brancae"; break; // Brain Tree
                case "sin": name = "Tube"; break; // Sinuous Tubers
                case "cry": name = "Ground"; break; // Crystalline Shards
                case "ane": name = "Sphere"; break; // Anemone
                case "bar": name = "Cone"; break; // Bark Mounds

            }

            if (BioScan.genusNames.ContainsKey(name)) name = BioScan.genusNames[name];

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
                    Game.log($"Add to group '{name}': {Status.here}");

                    var pos = Status.here.clone();
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
                        var dist = Util.getDistance(_, Status.here, radius);
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
            string? name;
            if (BioScan.genusNames.TryGetValue(entry.Genus.Split('_')[2], out name) && this.trackers.ContainsKey(name))
            {
                var td = this.trackers[name].FirstOrDefault();
                if (td != null)
                {
                    Game.log($"Distance to nearest '{name}' tracker: {Util.metersToString(td.distance)}");

                    if (td.distance < highlightDistance)
                    {
                        Game.log($"Auto removing tracker for: '{name}'/'{entry.Genus}'");
                        processCommand($"-{name}");
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

                var isClose = false;
                var bearingWidth = 75;
                foreach (var dd in this.trackers[name])
                {
                    var deg = dd.angle - game.status!.Heading;

                    isClose |= dd.distance < highlightDistance;
                    var brush = dd.distance < highlightDistance ? GameColors.brushCyan : GameColors.brushGameOrange;
                    var pen = dd.distance < highlightDistance ? GameColors.penCyan2 : null;

                    this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                    x += bearingWidth;
                }

                var sz = g.MeasureString(name, Game.settings.fontSmall);
                g.DrawString(
                    name,
                    Game.settings.fontSmall,
                    isClose ? GameColors.brushCyan : GameColors.brushGameOrange,
                    this.Width - indent - sz.Width + 3, y);
            }
        }
    }

}
