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

        private decimal n;

        public Longitude(decimal n)
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
                ? this.n.ToString("N6")
                : "+" + this.n.ToString("N6");
        }

        public static Longitude operator +(Longitude l, decimal n)
        {
            var a = l.n + n;

            // wrap around if too high
            while (a > limit) a -= 2 * limit;

            return new Longitude(a);
        }

        public static Longitude operator -(Longitude l, decimal n)
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

        public static implicit operator decimal(Longitude a)
        {
            return (decimal)a.n;
        }

        public static explicit operator double(Longitude a)
        {
            return (double)a.n;
        }

        public static implicit operator Longitude(double n)
        {
            return new Longitude((decimal)n);
        }

        public static implicit operator Longitude(decimal n)
        {
            return new Longitude(n);
        }

        public static explicit operator Longitude(int n)
        {
            return new Longitude(n);
        }
    }

}
