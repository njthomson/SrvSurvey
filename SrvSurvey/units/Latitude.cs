namespace SrvSurvey.units
{
    /// <summary>
    /// Represents an angle at Latitude, constrained between -90° and < +90°
    /// </summary>
    public struct Latitude
    {
        private const int limit = 90;

        private decimal n;

        public Latitude(decimal n)
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

        public static Latitude operator +(Latitude l, decimal n)
        {
            var a = l.n + n;

            // wrap around if too high
            while (a > limit) a -= 2 * limit;

            return new Latitude(a);
        }

        public static Latitude operator -(Latitude l, decimal n)
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

        public static implicit operator decimal(Latitude a)
        {
            return a.n;
        }

        public static explicit operator double(Latitude a)
        {
            return (double)a.n;
        }

        public static implicit operator Latitude(decimal n)
        {
            return new Latitude(n);
        }
        public static implicit operator Latitude(double n)
        {
            return new Latitude((decimal)n);
        }
        public static explicit operator Latitude(int n)
        {
            return new Latitude(n);
        }
    }
}
