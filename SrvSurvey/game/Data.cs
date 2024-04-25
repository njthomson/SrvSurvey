using Newtonsoft.Json;

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
                if (string.IsNullOrEmpty(json)) Game.log($"Why is this data file empty?\r\n{filepath}");

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
            var folder = Path.GetDirectoryName(this.filepath)!;
            Directory.CreateDirectory(folder);

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
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
                var currentGalacticRegion = Game.activeGame?.cmdr.galacticRegion ?? "";
                var mappedGalacticRegion = GalacticRegions.map.GetValueOrDefault(currentGalacticRegion);
                return mappedGalacticRegion ?? "???";
            }
        }

        public static Dictionary<string, string> map = new Dictionary<string, string>()
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
    }
}
