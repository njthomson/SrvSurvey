using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DecimalMath;
using SrvSurvey.game;

namespace SrvSurvey.units
{
    /// <summary>
    /// Represents a vector in meters
    /// </summary>
    class TrackingDelta
    {
        /// <summary>
        /// The absolute distance between the two points, in meters .
        /// </summary>
        public decimal distance { get; private set; }

        /// <summary>
        /// The relative distance along the X axis, in meters.
        /// </summary>
        public decimal dx { get; private set; }

        /// <summary>
        /// The relative distance along the Y axis, in meters.
        /// </summary>
        public decimal dy { get; private set; }

        /// <summary>
        /// The angle between the points, relative to North 0°. Meaning, set vehicle to THIS angle to track towards the target.
        /// </summary>
        public Angle angle { get; private set; }

        /// <summary>
        /// A percentage of how close/complete we are. 100% is ontop, 0% is on the opposite side of the planet.
        /// </summary>
        public decimal complete { get; private set; }

        private readonly decimal mpd;
        public readonly decimal radius;
        private LatLong2 current;
        private LatLong2 target;

        public TrackingDelta(double bodyRadius, LatLong2 targetLocation, LatLong2 current = null!)
        {
            this.radius = (decimal)bodyRadius;
            this.mpd = this.radius * DecimalEx.TwoPi / 360M;
            this.current = current ?? Status.here;
            this.target = targetLocation;
            this.calc();
        }

        public void calc()
        {
            this.distance = Util.getDistance(this.current, this.target, this.radius);

            this.complete = 100.0M / this.radius * this.distance;
            //Game.log($"ccomplete: {this.halfCirc} / {this.distance} / {this.complete}");
            var rad = Util.getBearingRad(this.current, this.target);

            this.angle = DecimalEx.ToDeg(rad);

            this.dx = DecimalEx.Sin(rad) * this.distance;
            this.dy = DecimalEx.Cos(rad) * this.distance;
        }

        public LatLong2 Current
        {
            get => this.current;
            set
            {
                this.current = value;
                this.calc();
            }
        }

        public LatLong2 Target
        {
            get => this.target;
            set
            {
                this.target = value;
                this.calc();
            }
        }

        public override string ToString()
        {
            return $"x: {Math.Round(this.dx)}, y: {Math.Round(this.dy)}, d: {Util.metersToString(this.distance)}, a: {this.angle}";
        }
    }
}
