using SrvSurvey.game;
using SrvSurvey.units;
using System.CodeDom.Compiler;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace SrvSurvey
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        const int rowHeight = 20;
        const int highlightDistance = 250;

        public static void processCommand(string msg)
        {
            if (Game.activeGame == null || !(msg.StartsWith(MsgCmd.trackAdd) || msg.StartsWith(MsgCmd.trackRemove))) return;

            if (msg.StartsWith("---"))
            {
                // stop showing target tracking
                Program.closePlotter<PlotTrackers>();
                Game.activeGame.cmdr.trackTargets = null;
                Game.activeGame.cmdr.Save();
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

            // create tracker if needed
            if (Game.activeGame.cmdr.trackTargets == null)
                Game.activeGame.cmdr.trackTargets = new Dictionary<string, List<LatLong2>>();
            var targets = Game.activeGame.cmdr.trackTargets;

            if (verb == "add")
            {
                if (!targets.ContainsKey(name))
                    targets[name] = new List<LatLong2>();

                // only add if less than 4 entries
                if (targets[name].Count < 4)
                {
                    Game.log($"Add to group '{name}': {Status.here}");
                    targets[name].Add(Status.here.clone());
                }
                else
                {
                    Game.log($"Group '{name}' has too many entries");
                }
            }
            else if (targets.ContainsKey(name))
            {
                if (verb == "clear" || (verb == "remove" && targets[name].Count == 1))
                {
                    // remove the whole group
                    targets.Remove(name);
                    Game.log($"Removing group '{name}'");
                }
                else if (verb == "remove")
                {
                    // remove the closest entry
                    var radius = (decimal)Game.activeGame.nearBody!.radius;
                    decimal minDist = decimal.MaxValue;
                    LatLong2 minEntry = null!;
                    foreach (var _ in targets[name])
                    {
                        var dist = Util.getDistance(_, Status.here, radius);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minEntry = _;
                        }
                    }
                    Game.log($"Removing closest entry from group '{name}': {minEntry}");
                    targets[name].Remove(minEntry);
                }
            }
            else
            {
                Game.log($"Group not found: '{name}'");
            }

            if (targets.Count > 0)
            {
                // show and adjust height if needed
                var form = Program.showPlotter<PlotTrackers>();
                form.setNewHeight();
            }
            else
            {
                Program.closePlotter<PlotTrackers>();
            }
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

                this.Invalidate();
            }
        }

        private PlotTrackers() : base()
        {
            this.Width = 360;
            this.Height = 100;
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

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            base.Game_modeChanged(newMode, force);
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

            // ?

            base.Status_StatusChanged(blink);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game.nearBody == null) return;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            g.DrawString($"Tracking {game.cmdr.trackTargets?.Count} targets:", Game.settings.fontSmall, GameColors.brushGameOrange, 4, 8);

            if (game.cmdr.trackTargets == null) return;

            int indent = 225 + 80;

            var y = 12;
            foreach (var target in game.cmdr.trackTargets)
            {
                y += rowHeight;

                var sz = g.MeasureString(target.Key, Game.settings.fontSmall);
                var x = (float)this.Width - indent; // - sz.Width;


                var dd = new TrackingDelta(game.nearBody!.radius, target.Value[0]);
                Angle deg = dd.angle - game.status!.Heading;

                var brush = dd.distance < highlightDistance ? GameColors.brushCyan : GameColors.brushGameOrange;
                var pen = dd.distance < highlightDistance ? GameColors.penCyan2 : null;

                //g.DrawString(target.Key, Game.settings.fontSmall, brush, x, y);

                var isClose = false;
                var xx = 75;
                foreach (var pos in target.Value)
                {
                    dd = new TrackingDelta(game.nearBody!.radius, pos);
                    deg = dd.angle - game.status!.Heading;

                    isClose |= dd.distance < highlightDistance;
                    brush = dd.distance < highlightDistance ? GameColors.brushCyan : GameColors.brushGameOrange;
                    pen = dd.distance < highlightDistance ? GameColors.penCyan2 : null;

                    this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                    x += xx;
                }

                x = this.Width - indent - sz.Width;
                brush = isClose ? GameColors.brushCyan : GameColors.brushGameOrange;
                g.DrawString(target.Key, Game.settings.fontSmall, brush, x, y);



                //this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                //x += sz.Width;
                //x += xx;
                //this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                //x += xx;
                //this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
                //x += xx;
                //this.drawBearingTo(x, y, "", (double)dd.distance, (double)deg, brush, pen);
            }
        }
    }

}
