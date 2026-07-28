using Newtonsoft.Json.Linq;

namespace SrvSurvey.net
{
    internal static class EddnMessageSanitizer
    {
        internal static JObject codexEntry(
            JObject raw,
            double[] starPosition,
            bool? odyssey,
            bool? horizons,
            string? statusBodyName,
            string? trackedBodyName,
            int? trackedBodyId)
        {
            ArgumentNullException.ThrowIfNull(raw);
            ArgumentNullException.ThrowIfNull(starPosition);

            var message = new JObject(raw);
            trim(
                message,
                "*_Localised",
                "BodyID",
                "BodyName",
                "IsNewEntry",
                "NewTraitsDiscovered");
            message["StarPos"] = new JArray(starPosition);
            if (odyssey.HasValue) message["odyssey"] = odyssey.Value;
            if (horizons.HasValue) message["horizons"] = horizons.Value;

            if (statusBodyName != null && statusBodyName == trackedBodyName)
            {
                message["BodyName"] = statusBodyName;
                if (trackedBodyId.HasValue
                    && raw.Value<int?>("BodyID") == trackedBodyId)
                    message["BodyID"] = trackedBodyId;
            }

            return message;
        }

        private static void trim(JObject obj, params string[] names)
        {
            foreach (var name in names)
            {
                if (name.StartsWith('*'))
                {
                    foreach (var property in obj.Properties().ToList())
                    {
                        if (property.Name.EndsWith(name[1..], StringComparison.Ordinal))
                            obj.Remove(property.Name);
                    }
                }
                else
                {
                    obj.Remove(name);
                }
            }

            foreach (var value in obj.Values())
            {
                if (value.Type == JTokenType.Object)
                {
                    trim((JObject)value, names);
                }
                else if (value.Type == JTokenType.Array)
                {
                    foreach (var item in (JArray)value)
                    {
                        if (item.Type == JTokenType.Object)
                            trim((JObject)item, names);
                    }
                }
            }
        }
    }
}
