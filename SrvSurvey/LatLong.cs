using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SrvSurvey
{
    public interface ILocation
    {
        double Longitude { get; set; }
        double Latitude { get; set; }

    }

    // "imageOffset": "286,403",
    // "imageOffset": "328,462" / 12500.0, -2
    // "imageOffset": "361,464"/ 14000.0,-3
    // 394,526 //  -4

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct LatLong
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        public static readonly LatLong Empty = new LatLong(0, 0);

        public static double Scale = 1000 * 5.5; //?3 12

        public static PointF From(ILocation entry)
        {
            return new PointF((float)entry.Longitude, (float)entry.Latitude);
        }

        public double Lat { get; set; }
        public double Long { get; set; }

        public LatLong(double lat, double @long)
        {
            this.Lat = lat;
            this.Long = @long;
        }

        public LatLong(ILocation entry)
        {
            this.Lat = entry.Latitude;
            this.Long = entry.Longitude;
        }

        [JsonIgnore]
        public float X { get { return (float)(this.Long * Scale); } }
        [JsonIgnore]
        public float Y { get { return (float)(this.Lat * -Scale); } }


        public override string ToString()
        {
            return $"Lat:{this.Lat.ToString("0.000000")}, Long:{this.Long.ToString("0.000000")}";
        }

        public double AsDeltaDist(bool positiveOnly = false)
        {
            var dist = Math.Sqrt(Math.Pow(this.Lat, 2) + Math.Pow(this.Long, 2));
            if (this.Long < 0 && !positiveOnly) 
                return dist *= -1;
            else 
                return dist;
        }

        public PointF ToDeltaMeters(double ratio)
        {
            return new PointF(
                (float)(this.Long * ratio),
                (float)(this.Lat * ratio));
        }


        public double ToAngle()
        {
            var opp = this.Lat; //< 0 ? -this.Lat : this.Lat;
            var adj = this.Long; //< 0 ? -this.Long : this.Long;
            return Math.Atan(opp / adj);
        }

        public LatLong RotateBy(int siteHeading)
        {
            return this.RotateBy(LatLong.degToRad(siteHeading));
        }

        public LatLong RotateBy(double siteHeading)
        {
            var rad = siteHeading + this.ToAngle();
            var dist = this.AsDeltaDist();

            return new LatLong(
                Math.Sin(rad) * dist,
                Math.Cos(rad) * dist);
        }

        public static LatLong operator +(LatLong a, LatLong b)
        {
            return new LatLong(a.Lat + b.Lat, a.Long + b.Long);
        }

        public static LatLong operator -(LatLong a, LatLong b)
        {
            return new LatLong(a.Lat - b.Lat, a.Long - b.Long);
        }


        public static LatLong operator *(LatLong a, double scale)
        {
            return new LatLong(a.Lat * scale, a.Long * scale);
        }

        public static LatLong operator /(LatLong a, double scale)
        {
            return new LatLong(a.Lat / scale, a.Long / scale);
        }

        public static LatLong operator *(LatLong a, float scale)
        {
            return new LatLong(a.Lat * scale, a.Long * scale);
        }

        public static bool operator ==(LatLong a, LatLong b)
        {
            return a.Lat == b.Lat && a.Long == b.Long;
        }

        public static bool operator !=(LatLong a, LatLong b)
        {
            return a.Lat != b.Lat || a.Long != b.Long;
        }

        public static explicit operator PointF(LatLong latLong)
        {
            // apply scale to both value and invert the Y
            return new PointF(latLong.X, latLong.Y);
            //return new PointF(
            //    (float)(latLong.Long * Scale),
            //    (float)(latLong.Lat * -Scale));
        }

        public static double degToRad(int heading)
        {
            const double ratio = 180 / Math.PI;
            return heading / ratio;
        }
    }
}
