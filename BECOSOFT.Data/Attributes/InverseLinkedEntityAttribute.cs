using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the property is linked to another table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InverseLinkedEntityAttribute : Attribute {
        /// <summary>
        /// The name of the foreign key property in this table
        /// </summary>
        public string ForeignKeyProperty { get; }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="foreignKeyProperty">The foreign key property name</param>
        public InverseLinkedEntityAttribute(string foreignKeyProperty) {
            ForeignKeyProperty = foreignKeyProperty;
        }
    }
}
