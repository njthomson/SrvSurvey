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

            public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
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

            public bool IsRoot { get; set; }
            public bool HasOrbitalElements { get; set; }

            // Cached absolute position (km)
            public Vec3d Position { get; set; }
            public bool PositionValid { get; set; }
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
            var visiting = new HashSet<int>();
            foreach (var body in bodies.Values)
                ComputeAbsolutePosition(body, time, computed, visiting);
        }

        private bool ComputeAbsolutePosition(OrbitalBody body, DateTime time, HashSet<int> computed, HashSet<int> visiting)
        {
            if (computed.Contains(body.BodyId))
                return body.PositionValid;

            if (!visiting.Add(body.BodyId))
            {
                body.PositionValid = false;
                computed.Add(body.BodyId);
                return false;
            }

            if (body.IsRoot)
            {
                body.Position = new Vec3d(0, 0, 0);
                body.PositionValid = true;
            }
            else if (!body.HasOrbitalElements || !TryCalculateOrbitalPosition(body, time, out var relativePos))
            {
                body.PositionValid = false;
            }
            else if (body.ParentId < 0 || !bodies.TryGetValue(body.ParentId, out var parent)
                || !ComputeAbsolutePosition(parent, time, computed, visiting))
            {
                body.PositionValid = false;
            }
            else
            {
                body.Position = relativePos + parent.Position;
                body.PositionValid = body.Position.IsFinite;
            }

            visiting.Remove(body.BodyId);
            computed.Add(body.BodyId);
            return body.PositionValid;
        }

        /// <summary>
        /// Calculate orbital position using Keplerian two-body mechanics.
        /// Returns position in kilometers relative to immediate parent.
        /// </summary>
        private bool TryCalculateOrbitalPosition(OrbitalBody body, DateTime time, out Vec3d position)
        {
            position = default;
            if (!double.IsFinite(body.SemiMajorAxis) || body.SemiMajorAxis <= 0
                || !double.IsFinite(body.OrbitalPeriod) || body.OrbitalPeriod <= 0
                || !double.IsFinite(body.Eccentricity) || body.Eccentricity < 0 || body.Eccentricity >= 1
                || !double.IsFinite(body.Inclination)
                || !double.IsFinite(body.ArgumentOfPeriapsis)
                || !double.IsFinite(body.LongitudeAscendingNode)
                || !double.IsFinite(body.MeanAnomalyAtEpoch))
                return false;

            // Convert to radians and km — negate angles to match game coordinate system
            double a = body.SemiMajorAxis / 1000.0; // meters to km
            double e = body.Eccentricity;
            double i = -(body.Inclination * Math.PI / 180.0);
            double omega = 2.0 * Math.PI - body.ArgumentOfPeriapsis * Math.PI / 180.0;
            double Omega = -(body.LongitudeAscendingNode * Math.PI / 180.0);
            double M0 = body.MeanAnomalyAtEpoch * Math.PI / 180.0;

            // Mean Anomaly at current time
            double deltaTime = (time - body.Epoch).TotalSeconds;
            double meanMotion = 2.0 * Math.PI / body.OrbitalPeriod;
            double M = M0 + meanMotion * deltaTime;

            // Normalize to [0, 2π)
            M = M % (2.0 * Math.PI);
            if (M < 0) M += 2.0 * Math.PI;

            // Solve Kepler's equation for Eccentric Anomaly
            if (!TrySolveKeplersEquation(M, e, out double E))
                return false;

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

            position = new Vec3d(x, y, z);
            return position.IsFinite;
        }

        /// <summary>
        /// Solve Kepler's equation M = E - e·sin(E) via Newton-Raphson iteration.
        /// </summary>
        private static bool TrySolveKeplersEquation(double M, double e, out double eccentricAnomaly, double tol = 1e-10, int maxIter = 30)
        {
            // Initial guess (good for small e)
            double E = M + e * Math.Sin(M) * (1.0 + e * Math.Cos(M));

            for (int n = 0; n < maxIter; n++)
            {
                double denominator = 1.0 - e * Math.Cos(E);
                if (!double.IsFinite(denominator) || Math.Abs(denominator) < double.Epsilon)
                {
                    eccentricAnomaly = default;
                    return false;
                }

                double dE = (E - e * Math.Sin(E) - M) / denominator;
                if (!double.IsFinite(dE))
                {
                    eccentricAnomaly = default;
                    return false;
                }

                E -= dE;
                if (Math.Abs(dE) < tol)
                {
                    eccentricAnomaly = E;
                    return true;
                }
            }

            eccentricAnomaly = default;
            return false;
        }

        /// <summary>
        /// Euclidean distance between two bodies in light-seconds.
        /// </summary>
        public double GetDistanceLightSeconds(int bodyId1, int bodyId2)
        {
            if (!bodies.TryGetValue(bodyId1, out var b1) || !b1.PositionValid
                || !bodies.TryGetValue(bodyId2, out var b2) || !b2.PositionValid)
                return double.PositiveInfinity;

            double distanceKm = b1.Position.DistanceTo(b2.Position);
            return distanceKm / 299792.458;
        }

        public bool HasBody(int bodyId) => bodies.ContainsKey(bodyId);

        public bool HasValidPosition(int bodyId)
            => bodies.TryGetValue(bodyId, out var body) && body.PositionValid;

        public OrbitalBody? GetBody(int bodyId)
            => bodies.TryGetValue(bodyId, out var body) ? body : null;

        public IEnumerable<OrbitalBody> GetAllBodies() => bodies.Values;
    }

    internal static class OrbitalHierarchy
    {
        public static bool IsSystemRoot(int parentId, bool isMainStar, bool isBarycentre)
            => parentId < 0 && (isMainStar || isBarycentre);

        /// <summary>
        /// Build immediate-parent relationships from journal parent chains. Each chain
        /// is ordered from the body's immediate parent through the system root.
        /// </summary>
        public static Dictionary<int, int> InferParentIds(
            IEnumerable<(int BodyId, IReadOnlyList<int> ParentChain)> bodies)
        {
            var parentIds = new Dictionary<int, int>();
            foreach (var (bodyId, parentChain) in bodies)
            {
                if (parentChain.Count == 0)
                    continue;

                parentIds.TryAdd(bodyId, parentChain[0]);
                for (int index = 0; index + 1 < parentChain.Count; index++)
                    parentIds.TryAdd(parentChain[index], parentChain[index + 1]);
            }
            return parentIds;
        }
    }

    /// <summary>
    /// Route optimizer: exact dynamic-programming solution for small systems, heuristic for large ones.
    /// </summary>
    internal static class RouteOptimizer
    {
        private const int ExactThreshold = 15;

        /// <summary>
        /// Find an efficient route visiting all target bodies, starting from startBodyId.
        /// Uses Held-Karp dynamic programming for up to 15 targets, nearest-neighbor + 2-opt above that.
        /// Returns ordered list of body IDs including the start.
        /// </summary>
        public static List<int> OptimizeRoute(int startBodyId, List<int> targetBodyIds, OrbitalCalculator calculator)
        {
            var targets = targetBodyIds.Where(id => id != startBodyId).Distinct().OrderBy(id => id).ToList();

            if (targets.Count == 0)
                return new List<int> { startBodyId };

            if (!calculator.HasValidPosition(startBodyId) || targets.Any(id => !calculator.HasValidPosition(id)))
                return new List<int>();

            if (targets.Count <= ExactThreshold)
                return ExactShortestRoute(startBodyId, targets, calculator);

            return HeuristicRoute(startBodyId, targets, calculator);
        }

        private static List<int> ExactShortestRoute(int startId, List<int> targets, OrbitalCalculator calc)
        {
            int count = targets.Count;
            int stateCount = 1 << count;
            var costs = new double[stateCount, count];
            var previous = new int[stateCount, count];

            for (int mask = 0; mask < stateCount; mask++)
            {
                for (int target = 0; target < count; target++)
                {
                    costs[mask, target] = double.PositiveInfinity;
                    previous[mask, target] = -1;
                }
            }

            for (int target = 0; target < count; target++)
                costs[1 << target, target] = calc.GetDistanceLightSeconds(startId, targets[target]);

            for (int mask = 1; mask < stateCount; mask++)
            {
                for (int last = 0; last < count; last++)
                {
                    if ((mask & (1 << last)) == 0 || !double.IsFinite(costs[mask, last]))
                        continue;

                    for (int next = 0; next < count; next++)
                    {
                        int nextBit = 1 << next;
                        if ((mask & nextBit) != 0)
                            continue;

                        int nextMask = mask | nextBit;
                        double candidate = costs[mask, last] + calc.GetDistanceLightSeconds(targets[last], targets[next]);
                        if (candidate < costs[nextMask, next])
                        {
                            costs[nextMask, next] = candidate;
                            previous[nextMask, next] = last;
                        }
                    }
                }
            }

            int fullMask = stateCount - 1;
            int bestLast = -1;
            double bestCost = double.PositiveInfinity;
            for (int last = 0; last < count; last++)
            {
                if (costs[fullMask, last] < bestCost)
                {
                    bestCost = costs[fullMask, last];
                    bestLast = last;
                }
            }

            if (bestLast < 0 || !double.IsFinite(bestCost))
                return new List<int>();

            var orderedTargets = new List<int>(count);
            int currentMask = fullMask;
            while (bestLast >= 0)
            {
                orderedTargets.Add(targets[bestLast]);
                int prior = previous[currentMask, bestLast];
                currentMask &= ~(1 << bestLast);
                bestLast = prior;
            }
            orderedTargets.Reverse();

            var route = new List<int>(count + 1) { startId };
            route.AddRange(orderedTargets);
            return route;
        }

        private static List<int> HeuristicRoute(int startId, List<int> targets, OrbitalCalculator calc)
        {
            var route = new List<int> { startId };
            var remaining = new SortedSet<int>(targets);
            int current = startId;

            while (remaining.Count > 0)
            {
                int nearest = -1;
                double minDist = double.MaxValue;

                foreach (var id in remaining)
                {
                    double d = calc.GetDistanceLightSeconds(current, id);
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

            if (route.Count > 3)
                Improve2Opt(route, calc);

            return route;
        }

        private static void Improve2Opt(List<int> route, OrbitalCalculator calc, int maxPasses = 20)
        {
            int n = route.Count;
            bool improved = true;
            int pass = 0;

            while (improved && pass < maxPasses)
            {
                improved = false;
                pass++;

                for (int i = 1; i < n - 1; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        double oldDist = calc.GetDistanceLightSeconds(route[i - 1], route[i]);
                        if (j + 1 < n)
                            oldDist += calc.GetDistanceLightSeconds(route[j], route[j + 1]);

                        double newDist = calc.GetDistanceLightSeconds(route[i - 1], route[j]);
                        if (j + 1 < n)
                            newDist += calc.GetDistanceLightSeconds(route[i], route[j + 1]);

                        if (newDist < oldDist)
                        {
                            route.Reverse(i, j - i + 1);
                            improved = true;
                        }
                    }
                }
            }
        }
    }
}
