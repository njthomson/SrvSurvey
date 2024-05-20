using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotGalMap : PlotBase, PlotterForm
    {
        private static bool smaller = true; // temp?
        private static GraphicsPath triangle;
        private static bool fancy = false;

        static PlotGalMap()
        {
            triangle = new GraphicsPath();
            triangle.AddPolygon(new Point[] {
                    scaled(new Point(0, 0)),
                    scaled(new Point(0, smaller ? 20 : 36)),
                    scaled(new Point(smaller ? 8 : 12,  smaller ? 10 : 18)),
                });
        }

        private Size boxSz;
        private List<RouteInfo> hops = new List<RouteInfo>();
        private double distanceJumped;

        private string? targetSystem;
        private string? targetStatus;
        private string? targetSubStatus;

        private string? nextSystem;
        private string? nextStatus;
        private string? nextSubStatus;

        private PlotGalMap() : base()
        {
            if (fancy)
            {
                this.Height = smaller ? scaled(36) : scaled(66);
                this.Font = GameColors.font18;

                // find the black box near the bottom of the screen, position ourself below it and a bit narrower
                findBlackBox();
                Game.log(boxSz);
                if (boxSz.Width > 300)
                {
                    this.Width = boxSz.Width - 40;
                }
                else
                {
                    // failed to match?
                    boxSz = Size.Empty;
                    this.Opacity = 0;
                }
            }
            else
            {
                this.MinimumSize = new Size(scaled(96), scaled(44));
                this.Font = GameColors.fontSmall2;
            }
        }

        private void findBlackBox()
        {
            var gameRect = Elite.getWindowRect();

            // don't bother if we have no rectangle to analyze
            if (gameRect.Width == 0 || gameRect.Height == 0) return;

            var hh = (int)(gameRect.Height * 0.5f);
            var watchRect = new Rectangle(
                gameRect.Left, gameRect.Top + hh,
                gameRect.Width, hh);

            var boxColor = Color.Black;
            var x = gameRect.Width / 2;
            var y = watchRect.Height - 4;
            using (var b = new Bitmap(watchRect.Width, watchRect.Height))
            {
                using (var g = Graphics.FromImage(b))
                {
                    g.CopyFromScreen(watchRect.Left, watchRect.Top, 0, 0, b.Size);

                    // up up until we hit pure black
                    while (y > 0 && !Util.isCloseColor(b.GetPixel(x, y), boxColor, 1)) y -= 2;

                    // exit early if we did not find the black box
                    if (y == 0) return;
                    boxSz.Height = hh - y;

                    // go left and right until we find not-black
                    while (x > 0 && Util.isCloseColor(b.GetPixel(x, y), boxColor, 1)) x -= 1;
                    var x1 = x;

                    x = gameRect.Width / 2;
                    while (x < b.Width && Util.isCloseColor(b.GetPixel(x, y), boxColor, 1)) x += 1;
                    boxSz.Width = x - x1;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initialize();
            // actively suppress the standard plotter background?
            if (fancy) this.BackgroundImage = null;

            this.reposition(Elite.getWindowRect(true));
            this.onJournalEntry(new NavRoute());
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap
                && Game.settings.useExternalData;
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            if (fancy)
            {
                if (boxSz == Size.Empty)
                {
                    // we failed to find the box properly
                    this.Opacity = 0;
                }
                else
                {
                    // This plotter does not honor typical opacity settings, and chooses it's own position
                    this.Opacity = PlotPos.getOpacity(this, 1);

                    // be horizontally centered, just below the game's black box
                    this.Left = gameRect.Left + ((gameRect.Width / 2) - (this.Width / 2));
                    this.Top = gameRect.Bottom - boxSz.Height + 4;

                    this.Invalidate();
                }
            }
            else
            {
                this.Opacity = PlotPos.getOpacity(this);
                PlotPos.reposition(this, gameRect);
                this.Invalidate();
            }
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            var showPlotter = PlotGalMap.allowPlotter;
            if (this.Opacity > 0 && !showPlotter)
                Program.closePlotter<PlotGalMap>();
            else if (this.Opacity == 0 && showPlotter)
                this.reposition(Elite.getWindowRect());
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (this.IsDisposed) return;
            this.hops.Clear();

            // lookup if target system has been discovered
            if (game.navRoute.Route.Count < 2)
                return;

            // the desintation is last
            this.hops.Add(RouteInfo.create(game.navRoute.Route.Last(), true));


            var next = game.fsdTarget != null
                ? game.navRoute.Route.Find(_ => _.StarSystem == game.fsdTarget) ?? game.navRoute.Route[1]
                : game.navRoute.Route[1];

            if (game.navRoute.Route.Count > 2)
                this.hops.Add(RouteInfo.create(next, false));

            this.distanceJumped = 0;
            for (int n = 1; n < game.navRoute.Route.Count; n++)
            {
                var d = Util.getSystemDistance(game.navRoute.Route[n - 1].StarPos, game.navRoute.Route[n].StarPos);
                this.distanceJumped += d;
            }

            //var target = game.navRoute.Route.LastOrDefault()?.StarSystem;
            //this.hops.Add(new RouteInfo(target, this));

            //var next = game.navRoute.Route.Count == 0 ? null : game.navRoute.Route[1].StarSystem;
            //this.hops.Add(new RouteInfo(next, this));

            //if (target != null)
            //    this.lookupSystem(target, false);
            //else
            //{
            //    this.targetSystem = null;
            //    this.targetStatus = null;
            //    this.targetSubStatus = null;
            //}

            //if (next != null && target != next)
            //    this.lookupSystem(next, true);
            //else
            //{
            //    this.nextSystem = null;
            //    this.nextStatus = null;
            //    this.nextSubStatus = null;
            //}
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (this.IsDisposed) return;
            this.hops.Clear();

            nextStatus = null;
            nextSubStatus = null;
            targetStatus = null;
            targetSubStatus = null;

            this.Invalidate();
        }

        private void lookupSystem(string systemName, bool isNext)
        {
            if (isNext)
            {
                nextSystem = systemName;
                nextStatus = "...";
                nextSubStatus = "";
            }
            else
            {
                targetSystem = systemName;
                targetStatus = "...";
                targetSubStatus = "";
            }
            this.Invalidate();

            // lookup in EDSM
            Game.edsm.getBodies(systemName).ContinueWith(result =>
            {
                if (result.Exception != null)
                {
                    Util.isFirewallProblem(result.Exception);
                    return;
                }

                var response = result.Result;

                var status = "";
                var subStatus = "";
                if (response.name == null || response.id64 == 0)
                {
                    // system is not known to EDSM
                    status = "Undiscovered system";
                }
                else if (response.bodyCount == 0)
                {
                    // system is known from routes but it has not been visited
                    status = "Unvisited system";
                }
                else
                {
                    if (response.bodyCount == response.bodies.Count)
                        status = $"Discovered, {response.bodyCount} bodies";
                    else
                        status = $"Discovered ({response.bodies.Count} of {response.bodyCount} bodies)";

                    var discCmdr = response.bodies.FirstOrDefault()?.discovery?.commander;
                    var discDate = response.bodies.FirstOrDefault()?.discovery?.date.ToShortDateString();
                    if (discCmdr != null && discDate != null)
                        subStatus = $"By {discCmdr}, {discDate}";
                }

                if (isNext)
                {
                    this.nextStatus = status;
                    this.nextSubStatus = subStatus;
                }
                else
                {
                    this.targetStatus = status;
                    this.targetSubStatus = subStatus;
                }

                if (this.Created)
                {
                    this.BeginInvoke(() =>
                    {
                        this.Invalidate();
                    });
                }

                // TODO: maybe lookup in Canonn for bio data too?
            });
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;
            try
            {
                this.resetPlotter(g);

                if (fancy)
                {
                    this.drawFancy();
                    return;
                }

                if (this.hops.Count == 0)
                {
                    this.drawTextAt(eight, $"No route set");
                    this.newLine(+ten, true);
                }
                else
                {
                    foreach (var hop in this.hops)
                        drawSystemSummary(hop);// "Next jump", nextSystem, nextStatus, nextSubStatus);


                    this.drawTextAt(eight, $"Total jumps: {game.navRoute.Route.Count - 1} ► Distance: {this.distanceJumped.ToString("N1")} ly", GameColors.brushGameOrange);
                    this.newLine(true);

                    this.drawTextAt(eight, $"Data from: edsm.net + spansh.co.uk", GameColors.brushGameOrangeDim);
                    this.newLine(true);
                }

                //// draw a more pedestrian UX
                //dty = ten;

                //drawSystemSummary("Next jump", nextSystem, nextStatus, nextSubStatus);

                //if (this.targetStatus == null)
                //{
                //    this.drawTextAt(eight, $"No route set");
                //    this.newLine(+ten, true);
                //}
                //else
                //{
                //    drawSystemSummary("Destination", targetSystem, targetStatus, targetSubStatus);
                //}

                this.formAdjustSize(+ten, +ten);
            }
            catch (Exception ex)
            {
                Game.log($"PlotGalMap.OnPaintBackground error: {ex}");
            }
        }

        private void drawSystemSummary(RouteInfo hop)
        {
            if (hop.destination)
                this.drawTextAt(eight, $"Destination: ");
            else
                this.drawTextAt(eight, $"Next jump:");

            // line 1: system name
            this.drawTextAt(eightSix, $"► {hop.systemName}", GameColors.fontSmall2Bold);
            this.newLine(true);

            // line 2: status
            if (hop.highlight)
                this.drawTextAt(eightSix, $"{hop.status}", GameColors.brushCyan, GameColors.fontSmall2Bold);
            else
                this.drawTextAt(eightSix, $"{hop.status}");

            // line 3: who discovered
            if (!string.IsNullOrEmpty(hop.subStatus))
            {
                this.newLine(true);
                this.drawTextAt(eightSix, $"{hop.subStatus}");
            }

            // line 4: bio signals?
            if (hop.sumGenus > 0)
            {
                this.newLine(true);
                this.drawTextAt(eightSix, $"{hop.sumGenus}x Genus", GameColors.brushCyan, GameColors.fontSmall2Bold);
            }

            this.newLine(+ten, true);
        }

        private void drawFancy()
        {
            this.Opacity = PlotPos.getOpacity(this, 1);
            if (boxSz == Size.Empty) return;

            // draw outer gray box
            g.DrawRectangle(bp, 1, 1, this.Width - 2, this.Height - 2);
            g.DrawRectangle(bp2, 2, 2, this.Width - 4, this.Height - 4);

            if (this.targetStatus != null)
                this.drawStatus66(this.targetStatus, this.targetSubStatus, this.Width * 0.715f);

            if (this.nextStatus != null)
                this.drawStatus66(this.nextStatus, this.nextSubStatus, this.Width * 0.34f);

            if (this.targetStatus != null || this.nextStatus != null)
            {
                dtx = eight;
                dty = oneFour;
                drawTextAt("(according to edsm.net)", GameColors.brushGameOrangeDim, GameColors.fontSmall);
            }
        }

        // TODO: add these to GameColours
        private Pen bp = new Pen(Color.FromArgb(255, 59, 56, 58), 2);
        private Pen bp2 = new Pen(Color.FromArgb(255, 39, 36, 38), 1);
        private Pen bp3 = new Pen(Color.FromArgb(255, 29, 26, 28), 2);
        private Brush bb = new SolidBrush(Color.FromArgb(255, 34, 34, 34));
        private Brush tb = new SolidBrush(Color.FromArgb(255, 255, 111, 0));

        private void drawStatus66(string? status, string? subStatus, float x)
        {
            if (smaller) { drawStatus24(status, subStatus, x); return; }

            // next system
            this.dtx = x;
            this.dty = four;

            g.TranslateTransform(dtx, dty + 8);
            g.FillPath(bb, triangle);
            g.DrawPath(bp3, triangle);
            g.TranslateTransform(-dtx, -dty - 8);
            this.dtx = x + 25;

            // big matching fonts
            dty += this.drawTextAt(status!, tb).Height;
            this.dtx = x + 25;
            this.drawTextAt(subStatus!, tb, GameColors.fontMiddle);
        }

        private void drawStatus24(string? status, string? subStatus, float x)
        {
            // next system
            this.dtx = x;
            this.dty = two;

            g.TranslateTransform(dtx, dty + 8);
            g.FillPath(bb, triangle);
            g.DrawPath(bp3, triangle);
            g.TranslateTransform(-dtx, -dty - 8);
            this.dtx = x + 25;

            //// big matching fonts
            //dty += this.drawTextAt(status!, tb).Height;
            //this.dtx = x + 25;
            //this.drawTextAt(subStatus!, tb, GameColors.fontMiddle);

            if (status != "..." && subStatus == "")
            {
                this.drawTextAt(status!, GameColors.brushCyan, GameColors.font18);
            }
            else
            {
                // small
                dty += this.drawTextAt(status!, tb, GameColors.fontMiddle).Height;
                this.dtx = x + 27;
                this.drawTextAt(subStatus!, tb, GameColors.fontSmall);
            }
        }
    }

    class RouteInfo
    {
        private static Dictionary<double, RouteInfo> cache = new Dictionary<double, RouteInfo>();

        public static RouteInfo create(RouteEntry entry, bool destination)
        {
            var info = cache.GetValueOrDefault(entry.SystemAddress);

            if (info == null)
            {
                info = new RouteInfo(entry, destination);
                cache.Add(entry.SystemAddress, info);
            }

            info.destination = destination;
            return info;
        }

        public RouteEntry entry;
        public string status = "...";
        public string? subStatus;
        public string? bio;
        public bool highlight;
        public bool destination;
        public int sumGenus;

        public RouteInfo(RouteEntry entry, bool destination)
        {
            this.entry = entry;
            this.destination = destination;

            this.lookupSystem();
        }

        public string systemName { get => entry.StarSystem; }

        private void lookupSystem()
        {
            // lookup in EDSM
            Game.edsm.getBodies(systemName).ContinueWith(result =>
            {
                if (result.Exception != null)
                {
                    Util.isFirewallProblem(result.Exception);
                    return;
                }
                var edsmResult = result.Result;

                if (edsmResult.name == null || edsmResult.id64 == 0)
                {
                    // system is not known to EDSM
                    status = "Undiscovered system";
                    highlight = true;
                }
                else if (edsmResult.bodyCount == 0)
                {
                    // system is known from routes but it has not been visited
                    status = "Unvisited system";
                    highlight = true;
                }
                else
                {
                    if (edsmResult.bodyCount == edsmResult.bodies.Count)
                        status = $"Discovered, {edsmResult.bodyCount} bodies";
                    else
                        status = $"Discovered ({edsmResult.bodies.Count} of {edsmResult.bodyCount} bodies)";

                    var discCmdr = edsmResult.bodies.FirstOrDefault()?.discovery?.commander;
                    var discDate = edsmResult.bodies.FirstOrDefault()?.discovery?.date.ToShortDateString();
                    if (discCmdr != null && discDate != null)
                        subStatus = $"By {discCmdr}, {discDate}";
                }

                var plotter = Program.getPlotter<PlotGalMap>();
                if (plotter != null && plotter.Created)
                    plotter.BeginInvoke(() => plotter.Invalidate());

            });

            Game.spansh.getSystemDump((long)entry.SystemAddress).ContinueWith(result =>
            {
                if (result.Exception != null)
                {
                    Util.isFirewallProblem(result.Exception);
                    return;
                }
                var spanshResult = result.Result;
                this.sumGenus = spanshResult.bodies.Sum(_ => _.signals?.genuses?.Count ?? 0);

            });

            // TODO: maybe lookup in Canonn for bio data too?

        }

    }

}
