namespace SrvSurvey.units
{
    internal class Point3
    {
        public int x;
        public int y;
        public int z;

        public Point3() { }

        public Point3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"[ {x}, {y}, {z} ]";
        }

        public bool contains(int size, Point3 pt)
        {
            var dx = Math.Abs(x - pt.x);
            var dy = Math.Abs(y - pt.y);
            var dz = Math.Abs(z - pt.z);

            return dx < size && dy < size && dz < size;
        }

        public static Point3 operator +(Point3 pt1, Point3 pt2)
        {
            return new Point3
            {
                x = pt1.x + pt2.x,
                y = pt1.y + pt2.y,
                z = pt1.z + pt2.z,
            };
        }

        public static Point3 operator -(Point3 pt1, Point3 pt2)
        {
            return new Point3
            {
                x = pt1.x - pt2.x,
                y = pt1.y - pt2.y,
                z = pt1.z - pt2.z,
            };
        }

        public static Point3 operator /(Point3 pt, int n)
        {
            return new Point3
            {
                x = pt.x / n,
                y = pt.y / n,
                z = pt.z / n,
            };
        }

        public static Point3 operator *(Point3 pt, int n)
        {
            return new Point3
            {
                x = pt.x * n,
                y = pt.y * n,
                z = pt.z * n,
            };
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            var p = obj as Point3;
            return this.GetHashCode() == p?.GetHashCode();
        }

        public static bool operator ==(Point3? p1, Point3? p2)
        {
            return p1?.GetHashCode() == p2?.GetHashCode();
        }

        public static bool operator !=(Point3? p1, Point3? p2)
        {
            return p1?.GetHashCode() != p2?.GetHashCode();
        }
    }
}
