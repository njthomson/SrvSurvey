using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;

namespace SrvSurvey.widgets
{
    public class Theme : Dictionary<string, Theme.RawValue>
    {
        static readonly string themePath = Path.Combine(Program.dataFolder, "theme.json");
        static readonly string themeDefaultPath = Path.Combine(Application.StartupPath, "theme.json");

        public static Theme loadTheme(bool useDefaultTheme = false)
        {
            // copy default theme if there's no custom one yet
            if (!useDefaultTheme && !File.Exists(themePath))
            {
                // read and write (a basic file copy has issues with BitLocker on Win11?)
                var txt = File.ReadAllText(themeDefaultPath);
                File.WriteAllText(themePath, txt);
            }

            Exception? caughtError = null;
            // read and parse file contents into tmp object
            var filepath = useDefaultTheme ? themeDefaultPath : themePath;
            var json = File.ReadAllText(filepath);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var theme = new Theme();

                    Theme? defaultTheme = null;
                    if (!useDefaultTheme)
                        defaultTheme = loadTheme(true);

                    var obj = JsonConvert.DeserializeObject<JObject>(json)!;
                    parseThemeObj(theme, "", obj, defaultTheme);

                    // fix bug where cyan and cyanDark were identical (and save)
                    if (theme["cyan"] == theme["cyanDark"])
                    {
                        theme.update("cyanDark", GameColors.Defaults.DarkCyan);
                        theme.saveUpdates();
                    }

                    // add any default theme colours missing in the custom theme (but do not save)
                    if (defaultTheme != null)
                        foreach (var name in defaultTheme.Keys)
                            if (!theme.ContainsKey(name))
                                theme.update(name, defaultTheme[name], JValue.CreateNull());

                    Game.log($"Loaded theme (default: {useDefaultTheme})");
                    //Game.log(JsonConvert.SerializeObject(theme, Formatting.Indented));
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

            if (caughtError != null)
            {
                // show a message box not the Oops dialog
                var rslt = MessageBox.Show(
                    $"There was a problem reading the custom theme file.\nIt has been renamed as:\n    %appdata%\\SrvSurvey\\SrvSurvey\\1.1.0.0\\theme.bad.json\n\nWould you like to open this file now?\n\nError: {caughtError.Message}",
                    $"SrvSurvey", MessageBoxButtons.YesNo, MessageBoxIcon.Error
                );

                if (rslt == DialogResult.Yes)
                    Util.openLink(backupPath);
            }

            // proceed with the default theme
            var newTheme = loadTheme();

            return newTheme;
        }

        private static void parseThemeObj(Theme theme, string prefix, JObject parentObj, Theme? defaultTheme)
        {
            var name = "";
            try
            {
                foreach (var pair in parentObj)
                {
                    name = pair.Key;
                    if (pair.Value == null) throw new Exception($"Colour value cannot be null");

                    string key = pair.Key;
                    string fullKey = $"{prefix}{key}";
                    JToken obj = pair.Value;

                    if (obj.Type == JTokenType.Array)
                    {
                        // this is an ARGB array of int's
                        var argb = obj.Values<int>().ToArray();
                        if (argb.Length == 4)
                            theme.Add(fullKey, new(Color.FromArgb(argb[0], argb[1], argb[2], argb[3]), obj));
                        else if (argb.Length == 3)
                            theme.Add(fullKey, new(Color.FromArgb(255, argb[0], argb[1], argb[2]), obj));
                        else
                            throw new Exception("Expected array of [ R, G, B ] or [ A, R, G, B ]");
                    }
                    else if (obj.Type == JTokenType.String)
                    {
                        var value = obj.Value<string>()!;
                        if (value.StartsWith("#"))
                        {
                            // parse as HTML colour, eg: "#FF0000" or "#FF0000aa"
                            theme.Add(fullKey, new(ColorTranslator.FromHtml(value), obj));
                        }
                        else
                        {
                            // is this is the name of a prior color
                            if (!theme.ContainsKey(value!)) throw new Exception($"Prior colour not found: {value}");
                            theme.Add(fullKey, new(theme[value], obj));
                        }
                    }
                    else if (obj?.Type == JTokenType.Null)
                    {
                        if (defaultTheme == null) throw new Exception($"Default colours not available");

                        // use the default theme value
                        if (!defaultTheme.ContainsKey(fullKey)) throw new Exception($"Default colour not found: {name}");
                        theme.Add(fullKey, new(defaultTheme[fullKey], obj));
                    }
                    else if (obj?.Type == JTokenType.Object)
                    {
                        // recurse down a level
                        parseThemeObj(theme, $"{key}.", (JObject)obj, defaultTheme);
                    }
                    else
                    {
                        throw new Exception($"Unexpected token type: {obj?.Type}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SrvException)
                    throw;
                else
                    throw new SrvException($"Failed to parse theme colour: '{prefix}{name}'\n\t{ex.Message}", true);
            }
        }

        public new Color this[string key]
        {
            get
            {
                var pair = base[key];
                return pair.color;
            }
        }

        public void update(string name, Color colour, JToken? token = null)
        {
            base[name] = new(colour, token ?? JToken.FromObject(new int[] { colour.A, colour.R, colour.G, colour.B }));
        }

        public void saveUpdates()
        {
            var root = new RawTheme();
            foreach (var (key, raw) in this)
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
                node[parts.First()] = raw.token;
            }

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

                            writer.WriteComment(headerComment);
                            writer.WriteWhitespace("\r\n");
                        }

                        // write out either colours serialized as an array of Int's, or child nodes
                        writer.WritePropertyName(key);

                        var token = rawValue as JToken;
                        var childObj = rawValue as RawTheme;
                        if (token?.Type == JTokenType.Array && token.FirstOrDefault()?.Type == JTokenType.Integer)
                        {
                            var subJson = "[ " + string.Join(", ", token.Values<int>()) + " ]";
                            writer.WriteRawValue(subJson);
                        }
                        else if (token?.Type == JTokenType.String || token?.Type == JTokenType.Null)
                        {
                            token.WriteTo(writer);
                        }
                        else if (childObj != null)
                        {
                            writeRaw(writer, childObj);
                        }
                        else
                        {
                            throw new Exception($"Unexpected theme entry: '{key}' ({token?.Type}): {token}");
                        }

                    }
                    writer.WriteEndObject();
                }
            }

            private static string headerComment = @"
   * Colour values may be one of the following formats:
   *
   * - Array of four integers representing ARGB values, each in the range 0-255
   * - Array of three integers representing RGB values, each in the range 0-255
   * - An HTML colour string, eg: ""#RRGGBB"" or ""#RRGGBBAA""
   * - A PRIOR named colour. Eg: 'colonise.surplus' can use 'green' but 'yellow' cannot use 'bio.gold'
   * - A null value means use the colour from the default theme
   *
   * Missing entries will use default theme colours, as if they were 'null'
   ";
        }

        public class RawValue
        {
            public Color color;
            public JToken token;

            public RawValue(Color color, JToken raw)
            {
                this.color = color;
                this.token = raw;
            }
        }
    }
}