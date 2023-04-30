using SrvSurvey.game;
using System;

namespace SrvSurvey.units
{

    /// <summary>
    /// Represents an angle, contrained between >= 0° and < 360°
    /// </summary>
    struct Angle
    {
        private const int limit = 360;

        public const double ratioDegreesToRadians = 180 / Math.PI;

        double n;

        public static Angle FromRadians(double radians)
        {
            var degrees = radians * ratioDegreesToRadians;
            return new Angle(degrees);
        }

        public Angle(double a)
        {
            //Game.log($"a: {a}");
            // wrap around if too high
            while (a >= limit) a -= limit;
            // wrap around if too low
            while (a < 0) a += limit;

            this.n = a;
        }

        public Angle(int n) : this((double)n)
        {
        }

        public Angle(decimal n) : this((double)n)
        {
        }

        public override string ToString()
        {
            return this.n.ToString("0") + "°";
        }

        public double radians
        {
            get
            {
                return this.n / ratioDegreesToRadians;
            }
        }

        public static Angle operator +(Angle l, int n)
        {
            var a = (l.n + n);

            // wrap around if too high
            while (a >= limit) a -= limit;

            return new Angle(a);
        }

        public static Angle operator -(Angle l, int n)
        {
            var a = l.n - n;

            // wrap around if too low
            while (a < 0) a += limit;

            return new Angle(a);
        }

        public static Angle operator -(int n, Angle r)
        {
            var a = n - r.n;

            // wrap around if too low
            while (a < 0) a += limit;

            return new Angle(a);
        }

        public static Angle operator +(Angle l, Angle r)
        {
            return l + r.n;
        }

        public static Angle operator -(Angle l, Angle r)
        {
            return l - r.n;
        }

        public static implicit operator double(Angle a)
        {
            return a.n;
        }

        public static implicit operator int(Angle a)
        {
            return (int)a.n;
        }

        public static implicit operator Angle(int n)
        {
            return new Angle(n);
        }

        public static implicit operator Angle(double n)
        {
            return new Angle(n);
        }

        public static implicit operator Angle(decimal n)
        {
            return new Angle(n);
        }
    }
}
