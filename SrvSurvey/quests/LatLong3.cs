using Newtonsoft.Json;
using System.Globalization;

namespace SrvSurvey.quests;

/// <summary> Represents a trackabale location at some lat/long value, with a given size radius. </summary>
[JsonConverter(typeof(LatLong3.JsonConverter))]
public class LatLong3
{
    public double lat;
    public double @long;
    public double size;

    public LatLong3() { }

    public LatLong3(double lat, double @long, double size)
    {
        this.lat = lat;
        this.@long = @long;
        this.size = size;
    }

    public override string ToString()
    {
        return $"{lat},{@long},{size}";
    }

    class JsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType) { return false; }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var txt = serializer.Deserialize<string>(reader);
            if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

            // eg: "12.34,56.78,50"
            var parts = txt.Split(',', StringSplitOptions.TrimEntries);
            return new LatLong3()
            {
                lat = double.Parse(parts[0], CultureInfo.InvariantCulture),
                @long = double.Parse(parts[1], CultureInfo.InvariantCulture),
                size = double.Parse(parts[2], CultureInfo.InvariantCulture),
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is LatLong3)
                writer.WriteValue(value.ToString());
            else
                throw new Exception($"Unexpected value: {value?.GetType().Name}");
        }
    }
}
