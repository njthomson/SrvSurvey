using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal readonly record struct GuardianMapMarkerOffset(double x, double y)
    {
        public bool isEmpty => x == 0 && y == 0;

        public PointF apply(PointF point)
        {
            return new PointF(
                point.X + (float)this.x,
                point.Y + (float)this.y);
        }
    }

    internal static class GuardianMapMarkerOffsetCalculator
    {
        public static GuardianMapMarkerOffset calculate(
            LatLong2 alignmentOrigin,
            LatLong2 correctedOrigin,
            int siteHeading,
            double planetRadiusMeters)
        {
            validateLocation(alignmentOrigin, nameof(alignmentOrigin));
            validateLocation(correctedOrigin, nameof(correctedOrigin));
            validateHeading(siteHeading);
            validateRadius(planetRadiusMeters);

            var distance = getDistance(
                alignmentOrigin,
                correctedOrigin,
                planetRadiusMeters);
            if (distance == 0)
                return default;

            var bearing = getBearing(alignmentOrigin, correctedOrigin);
            var mapAngle = (bearing - siteHeading) * Math.PI / 180d;
            return new GuardianMapMarkerOffset(
                -Math.Sin(mapAngle) * distance,
                Math.Cos(mapAngle) * distance);
        }

        public static PointF toSurfaceCoordinates(
            GuardianMapMarkerOffset markerOffset,
            int siteHeading)
        {
            validateHeading(siteHeading);

            var radians = siteHeading * Math.PI / 180d;
            return new PointF(
                (float)((-markerOffset.x * Math.Cos(radians))
                    + (markerOffset.y * Math.Sin(radians))),
                (float)((-markerOffset.x * Math.Sin(radians))
                    - (markerOffset.y * Math.Cos(radians))));
        }

        public static GuardianMapMarkerOffset rotateForHeading(
            GuardianMapMarkerOffset markerOffset,
            int oldSiteHeading,
            int newSiteHeading)
        {
            validateHeading(oldSiteHeading);
            validateHeading(newSiteHeading);

            var oldRadians = oldSiteHeading * Math.PI / 180d;
            var surfaceX = (-markerOffset.x * Math.Cos(oldRadians))
                + (markerOffset.y * Math.Sin(oldRadians));
            var surfaceY = (-markerOffset.x * Math.Sin(oldRadians))
                - (markerOffset.y * Math.Cos(oldRadians));
            var newRadians = newSiteHeading * Math.PI / 180d;
            return new GuardianMapMarkerOffset(
                (-surfaceX * Math.Cos(newRadians))
                    - (surfaceY * Math.Sin(newRadians)),
                (surfaceX * Math.Sin(newRadians))
                    - (surfaceY * Math.Cos(newRadians)));
        }

        public static LatLong2 recoverAlignmentOrigin(
            LatLong2 correctedOrigin,
            GuardianMapMarkerOffset markerOffset,
            int siteHeading,
            double planetRadiusMeters)
        {
            validateLocation(correctedOrigin, nameof(correctedOrigin));
            validateHeading(siteHeading);
            validateRadius(planetRadiusMeters);

            var distance = Math.Sqrt(
                (markerOffset.x * markerOffset.x)
                    + (markerOffset.y * markerOffset.y));
            if (!double.IsFinite(distance))
                throw new ArgumentOutOfRangeException(
                    nameof(markerOffset),
                    "The marker offset must contain finite coordinates.");

            if (distance == 0)
                return correctedOrigin.clone();

            var mapAngle = Math.Atan2(-markerOffset.x, markerOffset.y);
            var originalToCorrectedBearing = normalizeDegrees(
                (mapAngle * 180d / Math.PI) + siteHeading);
            return getDestination(
                correctedOrigin,
                normalizeDegrees(originalToCorrectedBearing + 180d),
                distance,
                planetRadiusMeters);
        }

        private static double getDistance(
            LatLong2 first,
            LatLong2 second,
            double radius)
        {
            if (first.Equals(second))
                return 0;

            var firstLatitude = degreesToRadians((double)first.Lat);
            var secondLatitude = degreesToRadians((double)second.Lat);
            var longitudeDelta = degreesToRadians(
                (double)second.Long - (double)first.Long);
            var cosine = (Math.Sin(firstLatitude) * Math.Sin(secondLatitude))
                + (Math.Cos(firstLatitude)
                    * Math.Cos(secondLatitude)
                    * Math.Cos(longitudeDelta));
            return Math.Acos(Math.Clamp(cosine, -1d, 1d)) * radius;
        }

        private static double getBearing(LatLong2 origin, LatLong2 target)
        {
            if (origin.Equals(target))
                return 0;

            var originLatitude = degreesToRadians((double)origin.Lat);
            var targetLatitude = degreesToRadians((double)target.Lat);
            var longitudeDelta = degreesToRadians(
                (double)target.Long - (double)origin.Long);
            var y = Math.Sin(longitudeDelta) * Math.Cos(targetLatitude);
            var x = (Math.Cos(originLatitude) * Math.Sin(targetLatitude))
                - (Math.Sin(originLatitude)
                    * Math.Cos(targetLatitude)
                    * Math.Cos(longitudeDelta));
            return normalizeDegrees(Math.Atan2(y, x) * 180d / Math.PI);
        }

        private static LatLong2 getDestination(
            LatLong2 origin,
            double bearingDegrees,
            double distanceMeters,
            double planetRadiusMeters)
        {
            var latitude = degreesToRadians((double)origin.Lat);
            var longitude = degreesToRadians((double)origin.Long);
            var bearing = degreesToRadians(bearingDegrees);
            var angularDistance = distanceMeters / planetRadiusMeters;
            var destinationLatitude = Math.Asin(
                (Math.Sin(latitude) * Math.Cos(angularDistance))
                    + (Math.Cos(latitude)
                        * Math.Sin(angularDistance)
                        * Math.Cos(bearing)));
            var destinationLongitude = longitude + Math.Atan2(
                Math.Sin(bearing)
                    * Math.Sin(angularDistance)
                    * Math.Cos(latitude),
                Math.Cos(angularDistance)
                    - (Math.Sin(latitude) * Math.Sin(destinationLatitude)));
            return new LatLong2(
                destinationLatitude * 180d / Math.PI,
                ((destinationLongitude * 180d / Math.PI + 540d) % 360d)
                    - 180d);
        }

        private static void validateHeading(int siteHeading)
        {
            if (siteHeading is < 0 or > 359)
                throw new ArgumentOutOfRangeException(
                    nameof(siteHeading),
                    "The site heading must be between 0 and 359 degrees.");
        }

        private static void validateRadius(double planetRadiusMeters)
        {
            if (!double.IsFinite(planetRadiusMeters)
                || planetRadiusMeters <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(planetRadiusMeters),
                    "The body radius must be positive.");
        }

        private static void validateLocation(
            LatLong2 location,
            string parameterName)
        {
            ArgumentNullException.ThrowIfNull(location, parameterName);
            var latitude = (double)location.Lat;
            var longitude = (double)location.Long;
            if (!double.IsFinite(latitude)
                || latitude is < -90 or > 90
                || !double.IsFinite(longitude)
                || longitude is < -180 or > 180)
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Latitude must be between -90 and 90 and longitude between -180 and 180 degrees.");
        }

        private static double normalizeDegrees(double degrees)
        {
            return ((degrees % 360d) + 360d) % 360d;
        }

        private static double degreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180d;
        }
    }
}
