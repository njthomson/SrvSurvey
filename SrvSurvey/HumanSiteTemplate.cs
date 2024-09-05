using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace SrvSurvey
{
    internal class HumanSiteTemplate
    {
        #region static loading and saving code

        public static List<HumanSiteTemplate> templates = new List<HumanSiteTemplate>()
        {
            new HumanSiteTemplate
            {
                economy = Economy.Tourist,
                subType = 1,
                landingPads = new List<HumanSitePoi2> {}
            },
        };

        public static void import(bool devReload = false)
        {
            try
            {
                // TODO: pubData distribute?

                // otherwise, use the file shipped with the app
                string filepath;
                if (devReload)
                {
                    Game.log($"Using humanSiteTemplates.json, devReload:{devReload}");
                    filepath = Path.Combine(Application.StartupPath, "..\\..\\..\\..", "humanSiteTemplates.json");
                }
                else
                {
                    Game.log($"Using humanSiteTemplates.json app package");
                    filepath = Path.Combine(Application.StartupPath, "humanSiteTemplates.json");
                }

                Game.log($"Reading humanSiteTemplates.json: {filepath}");
                if (File.Exists(filepath))
                {
                    using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var json = reader.ReadToEnd();
                        var newSites = JsonConvert.DeserializeObject<List<HumanSiteTemplate>>(json)!;
                        templates = newSites;

                        Game.log($"HumanSiteTemplate.Imported {templates.Count} templates");
                    }
                }
                else
                {
                    Game.log($"Missing file: {filepath}");
                }
            }
            catch (Exception ex)
            {
                Game.log($"Loading humanSiteTemplates.json failed:\r\n{ex}");
            }
        }

        public static HumanSiteTemplate? get(Economy economy, int subType)
        {
            var match = templates.Find(_ => _.economy == economy && _.subType == subType);
            return match;
        }

        public static void export(bool devReload = false)
        {
            var json = JsonConvert.SerializeObject(templates, Formatting.Indented)!;
            var filepath = Path.Combine(Application.StartupPath, "..\\..\\..\\..", "humanSiteTemplates.json");

            File.WriteAllText(filepath, json);
        }

        public override string ToString()
        {
            return $"{this.economy} #{this.subType}";
        }

        #endregion

        public Economy economy;
        public int subType;

        /// <summary>
        /// Approx angle and distance each pad is from the site origin
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<HumanSitePoi2> landingPads;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<HumanSitePoi2> secureDoors;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<HumanSitePoi2> namedPoi;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<HumanSitePoi2> dataTerminals;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<Building2> buildings = new List<Building2>();

        public LandingPads landPadSummary()
        {
            var pads = new LandingPads();
            foreach (var pad in this.landingPads)
            {
                if (pad.size == LandingPadSize.Small)
                    pads.Small++;
                if (pad.size == LandingPadSize.Medium)
                    pads.Medium++;
                else if (pad.size == LandingPadSize.Large)
                    pads.Large++;
            }
            return pads;
        }

        public string? getCurrentBld(PointF cmdrOffset, float siteHeading)
        {
            if (this.buildings == null) return null;

            var offset2 = cmdrOffset;
            offset2.Y *= -1;

            foreach (var bld in this.buildings)
                foreach (var gp in bld.paths)
                    if (gp.IsVisible(offset2))
                        return bld.name;

            //if (this.buildings == null) return null;

            //foreach (var bld in this.buildings)
            //{
            //    var rr = new Region(bld.rect);

            //    var transformMatrix = new System.Drawing.Drawing2D.Matrix();
            //    transformMatrix.RotateAt(siteHeading, bld.rect.Location);
            //    //transformMatrix.RotateAt(0, bld.rect.Location);
            //    rr.Transform(transformMatrix);

            //    var hit = rr.IsVisible(cmdrOffset);
            //    if (hit)
            //        return bld.name;
            //}

            return null;
        }

    }

    // ---

    [JsonConverter(typeof(HumanSitePoi2.JsonConverter))]
    internal class HumanSitePoi2
    {
        /// <summary>
        /// Relative to site origin
        /// </summary>
        public PointF offset;

        /// <summary>
        /// The rotation of this POI about it's center
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int rot;

        /// <summary>
        /// The security level needed, either to access the relevant room or the device itself.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int level;

        /// <summary>
        /// 1 if ground floor or 2+ if upstairs
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int floor;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? name;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public LandingPadSize size;

        /// <summary>
        /// Not saved. If the cmdr has already downloaded data from this terminal, emptied this thing, etc.
        /// </summary>
        [JsonIgnore]
        public bool processed;

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JToken>(reader);
                if (obj == null || !obj.HasValues) return null;

                var data = new HumanSitePoi2
                {
                    offset = obj["offset"]!.ToObject<PointF>()!,
                    floor = obj["floor"]?.Value<int>() ?? 0,
                    level = obj["level"]?.Value<int>() ?? 0,
                    rot = obj["rot"]?.Value<int>() ?? 0,
                    name = obj["name"]?.Value<string>(),
                    size = Enum.Parse<LandingPadSize>(obj["size"]?.Value<string>() ?? LandingPadSize.Unknown.ToString(), true),
                };

                return data;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as HumanSitePoi2;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                var obj = new JObject();

                if (data.size > 0) obj["size"] = JToken.FromObject(data.size);
                if (data.name != null) obj["name"] = JToken.FromObject(data.name);

                obj["offset"] = new JObject
                {
                    {  "X", JToken.FromObject(data.offset.X) },
                    {  "Y", JToken.FromObject(data.offset.Y) },
                };

                if (data.size == 0)
                {
                    // all but landing pags
                    obj["floor"] = JToken.FromObject(data.floor);
                    obj["level"] = JToken.FromObject(data.level);
                }

                if (data.rot > 0) obj["rot"] = JToken.FromObject(data.rot);

                var json = JsonConvert.SerializeObject(obj); // no indentation
                json = json.Replace("{", "{ ")
                    .Replace("}", " }")
                    .Replace(",", ", ")
                    .Replace(":", ": ");
                writer.WriteRawValue(json);
            }
        }
    }

    // ---

    internal abstract class HumanSitePoi
    {
        /// <summary>
        /// Relative to site origin
        /// </summary>
        public PointF offset;

        /// <summary>
        /// OLD! The distance of this POI from the site origin
        /// </summary>
        public decimal dist;
        /// <summary>
        /// OLD! The angle of this POI from the site origin
        /// </summary>
        public float angle;

        /// <summary>
        /// The rotation of this POI about it's center
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float rot;

        /// <summary>
        /// The security level needed, either to access the relevant room or the device itself.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int level;

        /// <summary>
        /// 1 if ground floor or 2+ if upstairs
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int floor;

    }

    // For ad-hoc POIs, like the power regular or alarm console
    internal class NamedPoi : HumanSitePoi
    {
        public string name;
    }

    internal class DataTerminal : HumanSitePoi
    {
        /// <summary>
        /// If the cmdr has already downloaded data from this terminal. Not saved.
        /// </summary>
        [JsonIgnore]
        public bool downloaded;
    }

    internal class SecureDoor : HumanSitePoi
    {
    }

    internal class LandingPad : HumanSitePoi
    {
        public LandingPadSize size;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum LandingPadSize
    {
        Unknown,
        Small,
        Medium,
        Large
    }

    #region JSON serializer

    [JsonConverter(typeof(Building.JsonConverter))]
    internal class Building
    {
        public RectangleF rect;
        public int rot;
        public string name;

        public override string ToString()
        {
            return $"{this.name}_{rect.Left.ToString(CultureInfo.InvariantCulture)}_{rect.Top.ToString(CultureInfo.InvariantCulture)}_{rect.Width.ToString(CultureInfo.InvariantCulture)}_{rect.Height.ToString(CultureInfo.InvariantCulture)}_{rot.ToString(CultureInfo.InvariantCulture)}";
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                // "{name}_{left}_{top}_{width}_{height}_{rot}"
                // eg: "HAB_12.0_34.0_55.0_66.0_77"
                var parts = txt.Split('_');
                var pf = new PointF(float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));
                var sf = new SizeF(float.Parse(parts[3], CultureInfo.InvariantCulture), float.Parse(parts[4], CultureInfo.InvariantCulture));

                var building = new Building()
                {
                    rect = new RectangleF(pf, sf),
                    rot = int.Parse(parts[5], CultureInfo.InvariantCulture),
                    name = parts[0],
                };

                return building;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as Building;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // create a single string
                var txt = data.ToString();
                writer.WriteValue(txt);
            }
        }

        #endregion
    }

    [JsonConverter(typeof(Building2.JsonConverter))]
    internal class Building2
    {
        public List<GraphicsPath> paths = new List<GraphicsPath>();
        public string name;

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JToken>(reader);
                if (obj == null || !obj.HasValues) return null;

                var data = new Building2
                {
                    name = obj["name"]!.Value<string>()!,
                };

                data.paths = new List<GraphicsPath>();
                foreach (var pathObj in obj["paths"]!.Values<JObject>())
                {
                    var fillMode = pathObj!["FillMode"]?.Value<int>() ?? 0;
                    var pts = pathObj["PathPoints"]!.ToObject<PointF[]>()!;
                    var types = pathObj["PathTypes"]!.ToObject<byte[]>()!;
                    if (types.Length > pts.Length)
                    {
                        var list = types.ToList();
                        list.RemoveAt(list.Count - 1);
                        types = list.ToArray();
                    }
                    var gp = new GraphicsPath(pts, types, (FillMode)fillMode);
                    data.paths.Add(gp);
                }

                return data;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as Building2;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                writer.WriteStartObject();

                writer.WritePropertyName("name");
                writer.WriteValue(data.name);

                writer.WritePropertyName("paths");
                writer.WriteStartArray();

                foreach (var item in data.paths)
                {
                    if (item.PathData.Points?.Length > 0)
                    {
                        var pts = item.PathData.Points.Select(_ => new JObject
                    {
                        { "X", JToken.FromObject(float.Parse(_.X.ToString("N1"), CultureInfo.InvariantCulture)) },
                        { "Y", JToken.FromObject(float.Parse(_.Y.ToString("N1"), CultureInfo.InvariantCulture)) },
                    });

                        var pathObj = new JObject
                    {
                        { "PathPoints", JToken.FromObject(pts) },
                        { "PathTypes", JToken.FromObject(item.PathTypes) },
                        { "FillMode", JToken.FromObject(item.FillMode) },
                    };
                        var js = JsonConvert.SerializeObject(pathObj); // no indentation
                        writer.WriteRawValue(js);
                    }
                }

                writer.WriteEndArray();

                writer.WriteEndObject();
            }
        }
    }
}