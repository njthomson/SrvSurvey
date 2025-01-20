using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    internal class PlotSphericalSearch : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap
                && (sphereLimitActive || boxelSearchActive);
        }

        private static bool sphereLimitActive { get => Game.activeGame?.cmdr.sphereLimit.active == true; }
        private static bool boxelSearchActive { get => Game.activeGame?.cmdr.boxelSearch?.active == true; }

        private double distance = -1;
        private string targetSystemName;
        private string? destinationName;
        private string? badDestination;

        private PlotSphericalSearch() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmaller;
            this.destinationName = game.status.Destination?.Name;
        }

        public override bool allow { get => PlotSphericalSearch.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(400);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            this.BeginInvoke(() =>
            {
                measureDistanceToSystem();
            });

            // put next boxel system in clipboard?
            if (game.cmdr.boxelSearch?.active == true && game.cmdr.boxelSearch.autoCopy)
            {
                // only pre-fill clipboard if we're inside the boxel search area
                var insideSearchArea = game.cmdr.boxelSearch.boxel.containsChild(game.cmdr.getCurrentBoxel());
                if (insideSearchArea)
                {
                    var text = game.cmdr.boxelSearch.getNextToVisit();
                    if (text != null)
                    {
                        Game.log($"Setting next boxel search system to clipboard: {text}");
                        Clipboard.SetText(text);
                    }
                }
            }
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (!sphereLimitActive || this.IsDisposed || game.cmdr.sphereLimit.centerStarPos == null) return;

            measureDistanceToSystem();
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (!sphereLimitActive || this.IsDisposed || game.cmdr.sphereLimit.centerStarPos == null) return;

            this.distance = -1;
            this.targetSystemName = "N/A";
            this.Invalidate();
        }

        private void measureDistanceToSystem()
        {
            if (!sphereLimitActive || game.cmdr.sphereLimit.centerStarPos == null) return;

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

            this.Invalidate();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game.systemData == null) return;
            base.Status_StatusChanged(blink);

            // if the destination changed ...
            if (game.status.Destination != null && this.destinationName != game.status.Destination.Name)
            {
                this.destinationName = game.status.Destination.Name;
                this.targetSystemName = game.status.Destination.Name;

                if (sphereLimitActive)
                {
                    this.distance = -1;
                    Game.edsm.getSystems(this.destinationName).ContinueWith(t =>
                    {
                        var data = t.Result;
                        if (!(data.Length > 0))
                        {
                            this.distance = -2;
                            this.Invalidate();
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

                        this.Invalidate();
                    });
                }

                if (game.cmdr.boxelSearch?.active == true)
                {
                    var bx = Boxel.parse(game.status.Destination.System, game.status.Destination.Name);
                    if (!game.cmdr.boxelSearch.boxel.containsChild(bx))
                        this.badDestination = $"{this.destinationName} is outside search boxel";
                    else if (bx?.massCode < game.cmdr.boxelSearch.lowMassCode)
                        this.badDestination = $"{this.destinationName} mass code too low";
                    else
                        this.badDestination = null;
                }

                this.Invalidate();
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

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (!sphereLimitActive && !boxelSearchActive)
            {
                Program.closePlotter<PlotSphericalSearch>();
                return;
            }

            if (sphereLimitActive)
                this.drawSphereLimit();
            if (boxelSearchActive)
                this.drawBoxelSearch();

            if (sphereLimitActive && formSize.Width < scaled(240)) formSize.Width = scaled(240);
            this.formAdjustSize(0, +ten);
        }

        private void drawSphereLimit()
        {
            var ww = ten + Util.maxWidth(this.Font, RES("From"), RES("To"), RES("Distance"));

            this.drawTextAt(eight, RES("From"));
            this.drawTextAt(ww, game.cmdr.sphereLimit.centerSystemName);
            newLine(true);

            this.drawTextAt(eight, RES("To"));
            this.drawTextAt(ww, this.targetSystemName);
            newLine(true);

            this.drawTextAt(eight, RES("Distance"));
            if (this.distance == -1)
            {
                this.drawTextAt(ww, "...");
            }
            else if (this.distance == -2)
            {
                this.drawTextAt(ww, RES("UnknownDist"), GameColors.brushRed);
            }
            else if (this.distance >= 0)
            {
                var dist = this.distance.ToString("N2");
                var limitDist = game.cmdr.sphereLimit.radius.ToString("N2");

                if (this.distance < game.cmdr.sphereLimit.radius)
                    this.drawTextAt(ww, RES("DistanceWithin", dist, limitDist), GameColors.brushCyan);
                else
                    this.drawTextAt(ww, RES("DistanceExceeds", dist, limitDist), GameColors.brushRed);
            }

            newLine(true);
        }

        private void drawBoxelSearch()
        {
            if (sphereLimitActive)
            {
                dty += four;
                strikeThrough(eight, dty, this.Width - oneSix, false);
                dty += eight;
            }
            else
            {
                dty += one;
            }

            var boxelSearch = Game.activeGame?.cmdr.boxelSearch;
            if (boxelSearch?.active != true || boxelSearch.current == null) return;

            var ff = GameColors.fontSmall;
            var ww = ten + Util.maxWidth(ff, RES("Boxel"), RES("Visited"), RES("Next"));

            this.drawTextAt(eight, RES("Boxel"), ff);
            this.drawTextAt(ww, boxelSearch.current.prefix + "xxx", ff);
            newLine(two, true);

            this.drawTextAt(eight, RES("Visited"), ff);
            this.drawTextAt(ww, RES("VisitedOf", boxelSearch.countVisited, boxelSearch.currentCount), ff);
            newLine(two, true);

            this.drawTextAt(eight, RES("Next"), ff);
            var next = boxelSearch.getNextToVisit() ?? "";
            if (next == boxelSearch.current.prefix || next == boxelSearch.boxel.prefix) next += " ?";
            this.drawTextAt(ww, next, GameColors.fontMiddle);
            dtx += ten;

            // warn if destination is outside the search boxel
            //this.drawTextAt(eight, "Destination:", ff);
            ff = GameColors.fontMiddle;
            if (this.badDestination != null)
            {
                newLine(+eight, true);
                this.drawTextAt2b(eight, this.Width - 4, this.badDestination , GameColors.red, ff);
                dtx += ten;
            }
            else if (this.destinationName != null)
            {
                newLine(+eight, true);
                this.drawTextAt2b(eight, this.Width - 4, $"Destination is within search boxel", GameColors.Cyan, ff);
                dtx += ten;
            }

            newLine(true);
        }
    }
}
