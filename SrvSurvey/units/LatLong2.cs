using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;

namespace SrvSurvey.units
{
    /// <summary>
    /// Represents a pair of Latitude/Longitude numbers for a location on a sphere
    /// </summary>

    [JsonConverter(typeof(LatLong2.JsonConverter))]
    class LatLong2
    {
        public static readonly LatLong2 Empty = new LatLong2(0, 0);

        public Latitude Lat;
        public Longitude Long;

        public LatLong2()
        {
            this.Lat = 0;
            this.Long = 0;
        }

        public LatLong2(double lat, double @long)
        {
            this.Lat = lat;
            this.Long = @long;
        }

        public LatLong2(ILocation entry)
        {
            this.Lat = entry.Latitude;
            this.Long = entry.Longitude;
        }

        public override string ToString()
        {
            return $"Lat/Long: {this.Lat},{this.Long}";
        }

        public override bool Equals(object? obj)
        {
            return this.ToString() == obj?.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Returns the distance of this LatLong as degrees to the origin.
        /// </summary>
        public double getDistance(bool positiveOnly)
        {
            var dist = Math.Sqrt(Math.Pow(this.Lat, 2) + Math.Pow(this.Long, 2));
            if (this.Long < 0 && !positiveOnly)
                return dist *= -1;
            else
                return dist;
        }

        public LatLong2 clone()
        {
            return new LatLong2(this.Lat, this.Long);
        }

        public static LatLong2 operator +(LatLong2 a, LatLong2 b)
        {
            return new LatLong2(a.Lat + b.Lat, a.Long + b.Long);
        }

        public static LatLong2 operator -(LatLong2 a, LatLong2 b)
        {
            return new LatLong2(a.Lat - b.Lat, a.Long - b.Long);
        }


        public static implicit operator LatLong2(LocationEntry location)
        {
            return new LatLong2(location);
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return false;
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JToken>(reader);
                if (obj == null || !obj.HasValues)
                    return null;

                var latlong = new LatLong2(
                    obj["lat"]!.Value<double>(),
                    obj["long"]!.Value<double>());
                return latlong;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var latLong = value as LatLong2;

                if (latLong == null)
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");

                var obj = new JObject();
                obj.Add("lat", (double)latLong.Lat);
                obj.Add("long", (double)latLong.Long);

                obj.WriteTo(writer);
            }
        }
    }
}