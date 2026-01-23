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

        if (property.PropertyType != null)
        {
            // For any IList/IDictionary/HashSet property, ignore null values (keeps the initialized empty list)
            if (typeof(IList).IsAssignableFrom(property.PropertyType) ||
                typeof(IDictionary).IsAssignableFrom(property.PropertyType) ||
                (property.PropertyType.IsGenericType && typeof(HashSet<>) == property.PropertyType.GetGenericTypeDefinition()))
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }
        }

        return property;
    }
}
