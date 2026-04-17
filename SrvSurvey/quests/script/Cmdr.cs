using Lua;
using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey.quests.script
{
    [LuaObject]
    public partial class Cmdr
    {
        private PlayChapter pc;

        public Cmdr(PlayChapter pc)
        {
            this.pc = pc;
        }

        [LuaMember]
        public LuaValue name => Game.activeGame?.Commander ?? "";

        [LuaMember]
        public LuaValue last(string eventName)
        {
            return pc.pq.getLast(eventName);
        }

        [LuaMember]
        public LuaValue lastDocked => pc.pq.getLast("Docked");

        [LuaMember]
        public LuaValue lastFSDJump => pc.pq.getLast("FSDJump");

        [LuaMember]
        public LuaValue getFactionRep(string factionName)
        {
            var match = Game.activeGame?.systemFactions?.Find(f => f.Name == factionName);
            return match?.MyReputation ?? double.NaN;
        }
        [LuaMember]
        public LuaValue getFactionInf(string factionName)
        {
            var match = Game.activeGame?.systemFactions?.Find(f => f.Name == factionName);
            return match?.Influence ?? double.NaN;
        }

        [LuaMember]
        public LuaTable getFactionStates(string factionName, string tense = "active")
        {
            var match = Game.activeGame?.systemFactions?.Find(f => f.Name == factionName);
            if (match == null) return [];

            if (tense == "recovering")
                return match.RecoveringStates?.Select(s => s.State).toTbl() ?? new LuaTable();

            if (tense == "pending")
                return match.PendingStates?.Select(s => s.State).toTbl() ?? new LuaTable();

            if (tense == "active")
                return match.ActiveStates?.Select(s => s.State).toTbl() ?? new[] { match.FactionState }.toTbl();

            throw new Exception($"Bad value for tense, try: active, pending, recovering");
        }

        [LuaMember]
        public LuaTable status
        {
            get
            {
                var tbl = LuaUtils.toTbl(Game.activeGame?.status);
                return tbl;
            }
        }

        [LuaMember]
        public double distanceFrom(double lat, double @long)
        {
            if (Game.activeGame?.status == null) return -1;

            var dist = (double)Util.getDistance(
                new LatLong2(lat, @long),
                Status.here,
                Game.activeGame.status.PlanetRadius);

            return dist;
        }

        [LuaMember]
        public bool isWithin(double lat, double @long, double targetDist)
        {
            var dist = distanceFrom(lat, @long);
            return dist < targetDist;
        }

        [LuaMember]
        public bool headingBetween(int heading, int tolerance)
        {
            if (Game.activeGame?.status == null) return false;

            var cmdrHeading = Game.activeGame.status.Heading;

            var left = heading - tolerance;
            if (left < 0) left += 360;

            var right = heading + tolerance;
            if (right >= 360) right -= 360;

            var between = left > right
                ? cmdrHeading >= left || cmdrHeading <= right
                : cmdrHeading >= left && cmdrHeading <= right;

            return between;
        }
    }
}
