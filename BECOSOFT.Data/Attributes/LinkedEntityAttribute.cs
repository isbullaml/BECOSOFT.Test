using BECOSOFT.Data.Models.Base;
using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the property is linked to another table with a one-to-one relationship.
    /// This attribute should only be used on a <see cref="BaseEntity"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedEntityAttribute : Attribute {
        /// <summary>
        /// The name of the foreign key in the other table.
        /// </summary>
        public string ForeignKeyColumn { get; }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="foreignKeyColumn">The foreign key column in the joined table.</param>
        public LinkedEntityAttribute(string foreignKeyColumn) {
            ForeignKeyColumn = foreignKeyColumn;
        }
    }
}
