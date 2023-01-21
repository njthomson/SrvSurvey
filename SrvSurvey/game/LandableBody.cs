using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    class LandableBody
    {
        public string starSystem;
        public string name;
        public double radius;

        /// <summary>
        /// Meters per 1° of Latitude or Longitude
        /// </summary>
        public double mpd
        {
            get
            {
                var bodyCircumferance = this.radius * Math.PI * 2F;
                return bodyCircumferance / 360D;

            }
        }
    }
}
