using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.game;
using System.Diagnostics;


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
            }
            else
            {
                Game.log($"Missing file: {filepath}");
            }

            Game.log($"SiteTemplate.Import: {sites[GuardianSiteData.SiteType.beta].poi}");
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

        //public Dictionary<string, LatLong> relicTowers = new Dictionary<string, LatLong>();
        //public Dictionary<string, LatLong> puddles = new Dictionary<string, LatLong>();

        //        public string? getNearestRelicTower(LatLong dp, double siteHeading)
        //        {
        //            var match = this.getNearestFrom(dp, siteHeading, this.relicTowers);

        //#pragma warning disable CS8602 // Dereference of a possibly null reference.
        //            if (match.Item2 > 0.0015)
        //                return null;
        //            else
        //                return match.Item1;
        //#pragma warning restore CS8602 // Dereference of a possibly null reference.

        //        }

        //        public string? getNearestPuddle(LatLong dp, double siteHeading)
        //        {
        //            var match = this.getNearestFrom(dp, siteHeading, this.puddles);

        //#pragma warning disable CS8602 // Dereference of a possibly null reference.
        //            if (match.Item2 > 0.0015)
        //                return null;
        //            else
        //                return match.Item1;
        //#pragma warning restore CS8602 // Dereference of a possibly null reference.
        //        }

//        public Tuple<string, double> getNearestPoi(LatLong dp, int siteHeading)
//        {
//#pragma warning disable CS8603 // Possible null reference return.
//            return getNearestPoi(dp, LatLong.degToRad(siteHeading));
//#pragma warning restore CS8603 // Possible null reference return.
//        }

//        public Tuple<string, double>? getNearestPoi(LatLong dp, double siteHeading)
//        {
//            var relic = this.getNearestFrom(dp, siteHeading, this.relicTowers);
//            var puddle = this.getNearestFrom(dp, siteHeading, this.puddles);

//            if (relic != null && puddle != null)
//            {
//                if (relic.Item2 < puddle.Item2)
//                    return relic;
//                else
//                    return puddle;
//            }
//            else if (relic != null)
//            {
//                return relic;
//            }
//            else if (puddle != null)
//            {
//                return puddle;
//            }
//            else
//            {
//                return null;
//            }
//        }

//        private Tuple<string, double>? getNearestFrom(LatLong dp, double siteHeading, Dictionary<string, LatLong> dictionary)
//        {
//            // find the closest known tower
//#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
//            string minKeyName = null;
//#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
//            double minDist = 100;
//            foreach (var tower in dictionary)
//            {
//                // rotate tower position by site heading;
//                var delta = dp - tower.Value.RotateBy(siteHeading);
//                var dist = delta.AsDeltaDist(true);

//                Debug.WriteLine($"{tower.Key} => {dist}");

//                if (dist < minDist)
//                {
//                    minDist = dist;
//                    minKeyName = tower.Key;
//                }
//            }

//            // enforce a minimum distance of 0.015
//#pragma warning disable CS8604 // Possible null reference argument.
//            return new Tuple<string, double>(minKeyName, minDist);
//#pragma warning restore CS8604 // Possible null reference argument.
//        }

        //public PoiVector getNearestPoi2(LatLong location, int siteHeading, POIType poiType)
        //{
        //    return getNearestPoi2(location, LatLong.degToRad(siteHeading), poiType);

        //}

        //public PoiVector getNearestPoi2(LatLong location, double siteHeading, POIType poiType)
        //{
        //    // find the closest known tower
        //    var nearestPoi = new PoiVector
        //    {
        //        distance = double.MaxValue
        //    };
        //    //double minDist = double.MaxValue;

        //    foreach (var poi in this.poi)
        //    {
        //        // rotate tower position by site heading;
        //        var delta = location - poi.Value.location.RotateBy(siteHeading);

        //        var dist = delta.AsDeltaDist(true);

        //        Debug.WriteLine($"{poi.Key}: {dist}");

        //        if (dist < nearestPoi.distance)
        //        {
        //            nearestPoi = new PoiVector
        //            {
        //                poi = poi.Value,
        //                distance = dist,
        //                fromLocation = location
        //            };
        //        }
        //    }

        //    // enforce a minimum distance of 0.015
        //    return nearestPoi;
        //}
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

        /* Possibilities for future use:
         * Obelisk or maybe just ActiveObelisk
         * ObeliskGroup
         * EneryPylon
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
}
