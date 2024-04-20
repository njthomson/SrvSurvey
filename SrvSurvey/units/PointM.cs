using DecimalMath;
using System.Runtime.CompilerServices;

namespace SrvSurvey.units
{
    // An equivalent of Point or PointF but using Decimals
    internal struct PointM
    {
        public Point p1 = new Point(); // tmp
        public PointF p2 = new PointF(); // tmp
        public static void junk()
        {
            var pd = new PointM((float)1, (float)2);
        }


        public static readonly PointM Empty = new PointM(0m, 0m);

        public decimal x;
        public decimal y;

        public PointM(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"X,Y: {x},{y}";
        }

        public PointM(float x, float y) : this((decimal)x, (decimal)y) { }
        public PointM(double x, double y) : this((decimal)x, (decimal)y) { }

        public PointM(Point pt) : this((decimal)pt.X, (decimal)pt.Y) { }
        public PointM(PointF pf) : this((decimal)pf.X, (decimal)pf.Y) { }

        public decimal X { get => this.x; }
        public decimal Y { get => this.y; }

        public static explicit operator PointF(PointM pm)
        {
            return new PointF((float)pm.x, (float)pm.y);
        }

        public static PointM operator -(PointM a, PointM b)
        {
            return new PointM(a.x - b.x, a.y - b.y);
        }
        public static PointM operator -(PointF a, PointM b)
        {
            return new PointM((decimal)a.X - b.x, (decimal)a.Y - b.y);
        }

        public static PointM operator +(PointM a, PointM b)
        {
            return new PointM(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        /// Performs Pythagoras calculation to calculate the longest side of a triangle
        /// </summary>
        public decimal dist
        {
            get
            {
                return DecimalEx.Sqrt(DecimalEx.Pow(this.x, 2) + DecimalEx.Pow(this.y, 2));
            }
        }

        /// <summary>
        /// Returns the angle assuming a right-angle triangle
        /// </summary>
        public decimal angle
        {
            get
            {
                // x is opposite / y is adjacent
                var angle = DecimalEx.ToDeg(DecimalEx.ATan2(this.x, this.y));
                if (angle < 0) angle += 360;
                return angle;
            }
        }

        public PointM rotate(decimal rotationAngle)
        {
            var length = this.dist;
            var deg = this.angle + rotationAngle;
            var rad = DecimalEx.ToRad(deg);

            var dx = DecimalEx.Sin(rad) * length;
            var dy = DecimalEx.Cos(rad) * length;
            return new PointM(dx, dy);
        }

    }
}
