namespace SrvSurvey.game
{
    /// <summary>
    /// Calculates orbital positions using Keplerian orbital mechanics.
    /// Based on standard two-body problem formulas (see Wikipedia: Orbital elements).
    /// </summary>
    internal class OrbitalCalculator
    {
        /// <summary>
        /// Double-precision 3D vector (System.Numerics.Vector3 uses float which loses
        /// precision at large orbital distances -- 100 AU in km needs more than 7 digits).
        /// </summary>
        public struct Vec3d
        {
            public double X, Y, Z;

            public Vec3d(double x, double y, double z) { X = x; Y = y; Z = z; }

            public static Vec3d operator +(Vec3d a, Vec3d b)
                => new Vec3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

            public double DistanceTo(Vec3d other)
            {
                double dx = X - other.X, dy = Y - other.Y, dz = Z - other.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }

        /// <summary>
        /// Represents a body's orbital parameters (Keplerian elements).
        /// </summary>
        public class OrbitalBody
        {
            public int BodyId { get; set; }
            public string Name { get; set; } = "";

            // Keplerian orbital elements
            public double SemiMajorAxis { get; set; }      // meters
            public double Eccentricity { get; set; }        // 0-1
            public double Inclination { get; set; }         // degrees
            public double ArgumentOfPeriapsis { get; set; }  // degrees (ω)
            public double LongitudeAscendingNode { get; set; } // degrees (Ω)
            public double MeanAnomalyAtEpoch { get; set; }  // degrees
            public DateTime Epoch { get; set; }             // timestamp for mean anomaly
            public double OrbitalPeriod { get; set; }       // seconds
            public double Radius { get; set; }              // meters (body radius)

            // Immediate parent only (first entry in journal Parents array)
            public int ParentId { get; set; } = -1;

            // Cached absolute position (km)
            public Vec3d Position { get; set; }
        }

        private Dictionary<int, OrbitalBody> bodies = new Dictionary<int, OrbitalBody>();

        /// <summary>
        /// Add a body to the orbital system.
        /// </summary>
        public void AddBody(OrbitalBody body)
        {
            bodies[body.BodyId] = body;
        }

        /// <summary>
        /// Calculate absolute positions of all bodies at the given time.
        /// Uses recursive descent so parents are always computed before children.
        /// </summary>
        public void UpdatePositions(DateTime time)
        {
            var computed = new HashSet<int>();
            foreach (var body in bodies.Values)
                ComputeAbsolutePosition(body, time, computed);
        }

        private void ComputeAbsolutePosition(OrbitalBody body, DateTime time, HashSet<int> computed)
        {
            if (computed.Contains(body.BodyId))
                return;

            // Compute this body's position relative to its immediate parent
            var relativePos = CalculateOrbitalPosition(body, time);
            body.Position = relativePos;

            // Add the immediate parent's absolute position (which already includes all ancestors)
            if (body.ParentId >= 0 && bodies.TryGetValue(body.ParentId, out var parent))
            {
                ComputeAbsolutePosition(parent, time, computed);
                body.Position = body.Position + parent.Position;
            }

            computed.Add(body.BodyId);
        }

        /// <summary>
        /// Calculate orbital position using Keplerian two-body mechanics.
        /// Returns position in kilometers relative to immediate parent.
        /// </summary>
        private Vec3d CalculateOrbitalPosition(OrbitalBody body, DateTime time)
        {
            // Root bodies (stars with no orbit) sit at the origin
            if (body.SemiMajorAxis <= 0 || body.OrbitalPeriod <= 0)
                return new Vec3d(0, 0, 0);

            // Convert to radians and km
            double a = body.SemiMajorAxis / 1000.0; // meters to km
            double e = body.Eccentricity;
            double i = body.Inclination * Math.PI / 180.0;
            double omega = body.ArgumentOfPeriapsis * Math.PI / 180.0;
            double Omega = body.LongitudeAscendingNode * Math.PI / 180.0;
            double M0 = body.MeanAnomalyAtEpoch * Math.PI / 180.0;

            // Mean Anomaly at current time
            double deltaTime = (time - body.Epoch).TotalSeconds;
            double meanMotion = 2.0 * Math.PI / body.OrbitalPeriod;
            double M = M0 + meanMotion * deltaTime;

            // Normalize to [0, 2π)
            M = M % (2.0 * Math.PI);
            if (M < 0) M += 2.0 * Math.PI;

            // Solve Kepler's equation for Eccentric Anomaly
            double E = SolveKeplersEquation(M, e);

            // True Anomaly via half-angle formula
            double nu = 2.0 * Math.Atan2(
                Math.Sqrt(1.0 + e) * Math.Sin(E / 2.0),
                Math.Sqrt(1.0 - e) * Math.Cos(E / 2.0)
            );

            // Radial distance from focus
            double r = a * (1.0 - e * Math.Cos(E));

            // Position in orbital plane (perifocal frame)
            double xP = r * Math.Cos(nu);
            double yP = r * Math.Sin(nu);

            // Perifocal-to-inertial rotation: R_z(-Ω) · R_x(-i) · R_z(-ω)
            double cO = Math.Cos(Omega), sO = Math.Sin(Omega);
            double ci = Math.Cos(i), si = Math.Sin(i);
            double cw = Math.Cos(omega), sw = Math.Sin(omega);

            double x = (cO * cw - sO * sw * ci) * xP + (-cO * sw - sO * cw * ci) * yP;
            double y = (sO * cw + cO * sw * ci) * xP + (-sO * sw + cO * cw * ci) * yP;
            double z = (sw * si) * xP + (cw * si) * yP;

            return new Vec3d(x, y, z);
        }

        /// <summary>
        /// Solve Kepler's equation M = E - e·sin(E) via Newton-Raphson iteration.
        /// </summary>
        private static double SolveKeplersEquation(double M, double e, double tol = 1e-10, int maxIter = 30)
        {
            // Initial guess (good for small e)
            double E = M + e * Math.Sin(M) * (1.0 + e * Math.Cos(M));

            for (int n = 0; n < maxIter; n++)
            {
                double dE = (E - e * Math.Sin(E) - M) / (1.0 - e * Math.Cos(E));
                E -= dE;
                if (Math.Abs(dE) < tol) break;
            }

            return E;
        }

        /// <summary>
        /// Euclidean distance between two bodies in light-seconds.
        /// </summary>
        public double GetDistanceLightSeconds(int bodyId1, int bodyId2)
        {
            if (!bodies.TryGetValue(bodyId1, out var b1) || !bodies.TryGetValue(bodyId2, out var b2))
                return double.MaxValue;

            double distanceKm = b1.Position.DistanceTo(b2.Position);
            return distanceKm / 299792.458;
        }

        public bool HasBody(int bodyId) => bodies.ContainsKey(bodyId);

        public OrbitalBody? GetBody(int bodyId)
            => bodies.TryGetValue(bodyId, out var body) ? body : null;

        public IEnumerable<OrbitalBody> GetAllBodies() => bodies.Values;
    }

    /// <summary>
    /// Route optimizer using greedy nearest-neighbor heuristic for TSP.
    /// </summary>
    internal static class RouteOptimizer
    {
        /// <summary>
        /// Find an efficient route visiting all target bodies, starting from startBodyId.
        /// Returns ordered list of body IDs including the start.
        /// </summary>
        public static List<int> OptimizeRoute(int startBodyId, List<int> targetBodyIds, OrbitalCalculator calculator)
        {
            var route = new List<int> { startBodyId };
            var remaining = new HashSet<int>(targetBodyIds.Where(id => id != startBodyId));

            if (remaining.Count == 0)
                return route;

            int current = startBodyId;

            while (remaining.Count > 0)
            {
                int nearest = -1;
                double minDist = double.MaxValue;

                foreach (var id in remaining)
                {
                    double d = calculator.GetDistanceLightSeconds(current, id);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = id;
                    }
                }

                if (nearest == -1) break;

                route.Add(nearest);
                remaining.Remove(nearest);
                current = nearest;
            }

            return route;
        }
    }
}
