using System;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Utilities.Extensions {
    public static class MemberInfoExtensions {
        public static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute {
            return memberInfo.GetCustomAttributes(inherit)
                             .OfType<T>()
                             .SingleOrDefault();
        }
    }
}