using Newtonsoft.Json;

namespace SrvSurvey.units
{
    internal class StarPos
    {
        private double[] coords = new double[3];

        [JsonIgnore]
        public string? systemName;

        public StarPos()
        {
        }

        public StarPos(double[] pos, string? systemName = null)
        {
            this.coords[0] = pos[0];
            this.coords[1] = pos[1];
            this.coords[2] = pos[2];
            this.systemName = systemName;
        }

        public StarPos(double x, double y, double z, string? systemName = null)
        {
            this.coords[0] = x;
            this.coords[1] = y;
            this.coords[2] = z;
            this.systemName = systemName;
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
