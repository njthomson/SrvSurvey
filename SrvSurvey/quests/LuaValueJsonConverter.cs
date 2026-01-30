using Lua;
using Newtonsoft.Json;

namespace SrvSurvey.quests;

class LuaValueJsonConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanConvert(Type objectType) { return false; }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var raw = serializer.Deserialize(reader);
        return raw.toLua();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (!(value is LuaValue))
            throw new Exception($"Unexpected value: {value?.GetType().Name}");

        var raw = (LuaValue)value;

        switch (raw.Type)
        {
            case LuaValueType.Nil:
                writer.WriteNull();
                return;
            case LuaValueType.Boolean:
                writer.WriteValue(raw.Read<bool>());
                return;
            case LuaValueType.Number:
                writer.WriteValue(raw.Read<double>());
                return;
            case LuaValueType.String:
                writer.WriteValue(raw.Read<string>());
                return;
            case LuaValueType.Table:
                // handled below
                break;

            default:
                throw new Exception($"Not supported: LuaValueType:{raw.TypeToString}");
        }

        // for tables...
        var table = raw.Read<LuaTable>();
        if (table.HashMapCount > 0 && table.ArrayLength > 0)
            throw new Exception($"Not supported: tables must be either an array or a map, not both");

        if (table.HashMapCount > 0)
        {
            // ... map style
            writer.WriteStartObject();
            foreach (var (key, childValue) in table)
            {
                if (key.Type != LuaValueType.String)
                    throw new Exception($"Not supported: Table keys must be strings. Found ({key.TypeToString()}) '{key.ToString()}' after path: {writer.Path}");

                writer.WritePropertyName(key.Read<string>());
                serializer.Serialize(writer, childValue);
            }
            writer.WriteEndObject();
        }
        else
        {
            // ... array style
            writer.WriteStartArray();

            foreach (var (key, childValue) in table)
                serializer.Serialize(writer, childValue);

            writer.WriteEndArray();
        }
    }
}
