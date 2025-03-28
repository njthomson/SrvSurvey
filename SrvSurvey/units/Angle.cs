﻿using DecimalMath;

namespace SrvSurvey.units
{

    /// <summary>
    /// Represents an angle, constrained between >= 0° and < 360°
    /// </summary>
    struct Angle
    {
        private const decimal limit = 360;

        public const double ratioDegreesToRadians = 180 / Math.PI;

        decimal n;

        public static Angle FromRadians(double radians)
        {
            var degrees = radians * ratioDegreesToRadians;
            return new Angle(degrees);
        }

        public Angle(double a) : this((decimal)a)
        {
        }

        public Angle(int a) : this((decimal)a)
        {
        }

        public Angle(decimal a) 
        {
            //Game.log($"a: {a}");
            // wrap around if too high
            while (a >= limit) a -= limit;
            // wrap around if too low
            while (a < 0) a += limit;

            this.n = a;
        }

        public override string ToString()
        {
            return this.n.ToString("N2") + "°";
        }

        public decimal radians
        {
            get
            {
                return DecimalEx.ToRad(this.n); // / ratioDegreesToRadians;
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
            return (double)a.n;
        }

        public static implicit operator decimal(Angle a)
        {
            return a.n;
        }

        public static implicit operator int(Angle a)
        {
            return (int)a.n;
        }

        public static implicit operator float(Angle a)
        {
            return (float)a.n;
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
