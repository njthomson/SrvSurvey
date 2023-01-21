using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.units
{
    /// <summary>
    /// Represents an angle at Longitude, contrained between -180° and < +180°
    /// </summary>
    struct Longitude
    {
        private const int limit = 180;

        private double n;

        public Longitude(double n)
        {
            this.n = n;

            // wrap around if too high
            while (this.n > limit) this.n -= 2 * limit;

            // wrap around if too low
            while (this.n < -limit) this.n += 2 * limit;
        }

        public override string ToString()
        {
            return this.n < 0
                ? this.n.ToString("0.000000")
                : "+" + this.n.ToString("0.000000");
        }

        public static Longitude operator +(Longitude l, double n)
        {
            var a = l.n + n;

            // wrap around if too high
            while (a > limit) a -= 2 * limit;

            return new Longitude(a);
        }

        public static Longitude operator -(Longitude l, double n)
        {
            var a = l.n - n;

            // wrap around if too low
            while (a < -limit) a += 2 * limit;

            return new Longitude(a);
        }

        public static Longitude operator +(Longitude l, Longitude r)
        {
            return l + r.n;
        }

        public static Longitude operator -(Longitude l, Longitude r)
        {
            return l - r.n;
        }

        public static implicit operator double(Longitude a)
        {
            return a.n;
        }

        public static implicit operator Longitude(double n)
        {
            return new Longitude(n);
        }

        public static explicit operator Longitude(int n)
        {
            return new Longitude(n);
        }
    }

}
