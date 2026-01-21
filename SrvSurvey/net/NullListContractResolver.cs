using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Reflection;

namespace SrvSurvey.net;

/// <summary>
/// A custom contract resolver that ignores null values for list properties during deserialization.
/// When combined with field initializers (e.g., `List&lt;T&gt; items = []`), this ensures that
/// lists keep their initialized empty value instead of being set to null from JSON.
/// </summary>
internal class NullListContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        // For any IList property, ignore null values (keeps the initialized empty list)
        if (property.PropertyType != null && typeof(IList).IsAssignableFrom(property.PropertyType))
        {
            property.NullValueHandling = NullValueHandling.Ignore;
        }

        return property;
    }
}
