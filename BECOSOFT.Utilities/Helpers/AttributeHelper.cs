using BECOSOFT.Utilities.Extensions;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Helpers {
    public static class AttributeHelper {
        public static TValue GetAttributeProperty<TAttribute, T, TValue>(Expression<Func<T, object>> propExpression, Func<TAttribute, TValue> valueFunc) where TAttribute : Attribute {
            var body = propExpression.Body as MemberExpression;
            if (body == null) { return default; }
            var property = body.Member;
            var attr = property.GetAttribute<TAttribute>();
            if (attr == null) { return default; }
            var value = valueFunc(attr);
            return value;
        }
    }
}
