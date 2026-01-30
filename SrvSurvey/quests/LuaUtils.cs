using Lua;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;

namespace SrvSurvey.quests;

internal static class LuaUtils
{
    public static LuaTable toTbl(Object? obj)
    {
        var tbl = new LuaTable();
        if (obj != null)
        {
            var raw = JObject.FromObject(obj);
            foreach (var (name, token) in raw)
            {
                if (name == null || token == null) continue;
                tbl[name] = token.toLua();
            }
        }
        return tbl;
    }

    public static LuaValue toLua(this object? raw)
    {
        if (raw == null)
            return LuaValue.Nil;
        if (raw is string)
            return new LuaValue((string)raw);
        if (raw is double)
            return new LuaValue((double)raw);
        if (raw is bool)
            return new LuaValue((bool)raw);

        if (raw is DateTimeOffset)
        {
            var time = (DateTimeOffset)raw;
            LuaTable table = new()
            {
                ["year"] = time.Year,
                ["month"] = time.Month,
                ["day"] = time.Day,
                ["hour"] = time.Hour,
                ["min"] = time.Minute,
                ["sec"] = time.Second,
                ["wday"] = (int)time.DayOfWeek + 1,
                ["yday"] = time.DayOfYear,
                ["isdst"] = new DateTime(time.ToLocalTime().Ticks).IsDaylightSavingTime(),
            };
            return table;
        }
        if (raw is DateTime)
        {
            var time = (DateTime)raw;
            LuaTable table = new()
            {
                ["year"] = time.Year,
                ["month"] = time.Month,
                ["day"] = time.Day,
                ["hour"] = time.Hour,
                ["min"] = time.Minute,
                ["sec"] = time.Second,
                ["wday"] = (int)time.DayOfWeek + 1,
                ["yday"] = time.DayOfYear,
                ["isdst"] = time.IsDaylightSavingTime(),
            };
            return table;
        }

        if (raw is JValue)
        {
            var val = (JValue)raw;
            switch (val.Type)
            {
                case JTokenType.Null: return LuaValue.Nil;
                case JTokenType.String: return new LuaValue(val.Value<string>()!);
                case JTokenType.Boolean: return new LuaValue(val.Value<bool>()!);
                case JTokenType.Integer: return new LuaValue(val.Value<long>()!);
                case JTokenType.Float: return new LuaValue(val.Value<double>()!);
                case JTokenType.Date: return toLua(val.Value);
                default: throw new Exception($"Unexpected value type: ({val.Type}) {val}");
            }
        }

        if (raw is JObject)
        {
            var obj = (JObject)raw;
            var tbl = new LuaTable(0, obj.Count);

            foreach (var child in obj)
                tbl[child.Key] = toLua(child.Value);

            return tbl;
        }

        if (raw is JArray)
        {
            var arr = (JArray)raw;
            var tbl = new LuaTable(arr.Count, 0);

            for (int n = 0; n < arr.Count; n++)
                tbl[n + 1] = toLua(arr[n]);

            return tbl;
        }

        throw new Exception($"Unexpected value: ({raw.GetType().Name}) {raw}");
    }
}
