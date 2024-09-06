using Newtonsoft.Json;
using System.Text;

namespace SrvSurvey.game
{
    /// <summary>
    /// A base class for various data file classes
    /// </summary>
    internal abstract class Data
    {
        public static bool suppressLoadingMsg = false;

        [JsonIgnore]
        public string filepath { get; protected set; }

        public static T? Load<T>(string filepath) where T : Data
        {
            // read and parse file contents into tmp object
            if (File.Exists(filepath))
            {
                var json = File.ReadAllText(filepath);
                if (string.IsNullOrEmpty(json))
                {
                    Game.log($"Why is this data file empty?\r\n{filepath}");
                    return null;
                }

                try
                {
                    var data = JsonConvert.DeserializeObject<T>(json)!;
                    if (!suppressLoadingMsg)
                        Game.log($"Loaded data from: {filepath}");
                    data.filepath = filepath;
                    return data;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to read data from: {filepath}\r\n{ex.Message}");
                    Game.log(json);
                }
            }
            else
            {
                Game.log($"Data file not found: {filepath}");
            }

            return null;
        }

        public void Save()
        {
            if (this.filepath == null) return;

            var folder = Path.GetDirectoryName(this.filepath)!;
            Directory.CreateDirectory(folder);

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            if (json.Length == 0) return;

            var success = false;
            var attempts = 0;
            while (!success && attempts < 50)
            {
                try
                {
                    attempts++;
                    File.WriteAllText(this.filepath, json);
                    success = true;
                }
                catch
                {
                    Game.log($"Failed on attempt {attempts} to save: {this.filepath}");
                    // swallow and try again
                    Application.DoEvents();
                }
            }
        }
    }

    internal static class GalacticRegions
    {
        public static string current
        {
            get
            {
                var currentGalacticRegion = Game.activeGame?.cmdr?.galacticRegion ?? "";
                var mappedGalacticRegion = GalacticRegions.mapRegions.GetValueOrDefault(currentGalacticRegion);
                return mappedGalacticRegion ?? "???";
            }
        }

        public static int getIdxFromName(string name)
        {
            // eg: "$Codex_RegionName_1;"
            return int.Parse(name.Replace(";", "").Substring(18));
        }

        public static int getIdxFromDisplayName(string name)
        {
            var match = GalacticRegions.mapRegions.FirstOrDefault(_ => _.Value == name);
            return int.Parse(match.Key.Replace(";", "").Substring(18));
        }

        public static string getNameFromIdx(int regionId)
        {
            return $"$Codex_RegionName_{regionId};";
        }

        public static string getDisplayNameFromIdx(int regionId)
        {
            return mapRegions[$"$Codex_RegionName_{regionId};"];
        }

        public static string getIdxFromNames(string names)
        {
            var regions = new List<string>();

            foreach (var region in names.Split(","))
            {
                if (region.StartsWith("!~"))
                {
                    var excludeRegions = GalacticRegions.mapArms[region.Substring(1)];
                    var includeRegions = GalacticRegions.mapRegions.Values.Where(_ => !excludeRegions.Contains(_.Replace(" ", "")));
                    regions.AddRange(includeRegions.Select(_ => _.Replace(" ", "")));

                }
                else if (region.StartsWith("~"))
                {
                    regions.AddRange(GalacticRegions.mapArms[region].Split(','));
                }
                else
                {
                    regions.Add(region);
                }
            }

            var foo = regions.Select(reg =>
            {
                var match = mapRegions.FirstOrDefault(_ => _.Value.Replace(" ", "") == reg);
                if (match.Key == null) return 0;
                return getIdxFromName(match.Key);
            })
                .OrderBy(id => id);

            // eg: "$Codex_RegionName_1;"
            return string.Join(",", foo);
        }

        public static int? currentIdxOverride { private get; set; }

        public static int currentIdx
        {
            get
            {
                // use override if set (for testing purposes)
                if (currentIdxOverride != null) return currentIdxOverride.Value;

                if (Game.activeGame?.cmdr.galacticRegion == null) return 9; // default is 9 'Inner Scutum-Centaurus Arm'

                var idx = getIdxFromName(Game.activeGame.cmdr.galacticRegion);
                return idx;
            }
        }

        // Data mined from: https://canonn.science/codex/appendices/
        // Names have spaces removed but not dash or apostophe characters

        public static Dictionary<string, string> mapRegions = new Dictionary<string, string>()
        {
            { "$Codex_RegionName_1;", "Galactic Centre" },
            { "$Codex_RegionName_2;", "Empyrean Straits" },
            { "$Codex_RegionName_3;", "Ryker's Hope" },
            { "$Codex_RegionName_4;", "Odin's Hold" },
            { "$Codex_RegionName_5;", "Norma Arm" },
            { "$Codex_RegionName_6;", "Arcadian Stream" },
            { "$Codex_RegionName_7;", "Izanami" },
            { "$Codex_RegionName_8;", "Inner Orion-Perseus Conflux" },
            { "$Codex_RegionName_9;", "Inner Scutum-Centaurus Arm" },
            { "$Codex_RegionName_10;", "Norma Expanse" },
            { "$Codex_RegionName_11;", "Trojan Belt" },
            { "$Codex_RegionName_12;", "The Veils" },
            { "$Codex_RegionName_13;", "Newton's Vault" },
            { "$Codex_RegionName_14;", "The Conduit" },
            { "$Codex_RegionName_15;", "Outer Orion-Perseus Conflux" },
            { "$Codex_RegionName_16;", "Orion-Cygnus Arm" },
            { "$Codex_RegionName_17;", "Temple" },
            { "$Codex_RegionName_18;", "Inner Orion Spur" },
            { "$Codex_RegionName_19;", "Hawking's Gap" },
            { "$Codex_RegionName_20;", "Dryman's Point" },
            { "$Codex_RegionName_21;", "Sagittarius-Carina Arm" },
            { "$Codex_RegionName_22;", "Mare Somnia" },
            { "$Codex_RegionName_23;", "Acheron" },
            { "$Codex_RegionName_24;", "Formorian Frontier" },
            { "$Codex_RegionName_25;", "Hieronymus Delta" },
            { "$Codex_RegionName_26;", "Outer Scutum-Centaurus Arm" },
            { "$Codex_RegionName_27;", "Outer Arm" },
            { "$Codex_RegionName_28;", "Aquila's Halo" },
            { "$Codex_RegionName_29;", "Errant Marches" },
            { "$Codex_RegionName_30;", "Perseus Arm" },
            { "$Codex_RegionName_31;", "The Formidine Rift" },
            { "$Codex_RegionName_32;", "Vulcan Gate" },
            { "$Codex_RegionName_33;", "Elysian Shore" },
            { "$Codex_RegionName_34;", "Sanguineous Rim" },
            { "$Codex_RegionName_35;", "Outer Orion Spur" },
            { "$Codex_RegionName_36;", "Achilles's Altar" },
            { "$Codex_RegionName_37;", "Xibalba" },
            { "$Codex_RegionName_38;", "Lyra's Song" },
            { "$Codex_RegionName_39;", "Tenebrae" },
            { "$Codex_RegionName_40;", "The Abyss" },
            { "$Codex_RegionName_41;", "Kepler's Crest" },
            { "$Codex_RegionName_42;", "The Void" },
        };

        public static Dictionary<string, string> mapArms = new Dictionary<string, string>()
        {
            { "~OrionCygnusArm", "Izanami,InnerOrion-PerseusConflux,InnerScutum-CentaurusArm,Orion-CygnusArm,Temple,InnerOrionSpur,ElysianShore,SanguineousRim,OuterOrionSpur" },
            { "~OuterArm", "NormaArm,ArcadianStream,Newton'sVault,TheConduit,OuterArm,ErrantMarches,TheFormidineRift,Xibalba,Kepler'sCrest" },
            { "~ScutumCentaurusArm", "InnerScutum-CentaurusArm,NormaExpanse,TrojanBelt,TheVeils,FormorianFrontier,HieronymusDelta,OuterScutum-CentaurusArm,Aquila'sHalo,TheVoid" },
            { "~PerseusArm", "Izanami,OuterOrion-PerseusConflux,PerseusArm,VulcanGate,ElysianShore,SanguineousRim,Achilles'sAltar,Lyra'sSong,Tenebrae" },
            { "~SagittariusCarinaArm", "InnerOrion-PerseusConflux,Orion-CygnusArm,Temple,InnerOrionSpur,Hawking'sGap,Dryman'sPoint,Sagittarius-CarinaArm,MareSomnia,Acheron,OuterOrionSpur,TheAbyss" },
        };

        /* All Region:
         * Acheron Achilles'sAltar Aquila'sHalo ArcadianStream Dryman'sPoint ElysianShore EmpyreanStraits ErrantMarches FormorianFrontier GalacticCentre Hawking'sGap HieronymusDelta InnerOrionSpur InnerOrion-PerseusConflux InnerScutum-CentaurusArm Izanami
         * Kepler'sCrest Lyra'sSong MareSomnia Newton'sVault NormaArm NormaExpanse Odin'sHold Orion-CygnusArm OuterArm OuterOrionSpur OuterOrion-PerseusConflux OuterScutum-CentaurusArm PerseusArm Ryker'sHope VulcanGate Sagittarius-CarinaArm SanguineousRim
         * Temple Xibalba Tenebrae TheAbyss TheConduit TheFormidineRift TheVeils TheVoid TrojanBelt
         */
    }

    internal class GalacticNeblulae
    {
        public static double distToClosest(double[] here)
        {
            var minDist = nebulae
                .Select(_ => Util.getSystemDistance(here, _.starPos))
                .Min();

            return minDist;
        }

        private static List<GalacticNeblulae> nebulae = new List<GalacticNeblulae>()
        {
            { new GalacticNeblulae("CGN I-1", "Dryio Bli AA-A h214", 1, "Dryio Bli GJ-T c6-4182", new double[] { 1707, -86.375, 28689.46875 }) },
            { new GalacticNeblulae("CGN I-2", "Eoch Bli AA-A h108", 1, "Eoch Bli GL-F c27-2877", new double[] { 1869.96875, -306, 28263.9375 }) },
            { new GalacticNeblulae("CGN I-3", "Eok Bluae AA-A h334", 1, "Eok Bluae MU-V c16-2452", new double[] { 292.46875, -968.96875, 27839.71875 }) },
            { new GalacticNeblulae("CGN I-4", "Oudaitt AA-A h675", 1, "Oudaitt NH-L d8-326", new double[] { 2464.53125, -2041.8125, 27840.8125 }) },
            { new GalacticNeblulae("CGN I-5", "Phipoea AA-A h486", 1, "Phipoea HJ-D c27-388", new double[] { -487.96875, 501.0625, 28258.59375 }) },
            { new GalacticNeblulae("CGN I-6", "Wrupeou AA-A h564", 1, "Wrupeou YF-T b22-2", new double[] { 2313.53125, -1580.96875, 28862.0625 }) },
            { new GalacticNeblulae("CGN I-7", "Dryaa Pruae AA-A h133", 2, "Dryaa Pruae GG-O c6-226", new double[] { -2195.84375, -1184.09375, 20976.53125 }) },
            { new GalacticNeblulae("CGN I-8", "Dryaa Pruae AA-A h157", 2, "Dryaa Pruae BG-X d1-5841", new double[] { -2527.09375, -958.09375, 20882.59375 }) },
            { new GalacticNeblulae("CGN I-9", "Dryi Aoc AA-A h60", 2, "Dryi Aoc CH-C d13-2203", new double[] { 3186.34375, 405.40625, 19304 }) },
            { new GalacticNeblulae("CGN I-10", "Eoch Grea AA-A h188", 2, "Eoch Grea NS-T e3-307", new double[] { 4612.1875, -1835.90625, 30399.25 }) },
            { new GalacticNeblulae("CGN I-11", "Eor Auscs AA-A h18", 2, "Eor Auscs DE-S c20-500", new double[] { 3270.6875, 1054.3125, 20301.9375 }) },
            { new GalacticNeblulae("CGN I-12", "Eorl Auwsy AA-A h72", 2, "Eorl Auwsy LX-Z c27-215", new double[] { 4949.9375, 164, 20640.125 }) },
            { new GalacticNeblulae("CGN I-13", "Eorm Phyloi AA-A h13", 2, "Eorm Phyloi OY-Z d37", new double[] { 6548.3125, 2042.625, 27246.28125 }) },
            { new GalacticNeblulae("CGN I-14", "Hypuae Scrua AA-A h693", 2, "Hypuae Scrua FL-W d2-61", new double[] { 833.0625, 2024.40625, 22247.34375 }) },
            { new GalacticNeblulae("CGN I-15", "Mylaifai AA-A h528", 2, "Mylaifai CN-C b45-44", new double[] { -608.0625, -340.40625, 19105.90625 }) },
            { new GalacticNeblulae("CGN I-16", "Pheia Auscs AA-A h23", 2, "Pheia Auscs KO-L c23-38", new double[] { 8677.5625, 772.125, 26846.6875 }) },
            { new GalacticNeblulae("CGN I-17", "Scheau Prao AA-A h401", 2, "Scheau Prao XF-E d12-1389", new double[] { 2005.34375, -816.03125, 25631.6875 }) },
            { new GalacticNeblulae("CGN I-18", "Shrogaae AA-A h78", 2, "Shrogaae LB-C c14-1067", new double[] { 4958.21875, 507.6875, 21307.96875 }) },
            { new GalacticNeblulae("CGN I-19", "Eorm Breae AA-A h514", 3, "Eorm Breae MT-D c27-21", new double[] { -117.90625, 1880.03125, 30847.75 }) },
            { new GalacticNeblulae("CGN I-20", "Greeroi AA-A h3", 3, "Greeroi MT-O d7-3", new double[] { 4617.96875, 1193.53125, 37984.65625 }) },
            { new GalacticNeblulae("CGN I-21", "Hypuae Briae AA-A h268", 3, "Hypuae Briae UM-Z b57-67", new double[] { 1064.84375, 486.21875, 36033.28125 }) },
            { new GalacticNeblulae("CGN I-22", "Hyuqu AA-A h7", 3, "Hyuqu IG-X c1-171", new double[] { 2849.5, -1107.25, 34868.59375 }) },
            { new GalacticNeblulae("CGN I-23", "Lyaisoo AA-A h87", 3, "Lyaisoo XL-D c28-163", new double[] { 3818.46875, 906.5, 32135.40625 }) },
            { new GalacticNeblulae("CGN I-24", "Scheau Blao AA-A h513", 3, "Scheau Blao QJ-Y b56-20", new double[] { 4310.78125, -1098.9375, 33448.25 }) },
            { new GalacticNeblulae("CGN I-25", "Scheau Bli AA-A h154", 3, "Scheau Bli HZ-B c28-188", new double[] { 2057.125, -671.84375, 33454.625 }) },
            { new GalacticNeblulae("CGN I-26", "Hypou Briae AA-A h40", 3, "Hypou Briae WD-T c3-2071", new double[] { -1007.875, 35.6875, 34959.53125 }) },
            { new GalacticNeblulae("CGN I-27", "Agnairt AA-A h36", 4, "Agnairt TA-U d4-360", new double[] { -10010.375, -33.71875, 22444.25 }) },
            { new GalacticNeblulae("CGN I-28", "Dryio Bloo AA-A h310", 4, "Dryio Bloo LT-Y d1-852", new double[] { -6340.0625, -1617.34375, 28578.71875 }) },
            { new GalacticNeblulae("CGN I-29", "Eimbaisys AA-A h605", 4, "Eimbaisys WK-O d6-87", new double[] { -4958.3125, 1641.3125, 30248.25 }) },
            { new GalacticNeblulae("CGN I-30", "Eorld Grie AA-A h578", 4, "Eorld Grie DE-E d13-1757", new double[] { -3612, -1356.75, 30838.25 }) },
            { new GalacticNeblulae("CGN I-31", "Foijaea AA-A h129", 4, "Foijaea VY-A e778", new double[] { -4243.03125, -1730.5, 32332.65625 }) },
            { new GalacticNeblulae("CGN I-32", "Hypiae Ausms AA-A h226", 4, "Hypiae Ausms LA-H c13-0", new double[] { -8416.5625, 2476.28125, 25103.4375 }) },
            { new GalacticNeblulae("CGN I-33", "Hypo Auf AA-A h37", 4, "Hypo Auf IZ-R c20-1", new double[] { -9397.34375, 2288.59375, 25440.0625 }) },
            { new GalacticNeblulae("CGN I-34", "Myumbai AA-A h235", 4, "Myumbai OK-D c13-11", new double[] { -6212.40625, -2143.78125, 22553.1875 }) },
            { new GalacticNeblulae("CGN I-35", "Phrae Flyou AA-A h30", 4, "Phrae Flyou EC-Y c16-383", new double[] { -10610.90625, -490.84375, 23987.65625 }) },
            { new GalacticNeblulae("CGN I-36", "Tepuae AA-A h503", 4, "Tepuae FJ-R c21-53", new double[] { -7929.96875, -1314.28125, 24180.75 }) },
            { new GalacticNeblulae("CGN I-37", "Xothuia AA-A h34", 4, "Xothuia ZJ-E c13-17", new double[] { -6660.65625, 633.21875, 30244.9375 }) },
            { new GalacticNeblulae("CGN I-38", "Xothuia AA-A h9", 4, "Xothuia HH-W b57-35", new double[] { -6516.15625, 138.65625, 30904 }) },
            { new GalacticNeblulae("CGN I-39", "Boepp AA-A h83", 5, "Boepp TU-E d12-1492", new double[] { -461.59375, -552.125, 16690.21875 }) },
            { new GalacticNeblulae("CGN I-40", "Boewnst AA-A h118", 5, "Boewnst JW-D c12-12", new double[] { -5784.53125, -1114.6875, 16111.375 }) },
            { new GalacticNeblulae("CGN I-41", "Byaa Ain AA-A h22", 5, "Byaa Ain XK-R c7-89", new double[] { -4254.625, 1136.6875, 15916.9375 }) },
            { new GalacticNeblulae("CGN I-42", "Byua Aim AA-A h16", 5, "Byua Aim NU-Q b34-79", new double[] { -3156.78125, 397.96875, 16325.96875 }) },
            { new GalacticNeblulae("CGN I-43", "Byua Aim AA-A h63", 5, "Byua Aim SR-L c24-8", new double[] { -2936.09375, 1168.15625, 16651.46875 }) },
            { new GalacticNeblulae("CGN I-44", "Eeshorks AA-A h15", 5, "Eeshorks ZK-B b2-9", new double[] { 1564.875, -782.875, 16906.375 }) },
            { new GalacticNeblulae("CGN I-45", "Eeshorps AA-A h80", 5, "Eeshorps FG-F b43-86", new double[] { 4037.21875, -424.71875, 17790.96875 }) },
            { new GalacticNeblulae("CGN I-46", "Eor Aoc AA-A h70", 5, "Eor Aoc EH-L d8-1344", new double[] { 2982, 509.09375, 17635.28125 }) },
            { new GalacticNeblulae("CGN I-47", "Floalt AA-A h110", 5, "Floalt KU-E d12-60", new double[] { -4996.96875, -572.625, 15341.3125 }) },
            { new GalacticNeblulae("CGN I-48", "Floarks AA-A h77", 5, "Floarks YR-Z c27-37", new double[] { -3342.34375, -1158.25, 15510.90625 }) },
            { new GalacticNeblulae("CGN I-49", "Floawns AA-A h359", 5, "Floawns EL-R c7-164", new double[] { 2450.0625, -111.53125, 14630.96875 }) },
            { new GalacticNeblulae("CGN I-50", "Froarks AA-A h22", 5, "Froarks TO-M c9-143", new double[] { -404.59375, 886.71875, 14724.25 }) },
            { new GalacticNeblulae("CGN I-51", "Greae Phio AA-A h33", 5, "Greae Phio LS-L c23-797", new double[] { 1354.03125, -487.15625, 16593.125 }) },
            { new GalacticNeblulae("CGN I-52", "Greae Phoea AA-A h41", 5, "Greae Phoea TZ-E d12-955", new double[] { 4822.625, -471.4375, 16657.125 }) },
            { new GalacticNeblulae("CGN I-53", "Gru Phio AA-A h52", 5, "Gru Phio CZ-K b11-25", new double[] { 3392.34375, -654.90625, 15834.21875 }) },
            { new GalacticNeblulae("CGN I-54", "Iowhail AA-A h93", 5, "Iowhail UI-K d8-7646", new double[] { -1648.78125, 110.65625, 16340.90625 }) },
            { new GalacticNeblulae("CGN I-55", "Mynoaw AA-A h23", 5, "Mynoaw NY-K b37-55", new double[] { 4690.6875, -934.875, 18940.34375 }) },
            { new GalacticNeblulae("CGN I-56", "Mynoaw AA-A h80", 5, "Mynoaw TK-S c19-105", new double[] { 4704.0625, -567.21875, 18980.5625 }) },
            { new GalacticNeblulae("CGN I-57", "Stranoa AA-A h3", 5, "Stranoa ST-Z c13-87", new double[] { 4261.28125, 1306.03125, 18743.125 }) },
            { new GalacticNeblulae("CGN I-58", "Egnaix AA-A h91", 6, "Egnaix GW-V e2-0", new double[] { 6124.28125, 2296.15625, 22529.3125 }) },
            { new GalacticNeblulae("CGN I-59", "Eord Prau AA-A h12", 6, "Eord Prau YP-N d7-2745", new double[] { 5862.59375, -557.5625, 20057.46875 }) },
            { new GalacticNeblulae("CGN I-60", "Hypio Prao AA-A h16", 6, "Hypio Prao NZ-J c24-30", new double[] { 8697.15625, -468.4375, 25586.84375 }) },
            { new GalacticNeblulae("CGN I-61", "Hypoe Ploe AA-A h27", 6, "Hypoe Ploe OU-F b27-15", new double[] { 8939.15625, -766.65625, 22574.5 }) },
            { new GalacticNeblulae("CGN I-62", "Bleethue AA-A h36", 7, "Bleethue KR-A b15-0", new double[] { -7171.15625, -992.875, 37657.8125 }) },
            { new GalacticNeblulae("CGN I-63", "Dryiquae AA-A h32", 7, "Dryiquae OI-B d13-0", new double[] { -4353.15625, -1284.1875, 41088.4375 }) },
            { new GalacticNeblulae("CGN I-64", "Phleedgoea AA-A h108", 7, "Phleedgoea NZ-L c22-21", new double[] { -8227, -794.8125, 34489 }) },
            { new GalacticNeblulae("CGN I-65", "Phleedgoe AA-A h40", 7, "Phleedgoe FY-L c23-13", new double[] { -10556.84375, -453.6875, 34515 }) },
            { new GalacticNeblulae("CGN I-66", "Phoi Bre AA-A h8", 7, "Phoi Bre KY-E b20", new double[] { -8823.625, 512.8125, 36062.40625 }) },
            { new GalacticNeblulae("CGN I-67", "Phoi Phyloea AA-A h167", 7, "Freasai QL-P c5-0", new double[] { -5743.46875, 2537.1875, 33736.65625 }) },
            { new GalacticNeblulae("CGN I-68", "Phraa Byoe AA-A h14", 7, "Phraa Byoe ZZ-V a118-1", new double[] { 12.625, -669.1875, 37333.90625 }) },
            { new GalacticNeblulae("CGN I-69", "Phroea Bluae AA-A h19", 7, "Phroea Bluae LI-E c14-551", new double[] { -4087.46875, -297.09375, 34106.84375 }) },
            { new GalacticNeblulae("CGN I-70", "Phroea Gree AA-A h34", 7, "Phroea Gree GK-H c26-555", new double[] { -6859.78125, -266.3125, 37194.5625 }) },
            { new GalacticNeblulae("CGN I-71", "Scheau Byoe AA-A h187", 7, "Scheau Byoe XJ-P d6-736", new double[] { -5379.21875, -579.21875, 35354.21875 }) },
            { new GalacticNeblulae("CGN I-72", "Segnao AA-A h50", 7, "Segnao TU-I c12-174", new double[] { -10466.6875, -103.4375, 36610.1875 }) },
            { new GalacticNeblulae("CGN I-73", "Teqo AA-A h45", 7, "Teqo EB-V c16-10", new double[] { -8297.875, -1143.9375, 36791.03125 }) },
            { new GalacticNeblulae("CGN I-74", "Vegnue AA-A h17", 7, "Vegnue PY-M b51-4", new double[] { -5676, 608.40625, 37172.9375 }) },
            { new GalacticNeblulae("CGN I-75", "Bleia5 YE-A h30", 7, "Bleia5 KX-S c4-812", new double[] { -5767.25, -567.09375, 12195.3125 }) },
            { new GalacticNeblulae("CGN I-76", "Dryae Bliae AA-A h45", 8, "Dryae Bliae YH-C c13-2", new double[] { -12937.6875, -1097.34375, 28938.96875 }) },
            { new GalacticNeblulae("CGN I-77", "Dryae Greau AA-A h37", 8, "Dryae Greau HU-X b34-2", new double[] { -11031.03125, -143.90625, 31689.59375 }) },
            { new GalacticNeblulae("CGN I-78", "Dryio Gree AA-A h40", 8, "Dryio Gree MV-I a19-0", new double[] { -8300.71875, -354.84375, 31138.25 }) },
            { new GalacticNeblulae("CGN I-79", "Dryoea Gree AA-A h66", 8, "Dryoea Gree CH-J a34-0", new double[] { -6982.21875, -213.53125, 31295.25 }) },
            { new GalacticNeblulae("CGN I-80", "Dryuae Bre AA-A h64", 8, "Dryuae Bre LX-X b48-1", new double[] { -8429.53125, 1198.625, 31989.625 }) },
            { new GalacticNeblulae("CGN I-81", "Eorl Bre AA-A h3", 8, "Eorl Bre NK-U b33-23", new double[] { -8841.65625, 602.5625, 30381.09375 }) },
            { new GalacticNeblulae("CGN I-82", "Hypua Flyoae AA-A h52", 8, "Hypua Flyoae LU-Y b20-38", new double[] { -12477.8125, -68.46875, 22415.3125 }) },
            { new GalacticNeblulae("CGN I-83", "Hypua Flyoae AA-A h83", 8, "Hypua Flyoae ZT-L c22-30", new double[] { -12722.78125, -843.90625, 22964.96875 }) },
            { new GalacticNeblulae("CGN I-84", "Oob Brue AA-A h0", 8, "Oob Brue BI-V c5-0", new double[] { -13622.15625, 1215.84375, 29896.875 }) },
            { new GalacticNeblulae("CGN I-85", "Oob Chreou AA-A h28", 8, "Oob Chreou PO-K b8-7", new double[] { -10873.03125, 16.25, 27288.15625 }) },
            { new GalacticNeblulae("CGN I-86", "Phreia Flyou AA-A h63", 8, "Phreia Flyou UB-A b32-0", new double[] { -14503.84375, -458.4375, 23947.90625 }) },
            { new GalacticNeblulae("CGN I-87", "Stuemiae AA-A h63", 8, "Stuemiae DT-W b58-5", new double[] { -9788.75, 380.0625, 27085.90625 }) },
            { new GalacticNeblulae("CGN I-88", "Xothaei AA-A h49", 8, "Xothaei GW-S a36-2", new double[] { -9239.5625, 477.3125, 30054 }) },
            { new GalacticNeblulae("CGN I-89", "Xothaei AA-A h52", 8, "Xothaei ML-I c24-4", new double[] { -9886.0625, 466.0625, 30701 }) },
            { new GalacticNeblulae("CGN I-90", "Xothuia AA-A h95", 8, "Xothuia KP-E d12-93", new double[] { -7742.8125, 660.375, 30714.09375 }) },
            { new GalacticNeblulae("CGN I-91", "Blaa Eaec AA-A h86", 9, "Blaa Eaec FD-V b19-49", new double[] { -7761.96875, -650.03125, 14729.46875 }) },
            { new GalacticNeblulae("CGN I-92", "Blaa Hypa AA-A h36", 9, "Blaa Hypa VK-T b31-0", new double[] { -8442.53125, -1162.9375, 12419.1875 }) },
            { new GalacticNeblulae("CGN I-93", "Blaa Hypa AA-A h53", 9, "Blaa Hypa JB-H c25-13", new double[] { -8742.71875, -738.5, 12823.5 }) },
            { new GalacticNeblulae("CGN I-94", "Blaa Hypa AA-A h59", 9, "Blaa Hypa EO-H a61-0", new double[] { -7862.625, -354.125, 12387.15625 }) },
            { new GalacticNeblulae("CGN I-95", "Bleae Aescs AA-A h25", 9, "Bleae Aescs ER-T d4-41", new double[] { -3058.4375, 1091.125, 12167.71875 }) },
            { new GalacticNeblulae("CGN I-96", "Bleae Aewsy AA-A h22", 9, "Bleae Aewsy QB-U c4-4", new double[] { -297.46875, 582.4375, 11939.28125 }) },
            { new GalacticNeblulae("CGN I-97", "Blo Aescs AA-A h11", 9, "Blo Aescs QL-C b1-2", new double[] { -7129, 417.34375, 11755.0625 }) },
            { new GalacticNeblulae("CGN I-98", "Blo Aescs AA-A h59", 9, "Blo Aescs VA-L d9-108", new double[] { -6986.09375, 1174.1875, 12605.65625 }) },
            { new GalacticNeblulae("CGN I-99", "Blua Eaec AA-A h74", 9, "Blua Eaec VY-K b27-32", new double[] { -6991.8125, -226.5, 14890.15625 }) },
            { new GalacticNeblulae("CGN I-100", "Boewnst AA-A h87", 9, "Boewnst VK-L b41-138", new double[] { -6193.40625, -139.3125, 16463.5625 }) },
            { new GalacticNeblulae("CGN I-101", "Bya Ail AA-A h65", 9, "Bya Ail NG-X d1-59", new double[] { -11777.625, 322.4375, 15750.65625 }) },
            { new GalacticNeblulae("CGN I-102", "Clookau AA-A h41", 9, "Clookau NZ-X b18-0", new double[] { -9603.21875, -541.4375, 12138.6875 }) },
            { new GalacticNeblulae("CGN I-103", "Crookaae AA-A h129", 9, "Crookaae VJ-O d7-91", new double[] { -3933.0625, 1033, 12401.5625 }) },
            { new GalacticNeblulae("CGN I-104", "Dryaea Flee AA-A h10", 9, "Dryaea Flee IO-V b18-13", new double[] { -11730.21875, 482, 18539.6875 }) },
            { new GalacticNeblulae("CGN I-105", "Dryaea Flee AA-A h89", 9, "Dryaea Flee ME-G c24-563", new double[] { -12346.5, 0, 19194.9375 }) },
            { new GalacticNeblulae("CGN I-106", "Dryooe Prou AA-A h131", 9, "Dryooe Prou ZS-S b4-36", new double[] { -9883.09375, -1138.8125, 20796.4375 }) },
            { new GalacticNeblulae("CGN I-107", "Dryooe Prou AA-A h55", 9, "Dryooe Prou PV-D b3-144", new double[] { -9876.09375, -329.0625, 20764.78125 }) },
            { new GalacticNeblulae("CGN I-108", "Eephaills AA-A h62", 9, "Eephaills SG-C c1-177", new double[] { -10369.21875, -449.4375, 16915.84375 }) },
            { new GalacticNeblulae("CGN I-109", "Eoch Flya AA-A h119", 9, "Eoch Flya CO-N c8-0", new double[] { -6383.46875, -1860.25, 17218.15625 }) },
            { new GalacticNeblulae("CGN I-110", "Eol Prou AA-A h89", 9, "Eol Prou PX-T d3-1860", new double[] { -9544.875, -902.9375, 19795.625 }) },
            { new GalacticNeblulae("CGN I-111", "Eulail AA-A h2", 9, "Eulail QX-T d3-103", new double[] { -8164.75, 1706.28125, 17234.6875 }) },
            { new GalacticNeblulae("CGN I-112", "Flyiedgai AA-A h50", 9, "Flyiedgai YE-K b52-0", new double[] { -6303.875, -779.28125, 9019.6875 }) },
            { new GalacticNeblulae("CGN I-113", "Grea Hypooe AA-A h44", 9, "Grea Hypooe XJ-I b26-1", new double[] { -11883.3125, -694.875, 13583.875 }) },
            { new GalacticNeblulae("CGN I-114", "Dr. Kay's Soul", 9, "Greae Hypa DF-R d4-23", new double[] { -8241.25, -1236.28125, 13439.1875 }) },
            { new GalacticNeblulae("CGN I-115", "Gru Hypue AA-A h69", 9, "Gru Hypue KS-T d3-27", new double[] { -5001.96875, -944.09375, 13399.5 }) },
            { new GalacticNeblulae("CGN I-116", "Pha Flee AA-A h24", 9, "Pha Flee IM-D b32-2", new double[] { -11975.03125, 1179.53125, 23941.46875 }) },
            { new GalacticNeblulae("CGN I-117", "Leamae AA-A h55", 9, "Leamae TB-F c27-105", new double[] { -11858.9375, -405.09375, 20577.25 }) },
            { new GalacticNeblulae("CGN I-118", "Nuekau AA-A h83", 9, "Nuekau BK-Z d44", new double[] { -1830.09375, -751.375, 13105.125 }) },
            { new GalacticNeblulae("CGN I-119", "Oephaif AA-A h100", 9, "Oephaif UG-U c16-445", new double[] { -11582.28125, -13.75, 17602.53125 }) },
            { new GalacticNeblulae("CGN I-120", "Oephaif AA-A h23", 9, "Oephaif HU-E b42-10", new double[] { -10638.625, 626.40625, 17773.3125 }) },
            { new GalacticNeblulae("CGN I-121", "Oephaif AA-A h5", 9, "Oephaif GE-E b1-10", new double[] { -10807.4375, 596.8125, 16889.3125 }) },
            { new GalacticNeblulae("CGN I-122", "Ooscs Aob AA-A h3", 9, "Ooscs Aob SZ-W d2-510", new double[] { -9909.1875, 995.8125, 17165.03125 }) },
            { new GalacticNeblulae("CGN I-123", "Plio Aim AA-A h1", 9, "Plio Aim ZS-W b4-0", new double[] { -8281.9375, 568.40625, 9280.8125 }) },
            { new GalacticNeblulae("CGN I-124", "Preae Ain AA-A h29", 9, "Preae Ain JB-A b6-0", new double[] { -4801.15625, 1099.625, 10580 }) },
            { new GalacticNeblulae("CGN I-125", "Preae Ain AA-A h43", 9, "Preae Ain VU-V c3-21", new double[] { -4810.125, 583.09375, 10620.125 }) },
            { new GalacticNeblulae("CGN I-126", "Prua Phoe AA-A h69", 9, "Prua Phoe AY-T a68-0", new double[] { -5863.4375, -322.34375, 11187.875 }) },
            { new GalacticNeblulae("CGN I-127", "Screaka AA-A h72", 9, "Screaka RE-M b35-69", new double[] { -13479.59375, 97.96875, 21455.59375 }) },
            { new GalacticNeblulae("CGN I-128", "Skaudai AA-A h71", 9, "Skaudai SJ-B b58-4", new double[] { -5493.09375, -589.28125, 10424.4375 }) },
            { new GalacticNeblulae("CGN I-129", "Blaa Hypai AA-A h55", 10, "Blaa Hypai UO-L b10-0", new double[] { 1775.90625, -783.9375, 11957.8125 }) },
            { new GalacticNeblulae("CGN I-130", "Blaa Hypai AA-A h68", 10, "Blaa Hypai AI-I b26-1", new double[] { 1220.40625, -694.625, 12312.8125 }) },
            { new GalacticNeblulae("CGN I-131", "Clookuia AA-A h35", 10, "Clookuia MI-K d8-10", new double[] { 2890.0625, -1215.96875, 12461.28125 }) },
            { new GalacticNeblulae("CGN I-132", "Puelaa AA-A h4", 10, "Puelaa OD-V b8-0", new double[] { 5375.0625, 1137.3125, 13200.5625 }) },
            { new GalacticNeblulae("CGN I-133", "Eock Bluae AA-A h36", 11, "Eock Bluae JQ-Y d5", new double[] { 16050.4375, -1106.40625, 27206.9375 }) },
            { new GalacticNeblulae("CGN I-134", "Flyai Flyuae AA-A h20", 12, "Flyai Flyuae XJ-A c2", new double[] { 17926.28125, -1201, 37360.59375 }) },
            { new GalacticNeblulae("CGN I-135", "Myoangooe AA-A h2", 13, "Myoangooe SU-G b42-0", new double[] { -11537.9375, 851.59375, 42082.53125 }) },
            { new GalacticNeblulae("CGN I-136", "Aiphaisty AA-A h3", 15, "Aiphaisty BC-C c13-50", new double[] { -17774.34375, 161.90625, 34085.46875 }) },
            { new GalacticNeblulae("CGN I-137", "Eolls Graae AA-A h31", 15, "Eolls Graae BG-W d2-44", new double[] { -18859.09375, -613.1875, 29953.4375 }) },
            { new GalacticNeblulae("CGN I-138", "Glaiseae AA-A h29", 15, "Glaiseae SO-G c27-5", new double[] { -16115.65625, -86.0625, 32121.40625 }) },
            { new GalacticNeblulae("CGN I-139", "Hypao Brai AA-A h6", 15, "Hypao Brai YR-I d10-6", new double[] { -15214.28125, 880.4375, 35661.75 }) },
            { new GalacticNeblulae("CGN I-140", "Vegnaa AA-A h5", 15, "Vegnaa ZB-J b39-1", new double[] { -15112.75, 522.875, 36907.8125 }) },
            { new GalacticNeblulae("CGN I-141", "Vegneia AA-A h4", 15, "Vegneia ZO-I d9-40", new double[] { -11779.15625, 61.40625, 36889.4375 }) },
            { new GalacticNeblulae("CGN I-142", "Agnaix AA-A h41", 16, "Agnaix QD-A d1-46", new double[] { -14929.65625, -451.40625, 22111.9375 }) },
            { new GalacticNeblulae("CGN I-143", "Boeppy AA-A h62", 16, "Boeppy NU-Q b8-2", new double[] { -14992.96875, -600.40625, 15772.6875 }) },
            { new GalacticNeblulae("CGN I-144", "Dryo Aob AA-A h29", 16, "Dryo Aob TU-W b45-1", new double[] { -15518.46875, 515.9375, 19118.71875 }) },
            { new GalacticNeblulae("CGN I-145", "Floaln AA-A h43", 16, "Floaln GX-U b57-0", new double[] { -12461.4375, -1281.59375, 15540.71875 }) },
            { new GalacticNeblulae("CGN I-146", "Floarph AA-A h49", 16, "Boeff XY-B b0", new double[] { -13279.375, -1104.96875, 15575.96875 }) },
            { new GalacticNeblulae("CGN I-147", "Iowhaih AA-A h42", 16, "Iowhaih GY-B c16-0", new double[] { -13067.1875, 1237.625, 16284.125 }) },
            { new GalacticNeblulae("CGN I-148", "Leameia AA-A h52", 16, "Leameia CQ-A b6-18", new double[] { -13779.34375, -123.40625, 19538.21875 }) },
            { new GalacticNeblulae("CGN I-149", "Phooe Aob AA-A h32", 16, "Phooe Aob ST-Y d1-9", new double[] { -14719.21875, 1007.59375, 23419.78125 }) },
            { new GalacticNeblulae("CGN I-150", "Ploadaea AA-A h1", 16, "Ploadaea KA-S c6-0", new double[] { -19175.21875, 908.375, 14582.5 }) },
            { new GalacticNeblulae("CGN I-151", "Zejoo AA-A h38", 16, "Zejoo WM-Q b52-2", new double[] { -15778.1875, -111.96875, 11586.21875 }) },
            { new GalacticNeblulae("CGN I-152", "Bya Phlai AA-A h2", 17, "Bya Phlai UO-P b38-0", new double[] { -12621.40625, 1017.53125, 13836.375 }) },
            { new GalacticNeblulae("CGN I-153", "Dehoae AA-A h56", 17, "Dehoae FL-W d2-0", new double[] { -15796.75, -533.8125, 9459.46875 }) },
            { new GalacticNeblulae("CGN I-154", "Eodgosly AA-A h38", 17, "Eodgosly TL-S c5-3", new double[] { -10244.15625, 653.625, 10703.9375 }) },
            { new GalacticNeblulae("CGN I-155", "Flyai Eaescs AA-A h45", 17, "Flyai Eaescs BG-N d7-8", new double[] { -12143.09375, -709.65625, 9864.78125 }) },
            { new GalacticNeblulae("CGN I-156", "Mycawsy AA-A h0", 17, "Mycawsy NE-D c1-0", new double[] { -12503.59375, -1544.25, 11780.71875 }) },
            { new GalacticNeblulae("CGN I-157", "Pra Eaewsy AA-A h56", 17, "Pra Eaewsy TL-C d13-5", new double[] { -15287.8125, -821.6875, 11609.5625 }) },
            { new GalacticNeblulae("CGN I-158", "Prai Hypoo AA-A h60", 17, "Prai Hypoo SA-I b0", new double[] { -9294.875, -458.40625, 7905.71875 }) },
            { new GalacticNeblulae("CGN I-159", "Thraikai AA-A h3", 17, "Thraikai XL-A c15-0", new double[] { -14901.125, 535.71875, 7256.8125 }) },
            { new GalacticNeblulae("CGN I-160", "Traikaae AA-A h2", 18, "Traikaae KT-P d6-4", new double[] { -959.6875, 898.78125, 4624.03125 }) },
            { new GalacticNeblulae("CGN I-161", "Traikeou AA-A h2", 18, "Traikeou BP-F b28-0", new double[] { -7189.625, 659.375, 4658.6875 }) },
            { new GalacticNeblulae("CGN I-162", "Pueliae AA-A h0", 19, "Pueliae TH-P c20-0", new double[] { 18059.21875, 416.1875, 13895.03125 }) },
            { new GalacticNeblulae("CGN I-163", "Eock Prau AA-A h31", 20, "Eock Prau WS-K c8-0", new double[] { 26255.59375, -1179.8125, 19779.8125 }) },
            { new GalacticNeblulae("CGN I-164", "Thueche AA-A h16", 25, "Thueche YB-B b5-0", new double[] { -13543.59375, -264.78125, 52798.03125 }) },
            { new GalacticNeblulae("CGN I-165", "Crooki AA-A h1", 32, "Crooki EV-R b35-1", new double[] { -23057.65625, 705.5, 12510.0625 }) },
            { new GalacticNeblulae("CGN I-166", "Floagh AA-A h56", 32, "Floagh JO-Z d13-0", new double[] { -20455.5, -1300.1875, 15506.5 }) },

            { new GalacticNeblulae("CGN III-164", "Running Man Nebula", 32, "V1745 Orionis", new double[] { 587.84375, -425.40625, -1077.5625 }) },

        };

        string designation;
        string name;
        int regionIdx;
        string systemName;
        double[] starPos;

        private GalacticNeblulae(string designation, string name, int regionIdx, string systemName, double[] starPos)
        {
            this.designation = designation;
            this.name = name;
            this.regionIdx = regionIdx;
            this.systemName = systemName;
            this.starPos = starPos;
        }

        public static async Task lookupStarPos()
        {
            var txt = new StringBuilder();
            foreach (var neb in nebulae)
            {
                if (neb.starPos[0] == 0)
                {
                    Game.log($"{neb.designation} {neb.name} ...");
                    var rslt = await Game.spansh.getSystem(neb.systemName);
                    var row = rslt.min_max.First();
                    var pos = new double[] { row.x, row.y, row.z };
                    txt.AppendLine($"            {{ new GalacticNeblulae(\"{neb.designation}\", \"{neb.name}\", {neb.regionIdx}, \"{neb.systemName}\", new double[] {{ {pos[0]}, {pos[1]}, {pos[2]} }}) }},");
                }
            }

            Game.log($"New code:\r\n\r\n{txt}");
        }
    }
}
