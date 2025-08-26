using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Diagnostics;

namespace SrvSurvey.widgets
{
    public class Theme : Dictionary<string, Color>
    {
        static readonly string themePath = Path.Combine(Program.dataFolder, "theme.json");
        static readonly string themeDefaultPath = Path.Combine(Application.StartupPath, "theme.json");

        public static Theme loadTheme(bool defaultTheme = false)
        {
            // copy default theme if there's no custom one yet
            if (!File.Exists(themePath))
            {
                // read and write (a basic file copy has issues with BitLocker on Win11?)
                var txt = File.ReadAllText(themeDefaultPath);
                File.WriteAllText(themePath, txt);
            }

            Exception? caughtError = null;
            // read and parse file contents into tmp object
            var filepath = defaultTheme ? themeDefaultPath : themePath;
            var json = File.ReadAllText(filepath);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var theme = new Theme();

                    var obj = JsonConvert.DeserializeObject<JObject>(json)!;
                    parseThemeObj(theme, "", obj);

                    Game.log($"Loaded theme (default: {defaultTheme})");
                    return theme;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to read theme: {ex}");
                    Game.log(json);
                    caughtError = ex;
                }
            }

            // we reach here if the file is corrupt

            // backup the corrupted file, then delete it and try again 
            var backupPath = Path.Combine(Program.dataFolder, "theme.bad.json");
            Game.log($"Resetting theme. Corrupt theme copied to: {backupPath}");

            File.Copy(themePath, backupPath, true);
            File.Delete(themePath);
            var newTheme = loadTheme();

            if (caughtError != null)
                Program.defer(() => FormErrorSubmit.Show(caughtError));
            return newTheme;
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

        public void update(string name, Color colour)
        {
            this[name] = colour;
        }

        public void saveUpdates()
        {
            var root = new RawTheme();
            foreach (var (key, colour) in this)
            {
                var node = root;

                // split on dots and create child nodes as needed
                var parts = key.Split('.').ToList();
                while (parts.Count > 1)
                {
                    var part = parts.First();
                    if (!node.ContainsKey(part))
                        node[part] = new RawTheme();

                    node = (RawTheme)node[part];
                    parts.RemoveAt(0);
                }

                // store the colour as an array of A, R, G, B
                node[parts.First()] = new int[4] { colour.A, colour.R, colour.G, colour.B, };
            }
            ;

            var json = JsonConvert.SerializeObject(root, Formatting.Indented);
            File.WriteAllText(themePath, json);
            Game.log($"Theme written to: {themePath}");
        }

        [JsonConverter(typeof(RawTheme.JsonConverter))]
        class RawTheme : Dictionary<string, object>
        {
            class JsonConverter : Newtonsoft.Json.JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return false;
                }

                public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
                {
                    throw new NotImplementedException("We should never hit this");
                }

                public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                {
                    var rawTheme = value as RawTheme;
                    if (rawTheme == null) throw new Exception($"Unexpected type: {value?.GetType().Name}");

                    writeRaw((JsonTextWriter)writer, rawTheme);
                }

                private void writeRaw(JsonTextWriter writer, RawTheme rawTheme)
                {
                    writer.WriteStartObject();
                    foreach (var (key, rawValue) in rawTheme)
                    {
                        // preserve top level comment if this is the first colour
                        if (key == "orange")
                        {
                            writer.WriteWhitespace("\r\n");

                            var i = writer.Indentation;
                            while (i-- > 0) writer.WriteWhitespace(writer.IndentChar.ToString());

                            writer.WriteComment(" One day this will contain all the colours used by SrvSurvey ");
                            writer.WriteWhitespace("\r\n");
                        }

                        // write out either colours serialized as an array of Int's, or child nodes
                        writer.WritePropertyName(key);

                        var intArray = rawValue as int[];
                        var childObj = rawValue as RawTheme;
                        if (intArray != null)
                        {
                            var subJson = "[ " + string.Join(", ", intArray) + " ]";
                            writer.WriteRawValue(subJson);
                        }
                        else if (childObj != null)
                        {
                            writeRaw(writer, childObj);
                        }
                        else
                        {
                            throw new Exception("Unexpected!");
                        }

                    }
                    writer.WriteEndObject();
                }
            }
        }
    }
}