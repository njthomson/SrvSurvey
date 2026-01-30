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
