using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public double distance { get; private set; }

        /// <summary>
        /// The relative distance along the X axis, in meters.
        /// </summary>
        public double dx { get; private set; }

        /// <summary>
        /// The relative distance along the Y axis, in meters.
        /// </summary>
        public double dy { get; private set; }

        /// <summary>
        /// The angle between the points, relative to North 0°. Meaning, set vehicle to THIS angle to track towards the target.
        /// </summary>
        public Angle angle { get; private set; }

        /// <summary>
        /// A percentage of how close/complete we are. 100% is ontop, 0% is on the opposite side of the planet.
        /// </summary>
        public double complete { get; private set; }

        private readonly double halfCirc;
        private readonly double mpd;
        private LatLong2 current;
        private LatLong2 target;

        public TrackingDelta(double bodyRadius, LatLong2 targetLocation) : this(bodyRadius, Game.activeGame.status.here, targetLocation)
        {
        }

        public TrackingDelta(double bodyRadius, LatLong2 currentLocation, LatLong2 targetLocation)
        {
            this.mpd = bodyRadius * Math.PI * 2 / 360;
            this.halfCirc = bodyRadius; // * this.mpd;
            this.current = currentLocation;
            this.target = targetLocation;
            this.calc();
        }

        private void calc()
        {
            // delta in LatLong degrees
            var dll = this.Point2 - this.Point1;

            this.dx = dll.Long * this.mpd;
            this.dy = dll.Lat * this.mpd;

            this.distance = dll.getDistance(true) * this.mpd;

            this.complete = 100.0 / this.halfCirc * this.distance;
            //Game.log($"ccomplete: {this.halfCirc} / {this.distance} / {this.complete}");
            // calculate the angle, adjusting for negatives
            var a = Math.Atan(/*opp*/dll.Long / /*adj*/dll.Lat) * Angle.ratioDegreesToRadians;
            this.angle = dll.Lat < 0 ? 180 + a : a;
        }


        public LatLong2 Point1

        {
            get => this.current;
            set
            {
                this.current = value;
                this.calc();
            }
        }

        public LatLong2 Point2

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
            return $"x: {this.dx}, y: {this.dy}, d: {Util.metersToString(this.distance)}, a: {this.angle}";
        }
    }
}
