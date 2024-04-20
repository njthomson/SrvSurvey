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
        public static readonly LatLong2 Empty = new LatLong2(0d, 0d);

        public Latitude Lat;
        public Longitude Long;

        public LatLong2()
        {
            this.Lat = 0d;
            this.Long = 0d;
        }

        public LatLong2(double lat, double @long)
        {
            this.Lat = lat;
            this.Long = @long;
        }
        public LatLong2(decimal lat, decimal @long)
        {
            this.Lat = (double)lat;
            this.Long = (double)@long;
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