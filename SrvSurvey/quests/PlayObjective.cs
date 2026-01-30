using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SrvSurvey.quests;

/// <summary> The runtime/persisted state for an individual quest objective </summary>
[JsonConverter(typeof(PlayObjective.JsonConverter))]
public class PlayObjective
{
    public State state;
    public int current;
    public int total;

    public override string ToString()
    {
        return $"{state},{current},{total}";
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum State
    {
        hidden,
        visible,
        complete,
        failed,
    }

    class JsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType) { return false; }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var txt = serializer.Deserialize<string>(reader);
            if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

            // eg: "visible"
            // eg: "complete"
            // eg: "visible,1,10"
            var parts = txt.Split(',', StringSplitOptions.TrimEntries);
            return new PlayObjective()
            {
                state = Enum.Parse<PlayObjective.State>(parts[0]),
                current = parts.Length != 3 ? 0 : int.Parse(parts[1]),
                total = parts.Length != 3 ? 0 : int.Parse(parts[2]),
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var objective = value as PlayObjective;
            if (objective == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

            // only include numbers if they are both not zero
            var json = objective.current == 0 && objective.total == 0
                ? $"{objective.state}"
                : $"{objective.state},{objective.current},{objective.total}";

            writer.WriteValue(json);
        }
    }
}
