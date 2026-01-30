using Lua;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SrvSurvey.quests;
using System.Collections;
using System.Reflection;

namespace SrvSurvey
{
    internal class CustomContractResolver : DefaultContractResolver
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

                // ignore nulls for all nullable value types
                if (property.PropertyType.IsValueType == true && property.PropertyType?.IsGenericType == true && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    property.NullValueHandling = NullValueHandling.Ignore;
            }

            return property;
        }

        public override JsonContract ResolveContract(Type type)
        {
            var contract = base.ResolveContract(type);

            if (type == typeof(LuaValue))
                contract.Converter = new LuaValueJsonConverter();

            return contract;
        }
    }

}
