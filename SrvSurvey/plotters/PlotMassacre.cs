using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotMassacre;

namespace SrvSurvey.plotters
{
    internal class PlotMassacre : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotMassacre),
            allowed = allowed,
            ctor = (game, def) => new PlotMassacre(game, def),
            defaultSize = new Size(180, 200), // Not 100, 80 ?
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotMassacre_TEST
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && game.cmdr?.trackMassacres?.Count > 0
                && game.isMode(GameMode.Flying, GameMode.ExternalPanel, GameMode.SuperCruising, GameMode.StationServices)
                ;
        }

        #endregion

        private PlotMassacre(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.Fonts.console_8;
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (game.cmdr.trackMassacres == null) return frame.Size;

            tt.draw(N.eight, Res.Header);
            tt.newLine(N.eight, true);

            foreach (var massacre in game.cmdr.trackMassacres.OrderBy(m => m.targetFaction + m.missionGiver))
            {
                var col = massacre.remaining == 0 ? GameColors.Orange : GameColors.Cyan;

                // draw target faction name
                tt.draw(N.eight, $"► ", col, GameColors.Fonts.console_8B);
                var x = tt.dtx;
                var sz = tt.draw($"{massacre.targetFaction}:", col, GameColors.Fonts.console_8B);

                if (massacre.remaining == 0)
                    tt.strikeThroughLast();

                // draw mission giver indented + smaller below
                tt.dty += N.three;
                var sz2 = tt.draw(x, $"\r\n{massacre.missionGiver}", massacre.remaining == 0 ? GameColors.OrangeDim : GameColors.DarkCyan, GameColors.Fonts.console_7);

                // draw count remaining
                tt.dty -= N.six;
                var txt = massacre.remaining == 0 ? "💀" : massacre.remaining.ToString();
                var sz3 = tt.draw(this.width - N.eight, txt, col, GameColors.Fonts.gothic_16B, true);

                var ww = sz.Width > sz2.Width ? sz.Width : sz2.Width;
                tt.dtx = x + ww + sz3.Width;
                tt.newLine(N.nine, true);
            }

            return tt.pad(N.oneSix, N.four);
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
