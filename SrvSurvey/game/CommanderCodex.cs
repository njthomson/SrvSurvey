using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    internal class CommanderCodex : Data
    {
        public static CommanderCodex Load(string fid, string commanderName)
        {
            var filepath = Path.Combine(Program.dataFolder, $"{fid}-codex.json");

            return Data.Load<CommanderCodex>(filepath)
                ?? new CommanderCodex()
                {
                    filepath = filepath,
                    commander = commanderName,
                    fid = fid,
                };
        }

        public string fid;
        public string commander;

        /// <summary>
        /// The timestamp, system address and body Id each entryId was first encountered
        /// </summary>
        public Dictionary<long, CodexFirst>? codexFirsts = new Dictionary<long, CodexFirst>();

        public void trackCodex(long entryId, DateTime timestamp, long systemAddress, int? bodyId)
        {
            if (this.codexFirsts == null) this.codexFirsts = new Dictionary<long, CodexFirst>();

            // exit early if this is not new
            if (this.codexFirsts.ContainsKey(entryId) && timestamp > this.codexFirsts[entryId].time) return;

            // add/update list and save
            this.codexFirsts[entryId] = new CodexFirst(timestamp, systemAddress, bodyId ?? -1);
            Game.log($"New first CodexEntry! {entryId} at systemAddress: {systemAddress}, bodyId: {bodyId}");

            // sort by entryId before saving
            this.codexFirsts = this.codexFirsts.OrderBy(_ => _.Key).ToDictionary(_ => _.Key, _ => _.Value);
            this.Save();
        }

        public bool isPersonalFirstDiscovery(long entryId, long systemAddress, int bodyId)
        {
            var match = this.codexFirsts?.GetValueOrDefault(entryId);

            // return true if entryId, systemAddress AND bodyId all match
            return match?.address == systemAddress && match?.bodyId == bodyId;
        }

    }


    [JsonConverter(typeof(CodexFirst.JsonConverter))]
    internal class CodexFirst
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime time;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long address;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int bodyId;

        public CodexFirst(DateTime timestamp, long address, int bodyId)
        {
            this.time = timestamp;
            this.address = address;
            this.bodyId = bodyId;
        }

        public override string ToString()
        {
            return time.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + $"_{address}_{bodyId}";
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                //var obj = serializer.Deserialize<JToken>(reader);
                //if (obj == null || !obj.HasValues) return null;



                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                // "{time}_{address}_{bodyId}"
                // eg: "xxx"
                var parts = txt.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var time = DateTime.Parse(parts[0], CultureInfo.InvariantCulture);
                var addr = long.Parse(parts[1], CultureInfo.InvariantCulture);
                var body = int.Parse(parts[2], CultureInfo.InvariantCulture);

                var data = new CodexFirst(time, addr, body);

                return data;

            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as CodexFirst;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");


                //var obj = new JObject();

                //obj["t"] = JToken.FromObject(data.time.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
                //obj["a"] = JToken.FromObject(data.address);
                //obj["b"] = JToken.FromObject(data.bodyId);

                //var json = JsonConvert.SerializeObject(obj); // no indentation
                //json = json.Replace("{", "{ ")
                //    .Replace("}", " }")
                //    .Replace(",", ", ")
                //    .Replace("\":", "\": ");
                //writer.WriteRawValue(json);

                var txt = data.ToString();
                writer.WriteValue(txt);
            }
        }

    }

}
