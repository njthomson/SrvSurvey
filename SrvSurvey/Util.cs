using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    static class Util
    {
        /// <summary>
        /// Returns a count of meters to a string with 4 significant digits, adjusting the units accordinly between: m, km, mm
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string metersToString(double m)
        {
            if (m < 1000)
            {
                // less than 1 thousand
                return m.ToString("#") + " m";
            }

            m = m / 1000;
            if (m < 1000)
            {
                // less than 1 thousand
                return m.ToString("###.#") + " km";
            }

            m = m / 1000;
            // over one 1 million
            return m.ToString("#.##") + " mm";

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
    }
}
