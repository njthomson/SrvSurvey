using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using System.Globalization;

namespace SrvSurvey
{
    internal class HumanSiteTemplate
    {
        #region static loading code

        public static List<HumanSiteTemplate> templates = new List<HumanSiteTemplate>()
        {
            new HumanSiteTemplate
            {
                economy = Economy.Tourist,
                subType = 1,
                landingPads = new List<LandingPad> {}
            },
        };

        public static void import(bool devReload = false)
        {
            // TODO: pubData distribute?

            // otherwise, use the file shipped with the app
            string filepath;
            if (devReload)
            {
                Game.log($"Using humanSiteTemplates.json, devReload:{devReload}");
                filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..", "humanSiteTemplates.json");
            }
            else
            {
                Game.log($"Using humanSiteTemplates.json app package");
                filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "humanSiteTemplates.json");
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

        public static HumanSiteTemplate? get(Economy economy, int subType)
        {
            var match = templates.Find(_ => _.economy == economy && _.subType == subType);
            return match;
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
        public List<LandingPad> landingPads;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<SecureDoor> secureDoors;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<DataTerminal> dataTerminals;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<NamedPoi> namedPoi;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<Building> buildings = new List<Building>();

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

            foreach (var bld in this.buildings)
            {
                var rr = new Region(bld.rect);

                var transformMatrix = new System.Drawing.Drawing2D.Matrix();
                transformMatrix.RotateAt(siteHeading - bld.rot, bld.rect.Location);

                rr.Transform(transformMatrix);

                if (rr.IsVisible(cmdrOffset))
                    return bld.name;
            }

            return null;
        }
    }

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
}
