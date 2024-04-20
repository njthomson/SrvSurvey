using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;

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
    }

    internal abstract class HumanSitePoi
    {
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
    }

    // For ad-hoc POIs, like the power regular or alarm console
    internal class NamedPoi : HumanSitePoi
    {
        public string name;
    }

    internal class DataTerminal : HumanSitePoi
    {
        /// <summary>
        /// The security level needed for the room or building, not the terminal itself
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int level;

        /// <summary>
        /// Zero if ground floor or 1 if upstairs
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool floor;
    }

    internal class SecureDoor : HumanSitePoi
    {
        /// <summary>
        /// The security level needed:, 1, 2, or 3
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int level;

        /// <summary>
        /// True when this is an external door
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool airlock;
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
}
