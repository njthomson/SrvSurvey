using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotSphericalSearch : PlotBase, PlotterForm
    {
        private double distance = -1;
        private string targetSystemName;
        private string? destinationName;

        private PlotSphericalSearch() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontMiddle;
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
        }

        public static bool allowPlotter
        {
            get => Game.activeGame != null
                && Game.activeGame.mode == GameMode.GalaxyMap
                && Game.activeGame.cmdr.sphereLimit.active;
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            if (this.IsDisposed || game.cmdr.sphereLimit.centerStarPos == null) return;

            measureDistanceToSystem();
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            if (this.IsDisposed || game.cmdr.sphereLimit.centerStarPos == null) return;

            this.distance = -1;
            this.targetSystemName = "N/A";
            this.Invalidate();
        }

        private void measureDistanceToSystem()
        {
            if (game.cmdr.sphereLimit.centerStarPos == null) return;

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

            Game.log($"Measuring distance to: {lastSystem.StarSystem}");
            this.targetSystemName = lastSystem.StarSystem;
            this.distance = Util.getSystemDistance(game.cmdr.sphereLimit.centerStarPos, lastSystem.StarPos);

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
                this.distance = -1;
                this.Invalidate();

                Game.edsm.getSystems(this.destinationName).ContinueWith(t =>
                {
                    var data = t.Result;
                    if (!(data.Length > 0)) return;

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
            this.dty += this.drawTextAt(RES("From", game.cmdr.sphereLimit.centerSystemName)).Height;
            this.dtx = eight;
            this.dty += this.drawTextAt(RES("To", this.targetSystemName)).Height;

            if (this.distance < 0)
            {
                this.drawTextAt(eight, RES("DistanceInProgress"));
            }
            else if (this.distance >= 0)
            {
                var dist = this.distance.ToString("N2");
                var limitDist = game.cmdr.sphereLimit.radius.ToString("N2");

                if (this.distance < game.cmdr.sphereLimit.radius)
                    this.drawTextAt(eight, RES("DistanceWithin", dist, limitDist), GameColors.brushCyan);
                else
                    this.drawTextAt(eight, RES("DistanceExceeds", dist, limitDist), GameColors.brushRed);
            }
        }
    }

}
