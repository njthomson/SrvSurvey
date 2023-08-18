using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotTrackers : PlotBase, PlotterForm
    {
        const int rowHeight = 20;

        public static void processCommand(string msg)
        {
            if (!msg.StartsWith(MsgCmd.track) || Game.activeGame == null) return;

            var parts = msg.Substring(MsgCmd.track.Length + 1).Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            var name = parts[0];
            var verb = parts.Length == 2 ? parts[1] : "here";

            // stop showing target tracking
            if (name == "off")
            {
                Program.closePlotter<PlotTrackers>();
                Game.activeGame.cmdr.trackTargets = null;
                Game.activeGame.cmdr.Save();
                return;
            }

            // create tracker if needed
            if (Game.activeGame.cmdr.trackTargets == null)
                Game.activeGame.cmdr.trackTargets = new Dictionary<string, LatLong2>();

            var targets = Game.activeGame.cmdr.trackTargets;

            if (verb == "here")
            {
                targets[name] = Status.here.clone();
            }
            else if (verb == "off")
            {
                targets.Remove(name);
            }
            else
            {
                // TODO: split pasted lat/long co-ordinates?
            }

            if (targets.Count > 0)
            {
                var form = Program.showPlotter<PlotTrackers>();

                // adjust height if needed
                var formHeight = 42 + (targets.Count * rowHeight);
                if (form.Height != formHeight)
                {
                    form.Height = formHeight;
                    form.BackgroundImage = GameGraphics.getBackgroundForForm(form);

                    form.Invalidate();
                }
            }
            else
            {
                Program.closePlotter<PlotTrackers>();
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

            var y = 12;
            foreach (var target in game.cmdr.trackTargets)
            {
                y += rowHeight;

                var sz = g.MeasureString(target.Key, Game.settings.fontSmall);
                var x = this.Width - 110 - sz.Width;

                var dd = new TrackingDelta(game.nearBody!.radius, target.Value);
                Angle deg = dd.angle - game.status!.Heading;

                var brush = dd.distance < 100 ? GameColors.brushCyan : null;
                var pen = dd.distance < 100 ? GameColors.penCyan2 : null;

                this.drawBearingTo(x, y, target.Key, (double)dd.distance, (double)deg, brush, pen);
            }
        }
    }

}
