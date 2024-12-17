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
            return $"[ {x}, {y}, {z}]";
        }
    }
}
