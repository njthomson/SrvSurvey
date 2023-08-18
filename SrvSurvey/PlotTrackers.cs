using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
            if (name == "off" || name == "reset")
            {
                Program.closePlotter<PlotTrackers>();
                Game.activeGame.cmdr.trackTargets = null;
                Game.activeGame.cmdr.Save();
                return;
            }

            // create tracker if needed
            var targets = Game.activeGame.cmdr.trackTargets?.targets;
            if (targets == null)
            {
                Game.activeGame.cmdr.trackTargets = new TrackTargets();
                targets = Game.activeGame.cmdr.trackTargets?.targets!;
            }
            var showPlotter = targets.Count > 0;

            //var targets = Game.activeGame.cmdr.trackTargets?.targets;
            if (verb == "here")
            {
                targets[name] = Status.here.clone();
                showPlotter = true;
            }
            else if (verb == "off")
            {
                targets.Remove(name);
            }
            else
            {
                // TODO: split pasted lat/long co-ordinates?
            }

            if (showPlotter)
            {
                var form = Program.showPlotter<PlotTrackers>();

                // adjust height if needed
                var formHeight = 52 + (targets.Count * rowHeight);
                if (form.Height != formHeight)
                {
                    form.Height = formHeight;
                    form.BackgroundImage = GameGraphics.getBackgroundForForm(form);

                    form.Invalidate();
                }
            }
        }

        private PlotTrackers() : base()
        {
            this.Width = 300;
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

        public override void reposition(Rectangle gameRect)
        {
            gameRect = new Rectangle(400, 400, 400, 400);

            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatRightBottom(this, gameRect);

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
            if (this.IsDisposed) return;
            this.Opacity = 1;

            this.g = e.Graphics;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnPaintBackground(e);

            this.drawHeaderText($"Tracking {game.cmdr.trackTargets?.targets?.Count} targets:");
            if (game.cmdr.trackTargets?.targets == null) return;

            var y = 24;
            foreach (var target in game.cmdr.trackTargets.targets)
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
