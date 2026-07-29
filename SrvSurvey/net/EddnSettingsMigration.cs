using Newtonsoft.Json.Linq;

namespace SrvSurvey.net
{
    internal static class EddnSettingsMigration
    {
        internal static bool useTestSchemas(JObject settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var current = settings["eddnUseTestSchemas"];
            if (current?.Type == JTokenType.Boolean)
                return current.Value<bool>();

            // A present but malformed replacement value must fail back to Live
            // schemas rather than reviving an obsolete endpoint selection.
            return current is null
                && isLegacyTestEnvironment(
                    settings.Value<string>("eddnEnvironment"));
        }

        internal static bool isLegacyTestEnvironment(string? value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "dev" => true,
                "beta" => true,
                _ => false,
            };
        }
    }
}
