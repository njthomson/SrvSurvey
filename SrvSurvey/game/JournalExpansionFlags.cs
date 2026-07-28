using Newtonsoft.Json.Linq;

namespace SrvSurvey.game
{
    internal readonly record struct JournalExpansionFlags(
        bool? horizons,
        bool? odyssey)
    {
        internal static JournalExpansionFlags fromLoadGame(JObject? raw)
        {
            return new JournalExpansionFlags(
                readBoolean(raw, "Horizons"),
                readBoolean(raw, "Odyssey"));
        }

        private static bool? readBoolean(JObject? raw, string name)
        {
            var value = raw?[name];
            return value?.Type == JTokenType.Boolean
                ? value.Value<bool>()
                : null;
        }
    }
}
