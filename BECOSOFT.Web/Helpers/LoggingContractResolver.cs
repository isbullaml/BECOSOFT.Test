using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Web;

namespace BECOSOFT.Web.Helpers {
    public class LoggingContractResolver : DefaultContractResolver {
        public static LoggingContractResolver Instance { get; } = new LoggingContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);

            if (typeof(HttpPostedFileBase).IsAssignableFrom(member.ReflectedType)) {
                property.Ignored = true;
            }

            return property;
        }
    }
}
