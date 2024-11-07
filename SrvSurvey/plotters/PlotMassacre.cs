using Newtonsoft.Json;
using SrvSurvey.game;

namespace SrvSurvey
{
    internal class PlotMassacre : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.settings.autoShowPlotMassacre_TEST
                && Game.activeGame?.cmdr?.trackMassacres?.Count > 0
                && Game.activeGame.isMode(GameMode.Flying, GameMode.ExternalPanel, GameMode.SuperCruising, GameMode.StationServices)
                ;
        }

        private PlotMassacre() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.Fonts.console_8;
        }

        public override bool allow { get => PlotMassacre.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(100);
            this.Height = scaled(80);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (game.cmdr.trackMassacres == null)
            {
                Program.closePlotter<PlotMassacre>();
                return;
            }

            drawTextAt2(eight, "Massacre kills remaining:");
            newLine(eight, true);

            foreach (var massacre in game.cmdr.trackMassacres.OrderBy(m => m.targetFaction + m.missionGiver))
            {
                var col = massacre.remaining == 0 ? GameColors.Orange : GameColors.Cyan;

                // draw target faction name
                drawTextAt2(eight, $"► ", col, GameColors.Fonts.console_8B);
                var x = dtx;
                var sz = drawTextAt2($"{massacre.targetFaction}:", col, GameColors.Fonts.console_8B);

                if (massacre.remaining == 0)
                    strikeThrough(dtx, dty + five, -sz.Width, false);

                // draw mission giver indented + smaller below
                dty += three;
                var sz2 = drawTextAt2(x, $"\r\n{massacre.missionGiver}", massacre.remaining == 0 ? GameColors.OrangeDim : GameColors.DarkCyan, GameColors.Fonts.console_7);

                // draw count remaining
                dty -= six;
                var txt = massacre.remaining == 0 ? "💀" : massacre.remaining.ToString();
                var sz3 = drawTextAt2(this.Width - eight, txt, col, GameColors.Fonts.gothic_16B, true);

                var ww = sz.Width > sz2.Width ? sz.Width : sz2.Width;
                dtx = x + ww + sz3.Width;
                newLine(nine, true);
            }

            this.formAdjustSize(oneSix, four);
        }
    }

    internal class TrackMassacre
    {
        public readonly static List<string> validMissionNames = new() { "Mission_Massacre", "Mission_MassacreWing" };

        public long missionId;
        public string missionGiver;
        public string targetFaction;
        public DateTimeOffset expires;
        public int killCount;
        public int remaining;

        public TrackMassacre(MissionAccepted? entry)
        {
            if (entry != null)
            {
                this.missionId = entry.MissionID;
                this.missionGiver = entry.Faction;
                this.targetFaction = entry.TargetFaction!;
                this.expires = entry.Expiry!.Value;
                this.killCount = entry.KillCount;
                this.remaining = entry.KillCount;
            }
        }

        public override string ToString()
        {
            return $"{targetFaction}:{remaining} of {killCount} ({missionId})";
        }

        [JsonIgnore]
        public bool expired
        {
            get => DateTimeOffset.UtcNow > this.expires;
        }
    }
}
