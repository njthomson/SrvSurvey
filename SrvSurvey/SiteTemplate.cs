using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Text;

namespace SrvSurvey
{
    /// <summary>
    /// Represents a class of Guardian Ruin or Structure. Eg: Alpha, Beta, Fistbump, etc
    /// </summary>
    class SiteTemplate
    {
        #region static loading code

        public static readonly Dictionary<GuardianSiteData.SiteType, SiteTemplate> sites = new Dictionary<GuardianSiteData.SiteType, SiteTemplate>();

        private static string editableFilepath = Path.Combine(Application.UserAppDataPath, "settlementTemplates.json");

        public static void Import(bool devReload = false)
        {
            string filepath = File.Exists(editableFilepath)
                ? editableFilepath
                : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "settlementTemplates.json");

            if (devReload)
                filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..", "settlementTemplates.json");

            if (File.Exists(filepath))
            {
                using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var json = reader.ReadToEnd();
                    var newSites = JsonConvert.DeserializeObject<Dictionary<GuardianSiteData.SiteType, SiteTemplate>>(json)!;
                    foreach(var _ in newSites)
                        SiteTemplate.sites[_.Key] = _.Value;

                    Game.log($"SiteTemplate.Imported {SiteTemplate.sites.Count} templates");
                }
            }
            else
            {
                Game.log($"Missing file: {filepath}");
            }
        }

        public static void SaveEdits()
        {
            Game.log($"Saving edits to SiteTemplates: {editableFilepath}");

            var json = JsonConvert.SerializeObject(SiteTemplate.sites);
            File.WriteAllText(editableFilepath, json);
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
        obelisk,
        brokeObelisk,

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
        public float rot;

        public override string ToString()
        {
            return $"{type} {name} {angle}° {dist}m rot:{rot}°";
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
