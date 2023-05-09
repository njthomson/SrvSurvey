using DecimalMath;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Diagnostics;

namespace SrvSurvey
{
    static class Util
    {
        public static double degToRad(int angle)
        {
            return angle / Angle.ratioDegreesToRadians;
        }

        public const decimal ratioDegreesToRadians = 180 / (decimal)Math.PI;

        public static decimal degToRad(decimal angle)
        {
            return angle / ratioDegreesToRadians;
        }

        public static double radToDeg(double angle)
        {
            return angle * Angle.ratioDegreesToRadians;
        }

        public static string metersToString(decimal m, bool asDelta = false)
        {
            return metersToString((double)m, asDelta);
        }

        /// <summary>
        /// Returns a count of meters to a string with 4 significant digits, adjusting the units accordinly between: m, km, mm
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string metersToString(double m, bool asDelta = false)
        {
            var prefix = "";
            if (asDelta)
            {
                prefix = m < 0 ? "-" : "+";
            }

            // make number positive
            if (m < 0) m = -m;

            if (m < 1)
            {
                // less than 1 thousand
                return "0 m";
            }

            if (m < 1000)
            {
                // less than 1 thousand
                return prefix + m.ToString("#") + " m";
            }

            m = m / 1000;
            if (m < 10)
            {
                // less than 1 ten
                return prefix + m.ToString("##.##") + " km";
            }
            else if (m < 1000)
            {
                // less than 1 thousand
                return prefix + m.ToString("###.#") + " km";
            }

            m = m / 1000;
            // over one 1 million
            return prefix + m.ToString("#.##") + " mm";

        }

        public static string timeSpanToString(TimeSpan ago)
        {
            if (ago.TotalSeconds < 60)
                return "just now";
            if (ago.TotalMinutes < 120)
                return ago.TotalMinutes.ToString("#") + " minutes ago";
            if (ago.TotalHours < 24)
                return ago.TotalHours.ToString("#") + " hours ago";
            return ago.TotalDays.ToString("#") + " days ago";
        }

        public static string getLocationString(string starSystem, string body)
        {
            if (string.IsNullOrEmpty(body))
                return starSystem;


            // for cases like: "StarSystem":"Adenets" .. "Body":"Adenets 8 c"
            if (body.Contains(starSystem))
                return body;

            // for cases like: "StarSystem":"Enki" .. "Body":"Ponce de Leon Vision",
            return $"{starSystem}, {body}";

        }

        public static void openLink(string link)
        {
            var info = new ProcessStartInfo(link);
            info.UseShellExecute = true;
            Process.Start(info);
        }

        public static string credits(long credits, bool hideUnits = false)
        {
            string txt;

            var millions = credits / 1000000.0D;
            if (millions == 0)
                txt = "0 M";
            else
                txt = millions.ToString("#.## M");

            if (!hideUnits)
                txt += " CR";
            return txt;
        }

        /// <summary>
        /// Returns the distance between two lat/long points on body with radius r.
        /// </summary>
        public static decimal getDistance(LatLong2 p1, LatLong2 p2, decimal r)
        {
            // don't bother calculating if positions are identical
            if (p1.Lat == p2.Lat && p1.Long == p2.Long)
                return 0;

            var lat1 = degToRad((decimal)p1.Lat);
            var lat2 = degToRad((decimal)p2.Lat);

            var angleDelta = DecimalEx.ACos(
                DecimalEx.Sin(lat1) * DecimalEx.Sin(lat2) +
                DecimalEx.Cos(lat1) * DecimalEx.Cos(lat2) * DecimalEx.Cos(Util.degToRad((decimal)p2.Long - (decimal)p1.Long))
            );
            var dist = angleDelta * r;

            return dist;
        }

        /// <summary>
        /// Returns the bearing between two lat/long points on a body.
        /// </summary>
        public static decimal getBearing(LatLong2 p1, LatLong2 p2)
        {
            var deg = DecimalEx.ToDeg(getBearingRad(p1, p2));
            if (deg < 0) deg += 360;

            return deg;
        }

        public static decimal getBearingRad(LatLong2 p1, LatLong2 p2)
        {
            var lat1 = Util.degToRad((decimal)p1.Lat);
            var lon1 = Util.degToRad((decimal)p1.Long);
            var lat2 = Util.degToRad((decimal)p2.Lat);
            var lon2 = Util.degToRad((decimal)p2.Long);

            var x = DecimalEx.Cos(lat2) * DecimalEx.Sin(lon2 - lon1);
            var y = DecimalEx.Cos(lat1) * DecimalEx.Sin(lat2) - DecimalEx.Sin(lat1) * DecimalEx.Cos(lat2) * DecimalEx.Cos(lon2 - lon1);

            var rad = DecimalEx.ATan2(x, y);
            if (rad < 0)
                rad += DecimalEx.TwoPi;

            return rad;
        }

        public static SizeF rotateLine(float angle, float length)
        {
            var heading= new Angle(angle);
            var dx = (float)Math.Sin(heading.radians) * length;
            var dy = (float)Math.Cos(heading.radians) * length;

            return new SizeF(dx, dy);
        }
    }
}
