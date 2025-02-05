using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Diagnostics;
using System.Globalization;

namespace SrvSurvey.units
{
    internal class StarPos
    {
        public static StarPos Sol = new StarPos(0, 0, 0, "Sol", 10477373803);

        public double x;
        public double y;
        public double z;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? systemName;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long address;

        public StarPos()
        {
        }

        public StarPos(double[]? pos, string? systemName = null, long? address = null)
        {
            if (pos != null)
            {
                this.x = pos[0];
                this.y = pos[1];
                this.z = pos[2];
            }
            this.systemName = systemName;

            if (address.HasValue)
                this.address = address.Value;
        }

        public StarPos(double x, double y, double z, string? systemName = null, long? address = null)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.systemName = systemName;

            if (address.HasValue)
                this.address = address.Value;
        }

        public override string ToString()
        {
            return $"[ {x}, {y}, {z} ]";
        }

        public string ToUrlParams()
        {
            return $"x={x}&y={y}&z={z}";
        }

        public StarRef toReference()
        {
            return new StarRef()
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
            return new double[] { pos.x, pos.y, pos.z };
        }

        public static implicit operator StarPos(double[]? pos)
        {
            if (pos == null)
                return null!;
            else
                return new StarPos(pos);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode()
                + y.GetHashCode()
                + z.GetHashCode()
                + address.GetHashCode()
                + this.systemName?.GetHashCode() ?? 0;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as StarPos;
            return this.GetHashCode() == other?.GetHashCode();
        }

        public static bool operator ==(StarPos? s1, StarPos? s2)
        {
            return s1?.GetHashCode() == s2?.GetHashCode();
        }

        public static bool operator !=(StarPos? s1, StarPos? s2)
        {
            return s1?.GetHashCode() != s2?.GetHashCode();
        }
    }

    /// <summary> A reference to a star system </summary>
    [JsonConverter(typeof(StarRef.JsonConverter))]
    internal class StarRef
    {
        public string name;
        public long id64;
        public double x;
        public double y;
        public double z;

        public StarRef() { }

        public StarRef(double[] pos, string systemName, long address)
        {
            this.x = pos[0];
            this.y = pos[1];
            this.z = pos[2];
            this.name = systemName;
            this.id64 = address;
        }

        public StarRef(double x, double y, double z, string systemName, long address)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.name = systemName;
            this.id64 = address;
        }

        public static StarRef from(IStarRef entry)
        {
            return new StarRef()
            {
                name = entry.StarSystem,
                id64 = entry.SystemAddress,
                x = entry.StarPos[0],
                y = entry.StarPos[1],
                z = entry.StarPos[2],
            };
        }

        public StarPos toStarPos()
        {
            return new StarPos(this.x, this.y, this.z, this.name, this.id64);
        }

        public override string ToString()
        {
            return name; // ComboStarSystem relies on this
            //return $"{name} ({id64}) [ {x}, {y}, {z} ]";
        }

        public double getDistanceFrom(StarRef? there)
        {
            if (there == null) return 0;

            var dist = Math.Sqrt(
                Math.Pow(this.x - there.x, 2)
                + Math.Pow(this.y - there.y, 2)
                + Math.Pow(this.z - there.z, 2)
            );
            return dist;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode()
                + y.GetHashCode()
                + z.GetHashCode()
                + id64.GetHashCode()
                + this.name?.GetHashCode() ?? 0;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as StarRef;
            return this.GetHashCode() == other?.GetHashCode();
        }

        public static bool operator ==(StarRef? s1, StarRef? s2)
        {
            return s1?.GetHashCode() == s2?.GetHashCode();
        }

        public static bool operator !=(StarRef? s1, StarRef? s2)
        {
            return s1?.GetHashCode() != s2?.GetHashCode();
        }

        public static implicit operator double[](StarRef pos)
        {
            return new double[] { pos.x, pos.y, pos.z };
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return false;
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                try
                {
                    if (objectType != typeof(StarRef))
                    {
                        // TODO: is there a more elegant way than removing the converter type after it is auto-assigned?
                        serializer.ContractResolver.ResolveContract(objectType).Converter = null;
                        return serializer.Deserialize(reader, objectType);
                    }

                    var obj = JToken.Load(reader);
                    if (obj == null) return null;

                    // read old format
                    if (obj.Type == JTokenType.Object)
                    {
                        var star2 = new StarRef()
                        {
                            name = obj["name"]?.Value<string>()!,
                            id64 = obj["id64"]?.Value<long>() ?? 0,
                            x = obj["x"]?.Value<double>() ?? 0,
                            y = obj["y"]?.Value<double>() ?? 0,
                            z = obj["z"]?.Value<double>() ?? 0,
                        };
                        return star2;
                    }
                    else if (obj.Type == JTokenType.Null)
                        return null;
                    else if (obj.Type != JTokenType.String)
                        throw new Exception($"Unexpected json type: {obj.Type}");

                    // decode a joined string
                    var txt = obj.Value<string>()!;
                    var parts = txt.Split('|');
                    if (parts.Length != 5) throw new Exception($"Unexpected StarRef: {txt}");

                    var star = new StarRef()
                    {
                        name = parts[0],
                        id64 = long.Parse(parts[1], CultureInfo.InvariantCulture),
                        x = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        y = double.Parse(parts[3], CultureInfo.InvariantCulture),
                        z = double.Parse(parts[4], CultureInfo.InvariantCulture),
                    };
                    return star;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to parse StarRef: ${ex}");
                    Debugger.Break();
                    return null;
                }
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var star = value as StarRef;
                if (star == null) throw new Exception($"Unexpected type: {value?.GetType().Name}");

                // encode as a singular string
                var json = $"{star.name}|{star.id64}|{star.x}|{star.y}|{star.z}";
                writer.WriteValue(json);
            }
        }
    }
}
