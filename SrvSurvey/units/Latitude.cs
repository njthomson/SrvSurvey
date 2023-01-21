using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.units
{
    /// <summary>
    /// Represents an angle at Latitude, contrained between -90° and < +90°
    /// </summary>
    struct Latitude
    {
        private const int limit = 90;

        private double n;

        public Latitude(double n)
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

        public static Latitude operator +(Latitude l, double n)
        {
            var a = l.n + n;

            // wrap around if too high
            while (a > limit) a -= 2 * limit;

            return new Latitude(a);
        }

        public static Latitude operator -(Latitude l, double n)
        {
            var a = l.n - n;

            // wrap around if too low
            while (a < -limit) a += 2*limit;

            return new Latitude(a);
        }

        public static Latitude operator +(Latitude l, Latitude r)
        {
            return l + r.n;
        }

        public static Latitude operator -(Latitude l, Latitude r)
        {
            return l - r.n;
        }

        public static implicit operator double(Latitude a)
        {
            return a.n;
        }

        public static implicit operator Latitude(double n)
        {
            return new Latitude(n);
        }

        public static explicit operator Latitude(int n)
        {
            return new Latitude(n);
        }
    }
}
