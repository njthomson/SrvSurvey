using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    internal class ShipCenterOffsets
    {
        public static PointM get(string shipType)
        {
            return mapShipCockpitOffsets.GetValueOrDefault(shipType);
        }

        public static void set(string shipType, PointM offset)
        {
            mapShipCockpitOffsets[shipType] = offset;
        }

        /// <summary>
        /// Per ship type, the relative location, in meters, of the cockpit to the center of the ship.
        /// </summary>
        private static Dictionary<string, PointM> mapShipCockpitOffsets = new Dictionary<string, PointM>()
        {
            // Determined by measuring difference between Docked lat/long and pad center lat/long, converted to meters.
            // TODO: figure this out for all the other ships
            { "sidewinder", new PointM(0.0039735241560325261715803963, -1.8918079917214574007873715993) }, // Sidewinder
            // Eagle
            { "hauler", new PointM(0.0969766998443601240987358377, -12.599239384408765342054135780) }, // Hauler
            { "adder", new PointM(-1.0448119356622934101273911152, -11.715904797681276720908616160) }, // Adder
            { "empire_eagle", new PointM(0.2458336285994541250352346834, -8.536714551074841702761431858) }, // Imperial Eagle
            { "viper", new PointM(0.1229161697432489848756919882, -7.1826149264813328843504872434) }, // Viper mk3
            { "cobramkiii", new PointM(-0.1576497087354904399530401764, -9.031276393889643127040323461) }, // Cobra mk3
            { "viper_mkiv", new PointM(-0.0000027673979308549679875255, -8.065723234733315600077184502) }, // Viper mk4
            { "diamondback", new PointM(-0.0000041614913376638187396197, -9.890813997121282004764478288) }, // Diamondback Scout
            // Cobra mk4
            { "type6", new PointM(0, -20.957581116002204026636899079) }, // Type 6
            { "dolphin", new PointM(0.24276978, -19.054316) }, // Dolphin
            { "diamondbackxl", new PointM(0.5462154, -18.501362) }, // Diamondback Explorer
            { "empire_courier", new PointM(0, -14.442907807215595156071678409) }, // Imperial Courier
            { "independant_trader", new PointM(0, -21.080499480318932414951958905) }, // Keelback
            // Asp Scout
            { "vulture", new PointM(0.2458256590423615571253037271, -16.131446806855020630091723412) }, // Vulture
            { "asp", new PointM(0, -25.075346320612607494013645436) }, // Asp Explorer
            // Federal Dropship
            // Type 7
            { "typex", new PointM(0, -26.120152417304799609067982615) }, // Alliance Chieftain
            // Federal Assault ship
            // Imperial Clipper
            // Alliance Crusader
            { "typex_3", new PointM(-0.1499968019864161648299420609, -23.326543081110057742124991058) }, // challenger
            // Federal Gunship
            { "krait_light", new PointM(0.6031567730891498506706891727, -29.808101534971506981430079248) }, // Krait Phantom
            { "krait_mkii", new PointM(-0.4390055060502030101940869492, -28.642501220707376952320201527) }, // Krait mk2
            { "orca", new PointM(1.8251181979081937256954303615, -60.721671830785327505179094956) }, // Orca
            // Fer-de-lance
            // Mamba
            { "python", new PointM(0.0242815204676919790218357202, -27.803238864751802112883958858) }, // Python
            { "python_nx", new PointM(-0.1985071274895448505330679553, -27.652575857555383324975206283) }, // Python mk2
            { "type9", new PointM(0, -41.976621414162772124132252950) }, // Type 9
            { "belugaliner", new PointM(-0.1590069086272495414747604156, -96.06768779190352971899572620) }, // Beluga
            { "type9_military", new PointM(0, -41.976621414162772124132252950) }, // Type 10
            // Anaconda
            { "federation_corvette", new PointM(0, 17.577326097292171045687273834) }, // Federal Corvette
            { "cutter", new PointM(0, -78.975049073498041219641152452) }, // Imperial Cutter
        };
    }
}
