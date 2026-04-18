using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the property is an object with other properties.
    /// On database-level these properties are one table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedBaseChildAttribute : Attribute {
    }
}