using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace SrvSurvey
{
    enum POIType
    {
        Unknown = 0,
        RelicTower,
        Puddle,

        /* Possibilities for future use:
         * Obelisk or maybe just ActiveObelisk
         * ObeliskGroup
         * EneryPylon
         * AncientBeacon
         * SkimmerSpawnPoint
         */
    }

    /// <summary>
    /// Some item or location expected within a Guardian site
    /// </summary>
    class SitePOI
    {
        /// <summary>
        /// The maximum distance from which we will match some POI
        /// </summary>
        public static readonly double MaxMatchDistance = 0.0015;

        public POIType poiType { get; set; }
        public string name { get; set; }
        public LatLong location { get; set; }
    }

    class PoiVector
    {
        public SitePOI poi;
        public LatLong fromLocation;
        public double distance;
    }

    /// <summary>
    /// Represents a class of Guardian Ruin or Structure. Eg: Alpha, Beta, Fistbump, etc
    /// </summary>
    class SiteTemplate
    {
        /// <summary>
        /// The class/type name of this Guardian site. Eg: Alpha, Beta, Fistbump, etc
        /// </summary>
        public string name;
        public string backgroundImage;
        public Point imageOffset;
        public double scaleFactor;
        public SortedDictionary<string, SitePOI> poi = new SortedDictionary<string, SitePOI>();

        public Dictionary<string, LatLong> relicTowers;
        public Dictionary<string, LatLong> puddles;

        public string getNearestRelicTower(LatLong dp, double siteHeading)
        {
            var match = this.getNearestFrom(dp, siteHeading, this.relicTowers);

            if (match.Item2 > 0.0015)
                return null;
            else
                return match.Item1;

        }

        public string getNearestPuddle(LatLong dp, double siteHeading)
        {
            var match = this.getNearestFrom(dp, siteHeading, this.puddles);

            if (match.Item2 > 0.0015)
                return null;
            else
                return match.Item1;
        }

        public Tuple<string, double> getNearestPoi(LatLong dp, int siteHeading)
        {
            return getNearestPoi(dp, LatLong.degToRad(siteHeading));
        }

        public Tuple<string, double> getNearestPoi(LatLong dp, double siteHeading)
        {
            var relic = this.getNearestFrom(dp, siteHeading, this.relicTowers);
            var puddle = this.getNearestFrom(dp, siteHeading, this.puddles);

            if (relic != null && puddle != null)
            {
                if (relic.Item2 < puddle.Item2)
                    return relic;
                else
                    return puddle;
            }
            else if (relic != null)
            {
                return relic;
            }
            else if (puddle != null)
            {
                return puddle;
            }
            else
            {
                return null;
            }
        }

        private Tuple<string, double> getNearestFrom(LatLong dp, double siteHeading, Dictionary<string, LatLong> dictionary)
        {
            // find the closest known tower
            string minKeyName = null; ;
            double minDist = 100;
            foreach (var tower in dictionary)
            {
                // rotate tower position by site heading;
                var delta = dp - tower.Value.RotateBy(siteHeading);
                var dist = delta.AsDeltaDist(true);

                Debug.WriteLine($"{tower.Key} => {dist}");

                if (dist < minDist)
                {
                    minDist = dist;
                    minKeyName = tower.Key;
                }
            }

            // enforce a minimum distance of 0.015
            return new Tuple<string, double>(minKeyName, minDist);
        }

        public PoiVector getNearestPoi2(LatLong location, int siteHeading, POIType poiType)
        {
            return getNearestPoi2(location, LatLong.degToRad(siteHeading), poiType);

        }
        public PoiVector getNearestPoi2(LatLong location, double siteHeading, POIType poiType)
        {
            // find the closest known tower
            var nearestPoi = new PoiVector
            {
                distance = double.MaxValue
            };
            //double minDist = double.MaxValue;

            foreach (var poi in this.poi)
            {
                // rotate tower position by site heading;
                var delta = location - poi.Value.location.RotateBy(siteHeading);

                var dist = delta.AsDeltaDist(true);

                Debug.WriteLine($"{poi.Key}: {dist}");

                if (dist < nearestPoi.distance)
                {
                    nearestPoi = new PoiVector
                    {
                        poi = poi.Value,
                        distance = dist,
                        fromLocation = location
                    };
                }
            }

            // enforce a minimum distance of 0.015
            return nearestPoi;
        }


        public static Dictionary<string, SiteTemplate> sites = new Dictionary<string, SiteTemplate>();
        /*{
            { "alpha", new SiteTemplate()
                {
                    name = "Alpha",
                    backgroundImage = @"D:\code\SrvSurvey\SrvSurvey\SrvSurvey\ruins-alpha.png",
                    imageOffset = new Point(+14, -24),
                    scaleFactor = 1000 * 14,
                    relicTowers = new Dictionary<string, LatLong>()
                    {
                        {"RTA01", new LatLong( 0.005571, -0.000714) },
                        {"RTA02", new LatLong( -0.003042, -0.002250) },
                        {"RTA03", new LatLong(0.009132, -0.004215) },

                        {"RTA04", new LatLong( 0.002438, 0.019091) },
                        {"RTA05", new LatLong(-0.004513, 0.018145) },

                        {"RTA06", new LatLong( 0.029463, 0.002231) },
                        {"RTA07", new LatLong( 0.016322, -0.017851) },

                        {"RTA08", new LatLong( -0.009545, -0.011736) },
                        {"RTA09", new LatLong( -0.014174, -0.017521) },

                        {"RTA10", new LatLong(  -0.019463, 0.003967) },
                        {"RTA11", new LatLong( -0.026322, 0.000083) },
                    },
                    puddles = new Dictionary<string, LatLong>()
                    {
                        { "PDA01", new LatLong(0.009737, 0.002209) },
                        { "PDA02", new LatLong(-0.014450, -0.016139) },
                        { "PDA03", new LatLong(-0.012611, -0.020539) },
                        { "PDA04", new LatLong(-0.003225, -0.014385) },
                        { "PDA05", new LatLong(0.016530, -0.016110) },
                        { "PDA06", new LatLong(0.012999, -0.006119) },
                        { "PDA07", new LatLong(0.019927, -0.004309) },
                        { "PDA08", new LatLong(0.027808, 0.000581) },
                        { "PDA09", new LatLong(0.013032, 0.021323) },
                        { "PDA10", new LatLong(0.007404, 0.015641) },
                        { "PDA11", new LatLong(-0.007863, 0.016579) },
                        { "PDA12", new LatLong(-0.001361, 0.013901) },
                        { "PDA13", new LatLong(-0.026818, 0.004532) },
                        { "PDA14", new LatLong(-0.007460, 0.004540) },
                    },
                }
            },
            { "beta", new SiteTemplate()
                {
                    name = "Beta",
                    backgroundImage = @"D:\code\SrvSurvey\SrvSurvey\SrvSurvey\ruins-beta.png",
                    imageOffset = new Point(-5, +19),
                    scaleFactor = 1000 * 14,
                    relicTowers = new Dictionary<string, LatLong>()
                    {
                        { "RTB01", new LatLong(-0.010917, -0.003083) },
                        { "RTB02", new LatLong(-0.010417, +0.003667) },
                        { "RTB03", new LatLong(-0.009417, -0.015333) },
                        { "RTB04", new LatLong(0.005198, -0.011287) },
                        { "RTB05", new LatLong(0.009116, -0.005629) },
                    },
                    puddles = new Dictionary<string, LatLong>()
                    {

                        { "PDB01", new LatLong(-0.008934, -0.011960) },
                        { "PDB02", new LatLong(-0.013264, -0.009650) },
                        { "PDB03", new LatLong(0.002595, -0.008022) },
                        { "PDB04", new LatLong(0.008293, -0.011670) },
                        { "PDB05", new LatLong(0.002282, -0.012678) },
                        { "PDB06", new LatLong(0.002736, -0.004835) },
                        { "PDB07", new LatLong(-0.001758, 0.000834) },
                        { "PDB08", new LatLong(-0.004072, 0.004117) },
                        { "PDB09", new LatLong(-0.007923, 0.005995) },
                        { "PDB10", new LatLong(-0.010896, 0.014352) },
                        { "PDB11", new LatLong(-0.008810, 0.013380) },
                        { "PDB12", new LatLong(-0.017485, -0.004476) },
                        { "PDB13", new LatLong(-0.016288, -0.007224) },
                        { "PDB14", new LatLong(-0.010653, -0.005727) },
                        { "PDB15", new LatLong(-0.013206, -0.004937) },
                        { "PDB16", new LatLong(0.016088, 0.001673) },
                        { "PDB17", new LatLong(-0.011638, 0.000167) },
                        { "PDB18", new LatLong(-0.003233, 0.001591) },
                    },
                }
            },
        };*/

        public static void Export()
        {
            string filepath = Path.Combine(SrvSurvey.journalFolder, "survey", "settlementTemplates2.json");

            Directory.CreateDirectory(Path.Combine(SrvSurvey.journalFolder, "survey"));

            var json = JsonConvert.SerializeObject(SiteTemplate.sites, Formatting.Indented);
            File.WriteAllText(filepath, json);
        }

        public static void Import()
        {
            string filepath = Path.Combine(SrvSurvey.journalFolder, "survey", "settlementTemplates2.json");

            if (File.Exists(filepath))
            {
                var json = File.ReadAllText(filepath);
                SiteTemplate.sites = JsonConvert.DeserializeObject<Dictionary<string, SiteTemplate>>(json);
            }

            /* migrate
            foreach (var site in sites.Values)
            {
                foreach (var tower in site.relicTowers)
                {
                    var poi = new SitePOI
                    {
                        poiType = POIType.RelicTower,
                        name = tower.Key,
                        location = tower.Value,
                    };

                    site.poi.Add(tower.Key, poi);
                }

                foreach (var puddle in site.puddles)
                {
                    var poi = new SitePOI
                    {
                        poiType = POIType.Puddle,
                        name = puddle.Key,
                        location = puddle.Value,
                    };

                    site.poi.Add(puddle.Key, poi);
                }
            }

            Export();
            // */
            Debug.WriteLine("done");
        }
    }
}
