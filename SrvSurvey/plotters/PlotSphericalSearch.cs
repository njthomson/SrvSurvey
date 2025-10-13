using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotSphericalSearch;

namespace SrvSurvey.plotters
{
    internal class PlotSphericalSearch : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotSphericalSearch),
            allowed = allowed,
            ctor = (game, def) => new PlotSphericalSearch(game, def),
            defaultSize = new Size(240, 240), // Not 400, 240 ?
        };

        public static bool allowed(Game game)
        {
            return game.mode == GameMode.GalaxyMap
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && (sphereLimitActive || boxelSearchActive || routeFollowActive);
        }

        #endregion

        private static bool sphereLimitActive { get => Game.activeGame?.cmdr?.sphereLimit.active == true; }
        private static bool boxelSearchActive { get => Game.activeGame?.cmdr?.boxelSearch?.active == true; }
        private static bool routeFollowActive { get => Game.activeGame?.cmdr?.route?.active == true && Game.activeGame.cmdr.route.nextHop != null; }

        private double distance = -1;
        private string targetSystemName = "";
        private string? destinationName;
        private string? badDestination;
        private bool nextBoxelSystemCopied;
        private bool nextRouteSystemCopied;

        private PlotSphericalSearch(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmaller;
            this.destinationName = game.status.Destination?.Name;

            Program.defer(() =>
            {
                measureDistanceToSystem();
            });

            // put next system in clipboard?
            Util.deferAfter(250, () =>
            {
                if (this.isClosed) return;

                // give priority to route following over boxel searches
                if (game.cmdr.route?.useNextHop == true)
                {
                    Clipboard.SetText(game.cmdr.route.nextHop?.name!);
                    this.nextRouteSystemCopied = true;
                    this.invalidate();
                }
                else if (game.cmdr.boxelSearch?.active == true && game.cmdr.boxelSearch.autoCopy)
                {
                    // only pre-fill clipboard if we're inside the boxel search area
                    var insideSearchArea = game.cmdr.boxelSearch.boxel.containsChild(game.cmdr.getCurrentBoxel());
                    if (insideSearchArea)
                    {
                        var text = game.cmdr.boxelSearch.nextSystem;
                        if (text != null)
                        {
                            Game.log($"Setting next boxel search system to clipboard: {text}");
                            Clipboard.SetText(text);
                            this.nextBoxelSystemCopied = true;
                            this.invalidate();
                        }
                    }
                }
            });
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if ((sphereLimitActive && game.cmdr.sphereLimit.centerStarPos != null) || game.cmdr.boxelSearch?.active == true)
                measureDistanceToSystem();
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (!sphereLimitActive || game.cmdr.sphereLimit.centerStarPos == null) return;

            this.distance = -1;
            this.targetSystemName = "N/A";
            this.invalidate();
        }

        private void measureDistanceToSystem()
        {
            if (this.isClosed) return;

            if (sphereLimitActive && game.cmdr.sphereLimit.centerStarPos != null)
            {

                var lastSystem = game.navRoute.Route.LastOrDefault();
                if (lastSystem?.StarSystem == null)
                {
                    if (game.systemData == null) return;

                    lastSystem = new RouteEntry()
                    {
                        StarSystem = game.systemData.name,
                        StarPos = game.systemData.starPos,
                    };
                }

                this.targetSystemName = lastSystem.StarSystem;
                this.distance = Util.getSystemDistance(game.cmdr.sphereLimit.centerStarPos, lastSystem.StarPos);
                Game.log($"Measuring distance: '{game.cmdr.sphereLimit.centerSystemName}' {game.cmdr.sphereLimit.centerStarPos} => '{lastSystem.StarSystem}' {lastSystem.StarPos} = {this.distance}");
            }

            // use the last entry in the route
            if (game.cmdr.boxelSearch?.active == true && game.navRoute.Route.Count > 1)
            {
                var lastHop = game.navRoute.Route.Last();
                var bx = Boxel.parse(lastHop.SystemAddress, lastHop.StarSystem);
                this.badDestination = this.checkBoxel(bx);
            }

            this.invalidate();
        }

        private string? checkBoxel(Boxel? bx)
        {
            if (bx == null || game.cmdr.boxelSearch == null)
                return null;
            else if (!game.cmdr.boxelSearch.boxel.containsChild(bx))
                return $"✋ {bx.name}\nOutside search boxel";
            else if (bx.massCode < game.cmdr.boxelSearch.lowMassCode)
                return $"⚠️ {bx.name}\nMass code too low";
            else if (game.cmdr.boxelSearch.systems.FirstOrDefault(sys => sys.name == bx)?.complete == true)
                return $"✔️ {bx.name}\nSystem already surveyed";
            else
                return null;
        }

        protected override void onStatusChange(Status status)
        {
            if (game.systemData == null) return;
            base.onStatusChange(status);

            // TODO: Use: status.changed.Contains(nameof(Status.Destination)) ?

            // if the destination changed ...
            if (game.status.Destination != null && this.destinationName != game.status.Destination.Name)
            {
                this.destinationName = game.status.Destination.Name;
                this.targetSystemName = game.status.Destination.Name;
                var isFirstRouteHop = game.navRoute.Route?.Count > 2 && game.navRoute.Route[1].StarSystem == game.status.Destination.Name;

                if (sphereLimitActive && !isFirstRouteHop)
                {
                    this.distance = -1;
                    Game.edsm.getSystems(this.destinationName).continueOnMain(null, data =>
                    {
                        if (this.isClosed || game.systemData == null) return;

                        if (!(data.Length > 0))
                        {
                            this.distance = -2;
                            this.invalidate();
                            return;
                        }

                        var starPos = data.FirstOrDefault()?.coords!;

                        var destination = new RouteEntry()
                        {
                            StarSystem = this.destinationName,
                            StarPos = starPos,
                        };

                        Game.log($"Measuring distance to: {destination.StarSystem}");
                        this.targetSystemName = destination.StarSystem;
                        this.distance = Util.getSystemDistance(game.cmdr.sphereLimit.centerStarPos!, destination.StarPos);

                        this.invalidate();
                    });
                }

                if (game.cmdr.boxelSearch?.active == true && !isFirstRouteHop && game.status.Destination.Body == 0)
                {
                    var bx = Boxel.parse(game.status.Destination.System, game.status.Destination.Name);
                    this.badDestination = this.checkBoxel(bx);
                }

                this.invalidate();
            }
        }

        //protected override void onPaintPlotter(PaintEventArgs e)
        //{
        //    var cc = Color.Red;
        //    //cc = Color.FromArgb(255, 255, 24, 0);
        //    cc = Color.FromArgb(255, 24, 255, 64);
        //    cc = Color.White;

        //    var ff = new Font("Arial", 16);
        //    ff = GameColors.fontSmall2;

        //    var bb = new HatchBrush(HatchStyle.DarkDownwardDiagonal, cc);

        //    var pt = new Point(14, 10);
        //    var sz = TextRenderer.MeasureText("Cyan", ff);

        //    //var rr = new Rectangle(pt.X - 1, pt.Y - 2, sz.Width + 140, sz.Height + 4);
        //    //g.FillRectangle(bb, rr);

        //    ////rr.Offset(+2, +2);
        //    //rr.Inflate(-2, -2);
        //    //g.FillRectangle(Brushes.Black, rr);


        //    pt.X += 2;
        //    //  🎂 🧁 🍥 ‡† ⁑ ⁂ ※ ⁜‼•🟎 🟂🟎🟒🟈⚑⚐⛿🏁🎌⛳🏴🏳🟎✩✭✪𓇽𓇼 🚕🛺🚐🚗🚜🚛🛬🚀🛩️☀️🌀☄️🔥⚡🌩️🌠☀️
        //    // 💫 🧭🧭🌍🌐🌏🌎🗽♨️🌅
        //    // 💎🪐🎁🍥🍪🧊⛩️🌋⛰️🗻❄️🎉🧨🎁🧿🎲🕹️📣🎨🧵🔇🔕🎚️🎛️📻📱📺💻🖥️💾📕📖📦📍📎✂️📌📐📈💼🔰🛡️🔨🗡️🔧🧪🚷🧴📵🧽➰🔻🔺🔔🔘🔳🔲🏁🚩🏴✔️✖️❌➕➖➗ℹ️📛⭕☑️📶🔅🔆⚠️⛔🚫🧻↘️⚰️🧯🧰📡🧬⚗️🔩⚙️🔓🗝️🗄️📩🧾📒📰🗞️🏷️📑🔖💡🔋🏮🕯🔌📞☎️💍👑🧶🎯🔮🧿🎈🏆🎖️🌌💫🚧💰
        //    // saturn 🪐
        //    TextRenderer.DrawText(g, "💎Cyan", ff, pt, cc, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        //    //pt.X += 16;
        //    //TextRenderer.DrawText(g, "Cyan", ff, pt, cc, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

        //    // 🔔 alarm
        //    // 🗝️ auth?
        //    // 🔋 battery?
        //    // 📩 data terminal?
        //    // 🔩 sample container


        //    return;

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (!allowed(game))
            {
                PlotBase2.remove(def);
                return frame.Size;
            }

            if (sphereLimitActive)
                this.drawSphereLimit(tt);
            if (boxelSearchActive)
                this.drawBoxelSearch(tt, sphereLimitActive);
            if (routeFollowActive)
                this.drawRouteFollow(tt, sphereLimitActive || boxelSearchActive);

            tt.setMinWidth(N.twoFourty);
            return tt.pad(+N.ten, +N.ten);
        }

        private void drawSphereLimit(TextCursor tt)
        {
            var ww = N.ten + Util.maxWidth(this.font, Res.From, Res.To, Res.Distance);

            tt.draw(N.eight, Res.From);
            tt.draw(ww, game.cmdr.sphereLimit.centerSystemName);
            tt.newLine(true);

            tt.draw(N.eight, Res.To);
            tt.draw(ww, this.targetSystemName);
            tt.newLine(true);

            tt.draw(N.eight, Res.Distance);
            if (this.distance == -1)
            {
                tt.draw(ww, "...");
            }
            else if (this.distance == -2)
            {
                tt.draw(ww, Res.UnknownDist, C.red);
            }
            else if (this.distance >= 0)
            {
                var dist = this.distance.ToString("N2");
                var limitDist = game.cmdr.sphereLimit.radius.ToString("N2");

                if (this.distance < game.cmdr.sphereLimit.radius)
                    tt.draw(ww, Res.DistanceWithin.format(dist, limitDist), C.cyan);
                else
                    tt.draw(ww, Res.DistanceExceeds.format(dist, limitDist), C.red);
            }

            tt.newLine(true);
        }

        private void drawBoxelSearch(TextCursor tt, bool drawLine)
        {
            if (drawLine)
            {
                tt.dty += N.four;
                tt.strikeThrough(N.eight, tt.dty, this.width - N.oneSix, false);
                tt.dty += N.eight;
            }
            else
            {
                tt.dty += N.one;
            }

            var boxelSearch = Game.activeGame?.cmdr.boxelSearch;
            if (boxelSearch?.active != true || boxelSearch.current == null) return;

            var ff = GameColors.fontSmall;
            var ww = N.ten + Util.maxWidth(ff, Res.Boxel, Res.Visited, Res.Next);

            tt.draw(N.eight, Res.Boxel, ff);
            tt.draw(ww, boxelSearch.boxel.prefix + " ...", ff);
            tt.newLine(N.two, true);

            // also show the current system if different than the top boxel
            if (boxelSearch.current.prefix != boxelSearch.boxel.prefix)
            {
                tt.draw(N.eight, "Current:", ff);
                tt.draw(ww, boxelSearch.current.prefix + " ...", ff);
                tt.newLine(N.two, true);
            }

            tt.draw(N.eight, Res.Visited, ff);
            var pp = (1f / boxelSearch.currentCount * boxelSearch.countSystemsComplete).ToString("p0");
            tt.draw(ww, Res.VisitedOf.format(pp, boxelSearch.countSystemsComplete, boxelSearch.currentCount), ff);
            tt.newLine(N.two, true);

            tt.draw(N.eight, Res.Next, ff);
            var next = boxelSearch.nextSystem;
            if (next == boxelSearch.current.prefix || next == boxelSearch.boxel.prefix) next += " ?";
            tt.draw(ww, next, GameColors.fontMiddle);
            tt.dtx += N.ten;

            // warn if destination is outside the search boxel
            if (this.badDestination?.Length > 0)
            {
                tt.newLine(+N.eight, true);
                tt.drawWrapped(N.eight, this.width - 4, this.badDestination, GameColors.red, GameColors.fontMiddle);
                tt.dtx += N.ten;
            }
            else if (this.destinationName != null)
            {
                tt.newLine(+N.eight, true);
                tt.drawWrapped(N.eight, this.width - 4, $"✔️ Destination is valid", GameColors.Orange, ff);
                tt.dtx += N.ten;
            }

            if (this.nextBoxelSystemCopied)
            {
                tt.newLine(+N.eight, true);
                tt.drawWrapped(N.eight, this.width - 4, "► Next search copied", GameColors.Cyan, GameColors.fontSmall);
            }

            tt.newLine(+N.two, true);
        }

        private void drawRouteFollow(TextCursor tt, bool drawLine)
        {
            if (drawLine)
            {
                tt.dty += N.four;
                tt.strikeThrough(N.eight, tt.dty, this.width - N.oneSix, false);
                tt.dty += N.eight;
            }
            else
            {
                tt.dty += N.one;
            }

            // TODO: Localize the following ...
            var ff = GameColors.Fonts.gothic_10;
            var ww = N.ten + Util.maxWidth(ff, Res.Boxel, "Distance:");
            var route = game.cmdr.route!;

            var txt = $"Following route: ";
            if (route.last != -1) txt += $" #{route.last + 1} of {route.hops.Count}";
            tt.draw(N.eight, txt, ff);
            tt.newLine(true);

            if (route.nextHop?.xyz == null)
                tt.draw(N.eight, $"Next hop: unknown distance", ff);
            else
            {
                var dist = Util.getSystemDistance(route.nextHop.xyz, game.cmdr.getCurrentStarRef()).ToString("N2");
                tt.draw(N.eight, $"Next hop: {dist} ly away", ff);
            }
            tt.newLine(true);

            // highlight if nextHop is NOT the current route final destination
            var col = C.orange;
            var lastHop = game.navRoute.Route.LastOrDefault();
            if (lastHop?.SystemAddress != route.nextHop?.id64 && lastHop?.StarSystem.like(route.nextHop?.name) != true)
                col = C.cyan;

            tt.dty += N.four;
            tt.draw(N.twoEight, route.nextHop?.name ?? "?", col, GameColors.Fonts.gothic_12B);
            tt.newLine(+N.four, true);

            if (route.nextHop?.notes != null)
            {
                tt.draw(N.eight, "► " + route.nextHop.notes, col, ff);
                tt.newLine(true);
            }

            if (this.nextRouteSystemCopied)
            {
                tt.drawWrapped(N.eight, this.width - 4, "► Next system copied", GameColors.Cyan, ff);
                tt.newLine(true);
            }
        }
    }
}
