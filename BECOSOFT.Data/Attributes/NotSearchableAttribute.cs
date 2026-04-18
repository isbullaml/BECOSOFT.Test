using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Defines a field or property to not be used query generation in QueryExpression
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class NotSearchableAttribute : Attribute {
    }
}
