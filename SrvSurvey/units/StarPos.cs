using Newtonsoft.Json;
using SrvSurvey.net;

namespace SrvSurvey.units
{
    internal class StarPos
    {
        public static StarPos Sol = new StarPos(0, 0, 0, "Sol", 10477373803);

        private double[] coords = new double[3];

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? systemName;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long address;

        public StarPos()
        {
        }

        public StarPos(double[] pos, string? systemName = null, long? address = null)
        {
            this.coords[0] = pos[0];
            this.coords[1] = pos[1];
            this.coords[2] = pos[2];
            this.systemName = systemName;

            if (address.HasValue)
                this.address = address.Value;
        }

        public StarPos(double x, double y, double z, string? systemName = null, long? address = null)
        {
            this.coords[0] = x;
            this.coords[1] = y;
            this.coords[2] = z;
            this.systemName = systemName;

            if (address.HasValue)
                this.address = address.Value;
        }

        public double x
        {
            get => coords[0];
            set { coords[0] = value; }
        }

        public double y
        {
            get => coords[1];
            set { coords[1] = value; }
        }

        public double z
        {
            get => coords[2];
            set { coords[2] = value; }
        }

        public override string ToString()
        {
            return $"[ {x}, {y}, {z} ]";
        }

        public string ToUrlParams()
        {
            return $"x={x}&y={y}&z={z}";
        }

        public Spansh.Reference toReference()
        {
            return new Spansh.Reference()
            {
                x = this.x,
                y = this.y,
                z = this.z,
                name = this.systemName!,
                id64 = this.address,
            };
        }

        public static implicit operator double[](StarPos pos)
        {
            return pos.coords;
        }

        public static implicit operator StarPos(double[] pos)
        {
            if (pos == null)
                return null!;
            else
                return new StarPos(pos);
        }
    }
}
