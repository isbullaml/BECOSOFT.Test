using BECOSOFT.Data.Models.Base;
using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the property is linked to another table with a one-to-many relationship.
    /// This attribute should only be used on a <see cref="BaseEntity"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedEntitiesAttribute : Attribute {
        /// <summary>
        /// The name of the foreign key in the other table.
        /// </summary>
        public string ForeignKeyColumn { get; }

        public LinkedEntitiesAttribute(string foreignKeyColumn) {
            ForeignKeyColumn = foreignKeyColumn;
        }
    }
}
