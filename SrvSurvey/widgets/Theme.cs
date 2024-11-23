using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;

namespace SrvSurvey.widgets
{
    public class Theme : Dictionary<string, Color>
    {
        static readonly string themePath = Path.Combine(Program.dataFolder, "theme.json");
        static readonly string themeDefaultPath = Path.Combine(Application.StartupPath, "theme.json");

        public static Theme loadTheme()
        {
            // copy default theme if there's no custom one yet
            if (!File.Exists(themePath))
                File.Copy(themeDefaultPath, themePath);

            // read and parse file contents into tmp object
            var json = File.ReadAllText(themePath);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var theme = new Theme();

                    var obj = JsonConvert.DeserializeObject<JObject>(json)!;
                    parseThemeObj(theme, "", obj);

                    Game.log($"Loaded theme");
                    return theme;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to read theme: {ex}");
                    Game.log(json);
                    MessageBox.Show("Failed to load theme. Please share the following and include the logs", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    FormErrorSubmit.Show(ex);
                }
            }

            // we reach here if the file is corrupt

            // backup the corrupted file, then delete it and try again 
            var backupPath = Path.Combine(Program.dataFolder, "theme.bad.json");
            Game.log($"Resetting theme. Corrupt theme copied to: {backupPath}");

            File.Copy(themePath, backupPath);
            File.Delete(themePath);
            return loadTheme();
        }

        private static void parseThemeObj(Theme theme, string prefix, JObject parentObj)
        {
            foreach (var pair in parentObj)
            {
                if (pair.Value == null) continue;

                string key = pair.Key;
                JToken obj = pair.Value;

                // Maybe support HTML/CSS style strings? eg: "FF112233"

                if (obj.Type == JTokenType.Array)
                {
                    // this is an ARGB array of int's
                    var argb = obj.Values<int>().ToArray();
                    theme.Add($"{prefix}{key}", Color.FromArgb(argb[0], argb[1], argb[2], argb[3]));
                }
                else if (obj?.Type == JTokenType.Object)
                {
                    // recurse down a level
                    parseThemeObj(theme, $"{key}.", (JObject)obj);
                }
            }
        }
    }
}
