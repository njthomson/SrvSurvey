using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Text;

namespace SrvSurvey
{
    /// <summary>
    /// Represents a class of Guardian Ruin or Structure. Eg: Alpha, Beta, Fistbump, etc
    /// </summary>
    class SiteTemplate
    {
        #region static loading code

        public static Dictionary<GuardianSiteData.SiteType, SiteTemplate> sites = new Dictionary<GuardianSiteData.SiteType, SiteTemplate>();

        public static void Import(bool devReload = false)
        {
            string filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "settlementTemplates.json");

            if (devReload)
                filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..", "settlementTemplates.json");

            if (File.Exists(filepath))
            {
                var json = File.ReadAllText(filepath);
                SiteTemplate.sites = JsonConvert.DeserializeObject<Dictionary<GuardianSiteData.SiteType, SiteTemplate>>(json)!;
                Game.log($"SiteTemplate.Imported {SiteTemplate.sites.Count} templates");

                /* Temp: Reformat POIs json 
                var txt = new StringBuilder("\r\n");
                txt.AppendLine("Alpha:");
                foreach (var poi in SiteTemplate.sites[GuardianSiteData.SiteType.alpha].poi)
                    txt.AppendFormat("  {{ \"name\": \"{0}\", \"dist\": {1}, \"angle\": {2}, \"type\": \"{3}\" }},\r\n", poi.name, poi.dist, poi.angle, poi.type);

                txt.AppendLine("Beta:");
                foreach (var poi in SiteTemplate.sites[GuardianSiteData.SiteType.beta].poi)
                    txt.AppendFormat("  {{ \"name\": \"{0}\", \"dist\": {1}, \"angle\": {2}, \"type\": \"{3}\" }},\r\n", poi.name, poi.dist, poi.angle, poi.type);

                txt.AppendLine("Gamma:");
                foreach (var poi in SiteTemplate.sites[GuardianSiteData.SiteType.gamma].poi)
                    txt.AppendFormat("  {{ \"name\": \"{0}\", \"dist\": {2}, \"angle\": {1}, \"type\": \"{3}\" }},\r\n", poi.name, poi.dist, poi.angle, poi.type);

                Game.log(txt);
                // End temp */
            }
            else
            {
                Game.log($"Missing file: {filepath}");
            }

        }

        #endregion

        #region instance members loaded from JSON

        /// <summary> The class/type name of this Guardian site. Eg: Alpha, Beta, Fistbump, etc </summary>
        public string name = "";

        /// <summary> The filename of the background image to use.</summary>
        public string backgroundImage = "";

        /// <summary> Offset applied to bring the site origin location to the center of the image </summary>
        public Point imageOffset = Point.Empty;

        /// <summary> The scale factor for the background image, adjusting that 1 pixel is 1 meter. </summary>
        public float scaleFactor = 1;

        public List<SitePOI> poi = new List<SitePOI>();

        #endregion

    }

    enum POIType
    {
        unknown = 0,
        relic,
        orb,
        casket,
        tablet,
        totem,
        urn,
        emptyPuddle,
        component,
        pylon,

        /* Possibilities for future use:
         * Obelisk or maybe just ActiveObelisk
         * ObeliskGroup
         * EneryPylon
         * EnergyTower
         * AncientBeacon
         * SkimmerSpawnPoint
         */
    }

    /// <summary> Some item or location expected within a Guardian site. </summary>
    class SitePOI
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public POIType type;
        public string name;
        public float angle;
        public float dist;

        public override string ToString()
        {
            return $"{type} {name} {angle}° {dist}m";
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SitePoiStatus
    {
        unknown,
        /// <summary> The POI has been confirmed present in some site. </summary>
        present,
        /// <summary> The POI has been confirmed NOT present in some site. </summary>
        absent,
        /// <summary> The POI has been confirmed present but with no item. Only applies to Puddles. </summary>
        empty,
    }
}
